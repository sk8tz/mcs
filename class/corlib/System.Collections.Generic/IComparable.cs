//
// System.Collections.Generic.IComparable
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_2_0
using System;
using System.Runtime.InteropServices;

namespace System.Collections.Generic {
	[CLSCompliant(false)]
	[ComVisible(false)]
	public interface IComparable<T> {
		int CompareTo(T other);
	}
}
#endif
