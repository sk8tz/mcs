// 
// System.Web.HttpRequest
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//
using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Web.Configuration;

namespace System.Web {
	[MonoTODO("Review security in all path access function")]
	public sealed class HttpRequest {
		private string []	_arrAcceptTypes;
		private string []	_arrUserLanguages;

		private byte [] _arrRawContent;
		private int	_iContentLength;

		private string	_sContentType;
		private string	_sHttpMethod;
		private string	_sRawUrl;
		private string	_sUserAgent;
		private string	_sUserHostAddress;
		private string	_sUserHostName;
		private string	_sPath;
		private string	_sPathInfo;
		private string _sFilePath;
		private string _sPathTranslated;
		private string _sQueryStringRaw;
		private string _sRequestType;
		private string _sRequestRootVirtualDir;

		private Encoding _oContentEncoding;

		private Uri _oUriReferrer;
		private Uri _oUrl;

		private int	_iTotalBytes;

		private HttpContext	_oContext;

		private HttpWorkerRequest _WorkerRequest;
		private HttpRequestStream	_oInputStream;
		private HttpClientCertificate _ClientCert;

		private HttpValueCollection _oServerVariables;
		private HttpValueCollection _oHeaders;
		private HttpValueCollection _oQueryString;
		private HttpValueCollection _oFormData;
		private HttpValueCollection _oParams;

		private HttpBrowserCapabilities _browser;

		public HttpRequest(string Filename, string Url, string Querystring) {
			_iContentLength = -1;
			_iTotalBytes = -1;

			_WorkerRequest = null;
			_sPathTranslated = Filename;
			_sRequestType = "GET";

			_oUrl = new Uri(Url);
			_sPath = _oUrl.AbsolutePath;

			_sQueryStringRaw = Querystring;
			_oQueryString = new HttpValueCollection(Querystring, true, Encoding.Default);
		}

		internal HttpRequest(HttpWorkerRequest WorkRequest, HttpContext Context) {
			_WorkerRequest = WorkRequest;
			_oContext = Context;

			_iContentLength = -1;
			_iTotalBytes = -1;
		}

		static private string MakeServerVariableFromHeader(string header) {
			return "HTTP_" + header.ToUpper().Replace("-", "_");
		}

		[MonoTODO("Need to support non-raw mode also..")]
		private string GetAllHeaders(bool Raw) {
			StringBuilder oData;

			if (null == _WorkerRequest) {
				return null;
			}

			oData = new StringBuilder(512);

			string sHeaderValue;
			string sHeaderName;
			int iCount = 0;

			// Add all know headers
			for (; iCount != 40; iCount++) {
				sHeaderValue = _WorkerRequest.GetKnownRequestHeader(iCount);
				if (null != sHeaderValue && sHeaderValue.Length > 0) {
					sHeaderName = _WorkerRequest.GetKnownRequestHeader(iCount);
					if (null != sHeaderName && sHeaderName.Length > 0) {
						oData.Append(sHeaderName);
						oData.Append(": ");
						oData.Append(sHeaderValue);
						oData.Append("\r\n");
					}
				}
			}

			// Get all other headers
			string [][] arrUnknownHeaders = _WorkerRequest.GetUnknownRequestHeaders();
			if (null != arrUnknownHeaders) {
				for (iCount = 0; iCount != arrUnknownHeaders.Length; iCount++) {
					oData.Append(arrUnknownHeaders[iCount][0]);
					oData.Append(": ");
					oData.Append(arrUnknownHeaders[iCount][1]);
					oData.Append("\r\n");
				}
			}

			return oData.ToString();
		}
      
