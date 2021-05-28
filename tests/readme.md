# Benchmark for OTLP protocol

Benchmarks for different versions of OTLP protocol.

## Build and Run

1. Download all version of proto files

    Bash:

    ```
    $ make gen
    ```

    **Note:** package names are modified to include version. (i.e. opentelemetry.**v090**.proto.common.v1)

2. Run benchmarks

    On Windows:

    ```
    > dotnet run -p dotnet/Benchmark/Benchmark.csproj -c Release -- -m --filter *MetricBench*
    ```
