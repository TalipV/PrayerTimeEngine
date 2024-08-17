using PrayerTimeEngine.Core.Data.EntityFramework;

namespace PrayerTimeEngine.Core.Tests.Unit
{
    public class EntityValidationTests
    {
        [Fact]
        public void All_Entities_With_DbSet_Should_Implement_IInsertedAt()
        {
            new AppDbContextMetaData().GetDbSetPropertyTypes()
                .Should()
                .AllSatisfy(entityType => typeof(IInsertedAt).IsAssignableFrom(entityType).Should().BeTrue(because: $"there is a DbSet for {entityType.FullName}"));
        }
    }
}
