//
// attribute.cs: Attribute Handler
//
// Author: Ravi Pratap (ravi@ximian.com)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//
//

using System;
using System.Diagnostics;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;

namespace Mono.CSharp {

	public class Attribute {
		public readonly string    Name;
		public readonly ArrayList Arguments;

		Location Location;

		public Type Type;
		
		//
		// The following are only meaningful when the attribute
		// being emitted is one of the builtin ones
		//
		AttributeTargets Targets;
		bool AllowMultiple;
		bool Inherited;

		bool UsageAttr = false;
		
		MethodImplOptions ImplOptions;
		UnmanagedType     UnmanagedType;
		CustomAttributeBuilder cb;
	
		/* non-null if named args present after Resolve () is called */
		PropertyInfo [] prop_info_arr;
		FieldInfo [] field_info_arr;
		object [] field_values_arr;
		object [] prop_values_arr;
		
		public Attribute (string name, ArrayList args, Location loc)
		{
			Name = name;
			Arguments = args;
			Location = loc;
		}

		void Error_InvalidNamedArgument (string name)
		{
			Report.Error (617, Location, "'" + name + "' is not a valid named attribute " +
				      "argument. Named attribute arguments must be fields which are not " +
				      "readonly, static or const, or properties with a set accessor which "+
				      "are not static.");
		}

		static void Error_AttributeArgumentNotValid (Location loc)
		{
			Report.Error (182, loc,
				      "An attribute argument must be a constant expression, typeof " +
				      "expression or array creation expression");
		}

		static void Error_AttributeConstructorMismatch (Location loc)
		{
			Report.Error (-6, loc,
                                      "Could not find a constructor for this argument list.");
		}
		
		private Type CheckAttributeType (EmitContext ec) {
			Type t;
			bool isattributeclass = true;
			
			t = RootContext.LookupType (ec.DeclSpace, Name, true, Location);
			if (t != null) {
				isattributeclass = t.IsSubclassOf (TypeManager.attribute_type);
				if (isattributeclass)
					return t;
			}
			t = RootContext.LookupType (ec.DeclSpace, Name + "Attribute", true, Location);
			if (t != null) {
				if (t.IsSubclassOf (TypeManager.attribute_type))
					return t;
			}
			if (!isattributeclass) {
				Report.Error (616, Location, "'" + Name + "': is not an attribute class");
				return null;
			}
			if (t != null) {
				Report.Error (616, Location, "'" + Name + "Attribute': is not an attribute class");
				return null;
			}
			Report.Error (
				246, Location, "Could not find attribute '" + Name + "' (are you" +
				" missing a using directive or an assembly reference ?)");
			return null;
		}

		public Type ResolveType (EmitContext ec)
		{
			Type = CheckAttributeType (ec);
			return Type;
		}

		/// <summary>
		///   Validates the guid string
		/// </summary>
		bool ValidateGuid (string guid)
		{
			try {
				new Guid (guid);
				return true;
			} catch {
				Report.Error (647, Location, "Format of GUID is invalid: " + guid);
				return false;
			}
		}

		//
		// Given an expression, if the expression is a valid attribute-argument-expression
		// returns an object that can be used to encode it, or null on failure.
		//
		public static bool GetAttributeArgumentExpression (Expression e, Location loc, out object result)
		{
			if (e is Constant) {
				result = ((Constant) e).GetValue ();
				return true;
			} else if (e is TypeOf) {
				result = ((TypeOf) e).TypeArg;
				return true;
			} else if (e is ArrayCreation){
				result =  ((ArrayCreation) e).EncodeAsAttribute ();
				if (result != null)
					return true;
			}

			result = null;
			Error_AttributeArgumentNotValid (loc);
			return false;
		}
		
