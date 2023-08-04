using PrayerTimeEngine.Domain.LocationService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Domain.NominatimLocation.Interfaces
{
    public interface IPlaceService
    {
        Task<List<Place>> SearchPlacesAsync(string searchTerm, string language);
    }
}
