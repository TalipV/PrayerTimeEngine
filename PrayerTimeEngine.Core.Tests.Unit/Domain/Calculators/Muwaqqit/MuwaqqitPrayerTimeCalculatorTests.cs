using NSubstitute;
using PrayerTimeEngine.Core.Domain;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Services;
using PrayerTimeEngine.Core.Tests.Common;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.Calculators.Muwaqqit
{
    public class MuwaqqitPrayerTimeCalculatorTests : BaseTest
    {
        private readonly IMuwaqqitDBAccess _muwaqqitDBAccessMock;
        private readonly IMuwaqqitApiService _muwaqqitApiServiceMock;
        private readonly MuwaqqitPrayerTimeCalculator _muwaqqitPrayerTimeCalculator;

        public MuwaqqitPrayerTimeCalculatorTests()
        {
            _muwaqqitDBAccessMock = Substitute.For<IMuwaqqitDBAccess>();
            _muwaqqitApiServiceMock = Substitute.For<IMuwaqqitApiService>();

            _muwaqqitPrayerTimeCalculator =
                new MuwaqqitPrayerTimeCalculator(
                    _muwaqqitDBAccessMock,
                    _muwaqqitApiServiceMock,
                    new TimeTypeAttributeService());
        }

        [Fact]
        public void GetPrayerTimesAsync_X_X()
        {
            throw new NotImplementedException();
        }
    }
}
