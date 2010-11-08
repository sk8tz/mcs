//
// System.Web.Handlers.AssemblyResourceLoader
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Marek Habersack <grendel@twistedcode.net>
//
// (C) 2003 Ben Maurer
// (C) 2010 Novell, Inc (http://novell.com/)

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
using System.Globalization;
using System.Reflection;
using System.IO;
using System.Resources;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Configuration;
using System.Web.Util;

namespace System.Web.Handlers
{
#if SYSTEM_WEB_EXTENSIONS
	partial class ScriptResourceHandler
	{
		const string HandlerFileName = "ScriptResource.axd";
		static Assembly currAsm = typeof (ScriptResourceHandler).Assembly;
#else
	#if NET_2_0
	public sealed
	#else
	internal // since this is in the .config file, we need to support it, since we dont have versoned support.
	#endif
	class AssemblyResourceLoader : IHttpHandler
	{
		const string HandlerFileName = "WebResource.axd";
		static Assembly currAsm = typeof (AssemblyResourceLoader).Assembly;
#endif
		const char QueryParamSeparator = '&';

		static readonly Dictionary <string, AssemblyEmbeddedResources> _embeddedResources = new Dictionary <string, AssemblyEmbeddedResources> (StringComparer.Ordinal);
		static readonly ReaderWriterLockSlim _embeddedResourcesLock = new ReaderWriterLockSlim ();

		[ThreadStatic]
		static Dictionary <string, string> stringHashCache;

		static Dictionary <string, string> StringHashCache {
			get {
				if (stringHashCache == null)
					stringHashCache = new Dictionary <string, string> (StringComparer.Ordinal);

				return stringHashCache;
			}
		}
		
		static string GetStringHash (KeyedHashAlgorithm kha, string str)
		{
			if (String.IsNullOrEmpty (str))
				return String.Empty;

			string result;
			Dictionary <string, string> cache = StringHashCache;
			if (cache.TryGetValue (str, out result))
				return result;
			
			result = Convert.ToBase64String (kha.ComputeHash (Encoding.UTF8.GetBytes (str)));
			cache.Add (str, result);
			return result;
		}
		
		static void InitEmbeddedResourcesUrls (KeyedHashAlgorithm kha, Assembly assembly, string assemblyName, string assemblyHash, AssemblyEmbeddedResources entry)
		{
			WebResourceAttribute [] attrs = (WebResourceAttribute []) assembly.GetCustomAttributes (typeof (WebResourceAttribute), false);
			WebResourceAttribute attr;
			string apath = assembly.Location;
			for (int i = 0; i < attrs.Length; i++) {
				attr = attrs [i];
				string resourceName = attr.WebResource;
				if (!String.IsNullOrEmpty (resourceName)) {
					string resourceNameHash = GetStringHash (kha, resourceName);
#if SYSTEM_WEB_EXTENSIONS
					bool debug = resourceName.EndsWith (".debug.js", StringComparison.OrdinalIgnoreCase);
					string dbgTail = debug ? "d" : String.Empty;
					string rkNoNotify = resourceNameHash + "f" + dbgTail;
					string rkNotify = resourceNameHash + "t" + dbgTail;

					if (!entry.Resources.ContainsKey (rkNoNotify)) {
						var er = new EmbeddedResource () {
							Name = resourceName,
							Attribute = attr, 
							Url = CreateResourceUrl (kha, assemblyName, assemblyHash, apath, rkNoNotify, debug, false)
						};
						
						entry.Resources.Add (rkNoNotify, er);
					}
					
					if (!entry.Resources.ContainsKey (rkNotify)) {
						var er = new EmbeddedResource () {
							Name = resourceName,
							Attribute = attr, 
							Url = CreateResourceUrl (kha, assemblyName, assemblyHash, apath, rkNotify, debug, true)
						};
						
						entry.Resources.Add (rkNotify, er);
					}
#else
					if (!entry.Resources.ContainsKey (resourceNameHash)) {
						var er = new EmbeddedResource () {
							Name = resourceName,
							Attribute = attr, 
							Url = CreateResourceUrl (kha, assemblyName, assemblyHash, apath, resourceNameHash, false, false)
						};
						entry.Resources.Add (resourceNameHash, er);
					}
#endif
				}
			}
		}

#if !SYSTEM_WEB_EXTENSIONS
		internal static string GetResourceUrl (Type type, string resourceName)
		{
			return GetResourceUrl (type.Assembly, resourceName, false);
		}
#endif
		static EmbeddedResource DecryptAssemblyResource (string val, out AssemblyEmbeddedResources entry)
		{
			entry = null;
			
			string[] parts = val.Split ('_');
			if (parts.Length != 3)
				return null;

			Encoding enc = Encoding.UTF8;
			string asmNameHash = parts [0];
			string resNameHash = parts [1];
			bool debug = parts [2] == "t";
			
			try {
				_embeddedResourcesLock.EnterReadLock ();
				if (!_embeddedResources.TryGetValue (asmNameHash, out entry) || entry == null)
					return null;
				
				EmbeddedResource res;
				if (!entry.Resources.TryGetValue (resNameHash, out res) || res == null) {
#if SYSTEM_WEB_EXTENSIONS
					if (!debug)
						return null;

					if (!entry.Resources.TryGetValue (resNameHash.Substring (0, resNameHash.Length - 1), out res))
						return null;
#else
					return null;
#endif
				}
				
				return res;
			} finally {
				_embeddedResourcesLock.ExitReadLock ();
			}
		}

