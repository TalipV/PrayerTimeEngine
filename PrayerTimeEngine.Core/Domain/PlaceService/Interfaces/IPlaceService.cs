using PrayerTimeEngine.Core.Domain.PlacesService.Models.Common;

namespace PrayerTimeEngine.Core.Domain.PlacesService.Interfaces
{
    public interface ILocationService
    {
        Task<List<BasicPlaceInfo>> SearchPlacesAsync(string searchTerm, string language);
        Task<BasicPlaceInfo> GetPlaceBasedOnPlace(BasicPlaceInfo place, string languageIdentif);
        Task<CompletePlaceInfo> GetTimezoneInfo(BasicPlaceInfo placeInfo);
    }
}
