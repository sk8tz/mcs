// 
// System.Web.HttpResponse
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Globalization;

namespace System.Web {
   public class HttpResponse {
      // Chunked encoding static helpers
      private static byte [] s_arrChunkSuffix = { 10, 13 };
      private static byte [] s_arrChunkEnd = { 10 , 13 };
      private static string s_sChunkedPrefix = "\r\n";

      private ArrayList	_Headers;
		
      private bool _bClientDisconnected;
      private bool _bSuppressHeaders;
      private bool _bSuppressContent;
      private bool _bChunked;
      private bool _bEnded;
      private bool _bBuffering;
      private bool _bHeadersSent;
      private bool _bFlushing;
      private long _lContentLength;
      private int _iStatusCode;

      private bool _ClientDisconnected;

      private string	_sContentType;
      private string	_sCacheControl;
      private string	_sTransferEncoding;
      private string	_sCharset;
      private string	_sStatusDescription;

      private HttpCookieCollection _Cookies;
      private HttpCachePolicy _CachePolicy;

      private Encoding _ContentEncoding;
		
      private HttpContext _Context;
      private HttpWriter _Writer;
      private TextWriter _TextWriter;

      private HttpWorkerRequest _WorkerRequest;

      [MonoTODO("Verify that this really works")]
      internal HttpResponse(TextWriter output) {
         _bBuffering = true;
         _bFlushing = false;
         _bHeadersSent = false;

         _Headers = new ArrayList();

         _sContentType = "text/html";

         _iStatusCode = 200;
         _sCharset = null;
         _sCacheControl = null;

         _lContentLength = 0;
         _bSuppressContent = false;
         _bSuppressHeaders = false;
         _bClientDisconnected = false;

         _bChunked = false;

         _TextWriter = output;
      }

      internal HttpResponse(HttpWorkerRequest WorkerRequest, HttpContext Context) {
         _Context = Context;
         _WorkerRequest = WorkerRequest;

         _bBuffering = true;
         _bFlushing = false;
         _bHeadersSent = false;

         _Headers = new ArrayList();

         _sContentType = "text/html";

         _iStatusCode = 200;
         _sCharset = null;
         _sCacheControl = null;

         _lContentLength = 0;
         _bSuppressContent = false;
         _bSuppressHeaders = false;
         _bClientDisconnected = false;

         _bChunked = false;

         _Writer = new HttpWriter(this);
         _TextWriter = _Writer;
      }

      internal System.Text.Encoder ContentEncoder {
         get {
            return ContentEncoding.GetEncoder();
         }
      }

      internal void FinalFlush() {
         Flush(true);
      }

      [MonoTODO("We need to add cache headers also")]
      private ArrayList GenerateHeaders() {
         ArrayList oHeaders = new ArrayList(_Headers.ToArray());

         // save culture info, we need us info here
         CultureInfo oSavedInfo = System.Threading.Thread.CurrentThread.CurrentCulture;
         System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(0x0409);

         oHeaders.Add(new HttpResponseHeader("Date", DateTime.Now.ToUniversalTime().ToString("ddd, d MMM yyyy HH:mm:ss") + " GMT"));

         System.Threading.Thread.CurrentThread.CurrentCulture = oSavedInfo;

         if (_lContentLength > 0) {
            oHeaders.Add(new HttpResponseHeader(HttpWorkerRequest.HeaderContentLength, _lContentLength.ToString()));
         }

         if (_sContentType != null) {
            if (_sContentType.IndexOf("charset=") == -1) {
               if (Charset.Length == 0) {
                  Charset = ContentEncoding.HeaderName;
               }

               // Time to build our string
               if (Charset.Length > 0) {
                  _sContentType += "; charset=" + Charset;
               }
            }

            oHeaders.Add(new HttpResponseHeader(HttpWorkerRequest.HeaderContentType, _sContentType));
         }

         if (_sCacheControl != null) {
            oHeaders.Add(new HttpResponseHeader(HttpWorkerRequest.HeaderPragma, _sCacheControl));
         }

         if (_sTransferEncoding != null) {
            oHeaders.Add(new HttpResponseHeader(HttpWorkerRequest.HeaderTransferEncoding, _sTransferEncoding));
         }

         // TODO: Add Cookie headers..

         return oHeaders;
      }

