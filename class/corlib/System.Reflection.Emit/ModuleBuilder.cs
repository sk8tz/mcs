
//
// System.Reflection.Emit/ModuleBuilder.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Diagnostics.SymbolStore;
using System.IO;

namespace System.Reflection.Emit {
	public class ModuleBuilder : Module {
		private TypeBuilder[] types;
		private CustomAttributeBuilder[] cattrs;
		private byte[] guid;
		private int table_idx;
		internal AssemblyBuilder assemblyb;
		internal ISymbolWriter symbol_writer;
		Hashtable name_cache;

		internal ModuleBuilder (AssemblyBuilder assb, string name, string fullyqname, bool emitSymbolInfo) {
			this.name = this.scopename = name;
			this.fqname = fullyqname;
			this.assembly = this.assemblyb = assb;
			guid = Guid.NewGuid().ToByteArray ();
			table_idx = get_next_table_index (this, 0x00, true);
			name_cache = new Hashtable ();

			if (emitSymbolInfo)
				GetSymbolWriter (fullyqname);
		}

		internal void GetSymbolWriter (string filename)
		{
			Assembly assembly;
			try {
				assembly = Assembly.Load ("Mono.CSharp.Debugger");
			} catch (FileNotFoundException) {
				return;
			}

			Type type = assembly.GetType ("Mono.CSharp.Debugger.MonoSymbolWriter");
			if (type == null)
				return;

			if (assemblyb.methods == null)
				assemblyb.methods = new ArrayList ();

			// First get the constructor.
			{
				Type[] arg_types = new Type [3];
				arg_types [0] = typeof (ModuleBuilder);
				arg_types [1] = typeof (string);
				arg_types [2] = typeof (ArrayList);
				ConstructorInfo constructor = type.GetConstructor (arg_types);

				object[] args = new object [3];
				args [0] = this;
				args [1] = filename;
				args [2] = assemblyb.methods;

				if (constructor == null)
					return;

				Object instance = constructor.Invoke (args);
				if (instance == null)
					return;

				if (!(instance is ISymbolWriter))
					return;

				symbol_writer = (ISymbolWriter) instance;
			}
		}

		public override string FullyQualifiedName {get { return fqname;}}

		[MonoTODO]
		public TypeBuilder DefineType (string name) {
			// FIXME: LAMESPEC: what other attributes should we use here as default?
			return DefineType (name, TypeAttributes.Public, typeof(object), null);
		}

		public TypeBuilder DefineType (string name, TypeAttributes attr) {
			return DefineType (name, attr, typeof(object), null);
		}

		public TypeBuilder DefineType (string name, TypeAttributes attr, Type parent) {
			return DefineType (name, attr, parent, null);
		}

		private TypeBuilder DefineType (string name, TypeAttributes attr, Type parent, Type[] interfaces, PackingSize packsize, int typesize) {
			TypeBuilder res = new TypeBuilder (this, name, attr, parent, interfaces, packsize, typesize);
			if (types != null) {
				TypeBuilder[] new_types = new TypeBuilder [types.Length + 1];
				System.Array.Copy (types, new_types, types.Length);
				new_types [types.Length] = res;
				types = new_types;
			} else {
				types = new TypeBuilder [1];
				types [0] = res;
			}
			name_cache.Add (name, res);
			return res;
		}

		public TypeBuilder DefineType (string name, TypeAttributes attr, Type parent, Type[] interfaces) {
			return DefineType (name, attr, parent, interfaces, PackingSize.Unspecified, TypeBuilder.UnspecifiedTypeSize);
		}

		public TypeBuilder DefineType (string name, TypeAttributes attr, Type parent, int typesize) {
			return DefineType (name, attr, parent, null, PackingSize.Unspecified, TypeBuilder.UnspecifiedTypeSize);
		}

		public TypeBuilder DefineType (string name, TypeAttributes attr, Type parent, PackingSize packsize) {
			return DefineType (name, attr, parent, null, packsize, TypeBuilder.UnspecifiedTypeSize);
		}

		public TypeBuilder DefineType (string name, TypeAttributes attr, Type parent, PackingSize packsize, int typesize) {
			return DefineType (name, attr, parent, null, packsize, typesize);
		}

