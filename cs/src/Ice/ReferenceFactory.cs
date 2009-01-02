// **********************************************************************
//
// Copyright (c) 2003-2009 ZeroC, Inc. All rights reserved.
//
// This copy of Ice is licensed to you under the terms described in the
// ICE_LICENSE file included in this distribution.
//
// **********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;

namespace IceInternal
{

    public class ReferenceFactory
    {
        public Reference
        create(Ice.Identity ident, string facet, Reference tmpl, EndpointI[] endpoints)
        {
            if(ident.name.Length == 0 && ident.category.Length == 0)
            {
                return null;
            }

            return create(ident, facet, tmpl.getMode(), tmpl.getSecure(), endpoints, null, null);
        }

        public Reference
        create(Ice.Identity ident, string facet, Reference tmpl, string adapterId)
        {
            if(ident.name.Length == 0 && ident.category.Length == 0)
            {
                return null;
            }
            
            //
            // Create new reference
            //
            return create(ident, facet, tmpl.getMode(), tmpl.getSecure(), null, adapterId, null);
        }

        public Reference create(Ice.Identity ident, Ice.ConnectionI connection)
        {
            if(ident.name.Length == 0 && ident.category.Length == 0)
            {
                return null;
            }
            
            //
            // Create new reference
            //
            FixedReference r = new FixedReference(instance_, 
                                                  _communicator, 
                                                  ident,
                                                  instance_.getDefaultContext(),
                                                  "", // Facet
                                                  Reference.Mode.ModeTwoway,
                                                  false,
                                                  connection);
            return updateCache(r);
        }

        public Reference copy(Reference r)
        {
            Ice.Identity ident = r.getIdentity();
            if(ident.name.Length == 0 && ident.category.Length == 0)
            {
                return null;
            }
            return (Reference)r.Clone();
        }

