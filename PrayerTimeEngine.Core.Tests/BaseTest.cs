using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.Extensions;
using PrayerTimeEngine.Core.Data.EntityFramework;
using System.Data.Common;

namespace PrayerTimeEngine.Core.Tests
{
    public abstract class BaseTest
    {
        private DbConnection _keepMemoryDbAliveDbConnection = null;
        public ServiceProvider ServiceProvider { get; set; }

        [SetUp]
        public void SetUp()
        {
            var serviceCollection = new ServiceCollection();
            addMockableDbInstanceToServiceCollection(serviceCollection);
            ConfigureServiceProvider(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();

            var database = ServiceProvider.GetService<AppDbContext>().Database;
            _keepMemoryDbAliveDbConnection = database.GetDbConnection();
            _keepMemoryDbAliveDbConnection.Open();
            database.EnsureCreated();
        }

        protected virtual void ConfigureServiceProvider(ServiceCollection serviceCollection) { }

        [TearDown]
        public void TearDown() 
        {
            _keepMemoryDbAliveDbConnection?.Dispose();
            _keepMemoryDbAliveDbConnection = null;

            ServiceProvider?.Dispose();
            ServiceProvider = null;
        }

        private void addMockableDbInstanceToServiceCollection(ServiceCollection serviceCollection) 
        {
            var dbOptions = new DbContextOptionsBuilder()
                .UseSqlite("Data Source=:memory:")
                //.LogTo(message => Debug.WriteLine(message), minimumLevel: LogLevel.Information)
                .Options;

            var mockableDbContext = Substitute.ForPartsOf<AppDbContext>(dbOptions);
            var mockableDbContextDatabase = Substitute.ForPartsOf<DatabaseFacade>(mockableDbContext);
            mockableDbContext.Configure().Database.Returns(mockableDbContextDatabase);

            serviceCollection.AddSingleton(mockableDbContext);
        }
    }
}
