//
// ecore.cs: Core of the Expression representation for the intermediate tree.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Marek Safar (marek.safar@seznam.cz)
//
// (C) 2001, 2002, 2003 Ximian, Inc.
//
//

namespace Mono.CSharp {
	using System;
	using System.Collections;
	using System.Diagnostics;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Text;

	/// <remarks>
	///   The ExprClass class contains the is used to pass the 
	///   classification of an expression (value, variable, namespace,
	///   type, method group, property access, event access, indexer access,
	///   nothing).
	/// </remarks>
	public enum ExprClass : byte {
		Invalid,
		
		Value,
		Variable,
		Namespace,
		Type,
		MethodGroup,
		PropertyAccess,
		EventAccess,
		IndexerAccess,
		Nothing, 
	}

	/// <remarks>
	///   This is used to tell Resolve in which types of expressions we're
	///   interested.
	/// </remarks>
	[Flags]
	public enum ResolveFlags {
		// Returns Value, Variable, PropertyAccess, EventAccess or IndexerAccess.
		VariableOrValue		= 1,

		// Returns a type expression.
		Type			= 2,

		// Returns a method group.
		MethodGroup		= 4,

		// Mask of all the expression class flags.
		MaskExprClass		= 7,

		// Disable control flow analysis while resolving the expression.
		// This is used when resolving the instance expression of a field expression.
		DisableFlowAnalysis	= 8,

		// Set if this is resolving the first part of a MemberAccess.
		Intermediate		= 16,

		// Disable control flow analysis _of struct_ while resolving the expression.
		// This is used when resolving the instance expression of a field expression.
		DisableStructFlowAnalysis	= 32,

	}

	//
	// This is just as a hint to AddressOf of what will be done with the
	// address.
	[Flags]
	public enum AddressOp {
		Store = 1,
		Load  = 2,
		LoadStore = 3
	};
	
	/// <summary>
	///   This interface is implemented by variables
	/// </summary>
	public interface IMemoryLocation {
		/// <summary>
		///   The AddressOf method should generate code that loads
		///   the address of the object and leaves it on the stack.
		///
		///   The `mode' argument is used to notify the expression
		///   of whether this will be used to read from the address or
		///   write to the address.
		///
		///   This is just a hint that can be used to provide good error
		///   reporting, and should have no other side effects. 
		/// </summary>
		void AddressOf (EmitContext ec, AddressOp mode);
	}

	/// <summary>
	///   This interface is implemented by variables
	/// </summary>
	public interface IVariable {
		VariableInfo VariableInfo {
			get;
		}

		bool VerifyFixed ();
	}

	/// <remarks>
	///   Base class for expressions
	/// </remarks>
	public abstract class Expression {
		public ExprClass eclass;
		protected Type type;
		protected Location loc;
		
		public Type Type {
			get { return type; }
			set { type = value; }
		}

		public virtual Location Location {
			get { return loc; }
		}

		/// <summary>
		///   Utility wrapper routine for Error, just to beautify the code
		/// </summary>
		public void Error (int error, string s)
		{
			Report.Error (error, loc, s);
		}

		// Not nice but we have broken hierarchy.
		public virtual void CheckMarshalByRefAccess ()
		{
		}

		public virtual bool GetAttributableValue (Type valueType, out object value)
		{
			Attribute.Error_AttributeArgumentNotValid (loc);
			value = null;
			return false;
		}

		public virtual string GetSignatureForError ()
		{
			return TypeManager.CSharpName (type);
		}

		public static bool IsAccessorAccessible (Type invocation_type, MethodInfo mi, out bool must_do_cs1540_check)
		{
			MethodAttributes ma = mi.Attributes & MethodAttributes.MemberAccessMask;

			must_do_cs1540_check = false; // by default we do not check for this

			if (ma == MethodAttributes.Public)
				return true;
			
			//
			// If only accessible to the current class or children
			//
			if (ma == MethodAttributes.Private)
				return TypeManager.IsPrivateAccessible (invocation_type, mi.DeclaringType) ||
					TypeManager.IsNestedChildOf (invocation_type, mi.DeclaringType);

			if (mi.DeclaringType.Assembly == invocation_type.Assembly ||
					TypeManager.IsFriendAssembly (mi.DeclaringType.Assembly)) {
				if (ma == MethodAttributes.Assembly || ma == MethodAttributes.FamORAssem)
					return true;
			} else {
				if (ma == MethodAttributes.Assembly || ma == MethodAttributes.FamANDAssem)
					return false;
			}

			// Family and FamANDAssem require that we derive.
			// FamORAssem requires that we derive if in different assemblies.
			if (!TypeManager.IsNestedFamilyAccessible (invocation_type, mi.DeclaringType))
				return false;

			if (!TypeManager.IsNestedChildOf (invocation_type, mi.DeclaringType))
				must_do_cs1540_check = true;

			return true;
		}

		/// <summary>
		///   Performs semantic analysis on the Expression
		/// </summary>
		///
		/// <remarks>
		///   The Resolve method is invoked to perform the semantic analysis
		///   on the node.
		///
		///   The return value is an expression (it can be the
		///   same expression in some cases) or a new
		///   expression that better represents this node.
		///   
		///   For example, optimizations of Unary (LiteralInt)
		///   would return a new LiteralInt with a negated
		///   value.
		///   
		///   If there is an error during semantic analysis,
		///   then an error should be reported (using Report)
		///   and a null value should be returned.
		///   
		///   There are two side effects expected from calling
		///   Resolve(): the the field variable "eclass" should
		///   be set to any value of the enumeration
		///   `ExprClass' and the type variable should be set
		///   to a valid type (this is the type of the
		///   expression).
		/// </remarks>
		public abstract Expression DoResolve (EmitContext ec);

		public virtual Expression DoResolveLValue (EmitContext ec, Expression right_side)
		{
			return null;
		}

		//
		// This is used if the expression should be resolved as a type or namespace name.
		// the default implementation fails.   
		//
		public virtual FullNamedExpression ResolveAsTypeStep (IResolveContext ec,  bool silent)
		{
			return null;
		}

		//
		// This is used to resolve the expression as a type, a null
		// value will be returned if the expression is not a type
		// reference
		//
		public virtual TypeExpr ResolveAsTypeTerminal (IResolveContext ec, bool silent)
		{
			TypeExpr te = ResolveAsBaseTerminal (ec, silent);
			if (te == null)
				return null;

			if (!silent) { // && !(te is TypeParameterExpr)) {
				ObsoleteAttribute obsolete_attr = AttributeTester.GetObsoleteAttribute (te.Type);
				if (obsolete_attr != null && !ec.IsInObsoleteScope) {
					AttributeTester.Report_ObsoleteMessage (obsolete_attr, te.GetSignatureForError (), Location);
				}
			}

			// Constrains don't need to be checked for overrides
			GenericMethod gm = ec.GenericDeclContainer as GenericMethod;
			if (gm != null && (gm.ModFlags & Modifiers.OVERRIDE) != 0) {
				te.loc = loc;
				return te;
			}

			ConstructedType ct = te as ConstructedType;
			if ((ct != null) && !ct.CheckConstraints (ec))
				return null;

			return te;
		}

		public TypeExpr ResolveAsBaseTerminal (IResolveContext ec, bool silent)
		{
			int errors = Report.Errors;

			FullNamedExpression fne = ResolveAsTypeStep (ec, silent);

			if (fne == null)
				return null;

			if (fne.eclass != ExprClass.Type) {
				if (!silent && errors == Report.Errors)
					fne.Error_UnexpectedKind (null, "type", loc);
				return null;
			}

			TypeExpr te = fne as TypeExpr;

			if (!te.CheckAccessLevel (ec.DeclContainer)) {
				Report.SymbolRelatedToPreviousError (te.Type);
				ErrorIsInaccesible (loc, TypeManager.CSharpName (te.Type));
				return null;
			}

			te.loc = loc;
			return te;
		}

		public static void ErrorIsInaccesible (Location loc, string name)
		{
			Report.Error (122, loc, "`{0}' is inaccessible due to its protection level", name);
		}

		protected static void Error_CannotAccessProtected (Location loc, MemberInfo m, Type qualifier, Type container)
		{
			Report.Error (1540, loc, "Cannot access protected member `{0}' via a qualifier of type `{1}'."
				+ " The qualifier must be of type `{2}' or derived from it", 
				TypeManager.GetFullNameSignature (m),
				TypeManager.CSharpName (qualifier),
				TypeManager.CSharpName (container));

		}

		public static void Error_InvalidExpressionStatement (Location loc)
		{
			Report.Error (201, loc, "Only assignment, call, increment, decrement, and new object " +
				       "expressions can be used as a statement");
		}
		
		public void Error_InvalidExpressionStatement ()
		{
			Error_InvalidExpressionStatement (loc);
		}

		protected void Error_CannotAssign (string to, string roContext)
		{
			Report.Error (1656, loc, "Cannot assign to `{0}' because it is a `{1}'",
				to, roContext);
		}

		public static void Error_VoidInvalidInTheContext (Location loc)
		{
			Report.Error (1547, loc, "Keyword `void' cannot be used in this context");
		}

		public virtual void Error_ValueCannotBeConverted (EmitContext ec, Location loc, Type target, bool expl)
		{
			if (Type.FullName == target.FullName){
				Report.ExtraInformation (loc,
					String.Format (
					"The type {0} has two conflicting definitions, one comes from {1} and the other from {2}",
					Type.FullName, Type.Assembly.FullName, target.Assembly.FullName));
							 
			}

			if (expl) {
				Report.Error (30, loc, "Cannot convert type `{0}' to `{1}'",
					GetSignatureForError (), TypeManager.CSharpName (target));
				return;
			}
			
			Expression e = (this is EnumConstant) ? ((EnumConstant)this).Child : this;
			bool b = Convert.ExplicitNumericConversion (e, target) != null;

			if (b ||
			    Convert.ExplicitReferenceConversionExists (Type, target) ||
			    Convert.ExplicitUnsafe (e, target) != null ||
			    (ec != null && Convert.UserDefinedConversion (ec, this, target, Location.Null, true) != null))
			{
				Report.Error (266, loc, "Cannot implicitly convert type `{0}' to `{1}'. " +
					      "An explicit conversion exists (are you missing a cast?)",
					TypeManager.CSharpName (Type), TypeManager.CSharpName (target));
				return;
			}

			if (Type != TypeManager.string_type && this is Constant && !(this is EmptyConstantCast)) {
				Report.Error (31, loc, "Constant value `{0}' cannot be converted to a `{1}'",
					((Constant)(this)).GetValue ().ToString (), TypeManager.CSharpName (target));
				return;
			}

			Report.Error (29, loc, "Cannot implicitly convert type {0} to `{1}'",
				Type == TypeManager.anonymous_method_type ?
				"anonymous method" : "`" + GetSignatureForError () + "'",
				TypeManager.CSharpName (target));
		}

		public static void Error_TypeDoesNotContainDefinition (Location loc, Type type, string name)
		{
			Report.Error (117, loc, "`{0}' does not contain a definition for `{1}'",
				TypeManager.CSharpName (type), name);
		}

		protected static void Error_ValueAssignment (Location loc)
		{
			Report.Error (131, loc, "The left-hand side of an assignment must be a variable, a property or an indexer");
		}

		ResolveFlags ExprClassToResolveFlags
		{
			get {
				switch (eclass) {
					case ExprClass.Type:
					case ExprClass.Namespace:
						return ResolveFlags.Type;

					case ExprClass.MethodGroup:
						return ResolveFlags.MethodGroup;

					case ExprClass.Value:
					case ExprClass.Variable:
					case ExprClass.PropertyAccess:
					case ExprClass.EventAccess:
					case ExprClass.IndexerAccess:
						return ResolveFlags.VariableOrValue;

					default:
						throw new Exception ("Expression " + GetType () +
							" ExprClass is Invalid after resolve");
				}
			}
		}
	       
		/// <summary>
		///   Resolves an expression and performs semantic analysis on it.
		/// </summary>
		///
		/// <remarks>
		///   Currently Resolve wraps DoResolve to perform sanity
		///   checking and assertion checking on what we expect from Resolve.
		/// </remarks>
		public Expression Resolve (EmitContext ec, ResolveFlags flags)
		{
			if ((flags & ResolveFlags.MaskExprClass) == ResolveFlags.Type) 
				return ResolveAsTypeStep (ec, false);

			bool do_flow_analysis = ec.DoFlowAnalysis;
			bool omit_struct_analysis = ec.OmitStructFlowAnalysis;
			if ((flags & ResolveFlags.DisableFlowAnalysis) != 0)
				do_flow_analysis = false;
			if ((flags & ResolveFlags.DisableStructFlowAnalysis) != 0)
				omit_struct_analysis = true;

			Expression e;
			using (ec.WithFlowAnalysis (do_flow_analysis, omit_struct_analysis)) {
				if (this is SimpleName) {
					bool intermediate = (flags & ResolveFlags.Intermediate) == ResolveFlags.Intermediate;
					e = ((SimpleName) this).DoResolve (ec, intermediate);
				} else {
					e = DoResolve (ec);
				}
			}

			if (e == null)
				return null;

			if ((flags & e.ExprClassToResolveFlags) == 0) {
				e.Error_UnexpectedKind (flags, loc);
				return null;
			}

			if (e.type == null && !(e is Namespace)) {
				throw new Exception (
					"Expression " + e.GetType () +
					" did not set its type after Resolve\n" +
					"called from: " + this.GetType ());
			}

			return e;
		}

		/// <summary>
		///   Resolves an expression and performs semantic analysis on it.
		/// </summary>
		public Expression Resolve (EmitContext ec)
		{
			Expression e = Resolve (ec, ResolveFlags.VariableOrValue | ResolveFlags.MethodGroup);

			if (e != null && e.eclass == ExprClass.MethodGroup && RootContext.Version == LanguageVersion.ISO_1) {
				((MethodGroupExpr) e).ReportUsageError ();
				return null;
			}
			return e;
		}

		public Constant ResolveAsConstant (EmitContext ec, MemberCore mc)
		{
			Expression e = Resolve (ec);
			if (e == null)
				return null;

			Constant c = e as Constant;
			if (c != null)
				return c;

			Const.Error_ExpressionMustBeConstant (loc, mc.GetSignatureForError ());
			return null;
		}

		/// <summary>
		///   Resolves an expression for LValue assignment
		/// </summary>
		///
		/// <remarks>
		///   Currently ResolveLValue wraps DoResolveLValue to perform sanity
		///   checking and assertion checking on what we expect from Resolve
		/// </remarks>
		public Expression ResolveLValue (EmitContext ec, Expression right_side, Location loc)
		{
			int errors = Report.Errors;
			bool out_access = right_side == EmptyExpression.OutAccess;

			Expression e = DoResolveLValue (ec, right_side);

			if (e != null && out_access && !(e is IMemoryLocation)) {
				// FIXME: There's no problem with correctness, the 'Expr = null' handles that.
				//        Enabling this 'throw' will "only" result in deleting useless code elsewhere,

				//throw new InternalErrorException ("ResolveLValue didn't return an IMemoryLocation: " +
				//				  e.GetType () + " " + e.GetSignatureForError ());
				e = null;
			}

			if (e == null) {
				if (errors == Report.Errors) {
					if (out_access)
						Report.Error (1510, loc, "A ref or out argument must be an assignable variable");
					else
						Error_ValueAssignment (loc);
				}
				return null;
			}

			if (e.eclass == ExprClass.Invalid)
				throw new Exception ("Expression " + e + " ExprClass is Invalid after resolve");

			if (e.eclass == ExprClass.MethodGroup) {
				((MethodGroupExpr) e).ReportUsageError ();
				return null;
			}

			if ((e.type == null) && !(e is ConstructedType))
				throw new Exception ("Expression " + e + " did not set its type after Resolve");

			return e;
		}

		/// <summary>
		///   Emits the code for the expression
		/// </summary>
		///
		/// <remarks>
		///   The Emit method is invoked to generate the code
		///   for the expression.  
		/// </remarks>
		public abstract void Emit (EmitContext ec);

		public virtual void EmitBranchable (EmitContext ec, Label target, bool onTrue)
		{
			Emit (ec);
			ec.ig.Emit (onTrue ? OpCodes.Brtrue : OpCodes.Brfalse, target);
		}

		/// <summary>
		///   Protected constructor.  Only derivate types should
		///   be able to be created
		/// </summary>

		protected Expression ()
		{
			eclass = ExprClass.Invalid;
			type = null;
		}

		/// <summary>
		///   Returns a fully formed expression after a MemberLookup
		/// </summary>
		/// 
		public static Expression ExprClassFromMemberInfo (Type containerType, MemberInfo mi, Location loc)
		{
			if (mi is EventInfo)
				return new EventExpr ((EventInfo) mi, loc);
			else if (mi is FieldInfo)
				return new FieldExpr ((FieldInfo) mi, loc);
			else if (mi is PropertyInfo)
				return new PropertyExpr (containerType, (PropertyInfo) mi, loc);
		        else if (mi is Type){
				return new TypeExpression ((System.Type) mi, loc);
			}

			return null;
		}

		protected static ArrayList almostMatchedMembers = new ArrayList (4);

		//
		// FIXME: Probably implement a cache for (t,name,current_access_set)?
		//
		// This code could use some optimizations, but we need to do some
		// measurements.  For example, we could use a delegate to `flag' when
		// something can not any longer be a method-group (because it is something
		// else).
		//
		// Return values:
		//     If the return value is an Array, then it is an array of
		//     MethodBases
		//   
		//     If the return value is an MemberInfo, it is anything, but a Method
		//
		//     null on error.
		//
		// FIXME: When calling MemberLookup inside an `Invocation', we should pass
		// the arguments here and have MemberLookup return only the methods that
		// match the argument count/type, unlike we are doing now (we delay this
		// decision).
		//
		// This is so we can catch correctly attempts to invoke instance methods
		// from a static body (scan for error 120 in ResolveSimpleName).
		//
		//
		// FIXME: Potential optimization, have a static ArrayList
		//

		public static Expression MemberLookup (Type container_type, Type queried_type, string name,
						       MemberTypes mt, BindingFlags bf, Location loc)
		{
			return MemberLookup (container_type, null, queried_type, name, mt, bf, loc);
		}

		//
		// Lookup type `queried_type' for code in class `container_type' with a qualifier of
		// `qualifier_type' or null to lookup members in the current class.
		//

