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
//
// Authors:
//		Federico Di Gregorio <fog@initd.org>

using System;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace MonoTests.System.Linq.Expressions
{
	[TestFixture]
	public class ExpressionTest_OrElse
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.OrElse (null, Expression.Constant (1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg2Null ()
		{
			Expression.OrElse (Expression.Constant (1), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void NoOperatorClass ()
		{
			Expression.OrElse (Expression.Constant (new NoOpClass ()), Expression.Constant (new NoOpClass ()));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Double ()
		{
			Expression.OrElse (Expression.Constant (1.0), Expression.Constant (2.0));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Integer ()
		{
			Expression.OrElse (Expression.Constant (1), Expression.Constant (2));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void MismatchedTypes ()
		{
			Expression.OrElse (Expression.Constant (new OpClass ()), Expression.Constant (true));
		}

		[Test]
		public void Boolean ()
		{
			BinaryExpression expr = Expression.OrElse (Expression.Constant (true), Expression.Constant (false));
			Assert.AreEqual (ExpressionType.OrElse, expr.NodeType, "OrElse#01");
			Assert.AreEqual (typeof (bool), expr.Type, "OrElse#02");
			Assert.IsNull (expr.Method, "OrElse#03");
			Assert.AreEqual ("(True || False)", expr.ToString(), "OrElse#04");
		}

		[Test]
		public void UserDefinedClass ()
		{
			// We can use the simplest version of GetMethod because we already know only one
			// exists in the very simple class we're using for the tests.
			MethodInfo mi = typeof (OpClass).GetMethod ("op_BitwiseOr");

			BinaryExpression expr = Expression.OrElse (Expression.Constant (new OpClass ()), Expression.Constant (new OpClass ()));
			Assert.AreEqual (ExpressionType.OrElse, expr.NodeType, "OrElse#05");
			Assert.AreEqual (typeof (OpClass), expr.Type, "OrElse#06");
			Assert.AreEqual (mi, expr.Method, "OrElse#07");
			Assert.AreEqual ("op_BitwiseOr", expr.Method.Name, "OrElse#08");
			Assert.AreEqual ("(value(MonoTests.System.Linq.Expressions.OpClass) || value(MonoTests.System.Linq.Expressions.OpClass))",
				expr.ToString(), "OrElse#09");
		}

		public class BrokenMethod {
			public static int operator | (BrokenMethod a, BrokenMethod b)
			{
				return 1;
			}
		}

		public class BrokenMethod2 {
			public static BrokenMethod2 operator | (BrokenMethod2 a, int b)
			{
				return null;
			}
		}
		
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void MethodInfoReturnType ()
		{
			Expression.OrElse (Expression.Constant (new BrokenMethod ()),
					   Expression.Constant (new BrokenMethod ()));
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void MethodInfoReturnType2 ()
		{
			Expression.OrElse (Expression.Constant (new BrokenMethod2 ()),
					   Expression.Constant (1));
		}
	}
}
