//
// System.Runtime.Remoting.Channels.Ipc.IpcChannel.cs
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
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Channels.Ipc
{
        public class IpcChannel : IChannelReceiver, IChannelSender, IChannel
        {
                object _innerChannel;

                public IpcChannel ()
                {
                        _innerChannel = Activator.CreateInstance (IpcChannelFactory.LoadChannel ());
                }

                public IpcChannel (string portName)
                {
                        _innerChannel = Activator.CreateInstance (IpcChannelFactory.LoadChannel (), new object [] {portName});
                }

                public IpcChannel (IDictionary properties,
                                   IClientChannelSinkProvider clientSinkProvider,
                                   IServerChannelSinkProvider serverSinkProvider)
                {
                        _innerChannel = Activator.CreateInstance (IpcChannelFactory.LoadChannel (), new object [] {properties, clientSinkProvider, serverSinkProvider});
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

                public IMessageSink CreateMessageSink (string url,
                                                       object remoteChannelData,
                                                       out string objectUri)
                {
                        return ((IChannelSender)_innerChannel).CreateMessageSink (url, remoteChannelData, out objectUri);
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
