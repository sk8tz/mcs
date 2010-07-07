//
// typemanager.cs: C# type manager
//
// Author: Miguel de Icaza (miguel@gnu.org)
//         Ravi Pratap     (ravi@ximian.com)
//         Marek Safar     (marek.safar@seznam.cz)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2001-2003 Ximian, Inc (http://www.ximian.com)
// Copyright 2003-2008 Novell, Inc.
//

using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Linq;

namespace Mono.CSharp {

	partial class TypeManager {
	//
	// A list of core types that the compiler requires or uses
	//
	static public PredefinedTypeSpec object_type;
	static public PredefinedTypeSpec value_type;
	static public PredefinedTypeSpec string_type;
	static public PredefinedTypeSpec int32_type;
	static public PredefinedTypeSpec uint32_type;
	static public PredefinedTypeSpec int64_type;
	static public PredefinedTypeSpec uint64_type;
	static public PredefinedTypeSpec float_type;
	static public PredefinedTypeSpec double_type;
	static public PredefinedTypeSpec char_type;
	static public PredefinedTypeSpec short_type;
	static public PredefinedTypeSpec decimal_type;
	static public PredefinedTypeSpec bool_type;
	static public PredefinedTypeSpec sbyte_type;
	static public PredefinedTypeSpec byte_type;
	static public PredefinedTypeSpec ushort_type;
	static public PredefinedTypeSpec enum_type;
	static public PredefinedTypeSpec delegate_type;
	static public PredefinedTypeSpec multicast_delegate_type;
	static public PredefinedTypeSpec void_type;
	static public PredefinedTypeSpec array_type;
	static public PredefinedTypeSpec runtime_handle_type;
	static public PredefinedTypeSpec type_type;
	static public PredefinedTypeSpec ienumerator_type;
	static public PredefinedTypeSpec ienumerable_type;
	static public PredefinedTypeSpec idisposable_type;
	static public PredefinedTypeSpec intptr_type;
	static public PredefinedTypeSpec uintptr_type;
	static public PredefinedTypeSpec runtime_field_handle_type;
	static public PredefinedTypeSpec attribute_type;
	static public PredefinedTypeSpec exception_type;


	static public TypeSpec null_type;
	static public TypeSpec typed_reference_type;
	static public TypeSpec arg_iterator_type;
	static public TypeSpec mbr_type;
	public static TypeSpec runtime_helpers_type;
	static public TypeSpec iasyncresult_type;
	static public TypeSpec asynccallback_type;
	static public TypeSpec runtime_argument_handle_type;
	static public TypeSpec void_ptr_type;

	// 
	// C# 2.0
	//
	static internal TypeSpec isvolatile_type;
	static public TypeSpec generic_ilist_type;
	static public TypeSpec generic_icollection_type;
	static public TypeSpec generic_ienumerator_type;
	static public TypeSpec generic_ienumerable_type;
	static public TypeSpec generic_nullable_type;

	//
	// C# 3.0
	//
	static internal TypeSpec expression_type;
	public static TypeSpec parameter_expression_type;
	public static TypeSpec fieldinfo_type;
	public static TypeSpec methodinfo_type;
	public static TypeSpec ctorinfo_type;

	//
	// C# 4.0
	//
	public static TypeSpec call_site_type;
	public static TypeSpec generic_call_site_type;
	public static TypeExpr binder_type;
	public static TypeSpec binder_flags;

	public static TypeExpr expression_type_expr;


	//
	// These methods are called by code generated by the compiler
	//
	static public FieldSpec string_empty;
	static public MethodSpec system_type_get_type_from_handle;
	static public MethodSpec bool_movenext_void;
	static public MethodSpec void_dispose_void;
	static public MethodSpec void_monitor_enter_object;
	static public MethodSpec void_monitor_exit_object;
	static public MethodSpec void_initializearray_array_fieldhandle;
	static public MethodSpec delegate_combine_delegate_delegate;
	static public MethodSpec delegate_remove_delegate_delegate;
	static public PropertySpec int_get_offset_to_string_data;
	static public MethodSpec int_interlocked_compare_exchange;
	static public PropertySpec ienumerator_getcurrent;
	public static MethodSpec methodbase_get_type_from_handle;
	public static MethodSpec methodbase_get_type_from_handle_generic;
	public static MethodSpec fieldinfo_get_field_from_handle;
	public static MethodSpec fieldinfo_get_field_from_handle_generic;
	public static MethodSpec activator_create_instance;

