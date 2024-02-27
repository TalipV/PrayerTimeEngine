using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using PrayerTimeEngine.BenchmarkDotNet.Benchmarks;

namespace PrayerTimeEngine.BenchmarkDotNet
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Summary summary = BenchmarkRunner.Run<MuwaqqitPrayerTimeCalculatorBenchmark>();
        }
    }
}
