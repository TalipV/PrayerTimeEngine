using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Domain.NominatimLocation.Models
{
    public class Address
    {
        public string city { get; set; }
        public string state { get; set; }

        [JsonProperty("ISO3166-2-lvl4")]
        public string ISO31662lvl4 { get; set; }
        public string country { get; set; }
        public string country_code { get; set; }
        public string city_district { get; set; }
        public string residential { get; set; }
        public string county { get; set; }
        public string municipality { get; set; }
        public string road { get; set; }
        public string neighbourhood { get; set; }
        public string suburb { get; set; }
        public string postcode { get; set; }
        public string hamlet { get; set; }
        public string town { get; set; }
        public string man_made { get; set; }
    }
}
