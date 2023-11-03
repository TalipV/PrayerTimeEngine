using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using PrayerTimeEngine.Core.Data.EntityFramework;
using System.Data.Common;
using PrayerTimeEngine.Core.Domain.Configuration.Services;

namespace PrayerTimeEngine.BenchmarkDotNet
{
    [Config(typeof(BenchmarkConfig))]
    [MemoryDiagnoser]
    public class ProfileServiceBenchmark
    {
        private static AppDbContext _appDbContext;
        private static DbConnection _sqlConnection;

        private static ProfileService _profileService;
        private static ProfileDBAccess _profileDBAccess;

        [GlobalSetup]
        public void Setup()
        {
            var dbOptions = new DbContextOptionsBuilder()
                .UseSqlite($"Data Source=PrayerTimeEngineDB_ET_Benchmark.db", x => x.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                .Options;

            _appDbContext = new AppDbContext(dbOptions);
            _profileDBAccess = new ProfileDBAccess(_appDbContext);
            _profileService = new ProfileService(_appDbContext, _profileDBAccess);

            _sqlConnection = _appDbContext.Database.GetDbConnection();
            _appDbContext.Database.EnsureCreated();

            var profiles = _profileService.GetProfiles();
        }

        private const int iterations = 1000;

        [Benchmark]
        public void GetProfiles_Full()
        {
            for (int i = 0; i < iterations; i++)
            {
                var result = 
                    _appDbContext.Profiles
                        .Include(x => x.TimeConfigs)
                        .Include(x => x.LocationConfigs)
                        .AsNoTracking()
                        .ToList();
            }
        }

        [Benchmark]
        public void GetProfiles_Minimum()
        {
            for (int i = 0; i < iterations; i++)
            {
                var result =
                    _appDbContext.Profiles
                        .AsNoTracking()
                        .ToList();
            }
        }

        [Benchmark]
        public void GetProfiles_OnlyTime()
        {
            for (int i = 0; i < iterations; i++)
            {
                var result =
                    _appDbContext.Profiles
                        .Include(x => x.TimeConfigs)
                        .AsNoTracking()
                        .ToList();
            }
        }

        [Benchmark]
        public void GetProfiles_OnlyLocation()
        {
            for (int i = 0; i < iterations; i++)
            {
                var result =
                    _appDbContext.Profiles
                        .Include(x => x.LocationConfigs)
                        .AsNoTracking()
                        .ToList();
            }
        }
    }
}
