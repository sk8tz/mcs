//
// System.Delegate.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
// TODO:  Mucho left to implement
//

using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;

namespace System {

	[MonoTODO]
	public abstract class Delegate : ICloneable, ISerializable {
		protected Type target_type;
		protected object m_target;
		protected string method_name;
		protected IntPtr method_ptr;
		protected MethodInfo method_info;

		protected Delegate (object target, string method)
		{
			if (target == null)
				throw new ArgumentNullException (Locale.GetText ("Target object is null"));

			if (method == null)
				throw new ArgumentNullException (Locale.GetText ("method name is null"));

			this.target_type = null;
			this.method_ptr = IntPtr.Zero;
			this.m_target = target;
			this.method_name = method;
		}

		protected Delegate (Type target_type, string method)
		{
			if (m_target == null)
				throw new ArgumentNullException (Locale.GetText ("Target type is null"));

			if (method == null)
				throw new ArgumentNullException (Locale.GetText ("method string is null"));

			this.target_type = target_type;
			this.method_ptr = IntPtr.Zero;
			this.m_target = null;
			this.method_name = method;
		}

		public MethodInfo Method {
			get {
				return method_info;
			}
		}

		public object Target {
			get {
				return m_target;
			}
		}

		//
		// Methods
		//

		public object DynamicInvoke( object[] args )
		{
			return DynamicInvokeImpl( args );
		}

		public virtual object DynamicInvokeImpl( object[] args )
		{
			return Method.Invoke( m_target, args );
		}

		public virtual object Clone()
		{
			return MemberwiseClone();
		}

		public override bool Equals (object o)
		{
			if ( o == null )
				return false;
			
			if ( o.GetType() != this.GetType() )
				return false;

			Delegate d = (Delegate) o;
			if ((d.target_type == target_type) &&
			    (d.m_target == m_target) &&
			    (d.method_name == method_name))
				return true;

			return false;
		}

		public override int GetHashCode ()
		{
			return method_name.GetHashCode ();
		}

		// This is from ISerializable
		[MonoTODO]
		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			// TODO: IMPLEMENT ME
		}

		public virtual Delegate[] GetInvocationList()
		{
			return new Delegate[] { this };
		}

		/// <symmary>
		///   Returns a new MulticastDelegate holding the
		///   concatenated invocation lists of MulticastDelegates a and b
		/// </symmary>
		public static Delegate Combine (Delegate a, Delegate b)
		{
			if (a == null){
				if (b == null)
					return null;
				return b;
			} else 
				if (b == null)
					return a;

			if (a.GetType () != b.GetType ())
				throw new ArgumentException (Locale.GetText ("Incompatible Delegate Types"));
			
			return a.CombineImpl (b);
		}

		/// <symmary>
		///   Returns a new MulticastDelegate holding the
		///   concatenated invocation lists of an Array of MulticastDelegates
		/// </symmary>
		public static Delegate Combine( Delegate[] delegates )
		{
			Delegate retval = null;

			foreach ( Delegate next in delegates ) {
				retval = Combine( retval, next );
			}

			return retval;
		}


		protected virtual Delegate CombineImpl (Delegate d)
		{
			throw new MulticastNotSupportedException ("");
		}
		
		[MonoTODO]
		public static Delegate Remove( Delegate source, Delegate value) {
			if ( source == null )
				return null;
				
			if ( value == null )
				return source;

			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual Delegate RemoveImpl(Delegate d)
		{
			throw new NotImplementedException();
		}

		public static bool operator ==( Delegate a, Delegate b )
		{
			if ( (object)a == null ) {
				if ((object)b == null)
					return true;
				return false;
			}
			return a.Equals( b );
		}

		public static bool operator !=( Delegate a, Delegate b )
		{
			return !(a == b);
		}
	}
}
