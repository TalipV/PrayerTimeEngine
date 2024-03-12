using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

namespace PrayerTimeEngine.BenchmarkDotNet
{
    public class BenchmarkConfig : ManualConfig
    {
        public BenchmarkConfig()
        {
            AddJob(Job.MediumRun);
            //this.WithOption(ConfigOptions.DisableOptimizationsValidator, true);
        }
    }
}
