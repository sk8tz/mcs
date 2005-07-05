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
using System.Text;

namespace Mono.CSharp {

	/// <summary>
	///   Abstract Base class for parameters of a method.
	/// </summary>
	public abstract class ParameterBase : Attributable {

		protected ParameterBuilder builder;
		public readonly Location Location;

		public ParameterBase (Attributes attrs, Location loc)
			: base (attrs)
		{
			Location = loc;
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.marshal_as_attr_type) {
				UnmanagedMarshal marshal = a.GetMarshal (this);
				if (marshal != null) {
					builder.SetMarshal (marshal);
				}
				return;
			}

			if (a.Type.IsSubclassOf (TypeManager.security_attr_type)) {
				a.Error_InvalidSecurityParent ();
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
			base (null, location)
		{
			try {
				builder = mb.DefineParameter (0, ParameterAttributes.None, "");			
			}
			catch (ArgumentOutOfRangeException) {
				Report.Warning (-24, location, "The Microsoft .NET Runtime 1.x does not permit setting custom attributes on the return type");
			}
		}

		public override void ApplyAttributeBuilder(Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.cls_compliant_attribute_type) {
				Report.Warning (3023, 1, a.Location, "CLSCompliant attribute has no meaning when applied to return types. Try putting it on the method instead");
			}

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
		public override string[] ValidAttributeTargets {
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
		public ImplicitParameter (MethodBuilder mb, Location loc):
			base (null, loc)
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
		public override string[] ValidAttributeTargets {
			get {
				return null;
			}
		}
	}

	/// <summary>
	///   Represents a single method parameter
	/// </summary>
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

		public Expression TypeName;
		public readonly Modifier ModFlags;
		public readonly string Name;
		Type parameter_type;

		EmitContext ec;  // because ApplyAtrribute doesn't have ec
		
		public Parameter (Expression type, string name, Modifier mod, Attributes attrs, Location loc)
			: base (attrs, loc)
		{
			Name = name;
			ModFlags = mod;
			TypeName = type;
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb)
		{
			if (a.Type == TypeManager.in_attribute_type && Attributes == ParameterAttributes.Out) {
				Report.Error (36, a.Location, "An out parameter cannot have the `In' attribute");
				return;
			}

			if (a.Type == TypeManager.param_array_type) {
				Report.Error (674, a.Location, "Do not use `System.ParamArrayAttribute'. Use the `params' keyword instead");
				return;
			}

			if (a.Type == TypeManager.out_attribute_type && (ModFlags & Modifier.REF) != 0 &&
			    !OptAttributes.Contains (TypeManager.in_attribute_type, ec)) {
				Report.Error (662, a.Location,
					"Cannot specify only `Out' attribute on a ref parameter. Use both `In' and `Out' attributes or neither");
				return;
			}

			if (a.Type == TypeManager.cls_compliant_attribute_type) {
				Report.Warning (3022, 1, a.Location, "CLSCompliant attribute has no meaning when applied to parameters. Try putting it on the method instead");
			}

			base.ApplyAttributeBuilder (a, cb);
		}

		// <summary>
		//   Resolve is used in method definitions
		// </summary>
		public bool Resolve (EmitContext ec)
		{
			this.ec = ec;

			TypeExpr texpr = TypeName.ResolveAsTypeTerminal (ec, false);
			if (texpr == null)
				return false;

			parameter_type = texpr.ResolveType (ec);
			
			if (parameter_type.IsAbstract && parameter_type.IsSealed) {
				Report.Error (721, Location, "`{0}': static types cannot be used as parameters", GetSignatureForError ());
				return false;
			}

			if (parameter_type == TypeManager.void_type){
				Report.Error (1536, Location, "Invalid parameter type 'void'");
				return false;
			}

			if ((ModFlags & Parameter.Modifier.ISBYREF) != 0){
				if (parameter_type == TypeManager.typed_reference_type ||
				    parameter_type == TypeManager.arg_iterator_type){
					Report.Error (1601, Location, "Method or delegate parameter cannot be of type `{0}'",
						GetSignatureForError ());
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
		public string GetSignature (EmitContext ec)
		{
			if (parameter_type == null){
				if (!Resolve (ec))
					return null;
			}

			return ExternalType ().FullName;
		}

		public string GetSignatureForError ()
		{
			string type_name;
			if (parameter_type != null)
				type_name = TypeManager.CSharpName (parameter_type);
			else if (TypeName.Type != null)
				type_name = TypeManager.CSharpName (TypeName.Type);
			else
				type_name = TypeName.ToString ();

			string mod = GetModifierSignature (ModFlags);
			if (mod.Length > 0)
				return String.Concat (mod, " ", type_name);

			return type_name;
		}

		public static string GetModifierSignature (Modifier mod)
		{
			switch (mod & unchecked (~Modifier.ISBYREF)) {
				case Modifier.OUT:
					return "out";
				case Modifier.PARAMS:
					return "params";
				case Modifier.REF:
					return "ref";
				case Modifier.ARGLIST:
					return "__arglist";
				default:
					return "";
			}
		}

		public void DefineParameter (EmitContext ec, MethodBuilder mb, ConstructorBuilder cb, int index)
		{
			ParameterAttributes par_attr = Attributes;
					
			if (mb == null)
				builder = cb.DefineParameter (index, par_attr, Name);
			else 
				builder = mb.DefineParameter (index, par_attr, Name);
					
			if (OptAttributes != null) {
				OptAttributes.Emit (ec, this);
			}
		}

		public override string[] ValidAttributeTargets {
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
		
		static Parameters empty_parameters;
		
		public Parameters (Parameter [] fixed_parameters, Parameter array_parameter)
		{
			FixedParameters = fixed_parameters;
			ArrayParameter  = array_parameter;
		}

		public Parameters (Parameter [] fixed_parameters, bool has_arglist)
		{
			FixedParameters = fixed_parameters;
			HasArglist = has_arglist;
		}

		/// <summary>
		///   This is used to reuse a set of empty parameters, because they
		///   are common
		/// </summary>
		public static Parameters EmptyReadOnlyParameters {
			get {
				if (empty_parameters == null)
					empty_parameters = new Parameters (null, null);
			
				return empty_parameters;
			}
		}
		
		public bool Empty {
			get {
				return (FixedParameters == null) && (ArrayParameter == null);
			}
		}

		public void ComputeSignature (EmitContext ec)
		{
			signature = "";
			if (FixedParameters != null){
				for (int i = 0; i < FixedParameters.Length; i++){
					Parameter par = FixedParameters [i];
					
					signature += par.GetSignature (ec);
				}
			}
			//
			// Note: as per the spec, the `params' arguments (ArrayParameter)
			// are not used in the signature computation for a method
			//
		}

		void Error_DuplicateParameterName (string name, Location loc)
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
					Error_DuplicateParameterName (base_name,
						FixedParameters [i].Location);
					return false;
				}

				if (base_name == array_par_name){
					Error_DuplicateParameterName (base_name,
						FixedParameters [i].Location);
					return false;
				}
			}
			return true;
		}
		
		/// <summary>
		///    Returns the signature of the Parameters evaluated in
		///    the @ec EmitContext
		/// </summary>
		public string GetSignature (EmitContext ec)
		{
			if (signature == null){
				VerifyArgs ();
				ComputeSignature (ec);
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

		public Parameter GetParameterByName (string name)
		{
			int idx;

			return GetParameterByName (name, out idx);
		}
		
		bool ComputeParameterTypes (EmitContext ec)
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
					
					if (p.Resolve (ec))
						t = p.ExternalType ();
					else
						failed = true;

					types [i] = t;
					i++;
				}
			}
			
			if (extra > 0){
				if (ArrayParameter.Resolve (ec))
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
		
		/// <summary>
		///   Returns the argument types as an array
		/// </summary>
		static Type [] no_types = new Type [0];
		
		public Type [] GetParameterInfo (EmitContext ec)
		{
			if (types != null)
				return types;
			
			if (FixedParameters == null && ArrayParameter == null)
				return no_types;

			if (ComputeParameterTypes (ec) == false){
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
		public Type GetParameterInfo (EmitContext ec, int idx, out Parameter.Modifier mod)
		{
			mod = Parameter.Modifier.NONE;
			
			if (!VerifyArgs ()){
				FixedParameters = null;
				return null;
			}

			if (FixedParameters == null && ArrayParameter == null)
				return null;
			
			if (types == null)
				if (ComputeParameterTypes (ec) == false)
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
		public void LabelParameters (EmitContext ec, MethodBase builder)
		{
			//
			// Define each type attribute (in/out/ref) and
			// the argument names.
			//
			int i = 0;
			
			MethodBuilder mb = builder as MethodBuilder;
			ConstructorBuilder cb = builder as ConstructorBuilder;

			if (FixedParameters != null) {
				for (i = 0; i < FixedParameters.Length; i++) {
					FixedParameters [i].DefineParameter (ec, mb, cb, i + 1);
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

		public string GetSignatureForError ()
		{
			StringBuilder sb = new StringBuilder ("(");
			if (FixedParameters != null) {
				for (int i = 0; i < FixedParameters.Length; ++i) {
					sb.Append (FixedParameters[i].GetSignatureForError ());
					if (i < FixedParameters.Length - 1)
						sb.Append (", ");
				}
			}
			sb.Append (')');
			return sb.ToString ();

		}
	}
}