		public CustomAttributeBuilder Resolve (EmitContext ec)
		{
			if (Type == null)
				Type = CheckAttributeType (ec);
			if (Type == null)
				return null;

			bool MethodImplAttr = false;
			bool MarshalAsAttr = false;
			bool GuidAttr = false;
			UsageAttr = false;

			bool DoCompares = true;
			if (Type == TypeManager.attribute_usage_type)
				UsageAttr = true;
			else if (Type == TypeManager.methodimpl_attr_type)
				MethodImplAttr = true;
			else if (Type == TypeManager.marshal_as_attr_type)
				MarshalAsAttr = true;
			else if (Type == TypeManager.guid_attr_type)
				GuidAttr = true;
			else
				DoCompares = false;

			// Now we extract the positional and named arguments
			
			ArrayList pos_args = new ArrayList ();
			ArrayList named_args = new ArrayList ();
			int pos_arg_count = 0;
			
			if (Arguments != null) {
				pos_args = (ArrayList) Arguments [0];
				if (pos_args != null)
					pos_arg_count = pos_args.Count;
				if (Arguments.Count > 1)
					named_args = (ArrayList) Arguments [1];
			}

			object [] pos_values = new object [pos_arg_count];

			//
			// First process positional arguments 
			//

			int i;
			for (i = 0; i < pos_arg_count; i++) {
				Argument a = (Argument) pos_args [i];
				Expression e;

				if (!a.Resolve (ec, Location))
					return null;

				e = a.Expr;

				object val;
				if (!GetAttributeArgumentExpression (e, Location, out val))
					return null;
				
				pos_values [i] = val;
				if (DoCompares){
					if (UsageAttr)
						this.Targets = (AttributeTargets) pos_values [0];
					else if (MethodImplAttr)
						this.ImplOptions = (MethodImplOptions) pos_values [0];
					else if (GuidAttr){
						//
						// we will later check the validity of the type
						//
						if (pos_values [0] is string){
							if (!ValidateGuid ((string) pos_values [0]))
								return null;
						}
						
					} else if (MarshalAsAttr)
						this.UnmanagedType =
						(System.Runtime.InteropServices.UnmanagedType) pos_values [0];
				}
			}

			//
			// Now process named arguments
			//

			ArrayList field_infos = null;
			ArrayList prop_infos  = null;
			ArrayList field_values = null;
			ArrayList prop_values = null;

			if (named_args.Count > 0) {
				field_infos = new ArrayList ();
				prop_infos  = new ArrayList ();
				field_values = new ArrayList ();
				prop_values = new ArrayList ();
			}
			
			for (i = 0; i < named_args.Count; i++) {
				DictionaryEntry de = (DictionaryEntry) named_args [i];
				string member_name = (string) de.Key;
				Argument a  = (Argument) de.Value;
				Expression e;
				
				if (!a.Resolve (ec, Location))
					return null;

				Expression member = Expression.MemberLookup (
					ec, Type, member_name,
					MemberTypes.Field | MemberTypes.Property,
					BindingFlags.Public | BindingFlags.Instance,
					Location);

				if (member == null || !(member is PropertyExpr || member is FieldExpr)) {
					Error_InvalidNamedArgument (member_name);
					return null;
				}

				e = a.Expr;
				if (member is PropertyExpr) {
					PropertyExpr pe = (PropertyExpr) member;
					PropertyInfo pi = pe.PropertyInfo;

					if (!pi.CanWrite) {
						Error_InvalidNamedArgument (member_name);
						return null;
					}

					if (e is Constant) {
						object o = ((Constant) e).GetValue ();
						prop_values.Add (o);
						
						if (UsageAttr) {
							if (member_name == "AllowMultiple")
								this.AllowMultiple = (bool) o;
							if (member_name == "Inherited")
								this.Inherited = (bool) o;
						}
						
					} else if (e is TypeOf) {
						prop_values.Add (((TypeOf) e).TypeArg);
					} else {
						Error_AttributeArgumentNotValid (Location);
						return null;
					}
					
					prop_infos.Add (pi);
					
				} else if (member is FieldExpr) {
					FieldExpr fe = (FieldExpr) member;
					FieldInfo fi = fe.FieldInfo;

					if (fi.IsInitOnly) {
						Error_InvalidNamedArgument (member_name);
						return null;
					}

					//
					// Handle charset here, and set the TypeAttributes
					
					if (e is Constant){
						object value = ((Constant) e).GetValue ();
						
						field_values.Add (value);
					} else if (e is TypeOf) {
						field_values.Add (((TypeOf) e).TypeArg);
					} else {
						Error_AttributeArgumentNotValid (Location);
						return null;
					}
					
					field_infos.Add (fi);
				}
			}

			Expression mg = Expression.MemberLookup (
				ec, Type, ".ctor", MemberTypes.Constructor,
				BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
                                Location);

			if (mg == null) {
                                Console.WriteLine ("Came here");
				Error_AttributeConstructorMismatch (Location);
				return null;
			}

			MethodBase constructor = Invocation.OverloadResolve (
				ec, (MethodGroupExpr) mg, pos_args, Location);

			if (constructor == null) {
				Error_AttributeConstructorMismatch (Location);
				return null;
			}

			//
			// Now we perform some checks on the positional args as they
			// cannot be null for a constructor which expects a parameter
			// of type object
			//

			ParameterData pd = Invocation.GetParameterData (constructor);

			int group_in_params_array = Int32.MaxValue;
			int pc = pd.Count;
			if (pc > 0 && pd.ParameterModifier (pc-1) == Parameter.Modifier.PARAMS)
				group_in_params_array = pc-1;
			
			for (int j = 0; j < pos_arg_count; ++j) {
				Argument a = (Argument) pos_args [j];
				
				if (a.Expr is NullLiteral && pd.ParameterType (j) == TypeManager.object_type) {
					Error_AttributeArgumentNotValid (Location);
					return null;
				}

				if (j < group_in_params_array)
					continue;
				
				if (j == group_in_params_array){
					object v = pos_values [j];
					int count = pos_arg_count - j;

					object [] array = new object [count];
					pos_values [j] = array;
					array [0] = v;
				} else {
					object [] array = (object []) pos_values [group_in_params_array];

					array [j - group_in_params_array] = pos_values [j];
				}
			}

			//
			// Adjust the size of the pos_values if it had params
			//
			if (group_in_params_array != Int32.MaxValue){
				int argc = group_in_params_array+1;
				object [] new_pos_values = new object [argc];

				for (int p = 0; p < argc; p++)
					new_pos_values [p] = pos_values [p];
				pos_values = new_pos_values;
			}

			try {
				if (named_args.Count > 0) {
					prop_info_arr = new PropertyInfo [prop_infos.Count];
					field_info_arr = new FieldInfo [field_infos.Count];
					field_values_arr = new object [field_values.Count];
					prop_values_arr = new object [prop_values.Count];

					field_infos.CopyTo  (field_info_arr, 0);
					field_values.CopyTo (field_values_arr, 0);

					prop_values.CopyTo  (prop_values_arr, 0);
					prop_infos.CopyTo   (prop_info_arr, 0);

					cb = new CustomAttributeBuilder (
						(ConstructorInfo) constructor, pos_values,
						prop_info_arr, prop_values_arr,
						field_info_arr, field_values_arr);
				}
				else
					cb = new CustomAttributeBuilder (
						(ConstructorInfo) constructor, pos_values);
			} catch (NullReferenceException) {
				// 
				// Don't know what to do here
				//
				Report.Warning (
				        -100, Location, "NullReferenceException while trying to create attribute. Something's wrong!");
			} catch (Exception e) {
				//
				// Sample:
				// using System.ComponentModel;
				// [DefaultValue (CollectionChangeAction.Add)]
				// class X { static void Main () {} }
				//
				Report.Warning (
					-23, Location,
					"The compiler can not encode this attribute in .NET due to\n" +
					"\ta bug in the .NET runtime.  Try the Mono runtime.\nThe error was: " + e.Message);
			}
			
			return cb;
		}

