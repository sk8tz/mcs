//
// codegen.cs: The code generator
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// Copyright 2001, 2002, 2003 Ximian, Inc.
// Copyright 2004 Novell, Inc.
//

//
// Please leave this defined on SVN: The idea is that when we ship the
// compiler to end users, if the compiler crashes, they have a chance
// to narrow down the problem.   
//
// Only remove it if you need to debug locally on your tree.
//
//#define PRODUCTION

using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Security.Permissions;

using Mono.Security.Cryptography;

namespace Mono.CSharp {

	/// <summary>
	///    Code generator class.
	/// </summary>
	public class CodeGen {
		static AppDomain current_domain;

		public static AssemblyClass Assembly;

		static CodeGen ()
		{
			Reset ();
		}

		public static void Reset ()
		{
			Assembly = new AssemblyClass ();
		}

		public static string Basename (string name)
		{
			int pos = name.LastIndexOf ('/');

			if (pos != -1)
				return name.Substring (pos + 1);

			pos = name.LastIndexOf ('\\');
			if (pos != -1)
				return name.Substring (pos + 1);

			return name;
		}

		public static string Dirname (string name)
		{
			int pos = name.LastIndexOf ('/');

			if (pos != -1)
				return name.Substring (0, pos);

			pos = name.LastIndexOf ('\\');
			if (pos != -1)
				return name.Substring (0, pos);

			return ".";
		}

		static public string FileName;

#if MS_COMPATIBLE
		const AssemblyBuilderAccess COMPILER_ACCESS = 0;
#else
		/* Keep this in sync with System.Reflection.Emit.AssemblyBuilder */
		const AssemblyBuilderAccess COMPILER_ACCESS = (AssemblyBuilderAccess) 0x800;
#endif
				
		//
		// Initializes the code generator variables for interactive use (repl)
		//
		static public void InitDynamic (string name)
		{
			current_domain = AppDomain.CurrentDomain;
			AssemblyName an = Assembly.GetAssemblyName (name, name);
			
			Assembly.Builder = current_domain.DefineDynamicAssembly (an, AssemblyBuilderAccess.Run | COMPILER_ACCESS);
			RootContext.ToplevelTypes = new ModuleContainer (true);
			RootContext.ToplevelTypes.Builder = Assembly.Builder.DefineDynamicModule (Basename (name), false);
			Assembly.Name = Assembly.Builder.GetName ();
		}
		
		//
		// Initializes the code generator variables
		//
		static public bool Init (string name, string output, bool want_debugging_support)
		{
			FileName = output;
			AssemblyName an = Assembly.GetAssemblyName (name, output);
			if (an == null)
				return false;

			if (an.KeyPair != null) {
				// If we are going to strong name our assembly make
				// sure all its refs are strong named
				foreach (Assembly a in GlobalRootNamespace.Instance.Assemblies) {
					AssemblyName ref_name = a.GetName ();
					byte [] b = ref_name.GetPublicKeyToken ();
					if (b == null || b.Length == 0) {
						Report.Error (1577, "Assembly generation failed " +
								"-- Referenced assembly '" +
								ref_name.Name +
								"' does not have a strong name.");
						//Environment.Exit (1);
					}
				}
			}
			
			current_domain = AppDomain.CurrentDomain;

			try {
				Assembly.Builder = current_domain.DefineDynamicAssembly (an,
					AssemblyBuilderAccess.RunAndSave | COMPILER_ACCESS, Dirname (name));
			}
			catch (ArgumentException) {
				// specified key may not be exportable outside it's container
				if (RootContext.StrongNameKeyContainer != null) {
					Report.Error (1548, "Could not access the key inside the container `" +
						RootContext.StrongNameKeyContainer + "'.");
					Environment.Exit (1);
				}
				throw;
			}
			catch (CryptographicException) {
				if ((RootContext.StrongNameKeyContainer != null) || (RootContext.StrongNameKeyFile != null)) {
					Report.Error (1548, "Could not use the specified key to strongname the assembly.");
					Environment.Exit (1);
				}
				return false;
			}

			// Get the complete AssemblyName from the builder
			// (We need to get the public key and token)
			Assembly.Name = Assembly.Builder.GetName ();

			//
			// Pass a path-less name to DefineDynamicModule.  Wonder how
			// this copes with output in different directories then.
			// FIXME: figure out how this copes with --output /tmp/blah
			//
			// If the third argument is true, the ModuleBuilder will dynamically
			// load the default symbol writer.
			//
			try {
				RootContext.ToplevelTypes.Builder = Assembly.Builder.DefineDynamicModule (
					Basename (name), Basename (output), want_debugging_support);

#if !MS_COMPATIBLE
				// TODO: We should use SymbolWriter from DefineDynamicModule
				if (want_debugging_support && !SymbolWriter.Initialize (RootContext.ToplevelTypes.Builder, output)) {
					Report.Error (40, "Unexpected debug information initialization error `{0}'",
						"Could not find the symbol writer assembly (Mono.CompilerServices.SymbolWriter.dll)");
					return false;
				}
#endif
			} catch (ExecutionEngineException e) {
				Report.Error (40, "Unexpected debug information initialization error `{0}'",
					e.Message);
				return false;
			}

			return true;
		}

