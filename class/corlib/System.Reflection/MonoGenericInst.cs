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
using System.Collections;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Reflection
{
	internal class MonoGenericInst : MonoType
	{
		private IntPtr klass;
		protected MonoGenericInst parent;
		protected Type generic_type;
		private MonoGenericInst[] interfaces;
		private MethodInfo[] methods;
		private ConstructorInfo[] ctors;
		private FieldInfo[] fields;

		[MonoTODO]
		internal MonoGenericInst ()
			: base (null)
		{
			// this should not be used
			throw new InvalidOperationException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern MethodInfo inflate_method (MonoGenericInst declaring, MonoGenericInst reflected, MethodInfo method);
	
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern ConstructorInfo inflate_ctor (MonoGenericInst declaring, MonoGenericInst reflected, ConstructorInfo ctor);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern FieldInfo inflate_field (MonoGenericInst declaring, MonoGenericInst reflected, FieldInfo field);

		private const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
		BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

		protected void inflate (MonoGenericInst reflected,
					ArrayList mlist, ArrayList clist, ArrayList flist)
		{
			if (parent != null)
				parent.inflate (reflected, mlist, clist, flist);
			else if (BaseType != null) {
				mlist.AddRange (generic_type.BaseType.GetMethods (flags));
				clist.AddRange (generic_type.BaseType.GetConstructors (flags));
				flist.AddRange (generic_type.BaseType.GetFields (flags));
			} else if (interfaces != null) {
				foreach (MonoGenericInst iface in interfaces)
					iface.inflate (iface, mlist, clist, flist);
			}

			foreach (MethodInfo m in generic_type.GetMethods (flags))
				mlist.Add (inflate_method (this, reflected, m));
			foreach (ConstructorInfo c in generic_type.GetConstructors (flags))
				clist.Add (inflate_ctor (this, reflected, c));
			foreach (FieldInfo f in generic_type.GetFields (flags))
				flist.Add (inflate_field (this, reflected, f));
		}

		void initialize ()
		{
			ArrayList mlist = new ArrayList ();
			ArrayList clist = new ArrayList ();
			ArrayList flist = new ArrayList ();

			inflate (this, mlist, clist, flist);

			methods = new MethodInfo [mlist.Count];
			mlist.CopyTo (methods, 0);

			ctors = new ConstructorInfo [clist.Count];
			clist.CopyTo (ctors, 0);

			fields = new FieldInfo [flist.Count];
			flist.CopyTo (fields, 0);
		}

		public override Type BaseType {
			get { return parent != null ? parent : generic_type.BaseType; }
		}

		protected override bool IsValueTypeImpl ()
		{
			if (BaseType == null)
				return false;
			if (BaseType == typeof (Enum) || BaseType == typeof (ValueType))
				return true;

			return BaseType.IsSubclassOf (typeof (ValueType));
		}

		public override MethodInfo[] GetMethods (BindingFlags bindingAttr)
		{
			if (methods == null)
				initialize ();

			return GetMethods_impl (bindingAttr);
		}

		protected MethodInfo[] GetMethods_impl (BindingFlags bindingAttr)
		{
			ArrayList l = new ArrayList ();
			bool match;
			MethodAttributes mattrs;

			foreach (MethodInfo c in methods) {
				match = false;
				mattrs = c.Attributes;
				if ((mattrs & MethodAttributes.MemberAccessMask) == MethodAttributes.Public) {
					if ((bindingAttr & BindingFlags.Public) != 0)
						match = true;
				} else {
					if ((bindingAttr & BindingFlags.NonPublic) != 0)
						match = true;
				}
				if (!match)
					continue;
				match = false;
				if ((mattrs & MethodAttributes.Static) != 0) {
					if ((bindingAttr & BindingFlags.Static) != 0)
						match = true;
				} else {
					if ((bindingAttr & BindingFlags.Instance) != 0)
						match = true;
				}
				if (!match)
					continue;
				l.Add (c);
			}
			MethodInfo[] result = new MethodInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		public override ConstructorInfo[] GetConstructors (BindingFlags bindingAttr)
		{
			if (ctors == null)
				initialize ();

			return GetConstructors_impl (bindingAttr);
		}

		protected ConstructorInfo[] GetConstructors_impl (BindingFlags bindingAttr)
		{
			ArrayList l = new ArrayList ();
			bool match;
			MethodAttributes mattrs;

			foreach (ConstructorInfo c in ctors) {
				match = false;
				mattrs = c.Attributes;
				if ((mattrs & MethodAttributes.MemberAccessMask) == MethodAttributes.Public) {
					if ((bindingAttr & BindingFlags.Public) != 0)
						match = true;
				} else {
					if ((bindingAttr & BindingFlags.NonPublic) != 0)
						match = true;
				}
				if (!match)
					continue;
				match = false;
				if ((mattrs & MethodAttributes.Static) != 0) {
					if ((bindingAttr & BindingFlags.Static) != 0)
						match = true;
				} else {
					if ((bindingAttr & BindingFlags.Instance) != 0)
						match = true;
				}
				if (!match)
					continue;
				l.Add (c);
			}
			ConstructorInfo[] result = new ConstructorInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		public override FieldInfo[] GetFields (BindingFlags bindingAttr)
		{
			if (fields == null)
				initialize ();

			return GetFields_impl (bindingAttr);
		}

		protected FieldInfo[] GetFields_impl (BindingFlags bindingAttr)
		{
			ArrayList l = new ArrayList ();
			bool match;
			FieldAttributes fattrs;

			foreach (FieldInfo c in fields) {
				match = false;
				fattrs = c.Attributes;
				if ((fattrs & FieldAttributes.FieldAccessMask) == FieldAttributes.Public) {
					if ((bindingAttr & BindingFlags.Public) != 0)
						match = true;
				} else {
					if ((bindingAttr & BindingFlags.NonPublic) != 0)
						match = true;
				}
				if (!match)
					continue;
				match = false;
				if ((fattrs & FieldAttributes.Static) != 0) {
					if ((bindingAttr & BindingFlags.Static) != 0)
						match = true;
				} else {
					if ((bindingAttr & BindingFlags.Instance) != 0)
						match = true;
				}
				if (!match)
					continue;
				l.Add (c);
			}
			FieldInfo[] result = new FieldInfo [l.Count];
			l.CopyTo (result);
			return result;
		}
	}

	internal class MonoInflatedMethod : MonoMethod
	{
		private readonly MethodInfo declaring;
		private readonly MonoGenericInst declaring_type;
		private readonly MonoGenericInst reflected_type;
		private readonly IntPtr ginst;

		public override Type DeclaringType {
			get {
				return declaring_type != null ? declaring_type : base.DeclaringType;
			}
		}

		public override Type ReflectedType {
			get {
				return reflected_type != null ? reflected_type : base.ReflectedType;
			}
		}

		public override bool IsDefined (Type attributeType, bool inherit)
		{
			// FIXME
			return false;
		}
		public override object[] GetCustomAttributes (bool inherit)
		{
			// FIXME
			return new object [0];
		}
		public override object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			// FIXME
			return new object [0];
		}
	}

	internal class MonoInflatedCtor : MonoCMethod
	{
		private readonly ConstructorInfo declaring;
		private readonly MonoGenericInst declaring_type;
		private readonly MonoGenericInst reflected_type;
		private readonly IntPtr ginst;

		public override Type DeclaringType {
			get {
				return declaring_type != null ? declaring_type : base.DeclaringType;
			}
		}

		public override Type ReflectedType {
			get {
				return reflected_type != null ? reflected_type : base.ReflectedType;
			}
		}

		public override bool IsDefined (Type attributeType, bool inherit)
		{
			// FIXME
			return false;
		}
		public override object[] GetCustomAttributes (bool inherit)
		{
			// FIXME
			return new object [0];
		}
		public override object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			// FIXME
			return new object [0];
		}
	}

	internal class MonoInflatedField : MonoField
	{
		private readonly IntPtr dhandle;
		private readonly MonoGenericInst declaring_type;
		private readonly MonoGenericInst reflected_type;

		public override Type DeclaringType {
			get {
				return declaring_type != null ? declaring_type : base.DeclaringType;
			}
		}

		public override Type ReflectedType {
			get {
				return reflected_type != null ? reflected_type : base.ReflectedType;
			}
		}
	}
}
