using Microsoft.EntityFrameworkCore;
using PrayerTimeEngine.Core.Data.EntityFramework;

namespace PrayerTimeEngine.Services;

internal class AppInitializer(
        IDbContextFactory<AppDbContext> dbContextFactory
    ) : IAppInitializer
{
    public bool IsInitialized { get; private set; }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (IsInitialized)
            return;

        using AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        await dbContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);

        IsInitialized = true;
    }
}
