

//
// System.Reflection/MonoProperty.cs
// The class used to represent Properties from the mono runtime.
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
	
	internal struct MonoPropertyInfo {
		public Type parent;
		public String name;
		public MethodInfo get_method;
		public MethodInfo set_method;
		public PropertyAttributes attrs;
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern void get_property_info (MonoProperty prop, out MonoPropertyInfo info);
	}

	internal class MonoProperty : PropertyInfo {
		internal IntPtr klass;
		internal IntPtr prop;
		
		public override PropertyAttributes Attributes {
			get {
				MonoPropertyInfo info;
				MonoPropertyInfo.get_property_info (this, out info);
				return info.attrs;
			}
		}
		public override bool CanRead {
			get {
				MonoPropertyInfo info;
				MonoPropertyInfo.get_property_info (this, out info);
				return (info.get_method != null);
			}
		}
		public override bool CanWrite {
			get {
				MonoPropertyInfo info;
				MonoPropertyInfo.get_property_info (this, out info);
				return (info.set_method != null);
			}
		}

		[MonoTODO]
		public override Type PropertyType {
			get {
				MonoPropertyInfo info;
				MonoPropertyInfo.get_property_info (this, out info);
				
				if (info.get_method != null) {
					return info.get_method.ReturnType;
				} else {
					// FIXME: take the last param to set_method
					return null;
				}
			}
		}
		public override Type ReflectedType {
			get {
				MonoPropertyInfo info;
				MonoPropertyInfo.get_property_info (this, out info);
				return info.parent;
			}
		}
		public override Type DeclaringType {
			get {
				MonoPropertyInfo info;
				MonoPropertyInfo.get_property_info (this, out info);
				return info.parent;
			}
		}
		public override string Name {
			get {
				MonoPropertyInfo info;
				MonoPropertyInfo.get_property_info (this, out info);
				return info.name;
			}
		}

		[MonoTODO]
		public override MethodInfo[] GetAccessors( bool nonPublic) {
			// FIXME: check nonPublic
			MonoPropertyInfo info;
			int n = 0;
			MonoPropertyInfo.get_property_info (this, out info);
			if (info.set_method != null)
				n++;
			if (info.get_method != null)
				n++;
			MethodInfo[] res = new MethodInfo [n];
			n = 0;
			if (info.set_method != null)
				res [n++] = info.set_method;
			if (info.get_method != null)
				res [n++] = info.get_method;
			return res;
		}

		[MonoTODO]
		public override MethodInfo GetGetMethod( bool nonPublic) {
			// FIXME: check nonPublic
			MonoPropertyInfo info;
			MonoPropertyInfo.get_property_info (this, out info);
			return info.get_method;
		}
		public override ParameterInfo[] GetIndexParameters() {
			MonoPropertyInfo info;
			MonoPropertyInfo.get_property_info (this, out info);
			if (info.get_method != null)
				return info.get_method.GetParameters ();
			return new ParameterInfo [0];
		}
		public override MethodInfo GetSetMethod( bool nonPublic) {
			// FIXME: check nonPublic
			MonoPropertyInfo info;
			MonoPropertyInfo.get_property_info (this, out info);
			return info.set_method;
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
		public override object GetValue( object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture) {
			return null;
		}
		public override void SetValue( object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture) {
		}
	}
}