		[MonoTODO("We need to handly 'dynamic' variables like AUTH_USER, that can be changed during runtime... special collection")]
		private void ParseServerVariables() {
			if (null == _WorkerRequest) {
				return;
			}

			if (_oServerVariables == null) {
				string sTmp;

				_oServerVariables = new HttpValueCollection();
				
				_oServerVariables.Add("ALL_HTTP", GetAllHeaders(false));
				_oServerVariables.Add("ALL_RAW", GetAllHeaders(true));

				_oServerVariables.Add("APPL_MD_PATH", _WorkerRequest.GetServerVariable("APPL_MD_PATH"));
				_oServerVariables.Add("AUTH_PASSWORD", _WorkerRequest.GetServerVariable("AUTH_PASSWORD"));
				_oServerVariables.Add("CERT_COOKIE", _WorkerRequest.GetServerVariable("CERT_COOKIE"));
				_oServerVariables.Add("CERT_FLAGS", _WorkerRequest.GetServerVariable("CERT_FLAGS"));
				_oServerVariables.Add("CERT_ISSUER", _WorkerRequest.GetServerVariable("CERT_ISSUER"));
				_oServerVariables.Add("CERT_KEYSIZE", _WorkerRequest.GetServerVariable("CERT_KEYSIZE"));
				_oServerVariables.Add("CERT_SECRETKEYSIZE", _WorkerRequest.GetServerVariable("CERT_SECRETKEYSIZE"));
				_oServerVariables.Add("CERT_SERIALNUMBER", _WorkerRequest.GetServerVariable("CERT_SERIALNUMBER"));
				_oServerVariables.Add("CERT_SERVER_ISSUER", _WorkerRequest.GetServerVariable("CERT_SERVER_ISSUER"));
				_oServerVariables.Add("CERT_SERVER_SUBJECT", _WorkerRequest.GetServerVariable("CERT_SERVER_SUBJECT"));
				_oServerVariables.Add("CERT_SUBJECT", _WorkerRequest.GetServerVariable("CERT_SUBJECT"));

				_oServerVariables.Add("GATEWAY_INTERFACE", _WorkerRequest.GetServerVariable("GATEWAY_INTERFACE"));
				_oServerVariables.Add("HTTPS", _WorkerRequest.GetServerVariable("HTTPS"));
				_oServerVariables.Add("HTTPS_KEYSIZE", _WorkerRequest.GetServerVariable("HTTPS_KEYSIZE"));
				_oServerVariables.Add("HTTPS_SECRETKEYSIZE", _WorkerRequest.GetServerVariable("HTTPS_SECRETKEYSIZE"));

				_oServerVariables.Add("CONTENT_TYPE", ContentType);
				_oServerVariables.Add("HTTPS_SERVER_ISSUER", _WorkerRequest.GetServerVariable("HTTPS_SERVER_ISSUER"));
				_oServerVariables.Add("HTTPS_SERVER_SUBJECT", _WorkerRequest.GetServerVariable("HTTPS_SERVER_SUBJECT"));
				_oServerVariables.Add("INSTANCE_ID", _WorkerRequest.GetServerVariable("INSTANCE_ID"));
				_oServerVariables.Add("INSTANCE_META_PATH", _WorkerRequest.GetServerVariable("INSTANCE_META_PATH"));
				_oServerVariables.Add("LOCAL_ADDR", _WorkerRequest.GetLocalAddress());
				_oServerVariables.Add("REMOTE_ADDR", UserHostAddress);
				_oServerVariables.Add("REMOTE_HOST", UserHostName);
				_oServerVariables.Add("REQUEST_METHOD", HttpMethod);
				_oServerVariables.Add("SERVER_NAME", _WorkerRequest.GetServerName());
				_oServerVariables.Add("SERVER_PORT", _WorkerRequest.GetLocalPort().ToString());
				_oServerVariables.Add("SERVER_PROTOCOL", _WorkerRequest.GetHttpVersion());
				_oServerVariables.Add("SERVER_SOFTWARE", _WorkerRequest.GetServerVariable("SERVER_SOFTWARE"));

				if (_WorkerRequest.IsSecure()) {
					_oServerVariables.Add("SERVER_PORT_SECURE", "1");
				} else {
					_oServerVariables.Add("SERVER_PORT_SECURE", "0");
				}

				sTmp = _WorkerRequest.GetKnownRequestHeader(HttpWorkerRequest.HeaderContentLength);
				if (null != sTmp) {
					_oServerVariables.Add("CONTENT_LENGTH", sTmp);
				}

				// TODO: Should be dynamic
				if (null != _oContext.User && _oContext.User.Identity.IsAuthenticated) {
					_oServerVariables.Add("AUTH_TYPE", _oContext.User.Identity.AuthenticationType);
					_oServerVariables.Add("AUTH_USER", _oContext.User.Identity.Name);
				} else {
					_oServerVariables.Add("AUTH_TYPE", "");
					_oServerVariables.Add("AUTH_USER", "");
				}

				_oServerVariables.Add("PATH_INFO", PathInfo);
				_oServerVariables.Add("PATH_TRANSLATED", PhysicalPath);
				_oServerVariables.Add("QUERY_STRING", QueryStringRaw);
				_oServerVariables.Add("SCRIPT_NAME", FilePath);
				// end dynamic
            
				_oServerVariables.MakeReadOnly();
			}
		}