      private void SendHeaders() {
         _WorkerRequest.SendStatus(StatusCode, StatusDescription);
			
         ArrayList oHeaders = GenerateHeaders();
         foreach (object oHeader in oHeaders) {
            ((HttpResponseHeader) oHeader).SendContent(_WorkerRequest);
         }
			
         _bHeadersSent = true;
      }

      public string Status {
         get {
            return StatusCode.ToString() + " " + StatusDescription;
         }

         set {
            string sMsg = "OK";
            int iCode = 200;

            try {
               iCode = Int32.Parse(value.Substring(0, value.IndexOf(' ')));
               sMsg = value.Substring(value.IndexOf(' ') + 1);
            }
            catch(Exception) {
               throw new HttpException("Invalid status string");
            }

            StatusCode = iCode;
            StatusDescription = sMsg;
         }
      }

      [MonoTODO()]
      public void AddCacheItemDependencies(ArrayList cacheKeys) {
         throw new NotImplementedException();
      }

      [MonoTODO()]
      public void AddCacheItemDependency(string cacheKey) {
         throw new NotImplementedException();
      }

      [MonoTODO()]
      public void AddFileDependencies(ArrayList filenames) {
         throw new NotImplementedException();
      }

      [MonoTODO()]
      public void AddFileDependency(string filename) {
         throw new NotImplementedException();
      }

      public void AddHeader(string name, string value) {
         AppendHeader(name, value);
      }

      [MonoTODO()]
      public void AppendCookie(HttpCookie cookie) {
         throw new NotImplementedException();
      }

      [MonoTODO()]
      public void AppendToLog(string param) {
         throw new NotImplementedException();
      }

      [MonoTODO()]
      public string ApplyAppPathModifier(string virtualPath) {
         throw new NotImplementedException();
      }

      public bool Buffer {
         get {
            return BufferOutput;
         }
			
         set {
            BufferOutput = value;
         }
      }

      public bool BufferOutput {
         get {
            return _bBuffering;
         }
			
         set {
            if (_Writer != null) {
               _Writer.Update();
            }

            _bBuffering = value;
         }
      }

      public HttpCachePolicy Cache {
         get {
            if (null == _CachePolicy) {
               _CachePolicy = new HttpCachePolicy();
            }

            return _CachePolicy;
         }
      }

      [MonoTODO("Set status in the cache policy")]
      public string CacheControl {
         get {
            return _sCacheControl;
         }

         set {
            if (_bHeadersSent) {
               throw new System.Web.HttpException("Headers has been sent to the client");
            }

            _sCacheControl = value;
         }
      }

      public string Charset {
         get {	
            if (null == _sCharset) {
               _sCharset = ContentEncoding.WebName;
            }

            return _sCharset;
         }
         set {
            if (_bHeadersSent) {
               throw new System.Web.HttpException("Headers has been sent to the client");
            }

            _sCharset = value;
         }
      }

      public Encoding ContentEncoding {
         get {
            if (_ContentEncoding == null) {
               _ContentEncoding = Encoding.UTF8;
            }

            return _ContentEncoding;
         }

         set {
            if (value == null) {
               throw new ArgumentException("Can't set a null as encoding");
            }

            _ContentEncoding = value;

            if (_Writer != null) {
               _Writer.Update();
            }
         }
      }

      public string ContentType {
         get {
            return _sContentType;
         }

         set {
            if (_bHeadersSent) {
               throw new System.Web.HttpException("Headers has been sent to the client");
            }

            _sContentType = value;
         }
      }

      public HttpCookieCollection Cookies {
         get {
            if (null == _Cookies) {
               _Cookies = new HttpCookieCollection(this, false);
            }
            return _Cookies;
         }
      }

      [MonoTODO("Set expires in the cache policy")]
      public int Expires {
         get {
            throw new NotImplementedException();
         }

         set {
            throw new NotImplementedException();
         }
      }

      [MonoTODO("Set expiresabsolute in the cache policy")]
      public int ExpiresAbsolute {
         get {
            throw new NotImplementedException();
         }

         set {
            throw new NotImplementedException();
         }
      }

      public Stream Filter {
         get {
            if (_Writer != null) {
               return _Writer.GetActiveFilter();
            }
            return null;
         }

         set {
            if (_Writer == null) {
               throw new HttpException("Filtering is not allowed");
            }

            _Writer.ActivateFilter(value);
         }
      }

      public bool IsClientConnected {
         get {
            if (_ClientDisconnected) {
               return false;
            }

            if (null != _WorkerRequest && (!_WorkerRequest.IsClientConnected())) {
               _ClientDisconnected = false;
               return false;
            }

            return true;
         }
      }
      
