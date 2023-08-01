using Newtonsoft.Json;
using PrayerTimeEngine.Common.Enum;
using PrayerTimeEngine.Domain.ConfigStore.Interfaces;
using PrayerTimeEngine.Domain.ConfigStore.Models;
using PrayerTimeEngine.Domain.Configuration.Interfaces;

namespace PrayerTimeEngine.Domain.ConfigStore.Services
{
    public class ConfigStoreDBAccess : IConfigStoreDBAccess
    {
        private readonly ISQLiteDB _db;
        private readonly IConfigurationSerializationService _configurationSerializationService;

        public ConfigStoreDBAccess(ISQLiteDB db, IConfigurationSerializationService configurationSerializationService)
        {
            _db = db;
            _configurationSerializationService = configurationSerializationService;
        }

        public async Task<List<Profile>> GetProfiles()
        {
            List<Profile> profiles = new List<Profile>();

            await _db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                SELECT Id, Name, SequenceNo
                FROM Profile";

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Profile profile = new Profile
                        {
                            ID = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            SequenceNo = reader.GetInt32(2),
                        };
                        profiles.Add(profile);
                    }
                }
            });

            return profiles;
        }

        public async Task<List<TimeSpecificConfig>> GetTimeSpecificConfigsByProfile(int profileID)
        {
            List<TimeSpecificConfig> timeSpecificConfigs = new List<TimeSpecificConfig>();

            await _db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                SELECT Id, ProfileID, TimeType, ConfigurationTypeName, JsonConfigurationString
                FROM TimeSpecificConfig
                WHERE ProfileID = $ProfileId;";

                command.Parameters.AddWithValue("$ProfileId", profileID);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        TimeSpecificConfig timeSpecificConfig = new TimeSpecificConfig
                        {
                            ID = reader.GetInt32(0),
                            ProfileID = reader.GetInt32(1),
                            TimeType = (ETimeType)reader.GetInt32(2)
                        };

                        string configurationTypeName = reader.GetString(3);
                        string jsonConfigurationString = reader.GetString(4);

                        timeSpecificConfig.CalculationConfiguration =
                            _configurationSerializationService.Deserialize(jsonConfigurationString, configurationTypeName);

                        timeSpecificConfigs.Add(timeSpecificConfig);
                    }
                }
            });

            return timeSpecificConfigs;
        }

        public async Task SaveProfile(Profile profile)
        {
            await DeleteProfile(profile.ID);

            await _db.ExecuteCommandAsync(async connection =>
            {
                using (var transaction = connection.BeginTransaction())
                {
                    var command = connection.CreateCommand();
                    command.CommandText =
                    @"
                    INSERT INTO Profile (Id, Name, SequenceNo, InsertDateTime) 
                    VALUES ($Id, $Name, $SequenceNo, $InsertDateTime);";

                    command.Parameters.AddWithValue("$Id", profile.ID);
                    command.Parameters.AddWithValue("$Name", profile.Name);
                    command.Parameters.AddWithValue("$SequenceNo", profile.SequenceNo);
                    command.Parameters.AddWithValue("$InsertDateTime", DateTime.Now);

                    await command.ExecuteNonQueryAsync();

                    foreach (var config in profile.Configurations)
                    {
                        if (config.Value == null)
                        {
                            continue;
                        }

                        var configCommand = connection.CreateCommand();
                        configCommand.CommandText =
                        @"
                        INSERT INTO TimeSpecificConfig (ProfileID, TimeType, ConfigurationTypeName, JsonConfigurationString, InsertDateTime) 
                        VALUES ($ProfileID, $TimeType, $ConfigurationTypeName, $JsonConfigurationString, $InsertDateTime);";

                        configCommand.Parameters.AddWithValue("$ProfileID", profile.ID);
                        configCommand.Parameters.AddWithValue("$TimeType", (int)config.Key);
                        configCommand.Parameters.AddWithValue("$ConfigurationTypeName", _configurationSerializationService.GetDiscriminator(config.Value.GetType()));
                        configCommand.Parameters.AddWithValue("$JsonConfigurationString", JsonConvert.SerializeObject(config.Value));
                        configCommand.Parameters.AddWithValue("$InsertDateTime", DateTime.Now);

                        await configCommand.ExecuteNonQueryAsync();
                    }

                    transaction.Commit();
                }
            });
        }


        public async Task DeleteProfile(int profileID)
        {
            await _db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                DELETE FROM LocationData 
                WHERE ProfileID = $ProfileID;

                DELETE FROM TimeSpecificConfig 
                WHERE ProfileID = $ProfileID;

                DELETE FROM Profile 
                WHERE ID = $ProfileID;";

                command.Parameters.AddWithValue("$ProfileID", profileID);
                await command.ExecuteNonQueryAsync();
            });
        }
    }
}
