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

namespace System.Windows.Forms
{
	public class DataGridBoolColumn : DataGridColumnStyle
	{
		#region	Local Variables
		private bool allownull;
		private object falsevalue;
		private object nullvalue;
		private object truevalue;
		#endregion	// Local Variables

		#region Constructors
		public DataGridBoolColumn () : base ()
		{
			CommonConstructor ();
		}

		public DataGridBoolColumn (PropertyDescriptor prop) : base (prop)
		{
			CommonConstructor ();
		}

		public DataGridBoolColumn (PropertyDescriptor prop, bool isDefault)  : base (prop)
		{
			CommonConstructor ();
		}

		private void CommonConstructor ()
		{
			allownull = true;
			falsevalue = false;
			nullvalue = null;
			truevalue = true;
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

					if (AllowNullChanged != null) {
						AllowNullChanged (this, EventArgs.Empty);
					}
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

					if (FalseValueChanged != null) {
						FalseValueChanged (this, EventArgs.Empty);
					}
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

					if (TrueValueChanged != null) {
						TrueValueChanged (this, EventArgs.Empty);
					}
				}
			}
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		[MonoTODO]
		protected internal override void Abort (int rowNum)
		{

		}

		[MonoTODO]
		protected internal override bool Commit (CurrencyManager dataSource, int rowNum)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override void ConcedeFocus ()
		{

		}

		[MonoTODO]
		protected internal override void Edit (CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText,  bool cellIsVisible)
		{

		}

		[MonoTODO]
		protected internal override void EnterNullValue ()
		{

		}
		
		protected internal override object GetColumnValueAtRow (CurrencyManager lm, int row)
		{
			object obj = base.GetColumnValueAtRow (lm, row);
			
			if (obj.Equals (truevalue)) {
				return true;
			}
			else {
				return false;
			}
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
			chkbox_size.Width -= 2;
			chkbox_size.Height -= 2;			
			rect.X = bounds.X + ((bounds.Width - chkbox_size.Width) / 2);
			rect.Y = bounds.Y + ((bounds.Height - chkbox_size.Height) / 2);
			rect.Width = chkbox_size.Width;
			rect.Height = chkbox_size.Height;
			
			bool value = (bool) GetColumnValueAtRow (source, rowNum);
			ThemeEngine.Current.CPDrawCheckBox (g, rect, value == true ? ButtonState.Checked : ButtonState.Normal);
		}

		[MonoTODO]
		protected internal override void SetColumnValueAtRow (CurrencyManager lm, int row, object value)
		{

		}
		#endregion	// Public Instance Methods
		
		#region Private Instance Methods
		internal static bool CanRenderType (Type type)
		{			
			return (type == typeof (Boolean));
		}
		#endregion Private Instance Methods	

		#region Events
		public event EventHandler AllowNullChanged;
		public event EventHandler FalseValueChanged;
		public event EventHandler TrueValueChanged;
		#endregion	// Events
	}
}
