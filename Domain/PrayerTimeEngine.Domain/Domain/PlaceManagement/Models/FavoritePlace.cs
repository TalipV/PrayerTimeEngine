using System.Diagnostics.CodeAnalysis;
using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;

namespace PrayerTimeEngine.Core.Domain.PlaceManagement.Models;

/// <summary>
/// A place the user saved as a favorite for the place search.
/// Equality is inherited from <see cref="BasicPlaceInfo"/>, so it compares the place data only (not the ID).
/// </summary>
public class FavoritePlace : BasicPlaceInfo, IEntity
{
    public int ID { get; set; }
    public Instant? InsertInstant { get; set; }

    public FavoritePlace()
    {
    }

    [SetsRequiredMembers]
    public FavoritePlace(BasicPlaceInfo place)
    {
        ExternalID = place.ExternalID;
        Longitude = place.Longitude;
        Latitude = place.Latitude;
        InfoLanguageCode = place.InfoLanguageCode;
        Country = place.Country;
        State = place.State;
        City = place.City;
        CityDistrict = place.CityDistrict;
        PostCode = place.PostCode;
        Street = place.Street;
    }
}
