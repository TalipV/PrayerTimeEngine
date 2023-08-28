using PrayerTimeEngine.Core.Domain.PlacesService.Models;

namespace PrayerTimeEngine.Core.Domain.PlacesService.Interfaces
{
    public interface ILocationService
    {
        Task<List<LocationIQPlace>> SearchPlacesAsync(string searchTerm, string language);
        Task<LocationIQPlace> GetPlaceByID(LocationIQPlace place, string languageIdentif);
    }
}
