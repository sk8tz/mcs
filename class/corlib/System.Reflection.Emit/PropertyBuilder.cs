
//
// System.Reflection.Emit/PropertyBuilder.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace System.Reflection.Emit {
	public sealed class PropertyBuilder : PropertyInfo {
		private PropertyAttributes attrs;
		private string name;
		private Type type;
		private Type[] parameters;
		private object def_value;
		private MethodBuilder set_method;
		private MethodBuilder get_method;
		private int table_idx = 0;
		internal TypeBuilder typeb;
		
		internal PropertyBuilder (TypeBuilder tb, string name, PropertyAttributes attributes, Type returnType, Type[] parameterTypes) {
			this.name = name;
			this.attrs = attributes;
			this.type = returnType;
			if (parameterTypes != null) {
				this.parameters = new Type [parameterTypes.Length];
				System.Array.Copy (parameterTypes, this.parameters, this.parameters.Length);
			}
			typeb = tb;
			table_idx = tb.module.assemblyb.get_next_table_index (0x17, true);
		}

		public override PropertyAttributes Attributes {
			get {return attrs;}
		}
		public override bool CanRead {
			get {return get_method != null;}
		}
		public override bool CanWrite {
			get {return set_method != null;}
		}
		public override Type DeclaringType {
			get {return null;}
		}
		public override string Name {
			get {return name;}
		}
		public PropertyToken PropertyToken {
			get {return new PropertyToken ();}
		}
		public override Type PropertyType {
			get {return type;}
		}
		public override Type ReflectedType {
			get {return null;}
		}
		public void AddOtherMethod( MethodBuilder mdBuilder) {
		}
		public override MethodInfo[] GetAccessors( bool nonPublic) {
			return null;
		}
		public override object[] GetCustomAttributes(bool inherit) {
			return null;
		}
		public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
			return null;
		}
		public override MethodInfo GetGetMethod( bool nonPublic) {
			return get_method;
		}
		public override ParameterInfo[] GetIndexParameters() {
			return null;
		}
		public override MethodInfo GetSetMethod( bool nonPublic) {
			return set_method;
		}
		public override object GetValue(object obj, object[] index) {
			return null;
		}
		public override object GetValue( object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture) {
			return null;
		}
		public override bool IsDefined( Type attributeType, bool inherit) {
			return false;
		}
		public void SetConstant( object defaultValue) {
			def_value = defaultValue;
		}
		public void SetCustomAttribute( CustomAttributeBuilder customBuilder) {
		}
		public void SetCustomAttribute( ConstructorInfo con, byte[] binaryAttribute) {
		}
		public void SetGetMethod( MethodBuilder mdBuilder) {
			get_method = mdBuilder;
		}
		public void SetSetMethod( MethodBuilder mdBuilder) {
			set_method = mdBuilder;
		}
		public override void SetValue( object obj, object value, object[] index) {
		}
		public override void SetValue( object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture) {
		}
	}
}

