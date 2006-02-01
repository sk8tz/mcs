//
// System.Web.HttpRequest.cs 
//
// 
// Author:
//	Miguel de Icaza (miguel@novell.com)
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.Util;
using System.Globalization;

namespace System.Web {
	
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class HttpRequest {
		HttpWorkerRequest worker_request;
		HttpContext context;
		WebROCollection query_string_nvc;

		//
		string filename, query_string;
		UriBuilder uri_builder;

		string client_target;

		//
		// On-demand computed values
		//
		HttpBrowserCapabilities browser_capabilities;
		string file_path, base_virtual_dir, root_virtual_dir;
		string content_type;
		int content_length = -1;
		Encoding encoding;
		string current_exe_path;
		string physical_path;
		string path_info;
		WebROCollection all_params;
		WebROCollection headers;
		Stream input_stream;
		InputFilterStream input_filter;
		Stream filter;
		HttpCookieCollection cookies;
		string http_method;

		WebROCollection form;
		HttpFileCollection files;
		
		ServerVariablesCollection server_variables;
		HttpClientCertificate client_cert;
		
		string request_type;
		string [] accept_types;
		string [] user_languages;
		Uri cached_url;
		
		// Validations
		bool validate_cookies, validate_query_string, validate_form;
		bool checked_cookies, checked_query_string, checked_form;
		
		public HttpRequest (string filename, string url, string queryString)
		{
			// warning 169: what are we supposed to do with filename?
			
			this.filename = filename;

			uri_builder = new UriBuilder (url);
			query_string = queryString;
		}

		void InitUriBuilder ()
		{
			uri_builder = new UriBuilder ();
			uri_builder.Scheme = worker_request.GetProtocol ();
			uri_builder.Host = worker_request.GetServerName ();
			int port = worker_request.GetLocalPort ();
			uri_builder.Port = port;
			uri_builder.Path = worker_request.GetUriPath ();
			if (query_string != null && query_string != "")
				uri_builder.Query = query_string;
		}
		
		internal HttpRequest (HttpWorkerRequest worker_request, HttpContext context)
		{
			this.worker_request = worker_request;
			this.context = context;
			if (worker_request != null)
				query_string = worker_request.GetQueryString ();
		}

		string [] SplitHeader (int header_index)
		{
			string [] result = null;
			string header = worker_request.GetKnownRequestHeader (header_index);
			if (header != null && header != "" && header.Trim () != "") {
				result = header.Split (',');
				for (int i = result.Length - 1; i >= 0; i--)
					result [i] = result [i].Trim ();
			}
			return result;
		}

		public string [] AcceptTypes {
			get {
				if (worker_request == null)
					return null;

				if (accept_types == null)
					accept_types = SplitHeader (HttpWorkerRequest.HeaderAccept);

				return accept_types;
			}
		}

#if NET_2_0
		string anonymous_id;
		public string AnonymousID {
			get {
				return anonymous_id;
			}
			internal set {
				anonymous_id = value;
			}
		}
#endif

		public string ApplicationPath {
			get {
				if (worker_request == null)
					return null;
				return worker_request.GetAppPath ();
			}
		}

		public HttpBrowserCapabilities Browser {
			get {
				if (browser_capabilities == null)
					browser_capabilities = (HttpBrowserCapabilities)
						HttpCapabilitiesBase.GetConfigCapabilities (null, this);

				return browser_capabilities;
			}

			set {
				browser_capabilities = value;
			}
		}

		public HttpClientCertificate ClientCertificate {
			get {
				if (client_cert == null)
					client_cert = new HttpClientCertificate (worker_request);
				return client_cert;
			}
		}

		static internal string GetParameter (string header, string attr)
		{
			int ap = header.IndexOf (attr);
			if (ap == -1)
				return null;

			ap += attr.Length;
			if (ap >= header.Length)
				return null;
			
			char ending = header [ap];
			if (ending != '"')
				ending = ' ';
			
			int end = header.IndexOf (ending, ap+1);
			if (end == -1)
				return (ending == '"') ? null : header.Substring (ap);

			return header.Substring (ap+1, end-ap-1);
		}

		public Encoding ContentEncoding {
			get {
				if (encoding == null){
					if (worker_request == null)
						throw new HttpException ("No HttpWorkerRequest");
					
					string content_type = ContentType;
					string parameter = GetParameter (content_type, "; charset=");
					if (parameter == null) {
						encoding = WebEncoding.RequestEncoding;
					} else {
						try {
							// Do what the #1 web server does
							encoding = Encoding.GetEncoding (parameter);
						} catch {
							encoding = WebEncoding.RequestEncoding;
						}
					}
				}
				return encoding;
			}

			set {
				encoding = value;
			}
		}

		public int ContentLength {
			get {
				if (content_length == -1){
					if (worker_request == null)
						return 0;

					string cl = worker_request.GetKnownRequestHeader (HttpWorkerRequest.HeaderContentLength);

					if (cl != null) {
						try {
							content_length = Int32.Parse (cl);
						} catch { }
					}
				}

				// content_length will still be < 0, but we know we gotta read from the client
				if (content_length < 0)
					return 0;

				return content_length;
			}
		}

		public string ContentType {
			get {
				if (content_type == null){
					if (worker_request != null)
						content_type = worker_request.GetKnownRequestHeader (HttpWorkerRequest.HeaderContentType);

					if (content_type == null)
						content_type = String.Empty;
				}
				
				return content_type;
			}

			set {
				content_type = value;
			}
		}

		public HttpCookieCollection Cookies {
			get {
				if (cookies == null) {
					if (worker_request == null) {
						cookies = new HttpCookieCollection ();
					} else {
						string cookie_hv = worker_request.GetKnownRequestHeader (HttpWorkerRequest.HeaderCookie);
						cookies = new HttpCookieCollection (cookie_hv);
					}
				}

				if (validate_cookies && !checked_cookies){
					ValidateCookieCollection (cookies);
					checked_cookies = true;
				}

				return cookies;
			}

		}

		public string CurrentExecutionFilePath {
			get {
				if (current_exe_path != null)
					return current_exe_path;

				return FilePath;
			}
		}

		public string FilePath {
			get {
				if (worker_request == null)
					return "/"; // required for 2.0

				if (file_path == null)
					file_path = UrlUtils.Canonic (worker_request.GetFilePath ());

				return file_path;
			}
		}

		internal string BaseVirtualDir {
			get {
				if (base_virtual_dir == null){
					base_virtual_dir = FilePath;
					int p = base_virtual_dir.LastIndexOf ('/');
					if (p != -1)
						base_virtual_dir = base_virtual_dir.Substring (0, p);
				}
				return base_virtual_dir;
			}
		}
		
		public HttpFileCollection Files {
			get {
				if (files == null) {
					files = new HttpFileCollection ();
					if ((worker_request != null) && IsContentType ("multipart/form-data", true)) {
						form = new WebROCollection ();
						LoadMultiPart ();
						form.Protect ();
					}
				}
				return files;
			}
		}

		public Stream Filter {
			get {
				if (filter != null)
					return filter;

				if (input_filter == null)
					input_filter = new InputFilterStream ();

				return input_filter;
			}

			set {
				// This checks that get_ was called before.
				if (input_filter == null)
					throw new HttpException ("Invalid filter");

				filter = value;
			}
		}

		Stream StreamCopy (Stream stream)
		{
#if !TARGET_JVM
			if (stream is IntPtrStream)
				return new IntPtrStream (stream);
#endif

			if (stream is MemoryStream) {
				MemoryStream other = (MemoryStream) stream;
				return new MemoryStream (other.GetBuffer (), 0, (int) other.Length, false, true);
			}

			throw new NotSupportedException ("The stream is " + stream.GetType ());
		}

		//
		// Loads the data on the form for multipart/form-data
		//
		void LoadMultiPart ()
		{
			string boundary = GetParameter (ContentType, "; boundary=");
			if (boundary == null)
				return;

			Stream input = StreamCopy (InputStream);
			HttpMultipart multi_part = new HttpMultipart (input, boundary, ContentEncoding);

			HttpMultipart.Element e;
			while ((e = multi_part.ReadNextElement ()) != null) {
				if (e.Filename == null){
					byte [] copy = new byte [e.Length];
				
					input.Position = e.Start;
					input.Read (copy, 0, (int) e.Length);

					form.Add (e.Name, ContentEncoding.GetString (copy));
				} else {
					//
					// We use a substream, as in 2.x we will support large uploads streamed to disk,
					//
					HttpPostedFile sub = new HttpPostedFile (e.Filename, e.ContentType, input, e.Start, e.Length);
					files.AddFile (e.Name, sub);
				}
			}
		}

		//
		// Adds the key/value to the form, and sets the argumets to empty
		//
		void AddRawKeyValue (StringBuilder key, StringBuilder value)
		{
			form.Add (HttpUtility.UrlDecode (key.ToString (), ContentEncoding),
				  HttpUtility.UrlDecode (value.ToString (), ContentEncoding));

			key.Length = 0;
			value.Length = 0;
		}

		//
		// Loads the form data from on a application/x-www-form-urlencoded post
		// 
		void LoadWwwForm ()
		{
			Stream input = StreamCopy (InputStream);
			StreamReader s = new StreamReader (input, ContentEncoding);

			StringBuilder key = new StringBuilder ();
			StringBuilder value = new StringBuilder ();
			int c;

			while ((c = s.Read ()) != -1){
				if (c == '='){
					value.Length = 0;
					while ((c = s.Read ()) != -1){
						if (c == '&'){
							AddRawKeyValue (key, value);
							break;
						} else
							value.Append ((char) c);
					}
					if (c == -1){
						AddRawKeyValue (key, value);
						return;
					}
				} else if (c == '&')
					AddRawKeyValue (key, value);
				else
					key.Append ((char) c);
			}
			if (c == -1)
				AddRawKeyValue (key, value);
		}
		
		bool IsContentType (string ct, bool starts_with)
		{
			if (starts_with)
				return StrUtils.StartsWith (ContentType, ct, true);

			return String.Compare (ContentType, ct, true, CultureInfo.InvariantCulture) == 0;
		}
		
		public NameValueCollection Form {
			get {
				if (form == null){
					form = new WebROCollection ();
					files = new HttpFileCollection ();

					if (IsContentType ("application/x-www-form-urlencoded", true))
						LoadWwwForm ();
					else if (IsContentType ("multipart/form-data", true))
						LoadMultiPart ();

					form.Protect ();
				}

				if (validate_form && !checked_form){
					ValidateNameValueCollection ("Form", form);
					checked_form = true;
				}
				
				return form;
			}
		}

		public NameValueCollection Headers {
			get {
				if (headers == null){
					headers = new WebROCollection ();
					if (worker_request == null) {
						headers.Protect ();
						return headers;
					}

					for (int i = 0; i < HttpWorkerRequest.RequestHeaderMaximum; i++){
						string hval = worker_request.GetKnownRequestHeader (i);

						if (hval == null || hval == "")
							continue;
						
						headers.Add (HttpWorkerRequest.GetKnownRequestHeaderName (i), hval);
					}
				
					string [][] unknown = worker_request.GetUnknownRequestHeaders ();
					if (unknown != null && unknown.GetUpperBound (0) != -1){
						int top = unknown.GetUpperBound (0) + 1;
						
						for (int i = 0; i < top; i++){
							// should check if unknown [i] is not null, but MS does not. 
							
							headers.Add (unknown [i][0], unknown [i][1]);
						}
					}
					headers.Protect ();
				}
				return headers;
			}
		}

		public string HttpMethod {
			get {
				if (http_method == null){
					if (worker_request != null)
						http_method = worker_request.GetHttpVerbName ();
					else
						http_method = "GET";
				}
				return http_method;
			}
		}


#if TARGET_JVM	
		const int INPUT_BUFFER_SIZE = 1024;

		void MakeInputStream ()
		{
			if (worker_request == null)
				throw new HttpException ("No HttpWorkerRequest");

			// consider for perf:
			//    return ((ServletWorkerRequest)worker_request).InputStream();

			//
			// Use an unmanaged memory block as this might be a large
			// upload
			//
			int content_length = ContentLength;

			if (content_length == 0 && HttpMethod == "POST")
				throw new HttpException (411, "Length expected");
			
#if NET_2_0
			HttpRuntimeSection config = (HttpRuntimeSection) WebConfigurationManager.GetSection ("system.web/httpRuntime");
#else
			HttpRuntimeConfig config = (HttpRuntimeConfig) HttpContext.GetAppConfig ("system.web/httpRuntime");
#endif
			
			if (content_length > (config.MaxRequestLength * 1024))
				throw new HttpException ("File exceeds httpRuntime limit");
			
			byte[] content = new byte[content_length];
			if (content == null)
				throw new HttpException (String.Format ("Not enough memory to allocate {0} bytes", content_length));

			int total;
			byte [] buffer;
			buffer = worker_request.GetPreloadedEntityBody ();
			if (buffer != null){
				total = buffer.Length;
				Array.Copy (buffer, content, total);
			} else
				total = 0;
			
			
			buffer = new byte [INPUT_BUFFER_SIZE];
			while (total < content_length){
				int n;
				n = worker_request.ReadEntityBody (buffer, Math.Min (content_length-total, INPUT_BUFFER_SIZE));
				if (n <= 0)
					break;
				Array.Copy (buffer, 0, content, total, n);
				total += n;
			} 
			if (total < content_length)
				throw new HttpException (411, "The uploaded file is incomplete");
							 
			input_stream = new MemoryStream (content, 0, content.Length, false, true);
		}
#else
		const int INPUT_BUFFER_SIZE = 32*1024;

		void DoFilter (byte [] buffer)
		{
			if (input_filter == null || filter == null)
				return;

			if (buffer.Length < 1024)
				buffer = new byte [1024];

			// Replace the input with the filtered input
			input_filter.BaseStream = input_stream;
			MemoryStream ms = new MemoryStream ();
			while (true) {
				int n = filter.Read (buffer, 0, buffer.Length);
				if (n <= 0)
					break;
				ms.Write (buffer, 0, n);
			}
			// From now on input_stream has the filtered input
			input_stream = new MemoryStream (ms.GetBuffer (), 0, (int) ms.Length, false, true);
		}

		void MakeInputStream ()
		{
			if (input_stream != null)
				return;

			if (worker_request == null) {
				input_stream = new MemoryStream (new byte [0], 0, 0, false, true);
				DoFilter (new byte [1024]);
				return;
			}

			//
			// Use an unmanaged memory block as this might be a large
			// upload
			//
			int content_length = ContentLength;

#if NET_2_0
			HttpRuntimeSection config = (HttpRuntimeSection) WebConfigurationManager.GetSection ("system.web/httpRuntime");
#else
			HttpRuntimeConfig config = (HttpRuntimeConfig) HttpContext.GetAppConfig ("system.web/httpRuntime");
#endif
			if ((content_length / 1024) > config.MaxRequestLength)
				throw new HttpException (400, "Upload size exceeds httpRuntime limit.");

			int total = 0;
			byte [] buffer;
			buffer = worker_request.GetPreloadedEntityBody ();
			// we check the instance field 'content_length' here, not the local var.
			if (this.content_length == 0 || worker_request.IsEntireEntityBodyIsPreloaded ()) {
				if (buffer == null || content_length == 0) {
					input_stream = new MemoryStream (new byte [0], 0, 0, false, true);
				} else {
					input_stream = new MemoryStream (buffer, 0, buffer.Length, false, true);
				}
				DoFilter (new byte [1024]);
				return;
			}

			if (buffer != null)
				total = buffer.Length;

			if (content_length > 0) {
				total = Math.Min (content_length, total);
				IntPtr content = Marshal.AllocHGlobal (content_length);
				if (content == (IntPtr) 0)
					throw new HttpException (String.Format ("Not enough memory to allocate {0} bytes.",
									content_length));

				if (total > 0)
					Marshal.Copy (buffer, 0, content, total);

				if (total < content_length) {
					buffer = new byte [Math.Min (content_length, INPUT_BUFFER_SIZE)];
					do {
						int n;
						int min = Math.Min (content_length - total, INPUT_BUFFER_SIZE);
						n = worker_request.ReadEntityBody (buffer, min);
						if (n <= 0)
							break;
						Marshal.Copy (buffer, 0, (IntPtr) ((long)content + total), n);
						total += n;
					} while (total < content_length);
				}

				input_stream = new IntPtrStream (content, total);
				DoFilter (buffer);
			} else {
				MemoryStream ms = new MemoryStream ();
				if (total > 0)
					ms.Write (buffer, 0, total);
				buffer = new byte [INPUT_BUFFER_SIZE];
				long maxlength = config.MaxRequestLength * 1024;
				int n;
				while (true) {
					n = worker_request.ReadEntityBody (buffer, INPUT_BUFFER_SIZE);
					if (n <= 0)
						break;
					total += n;
					if (total < 0 || total > maxlength)
						throw new HttpException (400, "Upload size exceeds httpRuntime limit.");
					ms.Write (buffer, 0, n);
				}
				input_stream = new MemoryStream (ms.GetBuffer (), 0, (int) ms.Length, false, true);
				DoFilter (buffer);
			}

			if (total < content_length)
				throw new HttpException (411, "The request body is incomplete.");
		}
#endif
		internal void ReleaseResources ()
		{
			if (input_stream != null){
				input_stream.Close ();
				input_stream = null;
			}
		}
		
		public Stream InputStream {
			get {
				if (input_stream == null)
					MakeInputStream ();

				return input_stream;
			}
		}

		public bool IsAuthenticated {
			get {
				if (context.User == null || context.User.Identity == null)
					return false;
				return context.User.Identity.IsAuthenticated;
			}
		}

		public bool IsSecureConnection {
			get {
				if (worker_request == null)
					return false;
				return worker_request.IsSecure ();
			}
		}

		public string this [string key] {
			[AspNetHostingPermission (SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Low)]
			get {
				// "The QueryString, Form, Cookies, or ServerVariables collection member
				// specified in the key parameter."
				string val = QueryString [key];
				if (val == null)
					val = Form [key];
				if (val == null) {
					HttpCookie cookie = Cookies [key];
					if (cookie != null)
						val = cookie.Value;
				}
				if (val == null)
					val = ServerVariables [key];

				return val;
			}
		}

		public NameValueCollection Params {
			[AspNetHostingPermission (SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Low)]
			get {
				if (all_params == null) {
					all_params = new WebROCollection ();

					all_params.Add (QueryString);

					/* special handling for Cookies since
					 * it isn't a NameValueCollection. */
					foreach (string key in Cookies.AllKeys) {
						all_params.Add (key, Cookies[key].Value);
					}

					all_params.Add (Form);
					all_params.Add (ServerVariables);
					all_params.Protect ();
				}

				return all_params;
			}
		}

		public string Path {
			get {
				if (uri_builder == null)
					InitUriBuilder ();
				
				return uri_builder.Path;
			}
		}

		public string PathInfo {
			get {
				if (path_info == null) {
					if (worker_request == null)
						return String.Empty;
					path_info = worker_request.GetPathInfo ();
				}

				return path_info;
			}
		}

		public string PhysicalApplicationPath {
			get {
				if (worker_request == null)
					throw new ArgumentNullException (); // like 2.0, 1.x throws TypeInitializationException

				string path = HttpRuntime.AppDomainAppPath;
				if (SecurityManager.SecurityEnabled) {
					new FileIOPermission (FileIOPermissionAccess.PathDiscovery, path).Demand ();
				}
				return path;
			}
		}

		public string PhysicalPath {
			get {
				if (worker_request == null)
					return String.Empty; // don't check security with an empty string!

				if (physical_path == null)
					physical_path = MapPath (CurrentExecutionFilePath);

				if (SecurityManager.SecurityEnabled) {
					new FileIOPermission (FileIOPermissionAccess.PathDiscovery, physical_path).Demand ();
				}
				return physical_path;
			}
		}

		internal string RootVirtualDir {
			get {
				if (root_virtual_dir == null){
					string fp = FilePath;
					int p = fp.LastIndexOf ('/');

					if (p == -1)
						root_virtual_dir = "/";
					else
						root_virtual_dir = fp.Substring (0, p);
				}

				return root_virtual_dir;
			}
		}

		public NameValueCollection QueryString {
			get {
				if (query_string_nvc == null){
					query_string_nvc = new WebROCollection ();

					if (uri_builder == null)
						InitUriBuilder ();
					
					string q = query_string;
					if (q != null && q != ""){
						string [] components = q.Split ('&');
						foreach (string kv in components){
							int pos = kv.IndexOf ('=');
							if (pos == -1){
								query_string_nvc.Add (null, HttpUtility.UrlDecode (kv));
							} else {
								string key = HttpUtility.UrlDecode (kv.Substring (0, pos));
								string val = HttpUtility.UrlDecode (kv.Substring (pos+1));
								
								query_string_nvc.Add (key, val);
							}
						}
					}
					query_string_nvc.Protect ();
				}
				
				if (validate_query_string && !checked_query_string) {
					ValidateNameValueCollection ("QueryString", query_string_nvc);
					checked_query_string = true;
				}
				
				return query_string_nvc;
			}
		}

		public string RawUrl {
			get {
				if (worker_request != null)
					return worker_request.GetRawUrl ();
				else {
					if (query_string != null && query_string != "")
						return uri_builder.Path + "?" + query_string;
					else
						return uri_builder.Path;
				}
			}
		}

		//
		// "GET" or "SET"
		//
		public string RequestType {
			get {
				if (request_type == null){
					if (worker_request != null) {
						request_type = worker_request.GetHttpVerbName ();
						http_method = request_type;
					} else {
						request_type = "GET";
					}
				}
				return request_type;
			}

			set {
				request_type = value;
			}
		}

		public NameValueCollection ServerVariables {
			[AspNetHostingPermission (SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Low)]
			get {
				if (server_variables == null)
					server_variables = new ServerVariablesCollection (this);

				return server_variables;
			}
		}

		public int TotalBytes {
			get {
				Stream ins = InputStream;
				return (int) ins.Length;
			}
		}

		public Uri Url {
			get {
				if (uri_builder == null)
					InitUriBuilder ();
				
				if (cached_url == null) {
					UriBuilder builder = new UriBuilder (uri_builder.Uri);
					builder.Path += path_info;
					cached_url = builder.Uri;
				}
				return cached_url;
			}
		}

		public Uri UrlReferrer {
			get {
				if (worker_request == null)
					return null;

				string hr = worker_request.GetKnownRequestHeader (HttpWorkerRequest.HeaderReferer);
				if (hr == null)
					return null;

				return new Uri (hr);
			}
		}

		public string UserAgent {
			get {
				if (worker_request == null)
					return null;

				return worker_request.GetKnownRequestHeader (HttpWorkerRequest.HeaderUserAgent);
			}
		}

		public string UserHostAddress {
			get {
				if (worker_request == null)
					return null;

				return worker_request.GetRemoteAddress ();
			}
		}

		public string UserHostName {
			get {
				if (worker_request == null)
					return null;

				return worker_request.GetRemoteName ();
			}
		}

		public string [] UserLanguages {
			get {
				if (worker_request == null)
					return null;

				if (user_languages == null)
					user_languages = SplitHeader (HttpWorkerRequest.HeaderAcceptLanguage);

				return user_languages;
			}
		}

		public byte [] BinaryRead (int count)
		{
			if (count < 0)
				throw new ArgumentException ("count is < 0");

			Stream s = InputStream;
			byte [] ret = new byte [count];
			if (s.Read (ret, 0, count) != count)
				throw new ArgumentException (
					String.Format ("count {0} exceeds length of available input {1}",
						count, s.Length - s.Position));
			return ret;
		}

		public int [] MapImageCoordinates (string imageFieldName)
		{
			string method = HttpMethod;
			NameValueCollection coll = null;
			if (method == "HEAD" || method == "GET")
				coll = QueryString;
			else if (method == "POST")
				coll = Form;

			if (coll == null)
				return null;

			string x = coll [imageFieldName + ".x"];
			if (x == null || x == "")
				return null;

			string y = coll [imageFieldName + ".y"];
			if (y == null || y == "")
				return null;

			int [] result = new int [2];
			try {
				result [0] = Int32.Parse (x);
				result [1] = Int32.Parse (y);
			} catch {
				return null;
			}

			return result;
		}

		public string MapPath (string virtualPath)
		{
			if (worker_request == null)
				return null;

			return MapPath (virtualPath, BaseVirtualDir, true);
		}

		public string MapPath (string virtualPath, string baseVirtualDir, bool allowCrossAppMapping)
		{
			if (worker_request == null)
				throw new HttpException ("No HttpWorkerRequest");

			if (virtualPath == null || virtualPath == "")
				virtualPath = "";
			else
				virtualPath = virtualPath.Trim ();

			if (virtualPath.IndexOf (':') != -1)
				throw new ArgumentNullException (
					String.Format ("MapPath: Invalid path '{0}', only virtual paths are accepted", virtualPath));

#if TARGET_J2EE
 			if (virtualPath.StartsWith(vmw.common.IAppDomainConfig.WAR_ROOT_SYMBOL))			
 				return 	virtualPath;			
#endif 
			if (System.IO.Path.DirectorySeparatorChar != '/')
				virtualPath = virtualPath.Replace (System.IO.Path.DirectorySeparatorChar, '/');

			if (UrlUtils.IsRooted (virtualPath))
				virtualPath = UrlUtils.Canonic (virtualPath);
			else {
				if (baseVirtualDir == null)
					baseVirtualDir = RootVirtualDir;
				virtualPath = UrlUtils.Combine (baseVirtualDir, virtualPath);
			}

			if (!allowCrossAppMapping){
				if (!StrUtils.StartsWith (virtualPath, RootVirtualDir, true))
					throw new HttpException ("MapPath: Mapping across applications not allowed");
				if (RootVirtualDir.Length > 1 && virtualPath.Length > 1 && virtualPath [0] != '/')
					throw new HttpException ("MapPath: Mapping across applications not allowed");
			}
			return worker_request.MapPath (virtualPath);
		}

		public void SaveAs (string filename, bool includeHeaders)
		{
			Stream output = new FileStream (filename, FileMode.Create);
			if (includeHeaders) {
				StringBuilder sb = new StringBuilder ();
				string version = String.Empty;
				string path = "/";
				if (worker_request != null) {
					version = worker_request.GetHttpVersion ();
					InitUriBuilder ();
					path = uri_builder.Path;
				}
				string qs = null;
				if (query_string != null && query_string != "")
					qs = "?" + query_string;

				sb.AppendFormat ("{0} {1}{2} {3}\r\n", HttpMethod, path, qs, version);
				NameValueCollection coll = Headers;
				foreach (string k in coll.AllKeys) {
					sb.Append (k);
					sb.Append (':');
					sb.Append (coll [k]);
					sb.Append ("\r\n");
				}
				sb.Append ("\r\n");
				// latin1
				byte [] bytes = Encoding.GetEncoding (28591).GetBytes (sb.ToString ());
				output.Write (bytes, 0, bytes.Length);
			}

			// More than 1 call to SaveAs works fine on MS, so we "copy" the stream
			// to keep InputStream in its state.
			Stream input = StreamCopy (InputStream);
			try {
				long len = input.Length;
				int buf_size = (int) Math.Min ((len < 0 ? 0 : len), 8192);
				byte [] data = new byte [buf_size];
				int count = 0;
				while (len > 0 && (count = input.Read (data, 0, buf_size)) > 0) {
					output.Write (data, 0, count);
					len -= count;
				}
			} finally {
				output.Flush ();
				output.Close ();
			}
		}

		public void ValidateInput ()
		{
			validate_cookies = true;
			validate_query_string = true;
			validate_form = true;
		}

#region internal routines
		internal string ClientTarget {
			get {
				return client_target;
			}

			set {
				client_target = value;
			}
		}

#if NET_2_0
		public
#else
		internal
#endif
		bool IsLocal {
			get {
				string address = worker_request.GetRemoteAddress ();

				return (address == "127.0.0.1");
			}
		}

		internal void SetFilePath (string path)
		{
			file_path = path;
		}

                internal void SetCurrentExePath (string path)
                {
			cached_url = null;
			current_exe_path = path;
			file_path = path;
			if (uri_builder == null)
				InitUriBuilder ();
			uri_builder.Path = path;
			// recreated on demand
			root_virtual_dir = null;
			base_virtual_dir = null;
			physical_path = null;
                }

		internal void SetPathInfo (string pi)
		{
			cached_url = null;
			path_info = pi;
		}

		// Headers is ReadOnly, so we need this hack for cookie-less sessions.
		internal void SetHeader (string name, string value)
		{
			WebROCollection h = (WebROCollection) Headers;
			h.Unprotect ();
			h [name] = value;
			h.Protect ();
		}

		// Notice: there is nothing raw about this querystring.
		internal string QueryStringRaw {
			get {
				if (uri_builder == null)
					InitUriBuilder ();
				
				return query_string;
			}

			set {
				if (uri_builder == null)
					InitUriBuilder ();

				query_string = value;
				query_string_nvc = null;
				if (uri_builder != null)
					uri_builder.Query = value;
			}
		}

		// Internal, dont know what it does, so flagged as public so we can see it.
		internal void SetForm (WebROCollection coll)
		{
			form = coll;
		}

		internal HttpWorkerRequest WorkerRequest {
			get {
				return worker_request;
			}
		}

		internal HttpContext Context {
			get {
				return context;
			}
		}

		static void ValidateNameValueCollection (string name, NameValueCollection coll)
		{
			if (coll == null)
				return;
		
			foreach (string key in coll.Keys) {
				string val = coll [key];
				if (val != null && val != "" && CheckString (val))
					ThrowValidationException (name, key, val);
			}
		}
		
		static void ValidateCookieCollection (HttpCookieCollection cookies)
		{
			if (cookies == null)
				return;
		
			int size = cookies.Count;
			HttpCookie cookie;
			for (int i = 0 ; i < size ; i++) {
				cookie = cookies[i];
				string value = cookie.Value;
				
				if (value != null && value != "" && CheckString (value))
					ThrowValidationException ("Cookies", cookie.Name, cookie.Value);
			}
		}
		
		static void ThrowValidationException (string name, string key, string value)
		{
			string v = "\"" + value + "\"";
			if (v.Length > 20)
				v = v.Substring (0, 16) + "...\"";
		
			string msg = String.Format ("A potentially dangerous Request.{0} value was " +
						    "detected from the client ({1}={2}).", name, key, v);
		
			throw new HttpRequestValidationException (msg);
		}
		
		static bool CheckString (string val)
		{
			foreach (char c in val) {
				if (c == '<' || c == '>' || c == '\xff1c' || c == '\xff1e')
					return true;
			}
		
			return false;
		}

	}
#endregion

#region Helper classes
	
