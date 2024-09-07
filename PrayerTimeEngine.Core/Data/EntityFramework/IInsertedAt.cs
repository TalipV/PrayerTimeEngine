using NodaTime;

namespace PrayerTimeEngine.Core.Data.EntityFramework;

public interface IInsertedAt
{
    Instant? InsertInstant { get; set; }
}
