//
// XmlNodeChangeType.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
#if NET_2_0

namespace System.Xml
{
	public enum XmlNodeChangeType
	{
		Updated,
		Inserted,
		Deleted,
		Unchanged
	}
}
#endif
