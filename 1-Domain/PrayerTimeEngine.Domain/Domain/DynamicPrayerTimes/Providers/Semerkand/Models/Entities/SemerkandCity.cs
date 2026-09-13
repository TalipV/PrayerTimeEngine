using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models.Entities;

public class SemerkandCity : IEntity
{
    public int ID { get; set; }
    public Instant? InsertInstant { get; set; }
    public string Name { get; set; }

    public int CountryID { get; set; }
    public SemerkandCountry Country { get; set; }
}
