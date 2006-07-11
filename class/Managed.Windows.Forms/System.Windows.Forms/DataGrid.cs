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
// Copyright (c) 2005,2006 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Jordi Mas i Hernandez	<jordi@ximian.com>
//	Chris Toshok		<toshok@ximian.com>
//
//

// NOT COMPLETE


using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections;
using System.Text;

namespace System.Windows.Forms
{
	internal struct DataGridRow {
		public bool IsSelected;
		public bool IsExpanded;
		public int VerticalOffset {
			get { return verticalOffset; }
			set {
				int delta = value - verticalOffset;
				if (relation_link != null)
					relation_link.Location = new Point (relation_link.Location.X,
									    relation_link.Location.Y + delta);
				verticalOffset = value;
			}
		}
		public int Height {
			get { return height; }
			set {
				int delta = value - height;
				if (relation_link != null)
					relation_link.Location = new Point (relation_link.Location.X,
									    relation_link.Location.Y + delta);

				height = value;
			}
		}
		public int RelationHeight;
		public LinkLabel relation_link;

		int verticalOffset;
		int height;
	}

	internal class DataGridParentRow
	{
		public string datamember;
		public DataRowView view;

		public DataGridParentRow (string datamember, DataRowView view)
		{
			this.datamember = datamember;
			this.view = view;
		}
	}

