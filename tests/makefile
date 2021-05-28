PROTO_VERSIONS := v0.4.0 v0.8.0 v0.9.0

help:
	@echo "Help..."

gen:
	@for v in $(PROTO_VERSIONS); do { \
		echo "ver $$v"; \
		\
		# Get version of proto code \
		mkdir -p proto/$$v; \
		cd ..; \
		git archive --format=tar -o tests/proto/$$v/proto.tar $$v; \
		cd tests; \
		tar -xf proto/$$v/proto.tar -C proto/$$v opentelemetry; \
		rm proto/$$v/proto.tar; \
		eval z=`echo $$v | sed -r 's/\.//g'`; \
		\
		# Rename Package to have version \
		find proto/$$v -name *.proto | xargs sed -i "s/opentelemetry\.proto/opentelemetry\.$$z\.proto/g"; \
		find proto/$$v -name metrics_config_service.proto | xargs sed -i 's/string equals/string equals2/g'; \
		\
		# Generate code for .NET \
		rm -r -f dotnet/Benchmark/proto_$$v; \
		mkdir -p dotnet/Benchmark/proto_$$v; \
		find proto/$$v -name *.proto | xargs protoc --csharp_out=dotnet/Benchmark/proto_$$v --proto_path=proto/$$v/; \
	}; \
	done;

clean:
	rm -r -f proto

bench-dotnet:
	dotnet run -p dotnet/Benchmark/Benchmark.csproj -c Release -- -m --filter *MetricBench*
