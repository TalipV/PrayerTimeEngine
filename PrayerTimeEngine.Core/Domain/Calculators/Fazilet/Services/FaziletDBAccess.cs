﻿using NodaTime;
using PrayerTimeEngine.Core.Common.Extension;
using PrayerTimeEngine.Core.Data.SQLite;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;

namespace PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Services
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
                command.CommandText = """
                    SELECT Id, Name
                    FROM FaziletCountries;
                    """;

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
                command.CommandText = """
                    INSERT INTO FaziletCountries (Id, Name, InsertInstant) 
                    VALUES ($Id, $Name, $InsertInstant);
                    """;

                command.Parameters.AddWithValue("$Id", id);
                command.Parameters.AddWithValue("$Name", name);
                command.Parameters.AddWithValue("$InsertInstant", SystemClock.Instance.GetCurrentInstant());

                await command.ExecuteNonQueryAsync();
            });
        }

        public async Task<Dictionary<string, int>> GetCitiesByCountryID(int countryId)
        {
            var cities = new Dictionary<string, int>();
            await _db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText = """
                    SELECT Id, Name
                    FROM FaziletCities
                    WHERE CountryId = $CountryId;
                    """;

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
                command.CommandText = """
                    INSERT INTO FaziletCities (Id, Name, CountryId, InsertInstant) 
                    VALUES ($Id, $Name, $CountryId, $InsertInstant);
                    """;

                command.Parameters.AddWithValue("$Id", id);
                command.Parameters.AddWithValue("$Name", name);
                command.Parameters.AddWithValue("$CountryId", countryId);
                command.Parameters.AddWithValue("$InsertInstant", SystemClock.Instance.GetCurrentInstant());

                await command.ExecuteNonQueryAsync();
            });
        }

        public async Task<FaziletPrayerTimes> GetTimesByDateAndCityID(LocalDate date, int cityId)
        {
            FaziletPrayerTimes time = null;

            await _db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText = """
                    SELECT Imsak, Fajr, Shuruq, Dhuhr, Asr, Maghrib, Isha, Date
                    FROM FaziletPrayerTimes
                    WHERE CityId = $CityId AND Date = $Date;
                    """;

                command.Parameters.AddWithValue("$CityId", cityId);
                command.Parameters.AddWithValue("$Date", date.GetStringForDBColumn());

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        time = new FaziletPrayerTimes
                        {
                            CityID = cityId,
                            Imsak = reader.GetString(0).GetZonedDateTimeFromDBColumnString(),
                            Fajr = reader.GetString(1).GetZonedDateTimeFromDBColumnString(),
                            Shuruq = reader.GetString(2).GetZonedDateTimeFromDBColumnString(),
                            Dhuhr = reader.GetString(3).GetZonedDateTimeFromDBColumnString(),
                            Asr = reader.GetString(4).GetZonedDateTimeFromDBColumnString(),
                            Maghrib = reader.GetString(5).GetZonedDateTimeFromDBColumnString(),
                            Isha = reader.GetString(6).GetZonedDateTimeFromDBColumnString(),
                            Date = reader.GetString(7).GetLocalDateFromDBColumnString()
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
                    command.CommandText = """
                        INSERT INTO FaziletCountries (Id, Name, InsertInstant) 
                        VALUES ($Id, $Name, $InsertInstant);
                        """;

                    command.Parameters.AddWithValue("$Id", country.Value);
                    command.Parameters.AddWithValue("$Name", country.Key);
                    command.Parameters.AddWithValue("$InsertInstant", SystemClock.Instance.GetCurrentInstant().GetStringForDBColumn());

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
                    command.CommandText = """
                        INSERT INTO FaziletCities (Id, Name, CountryId, InsertInstant) 
                        VALUES ($Id, $Name, $CountryId, $InsertInstant);
                        """;

                    command.Parameters.AddWithValue("$Id", city.Value);
                    command.Parameters.AddWithValue("$Name", city.Key);
                    command.Parameters.AddWithValue("$CountryId", countryId);
                    command.Parameters.AddWithValue("$InsertInstant", SystemClock.Instance.GetCurrentInstant().GetStringForDBColumn());

                    await command.ExecuteNonQueryAsync();
                }
            });
        }

        public async Task InsertFaziletPrayerTimes(LocalDate date, int cityID, FaziletPrayerTimes faziletPrayerTimes)
        {
            await _db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText = """
                    INSERT INTO FaziletPrayerTimes (Date, CityId, Imsak, Fajr, Shuruq, Dhuhr, Asr, Maghrib, Isha, InsertInstant) 
                    VALUES ($Date, $CityId, $Imsak, $Fajr, $Shuruq, $Dhuhr, $Asr, $Maghrib, $Isha, $InsertInstant);
                    """;

                command.Parameters.AddWithValue("$Date", date.GetStringForDBColumn());
                command.Parameters.AddWithValue("$CityId", cityID);

                command.Parameters.AddWithValue("$Imsak", faziletPrayerTimes.Imsak.GetStringForDBColumn());
                command.Parameters.AddWithValue("$Fajr", faziletPrayerTimes.Fajr.GetStringForDBColumn());
                command.Parameters.AddWithValue("$Shuruq", faziletPrayerTimes.Shuruq.GetStringForDBColumn());
                command.Parameters.AddWithValue("$Dhuhr", faziletPrayerTimes.Dhuhr.GetStringForDBColumn());
                command.Parameters.AddWithValue("$Asr", faziletPrayerTimes.Asr.GetStringForDBColumn());
                command.Parameters.AddWithValue("$Maghrib", faziletPrayerTimes.Maghrib.GetStringForDBColumn());
                command.Parameters.AddWithValue("$Isha", faziletPrayerTimes.Isha.GetStringForDBColumn());
                command.Parameters.AddWithValue("$InsertInstant", SystemClock.Instance.GetCurrentInstant().GetStringForDBColumn());

                await command.ExecuteNonQueryAsync();
            });
        }

        public async Task DeleteAllTimes()
        {
            await _db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM FaziletPrayerTimes;";
                await command.ExecuteNonQueryAsync();
            });
        }
    }
}