		internal static string GetResourceUrl (Assembly assembly, string resourceName, bool notifyScriptLoaded)
		{
			if (assembly == null)
				return String.Empty;
			
			MachineKeySection mks = MachineKeySection.Config;
			using (KeyedHashAlgorithm kha = MachineKeySectionUtils.GetValidationAlgorithm (mks)) {
				kha.Key = MachineKeySectionUtils.GetValidationKey (mks);
				return GetResourceUrl (kha, assembly, resourceName, notifyScriptLoaded);
			}
		}

		static string GetResourceUrl (KeyedHashAlgorithm kha, Assembly assembly, string resourceName, bool notifyScriptLoaded)
		{
			string assemblyName = assembly == currAsm ? "s" : assembly.GetName ().FullName;
			string assemblyNameHash = GetStringHash (kha, assemblyName);
			string resourceNameHash = GetStringHash (kha, resourceName);
			bool debug = false;
			string url;
			AssemblyEmbeddedResources entry;

			try {
				_embeddedResourcesLock.EnterUpgradeableReadLock ();
				if (!_embeddedResources.TryGetValue (assemblyNameHash, out entry) || entry == null) {
					try {
						_embeddedResourcesLock.EnterWriteLock ();
						entry = new AssemblyEmbeddedResources () {
							AssemblyName = assemblyName
						};
						InitEmbeddedResourcesUrls (kha, assembly, assemblyName, assemblyNameHash, entry);
						_embeddedResources.Add (assemblyNameHash, entry);
					} finally {
						_embeddedResourcesLock.ExitWriteLock ();
					}
				}
				string lookupKey;
#if SYSTEM_WEB_EXTENSIONS
				debug = resourceName.EndsWith (".debug.js", StringComparison.OrdinalIgnoreCase);
				string dbgTail = debug ? "d" : String.Empty;
				lookupKey = resourceNameHash + (notifyScriptLoaded ? "t" : "f") + dbgTail;
#else
				lookupKey = resourceNameHash;
#endif
				EmbeddedResource res;
				if (entry.Resources.TryGetValue (lookupKey, out res) && res != null)
					url = res.Url;
				else {
#if SYSTEM_WEB_EXTENSIONS
					if (debug) {
						resourceNameHash = GetStringHash (kha, resourceName.Substring (0, resourceName.Length - 9) + ".js");
						lookupKey = resourceNameHash + (notifyScriptLoaded ? "t" : "f");
					
						if (entry.Resources.TryGetValue (lookupKey, out res) && res != null)
							url = res.Url;
						else
							url = null;
					} else
#endif
						url = null;
				}
			} finally {
				_embeddedResourcesLock.ExitUpgradeableReadLock ();
			}

			if (url == null)
				url = CreateResourceUrl (kha, assemblyName, assemblyNameHash, assembly.Location, resourceNameHash, debug, notifyScriptLoaded);
			
			return url;
		}
		
