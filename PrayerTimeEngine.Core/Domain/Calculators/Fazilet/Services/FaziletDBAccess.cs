using NodaTime;
using PrayerTimeEngine.Core.Common.Extension;
using PrayerTimeEngine.Core.Data.SQLite;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models;

namespace PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Services
{
    public class FaziletDBAccess(
            ISQLiteDB db
        ) : IFaziletDBAccess
    {
        public async Task<Dictionary<string, int>> GetCountries()
        {
            var countries = new Dictionary<string, int>();
            await db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText = """
                    SELECT Id, Name
                    FROM FaziletCountries;
                    """;

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        countries.Add(reader.GetString(1), reader.GetInt32(0));
                    }
                }
            }).ConfigureAwait(false);

            return countries;
        }

        public async Task InsertCountry(int id, string name)
        {
            await db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText = """
                    INSERT INTO FaziletCountries (Id, Name, InsertInstant) 
                    VALUES ($Id, $Name, $InsertInstant);
                    """;

                command.Parameters.AddWithValue("$Id", id);
                command.Parameters.AddWithValue("$Name", name);
                command.Parameters.AddWithValue("$InsertInstant", SystemClock.Instance.GetCurrentInstant());

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        public async Task<Dictionary<string, int>> GetCitiesByCountryID(int countryId)
        {
            var cities = new Dictionary<string, int>();
            await db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText = """
                    SELECT Id, Name
                    FROM FaziletCities
                    WHERE CountryId = $CountryId;
                    """;

                command.Parameters.AddWithValue("$CountryId", countryId);

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        cities.Add(reader.GetString(1), reader.GetInt32(0));
                    }
                }
            }).ConfigureAwait(false);

            return cities;
        }

        public async Task InsertCity(int id, string name, int countryId)
        {
            await db.ExecuteCommandAsync(async connection =>
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

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        public async Task<FaziletPrayerTimes> GetTimesByDateAndCityID(LocalDate date, int cityId)
        {
            FaziletPrayerTimes time = null;

            await db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText = """
                    SELECT Imsak, Fajr, Shuruq, Dhuhr, Asr, Maghrib, Isha, Date
                    FROM FaziletPrayerTimes
                    WHERE CityId = $CityId AND Date = $Date;
                    """;

                command.Parameters.AddWithValue("$CityId", cityId);
                command.Parameters.AddWithValue("$Date", date.GetStringForDBColumn());

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    if (await reader.ReadAsync().ConfigureAwait(false))
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
            }).ConfigureAwait(false);

            return time;
        }

        public async Task InsertCountries(Dictionary<string, int> countries)
        {
            await db.ExecuteCommandAsync(async connection =>
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

                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }).ConfigureAwait(false);
        }

        public async Task InsertCities(Dictionary<string, int> cities, int countryId)
        {
            await db.ExecuteCommandAsync(async connection =>
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

                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }).ConfigureAwait(false);
        }

        public async Task InsertFaziletPrayerTimesIfNotExists(LocalDate date, int cityID, FaziletPrayerTimes faziletPrayerTimes)
        {
            await db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText = """
                    INSERT OR IGNORE INTO FaziletPrayerTimes (Date, CityId, Imsak, Fajr, Shuruq, Dhuhr, Asr, Maghrib, Isha, InsertInstant) 
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

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        public async Task DeleteAllTimes()
        {
            await db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM FaziletPrayerTimes;";
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }
}
