using System.ComponentModel.DataAnnotations;

namespace PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models.Entities
{
    public class FaziletCountry
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }

        public ICollection<FaziletCity> Cities { get; set; }
    }
}
