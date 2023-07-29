namespace PrayerTimeEngine;

public partial class App : Application
{
	public App(MainPage page, ISQLiteDB sqliteDB)
	{
		InitializeComponent();

		MainPage = new NavigationPage(page);

        // Initialize the database
        sqliteDB?.InitializeDatabase();
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = base.CreateWindow(activationState);

        const int newWidth = 440;
        const int newHeight = 600;

        window.Width = newWidth;
        window.Height = newHeight;

        return window;
    }
}
