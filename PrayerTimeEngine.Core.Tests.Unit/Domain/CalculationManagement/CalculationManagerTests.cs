using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Domain.CalculationManagement;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Interfaces;
using NSubstitute;
using PrayerTimeEngine.Core.Domain.Calculators;
using NodaTime;
using PrayerTimeEngine.Core.Domain.Models;
using FluentAssertions;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;
using Microsoft.Extensions.Logging;
using PrayerTimeEngine.Core.Tests.Common.TestData;
using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.CalculationManagement
{
    public class CalculationManagerTests : BaseTest
    {
        private readonly IPrayerTimeCalculatorFactory _prayerTimeServiceFactoryMock;
        private readonly IProfileService _profileServiceMock;
        private readonly CalculationManager _calculationManager;

        public CalculationManagerTests()
        {
            _prayerTimeServiceFactoryMock = Substitute.For<IPrayerTimeCalculatorFactory>();
            _profileServiceMock = Substitute.For<IProfileService>();
            _calculationManager = new CalculationManager(_prayerTimeServiceFactoryMock, _profileServiceMock, Substitute.For<ILogger<CalculationManager>>());
        }

        #region CalculatePrayerTimesAsync

        [Fact]
        //[Trait("Method", "CalculatePrayerTimesAsync")]
        public async Task CalculatePrayerTimesAsync_OneComplexCalculation_CalculatedSuccessfully()
        {
            // ARRANGE
            Profile profile = TestDataHelper.CreateNewCompleteTestProfile();
            ZonedDateTime zonedDate = new LocalDate(2024, 1, 1).AtStartOfDayInZone(DateTimeZone.Utc);

            var muwaqqitLocationData = Substitute.ForPartsOf<BaseLocationData>();
            _profileServiceMock.GetUntrackedReferenceOfProfile(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(profile);
            _profileServiceMock.GetLocationConfig(Arg.Is(profile), Arg.Is(ECalculationSource.Muwaqqit)).Returns(muwaqqitLocationData);

            GenericSettingConfiguration muwaqqitConfig = new MuwaqqitDegreeCalculationConfiguration { Degree = 14, TimeType = ETimeType.FajrStart };
            var muwaqqitPrayerTimeServiceMock = Substitute.For<IPrayerTimeCalculator>();

            _prayerTimeServiceFactoryMock.GetPrayerTimeCalculatorByCalculationSource(Arg.Is(ECalculationSource.Muwaqqit)).Returns(muwaqqitPrayerTimeServiceMock);

            List<(ETimeType, ZonedDateTime)> muwaqqitReturnValue =
                [
                    (ETimeType.FajrStart, zonedDate.PlusHours(4)),
                ];

            muwaqqitPrayerTimeServiceMock.GetPrayerTimesAsync(
                    Arg.Is(zonedDate.Date),
                    Arg.Is(muwaqqitLocationData),
                    Arg.Is<List<GenericSettingConfiguration>>(x => x.Contains(muwaqqitConfig)),
                    Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(muwaqqitReturnValue));

            _profileServiceMock.GetActiveComplexTimeConfigs(Arg.Is(profile)).Returns([muwaqqitConfig]);
            muwaqqitPrayerTimeServiceMock.GetUnsupportedTimeTypes().Returns([]);

            // ACT
            PrayerTimesBundle result = await _calculationManager.CalculatePrayerTimesAsync(profile.ID, zonedDate, default);

            // ASSERT
            result.Should().NotBeNull();

            result.Fajr.Start.Should().Be(zonedDate.PlusHours(4));

            muwaqqitPrayerTimeServiceMock.Awaiting(x => x.ReceivedWithAnyArgs(1).GetPrayerTimesAsync(default, default, default, default));
            muwaqqitPrayerTimeServiceMock
                .Awaiting(x => x.Received(1).GetPrayerTimesAsync(
                    Arg.Is(zonedDate.Date),
                    Arg.Is(muwaqqitLocationData),
                    Arg.Is<List<GenericSettingConfiguration>>(x => x.Contains(muwaqqitConfig)), 
                    Arg.Any<CancellationToken>()));
        }

        #endregion CalculatePrayerTimesAsync
    }
}