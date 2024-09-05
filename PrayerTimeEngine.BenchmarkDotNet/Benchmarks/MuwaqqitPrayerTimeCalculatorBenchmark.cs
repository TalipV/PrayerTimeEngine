﻿using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NSubstitute;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Interfaces;
using PrayerTimeEngine.Core.Domain;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Services;
using PrayerTimeEngine.Core.Domain.Models;
using System.Data.Common;
using NSubstitute.Extensions;
using PrayerTimeEngine.Core.Tests.Common.TestData;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models.Entities;
using PrayerTimeEngine.Core.Common;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace PrayerTimeEngine.BenchmarkDotNet.Benchmarks
{
    [Config(typeof(BenchmarkConfig))]
    [MemoryDiagnoser]
    public class MuwaqqitPrayerTimeCalculatorBenchmark
    {
        #region data

        private static readonly ZonedDateTime _zonedDateTime = new LocalDate(2023, 7, 29).AtStartOfDayInZone(TestDataHelper.EUROPE_VIENNA_TIME_ZONE);

        private static readonly List<GenericSettingConfiguration> _configs =
            [
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
            ];

        private static readonly MuwaqqitLocationData _locationData =
            new()
            {
                Latitude = 47.2803835M,
                Longitude = 11.41337M,
                TimezoneName = TestDataHelper.EUROPE_VIENNA_TIME_ZONE.Id
            };

        #endregion data

        private static MuwaqqitPrayerTimeCalculator getMuwaqqitPrayerTimeCalculator_DataFromDbStorage(
            IDbContextFactory<AppDbContext> dbContextFactory)
        {
            // to make sure that before the benchmark the data is gotten from the APIService and stored in the db
            new MuwaqqitPrayerTimeCalculator(
                    new MuwaqqitDBAccess(dbContextFactory),
                    SubstitutionHelper.GetMockedMuwaqqitApiService(),
                    new TimeTypeAttributeService()
                ).GetPrayerTimesAsync(_zonedDateTime, _locationData, _configs, default).GetAwaiter().GetResult();

            // throw exceptions when the calculator tries using the api
            IMuwaqqitApiService mockedMuwaqqitApiService = Substitute.For<IMuwaqqitApiService>();
            mockedMuwaqqitApiService.ReturnsForAll<Task<MuwaqqitPrayerTimes>>((callInfo) => throw new Exception("Don't use this!"));

            return new MuwaqqitPrayerTimeCalculator(
                    new MuwaqqitDBAccess(dbContextFactory),
                    mockedMuwaqqitApiService,
                    new TimeTypeAttributeService()
                );
        }

        private static MuwaqqitPrayerTimeCalculator getMuwaqqitPrayerTimeCalculator_DataFromApi()
        {
            return new MuwaqqitPrayerTimeCalculator(
                    // returns null per default
                    Substitute.For<IMuwaqqitDBAccess>(),
                    SubstitutionHelper.GetMockedMuwaqqitApiService(),
                    new TimeTypeAttributeService()
                );
        }

        private static DbConnection _dbContextKeepAliveSqlConnection;

        [GlobalSetup]
        public static void Setup()
        {

            var dbOptions = new DbContextOptionsBuilder()
                .UseSqlite($"Data Source=:memory:")
                .Options;

            var mockableDbContext =
                Substitute.ForPartsOf<AppDbContext>(
                    dbOptions,
                    new AppDbContextMetaData(),
                    Substitute.For<ISystemInfoService>());

            var mockableDbContextDatabase = Substitute.ForPartsOf<DatabaseFacade>(mockableDbContext);
            mockableDbContext.Configure().Database.Returns(mockableDbContextDatabase);
            _dbContextKeepAliveSqlConnection = mockableDbContext.Database.GetDbConnection();
            _dbContextKeepAliveSqlConnection.Open();
            mockableDbContext.Database.EnsureCreated();

            var dbContextFactoryMock = Substitute.For<IDbContextFactory<AppDbContext>>();
            dbContextFactoryMock.CreateDbContext().Returns(mockableDbContext);
            dbContextFactoryMock.CreateDbContextAsync().Returns(callInfo => Task.FromResult(mockableDbContext));

            _muwaqqitPrayerTimeCalculator_DataFromDbStorage = getMuwaqqitPrayerTimeCalculator_DataFromDbStorage(dbContextFactoryMock);
            _muwaqqitPrayerTimeCalculator_DataFromApi = getMuwaqqitPrayerTimeCalculator_DataFromApi();
        }

        private static MuwaqqitPrayerTimeCalculator _muwaqqitPrayerTimeCalculator_DataFromDbStorage = null;
        private static MuwaqqitPrayerTimeCalculator _muwaqqitPrayerTimeCalculator_DataFromApi = null;

#pragma warning disable CA1822 // Mark members as static
        [Benchmark]
        public List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> MuwaqqitPrayerTimeCalculator_GetDataFromDb()
        {
            var result = _muwaqqitPrayerTimeCalculator_DataFromDbStorage.GetPrayerTimesAsync(
                _zonedDateTime,
                locationData: _locationData,
                configurations: _configs, 
                cancellationToken: default).GetAwaiter().GetResult();

            if (result.Count != 16)
            {
                throw new Exception("No, no, no. Your benchmark is not working.");
            }

            return result;
        }

        [Benchmark]
        public List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> MuwaqqitPrayerTimeCalculator_GetDataFromApi()
        {
            var result = _muwaqqitPrayerTimeCalculator_DataFromApi.GetPrayerTimesAsync(
                _zonedDateTime,
                locationData: _locationData,
                configurations: _configs, 
                cancellationToken: default).GetAwaiter().GetResult();

            if (result.Count != 16)
            {
                throw new Exception("No, no, no. Your benchmark is not working.");
            }

            return result;
        }
#pragma warning restore CA1822 // Mark members as static
    }

}
