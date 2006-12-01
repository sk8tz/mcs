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

// NOT COMPLETE

using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections;

namespace System.Windows.Forms
{
	public class DataGridBoolColumn : DataGridColumnStyle
	{
		[Flags]
		private enum CheckState {
			Checked		= 0x00000001,
			UnChecked	= 0x00000002,
			Null		= 0x00000004,
			Selected	= 0x00000008
		}

		#region	Local Variables
		private bool allownull;
		private object falsevalue;
		private object nullvalue;
		private object truevalue;
		private Hashtable checkboxes_state;
		CheckState oldState;

		#endregion	// Local Variables

		#region Constructors
		public DataGridBoolColumn () : this (null, false)
		{
		}

		public DataGridBoolColumn (PropertyDescriptor prop) : this (prop, false)
		{
		}

		public DataGridBoolColumn (PropertyDescriptor prop, bool isDefault)  : base (prop)
		{
			falsevalue = false;
			nullvalue = null;
			truevalue = true;
			allownull = true;
			checkboxes_state = new Hashtable ();			
			is_default = isDefault;
		}
		#endregion

		#region Public Instance Properties
		[DefaultValue(true)]
		public bool AllowNull {
			get {
				return allownull;
			}
			set {
				if (value != allownull) {
					allownull = value;

					EventHandler eh = (EventHandler)(Events [AllowNullChangedEvent]);
					if (eh != null)
						eh (this, EventArgs.Empty);
				}
			}
		}

		[TypeConverter(typeof(System.ComponentModel.StringConverter))]
		public object FalseValue {
			get {
				return falsevalue;
			}
			set {
				if (value != falsevalue) {
					falsevalue = value;

					EventHandler eh = (EventHandler)(Events [FalseValueChangedEvent]);
					if (eh != null)
						eh (this, EventArgs.Empty);
				}
			}
		}

		[TypeConverter(typeof(System.ComponentModel.StringConverter))]
		public object NullValue {
			get {
				return nullvalue;
			}
			set {
				if (value != nullvalue) {
					nullvalue = value;

					// XXX no NullValueChangedEvent?  lame.
				}
			}
		}

		[TypeConverter(typeof(System.ComponentModel.StringConverter))]
		public object TrueValue {
			get {
				return truevalue;
			}
			set {
				if (value != truevalue) {
					truevalue = value;

					EventHandler eh = (EventHandler)(Events [TrueValueChangedEvent]);
					if (eh != null)
						eh (this, EventArgs.Empty);
				}
			}
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		protected internal override void Abort (int rowNum)
		{
			SetState (rowNum, oldState & ~CheckState.Selected);			
			grid.Invalidate (grid.GetCurrentCellBounds ());
		}

		protected internal override bool Commit (CurrencyManager source, int rowNum)
		{
			CheckState newState = GetState (source, rowNum);
			SetColumnValueAtRow (source, rowNum, FromStateToValue (newState));
			SetState (rowNum, newState & ~CheckState.Selected);
			grid.Invalidate (grid.GetCurrentCellBounds ());
			return true;
		}

		[MonoTODO]
		protected internal override void ConcedeFocus ()
		{
		}

		protected internal override void Edit (CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText,  bool cellIsVisible)
		{
			oldState = GetState (source, rowNum);
			SetState (rowNum, oldState | CheckState.Selected);
			grid.Invalidate (grid.GetCurrentCellBounds ());
		}

		[MonoTODO]
		protected internal override void EnterNullValue ()
		{

		}

		private bool ValueEquals (object value, object obj)
		{
			return value == null ? obj == null : value.Equals (obj);
		}

		protected internal override object GetColumnValueAtRow (CurrencyManager lm, int row)
		{
			object obj = base.GetColumnValueAtRow (lm, row);

			if (ValueEquals (nullvalue, obj)) {
				return Convert.DBNull;
			}

			if (ValueEquals (truevalue, obj)) {
				return true;
			}

			return false;
		}

		protected internal override int GetMinimumHeight ()
		{
			return ThemeEngine.Current.DataGridMinimumColumnCheckBoxHeight;
		}

		protected internal override int GetPreferredHeight (Graphics g, object value)
		{
			return ThemeEngine.Current.DataGridMinimumColumnCheckBoxHeight;
		}

		protected internal override Size GetPreferredSize (Graphics g, object value)
		{
			return new Size (ThemeEngine.Current.DataGridMinimumColumnCheckBoxWidth, ThemeEngine.Current.DataGridMinimumColumnCheckBoxHeight);
		}

		protected internal override void Paint (Graphics g, Rectangle bounds, CurrencyManager source, int rowNum)
		{
			Paint (g, bounds, source, rowNum, false);
		}

		protected internal override void Paint (Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, bool alignToRight)
		{
			Paint (g, bounds, source, rowNum, ThemeEngine.Current.ResPool.GetSolidBrush (DataGridTableStyle.BackColor),
				ThemeEngine.Current.ResPool.GetSolidBrush (DataGridTableStyle.ForeColor), alignToRight);
		}