	//
	// The constructors.
	//
	static public MethodSpec void_decimal_ctor_five_args;
	static public MethodSpec void_decimal_ctor_int_arg;
	public static MethodSpec void_decimal_ctor_long_arg;

	static Dictionary<Assembly, bool> assembly_internals_vis_attrs;

	static TypeManager ()
	{
		Reset ();
	}

	static public void Reset ()
	{
//		object_type = null;
	
		assembly_internals_vis_attrs = new Dictionary<Assembly, bool> ();
		
		// TODO: I am really bored by all this static stuff
		system_type_get_type_from_handle =
		bool_movenext_void =
		void_dispose_void =
		void_monitor_enter_object =
		void_monitor_exit_object =
		void_initializearray_array_fieldhandle =
		int_interlocked_compare_exchange =
		methodbase_get_type_from_handle =
		methodbase_get_type_from_handle_generic =
		fieldinfo_get_field_from_handle =
		fieldinfo_get_field_from_handle_generic =
		activator_create_instance =
		delegate_combine_delegate_delegate =
		delegate_remove_delegate_delegate = null;

		int_get_offset_to_string_data =
		ienumerator_getcurrent = null;

		void_decimal_ctor_five_args =
		void_decimal_ctor_int_arg =
		void_decimal_ctor_long_arg = null;

		string_empty = null;

		call_site_type =
		generic_call_site_type =
		binder_flags = null;

		binder_type = null;

		typed_reference_type = arg_iterator_type = mbr_type =
		runtime_helpers_type = iasyncresult_type = asynccallback_type =
		runtime_argument_handle_type = void_ptr_type = isvolatile_type =
		generic_ilist_type = generic_icollection_type = generic_ienumerator_type =
		generic_ienumerable_type = generic_nullable_type = expression_type =
		parameter_expression_type = fieldinfo_type = methodinfo_type = ctorinfo_type = null;

		expression_type_expr = null;
	}

	/// <summary>
	///   Returns the C# name of a type if possible, or the full type name otherwise
	/// </summary>
	static public string CSharpName (TypeSpec t)
	{
		return t.GetSignatureForError ();
	}

	static public string CSharpName (IList<TypeSpec> types)
	{
		if (types.Count == 0)
			return string.Empty;

		StringBuilder sb = new StringBuilder ();
		for (int i = 0; i < types.Count; ++i) {
			if (i > 0)
				sb.Append (",");

			sb.Append (CSharpName (types [i]));
		}
		return sb.ToString ();
	}

	static public string GetFullNameSignature (MemberSpec mi)
	{
		return mi.GetSignatureForError ();
	}

	static public string CSharpSignature (MemberSpec mb)
	{
		return mb.GetSignatureForError ();
	}

	//
	// Looks up a type, and aborts if it is not found.  This is used
	// by predefined types required by the compiler
	//
	public static TypeSpec CoreLookupType (CompilerContext ctx, string ns_name, string name, MemberKind kind, bool required)
	{
		return CoreLookupType (ctx, ns_name, name, 0, kind, required);
	}

	public static TypeSpec CoreLookupType (CompilerContext ctx, string ns_name, string name, int arity, MemberKind kind, bool required)
	{
		Namespace ns = GlobalRootNamespace.Instance.GetNamespace (ns_name, true);
		var te = ns.LookupType (ctx, name, arity, !required, Location.Null);
		var ts = te == null ? null : te.Type;

		if (!required)
			return ts;

		if (ts == null) {
			ctx.Report.Error (518, "The predefined type `{0}.{1}' is not defined or imported",
				ns_name, name);
			return null;
		}

		if (ts.Kind != kind) {
			ctx.Report.Error (520, "The predefined type `{0}.{1}' is not declared correctly",
				ns_name, name);
			return null;
		}

		return ts;
	}