        public Reference create(string s, string propertyPrefix)
        {
            if(s.Length == 0)
            {
                return null;
            }

            const string delim = " \t\n\r";

            int beg;
            int end = 0;

            beg = IceUtilInternal.StringUtil.findFirstNotOf(s, delim, end);
            if(beg == -1)
            {
                Ice.ProxyParseException e = new Ice.ProxyParseException();
                e.str = s;
                throw e;
            }

            //
            // Extract the identity, which may be enclosed in single
            // or double quotation marks.
            //
            string idstr = null;
            end = IceUtilInternal.StringUtil.checkQuote(s, beg);
            if(end == -1)
            {
                Ice.ProxyParseException e = new Ice.ProxyParseException();
                e.str = s;
                throw e;
            }
            else if(end == 0)
            {
                end = IceUtilInternal.StringUtil.findFirstOf(s, delim + ":@", beg);
                if(end == -1)
                {
                    end = s.Length;
                }
                idstr = s.Substring(beg, end - beg);
            }
            else
            {
                beg++; // Skip leading quote
                idstr = s.Substring(beg, end - beg);
                end++; // Skip trailing quote
            }

            if(beg == end)
            {
                Ice.ProxyParseException e = new Ice.ProxyParseException();
                e.str = s;
                throw e;
            }

            //
            // Parsing the identity may raise IdentityParseException.
            //
            Ice.Identity ident = instance_.stringToIdentity(idstr);

            if(ident.name.Length == 0)
            {
                //
                // An identity with an empty name and a non-empty
                // category is illegal.
                //
                if(ident.category.Length > 0)
                {
                    Ice.IllegalIdentityException e = new Ice.IllegalIdentityException();
                    e.id = ident;
                    throw e;
                }
                //
                // Treat a stringified proxy containing two double
                // quotes ("") the same as an empty string, i.e.,
                // a null proxy, but only if nothing follows the
                // quotes.
                //
                else if(IceUtilInternal.StringUtil.findFirstNotOf(s, delim, end) != -1)
                {
                    Ice.ProxyParseException e = new Ice.ProxyParseException();
                    e.str = s;
                    throw e;
                }
                else
                {
                    return null;
                }
            }

            string facet = "";
            Reference.Mode mode = Reference.Mode.ModeTwoway;
            bool secure = false;
            string adapter = "";

            while(true)
            {
                beg = IceUtilInternal.StringUtil.findFirstNotOf(s, delim, end);
                if(beg == -1)
                {
                    break;
                }

                if(s[beg] == ':' || s[beg] == '@')
                {
                    break;
                }

                end = IceUtilInternal.StringUtil.findFirstOf(s, delim + ":@", beg);
                if(end == -1)
                {
                    end = s.Length;
                }

                if(beg == end)
                {
                    break;
                }

                string option = s.Substring(beg, end - beg);
                if(option.Length != 2 || option[0] != '-')
                {
                    Ice.ProxyParseException e = new Ice.ProxyParseException();
                    e.str = s;
                    throw e;
                }

                //
                // Check for the presence of an option argument. The
                // argument may be enclosed in single or double
                // quotation marks.
                //
                string argument = null;
                int argumentBeg = IceUtilInternal.StringUtil.findFirstNotOf(s, delim, end);
                if(argumentBeg != -1)
                {
                    char ch = s[argumentBeg];
                    if(ch != '@' && ch != ':' && ch != '-')
                    {
                        beg = argumentBeg;
                        end = IceUtilInternal.StringUtil.checkQuote(s, beg);
                        if(end == -1)
                        {
                            Ice.ProxyParseException e = new Ice.ProxyParseException();
                            e.str = s;
                            throw e;
                        }
                        else if(end == 0)
                        {
                            end = IceUtilInternal.StringUtil.findFirstOf(s, delim + ":@", beg);
                            if(end == -1)
                            {
                                end = s.Length;
                            }
                            argument = s.Substring(beg, end - beg);
                        }
                        else
                        {
                            beg++; // Skip leading quote
                            argument = s.Substring(beg, end - beg);
                            end++; // Skip trailing quote
                        }
                    }
                }

                //
                // If any new options are added here,
                // IceInternal::Reference::toString() and its derived classes must be updated as well.
                //
                switch(option[1])
                {
                    case 'f':
                    {
                        if(argument == null)
                        {
                            Ice.ProxyParseException e = new Ice.ProxyParseException();
                            e.str = s;
                            throw e;
                        }

                        string token;
                        if(!IceUtilInternal.StringUtil.unescapeString(argument, 0, argument.Length, out token))
                        {
                            Ice.ProxyParseException e = new Ice.ProxyParseException();
                            e.str = s;
                            throw e;
                        }
                        facet = token;
                        break;
                    }

                    case 't':
                    {
                        if(argument != null)
                        {
                            Ice.ProxyParseException e = new Ice.ProxyParseException();
                            e.str = s;
                            throw e;
                        }
                        mode = Reference.Mode.ModeTwoway;
                        break;
                    }

                    case 'o':
                    {
                        if(argument != null)
                        {
                            Ice.ProxyParseException e = new Ice.ProxyParseException();
                            e.str = s;
                            throw e;
                        }
                        mode = Reference.Mode.ModeOneway;
                        break;
                    }

                    case 'O':
                    {
                        if(argument != null)
                        {
                            Ice.ProxyParseException e = new Ice.ProxyParseException();
                            e.str = s;
                            throw e;
                        }
                        mode = Reference.Mode.ModeBatchOneway;
                        break;
                    }

                    case 'd':
                    {
                        if(argument != null)
                        {
                            Ice.ProxyParseException e = new Ice.ProxyParseException();
                            e.str = s;
                            throw e;
                        }
                        mode = Reference.Mode.ModeDatagram;
                        break;
                    }

                    case 'D':
                    {
                        if(argument != null)
                        {
                            Ice.ProxyParseException e = new Ice.ProxyParseException();
                            e.str = s;
                            throw e;
                        }
                        mode = Reference.Mode.ModeBatchDatagram;
                        break;
                    }

                    case 's':
                    {
                        if(argument != null)
                        {
                            Ice.ProxyParseException e = new Ice.ProxyParseException();
                            e.str = s;
                            throw e;
                        }
                        secure = true;
                        break;
                    }

                    default:
                    {
                        Ice.ProxyParseException e = new Ice.ProxyParseException();
                        e.str = s;
                        throw e;
                    }
                }
            }

            if(beg == -1)
            {
                return create(ident, facet, mode, secure, null, null, propertyPrefix);
            }

            ArrayList endpoints = new ArrayList();

            if(s[beg] == ':')
            {
                ArrayList unknownEndpoints = new ArrayList();
                end = beg;

                while(end < s.Length && s[end] == ':')
                {
                    beg = end + 1;
                    
                    end = beg;
                    while(true)
                    {
                        end = s.IndexOf(':', end);
                        if(end == -1)
                        {
                            end = s.Length;
                            break;
                        }
                        else
                        {
                            bool quoted = false;
                            int quote = beg;
                            while(true)
                            {
                                quote = s.IndexOf((System.Char) '\"', quote);
                                if(quote == -1 || end < quote)
                                {
                                    break;
                                }
                                else
                                {
                                    quote = s.IndexOf((System.Char) '\"', ++quote);
                                    if(quote == -1)
                                    {
                                        break;
                                    }
                                    else if(end < quote)
                                    {
                                        quoted = true;
                                        break;
                                    }
                                    ++quote;
                                }
                            }
                            if(!quoted)
                            {
                                break;
                            }
                            ++end;
                        }
                    }
                    
                    string es = s.Substring(beg, end - beg);
                    EndpointI endp = instance_.endpointFactoryManager().create(es, false);
                    if(endp != null)
                    {
                        endpoints.Add(endp);
                    }
                    else
                    {
                        unknownEndpoints.Add(es);
                    }
                }
                if(endpoints.Count == 0)
                {
                    Ice.EndpointParseException e2 = new Ice.EndpointParseException();
                    e2.str = s;
                    throw e2;
                }
                else if(unknownEndpoints.Count != 0 &&
                        instance_.initializationData().properties.getPropertyAsIntWithDefault(
                                                                                "Ice.Warn.Endpoints", 1) > 0)
                {
                    string msg = "Proxy contains unknown endpoints:";
                    int sz = unknownEndpoints.Count;
                    for(int idx = 0; idx < sz; ++idx)
                    {
                        msg += " `" + (string)unknownEndpoints[idx] + "'";
                    }
                    instance_.initializationData().logger.warning(msg);
                }

                EndpointI[] ep = (EndpointI[])endpoints.ToArray(typeof(EndpointI));
                return create(ident, facet, mode, secure, ep, null, propertyPrefix);
            }
            else if(s[beg] == '@')
            {
                beg = IceUtilInternal.StringUtil.findFirstNotOf(s, delim, beg + 1);
                if(beg == -1)
                {
                    Ice.ProxyParseException e = new Ice.ProxyParseException();
                    e.str = s;
                    throw e;
                }

                string adapterstr = null;
                end = IceUtilInternal.StringUtil.checkQuote(s, beg);
                if(end == -1)
                {
                    Ice.ProxyParseException e = new Ice.ProxyParseException();
                    e.str = s;
                    throw e;
                }
                else if(end == 0)
                {
                    end = IceUtilInternal.StringUtil.findFirstOf(s, delim, beg);
                    if(end == -1)
                    {
                        end = s.Length;
                    }
                    adapterstr = s.Substring(beg, end - beg);
                }
                else
                {
                    beg++; // Skip leading quote
                    adapterstr = s.Substring(beg, end - beg);
                    end++; // Skip trailing quote
                }

                if(end != s.Length && IceUtilInternal.StringUtil.findFirstNotOf(s, delim, end) != -1)
                {
                    Ice.ProxyParseException e = new Ice.ProxyParseException();
                    e.str = s;
                    throw e;
                }

                if(!IceUtilInternal.StringUtil.unescapeString(adapterstr, 0, adapterstr.Length, out adapter) ||
                   adapter.Length == 0)
                {
                    Ice.ProxyParseException e = new Ice.ProxyParseException();
                    e.str = s;
                    throw e;
                }
                return create(ident, facet, mode, secure, null, adapter, propertyPrefix);
            }

            Ice.ProxyParseException ex = new Ice.ProxyParseException();
            ex.str = s;
            throw ex;
        }

