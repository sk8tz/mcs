//
// System.TypedReference.cs
//
// Authors:
//   Dick Porter (dick@ximian.com)
//   Paolo Molaro (lupus@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Reflection;
using System.Runtime.CompilerServices;

namespace System 
{
	[CLSCompliant (false)]
	public struct TypedReference 
	{
		RuntimeTypeHandle type;
		IntPtr value;
		IntPtr klass;

		public override bool Equals (object o)
		{
			throw new NotSupportedException (Locale.GetText ("This operation is not supported for this type."));
		}

		public override int GetHashCode ()
		{
			if (type.Value == IntPtr.Zero)
				return 0;
			return Type.GetTypeFromHandle (type).GetHashCode ();
		}

		public static Type GetTargetType (TypedReference value)
		{
			return Type.GetTypeFromHandle (value.type);
		}

		[MonoTODO]
		[CLSCompliant (false)]
		public static TypedReference MakeTypedReference (object target, FieldInfo[] flds) 
		{
			if (target == null) {
				throw new ArgumentNullException ("target");
			}
			if (flds == null) {
				throw new ArgumentNullException ("flds");
			}
			if (flds.Length == 0) {
				throw new ArgumentException (Locale.GetText ("flds has no elements"));
			}
			throw new NotImplementedException ();
		}

		/* how can we set something in value if it's passed by value? */
		[MonoTODO]
		[CLSCompliant (false)]
		public static void SetTypedReference (TypedReference target, object value) 
		{
			if (value == null) {
				throw new ArgumentNullException ("value");
			}
			throw new NotImplementedException ();
		}

		public static RuntimeTypeHandle TargetTypeToken (TypedReference value)
		{
			return value.type;
		}

		[MethodImpl (MethodImplOptions.InternalCall)]
		public extern static object ToObject (TypedReference value);
	}
}
