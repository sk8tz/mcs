//
// System.Runtime.Remoting.Channels.CORBA.CORBAClientFormatterSink.cs
//
// Author: Dietmar Maurer (dietmar@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Collections;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Channels;

namespace System.Runtime.Remoting.Channels.CORBA
{
	public class CORBAClientFormatterSink : IClientFormatterSink,
		IMessageSink, IClientChannelSink, IChannelSinkBase
	{
		IClientChannelSink nextInChain;
		CDRFormatter format = new CDRFormatter ();
		
		public CORBAClientFormatterSink (IClientChannelSink nextSink)
		{
			nextInChain = nextSink;
		}

		public IClientChannelSink NextChannelSink
		{
			get {
				return nextInChain;
			}
		}

		public IMessageSink NextSink
		{
			get {
				return (IMessageSink) nextInChain;
			}
		}

		public IDictionary Properties
		{
			get {
				return null;
			}
		}

		public IMessageCtrl AsyncProcessMessage (IMessage msg,
							 IMessageSink replySink)
		{
			throw new NotImplementedException ();
		}

		public void AsyncProcessRequest (IClientChannelSinkStack sinkStack,
						 IMessage msg,
						 ITransportHeaders headers,
						 Stream stream)
	        {
			// never called because the formatter sink is
			// always the first in the chain
			throw new NotSupportedException ();
		}

		[MonoTODO]
		public void AsyncProcessResponse (IClientResponseChannelSinkStack sinkStack,
						  object state,
						  ITransportHeaders headers,
						  Stream stream)
		{
			throw new NotImplementedException ();
		}

		public Stream GetRequestStream (IMessage msg,
						ITransportHeaders headers)
		{
			// never called
			throw new NotSupportedException ();
		}

		public void ProcessMessage (IMessage msg,
					    ITransportHeaders requestHeaders,
					    Stream requestStream,
					    out ITransportHeaders responseHeaders,
					    out Stream responseStream)
		{
			// never called because the formatter sink is
			// always the first in the chain
			throw new NotSupportedException ();
		}

		public IMessage SyncProcessMessage (IMessage msg)
		{
			IMethodCallMessage call = (IMethodCallMessage)msg;
			
			// we catch all exceptions to return them as message
			try {
				// create a new header
				TransportHeaders req_headers = new TransportHeaders ();

				//fixme: set some header values

				Stream out_stream = new MemoryStream ();
			
				// serialize msg to the stream
				format.SerializeRequest (out_stream, msg);

				// call the next sink
				ITransportHeaders resp_headers;
				Stream resp_stream;
				nextInChain.ProcessMessage (msg, req_headers, out_stream, out resp_headers,
							    out resp_stream);

				// deserialize resp_stream
				IMessage result = (IMessage) format.DeserializeResponse (resp_stream, call);

				// it's save to close the stream now
				resp_stream.Close ();

				return result;
				
			} catch (Exception e) {
				 return new ReturnMessage (e, call);
			}
		}
	}
}
