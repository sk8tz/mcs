//
// System.NullReferenceException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;

namespace System {

	public class NullReferenceException : SystemException {
		// Constructors
		public NullReferenceException ()
			: base (Locale.GetText ("A null value was found where an object instance was required"))
		{
		}

		public NullReferenceException (string message)
			: base (message)
		{
		}

		public NullReferenceException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}
