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

		public readonly Location Location;

		public Type Type;
		
		//
		// The following are only meaningful when the attribute
		// being emitted is an AttributeUsage attribute
		//
		AttributeTargets Targets;
		bool AllowMultiple;
		bool Inherited;

		bool UsageAttr = false;
		
		MethodImplOptions ImplOptions;
		UnmanagedType     UnmanagedType;
		CustomAttributeBuilder cb;
	
		// non-null if named args present after Resolve () is called
		PropertyInfo [] prop_info_arr;
		FieldInfo [] field_info_arr;
		object [] field_values_arr;
		object [] prop_values_arr;
		object [] pos_values;
		
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

		/// <summary>
                ///   Tries to resolve the type of the attribute. Flags an error if it can't.
                /// </summary>
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
			} else if (e is EmptyCast) {
				result = e;
				if (((EmptyCast) e).Child is Constant) {
					result = ((Constant) ((EmptyCast)e).Child).GetValue();
				}
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

                        //
                        // If we are a certain special attribute, we
                        // set the information accordingly
                        //
                        
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

			pos_values = new object [pos_arg_count];

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
					} else if (e is ArrayCreation) {
						prop_values.Add (((ArrayCreation) e).EncodeAsAttribute());
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
				        -101, Location, "NullReferenceException while trying to create attribute." +
                                        "Something's wrong!");
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

                /// <summary>
                ///   Get a string containing a list of valid targets for the attribute 'attr'
                /// </summary>
		static string GetValidTargets (Attribute attr)
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
				"It is valid on " + GetValidTargets (a) + "declarations only.");
		}

                /// <summary>
                ///   Ensure that Attribute 'a' is being applied to the right language element (target)
                /// </summary>
		public static bool CheckAttributeTarget (Attribute a, object element)
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
					if (tmp is AttributeUsageAttribute) { 
						targets = ((AttributeUsageAttribute) tmp).ValidOn;
                                                break;
                                        }
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
			} else if (element is Event) {
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
			} else if (element is Method || element is Operator ||
                                   element is Accessor) {
				if ((targets & AttributeTargets.Method) != 0)
					return true;
				else
					return false;
			} else if (element is ParameterBuilder) {
				if ((targets & AttributeTargets.Parameter) != 0 ||
                                    (targets & AttributeTargets.ReturnValue) != 0)
					return true;
				else
					return false;
			} else if (element is Property || element is Indexer ||
				   element is Accessor) {
				if ((targets & AttributeTargets.Property) != 0)
					return true;
				else
					return false;
			} else if (element is AssemblyClass){
				if ((targets & AttributeTargets.Assembly) != 0)
					return true;
				else
					return false;
			} else if (element is ModuleClass){
				if ((targets & AttributeTargets.Module) != 0)
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

		/// <summary>
		/// Returns value of CLSCompliantAttribute contructor parameter but because the method can be called
		/// before ApplyAttribute. We need to resolve the arguments.
		/// This situation occurs when class deps is differs from Emit order.  
		/// </summary>
		public bool GetClsCompliantAttributeValue (DeclSpace ds)
		{
			if (pos_values == null) {
				EmitContext ec = new EmitContext (ds, ds, Location, null, null, 0, false);

				// TODO: It is not neccessary to call whole Resolve (ApplyAttribute does it now) we need only ctor args.
				// But because a lot of attribute class code must be rewritten will be better to wait...
				Resolve (ec);
			}

			// Some error occurred
			if (pos_values [0] == null)
				return false;

			return (bool)pos_values [0];
		}

		static object GetFieldValue (Attribute a, string name)
                {
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

		static UnmanagedMarshal GetMarshal (Attribute a)
		{
			object o = GetFieldValue (a, "ArraySubType");
			UnmanagedType array_sub_type = o == null ? UnmanagedType.I4 : (UnmanagedType) o;
			
			switch (a.UnmanagedType) {
			case UnmanagedType.CustomMarshaler:
				MethodInfo define_custom = typeof (UnmanagedMarshal).GetMethod ("DefineCustom",
                                                                       BindingFlags.Static | BindingFlags.Public);
				if (define_custom == null)
					return null;
				
				object [] args = new object [4];
				args [0] = GetFieldValue (a, "MarshalTypeRef");
				args [1] = GetFieldValue (a, "MarshalCookie");
				args [2] = GetFieldValue (a, "MarshalType");
				args [3] = Guid.Empty;
				return (UnmanagedMarshal) define_custom.Invoke (null, args);
				
			case UnmanagedType.LPArray:				
				return UnmanagedMarshal.DefineLPArray (array_sub_type);
			
			case UnmanagedType.SafeArray:
				return UnmanagedMarshal.DefineSafeArray (array_sub_type);
			
			case UnmanagedType.ByValArray:
				return UnmanagedMarshal.DefineByValArray ((int) GetFieldValue (a, "SizeConst"));
			
			case UnmanagedType.ByValTStr:
				return UnmanagedMarshal.DefineByValTStr ((int) GetFieldValue (a, "SizeConst"));
			
			default:
				return UnmanagedMarshal.DefineUnmanagedMarshal (a.UnmanagedType);
			}
		}

		/// <summary>
		///   Applies the attributes specified on target 'kind' to the `builder'.
		/// </summary>
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

				if (attr_target == "return" && !(builder is ParameterBuilder))
					continue;
				
				foreach (Attribute a in asec.Attributes) {
					Location loc = a.Location;
					CustomAttributeBuilder cb = a.Resolve (ec);
					attr_type = a.Type;

					if (cb == null) 
						continue;
					
					if (!(kind is TypeContainer))
						if (!CheckAttributeTarget (a, kind)) {
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

					if (kind is IAttributeSupport) {
						if (attr_type == TypeManager.methodimpl_attr_type && a.ImplOptions == MethodImplOptions.InternalCall) {
							((MethodBuilder) builder).SetImplementationFlags (MethodImplAttributes.InternalCall | MethodImplAttributes.Runtime);
						} 
						else {
							IAttributeSupport attributeSupport = kind as IAttributeSupport;
							attributeSupport.SetCustomAttribute (cb);
						}
					}
					else if (kind is Method || kind is Operator || kind is Accessor) {
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
						if (attr_type == TypeManager.marshal_as_attr_type) {
							UnmanagedMarshal marshal = GetMarshal (a);
							if (marshal == null) {
								Report.Warning (-24, loc,
									"The Microsoft Runtime cannot set this marshal info. " +
									"Please use the Mono runtime instead.");
							} else {
								((FieldBuilder) builder).SetMarshal (marshal);
							}
						} else { 
							((FieldBuilder) builder).SetCustomAttribute (cb);
						}
					} else if (kind is Property || kind is Indexer) {

                                                if (builder is PropertyBuilder) 
                                                        ((PropertyBuilder) builder).SetCustomAttribute (cb);
                                                //
                                                // This is for the case we are setting attributes on
                                                // the get and set accessors
                                                //
                                                else if (builder is MethodBuilder)
                                                        ((MethodBuilder) builder).SetCustomAttribute (cb);
					} else if (kind is Event) {
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
							if (!CheckAttributeTarget (a, kind)) {
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
					} else if (kind is Delegate){
						if (!CheckAttributeTarget (a, kind)) {
							Error_AttributeNotValidForElement (a, loc);
							return;
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
						    (iface.Indexers != null)) {
							Report.Error (
								646, loc,
								"Cannot specify the DefaultMember attribute on" +
								" a type containing an indexer");
							return;
						}

						if (!CheckAttributeTarget (a, kind)) {
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

			// Here we are testing attribute arguments for array usage (error 3016)
			DeclSpace ds = kind as DeclSpace;
			if ((ds != null && ds.IsClsCompliaceRequired (ds)) ||
			    (kind is AssemblyClass && CodeGen.Assembly.IsClsCompliant)) {
				
				foreach (AttributeSection asec in opt_attrs.AttributeSections) {
					foreach (Attribute a in asec.Attributes) {
						if (a.Arguments == null)
							continue;

						ArrayList pos_args = (ArrayList) a.Arguments [0];
						if (pos_args != null) {
							foreach (Argument arg in pos_args) { 
								// Type is undefined (was error 246)
								if (arg.Type == null)
									return;

								if (arg.Type.IsArray) {
									Report.Error_T (3016, a.Location);
									return;
								}
							}
						}
					
						if (a.Arguments.Count < 2)
							continue;
					
						ArrayList named_args = (ArrayList) a.Arguments [1];
						foreach (DictionaryEntry de in named_args) {
							Argument arg  = (Argument) de.Value;

							// Type is undefined (was error 246)
							if (arg.Type == null)
								return;

							if (arg.Type.IsArray) 
							{
								Report.Error_T (3016, a.Location);
								return;
							}
						}
					}
				}
			}

		}

		public object GetValue (EmitContext ec, Constant c, Type target)
		{
			if (Convert.ImplicitConversionExists (ec, c, target))
				return c.GetValue ();

			Convert.Error_CannotImplicitConversion (Location, c.Type, target);
			return null;
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
#if FIXME
			bool exact_spelling = false;
#endif
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

						try {
							if (member_name == "CallingConvention"){
								object val = GetValue (ec, c, typeof (CallingConvention));
								if (val == null)
									return null;
								cc = (CallingConvention) val;
							} else if (member_name == "CharSet"){
								charset = (CharSet) c.GetValue ();
							} else if (member_name == "EntryPoint")
								entry_point = (string) c.GetValue ();
							else if (member_name == "SetLastError")
								set_last_err = (bool) c.GetValue ();
#if FIXME
							else if (member_name == "ExactSpelling")
								exact_spelling = (bool) c.GetValue ();
#endif
							else if (member_name == "PreserveSig")
								preserve_sig = (bool) c.GetValue ();
						} catch (InvalidCastException){
							Error_InvalidNamedArgument (member_name);
							Error_AttributeArgumentNotValid (Location);
						}
					} else { 
						Error_AttributeArgumentNotValid (Location);
						return null;
					}
					
				}
			}

			if (entry_point == null)
				entry_point = name;
			if (set_last_err)
				charset = (CharSet)((int)charset | 0x40);
			
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

		private Expression GetValue () 
		{
			if ((Arguments == null) || (Arguments.Count < 1))
				return null;
			ArrayList al = (ArrayList) Arguments [0];
			if ((al == null) || (al.Count < 1))
				return null;
			Argument arg = (Argument) al [0];
			if ((arg == null) || (arg.Expr == null))
				return null;
			return arg.Expr;
		}

		public string GetString () 
		{
			Expression e = GetValue ();
			if (e is StringLiteral)
				return (e as StringLiteral).Value;
			return null;
		}

		public bool GetBoolean () 
		{
			Expression e = GetValue ();
			if (e is BoolLiteral)
				return (e as BoolLiteral).Value;
			return false;
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

		public Attribute GetClsCompliantAttribute (DeclSpace ds)
		{
			foreach (AttributeSection attr_section in AttributeSections) {
				foreach (Attribute a in attr_section.Attributes) {

					// Unfortunately, we have also attributes that has not been resolved yet.
					// We need to simulate part of ApplyAttribute method.
					if (a.Type == null) {
						Type attr_type = RootContext.LookupType (ds, GetAttributeFullName (a.Name), true, a.Location);
						if (attr_type == TypeManager.cls_compliant_attribute_type)
							return a;

						continue;
					}

					if (a.Type == TypeManager.cls_compliant_attribute_type)
						return a;
				}
			}
			return null;
		}

		public static string GetAttributeFullName (string name)
		{
			if (name.EndsWith ("Attribute"))
				return name;
			return name + "Attribute";
		}
	}

	public interface IAttributeSupport
	{
		void SetCustomAttribute (CustomAttributeBuilder customBuilder);
	}

	/// <summary>
	/// Helper class for attribute verification routine.
	/// </summary>
	sealed class AttributeTester
	{
		static PtrHashtable analyzed_types = new PtrHashtable ();

		private AttributeTester ()
		{
		}

		/// <summary>
		/// Returns true if parameters of two compared methods are CLS-Compliant.
		/// It tests differing only in ref or out, or in array rank.
		/// </summary>
		public static bool AreOverloadedMethodParamsClsCompliant (Type[] types_a, Type[] types_b) 
		{
			if (types_a == null || types_b == null)
				return true;

			if (types_a.Length != types_b.Length)
				return true;

			for (int i = 0; i < types_b.Length; ++i) {
				Type aType = types_a [i];
				Type bType = types_b [i];

				if (aType.IsArray && bType.IsArray && aType.GetArrayRank () != bType.GetArrayRank () && aType.GetElementType () == bType.GetElementType ()) {
					return false;
				}

				Type aBaseType = aType;
				bool is_either_ref_or_out = false;

				if (aType.IsByRef || aType.IsPointer) {
					aBaseType = aType.GetElementType ();
					is_either_ref_or_out = true;
				}

				Type bBaseType = bType;
				if (bType.IsByRef || bType.IsPointer) 
				{
					bBaseType = bType.GetElementType ();
					is_either_ref_or_out = !is_either_ref_or_out;
				}

				if (aBaseType != bBaseType)
					continue;

				if (is_either_ref_or_out)
					return false;
			}
			return true;
		}

		/// <summary>
		/// Goes through all parameters and test if they are CLS-Compliant.
		/// </summary>
		public static bool AreParametersCompliant (Parameter[] fixedParameters, Location loc)
		{
			if (fixedParameters == null)
				return true;

			foreach (Parameter arg in fixedParameters) {
				if (!AttributeTester.IsClsCompliant (arg.ParameterType)) {
					Report.Error_T (3001, loc, arg.GetSignatureForError ());
					return false;
				}
			}
			return true;
		}


		/// <summary>
		/// This method tests the CLS compliance of external types. It doesn't test type visibility.
		/// </summary>
		public static bool IsClsCompliant (Type type) 
		{
			if (type == null)
				return true;

			object type_compliance = analyzed_types[type];
			if (type_compliance != null)
				return type_compliance == TRUE;

			if (type.IsPointer) {
				analyzed_types.Add (type, null);
				return false;
			}

			bool result;
			if (type.IsArray || type.IsByRef)	{
				result = IsClsCompliant (TypeManager.GetElementType (type));
			} else {
				result = AnalyzeTypeCompliance (type);
			}
			analyzed_types.Add (type, result ? TRUE : FALSE);
			return result;
		}                

		static object TRUE = new object ();
		static object FALSE = new object ();

		/// <summary>
		/// Non-hierarchical CLS Compliance analyzer
		/// </summary>
		public static bool IsComplianceRequired (MemberInfo mi, DeclSpace ds)
		{
			DeclSpace temp_ds = TypeManager.LookupDeclSpace (mi.DeclaringType);

			// Type is external, we can get attribute directly
			if (temp_ds == null) {
				object[] cls_attribute = mi.GetCustomAttributes (TypeManager.cls_compliant_attribute_type, false);
				return (cls_attribute.Length == 1 && ((CLSCompliantAttribute)cls_attribute[0]).IsCompliant);
			}

			string tmp_name;
			// Interface doesn't store full name
			if (temp_ds is Interface)
				tmp_name = mi.Name;
			else
				tmp_name = String.Concat (temp_ds.Name, ".", mi.Name);

			MemberCore mc = temp_ds.GetDefinition (tmp_name) as MemberCore;
			return mc.IsClsCompliaceRequired (ds);
		}

		public static void VerifyModulesClsCompliance ()
		{
			Module[] modules = TypeManager.Modules;
			if (modules == null)
				return;

			// The first module is generated assembly
			for (int i = 1; i < modules.Length; ++i) {
				Module module = modules [i];
				if (!IsClsCompliant (module)) {
					Report.Error_T (3013, module.Name);
					return;
				}
			}
		}

		static bool IsClsCompliant (ICustomAttributeProvider attribute_provider) 
		{
			object[] CompliantAttribute = attribute_provider.GetCustomAttributes (TypeManager.cls_compliant_attribute_type, false);
			if (CompliantAttribute.Length == 0)
				return false;

			return ((CLSCompliantAttribute)CompliantAttribute[0]).IsCompliant;
		}

		static bool AnalyzeTypeCompliance (Type type)
		{
			DeclSpace ds = TypeManager.LookupDeclSpace (type);
			if (ds != null) {
				return ds.IsClsCompliaceRequired (ds.Parent);
			}

			object[] CompliantAttribute = type.GetCustomAttributes (TypeManager.cls_compliant_attribute_type, false);
			if (CompliantAttribute.Length == 0) 
				return IsClsCompliant (type.Assembly);

			return ((CLSCompliantAttribute)CompliantAttribute[0]).IsCompliant;
		}
	}
}