      public TextWriter Output {
         get {
            return _TextWriter;
         }
      }

      public Stream OutputStream {
         get {
            if (_Writer == null) {
               throw new System.Web.HttpException("a Output stream not available when running with custom text writer");
            }

            return _Writer.OutputStream;
         }
      }

      public string StatusDescription {
         get {
            if (null == _sStatusDescription) {
               _sStatusDescription = HttpWorkerRequest.GetStatusDescription(_iStatusCode);
            } 
               
            return _sStatusDescription;
         }

         set {
            if (_bHeadersSent) {
               throw new System.Web.HttpException("Headers has been sent to the client");
            }

            _sStatusDescription = value;
         }
      }
	
      public int StatusCode {
         get {
            return _iStatusCode;
         }
         set {
            if (_bHeadersSent) {
               throw new System.Web.HttpException("Headers has been sent to the client");
            }

            _sStatusDescription = null;
            _iStatusCode = value;
         }
      }

      public bool SuppressContent {
         get {
            return _bSuppressContent;
         }
			
         set {
            if (_bHeadersSent) {
               throw new System.Web.HttpException("Headers has been sent to the client");
            }

            _bSuppressContent = true;
         }
      }

      public HttpRequest Request {
         get {
            return _Context.Request;
         }
      }

      public void AppendHeader(int iIndex, string value) {
         if (_bHeadersSent) {
            throw new System.Web.HttpException("Headers has been sent to the client");
         }
         switch (iIndex) {
            case HttpWorkerRequest.HeaderContentLength :	
               _lContentLength = Int64.Parse(value);
               break;
            case HttpWorkerRequest.HeaderContentEncoding :	
               _sContentType = value;
               break;
            case HttpWorkerRequest.HeaderTransferEncoding :	
               _sTransferEncoding = value;
               if (value.Equals("chunked")) {
                  _bChunked = true;
               } else {
                  _bChunked = false;
               }
               break;

            case HttpWorkerRequest.HeaderPragma	:	
               _sCacheControl = value;
               break;

            default	:	
               _Headers.Add(new HttpResponseHeader(iIndex, value));
               break;
         }
      

      }

      public void AppendHeader(string name, string value) {
         if (_bHeadersSent) {
            throw new System.Web.HttpException("Headers has been sent to the client");
         }
	
         switch (name.ToLower()) {
            case "content-length"		:	_lContentLength = Int64.Parse(value);
               break;
            case "content-type"			:	_sContentType = value;
               break;
            case "transfer-encoding"	:	_sTransferEncoding = value;
               if (value.Equals("chunked")) {
                  _bChunked = true;
               } else {
                  _bChunked = false;
               }

               break;
            case "pragma"				:	_sCacheControl = value;
               break;

            default						:	_Headers.Add(new HttpResponseHeader(name, value));
               break;
         }
      }
	
      public void BinaryWrite(byte [] buffer) {
         OutputStream.Write(buffer, 0, buffer.Length);
      }

      public void Clear() {
         if (_Writer != null) {
            _Writer.Clear();
         }
      }

      public void ClearContent() {
         Clear();
      }

      public void ClearHeaders() {
         if (_bHeadersSent) {
            throw new System.Web.HttpException("Headers has been sent to the client");
         }

         _sContentType = "text/html";

         _iStatusCode = 200;
         _sCharset = null;
         _Headers = new ArrayList();
         _sCacheControl = null;
         _sTransferEncoding = null;

         _lContentLength = 0;
         _bSuppressContent = false;
         _bSuppressHeaders = false;
         _bClientDisconnected = false;
      }

      public void Close() {
         _bClientDisconnected = false;

         _WorkerRequest.CloseConnection();

         _bClientDisconnected = true;
      }

      internal void Dispose() {
         if (_Writer != null) {
            _Writer.Dispose();
            _Writer = null;
         }
      }

      [MonoTODO("Handle callbacks into before done with session, needs to have a non ended flush here")]
      internal void FlushAtEndOfRequest() 
      {
         Flush(true);
      }

      [MonoTODO("Check timeout and if we can cancel the thread...")]
      public void End() {
         if (!_bEnded) {
            Flush();
            _WorkerRequest.CloseConnection();

            _bEnded = true;
         }
      }

      public void Flush() {
         Flush(false);
      }

