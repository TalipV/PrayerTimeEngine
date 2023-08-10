using PrayerTimeEngine.Data.SQLite;
using PrayerTimeEngine.Domain.Calculators.Muwaqqit.Interfaces;
using PrayerTimeEngine.Domain.Calculators.Muwaqqit.Models;

namespace PrayerTimeEngine.Domain.Calculators.Muwaqqit.Services
{
    public class MuwaqqitDBAccess : IMuwaqqitDBAccess
    {
        private readonly ISQLiteDB _db;

        public MuwaqqitDBAccess(ISQLiteDB db)
        {
            _db = db;
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
                command.CommandText =
                @"
                INSERT INTO MuwaqqitPrayerTimes (Date, Timezone, Longitude, Latitude, Fajr_Degree, Isha_Degree, Ishtibaq_Degree, AsrKaraha_Degree, Fajr, NextFajr, Shuruq, Duha, Dhuhr, AsrMithl, AsrMithlayn, AsrKaraha, Maghrib, Isha, Ishtibaq, InsertDateTime) 
                VALUES                          ($Date, $Timezone, $Longitude, $Latitude, $Fajr_Degree, $Isha_Degree, $Ishtibaq_Degree, $AsrKaraha_Degree, $Fajr, $NextFajr, $Shuruq, $Duha, $Dhuhr, $AsrMithl, $AsrMithlayn, $AsrKaraha, $Maghrib, $Isha, $Ishtibaq, $InsertDateTime);";

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
                command.CommandText =
                @"
                SELECT Fajr, NextFajr, Shuruq, Duha, Dhuhr, AsrMithl, AsrMithlayn, Maghrib, Isha, Ishtibaq, AsrKaraha
                FROM MuwaqqitPrayerTimes
                WHERE Date = $Date AND Longitude = $Longitude AND Latitude = $Latitude 
                AND Fajr_Degree = $Fajr_Degree AND Isha_Degree = $Isha_Degree AND Ishtibaq_Degree = $Ishtibaq_Degree AND AsrKaraha_Degree = $AsrKaraha_Degree;";

                command.Parameters.AddWithValue("$Date", date);
                command.Parameters.AddWithValue("$Longitude", longitude);
                command.Parameters.AddWithValue("$Latitude", latitude);
                command.Parameters.AddWithValue("$Fajr_Degree", fajrDegree);
                command.Parameters.AddWithValue("$Isha_Degree", ishaDegree);
                command.Parameters.AddWithValue("$Ishtibaq_Degree", ishtibaqDegree);
                command.Parameters.AddWithValue("$AsrKaraha_Degree", asrKarahaDegree);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        time = new MuwaqqitPrayerTimes
                        {
                            Date = date,
                            Longitude = longitude,
                            Latitude = latitude,
                            Fajr = reader.GetDateTime(0),
                            NextFajr = reader.GetDateTime(1),
                            Shuruq = reader.GetDateTime(2),
                            Duha = reader.GetDateTime(3),
                            Dhuhr = reader.GetDateTime(4),
                            Asr = reader.GetDateTime(5),
                            AsrMithlayn = reader.GetDateTime(6),
                            Maghrib = reader.GetDateTime(7),
                            Isha = reader.GetDateTime(8),
                            Ishtibaq = reader.GetDateTime(9),
                            AsrKaraha = reader.GetDateTime(10),
                        };
                    }
                }
            });

            return time;
        }
    }
}
