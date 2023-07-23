﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrayerTimeEngine.Code.Common.Enums;
using PrayerTimeEngine.Code.Domain.ConfigStore.Interfaces;
using PrayerTimeEngine.Code.Domain.ConfigStore.Models;
using PrayerTimeEngine.Code.Interfaces;

namespace PrayerTimeEngine.Code.Domain.ConfigStore.Services
{
    public class ConfigStoreService : IConfigStoreService
    {
        IConfigStoreDBAccess _configStoreDBAccess;

        public ConfigStoreService(IConfigStoreDBAccess configStoreDBAccess) 
        {
            _configStoreDBAccess = configStoreDBAccess;
        }

        public async Task<List<Profile>> GetProfiles()
        {
            List<Profile> profiles = await _configStoreDBAccess.GetProfiles();

            foreach (Profile profile in profiles)
            {
                profile.Configurations = 
                    (await _configStoreDBAccess.GetTimeSpecificConfigsByProfile(profile.ID))
                        .ToDictionary(x => (x.PrayerTime, x.PrayerTimeEvent), x => x.CalculationConfiguration);
            }

            return profiles;
        }

        public async Task SaveProfiles(List<Profile> profiles)
        {
            foreach (Profile profile in profiles)
            {
                await _configStoreDBAccess.SaveProfile(profile);
            }
        }
    }
}
