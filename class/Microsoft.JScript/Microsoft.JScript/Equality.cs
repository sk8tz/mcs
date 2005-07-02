//
// Equality.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, 2004 Cesar Lopez Nataren
// (C) 2005, Novell Inc, (http://novell.com)
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
using System.Text;
using System.Diagnostics;
using System.Reflection.Emit;

namespace Microsoft.JScript {

	public class Equality : BinaryOp {

		internal Equality (AST parent, AST left, AST right, JSToken op)
			: base (left, right, op)
		{
		}

		public Equality (int i)
			: base (null, null, (JSToken) i)
		{
		}

		[DebuggerStepThroughAttribute]
		[DebuggerHiddenAttribute]
		public bool EvaluateEquality (object v1, object v2)
		{
			IConvertible ic1 = v1 as IConvertible;
			IConvertible ic2 = v2 as IConvertible;

			TypeCode tc1 = Convert.GetTypeCode (v1, ic1);
			TypeCode tc2 = Convert.GetTypeCode (v2, ic2);

			switch (tc1) {
			case TypeCode.Empty:
				switch (tc2) {
				case TypeCode.Empty:
					return true;
				case TypeCode.DBNull:
					return true;
				}
				break;

			case TypeCode.DBNull:
				switch (tc1) {
				case TypeCode.DBNull:
					return true;
				case TypeCode.Empty:
					return true;
				}
				break;

			case TypeCode.Boolean:
				switch (tc1) {
				case TypeCode.Boolean:
					return ic1.ToBoolean (null) == ic2.ToBoolean (null);
				}
				break;

			case TypeCode.Char:
				switch (tc2) {
				case TypeCode.Char:
					return ic1.ToChar (null) == ic2.ToChar (null);
				case TypeCode.Double:
					return (double) ic1.ToChar (null) == ic2.ToDouble (null);
				}
				break;

			case TypeCode.Double:
				switch (tc2) {
				case TypeCode.Double:
					return ic1.ToDouble (null) == ic2.ToDouble (null);
				}
				break;

			case TypeCode.String:
				switch (tc2) {
				case TypeCode.String:
					return ic1.ToString (null) == ic2.ToString (null);

				case TypeCode.Double:
					return ic1.ToDouble (null) == ic2.ToDouble (null);
				}
				break;

			case TypeCode.Int32:
				switch (tc2) {
				case TypeCode.Double:
					return ic1.ToDouble (null) == ic2.ToDouble (null);
				}
				break;

			default:
				Console.WriteLine ("Equality, tc1 = {0}, tc2 = {1}", tc1, tc2);
				break;
			}
			throw new Exception ("error: Not an equality operator");
		}

		public static bool JScriptEquals (object v1, object v2)
		{
			throw new NotImplementedException ();
		}

		internal override bool Resolve (IdentificationTable context)
		{
			bool r = true;
			if (left != null)
				r &= left.Resolve (context);
			if (right != null)
				r &= right.Resolve (context);
			return r;
		}

		internal override bool Resolve (IdentificationTable context, bool no_effect)
		{
			this.no_effect = no_effect;
			return Resolve (context);
		}

		internal override void Emit (EmitContext ec)
		{
			ILGenerator ig = ec.ig;
			LocalBuilder local_builder;

			if (op != JSToken.None) {
				Type t = typeof (Equality);
				local_builder = ig.DeclareLocal (t);
				if (op == JSToken.Equal)
					ig.Emit (OpCodes.Ldc_I4_S, (byte) 53);
				else if (op == JSToken.NotEqual)
					ig.Emit (OpCodes.Ldc_I4_S, (byte) 54);
				ig.Emit (OpCodes.Newobj, t.GetConstructor (new Type [] { typeof (int) }));
				ig.Emit (OpCodes.Stloc, local_builder);
				ig.Emit (OpCodes.Ldloc, local_builder);
			}

			if (left != null)
				left.Emit (ec);
			if (right != null)
				right.Emit (ec);

			if (op == JSToken.Equal || op == JSToken.NotEqual) {
				ig.Emit (OpCodes.Call, typeof (Equality).GetMethod ("EvaluateEquality"));

				if (no_effect) {
					Label t_lbl = ig.DefineLabel ();
					Label f_lbl = ig.DefineLabel ();

					if (op == JSToken.Equal)
						ig.Emit (OpCodes.Brtrue_S, t_lbl);
					else if (op == JSToken.NotEqual)
						ig.Emit (OpCodes.Brfalse_S, t_lbl);

					ig.Emit (OpCodes.Ldc_I4_0);
					ig.Emit (OpCodes.Br_S, f_lbl);
					ig.MarkLabel (t_lbl);
					ig.Emit (OpCodes.Ldc_I4_1);
					ig.MarkLabel (f_lbl);
					ig.Emit (OpCodes.Pop);
				}
			}
		}
	}
}
