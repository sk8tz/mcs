//
// XmlInfoItemType.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
#if NET_2_0

namespace System.Xml
{
	public enum XmlInfoItemType
	{
		Element,
		Attribute,
		Namespace,
		Text,
		ProcessingInstruction,
		Comment,
		Document,
		AtomicValue
	}
}
#endif
