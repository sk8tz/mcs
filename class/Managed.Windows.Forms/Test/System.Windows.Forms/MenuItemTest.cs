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
//	Jordi Mas i Hernandez <jordi@ximian.com>
//
//

using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class MenuItemTest
	{
		[Test]
		public void MenuItemDefaultValues ()
		{
			MenuItem mi = new MenuItem ();

			Assert.IsFalse (mi.BarBreak, "DefaultValues#1");
			Assert.IsFalse (mi.Break, "DefaultValues#2");
			Assert.IsFalse (mi.Checked, "DefaultValues#3");
			Assert.IsFalse (mi.DefaultItem, "DefaultValues#4");
			Assert.IsTrue (mi.Enabled, "DefaultValues#5");
			Assert.AreEqual (-1, mi.Index, "DefaultValues#6");
			Assert.IsFalse (mi.IsParent, "DefaultValues#7");
			Assert.IsFalse (mi.MdiList, "DefaultValues#8");

			Assert.AreEqual (0, mi.MergeOrder, "DefaultValues#9");
			Assert.AreEqual (MenuMerge.Add, mi.MergeType, "DefaultValues#10");
			Assert.AreEqual ('\0', mi.Mnemonic, "DefaultValues#11");

			Assert.IsFalse (mi.OwnerDraw, "DefaultValues#12");
			Assert.IsNull (mi.Parent, "DefaultValues#13");
			Assert.IsFalse (mi.RadioCheck, "DefaultValues#14");
			Assert.AreEqual (Shortcut.None, mi.Shortcut, "DefaultValues#15");
			Assert.IsTrue (mi.ShowShortcut, "DefaultValues#16");
			Assert.AreEqual (string.Empty, mi.Text, "DefaultValues#17");
			Assert.IsTrue (mi.Visible, "DefaultValues#18");
		}

		[Test]
		public void MenuItemConstructors ()
		{
			MenuItem mi;
			MenuItem subitem1 = new MenuItem ("SubItem1");
			MenuItem subitem2 = new MenuItem ("SubItem2");
			MenuItem subitem3 = new MenuItem ("SubItem3");

			mi = new MenuItem ("Item1");
			Assert.AreEqual ("Item1", mi.Text, "Constructor#1");

			mi = new MenuItem ("Item2", new MenuItem [] {subitem1,
				subitem2, subitem3});

			Assert.AreEqual ("Item2", mi.Text, "Constructor#2");
			Assert.AreEqual (3, mi.MenuItems.Count, "Constructor#3");

			Assert.AreEqual ("SubItem1", mi.MenuItems[0].Text, "Constructor#4");
			Assert.AreEqual (mi, mi.MenuItems[0].Parent, "Constructor#5");

			Assert.AreEqual ("SubItem2", mi.MenuItems[1].Text, "Constructor#6");
			Assert.AreEqual (mi, mi.MenuItems[1].Parent, "Constructor#7");

			Assert.AreEqual ("SubItem3", mi.MenuItems[2].Text, "Constructor#8");
			Assert.AreEqual (mi, mi.MenuItems[2].Parent, "Constructor#9");
		}

		[Test]
		public void MenuItemSimpleMerge ()
		{
			MainMenu mymainmenu1 = new MainMenu ();
			MainMenu mymainmenu2 = new MainMenu ();
			MenuItem mymenuitem1 = new MenuItem ();
			MenuItem mymenuitem2 = new MenuItem ();
			mymenuitem1.Text = "A";
			mymenuitem2.Text = "B";
			mymainmenu1.MenuItems.Add (mymenuitem1);
			mymainmenu2.MenuItems.Add (mymenuitem2);
			mymainmenu1.MergeMenu (mymainmenu2);
			Assert.AreEqual (2, mymainmenu1.MenuItems.Count, "SimpleMerge#1");
		}

		[Test]
		[Category ("NotWorking")]
		public void MenuItemMerge ()
		{
			MenuItem item1 = new MenuItem ("File (0)");		// Position 0
			MenuItem item2 = new MenuItem ("Print the file (1)");	// Position 1
			MenuItem item3 = new MenuItem ("Print Preview (2)");	// Position 2
			MenuItem item4 = new MenuItem ("-");			// Position 3
			MenuItem item5 = new MenuItem ("Recent files (4)");	// Position 4
			MenuItem item6 = new MenuItem ("Exit (5)");		// Position 5

			MenuItem item10 = new MenuItem ("Compare... (6)");	// Replace pos 3
			MenuItem item11 = new MenuItem ("Sort (7)");
			MenuItem item12 = new MenuItem ("Conversions (8)");
			MenuItem item13 = new MenuItem ("Specials Functions (9)");
			MenuItem item14 = new MenuItem ("Another option... (10)");

			MenuItem item20 = new MenuItem ("autors.doc");
			MenuItem item21 = new MenuItem ("testing.html");
			MenuItem item22 = new MenuItem ("proves.txt");

			MenuItem[] first_items = new MenuItem[] {item1, item2, item3, item4, item5, item6};
			MenuItem[] second_items = new MenuItem[] {item10, item11, item12, item13, item14};
			MenuItem[] third_items = new MenuItem[] {item20, item21, item22};

			item14.MergeType = MenuMerge.Remove;
			item10.MergeOrder = 40;

			item11.MergeType = MenuMerge.Replace;
			item11.MergeOrder = 30;
			item12.MergeOrder = 5;
			item10.MergeType = MenuMerge.MergeItems;

			ContextMenu first_menu = new ContextMenu (first_items);
			ContextMenu second_menu = new ContextMenu (second_items);
			ContextMenu third_menu = new ContextMenu (third_items);

			first_menu.MergeMenu (second_menu);
			first_menu.MergeMenu (third_menu);

			Assert.AreEqual ("File (0)", first_menu.MenuItems[0].Text, "ItemMerge#1");
			Assert.AreEqual ("Print the file (1)", first_menu.MenuItems[1].Text, "ItemMerge#2");
			Assert.AreEqual ("Print Preview (2)", first_menu.MenuItems[2].Text, "ItemMerge#3");
			Assert.AreEqual ("-", first_menu.MenuItems[3].Text, "ItemMerge#4");
			Assert.AreEqual ("Recent files (4)", first_menu.MenuItems[4].Text, "ItemMerge#5");
			Assert.AreEqual ("Exit (5)", first_menu.MenuItems[5].Text, "ItemMerge#6");
			Assert.AreEqual ("Specials Functions (9)", first_menu.MenuItems[6].Text, "ItemMerge#7");
			Assert.AreEqual ("autors.doc", first_menu.MenuItems[7].Text, "ItemMerge#8");
			Assert.AreEqual ("testing.html", first_menu.MenuItems[8].Text, "ItemMerge#9");
			Assert.AreEqual ("proves.txt", first_menu.MenuItems[9].Text, "ItemMerge#10");
			Assert.AreEqual ("Conversions (8)", first_menu.MenuItems[10].Text, "ItemMerge#11");
			Assert.AreEqual ("Sort (7)", first_menu.MenuItems[11].Text, "ItemMerge#11");
			Assert.AreEqual ("Compare... (6)", first_menu.MenuItems[12].Text, "ItemMerge#11");
		}

		[Test]
		public void Text ()
		{
			MenuItem mi1 = new MenuItem ();
			mi1.Text = "A1";
			Assert.AreEqual ("A1", mi1.Text, "#1");

			MenuItem mi2 = new MenuItem ();
			mi2.Text = "A2a";
			Assert.AreEqual ("A2a", mi2.Text, "#2");

			mi1.MenuItems.Add (mi2);
			mi2.Text = "A2b";
			Assert.AreEqual ("A2b", mi2.Text, "#3");

			MainMenu mainMenu = new MainMenu ();
			mainMenu.MenuItems.Add (mi1);

			mi1.Text = "B1";
			Assert.AreEqual ("B1", mi1.Text, "#4");
			mi2.Text = "B2";
			Assert.AreEqual ("B2", mi2.Text, "#5");

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Menu = mainMenu;
			form.Show ();

			Assert.AreEqual ("B1", mi1.Text, "#6");
			Assert.AreEqual ("B2", mi2.Text, "#7");
			mi1.Text = "C1";
			Assert.AreEqual ("C1", mi1.Text, "#8");
			mi2.Text = "C2";
			Assert.AreEqual ("C2", mi2.Text, "#9");
		}

		bool event_reached;
		void event_test (object sender, EventArgs e)
		{
			event_reached = true;
		}

#if false
		void test_drawevent (object sender, DrawItemEventArgs e)
		{
			event_reached = true;
		}

		void test_measureevent (object sender, MeasureItemEventArgs e)
		{
			event_reached = true;
		}
#endif

		[Test]
		public void CloneEventTest ()
		{
			MenuItem mi1, mi2;

			event_reached = false;
			mi1 = new MenuItem ();
			mi1.Click += new EventHandler (event_test);
			mi2 = mi1.CloneMenu ();
			mi2.PerformClick ();
			Assert.IsTrue (event_reached);

			event_reached = false;
			mi1 = new MenuItem ();
			mi1.Select += new EventHandler (event_test);
			mi2 = mi1.CloneMenu ();
			mi2.PerformSelect ();
			Assert.IsTrue (event_reached);

#if no
			// these three can't be tested because of the broken MenuItem.CloneMenu

			event_reached = false;
			mi1 = new MenuItem ();
			mi1.Popup += new EventHandler (test_event);
			mi2 = mi1.CloneMenu ();
			mi2.PerformPopup ();
			Assert.IsTrue (event_reached);

			event_reached = false;
			mi1 = new MenuItem ();
			mi1.DrawItem += new DrawItemEventHandler (test_drawevent);
			mi2 = mi1.CloneMenu ();
			mi2.PerformDrawItem (new DrawItemEventArgs (null, null, Rectangle.Empty, 1, DrawItemState.None));
			Assert.IsTrue (event_reached);

			event_reached = false;
			mi1 = new MenuItem ();
			mi1.MeasureItem += new MeasureItemEventHandler (test_measureevent);
			mi2 = mi1.CloneMenu ();
			mi2.PerformMeasureItem (new MeasureItemEventArgs (null, 1));
			Assert.IsTrue (event_reached);
#endif
		}
	}
}
