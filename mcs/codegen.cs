//
// codegen.cs: The code generator
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;

using Mono.Security.Cryptography;

namespace Mono.CSharp {

	/// <summary>
	///    Code generator class.
	/// </summary>
	public class CodeGen {
		static AppDomain current_domain;
		static public SymbolWriter SymbolWriter;

		public static AssemblyClass Assembly;
		public static ModuleClass Module;

		static CodeGen ()
		{
			Assembly = new AssemblyClass ();
			Module = new ModuleClass (RootContext.Unsafe);
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

		static string TrimExt (string name)
		{
			int pos = name.LastIndexOf ('.');

			return name.Substring (0, pos);
		}

		static public string FileName;

		//
		// Initializes the symbol writer
		//
		static void InitializeSymbolWriter ()
		{
			SymbolWriter = SymbolWriter.GetSymbolWriter (Module.Builder);

			//
			// If we got an ISymbolWriter instance, initialize it.
			//
			if (SymbolWriter == null) {
				Report.Warning (
					-18, "Could not find the symbol writer assembly (Mono.CSharp.Debugger.dll). This is normally an installation problem. Please make sure to compile and install the mcs/class/Mono.CSharp.Debugger directory.");
				return;
			}
		}

		//
		// Initializes the code generator variables
		//
		static public void Init (string name, string output, bool want_debugging_support)
		{
			FileName = output;
			AssemblyName an = Assembly.GetAssemblyName (name, output);
			
			current_domain = AppDomain.CurrentDomain;

			try {
				Assembly.Builder = current_domain.DefineDynamicAssembly (an,
					AssemblyBuilderAccess.Save, Dirname (name));
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
				throw;
			}

			//
			// Pass a path-less name to DefineDynamicModule.  Wonder how
			// this copes with output in different directories then.
			// FIXME: figure out how this copes with --output /tmp/blah
			//
			// If the third argument is true, the ModuleBuilder will dynamically
			// load the default symbol writer.
			//
			Module.Builder = Assembly.Builder.DefineDynamicModule (
				Basename (name), Basename (output), want_debugging_support);

			if (want_debugging_support)
				InitializeSymbolWriter ();
		}

		static public void Save (string name)
		{
			try {
				Assembly.Builder.Save (Basename (name));
			} catch (System.IO.IOException io){
				Report.Error (16, "Could not write to file `"+name+"', cause: " + io.Message);
			}
		}
	}

	//
	// Provides "local" store across code that can yield: locals
	// or fields, notice that this should not be used by anonymous
	// methods to create local storage, those only require
	// variable mapping.
	//
	public class VariableStorage {
		ILGenerator ig;
		FieldBuilder fb;
		LocalBuilder local;
		
		static int count;
		
		public VariableStorage (EmitContext ec, Type t)
		{
			count++;
			if (ec.InIterator)
				fb = IteratorHandler.Current.MapVariable ("s_", count.ToString (), t);
			else
				local = ec.ig.DeclareLocal (t);
			ig = ec.ig;
		}

		public void EmitThis ()
		{
			if (fb != null)
				ig.Emit (OpCodes.Ldarg_0);
		}

		public void EmitStore ()
		{
			if (fb == null)
				ig.Emit (OpCodes.Stloc, local);
			else
				ig.Emit (OpCodes.Stfld, fb);
		}

		public void EmitLoad ()
		{
			if (fb == null)
				ig.Emit (OpCodes.Ldloc, local);
			else 
				ig.Emit (OpCodes.Ldfld, fb);
		}
		
		public void EmitCall (MethodInfo mi)
		{
			// FIXME : we should handle a call like tostring
			// here, where boxing is needed. However, we will
			// never encounter that with the current usage.
			
			bool value_type_call;
			EmitThis ();
			if (fb == null) {
				value_type_call = local.LocalType.IsValueType;
				
				if (value_type_call)
					ig.Emit (OpCodes.Ldloca, local);
				else
					ig.Emit (OpCodes.Ldloc, local);
			} else {
				value_type_call = fb.FieldType.IsValueType;
				
				if (value_type_call)
					ig.Emit (OpCodes.Ldflda, fb);
				else
					ig.Emit (OpCodes.Ldfld, fb);
			}
			
			ig.Emit (value_type_call ? OpCodes.Call : OpCodes.Callvirt, mi);
		}
	}
	
