using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;

namespace PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;

public interface IFavoritePlaceRepository
{
    /// <summary>Returns all favorites in the order they were added.</summary>
    Task<List<FavoritePlace>> GetFavoritePlacesAsync(CancellationToken cancellationToken);

    /// <summary>Adds the place as a favorite unless an equal favorite already exists.</summary>
    Task AddFavoritePlaceAsync(BasicPlaceInfo place, CancellationToken cancellationToken);

    /// <summary>Removes every favorite that equals the given place.</summary>
    Task RemoveFavoritePlaceAsync(BasicPlaceInfo place, CancellationToken cancellationToken);
}
