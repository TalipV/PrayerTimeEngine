using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;

namespace PrayerTimeEngine.Core.Data.EntityFramework
{
    internal static class EFCoreExtensions
    {
        // based on https://stackoverflow.com/questions/57060314/how-to-reload-collection-in-ef-core-2-x
        public static Task ReloadAsync(this CollectionEntry source, CancellationToken cancellationToken)
        {
            if (source.CurrentValue != null)
            {
                foreach (var item in source.CurrentValue)
                {
                    source.EntityEntry.Context.Entry(item).State = EntityState.Detached;
                }

                source.CurrentValue = null;
            }
            source.IsLoaded = false;
            return source.LoadAsync(cancellationToken);
        }
    }
}