	/// <summary>
	///   An Emit Context is created for each body of code (from methods,
	///   properties bodies, indexer bodies or constructor bodies)
	/// </summary>
	public class EmitContext {
		public DeclSpace DeclSpace;
		public DeclSpace TypeContainer;
		public ILGenerator   ig;

		/// <summary>
		///   This variable tracks the `checked' state of the compilation,
		///   it controls whether we should generate code that does overflow
		///   checking, or if we generate code that ignores overflows.
		///
		///   The default setting comes from the command line option to generate
		///   checked or unchecked code plus any source code changes using the
		///   checked/unchecked statements or expressions.   Contrast this with
		///   the ConstantCheckState flag.
		/// </summary>
		
		public bool CheckState;

		/// <summary>
		///   The constant check state is always set to `true' and cant be changed
		///   from the command line.  The source code can change this setting with
		///   the `checked' and `unchecked' statements and expressions. 
		/// </summary>
		public bool ConstantCheckState;

		/// <summary>
		///   Whether we are emitting code inside a static or instance method
		/// </summary>
		public bool IsStatic;

		/// <summary>
		///   Whether we are emitting a field initializer
		/// </summary>
		public bool IsFieldInitializer;

		/// <summary>
		///   The value that is allowed to be returned or NULL if there is no
		///   return type.
		/// </summary>
		public Type ReturnType;

		/// <summary>
		///   Points to the Type (extracted from the TypeContainer) that
		///   declares this body of code
		/// </summary>
		public Type ContainerType;
		
		/// <summary>
		///   Whether this is generating code for a constructor
		/// </summary>
		public bool IsConstructor;

		/// <summary>
		///   Whether we're control flow analysis enabled
		/// </summary>
		public bool DoFlowAnalysis;
		
		/// <summary>
		///   Keeps track of the Type to LocalBuilder temporary storage created
		///   to store structures (used to compute the address of the structure
		///   value on structure method invocations)
		/// </summary>
		public Hashtable temporary_storage;

		public Block CurrentBlock;

		public int CurrentFile;

		/// <summary>
		///   The location where we store the return value.
		/// </summary>
		LocalBuilder return_value;

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
		///   Whether we are inside an iterator block.
		/// </summary>
		public bool InIterator;

		public bool IsLastStatement;

		/// <summary>
		///   Whether remapping of locals, parameters and fields is turned on.
		///   Used by iterators and anonymous methods.
		/// </summary>
		public bool RemapToProxy;

		/// <summary>
		///  Whether we are inside an unsafe block
		/// </summary>
		public bool InUnsafe;

		/// <summary>
		///  Whether we are in a `fixed' initialization
		/// </summary>
		public bool InFixedInitializer;

		/// <summary>
		///  Whether we are inside an anonymous method.
		/// </summary>
		public bool InAnonymousMethod;
		
		/// <summary>
		///   Location for this EmitContext
		/// </summary>
		public Location loc;

		/// <summary>
		///   Used to flag that it is ok to define types recursively, as the
		///   expressions are being evaluated as part of the type lookup
		///   during the type resolution process
		/// </summary>
		public bool ResolvingTypeTree;
		
		/// <summary>
		///   Inside an enum definition, we do not resolve enumeration values
		///   to their enumerations, but rather to the underlying type/value
		///   This is so EnumVal + EnumValB can be evaluated.
		///
		///   There is no "E operator + (E x, E y)", so during an enum evaluation
		///   we relax the rules
		/// </summary>
		public bool InEnumContext;

		FlowBranching current_flow_branching;
		
