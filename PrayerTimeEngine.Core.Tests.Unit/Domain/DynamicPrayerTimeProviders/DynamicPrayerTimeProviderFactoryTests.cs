using NSubstitute;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Fazilet.Services;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Muwaqqit.Services;
using PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Services;

namespace PrayerTimeEngine.Core.Tests.Unit.Domain.DynamicPrayerTimeProviders;

public class DynamicPrayerTimeProviderFactoryTests
{
    private readonly IServiceProvider serviceProviderMock;
    private readonly DynamicPrayerTimeProviderFactory _dynamicPrayerTimeProviderFactory;

    public DynamicPrayerTimeProviderFactoryTests()
    {
        serviceProviderMock = Substitute.For<IServiceProvider>();
        _dynamicPrayerTimeProviderFactory = new DynamicPrayerTimeProviderFactory(serviceProviderMock);
    }

    #region GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider

    [Theory]
    [InlineData(EDynamicPrayerTimeProviderType.Muwaqqit, typeof(MuwaqqitDynamicPrayerTimeProvider))]
    [InlineData(EDynamicPrayerTimeProviderType.Fazilet, typeof(FaziletDynamicPrayerTimeProvider))]
    [InlineData(EDynamicPrayerTimeProviderType.Semerkand, typeof(SemerkandDynamicPrayerTimeProvider))]
    [Trait("Method", "GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider")]
    public void GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider_DifferentValidDynamicPrayerTimeProviders_RightTypeReturned(
        EDynamicPrayerTimeProviderType dynamicPrayerTimeProviderType, Type expectedType)
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
            _dynamicPrayerTimeProviderFactory.GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(dynamicPrayerTimeProviderType);
        }
        catch { }

        // ASSERT
        requestedType.Should().Be(expectedType);
        serviceProviderMock.ReceivedWithAnyArgs(1).GetService(default);
        serviceProviderMock.Received(1).GetService(Arg.Is(expectedType));
    }

    [Fact]
    [Trait("Method", "GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider")]
    public void GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider_DynamicPrayerTimeProviderNone_ArgumentException()
    {
        // ARRANGE & ACT
        Action action = () => _dynamicPrayerTimeProviderFactory.GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider(EDynamicPrayerTimeProviderType.None);

        // ASSERT
        action.Should().Throw<ArgumentException>();
    }

    #endregion GetDynamicPrayerTimeProviderByDynamicPrayerTimeProvider
}
