using BenchmarkDotNet.Running;
using PrayerTimeEngine.BenchmarkDotNet.Benchmarks;

Type[] types = 
    [
        typeof(SemerkandPrayerTimeCalculatorBenchmark),
        typeof(FaziletPrayerTimeCalculatorBenchmark),
        typeof(MuwaqqitPrayerTimeCalculatorBenchmark)
    ];

BenchmarkSwitcher.FromTypes(types).Run(args);