using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using PrayerTimeEngine.Data.SQLite;
using PrayerTimeEngine.Domain.Calculators.Muwaqqit.Interfaces;
using PrayerTimeEngine.Domain.Calculators.Muwaqqit.Models;

namespace PrayerTimeEngine.Domain.Calculators.Muwaqqit.Services
{
    public class MuwaqqitDBAccess : IMuwaqqitDBAccess
    {
        private const string _selectSQL = """
                    SELECT 
                        Date, Longitude, Latitude, InsertDateTime,
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
            DateTime date,
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
                    INSERT INTO MuwaqqitPrayerTimes (Date, Timezone, Longitude, Latitude, Fajr_Degree, Isha_Degree, Ishtibaq_Degree, AsrKaraha_Degree, Fajr, NextFajr, Shuruq, Duha, Dhuhr, AsrMithl, AsrMithlayn, AsrKaraha, Maghrib, Isha, Ishtibaq, InsertDateTime) 
                    VALUES                          ($Date, $Timezone, $Longitude, $Latitude, $Fajr_Degree, $Isha_Degree, $Ishtibaq_Degree, $AsrKaraha_Degree, $Fajr, $NextFajr, $Shuruq, $Duha, $Dhuhr, $AsrMithl, $AsrMithlayn, $AsrKaraha, $Maghrib, $Isha, $Ishtibaq, $InsertDateTime);
                    """;

                command.Parameters.AddWithValue("$Date", date);
                command.Parameters.AddWithValue("$Timezone", timezone);
                command.Parameters.AddWithValue("$Longitude", longitude);
                command.Parameters.AddWithValue("$Latitude", latitude);

                command.Parameters.AddWithValue("$Fajr_Degree", fajrDegree);
                command.Parameters.AddWithValue("$Isha_Degree", ishaDegree);
                command.Parameters.AddWithValue("$Ishtibaq_Degree", ishtibaqDegree);
                command.Parameters.AddWithValue("$AsrKaraha_Degree", asrKarahaDegree);

                command.Parameters.AddWithValue("$Fajr", prayerTimes.Fajr);
                command.Parameters.AddWithValue("$NextFajr", prayerTimes.NextFajr);
                command.Parameters.AddWithValue("$Shuruq", prayerTimes.Shuruq);
                command.Parameters.AddWithValue("$Duha", prayerTimes.Duha);
                command.Parameters.AddWithValue("$Dhuhr", prayerTimes.Dhuhr);

                command.Parameters.AddWithValue("$AsrMithl", prayerTimes.Asr);
                command.Parameters.AddWithValue("$AsrMithlayn", prayerTimes.AsrMithlayn);
                command.Parameters.AddWithValue("$AsrKaraha", prayerTimes.AsrKaraha);

                command.Parameters.AddWithValue("$Maghrib", prayerTimes.Maghrib);
                command.Parameters.AddWithValue("$Isha", prayerTimes.Isha);
                command.Parameters.AddWithValue("$Ishtibaq", prayerTimes.Ishtibaq);

                command.Parameters.AddWithValue("$InsertDateTime", DateTime.Now);

                await command.ExecuteNonQueryAsync();
            });

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

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        times.Add(getMuwaqqitPrayerTimesFromReader(reader));
                    }
                }
            });

            return times;
        }

        private static MuwaqqitPrayerTimes getMuwaqqitPrayerTimesFromReader(SqliteDataReader reader)
        {
            return new MuwaqqitPrayerTimes
            {
                Date = reader.GetDateTime(0),
                Longitude = reader.GetDecimal(1),
                Latitude = reader.GetDecimal(2),
                InsertDateTime = reader.GetDateTime(3),

                Fajr = reader.GetDateTime(4),
                NextFajr = reader.GetDateTime(5),
                Shuruq = reader.GetDateTime(6),
                Duha = reader.GetDateTime(7),
                Dhuhr = reader.GetDateTime(8),
                Asr = reader.GetDateTime(9),
                AsrMithlayn = reader.GetDateTime(10),
                Maghrib = reader.GetDateTime(11),
                Isha = reader.GetDateTime(12),
                Ishtibaq = reader.GetDateTime(13),
                AsrKaraha = reader.GetDateTime(14),

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
                await command.ExecuteNonQueryAsync();
            });
        }

        public async Task<MuwaqqitPrayerTimes> GetTimesAsync(
            DateTime date,
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

                command.Parameters.AddWithValue("$Date", date);
                command.Parameters.AddWithValue("$Longitude", longitude);
                command.Parameters.AddWithValue("$Latitude", latitude);
                command.Parameters.AddWithValue("$Fajr_Degree", fajrDegree);
                command.Parameters.AddWithValue("$Isha_Degree", ishaDegree);
                command.Parameters.AddWithValue("$Ishtibaq_Degree", ishtibaqDegree);
                command.Parameters.AddWithValue("$AsrKaraha_Degree", asrKarahaDegree);

                using SqliteDataReader reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    time = getMuwaqqitPrayerTimesFromReader(reader);
                }
            });

            return time;
        }
    }
}