        public Reference create(Ice.Identity ident, BasicStream s)
        {
            //
            // Don't read the identity here. Operations calling this
            // constructor read the identity, and pass it as a parameter.
            //

            if(ident.name.Length == 0 && ident.category.Length == 0)
            {
                return null;
            }

            //
            // For compatibility with the old FacetPath.
            //
            string[] facetPath = s.readStringSeq();
            string facet;
            if(facetPath.Length > 0)
            {
                if(facetPath.Length > 1)
                {
                    throw new Ice.ProxyUnmarshalException();
                }
                facet = facetPath[0];
            }
            else
            {
                facet = "";
            }

            int mode = (int)s.readByte();
            if(mode < 0 || mode > (int)Reference.Mode.ModeLast)
            {
                throw new Ice.ProxyUnmarshalException();
            }

            bool secure = s.readBool();

            EndpointI[] endpoints = null;
            string adapterId = "";

            int sz = s.readSize();
            if(sz > 0)
            {
                endpoints = new EndpointI[sz];
                for(int i = 0; i < sz; i++)
                {
                    endpoints[i] = instance_.endpointFactoryManager().read(s);
                }
            }
            else
            {
                adapterId = s.readString();
            }
            
            return create(ident, facet, (Reference.Mode)mode, secure, endpoints, adapterId, null);
        }

