//
// XPathScannerTests.cs
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
//

using System;
using System.Xml.XPath;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	public class XPathScannerTests : TestCase
	{
		public XPathScannerTests () : base ("MonoTests.System.Xml.XPathScannerTests testsuite") { }
		public XPathScannerTests (string name) : base (name) { }

		public void TestLocationPathWithOneNameTest ()
		{
			XPathScanner scanner = new XPathScanner ("foo");
			AssertEquals (XPathTokenType.NameTest, scanner.Scan ());
			AssertEquals ("foo", scanner.Value);
			AssertEquals (XPathTokenType.End, scanner.Scan ());
			AssertNull (scanner.Value);
		}

		public void TestLocationPathWithTwoNameTests ()
		{
			XPathScanner scanner = new XPathScanner ("foo/bar");
			AssertEquals (XPathTokenType.NameTest, scanner.Scan ());
			AssertEquals ("foo", scanner.Value);
			AssertEquals (XPathTokenType.Operator, scanner.Scan ());
			AssertEquals ("/", scanner.Value);
			AssertEquals (XPathTokenType.NameTest, scanner.Scan ());
			AssertEquals ("bar", scanner.Value);
			AssertEquals (XPathTokenType.End, scanner.Scan ());
			AssertNull (scanner.Value);
		}

		public void TestLocationPathWithOneQualifiedNameTest ()
		{
			XPathScanner scanner = new XPathScanner ("foo:bar");
			AssertEquals (XPathTokenType.NameTest, scanner.Scan ());
			AssertEquals ("foo:bar", scanner.Value);
			AssertEquals (XPathTokenType.End, scanner.Scan ());
			AssertNull (scanner.Value);
		}

		public void TestLocationPathWithTwoQualifiedNameTests ()
		{
			XPathScanner scanner = new XPathScanner ("foo:bar/baz:quux");
			AssertEquals (XPathTokenType.NameTest, scanner.Scan ());
			AssertEquals ("foo:bar", scanner.Value);
			AssertEquals (XPathTokenType.Operator, scanner.Scan ());
			AssertEquals ("/", scanner.Value);
			AssertEquals (XPathTokenType.NameTest, scanner.Scan ());
			AssertEquals ("baz:quux", scanner.Value);
			AssertEquals (XPathTokenType.End, scanner.Scan ());
			AssertNull (scanner.Value);
		}

		public void TestLocationPathWithOneNameTestWithAxisName ()
		{
			XPathScanner scanner = new XPathScanner ("child::foo");
			AssertEquals (XPathTokenType.AxisName, scanner.Scan ());
			AssertEquals ("child", scanner.Value);
			AssertEquals (XPathTokenType.ColonColon, scanner.Scan ());
			AssertEquals ("::", scanner.Value);
			AssertEquals (XPathTokenType.NameTest, scanner.Scan ());
			AssertEquals ("foo", scanner.Value);
			AssertEquals (XPathTokenType.End, scanner.Scan ());
			AssertNull (scanner.Value);
		}

		public void TestLocationPathWithTwoNameTestsWithAxisNames ()
		{
			XPathScanner scanner = new XPathScanner ("child::foo/preceding-sibling::bar");
			AssertEquals (XPathTokenType.AxisName, scanner.Scan ());
			AssertEquals ("child", scanner.Value);
			AssertEquals (XPathTokenType.ColonColon, scanner.Scan ());
			AssertEquals ("::", scanner.Value);
			AssertEquals (XPathTokenType.NameTest, scanner.Scan ());
			AssertEquals ("foo", scanner.Value);
			AssertEquals (XPathTokenType.Operator, scanner.Scan ());
			AssertEquals ("/", scanner.Value);
			AssertEquals (XPathTokenType.AxisName, scanner.Scan ());
			AssertEquals ("preceding-sibling", scanner.Value);
			AssertEquals (XPathTokenType.ColonColon, scanner.Scan ());
			AssertEquals ("::", scanner.Value);
			AssertEquals (XPathTokenType.NameTest, scanner.Scan ());
			AssertEquals ("bar", scanner.Value);
			AssertEquals (XPathTokenType.End, scanner.Scan ());
			AssertNull (scanner.Value);
		}

		public void TestCommentNodeType ()
		{
			XPathScanner scanner = new XPathScanner ("comment()");
			AssertEquals (XPathTokenType.NodeType, scanner.Scan ());
			AssertEquals ("comment", scanner.Value);
			AssertEquals (XPathTokenType.LeftParen, scanner.Scan ());
			AssertEquals ("(", scanner.Value);
			AssertEquals (XPathTokenType.RightParen, scanner.Scan ());
			AssertEquals (")", scanner.Value);
			AssertEquals (XPathTokenType.End, scanner.Scan ());
			AssertNull (scanner.Value);
		}

		public void TestNodeNodeType ()
		{
			XPathScanner scanner = new XPathScanner ("node()");
			AssertEquals (XPathTokenType.NodeType, scanner.Scan ());
			AssertEquals ("node", scanner.Value);
			AssertEquals (XPathTokenType.LeftParen, scanner.Scan ());
			AssertEquals ("(", scanner.Value);
			AssertEquals (XPathTokenType.RightParen, scanner.Scan ());
			AssertEquals (")", scanner.Value);
			AssertEquals (XPathTokenType.End, scanner.Scan ());
			AssertNull (scanner.Value);
		}

		public void TestProcessingInstructionNodeType ()
		{
			XPathScanner scanner = new XPathScanner ("processing-instruction()");
			AssertEquals (XPathTokenType.NodeType, scanner.Scan ());
			AssertEquals ("processing-instruction", scanner.Value);
			AssertEquals (XPathTokenType.LeftParen, scanner.Scan ());
			AssertEquals ("(", scanner.Value);
			AssertEquals (XPathTokenType.RightParen, scanner.Scan ());
			AssertEquals (")", scanner.Value);
			AssertEquals (XPathTokenType.End, scanner.Scan ());
			AssertNull (scanner.Value);
		}

		public void TestTextNodeType ()
		{
			XPathScanner scanner = new XPathScanner ("text()");
			AssertEquals (XPathTokenType.NodeType, scanner.Scan ());
			AssertEquals ("text", scanner.Value);
			AssertEquals (XPathTokenType.LeftParen, scanner.Scan ());
			AssertEquals ("(", scanner.Value);
			AssertEquals (XPathTokenType.RightParen, scanner.Scan ());
			AssertEquals (")", scanner.Value);
			AssertEquals (XPathTokenType.End, scanner.Scan ());
			AssertNull (scanner.Value);
		}

		public void TestFunctionName ()
		{
			XPathScanner scanner = new XPathScanner ("foo()");
			AssertEquals (XPathTokenType.FunctionName, scanner.Scan ());
			AssertEquals ("foo", scanner.Value);
			AssertEquals (XPathTokenType.LeftParen, scanner.Scan ());
			AssertEquals ("(", scanner.Value);
			AssertEquals (XPathTokenType.RightParen, scanner.Scan ());
			AssertEquals (")", scanner.Value);
			AssertEquals (XPathTokenType.End, scanner.Scan ());
			AssertNull (scanner.Value);
		}

		public void TestQualifiedFunctionName ()
		{
			XPathScanner scanner = new XPathScanner ("foo:bar()");
			AssertEquals (XPathTokenType.FunctionName, scanner.Scan ());
			AssertEquals ("foo:bar", scanner.Value);
			AssertEquals (XPathTokenType.LeftParen, scanner.Scan ());
			AssertEquals ("(", scanner.Value);
			AssertEquals (XPathTokenType.RightParen, scanner.Scan ());
			AssertEquals (")", scanner.Value);
			AssertEquals (XPathTokenType.End, scanner.Scan ());
			AssertNull (scanner.Value);
		}

		public void TestWildcardNameTest ()
		{
			XPathScanner scanner = new XPathScanner ("*");
			AssertEquals (XPathTokenType.NameTest, scanner.Scan ());
			AssertEquals ("*", scanner.Value);
			AssertEquals (XPathTokenType.End, scanner.Scan ());
			AssertNull (scanner.Value);
		}

		public void TestQualifiedWildcardNameTest ()
		{
			XPathScanner scanner = new XPathScanner ("foo:*");
			AssertEquals (XPathTokenType.NameTest, scanner.Scan ());
			AssertEquals ("foo:*", scanner.Value);
			AssertEquals (XPathTokenType.End, scanner.Scan ());
			AssertNull (scanner.Value);
		}

		public void TestTwoWildcardNameTests ()
		{
			XPathScanner scanner = new XPathScanner ("*/*");
			AssertEquals (XPathTokenType.NameTest, scanner.Scan ());
			AssertEquals ("*", scanner.Value);
			AssertEquals (XPathTokenType.Operator, scanner.Scan ());
			AssertEquals ("/", scanner.Value);
			AssertEquals (XPathTokenType.NameTest, scanner.Scan ());
			AssertEquals ("*", scanner.Value);
			AssertEquals (XPathTokenType.End, scanner.Scan ());
			AssertNull (scanner.Value);
		}

		public void TestTwoQualifiedWildcardNameTests ()
		{
			XPathScanner scanner = new XPathScanner ("foo:*/bar:*");
			AssertEquals (XPathTokenType.NameTest, scanner.Scan ());
			AssertEquals ("foo:*", scanner.Value);
			AssertEquals (XPathTokenType.Operator, scanner.Scan ());
			AssertEquals ("/", scanner.Value);
			AssertEquals (XPathTokenType.NameTest, scanner.Scan ());
			AssertEquals ("bar:*", scanner.Value);
			AssertEquals (XPathTokenType.End, scanner.Scan ());
			AssertNull (scanner.Value);
		}

		public void TestAttributeNameTest ()
		{
			XPathScanner scanner = new XPathScanner ("@foo");
			AssertEquals (XPathTokenType.At, scanner.Scan ());
			AssertEquals ("@", scanner.Value);
			AssertEquals (XPathTokenType.NameTest, scanner.Scan ());
			AssertEquals ("foo", scanner.Value);
			AssertEquals (XPathTokenType.End, scanner.Scan ());
			AssertNull (scanner.Value);
		}

		public void TestNameTestAndAttributeNameTest ()
		{
			XPathScanner scanner = new XPathScanner ("foo/@bar");
			AssertEquals (XPathTokenType.NameTest, scanner.Scan ());
			AssertEquals ("foo", scanner.Value);
			AssertEquals (XPathTokenType.Operator, scanner.Scan ());
			AssertEquals ("/", scanner.Value);
			AssertEquals (XPathTokenType.At, scanner.Scan ());
			AssertEquals ("@", scanner.Value);
			AssertEquals (XPathTokenType.NameTest, scanner.Scan ());
			AssertEquals ("bar", scanner.Value);
			AssertEquals (XPathTokenType.End, scanner.Scan ());
			AssertNull (scanner.Value);
		}

		public void TestAttributeAxis ()
		{
			XPathScanner scanner = new XPathScanner ("attribute::foo");
			AssertEquals (XPathTokenType.AxisName, scanner.Scan ());
			AssertEquals ("attribute", scanner.Value);
			AssertEquals (XPathTokenType.ColonColon, scanner.Scan ());
			AssertEquals ("::", scanner.Value);
			AssertEquals (XPathTokenType.NameTest, scanner.Scan ());
			AssertEquals ("foo", scanner.Value);
			AssertEquals (XPathTokenType.End, scanner.Scan ());
			AssertNull (scanner.Value);
		}

		public void TestNameTestAndAttributeAxis ()
		{
			XPathScanner scanner = new XPathScanner ("foo/attribute::bar");
			AssertEquals (XPathTokenType.NameTest, scanner.Scan ());
			AssertEquals ("foo", scanner.Value);
			AssertEquals (XPathTokenType.Operator, scanner.Scan ());
			AssertEquals ("/", scanner.Value);
			AssertEquals (XPathTokenType.AxisName, scanner.Scan ());
			AssertEquals ("attribute", scanner.Value);
			AssertEquals (XPathTokenType.ColonColon, scanner.Scan ());
			AssertEquals ("::", scanner.Value);
			AssertEquals (XPathTokenType.NameTest, scanner.Scan ());
			AssertEquals ("bar", scanner.Value);
			AssertEquals (XPathTokenType.End, scanner.Scan ());
			AssertNull (scanner.Value);
		}

		public void TestRoot ()
		{
			XPathScanner scanner = new XPathScanner ("/");
			AssertEquals (XPathTokenType.Operator, scanner.Scan ());
			AssertEquals ("/", scanner.Value);
			AssertEquals (XPathTokenType.End, scanner.Scan ());
			AssertNull (scanner.Value);
		}

		public void TestAbsoluteNameTest ()
		{
			XPathScanner scanner = new XPathScanner ("/foo");
			AssertEquals (XPathTokenType.Operator, scanner.Scan ());
			AssertEquals ("/", scanner.Value);
			AssertEquals (XPathTokenType.NameTest, scanner.Scan ());
			AssertEquals ("foo", scanner.Value);
			AssertEquals (XPathTokenType.End, scanner.Scan ());
			AssertNull (scanner.Value);
		}

		public void TestAbbreviatedAbsoluteLocationPathWithNameTest ()
		{
			XPathScanner scanner = new XPathScanner ("//foo");
			AssertEquals (XPathTokenType.Operator, scanner.Scan ());
			AssertEquals ("//", scanner.Value);
			AssertEquals (XPathTokenType.NameTest, scanner.Scan ());
			AssertEquals ("foo", scanner.Value);
			AssertEquals (XPathTokenType.End, scanner.Scan ());
			AssertNull (scanner.Value);
		}

		public void TestAbbreviatedRelativeLocationPathWithNameTest ()
		{
			XPathScanner scanner = new XPathScanner ("foo//bar");
			AssertEquals (XPathTokenType.NameTest, scanner.Scan ());
			AssertEquals ("foo", scanner.Value);
			AssertEquals (XPathTokenType.Operator, scanner.Scan ());
			AssertEquals ("//", scanner.Value);
			AssertEquals (XPathTokenType.NameTest, scanner.Scan ());
			AssertEquals ("bar", scanner.Value);
			AssertEquals (XPathTokenType.End, scanner.Scan ());
			AssertNull (scanner.Value);
		}

		public void TestAbbreviatedStepSelf ()
		{
			XPathScanner scanner = new XPathScanner (".");
			AssertEquals (XPathTokenType.Dot, scanner.Scan ());
			AssertEquals (".", scanner.Value);
			AssertEquals (XPathTokenType.End, scanner.Scan ());
			AssertNull (scanner.Value);
		}

		public void TestAbbreviatedStepParent ()
		{
			XPathScanner scanner = new XPathScanner ("..");
			AssertEquals (XPathTokenType.DotDot, scanner.Scan ());
			AssertEquals ("..", scanner.Value);
			AssertEquals (XPathTokenType.End, scanner.Scan ());
			AssertNull (scanner.Value);
		}
	}
}
