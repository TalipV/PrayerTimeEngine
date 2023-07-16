using Microsoft.Data.Sqlite;

public class SQLiteDB : ISQLiteDB
{
    private static readonly string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
    private const string DatabaseName = "FaziletPrayerTimesDB.db";
    private static readonly string DatabasePath = Path.Combine(folderPath, DatabaseName);

    private SqliteConnection getSqliteConnection()
    {
        return new SqliteConnection($"Data Source={DatabasePath}");
    }

    public void ExecuteCommand(Action<SqliteConnection> commandAction)
    {
        using (SqliteConnection connection = getSqliteConnection())
        {
            connection.Open();
            commandAction(connection);
            connection.Close();
        }
    }

    public void InitializeDatabase()
    {
#if DEBUG
        if (File.Exists(DatabasePath))
            File.Delete(DatabasePath);
#endif

        if (!File.Exists(DatabasePath))
        {
            this.ExecuteCommand(connection =>
            {
                connection.Open();
                createFaziletTablesIfNotExists(connection);
                createMuwaqqitTablesIfNotExists(connection);
                connection.Close();
            });
        }
    }

    private void createFaziletTablesIfNotExists(SqliteConnection connection)
    {
        string tableFaziletCountries = "CREATE TABLE IF NOT EXISTS " +
                                "FaziletCountries (Id INTEGER PRIMARY KEY, " +
                                "Name NVARCHAR(2048) NULL)";
        createTable(connection, tableFaziletCountries);

        string tableFaziletCities = "CREATE TABLE IF NOT EXISTS " +
                             "FaziletCities (Id INTEGER PRIMARY KEY, " +
                             "Name NVARCHAR(2048) NULL, " +
                             "CountryId INTEGER, " +
                             "FOREIGN KEY(CountryId) REFERENCES FaziletCountries(Id))";
        createTable(connection, tableFaziletCities);

        string tableFaziletPrayerTimes = "CREATE TABLE IF NOT EXISTS " +
                                  "FaziletPrayerTimes (Id INTEGER PRIMARY KEY AUTOINCREMENT, " +

                                  "Date DATETIME NOT NULL, " +
                                  "CityId INTEGER, " +

                                  "Imsak DATETIME NOT NULL, " +
                                  "Fajr DATETIME NOT NULL, " +
                                  "Shuruq DATETIME NOT NULL, " +
                                  "Dhuhr DATETIME NOT NULL, " +
                                  "Asr DATETIME NOT NULL, " +
                                  "Maghrib DATETIME NOT NULL, " +
                                  "Isha DATETIME NOT NULL, " +

                                  "FOREIGN KEY(CityId) REFERENCES FaziletCities(Id)," +
                                  "UNIQUE(Date,CityId))";
        createTable(connection, tableFaziletPrayerTimes);
    }
    
    private void createMuwaqqitTablesIfNotExists(SqliteConnection connection)
    {
        string tableMuwaqqitPrayerTimes = "CREATE TABLE IF NOT EXISTS " +
                                  "MuwaqqitPrayerTimes (Id INTEGER PRIMARY KEY AUTOINCREMENT, " +

                                  "Date DATETIME NOT NULL, " +
                                  "Longitude REAL, " +
                                  "Latitude REAL, " +

                                  "Fajr DATETIME NOT NULL, " +
                                  "Shuruq DATETIME NOT NULL, " +
                                  "Dhuhr DATETIME NOT NULL, " +
                                  "AsrMithl DATETIME NOT NULL, " +
                                  "AsrMithlayn DATETIME NOT NULL, " +
                                  "Maghrib DATETIME NOT NULL, " +
                                  "Isha DATETIME NOT NULL, " +

                                  "Fajr_Degree DATETIME NOT NULL, " +
                                  "Isha_Degree DATETIME NOT NULL)";
        createTable(connection, tableMuwaqqitPrayerTimes);
    }

    private void createTable(SqliteConnection db, string createTableCommand)
    {
        SqliteCommand createTable = new SqliteCommand(createTableCommand, db);
        createTable.ExecuteReader();
    }
}
