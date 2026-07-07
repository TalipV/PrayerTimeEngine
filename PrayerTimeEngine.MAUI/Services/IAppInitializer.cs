namespace PrayerTimeEngine.Services;

public interface IAppInitializer
{
    bool IsInitialized { get; }
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
