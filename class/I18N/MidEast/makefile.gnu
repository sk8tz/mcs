topdir = ../../..

LIBRARY = $(topdir)/class/lib/I18N.MidEast.dll

LIB_LIST = list.unix
LIB_FLAGS = -r corlib -r mscorlib -r I18N

SOURCES_INCLUDE=*.cs
SOURCES_EXCLUDE=

export MONO_PATH_PREFIX = $(topdir)/class/lib:

include $(topdir)/class/library.make
