//
// System.Reflection/MonoMethod.cs
// The class used to represent methods from the mono runtime.
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection {
	internal struct MonoMethodInfo {
		internal Type parent;
		internal Type ret;
		internal string name;
		internal MethodAttributes attrs;
		internal MethodImplAttributes iattrs;

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern void get_method_info (RuntimeMethodHandle handle, out MonoMethodInfo info);
	};
	
	/*
	 * Note: most of this class needs to be duplicated for the contructor, since
	 * the .NET reflection class hierarchy is so broken.
	 */
	internal class MonoMethod : MethodInfo {
		internal RuntimeMethodHandle mhandle;
		
		
		public override MethodInfo GetBaseDefinition() {
			return this; /* FIXME */
		}
		public override Type ReturnType {
			get {
				MonoMethodInfo info;
				MonoMethodInfo.get_method_info (mhandle, out info);
				return info.ret;
			}
		}
		public override ICustomAttributeProvider ReturnTypeCustomAttributes { 
			get {return null;}
		}
		
		public override MethodImplAttributes GetMethodImplementationFlags() {
			MonoMethodInfo info;
			MonoMethodInfo.get_method_info (mhandle, out info);
			return info.iattrs;
		}

		public override ParameterInfo[] GetParameters() {
			return null;
		}
		
		public override Object Invoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture) {
			throw new NotImplementedException ();
		}

		public override RuntimeMethodHandle MethodHandle { 
			get {return mhandle;} 
		}
		public override MethodAttributes Attributes { 
			get {
				MonoMethodInfo info;
				MonoMethodInfo.get_method_info (mhandle, out info);
				return info.attrs;
			} 
		}
		
		public override Type ReflectedType {
			get {
				MonoMethodInfo info;
				MonoMethodInfo.get_method_info (mhandle, out info);
				return info.parent;
			}
		}
		public override Type DeclaringType {
			get {
				MonoMethodInfo info;
				MonoMethodInfo.get_method_info (mhandle, out info);
				return info.parent;
			}
		}
		public override string Name {
			get {
				MonoMethodInfo info;
				MonoMethodInfo.get_method_info (mhandle, out info);
				return info.name;
			}
		}
		
		public override bool IsDefined (Type attribute_type, bool inherit) {
			return false;
		}

		public override object[] GetCustomAttributes( bool inherit) {
			return null;
		}
		public override object[] GetCustomAttributes( Type attributeType, bool inherit) {
			return null;
		}
	}
	
	internal class MonoCMethod : ConstructorInfo {
		internal RuntimeMethodHandle mhandle;
		
		public override MethodImplAttributes GetMethodImplementationFlags() {
			MonoMethodInfo info;
			MonoMethodInfo.get_method_info (mhandle, out info);
			return info.iattrs;
		}

		public override ParameterInfo[] GetParameters() {
			return null;
		}
		
		public override Object Invoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture) {
			throw new NotImplementedException ();
		}

		public override Object Invoke(BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture) {
			throw new NotImplementedException ();
		}

		public override RuntimeMethodHandle MethodHandle { 
			get {return mhandle;} 
		}
		public override MethodAttributes Attributes { 
			get {
				MonoMethodInfo info;
				MonoMethodInfo.get_method_info (mhandle, out info);
				return info.attrs;
			} 
		}
		
		public override Type ReflectedType {
			get {
				MonoMethodInfo info;
				MonoMethodInfo.get_method_info (mhandle, out info);
				return info.parent;
			}
		}
		public override Type DeclaringType {
			get {
				MonoMethodInfo info;
				MonoMethodInfo.get_method_info (mhandle, out info);
				return info.parent;
			}
		}
		public override string Name {
			get {
				MonoMethodInfo info;
				MonoMethodInfo.get_method_info (mhandle, out info);
				return info.name;
			}
		}
		
		public override bool IsDefined (Type attribute_type, bool inherit) {
			return false;
		}

		public override object[] GetCustomAttributes( bool inherit) {
			return null;
		}
		public override object[] GetCustomAttributes( Type attributeType, bool inherit) {
			return null;
		}
	}
}
