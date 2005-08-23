# -*- makefile -*-

with_mono_path = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH"

BOOTSTRAP_MCS = $(EXTERNAL_MCS)
MCS = $(with_mono_path) $(INTERNAL_MCS)

PROFILE_MCS_FLAGS = -d:NET_1_1 -d:ONLY_1_1 -d:BOOTSTRAP_WITH_OLDLIB
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
NO_INSTALL = yes
FRAMEWORK_VERSION = 1.0

.PHONY: profile-check do-profile-check
profile-check:
	@:

ifeq (.,$(thisdir))
all-recursive: do-profile-check
clean-local: clean-profile
endif

clean-profile:
	-rm -f $(PROFILE_CS) $(PROFILE_EXE) $(PROFILE_OUT)

PROFILE_CS  = $(topdir)/build/deps/basic-profile-check.cs
PROFILE_EXE = $(PROFILE_CS:.cs=.exe)
PROFILE_OUT = $(PROFILE_CS:.cs=.out)

do-profile-check:
	@ok=:; \
	rm -f $(PROFILE_EXE) $(PROFILE_OUT); \
	$(MAKE) -s $(PROFILE_OUT) > /dev/null 2>&1 || ok=false; \
	rm -f $(PROFILE_EXE) $(PROFILE_OUT); \
	if $$ok; then :; else \
	    echo "*** The compiler '$(EXTERNAL_MCS)' doesn't appear to be usable." 1>&2 ; \
	    if test -f $(topdir)/class/lib/monolite/mcs.exe; then \
		monolite_corlib_version=`$(with_mono_path) $(ILDISASM) $(topdir)/class/lib/monolite/mscorlib.dll | sed -n 's,.*mono_corlib_version.*int32.*(\([^)]*\)),\1,p'`; \
		source_corlib_version=`sed -n 's,.*mono_corlib_version.*=[^0-9]*\([0-9]*\)[^0-9]*$$,\1,p' $(topdir)/class/corlib/System/Environment.cs`; \
		case $$monolite_corlib_version in \
		0x* | 0X*) monolite_corlib_version=`echo $$monolite_corlib_version | sed s,^0[xX],,`; \
		  monolite_corlib_version=`(echo 16; echo i; echo $$monolite_corlib_version; echo p) | dc` ;; esac ; \
		if test x$$monolite_corlib_version = x$$source_corlib_version; then \
	            echo "*** Falling back to using pre-compiled binaries.  Be warned, this may not work." 1>&2 ; \
		    ( cd $(topdir)/jay && $(MAKE) ); \
		    ( cd $(topdir)/mcs && $(MAKE) PROFILE=basic cs-parser.cs ); \
		    ( cd $(topdir)/class/lib/monolite/ && cp *.exe *.dll ../basic ); \
		    case `ls -1t $(topdir)/class/lib/basic/mcs.exe $(topdir)/mcs/cs-parser.cs | sed 1q` in \
		    $(topdir)/class/lib/basic/mcs.exe) : ;; \
		    *) sleep 5; cp $(topdir)/class/lib/monolite/mcs.exe $(topdir)/class/lib/basic ;; \
		    esac; \
		else \
		    echo "*** The contents of your 'monolite' directory are out-of-date" 1>&2; \
		    echo "*** You may want to try 'make get-monolite-latest'" 1>&2; \
		    echo "*** The source has version $$source_corlib_version, but monolite has version $$monolite_corlib_version" 1>&2; \
		    exit 1; fi; \
	    else \
                echo "*** You need a C# compiler installed to build MCS (make sure mcs works from the command line)" 1>&2 ; \
                echo "*** Read INSTALL.txt for information on how to bootstrap a Mono installation." 1>&2 ; \
	        exit 1; fi; fi

$(PROFILE_CS): $(topdir)/build/profiles/basic.make
	echo 'class X { static int Main () { return 0; } }' > $@

$(PROFILE_EXE): $(PROFILE_CS)
	$(EXTERNAL_MCS) /out:$@ $<

$(PROFILE_OUT): $(PROFILE_EXE)
	$(EXTERNAL_RUNTIME) $< > $@ 2>&1
