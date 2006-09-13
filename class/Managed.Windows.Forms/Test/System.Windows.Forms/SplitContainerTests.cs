#if NET_2_0
using System;
using NUnit.Framework;
using System.Windows.Forms;
using System.Drawing;

namespace ProfessionalColorTest
{
	[TestFixture]
	public class SplitContainerTests
	{
		[Test]
		public void TestSplitContainerConstruction ()
		{
			SplitContainer sc = new SplitContainer ();

			Assert.AreEqual (new Size (150, 100), sc.Size, "A1");
			Assert.AreEqual (FixedPanel.None, sc.FixedPanel, "A2");
			Assert.AreEqual (false, sc.IsSplitterFixed, "A3");
			Assert.AreEqual (Orientation.Vertical, sc.Orientation, "A4");
			Assert.AreEqual (false, sc.Panel1Collapsed, "A6");
			Assert.AreEqual (25, sc.Panel1MinSize, "A7");
			Assert.AreEqual (false, sc.Panel2Collapsed, "A9");
			Assert.AreEqual (25, sc.Panel2MinSize, "A10");
			Assert.AreEqual (50, sc.SplitterDistance, "A11");
			Assert.AreEqual (1, sc.SplitterIncrement, "A12");
			Assert.AreEqual (new Rectangle(50, 0, 4, 100), sc.SplitterRectangle, "A13");
			Assert.AreEqual (4, sc.SplitterWidth, "A14");
			Assert.AreEqual (BorderStyle.None, sc.BorderStyle, "A14");
			Assert.AreEqual (DockStyle.None, sc.Dock, "A15");
		}
		
		[Test]
		public void TestNotReleventProperties()
		{
			// (MSDN lists are the following as "This property is not relevant to this class.")
			SplitContainer sc = new SplitContainer ();
			
			Assert.AreEqual (false, sc.AutoScroll, "B1");
			sc.AutoScroll = true;
			Assert.AreEqual (false, sc.AutoScroll, "B1-2");

			Assert.AreEqual (new Size(0,0), sc.AutoScrollMargin, "B2");
			sc.AutoScrollMargin = new Size (100, 100);
			Assert.AreEqual (new Size (100,100), sc.AutoScrollMargin, "B2-2");

			Assert.AreEqual (new Size (0, 0), sc.AutoScrollMinSize, "B3");
			sc.AutoScrollMinSize = new Size (100, 100);
			Assert.AreEqual (new Size (100, 100), sc.AutoScrollMinSize, "B3-2");

			//Assert.AreEqual (new Point (0, 0), sc.AutoScrollOffset, "B4");
			//sc.AutoScrollOffset = new Point (100, 100);
			//Assert.AreEqual (new Point (100, 100), sc.AutoScrollOffset, "B4-2");

			Assert.AreEqual (new Point (0, 0), sc.AutoScrollPosition, "B5");
			sc.AutoScrollPosition = new Point (100, 100);
			Assert.AreEqual (new Point (0, 0), sc.AutoScrollPosition, "B5-2");

			Assert.AreEqual (false, sc.AutoSize, "B6");
			sc.AutoSize = true;
			Assert.AreEqual (true, sc.AutoSize, "B6-2");

			//Assert.AreEqual (ImageLayout.Tile, sc.BackgroundImageLayout, "B7");
			//sc.BackgroundImageLayout =  ImageLayout.Stretch;
			//Assert.AreEqual (ImageLayout.Stretch, sc.BackgroundImageLayout, "B7-2");

			Assert.AreEqual (null, sc.BindingContext, "B8");
			sc.BindingContext = new BindingContext();
			Assert.AreEqual ("System.Windows.Forms.BindingContext", sc.BindingContext.ToString (), "B8-2");

			Assert.AreEqual (new Padding(0), sc.Padding, "B10");
			sc.Padding = new Padding (7);
			Assert.AreEqual (new Padding (7), sc.Padding, "B10-2");

			Assert.AreEqual (String.Empty, sc.Text, "B11");
			sc.Text = "Hello";
			Assert.AreEqual ("Hello", sc.Text, "B11-2");

		}

