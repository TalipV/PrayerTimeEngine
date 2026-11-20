using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using NSubstitute;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Interfaces;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Models.DTOs;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Waqti.Services;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Interfaces;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;

namespace PrayerTimeEngine.Core.Tests.Integration.Domain.DynamicPrayerTimes.Providers.Waqti;

public class WaqtiDynamicPrayerTimeProviderTests : BaseTest
{
    private static readonly LocalDate s_date = new(2023, 7, 30);

    private static readonly WaqtiLocationData s_innsbruckLocationData = new()
    {
        Latitude = 47.2692124M,
        Longitude = 11.4041024M,
        TimezoneName = TestDataHelper.EUROPE_VIENNA_TIME_ZONE.Id
    };

    private ServiceProvider GetServiceProvider(IWaqtiApiService waqtiApiService)
    {
        return CreateServiceProvider(
            configureServiceCollection: serviceCollection =>
            {
                serviceCollection.AddSingleton(GetHandledDbContextFactory());
                serviceCollection.AddSingleton(Substitute.For<IPlaceService>());

                serviceCollection.AddSingleton<IWaqtiRepository, WaqtiRepository>();
                serviceCollection.AddSingleton(waqtiApiService);
                serviceCollection.AddSingleton<WaqtiDynamicPrayerTimeProvider>();
            });
    }

    private static ZonedDateTime ZonedDate(LocalDate date)
        => date.AtStartOfDayInZone(DateTimeZoneProviders.Tzdb[s_innsbruckLocationData.TimezoneName]);

