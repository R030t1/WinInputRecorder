CXX=x86_64-w64-mingw32-g++
CXF=-municode -mwindows -Wall -pedantic -std=gnu++2a \
	-static -static-libgcc -static-libstdc++
BIN=Recorder.exe

.phony: default clean

default: ${BIN}

Recorder.pb.cc: Recorder.proto
	protoc --experimental_allow_proto3_optional \
		--proto_path=./ --cpp_out=./ \
		$^

${BIN}: Recorder.pb.cc Recorder.pb.h Recorder.cc Recorder.h 
	${CXX} ${CXF} -o $@ Recorder.cc

clean:
	rm *.pb.* ${BIN}