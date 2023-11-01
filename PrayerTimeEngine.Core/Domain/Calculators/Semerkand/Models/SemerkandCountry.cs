using System.ComponentModel.DataAnnotations;

namespace PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models
{
    public class SemerkandCountry
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }

        public ICollection<SemerkandCity> Cities { get; set; }
    }
}