		static public void Save (string name, bool saveDebugInfo)
		{
#if GMCS_SOURCE
			PortableExecutableKinds pekind;
			ImageFileMachine machine;

			switch (RootContext.Platform) {
			case Platform.X86:
				pekind = PortableExecutableKinds.Required32Bit;
				machine = ImageFileMachine.I386;
				break;
			case Platform.X64:
				pekind = PortableExecutableKinds.PE32Plus;
				machine = ImageFileMachine.AMD64;
				break;
			case Platform.IA64:
				pekind = PortableExecutableKinds.PE32Plus;
				machine = ImageFileMachine.IA64;
				break;
			case Platform.AnyCPU:
			default:
				pekind = PortableExecutableKinds.ILOnly;
				machine = ImageFileMachine.I386;
				break;
			}
#endif
			try {
#if GMCS_SOURCE
				Assembly.Builder.Save (Basename (name), pekind, machine);
#else
				Assembly.Builder.Save (Basename (name));
#endif
			}
			catch (COMException) {
				if ((RootContext.StrongNameKeyFile == null) || (!RootContext.StrongNameDelaySign))
					throw;

				// FIXME: it seems Microsoft AssemblyBuilder doesn't like to delay sign assemblies 
				Report.Error (1548, "Couldn't delay-sign the assembly with the '" +
					RootContext.StrongNameKeyFile +
					"', Use MCS with the Mono runtime or CSC to compile this assembly.");
			}
			catch (System.IO.IOException io) {
				Report.Error (16, "Could not write to file `"+name+"', cause: " + io.Message);
				return;
			}
			catch (System.UnauthorizedAccessException ua) {
				Report.Error (16, "Could not write to file `"+name+"', cause: " + ua.Message);
				return;
			}
			catch (System.NotImplementedException nie) {
				Report.RuntimeMissingSupport (Location.Null, nie.Message);
				return;
			}

			//
			// Write debuger symbol file
			//
			if (saveDebugInfo)
				SymbolWriter.WriteSymbolFile ();
			}
	}


	/// <summary>
	/// An interface to hold all the information needed in the resolving phase.
	/// </summary>
	public interface IMemberContext
	{
		//
		// A scope type context, it can be inflated for generic types
		//
		Type CurrentType { get; }

		//
		// A scope type parameters either VAR or MVAR
		//
		TypeParameter[] CurrentTypeParameters { get; }

		//
		// A type definition of the type context. For partial types definition use
		// CurrentTypeDefinition.PartialContainer otherwise the context is local
		//
		// TODO: CurrentType.Definition
		//
		TypeContainer CurrentTypeDefinition { get; }

		bool IsInObsoleteScope { get; }
		bool IsInUnsafeScope { get; }
		bool IsStatic { get; }

		ExtensionMethodGroupExpr LookupExtensionMethod (Type extensionType, string name, Location loc);
		FullNamedExpression LookupNamespaceOrType (string name, Location loc, bool ignore_cs0104);
		FullNamedExpression LookupNamespaceAlias (string name);
	}

	//
	// Block or statement resolving context
	//
	public class BlockContext : ResolveContext
	{
		FlowBranching current_flow_branching;

		public TypeInferenceContext ReturnTypeInference;

		public BlockContext (IMemberContext mc, ExplicitBlock block, Type returnType)
			: base (mc)
		{
			if (returnType == null)
				throw new ArgumentNullException ("returnType");

			this.return_type = returnType;

			// TODO: check for null value
			CurrentBlock = block;
		}

		public override FlowBranching CurrentBranching {
			get { return current_flow_branching; }
		}

		// <summary>
		//   Starts a new code branching.  This inherits the state of all local
		//   variables and parameters from the current branching.
		// </summary>
		public FlowBranching StartFlowBranching (FlowBranching.BranchingType type, Location loc)
		{
			current_flow_branching = FlowBranching.CreateBranching (CurrentBranching, type, null, loc);
			return current_flow_branching;
		}

		// <summary>
		//   Starts a new code branching for block `block'.
		// </summary>
		public FlowBranching StartFlowBranching (Block block)
		{
			Set (Options.DoFlowAnalysis);

			current_flow_branching = FlowBranching.CreateBranching (
				CurrentBranching, FlowBranching.BranchingType.Block, block, block.StartLocation);
			return current_flow_branching;
		}

		public FlowBranchingTryCatch StartFlowBranching (TryCatch stmt)
		{
			FlowBranchingTryCatch branching = new FlowBranchingTryCatch (CurrentBranching, stmt);
			current_flow_branching = branching;
			return branching;
		}

		public FlowBranchingException StartFlowBranching (ExceptionStatement stmt)
		{
			FlowBranchingException branching = new FlowBranchingException (CurrentBranching, stmt);
			current_flow_branching = branching;
			return branching;
		}

		public FlowBranchingLabeled StartFlowBranching (LabeledStatement stmt)
		{
			FlowBranchingLabeled branching = new FlowBranchingLabeled (CurrentBranching, stmt);
			current_flow_branching = branching;
			return branching;
		}

		public FlowBranchingIterator StartFlowBranching (Iterator iterator)
		{
			FlowBranchingIterator branching = new FlowBranchingIterator (CurrentBranching, iterator);
			current_flow_branching = branching;
			return branching;
		}

		public FlowBranchingToplevel StartFlowBranching (ToplevelBlock stmt, FlowBranching parent)
		{
			FlowBranchingToplevel branching = new FlowBranchingToplevel (parent, stmt);
			current_flow_branching = branching;
			return branching;
		}

