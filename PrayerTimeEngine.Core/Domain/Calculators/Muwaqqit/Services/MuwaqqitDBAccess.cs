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
        public async Task InsertMuwaqqitPrayerTimesAsync(MuwaqqitPrayerTimes prayerTimes)
        {
            await dbContext.MuwaqqitPrayerTimes.AddAsync(prayerTimes).ConfigureAwait(false);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<List<MuwaqqitPrayerTimes>> GetAllTimes()
        {
            return await dbContext.MuwaqqitPrayerTimes.ToListAsync().ConfigureAwait(false);
        }

        public async Task DeleteAllTimes()
        {
            dbContext.MuwaqqitPrayerTimes.RemoveRange(await dbContext.MuwaqqitPrayerTimes.ToListAsync().ConfigureAwait(false));
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<MuwaqqitPrayerTimes> GetTimesAsync(
            LocalDate date,
            decimal longitude,
            decimal latitude,
            double fajrDegree,
            double ishaDegree,
            double ishtibaqDegree,
            double asrKarahaDegree)
        {
            return await dbContext
                .MuwaqqitPrayerTimes
                .Where(x => 
                    x.Date == date
                    && x.Longitude == longitude
                    && x.Latitude == latitude
                    && x.FajrDegree == fajrDegree
                    && x.IshaDegree == ishaDegree
                    && x.IshtibaqDegree == ishtibaqDegree
                    && x.AsrKarahaDegree == asrKarahaDegree)
                .FirstOrDefaultAsync().ConfigureAwait(false);
        }
    }
}
