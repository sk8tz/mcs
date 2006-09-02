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
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//	John BouAntoun	jba-mono@optusnet.com.au
//
// TODO:
//		- implement custom formatting of the date time value
//		- implement any behaviour associate with UseUpDown (painting, key and mouse)
//		- implement key processing and responding
//		- fix MonthCalendar Popdown on form move
//		- wire in all events from monthcalendar


using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace System.Windows.Forms {
	[DefaultEvent("ValueChanged")]
	[DefaultProperty("Value")]
	[Designer("System.Windows.Forms.Design.DateTimePickerDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class DateTimePicker : Control {

		#region Public variables
		
		// this class has to have the specified hour, minute and second, as it says in msdn
		public static readonly DateTime MaxDateTime = new DateTime (9998, 12, 31, 23, 59, 59);
		public static readonly DateTime MinDateTime = new DateTime (1753, 1, 1);
		
		#endregion 	// Public variables
		
		#region Local variables
		
		protected static readonly Color DefaultMonthBackColor = ThemeEngine.Current.ColorWindow;
		protected static readonly Color DefaultTitleBackColor = ThemeEngine.Current.ColorActiveCaption;
		protected static readonly Color DefaultTitleForeColor = ThemeEngine.Current.ColorActiveCaptionText;
		protected static readonly Color DefaultTrailingForeColor = Color.Gray;
		
		internal MonthCalendar			month_calendar;
		bool							is_checked;
		string							custom_format;
		LeftRightAlignment				drop_down_align;
		DateTimePickerFormat			format;
		DateTime						max_date;
		DateTime						min_date;
		bool							show_check_box;
		bool							show_up_down;
		string							text;
		DateTime						date_value;
		
		// variables used for drawing and such
		internal int 					up_down_width;
		internal bool 					is_drop_down_visible;
		
		#endregion	// Local variables
		
		#region DateTimePickerAccessibleObject Subclass
		[ComVisible(true)]
		public class DateTimePickerAccessibleObject : ControlAccessibleObject {
			#region DateTimePickerAccessibleObject Local Variables
			private DateTimePicker	owner;
			#endregion	// DateTimePickerAccessibleObject Local Variables

			#region DateTimePickerAccessibleObject Constructors
			public DateTimePickerAccessibleObject(DateTimePicker owner) : base(owner) {
				this.owner = owner;
			}
			#endregion	// DateTimePickerAccessibleObject Constructors

			#region DateTimePickerAccessibleObject Properties
			public override AccessibleStates State {
				get {
					AccessibleStates	retval;

					retval = AccessibleStates.Default;

					if (owner.Checked) {
						retval |= AccessibleStates.Checked;
					}

					return retval;
				}
			}

			public override string Value {
				get {
					return owner.text;
				}
			}
			#endregion	// DateTimePickerAccessibleObject Properties
		}
		#endregion	// DateTimePickerAccessibleObject Sub-class

		#region public constructors
		
		// only public constructor
		public DateTimePicker () {
		
			// initialise the month calendar
			month_calendar = new MonthCalendar (this);
			month_calendar.CalendarDimensions = new Size (1, 1);
			month_calendar.MaxSelectionCount = 1;
			month_calendar.ForeColor = Control.DefaultForeColor;
			month_calendar.BackColor = DefaultMonthBackColor;
			month_calendar.TitleBackColor = DefaultTitleBackColor;
			month_calendar.TitleForeColor = DefaultTitleForeColor;
			month_calendar.TrailingForeColor = DefaultTrailingForeColor;
			month_calendar.Visible = false;

			
			// initialise other variables
			is_checked = false;
			custom_format = string.Empty;
			drop_down_align = LeftRightAlignment.Left;
			format = DateTimePickerFormat.Long;
			max_date = MaxDateTime;
			min_date = MinDateTime;
			show_check_box = false;
			show_up_down = false;
			date_value = DateTime.Now;
			text = FormatValue ();		
			
			up_down_width = 10;
			is_drop_down_visible = false;
			
			month_calendar.DateChanged += new DateRangeEventHandler (MonthCalendarDateChangedHandler);
			month_calendar.DateSelected += new DateRangeEventHandler (MonthCalendarDateSelectedHandler);
			KeyPress += new KeyPressEventHandler (KeyPressHandler);
//			LostFocus += new EventHandler (LostFocusHandler);
			MouseDown += new MouseEventHandler (MouseDownHandler);			
			Paint += new PaintEventHandler (PaintHandler);
			
			SetStyle (ControlStyles.UserPaint | ControlStyles.StandardClick, false);
			SetStyle (ControlStyles.FixedHeight, true);
		}
		
		#endregion
		
		#region public properties
		
		// no reason why this is overridden
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Color BackColor {
			set {
				base.BackColor = value;
			}
			get {
				return base.BackColor;
			}
		}
		
		// no reason why this is overridden
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Image BackgroundImage {
			set {
				base.BackgroundImage = value;
			}
			get {
				return base.BackgroundImage;
			}
		}

		[AmbientValue(null)]
		[Localizable(true)]
		public Font CalendarFont {
			set {
				month_calendar.Font = value;
			}
			get {
				return month_calendar.Font;
			}
		}

		public Color CalendarForeColor {
			set {
				month_calendar.ForeColor = value;
			}
			get {
				return month_calendar.ForeColor;
			}
		}

		public Color CalendarMonthBackground {
			set {
				month_calendar.BackColor = value;
			}
			get {
				return month_calendar.BackColor;
			}
		}

		public Color CalendarTitleBackColor {
			set {
				month_calendar.TitleBackColor = value;
			}
			get {
				return month_calendar.TitleBackColor;
			}
		}

		public Color CalendarTitleForeColor {
			set {
				month_calendar.TitleForeColor = value;
			}
			get {
				return month_calendar.TitleForeColor;
			}
		}

		public Color CalendarTrailingForeColor {
			set {
				month_calendar.TrailingForeColor = value;
			}
			get {
				return month_calendar.TrailingForeColor;
			}
		}
		
		// when checked the value is grayed out
		[Bindable(true)]
		[DefaultValue(true)]
		public bool Checked {
			set {
				if (is_checked != value) {
					is_checked = value;
					// invalidate the value inside this control
					this.Invalidate (date_area_rect);
				}
			}
			get {
				return is_checked;
			}
		}
		
		// the custom format string to format this control with
		[DefaultValue(null)]
		[RefreshProperties(RefreshProperties.Repaint)]
		public string CustomFormat {
			set {
				if (custom_format != value) {
					custom_format = value;
					if (this.Format == DateTimePickerFormat.Custom) {
						// TODO: change the text value of the dtp						
					}
				}
			}
			get {
				return custom_format;
			}
		}
		
		// which side the drop down is to be aligned on
		[DefaultValue(LeftRightAlignment.Left)]
		[Localizable(true)]
		public LeftRightAlignment DropDownAlign {
			set {
				if (drop_down_align != value) {
					drop_down_align = value;
				}
			}
			get {
				return drop_down_align;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Color ForeColor {
			set {
				base.ForeColor = value;
			}
			get {
				return base.ForeColor;
			}
		}
		
		// the format of the date time picker text, default is long
		[RefreshProperties(RefreshProperties.Repaint)]
		public DateTimePickerFormat Format {
			set {
				if (format != value) {
					format = value;
					this.OnFormatChanged (EventArgs.Empty);
					// invalidate the value inside this control
					this.Invalidate (date_area_rect);
				}
			}
			get {
				return format;
			}
		}
		
		public DateTime MaxDate {
			set {
				if (value < min_date) {
					throw new ArgumentException ();
				}
				if (value > MaxDateTime) {
					throw new SystemException ();
				}
				if (max_date != value) {
					max_date = value;
					
					// TODO: verify this is correct behaviour when value > max date
					if (Value > max_date) {
						Value = max_date;
						// invalidate the value inside this control
						this.Invalidate (date_area_rect);
					}
				}
			}
			get {
				return max_date;
			}
		}
		
		public DateTime MinDate {
			set {
				if (value < min_date) {
					throw new ArgumentException ();
				}
				if (value < MinDateTime) {
					throw new SystemException ();
				}
				if (min_date != value) {
					min_date = value;
					
					// TODO: verify this is correct behaviour when value > max date
					if (Value < min_date) {
						Value = min_date;
						// invalidate the value inside this control
						this.Invalidate (date_area_rect);
					}
				}
			}
			get {
				return min_date;
			}
		}
		
		// the prefered height to draw this control using current font
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int PreferredHeight {
			get {
				// Make it proportional
				return (int) Math.Ceiling (Font.Height * 1.5);
			}
		}
		
		// whether or not the check box is shown
		[DefaultValue(false)]
		public bool ShowCheckBox {
			set {
				if (show_check_box != value) {
					show_check_box = value;
					// invalidate the value inside this control
					this.Invalidate (date_area_rect);
				}
			}
			get {
				return show_check_box;
			}
		}
		
		// if true show the updown control, else popup the monthcalendar
		[DefaultValue(false)]
		public bool ShowUpDown {
			set {
				if (show_up_down != value) {
					show_up_down = value;
					// need to invalidate the whole control
					this.Invalidate ();
				}
			}
			get {
				return show_up_down;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override string Text {
			set {
				// TODO: if the format is a custom format we need to do a custom parse here
				DateTime parsed_value = DateTime.Parse (value);
				if (date_value != parsed_value) {
					Value = parsed_value;
				}
				text = FormatValue (); 
			}
			get {
				return text;
			}
		}	

		[Bindable(true)]
		[RefreshProperties(RefreshProperties.All)]
		public DateTime Value {
			set {
				if (date_value != value) {
					date_value = value;
					text = FormatValue ();
					this.OnValueChanged (EventArgs.Empty);
					this.Invalidate (date_area_rect);
				}
			}
			get {
				return date_value;
			}			
		}

		#endregion 	// public properties
		
		#region public methods
		
		// just return the text value
		public override string ToString () {
			return this.Text;
		} 
				
		#endregion 	// public methods
		
		#region public events
		
		// raised when the monthcalendar is closed
		public event EventHandler CloseUp;
		
		// raised when the monthcalendar is opened
		public event EventHandler DropDown;
		
		// raised when the format of the value is changed
		public event EventHandler FormatChanged;
		
		// raised when the date Value is changed
		public event EventHandler ValueChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackColorChanged {
			add {
				base.BackColorChanged += value;
			}

			remove {
				base.BackColorChanged -= value;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add {
				base.BackgroundImageChanged += value;
			}

			remove {
				base.BackgroundImageChanged -= value;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler ForeColorChanged {
			add {
				base.ForeColorChanged += value;
			}

			remove {
				base.ForeColorChanged -= value;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event PaintEventHandler Paint {
			add {
				base.Paint += value;
			}

			remove {
				base.Paint -= value;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public new event EventHandler TextChanged {
			add {
				base.TextChanged += value;
			}

			remove {
				base.TextChanged -= value;
			}
		}
		#endregion	// public events
		
		#region protected properties

		// not sure why we're overriding this one		
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}
		
		// specify the default size for this control
		protected override Size DefaultSize {
			get {
				// todo actually measure this properly
				return new Size (200, PreferredHeight);
			}
		}
		
		#endregion	// protected properties
		
		#region protected methods
		
		// not sure why we're overriding this one
		protected override AccessibleObject CreateAccessibilityInstance () {
			return base.CreateAccessibilityInstance ();
		}
		
		// not sure why we're overriding this one
		protected override void CreateHandle () {
			base.CreateHandle ();
		}
		
		// not sure why we're overriding this one
		protected override void DestroyHandle () {
			base.DestroyHandle ();
		}
		
		// not sure why we're overriding this one
		protected override void Dispose (bool disposing) {
			base.Dispose (disposing);
		}
		
		// find out if this key is an input key for us, depends on which date part is focused
		protected override bool IsInputKey (Keys keyData) {
			// TODO: fix this implementation of IsInputKey
			return false;
		}
		
		// raises the CloseUp event
		protected virtual void OnCloseUp (EventArgs eventargs) {
			if (this.CloseUp != null) {
				this.CloseUp (this, eventargs);
			}
		}
		
		// raise the drop down event
		protected virtual void OnDropDown (EventArgs eventargs) {
			if (this.DropDown != null) {
				this.DropDown (this, eventargs);
			}
		}

		protected override void OnFontChanged(EventArgs e) {
			// FIXME - do we need to update/invalidate/recalc our stuff?
			month_calendar.Font = Font;
			Size = new Size (Size.Width, PreferredHeight);

			base.OnFontChanged (e);
		}
		
		// raises the format changed event
		protected virtual void OnFormatChanged (EventArgs e) {
			if (this.FormatChanged != null) {
				this.FormatChanged (this, e);
			}
		}
		
		// not sure why we're overriding this one 
		protected override void OnSystemColorsChanged (EventArgs e) {
			base.OnSystemColorsChanged (e);
		}
		
		// raise the ValueChanged event
		protected virtual void OnValueChanged (EventArgs eventargs) {
			if (this.ValueChanged != null) {
				this.ValueChanged (this, eventargs);
			}
		}
		
		// overridden to set the bounds of this control properly
		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) {
			// TODO: ensure I implemented the bounds core setting properly.
			if ((specified & BoundsSpecified.Height) == BoundsSpecified.Height ||
				(specified & BoundsSpecified.Size) == BoundsSpecified.Size)  {
				base.SetBoundsCore (x, y, width, DefaultSize.Height, specified);
			} else {
				base.SetBoundsCore (x, y, width, height, specified);
			}
			
			// need to set the rectangles for all the support internal rects
			// this is done here as a optimisation since this is an array of rects
			if ((specified & BoundsSpecified.X) == BoundsSpecified.X ||
				(specified & BoundsSpecified.Y) == BoundsSpecified.Y) {
				// TODO set up all the datepart rects
			}
		}

		// not sure why we're overriding this
		protected override void WndProc (ref Message m) {
			base.WndProc (ref m);
		}
		
		#endregion	// protected methods
		
		#region internal / private properties
		
		// this is the region that the date and the check box is drawn on
		internal Rectangle date_area_rect {
			get {
				Rectangle rect = this.ClientRectangle;
				if (ShowUpDown) {
					// set the space to the left of the up/down button
					if (rect.Width > (up_down_width + 4)) {
						rect.Width -= (up_down_width + 4);
					} else {
						rect.Width = 0;
					}
				} else {
					// set the space to the left of the up/down button
					// TODO make this use up down button
					if (rect.Width > (SystemInformation.VerticalScrollBarWidth + 4)) {
						rect.Width -= SystemInformation.VerticalScrollBarWidth;
					} else {
						rect.Width = 0;
					}
				}
				
				rect.Inflate (-2, -2);
				return rect;
			}
		}
		
		// the rectangle for the drop down arrow
		internal Rectangle drop_down_arrow_rect {
			get {
				Rectangle rect = this.ClientRectangle;
				rect.X = rect.Right - SystemInformation.VerticalScrollBarWidth - 2;
				if (rect.Width > (SystemInformation.VerticalScrollBarWidth + 2)) {
					rect.Width = SystemInformation.VerticalScrollBarWidth;
				} else {
					rect.Width = Math.Max (rect.Width - 2, 0);
				}
				
				rect.Inflate (0, -2);
				return rect;
			}
		}
		
		// the part of the date that is currently hilighted
		internal Rectangle hilight_date_area {
			get {
				// TODO: put hilighted part calculation in here
				return Rectangle.Empty;
			}
		}	
			
		#endregion
		
		#region internal / private methods
		
		private Point CalculateDropDownLocation (Rectangle parent_control_rect, Size child_size, bool align_left)
		{
			// default bottom left
			Point location = new Point(parent_control_rect.Left + 5, parent_control_rect.Bottom);
			// now adjust the alignment
			if (!align_left) {
				location.X = parent_control_rect.Right - child_size.Width;				
			}
			
			Point screen_location = PointToScreen (location);			
			Rectangle working_area = Screen.FromControl(this).WorkingArea;
			// now adjust if off the right side of the screen			
			if (screen_location.X < working_area.X) {
				screen_location.X = working_area.X;
			}  
			// now adjust if it should be displayed above control
			if (screen_location.Y + child_size.Height > working_area.Bottom) {
				screen_location.Y -= (parent_control_rect.Height + child_size.Height);
			}
			return screen_location;
		}
		
		// actually draw this control
		internal void Draw (Rectangle clip_rect, Graphics dc)
		{			
			ThemeEngine.Current.DrawDateTimePicker (dc, clip_rect, this);
		}			
		
		// drop the calendar down
		internal void DropDownMonthCalendar ()
		{
			// ensure the right date is set for the month_calendar
			month_calendar.SetDate (this.date_value);
			// get a rectangle that has the dimensions of the text area,
			// but the height of the dtp control.
			Rectangle align_area = this.date_area_rect;
			align_area.Y = this.ClientRectangle.Y;
			align_area.Height = this.ClientRectangle.Height;
			
			// establish the month calendar's location
			month_calendar.Location = CalculateDropDownLocation (
				align_area,
				month_calendar.Size,
				(this.DropDownAlign == LeftRightAlignment.Left));
			month_calendar.Show ();
			month_calendar.Focus ();
			month_calendar.Capture = true;	
			
			// fire any registered events
			if (this.DropDown != null) {
				this.DropDown (this, EventArgs.Empty);
			}
		}
		
		// hide the month calendar
		internal void HideMonthCalendar () 
		{
			this.is_drop_down_visible = false;
    		Invalidate (drop_down_arrow_rect);
    		month_calendar.Capture = false;
    		if (month_calendar.Visible) {
    			month_calendar.Hide ();
    		}
    	}

		// raised by any key down events
		private void KeyPressHandler (object sender, KeyPressEventArgs e) {
			switch (e.KeyChar) {
				default:
					break;
			}
			e.Handled = true;
		}
		
//		// if we lose focus and the drop down is up, then close it
//		private void LostFocusHandler (object sender, EventArgs e) 
//		{
//			if (is_drop_down_visible && !month_calendar.Focused) {
//				this.HideMonthCalendar ();				
//			}			
//		}
		
		private void MonthCalendarDateChangedHandler (object sender, DateRangeEventArgs e)
		{
			this.Value = e.Start.Date.Add (this.Value.TimeOfDay);
		}

		// fired when a user clicks on the month calendar to select a date
		private void MonthCalendarDateSelectedHandler (object sender, DateRangeEventArgs e)
		{
			this.HideMonthCalendar ();	
			this.Focus ();			
		} 

		// to check if the mouse has come down on this control
		private void MouseDownHandler (object sender, MouseEventArgs e)
		{
			/* Click On button*/
			if (ShowUpDown) {
				// TODO: Process clicking for UPDown
			} else {
				if (is_drop_down_visible == false && drop_down_arrow_rect.Contains (e.X, e.Y)) {
					is_drop_down_visible = true;
					Invalidate (drop_down_arrow_rect);
					DropDownMonthCalendar ();
    			} else {
    				// mouse down on this control anywhere else collapses it
    				if (is_drop_down_visible) {    				
    					HideMonthCalendar ();
    				}
    			} 
    		}
		}
		
		
		// paint this control now
		private void PaintHandler (object sender, PaintEventArgs pe) {
			if (Width <= 0 || Height <=  0 || Visible == false)
    				return;

			Draw (pe.ClipRectangle, pe.Graphics);
		}
		
		private string FormatValue () {
			string ret_value = string.Empty;
			switch (format) {
				case DateTimePickerFormat.Custom:
					// TODO implement custom text formatting
					ret_value = date_value.ToString ();
					break;
				case DateTimePickerFormat.Short:
					ret_value = date_value.ToShortDateString ();
					break;
				case DateTimePickerFormat.Time:
					ret_value = date_value.ToLongTimeString ();
					break;
				default:
					ret_value = date_value.ToLongDateString ();
					break;
			}
			return ret_value;
		}
		
		#endregion		
	}
}