		static string CreateResourceUrl (KeyedHashAlgorithm kha, string assemblyName, string assemblyNameHash, string assemblyPath, string resourceNameHash, bool debug, bool notifyScriptLoaded)
		{
			string atime = String.Empty;
			string extra = String.Empty;
#if SYSTEM_WEB_EXTENSIONS
			extra = QueryParamSeparator + "n=" + (notifyScriptLoaded ? "t" : "f");
#endif

#if TARGET_JVM
			atime = QueryParamSeparator + "t=" + assemblyName.GetHashCode ();
#else
			if (!String.IsNullOrEmpty (assemblyPath) && File.Exists (assemblyPath))
				atime = QueryParamSeparator + "t=" + File.GetLastWriteTimeUtc (assemblyPath).Ticks;
			else
				atime = QueryParamSeparator + "t=" + DateTime.UtcNow.Ticks;
#endif
			string d = assemblyNameHash + "_" + resourceNameHash +  (debug ? "_t" : "_f");
			string href = HandlerFileName + "?d=" + d + atime + extra;
			HttpContext ctx = HttpContext.Current;
			HttpRequest req = ctx != null ? ctx.Request : null;
			if (req != null) {
				string appPath = VirtualPathUtility.AppendTrailingSlash (req.ApplicationPath);
				href = appPath + href;
			}
			
			return href;
		}

#if SYSTEM_WEB_EXTENSIONS
		protected virtual void ProcessRequest (HttpContext context)
#else
		void System.Web.IHttpHandler.ProcessRequest (HttpContext context)
#endif
		{
			HttpRequest request = context.Request;
			// val is URL-encoded, which means every + has been replaced with ' ', we
			// need to revert that or the base64 conversion will fail.
			string d = request.QueryString ["d"];
			if (!String.IsNullOrEmpty (d))
				d = d.Replace (' ', '+');

			AssemblyEmbeddedResources entry;
			EmbeddedResource res = DecryptAssemblyResource (d, out entry);
			WebResourceAttribute wra = res != null ? res.Attribute : null;
			if (wra == null)
				throw new HttpException (404, "Resource not found");
			
			Assembly assembly;
			if (entry.AssemblyName == "s")
				assembly = currAsm;
			else
				assembly = Assembly.Load (entry.AssemblyName);
			
			HttpResponse response = context.Response;
			string req_cache = request.Headers ["Cache-Control"];
			if (String.Compare (req_cache, "max-age=0", StringComparison.Ordinal) == 0) {
				long atime;
				if (Int64.TryParse (request.QueryString ["t"], out atime)) {
					if (atime == File.GetLastWriteTimeUtc (assembly.Location).Ticks) {
						response.Clear ();
						response.StatusCode = 304;
						response.ContentType = null;
						response.CacheControl = "public"; // easier to set it to public as MS than remove it
						context.ApplicationInstance.CompleteRequest ();
						return;
					}
				}
			}
			string modif_since = request.Headers ["If-Modified-Since"];
			if (!String.IsNullOrEmpty (modif_since)) {
				try {
					DateTime modif;
					if (DateTime.TryParseExact (modif_since, "r", null, 0, out modif)) {
						if (File.GetLastWriteTimeUtc (assembly.Location) <= modif) {
							response.Clear ();
							response.StatusCode = 304;
							response.ContentType = null;
							response.CacheControl = "public"; // easier to set it to public as MS than remove it
							context.ApplicationInstance.CompleteRequest ();
							return;
						}
					}
				} catch {}
			}

			response.ContentType = wra.ContentType;

			DateTime utcnow = DateTime.UtcNow;
			response.Headers.Add ("Last-Modified", utcnow.ToString ("r"));
			response.ExpiresAbsolute = utcnow.AddYears (1);
			response.CacheControl = "public";

			Stream s = assembly.GetManifestResourceStream (res.Name);
			if (s == null)
				throw new HttpException (404, "Resource " + res.Name + " not found");

			if (wra.PerformSubstitution) {
				using (StreamReader r = new StreamReader (s)) {
					TextWriter w = response.Output;
					new PerformSubstitutionHelper (assembly).PerformSubstitution (r, w);
				}
			} else if (response.OutputStream is HttpResponseStream) {
				UnmanagedMemoryStream st = (UnmanagedMemoryStream) s;
				HttpResponseStream hstream = (HttpResponseStream) response.OutputStream;
				unsafe {
					hstream.WritePtr (new IntPtr (st.PositionPointer), (int) st.Length);
				}
			} else {
				byte [] buf = new byte [1024];
				Stream output = response.OutputStream;
				int c;
				do {
					c = s.Read (buf, 0, 1024);
					output.Write (buf, 0, c);
				} while (c > 0);
			}
#if SYSTEM_WEB_EXTENSIONS
			TextWriter writer = response.Output;
			foreach (ScriptResourceAttribute sra in assembly.GetCustomAttributes (typeof (ScriptResourceAttribute), false)) {
				if (String.Compare (sra.ScriptName, res.Name, StringComparison.Ordinal) == 0) {
					string scriptResourceName = sra.ScriptResourceName;
					ResourceSet rset = null;
					try {
						rset = new ResourceManager (scriptResourceName, assembly).GetResourceSet (Threading.Thread.CurrentThread.CurrentUICulture, true, true);
					}
					catch (MissingManifestResourceException) {
#if TARGET_JVM // GetResourceSet does not throw  MissingManifestResourceException if ressource is not exists
					}
					if (rset == null) {
#endif
						if (scriptResourceName.EndsWith (".resources")) {
							scriptResourceName = scriptResourceName.Substring (0, scriptResourceName.Length - 10);
							rset = new ResourceManager (scriptResourceName, assembly).GetResourceSet (Threading.Thread.CurrentThread.CurrentUICulture, true, true);
						}
#if !TARGET_JVM
						else
							throw;
#endif
					}
					if (rset == null)
						break;
					writer.WriteLine ();
					string ns = sra.TypeName;
					int indx = ns.LastIndexOf ('.');
					if (indx > 0)
						writer.WriteLine ("Type.registerNamespace('" + ns.Substring (0, indx) + "')");
					writer.Write ("{0}={{", sra.TypeName);
					bool first = true;
					foreach (DictionaryEntry de in rset) {
						string value = de.Value as string;
						if (value != null) {
							if (first)
								first = false;
							else
								writer.Write (',');
							writer.WriteLine ();
							writer.Write ("{0}:{1}", GetScriptStringLiteral ((string) de.Key), GetScriptStringLiteral (value));
						}
					}
					writer.WriteLine ();
					writer.WriteLine ("};");
					break;
				}
			}

			bool notifyScriptLoaded = request.QueryString ["n"] == "t";
			if (notifyScriptLoaded) {
				writer.WriteLine ();
				writer.WriteLine ("if(typeof(Sys)!=='undefined')Sys.Application.notifyScriptLoaded();");
			}
#endif
		}