      private void Flush(bool bFinish) {
         if (_bFlushing) {
            return;
         }

         _bFlushing = true;

         if (_Writer != null) {
            _Writer.FlushBuffers();
         } else {
            _TextWriter.Flush();
         }

         try {
            if (!_bHeadersSent) {
               if (!_bSuppressHeaders && !_bClientDisconnected) {
                  if (_Writer != null && BufferOutput) {
                     _lContentLength = _Writer.BufferSize;
                  } 
                  else {
                     _lContentLength = 0;
                  }
		
                  if (_lContentLength == 0 && _iStatusCode == 200 && _sTransferEncoding == null) {
                     // Check we are going todo chunked encoding
                     string sProto = _Context.Request.ServerVariables["SERVER_PROTOCOL"];
                     if (sProto != null && sProto == "HTTP/1.1") {
                        AppendHeader(HttpWorkerRequest.HeaderTransferEncoding, "chunked");
                     }  
                     else {
                        // Just in case, the old browsers sends a HTTP/1.0 request with Connection: Keep-Alive
                        AppendHeader(HttpWorkerRequest.HeaderConnection, "Close");
                     }
                  } 

                  SendHeaders();
               }					
            }	
            if ((!_bSuppressContent && Request.HttpMethod.Equals("HEAD")) || _Writer == null) {
               _bSuppressContent = true;
            }

            if (!_bSuppressContent) {
               _bClientDisconnected = false;
               if (_bChunked) {
                  Encoding oASCII = Encoding.ASCII;

                  byte [] arrPrefix = oASCII.GetBytes(Convert.ToString(_Writer.BufferSize, 16) + s_sChunkedPrefix);
                  _WorkerRequest.SendResponseFromMemory(arrPrefix, arrPrefix.Length);

                  _Writer.SendContent(_WorkerRequest);

                  _WorkerRequest.SendResponseFromMemory(s_arrChunkSuffix, s_arrChunkSuffix.Length);
                  if (bFinish) {
                     _WorkerRequest.SendResponseFromMemory(s_arrChunkEnd, s_arrChunkEnd.Length);
                  }
               } 
               else {
                  _Writer.SendContent(_WorkerRequest);
               }

               _WorkerRequest.FlushResponse(bFinish);	

               if (!bFinish) {
                  _Writer.Clear();
               }
            }
         }
         finally {
            _bFlushing = false;
         }
      }

      public void Pics(string value) {
         AppendHeader("PICS-Label", value);
      }


      public void Redirect(string url) {
         Redirect(url, true);
      }

      public void Redirect(string url, bool endResponse) {
         if (_bHeadersSent) {
            throw new System.Web.HttpException("Headers has been sent to the client");
         }

         Clear();

         StatusCode = 302;
         AppendHeader(HttpWorkerRequest.HeaderLocation, url);

         // Text for browsers that can't handle location header
         Write("<html><head><title>Object moved</title></head><body>\r\n");
         Write("<h2>Object moved to <a href='" + url + "'>here</a></h2>\r\n");
         Write("</body><html>\r\n");

         if (endResponse) {
            End();
         }
      }

      public void Write(char ch) {
         _TextWriter.Write(ch);
      }

      public void Write(object obj) {
         _TextWriter.Write(obj);
      }

      public void Write(string str) {
         _TextWriter.Write(str);
      }

      public void Write(char [] buffer, int index, int count) {
         _TextWriter.Write(buffer, index, count);
      }

      [MonoTODO()]
      public static void RemoveOutputCacheItem(string path) {
         throw new NotImplementedException();
      }

      [MonoTODO()]
      public void SetCookie(HttpCookie cookie) {
         throw new NotImplementedException();
      }

      public void WriteFile(string filename) {
         WriteFile(filename, false);
      }

      [MonoTODO()]
      public void WriteFile(string filename, bool readIntoMemory) {
         throw new NotImplementedException();
      }
      
      [MonoTODO()]
      public void WriteFile(string filename, long offset, long size) {
         throw new NotImplementedException();
      }

      [MonoTODO("Should we support fileHandle ptrs?")]
      public void WriteFile(IntPtr fileHandle, long offset, long size) {
      }   

      [MonoTODO()]
      internal void OnCookieAdd(HttpCookie cookie) {
      }

      [MonoTODO("Do we need this?")]
      internal void OnCookieChange(HttpCookie cookie) {
      }

      [MonoTODO()]
      internal void GoingToChangeCookieColl() {
      }

      [MonoTODO()]
      internal void ChangedCookieColl() {
      }
   }
}