	static MemberSpec GetPredefinedMember (TypeSpec t, MemberFilter filter, Location loc)
	{
		const BindingRestriction restrictions = BindingRestriction.AccessibleOnly | BindingRestriction.DeclaredOnly;
		var member = MemberCache.FindMember (t, filter, restrictions);

		if (member != null)
			return member;

		string method_args = null;
		if (filter.Parameters != null)
			method_args = filter.Parameters.GetSignatureForError ();

		RootContext.ToplevelTypes.Compiler.Report.Error (656, loc, "The compiler required member `{0}.{1}{2}' could not be found or is inaccessible",
			TypeManager.CSharpName (t), filter.Name, method_args);

		return null;
	}

	//
	// Returns the ConstructorInfo for "args"
	//
	public static MethodSpec GetPredefinedConstructor (TypeSpec t, Location loc, params TypeSpec [] args)
	{
		var pc = ParametersCompiled.CreateFullyResolved (args);
		return GetPredefinedMember (t, MemberFilter.Constructor (pc), loc) as MethodSpec;
	}

	//
	// Returns the method specification for a method named `name' defined
	// in type `t' which takes arguments of types `args'
	//
	public static MethodSpec GetPredefinedMethod (TypeSpec t, string name, Location loc, params TypeSpec [] args)
	{
		var pc = ParametersCompiled.CreateFullyResolved (args);
		return GetPredefinedMethod (t, MemberFilter.Method (name, 0, pc, null), loc);
	}

	public static MethodSpec GetPredefinedMethod (TypeSpec t, MemberFilter filter, Location loc)
	{
		return GetPredefinedMember (t, filter, loc) as MethodSpec;
	}

	public static FieldSpec GetPredefinedField (TypeSpec t, string name, Location loc, TypeSpec type)
	{
		return GetPredefinedMember (t, MemberFilter.Field (name, type), loc) as FieldSpec;
	}

	public static PropertySpec GetPredefinedProperty (TypeSpec t, string name, Location loc, TypeSpec type)
	{
		return GetPredefinedMember (t, MemberFilter.Property (name, type), loc) as PropertySpec;
	}

	public static IList<PredefinedTypeSpec> InitCoreTypes ()
	{
		var core_types = new PredefinedTypeSpec[] {
			object_type = new PredefinedTypeSpec (MemberKind.Class, "System", "Object"),
			value_type = new PredefinedTypeSpec (MemberKind.Class, "System", "ValueType"),
			attribute_type = new PredefinedTypeSpec (MemberKind.Class, "System", "Attribute"),

			int32_type = new PredefinedTypeSpec (MemberKind.Struct, "System", "Int32"),
			int64_type = new PredefinedTypeSpec (MemberKind.Struct, "System", "Int64"),
			uint32_type = new PredefinedTypeSpec (MemberKind.Struct, "System", "UInt32"),
			uint64_type = new PredefinedTypeSpec (MemberKind.Struct, "System", "UInt64"),
			byte_type = new PredefinedTypeSpec (MemberKind.Struct, "System", "Byte"),
			sbyte_type = new PredefinedTypeSpec (MemberKind.Struct, "System", "SByte"),
			short_type = new PredefinedTypeSpec (MemberKind.Struct, "System", "Int16"),
			ushort_type = new PredefinedTypeSpec (MemberKind.Struct, "System", "UInt16"),

			ienumerator_type = new PredefinedTypeSpec (MemberKind.Interface, "System.Collections", "IEnumerator"),
			ienumerable_type = new PredefinedTypeSpec (MemberKind.Interface, "System.Collections", "IEnumerable"),
			idisposable_type = new PredefinedTypeSpec (MemberKind.Interface, "System", "IDisposable"),

			char_type = new PredefinedTypeSpec (MemberKind.Struct, "System", "Char"),
			string_type = new PredefinedTypeSpec (MemberKind.Class, "System", "String"),
			float_type = new PredefinedTypeSpec (MemberKind.Struct, "System", "Single"),
			double_type = new PredefinedTypeSpec (MemberKind.Struct, "System", "Double"),
			decimal_type = new PredefinedTypeSpec (MemberKind.Struct, "System", "Decimal"),
			bool_type = new PredefinedTypeSpec (MemberKind.Struct, "System", "Boolean"),
			intptr_type = new PredefinedTypeSpec (MemberKind.Struct, "System", "IntPtr"),
			uintptr_type = new PredefinedTypeSpec (MemberKind.Struct, "System", "UIntPtr"),

			multicast_delegate_type = new PredefinedTypeSpec (MemberKind.Class, "System", "MulticastDelegate"),
			delegate_type = new PredefinedTypeSpec (MemberKind.Class, "System", "Delegate"),
			enum_type = new PredefinedTypeSpec (MemberKind.Class, "System", "Enum"),
			array_type = new PredefinedTypeSpec (MemberKind.Class, "System", "Array"),
			void_type = new PredefinedTypeSpec (MemberKind.Struct, "System", "Void"),
			type_type = new PredefinedTypeSpec (MemberKind.Class, "System", "Type"),
			exception_type = new PredefinedTypeSpec (MemberKind.Class, "System", "Exception"),
			runtime_field_handle_type = new PredefinedTypeSpec (MemberKind.Struct, "System", "RuntimeFieldHandle"),
			runtime_handle_type = new PredefinedTypeSpec (MemberKind.Struct, "System", "RuntimeTypeHandle"),
		};

		return core_types;
	}

