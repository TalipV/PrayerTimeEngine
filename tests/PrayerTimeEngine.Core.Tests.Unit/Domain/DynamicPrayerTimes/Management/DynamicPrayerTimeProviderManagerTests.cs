using Microsoft.Extensions.Logging;
using NodaTime;
using NSubstitute;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Management;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Models;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Models;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.DynamicPrayerTimes.Management;

public class DynamicPrayerTimeProviderManagerTests : BaseTest
{
    private readonly IDynamicPrayerTimeProviderFactory _prayerTimeServiceFactoryMock;
    private readonly IProfileService _profileServiceMock;
    private readonly ILogger<DynamicPrayerTimeProviderManager> _loggerMock;
    private readonly DynamicPrayerTimeProviderManager _dynamicPrayerTimeProviderManager;
    private Instant _currentInstant = Instant.FromUtc(2024, 1, 1, 12, 0);

    public DynamicPrayerTimeProviderManagerTests()
    {
        _prayerTimeServiceFactoryMock = Substitute.For<IDynamicPrayerTimeProviderFactory>();
        _profileServiceMock = Substitute.For<IProfileService>();
        _loggerMock = Substitute.For<ILogger<DynamicPrayerTimeProviderManager>>();

        ISystemInfoService systemInfoServiceMock = Substitute.For<ISystemInfoService>();
        systemInfoServiceMock.GetCurrentInstant().Returns(_ => _currentInstant);

        _dynamicPrayerTimeProviderManager = new DynamicPrayerTimeProviderManager(
            _prayerTimeServiceFactoryMock,
            _profileServiceMock,
            systemInfoServiceMock,
            _loggerMock,
            [Substitute.For<IPrayerTimeCacheCleaner>()]);
    }

    #region CalculatePrayerTimesAsync

