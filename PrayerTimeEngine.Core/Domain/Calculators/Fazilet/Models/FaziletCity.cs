using System.ComponentModel.DataAnnotations;

namespace PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models
{
    public class FaziletCity
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }

        public int CountryID { get; set; }
        public FaziletCountry Country { get; set; }
    }
}
