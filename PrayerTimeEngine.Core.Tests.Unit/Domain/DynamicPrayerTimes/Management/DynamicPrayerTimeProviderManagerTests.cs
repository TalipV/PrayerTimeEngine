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
        var profile = TestDataHelper.CreateNewCompleteTestProfile();
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

    #endregion CalculatePrayerTimesAsync
}