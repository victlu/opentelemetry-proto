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


|          Method | Version | ConfigSet |      Mean |     Error |    StdDev |    Median | MessageSize |    Gen 0 |   Gen 1 | Gen 2 | Allocated |
|---------------- |-------- |---------- |----------:|----------:|----------:|----------:|------------ |---------:|--------:|------:|----------:|
|     EncodeGauge |   0.9.0 |     Small | 105.56 us |  2.074 us |  2.130 us | 105.23 us |        9158 |  11.2305 |       - |     - |     46 KB |
|       EncodeSum |   0.9.0 |     Small | 110.32 us |  2.191 us |  1.942 us | 109.84 us |        9178 |  11.2305 |  1.4648 |     - |     46 KB |
|   EncodeSummary |   0.9.0 |     Small |  33.17 us |  0.625 us |  1.158 us |  32.79 us |        3118 |   5.0659 |       - |     - |     21 KB |
| EncodeHistogram |   0.9.0 |     Small | 190.57 us |  3.038 us |  2.693 us | 189.96 us |       24960 |  35.1563 | 10.7422 |     - |    159 KB |
|     DecodeGauge |   0.9.0 |     Small | 266.04 us |  1.983 us |  1.758 us | 265.59 us |        9158 |  67.3828 |  0.4883 |     - |    276 KB |
|       DecodeSum |   0.9.0 |     Small | 271.22 us |  2.994 us |  2.500 us | 270.81 us |        9178 |  67.3828 |  0.4883 |     - |    276 KB |
|   DecodeSummary |   0.9.0 |     Small |  90.24 us |  1.097 us |  0.972 us |  90.24 us |        3118 |  18.4326 |  0.4883 |     - |     76 KB |
| DecodeHistogram |   0.9.0 |     Small | 497.86 us |  4.463 us |  3.956 us | 496.87 us |       24960 | 112.3047 | 30.2734 |     - |    500 KB |
|     EncodeGauge |    HEAD |     Small | 112.11 us |  1.581 us |  1.401 us | 111.88 us |        9758 |  11.3525 |  1.4648 |     - |     47 KB |
|       EncodeSum |    HEAD |     Small | 110.38 us |  1.459 us |  1.293 us | 110.35 us |        9778 |  11.4746 |  0.1221 |     - |     47 KB |
|   EncodeSummary |    HEAD |     Small |  32.20 us |  0.561 us |  0.497 us |  32.14 us |        3238 |   5.1270 |       - |     - |     21 KB |
| EncodeHistogram |    HEAD |     Small | 283.95 us | 21.677 us | 63.915 us | 302.32 us |       25560 |  38.3301 |  9.5215 |     - |    160 KB |
|     DecodeGauge |    HEAD |     Small | 287.67 us |  5.510 us |  4.601 us | 287.51 us |        9758 |  67.3828 |  0.4883 |     - |    276 KB |
|       DecodeSum |    HEAD |     Small | 289.37 us |  4.668 us |  5.376 us | 288.56 us |        9778 |  67.3828 |  0.4883 |     - |    276 KB |
|   DecodeSummary |    HEAD |     Small |  93.65 us |  1.832 us |  2.508 us |  93.03 us |        3238 |  18.5547 |       - |     - |     76 KB |
| DecodeHistogram |    HEAD |     Small | 523.07 us |  8.982 us | 11.031 us | 520.46 us |       25560 | 110.3516 | 36.1328 |     - |    501 KB |
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
                numExemplars = 1,
                }),
        };

        public static string filename = $"{Path.GetTempPath()}{nameof(MetricBenchmark)}.bench";

        private long gaugeEncodeMessageSize;
        private long sumEncodeMessageSize;
        private long summaryEncodeMessageSize;
        private long histogramEncodeMessageSize;
        private long gaugeDecodeMessageSize;
        private long sumDecodeMessageSize;
        private long summaryDecodeMessageSize;
        private long histogramDecodeMessageSize;

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

            this.gaugeMessage = this.metricBench.EncodeGauge();
            this.sumMessage = this.metricBench.EncodeSum();
            this.summaryMessage = this.metricBench.EncodeSummary();
            this.histogramMessage = this.metricBench.EncodeHistogram();

            this.gaugeEncodeMessageSize = 0;
            this.sumEncodeMessageSize = 0;
            this.summaryEncodeMessageSize = 0;
            this.histogramEncodeMessageSize = 0;
            this.gaugeDecodeMessageSize = 0;
            this.sumDecodeMessageSize = 0;
            this.summaryDecodeMessageSize = 0;
            this.histogramDecodeMessageSize = 0;
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            Action<string, long> appendToFile = (key, value) =>
            {
                if (value > 0)
                {
                    var folderPath = string.Join("_", new string[] {
                        $"{nameof(MetricBenchmark)}.{nameof(MetricBench)}",
                        key,
                        "DefaultJob",
                        $"Version-{this.Version}",
                        $"ConfigSet-{this.ConfigSet}",
                    });

                    File.AppendAllText(filename, $"{folderPath}={value}{Environment.NewLine}");
                }
            };

            appendToFile(nameof(EncodeGauge), this.gaugeEncodeMessageSize);
            appendToFile(nameof(EncodeSum), this.sumEncodeMessageSize);
            appendToFile(nameof(EncodeSummary), this.summaryEncodeMessageSize);
            appendToFile(nameof(EncodeHistogram), this.histogramEncodeMessageSize);
            appendToFile(nameof(DecodeGauge), this.gaugeDecodeMessageSize);
            appendToFile(nameof(DecodeSum), this.sumDecodeMessageSize);
            appendToFile(nameof(DecodeSummary), this.summaryDecodeMessageSize);
            appendToFile(nameof(DecodeHistogram), this.histogramDecodeMessageSize);
        }

        [Benchmark]
        public byte[] EncodeGauge()
        {
            var bytes = this.metricBench.EncodeGauge();
            this.gaugeEncodeMessageSize = bytes.LongLength;
            return bytes;
        }

        [Benchmark]
        public byte[] EncodeSum()
        {
            var bytes = this.metricBench.EncodeSum();
            this.sumEncodeMessageSize = bytes.LongLength;
            return bytes;
        }

        [Benchmark]
        public byte[] EncodeSummary()
        {
            var bytes = this.metricBench.EncodeSummary();
            this.summaryEncodeMessageSize = bytes.LongLength;
            return bytes;
        }

        [Benchmark]
        public byte[] EncodeHistogram()
        {
            var bytes = this.metricBench.EncodeHistogram();
            this.histogramEncodeMessageSize = bytes.LongLength;
            return bytes;
        }

        [Benchmark]
        public List<String> DecodeGauge()
        {
            this.gaugeDecodeMessageSize = this.gaugeMessage.LongLength;
            return this.metricBench.Decode(this.gaugeMessage);
        }

        [Benchmark]
        public List<String> DecodeSum()
        {
            this.sumDecodeMessageSize = this.sumMessage.LongLength;
            return this.metricBench.Decode(this.sumMessage);
        }

        [Benchmark]
        public List<String> DecodeSummary()
        {
            this.summaryDecodeMessageSize = this.summaryMessage.LongLength;
            return this.metricBench.Decode(this.summaryMessage);
        }

        [Benchmark]
        public List<String> DecodeHistogram()
        {
            this.histogramDecodeMessageSize = this.histogramMessage.LongLength;
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
            public int numExemplars;
        }
    }
}