	/// <remarks>
	///   The types have to be initialized after the initial
	///   population of the type has happened (for example, to
	///   bootstrap the corlib.dll
	/// </remarks>
	public static bool InitCoreTypes (CompilerContext ctx, IList<PredefinedTypeSpec> predefined)
	{
		foreach (var p in predefined) {
			var found = CoreLookupType (ctx, p.Namespace, p.Name, p.Kind, true);
			if (found == null || found == p)
				continue;

			if (!RootContext.StdLib) {
				var ns = GlobalRootNamespace.Instance.GetNamespace (p.Namespace, false);
				ns.ReplaceTypeWithPredefined (found, p);

				var tc = found.MemberDefinition as TypeContainer;
				tc.SetPredefinedSpec (p);
				p.SetDefinition (found);
			}
		}

		PredefinedAttributes.Get.ParamArray.Initialize (ctx, false);
		PredefinedAttributes.Get.Out.Initialize (ctx, false);

		return ctx.Report.Errors == 0;
	}

	//
	// Initializes optional core types
	//
	public static void InitOptionalCoreTypes (CompilerContext ctx)
	{
		//
		// These are only used for compare purposes
		//
		null_type = InternalType.Null;

		void_ptr_type = PointerContainer.MakeType (void_type);

		//
		// Initialize InternalsVisibleTo as the very first optional type. Otherwise we would populate
		// types cache with incorrect accessiblity when any of optional types is internal.
		//
		PredefinedAttributes.Get.Initialize (ctx);

		runtime_argument_handle_type = CoreLookupType (ctx, "System", "RuntimeArgumentHandle", MemberKind.Struct, false);
		asynccallback_type = CoreLookupType (ctx, "System", "AsyncCallback", MemberKind.Delegate, false);
		iasyncresult_type = CoreLookupType (ctx, "System", "IAsyncResult", MemberKind.Interface, false);
		typed_reference_type = CoreLookupType (ctx, "System", "TypedReference", MemberKind.Struct, false);
		arg_iterator_type = CoreLookupType (ctx, "System", "ArgIterator", MemberKind.Struct, false);
		mbr_type = CoreLookupType (ctx, "System", "MarshalByRefObject", MemberKind.Class, false);

		generic_ienumerator_type = CoreLookupType (ctx, "System.Collections.Generic", "IEnumerator", 1, MemberKind.Interface, false);
		generic_ilist_type = CoreLookupType (ctx, "System.Collections.Generic", "IList", 1, MemberKind.Interface, false);
		generic_icollection_type = CoreLookupType (ctx, "System.Collections.Generic", "ICollection", 1, MemberKind.Interface, false);
		generic_ienumerable_type = CoreLookupType (ctx, "System.Collections.Generic", "IEnumerable", 1, MemberKind.Interface, false);
		generic_nullable_type = CoreLookupType (ctx, "System", "Nullable", 1, MemberKind.Struct, false);

		//
		// Optional types which are used as types and for member lookup
		//
		runtime_helpers_type = CoreLookupType (ctx, "System.Runtime.CompilerServices", "RuntimeHelpers", MemberKind.Class, false);

		// New in .NET 3.5
		// Note: extension_attribute_type is already loaded
		expression_type = CoreLookupType (ctx, "System.Linq.Expressions", "Expression", 1, MemberKind.Class, false);
	}

