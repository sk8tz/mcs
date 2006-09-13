//
// SplitContainer.cs
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
// Copyright (c) 2006 Jonathan Pobst
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

#if NET_2_0
using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms
{
	[ComVisibleAttribute (true)]
	[ClassInterfaceAttribute (ClassInterfaceType.AutoDispatch)]
	public class SplitContainer : ContainerControl
	{
		#region Local Variables
		private FixedPanel fixed_panel;
		private int splitter_distance;
		private int splitter_width;
		private int splitter_increment;
		private Orientation orientation;
		private bool binding_context_set;

		private SplitterPanel panel1;
		private bool panel1_collapsed;
		private int panel1_min_size;

		private SplitterPanel panel2;
		private bool panel2_collapsed;
		private int panel2_min_size;

		private Splitter splitter;
		#endregion

		#region Public Events
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		new public event EventHandler AutoSizeChanged;
		public event SplitterEventHandler SplitterMoved;
		public event SplitterCancelEventHandler SplitterMoving;
		#endregion

		#region Public Constructors
		public SplitContainer ()
		{
			fixed_panel = FixedPanel.None;
			orientation = Orientation.Vertical;
			splitter_distance = 50;
			splitter_width = 4;
			splitter_increment = 1;
			panel1_collapsed = false;
			panel2_collapsed = false;
			panel1_min_size = 25;
			panel2_min_size = 25;
			binding_context_set = false;

			panel1 = new SplitterPanel (this);
			panel2 = new SplitterPanel (this);
			splitter = new Splitter ();

			splitter.TabStop = true;
			splitter.Size = new System.Drawing.Size (4, 4);
			splitter.SplitterMoved += new SplitterEventHandler (splitter_SplitterMoved);
			splitter.SplitterMoving += new SplitterEventHandler (splitter_SplitterMoving);

			panel1.Size = new Size (50, 50);

			this.Controls.Add (panel2);
			this.Controls.Add (splitter);
			this.Controls.Add (panel1);

			panel1.Dock = DockStyle.Left;
			panel2.Dock = DockStyle.Fill;
			splitter.Dock = DockStyle.Left;
		}
		#endregion

		#region Public Properties
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Localizable (true)]
		public override bool AutoScroll {
			get { return base.AutoScroll; }
			set { }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		new public Size AutoScrollMargin {
			get { return base.AutoScrollMargin; }
			set { base.AutoScrollMargin = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		new public Size AutoScrollMinSize {
			get { return base.AutoScrollMinSize; }
			set { base.AutoScrollMinSize = value; }
		}

		//Uncomment once this has been implemented in Control.cs
		//[Browsable (false)]
		//[EditorBrowsable (EditorBrowsableState.Never)]
		//public override Point AutoScrollOffset {
		//        get { return base.AutoScrollOffset; }
		//        set { base.AutoScrollOffset = value; }
		//}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		new public Point AutoScrollPosition {
			get { return base.AutoScrollPosition; }
			set { base.AutoScrollPosition = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override bool AutoSize {
			get { return base.AutoSize; }
			set {
				base.AutoSize = value;
				if (AutoSizeChanged != null) AutoSizeChanged (this, new EventArgs ());
			}
		}

		public override Image BackgroundImage {
			get { return base.BackgroundImage; }
			set {
				base.BackgroundImage = value;
				UpdateSplitterBackground ();
			}
		}

		//Uncomment once this has been implemented in Control.cs
		//[Browsable (false)]
		//[EditorBrowsable (EditorBrowsableState.Never)]
		//public override ImageLayout BackgroundImageLayout {
		//        get { return base.BackgroundImageLayout; }
		//        set { base.BackgroundImageLayout = value; }
		//}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override BindingContext BindingContext {
			get { return binding_context_set ? base.BindingContext : null; }
			set {
				binding_context_set = true;
				base.BindingContext = value;
			}
		}

		// MSDN says default is Fixed3D, creating a new SplitContainer says otherwise.
		[DefaultValue (BorderStyle.None)]
		public BorderStyle BorderStyle
		{
			get { return panel1.BorderStyle; }
			set {
				if (!Enum.IsDefined (typeof (BorderStyle), value))
					throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for BorderStyle", value));
					
				panel1.BorderStyle = value;
				panel2.BorderStyle = value;
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		new public ControlCollection Controls { get { return base.Controls; } }

		new public DockStyle Dock {
			get { return base.Dock; }
			set { base.Dock = value; }
		}

		[DefaultValue (FixedPanel.None)]
		public FixedPanel FixedPanel {
			get { return this.fixed_panel; }
			set {
				if (!Enum.IsDefined (typeof (FixedPanel), value))
					throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for FixedPanel", value));

				this.fixed_panel = value;
			}
		}

		[Localizable (true)]
		[DefaultValue (false)]
		public bool IsSplitterFixed {
			get { return !splitter.Enabled; }
			set { splitter.Enabled = !value; }
		}

		[Localizable (true)]
		[DefaultValue (Orientation.Vertical)]
		public Orientation Orientation {
			get { return this.orientation; }
			set {
				if (!Enum.IsDefined (typeof (Orientation), value))
					throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for Orientation", value));

				if (this.orientation != value) {
					this.orientation = value;

					switch (value) {
						case Orientation.Vertical:
							panel1.Dock = DockStyle.Left;
							panel2.Dock = DockStyle.Fill;
							splitter.Dock = DockStyle.Left;
							splitter.Width = this.splitter_width;
							panel1.InternalWidth = this.splitter_distance;
							if (panel2.Width < panel2_min_size)
								panel1.InternalWidth = this.Width - this.splitter_width - panel2_min_size;
							break;
						case Orientation.Horizontal:
						default:
							panel1.Dock = DockStyle.Top;
							panel2.Dock = DockStyle.Fill;
							splitter.Dock = DockStyle.Top;
							splitter.Height = this.splitter_width;
							panel1.InternalHeight = this.splitter_distance;
							if (panel2.Height < panel2_min_size)
								panel1.InternalHeight = this.Height - this.splitter_width - panel2_min_size;
							break;
					}

					this.PerformLayout ();
				}
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		new public Padding Padding {
			get { return base.Padding; }
			set { base.Padding = value; }
		}

		[Localizable (false)]
		public SplitterPanel Panel1 { get { return this.panel1; } }

		[DefaultValue (false)]
		public bool Panel1Collapsed {
			get { return this.panel1_collapsed; }
			set {
				this.panel1_collapsed = value;
				this.panel1.Visible = !value;
				this.splitter.Visible = !value;
			}
		}

		[Localizable (true)]
		[DefaultValue (25)]
		public int Panel1MinSize {
			get { return this.panel1_min_size; }
			set { 
				this.panel1_min_size = value; 
				this.splitter.MinSize = value; 
			}
		}

		[Localizable (false)]
		public SplitterPanel Panel2 { get { return this.panel2; } }

		[DefaultValue (false)]
		public bool Panel2Collapsed {
			get { return this.panel2_collapsed; }
			set {
				this.panel2_collapsed = value; 
				this.panel2.Visible = !value;
				this.splitter.Visible = !value;
			}
		}

		[Localizable (true)]
		[DefaultValue (25)]
		public int Panel2MinSize {
			get { return this.panel2_min_size; }
			set { this.panel2_min_size = value; this.splitter.MinExtra = value; }
		}

		// MSDN says the default is 40, MS's implementation defaults to 50.
		[Localizable (true)]
		[DefaultValue (50)]
		public int SplitterDistance {
			get { return this.splitter_distance; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ();

				if (value < this.panel1_min_size)
					value = this.panel1_min_size;

				switch (this.orientation) {
					case Orientation.Vertical:
						if (value > this.Width - this.panel2_min_size - this.splitter_width)
							value = this.Width - this.panel2_min_size - this.splitter_width;
						panel1.InternalWidth = value;
						break;
					case Orientation.Horizontal:
					default:
						if (value > this.Height - this.panel2_min_size - this.splitter_width)
							value = this.Height - this.panel2_min_size - this.splitter_width;
						panel1.InternalHeight = value;
						break;
				}

				this.splitter_distance = value;

				UpdateSplitterBackground ();
			}
		}

		[Localizable (true)]
		[DefaultValue (1)]
		[MonoTODO ("Not implemented.")]
		public int SplitterIncrement {
			get { return this.splitter_increment; }
			set { this.splitter_increment = value; }
		}

		[Browsable (false)]
		public Rectangle SplitterRectangle { get { return splitter.Bounds; } }

		[Localizable (true)]
		[DefaultValue (4)]
		public int SplitterWidth {
			get { return this.splitter_width; }
			set {
				if (value < 1)
					throw new ArgumentOutOfRangeException ();

				this.splitter_width = value;

				switch (this.orientation) {
					case Orientation.Horizontal:
						splitter.Height = value;
						break;
					case Orientation.Vertical:
					default:
						splitter.Width = value;
						break;
				}
			}
		}

		[DefaultValue (true)]
		new public bool TabStop {
			get { return splitter.TabStop; }
			set { splitter.TabStop = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Bindable (false)]
		public override string Text {
			get { return base.Text; }
			set { base.Text = value; }
		}
		#endregion

		#region Protected Properties
		protected override Size DefaultSize { get { return new Size (150, 100); } }
		#endregion

		#region Public Methods
		public void OnSplitterMoved (SplitterEventArgs e)
		{
			if (SplitterMoved != null) SplitterMoved (this, e);
		}

		public void OnSplitterMoving (SplitterCancelEventArgs e)
		{
			if (SplitterMoving != null) SplitterMoving (this, e);

			if (e.Cancel == true) {
				e.SplitX = splitter.Location.X;
				e.SplitY = splitter.Location.Y;
			}
		}
		#endregion

		#region Private Methods
		private void splitter_SplitterMoving (object sender, SplitterEventArgs e)
		{
			SplitterCancelEventArgs ea = new SplitterCancelEventArgs (e.X, e.Y, e.SplitX, e.SplitY);
			this.OnSplitterMoving (ea);
			e.SplitX = ea.SplitX;
			e.SplitY = ea.SplitY;
		}

		private void splitter_SplitterMoved (object sender, SplitterEventArgs e)
		{
			this.OnSplitterMoved (e);
		}

		private void UpdateSplitterBackground ()
		{
			if (this.BackgroundImage != null) {
				Bitmap b = new Bitmap (splitter.Width, splitter.Height);
				Graphics.FromImage (b).DrawImage (base.BackgroundImage, new Rectangle (0, 0, b.Width, b.Height), this.SplitterRectangle, GraphicsUnit.Pixel);
				splitter.BackgroundImage = b;
			}
			else
				splitter.BackgroundImage = this.BackgroundImage;
		}
		#endregion
	}
}
#endif