		[Test]
		public void TestRelevantProperties ()
		{
			SplitContainer sc = new SplitContainer ();
			
			sc.BorderStyle = BorderStyle.FixedSingle;
			Assert.AreEqual (BorderStyle.FixedSingle, sc.BorderStyle, "C1");

			sc.Dock =  DockStyle.Fill;
			Assert.AreEqual (DockStyle.Fill, sc.Dock, "C2");

			sc.FixedPanel = FixedPanel.Panel1;
			Assert.AreEqual (FixedPanel.Panel1, sc.FixedPanel, "C3");

			sc.IsSplitterFixed = true;
			Assert.AreEqual (true, sc.IsSplitterFixed, "C4");

			sc.Orientation = Orientation.Horizontal;
			Assert.AreEqual (Orientation.Horizontal, sc.Orientation, "C5");

			sc.Panel1Collapsed = true;
			Assert.AreEqual (true, sc.Panel1Collapsed, "C6");
			
			sc.Panel1MinSize = 10;
			Assert.AreEqual (10, sc.Panel1MinSize, "C7");

			sc.Panel2Collapsed = true;
			Assert.AreEqual (true, sc.Panel2Collapsed, "C8");

			sc.Panel2MinSize = 10;
			Assert.AreEqual (10, sc.Panel2MinSize, "C9");

			sc.SplitterDistance = 77;
			Assert.AreEqual (77, sc.SplitterDistance, "C10");
			
			sc.SplitterIncrement = 5;
			Assert.AreEqual (5, sc.SplitterIncrement, "C11");
			
			sc.SplitterWidth = 10;
			Assert.AreEqual (10, sc.SplitterWidth, "C12");
		}
		