	//
	// Stream-based multipart handling.
	//
	// In this incarnation deals with an HttpInputStream as we are now using
	// IntPtr-based streams instead of byte [].   In the future, we will also
	// send uploads above a certain threshold into the disk (to implement
	// limit-less HttpInputFiles). 
	//
	
	class HttpMultipart {

		public class Element {
			public string ContentType;
			public string Name;
			public string Filename;
			public long Start;
			public long Length;
			
			public override string ToString ()
			{
				return string.Format ("ContentType {0}, Name {1}, Filename {2}, Start {3}, Length {4}",
					ContentType, Name, Filename, Start, Length);
			}
		}
		
		Stream data;
		string boundary;
		byte [] boundary_bytes;
		byte [] buffer;
		bool at_eof;
		Encoding encoding;
		StringBuilder sb;
		
		const byte HYPHEN = (byte) '-', LF = (byte) '\n', CR = (byte) '\r';
		
		// See RFC 2046 
		// In the case of multipart entities, in which one or more different
		// sets of data are combined in a single body, a "multipart" media type
		// field must appear in the entity's header.  The body must then contain
		// one or more body parts, each preceded by a boundary delimiter line,
		// and the last one followed by a closing boundary delimiter line.
		// After its boundary delimiter line, each body part then consists of a
		// header area, a blank line, and a body area.  Thus a body part is
		// similar to an RFC 822 message in syntax, but different in meaning.
		
