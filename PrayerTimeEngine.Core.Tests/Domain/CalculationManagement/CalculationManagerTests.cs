using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Domain.CalculationManagement;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using NSubstitute;
using PrayerTimeEngine.Core.Domain.Calculators;
using NodaTime;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models;
using PrayerTimeEngine.Core.Domain.Models;
using FluentAssertions;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.CalculationManagement
{
    public class CalculationManagerTests : BaseTest
    {
        private readonly IPrayerTimeServiceFactory _prayerTimeServiceFactoryMock;
        private readonly IProfileService _profileServiceMock;
        private readonly CalculationManager _calculationManager;

        public CalculationManagerTests()
        {
            _prayerTimeServiceFactoryMock = Substitute.For<IPrayerTimeServiceFactory>();
            _profileServiceMock = Substitute.For<IProfileService>();
            _calculationManager = new CalculationManager(_prayerTimeServiceFactoryMock, _profileServiceMock);
        }

        #region CalculatePrayerTimesAsync

        [Fact]
        //[Trait("Method", "CalculatePrayerTimesAsync")]
        public async Task CalculatePrayerTimesAsync_OneComplexCalculation_CalculatedSuccessfully()
        {
            // ARRANGE
            Profile profile = TestData.CreateNewCompleteTestProfile();
            ZonedDateTime zonedDate = new LocalDate(2024, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);

            var muwaqqitLocationData = Substitute.ForPartsOf<BaseLocationData>();
            _profileServiceMock.GetUntrackedReferenceOfProfile(Arg.Any<int>()).Returns(profile);
            _profileServiceMock.GetLocationConfig(Arg.Is(profile), Arg.Is(ECalculationSource.Muwaqqit)).Returns(muwaqqitLocationData);

            GenericSettingConfiguration muwaqqitConfig = new MuwaqqitDegreeCalculationConfiguration { Degree = 14, TimeType = ETimeType.FajrStart };
            var muwaqqitPrayerTimeServiceMock = Substitute.For<IPrayerTimeService>();
            var muwaqqitCalculation = Substitute.For<ICalculationPrayerTimes>();

            _prayerTimeServiceFactoryMock.GetPrayerTimeCalculatorByCalculationSource(Arg.Is(ECalculationSource.Muwaqqit)).Returns(muwaqqitPrayerTimeServiceMock);

            var muwaqqitReturnValue = new[] { muwaqqitCalculation }.ToLookup(x => x, x => ETimeType.FajrStart);
            muwaqqitPrayerTimeServiceMock.GetPrayerTimesAsync(
                    Arg.Is(zonedDate.Date), 
                    Arg.Is(muwaqqitLocationData), 
                    Arg.Is<List<GenericSettingConfiguration>>(x => x.Contains(muwaqqitConfig)))
                .Returns(Task.FromResult(muwaqqitReturnValue));

            _profileServiceMock.GetActiveComplexTimeConfigs(Arg.Is(profile)).Returns([muwaqqitConfig]);
            muwaqqitPrayerTimeServiceMock.GetUnsupportedTimeTypes().Returns([]);
            muwaqqitCalculation.GetZonedDateTimeForTimeType(Arg.Is(ETimeType.FajrStart)).Returns(zonedDate.PlusHours(4));

            // ACT
            PrayerTimesBundle result = await _calculationManager.CalculatePrayerTimesAsync(profile.ID, zonedDate);

            // ASSERT
            result.Should().NotBeNull();

            result.Fajr.Start.Should().Be(zonedDate.PlusHours(4));

            muwaqqitPrayerTimeServiceMock.Awaiting(x => x.ReceivedWithAnyArgs(1).GetPrayerTimesAsync(default, default, default));
            muwaqqitPrayerTimeServiceMock
                .Awaiting(x => x.Received(1).GetPrayerTimesAsync(
                    Arg.Is(zonedDate.Date), 
                    Arg.Is(muwaqqitLocationData), 
                    Arg.Is<List<GenericSettingConfiguration>>(x => x.Contains(muwaqqitConfig))));
        }

        #endregion CalculatePrayerTimesAsync
    }
}