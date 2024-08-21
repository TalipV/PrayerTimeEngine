using PrayerTimeEngine.Core.Domain.PlaceManagement.Models;

namespace PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces
{
    public interface IPlaceService
    {
        Task<List<BasicPlaceInfo>> SearchPlacesAsync(string searchTerm, string language, CancellationToken cancellationToken);
        Task<BasicPlaceInfo> GetPlaceBasedOnPlace(BasicPlaceInfo place, string languageIdentif, CancellationToken cancellationToken);
        Task<ProfilePlaceInfo> GetTimezoneInfo(BasicPlaceInfo placeInfo, CancellationToken cancellationToken);
    }
}
