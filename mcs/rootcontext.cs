//
// rootcontext.cs: keeps track of our tree representation, and assemblies loaded.
//
// Author: Miguel de Icaza (miguel@ximian.com)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;

namespace CIR {

	public class RootContext {

		//
		// Contains the parsed tree
		//
		Tree tree;

		//
		// Contains loaded assemblies and our generated code as we go.
		//
		TypeManager type_manager;

		//
		// The System.Reflection.Emit CodeGenerator
		//
		CilCodeGen cg;

		//
		// The module builder pointer
		//
		ModuleBuilder mb;

		//
		// Error reporting object
		// 
		Report report;

		//
		// The `System.Object' and `System.ValueType' types, as they
		// are used often
		//
		Type object_type;
		Type value_type;

		//
		// Whether we are being linked against the standard libraries.
		// This is only used to tell whether `System.Object' should
		// have a parent or not.
		//
		bool stdlib = true;
		
		public RootContext ()
		{
			tree = new Tree ();
			type_manager = new TypeManager ();
			report = new Report ();

			object_type = System.Type.GetType ("System.Object");
			value_type = System.Type.GetType ("System.ValueType");
		}

		public TypeManager TypeManager {
			get {
				return type_manager;
			}
		}

		public Tree Tree {
			get {
				return tree;
			}
		}

		public CilCodeGen CodeGen {
			get {
				return cg;
			}

			set {
				//
				// Temporary hack, we should probably
				// intialize `cg' rather than depending on
				// external initialization of it.
				//
				cg = value;
				mb = cg.ModuleBuilder;
			}
		}

		//
		// Returns the Type that represents the interface whose name
		// is `name'.
		//
		
		Type GetInterfaceTypeByName (string name)
		{
			Interface parent;
			Type t = type_manager.LookupType (name);

			if (t != null) {

				if (t.IsInterface)
					return t;
				
				string cause;
				
				if (t.IsValueType)
					cause = "is a struct";
				else if (t.IsClass) 
					cause = "is a class";
				else
					cause = "Should not happen.";

				report.Error (527, "`"+name+"' " + cause + ", need an interface instead");
				
				return null;
			}

			parent = (Interface) tree.Interfaces [name];
			if (parent == null){
				string cause = "is undefined";
				
				if (tree.Classes [name] != null)
					cause = "is a class";
				else if (tree.Structs [name] != null)
					cause = "is a struct";
				
				report.Error (527, "`"+name+"' " + cause + ", need an interface instead");
				return null;
			}
			
			t = CreateInterface ((Interface) parent);
			if (t == null){
				report.Error (529,
					      "Inherited interface `"+name+"' is circular");
				return null;
			}

			return t;
		}
		
		//
		// Returns the list of interfaces that this interface implements
		// Or null if it does not implement any interface.
		//
		// Sets the error boolean accoringly.
		//
		Type [] GetInterfaceBases (Interface iface, out bool error)
		{
			ArrayList bases = iface.Bases;
			Type [] tbases;
			int i;

			error = false;
			if (bases == null)
				return null;
			
			tbases = new Type [bases.Count];
			i = 0;

			foreach (string name in iface.Bases){
				Type t;

				t = GetInterfaceTypeByName (name);
				if (t == null){
					error = true;
					return null;
				}
				
				tbases [i++] = t;
			}

			return tbases;
		}
		
		//
		// Creates the Interface @iface using the ModuleBuilder
		//
		// TODO:
		//   Rework the way we recurse, because for recursive
		//   definitions of interfaces (A:B and B:A) we report the
		//   error twice, rather than once.  
		//
		TypeBuilder CreateInterface (Interface iface)
		{
			TypeBuilder tb = iface.Definition;
			Type [] ifaces;
			string name;
			bool error;

			if (tb != null)
				return tb;
			
			if (iface.InTransit)
				return null;
			
			iface.InTransit = true;

			name = iface.Name;

			ifaces = GetInterfaceBases (iface, out error);

			if (error)
				return null;

			tb = mb.DefineType (name,
					    TypeAttributes.Interface |
					    TypeAttributes.Public |
					    TypeAttributes.Abstract,
					    null,   // Parent Type
					    ifaces);
			iface.Definition = tb;

			type_manager.AddUserType (name, tb);

			iface.InTransit = false;
			return tb;
		}

		//
		// Returns the type for an interface or a class
		//
		Type GetInterfaceOrClass (string name, bool is_class)
		{
			Type t = type_manager.LookupType (name);
			Class parent;

			if (t != null)
				return t;

			parent = (Class) tree.Classes [name];
			if (parent == null){
				report.Error (246, "Can not find type `"+name+"'");
				return null;
			}

			t = CreateType ((Class) parent, is_class);
			if (t == null){
				report.Error (146, "Class definition is circular: `"+name+"'");
				return null;
			}

			return t;
		}