    [Fact]
    public async Task GetPrayerTimesAsync_NormalInput_PrayerTimesForThatDay()
    {
        // ARRANGE
        ServiceProvider serviceProvider = GetServiceProvider(SubstitutionHelper.GetMockedWaqtiApiService());
        List<GenericSettingConfiguration> configs =
            [
                new WaqtiCalculationConfiguration { TimeType = ETimeType.FajrStart, Method = "Mwl2007" },
                new WaqtiCalculationConfiguration { TimeType = ETimeType.FajrEnd, Method = "Mwl2007" },
                new WaqtiCalculationConfiguration { TimeType = ETimeType.FajrGhalas, Method = "Mwl2007", Degree = -7.5 },
                new WaqtiCalculationConfiguration { TimeType = ETimeType.FajrKaraha, Method = "Mwl2007", Degree = -4.5 },
                new WaqtiCalculationConfiguration { TimeType = ETimeType.DuhaStart, Method = "Mwl2007" },
                new WaqtiCalculationConfiguration { TimeType = ETimeType.DhuhrStart, Method = "Mwl2007" },
                new WaqtiCalculationConfiguration { TimeType = ETimeType.DhuhrEnd, Method = "Mwl2007" },
                new WaqtiCalculationConfiguration { TimeType = ETimeType.AsrStart, Method = "Mwl2007" },
                new WaqtiCalculationConfiguration { TimeType = ETimeType.AsrEnd, Method = "Mwl2007" },
                new WaqtiCalculationConfiguration { TimeType = ETimeType.AsrMithlayn, Method = "Mwl2007" },
                new WaqtiCalculationConfiguration { TimeType = ETimeType.AsrKaraha, Method = "Mwl2007", Degree = 4.5 },
                new WaqtiCalculationConfiguration { TimeType = ETimeType.MaghribStart, Method = "Mwl2007" },
                new WaqtiCalculationConfiguration { TimeType = ETimeType.MaghribEnd, Method = "Mwl2007" },
                new WaqtiCalculationConfiguration { TimeType = ETimeType.MaghribIshtibak, Method = "Mwl2007", Degree = -10 },
                new WaqtiCalculationConfiguration { TimeType = ETimeType.IshaStart, Method = "Mwl2007" },
                new WaqtiCalculationConfiguration { TimeType = ETimeType.IshaEnd, Method = "Mwl2007" },
            ];

        WaqtiDynamicPrayerTimeProvider waqtiDynamicPrayerTimeProvider = serviceProvider.GetRequiredService<WaqtiDynamicPrayerTimeProvider>();

        // ACT
        List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> result =
            await waqtiDynamicPrayerTimeProvider.GetPrayerTimesAsync(ZonedDate(s_date), s_innsbruckLocationData, configs, default);

        // ASSERT
        result.Should().HaveCount(16);

        // derived from Fajr / Isha calculated for their own angle (exact raw times)
        result.FirstOrDefault(x => x.TimeType == ETimeType.FajrGhalas).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 05, 02, 30));
        result.FirstOrDefault(x => x.TimeType == ETimeType.FajrKaraha).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 05, 24, 21));
        result.FirstOrDefault(x => x.TimeType == ETimeType.AsrKaraha).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 20, 15, 53));
        result.FirstOrDefault(x => x.TimeType == ETimeType.MaghribIshtibak).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 21, 56, 51));

        result.FirstOrDefault(x => x.TimeType == ETimeType.FajrStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 03, 28, 00));
        result.FirstOrDefault(x => x.TimeType == ETimeType.FajrEnd).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 05, 47, 00));

        result.FirstOrDefault(x => x.TimeType == ETimeType.DuhaStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 06, 24, 00));

        result.FirstOrDefault(x => x.TimeType == ETimeType.DhuhrStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 13, 25, 00));
        result.FirstOrDefault(x => x.TimeType == ETimeType.DhuhrEnd).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 17, 29, 00));

        result.FirstOrDefault(x => x.TimeType == ETimeType.AsrStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 17, 29, 00));
        result.FirstOrDefault(x => x.TimeType == ETimeType.AsrEnd).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 20, 53, 00));
        result.FirstOrDefault(x => x.TimeType == ETimeType.AsrMithlayn).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 18, 37, 00));

        result.FirstOrDefault(x => x.TimeType == ETimeType.MaghribStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 20, 53, 00));
        result.FirstOrDefault(x => x.TimeType == ETimeType.MaghribEnd).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 23, 01, 00));

        result.FirstOrDefault(x => x.TimeType == ETimeType.IshaStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 23, 01, 00));
        result.FirstOrDefault(x => x.TimeType == ETimeType.IshaEnd).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 31, 03, 30, 00));
    }

    [Fact]
    public async Task GetPrayerTimesAsync_DerivedTimeTypes_EachAnglePairStoredAsOwnMonth()
    {
        // ARRANGE
        ServiceProvider serviceProvider = GetServiceProvider(SubstitutionHelper.GetMockedWaqtiApiService());
        List<GenericSettingConfiguration> configs =
            [
                new WaqtiCalculationConfiguration { TimeType = ETimeType.FajrGhalas, Method = "Mwl2007", Degree = -7.5 },
                new WaqtiCalculationConfiguration { TimeType = ETimeType.FajrKaraha, Method = "Mwl2007", Degree = -4.5 },
                new WaqtiCalculationConfiguration { TimeType = ETimeType.AsrKaraha, Method = "Mwl2007", Degree = 4.5 },
                new WaqtiCalculationConfiguration { TimeType = ETimeType.MaghribIshtibak, Method = "Mwl2007", Degree = -10 },
            ];

        WaqtiDynamicPrayerTimeProvider waqtiDynamicPrayerTimeProvider = serviceProvider.GetRequiredService<WaqtiDynamicPrayerTimeProvider>();

        // ACT
        await waqtiDynamicPrayerTimeProvider.GetPrayerTimesAsync(ZonedDate(s_date), s_innsbruckLocationData, configs, default);

        // ASSERT
        using AppDbContext dbContext = await GetHandledDbContextFactory().CreateDbContextAsync();
        (await dbContext.WaqtiPrayerTimes.CountAsync(x => x.FajrAngle == 4.5 && x.IshaaAngle == -4.5)).Should().Be(31);
        (await dbContext.WaqtiPrayerTimes.CountAsync(x => x.FajrAngle == 7.5 && x.IshaaAngle == 10)).Should().Be(31);
        (await dbContext.WaqtiPrayerTimes.CountAsync()).Should().Be(62);
    }

    [Fact]
    public async Task GetPrayerTimesAsync_TwoMethods_EachTimeFromItsMethod()
    {
        // ARRANGE
        ServiceProvider serviceProvider = GetServiceProvider(SubstitutionHelper.GetMockedWaqtiApiService());
        List<GenericSettingConfiguration> configs =
            [
                new WaqtiCalculationConfiguration { TimeType = ETimeType.FajrStart, Method = "Diyanet" },
                new WaqtiCalculationConfiguration { TimeType = ETimeType.FajrEnd, Method = "Mwl2007" },
                new WaqtiCalculationConfiguration { TimeType = ETimeType.MaghribStart, Method = "Diyanet" },
                new WaqtiCalculationConfiguration { TimeType = ETimeType.IshaStart, Method = "Mwl2007" },
            ];

        WaqtiDynamicPrayerTimeProvider waqtiDynamicPrayerTimeProvider = serviceProvider.GetRequiredService<WaqtiDynamicPrayerTimeProvider>();

        // ACT
        List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> result =
            await waqtiDynamicPrayerTimeProvider.GetPrayerTimesAsync(ZonedDate(s_date), s_innsbruckLocationData, configs, default);

        // ASSERT
        result.Should().HaveCount(4);
        result.FirstOrDefault(x => x.TimeType == ETimeType.FajrStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 03, 30, 00));
        result.FirstOrDefault(x => x.TimeType == ETimeType.FajrEnd).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 05, 47, 00));
        result.FirstOrDefault(x => x.TimeType == ETimeType.MaghribStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 20, 57, 00));
        result.FirstOrDefault(x => x.TimeType == ETimeType.IshaStart).ZonedDateTime.LocalDateTime.Should().Be(new LocalDateTime(2023, 7, 30, 23, 01, 00));

        // one stored month per method
        using AppDbContext dbContext = await GetHandledDbContextFactory().CreateDbContextAsync();
        (await dbContext.WaqtiPrayerTimes.CountAsync(x => x.Method == "Mwl2007")).Should().Be(31);
        (await dbContext.WaqtiPrayerTimes.CountAsync(x => x.Method == "Diyanet")).Should().Be(31);
    }

    [Fact]
    public async Task GetPrayerTimesAsync_OtherDayOfAnAlreadyLoadedMonth_TakenFromDbWithoutApiRequest()
    {
        // ARRANGE
        IWaqtiApiService mockedApiService = SubstitutionHelper.GetMockedWaqtiApiService();
        IWaqtiApiService countingApiService = Substitute.For<IWaqtiApiService>();
        countingApiService
            .GetMonthPrayerTimesAsync(Arg.Any<WaqtiMonthPrayerTimesRequestDTO>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => mockedApiService.GetMonthPrayerTimesAsync(callInfo.Arg<WaqtiMonthPrayerTimesRequestDTO>(), callInfo.Arg<CancellationToken>()));

        ServiceProvider serviceProvider = GetServiceProvider(countingApiService);
        WaqtiDynamicPrayerTimeProvider waqtiDynamicPrayerTimeProvider = serviceProvider.GetRequiredService<WaqtiDynamicPrayerTimeProvider>();
        List<GenericSettingConfiguration> configs =
            [
                new WaqtiCalculationConfiguration { TimeType = ETimeType.DhuhrStart, Method = "Mwl2007" },
            ];

        // ACT
        await waqtiDynamicPrayerTimeProvider.GetPrayerTimesAsync(ZonedDate(s_date), s_innsbruckLocationData, configs, default);
        List<(ETimeType TimeType, ZonedDateTime ZonedDateTime)> resultOfOtherDay =
            await waqtiDynamicPrayerTimeProvider.GetPrayerTimesAsync(ZonedDate(new LocalDate(2023, 7, 1)), s_innsbruckLocationData, configs, default);

        // ASSERT
        resultOfOtherDay.Should().ContainSingle();
        resultOfOtherDay.Single().ZonedDateTime.Date.Should().Be(new LocalDate(2023, 7, 1));
        await countingApiService.Received(1).GetMonthPrayerTimesAsync(Arg.Any<WaqtiMonthPrayerTimesRequestDTO>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPrayerTimesAsync_ParallelRequestsForTheSameMonth_OnlyOneApiRequestAndNoDuplicates()
    {
        // ARRANGE
        IWaqtiApiService mockedApiService = SubstitutionHelper.GetMockedWaqtiApiService();
        IWaqtiApiService countingApiService = Substitute.For<IWaqtiApiService>();
        countingApiService
            .GetMonthPrayerTimesAsync(Arg.Any<WaqtiMonthPrayerTimesRequestDTO>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => mockedApiService.GetMonthPrayerTimesAsync(callInfo.Arg<WaqtiMonthPrayerTimesRequestDTO>(), callInfo.Arg<CancellationToken>()));

        ServiceProvider serviceProvider = GetServiceProvider(countingApiService);
        WaqtiDynamicPrayerTimeProvider waqtiDynamicPrayerTimeProvider = serviceProvider.GetRequiredService<WaqtiDynamicPrayerTimeProvider>();
        List<GenericSettingConfiguration> configs =
            [
                new WaqtiCalculationConfiguration { TimeType = ETimeType.DhuhrStart, Method = "Mwl2007" },
            ];

        // ACT
        await Task.WhenAll(
            Enumerable.Range(1, 5).Select(day =>
                waqtiDynamicPrayerTimeProvider.GetPrayerTimesAsync(ZonedDate(new LocalDate(2023, 7, day)), s_innsbruckLocationData, configs, default)));

        // ASSERT
        await countingApiService.Received(1).GetMonthPrayerTimesAsync(Arg.Any<WaqtiMonthPrayerTimesRequestDTO>(), Arg.Any<CancellationToken>());

        using AppDbContext dbContext = await GetHandledDbContextFactory().CreateDbContextAsync();
        (await dbContext.WaqtiPrayerTimes.CountAsync()).Should().Be(31);
    }
}
