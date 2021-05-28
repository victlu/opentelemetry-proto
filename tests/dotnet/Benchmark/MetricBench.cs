using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace MetricBenchmark
{
    public class MetricBench
    {
        [Params("0.8.0", "0.9.0")]
        public string Version { get; set; }

        [Params("Small")]
        public string ConfigSet { get; set; }

        private (string name, bool isDouble, int numResources, int numLabels, int numLibs, int numMetrics, int numDataPoints)[] configSet = {
            ( "Small", false, 3, 3, 1, 10, 10 ),
        };

        private IMetricBench metricBench;
        private byte[] gaugeMessage;

        [GlobalSetup]
        void Setup()
        {
            var config = this.configSet[0];
            foreach (var set in this.configSet)
            {
                if (this.ConfigSet == set.name)
                {
                    config = set;
                    break;
                }
            }

            switch (this.Version)
            {
                case "0.8.0":
                    this.metricBench = new MetricV080(config.isDouble, config.numResources, config.numLabels, config.numLibs, config.numMetrics, config.numDataPoints);
                    break;

                case "0.9.0":
                    this.metricBench = new MetricV090(config.isDouble, config.numResources, config.numLabels, config.numLibs, config.numMetrics, config.numDataPoints);
                    break;

                default:
                    throw new Exception("Bad Version");
            }

            this.gaugeMessage = this.metricBench.EncodeGauge();
        }

        [Benchmark]
        public byte[] EncodeGauge()
        {
            return this.metricBench?.EncodeGauge();
        }

        [Benchmark]
        public List<String> DecodeGauge()
        {
            return this.metricBench?.Decode(this.gaugeMessage);
        }
    }
}
