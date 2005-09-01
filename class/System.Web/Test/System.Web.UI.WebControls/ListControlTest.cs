//
// Tests for System.Web.UI.WebControls.ListBoxTest.cs
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
using System.Drawing;
using System.Collections;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.System.Web.UI.WebControls {

	public class ListControlPoker : ListControl {

		public ListControlPoker ()
		{
		}

		public void TrackState ()
		{
			TrackViewState ();
		}

		public object SaveState ()
		{
			return SaveViewState ();
		}

		public void LoadState (object state)
		{
			LoadViewState (state);
		}

		public object ViewStateValue (string name)
		{
			return ViewState [name];
		}

		public string Render ()
		{
			StringWriter sw = new StringWriter ();
			sw.NewLine = "\n";
			HtmlTextWriter writer = new HtmlTextWriter (sw);
			base.Render (writer);
			return writer.InnerWriter.ToString ();
		}

#if NET_2_0
		public HtmlTextWriterTag GetTagKey ()
		{
			return TagKey;
		}
#endif
	}

	[TestFixture]
	public class ListControlTest {

		private Hashtable changed = new Hashtable ();

		[Test]
		public void DefaultProperties ()
		{
			ListControlPoker p = new ListControlPoker ();

			Assert.AreEqual (p.AutoPostBack, false, "A1");
			Assert.AreEqual (p.DataMember, String.Empty, "A2");
			Assert.AreEqual (p.DataSource, null, "A3");
			Assert.AreEqual (p.DataTextField, String.Empty, "A4");
			Assert.AreEqual (p.DataTextFormatString, String.Empty, "A5");
			Assert.AreEqual (p.DataValueField, String.Empty, "A6");
			Assert.AreEqual (p.Items.Count, 0, "A7");
			Assert.AreEqual (p.SelectedIndex, -1,"A8");
			Assert.AreEqual (p.SelectedItem, null, "A9");
			Assert.AreEqual (p.SelectedValue, String.Empty, "A10");
#if NET_2_0
			Assert.IsFalse (p.AppendDataBoundItems, "A11");
			Assert.AreEqual (p.Text, "", "A12");
			Assert.AreEqual (p.GetTagKey(), HtmlTextWriterTag.Select, "A13");
#endif
		}

		[Test]
		public void CleanProperties ()
		{
			ListControlPoker p = new ListControlPoker ();

			p.AutoPostBack = true;
			Assert.AreEqual (p.AutoPostBack, true, "A2");

			p.DataMember = "DataMember";
			Assert.AreEqual (p.DataMember, "DataMember", "A3");

			p.DataSource = "DataSource";
			Assert.AreEqual (p.DataSource, "DataSource", "A4");

			p.DataTextField = "DataTextField";
			Assert.AreEqual (p.DataTextField, "DataTextField", "A5");

			p.DataTextFormatString = "DataTextFormatString";
			Assert.AreEqual (p.DataTextFormatString, "DataTextFormatString", "A6");

			p.DataValueField = "DataValueField";
			Assert.AreEqual (p.DataValueField, "DataValueField", "A7");

			p.SelectedIndex = 10;
			Assert.AreEqual (p.SelectedIndex, -1, "A8");

			p.SelectedValue = "SelectedValue";
			Assert.AreEqual (p.SelectedValue, String.Empty, "A9");
		}

		[Test]
		public void NullProperties ()
		{
			ListControlPoker p = new ListControlPoker ();

			p.DataMember = null;
			Assert.AreEqual (p.DataMember, String.Empty, "A1");

			p.DataSource = null;
			Assert.AreEqual (p.DataSource, null, "A2");

			p.DataTextField = null;
			Assert.AreEqual (p.DataTextField, String.Empty, "A3");

			p.DataTextFormatString = null;
			Assert.AreEqual (p.DataTextFormatString, String.Empty, "A4");

			p.DataValueField = null;
			Assert.AreEqual (p.DataValueField, String.Empty, "A5");

			p.SelectedValue = null;
			Assert.AreEqual (p.SelectedValue, String.Empty, "A6");
		}

		[Test]
		public void ClearSelection ()
		{
			ListControlPoker p = new ListControlPoker ();

			ListItem foo = new ListItem ("foo");
			ListItem bar = new ListItem ("bar");

			BeginIndexChanged (p);

			p.Items.Add (foo);
			p.Items.Add (bar);
			p.SelectedIndex = 1;

			// sanity for the real test
			Assert.AreEqual (p.Items.Count, 2, "A1");
			Assert.AreEqual (p.SelectedIndex, 1, "A2");
			Assert.AreEqual (p.SelectedItem, bar, "A3");
			Assert.AreEqual (p.SelectedValue, bar.Value, "A4");
			
			p.ClearSelection ();

			Assert.AreEqual (p.SelectedIndex, -1, "A5");
			Assert.AreEqual (p.SelectedItem, null, "A6");
			Assert.AreEqual (p.SelectedValue, String.Empty, "A7");
			Assert.IsFalse (EndIndexChanged (p), "A8");

			// make sure we are still sane
			Assert.AreEqual (p.Items.Count, 2, "A9");
		}

		[Test]
		// Tests Save/Load/Track ViewState
		public void ViewState ()
		{
			ListControlPoker a = new ListControlPoker ();
			ListControlPoker b = new ListControlPoker ();

			a.TrackState ();

			BeginIndexChanged (a);
			BeginIndexChanged (b);

			a.Items.Add ("a");
			a.Items.Add ("b");
			a.Items.Add ("c");
			a.SelectedIndex = 2;

			object state = a.SaveState ();
			b.LoadState (state);

#if NET_2_0
			Assert.AreEqual (b.SelectedIndex, -1, "A1");
#else
			Assert.AreEqual (b.SelectedIndex, 2, "A1");
#endif
			Assert.AreEqual (b.Items.Count, 3, "A2");

			Assert.AreEqual (b.Items [0].Value, "a", "A3");
			Assert.AreEqual (b.Items [1].Value, "b", "A4");
			Assert.AreEqual (b.Items [2].Value, "c", "A5");

			Assert.IsFalse (EndIndexChanged (a), "A6");
			Assert.IsFalse (EndIndexChanged (b), "A7");
		}

		[Test]
		public void ViewStateContents ()
		{
			ListControlPoker p = new ListControlPoker ();

			p.TrackState ();

			// So the selected index can be set
			p.Items.Add ("one");
			p.Items.Add ("two");

			p.AutoPostBack = false;
			p.DataMember = "DataMember";
			p.DataSource = "DataSource";
			p.DataTextField = "DataTextField";
			p.DataTextFormatString = "DataTextFormatString";
			p.DataValueField = "DataValueField";
			p.SelectedIndex = 1;
#if NET_2_0
			p.AppendDataBoundItems = true;
			p.Text = "Text";
#endif

			Assert.AreEqual (p.ViewStateValue ("AutoPostBack"), false, "A1");
			Assert.AreEqual (p.ViewStateValue ("DataMember"), "DataMember", "A2");

			Assert.AreEqual (p.ViewStateValue ("DataSource"), null, "A3");
			Assert.AreEqual (p.ViewStateValue ("DataTextField"), "DataTextField", "A4");
			Assert.AreEqual (p.ViewStateValue ("DataTextFormatString"),
					"DataTextFormatString", "A5");
			Assert.AreEqual (p.ViewStateValue ("DataValueField"), "DataValueField", "A6");

#if NET_2_0
			Assert.AreEqual (p.ViewStateValue ("AppendDataBoundItems"), true, "A7");
#endif

			// None of these are saved
			Assert.AreEqual (p.ViewStateValue ("SelectedIndex"), null, "A8");
			Assert.AreEqual (p.ViewStateValue ("SelectedItem"), null, "A9");
			Assert.AreEqual (p.ViewStateValue ("SelectedValue"), null, "A10");
#if NET_2_0
			Assert.AreEqual (p.ViewStateValue ("Text"), null, "A11");
#endif

		}

		[Test]
		public void SelectedIndex ()
		{
			ListControlPoker p = new ListControlPoker ();

			p.Items.Add ("one");
			p.Items.Add ("two");
			p.Items.Add ("three");
			p.Items.Add ("four");

			p.Items [2].Selected = true;
			p.Items [1].Selected = true;

			Assert.AreEqual (p.SelectedIndex, 1, "A1");

			p.ClearSelection ();
			p.Items [3].Selected = true;

			Assert.AreEqual (p.SelectedIndex, 3, "A2");

			p.SelectedIndex = 1;
			Assert.AreEqual (p.SelectedIndex, 1, "A3");
			Assert.IsFalse (p.Items [3].Selected, "A4");
		}

		[Test]
		public void Render ()
		{
			ListControlPoker p = new ListControlPoker ();

			string s = p.Render ();
			string expected = "<select>\n\n</select>";
			Assert.AreEqual (s, expected, "A1");
		}

		[Test]
#if !NET_2_0
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
#endif
		public void ItemsTooHigh ()
		{
			ListControlPoker l = new ListControlPoker ();

			l.Items.Add ("foo");
			l.SelectedIndex = 1;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ItemsTooLow ()
		{
			ListControlPoker l = new ListControlPoker ();

			l.Items.Add ("foo");
			l.SelectedIndex = -2;
		}

		[Test]
		public void ItemsOk ()
		{
			ListControlPoker l = new ListControlPoker ();

			l.Items.Add ("foo");
			l.SelectedIndex = 0;
			l.SelectedIndex = -1;
		}

		private void BeginIndexChanged (ListControl l)
		{
			l.SelectedIndexChanged += new EventHandler (IndexChangedHandler);
		}

		private bool EndIndexChanged (ListControl l)
		{
			bool res = changed [l] != null;
			changed [l] = null;
			return res;
		}

		private void IndexChangedHandler (object sender, EventArgs e)
		{
			changed [sender] = new object ();
		}
	}
}