		// <summary>
		//   Ends a code branching.  Merges the state of locals and parameters
		//   from all the children of the ending branching.
		// </summary>
		public bool EndFlowBranching ()
		{
			FlowBranching old = current_flow_branching;
			current_flow_branching = current_flow_branching.Parent;

			FlowBranching.UsageVector vector = current_flow_branching.MergeChild (old);
			return vector.IsUnreachable;
		}

		// <summary>
		//   Kills the current code branching.  This throws away any changed state
		//   information and should only be used in case of an error.
		// </summary>
		// FIXME: this is evil
		public void KillFlowBranching ()
		{
			current_flow_branching = current_flow_branching.Parent;
		}

		//
		// This method is used during the Resolution phase to flag the
		// need to define the ReturnLabel
		//
		public void NeedReturnLabel ()
		{
			if (!HasReturnLabel)
				HasReturnLabel = true;
		}
	}

	/// <summary>
	///   An Emit Context is created for each body of code (from methods,
	///   properties bodies, indexer bodies or constructor bodies)
	/// </summary>
	public class EmitContext : IMemberContext {

		[Flags]
		public enum Options
		{
			/// <summary>
			///   This flag tracks the `checked' state of the compilation,
			///   it controls whether we should generate code that does overflow
			///   checking, or if we generate code that ignores overflows.
			///
			///   The default setting comes from the command line option to generate
			///   checked or unchecked code plus any source code changes using the
			///   checked/unchecked statements or expressions.   Contrast this with
			///   the ConstantCheckState flag.
			/// </summary>
			CheckedScope = 1 << 0,

			/// <summary>
			///   The constant check state is always set to `true' and cant be changed
			///   from the command line.  The source code can change this setting with
			///   the `checked' and `unchecked' statements and expressions. 
			/// </summary>
			ConstantCheckState = 1 << 1,

			AllCheckStateFlags = CheckedScope | ConstantCheckState,

			//
			// unsafe { ... } scope
			//
			UnsafeScope = 1 << 2,
			CatchScope = 1 << 3,
			FinallyScope = 1 << 4,
			FieldInitializerScope = 1 << 5,
			CompoundAssignmentScope = 1 << 6,
			FixedInitializerScope = 1 << 7,
			BaseInitializer = 1 << 8,

			//
			// Inside an enum definition, we do not resolve enumeration values
			// to their enumerations, but rather to the underlying type/value
			// This is so EnumVal + EnumValB can be evaluated.
			//
			// There is no "E operator + (E x, E y)", so during an enum evaluation
			// we relax the rules
			//
			EnumScope = 1 << 9,

			ConstantScope = 1 << 10,

			ConstructorScope = 1 << 11,

			/// <summary>
			///   Whether control flow analysis is enabled
			/// </summary>
			DoFlowAnalysis = 1 << 20,

			/// <summary>
			///   Whether control flow analysis is disabled on structs
			///   (only meaningful when DoFlowAnalysis is set)
			/// </summary>
			OmitStructFlowAnalysis = 1 << 21,

			///
			/// Indicates the current context is in probing mode, no errors are reported. 
			///
			ProbingMode = 1	<<	22,

			//
			// Return and ContextualReturn statements will set the ReturnType
			// value based on the expression types of each return statement
			// instead of the method return type which is initially null.
			//
			InferReturnType = 1 << 23,

			OmitDebuggingInfo = 1 << 24
		}

		// utility helper for CheckExpr, UnCheckExpr, Checked and Unchecked statements
		// it's public so that we can use a struct at the callsite
		public struct FlagsHandle : IDisposable
		{
			EmitContext ec;
			readonly Options invmask, oldval;

			public FlagsHandle (EmitContext ec, Options flagsToSet)
				: this (ec, flagsToSet, flagsToSet)
			{
			}

			internal FlagsHandle (EmitContext ec, Options mask, Options val)
			{
				this.ec = ec;
				invmask = ~mask;
				oldval = ec.flags & mask;
				ec.flags = (ec.flags & invmask) | (val & mask);

				if ((mask & Options.ProbingMode) != 0)
					Report.DisableReporting ();
			}

			public void Dispose ()
			{
				if ((invmask & Options.ProbingMode) == 0)
					Report.EnableReporting ();

				ec.flags = (ec.flags & invmask) | oldval;
			}
		}

		protected Options flags;

		public ILGenerator ig;

		/// <summary>
		///   The value that is allowed to be returned or NULL if there is no
		///   return type.
		/// </summary>
		protected Type return_type;

		/// <summary>
		///   Keeps track of the Type to LocalBuilder temporary storage created
		///   to store structures (used to compute the address of the structure
		///   value on structure method invocations)
		/// </summary>
		public Hashtable temporary_storage;

		public Block CurrentBlock;

		/// <summary>
		///   The location where we store the return value.
		/// </summary>
		public LocalBuilder return_value;

		/// <summary>
		///   The location where return has to jump to return the
		///   value
		/// </summary>
		public Label ReturnLabel;

		/// <summary>
		///   If we already defined the ReturnLabel
		/// </summary>
		public bool HasReturnLabel;

		/// <summary>
		///  Whether we are inside an anonymous method.
		/// </summary>
		public AnonymousExpression CurrentAnonymousMethod;
		
		public IMemberContext MemberContext;

