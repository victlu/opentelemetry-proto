using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Opentelemetry.HEAD.Proto.Collector.Metrics.V1;
using Opentelemetry.HEAD.Proto.Common.V1;
using Opentelemetry.HEAD.Proto.Metrics.V1;
using Opentelemetry.HEAD.Proto.Resource.V1;

namespace MetricBenchmark
{
    public class MetricHEAD : IMetricBench
    {
        private (string name, object value)[] resources;
        private (string name, string value)[] labels;
        private MetricBench.MetricBenchConfig config;

        public MetricHEAD(MetricBench.MetricBenchConfig config)
        {
            this.config = config;

            this.resources = new (string name, object value)[this.config.numResources];
            for (int i = 0; i < this.config.numResources; i++)
            {
                this.resources[i] = ( $"Resource{i}", $"Value{i}");
            }

            this.labels = new (string name, string value)[this.config.numResources];
            for (int i = 0; i < this.config.numResources; i++)
            {
                this.labels[i] = ( $"Label{i}", $"Value{i}");
            }
        }

        public byte[] EncodeGauge()
        {
            DateTimeOffset dt = DateTimeOffset.UtcNow;

            var resmetric = new ResourceMetrics();
            resmetric.Resource = this.BuildResource();

            for (int lib = 0; lib < this.config.numLibs; lib++)
            {
                var instMetric = new InstrumentationLibraryMetrics();

                instMetric.InstrumentationLibrary = new InstrumentationLibrary();
                instMetric.InstrumentationLibrary.Name = $"Library{lib}";
                instMetric.InstrumentationLibrary.Version = "1.0.0";

                for (int m = 0; m < this.config.numMetrics; m++)
                {
                    Metric metric = new Metric();
                    metric.Name = $"Metric_{m}";

                    var attribs = this.BuildAttributes();

                    var gauge = new Gauge();
                    metric.Gauge = gauge;

                    for (int dp = 0; dp < this.config.numDataPoints; dp++)
                    {
                        var datapoint = new NumberDataPoint();
                        datapoint.Flags = (1<<7) | (1<<29);

                        datapoint.StartTimeUnixNano = (ulong)dt.ToUnixTimeMilliseconds() * 100000;
                        datapoint.TimeUnixNano = (ulong)dt.ToUnixTimeMilliseconds() * 100000;
                        datapoint.Attributes.AddRange(attribs);
                        datapoint.Flags = (1<<7) | (1<<29);

                        if (this.config.isDouble)
                        {
                            datapoint.AsDouble = (double)(dp + 100.1);
                        }
                        else
                        {
                            datapoint.AsInt = (long)(dp + 100);
                        }

                        gauge.DataPoints.Add(datapoint);
                    }

                    instMetric.Metrics.Add(metric);
                }

                resmetric.InstrumentationLibraryMetrics.Add(instMetric);
            }

            var request = new ExportMetricsServiceRequest();
            request.ResourceMetrics.Add(resmetric);
            var bytes = request.ToByteArray();
            return bytes;
        }

        public byte[] EncodeSum()
        {
            DateTimeOffset dt = DateTimeOffset.UtcNow;

            var resmetric = new ResourceMetrics();
            resmetric.Resource = this.BuildResource();

            for (int lib = 0; lib < this.config.numLibs; lib++)
            {
                var instMetric = new InstrumentationLibraryMetrics();

                instMetric.InstrumentationLibrary = new InstrumentationLibrary();
                instMetric.InstrumentationLibrary.Name = $"Library{lib}";
                instMetric.InstrumentationLibrary.Version = "1.0.0";

                for (int m = 0; m < this.config.numMetrics; m++)
                {
                    Metric metric = new Metric();
                    metric.Name = $"Metric_{m}";

                    var attribs = this.BuildAttributes();

                    metric.Sum = new Sum();
                    var sum = metric.Sum;
                    sum.IsMonotonic = true;

                    for (int dp = 0; dp < this.config.numDataPoints; dp++)
                    {
                        var datapoint = new NumberDataPoint();

                        datapoint.StartTimeUnixNano = (ulong)dt.ToUnixTimeMilliseconds() * 100000;
                        datapoint.TimeUnixNano = (ulong)dt.ToUnixTimeMilliseconds() * 100000;
                        datapoint.Attributes.AddRange(attribs);
                        datapoint.Flags = (1<<7) | (1<<29);

                        if (this.config.isDouble)
                        {
                            datapoint.AsDouble = (double)(dp + 100.1);
                        }
                        else
                        {
                            datapoint.AsInt = (long)(dp + 100);
                        }

                        sum.DataPoints.Add(datapoint);
                    }

                    instMetric.Metrics.Add(metric);
                }

                resmetric.InstrumentationLibraryMetrics.Add(instMetric);
            }

            var request = new ExportMetricsServiceRequest();
            request.ResourceMetrics.Add(resmetric);
            var bytes = request.ToByteArray();
            return bytes;
        }