		public static Expression MemberLookup (Type container_type,
						       Type qualifier_type, Type queried_type,
						       string name, MemberTypes mt,
						       BindingFlags bf, Location loc)
		{
			almostMatchedMembers.Clear ();

			MemberInfo [] mi = TypeManager.MemberLookup (container_type, qualifier_type,
								     queried_type, mt, bf, name, almostMatchedMembers);

			if (mi == null)
				return null;

			if (mi.Length > 1) {
				bool is_interface = qualifier_type != null && qualifier_type.IsInterface;
				ArrayList methods = new ArrayList (2);
				ArrayList non_methods = null;

				foreach (MemberInfo m in mi) {
					if (m is MethodBase) {
						methods.Add (m);
						continue;
					}

					if (non_methods == null) {
						non_methods = new ArrayList (2);
						non_methods.Add (m);
						continue;
					}

					foreach (MemberInfo n_m in non_methods) {
						if (m.DeclaringType.IsInterface && TypeManager.ImplementsInterface (m.DeclaringType, n_m.DeclaringType))
							continue;

						Report.SymbolRelatedToPreviousError (m);
						Report.Error (229, loc, "Ambiguity between `{0}' and `{1}'",
							TypeManager.GetFullNameSignature (m), TypeManager.GetFullNameSignature (n_m));
						return null;
					}
				}

				if (methods.Count == 0)
					return ExprClassFromMemberInfo (container_type, (MemberInfo)non_methods [0], loc);

				if (non_methods != null) {
					MethodBase method = (MethodBase) methods [0];
					MemberInfo non_method = (MemberInfo) non_methods [0];
					if (method.DeclaringType == non_method.DeclaringType) {
						// Cannot happen with C# code, but is valid in IL
						Report.SymbolRelatedToPreviousError (method);
						Report.SymbolRelatedToPreviousError (non_method);
						Report.Error (229, loc, "Ambiguity between `{0}' and `{1}'",
							      TypeManager.GetFullNameSignature (non_method),
							      TypeManager.CSharpSignature (method));
						return null;
					}

					if (is_interface) {
						Report.SymbolRelatedToPreviousError (method);
						Report.SymbolRelatedToPreviousError (non_method);
						Report.Warning (467, 2, loc, "Ambiguity between method `{0}' and non-method `{1}'. Using method `{0}'",
								TypeManager.CSharpSignature (method), TypeManager.GetFullNameSignature (non_method));
					}
				}

				return new MethodGroupExpr (methods, loc);
			}

			if (mi [0] is MethodBase)
				return new MethodGroupExpr (mi, loc);

			return ExprClassFromMemberInfo (container_type, mi [0], loc);
		}

		public const MemberTypes AllMemberTypes =
			MemberTypes.Constructor |
			MemberTypes.Event       |
			MemberTypes.Field       |
			MemberTypes.Method      |
			MemberTypes.NestedType  |
			MemberTypes.Property;
		
		public const BindingFlags AllBindingFlags =
			BindingFlags.Public |
			BindingFlags.Static |
			BindingFlags.Instance;

		public static Expression MemberLookup (Type container_type, Type queried_type,
						       string name, Location loc)
		{
			return MemberLookup (container_type, null, queried_type, name,
					     AllMemberTypes, AllBindingFlags, loc);
		}

		public static Expression MemberLookup (Type container_type, Type qualifier_type,
						       Type queried_type, string name, Location loc)
		{
			return MemberLookup (container_type, qualifier_type, queried_type,
					     name, AllMemberTypes, AllBindingFlags, loc);
		}

		public static MethodGroupExpr MethodLookup (Type container_type, Type queried_type,
						       string name, Location loc)
		{
			return (MethodGroupExpr)MemberLookup (container_type, null, queried_type, name,
					     MemberTypes.Method, AllBindingFlags, loc);
		}

		/// <summary>
		///   This is a wrapper for MemberLookup that is not used to "probe", but
		///   to find a final definition.  If the final definition is not found, we
		///   look for private members and display a useful debugging message if we
		///   find it.
		/// </summary>
		public static Expression MemberLookupFinal (EmitContext ec, Type qualifier_type,
							    Type queried_type, string name, Location loc)
		{
			return MemberLookupFinal (ec, qualifier_type, queried_type, name,
						  AllMemberTypes, AllBindingFlags, loc);
		}

		public static Expression MemberLookupFinal (EmitContext ec, Type qualifier_type,
							    Type queried_type, string name,
							    MemberTypes mt, BindingFlags bf,
							    Location loc)
		{
			Expression e;

			int errors = Report.Errors;

			e = MemberLookup (ec.ContainerType, qualifier_type, queried_type, name, mt, bf, loc);

			if (e == null && errors == Report.Errors)
				// No errors were reported by MemberLookup, but there was an error.
				MemberLookupFailed (ec.ContainerType, qualifier_type, queried_type, name, null, true, loc);

			return e;
		}

		public static void MemberLookupFailed (Type container_type, Type qualifier_type,
						       Type queried_type, string name,
						       string class_name, bool complain_if_none_found, 
						       Location loc)
		{
			if (almostMatchedMembers.Count != 0) {
				for (int i = 0; i < almostMatchedMembers.Count; ++i) {
					MemberInfo m = (MemberInfo) almostMatchedMembers [i];
					for (int j = 0; j < i; ++j) {
						if (m == almostMatchedMembers [j]) {
							m = null;
							break;
						}
					}
					if (m == null)
						continue;
					
					Type declaring_type = m.DeclaringType;
					
					Report.SymbolRelatedToPreviousError (m);
					if (qualifier_type == null) {
						Report.Error (38, loc, "Cannot access a nonstatic member of outer type `{0}' via nested type `{1}'",
							      TypeManager.CSharpName (m.DeclaringType),
							      TypeManager.CSharpName (container_type));
						
					} else if (qualifier_type != container_type &&
						   TypeManager.IsNestedFamilyAccessible (container_type, declaring_type)) {
						// Although a derived class can access protected members of
						// its base class it cannot do so through an instance of the
						// base class (CS1540).  If the qualifier_type is a base of the
						// ec.ContainerType and the lookup succeeds with the latter one,
						// then we are in this situation.
						Error_CannotAccessProtected (loc, m, qualifier_type, container_type);
					} else {
						ErrorIsInaccesible (loc, TypeManager.GetFullNameSignature (m));
					}
				}
				almostMatchedMembers.Clear ();
				return;
			}

			MemberInfo[] lookup = null;
			if (queried_type == null) {
				class_name = "global::";
			} else {
				lookup = TypeManager.MemberLookup (queried_type, null, queried_type,
					AllMemberTypes, AllBindingFlags |
					BindingFlags.NonPublic, name, null);
			}

			if (lookup == null) {
				if (!complain_if_none_found)
					return;

				if (class_name != null)
					Report.Error (103, loc, "The name `{0}' does not exist in the context of `{1}'",
						name, class_name);
				else
					Error_TypeDoesNotContainDefinition (loc, queried_type, name);
				return;
			}

			if (TypeManager.MemberLookup (queried_type, null, queried_type,
						      AllMemberTypes, AllBindingFlags |
						      BindingFlags.NonPublic, name, null) == null) {
				if ((lookup.Length == 1) && (lookup [0] is Type)) {
					Type t = (Type) lookup [0];

					Report.Error (305, loc,
						      "Using the generic type `{0}' " +
						      "requires {1} type arguments",
						      TypeManager.CSharpName (t),
						      TypeManager.GetNumberOfTypeArguments (t).ToString ());
					return;
				}
			}

			MemberList ml = TypeManager.FindMembers (queried_type, MemberTypes.Constructor,
								 BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly, null, null);
			if (name == ".ctor" && ml.Count == 0)
			{
				Report.Error (143, loc, "The type `{0}' has no constructors defined", TypeManager.CSharpName (queried_type));
				return;
			}

			Report.SymbolRelatedToPreviousError (lookup [0]);
			ErrorIsInaccesible (loc, TypeManager.GetFullNameSignature (lookup [0]));
		}

		/// <summary>
		///   Returns an expression that can be used to invoke operator true
		///   on the expression if it exists.
		/// </summary>
		static public Expression GetOperatorTrue (EmitContext ec, Expression e, Location loc)
		{
			return GetOperatorTrueOrFalse (ec, e, true, loc);
		}

		/// <summary>
		///   Returns an expression that can be used to invoke operator false
		///   on the expression if it exists.
		/// </summary>
		static public Expression GetOperatorFalse (EmitContext ec, Expression e, Location loc)
		{
			return GetOperatorTrueOrFalse (ec, e, false, loc);
		}

		static Expression GetOperatorTrueOrFalse (EmitContext ec, Expression e, bool is_true, Location loc)
		{
			MethodBase method;
			Expression operator_group;

#if GMCS_SOURCE
			if (TypeManager.IsNullableType (e.Type))
				return new Nullable.OperatorTrueOrFalse (e, is_true, loc).Resolve (ec);
#endif

			operator_group = MethodLookup (ec.ContainerType, e.Type, is_true ? "op_True" : "op_False", loc);
			if (operator_group == null)
				return null;

			ArrayList arguments = new ArrayList ();
			arguments.Add (new Argument (e, Argument.AType.Expression));
			method = ((MethodGroupExpr) operator_group).OverloadResolve (
				ec, arguments, false, loc);

			if (method == null)
				return null;

			return new StaticCallExpr ((MethodInfo) method, arguments, loc);
		}

		/// <summary>
		///   Resolves the expression `e' into a boolean expression: either through
		///   an implicit conversion, or through an `operator true' invocation
		/// </summary>
		public static Expression ResolveBoolean (EmitContext ec, Expression e, Location loc)
		{
			e = e.Resolve (ec);
			if (e == null)
				return null;

			if (e.Type == TypeManager.bool_type)
				return e;

			Expression converted = Convert.ImplicitConversion (ec, e, TypeManager.bool_type, Location.Null);

			if (converted != null)
				return converted;

			//
			// If no implicit conversion to bool exists, try using `operator true'
			//
			converted = Expression.GetOperatorTrue (ec, e, loc);
			if (converted == null){
				e.Error_ValueCannotBeConverted (ec, loc, TypeManager.bool_type, false);
				return null;
			}
			return converted;
		}
		
		public virtual string ExprClassName
		{
			get {
				switch (eclass){
					case ExprClass.Invalid:
						return "Invalid";
					case ExprClass.Value:
						return "value";
					case ExprClass.Variable:
						return "variable";
					case ExprClass.Namespace:
						return "namespace";
					case ExprClass.Type:
						return "type";
					case ExprClass.MethodGroup:
						return "method group";
					case ExprClass.PropertyAccess:
						return "property access";
					case ExprClass.EventAccess:
						return "event access";
					case ExprClass.IndexerAccess:
						return "indexer access";
					case ExprClass.Nothing:
						return "null";
				}
				throw new Exception ("Should not happen");
			}
		}
		
		/// <summary>
		///   Reports that we were expecting `expr' to be of class `expected'
		/// </summary>
		public void Error_UnexpectedKind (DeclSpace ds, string expected, Location loc)
		{
			Error_UnexpectedKind (ds, expected, ExprClassName, loc);
		}

		public void Error_UnexpectedKind (DeclSpace ds, string expected, string was, Location loc)
		{
			string name = GetSignatureForError ();
			if (ds != null)
				name = ds.GetSignatureForError () + '.' + name;

			Report.Error (118, loc, "`{0}' is a `{1}' but a `{2}' was expected",
			      name, was, expected);
		}

		public void Error_UnexpectedKind (ResolveFlags flags, Location loc)
		{
			string [] valid = new string [4];
			int count = 0;

			if ((flags & ResolveFlags.VariableOrValue) != 0) {
				valid [count++] = "variable";
				valid [count++] = "value";
			}

			if ((flags & ResolveFlags.Type) != 0)
				valid [count++] = "type";

			if ((flags & ResolveFlags.MethodGroup) != 0)
				valid [count++] = "method group";

			if (count == 0)
				valid [count++] = "unknown";

			StringBuilder sb = new StringBuilder (valid [0]);
			for (int i = 1; i < count - 1; i++) {
				sb.Append ("', `");
				sb.Append (valid [i]);
			}
			if (count > 1) {
				sb.Append ("' or `");
				sb.Append (valid [count - 1]);
			}

			Report.Error (119, loc, 
				"Expression denotes a `{0}', where a `{1}' was expected", ExprClassName, sb.ToString ());
		}
		
		public static void UnsafeError (Location loc)
		{
			Report.Error (214, loc, "Pointers and fixed size buffers may only be used in an unsafe context");
		}

		//
		// Load the object from the pointer.  
		//
		public static void LoadFromPtr (ILGenerator ig, Type t)
		{
			if (t == TypeManager.int32_type)
				ig.Emit (OpCodes.Ldind_I4);
			else if (t == TypeManager.uint32_type)
				ig.Emit (OpCodes.Ldind_U4);
			else if (t == TypeManager.short_type)
				ig.Emit (OpCodes.Ldind_I2);
			else if (t == TypeManager.ushort_type)
				ig.Emit (OpCodes.Ldind_U2);
			else if (t == TypeManager.char_type)
				ig.Emit (OpCodes.Ldind_U2);
			else if (t == TypeManager.byte_type)
				ig.Emit (OpCodes.Ldind_U1);
			else if (t == TypeManager.sbyte_type)
				ig.Emit (OpCodes.Ldind_I1);
			else if (t == TypeManager.uint64_type)
				ig.Emit (OpCodes.Ldind_I8);
			else if (t == TypeManager.int64_type)
				ig.Emit (OpCodes.Ldind_I8);
			else if (t == TypeManager.float_type)
				ig.Emit (OpCodes.Ldind_R4);
			else if (t == TypeManager.double_type)
				ig.Emit (OpCodes.Ldind_R8);
			else if (t == TypeManager.bool_type)
				ig.Emit (OpCodes.Ldind_I1);
			else if (t == TypeManager.intptr_type)
				ig.Emit (OpCodes.Ldind_I);
			else if (TypeManager.IsEnumType (t)) {
				if (t == TypeManager.enum_type)
					ig.Emit (OpCodes.Ldind_Ref);
				else
					LoadFromPtr (ig, TypeManager.EnumToUnderlying (t));
			} else if (t.IsValueType || TypeManager.IsGenericParameter (t))
				ig.Emit (OpCodes.Ldobj, t);
			else if (t.IsPointer)
				ig.Emit (OpCodes.Ldind_I);
			else
				ig.Emit (OpCodes.Ldind_Ref);
		}

		//
		// The stack contains the pointer and the value of type `type'
		//
		public static void StoreFromPtr (ILGenerator ig, Type type)
		{
			if (TypeManager.IsEnumType (type))
				type = TypeManager.EnumToUnderlying (type);
			if (type == TypeManager.int32_type || type == TypeManager.uint32_type)
				ig.Emit (OpCodes.Stind_I4);
			else if (type == TypeManager.int64_type || type == TypeManager.uint64_type)
				ig.Emit (OpCodes.Stind_I8);
			else if (type == TypeManager.char_type || type == TypeManager.short_type ||
				 type == TypeManager.ushort_type)
				ig.Emit (OpCodes.Stind_I2);
			else if (type == TypeManager.float_type)
				ig.Emit (OpCodes.Stind_R4);
			else if (type == TypeManager.double_type)
				ig.Emit (OpCodes.Stind_R8);
			else if (type == TypeManager.byte_type || type == TypeManager.sbyte_type ||
				 type == TypeManager.bool_type)
				ig.Emit (OpCodes.Stind_I1);
			else if (type == TypeManager.intptr_type)
				ig.Emit (OpCodes.Stind_I);
			else if (type.IsValueType || TypeManager.IsGenericParameter (type))
				ig.Emit (OpCodes.Stobj, type);
			else
				ig.Emit (OpCodes.Stind_Ref);
		}
		
		//
		// Returns the size of type `t' if known, otherwise, 0
		//
		public static int GetTypeSize (Type t)
		{
			t = TypeManager.TypeToCoreType (t);
			if (t == TypeManager.int32_type ||
			    t == TypeManager.uint32_type ||
			    t == TypeManager.float_type)
			        return 4;
			else if (t == TypeManager.int64_type ||
				 t == TypeManager.uint64_type ||
				 t == TypeManager.double_type)
			        return 8;
			else if (t == TypeManager.byte_type ||
				 t == TypeManager.sbyte_type ||
				 t == TypeManager.bool_type) 	
			        return 1;
			else if (t == TypeManager.short_type ||
				 t == TypeManager.char_type ||
				 t == TypeManager.ushort_type)
				return 2;
			else if (t == TypeManager.decimal_type)
				return 16;
			else
				return 0;
		}

		public static void Error_NegativeArrayIndex (Location loc)
		{
			Report.Error (248, loc, "Cannot create an array with a negative size");
		}

		protected void Error_CannotCallAbstractBase (string name)
		{
			Report.Error (205, loc, "Cannot call an abstract base member `{0}'", name);
		}
		
		//
		// Converts `source' to an int, uint, long or ulong.
		//
		public Expression ExpressionToArrayArgument (EmitContext ec, Expression source, Location loc)
		{
			Expression target;
			
			using (ec.With (EmitContext.Flags.CheckState, true)) {
				target = Convert.ImplicitConversion (ec, source, TypeManager.int32_type, loc);
				if (target == null)
					target = Convert.ImplicitConversion (ec, source, TypeManager.uint32_type, loc);
				if (target == null)
					target = Convert.ImplicitConversion (ec, source, TypeManager.int64_type, loc);
				if (target == null)
					target = Convert.ImplicitConversion (ec, source, TypeManager.uint64_type, loc);

				if (target == null) {
					source.Error_ValueCannotBeConverted (ec, loc, TypeManager.int32_type, false);
					return null;
				}
			}

			//
			// Only positive constants are allowed at compile time
			//
			if (target is Constant){
				if (target is IntConstant){
					if (((IntConstant) target).Value < 0){
						Error_NegativeArrayIndex (loc);
						return null;
					}
				}

				if (target is LongConstant){
					if (((LongConstant) target).Value < 0){
						Error_NegativeArrayIndex (loc);
						return null;
					}
				}
				
			}

			return target;
		}

		//
		// Derived classes implement this method by cloning the fields that
		// could become altered during the Resolve stage
		//
		// Only expressions that are created for the parser need to implement
		// this.
		//
		protected virtual void CloneTo (CloneContext clonectx, Expression target)
		{
			throw new NotImplementedException (
				String.Format (
					"CloneTo not implemented for expression {0}", this.GetType ()));
		}

		//
		// Clones an expression created by the parser.
		//
		// We only support expressions created by the parser so far, not
		// expressions that have been resolved (many more classes would need
		// to implement CloneTo).
		//
		// This infrastructure is here merely for Lambda expressions which
		// compile the same code using different type values for the same
		// arguments to find the correct overload
		//
		public Expression Clone (CloneContext clonectx)
		{
			Expression cloned = (Expression) MemberwiseClone ();
			CloneTo (clonectx, cloned);

			return cloned;
		}
	}