		/// <summary>
		///    The current iterator
		/// </summary>
		public Iterator CurrentIterator {
			get { return CurrentAnonymousMethod as Iterator; }
		}

		public EmitContext (IMemberContext rc, ILGenerator ig, Type return_type)
		{
			this.MemberContext = rc;
			this.ig = ig;

			//
			// The default setting comes from the command line option
			//
			if (RootContext.Checked)
				flags |= Options.CheckedScope;

			//
			// The constant check state is always set to true
			//
			flags |= Options.ConstantCheckState;

			this.return_type = return_type;
		}

		public virtual FlowBranching CurrentBranching {
			get { return null; }
		}

		public Type CurrentType {
			get { return MemberContext.CurrentType; }
		}

		public TypeParameter[] CurrentTypeParameters {
			get { return MemberContext.CurrentTypeParameters; }
		}

		public TypeContainer CurrentTypeDefinition {
			get { return MemberContext.CurrentTypeDefinition; }
		}

		public bool InUnsafe {
			get { return HasSet (Options.UnsafeScope) || MemberContext.IsInUnsafeScope; }
		}

		public bool HasSet (Options options)
		{
			return (this.flags & options) == options;
		}

		public bool HasAny (Options options)
		{
			return (this.flags & options) != 0;
		}

		// Temporarily set all the given flags to the given value.  Should be used in an 'using' statement
		public FlagsHandle Set (Options options)
		{
			return new FlagsHandle (this, options);
		}

		public FlagsHandle With (Options options, bool enable)
		{
			return new FlagsHandle (this, options, enable ? options : 0);
		}

		public FlagsHandle WithFlowAnalysis (bool do_flow_analysis, bool omit_struct_analysis)
		{
			Options newflags = 
				(do_flow_analysis ? Options.DoFlowAnalysis : 0) |
				(omit_struct_analysis ? Options.OmitStructFlowAnalysis : 0);
			return new FlagsHandle (this, Options.DoFlowAnalysis | Options.OmitStructFlowAnalysis, newflags);
		}
		
		// IResolveContext.IsInObsoleteScope
		public bool IsInObsoleteScope {
			get {
				// Disables obsolete checks when probing is on
				return IsInProbingMode || MemberContext.IsInObsoleteScope;
			}
		}

		public bool IsInProbingMode {
			get { return (flags & Options.ProbingMode) != 0; }
		}

		bool IMemberContext.IsInUnsafeScope {
			get { return InUnsafe; }
		}

		public bool IsStatic {
			get { return MemberContext.IsStatic; }
		}

		public bool OmitDebuggingInfo {
			get { return (flags & Options.OmitDebuggingInfo) != 0; }
			set {
				if (value)
					flags |= Options.OmitDebuggingInfo;
				else
					flags &= ~Options.OmitDebuggingInfo;
			}
		}
		
		public Type ReturnType {
			get {
				return return_type;
			}
		}

		/// <summary>
		///   This is called immediately before emitting an IL opcode to tell the symbol
		///   writer to which source line this opcode belongs.
		/// </summary>
		public void Mark (Location loc)
		{
			if (!SymbolWriter.HasSymbolWriter || OmitDebuggingInfo || loc.IsNull)
				return;

			SymbolWriter.MarkSequencePoint (ig, loc);
		}

		public void DefineLocalVariable (string name, LocalBuilder builder)
		{
			SymbolWriter.DefineLocalVariable (name, builder);
		}

		public void BeginScope ()
		{
			ig.BeginScope();
			SymbolWriter.OpenScope(ig);
		}

		public void EndScope ()
		{
			ig.EndScope();
			SymbolWriter.CloseScope(ig);
		}

		/// <summary>
		///   Returns a temporary storage for a variable of type t as 
		///   a local variable in the current body.
		/// </summary>
		public LocalBuilder GetTemporaryLocal (Type t)
		{
			if (temporary_storage != null) {
				object o = temporary_storage [t];
				if (o != null) {
					if (o is Stack) {
						Stack s = (Stack) o;
						o = s.Count == 0 ? null : s.Pop ();
					} else {
						temporary_storage.Remove (t);
					}
				}
				if (o != null)
					return (LocalBuilder) o;
			}
			return ig.DeclareLocal (t);
		}

		public void FreeTemporaryLocal (LocalBuilder b, Type t)
		{
			if (temporary_storage == null) {
				temporary_storage = new Hashtable ();
				temporary_storage [t] = b;
				return;
			}
			object o = temporary_storage [t];
			if (o == null) {
				temporary_storage [t] = b;
				return;
			}
			Stack s = o as Stack;
			if (s == null) {
				s = new Stack ();
				s.Push (o);
				temporary_storage [t] = s;
			}
			s.Push (b);
		}

		/// <summary>
		///   Current loop begin and end labels.
		/// </summary>
		public Label LoopBegin, LoopEnd;

		/// <summary>
		///   Default target in a switch statement.   Only valid if
		///   InSwitch is true
		/// </summary>
		public Label DefaultTarget;

		/// <summary>
		///   If this is non-null, points to the current switch statement
		/// </summary>
		public Switch Switch;