		static string GetValidPlaces (Attribute attr)
		{
			StringBuilder sb = new StringBuilder ();
			AttributeTargets targets = 0;
			
			TypeContainer a = TypeManager.LookupAttr (attr.Type);

			if (a == null) {
				
				System.Attribute [] attrs = null;
				
				try {
					attrs = System.Attribute.GetCustomAttributes (attr.Type);
					
				} catch {
					Report.Error (-20, attr.Location, "Cannot find attribute type " + attr.Name +
						      " (maybe you forgot to set the usage using the" +
						      " AttributeUsage attribute ?).");
					return null;
				}
					
				foreach (System.Attribute tmp in attrs)
					if (tmp is AttributeUsageAttribute) {
						targets = ((AttributeUsageAttribute) tmp).ValidOn;
						break;
					}
			} else
				targets = a.Targets;

			
			if ((targets & AttributeTargets.Assembly) != 0)
				sb.Append ("'assembly' ");

			if ((targets & AttributeTargets.Class) != 0)
				sb.Append ("'class' ");

			if ((targets & AttributeTargets.Constructor) != 0)
				sb.Append ("'constructor' ");

			if ((targets & AttributeTargets.Delegate) != 0)
				sb.Append ("'delegate' ");

			if ((targets & AttributeTargets.Enum) != 0)
				sb.Append ("'enum' ");

			if ((targets & AttributeTargets.Event) != 0)
				sb.Append ("'event' ");

			if ((targets & AttributeTargets.Field) != 0)
				sb.Append ("'field' ");

			if ((targets & AttributeTargets.Interface) != 0)
				sb.Append ("'interface' ");

			if ((targets & AttributeTargets.Method) != 0)
				sb.Append ("'method' ");

			if ((targets & AttributeTargets.Module) != 0)
				sb.Append ("'module' ");

			if ((targets & AttributeTargets.Parameter) != 0)
				sb.Append ("'parameter' ");

			if ((targets & AttributeTargets.Property) != 0)
				sb.Append ("'property' ");

			if ((targets & AttributeTargets.ReturnValue) != 0)
				sb.Append ("'return value' ");

			if ((targets & AttributeTargets.Struct) != 0)
				sb.Append ("'struct' ");

			return sb.ToString ();

		}

