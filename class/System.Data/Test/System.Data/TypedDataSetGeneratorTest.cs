//
// TypedDataSetGeneratorTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.CodeDom.Compiler;
using System.Data;
using NUnit.Framework;
using Microsoft.CSharp;

namespace MonoTests.System.Data
{
	public class TypedDataSetGeneratorTest : Assertion
	{
		private ICodeGenerator gen;

		public TypedDataSetGeneratorTest ()
		{
			gen = new CSharpCodeProvider ().CreateGenerator ();
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void TestGenerateIdNameNullName ()
		{
			TypedDataSetGenerator.GenerateIdName (null, gen);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void TestGenerateIdNameNullProvider ()
		{
			TypedDataSetGenerator.GenerateIdName ("a", null);
		}

		[Test]
		public void TestGenerateIdName ()
		{
		
			AssertEquals ("a", TypedDataSetGenerator.GenerateIdName ("a", gen));
			AssertEquals ("_int", TypedDataSetGenerator.GenerateIdName ("int", gen));
			AssertEquals ("_", TypedDataSetGenerator.GenerateIdName ("_", gen));
			AssertEquals ("1", TypedDataSetGenerator.GenerateIdName ("1", gen));
			AssertEquals ("1a", TypedDataSetGenerator.GenerateIdName ("1a", gen));
			AssertEquals ("1*2", TypedDataSetGenerator.GenerateIdName ("1*2", gen));
			AssertEquals ("-", TypedDataSetGenerator.GenerateIdName ("-", gen));
			AssertEquals ("+", TypedDataSetGenerator.GenerateIdName ("+", gen));
			AssertEquals ("", TypedDataSetGenerator.GenerateIdName ("", gen));
			AssertEquals ("--", TypedDataSetGenerator.GenerateIdName ("--", gen));
			AssertEquals ("++", TypedDataSetGenerator.GenerateIdName ("++", gen));
			AssertEquals ("\u3042", TypedDataSetGenerator.GenerateIdName ("\u3042", gen));
		}

	}
}
