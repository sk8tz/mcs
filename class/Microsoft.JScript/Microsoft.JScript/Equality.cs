//
// Equality.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
//

using System;
using System.Text;
using System.Reflection.Emit;

namespace Microsoft.JScript {

	public class Equality : BinaryOp {

		internal Equality (AST parent, AST left, AST right, JSToken op)
		{
			this.left = left;
			this.right = right;
			this.current_op = op;
		}

		public Equality (Context context, AST oper1, AST oper2, JSToken operatorTok)
		{
			throw new NotImplementedException ();
		}

		public Equality (int i)
		{
			current_op = (JSToken) i;
		}

		public bool EvaluateEquality (object v1, object v2)
		{
			return false;
		}


		public static bool JScriptEquals (object v1, object v2)
		{
			throw new NotImplementedException ();
		}

		public override string ToString ()
		{
			StringBuilder sb = new StringBuilder ();

			sb.Append (left.ToString ());

			if (current_op != JSToken.None)
				sb.Append (current_op + " ");

			if (right != null)
				sb.Append (right.ToString ());

			return sb.ToString ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			return true;
		}

		internal override bool Resolve (IdentificationTable context, bool no_effect)
		{
			throw new NotImplementedException ();
		}	       

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}
}
