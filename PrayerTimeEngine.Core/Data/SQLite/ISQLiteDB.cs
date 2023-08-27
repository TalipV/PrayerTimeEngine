using Microsoft.Data.Sqlite;

namespace PrayerTimeEngine.Data.SQLite
{
    public interface ISQLiteDB
    {
        public void InitializeDatabase();
        public Task ExecuteCommandAsync(Func<SqliteConnection, Task> commandAction);
    }
}