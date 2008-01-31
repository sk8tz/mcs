﻿//
// MemberExpression.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//
// (C) 2008 Novell, Inc. (http://www.novell.com)
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
using System.Reflection;
using System.Reflection.Emit;

namespace System.Linq.Expressions {

	public sealed class MemberExpression : Expression {

		Expression expression;
		MemberInfo member;

		public Expression Expression {
			get { return expression; }
		}

		public MemberInfo Member {
			get { return member; }
		}

		internal MemberExpression (Expression expression, MemberInfo member, Type type)
			: base (ExpressionType.MemberAccess, type)
		{
			this.expression = expression;
			this.member = member;
		}

		internal override void Emit (EmitContext ec)
		{
			var property = member as PropertyInfo;
			if (property != null) {
				EmitPropertyAccess (ec, property);
				return;
			}

			var field = member as FieldInfo;
			if (field != null) {
				EmitFieldAccess (ec, field);
				return;
			}

			throw new NotSupportedException ();
		}

		void EmitPropertyAccess (EmitContext ec, PropertyInfo property)
		{
			throw new NotImplementedException ();
		}

		void EmitFieldAccess (EmitContext ec, FieldInfo field)
		{
			if (!field.IsStatic) {
				expression.Emit (ec);
				ec.ig.Emit (OpCodes.Ldfld, field);
			} else
				ec.ig.Emit (OpCodes.Ldsfld, field);
		}
	}
}
