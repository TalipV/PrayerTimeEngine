using BenchmarkDotNet.Running;
using PrayerTimeEngine.BenchmarkDotNet.Benchmarks;

Type[] types = 
    [
        typeof(SemerkandDynamicPrayerTimeProviderBenchmark),
        typeof(FaziletDynamicPrayerTimeProviderBenchmark),
        typeof(MuwaqqitDynamicPrayerTimeProviderBenchmark)
    ];

BenchmarkSwitcher.FromTypes(types).Run(args);