// 
// System.Web.Services.Protocols.SoapMessage.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.IO;
using System.Web.Services;

namespace System.Web.Services.Protocols {
	public abstract class SoapMessage {

		#region Fields

		string content_type = "text/xml";
		string content_encoding;
		SoapException exception = null;
		SoapHeaderCollection headers;
		SoapMessageStage stage;
		Stream stream;
		object[] inParameters;
		object[] outParameters;
		
		#endregion // Fields

		#region Constructors

		internal SoapMessage ()
		{
			headers = new SoapHeaderCollection ();
		}

		internal SoapMessage (Stream stream)
		{
			this.stream = stream;
		}

		internal SoapMessage (Stream stream, SoapException exception)
		{
			this.exception = exception;
			this.stream = stream;
			headers = new SoapHeaderCollection ();
		}

		#endregion

		#region Properties

		internal object[] InParameters 
		{
			get { return inParameters; }
			set { inParameters = value; }
		}

		internal object[] OutParameters 
		{
			get { return outParameters; }
			set { outParameters = value; }
		}

		public abstract string Action 
		{
			get;
		}

		public string ContentType {
			get { return content_type; }
			set { content_type = value; }
		}

		public SoapException Exception {
			get { return exception; }
		}

		public SoapHeaderCollection Headers {
			get { return headers; }
		}

		public abstract LogicalMethodInfo MethodInfo {
			get;
		}

		public abstract bool OneWay {
			get;
		}

		public SoapMessageStage Stage {
			get { return stage; }
		}

		internal void SetStage (SoapMessageStage stage)
		{
			this.stage = stage;
		}
		
		public Stream Stream {
			get {
				return stream;
			}
		}

		public abstract string Url {
			get;
		}
		
#if NET_1_1
		public string ContentEncoding
		{
			get { return content_encoding; }
			set { content_encoding = value; }
		}
#else
		internal string ContentEncoding
		{
			get { return content_encoding; }
			set { content_encoding = value; }
		}
#endif
 
		#endregion Properties

		#region Methods

		protected abstract void EnsureInStage ();
		protected abstract void EnsureOutStage ();

		protected void EnsureStage (SoapMessageStage stage) 
		{
			if ((((int) stage) & ((int) Stage)) == 0)
				throw new InvalidOperationException ("The current SoapMessageStage is not the asserted stage or stages.");
		}

		public object GetInParameterValue (int index) 
		{
			return inParameters [index];
		}

		public object GetOutParameterValue (int index) 
		{
			if (MethodInfo.IsVoid) return outParameters [index];
			else return outParameters [index + 1];
		}

		public object GetReturnValue ()
		{
			if (!MethodInfo.IsVoid && exception == null) return outParameters [0];
			else return null;
		}

		internal void SetHeaders (SoapHeaderCollection headers)
		{
			this.headers = headers;
		}

		internal void SetException (SoapException ex)
		{
			exception = ex;
		}

		internal void CollectHeaders (object target, HeaderInfo[] headers, SoapHeaderDirection direction)
		{
			Headers.Clear ();
			foreach (HeaderInfo hi in headers) 
			{
				if ((hi.Direction & direction) != 0) 
				{
					SoapHeader headerVal = hi.GetHeaderValue (target) as SoapHeader;
					if (headerVal != null)
						Headers.Add (headerVal);
				}
			}
		}

		internal void UpdateHeaderValues (object target, HeaderInfo[] headersInfo)
		{
			foreach (SoapHeader header in Headers)
			{
				HeaderInfo hinfo = FindHeader (headersInfo, header.GetType ());
				if (hinfo != null)
					hinfo.SetHeaderValue (target, header);
				else
					if (header.MustUnderstand)
					throw new SoapHeaderException ("Unknown header", SoapException.MustUnderstandFaultCode);
				header.DidUnderstand = false;
			}
		}

		HeaderInfo FindHeader (HeaderInfo[] headersInfo, Type headerType)
		{
			foreach (HeaderInfo headerInfo in headersInfo)
				if (headerInfo.HeaderType == headerType) return headerInfo;
			return null;
		}

		#endregion // Methods
	}
}
