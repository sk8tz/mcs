//
// ComboBoxTest.cs: Test cases for ComboBox.
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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//   	Ritvik Mayank <mritvik@novell.com>
//	Jordi Mas i Hernandez <jordi@ximian.com>
//


using System;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using NUnit.Framework;
using System.Collections;
using System.ComponentModel;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ListBoxTest
	{
		ListBox listBox;
		Form form;

		[SetUp]
		public void SetUp()
		{
			listBox = new ListBox();
			form = new Form();
		}

		[Test]
		public void ListBoxPropertyTest ()
		{
			Assert.AreEqual (0, listBox.ColumnWidth, "#1");
			Assert.AreEqual (DrawMode.Normal, listBox.DrawMode, "#2");
			Assert.AreEqual (0, listBox.HorizontalExtent, "#3");
			Assert.AreEqual (false, listBox.HorizontalScrollbar, "#4");
			Assert.AreEqual (true, listBox.IntegralHeight, "#5");
			//Assert.AreEqual (13, listBox.ItemHeight, "#6"); // Note: Item height depends on the current font.
			listBox.Items.Add ("a");
			listBox.Items.Add ("b");
			listBox.Items.Add ("c");
			Assert.AreEqual (3,  listBox.Items.Count, "#7");
			Assert.AreEqual (false, listBox.MultiColumn, "#8");
			//Assert.AreEqual (46, listBox.PreferredHeight, "#9"); // Note: Item height depends on the current font.
			//Assert.AreEqual (RightToLeft.No , listBox.RightToLeft, "#10"); // Depends on Windows version
			Assert.AreEqual (false, listBox.ScrollAlwaysVisible, "#11");
			Assert.AreEqual (-1, listBox.SelectedIndex, "#12");
			listBox.SetSelected (2,true);
			Assert.AreEqual (2, listBox.SelectedIndices[0], "#13");
			Assert.AreEqual ("c", listBox.SelectedItem, "#14");
			Assert.AreEqual ("c", listBox.SelectedItems[0], "#15");
			Assert.AreEqual (SelectionMode.One, listBox.SelectionMode, "#16");
			listBox.SetSelected (2,false);
			Assert.AreEqual (false, listBox.Sorted, "#17");
			Assert.AreEqual ("", listBox.Text, "#18");
			Assert.AreEqual (0, listBox.TopIndex, "#19");
			Assert.AreEqual (true, listBox.UseTabStops, "#20");
		}

		[Test]
		public void BeginEndUpdateTest ()
		{
			form.Visible = true;
			listBox.Items.Add ("A");
			listBox.Visible = true;
			form.Controls.Add (listBox);
			listBox.BeginUpdate ();
			for (int x = 1; x < 5000; x++)
			{
				listBox.Items.Add ("Item " + x.ToString ());
			}
			listBox.EndUpdate ();
			listBox.SetSelected (1, true);
			listBox.SetSelected (3, true);
			Assert.AreEqual (true, listBox.SelectedItems.Contains ("Item 3"), "#21");
		}

		[Test]
		public void ClearSelectedTest ()
		{
			form.Visible = true;
			listBox.Items.Add ("A");
			listBox.Visible = true;
			form.Controls.Add (listBox);
			listBox.SetSelected (0, true);
			Assert.AreEqual ("A", listBox.SelectedItems [0].ToString (),"#22");
			listBox.ClearSelected ();
			Assert.AreEqual (0, listBox.SelectedItems.Count,"#23");
		}

		[Ignore ("It depends on user system settings")]
		public void GetItemHeightTest ()
		{
			listBox.Visible = true;
			form.Controls.Add (listBox);
			listBox.Items.Add ("A");
			Assert.AreEqual (13, listBox.GetItemHeight (0) , "#28");
		}

		[Ignore ("It depends on user system settings")]
		public void GetItemRectangleTest ()
		{
			form.Visible = true;
			listBox.Visible = true;
			form.Controls.Add (listBox);
			listBox.Items.Add ("A");
			Assert.AreEqual (new Rectangle(0,0,116,13), listBox.GetItemRectangle (0), "#29");
		}

		[Test]
		public void GetSelectedTest ()
		{
			listBox.Items.Add ("A");
			listBox.Items.Add ("B");
			listBox.Items.Add ("C");
			listBox.Items.Add ("D");
			listBox.Sorted = true;
			listBox.SetSelected (0,true);
			listBox.SetSelected (2,true);
			listBox.TopIndex=0;
			Assert.AreEqual (true, listBox.GetSelected (0), "#30");
			listBox.SetSelected (2,false);
			Assert.AreEqual (false, listBox.GetSelected (2), "#31");
		}

		[Test]
		public void IndexFromPointTest ()
		{
			listBox.Items.Add ("A");
			Point pt = new Point (100,100);
				listBox.IndexFromPoint (pt);
			Assert.AreEqual (-1, listBox.IndexFromPoint (100,100), "#32");
		}

		[Test]
		public void FindStringTest ()
		{
			listBox.FindString ("Hola", -5); // No exception, it's empty
			int x = listBox.FindString ("Hello");
			Assert.AreEqual (-1, x, "#19");
			listBox.Items.AddRange(new object[] {"ACBD", "ABDC", "ACBD", "ABCD"});
			String myString = "ABC";
			x = listBox.FindString (myString);
			Assert.AreEqual (3, x, "#191");
			x = listBox.FindString (string.Empty);
			Assert.AreEqual (0, x, "#192");
			x = listBox.FindString ("NonExistant");
			Assert.AreEqual (-1, x, "#193");
		}

		[Test]
		public void FindStringExactTest ()
		{
			listBox.FindStringExact ("Hola", -5); // No exception, it's empty
			int x = listBox.FindStringExact ("Hello");
			Assert.AreEqual (-1, x, "#20");
			listBox.Items.AddRange (new object[] {"ABCD","ABC","ABDC"});
			String myString = "ABC";
			x = listBox.FindStringExact (myString);
			Assert.AreEqual (1, x, "#201");
			x = listBox.FindStringExact (string.Empty);
			Assert.AreEqual (-1, x, "#202");
			x = listBox.FindStringExact ("NonExistant");
			Assert.AreEqual (-1, x, "#203");
		}

		//
		// Exceptions
		//

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void BorderStyleException ()
		{
			listBox.BorderStyle = (BorderStyle) 10;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ColumnWidthException ()
		{
			listBox.ColumnWidth = -1;
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void DrawModeException ()
		{
			listBox.DrawMode = (DrawMode) 10;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DrawModeAndMultiColumnException ()
		{
			listBox.MultiColumn = true;
			listBox.DrawMode = DrawMode.OwnerDrawVariable;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ItemHeightException ()
		{
			listBox.ItemHeight = 256;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void SelectedIndexException ()
		{
			listBox.SelectedIndex = -2;
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void SelectedIndexModeNoneException ()
		{
			listBox.SelectionMode = SelectionMode.None;
			listBox.SelectedIndex = -1;
		}

		[Test]
		[ExpectedException (typeof (InvalidEnumArgumentException))]
		public void SelectionModeException ()
		{
			listBox.SelectionMode = (SelectionMode) 10;
		}

		[Test][ExpectedException(typeof(InvalidOperationException))]
		public void SelectedValueNull()
		{
			listBox.SelectedValue = null;
		}

		[Test][ExpectedException(typeof(InvalidOperationException))]
		public void SelectedValueEmptyString()
		{
			listBox.SelectedValue = String.Empty;
		}

		//
		// Events
		//
		private bool eventFired;

		private void GenericHandler (object sender,  EventArgs e)
		{
			eventFired = true;
		}


	}

	[TestFixture]
	public class ListBoxObjectCollectionTest
	{
		ListBox.ObjectCollection col;

		[SetUp]
		public void SetUp()
		{
			col = new ListBox.ObjectCollection (new ListBox ());
		}

		[Test]
		public void DefaultProperties ()
		{
			Assert.AreEqual (false, col.IsReadOnly, "#B1");
			Assert.AreEqual (false, ((ICollection)col).IsSynchronized, "#B2");
			Assert.AreEqual (col, ((ICollection)col).SyncRoot, "#B3");
			Assert.AreEqual (false, ((IList)col).IsFixedSize, "#B4");
			Assert.AreEqual (0, col.Count);
		}

		[Test]
		public void AddTest ()
		{
			col.Add ("Item1");
			col.Add ("Item2");
			Assert.AreEqual (2, col.Count, "#C1");
		}

		[Test]
		public void ClearTest ()
		{
			col.Add ("Item1");
			col.Add ("Item2");
			col.Clear ();
			Assert.AreEqual (0, col.Count, "#D1");
		}

		[Test]
		public void ContainsTest ()
		{
			object obj = "Item1";
			col.Add (obj);
			Assert.AreEqual (true, col.Contains ("Item1"), "#E1");
			Assert.AreEqual (false, col.Contains ("Item2"), "#E2");
		}

		[Test]
		public void IndexOfTest ()
		{
			col.Add ("Item1");
			col.Add ("Item2");
			Assert.AreEqual (1, col.IndexOf ("Item2"), "#F1");
		}

		[Test]
		public void RemoveTest ()
		{
			col.Add ("Item1");
			col.Add ("Item2");
			col.Remove ("Item1");
			Assert.AreEqual (1, col.Count, "#G1");
		}

		[Test]
		public void RemoveAtTest ()
		{
			col.Add ("Item1");
			col.Add ("Item2");
			col.RemoveAt (0);
			Assert.AreEqual (1, col.Count, "#H1");
			Assert.AreEqual (true, col.Contains ("Item2"), "#H1");
		}

	}
}
