using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.Extensions;
using PrayerTimeEngine.Core.Data.EntityFramework;
using System.Data.Common;

namespace PrayerTimeEngine.Core.Tests.Common
{
    public abstract class BaseTest : IDisposable
    {
        public static readonly string TEST_DATA_FILE_PATH = Path.Combine(Directory.GetCurrentDirectory(), "TestData");

        private AppDbContext _testAppDbContext;
        private DbConnection _keepMemoryDbAliveDbConnection = null;

        protected static ServiceProvider createServiceProvider(Action<ServiceCollection> configureServiceCollection)
        {
            var serviceCollection = new ServiceCollection();
            configureServiceCollection(serviceCollection);
            return serviceCollection.BuildServiceProvider();
        }

        private static AppDbContext createTestAppDbContext()
        {
            var dbOptions = new DbContextOptionsBuilder()
                .UseSqlite($"Data Source=:memory:")
                .Options;

            var mockableDbContext = Substitute.ForPartsOf<AppDbContext>(dbOptions);
            var mockableDbContextDatabase = Substitute.ForPartsOf<DatabaseFacade>(mockableDbContext);
            mockableDbContext.Configure().Database.Returns(mockableDbContextDatabase);

            return mockableDbContext;
        }

        protected AppDbContext GetHandledDbContext()
        {
            _testAppDbContext = createTestAppDbContext();
            var database = _testAppDbContext.Database;
            _keepMemoryDbAliveDbConnection = database.GetDbConnection();
            _keepMemoryDbAliveDbConnection.Open();
            database.EnsureCreated();
            
            return _testAppDbContext;
        }

        public void Dispose()
        {
            _keepMemoryDbAliveDbConnection?.Dispose();
            _keepMemoryDbAliveDbConnection = null;

            _testAppDbContext?.Dispose();
            _testAppDbContext = null;
        }
    }
}
