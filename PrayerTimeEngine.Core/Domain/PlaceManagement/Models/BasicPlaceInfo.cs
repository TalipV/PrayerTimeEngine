using System.Text;

namespace PrayerTimeEngine.Core.Domain.PlaceManagement.Models;

public class BasicPlaceInfo
{
    public string ExternalID { get; set; }

    public required decimal Longitude { get; set; }
    public required decimal Latitude { get; set; }
    public required string InfoLanguageCode { get; set; }

    public required string Country { get; set; }
    public required string City { get; set; }
    public required string CityDistrict { get; set; }
    public required string PostCode { get; set; }
    public required string Street { get; set; }

    public string DisplayText
    {
        get
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.Append(Country);
            stringBuilder.Append(", ");
            stringBuilder.Append(City);

            if (!string.IsNullOrWhiteSpace(CityDistrict))
            {
                stringBuilder.Append(", ");
                stringBuilder.Append(CityDistrict);
            }

            if (!string.IsNullOrWhiteSpace(Street))
            {
                stringBuilder.Append(", ");
                stringBuilder.Append(Street);
            }

            return stringBuilder.ToString();
        }
    }

    public override bool Equals(object obj)
    {
        if (obj is not BasicPlaceInfo other)
        {
            return false;
        }

        return ExternalID == other.ExternalID &&
            Longitude == other.Longitude &&
            Latitude == other.Latitude &&
            InfoLanguageCode == other.InfoLanguageCode &&
            Country == other.Country &&
            City == other.City &&
            CityDistrict == other.CityDistrict &&
            PostCode == other.PostCode &&
            Street == other.Street;
    }

    public override int GetHashCode()
    {
        // split up because the method doesn't accept more than 8 parameters
        return HashCode.Combine(
            HashCode.Combine(ExternalID, Longitude, Latitude, InfoLanguageCode, Country, City, CityDistrict),
            PostCode, Street);
    }
}
