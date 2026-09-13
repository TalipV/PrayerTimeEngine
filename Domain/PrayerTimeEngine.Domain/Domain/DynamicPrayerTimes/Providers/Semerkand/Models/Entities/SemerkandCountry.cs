using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models.Entities;

public class SemerkandCountry : IEntity
{
    public int ID { get; set; }
    public Instant? InsertInstant { get; set; }
    public string Name { get; set; }

    public ICollection<SemerkandCity> Cities { get; set; }
}
