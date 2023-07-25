using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using PrayerTimeEngine.Code.Domain.ConfigStore.Interfaces;
using PrayerTimeEngine.Code.Domain.ConfigStore.Models;
using PrayerTimeEngine.Code.Domain.Model;

namespace PrayerTimeEngine.Code.Domain.ConfigStore.Services
{
    public class ConfigStoreDBAccess : IConfigStoreDBAccess
    {
        private ISQLiteDB _db;

        public ConfigStoreDBAccess(ISQLiteDB db) 
        { 
            this._db = db;
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
                SELECT Id, ProfileID, PrayerTime, PrayerTimeEvent, ConfigurationTypeName, JsonConfigurationString
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
                            PrayerTime = (EPrayerTime)reader.GetInt32(2),
                            PrayerTimeEvent = (EPrayerTimeEvent)reader.GetInt32(3),
                        };

                        string configurationTypeName = reader.GetString(4);
                        string jsonConfigurationString = reader.GetString(5);

                        timeSpecificConfig.CalculationConfiguration =
                            BaseCalculationConfiguration.GetCalculationConfigurationFromJsonString(jsonConfigurationString, configurationTypeName);

                        timeSpecificConfigs.Add(timeSpecificConfig);
                    }
                }
            });

            return timeSpecificConfigs;
        }

        public async Task<List<ILocationConfig>> GetLocationConfigsByProfile(int profileID)
        {
            List<ILocationConfig> timeSpecificConfigs = new List<ILocationConfig>();

            await _db.ExecuteCommandAsync(async connection =>
            {
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                SELECT Id, ProfileID, LocationConfigTypeName, JsonLocationInfo
                FROM LocationConfig
                WHERE ProfileID = $ProfileId;";

                command.Parameters.AddWithValue("$ProfileId", profileID);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        throw new NotImplementedException();
                        //TimeSpecificConfig timeSpecificConfig = new TimeSpecificConfig
                        //{
                        //    ID = reader.GetInt32(0),
                        //    ProfileID = reader.GetInt32(1),
                        //    PrayerTime = (EPrayerTime)reader.GetInt32(2),
                        //    PrayerTimeEvent = (EPrayerTimeEvent)reader.GetInt32(3),
                        //    ConfigurationTypeName = reader.GetString(4),
                        //    JsonConfigurationString = reader.GetString(5),
                        //};
                        //timeSpecificConfigs.Add(timeSpecificConfig);
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
                        INSERT INTO TimeSpecificConfig (ProfileID, PrayerTime, PrayerTimeEvent, ConfigurationTypeName, JsonConfigurationString, InsertDateTime) 
                        VALUES ($ProfileID, $PrayerTime, $PrayerTimeEvent, $ConfigurationTypeName, $JsonConfigurationString, $InsertDateTime);";

                        configCommand.Parameters.AddWithValue("$ProfileID", profile.ID);
                        configCommand.Parameters.AddWithValue("$PrayerTime", (int)config.Key.Item1);
                        configCommand.Parameters.AddWithValue("$PrayerTimeEvent", (int)config.Key.Item2);
                        configCommand.Parameters.AddWithValue("$ConfigurationTypeName", BaseCalculationConfiguration.GetDiscriminatorForConfigurationType(config.Value.GetType()));
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
