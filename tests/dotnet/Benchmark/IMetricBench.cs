using System.Collections.Generic;

namespace MetricBenchmark
{
    public interface IMetricBench
    {
        byte[] EncodeGauge();

        List<string> Decode(byte[] bytes);
    }
}
