//
// statement.cs: Statement representation for the IL tree.
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Mono.CSharp {

	using System.Collections;
	
	public abstract class Statement {

		/// <summary>
		///   Return value indicates whether all code paths emitted return.
		/// </summary>
		public abstract bool Emit (EmitContext ec);

		/// <remarks>
		///    Emits a bool expression.
		/// </remarks>
		public static bool EmitBoolExpression (EmitContext ec, Expression e, Label l, bool isTrue)
		{
			e = e.Resolve (ec);

			if (e == null)
				return false;

			if (e.Type != TypeManager.bool_type)
				e = Expression.ConvertImplicit (ec, e, TypeManager.bool_type,
								new Location (-1));

			if (e == null){
				Report.Error (
					31, "Can not convert the expression to a boolean");
				return false;
			}

			bool invert = false;
			if (e is Unary){
				Unary u = (Unary) e;
				
				if (u.Oper == Unary.Operator.LogicalNot){
					invert = true;

					u.EmitLogicalNot (ec);
				}
			} 

			if (!invert)
				e.Emit (ec);

			if (isTrue){
				if (invert)
					ec.ig.Emit (OpCodes.Brfalse, l);
				else
					ec.ig.Emit (OpCodes.Brtrue, l);
			} else {
				if (invert)
					ec.ig.Emit (OpCodes.Brtrue, l);
				else
					ec.ig.Emit (OpCodes.Brfalse, l);
			}
			
			return true;
		}

	}

	public class EmptyStatement : Statement {
		public override bool Emit (EmitContext ec)
		{
			return false;
		}
	}
	
	public class If : Statement {
		public readonly Expression  Expr;
		public readonly Statement   TrueStatement;
		public readonly Statement   FalseStatement;
		
		public If (Expression expr, Statement trueStatement)
		{
			Expr = expr;
			TrueStatement = trueStatement;
		}

		public If (Expression expr,
			   Statement trueStatement,
			   Statement falseStatement)
		{
			Expr = expr;
			TrueStatement = trueStatement;
			FalseStatement = falseStatement;
		}

		public override bool Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label false_target = ig.DefineLabel ();
			Label end;
			bool is_true_ret, is_false_ret;
			
			if (!EmitBoolExpression (ec, Expr, false_target, false))
				return false;
			
			is_true_ret = TrueStatement.Emit (ec);
			is_false_ret = is_true_ret;

			if (FalseStatement != null){
				bool branch_emitted = false;
				
				end = ig.DefineLabel ();
				if (!is_true_ret){
					ig.Emit (OpCodes.Br, end);
					branch_emitted = true;
				}
			
				ig.MarkLabel (false_target);
				is_false_ret = FalseStatement.Emit (ec);

				if (branch_emitted)
					ig.MarkLabel (end);
			} else {
				ig.MarkLabel (false_target);
				is_false_ret = false;
			}

			return is_true_ret && is_false_ret;
		}
	}

	public class Do : Statement {
		public readonly Expression Expr;
		public readonly Statement  EmbeddedStatement;
		
		public Do (Statement statement, Expression boolExpr)
		{
			Expr = boolExpr;
			EmbeddedStatement = statement;
		}

		public override bool Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label loop = ig.DefineLabel ();
			Label old_begin = ec.LoopBegin;
			Label old_end = ec.LoopEnd;
			bool  old_inloop = ec.InLoop;
			
			ec.LoopBegin = ig.DefineLabel ();
			ec.LoopEnd = ig.DefineLabel ();
			ec.InLoop = true;
				
			ig.MarkLabel (loop);
			EmbeddedStatement.Emit (ec);
			ig.MarkLabel (ec.LoopBegin);
			EmitBoolExpression (ec, Expr, loop, true);
			ig.MarkLabel (ec.LoopEnd);

			ec.LoopBegin = old_begin;
			ec.LoopEnd = old_end;
			ec.InLoop = old_inloop;
			
			return false;
		}
	}

	public class While : Statement {
		public readonly Expression Expr;
		public readonly Statement Statement;
		
		public While (Expression boolExpr, Statement statement)
		{
			Expr = boolExpr;
			Statement = statement;
		}

		public override bool Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label old_begin = ec.LoopBegin;
			Label old_end = ec.LoopEnd;
			bool old_inloop = ec.InLoop;
			
			ec.LoopBegin = ig.DefineLabel ();
			ec.LoopEnd = ig.DefineLabel ();
			ec.InLoop = true;
			
			ig.MarkLabel (ec.LoopBegin);
			EmitBoolExpression (ec, Expr, ec.LoopEnd, false);
			Statement.Emit (ec);
			ig.Emit (OpCodes.Br, ec.LoopBegin);
			ig.MarkLabel (ec.LoopEnd);

			ec.LoopBegin = old_begin;
			ec.LoopEnd = old_end;
			ec.InLoop = old_inloop;
			
			return false;
		}
	}

	public class For : Statement {
		public readonly Statement InitStatement;
		public readonly Expression Test;
		public readonly Statement Increment;
		public readonly Statement Statement;
		
		public For (Statement initStatement,
			    Expression test,
			    Statement increment,
			    Statement statement)
		{
			InitStatement = initStatement;
			Test = test;
			Increment = increment;
			Statement = statement;
		}

		public override bool Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label old_begin = ec.LoopBegin;
			Label old_end = ec.LoopEnd;
			bool old_inloop = ec.InLoop;
			Label loop = ig.DefineLabel ();

			if (InitStatement != null)
				if (! (InitStatement is EmptyStatement))
					InitStatement.Emit (ec);

			ec.LoopBegin = ig.DefineLabel ();
			ec.LoopEnd = ig.DefineLabel ();
			ec.InLoop = true;

			ig.MarkLabel (loop);
			EmitBoolExpression (ec, Test, ec.LoopEnd, false);
			Statement.Emit (ec);
			ig.MarkLabel (ec.LoopBegin);
			if (!(Increment is EmptyStatement))
				Increment.Emit (ec);
			ig.Emit (OpCodes.Br, loop);
			ig.MarkLabel (ec.LoopEnd);

			ec.LoopBegin = old_begin;
			ec.LoopEnd = old_end;
			ec.InLoop = old_inloop;
			return false;
		}
	}
	
	public class StatementExpression : Statement {
		public readonly ExpressionStatement Expr;
		
		public StatementExpression (ExpressionStatement expr)
		{
			Expr = expr;
		}

		public override bool Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Expression ne;
			
			ne = Expr.Resolve (ec);
			if (ne != null){
				if (ne is ExpressionStatement)
					((ExpressionStatement) ne).EmitStatement (ec);
				else {
					ne.Emit (ec);
					ig.Emit (OpCodes.Pop);
				}
			}

			return false;
		}

		public override string ToString ()
		{
			return "StatementExpression (" + Expr + ")";
		}
	}

	public class Return : Statement {
		public Expression Expr;
		public readonly Location loc;
		
		public Return (Expression expr, Location l)
		{
			Expr = expr;
			loc = l;
		}

		public override bool Emit (EmitContext ec)
		{
			if (ec.ReturnType == null){
				if (Expr != null){
					Report.Error (127, loc, "Return with a value not allowed here");
					return false;
				}
			} else {
				if (Expr == null){
					Report.Error (126, loc, "An object of type `" +
						      TypeManager.CSharpName (ec.ReturnType) + "' is " +
						      "expected for the return statement");
					return false;
				}

				Expr = Expr.Resolve (ec);
				if (Expr == null)
					return false;

				if (Expr.Type != ec.ReturnType)
					Expr = Expression.ConvertImplicitRequired (
						ec, Expr, ec.ReturnType, loc);

				if (Expr == null)
					return false;

				Expr.Emit (ec);
			}

			ec.ig.Emit (OpCodes.Ret);

			return true; 
		}
	}

	public class Goto : Statement {
		string target;
		Location loc;
			
		public Goto (string label, Location l)
		{
			loc = l;
			target = label;
		}

		public string Target {
			get {
				return target;
			}
		}

		public override bool Emit (EmitContext ec)
		{
			Console.WriteLine ("Attempting to goto to: " + target);
			
			return false;
		}
	}

	/// <summary>
	///   `goto default' statement
	/// </summary>
	public class GotoDefault : Statement {
		Location loc;
		
		public GotoDefault (Location l)
		{
			loc = l;
		}

		public override bool Emit (EmitContext ec)
		{
			if (!ec.InSwitch){
				Report.Error (153, loc, "goto default is only valid in a switch statement");
				return false;
			}

			if (!ec.GotDefault){
				Report.Error (159, loc, "No default target on switch statement");
				return false;
			}
			ec.ig.Emit (OpCodes.Br, ec.DefaultTarget);
			return false;
		}
	}

	/// <summary>
	///   `goto case' statement
	/// </summary>
	public class GotoCase : Statement {
		Location loc;
		Expression expr;
		
		public GotoCase (Expression e, Location l)
		{
			expr = e;
			loc = l;
		}

		public override bool Emit (EmitContext ec)
		{
			if (!ec.InSwitch){
				Report.Error (153, loc, "goto case is only valid in a switch statement");
				return false;
			}

			expr = expr.Resolve (ec);
			if (expr == null)
				return false;

			expr = Expression.Reduce (ec, expr);
			if (!(expr is Literal)){
				Report.Error (159, loc, "Target expression for goto case is not constant");
				return false;
			}
			
			// FIXME: implement me.

			throw new Exception ("FIXME: IMPLEMENT ME");
		}
	}
	
	public class Throw : Statement {
		public readonly Expression Expr;
		
		public Throw (Expression expr)
		{
			Expr = expr;
		}

		public override bool Emit (EmitContext ec)
		{
			Expression e = Expr.Resolve (ec);

			if (e == null)
				return false;

			e.Emit (ec);
			ec.ig.Emit (OpCodes.Throw);

			return true;
		}
	}

	public class Break : Statement {
		Location loc;
		
		public Break (Location l)
		{
			loc = l;
		}

		public override bool Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (!(ec.InLoop || ec.InSwitch)){
				Report.Error (139, loc, "No enclosing loop or switch to continue to");
				return false;
			}
			
			ig.Emit (OpCodes.Br, ec.LoopEnd);
			return false;
		}
	}

	public class Continue : Statement {
		Location loc;
		
		public Continue (Location l)
		{
			loc = l;
		}

		public override bool Emit (EmitContext ec)
		{
			Label begin = ec.LoopBegin;
			
			if (!ec.InLoop){
				Report.Error (139, loc, "No enclosing loop to continue to");
				return false;
			} 

			ec.ig.Emit (OpCodes.Br, begin);
			return false;
		}
	}
	
	public class VariableInfo {
		public readonly string Type;
		public LocalBuilder LocalBuilder;
		public Type VariableType;
		public readonly Location Location;
		
		int  idx;
		public bool Used;
		public bool Assigned;

		public VariableInfo (string type, Location l)
		{
			Type = type;
			LocalBuilder = null;
			idx = -1;
			Location = l;
		}

		public int Idx {
			get {
				if (idx == -1)
					throw new Exception ("Unassigned idx for variable");
				
				return idx;
			}

			set {
				idx = value;
			}
		}

	}
		
	/// <summary>
	///   Block represents a C# block.
	/// </summary>
	///
	/// <remarks>
	///   This class is used in a number of places: either to represent
	///   explicit blocks that the programmer places or implicit blocks.
	///
	///   Implicit blocks are used as labels or to introduce variable
	///   declarations.
	/// </remarks>
	public class Block : Statement {
		public readonly Block  Parent;
		public readonly bool   Implicit;
		public readonly string Label;

		//
		// The statements in this block
		//
		StatementCollection statements;

		//
		// An array of Blocks.  We keep track of children just
		// to generate the local variable declarations.
		//
		// Statements and child statements are handled through the
		// statements.
		//
		ArrayList children;
		
		//
		// Labels.  (label, block) pairs.
		//
		Hashtable labels;

		//
		// Keeps track of (name, type) pairs
		//
		Hashtable variables;

		//
		// Keeps track of constants
		Hashtable constants;

		//
		// Maps variable names to ILGenerator.LocalBuilders
		//
		Hashtable local_builders;

		bool used = false;

		static int id;

		int this_id;
		
		public Block (Block parent)
		{
			if (parent != null)
				parent.AddChild (this);
			
			this.Parent = parent;
			this.Implicit = false;

			this_id = id++;
		}

		public Block (Block parent, bool implicit_block)
		{
			if (parent != null)
				parent.AddChild (this);
			
			this.Parent = parent;
			this.Implicit = true;
			this_id = id++;
		}

		public Block (Block parent, string labeled)
		{
			if (parent != null)
				parent.AddChild (this);
			
			this.Parent = parent;
			this.Implicit = true;
			Label = labeled;
			this_id = id++;
		}

		public int ID {
			get {
				return this_id;
			}
		}
		
		void AddChild (Block b)
		{
			if (children == null)
				children = new ArrayList ();
			
			children.Add (b);
		}

		/// <summary>
		///   Adds a label to the current block. 
		/// </summary>
		///
		/// <returns>
		///   false if the name already exists in this block. true
		///   otherwise.
		/// </returns>
		///
		public bool AddLabel (string name, Block block)
		{
			if (labels == null)
				labels = new Hashtable ();
			if (labels.Contains (name))
				return false;
			
			labels.Add (name, block);
			return true;
		}

		public bool AddVariable (string type, string name, Location l)
		{
			if (variables == null)
				variables = new Hashtable ();

			if (GetVariableType (name) != null)
				return false;

			VariableInfo vi = new VariableInfo (type, l);

			variables.Add (name, vi);

			return true;
		}

		public bool AddConstant (string type, string name, Expression value, Location l)
		{
			if (!AddVariable (type, name, l))
				return false;
			
			if (constants == null)
				constants = new Hashtable ();

			constants.Add (name, value);

			return true;
		}

		public Hashtable Variables {
			get {
				return variables;
			}
		}

		public VariableInfo GetVariableInfo (string name)
		{
			if (variables != null) {
				object temp;
				temp = variables [name];

				if (temp != null){
					return (VariableInfo) temp;
				}
			}

			if (Parent != null)
				return Parent.GetVariableInfo (name);

			return null;
		}
		
		public string GetVariableType (string name)
		{
			VariableInfo vi = GetVariableInfo (name);

			if (vi != null)
				return vi.Type;

			return null;
		}

		public Expression GetConstantExpression (string name)
		{
			if (constants != null) {
				object temp;
				temp = constants [name];
				
				if (temp != null)
					return (Expression) temp;
			}
			
			if (Parent != null)
				return Parent.GetConstantExpression (name);

			return null;
		}
		
		/// <summary>
		///   True if the variable named @name has been defined
		///   in this block
		/// </summary>
		public bool IsVariableDefined (string name)
		{
			if (variables != null) {
				if (variables.Contains (name))
					return true;
			}
			
			if (Parent != null)
				return Parent.IsVariableDefined (name);

			return false;
		}

		/// <summary>
		///   True if the variable named @name is a constant
		///  </summary>
		public bool IsConstant (string name)
		{
			Expression e = null;
			
			e = GetConstantExpression (name);
			
			return e != null;
		}
		
		/// <summary>
		///   Use to fetch the statement associated with this label
		/// </summary>
		public Statement this [string name] {
			get {
				return (Statement) labels [name];
			}
		}

		/// <returns>
		///   A list of labels that were not used within this block
		/// </returns>
		public string [] GetUnreferenced ()
		{
			// FIXME: Implement me
			return null;
		}

		public StatementCollection Statements {
			get {
				if (statements == null)
					statements = new StatementCollection ();

				return statements;
			}
		}

		public void AddStatement (Statement s)
		{
			if (statements == null)
				statements = new StatementCollection ();

			statements.Add (s);
			used = true;
		}

		public bool Used {
			get {
				return used;
			}
		}

		public void Use ()
		{
			used = true;
		}
		
		/// <summary>
		///   Emits the variable declarations and labels.
		/// </summary>
		/// <remarks>
		///   tc: is our typecontainer (to resolve type references)
		///   ig: is the code generator:
		///   toplevel: the toplevel block.  This is used for checking 
		///   		that no two labels with the same name are used.
		/// </remarks>
		public void EmitMeta (TypeContainer tc, ILGenerator ig, Block toplevel, int count)
		{
			//
			// Process this block variables
			//
			if (variables != null){
				local_builders = new Hashtable ();
				
				foreach (DictionaryEntry de in variables){
					string name = (string) de.Key;
					VariableInfo vi = (VariableInfo) de.Value;
					Type t;
					
					t = tc.LookupType (vi.Type, false);
					if (t == null)
						continue;

					vi.VariableType = t;
					vi.LocalBuilder = ig.DeclareLocal (t);
					vi.Idx = count++;
				}
			}

			//
			// Now, handle the children
			//
			if (children != null){
				foreach (Block b in children)
					b.EmitMeta (tc, ig, toplevel, count);
			}
		}

		public void UsageWarning ()
		{
			string name;
			
			if (variables != null){
				foreach (DictionaryEntry de in variables){
					VariableInfo vi = (VariableInfo) de.Value;
					
					if (vi.Used)
						continue;
					
					name = (string) de.Key;
						
					if (vi.Assigned){
						Report.Warning (
							219, vi.Location, "The variable `" + name +
							"' is assigned but its value is never used");
					} else {
						Report.Warning (
							168, vi.Location, "The variable `" +
							name +
							"' is declared but never used");
					} 
				}
			}

			if (children != null)
				foreach (Block b in children)
					b.UsageWarning ();
		}

		public override bool Emit (EmitContext ec)
		{
			bool is_ret = false;
			Block prev_block = ec.CurrentBlock;

			ec.CurrentBlock = this;
			foreach (Statement s in Statements)
				is_ret = s.Emit (ec);

			ec.CurrentBlock = prev_block;
			return is_ret;
		}
	}

	public class SwitchLabel {
		Expression label;
		Literal converted;
		public Location loc;
		
		//
		// if expr == null, then it is the default case.
		//
		public SwitchLabel (Expression expr, Location l)
		{
			label = expr;
			loc = l;
		}

		public Expression Label {
			get {
				return label;
			}
		}

		public Literal Converted {
			get {
				return converted;
			}
		}
		
		//
		// Resolves the expression, reduces it to a literal if possible
		// and then converts it to the requested type.
		//
		public bool ResolveAndReduce (EmitContext ec, Type required_type)
		{
			if (label == null)
				return true;
			
			Expression e = label.Resolve (ec);

			if (e == null)
				return false;

			label = e.Reduce (ec);

			if (!(label is Literal)){
				Console.WriteLine ("Value is: " + label);
				Report.Error (150, loc,
					      "A constant value is expected");
				return false;
			}

			if (label is StringLiteral || label is NullLiteral){
				if (required_type == TypeManager.string_type){
					converted = (Literal) label;
					return true;
				}
			}

			converted = Expression.ConvertIntLiteral ((Literal) label, required_type, loc);
			if (converted == null)
				return false;
			
			return true;
		}
	}

	public class SwitchSection {
		// An array of SwitchLabels.
		public readonly ArrayList Labels;
		public readonly Block Block;
		
		public SwitchSection (ArrayList labels, Block block)
		{
			Labels = labels;
			Block = block;
		}
	}
	
	public class Switch : Statement {
		ArrayList sections;
		Expression expr;
		Location loc;

		//
		// The types allowed to be implicitly cast from
		// on the governing type
		//
		static Type [] allowed_types;
		
		public Switch (Expression e, ArrayList sects, Location l)
		{
			expr = e;
			sections = sects;
			loc = l;
		}

		public Expression Expr {
			get {
				return expr;
			}
		}

		public ArrayList Sections {
			get {
				return sections;
			}
		}

		//
		// Determines the governing type for a switch.  The returned
		// expression might be the expression from the switch, or an
		// expression that includes any potential conversions to the
		// integral types or to string.
		//
		Expression SwitchGoverningType (EmitContext ec, Type t)
		{
			if (t == TypeManager.int32_type ||
			    t == TypeManager.uint32_type ||
			    t == TypeManager.char_type ||
			    t == TypeManager.byte_type ||
			    t == TypeManager.sbyte_type ||
			    t == TypeManager.ushort_type ||
			    t == TypeManager.short_type ||
			    t == TypeManager.uint64_type ||
			    t == TypeManager.int64_type ||
			    t == TypeManager.string_type ||
			    t.IsSubclassOf (TypeManager.enum_type))
				return expr;

			if (allowed_types == null){
				allowed_types = new Type [] {
					TypeManager.sbyte_type,
					TypeManager.byte_type,
					TypeManager.short_type,
					TypeManager.ushort_type,
					TypeManager.int32_type,
					TypeManager.uint32_type,
					TypeManager.int64_type,
					TypeManager.uint64_type,
					TypeManager.char_type,
					TypeManager.string_type
				};
			}

			//
			// Try to find a *user* defined implicit conversion.
			//
			// If there is no implicit conversion, or if there are multiple
			// conversions, we have to report an error
			//
			Expression converted = null;
			foreach (Type tt in allowed_types){
				Expression e;
				
				e = Expression.ImplicitUserConversion (ec, expr, tt, loc);
				if (e == null)
					continue;

				if (converted != null){
					Report.Error (-12, loc, "More than one conversion to an integral " +
						      " type exists for type `" +
						      TypeManager.CSharpName (expr.Type)+"'");
					return null;
				}
			}
			return converted;
		}

		void error152 (string n)
		{
			Report.Error (
				152, "The label `" + n + ":' " +
				"is already present on this switch statement");
		}
		
		//
		// Performs the basic sanity checks on the switch statement
		// (looks for duplicate keys and non-constant expressions).
		//
		// It also returns a hashtable with the keys that we will later
		// use to compute the switch tables
		//
		Hashtable CheckSwitch (EmitContext ec, Type switch_type)
		{
			bool error = false;
			Hashtable elements = new Hashtable ();
				
			ec.GotDefault = false;
			foreach (SwitchSection ss in sections){
				foreach (SwitchLabel sl in ss.Labels){
					if (sl.Label == null){
						if (ec.GotDefault){
							error152 ("default");
							error = true;
						}
						ec.GotDefault = true;
						continue;
					}
					
					if (!sl.ResolveAndReduce (ec, switch_type)){
						error = true;
						continue;
					}

					Literal key = sl.Converted;

					//
					// We can do this because ConvertIntLiteral
					// will only return the following 4 kinds literals and
					// they will be consistent. 
					//
					string lname = null;
					if (switch_type == TypeManager.uint64_type){
						ulong v = ((ULongLiteral) key).Value;

						if (elements.Contains (v))
							lname = v.ToString ();
						else
							elements.Add (v, key);
					} else if (switch_type == TypeManager.int64_type){
						long v = ((LongLiteral) key).Value;

						if (elements.Contains (v))
							lname = v.ToString ();
						else
							elements.Add (v, key);
					} else if (switch_type == TypeManager.uint32_type){
						uint v = ((UIntLiteral) key).Value;

						if (elements.Contains (v))
							lname = v.ToString ();
						else
							elements.Add (v, key);
					} else if (switch_type == TypeManager.string_type){
						if (key is NullLiteral){
							if (elements.Contains (NullLiteral.Null))
								lname = "null";
							else
								elements.Add (NullLiteral.Null, null);
						} else {
							string s;
							
							s = ((StringLiteral) key).Value;

							if (elements.Contains (s))
								lname = s;
							else
								elements.Add (s, key);
						}
					} else {
						int v = ((IntLiteral) key).Value;

						if (elements.Contains (v))
							lname = v.ToString ();
						else
							elements.Add (v, key);
					}

					if (lname != null){
						error152 ("case + " + lname);
						error = true;
					}
				}
			}
			if (error)
				return null;
			
			return elements;
		}

		//
		// This simple emit switch works, but does not take advantage of the
		// `switch' opcode.  The swithc opcode uses a jump table that we are not
		// computing at this point
		//
		bool SimpleSwitchEmit (EmitContext ec, LocalBuilder val, Type switch_type, Hashtable el)
		{
			ILGenerator ig = ec.ig;
			Label end_of_switch = ig.DefineLabel ();
			Label next_test = ig.DefineLabel ();
			Label null_target = ig.DefineLabel ();
			bool default_found = false;
			bool first_test = true;
			bool pending_goto_end = false;
			bool all_return = true;
			bool is_string = false;
			bool null_found;
			
			//
			// Special processing for strings: we cant compare
			// against null.
			//
			if (switch_type == TypeManager.string_type){
				ig.Emit (OpCodes.Ldloc, val);
				is_string = true;
				
				if (el.Contains (NullLiteral.Null)){
					ig.Emit (OpCodes.Brfalse, null_target);
				} else
					ig.Emit (OpCodes.Brfalse, ec.DefaultTarget);

				ig.Emit (OpCodes.Ldloc, val);
				ig.Emit (OpCodes.Call, TypeManager.string_isinterneted_string);
				ig.Emit (OpCodes.Stloc, val);
			}
			
			foreach (SwitchSection ss in sections){
				Label sec_begin = ig.DefineLabel ();

				if (pending_goto_end)
					ig.Emit (OpCodes.Br, end_of_switch);

				int label_count = ss.Labels.Count;
				null_found = false;
				foreach (SwitchLabel sl in ss.Labels){
					if (!first_test){
						ig.MarkLabel (next_test);
						next_test = ig.DefineLabel ();
					}
					//
					// If we are the default target
					//
					if (sl.Label == null){
						ig.MarkLabel (ec.DefaultTarget);
						default_found = true;
					} else {
						Literal lit = sl.Converted;

						if (lit is NullLiteral){
							null_found = true;
							if (label_count == 1)
								ig.Emit (OpCodes.Br, next_test);
							continue;
									      
						}
						if (is_string){
							StringLiteral str = (StringLiteral) lit;

							ig.Emit (OpCodes.Ldloc, val);
							ig.Emit (OpCodes.Ldstr, str.Value);
							if (label_count == 1)
								ig.Emit (OpCodes.Bne_Un, next_test);
							else
								ig.Emit (OpCodes.Beq, sec_begin);
						} else {
							ig.Emit (OpCodes.Ldloc, val);
							lit.Emit (ec);
							ig.Emit (OpCodes.Ceq);
							if (label_count == 1)
								ig.Emit (OpCodes.Brfalse, next_test);
							else
								ig.Emit (OpCodes.Brtrue, sec_begin);
						}
					}
				}
				if (label_count != 1)
					ig.Emit (OpCodes.Br, next_test);
				
				if (null_found)
					ig.MarkLabel (null_target);
				ig.MarkLabel (sec_begin);
				if (ss.Block.Emit (ec))
					pending_goto_end = false;
				else {
					all_return = false;
					pending_goto_end = true;
				}
				first_test = false;
			}
			if (!default_found)
				ig.MarkLabel (ec.DefaultTarget);
			ig.MarkLabel (next_test);
			ig.MarkLabel (end_of_switch);
			
			return all_return;
		}
		
		public override bool Emit (EmitContext ec)
		{
			expr = expr.Resolve (ec);
			if (expr == null)
				return false;

			Expression new_expr = SwitchGoverningType (ec, expr.Type);
			if (new_expr == null){
				Report.Error (151, loc, "An integer type or string was expected for switch");
				return false;
			}

			// Validate switch.
			
			Type switch_type = new_expr.Type;
			Hashtable elements = CheckSwitch (ec, switch_type);
			
			if (elements == null)
				return false;

			ec.SwitchElements = elements;
			
			// Store variable for comparission purposes
			LocalBuilder value = ec.ig.DeclareLocal (switch_type);
			new_expr.Emit (ec);
			ec.ig.Emit (OpCodes.Stloc, value);

			ILGenerator ig = ec.ig;

			//
			// Setup the codegen context
			//
			Label old_end = ec.LoopEnd;
			Label old_default_target = ec.DefaultTarget;
			bool old_in_switch = ec.InSwitch;
			bool old_got_default = ec.GotDefault;
			Hashtable old_elements = ec.SwitchElements;
			
			ec.LoopEnd = ig.DefineLabel ();
			ec.DefaultTarget = ig.DefineLabel ();
			ec.InSwitch = true;

			// Emit Code.
			bool all_return =  SimpleSwitchEmit (ec, value, switch_type, elements);

			// Restore context state. 
			ig.MarkLabel (ec.LoopEnd);

			//
			// FIXME: I am emitting a nop, because the switch performs
			// no analysis on whether something ever reaches the end
			//
			// try: b (int a) { switch (a) { default: return 0; }  }
			ig.Emit (OpCodes.Nop);

			//
			// Restore the previous context
			//
			ec.InSwitch = old_in_switch;
			ec.LoopEnd = old_end;
			ec.DefaultTarget = old_default_target;
			ec.GotDefault = old_got_default;
			ec.SwitchElements = old_elements;
			
			//
			// Because we have a nop at the end
			//
			return false;
		}
	}

	public class Lock : Statement {
		public readonly Expression Expr;
		public readonly Statement Statement;
		Location loc;
			
		public Lock (Expression expr, Statement stmt, Location l)
		{
			Expr = expr;
			Statement = stmt;
			loc = l;
		}

		public override bool Emit (EmitContext ec)
		{
			Expression e = Expr.Resolve (ec);
			if (e == null)
				return false;

			Type type = e.Type;
			
			if (type.IsValueType){
				Report.Error (185, loc, "lock statement requires the expression to be " +
					      " a reference type (type is: `" +
					      TypeManager.CSharpName (type) + "'");
				return false;
			}

			ILGenerator ig = ec.ig;
			LocalBuilder temp = ig.DeclareLocal (type);
				
			e.Emit (ec);
			ig.Emit (OpCodes.Dup);
			ig.Emit (OpCodes.Stloc, temp);
			ig.Emit (OpCodes.Call, TypeManager.void_monitor_enter_object);

			// try
			Label end = ig.BeginExceptionBlock ();
			Label finish = ig.DefineLabel ();
			Statement.Emit (ec);
			// ig.Emit (OpCodes.Leave, finish);

			ig.MarkLabel (finish);
			
			// finally
			ig.BeginFinallyBlock ();
			ig.Emit (OpCodes.Ldloc, temp);
			ig.Emit (OpCodes.Call, TypeManager.void_monitor_exit_object);
			ig.EndExceptionBlock ();
			
			return false;
		}
	}

	public class Unchecked : Statement {
		public readonly Block Block;
		
		public Unchecked (Block b)
		{
			Block = b;
		}

		public override bool Emit (EmitContext ec)
		{
			bool previous_state = ec.CheckState;
			bool val;
			
			ec.CheckState = false;
			val = Block.Emit (ec);
			ec.CheckState = previous_state;

			return val;
		}
	}

	public class Checked : Statement {
		public readonly Block Block;
		
		public Checked (Block b)
		{
			Block = b;
		}

		public override bool Emit (EmitContext ec)
		{
			bool previous_state = ec.CheckState;
			bool val;
			
			ec.CheckState = true;
			val = Block.Emit (ec);
			ec.CheckState = previous_state;

			return val;
		}
	}

	public class Catch {
		public readonly string Type;
		public readonly string Name;
		public readonly Block  Block;
		
		public Catch (string type, string name, Block block)
		{
			Type = type;
			Name = name;
			Block = block;
		}
	}

	public class Try : Statement {
		public readonly Block Fini, Block;
		public readonly ArrayList Specific;
		public readonly Catch General;
		
		//
		// specific, general and fini might all be null.
		//
		public Try (Block block, ArrayList specific, Catch general, Block fini)
		{
			if (specific == null && general == null){
				Console.WriteLine ("CIR.Try: Either specific or general have to be non-null");
			}
			
			this.Block = block;
			this.Specific = specific;
			this.General = general;
			this.Fini = fini;
		}

		public override bool Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label end;
			Label finish = ig.DefineLabel ();;
			bool returns;
			
			end = ig.BeginExceptionBlock ();
			returns = Block.Emit (ec);
			ig.Emit (OpCodes.Leave, finish);
			
			foreach (Catch c in Specific){
				Type catch_type = ec.TypeContainer.LookupType (c.Type, false);
				VariableInfo vi;
				
				if (catch_type == null)
					return false;

				ig.BeginCatchBlock (catch_type);

				if (c.Name != null){
					vi = c.Block.GetVariableInfo (c.Name);
					if (vi == null){
						Console.WriteLine ("This should not happen! variable does not exist in this block");
						Environment.Exit (0);
					}
				
					ig.Emit (OpCodes.Stloc, vi.LocalBuilder);
				} else
					ig.Emit (OpCodes.Pop);
				
				c.Block.Emit (ec);
			}

			if (General != null){
				ig.BeginCatchBlock (TypeManager.object_type);
				ig.Emit (OpCodes.Pop);
			}

			ig.MarkLabel (finish);
			if (Fini != null){
				ig.BeginFinallyBlock ();
				Fini.Emit (ec);
			}
			
			ig.EndExceptionBlock ();

			//
			// FIXME: Is this correct?
			// Replace with `returns' and check test-18, maybe we can
			// perform an optimization here.
			//
			return false;
		}
	}

	public class Using : Statement {
		object expression_or_block;
		Statement Statement;
		Location loc;
		
		public Using (object expression_or_block, Statement stmt, Location l)
		{
			this.expression_or_block = expression_or_block;
			Statement = stmt;
			loc = l;
		}

		public override bool Emit (EmitContext ec)
		{
			//
			// Expressions are simple. 
			// The problem is with blocks, blocks might contain
			// more than one variable, ie like this:
			//
			// using (a = new X (), b = new Y ()) stmt;
			//
			// which is turned into:
			// using (a = new X ()) using (b = new Y ()) stmt;
			//
			// The trick is that the block will contain a bunch
			// of potential Assign expressions
			//
			//
			// We need to signal an error if a variable lacks
			// an assignment. (210).
			//
			// This is one solution.  Another is to set a flag
			// when we get the USING token, and have declare_local_variables
			// do something *different* that we can better cope with
			//
			throw new Exception ("Implement me!");
		}
	}

	/// <summary>
	///   Implementation of the foreach C# statement
	/// </summary>
	public class Foreach : Statement {
		string type;
		LocalVariableReference variable;
		Expression expr;
		Statement statement;
		Location loc;
		
		public Foreach (string type, LocalVariableReference var, Expression expr,
				Statement stmt, Location l)
		{
			this.type = type;
			this.variable = var;
			this.expr = expr;
			statement = stmt;
			loc = l;
		}

		static bool GetEnumeratorFilter (MemberInfo m, object criteria)
		{
			if (m == null)
				return false;
			
			if (!(m is MethodInfo))
				return false;
			
			if (m.Name != "GetEnumerator")
				return false;
			
			MethodInfo mi = (MethodInfo) m;
			
			if (mi.ReturnType != TypeManager.ienumerator_type)
				return false;
			
			Type [] args = TypeManager.GetArgumentTypes (mi);
			if (args == null)
				return true;
			
			if (args.Length == 0)
				return true;
			
			return false;
		}
		
		/// <summary>
		///   This filter is used to find the GetEnumerator method
		///   on which IEnumerator operates
		/// </summary>
		static MemberFilter FilterEnumerator;
		
		static Foreach ()
		{
			FilterEnumerator = new MemberFilter (GetEnumeratorFilter);
		}

                void error1579 (Type t)
                {
                        Report.Error (1579, loc,
                                      "foreach statement cannot operate on variables of type `" +
                                      t.FullName + "' because that class does not provide a " +
                                      " GetEnumerator method or it is inaccessible");
                }

		MethodInfo ProbeCollectionType (Type t)
		{
			MemberInfo [] mi;

			mi = TypeContainer.FindMembers (t, MemberTypes.Method,
							BindingFlags.Public | BindingFlags.Instance,
							FilterEnumerator, null);

			if (mi == null){
				error1579 (t);
				return null;
			}

			if (mi.Length == 0){
				error1579 (t);
				return null;
			}

			return (MethodInfo) mi [0];
		}

		//
		// FIXME: possible optimization.
		// We might be able to avoid creating `empty' if the type is the sam
		//
		bool EmitCollectionForeach (EmitContext ec, Type var_type, MethodInfo get_enum)
		{
			ILGenerator ig = ec.ig;
			LocalBuilder enumerator, disposable;
			Expression empty = new EmptyExpression ();
			Expression conv;

			//
			// FIXME: maybe we can apply the same trick we do in the
			// array handling to avoid creating empty and conv in some cases.
			//
			// Although it is not as important in this case, as the type
			// will not likely be object (what the enumerator will return).
			//
			conv = Expression.ConvertExplicit (ec, empty, var_type, loc);
			if (conv == null)
				return false;
			
			enumerator = ig.DeclareLocal (TypeManager.ienumerator_type);
			disposable = ig.DeclareLocal (TypeManager.idisposable_type);
			
			//
			// Instantiate the enumerator

			if (expr.Type.IsValueType){
				if (expr is IMemoryLocation){
					IMemoryLocation ml = (IMemoryLocation) expr;

					ml.AddressOf (ec);
				} else
					throw new Exception ("Expr " + expr + " of type " + expr.Type +
							     " does not implement IMemoryLocation");
				ig.Emit (OpCodes.Call, get_enum);
			} else {
				expr.Emit (ec);
				ig.Emit (OpCodes.Callvirt, get_enum);
			}
			ig.Emit (OpCodes.Stloc, enumerator);

			//
			// Protect the code in a try/finalize block, so that
			// if the beast implement IDisposable, we get rid of it
			//
			Label l = ig.BeginExceptionBlock ();
			Label end_try = ig.DefineLabel ();
			
			ig.MarkLabel (ec.LoopBegin);
			ig.Emit (OpCodes.Ldloc, enumerator);
			ig.Emit (OpCodes.Callvirt, TypeManager.bool_movenext_void);
			ig.Emit (OpCodes.Brfalse, end_try);
			ig.Emit (OpCodes.Ldloc, enumerator);
			ig.Emit (OpCodes.Callvirt, TypeManager.object_getcurrent_void);
			variable.EmitAssign (ec, conv);
			statement.Emit (ec);
			ig.Emit (OpCodes.Br, ec.LoopBegin);
			ig.MarkLabel (end_try);

			// The runtime provides this for us.
			// ig.Emit (OpCodes.Leave, end);

			//
			// Now the finally block
			//
			Label end_finally = ig.DefineLabel ();
			
			ig.BeginFinallyBlock ();
			ig.Emit (OpCodes.Ldloc, enumerator);
			ig.Emit (OpCodes.Isinst, TypeManager.idisposable_type);
			ig.Emit (OpCodes.Stloc, disposable);
			ig.Emit (OpCodes.Ldloc, disposable);
			ig.Emit (OpCodes.Brfalse, end_finally);
			ig.Emit (OpCodes.Ldloc, disposable);
			ig.Emit (OpCodes.Callvirt, TypeManager.void_dispose_void);
			ig.MarkLabel (end_finally);

			// The runtime generates this anyways.
			// ig.Emit (OpCodes.Endfinally);

			ig.EndExceptionBlock ();

			ig.MarkLabel (ec.LoopEnd);
			return false;
		}

		//
		// FIXME: possible optimization.
		// We might be able to avoid creating `empty' if the type is the sam
		//
		bool EmitArrayForeach (EmitContext ec, Type var_type)
		{
			Type array_type = expr.Type;
			Type element_type = array_type.GetElementType ();
			Expression conv = null;
			Expression empty = new EmptyExpression (var_type);
			
			conv = Expression.ConvertExplicit (ec, empty, var_type, loc);
			if (conv == null)
					return false;

			int rank = array_type.GetArrayRank ();
			ILGenerator ig = ec.ig;

			LocalBuilder copy = ig.DeclareLocal (array_type);
			
			//
			// Make our copy of the array
			//
			expr.Emit (ec);
			ig.Emit (OpCodes.Stloc, copy);
			
			if (rank == 1){
				LocalBuilder counter = ig.DeclareLocal (TypeManager.int32_type);

				Label loop, test;
				
				ig.Emit (OpCodes.Ldc_I4_0);
				ig.Emit (OpCodes.Stloc, counter);
				test = ig.DefineLabel ();
				ig.Emit (OpCodes.Br, test);

				loop = ig.DefineLabel ();
				ig.MarkLabel (loop);

				ig.Emit (OpCodes.Ldloc, copy);
				ig.Emit (OpCodes.Ldloc, counter);
				ArrayAccess.EmitLoadOpcode (ig, var_type);

				variable.EmitAssign (ec, conv);

				statement.Emit (ec);

				ig.MarkLabel (ec.LoopBegin);
				ig.Emit (OpCodes.Ldloc, counter);
				ig.Emit (OpCodes.Ldc_I4_1);
				ig.Emit (OpCodes.Add);
				ig.Emit (OpCodes.Stloc, counter);

				ig.MarkLabel (test);
				ig.Emit (OpCodes.Ldloc, counter);
				ig.Emit (OpCodes.Ldloc, copy);
				ig.Emit (OpCodes.Ldlen);
				ig.Emit (OpCodes.Conv_I4);
				ig.Emit (OpCodes.Blt, loop);
			} else {
				LocalBuilder [] dim_len   = new LocalBuilder [rank];
				LocalBuilder [] dim_count = new LocalBuilder [rank];
				Label [] loop = new Label [rank];
				Label [] test = new Label [rank];
				int dim;
				
				for (dim = 0; dim < rank; dim++){
					dim_len [dim] = ig.DeclareLocal (TypeManager.int32_type);
					dim_count [dim] = ig.DeclareLocal (TypeManager.int32_type);
					test [dim] = ig.DefineLabel ();
					loop [dim] = ig.DefineLabel ();
				}
					
				for (dim = 0; dim < rank; dim++){
					ig.Emit (OpCodes.Ldloc, copy);
					IntLiteral.EmitInt (ig, dim);
					ig.Emit (OpCodes.Callvirt, TypeManager.int_getlength_int);
					ig.Emit (OpCodes.Stloc, dim_len [dim]);
				}

				for (dim = 0; dim < rank; dim++){
					ig.Emit (OpCodes.Ldc_I4_0);
					ig.Emit (OpCodes.Stloc, dim_count [dim]);
					ig.Emit (OpCodes.Br, test [dim]);
					ig.MarkLabel (loop [dim]);
				}

				ig.Emit (OpCodes.Ldloc, copy);
				for (dim = 0; dim < rank; dim++)
					ig.Emit (OpCodes.Ldloc, dim_count [dim]);

				//
				// FIXME: Maybe we can cache the computation of `get'?
				//
				Type [] args = new Type [rank];
				MethodInfo get;

				for (int i = 0; i < rank; i++)
					args [i] = TypeManager.int32_type;

				ModuleBuilder mb = ec.TypeContainer.RootContext.ModuleBuilder;
				get = mb.GetArrayMethod (
					array_type, "Get",
					CallingConventions.HasThis| CallingConventions.Standard,
					var_type, args);
				ig.Emit (OpCodes.Call, get);
				variable.EmitAssign (ec, conv);
				statement.Emit (ec);
				ig.MarkLabel (ec.LoopBegin);
				for (dim = rank - 1; dim >= 0; dim--){
					ig.Emit (OpCodes.Ldloc, dim_count [dim]);
					ig.Emit (OpCodes.Ldc_I4_1);
					ig.Emit (OpCodes.Add);
					ig.Emit (OpCodes.Stloc, dim_count [dim]);

					ig.MarkLabel (test [dim]);
					ig.Emit (OpCodes.Ldloc, dim_count [dim]);
					ig.Emit (OpCodes.Ldloc, dim_len [dim]);
					ig.Emit (OpCodes.Blt, loop [dim]);
				}
			}
			ig.MarkLabel (ec.LoopEnd);
			
			return false;
		}
		
		public override bool Emit (EmitContext ec)
		{
			Type var_type;
			bool ret_val;
			
			expr = expr.Resolve (ec);
			if (expr == null)
				return false;

			var_type = ec.TypeContainer.LookupType (type, false);
			if (var_type == null)
				return false;
			
			//
			// We need an instance variable.  Not sure this is the best
			// way of doing this.
			//
			// FIXME: When we implement propertyaccess, will those turn
			// out to return values in ExprClass?  I think they should.
			//
			if (!(expr.ExprClass == ExprClass.Variable || expr.ExprClass == ExprClass.Value)){
				error1579 (expr.Type);
				return false;
			}

			ILGenerator ig = ec.ig;
			
			Label old_begin = ec.LoopBegin, old_end = ec.LoopEnd;
			bool old_inloop = ec.InLoop;
			ec.LoopBegin = ig.DefineLabel ();
			ec.LoopEnd = ig.DefineLabel ();
			ec.InLoop = true;
			
			if (expr.Type.IsArray)
				ret_val = EmitArrayForeach (ec, var_type);
			else {
				MethodInfo get_enum;
				
				if ((get_enum = ProbeCollectionType (expr.Type)) == null)
					return false;

				ret_val = EmitCollectionForeach (ec, var_type, get_enum);
			}
			
			ec.LoopBegin = old_begin;
			ec.LoopEnd = old_end;
			ec.InLoop = old_inloop;

			return ret_val;
		}
	}
}

