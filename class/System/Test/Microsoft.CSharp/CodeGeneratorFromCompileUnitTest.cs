//
// Microsoft.CSharp.* Test Cases
//
// Authors:
// 	Erik LeBel (eriklebel@yahoo.ca)
//
// (c) 2003 Erik LeBel
//
using System;
using System.Text;
using System.CodeDom;
using System.CodeDom.Compiler;

using NUnit.Framework;

namespace MonoTests.Microsoft.CSharp
{
	///
	/// <summary>
	///	Test ICodeGenerator's GenerateCodeFromCompileUnit, along with a 
	///	minimal set CodeDom components.
	/// </summary>
	///
	[TestFixture]
	public class CodeGeneratorFromCompileUnitTest : CodeGeneratorTestBase
	{
		string codeUnitHeader = "";
		CodeCompileUnit codeUnit = null;

		public CodeGeneratorFromCompileUnitTest ()
		{
			Init();
			Generate();
			codeUnitHeader = Code;
		}
		
		[SetUp]
		public void Init ()
		{
			InitBase ();
			codeUnit = new CodeCompileUnit ();
		}
		
		protected override string Code {
			get { return base.Code.Substring (codeUnitHeader.Length); }
		}
		
		protected override void Generate ()
		{
			generator.GenerateCodeFromCompileUnit (codeUnit, writer, options);
			writer.Close ();
		}
		
		[Test]
		public void DefaultCodeUnitTest ()
		{
			Generate ();
			Assertion.AssertEquals ("", Code);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void NullCodeUnitTest ()
		{
			codeUnit = null;
			Generate();
		}

		[Test]
		public void ReferencedTest ()
		{
			codeUnit.ReferencedAssemblies.Add ("System.dll");
			Generate();
			Assertion.AssertEquals ("", Code);
		}

		[Test]
		public void SimpleNamespaceTest ()
		{
			CodeNamespace ns = new CodeNamespace ("A");
			codeUnit.Namespaces.Add (ns);
			Generate ();
			Assertion.AssertEquals ("namespace A {\n    \n}\n", Code);
		}

		[Test]
		public void ReferenceAndSimpleNamespaceTest()
		{
			CodeNamespace ns = new CodeNamespace ("A");
			codeUnit.Namespaces.Add (ns);
			codeUnit.ReferencedAssemblies.Add ("using System;");
			Generate ();
			Assertion.AssertEquals ("namespace A {\n    \n}\n", Code);
		}

		[Test]
		public void SimpleAttributeTest ()
		{
			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";

			codeUnit.AssemblyCustomAttributes.Add (attrDec);
			Generate ();
			Assertion.AssertEquals ("[assembly: A()]", Code.Trim ());
		}

		[Test]
		[Ignore ("Bug #75190")]
		public void CodeSnippetTest ()
		{
			codeUnit = new CodeSnippetCompileUnit ("public class Test1 {}");
			generator.GenerateCodeFromCompileUnit (codeUnit, writer, options);
			writer.Close ();
			Assertion.AssertEquals ("public class Test1 {}" + writer.NewLine, writer.ToString());
		}

		/* FIXME
		[Test]
		public void AttributeWithValueTest ()
		{
			CodeAttributeDeclaration attrDec = new CodeAttributeDeclaration ();
			attrDec.Name = "A";
			

			codeUnit.AssemblyCustomAttributes.Add (attrDec);
			Generate ();
			Assertion.AssertEquals ("[assembly: A()]\n\n", Code);
		}*/

	}
}