        public byte[] EncodeSummary()
        {
            DateTimeOffset dt = DateTimeOffset.UtcNow;

            var resmetric = new ResourceMetrics();
            resmetric.Resource = this.BuildResource();

            for (int lib = 0; lib < this.config.numLibs; lib++)
            {
                var instMetric = new InstrumentationLibraryMetrics();

                instMetric.InstrumentationLibrary = new InstrumentationLibrary();
                instMetric.InstrumentationLibrary.Name = $"Library{lib}";
                instMetric.InstrumentationLibrary.Version = "1.0.0";

                for (int m = 0; m < this.config.numMetrics; m++)
                {
                    Metric metric = new Metric();

                    metric.Name = $"Metric_{m}";
                    metric.Summary = new Summary();

                    var attribs = this.BuildAttributes();

                    for (int tsx = 0; tsx < this.config.numTimeseries; tsx++)
                    {
                        var datapoint = new SummaryDataPoint();

                        datapoint.StartTimeUnixNano = (ulong)dt.ToUnixTimeMilliseconds() * 100000;
                        datapoint.TimeUnixNano = (ulong)dt.ToUnixTimeMilliseconds() * 100000;
                        datapoint.Attributes.AddRange(attribs);
                        datapoint.Flags = (1<<7) | (1<<29);

                        datapoint.Count = 1;
                        datapoint.Sum = tsx + 100.1;

                        for (int qv = 0; qv < this.config.numQV; qv++)
                        {
                            var qvalues = new SummaryDataPoint.Types.ValueAtQuantile();
                            qvalues.Quantile = qv * (1 / this.config.numQV);
                            qvalues.Value = 1.0;

                            datapoint.QuantileValues.Add(qvalues);
                        }

                        metric.Summary.DataPoints.Add(datapoint);
                    }

                    instMetric.Metrics.Add(metric);
                }

                resmetric.InstrumentationLibraryMetrics.Add(instMetric);
            }

            var request = new ExportMetricsServiceRequest();
            request.ResourceMetrics.Add(resmetric);
            var bytes = request.ToByteArray();
            return bytes;
        }

        public byte[] EncodeHistogram()
        {
            DateTimeOffset dt = DateTimeOffset.UtcNow;

            var resmetric = new ResourceMetrics();
            resmetric.Resource = this.BuildResource();

            for (int lib = 0; lib < this.config.numLibs; lib++)
            {
                var instMetric = new InstrumentationLibraryMetrics();

                instMetric.InstrumentationLibrary = new InstrumentationLibrary();
                instMetric.InstrumentationLibrary.Name = $"Library{lib}";
                instMetric.InstrumentationLibrary.Version = "1.0.0";

                for (int m = 0; m < this.config.numMetrics; m++)
                {
                    Metric metric = new Metric();

                    metric.Name = $"Metric_{m}";
                    metric.Histogram = new Histogram();

                    var attribs = this.BuildAttributes();

                    for (int dp = 0; dp < this.config.numDataPoints; dp++)
                    {
                        var datapoint = new HistogramDataPoint();

                        datapoint.StartTimeUnixNano = (ulong)dt.ToUnixTimeMilliseconds() * 100000;
                        datapoint.TimeUnixNano = (ulong)dt.ToUnixTimeMilliseconds() * 100000;
                        datapoint.Attributes.AddRange(attribs);
                        datapoint.Flags = (1<<7) | (1<<29);

                        metric.Histogram.DataPoints.Add(datapoint);
                    }

                    instMetric.Metrics.Add(metric);
                }

                resmetric.InstrumentationLibraryMetrics.Add(instMetric);
            }

            var request = new ExportMetricsServiceRequest();
            request.ResourceMetrics.Add(resmetric);
            var bytes = request.ToByteArray();
            return bytes;
        }

