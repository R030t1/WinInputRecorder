CXX=x86_64-w64-mingw32-g++
CXF=-municode -mwindows -Wall -pedantic -std=gnu++2a \
	-static -static-libgcc -static-libstdc++
BIN=Recorder.exe

.phony: clean

${BIN}: Recorder.cpp Recorder.hpp 
	${CXX} ${CXF} -o $@ Recorder.cpp

clean:
	rm ${BIN}