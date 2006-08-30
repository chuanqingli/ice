# **********************************************************************
#
# Copyright (c) 2003-2006 ZeroC, Inc. All rights reserved.
#
# This copy of Ice is licensed to you under the terms described in the
# ICE_LICENSE file included in this distribution.
#
# **********************************************************************

top_srcdir	= ..\..\..\..

SERVER		= server.exe

TARGETS		= $(SERVER)

SOBJS		= Test.obj \
		  Types.obj \
		  TestI.obj \
		  Server.obj

SRCS		= $(SOBJS:.obj=.cpp)

!include $(top_srcdir)/config/Make.rules.mak

CPPFLAGS	= -I. -I../../../include $(CPPFLAGS)

!if "$(BORLAND_HOME)" == "" & "$(OPTIMIZE)" != "yes"
PDBFLAGS        = /pdb:$(SERVER:.exe=.pdb)
!endif

$(SERVER): $(SOBJS)
	del /q $@
	$(LINK) $(LD_EXEFLAGS) $(PDBFLAGS) $(SOBJS) $(PREOUT)$@ $(PRELIBS)$(LIBS)

Test.cpp Test.h: Test.ice $(SLICE2CPP) $(SLICEPARSERLIB)
	$(SLICE2CPP) --checksum $(SLICE2CPPFLAGS) Test.ice

Types.cpp Types.h: Types.ice $(SLICE2CPP) $(SLICEPARSERLIB)
	$(SLICE2CPP) --checksum $(SLICE2CPPFLAGS) Types.ice

clean::
	del /q Test.cpp Test.h
	del /q Types.cpp Types.h

!include .depend