	public static bool IsBuiltinType (TypeSpec t)
	{
		if (t == object_type || t == string_type || t == int32_type || t == uint32_type ||
		    t == int64_type || t == uint64_type || t == float_type || t == double_type ||
		    t == char_type || t == short_type || t == decimal_type || t == bool_type ||
		    t == sbyte_type || t == byte_type || t == ushort_type || t == void_type)
			return true;
		else
			return false;
	}

	//
	// This is like IsBuiltinType, but lacks decimal_type, we should also clean up
	// the pieces in the code where we use IsBuiltinType and special case decimal_type.
	// 
	public static bool IsPrimitiveType (TypeSpec t)
	{
		return (t == int32_type || t == uint32_type ||
		    t == int64_type || t == uint64_type || t == float_type || t == double_type ||
		    t == char_type || t == short_type || t == bool_type ||
		    t == sbyte_type || t == byte_type || t == ushort_type);
	}

	// Obsolete
	public static bool IsDelegateType (TypeSpec t)
	{
		return t.IsDelegate;
	}

	//
	// When any element of the type is a dynamic type
	//
	// This method builds a transformation array for dynamic types
	// used in places where DynamicAttribute cannot be applied to.
	// It uses bool flag when type is of dynamic type and each
	// section always starts with "false" for some reason.
	//
	// LAMESPEC: This should be part of C# specification !
	// 
	// Example: Func<dynamic, int, dynamic[]>
	// Transformation: { false, true, false, false, true }
	//
	public static bool[] HasDynamicTypeUsed (TypeSpec t)
	{
		var ac = t as ArrayContainer;
		if (ac != null) {
			if (HasDynamicTypeUsed (ac.Element) != null)
				return new bool[] { false, true };

			return null;
		}

		if (t == null)
			return null;

		if (IsGenericType (t)) {
			List<bool> transform = null;
			var targs = GetTypeArguments (t);
			for (int i = 0; i < targs.Length; ++i) {
				var element = HasDynamicTypeUsed (targs [i]);
				if (element != null) {
					if (transform == null) {
						transform = new List<bool> ();
						for (int ii = 0; ii <= i; ++ii)
							transform.Add (false);
					}

					transform.AddRange (element);
				} else if (transform != null) {
					transform.Add (false);
				}
			}

			if (transform != null)
				return transform.ToArray ();
		}

		if (object.ReferenceEquals (InternalType.Dynamic, t))
			return new bool [] { true };

		return null;
	}
	
	// Obsolete
	public static bool IsEnumType (TypeSpec t)
	{
		return t.IsEnum;
	}

	public static bool IsBuiltinOrEnum (TypeSpec t)
	{
		if (IsBuiltinType (t))
			return true;
		
		if (IsEnumType (t))
			return true;

		return false;
	}

	//
	// Whether a type is unmanaged.  This is used by the unsafe code (25.2)
	//
	public static bool IsUnmanagedType (TypeSpec t)
	{
		var ds = t.MemberDefinition as DeclSpace;
		if (ds != null)
			return ds.IsUnmanagedType ();

		// some builtins that are not unmanaged types
		if (t == TypeManager.object_type || t == TypeManager.string_type)
			return false;

		if (IsBuiltinOrEnum (t))
			return true;

		// Someone did the work of checking if the ElementType of t is unmanaged.  Let's not repeat it.
		if (t.IsPointer)
			return IsUnmanagedType (GetElementType (t));

		if (!IsValueType (t))
			return false;

		if (t.IsNested && t.DeclaringType.IsGenericOrParentIsGeneric)
			return false;

		return true;
	}