		[MonoTODO("Handle Multipart data also, content-encoding check")]
		private void ParseFormData() {
			if (_oFormData == null) {
				byte [] arrData = GetRawContent();
				_oFormData = new HttpValueCollection(ContentEncoding.GetString(arrData), true, ContentEncoding);
			}
		}

		[MonoTODO("void Dispose")]
		internal void Dispose() {			
		}

		[MonoTODO("Handle Multipart data, max content length?")]
		private byte [] GetRawContent() {
			if (null == _arrRawContent) {
				if (null == _WorkerRequest) {
					if (QueryStringRaw == null)
						return null;
					char [] q = QueryStringRaw.ToCharArray ();
					_arrRawContent = new byte [q.Length];
					for (int i = 0; i < q.Length; i++)
						_arrRawContent [i] = (byte) q [i];
					return _arrRawContent;
				}

				// TODO: Check max length?
				_arrRawContent = _WorkerRequest.GetPreloadedEntityBody();
				if (!_WorkerRequest.IsEntireEntityBodyIsPreloaded()) {
					byte [] arrTemp;
					byte [] arrBuffer;
					
					arrBuffer = new byte[16384];
					int iLoaded = 16384;

					while (iLoaded == arrBuffer.Length) {
						iLoaded = _WorkerRequest.ReadEntityBody(arrBuffer, arrBuffer.Length);
						
						// Save data
						arrTemp = new byte[_arrRawContent.Length + iLoaded];
						
						Array.Copy(_arrRawContent, 0, arrTemp, 0, _arrRawContent.Length);
						Array.Copy(arrBuffer, 0, arrTemp, _arrRawContent.Length, iLoaded);

						_arrRawContent = arrTemp;
					}
				}
			} 

			return _arrRawContent;
		}

		public string [] AcceptTypes {
			get {
				if (null == _arrAcceptTypes && null != _WorkerRequest) {
					_arrAcceptTypes = HttpHelper.ParseMultiValueHeader(_WorkerRequest.GetKnownRequestHeader(HttpWorkerRequest.HeaderAccept));
				} 

				return _arrAcceptTypes;
				
			}
		}

		public string ApplicationPath {
			get {
				if (null != _WorkerRequest) {
					return _WorkerRequest.GetAppPath();
				}

				return null;
			}
		}

		public HttpBrowserCapabilities Browser {
			get { return _browser; }

			set { _browser = value; }
		}

		public HttpClientCertificate ClientCertificate {
			get {
				if (null == _ClientCert) {
					_ClientCert = new HttpClientCertificate(_oContext);
				}

				return _ClientCert;
			}
		}

		[MonoTODO("Get content encoding from Syste.Web.Configuration namespace")]
		public Encoding ContentEncoding {
			get {
				if (_oContentEncoding == null) {
					// TODO: Get from config what is the default encoding
					// TODO: Should we get encoding from HttpHeaders? Just get charset from ContentType and the get encoding..
					_oContentEncoding = Encoding.Default;
				}

				return _oContentEncoding;
			}

			set {
				_oContentEncoding = value;
			}
		}

		public int ContentLength {
			get {
				if (_iContentLength == -1 && null != _WorkerRequest) {
					string sLength = _WorkerRequest.GetKnownRequestHeader(HttpWorkerRequest.HeaderContentLength);
					if (sLength != null) {
						try {
							_iContentLength = Int32.Parse(sLength);
						}
						catch(Exception) {
						}
					} 
				}

				if (_iContentLength < 0) {
					_iContentLength = 0;
				}

				return _iContentLength;
			}
		}

		public string ContentType {
			get {
				if (null == _sContentType) {
					if (null != _WorkerRequest) {
						_sContentType = _WorkerRequest.GetKnownRequestHeader(HttpWorkerRequest.HeaderContentType);
					}

					if (null == _sContentType) {
						_sContentType = string.Empty;
					}
				}

				return _sContentType;
			}
		}

