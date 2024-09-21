using BenchmarkDotNet.Running;
using PrayerTimeEngine.BenchmarkDotNet.Benchmarks;

Type[] types = 
    [
        typeof(SemerkandDynamicPrayerTimeProviderBenchmark),
        typeof(FaziletDynamicPrayerTimeProviderBenchmark),
        typeof(MuwaqqitDynamicPrayerTimeProviderBenchmark),
        typeof(MawaqitMosquePrayerTimeProviderBenchmark),
        typeof(MyMosqMosquePrayerTimeProviderBenchmark),
    ];

BenchmarkSwitcher.FromTypes(types).Run(args);


/*

| Method                                            | Mean        | Error       | StdDev      |          |         | Allocated  |
|------------------------------------------------   |-----------: |------------:|------------:|--------: |-------: |----------: |
| FaziletDynamicPrayerTimeProvider_GetDataFromDb    |   336.2 us  |     3.23 us |     4.63 us |          |         | 258.68 KB  |
| FaziletDynamicPrayerTimeProvider_GetDataFromApi   | 5,967.2 us  | 1,140.41 us | 1,706.91 us |          |         | 136.05 KB  |
                                                                                                                                
| Method                                            | Mean        | Error       | StdDev      | Gen0     | Gen1    | Allocated  |
|-------------------------------------------------  |-----------: |----------:  |------------:|--------: |-------: |----------: |
| MuwaqqitDynamicPrayerTimeProvider_GetDataFromDb   |   448.4 us  |   4.54 us   |     6.50 us | 17.5781  | 3.9063  | 302.54 KB  |
| MuwaqqitDynamicPrayerTimeProvider_GetDataFromApi  | 5,256.3 us  | 970.78 us   | 1,453.02 us |  3.9063  |      -  |  79.25 KB  |
                                                                                
| Method                                            | Mean        | Error       | StdDev      | Gen0     | Gen1    | Allocated  |
|-------------------------------------------------- |-----------: |----------:  |----------:  |---------:|--------:|-----------:|
| SemerkandDynamicPrayerTimeProvider_GetDataFromDb  |   336.5 us  |   2.43 us   |   3.48 us   |  15.6250 |  3.9063 |   256.8 KB |
| SemerkandDynamicPrayerTimeProvider_GetDataFromApi | 3,627.1 us  | 503.05 us   | 752.94 us   | 140.6250 | 15.6250 | 2370.37 KB |
                                                                                
| Method                                            | Mean        | Error       | StdDev      |          |         | Allocated  |
|-----------------------------------------------    |------------:|----------:  |-----------: |--------: |-------: |----------: |
| MawaqitMosquePrayerTimeProvider_GetDataFromDb     |    89.80 us |  0.661 us   |   0.927 us  |          |         |  63.35 KB  |
| MawaqitMosquePrayerTimeProvider_GetDataFromApi    | 1,708.95 us | 98.337 us   | 147.187 us  |          |         | 2309.6 KB  |
                                                                                                                                
| Method                                            | Mean        | Error       | StdDev      |          |         | Allocated  |
|----------------------------------------------     |-----------: |----------:  |------------:|--------: |-------: |----------: |
| MyMosqMosquePrayerTimeProvider_GetDataFromDb      |   111.4 us  |   0.62 us   |     0.89 us |          |         |  68.04 KB  |
| MyMosqMosquePrayerTimeProvider_GetDataFromApi     | 2,778.2 us  | 815.31 us   | 1,220.32 us |          |         | 623.72 KB  |
                                                    
 * */