		public MethodInfo GetArrayMethod( Type arrayClass, string methodName, CallingConventions callingConvention, Type returnType, Type[] parameterTypes) {
			return new MonoArrayMethod (arrayClass, methodName, callingConvention, returnType, parameterTypes);
		}

		public EnumBuilder DefineEnum( string name, TypeAttributes visibility, Type underlyingType) {
			EnumBuilder eb = new EnumBuilder (this, name, visibility, underlyingType);
			return eb;
		}

		public override Type GetType( string className) {
			return GetType (className, false, false);
		}
		
		public override Type GetType( string className, bool ignoreCase) {
			return GetType (className, false, ignoreCase);
		}

		private TypeBuilder search_in_array (TypeBuilder[] arr, string className, bool ignoreCase) {
			int i;
			if (arr == types && !ignoreCase)
				return (TypeBuilder)name_cache [className];
			for (i = 0; i < arr.Length; ++i) {
				if (String.Compare (className, arr [i].FullName, ignoreCase) == 0) {
					return arr [i];
				}
			}
			return null;
		}

		private TypeBuilder search_nested_in_array (TypeBuilder[] arr, string className, bool ignoreCase) {
			int i;
			for (i = 0; i < arr.Length; ++i) {
				if (String.Compare (className, arr [i].Name, ignoreCase) == 0)
					return arr [i];
			}
			return null;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern Type create_modified_type (TypeBuilder tb, string modifiers);

		static char[] type_modifiers = {'&', '[', '*'};

		private TypeBuilder GetMaybeNested (TypeBuilder t, string className, bool ignoreCase) {
			int subt;
			string pname, rname;

			subt = className.IndexOf ('+');
			if (subt < 0) {
				if (t.subtypes != null)
					return search_nested_in_array (t.subtypes, className, ignoreCase);
				return null;
			}
			if (t.subtypes != null) {
				pname = className.Substring (0, subt);
				rname = className.Substring (subt + 1);
				TypeBuilder result = search_nested_in_array (t.subtypes, pname, ignoreCase);
				if (result != null)
					return GetMaybeNested (result, rname, ignoreCase);
			}
			return null;
		}
		
		public override Type GetType( string className, bool throwOnError, bool ignoreCase) {
			int subt;
			string orig = className;
			string modifiers;
			TypeBuilder result = null;

			if (types == null && throwOnError)
				throw new TypeLoadException (className);

			subt = className.IndexOfAny (type_modifiers);
			if (subt >= 0) {
				modifiers = className.Substring (subt);
				className = className.Substring (0, subt);
			} else
				modifiers = null;
			
			subt = className.IndexOf ('+');
			if (subt < 0) {
				if (types != null)
					result = search_in_array (types, className, ignoreCase);
			} else {
				string pname, rname;
				pname = className.Substring (0, subt);
				rname = className.Substring (subt + 1);
				result = search_in_array (types, pname, ignoreCase);
				if (result != null)
					result = GetMaybeNested (result, rname, ignoreCase);
			}
			if ((result == null) && throwOnError)
				throw new TypeLoadException (orig);
			if (result != null && (modifiers != null))
				return create_modified_type (result, modifiers);
			return result;
		}

		internal int get_next_table_index (object obj, int table, bool inc) {
			return assemblyb.get_next_table_index (obj, table, inc);
		}

		public void SetCustomAttribute( CustomAttributeBuilder customBuilder) {
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
		public void SetCustomAttribute( ConstructorInfo con, byte[] binaryAttribute) {
			SetCustomAttribute (new CustomAttributeBuilder (con, binaryAttribute));
		}

		public ISymbolWriter GetSymWriter () {
			return symbol_writer;
		}

		public ISymbolDocumentWriter DefineDocument (string url, Guid language, Guid languageVendor, Guid documentType) {
			if (symbol_writer == null)
				throw new InvalidOperationException ();

			return symbol_writer.DefineDocument (url, language, languageVendor, documentType);
		}

		public override Type [] GetTypes ()
		{
			if (types == null)
				return new TypeBuilder [0];

			int n = types.Length;
			TypeBuilder [] copy = new TypeBuilder [n];
			Array.Copy (types, copy, n);

			return copy;
		}
	}
}
