//
// Tests for System.Web.UI.WebControls.CheckBoxList.cs 
//
// Author:
//	Jackson Harper (jackson@ximian.com)
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using NUnit.Framework;
using System;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.System.Web.UI.WebControls {

	public class CheckBoxListPoker : CheckBoxList {

		public Style CreateStyle ()
		{
			return CreateControlStyle ();
		}

		public Control FindControlPoke (string name, int offset)
		{
			return FindControl (name, offset);
		}
	}

	[TestFixture]
	public class CheckBoxListTest {

		[Test]
		public void Defaults ()
		{
			CheckBoxList c = new CheckBoxList ();

			Assert.AreEqual (c.CellPadding, -1, "A1");
			Assert.AreEqual (c.CellSpacing, -1, "A2");
			Assert.AreEqual (c.RepeatColumns, 0, "A3");
			Assert.AreEqual (c.RepeatDirection,
					RepeatDirection.Vertical, "A4");
			Assert.AreEqual (c.RepeatLayout,
					RepeatLayout.Table, "A5");
			Assert.AreEqual (c.TextAlign, TextAlign.Right, "A6");
		}

		[Test]
		public void CleanProperties ()
		{
			CheckBoxList c = new CheckBoxList ();

			c.CellPadding = Int32.MaxValue;
			Assert.AreEqual (c.CellPadding, Int32.MaxValue, "A1");

			c.CellSpacing = Int32.MaxValue;
			Assert.AreEqual (c.CellSpacing, Int32.MaxValue, "A2");

			c.RepeatColumns = Int32.MaxValue;
			Assert.AreEqual (c.RepeatColumns, Int32.MaxValue, "A3");

			foreach (RepeatDirection d in
					Enum.GetValues (typeof (RepeatDirection))) {
				c.RepeatDirection = d;
				Assert.AreEqual (c.RepeatDirection, d, "A4-" + d);
			}

			foreach (RepeatLayout l in
					Enum.GetValues (typeof (RepeatLayout))) {
				c.RepeatLayout = l;
				Assert.AreEqual (c.RepeatLayout, l, "A5-" + l);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CellPaddingTooLow ()
		{
			CheckBoxList c = new CheckBoxList ();
			c.CellPadding = -2;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CellSpacingTooLow ()
		{
			CheckBoxList c = new CheckBoxList ();
			c.CellSpacing = -2;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void RepeatColumsTooLow ()
		{
			CheckBoxList c = new CheckBoxList ();
			c.RepeatColumns = -1;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void RepeatDirection_Invalid ()
		{
			CheckBoxList c = new CheckBoxList ();
			c.RepeatDirection = (RepeatDirection) Int32.MaxValue;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void RepeatLayout_Invalid ()
		{
			CheckBoxList c = new CheckBoxList ();
			c.RepeatLayout = (RepeatLayout) Int32.MaxValue;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TextAlign_Invalid ()
		{
			CheckBoxList c = new CheckBoxList ();
			c.TextAlign = (TextAlign) Int32.MaxValue;
		}

		[Test]
		public void ChildCheckBoxControl ()
		{
			CheckBoxList c = new CheckBoxList ();
			Assert.AreEqual (c.Controls.Count, 1, "A1");
			Assert.AreEqual (c.Controls [0].GetType (), typeof (CheckBox), "A2");
		}

		[Test]
		public void CreateStyle ()
		{
			CheckBoxListPoker c = new CheckBoxListPoker ();
			Assert.AreEqual (c.CreateStyle ().GetType (), typeof (TableStyle), "A1");
		}

		[Test]
		public void RepeatInfoProperties ()
		{
			IRepeatInfoUser ri = new CheckBoxList ();

			Assert.IsFalse (ri.HasFooter, "A1");
			Assert.IsFalse (ri.HasHeader, "A2");
			Assert.IsFalse (ri.HasSeparators, "A3");
			Assert.AreEqual (ri.RepeatedItemCount, 0, "A4");
		}

		[Test]
		public void RepeatInfoCount ()
		{
			CheckBoxList c = new CheckBoxList ();
			IRepeatInfoUser ri = (IRepeatInfoUser) c;

			Assert.AreEqual (ri.RepeatedItemCount, 0, "A1");

			c.Items.Add ("one");
			c.Items.Add ("two");
			c.Items.Add ("three");
			Assert.AreEqual (ri.RepeatedItemCount, 3, "A2");
		}

		[Test]
		public void RepeatInfoStyle ()
		{
			IRepeatInfoUser ri = new CheckBoxList ();

			foreach (ListItemType t in Enum.GetValues (typeof (ListItemType))) {
				Assert.AreEqual (ri.GetItemStyle (t, 0), null, "A1-" + t);
				Assert.AreEqual (ri.GetItemStyle (t, 1), null, "A2-" + t);
				Assert.AreEqual (ri.GetItemStyle (t, 2), null, "A3-" + t);
				Assert.AreEqual (ri.GetItemStyle (t, 3), null, "A4-" + t);
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void RepeatInfoRenderOutOfRange ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			IRepeatInfoUser ri = new CheckBoxList ();

			ri.RenderItem (ListItemType.Item, -1, new RepeatInfo (), tw); 
		}

		[Test]
		public void RepeatInfoRenderItem ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			CheckBoxList c = new CheckBoxList ();
			IRepeatInfoUser ri = (IRepeatInfoUser) c;
			RepeatInfo r = new RepeatInfo ();

			c.Items.Add ("one");
			c.Items.Add ("two");

			ri.RenderItem (ListItemType.Item, 0, r, tw); 
			Assert.AreEqual ("<input id=\"0\" type=\"checkbox\" name=\"0\" />" +
					"<label for=\"0\">one</label>", sw.ToString (), "A1");
		}

		[Test]
		public void FindControl ()
		{
			CheckBoxListPoker p = new CheckBoxListPoker ();

			p.ID = "id";
			p.Items.Add ("one");
			p.Items.Add ("two");
			p.Items.Add ("three");

			// Everything seems to return this.
			Assert.AreEqual (p.FindControlPoke (String.Empty, 0), p, "A1");
			Assert.AreEqual (p.FindControlPoke ("id", 0), p, "A2");
			Assert.AreEqual (p.FindControlPoke ("id_0", 0), p, "A3");
			Assert.AreEqual (p.FindControlPoke ("id_1", 0), p, "A4");
			Assert.AreEqual (p.FindControlPoke ("id_2", 0), p, "A5");
			Assert.AreEqual (p.FindControlPoke ("id_3", 0), p, "A6");
			Assert.AreEqual (p.FindControlPoke ("0", 0), p, "A7");

			Assert.AreEqual (p.FindControlPoke (String.Empty, 10), p, "A1");
			Assert.AreEqual (p.FindControlPoke ("id", 10), p, "A2");
			Assert.AreEqual (p.FindControlPoke ("id_0", 10), p, "A3");
			Assert.AreEqual (p.FindControlPoke ("id_1", 10), p, "A4");
			Assert.AreEqual (p.FindControlPoke ("id_2", 10), p, "A5");
			Assert.AreEqual (p.FindControlPoke ("id_3", 10), p, "A6");
			Assert.AreEqual (p.FindControlPoke ("0", 10), p, "A7");
		}

		private void Render (CheckBoxList list, string expected, string test)
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new CleanHtmlTextWriter (sw);
			sw.NewLine = "\n";

			list.RenderControl (tw);
			Assert.AreEqual (expected, sw.ToString (), test);
		}

		[Test]
		public void RenderEmpty ()
		{
			CheckBoxList c = new CheckBoxList ();

#if NET_2_0
			Render (c, "", "A1");
#else
			Render (c, "<table border=\"0\">\n\n</table>", "A1");
#endif
			c.CellPadding = 1;
#if NET_2_0
			Render (c, "", "A2");
#else
			Render (c, "<table border=\"0\" cellpadding=\"1\">\n\n</table>", "A2");
#endif

			c = new CheckBoxList ();
			c.CellPadding = 1;
#if NET_2_0
			Render (c, "", "A3");
#else
			Render (c, "<table border=\"0\" cellpadding=\"1\">\n\n</table>", "A3");
#endif

			c = new CheckBoxList ();
			c.TextAlign = TextAlign.Left;
#if NET_2_0
			Render (c, "", "A4");
#else
			Render (c, "<table border=\"0\">\n\n</table>", "A4");
#endif
		}

		[Test]
#if NET_2_0
		[Category("NotDotNet")] // MS's implementation throws NRE's from these
#endif
		public void Render ()
		{
			CheckBoxList c;
			c = new CheckBoxList ();
			c.Items.Add ("foo");
			Render (c, "<table border=\"0\">\n\t<tr>\n\t\t<td><input id=\"0\" " +
					"name=\"0\" type=\"checkbox\" />" +
					"<label for=\"0\">foo</label>" +
					"</td>\n\t</tr>\n</table>", "A5");

			c = new CheckBoxList ();
			c.Items.Add ("foo");
			Render (c, "<table border=\"0\">\n\t<tr>\n\t\t<td><input id=\"0\" " +
					"name=\"0\" type=\"checkbox\" />" +
					"<label for=\"0\">foo</label>" +
					"</td>\n\t</tr>\n</table>", "A6");
		}

		// bug 51648
		[Test]
#if NET_2_0
		[Category("NotDotNet")] // MS's implementation throws NRE's from these
#endif
		public void TestTabIndex ()
		{
			CheckBoxList c = new CheckBoxList ();
			c.TabIndex = 5;
			c.Items.Add ("Item1");
			string exp = @"<table border=""0"">
	<tr>
		<td><input id=""0"" name=""0"" tabindex=""5"" type=""checkbox"" /><label for=""0"">Item1</label></td>
	</tr>
</table>";
			Render (c, exp, "B1");
		}

		// bug 48802
		[Test]
#if NET_2_0
		[Category("NotDotNet")] // MS's implementation throws NRE's from these
#endif
		public void TestDisabled ()
		{
			CheckBoxList c = new CheckBoxList ();
			c.Enabled = false;
			c.Items.Add ("Item1");
			string exp = @"<table border=""0"" disabled=""disabled"">
	<tr>
		<td><span disabled=""disabled""><input disabled=""disabled"" id=""0"" name=""0"" type=""checkbox"" /><label for=""0"">Item1</label></span></td>
	</tr>
</table>";
			Render (c, exp, "C1");
		}		
		
	}
}

