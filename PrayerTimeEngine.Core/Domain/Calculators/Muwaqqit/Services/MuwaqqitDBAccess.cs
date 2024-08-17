using Microsoft.EntityFrameworkCore;
using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Services
{
    public class MuwaqqitDBAccess(
            AppDbContext dbContext
        ) : IMuwaqqitDBAccess, IPrayerTimeCacheCleaner
    {
        public Task<List<MuwaqqitPrayerTimes>> GetAllTimes(CancellationToken cancellationToken)
        {
            return dbContext
                .MuwaqqitPrayerTimes.AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        private static readonly Func<AppDbContext, ZonedDateTime, decimal, decimal, double, double, double, double, IAsyncEnumerable<MuwaqqitPrayerTimes>> compiledQuery_GetTimesAsync =
            EF.CompileAsyncQuery(
                (AppDbContext context, ZonedDateTime date, decimal longitude, decimal latitude, double fajrDegree, double ishaDegree, double ishtibaqDegree, double asrKarahaDegree) =>
                    context.MuwaqqitPrayerTimes.AsNoTracking()
                    .Where(x =>
                        x.Date == date
                        && x.Longitude == longitude
                        && x.Latitude == latitude
                        && x.FajrDegree == fajrDegree
                        && x.IshaDegree == ishaDegree
                        && x.IshtibaqDegree == ishtibaqDegree
                        && x.AsrKarahaDegree == asrKarahaDegree));

        public Task<MuwaqqitPrayerTimes> GetTimesAsync(
            ZonedDateTime date,
            decimal longitude,
            decimal latitude,
            double fajrDegree,
            double ishaDegree,
            double ishtibaqDegree,
            double asrKarahaDegree, CancellationToken cancellationToken)
        {
            return compiledQuery_GetTimesAsync(dbContext, date, longitude, latitude, fajrDegree, ishaDegree, ishtibaqDegree, asrKarahaDegree)
                .FirstOrDefaultAsync(cancellationToken)
                .AsTask();
        }

        public async Task InsertMuwaqqitPrayerTimesAsync(IEnumerable<MuwaqqitPrayerTimes> muwaqqitPrayerTimesLst, CancellationToken cancellationToken)
        {
            await dbContext.MuwaqqitPrayerTimes.AddRangeAsync(muwaqqitPrayerTimesLst, cancellationToken).ConfigureAwait(false);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteCacheDataAsync(ZonedDateTime deleteBeforeDate, CancellationToken cancellationToken)
        {
            await dbContext.MuwaqqitPrayerTimes
                .Where(p => p.Date.ToInstant() < deleteBeforeDate.ToInstant())
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);
        }

    }
}
