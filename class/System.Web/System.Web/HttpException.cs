// 
// System.Web.HttpException
//
// Authors:
// 	Patrik Torstensson (Patrik.Torstensson@labs2.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2002 Patrik Torstensson
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
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

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Web.Util;
using System.Web.Compilation;

namespace System.Web
{
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#if NET_2_0
	[Serializable]
#endif
	public class HttpException : ExternalException
	{
		int http_code = 500;

		public HttpException ()
		{
		}

		public HttpException (string message)
			: base (message)
		{
		}

		public HttpException (string message, Exception innerException)
			: base (message, innerException)
		{
		}

		public HttpException (int httpCode, string message) : base (message)
		{
			http_code = httpCode;
		}

#if NET_2_0
		protected HttpException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			http_code = info.GetInt32 ("_httpCode");
		}

		[SecurityPermission (SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("_httpCode", http_code);
		}
#endif

		public HttpException (int httpCode, string message, int hr) 
			: base (message, hr)
		{
			http_code = httpCode;
		}

		public HttpException (string message, int hr)
			: base (message, hr)
		{
		}
	
		public HttpException (int httpCode, string message, Exception innerException)
			: base (message, innerException)
		{
			http_code = httpCode;
		}

		public string GetHtmlErrorMessage ()
		{
			if (!(this.InnerException is HtmlizedException))
				return GetDefaultErrorMessage ();

			return GetHtmlizedErrorMessage ();
		}

		internal virtual string Description {
			get { return "Error processing request."; }
		}
		
		string GetDefaultErrorMessage ()
		{
			StringBuilder builder = new StringBuilder ("<html>\r\n<title>");
			builder.Append ("Error");
			if (http_code != 0)
				builder.Append (" " + http_code);

			builder.AppendFormat ("</title><body bgcolor=\"white\">" + 
					      "<h1><font color=\"red\">Server error in '{0}' " + 
					      "application</font></h1><hr>\r\n",
					      HtmlEncode (HttpRuntime.AppDomainAppVirtualPath));

			builder.AppendFormat ("<h2><font color=\"maroon\"><i>{0}</i></font></h2>\r\n",
					      HtmlEncode (Message));
			builder.AppendFormat ("<b>Description: </b>{0}\r\n<p>\r\n", Description);
			builder.Append ("<b>Error Message: </b>");
			if (http_code != 0)
				builder.AppendFormat ("HTTP {0}. ", http_code);

			builder.AppendFormat ("{0}\r\n<p>\r\n", HtmlEncode (this.Message));

			if (InnerException != null) {
				builder.AppendFormat ("<b>Stack Trace: </b>");
				builder.Append ("<table summary=\"Stack Trace\" width=\"100%\" " +
						"bgcolor=\"#ffffc\">\r\n<tr><td>");
				WriteTextAsCode (builder, InnerException.ToString ());
#if TARGET_J2EE //Required, because toString of Java doesn't print stackTrace
				WriteTextAsCode (builder, InnerException.StackTrace);
#endif
				builder.Append ("</td></tr>\r\n</table>\r\n<p>\r\n");
			}

			builder.AppendFormat ("<hr>\r\n{0}</body>\r\n</html>\r\n", DateTime.UtcNow);
			builder.AppendFormat ("<!--\r\n{0}\r\n-->\r\n", HttpUtility.HtmlEncode (this.ToString ()));
#if TARGET_J2EE //Required, because toString of Java doesn't print stackTrace
			builder.AppendFormat ("<!--\r\n{0}\r\n-->\r\n", HttpUtility.HtmlEncode (this.StackTrace));
#endif

			return builder.ToString ();
		}

		static string HtmlEncode (string s)
		{
			if (s == null)
				return s;

			string res = HttpUtility.HtmlEncode (s);
			return res.Replace ("\r\n", "<br />");
		}

		string GetHtmlizedErrorMessage ()
		{
			StringBuilder builder = new StringBuilder ("<html>\r\n<title>");
			HtmlizedException exc = (HtmlizedException) this.InnerException;
			builder.Append (exc.Title);
			builder.AppendFormat ("</title><body bgcolor=\"white\">" + 
					      "<h1><font color=\"red\">Server Error in '{0}' " + 
					      "Application</font></h1><hr>\r\n",
					      HttpRuntime.AppDomainAppVirtualPath);

			builder.AppendFormat ("<h2><font color=\"maroon\"><i>{0}</i></font></h2>\r\n", exc.Title);
			builder.AppendFormat ("<b>Description: </b>{0}\r\n<p>\r\n", HtmlEncode (exc.Description));
			string errorMessage = "<br>" + HtmlEncode (exc.ErrorMessage).Replace ("\n", "<br>");
			builder.AppendFormat ("<b>Error message: </b>{0}\r\n<p>\r\n", errorMessage);

			if (exc.FileName != null)
				builder.AppendFormat ("<b>File name: </b> {0}", HtmlEncode (exc.FileName));

			if (exc.FileText != null) {
				if (exc.SourceFile != exc.FileName)
					builder.AppendFormat ("<p><b>Source File: </b>{0}", exc.SourceFile);

				if (exc is ParseException) {
					builder.Append ("&nbsp;&nbsp;&nbsp;&nbsp;<b>Line: <b>");
					builder.Append (exc.ErrorLines [0]);
				}

				builder.Append ("\r\n<p>\r\n");

				if (exc is ParseException) {
					builder.Append ("<b>Source Error: </b>\r\n");
					builder.Append ("<table summary=\"Source error\" width=\"100%\"" +
							" bgcolor=\"#ffffc\">\r\n<tr><td>");
					WriteSource (builder, exc);
					builder.Append ("</td></tr>\r\n</table>\r\n<p>\r\n");
				} else {
					builder.Append ("<table summary=\"Source file\" width=\"100%\" " +
							"bgcolor=\"#ffffc\">\r\n<tr><td>");
					WriteSource (builder, exc);
					builder.Append ("</td></tr>\r\n</table>\r\n<p>\r\n");
				}
			}
			
			builder.Append ("<hr>\r\n</body>\r\n</html>\r\n");
			builder.AppendFormat ("<!--\r\n{0}\r\n-->\r\n", HtmlEncode (exc.ToString ()));
#if TARGET_JVM
			builder.AppendFormat ("<!--\r\n{0}\r\n-->\r\n", HtmlEncode (exc.StackTrace));
#endif
			return builder.ToString ();
		}

		static void WriteTextAsCode (StringBuilder builder, string text)
		{
			builder.Append ("<code><pre>\r\n");
			builder.AppendFormat ("{0}", HtmlEncode (text));
			builder.Append ("</pre></code>\r\n");
		}

#if TARGET_J2EE
		static void WriteSource (StringBuilder builder, HtmlizedException e)
		{
			builder.Append ("<code><pre>");
			WritePageSource (builder, e);
			builder.Append ("</pre></code>\r\n");
		}

#else
		static void WriteSource (StringBuilder builder, HtmlizedException e)
		{
			builder.Append ("<code><pre>");
			if (e is CompilationException)
				WriteCompilationSource (builder, e);
			else
				WritePageSource (builder, e);

			builder.Append ("</pre></code>\r\n");
		}
#endif
		
		static void WriteCompilationSource (StringBuilder builder, HtmlizedException e)
		{
			int [] a = e.ErrorLines;
			string s;
			int line = 0;
			int index = 0;
			int errline = 0;

			if (a != null && a.Length > 0)
				errline = a [0];
			
			TextReader reader = new StringReader (e.FileText);
			while ((s = reader.ReadLine ()) != null) {
				line++;

				if (errline == line)
					builder.Append ("<span style=\"color: red\">");

				builder.AppendFormat ("Line {0}: {1}\r\n", line, HtmlEncode (s));

				if (line == errline) {
					builder.Append ("</span>");
					errline = (++index < a.Length) ? a [index] : 0;
				}
			}
		}

		static void WritePageSource (StringBuilder builder, HtmlizedException e)
		{
			string s;
			int line = 0;
			int beginerror = e.ErrorLines [0];
			int enderror = e.ErrorLines [1];
			int begin = beginerror - 3;
			int end = enderror + 3;
			if (begin <= 0)
				begin = 1;
			
			TextReader reader = new StringReader (e.FileText);
			while ((s = reader.ReadLine ()) != null) {
				line++;
				if (line < begin)
					continue;

				if (line > end)
					break;

				if (beginerror == line)
					builder.Append ("<span style=\"color: red\">");

				builder.AppendFormat ("{0}\r\n", HtmlEncode (s));

				if (enderror <= line) {
					builder.Append ("</span>");
					enderror = end + 1; // one shot
				}
			}
		}
		
		public int GetHttpCode ()
		{
			return http_code;
		}

		public static HttpException CreateFromLastError (string message)
		{
			WebTrace.WriteLine ("CreateFromLastError");
			return new HttpException (message, 0);
		}
	}
}

