using System.Text.Json;
using Microsoft.Data.Sqlite;
using NodaTime;
using PrayerTimeEngine.Core.Common.Enum;
using PrayerTimeEngine.Core.Common.Extension;
using PrayerTimeEngine.Core.Data.SQLite;
using PrayerTimeEngine.Core.Domain.Configuration.Interfaces;
using PrayerTimeEngine.Core.Domain.Configuration.Models;
using PrayerTimeEngine.Core.Domain.Model;

namespace PrayerTimeEngine.Core.Domain.Configuration.Services
{
    public class ConfigStoreDBAccess : IConfigStoreDBAccess
    {
        private readonly ISQLiteDB _db;

        public ConfigStoreDBAccess(ISQLiteDB db)
        {
            _db = db;
        }

        public async Task<List<Profile>> GetProfiles()
        {
            List<Profile> profiles = new();

            await _db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText =
                """
                SELECT Id, Name, LocationName, SequenceNo
                FROM Profile;
                """;

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Profile profile = new()
                        {
                            ID = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            LocationName = reader.GetString(2),
                            SequenceNo = reader.GetInt32(3),
                            Configurations = new(),
                            LocationDataByCalculationSource = new()
                        };

                        // Fetch configurations for the current profile
                        var configCommand = connection.CreateCommand();
                        configCommand.CommandText = """
                            SELECT TimeType, JsonConfigurationString
                            FROM TimeSpecificConfig
                            WHERE ProfileID = $ProfileId;
                            """;

                        configCommand.Parameters.AddWithValue("$ProfileId", profile.ID);

                        using (var configReader = await configCommand.ExecuteReaderAsync())
                        {
                            while (await configReader.ReadAsync())
                            {
                                ETimeType timeType = (ETimeType)configReader.GetInt32(0);
                                string jsonConfigurationString = configReader.GetString(1);

                                profile.Configurations[timeType] =
                                    JsonSerializer.Deserialize<GenericSettingConfiguration>(jsonConfigurationString);
                            }
                        }

                        // Fetch location data for the current profile
                        var locationDataCommand = connection.CreateCommand();
                        locationDataCommand.CommandText = """
                            SELECT CalculationSource, JsonLocationData
                            FROM LocationData
                            WHERE ProfileID = $ProfileId;
                            """;

                        locationDataCommand.Parameters.AddWithValue("$ProfileId", profile.ID);

                        using (var locationDataReader = await locationDataCommand.ExecuteReaderAsync())
                        {
                            while (await locationDataReader.ReadAsync())
                            {
                                ECalculationSource calculationSource = (ECalculationSource)locationDataReader.GetInt32(0);
                                string jsonLocationData = locationDataReader.GetString(1);

                                profile.LocationDataByCalculationSource[calculationSource] =
                                    JsonSerializer.Deserialize<BaseLocationData>(jsonLocationData);
                            }
                        }

                        profiles.Add(profile);
                    }
                }
            });

            return profiles;
        }


        public async Task<List<TimeSpecificConfig>> GetTimeSpecificConfigsByProfile(int profileID)
        {
            List<TimeSpecificConfig> timeSpecificConfigs = new();

            await _db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText = """
                    SELECT Id, ProfileID, TimeType, JsonConfigurationString
                    FROM TimeSpecificConfig
                    WHERE ProfileID = $ProfileId;
                    """;

                command.Parameters.AddWithValue("$ProfileId", profileID);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        TimeSpecificConfig timeSpecificConfig = new()
                        {
                            ID = reader.GetInt32(0),
                            ProfileID = reader.GetInt32(1),
                            TimeType = (ETimeType)reader.GetInt32(2)
                        };

                        string configurationTypeName = reader.GetString(3);
                        string jsonConfigurationString = reader.GetString(4);

                        timeSpecificConfig.CalculationConfiguration =
                            JsonSerializer.Deserialize<GenericSettingConfiguration>(jsonConfigurationString);

                        timeSpecificConfigs.Add(timeSpecificConfig);
                    }
                }
            });

            return timeSpecificConfigs;
        }

        public async Task SaveProfile(Profile profile)
        {
            await DeleteProfile(profile);

            await _db.ExecuteCommandAsync(async connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    await insertProfile(connection, profile);
                    await insertTimeSpecificConfigs(connection, profile);
                    await insertLocationData(connection, profile);

                    transaction.Commit();
                }
            });
        }

        private async Task insertProfile(SqliteConnection connection, Profile profile)
        {
            var command = connection.CreateCommand();
            command.CommandText = """
                INSERT INTO Profile (Id, Name, LocationName, SequenceNo, InsertInstant) 
                VALUES ($Id, $Name, $LocationName, $SequenceNo, $InsertInstant);
                """;

            command.Parameters.AddWithValue("$Id", profile.ID);
            command.Parameters.AddWithValue("$Name", profile.Name);
            command.Parameters.AddWithValue("$LocationName", profile.LocationName);
            command.Parameters.AddWithValue("$SequenceNo", profile.SequenceNo);
            command.Parameters.AddWithValue("$InsertInstant", SystemClock.Instance.GetCurrentInstant().GetStringForDBColumn());

            await command.ExecuteNonQueryAsync();
        }

        private async Task insertTimeSpecificConfigs(SqliteConnection connection, Profile profile)
        {
            foreach (var config in profile.Configurations)
            {
                if (config.Value == null)
                {
                    continue;
                }

                var configCommand = connection.CreateCommand();
                configCommand.CommandText = """
                    INSERT INTO TimeSpecificConfig (ProfileID, TimeType, JsonConfigurationString, InsertInstant) 
                    VALUES ($ProfileID, $TimeType, $JsonConfigurationString, $InsertInstant);
                    """;

                configCommand.Parameters.AddWithValue("$ProfileID", profile.ID);
                configCommand.Parameters.AddWithValue("$TimeType", (int)config.Key);
                configCommand.Parameters.AddWithValue("$JsonConfigurationString", JsonSerializer.Serialize(config.Value));
                configCommand.Parameters.AddWithValue("$InsertInstant", SystemClock.Instance.GetCurrentInstant().GetStringForDBColumn());

                await configCommand.ExecuteNonQueryAsync();
            }
        }

        private static async Task insertLocationData(SqliteConnection connection, Profile profile)
        {
            foreach (var locationData in profile.LocationDataByCalculationSource)
            {
                if (locationData.Value == null)
                {
                    continue;
                }

                var configCommand = connection.CreateCommand();
                configCommand.CommandText = """
                    INSERT INTO LocationData (ProfileID, CalculationSource, JsonLocationData, InsertInstant) 
                    VALUES ($ProfileID, $CalculationSource, $JsonLocationData, $InsertInstant);
                    """;

                configCommand.Parameters.AddWithValue("$ProfileID", profile.ID);
                configCommand.Parameters.AddWithValue("$CalculationSource", locationData.Value.Source);
                configCommand.Parameters.AddWithValue("$JsonLocationData", JsonSerializer.Serialize(locationData.Value));
                configCommand.Parameters.AddWithValue("$InsertInstant", SystemClock.Instance.GetCurrentInstant().GetStringForDBColumn());

                await configCommand.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteProfile(Profile profile)
        {
            await _db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText = """
                    DELETE FROM LocationData 
                    WHERE ProfileID = $ProfileID;

                    DELETE FROM TimeSpecificConfig 
                    WHERE ProfileID = $ProfileID;

                    DELETE FROM Profile 
                    WHERE ID = $ProfileID;
                    """;

                command.Parameters.AddWithValue("$ProfileID", profile.ID);
                await command.ExecuteNonQueryAsync();
            });
        }
    }
}
