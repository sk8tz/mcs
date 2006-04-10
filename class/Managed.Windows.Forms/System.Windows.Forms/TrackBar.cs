//
// System.Windows.Forms.TrackBar.cs
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
//
// Copyright (c) 2004-2006 Novell, Inc.
//
// Authors:
//	Jordi Mas i Hernandez, jordi@ximian.com
//
// TODO:
//		- The AutoSize functionality seems quite broken for vertical controls in .Net 1.1. Not
//		sure if we are implementing it the right way.
//


// NOT COMPLETE

using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Timers;

namespace System.Windows.Forms
{	
	[Designer("System.Windows.Forms.Design.TrackBarDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[DefaultEvent ("Scroll")]
	[DefaultProperty("Value")]
	public class TrackBar : Control, ISupportInitialize
	{
		private int minimum;
		private int maximum;
		internal int tickFrequency;
		private bool autosize;
		private int position;
		private int smallChange;
		private int largeChange;
		private Orientation orientation;
		private TickStyle tickStyle;		
		private Rectangle thumb_pos = new Rectangle ();	 /* Current position and size of the thumb */
		private Rectangle thumb_area = new Rectangle (); /* Area where the thumb can scroll */
		internal bool thumb_pressed = false;		 
		private System.Timers.Timer holdclick_timer = new System.Timers.Timer ();
		internal int thumb_mouseclick;		
		private bool mouse_clickmove;

		#region events
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler Click {
			add { base.Click += value; }
			remove { base.Click -= value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler DoubleClick {
			add { base.DoubleClick += value; }
			remove { base.DoubleClick -= value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler FontChanged {
			add { base.FontChanged += value; }
			remove { base.FontChanged -= value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler ForeColorChanged {
			add { base.ForeColorChanged += value; }
			remove { base.ForeColorChanged -= value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler ImeModeChanged {
			add { base.ImeModeChanged += value; }
			remove { base.ImeModeChanged -= value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event PaintEventHandler Paint {
			add { base.Paint += value; }
			remove { base.Paint -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}

		public event EventHandler Scroll;
		public event EventHandler ValueChanged;
		
		#endregion // Events

		public TrackBar ()
		{
			orientation = Orientation.Horizontal;
			minimum = 0;
			maximum = 10;
			tickFrequency = 1;
			autosize = true;
			position = 0;
			tickStyle = TickStyle.BottomRight;
			smallChange = 1;
			largeChange = 5;			
			mouse_clickmove = false;			
			MouseDown += new MouseEventHandler (OnMouseDownTB); 
			MouseUp += new MouseEventHandler (OnMouseUpTB); 
			MouseMove += new MouseEventHandler (OnMouseMoveTB);
			KeyDown += new KeyEventHandler (OnKeyDownTB);
			holdclick_timer.Elapsed += new ElapsedEventHandler (OnFirstClickTimer);

			SetStyle (ControlStyles.UserPaint | ControlStyles.Opaque, false);
		}

		#region Private & Internal Properties
		internal Rectangle ThumbPos {
			get {
				return thumb_pos;
			}

			set {
				thumb_pos = value;
			}
		}

		internal Rectangle ThumbArea {
			get {
				return thumb_area;
			}

			set {
				thumb_area = value;
			}
		}
		#endregion	// Private & Internal Properties

		#region Public Properties

		[DefaultValue (true)]
		public bool AutoSize {
			get { return autosize; }
			set { autosize = value;}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Browsable (false)]
		public override Image BackgroundImage {
			get { return base.BackgroundImage; }
			set { base.BackgroundImage = value; }
		}

		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}

		protected override ImeMode DefaultImeMode {
			get {return ImeMode.Disable; }
		}

		protected override Size DefaultSize {
			get { return ThemeEngine.Current.TrackBarDefaultSize; }
		}	
		
		[Browsable(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Font Font {
			get { return base.Font;	}
			set { base.Font = value; }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	
		[Browsable (false)]
		public override Color ForeColor {
			get { return base.ForeColor; }
			set { base.ForeColor = value; }
		}		

		[EditorBrowsable (EditorBrowsableState.Never)]	
		[Browsable (false)]
		public new ImeMode ImeMode {
			get { return base.ImeMode; }
			set { base.ImeMode = value; }
		}
		
		[DefaultValue (5)]
		public int LargeChange 
		{
			get { return largeChange; }
			set {
				if (value < 0)
					throw new Exception( string.Format("Value '{0}' must be greater than or equal to 0.", value));

				largeChange = value;				
			}
		}

		[DefaultValue (10)]
		[RefreshProperties (RefreshProperties.All)]		
		public int Maximum {
			get { return maximum; }
			set {
				if (maximum != value)  {
					maximum = value;

					if (maximum < minimum)
						minimum = maximum;

					Refresh ();
				}
			}
		}

		[DefaultValue (0)]
		[RefreshProperties (RefreshProperties.All)]		
		public int Minimum {
			get { return minimum; }
			set {

				if (Minimum != value) {
					minimum = value;

					if (minimum > maximum)
						maximum = minimum;

					Refresh ();
				}
			}
		}

		[DefaultValue (Orientation.Horizontal)]
		[Localizable (true)]
		public Orientation Orientation {
			get { return orientation; }
			set {
				if (!Enum.IsDefined (typeof (Orientation), value))
					throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for Orientation", value));

				/* Orientation can be changed once the control has been created */
				if (orientation != value) {
					orientation = value;
				
					int old_witdh = Width;
					Width = Height;
					Height = old_witdh;
					Refresh (); 
				}
			}
		}

		[DefaultValue (1)]
		public int SmallChange {
			get { return smallChange;}
			set {
				if ( value < 0 )
					throw new Exception( string.Format("Value '{0}' must be greater than or equal to 0.", value));

				if (smallChange != value) {
					smallChange = value;					
				}
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Bindable (false)]
		[Browsable (false)]
		public override string Text {
			get {	return base.Text; }			
			set { base.Text = value; }
		}

		[DefaultValue (1)]
		public int TickFrequency {
			get { return tickFrequency; }
			set {
				if ( value > 0 ) {
					tickFrequency = value;
					Refresh ();
				}
			}
		}

		[DefaultValue (TickStyle.BottomRight)]
		public TickStyle TickStyle {
			get { return tickStyle; }
			set { 				
				if (!Enum.IsDefined (typeof (TickStyle), value))
					throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for TickStyle", value));
				
				if (tickStyle != value) {
					tickStyle = value;
					Refresh ();
				}
			}
		}
		
		[DefaultValue (0)]
		[Bindable (true)]
		public int Value {
			get { return position; }
			set {
				if (value < Minimum || value > Maximum)
					throw new ArgumentException(
						string.Format("'{0}' is not a valid value for 'Value'. 'Value' should be between 'Minimum' and 'Maximum'", value));
				
				if (position != value) {													
					position = value;					
					
					if (ValueChanged != null)				
						ValueChanged (this, new EventArgs ());
						
					Invalidate (thumb_area);
				}				
			}
		}

		#endregion //Public Properties

		#region Public Methods

		public void BeginInit ()		
		{

		}

		protected override void CreateHandle ()
		{
			base.CreateHandle ();
		}


		public void EndInit ()		
		{

		}

		protected override bool IsInputKey (Keys keyData)
		{
			if ((keyData & Keys.Alt) == 0) {
				switch (keyData & Keys.KeyCode) {
				case Keys.Down:
				case Keys.Right:
				case Keys.Up:
				case Keys.Left:
				case Keys.PageUp:
				case Keys.PageDown:
				case Keys.Home:
				case Keys.End:
					return true;
				}
			}
			return base.IsInputKey (keyData);
		}

		protected override void OnBackColorChanged (EventArgs e)
		{
			base.OnBackColorChanged (e);
		}

		protected override void OnHandleCreated (EventArgs e)
		{	
			base.OnHandleCreated (e);
					
			if (AutoSize)
				if (Orientation == Orientation.Horizontal)
					Size = new Size (Width, 40);
				else
					Size = new Size (50, Height);
			
			UpdatePos (Value, true);			
		}
	
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void OnMouseWheel (MouseEventArgs e)
		{
			base.OnMouseWheel (e);
			
			if (!Enabled) return;
    			
			if (e.Delta > 0)
				SmallDecrement ();
			else
				SmallIncrement ();    					
		}

		protected virtual void OnScroll (EventArgs e) 
		{
			if (Scroll != null) 
				Scroll (this, e);
		}

		protected virtual void OnValueChanged (EventArgs e) 
		{
			if (ValueChanged != null) 
				ValueChanged (this, e);
		}

		public void SetRange (int minValue, int maxValue)
		{
			Minimum = minValue;
			Maximum = maxValue;			
		}

		public override string ToString()
		{
			return string.Format("System.Windows.Forms.Trackbar, Minimum: {0}, Maximum: {1}, Value: {2}",
						Minimum, Maximum, Value);
		}
							

		protected override void WndProc (ref Message m)
    		{
			base.WndProc (ref m);
    		}
    		
		#endregion Public Methods

		#region Private Methods
		
		private void UpdatePos (int newPos, bool update_trumbpos)
		{
			if (newPos < minimum){
				Value = minimum;
			}
			else {
				if (newPos > maximum) {
					Value = maximum;
				}
				else {
					Value = newPos;
				}
			}
		}
		
		private void LargeIncrement ()
    		{    			
			UpdatePos (position + LargeChange, true);
			Invalidate (thumb_area);
			OnScroll (new EventArgs ());
    		}

    		private void LargeDecrement ()
    		{
			UpdatePos (position - LargeChange, true);
			Invalidate (thumb_area);
			OnScroll (new EventArgs ());
    		}

		private void SmallIncrement ()
    		{    			
			UpdatePos (position + SmallChange, true);
			Invalidate (thumb_area);
			OnScroll (new EventArgs ());
    		}

    		private void SmallDecrement ()
    		{
			UpdatePos (position - SmallChange, true);
			Invalidate (thumb_area);
			OnScroll (new EventArgs ());	
    		}
    		
		private void OnMouseUpTB (object sender, MouseEventArgs e)
		{	
			if (!Enabled) return;			

			if (thumb_pressed == true || mouse_clickmove == true) {	
				thumb_pressed = false;
				holdclick_timer.Enabled = false;
				this.Capture = false;
				Invalidate (thumb_area);
			}
		}

		private void OnMouseDownTB (object sender, MouseEventArgs e)
    		{
    			if (!Enabled) return;			    			

			bool fire_timer = false;
    			
    			Point point = new Point (e.X, e.Y);

			if (orientation == Orientation.Horizontal) {
				
				if (thumb_pos.Contains (point)) {
					this.Capture = true;
					thumb_pressed = true;
					thumb_mouseclick = e.X;
					Invalidate (thumb_area);
				}
				else {
					if (ClientRectangle.Contains (point)) {
						if (e.X > thumb_pos.X + thumb_pos.Width)
							LargeIncrement ();
						else
							LargeDecrement ();

						Invalidate (thumb_area);
						fire_timer = true;
						mouse_clickmove = true;
					}
				}
			}
			else {
				if (thumb_pos.Contains (point)) {
					this.Capture = true;
					thumb_pressed = true;
					thumb_mouseclick = e.Y;
					Invalidate (thumb_area);
					
				}
				else {
					if (ClientRectangle.Contains (point)) {
						if (e.Y > thumb_pos.Y + thumb_pos.Height)
							LargeDecrement ();
						else
							LargeIncrement ();

						Invalidate (thumb_area);
						fire_timer = true;
						mouse_clickmove = true;
					}
				}
			}

			if (fire_timer) { 				
				holdclick_timer.Interval = 300;
				holdclick_timer.Enabled = true;				
			}			
    		}

    		private void OnMouseMoveTB (object sender, MouseEventArgs e)
    		{    			
    			if (!Enabled) return;
    		
    			/* Moving the thumb */
    			if (thumb_pressed) {
								 				
    				if (orientation == Orientation.Horizontal){
					if (ClientRectangle.Contains (e.X, thumb_pos.Y))
						thumb_mouseclick = e.X;	
				}
    				else {
					if (ClientRectangle.Contains (thumb_pos.X, e.Y))
						thumb_mouseclick = e.Y;
				}

				Invalidate (thumb_area);
    				OnScroll (new EventArgs ());
			}
    		}

		internal override void OnPaintInternal (PaintEventArgs pevent)
		{		
			ThemeEngine.Current.DrawTrackBar (pevent.Graphics, pevent.ClipRectangle, this);
		}

		private void OnKeyDownTB (object sender, KeyEventArgs e) 
		{
			switch (e.KeyCode) {
			
			case Keys.Down:
			case Keys.Right:
				SmallDecrement ();
				break;

			case Keys.Up:
			case Keys.Left:
				SmallIncrement ();
				break;

			case Keys.PageUp:
				LargeIncrement ();
				break;

			case Keys.PageDown:
				LargeDecrement ();
				break;

			case Keys.Home:
				Value = Maximum;
				break;

			case Keys.End:
				Value = Minimum;
				break;

			default:
				break;
			}
		}

		private void OnFirstClickTimer (Object source, ElapsedEventArgs e)
		{						
			Point pnt;
			pnt = PointToClient (MousePosition);			

			if (thumb_area.Contains (pnt)) 	{
				if (orientation == Orientation.Horizontal) {
					if (pnt.X > thumb_pos.X + thumb_pos.Width)
						LargeIncrement ();

					if (pnt.X < thumb_pos.X)
						LargeDecrement ();						
				}
				else 				{
					if (pnt.Y > thumb_pos.Y + thumb_pos.Height)
						LargeIncrement ();

					if (pnt.Y < thumb_pos.Y)
						LargeDecrement ();
				}

				Invalidate (thumb_area);

			}			
		}					

		protected override void SetBoundsCore (int x, int y,int width, int height, BoundsSpecified specified)
		{
			base.SetBoundsCore (x, y,width,	height, specified);
		}

		
    		#endregion // Private Methods
	}
}

