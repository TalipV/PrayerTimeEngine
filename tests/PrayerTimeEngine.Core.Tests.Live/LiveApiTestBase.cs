using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Infrastructure;
using PrayerTimeEngine.Core.Tests.Common;

namespace PrayerTimeEngine.Core.Tests.Live;

/// <summary>
/// Wires up the real Infrastructure adapters (incl. the HttpClient resilience pipeline the app uses),
/// so the live tests call the external APIs exactly like the app does. The database is never touched.
/// </summary>
public abstract class LiveApiTestBase : BaseTest
{
    /// <summary>
    /// The app's public LocationIQ key (see <c>AppApiKeys.cs</c> in the MAUI project), overridable
    /// through the <c>LOCATIONIQ_API_KEY</c> environment variable.
    /// </summary>
    private const string DefaultLocationIQApiKey = "pk.5fdf59f9c3682abc89749e42d1f68d8d";

    protected ServiceProvider ServiceProvider { get; } = CreateServiceProvider(serviceCollection =>
    {
        serviceCollection.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        serviceCollection.AddSingleton(Substitute.For<ISystemInfoService>());
        serviceCollection.AddInfrastructure(
            databasePath: ":memory:",
            locationIQApiKey: Environment.GetEnvironmentVariable("LOCATIONIQ_API_KEY") ?? DefaultLocationIQApiKey);
    });
}
