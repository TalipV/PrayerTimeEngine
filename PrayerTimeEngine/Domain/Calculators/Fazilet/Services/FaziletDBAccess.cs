using PrayerTimeEngine.Domain.Calculators.Fazilet.Interfaces;
using PrayerTimeEngine.Domain.Calculators.Fazilet.Models;
using System;

namespace PrayerTimeEngine.Domain.Calculators.Fazilet.Services
{
    public class FaziletDBAccess : IFaziletDBAccess
    {
        private readonly ISQLiteDB _db;

        public FaziletDBAccess(ISQLiteDB db)
        {
            _db = db;
        }

        public async Task<Dictionary<string, int>> GetCountries()
        {
            var countries = new Dictionary<string, int>();
            await _db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                SELECT Id, Name
                FROM FaziletCountries;
                ";

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        countries.Add(reader.GetString(1), reader.GetInt32(0));
                    }
                }
            });

            return countries;
        }

        public async Task InsertCountry(int id, string name)
        {
            await _db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                INSERT INTO FaziletCountries (Id, Name, InsertDateTime) 
                VALUES ($Id, $Name, $InsertDateTime);";

                command.Parameters.AddWithValue("$Id", id);
                command.Parameters.AddWithValue("$Name", name);
                command.Parameters.AddWithValue("$InsertDateTime", DateTime.Now);

                await command.ExecuteNonQueryAsync();
            });
        }

        public async Task<Dictionary<string, int>> GetCitiesByCountryID(int countryId)
        {
            var cities = new Dictionary<string, int>();
            await _db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                SELECT Id, Name
                FROM FaziletCities
                WHERE CountryId = $CountryId;";

                command.Parameters.AddWithValue("$CountryId", countryId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        cities.Add(reader.GetString(1), reader.GetInt32(0));
                    }
                }
            });
            return cities;
        }

        public async Task InsertCity(int id, string name, int countryId)
        {
            await _db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                INSERT INTO FaziletCities (Id, Name, CountryId, InsertDateTime) 
                VALUES ($Id, $Name, $CountryId, $InsertDateTime);";

                command.Parameters.AddWithValue("$Id", id);
                command.Parameters.AddWithValue("$Name", name);
                command.Parameters.AddWithValue("$CountryId", countryId);
                command.Parameters.AddWithValue("$InsertDateTime", DateTime.Now);

                await command.ExecuteNonQueryAsync();
            });
        }

        public async Task<FaziletPrayerTimes> GetTimesByDateAndCityID(DateTime date, int cityId)
        {
            FaziletPrayerTimes time = null;

            await _db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                SELECT Imsak, Fajr, Shuruq, Dhuhr, Asr, Maghrib, Isha, Date
                FROM FaziletPrayerTimes
                WHERE CityId = $CityId AND Date = $Date;";

                command.Parameters.AddWithValue("$CityId", cityId);
                command.Parameters.AddWithValue("$Date", date.Date);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        time = new FaziletPrayerTimes
                        {
                            CityID = cityId,
                            Imsak = reader.GetDateTime(0),
                            Fajr = reader.GetDateTime(1),
                            Shuruq = reader.GetDateTime(2),
                            Dhuhr = reader.GetDateTime(3),
                            Asr = reader.GetDateTime(4),
                            Maghrib = reader.GetDateTime(5),
                            Isha = reader.GetDateTime(6),
                            Date = reader.GetDateTime(7)
                        };
                    }
                }
            });

            return time;
        }

        public async Task InsertCountries(Dictionary<string, int> countries)
        {
            await _db.ExecuteCommandAsync(async connection =>
            {
                foreach (var country in countries)
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                    INSERT INTO FaziletCountries (Id, Name, InsertDateTime) 
                    VALUES ($Id, $Name, $InsertDateTime);";

                    command.Parameters.AddWithValue("$Id", country.Value);
                    command.Parameters.AddWithValue("$Name", country.Key);
                    command.Parameters.AddWithValue("$InsertDateTime", DateTime.Now);

                    await command.ExecuteNonQueryAsync();
                }
            });
        }

        public async Task InsertCities(Dictionary<string, int> cities, int countryId)
        {
            await _db.ExecuteCommandAsync(async connection =>
            {
                foreach (var city in cities)
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
            INSERT INTO FaziletCities (Id, Name, CountryId, InsertDateTime) 
            VALUES ($Id, $Name, $CountryId, $InsertDateTime);";

                    command.Parameters.AddWithValue("$Id", city.Value);
                    command.Parameters.AddWithValue("$Name", city.Key);
                    command.Parameters.AddWithValue("$CountryId", countryId);
                    command.Parameters.AddWithValue("$InsertDateTime", DateTime.Now);

                    await command.ExecuteNonQueryAsync();
                }
            });
        }

        public async Task InsertFaziletPrayerTimes(DateTime date, int cityID, FaziletPrayerTimes faziletPrayerTimes)
        {
            await _db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                INSERT INTO FaziletPrayerTimes (Date, CityId, Imsak, Fajr, Shuruq, Dhuhr, Asr, Maghrib, Isha, InsertDateTime) 
                VALUES ($Date, $CityId, $Imsak, $Fajr, $Shuruq, $Dhuhr, $Asr, $Maghrib, $Isha, $InsertDateTime);";

                command.Parameters.AddWithValue("$Date", date);
                command.Parameters.AddWithValue("$CityId", cityID);

                command.Parameters.AddWithValue("$Imsak", faziletPrayerTimes.Imsak);
                command.Parameters.AddWithValue("$Fajr", faziletPrayerTimes.Fajr);
                command.Parameters.AddWithValue("$Shuruq", faziletPrayerTimes.Shuruq);
                command.Parameters.AddWithValue("$Dhuhr", faziletPrayerTimes.Dhuhr);
                command.Parameters.AddWithValue("$Asr", faziletPrayerTimes.Asr);
                command.Parameters.AddWithValue("$Maghrib", faziletPrayerTimes.Maghrib);
                command.Parameters.AddWithValue("$Isha", faziletPrayerTimes.Isha);
                command.Parameters.AddWithValue("$InsertDateTime", DateTime.Now);

                await command.ExecuteNonQueryAsync();
            });
        }

        public async Task DeleteAllPrayerTimes()
        {
            await _db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                DELETE FROM FaziletPrayerTimes;";

                await command.ExecuteNonQueryAsync();
            });
        }
    }
}