		public HttpMultipart (Stream data, string b, Encoding encoding)
		{
			this.data = data;
			boundary = b;
			boundary_bytes = encoding.GetBytes (b);
			buffer = new byte [boundary_bytes.Length + 2]; // CRLF or '--'
			this.encoding = encoding;
			sb = new StringBuilder ();
		}

		string ReadLine ()
		{
			// CRLF or LF are ok as line endings.
			bool got_cr = false;
			int b = 0;
			sb.Length = 0;
			while (true) {
				b = data.ReadByte ();
				if (b == -1) {
					return null;
				}

				if (b == LF) {
					break;
				}
				got_cr = (b == CR);
				sb.Append ((char) b);
			}

			if (got_cr)
				sb.Length--;

			return sb.ToString ();

		}

		static string GetContentDispositionAttribute (string l, string name)
		{
			int idx = l.IndexOf (name + "=\"");
			if (idx < 0)
				return null;
			int begin = idx + name.Length + "=\"".Length;
			int end = l.IndexOf ('"', begin);
			if (end < 0)
				return null;
			if (begin == end)
				return "";
			return l.Substring (begin, end - begin);
		}

		bool ReadBoundary ()
		{
			try {
				string line = ReadLine ();
				while (line == "")
					line = ReadLine ();
				if (line [0] != '-' || line [1] != '-')
					return false;

				if (!StrUtils.EndsWith (line, boundary, false))
					return true;
			} catch {
			}

			return false;
		}

