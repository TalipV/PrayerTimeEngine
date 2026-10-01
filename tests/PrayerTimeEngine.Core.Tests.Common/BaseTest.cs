using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.Core;
using NSubstitute.Extensions;
using PrayerTimeEngine.Core.Common;
using PrayerTimeEngine.Core.Data.EntityFramework;

namespace PrayerTimeEngine.Core.Tests.Common;

public abstract class BaseTest : IDisposable
{
    private readonly Guid _testSessionID = Guid.NewGuid();

    private IDbContextFactory<AppDbContext> _dbContextFactoryMock = null;

    private AppDbContext _keepMemoryDbAliveDbContext = null;
    private DbConnection _keepMemoryDbAliveDbConnection = null;

    protected AppDbContext TestArrangeDbContext { get; private set; }
    protected AppDbContext TestAssertDbContext { get; private set; }

    protected static ServiceProvider CreateServiceProvider(Action<ServiceCollection> configureServiceCollection)
    {
        var serviceCollection = new ServiceCollection();
        configureServiceCollection(serviceCollection);
        return serviceCollection.BuildServiceProvider();
    }

    private IDbContextFactory<AppDbContext> CreateTestAppDbContextFactory()
    {
        IDbContextFactory<AppDbContext> dbContextFactoryMock = Substitute.For<IDbContextFactory<AppDbContext>>();

        dbContextFactoryMock.CreateDbContext().Returns(GetMockableAppDbContext);
        dbContextFactoryMock.CreateDbContextAsync().Returns(callInfo => Task.FromResult(GetMockableAppDbContext(callInfo)));

        return dbContextFactoryMock;
    }

    private AppDbContext GetMockableAppDbContext(CallInfo callInfo)
    {
        // included Guid to be double safe from one test affecting another
        DbContextOptions options =
            new DbContextOptionsBuilder()
                .UseSqlite($"Data Source=TestDb{_testSessionID:N};Mode=Memory;Cache=Shared")
                .Options;

        AppDbContext mockableDbContext =
            Substitute.ForPartsOf<AppDbContext>(
                options,
                new AppDbContextMetaData(),
                Substitute.For<ISystemInfoService>());

        DatabaseFacade mockableDbContextDatabase = Substitute.ForPartsOf<DatabaseFacade>(mockableDbContext);
        mockableDbContext.Configure().Database.Returns(mockableDbContextDatabase);

        return mockableDbContext;
    }

    protected IDbContextFactory<AppDbContext> GetHandledDbContextFactory()
    {
        if (_dbContextFactoryMock != null)
        {
            return _dbContextFactoryMock;
        }

        _dbContextFactoryMock = CreateTestAppDbContextFactory();
        _keepMemoryDbAliveDbContext = _dbContextFactoryMock.CreateDbContext();
        TestArrangeDbContext = _dbContextFactoryMock.CreateDbContext();
        TestAssertDbContext = _dbContextFactoryMock.CreateDbContext();

        DatabaseFacade database = _keepMemoryDbAliveDbContext.Database;
        _keepMemoryDbAliveDbConnection = database.GetDbConnection();
        _keepMemoryDbAliveDbConnection.Open();
        database.EnsureCreated();

        return _dbContextFactoryMock;
    }

    public void Dispose()
    {
        _keepMemoryDbAliveDbConnection?.Dispose();
        _keepMemoryDbAliveDbConnection = null;

        _keepMemoryDbAliveDbContext?.Dispose();
        _keepMemoryDbAliveDbContext = null;

        TestArrangeDbContext?.Dispose();
        TestArrangeDbContext = null;

        TestAssertDbContext?.Dispose();
        TestAssertDbContext = null;

        GC.SuppressFinalize(this);
    }
}
