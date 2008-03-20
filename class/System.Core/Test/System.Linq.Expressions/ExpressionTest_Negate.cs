//
// ExpressionTest_Negate.cs
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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace MonoTests.System.Linq.Expressions
{
	[TestFixture]
	public class ExpressionTest_Negate
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.Negate (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MethodArgNotStatic ()
		{
			Expression.Negate (Expression.Constant (new object ()), typeof (OpClass).GetMethod ("WrongUnaryNotStatic"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MethodArgParameterCount ()
		{
			Expression.Negate (Expression.Constant (new object ()), typeof (OpClass).GetMethod ("WrongUnaryParameterCount"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MethodArgReturnsVoid ()
		{
			Expression.Negate (Expression.Constant (new object ()), typeof (OpClass).GetMethod ("WrongUnaryReturnVoid"));
		}

		[Test]
		public void Number ()
		{
			var up = Expression.Negate (Expression.Constant (1));
			Assert.AreEqual ("-1", up.ToString ());
		}

		[Test]
		public void UserDefinedClass ()
		{
			var mi = typeof (OpClass).GetMethod ("op_UnaryNegation");

			var expr = Expression.Negate (Expression.Constant (new OpClass ()));
			Assert.AreEqual (ExpressionType.Negate, expr.NodeType);
			Assert.AreEqual (typeof (OpClass), expr.Type);
			Assert.AreEqual (mi, expr.Method);
			Assert.AreEqual ("op_UnaryNegation", expr.Method.Name);
			Assert.AreEqual ("-value(MonoTests.System.Linq.Expressions.OpClass)",	expr.ToString ());
		}
	}
}