        public ReferenceFactory setDefaultRouter(Ice.RouterPrx defaultRouter)
        {
            if(_defaultRouter == null ? defaultRouter == null : _defaultRouter.Equals(defaultRouter))
            {
                return this;
            }
            
            ReferenceFactory factory = new ReferenceFactory(instance_, _communicator);
            factory._defaultLocator = _defaultLocator;
            factory._defaultRouter = defaultRouter;
            return factory;
        }

        public Ice.RouterPrx getDefaultRouter()
        {
            return _defaultRouter;
        }

        public ReferenceFactory setDefaultLocator(Ice.LocatorPrx defaultLocator)
        {
            if(_defaultLocator == null ? defaultLocator == null : _defaultLocator.Equals(defaultLocator))
            {
                return this;
            }
            
            ReferenceFactory factory = new ReferenceFactory(instance_, _communicator);
            factory._defaultLocator = defaultLocator;
            factory._defaultRouter = _defaultRouter;
            return factory;
        }

        public Ice.LocatorPrx getDefaultLocator()
        {
            return _defaultLocator;
        }

        //
        // Only for use by Instance
        //
        internal ReferenceFactory(Instance instance, Ice.Communicator communicator)
        {
            instance_ = instance;
            _communicator = communicator;
        }

        internal void destroy()
        {
            lock(this)
            {
                _references.Clear();
            }
        }

        private Reference updateCache(Reference @ref)
        {
            lock(this)
            {
                //
                // If we already have an equivalent reference, use such equivalent
                // reference. Otherwise add the new reference to the reference
                // set.
                //
                WeakReference w = new WeakReference(@ref);
                WeakReference val = (WeakReference)_references[w];
                if(val != null)
                {
                    Reference r = (Reference)val.Target;
                    if(r != null && r.Equals(@ref))
                    {
                        return r;
                    }
                }
                _references[w] = w;
                return @ref;
            }
        }

        static private readonly string[] _suffixes =
        {
            "EndpointSelection",
            "ConnectionCached",
            "PreferSecure",
            "LocatorCacheTimeout",
            "Locator",
            "Router",
            "CollocationOptimized"
        };

        private void
        checkForUnknownProperties(String prefix)
        {
            //
            // Do not warn about unknown properties if Ice prefix, ie Ice, Glacier2, etc
            //
            for(int i = 0; IceInternal.PropertyNames.clPropNames[i] != null; ++i)
            {
                if(prefix.StartsWith(IceInternal.PropertyNames.clPropNames[i] + "."))
                {
                    return;
                }
            }

            ArrayList unknownProps = new ArrayList();
            Dictionary<string, string> props
                = instance_.initializationData().properties.getPropertiesForPrefix(prefix + ".");
            foreach(String prop in props.Keys)
            {
                bool valid = false;
                for(int i = 0; i < _suffixes.Length; ++i)
                {
                    if(prop.Equals(prefix + "." + _suffixes[i]))
                    {
                        valid = true;
                        break;
                    }
                }

                if(!valid)
                {
                    unknownProps.Add(prop);
                }
            }

            if(unknownProps.Count != 0)
            {
                string message = "found unknown properties for proxy '" + prefix + "':";
                foreach(string s in unknownProps)
                {
                    message += "\n    " + s;
                }
                instance_.initializationData().logger.warning(message);
            }
        }

