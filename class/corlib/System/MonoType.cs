//
// System.MonoType
//
// Sean MacIsaac (macisaac@ximian.com)
// Paolo Molaro (lupus@ximian.com)
// Patrik Torstensson (patrik.torstensson@labs2.com)
//
// (C) 2001 Ximian, Inc.
//

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Runtime.Serialization;

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
				return found;
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
			return (ConstructorInfo)binder.SelectMethod (bindingAttr, match, types, modifiers);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override ConstructorInfo[] GetConstructors (BindingFlags bindingAttr);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern EventInfo InternalGetEvent (string name, BindingFlags bindingAttr);

		public override EventInfo GetEvent (string name, BindingFlags bindingAttr)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			return InternalGetEvent (name, bindingAttr);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override EventInfo[] GetEvents (BindingFlags bindingAttr);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override FieldInfo GetField (string name, BindingFlags bindingAttr);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override FieldInfo[] GetFields (BindingFlags bindingAttr);

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
		public extern override MethodInfo[] GetMethods (BindingFlags bindingAttr);

		protected override MethodInfo GetMethodImpl (string name, BindingFlags bindingAttr,
							     Binder binder,
							     CallingConventions callConvention,
							     Type[] types, ParameterModifier[] modifiers)
		{
			MethodInfo[] methods = GetMethods (bindingAttr);
			bool ignoreCase = ((bindingAttr & BindingFlags.IgnoreCase) != 0);
			MethodInfo found = null;
			MethodBase[] match;
			int typesLen = (types != null) ? types.Length : 0;
			int count = 0;
			
			foreach (MethodInfo m in methods) {
				if (String.Compare (m.Name, name, ignoreCase, CultureInfo.InvariantCulture) != 0)
					continue;
				// Under MS.NET, Standard|HasThis matches Standard...
				if (callConvention != CallingConventions.Any && ((m.CallingConvention & callConvention) != callConvention))
					continue;
				found = m;
				count++;
			}

			if (count == 0)
				return null;
			
			if (count == 1 && typesLen == 0) 
				return found;

			match = new MethodBase [count];
			if (count == 1)
				match [0] = found;
			else {
				count = 0;
				foreach (MethodInfo m in methods) {
					if (String.Compare (m.Name, name, ignoreCase, CultureInfo.InvariantCulture) != 0)
						continue;
					if (callConvention != CallingConventions.Any && ((m.CallingConvention & callConvention) != callConvention))
						continue;
					match [count++] = m;
				}
			}
			
			if (types == null) 
				return (MethodInfo) Binder.FindMostDerivedMatch (match);

			if (binder == null)
				binder = Binder.DefaultBinder;
			
			return (MethodInfo)binder.SelectMethod (bindingAttr, match, types, modifiers);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override Type GetNestedType (string name, BindingFlags bindingAttr);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override Type[] GetNestedTypes (BindingFlags bindingAttr);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override PropertyInfo[] GetProperties( BindingFlags bindingAttr);

		[MonoTODO]
		protected override PropertyInfo GetPropertyImpl (string name, BindingFlags bindingAttr,
								 Binder binder, Type returnType,
								 Type[] types,
								 ParameterModifier[] modifiers)
		{
			// fixme: needs to use the binder, and send the modifiers to that binder
			if (null == name || types == null)
				throw new ArgumentNullException ();
			
			PropertyInfo ret = null;
			PropertyInfo [] props = GetProperties(bindingAttr);
			bool ignoreCase = ((bindingAttr & BindingFlags.IgnoreCase) != 0);

			foreach (PropertyInfo info in props) {
					if (String.Compare (info.Name, name, ignoreCase, CultureInfo.InvariantCulture) != 0) 
						continue;

					if (returnType != null && info.PropertyType != returnType)
							continue;

					if (types.Length > 0) {
						ParameterInfo[] parameterInfo = info.GetIndexParameters ();

						if (parameterInfo.Length != types.Length)
							continue;

						int i;
						bool match = true;

						for (i = 0; i < types.Length; i ++)
							if (parameterInfo [i].ParameterType != types [i]) {
								match = false;
								break;
							}

						if (!match)
							continue;
					}

					if (null != ret)
						throw new AmbiguousMatchException();

					ret = info;
			}

			return ret;
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

		protected override bool IsCOMObjectImpl ()
		{
			return false;
		}

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
			if ((invokeAttr & BindingFlags.SetField) != 0 && ((args == null) || args.Length != 1))
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
				if (ctor == null)
					throw new MissingMethodException ();
				object result = ctor.Invoke (target, invokeAttr, binder, args, culture);
				binder.ReorderArgumentArray (ref args, state);
				return result;
			}
			if (name == String.Empty && Attribute.IsDefined (this, typeof (DefaultMemberAttribute))) {
				DefaultMemberAttribute attr = (DefaultMemberAttribute) Attribute.GetCustomAttribute (this, typeof (DefaultMemberAttribute));
				name = attr.MemberName;
			}
			bool ignoreCase = (invokeAttr & BindingFlags.IgnoreCase) != 0;
			if ((invokeAttr & BindingFlags.InvokeMethod) != 0) {
				MethodInfo[] methods = GetMethods (invokeAttr);
				object state = null;
				int i, count = 0;
				for (i = 0; i < methods.Length; ++i) {
					if (String.Compare (methods [i].Name, name, ignoreCase, CultureInfo.InvariantCulture) == 0)
						count++;
				}
				MethodBase[] smethods = new MethodBase [count];
				count = 0;
				for (i = 0; i < methods.Length; ++i) {
					if (String.Compare (methods [i].Name, name, ignoreCase, CultureInfo.InvariantCulture) == 0)
						smethods [count++] = methods [i];
				}
				MethodBase m = binder.BindToMethod (invokeAttr, smethods, ref args, modifiers, culture, namedParameters, out state);
				if (m == null)
					throw new MissingMethodException ();
				object result = m.Invoke (target, invokeAttr, binder, args, culture);
				binder.ReorderArgumentArray (ref args, state);
				return result;
			}
			if ((invokeAttr & BindingFlags.GetField) != 0) {
				FieldInfo f = GetField (name, invokeAttr);
				if (f != null) {
					return f.GetValue (target);
				} else if ((invokeAttr & BindingFlags.GetProperty) == 0) {
					throw new MissingFieldException ();
				}
				/* try GetProperty */
			} else if ((invokeAttr & BindingFlags.SetField) != 0) {
				FieldInfo f = GetField (name, invokeAttr);
				if (f != null) {
					f.SetValue (target, args [0]);
					return null;
				} else if ((invokeAttr & BindingFlags.SetProperty) == 0) {
					throw new MissingFieldException ();
				}
				/* try SetProperty */
			}
			if ((invokeAttr & BindingFlags.GetProperty) != 0) {
				PropertyInfo[] properties = GetProperties (invokeAttr);
				object state = null;
				int i, count = 0;
				for (i = 0; i < properties.Length; ++i) {
					if (String.Compare (properties [i].Name, name, ignoreCase, CultureInfo.InvariantCulture) == 0 && (properties [i].GetGetMethod () != null))
						count++;
				}
				MethodBase[] smethods = new MethodBase [count];
				count = 0;
				for (i = 0; i < properties.Length; ++i) {
					MethodBase mb = properties [i].GetGetMethod ();
					if (String.Compare (properties [i].Name, name, ignoreCase, CultureInfo.InvariantCulture) == 0 && (mb != null))
						smethods [count++] = mb;
				}
				MethodBase m = binder.BindToMethod (invokeAttr, smethods, ref args, modifiers, culture, namedParameters, out state);
				if (m == null)
					throw new MissingFieldException ();
				object result = m.Invoke (target, invokeAttr, binder, args, culture);
				binder.ReorderArgumentArray (ref args, state);
				return result;
			} else if ((invokeAttr & BindingFlags.SetProperty) != 0) {
				PropertyInfo[] properties = GetProperties (invokeAttr);
				object state = null;
				int i, count = 0;
				for (i = 0; i < properties.Length; ++i) {
					if (String.Compare (properties [i].Name, name, ignoreCase, CultureInfo.InvariantCulture) == 0 && (properties [i].GetSetMethod () != null))
						count++;
				}
				MethodBase[] smethods = new MethodBase [count];
				count = 0;
				for (i = 0; i < properties.Length; ++i) {
					MethodBase mb = properties [i].GetSetMethod ();
					if (String.Compare (properties [i].Name, name, ignoreCase, CultureInfo.InvariantCulture) == 0 && (mb != null))
						smethods [count++] = mb;
				}
				MethodBase m = binder.BindToMethod (invokeAttr, smethods, ref args, modifiers, culture, namedParameters, out state);
				if (m == null)
					throw new MissingFieldException ();
				object result = m.Invoke (target, invokeAttr, binder, args, culture);
				binder.ReorderArgumentArray (ref args, state);
				return result;
			}
			return null;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern override Type GetElementType ();

		public extern override Type UnderlyingSystemType {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public extern override Assembly Assembly {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public override string AssemblyQualifiedName {
			get {
				return getFullName () + ", " + Assembly.ToString ();
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern string getFullName();

		public extern override Type BaseType {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public override string FullName {
			get {
				return getFullName ();
			}
		}

		public override Guid GUID {
			get {
				return Guid.Empty;
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

#if NET_1_2
		public extern override bool HasGenericArguments {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}

		public override bool ContainsGenericParameters {
			get {
				if (IsGenericParameter)
					return true;

				if (HasGenericArguments) {
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

		public extern override MethodInfo DeclaringMethod {
			[MethodImplAttribute(MethodImplOptions.InternalCall)]
			get;
		}
#endif
	}
}
