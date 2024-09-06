using NodaTime;
using PrayerTimeEngine.Core.Tests.Common;
using PrayerTimeEngine.Core.Tests.Common.TestData;
using System.Net;
using PrayerTimeEngine.Core.Domain.Calculators.Mosques.Mawaqit.Services;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Mosques.Mawaqit.Models.Entities;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.DynamicPrayerTimeProviders.Mawaqit
{
    public class MawaqitApiServiceTests : BaseTest
    {
        private readonly MockHttpMessageHandler _mockHttpMessageHandler;
        private readonly MawaqitApiService _mawaqitApiService;

        public MawaqitApiServiceTests()
        {
            _mockHttpMessageHandler = new MockHttpMessageHandler();
            var httpClient = new HttpClient(_mockHttpMessageHandler)
            {
                BaseAddress = new Uri("https://mawaqit.net/fr")
            };
            _mawaqitApiService = new MawaqitApiService(httpClient);
        }

        [Fact]
        public async Task GetPrayerTimesAsync_IrrelevantInput_MockedHttpResults()
        {
            // ARRANGE
            var date = new LocalDate(2024, 8, 29);
            string externalID = "hamza-koln";

            _mockHttpMessageHandler.HandleRequestFunc =
                (request) =>
                {
                    Stream responseStream = File.OpenRead(Path.Combine(TestDataHelper.MAWAQIT_TEST_DATA_FILE_PATH, "Mawaqit_ResponsePageContent_20240829_hamza-koln.txt"));

                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StreamContent(responseStream)
                    };
                };

            // ACT
            var response = await _mawaqitApiService.GetPrayerTimesAsync(externalID, cancellationToken: default);
            var times = response.ToMawaqitPrayerTimes(externalID).ToList();
            MawaqitPrayerTimes time = times.FirstOrDefault(x => x.Date == date);

            // ASSERT
            time.Should().NotBeNull();

            time.ID.Should().Be(0);
            time.Date.Should().Be(new LocalDate(2024, 8, 29));
            time.ExternalID.Should().Be(externalID);
            time.InsertInstant.Should().BeNull();

            time.Fajr.Should().Be(new LocalTime(05, 05, 00));
            time.FajrCongregation.Should().Be(new LocalTime(05, 35, 00));
            time.Shuruq.Should().Be(new LocalTime(06, 35, 00));
            time.Dhuhr.Should().Be(new LocalTime(13, 35, 00));
            time.DhuhrCongregation.Should().Be(new LocalTime(13, 45, 00));
            time.Asr.Should().Be(new LocalTime(17, 22, 00));
            time.AsrCongregation.Should().Be(new LocalTime(17, 32, 00));
            time.Maghrib.Should().Be(new LocalTime(20, 30, 00));
            time.MaghribCongregation.Should().Be(new LocalTime(20, 35, 00));
            time.Isha.Should().Be(new LocalTime(22, 06, 00));
            time.IshaCongregation.Should().Be(new LocalTime(22, 16, 00));

            time.Jumuah.Should().Be(new LocalTime(14, 30, 00));
            time.Jumuah2.Should().BeNull();
        }
    }
}
