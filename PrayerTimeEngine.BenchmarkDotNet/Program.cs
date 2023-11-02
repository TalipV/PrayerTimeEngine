using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

namespace PrayerTimeEngine.BenchmarkDotNet
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<MuwaqqitPrayerTimeCalculatorBenchmark>();
        }
    }
}
