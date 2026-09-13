using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
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
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.DynamicPrayerTimes.Management;

public class DynamicPrayerTimeProviderManagerTests : BaseTest
{
    private readonly IDynamicPrayerTimeProviderFactory _prayerTimeServiceFactoryMock;
    private readonly IProfileService _profileServiceMock;
    private readonly DynamicPrayerTimeProviderManager _dynamicPrayerTimeProviderManager;

    public DynamicPrayerTimeProviderManagerTests()
    {
        _prayerTimeServiceFactoryMock = Substitute.For<IDynamicPrayerTimeProviderFactory>();
        _profileServiceMock = Substitute.For<IProfileService>();
        _dynamicPrayerTimeProviderManager = new DynamicPrayerTimeProviderManager(
            _prayerTimeServiceFactoryMock,
            _profileServiceMock,
            Substitute.For<ISystemInfoService>(),
            Substitute.For<ILogger<DynamicPrayerTimeProviderManager>>(),
            [Substitute.For<IPrayerTimeCacheCleaner>()]);
    }

    #region CalculatePrayerTimesAsync

    [Fact]
    //[Trait("Method", "CalculatePrayerTimesAsync")]
    public async Task CalculatePrayerTimesAsync_OneComplexCalculation_CalculatedSuccessfully()
    {
        // ARRANGE
        var profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        ZonedDateTime zonedDate = new LocalDate(2024, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);
        ZonedDateTime zonedDateOneDayBefore = zonedDate.Plus(Duration.FromDays(-1));
        ZonedDateTime zonedDateOneDayAfter = zonedDate.Plus(Duration.FromDays(1));

        var muwaqqitLocationData = Substitute.ForPartsOf<BaseLocationData>();
        _profileServiceMock.GetUntrackedReferenceOfProfile(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(profile);
        _profileServiceMock.GetLocationConfig(Arg.Is(profile), Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit)).Returns(muwaqqitLocationData);

        GenericSettingConfiguration muwaqqitConfig = new MuwaqqitDegreeCalculationConfiguration { Degree = 14, TimeType = ETimeType.FajrStart };
        var muwaqqitPrayerTimeServiceMock = Substitute.For<IDynamicPrayerTimeProvider>();

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
                var dateInput = callInfo.Arg<ZonedDateTime>();
                if (dateInput == zonedDateOneDayBefore)
                    return Task.FromResult(muwaqqitReturnValuePreviousDay);
                else if (dateInput == zonedDate)
                    return Task.FromResult(muwaqqitReturnValueCurrentDay);
                else if (dateInput == zonedDateOneDayAfter)
                    return Task.FromResult(muwaqqitReturnValueNextDay);

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
        var profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        ZonedDateTime zonedDate = new LocalDate(2024, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);

        var muwaqqitLocationData = Substitute.ForPartsOf<BaseLocationData>();
        _profileServiceMock.GetUntrackedReferenceOfProfile(
                Arg.Any<int>(), 
                Arg.Any<CancellationToken>())
            .Returns(profile);
        _profileServiceMock.GetLocationConfig(
                Arg.Is(profile), 
                Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit))
            .Returns(muwaqqitLocationData);

        GenericSettingConfiguration muwaqqitConfig = new MuwaqqitDegreeCalculationConfiguration { Degree = 14, TimeType = ETimeType.FajrStart };
        var muwaqqitPrayerTimeServiceMock = Substitute.For<IDynamicPrayerTimeProvider>();

