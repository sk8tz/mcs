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
// Copyright (c) 2004-2006 Novell, Inc.
//
// Authors:
//	John BouAntoun	jba-mono@optusnet.com.au
//
// REMAINING TODO:
//	- get the date_cell_size and title_size to be pixel perfect match of SWF

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;

namespace System.Windows.Forms {
	[DefaultProperty("SelectionRange")]
	[DefaultEvent("DateChanged")]
	[Designer ("System.Windows.Forms.Design.MonthCalendarDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class MonthCalendar : Control {
		#region Local variables
		DateTime []		annually_bolded_dates;
		Color 			back_color;
		DateTime []		bolded_dates;
		Size 			calendar_dimensions;
		Day 			first_day_of_week;
		Color 			fore_color;
		DateTime 		max_date;
		int 			max_selection_count;
		DateTime 		min_date;
		DateTime []		monthly_bolded_dates;
		int 			scroll_change;
		SelectionRange 		selection_range;
		bool 			show_today;
		bool 			show_today_circle;
		bool 			show_week_numbers;
		Color 			title_back_color;
		Color 			title_fore_color;
		DateTime 		today_date;
		bool 			today_date_set;
		Color 			trailing_fore_color;
		ContextMenu		menu;
		NumericUpDown	year_updown;
		Timer		 	timer;
		
		// internal variables used
		internal DateTime 		current_month;			// the month that is being displayed in top left corner of the grid		
		internal DateTimePicker	owner;					// used if this control is popped up
		internal int 			button_x_offset;
		internal Size 			button_size;
		internal Size			title_size;
		internal Size			date_cell_size;
		internal Size			calendar_spacing;
		internal int			divider_line_offset;
		internal DateTime		clicked_date;
		internal Rectangle 		clicked_rect;
		internal bool			is_date_clicked;
		internal bool			is_previous_clicked;
		internal bool			is_next_clicked;
		internal bool 			is_shift_pressed;
		internal DateTime		first_select_start_date;
		internal int			last_clicked_calendar_index;
		internal Rectangle		last_clicked_calendar_rect;
		internal Font 			bold_font;			// Cache the font in FontStyle.Bold
		internal StringFormat		centered_format;		// Cache centered string format
		private Point			month_title_click_location;
		// this is used to see which item was actually clicked on in the beginning
		// so that we know which item to fire on timer
		//	0: date clicked
		//	1: previous clicked
		//	2: next clicked
		private bool[]			click_state;	
		
		// arraylists used to store new dates
		ArrayList 				added_bolded_dates;
		ArrayList 				removed_bolded_dates;
		ArrayList 				added_annually_bolded_dates;
		ArrayList 				removed_annually_bolded_dates;
		ArrayList 				added_monthly_bolded_dates;
		ArrayList 				removed_monthly_bolded_dates;
		
		
		#endregion	// Local variables

		#region Public Constructors

		public MonthCalendar () {
			// set up the control painting
			SetStyle (ControlStyles.UserPaint | ControlStyles.StandardClick, false);
			
			// mouse down timer
			timer = new Timer ();
			timer.Interval = 500;
			timer.Enabled = false;
			
			// initialise default values 
			DateTime now = DateTime.Now.Date;
			selection_range = new SelectionRange (now, now);
			today_date = now;
			current_month = new DateTime (now.Year , now.Month, 1);

			// iniatialise local members
			annually_bolded_dates = null;
			back_color = ThemeEngine.Current.ColorWindow;
			bolded_dates = null;
			calendar_dimensions = new Size (1,1);
			first_day_of_week = Day.Default;
			fore_color = SystemColors.ControlText;
			max_date = new DateTime (9998, 12, 31);
			max_selection_count = 7;
			min_date = new DateTime (1953, 1, 1);
			monthly_bolded_dates = null;
			scroll_change = 0;
			show_today = true;
			show_today_circle = true;
			show_week_numbers = false;
			title_back_color = ThemeEngine.Current.ColorActiveCaption;
			title_fore_color = ThemeEngine.Current.ColorActiveCaptionText;
			today_date_set = false;
			trailing_fore_color = Color.Gray;
			bold_font = new Font (Font, Font.Style | FontStyle.Bold);
			centered_format = new StringFormat ();
			centered_format.LineAlignment = StringAlignment.Center;
			centered_format.Alignment = StringAlignment.Center;
			
			// initialise the arraylest for bolded dates
			added_bolded_dates = new ArrayList ();
			removed_bolded_dates = new ArrayList ();
			added_annually_bolded_dates = new ArrayList ();
			removed_annually_bolded_dates = new ArrayList ();
			added_monthly_bolded_dates = new ArrayList ();
			removed_monthly_bolded_dates = new ArrayList ();
		
			// intiailise internal variables used
			button_x_offset = 5;
			button_size = new Size (22, 17);
			// default settings based on 8.25 pt San Serif Font
			// Not sure of algorithm used to establish this
			date_cell_size = new Size (24, 16);		// default size at san-serif 8.25
			divider_line_offset = 4;
			calendar_spacing = new Size (4, 5);		// horiz and vert spacing between months in a calendar grid

			// set some state info
			clicked_date = now;
			is_date_clicked = false;
			is_previous_clicked = false;
			is_next_clicked = false;
			is_shift_pressed = false;
			click_state = new bool [] {false, false, false};
			first_select_start_date = now;
			month_title_click_location = Point.Empty;

			// set up context menu
			SetUpContextMenu ();

			
			// event handlers
//			LostFocus += new EventHandler (LostFocusHandler);
			timer.Tick += new EventHandler (TimerHandler);
			MouseMove += new MouseEventHandler (MouseMoveHandler);
			MouseDown += new MouseEventHandler (MouseDownHandler);
			KeyDown += new KeyEventHandler (KeyDownHandler);
			MouseUp += new MouseEventHandler (MouseUpHandler);
			KeyUp += new KeyEventHandler (KeyUpHandler);
			
			// this replaces paint so call the control version			
			base.Paint += new PaintEventHandler (PaintHandler);
		}
		
		// called when this control is added to date time picker
		internal MonthCalendar (DateTimePicker owner) : this () {
			this.owner = owner;
			this.is_visible = false;
			this.Size = this.DefaultSize;
		}

		#endregion	// Public Constructors

		#region Public Instance Properties

		// dates to make bold on calendar annually (recurring)
		[Localizable (true)]
		public DateTime[] AnnuallyBoldedDates {
			set {
				if (annually_bolded_dates == null || annually_bolded_dates != value) {
					annually_bolded_dates = value;
					this.UpdateBoldedDates ();
					this.Invalidate ();
				}
			}
			get {
					return annually_bolded_dates;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Image BackgroundImage {
			get {
				return base.BackgroundImage;
			}
			set {
				base.BackgroundImage = value;
			}
		}


		// the back color for the main part of the calendar
		public override Color BackColor {
			set {
				if (back_color != value) {
					back_color = value;
					this.OnBackColorChanged (EventArgs.Empty);
					this.Invalidate ();
				}
			}
			get {
				return back_color;
			}
		}

		// specific dates to make bold on calendar (non-recurring)
		[Localizable (true)]
		public DateTime[] BoldedDates {
			set {
				if (bolded_dates == null || bolded_dates != value) {
					bolded_dates = value;
					this.UpdateBoldedDates ();
					this.Invalidate ();
				}
			}
			get {
					return bolded_dates;
			}
		}

		// the configuration of the monthly grid display - only allowed to display at most,
		// 1 calendar year at a time, will be trimmed to fit it properly
		[Localizable (true)]
		public Size CalendarDimensions {
			set {
				if (value.Width < 0 || value.Height < 0) {
					throw new ArgumentException ();
				}
				if (calendar_dimensions != value) {
					// squeeze the grid into 1 calendar year
					if (value.Width * value.Height > 12) {
						// iteratively reduce the largest dimension till our
						// product is less than 12
						if (value.Width > 12 && value.Height > 12) {
							calendar_dimensions = new Size (4, 3);
						} else if (value.Width > 12) {
							for (int i = 12; i > 0; i--) {
								if (i * value.Height <= 12) {
									calendar_dimensions = new Size (i, value.Height);
									break;
								}
							}
						} else if (value.Height > 12) {
							for (int i = 12; i > 0; i--) {
								if (i * value.Width <= 12) {
									calendar_dimensions = new Size (value.Width, i);
									break;
								}
							}
						}
					} else {
						calendar_dimensions = value;
					}
					this.Invalidate ();
				}
			}
			get {
				return calendar_dimensions;
			}
		}

		// the first day of the week to display
		[Localizable (true)]
		[DefaultValue (Day.Default)]
		public Day FirstDayOfWeek {
			set {
				if (first_day_of_week != value) {
					first_day_of_week = value;
					this.Invalidate ();
				}
			}
			get {
				return first_day_of_week;
			}
		}

		// the fore color for the main part of the calendar
		public override Color ForeColor {
			set {
				if (fore_color != value) {
					fore_color = value;
					this.OnForeColorChanged (EventArgs.Empty);
					this.Invalidate ();
				}
			}
			get {
				return fore_color;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public ImeMode ImeMode {
			get {
				return ime_mode;
			}

			set {
				if (ime_mode != value) {
					ime_mode = value;

					if (ImeModeChanged != null) {
						ImeModeChanged(this, EventArgs.Empty);
					}
				}
			}
		}

		// the maximum date allowed to be selected on this month calendar
		public DateTime MaxDate {
			set {
				if (value < MinDate) {
					throw new ArgumentException();
				}

				if (max_date != value) {
					max_date = value;
				}
			}
			get {
				return max_date;
			}
		}

		// the maximum number of selectable days
		[DefaultValue (7)]
		public int MaxSelectionCount {
			set {
				if (value < 0) {
					throw new ArgumentException();
				}
		
				// can't set selectioncount less than already selected dates
				if ((SelectionEnd - SelectionStart).Days > value) {
					throw new ArgumentException();
				}
			
				if (max_selection_count != value) {
					max_selection_count = value;
				}
			}
			get {
				return max_selection_count;
			}
		}

		// the minimum date allowed to be selected on this month calendar
		public DateTime MinDate {
			set {
				if (value < new DateTime (1953, 1, 1)) {
					throw new ArgumentException();
				}

				if (value > MaxDate) {
					throw new ArgumentException();
				}

				if (max_date != value) {
					min_date = value;
				}
			}
			get {
				return min_date;
			}
		}

		// dates to make bold on calendar monthly (recurring)
		[Localizable (true)]
		public DateTime[] MonthlyBoldedDates {
			set {
				if (monthly_bolded_dates == null || monthly_bolded_dates != value) {
					monthly_bolded_dates = value;
					this.UpdateBoldedDates ();
					this.Invalidate ();
				}
			}
			get {
					return monthly_bolded_dates;
			}
		}

		// the ammount by which to scroll this calendar by
		[DefaultValue (0)]
		public int ScrollChange {
			set {
				if (value < 0 || value > 20000) {
					throw new ArgumentException();
				}

				if (scroll_change != value) {
					scroll_change = value;
				}
			}
			get {
				// if zero it to the default -> the total number of months currently visible
				if (scroll_change == 0) {
					return CalendarDimensions.Width * CalendarDimensions.Height;
				}
				return scroll_change;
			}
		}


		// the last selected date
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public DateTime SelectionEnd {
			set {
				if (value < MinDate || value > MaxDate) {
					throw new ArgumentException();
				}

				if (SelectionRange.End != value) {
					DateTime old_end = SelectionRange.End; 
					// make sure the end obeys the max selection range count
					if (value < SelectionRange.Start) {
						SelectionRange.Start = value;
					}
					if (value.AddDays((MaxSelectionCount-1)*-1) > SelectionRange.Start) {
						SelectionRange.Start = value.AddDays((MaxSelectionCount-1)*-1);
					}
					SelectionRange.End = value;
					this.InvalidateDateRange (new SelectionRange (old_end, SelectionRange.End));
					this.OnDateChanged (new DateRangeEventArgs (SelectionStart, SelectionEnd));
				}
			}
			get {
				return SelectionRange.End;
			}
		}

		// the range of selected dates
		public SelectionRange SelectionRange {
			set {
				if (selection_range != value) {
					SelectionRange old_range = selection_range;

					// make sure the end obeys the max selection range count
					if (value.End.AddDays((MaxSelectionCount-1)*-1) > value.Start) {
						selection_range = new SelectionRange (value.End.AddDays((MaxSelectionCount-1)*-1), value.End);
					} else {
						selection_range = value;
					}
					SelectionRange visible_range = this.GetDisplayRange(true);
					if(visible_range.Start > selection_range.End) {
						this.current_month = new DateTime (selection_range.Start.Year, selection_range.Start.Month, 1);
						this.Invalidate ();
					} else if (visible_range.End < selection_range.Start) {
						int year_diff = selection_range.End.Year - visible_range.End.Year;
						int month_diff = selection_range.End.Month - visible_range.End.Month;
						this.current_month = current_month.AddMonths(year_diff * 12 + month_diff);
						this.Invalidate ();
					}
					// invalidate the selected range changes
					DateTime diff_start = old_range.Start;
					DateTime diff_end = old_range.End;
					// now decide which region is greated
					if (old_range.Start > SelectionRange.Start) {
						diff_start = SelectionRange.Start;
					} else if (old_range.Start == SelectionRange.Start) {
						if (old_range.End < SelectionRange.End) {
							diff_start = old_range.End;
						} else {
							diff_start = SelectionRange.End;
						}
					}
					if (old_range.End < SelectionRange.End) {
						diff_end = SelectionRange.End;
					} else if (old_range.End == SelectionRange.End) {
						if (old_range.Start < SelectionRange.Start) {
							diff_end = SelectionRange.Start;
						} else {
							diff_end = old_range.Start;
						}
					}


					// invalidate the region required	
					SelectionRange new_range = new SelectionRange (diff_start, diff_end);
					if (new_range.End != old_range.End || new_range.Start != old_range.Start)
						this.InvalidateDateRange (new_range);
					// raise date changed event
					this.OnDateChanged (new DateRangeEventArgs (SelectionStart, SelectionEnd));
				}
			}
			get {
				return selection_range;
			}
		}

		// the first selected date
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public DateTime SelectionStart {
			set {
				if (value < MinDate || value > MaxDate) {
					throw new ArgumentException();
				}

				if (SelectionRange.Start != value) {
					DateTime old_start = SelectionRange.Start; 
					// make sure the end obeys the max selection range count
					if (value > SelectionRange.End) {
						SelectionRange.End = value;
					} else if (value.AddDays(MaxSelectionCount-1) < SelectionRange.End) {
						SelectionRange.End = value.AddDays(MaxSelectionCount-1);
					}
					SelectionRange.Start = value;
					DateTime new_month = new DateTime(value.Year, value.Month, 1);
					if (current_month != new_month) {
						current_month = new_month;
						this.Invalidate ();
					} else {
						this.InvalidateDateRange (new SelectionRange (old_start, SelectionRange.Start));
					}
					this.OnDateChanged (new DateRangeEventArgs (SelectionStart, SelectionEnd));
				}
			}
			get {
				return selection_range.Start;
			}
		}

		// whether or not to show todays date
		[DefaultValue (true)]
		public bool ShowToday {
			set {
				if (show_today != value) {
					show_today = value;
					this.Invalidate ();
				}
			}
			get {
				return show_today;
			}
		}

		// whether or not to show a circle around todays date
		[DefaultValue (true)]
		public bool ShowTodayCircle {
			set {
				if (show_today_circle != value) {
					show_today_circle = value;
					this.Invalidate ();
				}
			}
			get {
				return show_today_circle;
			}
		}

		// whether or not to show numbers beside each row of weeks
		[Localizable (true)]
		[DefaultValue (false)]
		public bool ShowWeekNumbers {
			set {
				if (show_week_numbers != value) {
					show_week_numbers = value;
					this.Invalidate ();
				}
			}
			get {
				return show_week_numbers;
			}
		}

		// the rectangle size required to render one month based on current font
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public Size SingleMonthSize {
			get {
				if (this.Font == null) {
					throw new InvalidOperationException();
				}

				// multiplier is sucked out from the font size
				int multiplier = this.Font.Height;

				// establis how many columns and rows we have					
				int column_count = (ShowWeekNumbers) ? 8 : 7;
				int row_count = 7;		// not including the today date

				// set the date_cell_size and the title_size
				date_cell_size = new Size ((int) Math.Ceiling (1.8 * multiplier), multiplier);
				title_size = new Size ((date_cell_size.Width * column_count), 2 * multiplier);

				return new Size (column_count * date_cell_size.Width, row_count * date_cell_size.Height + title_size.Height);
			}
		}

		[Bindable(false)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override string Text {
			get {
				return base.Text;
			}
			set {
				base.Text = value;
			}
		}

		// the back color for the title of the calendar and the
		// forecolor for the day of the week text
		public Color TitleBackColor {
			set {
				if (title_back_color != value) {
					title_back_color = value;
					this.Invalidate ();
				}
			}
			get {
				return title_back_color;
			}
		}

		// the fore color for the title of the calendar
		public Color TitleForeColor {
			set {
				if (title_fore_color != value) {
					title_fore_color = value;
					this.Invalidate ();
				}
			}
			get {
				return title_fore_color;
			}
		}

		// the date this calendar is using to refer to today's date
		public DateTime TodayDate {
			set {
				today_date_set = true;
				if (today_date != value) {
					today_date = value;
					this.Invalidate ();
				}
			}
			get {
				return today_date;
			}
		}

		// tells if user specifically set today_date for this control		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public bool TodayDateSet {
			get {
				return today_date_set;
			}
		}

		// the color used for trailing dates in the calendar
		public Color TrailingForeColor {
			set {
				if (trailing_fore_color != value) {
					trailing_fore_color = value;
					SelectionRange bounds = this.GetDisplayRange (false);
					SelectionRange visible_bounds = this.GetDisplayRange (true);
					this.InvalidateDateRange (new SelectionRange (bounds.Start, visible_bounds.Start));
					this.InvalidateDateRange (new SelectionRange (bounds.End, visible_bounds.End));
				}
			}
			get {
				return trailing_fore_color;
			}
		}

		#endregion	// Public Instance Properties

		#region Protected Instance Properties

		// overloaded to allow controll to be windowed for drop down
		protected override CreateParams CreateParams {
			get {
				if (this.owner == null) {
					return base.CreateParams;					
				} else {
					CreateParams cp = base.CreateParams;					
					cp.Style ^= (int) WindowStyles.WS_CHILD;
					cp.Style |= (int) WindowStyles.WS_POPUP;
					cp.ExStyle |= (int)(WindowExStyles.WS_EX_TOOLWINDOW | WindowExStyles.WS_EX_TOPMOST);

					return cp;
				}
			}
		}
	
		// not sure what to put in here - just doing a base() call - jba
		protected override ImeMode DefaultImeMode {
			get {
				return base.DefaultImeMode;
			}
		}

		protected override Size DefaultSize {
			get {
				Size single_month = SingleMonthSize;
				// get the width
				int width = calendar_dimensions.Width * single_month.Width;
				if (calendar_dimensions.Width > 1) {
					width += (calendar_dimensions.Width - 1) * calendar_spacing.Width;
				}

				// get the height
				int height = calendar_dimensions.Height * single_month.Height;
				if (this.ShowToday) {
					height += date_cell_size.Height + 2;		// add the height of the "Today: " ...
				}
				if (calendar_dimensions.Height > 1) {
					height += (calendar_dimensions.Height - 1) * calendar_spacing.Height;
				}

				// add the 1 pixel boundary
				if (width > 0) {
					width += 2;
				}
				if (height > 0) {
					height +=2;
				}

				return new Size (width, height);
			}
		}

		#endregion	// Protected Instance Properties

		#region Public Instance Methods

		// add a date to the anually bolded date arraylist
		public void AddAnnuallyBoldedDate (DateTime date) {
			added_annually_bolded_dates.Add (date.Date);
		}

		// add a date to the normal bolded date arraylist
		public void AddBoldedDate (DateTime date) {
			added_bolded_dates.Add (date.Date);
		}

		// add a date to the anually monthly date arraylist
		public void AddMonthlyBoldedDate (DateTime date) {
			added_monthly_bolded_dates.Add (date.Date);
		}

		// if visible = true, return only the dates of full months, else return all dates visible
		public SelectionRange GetDisplayRange (bool visible) {
			DateTime start;
			DateTime end;
			start = new DateTime (current_month.Year, current_month.Month, 1);
			end = start.AddMonths (calendar_dimensions.Width * calendar_dimensions.Height);
			end = end.AddDays(-1);

			// process all visible dates if needed (including the grayed out dates
			if (!visible) {
				start = GetFirstDateInMonthGrid (start);
				end = GetLastDateInMonthGrid (end);
			}

			return new SelectionRange (start, end);
		}

		// HitTest overload that recieve's x and y co-ordinates as separate ints
		public HitTestInfo HitTest (int x, int y) {
			return HitTest (new Point (x, y));
		}

		// returns a HitTestInfo for MonthCalendar element's under the specified point
		public HitTestInfo HitTest (Point point) {
			return HitTest (point, out last_clicked_calendar_index, out last_clicked_calendar_rect);
		}

		// clears all the annually bolded dates
		public void RemoveAllAnnuallyBoldedDates () {
			annually_bolded_dates = null;
			added_annually_bolded_dates.Clear ();
			removed_annually_bolded_dates.Clear ();
		}

		// clears all the normal bolded dates
		public void RemoveAllBoldedDates () {
			bolded_dates = null;
			added_bolded_dates.Clear ();
			removed_bolded_dates.Clear ();
		}

		// clears all the monthly bolded dates
		public void RemoveAllMonthlyBoldedDates () {
			monthly_bolded_dates = null;
			added_monthly_bolded_dates.Clear ();
			removed_monthly_bolded_dates.Clear ();
		}

		// clears the specified annually bolded date (only compares day and month)
		// only removes the first instance of the match
		public void RemoveAnnuallyBoldedDate (DateTime date) {
			if (!removed_annually_bolded_dates.Contains (date.Date)) {
				removed_annually_bolded_dates.Add (date.Date);
			}
		}

		// clears all the normal bolded date
		// only removes the first instance of the match
		public void RemoveBoldedDate (DateTime date) {
			if (!removed_bolded_dates.Contains (date.Date)) {
				removed_bolded_dates.Add (date.Date);
			}
		}

		// clears the specified monthly bolded date (only compares day and month)
		// only removes the first instance of the match
		public void RemoveMonthlyBoldedDate (DateTime date) {
			if (!removed_monthly_bolded_dates.Contains (date.Date)) {
				removed_monthly_bolded_dates.Add (date.Date);
			}
		}

		// sets the calendar_dimensions. If product is > 12, the larger dimension is reduced to make product < 12
		public void SetCalendarDimensions(int x, int y) {
			this.CalendarDimensions = new Size(x, y);
		}

		// sets the currently selected date as date
		public void SetDate (DateTime date) {
			this.SetSelectionRange (date.Date, date.Date);
		}

		// utility method set the SelectionRange property using individual dates
		public void SetSelectionRange (DateTime date1, DateTime date2) {
			this.SelectionRange = new SelectionRange(date1, date2);
		}

		public override string ToString () {
			return this.GetType().Name + ", " + this.SelectionRange.ToString ();
		}
				
		// usually called after an AddBoldedDate method is called
		// formats monthly and daily bolded dates according to the current calendar year
		public void UpdateBoldedDates () {
			UpdateDateArray (ref bolded_dates, added_bolded_dates, removed_bolded_dates);
			UpdateDateArray (ref monthly_bolded_dates, added_monthly_bolded_dates, removed_monthly_bolded_dates);
			UpdateDateArray (ref annually_bolded_dates, added_annually_bolded_dates, removed_annually_bolded_dates);
		}

		#endregion	// Public Instance Methods

		#region	Protected Instance Methods

		// not sure why this needs to be overriden
		protected override void CreateHandle () {
			base.CreateHandle ();
		}

		private void CreateYearUpDown ()
		{
			year_updown = new NumericUpDown ();
			year_updown.Font = this.Font;
			year_updown.Minimum = MinDate.Year;
			year_updown.Maximum = MaxDate.Year;
			year_updown.ReadOnly = true;
			year_updown.Visible = false;
			this.Controls.AddImplicit (year_updown);
			year_updown.ValueChanged += new EventHandler(UpDownYearChangedHandler);
		}

		// not sure why this needs to be overriden
		protected override void Dispose (bool disposing) {
			base.Dispose (disposing);
		}

		// not sure why this needs to be overriden
		protected override bool IsInputKey (Keys keyData) {
			return base.IsInputKey (keyData);
		}

		// not sure why this needs to be overriden
		protected override void OnBackColorChanged (EventArgs e) {
			base.OnBackColorChanged (e);
			this.Invalidate ();
		}

		// raises the date changed event
		protected virtual void OnDateChanged (DateRangeEventArgs drevent) {
			if (this.DateChanged != null) {
				this.DateChanged (this, drevent);
			}
		}

		// raises the DateSelected event
		protected virtual void OnDateSelected (DateRangeEventArgs drevent) {
			if (this.DateSelected != null) {
				this.DateSelected (this, drevent);
			}
		}

		protected override void OnFontChanged (EventArgs e) {
			bold_font = new Font (Font, Font.Style | FontStyle.Bold);
			base.OnFontChanged (e);
		}

		protected override void OnForeColorChanged (EventArgs e) {
			base.OnForeColorChanged (e);
		}

		protected override void OnHandleCreated (EventArgs e) {
			base.OnHandleCreated (e);
			CreateYearUpDown ();
		}

		// i think this is overriden to not allow the control to be changed to an arbitrary size
		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified) {
			if ((specified & BoundsSpecified.Height) == BoundsSpecified.Height ||
				(specified & BoundsSpecified.Width) == BoundsSpecified.Width ||
				(specified & BoundsSpecified.Size) == BoundsSpecified.Size) {
				// only allow sizes = default size to be set
				Size min_size = DefaultSize;
				Size max_size = new Size (
					DefaultSize.Width + SingleMonthSize.Width + calendar_spacing.Width,
					DefaultSize.Height + SingleMonthSize.Height + calendar_spacing.Height);
				int x_mid_point = (max_size.Width + min_size.Width)/2;
				int y_mid_point = (max_size.Height + min_size.Height)/2;
				if (width < x_mid_point) {
					width = min_size.Width;
				} else {
					width = max_size.Width;
				}
				if (height < y_mid_point) {
					height = min_size.Height;
				} else {
					height = max_size.Height;
				}
				base.SetBoundsCore (x, y, width, height, specified);
			} else {
				base.SetBoundsCore (x, y, width, height, specified);
			}
		}

		protected override void WndProc (ref Message m) {
			base.WndProc (ref m);
		}

		#endregion	// Protected Instance Methods

		#region public events

		// fired when the date is changed (either explicitely or implicitely)
		// when navigating the month selector
		public event DateRangeEventHandler DateChanged;

		// fired when the user explicitely clicks on date to select it
		public event DateRangeEventHandler DateSelected;

		[Browsable(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public event EventHandler BackgroundImageChanged;

		// this event is overridden to supress it from being fired
		[Browsable(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public event EventHandler Click;

		// this event is overridden to supress it from being fired
		[Browsable(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public event EventHandler DoubleClick;

		[Browsable(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public event EventHandler ImeModeChanged;

		[Browsable(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event PaintEventHandler Paint;

		[Browsable(false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public event EventHandler TextChanged;
		#endregion	// public events

		#region internal properties

		internal DateTime CurrentMonth {
			set {
				// only interested in if the month (not actual date) has changed
				if (value.Month != current_month.Month ||
					value.Year != current_month.Year) {
					this.SelectionRange = new SelectionRange(
						this.SelectionStart.Add(value.Subtract(current_month)),
						this.SelectionEnd.Add(value.Subtract(current_month)));
					current_month = value;
					UpdateBoldedDates();
					this.Invalidate();
				}
			}
			get {
				return current_month;
			}
		}

		#endregion	// internal properties

		#region internal/private methods
		internal HitTestInfo HitTest (
			Point point,
			out int calendar_index,
			out Rectangle calendar_rect) {
			// start by initialising the ref parameters
			calendar_index = -1;
			calendar_rect = Rectangle.Empty;

			// before doing all the hard work, see if the today's date wasn't clicked
			Rectangle today_rect = new Rectangle (
				ClientRectangle.X, 
				ClientRectangle.Bottom - date_cell_size.Height,
				7 * date_cell_size.Width,
				date_cell_size.Height);
			if (today_rect.Contains (point) && this.ShowToday) {
				return new HitTestInfo(HitArea.TodayLink, point, DateTime.Now);
			}

			Size month_size = SingleMonthSize;
			// define calendar rect's that this thing can land in
			Rectangle[] calendars = new Rectangle [CalendarDimensions.Width * CalendarDimensions.Height];
			for (int i=0; i < CalendarDimensions.Width * CalendarDimensions.Height; i ++) {
				if (i == 0) {
					calendars[i] = new Rectangle (
						new Point (ClientRectangle.X + 1, ClientRectangle.Y + 1),
						month_size);
				} else {
					// calendar on the next row
					if (i % CalendarDimensions.Width == 0) {
						calendars[i] = new Rectangle (
							new Point (calendars[i-CalendarDimensions.Width].X, calendars[i-CalendarDimensions.Width].Bottom + calendar_spacing.Height),
							month_size);
					} else {
						// calendar on the next column
						calendars[i] = new Rectangle (
							new Point (calendars[i-1].Right + calendar_spacing.Width, calendars[i-1].Y),
							month_size);
					}
				}
			}
			
			// through each trying to find a match
			for (int i = 0; i < calendars.Length ; i++) {
				if (calendars[i].Contains (point)) {					
					// check the title section
					Rectangle title_rect = new Rectangle (
						calendars[i].Location,
						title_size);
					if (title_rect.Contains (point) ) {
						// make sure it's not a previous button
						if (i == 0) {
							Rectangle button_rect = new Rectangle(
								new Point (calendars[i].X + button_x_offset, (title_size.Height - button_size.Height)/2),
								button_size);
							if (button_rect.Contains (point)) {
								return new HitTestInfo(HitArea.PrevMonthButton, point, DateTime.Now);
							}
						}
						// make sure it's not the next button
						if (i % CalendarDimensions.Height == 0 && i % CalendarDimensions.Width == calendar_dimensions.Width - 1) {
							Rectangle button_rect = new Rectangle(
								new Point (calendars[i].Right - button_x_offset - button_size.Width, (title_size.Height - button_size.Height)/2),
								button_size);
							if (button_rect.Contains (point)) {
								return new HitTestInfo(HitArea.NextMonthButton, point, DateTime.Now);
							}
						}

						// indicate which calendar and month it was
						calendar_index = i;
						calendar_rect = calendars[i];

						// make sure it's not the month or the year of the calendar
						if (GetMonthNameRectangle (title_rect, i).Contains (point)) {
							return new HitTestInfo(HitArea.TitleMonth, point, DateTime.Now);
						}
						if (GetYearNameRectangle (title_rect, i).Contains (point)) {
							return new HitTestInfo(HitArea.TitleYear, point, DateTime.Now);
						}

						// return the hit test in the title background
						return new HitTestInfo(HitArea.TitleBackground, point, DateTime.Now);
					}

					Point date_grid_location = new Point (calendars[i].X, title_rect.Bottom);

					// see if it's in the Week numbers
					if (ShowWeekNumbers) {
						Rectangle weeks_rect = new Rectangle (
							date_grid_location,
							new Size (date_cell_size.Width,Math.Max (calendars[i].Height - title_rect.Height, 0)));
						if (weeks_rect.Contains (point)) {
							return new HitTestInfo(HitArea.WeekNumbers, point, DateTime.Now);
						}

						// move the location of the grid over
						date_grid_location.X += date_cell_size.Width;
					}

					// see if it's in the week names
					Rectangle day_rect = new Rectangle (
						date_grid_location,
						new Size (Math.Max (calendars[i].Right - date_grid_location.X, 0), date_cell_size.Height));
					if (day_rect.Contains (point)) {
						return new HitTestInfo(HitArea.DayOfWeek, point, DateTime.Now);
					}
						
					// finally see if it was a date that was clicked
					Rectangle date_grid = new Rectangle (
						new Point (day_rect.X, day_rect.Bottom),
						new Size (day_rect.Width, Math.Max(calendars[i].Bottom - day_rect.Bottom, 0)));
					if (date_grid.Contains (point)) {
						clicked_rect = date_grid;
						// okay so it's inside the grid, get the offset
						Point offset = new Point (point.X - date_grid.X, point.Y - date_grid.Y);
						int row = offset.Y / date_cell_size.Height;
						int col = offset.X / date_cell_size.Width;
						// establish our first day of the month
						DateTime calendar_month = this.CurrentMonth.AddMonths(i);
						DateTime first_day = GetFirstDateInMonthGrid (calendar_month);
						DateTime time = first_day.AddDays ((row * 7) + col);
						// establish which date was clicked
						if (time.Year != calendar_month.Year || time.Month != calendar_month.Month) {
							if (time < calendar_month && i == 0) {
								return new HitTestInfo(HitArea.PrevMonthDate, point, time);
							} else if (time > calendar_month && i == CalendarDimensions.Width*CalendarDimensions.Height - 1) {
								return new HitTestInfo(HitArea.NextMonthDate, point, time);
							}							
							return new HitTestInfo(HitArea.Nowhere, point, time);
						}
						return new HitTestInfo(HitArea.Date, point, time);
					}
				}				
			}

			return new HitTestInfo ();
		}

		// returns the date of the first cell of the specified month grid
		internal DateTime GetFirstDateInMonthGrid (DateTime month) {
			// convert the first_day_of_week into a DayOfWeekEnum
			DayOfWeek first_day = GetDayOfWeek (first_day_of_week);
			// find the first day of the month
			DateTime first_date_of_month = new DateTime (month.Year, month.Month, 1);
			DayOfWeek first_day_of_month = first_date_of_month.DayOfWeek;			
			// adjust for the starting day of the week
			int offset = first_day_of_month - first_day;
			if (offset < 0) {
				offset += 7;
			}
			return first_date_of_month.AddDays (-1*offset);
		}

		// returns the date of the last cell of the specified month grid
		internal DateTime GetLastDateInMonthGrid (DateTime month) 
		{
			DateTime start = GetFirstDateInMonthGrid(month);
			return start.AddDays ((7 * 6)-1);
		}
		
		internal bool IsBoldedDate (DateTime date) {
			// check bolded dates
			if (bolded_dates != null && bolded_dates.Length > 0) {
				foreach (DateTime bolded_date in bolded_dates) {
					if (bolded_date.Date == date.Date) {
						return true;
					}
				}
			}
			// check monthly dates
			if (monthly_bolded_dates != null && monthly_bolded_dates.Length > 0) {
				foreach (DateTime bolded_date in monthly_bolded_dates) {
					if (bolded_date.Day == date.Day) {
						return true;
					}
				}
			}
			// check yearly dates
			if (annually_bolded_dates != null && annually_bolded_dates.Length > 0) {
				foreach (DateTime bolded_date in annually_bolded_dates) {
					if (bolded_date.Month == date.Month && bolded_date.Day == date.Day) {
						return true;
					}
				}
			}
			
			return false;  // no match
		}
		
		// updates the specified bolded dates array with ones to add and ones to remove
		private void UpdateDateArray (ref DateTime [] dates, ArrayList to_add, ArrayList to_remove) {
			ArrayList list = new ArrayList ();
			
			// update normal bolded dates
			if (dates != null) {
				foreach (DateTime date in dates) {
					list.Add (date.Date);
				}
			}
			
			// add new ones
			foreach (DateTime date in to_add) {
				if (!list.Contains (date.Date)) {
					list.Add (date.Date);
				}
			}
			to_add.Clear ();
			
			// remove ones to remove
			foreach (DateTime date in to_remove) {
				if (list.Contains (date.Date)) {
					list.Remove (date.Date);
				}
			}
			to_remove.Clear ();
			// set up the array now 
			if (list.Count > 0) {
				dates = (DateTime []) list.ToArray (typeof (DateTime));
				Array.Sort (dates);
				list.Clear ();
			} else {
				dates = null;
			}
		}

		// initialise the context menu
		private void SetUpContextMenu () {
			menu = new ContextMenu ();
			for (int i=0; i < 12; i++) {
				MenuItem menu_item = new MenuItem ( new DateTime (2000, i+1, 1).ToString ("MMMM"));
				menu_item.Click += new EventHandler (MenuItemClickHandler);
				menu.MenuItems.Add (menu_item);
			}
		}

		// initialises text value and show's year up down in correct position
		private void PrepareYearUpDown (Point p) {
			Rectangle old_location = year_updown.Bounds;

			// set position
			Rectangle title_rect = new Rectangle(
				last_clicked_calendar_rect.Location,
				title_size);

			year_updown.Bounds = GetYearNameRectangle(
				title_rect, 
				last_clicked_calendar_index);
			year_updown.Top -= 4;
			year_updown.Width += (int) (this.Font.Size * 4);
			// set year - only do this if this isn't being called because of a year up down click
			if(year_updown.Bounds != old_location) {
				year_updown.Value = current_month.AddMonths(last_clicked_calendar_index).Year;			
			}

			if(!year_updown.Visible) {
				year_updown.Visible = true;
			}
		}

		// returns the first date of the month
		private DateTime GetFirstDateInMonth (DateTime date) {
			return new DateTime (date.Year, date.Month, 1);
		}

		// returns the last date of the month
		private DateTime GetLastDateInMonth (DateTime date) {
			return new DateTime (date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
		}

		// called in response to users seletion with shift key
		private void AddTimeToSelection (int delta, bool isDays)
		{
			DateTime cursor_point;
			DateTime end_point;
			// okay we add the period to the date that is not the same as the 
			// start date when shift was first clicked.
			if (SelectionStart != first_select_start_date) {
				cursor_point = SelectionStart;
			} else {
				cursor_point = SelectionEnd;
			}
			// add the days
			if (isDays) {
				end_point = cursor_point.AddDays (delta);
			} else {
				// delta must be months
				end_point = cursor_point.AddMonths (delta);
			}
			// set the new selection range
			SelectionRange range = new SelectionRange (first_select_start_date, end_point);
			if (range.Start.AddDays (MaxSelectionCount-1) < range.End) {
				// okay the date is beyond what is allowed, lets set the maximum we can
				if (range.Start != first_select_start_date) {
					range.Start = range.End.AddDays ((MaxSelectionCount-1)*-1);
				} else {
					range.End = range.Start.AddDays (MaxSelectionCount-1);
				}
			}
			this.SelectionRange = range;
		}

		// attempts to add the date to the selection without throwing exception
		private void SelectDate (DateTime date) {
			// try and add the new date to the selction range
			if (is_shift_pressed || (click_state [0])) {
				SelectionRange range = new SelectionRange (first_select_start_date, date);
				if (range.Start.AddDays (MaxSelectionCount-1) < range.End) {
					// okay the date is beyond what is allowed, lets set the maximum we can
					if (range.Start != first_select_start_date) {
						range.Start = range.End.AddDays ((MaxSelectionCount-1)*-1);
					} else {
						range.End = range.Start.AddDays (MaxSelectionCount-1);
					}
				}
				SelectionRange = range;
			} else {
				SelectionRange = new SelectionRange (date, date);
				first_select_start_date = date;
			}
		}

		// gets the week of the year
		internal int GetWeekOfYear (DateTime date) {
			// convert the first_day_of_week into a DayOfWeekEnum
			DayOfWeek first_day = GetDayOfWeek (first_day_of_week);
			// find the first day of the year
			DayOfWeek first_day_of_year = new DateTime (date.Year, 1, 1).DayOfWeek;
			// adjust for the starting day of the week
			int offset = first_day_of_year - first_day;
			int week = ((date.DayOfYear + offset) / 7) + 1;
			return week;
		}

		// convert a Day enum into a DayOfWeek enum
		internal DayOfWeek GetDayOfWeek (Day day) {
			if (day == Day.Default) {
				return DayOfWeek.Sunday;
			} else {
				return (DayOfWeek) DayOfWeek.Parse (typeof (DayOfWeek), day.ToString ());
			}
		}

		// returns the rectangle for themonth name
		internal Rectangle GetMonthNameRectangle (Rectangle title_rect, int calendar_index) {
			Graphics g = this.DeviceContext;
			DateTime this_month = this.current_month.AddMonths (calendar_index);
			Size title_text_size = g.MeasureString (this_month.ToString ("MMMM yyyy"), this.Font).ToSize ();
			Size month_size = g.MeasureString (this_month.ToString ("MMMM"), this.Font).ToSize ();
			// return only the month name part of that
			return new Rectangle (
				new Point (
					title_rect.X + ((title_rect.Width - title_text_size.Width)/2),
					title_rect.Y + ((title_rect.Height - title_text_size.Height)/2)),
				month_size);
		}

		// returns the rectangle for the year in the title
		internal Rectangle GetYearNameRectangle (Rectangle title_rect, int calendar_index) {			
			Graphics g = this.DeviceContext;
			DateTime this_month = this.current_month.AddMonths (calendar_index);
			Size title_text_size = g.MeasureString (this_month.ToString ("MMMM yyyy"), this.Font).ToSize ();
			Size year_size = g.MeasureString (this_month.ToString ("yyyy"), this.Font).ToSize ();
			// find out how much space the title took
			Rectangle text_rect =  new Rectangle (
				new Point (
					title_rect.X + ((title_rect.Width - title_text_size.Width)/2),
					title_rect.Y + ((title_rect.Height - title_text_size.Height)/2)),
				title_text_size);
			// return only the rect of the year
			return new Rectangle (
				new Point (
					text_rect.Right - year_size.Width,
					text_rect.Y),
				year_size);
		}

		// determine if date is allowed to be drawn in month
		internal bool IsValidWeekToDraw (DateTime month, DateTime date, int row, int col) {
			DateTime tocheck = month.AddMonths (-1);
			if ((month.Year == date.Year && month.Month == date.Month) ||
				(tocheck.Year == date.Year && tocheck.Month == date.Month)) {
				return true;
			}

			// check the railing dates (days in the month after the last month in grid)
			if (row == CalendarDimensions.Height - 1 && col == CalendarDimensions.Width - 1) {
				tocheck = month.AddMonths (1);
				return (tocheck.Year == date.Year && tocheck.Month == date.Month) ;
			}

			return false;			
		}

		// set one item clicked and all others off
		private void SetItemClick(HitTestInfo hti) 
		{
			switch(hti.HitArea) {
				case HitArea.NextMonthButton:
					this.is_previous_clicked = false;
					this.is_next_clicked = true;
					this.is_date_clicked = false;
					break;
				case HitArea.PrevMonthButton:
					this.is_previous_clicked = true;
					this.is_next_clicked = false;
					this.is_date_clicked = false;
					break;
				case HitArea.PrevMonthDate:
				case HitArea.NextMonthDate:
				case HitArea.Date:
					this.clicked_date = hti.Time;
					this.is_previous_clicked = false;
					this.is_next_clicked = false;
					this.is_date_clicked = true;
					break;
				default :
					this.is_previous_clicked = false;
					this.is_next_clicked = false;
					this.is_date_clicked = false;
					break;
			}
		}

		// called when the year is changed
		private void UpDownYearChangedHandler (object sender, EventArgs e) {
			int initial_year_value = this.CurrentMonth.AddMonths(last_clicked_calendar_index).Year;
			int diff = (int) year_updown.Value - initial_year_value;
			this.CurrentMonth = this.CurrentMonth.AddYears(diff);
		}

		// called when context menu is clicked
		private void MenuItemClickHandler (object sender, EventArgs e) {
			MenuItem item = sender as MenuItem;
			if (item != null && month_title_click_location != Point.Empty) {
				// establish which month we want to move to
				if (item.Parent == null) {
					return;
				}
				int new_month = item.Parent.MenuItems.IndexOf (item) + 1;
				if (new_month == 0) {
					return;
				}
				// okay let's establish which calendar was hit
				Size month_size = this.SingleMonthSize;
				for (int i=0; i < CalendarDimensions.Height; i++) {
					for (int j=0; j < CalendarDimensions.Width; j++) {
						int month_index = (i * CalendarDimensions.Width) + j;
						Rectangle month_rect = new Rectangle ( new Point (0, 0), month_size);
						if (j == 0) {
							month_rect.X = this.ClientRectangle.X + 1;
						} else {
							month_rect.X = this.ClientRectangle.X + 1 + ((j)*(month_size.Width+calendar_spacing.Width));
						}
						if (i == 0) {
							month_rect.Y = this.ClientRectangle.Y + 1;
						} else {
							month_rect.Y = this.ClientRectangle.Y + 1 + ((i)*(month_size.Height+calendar_spacing.Height));
						}
						// see if the point is inside
						if (month_rect.Contains (month_title_click_location)) {
							DateTime clicked_month = CurrentMonth.AddMonths (month_index);
							// get the month that we want to move to
							int month_offset = new_month - clicked_month.Month;
							
							// move forward however more months we need to
							this.CurrentMonth = this.CurrentMonth.AddMonths (month_offset);
							break;
						}
					}
				}

				// clear the point
				month_title_click_location = Point.Empty;
			}
		}
		
		// raised on the timer, for mouse hold clicks
		private void TimerHandler (object sender, EventArgs e) {
			// now find out which area was click
			if (this.Capture) {
				HitTestInfo hti = this.HitTest (this.PointToClient (MousePosition));
				// see if it was clicked on the prev or next mouse 
				if (click_state [1] || click_state [2]) {
					// invalidate the area where the mouse was last held
					DoMouseUp ();
					// register the click
					if (hti.HitArea == HitArea.PrevMonthButton ||
						hti.HitArea == HitArea.NextMonthButton) {
						DoButtonMouseDown (hti);
						click_state [1] = (hti.HitArea == HitArea.PrevMonthButton);
						click_state [2] = !click_state [1];
					}
					if (timer.Interval != 300) {
						timer.Interval = 300;
					}
				}
			} else  {
				timer.Enabled = false;
			}
		}
		
		// selects one of the buttons
		private void DoButtonMouseDown (HitTestInfo hti) {
			// show the click then move on
			SetItemClick(hti);
			if (hti.HitArea == HitArea.PrevMonthButton) {
				// invalidate the prev monthbutton
				this.Invalidate(
					new Rectangle (
						this.ClientRectangle.X + 1 + button_x_offset,
						this.ClientRectangle.Y + 1 + (title_size.Height - button_size.Height)/2,
						button_size.Width,
						button_size.Height));
				this.CurrentMonth = this.CurrentMonth.AddMonths (ScrollChange*-1);
			} else {
				// invalidate the next monthbutton
				this.Invalidate(
					new Rectangle (
						this.ClientRectangle.Right - 1 - button_x_offset - button_size.Width,
						this.ClientRectangle.Y + 1 + (title_size.Height - button_size.Height)/2,
						button_size.Width,
						button_size.Height));					
				this.CurrentMonth = this.CurrentMonth.AddMonths (ScrollChange);
			}
		}
		
		// selects the clicked date
		private void DoDateMouseDown (HitTestInfo hti) {
			SetItemClick(hti);
		}
		
		// event run on the mouse up event
		private void DoMouseUp () {

			HitTestInfo hti = this.HitTest (this.PointToClient (MousePosition));
			switch (hti.HitArea) {
			case HitArea.PrevMonthDate:
			case HitArea.NextMonthDate:
			case HitArea.Date:
				this.SelectDate (clicked_date);
				this.OnDateSelected (new DateRangeEventArgs (SelectionStart, SelectionEnd));
				break;
			}

			// invalidate the next monthbutton
			if (this.is_next_clicked) {
				this.Invalidate(
					new Rectangle (
						this.ClientRectangle.Right - 1 - button_x_offset - button_size.Width,
						this.ClientRectangle.Y + 1 + (title_size.Height - button_size.Height)/2,
						button_size.Width,
						button_size.Height));
			}					
			// invalidate the prev monthbutton
			if (this.is_previous_clicked) {
				this.Invalidate(
					new Rectangle (
						this.ClientRectangle.X + 1 + button_x_offset,
						this.ClientRectangle.Y + 1 + (title_size.Height - button_size.Height)/2,
						button_size.Width,
						button_size.Height));
			}
			if (this.is_date_clicked) {
				// invalidate the area under the cursor, to remove focus rect
				this.InvalidateDateRange (new SelectionRange (clicked_date, clicked_date));				
			}
			this.is_previous_clicked = false;
			this.is_next_clicked = false;
			this.is_date_clicked = false;
		}
		
//		// need when in windowed mode
//		private void LostFocusHandler (object sender, EventArgs e) 
//		{
//			if (this.owner != null) {
//				if (this.Visible) {
//					this.owner.HideMonthCalendar ();
//				}
//			}
//		}
		
		// occurs when mouse moves around control, used for selection
		private void MouseMoveHandler (object sender, MouseEventArgs e) {
			HitTestInfo hti = this.HitTest (e.X, e.Y);
			// clear the last clicked item 
			if (click_state [0]) {
				// register the click
				if (hti.HitArea == HitArea.PrevMonthDate ||
					hti.HitArea == HitArea.NextMonthDate ||
					hti.HitArea == HitArea.Date)
				{
					Rectangle prev_rect = clicked_rect;
					DateTime prev_clicked = clicked_date;
					DoDateMouseDown (hti);
					if (owner == null) {
						click_state [0] = true;
					} else {
						click_state [0] = false;
						click_state [1] = false;
						click_state [2] = false;
					}

					if (prev_clicked != clicked_date) {
						Rectangle invalid = Rectangle.Union (prev_rect, clicked_rect);
						Invalidate (invalid);
					}
				}
				
			}
		}
		
		// to check if the mouse has come down on this control
		private void MouseDownHandler (object sender, MouseEventArgs e)
		{
			// clear the click_state variables
			click_state [0] = false;
			click_state [1] = false;
			click_state [2] = false;

			// disable the timer if it was enabled 
			if (timer.Enabled) {
				timer.Stop ();
				timer.Enabled = false;
			}
			
			Point point = new Point (e.X, e.Y);
			// figure out if we are in drop down mode and a click happened outside us
			if (this.owner != null) {
				if (!this.ClientRectangle.Contains (point)) {
					this.owner.HideMonthCalendar ();
					return;					
				}
			}

			//establish where was hit
			HitTestInfo hti = this.HitTest(point);
			// hide the year numeric up down if it was clicked
			if (year_updown != null && year_updown.Visible && hti.HitArea != HitArea.TitleYear)
			{
				year_updown.Visible = false;
			}
			switch (hti.HitArea) {
				case HitArea.PrevMonthButton:
				case HitArea.NextMonthButton:
					DoButtonMouseDown (hti);
					click_state [1] = (hti.HitArea == HitArea.PrevMonthDate);
					click_state [2] = !click_state [1];					
					timer.Interval = 750;
					timer.Start ();
					break;
				case HitArea.Date:
				case HitArea.PrevMonthDate:
				case HitArea.NextMonthDate:
					DoDateMouseDown (hti);
					// leave clicked state blank if drop down window
					if (owner == null) {
						click_state [0] = true;
					} else {
						click_state [0] = false;
						click_state [1] = false;
						click_state [2] = false;
					}
					break;
				case HitArea.TitleMonth:
					month_title_click_location = hti.Point;
					menu.Show (this, hti.Point);		
					break;
				case HitArea.TitleYear:
					// place the numeric up down
					PrepareYearUpDown(hti.Point);
					break;
				case HitArea.TodayLink:
					this.SetSelectionRange (DateTime.Now.Date, DateTime.Now.Date);
					this.OnDateSelected (new DateRangeEventArgs (SelectionStart, SelectionEnd));
					break;
				default:
					this.is_previous_clicked = false;
					this.is_next_clicked = false;
					this.is_date_clicked = false;				
					break;
			}
		}

		// raised by any key down events
		private void KeyDownHandler (object sender, KeyEventArgs e) {
			// send keys to the year_updown control, let it handle it
			if(year_updown.Visible) {
				switch (e.KeyCode) {
					case Keys.Enter:
						year_updown.Visible = false;
						break;
					case Keys.Up:
						year_updown.Value = year_updown.Value + 1;
						break;
					case Keys.Down:
						year_updown.Value = year_updown.Value - 1;
						break;
				}
			} else {
				if (!is_shift_pressed && e.Shift) {
					first_select_start_date = SelectionStart;
					is_shift_pressed = e.Shift;
				}
				switch (e.KeyCode) {
					case Keys.Home:
						// set the date to the start of the month
						if (is_shift_pressed) {
							DateTime date = GetFirstDateInMonth (first_select_start_date);
							if (date < first_select_start_date.AddDays ((MaxSelectionCount-1)*-1)) {
								date = first_select_start_date.AddDays ((MaxSelectionCount-1)*-1);
							}
							this.SetSelectionRange (date, first_select_start_date);
						} else {
							DateTime date = GetFirstDateInMonth (this.SelectionStart);
							this.SetSelectionRange (date, date);
						}
						this.OnDateSelected (new DateRangeEventArgs (SelectionStart, SelectionEnd));
						break;
					case Keys.End:
						// set the date to the last of the month
						if (is_shift_pressed) {
							DateTime date = GetLastDateInMonth (first_select_start_date);
							if (date > first_select_start_date.AddDays (MaxSelectionCount-1)) {
								date = first_select_start_date.AddDays (MaxSelectionCount-1);
							}
							this.SetSelectionRange (date, first_select_start_date);
						} else {
							DateTime date = GetLastDateInMonth (this.SelectionStart);
							this.SetSelectionRange (date, date);
						}
						this.OnDateSelected (new DateRangeEventArgs (SelectionStart, SelectionEnd));
						break;
					case Keys.PageUp:
						// set the date to the last of the month
						if (is_shift_pressed) {
							this.AddTimeToSelection (-1, false);
						} else {
							DateTime date = this.SelectionStart.AddMonths (-1);
							this.SetSelectionRange (date, date);
						}
						this.OnDateSelected (new DateRangeEventArgs (SelectionStart, SelectionEnd));
						break;
					case Keys.PageDown:
						// set the date to the last of the month
						if (is_shift_pressed) {
							this.AddTimeToSelection (1, false);
						} else {
							DateTime date = this.SelectionStart.AddMonths (1);
							this.SetSelectionRange (date, date);
						}
						this.OnDateSelected (new DateRangeEventArgs (SelectionStart, SelectionEnd));
						break;
					case Keys.Up:
						// set the back 1 week
						if (is_shift_pressed) {
							this.AddTimeToSelection (-7, true);						
						} else {
							DateTime date = this.SelectionStart.AddDays (-7);
							this.SetSelectionRange (date, date);
						}
						this.OnDateSelected (new DateRangeEventArgs (SelectionStart, SelectionEnd));
						break;
					case Keys.Down:
						// set the date forward 1 week
						if (is_shift_pressed) {
							this.AddTimeToSelection (7, true);
						} else {
							DateTime date = this.SelectionStart.AddDays (7);
							this.SetSelectionRange (date, date);
						}
						this.OnDateSelected (new DateRangeEventArgs (SelectionStart, SelectionEnd));					
						break;
					case Keys.Left:
						// move one left
						if (is_shift_pressed) {
							this.AddTimeToSelection (-1, true);
						} else {
							DateTime date = this.SelectionStart.AddDays (-1);
							this.SetSelectionRange (date, date);
						}
						this.OnDateSelected (new DateRangeEventArgs (SelectionStart, SelectionEnd));
						break;
					case Keys.Right:
						// move one left
						if (is_shift_pressed) {
							this.AddTimeToSelection (1, true);
						} else {
							DateTime date = this.SelectionStart.AddDays (1);
							this.SetSelectionRange (date, date);
						}
						this.OnDateSelected (new DateRangeEventArgs (SelectionStart, SelectionEnd));
						break;
					default:
						break;
				}
				e.Handled = true;
			}
		}

		// to check if the mouse has come up on this control
		private void MouseUpHandler (object sender, MouseEventArgs e)
		{
			if (timer.Enabled) {
				timer.Stop ();
			}
			// clear the click state array
			click_state [0] = false;
			click_state [1] = false;
			click_state [2] = false;
			// do the regulare mouseup stuff
			this.DoMouseUp ();
		}

		// raised by any key up events
		private void KeyUpHandler (object sender, KeyEventArgs e) {
			is_shift_pressed = e.Shift ;
			e.Handled = true;
		}

		// paint this control now
		private void PaintHandler (object sender, PaintEventArgs pe) {
			if (Width <= 0 || Height <=  0 || Visible == false)
    				return;

			Draw (pe.ClipRectangle, pe.Graphics);

			// fire the new paint handler
			if (this.Paint != null) 
			{
				this.Paint (sender, pe);
			}
		}

		// returns the region of the control that needs to be redrawn 
		private void InvalidateDateRange (SelectionRange range) {
			SelectionRange bounds = this.GetDisplayRange (false);

			if (range.End < bounds.Start || range.Start > bounds.End) {
				// don't invalidate anything, as the modified date range
				// is outside the visible bounds of this control
				return;
			}
			// adjust the start and end to be inside the visible range
			if (range.Start < bounds.Start) {
				range = new SelectionRange (bounds.Start, range.End);
			}
			if (range.End > bounds.End) {
				range = new SelectionRange (range.Start, bounds.End);
			}
			// now invalidate the date rectangles as series of rows
			DateTime last_month = this.current_month.AddMonths ((CalendarDimensions.Width * CalendarDimensions.Height)).AddDays (-1);
			DateTime current = range.Start;
			while (current <= range.End) {
				DateTime month_end = new DateTime (current.Year, current.Month, 1).AddMonths (1).AddDays (-1);;
				Rectangle start_rect;
				Rectangle end_rect;
				// see if entire selection is in this current month
				if (range.End <= month_end && current < last_month)	{
					// the end is the last date
					if (current < this.current_month) {
						start_rect = GetDateRowRect (current_month, current_month);
					} else {
						start_rect = GetDateRowRect (current, current);
					}
					end_rect = GetDateRowRect (current, range.End);
				} else if (current < last_month) {
					// otherwise it simply means we have a selection spaning
					// multiple months simply set rectangle inside the current month
					start_rect = GetDateRowRect (current, current);
					end_rect = GetDateRowRect (month_end, month_end);
				} else {
					// it's outside the visible range
					start_rect = GetDateRowRect (last_month, last_month.AddDays (1));
					end_rect = GetDateRowRect (last_month, range.End);
				}
				// push to the next month
				current = month_end.AddDays (1);
				// invalidate from the start row to the end row for this month				
				this.Invalidate (
					new Rectangle (
						start_rect.X,
						start_rect.Y,
						start_rect.Width,
						Math.Max (end_rect.Bottom - start_rect.Y, 0)));
				}
		} 
		
		// gets the rect of the row where the specified date appears on the specified month
		private Rectangle GetDateRowRect (DateTime month, DateTime date) {
			// first get the general rect of the supplied month
			Size month_size = SingleMonthSize;
			Rectangle month_rect = Rectangle.Empty;
			for (int i=0; i < CalendarDimensions.Width*CalendarDimensions.Height; i++) {
				DateTime this_month = this.current_month.AddMonths (i);
				if (month.Year == this_month.Year && month.Month == this_month.Month) {
					month_rect = new Rectangle (
						this.ClientRectangle.X + 1 + (month_size.Width * (i%CalendarDimensions.Width)) + (this.calendar_spacing.Width * (i%CalendarDimensions.Width)),
						this.ClientRectangle.Y + 1 + (month_size.Height * (i/CalendarDimensions.Width)) + (this.calendar_spacing.Height * (i/CalendarDimensions.Width)),
						month_size.Width,
						month_size.Height);
						break;		
				}
			}
			// now find out where in the month the supplied date is
			if (month_rect == Rectangle.Empty) {
				return Rectangle.Empty;
			}
			// find out which row this date is in
			int row = -1;
			DateTime first_date = GetFirstDateInMonthGrid (month);
			DateTime end_date = first_date.AddDays (7); 
			for (int i=0; i < 6; i++) {
				if (date >= first_date && date < end_date) {
					row = i;
					break;
				}
				first_date = end_date;
				end_date = end_date.AddDays (7);
			}
			// ensure it's a valid row
			if (row < 0) {
				return Rectangle.Empty;
			}
			int x_offset = (this.ShowWeekNumbers) ? date_cell_size.Width : 0;
			int y_offset = title_size.Height + (date_cell_size.Height * (row + 1));
			return new Rectangle (
				month_rect.X + x_offset,
				month_rect.Y + y_offset,
				date_cell_size.Width * 7,
				date_cell_size.Height);
		}

		internal void Draw (Rectangle clip_rect, Graphics dc)
		{
			ThemeEngine.Current.DrawMonthCalendar (dc, clip_rect, this);
		}

		#endregion 	//internal methods

		#region internal drawing methods


		#endregion	// internal drawing methods

		#region inner classes and enumerations

		// enumeration about what type of area on the calendar was hit 
		public enum HitArea {
			Nowhere,
			TitleBackground,
			TitleMonth,
			TitleYear,
			NextMonthButton,
			PrevMonthButton,
			CalendarBackground,
			Date,
			NextMonthDate,
			PrevMonthDate,
			DayOfWeek,
			WeekNumbers,
			TodayLink
		}
		
		// info regarding to a hit test on this calendar
		public sealed class HitTestInfo {

			private HitArea hit_area;
			private Point point;
			private DateTime time;

			// default constructor
			internal HitTestInfo () {
				hit_area = HitArea.Nowhere;
				point = new Point (0, 0);
				time = DateTime.Now;
			}

			// overload receives all properties
			internal HitTestInfo (HitArea hit_area, Point point, DateTime time) {
				this.hit_area = hit_area;
				this.point = point;
				this.time = time;
			}

			// the type of area that was hit
			public HitArea HitArea {
				get {
					return hit_area;
				}
			}

			// the point that is being test
			public Point Point {
				get {
					return point;
				}
			}
			
			// the date under the hit test point, only valid if HitArea is Date
			public DateTime Time {
				get {
					return time;
				}
			}
		}

		#endregion 	// inner classes
	}
}