		public static void Error_AttributeNotValidForElement (Attribute a, Location loc)
		{
			Report.Error (
				592, loc, "Attribute '" + a.Name +
				"' is not valid on this declaration type. " +
				"It is valid on " + GetValidPlaces (a) + "declarations only.");
		}

		public static bool CheckAttribute (Attribute a, object element)
		{
			TypeContainer attr = TypeManager.LookupAttr (a.Type);
			AttributeTargets targets = 0;

			
			if (attr == null) {
				System.Attribute [] attrs = null;
				
				try {
					attrs = System.Attribute.GetCustomAttributes (a.Type);

				} catch {
					Report.Error (-20, a.Location, "Cannot find attribute type " + a.Name +
						      " (maybe you forgot to set the usage using the" +
						      " AttributeUsage attribute ?).");
					return false;
				}
					
				foreach (System.Attribute tmp in attrs)
					if (tmp is AttributeUsageAttribute) 
						targets = ((AttributeUsageAttribute) tmp).ValidOn;
			} else
				targets = attr.Targets;

			if (element is Class) {
				if ((targets & AttributeTargets.Class) != 0)
					return true;
				else
					return false;
				
			} else if (element is Struct) {
				if ((targets & AttributeTargets.Struct) != 0)
					return true;
				else
					return false;
			} else if (element is Constructor) {
				if ((targets & AttributeTargets.Constructor) != 0)
					return true;
				else
					return false;
			} else if (element is Delegate) {
				if ((targets & AttributeTargets.Delegate) != 0)
					return true;
				else
					return false;
			} else if (element is Enum) {
				if ((targets & AttributeTargets.Enum) != 0)
					return true;
				else
					return false;
			} else if (element is Event || element is InterfaceEvent) {
				if ((targets & AttributeTargets.Event) != 0)
					return true;
				else
					return false;
			} else if (element is Field || element is FieldBuilder) {
				if ((targets & AttributeTargets.Field) != 0)
					return true;
				else
					return false;
			} else if (element is Interface) {
				if ((targets & AttributeTargets.Interface) != 0)
					return true;
				else
					return false;
			} else if (element is Method || element is Operator || element is InterfaceMethod || element is Accessor) {
				if ((targets & AttributeTargets.Method) != 0)
					return true;
				else
					return false;
			} else if (element is ParameterBuilder) {
				if ((targets & AttributeTargets.Parameter) != 0)
					return true;
				else
					return false;
			} else if (element is Property || element is Indexer ||
				   element is InterfaceProperty || element is InterfaceIndexer) {
				if ((targets & AttributeTargets.Property) != 0)
					return true;
				else
					return false;
			} else if (element is AssemblyBuilder){
				if ((targets & AttributeTargets.Assembly) != 0)
					return true;
				else
					return false;
			}

			return false;
		}

