//
// parameter.cs: Parameter definition.
//
// Author: Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//
//
//
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;

namespace Mono.CSharp {

	/// <summary>
	///   Abstract Base class for parameters of a method.
	/// </summary>
	public abstract class ParameterBase : Attributable {

		protected ParameterBuilder builder;

		public ParameterBase (Attributes attrs)
			: base (attrs)
		{
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.marshal_as_attr_type) {
				UnmanagedMarshal marshal = a.GetMarshal ();
				if (marshal != null) {
					builder.SetMarshal (marshal);
					return;
				}
				Report.Warning_T (-24, a.Location);
				return;
			}

			builder.SetCustomAttribute (cb);
		}

		public override bool IsClsCompliaceRequired(DeclSpace ds)
		{
			return false;
		}
	}

	/// <summary>
	/// Class for applying custom attributes on the return type
	/// </summary>
	public class ReturnParameter: ParameterBase {
		public ReturnParameter (MethodBuilder mb, Location location):
			base (null)
		{
			try {
				builder = mb.DefineParameter (0, ParameterAttributes.None, "");			
			}
			catch (ArgumentOutOfRangeException) {
				Report.Warning_T (-28, location);
			}
		}

		public override void ApplyAttributeBuilder(Attribute a, CustomAttributeBuilder cb)
		{
			// This occurs after Warning -28
			if (builder == null)
				return;

			base.ApplyAttributeBuilder (a, cb);
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.ReturnValue;
			}
		}

		/// <summary>
		/// Is never called
		/// </summary>
		protected override string[] ValidAttributeTargets {
			get {
				return null;
			}
		}
	}

	/// <summary>
	/// Class for applying custom attributes on the implicit parameter type
	/// of the 'set' method in properties, and the 'add' and 'remove' methods in events.
	/// </summary>
	public class ImplicitParameter: ParameterBase {
		public ImplicitParameter (MethodBuilder mb):
			base (null)
		{
			builder = mb.DefineParameter (1, ParameterAttributes.None, "");			
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Parameter;
			}
		}

		/// <summary>
		/// Is never called
		/// </summary>
		protected override string[] ValidAttributeTargets {
			get {
				return null;
			}
		}
	}

	/// <summary>
	///   Represents a single method parameter
	/// </summary>

	//TODO: Add location member to this or base class for better error location and all methods simplification.
	public class Parameter : ParameterBase {
		[Flags]
		public enum Modifier : byte {
			NONE    = 0,
			REF     = 1,
			OUT     = 2,
			PARAMS  = 4,
			// This is a flag which says that it's either REF or OUT.
			ISBYREF = 8,
			ARGLIST = 16
		}

		static string[] attribute_targets = new string [] { "param" };

		public readonly Expression TypeName;
		public readonly Modifier ModFlags;
		public readonly string Name;
		Type parameter_type;
		
		public Parameter (Expression type, string name, Modifier mod, Attributes attrs)
			: base (attrs)
		{
			Name = name;
			ModFlags = mod;
			TypeName = type;
		}

		// <summary>
		//   Resolve is used in method definitions
		// </summary>
		public bool Resolve (DeclSpace ds, Location l)
		{
			parameter_type = ds.ResolveType (TypeName, false, l);

			if (parameter_type == TypeManager.void_type){
				Report.Error (1536, l, "`void' parameter is not permitted");
				return false;
			}

			if ((ModFlags & Parameter.Modifier.ISBYREF) != 0){
				if (parameter_type == TypeManager.typed_reference_type ||
				    parameter_type == TypeManager.arg_iterator_type){
					Report.Error (1601, l,
						      "out or ref parameter can not be of type TypedReference or ArgIterator");
					return false;
				}
			}
			
			return parameter_type != null;
		}

		public Type ExternalType ()
		{
			if ((ModFlags & Parameter.Modifier.ISBYREF) != 0)
				return TypeManager.GetReferenceType (parameter_type);
			
			return parameter_type;
		}

		public Type ParameterType {
			get {
				return parameter_type;
			}
		}
		
		public ParameterAttributes Attributes {
			get {
				int flags = ((int) ModFlags) & ~((int) Parameter.Modifier.ISBYREF);
				switch ((Modifier) flags) {
				case Modifier.NONE:
					return ParameterAttributes.None;
				case Modifier.REF:
					return ParameterAttributes.None;
				case Modifier.OUT:
					return ParameterAttributes.Out;
				case Modifier.PARAMS:
					return 0;
				}
				
				return ParameterAttributes.None;
			}
		}
		
		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Parameter;
			}
		}

		/// <summary>
		///   Returns the signature for this parameter evaluating it on the
		///   @tc context
		/// </summary>
		public string GetSignature (DeclSpace ds, Location loc)
		{
			if (parameter_type == null){
				if (!Resolve (ds, loc))
					return null;
			}

			return ExternalType ().FullName;
		}

		public string GetSignatureForError ()
		{
			string typeName = TypeManager.CSharpName (parameter_type);
			switch (ModFlags & ~Modifier.ISBYREF) {
				case Modifier.OUT:
					return "out " + typeName;
				case Modifier.PARAMS:
					return "params " + typeName;
				case Modifier.REF:
					return "ref " + typeName;
			}
			return typeName;
		}

		public void DefineParameter (EmitContext ec, MethodBuilder mb, ConstructorBuilder cb, int index, Location loc)
		{
			ParameterAttributes par_attr = Attributes;
					
			if (mb == null)
				builder = cb.DefineParameter (index, par_attr, Name);
			else 
				builder = mb.DefineParameter (index, par_attr, Name);
					
			if (OptAttributes != null) {
				OptAttributes.Emit (ec, this);
	
				if (par_attr == ParameterAttributes.Out){
					if (OptAttributes.Contains (TypeManager.in_attribute_type, ec))
						Report.Error (36, loc,	"Can not use [In] attribute on out parameter");
				}
			}
		}

		protected override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}
	}

	/// <summary>
	///   Represents the methods parameters
	/// </summary>
	public class Parameters {
		public Parameter [] FixedParameters;
		public readonly Parameter ArrayParameter;
		public readonly bool HasArglist;
		string signature;
		Type [] types;
		Location loc;
		
		static Parameters empty_parameters;
		
		public Parameters (Parameter [] fixed_parameters, Parameter array_parameter, Location l)
		{
			FixedParameters = fixed_parameters;
			ArrayParameter  = array_parameter;
			loc = l;
		}

		public Parameters (Parameter [] fixed_parameters, bool has_arglist, Location l)
		{
			FixedParameters = fixed_parameters;
			HasArglist = has_arglist;
			loc = l;
		}

		/// <summary>
		///   This is used to reuse a set of empty parameters, because they
		///   are common
		/// </summary>
		public static Parameters EmptyReadOnlyParameters {
			get {
				if (empty_parameters == null)
					empty_parameters = new Parameters (null, null, Location.Null);
			
				return empty_parameters;
			}
		}
		
		public bool Empty {
			get {
				return (FixedParameters == null) && (ArrayParameter == null);
			}
		}
		
		public void ComputeSignature (DeclSpace ds)
		{
			signature = "";
			if (FixedParameters != null){
				for (int i = 0; i < FixedParameters.Length; i++){
					Parameter par = FixedParameters [i];
					
					signature += par.GetSignature (ds, loc);
				}
			}
			//
			// Note: as per the spec, the `params' arguments (ArrayParameter)
			// are not used in the signature computation for a method
			//
		}

		void Error_DuplicateParameterName (string name)
		{
			Report.Error (
				100, loc, "The parameter name `" + name + "' is a duplicate");
		}
		
		public bool VerifyArgs ()
		{
			int count;
			int i, j;

			if (FixedParameters == null)
				return true;
			
			count = FixedParameters.Length;
			string array_par_name = ArrayParameter != null ? ArrayParameter.Name : null;
			for (i = 0; i < count; i++){
				string base_name = FixedParameters [i].Name;
				
				for (j = i + 1; j < count; j++){
					if (base_name != FixedParameters [j].Name)
						continue;
					Error_DuplicateParameterName (base_name);
					return false;
				}

				if (base_name == array_par_name){
					Error_DuplicateParameterName (base_name);
					return false;
				}
			}
			return true;
		}
		
		/// <summary>
		///    Returns the signature of the Parameters evaluated in
		///    the @tc environment
		/// </summary>
		public string GetSignature (DeclSpace ds)
		{
			if (signature == null){
				VerifyArgs ();
				ComputeSignature (ds);
			}
			
			return signature;
		}
		
		/// <summary>
		///    Returns the paramenter information based on the name
		/// </summary>
		public Parameter GetParameterByName (string name, out int idx)
		{
			idx = 0;
			int i = 0;

			if (FixedParameters != null){
				foreach (Parameter par in FixedParameters){
					if (par.Name == name){
						idx = i;
						return par;
					}
					i++;
				}
			}

			if (ArrayParameter != null){
				if (name == ArrayParameter.Name){
					idx = i;
					return ArrayParameter;
				}
			}
			
			return null;
		}

		bool ComputeParameterTypes (DeclSpace ds)
		{
			int extra = (ArrayParameter != null) ? 1 : 0;
			int i = 0;
			int pc;

			if (FixedParameters == null)
				pc = extra;
			else
				pc = extra + FixedParameters.Length;

			types = new Type [pc];
			
			if (!VerifyArgs ()){
				FixedParameters = null;
				return false;
			}

			bool failed = false;
			if (FixedParameters != null){
				foreach (Parameter p in FixedParameters){
					Type t = null;
					
					if (p.Resolve (ds, loc))
						t = p.ExternalType ();
					else
						failed = true;

					types [i] = t;
					i++;
				}
			}
			
			if (extra > 0){
				if (ArrayParameter.Resolve (ds, loc))
					types [i] = ArrayParameter.ExternalType ();
				else 
					failed = true;
			}

			if (failed){
				types = null;
				return false;
			}

			return true;
		}

		//
		// This variant is used by Delegates, because they need to
		// resolve/define names, instead of the plain LookupType
		//
		public bool ComputeAndDefineParameterTypes (DeclSpace ds)
		{
			int extra = (ArrayParameter != null) ? 1 : 0;
			int i = 0;
			int pc;

			if (FixedParameters == null)
				pc = extra;
			else
				pc = extra + FixedParameters.Length;
			
			types = new Type [pc];
			
			if (!VerifyArgs ()){
				FixedParameters = null;
				return false;
			}

			bool ok_flag = true;
			
			if (FixedParameters != null){
				foreach (Parameter p in FixedParameters){
					Type t = null;
					
					if (p.Resolve (ds, loc))
						t = p.ExternalType ();
					else
						ok_flag = false;
					
					types [i] = t;
					i++;
				}
			}
			
			if (extra > 0){
				if (ArrayParameter.Resolve (ds, loc))
					types [i] = ArrayParameter.ExternalType ();
				else
					ok_flag = false;
			}

			//
			// invalidate the cached types
			//
			if (!ok_flag){
				types = null;
			}
			
			return ok_flag;
		}
		
		/// <summary>
		///   Returns the argument types as an array
		/// </summary>
		static Type [] no_types = new Type [0];
		
		public Type [] GetParameterInfo (DeclSpace ds)
		{
			if (types != null)
				return types;
			
			if (FixedParameters == null && ArrayParameter == null)
				return no_types;

			if (ComputeParameterTypes (ds) == false){
				types = null;
				return null;
			}

			return types;
		}

		/// <summary>
		///   Returns the type of a given parameter, and stores in the `is_out'
		///   boolean whether this is an out or ref parameter.
		///
		///   Note that the returned type will not contain any dereference in this
		///   case (ie, you get "int" for a ref int instead of "int&"
		/// </summary>
		public Type GetParameterInfo (DeclSpace ds, int idx, out Parameter.Modifier mod)
		{
			mod = Parameter.Modifier.NONE;
			
			if (!VerifyArgs ()){
				FixedParameters = null;
				return null;
			}

			if (FixedParameters == null && ArrayParameter == null)
				return null;
			
			if (types == null)
				if (ComputeParameterTypes (ds) == false)
					return null;

			//
			// If this is a request for the variable lenght arg.
			//
			int array_idx = (FixedParameters != null ? FixedParameters.Length : 0);
			if (idx == array_idx)
				return types [idx];

			//
			// Otherwise, it is a fixed parameter
			//
			Parameter p = FixedParameters [idx];
			mod = p.ModFlags;

			if ((mod & (Parameter.Modifier.REF | Parameter.Modifier.OUT)) != 0)
				mod |= Parameter.Modifier.ISBYREF;

			return p.ParameterType;
		}

		public CallingConventions GetCallingConvention ()
		{
			if (HasArglist)
				return CallingConventions.VarArgs;
			else
				return CallingConventions.Standard;
		}

		//
		// The method's attributes are passed in because we need to extract
		// the "return:" attribute from there to apply on the return type
		//
		public void LabelParameters (EmitContext ec,
			MethodBase builder,
			Location loc) {
			//
			// Define each type attribute (in/out/ref) and
			// the argument names.
			//
			int i = 0;
			
			MethodBuilder mb = builder as MethodBuilder;
			ConstructorBuilder cb = builder as ConstructorBuilder;

			if (FixedParameters != null) {
				for (i = 0; i < FixedParameters.Length; i++) {
					FixedParameters [i].DefineParameter (ec, mb, cb, i + 1, loc);
				}
			}

			if (ArrayParameter != null){
				ParameterBuilder pb;
				Parameter array_param = ArrayParameter;

				if (mb == null)
					pb = cb.DefineParameter (
						i + 1, array_param.Attributes,
						array_param.Name);
				else
					pb = mb.DefineParameter (
						i + 1, array_param.Attributes,
						array_param.Name);
					
				CustomAttributeBuilder a = new CustomAttributeBuilder (
					TypeManager.cons_param_array_attribute, new object [0]);
				
				pb.SetCustomAttribute (a);
			}
		}
	}
}
