topdir = ..

LIBRARY = ../class/lib/NUnitCore_mono.dll

LIB_LIST = list.unix
LIB_FLAGS = -r ../class/lib/corlib.dll -r ../class/lib/System.dll

export MONO_PATH_PREFIX = ../class/lib:

include ../class/library.make

