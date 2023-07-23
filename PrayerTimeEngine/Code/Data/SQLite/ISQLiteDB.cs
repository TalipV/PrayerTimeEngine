using Microsoft.Data.Sqlite;

public interface ISQLiteDB
{
    public void InitializeDatabase();
    public Task ExecuteCommandAsync(Func<SqliteConnection, Task> commandAction);
}