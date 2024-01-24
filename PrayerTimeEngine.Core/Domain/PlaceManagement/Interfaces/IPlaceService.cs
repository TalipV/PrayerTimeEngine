using PrayerTimeEngine.Core.Domain.PlaceManagement.Models.Common;

namespace PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces
{
    public interface IPlaceService
    {
        Task<List<BasicPlaceInfo>> SearchPlacesAsync(string searchTerm, string language);
        Task<BasicPlaceInfo> GetPlaceBasedOnPlace(BasicPlaceInfo place, string languageIdentif);
        Task<CompletePlaceInfo> GetTimezoneInfo(BasicPlaceInfo placeInfo);
    }
}
