using BenchmarkDotNet.Running;

namespace PrayerTimeEngine.BenchmarkDotNet
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<ProfileServiceBenchmark>();
        }
    }
}