		sealed class PerformSubstitutionHelper
		{
			readonly Assembly _assembly;
			static readonly Regex _regex = new Regex (@"\<%=[ ]*WebResource[ ]*\([ ]*""([^""]+)""[ ]*\)[ ]*%\>");

			public PerformSubstitutionHelper (Assembly assembly) {
				_assembly = assembly;
			}

			public void PerformSubstitution (TextReader reader, TextWriter writer) {
				string line = reader.ReadLine ();
				while (line != null) {
					if (line.Length > 0 && _regex.IsMatch (line))
						line = _regex.Replace (line, new MatchEvaluator (PerformSubstitutionReplace));
					writer.WriteLine (line);
					line = reader.ReadLine ();
				}
			}

			string PerformSubstitutionReplace (Match m) {
				string resourceName = m.Groups [1].Value;
#if SYSTEM_WEB_EXTENSIONS
				return ScriptResourceHandler.GetResourceUrl (_assembly, resourceName, false);
#else
				return AssemblyResourceLoader.GetResourceUrl (_assembly, resourceName, false);
#endif
			}
		}
		
#if !SYSTEM_WEB_EXTENSIONS
		bool System.Web.IHttpHandler.IsReusable { get { return true; } }
#endif
		sealed class EmbeddedResource
		{
			public string Name;
			public string Url;
			public WebResourceAttribute Attribute;
		}
		
		sealed class AssemblyEmbeddedResources
		{
			public string AssemblyName = String.Empty;
			public Dictionary <string, EmbeddedResource> Resources = new Dictionary <string, EmbeddedResource> (StringComparer.Ordinal);
		}
	}
}

