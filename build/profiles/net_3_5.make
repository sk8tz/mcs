# -*- makefile -*-

MCS = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_GMCS)
# nuttzing!

profile-check:
	@:

PROFILE_MCS_FLAGS = -d:NET_1_1 -d:NET_2_0 -d:NET_3_5 -langversion:linq
FRAMEWORK_VERSION = 3.5
