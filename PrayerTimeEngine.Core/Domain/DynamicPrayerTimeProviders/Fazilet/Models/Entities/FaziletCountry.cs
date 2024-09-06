using NodaTime;
using PrayerTimeEngine.Core.Data.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrayerTimeEngine.Core.Domain.DynamicPrayerTimeProviders.Fazilet.Models.Entities
{
    public class FaziletCountry : IInsertedAt
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]   // IDs come from API
        public int ID { get; set; }
        public Instant? InsertInstant { get; set; }
        public string Name { get; set; }

        public ICollection<FaziletCity> Cities { get; set; }
    }
}
