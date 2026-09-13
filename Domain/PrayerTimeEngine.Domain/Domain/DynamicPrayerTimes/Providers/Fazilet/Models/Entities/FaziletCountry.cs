using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Models.Entities;

public class FaziletCountry : IEntity
{
    public int ID { get; set; }
    public Instant? InsertInstant { get; set; }
    public string Name { get; set; }

    public ICollection<FaziletCity> Cities { get; set; }
}
