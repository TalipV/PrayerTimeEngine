using PrayerTimeEngine.Domain.LocationService.Models;

namespace PrayerTimeEngine.Domain.NominatimLocation.Interfaces
{
    public interface ILocationService
    {
        Task<List<LocationIQPlace>> SearchPlacesAsync(string searchTerm, string language);
        Task<LocationIQPlace> GetPlaceByID(LocationIQPlace place, string languageIdentif);
    }
}