	//
	// Null is considered to be a reference type
	//			
	public static bool IsReferenceType (TypeSpec t)
	{
		if (t.IsGenericParameter)
			return ((TypeParameterSpec) t).IsReferenceType;

		return !t.IsStruct && !IsEnumType (t);
	}			
		
	public static bool IsValueType (TypeSpec t)
	{
		if (t.IsGenericParameter)
			return ((TypeParameterSpec) t).IsValueType;

		return t.IsStruct || IsEnumType (t);
	}

	public static bool IsStruct (TypeSpec t)
	{
		return t.IsStruct;
	}

	public static bool IsSubclassOf (TypeSpec type, TypeSpec base_type)
	{
		do {
			if (type == base_type)
				return true;

			type = type.BaseType;
		} while (type != null);

		return false;
	}

	public static bool IsFamilyAccessible (TypeSpec type, TypeSpec parent)
	{
//		TypeParameter tparam = LookupTypeParameter (type);
//		TypeParameter pparam = LookupTypeParameter (parent);

		if (type.Kind == MemberKind.TypeParameter && parent.Kind == MemberKind.TypeParameter) { // (tparam != null) && (pparam != null)) {
			if (type == parent)
				return true;

			throw new NotImplementedException ("net");
//			return tparam.IsSubclassOf (parent);
		}

		do {
			if (IsInstantiationOfSameGenericType (type, parent))
				return true;

			type = type.BaseType;
		} while (type != null);

		return false;
	}

	//
	// Checks whether `type' is a subclass or nested child of `base_type'.
	//
	public static bool IsNestedFamilyAccessible (TypeSpec type, TypeSpec base_type)
	{
		do {
			if (IsFamilyAccessible (type, base_type))
				return true;

			// Handle nested types.
			type = type.DeclaringType;
		} while (type != null);

		return false;
	}

	//
	// Checks whether `type' is a nested child of `parent'.
	//
	public static bool IsNestedChildOf (TypeSpec type, TypeSpec parent)
	{
		if (type == null)
			return false;

		type = type.GetDefinition (); // DropGenericTypeArguments (type);
		parent = parent.GetDefinition (); // DropGenericTypeArguments (parent);

		if (IsEqual (type, parent))
			return false;

		type = type.DeclaringType;
		while (type != null) {
			if (IsEqual (type.GetDefinition (), parent))
				return true;

			type = type.DeclaringType;
		}

		return false;
	}

	public static bool IsSpecialType (TypeSpec t)
	{
		return t == arg_iterator_type || t == typed_reference_type;
	}

	//
	// Checks whether `invocationAssembly' is same or a friend of the assembly
	//
	public static bool IsThisOrFriendAssembly (Assembly invocationAssembly, Assembly assembly)
	{
		if (assembly == null)
			throw new ArgumentNullException ("assembly");

		// TODO: This can happen for constants used at assembly level and
		// predefined members
		// But there is no way to test for it for now, so it could be abused
		// elsewhere too.
		if (invocationAssembly == null)
			invocationAssembly = CodeGen.Assembly.Builder;

		if (invocationAssembly == assembly)
			return true;

		bool value;
		if (assembly_internals_vis_attrs.TryGetValue (assembly, out value))
			return value;

		object[] attrs = assembly.GetCustomAttributes (typeof (InternalsVisibleToAttribute), false);
		if (attrs.Length == 0) {
			assembly_internals_vis_attrs.Add (assembly, false);
			return false;
		}

		bool is_friend = false;

		AssemblyName this_name = CodeGen.Assembly.Name;
		if (this_name == null)
			return false;

		byte [] this_token = this_name.GetPublicKeyToken ();
		foreach (InternalsVisibleToAttribute attr in attrs) {
			if (attr.AssemblyName == null || attr.AssemblyName.Length == 0)
				continue;
			
			AssemblyName aname = null;
			try {
				aname = new AssemblyName (attr.AssemblyName);
			} catch (FileLoadException) {
			} catch (ArgumentException) {
			}

			if (aname == null || aname.Name != this_name.Name)
				continue;
			
			byte [] key_token = aname.GetPublicKeyToken ();
			if (key_token != null) {
				if (this_token.Length == 0) {
					// Same name, but assembly is not strongnamed
					Error_FriendAccessNameNotMatching (aname.FullName, RootContext.ToplevelTypes.Compiler.Report);
					break;
				}
				
				if (!CompareKeyTokens (this_token, key_token))
					continue;
			}

			is_friend = true;
			break;
		}

		assembly_internals_vis_attrs.Add (assembly, is_friend);
		return is_friend;
	}

