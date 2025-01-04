using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PrayerTimeEngine.Core.Common;

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
            NSubstitute.Substitute.For<ISystemInfoService>());
    }
}
