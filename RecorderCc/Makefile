CXX= x86_64-w64-mingw32-g++
CXF= -municode -mwindows -Wall -pedantic -std=gnu++2a \
	-static-libgcc -static-libstdc++ \
	$(shell pkg-config --cflags protobuf)
CXL= $(shell pkg-config --libs protobuf) \
	-lboost_system-mt -lboost_filesystem-mt -lboost_iostreams-mt
BIN=Recorder.exe

.phony: default clean

default: $(BIN)

Recorder.pb.cc: Recorder.proto
	protoc --experimental_allow_proto3_optional \
		--proto_path=./ --cpp_out=./ \
		$^

Recorder.pb.o: Recorder.pb.cc Recorder.pb.h
	${CXX} $(CXF) Recorder.pb.cc -c -o $@

${BIN}: Recorder.pb.o Recorder.cc Recorder.h
	${CXX} $(CXF) Recorder.pb.cc Recorder.cc -o $@ ${CXL}

release:
	mkdir -p WinInputRecorder
	cp $(shell ldd Recorder.exe | awk -v IGNORECASE=1 '!/System32/ { print $$3; }') ./WinInputRecorder
	cp Recorder.exe ./WinInputRecorder
	zip WinInputRecorder -r ./WinInputRecorder 

clean:
	rm -rf *.o *.pb.* WinInputRecorder WinInputRecorder.zip ${BIN}

# ldd Recorder.exe | awk 'BEGIN { IGNORECASE=1 } !/System32/ { print $3; }'

# x86_64-w64-mingw32-g++ -municode -mwindows -std=gnu++2a `pkg-config --cflags --libs protobuf` Recorder.pb.cc -c -o Recorder.pb.o
# x86_64-w64-mingw32-g++ -municode -mwindows -std=gnu++2a `pkg-config --cflags --libs protobuf` Recorder.cc Recorder.pb.o -o Recorder -lprotobuf

# Below works.
#g++ -mwindows -municode -std=gnu++2a \
# Recorder.cc Recorder.pb.cc -o Recorder \
# `pkg-config --cflags --libs protobuf`

# After doing:
#g++ -mwindows -municode -std=gnu++2a \
# Recorder.pb.cc -o Recorder.pb.o -c \
# `pkg-config --cflags --libs protobuf`