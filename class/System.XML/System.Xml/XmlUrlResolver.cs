// System.Xml.XmlUrlResolver.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//	   Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C) Ximian, Inc.
//

using System.Net;
using System.IO;
using Mono.Xml.Native;

namespace System.Xml
{
	public class XmlUrlResolver : XmlResolver
	{
		// Field
		ICredentials credential;
		WebClient webClientInternal;
		WebClient webClient {
			get {
				if (webClientInternal == null)
					webClientInternal = new WebClient ();
				return webClientInternal;
			}
		}

		// Constructor
		public XmlUrlResolver ()
			: base ()
		{
		}

		// Properties		
		public override ICredentials Credentials
		{
			set { credential = value; }
		}
		
		// Methods
		[MonoTODO("Use Credentials; Uri must be absolute.")]
		public override object GetEntity (Uri absoluteUri, string role, Type ofObjectToReturn)
		{
			// (MS documentation says) parameter role isn't used yet.
			Stream s = null;
			using (s) {
				WebClient wc = new WebClient ();
//				wc.Credentials = credential;
				s = wc.OpenRead (absoluteUri.ToString ());
				if (s.GetType ().IsSubclassOf (ofObjectToReturn))
					return s;
				wc.Dispose ();
			}
			return null;
		}

		public override Uri ResolveUri (Uri baseUri, string relativeUri)
		{
			if (baseUri == null) {
				try {
					return new Uri (relativeUri);
				} catch (UriFormatException) {
					return new Uri (Path.Combine (Path.GetFullPath ("."), relativeUri));
				}
			} else
				return new Uri (baseUri, relativeUri);
		}
	}
}
