//
// TabControlTest.cs: Test cases for TabControl.
//
// Author:
//   Ritvik Mayank (mritvik@novell.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Drawing;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class TabControlTest
	{
		private int _selected_index_changed = 0;

		private class TabControlPoker : TabControl {

			public bool CheckIsInputKey (Keys key)
			{
				return IsInputKey (key);
			}

			protected override void WndProc (ref Message m)
			{
				base.WndProc (ref m);
			}
		}

		[SetUp]
		public void SetUp ()
		{
			_selected_index_changed = 0;
		}

		[Test]
		public void TabControlPropertyTest ()
		{
			Form myForm = new Form ();
			myForm.ShowInTaskbar = false;
			TabControl myTabControl = new TabControl ();
			myTabControl.Visible = true;
			myTabControl.Name = "Mono TabControl";
		
			// A 
			Assert.AreEqual (TabAlignment.Top, myTabControl.Alignment, "A1");
			Assert.AreEqual (TabAppearance.Normal, myTabControl.Appearance, "#A2");
		
			// D 
			Assert.AreEqual (TabDrawMode.Normal, myTabControl.DrawMode, "#D5");
		
			// H
			Assert.AreEqual (false, myTabControl.HotTrack, "#H1");
		
			// I 
			Assert.AreEqual (null, myTabControl.ImageList, "#I1");

			// M 
			Assert.AreEqual (false, myTabControl.Multiline, "#M1");
		
			// P
			Assert.AreEqual (6, myTabControl.Padding.X, "#P1");
			Assert.AreEqual (3, myTabControl.Padding.Y, "#P1");

			// R
			Assert.AreEqual (0, myTabControl.RowCount, "#R1");

			// S
			Assert.AreEqual (-1, myTabControl.SelectedIndex, "#S1");
			Assert.AreEqual (null, myTabControl.SelectedTab, "#S2");
			Assert.AreEqual (false, myTabControl.ShowToolTips, "#S3");
			Assert.AreEqual (TabSizeMode.Normal, myTabControl.SizeMode, "#S4");

			// T
			Assert.AreEqual (0, myTabControl.TabCount, "#T1");
			Assert.AreEqual (0, myTabControl.TabPages.Count, "#T2");

			myForm.Dispose ();
		}

		[Test]
		[Category ("NotWorking")]
		public void GetTabRectTest ()
		{
			TabControl myTabControl = new TabControl ();
			TabPage myTabPage = new TabPage();
			myTabControl.Controls.Add(myTabPage);
			myTabPage.TabIndex = 0;
			Rectangle myTabRect = myTabControl.GetTabRect (0);
			Assert.AreEqual (2, myTabRect.X, "#GetT1");
			Assert.AreEqual (2, myTabRect.Y, "#GetT2");
			Assert.AreEqual (42, myTabRect.Width, "#GetT3");
			// It is environment dependent
			//Assert.AreEqual (18, myTabRect.Height, "#GetT4");
		}

		[Test]
		public void ToStringTest ()
		{
			TabControl myTabControl = new TabControl ();
			Assert.AreEqual ("System.Windows.Forms.TabControl, TabPages.Count: 0", myTabControl.ToString(), "#1");
		}

		[Test]
		public void ClearTabPagesTest ()
		{
			// no tab pages
			TabControl tab = new TabControl ();
			tab.TabPages.Clear ();
			Assert.AreEqual (-1, tab.SelectedIndex, "#A1");
			Assert.AreEqual (0, tab.TabPages.Count, "#A2");

			// single tab page
			tab.Controls.Add (new TabPage ());
			Assert.AreEqual (0, tab.SelectedIndex, "#B1");
			Assert.AreEqual (1, tab.TabPages.Count, "#B2");
			tab.TabPages.Clear();
			Assert.AreEqual (-1, tab.SelectedIndex, "#B3");
			Assert.AreEqual (0, tab.TabPages.Count, "#B4");

			// multiple tab pages
			tab.Controls.Add (new TabPage ());
			tab.Controls.Add (new TabPage ());
			tab.Controls.Add (new TabPage ());
			Assert.AreEqual (0, tab.SelectedIndex, "#C1");
			Assert.AreEqual (3, tab.TabPages.Count, "#C2");
			tab.SelectedIndex = 1;
			tab.TabPages.Clear ();
			Assert.AreEqual (-1, tab.SelectedIndex, "#C3");
			Assert.AreEqual (0, tab.TabPages.Count, "#C4");
		}

		[Test]
		[Category ("NotWorking")]
		public void Controls_Remove_HandleCreated ()
		{
			TabControl tab = new TabControl ();
			tab.SelectedIndexChanged += new EventHandler (SelectedIndexChanged);

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (tab);
			form.Show ();

			tab.Controls.Add (new TabPage ());
			tab.Controls.Add (new TabPage ());
			tab.Controls.Add (new TabPage ());
			tab.Controls.Add (new TabPage ());
			tab.Controls.Add (new TabPage ());
			tab.Controls.Add (new TabPage ());

			Assert.AreEqual (0, tab.SelectedIndex, "#A1");
			Assert.AreEqual (6, tab.TabPages.Count, "#A2");
			Assert.AreEqual (0, _selected_index_changed, "#A3");

			// remove selected tab
			tab.SelectedIndex = 2;
			Assert.AreEqual (2, tab.SelectedIndex, "#B1");
			Assert.AreEqual (1, _selected_index_changed, "#B2");
			tab.Controls.RemoveAt (2);
#if NET_2_0
			Assert.AreEqual (0, tab.SelectedIndex, "#B3");
#else
			Assert.AreEqual (1, tab.SelectedIndex, "#B3");
#endif
			Assert.AreEqual (5, tab.TabPages.Count, "#B4");
			Assert.AreEqual (2, _selected_index_changed, "#B5");

			// remove not-selected tab
#if NET_2_0
			Assert.AreEqual (0, tab.SelectedIndex, "#C1");
#else
			Assert.AreEqual (1, tab.SelectedIndex, "#C1");
#endif
			Assert.AreEqual (2, _selected_index_changed, "#C2");
			tab.Controls.RemoveAt (3);
#if NET_2_0
			Assert.AreEqual (0, tab.SelectedIndex, "#C3");
#else
			Assert.AreEqual (2, tab.SelectedIndex, "#C3");
#endif
			Assert.AreEqual (4, tab.TabPages.Count, "#C4");
#if NET_2_0
			Assert.AreEqual (2, _selected_index_changed, "#C5");
#else
			Assert.AreEqual (3, _selected_index_changed, "#C5");
#endif

			// remove last tab
			tab.Controls.RemoveAt (3);
#if NET_2_0
			Assert.AreEqual (0, tab.SelectedIndex, "#D1");
#else
			Assert.AreEqual (2, tab.SelectedIndex, "#D1");
#endif
			Assert.AreEqual (3, tab.TabPages.Count, "#D2");
#if NET_2_0
			Assert.AreEqual (2, _selected_index_changed, "#D3");
#else
			Assert.AreEqual (3, _selected_index_changed, "#D3");
#endif

			// remove first tab
			tab.Controls.RemoveAt (0);
			Assert.AreEqual (0, tab.SelectedIndex, "#E1");
			Assert.AreEqual (2, tab.TabPages.Count, "#E2");
#if NET_2_0
			Assert.AreEqual (3, _selected_index_changed, "#E3");
#else
			Assert.AreEqual (4, _selected_index_changed, "#E3");
#endif

			// remove remaining tabs
			tab.Controls.RemoveAt (1);
			tab.Controls.RemoveAt (0);
			Assert.AreEqual (-1, tab.SelectedIndex, "#F1");
			Assert.AreEqual (0, tab.TabPages.Count, "#F2");
#if NET_2_0
			Assert.AreEqual (4, _selected_index_changed, "#F3");
#else
			Assert.AreEqual (5, _selected_index_changed, "#F3");
#endif
		}

		[Test]
		[Category ("NotWorking")]
		public void Controls_Remove_HandleNotCreated ()
		{
			TabControl tab = new TabControl ();
			tab.SelectedIndexChanged += new EventHandler (SelectedIndexChanged);
			tab.Controls.Add (new TabPage ());
			tab.Controls.Add (new TabPage ());
			tab.Controls.Add (new TabPage ());
			tab.Controls.Add (new TabPage ());
			tab.Controls.Add (new TabPage ());
			tab.Controls.Add (new TabPage ());

			Assert.AreEqual (-1, tab.SelectedIndex, "#A1");
			Assert.AreEqual (6, tab.TabPages.Count, "#A2");
			Assert.AreEqual (0, _selected_index_changed, "#A3");

			// remove selected tab
			tab.SelectedIndex = 2;
			Assert.AreEqual (2, tab.SelectedIndex, "#B1");
#if NET_2_0
			Assert.AreEqual (0, _selected_index_changed, "#B2");
#else
			Assert.AreEqual (1, _selected_index_changed, "#B2");
#endif
			tab.Controls.RemoveAt (2);
#if NET_2_0
			Assert.AreEqual (0, tab.SelectedIndex, "#B3");
#else
			Assert.AreEqual (1, tab.SelectedIndex, "#B3");
#endif
			Assert.AreEqual (5, tab.TabPages.Count, "#B4");
#if NET_2_0
			Assert.AreEqual (0, _selected_index_changed, "#B5");
#else
			Assert.AreEqual (2, _selected_index_changed, "#B5");
#endif

			// remove not-selected tab
			tab.Controls.RemoveAt (3);
#if NET_2_0
			Assert.AreEqual (0, tab.SelectedIndex, "#C3");
#else
			Assert.AreEqual (2, tab.SelectedIndex, "#C3");
#endif
			Assert.AreEqual (4, tab.TabPages.Count, "#C4");
#if NET_2_0
			Assert.AreEqual (0, _selected_index_changed, "#C5");
#else
			Assert.AreEqual (3, _selected_index_changed, "#C5");
#endif

			// remove last tab
			tab.Controls.RemoveAt (3);
#if NET_2_0
			Assert.AreEqual (0, tab.SelectedIndex, "#D1");
#else
			Assert.AreEqual (2, tab.SelectedIndex, "#D1");
#endif
			Assert.AreEqual (3, tab.TabPages.Count, "#D2");
#if NET_2_0
			Assert.AreEqual (0, _selected_index_changed, "#D3");
#else
			Assert.AreEqual (3, _selected_index_changed, "#D3");
#endif

			// remove first tab
			tab.Controls.RemoveAt (0);
			Assert.AreEqual (0, tab.SelectedIndex, "#E1");
			Assert.AreEqual (2, tab.TabPages.Count, "#E2");
#if NET_2_0
			Assert.AreEqual (0, _selected_index_changed, "#E3");
#else
			Assert.AreEqual (4, _selected_index_changed, "#E3");
#endif

			// remove remaining tabs
			tab.Controls.RemoveAt (1);
			tab.Controls.RemoveAt (0);
			Assert.AreEqual (0, tab.SelectedIndex, "#F1");
			Assert.AreEqual (0, tab.TabPages.Count, "#F2");
#if NET_2_0
			Assert.AreEqual (0, _selected_index_changed, "#F3");
#else
			Assert.AreEqual (4, _selected_index_changed, "#F3");
#endif

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (tab);
			form.Show ();
			Assert.AreEqual (-1, tab.SelectedIndex, "#G1");
			Assert.AreEqual (0, tab.TabPages.Count, "#G2");
#if NET_2_0
			Assert.AreEqual (0, _selected_index_changed, "#G3");
#else
			Assert.AreEqual (4, _selected_index_changed, "#G3");
#endif
		}

		[Test]
		public void SelectedIndex ()
		{
			TabControl tab = new TabControl ();
			tab.SelectedIndexChanged += new EventHandler (SelectedIndexChanged);
			tab.Controls.Add (new TabPage ());
			tab.Controls.Add (new TabPage ());

			tab.SelectedIndex = 0;
#if NET_2_0
			Assert.AreEqual (0, _selected_index_changed, "#A1");
#else
			Assert.AreEqual (1, _selected_index_changed, "#A1");
#endif
			Assert.AreEqual (0, tab.SelectedIndex, "#A2");

			tab.SelectedIndex = -1;
#if NET_2_0
			Assert.AreEqual (0, _selected_index_changed, "#B1");
#else
			Assert.AreEqual (2, _selected_index_changed, "#B1");
#endif
			Assert.AreEqual (-1, tab.SelectedIndex, "#B2");

			tab.SelectedIndex = 1;
#if NET_2_0
			Assert.AreEqual (0, _selected_index_changed, "#C1");
#else
			Assert.AreEqual (3, _selected_index_changed, "#C1");
#endif
			Assert.AreEqual (1, tab.SelectedIndex, "#C2");

			tab.SelectedIndex = 1;
#if NET_2_0
			Assert.AreEqual (0, _selected_index_changed, "#D1");
#else
			Assert.AreEqual (3, _selected_index_changed, "#D1");
#endif
			Assert.AreEqual (1, tab.SelectedIndex, "#D2");


			tab.SelectedIndex = 6;
#if NET_2_0
			Assert.AreEqual (0, _selected_index_changed, "#E1");
#else
			Assert.AreEqual (4, _selected_index_changed, "#E1");
#endif
			Assert.AreEqual (6, tab.SelectedIndex, "#E2");

			tab.SelectedIndex = 6;
#if NET_2_0
			Assert.AreEqual (0, _selected_index_changed, "#E31");
#else
			Assert.AreEqual (4, _selected_index_changed, "#E3");
#endif
			Assert.AreEqual (6, tab.SelectedIndex, "#E4");

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (tab);

			form.Show ();

			Assert.AreEqual (0, tab.SelectedIndex, "#E5");

			tab.SelectedIndex = 0;
#if NET_2_0
			Assert.AreEqual (0, _selected_index_changed, "#F1");
#else
			Assert.AreEqual (4, _selected_index_changed, "#F1");
#endif
			Assert.AreEqual (0, tab.SelectedIndex, "#F2");

			tab.SelectedIndex = -1;
#if NET_2_0
			Assert.AreEqual (1, _selected_index_changed, "#G1");
#else
			Assert.AreEqual (5, _selected_index_changed, "#G1");
#endif
			Assert.AreEqual (-1, tab.SelectedIndex, "#G2");

			tab.SelectedIndex = 1;
#if NET_2_0
			Assert.AreEqual (2, _selected_index_changed, "#H1");
#else
			Assert.AreEqual (6, _selected_index_changed, "#H1");
#endif
			Assert.AreEqual (1, tab.SelectedIndex, "#H2");

			tab.SelectedIndex = 1;
#if NET_2_0
			Assert.AreEqual (2, _selected_index_changed, "#I1");
#else
			Assert.AreEqual (6, _selected_index_changed, "#I1");
#endif
			Assert.AreEqual (1, tab.SelectedIndex, "#I2");

			form.Dispose ();
		}

		[Test] // bug #78395
		public void SelectedIndex_Ignore ()
		{
			TabControl c = new TabControl ();
			c.SelectedIndexChanged += new EventHandler (SelectedIndexChanged);
			c.SelectedIndex = 0;
#if NET_2_0
			Assert.AreEqual (0, _selected_index_changed, "#1");
#else
			Assert.AreEqual (1, _selected_index_changed, "#1");
#endif

			c.TabPages.Add (new TabPage ());
			c.TabPages.Add (new TabPage ());
			Assert.AreEqual (0, c.SelectedIndex, "#2");
			Form f = new Form ();
			f.ShowInTaskbar = false;
			f.Controls.Add (c);
			f.Show ();
#if NET_2_0
			Assert.AreEqual (0, _selected_index_changed, "#3");
#else
			Assert.AreEqual (1, _selected_index_changed, "#3");
#endif
			c.SelectedIndex = 2; // beyond the pages - ignored
#if NET_2_0
			Assert.AreEqual (1, _selected_index_changed, "#4");
#else
			Assert.AreEqual (2, _selected_index_changed, "#4");
#endif
			Assert.AreEqual (0, c.SelectedIndex, "#4");
			f.Dispose ();
		}

		[Test]
		public void SelectedIndex_Negative ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;
			TabControl tab = new TabControl ();
			tab.SelectedIndexChanged += new EventHandler (SelectedIndexChanged);
			form.Controls.Add (tab);

			Assert.AreEqual (-1, tab.SelectedIndex, "#A1");
			tab.SelectedIndex = -1;
			Assert.AreEqual (-1, tab.SelectedIndex, "#A2");
			Assert.AreEqual (0, _selected_index_changed, "#A3");

			try {
				tab.SelectedIndex = -2;
				Assert.Fail ("#B1");
#if NET_2_0
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsTrue (ex.Message.IndexOf ("'-2'") != -1, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'SelectedIndex'") != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf ("-1") != -1, "#B6");
				Assert.IsNotNull (ex.ParamName, "#B7");
				Assert.AreEqual ("SelectedIndex", ex.ParamName, "#B8");
			}
#else
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsTrue (ex.Message.IndexOf ("'-2'") != -1, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("'value'") != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf ("-1") != -1, "#B6");
				Assert.IsNull (ex.ParamName, "#B7");
			}
#endif

			Assert.AreEqual (0, _selected_index_changed, "#C1");
			Assert.AreEqual (-1, tab.SelectedIndex, "#C2");
			form.Show ();
			Assert.AreEqual (0, _selected_index_changed, "#C3");
			Assert.AreEqual (-1, tab.SelectedIndex, "#C4");

			try {
				tab.SelectedIndex = -5;
				Assert.Fail ("#D1");
#if NET_2_0
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#D2");
				Assert.IsNotNull (ex.Message, "#D3");
				Assert.IsTrue (ex.Message.IndexOf ("'-5'") != -1, "#D4");
				Assert.IsTrue (ex.Message.IndexOf ("'SelectedIndex'") != -1, "#D5");
				Assert.IsTrue (ex.Message.IndexOf ("-1") != -1, "#D6");
				Assert.IsNotNull (ex.ParamName, "#D7");
				Assert.AreEqual ("SelectedIndex", ex.ParamName, "#D8");
			}
#else
			} catch (ArgumentException ex) {
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#D2");
				Assert.IsNotNull (ex.Message, "#D3");
				Assert.IsTrue (ex.Message.IndexOf ("'-5'") != -1, "#D4");
				Assert.IsTrue (ex.Message.IndexOf ("'value'") != -1, "#D5");
				Assert.IsTrue (ex.Message.IndexOf ("-1") != -1, "#D6");
				Assert.IsNull (ex.ParamName, "#D7");
			}
