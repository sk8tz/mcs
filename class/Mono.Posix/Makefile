thisdir = class/Mono.Posix
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Posix.dll
LIB_MCS_FLAGS = /unsafe /r:$(corlib) /r:System.dll /r:System.Runtime.Remoting.dll
TEST_MCS_FLAGS = /r:Mono.Posix.dll /r:System.dll /nowarn:0219,0618

include ../../build/library.make

EXTRA_DISTFILES = Mono.Unix/make-map.cs

all-local: Mono.Unix/make-map.exe 

Mono.Unix/make-map.exe: Mono.Unix/make-map.cs $(the_lib)
	cp $(the_lib) Mono.Unix/
ifneq ($(PLATFORM),win32)
	$(CSCOMPILE)  -out:Mono.Unix/make-map.exe -r:Mono.Posix.dll Mono.Unix/make-map.cs
else
	$(CSCOMPILE)  -out:Mono.Unix/make-map.exe -r:Mono.Posix.dll Mono.Unix\\make-map.cs
endif

CLEAN_FILES = Mono.Unix/make-map.exe Mono.Unix/Mono.Posix.dll
