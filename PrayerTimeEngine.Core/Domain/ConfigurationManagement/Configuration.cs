using PrayerTimeEngine.Core.Domain.ProfileManagement.Models.Entities;

namespace PrayerTimeEngine.Core.Domain.ConfigurationManagement;
public class Configuration
{
    public ICollection<Profile> Profiles { get; set; } = [];
}