		//
		// This method should be invoked to pull the IndexerName attribute from an
		// Indexer if it exists.
		//
		public static string ScanForIndexerName (EmitContext ec, Attributes opt_attrs)
		{
			if (opt_attrs == null)
				return null;
			if (opt_attrs.AttributeSections == null)
				return null;

			foreach (AttributeSection asec in opt_attrs.AttributeSections) {
				if (asec.Attributes == null)
					continue;

				foreach (Attribute a in asec.Attributes){
					if (a.ResolveType (ec) == null)
						return null;
					
					if (a.Type != TypeManager.indexer_name_type)
						continue;

					//
					// So we have found an IndexerName, pull the data out.
					//
					if (a.Arguments == null || a.Arguments [0] == null){
						Error_AttributeConstructorMismatch (a.Location);
						return null;
					}
					ArrayList pos_args = (ArrayList) a.Arguments [0];
					if (pos_args.Count == 0){
						Error_AttributeConstructorMismatch (a.Location);
						return null;
					}
					
					Argument arg = (Argument) pos_args [0];
					if (!arg.Resolve (ec, a.Location))
						return null;
					
					Expression e = arg.Expr;
					if (!(e is StringConstant)){
						Error_AttributeConstructorMismatch (a.Location);
						return null;
					}

					//
					// Remove the attribute from the list
					//
					asec.Attributes.Remove (a);

					return (((StringConstant) e).Value);
				}
			}
			return null;
		}

		//
		// This pulls the condition name out of a Conditional attribute
		//
		public string Conditional_GetConditionName ()
		{
			//
			// So we have a Conditional, pull the data out.
			//
			if (Arguments == null || Arguments [0] == null){
				Error_AttributeConstructorMismatch (Location);
				return null;
			}

			ArrayList pos_args = (ArrayList) Arguments [0];
			if (pos_args.Count != 1){
				Error_AttributeConstructorMismatch (Location);
				return null;
			}

			Argument arg = (Argument) pos_args [0];	
			if (!(arg.Expr is StringConstant)){
				Error_AttributeConstructorMismatch (Location);
				return null;
			}

			return ((StringConstant) arg.Expr).Value;
		}

		//
		// This pulls the obsolete message and error flag out of an Obsolete attribute
		//
		public string Obsolete_GetObsoleteMessage (out bool is_error)
		{
			is_error = false;
			//
			// So we have an Obsolete, pull the data out.
			//
			if (Arguments == null || Arguments [0] == null)
				return "";

			ArrayList pos_args = (ArrayList) Arguments [0];
			if (pos_args.Count == 0)
				return "";
			else if (pos_args.Count > 2){
				Error_AttributeConstructorMismatch (Location);
				return null;
			}

			Argument arg = (Argument) pos_args [0];	
			if (!(arg.Expr is StringConstant)){
				Error_AttributeConstructorMismatch (Location);
				return null;
			}

			if (pos_args.Count == 2){
				Argument arg2 = (Argument) pos_args [1];
				if (!(arg2.Expr is BoolConstant)){
					Error_AttributeConstructorMismatch (Location);
					return null;
				}
				is_error = ((BoolConstant) arg2.Expr).Value;
			}

			return ((StringConstant) arg.Expr).Value;
		}

