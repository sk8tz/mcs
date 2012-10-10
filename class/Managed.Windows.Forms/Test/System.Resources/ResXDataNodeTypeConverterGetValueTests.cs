﻿// Permission is hereby granted, free of charge, to any person obtaining
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
// Copyright (c) 2012 Gary Barnett
//
// Authors:
//	Gary Barnett

#if NET_2_0
using System;
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Resources;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Collections;

using NUnit.Framework;
using System.ComponentModel.Design;
using System.Runtime.Serialization.Formatters.Binary;

namespace MonoTests.System.Resources
{
	[TestFixture]
	public class ResXDataNodeTypeConverterGetValueTests : MonoTests.System.Windows.Forms.TestHelper {
		string _tempDirectory;
		string _otherTempDirectory;
		/*
		[Test, ExpectedException (typeof (NotImplementedException))]
		public void GetValueParamIsTouchedWhenEmbeddedReturnedFromResXResourceReader ()
		{
			// after running the enumerator of ResXResourceReader with UseResXDataNodes set 
			// to true, check params supplied to GetValue method
			// of ResXDataNode are used to deserialise

			// for now just throwing exception in param object to ensure its accessed

			ResXDataNode originalNode, returnedNode;
				
			originalNode = GetNodeEmdeddedIcon ();

			string fileName = GetResXFileWithNode (originalNode);

			using (ResXResourceReader reader = new ResXResourceReader (fileName)) {
				reader.UseResXDataNodes = true;

				IDictionaryEnumerator enumerator = reader.GetEnumerator ();
				enumerator.MoveNext ();
				returnedNode = (ResXDataNode) ((DictionaryEntry) enumerator.Current).Value;

				Assert.IsNotNull (returnedNode, "#A1");

				Icon ico = (Icon)returnedNode.GetValue (new ExceptionalTypeResolutionService ());
			}
		}
		*/

		[Test]
		public void ITRSNotUsedWhenCreatedNew ()
		{
			// check supplying params to GetValue of the UseResXDataNode does not change the output
			// of the method for an instance created manually

			ResXDataNode node;
			node = new ResXDataNode ("along", 34L);
			object obj = node.GetValue (new AlwaysReturnIntTypeResolutionService ());
			Assert.IsInstanceOfType (typeof(long), obj, "#A1");
		}

		[Test]
		public void ITRSUsedEachTimeWithNodeFromReader ()
		{
			// check GetValue uses ITRS param each time its called for a node from a ResXResourceReader 
			// for an object stored by means of a typeconverter, 

			ResXDataNode returnedNode, originalNode;

			originalNode = new ResXDataNode ("aNumber", 23L);

			string fileName = GetResXFileWithNode (originalNode, "long.resx");

			// should load assembly referenced in file
			using (ResXResourceReader reader = new ResXResourceReader (fileName)) {
				reader.UseResXDataNodes = true;

				IDictionaryEnumerator enumerator = reader.GetEnumerator ();
				enumerator.MoveNext ();
				returnedNode = (ResXDataNode) ((DictionaryEntry) enumerator.Current).Value;

				Assert.IsNotNull (returnedNode, "#A1");

				object newVal = returnedNode.GetValue (new AlwaysReturnIntTypeResolutionService ());

				Assert.AreEqual (typeof (int).AssemblyQualifiedName, newVal.GetType ().AssemblyQualifiedName, "#A2");

				object origVal = returnedNode.GetValue ((ITypeResolutionService) null);

				Assert.AreEqual (typeof (long).AssemblyQualifiedName, origVal.GetType().AssemblyQualifiedName, "#A3");
			}
		}
		
		[TearDown]
		protected override void TearDown ()
		{
			//teardown
			if (Directory.Exists (_tempDirectory))
				Directory.Delete (_tempDirectory, true);

			base.TearDown ();
		}

		string GetResXFileWithNode (ResXDataNode node, string filename)
		{
			string fullfileName;

			_tempDirectory = Path.Combine (Path.GetTempPath (), "ResXDataNodeTest");
			_otherTempDirectory = Path.Combine (_tempDirectory, "in");
			if (!Directory.Exists (_otherTempDirectory)) {
				Directory.CreateDirectory (_otherTempDirectory);
			}

			fullfileName = Path.Combine (_tempDirectory, filename);

			using (ResXResourceWriter writer = new ResXResourceWriter (fullfileName)) {
				writer.AddResource (node);
			}

			return fullfileName;
		}


	}
}
#endif
