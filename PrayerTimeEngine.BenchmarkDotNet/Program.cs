using BenchmarkDotNet.Running;
using PrayerTimeEngine.BenchmarkDotNet.Benchmarks;

namespace PrayerTimeEngine.BenchmarkDotNet
{
    internal class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<SemerkandPrayerTimeCalculatorBenchmark>();
            //BenchmarkRunner.Run<FaziletPrayerTimeCalculatorBenchmark>();
            //BenchmarkRunner.Run<MuwaqqitPrayerTimeCalculatorBenchmark>();
        }
    }
}
