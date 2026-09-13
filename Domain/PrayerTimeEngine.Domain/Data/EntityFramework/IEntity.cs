using NodaTime;

namespace PrayerTimeEngine.Core.Data.EntityFramework;

public interface IEntity
{
    int ID { get; }
    Instant? InsertInstant { get; set; }
}