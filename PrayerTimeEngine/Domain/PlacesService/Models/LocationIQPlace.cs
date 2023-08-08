using System.Net;
using PrayerTimeEngine.Domain.NominatimLocation.Models;

namespace PrayerTimeEngine.Domain.LocationService.Models
{
    public class LocationIQPlace
    {
        public string place_id { get; set; }
        public string licence { get; set; }
        public string osm_type { get; set; }
        public string osm_id { get; set; }
        public List<string> boundingbox { get; set; }
        public string lat { get; set; }
        public string lon { get; set; }
        public string display_name { get; set; }
        public string @class { get; set; }
        public string type { get; set; }
        public double importance { get; set; }
        public string icon { get; set; }
        public LocationIQAddress address { get; set; }

        public override string ToString()
        {
            return this.display_name;
        }
    }
}