        public List<string> Decode(byte[] bytes)
        {
            var parser = new Google.Protobuf.MessageParser<ExportMetricsServiceRequest>(() => new ExportMetricsServiceRequest());
            var request = parser.ParseFrom(bytes);

            var extracts = new List<string>();

            foreach (var resmetric in request.ResourceMetrics)
            {
                foreach (var attr in resmetric.Resource.Attributes)
                {
                    switch (attr.Value.ValueCase)
                    {
                        case AnyValue.ValueOneofCase.StringValue:
                            extracts.Add($"{attr.Key}={attr.Value.StringValue}");
                            break;

                        case AnyValue.ValueOneofCase.IntValue:
                            extracts.Add($"{attr.Key}={attr.Value.IntValue}");
                            break;

                        case AnyValue.ValueOneofCase.DoubleValue:
                            extracts.Add($"{attr.Key}={attr.Value.DoubleValue}");
                            break;

                        default:
                            extracts.Add($"{attr.Key}={attr.Value}");
                            break;
                    }
                }

                foreach (var meter in resmetric.InstrumentationLibraryMetrics)
                {
                    extracts.Add($"{meter.InstrumentationLibrary.Name}/{meter.InstrumentationLibrary.Version}");

                    foreach (var metric in meter.Metrics)
                    {
                        extracts.Add(metric.Name);

                        RepeatedField<NumberDataPoint> gaugedps = null;
                        RepeatedField<SummaryDataPoint> summarydps = null;
                        RepeatedField<HistogramDataPoint> histdps = null;

                        switch (metric.DataCase)
                        {
                            case Metric.DataOneofCase.Gauge:
                                gaugedps = metric.Gauge.DataPoints;
                                break;

                            case Metric.DataOneofCase.Summary:
                                summarydps = metric.Summary.DataPoints;
                                break;

                            case Metric.DataOneofCase.Histogram:
                                histdps = metric.Histogram.DataPoints;
                                break;
                        }

                        if (gaugedps is not null)
                        {
                            ulong flags = 0;
                            long suml = 0;
                            double sumd = 0;

                            foreach (var dp in gaugedps)
                            {
                                extracts.Add(string.Join(",", dp.Attributes.Select(lbl => $"{lbl.Key}={lbl.Value.ToString()}")));

                                switch (dp.ValueCase)
                                {
                                    case NumberDataPoint.ValueOneofCase.AsInt:
                                        suml += dp.AsInt;
                                        break;

                                    case NumberDataPoint.ValueOneofCase.AsDouble:
                                        sumd += dp.AsDouble;
                                        break;
                                }

                                flags = dp.Flags;
                            }

                            extracts.Add($"sum:{suml}/{sumd}");
                        }

                        if (summarydps is not null)
                        {
                            ulong flags = 0;

                            foreach (var dp in summarydps)
                            {
                                extracts.Add(string.Join(",", dp.Attributes.Select(lbl => $"{lbl.Key}={lbl.Value.ToString()}")));

                                extracts.Add($"{dp.Count}/{dp.Sum}");

                                extracts.Add(string.Join(",", dp.QuantileValues.Select(qv => $"{qv.Quantile}:{qv.Value}")));

                                flags = dp.Flags;
                            }
                        }

                        if (histdps is not null)
                        {
                            ulong flags = 0;

                            foreach (var dp in histdps)
                            {
                                extracts.Add(string.Join(",", dp.Attributes.Select(lbl => $"{lbl.Key}={lbl.Value.ToString()}")));

                                extracts.Add(string.Join(",", dp.BucketCounts.Select(k => $"{k}")));

                                extracts.Add(string.Join(",", dp.ExplicitBounds.Select(k => $"{k}")));

                                long suml = 0;
                                double sumd = 0.0;

                                foreach (var xp in dp.Exemplars)
                                {
                                    extracts.Add(string.Join(",", xp.FilteredAttributes.Select(lbl => $"{lbl.Key}={lbl.Value.ToString()}")));

                                    switch (xp.ValueCase)
                                    {
                                        case Exemplar.ValueOneofCase.AsInt:
                                            suml += xp.AsInt;
                                            break;

                                        case Exemplar.ValueOneofCase.AsDouble:
                                            sumd += xp.AsDouble;
                                            break;
                                    }
                                }

                                extracts.Add($"sum:{suml}/{sumd}");

                                flags = dp.Flags;
                            }
                        }
                    }
                }
            }

            return extracts;
        }

        private Resource BuildResource()
        {
            var resource = new Resource();
            resource.DroppedAttributesCount = 0;
            var attribs = resource.Attributes;
            foreach (var res in this.resources)
            {
                var kv = new KeyValue();
                kv.Key = res.name;
                kv.Value = new AnyValue();
                if (res.value is string s)
                {
                    kv.Value.StringValue = s;
                }
                else if (res.value is long l)
                {
                    kv.Value.IntValue = l;
                }

                attribs.Add(kv);
            }

            return resource;
        }

        private RepeatedField<KeyValue> BuildAttributes()
        {
            var attribs = new RepeatedField<KeyValue>();
            foreach (var l in this.labels)
            {
                var kv = new KeyValue();
                kv.Key = l.name;
                kv.Value = new AnyValue();
                kv.Value.StringValue = l.value;
                attribs.Add(kv);
            }
            
            return attribs;
        }
    }
}