		public EmitContext (DeclSpace parent, DeclSpace ds, Location l, ILGenerator ig,
				    Type return_type, int code_flags, bool is_constructor)
		{
			this.ig = ig;

			TypeContainer = parent;
			DeclSpace = ds;
			CheckState = RootContext.Checked;
			ConstantCheckState = true;
			
			IsStatic = (code_flags & Modifiers.STATIC) != 0;
			InIterator = (code_flags & Modifiers.METHOD_YIELDS) != 0;
			RemapToProxy = InIterator;
			ReturnType = return_type;
			IsConstructor = is_constructor;
			CurrentBlock = null;
			CurrentFile = 0;
			
			if (parent != null){
				// Can only be null for the ResolveType contexts.
				ContainerType = parent.TypeBuilder;
				if (parent.UnsafeContext)
					InUnsafe = true;
				else
					InUnsafe = (code_flags & Modifiers.UNSAFE) != 0;
			}
			loc = l;
			
			if (ReturnType == TypeManager.void_type)
				ReturnType = null;
		}

		public EmitContext (TypeContainer tc, Location l, ILGenerator ig,
				    Type return_type, int code_flags, bool is_constructor)
			: this (tc, tc, l, ig, return_type, code_flags, is_constructor)
		{
		}

		public EmitContext (TypeContainer tc, Location l, ILGenerator ig,
				    Type return_type, int code_flags)
			: this (tc, tc, l, ig, return_type, code_flags, false)
		{
		}

		public FlowBranching CurrentBranching {
			get {
				return current_flow_branching;
			}
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
			FlowBranching.BranchingType type;

			if (CurrentBranching.Type == FlowBranching.BranchingType.Switch)
				type = FlowBranching.BranchingType.SwitchSection;
			else
				type = FlowBranching.BranchingType.Block;

			current_flow_branching = FlowBranching.CreateBranching (CurrentBranching, type, block, block.StartLocation);
			return current_flow_branching;
		}

		// <summary>
		//   Ends a code branching.  Merges the state of locals and parameters
		//   from all the children of the ending branching.
		// </summary>
		public FlowBranching.UsageVector DoEndFlowBranching ()
		{
			FlowBranching old = current_flow_branching;
			current_flow_branching = current_flow_branching.Parent;

			return current_flow_branching.MergeChild (old);
		}

		// <summary>
		//   Ends a code branching.  Merges the state of locals and parameters
		//   from all the children of the ending branching.
		// </summary>
		public FlowBranching.Reachability EndFlowBranching ()
		{
			FlowBranching.UsageVector vector = DoEndFlowBranching ();

			return vector.Reachability;
		}

		// <summary>
		//   Kills the current code branching.  This throws away any changed state
		//   information and should only be used in case of an error.
		// </summary>
		public void KillFlowBranching ()
		{
			current_flow_branching = current_flow_branching.Parent;
		}