        _prayerTimeServiceFactoryMock
            .GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit))
            .Returns(muwaqqitPrayerTimeServiceMock);

        InvalidOperationException expextedException = new InvalidOperationException("calculator failed");
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
        var profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        ZonedDateTime zonedDate = new LocalDate(2024, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);

        _profileServiceMock.GetUntrackedReferenceOfProfile(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(profile);

        // failing Muwaqqit provider
        var muwaqqitLocationData = Substitute.ForPartsOf<BaseLocationData>();
        _profileServiceMock.GetLocationConfig(Arg.Is(profile), Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit)).Returns(muwaqqitLocationData);
        GenericSettingConfiguration muwaqqitConfig = new MuwaqqitDegreeCalculationConfiguration { Degree = 14, TimeType = ETimeType.FajrStart };
        var muwaqqitPrayerTimeServiceMock = Substitute.For<IDynamicPrayerTimeProvider>();
        muwaqqitPrayerTimeServiceMock.GetUnsupportedTimeTypes().Returns([]);
        muwaqqitPrayerTimeServiceMock.GetPrayerTimesAsync(
                Arg.Any<ZonedDateTime>(),
                Arg.Is(muwaqqitLocationData),
                Arg.Any<List<GenericSettingConfiguration>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<(ETimeType, ZonedDateTime)>>(new InvalidOperationException("muwaqqit failed")));
        _prayerTimeServiceFactoryMock.GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit)).Returns(muwaqqitPrayerTimeServiceMock);

        // succeeding Fazilet provider
        var faziletLocationData = Substitute.ForPartsOf<BaseLocationData>();
        _profileServiceMock.GetLocationConfig(Arg.Is(profile), Arg.Is(EDynamicPrayerTimeProviderType.Fazilet)).Returns(faziletLocationData);
        GenericSettingConfiguration faziletConfig = new() { TimeType = ETimeType.DhuhrStart, Source = EDynamicPrayerTimeProviderType.Fazilet };
        var faziletPrayerTimeServiceMock = Substitute.For<IDynamicPrayerTimeProvider>();
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
        var profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        ZonedDateTime zonedDate = new LocalDate(2024, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);

        _profileServiceMock.GetUntrackedReferenceOfProfile(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(profile);

        // Muwaqqit without location data
        _profileServiceMock.GetLocationConfig(Arg.Is(profile), Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit)).Returns((BaseLocationData)null);
        GenericSettingConfiguration muwaqqitConfig = new MuwaqqitDegreeCalculationConfiguration { Degree = 14, TimeType = ETimeType.FajrStart };

        // Fazilet with location data
        var faziletLocationData = Substitute.ForPartsOf<BaseLocationData>();
        _profileServiceMock.GetLocationConfig(Arg.Is(profile), Arg.Is(EDynamicPrayerTimeProviderType.Fazilet)).Returns(faziletLocationData);
        GenericSettingConfiguration faziletConfig = new() { TimeType = ETimeType.DhuhrStart, Source = EDynamicPrayerTimeProviderType.Fazilet };
        var faziletPrayerTimeServiceMock = Substitute.For<IDynamicPrayerTimeProvider>();
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
        var profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        ZonedDateTime zonedDate = new LocalDate(2024, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);

        _profileServiceMock.GetUntrackedReferenceOfProfile(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(profile);

        // Muwaqqit with a config whose time type it doesn't support
        var muwaqqitLocationData = Substitute.ForPartsOf<BaseLocationData>();
        _profileServiceMock.GetLocationConfig(Arg.Is(profile), Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit)).Returns(muwaqqitLocationData);
        GenericSettingConfiguration muwaqqitConfig = new MuwaqqitDegreeCalculationConfiguration { Degree = 14, TimeType = ETimeType.FajrStart };
        var muwaqqitPrayerTimeServiceMock = Substitute.For<IDynamicPrayerTimeProvider>();
        muwaqqitPrayerTimeServiceMock.GetUnsupportedTimeTypes().Returns([ETimeType.FajrStart]);
        _prayerTimeServiceFactoryMock.GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit)).Returns(muwaqqitPrayerTimeServiceMock);

        // valid Fazilet provider
        var faziletLocationData = Substitute.ForPartsOf<BaseLocationData>();
        _profileServiceMock.GetLocationConfig(Arg.Is(profile), Arg.Is(EDynamicPrayerTimeProviderType.Fazilet)).Returns(faziletLocationData);
        GenericSettingConfiguration faziletConfig = new() { TimeType = ETimeType.DhuhrStart, Source = EDynamicPrayerTimeProviderType.Fazilet };
        var faziletPrayerTimeServiceMock = Substitute.For<IDynamicPrayerTimeProvider>();
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
        var profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        ZonedDateTime zonedDate = new LocalDate(2024, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);

        var muwaqqitLocationData = Substitute.ForPartsOf<BaseLocationData>();
        _profileServiceMock.GetUntrackedReferenceOfProfile(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(profile);
        _profileServiceMock.GetLocationConfig(Arg.Is(profile), Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit)).Returns(muwaqqitLocationData);

        GenericSettingConfiguration muwaqqitConfig = new MuwaqqitDegreeCalculationConfiguration { Degree = 14, TimeType = ETimeType.FajrStart };
        var muwaqqitPrayerTimeServiceMock = Substitute.For<IDynamicPrayerTimeProvider>();
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
    public async Task CalculatePrayerTimesAsync_FailedCalculation_SecondCallRecalculates()
    {
        // ARRANGE
        var profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        ZonedDateTime zonedDate = new LocalDate(2024, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);

        var muwaqqitLocationData = Substitute.ForPartsOf<BaseLocationData>();
        _profileServiceMock.GetUntrackedReferenceOfProfile(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(profile);
        _profileServiceMock.GetLocationConfig(Arg.Is(profile), Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit)).Returns(muwaqqitLocationData);

        GenericSettingConfiguration muwaqqitConfig = new MuwaqqitDegreeCalculationConfiguration { Degree = 14, TimeType = ETimeType.FajrStart };
        var muwaqqitPrayerTimeServiceMock = Substitute.For<IDynamicPrayerTimeProvider>();
        muwaqqitPrayerTimeServiceMock.GetUnsupportedTimeTypes().Returns([]);
        muwaqqitPrayerTimeServiceMock.GetPrayerTimesAsync(
                Arg.Any<ZonedDateTime>(),
                Arg.Is(muwaqqitLocationData),
                Arg.Any<List<GenericSettingConfiguration>>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.FromException<List<(ETimeType, ZonedDateTime)>>(new InvalidOperationException("calculator failed")));
        _prayerTimeServiceFactoryMock.GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit)).Returns(muwaqqitPrayerTimeServiceMock);

        _profileServiceMock.GetActiveComplexTimeConfigs(Arg.Is(profile)).Returns([muwaqqitConfig]);

        // ACT
        CalculatePrayerTimesResultVO firstResult = await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, zonedDate, default);
        CalculatePrayerTimesResultVO secondResult = await _dynamicPrayerTimeProviderManager.CalculatePrayerTimesAsync(profile.ID, zonedDate, default);

        // ASSERT
        firstResult.CalculationErrors.Should().HaveCount(3);
        secondResult.CalculationErrors.Should().HaveCount(3);

        // erroneous results must not be cached, so both calls have to actually calculate (3 calls each)
        await muwaqqitPrayerTimeServiceMock.ReceivedWithAnyArgs(6).GetPrayerTimesAsync(default, default, default, default);
    }

    [Fact]
    public async Task CalculatePrayerTimesAsync_RequestForOtherDate_OnlyLatestCalculationCached()
    {
        // ARRANGE
        var profile = TestDataHelper.CreateCompleteTestDynamicProfile();
        ZonedDateTime firstZonedDate = new LocalDate(2024, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);
        ZonedDateTime secondZonedDate = new LocalDate(2024, 1, 5).AtStartOfDayInZone(DateTimeZone.Utc);

        var muwaqqitLocationData = Substitute.ForPartsOf<BaseLocationData>();
        _profileServiceMock.GetUntrackedReferenceOfProfile(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(profile);
        _profileServiceMock.GetLocationConfig(Arg.Is(profile), Arg.Is(EDynamicPrayerTimeProviderType.Muwaqqit)).Returns(muwaqqitLocationData);

        GenericSettingConfiguration muwaqqitConfig = new MuwaqqitDegreeCalculationConfiguration { Degree = 14, TimeType = ETimeType.FajrStart };
        var muwaqqitPrayerTimeServiceMock = Substitute.For<IDynamicPrayerTimeProvider>();
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
}
