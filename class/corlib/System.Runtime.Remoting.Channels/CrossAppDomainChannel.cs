//
// System.Runtime.Remoting.Channels.CrossDomainChannel.cs
//
// Author: Patrik Torstensson (totte_mono@yahoo.com)
//
// 2003 (C) Copyright, Ximian, Inc.
//

using System.Collections;
using System.IO;
using System.Threading;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;   
using System.Runtime.Remoting.Channels; 
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace System.Runtime.Remoting.Channels 
{

	// Holds the cross appdomain channel data (used to get/create the correct sink)
	[Serializable]
	internal class CrossAppDomainChannelData 
	{
		// TODO: Add context support
		private int _domainId;

		internal CrossAppDomainChannelData(int domainId) 
		{
			_domainId = domainId;
		}

		internal int DomainID 
		{  
			get { return _domainId;	}
		}
	}

	// Responsible for marshalling objects between appdomains
	[Serializable]
	internal class CrossAppDomainChannel : IChannel, IChannelSender, IChannelReceiver 
	{
		private const String _strName = "MONOCAD";
		private const String _strBaseURI = "MONOCADURI";
		
		private static Object s_lock = new Object();

		internal static void RegisterCrossAppDomainChannel() 
		{
			lock (s_lock) 
			{
				// todo: make singleton
				CrossAppDomainChannel monocad = new CrossAppDomainChannel();
				ChannelServices.RegisterChannel ((IChannel) monocad);
			}
		}		

		// IChannel implementation
		public virtual String ChannelName 
		{
			get { return _strName; }
		}
    
		public virtual int ChannelPriority 
		{
			get { return 100; }
		}
		
		public String Parse(String url, out String objectURI) 
		{
			objectURI = url;
			return null;
		}	

		// IChannelReceiver
		public virtual Object ChannelData 
		{
			get { return new CrossAppDomainChannelData(Thread.GetDomainID()); }
		}	
		
		public virtual String[] GetUrlsForUri(String objectURI) 
		{
			throw new NotSupportedException("CrossAppdomain channel dont support UrlsForUri");
		}	
		
		// Dummies
		public virtual void StartListening(Object data) {}
		public virtual void StopListening(Object data) {}	

		// IChannelSender
		public virtual IMessageSink CreateMessageSink(String url, Object data, out String uri) 
		{
			uri = null;
			IMessageSink sink = null;
            
			if (url == null && data != null) 
			{
				// Get the data and then get the sink
				CrossAppDomainChannelData cadData = data as CrossAppDomainChannelData;
				if (cadData != null) 
					// GetSink creates a new sink if we don't have any (use contexts here later)
					sink = CrossAppDomainSink.GetSink(cadData.DomainID);
			} 
			else 
			{
				if (url != null && data == null) 
				{
					if (url.StartsWith(_strName)) 
					{
						throw new NotSupportedException("Can't create a named channel via crossappdomain");
					}
				}
			}

			return sink;
		}

	}
	
	[MonoTODO("Handle domain unloading?")]
	internal class CrossAppDomainSink : IMessageSink 
	{
		private static Hashtable s_sinks = new Hashtable();

		private int _domainID;

		internal CrossAppDomainSink(int domainID) 
		{
			_domainID = domainID;
		}
		
		internal static CrossAppDomainSink GetSink(int domainID) 
		{
			// Check if we have a sink for the current domainID
			// note, locking is not to bad here, very few class to GetSink
			lock (s_sinks.SyncRoot) 
			{
				if (s_sinks.ContainsKey(domainID)) 
					return (CrossAppDomainSink) s_sinks[domainID];
				else 
				{
					CrossAppDomainSink sink = new CrossAppDomainSink(domainID);
					s_sinks[domainID] = sink;

					return sink;
				}
			}
		}

		public virtual IMessage SyncProcessMessage(IMessage msgRequest) 
		{
			IMessage retMessage = null;

			try 
			{
				// Time to transit into the "our" domain
				byte [] arrResponse = null;
				byte [] arrRequest = null; 
				
				CADMethodReturnMessage cadMrm = null;
				CADMethodCallMessage cadMsg;
				
				cadMsg = CADMethodCallMessage.Create (msgRequest);
				if (null == cadMsg) {
					// Serialize the request message
					MemoryStream reqMsgStream = CADSerializer.SerializeMessage(msgRequest);
					arrRequest = reqMsgStream.GetBuffer();
				}

				// TODO: Use Contexts instead of domain id only
				AppDomain currentDomain = AppDomain.InternalSetDomainByID ( _domainID );
				try 
				{
					IMessage reqDomMsg;

					if (null != arrRequest)
						reqDomMsg = CADSerializer.DeserializeMessage (new MemoryStream(arrRequest), null);
					else
						reqDomMsg = new MethodCall (cadMsg);

					IMessage retDomMsg = ChannelServices.SyncDispatchMessage (reqDomMsg);

					cadMrm = CADMethodReturnMessage.Create (retDomMsg);
					if (null == cadMrm) {
						arrResponse = CADSerializer.SerializeMessage (retDomMsg).GetBuffer();
					} else
						arrResponse = null;

				}
				catch (Exception e) 
				{
					IMessage errorMsg = new MethodResponse (e, new ErrorMessage());
					arrResponse = CADSerializer.SerializeMessage (errorMsg).GetBuffer(); 
				}   
				finally 
				{
					AppDomain.InternalSetDomain (currentDomain);
				}
				
				if (null != arrResponse) {
					// Time to deserialize the message
					MemoryStream respMsgStream = new MemoryStream(arrResponse);

					// Deserialize the response message
					retMessage = CADSerializer.DeserializeMessage(respMsgStream, msgRequest as IMethodCallMessage);
				} else
					retMessage = new MethodResponse (msgRequest as IMethodCallMessage, cadMrm);
			}
			catch (Exception e) 
			{
				try
				{
					Console.WriteLine("Exception in base domain");
					retMessage = new ReturnMessage (e, msgRequest as IMethodCallMessage);
				}
				catch (Exception)
				{
					// this is just to be sure
				}
			}

		    	return retMessage;
		}

		public virtual IMessageCtrl AsyncProcessMessage(IMessage reqMsg, IMessageSink replySink) 
		{
			throw new NotSupportedException();
		}
		
		public IMessageSink NextSink { get { return null; } }
	}

	internal class CADSerializer 
	{
		internal static IMessage DeserializeMessage(MemoryStream mem, IMethodCallMessage msg)
		{
			BinaryFormatter serializer = new BinaryFormatter();                

			serializer.SurrogateSelector = null;
			mem.Position = 0;

			if (msg == null)
				return (IMessage) serializer.Deserialize(mem, null);
			else
				return (IMessage) serializer.DeserializeMethodResponse(mem, null, msg);
		}
		
		internal static MemoryStream SerializeMessage(IMessage msg)
		{
			MemoryStream mem = new MemoryStream ();
			BinaryFormatter serializer = new BinaryFormatter ();                

			serializer.SurrogateSelector = new RemotingSurrogateSelector ();
			serializer.Serialize (mem, msg);

			mem.Position = 0;

			return mem;
		}

		internal static MemoryStream SerializeObject(object obj)
		{
			MemoryStream mem = new MemoryStream ();
			BinaryFormatter serializer = new BinaryFormatter ();                

			serializer.SurrogateSelector = new RemotingSurrogateSelector ();
			serializer.Serialize (mem, obj);

			mem.Position = 0;

			return mem;
		}

		internal static object DeserializeObject(MemoryStream mem)
		{
			BinaryFormatter serializer = new BinaryFormatter();                

			serializer.SurrogateSelector = null;
			mem.Position = 0;

			return serializer.Deserialize (mem);
		}
	}
}
