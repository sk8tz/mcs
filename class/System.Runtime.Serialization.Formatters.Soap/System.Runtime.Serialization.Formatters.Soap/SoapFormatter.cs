// created on 07/04/2003 at 17:16
//
//	System.Runtime.Serialization.Formatters.Soap.SoapFormatter
//
//	Authors:
//		Jean-Marc Andre (jean-marc.andre@polymtl.ca)
//

using System;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Serialization;
using System.Runtime.Remoting.Messaging;
using System.Xml.Serialization;


namespace System.Runtime.Serialization.Formatters.Soap {
	enum RemMessageType {
		MethodCall, MethodResponse, ServerFault, NotRecognize
	}
	
	public class SoapFormatter: IRemotingFormatter, IFormatter {
		private ObjectWriter _objWriter;
		private SoapWriter _soapWriter;
		private SerializationBinder _binder;
		private StreamingContext _context;
		private ISurrogateSelector _selector;
		private FormatterAssemblyStyle _assemblyFormat = FormatterAssemblyStyle.Full;
		private ISoapMessage _topObject;
		
		public SoapFormatter() {
			
		}
		
		public SoapFormatter(ISurrogateSelector selector, StreamingContext context):this() {
			_selector = selector;
			_context = context;
		}
		
		~SoapFormatter() {
		}

	
		public object Deserialize(Stream serializationStream) {
			return Deserialize(serializationStream, null);
		}
		
		public object Deserialize(Stream serializationStream, HeaderHandler handler) {
			SoapParser parser = new SoapParser(serializationStream);
			SoapReader soapReader = new SoapReader(parser);
			soapReader.Binder = _binder;
			
			
			if(_topObject != null) soapReader.TopObject = _topObject;
			ObjectReader reader = new ObjectReader(_selector, _context, soapReader);
			parser.Run();
			object objReturn = reader.TopObject;
			return objReturn;
		}
		
		
		
		public void Serialize(Stream serializationStream, object graph) {
			Serialize(serializationStream, graph, null);
		}
		
		public void Serialize(Stream serializationStream, object graph, Header[] headers) {
			if(serializationStream == null)
				throw new ArgumentNullException("serializationStream");
			if(!serializationStream.CanWrite)
				throw new SerializationException("Can't write in the serialization stream");
			_soapWriter = new SoapWriter(serializationStream);
			_objWriter = new ObjectWriter((ISoapWriter) _soapWriter, _selector,  new StreamingContext(StreamingContextStates.File));
			_soapWriter.Writer = _objWriter;
			_objWriter.Serialize(graph);
			
		}
		
		public ISurrogateSelector SurrogateSelector {
			get {
				return _selector;
			}
			set {
				_selector = value;
			}
		}
		
		
		public SerializationBinder Binder {
			get {
				return _binder;
			}
			set {
				_binder = value;
			}
		}
		
		public StreamingContext Context {
			get {
				return _context;
			}
			set {
				_context = value;
			}
		}
		
		public ISoapMessage TopObject {
			get {
				return _topObject;
			}
			set {
				_topObject = value;
			}
		}
		
		[MonoTODO ("Interpret this")]
		public FormatterAssemblyStyle AssemblyFormat
		{
			get {
				return _assemblyFormat;
			}
			set {
				_assemblyFormat = value;
			}
		}

	}
}
