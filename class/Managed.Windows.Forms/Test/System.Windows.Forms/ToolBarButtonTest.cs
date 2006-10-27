//
// ToolBarButtonTest.cs: Test cases for ToolBarButton.
//
// Author:
//   Chris Toshok (toshok@ximian.com)
//
// (C) 2006 Novell, Inc. (http://www.novell.com)
//

using System;
using NUnit.Framework;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.Remoting;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class ToolBarButtonTest 
	{
		[Test]
		public void CtorTest1 ()
		{
			ToolBarButton tbb = new ToolBarButton ();
			Assert.IsNull (tbb.DropDownMenu, "A3");
			Assert.IsTrue (tbb.Enabled, "A4");
			Assert.AreEqual (-1, tbb.ImageIndex, "A5");
			Assert.IsFalse (tbb.PartialPush, "A6");
			Assert.IsFalse (tbb.Pushed, "A7");
			Assert.AreEqual (Rectangle.Empty, tbb.Rectangle, "A8");
			Assert.AreEqual (ToolBarButtonStyle.PushButton, tbb.Style, "A8");
			Assert.IsNull (tbb.Tag, "A9");
			Assert.AreEqual ("", tbb.Text, "A10");
			Assert.AreEqual ("", tbb.ToolTipText, "A11");
			Assert.IsTrue (tbb.Visible, "A12");
		}

		[Test]
		public void CtorTest2 ()
		{
			ToolBarButton tbb = new ToolBarButton ("hi there");
			Assert.IsNull (tbb.DropDownMenu, "A3");
			Assert.IsTrue (tbb.Enabled, "A4");
			Assert.AreEqual (-1, tbb.ImageIndex, "A5");
			Assert.IsFalse (tbb.PartialPush, "A6");
			Assert.IsFalse (tbb.Pushed, "A7");
			Assert.AreEqual (Rectangle.Empty, tbb.Rectangle, "A8");
			Assert.AreEqual (ToolBarButtonStyle.PushButton, tbb.Style, "A8");
			Assert.IsNull (tbb.Tag, "A9");
			Assert.AreEqual ("hi there", tbb.Text, "A10");
			Assert.AreEqual ("", tbb.ToolTipText, "A11");
			Assert.IsTrue (tbb.Visible, "A12");
		}

		[Test]
		public void ToolTipText ()
		{
			ToolBarButton tbb = new ToolBarButton ();
			Assert.AreEqual ("", tbb.ToolTipText, "A1");

			tbb.ToolTipText = "hi there";
			Assert.AreEqual ("hi there", tbb.ToolTipText, "A2");

			tbb.ToolTipText = null;
			Assert.AreEqual ("", tbb.ToolTipText, "A3");
		}

		[Test]
		public void Text ()
		{
			ToolBarButton tbb = new ToolBarButton ();
			Assert.AreEqual ("", tbb.Text, "A1");

			tbb.Text = "hi there";
			Assert.AreEqual ("hi there", tbb.Text, "A2");

			tbb.Text = null;
			Assert.AreEqual ("", tbb.Text, "A3");
		}
	}

}
