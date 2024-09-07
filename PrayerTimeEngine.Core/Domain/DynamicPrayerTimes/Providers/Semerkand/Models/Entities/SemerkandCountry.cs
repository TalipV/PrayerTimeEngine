using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimes.Providers.Semerkand.Models.Entities;

public class SemerkandCountry : IInsertedAt
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]   // IDs come from API
    public int ID { get; set; }
    public Instant? InsertInstant { get; set; }
    public string Name { get; set; }

    public ICollection<SemerkandCity> Cities { get; set; }
}
