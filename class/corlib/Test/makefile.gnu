topdir = ../../..

LIBRARY = corlib_test.dll

LIB_LIST = corlib_test.args
LIB_FLAGS =	\
		/debug+ /debug:full \
		-r $(topdir)/class/lib/corlib.dll \
		-r $(topdir)/class/lib/System.dll \
	    -r $(topdir)/class/lib/NUnit.Framework.dll

ifdef SUBDIR
USE_SOURCE_RULES=1
SOURCES_INCLUDE=./$(SUBDIR)/*.cs
SOURCES_EXCLUDE=_DUMMY_
endif

include $(topdir)/class/library.make

NUNITCONSOLE=$(topdir)/nunit20/nunit-console.exe
MONO_PATH = $(topdir)/class/lib:.

test: $(LIBRARY) run_test

.PHONY: run_test

run_test:
	-MONO_PATH=$(MONO_PATH) mono --debug $(NUNITCONSOLE) corlib_test.dll
