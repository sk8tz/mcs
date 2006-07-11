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
	public class DataGridTextBoxColumn : DataGridColumnStyle
	{
		#region	Local Variables
		private string format;
		private IFormatProvider format_provider = null;
		private StringFormat string_format =  new StringFormat ();
		private DataGridTextBox textbox;
		private static readonly int offset_x = 0;
		private static readonly int offset_y = 0;
		#endregion	// Local Variables

		#region Constructors
		public DataGridTextBoxColumn () : this (null, String.Empty, false)
		{
		}

		public DataGridTextBoxColumn (PropertyDescriptor prop) : this (prop, String.Empty, false)
		{
		}
		
		public DataGridTextBoxColumn (PropertyDescriptor prop,  bool isDefault) : this (prop, String.Empty, isDefault)
		{
		}

		public DataGridTextBoxColumn (PropertyDescriptor prop,  string format) : this (prop, format, false)
		{
		}
		
		public DataGridTextBoxColumn (PropertyDescriptor prop,  string format, bool isDefault) : base (prop)
		{
			Format = format;
			is_default = isDefault;

			textbox = new DataGridTextBox ();
			textbox.Multiline = true;
			textbox.WordWrap = false;
			textbox.BorderStyle = BorderStyle.None;
			textbox.Visible = false;
		}

		#endregion

		#region Public Instance Properties
		[Editor("System.Windows.Forms.Design.DataGridColumnStyleFormatEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		public string Format {
			get { return format; }
			set {
				if (value != format) {
					format = value;
					Invalidate ();
				}
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public IFormatProvider FormatInfo {
			get { return format_provider; }
			set {
				if (value != format_provider) {
					format_provider = value;
				}
			}
		}

		[DefaultValue(null)]
		public override PropertyDescriptor PropertyDescriptor {
			set { base.PropertyDescriptor = value; }
		}

		public override bool ReadOnly {
			get { return base.ReadOnly; }
			set { base.ReadOnly = value; }
		}
		
		[Browsable(false)]
		public virtual TextBox TextBox {
			get { return textbox; }
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods

		protected internal override void Abort (int rowNum)
		{
			EndEdit ();			
		}
		
		protected internal override bool Commit (CurrencyManager dataSource, int rowNum)
		{
			DataGridTextBox box = (DataGridTextBox)textbox;

			/* Do not write data if not editing. */
			if (box.IsInEditOrNavigateMode)
				return true;

			try {
				object obj = GetColumnValueAtRow (dataSource, rowNum);
				string existing_text = GetFormattedString (obj);

				if (existing_text != textbox.Text) {
					if (textbox.Text == NullText)
						SetColumnValueAtRow (dataSource, rowNum, DBNull.Value);
					else
						SetColumnValueAtRow (dataSource, rowNum, textbox.Text);
				}
			}
			catch {
				return false;
			}
			
			EndEdit ();			
			return true;
		}

		[MonoTODO]
		protected internal override void ConcedeFocus ()
		{
			HideEditBox ();
		}

		protected internal override void Edit (CurrencyManager source, int rowNum,  Rectangle bounds,  bool _ro, string instantText, bool cellIsVisible)
		{
			object obj;
			
			grid.SuspendLayout ();

			textbox.TextAlign = alignment;
			
			if ((ParentReadOnly == true)  || 
				(ParentReadOnly == false && ReadOnly == true) || 
				(ParentReadOnly == false && _ro == true)) {
				textbox.ReadOnly = true;
			} else {
				textbox.ReadOnly = false;
			}			
			
			textbox.Location = new Point (bounds.X + offset_x, bounds.Y + offset_y);
			textbox.Size = new Size (bounds.Width - offset_x, bounds.Height - offset_y);

			obj = GetColumnValueAtRow (source, rowNum);
			textbox.Text = GetFormattedString (obj);

			textbox.Visible = cellIsVisible;
			textbox.Focus ();
			textbox.SelectAll ();
			grid.ResumeLayout (false);
		}

		protected void EndEdit ()
		{
			HideEditBox ();
		}

		protected internal override void EnterNullValue ()
		{
			textbox.Text = NullText;
		}

		protected internal override int GetMinimumHeight ()
		{
			return FontHeight + 3;
		}

		[MonoTODO]
		protected internal override int GetPreferredHeight (Graphics g, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override Size GetPreferredSize (Graphics g, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void HideEditBox ()
		{
			grid.SuspendLayout ();
			textbox.Bounds = Rectangle.Empty;
			grid.ResumeLayout (false);
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
			object obj;
			obj = GetColumnValueAtRow (source, rowNum);

			PaintText (g, bounds, GetFormattedString (obj),  backBrush, foreBrush, alignToRight);
		}

		protected void PaintText (Graphics g, Rectangle bounds, string text, bool alignToRight)
		{
			PaintText (g, bounds, text,  ThemeEngine.Current.ResPool.GetSolidBrush (DataGridTableStyle.BackColor),
				ThemeEngine.Current.ResPool.GetSolidBrush (DataGridTableStyle.ForeColor), alignToRight);
		}

		protected void PaintText (Graphics g, Rectangle textBounds, string text, Brush backBrush, Brush foreBrush, bool alignToRight)
		{
			if (alignToRight == true) {
				string_format.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
			} else {
				string_format.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;
			}
			
			switch (alignment) {
			case HorizontalAlignment.Center:
				string_format.Alignment = StringAlignment.Center;
				break;
			case HorizontalAlignment.Right:
				string_format.Alignment = StringAlignment.Far;
				break;			
			default:
				string_format.Alignment = StringAlignment.Near;
				break;
			}			
					
			g.FillRectangle (backBrush, textBounds);
			PaintGridLine (g, textBounds);
			
			textBounds.Y += offset_y;
			textBounds.Height -= offset_y;
			
			string_format.FormatFlags |= StringFormatFlags.NoWrap;
			g.DrawString (text, DataGridTableStyle.DataGrid.Font, foreBrush, textBounds, string_format);
			
		}
		
		protected internal override void ReleaseHostedControl ()
		{			
			if (textbox != null) {
				grid.SuspendLayout ();
				grid.Controls.Remove (textbox);
				grid.Invalidate (new Rectangle (textbox.Location, textbox.Size));
				textbox.Dispose ();
				textbox = null;
				grid.ResumeLayout (false);
			}
		}

		protected override void SetDataGridInColumn (DataGrid value)
		{
			base.SetDataGridInColumn (value);

			if (value != null) {
				textbox.SetDataGrid (grid);			
				grid.SuspendLayout ();
				grid.Controls.Add (textbox);
				grid.ResumeLayout (false);
			}
		}
		
		protected internal override void UpdateUI (CurrencyManager source, int rowNum, string instantText)
		{

		}

		#endregion	// Public Instance Methods


		#region Private Instance Methods

		private string GetFormattedString (object obj)
		{
			if (obj == DBNull.Value) {
				return NullText;
			}
			
			if (format != null && obj as IFormattable != null) {
				return ((IFormattable)obj).ToString (format, format_provider);
			}

			return obj.ToString ();

		}
		#endregion Private Instance Methods
	}
}
