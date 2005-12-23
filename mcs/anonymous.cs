//
// anonymous.cs: Support for anonymous methods
//
// Author:
//   Miguel de Icaza (miguel@ximain.com)
//
// (C) 2003, 2004 Novell, Inc.
//
// TODO: Ideally, we should have the helper classes emited as a hierarchy to map
// their nesting, and have the visibility set to private, instead of NestedAssembly
//
//
//

using System;
using System.Text;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp {

	public abstract class AnonymousContainer : Expression
	{
		// Used to generate unique method names.
		protected static int anonymous_method_count;
		    
		// An array list of AnonymousMethodParameter or null
		public Parameters Parameters;

		//
		// The block that makes up the body for the anonymous mehtod
		//
		public ToplevelBlock Block;

		//
		// The container block for this anonymous method.
		//
		public Block ContainingBlock;

		//
		// The implicit method we create
		//
		public Method method;

		protected MethodInfo invoke_mb;
		
		// The emit context for the anonymous method
		public EmitContext aec;
		public Parameters amp;
		protected bool unreachable;

		//
		// The modifiers applied to the method, we aggregate them
		//
		protected int method_modifiers = Modifiers.PRIVATE;
		
		//
		// During the resolve stage of the anonymous method body,
		// we discover the actual scope where we are hosted, or
		// null to host the method in the same class
		//
		public ScopeInfo Scope;

		//
		// Points to our container anonymous method if its present
		//
		public AnonymousContainer ContainerAnonymousMethod;	

		protected AnonymousContainer (Parameters parameters, ToplevelBlock container,
					      ToplevelBlock block, Location l)
		{
			Parameters = parameters;
			Block = block;
			loc = l;

			//
			// The order is important: this setups the CaptureContext tree hierarchy.
			//
			if (container == null) {
				Report.Error (1706, l, "Anonymous methods are not allowed in attribute declaration");
				return;
			}
			container.SetHaveAnonymousMethods (l, this);
			block.SetHaveAnonymousMethods (l, this);
		}

		protected AnonymousContainer (Parameters parameters, ToplevelBlock container,
					      Location l):
			this (parameters, container, new ToplevelBlock (container, parameters, l), l)
		{
		}

		public override Expression DoResolve (EmitContext ec)
		{
			//
			// Set class type, set type
			//

			eclass = ExprClass.Value;
			
			//
			// This hack means `The type is not accessible
			// anywhere', we depend on special conversion
			// rules.
			// 
			type = TypeManager.anonymous_method_type;

			return this;
		}

		protected abstract bool CreateMethodHost (EmitContext ec);

		public abstract void CreateScopeType (EmitContext ec, ScopeInfo scope);

		public abstract bool IsIterator {
			get;
		}
	}

	public class AnonymousMethod : AnonymousContainer
	{
		public AnonymousMethod (Parameters parameters, ToplevelBlock container,
					ToplevelBlock block, Location l)
			: base (parameters, container, block, l)
		{
		}

		public override bool IsIterator {
			get { return false; }
		}

		public override void Emit (EmitContext ec)
		{
			// nothing, as we only exist to not do anything.
		}

		//
		// Creates the host for the anonymous method
		//
		protected override bool CreateMethodHost (EmitContext ec)
		{
			//
			// Crude hack follows: we replace the TypeBuilder during the
			// definition to get the method hosted in the right class
			//
			
			TypeBuilder current_type = ec.TypeContainer.TypeBuilder;
			TypeBuilder type_host = (Scope == null ) // || Scope.ScopeTypeBuilder == null)
				? current_type : Scope.ScopeTypeBuilder;

			if (current_type == null)
				throw new Exception ("The current_type is null");
			
			if (type_host == null)
				throw new Exception (String.Format ("Type host is null, Scope is {0}", Scope == null ? "null" : "Not null"));

			if (current_type != type_host)
				method_modifiers = Modifiers.INTERNAL;

			if (current_type == type_host && ec.IsStatic){
				method_modifiers |= Modifiers.STATIC;
				current_type = null;
			} 

			method = new Method (
				(TypeContainer) ec.TypeContainer,
				new TypeExpression (invoke_mb.ReturnType, loc),
				method_modifiers, false, new MemberName ("<#AnonymousMethod>" + anonymous_method_count++, loc),
				Parameters, null);
			method.Block = Block;
			
			//
			// Swap the TypeBuilder while we define the method, then restore
			//
			if (current_type != null)
				ec.TypeContainer.TypeBuilder = type_host;
			bool res = method.Define ();
			if (current_type != null)
				ec.TypeContainer.TypeBuilder = current_type;
			return res;
		}
		
		void Error_ParameterMismatch (Type t)
		{
			Report.Error (1661, loc, "Anonymous method could not be converted to delegate `" +
				      "{0}' since there is a parameter mismatch", TypeManager.CSharpName (t));
		}

		//
		// Returns true if this anonymous method can be implicitly
		// converted to the delegate type `delegate_type'
		//
		public Expression Compatible (EmitContext ec, Type delegate_type, bool probe)
		{
			//
			// At this point its the first time we know the return type that is 
			// needed for the anonymous method.  We create the method here.
			//

			invoke_mb = (MethodInfo) Delegate.GetInvokeMethod (ec, delegate_type, loc);
			ParameterData invoke_pd = TypeManager.GetParameterData (invoke_mb);

			if (Parameters == null){				
				//
				// We provide a set of inaccessible parameters
				//
				Parameter [] fixedpars = new Parameter [invoke_pd.Count];
				
				for (int i = 0; i < invoke_pd.Count; i++){
					fixedpars [i] = new Parameter (
						invoke_pd.ParameterType (i),
						"+" + i, invoke_pd.ParameterModifier (i), null, loc);
				}
				
				Parameters = new Parameters (fixedpars);
			}
			
			//
			// First, parameter types of `delegate_type' must be compatible
			// with the anonymous method.
			//
			Parameters.Resolve (ec);
			amp = Parameters;
			
			if (amp.Count != invoke_pd.Count){
				if (!probe){
					Report.Error (1593, loc, "Delegate `{0}' does not take `{1}' arguments",
						TypeManager.CSharpName (delegate_type), amp.Count.ToString ());
					Error_ParameterMismatch (delegate_type);
				}
				return null;
			}
			
			for (int i = 0; i < amp.Count; i++){
				Parameter.Modifier amp_mod = amp.ParameterModifier (i);

				if (!probe) {
					if ((amp_mod & (Parameter.Modifier.OUT | Parameter.Modifier.REF)) != 0){
						Report.Error (1677, loc, "Parameter `{0}' should not be declared with the `{1}' keyword", 
							(i+1).ToString (), Parameter.GetModifierSignature (amp_mod));
						Error_ParameterMismatch (delegate_type);
						return null;
					}

					if (amp_mod != invoke_pd.ParameterModifier (i)){
						Report.Error (1676, loc, "Parameter `{0}' must be declared with the `{1}' keyword",
							(i+1).ToString (), Parameter.GetModifierSignature (invoke_pd.ParameterModifier (i)));
						Error_ParameterMismatch (delegate_type);
						return null;
					}
				
					if (amp.ParameterType (i) != invoke_pd.ParameterType (i)){
						Report.Error (1678, loc, "Parameter `{0}' is declared as type `{1}' but should be `{2}'",
							(i+1).ToString (),
							TypeManager.CSharpName (amp.ParameterType (i)),
							TypeManager.CSharpName (invoke_pd.ParameterType (i)));
						Error_ParameterMismatch (delegate_type);
						return null;
					}
				}
			}

			//
			// If we are only probing, return ourselves
			//
			if (probe)
				return this;
			
			//
			// Second: the return type of the delegate must be compatible with 
			// the anonymous type.   Instead of doing a pass to examine the block
			// we satisfy the rule by setting the return type on the EmitContext
			// to be the delegate type return type.
			//

			//MethodBuilder builder = method_data.MethodBuilder;
			//ILGenerator ig = builder.GetILGenerator ();

			
			aec = new EmitContext (
				ec.TypeContainer, ec.DeclSpace, loc, null,
				invoke_mb.ReturnType,
				/* REVIEW */ (ec.InIterator ? Modifiers.METHOD_YIELDS : 0) |
				(ec.InUnsafe ? Modifiers.UNSAFE : 0) |
				(ec.IsStatic ? Modifiers.STATIC : 0),
				/* No constructor */ false);

			aec.CurrentAnonymousMethod = this;
			ContainerAnonymousMethod = ec.CurrentAnonymousMethod;
			ContainingBlock = ec.CurrentBlock;

			if (aec.ResolveTopBlock (ec, Block, amp, null, out unreachable))
				return new AnonymousDelegate (this, delegate_type, loc).Resolve (ec);

			return null;
		}

		public override string ExprClassName {
			get {
				return "anonymous method";
			}
		}

		public MethodBuilder GetMethodBuilder ()
		{
			return method.MethodData.MethodBuilder;
		}

		public override string GetSignatureForError ()
		{
			string s = TypeManager.CSharpSignature (invoke_mb);
			return s.Substring (0, s.IndexOf (".Invoke("));
		}
		
		public bool EmitMethod (EmitContext ec)
		{
			if (!CreateMethodHost (ec))
				return false;

			MethodBuilder builder = GetMethodBuilder ();
			ILGenerator ig = builder.GetILGenerator ();
			aec.ig = ig;
			
			Parameters.ApplyAttributes (aec, builder);

			//
			// Adjust based on the computed state of the
			// method from CreateMethodHost
			
			aec.MethodIsStatic = (method_modifiers & Modifiers.STATIC) != 0;
			
			aec.EmitMeta (Block);
			aec.EmitResolvedTopBlock (Block, unreachable);
			return true;
		}

		public override void CreateScopeType (EmitContext ec, ScopeInfo scope)
		{
			TypeBuilder container = ec.TypeContainer.TypeBuilder;
			string name = String.Format ("<>AnonHelp<{0}>", scope.id);

			scope.ScopeTypeBuilder = container.DefineNestedType (
				name, TypeAttributes.AutoLayout | TypeAttributes.Class |
				TypeAttributes.NestedAssembly, TypeManager.object_type, null);

			Type [] constructor_types = TypeManager.NoTypes;
			scope.ScopeConstructor = scope.ScopeTypeBuilder.DefineConstructor (
				MethodAttributes.Public | MethodAttributes.HideBySig |
				MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
				CallingConventions.HasThis, constructor_types);

			TypeManager.RegisterMethod (scope.ScopeConstructor, Parameters.EmptyReadOnlyParameters);

			ILGenerator cig = scope.ScopeConstructor.GetILGenerator ();
			cig.Emit (OpCodes.Ldarg_0);
			cig.Emit (OpCodes.Call, TypeManager.object_ctor);
			cig.Emit (OpCodes.Ret);
		}

		public static void Error_AddressOfCapturedVar (string name, Location loc)
		{
			Report.Error (1686, loc,
				"Local variable `{0}' or its members cannot have their address taken and be used inside an anonymous method block",
				name);
		}
	}

	//
	// This will emit the code for the delegate, as well delegate creation on the host
	//
	public class AnonymousDelegate : DelegateCreation {
		AnonymousMethod am;

		public AnonymousDelegate (AnonymousMethod am, Type target_type, Location l)
		{
			type = target_type;
			loc = l;
			this.am = am;
		}

		public override Expression DoResolve (EmitContext ec)
		{
			eclass = ExprClass.Value;

			return this;
		}
		
		public override void Emit (EmitContext ec)
		{
			if (!am.EmitMethod (ec))
				return;

			//
			// Now emit the delegate creation.
			//
			if ((am.method.ModFlags & Modifiers.STATIC) == 0)
				delegate_instance_expression = new AnonymousInstance (am);
			
			Expression ml = Expression.MemberLookup (ec, type, ".ctor", loc);
			constructor_method = ((MethodGroupExpr) ml).Methods [0];
			delegate_method = am.GetMethodBuilder ();
			base.Emit (ec);
		}

		class AnonymousInstance : Expression {
			AnonymousMethod am;
			
			public AnonymousInstance (AnonymousMethod am)
			{
				this.am = am;
				eclass = ExprClass.Value;
			}

			public override Expression DoResolve (EmitContext ec)
			{
				return this;
			}
			
			public override void Emit (EmitContext ec)
			{
				am.aec.EmitMethodHostInstance (ec, am);
			}
		}
	}

	class CapturedParameter {
		public Type Type;
		public FieldBuilder FieldBuilder;
		public int Idx;

		public CapturedParameter (Type type, int idx)
		{
			Type = type;
			Idx = idx;
		}
	}

	//
	// Here we cluster all the variables captured on a given scope, we also
	// keep some extra information that might be required on each scope.
	//
	public class ScopeInfo {
		public CaptureContext CaptureContext;
		public ScopeInfo ParentScope;
		public Block ScopeBlock;
		public bool NeedThis = false;
		public bool HostsParameters = false;
		
		// For tracking the number of scopes created.
		public int id;
		static int count;
		bool inited = false;
		
		ArrayList locals = new ArrayList ();
		ArrayList children = new ArrayList ();

		//
		// The types and fields generated
		//
		public TypeBuilder ScopeTypeBuilder;
		public ConstructorBuilder ScopeConstructor;
		public FieldBuilder THIS;
		public FieldBuilder ParentLink;

		//
		// Points to the object of type `ScopeTypeBuilder' that
		// holds the data for the scope
		//
		LocalBuilder scope_instance;
		
		public ScopeInfo (CaptureContext cc, Block b)
		{
			CaptureContext = cc;
			ScopeBlock = b;
			id = count++;

			cc.AddScope (this);
		}

		public void AddLocal (LocalInfo li)
		{
			if (locals.Contains (li))
				return;

			locals.Add (li);
		}

		public bool IsCaptured (LocalInfo li)
		{
			return locals.Contains (li);
		}
		
		internal void AddChild (ScopeInfo si)
		{
			if (children.Contains (si))
				return;

			//
			// If any of the current children should be a children of `si', move them there
			//
			ArrayList move_queue = null;
			foreach (ScopeInfo child in children){
				if (child.ScopeBlock.IsChildOf (si.ScopeBlock)){
					if (move_queue == null)
						move_queue = new ArrayList ();
					move_queue.Add (child);
					child.ParentScope = si;
					si.AddChild (child);
				}
			}
			
			children.Add (si);

			if (move_queue != null){
				foreach (ScopeInfo child in move_queue){
					children.Remove (child);
				}
			}
		} 

		static int indent = 0;

		void Pad ()
		{
			for (int i = 0; i < indent; i++)
				Console.Write ("    ");
		}

		void EmitDebug ()
		{
			//Console.WriteLine (Environment.StackTrace);
			Pad ();
			Console.WriteLine ("START");
			indent++;
			Pad ();
			Console.WriteLine ("NeedThis=" + NeedThis);
			foreach (LocalInfo li in locals){
				Pad ();
				Console.WriteLine ("var {0}", MakeFieldName (li.Name));
			}
			
			foreach (ScopeInfo si in children)
				si.EmitDebug ();
			indent--;
			Pad ();
			Console.WriteLine ("END");
		}
		
		public string MakeHelperName ()
		{
			return String.Format ("<>AnonHelp<{0}>", id);
		}

		private string MakeFieldName (string local_name)
		{
			return "<" + id + ":" + local_name + ">";
		}

		public void EmitScopeType (EmitContext ec)
		{
			// EmitDebug ();

			if (ScopeTypeBuilder != null)
				return;
			
			TypeBuilder container = ec.TypeContainer.TypeBuilder;

			CaptureContext.Host.CreateScopeType (ec, this);
			
			if (NeedThis)
				THIS = ScopeTypeBuilder.DefineField ("<>THIS", container, FieldAttributes.Assembly);

			if (ParentScope != null){
				if (ParentScope.ScopeTypeBuilder == null){
					throw new Exception (String.Format ("My parent has not been initialized {0} and {1}", ParentScope, this));
				}

				if (ParentScope.ScopeTypeBuilder != ScopeTypeBuilder)
					ParentLink = ScopeTypeBuilder.DefineField ("<>parent", ParentScope.ScopeTypeBuilder,
										   FieldAttributes.Assembly);
			}
			
			if (NeedThis && ParentScope != null)
				throw new Exception ("I was not expecting THIS && having a parent");

			foreach (LocalInfo info in locals)
				info.FieldBuilder = ScopeTypeBuilder.DefineField (
					MakeFieldName (info.Name), info.VariableType, FieldAttributes.Assembly);

			if (HostsParameters){
				Hashtable captured_parameters = CaptureContext.captured_parameters;
				
				foreach (DictionaryEntry de in captured_parameters){
					string name = (string) de.Key;
					CapturedParameter cp = (CapturedParameter) de.Value;
					FieldBuilder fb;
					
					fb = ScopeTypeBuilder.DefineField ("<p:" + name + ">", cp.Type, FieldAttributes.Assembly);
					cp.FieldBuilder = fb;
				}
			}

			foreach (ScopeInfo si in children){
				si.EmitScopeType (ec);
			}
		}

		public void CloseTypes ()
		{
			RootContext.RegisterCompilerGeneratedType (ScopeTypeBuilder);
			foreach (ScopeInfo si in children)
				si.CloseTypes ();
		}

		//
		// Emits the initialization code for the scope
		//
		public void EmitInitScope (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (inited)
				return;

			if (ScopeConstructor == null)
				throw new Exception ("ScopeConstructor is null for" + this.ToString ());
			
			if (!CaptureContext.Host.IsIterator) {
				scope_instance = ig.DeclareLocal (ScopeTypeBuilder);
				ig.Emit (OpCodes.Newobj, (ConstructorInfo) ScopeConstructor);
				ig.Emit (OpCodes.Stloc, scope_instance);
			}

			if (THIS != null){
				if (CaptureContext.Host.IsIterator) {
					ig.Emit (OpCodes.Ldarg_0);
					ig.Emit (OpCodes.Ldarg_1);
				} else {
					ig.Emit (OpCodes.Ldloc, scope_instance);
					ig.Emit (OpCodes.Ldarg_0);
				}
				ig.Emit (OpCodes.Stfld, THIS);
			}

			//
			// Copy the parameter values, if any
			//
			int extra = ec.IsStatic ? 0 : 1;
			if (CaptureContext.Host.IsIterator)
				extra++;
			if (HostsParameters){
				Hashtable captured_parameters = CaptureContext.captured_parameters;
				
				foreach (DictionaryEntry de in captured_parameters){
					CapturedParameter cp = (CapturedParameter) de.Value;

					EmitScopeInstance (ig);
					ParameterReference.EmitLdArg (ig, cp.Idx + extra);
					ig.Emit (OpCodes.Stfld, cp.FieldBuilder);
				}
			}

			if (ParentScope != null){
				if (!ParentScope.inited)
					ParentScope.EmitInitScope (ec);

				if (ParentScope.ScopeTypeBuilder != ScopeTypeBuilder) {
					//
					// Only emit initialization in our capturecontext world
					//
					if (ParentScope.CaptureContext == CaptureContext){
						EmitScopeInstance (ig);
						ParentScope.EmitScopeInstance (ig);
						ig.Emit (OpCodes.Stfld, ParentLink);
					} else {
						EmitScopeInstance (ig);
						ig.Emit (OpCodes.Ldarg_0);
						ig.Emit (OpCodes.Stfld, ParentLink);
					}
				}
			}
			inited = true;
		}

		public void EmitScopeInstance (ILGenerator ig)
		{
			if (CaptureContext.Host.IsIterator)
				ig.Emit (OpCodes.Ldarg_0);
			else
				ig.Emit (OpCodes.Ldloc, scope_instance);
		}

		public static void CheckCycles (string msg, ScopeInfo s)
		{
			ArrayList l = new ArrayList ();
			int n = 0;
			
			for (ScopeInfo p = s; p != null; p = p.ParentScope,n++){
				if (l.Contains (p)){
					Console.WriteLine ("Loop detected {0} in {1}", n, msg);
					throw new Exception ();
				}
				l.Add (p);
			}
		}
		
		static void DoPath (StringBuilder sb, ScopeInfo start)
		{
			CheckCycles ("print", start);
			
			if (start.ParentScope != null){
				DoPath (sb, start.ParentScope);
				sb.Append (", ");
			}
			sb.Append ((start.id).ToString ());
		}
		
		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			
			sb.Append ("{");
			if (CaptureContext != null){
				sb.Append (CaptureContext.ToString ());
				sb.Append (":");
			}

			DoPath (sb, this);
			sb.Append ("}");

			return sb.ToString ();
		}
	}

	//
	// CaptureContext objects are created on demand if a method has
	// anonymous methods and kept on the ToplevelBlock.
	//
	// If they exist, all ToplevelBlocks in the containing block are
	// linked together (children pointing to their parents).
	//
	public class CaptureContext {
		public static int count;
		public int cc_id;
		public Location loc;
		
		//
		// Points to the toplevel block that owns this CaptureContext
		//
		ToplevelBlock toplevel_owner;
		Hashtable scopes = new Hashtable ();
		bool have_captured_vars = false;
		bool referenced_this = false;
		ScopeInfo topmost = null;

		//
		// Captured fields
		//
		Hashtable captured_fields = new Hashtable ();
		Hashtable captured_variables = new Hashtable ();
		public Hashtable captured_parameters = new Hashtable ();
		public AnonymousContainer Host;
		
		public CaptureContext (ToplevelBlock toplevel_owner, Location loc,
				       AnonymousContainer host)
		{
			cc_id = count++;
			this.toplevel_owner = toplevel_owner;
			this.loc = loc;

			if (host != null)
				Host = host;
		}

		void DoPath (StringBuilder sb, CaptureContext cc)
		{
			if (cc.ParentCaptureContext != null){
				DoPath (sb, cc.ParentCaptureContext);
				sb.Append (".");
			}
			sb.Append (cc.cc_id.ToString ());
		}

		public void ReParent (ToplevelBlock new_toplevel, AnonymousContainer new_host)
		{
			toplevel_owner = new_toplevel;
			Host = new_host;

			for (CaptureContext cc = ParentCaptureContext; cc != null;
			     cc = cc.ParentCaptureContext) {
				cc.Host = new_host;
			}
		}
		
		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();
			sb.Append ("[");
			DoPath (sb, this);
			sb.Append ("]");
			return sb.ToString ();
		}

		public ToplevelBlock ParentToplevel {
			get {
				return toplevel_owner.Container;
			}
		}

		public CaptureContext ParentCaptureContext {
			get {
				ToplevelBlock parent = ParentToplevel;
				
				return (parent == null) ? null : parent.CaptureContext;
			}
		}

		// Returns the deepest of two scopes
		public ScopeInfo Deepest (ScopeInfo a, ScopeInfo b)
		{
			ScopeInfo p;

			if (a == null)
				return b;
			if (b == null)
				return a;
			if (a == b)
				return a;

			//
			// If they Scopes are on the same CaptureContext, we do the double
			// checks just so if there is an invariant change in the future,
			// we get the exception at the end
			//
			for (p = a; p != null; p = p.ParentScope)
				if (p == b)
					return a;
			
			for (p = b; p != null; p = p.ParentScope)
				if (p == a)
					return b;

			CaptureContext ca = a.CaptureContext;
			CaptureContext cb = b.CaptureContext;

			for (CaptureContext c = ca; c != null; c = c.ParentCaptureContext)
				if (c == cb)
					return a;

			for (CaptureContext c = cb; c != null; c = c.ParentCaptureContext)
				if (c == ca)
					return b;
			throw new Exception ("Should never be reached");
		}

		void AdjustMethodScope (AnonymousContainer am, ScopeInfo scope)
		{
			am.Scope = Deepest (am.Scope, scope);
		}

		void LinkScope (ScopeInfo scope, int id)
		{
			ScopeInfo parent = (ScopeInfo) scopes [id];
			if (parent == scope)
				return;
			
			scope.ParentScope = parent;
			parent.AddChild (scope);

			if (scope == topmost)
				topmost = parent;
		}
		
		public void AddLocal (AnonymousContainer am, LocalInfo li)
		{
			if (li.Block.Toplevel != toplevel_owner){
				ParentCaptureContext.AddLocal (am, li);
				return;
			}
			int block_id = li.Block.ID;
			ScopeInfo scope;
			if (scopes [block_id] == null){
				scope = new ScopeInfo (this, li.Block);
				scopes [block_id] = scope;
			} else
				scope = (ScopeInfo) scopes [block_id];

			if (topmost == null){
				topmost = scope;
			} else {
				// Link to parent

				for (Block b = scope.ScopeBlock.Parent; b != null; b = b.Parent){
					if (scopes [b.ID] != null){
						LinkScope (scope, b.ID);
						break;
					}
				}

				if (scope.ParentScope == null && ParentCaptureContext != null){
					CaptureContext pcc = ParentCaptureContext;
					
					for (Block b = am.ContainingBlock; b != null; b = b.Parent){
						if (pcc.scopes [b.ID] != null){
							pcc.LinkScope (scope, b.ID);
 							break;
						}
					}
				}
			}

			//
			// Adjust the owner
			//
			if (Host != null)
				AdjustMethodScope (Host, topmost);

			//
			// Adjust the user
			//
			AdjustMethodScope (am, scope);
			
			if (captured_variables [li] != null)
				return;
			
			have_captured_vars = true;
			captured_variables [li] = li;
			scope.AddLocal (li);
		}

		//
		// Retursn the CaptureContext for the block that defines the parameter `name'
		//
		static CaptureContext _ContextForParameter (ToplevelBlock current, string name)
		{
			ToplevelBlock container = current.Container;
			if (container != null){
				CaptureContext cc = _ContextForParameter (container, name);
				if (cc != null)
					return cc;
			}
			if (current.IsParameterReference (name))
				return current.ToplevelBlockCaptureContext;
			return null;
		}

		static CaptureContext ContextForParameter (ToplevelBlock current, string name)
		{
			CaptureContext cc = _ContextForParameter (current, name);
			if (cc == null)
				throw new Exception (String.Format ("request for parameteter {0} failed: not found", name));
			return cc;
		}
		
		//
		// Records the captured parameter at the appropriate CaptureContext
		//
		public void AddParameter (EmitContext ec, AnonymousContainer am,
					  string name, Type t, int idx)
		{
			CaptureContext cc = ContextForParameter (ec.CurrentBlock.Toplevel, name);

			cc.AddParameterToContext (am, name, t, idx);
		}

		//
		// Records the parameters in the context
		//
		public void AddParameterToContext (AnonymousContainer am, string name, Type t, int idx)
		{
			if (captured_parameters == null)
				captured_parameters = new Hashtable ();
			if (captured_parameters [name] == null)
				captured_parameters [name] = new CapturedParameter (t, idx);

			if (topmost == null){
				//
				// Create one ScopeInfo, if there are none.
				//
				topmost = new ScopeInfo (this, toplevel_owner);
				scopes [toplevel_owner.ID] = topmost;
			} else {
				//
				// If the topmost ScopeInfo is not at the topblock level, insert
				// a new ScopeInfo there.
				//
				// FIXME: This code probably should be evolved to be like the code
				// in AddLocal
				//
				if (topmost.ScopeBlock != toplevel_owner){
					ScopeInfo par_si = new ScopeInfo (this, toplevel_owner);
					ScopeInfo old_top = topmost;
					scopes [toplevel_owner.ID] = topmost;
					topmost.ParentScope = par_si;
					topmost = par_si;
					topmost.AddChild (old_top);
				}
			}
			
			topmost.HostsParameters = true;
			AdjustMethodScope (am, topmost);
		}

		//
		// Captured fields are only recorded on the topmost CaptureContext, because that
		// one is the one linked to the owner of instance fields
		//
		public void AddField (EmitContext ec, AnonymousContainer am, FieldExpr fe)
		{
			if (fe.FieldInfo.IsStatic)
				throw new Exception ("Attempt to register a static field as a captured field");
			CaptureContext parent = ParentCaptureContext;
			if (parent != null) {
				parent.AddField (ec, am, fe);
				return;
			}

			if (topmost == null){
				//
				// Create one ScopeInfo, if there are none.
				//
				topmost = new ScopeInfo (this, toplevel_owner);
				scopes [toplevel_owner.ID] = topmost;
			}
			
			AdjustMethodScope (am, topmost);
		}

		public void CaptureThis (AnonymousContainer am)
		{
			CaptureContext parent = ParentCaptureContext;
			if (parent != null) {
				parent.CaptureThis (am);
				return;
			}
			referenced_this = true;

			if (topmost == null){
				//
				// Create one ScopeInfo, if there are none.
				//
				topmost = new ScopeInfo (this, toplevel_owner);
				scopes [toplevel_owner.ID] = topmost;
			
				AdjustMethodScope (am, topmost);
			}
		}

		public bool HaveCapturedVariables {
			get {
				return have_captured_vars;
			}
		}
		
		public bool HaveCapturedFields {
			get {
				CaptureContext parent = ParentCaptureContext;
				if (parent != null)
					return parent.HaveCapturedFields;
				return captured_fields.Count > 0;
			}
		}

		public bool IsCaptured (LocalInfo local)
		{
			foreach (ScopeInfo si in scopes.Values){
				if (si.IsCaptured (local))
					return true;
			}
			return false;
		}

		//
		// Returns whether the parameter is captured
		//
		public bool IsParameterCaptured (string name)
		{
			if (ParentCaptureContext != null && ParentCaptureContext.IsParameterCaptured (name))
				return true;
			
			if (captured_parameters != null)
				return captured_parameters [name] != null;
			return false;
		}

		public void EmitAnonymousHelperClasses (EmitContext ec)
		{
			if (topmost != null){
				topmost.NeedThis = HaveCapturedFields || referenced_this;
				topmost.EmitScopeType (ec);
			} 
		}

		public void CloseAnonymousHelperClasses ()
		{
			if (topmost != null)
				topmost.CloseTypes ();
		}

		public void EmitInitScope (EmitContext ec)
		{
			EmitAnonymousHelperClasses (ec);
			if (topmost != null)
				topmost.EmitInitScope (ec);
		}

		ScopeInfo GetScopeFromBlock (EmitContext ec, Block b)
		{
			ScopeInfo si;
			
			si = (ScopeInfo) scopes [b.ID];
			if (si == null)
				throw new Exception ("Si is null for block " + b.ID);
			si.EmitInitScope (ec);

			return si;
		}

		//
		// Emits the opcodes necessary to load the instance of the captured
		// variable in `li'
		//
		public void EmitCapturedVariableInstance (EmitContext ec, LocalInfo li,
							  AnonymousContainer am)
		{
			ILGenerator ig = ec.ig;
			ScopeInfo si;

			if (li.Block.Toplevel == toplevel_owner){
				si = GetScopeFromBlock (ec, li.Block);
				si.EmitScopeInstance (ig);
				return;
			}

			si = am.Scope;
			ig.Emit (OpCodes.Ldarg_0);
			if (si != null){
				if (am.IsIterator && (si.ScopeBlock.Toplevel == li.Block.Toplevel)) {
					return;
				}

				while (si.ScopeBlock.ID != li.Block.ID){
					if (si.ParentLink != null)
						ig.Emit (OpCodes.Ldfld, si.ParentLink);
					si = si.ParentScope;
					if (si == null) {
						si = am.Scope;
						Console.WriteLine ("Target: {0} {1}", li.Block.ID, li.Name);
						while (si.ScopeBlock.ID != li.Block.ID){
							Console.WriteLine ("Trying: {0}", si.ScopeBlock.ID);
							si = si.ParentScope;
						}

						throw new Exception (
							     String.Format ("Never found block {0} starting at {1} while looking up {2}",
									    li.Block.ID, am.Scope.ScopeBlock.ID, li.Name));
					}
				}
			}
		}

		//
		// Internal routine that loads the instance to reach parameter `name'
		//
		void EmitParameterInstance (EmitContext ec, string name)
		{
			CaptureContext cc = ContextForParameter (ec.CurrentBlock.Toplevel, name);
			if (cc != this){
				cc.EmitParameterInstance (ec, name);
				return;
			}

			CapturedParameter par_info = (CapturedParameter) captured_parameters [name];
			if (par_info != null){
				// 
				// FIXME: implementing this.
				//
			}
			ILGenerator ig = ec.ig;

			ScopeInfo si;

			if (ec.CurrentBlock.Toplevel == toplevel_owner) {
				si = GetScopeFromBlock (ec, toplevel_owner);
				si.EmitScopeInstance (ig);
			} else {
				si = ec.CurrentAnonymousMethod.Scope;
				ig.Emit (OpCodes.Ldarg_0);
			}

			if (si != null){
				while (si.ParentLink != null) {
					ig.Emit (OpCodes.Ldfld, si.ParentLink);
					si = si.ParentScope;
				} 
			}
		}

		//
		// Emits the code necessary to load the parameter named `name' within
		// an anonymous method.
		//
		public void EmitParameter (EmitContext ec, string name, bool leave_copy, bool prepared, ref LocalTemporary temp)
		{
			CaptureContext cc = ContextForParameter (ec.CurrentBlock.Toplevel, name);
			if (cc != this){
				cc.EmitParameter (ec, name, leave_copy, prepared, ref temp);
				return;
			}
			if (!prepared)
				EmitParameterInstance (ec, name);
			CapturedParameter par_info = (CapturedParameter) captured_parameters [name];
			if (par_info != null){
				// 
				// FIXME: implementing this.
				//
			}
			ec.ig.Emit (OpCodes.Ldfld, par_info.FieldBuilder);

			if (leave_copy){
				ec.ig.Emit (OpCodes.Dup);
				temp = new LocalTemporary (ec, par_info.FieldBuilder.FieldType);
				temp.Store (ec);
			}
		}

		//
		// Implements the assignment of `source' to the paramenter named `name' within
		// an anonymous method.
		//
		public void EmitAssignParameter (EmitContext ec, string name, Expression source, bool leave_copy, bool prepare_for_load, ref LocalTemporary temp)
		{
			CaptureContext cc = ContextForParameter (ec.CurrentBlock.Toplevel, name);
			if (cc != this){
				cc.EmitAssignParameter (ec, name, source, leave_copy, prepare_for_load, ref temp);
				return;
			}
			ILGenerator ig = ec.ig;
			CapturedParameter par_info = (CapturedParameter) captured_parameters [name];

			EmitParameterInstance (ec, name);
			if (prepare_for_load)
				ig.Emit (OpCodes.Dup);
			source.Emit (ec);
			if (leave_copy){
				ig.Emit (OpCodes.Dup);
				temp = new LocalTemporary (ec, par_info.FieldBuilder.FieldType);
				temp.Store (ec);
			}
			ig.Emit (OpCodes.Stfld, par_info.FieldBuilder);
			if (temp != null)
				temp.Emit (ec);
		}

		//
		// Emits the address for the parameter named `name' within
		// an anonymous method.
		//
		public void EmitAddressOfParameter (EmitContext ec, string name)
		{
			CaptureContext cc = ContextForParameter (ec.CurrentBlock.Toplevel, name);
			if (cc != this){
				cc.EmitAddressOfParameter (ec, name);
				return;
			}
			EmitParameterInstance (ec, name);
			CapturedParameter par_info = (CapturedParameter) captured_parameters [name];
			ec.ig.Emit (OpCodes.Ldflda, par_info.FieldBuilder);
		}

		//
		// The following methods are only invoked on the host for the
		// anonymous method.
		//
		public void EmitMethodHostInstance (EmitContext target, AnonymousContainer am)
		{
			ILGenerator ig = target.ig;
			ScopeInfo si = am.Scope;

			AnonymousContainer container = am.ContainerAnonymousMethod;

			if ((si == null) || ((container != null) && (si == container.Scope))) {
				ig.Emit (OpCodes.Ldarg_0);
				return;
			}

			si.EmitInitScope (target);
			si.EmitScopeInstance (ig);
		}

		ArrayList all_scopes = new ArrayList ();
		
		public void AddScope (ScopeInfo si)
		{
			all_scopes.Add (si);
			toplevel_owner.RegisterCaptureContext (this);
		}

		//
		// Links any scopes that were not linked previously
		//
		public void AdjustScopes ()
		{
			foreach (ScopeInfo scope in all_scopes){
				if (scope.ParentScope != null)
					continue;

				for (Block b = scope.ScopeBlock.Parent; b != null; b = b.Parent){
					if (scopes [b.ID] != null){
						LinkScope (scope, b.ID);
						break;
					}
				}

				if (scope.ParentScope == null && ParentCaptureContext != null){
					CaptureContext pcc = ParentCaptureContext;
					
					for (Block b = Host.ContainingBlock; b != null; b = b.Parent){
						if (pcc.scopes [b.ID] != null){
							pcc.LinkScope (scope, b.ID);
 							break;
						}
					}
				}
			}
		}
	}
}
