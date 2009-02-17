// **********************************************************************
//
// Copyright (c) 2003-2009 ZeroC, Inc. All rights reserved.
//
// This copy of Ice is licensed to you under the terms described in the
// ICE_LICENSE file included in this distribution.
//
// **********************************************************************

#include <Ice/Ice.h>
#include <BI.h>


void
BI::shutdown(const Ice::Current& c)
{
    c.adapter->getCommunicator()->shutdown();
}
