using Microsoft.Data.Sqlite;

public class SQLiteDB : ISQLiteDB
{
    private static readonly string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
    private const string DatabaseName = "PrayerTimeEngineDB.db";
    private static readonly string DatabasePath = Path.Combine(folderPath, DatabaseName);

    private SqliteConnection getSqliteConnection()
    {
        return new SqliteConnection($"Data Source={DatabasePath}");
    }

    public async Task ExecuteCommandAsync(Func<SqliteConnection, Task> commandAction)
    {
        using (SqliteConnection connection = getSqliteConnection())
        {
            await connection.OpenAsync();
            await commandAction(connection);
        }
    }

    public void InitializeDatabase()
    {
#if DEBUG
        //if (File.Exists(DatabasePath))
        //    File.Delete(DatabasePath);
#endif

        if (!File.Exists(DatabasePath))
        {
            using (SqliteConnection connection = getSqliteConnection())
            {
                connection.Open();
                createProfileTablesIfNotExists(connection);
                createFaziletTablesIfNotExists(connection);
                createSemerkandTablesIfNotExists(connection);
                createMuwaqqitTablesIfNotExists(connection);
            }
        }
    }

    private void createProfileTablesIfNotExists(SqliteConnection connection) 
    {
        string tableProfil =
            @"CREATE TABLE IF NOT EXISTS
                Profile (
                    Id INTEGER PRIMARY KEY,
                    Name NVARCHAR(2048) NOT NULL,
                    SequenceNo INTEGER NOT NULL,
                    InsertDateTime DATETIME NOT NULL,
                    UNIQUE(SequenceNo),
                    UNIQUE(Name))";
        createTable(connection, tableProfil);

        string tableTimeSpecificConfig =
            @"CREATE TABLE IF NOT EXISTS
                TimeSpecificConfig (
                    Id INTEGER PRIMARY KEY,
                    ProfileID INTEGER NOT NULL,
                    TimeType INTEGER NOT NULL,

                    ConfigurationTypeName NVARCHAR(100) NOT NULL,
                    JsonConfigurationString TEXT NOT NULL,

                    InsertDateTime DATETIME NOT NULL,

                    UNIQUE(ProfileID, TimeType),
                    FOREIGN KEY(ProfileID) REFERENCES Profile(Id))";
        createTable(connection, tableTimeSpecificConfig);

        string tableLocationData =
            @"CREATE TABLE IF NOT EXISTS
                LocationData (
                    Id INTEGER PRIMARY KEY,
                    ProfileID INTEGER NOT NULL,

                    LocationConfigTypeName NVARCHAR(100) NOT NULL,
                    JsonLocationInfo TEXT NOT NULL,

                    InsertDateTime DATETIME NOT NULL,

                    UNIQUE(ProfileID, LocationConfigTypeName),
                    FOREIGN KEY(ProfileID) REFERENCES Profile(Id))";
        createTable(connection, tableLocationData);
    }

    private void createFaziletTablesIfNotExists(SqliteConnection connection)
    {
        string tableFaziletCountries =
            @"CREATE TABLE IF NOT EXISTS 
                FaziletCountries (
                    Id INTEGER PRIMARY KEY, 
                    Name NVARCHAR(200) NOT NULL,
                    InsertDateTime DATETIME NOT NULL,
                    UNIQUE(Name))";
        createTable(connection, tableFaziletCountries);

        string tableFaziletCities =
            @"CREATE TABLE IF NOT EXISTS 
                FaziletCities (
                    Id INTEGER PRIMARY KEY, 
                    CountryId INTEGER NOT NULL, 
                    Name NVARCHAR(200) NOT NULL, 
                    InsertDateTime DATETIME NOT NULL,
                    FOREIGN KEY(CountryId) REFERENCES FaziletCountries(Id),
                    UNIQUE(CountryId, Name))";
        createTable(connection, tableFaziletCities);

        string tableFaziletPrayerTimes =
            @"CREATE TABLE IF NOT EXISTS 
                FaziletPrayerTimes (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT, 

                    Date DATETIME NOT NULL, 
                    CityId INTEGER NOT NULL, 

                    Imsak DATETIME NOT NULL, 
                    Fajr DATETIME NOT NULL, 
                    Shuruq DATETIME NOT NULL, 
                    Dhuhr DATETIME NOT NULL, 
                    Asr DATETIME NOT NULL, 
                    Maghrib DATETIME NOT NULL, 
                    Isha DATETIME NOT NULL, 
                    InsertDateTime DATETIME NOT NULL,

                    FOREIGN KEY(CityId) REFERENCES FaziletCities(Id),
                    UNIQUE(Date, CityId))";
        createTable(connection, tableFaziletPrayerTimes);
    }