	static bool CompareKeyTokens (byte [] token1, byte [] token2)
	{
		for (int i = 0; i < token1.Length; i++)
			if (token1 [i] != token2 [i])
				return false;

		return true;
	}

	static void Error_FriendAccessNameNotMatching (string other_name, Report Report)
	{
		Report.Error (281,
			"Friend access was granted to `{0}', but the output assembly is named `{1}'. Try adding a reference to `{0}' or change the output assembly name to match it",
			other_name, CodeGen.Assembly.Name.FullName);
	}

	public static TypeSpec GetElementType (TypeSpec t)
	{
		return ((ElementTypeSpec)t).Element;
	}

	/// <summary>
	/// This method is not implemented by MS runtime for dynamic types
	/// </summary>
	public static bool HasElementType (TypeSpec t)
	{
		return t is ElementTypeSpec;
	}

	static NumberFormatInfo nf_provider = CultureInfo.CurrentCulture.NumberFormat;

	// This is a custom version of Convert.ChangeType() which works
	// with the TypeBuilder defined types when compiling corlib.
	public static object ChangeType (object value, TypeSpec targetType, out bool error)
	{
		IConvertible convert_value = value as IConvertible;
		
		if (convert_value == null){
			error = true;
			return null;
		}
		
		//
		// We cannot rely on build-in type conversions as they are
		// more limited than what C# supports.
		// See char -> float/decimal/double conversion
		//
		error = false;
		try {
			if (targetType == TypeManager.bool_type)
				return convert_value.ToBoolean (nf_provider);
			if (targetType == TypeManager.byte_type)
				return convert_value.ToByte (nf_provider);
			if (targetType == TypeManager.char_type)
				return convert_value.ToChar (nf_provider);
			if (targetType == TypeManager.short_type)
				return convert_value.ToInt16 (nf_provider);
			if (targetType == TypeManager.int32_type)
				return convert_value.ToInt32 (nf_provider);
			if (targetType == TypeManager.int64_type)
				return convert_value.ToInt64 (nf_provider);
			if (targetType == TypeManager.sbyte_type)
				return convert_value.ToSByte (nf_provider);

			if (targetType == TypeManager.decimal_type) {
				if (convert_value.GetType () == typeof (char))
					return (decimal) convert_value.ToInt32 (nf_provider);
				return convert_value.ToDecimal (nf_provider);
			}

			if (targetType == TypeManager.double_type) {
				if (convert_value.GetType () == typeof (char))
					return (double) convert_value.ToInt32 (nf_provider);
				return convert_value.ToDouble (nf_provider);
			}

			if (targetType == TypeManager.float_type) {
				if (convert_value.GetType () == typeof (char))
					return (float)convert_value.ToInt32 (nf_provider);
				return convert_value.ToSingle (nf_provider);
			}

			if (targetType == TypeManager.string_type)
				return convert_value.ToString (nf_provider);
			if (targetType == TypeManager.ushort_type)
				return convert_value.ToUInt16 (nf_provider);
			if (targetType == TypeManager.uint32_type)
				return convert_value.ToUInt32 (nf_provider);
			if (targetType == TypeManager.uint64_type)
				return convert_value.ToUInt64 (nf_provider);
			if (targetType == TypeManager.object_type)
				return value;

			error = true;
		} catch {
			error = true;
		}
		return null;
	}

