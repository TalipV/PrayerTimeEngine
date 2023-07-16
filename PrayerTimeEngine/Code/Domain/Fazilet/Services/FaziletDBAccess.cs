using PrayerTimeEngine.Code.Domain.Fazilet.Interfaces;
using PrayerTimeEngine.Code.Domain.Fazilet.Models;

namespace PrayerTimeEngine.Code.Domain.Fazilet.Services
{
    public class FaziletDBAccess : IFaziletDBAccess
    {
        private readonly ISQLiteDB _db;

        public FaziletDBAccess(ISQLiteDB db)
        {
            _db = db;
        }

        public Dictionary<string, int> GetCountries()
        {
            var countries = new Dictionary<string, int>();
            _db.ExecuteCommand(connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                SELECT Id, Name
                FROM FaziletCountries;
                ";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        countries.Add(reader.GetString(1), reader.GetInt32(0));
                    }
                }
            });

            return countries;
        }

        public void InsertCountry(int id, string name)
        {
            _db.ExecuteCommand(connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                INSERT INTO FaziletCountries (Id, Name) 
                VALUES ($Id, $Name);";

                command.Parameters.AddWithValue("$Id", id);
                command.Parameters.AddWithValue("$Name", name);

                command.ExecuteNonQuery();
            });
        }

        public Dictionary<string, int> GetCitiesByCountryID(int countryId)
        {
            var cities = new Dictionary<string, int>();
            _db.ExecuteCommand(connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                SELECT Id, Name
                FROM FaziletCities
                WHERE CountryId = $CountryId;";

                command.Parameters.AddWithValue("$CountryId", countryId);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cities.Add(reader.GetString(1), reader.GetInt32(0));
                    }
                }
            });
            return cities;
        }

        public void InsertCity(int id, string name, int countryId)
        {
            _db.ExecuteCommand(connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                INSERT INTO FaziletCities (Id, Name, CountryId) 
                VALUES ($Id, $Name, $CountryId);";

                command.Parameters.AddWithValue("$Id", id);
                command.Parameters.AddWithValue("$Name", name);
                command.Parameters.AddWithValue("$CountryId", countryId);

                command.ExecuteNonQuery();
            });
        }

        public FaziletPrayerTimes GetTimesByDateAndCityID(DateTime date, int cityId)
        {
            FaziletPrayerTimes time = null;

            _db.ExecuteCommand(connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                SELECT Imsak, Fajr, Shuruq, Dhuhr, Asr, Maghrib, Isha, Date
                FROM FaziletPrayerTimes
                WHERE CityId = $CityId AND $Date = $Date;";

                command.Parameters.AddWithValue("$CityId", cityId);
                command.Parameters.AddWithValue("$Date", date.Date);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        time = new FaziletPrayerTimes(
                            cityId,
                            reader.GetDateTime(0),
                            reader.GetDateTime(1),
                            reader.GetDateTime(2),
                            reader.GetDateTime(3),
                            reader.GetDateTime(4),
                            reader.GetDateTime(5),
                            reader.GetDateTime(6),
                            reader.GetDateTime(7)
                        );
                    }
                }
            });

            return time;
        }

        public void InsertCountries(Dictionary<string, int> countries)
        {
            _db.ExecuteCommand(connection =>
            {
                foreach (var country in countries)
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
            INSERT INTO FaziletCountries (Id, Name) 
            VALUES ($Id, $Name);";

                    command.Parameters.AddWithValue("$Id", country.Value);
                    command.Parameters.AddWithValue("$Name", country.Key);

                    command.ExecuteNonQuery();
                }
            });
        }

        public void InsertCities(Dictionary<string, int> cities, int countryId)
        {
            _db.ExecuteCommand(connection =>
            {
                foreach (var city in cities)
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
            INSERT INTO FaziletCities (Id, Name, CountryId) 
            VALUES ($Id, $Name, $CountryId);";

                    command.Parameters.AddWithValue("$Id", city.Value);
                    command.Parameters.AddWithValue("$Name", city.Key);
                    command.Parameters.AddWithValue("$CountryId", countryId);

                    command.ExecuteNonQuery();
                }
            });
        }

        public void InsertFaziletPrayerTimes(DateTime date, int cityID, FaziletPrayerTimes faziletPrayerTimes)
        {
            _db.ExecuteCommand(connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                INSERT INTO FaziletPrayerTimes (Date, CityId, Imsak, Fajr, Shuruq, Dhuhr, Asr, Maghrib, Isha) 
                VALUES ($Date, $CityId, $Imsak, $Fajr, $Shuruq, $Dhuhr, $Asr, $Maghrib, $Isha);";

                command.Parameters.AddWithValue("$Date", date);
                command.Parameters.AddWithValue("$CityId", cityID);

                command.Parameters.AddWithValue("$Imsak", faziletPrayerTimes.Imsak);
                command.Parameters.AddWithValue("$Fajr", faziletPrayerTimes.Fajr);
                command.Parameters.AddWithValue("$Shuruq", faziletPrayerTimes.Shuruq);
                command.Parameters.AddWithValue("$Dhuhr", faziletPrayerTimes.Dhuhr);
                command.Parameters.AddWithValue("$Asr", faziletPrayerTimes.Asr);
                command.Parameters.AddWithValue("$Maghrib", faziletPrayerTimes.Maghrib);
                command.Parameters.AddWithValue("$Isha", faziletPrayerTimes.Isha);

                command.ExecuteNonQuery();
            });
        }

        public void DeleteAllPrayerTimes()
        {
            _db.ExecuteCommand(connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                DELETE FROM FaziletPrayerTimes;";

                command.ExecuteNonQuery();
            });
        }
    }
}
