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

using System.Drawing;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace System.Windows.Forms
{
	[DesignTimeVisible(false)]
	[DefaultProperty("Header")]
	[ToolboxItem(false)]
	public abstract class DataGridColumnStyle : Component, IDataGridColumnStyleEditingNotificationService
	{
		[ComVisible(true)]
		protected class DataGridColumnHeaderAccessibleObject : AccessibleObject
		{
			#region Local Variables
			private DataGridColumnStyle owner;			
			#endregion

			#region Constructors
			public DataGridColumnHeaderAccessibleObject (DataGridColumnStyle columnstyle)
			{
				owner = columnstyle;
			}
			#endregion //Constructors

			#region Public Instance Properties
			[MonoTODO]
			public override Rectangle Bounds {
				get {
					throw new NotImplementedException ();
				}
			}

			public override string Name {
				get {
					throw new NotImplementedException ();
				}
			}

			protected DataGridColumnStyle Owner {
				get { return owner; }
			}

			public override AccessibleObject Parent {
				get {
					throw new NotImplementedException ();
				}
			}

			public override AccessibleRole Role {
				get {
					throw new NotImplementedException ();
				}
			}
			#endregion

			#region Public Instance Methods
			[MonoTODO]
			public override AccessibleObject Navigate (AccessibleNavigation navdir)
			{
				throw new NotImplementedException ();
			}
			#endregion Public Instance Methods
		}

		protected class CompModSwitches
		{
			public CompModSwitches ()
			{
			}

			#region Public Instance Methods
			[MonoTODO]
			public static TraceSwitch DGEditColumnEditing {
				get {
					throw new NotImplementedException ();
				}
			}
			#endregion Public Instance Methods
		}		
		
		#region	Local Variables
		internal HorizontalAlignment alignment;
		private int fontheight;
		internal DataGridTableStyle table_style;
		private string header_text;
		private string mapping_name;
		private string null_text;
		private PropertyDescriptor property_descriptor;
		private bool _readonly;
		private int width;
		internal bool is_default;
		internal DataGrid grid;
		private DataGridColumnHeaderAccessibleObject accesible_object;
		private StringFormat string_format_hdr;
		static string def_null_text = "(null)";
		#endregion	// Local Variables

		#region Constructors
		public DataGridColumnStyle ()
		{
			CommmonConstructor ();
			property_descriptor = null;
		}

		public DataGridColumnStyle (PropertyDescriptor prop)
		{
			CommmonConstructor ();
			property_descriptor = prop;
		}

		private void CommmonConstructor ()
		{
			fontheight = -1;
			table_style = null;
			header_text = string.Empty;
			mapping_name  = string.Empty;
			null_text = def_null_text;
			accesible_object = new DataGridColumnHeaderAccessibleObject (this);
			_readonly = false;
			width = -1;
			grid = null;
			is_default = false;
			alignment = HorizontalAlignment.Left;
			string_format_hdr = new StringFormat ();
			string_format_hdr.FormatFlags |= StringFormatFlags.NoWrap;
			string_format_hdr.LineAlignment  = StringAlignment.Center;
		}

		#endregion

		#region Public Instance Properties
		[Localizable(true)]
		[DefaultValue(HorizontalAlignment.Left)]
		public virtual HorizontalAlignment Alignment {
			get {
				return alignment;
			}
			set {				
				if (value != alignment) {
					alignment = value;
					
					if (table_style != null && table_style.DataGrid != null) {
						table_style.DataGrid.Invalidate ();
					}
					
					if (AlignmentChanged != null) {
						AlignmentChanged (this, EventArgs.Empty);
					}					
				}
			}
		}

		[Browsable(false)]
		public virtual DataGridTableStyle DataGridTableStyle {
			get {
				return table_style;
			}			
		}
		
		protected int FontHeight {
			get {
				if (fontheight != -1) {
					return fontheight;
				}

				if (table_style != null) {
					//return table_style.DataGrid.FontHeight
					return -1;
				}

				// TODO: Default Datagrid font height
				return -1;
			}
		}

		[Browsable(false)]
		public AccessibleObject HeaderAccessibleObject {
			get {
				return accesible_object;
			}
		}

		[Localizable(true)]
		public virtual string HeaderText {
			get {
				return header_text;
			}
			set {
				if (value != header_text) {
					header_text = value;
					
					if (table_style != null && table_style.DataGrid != null) {
						table_style.DataGrid.Invalidate ();
					}

					if (HeaderTextChanged != null) {
						HeaderTextChanged (this, EventArgs.Empty);
					}
				}
			}
		}

		[Editor("System.Windows.Forms.Design.DataGridColumnStyleMappingNameEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		[Localizable(true)]
		public string MappingName {
			get {
				return mapping_name;
			}
			set {
				if (value != mapping_name) {
					mapping_name = value;

					if (MappingNameChanged != null) {
						MappingNameChanged (this, EventArgs.Empty);
					}
				}
			}
		}

		[Localizable(true)]
		public virtual string NullText {
			get {
				return null_text;
			}
			set {
				if (value != null_text) {
					null_text = value;
					
					if (table_style != null && table_style.DataGrid != null) {
						table_style.DataGrid.Invalidate ();
					}

					if (NullTextChanged != null) {
						NullTextChanged (this, EventArgs.Empty);
					}
				}
			}
		}

		[Browsable(false)]
		[DefaultValue(null)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public virtual PropertyDescriptor PropertyDescriptor {
			get {
				return property_descriptor;
			}
			set {
				if (value != property_descriptor) {
					property_descriptor = value;					

					if (PropertyDescriptorChanged != null) {
						PropertyDescriptorChanged (this, EventArgs.Empty);
					}
				}
			}
		}

		[DefaultValue(false)]
		public virtual bool ReadOnly  {
			get {
				return _readonly;
			}
			set {
				if (value != _readonly) {
					_readonly = value;
					
					if (table_style != null && table_style.DataGrid != null) {
						table_style.DataGrid.CalcAreasAndInvalidate ();
					}
					
					if (ReadOnlyChanged != null) {
						ReadOnlyChanged (this, EventArgs.Empty);
					}
				}
			}
		}

		[DefaultValue(100)]
		[Localizable(true)]
		public virtual int Width {
			get {
				return width;
			}
			set {
				if (value != width) {
					width = value;
					
					if (table_style != null && table_style.DataGrid != null) {
						table_style.DataGrid.CalcAreasAndInvalidate ();
					}

					if (WidthChanged != null) {
						WidthChanged (this, EventArgs.Empty);
					}
				}
			}
		}

		#endregion	// Public Instance Properties
		
		#region Private Instance Properties
		
		// The logic seems to be that: 
		// - If DataGrid.ReadOnly is true all the tables and columns are readonly ignoring other settings
		// - If DataGridTableStyle.ReadOnly is true all columns are readonly ignoring other settings
		// - If DataGrid.ReadOnly and DataGridTableStyle.ReadOnly are false, the columns settings are mandatory
		//
		internal bool ParentReadOnly {
			get {				
				if (grid != null) {
					if (grid.ReadOnly == true) {
						return true;
					}
					
					if (grid.ListManager != null && grid.ListManager.CanAddRows == false) {
						return true;
					}				
				}
				
				if (table_style != null) {
					if (table_style.ReadOnly == true) {
						return true;
					}
				}
				
				return false;
			}
		}
		
		internal DataGridTableStyle TableStyle {
			set { table_style = value; }
		}
		
		internal bool IsDefault {
			get { return is_default; }
		}
		#endregion Private Instance Properties

		#region Public Instance Methods
		protected internal abstract void Abort (int rowNum);

		[MonoTODO]
		protected void BeginUpdate ()
		{

		}
		
		protected void CheckValidDataSource (CurrencyManager value)
		{
			if (value == null) {
				throw new ArgumentNullException ("CurrencyManager cannot be null");
			}
			
			if (property_descriptor == null) {
				throw new ApplicationException ("The PropertyDescriptor for this column is a null reference");
			}
		}

		[MonoTODO]
		protected internal virtual void ColumnStartedEditing (Control editingControl)
		{

		}

		protected internal abstract bool Commit (CurrencyManager dataSource, int rowNum);


		protected internal virtual void ConcedeFocus ()
		{

		}
		
		protected virtual AccessibleObject CreateHeaderAccessibleObject ()
		{
			return new DataGridColumnHeaderAccessibleObject (this);
		}

		protected internal virtual void Edit (CurrencyManager source, int rowNum,  Rectangle bounds,  bool readOnly)
		{
			Edit (source, rowNum, bounds, readOnly, string.Empty);
		}
		
		protected internal virtual void Edit (CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText)
		{	
			Edit (source, rowNum, bounds, readOnly, instantText, true);
		}

		protected internal abstract void Edit (CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly,   string instantText,  bool cellIsVisible);


		[MonoTODO]
		protected void EndUpdate ()
		{

		}

		protected internal virtual void EnterNullValue () {}
		
		protected internal virtual object GetColumnValueAtRow (CurrencyManager source, int rowNum)
		{			
			CheckValidDataSource (source);
			return property_descriptor.GetValue (source.GetItem (rowNum));
		}

		protected internal abstract int GetMinimumHeight ();

		protected internal abstract int GetPreferredHeight (Graphics g, object value);

		protected internal abstract Size GetPreferredSize (Graphics g,  object value);

		void  IDataGridColumnStyleEditingNotificationService.ColumnStartedEditing (Control editingControl)
		{

		}

		protected virtual void Invalidate ()
		{
			grid.grid_drawing.InvalidateColumn (this);
		}

		protected internal abstract void Paint (Graphics g, Rectangle bounds, CurrencyManager source, int rowNum);
		protected internal abstract void Paint (Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, bool alignToRight);
		
		protected internal virtual void Paint (Graphics g, Rectangle bounds, CurrencyManager source, int rowNum,
   			Brush backBrush,  Brush foreBrush, bool alignToRight) {}

		protected internal virtual void ReleaseHostedControl () {}

		public void ResetHeaderText ()
		{
			HeaderText = string.Empty;
		}

		protected internal virtual void SetColumnValueAtRow (CurrencyManager source, int rowNum,  object value)
		{
			CheckValidDataSource (source);
			property_descriptor.SetValue (source.GetItem (rowNum), value);			
		}

		protected virtual void SetDataGrid (DataGrid value)
		{
			grid = value;
			
			if (property_descriptor != null || value == null || value.ListManager == null) {
				return;
			}
			
			PropertyDescriptorCollection propcol = value.ListManager.GetItemProperties ();
			for (int i = 0; i < propcol.Count ; i++) {
				if (propcol[i].Name == mapping_name) {
					property_descriptor = propcol[i];
					break;
				}
			}			
		}

		protected virtual void SetDataGridInColumn (DataGrid value)
		{
			SetDataGrid (value);
		}
		
		internal void SetDataGridInternal (DataGrid value)
		{
			SetDataGridInColumn (value);
		}

		protected internal virtual void UpdateUI (CurrencyManager source, int rowNum, string instantText)
		{

		}
		#endregion	// Public Instance Methods
		
		#region Private Instance Methods
		virtual internal void OnMouseDown (MouseEventArgs e, int row, int column) {}
		virtual internal void OnKeyDown (KeyEventArgs ke, int row, int column) {}
		
		internal void PaintHeader (Graphics g, Rectangle bounds, int colNum)
		{	
			// Background
			g.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (DataGridTableStyle.CurrentHeaderBackColor), 
				bounds);
			
			// Paint Borders			
			g.DrawLine (ThemeEngine.Current.ResPool.GetPen (ThemeEngine.Current.ColorButtonHilight),			
				bounds.X, bounds.Y, bounds.X + bounds.Width, bounds.Y);
			
			if (colNum == 0) {	
				g.DrawLine (ThemeEngine.Current.ResPool.GetPen (ThemeEngine.Current.ColorButtonHilight),
					bounds.X, bounds.Y, bounds.X, bounds.Y + bounds.Height);
			} else {
				g.DrawLine (ThemeEngine.Current.ResPool.GetPen (ThemeEngine.Current.ColorButtonHilight),
					bounds.X, bounds.Y + 2, bounds.X, bounds.Y + bounds.Height - 2);
			}
			
			g.DrawLine (ThemeEngine.Current.ResPool.GetPen (ThemeEngine.Current.ColorButtonShadow),
				bounds.X + bounds.Width - 1, bounds.Y + 2 , bounds.X + bounds.Width - 1, bounds.Y + bounds.Height - 2);
			
			bounds.X += 2;
			bounds.Width -=	2;
			g.DrawString (HeaderText, DataGridTableStyle.HeaderFont, ThemeEngine.Current.ResPool.GetSolidBrush (DataGridTableStyle.CurrentHeaderForeColor), 
				bounds, string_format_hdr);
		}
				
		internal void PaintNewRow (Graphics g, Rectangle bounds, Brush backBrush, Brush foreBrush)
		{
			g.FillRectangle (backBrush, bounds);
			PaintGridLine (g, bounds);
		}
		
		internal void PaintGridLine (Graphics g, Rectangle bounds)
		{
			if (table_style.CurrentGridLineStyle != DataGridLineStyle.Solid) {
				return;
			}
			
			g.DrawLine (ThemeEngine.Current.ResPool.GetPen (table_style.CurrentGridLineColor),
				bounds.X, bounds.Y + bounds.Height - 1, bounds.X + bounds.Width - 1, bounds.Y + bounds.Height - 1);
			
			g.DrawLine (ThemeEngine.Current.ResPool.GetPen (table_style.CurrentGridLineColor),
				bounds.X + bounds.Width - 1, bounds.Y , bounds.X + bounds.Width - 1, bounds.Y + bounds.Height);
		}
		
		#endregion Private Instance Methods


		#region Events
		public event EventHandler AlignmentChanged;
		public event EventHandler FontChanged;
		public event EventHandler HeaderTextChanged;
		public event EventHandler MappingNameChanged;
		public event EventHandler NullTextChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public event EventHandler PropertyDescriptorChanged;
		public event EventHandler ReadOnlyChanged;
		public event EventHandler WidthChanged;
		#endregion	// Events
	}
}