		static object GetFieldValue (Attribute a, string name) {
			int i;
			if (a.field_info_arr == null)
				return null;
			i = 0;
			foreach (FieldInfo fi in a.field_info_arr) {
				if (fi.Name == name)
					return a.field_values_arr [i];
				i++;
			}
			return null;
		}

		static UnmanagedMarshal GetMarshal (Attribute a) {
			UnmanagedMarshal marshal;

			if (a.UnmanagedType == UnmanagedType.CustomMarshaler) {
				MethodInfo define_custom = typeof (UnmanagedMarshal).GetMethod ("DefineCustom", BindingFlags.Static | BindingFlags.Public);
				if (define_custom == null) {
					return null;
				}
				object[] args = new object [4];
				args [0] = GetFieldValue (a, "MarshalTypeRef");
				args [1] = GetFieldValue (a, "MarshalCookie");
				args [2] = GetFieldValue (a, "MarshalType");
				args [3] = Guid.Empty;
				marshal = (UnmanagedMarshal) define_custom.Invoke (null, args);
			/*
			 * need to special case other special marshal types
			 */
			} else {
				marshal = UnmanagedMarshal.DefineUnmanagedMarshal (a.UnmanagedType);
			}
			return marshal;
		}

		//
		// Applies the attributes to the `builder'.
		//
		public static void ApplyAttributes (EmitContext ec, object builder, object kind,
						    Attributes opt_attrs)
		{
			Type attr_type = null;
			
			if (opt_attrs == null)
				return;
			if (opt_attrs.AttributeSections == null)
				return;

			ArrayList emitted_attrs = new ArrayList ();
			ArrayList emitted_targets = new ArrayList ();

			foreach (AttributeSection asec in opt_attrs.AttributeSections) {
				string attr_target = asec.Target;
				
				if (asec.Attributes == null)
					continue;

				if (attr_target == "assembly" && !(builder is AssemblyBuilder))
					continue;

				if (attr_target == "return" && !(builder is ParameterBuilder))
					continue;
				
				foreach (Attribute a in asec.Attributes) {
					Location loc = a.Location;
					CustomAttributeBuilder cb = a.Resolve (ec);
					attr_type = a.Type;

					if (cb == null) 
						continue;
					
					if (!(kind is TypeContainer))
						if (!CheckAttribute (a, kind)) {
							Error_AttributeNotValidForElement (a, loc);
							return;
						}

					//
					// Perform the check for duplicate attributes
					//
					if (emitted_attrs.Contains (attr_type) &&
					    emitted_targets.Contains (attr_target) &&
					    !TypeManager.AreMultipleAllowed (attr_type)) {
						Report.Error (579, loc, "Duplicate '" + a.Name + "' attribute");
						return;
					}

					if (kind is Method || kind is Operator || kind is InterfaceMethod ||
					    kind is Accessor) {
						if (attr_type == TypeManager.methodimpl_attr_type) {
							if (a.ImplOptions == MethodImplOptions.InternalCall)
								((MethodBuilder) builder).
								SetImplementationFlags (
									MethodImplAttributes.InternalCall |
									MethodImplAttributes.Runtime);
							else
								((MethodBuilder) builder).SetCustomAttribute (cb);
						} else if (attr_type != TypeManager.dllimport_type){
							((MethodBuilder) builder).SetCustomAttribute (cb);
						}
					} else if (kind is Constructor) {
						((ConstructorBuilder) builder).SetCustomAttribute (cb);
					} else if (kind is Field) {
						((FieldBuilder) builder).SetCustomAttribute (cb);
					} else if (kind is Property || kind is Indexer ||
						   kind is InterfaceProperty || kind is InterfaceIndexer) {
						((PropertyBuilder) builder).SetCustomAttribute (cb);
					} else if (kind is Event || kind is InterfaceEvent) {
						((MyEventBuilder) builder).SetCustomAttribute (cb);
					} else if (kind is ParameterBuilder) {

						if (attr_type == TypeManager.marshal_as_attr_type) {
							UnmanagedMarshal marshal = GetMarshal (a);
							if (marshal == null) {
								Report.Warning (-24, loc,
									"The Microsoft Runtime cannot set this marshal info. " +
									"Please use the Mono runtime instead.");
							} else {
								((ParameterBuilder) builder).SetMarshal (marshal);
							}
						} else { 

							try {
								((ParameterBuilder) builder).SetCustomAttribute (cb);
							} catch (System.ArgumentException) {
								Report.Warning (-24, loc,
										"The Microsoft Runtime cannot set attributes \n" +
										"on the return type of a method. Please use the \n" +
										"Mono runtime instead.");
							}

						}
					} else if (kind is Enum) {
						((TypeBuilder) builder).SetCustomAttribute (cb); 

					} else if (kind is TypeContainer) {
						TypeContainer tc = (TypeContainer) kind;
						
						if (a.UsageAttr) {
							tc.Targets = a.Targets;
							tc.AllowMultiple = a.AllowMultiple;
							tc.Inherited = a.Inherited;

							TypeManager.RegisterAttributeAllowMultiple (tc.TypeBuilder,
												    tc.AllowMultiple);
							
						} else if (attr_type == TypeManager.default_member_type) {
							if (tc.Indexers != null) {
								Report.Error (646, loc,
								      "Cannot specify the DefaultMember attribute on" +
								      " a type containing an indexer");
								return;
							}

						} else {
							if (!CheckAttribute (a, kind)) {
								Error_AttributeNotValidForElement (a, loc);
								return;
							}
						}

						try {
							((TypeBuilder) builder).SetCustomAttribute (cb);
						} catch (System.ArgumentException) {
							Report.Warning (
								-21, loc,
						"The CharSet named property on StructLayout\n"+
						"\tdoes not work correctly on Microsoft.NET\n"+
						"\tYou might want to remove the CharSet declaration\n"+
						"\tor compile using the Mono runtime instead of the\n"+
						"\tMicrosoft .NET runtime");
						}
						
					} else if (kind is Interface) {
						Interface iface = (Interface) kind;

						if ((attr_type == TypeManager.default_member_type) &&
						    (iface.InterfaceIndexers != null)) {
							Report.Error (
								646, loc,
								"Cannot specify the DefaultMember attribute on" +
								" a type containing an indexer");
							return;
						}

						if (!CheckAttribute (a, kind)) {
							Error_AttributeNotValidForElement (a, loc);
							return;
						}

						((TypeBuilder) builder).SetCustomAttribute (cb);
					} else if (kind is AssemblyBuilder){
						((AssemblyBuilder) builder).SetCustomAttribute (cb);
					} else if (kind is ModuleBuilder) {
						((ModuleBuilder) builder).SetCustomAttribute (cb);
					} else if (kind is FieldBuilder) {
						if (attr_type == TypeManager.marshal_as_attr_type) {
							UnmanagedMarshal marshal = GetMarshal (a);
							if (marshal == null) {
								Report.Warning (-24, loc,
									"The Microsoft Runtime cannot set this marshal info. " +
									"Please use the Mono runtime instead.");
							} else {
								((ParameterBuilder) builder).SetMarshal (marshal);
							}
						} else { 
							((FieldBuilder) builder).SetCustomAttribute (cb);
						}
					} else
						throw new Exception ("Unknown kind: " + kind);

					//
					// Once an attribute type has been emitted once we
					// keep track of the info to prevent multiple occurences
					// for attributes which do not explicitly allow it
					//
					if (!emitted_attrs.Contains (attr_type))
						emitted_attrs.Add (attr_type);

					//
					// We keep of this target-wise and so emitted targets
					// are tracked too
					//
					if (!emitted_targets.Contains (attr_target))
						emitted_targets.Add (attr_target);
				}
				
				
			}
		}

