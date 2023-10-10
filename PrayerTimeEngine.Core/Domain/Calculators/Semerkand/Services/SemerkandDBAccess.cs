using NodaTime;
using PrayerTimeEngine.Core.Common.Extension;
using PrayerTimeEngine.Core.Data.SQLite;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Interfaces;
using PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models;

namespace PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Services
{
    public class SemerkandDBAccess(
            ISQLiteDB db
        ) : ISemerkandDBAccess
    {
        public async Task<Dictionary<string, int>> GetCountries()
        {
            var countries = new Dictionary<string, int>();
            await db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText = """
                    SELECT Id, Name
                    FROM SemerkandCountries;
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
                    INSERT INTO SemerkandCountries (Id, Name, InsertInstant) 
                    VALUES ($Id, $Name, $InsertInstant);
                    """;

                command.Parameters.AddWithValue("$Id", id);
                command.Parameters.AddWithValue("$Name", name);
                command.Parameters.AddWithValue("$InsertInstant", SystemClock.Instance.GetCurrentInstant().GetStringForDBColumn());

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
                    FROM SemerkandCities
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
                    INSERT INTO SemerkandCities (Id, Name, CountryId, InsertInstant) 
                    VALUES ($Id, $Name, $CountryId, $InsertInstant);
                    """;

                command.Parameters.AddWithValue("$Id", id);
                command.Parameters.AddWithValue("$Name", name);
                command.Parameters.AddWithValue("$CountryId", countryId);
                command.Parameters.AddWithValue("$InsertInstant", SystemClock.Instance.GetCurrentInstant().GetStringForDBColumn());

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        public async Task<SemerkandPrayerTimes> GetTimesByDateAndCityID(LocalDate date, int cityId)
        {
            SemerkandPrayerTimes time = null;

            await db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText = """
                    SELECT Fajr, Shuruq, Dhuhr, Asr, Maghrib, Isha, Date
                    FROM SemerkandPrayerTimes
                    WHERE CityId = $CityId AND Date = $Date;
                    """;

                command.Parameters.AddWithValue("$CityId", cityId);
                command.Parameters.AddWithValue("$Date", date.GetStringForDBColumn());

                using (var reader = await command.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    if (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        time =
                        new SemerkandPrayerTimes
                        {
                            CityID = cityId,
                            Fajr = reader.GetString(0).GetZonedDateTimeFromDBColumnString(),
                            Shuruq = reader.GetString(1).GetZonedDateTimeFromDBColumnString(),
                            Dhuhr = reader.GetString(2).GetZonedDateTimeFromDBColumnString(),
                            Asr = reader.GetString(3).GetZonedDateTimeFromDBColumnString(),
                            Maghrib = reader.GetString(4).GetZonedDateTimeFromDBColumnString(),
                            Isha = reader.GetString(5).GetZonedDateTimeFromDBColumnString(),
                            Date = reader.GetString(6).GetLocalDateFromDBColumnString()
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
                        INSERT INTO SemerkandCountries (Id, Name, InsertInstant) 
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
                        INSERT INTO SemerkandCities (Id, Name, CountryId, InsertInstant) 
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

        public async Task InsertSemerkandPrayerTimes(LocalDate date, int cityID, SemerkandPrayerTimes semerkandPrayerTimes)
        {
            await db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText = """
                    INSERT OR IGNORE INTO SemerkandPrayerTimes (Date, CityId, Fajr, Shuruq, Dhuhr, Asr, Maghrib, Isha, InsertInstant) 
                    VALUES ($Date, $CityId, $Fajr, $Shuruq, $Dhuhr, $Asr, $Maghrib, $Isha, $InsertInstant);
                    """;

                command.Parameters.AddWithValue("$Date", date.GetStringForDBColumn());
                command.Parameters.AddWithValue("$CityId", cityID);

                command.Parameters.AddWithValue("$Fajr", semerkandPrayerTimes.Fajr.GetStringForDBColumn());
                command.Parameters.AddWithValue("$Shuruq", semerkandPrayerTimes.Shuruq.GetStringForDBColumn());
                command.Parameters.AddWithValue("$Dhuhr", semerkandPrayerTimes.Dhuhr.GetStringForDBColumn());
                command.Parameters.AddWithValue("$Asr", semerkandPrayerTimes.Asr.GetStringForDBColumn());
                command.Parameters.AddWithValue("$Maghrib", semerkandPrayerTimes.Maghrib.GetStringForDBColumn());
                command.Parameters.AddWithValue("$Isha", semerkandPrayerTimes.Isha.GetStringForDBColumn());
                command.Parameters.AddWithValue("$InsertInstant", SystemClock.Instance.GetCurrentInstant().GetStringForDBColumn());

                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        public async Task DeleteAllPrayerTimes()
        {
            await db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM SemerkandPrayerTimes;";
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }
}
