//
// XmlNamespaceScope.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// Note that this is different from "XPathNamespaceScope" 
// while the member definitions are all the same.
//
#if NET_2_0

namespace System.Xml
{
	public enum XmlNamespaceScope
	{
		All,
		ExcludeXml,
		Local 
	}
}
#endif
