//
// Expressions.cs
//
// Author:
//   Juraj Skripsky (juraj@hotfeet.ch)
//
// (C) 2004 HotFeet GmbH (http://www.hotfeet.ch)
//

using System;
using System.Collections;
using System.Data;

namespace Mono.Data.SqlExpressions {
	internal interface IExpression {
		object Eval (DataRow row);
	}
	
	// abstract base classes
	internal abstract class UnaryExpression : IExpression {
		protected IExpression expr;
	
		public UnaryExpression (IExpression e)
		{
			expr = e;
		}
		
		abstract public object Eval (DataRow row);		
	}
	
	internal abstract class BinaryExpression : IExpression {
		protected IExpression expr1, expr2;
	
		protected BinaryExpression (IExpression e1, IExpression e2)
		{
			expr1 = e1;
			expr2 = e2;
		}

		abstract public object Eval (DataRow row);
	}
	
	internal enum Operation {
		AND, OR,
		EQ, NE, LT, LE, GT, GE,
		ADD, SUB, MUL, DIV, MOD
	}
	
	internal abstract class BinaryOpExpression : BinaryExpression {
		protected Operation op;
	
		protected BinaryOpExpression (Operation op, IExpression e1, IExpression e2) : base (e1, e2)
		{
			this.op = op;
		}
	}
}
