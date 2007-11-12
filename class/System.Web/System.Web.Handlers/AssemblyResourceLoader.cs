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
using System.Globalization;
using System.Reflection;
using System.IO;
using System.Resources;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;

namespace System.Web.Handlers {
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
	class AssemblyResourceLoader : IHttpHandler {		
		const string HandlerFileName = "WebResource.axd";
		static Assembly currAsm = typeof (AssemblyResourceLoader).Assembly;
#endif
		const char QueryParamSeparator = '&';

		static readonly Hashtable _embeddedResources = Hashtable.Synchronized (new Hashtable ());
#if SYSTEM_WEB_EXTENSIONS
		static ScriptResourceHandler () {
			MachineKeySectionUtils.AutoGenKeys ();
		}
#endif

		static void InitEmbeddedResourcesUrls (Assembly assembly, Hashtable hashtable)
		{
			WebResourceAttribute [] attrs = (WebResourceAttribute []) assembly.GetCustomAttributes (typeof (WebResourceAttribute), false);
			for (int i = 0; i < attrs.Length; i++) {
				string resourceName = attrs [i].WebResource;
				if (resourceName != null && resourceName.Length > 0) {
#if SYSTEM_WEB_EXTENSIONS
					hashtable.Add (new ResourceKey (resourceName, false), CreateResourceUrl (assembly, resourceName, false));
					hashtable.Add (new ResourceKey (resourceName, true), CreateResourceUrl (assembly, resourceName, true));
#else
					hashtable.Add (resourceName, CreateResourceUrl (assembly, resourceName, false));
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

		static string GetHexString (byte [] bytes)
		{
			const int letterPart = 55;
			const int numberPart = 48;
			char [] result = new char [bytes.Length * 2];
			for (int i = 0; i < bytes.Length; i++) {
				int tmp = (int) bytes [i];
				int second = tmp & 15;
				int first = (tmp >> 4) & 15;
				result [(i * 2)] = (char) (first > 9 ? letterPart + first : numberPart + first);
				result [(i * 2) + 1] = (char) (second > 9 ? letterPart + second : numberPart + second);
			}
			return new string (result);
		}
		
		static byte[] GetEncryptionKey ()
		{
#if NET_2_0
			return MachineKeySectionUtils.DecryptionKey192Bits ();
#else
			MachineKeyConfig config = HttpContext.GetAppConfig ("system.web/machineKey") as MachineKeyConfig;
			return config.DecryptionKey192Bits;
#endif
		}

		static byte[] GetBytes (string val)
		{
#if NET_2_0
			return MachineKeySectionUtils.GetBytes (val, val.Length);
#else
			return MachineKeyConfig.GetBytes (val, val.Length);
#endif
		}		
		
		static byte [] init_vector = { 0xD, 0xE, 0xA, 0xD, 0xB, 0xE, 0xE, 0xF };
		
		static string EncryptAssemblyResource (string asmName, string resName)
		{
			byte[] key = GetEncryptionKey ();
			byte[] bytes = Encoding.UTF8.GetBytes (String.Format ("{0};{1}", asmName, resName));
			string result;
			
			ICryptoTransform encryptor = TripleDES.Create ().CreateEncryptor (key, init_vector);
			result = GetHexString (encryptor.TransformFinalBlock (bytes, 0, bytes.Length));
			bytes = null;

			return String.Format ("d={0}", result.ToLower (CultureInfo.InvariantCulture));
		}

		static void DecryptAssemblyResource (string val, out string asmName, out string resName)
		{
			byte[] key = GetEncryptionKey ();
			byte[] bytes = GetBytes (val);
			byte[] result;

			asmName = null;
			resName = null;			

			ICryptoTransform decryptor = TripleDES.Create ().CreateDecryptor (key, init_vector);
			result = decryptor.TransformFinalBlock (bytes, 0, bytes.Length);
			bytes = null;

			string data = Encoding.UTF8.GetString (result);
			result = null;

			string[] parts = data.Split (';');
			if (parts.Length != 2)
				return;
			
			asmName = parts [0];
			resName = parts [1];
		}

		internal static string GetResourceUrl (Assembly assembly, string resourceName, bool notifyScriptLoaded)
		{
			Hashtable hashtable = (Hashtable)_embeddedResources [assembly];
			if (hashtable == null) {
				hashtable = new Hashtable ();
				InitEmbeddedResourcesUrls (assembly, hashtable);
				_embeddedResources [assembly] = hashtable;
			}
#if SYSTEM_WEB_EXTENSIONS
			string url = (string) hashtable [new ResourceKey (resourceName, notifyScriptLoaded)];
#else
			string url = (string) hashtable [resourceName];
#endif
			if (url == null)
				url = CreateResourceUrl (assembly, resourceName, notifyScriptLoaded);
			return url;
		}
		
		static string CreateResourceUrl (Assembly assembly, string resourceName, bool notifyScriptLoaded)
		{

			string aname = assembly == currAsm ? "s" : assembly.GetName ().FullName;
			string apath = assembly.Location;
			string atime = String.Empty;
			string extra = String.Empty;
#if SYSTEM_WEB_EXTENSIONS
			extra = String.Format ("{0}n={1}", QueryParamSeparator, notifyScriptLoaded ? "t" : "f");
#endif

#if TARGET_JVM
			atime = String.Format ("{0}t={1}", QueryParamSeparator, assembly.GetHashCode ());
#else
			if (apath != String.Empty)
				atime = String.Format ("{0}t={1}", QueryParamSeparator, File.GetLastWriteTimeUtc (apath).Ticks);
#endif
			string href = String.Format ("{0}?{1}{2}{3}", HandlerFileName,
						     EncryptAssemblyResource (aname, resourceName), atime, extra);

			HttpContext ctx = HttpContext.Current;
			if (ctx != null && ctx.Request != null) {
				string appPath = VirtualPathUtility.AppendTrailingSlash (ctx.Request.ApplicationPath);
				href = appPath + href;
			}
			
			return href;
		}

	
#if SYSTEM_WEB_EXTENSIONS
		protected virtual void ProcessRequest (HttpContext context)
#else
		[MonoTODO ("Substitution not implemented")]
		void System.Web.IHttpHandler.ProcessRequest (HttpContext context)
#endif
		{
			string resourceName;
			string asmName;
			Assembly assembly;

			DecryptAssemblyResource (context.Request.QueryString ["d"], out asmName, out resourceName);
			if (resourceName == null)
				throw new HttpException (404, "No resource name given");
			
			if (asmName == null || asmName == "s")
				assembly = currAsm;
			else
				assembly = Assembly.Load (asmName);
			
			WebResourceAttribute wra = null;
			WebResourceAttribute [] attrs = (WebResourceAttribute []) assembly.GetCustomAttributes (typeof (WebResourceAttribute), false);
			for (int i = 0; i < attrs.Length; i++) {
				if (attrs [i].WebResource == resourceName) {
					wra = attrs [i];
					break;
				}
			}
#if SYSTEM_WEB_EXTENSIONS
			if (wra == null && resourceName.Length > 9 && resourceName.EndsWith (".debug.js", StringComparison.OrdinalIgnoreCase)) {
				resourceName = String.Concat (resourceName.Substring (0, resourceName.Length - 9), ".js");
				for (int i = 0; i < attrs.Length; i++) {
					if (attrs [i].WebResource == resourceName) {
						wra = attrs [i];
						break;
					}
				}
			}
#endif
			if (wra == null)
				throw new HttpException (404, string.Format ("Resource {0} not found", resourceName));
			
			context.Response.ContentType = wra.ContentType;

			/* tell the client they can cache resources for 1 year */
			context.Response.ExpiresAbsolute = DateTime.Now.AddYears (1);
			context.Response.CacheControl = "private";

			Stream s = assembly.GetManifestResourceStream (resourceName);
			if (s == null)
				throw new HttpException (404, string.Format ("Resource {0} not found", resourceName));

			if (wra.PerformSubstitution) {
				StreamReader r = new StreamReader (s);
				TextWriter w = context.Response.Output;
				new PerformSubstitutionHelper (assembly).PerformSubstitution (r, w);
			}
			else {
				byte [] buf = new byte [1024];
				Stream output = context.Response.OutputStream;
				int c;
				do {
					c = s.Read (buf, 0, 1024);
					output.Write (buf, 0, c);
				} while (c > 0);
			}
#if SYSTEM_WEB_EXTENSIONS
			TextWriter writer = context.Response.Output;
			foreach (ScriptResourceAttribute sra in assembly.GetCustomAttributes (typeof (ScriptResourceAttribute), false)) {
				if (sra.ScriptName == resourceName) {
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
					writer.Write ("{0}={{", sra.TypeName);
					bool first = true;
					foreach (DictionaryEntry entry in rset) {
						string value = entry.Value as string;
						if (value != null) {
							if (first)
								first = false;
							else
								writer.Write (',');
							writer.WriteLine ();
							writer.Write ("{0}:{1}", GetScriptStringLiteral ((string) entry.Key), GetScriptStringLiteral (value));
						}
					}
					writer.WriteLine ();
					writer.WriteLine ("};");
					break;
				}
			}

			bool notifyScriptLoaded = context.Request.QueryString ["n"] == "t";
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
	}
}