		public MethodBuilder DefinePInvokeMethod (EmitContext ec, TypeBuilder builder, string name,
							  MethodAttributes flags, Type ret_type, Type [] param_types)
		{
			//
			// We extract from the attribute the information we need 
			//

			if (Arguments == null) {
				Console.WriteLine ("Internal error : this is not supposed to happen !");
				return null;
			}

			Type = CheckAttributeType (ec);
			if (Type == null)
				return null;
			
			ArrayList named_args = new ArrayList ();
			
			ArrayList pos_args = (ArrayList) Arguments [0];
			if (Arguments.Count > 1)
				named_args = (ArrayList) Arguments [1];
			

			string dll_name = null;
			
			Argument tmp = (Argument) pos_args [0];

			if (!tmp.Resolve (ec, Location))
				return null;
			
			if (tmp.Expr is Constant)
				dll_name = (string) ((Constant) tmp.Expr).GetValue ();
			else { 
				Error_AttributeArgumentNotValid (Location);
				return null;
			}

			// Now we process the named arguments
			CallingConvention cc = CallingConvention.Winapi;
			CharSet charset = CharSet.Ansi;
			bool preserve_sig = true;
			bool exact_spelling = false;
			bool set_last_err = false;
			string entry_point = null;

			for (int i = 0; i < named_args.Count; i++) {

				DictionaryEntry de = (DictionaryEntry) named_args [i];

				string member_name = (string) de.Key;
				Argument a  = (Argument) de.Value;

				if (!a.Resolve (ec, Location))
					return null;

				Expression member = Expression.MemberLookup (
					ec, Type, member_name, 
					MemberTypes.Field | MemberTypes.Property,
					BindingFlags.Public | BindingFlags.Instance,
					Location);

				if (member == null || !(member is FieldExpr)) {
					Error_InvalidNamedArgument (member_name);
					return null;
				}

				if (member is FieldExpr) {
					FieldExpr fe = (FieldExpr) member;
					FieldInfo fi = fe.FieldInfo;

					if (fi.IsInitOnly) {
						Error_InvalidNamedArgument (member_name);
						return null;
					}

					if (a.Expr is Constant) {
						Constant c = (Constant) a.Expr;
						
						if (member_name == "CallingConvention")
							cc = (CallingConvention) c.GetValue ();
						else if (member_name == "CharSet")
							charset = (CharSet) c.GetValue ();
						else if (member_name == "EntryPoint")
							entry_point = (string) c.GetValue ();
						else if (member_name == "SetLastError")
							set_last_err = (bool) c.GetValue ();
						else if (member_name == "ExactSpelling")
							exact_spelling = (bool) c.GetValue ();
						else if (member_name == "PreserveSig")
							preserve_sig = (bool) c.GetValue ();
					} else { 
						Error_AttributeArgumentNotValid (Location);
						return null;
					}
					
				}
			}

			if (entry_point == null)
				entry_point = name;
			
			MethodBuilder mb = builder.DefinePInvokeMethod (
				name, dll_name, entry_point, flags | MethodAttributes.HideBySig,
				CallingConventions.Standard,
				ret_type,
				param_types,
				cc,
				charset);

			if (preserve_sig)
				mb.SetImplementationFlags (MethodImplAttributes.PreserveSig);
			
			return mb;
		}
		
	}
	
	public class AttributeSection {
		public readonly string    Target;
		public readonly ArrayList Attributes;
		
		public AttributeSection (string target, ArrayList attrs)
		{
			Target = target;
			Attributes = attrs;
		}
		
	}

	public class Attributes {
		public ArrayList AttributeSections;

		public Attributes (AttributeSection a)
		{
			AttributeSections = new ArrayList ();
			AttributeSections.Add (a);

		}

		public void AddAttributeSection (AttributeSection a)
		{
			if (a != null && !AttributeSections.Contains (a))
				AttributeSections.Add (a);
		}

		public bool Contains (Type t)
		{
			foreach (AttributeSection attr_section in AttributeSections){
				foreach (Attribute a in attr_section.Attributes){
					if (a.Type == t)
						return true;
				}
			}
                        
			return false;
		}
	}
}
