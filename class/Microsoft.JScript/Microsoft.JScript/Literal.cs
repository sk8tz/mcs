//
// Literal.cs: This class groups the differents types of Literals.
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) Cesar Lopez Nataren 
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Reflection.Emit;

namespace Microsoft.JScript {

	internal class This : AST {

		internal override bool Resolve (IdentificationTable context)
		{
			throw new NotImplementedException ();
		}

		internal override void Emit (EmitContext ec)
		{
			throw new NotImplementedException ();
		}
	}

	internal class BooleanLiteral : Exp {

		internal bool val;

		internal BooleanLiteral (AST parent, bool val)
		{
			this.parent = parent;
			this.val = val;
		}

		public override string ToString ()
		{
			return val.ToString ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			return true;
		}

		internal override bool Resolve (IdentificationTable context, bool no_effect)
		{
			this.no_effect = no_effect;
			return true;
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;

			if (val)
				ig.Emit (OpCodes.Ldc_I4_1);
			else
				ig.Emit (OpCodes.Ldc_I4_0);

			ig.Emit (OpCodes.Box, typeof (System.Boolean));

			if (no_effect)
				ig.Emit (OpCodes.Pop);
		}
	}

	public class NumericLiteral : Exp {

		double val;

		internal NumericLiteral (AST parent, double val)
		{
			this.parent = parent;
			this.val = val;
		}

		public override string ToString ()
		{
			return val.ToString ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			return true;
		}

		internal override bool Resolve (IdentificationTable context, bool no_effect)
		{			
			this.no_effect = no_effect;
			return Resolve (context);
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			if (parent is Unary) {
				Unary tmp = parent as Unary;
				if (tmp.oper == JSToken.Minus)
					ig.Emit (OpCodes.Ldc_I4, (int) (val * -1));
			} else
				ig.Emit (OpCodes.Ldc_I4, (int) val);
			ig.Emit (OpCodes.Box, typeof (System.Int32));
			if (no_effect)
				ig.Emit (OpCodes.Pop);
		}
	}
}
