//
// System.ValueType.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System {
	
	public abstract class ValueType {

		// <summary>
		//   ValueType constructor
		// </summary>
		protected ValueType ()
		{
		}

		// <summary>
		//   True if this instance and o represent the same type
		//   and have the same value.
		// </summary>
		[TODO]
		public override bool Equals (object o)
		{
			if (o == null)
				throw new ArgumentNullException ();

			if (o.GetType() != this.GetType())
				return false;

			// TODO:
			//   Now implement bit compare here.
			
			// TODO: Implement me!
			return false;
		}

		// <summary>
		//   Gets a hashcode for this value type using the
		//   bits in the structure
		// </summary>
		[TODO]
		public override int GetHashCode ()
		{
			if (this == null)
				return 0;

			// TODO: compute a hashcode based on the actual
			// contents.

			return 0;
		}

		// <summary>
		//   Stringified representation of this ValueType.
		//   Must be overriden for better results, by default
		//   it just returns the Type name.
		// </summary>
		public override string ToString ()
		{
			return GetType().FullName;
		}
	}
}
