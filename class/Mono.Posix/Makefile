thisdir = class/Mono.Posix
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Posix.dll
LIB_MCS_FLAGS = /unsafe /r:$(corlib) /r:System.dll
NO_TEST = yes

include ../../build/library.make

all-local: Mono.Posix/make-map.exe 

Mono.Posix/make-map.exe: Mono.Posix/make-map.cs $(the_lib)
	cp $(the_lib) Mono.Posix/
ifneq ($(PLATFORM),win32)
	$(CSCOMPILE)  -out:Mono.Posix/make-map.exe -r:Mono.Posix.dll Mono.Posix/make-map.cs
else
	$(CSCOMPILE)  -out:Mono.Posix/make-map.exe -r:Mono.Posix.dll Mono.Posix\\make-map.cs
endif

