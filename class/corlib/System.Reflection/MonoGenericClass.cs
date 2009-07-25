//
// System.Reflection.MonoGenericClass
//
// Sean MacIsaac (macisaac@ximian.com)
// Paolo Molaro (lupus@ximian.com)
// Patrik Torstensson (patrik.torstensson@labs2.com)
//
// (C) 2001 Ximian, Inc.
//

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

using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Runtime.Serialization;

#if NET_2_0 || BOOTSTRAP_NET_2_0

namespace System.Reflection
{
	/*
	 * MonoGenericClass represents an instantiation of a generic TypeBuilder. MS
	 * calls this class TypeBuilderInstantiation (a much better name). MS returns 
	 * NotImplementedException for many of the methods but we can't do that as gmcs
	 * depends on them.
	 */
	internal class MonoGenericClass : MonoType
	{
		#region Keep in sync with object-internals.h
#pragma warning disable 649
		internal TypeBuilder generic_type;
		bool initialized;
#pragma warning restore 649
		#endregion

		Hashtable fields, ctors, methods;
		int event_count;

		internal MonoGenericClass ()
			: base (null)
		{
			// this should not be used
			throw new InvalidOperationException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern void initialize (MethodInfo[] methods, ConstructorInfo[] ctors, FieldInfo[] fields, PropertyInfo[] properties, EventInfo[] events);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern MethodInfo GetCorrespondingInflatedMethod (MethodInfo generic);

		private const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
		BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

		void initialize ()
		{
			if (initialized)
				return;

			MonoGenericClass parent = GetParentType () as MonoGenericClass;
			if (parent != null)
				parent.initialize ();

			EventInfo[] events = generic_type.GetEvents_internal (flags);
			event_count = events.Length;

			initialize (generic_type.GetMethods (flags),
						generic_type.GetConstructors (flags),
						generic_type.GetFields (flags),
						generic_type.GetProperties (flags),
						events);

			initialized = true;
		}

		Type GetParentType ()
		{
			return InflateType (generic_type.BaseType);		
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern Type InflateType_internal (Type type);

		internal Type InflateType (Type type)
		{
			if (type == null)
				return null;
			if (!type.IsGenericParameter && !type.ContainsGenericParameters)
				return type;
			return InflateType_internal (type);
		}
		
		public override Type BaseType {
			get {
				Type parent = GetParentType ();
				return parent != null ? parent : generic_type.BaseType;
			}
		}

		Type[] GetInterfacesInternal ()
		{
			if (generic_type.interfaces == null)
				return new Type [0];
			Type[] res = new Type [generic_type.interfaces.Length];
			for (int i = 0; i < res.Length; ++i)
				res [i] = InflateType (generic_type.interfaces [i]);
			return res;
		}

		public override Type[] GetInterfaces ()
		{
			if (!generic_type.IsCompilerContext)
				throw new NotSupportedException ();
			return GetInterfacesInternal ();
		}

		protected override bool IsValueTypeImpl ()
		{
			return generic_type.IsValueType;
		}

		internal override MethodInfo GetMethod (MethodInfo fromNoninstanciated)
		{
			initialize ();

			if (!(fromNoninstanciated is MethodBuilder))
				throw new InvalidOperationException ("Inflating non MethodBuilder objects is not supported: " + fromNoninstanciated.GetType ());
	
			if (fromNoninstanciated is MethodBuilder) {
				MethodBuilder mb = (MethodBuilder)fromNoninstanciated;

				// FIXME: We can't yet handle creating generic instantiations of
				// MethodOnTypeBuilderInst objects
				// Also, mono_image_get_method_on_inst_token () can't handle generic
				// methods
				if (!mb.IsGenericMethodDefinition) {
					if (methods == null)
						methods = new Hashtable ();
					if (!methods.ContainsKey (mb))
						methods [mb] = new MethodOnTypeBuilderInst (this, mb);
					return (MethodInfo)methods [mb];
				}
			}

			return GetCorrespondingInflatedMethod (fromNoninstanciated);
		}

		internal override ConstructorInfo GetConstructor (ConstructorInfo fromNoninstanciated)
		{
			initialize ();

			if (!(fromNoninstanciated is ConstructorBuilder))
				throw new InvalidOperationException ("Inflating non ConstructorBuilder objects is not supported: " + fromNoninstanciated.GetType ());

			ConstructorBuilder cb = (ConstructorBuilder)fromNoninstanciated;
			if (ctors == null)
				ctors = new Hashtable ();
			if (!ctors.ContainsKey (cb))
				ctors [cb] = new ConstructorOnTypeBuilderInst (this, cb);
			return (ConstructorInfo)ctors [cb];
		}

		internal override FieldInfo GetField (FieldInfo fromNoninstanciated)
		{
			initialize ();

			if (fromNoninstanciated is FieldOnTypeBuilderInst && generic_type.IsCompilerContext) {
				FieldOnTypeBuilderInst finst = (FieldOnTypeBuilderInst)fromNoninstanciated;
				fromNoninstanciated = finst.fb;
			}

			if (!(fromNoninstanciated is FieldBuilder))
				throw new InvalidOperationException ("Inflating non FieldBuilder objects is not supported: " + fromNoninstanciated.GetType ());

			FieldBuilder fb = (FieldBuilder)fromNoninstanciated;
			if (fields == null)
				fields = new Hashtable ();
			if (!fields.ContainsKey (fb))
				fields [fb] = new FieldOnTypeBuilderInst (this, fb);
			return (FieldInfo)fields [fb];
		}
		
		public override MethodInfo[] GetMethods (BindingFlags bf)
		{
			if (!generic_type.IsCompilerContext)
				throw new NotSupportedException ();

			ArrayList l = new ArrayList ();

			//
			// Walk up our class hierarchy and retrieve methods from our
			// parent classes.
			//

			Type current_type = this;
			do {
				MonoGenericClass gi = current_type as MonoGenericClass;
				if (gi != null)
					l.AddRange (gi.GetMethodsInternal (bf, this));
				else if (current_type is TypeBuilder)
					l.AddRange (current_type.GetMethods (bf));
				else {
					// If we encounter a `MonoType', its
					// GetMethodsByName() will return all the methods
					// from its parent type(s), so we can stop here.
					MonoType mt = (MonoType) current_type;
					l.AddRange (mt.GetMethodsByName (null, bf, false, this));
					break;
				}

				if ((bf & BindingFlags.DeclaredOnly) != 0)
					break;
				current_type = current_type.BaseType;
			} while (current_type != null);

			MethodInfo[] result = new MethodInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		MethodInfo[] GetMethodsInternal (BindingFlags bf, MonoGenericClass reftype)
		{
			if (generic_type.num_methods == 0)
				return new MethodInfo [0];

			ArrayList l = new ArrayList ();
			bool match;
			MethodAttributes mattrs;
			MethodInfo accessor;

			initialize ();

			for (int i = 0; i < generic_type.num_methods; ++i) {
				MethodInfo c = generic_type.methods [i];

				match = false;
				mattrs = c.Attributes;
				if ((mattrs & MethodAttributes.MemberAccessMask) == MethodAttributes.Public) {
					if ((bf & BindingFlags.Public) != 0)
						match = true;
				} else {
					if ((bf & BindingFlags.NonPublic) != 0)
						match = true;
				}
				if (!match)
					continue;
				match = false;
				if ((mattrs & MethodAttributes.Static) != 0) {
					if ((bf & BindingFlags.Static) != 0)
						match = true;
				} else {
					if ((bf & BindingFlags.Instance) != 0)
						match = true;
				}
				if (!match)
					continue;
				c = TypeBuilder.GetMethod (this, c);
				l.Add (c);
			}

			MethodInfo[] result = new MethodInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		public override ConstructorInfo[] GetConstructors (BindingFlags bf)
		{
			if (!generic_type.IsCompilerContext)
				throw new NotSupportedException ();

			ArrayList l = new ArrayList ();

			Type current_type = this;
			do {
				MonoGenericClass gi = current_type as MonoGenericClass;
				if (gi != null)
					l.AddRange (gi.GetConstructorsInternal (bf, this));
				else if (current_type is TypeBuilder)
					l.AddRange (current_type.GetConstructors (bf));
				else {
					MonoType mt = (MonoType) current_type;
					l.AddRange (mt.GetConstructors_internal (bf, this));
					break;
				}

				if ((bf & BindingFlags.DeclaredOnly) != 0)
					break;
				current_type = current_type.BaseType;
			} while (current_type != null);

			ConstructorInfo[] result = new ConstructorInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		ConstructorInfo[] GetConstructorsInternal (BindingFlags bf, MonoGenericClass reftype)
		{
			if (generic_type.ctors == null)
				return new ConstructorInfo [0];

			ArrayList l = new ArrayList ();
			bool match;
			MethodAttributes mattrs;

			initialize ();

			for (int i = 0; i < generic_type.ctors.Length; i++) {
				ConstructorInfo c = generic_type.ctors [i];

				match = false;
				mattrs = c.Attributes;
				if ((mattrs & MethodAttributes.MemberAccessMask) == MethodAttributes.Public) {
					if ((bf & BindingFlags.Public) != 0)
						match = true;
				} else {
					if ((bf & BindingFlags.NonPublic) != 0)
						match = true;
				}
				if (!match)
					continue;
				match = false;
				if ((mattrs & MethodAttributes.Static) != 0) {
					if ((bf & BindingFlags.Static) != 0)
						match = true;
				} else {
					if ((bf & BindingFlags.Instance) != 0)
						match = true;
				}
				if (!match)
					continue;
				l.Add (TypeBuilder.GetConstructor (this, c));
			}

			ConstructorInfo[] result = new ConstructorInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		public override FieldInfo[] GetFields (BindingFlags bf)
		{
			if (!generic_type.IsCompilerContext)
				throw new NotSupportedException ();

			ArrayList l = new ArrayList ();

			Type current_type = this;
			do {
				MonoGenericClass gi = current_type as MonoGenericClass;
				if (gi != null)
					l.AddRange (gi.GetFieldsInternal (bf, this));
				else if (current_type is TypeBuilder)
					l.AddRange (current_type.GetFields (bf));
				else {
					MonoType mt = (MonoType) current_type;
					l.AddRange (mt.GetFields_internal (bf, this));
					break;
				}

				if ((bf & BindingFlags.DeclaredOnly) != 0)
					break;
				current_type = current_type.BaseType;
			} while (current_type != null);

			FieldInfo[] result = new FieldInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		FieldInfo[] GetFieldsInternal (BindingFlags bf, MonoGenericClass reftype)
		{
			if (generic_type.num_fields == 0)
				return new FieldInfo [0];

			ArrayList l = new ArrayList ();
			bool match;
			FieldAttributes fattrs;

			initialize ();

			for (int i = 0; i < generic_type.num_fields; i++) {
				FieldInfo c = generic_type.fields [i];

				match = false;
				fattrs = c.Attributes;
				if ((fattrs & FieldAttributes.FieldAccessMask) == FieldAttributes.Public) {
					if ((bf & BindingFlags.Public) != 0)
						match = true;
				} else {
					if ((bf & BindingFlags.NonPublic) != 0)
						match = true;
				}
				if (!match)
					continue;
				match = false;
				if ((fattrs & FieldAttributes.Static) != 0) {
					if ((bf & BindingFlags.Static) != 0)
						match = true;
				} else {
					if ((bf & BindingFlags.Instance) != 0)
						match = true;
				}
				if (!match)
					continue;
				l.Add (TypeBuilder.GetField (this, c));
			}

			FieldInfo[] result = new FieldInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		public override PropertyInfo[] GetProperties (BindingFlags bf)
		{
			if (!generic_type.IsCompilerContext)
				throw new NotSupportedException ();

			ArrayList l = new ArrayList ();

			Type current_type = this;
			do {
				MonoGenericClass gi = current_type as MonoGenericClass;
				if (gi != null)
					l.AddRange (gi.GetPropertiesInternal (bf, this));
				else if (current_type is TypeBuilder)
					l.AddRange (current_type.GetProperties (bf));
				else {
					MonoType mt = (MonoType) current_type;
					l.AddRange (mt.GetPropertiesByName (null, bf, false, this));
					break;
				}

				if ((bf & BindingFlags.DeclaredOnly) != 0)
					break;
				current_type = current_type.BaseType;
			} while (current_type != null);

			PropertyInfo[] result = new PropertyInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		PropertyInfo[] GetPropertiesInternal (BindingFlags bf, MonoGenericClass reftype)
		{
			if (generic_type.properties == null)
				return new PropertyInfo [0];

			ArrayList l = new ArrayList ();
			bool match;
			MethodAttributes mattrs;
			MethodInfo accessor;

			initialize ();

			foreach (PropertyInfo pinfo in generic_type.properties) {
				match = false;
				accessor = pinfo.GetGetMethod (true);
				if (accessor == null)
					accessor = pinfo.GetSetMethod (true);
				if (accessor == null)
					continue;
				mattrs = accessor.Attributes;
				if ((mattrs & MethodAttributes.MemberAccessMask) == MethodAttributes.Public) {
					if ((bf & BindingFlags.Public) != 0)
						match = true;
				} else {
					if ((bf & BindingFlags.NonPublic) != 0)
						match = true;
				}
				if (!match)
					continue;
				match = false;
				if ((mattrs & MethodAttributes.Static) != 0) {
					if ((bf & BindingFlags.Static) != 0)
						match = true;
				} else {
					if ((bf & BindingFlags.Instance) != 0)
						match = true;
				}
				if (!match)
					continue;
				l.Add (new PropertyOnTypeBuilderInst (reftype, pinfo));
			}
			PropertyInfo[] result = new PropertyInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		public override EventInfo[] GetEvents (BindingFlags bf)
		{
			if (!generic_type.IsCompilerContext)
				throw new NotSupportedException ();

			ArrayList l = new ArrayList ();

			Type current_type = this;
			do {
				MonoGenericClass gi = current_type as MonoGenericClass;
				if (gi != null)
					l.AddRange (gi.GetEventsInternal (bf, this));
				else if (current_type is TypeBuilder)
					l.AddRange (current_type.GetEvents (bf));
				else {
					MonoType mt = (MonoType) current_type;
					l.AddRange (mt.GetEvents (bf));
					break;
				}

				if ((bf & BindingFlags.DeclaredOnly) != 0)
					break;
				current_type = current_type.BaseType;
			} while (current_type != null);

			EventInfo[] result = new EventInfo [l.Count];
			l.CopyTo (result);
			return result;
		}
	
		EventInfo[] GetEventsInternal (BindingFlags bf, MonoGenericClass reftype) {
			if (generic_type.events == null)
				return new EventInfo [0];

			initialize ();

			ArrayList l = new ArrayList ();
			bool match;
			MethodAttributes mattrs;
			MethodInfo accessor;

			for (int i = 0; i < event_count; ++i) {
				EventBuilder ev = generic_type.events [i];

				match = false;
				accessor = ev.add_method;
				if (accessor == null)
					accessor = ev.remove_method;
				if (accessor == null)
					continue;
				mattrs = accessor.Attributes;
				if ((mattrs & MethodAttributes.MemberAccessMask) == MethodAttributes.Public) {
					if ((bf & BindingFlags.Public) != 0)
						match = true;
				} else {
					if ((bf & BindingFlags.NonPublic) != 0)
						match = true;
				}
				if (!match)
					continue;
				match = false;
				if ((mattrs & MethodAttributes.Static) != 0) {
					if ((bf & BindingFlags.Static) != 0)
						match = true;
				} else {
					if ((bf & BindingFlags.Instance) != 0)
						match = true;
				}
				if (!match)
					continue;
				l.Add (new EventOnTypeBuilderInst (reftype, ev));
			}
			EventInfo[] result = new EventInfo [l.Count];
			l.CopyTo (result);
			return result;
		}

		public override Type[] GetNestedTypes (BindingFlags bf)
		{
			return generic_type.GetNestedTypes (bf);
		}

		public override bool IsAssignableFrom (Type c)
		{
			if (c == this)
				return true;

			Type[] interfaces = GetInterfacesInternal ();

			if (c.IsInterface) {
				if (interfaces == null)
					return false;
				foreach (Type t in interfaces)
					if (c.IsAssignableFrom (t))
						return true;
				return false;
			}

			Type parent = GetParentType ();
			if (parent == null)
				return c == typeof (object);
			else
				return c.IsAssignableFrom (parent);
		}
	}
}

#endif
