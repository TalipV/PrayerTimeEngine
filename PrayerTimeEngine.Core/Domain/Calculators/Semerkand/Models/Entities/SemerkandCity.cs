﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrayerTimeEngine.Core.Domain.Calculators.Semerkand.Models.Entities
{
    public class SemerkandCity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]   // IDs come from API
        public int ID { get; set; }
        public string Name { get; set; }

        public int CountryID { get; set; }
        public SemerkandCountry Country { get; set; }
    }
}
