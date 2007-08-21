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
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Author:
//	Rolf Bjarne Kvinge  (RKvinge@novell.com)
//


#if NET_2_0

using NUnit.Framework;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections;

namespace MonoTests.System.Windows.Forms
{

	[TestFixture]
	public class DataGridViewRowCollectionTest
	{
		private DataGridView CreateAndFill ()
		{
			DataGridView dgv = DataGridViewCommon.CreateAndFill ();
			DataGridViewRow row = new DataGridViewRow ();
			row.Cells.Add (new DataGridViewComboBoxCell ());
			row.Cells.Add (new DataGridViewComboBoxCell ());
			dgv.Rows.Add (row);
			return dgv;
		}

		[Test]
		public void ToStringTest ()
		{
			Assert.AreEqual ("System.Windows.Forms.DataGridViewRowCollection", new DataGridViewRowCollection (null).ToString (), "A");

			using (DataGridView dgv = CreateAndFill ()) {
				Assert.AreEqual ("System.Windows.Forms.DataGridViewRowCollection", dgv.Rows.ToString (), "B");
			}

		}
		
		[Test]
		public void CtorTest ()
		{
			DataGridViewRowCollection rc;
			
			rc = new DataGridViewRowCollection (null);
			Assert.AreEqual (0, rc.Count, "#01");
			
			using (DataGridView dgv = new DataGridView ()) {
				rc = new DataGridViewRowCollection (dgv);
				Assert.AreEqual (0, rc.Count, "#02");
				Assert.IsTrue (rc != dgv.Rows, "#03");
			}
		}
		
		[Test]
		public void AddTest ()
		{
			DataGridViewRow row;
			DataGridViewCell cell;
			
			using (DataGridView dgv = new DataGridView ()) {
				dgv.Columns.Add ("a", "A");
				row = new DataGridViewRow ();
				dgv.Rows.Add (row);
				Assert.AreEqual (-1, row.Index, "#01");
			}

			using (DataGridView dgv = new DataGridView ()) {
				dgv.Columns.Add ("a", "A");
				row = new DataGridViewRow ();
				cell = new DataGridViewTextBoxCell ();
				cell.Value = "abc";
				row.Cells.Add (cell);
				dgv.Rows.Add (row);
				Assert.AreEqual (0, row.Index, "#02");
			}
		}
	}
}
#endif