		string ReadHeaders ()
		{
			string s = ReadLine ();
			if (s == "")
				return null;

			return s;
		}

		bool CompareBytes (byte [] orig, byte [] other)
		{
			for (int i = orig.Length - 1; i >= 0; i--)
				if (orig [i] != other [i])
					return false;

			return true;
		}

		long MoveToNextBoundary ()
		{
			long retval = 0;
			bool got_cr = false;

			int state = 0;
			int c = data.ReadByte ();
			while (true) {
				if (c == -1)
					return -1;

				if (state == 0 && c == LF) {
					retval = data.Position - 1;
					if (got_cr)
						retval--;
					state = 1;
					c = data.ReadByte ();
				} else if (state == 0) {
					got_cr = (c == CR);
					c = data.ReadByte ();
				} else if (state == 1 && c == '-') {
					c = data.ReadByte ();
					if (c == -1)
						return -1;

					if (c != '-') {
						state = 0;
						got_cr = false;
						continue; // no ReadByte() here
					}

					int nread = data.Read (buffer, 0, buffer.Length);
					int bl = buffer.Length;
					if (nread != bl)
						return -1;

					if (!CompareBytes (boundary_bytes, buffer)) {
						state = 0;
						data.Position = retval + 2;
						if (got_cr) {
							data.Position++;
							got_cr = false;
						}
						c = data.ReadByte ();
						continue;
					}

					if (buffer [bl - 2] == '-' && buffer [bl - 1] == '-') {
						at_eof = true;
					} else if (buffer [bl - 2] != CR || buffer [bl - 1] != LF) {
						state = 0;
						data.Position = retval + 2;
						if (got_cr) {
							data.Position++;
							got_cr = false;
						}
						c = data.ReadByte ();
						continue;
					}
					data.Position = retval + 2;
					if (got_cr)
						data.Position++;
					break;
				} else {
					// state == 1
					state = 0; // no ReadByte() here
				}
			}

			return retval;
		}

		public Element ReadNextElement ()
		{
			if (at_eof || ReadBoundary ())
				return null;

			Element elem = new Element ();
			string header;
			while ((header = ReadHeaders ()) != null) {
				if (StrUtils.StartsWith (header, "Content-Disposition:", true)) {
					elem.Name = GetContentDispositionAttribute (header, "name");
					elem.Filename = GetContentDispositionAttribute (header, "filename");      
				} else if (StrUtils.StartsWith (header, "Content-Type:", true)) {
					elem.ContentType = header.Substring ("Content-Type:".Length).Trim ();
				}
			}

			long start = data.Position;
			elem.Start = start;
			long pos = MoveToNextBoundary ();
			if (pos == -1)
				return null;

			elem.Length = pos - start;
			return elem;
		}
		
	}
#endregion
}