    [Fact]
    //[Trait("Method", "CalculatePrayerTimesAsync")]
    public async Task CalculatePrayerTimesAsync_OneComplexCalculation_CalculatedSuccessfully()
    {
        // ARRANGE
        DynamicProfile profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        ZonedDateTime zonedDate = new LocalDate(2024, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);
        ZonedDateTime zonedDateOneDayBefore = zonedDate.Plus(Duration.FromDays(-1));
        ZonedDateTime zonedDateOneDayAfter = zonedDate.Plus(Duration.FromDays(1));

        BaseLocationData muwaqqitLocationData = Substitute.ForPartsOf<BaseLocationData>();
        _profileServiceMock.GetUntrackedReferenceOfProfile(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(profile);
        _profileServiceMock.GetLocationConfig(Arg.Is(profile), Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit)).Returns(muwaqqitLocationData);

        GenericSettingConfiguration muwaqqitConfig = new MuwaqqitDegreeCalculationConfiguration { Degree = 14, TimeType = ETimeType.FajrStart };
        IDynamicPrayerTimeProvider muwaqqitPrayerTimeServiceMock = Substitute.For<IDynamicPrayerTimeProvider>();

        _prayerTimeServiceFactoryMock.GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit)).Returns(muwaqqitPrayerTimeServiceMock);

        List<(ETimeType, ZonedDateTime)> muwaqqitReturnValuePreviousDay = [(ETimeType.FajrStart, zonedDate.PlusHours(3))];
        List<(ETimeType, ZonedDateTime)> muwaqqitReturnValueCurrentDay = [(ETimeType.FajrStart, zonedDate.PlusHours(4))];
        List<(ETimeType, ZonedDateTime)> muwaqqitReturnValueNextDay = [(ETimeType.FajrStart, zonedDate.PlusHours(5))];

        muwaqqitPrayerTimeServiceMock.GetPrayerTimesAsync(
                Arg.Is<ZonedDateTime>(x => x == zonedDate || x == zonedDateOneDayBefore || x == zonedDateOneDayAfter),
                Arg.Is(muwaqqitLocationData),
                Arg.Is<List<GenericSettingConfiguration>>(x => x.Contains(muwaqqitConfig)),
                Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                ZonedDateTime dateInput = callInfo.Arg<ZonedDateTime>();
                if (dateInput == zonedDateOneDayBefore)
                {
                    return Task.FromResult(muwaqqitReturnValuePreviousDay);
                }
                else if (dateInput == zonedDate)
                {
                    return Task.FromResult(muwaqqitReturnValueCurrentDay);
                }
                else if (dateInput == zonedDateOneDayAfter)
                {
                    return Task.FromResult(muwaqqitReturnValueNextDay);
                }

                throw new Exception("Unreachable");
            });

        _profileServiceMock.GetActiveComplexTimeConfigs(Arg.Is(profile)).Returns([muwaqqitConfig]);
        muwaqqitPrayerTimeServiceMock.GetUnsupportedTimeTypes().Returns([]);

        // ACT
        DynamicPrayerTimesDaySet result = (await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, zonedDate, default)).DynamicPrayerTimesDaySet;

        // ASSERT
        result.Should().NotBeNull();

        result.PreviousDay.Fajr.Start.Should().Be(zonedDate.PlusHours(3));
        result.CurrentDay.Fajr.Start.Should().Be(zonedDate.PlusHours(4));
        result.NextDay.Fajr.Start.Should().Be(zonedDate.PlusHours(5));

        await muwaqqitPrayerTimeServiceMock.ReceivedWithAnyArgs(3).GetPrayerTimesAsync(default, default, default, default);
        await muwaqqitPrayerTimeServiceMock.Received(1).GetPrayerTimesAsync(
                Arg.Is(zonedDate),
                Arg.Is(muwaqqitLocationData),
                Arg.Is<List<GenericSettingConfiguration>>(x => x.Contains(muwaqqitConfig)),
                Arg.Any<CancellationToken>());
        await muwaqqitPrayerTimeServiceMock.Received(1).GetPrayerTimesAsync(
                Arg.Is(zonedDateOneDayBefore),
                Arg.Is(muwaqqitLocationData),
                Arg.Is<List<GenericSettingConfiguration>>(x => x.Contains(muwaqqitConfig)),
                Arg.Any<CancellationToken>());
        await muwaqqitPrayerTimeServiceMock.Received(1).GetPrayerTimesAsync(
                Arg.Is(zonedDateOneDayAfter),
                Arg.Is(muwaqqitLocationData),
                Arg.Is<List<GenericSettingConfiguration>>(x => x.Contains(muwaqqitConfig)),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CalculatePrayerTimesAsync_OneComplexCalculationFails_ReturnsCalculationErrors()
    {
        // ARRANGE
        DynamicProfile profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        ZonedDateTime zonedDate = new LocalDate(2024, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);

        BaseLocationData muwaqqitLocationData = Substitute.ForPartsOf<BaseLocationData>();
        _profileServiceMock.GetUntrackedReferenceOfProfile(
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(profile);
        _profileServiceMock.GetLocationConfig(
                Arg.Is(profile),
                Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit))
            .Returns(muwaqqitLocationData);

        GenericSettingConfiguration muwaqqitConfig = new MuwaqqitDegreeCalculationConfiguration { Degree = 14, TimeType = ETimeType.FajrStart };
        IDynamicPrayerTimeProvider muwaqqitPrayerTimeServiceMock = Substitute.For<IDynamicPrayerTimeProvider>();

        _prayerTimeServiceFactoryMock
            .GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit))
            .Returns(muwaqqitPrayerTimeServiceMock);

        var expextedException = new InvalidOperationException("calculator failed");
        muwaqqitPrayerTimeServiceMock.GetPrayerTimesAsync(
                Arg.Any<ZonedDateTime>(),
                Arg.Is(muwaqqitLocationData),
                Arg.Is<List<GenericSettingConfiguration>>(x => x.Contains(muwaqqitConfig)),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<(ETimeType, ZonedDateTime)>>(expextedException));

        _profileServiceMock.GetActiveComplexTimeConfigs(Arg.Is(profile)).Returns([muwaqqitConfig]);
        muwaqqitPrayerTimeServiceMock.GetUnsupportedTimeTypes().Returns([]);

        // ACT
        CalculatePrayerTimesResultVO result =
            await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(
                profile.ID,
                zonedDate,
                default);

        // ASSERT
        result.DynamicPrayerTimesDaySet.Should().NotBeNull();
        result.CalculationErrors.Should().HaveCount(3).And.OnlyContain(x =>
            x.DynamicPrayerTimeProviderType == EDynamicPrayerTimeProviderType.Muwaqqit
            && x.TimeTypes.SequenceEqual(new[] { ETimeType.FajrStart })
            && x.Exception != null
            && x.Exception.GetType().FullName == expextedException.GetType().FullName
            && x.Exception.Message == expextedException.Message);
    }

    [Fact]
    public async Task CalculatePrayerTimesAsync_OneOfTwoProvidersFails_OtherProviderStillCalculated()
    {
        // ARRANGE
        DynamicProfile profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        ZonedDateTime zonedDate = new LocalDate(2024, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);

        _profileServiceMock.GetUntrackedReferenceOfProfile(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(profile);

        // failing Muwaqqit provider
        BaseLocationData muwaqqitLocationData = Substitute.ForPartsOf<BaseLocationData>();
        _profileServiceMock.GetLocationConfig(Arg.Is(profile), Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit)).Returns(muwaqqitLocationData);
        GenericSettingConfiguration muwaqqitConfig = new MuwaqqitDegreeCalculationConfiguration { Degree = 14, TimeType = ETimeType.FajrStart };
        IDynamicPrayerTimeProvider muwaqqitPrayerTimeServiceMock = Substitute.For<IDynamicPrayerTimeProvider>();
        muwaqqitPrayerTimeServiceMock.GetUnsupportedTimeTypes().Returns([]);
        muwaqqitPrayerTimeServiceMock.GetPrayerTimesAsync(
                Arg.Any<ZonedDateTime>(),
                Arg.Is(muwaqqitLocationData),
                Arg.Any<List<GenericSettingConfiguration>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<(ETimeType, ZonedDateTime)>>(new InvalidOperationException("muwaqqit failed")));
        _prayerTimeServiceFactoryMock.GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit)).Returns(muwaqqitPrayerTimeServiceMock);

        // succeeding Fazilet provider
        BaseLocationData faziletLocationData = Substitute.ForPartsOf<BaseLocationData>();
        _profileServiceMock.GetLocationConfig(Arg.Is(profile), Arg.Is(EDynamicPrayerTimeProviderType.Fazilet)).Returns(faziletLocationData);
        GenericSettingConfiguration faziletConfig = new() { TimeType = ETimeType.DhuhrStart, Source = EDynamicPrayerTimeProviderType.Fazilet };
        IDynamicPrayerTimeProvider faziletPrayerTimeServiceMock = Substitute.For<IDynamicPrayerTimeProvider>();
        faziletPrayerTimeServiceMock.GetUnsupportedTimeTypes().Returns([]);
        faziletPrayerTimeServiceMock.GetPrayerTimesAsync(
                Arg.Any<ZonedDateTime>(),
                Arg.Is(faziletLocationData),
                Arg.Any<List<GenericSettingConfiguration>>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult<List<(ETimeType, ZonedDateTime)>>([(ETimeType.DhuhrStart, callInfo.Arg<ZonedDateTime>().PlusHours(12))]));
        _prayerTimeServiceFactoryMock.GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(Arg.Is(EDynamicPrayerTimeProviderType.Fazilet)).Returns(faziletPrayerTimeServiceMock);

        _profileServiceMock.GetActiveComplexTimeConfigs(Arg.Is(profile)).Returns([muwaqqitConfig, faziletConfig]);

        // ACT
        CalculatePrayerTimesResultVO result = await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, zonedDate, default);

        // ASSERT
        result.DynamicPrayerTimesDaySet.Should().NotBeNull();

        // the Fazilet times were calculated despite the Muwaqqit failure
        result.DynamicPrayerTimesDaySet.PreviousDay.Dhuhr.Start.Should().Be(zonedDate.Plus(Duration.FromDays(-1)).PlusHours(12));
        result.DynamicPrayerTimesDaySet.CurrentDay.Dhuhr.Start.Should().Be(zonedDate.PlusHours(12));
        result.DynamicPrayerTimesDaySet.NextDay.Dhuhr.Start.Should().Be(zonedDate.Plus(Duration.FromDays(1)).PlusHours(12));
        result.DynamicPrayerTimesDaySet.CurrentDay.Fajr.Start.Should().BeNull();

        // and the Muwaqqit failure is visible as calculation errors (one per calculated day)
        result.CalculationErrors.Should().HaveCount(3).And.OnlyContain(x =>
            x.DynamicPrayerTimeProviderType == EDynamicPrayerTimeProviderType.Muwaqqit);
    }

    [Fact]
    public async Task CalculatePrayerTimesAsync_ProviderWithoutLocationData_ProviderSkippedWithoutErrors()
    {
        // ARRANGE
        DynamicProfile profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        ZonedDateTime zonedDate = new LocalDate(2024, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);

        _profileServiceMock.GetUntrackedReferenceOfProfile(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(profile);

        // Muwaqqit without location data
        _profileServiceMock.GetLocationConfig(Arg.Is(profile), Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit)).Returns((BaseLocationData)null);
        GenericSettingConfiguration muwaqqitConfig = new MuwaqqitDegreeCalculationConfiguration { Degree = 14, TimeType = ETimeType.FajrStart };

        // Fazilet with location data
        BaseLocationData faziletLocationData = Substitute.ForPartsOf<BaseLocationData>();
        _profileServiceMock.GetLocationConfig(Arg.Is(profile), Arg.Is(EDynamicPrayerTimeProviderType.Fazilet)).Returns(faziletLocationData);
        GenericSettingConfiguration faziletConfig = new() { TimeType = ETimeType.DhuhrStart, Source = EDynamicPrayerTimeProviderType.Fazilet };
        IDynamicPrayerTimeProvider faziletPrayerTimeServiceMock = Substitute.For<IDynamicPrayerTimeProvider>();
        faziletPrayerTimeServiceMock.GetUnsupportedTimeTypes().Returns([]);
        faziletPrayerTimeServiceMock.GetPrayerTimesAsync(
                Arg.Any<ZonedDateTime>(),
                Arg.Is(faziletLocationData),
                Arg.Any<List<GenericSettingConfiguration>>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult<List<(ETimeType, ZonedDateTime)>>([(ETimeType.DhuhrStart, callInfo.Arg<ZonedDateTime>().PlusHours(12))]));
        _prayerTimeServiceFactoryMock.GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(Arg.Is(EDynamicPrayerTimeProviderType.Fazilet)).Returns(faziletPrayerTimeServiceMock);

        _profileServiceMock.GetActiveComplexTimeConfigs(Arg.Is(profile)).Returns([muwaqqitConfig, faziletConfig]);

        // ACT
        CalculatePrayerTimesResultVO result = await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, zonedDate, default);

        // ASSERT
        result.DynamicPrayerTimesDaySet.Should().NotBeNull();
        result.CalculationErrors.Should().BeEmpty();

        // missing location data just means that the associated times remain at null
        result.DynamicPrayerTimesDaySet.CurrentDay.Dhuhr.Start.Should().Be(zonedDate.PlusHours(12));
        result.DynamicPrayerTimesDaySet.CurrentDay.Fajr.Start.Should().BeNull();

        // the Muwaqqit provider wasn't even created
        _prayerTimeServiceFactoryMock.DidNotReceive().GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit));
    }

    [Fact]
    public async Task CalculatePrayerTimesAsync_ConfigsWithUnsupportedTimeTypes_ReturnsCalculationErrorsInsteadOfThrowing()
    {
        // ARRANGE
        DynamicProfile profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        ZonedDateTime zonedDate = new LocalDate(2024, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);

        _profileServiceMock.GetUntrackedReferenceOfProfile(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(profile);

        // Muwaqqit with a config whose time type it doesn't support
        BaseLocationData muwaqqitLocationData = Substitute.ForPartsOf<BaseLocationData>();
        _profileServiceMock.GetLocationConfig(Arg.Is(profile), Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit)).Returns(muwaqqitLocationData);
        GenericSettingConfiguration muwaqqitConfig = new MuwaqqitDegreeCalculationConfiguration { Degree = 14, TimeType = ETimeType.FajrStart };
        IDynamicPrayerTimeProvider muwaqqitPrayerTimeServiceMock = Substitute.For<IDynamicPrayerTimeProvider>();
        muwaqqitPrayerTimeServiceMock.GetUnsupportedTimeTypes().Returns([ETimeType.FajrStart]);
        _prayerTimeServiceFactoryMock.GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit)).Returns(muwaqqitPrayerTimeServiceMock);

        // valid Fazilet provider
        BaseLocationData faziletLocationData = Substitute.ForPartsOf<BaseLocationData>();
        _profileServiceMock.GetLocationConfig(Arg.Is(profile), Arg.Is(EDynamicPrayerTimeProviderType.Fazilet)).Returns(faziletLocationData);
        GenericSettingConfiguration faziletConfig = new() { TimeType = ETimeType.DhuhrStart, Source = EDynamicPrayerTimeProviderType.Fazilet };
        IDynamicPrayerTimeProvider faziletPrayerTimeServiceMock = Substitute.For<IDynamicPrayerTimeProvider>();
        faziletPrayerTimeServiceMock.GetUnsupportedTimeTypes().Returns([]);
        faziletPrayerTimeServiceMock.GetPrayerTimesAsync(
                Arg.Any<ZonedDateTime>(),
                Arg.Is(faziletLocationData),
                Arg.Any<List<GenericSettingConfiguration>>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult<List<(ETimeType, ZonedDateTime)>>([(ETimeType.DhuhrStart, callInfo.Arg<ZonedDateTime>().PlusHours(12))]));
        _prayerTimeServiceFactoryMock.GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(Arg.Is(EDynamicPrayerTimeProviderType.Fazilet)).Returns(faziletPrayerTimeServiceMock);

        _profileServiceMock.GetActiveComplexTimeConfigs(Arg.Is(profile)).Returns([muwaqqitConfig, faziletConfig]);

        // ACT
        CalculatePrayerTimesResultVO result = await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, zonedDate, default);

        // ASSERT
        result.DynamicPrayerTimesDaySet.Should().NotBeNull();
        result.DynamicPrayerTimesDaySet.CurrentDay.Dhuhr.Start.Should().Be(zonedDate.PlusHours(12));

        result.CalculationErrors.Should().HaveCount(3).And.OnlyContain(x =>
            x.DynamicPrayerTimeProviderType == EDynamicPrayerTimeProviderType.Muwaqqit
            && x.Exception is ArgumentException);

        await muwaqqitPrayerTimeServiceMock.DidNotReceiveWithAnyArgs().GetPrayerTimesAsync(default, default, default, default);
    }

    [Fact]
    public async Task CalculatePrayerTimesAsync_SuccessfulCalculation_SecondCallServedFromCache()
    {
        // ARRANGE
        DynamicProfile profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        ZonedDateTime zonedDate = new LocalDate(2024, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);

        BaseLocationData muwaqqitLocationData = Substitute.ForPartsOf<BaseLocationData>();
        _profileServiceMock.GetUntrackedReferenceOfProfile(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(profile);
        _profileServiceMock.GetLocationConfig(Arg.Is(profile), Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit)).Returns(muwaqqitLocationData);

        GenericSettingConfiguration muwaqqitConfig = new MuwaqqitDegreeCalculationConfiguration { Degree = 14, TimeType = ETimeType.FajrStart };
        IDynamicPrayerTimeProvider muwaqqitPrayerTimeServiceMock = Substitute.For<IDynamicPrayerTimeProvider>();
        muwaqqitPrayerTimeServiceMock.GetUnsupportedTimeTypes().Returns([]);
        muwaqqitPrayerTimeServiceMock.GetPrayerTimesAsync(
                Arg.Any<ZonedDateTime>(),
                Arg.Is(muwaqqitLocationData),
                Arg.Any<List<GenericSettingConfiguration>>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult<List<(ETimeType, ZonedDateTime)>>([(ETimeType.FajrStart, callInfo.Arg<ZonedDateTime>().PlusHours(4))]));
        _prayerTimeServiceFactoryMock.GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit)).Returns(muwaqqitPrayerTimeServiceMock);

        _profileServiceMock.GetActiveComplexTimeConfigs(Arg.Is(profile)).Returns([muwaqqitConfig]);

        // ACT
        CalculatePrayerTimesResultVO firstResult = await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, zonedDate, default);
        CalculatePrayerTimesResultVO secondResult = await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, zonedDate, default);

        // ASSERT
        firstResult.CalculationErrors.Should().BeEmpty();
        secondResult.DynamicPrayerTimesDaySet.Should().BeSameAs(firstResult.DynamicPrayerTimesDaySet);

        // 3 calls (previous, current and next day) for the first calculation, none for the second one
        await muwaqqitPrayerTimeServiceMock.ReceivedWithAnyArgs(3).GetPrayerTimesAsync(default, default, default, default);
    }

    [Fact]
    public async Task CalculatePrayerTimesAsync_FailedCalculation_SecondCallWithinRetryDelayServedFromCache()
    {
        // ARRANGE
        DynamicProfile profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        ZonedDateTime zonedDate = new LocalDate(2024, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);
        IDynamicPrayerTimeProvider muwaqqitPrayerTimeServiceMock = SetUpMuwaqqitProvider(profile, shouldFail: () => true);

        // ACT
        CalculatePrayerTimesResultVO firstResult = await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, zonedDate, default);
        _currentInstant += Duration.FromSeconds(29);
        CalculatePrayerTimesResultVO secondResult = await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, zonedDate, default);

        // ASSERT
        firstResult.CalculationErrors.Should().HaveCount(3);
        secondResult.CalculationErrors.Should().HaveCount(3);
        secondResult.DynamicPrayerTimesDaySet.Should().BeSameAs(firstResult.DynamicPrayerTimesDaySet);

        // the failed provider must not be asked again before the retry delay passed (only the 3 initial calls)
        await muwaqqitPrayerTimeServiceMock.ReceivedWithAnyArgs(3).GetPrayerTimesAsync(default, default, default, default);
    }

    [Fact]
    public async Task CalculatePrayerTimesAsync_FailedCalculation_CallAfterRetryDelayRecalculates()
    {
        // ARRANGE
        DynamicProfile profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        ZonedDateTime zonedDate = new LocalDate(2024, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);
        IDynamicPrayerTimeProvider muwaqqitPrayerTimeServiceMock = SetUpMuwaqqitProvider(profile, shouldFail: () => true);

        // ACT
        await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, zonedDate, default);
        _currentInstant += Duration.FromSeconds(30);
        CalculatePrayerTimesResultVO secondResult = await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, zonedDate, default);

        // ASSERT
        secondResult.CalculationErrors.Should().HaveCount(3);
        await muwaqqitPrayerTimeServiceMock.ReceivedWithAnyArgs(6).GetPrayerTimesAsync(default, default, default, default);
    }

    [Fact]
    public async Task CalculatePrayerTimesAsync_ConsecutiveFailures_RetryDelayDoubles()
    {
        // ARRANGE
        DynamicProfile profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        ZonedDateTime zonedDate = new LocalDate(2024, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);
        IDynamicPrayerTimeProvider muwaqqitPrayerTimeServiceMock = SetUpMuwaqqitProvider(profile, shouldFail: () => true);

        // ACT + ASSERT
        // first failure: retry after 30 seconds
        await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, zonedDate, default);
        _currentInstant += Duration.FromSeconds(30);

        // second failure: retry after 60 seconds
        await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, zonedDate, default);
        await muwaqqitPrayerTimeServiceMock.ReceivedWithAnyArgs(6).GetPrayerTimesAsync(default, default, default, default);

        _currentInstant += Duration.FromSeconds(59);
        await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, zonedDate, default);
        await muwaqqitPrayerTimeServiceMock.ReceivedWithAnyArgs(6).GetPrayerTimesAsync(default, default, default, default);

        _currentInstant += Duration.FromSeconds(1);
        await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, zonedDate, default);
        await muwaqqitPrayerTimeServiceMock.ReceivedWithAnyArgs(9).GetPrayerTimesAsync(default, default, default, default);
    }

    [Fact]
    public async Task CalculatePrayerTimesAsync_ProviderRecoversAfterFailure_CompleteResultCached()
    {
        // ARRANGE
        DynamicProfile profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        ZonedDateTime zonedDate = new LocalDate(2024, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);
        bool isProviderDown = true;
        IDynamicPrayerTimeProvider muwaqqitPrayerTimeServiceMock = SetUpMuwaqqitProvider(profile, shouldFail: () => isProviderDown);

        // ACT
        await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, zonedDate, default);
        isProviderDown = false;
        _currentInstant += Duration.FromSeconds(30);
        CalculatePrayerTimesResultVO recoveredResult = await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, zonedDate, default);
        _currentInstant += Duration.FromHours(1);
        CalculatePrayerTimesResultVO cachedResult = await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, zonedDate, default);

        // ASSERT
        recoveredResult.CalculationErrors.Should().BeEmpty();
        recoveredResult.DynamicPrayerTimesDaySet.CurrentDay.Fajr.Start.Should().Be(zonedDate.PlusHours(4));
        cachedResult.DynamicPrayerTimesDaySet.Should().BeSameAs(recoveredResult.DynamicPrayerTimesDaySet);
        await muwaqqitPrayerTimeServiceMock.ReceivedWithAnyArgs(6).GetPrayerTimesAsync(default, default, default, default);

        // one recovery message per day (previous, current and next)
        GetLogCount(LogLevel.Information).Should().Be(3);
    }

    [Fact]
    public async Task CalculatePrayerTimesAsync_ProviderKeepsFailing_OnlyFirstFailureLoggedAsError()
    {
        // ARRANGE
        DynamicProfile profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        ZonedDateTime zonedDate = new LocalDate(2024, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);
        SetUpMuwaqqitProvider(profile, shouldFail: () => true);

        // ACT
        // three failing attempts, each one right after the previous retry delay (30 s, then 60 s) passed
        await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, zonedDate, default);
        _currentInstant += Duration.FromSeconds(30);
        await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, zonedDate, default);
        _currentInstant += Duration.FromSeconds(60);
        await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, zonedDate, default);

        // calls within the retry delay are neither forwarded nor logged
        for (int i = 0; i < 10; i++)
        {
            await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, zonedDate, default);
        }

        // ASSERT
        // per day (previous, current and next): the first failure as error, the following ones as warning
        GetLogCount(LogLevel.Error).Should().Be(3);
        GetLogCount(LogLevel.Warning).Should().Be(6);
    }

    [Fact]
    public async Task CalculatePrayerTimesAsync_ProfileChangedDuringRetryDelay_Recalculates()
    {
        // ARRANGE
        DynamicProfile profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        ZonedDateTime zonedDate = new LocalDate(2024, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);
        IDynamicPrayerTimeProvider muwaqqitPrayerTimeServiceMock = SetUpMuwaqqitProvider(profile, shouldFail: () => true);

        // ACT
        await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, zonedDate, default);

        // e.g. the user picked another location to get rid of the error
        _profileServiceMock.GetProfileVersion(profile.ID).Returns(1);
        await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, zonedDate, default);

        // ASSERT
        await muwaqqitPrayerTimeServiceMock.ReceivedWithAnyArgs(6).GetPrayerTimesAsync(default, default, default, default);
    }
    [Fact]
    public async Task CalculatePrayerTimesAsync_RequestForOtherDate_OnlyLatestCalculationCached()
    {
        // ARRANGE
        DynamicProfile profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        ZonedDateTime firstZonedDate = new LocalDate(2024, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);
        ZonedDateTime secondZonedDate = new LocalDate(2024, 1, 5).AtStartOfDayInZone(DateTimeZone.Utc);

        BaseLocationData muwaqqitLocationData = Substitute.ForPartsOf<BaseLocationData>();
        _profileServiceMock.GetUntrackedReferenceOfProfile(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(profile);
        _profileServiceMock.GetLocationConfig(Arg.Is(profile), Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit)).Returns(muwaqqitLocationData);

        GenericSettingConfiguration muwaqqitConfig = new MuwaqqitDegreeCalculationConfiguration { Degree = 14, TimeType = ETimeType.FajrStart };
        IDynamicPrayerTimeProvider muwaqqitPrayerTimeServiceMock = Substitute.For<IDynamicPrayerTimeProvider>();
        muwaqqitPrayerTimeServiceMock.GetUnsupportedTimeTypes().Returns([]);
        muwaqqitPrayerTimeServiceMock.GetPrayerTimesAsync(
                Arg.Any<ZonedDateTime>(),
                Arg.Is(muwaqqitLocationData),
                Arg.Any<List<GenericSettingConfiguration>>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult<List<(ETimeType, ZonedDateTime)>>([(ETimeType.FajrStart, callInfo.Arg<ZonedDateTime>().PlusHours(4))]));
        _prayerTimeServiceFactoryMock.GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit)).Returns(muwaqqitPrayerTimeServiceMock);

        _profileServiceMock.GetActiveComplexTimeConfigs(Arg.Is(profile)).Returns([muwaqqitConfig]);

        // ACT
        await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, firstZonedDate, default);
        await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, secondZonedDate, default);
        await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, firstZonedDate, default);

        // ASSERT
        // only the latest calculation is kept per profile, so the third call has to recalculate
        // the first date (3 provider calls per calculation, i.e. 9 in total without a single cache hit)
        await muwaqqitPrayerTimeServiceMock.ReceivedWithAnyArgs(9).GetPrayerTimesAsync(default, default, default, default);
    }

    #endregion CalculatePrayerTimesAsync

    #region TryGetAlreadyCalculatedPrayerTimes

    [Fact]
    public void TryGetAlreadyCalculatedPrayerTimes_NothingCalculatedYet_ReturnsFalseWithoutCalculating()
    {
        // ARRANGE
        DynamicProfile profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        ZonedDateTime zonedDate = new LocalDate(2024, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);

        // ACT
        bool found = _dynamicPrayerTimeProviderManager.TryGetAlreadyCalculatedPrayerTimes(profile.ID, zonedDate, out DynamicPrayerTimesDaySet daySet);

        // ASSERT
        found.Should().BeFalse();
        daySet.Should().BeNull();

        // a pure read must never reach the profile service to trigger a (potentially networking) calculation
        _profileServiceMock.DidNotReceiveWithAnyArgs().GetUntrackedReferenceOfProfile(default, default);
    }

    [Fact]
    public async Task TryGetAlreadyCalculatedPrayerTimes_AfterSuccessfulCalculation_ReturnsCachedDaySetWithoutRecalculating()
    {
        // ARRANGE
        DynamicProfile profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        ZonedDateTime zonedDate = new LocalDate(2024, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);

        BaseLocationData muwaqqitLocationData = Substitute.ForPartsOf<BaseLocationData>();
        _profileServiceMock.GetUntrackedReferenceOfProfile(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(profile);
        _profileServiceMock.GetLocationConfig(Arg.Is(profile), Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit)).Returns(muwaqqitLocationData);

        GenericSettingConfiguration muwaqqitConfig = new MuwaqqitDegreeCalculationConfiguration { Degree = 14, TimeType = ETimeType.FajrStart };
        IDynamicPrayerTimeProvider muwaqqitPrayerTimeServiceMock = Substitute.For<IDynamicPrayerTimeProvider>();
        muwaqqitPrayerTimeServiceMock.GetUnsupportedTimeTypes().Returns([]);
        muwaqqitPrayerTimeServiceMock.GetPrayerTimesAsync(
                Arg.Any<ZonedDateTime>(),
                Arg.Is(muwaqqitLocationData),
                Arg.Any<List<GenericSettingConfiguration>>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult<List<(ETimeType, ZonedDateTime)>>([(ETimeType.FajrStart, callInfo.Arg<ZonedDateTime>().PlusHours(4))]));
        _prayerTimeServiceFactoryMock.GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit)).Returns(muwaqqitPrayerTimeServiceMock);

        _profileServiceMock.GetActiveComplexTimeConfigs(Arg.Is(profile)).Returns([muwaqqitConfig]);

        CalculatePrayerTimesResultVO calculated = await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, zonedDate, default);

        // ACT
        // a different time on the same day must resolve to the same cached (start of day) entry
        bool found = _dynamicPrayerTimeProviderManager.TryGetAlreadyCalculatedPrayerTimes(profile.ID, zonedDate.PlusHours(10), out DynamicPrayerTimesDaySet daySet);

        // ASSERT
        found.Should().BeTrue();
        daySet.Should().BeSameAs(calculated.DynamicPrayerTimesDaySet);

        // reading from the cache must not have triggered any additional provider calls (only the 3 initial ones)
        await muwaqqitPrayerTimeServiceMock.ReceivedWithAnyArgs(3).GetPrayerTimesAsync(default, default, default, default);
    }

    [Fact]
    public async Task TryGetAlreadyCalculatedPrayerTimes_ForOtherDateThanCalculated_ReturnsFalse()
    {
        // ARRANGE
        DynamicProfile profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        ZonedDateTime calculatedDate = new LocalDate(2024, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);
        ZonedDateTime otherDate = new LocalDate(2024, 1, 2).AtStartOfDayInZone(DateTimeZone.Utc);

        BaseLocationData muwaqqitLocationData = Substitute.ForPartsOf<BaseLocationData>();
        _profileServiceMock.GetUntrackedReferenceOfProfile(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(profile);
        _profileServiceMock.GetLocationConfig(Arg.Is(profile), Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit)).Returns(muwaqqitLocationData);

        GenericSettingConfiguration muwaqqitConfig = new MuwaqqitDegreeCalculationConfiguration { Degree = 14, TimeType = ETimeType.FajrStart };
        IDynamicPrayerTimeProvider muwaqqitPrayerTimeServiceMock = Substitute.For<IDynamicPrayerTimeProvider>();
        muwaqqitPrayerTimeServiceMock.GetUnsupportedTimeTypes().Returns([]);
        muwaqqitPrayerTimeServiceMock.GetPrayerTimesAsync(
                Arg.Any<ZonedDateTime>(),
                Arg.Is(muwaqqitLocationData),
                Arg.Any<List<GenericSettingConfiguration>>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => Task.FromResult<List<(ETimeType, ZonedDateTime)>>([(ETimeType.FajrStart, callInfo.Arg<ZonedDateTime>().PlusHours(4))]));
        _prayerTimeServiceFactoryMock.GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit)).Returns(muwaqqitPrayerTimeServiceMock);

        _profileServiceMock.GetActiveComplexTimeConfigs(Arg.Is(profile)).Returns([muwaqqitConfig]);

        await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, calculatedDate, default);

        // ACT
        bool found = _dynamicPrayerTimeProviderManager.TryGetAlreadyCalculatedPrayerTimes(profile.ID, otherDate, out DynamicPrayerTimesDaySet daySet);

        // ASSERT
        found.Should().BeFalse();
        daySet.Should().BeNull();
    }

    [Fact]
    public async Task TryGetAlreadyCalculatedPrayerTimes_AfterPartiallyFailedCalculation_ReturnsPartialDaySet()
    {
        // ARRANGE
        DynamicProfile profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        ZonedDateTime zonedDate = new LocalDate(2024, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);
        SetUpMuwaqqitProvider(profile, shouldFail: () => true);

        CalculatePrayerTimesResultVO calculated = await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, zonedDate, default);

        // ACT
        bool found = _dynamicPrayerTimeProviderManager.TryGetAlreadyCalculatedPrayerTimes(profile.ID, zonedDate, out DynamicPrayerTimesDaySet daySet);

        // ASSERT
        // the times of the working providers are better than nothing for consumers like the notification
        calculated.CalculationErrors.Should().NotBeEmpty();
        found.Should().BeTrue();
        daySet.Should().BeSameAs(calculated.DynamicPrayerTimesDaySet);
    }

    #endregion TryGetAlreadyCalculatedPrayerTimes

    #region cancellation handling

    [Fact]
    public async Task CalculatePrayerTimesAsync_CanceledTokenAndProviderThrowsNonCancellationException_ThrowsOperationCanceledAndDoesNotCache()
    {
        // ARRANGE
        DynamicProfile profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        ZonedDateTime zonedDate = new LocalDate(2024, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);

        BaseLocationData muwaqqitLocationData = Substitute.ForPartsOf<BaseLocationData>();
        _profileServiceMock.GetUntrackedReferenceOfProfile(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(profile);
        _profileServiceMock.GetLocationConfig(Arg.Is(profile), Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit)).Returns(muwaqqitLocationData);

        GenericSettingConfiguration muwaqqitConfig = new MuwaqqitDegreeCalculationConfiguration { Degree = 14, TimeType = ETimeType.FajrStart };
        IDynamicPrayerTimeProvider muwaqqitPrayerTimeServiceMock = Substitute.For<IDynamicPrayerTimeProvider>();
        muwaqqitPrayerTimeServiceMock.GetUnsupportedTimeTypes().Returns([]);

        // a canceled request that bubbles up from the HTTP stack as something other than a plain
        // OperationCanceledException (as Refit / the resilience pipeline can do)
        muwaqqitPrayerTimeServiceMock.GetPrayerTimesAsync(
                Arg.Any<ZonedDateTime>(),
                Arg.Is(muwaqqitLocationData),
                Arg.Any<List<GenericSettingConfiguration>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<(ETimeType, ZonedDateTime)>>(new InvalidOperationException("Canceled")));
        _prayerTimeServiceFactoryMock.GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit)).Returns(muwaqqitPrayerTimeServiceMock);

        _profileServiceMock.GetActiveComplexTimeConfigs(Arg.Is(profile)).Returns([muwaqqitConfig]);

        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // ACT + ASSERT
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, zonedDate, cancellationTokenSource.Token));

        // a canceled calculation must not be cached
        _dynamicPrayerTimeProviderManager.TryGetAlreadyCalculatedPrayerTimes(profile.ID, zonedDate, out _).Should().BeFalse();
    }

    #endregion cancellation handling

    /// <summary>
    /// Sets up a Muwaqqit provider for a single FajrStart config, which either fails or returns
    /// Fajr at 04:00 of the requested day, depending on <paramref name="shouldFail"/> at call time.
    /// </summary>
    private IDynamicPrayerTimeProvider SetUpMuwaqqitProvider(DynamicProfile profile, Func<bool> shouldFail)
    {
        BaseLocationData muwaqqitLocationData = Substitute.ForPartsOf<BaseLocationData>();
        _profileServiceMock.GetUntrackedReferenceOfProfile(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(profile);
        _profileServiceMock.GetLocationConfig(Arg.Is(profile), Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit)).Returns(muwaqqitLocationData);

        GenericSettingConfiguration muwaqqitConfig = new MuwaqqitDegreeCalculationConfiguration { Degree = 14, TimeType = ETimeType.FajrStart };
        IDynamicPrayerTimeProvider muwaqqitPrayerTimeServiceMock = Substitute.For<IDynamicPrayerTimeProvider>();
        muwaqqitPrayerTimeServiceMock.GetUnsupportedTimeTypes().Returns([]);
        muwaqqitPrayerTimeServiceMock.GetPrayerTimesAsync(
                Arg.Any<ZonedDateTime>(),
                Arg.Is(muwaqqitLocationData),
                Arg.Any<List<GenericSettingConfiguration>>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => shouldFail()
                ? Task.FromException<List<(ETimeType, ZonedDateTime)>>(new InvalidOperationException("Socket closed"))
                : Task.FromResult<List<(ETimeType, ZonedDateTime)>>([(ETimeType.FajrStart, callInfo.Arg<ZonedDateTime>().PlusHours(4))]));
        _prayerTimeServiceFactoryMock.GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit)).Returns(muwaqqitPrayerTimeServiceMock);

        _profileServiceMock.GetActiveComplexTimeConfigs(Arg.Is(profile)).Returns([muwaqqitConfig]);

        return muwaqqitPrayerTimeServiceMock;
    }

    private int GetLogCount(LogLevel logLevel)
    {
        return _loggerMock.ReceivedCalls().Count(call =>
            call.GetMethodInfo().Name == nameof(ILogger.Log)
            && call.GetArguments()[0] is LogLevel callLogLevel
            && callLogLevel == logLevel);
    }
}
