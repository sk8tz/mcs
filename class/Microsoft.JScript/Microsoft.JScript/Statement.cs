//
// Statement.cs:
//
// Author:
//	Cesar Octavio Lopez Nataren
//
// (C) 2003, 2004 Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

using System;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace Microsoft.JScript {

	internal class If : AST {

		internal AST cond, true_stm, false_stm;

		internal If (AST parent, AST condition, AST true_stm, AST false_stm)
		{
			this.cond = condition;
			this.true_stm = true_stm;
			this.false_stm = false_stm;
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			if (cond != null)
				sb.Append (cond.ToString ());
			if (true_stm != null)
				sb.Append (true_stm.ToString ());
			if (false_stm != null)
				sb.Append (false_stm.ToString ());
			
			return sb.ToString ();
		}
		
		internal override bool Resolve (IdentificationTable context)
		{
			bool r = true;

			if (cond != null)
				if (cond is Exp)
					r &= ((Exp) cond).Resolve (context, false);

			if (true_stm != null)
				if (true_stm is Exp)
					r &= ((Exp) true_stm).Resolve (context, true);
				else
					r &= true_stm.Resolve (context);			

			if (false_stm != null)
				if (false_stm is Exp)
					r &= ((Exp) false_stm).Resolve (context, true);
				else
					r &= false_stm.Resolve (context);			
			return r;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			Label true_lbl = ig.DefineLabel ();
			Label merge_lbl = ig.DefineLabel ();
			CodeGenerator.fall_false (ec, cond, true_lbl);			
			if (true_stm != null)
				true_stm.Emit (ec);			
			ig.Emit (OpCodes.Br, merge_lbl);
			ig.MarkLabel (true_lbl);
			if (false_stm != null)
				false_stm.Emit (ec);			
			ig.MarkLabel (merge_lbl);
		}
 	}

	internal class Continue : AST {

		internal string identifier;

		public override string ToString ()
		{
			return identifier;
		}

		internal override bool Resolve (IdentificationTable context)
		{
			throw new NotImplementedException ();
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}

	internal class Break : AST {

		internal string identifier;

		public override string ToString ()
		{
			return identifier;
		}

		internal override bool Resolve (IdentificationTable context)
		{
			throw new NotImplementedException ();
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}

	internal class Return : AST {

		internal AST expression;

		internal Return (AST parent, AST exp)
		{
			this.parent = parent;
			expression = exp;
		}

		public override string ToString ()
		{
			return expression.ToString ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			if (parent == null)
				throw new Exception ("error JS1018: 'return' statement outside of function");

			return expression.Resolve (context);
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}

	internal class DoWhile : AST {

		AST stm, exp;
		
		internal DoWhile (AST parent, AST stm, AST exp)
		{
			this.parent = parent;
			this.stm = stm;
			this.exp = exp;
		}

		internal override bool Resolve (IdentificationTable context)
		{
			bool r = true;

			if (stm != null)
				r &= stm.Resolve (context);
			if (exp != null)
				r &= exp.Resolve (context);
			return r;
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}


	internal class While : AST {
		
		AST exp, stm;

		internal While (AST parent, AST exp, AST stm)
		{
			this.parent = parent;
			this.exp = exp;
			this.stm = stm;
		}

		internal override bool Resolve (IdentificationTable context)
		{
			bool r = true;

			if (exp != null)
				if (exp is Exp)
					r &= ((Exp) exp).Resolve (context, false);
				else 
					r &= exp.Resolve (context);
			if (stm != null)
				r &= stm.Resolve (context);
			return r;
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}


}
