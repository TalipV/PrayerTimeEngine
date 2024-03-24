using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models.Entities
{
    public class SemerkandCountry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]   // IDs come from API
        public int ID { get; set; }
        public string Name { get; set; }

        public ICollection<SemerkandCity> Cities { get; set; }
    }
}
