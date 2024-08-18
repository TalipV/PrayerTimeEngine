using Microsoft.EntityFrameworkCore;

namespace PrayerTimeEngine.Core.Data.EntityFramework
{
    public class AppDbContextMetaData
    {
        private readonly Lazy<List<Type>> _getDbSetPropertyTypesLazy = new Lazy<List<Type>>(getDbSetPropertyTypesInternal);

        private static List<Type> getDbSetPropertyTypesInternal()
        {
            return typeof(AppDbContext).GetProperties()
                .Where(prop => prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .Select(prop => prop.PropertyType.GetGenericArguments().FirstOrDefault())
                .Where(x => x is not null)
                .ToList();
        }

        public List<Type> GetDbSetPropertyTypes()
        {
            return _getDbSetPropertyTypesLazy.Value;
        }
    }
}
