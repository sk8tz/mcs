// 
// System.Web.Services.Protocols.SoapMessage.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.IO;
using System.Web.Services;

namespace System.Web.Services.Protocols {
	public abstract class SoapMessage {

		#region Fields

		string contentType;
		SoapException exception;
		SoapHeaderCollection headers;
		SoapMessageStage stage;

		#endregion // Fields

		#region Constructors

		internal SoapMessage ()
		{
			contentType = "text/xml";
			exception = null;
			headers = null;
		}

		#endregion // Fields

		#region Properties

		public abstract string Action {
			get;
		}

		public string ContentType {
			get { return contentType; }
			set { contentType = value; }
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

		public Stream Stream {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		public abstract string Url {
			get;
		}

		#endregion Properties

		#region Methods

		protected abstract void EnsureInStage ();
		protected abstract void EnsureOutStage ();

		[MonoTODO]
		protected void EnsureStage (SoapMessageStage stage) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object GetInParameterValue (int index) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object GetOutParameterValue (int index) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object GetReturnValue ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
