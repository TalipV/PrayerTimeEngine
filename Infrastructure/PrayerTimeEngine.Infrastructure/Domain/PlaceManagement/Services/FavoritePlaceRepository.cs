using Microsoft.EntityFrameworkCore;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;

namespace PrayerTimeEngine.Core.Domain.PlaceManagement.Services;

public class FavoritePlaceRepository(
        IDbContextFactory<AppDbContext> dbContextFactory
    ) : IFavoritePlaceRepository
{
    public async Task<List<FavoritePlace>> GetFavoritePlacesAsync(CancellationToken cancellationToken)
    {
        using AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.FavoritePlaces.AsNoTracking()
            .OrderBy(x => x.ID)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task AddFavoritePlaceAsync(BasicPlaceInfo place, CancellationToken cancellationToken)
    {
        using AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        // compared in memory: the list is small and SQLite stores the decimal coordinates as text
        List<FavoritePlace> favoritePlaces = await dbContext.FavoritePlaces.ToListAsync(cancellationToken).ConfigureAwait(false);
        if (favoritePlaces.Any(x => x.Equals(place)))
        {
            return;
        }

        dbContext.FavoritePlaces.Add(new FavoritePlace(place));
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task RemoveFavoritePlaceAsync(BasicPlaceInfo place, CancellationToken cancellationToken)
    {
        using AppDbContext dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        List<FavoritePlace> favoritePlaces = await dbContext.FavoritePlaces.ToListAsync(cancellationToken).ConfigureAwait(false);
        List<FavoritePlace> matchingFavoritePlaces = [.. favoritePlaces.Where(x => x.Equals(place))];
        if (matchingFavoritePlaces.Count == 0)
        {
            return;
        }

        dbContext.FavoritePlaces.RemoveRange(matchingFavoritePlaces);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}
