using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace MetricBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            File.Delete(MetricBench.filename);

            BenchmarkSwitcher
                .FromAssembly(typeof(Program).Assembly)
                .Run(args);
        }
    }
}
