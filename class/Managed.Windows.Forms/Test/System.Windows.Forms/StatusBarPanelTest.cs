//
// StatusBarPanelTest.cs: Test cases for StatusBar.
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
	public class StatusBarPanelTest 
	{
		[Test]
		public void MinimumWidth1 ()
		{
			StatusBarPanel p = new StatusBarPanel ();
			Assert.AreEqual (10, p.MinWidth, "1");
		}

		[Test]
		public void MinimumWidth2 ()
		{
			StatusBarPanel p = new StatusBarPanel ();
			p.Width = 50;
			p.MinWidth = 100;
			Assert.AreEqual (100, p.Width, "1");
		}

		[Test]
		public void MinimumWidth3 ()
		{
			StatusBarPanel p = new StatusBarPanel ();
			p.Width = 50;
			p.MinWidth = 200;
			p.MinWidth = 25;
			Assert.AreEqual (200, p.Width, "#1");
			
			p = new StatusBarPanel ();
			p.Width = 50;
			p.MinWidth = 25;
			Assert.AreEqual (50, p.Width, "#2");
			
			p = new StatusBarPanel ();
			p.Width = 50;
			p.MinWidth = 100;
			Assert.AreEqual (100, p.Width, "#3");
			
			p = new StatusBarPanel ();
			p.MinWidth = 200;
			Assert.AreEqual (200, p.Width, "#4");
		}
		
		[Test]
		public void MinWidth_AutoSize_None ()
		{
			StatusBarPanel p = new StatusBarPanel ();
			p.Width = 50;
			Assert.AreEqual (10, p.MinWidth, "#1");
		}
		
		[Test]
		public void ToStringTest ()
		{
			StatusBarPanel p = new StatusBarPanel ();
			Assert.AreEqual ("StatusBarPanel: {}", p.ToString(), "1");

			p.Text = "Hello";
			Assert.AreEqual ("StatusBarPanel: {Hello}", p.ToString(), "2");

			p.Text = "}";
			Assert.AreEqual ("StatusBarPanel: {}}", p.ToString(), "3");
		}
		
		[Test]
		public void DefaultPropertiesTest ()
		{
			StatusBarPanel p = new StatusBarPanel ();
			Assert.AreEqual (100, p.Width, "#1");
			Assert.AreEqual (10, p.MinWidth, "#2");
			Assert.AreEqual (String.Empty, p.Text, "#3");
			Assert.AreEqual (HorizontalAlignment.Left, p.Alignment, "#4");
			Assert.AreEqual (StatusBarPanelAutoSize.None, p.AutoSize, "#5");
			Assert.AreEqual (StatusBarPanelBorderStyle.Sunken, p.BorderStyle, "#6");
			Assert.AreEqual (StatusBarPanelStyle.Text, p.Style, "#7");
			Assert.AreEqual (String.Empty, p.ToolTipText, "#8");
			
		}
		
	}
}
