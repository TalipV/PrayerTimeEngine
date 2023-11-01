using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.Extensions;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Configuration.Interfaces;
using PrayerTimeEngine.Core.Domain.Configuration.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Core.Tests
{
    public abstract class BaseTest
    {
        private DbConnection _keepMemoryDbAliveDbConnection = null;
        protected ServiceProvider ServiceProvider { get; set; }

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
            _keepMemoryDbAliveDbConnection?.Close();
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
