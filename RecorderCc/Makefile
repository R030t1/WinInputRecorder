CXX=x86_64-w64-mingw32-g++
CXF=-municode -mwindows -Wall -pedantic -std=gnu++2a \
	-static-libgcc -static-libstdc++ \
	$(pkg-config --cflags --libs protobuf)
BIN=Recorder.exe

.phony: default clean

default: ${BIN}

Recorder.pb.cc: Recorder.proto
	protoc --experimental_allow_proto3_optional \
		--proto_path=./ --cpp_out=./ \
		$^

Recorder.pb.o: Recorder.pb.cc Recorder.pb.h
	${CXX} ${CXF} Recorder.pb.cc -c -o $@

${BIN}: Recorder.pb.o Recorder.cc Recorder.h
	${CXX} ${CXF} Recorder.pb.cc Recorder.cc -o $@ -lprotobuf

clean:
	rm *.o *.pb.* ${BIN}

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