		[Test]
		public void TestPanelProperties ()
		{
			SplitContainer sc = new SplitContainer ();
			SplitterPanel p = sc.Panel1;

			Assert.AreEqual (AnchorStyles.Top | AnchorStyles.Left, p.Anchor, "D1");
			p.Anchor = AnchorStyles.None;
			Assert.AreEqual (AnchorStyles.None, p.Anchor, "D1-2");

			Assert.AreEqual (false, p.AutoSize, "D2");
			p.AutoSize = true;
			Assert.AreEqual (true, p.AutoSize, "D2-2");

			//Assert.AreEqual (AutoSizeMode.GrowOnly, p.AutoSizeMode, "D3");
			//p.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			//Assert.AreEqual (AutoSizeMode.GrowOnly, p.AutoSizeMode, "D3-2");

			Assert.AreEqual (BorderStyle.None, p.BorderStyle, "D4");
			p.BorderStyle = BorderStyle.FixedSingle;
			Assert.AreEqual (BorderStyle.FixedSingle, p.BorderStyle, "D4-2");

			//Assert.AreEqual (DockStyle.None, p.Dock, "D5");
			//p.Dock = DockStyle.Left;
			//Assert.AreEqual (DockStyle.Left, p.Dock, "D5-2");

			//Assert.AreEqual (new Point(0,0), p.Location, "D7");
			//p.Location = new Point (10, 10);
			//Assert.AreEqual (new Point (0, 0), p.Location, "D7-2");

			Assert.AreEqual (new Size (0, 0), p.MaximumSize, "D8");
			p.MaximumSize = new Size (10, 10);
			Assert.AreEqual (new Size (10, 10), p.MaximumSize, "D8-2");

			Assert.AreEqual (new Size (0, 0), p.MinimumSize, "D9");
			p.MinimumSize = new Size (10, 10);
			Assert.AreEqual (new Size (10, 10), p.MinimumSize, "D9-2");

			Assert.AreEqual (String.Empty, p.Name, "D10");
			p.Name = "MyPanel";
			Assert.AreEqual ("MyPanel", p.Name, "D10-2");

			// We set a new max/min size above, so let's start over with new controls
			sc = new SplitContainer();
			p = sc.Panel1;
			
			//Assert.AreEqual (new Size (50, 100), p.Size, "D12");
			//p.Size = new Size (10, 10);
			//Assert.AreEqual (new Size (50, 100), p.Size, "D12-2");

			//Assert.AreEqual (0, p.TabIndex, "D13");
			p.TabIndex = 4;
			Assert.AreEqual (4, p.TabIndex, "D13-2");

			Assert.AreEqual (false, p.TabStop, "D14");
			p.TabStop = true;
			Assert.AreEqual (true, p.TabStop, "D14-2");

			Assert.AreEqual (true, p.Visible, "D15");
			p.Visible = false;
			Assert.AreEqual (false, p.Visible, "D15-2");
		}
		
		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void TestPanelHeightProperty ()
		{
			SplitContainer sc = new SplitContainer ();
			SplitterPanel p = sc.Panel1;

			Assert.AreEqual (100, p.Height, "E1");
			
			p.Height = 200;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void TestPanelWidthProperty ()
		{
			SplitContainer sc = new SplitContainer ();
			SplitterPanel p = sc.Panel1;

			Assert.AreEqual (50, p.Width, "F1");

			p.Width = 200;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void TestPanelParentProperty ()
		{
			SplitContainer sc = new SplitContainer ();
			SplitContainer sc2 = new SplitContainer ();
			SplitterPanel p = sc.Panel1;

			Assert.AreEqual (sc, p.Parent, "G1");

			p.Parent = sc2;
		}

		[Test]
		public void TestSplitterPosition ()
		{
			SplitContainer sc = new SplitContainer ();

			Assert.AreEqual (new Rectangle (50, 0, 4, 100), sc.SplitterRectangle, "H1");
			
			sc.Orientation = Orientation.Horizontal;
			Assert.AreEqual (new Rectangle (0, 50, 150, 4), sc.SplitterRectangle, "H2");
		}

		[Test]
		[Ignore ("SplitContainer.FixedPanel not yet implemented")]
		public void TestFixedPanelNone ()
		{
			SplitContainer sc = new SplitContainer ();

			Assert.AreEqual (50, sc.SplitterDistance, "I1");

			sc.Width = 300;
			Assert.AreEqual (100, sc.SplitterDistance, "I2");
		}
		
		[Test]
		[Ignore ("SplitContainer.FixedPanel not yet implemented")]
		public void TestFixedPanel1 ()
		{
			SplitContainer sc = new SplitContainer ();
			sc.FixedPanel = FixedPanel.Panel1;
			
			Assert.AreEqual (50, sc.SplitterDistance, "J1");

			sc.Width = 300;
			Assert.AreEqual (50, sc.SplitterDistance, "J2");
		}
		
		[Test]
		[Ignore ("SplitContainer.FixedPanel not yet implemented")]
		public void TestFixedPanel2 ()
		{
			SplitContainer sc = new SplitContainer ();
			sc.FixedPanel = FixedPanel.Panel2;

			Assert.AreEqual (50, sc.SplitterDistance, "K1");

			sc.Width = 300;
			Assert.AreEqual (200, sc.SplitterDistance, "K2");
		}

		[Test]
		public void TestSplitterDistance ()
		{
			SplitContainer sc = new SplitContainer ();

			Assert.AreEqual (new Rectangle (50, 0, 4, 100), sc.SplitterRectangle, "L1");

			sc.SplitterDistance = 100;
			Assert.AreEqual (new Rectangle (100, 0, 4, 100), sc.SplitterRectangle, "L2");
		}

		[Test]
		public void TestSplitterWidth ()
		{
			SplitContainer sc = new SplitContainer ();

			Assert.AreEqual (new Rectangle (50, 0, 4, 100), sc.SplitterRectangle, "M1");

			sc.SplitterWidth = 10;
			Assert.AreEqual (new Rectangle (50, 0, 10, 100), sc.SplitterRectangle, "M2");
		}
	}
}
#endif