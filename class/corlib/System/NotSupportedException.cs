//
// System.NotSupportedException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

namespace System {

	public class NotSupportedException : SystemException {
		// Constructors
		public NotSupportedException ()
			: base ("Operation is not supported")
		{
		}

		public NotSupportedException (string message)
			: base (message)
		{
		}

		public NotSupportedException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}