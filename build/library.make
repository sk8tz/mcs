# -*- makefile -*-
#
# The rules for building our class libraries.
#
# The NO_TEST stuff is not too pleasant but whatcha
# gonna do.

# All the dep files now land in the same directory so we
# munge in the library name to keep the files from clashing.

sourcefile = $(LIBRARY).sources
ifdef PLATFORM_CHANGE_SEPARATOR_CMD
response = $(depsdir)/$(PROFILE)_$(LIBRARY).response
else
response = $(sourcefile)
endif
makefrag = $(depsdir)/$(PROFILE)_$(LIBRARY).makefrag
stampfile = $(depsdir)/$(PROFILE)_$(LIBRARY).stamp
the_lib = $(topdir)/class/lib/$(PROFILE)/$(LIBRARY)
the_pdb = $(patsubst %.dll,%.pdb,$(the_lib))

ifndef NO_TEST
test_lib = $(patsubst %.dll,%_test.dll,$(LIBRARY))
test_pdb = $(patsubst %.dll,%.pdb,$(test_lib))
test_sourcefile = $(test_lib).sources
test_response = $(depsdir)/$(PROFILE)_$(test_lib).response
test_makefrag = $(depsdir)/$(PROFILE)_$(test_lib).makefrag
test_stampfile = $(depsdir)/$(PROFILE)_$(test_lib).stamp
test_flags = /r:$(the_lib) /r:$(topdir)/class/lib/$(PROFILE)/NUnit.Framework.dll $(TEST_MCS_FLAGS)
endif

all-local: $(the_lib)

install-local: $(the_lib)
	$(MKINSTALLDIRS) $(DESTDIR)$(prefix)/lib
	$(INSTALL_LIB) $(the_lib) $(DESTDIR)$(prefix)/lib

uninstall-local:
	-rm -f $(DESTDIR)$(prefix)/lib/$(LIBRARY)

clean-local:
	-rm -f $(the_lib) $(makefrag) $(test_lib) \
	       $(test_makefrag) $(test_response) \
	       $(stampfile) $(test_stampfile) \
	       $(the_pdb) $(test_pdb) \
	       TestResult.xml
ifdef PLATFORM_CHANGE_SEPARATOR_CMD
	-rm -rf $(response)
endif

ifndef NO_TEST
test-local: $(the_lib) $(test_lib)

run-test-local:
	$(TEST_RUNTIME) $(TEST_HARNESS) $(test_lib)

else
test-local: $(the_lib)

run-test-local:
endif

DISTFILES = $(sourcefile) $(test_sourcefile) $(EXTRA_DISTFILES)

ifdef NO_TEST
TEST_FILES = 
else
TEST_FILES = `cat $(test_sourcefile) |sed -e 's,^\(.\),Test/\1,'`
endif

dist-local: dist-default
	for f in `cat $(sourcefile)` $(TEST_FILES) ; do \
	    dest=`dirname $(distdir)/$$f` ; \
	    $(MKINSTALLDIRS) $$dest && cp $$f $$dest || exit 1 ; \
	done

# Fun with dependency tracking

$(the_lib): $(makefrag) $(stampfile) $(response)
	$(CSCOMPILE) $(LIBRARY_FLAGS) $(LIB_MCS_FLAGS) /target:library /out:$@ @$(response)

# warning: embedded tab in the 'echo touch' line
$(makefrag): $(sourcefile)
	@echo Creating $@ ...
	@echo "HAVE_MAKEFRAG = yes" >$@.new
	@echo "$(stampfile): \\" >>$@.new
	@cat $< |sed -e 's,\.cs[ \t]*$$,\.cs \\,' >>$@.new
	@cat $@.new |sed -e '$$s, \\$$,,' >$@
	@echo "	touch \$$@" >>$@
	@rm -rf $@.new

ifdef PLATFORM_CHANGE_SEPARATOR_CMD
$(response): $(sourcefile)
	@echo Creating $@ ...
	@cat $< |$(PLATFORM_CHANGE_SEPARATOR_CMD) >$@
endif

-include $(makefrag)

ifndef HAVE_MAKEFRAG
$(stampfile):
	touch $@
endif

# for now, don't give any /lib flags or set MONO_PATH, since we
# give a full path to the assembly.

ifndef NO_TEST
$(test_lib): $(test_makefrag) $(the_lib) $(test_response) $(test_stampfile)
	$(CSCOMPILE) /target:library /out:$@ $(test_flags) @$(test_response)

$(test_response): $(test_sourcefile)
	@echo Creating $@ ...
ifdef PLATFORM_CHANGE_SEPARATOR_CMD
	@cat $< |sed -e 's,^\(.\),Test/\1,' |$(PLATFORM_CHANGE_SEPARATOR_CMD) >$@
else
	@cat $< |sed -e 's,^\(.\),Test/\1,' >$@
endif

# warning: embedded tab in the 'echo touch' line
$(test_makefrag): $(test_response)
	@echo Creating $@ ...
	@echo "HAVE_TEST_MAKEFRAG = yes" >$@.new
	@echo "$(test_stampfile): \\" >>$@.new
	@cat $< |sed -e 's,\.cs[ \t]*$$,\.cs \\,' >>$@.new
	@cat $@.new |sed -e '$$s, \\$$,,' >$@
	@echo "	touch \$$@" >>$@
	@rm -rf $@.new

-include $(test_makefrag)
endif

ifndef HAVE_TEST_MAKEFRAG
$(test_stampfile):
	touch $@
endif

