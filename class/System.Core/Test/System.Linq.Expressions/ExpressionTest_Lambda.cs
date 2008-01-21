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
//   Miguel de Icaza (miguel@novell.com)

using System;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace MonoTests.System.Linq.Expressions
{
	[TestFixture]
	public class ExpressionTest_Lambda
	{
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void NonDelegateTypeInCtor ()
		{
			// The first parameter must be a delegate type
			Expression.Lambda (typeof(string), Expression.Constant (1), new ParameterExpression [0]);
		}

		delegate object delegate_object_emtpy ();
		delegate object delegate_object_int (int a);
		delegate object delegate_object_string (string s);
		delegate object delegate_object_object (object s);

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void InvalidConversion ()
		{
			// float to object, invalid
			Expression.Lambda (typeof (delegate_object_emtpy), Expression.Constant (1.0), new ParameterExpression [0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void InvalidConversion2 ()
		{
			// float to object, invalid
			Expression.Lambda (typeof (delegate_object_emtpy), Expression.Constant (1), new ParameterExpression [0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void InvalidArgCount ()
		{
			// missing a parameter
			Expression.Lambda (typeof (delegate_object_int), Expression.Constant ("foo"), new ParameterExpression [0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void InvalidArgCount2 ()
		{
			// extra parameter
			ParameterExpression p = Expression.Parameter (typeof (int), "AAA");
			Expression.Lambda (typeof (delegate_object_emtpy), Expression.Constant ("foo"), new ParameterExpression [1] {p});
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void InvalidArgType ()
		{
			// invalid argument type
			
			ParameterExpression p = Expression.Parameter (typeof (string), "AAA");
			Expression.Lambda (typeof (delegate_object_int), Expression.Constant ("foo"), new ParameterExpression [1] {p});
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void InvalidArgType2 ()
		{
			// invalid argument type
			
			ParameterExpression p = Expression.Parameter (typeof (string), "AAA");
			Expression.Lambda (typeof (delegate_object_object), Expression.Constant ("foo"), new ParameterExpression [1] {p});
		}
		
		[Test]
		public void Assignability ()
		{
			// allowed: string to object
			Expression.Lambda (typeof (delegate_object_emtpy), Expression.Constant ("string"), new ParameterExpression [0]);

			// allowed delegate has string, delegate has base class (object)
			ParameterExpression p = Expression.Parameter (typeof (object), "ParObject");
			Expression.Lambda (typeof (delegate_object_string), Expression.Constant (""), new ParameterExpression [1] {p});
		}

		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void ParameterOutOfScope ()
		{
			ParameterExpression a = Expression.Parameter(typeof (int), "a");
			ParameterExpression second_a = Expression.Parameter(typeof(int), "a");

			// Here we have the same name for the parameter expression, but
			// we pass a different object to the Lambda expression, so they are
			// different, this should throw
			Expression<Func<int,int>> l = Expression.Lambda<Func<int,int>>(a, new ParameterExpression [] { second_a });
			l.Compile ();
		}

		[Test]
		public void ParameterRefTest ()
		{
			ParameterExpression a = Expression.Parameter(typeof(int), "a");
			ParameterExpression b = Expression.Parameter(typeof(int), "b");

			Expression<Func<int,int,int>> l = Expression.Lambda<Func<int,int,int>>(
				Expression.Add (a, b), new ParameterExpression [] { a, b });
			Func<int,int,int> xx = l.Compile ();
			int res = xx (10, 20);
			Assert.AreEqual (res, 30);
		}
		
		[Test]
		public void Compile ()
		{
			Expression<Func<int>> l = Expression.Lambda<Func<int>> (Expression.Constant (1), new ParameterExpression [0]);
			Func<int> fi = l.Compile ();
			fi ();
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void ReturnValueCheck ()
		{
			ParameterExpression p1 = Expression.Parameter(typeof(int?), "va");
			ParameterExpression p2 = Expression.Parameter(typeof(int?), "vb");
			Expression add = Expression.Add(p1, p2);
			

			// This should throw, since the add.Type is "int?" and the return
			// type we have here is int.
			Expression<Func<int?, int?, int>> efu = Expression.Lambda<Func<int?,int?,int>> (add, p1, p2);
		}
	}
}
