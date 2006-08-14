//
// System.MonoType
//
// Authors: 
// 	Sean MacIsaac (macisaac@ximian.com)
// 	Paolo Molaro (lupus@ximian.com)
// 	Patrik Torstensson (patrik.torstensson@labs2.com)
//	Gonzalo Paniagua (gonzalo@ximian.com)
//
// (c) 2001-2003 Ximian, Inc.
// Copyright (C) 2003-2005 Novell, Inc (http://www.novell.com)
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

using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security;

namespace System
{
	[Serializable]
	internal class MonoType : Type, ISerializable
	{

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern void type_from_obj (MonoType type, Object obj);
		
		[MonoTODO]
		internal MonoType (Object obj)
		{
			// this should not be used - lupus
			type_from_obj (this, obj);
			
			throw new NotImplementedException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern TypeAttributes get_attributes (Type type);
	
		protected override TypeAttributes GetAttributeFlagsImpl ()
		{
			return get_attributes (this);
		}

		protected override ConstructorInfo GetConstructorImpl (BindingFlags bindingAttr,
								       Binder binder,
								       CallingConventions callConvention,
								       Type[] types,
								       ParameterModifier[] modifiers)
		{
			if (bindingAttr == BindingFlags.Default)
				bindingAttr = BindingFlags.Public | BindingFlags.Instance;

			ConstructorInfo[] methods = GetConstructors (bindingAttr);
			ConstructorInfo found = null;
			MethodBase[] match;
			int count = 0;
			foreach (ConstructorInfo m in methods) {
				// Under MS.NET, Standard|HasThis matches Standard...
				if (callConvention != CallingConventions.Any && ((m.CallingConvention & callConvention) != callConvention))
					continue;
				found = m;
				count++;
			}
			if (count == 0)
				return null;
			if (types == null) {
				if (count > 1)
					throw new AmbiguousMatchException ();
				return (ConstructorInfo) CheckMethodSecurity (found);
			}
			match = new MethodBase [count];
			if (count == 1)
				match [0] = found;
			else {
				count = 0;
				foreach (ConstructorInfo m in methods) {
					if (callConvention != CallingConventions.Any && ((m.CallingConvention & callConvention) != callConvention))
						continue;
					match [count++] = m;
				}
			}
			if (binder == null)
				binder = Binder.DefaultBinder;
			return (ConstructorInfo) CheckMethodSecurity (binder.SelectMethod (bindingAttr, match, types, modifiers));
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern ConstructorInfo[] GetConstructors_internal (BindingFlags bindingAttr, Type reflected_type);

		public override ConstructorInfo[] GetConstructors (BindingFlags bindingAttr)
		{
			return GetConstructors_internal (bindingAttr, this);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern EventInfo InternalGetEvent (string name, BindingFlags bindingAttr);

		public override EventInfo GetEvent (string name, BindingFlags bindingAttr)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			return InternalGetEvent (name, bindingAttr);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern EventInfo[] GetEvents_internal (BindingFlags bindingAttr, Type reflected_type);

		public override EventInfo[] GetEvents (BindingFlags bindingAttr)
		{
			return GetEvents_internal (bindingAttr, this);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override FieldInfo GetField (string name, BindingFlags bindingAttr);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern FieldInfo[] GetFields_internal (BindingFlags bindingAttr, Type reflected_type);

		public override FieldInfo[] GetFields (BindingFlags bindingAttr)
		{
			return GetFields_internal (bindingAttr, this);
		}
		
		public override Type GetInterface (string name, bool ignoreCase)
		{
			if (name == null)
				throw new ArgumentNullException ();

			Type[] interfaces = GetInterfaces();

			foreach (Type type in interfaces) {
				if (String.Compare (type.Name, name, ignoreCase, CultureInfo.InvariantCulture) == 0)
					return type;
				if (String.Compare (type.FullName, name, ignoreCase, CultureInfo.InvariantCulture) == 0)
					return type;
			}

			return null;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override Type[] GetInterfaces();
		
		public override MemberInfo[] GetMembers( BindingFlags bindingAttr)
		{
			return FindMembers (MemberTypes.All, bindingAttr, null, null);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern MethodInfo [] GetMethodsByName (string name, BindingFlags bindingAttr, bool ignoreCase, Type reflected_type);

		public override MethodInfo [] GetMethods (BindingFlags bindingAttr)
		{
			return GetMethodsByName (null, bindingAttr, false, this);
		}

		protected override MethodInfo GetMethodImpl (string name, BindingFlags bindingAttr,
							     Binder binder,
							     CallingConventions callConvention,
							     Type[] types, ParameterModifier[] modifiers)
		{
			bool ignoreCase = ((bindingAttr & BindingFlags.IgnoreCase) != 0);
			MethodInfo[] methods = GetMethodsByName (name, bindingAttr, ignoreCase, this);
			MethodInfo found = null;
			MethodBase[] match;
			int count = 0;
			
			foreach (MethodInfo m in methods) {
				// Under MS.NET, Standard|HasThis matches Standard...
				if (callConvention != CallingConventions.Any && ((m.CallingConvention & callConvention) != callConvention))
					continue;
				found = m;
				count++;
			}

			if (count == 0)
				return null;
			
			if (count == 1 && types == null) 
				return (MethodInfo) CheckMethodSecurity (found);

			match = new MethodBase [count];
			if (count == 1)
				match [0] = found;
			else {
				count = 0;
				foreach (MethodInfo m in methods) {
					if (callConvention != CallingConventions.Any && ((m.CallingConvention & callConvention) != callConvention))
						continue;
					match [count++] = m;
				}
			}

			if (types == null) 
				return (MethodInfo) CheckMethodSecurity (Binder.FindMostDerivedMatch (match));

			if (binder == null)
				binder = Binder.DefaultBinder;
			
			return (MethodInfo) CheckMethodSecurity (binder.SelectMethod (bindingAttr, match, types, modifiers));
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern MethodInfo GetCorrespondingInflatedMethod (MethodInfo generic);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern ConstructorInfo GetCorrespondingInflatedConstructor (ConstructorInfo generic);

		internal override MethodInfo GetMethod (MethodInfo fromNoninstanciated)
                {
                        return GetCorrespondingInflatedMethod (fromNoninstanciated);
                }

		internal override ConstructorInfo GetConstructor (ConstructorInfo fromNoninstanciated)
		{
                        return GetCorrespondingInflatedConstructor (fromNoninstanciated);
		}

		internal override FieldInfo GetField (FieldInfo fromNoninstanciated)
		{
			return GetField (fromNoninstanciated.Name);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override Type GetNestedType (string name, BindingFlags bindingAttr);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override Type[] GetNestedTypes (BindingFlags bindingAttr);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern PropertyInfo[] GetPropertiesByName (string name, BindingFlags bindingAttr, bool icase, Type reflected_type);

		public override PropertyInfo [] GetProperties (BindingFlags bindingAttr)
		{
			return GetPropertiesByName (null, bindingAttr, false, this);
		}

		protected override PropertyInfo GetPropertyImpl (string name, BindingFlags bindingAttr,
								 Binder binder, Type returnType,
								 Type[] types,
								 ParameterModifier[] modifiers)
		{
			bool ignoreCase = ((bindingAttr & BindingFlags.IgnoreCase) != 0);
			PropertyInfo [] props = GetPropertiesByName (name, bindingAttr, ignoreCase, this);
			int count = props.Length;
			if (count == 0)
				return null;
			
			if (count == 1 && (types == null || types.Length == 0)) 
				return props [0];

			if (binder == null)
				binder = Binder.DefaultBinder;
			
			return binder.SelectProperty (bindingAttr, props, returnType, types, modifiers);
		}

		protected override bool HasElementTypeImpl ()
		{
			return IsArrayImpl() || IsByRefImpl() || IsPointerImpl ();
		}

		protected override bool IsArrayImpl ()
		{
			return Type.IsArrayImpl (this);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected extern override bool IsByRefImpl ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		protected extern override bool IsCOMObjectImpl ();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected extern override bool IsPointerImpl ();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		protected extern override bool IsPrimitiveImpl ();

		protected override bool IsValueTypeImpl ()
		{
			return type_is_subtype_of (this, typeof (System.ValueType), false) &&
				this != typeof (System.ValueType) &&
				this != typeof (System.Enum);
		}
		
		public override object InvokeMember (string name, BindingFlags invokeAttr,
						     Binder binder, object target, object[] args,
						     ParameterModifier[] modifiers,
						     CultureInfo culture, string[] namedParameters)
		{

			if ((invokeAttr & BindingFlags.CreateInstance) != 0) {
				if ((invokeAttr & (BindingFlags.GetField |
						BindingFlags.GetField | BindingFlags.GetProperty |
						BindingFlags.SetProperty)) != 0)
					throw new ArgumentException ("invokeAttr");
			} else if (name == null)
				throw new ArgumentNullException ("name");
			if ((invokeAttr & BindingFlags.GetField) != 0 && (invokeAttr & BindingFlags.SetField) != 0)
				throw new ArgumentException ("invokeAttr");
			if ((invokeAttr & BindingFlags.GetProperty) != 0 && (invokeAttr & BindingFlags.SetProperty) != 0)
				throw new ArgumentException ("invokeAttr");
			if ((invokeAttr & BindingFlags.InvokeMethod) != 0 && (invokeAttr & (BindingFlags.SetProperty|BindingFlags.SetField)) != 0)
				throw new ArgumentException ("invokeAttr");
			if ((namedParameters != null) && ((args == null) || args.Length < namedParameters.Length))
				throw new ArgumentException ("namedParameters cannot be more than named arguments in number");

			/* set some defaults if none are provided :-( */
			if ((invokeAttr & (BindingFlags.Public|BindingFlags.NonPublic)) == 0)
				invokeAttr |= BindingFlags.Public;
			if ((invokeAttr & (BindingFlags.Static|BindingFlags.Instance)) == 0)
				invokeAttr |= BindingFlags.Static|BindingFlags.Instance;

			if (binder == null)
				binder = Binder.DefaultBinder;
			if ((invokeAttr & BindingFlags.CreateInstance) != 0) {
				/* the name is ignored */
				invokeAttr |= BindingFlags.DeclaredOnly;
				ConstructorInfo[] ctors = GetConstructors (invokeAttr);
				object state = null;
				MethodBase ctor = binder.BindToMethod (invokeAttr, ctors, ref args, modifiers, culture, namedParameters, out state);
				if (ctor == null) {
					if (this.IsValueType && args == null)
						return Activator.CreateInstanceInternal (this);
					
					throw new MissingMethodException ("Constructor on type '" + FullName + "' not found.");
				}
				object result = ctor.Invoke (target, invokeAttr, binder, args, culture);
				binder.ReorderArgumentArray (ref args, state);
				return result;
			}
			if (name == String.Empty && Attribute.IsDefined (this, typeof (DefaultMemberAttribute))) {
				DefaultMemberAttribute attr = (DefaultMemberAttribute) Attribute.GetCustomAttribute (this, typeof (DefaultMemberAttribute));
				name = attr.MemberName;
			}
			bool ignoreCase = (invokeAttr & BindingFlags.IgnoreCase) != 0;
			string throwMissingMethodDescription = null;
			bool throwMissingFieldException = false;
			
			if ((invokeAttr & BindingFlags.InvokeMethod) != 0) {
				MethodInfo[] methods = GetMethodsByName (name, invokeAttr, ignoreCase, this);
				object state = null;
				MethodBase m = binder.BindToMethod (invokeAttr, methods, ref args, modifiers, culture, namedParameters, out state);
				if (m == null) {
					if (methods.Length > 0)
						throwMissingMethodDescription = "The best match for method " + name + " has some invalid parameter.";
					else
						throwMissingMethodDescription = "Cannot find method " + name + ".";
				} else {
					object result = m.Invoke (target, invokeAttr, binder, args, culture);
					binder.ReorderArgumentArray (ref args, state);
					return result;
				}
			}
			if ((invokeAttr & BindingFlags.GetField) != 0) {
				FieldInfo f = GetField (name, invokeAttr);
				if (f != null) {
					return f.GetValue (target);
				} else if ((invokeAttr & BindingFlags.GetProperty) == 0) {
					throwMissingFieldException = true;
				}
				/* try GetProperty */
			} else if ((invokeAttr & BindingFlags.SetField) != 0) {
				if ((args == null) || args.Length != 1)
					throw new ArgumentException ("invokeAttr");

				FieldInfo f = GetField (name, invokeAttr);
				if (f != null) {
					f.SetValue (target, args [0]);
					return null;
				} else if ((invokeAttr & BindingFlags.SetProperty) == 0) {
					throwMissingFieldException = true;
				}
				/* try SetProperty */
			}
			if ((invokeAttr & BindingFlags.GetProperty) != 0) {
				PropertyInfo[] properties = GetPropertiesByName (name, invokeAttr, ignoreCase, this);
				object state = null;
				int i, count = 0;
				for (i = 0; i < properties.Length; ++i) {
					if ((properties [i].GetGetMethod (true) != null))
						count++;
				}
				MethodBase[] smethods = new MethodBase [count];
				count = 0;
				for (i = 0; i < properties.Length; ++i) {
					MethodBase mb = properties [i].GetGetMethod (true);
					if (mb != null)
						smethods [count++] = mb;
				}
				MethodBase m = binder.BindToMethod (invokeAttr, smethods, ref args, modifiers, culture, namedParameters, out state);
				if (m == null) {
					throwMissingFieldException = true;
				} else {
					object result = m.Invoke (target, invokeAttr, binder, args, culture);
					binder.ReorderArgumentArray (ref args, state);
					return result;
				}
			} else if ((invokeAttr & BindingFlags.SetProperty) != 0) {
				PropertyInfo[] properties = GetPropertiesByName (name, invokeAttr, ignoreCase, this);
				object state = null;
				int i, count = 0;
				for (i = 0; i < properties.Length; ++i) {
					if (properties [i].GetSetMethod (true) != null)
						count++;
				}
				MethodBase[] smethods = new MethodBase [count];
				count = 0;
				for (i = 0; i < properties.Length; ++i) {
					MethodBase mb = properties [i].GetSetMethod (true);
					if (mb != null)
						smethods [count++] = mb;
				}
				MethodBase m = binder.BindToMethod (invokeAttr, smethods, ref args, modifiers, culture, namedParameters, out state);
				if (m == null) {
					throwMissingFieldException = true;
				} else {
					object result = m.Invoke (target, invokeAttr, binder, args, culture);
					binder.ReorderArgumentArray (ref args, state);
					return result;
				}
			}
			if (throwMissingMethodDescription != null)
				throw new MissingMethodException(throwMissingMethodDescription);
			if (throwMissingFieldException)
				throw new MissingFieldException("Cannot find variable " + name + ".");

			return null;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override Type GetElementType ();

		public override Type UnderlyingSystemType {
			get {
				// This has _nothing_ to do with getting the base type of an enum etc.
				return this;
			}
		}

		public extern override Assembly Assembly {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public override string AssemblyQualifiedName {
			get {
				return getFullName (true, true);
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern string getFullName(bool full_name, bool assembly_qualified);

		public extern override Type BaseType {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public override string FullName {
			get {
				return getFullName (true, false);
			}
		}

		public override Guid GUID {
			get {
				object[] att = GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), true);
				if (att.Length == 0)
					return Guid.Empty;
				return new Guid(((System.Runtime.InteropServices.GuidAttribute)att[0]).Value);
			}
		}

		public override bool IsDefined (Type attributeType, bool inherit)
		{
			return MonoCustomAttrs.IsDefined (this, attributeType, inherit);
		}

		public override object[] GetCustomAttributes (bool inherit)
		{
			return MonoCustomAttrs.GetCustomAttributes (this, inherit);
		}

		public override object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			if (attributeType == null)
			{
				throw new ArgumentNullException("attributeType");
			}
			
			return MonoCustomAttrs.GetCustomAttributes (this, attributeType, inherit);
		}

		public override MemberTypes MemberType {
			get {
				if (DeclaringType != null)
					return MemberTypes.NestedType;
				else
					return MemberTypes.TypeInfo;
			}
		}

		public extern override string Name {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public extern override string Namespace {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public extern override Module Module {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public extern override Type DeclaringType {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public override Type ReflectedType {
			get {
				return DeclaringType;
			}
		}

		public override RuntimeTypeHandle TypeHandle {
			get {
				return _impl;
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override int GetArrayRank ();

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			UnitySerializationHolder.GetTypeData (this, info, context);
		}

		public override string ToString()
		{
			return getFullName (false, false);
		}

#if NET_2_0 || BOOTSTRAP_NET_2_0
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override Type [] GetGenericArguments ();

		public override bool ContainsGenericParameters {
			get {
				if (IsGenericParameter)
					return true;

				if (IsGenericType) {
					foreach (Type arg in GetGenericArguments ())
						if (arg.ContainsGenericParameters)
							return true;
				}

				if (HasElementType)
					return GetElementType ().ContainsGenericParameters;

				return false;
			}
		}

		public extern override bool IsGenericParameter {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public extern override MethodBase DeclaringMethod {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}
#endif

		private MethodBase CheckMethodSecurity (MethodBase mb)
		{
			if (!SecurityManager.SecurityEnabled || (mb == null))
				return mb;

			// Sadly we have no way to know which kind of security action this is
			// so we must do it the hard way. Actually this isn't so bad 
			// because we can skip the (mb.Attributes & MethodAttributes.HasSecurity)
			// icall required (and do it ourselves)

			// this (unlike the Invoke step) is _and stays_ a LinkDemand (caller)
			return SecurityManager.ReflectedLinkDemandQuery (mb) ? mb : null;
		}
	}
}
