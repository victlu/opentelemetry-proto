PROTO_VERSIONS := v0.4.0 v0.8.0 v0.9.0

help:
	@echo "gen - Get versions of OTLP proto to the proto/ folder. Package names modified to include version."
	@echo "bench-dotnet - Run benchmarks for DotNet."
	@echo "clean - remove proto/ folder."

gen:
	@for v in $(PROTO_VERSIONS); do { \
		echo "ver $$v"; \
		\
		# Get different versions of OTLP proto code \
		mkdir -p proto/$$v; \
		cd ..; \
		git archive --format=tar -o tests/proto/$$v/proto.tar $$v; \
		cd tests; \
		tar -xf proto/$$v/proto.tar -C proto/$$v opentelemetry; \
		rm proto/$$v/proto.tar; \
		eval z=`echo $$v | sed -r 's/\.//g'`; \
		\
		# Rename Package names to include version \
		find proto/$$v -name *.proto | xargs sed -i "s/opentelemetry\.proto/opentelemetry\.$$z\.proto/g"; \
		find proto/$$v -name metrics_config_service.proto | xargs sed -i 's/string equals/string equals2/g'; \
	}; \
	done;

clean:
	rm -r -f proto

bench-dotnet:
	@echo Run in Windows:
	@echo "  > dotnet run -p dotnet/Benchmark/Benchmark.csproj -c Release -- -m --filter *MetricBench*"
