//
// ExpressionTest_CallWithExpression.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace MonoTests.System.Linq.Expressions {

	[TestFixture]
	public class ExpressionTest_CallWithExpression {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ArgMethodNull ()
		{
			Expression.Call (Expression.Constant (new object ()), null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ArgInstanceNullForNonStaticMethod ()
		{
			Expression.Call (null, typeof (object).GetMethod ("ToString"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void InstanceTypeDoesntMatchMethodDeclaringType ()
		{
			Expression.Call (Expression.Constant (1), typeof (string).GetMethod ("Intern"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MethodArgumentCountDoesnMatchParameterLength ()
		{
			Expression.Call (Expression.Constant (new object ()), typeof (object).GetMethod ("ToString"), Expression.Constant (new object ()));
		}

		public class Foo {
			public void Bar (string s)
			{
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void MethodHasNullArgument ()
		{
			Expression.Call (Expression.New (typeof (Foo)), typeof (Foo).GetMethod ("Bar"), null as Expression);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MethodArgumentDoesntMatchParameterType ()
		{
			Expression.Call (Expression.New (typeof (Foo)), typeof (Foo).GetMethod ("Bar"), Expression.Constant (42));
		}

		[Test]
		public void CallToString ()
		{
			var call = Expression.Call (Expression.Constant (new object ()), typeof (object).GetMethod ("ToString"));
			Assert.AreEqual ("value(System.Object).ToString()", call.ToString ());
		}

		[Test]
		public void CallStringIsNullOrEmpty ()
		{
			var call = Expression.Call (null, typeof (string).GetMethod ("IsNullOrEmpty"), Expression.Constant (""));
			Assert.AreEqual ("IsNullOrEmpty(\"\")", call.ToString ());
		}

		public static object Identity (object o)
		{
			return o;
		}

		[Test]
		public void CompileSimpleStaticCall ()
		{
			var p = Expression.Parameter (typeof (object), "o");
			var lambda = Expression.Lambda<Func<object, object>> (Expression.Call (GetType ().GetMethod ("Identity"), p), p);

			var i = lambda.Compile ();

			Assert.AreEqual (2, i (2));
			Assert.AreEqual ("Foo", i ("Foo"));
		}

		[Test]
		public void CompileSimpleInstanceCall ()
		{
			var p = Expression.Parameter (typeof (string), "p");
			var lambda = Expression.Lambda<Func<string, string>> (
				Expression.Call (
					p, typeof (string).GetMethod ("ToString", Type.EmptyTypes)),
				p);

			var ts = lambda.Compile ();

			Assert.AreEqual ("foo", ts ("foo"));
			Assert.AreEqual ("bar", ts ("bar"));
		}
	}
}
