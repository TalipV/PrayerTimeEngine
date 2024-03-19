using PrayerTimeEngine.Core.Domain.PlaceManagement.Models.DTOs;
using Refit;

namespace PrayerTimeEngine.Core.Domain.PlaceManagement.Services
{
    public interface ILocationIQApiService
    {
        private const string ACCESS_TOKEN = "pk.c515e4d0e2c522f5b06068b45574bb68";
        private const string MAX_RESULTS = "10";

        [Get($$"""/timezone?format=json&key={{ACCESS_TOKEN}}&lat={latitude}&lon={longitude}""")]
        Task<LocationIQTimezoneResponseDTO> GetTimezoneAsync(
            decimal latitude, 
            decimal longitude, 
            CancellationToken cancellationToken);

        // &countrycodes is also available to restrict results to countries
        [Get($$"""/autocomplete?format=json&key={{ACCESS_TOKEN}}&addressdetails=1&limit={{MAX_RESULTS}}&accept-language={language}&q={searchTerm}""")]
        Task<List<LocationIQPlace>> GetPlacesAsync(
            string language, 
            string searchTerm, 
            CancellationToken cancellationToken);

        [Get($$"""/reverse?format=json&key={{ACCESS_TOKEN}}&accept-language={language}&lat={latitude}&lon={longitude}&osm_id={osmID}""")]
        Task<LocationIQPlace> GetSpecificPlaceAsync(
            string language,
            decimal latitude,
            decimal longitude,
            string osmID,
            CancellationToken cancellationToken);
    }
}
