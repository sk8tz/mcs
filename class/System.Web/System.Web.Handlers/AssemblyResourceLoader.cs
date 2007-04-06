//
// System.Web.Handlers.AssemblyResourceLoader
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

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

using System.Web.UI;
using System.Reflection;
using System.IO;

namespace System.Web.Handlers {
	#if NET_2_0
	public sealed
	#else
	internal // since this is in the .config file, we need to support it, since we dont have versoned support.
	#endif
	class AssemblyResourceLoader : IHttpHandler {		
		internal static string GetResourceUrl (Type type, string resourceName)
		{
			string aname = type.Assembly == typeof(AssemblyResourceLoader).Assembly ? "s" : HttpUtility.UrlEncode (type.Assembly.GetName ().FullName);
			string apath = type.Assembly.Location;
			string atime = String.Empty;

			if (apath != String.Empty)
				atime = String.Format ("{0}t={1}", HttpUtility.QueryParamSeparator, File.GetLastWriteTimeUtc (apath).Ticks);

			string href = String.Format ("WebResource.axd?a={1}{0}r={2}{3}",
						     HttpUtility.QueryParamSeparator, aname,
						     HttpUtility.UrlEncode (resourceName), atime);
			
			if (HttpContext.Current != null && HttpContext.Current.Request != null) {
				string appPath = HttpContext.Current.Request.ApplicationPath;
				if (!appPath.EndsWith ("/"))
					appPath += "/";

				href = appPath + href;
			}
			
			return href;
		}

	
		[MonoTODO ("Substitution not implemented")]
		void System.Web.IHttpHandler.ProcessRequest (HttpContext context)
		{
			string resourceName = context.Request.QueryString ["r"];
			string asmName = context.Request.QueryString ["a"];
			Assembly assembly;
			
			if (asmName == null || asmName == "s") assembly = GetType().Assembly;
			else assembly = Assembly.Load (asmName);
			
			bool found = false;
			foreach (WebResourceAttribute wra in assembly.GetCustomAttributes (typeof (WebResourceAttribute), false)) {
				if (wra.WebResource == resourceName) {
					context.Response.ContentType = wra.ContentType;

					/* tell the client they can cache resources for 1 year */
					context.Response.ExpiresAbsolute = DateTime.Now.AddYears(1); 
					context.Response.CacheControl = "public";
					context.Response.Cache.VaryByParams ["r"] = true;
					context.Response.Cache.VaryByParams ["t"] = true;

					if (wra.PerformSubstitution)
						throw new NotImplementedException ("Substitution not implemented");
					
					found = true;
					break;
				}
			}
			if (!found)
				return;
			
			Stream s = assembly.GetManifestResourceStream (resourceName);
			if (s == null)
				return;
			
			byte [] buf = new byte [1024];
			Stream output = context.Response.OutputStream;
			int c;
			do {
				c = s.Read (buf, 0, 1024);
				output.Write (buf, 0, c);
			} while (c > 0);
		}
		
		bool System.Web.IHttpHandler.IsReusable { get { return true; } }
	}
}

