using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models.Entities;

public class FaziletCity : IEntity
{
    public int ID { get; set; }
    public Instant? InsertInstant { get; set; }
    public string Name { get; set; }

    public int CountryID { get; set; }
    public FaziletCountry Country { get; set; }
}
