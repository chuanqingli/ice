# **********************************************************************
#
# Copyright (c) 2003-2006 ZeroC, Inc. All rights reserved.
#
# This copy of Ice is licensed to you under the terms described in the
# ICE_LICENSE file included in this distribution.
#
# **********************************************************************

top_srcdir	= ..\..\..\..

CLIENT		= client.exe
SERVER		= server.exe
SERVERAMD	= serveramd.exe

TARGETS		= $(CLIENT) $(SERVER) $(SERVERAMD)

COBJS		= Test.obj \
		  ClientPrivate.obj \
		  Client.obj \
		  AllTests.obj \
		  Forward.obj

SOBJS		= Test.obj \
    		  ServerPrivate.obj \
		  TestI.obj \
		  Server.obj \
		  Forward.obj

SAMDOBJS	= TestAMD.obj \
    		  ServerPrivateAMD.obj \
		  TestAMDI.obj \
		  ServerAMD.obj \
		  Forward.obj

SRCS		= $(COBJS:.obj=.cpp) \
		  $(SOBJS:.obj=.cpp) \
		  $(SAMDOBJS:.obj=.cpp)

!include $(top_srcdir)/config/Make.rules.mak

ICECPPFLAGS	= -I. $(ICECPPFLAGS)

CPPFLAGS	= -I. -I../../../include $(CPPFLAGS)

!if "$(BORLAND_HOME)" == "" & "$(OPTIMIZE)" != "yes"
CPDBFLAGS        = /pdb:$(CLIENT:.exe=.pdb)
SPDBFLAGS        = /pdb:$(SERVER:.exe=.pdb)
SAPDBFLAGS       = /pdb:$(SERVERAMD:.exe=.pdb)
!endif

$(CLIENT): $(COBJS)
	del /q $@
	$(LINK) $(LD_EXEFLAGS) $(CPDBFLAGS) $(COBJS) $(PREOUT)$@ $(PRELIBS)$(LIBS)

$(SERVER): $(SOBJS)
	del /q $@
	$(LINK) $(LD_EXEFLAGS) $(SPDBFLAGS) $(SOBJS) $(PREOUT)$@ $(PRELIBS)$(LIBS)

$(SERVERAMD): $(SAMDOBJS)
	del /q $@
	$(LINK) $(LD_EXEFLAGS) $(SAPDBFLAGS) $(SAMDOBJS) $(PREOUT)$@ $(PRELIBS)$(LIBS)

Test.cpp Test.h: Test.ice $(SLICE2CPP) $(SLICEPARSERLIB)
	$(SLICE2CPP) $(SLICE2CPPFLAGS) Test.ice

TestAMD.cpp TestAMD.h: TestAMD.ice $(SLICE2CPP) $(SLICEPARSERLIB)
        $(SLICE2CPP) $(SLICE2CPPFLAGS) TestAMD.ice

ClientPrivate.cpp ClientPrivate.h: ClientPrivate.ice $(SLICE2CPP) $(SLICEPARSERLIB)
	$(SLICE2CPP) $(SLICE2CPPFLAGS) ClientPrivate.ice

ServerPrivate.cpp ServerPrivate.h: ServerPrivate.ice $(SLICE2CPP) $(SLICEPARSERLIB)
	$(SLICE2CPP) $(SLICE2CPPFLAGS) ServerPrivate.ice

ServerPrivateAMD.cpp ServerPrivateAMD.h: ServerPrivateAMD.ice $(SLICE2CPP) $(SLICEPARSERLIB)
        $(SLICE2CPP) $(SLICE2CPPFLAGS) ServerPrivateAMD.ice

Forward.cpp Forward.h: Forward.ice $(SLICE2CPP) $(SLICEPARSERLIB)
	$(SLICE2CPP) $(SLICE2CPPFLAGS) Forward.ice

clean::
	del /q Test.cpp Test.h
	del /q TestAMD.cpp TestAMD.h
	del /q ClientPrivate.cpp ClientPrivate.h
	del /q ServerPrivate.cpp ServerPrivate.h
	del /q ServerPrivateAMD.cpp ServerPrivateAMD.h
	del /q Forward.cpp Forward.h

!include .depend
