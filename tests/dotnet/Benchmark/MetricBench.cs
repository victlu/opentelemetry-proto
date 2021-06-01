using System;
using System.Collections.Generic;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

/*
BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19043.1023 (21H1/May2021Update)
Intel Core i7-1065G7 CPU 1.30GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=5.0.202
  [Host]     : .NET 5.0.5 (5.0.521.16609), X64 RyuJIT
  DefaultJob : .NET 5.0.5 (5.0.521.16609), X64 RyuJIT


|          Method | Version | ConfigSet |      Mean |    Error |    StdDev |    Median | MessageSize |
|---------------- |-------- |---------- |----------:|---------:|----------:|----------:|------------ |
|     EncodeGauge |   0.9.0 |     Small | 112.80 us | 3.220 us |  9.341 us | 110.26 us |        9156 |
|       EncodeSum |   0.9.0 |     Small | 103.51 us | 2.045 us |  4.131 us | 101.89 us |        9176 |
|   EncodeSummary |   0.9.0 |     Small |  31.57 us | 0.622 us |  1.404 us |  30.89 us |        3116 |
| EncodeHistogram |   0.9.0 |     Small | 108.90 us | 2.164 us |  4.118 us | 107.31 us |        8256 |
|     DecodeGauge |   0.9.0 |     Small | 295.24 us | 7.771 us | 22.912 us | 288.27 us |        9156 |
|       DecodeSum |   0.9.0 |     Small |  61.21 us | 1.127 us |  0.999 us |  60.87 us |        9176 |
|   DecodeSummary |   0.9.0 |     Small | 102.50 us | 3.220 us |  9.291 us |  99.81 us |        3116 |
| DecodeHistogram |   0.9.0 |     Small | 345.89 us | 7.918 us | 22.972 us | 338.98 us |        8256 |
|     EncodeGauge |    HEAD |     Small | 123.79 us | 2.814 us |  8.029 us | 120.99 us |        9756 |
|       EncodeSum |    HEAD |     Small | 115.41 us | 2.221 us |  4.005 us | 114.15 us |        9776 |
|   EncodeSummary |    HEAD |     Small |  35.01 us | 0.635 us |  1.296 us |  34.72 us |        3236 |
| EncodeHistogram |    HEAD |     Small | 119.33 us | 2.350 us |  4.801 us | 117.54 us |        8856 |
|     DecodeGauge |    HEAD |     Small | 298.17 us | 5.916 us |  5.244 us | 297.77 us |        9756 |
|       DecodeSum |    HEAD |     Small |  72.74 us | 1.931 us |  5.634 us |  71.26 us |        9776 |
|   DecodeSummary |    HEAD |     Small |  98.36 us | 1.955 us |  4.373 us |  96.77 us |        3236 |
| DecodeHistogram |    HEAD |     Small | 341.06 us | 6.723 us |  8.503 us | 339.37 us |        8856 |
*/

namespace MetricBenchmark
{
    [Config(typeof(MyConfig))]
    public class MetricBench
    {
        public class MyConfig : ManualConfig
        {
            public MyConfig()
            {
                AddColumn(new SizeColumn("MessageSize", MetricBench.filename, (name) =>
                {
                    return $"{name} B";
                }));
            }
        }

        [Params("HEAD", "0.9.0")]
        public string Version { get; set; }

        [Params("Small")]
        public string ConfigSet { get; set; }

        private (string name, MetricBenchConfig config)[] configSet = {
            ( "Small", new MetricBenchConfig {
                isDouble = false,
                numResources = 3,
                numLabels = 3,
                numLibs = 1,
                numDataPoints = 10,
                numMetrics = 10,
                numTimeseries = 2,
                numQV = 4,
                }),
        };

        public static string filename = $"{Path.GetTempPath()}{nameof(MetricBenchmark)}.bench";

        private readonly Dictionary<string, long> columnValues = new Dictionary<string, long>();

        private IMetricBench metricBench;
        private byte[] gaugeMessage;
        private byte[] sumMessage;
        private byte[] summaryMessage;
        private byte[] histogramMessage;

        [GlobalSetup]
        public void Setup()
        {
            var config = this.configSet[0].config;
            foreach (var set in this.configSet)
            {
                if (this.ConfigSet == set.name)
                {
                    config = set.config;
                    break;
                }
            }

            switch (this.Version)
            {
                case "HEAD":
                    this.metricBench = new MetricHEAD(config);
                    break;

                case "0.9.0":
                    this.metricBench = new MetricV090(config);
                    break;

                default:
                    throw new Exception("Bad Version");
            }

            this.columnValues.Clear();

            this.gaugeMessage = this.metricBench.EncodeGauge();
            this.sumMessage = this.metricBench.EncodeSum();
            this.summaryMessage = this.metricBench.EncodeSummary();
            this.histogramMessage = this.metricBench.EncodeHistogram();
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            foreach (var value in this.columnValues)
            {
                var folderPath = string.Join("_", new string[] {
                    $"{nameof(MetricBenchmark)}.{nameof(MetricBench)}",
                    value.Key,
                    "DefaultJob",
                    $"Version-{this.Version}",
                    $"ConfigSet-{this.ConfigSet}",
                });

                File.AppendAllText(filename, $"{folderPath}={value.Value}{Environment.NewLine}");
            }
        }

        [Benchmark]
        public byte[] EncodeGauge()
        {
            var bytes = this.metricBench.EncodeGauge();
            this.columnValues[nameof(EncodeGauge)] = bytes.LongLength;
            return bytes;
        }

        [Benchmark]
        public byte[] EncodeSum()
        {
            var bytes = this.metricBench.EncodeSum();
            this.columnValues[nameof(EncodeSum)] = bytes.LongLength;
            return bytes;
        }

        [Benchmark]
        public byte[] EncodeSummary()
        {
            var bytes = this.metricBench.EncodeSummary();
            this.columnValues[nameof(EncodeSummary)] = bytes.LongLength;
            return bytes;
        }

        [Benchmark]
        public byte[] EncodeHistogram()
        {
            var bytes = this.metricBench.EncodeHistogram();
            this.columnValues[nameof(EncodeHistogram)] = bytes.LongLength;
            return bytes;
        }

        [Benchmark]
        public List<String> DecodeGauge()
        {
            this.columnValues[nameof(DecodeGauge)] = this.gaugeMessage.LongLength;
            return this.metricBench.Decode(this.gaugeMessage);
        }

        [Benchmark]
        public List<String> DecodeSum()
        {
            this.columnValues[nameof(DecodeSum)] = this.sumMessage.LongLength;
            return this.metricBench.Decode(this.sumMessage);
        }

        [Benchmark]
        public List<String> DecodeSummary()
        {
            this.columnValues[nameof(DecodeSummary)] = this.summaryMessage.LongLength;
            return this.metricBench.Decode(this.summaryMessage);
        }

        [Benchmark]
        public List<String> DecodeHistogram()
        {
            this.columnValues[nameof(DecodeHistogram)] = this.histogramMessage.LongLength;
            return this.metricBench.Decode(this.histogramMessage);
        }

        public struct MetricBenchConfig
        {
            public bool isDouble;
            public int numResources;
            public int numLabels;
            public int numLibs;
            public int numMetrics;
            public int numDataPoints;
            public int numTimeseries;
            public int numQV;
        }
    }
}
