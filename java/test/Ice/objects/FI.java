// **********************************************************************
//
// Copyright (c) 2003-2009 ZeroC, Inc. All rights reserved.
//
// This copy of Ice is licensed to you under the terms described in the
// ICE_LICENSE file included in this distribution.
//
// **********************************************************************

import Test.*;

public final class FI extends F
{
    public
    FI()
    {
    }

    public
    FI(E e)
    {
        super(e, e);
    }

    public boolean
    checkValues(Ice.Current current)
    {
        return e1 != null && e1 == e2;
    }
}