		/// <summary>
		///   ReturnValue creates on demand the LocalBuilder for the
		///   return value from the function.  By default this is not
		///   used.  This is only required when returns are found inside
		///   Try or Catch statements.
		///
		///   This method is typically invoked from the Emit phase, so
		///   we allow the creation of a return label if it was not
		///   requested during the resolution phase.   Could be cleaned
		///   up, but it would replicate a lot of logic in the Emit phase
		///   of the code that uses it.
		/// </summary>
		public LocalBuilder TemporaryReturn ()
		{
			if (return_value == null){
				return_value = ig.DeclareLocal (return_type);
				if (!HasReturnLabel){
					ReturnLabel = ig.DefineLabel ();
					HasReturnLabel = true;
				}
			}

			return return_value;
		}

		#region IResolveContext Members

		public ExtensionMethodGroupExpr LookupExtensionMethod (Type extensionType, string name, Location loc)
		{
			return MemberContext.LookupExtensionMethod (extensionType, name, loc);
		}

		public FullNamedExpression LookupNamespaceOrType (string name, Location loc, bool ignore_cs0104)
		{
			return MemberContext.LookupNamespaceOrType (name, loc, ignore_cs0104);
		}

		public FullNamedExpression LookupNamespaceAlias (string name)
		{
			return MemberContext.LookupNamespaceAlias (name);
		}

		#endregion
	}

	public class ResolveContext : EmitContext
	{
		//
		// Holds a varible used during collection or object initialization.
		//
		public Expression CurrentInitializerVariable;

		public ResolveContext (IMemberContext mc)
			: base (mc, null, null)
		{
		}

		public ResolveContext (IMemberContext mc, Options options)
			: this (mc)
		{
			Set (options);
		}

		public bool ConstantCheckState {
			get { return (flags & Options.ConstantCheckState) != 0; }
		}

		public bool DoFlowAnalysis {
			get { return (flags & Options.DoFlowAnalysis) != 0; }
		}

		public bool IsVariableCapturingRequired {
			get {
				return !IsInProbingMode && (CurrentBranching == null || !CurrentBranching.CurrentUsageVector.IsUnreachable);
			}
		}

		public bool OmitStructFlowAnalysis {
			get { return (flags & Options.OmitStructFlowAnalysis) != 0; }
		}

		// TODO: Merge with CompilerGeneratedThis
		public Expression GetThis (Location loc)
		{
			This my_this;
			if (CurrentBlock != null)
				my_this = new This (CurrentBlock, loc);
			else
				my_this = new This (loc);

			if (!my_this.ResolveBase (this))
				my_this = null;

			return my_this;
		}

		public bool MustCaptureVariable (LocalInfo local)
		{
			if (CurrentAnonymousMethod == null)
				return false;

			// FIXME: IsIterator is too aggressive, we should capture only if child
			// block contains yield
			if (CurrentAnonymousMethod.IsIterator)
				return true;

			return local.Block.Toplevel != CurrentBlock.Toplevel;
		}
	}


	public abstract class CommonAssemblyModulClass : Attributable, IMemberContext
	{
		public void AddAttributes (ArrayList attrs, IMemberContext context)
		{
			foreach (Attribute a in attrs)
				a.AttachTo (this, context);

			if (attributes == null) {
				attributes = new Attributes (attrs);
				return;
			}
			attributes.AddAttributes (attrs);
		}

		public virtual void Emit (TypeContainer tc) 
		{
			if (OptAttributes == null)
				return;

			OptAttributes.Emit ();
		}

		protected Attribute ResolveAttribute (PredefinedAttribute a_type)
		{
			Attribute a = OptAttributes.Search (a_type);
			if (a != null) {
				a.Resolve ();
			}
			return a;
		}

		#region IResolveContext Members

		public Type CurrentType {
			get { return null; }
		}

		public TypeParameter[] CurrentTypeParameters {
			get { return null; }
		}

		public TypeContainer CurrentTypeDefinition {
			get { return RootContext.ToplevelTypes; }
		}

		public DeclSpace DeclContainer {
			get { return RootContext.ToplevelTypes; }
		}

		public bool IsInObsoleteScope {
			get { return false; }
		}

		public bool IsInUnsafeScope {
			get { return false; }
		}

		public bool IsStatic {
			get { return false; }
		}

		public ExtensionMethodGroupExpr LookupExtensionMethod (Type extensionType, string name, Location loc)
		{
			throw new NotImplementedException ();
		}

		public FullNamedExpression LookupNamespaceOrType (string name, Location loc, bool ignore_cs0104)
		{
			return RootContext.ToplevelTypes.LookupNamespaceOrType (name, loc, ignore_cs0104);
		}

		public FullNamedExpression LookupNamespaceAlias (string name)
		{
			return null;
		}

		#endregion
	}
                
	public class AssemblyClass : CommonAssemblyModulClass {
		// TODO: make it private and move all builder based methods here
		public AssemblyBuilder Builder;
		bool is_cls_compliant;
		bool wrap_non_exception_throws;

		public Attribute ClsCompliantAttribute;

		ListDictionary declarative_security;
		bool has_extension_method;		
		public AssemblyName Name;
		MethodInfo add_type_forwarder;
		ListDictionary emitted_forwarders;

		// Module is here just because of error messages
		static string[] attribute_targets = new string [] { "assembly", "module" };

		public AssemblyClass (): base ()
		{
			wrap_non_exception_throws = true;
		}

		public bool HasExtensionMethods {
			set {
				has_extension_method = value;
			}
		}

		public bool IsClsCompliant {
			get {
				return is_cls_compliant;
			}
		}

