using Microsoft.Extensions.Logging;
using NSubstitute;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Services;
using PrayerTimeEngine.Core.Domain.PlaceManagement.Services;
using PrayerTimeEngine.Core.Tests.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.Calculators.Semerkand
{
    public class SemerkandApiServiceTests
    {
        private readonly MockHttpMessageHandler _mockHttpMessageHandler;
        private readonly SemerkandApiService _semerkandApiService;

        public SemerkandApiServiceTests()
        {
            _mockHttpMessageHandler = new MockHttpMessageHandler();
            HttpClient httpClient = new HttpClient(_mockHttpMessageHandler);
            _semerkandApiService = new SemerkandApiService(httpClient);
        }

        [Fact]
        public void X_X_X()
        {
            //_semerkandApiService.GetTimesByCityID();
            //_semerkandApiService.GetCitiesByCountryID();
            //_semerkandApiService.GetCountries();
        }
    }
}
