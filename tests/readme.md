# Benchmark for OTLP protocol

Benchmarks for different versions of OTLP protocol.

## Build and Run

1. Download all version of proto files and generate language code

    ```
    > make gen
    ```

2. Run benchmarks

    On Windows:

    ```
    > dotnet run -p dotnet/Benchmark/Benchmark.csproj -c Release -- -m --filter *MetricBench*
    ```
