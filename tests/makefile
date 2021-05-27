PROTO_VERSIONS := v0.4.0 v0.8.0 v0.9.0

help:
	@echo "Help..."

extract:
	@for v in $(PROTO_VERSIONS); do { \
		echo "ver $$v"; \
		mkdir -p proto/$$v; \
		cd ..; \
		git archive --format=tar -o tests/proto/$$v/proto.tar $$v; \
		cd tests; \
		tar -xf proto/$$v/proto.tar -C proto/$$v opentelemetry; \
		eval z=`echo $$v | sed -r 's/\.//g'`; \
		find proto/$$v -name *.proto | xargs sed -i "s/opentelemetry\.proto/opentelemetry\.$$z\.proto/g"; \
		rm proto/$$v/proto.tar; \
	}; \
	done;

gen-protobuf-dotnet:
	@for v in $(PROTO_VERSIONS); do { \
		echo "ver $$v"; \
		rm -r -f dotnet/Benchmark/proto_$$v; \
		mkdir -p dotnet/Benchmark/proto_$$v; \
		find proto/$$v -name *.proto | xargs protoc --csharp_out=dotnet/Benchmark/proto_$$v --proto_path=proto/$$v/; \
	}; \
	done;

test:
	@for v in $(PROTO_VERSIONS); do \
		eval z=`echo $$v | sed -r 's/\.//g'`; \
		echo "hello z=$$z a=$$a"; \
	done;

clean:
	rm -r -f proto

bench-dotnet:
	dotnet run -p dotnet/Benchmark/Benchmark.csproj --
