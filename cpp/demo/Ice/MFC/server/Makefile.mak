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

OBJS		= Hello.obj \
		  HelloI.obj \
		  HelloServer.obj \
		  HelloServerDlg.obj \
		  LogI.obj \
		  stdafx.obj
		
SRCS		= $(OBJS:.obj=.cpp)

!include $(top_srcdir)/config/Make.rules.mak

CPPFLAGS	= -I. $(CPPFLAGS) -D_AFXDLL

!if "$(BORLAND_HOME)" == "" & "$(OPTIMIZE)" != "yes"
PDBFLAGS        = /pdb:$(SERVER:.exe=.pdb)
!endif

$(SERVER): $(OBJS) $(COBJS) HelloServer.res
	del /q $@
	$(LINK) $(LD_EXEFLAGS) $(PDBFLAGS) /subsystem:windows $(OBJS) $(COBJS) HelloServer.res \
	  $(PREOUT)$@ $(PRELIBS)$(LIBS)

Hello.cpp Hello.h: Hello.ice $(SLICE2CPP) $(SLICEPARSERLIB)
	$(SLICE2CPP) $(SLICE2CPPFLAGS) Hello.ice

HelloServer.res: HelloServer.rc
	rc.exe HelloServer.rc

clean::
	del /q Hello.cpp Hello.h
	del /q HelloServer.res

!include .depend
