MCS = mono $(topdir)/mcs/mcs.exe
MCS_FLAGS = --target library --noconfig
INSTALL = /usr/bin/install
prefix = /usr

all: .makefrag $(LIBRARY)

clean:
	-rm -rf $(LIBRARY) .response .makefrag library-deps.stamp

.response: $(LIB_LIST)
	cat $^ |egrep '\.cs$$' >$@

.makefrag: $(LIB_LIST) $(topdir)/class/library.make
	echo -n "library-deps.stamp: " >$@.new
	cat $^ |egrep '\.cs$$' | sed -e 's,\.cs,.cs \\,' >>$@.new
	cat $@.new |sed -e '$$s, \\$$,,' >$@
	echo -e "\ttouch library-deps.stamp" >>$@
	rm -rf $@.new

-include .makefrag

$(LIBRARY): .response library-deps.stamp
	MONO_PATH=$(MONO_PATH_PREFIX)$(MONO_PATH) $(MCS) $(MCS_FLAGS) -o $(LIBRARY) $(LIB_FLAGS) @.response

install: all
	mkdir -p $(prefix)/lib/
	$(INSTALL) -m 644 $(LIBRARY) $(prefix)/lib/

