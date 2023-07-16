namespace PrayerTimeEngine;

public partial class App : Application
{
	public App(MainPage page, ISQLiteDB sqliteDB)
	{
		InitializeComponent();

		MainPage = page;

        // Initialize the database
        sqliteDB?.InitializeDatabase();
    }
}
