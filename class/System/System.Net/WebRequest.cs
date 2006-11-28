//
// System.Net.WebRequest
//
// Authors:
//   Lawrence Pit (loz@cable.a2000.nl)
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

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization;
#if NET_2_0
using System.Net.Configuration;
#endif

namespace System.Net 
{
	[Serializable]
	public abstract class WebRequest : MarshalByRefObject, ISerializable
	{
		static HybridDictionary prefixes = new HybridDictionary ();
		
		// Constructors
		
		static WebRequest ()
		{
#if NET_2_0 && CONFIGURATION_DEP
			object cfg = ConfigurationManager.GetSection ("system.net/webRequestModules");
			WebRequestModulesSection s = cfg as WebRequestModulesSection;
			if (s != null) {
				foreach (WebRequestModuleElement el in
					 s.WebRequestModules)
					AddPrefix (el.Prefix, el.Type);
				return;
			}
#endif
			ConfigurationSettings.GetConfig ("system.net/webRequestModules");
		}
		
		protected WebRequest () 
		{
		}
		
		protected WebRequest (SerializationInfo serializationInfo, StreamingContext streamingContext) 
		{
		}

		static Exception GetMustImplement ()
		{
			return new NotImplementedException ("This method must be implemented in derived classes");
		}
		
		// Properties
		
		public virtual string ConnectionGroupName { 
			get { throw GetMustImplement (); }
			set { throw GetMustImplement (); }
		}
		
		public virtual long ContentLength { 
			get { throw GetMustImplement (); }
			set { throw GetMustImplement (); }
		}
		
		public virtual string ContentType { 
			get { throw GetMustImplement (); }
			set { throw GetMustImplement (); }
		}
		
		public virtual ICredentials Credentials { 
			get { throw GetMustImplement (); }
			set { throw GetMustImplement (); }
		}
		
		public virtual WebHeaderCollection Headers { 
			get { throw GetMustImplement (); }
			set { throw GetMustImplement (); }
		}
		
		public virtual string Method { 
			get { throw GetMustImplement (); }
			set { throw GetMustImplement (); }
		}
		
		public virtual bool PreAuthenticate { 
			get { throw GetMustImplement (); }
			set { throw GetMustImplement (); }
		}
		
		public virtual IWebProxy Proxy { 
			get { throw GetMustImplement (); }
			set { throw GetMustImplement (); }
		}
		
		public virtual Uri RequestUri { 
			get { throw GetMustImplement (); }
		}
		
		public virtual int Timeout { 
			get { throw GetMustImplement (); }
			set { throw GetMustImplement (); }
		}
		
#if NET_2_0
		volatile static IWebProxy proxy;
		static readonly object lockobj = new object ();
		
		public static IWebProxy DefaultWebProxy {
			get {
				lock (lockobj) {
					if (proxy == null)
						proxy = GetDefaultWebProxy ();
					return proxy;
				}
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("WebRequest.DefaultWebProxy",
							"null IWebProxy not allowed.");
				proxy = value;
			}
		}
		
		[MonoTODO("Needs to respect Module, Proxy.AutoDetect, and Proxy.ScriptLocation config settings")]
		static IWebProxy GetDefaultWebProxy ()
		{
			WebProxy p = null;
			
#if CONFIGURATION_DEP
			DefaultProxySection sec = ConfigurationManager.GetSection ("system.net/defaultProxy") as DefaultProxySection;
			if (sec == null)
				return GetSystemWebProxy ();
			
			ProxyElement pe = sec.Proxy;
			
			if ((pe.UseSystemDefault != ProxyElement.UseSystemDefaultValues.False) && (pe.ProxyAddress == null))
				p = (WebProxy) GetSystemWebProxy ();
			else
				p = new WebProxy ();
			
			if (pe.ProxyAddress != null)
				p.Address = pe.ProxyAddress;
			
			if (pe.BypassOnLocal != ProxyElement.BypassOnLocalValues.Unspecified)
				p.BypassProxyOnLocal = (pe.BypassOnLocal == ProxyElement.BypassOnLocalValues.True);
#endif
			return p;
		}		
#endif

