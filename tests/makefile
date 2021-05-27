PROTO_VERSIONS := v0.4.0 v0.8.0 v0.9.0

help:
	@echo "Help..."

extractall: extract040 extract080 extract090

extract:
	@for v in $(PROTO_VERSIONS); do { \
		echo "ver $$v"; \
		mkdir -p proto/$$v; \
		cd ..; \
		git archive --format=tar -o tests/proto/$$v/proto.tar $$v; \
		cd tests; \
		tar -xf proto/$$v/proto.tar -C proto/$$v opentelemetry; \
		rm proto/$$v/proto.tar; \
	}; \
	done;

clean:
	rm -r -f proto
