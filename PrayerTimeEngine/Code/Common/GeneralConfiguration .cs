﻿using PrayerTimeEngine.Code.Common.Enums;
using PrayerTimeEngine.Code.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Code.Common
{
    public class GeneralConfiguration : BaseCalculationConfiguration
    {
        public GeneralConfiguration(int minuteAdjustment) : base(minuteAdjustment)
        {
        }

        public override ECalculationSource Source => ECalculationSource.None;
    }
}
