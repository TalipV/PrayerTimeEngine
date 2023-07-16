using PrayerTimeEngine.Code.Domain.Muwaqqit.Interfaces;
using PrayerTimeEngine.Code.Domain.Muwaqqit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Code.Domain.Muwaqqit.Services
{
    public class MuwaqqitDBAccess : IMuwaqqitDBAccess
    {
        private readonly ISQLiteDB _db;

        public MuwaqqitDBAccess(ISQLiteDB db)
        {
            _db = db;
        }

        public void InsertMuwaqqitPrayerTimes(DateTime date, decimal longitude, decimal latitude, decimal fajrDegree, decimal ishaDegree, MuwaqqitPrayerTimes prayerTimes)
        {
            _db.ExecuteCommand(connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
        INSERT INTO MuwaqqitPrayerTimes (Date, Longitude, Latitude, Fajr_Degree, Isha_Degree, Fajr, Shuruq, Dhuhr, AsrMithl, AsrMithlayn, Maghrib, Isha) 
        VALUES ($Date, $Longitude, $Latitude, $Fajr_Degree, $Isha_Degree, $Fajr, $Shuruq, $Dhuhr, $AsrMithl, $AsrMithlayn, $Maghrib, $Isha);";

                command.Parameters.AddWithValue("$Date", date);
                command.Parameters.AddWithValue("$Longitude", longitude);
                command.Parameters.AddWithValue("$Latitude", latitude);
                command.Parameters.AddWithValue("$Fajr_Degree", fajrDegree);
                command.Parameters.AddWithValue("$Isha_Degree", ishaDegree);

                command.Parameters.AddWithValue("$Fajr", prayerTimes.Fajr);
                command.Parameters.AddWithValue("$Shuruq", prayerTimes.Shuruq);
                command.Parameters.AddWithValue("$Dhuhr", prayerTimes.Dhuhr);
                command.Parameters.AddWithValue("$AsrMithl", prayerTimes.AsrMithl);
                command.Parameters.AddWithValue("$AsrMithlayn", prayerTimes.AsrMithlayn);
                command.Parameters.AddWithValue("$Maghrib", prayerTimes.Maghrib);
                command.Parameters.AddWithValue("$Isha", prayerTimes.Isha);

                command.ExecuteNonQuery();
            });
        }

        public MuwaqqitPrayerTimes GetTimes(DateTime date, decimal longitude, decimal latitude, decimal fajrDegree, decimal ishaDegree)
        {
            MuwaqqitPrayerTimes time = null;

            _db.ExecuteCommand(connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
        SELECT Fajr, Shuruq, Dhuhr, AsrMithl, AsrMithlayn, Maghrib, Isha
        FROM MuwaqqitPrayerTimes
        WHERE Date = $Date AND Longitude = $Longitude AND Latitude = $Latitude AND Fajr_Degree = $Fajr_Degree AND Isha_Degree = $Isha_Degree;";

                command.Parameters.AddWithValue("$Date", date);
                command.Parameters.AddWithValue("$Longitude", longitude);
                command.Parameters.AddWithValue("$Latitude", latitude);
                command.Parameters.AddWithValue("$Fajr_Degree", fajrDegree);
                command.Parameters.AddWithValue("$Isha_Degree", ishaDegree);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        time = new MuwaqqitPrayerTimes(
                            date,
                            longitude,
                            latitude,
                            reader.GetDateTime(0),
                            reader.GetDateTime(1),
                            reader.GetDateTime(2),
                            reader.GetDateTime(3),
                            reader.GetDateTime(4),
                            reader.GetDateTime(5),
                            reader.GetDateTime(6)
                        );
                    }
                }
            });

            return time;
        }
    }
}
