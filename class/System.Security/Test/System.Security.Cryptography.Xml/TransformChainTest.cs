//
// TransformChainTest.cs - NUnit Test Cases for TransformChain
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.Xml;
using System.Xml;

namespace MonoTests.System.Security.Cryptography.Xml {

public class TransformChainTest : TestCase {

	public TransformChainTest () : base ("System.Security.Cryptography.Xml.TransformChain testsuite") {}
	public TransformChainTest (string name) : base (name) {}

	protected TransformChain chain;

	protected override void SetUp () 
	{
		chain = new TransformChain ();
	}

	protected override void TearDown () {}

	public static ITest Suite {
		get { 
			return new TestSuite (typeof (TransformChainTest)); 
		}
	}

	public void TestEmpty () 
	{
		AssertEquals ("empty count", 0, chain.Count);
		AssertNotNull ("IEnumerator", chain.GetEnumerator ());
		AssertEquals ("ToString()", "System.Security.Cryptography.Xml.TransformChain", chain.ToString ());
	}

	public void TestChain () 
	{
		XmlDsigBase64Transform base64 = new XmlDsigBase64Transform ();
		chain.Add (base64);
		AssertEquals ("XmlDsigBase64Transform", base64, chain[0]);
		AssertEquals ("count 1", 1, chain.Count);

		XmlDsigC14NTransform c14n = new XmlDsigC14NTransform ();
		chain.Add (c14n);
		AssertEquals ("XmlDsigC14NTransform", c14n, chain[1]);
		AssertEquals ("count 2", 2, chain.Count);

		XmlDsigC14NWithCommentsTransform c14nc = new XmlDsigC14NWithCommentsTransform ();
		chain.Add (c14nc);
		AssertEquals ("XmlDsigC14NWithCommentsTransform", c14nc, chain[2]);
		AssertEquals ("count 3", 3, chain.Count);

		XmlDsigEnvelopedSignatureTransform esign = new XmlDsigEnvelopedSignatureTransform ();
		chain.Add (esign);
		AssertEquals ("XmlDsigEnvelopedSignatureTransform", esign, chain[3]);
		AssertEquals ("count 4", 4, chain.Count);

		XmlDsigXPathTransform xpath = new XmlDsigXPathTransform ();
		chain.Add (xpath);
		AssertEquals ("XmlDsigXPathTransform", xpath, chain[4]);
		AssertEquals ("count 5", 5, chain.Count);

		XmlDsigXsltTransform xslt = new XmlDsigXsltTransform ();
		chain.Add (xslt);
		AssertEquals ("XmlDsigXsltTransform", xslt, chain[5]);
		AssertEquals ("count 6", 6, chain.Count);
	}

}

}