	/// <summary>
	///   Utility function that can be used to probe whether a type
	///   is managed or not.  
	/// </summary>
	public static bool VerifyUnmanaged (CompilerContext ctx, TypeSpec t, Location loc)
	{
		while (t.IsPointer)
			t = GetElementType (t);

		if (IsUnmanagedType (t))
			return true;

		ctx.Report.SymbolRelatedToPreviousError (t);
		ctx.Report.Error (208, loc,
			"Cannot take the address of, get the size of, or declare a pointer to a managed type `{0}'",
			CSharpName (t));

		return false;	
	}
#region Generics
	// This method always return false for non-generic compiler,
	// while Type.IsGenericParameter is returned if it is supported.
	public static bool IsGenericParameter (TypeSpec type)
	{
		return type.IsGenericParameter;
	}

	public static bool IsGenericType (TypeSpec type)
	{
		return type.IsGeneric;
	}

	// TODO: Implement correctly
	public static bool ContainsGenericParameters (TypeSpec type)
	{
		return type.GetMetaInfo ().ContainsGenericParameters;
	}

	public static bool IsEqual (TypeSpec a, TypeSpec b)
	{
		return a == b && !(a is InternalType);
	}

	public static TypeSpec[] GetTypeArguments (TypeSpec t)
	{
		// TODO: return empty array !!
		return t.TypeArguments;
	}

	/// <summary>
	///   Check whether `type' and `parent' are both instantiations of the same
	///   generic type.  Note that we do not check the type parameters here.
	/// </summary>
	public static bool IsInstantiationOfSameGenericType (TypeSpec type, TypeSpec parent)
	{
		return type == parent || type.MemberDefinition == parent.MemberDefinition;
	}

	public static bool IsNullableType (TypeSpec t)
	{
		return generic_nullable_type == t.GetDefinition ();
	}
#endregion

	//
	// Looks up a member called `name' in the `queried_type'.  This lookup
	// is done by code that is contained in the definition for `invocation_type'
	// through a qualifier of type `qualifier_type' (or null if there is no qualifier).
	//
	// `invocation_type' is used to check whether we're allowed to access the requested
	// member wrt its protection level.
	//
	// When called from MemberAccess, `qualifier_type' is the type which is used to access
	// the requested member (`class B { A a = new A (); a.foo = 5; }'; here invocation_type
	// is B and qualifier_type is A).  This is used to do the CS1540 check.
	//
	// When resolving a SimpleName, `qualifier_type' is null.
	//
	// The `qualifier_type' is used for the CS1540 check; it's normally either null or
	// the same than `queried_type' - except when we're being called from BaseAccess;
	// in this case, `invocation_type' is the current type and `queried_type' the base
	// type, so this'd normally trigger a CS1540.
	//
	// The binding flags are `bf' and the kind of members being looked up are `mt'
	//
	// The return value always includes private members which code in `invocation_type'
	// is allowed to access (using the specified `qualifier_type' if given); only use
	// BindingFlags.NonPublic to bypass the permission check.
	//
	// The 'almost_match' argument is used for reporting error CS1540.
	//
	// Returns an array of a single element for everything but Methods/Constructors
	// that might return multiple matches.
	//
	public static IList<MemberSpec> MemberLookup (TypeSpec invocation_type, TypeSpec qualifier_type,
						  TypeSpec queried_type, MemberKind mt,
						  BindingRestriction opt, string name, int arity, IList<MemberSpec> almost_match)
	{
		Timer.StartTimer (TimerType.MemberLookup);

		var retval = RealMemberLookup (invocation_type, qualifier_type,
							queried_type, mt, opt, name, arity, almost_match);

		Timer.StopTimer (TimerType.MemberLookup);

		return retval;
	}

	static IList<MemberSpec> RealMemberLookup (TypeSpec invocation_type, TypeSpec qualifier_type,
						   TypeSpec queried_type, MemberKind mt,
						   BindingRestriction bf, string name, int arity, IList<MemberSpec> almost_match)
	{
		MemberFilter filter = new MemberFilter (name, arity, mt, null, null);
		if ((bf & BindingRestriction.AccessibleOnly) != 0) {
			filter.InvocationType = invocation_type ?? InternalType.FakeInternalType;
		}

		return MemberCache.FindMembers (queried_type, filter, bf);
	}	
}

}
