using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrayerTimeEngine.Core.Domain.Calculators.Fazilet.Models.Entities
{
    public class FaziletCity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]   // IDs come frfaom API
        public int ID { get; set; }
        public string Name { get; set; }

        public int CountryID { get; set; }
        public FaziletCountry Country { get; set; }
    }
}