		[MonoTODO()]
		public HttpCookieCollection Cookies {
			get {
				throw new NotImplementedException();
			}
		}

		[MonoTODO()]
		public string CurrentExecutionFilePath {
			get {
				throw new NotImplementedException();
			}
		}

		public string FilePath {
			get {
				if (null == _sFilePath && null != _WorkerRequest) {
					_sFilePath = _WorkerRequest.GetFilePath();
				}

				return _sFilePath;
			}
		}

		[MonoTODO()]
		public HttpFileCollection Files {
			get {
				throw new NotImplementedException();
			}
		}

		[MonoTODO("Use stream filter in the request stream")]
		public Stream Filter {
			get {
				throw new NotImplementedException();
			}

			set {
				throw new NotImplementedException();
			}
		}


		public NameValueCollection Form {
			get {
				ParseFormData();

				return (NameValueCollection) _oFormData;
			}
		}

		public NameValueCollection Headers {
			get {
				if (_oHeaders == null) {
					_oHeaders = new HttpValueCollection();

					if (null != _WorkerRequest) {
						string sHeaderValue;
						string sHeaderName;
						int iCount = 0;

						// Add all know headers
						for (; iCount != 40; iCount++) {
							sHeaderValue = _WorkerRequest.GetKnownRequestHeader(iCount);
							if (null != sHeaderValue && sHeaderValue.Length > 0) {
								sHeaderName = _WorkerRequest.GetKnownRequestHeader(iCount);
								if (null != sHeaderName && sHeaderName.Length > 0) {
									_oHeaders.Add(sHeaderName, sHeaderValue);
								}
							}
						}

						// Get all other headers
						string [][] arrUnknownHeaders = _WorkerRequest.GetUnknownRequestHeaders();
						if (null != arrUnknownHeaders) {
							for (iCount = 0; iCount != arrUnknownHeaders.Length; iCount++) {
								_oHeaders.Add(arrUnknownHeaders[iCount][0], arrUnknownHeaders[iCount][1]);
							}
						}
					}

					// Make headers read-only
					_oHeaders.MakeReadOnly();
				}

				return (NameValueCollection) _oHeaders;
			}
		}

		public string HttpMethod {
			get {
				if (null == _sHttpMethod) {
					if (null != _WorkerRequest) {
						_sHttpMethod = _WorkerRequest.GetHttpVerbName().ToUpper();
					}
               
					if (_sHttpMethod == null) {
						if (RequestType != null)
							_sHttpMethod = RequestType;
						else
							_sHttpMethod = "GET";
					}
				}

				return _sHttpMethod;
			}
		}

		public Stream InputStream {
			get {
				if (_oInputStream == null) {
					byte [] arrInputData = GetRawContent();

					if (null != arrInputData) {
						_oInputStream = new HttpRequestStream(arrInputData, 0, arrInputData.Length);
					} else {
						_oInputStream = new HttpRequestStream(null, 0, 0);
					}
				}

				return _oInputStream;
			} 
		}

		public bool IsAuthenticated {
			get {
				if (_oContext != null && _oContext.User != null && _oContext.User.Identity != null) {
					return _oContext.User.Identity.IsAuthenticated;
				}

				return false;
			}
		}

		public bool IsSecureConnection {
			get {
				if (null != _WorkerRequest) {
					return _WorkerRequest.IsSecure();
				}

				return false;
			}
		}

		[MonoTODO("Call item in querystring, form, cookie and servervariables")]
		public string this [string sKey] {
			get {
				throw new NotImplementedException();
			}
		}

		[MonoTODO("Add cookie collection to our Params collection via merge")]
		public NameValueCollection Params {
			get {
				if (_oParams == null) {
					_oParams = new HttpValueCollection();
					
					_oParams.Merge(QueryString);
					_oParams.Merge(Form);
					_oParams.Merge(ServerVariables);
					// TODO: Cookie

					_oParams.MakeReadOnly();
				}

				return (NameValueCollection) _oParams;
			}
		}
		
