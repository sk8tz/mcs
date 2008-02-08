//
// ExpressionTest_Invoke.cs
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

namespace MonoTests.System.Linq.Expressions {

	[TestFixture]
	public class ExpressionTest_Invoke {

		static Expression CreateInvokable<T> ()
		{
			return Expression.Parameter (typeof (T), "invokable");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullExpression ()
		{
			Expression.Invoke (null, new Expression [0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullArgument ()
		{
			Expression.Invoke (CreateInvokable<Action<int>> (), new [] { null as Expression });
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void NonInvokableExpressionType ()
		{
			Expression.Invoke (CreateInvokable<int> (), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ArgumentCountDoesntMatchParametersLength ()
		{
			Expression.Invoke (CreateInvokable<Action<int>> (), 1.ToConstant (), 2.ToConstant ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ArgumentNotAssignableToParameterType ()
		{
			Expression.Invoke (CreateInvokable<Action<int>> (), "".ToConstant ());
		}

		[Test]
		public void EmptyArguments ()
		{
			var invoke = Expression.Invoke (CreateInvokable<Action> (), null);
			Assert.AreEqual (typeof (void), invoke.Type);
			Assert.IsNotNull (invoke.Arguments);
			Assert.AreEqual (0, invoke.Arguments.Count);
			Assert.AreEqual ("Invoke(invokable)", invoke.ToString ());
		}

		[Test]
		public void InvokeFunc ()
		{
			var invoke = Expression.Invoke (CreateInvokable<Func<string, string, int>> (), "foo".ToConstant (), "bar".ToConstant ());
			Assert.AreEqual (typeof (int), invoke.Type);
			Assert.AreEqual (2, invoke.Arguments.Count);
			Assert.AreEqual ("Invoke(invokable, \"foo\", \"bar\")", invoke.ToString ());
		}
	}
}
