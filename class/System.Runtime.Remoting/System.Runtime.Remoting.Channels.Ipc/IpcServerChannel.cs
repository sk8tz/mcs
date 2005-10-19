//
// System.Runtime.Remoting.Channels.Ipc.IpcServerChannel.cs
//
// Author: Robert Jordan (robertj@gmx.net)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0

using System;
using System.Collections;
using System.Runtime.Remoting;

namespace System.Runtime.Remoting.Channels.Ipc
{
        public class IpcServerChannel : IChannelReceiver, IChannel
        {
                object _innerChannel;

                public IpcServerChannel (string portName)
                {
                        _innerChannel = Activator.CreateInstance (IpcChannelFactory.LoadServerChannel (), new object [] {portName});
                }

                public IpcServerChannel (IDictionary properties,
                                         IServerChannelSinkProvider serverSinkProvider)
                {
                        _innerChannel = Activator.CreateInstance (IpcChannelFactory.LoadServerChannel (), new object [] {properties, serverSinkProvider});
                }

                public IpcServerChannel (string name, string portName,
                                         IServerChannelSinkProvider serverSinkProvider)
                {
                        _innerChannel = Activator.CreateInstance (IpcChannelFactory.LoadServerChannel (), new object [] {name, portName, serverSinkProvider});
                }
        
                public IpcServerChannel (string name, string portName)
                {
                        _innerChannel = Activator.CreateInstance (IpcChannelFactory.LoadServerChannel (), new object [] {name, portName});
                }

                public string ChannelName
                {
                        get { return ((IChannel)_innerChannel).ChannelName; }
                }

                public int ChannelPriority
                {
                        get { return ((IChannel)_innerChannel).ChannelPriority; }
                }

                public string Parse (string url, out string objectUri)
                {
                        return ((IChannel)_innerChannel).Parse (url, out objectUri);
                }

                public object ChannelData
                {
                        get { return ((IChannelReceiver)_innerChannel).ChannelData; }
                }

                public string[] GetUrlsForUri (string objectUri)
                {
                        return ((IChannelReceiver)_innerChannel).GetUrlsForUri (objectUri);
                }

                public void StartListening (object data)
                {
                        ((IChannelReceiver)_innerChannel).StartListening (data);
                }

                public void StopListening (object data)
                {
                        ((IChannelReceiver)_innerChannel).StopListening (data);
                }
        }
}

#endif