		public void EmitTopBlock (Block block, InternalParameters ip, Location loc)
		{
			bool unreachable = false;

			if (!Location.IsNull (loc))
				CurrentFile = loc.File;

			if (block != null){
			    try {
				int errors = Report.Errors;

				block.EmitMeta (this, ip);

				if (Report.Errors == errors){
					bool old_do_flow_analysis = DoFlowAnalysis;
					DoFlowAnalysis = true;

					current_flow_branching = FlowBranching.CreateBranching (
						null, FlowBranching.BranchingType.Block, block, loc);

					if (!block.Resolve (this)) {
						current_flow_branching = null;
						DoFlowAnalysis = old_do_flow_analysis;
						return;
					}

					FlowBranching.Reachability reachability = current_flow_branching.MergeTopBlock ();
					current_flow_branching = null;
					
					DoFlowAnalysis = old_do_flow_analysis;

					block.Emit (this);

					if (reachability.AlwaysReturns ||
					    reachability.AlwaysThrows ||
					    reachability.IsUnreachable)
						unreachable = true;
				}
			    } catch (Exception e) {
					Console.WriteLine ("Exception caught by the compiler while compiling:");
					Console.WriteLine ("   Block that caused the problem begin at: " + loc);
					
					if (CurrentBlock != null){
						Console.WriteLine ("                     Block being compiled: [{0},{1}]",
								   CurrentBlock.StartLocation, CurrentBlock.EndLocation);
					}
					Console.WriteLine (e.GetType ().FullName + ": " + e.Message);
					Console.WriteLine (Report.FriendlyStackTrace (e));
					
					Environment.Exit (1);
			    }
			}

			if (ReturnType != null && !unreachable){
				if (!InIterator){
					Report.Error (161, loc, "Not all code paths return a value");
					return;
				}
			}

			if (HasReturnLabel)
				ig.MarkLabel (ReturnLabel);
			if (return_value != null){
				ig.Emit (OpCodes.Ldloc, return_value);
				ig.Emit (OpCodes.Ret);
			} else {
				//
				// If `HasReturnLabel' is set, then we already emitted a
				// jump to the end of the method, so we must emit a `ret'
				// there.
				//
				// Unfortunately, System.Reflection.Emit automatically emits
				// a leave to the end of a finally block.  This is a problem
				// if no code is following the try/finally block since we may
				// jump to a point after the end of the method.
				// As a workaround, we're always creating a return label in
				// this case.
				//

				if ((block != null) && block.IsDestructor) {
					// Nothing to do; S.R.E automatically emits a leave.
				} else if (HasReturnLabel || (!unreachable && !InIterator)) {
					if (ReturnType != null)
						ig.Emit (OpCodes.Ldloc, TemporaryReturn ());
					ig.Emit (OpCodes.Ret);
				}
			}
		}

		/// <summary>
		///   This is called immediately before emitting an IL opcode to tell the symbol
		///   writer to which source line this opcode belongs.
		/// </summary>
		public void Mark (Location loc, bool check_file)
		{
			if ((CodeGen.SymbolWriter == null) || Location.IsNull (loc))
				return;

			if (check_file && (CurrentFile != loc.File))
				return;

			ig.MarkSequencePoint (null, loc.Row, 0, 0, 0);
		}

		/// <summary>
		///   Returns a temporary storage for a variable of type t as 
		///   a local variable in the current body.
		/// </summary>
		public LocalBuilder GetTemporaryLocal (Type t)
		{
			LocalBuilder location = null;
			
			if (temporary_storage != null){
				object o = temporary_storage [t];
				if (o != null){
					if (o is ArrayList){
						ArrayList al = (ArrayList) o;
						
						for (int i = 0; i < al.Count; i++){
							if (al [i] != null){
								location = (LocalBuilder) al [i];
								al [i] = null;
								break;
							}
						}
					} else
						location = (LocalBuilder) o;
					if (location != null)
						return location;
				}
			}
			
			return ig.DeclareLocal (t);
		}

