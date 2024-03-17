using System.ComponentModel.DataAnnotations;

namespace PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models.Entities
{
    public class SemerkandCity
    {
        [Key]
        public int ID { get; set; }
        public string Name { get; set; }

        public int CountryID { get; set; }
        public SemerkandCountry Country { get; set; }
    }
}