	/// <summary>
	///   This is just a base class for expressions that can
	///   appear on statements (invocations, object creation,
	///   assignments, post/pre increment and decrement).  The idea
	///   being that they would support an extra Emition interface that
	///   does not leave a result on the stack.
	/// </summary>
	public abstract class ExpressionStatement : Expression {

		public virtual ExpressionStatement ResolveStatement (EmitContext ec)
		{
			Expression e = Resolve (ec);
			if (e == null)
				return null;

			ExpressionStatement es = e as ExpressionStatement;
			if (es == null)
				Error_InvalidExpressionStatement ();

			return es;
		}

		/// <summary>
		///   Requests the expression to be emitted in a `statement'
		///   context.  This means that no new value is left on the
		///   stack after invoking this method (constrasted with
		///   Emit that will always leave a value on the stack).
		/// </summary>
		public abstract void EmitStatement (EmitContext ec);
	}

	/// <summary>
	///   This kind of cast is used to encapsulate the child
	///   whose type is child.Type into an expression that is
	///   reported to return "return_type".  This is used to encapsulate
	///   expressions which have compatible types, but need to be dealt
	///   at higher levels with.
	///
	///   For example, a "byte" expression could be encapsulated in one
	///   of these as an "unsigned int".  The type for the expression
	///   would be "unsigned int".
	///
	/// </summary>
	public class EmptyCast : Expression {
		protected Expression child;

		public EmptyCast (Expression child, Type return_type)
		{
			eclass = child.eclass;
			loc = child.Location;
			type = return_type;
			this.child = child;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			child.Emit (ec);
		}

		public override bool GetAttributableValue (Type valueType, out object value)
		{
			return child.GetAttributableValue (valueType, out value);
		}

		protected override void CloneTo (CloneContext clonectx, Expression t)
		{
			EmptyCast target = (EmptyCast) t;

			target.child = child.Clone (clonectx);
		}
	}

	/// <summary>
	///    Performs a cast using an operator (op_Explicit or op_Implicit)
	/// </summary>
	public class OperatorCast : EmptyCast {
		MethodInfo conversion_operator;
		bool find_explicit;
			
		public OperatorCast (Expression child, Type target_type) : this (child, target_type, false) {}

		public OperatorCast (Expression child, Type target_type, bool find_explicit)
			: base (child, target_type)
		{
			this.find_explicit = find_explicit;
		}

		// Returns the implicit operator that converts from
		// 'child.Type' to our target type (type)
		MethodInfo GetConversionOperator (bool find_explicit)
		{
			string operator_name = find_explicit ? "op_Explicit" : "op_Implicit";

			MemberInfo [] mi;

			mi = TypeManager.MemberLookup (child.Type, child.Type, child.Type, MemberTypes.Method,
				BindingFlags.Static | BindingFlags.Public, operator_name, null);

			if (mi == null){
				mi = TypeManager.MemberLookup (type, type, type, MemberTypes.Method,
							       BindingFlags.Static | BindingFlags.Public, operator_name, null);
			}
			
			foreach (MethodInfo oper in mi) {
				ParameterData pd = TypeManager.GetParameterData (oper);

				if (pd.ParameterType (0) == child.Type && oper.ReturnType == type)
					return oper;
			}

			return null;


		}
		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			child.Emit (ec);
			conversion_operator = GetConversionOperator (find_explicit);

			if (conversion_operator == null)
				throw new InternalErrorException ("Outer conversion routine is out of sync");