		public bool WrapNonExceptionThrows {
			get {
				return wrap_non_exception_throws;
			}
		}

		public override AttributeTargets AttributeTargets {
			get {
				return AttributeTargets.Assembly;
			}
		}

		public override bool IsClsComplianceRequired ()
		{
			return is_cls_compliant;
		}

		public void Resolve ()
		{
			if (RootContext.Unsafe) {
				//
				// Emits [assembly: SecurityPermissionAttribute (SecurityAction.RequestMinimum, SkipVerification = true)]
				// when -unsafe option was specified
				//
				
				Location loc = Location.Null;

				MemberAccess system_security_permissions = new MemberAccess (new MemberAccess (
					new QualifiedAliasMember (QualifiedAliasMember.GlobalAlias, "System", loc), "Security", loc), "Permissions", loc);

				Arguments pos = new Arguments (1);
				pos.Add (new Argument (new MemberAccess (new MemberAccess (system_security_permissions, "SecurityAction", loc), "RequestMinimum")));

				Arguments named = new Arguments (1);
				named.Add (new NamedArgument (new LocatedToken (loc, "SkipVerification"), (new BoolLiteral (true, loc))));

				GlobalAttribute g = new GlobalAttribute (new NamespaceEntry (null, null, null), "assembly",
					new MemberAccess (system_security_permissions, "SecurityPermissionAttribute"),
					new Arguments[] { pos, named }, loc, false);
				g.AttachTo (this, this);

				if (g.Resolve () != null) {
					declarative_security = new ListDictionary ();
					g.ExtractSecurityPermissionSet (declarative_security);
				}
			}

			if (OptAttributes == null)
				return;

			// Ensure that we only have GlobalAttributes, since the Search isn't safe with other types.
			if (!OptAttributes.CheckTargets())
				return;

			ClsCompliantAttribute = ResolveAttribute (PredefinedAttributes.Get.CLSCompliant);

			if (ClsCompliantAttribute != null) {
				is_cls_compliant = ClsCompliantAttribute.GetClsCompliantAttributeValue ();
			}

			Attribute a = ResolveAttribute (PredefinedAttributes.Get.RuntimeCompatibility);
			if (a != null) {
				object val = a.GetPropertyValue ("WrapNonExceptionThrows");
				if (val != null)
					wrap_non_exception_throws = (bool) val;
			}
		}

		// fix bug #56621
		private void SetPublicKey (AssemblyName an, byte[] strongNameBlob) 
		{
			try {
				// check for possible ECMA key
				if (strongNameBlob.Length == 16) {
					// will be rejected if not "the" ECMA key
					an.SetPublicKey (strongNameBlob);
				}
				else {
					// take it, with or without, a private key
					RSA rsa = CryptoConvert.FromCapiKeyBlob (strongNameBlob);
					// and make sure we only feed the public part to Sys.Ref
					byte[] publickey = CryptoConvert.ToCapiPublicKeyBlob (rsa);
					
					// AssemblyName.SetPublicKey requires an additional header
					byte[] publicKeyHeader = new byte [12] { 0x00, 0x24, 0x00, 0x00, 0x04, 0x80, 0x00, 0x00, 0x94, 0x00, 0x00, 0x00 };

					byte[] encodedPublicKey = new byte [12 + publickey.Length];
					Buffer.BlockCopy (publicKeyHeader, 0, encodedPublicKey, 0, 12);
					Buffer.BlockCopy (publickey, 0, encodedPublicKey, 12, publickey.Length);
					an.SetPublicKey (encodedPublicKey);
				}
			}
			catch (Exception) {
				Error_AssemblySigning ("The specified file `" + RootContext.StrongNameKeyFile + "' is incorrectly encoded");
				Environment.Exit (1);
			}
		}

