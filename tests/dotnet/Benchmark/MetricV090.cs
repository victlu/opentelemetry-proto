using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Opentelemetry.V090.Proto.Collector.Metrics.V1;
using Opentelemetry.V090.Proto.Common.V1;
using Opentelemetry.V090.Proto.Metrics.V1;
using Opentelemetry.V090.Proto.Resource.V1;

namespace MetricBenchmark
{
    public class MetricV090 : IMetricBench
    {
        private bool isDouble;
        private (string name, object value)[] resources;
        private (string name, string value)[] labels;
        private int numLibs;
        private int numMetrics;
        private int numPoints;

        public MetricV090(bool isDouble, int numResources, int numLabels, int numLibs, int numMetrics, int numPoints)
        {
            this.isDouble = isDouble;
            this.numLibs = numLibs;
            this.numMetrics = numMetrics;
            this.numPoints = numPoints;

            this.resources = new (string name, object value)[numResources];
            for (int i = 0; i < numResources; i++)
            {
                this.resources[i] = ( $"Resource{i}", $"Value{i}");
            }

            this.labels = new (string name, string value)[numResources];
            for (int i = 0; i < numResources; i++)
            {
                this.labels[i] = ( $"Label{i}", $"Value{i}");
            }
        }

        public byte[] EncodeGauge()
        {
            DateTimeOffset dt = DateTimeOffset.UtcNow;

            var resmetric = new ResourceMetrics();

            for (int lib = 0; lib < this.numLibs; lib++)
            {
                var instMetric = new InstrumentationLibraryMetrics();

                instMetric.InstrumentationLibrary = new InstrumentationLibrary();
                instMetric.InstrumentationLibrary.Name = $"Library{lib}";
                instMetric.InstrumentationLibrary.Version = "1.0.0";

                for (int m = 0; m < this.numMetrics; m++)
                {
                    Metric metric = new Metric();
                    metric.Name = $"Metric_{m}";

                    var stringLabels = new RepeatedField<StringKeyValue>();
                    foreach (var l in this.labels)
                    {
                        var kv = new StringKeyValue();
                        kv.Key = l.name;
                        // kv.Value = new AnyValue();
                        // kv.Value.StringValue = l.value;
                        kv.Value = l.value;
                        stringLabels.Add(kv);
                    }

                    var gauge = new Gauge();
                    metric.Gauge = gauge;

                    if (isDouble)
                    {
                        for (int dp = 0; dp < this.numPoints; dp++)
                        {
                            var datapoint = new NumberDataPoint();

                            datapoint.StartTimeUnixNano = (ulong)dt.ToUnixTimeMilliseconds() * 100000;
                            datapoint.TimeUnixNano = (ulong)dt.ToUnixTimeMilliseconds() * 100000;
                            datapoint.Labels.AddRange(stringLabels);
                            datapoint.AsDouble = (double)(dp + 100.1);

                            gauge.DataPoints.Add(datapoint);
                        }
                    }
                    else
                    {
                        for (int dp = 0; dp < this.numPoints; dp++)
                        {
                            var datapoint = new NumberDataPoint();

                            datapoint.StartTimeUnixNano = (ulong)dt.ToUnixTimeMilliseconds() * 100000;
                            datapoint.TimeUnixNano = (ulong)dt.ToUnixTimeMilliseconds() * 100000;
                            datapoint.Labels.AddRange(stringLabels);
                            datapoint.AsInt = (long)(dp + 100);

                            gauge.DataPoints.Add(datapoint);
                        }
                    }

                    instMetric.Metrics.Add(metric);
                }

                resmetric.InstrumentationLibraryMetrics.Add(instMetric);
            }

            resmetric.Resource = this.BuildResource();

            var request = new ExportMetricsServiceRequest();
            request.ResourceMetrics.Add(resmetric);
            var bytes = request.ToByteArray();
            return bytes;
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
                        RepeatedField<SummaryDataPoint> sumdps = null;
                        RepeatedField<HistogramDataPoint> histdps = null;

                        switch (metric.DataCase)
                        {
                            case Metric.DataOneofCase.Gauge:
                                gaugedps = metric.Gauge.DataPoints;
                                break;

                            case Metric.DataOneofCase.Summary:
                                sumdps = metric.Summary.DataPoints;
                                break;

                            case Metric.DataOneofCase.Histogram:
                                histdps = metric.Histogram.DataPoints;
                                break;
                        }

                        if (gaugedps is not null)
                        {
                            long suml = 0;
                            double sumd = 0;

                            foreach (var dp in gaugedps)
                            {
                                extracts.Add(string.Join(",", dp.Labels.Select(lbl => $"{lbl.Key}={lbl.Value}")));

                                switch (dp.ValueCase)
                                {
                                    case NumberDataPoint.ValueOneofCase.AsInt:
                                        suml += dp.AsInt;
                                        break;

                                    case NumberDataPoint.ValueOneofCase.AsDouble:
                                        sumd += dp.AsDouble;
                                        break;
                                }
                            }

                            extracts.Add($"sum:{suml}/{sumd}");
                        }

                        if (sumdps is not null)
                        {
                            foreach (var dp in sumdps)
                            {
                                extracts.Add(string.Join(",", dp.Labels.Select(lbl => $"{lbl.Key}={lbl.Value}")));

                                extracts.Add($"{dp.Count}/{dp.Sum}");

                                extracts.Add(string.Join(",", dp.QuantileValues.Select(qv => $"{qv.Quantile}:{qv.Value}")));
                            }
                        }

                        if (histdps is not null)
                        {
                            foreach (var dp in histdps)
                            {
                                extracts.Add(string.Join(",", dp.Labels.Select(lbl => $"{lbl.Key}={lbl.Value}")));

                                extracts.Add(string.Join(",", dp.BucketCounts.Select(k => $"{k}")));

                                extracts.Add(string.Join(",", dp.ExplicitBounds.Select(k => $"{k}")));

                                long suml = 0;
                                double sumd = 0.0;

                                foreach (var xp in dp.Exemplars)
                                {
                                    extracts.Add(string.Join(",", xp.FilteredLabels.Select(lbl => $"{lbl.Key}={lbl.Value}")));

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
                            }
                        }
                    }
                }
            }

            return extracts;
        }
    }
}