		public string Path {
			get {
				if (_sPath == null) {
					if (null != _WorkerRequest) {
						_sPath = _WorkerRequest.GetUriPath();
					}

					if (_sPath == null) {
						_sPath = string.Empty;
					}
				}

				return _sPath;
			}
		}
		
		public string PathInfo {
			get {
				if (_sPathInfo == null) {
					if (null != _WorkerRequest) {
						_sPathInfo = _WorkerRequest.GetPathInfo();
					}

					if (_sPathInfo == null) {
						_sPathInfo = string.Empty;
					}
				}
				
				return _sPathInfo;
			}
		}

		public string PhysicalApplicationPath {
			get {
				if (null != _WorkerRequest) {
					return _WorkerRequest.GetAppPathTranslated();
				}

				return null;
			}
		}

		public string PhysicalPath {
			get {
				if (null != _WorkerRequest) {
					_sPathTranslated = _WorkerRequest.GetFilePathTranslated();
					if (null == _sPathTranslated) {
						_sPathTranslated = _WorkerRequest.MapPath(FilePath);
					}
				}

				return _sPathTranslated;
			}
		}

		public NameValueCollection QueryString {
			get {
				if (_oQueryString == null) {
					_oQueryString = new HttpValueCollection(QueryStringRaw, true, Encoding.UTF8);
				}

				return _oQueryString;
			}
		}

		// Used to parse the querystring
		internal string QueryStringRaw {
			get {
				if (_sQueryStringRaw == null && null != _WorkerRequest) {
					byte [] arrQuerystringBytes = _WorkerRequest.GetQueryStringRawBytes();
					if (null != arrQuerystringBytes && arrQuerystringBytes.Length > 0) {
						_sQueryStringRaw = ContentEncoding.GetString(arrQuerystringBytes);
					} else {
						_sQueryStringRaw = _WorkerRequest.GetQueryString();   
					}
				}

				if (_sQueryStringRaw == null) {
					_sQueryStringRaw = string.Empty;
				}

				return _sQueryStringRaw;
			}
		}

		public string RawUrl {
			get {
				if (null == _sRawUrl) {
					if (null != _WorkerRequest) {
						_sRawUrl = _WorkerRequest.GetRawUrl();
					} else {
						_sRawUrl = Path;
						if (QueryStringRaw != null && QueryStringRaw.Length > 0) {
							_sRawUrl = _sRawUrl + "?" + QueryStringRaw;
						}
					}
				}

				return _sRawUrl;
			}
		}

		public string RequestType {
			get {
				if (null == _sRequestType) {
					return HttpMethod;
				}
         
				return _sRequestType;
			}

			set {
				_sRequestType = value;
			}
		}
		
      
		public NameValueCollection ServerVariables {
			get {
				ParseServerVariables();

				return (NameValueCollection) _oServerVariables;
			}
		}      

		public int TotalBytes {
			get {
				if (_iTotalBytes == -1) {
					if (null != InputStream) {
						_iTotalBytes = (int) InputStream.Length;
					} else {
						_iTotalBytes = 0;
					}
				}

				return _iTotalBytes;
			}
		}

		public Uri Url {
			get {
				if (null == _oUrl) {
					_oUrl = new Uri(RawUrl);
				}

				return _oUrl;
			}
		}

		public Uri UrlReferrer {
			get {
				if (null == _oUriReferrer && null != _WorkerRequest) {
					string sReferrer = _WorkerRequest.GetKnownRequestHeader(HttpWorkerRequest.HeaderReferer);
					if (null != sReferrer && sReferrer.Length > 0) {
						try {
							if (sReferrer.IndexOf("://") >= 0) {
								_oUriReferrer = new Uri(sReferrer);
							} else {
								_oUriReferrer = new Uri(this.Url, sReferrer);
							}
						}
						catch (Exception) {
						}
					}
				}

				return _oUriReferrer;
			}
		}

		public string UserAgent {
			get {
				if (_sUserAgent == null && _WorkerRequest != null) {
					_sUserAgent = _WorkerRequest.GetKnownRequestHeader(HttpWorkerRequest.HeaderUserAgent);
				}

				if (_sUserAgent == null) {
					_sUserAgent = string.Empty;
				}

				return _sUserAgent;
			}
		}