#endif

			Assert.AreEqual (-1, tab.SelectedIndex, "#E1");
			tab.SelectedIndex = -1;
			Assert.AreEqual (-1, tab.SelectedIndex, "#E2");
			Assert.AreEqual (0, _selected_index_changed, "#E3");

			form.Dispose ();
		}

		[Test]
		[Category ("NotWorking")]
		public void InputKeyTest ()
		{
			TabControlPoker p = new TabControlPoker ();

			foreach (Keys key in Enum.GetValues (typeof (Keys))) {
				switch (key) {
				case Keys.PageUp:
				case Keys.PageDown:
				case Keys.End:
				case Keys.Home:
					continue;
				}
				Assert.IsFalse (p.CheckIsInputKey (key), "FALSE- " + key);
			}

			Assert.IsTrue (p.CheckIsInputKey (Keys.PageUp), "TRUE-pageup");
			Assert.IsTrue (p.CheckIsInputKey (Keys.PageDown), "TRUE-pagedown");
			Assert.IsTrue (p.CheckIsInputKey (Keys.End), "TRUE-end");
			Assert.IsTrue (p.CheckIsInputKey (Keys.Home), "TRUE-home");

			// Create the handle, things are a little different with
			// the handle created
			IntPtr dummy = p.Handle;
			
			TestHelper.RemoveWarning (dummy);

			foreach (Keys key in Enum.GetValues (typeof (Keys))) {
				switch (key) {
				case Keys.Left:
				case Keys.Right:
				case Keys.Up:
				case Keys.Down:
				case Keys.PageUp:
				case Keys.PageDown:
				case Keys.End:
				case Keys.Home:
					continue;
				}
				Assert.IsFalse (p.CheckIsInputKey (key), "PH-FALSE- " + key);
			}

			Assert.IsTrue (p.CheckIsInputKey (Keys.Left), "PH-TRUE-left");
			Assert.IsTrue (p.CheckIsInputKey (Keys.Right), "PH-TRUE-right");
			Assert.IsTrue (p.CheckIsInputKey (Keys.Up), "PH-TRUE-up");
			Assert.IsTrue (p.CheckIsInputKey (Keys.Down), "PH-TRUE-down");
			Assert.IsTrue (p.CheckIsInputKey (Keys.PageUp), "PH-TRUE-pageup");
			Assert.IsTrue (p.CheckIsInputKey (Keys.PageDown), "PH-TRUE-pagedown");
			Assert.IsTrue (p.CheckIsInputKey (Keys.End), "PH-TRUE-end");
			Assert.IsTrue (p.CheckIsInputKey (Keys.Home), "PH-TRUE-home");
		}

		[Test] // bug #79847
		public void NoTabPages ()
		{
			Form form = new Form ();
			TabControl tab = new TabControl ();
			tab.SelectedIndex = 0;
			Assert.AreEqual (0, tab.SelectedIndex, "#A1");

			form.Controls.Add (tab);

			Assert.AreEqual (0, tab.SelectedIndex, "#A2");

			form.ShowInTaskbar = false;
			form.Show ();

			Assert.AreEqual (-1, tab.SelectedIndex, "#A3");

			form.Dispose ();
		}

		private void SelectedIndexChanged (object sender, EventArgs e)
		{
			_selected_index_changed++;
		}
		
