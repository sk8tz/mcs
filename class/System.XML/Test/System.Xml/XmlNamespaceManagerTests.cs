//
// XmlNamespaceManagerTests.cs
//
// Authors:
//   Jason Diamond (jason@injektilo.org)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
// (C) 2003 Martin Willemoes Hansen
//

using System;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlNamespaceManagerTests
	{
		private XmlNameTable nameTable;
		private XmlNamespaceManager namespaceManager;

		[SetUp]
		public void GetReady ()
		{
			nameTable = new NameTable ();
			namespaceManager = new XmlNamespaceManager (nameTable);
		}

		[Test]
		public void NewNamespaceManager ()
		{
			// make sure that you can call PopScope when there aren't any to pop.
			Assertion.Assert (!namespaceManager.PopScope ());

			// the following strings should have been added to the name table by the
			// namespace manager.
			string xmlnsPrefix = nameTable.Get ("xmlns");
			string xmlPrefix = nameTable.Get ("xml");
			string stringEmpty = nameTable.Get (String.Empty);
			string xmlnsNamespace = "http://www.w3.org/2000/xmlns/";
			string xmlNamespace = "http://www.w3.org/XML/1998/namespace";

			// none of them should be null.
			Assertion.AssertNotNull (xmlnsPrefix);
			Assertion.AssertNotNull (xmlPrefix);
			Assertion.AssertNotNull (stringEmpty);
			Assertion.AssertNotNull (xmlnsNamespace);
			Assertion.AssertNotNull (xmlNamespace);

			// Microsoft's XmlNamespaceManager reports that these three
			// namespaces aren't declared for some reason.
			Assertion.Assert (!namespaceManager.HasNamespace ("xmlns"));
			Assertion.Assert (!namespaceManager.HasNamespace ("xml"));
			Assertion.Assert (!namespaceManager.HasNamespace (String.Empty));

			// these three namespaces are declared by default.
			Assertion.AssertEquals ("http://www.w3.org/2000/xmlns/", namespaceManager.LookupNamespace ("xmlns"));
			Assertion.AssertEquals ("http://www.w3.org/XML/1998/namespace", namespaceManager.LookupNamespace ("xml"));
			Assertion.AssertEquals (String.Empty, namespaceManager.LookupNamespace (String.Empty));

			// the namespaces should be the same references found in the name table.
			Assertion.AssertSame (xmlnsNamespace, namespaceManager.LookupNamespace ("xmlns"));
			Assertion.AssertSame (xmlNamespace, namespaceManager.LookupNamespace ("xml"));
			Assertion.AssertSame (stringEmpty, namespaceManager.LookupNamespace (String.Empty));

			// looking up undeclared namespaces should return null.
			Assertion.AssertNull (namespaceManager.LookupNamespace ("foo"));
		}

		[Test]
		public void AddNamespace ()
		{
			// add a new namespace.
			namespaceManager.AddNamespace ("foo", "http://foo/");
			// make sure the new namespace is there.
			Assertion.Assert (namespaceManager.HasNamespace ("foo"));
			Assertion.AssertEquals ("http://foo/", namespaceManager.LookupNamespace ("foo"));
		}

		[Test]
		public void AddNamespaceWithNameTable ()
		{
			// add a known reference to the name table.
			string fooNamespace = "http://foo/";
			nameTable.Add(fooNamespace);

			// create a new string with the same value but different address.
			string fooNamespace2 = "http://";
			fooNamespace2 += "foo/";

			// the references must be different in order for this test to prove anything.
			Assertion.Assert (!Object.ReferenceEquals (fooNamespace, fooNamespace2));

			// add the namespace with the reference that's not in the name table.
			namespaceManager.AddNamespace ("foo", fooNamespace2);

			// the returned reference should be the same one that's in the name table.
			Assertion.AssertSame (fooNamespace, namespaceManager.LookupNamespace ("foo"));
		}

		[Test]
		public void PushScope ()
		{
			// add a new namespace.
			namespaceManager.AddNamespace ("foo", "http://foo/");
			// make sure the new namespace is there.
			Assertion.Assert (namespaceManager.HasNamespace ("foo"));
			Assertion.AssertEquals ("http://foo/", namespaceManager.LookupNamespace ("foo"));
			// push a new scope.
			namespaceManager.PushScope ();
			// add a new namespace.
			namespaceManager.AddNamespace ("bar", "http://bar/");
			// make sure the old namespace is not in this new scope.
			Assertion.Assert (!namespaceManager.HasNamespace ("foo"));
			// but we're still supposed to be able to lookup the old namespace.
			Assertion.AssertEquals ("http://foo/", namespaceManager.LookupNamespace ("foo"));
			// make sure the new namespace is there.
			Assertion.Assert (namespaceManager.HasNamespace ("bar"));
			Assertion.AssertEquals ("http://bar/", namespaceManager.LookupNamespace ("bar"));
		}

		[Test]
		public void PopScope ()
		{
			// add some namespaces and a scope.
			PushScope ();
			// pop the scope.
			Assertion.Assert (namespaceManager.PopScope ());
			// make sure the first namespace is still there.
			Assertion.Assert (namespaceManager.HasNamespace ("foo"));
			Assertion.AssertEquals ("http://foo/", namespaceManager.LookupNamespace ("foo"));
			// make sure the second namespace is no longer there.
			Assertion.Assert (!namespaceManager.HasNamespace ("bar"));
			Assertion.AssertNull (namespaceManager.LookupNamespace ("bar"));
			// make sure there are no more scopes to pop.
			Assertion.Assert (!namespaceManager.PopScope ());
			// make sure that popping again doesn't cause an exception.
			Assertion.Assert (!namespaceManager.PopScope ());
		}

		[Test]
		public void LookupPrefix ()
		{
			// This test should use an empty nametable.
			XmlNamespaceManager nsmgr =
				new XmlNamespaceManager (new NameTable ());
			nsmgr.NameTable.Add ("urn:hoge");
			nsmgr.NameTable.Add ("urn:fuga");
			nsmgr.AddNamespace (string.Empty, "urn:hoge");
			Assertion.AssertNull (nsmgr.LookupPrefix ("urn:fuga"));
			Assertion.AssertEquals (String.Empty, nsmgr.LookupPrefix ("urn:hoge"));
		}
	}
}
