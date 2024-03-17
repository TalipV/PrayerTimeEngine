using Microsoft.EntityFrameworkCore;
using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;

namespace PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Services
{
    public class MuwaqqitDBAccess(
            AppDbContext dbContext
        ) : IMuwaqqitDBAccess
    {
        public Task<List<MuwaqqitPrayerTimes>> GetAllTimes(CancellationToken cancellationToken)
        {
            return dbContext
                .MuwaqqitPrayerTimes.AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public Task<MuwaqqitPrayerTimes> GetTimesAsync(
            LocalDate date,
            decimal longitude,
            decimal latitude,
            double fajrDegree,
            double ishaDegree,
            double ishtibaqDegree,
            double asrKarahaDegree, CancellationToken cancellationToken)
        {
            return dbContext
                .MuwaqqitPrayerTimes.AsNoTracking()
                .Where(x => 
                    x.Date == date
                    && x.Longitude == longitude
                    && x.Latitude == latitude
                    && x.FajrDegree == fajrDegree
                    && x.IshaDegree == ishaDegree
                    && x.IshtibaqDegree == ishtibaqDegree
                    && x.AsrKarahaDegree == asrKarahaDegree)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task InsertMuwaqqitPrayerTimesAsync(MuwaqqitPrayerTimes prayerTimes, CancellationToken cancellationToken)
        {
            await dbContext.MuwaqqitPrayerTimes.AddAsync(prayerTimes, cancellationToken).ConfigureAwait(false);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteAllTimes(CancellationToken cancellationToken)
        {
            List<MuwaqqitPrayerTimes> toBeDeletedTimes = await dbContext.MuwaqqitPrayerTimes.ToListAsync(cancellationToken).ConfigureAwait(false);

            dbContext.MuwaqqitPrayerTimes.RemoveRange(toBeDeletedTimes);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