        private Reference create(Ice.Identity ident,
                                 string facet,
                                 Reference.Mode mode,
                                 bool secure,
                                 EndpointI[] endpoints,
                                 string adapterId,
                                 string propertyPrefix)
        {
            DefaultsAndOverrides defaultsAndOverrides = instance_.defaultsAndOverrides();

            //
            // Default local proxy options.
            //
            LocatorInfo locatorInfo = instance_.locatorManager().get(_defaultLocator);
            RouterInfo routerInfo = instance_.routerManager().get(_defaultRouter);
            bool collocOptimized = defaultsAndOverrides.defaultCollocationOptimization;
            bool cacheConnection = true;
            bool preferSecure = defaultsAndOverrides.defaultPreferSecure;
            Ice.EndpointSelectionType endpointSelection = defaultsAndOverrides.defaultEndpointSelection;
            int locatorCacheTimeout = defaultsAndOverrides.defaultLocatorCacheTimeout;
        
            //
            // Override the defaults with the proxy properties if a property prefix is defined.
            //
            if(propertyPrefix != null && propertyPrefix.Length > 0)
            {
                Ice.Properties properties = instance_.initializationData().properties;

                //
                // Warn about unknown properties.
                //
                if(properties.getPropertyAsIntWithDefault("Ice.Warn.UnknownProperties", 1) > 0)
                {
                    checkForUnknownProperties(propertyPrefix);
                }
            
                string property;
            
                property = propertyPrefix + ".Locator";
                Ice.LocatorPrx locator = Ice.LocatorPrxHelper.uncheckedCast(_communicator.propertyToProxy(property));
                if(locator != null)
                {
                    locatorInfo = instance_.locatorManager().get(locator);
                }

                property = propertyPrefix + ".Router";
                Ice.RouterPrx router = Ice.RouterPrxHelper.uncheckedCast(_communicator.propertyToProxy(property));
                if(router != null)
                {
                    if(propertyPrefix.EndsWith(".Router"))
                    {
                        string s = "`" + property + "=" + properties.getProperty(property) +
                            "': cannot set a router on a router; setting ignored";
                        instance_.initializationData().logger.warning(s);
                    }
                    else
                    {
                        routerInfo = instance_.routerManager().get(router);
                    }
                }
    
                property = propertyPrefix + ".CollocationOptimized";
                collocOptimized = properties.getPropertyAsIntWithDefault(property, collocOptimized ? 1 : 0) > 0;

                property = propertyPrefix + ".ConnectionCached";
                cacheConnection = properties.getPropertyAsIntWithDefault(property, cacheConnection ? 1 : 0) > 0;

                property = propertyPrefix + ".PreferSecure";
                preferSecure = properties.getPropertyAsIntWithDefault(property, preferSecure ? 1 : 0) > 0;

                property = propertyPrefix + ".EndpointSelection";
                if(properties.getProperty(property).Length > 0)
                {
                    string type = properties.getProperty(property);
                    if(type.Equals("Random"))
                    {
                        endpointSelection = Ice.EndpointSelectionType.Random;
                    } 
                    else if(type.Equals("Ordered"))
                    {
                        endpointSelection = Ice.EndpointSelectionType.Ordered;
                    }
                    else
                    {
                        throw new Ice.EndpointSelectionTypeParseException(type);
                    }
                }
        
                property = propertyPrefix + ".LocatorCacheTimeout";
                locatorCacheTimeout = properties.getPropertyAsIntWithDefault(property, locatorCacheTimeout);
            }
        
            //
            // Create new reference
            //
            return updateCache(new RoutableReference(instance_, 
                                                     _communicator,
                                                     ident,
                                                     instance_.getDefaultContext(), 
                                                     facet,
                                                     mode,
                                                     secure,
                                                     endpoints,
                                                     adapterId,
                                                     locatorInfo,
                                                     routerInfo,
                                                     collocOptimized,
                                                     cacheConnection,
                                                     preferSecure,
                                                     endpointSelection,
                                                     locatorCacheTimeout));
        }

        private Instance instance_;
        private Ice.Communicator _communicator;
        private Ice.RouterPrx _defaultRouter;
        private Ice.LocatorPrx _defaultLocator;
        private Hashtable _references = new Hashtable();
    }

}
