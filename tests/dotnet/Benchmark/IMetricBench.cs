using System.Collections.Generic;

namespace MetricBenchmark
{
    public interface IMetricBench
    {
        byte[] EncodeGauge();

        byte[] EncodeSum();

        byte[] EncodeSummary();

        byte[] EncodeHistogram();

        List<string> Decode(byte[] bytes);
    }
}
