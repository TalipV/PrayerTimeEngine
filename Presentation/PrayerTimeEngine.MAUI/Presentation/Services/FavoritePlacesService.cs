using Microsoft.Extensions.Logging;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;

namespace PrayerTimeEngine.Presentation.Services;

/// <summary>
/// The user's favorite places, stored in the database table of <see cref="IFavoritePlaceRepository"/>.
/// Deduplication relies on <see cref="BasicPlaceInfo"/>'s value equality.
/// </summary>
public class FavoritePlacesService(
        IFavoritePlaceRepository favoritePlaceRepository,
        ILogger<FavoritePlacesService> logger
    )
{
    public async Task<List<FavoritePlace>> GetFavoritesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await favoritePlaceRepository.GetFavoritePlacesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            // the place search must keep working without favorites
            logger.LogError(exception, "Error while loading the favorite places");
            return [];
        }
    }

    public async Task<bool> IsFavoriteAsync(BasicPlaceInfo place, CancellationToken cancellationToken)
    {
        if (place is null)
        {
            return false;
        }

        List<FavoritePlace> favorites = await GetFavoritesAsync(cancellationToken);
        return favorites.Any(x => x.Equals(place));
    }

    public async Task AddAsync(BasicPlaceInfo place, CancellationToken cancellationToken)
    {
        if (place is null)
        {
            return;
        }

        await favoritePlaceRepository.AddFavoritePlaceAsync(place, cancellationToken);
    }

    public async Task RemoveAsync(BasicPlaceInfo place, CancellationToken cancellationToken)
    {
        if (place is null)
        {
            return;
        }

        await favoritePlaceRepository.RemoveFavoritePlaceAsync(place, cancellationToken);
    }
}
