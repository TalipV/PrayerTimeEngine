using Microsoft.Data.Sqlite;
using PrayerTimeEngine.Core.Data.SQLite;

namespace PrayerTimeEngineUnitTests.Mocks
{
    public class MockSQLiteDB : ISQLiteDB
    {
        public async Task ExecuteCommandAsync(Func<SqliteConnection, Task> commandAction)
        {
            // Creating a real connection to an in-memory database
            // and creating a command to simulate the behavior
            using var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            // easiest approach to mock
            var command = connection.CreateCommand();
            command.CommandText = "CREATE TABLE Dummy (DummyColumn TEXT);";
            await command.ExecuteNonQueryAsync(); // Create an empty table

            await commandAction(connection); // Pass the connection with the empty table
        }

        public void InitializeDatabase()
        {
            throw new NotImplementedException();
        }
    }
}
