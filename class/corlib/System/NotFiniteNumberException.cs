//
// System.NotFiniteNumberException.cs
//
// Author:
//   Joe Shaw (joe@ximian.com)
//   Duncan Mak (duncan@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//
using System.Globalization;
using System.Runtime.Serialization;

namespace System {

	[Serializable]
	public class NotFiniteNumberException : ArithmeticException {
		double offending_number;

		// Constructors
		public NotFiniteNumberException ()
			: base (Locale.GetText ("The number encountered was not a finite quantity"))
		{
		}

		public NotFiniteNumberException (double offending_number)
		{
			this.offending_number = offending_number;
		}

		public NotFiniteNumberException (string message)
			: base (message)
		{
		}

		public NotFiniteNumberException (string message, double offending_number)
		{
			this.offending_number = offending_number;
		}

		public NotFiniteNumberException (string message, double offending_number, Exception inner)
			: base (message, inner)
		{
			this.offending_number = offending_number;
		}

		protected NotFiniteNumberException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
			offending_number = info.GetDouble ("OffendingNumber");
		}		

		// Properties
		public double OffendingNumber {
			get {
				return offending_number;
			}
		}

		// Method

		public override void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData (info, context);
			info.AddValue ("OffendingNumber", offending_number);
		}
	}
}