		// Methods
		
		public virtual void Abort()
		{
			throw GetMustImplement ();
		}
		
		public virtual IAsyncResult BeginGetRequestStream (AsyncCallback callback, object state) 
		{
			throw GetMustImplement ();
		}
		
		public virtual IAsyncResult BeginGetResponse (AsyncCallback callback, object state)
		{
			throw GetMustImplement ();
		}

		public static WebRequest Create (string requestUriString) 
		{
			if (requestUriString == null)
				throw new ArgumentNullException ("requestUriString");
			return Create (new Uri (requestUriString));
		}
				
		public static WebRequest Create (Uri requestUri) 
		{
			if (requestUri == null)
				throw new ArgumentNullException ("requestUri");
			return GetCreator (requestUri.AbsoluteUri).Create (requestUri);
		}
		
		public static WebRequest CreateDefault (Uri requestUri)
		{
			if (requestUri == null)
				throw new ArgumentNullException ("requestUri");
			return GetCreator (requestUri.Scheme).Create (requestUri);
		}

		public virtual Stream EndGetRequestStream (IAsyncResult asyncResult)
		{
			throw GetMustImplement ();
		}
		
		public virtual WebResponse EndGetResponse (IAsyncResult asyncResult)
		{
			throw GetMustImplement ();
		}
		
		public virtual Stream GetRequestStream()
		{
			throw GetMustImplement ();
		}
		
		public virtual WebResponse GetResponse()
		{
			throw GetMustImplement ();
		}
		
#if NET_2_0
		[MonoTODO("Look in other places for proxy config info")]
		public static IWebProxy GetSystemWebProxy ()
		{
			string address = Environment.GetEnvironmentVariable ("http_proxy");
			if (address != null) {
				try {
					WebProxy p = new WebProxy (address);
					return p;
				} catch (UriFormatException) {}
			}
			return new WebProxy ();
		}
#endif

		void ISerializable.GetObjectData (SerializationInfo serializationInfo,
		   				  StreamingContext streamingContext)
		{
			throw new NotSupportedException ();
		}

		public static bool RegisterPrefix (string prefix, IWebRequestCreate creator)
		{
			if (prefix == null)
				throw new ArgumentNullException("prefix");
			if (creator == null)
				throw new ArgumentNullException("creator");			
			
			lock (prefixes.SyncRoot) {
				string lowerCasePrefix = prefix.ToLower ();
				if (prefixes.Contains (lowerCasePrefix))
					return false;
				prefixes.Add (lowerCasePrefix, creator);
			}
			return true;
		}
		
		private static IWebRequestCreate GetCreator (string prefix)
		{
			int longestPrefix = -1;
			IWebRequestCreate creator = null;

			prefix = prefix.ToLower ();

			IDictionaryEnumerator e = prefixes.GetEnumerator ();
			while (e.MoveNext ()) {
				string key = e.Key as string;

				if (key.Length <= longestPrefix) 
					continue;
				
				if (!prefix.StartsWith (key))
					continue;					
					
				longestPrefix = key.Length;
				creator = (IWebRequestCreate) e.Value;
			}
			
			if (creator == null) 
				throw new NotSupportedException (prefix);
				
			return creator;
		}

		internal static void ClearPrefixes ()
		{
			prefixes.Clear ();
		}

		internal static void RemovePrefix (string prefix)
		{
			prefixes.Remove (prefix);
		}

		internal static void AddPrefix (string prefix, string typeName)
		{
			Type type = Type.GetType (typeName);
			if (type == null)
				throw new ConfigurationException (String.Format ("Type {0} not found", typeName));
			AddPrefix (prefix, type);
		}

		internal static void AddPrefix (string prefix, Type type)
		{
			object o = Activator.CreateInstance (type, true);
			prefixes [prefix] = o;
		}
	}
}

