# -*- makefile -*-
#
# This makefile fragment has (default) configuration
# settings for building MCS.

# DO NOT EDIT THIS FILE! Create config.make and override settings
# there.

RUNTIME_FLAGS = 
TEST_HARNESS = $(topdir)/class/lib/nunit-console.exe
MCS_FLAGS = $(PLATFORM_DEBUG_FLAGS) /nowarn:1595 /nowarn:0169 \
 /nowarn:0109 /nowarn:0067 /nowarn:0649 /nowarn:0679
LIBRARY_FLAGS = /noconfig
CFLAGS = -g -O2
INSTALL = /usr/bin/install
RUNTIME = mono $(RUNTIME_FLAGS)
TEST_RUNTIME=$(PLATFORM_RUNTIME)
prefix = /usr/local

# In case you want to add MCS_FLAGS, this lets you not have to
# keep track of the default value

DEFAULT_MCS_FLAGS := $(MCS_FLAGS)

# Not all echos are alike. HP/UX doesn't need a -e command.
# Test for this here

ifeq ($(shell echo -e foo),foo)
ECHO_ESCAPE=echo -e
else
ECHO_ESCAPE=echo
endif

# You shouldn't need to set these but might on a 
# weird platform.

# CC = cc
# SHELL = /bin/sh
# MAKE = gmake 