	[DefaultEvent("Navigate")]
	[DefaultProperty("DataSource")]
	[Designer("System.Windows.Forms.Design.DataGridDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class DataGrid : Control, ISupportInitialize, IDataGridEditingService
	{
		[Flags]
		[Serializable]
		public enum HitTestType
		{
			None		= 0,
			Cell		= 1,
			ColumnHeader	= 2,
			RowHeader	= 4,
			ColumnResize	= 8,
			RowResize	= 16,
			Caption		= 32,
			ParentRows	= 64
		}

		public sealed class HitTestInfo
		{
			public static readonly HitTestInfo Nowhere = null;

			int row;
			int column;
			DataGrid.HitTestType type;

			#region Private Constructors
			internal HitTestInfo () : this (-1, -1, HitTestType.None)
			{
			}

			internal HitTestInfo (int row, int column, DataGrid.HitTestType type)
			{
				this.row = row;
				this.column = column;
				this.type = type;
			}
			#endregion


			#region Public Instance Properties
			public int Column {
				get { return column; }
			}

			public int Row {
				get { return row; }
			}
			public DataGrid.HitTestType Type {
				get { return type; }
			}
			#endregion //Public Instance Properties

			public override bool Equals (object o)
			{
				if (!(o is HitTestInfo))
					return false;

				HitTestInfo obj = (HitTestInfo) o;
				return (obj.Column == column && obj.Row == row && obj.Type ==type);
			}

			public override int GetHashCode ()
			{
				return row ^ column;
			}

			public override string ToString ()
			{
				return "{ " + type + "," + row + "," + column + "}";
			}

		}

		#region	Local Variables
		/* cached theme defaults */
		static readonly Color	def_background_color = ThemeEngine.Current.DataGridBackgroundColor;
		static readonly Color	def_caption_backcolor = ThemeEngine.Current.DataGridCaptionBackColor;
		static readonly Color	def_caption_forecolor = ThemeEngine.Current.DataGridCaptionForeColor;
		static readonly Color	def_parentrowsback_color = ThemeEngine.Current.DataGridParentRowsBackColor;
		static readonly Color	def_parentrowsfore_color = ThemeEngine.Current.DataGridParentRowsForeColor;

		/* colors */
		Color background_color;
		Color caption_backcolor;
		Color caption_forecolor;
		Color parentrowsback_color;
		Color parentrowsfore_color;

		/* flags to determine which areas of the datagrid are shown */
		bool caption_visible;
		bool parentrows_visible;

		GridTableStylesCollection styles_collection;
		DataGridParentRowsLabelStyle parentrowslabel_style;
		DataGridTableStyle default_style;
		DataGridTableStyle grid_style;
		DataGridTableStyle current_style;

		/* selection */
		DataGridCell current_cell;
		Hashtable selected_rows;
		int selection_start; // used for range selection

		/* layout/rendering */
		bool allow_navigation;
		int first_visiblerow;
		int first_visiblecolumn;
		int visiblerow_count;
		int visiblecolumn_count;
		Font caption_font;
		string caption_text;
		bool flatmode;
		HScrollBar horiz_scrollbar;
		VScrollBar vert_scrollbar;
		int horiz_pixeloffset;
		Button navigate_back_button;
		Button hide_parent_rows_button;

		/* databinding */
		object datasource;
		string datamember;
		CurrencyManager list_manager;
		bool _readonly;
		DataGridRow[] rows;
		bool initializing;

		/* column resize fields */
		bool column_resize_active;
		int resize_column_x;
		int resize_column_width_delta;
		int resize_column;
		
		/* row resize fields */
		bool row_resize_active;
		int resize_row_y;
		int resize_row_height_delta;
		int resize_row;

		/* used to make sure we don't endlessly recurse calling set_CurrentCell and OnListManagerPositionChanged */
		bool from_positionchanged_handler;

		/* editing state */
		bool is_editing;		// Current cell is edit mode
		bool is_changing;

		internal Stack parentRows;

		#endregion // Local Variables

		#region Public Constructors
		public DataGrid ()
		{
			allow_navigation = true;
			background_color = def_background_color;
			border_style = BorderStyle.Fixed3D;
			caption_backcolor = def_caption_backcolor;
			caption_forecolor = def_caption_forecolor;
			caption_text = string.Empty;
			caption_visible = true;
			datamember = string.Empty;
			parentrowsback_color = def_parentrowsback_color;
			parentrowsfore_color = def_parentrowsfore_color;
			parentrows_visible = true;
			current_cell = new DataGridCell ();
			parentrowslabel_style = DataGridParentRowsLabelStyle.Both;
			selected_rows = new Hashtable ();
			selection_start = -1;
			rows = new DataGridRow [0];

			default_style = new DataGridTableStyle (true);
			grid_style = new DataGridTableStyle ();

			styles_collection = new GridTableStylesCollection (this);
			styles_collection.CollectionChanged += new CollectionChangeEventHandler (OnTableStylesCollectionChanged);

			CurrentTableStyle = grid_style;

			horiz_scrollbar = new ImplicitHScrollBar ();
			horiz_scrollbar.Scroll += new ScrollEventHandler (GridHScrolled);
			vert_scrollbar = new ImplicitVScrollBar ();
			vert_scrollbar.Scroll += new ScrollEventHandler (GridVScrolled);

			SetStyle (ControlStyles.UserMouse, true);

			parentRows = new Stack ();
		}

		#endregion	// Public Constructor

		#region Public Instance Properties

		[DefaultValue(true)]
		public bool AllowNavigation {
			get { return allow_navigation; }
			set {
				if (allow_navigation != value) {
					allow_navigation = value;
					OnAllowNavigationChanged (EventArgs.Empty);
				}
			}
		}

		[DefaultValue(true)]
		public bool AllowSorting {
			get { return grid_style.AllowSorting; }
			set { grid_style.AllowSorting = value; }
		}

		public Color AlternatingBackColor  {
			get { return grid_style.AlternatingBackColor; }
			set { grid_style.AlternatingBackColor = value; }
		}

		public override Color BackColor {
			get { return grid_style.BackColor; }
			set { grid_style.BackColor = value; }
		}

		public Color BackgroundColor {
			get { return background_color; }
			set {
				 if (background_color != value) {
					background_color = value;
					OnBackgroundColorChanged (EventArgs.Empty);
					Invalidate ();
				}
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Image BackgroundImage {
			get { return base.BackgroundImage; }
			set {
				if (base.BackgroundImage == value)
					return;

    				base.BackgroundImage = value;
				Invalidate ();
			}
		}

		[DispId(-504)]
		[DefaultValue(BorderStyle.Fixed3D)]
		public BorderStyle BorderStyle {
			get { return InternalBorderStyle; }
			set { 
				InternalBorderStyle = value; 
				CalcAreasAndInvalidate ();
				OnBorderStyleChanged (EventArgs.Empty);
			}
		}

		public Color CaptionBackColor {
			get { return caption_backcolor; }
			set {
				if (caption_backcolor != value) {
					caption_backcolor = value;
					InvalidateCaption ();
				}
			}
		}

		[Localizable(true)]
		[AmbientValue(null)]
		public Font CaptionFont {
			get {
				if (caption_font == null)
					return Font;

				return caption_font;
			}
			set {
				if (caption_font != null && caption_font.Equals (value))
					return;

				caption_font = value;
				CalcAreasAndInvalidate ();
			}
		}

		public Color CaptionForeColor {
			get { return caption_forecolor; }
			set {
				if (caption_forecolor != value) {
					caption_forecolor = value;
					InvalidateCaption ();
				}
			}
		}

		[Localizable(true)]
		[DefaultValue("")]
		public string CaptionText {
			get { return caption_text; }
			set {
				if (caption_text != value) {
					caption_text = value;
					InvalidateCaption ();
				}
			}
		}

		[DefaultValue(true)]
		public bool CaptionVisible {
			get { return caption_visible; }
			set {
				if (caption_visible != value) {
					caption_visible = value;
					CalcAreasAndInvalidate ();
					OnCaptionVisibleChanged (EventArgs.Empty);
				}
			}
		}

		[DefaultValue(true)]
		public bool ColumnHeadersVisible {
			get { return grid_style.ColumnHeadersVisible; }
			set { grid_style.ColumnHeadersVisible = value; }
		}

		bool setting_current_cell;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DataGridCell CurrentCell {
			get { return current_cell; }
			set {
				if (current_cell.Equals (value))
					return;

				if (setting_current_cell)
					return;
				setting_current_cell = true;

				int old_row = current_cell.RowNumber;

				bool was_editing = is_editing;
				bool need_add = value.RowNumber >= RowsCount;

				if (need_add)
					value.RowNumber = RowsCount;
#if true
				if (was_editing) {
					EndEdit (CurrentTableStyle.GridColumnStyles[current_cell.ColumnNumber],
						 current_cell.RowNumber,
						 IsAdding && !is_changing);

					if (value.RowNumber != old_row) {
						ListManager.EndCurrentEdit ();
					}
				}
#else
				if (value.RowNumber != old_row && IsAdding && !is_changing) {
					ListManager.CancelCurrentEdit ();
					UpdateVisibleRowCount ();
				}
#endif

				if (was_editing)
					CurrentTableStyle.GridColumnStyles[current_cell.ColumnNumber].ConcedeFocus ();

				//Console.WriteLine ("set_CurrentCell, {0}x{1}, RowsCount = {2}, from {3}", value.RowNumber, value.ColumnNumber, RowsCount, Environment.StackTrace);
				if (need_add) {
					//Console.WriteLine ("+ calling AddNew, RowsCount = {0}, rows.Length = {1}", RowsCount, rows.Length);
					ListManager.AddNew ();
				}

				if (value.ColumnNumber >= CurrentTableStyle.GridColumnStyles.Count) {
					value.ColumnNumber = CurrentTableStyle.GridColumnStyles.Count == 0 ? 0: CurrentTableStyle.GridColumnStyles.Count - 1;
				}
					
				EnsureCellVisibility (value);
				current_cell = value;
			
				if (current_cell.RowNumber != old_row) {
					InvalidateRowHeader (old_row);
					InvalidateRowHeader (current_cell.RowNumber);
				}

				list_manager.Position = current_cell.RowNumber;

				OnCurrentCellChanged (EventArgs.Empty);

				if (was_editing)
					EditCurrentCell ();

				setting_current_cell = false;
			}
		}

		int CurrentRow {
			get { return current_cell.RowNumber; }
			set { CurrentCell = new DataGridCell (value, current_cell.ColumnNumber); }
		}

		int CurrentColumn {
			get { return current_cell.ColumnNumber; }
			set { CurrentCell = new DataGridCell (current_cell.RowNumber, value); }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int CurrentRowIndex {
			get {
				if (ListManager == null)
					return -1;
				
				return CurrentRow;
			}
			set { CurrentRow = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Cursor Cursor {
			get { return base.Cursor; }
			set { base.Cursor = value; }
		}

		[DefaultValue(null)]
		[Editor ("System.Windows.Forms.Design.DataMemberListEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public string DataMember {
			get { return datamember; }
			set {
				if (SetDataMember (value)) {					
					SetDataSource (datasource);
					SetNewDataSource ();
				}
			}
		}

		[DefaultValue(null)]
		[RefreshProperties(RefreshProperties.Repaint)]
		[TypeConverter("System.Windows.Forms.Design.DataSourceConverter, " + Consts.AssemblySystem_Design)]
		public object DataSource {
			get { return datasource; }
			set {
				SetDataMember ("");
				SetDataSource (value);
				SetNewDataSource ();					
			}
		}

		protected override Size DefaultSize {
			get { return new Size (130, 80); }
		}

		[Browsable(false)]
		public int FirstVisibleColumn {
			get { return first_visiblecolumn; }
		}

		[DefaultValue(false)]
		public bool FlatMode {
			get { return flatmode; }
			set {
				if (flatmode != value) {
					flatmode = value;
					OnFlatModeChanged (EventArgs.Empty);
					Refresh ();
				}
			}
		}

		public override Color ForeColor {
			get { return grid_style.ForeColor; }
			set { grid_style.ForeColor = value; }
		}

		public Color GridLineColor {
			get { return grid_style.GridLineColor; }
			set {
				if (value == Color.Empty)
					throw new ArgumentException ("Color.Empty value is invalid.");

				grid_style.GridLineColor = value;
			}
		}

		[DefaultValue(DataGridLineStyle.Solid)]
		public DataGridLineStyle GridLineStyle {
			get { return grid_style.GridLineStyle; }
			set { grid_style.GridLineStyle = value; }
		}

		public Color HeaderBackColor {
			get { return grid_style.HeaderBackColor; }
			set {
				if (value == Color.Empty)
					throw new ArgumentException ("Color.Empty value is invalid.");

				grid_style.HeaderBackColor = value;
			}
		}

		public Font HeaderFont {
			get { return grid_style.HeaderFont; }
			set { grid_style.HeaderFont = value; }
		}

		public Color HeaderForeColor {
			get { return grid_style.HeaderForeColor; }
			set { grid_style.HeaderForeColor = value; }
		}

		protected ScrollBar HorizScrollBar {
			get { return horiz_scrollbar; }
		}
		internal ScrollBar HScrollBar {
			get { return horiz_scrollbar; }
		}

		internal int HorizPixelOffset {
			get { return horiz_pixeloffset; }
		}

		internal bool IsAdding {
			get { return ShowEditRow && list_manager.Position == rows.Length; }
		}

		internal bool IsChanging {
			get { return is_changing; }
		}

		public object this [DataGridCell cell] {
			get { return this [cell.RowNumber, cell.ColumnNumber]; }
			set { this [cell.RowNumber, cell.ColumnNumber] = value; }
		}

		public object this [int rowIndex, int columnIndex] {
			get { return CurrentTableStyle.GridColumnStyles[columnIndex].GetColumnValueAtRow (ListManager,
													  rowIndex); }
			set { CurrentTableStyle.GridColumnStyles[columnIndex].SetColumnValueAtRow (ListManager,
												   rowIndex, value); }
		}

		public Color LinkColor {
			get { return grid_style.LinkColor; }
			set { grid_style.LinkColor = value; }
		}

		[ComVisible(false)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Color LinkHoverColor {
			get { return grid_style.LinkHoverColor; }
			set { grid_style.LinkHoverColor = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected internal CurrencyManager ListManager {
			get {
				if (BindingContext == null || DataSource  == null)
					return null;

				if (list_manager != null)
					return list_manager;

				list_manager = (CurrencyManager) BindingContext [datasource, DataMember];

				if (list_manager != null)
					ConnectListManagerEvents ();

				rows = new DataGridRow [RowsCount + 1];
				for (int i = 0; i < rows.Length; i ++) {
					rows[i].Height = RowHeight;
					if (i > 0)
						rows[i].VerticalOffset = rows[i-1].VerticalOffset + rows[i-1].Height;
				}
				return list_manager;
			}
			set { throw new NotSupportedException ("Operation is not supported."); }
		}

		public Color ParentRowsBackColor {
			get { return parentrowsback_color; }
			set {
				if (parentrowsback_color != value) {
					parentrowsback_color = value;
					if (parentrows_visible) {
						Refresh ();
					}
				}
			}
		}

		public Color ParentRowsForeColor {
			get { return parentrowsfore_color; }
			set {
				if (parentrowsfore_color != value) {
					parentrowsfore_color = value;
					if (parentrows_visible) {
						Refresh ();
					}
				}
			}
		}

		[DefaultValue(DataGridParentRowsLabelStyle.Both)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public DataGridParentRowsLabelStyle ParentRowsLabelStyle {
			get { return parentrowslabel_style; }
			set {
				if (parentrowslabel_style != value) {
					parentrowslabel_style = value;
					if (parentrows_visible) {
						Refresh ();
					}

					OnParentRowsLabelStyleChanged (EventArgs.Empty);
				}
			}
		}

		[DefaultValue(true)]
		public bool ParentRowsVisible {
			get { return parentrows_visible; }
			set {
				if (parentrows_visible != value) {
					parentrows_visible = value;
					CalcAreasAndInvalidate ();
					OnParentRowsVisibleChanged (EventArgs.Empty);
				}
			}
		}

		// Settting this property seems to have no effect.
		[DefaultValue(75)]
		[TypeConverter(typeof(DataGridPreferredColumnWidthTypeConverter))]
		public int PreferredColumnWidth {
			get { return grid_style.PreferredColumnWidth; }
			set { grid_style.PreferredColumnWidth = value; }
		}

		public int PreferredRowHeight {
			get { return grid_style.PreferredRowHeight; }
			set { grid_style.PreferredRowHeight = value; }
		}

		[DefaultValue(false)]
		public bool ReadOnly {
			get { return _readonly; }
			set {
				if (_readonly != value) {
					_readonly = value;
					OnReadOnlyChanged (EventArgs.Empty);
					CalcAreasAndInvalidate ();
				}
			}
		}

		[DefaultValue(true)]
		public bool RowHeadersVisible {
			get { return grid_style.RowHeadersVisible; }
			set { grid_style.RowHeadersVisible = value; }
		}

		[DefaultValue(35)]
		public int RowHeaderWidth {
			get { return grid_style.RowHeaderWidth; }
			set { grid_style.RowHeaderWidth = value; }
		}

		internal DataGridRow[] Rows {
			get { return rows; }
		}

		public Color SelectionBackColor {
			get { return grid_style.SelectionBackColor; }
			set { grid_style.SelectionBackColor = value; }
		}

		public Color SelectionForeColor  {
			get { return grid_style.SelectionForeColor; }
			set { grid_style.SelectionForeColor = value; }
		}

		public override ISite Site {
			get { return base.Site; }
			set { base.Site = value; }
		}

		[Localizable(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public GridTableStylesCollection TableStyles {
			get { return styles_collection; }
		}

		[Bindable(false)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override string Text {
			get { return base.Text; }
			set { base.Text = value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		protected ScrollBar VertScrollBar {
			get { return vert_scrollbar; }
		}
		internal ScrollBar VScrollBar {
			get { return vert_scrollbar; }
		}

		[Browsable(false)]
		public int VisibleColumnCount {
			get { return visiblecolumn_count; }
		}

		// Calculated at DataGridDrawing.CalcRowHeaders
		[Browsable(false)]
		public int VisibleRowCount {
			get { return visiblerow_count; }
		}

		#endregion	// Public Instance Properties

		#region Private Instance Properties
		internal DataGridTableStyle CurrentTableStyle {
			get { return current_style; }
			set {
				current_style = value;
				current_style.DataGrid = this;
				CalcAreasAndInvalidate ();
			}
		}

		internal int FirstVisibleRow {
			get { return first_visiblerow; }
		}
		
		internal int RowsCount {
			get { return ListManager != null ? ListManager.Count : 0; }
		}

		internal int RowHeight {
			get {
				if (CurrentTableStyle.CurrentPreferredRowHeight > Font.Height + 3 + 1 /* line */)
					return CurrentTableStyle.CurrentPreferredRowHeight;
				else
					return Font.Height + 3 + 1 /* line */;
			}
		}
		
		internal bool ShowEditRow {
			get {
				if (ListManager != null && !ListManager.CanAddRows)
					return false;

				return !_readonly;
			}
		}
		
		internal bool ShowParentRows {
			get { return ParentRowsVisible && parentRows.Count > 0; }
		}
		
		#endregion Private Instance Properties

		#region Public Instance Methods

		[MonoTODO]
		public bool BeginEdit (DataGridColumnStyle gridColumn, int rowNumber)
		{
			if (is_changing)
				return false;

			int column = CurrentTableStyle.GridColumnStyles.IndexOf (gridColumn);
			if (column < 0)
				return false;

			CurrentCell = new DataGridCell (rowNumber, column);

			/* force editing of CurrentCell if we aren't already editing */
			EditCurrentCell ();

			return true;
		}

		public void BeginInit ()
		{
			initializing = true;
		}

		protected virtual void CancelEditing ()
		{
			if (!is_editing)
				return;

			CurrentTableStyle.GridColumnStyles[current_cell.ColumnNumber].ConcedeFocus ();

			if (is_changing) {
				if (current_cell.ColumnNumber < CurrentTableStyle.GridColumnStyles.Count)
					CurrentTableStyle.GridColumnStyles[current_cell.ColumnNumber].Abort (current_cell.RowNumber);
				is_changing = false;
				InvalidateRowHeader (current_cell.RowNumber);
			}

			if (IsAdding) {
				ListManager.CancelCurrentEdit ();
			}

			is_editing = false;

			//Invalidate ();
		}

		[MonoTODO]
		public void Collapse (int row)
		{
			if (!rows[row].IsExpanded)
				return;

			SuspendLayout ();
			rows[row].IsExpanded = false;
			for (int i = 1; i < rows.Length - row; i ++)
				rows[row + i].VerticalOffset -= rows[row].RelationHeight;

			rows[row].relation_link.Visible = false;
			rows[row].relation_link.Dispose ();
			rows[row].relation_link = null;

			rows[row].Height -= rows[row].RelationHeight;
			rows[row].RelationHeight = 0;
			ResumeLayout (false);

			/* XX need to redraw from @row down */
			CalcAreasAndInvalidate ();			
		}

		protected internal virtual void ColumnStartedEditing (Control editingControl)
		{
			bool need_invalidate = is_changing == false;
			// XXX calculate the row header to invalidate
			// (using the editingControl's position?)
			// instead of using CurrentRow
			is_changing = true;
			if (need_invalidate)
				InvalidateRowHeader (CurrentRow);
		}

		protected internal virtual void ColumnStartedEditing (Rectangle bounds)
		{
			bool need_invalidate = is_changing == false;
			// XXX calculate the row header to invalidate
			// instead of using CurrentRow
			is_changing = true;
			if (need_invalidate)
				InvalidateRowHeader (CurrentRow);
		}

		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			return base.CreateAccessibilityInstance ();
		}

		protected virtual DataGridColumnStyle CreateGridColumn (PropertyDescriptor prop)
		{
			return CreateGridColumn (prop, false);
		}

		protected virtual DataGridColumnStyle CreateGridColumn (PropertyDescriptor prop, bool isDefault)
		{
			throw new NotImplementedException();
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		public bool EndEdit (DataGridColumnStyle gridColumn, int rowNumber, bool shouldAbort)
		{
			if (!is_editing && !is_changing)
				return true;

			if (IsAdding) {
				if (shouldAbort)
					ListManager.CancelCurrentEdit ();
				else
					CalcAreasAndInvalidate ();
			}

			if (shouldAbort || gridColumn.ParentReadOnly)
				gridColumn.Abort (rowNumber);
			else
				gridColumn.Commit (ListManager, rowNumber);

			is_editing = false;
			is_changing = false;
			InvalidateRowHeader (CurrentRow);
			return true;
		}

		public void EndInit ()
		{
			initializing = false;
		}

		void OnRelationLinkClicked (object sender, LinkLabelLinkClickedEventArgs args)
		{
			LinkLabel relation_link = (LinkLabel)sender;
			int row = (int)args.Link.LinkData;
			string relation = CurrentTableStyle.Relations[relation_link.Links.IndexOf (args.Link)];

			NavigateTo (row, relation);
		}

		public void Expand (int row)
		{
			if (rows[row].IsExpanded)
				return;

			rows[row].IsExpanded = true;

			string[] relations = CurrentTableStyle.Relations;
			int i;

			LinkLabel relation_link = new LinkLabel ();

			relation_link.BorderStyle = BorderStyle.FixedSingle;
			relation_link.AutoSize = true;
			relation_link.LinkClicked += new LinkLabelLinkClickedEventHandler (OnRelationLinkClicked);
			relation_link.LinkColor = LinkColor;
			relation_link.ActiveLinkColor = LinkHoverColor;

			StringBuilder relation_text = new StringBuilder ("");

			for (i = 0; i < relations.Length; i ++) {
				if (i > 0)
					relation_text.Append ("\n");

				relation_text.Append (relations[i]);
			}

			relation_link.Text = relation_text.ToString();

			int start = 0;
			for (i = 0; i < relations.Length; i ++) {
				//Console.WriteLine ("adding relation link for text '{0}'", relation_link.Text.Substring (start, relations[i].Length));
				relation_link.Links.Add (start, relations[i].Length + (i < relations.Length - 1 ? 1 : 0), row);
				start += relations[i].Length + 1;
			}

			SuspendLayout ();
			Controls.Add (relation_link);

			relation_link.Location = new Point (cells_area.X + 1,
							    cells_area.Y + rows[row].VerticalOffset - rows[FirstVisibleRow].VerticalOffset + rows[row].Height + 1);
			relation_link.Height = relation_link.Font.Height * relations.Length + 3;

			for (i = 1; i < rows.Length - row; i ++)
				rows[row + i].VerticalOffset += relation_link.Height + 3;
			rows[row].Height += relation_link.Height + 3;
			rows[row].RelationHeight = relation_link.Height + 3;

			rows[row].relation_link = relation_link;
			ResumeLayout (false);

			/* XX need to redraw from @row down */
			CalcAreasAndInvalidate ();
		}

		public Rectangle GetCellBounds (DataGridCell cell)
		{
			return GetCellBounds (cell.RowNumber, cell.ColumnNumber);
		}

		public Rectangle GetCellBounds (int row, int col)
		{
			Rectangle bounds = new Rectangle ();
			int col_pixel;

			bounds.Width = CurrentTableStyle.GridColumnStyles[col].Width;
			bounds.Height = rows[row].Height - rows[row].RelationHeight;
			bounds.Y = cells_area.Y + rows[row].VerticalOffset - rows[FirstVisibleRow].VerticalOffset;
			col_pixel = GetColumnStartingPixel (col);
			bounds.X = cells_area.X + col_pixel - horiz_pixeloffset;
			return bounds;
		}

		public Rectangle GetCurrentCellBounds ()
		{
			return GetCellBounds (current_cell.RowNumber, current_cell.ColumnNumber);
		}

		protected virtual string GetOutputTextDelimiter ()
		{
			return string.Empty;
		}

		protected virtual void GridHScrolled (object sender, ScrollEventArgs se)
		{
			if (se.NewValue == horiz_pixeloffset ||
			    se.Type == ScrollEventType.EndScroll) {
				return;
			}

			ScrollToColumnInPixels (se.NewValue);
		}

		protected virtual void GridVScrolled (object sender, ScrollEventArgs se)
		{
			int old_first_visiblerow = first_visiblerow;
			first_visiblerow = se.NewValue;

			if (first_visiblerow == old_first_visiblerow)
				return;

			UpdateVisibleRowCount ();

			if (first_visiblerow == old_first_visiblerow)
				return;
			
			ScrollToRow (old_first_visiblerow, first_visiblerow);
		}

		public HitTestInfo HitTest (Point position)
		{
			return HitTest (position.X, position.Y);
		}

		const int RESIZE_HANDLE_HORIZ_SIZE = 5;
		const int RESIZE_HANDLE_VERT_SIZE = 3;

		// From Point to Cell
		public HitTestInfo HitTest (int x, int y)
		{
			if (columnhdrs_area.Contains (x, y)) {
				int offset_x = x + horiz_pixeloffset;
				int column_x;
				int column_under_mouse = FromPixelToColumn (offset_x, out column_x);
				
				if ((column_x + CurrentTableStyle.GridColumnStyles[column_under_mouse].Width - offset_x < RESIZE_HANDLE_HORIZ_SIZE)
				    && column_under_mouse < CurrentTableStyle.GridColumnStyles.Count) {

					return new HitTestInfo (-1, column_under_mouse, HitTestType.ColumnResize);
				}
				else {
					return new HitTestInfo (-1, column_under_mouse, HitTestType.ColumnHeader);
				}
			}

			if (rowhdrs_area.Contains (x, y)) {
				int posy;
				int rcnt = FirstVisibleRow + VisibleRowCount;
				for (int r = FirstVisibleRow; r < rcnt; r++) {
					posy = cells_area.Y + rows[r].VerticalOffset - rows[FirstVisibleRow].VerticalOffset;
					if (y <= posy + rows[r].Height) {
						if ((posy + rows[r].Height) - y < RESIZE_HANDLE_VERT_SIZE) {
							return new HitTestInfo (r, -1, HitTestType.RowResize);
						}
						else {
							return new HitTestInfo (r, -1, HitTestType.RowHeader);
						}
					}
				}
			}

			if (caption_area.Contains (x, y)) {
				return new HitTestInfo (-1, -1, HitTestType.Caption);
			}

			if (parent_rows.Contains (x, y)) {
				return new HitTestInfo (-1, -1, HitTestType.ParentRows);
			}

			int pos_y, pos_x, width;
			int rowcnt = FirstVisibleRow + VisibleRowCount;
			for (int row = FirstVisibleRow; row < rowcnt; row++) {

				pos_y = cells_area.Y + rows[row].VerticalOffset - rows[FirstVisibleRow].VerticalOffset;
				if (y <= pos_y + rows[row].Height) {
					int col_pixel;
					int column_cnt = first_visiblecolumn + visiblecolumn_count;
					for (int column = first_visiblecolumn; column < column_cnt; column++) {

						col_pixel = GetColumnStartingPixel (column);
						pos_x = cells_area.X + col_pixel - horiz_pixeloffset;
						width = CurrentTableStyle.GridColumnStyles[column].Width;

						if (x <= pos_x + width) { // Column found
							return new HitTestInfo (row, column, HitTestType.Cell);
						}
					}

					break;
				}
			}

			return new HitTestInfo ();
		}

		public bool IsExpanded (int rowNumber)
		{
			return (rows[rowNumber].IsExpanded);
		}

		public bool IsSelected (int row)
		{
			return selected_rows[row] != null;
		}

		[MonoTODO]
		public void NavigateBack ()
		{
			if (parentRows.Count == 0)
				return;

			DataGridParentRow el = (DataGridParentRow)parentRows.Pop ();
			DataMember = el.datamember;
		}

		[MonoTODO]
		public void NavigateTo (int rowNumber, string relationName)
		{
			if (allow_navigation == false)
				return;
			
			parentRows.Push (new DataGridParentRow (DataMember, (DataRowView)list_manager.Current));

			DataMember = String.Format ("{0}.{1}", DataMember, relationName);
		}

		protected virtual void OnAllowNavigationChanged (EventArgs e)
		{
			if (AllowNavigationChanged != null) {
				AllowNavigationChanged (this, e);
			}
		}

		protected void OnBackButtonClicked (object sender,  EventArgs e)
		{
			if (BackButtonClick != null)
				BackButtonClick (sender, e);
		}

		protected override void OnBackColorChanged (EventArgs e)
		{
			base.OnBackColorChanged (e);
		}

		protected virtual void OnBackgroundColorChanged (EventArgs e)
		{
			if (BackgroundColorChanged != null)
				BackgroundColorChanged (this, e);
		}

		protected override void OnBindingContextChanged (EventArgs e)
		{
			base.OnBindingContextChanged (e);

			current_style.CreateColumnsForTable (false);
			CalcAreasAndInvalidate ();
		}

		protected virtual void OnBorderStyleChanged (EventArgs e)
		{
			if (BorderStyleChanged != null)
				BorderStyleChanged (this, e);
		}

		protected virtual void OnCaptionVisibleChanged (EventArgs e)
		{
			if (CaptionVisibleChanged != null)
				CaptionVisibleChanged (this, e);
		}

		protected virtual void OnCurrentCellChanged (EventArgs e)
		{
			if (CurrentCellChanged != null)
				CurrentCellChanged (this, e);
		}

		protected virtual void OnDataSourceChanged (EventArgs e)
		{
			if (DataSourceChanged != null)
				DataSourceChanged (this, e);
		}

		protected override void OnEnter (EventArgs e)
		{
			base.OnEnter (e);
		}

		protected virtual void OnFlatModeChanged (EventArgs e)
		{
			if (FlatModeChanged != null)
				FlatModeChanged (this, e);
		}

		protected override void OnFontChanged (EventArgs e)
		{
			CalcGridAreas ();
			base.OnFontChanged (e);
		}

		protected override void OnForeColorChanged (EventArgs e)
		{
			base.OnForeColorChanged (e);
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);
			CalcGridAreas ();
		}

		protected override void OnHandleDestroyed (EventArgs e)
		{
			base.OnHandleDestroyed (e);
		}

		protected override void OnKeyDown (KeyEventArgs ke)
		{
			base.OnKeyDown (ke);
			
			if (ProcessGridKey (ke) == true)
				ke.Handled = true;

			/* TODO: we probably don't need this check,
			 * since current_cell wouldn't have been set
			 * to something invalid */
			if (CurrentTableStyle.GridColumnStyles.Count > 0) {
				CurrentTableStyle.GridColumnStyles[current_cell.ColumnNumber].OnKeyDown
					(ke, current_cell.RowNumber, current_cell.ColumnNumber);
			}
		}

		protected override void OnKeyPress (KeyPressEventArgs kpe)
		{
			base.OnKeyPress (kpe);
		}

		protected override void OnLayout (LayoutEventArgs levent)
		{
			base.OnLayout (levent);
			CalcAreasAndInvalidate ();			
		}

		protected override void OnLeave (EventArgs e)
		{
			base.OnLeave (e);

#if false
			/* we get an OnLeave call when the
			 * DataGridTextBox control is focused, so we
			 * need to ignore that.  If we get an OnLeave
			 * call when a child control is not receiving
			 * focus, we need to cancel the current
			 * edit. */
			if (IsAdding) {
				ListManager.CancelCurrentEdit ();
			}
#endif
		}

		protected override void OnMouseDown (MouseEventArgs e)
		{
			base.OnMouseDown (e);

			bool ctrl_pressed = ((Control.ModifierKeys & Keys.Control) != 0);
			bool shift_pressed = ((Control.ModifierKeys & Keys.Shift) != 0);

			HitTestInfo testinfo;
			testinfo = HitTest (e.X, e.Y);

			switch (testinfo.Type) {
			case HitTestType.Cell:
			{
				if (testinfo.Row < 0 || testinfo.Column < 0)
					break;
					
				DataGridCell new_cell = new DataGridCell (testinfo.Row, testinfo.Column);

				if ((new_cell.Equals (current_cell) == false) || (!is_editing)) {
					CurrentCell = new_cell;
					EditCurrentCell ();
				} else {
					CurrentTableStyle.GridColumnStyles[testinfo.Column].OnMouseDown (e, testinfo.Row, testinfo.Column);
				}

				break;
			}

			case HitTestType.RowHeader:
			{
				bool expansion_click = false;
				if (CurrentTableStyle.HasRelations) {
					if (e.X > rowhdrs_area.X + rowhdrs_area.Width / 2) {
						/* it's in the +/- space */
						if (IsExpanded (testinfo.Row))
							Collapse (testinfo.Row);
						else
							Expand (testinfo.Row);

						expansion_click = true;
					}
				}

				if (!ctrl_pressed &&
				    !shift_pressed &&
				    !expansion_click) {
					ResetSelection (); // Invalidates selected rows
				}

				if ((shift_pressed ||
				     expansion_click)
				    && selection_start != -1) {
					ShiftSelection (testinfo.Row);
				} else { // ctrl_pressed or single item
					selection_start = testinfo.Row;
					Select (testinfo.Row);
				}

				CancelEditing ();
				CurrentRow = testinfo.Row;
				//Console.WriteLine ("After setting CurrentRow, list_manager.Position = {0}", list_manager.Position);
				OnRowHeaderClick (EventArgs.Empty);

				break;
			}

			case HitTestType.ColumnHeader:
			{
				if (CurrentTableStyle.GridColumnStyles.Count == 0)
					break;

				if (AllowSorting == false)
					break;

				if (ListManager.List is IBindingList == false)
					break;
			
				ListSortDirection direction = ListSortDirection.Ascending;
				PropertyDescriptor prop = CurrentTableStyle.GridColumnStyles[testinfo.Column].PropertyDescriptor;
				IBindingList list = (IBindingList) ListManager.List;

				if (list.SortProperty != null) {
					CurrentTableStyle.GridColumnStyles[list.SortProperty].ArrowDrawingMode 
						= DataGridColumnStyle.ArrowDrawing.No;
				}

				if (prop == list.SortProperty && list.SortDirection == ListSortDirection.Ascending) {
					direction = ListSortDirection.Descending;
				}
				
				CurrentTableStyle.GridColumnStyles[testinfo.Column].ArrowDrawingMode =
					direction == ListSortDirection.Ascending ? 
					DataGridColumnStyle.ArrowDrawing.Ascending : DataGridColumnStyle.ArrowDrawing.Descending;
				
				list.ApplySort (prop, direction);
				Refresh ();
				break;
			}

			case HitTestType.ColumnResize:
			{
				resize_column = testinfo.Column;
				column_resize_active = true;
				resize_column_x = e.X;
				resize_column_width_delta = 0;
				DrawResizeLineVert (resize_column_x);
				break;
			}

			case HitTestType.RowResize:
			{
				resize_row = testinfo.Row;
				row_resize_active = true;
				resize_row_y = e.Y;
				resize_row_height_delta = 0;
				DrawResizeLineHoriz (resize_row_y);
				break;
			}

			default:
				break;
			}
		}

		protected override void OnMouseLeave (EventArgs e)
		{
			base.OnMouseLeave (e);
		}

		protected override void OnMouseMove (MouseEventArgs e)
		{
			base.OnMouseMove (e);

			if (column_resize_active) {
				/* erase the old line */
				DrawResizeLineVert (resize_column_x + resize_column_width_delta);

				resize_column_width_delta = e.X - resize_column_x;

				/* draw the new line */
				DrawResizeLineVert (resize_column_x + resize_column_width_delta);
				return;
			}
			else if (row_resize_active) {
				/* erase the old line */
				DrawResizeLineHoriz (resize_row_y + resize_row_height_delta);

				resize_row_height_delta = e.Y - resize_row_y;

				/* draw the new line */
				DrawResizeLineHoriz (resize_row_y + resize_row_height_delta);
				return;
			}
			else {
				HitTestInfo testinfo;
				testinfo = HitTest (e.X, e.Y);

				switch (testinfo.Type) {
				case HitTestType.ColumnResize:
					Cursor = Cursors.VSplit;
					break;
				case HitTestType.RowResize:
					Cursor = Cursors.HSplit;
					break;
				default:
					Cursor = Cursors.Default;
					break;
				}
			}
		}

		protected override void OnMouseUp (MouseEventArgs e)
		{
			base.OnMouseUp (e);

			if (column_resize_active) {
				column_resize_active = false;
				if (resize_column_width_delta + CurrentTableStyle.GridColumnStyles[resize_column].Width < 0)
					resize_column_width_delta = -CurrentTableStyle.GridColumnStyles[resize_column].Width;
				CurrentTableStyle.GridColumnStyles[resize_column].Width += resize_column_width_delta;
				width_of_all_columns += resize_column_width_delta;
				Invalidate ();
			}
			else if (row_resize_active) {
				row_resize_active = false;

				if (resize_row_height_delta + rows[resize_row].Height < 0)
					resize_row_height_delta = -rows[resize_row].Height;

				rows[resize_row].Height = rows[resize_row].Height + resize_row_height_delta;
				for (int i = resize_row + 1; i < rows.Length; i ++)
					rows[i].VerticalOffset += resize_row_height_delta;

				CalcAreasAndInvalidate ();
			}
		}

		protected override void OnMouseWheel (MouseEventArgs e)
		{
			base.OnMouseWheel (e);

			bool ctrl_pressed = ((Control.ModifierKeys & Keys.Control) != 0);
			int pixels;

			if (ctrl_pressed) { // scroll horizontally
				if (e.Delta > 0) {
					/* left */
					pixels = Math.Max (horiz_scrollbar.Minimum,
							   horiz_scrollbar.Value - horiz_scrollbar.LargeChange);
				}
				else {
					/* right */
					pixels = Math.Min (horiz_scrollbar.Maximum - horiz_scrollbar.LargeChange + 1,
							   horiz_scrollbar.Value + horiz_scrollbar.LargeChange);
				}

				GridHScrolled (this, new ScrollEventArgs (ScrollEventType.ThumbPosition, pixels));
				horiz_scrollbar.Value = pixels;
			} else {
				if (e.Delta > 0) {
					/* up */
					pixels = Math.Max (vert_scrollbar.Minimum,
							   vert_scrollbar.Value - vert_scrollbar.LargeChange);
				}
				else {
					/* down */
					pixels = Math.Min (vert_scrollbar.Maximum - vert_scrollbar.LargeChange + 1,
							   vert_scrollbar.Value + vert_scrollbar.LargeChange);
				}

				GridVScrolled (this, new ScrollEventArgs (ScrollEventType.ThumbPosition, pixels));
				vert_scrollbar.Value = pixels;
			}
		}

		protected void OnNavigate (NavigateEventArgs e)
		{
			if (Navigate != null)
				Navigate (this, e);
		}

		protected override void OnPaint (PaintEventArgs pe)
		{
			ThemeEngine.Current.DataGridPaint (pe, this);
		}

		protected override void OnPaintBackground (PaintEventArgs ebe)
		{
		}

		protected virtual void OnParentRowsLabelStyleChanged (EventArgs e)
		{
			if (ParentRowsLabelStyleChanged != null)
				ParentRowsLabelStyleChanged (this, e);
		}

		protected virtual void OnParentRowsVisibleChanged (EventArgs e)
		{
			if (ParentRowsVisibleChanged != null)
				ParentRowsVisibleChanged (this, e);
		}

		protected virtual void OnReadOnlyChanged (EventArgs e)
		{
			if (ReadOnlyChanged != null)
				ReadOnlyChanged (this, e);
		}

		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);
		}

		protected void OnRowHeaderClick (EventArgs e)
		{
			if (RowHeaderClick != null)
				RowHeaderClick (this, e);
		}

		protected void OnScroll (EventArgs e)
		{
			if (Scroll != null)
				Scroll (this, e);
		}

		protected void OnShowParentDetailsButtonClicked (object sender, EventArgs e)
		{
			if (ShowParentDetailsButtonClick != null)
				ShowParentDetailsButtonClick (sender, e);
		}

		protected override bool ProcessDialogKey (Keys keyData)
		{
			return ProcessGridKey (new KeyEventArgs (keyData));
		}

		void UpdateSelectionAfterCursorMove (bool extend_selection)
		{
			if (extend_selection) {
				CancelEditing ();
				ShiftSelection (CurrentRow);
			}
			else {
				ResetSelection ();
				if (!is_editing)
					EditCurrentCell ();
			}
		}

		protected bool ProcessGridKey (KeyEventArgs ke)
		{
			/* if we have no rows, exit immediately.
			   XXX is this necessary? */
			if (RowsCount == 0)
				return false;

			bool ctrl_pressed = ((ke.Modifiers & Keys.Control) != 0);
			bool alt_pressed = ((ke.Modifiers & Keys.Alt) != 0);
			bool shift_pressed = ((ke.Modifiers & Keys.Shift) != 0);

			switch (ke.KeyCode) {
			case Keys.Escape:
				CancelEditing ();
				return true;
				
			case Keys.D0:
				if (alt_pressed) {
					if (is_editing)
						CurrentTableStyle.GridColumnStyles[CurrentColumn].EnterNullValue ();
				}
				return true;

			case Keys.Enter:
				CurrentRow ++;
				if (!is_editing)
					EditCurrentCell ();
				return true;

			case Keys.Tab:
				if (CurrentColumn < CurrentTableStyle.GridColumnStyles.Count - 1)
					CurrentColumn ++;
				else if ((CurrentRow <= RowsCount - 1) && (CurrentColumn == CurrentTableStyle.GridColumnStyles.Count - 1))
					CurrentCell = new DataGridCell (CurrentRow + 1, 0);

				UpdateSelectionAfterCursorMove (false);

				return true;

			case Keys.Right:
				if (ctrl_pressed) {
					CurrentColumn = CurrentTableStyle.GridColumnStyles.Count - 1;
				}
				else {
					if (CurrentColumn < CurrentTableStyle.GridColumnStyles.Count - 1) {
						CurrentColumn ++;
					} else if (CurrentRow < RowsCount - 1
						   || (CurrentRow == RowsCount - 1
						       && !IsAdding)) {
						CurrentCell = new DataGridCell (CurrentRow + 1, 0);
					}
				}

				UpdateSelectionAfterCursorMove (false);

				return true;

			case Keys.Left:
				if (ctrl_pressed) {
					CurrentColumn = 0;
				}
				else {
					if (current_cell.ColumnNumber > 0)
						CurrentColumn --;
					else if (CurrentRow > 0)
						CurrentCell = new DataGridCell (CurrentRow - 1, CurrentTableStyle.GridColumnStyles.Count - 1);
				}

				UpdateSelectionAfterCursorMove (false);

				return true;

			case Keys.Up:
				if (ctrl_pressed)
					CurrentRow = 0;
				else if (CurrentRow > 0)
					CurrentRow --;

				UpdateSelectionAfterCursorMove (shift_pressed);

				return true;

			case Keys.Down:
				if (ctrl_pressed)
					CurrentRow = RowsCount - 1;
				else if (CurrentRow < RowsCount - 1)
					CurrentRow ++;
				else if (CurrentRow == RowsCount - 1 && !IsAdding && !shift_pressed)
					CurrentRow ++;

				UpdateSelectionAfterCursorMove (shift_pressed);

				return true;

			case Keys.PageUp:
				if (CurrentRow > VLargeChange)
					CurrentRow -= VLargeChange;
				else
					CurrentRow = 0;

				UpdateSelectionAfterCursorMove (shift_pressed);

				return true;

			case Keys.PageDown:
				if (CurrentRow < RowsCount - VLargeChange)
					CurrentRow += VLargeChange;
				else
					CurrentRow = RowsCount - 1;

				UpdateSelectionAfterCursorMove (shift_pressed);

				return true;

			case Keys.Home:
				if (ctrl_pressed)
					CurrentCell = new DataGridCell (0, 0);
				else
					CurrentColumn = 0;

				UpdateSelectionAfterCursorMove (ctrl_pressed && shift_pressed);

				return true;

			case Keys.End:
				if (ctrl_pressed)
					CurrentCell = new DataGridCell (RowsCount - 1, CurrentTableStyle.GridColumnStyles.Count - 1);
				else
					CurrentColumn = CurrentTableStyle.GridColumnStyles.Count - 1;

				UpdateSelectionAfterCursorMove (ctrl_pressed && shift_pressed);

				return true;

			case Keys.Delete:
				foreach (int row in selected_rows.Keys) {
					ListManager.RemoveAt (row);						
				}
				selected_rows.Clear ();
				CalcAreasAndInvalidate ();

				return true;
			}

			return false; // message not processed
		}

		protected override bool ProcessKeyPreview (ref Message m)
		{
			if ((Msg)m.Msg == Msg.WM_KEYDOWN) {
				Keys key = (Keys) m.WParam.ToInt32 ();
				KeyEventArgs ke = new KeyEventArgs (key);
				if (ProcessGridKey (ke) == true) {
					return true;
				}
			}

			return base.ProcessKeyPreview (ref m);
		}
		
		protected bool ProcessTabKey (Keys keyData)
		{
			return false;
		}

		public void ResetAlternatingBackColor ()
		{
			grid_style.AlternatingBackColor = default_style.AlternatingBackColor;
		}

		public override void ResetBackColor ()
		{
			grid_style.BackColor = default_style.BackColor;
		}

		public override void ResetForeColor ()
		{
			base.ResetForeColor ();
		}

		public void ResetGridLineColor ()
		{
			grid_style.GridLineColor = default_style.GridLineColor;
		}

		public void ResetHeaderBackColor ()
		{
			grid_style.HeaderBackColor = default_style.HeaderBackColor;
		}

		public void ResetHeaderFont ()
		{
			grid_style.HeaderFont = default_style.HeaderFont;
		}

		public void ResetHeaderForeColor ()
		{
			grid_style.HeaderForeColor = default_style.HeaderForeColor;
		}

		public void ResetLinkColor ()
		{
			grid_style.LinkColor = default_style.LinkColor;
		}

		public void ResetLinkHoverColor ()
		{
			grid_style.LinkHoverColor = default_style.LinkHoverColor;
		}

		protected void ResetSelection ()
		{
			InvalidateSelection ();
			selected_rows.Clear ();
			selection_start = -1;
		}

		void InvalidateSelection ()
		{
			foreach (int row in selected_rows.Keys) {
				InvalidateRow (row);
				InvalidateRowHeader (row);
			}
		}

		public void ResetSelectionBackColor ()
		{
			grid_style.SelectionBackColor = default_style.SelectionBackColor;
		}

		public void ResetSelectionForeColor ()
		{
			grid_style.SelectionForeColor = default_style.SelectionForeColor;
		}

		public void Select (int row)
		{
			if (selected_rows.Count == 0)
				selection_start = row;

			if (selected_rows[row] == null)
				selected_rows.Add (row, true);
			else
				selected_rows[row] = true;

			InvalidateRow (row);
		}

		public void SetDataBinding (object dataSource, string dataMember)
		{
			this.datamember = string.Empty;
			SetDataSource (dataSource);
			SetDataMember (dataMember);		
			SetNewDataSource ();
		}

		protected virtual bool ShouldSerializeAlternatingBackColor ()
		{
			return (grid_style.AlternatingBackColor != default_style.AlternatingBackColor);
		}

		protected virtual bool ShouldSerializeBackgroundColor ()
		{
			return (background_color != def_background_color);
		}

		protected virtual bool ShouldSerializeCaptionBackColor ()
		{
			return (caption_backcolor != def_caption_backcolor);
		}

		protected virtual bool ShouldSerializeCaptionForeColor ()
		{
			return caption_forecolor != def_caption_forecolor;
		}

		protected virtual bool ShouldSerializeGridLineColor ()
		{
			return grid_style.GridLineColor != default_style.GridLineColor;
		}

		protected virtual bool ShouldSerializeHeaderBackColor ()
		{
			return grid_style.HeaderBackColor != default_style.HeaderBackColor;
		}

		protected bool ShouldSerializeHeaderFont ()
		{
			return grid_style.HeaderFont != default_style.HeaderFont;
		}

		protected virtual bool ShouldSerializeHeaderForeColor ()
		{
			return grid_style.HeaderForeColor != default_style.HeaderForeColor;
		}

		protected virtual bool ShouldSerializeLinkHoverColor ()
		{
			return grid_style.LinkHoverColor != grid_style.LinkHoverColor;
		}

		protected virtual bool ShouldSerializeParentRowsBackColor ()
		{
			return parentrowsback_color != def_parentrowsback_color;
		}

		protected virtual bool ShouldSerializeParentRowsForeColor ()
		{
			return parentrowsback_color != def_parentrowsback_color;
		}

		protected bool ShouldSerializePreferredRowHeight ()
		{
			return grid_style.PreferredRowHeight != default_style.PreferredRowHeight;
		}

		protected bool ShouldSerializeSelectionBackColor ()
		{
			return grid_style.SelectionBackColor != default_style.SelectionBackColor;
		}

		protected virtual bool ShouldSerializeSelectionForeColor ()
		{
			return grid_style.SelectionForeColor != default_style.SelectionForeColor;
		}

		public void SubObjectsSiteChange (bool site)
		{
		}

		public void UnSelect (int row)
		{
			selected_rows.Remove (row);
			InvalidateRow (row);
		}
		#endregion	// Public Instance Methods

		#region Private Instance Methods

		internal void CalcAreasAndInvalidate ()
		{
			CalcGridAreas ();
			Invalidate ();
		}
		
		private void ConnectListManagerEvents ()
		{
			list_manager.PositionChanged += new EventHandler (OnListManagerPositionChanged);
			list_manager.ItemChanged += new ItemChangedEventHandler (OnListManagerItemChanged);
		}
		
		private void DisconnectListManagerEvents ()
		{
			list_manager.PositionChanged -= new EventHandler (OnListManagerPositionChanged);
			list_manager.ItemChanged -= new ItemChangedEventHandler (OnListManagerItemChanged);
		}

		private void EnsureCellVisibility (DataGridCell cell)
		{
			if (cell.ColumnNumber <= first_visiblecolumn ||
				cell.ColumnNumber + 1 >= first_visiblecolumn + visiblecolumn_count) {			

				first_visiblecolumn = GetFirstColumnForColumnVisibility (first_visiblecolumn, cell.ColumnNumber);
                                int pixel = GetColumnStartingPixel (first_visiblecolumn);
				ScrollToColumnInPixels (pixel);
				horiz_scrollbar.Value = pixel;
				Update();
			}

			if (cell.RowNumber < first_visiblerow ||
			    cell.RowNumber + 1 >= first_visiblerow + visiblerow_count) {

                                if (cell.RowNumber + 1 >= first_visiblerow + visiblerow_count) {
					int old_first_visiblerow = first_visiblerow;
					first_visiblerow = 1 + cell.RowNumber - visiblerow_count;
					UpdateVisibleRowCount ();
					ScrollToRow (old_first_visiblerow, first_visiblerow);
				} else {
					int old_first_visiblerow = first_visiblerow;
					first_visiblerow = cell.RowNumber;
					UpdateVisibleRowCount ();
					ScrollToRow (old_first_visiblerow, first_visiblerow);
				}

				vert_scrollbar.Value = first_visiblerow;
			}
		}
		
		private bool SetDataMember (string member)
		{
			if (member == datamember)
				return false;

			datamember = member;

			if (list_manager != null) {
				DisconnectListManagerEvents ();
				list_manager = null;
			}

			return true;
		}

		private void SetDataSource (object source)
		{			
			if (source != null && source as IListSource != null && source as IList != null) {
				throw new Exception ("Wrong complex data binding source");
			}

			if (is_editing)
				CancelEditing ();

			current_cell = new DataGridCell ();
			datasource = source;
			if (list_manager != null) {
				DisconnectListManagerEvents ();
				list_manager = null;
			}

			OnDataSourceChanged (EventArgs.Empty);
		}

		void DisposeRelationLinks ()
		{
			//Console.WriteLine ("Disposing of relation links");
			SuspendLayout ();
			for (int i = 0; i < rows.Length; i ++) {
				if (rows[i].relation_link != null) {
					rows[i].relation_link.Visible = false;
					rows[i].relation_link.Dispose ();
					rows[i].relation_link = null;
				}
			}
			ResumeLayout (false);
		}

		private void SetNewDataSource ()
		{
			DisposeRelationLinks ();

			if (ListManager != null) {
				string list_name = ListManager.GetListName (null);
				if (TableStyles[list_name] == null) {
					current_style.GridColumnStyles.Clear ();			
					current_style.CreateColumnsForTable (false);
				}
				else if (CurrentTableStyle.MappingName != list_name) {
					// If the style has been defined by the user, use it
					CurrentTableStyle = styles_collection[list_name];
					current_style.CreateColumnsForTable (true);
				}
				else
					current_style.CreateColumnsForTable (false);
			}
			else
				current_style.CreateColumnsForTable (false);
			
			CalcAreasAndInvalidate ();			
		}

		private void OnListManagerPositionChanged (object sender, EventArgs e)
		{
			//Console.WriteLine ("list manager position = {0}", list_manager.Position);
			from_positionchanged_handler = true;
			CurrentRow = list_manager.Position;
			from_positionchanged_handler = false;
		}

		private void OnListManagerItemChanged (object sender, ItemChangedEventArgs e)
		{
			if (e.Index == -1) {
				if (ListManager.Count >= rows.Length) {
					DataGridRow[] new_rows = new DataGridRow[ListManager.Count + 1];
					if (ListManager.Count >= rows.Length) {
						Array.Copy (rows, 0, new_rows, 0, rows.Length);
						for (int i = rows.Length; i < ListManager.Count + 1; i ++) {
							new_rows[i].Height = RowHeight;
							if (i > 0)
								new_rows[i].VerticalOffset = new_rows[i-1].VerticalOffset + new_rows[i-1].Height;
						}
					}
					else
						Array.Copy (rows, 0, new_rows, 0, new_rows.Length);
					rows = new_rows;
				}
				ResetSelection ();
				CalcAreasAndInvalidate ();
			}
			else {
				InvalidateRow (e.Index);
			}
		}

		private void OnTableStylesCollectionChanged (object sender, CollectionChangeEventArgs e)
		{
			if (ListManager == null)
				return;
			
			string list_name = ListManager.GetListName (null);
			switch (e.Action){
				case CollectionChangeAction.Add: {
					if (e.Element != null && String.Compare (list_name, ((DataGridTableStyle)e.Element).MappingName, true) == 0) {
						CurrentTableStyle = (DataGridTableStyle)e.Element;
						((DataGridTableStyle) e.Element).CreateColumnsForTable (false);
					}
					break;
				}

				case CollectionChangeAction.Remove: {
					if (e.Element != null && String.Compare (list_name, ((DataGridTableStyle)e.Element).MappingName, true) == 0) {
						CurrentTableStyle = default_style;						
						current_style.GridColumnStyles.Clear ();
						current_style.CreateColumnsForTable (false);
					}
					break;
				}	

				
				case CollectionChangeAction.Refresh: {
					if (e.Element != null && String.Compare (list_name, ((DataGridTableStyle)e.Element).MappingName, true) == 0) {
						CurrentTableStyle = (DataGridTableStyle)e.Element;
						((DataGridTableStyle) e.Element).CreateColumnsForTable (false);
					} else {
						CurrentTableStyle = default_style;
						current_style.GridColumnStyles.Clear ();
						current_style.CreateColumnsForTable (false);

					}
					break;

				}
			}						
			CalcAreasAndInvalidate ();
		}

		private void EditCurrentCell ()
		{
			is_editing = true;

			CurrentTableStyle.GridColumnStyles[current_cell.ColumnNumber].Edit (ListManager,
				current_cell.RowNumber, GetCellBounds (current_cell.RowNumber, current_cell.ColumnNumber),
				_readonly, string.Empty, true);
		}

		private void ShiftSelection (int index)
		{
			// we have to save off selection_start
			// because ResetSelection clobbers it
			int saved_selection_start = selection_start;
			int start, end;

			ResetSelection ();
			selection_start = saved_selection_start;

			if (index >= selection_start) {
				start = selection_start;
				end = index;
			}
			else {
				start = index;
				end = selection_start;
			}

			for (int idx = start; idx <= end; idx ++) {
				Select (idx);
			}
		}

		private void ScrollToColumnInPixels (int pixel)
		{
			int pixels;

			if (pixel > horiz_pixeloffset) // ScrollRight
				pixels = -1 * (pixel - horiz_pixeloffset);
			else
				pixels = horiz_pixeloffset - pixel;

			Rectangle area = cells_area;
				
			horiz_pixeloffset = pixel;
			UpdateVisibleColumn ();

			if (ColumnHeadersVisible == true) {
				area.Y -= ColumnHeadersArea.Height;
				area.Height += ColumnHeadersArea.Height;
			}

			XplatUI.ScrollWindow (Handle, area, pixels, 0, false);
		}

		private void ScrollToRow (int old_row, int new_row)
		{
			int pixels = 0;
			int i;

			if (new_row > old_row) { // Scrolldown
				for (i = old_row; i < new_row; i ++)
					pixels -= rows[i].Height;
			}
			else {
				for (i = new_row; i < old_row; i ++)
					pixels += rows[i].Height;
			}

			Rectangle rows_area = cells_area; // Cells area - partial rows space
			if (RowHeadersVisible) {
				rows_area.X -= RowHeaderWidth;
				rows_area.Width += RowHeaderWidth;
			}

			rows_area.Height = cells_area.Height - cells_area.Height % RowHeight;

			/* scroll the window */
			XplatUI.ScrollWindow (Handle, rows_area, 0, pixels, false);
			

			/* now update the position of all the relation links */
			SuspendLayout ();
			for (i = 0; i < rows.Length; i ++) {
				if (rows[i].relation_link != null) {
					rows[i].relation_link.Visible = i >= new_row;
					rows[i].relation_link.Location = new Point (rows[i].relation_link.Location.X,
										    rows[i].relation_link.Location.Y + pixels);
				}
			}
			ResumeLayout (false);
		}

		#endregion Private Instance Methods


		#region Events
		public event EventHandler AllowNavigationChanged;
		public event EventHandler BackButtonClick;
		public event EventHandler BackgroundColorChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler CursorChanged {
			add { base.CursorChanged += value; }
			remove { base.CursorChanged -= value; }
		}

		public event EventHandler BorderStyleChanged;
		public event EventHandler CaptionVisibleChanged;
		public event EventHandler CurrentCellChanged;
		public event EventHandler DataSourceChanged;
		public event EventHandler FlatModeChanged;
		public event NavigateEventHandler Navigate;
		public event EventHandler ParentRowsLabelStyleChanged;
		public event EventHandler ParentRowsVisibleChanged;
		public event EventHandler ReadOnlyChanged;
		protected event EventHandler RowHeaderClick;
		public event EventHandler Scroll;
		public event EventHandler ShowParentDetailsButtonClick;

		#endregion	// Events




		#region Code originally in DataGridDrawingLogic.cs

		#region	Local Variables

		// Areas
		Rectangle parent_rows;
		int width_of_all_columns;

		internal Rectangle caption_area;
		internal Rectangle columnhdrs_area;	// Used columns header area
		internal int columnhdrs_maxwidth; 	// Total width (max width) for columns headrs
		internal Rectangle rowhdrs_area;	// Used Headers rows area
		internal Rectangle cells_area;
		#endregion // Local Variables


		#region Public Instance Methods

		// Calc the max with of all columns
		int CalcAllColumnsWidth ()
		{
			int width = 0;
			int cnt = CurrentTableStyle.GridColumnStyles.Count;

			for (int col = 0; col < cnt; col++)
				width += CurrentTableStyle.GridColumnStyles[col].Width;

			return width;
		}

		// Gets a column from a pixel
		int FromPixelToColumn (int pixel, out int column_x)
		{
			int width = 0;
			int cnt = CurrentTableStyle.GridColumnStyles.Count;
			column_x = 0;

			if (cnt == 0)
				return 0;
				
			if (CurrentTableStyle.CurrentRowHeadersVisible) {
				width += rowhdrs_area.X + rowhdrs_area.Width;
				column_x += rowhdrs_area.X + rowhdrs_area.Width;
			}

			for (int col = 0; col < cnt; col++) {
				width += CurrentTableStyle.GridColumnStyles[col].Width;

				if (pixel < width)
					return col;

				column_x += CurrentTableStyle.GridColumnStyles[col].Width;
			}

			return cnt - 1;
		}

		internal int GetColumnStartingPixel (int my_col)
		{
			int width = 0;
			int cnt = CurrentTableStyle.GridColumnStyles.Count;

			for (int col = 0; col < cnt; col++) {

				if (my_col == col)
					return width;

				width += CurrentTableStyle.GridColumnStyles[col].Width;
			}

			return 0;
		}
		
		// Which column has to be the first visible column to ensure a column visibility
		int GetFirstColumnForColumnVisibility (int current_first_visiblecolumn, int column)
		{
			int new_col = column;
			int width = 0;
			
			if (column > current_first_visiblecolumn) { // Going forward								
				for (new_col = column; new_col >= 0; new_col--){
					width += CurrentTableStyle.GridColumnStyles[new_col].Width;
					
					if (width >= cells_area.Width)
						return new_col + 1;
						//return new_col < CurrentTableStyle.GridColumnStyles.Count ? new_col + 1 : CurrentTableStyle.GridColumnStyles.Count;
				}
				return 0;
			} else {				
				return  column;
			}			
		}

		bool in_calc_grid_areas;
		void CalcGridAreas ()
		{
			if (IsHandleCreated == false) // Delay calculations until the handle is created
				return;

			/* make sure we don't happen to end up in this method again */
			if (in_calc_grid_areas)
				return;

			in_calc_grid_areas = true;

			/* Order is important. E.g. row headers max. height depends on caption */
			horiz_pixeloffset = 0;
			CalcCaption ();
			CalcParentRows ();
			CalcParentButtons ();
			UpdateVisibleRowCount ();
			CalcRowHeaders ();
			width_of_all_columns = CalcAllColumnsWidth ();
			CalcColumnHeaders ();
			CalcCellsArea ();

			bool needHoriz = false;
			bool needVert = false;

			/* figure out which scrollbars we need, and what the visible areas are */
			int visible_cells_width = cells_area.Width;
			int visible_cells_height = cells_area.Height;
			int allrows = RowsCount;

			if (ShowEditRow && RowsCount > 0)
				allrows++;

			/* use a loop to iteratively calculate whether
			 * we need horiz/vert scrollbars. */
			for (int i = 0; i < 3; i ++) {
				if (needVert)
					visible_cells_width = cells_area.Width - vert_scrollbar.Width;
				if (needHoriz)
					visible_cells_height = cells_area.Height - horiz_scrollbar.Height;

				UpdateVisibleRowCount ();

				needHoriz = (width_of_all_columns > visible_cells_width);
				needVert = (visiblerow_count != allrows);
			}

			int horiz_scrollbar_width = ClientRectangle.Width;
			int horiz_scrollbar_maximum = 0;
			int vert_scrollbar_height = 0;
			int vert_scrollbar_maximum = 0;

			if (needVert)
				SetUpVerticalScrollBar (out vert_scrollbar_height, out vert_scrollbar_maximum);

			if (needHoriz)
				SetUpHorizontalScrollBar (out horiz_scrollbar_maximum);

			cells_area.Width = visible_cells_width;
			cells_area.Height = visible_cells_height;

			if (needVert && needHoriz) {
				if (ShowParentRows)
					parent_rows.Width -= vert_scrollbar.Width;

				if (!ShowingColumnHeaders) {
					if (columnhdrs_area.X + columnhdrs_area.Width > vert_scrollbar.Location.X) {
						columnhdrs_area.Width -= vert_scrollbar.Width;
					}
				}

				horiz_scrollbar_width -= vert_scrollbar.Width;
				vert_scrollbar_height -= horiz_scrollbar.Height;
			}

			if (needVert) {
				if (rowhdrs_area.Y + rowhdrs_area.Height > ClientRectangle.Y + ClientRectangle.Height) {
					rowhdrs_area.Height -= horiz_scrollbar.Height;
				}

				vert_scrollbar.Height = vert_scrollbar_height;
				vert_scrollbar.Maximum = vert_scrollbar_maximum;
				Controls.Add (vert_scrollbar);
				vert_scrollbar.Visible = true;
			}
			else {
				Controls.Remove (vert_scrollbar);
				vert_scrollbar.Visible = false;
			}

			if (needHoriz) {
				horiz_scrollbar.Width = horiz_scrollbar_width;
				horiz_scrollbar.Maximum = horiz_scrollbar_maximum;
				Controls.Add (horiz_scrollbar);
				horiz_scrollbar.Visible = true;
			}
			else {
				Controls.Remove (horiz_scrollbar);
				horiz_scrollbar.Visible = false;
			}

			UpdateVisibleColumn ();
			UpdateVisibleRowCount ();

			in_calc_grid_areas = false;
		}

		void CalcCaption ()
		{
			caption_area.X = ClientRectangle.X;
			caption_area.Y = ClientRectangle.Y;
			caption_area.Width = ClientRectangle.Width;
			if (caption_visible)
				caption_area.Height = CaptionFont.Height + 6;
			else
				caption_area.Height = 0;
		}

		void CalcCellsArea ()
		{
			cells_area.X = ClientRectangle.X + rowhdrs_area.Width;
			cells_area.Y = columnhdrs_area.Y + columnhdrs_area.Height;
			cells_area.Width = ClientRectangle.X + ClientRectangle.Width - cells_area.X;
			cells_area.Height = ClientRectangle.Y + ClientRectangle.Height - cells_area.Y;
		}

		void CalcColumnHeaders ()
		{
			int max_width_cols;

			columnhdrs_area.X = ClientRectangle.X;
			columnhdrs_area.Y = parent_rows.Y + parent_rows.Height;

			// TODO: take into account Scrollbars
			columnhdrs_maxwidth = ClientRectangle.X + ClientRectangle.Width - columnhdrs_area.X;
			max_width_cols = columnhdrs_maxwidth;

			if (CurrentTableStyle.CurrentRowHeadersVisible)
				max_width_cols -= RowHeaderWidth;

			if (width_of_all_columns > max_width_cols) {
				columnhdrs_area.Width = columnhdrs_maxwidth;
			} else {
				columnhdrs_area.Width = width_of_all_columns;

				if (CurrentTableStyle.CurrentRowHeadersVisible)
					columnhdrs_area.Width += RowHeaderWidth;
			}

			if (ShowingColumnHeaders)
				columnhdrs_area.Height = CurrentTableStyle.HeaderFont.Height + 6;
			else
				columnhdrs_area.Height = 0;
		}

		void CalcParentRows ()
		{
			parent_rows.X = ClientRectangle.X;
			parent_rows.Y = caption_area.Y + caption_area.Height;
			parent_rows.Width = ClientRectangle.Width;
			if (ShowParentRows)
				parent_rows.Height = (CaptionFont.Height + 3) * parentRows.Count;
			else
				parent_rows.Height = 0;
		}

		void NavigateBackClicked (object sender, EventArgs args)
		{
			NavigateBack ();
		}

		void HideParentRowsClicked (object sender, EventArgs args)
		{
			ParentRowsVisible = !ParentRowsVisible;
		}

		void CalcParentButtons ()
		{
			if (parentRows.Count > 0 && CaptionVisible) {
				if (navigate_back_button == null) {
					navigate_back_button = new Button ();
					navigate_back_button.Text = "<-";
					navigate_back_button.BackColor = ThemeEngine.Current.DataGridCaptionBackColor;
					navigate_back_button.ForeColor = ThemeEngine.Current.DataGridCaptionForeColor;
					navigate_back_button.Location = new Point (ClientRectangle.X + ClientRectangle.Width - 2 * (caption_area.Height - 2) - 8, caption_area.Y + 1);
					navigate_back_button.Size = new Size (caption_area.Height - 2, caption_area.Height - 2);
					navigate_back_button.Click += new EventHandler (NavigateBackClicked);
				}

				if (hide_parent_rows_button == null) {
					hide_parent_rows_button = new Button ();
					hide_parent_rows_button.Text = "X";
					hide_parent_rows_button.BackColor = ThemeEngine.Current.DataGridCaptionBackColor;
					hide_parent_rows_button.ForeColor = ThemeEngine.Current.DataGridCaptionForeColor;
					hide_parent_rows_button.Location = new Point (ClientRectangle.X + ClientRectangle.Width - (caption_area.Height - 2) - 4, caption_area.Y + 1);
					hide_parent_rows_button.Size = new Size (caption_area.Height - 2, caption_area.Height - 2);
					hide_parent_rows_button.Click += new EventHandler (HideParentRowsClicked);
				}

				Controls.Add (navigate_back_button);
				Controls.Add (hide_parent_rows_button);
			}
			else {
				SuspendLayout ();
				if (navigate_back_button != null) {
					navigate_back_button.Dispose ();
					navigate_back_button = null;
				}
				if (hide_parent_rows_button != null) {
					hide_parent_rows_button.Dispose ();
					hide_parent_rows_button = null;
				}
				ResumeLayout (false);
			}

		}

		void CalcRowHeaders ()
		{
			rowhdrs_area.X = ClientRectangle.X;
			rowhdrs_area.Y = columnhdrs_area.Y + columnhdrs_area.Height;
			rowhdrs_area.Height = ClientRectangle.Height + ClientRectangle.Y - rowhdrs_area.Y;

			if (CurrentTableStyle.CurrentRowHeadersVisible)
				rowhdrs_area.Width = RowHeaderWidth;
			else
				rowhdrs_area.Width = 0;
		}

		int GetVisibleRowCount (int visibleHeight)
		{
			int rows_height = 0;
			int r;
			for (r = FirstVisibleRow; r < RowsCount; r ++) {
				if (rows_height + rows[r].Height >= visibleHeight)
					break;
				rows_height += rows[r].Height;
			}

			/* add in the edit row if it'll fit */
			if (ShowEditRow && RowsCount > 0 && visibleHeight - rows_height > RowHeight)
				r ++;

			if (r < rows.Length - 1)
				r ++;

			return r - FirstVisibleRow;
		}

		void UpdateVisibleColumn ()
		{
			if (CurrentTableStyle.GridColumnStyles.Count == 0) {
				visiblecolumn_count = 0;
				return;	
			}
			
			int col;
			int max_pixel = horiz_pixeloffset + cells_area.Width;
			int unused;

			first_visiblecolumn = FromPixelToColumn (horiz_pixeloffset, out unused);

			col = FromPixelToColumn (max_pixel, out unused);
			
			visiblecolumn_count = 1 + col - first_visiblecolumn;
			
			if (first_visiblecolumn + visiblecolumn_count < CurrentTableStyle.GridColumnStyles.Count) { 
				visiblecolumn_count++; // Partially visible column
			}
		}

		void UpdateVisibleRowCount ()
		{
			visiblerow_count = GetVisibleRowCount (cells_area.Height);

			CalcRowHeaders (); // Height depends on num of visible rows

			// XXX
			Invalidate ();
		}


		void InvalidateCaption ()
		{
			if (caption_area.IsEmpty)
				return;

			Invalidate (caption_area);
		}

		void InvalidateRow (int row)
		{
			if (row < FirstVisibleRow || row > FirstVisibleRow + VisibleRowCount)
				return;

			Rectangle rect_row = new Rectangle ();

			rect_row.X = cells_area.X;
			rect_row.Width = width_of_all_columns;
			if (rect_row.Width > cells_area.Width)
				rect_row.Width = cells_area.Width;
			rect_row.Height = rows[row].Height;
			rect_row.Y = cells_area.Y + rows[row].VerticalOffset - rows[FirstVisibleRow].VerticalOffset;
			Invalidate (rect_row);
		}

		void InvalidateRowHeader (int row)
		{
			Rectangle rect_rowhdr = new Rectangle ();
			rect_rowhdr.X = rowhdrs_area.X;
			rect_rowhdr.Width = rowhdrs_area.Width;
			rect_rowhdr.Height = rows[row].Height;
			rect_rowhdr.Y = rowhdrs_area.Y + rows[row].VerticalOffset - rows[FirstVisibleRow].VerticalOffset;
			Invalidate (rect_rowhdr);
		}	

		internal void InvalidateColumn (DataGridColumnStyle column)
		{
			Rectangle rect_col = new Rectangle ();
			int col_pixel;
			int col = -1;

			col = CurrentTableStyle.GridColumnStyles.IndexOf (column);

			if (col == -1)
				return;

			rect_col.Width = column.Width;
			col_pixel = GetColumnStartingPixel (col);
			rect_col.X = cells_area.X + col_pixel - horiz_pixeloffset;
			rect_col.Y = cells_area.Y;
			rect_col.Height = cells_area.Height;
			Invalidate (rect_col);
		}

		void DrawResizeLineVert (int x)
		{
			XplatUI.DrawReversibleRectangle (Handle,
							 new Rectangle (x, cells_area.Y, 1, cells_area.Height - 3),
							 2);
		}

		void DrawResizeLineHoriz (int y)
		{
			XplatUI.DrawReversibleRectangle (Handle,
							 new Rectangle (cells_area.X, y, cells_area.Width - 3, 1),
							 2);
		}

		void SetUpHorizontalScrollBar (out int maximum)
		{
			maximum = width_of_all_columns;

			horiz_scrollbar.Location = new Point (ClientRectangle.X, ClientRectangle.Y +
				ClientRectangle.Height - horiz_scrollbar.Height);

			horiz_scrollbar.Size = new Size (ClientRectangle.Width,
				horiz_scrollbar.Height);

			horiz_scrollbar.LargeChange = cells_area.Width;
		}


		void SetUpVerticalScrollBar (out int height, out int maximum)
		{
			int y;
			
			if (caption_visible) {
				y = ClientRectangle.Y + caption_area.Height;
				height = ClientRectangle.Height - caption_area.Height;
			} else {
				y = ClientRectangle.Y;
				height = ClientRectangle.Height;
			}

			vert_scrollbar.Location = new Point (ClientRectangle.X +
							     ClientRectangle.Width - vert_scrollbar.Width, y);

			vert_scrollbar.Size = new Size (vert_scrollbar.Width,
							height);

			maximum = RowsCount;
			
			if (ShowEditRow && RowsCount > 0) {
				maximum++;	
			}
			
			vert_scrollbar.LargeChange = VLargeChange;
		}

		#endregion // Public Instance Methods

		#region Instance Properties
		// Returns the ColumnHeaders area excluding the rectangle shared with RowHeaders
		internal Rectangle ColumnHeadersArea {
			get {
				Rectangle columns_area = columnhdrs_area;

				if (CurrentTableStyle.CurrentRowHeadersVisible) {
					columns_area.X += RowHeaderWidth;
					columns_area.Width -= RowHeaderWidth;
				}
				return columns_area;
			}
		}

		bool ShowingColumnHeaders {
			get { return ColumnHeadersVisible && CurrentTableStyle.GridColumnStyles.Count > 0; }
		}

		internal Rectangle RowHeadersArea {
			get { return rowhdrs_area; }
		}

		internal Rectangle ParentRowsArea {
			get { return parent_rows; }
		}

		int VLargeChange {
			get { return VisibleRowCount; }
		}

		#endregion Instance Properties

		#endregion // Code originally in DataGridDrawingLogic.cs
	}
}