		// TODO: rewrite this code (to kill N bugs and make it faster) and use standard ApplyAttribute way.
		public AssemblyName GetAssemblyName (string name, string output) 
		{
			if (OptAttributes != null) {
				foreach (Attribute a in OptAttributes.Attrs) {
					// cannot rely on any resolve-based members before you call Resolve
					if (a.ExplicitTarget == null || a.ExplicitTarget != "assembly")
						continue;

					// TODO: This code is buggy: comparing Attribute name without resolving is wrong.
					//       However, this is invoked by CodeGen.Init, when none of the namespaces
					//       are loaded yet.
					// TODO: Does not handle quoted attributes properly
					switch (a.Name) {
					case "AssemblyKeyFile":
					case "AssemblyKeyFileAttribute":
					case "System.Reflection.AssemblyKeyFileAttribute":
						if (RootContext.StrongNameKeyFile != null) {
							Report.SymbolRelatedToPreviousError (a.Location, a.GetSignatureForError ());
							Report.Warning (1616, 1, "Option `{0}' overrides attribute `{1}' given in a source file or added module",
									"keyfile", "System.Reflection.AssemblyKeyFileAttribute");
						} else {
							string value = a.GetString ();
							if (value != null && value.Length != 0)
								RootContext.StrongNameKeyFile = value;
						}
						break;
					case "AssemblyKeyName":
					case "AssemblyKeyNameAttribute":
					case "System.Reflection.AssemblyKeyNameAttribute":
						if (RootContext.StrongNameKeyContainer != null) {
							Report.SymbolRelatedToPreviousError (a.Location, a.GetSignatureForError ());
							Report.Warning (1616, 1, "Option `{0}' overrides attribute `{1}' given in a source file or added module",
									"keycontainer", "System.Reflection.AssemblyKeyNameAttribute");
						} else {
							string value = a.GetString ();
							if (value != null && value.Length != 0)
								RootContext.StrongNameKeyContainer = value;
						}
						break;
					case "AssemblyDelaySign":
					case "AssemblyDelaySignAttribute":
					case "System.Reflection.AssemblyDelaySignAttribute":
						RootContext.StrongNameDelaySign = a.GetBoolean ();
						break;
					}
				}
			}
			
			AssemblyName an = new AssemblyName ();
			an.Name = Path.GetFileNameWithoutExtension (name);

			// note: delay doesn't apply when using a key container
			if (RootContext.StrongNameKeyContainer != null) {
				an.KeyPair = new StrongNameKeyPair (RootContext.StrongNameKeyContainer);
				return an;
			}

			// strongname is optional
			if (RootContext.StrongNameKeyFile == null)
				return an;

			string AssemblyDir = Path.GetDirectoryName (output);

			// the StrongName key file may be relative to (a) the compiled
			// file or (b) to the output assembly. See bugzilla #55320
			// http://bugzilla.ximian.com/show_bug.cgi?id=55320

			// (a) relative to the compiled file
			string filename = Path.GetFullPath (RootContext.StrongNameKeyFile);
			bool exist = File.Exists (filename);
			if ((!exist) && (AssemblyDir != null) && (AssemblyDir != String.Empty)) {
				// (b) relative to the outputed assembly
				filename = Path.GetFullPath (Path.Combine (AssemblyDir, RootContext.StrongNameKeyFile));
				exist = File.Exists (filename);
			}

			if (exist) {
				using (FileStream fs = new FileStream (filename, FileMode.Open, FileAccess.Read)) {
					byte[] snkeypair = new byte [fs.Length];
					fs.Read (snkeypair, 0, snkeypair.Length);

					if (RootContext.StrongNameDelaySign) {
						// delayed signing - DO NOT include private key
						SetPublicKey (an, snkeypair);
					}
					else {
						// no delay so we make sure we have the private key
						try {
							CryptoConvert.FromCapiPrivateKeyBlob (snkeypair);
							an.KeyPair = new StrongNameKeyPair (snkeypair);
						}
						catch (CryptographicException) {
							if (snkeypair.Length == 16) {
								// error # is different for ECMA key
								Report.Error (1606, "Could not sign the assembly. " + 
									"ECMA key can only be used to delay-sign assemblies");
							}
							else {
								Error_AssemblySigning ("The specified file `" + RootContext.StrongNameKeyFile + "' does not have a private key");
							}
							return null;
						}
					}
				}
			}
			else {
				Error_AssemblySigning ("The specified file `" + RootContext.StrongNameKeyFile + "' does not exist");
				return null;
			}
			return an;
		}

		void Error_AssemblySigning (string text)
		{
			Report.Error (1548, "Error during assembly signing. " + text);
		}

		bool CheckInternalsVisibleAttribute (Attribute a)
		{
			string assembly_name = a.GetString ();
			if (assembly_name.Length == 0)
				return false;
				
			AssemblyName aname = null;
			try {
#if GMCS_SOURCE
				aname = new AssemblyName (assembly_name);
#else
				throw new NotSupportedException ();
#endif
			} catch (FileLoadException) {
			} catch (ArgumentException) {
			}
				
			// Bad assembly name format
			if (aname == null)
				Report.Warning (1700, 3, a.Location, "Assembly reference `" + assembly_name + "' is invalid and cannot be resolved");
			// Report error if we have defined Version or Culture
			else if (aname.Version != null || aname.CultureInfo != null)
				throw new Exception ("Friend assembly `" + a.GetString () + 
						"' is invalid. InternalsVisibleTo cannot have version or culture specified.");
			else if (aname.GetPublicKey () == null && Name.GetPublicKey () != null && Name.GetPublicKey ().Length != 0) {
				Report.Error (1726, a.Location, "Friend assembly reference `" + aname.FullName + "' is invalid." +
						" Strong named assemblies must specify a public key in their InternalsVisibleTo declarations");
				return false;
			}

			return true;
		}

		static bool IsValidAssemblyVersion (string version)
		{
			Version v;
			try {
				v = new Version (version);
			} catch {
				try {
					int major = int.Parse (version, CultureInfo.InvariantCulture);
					v = new Version (major, 0);
				} catch {
					return false;
				}
			}

			foreach (int candidate in new int [] { v.Major, v.Minor, v.Build, v.Revision }) {
				if (candidate > ushort.MaxValue)
					return false;
			}

			return true;
		}