#if NET_2_0
		[Test]
		public void MethodSelectTab ()
		{
			TabControl tc = new TabControl ();
			tc.TabPages.Add (new TabPage ("One"));
			tc.TabPages.Add (new TabPage ("Two"));
			
			tc.SelectTab (1);
			Assert.AreEqual (1, tc.SelectedIndex, "A1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void MethodSelectTabAOORE ()
		{
			TabControl tc = new TabControl ();
			tc.TabPages.Add (new TabPage ("One"));
			tc.TabPages.Add (new TabPage ("Two"));

			tc.SelectTab (-1);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void MethodSelectTabAOORE2 ()
		{
			TabControl tc = new TabControl ();
			tc.TabPages.Add (new TabPage ("One"));
			tc.TabPages.Add (new TabPage ("Two"));

			tc.SelectTab (2);
		}

		[Test]
		public void EventSelected ()
		{
			Form f = new Form ();
			TabControl tc = new TabControl ();
			TabControlEventArgs tcea = new TabControlEventArgs (null, 0, TabControlAction.Deselected);
			
			f.Controls.Add (tc);
			string events = string.Empty;
			tc.SelectedIndexChanged += new EventHandler (delegate (Object obj, EventArgs e) { events += ("SelectedIndexChanged;"); });
			tc.Selected += new TabControlEventHandler (delegate (Object obj, TabControlEventArgs e) { events += ("Selected;"); tcea = e; });
			
			TabPage tp1 = new TabPage ("One");
			TabPage tp2 = new TabPage ("Two");
			
			tc.TabPages.Add (tp1);
			tc.TabPages.Add (tp2);
			
			f.Show ();
			tc.SelectTab (1);
			Assert.AreEqual ("Selected;SelectedIndexChanged;", events, "A1");
			Assert.AreEqual (TabControlAction.Selected, tcea.Action, "A2");
			Assert.AreSame (tp2, tcea.TabPage, "A3");
			Assert.AreEqual (1, tcea.TabPageIndex, "A4");
			f.Close ();
		}
#endif
	}
}
