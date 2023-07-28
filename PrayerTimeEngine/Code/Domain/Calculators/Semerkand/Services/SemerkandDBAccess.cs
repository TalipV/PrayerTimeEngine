using PrayerTimeEngine.Code.Domain.Calculator.Semerkand.Models;
using PrayerTimeEngine.Code.Domain.Calculators.Semerkand.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Code.Domain.Calculators.Semerkand.Services
{
    public class SemerkandDBAccess : ISemerkandDBAccess
    {
        private readonly ISQLiteDB _db;

        public SemerkandDBAccess(ISQLiteDB db)
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
                FROM SemerkandCountries;
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
                INSERT INTO SemerkandCountries (Id, Name, InsertDateTime) 
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
                FROM SemerkandCities
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
                INSERT INTO SemerkandCities (Id, Name, CountryId, InsertDateTime) 
                VALUES ($Id, $Name, $CountryId, $InsertDateTime);";

                command.Parameters.AddWithValue("$Id", id);
                command.Parameters.AddWithValue("$Name", name);
                command.Parameters.AddWithValue("$CountryId", countryId);
                command.Parameters.AddWithValue("$InsertDateTime", DateTime.Now);

                await command.ExecuteNonQueryAsync();
            });
        }

        public async Task<SemerkandPrayerTimes> GetTimesByDateAndCityID(DateTime date, int cityId)
        {
            SemerkandPrayerTimes time = null;

            await _db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                SELECT Fajr, Shuruq, Dhuhr, Asr, Maghrib, Isha, Date
                FROM SemerkandPrayerTimes
                WHERE CityId = $CityId AND Date = $Date;";

                command.Parameters.AddWithValue("$CityId", cityId);
                command.Parameters.AddWithValue("$Date", date.Date);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        time = 
                        new SemerkandPrayerTimes
                        {
                            CityID = cityId,
                            Fajr = reader.GetDateTime(0),
                            Shuruq = reader.GetDateTime(1),
                            Dhuhr = reader.GetDateTime(2),
                            Asr = reader.GetDateTime(3),
                            Maghrib = reader.GetDateTime(4),
                            Isha = reader.GetDateTime(5),
                            Date = reader.GetDateTime(6)
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
                    INSERT INTO SemerkandCountries (Id, Name, InsertDateTime) 
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
            INSERT INTO SemerkandCities (Id, Name, CountryId, InsertDateTime) 
            VALUES ($Id, $Name, $CountryId, $InsertDateTime);";

                    command.Parameters.AddWithValue("$Id", city.Value);
                    command.Parameters.AddWithValue("$Name", city.Key);
                    command.Parameters.AddWithValue("$CountryId", countryId);
                    command.Parameters.AddWithValue("$InsertDateTime", DateTime.Now);

                    await command.ExecuteNonQueryAsync();
                }
            });
        }

        public async Task InsertSemerkandPrayerTimes(DateTime date, int cityID, SemerkandPrayerTimes semerkandPrayerTimes)
        {
            await _db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                INSERT INTO SemerkandPrayerTimes (Date, CityId, Fajr, Shuruq, Dhuhr, Asr, Maghrib, Isha, InsertDateTime) 
                VALUES ($Date, $CityId, $Fajr, $Shuruq, $Dhuhr, $Asr, $Maghrib, $Isha, $InsertDateTime);";

                command.Parameters.AddWithValue("$Date", date);
                command.Parameters.AddWithValue("$CityId", cityID);

                command.Parameters.AddWithValue("$Fajr", semerkandPrayerTimes.Fajr);
                command.Parameters.AddWithValue("$Shuruq", semerkandPrayerTimes.Shuruq);
                command.Parameters.AddWithValue("$Dhuhr", semerkandPrayerTimes.Dhuhr);
                command.Parameters.AddWithValue("$Asr", semerkandPrayerTimes.Asr);
                command.Parameters.AddWithValue("$Maghrib", semerkandPrayerTimes.Maghrib);
                command.Parameters.AddWithValue("$Isha", semerkandPrayerTimes.Isha);
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
                DELETE FROM SemerkandPrayerTimes;";

                await command.ExecuteNonQueryAsync();
            });
        }
    }
}
