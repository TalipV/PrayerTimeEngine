using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using NodaTime;
using PrayerTimeEngine.Core.Common.Extension;
using PrayerTimeEngine.Core.Data.SQLite;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Models;

namespace PrayerTimeEngine.Core.Domain.Calculators.Muwaqqit.Services
{
    public class MuwaqqitDBAccess : IMuwaqqitDBAccess
    {
        private const string _selectSQL = """
                    SELECT 
                        Date, Longitude, Latitude, InsertInstant,
                        Fajr, NextFajr, Shuruq, Duha, Dhuhr, AsrMithl, AsrMithlayn, Maghrib, Isha, Ishtibaq, AsrKaraha,
                        Fajr_Degree, AsrKaraha_Degree, Ishtibaq_Degree, Isha_Degree
                    FROM MuwaqqitPrayerTimes
                    """;

        private readonly ISQLiteDB _db;
        private readonly ILogger _logger;

        public MuwaqqitDBAccess(
            ISQLiteDB db,
            ILogger<MuwaqqitDBAccess> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task InsertMuwaqqitPrayerTimesAsync(
            LocalDate date,
            string timezone,
            decimal longitude,
            decimal latitude,
            double fajrDegree,
            double ishaDegree,
            double ishtibaqDegree,
            double asrKarahaDegree,
            MuwaqqitPrayerTimes prayerTimes)
        {
            await _db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText = """
                    INSERT INTO MuwaqqitPrayerTimes (Date, Timezone, Longitude, Latitude, Fajr_Degree, Isha_Degree, Ishtibaq_Degree, AsrKaraha_Degree, Fajr, NextFajr, Shuruq, Duha, Dhuhr, AsrMithl, AsrMithlayn, AsrKaraha, Maghrib, Isha, Ishtibaq, InsertInstant) 
                    VALUES                          ($Date, $Timezone, $Longitude, $Latitude, $Fajr_Degree, $Isha_Degree, $Ishtibaq_Degree, $AsrKaraha_Degree, $Fajr, $NextFajr, $Shuruq, $Duha, $Dhuhr, $AsrMithl, $AsrMithlayn, $AsrKaraha, $Maghrib, $Isha, $Ishtibaq, $InsertInstant);
                    """;

                command.Parameters.AddWithValue("$Date", date.GetStringForDBColumn());
                command.Parameters.AddWithValue("$Timezone", timezone);
                command.Parameters.AddWithValue("$Longitude", longitude);
                command.Parameters.AddWithValue("$Latitude", latitude);

                command.Parameters.AddWithValue("$Fajr_Degree", fajrDegree);
                command.Parameters.AddWithValue("$Isha_Degree", ishaDegree);
                command.Parameters.AddWithValue("$Ishtibaq_Degree", ishtibaqDegree);
                command.Parameters.AddWithValue("$AsrKaraha_Degree", asrKarahaDegree);

                command.Parameters.AddWithValue("$Fajr", prayerTimes.Fajr.GetStringForDBColumn());
                command.Parameters.AddWithValue("$NextFajr", prayerTimes.NextFajr.GetStringForDBColumn());
                command.Parameters.AddWithValue("$Shuruq", prayerTimes.Shuruq.GetStringForDBColumn());
                command.Parameters.AddWithValue("$Duha", prayerTimes.Duha.GetStringForDBColumn());
                command.Parameters.AddWithValue("$Dhuhr", prayerTimes.Dhuhr.GetStringForDBColumn());

                command.Parameters.AddWithValue("$AsrMithl", prayerTimes.Asr.GetStringForDBColumn());
                command.Parameters.AddWithValue("$AsrMithlayn", prayerTimes.AsrMithlayn.GetStringForDBColumn());
                command.Parameters.AddWithValue("$AsrKaraha", prayerTimes.AsrKaraha.GetStringForDBColumn());

                command.Parameters.AddWithValue("$Maghrib", prayerTimes.Maghrib.GetStringForDBColumn());
                command.Parameters.AddWithValue("$Isha", prayerTimes.Isha.GetStringForDBColumn());
                command.Parameters.AddWithValue("$Ishtibaq", prayerTimes.Ishtibaq.GetStringForDBColumn());

                command.Parameters.AddWithValue("$InsertInstant", SystemClock.Instance.GetCurrentInstant().GetStringForDBColumn());

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);

            _logger.LogDebug(
                "Inserted time: {Date} ({Timezone}), ({Longitude}/{Latitude}), Fajr {FajrDegree}, AsrKaraha {AsrKarahaDegree}, Ishtibaq {IshtibaqDegree}, Isha {IshaDegree}",
                date, timezone, longitude, latitude, fajrDegree, asrKarahaDegree, ishtibaqDegree, ishaDegree);
        }