			ig.Emit (OpCodes.Call, conversion_operator);
		}
		
	}
	
	/// <summary>
	/// 	This is a numeric cast to a Decimal
	/// </summary>
	public class CastToDecimal : EmptyCast {
		MethodInfo conversion_operator;

		public CastToDecimal (Expression child)
			: this (child, false)
		{
		}

		public CastToDecimal (Expression child, bool find_explicit)
			: base (child, TypeManager.decimal_type)
		{
			conversion_operator = GetConversionOperator (find_explicit);

			if (conversion_operator == null)
				throw new InternalErrorException ("Outer conversion routine is out of sync");
		}

		// Returns the implicit operator that converts from
		// 'child.Type' to System.Decimal.
		MethodInfo GetConversionOperator (bool find_explicit)
		{
			string operator_name = find_explicit ? "op_Explicit" : "op_Implicit";
			
			MemberInfo [] mi = TypeManager.MemberLookup (type, type, type, MemberTypes.Method,
				BindingFlags.Static | BindingFlags.Public, operator_name, null);

			foreach (MethodInfo oper in mi) {
				ParameterData pd = TypeManager.GetParameterData (oper);

				if (pd.ParameterType (0) == child.Type && oper.ReturnType == type)
					return oper;
			}

			return null;
		}
		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			child.Emit (ec);

			ig.Emit (OpCodes.Call, conversion_operator);
		}
	}

	/// <summary>
	/// 	This is an explicit numeric cast from a Decimal
	/// </summary>
	public class CastFromDecimal : EmptyCast
	{
		static IDictionary operators;

		public CastFromDecimal (Expression child, Type return_type)
			: base (child, return_type)
		{
			if (child.Type != TypeManager.decimal_type)
				throw new InternalErrorException (
					"The expected type is Decimal, instead it is " + child.Type.FullName);
		}

		// Returns the explicit operator that converts from an
		// express of type System.Decimal to 'type'.
		public Expression Resolve ()
		{
			if (operators == null) {
				 MemberInfo[] all_oper = TypeManager.MemberLookup (TypeManager.decimal_type,
					TypeManager.decimal_type, TypeManager.decimal_type, MemberTypes.Method,
					BindingFlags.Static | BindingFlags.Public, "op_Explicit", null);

				operators = new System.Collections.Specialized.HybridDictionary ();
				foreach (MethodInfo oper in all_oper) {
					ParameterData pd = TypeManager.GetParameterData (oper);
					if (pd.ParameterType (0) == TypeManager.decimal_type)
						operators.Add (oper.ReturnType, oper);
				}
			}

			return operators.Contains (type) ? this : null;
		}

		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			child.Emit (ec);

			ig.Emit (OpCodes.Call, (MethodInfo)operators [type]);
		}
	}

	
	//
	// Constant specialization of EmptyCast.
	// We need to special case this since an empty cast of
	// a constant is still a constant. 
	//
	public class EmptyConstantCast : Constant
	{
		public readonly Constant child;

		public EmptyConstantCast(Constant child, Type type)
			: base (child.Location)
		{
			eclass = child.eclass;
			this.child = child;
			this.type = type;
		}

		public override string AsString ()
		{
			return child.AsString ();
		}

		public override object GetValue ()
		{
			return child.GetValue ();
		}

		public override Constant ConvertExplicitly (bool inCheckedContext, Type target_type)
		{
			// FIXME: check that 'type' can be converted to 'target_type' first
			return child.ConvertExplicitly (inCheckedContext, target_type);
		}

		public override Constant Increment ()
		{
			return child.Increment ();
		}

		public override bool IsDefaultValue {
			get { return child.IsDefaultValue; }
		}

		public override bool IsNegative {
			get { return child.IsNegative; }
		}

		public override void Emit (EmitContext ec)
		{
			child.Emit (ec);
		}

		public override Constant ConvertImplicitly (Type target_type)
		{
			// FIXME: Do we need to check user conversions?
			if (!Convert.ImplicitStandardConversionExists (this, target_type))
				return null;
			return child.ConvertImplicitly (target_type);
		}
	}


	/// <summary>
	///  This class is used to wrap literals which belong inside Enums
	/// </summary>
	public class EnumConstant : Constant {
		public Constant Child;

		public EnumConstant (Constant child, Type enum_type):
			base (child.Location)
		{
			eclass = child.eclass;
			this.Child = child;
			type = enum_type;
		}
		
		public override Expression DoResolve (EmitContext ec)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			Child.Emit (ec);
		}

		public override bool GetAttributableValue (Type valueType, out object value)
		{
			value = GetTypedValue ();
			return true;
		}

		public override string GetSignatureForError()
		{
			return TypeManager.CSharpName (Type);
		}

		public override object GetValue ()
		{
			return Child.GetValue ();
		}

		public override object GetTypedValue ()
		{
			// FIXME: runtime is not ready to work with just emited enums
			if (!RootContext.StdLib) {
				return Child.GetValue ();
			}

			return System.Enum.ToObject (type, Child.GetValue ());
		}
		
		public override string AsString ()
		{
			return TypeManager.CSharpEnumValue (type, Child.GetValue ());
		}

		public override Constant Increment()
		{
			return new EnumConstant (Child.Increment (), type);
		}

		public override bool IsDefaultValue {
			get {
				return Child.IsDefaultValue;
			}
		}

		public override bool IsZeroInteger {
			get { return Child.IsZeroInteger; }
		}

		public override bool IsNegative {
			get {
				return Child.IsNegative;
			}
		}

		public override Constant ConvertExplicitly(bool inCheckedContext, Type target_type)
		{
			if (Child.Type == target_type)
				return Child;

			return Child.ConvertExplicitly (inCheckedContext, target_type);
		}

		public override Constant ConvertImplicitly (Type type)
		{
			Type this_type = TypeManager.DropGenericTypeArguments (Type);
			type = TypeManager.DropGenericTypeArguments (type);

			if (this_type == type) {
				// This is workaround of mono bug. It can be removed when the latest corlib spreads enough
				if (TypeManager.IsEnumType (type.UnderlyingSystemType))
					return this;

				Type child_type = TypeManager.DropGenericTypeArguments (Child.Type);
				if (type.UnderlyingSystemType != child_type)
					Child = Child.ConvertImplicitly (type.UnderlyingSystemType);
				return this;
			}

			if (!Convert.ImplicitStandardConversionExists (this, type)){
				return null;
			}

			return Child.ConvertImplicitly(type);
		}

	}

	/// <summary>
	///   This kind of cast is used to encapsulate Value Types in objects.
	///
	///   The effect of it is to box the value type emitted by the previous
	///   operation.
	/// </summary>
	public class BoxedCast : EmptyCast {

		public BoxedCast (Expression expr, Type target_type)
			: base (expr, target_type)
		{
			eclass = ExprClass.Value;
		}
		
		public override Expression DoResolve (EmitContext ec)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			base.Emit (ec);
			
			ec.ig.Emit (OpCodes.Box, child.Type);
		}
	}

	public class UnboxCast : EmptyCast {
		public UnboxCast (Expression expr, Type return_type)
			: base (expr, return_type)
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override Expression DoResolveLValue (EmitContext ec, Expression right_side)
		{
			if (right_side == EmptyExpression.LValueMemberAccess || right_side == EmptyExpression.LValueMemberOutAccess)
				Report.Error (445, loc, "Cannot modify the result of an unboxing conversion");
			return base.DoResolveLValue (ec, right_side);
		}

		public override void Emit (EmitContext ec)
		{
			Type t = type;
			ILGenerator ig = ec.ig;
			
			base.Emit (ec);
#if GMCS_SOURCE
			if (t.IsGenericParameter || t.IsGenericType && t.IsValueType)
				ig.Emit (OpCodes.Unbox_Any, t);
			else
#endif
			{
				ig.Emit (OpCodes.Unbox, t);

				LoadFromPtr (ig, t);
			}
		}
	}
	
	/// <summary>
	///   This is used to perform explicit numeric conversions.
	///
	///   Explicit numeric conversions might trigger exceptions in a checked
	///   context, so they should generate the conv.ovf opcodes instead of
	///   conv opcodes.
	/// </summary>
	public class ConvCast : EmptyCast {
		public enum Mode : byte {
			I1_U1, I1_U2, I1_U4, I1_U8, I1_CH,
			U1_I1, U1_CH,
			I2_I1, I2_U1, I2_U2, I2_U4, I2_U8, I2_CH,
			U2_I1, U2_U1, U2_I2, U2_CH,
			I4_I1, I4_U1, I4_I2, I4_U2, I4_U4, I4_U8, I4_CH,
			U4_I1, U4_U1, U4_I2, U4_U2, U4_I4, U4_CH,
			I8_I1, I8_U1, I8_I2, I8_U2, I8_I4, I8_U4, I8_U8, I8_CH,
			U8_I1, U8_U1, U8_I2, U8_U2, U8_I4, U8_U4, U8_I8, U8_CH,
			CH_I1, CH_U1, CH_I2,
			R4_I1, R4_U1, R4_I2, R4_U2, R4_I4, R4_U4, R4_I8, R4_U8, R4_CH,
			R8_I1, R8_U1, R8_I2, R8_U2, R8_I4, R8_U4, R8_I8, R8_U8, R8_CH, R8_R4
		}

		Mode mode;
		
		public ConvCast (Expression child, Type return_type, Mode m)
			: base (child, return_type)
		{
			mode = m;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override string ToString ()
		{
			return String.Format ("ConvCast ({0}, {1})", mode, child);
		}
		
		public override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			
			base.Emit (ec);

			if (ec.CheckState){
				switch (mode){
				case Mode.I1_U1: ig.Emit (OpCodes.Conv_Ovf_U1); break;
				case Mode.I1_U2: ig.Emit (OpCodes.Conv_Ovf_U2); break;
				case Mode.I1_U4: ig.Emit (OpCodes.Conv_Ovf_U4); break;
				case Mode.I1_U8: ig.Emit (OpCodes.Conv_Ovf_U8); break;
				case Mode.I1_CH: ig.Emit (OpCodes.Conv_Ovf_U2); break;

				case Mode.U1_I1: ig.Emit (OpCodes.Conv_Ovf_I1_Un); break;
				case Mode.U1_CH: /* nothing */ break;

				case Mode.I2_I1: ig.Emit (OpCodes.Conv_Ovf_I1); break;
				case Mode.I2_U1: ig.Emit (OpCodes.Conv_Ovf_U1); break;
				case Mode.I2_U2: ig.Emit (OpCodes.Conv_Ovf_U2); break;
				case Mode.I2_U4: ig.Emit (OpCodes.Conv_Ovf_U4); break;
				case Mode.I2_U8: ig.Emit (OpCodes.Conv_Ovf_U8); break;
				case Mode.I2_CH: ig.Emit (OpCodes.Conv_Ovf_U2); break;

				case Mode.U2_I1: ig.Emit (OpCodes.Conv_Ovf_I1_Un); break;
				case Mode.U2_U1: ig.Emit (OpCodes.Conv_Ovf_U1_Un); break;
				case Mode.U2_I2: ig.Emit (OpCodes.Conv_Ovf_I2_Un); break;
				case Mode.U2_CH: /* nothing */ break;

				case Mode.I4_I1: ig.Emit (OpCodes.Conv_Ovf_I1); break;
				case Mode.I4_U1: ig.Emit (OpCodes.Conv_Ovf_U1); break;
				case Mode.I4_I2: ig.Emit (OpCodes.Conv_Ovf_I2); break;
				case Mode.I4_U4: ig.Emit (OpCodes.Conv_Ovf_U4); break;
				case Mode.I4_U2: ig.Emit (OpCodes.Conv_Ovf_U2); break;
				case Mode.I4_U8: ig.Emit (OpCodes.Conv_Ovf_U8); break;
				case Mode.I4_CH: ig.Emit (OpCodes.Conv_Ovf_U2); break;

				case Mode.U4_I1: ig.Emit (OpCodes.Conv_Ovf_I1_Un); break;
				case Mode.U4_U1: ig.Emit (OpCodes.Conv_Ovf_U1_Un); break;
				case Mode.U4_I2: ig.Emit (OpCodes.Conv_Ovf_I2_Un); break;
				case Mode.U4_U2: ig.Emit (OpCodes.Conv_Ovf_U2_Un); break;
				case Mode.U4_I4: ig.Emit (OpCodes.Conv_Ovf_I4_Un); break;
				case Mode.U4_CH: ig.Emit (OpCodes.Conv_Ovf_U2_Un); break;

				case Mode.I8_I1: ig.Emit (OpCodes.Conv_Ovf_I1); break;
				case Mode.I8_U1: ig.Emit (OpCodes.Conv_Ovf_U1); break;
				case Mode.I8_I2: ig.Emit (OpCodes.Conv_Ovf_I2); break;
				case Mode.I8_U2: ig.Emit (OpCodes.Conv_Ovf_U2); break;
				case Mode.I8_I4: ig.Emit (OpCodes.Conv_Ovf_I4); break;
				case Mode.I8_U4: ig.Emit (OpCodes.Conv_Ovf_U4); break;
				case Mode.I8_U8: ig.Emit (OpCodes.Conv_Ovf_U8); break;
				case Mode.I8_CH: ig.Emit (OpCodes.Conv_Ovf_U2); break;

				case Mode.U8_I1: ig.Emit (OpCodes.Conv_Ovf_I1_Un); break;
				case Mode.U8_U1: ig.Emit (OpCodes.Conv_Ovf_U1_Un); break;
				case Mode.U8_I2: ig.Emit (OpCodes.Conv_Ovf_I2_Un); break;
				case Mode.U8_U2: ig.Emit (OpCodes.Conv_Ovf_U2_Un); break;
				case Mode.U8_I4: ig.Emit (OpCodes.Conv_Ovf_I4_Un); break;
				case Mode.U8_U4: ig.Emit (OpCodes.Conv_Ovf_U4_Un); break;
				case Mode.U8_I8: ig.Emit (OpCodes.Conv_Ovf_I8_Un); break;
				case Mode.U8_CH: ig.Emit (OpCodes.Conv_Ovf_U2_Un); break;

				case Mode.CH_I1: ig.Emit (OpCodes.Conv_Ovf_I1_Un); break;
				case Mode.CH_U1: ig.Emit (OpCodes.Conv_Ovf_U1_Un); break;
				case Mode.CH_I2: ig.Emit (OpCodes.Conv_Ovf_I2_Un); break;

				case Mode.R4_I1: ig.Emit (OpCodes.Conv_Ovf_I1); break;
				case Mode.R4_U1: ig.Emit (OpCodes.Conv_Ovf_U1); break;
				case Mode.R4_I2: ig.Emit (OpCodes.Conv_Ovf_I2); break;
				case Mode.R4_U2: ig.Emit (OpCodes.Conv_Ovf_U2); break;
				case Mode.R4_I4: ig.Emit (OpCodes.Conv_Ovf_I4); break;
				case Mode.R4_U4: ig.Emit (OpCodes.Conv_Ovf_U4); break;
				case Mode.R4_I8: ig.Emit (OpCodes.Conv_Ovf_I8); break;
				case Mode.R4_U8: ig.Emit (OpCodes.Conv_Ovf_U8); break;
				case Mode.R4_CH: ig.Emit (OpCodes.Conv_Ovf_U2); break;

				case Mode.R8_I1: ig.Emit (OpCodes.Conv_Ovf_I1); break;
				case Mode.R8_U1: ig.Emit (OpCodes.Conv_Ovf_U1); break;
				case Mode.R8_I2: ig.Emit (OpCodes.Conv_Ovf_I2); break;
				case Mode.R8_U2: ig.Emit (OpCodes.Conv_Ovf_U2); break;
				case Mode.R8_I4: ig.Emit (OpCodes.Conv_Ovf_I4); break;
				case Mode.R8_U4: ig.Emit (OpCodes.Conv_Ovf_U4); break;
				case Mode.R8_I8: ig.Emit (OpCodes.Conv_Ovf_I8); break;
				case Mode.R8_U8: ig.Emit (OpCodes.Conv_Ovf_U8); break;
				case Mode.R8_CH: ig.Emit (OpCodes.Conv_Ovf_U2); break;
				case Mode.R8_R4: ig.Emit (OpCodes.Conv_R4); break;
				}
			} else {
				switch (mode){
				case Mode.I1_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.I1_U2: ig.Emit (OpCodes.Conv_U2); break;
				case Mode.I1_U4: ig.Emit (OpCodes.Conv_U4); break;
				case Mode.I1_U8: ig.Emit (OpCodes.Conv_I8); break;
				case Mode.I1_CH: ig.Emit (OpCodes.Conv_U2); break;

				case Mode.U1_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.U1_CH: ig.Emit (OpCodes.Conv_U2); break;

				case Mode.I2_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.I2_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.I2_U2: ig.Emit (OpCodes.Conv_U2); break;
				case Mode.I2_U4: ig.Emit (OpCodes.Conv_U4); break;
				case Mode.I2_U8: ig.Emit (OpCodes.Conv_I8); break;
				case Mode.I2_CH: ig.Emit (OpCodes.Conv_U2); break;

				case Mode.U2_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.U2_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.U2_I2: ig.Emit (OpCodes.Conv_I2); break;
				case Mode.U2_CH: /* nothing */ break;

				case Mode.I4_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.I4_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.I4_I2: ig.Emit (OpCodes.Conv_I2); break;
				case Mode.I4_U4: /* nothing */ break;
				case Mode.I4_U2: ig.Emit (OpCodes.Conv_U2); break;
				case Mode.I4_U8: ig.Emit (OpCodes.Conv_I8); break;
				case Mode.I4_CH: ig.Emit (OpCodes.Conv_U2); break;

				case Mode.U4_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.U4_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.U4_I2: ig.Emit (OpCodes.Conv_I2); break;
				case Mode.U4_U2: ig.Emit (OpCodes.Conv_U2); break;
				case Mode.U4_I4: /* nothing */ break;
				case Mode.U4_CH: ig.Emit (OpCodes.Conv_U2); break;

				case Mode.I8_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.I8_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.I8_I2: ig.Emit (OpCodes.Conv_I2); break;
				case Mode.I8_U2: ig.Emit (OpCodes.Conv_U2); break;
				case Mode.I8_I4: ig.Emit (OpCodes.Conv_I4); break;
				case Mode.I8_U4: ig.Emit (OpCodes.Conv_U4); break;
				case Mode.I8_U8: /* nothing */ break;
				case Mode.I8_CH: ig.Emit (OpCodes.Conv_U2); break;

				case Mode.U8_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.U8_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.U8_I2: ig.Emit (OpCodes.Conv_I2); break;
				case Mode.U8_U2: ig.Emit (OpCodes.Conv_U2); break;
				case Mode.U8_I4: ig.Emit (OpCodes.Conv_I4); break;
				case Mode.U8_U4: ig.Emit (OpCodes.Conv_U4); break;
				case Mode.U8_I8: /* nothing */ break;
				case Mode.U8_CH: ig.Emit (OpCodes.Conv_U2); break;

				case Mode.CH_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.CH_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.CH_I2: ig.Emit (OpCodes.Conv_I2); break;

				case Mode.R4_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.R4_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.R4_I2: ig.Emit (OpCodes.Conv_I2); break;
				case Mode.R4_U2: ig.Emit (OpCodes.Conv_U2); break;
				case Mode.R4_I4: ig.Emit (OpCodes.Conv_I4); break;
				case Mode.R4_U4: ig.Emit (OpCodes.Conv_U4); break;
				case Mode.R4_I8: ig.Emit (OpCodes.Conv_I8); break;
				case Mode.R4_U8: ig.Emit (OpCodes.Conv_U8); break;
				case Mode.R4_CH: ig.Emit (OpCodes.Conv_U2); break;

				case Mode.R8_I1: ig.Emit (OpCodes.Conv_I1); break;
				case Mode.R8_U1: ig.Emit (OpCodes.Conv_U1); break;
				case Mode.R8_I2: ig.Emit (OpCodes.Conv_I2); break;
				case Mode.R8_U2: ig.Emit (OpCodes.Conv_U2); break;
				case Mode.R8_I4: ig.Emit (OpCodes.Conv_I4); break;
				case Mode.R8_U4: ig.Emit (OpCodes.Conv_U4); break;
				case Mode.R8_I8: ig.Emit (OpCodes.Conv_I8); break;
				case Mode.R8_U8: ig.Emit (OpCodes.Conv_U8); break;
				case Mode.R8_CH: ig.Emit (OpCodes.Conv_U2); break;
				case Mode.R8_R4: ig.Emit (OpCodes.Conv_R4); break;
				}
			}
		}
	}
	
	public class OpcodeCast : EmptyCast {
		OpCode op, op2;
		bool second_valid;
		
		public OpcodeCast (Expression child, Type return_type, OpCode op)
			: base (child, return_type)
			
		{
			this.op = op;
			second_valid = false;
		}

		public OpcodeCast (Expression child, Type return_type, OpCode op, OpCode op2)
			: base (child, return_type)
			
		{
			this.op = op;
			this.op2 = op2;
			second_valid = true;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			base.Emit (ec);
			ec.ig.Emit (op);

			if (second_valid)
				ec.ig.Emit (op2);
		}			
	}

	/// <summary>
	///   This kind of cast is used to encapsulate a child and cast it
	///   to the class requested
	/// </summary>
	public class ClassCast : EmptyCast {
		public ClassCast (Expression child, Type return_type)
			: base (child, return_type)
			
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			// This should never be invoked, we are born in fully
			// initialized state.

			return this;
		}

		public override void Emit (EmitContext ec)
		{
			base.Emit (ec);

			if (TypeManager.IsGenericParameter (child.Type))
				ec.ig.Emit (OpCodes.Box, child.Type);

#if GMCS_SOURCE
			if (type.IsGenericParameter)
				ec.ig.Emit (OpCodes.Unbox_Any, type);
			else
#endif
				ec.ig.Emit (OpCodes.Castclass, type);
		}
	}
	
	/// <summary>
	///   SimpleName expressions are formed of a single word and only happen at the beginning 
	///   of a dotted-name.
	/// </summary>
	public class SimpleName : Expression {
		public string Name;
		public readonly TypeArguments Arguments;
		bool in_transit;

		public SimpleName (string name, Location l)
		{
			Name = name;
			loc = l;
		}

		public SimpleName (string name, TypeArguments args, Location l)
		{
			Name = name;
			Arguments = args;
			loc = l;
		}

		public SimpleName (string name, TypeParameter[] type_params, Location l)
		{
			Name = name;
			loc = l;

			Arguments = new TypeArguments (l);
			foreach (TypeParameter type_param in type_params)
				Arguments.Add (new TypeParameterExpr (type_param, l));
		}

		public static string RemoveGenericArity (string name)
		{
			int start = 0;
			StringBuilder sb = null;
			do {
				int pos = name.IndexOf ('`', start);
				if (pos < 0) {
					if (start == 0)
						return name;

					sb.Append (name.Substring (start));
					break;
				}

				if (sb == null)
					sb = new StringBuilder ();
				sb.Append (name.Substring (start, pos-start));

				pos++;
				while ((pos < name.Length) && Char.IsNumber (name [pos]))
					pos++;

				start = pos;
			} while (start < name.Length);

			return sb.ToString ();
		}

		public SimpleName GetMethodGroup ()
		{
			return new SimpleName (RemoveGenericArity (Name), Arguments, loc);
		}

		public static void Error_ObjectRefRequired (EmitContext ec, Location l, string name)
		{
			if (ec.IsFieldInitializer)
				Report.Error (236, l,
					"A field initializer cannot reference the nonstatic field, method, or property `{0}'",
					name);
			else
				Report.Error (
					120, l, "`{0}': An object reference is required for the nonstatic field, method or property",
					name);
		}

		public bool IdenticalNameAndTypeName (EmitContext ec, Expression resolved_to, Location loc)
		{
			return resolved_to != null && resolved_to.Type != null && 
				resolved_to.Type.Name == Name &&
				(ec.DeclContainer.LookupNamespaceOrType (Name, loc, /* ignore_cs0104 = */ true) != null);
		}

		public override Expression DoResolve (EmitContext ec)
		{
			return SimpleNameResolve (ec, null, false);
		}

		public override Expression DoResolveLValue (EmitContext ec, Expression right_side)
		{
			return SimpleNameResolve (ec, right_side, false);
		}
		

		public Expression DoResolve (EmitContext ec, bool intermediate)
		{
			return SimpleNameResolve (ec, null, intermediate);
		}

		private bool IsNestedChild (Type t, Type parent)
		{
			if (parent == null)
				return false;

			while (parent != null) {
				parent = TypeManager.DropGenericTypeArguments (parent);
				if (TypeManager.IsNestedChildOf (t, parent))
					return true;

				parent = parent.BaseType;
			}

			return false;
		}

		FullNamedExpression ResolveNested (IResolveContext ec, Type t)
		{
			if (!TypeManager.IsGenericTypeDefinition (t) && !TypeManager.IsGenericType (t))
				return null;

			DeclSpace ds = ec.DeclContainer;
			while (ds != null) {
				if (IsNestedChild (t, ds.TypeBuilder))
					break;

				ds = ds.Parent;
			}

			if (ds == null)
				return null;

			Type[] gen_params = TypeManager.GetTypeArguments (t);

			int arg_count = Arguments != null ? Arguments.Count : 0;

			for (; (ds != null) && ds.IsGeneric; ds = ds.Parent) {
				if (arg_count + ds.CountTypeParameters == gen_params.Length) {
					TypeArguments new_args = new TypeArguments (loc);
					foreach (TypeParameter param in ds.TypeParameters)
						new_args.Add (new TypeParameterExpr (param, loc));

					if (Arguments != null)
						new_args.Add (Arguments);

					return new ConstructedType (t, new_args, loc);
				}
			}

			return null;
		}

		public override FullNamedExpression ResolveAsTypeStep (IResolveContext ec, bool silent)
		{
			FullNamedExpression fne = ec.GenericDeclContainer.LookupGeneric (Name, loc);
			if (fne != null)
				return fne.ResolveAsTypeStep (ec, silent);

			int errors = Report.Errors;
			fne = ec.DeclContainer.LookupNamespaceOrType (Name, loc, /*ignore_cs0104=*/ false);

			if (fne != null) {
				if (fne.Type == null)
					return fne;

				FullNamedExpression nested = ResolveNested (ec, fne.Type);
				if (nested != null)
					return nested.ResolveAsTypeStep (ec, false);

				if (Arguments != null) {
					ConstructedType ct = new ConstructedType (fne, Arguments, loc);
					return ct.ResolveAsTypeStep (ec, false);
				}

				return fne;
			}

			if (silent || errors != Report.Errors)
				return null;

			MemberCore mc = ec.DeclContainer.GetDefinition (Name);
			if (mc != null) {
				Error_UnexpectedKind (ec.DeclContainer, "type", GetMemberType (mc), loc);
				return null;
			}

			string ns = ec.DeclContainer.NamespaceEntry.NS.Name;
			string fullname = (ns.Length > 0) ? ns + "." + Name : Name;
			foreach (Assembly a in RootNamespace.Global.Assemblies) {
				Type type = a.GetType (fullname);
				if (type != null) {
					Report.SymbolRelatedToPreviousError (type);
					Expression.ErrorIsInaccesible (loc, fullname);
					return null;
				}
			}

			Type t = ec.DeclContainer.LookupAnyGeneric (Name);
			if (t != null) {
				Namespace.Error_InvalidNumberOfTypeArguments (t, loc);
				return null;
			}

			NamespaceEntry.Error_NamespaceNotFound (loc, Name);
			return null;
		}

		// TODO: I am still not convinced about this. If someone else will need it
		// implement this as virtual property in MemberCore hierarchy
		public static string GetMemberType (MemberCore mc)
		{
			if (mc is Property)
				return "property";
			if (mc is Indexer)
				return "indexer";
			if (mc is FieldBase)
				return "field";
			if (mc is MethodCore)
				return "method";
			if (mc is EnumMember)
				return "enum";
			if (mc is Event)
				return "event";

			return "type";
		}

		Expression SimpleNameResolve (EmitContext ec, Expression right_side, bool intermediate)
		{
			if (in_transit)
				return null;
			in_transit = true;

			Expression e = DoSimpleNameResolve (ec, right_side, intermediate);
			if (e == null)
				return null;

			if (ec.CurrentBlock == null || ec.CurrentBlock.CheckInvariantMeaningInBlock (Name, e, Location))
				return e;

			return null;
		}

		/// <remarks>
		///   7.5.2: Simple Names. 
		///
		///   Local Variables and Parameters are handled at
		///   parse time, so they never occur as SimpleNames.
		///
		///   The `intermediate' flag is used by MemberAccess only
		///   and it is used to inform us that it is ok for us to 
		///   avoid the static check, because MemberAccess might end
		///   up resolving the Name as a Type name and the access as
		///   a static type access.
		///
		///   ie: Type Type; .... { Type.GetType (""); }
		///
		///   Type is both an instance variable and a Type;  Type.GetType
		///   is the static method not an instance method of type.
		/// </remarks>
		Expression DoSimpleNameResolve (EmitContext ec, Expression right_side, bool intermediate)
		{
			Expression e = null;

			//
			// Stage 1: Performed by the parser (binding to locals or parameters).
			//
			Block current_block = ec.CurrentBlock;
			if (current_block != null){
				LocalInfo vi = current_block.GetLocalInfo (Name);
				if (vi != null){
					if (Arguments != null) {
						Report.Error (307, loc,
							      "The variable `{0}' cannot be used with type arguments",
							      Name);
						return null;
					}

					LocalVariableReference var = new LocalVariableReference (ec.CurrentBlock, Name, loc);
					if (right_side != null) {
						return var.ResolveLValue (ec, right_side, loc);
					} else {
						ResolveFlags rf = ResolveFlags.VariableOrValue;
						if (intermediate)
							rf |= ResolveFlags.DisableFlowAnalysis;
						return var.Resolve (ec, rf);
					}
				}

				ParameterReference pref = current_block.Toplevel.GetParameterReference (Name, loc);
				if (pref != null) {
					if (Arguments != null) {
						Report.Error (307, loc,
							      "The variable `{0}' cannot be used with type arguments",
							      Name);
						return null;
					}

					if (right_side != null)
						return pref.ResolveLValue (ec, right_side, loc);
					else
						return pref.Resolve (ec);
				}
			}
			
			//
			// Stage 2: Lookup members 
			//

			DeclSpace lookup_ds = ec.DeclContainer;
			Type almost_matched_type = null;
			ArrayList almost_matched = null;
			do {
				if (lookup_ds.TypeBuilder == null)
					break;

				e = MemberLookup (ec.ContainerType, lookup_ds.TypeBuilder, Name, loc);
				if (e != null)
					break;

				if (almost_matched == null && almostMatchedMembers.Count > 0) {
					almost_matched_type = lookup_ds.TypeBuilder;
					almost_matched = (ArrayList) almostMatchedMembers.Clone ();
				}

				lookup_ds =lookup_ds.Parent;
			} while (lookup_ds != null);

			if (e == null && ec.ContainerType != null)
				e = MemberLookup (ec.ContainerType, ec.ContainerType, Name, loc);

			if (e == null) {
				if (almost_matched == null && almostMatchedMembers.Count > 0) {
					almost_matched_type = ec.ContainerType;
					almost_matched = (ArrayList) almostMatchedMembers.Clone ();
				}
				e = ResolveAsTypeStep (ec, true);
			}

			if (e == null) {
				if (almost_matched != null)
					almostMatchedMembers = almost_matched;
				if (almost_matched_type == null)
					almost_matched_type = ec.ContainerType;
				MemberLookupFailed (ec.ContainerType, null, almost_matched_type, ((SimpleName) this).Name, ec.DeclContainer.Name, true, loc);
				return null;
			}

			if (e is TypeExpr) {
				if (Arguments == null)
					return e;

				ConstructedType ct = new ConstructedType (
					(FullNamedExpression) e, Arguments, loc);
				return ct.ResolveAsTypeStep (ec, false);
			}

			if (e is MemberExpr) {
				MemberExpr me = (MemberExpr) e;

				Expression left;
				if (me.IsInstance) {
					if (ec.IsStatic || ec.IsFieldInitializer) {
						//
						// Note that an MemberExpr can be both IsInstance and IsStatic.
						// An unresolved MethodGroupExpr can contain both kinds of methods
						// and each predicate is true if the MethodGroupExpr contains
						// at least one of that kind of method.
						//

						if (!me.IsStatic &&
						    (!intermediate || !IdenticalNameAndTypeName (ec, me, loc))) {
							Error_ObjectRefRequired (ec, loc, me.GetSignatureForError ());
							return EmptyExpression.Null;
						}

						//
						// Pass the buck to MemberAccess and Invocation.
						//
						left = EmptyExpression.Null;
					} else {
						left = ec.GetThis (loc);
					}
				} else {
					left = new TypeExpression (ec.ContainerType, loc);
				}

				e = me.ResolveMemberAccess (ec, left, loc, null);
				if (e == null)
					return null;

				me = e as MemberExpr;
				if (me == null)
					return e;

				if (Arguments != null) {
					MethodGroupExpr mg = me as MethodGroupExpr;
					if (mg == null)
						return null;

					return mg.ResolveGeneric (ec, Arguments);
				}

				if (!me.IsStatic && (me.InstanceExpression != null) &&
				    TypeManager.IsNestedFamilyAccessible (me.InstanceExpression.Type, me.DeclaringType) &&
				    me.InstanceExpression.Type != me.DeclaringType &&
				    !TypeManager.IsFamilyAccessible (me.InstanceExpression.Type, me.DeclaringType) &&
				    (!intermediate || !IdenticalNameAndTypeName (ec, e, loc))) {
					Report.Error (38, loc, "Cannot access a nonstatic member of outer type `{0}' via nested type `{1}'",
						TypeManager.CSharpName (me.DeclaringType), TypeManager.CSharpName (me.InstanceExpression.Type));
					return null;
				}

				return (right_side != null)
					? me.DoResolveLValue (ec, right_side)
					: me.DoResolve (ec);
			}

			return e;
		}
		
		public override void Emit (EmitContext ec)
		{
			throw new InternalErrorException ("The resolve phase was not executed");
		}

		public override string ToString ()
		{
			return Name;
		}

		public override string GetSignatureForError ()
		{
			return Name;
		}

		protected override void CloneTo (CloneContext clonectx, Expression target)
		{
			// CloneTo: Nothing, we do not keep any state on this expression
		}
	}

	/// <summary>
	///   Represents a namespace or a type.  The name of the class was inspired by
	///   section 10.8.1 (Fully Qualified Names).
	/// </summary>
	public abstract class FullNamedExpression : Expression {
		public override FullNamedExpression ResolveAsTypeStep (IResolveContext ec, bool silent)
		{
			return this;
		}

		public abstract string FullName {
			get;
		}
	}
	
	/// <summary>
	///   Expression that evaluates to a type
	/// </summary>
	public abstract class TypeExpr : FullNamedExpression {
		override public FullNamedExpression ResolveAsTypeStep (IResolveContext ec, bool silent)
		{
			TypeExpr t = DoResolveAsTypeStep (ec);
			if (t == null)
				return null;

			eclass = ExprClass.Type;
			return t;
		}

		override public Expression DoResolve (EmitContext ec)
		{
			return ResolveAsTypeTerminal (ec, false);
		}

		override public void Emit (EmitContext ec)
		{
			throw new Exception ("Should never be called");
		}

		public virtual bool CheckAccessLevel (DeclSpace ds)
		{
			return ds.CheckAccessLevel (Type);
		}

		public virtual bool AsAccessible (DeclSpace ds, int flags)
		{
			return ds.AsAccessible (Type, flags);
		}

		public virtual bool IsClass {
			get { return Type.IsClass; }
		}

		public virtual bool IsValueType {
			get { return Type.IsValueType; }
		}

		public virtual bool IsInterface {
			get { return Type.IsInterface; }
		}

		public virtual bool IsSealed {
			get { return Type.IsSealed; }
		}

		public virtual bool CanInheritFrom ()
		{
			if (Type == TypeManager.enum_type ||
			    (Type == TypeManager.value_type && RootContext.StdLib) ||
			    Type == TypeManager.multicast_delegate_type ||
			    Type == TypeManager.delegate_type ||
			    Type == TypeManager.array_type)
				return false;

			return true;
		}

		protected abstract TypeExpr DoResolveAsTypeStep (IResolveContext ec);

		public abstract string Name {
			get;
		}

		public override bool Equals (object obj)
		{
			TypeExpr tobj = obj as TypeExpr;
			if (tobj == null)
				return false;

			return Type == tobj.Type;
		}

		public override int GetHashCode ()
		{
			return Type.GetHashCode ();
		}
		
		public override string ToString ()
		{
			return Name;
		}
	}

	/// <summary>
	///   Fully resolved Expression that already evaluated to a type
	/// </summary>
	public class TypeExpression : TypeExpr {
		public TypeExpression (Type t, Location l)
		{
			Type = t;
			eclass = ExprClass.Type;
			loc = l;
		}

		protected override TypeExpr DoResolveAsTypeStep (IResolveContext ec)
		{
			return this;
		}

		public override TypeExpr ResolveAsTypeTerminal (IResolveContext ec, bool silent)
		{
			return this;
		}

		public override string Name {
			get { return Type.ToString (); }
		}

		public override string FullName {
			get { return Type.FullName; }
		}
	}

	/// <summary>
	///   Used to create types from a fully qualified name.  These are just used
	///   by the parser to setup the core types.  A TypeLookupExpression is always
	///   classified as a type.
	/// </summary>
	public sealed class TypeLookupExpression : TypeExpr {
		readonly string name;
		
		public TypeLookupExpression (string name)
		{
			this.name = name;
			eclass = ExprClass.Type;
		}

		public override TypeExpr ResolveAsTypeTerminal (IResolveContext ec, bool silent)
		{
			// It's null for corlib compilation only
			if (type == null)
				return DoResolveAsTypeStep (ec);

			return this;
		}

		private class UnexpectedType
		{
		}

		// This performes recursive type lookup, providing support for generic types.
		// For example, given the type:
		//
		// System.Collections.Generic.KeyValuePair`2[[System.Int32],[System.String]]
		//
		// The types will be checked in the following order:
		//                                                                             _
		// System                                                                       |
		// System.Collections                                                           |
		// System.Collections.Generic                                                   |
		//                        _                                                     |
		//     System              | recursive call 1                                   |
		//     System.Int32       _|                                                    | main method call
		//                        _                                                     |
		//     System              | recursive call 2                                   |
		//     System.String      _|                                                    |
		//                                                                              |
		// System.Collections.Generic.KeyValuePair`2[[System.Int32],[System.String]]   _|
		//
		private Type TypeLookup (IResolveContext ec, string name)
		{
			int index = 0;
			int dot = 0;
			bool done = false;
			FullNamedExpression resolved = null;
			Type type = null;
			Type recursive_type = null;
			while (index < name.Length) {
				if (name[index] == '[') {
					int open = index;
					int braces = 1;
					do {
						index++;
						if (name[index] == '[')
							braces++;
						else if (name[index] == ']')
							braces--;
					} while (braces > 0);
					recursive_type = TypeLookup (ec, name.Substring (open + 1, index - open - 1));
					if (recursive_type == null || (recursive_type == typeof(UnexpectedType)))
						return recursive_type;
				}
				else {
					if (name[index] == ',')
						done = true;
					else if ((name[index] == '.' && !done) || (index == name.Length && name[0] != '[')) {
						string substring = name.Substring(dot, index - dot);

						if (resolved == null)
							resolved = RootNamespace.Global.Lookup (ec.DeclContainer, substring, Location.Null);
						else if (resolved is Namespace)
						    resolved = (resolved as Namespace).Lookup (ec.DeclContainer, substring, Location.Null);
						else if (type != null)
							type = TypeManager.GetNestedType (type, substring);
						else
							return null;

						if (resolved == null)
							return null;
						else if (type == null && resolved is TypeExpr)
							type = resolved.Type;

						dot = index + 1;
					}
				}
				index++;
			}
			if (name[0] != '[') {
				string substring = name.Substring(dot, index - dot);

				if (type != null)
					return TypeManager.GetNestedType (type, substring);
				else if (resolved != null) {
					resolved = (resolved as Namespace).Lookup (ec.DeclContainer, substring, Location.Null);
					if (resolved is TypeExpr)
						return resolved.Type;
					else {
						resolved.Error_UnexpectedKind (ec.DeclContainer, "type", loc);
						return typeof (UnexpectedType);
					}
				}
				else
					return null;
			}
			else
				return recursive_type;
			}

		protected override TypeExpr DoResolveAsTypeStep (IResolveContext ec)
		{
			Type t = TypeLookup (ec, name);
			if (t == null || !ec.DeclContainer.CheckAccessLevel (t)) {
				NamespaceEntry.Error_NamespaceNotFound (loc, name);
				return null;
			}
			else if (t == typeof(UnexpectedType))
				return null;
			type = t;
			return this;
		}

		public override string Name {
			get { return name; }
		}

		public override string FullName {
			get { return name; }
		}

		protected override void CloneTo (CloneContext clonectx, Expression target)
		{
			// CloneTo: Nothing, we do not keep any state on this expression
		}
	}

	/// <summary>
	///   Represents an "unbound generic type", ie. typeof (Foo<>).
	///   See 14.5.11.
	/// </summary>
	public class UnboundTypeExpression : TypeExpr
	{
		MemberName name;

		public UnboundTypeExpression (MemberName name, Location l)
		{
			this.name = name;
			loc = l;
		}

		protected override TypeExpr DoResolveAsTypeStep (IResolveContext ec)
		{
			Expression expr;
			if (name.Left != null) {
				Expression lexpr = name.Left.GetTypeExpression ();
				expr = new MemberAccess (lexpr, name.Basename);
			} else {
				expr = new SimpleName (name.Basename, loc);
			}

			FullNamedExpression fne = expr.ResolveAsTypeStep (ec, false);
			if (fne == null)
				return null;

			type = fne.Type;
			return new TypeExpression (type, loc);
		}

		public override string Name {
			get { return name.FullName; }
		}

		public override string FullName {
			get { return name.FullName; }
		}
	}

	public class TypeAliasExpression : TypeExpr {
		FullNamedExpression alias;
		TypeExpr texpr;
		TypeArguments args;
		string name;

		public TypeAliasExpression (FullNamedExpression alias, TypeArguments args, Location l)
		{
			this.alias = alias;
			this.args = args;
			loc = l;

			eclass = ExprClass.Type;
			if (args != null)
				name = alias.FullName + "<" + args.ToString () + ">";
			else
				name = alias.FullName;
		}

		public override string Name {
			get { return alias.FullName; }
		}

		public override string FullName {
			get { return name; }
		}

		protected override TypeExpr DoResolveAsTypeStep (IResolveContext ec)
		{
			texpr = alias.ResolveAsTypeTerminal (ec, false);
			if (texpr == null)
				return null;

			Type type = texpr.Type;
			int num_args = TypeManager.GetNumberOfTypeArguments (type);

			if (args != null) {
				if (num_args == 0) {
					Report.Error (308, loc,
						      "The non-generic type `{0}' cannot " +
						      "be used with type arguments.",
						      TypeManager.CSharpName (type));
					return null;
				}

				ConstructedType ctype = new ConstructedType (type, args, loc);
				return ctype.ResolveAsTypeTerminal (ec, false);
			} else if (num_args > 0) {
				Report.Error (305, loc,
					      "Using the generic type `{0}' " +
					      "requires {1} type arguments",
					      TypeManager.CSharpName (type), num_args.ToString ());
				return null;
			}

			return texpr;
		}

		public override bool CheckAccessLevel (DeclSpace ds)
		{
			return texpr.CheckAccessLevel (ds);
		}

		public override bool AsAccessible (DeclSpace ds, int flags)
		{
			return texpr.AsAccessible (ds, flags);
		}

		public override bool IsClass {
			get { return texpr.IsClass; }
		}

		public override bool IsValueType {
			get { return texpr.IsValueType; }
		}

		public override bool IsInterface {
			get { return texpr.IsInterface; }
		}

		public override bool IsSealed {
			get { return texpr.IsSealed; }
		}
	}

	/// <summary>
	///   This class denotes an expression which evaluates to a member
	///   of a struct or a class.
	/// </summary>
	public abstract class MemberExpr : Expression
	{
		/// <summary>
		///   The name of this member.
		/// </summary>
		public abstract string Name {
			get;
		}

		/// <summary>
		///   Whether this is an instance member.
		/// </summary>
		public abstract bool IsInstance {
			get;
		}

		/// <summary>
		///   Whether this is a static member.
		/// </summary>
		public abstract bool IsStatic {
			get;
		}

		/// <summary>
		///   The type which declares this member.
		/// </summary>
		public abstract Type DeclaringType {
			get;
		}

		/// <summary>
		///   The instance expression associated with this member, if it's a
		///   non-static member.
		/// </summary>
		public Expression InstanceExpression;

		public static void error176 (Location loc, string name)
		{
			Report.Error (176, loc, "Static member `{0}' cannot be accessed " +
				      "with an instance reference, qualify it with a type name instead", name);
		}

		// TODO: possible optimalization
		// Cache resolved constant result in FieldBuilder <-> expression map
		public virtual Expression ResolveMemberAccess (EmitContext ec, Expression left, Location loc,
							       SimpleName original)
		{
			//
			// Precondition:
			//   original == null || original.Resolve (...) ==> left
			//

			if (left is TypeExpr) {
				left = left.ResolveAsTypeTerminal (ec, true);
				if (left == null)
					return null;

				if (!IsStatic) {
					SimpleName.Error_ObjectRefRequired (ec, loc, GetSignatureForError ());
					return null;
				}

				return this;
			}
				
			if (!IsInstance) {
				if (original != null && original.IdenticalNameAndTypeName (ec, left, loc))
					return this;

				error176 (loc, GetSignatureForError ());
				return null;
			}

			InstanceExpression = left;

			return this;
		}

		protected void EmitInstance (EmitContext ec, bool prepare_for_load)
		{
			if (IsStatic)
				return;

			if (InstanceExpression == EmptyExpression.Null) {
				SimpleName.Error_ObjectRefRequired (ec, loc, GetSignatureForError ());
				return;
			}
				
			if (InstanceExpression.Type.IsValueType) {
				if (InstanceExpression is IMemoryLocation) {
					((IMemoryLocation) InstanceExpression).AddressOf (ec, AddressOp.LoadStore);
				} else {
					LocalTemporary t = new LocalTemporary (InstanceExpression.Type);
					InstanceExpression.Emit (ec);
					t.Store (ec);
					t.AddressOf (ec, AddressOp.Store);
				}
			} else
				InstanceExpression.Emit (ec);

			if (prepare_for_load)
				ec.ig.Emit (OpCodes.Dup);
		}
	}

	/// 
	/// Represents group of extension methods
	/// 
	public class ExtensionMethodGroupExpr : MethodGroupExpr
	{
		NamespaceEntry namespaceEntry;

		public ExtensionMethodGroupExpr (ArrayList list, NamespaceEntry n, Type extensionType, Location l)
			: base (list, l)
		{
			this.namespaceEntry = n;
			this.type = extensionType;
		}

		public override bool IsBase {
			get { return true; }
		}

		public override bool IsStatic {
			get { return true; }
		}

		public bool IsTopLevel {
			get { return namespaceEntry == null; }
		}

		public override MethodBase OverloadExtensionResolve (EmitContext ec, ref ArrayList arguments, ref MethodGroupExpr mg,
			Expression expr, Location loc)
		{
			if (arguments == null)
				arguments = new ArrayList (1);

			Expression extension_argument = ((MemberAccess)expr).Expr;
			if ((extension_argument.eclass & (ExprClass.Value | ExprClass.Variable)) == 0)
				return null;

			Argument a = new Argument (extension_argument);
			a.Resolve (ec, loc);
			arguments.Insert (0, a);

			mg = this;
			do {
				MethodBase method = mg.OverloadResolve (ec, arguments, true, loc);
				if (method != null)
					return method;

				ExtensionMethodGroupExpr e = namespaceEntry.LookupExtensionMethod (type, null, Name);
				if (e == null)
					return mg.OverloadResolve (ec, arguments, false, loc);

				mg = e;
				namespaceEntry = e.namespaceEntry;
			} while (true);
		}
	}

	/// <summary>
	///   MethodGroup Expression.
	///  
	///   This is a fully resolved expression that evaluates to a type
	/// </summary>
	public class MethodGroupExpr : MemberExpr {
		public MethodBase [] Methods;
		bool has_type_arguments = false;
 		bool identical_type_name = false;
		bool is_base;
		
		public MethodGroupExpr (MemberInfo [] mi, Location l)
		{
			Methods = new MethodBase [mi.Length];
			mi.CopyTo (Methods, 0);
			eclass = ExprClass.MethodGroup;

			// Set the type to something that will never be useful, which will
			// trigger the proper conversions.
			type = typeof (MethodGroupExpr);
			loc = l;
		}

		public MethodGroupExpr (ArrayList list, Location l)
		{
			try {
				Methods = (MethodBase[])list.ToArray (typeof (MethodBase));
			} catch {
				foreach (MemberInfo m in list){
					if (!(m is MethodBase)){
						Console.WriteLine ("Name " + m.Name);
						Console.WriteLine ("Found a: " + m.GetType ().FullName);
					}
				}
				throw;
			}

			loc = l;
			eclass = ExprClass.MethodGroup;
			type = TypeManager.object_type;
		}

		public override Type DeclaringType {
			get {
                                //
                                // We assume that the top-level type is in the end
                                //
				return Methods [Methods.Length - 1].DeclaringType;
                                //return Methods [0].DeclaringType;
			}
		}

		public bool HasTypeArguments {
			get {
				return has_type_arguments;
			}

			set {
				has_type_arguments = value;
			}
		}

		public bool IdenticalTypeName {
			get {
				return identical_type_name;
			}

			set {
				identical_type_name = value;
			}
		}

		public virtual bool IsBase {
			get {
				return is_base;
			}
			set {
				is_base = value;
			}
		}

		public override string GetSignatureForError ()
		{
			return TypeManager.CSharpSignature (Methods [0]);
		}

		public override string Name {
			get {
				return Methods [0].Name;
			}
		}

		public override bool IsInstance {
			get {
				foreach (MethodBase mb in Methods)
					if (!mb.IsStatic)
						return true;

				return false;
			}
		}

		public override bool IsStatic {
			get {
				foreach (MethodBase mb in Methods)
					if (mb.IsStatic)
						return true;

				return false;
			}
		}

		/// <summary>
		///   Determines "better conversion" as specified in 14.4.2.3
		///
		///    Returns : p    if a->p is better,
		///              q    if a->q is better,
		///              null if neither is better
		/// </summary>
		static Type BetterConversion (EmitContext ec, Argument a, Type p, Type q)
		{
			Type argument_type = TypeManager.TypeToCoreType (a.Type);
			Expression argument_expr = a.Expr;

			if (argument_type == null)
				throw new Exception ("Expression of type " + a.Expr +
					" does not resolve its type");

			if (p == null || q == null)
				throw new InternalErrorException ("BetterConversion Got a null conversion");

			if (p == q)
				return null;

			if (argument_expr is NullLiteral) 
			{
				//
				// If the argument is null and one of the types to compare is 'object' and
				// the other is a reference type, we prefer the other.
				//
				// This follows from the usual rules:
				//   * There is an implicit conversion from 'null' to type 'object'
				//   * There is an implicit conversion from 'null' to any reference type
				//   * There is an implicit conversion from any reference type to type 'object'
				//   * There is no implicit conversion from type 'object' to other reference types
				//  => Conversion of 'null' to a reference type is better than conversion to 'object'
				//
				//  FIXME: This probably isn't necessary, since the type of a NullLiteral is the 
				//         null type. I think it used to be 'object' and thus needed a special 
				//         case to avoid the immediately following two checks.
				//
				if (!p.IsValueType && q == TypeManager.object_type)
					return p;
				if (!q.IsValueType && p == TypeManager.object_type)
					return q;
			}
                                
			if (argument_type == p)
				return p;

			if (argument_type == q)
				return q;

			Expression p_tmp = new EmptyExpression (p);
			Expression q_tmp = new EmptyExpression (q);

			bool p_to_q = Convert.ImplicitConversionExists (ec, p_tmp, q);
			bool q_to_p = Convert.ImplicitConversionExists (ec, q_tmp, p);

			if (p_to_q && !q_to_p)
				return p;

			if (q_to_p && !p_to_q)
				return q;

			if (p == TypeManager.sbyte_type)
				if (q == TypeManager.byte_type || q == TypeManager.ushort_type ||
					q == TypeManager.uint32_type || q == TypeManager.uint64_type)
					return p;
			if (q == TypeManager.sbyte_type)
				if (p == TypeManager.byte_type || p == TypeManager.ushort_type ||
					p == TypeManager.uint32_type || p == TypeManager.uint64_type)
					return q;

			if (p == TypeManager.short_type)
				if (q == TypeManager.ushort_type || q == TypeManager.uint32_type ||
					q == TypeManager.uint64_type)
					return p;
			if (q == TypeManager.short_type)
				if (p == TypeManager.ushort_type || p == TypeManager.uint32_type ||
					p == TypeManager.uint64_type)
					return q;

			if (p == TypeManager.int32_type)
				if (q == TypeManager.uint32_type || q == TypeManager.uint64_type)
					return p;
			if (q == TypeManager.int32_type)
				if (p == TypeManager.uint32_type || p == TypeManager.uint64_type)
					return q;

			if (p == TypeManager.int64_type)
				if (q == TypeManager.uint64_type)
					return p;
			if (q == TypeManager.int64_type)
				if (p == TypeManager.uint64_type)
					return q;

			return null;
		}

		/// <summary>
		///   Determines "Better function" between candidate
		///   and the current best match
		/// </summary>
		/// <remarks>
		///    Returns a boolean indicating :
		///     false if candidate ain't better
		///     true  if candidate is better than the current best match
		/// </remarks>
		static bool BetterFunction (EmitContext ec, ArrayList args, int argument_count,
			MethodBase candidate, bool candidate_params,
			MethodBase best, bool best_params)
		{
			ParameterData candidate_pd = TypeManager.GetParameterData (candidate);
			ParameterData best_pd = TypeManager.GetParameterData (best);
		
			bool better_at_least_one = false;
			bool same = true;
			for (int j = 0, c_idx = 0, b_idx = 0; j < argument_count; ++j, ++c_idx, ++b_idx) 
			{
				Argument a = (Argument) args [j];

				Type ct = TypeManager.TypeToCoreType (candidate_pd.ParameterType (c_idx));
				Type bt = TypeManager.TypeToCoreType (best_pd.ParameterType (b_idx));

				if (candidate_params && candidate_pd.ParameterModifier (c_idx) == Parameter.Modifier.PARAMS) 
				{
					ct = TypeManager.GetElementType (ct);
					--c_idx;
				}

				if (best_params && best_pd.ParameterModifier (b_idx) == Parameter.Modifier.PARAMS) 
				{
					bt = TypeManager.GetElementType (bt);
					--b_idx;
				}

				if (ct.Equals (bt))
					continue;

				same = false;
				Type better = BetterConversion (ec, a, ct, bt);

				// for each argument, the conversion to 'ct' should be no worse than 
				// the conversion to 'bt'.
				if (better == bt)
					return false;

				// for at least one argument, the conversion to 'ct' should be better than 
				// the conversion to 'bt'.
				if (better == ct)
					better_at_least_one = true;
			}

			if (better_at_least_one)
				return true;

			//
			// This handles the case
			//
			//   Add (float f1, float f2, float f3);
			//   Add (params decimal [] foo);
			//
			// The call Add (3, 4, 5) should be ambiguous.  Without this check, the
			// first candidate would've chosen as better.
			//
			if (!same)
				return false;

			//
			// The two methods have equal parameter types.  Now apply tie-breaking rules
			//
			if (TypeManager.IsGenericMethod (best) && !TypeManager.IsGenericMethod (candidate))
				return true;
			if (!TypeManager.IsGenericMethod (best) && TypeManager.IsGenericMethod (candidate))
				return false;

			//
			// This handles the following cases:
			//
			//   Trim () is better than Trim (params char[] chars)
			//   Concat (string s1, string s2, string s3) is better than
			//     Concat (string s1, params string [] srest)
			//   Foo (int, params int [] rest) is better than Foo (params int [] rest)
			//
			if (!candidate_params && best_params)
				return true;
			if (candidate_params && !best_params)
				return false;

			int candidate_param_count = candidate_pd.Count;
			int best_param_count = best_pd.Count;

			if (candidate_param_count != best_param_count)
				// can only happen if (candidate_params && best_params)
				return candidate_param_count > best_param_count;

			//
			// now, both methods have the same number of parameters, and the parameters have the same types
			// Pick the "more specific" signature
			//

			MethodBase orig_candidate = TypeManager.DropGenericMethodArguments (candidate);
			MethodBase orig_best = TypeManager.DropGenericMethodArguments (best);

			ParameterData orig_candidate_pd = TypeManager.GetParameterData (orig_candidate);
			ParameterData orig_best_pd = TypeManager.GetParameterData (orig_best);

			bool specific_at_least_once = false;
			for (int j = 0; j < candidate_param_count; ++j) 
			{
				Type ct = TypeManager.TypeToCoreType (orig_candidate_pd.ParameterType (j));
				Type bt = TypeManager.TypeToCoreType (orig_best_pd.ParameterType (j));
				if (ct.Equals (bt))
					continue;
				Type specific = MoreSpecific (ct, bt);
				if (specific == bt)
					return false;
				if (specific == ct)
					specific_at_least_once = true;
			}

			if (specific_at_least_once)
				return true;

			// FIXME: handle lifted operators
			// ...

			return false;
		}

		public override Expression ResolveMemberAccess (EmitContext ec, Expression left, Location loc,
								SimpleName original)
		{
			if (!(left is TypeExpr) &&
			    original != null && original.IdenticalNameAndTypeName (ec, left, loc))
				IdenticalTypeName = true;

			return base.ResolveMemberAccess (ec, left, loc, original);
		}
		
		override public Expression DoResolve (EmitContext ec)
		{
			if (!IsInstance)
				InstanceExpression = null;

			if (InstanceExpression != null) {
				InstanceExpression = InstanceExpression.DoResolve (ec);
				if (InstanceExpression == null)
					return null;
			}

			return this;
		}

		public void ReportUsageError ()
		{
			Report.Error (654, loc, "Method `" + DeclaringType + "." +
				      Name + "()' is referenced without parentheses");
		}

		override public void Emit (EmitContext ec)
		{
			ReportUsageError ();
		}

		public static bool IsAncestralType (Type first_type, Type second_type)
		{
			return first_type != second_type &&
				(TypeManager.IsSubclassOf (second_type, first_type) ||
				TypeManager.ImplementsInterface (second_type, first_type));
		}		

		public static bool IsOverride (MethodBase cand_method, MethodBase base_method)
		{
			if (!IsAncestralType (base_method.DeclaringType, cand_method.DeclaringType))
				return false;

			ParameterData cand_pd = TypeManager.GetParameterData (cand_method);
			ParameterData base_pd = TypeManager.GetParameterData (base_method);
		
			if (cand_pd.Count != base_pd.Count)
				return false;

			for (int j = 0; j < cand_pd.Count; ++j) 
			{
				Parameter.Modifier cm = cand_pd.ParameterModifier (j);
				Parameter.Modifier bm = base_pd.ParameterModifier (j);
				Type ct = TypeManager.TypeToCoreType (cand_pd.ParameterType (j));
				Type bt = TypeManager.TypeToCoreType (base_pd.ParameterType (j));

				if (cm != bm || ct != bt)
					return false;
			}

			return true;
		}

		static Type MoreSpecific (Type p, Type q)
		{
			if (TypeManager.IsGenericParameter (p) && !TypeManager.IsGenericParameter (q))
				return q;
			if (!TypeManager.IsGenericParameter (p) && TypeManager.IsGenericParameter (q))
				return p;

			if (TypeManager.HasElementType (p)) 
			{
				Type pe = TypeManager.GetElementType (p);
				Type qe = TypeManager.GetElementType (q);
				Type specific = MoreSpecific (pe, qe);
				if (specific == pe)
					return p;
				if (specific == qe)
					return q;
			} 
			else if (TypeManager.IsGenericType (p)) 
			{
				Type[] pargs = TypeManager.GetTypeArguments (p);
				Type[] qargs = TypeManager.GetTypeArguments (q);

				bool p_specific_at_least_once = false;
				bool q_specific_at_least_once = false;

				for (int i = 0; i < pargs.Length; i++) 
				{
					Type specific = MoreSpecific (pargs [i], qargs [i]);
					if (specific == pargs [i])
						p_specific_at_least_once = true;
					if (specific == qargs [i])
						q_specific_at_least_once = true;
				}

				if (p_specific_at_least_once && !q_specific_at_least_once)
					return p;
				if (!p_specific_at_least_once && q_specific_at_least_once)
					return q;
			}

			return null;
		}

		public virtual MethodBase OverloadExtensionResolve (EmitContext ec, ref ArrayList arguments, ref MethodGroupExpr mg,
			Expression expr, Location loc)
		{
			MethodBase method = OverloadResolve (ec, arguments, true, loc);
			if (method != null) {
				mg = this;
				return method;
			}

			MemberAccess mexpr = expr as MemberAccess;
			if (mexpr != null) {
				ExtensionMethodGroupExpr emg = ec.DeclContainer.LookupExtensionMethod (mexpr.Expr.Type, Name);
				if (emg != null) {
					return OverloadExtensionResolve (ec, ref arguments, ref mg, expr, loc);
				}
			}

			return OverloadResolve (ec, arguments, false, loc);
		}

		/// <summary>
		///   Find the Applicable Function Members (7.4.2.1)
		///
		///   me: Method Group expression with the members to select.
		///       it might contain constructors or methods (or anything
		///       that maps to a method).
		///
		///   Arguments: ArrayList containing resolved Argument objects.
		///
		///   loc: The location if we want an error to be reported, or a Null
		///        location for "probing" purposes.
		///
		///   Returns: The MethodBase (either a ConstructorInfo or a MethodInfo)
		///            that is the best match of me on Arguments.
		///
		/// </summary>
		public virtual MethodBase OverloadResolve (EmitContext ec, ArrayList Arguments,
			bool may_fail, Location loc)
		{
			MethodBase method = null;
			bool method_params = false;
			Type applicable_type = null;
			int arg_count = 0;
			ArrayList candidates = new ArrayList (2);
			ArrayList candidate_overrides = null;

			//
			// Used to keep a map between the candidate
			// and whether it is being considered in its
			// normal or expanded form
			//
			// false is normal form, true is expanded form
			//
			Hashtable candidate_to_form = null;

			if (Arguments != null)
				arg_count = Arguments.Count;

			if (RootContext.Version == LanguageVersion.ISO_1 && Name == "Invoke" && TypeManager.IsDelegateType (DeclaringType)) {
				if (!may_fail)
					Report.Error (1533, loc, "Invoke cannot be called directly on a delegate");
				return null;
			}

			int nmethods = Methods.Length;

			if (!IsBase) {
				//
				// Methods marked 'override' don't take part in 'applicable_type'
				// computation, nor in the actual overload resolution.
				// However, they still need to be emitted instead of a base virtual method.
				// So, we salt them away into the 'candidate_overrides' array.
				//
				// In case of reflected methods, we replace each overriding method with
				// its corresponding base virtual method.  This is to improve compatibility
				// with non-C# libraries which change the visibility of overrides (#75636)
				//
				int j = 0;
				for (int i = 0; i < Methods.Length; ++i) {
					MethodBase m = Methods [i];
#if GMCS_SOURCE
					Type [] gen_args = null;
					if (m.IsGenericMethod && !m.IsGenericMethodDefinition)
						gen_args = m.GetGenericArguments ();
#endif
					if (TypeManager.IsOverride (m)) {
						if (candidate_overrides == null)
							candidate_overrides = new ArrayList ();
						candidate_overrides.Add (m);
						m = TypeManager.TryGetBaseDefinition (m);
#if GMCS_SOURCE
						if (m != null && gen_args != null) {
							if (!m.IsGenericMethodDefinition)
								throw new InternalErrorException ("GetBaseDefinition didn't return a GenericMethodDefinition");
							m = ((MethodInfo) m).MakeGenericMethod (gen_args);
						}
#endif
					}
					if (m != null)
						Methods [j++] = m;
				}
				nmethods = j;
			}

			int applicable_errors = Report.Errors;
			
			//
			// First we construct the set of applicable methods
			//
			bool is_sorted = true;
			for (int i = 0; i < nmethods; i++) {
				Type decl_type = Methods [i].DeclaringType;

				//
				// If we have already found an applicable method
				// we eliminate all base types (Section 14.5.5.1)
				//
				if (applicable_type != null && IsAncestralType (decl_type, applicable_type))
					continue;

				//
				// Check if candidate is applicable (section 14.4.2.1)
				//   Is candidate applicable in normal form?
				//
				bool is_applicable = Invocation.IsApplicable (ec, this, Arguments, arg_count, ref Methods [i]);

				if (!is_applicable && Invocation.IsParamsMethodApplicable (ec, this, Arguments, arg_count, ref Methods [i])) {
					MethodBase candidate = Methods [i];
					if (candidate_to_form == null)
						candidate_to_form = new PtrHashtable ();
					candidate_to_form [candidate] = candidate;
					// Candidate is applicable in expanded form
					is_applicable = true;
				}

				if (!is_applicable)
					continue;

				candidates.Add (Methods [i]);

				if (applicable_type == null)
					applicable_type = decl_type;
				else if (applicable_type != decl_type) {
					is_sorted = false;
					if (IsAncestralType (applicable_type, decl_type))
						applicable_type = decl_type;
				}
			}

			if (applicable_errors != Report.Errors)
				return null;
			
			int candidate_top = candidates.Count;

			if (applicable_type == null) {
				//
				// Okay so we have failed to find anything so we
				// return by providing info about the closest match
				//
				int errors = Report.Errors;
				for (int i = 0; i < nmethods; ++i) {
					MethodBase c = Methods [i];
					ParameterData pd = TypeManager.GetParameterData (c);

					if (pd.Count != arg_count)
						continue;

#if GMCS_SOURCE
					if (!TypeManager.InferTypeArguments (ec, Arguments, ref c))
						continue;
					if (TypeManager.IsGenericMethodDefinition (c))
						continue;
#endif

					Invocation.VerifyArgumentsCompat (ec, Arguments, arg_count,
						c, false, null, may_fail, loc);

					if (!may_fail && errors == Report.Errors){
						
						throw new InternalErrorException (
							"VerifyArgumentsCompat and IsApplicable do not agree; " +
							"likely reason: ImplicitConversion and ImplicitConversionExists have gone out of sync");
					}

					break;
				}

				if (!may_fail && errors == Report.Errors) {
					string report_name = Name;
					if (report_name == ".ctor")
						report_name = TypeManager.CSharpName (DeclaringType);
                                        
#if GMCS_SOURCE
					//
					// Type inference
					//
					for (int i = 0; i < Methods.Length; ++i) {
						MethodBase c = Methods [i];
						ParameterData pd = TypeManager.GetParameterData (c);

						if (pd.Count != arg_count)
							continue;

						if (TypeManager.InferTypeArguments (ec, Arguments, ref c))
							continue;

						Report.Error (
							411, loc, "The type arguments for " +
							"method `{0}' cannot be inferred from " +
							"the usage. Try specifying the type " +
							"arguments explicitly", TypeManager.CSharpSignature (c));
						return null;
					}
#endif

					Invocation.Error_WrongNumArguments (loc, report_name, arg_count);
				}
                                
				return null;
			}

			if (!is_sorted) {
				//
				// At this point, applicable_type is _one_ of the most derived types
				// in the set of types containing the methods in this MethodGroup.
				// Filter the candidates so that they only contain methods from the
				// most derived types.
				//

				int finalized = 0; // Number of finalized candidates

				do {
					// Invariant: applicable_type is a most derived type
					
					// We'll try to complete Section 14.5.5.1 for 'applicable_type' by 
					// eliminating all it's base types.  At the same time, we'll also move
					// every unrelated type to the end of the array, and pick the next
					// 'applicable_type'.

					Type next_applicable_type = null;
					int j = finalized; // where to put the next finalized candidate
					int k = finalized; // where to put the next undiscarded candidate
					for (int i = finalized; i < candidate_top; ++i) {
						MethodBase candidate = (MethodBase) candidates [i];
						Type decl_type = candidate.DeclaringType;

						if (decl_type == applicable_type) {
							candidates [k++] = candidates [j];
							candidates [j++] = candidates [i];
							continue;
						}

						if (IsAncestralType (decl_type, applicable_type))
							continue;

						if (next_applicable_type != null &&
							IsAncestralType (decl_type, next_applicable_type))
							continue;

						candidates [k++] = candidates [i];

						if (next_applicable_type == null ||
							IsAncestralType (next_applicable_type, decl_type))
							next_applicable_type = decl_type;
					}

					applicable_type = next_applicable_type;
					finalized = j;
					candidate_top = k;
				} while (applicable_type != null);
			}

			//
			// Now we actually find the best method
			//

			method = (MethodBase) candidates [0];
			method_params = candidate_to_form != null && candidate_to_form.Contains (method);
			for (int ix = 1; ix < candidate_top; ix++) {
				MethodBase candidate = (MethodBase) candidates [ix];

				if (candidate == method)
					continue;

				bool cand_params = candidate_to_form != null && candidate_to_form.Contains (candidate);

				if (BetterFunction (ec, Arguments, arg_count, 
					candidate, cand_params,
					method, method_params)) {
					method = candidate;
					method_params = cand_params;
				}
			}
			//
			// Now check that there are no ambiguities i.e the selected method
			// should be better than all the others
			//
			MethodBase ambiguous = null;
			for (int ix = 0; ix < candidate_top; ix++) {
				MethodBase candidate = (MethodBase) candidates [ix];

				if (candidate == method)
					continue;

				bool cand_params = candidate_to_form != null && candidate_to_form.Contains (candidate);
				if (!BetterFunction (ec, Arguments, arg_count,
					method, method_params,
					candidate, cand_params)) 
				{
					if (!may_fail)
						Report.SymbolRelatedToPreviousError (candidate);
					ambiguous = candidate;
				}
			}

			if (ambiguous != null) {
				Report.SymbolRelatedToPreviousError (method);
				Report.Error (121, loc, "The call is ambiguous between the following methods or properties: `{0}' and `{1}'",
					TypeManager.CSharpSignature (ambiguous), TypeManager.CSharpSignature (method));
				return method;
			}

			//
			// If the method is a virtual function, pick an override closer to the LHS type.
			//
			if (!IsBase && method.IsVirtual) {
				if (TypeManager.IsOverride (method))
					throw new InternalErrorException (
						"Should not happen.  An 'override' method took part in overload resolution: " + method);

				if (candidate_overrides != null)
					foreach (MethodBase candidate in candidate_overrides) {
						if (IsOverride (candidate, method))
							method = candidate;
					}
			}

			//
			// And now check if the arguments are all
			// compatible, perform conversions if
			// necessary etc. and return if everything is
			// all right
			//
			if (!Invocation.VerifyArgumentsCompat (ec, Arguments, arg_count, method,
				method_params, null, may_fail, loc))
				return null;

			if (method == null)
				return null;

			MethodBase the_method = TypeManager.DropGenericMethodArguments (method);
#if GMCS_SOURCE
			if (the_method.IsGenericMethodDefinition &&
			    !ConstraintChecker.CheckConstraints (ec, the_method, method, loc))
				return null;
#endif

			IMethodData data = TypeManager.GetMethod (the_method);
			if (data != null)
				data.SetMemberIsUsed ();

			return method;
		}


		bool RemoveMethods (bool keep_static)
		{
			ArrayList smethods = new ArrayList ();

			foreach (MethodBase mb in Methods){
				if (mb.IsStatic == keep_static)
					smethods.Add (mb);
			}

			if (smethods.Count == 0)
				return false;

			Methods = new MethodBase [smethods.Count];
			smethods.CopyTo (Methods, 0);

			return true;
		}
		
		/// <summary>
		///   Removes any instance methods from the MethodGroup, returns
		///   false if the resulting set is empty.
		/// </summary>
		public bool RemoveInstanceMethods ()
		{
			return RemoveMethods (true);
		}

		/// <summary>
		///   Removes any static methods from the MethodGroup, returns
		///   false if the resulting set is empty.
		/// </summary>
		public bool RemoveStaticMethods ()
		{
			return RemoveMethods (false);
		}

		public Expression ResolveGeneric (EmitContext ec, TypeArguments args)
		{
#if GMCS_SOURCE
			if (!args.Resolve (ec))
				return null;

			Type[] atypes = args.Arguments;

			int first_count = 0;
			MethodInfo first = null;

			ArrayList list = new ArrayList ();
			foreach (MethodBase mb in Methods) {
				MethodInfo mi = mb as MethodInfo;
				if ((mi == null) || !mb.IsGenericMethod)
					continue;

				Type[] gen_params = mb.GetGenericArguments ();

				if (first == null) {
					first = mi;
					first_count = gen_params.Length;
				}

				if (gen_params.Length != atypes.Length)
					continue;

				mi = mi.MakeGenericMethod (atypes);
				list.Add (mi);

#if MS_COMPATIBLE
				// MS implementation throws NotSupportedException for GetParameters
				// on unbaked generic method
				Parameters p = ((Parameters)TypeManager.GetParameterData (mi)).Clone ();
				p.InflateTypes (gen_params, atypes);
				TypeManager.RegisterMethod (mi, p);
#endif
			}

			if (list.Count > 0) {
				MethodGroupExpr new_mg = new MethodGroupExpr (list, Location);
				new_mg.InstanceExpression = InstanceExpression;
				new_mg.HasTypeArguments = true;
				new_mg.IsBase = IsBase;
				return new_mg;
			}

			if (first != null) {
				Report.SymbolRelatedToPreviousError (first);
				Report.Error (
					305, loc, "Using the generic method `{0}' requires `{1}' type arguments",
					TypeManager.CSharpSignature (first), first_count.ToString ());
			} else
				Report.Error (
					308, loc, "The non-generic method `{0}' " +
					"cannot be used with type arguments", Name);

			return null;
#else
			throw new NotImplementedException ();
#endif
		}
	}

	/// <summary>
	///   Fully resolved expression that evaluates to a Field
	/// </summary>
	public class FieldExpr : MemberExpr, IAssignMethod, IMemoryLocation, IVariable {
		public readonly FieldInfo FieldInfo;
		VariableInfo variable_info;
		
		LocalTemporary temp;
		bool prepared;
		bool in_initializer;

		public FieldExpr (FieldInfo fi, Location l, bool in_initializer):
			this (fi, l)
		{
			this.in_initializer = in_initializer;
		}
		
		public FieldExpr (FieldInfo fi, Location l)
		{
			FieldInfo = fi;
			eclass = ExprClass.Variable;
			type = TypeManager.TypeToCoreType (fi.FieldType);
			loc = l;
		}

		public override string Name {
			get {
				return FieldInfo.Name;
			}
		}

		public override bool IsInstance {
			get {
				return !FieldInfo.IsStatic;
			}
		}

		public override bool IsStatic {
			get {
				return FieldInfo.IsStatic;
			}
		}

		public override Type DeclaringType {
			get {
				return FieldInfo.DeclaringType;
			}
		}

		public override string GetSignatureForError ()
		{
			return TypeManager.GetFullNameSignature (FieldInfo);
		}

		public VariableInfo VariableInfo {
			get {
				return variable_info;
			}
		}

		public override Expression ResolveMemberAccess (EmitContext ec, Expression left, Location loc,
								SimpleName original)
		{
			FieldInfo fi = TypeManager.GetGenericFieldDefinition (FieldInfo);

			Type t = fi.FieldType;

			if (fi.IsLiteral || (fi.IsInitOnly && t == TypeManager.decimal_type)) {
				IConstant ic = TypeManager.GetConstant (fi);
				if (ic == null) {
					if (fi.IsLiteral) {
						ic = new ExternalConstant (fi);
					} else {
						ic = ExternalConstant.CreateDecimal (fi);
						if (ic == null) {
							return base.ResolveMemberAccess (ec, left, loc, original);
						}
					}
					TypeManager.RegisterConstant (fi, ic);
				}

				bool left_is_type = left is TypeExpr;
				if (!left_is_type && (original == null || !original.IdenticalNameAndTypeName (ec, left, loc))) {
					Report.SymbolRelatedToPreviousError (FieldInfo);
					error176 (loc, TypeManager.GetFullNameSignature (FieldInfo));
					return null;
				}

				if (ic.ResolveValue ()) {
					if (!ec.IsInObsoleteScope)
						ic.CheckObsoleteness (loc);
				}

				return ic.CreateConstantReference (loc);
			}
			
			if (t.IsPointer && !ec.InUnsafe) {
				UnsafeError (loc);
				return null;
			}

			return base.ResolveMemberAccess (ec, left, loc, original);
		}

		override public Expression DoResolve (EmitContext ec)
		{
			return DoResolve (ec, false, false);
		}

		Expression DoResolve (EmitContext ec, bool lvalue_instance, bool out_access)
		{
			if (!FieldInfo.IsStatic){
				if (InstanceExpression == null){
					//
					// This can happen when referencing an instance field using
					// a fully qualified type expression: TypeName.InstanceField = xxx
					// 
					SimpleName.Error_ObjectRefRequired (ec, loc, GetSignatureForError ());
					return null;
				}

				// Resolve the field's instance expression while flow analysis is turned
				// off: when accessing a field "a.b", we must check whether the field
				// "a.b" is initialized, not whether the whole struct "a" is initialized.

				if (lvalue_instance) {
					using (ec.With (EmitContext.Flags.DoFlowAnalysis, false)) {
						Expression right_side =
							out_access ? EmptyExpression.LValueMemberOutAccess : EmptyExpression.LValueMemberAccess;
						InstanceExpression = InstanceExpression.ResolveLValue (ec, right_side, loc);
					}
				} else {
					ResolveFlags rf = ResolveFlags.VariableOrValue | ResolveFlags.DisableFlowAnalysis;
					InstanceExpression = InstanceExpression.Resolve (ec, rf);
				}

				if (InstanceExpression == null)
					return null;

				InstanceExpression.CheckMarshalByRefAccess ();
			}

			if (!in_initializer && !ec.IsFieldInitializer) {
				ObsoleteAttribute oa;
				FieldBase f = TypeManager.GetField (FieldInfo);
				if (f != null) {
					if (!ec.IsInObsoleteScope)
						f.CheckObsoleteness (loc);
                                
					// To be sure that type is external because we do not register generated fields
				} else if (!(FieldInfo.DeclaringType is TypeBuilder)) {                                
					oa = AttributeTester.GetMemberObsoleteAttribute (FieldInfo);
					if (oa != null)
						AttributeTester.Report_ObsoleteMessage (oa, TypeManager.GetFullNameSignature (FieldInfo), loc);
				}
			}

			AnonymousContainer am = ec.CurrentAnonymousMethod;
			if (am != null){
				if (!FieldInfo.IsStatic){
					if (!am.IsIterator && (ec.TypeContainer is Struct)){
 						Report.Error (1673, loc,
 						"Anonymous methods inside structs cannot access instance members of `{0}'. Consider copying `{0}' to a local variable outside the anonymous method and using the local instead",
 							"this");
						return null;
					}
				}
			}
			
			// If the instance expression is a local variable or parameter.
			IVariable var = InstanceExpression as IVariable;
			if ((var == null) || (var.VariableInfo == null))
				return this;

			VariableInfo vi = var.VariableInfo;
			if (!vi.IsFieldAssigned (ec, FieldInfo.Name, loc))
				return null;

			variable_info = vi.GetSubStruct (FieldInfo.Name);
			return this;
		}

		static readonly int [] codes = {
			191,	// instance, write access
			192,	// instance, out access
			198,	// static, write access
			199,	// static, out access
			1648,	// member of value instance, write access
			1649,	// member of value instance, out access
			1650,	// member of value static, write access
			1651	// member of value static, out access
		};

		static readonly string [] msgs = {
			/*0191*/ "A readonly field `{0}' cannot be assigned to (except in a constructor or a variable initializer)",
			/*0192*/ "A readonly field `{0}' cannot be passed ref or out (except in a constructor)",
			/*0198*/ "A static readonly field `{0}' cannot be assigned to (except in a static constructor or a variable initializer)",
			/*0199*/ "A static readonly field `{0}' cannot be passed ref or out (except in a static constructor)",
			/*1648*/ "Members of readonly field `{0}' cannot be modified (except in a constructor or a variable initializer)",
			/*1649*/ "Members of readonly field `{0}' cannot be passed ref or out (except in a constructor)",
			/*1650*/ "Fields of static readonly field `{0}' cannot be assigned to (except in a static constructor or a variable initializer)",
			/*1651*/ "Fields of static readonly field `{0}' cannot be passed ref or out (except in a static constructor)"
		};

		// The return value is always null.  Returning a value simplifies calling code.
		Expression Report_AssignToReadonly (Expression right_side)
		{
			int i = 0;
			if (right_side == EmptyExpression.OutAccess || right_side == EmptyExpression.LValueMemberOutAccess)
				i += 1;
			if (IsStatic)
				i += 2;
			if (right_side == EmptyExpression.LValueMemberAccess || right_side == EmptyExpression.LValueMemberOutAccess)
				i += 4;
			Report.Error (codes [i], loc, msgs [i], GetSignatureForError ());

			return null;
		}
		
		override public Expression DoResolveLValue (EmitContext ec, Expression right_side)
		{
			IVariable var = InstanceExpression as IVariable;
			if ((var != null) && (var.VariableInfo != null))
				var.VariableInfo.SetFieldAssigned (ec, FieldInfo.Name);

			bool lvalue_instance = !FieldInfo.IsStatic && FieldInfo.DeclaringType.IsValueType;
			bool out_access = right_side == EmptyExpression.OutAccess || right_side == EmptyExpression.LValueMemberOutAccess;

			Expression e = DoResolve (ec, lvalue_instance, out_access);

			if (e == null)
				return null;

			FieldBase fb = TypeManager.GetField (FieldInfo);
			if (fb != null)
				fb.SetAssigned ();

			if (FieldInfo.IsInitOnly) {
				// InitOnly fields can only be assigned in constructors or initializers
				if (!ec.IsFieldInitializer && !ec.IsConstructor)
					return Report_AssignToReadonly (right_side);

				if (ec.IsConstructor) {
					Type ctype = ec.TypeContainer.CurrentType;
					if (ctype == null)
						ctype = ec.ContainerType;

					// InitOnly fields cannot be assigned-to in a different constructor from their declaring type
					if (!TypeManager.IsEqual (ctype, FieldInfo.DeclaringType))
						return Report_AssignToReadonly (right_side);
					// static InitOnly fields cannot be assigned-to in an instance constructor
					if (IsStatic && !ec.IsStatic)
						return Report_AssignToReadonly (right_side);
					// instance constructors can't modify InitOnly fields of other instances of the same type
					if (!IsStatic && !(InstanceExpression is This))
						return Report_AssignToReadonly (right_side);
				}
			}

			if (right_side == EmptyExpression.OutAccess &&
			    !IsStatic && !(InstanceExpression is This) && DeclaringType.IsSubclassOf (TypeManager.mbr_type)) {
				Report.SymbolRelatedToPreviousError (DeclaringType);
				Report.Warning (197, 1, loc,
						"Passing `{0}' as ref or out or taking its address may cause a runtime exception because it is a field of a marshal-by-reference class",
						GetSignatureForError ());
			}

			return this;
		}

		public override void CheckMarshalByRefAccess ()
		{
			if (!IsStatic && Type.IsValueType && !(InstanceExpression is This) && DeclaringType.IsSubclassOf (TypeManager.mbr_type)) {
				Report.SymbolRelatedToPreviousError (DeclaringType);
				Report.Warning (1690, 1, loc, "Cannot call methods, properties, or indexers on `{0}' because it is a value type member of a marshal-by-reference class",
						GetSignatureForError ());
			}
		}

		public bool VerifyFixed ()
		{
			IVariable variable = InstanceExpression as IVariable;
			// A variable of the form V.I is fixed when V is a fixed variable of a struct type.
			// We defer the InstanceExpression check after the variable check to avoid a 
			// separate null check on InstanceExpression.
			return variable != null && InstanceExpression.Type.IsValueType && variable.VerifyFixed ();
		}

		public override int GetHashCode ()
		{
			return FieldInfo.GetHashCode ();
		}

		public override bool Equals (object obj)
		{
			FieldExpr fe = obj as FieldExpr;
			if (fe == null)
				return false;

			if (FieldInfo != fe.FieldInfo)
				return false;

			if (InstanceExpression == null || fe.InstanceExpression == null)
				return true;

			return InstanceExpression.Equals (fe.InstanceExpression);
		}
		
		public void Emit (EmitContext ec, bool leave_copy)
		{
			ILGenerator ig = ec.ig;
			bool is_volatile = false;

			FieldBase f = TypeManager.GetField (FieldInfo);
			if (f != null){
				if ((f.ModFlags & Modifiers.VOLATILE) != 0)
					is_volatile = true;

				f.SetMemberIsUsed ();
			}
			
			if (FieldInfo.IsStatic){
				if (is_volatile)
					ig.Emit (OpCodes.Volatile);
				
				ig.Emit (OpCodes.Ldsfld, FieldInfo);
			} else {
				if (!prepared)
					EmitInstance (ec, false);

				if (is_volatile)
					ig.Emit (OpCodes.Volatile);

				IFixedBuffer ff = AttributeTester.GetFixedBuffer (FieldInfo);
				if (ff != null)
				{
					ig.Emit (OpCodes.Ldflda, FieldInfo);
					ig.Emit (OpCodes.Ldflda, ff.Element);
				}
				else {
					ig.Emit (OpCodes.Ldfld, FieldInfo);
				}
			}

			if (leave_copy) {
				ec.ig.Emit (OpCodes.Dup);
				if (!FieldInfo.IsStatic) {
					temp = new LocalTemporary (this.Type);
					temp.Store (ec);
				}
			}
		}

		public void EmitAssign (EmitContext ec, Expression source, bool leave_copy, bool prepare_for_load)
		{
			FieldAttributes fa = FieldInfo.Attributes;
			bool is_static = (fa & FieldAttributes.Static) != 0;
			bool is_readonly = (fa & FieldAttributes.InitOnly) != 0;
			ILGenerator ig = ec.ig;
			prepared = prepare_for_load;

			if (is_readonly && !ec.IsConstructor){
				Report_AssignToReadonly (source);
				return;
			}

			EmitInstance (ec, prepare_for_load);

			source.Emit (ec);
			if (leave_copy) {
				ec.ig.Emit (OpCodes.Dup);
				if (!FieldInfo.IsStatic) {
					temp = new LocalTemporary (this.Type);
					temp.Store (ec);
				}
			}

			FieldBase f = TypeManager.GetField (FieldInfo);
			if (f != null){
				if ((f.ModFlags & Modifiers.VOLATILE) != 0)
					ig.Emit (OpCodes.Volatile);
					
				f.SetAssigned ();
			}

			if (is_static)
				ig.Emit (OpCodes.Stsfld, FieldInfo);
			else 
				ig.Emit (OpCodes.Stfld, FieldInfo);
			
			if (temp != null) {
				temp.Emit (ec);
				temp.Release (ec);
			}
		}

		public override void Emit (EmitContext ec)
		{
			Emit (ec, false);
		}

		public void AddressOf (EmitContext ec, AddressOp mode)
		{
			ILGenerator ig = ec.ig;

			FieldBase f = TypeManager.GetField (FieldInfo);
			if (f != null){
				if ((f.ModFlags & Modifiers.VOLATILE) != 0){
					Report.Warning (420, 1, loc, "`{0}': A volatile fields cannot be passed using a ref or out parameter",
							f.GetSignatureForError ());
					return;
				}
					
				if ((mode & AddressOp.Store) != 0)
					f.SetAssigned ();
				if ((mode & AddressOp.Load) != 0)
					f.SetMemberIsUsed ();
			}

			//
			// Handle initonly fields specially: make a copy and then
			// get the address of the copy.
			//
			bool need_copy;
			if (FieldInfo.IsInitOnly){
				need_copy = true;
				if (ec.IsConstructor){
					if (FieldInfo.IsStatic){
						if (ec.IsStatic)
							need_copy = false;
					} else
						need_copy = false;
				}
			} else
				need_copy = false;
			
			if (need_copy){
				LocalBuilder local;
				Emit (ec);
				local = ig.DeclareLocal (type);
				ig.Emit (OpCodes.Stloc, local);
				ig.Emit (OpCodes.Ldloca, local);
				return;
			}


			if (FieldInfo.IsStatic){
				ig.Emit (OpCodes.Ldsflda, FieldInfo);
			} else {
				if (!prepared)
					EmitInstance (ec, false);
				ig.Emit (OpCodes.Ldflda, FieldInfo);
			}
		}
	}

	//
	// A FieldExpr whose address can not be taken
	//
	public class FieldExprNoAddress : FieldExpr, IMemoryLocation {
		public FieldExprNoAddress (FieldInfo fi, Location loc) : base (fi, loc)
		{
		}
		
		public new void AddressOf (EmitContext ec, AddressOp mode)
		{
			Report.Error (-215, "Report this: Taking the address of a remapped parameter not supported");
		}
	}
	
	/// <summary>
	///   Expression that evaluates to a Property.  The Assign class
	///   might set the `Value' expression if we are in an assignment.
	///
	///   This is not an LValue because we need to re-write the expression, we
	///   can not take data from the stack and store it.  
	/// </summary>
	public class PropertyExpr : MemberExpr, IAssignMethod {
		public readonly PropertyInfo PropertyInfo;

		//
		// This is set externally by the  `BaseAccess' class
		//
		public bool IsBase;
		MethodInfo getter, setter;
		bool is_static;

		bool resolved;
		
		LocalTemporary temp;
		bool prepared;

		public PropertyExpr (Type containerType, PropertyInfo pi, Location l)
		{
			PropertyInfo = pi;
			eclass = ExprClass.PropertyAccess;
			is_static = false;
			loc = l;

			type = TypeManager.TypeToCoreType (pi.PropertyType);

			ResolveAccessors (containerType);
		}

		public override string Name {
			get {
				return PropertyInfo.Name;
			}
		}

		public override bool IsInstance {
			get {
				return !is_static;
			}
		}

		public override bool IsStatic {
			get {
				return is_static;
			}
		}
		
		public override Type DeclaringType {
			get {
				return PropertyInfo.DeclaringType;
			}
		}

		public override string GetSignatureForError ()
		{
			return TypeManager.GetFullNameSignature (PropertyInfo);
		}

		void FindAccessors (Type invocation_type)
		{
			const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
				BindingFlags.Static | BindingFlags.Instance |
				BindingFlags.DeclaredOnly;

			Type current = PropertyInfo.DeclaringType;
			for (; current != null; current = current.BaseType) {
				MemberInfo[] group = TypeManager.MemberLookup (
					invocation_type, invocation_type, current,
					MemberTypes.Property, flags, PropertyInfo.Name, null);

				if (group == null)
					continue;

				if (group.Length != 1)
					// Oooops, can this ever happen ?
					return;

				PropertyInfo pi = (PropertyInfo) group [0];

				if (getter == null)
					getter = pi.GetGetMethod (true);

				if (setter == null)
					setter = pi.GetSetMethod (true);

				MethodInfo accessor = getter != null ? getter : setter;

				if (!accessor.IsVirtual)
					return;
			}
		}

		//
		// We also perform the permission checking here, as the PropertyInfo does not
		// hold the information for the accessibility of its setter/getter
		//
		// TODO: Refactor to use some kind of cache together with GetPropertyFromAccessor
		void ResolveAccessors (Type containerType)
		{
			FindAccessors (containerType);

			if (getter != null) {
				MethodBase the_getter = TypeManager.DropGenericMethodArguments (getter);
				IMethodData md = TypeManager.GetMethod (the_getter);
				if (md != null)
					md.SetMemberIsUsed ();

				is_static = getter.IsStatic;
			}

			if (setter != null) {
				MethodBase the_setter = TypeManager.DropGenericMethodArguments (setter);
				IMethodData md = TypeManager.GetMethod (the_setter);
				if (md != null)
					md.SetMemberIsUsed ();

				is_static = setter.IsStatic;
			}
		}

		bool InstanceResolve (EmitContext ec, bool lvalue_instance, bool must_do_cs1540_check)
		{
			if (is_static) {
				InstanceExpression = null;
				return true;
			}

			if (InstanceExpression == null) {
				SimpleName.Error_ObjectRefRequired (ec, loc, GetSignatureForError ());
				return false;
			}

			InstanceExpression = InstanceExpression.DoResolve (ec);
			if (lvalue_instance && InstanceExpression != null)
				InstanceExpression = InstanceExpression.ResolveLValue (ec, EmptyExpression.LValueMemberAccess, loc);

			if (InstanceExpression == null)
				return false;

			InstanceExpression.CheckMarshalByRefAccess ();

			if (must_do_cs1540_check && (InstanceExpression != EmptyExpression.Null) &&
			    !TypeManager.IsInstantiationOfSameGenericType (InstanceExpression.Type, ec.ContainerType) &&
			    !TypeManager.IsNestedChildOf (ec.ContainerType, InstanceExpression.Type) &&
			    !TypeManager.IsSubclassOf (InstanceExpression.Type, ec.ContainerType)) {
				Report.SymbolRelatedToPreviousError (PropertyInfo);
				Error_CannotAccessProtected (loc, PropertyInfo, InstanceExpression.Type, ec.ContainerType);
				return false;
			}

			return true;
		}

		void Error_PropertyNotFound (MethodInfo mi, bool getter)
		{
			// TODO: correctly we should compare arguments but it will lead to bigger changes
			if (mi is MethodBuilder) {
				Error_TypeDoesNotContainDefinition (loc, PropertyInfo.DeclaringType, Name);
				return;
			}

			StringBuilder sig = new StringBuilder (TypeManager.CSharpName (mi.DeclaringType));
			sig.Append ('.');
			ParameterData iparams = TypeManager.GetParameterData (mi);
			sig.Append (getter ? "get_" : "set_");
			sig.Append (Name);
			sig.Append (iparams.GetSignatureForError ());

			Report.SymbolRelatedToPreviousError (mi);
			Report.Error (1546, loc, "Property `{0}' is not supported by the C# language. Try to call the accessor method `{1}' directly",
				Name, sig.ToString ());
		}
		
		override public Expression DoResolve (EmitContext ec)
		{
			if (resolved)
				return this;

			if (getter != null){
				if (TypeManager.GetParameterData (getter).Count != 0){
					Error_PropertyNotFound (getter, true);
					return null;
				}
			}

			if (getter == null){
				//
				// The following condition happens if the PropertyExpr was
				// created, but is invalid (ie, the property is inaccessible),
				// and we did not want to embed the knowledge about this in
				// the caller routine.  This only avoids double error reporting.
				//
				if (setter == null)
					return null;

				if (InstanceExpression != EmptyExpression.Null) {
					Report.Error (154, loc, "The property or indexer `{0}' cannot be used in this context because it lacks the `get' accessor",
						TypeManager.GetFullNameSignature (PropertyInfo));
					return null;
				}
			} 

			bool must_do_cs1540_check = false;
			if (getter != null &&
			    !IsAccessorAccessible (ec.ContainerType, getter, out must_do_cs1540_check)) {
				PropertyBase.PropertyMethod pm = TypeManager.GetMethod (getter) as PropertyBase.PropertyMethod;
				if (pm != null && pm.HasCustomAccessModifier) {
					Report.SymbolRelatedToPreviousError (pm);
					Report.Error (271, loc, "The property or indexer `{0}' cannot be used in this context because the get accessor is inaccessible",
						TypeManager.CSharpSignature (getter));
				}
				else {
					Report.SymbolRelatedToPreviousError (getter);
					ErrorIsInaccesible (loc, TypeManager.CSharpSignature (getter));
				}
				return null;
			}
			
			if (!InstanceResolve (ec, false, must_do_cs1540_check))
				return null;

			//
			// Only base will allow this invocation to happen.
			//
			if (IsBase && getter.IsAbstract) {
				Error_CannotCallAbstractBase (TypeManager.GetFullNameSignature (PropertyInfo));
				return null;
			}

			if (PropertyInfo.PropertyType.IsPointer && !ec.InUnsafe){
				UnsafeError (loc);
				return null;
			}

			resolved = true;

			return this;
		}

		override public Expression DoResolveLValue (EmitContext ec, Expression right_side)
		{
			if (right_side == EmptyExpression.OutAccess) {
				Report.Error (206, loc, "A property or indexer `{0}' may not be passed as an out or ref parameter",
					      GetSignatureForError ());
				return null;
			}

			if (right_side == EmptyExpression.LValueMemberAccess || right_side == EmptyExpression.LValueMemberOutAccess) {
				Report.Error (1612, loc, "Cannot modify the return value of `{0}' because it is not a variable",
					      GetSignatureForError ());
				return null;
			}

			if (setter == null){
				//
				// The following condition happens if the PropertyExpr was
				// created, but is invalid (ie, the property is inaccessible),
				// and we did not want to embed the knowledge about this in
				// the caller routine.  This only avoids double error reporting.
				//
				if (getter == null)
					return null;
				Report.Error (200, loc, "Property or indexer `{0}' cannot be assigned to (it is read only)",
					      GetSignatureForError ());
				return null;
			}

			if (TypeManager.GetParameterData (setter).Count != 1){
				Error_PropertyNotFound (setter, false);
				return null;
			}

			bool must_do_cs1540_check;
			if (!IsAccessorAccessible (ec.ContainerType, setter, out must_do_cs1540_check)) {
				PropertyBase.PropertyMethod pm = TypeManager.GetMethod (setter) as PropertyBase.PropertyMethod;
				if (pm != null && pm.HasCustomAccessModifier) {
					Report.SymbolRelatedToPreviousError (pm);
					Report.Error (272, loc, "The property or indexer `{0}' cannot be used in this context because the set accessor is inaccessible",
						TypeManager.CSharpSignature (setter));
				}
				else {
					Report.SymbolRelatedToPreviousError (setter);
					ErrorIsInaccesible (loc, TypeManager.CSharpSignature (setter));
				}
				return null;
			}
			
			if (!InstanceResolve (ec, PropertyInfo.DeclaringType.IsValueType, must_do_cs1540_check))
				return null;
			
			//
			// Only base will allow this invocation to happen.
			//
			if (IsBase && setter.IsAbstract){
				Error_CannotCallAbstractBase (TypeManager.GetFullNameSignature (PropertyInfo));
				return null;
			}

			return this;
		}
		
		public override void Emit (EmitContext ec)
		{
			Emit (ec, false);
		}
		
		public void Emit (EmitContext ec, bool leave_copy)
		{
			//
			// Special case: length of single dimension array property is turned into ldlen
			//
			if ((getter == TypeManager.system_int_array_get_length) ||
			    (getter == TypeManager.int_array_get_length)){
				Type iet = InstanceExpression.Type;

				//
				// System.Array.Length can be called, but the Type does not
				// support invoking GetArrayRank, so test for that case first
				//
				if (iet != TypeManager.array_type && (iet.GetArrayRank () == 1)) {
					if (!prepared)
						EmitInstance (ec, false);
					ec.ig.Emit (OpCodes.Ldlen);
					ec.ig.Emit (OpCodes.Conv_I4);
					return;
				}
			}

			Invocation.EmitCall (ec, IsBase, IsStatic, InstanceExpression, getter, null, loc, prepared, false);
			
			if (leave_copy) {
				ec.ig.Emit (OpCodes.Dup);
				if (!is_static) {
					temp = new LocalTemporary (this.Type);
					temp.Store (ec);
				}
			}
		}

		//
		// Implements the IAssignMethod interface for assignments
		//
		public void EmitAssign (EmitContext ec, Expression source, bool leave_copy, bool prepare_for_load)
		{
			Expression my_source = source;

			prepared = prepare_for_load;
			
			if (prepared) {
				source.Emit (ec);
				if (leave_copy) {
					ec.ig.Emit (OpCodes.Dup);
					if (!is_static) {
						temp = new LocalTemporary (this.Type);
						temp.Store (ec);
					}
				}
			} else if (leave_copy) {
				source.Emit (ec);
				if (!is_static) {
					temp = new LocalTemporary (this.Type);
					temp.Store (ec);
				}
				my_source = temp;
			}
			
			ArrayList args = new ArrayList (1);
			args.Add (new Argument (my_source, Argument.AType.Expression));
			
			Invocation.EmitCall (ec, IsBase, IsStatic, InstanceExpression, setter, args, loc, false, prepared);
			
			if (temp != null) {
				temp.Emit (ec);
				temp.Release (ec);
			}
		}
	}

	/// <summary>
	///   Fully resolved expression that evaluates to an Event
	/// </summary>
	public class EventExpr : MemberExpr {
		public readonly EventInfo EventInfo;

		bool is_static;
		MethodInfo add_accessor, remove_accessor;

		public EventExpr (EventInfo ei, Location loc)
		{
			EventInfo = ei;
			this.loc = loc;
			eclass = ExprClass.EventAccess;

			add_accessor = TypeManager.GetAddMethod (ei);
			remove_accessor = TypeManager.GetRemoveMethod (ei);
			if (add_accessor.IsStatic || remove_accessor.IsStatic)
				is_static = true;

			if (EventInfo is MyEventBuilder){
				MyEventBuilder eb = (MyEventBuilder) EventInfo;
				type = eb.EventType;
				eb.SetUsed ();
			} else
				type = EventInfo.EventHandlerType;
		}

		public override string Name {
			get {
				return EventInfo.Name;
			}
		}

		public override bool IsInstance {
			get {
				return !is_static;
			}
		}

		public override bool IsStatic {
			get {
				return is_static;
			}
		}

		public override Type DeclaringType {
			get {
				return EventInfo.DeclaringType;
			}
		}

		public override Expression ResolveMemberAccess (EmitContext ec, Expression left, Location loc,
								SimpleName original)
		{
			//
			// If the event is local to this class, we transform ourselves into a FieldExpr
			//

			if (EventInfo.DeclaringType == ec.ContainerType ||
			    TypeManager.IsNestedChildOf(ec.ContainerType, EventInfo.DeclaringType)) {
				EventField mi = TypeManager.GetEventField (EventInfo);

				if (mi != null) {
					if (!ec.IsInObsoleteScope)
						mi.CheckObsoleteness (loc);

					FieldExpr ml = new FieldExpr (mi.FieldBuilder, loc);

					InstanceExpression = null;
				
					return ml.ResolveMemberAccess (ec, left, loc, original);
				}
			}

			return base.ResolveMemberAccess (ec, left, loc, original);
		}


		bool InstanceResolve (EmitContext ec, bool must_do_cs1540_check)
		{
			if (is_static) {
				InstanceExpression = null;
				return true;
			}

			if (InstanceExpression == null) {
				SimpleName.Error_ObjectRefRequired (ec, loc, GetSignatureForError ());
				return false;
			}

			InstanceExpression = InstanceExpression.DoResolve (ec);
			if (InstanceExpression == null)
				return false;

			//
			// This is using the same mechanism as the CS1540 check in PropertyExpr.
			// However, in the Event case, we reported a CS0122 instead.
			//
			if (must_do_cs1540_check && InstanceExpression != EmptyExpression.Null &&
			    InstanceExpression.Type != ec.ContainerType &&
			    ec.ContainerType.IsSubclassOf (InstanceExpression.Type)) {
				Report.SymbolRelatedToPreviousError (EventInfo);
				ErrorIsInaccesible (loc, TypeManager.CSharpSignature (EventInfo));
				return false;
			}

			return true;
		}

		public override Expression DoResolveLValue (EmitContext ec, Expression right_side)
		{
			return DoResolve (ec);
		}

		public override Expression DoResolve (EmitContext ec)
		{
			bool must_do_cs1540_check;
			if (!(IsAccessorAccessible (ec.ContainerType, add_accessor, out must_do_cs1540_check) &&
			      IsAccessorAccessible (ec.ContainerType, remove_accessor, out must_do_cs1540_check))) {
				Report.SymbolRelatedToPreviousError (EventInfo);
				ErrorIsInaccesible (loc, TypeManager.CSharpSignature (EventInfo));
				return null;
			}

			if (!InstanceResolve (ec, must_do_cs1540_check))
				return null;
			
			return this;
		}		

		public override void Emit (EmitContext ec)
		{
			if (InstanceExpression is This)
				Report.Error (79, loc, "The event `{0}' can only appear on the left hand side of += or -=", GetSignatureForError ());
			else
				Report.Error (70, loc, "The event `{0}' can only appear on the left hand side of += or -= "+
					      "(except on the defining type)", Name);
		}

		public override string GetSignatureForError ()
		{
			return TypeManager.CSharpSignature (EventInfo);
		}

		public void EmitAddOrRemove (EmitContext ec, Expression source)
		{
			BinaryDelegate source_del = source as BinaryDelegate;
			if (source_del == null) {
				Emit (ec);
				return;
			}
			Expression handler = source_del.Right;
			
			Argument arg = new Argument (handler, Argument.AType.Expression);
			ArrayList args = new ArrayList ();
				
			args.Add (arg);
			
			if (source_del.IsAddition)
				Invocation.EmitCall (
					ec, false, IsStatic, InstanceExpression, add_accessor, args, loc);
			else
				Invocation.EmitCall (
					ec, false, IsStatic, InstanceExpression, remove_accessor, args, loc);
		}
	}

	public class TemporaryVariable : Expression, IMemoryLocation
	{
		LocalInfo li;
		Variable var;
		
		public TemporaryVariable (Type type, Location loc)
		{
			this.type = type;
			this.loc = loc;
			eclass = ExprClass.Value;
		}
		
		public override Expression DoResolve (EmitContext ec)
		{
			if (li != null)
				return this;
			
			TypeExpr te = new TypeExpression (type, loc);
			li = ec.CurrentBlock.AddTemporaryVariable (te, loc);
			if (!li.Resolve (ec))
				return null;

			if (ec.MustCaptureVariable (li)) {
				ScopeInfo scope = li.Block.CreateScopeInfo ();
				var = scope.AddLocal (li);
				type = var.Type;
			}
			
			return this;
		}

		public Variable Variable {
			get { return var != null ? var : li.Variable; }
		}
		
		public override void Emit (EmitContext ec)
		{
			Variable.EmitInstance (ec);
			Variable.Emit (ec);
		}
		
		public void EmitLoadAddress (EmitContext ec)
		{
			Variable.EmitInstance (ec);
			Variable.EmitAddressOf (ec);
		}
		
		public void Store (EmitContext ec, Expression right_side)
		{
			Variable.EmitInstance (ec);
			right_side.Emit (ec);
			Variable.EmitAssign (ec);
		}
		
		public void EmitThis (EmitContext ec)
		{
			Variable.EmitInstance (ec);
		}
		
		public void EmitStore (EmitContext ec)
		{
			Variable.EmitAssign (ec);
		}
		
		public void AddressOf (EmitContext ec, AddressOp mode)
		{
			EmitLoadAddress (ec);
		}
	}
	
	public sealed class VarExpr : Expression
	{
		public bool Handled;
		public VarExpr (Location loc)
		{
			this.loc = loc;
		}
		
		public override Expression DoResolve (EmitContext ec)
		{
			return null;
		}
		
		public override void Emit (EmitContext ec)
		{
		}
	}
	
}	
