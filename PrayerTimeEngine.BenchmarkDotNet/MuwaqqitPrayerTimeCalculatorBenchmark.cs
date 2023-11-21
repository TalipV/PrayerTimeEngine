using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Services;
using PrayerTimeEngine.Core.Tests.Integration.MuwaqqitAPI;
using System.Data.Common;

namespace PrayerTimeEngine.BenchmarkDotNet
{
    [Config(typeof(BenchmarkConfig))]
    [MemoryDiagnoser]
    public class MuwaqqitPrayerTimeCalculatorBenchmark
    {
        private static AppDbContext _appDbContext = null;
        private static MuwaqqitPrayerTimeCalculator _muwaqqitPrayerTimeCalculator = null;
        private static List<GenericSettingConfiguration> _configs =
            new()
            {
                    new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.FajrStart, Degree = -12.0 },
                    new GenericSettingConfiguration { TimeType = ETimeType.FajrEnd, Source = ECalculationSource.Muwaqqit },
                    new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.FajrGhalas, Degree = -7.5 },
                    new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.FajrKaraha, Degree = -4.5 },
                    new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.DuhaStart, Degree = 3.5 },
                    new GenericSettingConfiguration { TimeType = ETimeType.DhuhrStart, Source = ECalculationSource.Muwaqqit },
                    new GenericSettingConfiguration { TimeType = ETimeType.DhuhrEnd, Source = ECalculationSource.Muwaqqit },
                    new GenericSettingConfiguration { TimeType = ETimeType.AsrStart, Source = ECalculationSource.Muwaqqit },
                    new GenericSettingConfiguration { TimeType = ETimeType.AsrEnd, Source = ECalculationSource.Muwaqqit },
                    new GenericSettingConfiguration { TimeType = ETimeType.AsrMithlayn, Source = ECalculationSource.Muwaqqit },
                    new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.AsrKaraha, Degree = 4.5 },
                    new GenericSettingConfiguration { TimeType = ETimeType.MaghribStart, Source = ECalculationSource.Muwaqqit },
                    new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.MaghribEnd, Degree = -12.0 },
                    new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.MaghribIshtibaq, Degree = -8 },
                    new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.IshaStart, Degree = -15.5 },
                    new MuwaqqitDegreeCalculationConfiguration { TimeType = ETimeType.IshaEnd, Degree = -15.0 },
            };
        private static MuwaqqitLocationData _locationData =
            new MuwaqqitLocationData
            {
                Latitude = 47.2803835M,
                Longitude = 11.41337M,
                TimezoneName = "Europe/Vienna"
            };

        [GlobalSetup]
        public void Setup()
        {
            var stuff = new MuwaqqitPrayerTimeCalculatorTests();
            stuff.SetUp();
            _appDbContext = stuff.ServiceProvider.GetService<AppDbContext>();
            _muwaqqitPrayerTimeCalculator = stuff.ServiceProvider.GetService<MuwaqqitPrayerTimeCalculator>();
        }

        private static DbConnection _sqlConnection;

        [IterationSetup]
        public void IterationSetup()
        {
            _sqlConnection = _appDbContext.Database.GetDbConnection();
            _appDbContext.Database.EnsureCreated();
        }

        [IterationCleanup]
        public void IterationCleanup()
        {
            _sqlConnection?.Close();
            _sqlConnection?.Dispose();
        }

        private static LocalDate localDate = new LocalDate(2023, 7, 30);

        [Benchmark]
        public void Test1()
        {
            for(int i = 0; i < 10; i++)
            {
                var result = _muwaqqitPrayerTimeCalculator.GetPrayerTimesAsync(
                    date: localDate,
                    locationData: _locationData,
                    configurations: _configs);
            }
        }
    }

}
