using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Data.EntityFramework;
using System.Reflection;

namespace PrayerTimeEngine.Core.Tests.Unit;

public class EntityValidationTests
{
    [Fact]
    public void All_Entities_With_DbSet_Should_Implement_IEntity()
    {
        new AppDbContextMetaData().GetDbSetPropertyTypes()
            .Should()
            .AllSatisfy(entityType => typeof(IEntity).IsAssignableFrom(entityType).Should().BeTrue(because: $"there is a DbSet for {entityType.FullName}"));
    }

    [Fact]
    public void Enums_Should_Not_Have_Duplicate_Values()
    {
        Assembly assembly = typeof(ETimeType).Assembly;

        foreach (Type enumType in assembly.GetTypes().Where(t => t.IsEnum))
        {
            var enumValues = Enum.GetValues(enumType);

            if (enumValues.Length == enumValues.OfType<object>().ToHashSet().Count)
            {
                continue;
            }

            var duplicateEnumValues = enumValues
                .Cast<object>()
                .GroupBy(v => v)
                .Where(g => g.Count() > 1)
                .Select(g => Convert.ToInt64(g.Key).ToString()) // works with all underlying enum types (int, byte, long, ...)
                .ToList();

            duplicateEnumValues.Should().BeEmpty(because: $"{enumType.Name} should not contain duplicate values");
        }
    }
}
