using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models.Entities
{
    public class FaziletCountry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]   // IDs come from API
        public int ID { get; set; }
        public string Name { get; set; }

        public ICollection<FaziletCity> Cities { get; set; }
    }
}
