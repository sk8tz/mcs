//
// System.Xml.XmlNodeTests
//
// Author:
//   Kral Ferch <kral_ferch@hotmail.com>
//
// (C) 2002 Kral Ferch
//

using System;
using System.IO;
using System.Text;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.Xml
{
	public class XmlNodeTests : TestCase
	{
		public XmlNodeTests () : base ("MonoTests.System.Xml.XmlNodeTests testsuite") {}
		public XmlNodeTests (string name) : base (name) {}

		XmlDocument document;
		XmlElement element;
		XmlElement element2;
		bool inserted;
		bool inserting;
		bool removed;
		bool removing;

		protected override void SetUp ()
		{
			document = new XmlDocument ();
			document.NodeInserted += new XmlNodeChangedEventHandler (this.EventNodeInserted);
			document.NodeInserting += new XmlNodeChangedEventHandler (this.EventNodeInserting);
			document.NodeRemoved += new XmlNodeChangedEventHandler (this.EventNodeRemoved);
			document.NodeRemoving += new XmlNodeChangedEventHandler (this.EventNodeRemoving);
			element = document.CreateElement ("foo");
			element2 = document.CreateElement ("bar");
		}

		private void EventNodeInserted(Object sender, XmlNodeChangedEventArgs e)
		{
			inserted = true;
		}

		private void EventNodeInserting (Object sender, XmlNodeChangedEventArgs e)
		{
			inserting = true;
		}

		private void EventNodeRemoved(Object sender, XmlNodeChangedEventArgs e)
		{
			removed = true;
		}

		private void EventNodeRemoving (Object sender, XmlNodeChangedEventArgs e)
		{
			removing = true;
		}

		public void TestAppendChild ()
		{
			XmlComment comment;

			inserted = false;
			inserting = false;
			element.AppendChild (element2);
			Assert (inserted);
			Assert (inserting);

			// Can only append to elements, documents, and attributes
			try 
			{
				comment = document.CreateComment ("baz");
				comment.AppendChild (element2);
				Fail ("Expected an InvalidOperationException to be thrown.");
			} 
			catch (InvalidOperationException) {}

			// Can't append a node from one document into another document.
			XmlDocument document2 = new XmlDocument();
			AssertEquals (1, element.ChildNodes.Count);
			try 
			{
				element2 = document2.CreateElement ("qux");
				element.AppendChild (element2);
				Fail ("Expected an ArgumentException to be thrown.");
			} 
			catch (ArgumentException) {}
			AssertEquals (1, element.ChildNodes.Count);

			// Can't append to a readonly node.
/* TODO put this in when I figure out how to create a read-only node.
			try 
			{
				XmlElement element3 = (XmlElement)element.CloneNode (false);
				Assert (!element.IsReadOnly);
				Assert (element3.IsReadOnly);
				element2 = document.CreateElement ("quux");
				element3.AppendChild (element2);
				Fail ("Expected an ArgumentException to be thrown.");
			} 
			catch (ArgumentException) {}
*/
		}

		public void TestInsertBefore()
		{
			document = new XmlDocument();
			document.LoadXml("<root><sub /></root>");
			XmlElement docelem = document.DocumentElement;
			docelem.InsertBefore(document.CreateElement("good_child"), docelem.FirstChild);
			AssertEquals("InsertBefore.Normal", "good_child", docelem.FirstChild.Name);
			try {
				document.InsertBefore(document.CreateElement("BAD_MAN"), docelem);
				Fail("#InsertBefore.BadPositionButNoError.1");
			}
			catch(XmlException) {}
		}

		public void TestInsertAfter()
		{
			document = new XmlDocument();
			document.LoadXml("<root><sub1 /><sub2 /></root>");
			XmlElement docelem = document.DocumentElement;
			XmlElement newelem = document.CreateElement("good_child");
			docelem.InsertAfter(newelem, docelem.FirstChild);
			AssertEquals("InsertAfter.Normal", 3, docelem.ChildNodes.Count);
			AssertEquals("InsertAfter.First", "sub1", docelem.FirstChild.Name);
			AssertEquals("InsertAfter.Last", "sub2", docelem.LastChild.Name);
			AssertEquals("InsertAfter.Prev", "good_child", docelem.FirstChild.NextSibling.Name);
			AssertEquals("InsertAfter.Next", "good_child", docelem.LastChild.PreviousSibling.Name);
			try 
			{
				document.InsertAfter(document.CreateElement("BAD_MAN"), docelem);
				Fail("#InsertAfter.BadPositionButNoError.1");
			}
			catch(XmlException) {}
		}

		public void TestPrependChild()
		{
			document = new XmlDocument();
			document.LoadXml("<root><sub1 /><sub2 /></root>");
			XmlElement docelem = document.DocumentElement;
			docelem.PrependChild(document.CreateElement("prepender"));
			AssertEquals("PrependChild", "prepender", docelem.FirstChild.Name);
		}

		public void saveTestRemoveAll ()
		{
			// TODO:  put this test back in when AttributeCollection.RemoveAll() is implemented.
			element.AppendChild(element2);
			removed = false;
			removing = false;
			element.RemoveAll ();
			Assert (removed);
			Assert (removing);
		}

		public void TestRemoveChild ()
		{
			element.AppendChild(element2);
			removed = false;
			removing = false;
			element.RemoveChild (element2);
			Assert (removed);
			Assert (removing);
		}
	}
}