    private void createSemerkandTablesIfNotExists(SqliteConnection connection)
    {
        string tableSemerkandCountries =
            @"CREATE TABLE IF NOT EXISTS 
                SemerkandCountries (
                    Id INTEGER PRIMARY KEY, 
                    Name NVARCHAR(200) NOT NULL,
                    InsertDateTime DATETIME NOT NULL,
                    UNIQUE(Name))";
        createTable(connection, tableSemerkandCountries);

        string tableSemerkandCities =
            @"CREATE TABLE IF NOT EXISTS 
                SemerkandCities (
                    Id INTEGER PRIMARY KEY, 
                    CountryId INTEGER NOT NULL, 
                    Name NVARCHAR(200) NOT NULL, 
                    InsertDateTime DATETIME NOT NULL,
                    FOREIGN KEY(CountryId) REFERENCES SemerkandCountries(Id),
                    UNIQUE(CountryId, Name))";
        createTable(connection, tableSemerkandCities);

        string tableSemerkandPrayerTimes =
            @"CREATE TABLE IF NOT EXISTS 
                SemerkandPrayerTimes (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT, 

                    Date DATETIME NOT NULL, 
                    CityId INTEGER NOT NULL, 

                    Fajr DATETIME NOT NULL, 
                    Shuruq DATETIME NOT NULL, 
                    Dhuhr DATETIME NOT NULL, 
                    Asr DATETIME NOT NULL, 
                    Maghrib DATETIME NOT NULL, 
                    Isha DATETIME NOT NULL, 
                    InsertDateTime DATETIME NOT NULL,

                    FOREIGN KEY(CityId) REFERENCES SemerkandCities(Id),
                    UNIQUE(Date, CityId))";
        createTable(connection, tableSemerkandPrayerTimes);
    }
    
    private void createMuwaqqitTablesIfNotExists(SqliteConnection connection)
    {
        string tableMuwaqqitPrayerTimes =
            @"CREATE TABLE IF NOT EXISTS 
                MuwaqqitPrayerTimes (

                Id INTEGER PRIMARY KEY AUTOINCREMENT, 

                Date DATETIME NOT NULL, 
                Longitude REAL NOT NULL, 
                Latitude REAL NOT NULL, 
                Timezone VARCHAR(100) NOT NULL, 

                Fajr DATETIME NOT NULL, 
                NextFajr DATETIME NOT NULL, 
                Fajr_Degree REAL NOT NULL, 

                Shuruq DATETIME NOT NULL, 
                Duha DATETIME NOT NULL, 
                Dhuhr DATETIME NOT NULL, 

                AsrMithl DATETIME NOT NULL, 
                AsrMithlayn DATETIME NOT NULL, 
                AsrKaraha DATETIME NOT NULL, 
                AsrKaraha_Degree REAL NOT NULL, 

                Maghrib DATETIME NOT NULL, 
                Ishtibaq DATETIME NOT NULL, 
                Ishtibaq_Degree REAL NOT NULL, 

                Isha DATETIME NOT NULL, 
                Isha_Degree REAL NOT NULL,
                InsertDateTime DATETIME NOT NULL,
                UNIQUE(Date, Longitude, Latitude, Timezone, Fajr_Degree, AsrKaraha_Degree, Ishtibaq_Degree, Isha_Degree))";

        createTable(connection, tableMuwaqqitPrayerTimes);
    }

    private void createTable(SqliteConnection db, string createTableCommand)
    {
        SqliteCommand createTable = new SqliteCommand(createTableCommand, db);
        createTable.ExecuteReader();
    }
}
