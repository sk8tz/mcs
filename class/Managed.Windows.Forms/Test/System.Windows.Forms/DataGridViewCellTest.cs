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
// Author:
//	Pedro Martínez Juliá <pedromj@gmail.com>
//


#if NET_2_0

using NUnit.Framework;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections;

namespace MonoTests.System.Windows.Forms {

	[TestFixture]
	public class DataGridViewCellTest {
		
		[SetUp]
		public void GetReady() {}

		[TearDown]
		public void Clean() {}

		[Test]
		public void GetClipboardContentTest ()
		{
			DataGridViewClipboardCell cell = new DataGridViewClipboardCell ();
			
			cell.Value = "abc";
			Assert.IsNull (cell.GetClipboardContentPublic (0, false, false, false, false, DataFormats.Text), "#01");
			
			using (DataGridView dgv = new DataGridView ()) {
				dgv.Columns.Add ("A", "A");
				DataGridViewRow row = new DataGridViewRow ();
				row.Cells.Add (cell);
				dgv.Rows.Add (row);
				cell.Selected = true;

				Assert.AreEqual ("abc\t", cell.GetClipboardContentPublic (0, false, false, false, false, DataFormats.Text), "#A1");
				Assert.AreEqual ("abc\t", cell.GetClipboardContentPublic (0, false, false, false, false, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TD>abc</TD>", cell.GetClipboardContentPublic (0, false, false, false, false, DataFormats.Html), "#A3");
				Assert.AreEqual ("abc,", cell.GetClipboardContentPublic (0, false, false, false, false, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual ("abc\t", cell.GetClipboardContentPublic (0, true, false, false, false, DataFormats.Text), "#A1");
				Assert.AreEqual ("abc\t", cell.GetClipboardContentPublic (0, true, false, false, false, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TR><TD>abc</TD>", cell.GetClipboardContentPublic (0, true, false, false, false, DataFormats.Html), "#A3");
				Assert.AreEqual ("abc,", cell.GetClipboardContentPublic (0, true, false, false, false, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual (string.Format("abc{0}", Environment.NewLine), cell.GetClipboardContentPublic (0, false, true, false, false, DataFormats.Text), "#A1");
				Assert.AreEqual (string.Format ("abc{0}", Environment.NewLine), cell.GetClipboardContentPublic (0, false, true, false, false, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TD>abc</TD></TR>", cell.GetClipboardContentPublic (0, false, true, false, false, DataFormats.Html), "#A3");
				Assert.AreEqual (string.Format ("abc{0}", Environment.NewLine), cell.GetClipboardContentPublic (0, false, true, false, false, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual ("abc\t", cell.GetClipboardContentPublic (0, false, false, true, false, DataFormats.Text), "#A1");
				Assert.AreEqual ("abc\t", cell.GetClipboardContentPublic (0, false, false, true, false, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TD>abc</TD>", cell.GetClipboardContentPublic (0, false, false, true, false, DataFormats.Html), "#A3");
				Assert.AreEqual ("abc,", cell.GetClipboardContentPublic (0, false, false, true, false, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual ("abc\t", cell.GetClipboardContentPublic (0, false, false, false, true, DataFormats.Text), "#A1");
				Assert.AreEqual ("abc\t", cell.GetClipboardContentPublic (0, false, false, false, true, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TD>abc</TD>", cell.GetClipboardContentPublic (0, false, false, false, true, DataFormats.Html), "#A3");
				Assert.AreEqual ("abc,", cell.GetClipboardContentPublic (0, false, false, false, true, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual ("abc", cell.GetClipboardContentPublic (0, true, true, true, true, DataFormats.Text), "#A1");
				Assert.AreEqual ("abc", cell.GetClipboardContentPublic (0, true, true, true, true, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TABLE><TR><TD>abc</TD></TR></TABLE>", cell.GetClipboardContentPublic (0, true, true, true, true, DataFormats.Html), "#A3");
				Assert.AreEqual ("abc", cell.GetClipboardContentPublic (0, true, true, true, true, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual ("abc", cell.GetClipboardContentPublic (0, false, true, true, true, DataFormats.Text), "#A1");
				Assert.AreEqual ("abc", cell.GetClipboardContentPublic (0, false, true, true, true, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TD>abc</TD></TR></TABLE>", cell.GetClipboardContentPublic (0, false, true, true, true, DataFormats.Html), "#A3");
				Assert.AreEqual ("abc", cell.GetClipboardContentPublic (0, false, true, true, true, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual ("abc\t", cell.GetClipboardContentPublic (0, true, false, true, true, DataFormats.Text), "#A1");
				Assert.AreEqual ("abc\t", cell.GetClipboardContentPublic (0, true, false, true, true, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TABLE><TR><TD>abc</TD>", cell.GetClipboardContentPublic (0, true, false, true, true, DataFormats.Html), "#A3");
				Assert.AreEqual ("abc,", cell.GetClipboardContentPublic (0, true, false, true, true, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual ("abc", cell.GetClipboardContentPublic (0, true, true, false, true, DataFormats.Text), "#A1");
				Assert.AreEqual ("abc", cell.GetClipboardContentPublic (0, true, true, false, true, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TR><TD>abc</TD></TR></TABLE>", cell.GetClipboardContentPublic (0, true, true, false, true, DataFormats.Html), "#A3");
				Assert.AreEqual ("abc", cell.GetClipboardContentPublic (0, true, true, false, true, DataFormats.CommaSeparatedValue), "#A4");
				
				Assert.AreEqual ("abc" + Environment.NewLine, cell.GetClipboardContentPublic (0, true, true, true, false, DataFormats.Text), "#A1");
				Assert.AreEqual ("abc" + Environment.NewLine, cell.GetClipboardContentPublic (0, true, true, true, false, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TABLE><TR><TD>abc</TD></TR>", cell.GetClipboardContentPublic (0, true, true, true, false, DataFormats.Html), "#A3");
				Assert.AreEqual ("abc" + Environment.NewLine, cell.GetClipboardContentPublic (0, true, true, true, false, DataFormats.CommaSeparatedValue), "#A4");
				
				cell.Selected = false;

				Assert.AreEqual ("\t", cell.GetClipboardContentPublic (0, false, false, false, false, DataFormats.Text), "#A1");
				Assert.AreEqual ("\t", cell.GetClipboardContentPublic (0, false, false, false, false, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TD>&nbsp;</TD>", cell.GetClipboardContentPublic (0, false, false, false, false, DataFormats.Html), "#A3");
				Assert.AreEqual (",", cell.GetClipboardContentPublic (0, false, false, false, false, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual ("\t", cell.GetClipboardContentPublic (0, true, false, false, false, DataFormats.Text), "#A1");
				Assert.AreEqual ("\t", cell.GetClipboardContentPublic (0, true, false, false, false, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TR><TD>&nbsp;</TD>", cell.GetClipboardContentPublic (0, true, false, false, false, DataFormats.Html), "#A3");
				Assert.AreEqual (",", cell.GetClipboardContentPublic (0, true, false, false, false, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual (string.Format ("{0}", Environment.NewLine), cell.GetClipboardContentPublic (0, false, true, false, false, DataFormats.Text), "#A1");
				Assert.AreEqual (string.Format ("{0}", Environment.NewLine), cell.GetClipboardContentPublic (0, false, true, false, false, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TD>&nbsp;</TD></TR>", cell.GetClipboardContentPublic (0, false, true, false, false, DataFormats.Html), "#A3");
				Assert.AreEqual (string.Format ("{0}", Environment.NewLine), cell.GetClipboardContentPublic (0, false, true, false, false, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual ("\t", cell.GetClipboardContentPublic (0, false, false, true, false, DataFormats.Text), "#A1");
				Assert.AreEqual ("\t", cell.GetClipboardContentPublic (0, false, false, true, false, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TD>&nbsp;</TD>", cell.GetClipboardContentPublic (0, false, false, true, false, DataFormats.Html), "#A3");
				Assert.AreEqual (",", cell.GetClipboardContentPublic (0, false, false, true, false, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual ("\t", cell.GetClipboardContentPublic (0, false, false, false, true, DataFormats.Text), "#A1");
				Assert.AreEqual ("\t", cell.GetClipboardContentPublic (0, false, false, false, true, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TD>&nbsp;</TD>", cell.GetClipboardContentPublic (0, false, false, false, true, DataFormats.Html), "#A3");
				Assert.AreEqual (",", cell.GetClipboardContentPublic (0, false, false, false, true, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual ("", cell.GetClipboardContentPublic (0, true, true, true, true, DataFormats.Text), "#A1");
				Assert.AreEqual ("", cell.GetClipboardContentPublic (0, true, true, true, true, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TABLE><TR><TD>&nbsp;</TD></TR></TABLE>", cell.GetClipboardContentPublic (0, true, true, true, true, DataFormats.Html), "#A3");
				Assert.AreEqual ("", cell.GetClipboardContentPublic (0, true, true, true, true, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual ("", cell.GetClipboardContentPublic (0, false, true, true, true, DataFormats.Text), "#A1");
				Assert.AreEqual ("", cell.GetClipboardContentPublic (0, false, true, true, true, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TD>&nbsp;</TD></TR></TABLE>", cell.GetClipboardContentPublic (0, false, true, true, true, DataFormats.Html), "#A3");
				Assert.AreEqual ("", cell.GetClipboardContentPublic (0, false, true, true, true, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual ("\t", cell.GetClipboardContentPublic (0, true, false, true, true, DataFormats.Text), "#A1");
				Assert.AreEqual ("\t", cell.GetClipboardContentPublic (0, true, false, true, true, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TABLE><TR><TD>&nbsp;</TD>", cell.GetClipboardContentPublic (0, true, false, true, true, DataFormats.Html), "#A3");
				Assert.AreEqual (",", cell.GetClipboardContentPublic (0, true, false, true, true, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual ("", cell.GetClipboardContentPublic (0, true, true, false, true, DataFormats.Text), "#A1");
				Assert.AreEqual ("", cell.GetClipboardContentPublic (0, true, true, false, true, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TR><TD>&nbsp;</TD></TR></TABLE>", cell.GetClipboardContentPublic (0, true, true, false, true, DataFormats.Html), "#A3");
				Assert.AreEqual ("", cell.GetClipboardContentPublic (0, true, true, false, true, DataFormats.CommaSeparatedValue), "#A4");

				Assert.AreEqual ("" + Environment.NewLine, cell.GetClipboardContentPublic (0, true, true, true, false, DataFormats.Text), "#A1");
				Assert.AreEqual ("" + Environment.NewLine, cell.GetClipboardContentPublic (0, true, true, true, false, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TABLE><TR><TD>&nbsp;</TD></TR>", cell.GetClipboardContentPublic (0, true, true, true, false, DataFormats.Html), "#A3");
				Assert.AreEqual ("" + Environment.NewLine, cell.GetClipboardContentPublic (0, true, true, true, false, DataFormats.CommaSeparatedValue), "#A4");
				
			}
		}
		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void GetClipboardContentTestException ()
		{
			DataGridViewClipboardCell cell = new DataGridViewClipboardCell ();

			cell.Value = "abc";
			Assert.IsNull (cell.GetClipboardContentPublic (0, false, false, false, false, DataFormats.Text), "#01");

			using (DataGridView dgv = new DataGridView ()) {
				dgv.Columns.Add ("A", "A");
				DataGridViewRow row = new DataGridViewRow ();
				row.Cells.Add (cell);
				dgv.Rows.Add (row);
				cell.Selected = true;

				Assert.AreEqual ("abc\t", cell.GetClipboardContentPublic (123, false, false, false, false, DataFormats.Text), "#A1");
				Assert.AreEqual ("abc\t", cell.GetClipboardContentPublic (123, false, false, false, false, DataFormats.UnicodeText), "#A2");
				Assert.AreEqual ("<TD>abc</TD>", cell.GetClipboardContentPublic (123, false, false, false, false, DataFormats.Html), "#A3");
				Assert.AreEqual ("abc,", cell.GetClipboardContentPublic (123, false, false, false, false, DataFormats.CommaSeparatedValue), "#A4");

			}
		}

		[Test]
		public void ReadOnlyTest ()
		{
			using (DataGridView dgv = DataGridViewCommon.CreateAndFill ()) {
				Assert.IsFalse (dgv.ReadOnly, "1-DGV");
				Assert.IsFalse (dgv.Rows [0].ReadOnly, "1-R0");
				Assert.IsFalse (dgv.Rows [1].ReadOnly, "1-R1");
				Assert.IsFalse (dgv.Columns [0].ReadOnly, "1-C1");
				Assert.IsFalse (dgv.Columns [1].ReadOnly, "1-C1");
				Assert.IsFalse (dgv [0, 0].ReadOnly, "1-C00");
				Assert.IsFalse (dgv [1, 0].ReadOnly, "1-C10");
				Assert.IsFalse (dgv [0, 1].ReadOnly, "1-C01");
				Assert.IsFalse (dgv [1, 1].ReadOnly, "1-C11");
			}


			using (DataGridView dgv = DataGridViewCommon.CreateAndFill ()) {
				dgv.ReadOnly = true;
				Assert.IsTrue (dgv.ReadOnly, "2-DGV");
				Assert.IsTrue (dgv.Rows [0].ReadOnly, "2-R0");
				Assert.IsTrue (dgv.Rows [1].ReadOnly, "2-R1");
				Assert.IsTrue (dgv.Columns [0].ReadOnly, "2-C1");
				Assert.IsTrue (dgv.Columns [1].ReadOnly, "2-C1");
				Assert.IsTrue (dgv [0, 0].ReadOnly, "2-C00");
				Assert.IsTrue (dgv [1, 0].ReadOnly, "2-C10");
				Assert.IsTrue (dgv [0, 1].ReadOnly, "2-C01");
				Assert.IsTrue (dgv [1, 1].ReadOnly, "2-C11");
			}

			// If the DGV is ReadOnly, everything is ReadOnly.
			using (DataGridView dgv = DataGridViewCommon.CreateAndFill ()) {
				dgv.ReadOnly = true;
				Assert.IsTrue (dgv.ReadOnly, "3-DGV");
				dgv.Rows [0].ReadOnly = false;
				Assert.IsTrue (dgv.Rows [0].ReadOnly, "3-R0");
				Assert.IsTrue (dgv.Rows [1].ReadOnly, "3-R1");
				dgv.Columns [0].ReadOnly = false;
				Assert.IsTrue (dgv.Columns [0].ReadOnly, "3-C1");
				Assert.IsTrue (dgv.Columns [1].ReadOnly, "3-C1");
				dgv [0, 0].ReadOnly = false;
				Assert.IsTrue (dgv [0, 0].ReadOnly, "3-C00");
				Assert.IsTrue (dgv [1, 0].ReadOnly, "3-C10");
				Assert.IsTrue (dgv [0, 1].ReadOnly, "3-C01");
				Assert.IsTrue (dgv [1, 1].ReadOnly, "3-C11");
			}

			using (DataGridView dgv = DataGridViewCommon.CreateAndFill ()) {
				Assert.IsFalse (dgv.ReadOnly, "4-DGV");
				dgv.Rows [0].ReadOnly = true;
				Assert.IsTrue (dgv.Rows [0].ReadOnly, "4-R0");
				Assert.IsFalse (dgv.Rows [1].ReadOnly, "4-R1");
				dgv.Columns [0].ReadOnly = true;
				Assert.IsTrue (dgv.Columns [0].ReadOnly, "4-C1");
				Assert.IsFalse (dgv.Columns [1].ReadOnly, "4-C1");
				Assert.IsTrue (dgv [0, 0].ReadOnly, "4-C00");
				Assert.IsTrue (dgv [1, 0].ReadOnly, "4-C10");
				Assert.IsTrue (dgv [0, 1].ReadOnly, "4-C01");
				Assert.IsFalse (dgv [1, 1].ReadOnly, "4-C11");
			}

			using (DataGridView dgv = DataGridViewCommon.CreateAndFill ()) {
				Assert.IsFalse (dgv.ReadOnly, "5-DGV");
				dgv.Rows [0].ReadOnly = true;
				Assert.IsTrue (dgv.Rows [0].ReadOnly, "5-R0");
				Assert.IsFalse (dgv.Rows [1].ReadOnly, "5-R1");
				dgv.Columns [0].ReadOnly = true;
				Assert.IsTrue (dgv.Columns [0].ReadOnly, "5-C1");
				Assert.IsFalse (dgv.Columns [1].ReadOnly, "5-C1");
				dgv [0, 0].ReadOnly = false; // Cell override
				Assert.IsFalse (dgv [0, 0].ReadOnly, "5-C00");
				Assert.IsTrue (dgv [1, 0].ReadOnly, "5-C10");
				Assert.IsTrue (dgv [0, 1].ReadOnly, "5-C01");
				Assert.IsFalse (dgv [1, 1].ReadOnly, "5-C11");
			}
		}

		[Test]
		public void EditTypeTest ()
		{
			DataGridViewCell cell =new DataGridViewCellMockObject ();
			Assert.AreEqual ("DataGridViewTextBoxEditingControl", cell.EditType.Name, "#01");
		}
		[Test]
		public void TestDefaultValues ()
		{
			DataGridViewCell cell = new DataGridViewCellMockObject ();

			Assert.IsNotNull (cell.AccessibilityObject, "#cell.AccessibilityObject");
			Assert.AreEqual (-1, cell.ColumnIndex, "#cell.ColumnIndex");
			Assert.IsNotNull (cell.ContentBounds, "#cell.ContentBounds");
			Assert.IsNull (cell.ContextMenuStrip, "#cell.ContextMenuStrip");
			Assert.IsNull (cell.DataGridView, "#cell.DataGridView");
			Assert.IsNull (cell.DefaultNewRowValue, "#cell.DefaultNewRowValue");
			Assert.AreEqual (false, cell.Displayed, "#cell.Displayed");
			Assert.IsNull (cell.EditedFormattedValue, "#cell.EditedFormattedValue");
			Assert.IsNotNull (cell.EditType, "#cell.EditType");
			try {
				object zxf = cell.ErrorIconBounds;
				TestHelper.RemoveWarning (zxf);
				Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#cell.ErrorIconBounds");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (@"Cell is not in a DataGridView. The cell cannot retrieve the inherited cell style.", ex.Message);
			} catch (Exception ex) {
				Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#cell.ErrorIconBounds");
			}
			Assert.AreEqual (@"", cell.ErrorText, "#cell.ErrorText");
			Assert.IsNull (cell.FormattedValue, "#cell.FormattedValue");
			Assert.IsNull (cell.FormattedValueType, "#cell.FormattedValueType");
			Assert.AreEqual (false, cell.Frozen, "#cell.Frozen");
			Assert.AreEqual (false, cell.HasStyle, "#cell.HasStyle");
			Assert.AreEqual (DataGridViewElementStates.ResizableSet, cell.InheritedState, "#cell.InheritedState");
			try {
				object zxf = cell.InheritedStyle;
				TestHelper.RemoveWarning (zxf);
				Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#cell.InheritedStyle");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (@"Cell is not in a DataGridView. The cell cannot retrieve the inherited cell style.", ex.Message);
			} catch (Exception ex) {
				Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#cell.InheritedStyle");
			}
			Assert.AreEqual (false, cell.IsInEditMode, "#cell.IsInEditMode");
			Assert.IsNull (cell.OwningColumn, "#cell.OwningColumn");
			Assert.IsNull (cell.OwningRow, "#cell.OwningRow");
			Assert.IsNotNull (cell.PreferredSize, "#cell.PreferredSize");
			Assert.AreEqual (false, cell.ReadOnly, "#cell.ReadOnly");
			Assert.AreEqual (false, cell.Resizable, "#cell.Resizable");
			Assert.AreEqual (-1, cell.RowIndex, "#cell.RowIndex");
			Assert.AreEqual (false, cell.Selected, "#cell.Selected");
			Assert.IsNotNull (cell.Size, "#cell.Size");
			Assert.AreEqual (DataGridViewElementStates.None, cell.State, "#cell.State");
			Assert.IsNotNull (cell.Style, "#cell.Style");
			Assert.IsNull (cell.Tag, "#cell.Tag");
			Assert.AreEqual (@"", cell.ToolTipText, "#cell.ToolTipText");
			Assert.IsNull (cell.Value, "#cell.Value");
			Assert.IsNull (cell.ValueType, "#cell.ValueType");
			Assert.AreEqual (false, cell.Visible, "#cell.Visible");
		}

		[Test]
		public void AddRow_Changes ()
		{

			using (DataGridView dgv = new DataGridView ()) {
				DataGridViewColumn col = new DataGridViewComboBoxColumn ();
				DataGridViewRow row = new DataGridViewRow ();
				DataGridViewCell cell = new DataGridViewComboBoxCell ();

				Assert.IsNotNull (cell.AccessibilityObject, "#A cell.AccessibilityObject");
				Assert.AreEqual (-1, cell.ColumnIndex, "#A cell.ColumnIndex");
				Assert.IsNotNull (cell.ContentBounds, "#A cell.ContentBounds");
				Assert.IsNull (cell.ContextMenuStrip, "#A cell.ContextMenuStrip");
				Assert.IsNull (cell.DataGridView, "#A cell.DataGridView");
				Assert.IsNull (cell.DefaultNewRowValue, "#A cell.DefaultNewRowValue");
				Assert.AreEqual (false, cell.Displayed, "#A cell.Displayed");
				Assert.IsNull (cell.EditedFormattedValue, "#A cell.EditedFormattedValue");
				Assert.IsNotNull (cell.EditType, "#A cell.EditType");
				try {
					object zxf = cell.ErrorIconBounds;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#A cell.ErrorIconBounds");
				} catch (InvalidOperationException ex) {
					Assert.AreEqual (@"Cell is not in a DataGridView. The cell cannot retrieve the inherited cell style.", ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#A cell.ErrorIconBounds");
				}
				Assert.AreEqual (@"", cell.ErrorText, "#A cell.ErrorText");
				Assert.IsNull (cell.FormattedValue, "#A cell.FormattedValue");
				Assert.IsNotNull (cell.FormattedValueType, "#A cell.FormattedValueType");
				Assert.AreEqual (false, cell.Frozen, "#A cell.Frozen");
				Assert.AreEqual (false, cell.HasStyle, "#A cell.HasStyle");
				Assert.AreEqual (DataGridViewElementStates.ResizableSet, cell.InheritedState, "#A cell.InheritedState");
				try {
					object zxf = cell.InheritedStyle;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#A cell.InheritedStyle");
				} catch (InvalidOperationException ex) {
					Assert.AreEqual (@"Cell is not in a DataGridView. The cell cannot retrieve the inherited cell style.", ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#A cell.InheritedStyle");
				}
				Assert.AreEqual (false, cell.IsInEditMode, "#A cell.IsInEditMode");
				Assert.IsNull (cell.OwningColumn, "#A cell.OwningColumn");
				Assert.IsNull (cell.OwningRow, "#A cell.OwningRow");
				Assert.IsNotNull (cell.PreferredSize, "#A cell.PreferredSize");
				Assert.AreEqual (false, cell.ReadOnly, "#A cell.ReadOnly");
				Assert.AreEqual (false, cell.Resizable, "#A cell.Resizable");
				Assert.AreEqual (-1, cell.RowIndex, "#A cell.RowIndex");
				Assert.AreEqual (false, cell.Selected, "#A cell.Selected");
				Assert.IsNotNull (cell.Size, "#A cell.Size");
				Assert.AreEqual (DataGridViewElementStates.None, cell.State, "#A cell.State");
				if (cell.HasStyle)
					Assert.IsNotNull (cell.Style, "#A cell.Style");
				Assert.IsNull (cell.Tag, "#A cell.Tag");
				Assert.AreEqual (@"", cell.ToolTipText, "#A cell.ToolTipText");
				Assert.IsNull (cell.Value, "#A cell.Value");
				Assert.IsNotNull (cell.ValueType, "#A cell.ValueType");
				Assert.AreEqual (false, cell.Visible, "#A cell.Visible");
				
				row.Cells.Add (cell);

				Assert.IsNotNull (cell.AccessibilityObject, "#B cell.AccessibilityObject");
				Assert.AreEqual (-1, cell.ColumnIndex, "#B cell.ColumnIndex");
				Assert.IsNotNull (cell.ContentBounds, "#B cell.ContentBounds");
				Assert.IsNull (cell.ContextMenuStrip, "#B cell.ContextMenuStrip");
				Assert.IsNull (cell.DataGridView, "#B cell.DataGridView");
				Assert.IsNull (cell.DefaultNewRowValue, "#B cell.DefaultNewRowValue");
				Assert.AreEqual (false, cell.Displayed, "#B cell.Displayed");
				Assert.IsNull (cell.EditedFormattedValue, "#B cell.EditedFormattedValue");
				Assert.IsNotNull (cell.EditType, "#B cell.EditType");
				try {
					object zxf = cell.ErrorIconBounds;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#B cell.ErrorIconBounds");
				} catch (InvalidOperationException ex) {
					Assert.AreEqual (@"Cell is not in a DataGridView. The cell cannot retrieve the inherited cell style.", ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#B cell.ErrorIconBounds");
				}
				Assert.AreEqual (@"", cell.ErrorText, "#B cell.ErrorText");
				Assert.IsNull (cell.FormattedValue, "#B cell.FormattedValue");
				Assert.IsNotNull (cell.FormattedValueType, "#B cell.FormattedValueType");
				Assert.AreEqual (false, cell.Frozen, "#B cell.Frozen");
				Assert.AreEqual (false, cell.HasStyle, "#B cell.HasStyle");
				Assert.AreEqual (DataGridViewElementStates.ResizableSet | DataGridViewElementStates.Visible, cell.InheritedState, "#B cell.InheritedState");
				try {
					object zxf = cell.InheritedStyle;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#B cell.InheritedStyle");
				} catch (InvalidOperationException ex) {
					Assert.AreEqual (@"Cell is not in a DataGridView. The cell cannot retrieve the inherited cell style.", ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#B cell.InheritedStyle");
				}
				Assert.AreEqual (false, cell.IsInEditMode, "#B cell.IsInEditMode");
				Assert.IsNull (cell.OwningColumn, "#B cell.OwningColumn");
				Assert.IsNotNull (cell.OwningRow, "#B cell.OwningRow");
				Assert.IsNotNull (cell.PreferredSize, "#B cell.PreferredSize");
				Assert.AreEqual (false, cell.ReadOnly, "#B cell.ReadOnly");
				Assert.AreEqual (false, cell.Resizable, "#B cell.Resizable");
				Assert.AreEqual (-1, cell.RowIndex, "#B cell.RowIndex");
				Assert.AreEqual (false, cell.Selected, "#B cell.Selected");
				Assert.IsNotNull (cell.Size, "#B cell.Size");
				Assert.AreEqual (DataGridViewElementStates.None, cell.State, "#B cell.State");
				if (cell.HasStyle)
					Assert.IsNotNull (cell.Style, "#B cell.Style");
				Assert.IsNull (cell.Tag, "#B cell.Tag");
				Assert.AreEqual (@"", cell.ToolTipText, "#B cell.ToolTipText");
				Assert.IsNull (cell.Value, "#B cell.Value");
				Assert.IsNotNull (cell.ValueType, "#B cell.ValueType");
				Assert.AreEqual (true, cell.Visible, "#B cell.Visible");
				
				dgv.Columns.Add (col);

				Assert.IsNotNull (cell.AccessibilityObject, "#C cell.AccessibilityObject");
				Assert.AreEqual (-1, cell.ColumnIndex, "#C cell.ColumnIndex");
				Assert.IsNotNull (cell.ContentBounds, "#C cell.ContentBounds");
				Assert.IsNull (cell.ContextMenuStrip, "#C cell.ContextMenuStrip");
				Assert.IsNull (cell.DataGridView, "#C cell.DataGridView");
				Assert.IsNull (cell.DefaultNewRowValue, "#C cell.DefaultNewRowValue");
				Assert.AreEqual (false, cell.Displayed, "#C cell.Displayed");
				Assert.IsNull (cell.EditedFormattedValue, "#C cell.EditedFormattedValue");
				Assert.IsNotNull (cell.EditType, "#C cell.EditType");
				try {
					object zxf = cell.ErrorIconBounds;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#C cell.ErrorIconBounds");
				} catch (InvalidOperationException ex) {
					Assert.AreEqual (@"Cell is not in a DataGridView. The cell cannot retrieve the inherited cell style.", ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#C cell.ErrorIconBounds");
				}
				Assert.AreEqual (@"", cell.ErrorText, "#C cell.ErrorText");
				Assert.IsNull (cell.FormattedValue, "#C cell.FormattedValue");
				Assert.IsNotNull (cell.FormattedValueType, "#C cell.FormattedValueType");
				Assert.AreEqual (false, cell.Frozen, "#C cell.Frozen");
				Assert.AreEqual (false, cell.HasStyle, "#C cell.HasStyle");
				Assert.AreEqual (DataGridViewElementStates.ResizableSet | DataGridViewElementStates.Visible, cell.InheritedState, "#C cell.InheritedState");
				try {
					object zxf = cell.InheritedStyle;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#C cell.InheritedStyle");
				} catch (InvalidOperationException ex) {
					Assert.AreEqual (@"Cell is not in a DataGridView. The cell cannot retrieve the inherited cell style.", ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#C cell.InheritedStyle");
				}
				Assert.AreEqual (false, cell.IsInEditMode, "#C cell.IsInEditMode");
				Assert.IsNull (cell.OwningColumn, "#C cell.OwningColumn");
				Assert.IsNotNull (cell.OwningRow, "#C cell.OwningRow");
				Assert.IsNotNull (cell.PreferredSize, "#C cell.PreferredSize");
				Assert.AreEqual (false, cell.ReadOnly, "#C cell.ReadOnly");
				Assert.AreEqual (false, cell.Resizable, "#C cell.Resizable");
				Assert.AreEqual (-1, cell.RowIndex, "#C cell.RowIndex");
				Assert.AreEqual (false, cell.Selected, "#C cell.Selected");
				Assert.IsNotNull (cell.Size, "#C cell.Size");
				Assert.AreEqual (DataGridViewElementStates.None, cell.State, "#C cell.State");
				if (cell.HasStyle)
					Assert.IsNotNull (cell.Style, "#C cell.Style");
				Assert.IsNull (cell.Tag, "#C cell.Tag");
				Assert.AreEqual (@"", cell.ToolTipText, "#C cell.ToolTipText");
				Assert.IsNull (cell.Value, "#C cell.Value");
				Assert.IsNotNull (cell.ValueType, "#C cell.ValueType");
				Assert.AreEqual (true, cell.Visible, "#C cell.Visible");
				
				dgv.Rows.Add (row);

				Assert.IsNotNull (cell.AccessibilityObject, "#C cell.AccessibilityObject");
				Assert.AreEqual (0, cell.ColumnIndex, "#C cell.ColumnIndex");
				try {
					object zxf = cell.ContentBounds;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', but no exception was thrown.", "#C cell.ContentBounds");
				} catch (ArgumentOutOfRangeException ex) {
					Assert.AreEqual (string.Format (@"Specified argument was out of the range of valid values.{0}Parameter name: rowIndex", Environment.NewLine), ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', got '" + ex.GetType ().FullName + "'.", "#C cell.ContentBounds");
				}
				Assert.IsNull (cell.ContextMenuStrip, "#C cell.ContextMenuStrip");
				Assert.IsNotNull (cell.DataGridView, "#C cell.DataGridView");
				Assert.IsNull (cell.DefaultNewRowValue, "#C cell.DefaultNewRowValue");
				Assert.AreEqual (false, cell.Displayed, "#C cell.Displayed");
				try {
					object zxf = cell.EditedFormattedValue;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', but no exception was thrown.", "#C cell.EditedFormattedValue");
				} catch (ArgumentOutOfRangeException ex) {
					Assert.AreEqual (string.Format (@"Specified argument was out of the range of valid values.{0}Parameter name: rowIndex", Environment.NewLine), ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', got '" + ex.GetType ().FullName + "'.", "#C cell.EditedFormattedValue");
				}
				Assert.IsNotNull (cell.EditType, "#C cell.EditType");
				try {
					object zxf = cell.ErrorIconBounds;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', but no exception was thrown.", "#C cell.ErrorIconBounds");
				} catch (ArgumentOutOfRangeException ex) {
					Assert.AreEqual (string.Format (@"Specified argument was out of the range of valid values.{0}Parameter name: rowIndex", Environment.NewLine), ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', got '" + ex.GetType ().FullName + "'.", "#C cell.ErrorIconBounds");
				}
				Assert.AreEqual (@"", cell.ErrorText, "#C cell.ErrorText");
				try {
					object zxf = cell.FormattedValue;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', but no exception was thrown.", "#C cell.FormattedValue");
				} catch (ArgumentOutOfRangeException ex) {
					Assert.AreEqual (string.Format (@"Specified argument was out of the range of valid values.{0}Parameter name: rowIndex", Environment.NewLine), ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', got '" + ex.GetType ().FullName + "'.", "#C cell.FormattedValue");
				}
				Assert.IsNotNull (cell.FormattedValueType, "#C cell.FormattedValueType");
				Assert.AreEqual (false, cell.Frozen, "#C cell.Frozen");
				Assert.AreEqual (false, cell.HasStyle, "#C cell.HasStyle");
				try {
					object zxf = cell.InheritedState;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', but no exception was thrown.", "#C cell.InheritedState");
				} catch (ArgumentOutOfRangeException ex) {
					Assert.AreEqual (string.Format (@"Specified argument was out of the range of valid values.{0}Parameter name: rowIndex", Environment.NewLine), ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', got '" + ex.GetType ().FullName + "'.", "#C cell.InheritedState");
				}
				try {
					object zxf = cell.InheritedStyle;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', but no exception was thrown.", "#C cell.InheritedStyle");
				} catch (ArgumentOutOfRangeException ex) {
					Assert.AreEqual (string.Format (@"Specified argument was out of the range of valid values.{0}Parameter name: rowIndex", Environment.NewLine), ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', got '" + ex.GetType ().FullName + "'.", "#C cell.InheritedStyle");
				}
				try {
					object zxf = cell.IsInEditMode;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#C cell.IsInEditMode");
				} catch (InvalidOperationException ex) {
					Assert.AreEqual (@"Operation cannot be performed on a cell of a shared row.", ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#C cell.IsInEditMode");
				}
				Assert.IsNotNull (cell.OwningColumn, "#C cell.OwningColumn");
				Assert.IsNotNull (cell.OwningRow, "#C cell.OwningRow");
				try {
					object zxf = cell.PreferredSize;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', but no exception was thrown.", "#C cell.PreferredSize");
				} catch (ArgumentOutOfRangeException ex) {
					Assert.AreEqual (string.Format (@"Specified argument was out of the range of valid values.{0}Parameter name: rowIndex", Environment.NewLine), ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', got '" + ex.GetType ().FullName + "'.", "#C cell.PreferredSize");
				}
				Assert.AreEqual (false, cell.ReadOnly, "#C cell.ReadOnly");
				Assert.AreEqual (false, cell.Resizable, "#C cell.Resizable");
				Assert.AreEqual (-1, cell.RowIndex, "#C cell.RowIndex");
				Assert.AreEqual (false, cell.Selected, "#C cell.Selected");
				try {
					object zxf = cell.Size;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.InvalidOperationException', but no exception was thrown.", "#C cell.Size");
				} catch (InvalidOperationException ex) {
					Assert.AreEqual (@"Getting the Size property of a cell in a shared row is not a valid operation.", ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.InvalidOperationException', got '" + ex.GetType ().FullName + "'.", "#C cell.Size");
				}
				Assert.AreEqual (DataGridViewElementStates.None, cell.State, "#C cell.State");
				if (cell.HasStyle)
					Assert.IsNotNull (cell.Style, "#C cell.Style");
				Assert.IsNull (cell.Tag, "#C cell.Tag");
				Assert.AreEqual (@"", cell.ToolTipText, "#C cell.ToolTipText");
				try {
					object zxf = cell.Value;
					TestHelper.RemoveWarning (zxf);
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', but no exception was thrown.", "#C cell.Value");
				} catch (ArgumentOutOfRangeException ex) {
					Assert.AreEqual (string.Format (@"Specified argument was out of the range of valid values.{0}Parameter name: rowIndex", Environment.NewLine), ex.Message);
				} catch (Exception ex) {
					Assert.Fail ("Expected 'System.ArgumentOutOfRangeException', got '" + ex.GetType ().FullName + "'.", "#C cell.Value");
				}
				Assert.IsNotNull (cell.ValueType, "#C cell.ValueType");
				Assert.AreEqual (false, cell.Visible, "#C cell.Visible");
			}
		}

		[Test]
		[ExpectedException(typeof(InvalidEnumArgumentException))]
		public void TestException () {
			throw new InvalidEnumArgumentException();
		}

		/*
		[Test]
		[ExpectedException(typeof(Exception))]
		public void TestException () {
			ConcreteCollection myCollection;
			myCollection = new ConcreteCollection();
			....
			AssertEquals ("#UniqueID", expected, actual);
			....
			Fail ("Message");
		}
		*/
		class DataGridViewCellMockObject : DataGridViewCell
		{
			public DataGridViewCellMockObject ()
			{
			}
		}

		public class DataGridViewClipboardCell : DataGridViewCell
		{
			public DataGridViewClipboardCell ()
			{
			}

			public object GetClipboardContentPublic (int rowIndex, bool firstCell, bool lastCell, bool inFirstRow, bool inLastRow, string format)
			{
				return GetClipboardContent (rowIndex, firstCell, lastCell, inFirstRow, inLastRow, format);
			}

			public override Type FormattedValueType
			{
				get
				{
					return typeof (string);
				}
			}
		}
	}
}

#endif
