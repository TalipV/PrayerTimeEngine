using NodaTime;
using NSubstitute;
using PrayerTimeEngine.Core.Domain.Calculators.Mosques;
using PrayerTimeEngine.Core.Domain.Calculators.Mosques.MyMosq.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Mosques.MyMosq.Models.Entities;
using PrayerTimeEngine.Core.Domain.Calculators.Mosques.MyMosq.Services;
using PrayerTimeEngine.Core.Tests.Common;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.Calculators.MyMosq
{
    public class MyMosqPrayerTimeCalculatorTests : BaseTest
    {
        private readonly IMyMosqDBAccess _myMosqDBAccessMock;
        private readonly IMyMosqApiService _myMosqApiServiceMock;
        private readonly MyMosqPrayerTimeService _myMosqPrayerTimeService;

        public MyMosqPrayerTimeCalculatorTests()
        {
            _myMosqDBAccessMock = Substitute.For<IMyMosqDBAccess>();
            _myMosqApiServiceMock = Substitute.For<IMyMosqApiService>();

            _myMosqPrayerTimeService =
                new MyMosqPrayerTimeService(
                    _myMosqDBAccessMock,
                    _myMosqApiServiceMock);
        }

        #region GetPrayerTimesAsync

        [Fact]
        public async Task GetPrayerTimesAsync_AllValuesFromDbCache_Success()
        {
            // ARRANGE
            var date = new LocalDate(2024, 8, 30);
            string externalID = "1239";

            var times = new MyMosqPrayerTimes
            {
                ID = 0,
                Date = new LocalDate(2024, 8, 29),
                ExternalID = externalID,
                Fajr = new LocalTime(05, 05, 00),
                FajrCongregation = new LocalTime(05, 35, 00),
                Shuruq = new LocalTime(06, 35, 00),
                Dhuhr = new LocalTime(13, 35, 00),
                DhuhrCongregation = new LocalTime(13, 45, 00),
                Asr = new LocalTime(17, 22, 00),
                AsrCongregation = new LocalTime(17, 32, 00),
                Maghrib = new LocalTime(20, 30, 00),
                MaghribCongregation = new LocalTime(20, 35, 00),
                Isha = new LocalTime(22, 06, 00),
                IshaCongregation = new LocalTime(22, 16, 00),
                Jumuah = new LocalTime(14, 30, 00),
                Jumuah2 = new LocalTime(15, 30, 00),
            };

            _myMosqDBAccessMock.GetPrayerTimesAsync(
                Arg.Is(date),
                Arg.Is(externalID),
                Arg.Any<CancellationToken>())
                .Returns(times);

            // ACT
            IMosquePrayerTimes calculationResult = await _myMosqPrayerTimeService.GetPrayerTimesAsync(date, externalID, default);

            // ASSERT
            calculationResult.Should().NotBeNull();
            calculationResult.Should().BeEquivalentTo(times);

            _myMosqApiServiceMock.ReceivedCalls().Should().BeEmpty();
            _myMosqDBAccessMock.ReceivedCalls().Should().HaveCount(1);
            await _myMosqDBAccessMock.Received(1).GetPrayerTimesAsync(Arg.Is(date), Arg.Is(externalID), Arg.Any<CancellationToken>());
        }

        #endregion GetPrayerTimesAsync
    }
}