		public override void ApplyAttributeBuilder (Attribute a, CustomAttributeBuilder cb, PredefinedAttributes pa)
		{
			if (a.IsValidSecurityAttribute ()) {
				if (declarative_security == null)
					declarative_security = new ListDictionary ();

				a.ExtractSecurityPermissionSet (declarative_security);
				return;
			}

			if (a.Type == pa.AssemblyCulture) {
				string value = a.GetString ();
				if (value == null || value.Length == 0)
					return;

				if (RootContext.Target == Target.Exe) {
					a.Error_AttributeEmitError ("The executables cannot be satelite assemblies, remove the attribute or keep it empty");
					return;
				}
			}

			if (a.Type == pa.AssemblyVersion) {
				string value = a.GetString ();
				if (value == null || value.Length == 0)
					return;

				value = value.Replace ('*', '0');

				if (!IsValidAssemblyVersion (value)) {
					a.Error_AttributeEmitError (string.Format ("Specified version `{0}' is not valid", value));
					return;
				}
			}

			if (a.Type == pa.InternalsVisibleTo && !CheckInternalsVisibleAttribute (a))
				return;

			if (a.Type == pa.TypeForwarder) {
				Type t = a.GetArgumentType ();
				if (t == null || TypeManager.HasElementType (t)) {
					Report.Error (735, a.Location, "Invalid type specified as an argument for TypeForwardedTo attribute");
					return;
				}

				t = TypeManager.DropGenericTypeArguments (t);
				if (emitted_forwarders == null) {
					emitted_forwarders = new ListDictionary();
				} else if (emitted_forwarders.Contains(t)) {
					Report.SymbolRelatedToPreviousError(((Attribute)emitted_forwarders[t]).Location, null);
					Report.Error(739, a.Location, "A duplicate type forward of type `{0}'",
						TypeManager.CSharpName(t));
					return;
				}

				emitted_forwarders.Add(t, a);

				if (TypeManager.LookupDeclSpace (t) != null) {
					Report.SymbolRelatedToPreviousError (t);
					Report.Error (729, a.Location, "Cannot forward type `{0}' because it is defined in this assembly",
						TypeManager.CSharpName (t));
					return;
				}

				if (t.DeclaringType != null) {
					Report.Error (730, a.Location, "Cannot forward type `{0}' because it is a nested type",
						TypeManager.CSharpName (t));
					return;
				}

				if (add_type_forwarder == null) {
					add_type_forwarder = typeof (AssemblyBuilder).GetMethod ("AddTypeForwarder",
						BindingFlags.NonPublic | BindingFlags.Instance);

					if (add_type_forwarder == null) {
						Report.RuntimeMissingSupport (a.Location, "TypeForwardedTo attribute");
						return;
					}
				}

				add_type_forwarder.Invoke (Builder, new object[] { t });
				return;
			}
			
			if (a.Type == pa.Extension) {
				a.Error_MisusedExtensionAttribute ();
				return;
			}

			Builder.SetCustomAttribute (cb);
		}

		public override void Emit (TypeContainer tc)
		{
			base.Emit (tc);

			if (has_extension_method)
				PredefinedAttributes.Get.Extension.EmitAttribute (Builder);

			// FIXME: Does this belong inside SRE.AssemblyBuilder instead?
			PredefinedAttribute pa = PredefinedAttributes.Get.RuntimeCompatibility;
			if (pa.IsDefined && (OptAttributes == null || !OptAttributes.Contains (pa))) {
				ConstructorInfo ci = TypeManager.GetPredefinedConstructor (
					pa.Type, Location.Null, Type.EmptyTypes);
				PropertyInfo [] pis = new PropertyInfo [1];
				pis [0] = TypeManager.GetPredefinedProperty (pa.Type,
					"WrapNonExceptionThrows", Location.Null, TypeManager.bool_type);
				object [] pargs = new object [1];
				pargs [0] = true;
				Builder.SetCustomAttribute (new CustomAttributeBuilder (ci, new object [0], pis, pargs));
			}

			if (declarative_security != null) {

				MethodInfo add_permission = typeof (AssemblyBuilder).GetMethod ("AddPermissionRequests", BindingFlags.Instance | BindingFlags.NonPublic);
				object builder_instance = Builder;

				try {
					// Microsoft runtime hacking
					if (add_permission == null) {
						Type assembly_builder = typeof (AssemblyBuilder).Assembly.GetType ("System.Reflection.Emit.AssemblyBuilderData");
						add_permission = assembly_builder.GetMethod ("AddPermissionRequests", BindingFlags.Instance | BindingFlags.NonPublic);

						FieldInfo fi = typeof (AssemblyBuilder).GetField ("m_assemblyData", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
						builder_instance = fi.GetValue (Builder);
					}

					object[] args = new object [] { declarative_security [SecurityAction.RequestMinimum],
												  declarative_security [SecurityAction.RequestOptional],
												  declarative_security [SecurityAction.RequestRefuse] };
					add_permission.Invoke (builder_instance, args);
				}
				catch {
					Report.RuntimeMissingSupport (Location.Null, "assembly permission setting");
				}
			}
		}

		public override string[] ValidAttributeTargets {
			get {
				return attribute_targets;
			}
		}

		// Wrapper for AssemblyBuilder.AddModule
		static MethodInfo adder_method;
		static public MethodInfo AddModule_Method {
			get {
				if (adder_method == null)
					adder_method = typeof (AssemblyBuilder).GetMethod ("AddModule", BindingFlags.Instance|BindingFlags.NonPublic);
				return adder_method;
			}
		}
		public Module AddModule (string module)
		{
			MethodInfo m = AddModule_Method;
			if (m == null) {
				Report.RuntimeMissingSupport (Location.Null, "/addmodule");
				Environment.Exit (1);
			}

			try {
				return (Module) m.Invoke (Builder, new object [] { module });
			} catch (TargetInvocationException ex) {
				throw ex.InnerException;
			}
		}		
	}
}
