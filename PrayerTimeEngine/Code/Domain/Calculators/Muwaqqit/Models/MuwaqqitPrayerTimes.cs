﻿using PrayerTimeEngine.Code.Common.Enum;
using PrayerTimeEngine.Code.Domain.CalculationService.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrayerTimeEngine.Code.Domain.Calculator.Muwaqqit.Models
{
    public class MuwaqqitPrayerTimes : ICalculationPrayerTimes
    {
        public required DateTime Date { get; set; }
        public required decimal Longitude { get; set; }
        public required decimal Latitude { get; set; }

        public required DateTime Fajr { get; set; }
        public required DateTime NextFajr { get; set; }
        public required DateTime Shuruq { get; set; }
        public required DateTime Duha { get; set; }
        public required DateTime Dhuhr { get; set; }
        public required DateTime AsrMithl { get; set; }
        public required DateTime AsrMithlayn { get; set; }
        public required DateTime Maghrib { get; set; }
        public required DateTime Isha { get; set; }
        public required DateTime Ishtibaq { get; set; }
        public required DateTime AsrKaraha { get; set; }

        public DateTime GetDateTimeForTimeType(ETimeType timeType)
        {
            switch (timeType)
            {
                case ETimeType.FajrStart:
                case ETimeType.FajrGhalas:
                case ETimeType.FajrKaraha:
                    return this.Fajr;
                case ETimeType.FajrEnd:
                    return this.Shuruq;
                case ETimeType.DuhaStart:
                    return this.Duha;
                case ETimeType.DhuhrStart:
                case ETimeType.DuhaEnd:
                    return this.Dhuhr;
                case ETimeType.DhuhrEnd:
                    return this.AsrMithl;
                case ETimeType.AsrStart:
                    return this.AsrMithl;
                case ETimeType.AsrEnd:
                    return this.Maghrib;
                case ETimeType.AsrMithlayn:
                    return this.AsrMithlayn;
                case ETimeType.AsrKaraha:
                    return this.AsrKaraha;
                case ETimeType.MaghribStart:
                    return this.Maghrib;
                case ETimeType.MaghribIshtibaq:
                    return this.Ishtibaq;
                case ETimeType.MaghribEnd:
                case ETimeType.IshaStart:
                    return this.Isha;
                case ETimeType.IshaEnd:
                    return this.NextFajr;
                default:
                    throw new ArgumentException($"Invalid {nameof(timeType)} value: {timeType}.");
            }
        }
    }
}
