// 
// System.Web.Services.Protocols.WebClientProtocol.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System.Collections.Specialized;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Services;

namespace System.Web.Services.Protocols {
	public abstract class WebClientProtocol : Component {

		#region Fields

		string connectionGroupName;
		ICredentials credentials;
		bool preAuthenticate;
		Encoding requestEncoding;
		int timeout;
		string url;
		bool abort;
		static HybridDictionary cache;

		#endregion

		#region Constructors

		static WebClientProtocol ()
		{
			cache = new HybridDictionary ();
		}

		protected WebClientProtocol () 
		{
			connectionGroupName = String.Empty;
			credentials = null;
			preAuthenticate = false;
			requestEncoding = null;
			timeout = 100000;
			url = String.Empty;
			abort = false;
		}
		
		#endregion // Constructors

		#region Properties

		[DefaultValue ("")]
		public string ConnectionGroupName {
			get { return connectionGroupName; }
			set { connectionGroupName = value; }
		}

		public ICredentials Credentials {
			get { return credentials; }
			set { credentials = value; }
		}

		[DefaultValue (false)]
		[WebServicesDescription ("Enables pre authentication of the request.")]
		public bool PreAuthenticate {
			get { return preAuthenticate; }
			set { preAuthenticate = value; }
		}

		[DefaultValue ("")]
		[RecommendedAsConfigurable (true)]
		[WebServicesDescription ("The encoding to use for requests.")]
		public Encoding RequestEncoding {
			get { return requestEncoding; }
			set { requestEncoding = value; }
		}

		[DefaultValue (100000)]
		[RecommendedAsConfigurable (true)]
		[WebServicesDescription ("Sets the timeout in milliseconds to be used for synchronous calls.  The default of -1 means infinite.")]
		public int Timeout {
			get { return timeout; }
			set { timeout = value; }
		}

		[DefaultValue ("")]
		[RecommendedAsConfigurable (true)]
		[WebServicesDescription ("The base URL to the server to use for requests.")]
		public string Url {
			get { return url; }
			set { url = value; }
		}

		#endregion // Properties

		#region Methods

		public virtual void Abort ()
		{
			abort = true;
		}

		protected static void AddToCache (Type type, object value)
		{
			cache [type] = value;
		}

		protected static object GetFromCache (Type type)
		{
			return cache [type];
		}

		protected virtual WebRequest GetWebRequest (Uri uri)
		{
			return WebRequest.Create (uri);
		}

		protected virtual WebResponse GetWebResponse (WebRequest request)
		{
			if (abort)
				throw new WebException ("The operation has been aborted.", WebExceptionStatus.RequestCanceled);
			return request.GetResponse ();
		}

		protected virtual WebResponse GetWebResponse (WebRequest request, IAsyncResult result)
		{
			if (abort)
				throw new WebException ("The operation has been aborted.", WebExceptionStatus.RequestCanceled);

			IAsyncResult ar = request.BeginGetResponse (null, null);
			ar.AsyncWaitHandle.WaitOne ();
			return request.EndGetResponse (result);
		}

		#endregion // Methods
	}
}
