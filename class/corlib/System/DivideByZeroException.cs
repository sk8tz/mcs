//
// System.DivideByZeroException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Globalization;
namespace System {

	[Serializable]
	public class DivideByZeroException : ArithmeticException {
		// Constructors
		public DivideByZeroException ()
			: base (Locale.GetText ("Division by zero"))
		{
		}

		public DivideByZeroException (string message)
			: base (message)
		{
		}

		public DivideByZeroException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}