		protected internal override void Paint (Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
		{
			Size chkbox_size = GetPreferredSize (g, null);
			Rectangle rect = new Rectangle ();			
			ButtonState state;
			CheckState check_state = GetState (source, rowNum);

			chkbox_size.Width -= 2;
			chkbox_size.Height -= 2;

			rect.X = bounds.X + ((bounds.Width - chkbox_size.Width) / 2);
			rect.Y = bounds.Y + ((bounds.Height - chkbox_size.Height) / 2);
			rect.Width = chkbox_size.Width;
			rect.Height = chkbox_size.Height;			
			
			// If the cell is selected
			if ((check_state & CheckState.Selected) == CheckState.Selected) { 
				backBrush = ThemeEngine.Current.ResPool.GetSolidBrush (grid.SelectionBackColor);
				check_state &= ~CheckState.Selected;
			}
						
			g.FillRectangle (backBrush, bounds);			
			
			switch (check_state) {
			case CheckState.Checked:
				state = ButtonState.Checked;
				break;
			case CheckState.Null:
				state = ButtonState.Checked | ButtonState.Inactive;
				break;
			case CheckState.UnChecked:
			default:
				state = ButtonState.Normal;
				break;
			}

			ThemeEngine.Current.CPDrawCheckBox (g, rect, state);
			PaintGridLine (g, bounds);
		}

		protected internal override void SetColumnValueAtRow (CurrencyManager lm, int row, object obj)
		{
			object value = null;

			if (ValueEquals (nullvalue, obj))
				value = Convert.DBNull;
			else if (ValueEquals (truevalue, obj))
				value = true;
			else if (ValueEquals (falsevalue, obj))
				value = false;
			/* else error? */

			base.SetColumnValueAtRow (lm, row, value);
		}
		#endregion	// Public Instance Methods

		#region Private Instance Methods
		private object FromStateToValue (CheckState state)
		{
			if ((state & CheckState.Checked) == CheckState.Checked)
				return truevalue;
			else if ((state & CheckState.Null) == CheckState.Null)
				return nullvalue;
			else
				return falsevalue;
		}

		private CheckState FromValueToState (object obj)
		{
			if (ValueEquals (truevalue, obj))
				return CheckState.Checked;
			else if (ValueEquals (nullvalue, obj))
				return CheckState.Null;
			else
				return CheckState.UnChecked;
		}

		private CheckState GetState (CurrencyManager source, int row)
		{
			CheckState state;

			if (checkboxes_state[row] == null) {
				object value = GetColumnValueAtRow (source, row);
				state =	FromValueToState (value);
				checkboxes_state.Add (row, state);
			} else {
				state = (CheckState) checkboxes_state[row];
			}

			return state;
		}

		private CheckState GetNextState (CheckState state)
		{
			CheckState new_state;
			bool selected = ((state & CheckState.Selected) == CheckState.Selected);

			switch (state & ~CheckState.Selected) {
			case CheckState.Checked:
				new_state = CheckState.Null;
				break;
			case CheckState.Null:
				new_state = CheckState.UnChecked;
				break;
			case CheckState.UnChecked:
			default:
				new_state = CheckState.Checked;
				break;
			}
			
			if (selected) {
				new_state = new_state | CheckState.Selected;
			}

			return new_state;
		}

		internal override void OnKeyDown (KeyEventArgs ke, int row, int column)
		{
			switch (ke.KeyCode) {
			case Keys.Space:
				NextState (row, column);
				break;
			}
		}

		internal override void OnMouseDown (MouseEventArgs e, int row, int column)
		{
			NextState (row, column);
		}

		private void NextState (int row, int column)
		{
			grid.ColumnStartedEditing (new Rectangle());

			SetState (row, GetNextState (GetState (null, row)));

			grid.Invalidate (grid.GetCellBounds (row, column));
		}

		private void SetState (int row, CheckState state)
		{
			checkboxes_state[row] = state;
		}

		#endregion Private Instance Methods

		#region Events
		static object AllowNullChangedEvent = new object ();
		static object FalseValueChangedEvent = new object ();
		static object TrueValueChangedEvent = new object ();

		public event EventHandler AllowNullChanged {
			add { Events.AddHandler (AllowNullChangedEvent, value); }
			remove { Events.RemoveHandler (AllowNullChangedEvent, value); }
		}

		public event EventHandler FalseValueChanged {
			add { Events.AddHandler (FalseValueChangedEvent, value); }
			remove { Events.RemoveHandler (FalseValueChangedEvent, value); }
		}

		public event EventHandler TrueValueChanged {
			add { Events.AddHandler (TrueValueChangedEvent, value); }
			remove { Events.RemoveHandler (TrueValueChangedEvent, value); }
		}
		#endregion	// Events
	}
}
