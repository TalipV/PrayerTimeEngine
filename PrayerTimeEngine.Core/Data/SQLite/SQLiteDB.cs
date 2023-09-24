    using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace PrayerTimeEngine.Core.Data.SQLite
{
    public class SQLiteDB : ISQLiteDB
    {
        public SQLiteDB(ILogger<SQLiteDB> logger)
        {
            _logger = logger;
        }

        private readonly ILogger<SQLiteDB> _logger;
        private static readonly string _folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        private const string _databaseName = "PrayerTimeEngineDB.db";
        private static readonly string _databasePath = Path.Combine(_folderPath, _databaseName);

        public SqliteConnection GetSqliteConnection(string connectionString = null)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                connectionString = $"Data Source={_databasePath}";

            return new SqliteConnection(connectionString);
        }

        public async Task ExecuteCommandAsync(Func<SqliteConnection, Task> commandAction)
        {
            using (SqliteConnection connection = GetSqliteConnection())
            {
                await connection.OpenAsync().ConfigureAwait(false);
                await commandAction(connection).ConfigureAwait(false);
            }
        }

        public void InitializeDatabase(bool filePathDatabase = true)
        {
            _logger.LogDebug("Initialize");

#if DEBUG
            //if (filePathDatabase && File.Exists(_databasePath))
            //    File.Delete(_databasePath);
            //_logger.LogDebug("Delete database");
#endif

            if (!filePathDatabase || !File.Exists(_databasePath))
            {
                _logger.LogDebug("Create table schemas");

                using (SqliteConnection connection = GetSqliteConnection())
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
            string tableProfil = """
                CREATE TABLE IF NOT EXISTS
                    Profile (
                        Id INTEGER PRIMARY KEY,
                        Name TEXT NOT NULL,
                        LocationName TEXT NOT NULL,
                        SequenceNo INTEGER NOT NULL,
                        InsertInstant TEXT NOT NULL,
                        UNIQUE(SequenceNo),
                        UNIQUE(Name))
                """;
            createTable(connection, tableProfil);

            string tableTimeSpecificConfig = """
                CREATE TABLE IF NOT EXISTS
                    TimeSpecificConfig (
                        Id INTEGER PRIMARY KEY,
                        ProfileID INTEGER NOT NULL,
                        TimeType INTEGER NOT NULL,
                        JsonConfigurationString TEXT NOT NULL,

                        InsertInstant TEXT NOT NULL,
                        UNIQUE(ProfileID, TimeType),
                        FOREIGN KEY(ProfileID) REFERENCES Profile(Id))
                """;
            createTable(connection, tableTimeSpecificConfig);

            string tableLocationData = """
                CREATE TABLE IF NOT EXISTS
                    LocationData (
                        Id INTEGER PRIMARY KEY,
                        ProfileID INTEGER NOT NULL,
                        CalculationSource INTEGER NOT NULL,
                        JsonLocationData TEXT NOT NULL,

                        InsertInstant TEXT NOT NULL,

                        UNIQUE(ProfileID, CalculationSource),
                        FOREIGN KEY(ProfileID) REFERENCES Profile(Id))
                """;
            createTable(connection, tableLocationData);
        }

        private void createFaziletTablesIfNotExists(SqliteConnection connection)
        {
            string tableFaziletCountries = """
                CREATE TABLE IF NOT EXISTS 
                    FaziletCountries (
                        Id INTEGER PRIMARY KEY, 
                        Name TEXT NOT NULL,
                        InsertInstant TEXT NOT NULL,
                        UNIQUE(Name))
                """;
            createTable(connection, tableFaziletCountries);

            string tableFaziletCities = """
                CREATE TABLE IF NOT EXISTS 
                    FaziletCities (
                        Id INTEGER PRIMARY KEY, 
                        CountryId INTEGER NOT NULL, 
                        Name TEXT NOT NULL, 
                        InsertInstant TEXT NOT NULL,
                        FOREIGN KEY(CountryId) REFERENCES FaziletCountries(Id),
                        UNIQUE(CountryId, Name))
                """;
            createTable(connection, tableFaziletCities);

            string tableFaziletPrayerTimes = """
                CREATE TABLE IF NOT EXISTS 
                    FaziletPrayerTimes (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT, 

                        Date TEXT NOT NULL, 
                        CityId INTEGER NOT NULL, 

                        Imsak TEXT NOT NULL, 
                        Fajr TEXT NOT NULL, 
                        Shuruq TEXT NOT NULL, 
                        Dhuhr TEXT NOT NULL, 
                        Asr TEXT NOT NULL, 
                        Maghrib TEXT NOT NULL, 
                        Isha TEXT NOT NULL, 
                        InsertInstant TEXT NOT NULL,

                        FOREIGN KEY(CityId) REFERENCES FaziletCities(Id),
                        UNIQUE(Date, CityId))
                """;
            createTable(connection, tableFaziletPrayerTimes);
        }

        private void createSemerkandTablesIfNotExists(SqliteConnection connection)
        {
            string tableSemerkandCountries = """
                CREATE TABLE IF NOT EXISTS 
                    SemerkandCountries (
                        Id INTEGER PRIMARY KEY, 
                        Name TEXT NOT NULL,
                        InsertInstant TEXT NOT NULL,
                        UNIQUE(Name))
                """;
            createTable(connection, tableSemerkandCountries);

            string tableSemerkandCities = """
                CREATE TABLE IF NOT EXISTS 
                    SemerkandCities (
                        Id INTEGER PRIMARY KEY, 
                        CountryId INTEGER NOT NULL, 
                        Name TEXT NOT NULL, 
                        InsertInstant TEXT NOT NULL,
                        FOREIGN KEY(CountryId) REFERENCES SemerkandCountries(Id),
                        UNIQUE(CountryId, Name))
                """;
            createTable(connection, tableSemerkandCities);

            string tableSemerkandPrayerTimes = """
                CREATE TABLE IF NOT EXISTS 
                    SemerkandPrayerTimes (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT, 

                        Date TEXT NOT NULL, 
                        CityId INTEGER NOT NULL, 

                        Fajr TEXT NOT NULL, 
                        Shuruq TEXT NOT NULL, 
                        Dhuhr TEXT NOT NULL, 
                        Asr TEXT NOT NULL, 
                        Maghrib TEXT NOT NULL, 
                        Isha TEXT NOT NULL, 
                        InsertInstant TEXT NOT NULL,

                        FOREIGN KEY(CityId) REFERENCES SemerkandCities(Id),
                        UNIQUE(Date, CityId))
                """;
            createTable(connection, tableSemerkandPrayerTimes);
        }

        private void createMuwaqqitTablesIfNotExists(SqliteConnection connection)
        {
            string tableMuwaqqitPrayerTimes = """
                CREATE TABLE IF NOT EXISTS 
                    MuwaqqitPrayerTimes (

                    Id INTEGER PRIMARY KEY AUTOINCREMENT, 

                    Date TEXT NOT NULL, 
                    Longitude REAL NOT NULL, 
                    Latitude REAL NOT NULL, 
                    Timezone TEXT NOT NULL, 

                    Fajr TEXT NOT NULL, 
                    NextFajr TEXT NOT NULL, 
                    Fajr_Degree REAL NOT NULL, 

                    Shuruq TEXT NOT NULL, 
                    Duha TEXT NOT NULL, 
                    Dhuhr TEXT NOT NULL, 

                    AsrMithl TEXT NOT NULL, 
                    AsrMithlayn TEXT NOT NULL, 
                    AsrKaraha TEXT NOT NULL, 
                    AsrKaraha_Degree REAL NOT NULL, 

                    Maghrib TEXT NOT NULL, 
                    Ishtibaq TEXT NOT NULL, 
                    Ishtibaq_Degree REAL NOT NULL, 

                    Isha TEXT NOT NULL, 
                    Isha_Degree REAL NOT NULL,
                    InsertInstant TEXT NOT NULL,
                    UNIQUE(Date, Longitude, Latitude, Timezone, Fajr_Degree, AsrKaraha_Degree, Ishtibaq_Degree, Isha_Degree))
                """;

            createTable(connection, tableMuwaqqitPrayerTimes);
        }

        private void createTable(SqliteConnection db, string createTableCommand)
        {
            SqliteCommand createTable = new(createTableCommand, db);
            createTable.ExecuteReader();
        }
    }
}