        public async Task<List<MuwaqqitPrayerTimes>> GetAllTimes()
        {
            List<MuwaqqitPrayerTimes> times = new();

            await _db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText = _selectSQL;

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        times.Add(getMuwaqqitPrayerTimesFromReader(reader));
                    }
                }
            }).ConfigureAwait(false);

            return times;
        }

        private static MuwaqqitPrayerTimes getMuwaqqitPrayerTimesFromReader(SqliteDataReader reader)
        {
            return new MuwaqqitPrayerTimes
            {
                Date = reader.GetString(0).GetLocalDateFromDBColumnString(),
                Longitude = reader.GetDecimal(1),
                Latitude = reader.GetDecimal(2),
                InsertInstant = reader.GetString(3).GetInstantFromDBColumnString(),

                Fajr = reader.GetString(4).GetZonedDateTimeFromDBColumnString(),
                NextFajr = reader.GetString(5).GetZonedDateTimeFromDBColumnString(),
                Shuruq = reader.GetString(6).GetZonedDateTimeFromDBColumnString(),
                Duha = reader.GetString(7).GetZonedDateTimeFromDBColumnString(),
                Dhuhr = reader.GetString(8).GetZonedDateTimeFromDBColumnString(),
                Asr = reader.GetString(9).GetZonedDateTimeFromDBColumnString(),
                AsrMithlayn = reader.GetString(10).GetZonedDateTimeFromDBColumnString(),
                Maghrib = reader.GetString(11).GetZonedDateTimeFromDBColumnString(),
                Isha = reader.GetString(12).GetZonedDateTimeFromDBColumnString(),
                Ishtibaq = reader.GetString(13).GetZonedDateTimeFromDBColumnString(),
                AsrKaraha = reader.GetString(14).GetZonedDateTimeFromDBColumnString(),

                FajrDegree = reader.GetDouble(15),
                AsrKarahaDegree = reader.GetDouble(16),
                IshtibaqDegree = reader.GetDouble(17),
                IshaDegree = reader.GetDouble(18),
            };
        }

        public async Task DeleteAllTimes()
        {
            await _db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM MuwaqqitPrayerTimes;";
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
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
            MuwaqqitPrayerTimes time = null;

            await _db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText = $"""
                    {_selectSQL}
                    WHERE
                        Date = $Date AND Longitude = $Longitude AND Latitude = $Latitude 
                        AND Fajr_Degree = $Fajr_Degree
                        AND Isha_Degree = $Isha_Degree
                        AND Ishtibaq_Degree = $Ishtibaq_Degree
                        AND AsrKaraha_Degree = $AsrKaraha_Degree;
                    """;

                command.Parameters.AddWithValue("$Date", date.GetStringForDBColumn());
                command.Parameters.AddWithValue("$Longitude", longitude);
                command.Parameters.AddWithValue("$Latitude", latitude);
                command.Parameters.AddWithValue("$Fajr_Degree", fajrDegree);
                command.Parameters.AddWithValue("$Isha_Degree", ishaDegree);
                command.Parameters.AddWithValue("$Ishtibaq_Degree", ishtibaqDegree);
                command.Parameters.AddWithValue("$AsrKaraha_Degree", asrKarahaDegree);

                using SqliteDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                if (await reader.ReadAsync().ConfigureAwait(false))
                {
                    time = getMuwaqqitPrayerTimesFromReader(reader);
                }
            }).ConfigureAwait(false);

            return time;
        }
    }
}
