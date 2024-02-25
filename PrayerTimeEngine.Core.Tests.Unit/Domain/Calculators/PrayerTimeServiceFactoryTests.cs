using NSubstitute;
using PrayerTimeEngine.Core.Domain.Calculators;
using FluentAssertions;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Services;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Services;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Services;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.Calculators
{
    public class PrayerTimeServiceFactoryTests
    {
        private readonly IServiceProvider serviceProviderMock;
        private readonly PrayerTimeServiceFactory _prayerTimeServiceFactory;

        public PrayerTimeServiceFactoryTests()
        {
            serviceProviderMock = Substitute.For<IServiceProvider>();
            _prayerTimeServiceFactory = new PrayerTimeServiceFactory(serviceProviderMock);
        }

        #region GetPrayerTimeCalculatorByCalculationSource

        [Theory]
        [InlineData(ECalculationSource.Muwaqqit, typeof(MuwaqqitPrayerTimeCalculator))]
        [InlineData(ECalculationSource.Fazilet, typeof(FaziletPrayerTimeCalculator))]
        [InlineData(ECalculationSource.Semerkand, typeof(SemerkandPrayerTimeCalculator))]
        [Trait("Method", "GetPrayerTimeCalculatorByCalculationSource")]
        public void GetPrayerTimeCalculatorByCalculationSource_DifferentValidCalculationSources_RightTypeReturned(
            ECalculationSource calculationSource, Type expectedType)
        {
            // ARRANGE
            Type requestedType = null;
            serviceProviderMock.When(x => x.GetService(Arg.Any<Type>()))
                .Do(x => requestedType = x.Arg<Type>());

            // ACT
            // ignore exception because the service provider has some unmockable things
            // which are not really important for the test anyway so...
            try 
            { 
                _prayerTimeServiceFactory.GetPrayerTimeCalculatorByCalculationSource(calculationSource); 
            }
            catch { }

            // ASSERT
            requestedType.Should().Be(expectedType);
            serviceProviderMock.ReceivedWithAnyArgs(1).GetService(default);
            serviceProviderMock.Received(1).GetService(Arg.Is(expectedType));
        }

        [Fact]
        [Trait("Method", "GetPrayerTimeCalculatorByCalculationSource")]
        public void GetPrayerTimeCalculatorByCalculationSource_CalculationSourceNone_ArgumentException()
        {
            // ARRANGE & ACT
            Action action = () => _prayerTimeServiceFactory.GetPrayerTimeCalculatorByCalculationSource(ECalculationSource.None);

            // ASSERT
            action.Should().Throw<ArgumentException>();
        }

        #endregion GetPrayerTimeCalculatorByCalculationSource
    }
}
