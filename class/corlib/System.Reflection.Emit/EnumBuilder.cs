
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

//
// System.Reflection.Emit/EnumBuilder.cs
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
	public sealed class EnumBuilder : Type {
		private CustomAttributeBuilder[] cattrs;
		private TypeBuilder _tb;
		private FieldBuilder _underlyingField;
		private Type _underlyingType;

		internal EnumBuilder (ModuleBuilder mb, string name, TypeAttributes visibility, Type underlyingType)
		{
			_tb = new TypeBuilder (mb, name, (visibility | TypeAttributes.Sealed), 
				typeof(Enum), null, PackingSize.Unspecified, 0, null);
			_underlyingType = underlyingType;
			_underlyingField = _tb.DefineField ("value__", underlyingType,
				(FieldAttributes.SpecialName | FieldAttributes.Private));
			setup_enum_type (_tb);
		}

		public override Assembly Assembly {
			get {
				return _tb.Assembly;
			}
		}

		public override string AssemblyQualifiedName {
			get {
				return _tb.AssemblyQualifiedName;
			}
		}

		public override Type BaseType {
			get {
				return _tb.BaseType;
			}
		}

		public override Type DeclaringType {
			get {
				return _tb.DeclaringType;
			}
		}

		public override string FullName {
			get {
				return _tb.FullName;
			}
		}

		public override Guid GUID {
			get {
				return _tb.GUID;
			}
		}

		public override Module Module {
			get {
				return _tb.Module;
			}
		}

		public override string Name {
			get {
				return _tb.Name;
			}
		}

		public override string Namespace {
			get { 
				return _tb.Namespace;
			}
		}

		public override Type ReflectedType {
			get {
				return _tb.ReflectedType;
			}
		}

		public override RuntimeTypeHandle TypeHandle {
			get {
				return _tb.TypeHandle;
			}
		}

		public TypeToken TypeToken {
			get {
				return _tb.TypeToken;
			}
		}

		public FieldBuilder UnderlyingField {
			get {
				return _underlyingField;
			}
		}

		public override Type UnderlyingSystemType {
			get {
				return _underlyingType;
			}
		}

		public Type CreateType ()
		{
			Type res = _tb.CreateType ();
			return res;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void setup_enum_type (Type t);

		public FieldBuilder DefineLiteral (string literalName, object literalValue)
		{
			FieldBuilder fieldBuilder = _tb.DefineField (literalName, 
				_underlyingType, (FieldAttributes.Literal | 
				(FieldAttributes.Static | FieldAttributes.Public)));
			fieldBuilder.SetConstant (literalValue);
			return fieldBuilder;
		}

		protected override TypeAttributes GetAttributeFlagsImpl ()
		{
			return _tb.attrs;
		}

		protected override ConstructorInfo GetConstructorImpl (
			BindingFlags bindingAttr, Binder binder, CallingConventions cc,
			Type[] types, ParameterModifier[] modifiers)
		{
			return _tb.GetConstructor (bindingAttr, binder, cc, types, 
				modifiers);
		}

		public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
		{
			return _tb.GetConstructors (bindingAttr);
		}

		public override object[] GetCustomAttributes(bool inherit)
		{
			return _tb.GetCustomAttributes (inherit);
		}

		public override object[] GetCustomAttributes(Type attributeType, bool inherit)
		{
			return _tb.GetCustomAttributes (attributeType, inherit);
		}

		public override Type GetElementType()
		{
			return _tb.GetElementType ();
		}

		public override EventInfo GetEvent( string name, BindingFlags bindingAttr)
		{
			return _tb.GetEvent (name, bindingAttr);
		}

		public override EventInfo[] GetEvents()
		{
			return _tb.GetEvents ();
		}

		public override EventInfo[] GetEvents( BindingFlags bindingAttr)
		{
			return _tb.GetEvents (bindingAttr);
		}

		public override FieldInfo GetField( string name, BindingFlags bindingAttr)
		{
			return _tb.GetField (name, bindingAttr);
		}

		public override FieldInfo[] GetFields( BindingFlags bindingAttr)
		{
			return _tb.GetFields (bindingAttr);
		}

		public override Type GetInterface (string name, bool ignoreCase)
		{
			return _tb.GetInterface (name, ignoreCase);
		}

		public override InterfaceMapping GetInterfaceMap (Type interfaceType)
		{
			return _tb.GetInterfaceMap (interfaceType);
		}

		public override Type[] GetInterfaces()
		{
			return _tb.GetInterfaces ();
		}

		public override MemberInfo[] GetMember (string name, MemberTypes type, BindingFlags bindingAttr)
		{
			return _tb.GetMember (name, type, bindingAttr);
		}

		public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
		{
			return _tb.GetMembers (bindingAttr);
		}

		protected override MethodInfo GetMethodImpl (
			string name, BindingFlags bindingAttr, Binder binder,
			CallingConventions callConvention, Type[] types,
			ParameterModifier[] modifiers)
		{
			if (types == null) {
				return _tb.GetMethod (name, bindingAttr);
			}

			return _tb.GetMethod (name, bindingAttr, binder, 
				callConvention, types, modifiers);
		}

		public override MethodInfo[] GetMethods (BindingFlags bindingAttr)
		{
			return _tb.GetMethods (bindingAttr);
		}

		public override Type GetNestedType (string name, BindingFlags bindingAttr)
		{
			return _tb.GetNestedType (name, bindingAttr);
		}

		public override Type[] GetNestedTypes (BindingFlags bindingAttr)
		{
			return _tb.GetNestedTypes (bindingAttr);
		}

		public override PropertyInfo[] GetProperties (BindingFlags bindingAttr)
		{
			return _tb.GetProperties (bindingAttr);
		}

		protected override PropertyInfo GetPropertyImpl (
			string name, BindingFlags bindingAttr, Binder binder,
			Type returnType, Type[] types,
			ParameterModifier[] modifiers)
		{
			throw CreateNotSupportedException ();
		}

		protected override bool HasElementTypeImpl ()
		{
			return _tb.HasElementType;
		}

		public override object InvokeMember (
			string name, BindingFlags invokeAttr, Binder binder,
			object target, object[] args,
			ParameterModifier[] modifiers, CultureInfo culture,
			string[] namedParameters)
		{
			return _tb.InvokeMember (name, invokeAttr, binder, target, 
				args, modifiers, culture, namedParameters);
		}

		protected override bool IsArrayImpl()
		{
			return false;
		}

		protected override bool IsByRefImpl()
		{
			return false;
		}

		protected override bool IsCOMObjectImpl()
		{
			return false;
		}

		protected override bool IsPointerImpl()
		{
			return false;
		}

		protected override bool IsPrimitiveImpl()
		{
			return false;
		}

		protected override bool IsValueTypeImpl()
		{
			return true;
		}

		public override bool IsDefined (Type attributeType, bool inherit)
		{
			return _tb.IsDefined (attributeType, inherit);
		}

		public void SetCustomAttribute (CustomAttributeBuilder customBuilder)
		{
			if (cattrs != null) {
				CustomAttributeBuilder[] new_array = new CustomAttributeBuilder [cattrs.Length + 1];
				cattrs.CopyTo (new_array, 0);
				new_array [cattrs.Length] = customBuilder;
				cattrs = new_array;
			} else {
				cattrs = new CustomAttributeBuilder [1];
				cattrs [0] = customBuilder;
			}
		}

		public void SetCustomAttribute (ConstructorInfo con, byte[] binaryAttribute)
		{
			SetCustomAttribute (new CustomAttributeBuilder (con, binaryAttribute));
		}

#if NET_2_0 || BOOTSTRAP_NET_2_0
		[MonoTODO]
		public override Type[] GetGenericArguments ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool HasGenericArguments {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override bool ContainsGenericParameters {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override bool IsGenericParameter {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override int GenericParameterPosition {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override MethodInfo DeclaringMethod {
			get {
				throw new NotImplementedException ();
			}
		}
#endif

		private Exception CreateNotSupportedException ()
		{
			return new NotSupportedException ("The invoked member is not supported in a dynamic module.");
		}
	}
}