		public string UserHostAddress {
			get {
				if (_sUserHostAddress == null && null != _WorkerRequest) {
					_sUserHostAddress = _WorkerRequest.GetRemoteAddress();
				}

				if (_sUserHostAddress == null || _sUserHostAddress.Length == 0) {
					_sUserHostAddress = "127.0.0.1";
				}

				return _sUserHostAddress;
			}
		}
		
		public string UserHostName {
			get {
				if (_sUserHostName == null && null != _WorkerRequest) {
					_sUserHostName = _WorkerRequest.GetRemoteName();
				}

				if (_sUserHostName == null || _sUserHostName.Length == 0) {
					_sUserHostName = UserHostAddress;
				}

				return _sUserHostName;
			}
		}
		
		public string [] UserLanguages {
			get {
				if (_arrUserLanguages == null && null != _WorkerRequest) {
					_arrUserLanguages = HttpHelper.ParseMultiValueHeader(_WorkerRequest.GetKnownRequestHeader(HttpWorkerRequest.HeaderAcceptLanguage));
				}

				return _arrUserLanguages;
			}
		}

		public byte [] BinaryRead(int count) {
			int iSize = TotalBytes;
			if (iSize == 0) {
				throw new ArgumentException();
			}

			byte [] arrData = new byte[iSize];
			
			int iRetSize = InputStream.Read(arrData, 0, iSize);
			if (iRetSize != iSize) {
				byte [] tmpData = new byte[iRetSize];
				if (iRetSize > 0) {
					Array.Copy(arrData, 0, tmpData, 0, iRetSize);
				}

				arrData = tmpData;
			}

			return arrData;
		}

		public int [] MapImageCoordinates(string ImageFieldName) {
			NameValueCollection oItems;

			if (HttpMethod == "GET" || HttpMethod == "HEAD") {
				oItems = QueryString;
			} else if (HttpMethod == "POST") {
				oItems = Form;
			} else {
				return null;
			}

			int [] arrRet = null;
			try {
				string sX = oItems.Get(ImageFieldName + ".x");
				string sY = oItems.Get(ImageFieldName + ".y");

				if (null != sX && null != sY) {
					int [] arrTmp = new Int32[2];
					arrRet[0] = Int32.Parse(sX);
					arrRet[1] = Int32.Parse(sY);

					arrRet = arrTmp;
				}
			}
			catch (Exception) {
			}

			return arrRet;
		}

		public string MapPath(string VirtualPath) {
			if (null == _sRequestRootVirtualDir) {
				if (null == FilePath || FilePath.Length == 0) {
					throw new ArgumentException("Filepath can't be empty");
				}

				if (FilePath[0] != '/') {
					throw new ArgumentException("Filepath must be a root");
				}

				_sRequestRootVirtualDir = FilePath.Substring(0, FilePath.LastIndexOf('/'));
				if (_sRequestRootVirtualDir.Length == 0) {
					_sRequestRootVirtualDir = "/";
				}
			}

			return MapPath(VirtualPath, _sRequestRootVirtualDir, true);
		}

		[MonoTODO("Build a path to send to MapPath in the workerrequest")]
		public string MapPath(string virtualPath, string baseVirtualDir, bool allowCrossAppMapping) {
			throw new NotImplementedException();
		}

		public void SaveAs(string filename, bool includeHeaders) {
			FileStream oFile;
			TextWriter oWriter;
			HttpRequestStream oData;

			oFile = new FileStream(filename, FileMode.CreateNew);
			if (includeHeaders) {
				oWriter = new StreamWriter(oFile);
				oWriter.Write(HttpMethod + " " + Path);

				if (QueryStringRaw != null && QueryStringRaw.Length > 0)
					oWriter.Write("?" + QueryStringRaw);
				if (_WorkerRequest != null) {
					oWriter.Write(" " + _WorkerRequest.GetHttpVersion() + "\r\n");
					oWriter.Write(GetAllHeaders(true));
				} else {
					oWriter.Write("\r\n");
				}

				oWriter.Write("\r\n");
				oWriter.Flush();
			}

			oData = (HttpRequestStream) InputStream;

			if (oData.DataLength > 0) {
				oFile.Write(oData.Data, oData.DataOffset, oData.DataLength);
			}

			oFile.Flush();
			oFile.Close();
		}
	}
}
