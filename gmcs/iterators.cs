//
// iterators.cs: Support for implementing iterators
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2003 Ximian, Inc.
//
// TODO:
//    Flow analysis for Yield.
//    Emit calls to base object constructor.
//
// Generics note:
//    Current should be defined to return T, and IEnumerator.Current returns object
//

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp {

	public interface IIteratorContainer {

		//
		// Invoked if a yield statement is found in the body
		//
		void SetYields ();
	}
	
	public class Yield : Statement {
		Expression expr;
		ArrayList finally_blocks;
		bool resolved;

		public Yield (Expression expr, Location l)
		{
			this.expr = expr;
			loc = l;
		}

		public static bool CheckContext (EmitContext ec, Location loc)
		{
			if (ec.InFinally) {
				Report.Error (1625, loc, "Cannot yield in the body of a " +
					      "finally clause");
				return false;
			} 
			
			if (ec.InUnsafe) {
				Report.Error (1629, loc, "Unsafe code may not appear in iterators");
				return false;
			}
			if (ec.InCatch){
				Report.Error (1631, loc, "Cannot yield in the body of a " +
					      "catch clause");
				return false;
			}

			AnonymousContainer am = ec.CurrentAnonymousMethod;
			if ((am != null) && !am.IsIterator){
				Report.Error (1621, loc, "The yield statement cannot be used inside anonymous method blocks");
				return false;
			}

			if (ec.CurrentBranching.InTryWithCatch ()) {
				Report.Error (1626, loc, "Cannot yield a value in the body of a " +
					"try block with a catch clause");
				return false;
			}
			return true;
		}
		
		public override bool Resolve (EmitContext ec)
		{
			expr = expr.Resolve (ec);
			if (expr == null)
				return false;

			resolved = true;

			if (!CheckContext (ec, loc))
				return false;

			Iterator iterator = ec.CurrentIterator;

			if (expr.Type != iterator.IteratorType){
				expr = Convert.ImplicitConversionRequired (
					ec, expr, iterator.IteratorType, loc);
				if (expr == null)
					return false;
			}

			ec.CurrentBranching.StealFinallyClauses (ref finally_blocks);
			return true;
		}

		protected override void DoEmit (EmitContext ec)
		{
			ec.CurrentIterator.MarkYield (ec, expr, finally_blocks);
		}
	}

	public class YieldBreak : Statement {

		public YieldBreak (Location l)
		{
			loc = l;
		}

		public override bool Resolve (EmitContext ec)
		{
			if (!Yield.CheckContext (ec, loc))
				return false;

			ec.CurrentBranching.CurrentUsageVector.Goto ();
			return true;
		}

		protected override void DoEmit (EmitContext ec)
		{
			ec.CurrentIterator.EmitYieldBreak (ec.ig);
		}
	}

	public class Iterator : Class {
		protected ToplevelBlock original_block;
		protected ToplevelBlock block;
		string original_name;

		Type iterator_type;
		TypeExpr iterator_type_expr;
		bool is_enumerable;
		public readonly bool IsStatic;

		Hashtable fields;

		//
		// The state as we generate the iterator
		//
		Label move_next_ok, move_next_error;
		ArrayList resume_points = new ArrayList ();
		int pc;
		
		//
		// Context from the original method
		//
		TypeContainer container;
		TypeExpr current_type;
		Type this_type;
		Type return_type;
		InternalParameters parameters;

		MethodInfo dispose_method;
		MoveNextMethod move_next_method;
		Constructor ctor;
		CaptureContext cc;

		Expression enumerator_type;
		Expression enumerable_type;
		Expression generic_enumerator_type;
		Expression generic_enumerable_type;
		TypeArguments generic_args;

		protected enum State {
			Uninitialized	= -2,
			After,
			Running
		}

		static int proxy_count;

		public void EmitYieldBreak (ILGenerator ig)
		{
			ig.Emit (OpCodes.Ldarg_0);
			IntConstant.EmitInt (ig, (int) State.After);
			ig.Emit (OpCodes.Stfld, pc_field.FieldBuilder);
			ig.Emit (OpCodes.Br, move_next_error);
		}

		public void EmitMoveNext (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			move_next_ok = ig.DefineLabel ();
			move_next_error = ig.DefineLabel ();

			LocalBuilder retval = ec.GetTemporaryLocal (TypeManager.int32_type);

			ig.BeginExceptionBlock ();

			Label dispatcher = ig.DefineLabel ();
			ig.Emit (OpCodes.Br, dispatcher);

			ResumePoint entry_point = new ResumePoint (null);
			resume_points.Add (entry_point);
			entry_point.Define (ig);

			ec.EmitTopBlock (original_block, parameters, Location);

			EmitYieldBreak (ig);

			ig.MarkLabel (dispatcher);

			Label [] labels = new Label [resume_points.Count];
			for (int i = 0; i < labels.Length; i++)
				labels [i] = ((ResumePoint) resume_points [i]).Label;

			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Ldfld, pc_field.FieldBuilder);
			ig.Emit (OpCodes.Switch, labels);

			Label end = ig.DefineLabel ();

			ig.MarkLabel (move_next_error);
  			ig.Emit (OpCodes.Ldc_I4_0); 
			ig.Emit (OpCodes.Stloc, retval);
			ig.Emit (OpCodes.Leave, end);

			ig.MarkLabel (move_next_ok);
			ig.Emit (OpCodes.Ldc_I4_1);
			ig.Emit (OpCodes.Stloc, retval);
			ig.Emit (OpCodes.Leave, end);

			ig.BeginFaultBlock ();

			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Callvirt, dispose_method);

			ig.EndExceptionBlock ();

			ig.MarkLabel (end);
			ig.Emit (OpCodes.Ldloc, retval);
			ig.Emit (OpCodes.Ret);
		}

		public void EmitDispose (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			Label end = ig.DefineLabel ();
			Label dispatcher = ig.DefineLabel ();
			ig.Emit (OpCodes.Br, dispatcher);

			Label [] labels = new Label [resume_points.Count];
			for (int i = 0; i < labels.Length; i++) {
				ResumePoint point = (ResumePoint) resume_points [i];

				if (point.FinallyBlocks == null) {
					labels [i] = end;
					continue;
				}

				labels [i] = ig.DefineLabel ();
				ig.MarkLabel (labels [i]);

				ig.BeginExceptionBlock ();
				ig.BeginFinallyBlock ();

				foreach (ExceptionStatement stmt in point.FinallyBlocks) {
					if (stmt != null)
						stmt.EmitFinally (ec);
				}

				ig.EndExceptionBlock ();
				ig.Emit (OpCodes.Br, end);
			}

			ig.MarkLabel (dispatcher);
			ig.Emit (OpCodes.Ldarg_0);
			ig.Emit (OpCodes.Ldfld, pc_field.FieldBuilder);
			ig.Emit (OpCodes.Switch, labels);

			ig.Emit (OpCodes.Ldarg_0);
			IntConstant.EmitInt (ig, (int) State.After);
			ig.Emit (OpCodes.Stfld, pc_field.FieldBuilder);

			ig.MarkLabel (end);
		}

		protected class ResumePoint
		{
			public Label Label;
			public readonly ExceptionStatement[] FinallyBlocks;

			public ResumePoint (ArrayList list)
			{
				if (list != null) {
					FinallyBlocks = new ExceptionStatement [list.Count];
					list.CopyTo (FinallyBlocks, 0);
				}
			}

			public void Define (ILGenerator ig)
			{
				Label = ig.DefineLabel ();
				ig.MarkLabel (Label);
			}
		}

		//
		// Called back from Yield
		//
		public void MarkYield (EmitContext ec, Expression expr,
				       ArrayList finally_blocks)
		{
			ILGenerator ig = ec.ig;

			// Store the new current
			ig.Emit (OpCodes.Ldarg_0);
			expr.Emit (ec);
			ig.Emit (OpCodes.Stfld, current_field.FieldBuilder);

			// increment pc
			pc++;
			ig.Emit (OpCodes.Ldarg_0);
			IntConstant.EmitInt (ig, pc);
			ig.Emit (OpCodes.Stfld, pc_field.FieldBuilder);

			// Return ok
			ig.Emit (OpCodes.Br, move_next_ok);

			ResumePoint point = new ResumePoint (finally_blocks);
			resume_points.Add (point);
			point.Define (ig);
		}

		public void MarkFinally (EmitContext ec, ArrayList finally_blocks)
		{
			ILGenerator ig = ec.ig;

			// increment pc
			pc++;
			ig.Emit (OpCodes.Ldarg_0);
			IntConstant.EmitInt (ig, pc);
			ig.Emit (OpCodes.Stfld, pc_field.FieldBuilder);

			ResumePoint point = new ResumePoint (finally_blocks);
			resume_points.Add (point);
			point.Define (ig);
		}

		private static MemberName MakeProxyName (string name)
		{
			int pos = name.LastIndexOf ('.');
			if (pos > 0)
				name = name.Substring (pos + 1);

			return new MemberName ("<" + name + ">__" + (proxy_count++));
		}

		//
		// Our constructor
		//
		public Iterator (TypeContainer container, string name, Type return_type,
				 InternalParameters parameters,
				 int modifiers, ToplevelBlock block, Location loc)
			: base (container.NamespaceEntry, container, MakeProxyName (name),
				(modifiers & Modifiers.UNSAFE) | Modifiers.PRIVATE, null, loc)
		{
			this.container = container;
			this.return_type = return_type;
			this.parameters = parameters;
			this.original_name = name;
			this.original_block = block;
			this.block = new ToplevelBlock (block, parameters.Parameters, loc);

			fields = new Hashtable ();

			IsStatic = (modifiers & Modifiers.STATIC) != 0;
		}

		public AnonymousContainer Host {
			get { return move_next_method; }
		}

		public bool DefineIterator ()
		{
			ec = new EmitContext (this, Mono.CSharp.Location.Null, null, null, ModFlags);
			ec.CurrentAnonymousMethod = move_next_method;
			ec.CurrentIterator = this;
			ec.InIterator = true;

			if (!CheckType (return_type)) {
				Report.Error (
					1624, Location,
					"The body of `{0}' cannot be an iterator block " +
					"because '{1}' is not an iterator interface type",
					original_name, TypeManager.CSharpName (return_type));
				return false;
			}

			for (int i = 0; i < parameters.Count; i++){
				Parameter.Modifier mod = parameters.ParameterModifier (i);
				if ((mod & (Parameter.Modifier.REF | Parameter.Modifier.OUT)) != 0){
					Report.Error (
						1623, Location,
						"Iterators cannot have ref or out parameters");
					return false;
				}

				if ((mod & Parameter.Modifier.ARGLIST) != 0) {
					Report.Error (1636, Location, "__arglist is not allowed in parameter list of iterators");
					return false;
				}

				if (parameters.ParameterType (i).IsPointer) {
					Report.Error (1637, Location, "Iterators cannot have unsafe parameters or yield types");
					return false;
				}
			}

			if (container.CurrentType != null)
				this_type = container.CurrentType;
			else
				this_type = container.TypeBuilder;

			generic_args = new TypeArguments (Location);
			generic_args.Add (new TypeExpression (iterator_type, Location));

			ArrayList list = new ArrayList ();
			if (is_enumerable) {
				enumerable_type = new TypeExpression (
					TypeManager.ienumerable_type, Location);
				list.Add (enumerable_type);

				generic_enumerable_type = new ConstructedType (
					TypeManager.generic_ienumerable_type,
					generic_args, Location);
				list.Add (generic_enumerable_type);
			}

			enumerator_type = new TypeExpression (
				TypeManager.ienumerator_type, Location);
			list.Add (enumerator_type);

			list.Add (new TypeExpression (TypeManager.idisposable_type, Location));

			generic_enumerator_type = new ConstructedType (
				TypeManager.generic_ienumerator_type,
				generic_args, Location);
			list.Add (generic_enumerator_type);

			iterator_type_expr = new TypeExpression (iterator_type, Location);

			container.AddIterator (this);

			Bases = list;
			return true;
		}

		MethodInfo FetchMethodDispose ()
		{
			MemberList dispose_list;

			dispose_list = FindMembers (
				current_type.Type,
				MemberTypes.Method, BindingFlags.Public | BindingFlags.Instance,
				Type.FilterName, "Dispose");

			if (dispose_list.Count != 1)
				throw new InternalErrorException ("Cannot find Dipose() method.");

			return (MethodInfo) dispose_list [0];
		}

		protected override bool DoDefineMembers ()
		{
			ec.InIterator = true;
			ec.CurrentIterator = this;
			ec.CurrentAnonymousMethod = move_next_method;
			ec.capture_context = cc;

			if (!base.DoDefineMembers ())
				return false;

			dispose_method = FetchMethodDispose ();
			if (dispose_method == null)
				return false;

			return true;
		}

		public override bool Define ()
		{
			if (!base.Define ())
				return false;

			ec.InIterator = true;
			ec.CurrentIterator = this;
			ec.CurrentAnonymousMethod = move_next_method;
			ec.capture_context = cc;
			ec.TypeContainer = ec.TypeContainer.Parent;

			if (ec.TypeContainer.CurrentType != null)
				ec.ContainerType = ec.TypeContainer.CurrentType;
			else
				ec.ContainerType = ec.TypeContainer.TypeBuilder;

			ec.ig = move_next_method.method.MethodBuilder.GetILGenerator ();

			if (!ctor.Define ())
				return false;

			bool unreachable;

			if (!ec.ResolveTopBlock (null, original_block, parameters, Location, out unreachable))
				return false;

			if (!ec.ResolveTopBlock (null, block, parameters, Location, out unreachable))
				return false;

			original_block.CompleteContexts ();

			cc.EmitAnonymousHelperClasses (ec);

			return true;
		}

		//
		// Returns the new block for the method, or null on failure
		//
		protected override bool DefineNestedTypes ()
		{
			if (CurrentType != null)
				current_type = new TypeExpression (CurrentType, Location);
			else
				current_type = new TypeExpression (TypeBuilder, Location);

			Define_Fields ();
			Define_Current (false);
			Define_Current (true);
			Define_MoveNext ();
			Define_Reset ();
			Define_Dispose ();

			Create_Block ();

			Define_Constructor ();

			if (is_enumerable) {
				Define_GetEnumerator (false);
				Define_GetEnumerator (true);
			}

			return base.DefineNestedTypes ();
		}


		Field pc_field;
		Field current_field;
		LocalInfo pc_local;
		LocalInfo current_local;
		Method dispose;

		void Create_Block ()
		{
			original_block.SetHaveAnonymousMethods (Location, move_next_method);
			block.SetHaveAnonymousMethods (Location, move_next_method);

			cc = original_block.CaptureContext;

			int first = IsStatic ? 0 : 1;

			ArrayList args = new ArrayList ();
			if (!IsStatic) {
				Type t = this_type;
				args.Add (new Argument (
					new ThisParameterReference (t, Location)));
				cc.CaptureThis ();
			}

			args.Add (new Argument (new BoolLiteral (false)));

			for (int i = 0; i < parameters.Count; i++) {
				Type t = parameters.ParameterType (i);
				string name = parameters.ParameterName (i);

				args.Add (new Argument (
					new SimpleParameterReference (t, first + i, Location)));

				cc.AddParameterToContext (move_next_method, name, t, first + i);
			}

			Expression new_expr = new New (current_type, args, Location);

			block.AddStatement (new NoCheckReturn (new_expr, Location));
		}

		void Define_Fields ()
		{
			pc_field = new Field (
				this, TypeManager.system_int32_expr, Modifiers.PRIVATE, "$PC",
				null, null, Location);
			AddField (pc_field);

			current_field = new Field (
				this, iterator_type_expr, Modifiers.PRIVATE, "$current",
				null, null, Location);
			AddField (current_field);
		}

		void Define_Constructor ()
		{
			Parameters ctor_params;

			ArrayList list = new ArrayList ();

			if (!IsStatic)
				list.Add (new Parameter (
					new TypeExpression (this_type, Location),
					"this", Parameter.Modifier.NONE, null));
			list.Add (new Parameter (
				TypeManager.system_boolean_expr, "initialized",
				Parameter.Modifier.NONE, null));

			Parameter[] old_fixed = parameters.Parameters.FixedParameters;
			if (old_fixed != null)
				list.AddRange (old_fixed);

			Parameter[] fixed_params = new Parameter [list.Count];
			list.CopyTo (fixed_params);

			ctor_params = new Parameters (
				fixed_params, parameters.Parameters.ArrayParameter,
				Location);

			ctor = new Constructor (
				this, Name, Modifiers.PUBLIC, ctor_params,
				new ConstructorBaseInitializer (null, Location),
				Location);
			AddConstructor (ctor);

			ctor.Block = new ToplevelBlock (block, parameters.Parameters, Location);

			int first = IsStatic ? 2 : 3;

			State initial = is_enumerable ? State.Uninitialized : State.Running;
			ctor.Block.AddStatement (new SetState (this, initial, Location));

			ctor.Block.AddStatement (new If (
				new SimpleParameterReference (
					TypeManager.bool_type, first - 1, Location),
				new SetState (this, State.Running, Location),
				Location));

			ctor.Block.AddStatement (new InitScope (this, cc, Location));
		}

		Statement Create_ThrowInvalidOperation ()
		{
			TypeExpr ex_type = new TypeExpression (
				TypeManager.invalid_operation_exception_type, Location);

			return new Throw (new New (ex_type, null, Location), Location);
		}

		Statement Create_ThrowNotSupported ()
		{
			TypeExpr ex_type = new TypeExpression (
				TypeManager.not_supported_exception_type, Location);

			return new Throw (new New (ex_type, null, Location), Location);
		}

		void Define_Current (bool is_generic)
		{
			MemberName left;
			Expression type;
			if (is_generic) {
				left = new MemberName (
					"System.Collections.Generic.IEnumerator",
					generic_args);
				type = iterator_type_expr;
			} else {
				left = new MemberName ("System.Collections.IEnumerator");
				type = TypeManager.system_object_expr;
			}

			MemberName name = new MemberName (left, "Current", null);

			ToplevelBlock get_block = new ToplevelBlock (
				block, parameters.Parameters, Location);

			get_block.AddStatement (new If (
				new Binary (
					Binary.Operator.LessThanOrEqual,
					new FieldExpression (this, pc_field),
					new IntLiteral ((int) State.Running), Location),
				Create_ThrowInvalidOperation (),
				new Return (
					new FieldExpression (this, current_field), Location),
				Location));

			Accessor getter = new Accessor (get_block, 0, null, Location);

			Property current = new Property (
				this, type, 0, false, name, null, getter, null, Location);
			AddProperty (current);
		}

		void Define_MoveNext ()
		{
			move_next_method = new MoveNextMethod (this, Location);

			original_block.ReParent (block, move_next_method);

			move_next_method.CreateMethod (ec);

			AddMethod (move_next_method.method);
		}

		void Define_GetEnumerator (bool is_generic)
		{
			MemberName left;
			Expression type;
			if (is_generic) {
				left = new MemberName (
					"System.Collections.Generic.IEnumerable",
					generic_args);
				type = generic_enumerator_type;
			} else {
				left = new MemberName ("System.Collections.IEnumerable");
				type = enumerator_type;
			}

			MemberName name = new MemberName (left, "GetEnumerator", null);

			Method get_enumerator = new Method (
				this, null, type, 0, false, name,
				Parameters.EmptyReadOnlyParameters, null,
				Location.Null);
			AddMethod (get_enumerator);

			get_enumerator.Block = new ToplevelBlock (
				block, parameters.Parameters, Location);

			get_enumerator.Block.SetHaveAnonymousMethods (Location, move_next_method);

			Expression ce = new MemberAccess (
				new SimpleName ("System.Threading.Interlocked", Location),
				"CompareExchange", Location);

			Expression pc = new FieldExpression (this, pc_field);
			Expression before = new IntLiteral ((int) State.Running);
			Expression uninitialized = new IntLiteral ((int) State.Uninitialized);

			ArrayList args = new ArrayList ();
			args.Add (new Argument (pc, Argument.AType.Ref));
			args.Add (new Argument (before, Argument.AType.Expression));
			args.Add (new Argument (uninitialized, Argument.AType.Expression));

			get_enumerator.Block.AddStatement (new If (
				new Binary (
					Binary.Operator.Equality,
					new Invocation (ce, args, Location),
					uninitialized, Location),
				new Return (new ThisParameterReference (type.Type, Location),
					    Location),
				Location));

			args = new ArrayList ();
			if (!IsStatic) {
				args.Add (new Argument (new CapturedThisReference (this, Location)));
			}

			args.Add (new Argument (new BoolLiteral (true)));

			for (int i = 0; i < parameters.Count; i++) {
				Expression cp = new CapturedParameterReference (
					this, parameters.ParameterType (i),
					parameters.ParameterName (i), Location);
				args.Add (new Argument (cp));
			}

			Expression new_expr = new New (current_type, args, Location);
			get_enumerator.Block.AddStatement (new Return (new_expr, Location));
		}

		protected class SimpleParameterReference : Expression
		{
			int idx;

			public SimpleParameterReference (Type type, int idx, Location loc)
			{
				this.idx = idx;
				this.loc = loc;
				this.type = type;
				eclass = ExprClass.Variable;
			}

			public override Expression DoResolve (EmitContext ec)
			{
				return this;
			}

			public override void Emit (EmitContext ec)
			{
				DoEmit (ec);
			}

			protected virtual void DoEmit (EmitContext ec)
			{
				ParameterReference.EmitLdArg (ec.ig, idx);
			}
		}

		protected class ThisParameterReference : SimpleParameterReference, IMemoryLocation
		{
			public ThisParameterReference (Type type, Location loc)
				: base (type, 0, loc)
			{ }

			protected override void DoEmit (EmitContext ec)
			{
				base.DoEmit (ec);
				if (ec.TypeContainer is Struct)
					ec.ig.Emit (OpCodes.Ldobj, type);
			}

			public void AddressOf (EmitContext ec, AddressOp mode)
			{
				if (ec.TypeContainer is Struct)
					ec.ig.Emit (OpCodes.Ldarga, 0);
				else
					ec.ig.Emit (OpCodes.Ldarg, 0);
			}
		}

		protected class CapturedParameterReference : Expression
		{
			Iterator iterator;
			string name;

			public CapturedParameterReference (Iterator iterator, Type type,
							   string name, Location loc)
			{
				this.iterator = iterator;
				this.loc = loc;
				this.type = type;
				this.name = name;
				eclass = ExprClass.Variable;
			}

			public override Expression DoResolve (EmitContext ec)
			{
				return this;
			}

			public override void Emit (EmitContext ec)
			{
				ec.CurrentAnonymousMethod = iterator.move_next_method;

				iterator.cc.EmitParameter (ec, name);
			}
		}

		protected class CapturedThisReference : Expression
		{
			Iterator iterator;

			public CapturedThisReference (Iterator iterator, Location loc)
			{
				this.iterator = iterator;
				this.loc = loc;
				this.type = iterator.this_type;
				eclass = ExprClass.Variable;
			}

			public override Expression DoResolve (EmitContext ec)
			{
				return this;
			}

			public override void Emit (EmitContext ec)
			{
				ec.ig.Emit (OpCodes.Ldarg_0);
				ec.ig.Emit (OpCodes.Ldfld, iterator.move_next_method.Scope.THIS);
			}
		}

		protected class FieldExpression : Expression
		{
			Iterator iterator;
			Field field;

			public FieldExpression (Iterator iterator, Field field)
			{
				this.iterator = iterator;
				this.field = field;
				this.loc = iterator.Location;
			}

			public override Expression DoResolveLValue (EmitContext ec, Expression right_side)
			{
				FieldExpr fexpr = new FieldExpr (field.FieldBuilder, loc);
				fexpr.InstanceExpression = new ThisParameterReference (
					iterator.this_type, loc);
				return fexpr.ResolveLValue (ec, right_side);
			}

			public override Expression DoResolve (EmitContext ec)
			{
				FieldExpr fexpr = new FieldExpr (field.FieldBuilder, loc);
				fexpr.InstanceExpression = new ThisParameterReference (
					iterator.this_type, loc);
				return fexpr.Resolve (ec);
			}

			public override void Emit (EmitContext ec)
			{
				throw new InvalidOperationException ();
			}
		}

		protected class MoveNextMethod : AnonymousContainer
		{
			Iterator iterator;

			public MoveNextMethod (Iterator iterator, Location loc)
				: base (iterator.parameters.Parameters, iterator.original_block, loc)
			{
				this.iterator = iterator;
			}

			protected override bool CreateMethodHost (EmitContext ec)
			{
				method = new Method (
					iterator, null, TypeManager.system_boolean_expr,
					Modifiers.PUBLIC, false, new MemberName ("MoveNext"),
					Parameters.EmptyReadOnlyParameters, null, loc);

				method.Block = Block;

				MoveNextStatement inline = new MoveNextStatement (iterator, loc);
				Block.AddStatement (inline);

				return true;
			}

			public bool CreateMethod (EmitContext ec)
			{
				return CreateMethodHost (ec);
			}

			public override bool IsIterator {
				get { return true; }
			}

			public override void CreateScopeType (EmitContext ec, ScopeInfo scope)
			{
				scope.ScopeTypeBuilder = iterator.TypeBuilder;
				scope.ScopeConstructor = iterator.ctor.ConstructorBuilder;
			}

			public override void Emit (EmitContext ec)
			{
				throw new InternalErrorException ();
			}
		}

		protected class MoveNextStatement : Statement {
			Iterator iterator;

			public MoveNextStatement (Iterator iterator, Location loc)
			{
				this.loc = loc;
				this.iterator = iterator;
			}

			public override bool Resolve (EmitContext ec)
			{
				return true;
			}

			protected override void DoEmit (EmitContext ec)
			{
				ec.CurrentIterator = iterator;
				ec.CurrentAnonymousMethod = iterator.move_next_method;
				ec.InIterator = true;

				iterator.EmitMoveNext (ec);
			}
		}

		protected class DisposeMethod : Statement {
			Iterator iterator;

			public DisposeMethod (Iterator iterator, Location loc)
			{
				this.loc = loc;
				this.iterator = iterator;
			}

			public override bool Resolve (EmitContext ec)
			{
				return true;
			}

			protected override void DoEmit (EmitContext ec)
			{
				iterator.EmitDispose (ec);
			}
		}

		protected class StatementList : Statement {
			ArrayList statements;

			public StatementList (Location loc)
			{
				this.loc = loc;
				statements = new ArrayList ();
			}

			public void Add (Statement statement)
			{
				statements.Add (statement);
			}

			public override bool Resolve (EmitContext ec)
			{
				foreach (Statement stmt in statements) {
					if (!stmt.Resolve (ec))
						return false;
				}

				return true;
			}

			protected override void DoEmit (EmitContext ec)
			{
				foreach (Statement stmt in statements)
					stmt.Emit (ec);
			}
		}

		protected class SetState : Statement
		{
			Iterator iterator;
			State state;

			public SetState (Iterator iterator, State state, Location loc)
			{
				this.iterator = iterator;
				this.state = state;
				this.loc = loc;
			}

			public override bool Resolve (EmitContext ec)
			{
				return true;
			}

			protected override void DoEmit (EmitContext ec)
			{
				ec.ig.Emit (OpCodes.Ldarg_0);
				IntConstant.EmitInt (ec.ig, (int) state);
				ec.ig.Emit (OpCodes.Stfld, iterator.pc_field.FieldBuilder);
			}
		}

		protected class InitScope : Statement
		{
			Iterator iterator;
			CaptureContext cc;

			public InitScope (Iterator iterator, CaptureContext cc, Location loc)
			{
				this.iterator = iterator;
				this.cc = cc;
				this.loc = loc;
			}

			public override bool Resolve (EmitContext ec)
			{
				return true;
			}

			protected override void DoEmit (EmitContext ec)
			{
				iterator.cc.EmitInitScope (ec);
			}
		}

		void Define_Reset ()
		{
			Method reset = new Method (
				this, null, TypeManager.system_void_expr, Modifiers.PUBLIC,
				false, new MemberName ("Reset"),
				Parameters.EmptyReadOnlyParameters, null, Location);
			AddMethod (reset);

			reset.Block = new ToplevelBlock (Location);
			reset.Block = new ToplevelBlock (block, parameters.Parameters, Location);
			reset.Block.SetHaveAnonymousMethods (Location, move_next_method);

			reset.Block.AddStatement (Create_ThrowNotSupported ());
		}

		void Define_Dispose ()
		{
			dispose = new Method (
				this, null, TypeManager.system_void_expr, Modifiers.PUBLIC,
				false, new MemberName ("Dispose"),
				Parameters.EmptyReadOnlyParameters, null, Location);
			AddMethod (dispose);

			dispose.Block = new ToplevelBlock (block, parameters.Parameters, Location);
			dispose.Block.SetHaveAnonymousMethods (Location, move_next_method);

			dispose.Block.AddStatement (new DisposeMethod (this, Location));
		}

		public ToplevelBlock Block {
			get { return block; }
		}

		public Type IteratorType {
			get { return iterator_type; }
		}

		//
		// This return statement tricks return into not flagging an error for being
		// used in a Yields method
		//
		class NoCheckReturn : Statement {
			public Expression Expr;
		
			public NoCheckReturn (Expression expr, Location l)
			{
				Expr = expr;
				loc = l;
			}

			public override bool Resolve (EmitContext ec)
			{
				Expr = Expr.Resolve (ec);
				if (Expr == null)
					return false;

				FlowBranching.UsageVector vector = ec.CurrentBranching.CurrentUsageVector;
				ec.CurrentBranching.CurrentUsageVector.Return ();

				return true;
			}

			protected override void DoEmit (EmitContext ec)
			{
				Expr.Emit (ec);
				ec.ig.Emit (OpCodes.Ret);
			}
		}

		bool CheckType (Type t)
		{
			if (t == TypeManager.ienumerable_type) {
				iterator_type = TypeManager.object_type;
				is_enumerable = true;
				return true;
			} else if (t == TypeManager.ienumerator_type) {
				iterator_type = TypeManager.object_type;
				is_enumerable = false;
				return true;
			}

			if (!t.IsGenericInstance)
				return false;

			Type[] args = TypeManager.GetTypeArguments (t);
			if (args.Length != 1)
				return false;

			Type gt = t.GetGenericTypeDefinition ();
			if (gt == TypeManager.generic_ienumerable_type) {
				iterator_type = args [0];
				is_enumerable = true;
				return true;
			} else if (gt == TypeManager.generic_ienumerator_type) {
				iterator_type = args [0];
				is_enumerable = false;
				return true;
			}

			return false;
		}
	}
}

