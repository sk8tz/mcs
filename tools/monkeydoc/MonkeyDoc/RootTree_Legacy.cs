using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Diagnostics;
using System.Collections.Generic;

using Mono.Utilities;
using Lucene.Net.Index;

#if LEGACY_MODE

namespace MonkeyDoc
{
	using Generators;

	public partial class RootTree
	{
		static IDocGenerator<string> rawGenerator = new RawGenerator ();
		static HtmlGenerator htmlGenerator = new HtmlGenerator (null);

		[Obsolete ("Use RawGenerator directly")]
		public XmlDocument GetHelpXml (string id)
		{
			var doc = new XmlDocument ();
			doc.LoadXml (RenderUrl (id, rawGenerator));
			return doc;
		}

		[Obsolete ("Use the RenderUrl variant accepting a generator")]
		public string RenderUrl (string url, out Node n)
		{
			return RenderUrl (url, htmlGenerator, out n);
		}
	}
}

#endif
