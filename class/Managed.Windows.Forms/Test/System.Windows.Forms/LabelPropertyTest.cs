//
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//      Ritvik Mayank (mritvik@novell.com)
//
// $Log: LabelPropertyTest.cs,v $
// Adding tests for Label
//



using System;
using NUnit.Framework;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.Remoting;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class LabelTest {

		[Test]
		public void PubPropTest ()
		{
			Label l = new Label ();
			
			Assert.AreEqual (false, l.AllowDrop , "#1");
			Assert.AreEqual (false, l.AccessibilityObject == null, "#2");
			Assert.AreEqual (false , l.AutoSize , "#3");
			Assert.AreEqual (AnchorStyles.Top | AnchorStyles.Left, l.Anchor, "#4");
			Assert.AreEqual (false, l.AutoSize, "#5");	
			
			Assert.AreEqual ("Control", l.BackColor.Name  , "#6");
			Assert.AreEqual (null, l.BindingContext, "#7");
			Assert.AreEqual (null, l.BackgroundImage, "#8");
			Assert.AreEqual (BorderStyle.None , l.BorderStyle, "#9");		
			Assert.AreEqual (23, l.Bottom, "#10");
			Assert.AreEqual (new Rectangle (0,0,100,23), l.Bounds, "#11");
			
			Assert.AreEqual (false, l.CanFocus, "#12");
			Assert.AreEqual (false, l.CanSelect, "#13");
			Assert.AreEqual (false, l.Capture, "#14");
			Assert.AreEqual (true, l.CausesValidation, "#15");
			Assert.AreEqual (new Rectangle (0,0,100,23), l.ClientRectangle, "#16");
			Assert.AreEqual (23, l.ClientSize.Height, "#17a");
			Assert.AreEqual (100, l.ClientSize.Width, "#17b");
			Assert.AreEqual (null, l.Container, "#19");
			Assert.AreEqual (false, l.ContainsFocus, "#20");
			Assert.AreEqual (null, l.ContextMenu, "#21");
			Assert.AreEqual (true, l.Created, "#23");
			Assert.AreEqual (Cursors.Default , l.Cursor, "#24");
			
			Assert.AreEqual (false, l.DataBindings == null  , "#25");
			Assert.AreEqual (false, l.Disposing  , "#27");
			Assert.AreEqual (DockStyle.None, l.Dock, "#28");
			
			Assert.AreEqual (true, l.Enabled, "#29");
			
			Assert.AreEqual (FlatStyle.Standard, l.FlatStyle, "#30");
			Assert.AreEqual (false, l.Focused, "#31");
			Assert.AreEqual (FontFamily.GenericSansSerif, l.Font.FontFamily, "#32");
			Assert.AreEqual (SystemColors.ControlText, l.ForeColor, "#33");
			
			Assert.AreEqual (((IWin32Window)l).Handle, l.Handle, "#34");
			Assert.AreEqual (false, l.HasChildren, "#35");
			Assert.AreEqual (23, l.Height, "#36");
			
			Assert.AreEqual (null, l.Image, "#37");
			Assert.AreEqual (ContentAlignment.MiddleCenter, l.ImageAlign, "#38");
			Assert.AreEqual (-1, l.ImageIndex, "#39");
			Assert.AreEqual (null, l.ImageList, "#40");
			Assert.AreEqual (false, l.InvokeRequired, "#42");
			Assert.AreEqual (false, l.IsAccessible, "#43");
			Assert.AreEqual (false, l.IsDisposed, "#44");
			Assert.AreEqual (true, l.IsHandleCreated, "#45");
			
			Assert.AreEqual (0, l.Left, "#46");
			Assert.AreEqual (Point.Empty, l.Location, "#47");
			
			Assert.AreEqual ("", l.Name, "#48a");
			l.Name = "New Label";
			Assert.AreEqual ("New Label", l.Name, "#48b");
			
			Assert.AreEqual (null, l.Parent, "#49");
			Assert.AreEqual (16, l.PreferredHeight, "#50");
			Assert.AreEqual (0, l.PreferredWidth, "#51");

			Assert.AreEqual (false, l.RecreatingHandle, "#54");
			Assert.AreEqual (null, l.Region, "#55");
			Assert.AreEqual (100, l.Right, "#56");
			Assert.AreEqual (RightToLeft.No, l.RightToLeft, "#57");
			
			Assert.AreEqual (null, l.Site, "#58");
			Assert.AreEqual (23, l.Size.Height, "#59a");
			Assert.AreEqual (100, l.Size.Width, "#59b");
			
			Assert.AreEqual (0, l.TabIndex, "#60");
			Assert.AreEqual (null, l.Tag, "#61");
			Assert.AreEqual ("", l.Text, "#62");
			Assert.AreEqual (ContentAlignment.TopLeft, l.TextAlign, "#63");
			Assert.AreEqual (0, l.Top, "#64");
			Assert.AreEqual (null, l.TopLevelControl, "#65");
			
			Assert.AreEqual (true, l.UseMnemonic, "#66");
			
			Assert.AreEqual (true, l.Visible, "#67");
			
			Assert.AreEqual (100, l.Width, "#68");
		}

		[Test]
		public void LabelEqualsTest () {
			Label s1 = new Label ();
			Label s2 = new Label ();
			s1.Text = "abc";
			s2.Text = "abc";
			Assert.AreEqual (false, s1.Equals (s2), "#69");
			Assert.AreEqual (true, s1.Equals (s1), "#70");
		}
		
		[Test]
		public void LabelScaleTest () {
			Label r1 = new Label ();
			r1.Width = 40;
			r1.Height = 20 ;
			r1.Scale (2);
			Assert.AreEqual (80, r1.Width, "#71");
			Assert.AreEqual (40, r1.Height, "#72");

		}		
	}
}