		//
		// This function computes the Base class and also the
		// list of interfaces that the class or struct @c implements.
		//
		// The return value is an array (might be null) of
		// interfaces implemented (as Types).
		//
		// The @parent argument is set to the parent object or null
		// if this is `System.Object'. 
		//
		Type [] GetClassBases (TypeContainer tc, bool is_class, out Type parent, out bool error)
		{
			ArrayList bases = tc.Bases;
			int count;
			int start, j, i;
			
			error = false;

			if (is_class)
				parent = null;
			else
				parent = value_type;

			if (bases == null){
				if (is_class){
					if (stdlib)
						parent = object_type;
					else if (tc.Name != "System.Object")
						parent = object_type;
				} else {
					//
					// If we are compiling our runtime,
					// and we are defining ValueType, then our
					// parent is `System.Object'.
					//
					if (!stdlib && tc. Name == "System.ValueType")
						parent = object_type;
				}

				return null;
			}

			//
			// Bases should be null if there are no bases at all
			//
			count = bases.Count;
			Debug.Assert (count > 0);

			if (is_class){
				string name = (string) bases [0];
				Type first = GetInterfaceOrClass (name, is_class);

				if (first.IsClass){
					parent = first;
					start = 1;
				} else {
					parent = object_type;
					start = 0;
				}
			} else {
				start = 0;
			}

			Type [] ifaces = new Type [count-start];
			
			for (i = start, j = 0; i < count; i++, j++){
				string name = (string) bases [i];
				Type t = GetInterfaceOrClass (name, is_class);

				if (t == null){
					error = true;
					return null;
				}

				if (is_class == false && !t.IsInterface){
					report.Error (527, "In Struct `"+tc.Name+"', type `"+
						      name+"' is not an interface");
					error = true;
					return null;
				}
				
				if (t.IsSealed) {
					string detail = "";
					
					if (t.IsValueType)
						detail = " (a class can not inherit from a struct)";
							
					report.Error (509, "class `"+tc.Name+
						      "': Cannot inherit from sealed class `"+
						      bases [i]+"'"+detail);
					error = true;
					return null;
				}

				if (t.IsClass) {
					if (parent != null){
						report.Error (527, "In Class `"+tc.Name+"', type `"+
							      name+"' is not an interface");
						error = true;
						return null;
					}
				}
				
				ifaces [j] = t;
			}

			return ifaces;
		}

		// <remarks>
		//   Creates the TypeBuilder for the TypeContainer @tc (a Class or a Struct)
		// </remarks>
		//
		TypeBuilder CreateType (TypeContainer tc, bool is_class)
		{
			TypeBuilder tb = tc.Definition;
			Type parent;
			Type [] ifaces;
			bool error;
			string name;
			
			if (tb != null)
				return tb;

			if (tc.InTransit)
				return null;
			tc.InTransit = true;

			name = tc.Name;

			ifaces = GetClassBases (tc, is_class, out parent, out error); 

			if (error)
				return null;

			tb = mb.DefineType (name,
					    tc.TypeAttr | TypeAttributes.Class,
					    parent,
					    ifaces);

			tc.Definition = tb;
			type_manager.AddUserType (name, tb);
			tc.InTransit = false;
			
			return tb;
		}

		// <remarks>
		//   This function is used to resolve the hierarchy tree.
		//   It processes interfaces, structs and classes in that order.
		//
		//   It creates the TypeBuilder's as it processes the user defined
		//   types.  
		// </remarks>
		public void ResolveTree ()
		{
			Hashtable ifaces, classes, structs;

			//
			// Interfaces are processed first, as classes and
			// structs might inherit from an object or implement
			// a set of interfaces, we need to be able to tell
			// them appart by just using the TypeManager.
			//
			ifaces = tree.Interfaces;
			if (ifaces != null){
				foreach (DictionaryEntry de in ifaces)
					CreateInterface ((Interface) de.Value);
			}

			//
			// Process structs and classes next.  Our code assumes
			// this order (just for error reporting purposes).
			//
			structs = tree.Structs;
			if (structs != null){
				foreach (DictionaryEntry de in structs)
					CreateType ((Struct) de.Value, false);
			}

			classes = tree.Classes;
			if (classes != null){
				foreach (DictionaryEntry de in classes)
					CreateType ((Class) de.Value, true);
			}
		}
			
		// <summary>
		//   Closes all open types
		// </summary>
		//
		// <remarks>
		//   We usually use TypeBuilder types.  When we are done
		//   creating the type (which will happen after we have addded
		//   methods, fields, etc) we need to "Define" them before we
		//   can save the Assembly
		// </remarks>
		public void CloseTypes ()
		{
			foreach (TypeBuilder t in type_manager.UserTypes){
				t.CreateType ();
			}
		}

		// <summary>
		//   Compiling against Standard Libraries property.
		// </summary>
		public bool StdLib {
			get {
				return stdlib;
			}

			set {
				stdlib = value;
			}
		}

		public Report Report {
			get {
				return report;
			}
		}
	}
}
	      