		public void FreeTemporaryLocal (LocalBuilder b, Type t)
		{
			if (temporary_storage == null){
				temporary_storage = new Hashtable ();
				temporary_storage [t] = b;
				return;
			}
			object o = temporary_storage [t];
			if (o == null){
				temporary_storage [t] = b;
				return;
			}
			if (o is ArrayList){
				ArrayList al = (ArrayList) o;
				for (int i = 0; i < al.Count; i++){
					if (al [i] == null){
						al [i] = b;
						return;
					}
				}
				al.Add (b);
				return;
			}
			ArrayList replacement = new ArrayList ();
			replacement.Add (o);
			temporary_storage.Remove (t);
			temporary_storage [t] = replacement;
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
		/// </summary>
		public LocalBuilder TemporaryReturn ()
		{
			if (return_value == null){
				return_value = ig.DeclareLocal (ReturnType);
				ReturnLabel = ig.DefineLabel ();
				HasReturnLabel = true;
			}

			return return_value;
		}

		public void NeedReturnLabel ()
		{
			if (!HasReturnLabel) {
				ReturnLabel = ig.DefineLabel ();
				HasReturnLabel = true;
			}
		}

		//
		// Creates a field `name' with the type `t' on the proxy class
		//
		public FieldBuilder MapVariable (string name, Type t)
		{
			if (InIterator){
				return IteratorHandler.Current.MapVariable ("v_", name, t);
			}

			throw new Exception ("MapVariable for an unknown state");
		}

		//
		// Invoke this routine to remap a VariableInfo into the
		// proper MemberAccess expression
		//
		public Expression RemapLocal (LocalInfo local_info)
		{
			FieldExpr fe = new FieldExpr (local_info.FieldBuilder, loc);
			fe.InstanceExpression = new ProxyInstance ();
			return fe.DoResolve (this);
		}

		public Expression RemapLocalLValue (LocalInfo local_info, Expression right_side)
		{
			FieldExpr fe = new FieldExpr (local_info.FieldBuilder, loc);
			fe.InstanceExpression = new ProxyInstance ();
			return fe.DoResolveLValue (this, right_side);
		}

		public Expression RemapParameter (int idx)
		{
			FieldExpr fe = new FieldExprNoAddress (IteratorHandler.Current.parameter_fields [idx], loc);
			fe.InstanceExpression = new ProxyInstance ();
			return fe.DoResolve (this);
		}

		public Expression RemapParameterLValue (int idx, Expression right_side)
		{
			FieldExpr fe = new FieldExprNoAddress (IteratorHandler.Current.parameter_fields [idx], loc);
			fe.InstanceExpression = new ProxyInstance ();
			return fe.DoResolveLValue (this, right_side);
		}
		
		//
		// Emits the proper object to address fields on a remapped
		// variable/parameter to field in anonymous-method/iterator proxy classes.
		//
		public void EmitThis ()
		{
			ig.Emit (OpCodes.Ldarg_0);

			if (!IsStatic){
				if (InIterator)
					ig.Emit (OpCodes.Ldfld, IteratorHandler.Current.this_field);
				else
					throw new Exception ("EmitThis for an unknown state");
			}
		}

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
	}


	public abstract class CommonAssemblyModulClass: IAttributeSupport {
		protected Hashtable m_attributes;

		protected CommonAssemblyModulClass () 
		{
			m_attributes = new Hashtable ();
		}

		//
		// Adds a global attribute that was declared in `container', 
		// the attribute is in `attr', and it was defined at `loc'
		//                
		public void AddAttribute (TypeContainer container, AttributeSection attr)
		{
			NamespaceEntry ns = container.NamespaceEntry;
			Attributes a = (Attributes) m_attributes [ns];

			if (a == null) {
				m_attributes [ns] = new Attributes (attr);
				return;
			}

			a.AddAttributeSection (attr);
		}

		public virtual void Emit () 
		{
			if (m_attributes.Count < 1)
				return;

			TypeContainer dummy = new TypeContainer ();
			EmitContext temp_ec = new EmitContext (dummy, Mono.CSharp.Location.Null, null, null, 0, false);
			
			foreach (DictionaryEntry de in m_attributes)
			{
				NamespaceEntry ns = (NamespaceEntry) de.Key;
				Attributes attrs = (Attributes) de.Value;
				
				dummy.NamespaceEntry = ns;
				Attribute.ApplyAttributes (temp_ec, null, this, attrs);
			}
		}
                
		protected Attribute GetClsCompliantAttribute ()
		{
			if (m_attributes.Count < 1)
				return null;

			EmitContext temp_ec = new EmitContext (new TypeContainer (), Mono.CSharp.Location.Null, null, null, 0, false);
			
			foreach (DictionaryEntry de in m_attributes) {

				NamespaceEntry ns = (NamespaceEntry) de.Key;
				Attributes attrs = (Attributes) de.Value;
				temp_ec.TypeContainer.NamespaceEntry = ns;

				foreach (AttributeSection attr_section in attrs.AttributeSections) {
					foreach (Attribute a in attr_section.Attributes) {
						Type attributeType = RootContext.LookupType (temp_ec.DeclSpace, Attributes.GetAttributeFullName (a.Name), true, Location.Null);
						if (attributeType == TypeManager.cls_compliant_attribute_type) {
							a.Resolve (temp_ec);
							return a;
						}
					}
				}
			}
			return null;
		}
                
		#region IAttributeSupport Members
		public abstract void SetCustomAttribute(CustomAttributeBuilder customBuilder);
		#endregion

	}
	

