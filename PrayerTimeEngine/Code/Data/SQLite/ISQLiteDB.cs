using Microsoft.Data.Sqlite;

public interface ISQLiteDB
{
    public void InitializeDatabase();
    public void ExecuteCommand(Action<SqliteConnection> commandAction);
}