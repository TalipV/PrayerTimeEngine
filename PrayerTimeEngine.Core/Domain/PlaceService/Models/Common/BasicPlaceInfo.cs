using System.Text;

namespace PrayerTimeEngine.Core.Domain.PlacesService.Models.Common
{
    public class BasicPlaceInfo
    {
        public BasicPlaceInfo(BasicPlaceInfo basicPlaceInfo)
            : this(
            basicPlaceInfo.ID,
            basicPlaceInfo.Longitude, basicPlaceInfo.Latitude,
            basicPlaceInfo.InfoLanguageCode,
            basicPlaceInfo.Country, basicPlaceInfo.City, basicPlaceInfo.CityDistrict,
            basicPlaceInfo.PostCode, basicPlaceInfo.Street)
        {
        }

        public BasicPlaceInfo(
            string id,
            decimal longitude, decimal latitude,
            string infoLanguageCode,
            string country, string city, string cityDistrict, 
            string postCode, string street)
        {
            ID = id;
            Longitude = longitude;
            Latitude = latitude;
            InfoLanguageCode = infoLanguageCode;
            Country = country;
            City = city;
            CityDistrict = cityDistrict;
            PostCode = postCode;
            Street = street;
        }

        public string ID { get; set; }

        public decimal Longitude { get; set; }
        public decimal Latitude { get; set; }
        public string InfoLanguageCode { get; set; }

        public string Country { get; set; }
        public string City { get; set; }
        public string CityDistrict { get; set; }
        public string PostCode { get; set; }
        public string Street { get; set; }

        public string DisplayText
        {
            get
            {
                StringBuilder stringBuilder = new StringBuilder();

                stringBuilder.Append(this.Country);
                stringBuilder.Append(", ");
                stringBuilder.Append(this.City);

                if (!string.IsNullOrWhiteSpace(this.CityDistrict))
                {
                    stringBuilder.Append(", ");
                    stringBuilder.Append(this.CityDistrict);
                }

                if (!string.IsNullOrWhiteSpace(this.Street))
                {
                    stringBuilder.Append(", ");
                    stringBuilder.Append(this.Street);
                }

                return stringBuilder.ToString();
            }
        }
    }
}