	public class AssemblyClass: CommonAssemblyModulClass {
		// TODO: make it private and move all builder based methods here
		public AssemblyBuilder Builder;
                    
		bool is_cls_compliant;

		public AssemblyClass (): base ()
		{
			is_cls_compliant = false;
		}

		public bool IsClsCompliant {
			get {
				return is_cls_compliant;
			}
		}

		public void ResolveClsCompliance ()
		{
			Attribute a = GetClsCompliantAttribute ();
			if (a == null)
				return;

			is_cls_compliant = a.GetClsCompliantAttributeValue (null);
		}

		public AssemblyName GetAssemblyName (string name, string output) 
		{
			// scan assembly attributes for strongname related attr
			foreach (DictionaryEntry nsattr in m_attributes) {
				ArrayList list = ((Attributes)nsattr.Value).AttributeSections;
				for (int i=0; i < list.Count; i++) {
					AttributeSection asect = (AttributeSection) list [i];
					if (asect.Target != "assembly")
						continue;
					// strongname attributes don't support AllowMultiple
					Attribute a = (Attribute) asect.Attributes [0];
					switch (a.Name) {
						case "AssemblyKeyFile":
							if (RootContext.StrongNameKeyFile != null) {
								Report.Warning (1616, "Compiler option -keyfile overrides " +
									"AssemblyKeyFileAttribute");
							}
							else {
								string value = a.GetString ();
								if (value != String.Empty)
									RootContext.StrongNameKeyFile = value;
							}
							break;
						case "AssemblyKeyName":
							if (RootContext.StrongNameKeyContainer != null) {
								Report.Warning (1616, "Compiler option -keycontainer overrides " +
									"AssemblyKeyNameAttribute");
							}
							else {
								string value = a.GetString ();
								if (value != String.Empty)
									RootContext.StrongNameKeyContainer = value;
							}
							break;
						case "AssemblyDelaySign":
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
				using (FileStream fs = new FileStream (filename, FileMode.Open)) {
					byte[] snkeypair = new byte [fs.Length];
					fs.Read (snkeypair, 0, snkeypair.Length);

					try {
						// this will import public or private/public keys
						RSA rsa = CryptoConvert.FromCapiKeyBlob (snkeypair);
						// only the public part must be supplied if signature is delayed
						byte[] key = CryptoConvert.ToCapiKeyBlob (rsa, !RootContext.StrongNameDelaySign);
						an.KeyPair = new StrongNameKeyPair (key);
					}
					catch (CryptographicException) {
						Report.Error (1548, "Could not strongname the assembly. File `" +
							RootContext.StrongNameKeyFile + "' incorrectly encoded.");
						Environment.Exit (1);
					}
				}
			}
			else {
				Report.Error (1548, "Could not strongname the assembly. File `" +
					RootContext.StrongNameKeyFile + "' not found.");
				Environment.Exit (1);
			}
			return an;
		}

		public override void SetCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			Builder.SetCustomAttribute (customBuilder);
		}
	}

	public class ModuleClass: CommonAssemblyModulClass {
		// TODO: make it private and move all builder based methods here
		public ModuleBuilder Builder;
            
		bool m_module_is_unsafe;

		public ModuleClass (bool is_unsafe)
		{
			m_module_is_unsafe = is_unsafe;
		}

		public override void Emit () 
		{
			base.Emit ();

			Attribute a = GetClsCompliantAttribute ();
			if (a != null) {
				Report.Warning (3012, a.Location);
			}

			if (!m_module_is_unsafe)
				return;

			if (TypeManager.unverifiable_code_ctor == null) {
				Console.WriteLine ("Internal error ! Cannot set unverifiable code attribute.");
				return;
			}
				
			SetCustomAttribute (new CustomAttributeBuilder (TypeManager.unverifiable_code_ctor, new object [0]));
		}
                
		public override void SetCustomAttribute(CustomAttributeBuilder customBuilder)
		{
			Builder.SetCustomAttribute (customBuilder);
		}
	}

}
