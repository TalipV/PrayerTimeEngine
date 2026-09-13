using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NodaTime;
using PrayerTimeEngine.Core.Common;
using System.Globalization;

namespace PrayerTimeEngine.Core.Data.EntityFramework;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite("Data Source=:memory:");

        return new AppDbContext(
            optionsBuilder.Options,
            new AppDbContextMetaData(),
            new DummySystemInfoService());
    }

    /// <summary>
    /// Only the EF Core tooling uses this factory and it never saves any entities,
    /// which is the only situation in which the <see cref="ISystemInfoService"/> is used.
    /// </summary>
    private class DummySystemInfoService : ISystemInfoService
    {
        public ZonedDateTime GetCurrentZonedDateTime() => throw new NotImplementedException();
        public Instant GetCurrentInstant() => throw new NotImplementedException();
        public DateTimeZone GetSystemTimeZone() => throw new NotImplementedException();
        public CultureInfo GetSystemCulture() => throw new NotImplementedException();
        public ZonedDateTime? GetInCurrentZone(ZonedDateTime? zonedDateTime) => throw new NotImplementedException();
        public ZonedDateTime GetInCurrentZone(ZonedDateTime zonedDateTime) => throw new NotImplementedException();
    }
}
