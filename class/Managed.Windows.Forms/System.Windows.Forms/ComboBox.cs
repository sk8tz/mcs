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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Jordi Mas i Hernandez, jordi@ximian.com
//
//

// NOT COMPLETE

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace System.Windows.Forms
{

	public class ComboBox : ListControl
	{
		private DrawMode draw_mode;
		private ComboBoxStyle dropdown_style;
		private int dropdown_width;
		private int max_length;
		private int preferred_height;
		private int selected_index;
		private object selected_item;
		internal ObjectCollection items;
		private bool suspend_ctrlupdate;
		private int maxdrop_items;
		private bool integral_height;
		private bool sorted;
		internal ComboBoxInfo combobox_info;
		private readonly int def_button_width = 16;
		private bool clicked;
		private ComboListBox listbox_ctrl;
		private StringFormat string_format;
		private TextBox textbox_ctrl;

		internal class ComboBoxInfo
		{
			internal int item_height; 		/* Item's height */
			internal Rectangle textarea;		/* Rectangle of the editable text area  */
			internal Rectangle textarea_drawable;	/* Rectangle of the editable text area - decorations */
			internal Rectangle button_rect;
			internal bool show_button;		/* Is the DropDown button shown? */
			internal ButtonState button_status;	/* Drop button status */
			internal Size listbox_size;
			internal Rectangle listbox_area;	/* ListBox area in Simple combox, not used in the rest */

			public ComboBoxInfo ()
			{
				button_status = ButtonState.Normal;
				show_button = false;
				item_height = 0;
			}
		}

		internal class ComboBoxItem
		{
			internal int Index;

			public ComboBoxItem (int index)
			{
				Index = index;
			}
		}

		public ComboBox ()
		{
			listbox_ctrl = null;
			textbox_ctrl = null;
			combobox_info = new ComboBoxInfo ();			
			DropDownStyle = ComboBoxStyle.DropDown;
			BackColor = ThemeEngine.Current.ColorWindow;
			draw_mode = DrawMode.Normal;
			selected_index = -1;
			selected_item = null;
			maxdrop_items = 8;			
			combobox_info.item_height = FontHeight + 2;
			suspend_ctrlupdate = false;
			clicked = false;			

			items = new ObjectCollection (this);
			string_format = new StringFormat ();			
			

			/* Events */
			MouseDown += new MouseEventHandler (OnMouseDownCB);
			MouseUp += new MouseEventHandler (OnMouseUpCB);			
		}

		#region Events
		public new event EventHandler BackgroundImageChanged;
		public event DrawItemEventHandler DrawItem;
		public event EventHandler DropDown;
		public event EventHandler DropDownStyleChanged;
		public event MeasureItemEventHandler MeasureItem;
		public new event PaintEventHandler Paint;
		public event EventHandler SelectedIndexChanged;
		public event EventHandler SelectionChangeCommitted;
		#endregion Events

		#region Public Properties
		public override Color BackColor {
			get { return base.BackColor; }
			set {
				if (base.BackColor == value)
					return;

    				base.BackColor = value;
				Refresh ();
			}
		}

		public override Image BackgroundImage {
			get { return base.BackgroundImage; }
			set {
				if (base.BackgroundImage == value)
					return;

    				base.BackgroundImage = value;

    				if (BackgroundImageChanged != null)
					BackgroundImageChanged (this, EventArgs.Empty);

				Refresh ();
			}
		}

		protected override CreateParams CreateParams {
			get { return base.CreateParams;}
		}

		protected override Size DefaultSize {
			get { return new Size (121, PreferredHeight); }
		}

		public DrawMode DrawMode {
			get { return draw_mode; }

    			set {
				if (!Enum.IsDefined (typeof (DrawMode), value))
					throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for DrawMode", value));

				if (draw_mode == value)
					return;

    				draw_mode = value;
				Refresh ();
    			}
		}

		public ComboBoxStyle DropDownStyle {
			get { return dropdown_style; }

    			set {
				if (!Enum.IsDefined (typeof (ComboBoxStyle), value))
					throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for ComboBoxStyle", value));

				if (dropdown_style == value)
					return;					
									
				if (dropdown_style != ComboBoxStyle.DropDownList) {					
					if (textbox_ctrl != null) {
						Controls.Remove (textbox_ctrl);
						textbox_ctrl.Dispose ();
						textbox_ctrl = null;
					}
				}
				
				dropdown_style = value;					
				
				if (dropdown_style == ComboBoxStyle.Simple) {
					CBoxInfo.show_button = false;					
					CreateComboListBox ();
					Controls.Add (listbox_ctrl);    					
				}
				else {
					CBoxInfo.show_button = true;
					if (listbox_ctrl != null) {
						listbox_ctrl.Dispose ();
						Controls.Remove (listbox_ctrl);
						listbox_ctrl = null;
					}
				}				
				
				if (dropdown_style != ComboBoxStyle.DropDownList) {
					textbox_ctrl = new TextBox ();
					Controls.Add (textbox_ctrl);					
					CalcTextArea ();					
				}			
				
				
				if (DropDownStyleChanged  != null)
					DropDownStyleChanged (this, EventArgs.Empty);
    				
				Refresh ();
    			}
		}

		public int DropDownWidth {
			get { return dropdown_width; }
			set {

				if (dropdown_width == value)
					return;

    				dropdown_width = value;
				Refresh ();
			}
		}

		public override bool Focused {
			get { return base.Focused; }
		}

		public override Color ForeColor {
			get { return base.ForeColor; }
			set {

				if (base.ForeColor == value)
					return;

    				base.ForeColor = value;
				Refresh ();
			}
		}

		public bool IntegralHeight {
			get { return integral_height; }
			set {
				if (integral_height == value)
					return;

    				integral_height = value;
    				Refresh ();
			}
		}

		public virtual int ItemHeight {
			get { return combobox_info.item_height; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("The item height value is less than zero");

				combobox_info.item_height = value;
				Refresh ();
			}
		}


		public ComboBox.ObjectCollection Items {
			get { return items; }
		}

		public int MaxDropDownItems {
			get { return maxdrop_items; }
			set {
				if (maxdrop_items == value)
					return;

    				maxdrop_items = value;
			}
		}

		public int MaxLength {
			get { return max_length; }
			set {
				if (max_length == value)
					return;

    				max_length = value;
			}
		}

		public int PreferredHeight {
			get { return preferred_height; }
		}

		public override int SelectedIndex {
			get { return selected_index; }
			set {
				if (value < -2 || value >= Items.Count)
					throw new ArgumentOutOfRangeException ("Index of out range");

				if (selected_index == value)
					return;

    				selected_index = value;
    				
    				if (dropdown_style != ComboBoxStyle.DropDownList) {
    					textbox_ctrl.Text = Items[selected_index].ToString ();
    				}
    				
    				OnSelectedIndexChanged  (new EventArgs ());
    				Refresh ();
			}
		}

		public object SelectedItem {
			get {
				if (selected_index !=-1 && Items.Count > 0)
					return Items[selected_index];
				else
					return null;
				}
			set {
				if (selected_item == value)
					return;

    				int index = Items.IndexOf (value);

				if (index == -1)
					return;

				selected_index = index;
				
				if (dropdown_style != ComboBoxStyle.DropDownList) {
    					textbox_ctrl.Text = Items[selected_index].ToString ();
    				}
				
				OnSelectedItemChanged  (new EventArgs ());
				Refresh ();
			}
		}
		
		public string SelectedText {
			get {
				if (dropdown_style == ComboBoxStyle.DropDownList)
					return "";
					
				return textbox_ctrl.Text;
			}
			set {
				if (dropdown_style == ComboBoxStyle.DropDownList)
					return;
				
				if (textbox_ctrl.Text == value)
					return;
					
				textbox_ctrl.SelectedText = value;
			}
		}

		public int SelectionLength {
			get {
				if (dropdown_style == ComboBoxStyle.DropDownList) 
					return 0;
				
				return textbox_ctrl.SelectionLength;
			}
			set {
				if (dropdown_style == ComboBoxStyle.DropDownList) 
					return;
					
				if (textbox_ctrl.SelectionLength == value)
					return;
					
				textbox_ctrl.SelectionLength = value;
			}
		}

		public int SelectionStart {
			get { 
				if (dropdown_style == ComboBoxStyle.DropDownList) 
					return 0; 					
				
				return textbox_ctrl.SelectionStart;				
			}
			set {
				if (dropdown_style == ComboBoxStyle.DropDownList) 
					return;
				
				if (textbox_ctrl.SelectionStart == value)
					return;					
				
				textbox_ctrl.SelectionStart = value;
			}
		}

		public bool Sorted {
			get { return sorted; }

    			set {
				if (sorted == value)
					return;

    				sorted = value;
    			}
    		}

		public override string Text {
			get { return ""; /*throw new NotImplementedException ();*/ }
			set {}
		}

		#endregion Public Properties

		#region Private Properties
		internal ComboBoxInfo CBoxInfo {
			get { return combobox_info; }
		}

		#endregion Private Properties

		#region Public Methods
		protected virtual void AddItemsCore (object[] value)
		{

		}

		public void BeginUpdate ()
		{
			suspend_ctrlupdate = true;
		}

		protected virtual void Dispose (bool disposing)
		{

		}

		public void EndUpdate ()
		{
			suspend_ctrlupdate = false;
			Refresh ();
		}

		public int FindString (string s)
		{
			return FindString (s, 0);
		}

		public int FindString (string s, int startIndex)
		{
			for (int i = startIndex; i < Items.Count; i++) {
				if ((Items[i].ToString ()).StartsWith (s))
					return i;
			}

			return -1;
		}

		public int FindStringExact (string s)
		{
			return FindStringExact (s, 0);
		}

		public int FindStringExact (string s, int startIndex)
		{
			for (int i = startIndex; i < Items.Count; i++) {
				if ((Items[i].ToString ()).Equals (s))
					return i;
			}

			return -1;
		}

		public int GetItemHeight (int index)
		{
			throw new NotImplementedException ();
		}

		protected override bool IsInputKey (Keys keyData)
		{
			return false;
		}

		protected override void OnBackColorChanged (EventArgs e)
		{
			base.OnBackColorChanged (e);
		}

		protected override void OnDataSourceChanged (EventArgs e)
		{
			base.OnDataSourceChanged (e);
		}

		protected override void OnDisplayMemberChanged (EventArgs e)
		{
			base.OnDisplayMemberChanged (e);
		}

		protected virtual void OnDrawItem (DrawItemEventArgs e)
		{
			if (DrawItem != null && (DrawMode == DrawMode.OwnerDrawFixed || DrawMode == DrawMode.OwnerDrawVariable)) {				
				DrawItem (this, e);
				return;
			}
			
			Rectangle text_draw = e.Bounds;
			
			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected) {

				e.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush
					(ThemeEngine.Current.ColorHilight), text_draw);

				e.Graphics.DrawString (Items[e.Index].ToString (), e.Font,
					ThemeEngine.Current.ResPool.GetSolidBrush (ThemeEngine.Current.ColorHilightText),
					text_draw, string_format);

				// It seems to be a bug in CPDrawFocusRectangle
				//ThemeEngine.Current.CPDrawFocusRectangle (e.Graphics, e.Bounds,
				//	ThemeEngine.Current.ColorHilightText, BackColor);
			}
			else {
				e.Graphics.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush
					(e.BackColor), e.Bounds);

				e.Graphics.DrawString (Items[e.Index].ToString (), e.Font,
					ThemeEngine.Current.ResPool.GetSolidBrush (e.ForeColor),
					text_draw, string_format);
			}
		}

		protected virtual void OnDropDownStyleChanged (EventArgs e)
		{

		}

		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
			
			if (textbox_ctrl != null) {
				textbox_ctrl.Font = Font;
			}
			
			combobox_info.item_height = FontHeight + 2;
		}

		protected override void OnForeColorChanged (EventArgs e)
		{
			base.OnForeColorChanged (e);
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);
			CalcTextArea ();			
		}

		protected override void OnHandleDestroyed (EventArgs e)
		{
			base.OnHandleDestroyed (e);
		}

		protected override void OnKeyPress (KeyPressEventArgs e)
		{

		}

		protected virtual void OnMeasureItem (MeasureItemEventArgs e)
		{

		}

		protected override void OnParentBackColorChanged (EventArgs e)
		{
			base.OnParentBackColorChanged (e);
		}

		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);			
			CalcTextArea ();			
		}

		protected override void OnSelectedIndexChanged (EventArgs e)
		{
			base.OnSelectedIndexChanged (e);

			if (SelectedIndexChanged != null)
				SelectedIndexChanged (this, e);
		}

		protected virtual void OnSelectedItemChanged (EventArgs e)
		{

		}

		protected override void OnSelectedValueChanged (EventArgs e)
		{
			base.OnSelectedValueChanged (e);
		}

		protected virtual void OnSelectionChangeCommitted (EventArgs e)
		{

		}

		protected override void RefreshItem (int index)
		{

		}


		protected virtual void Select (int start, int lenght)
		{

		}

		public void SelectAll ()
		{

		}

		protected override void SetBoundsCore (int x, int y, int width, int height, BoundsSpecified specified)
		{
			base.SetBoundsCore (x, y, width, height, specified);
		}

		protected override void SetItemCore (int index, object value)
		{
			if (index < 0 || index >= Items.Count)
				return;

			Items[index] = value;
		}

		protected override void SetItemsCore (IList value)
		{

		}

		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		protected override void WndProc (ref Message m)
		{

			switch ((Msg) m.Msg) {

			case Msg.WM_PAINT: {
				PaintEventArgs	paint_event;
				paint_event = XplatUI.PaintEventStart (Handle);
				OnPaintCB (paint_event);
				XplatUI.PaintEventEnd (Handle);
				return;
			}

			case Msg.WM_ERASEBKGND:
				m.Result = (IntPtr) 1;
				return;

			default:
				break;
			}

			base.WndProc (ref m);

		}

		#endregion Public Methods

		#region Private Methods

		internal void ButtonReleased ()
		{
			combobox_info.button_status = ButtonState.Normal;
			Invalidate (combobox_info.button_rect);
		}

		// Calcs the text area size
		internal void CalcTextArea ()
		{	
			combobox_info.textarea = ClientRectangle;
					
			/* Edit area */
			combobox_info.textarea.Height = ItemHeight + ThemeEngine.Current.DrawComboBoxEditDecorationTop () +
					ThemeEngine.Current.DrawComboBoxEditDecorationBottom () + 2;
					// TODO: Does the +2 changes a different font resolutions?
			
			/* Edit area - minus decorations (text drawable area) */
			combobox_info.textarea_drawable = combobox_info.textarea;
			combobox_info.textarea_drawable.Y += ThemeEngine.Current.DrawComboBoxEditDecorationTop ();
			combobox_info.textarea_drawable.X += ThemeEngine.Current.DrawComboBoxEditDecorationLeft ();
			combobox_info.textarea_drawable.Height -= ThemeEngine.Current.DrawComboBoxEditDecorationBottom ();
			combobox_info.textarea_drawable.Height -= ThemeEngine.Current.DrawComboBoxEditDecorationTop();
			combobox_info.textarea_drawable.Width -= ThemeEngine.Current.DrawComboBoxEditDecorationRight ();
			combobox_info.textarea_drawable.Width -= ThemeEngine.Current.DrawComboBoxEditDecorationLeft ();
			
			/* Non-drawable area */
			Region area = new Region (ClientRectangle);
			area.Exclude (combobox_info.textarea);
			RectangleF bounds = area.GetBounds (DeviceContext);
			combobox_info.listbox_area = new Rectangle ((int)bounds.X, (int)bounds.Y, 
				(int)bounds.Width, (int)bounds.Height);				
			
			if (CBoxInfo.show_button) {
				combobox_info.textarea_drawable.Width -= def_button_width;

				combobox_info.button_rect = new Rectangle (combobox_info.textarea_drawable.X + combobox_info.textarea_drawable.Width,
					combobox_info.textarea_drawable.Y, 	def_button_width, combobox_info.textarea_drawable.Height);
			}
			
			if (dropdown_style != ComboBoxStyle.DropDownList) { /* There is an edit control*/
				if (textbox_ctrl != null) {
					textbox_ctrl.Location = new Point (combobox_info.textarea_drawable.X, combobox_info.textarea_drawable.Y);
					textbox_ctrl.Size = new Size (combobox_info.textarea_drawable.Width, combobox_info.textarea_drawable.Height);
				}
			}
			
			if (listbox_ctrl != null && dropdown_style == ComboBoxStyle.Simple) {
				listbox_ctrl.Location = new Point (combobox_info.textarea.X, combobox_info.textarea.Y +
					combobox_info.textarea.Height);
				listbox_ctrl.CalcListBoxArea ();
			}
		}

		private void CreateComboListBox ()
		{			
			listbox_ctrl = new ComboListBox (this);
			//listbox_ctrl.Size = combobox_info.listbox_size;
			listbox_ctrl.Size = new Size (100, 100);			
			
		}
		
		internal void Draw (Rectangle clip)
		{					
			// No edit control, we paint the edit ourselfs
			if (dropdown_style == ComboBoxStyle.DropDownList) {

				if (selected_index != -1) {
					
					Rectangle item_rect = combobox_info.textarea_drawable;
					item_rect.Height = ItemHeight;
					
					OnDrawItem (new DrawItemEventArgs (DeviceContext, Font, item_rect,
								selected_index, DrawItemState.Selected,
								ForeColor, BackColor));				
				}
				else
					DeviceContext.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (BackColor), 
						combobox_info.textarea_drawable);
			}			
			
			if (clip.IntersectsWith (combobox_info.listbox_area) == true) {
				DeviceContext.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (Parent.BackColor), 
						combobox_info.listbox_area);
			}
			
			if (CBoxInfo.show_button) {
				DeviceContext.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush (ThemeEngine.Current.ColorButtonFace),
					combobox_info.button_rect);

				ThemeEngine.Current.CPDrawComboButton (DeviceContext,
					combobox_info.button_rect, combobox_info.button_status);				
			}			
			
			ThemeEngine.Current.DrawComboBoxEditDecorations (DeviceContext, this, combobox_info.textarea);
		}

		internal virtual void OnMouseDownCB (object sender, MouseEventArgs e)
    		{
    			/* Click On button*/
    			if (clicked == false && combobox_info.button_rect.Contains (e.X, e.Y)) {

    				clicked = true;

    				if (combobox_info.button_status == ButtonState.Normal) {
    						combobox_info.button_status = ButtonState.Pushed;
    				}
					else {
    					if (combobox_info.button_status == ButtonState.Pushed) {
    						combobox_info.button_status = ButtonState.Normal;
    					}
    				}

    				if (combobox_info.button_status == ButtonState.Pushed) {
    					if (listbox_ctrl == null) {
    						CreateComboListBox ();
    					}

					listbox_ctrl.Location = PointToScreen (new Point (combobox_info.textarea.X, combobox_info.textarea.Y +
						combobox_info.textarea.Height));						
    					listbox_ctrl.ShowWindow ();
    				}

    				Invalidate (combobox_info.button_rect);
    			}
    		}

    		internal virtual void OnMouseUpCB (object sender, MouseEventArgs e)
    		{
    			/* Click on button*/
    			if (clicked == true && combobox_info.button_rect.Contains (e.X, e.Y)) {

    				clicked = false;
    			}

    		}

		private void OnPaintCB (PaintEventArgs pevent)
		{
			if (Width <= 0 || Height <=  0 || Visible == false || suspend_ctrlupdate == true)
    				return;
    				
    			Rectangle rect = ClientRectangle;

			/* Copies memory drawing buffer to screen*/
			Draw (rect);			
			pevent.Graphics.DrawImage (ImageBuffer, rect, rect, GraphicsUnit.Pixel);

			if (Paint != null)
				Paint (this, pevent);
		}

		#endregion Private Methods


		/*
			ComboBox.ObjectCollection
		*/
		public class ObjectCollection : IList, ICollection, IEnumerable
		{

			private ComboBox owner;
			internal ArrayList object_items = new ArrayList ();
			internal ArrayList listbox_items = new ArrayList ();

			public ObjectCollection (ComboBox owner)
			{
				this.owner = owner;
			}


			#region Public Properties
			public virtual int Count {
				get { return object_items.Count; }
			}

			public virtual bool IsReadOnly {
				get { return false; }
			}

			public virtual object this [int index] {
				get {
					if (index < 0 || index >= Count)
						throw new ArgumentOutOfRangeException ("Index of out range");

					return object_items[index];
				}
				set {
					if (index < 0 || index >= Count)
						throw new ArgumentOutOfRangeException ("Index of out range");

					object_items[index] = value;
				}
			}

			bool ICollection.IsSynchronized {
				get { return false; }
			}

			object ICollection.SyncRoot {
				get { return this; }
			}

			bool IList.IsFixedSize {
				get { return false; }
			}

			#endregion Public Properties

			#region Public Methods
			public int Add (object item)
			{
				int idx;

				idx = AddItem (item);
				return idx;
			}

			public void AddRange (object[] items)
			{
				foreach (object mi in items)
					AddItem (mi);
			}


			public virtual void Clear ()
			{
				object_items.Clear ();
				listbox_items.Clear ();

			}
			public virtual bool Contains (object obj)
			{
				return object_items.Contains (obj);
			}

			public void CopyTo (object[] dest, int arrayIndex)
			{
				object_items.CopyTo (dest, arrayIndex);
			}

			void ICollection.CopyTo (Array dest, int index)
			{
				object_items.CopyTo (dest, index);
			}

			public virtual IEnumerator GetEnumerator ()
			{
				return object_items.GetEnumerator ();
			}

			int IList.Add (object item)
			{
				return Add (item);
			}

			public virtual int IndexOf (object value)
			{
				return object_items.IndexOf (value);
			}

			public virtual void Insert (int index,  object item)
			{
				throw new NotImplementedException ();
			}

			public virtual void Remove (object value)
			{
				RemoveAt (IndexOf (value));

			}

			public virtual void RemoveAt (int index)
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException ("Index of out range");

				object_items.RemoveAt (index);
				listbox_items.RemoveAt (index);
				//owner.UpdateItemInfo (false, -1, -1);
			}
			#endregion Public Methods

			#region Private Methods
			private int AddItem (object item)
			{
				int cnt = object_items.Count;
				object_items.Add (item);
				listbox_items.Add (new ComboBox.ComboBoxItem (cnt));
				return cnt;
			}

			internal ComboBox.ComboBoxItem GetComboBoxItem (int index)
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException ("Index of out range");

				return (ComboBox.ComboBoxItem) listbox_items[index];
			}

			internal void SetComboBoxItem (ComboBox.ComboBoxItem item, int index)
			{
				if (index < 0 || index >= Count)
					throw new ArgumentOutOfRangeException ("Index of out range");

				listbox_items[index] = item;
			}

			#endregion Private Methods
		}

		/*
			class ComboListBox
		*/
		internal class ComboListBox : Control
		{
			private ComboBox owner;
			private bool need_vscrollbar;
			private VScrollBar vscrollbar_ctrl;
			private int top_item;			/* First item that we show the in the current page */
			private int last_item;			/* Last visible item */
			private int highlighted_item;		/* Item that is currently selected */
			internal int page_size;			/* Number of listbox items per page */
			private Rectangle textarea_drawable;	/* Rectangle of the drawable text area */
			
			internal enum ItemNavigation
			{
				First,
				Last,
				Next,
				Previous,
				NextPage,
				PreviousPage,
			}

			public ComboListBox (ComboBox owner) : base ()
			{	
				this.owner = owner;				
				need_vscrollbar = false;
				top_item = 0;
				last_item = 0;
				page_size = 0;
				highlighted_item = -1;

				MouseDown += new MouseEventHandler (OnMouseDownPUW);
				MouseMove += new MouseEventHandler (OnMouseMovePUW);
				MouseUp += new MouseEventHandler (OnMouseUpPUW);
				KeyDown += new KeyEventHandler (OnKeyDownPUW);
				Paint += new PaintEventHandler (OnPaintPUW);
				SetStyle (ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
				SetStyle (ControlStyles.ResizeRedraw | ControlStyles.Opaque, true);

				/* Vertical scrollbar */
				vscrollbar_ctrl = new VScrollBar ();
				vscrollbar_ctrl.Minimum = 0;
				vscrollbar_ctrl.SmallChange = 1;
				vscrollbar_ctrl.LargeChange = 1;
				vscrollbar_ctrl.Maximum = 0;
				vscrollbar_ctrl.ValueChanged += new EventHandler (VerticalScrollEvent);
				vscrollbar_ctrl.Visible = false;				
			}

			protected override CreateParams CreateParams
			{
				get {
					CreateParams cp = base.CreateParams;					
					if (owner != null && owner.DropDownStyle != ComboBoxStyle.Simple) {
						cp.Style = unchecked ((int)(WindowStyles.WS_POPUP | WindowStyles.WS_VISIBLE | WindowStyles.WS_CLIPSIBLINGS | WindowStyles.WS_CLIPCHILDREN));
						cp.ExStyle |= (int)WindowStyles.WS_EX_TOOLWINDOW;
					}					
					return cp;
				}
			}

			#region Private Methods

			protected override void CreateHandle ()
			{			
				base.CreateHandle ();
				Controls.Add (vscrollbar_ctrl);
			}

			// Calcs the listbox area
			internal void CalcListBoxArea ()
			{				
				int width, height;
				
				if (owner.DropDownStyle == ComboBoxStyle.Simple) {
					width = owner.CBoxInfo.listbox_area.Width;
					height = owner.CBoxInfo.listbox_area.Height;

					if (owner.IntegralHeight == true) {
						int remaining = (height - 2 -
							ThemeEngine.Current.DrawComboListBoxDecorationBottom (owner.DropDownStyle) -
							ThemeEngine.Current.DrawComboListBoxDecorationTop (owner.DropDownStyle)) %
							(owner.ItemHeight - 2);
							
						Console.WriteLine("CalcListBoxArea h:{0} remaining {1}, itemh {2}",
							height - 2 - 
							ThemeEngine.Current.DrawComboListBoxDecorationBottom (owner.DropDownStyle)-
							ThemeEngine.Current.DrawComboListBoxDecorationTop (owner.DropDownStyle),
							remaining, (owner.ItemHeight - 2));
		
						if (remaining > 0) {
							height -= remaining;							
						}
					}
				}
				else {

					width = owner.ClientRectangle.Width;
	
					if (owner.Items.Count <= owner.MaxDropDownItems) {
						height = (owner.ItemHeight - 2) * owner.Items.Count;						
					}
					else {
						height = (owner.ItemHeight - 2) * owner.MaxDropDownItems;						
					}
					
					height += ThemeEngine.Current.DrawComboListBoxDecorationBottom (owner.DropDownStyle);				
					height += ThemeEngine.Current.DrawComboListBoxDecorationTop (owner.DropDownStyle);

				}
				
				if (owner.Items.Count <= owner.MaxDropDownItems) {
					need_vscrollbar = false;
				}
				else {
					need_vscrollbar = true;
					vscrollbar_ctrl.Height = height - ThemeEngine.Current.DrawComboListBoxDecorationBottom (owner.DropDownStyle) -
						ThemeEngine.Current.DrawComboListBoxDecorationTop (owner.DropDownStyle);
						
					vscrollbar_ctrl.Location = new Point (width - vscrollbar_ctrl.Width - ThemeEngine.Current.DrawComboListBoxDecorationRight (owner.DropDownStyle), 
						ThemeEngine.Current.DrawComboListBoxDecorationTop (owner.DropDownStyle));					
	
					vscrollbar_ctrl.Maximum = owner.Items.Count - owner.MaxDropDownItems;
				}

				if (vscrollbar_ctrl.Visible != need_vscrollbar)
					vscrollbar_ctrl.Visible = need_vscrollbar;

				Size = new Size (width, height);
				textarea_drawable = ClientRectangle;
				textarea_drawable.Width = width;
				textarea_drawable.Height = height;				

				// Exclude decorations
				textarea_drawable.X += ThemeEngine.Current.DrawComboListBoxDecorationLeft (owner.DropDownStyle);
				textarea_drawable.Y += ThemeEngine.Current.DrawComboListBoxDecorationTop (owner.DropDownStyle);
				textarea_drawable.Width -= ThemeEngine.Current.DrawComboListBoxDecorationRight (owner.DropDownStyle);
				textarea_drawable.Width -= ThemeEngine.Current.DrawComboListBoxDecorationLeft (owner.DropDownStyle);
				textarea_drawable.Height -= ThemeEngine.Current.DrawComboListBoxDecorationBottom (owner.DropDownStyle);				
				textarea_drawable.Height -= ThemeEngine.Current.DrawComboListBoxDecorationTop (owner.DropDownStyle);
				
				if (need_vscrollbar)
					textarea_drawable.Width -= vscrollbar_ctrl.Width;

				last_item = LastVisibleItem ();				
				page_size = textarea_drawable.Height / (owner.ItemHeight - 2);
				
				Console.WriteLine ("ComboListBox.CalcListBoxArea {0} page_size {1}, dh: {2}, itemh {3}", textarea_drawable,
					page_size, textarea_drawable.Height, (owner.ItemHeight - 2));
			}			

			private void Draw (Rectangle clip)
			{	
				Rectangle cl = ClientRectangle;				
				
				DeviceContext.FillRectangle (ThemeEngine.Current.ResPool.GetSolidBrush
					(owner.BackColor), ClientRectangle);

				if (owner.Items.Count > 0) {
					Rectangle item_rect;
					DrawItemState state = DrawItemState.None;

					for (int i = top_item; i <= last_item; i++) {
						item_rect = GetItemDisplayRectangle (i, top_item);

						if (clip.IntersectsWith (item_rect) == false)
							continue;

						/* Draw item */
						state = DrawItemState.None;

						if (i == highlighted_item)
							state |= DrawItemState.Selected;

						owner.OnDrawItem (new DrawItemEventArgs (DeviceContext, owner.Font, item_rect,
							i, state, owner.ForeColor, owner.BackColor));
					}
				}			
				
				ThemeEngine.Current.DrawComboListBoxDecorations (DeviceContext, owner, ClientRectangle);
			}

			private Rectangle GetItemDisplayRectangle (int index, int first_displayble)
			{
				if (index < 0 || index >= owner.Items.Count)
					throw new  ArgumentOutOfRangeException ("GetItemRectangle index out of range.");

				Rectangle item_rect = new Rectangle ();

				item_rect.X = 0;
				item_rect.Y = 2 + ((owner.ItemHeight - 2) * (index - first_displayble));
				item_rect.Height = owner.ItemHeight;
				item_rect.Width = textarea_drawable.Width;

				return item_rect;
			}

			public void HideWindow ()
			{
				if (owner.DropDownStyle == ComboBoxStyle.Simple)
					return;
					
				owner.ButtonReleased ();
				Hide ();
			}

			private int IndexFromPointDisplayRectangle (int x, int y)
			{
	    			for (int i = top_item; i <= last_item; i++) {
					if (GetItemDisplayRectangle (i, top_item).Contains (x, y) == true)
						return i;
				}

				return -1;
			}

			private int LastVisibleItem ()
			{
				Rectangle item_rect;
				int top_y = textarea_drawable.Y + textarea_drawable.Height;
				int i = 0;

				for (i = top_item; i < owner.Items.Count; i++) {
					item_rect = GetItemDisplayRectangle (i, top_item);				
					if (item_rect.Y + item_rect.Height > top_y) {
						return i;
					}
				}
				return i;
			}
			
			private void NavigateItem (ItemNavigation navigation)
			{
				switch (navigation) {
				case ItemNavigation.Next: {
					if (highlighted_item + 1 < owner.Items.Count) {
						
						if (highlighted_item + 1 > last_item) {
							top_item++;
							vscrollbar_ctrl.Value = top_item;
						}
						SetHighLightedItem (highlighted_item + 1);
					}
					break;
				}
				
				case ItemNavigation.Previous: {
					if (highlighted_item > 0) {						
						
						if (highlighted_item - 1 < top_item) {							
							top_item--;
							vscrollbar_ctrl.Value = top_item;							
						}
						SetHighLightedItem (highlighted_item - 1);
					}					
					break;
				}
				
				case ItemNavigation.NextPage: {
					if (highlighted_item + page_size - 1 > owner.Items.Count) {
						top_item = owner.Items.Count - page_size;
						vscrollbar_ctrl.Value = top_item; 						
						SetHighLightedItem (owner.Items.Count - 1);						
					}
					else {
						if (highlighted_item + page_size - 1  > last_item) {
							top_item = highlighted_item;
							vscrollbar_ctrl.Value = highlighted_item;
						}
					
						SetHighLightedItem (highlighted_item + page_size - 1);
					}
					
					break;
				}
				
				case ItemNavigation.PreviousPage: {					
					
					/* Go to the first item*/
					if (highlighted_item - (page_size - 1) <= 0) {
																		
						top_item = 0;
						vscrollbar_ctrl.Value = top_item;
						SetHighLightedItem (0);
						
					
					}
					else { /* One page back */
						if (highlighted_item - (page_size - 1)  < top_item) {
							top_item = highlighted_item - (page_size - 1);
							vscrollbar_ctrl.Value = top_item;
						}
					
						SetHighLightedItem (highlighted_item - (page_size - 1));
					}
					
					break;
				}				
					
				default:
					break;
				}		
				
			}
			
			private void OnKeyDownPUW (object sender, KeyEventArgs e) 			
			{				
				switch (e.KeyCode) {			
				case Keys.Up:
					NavigateItem (ItemNavigation.Previous);
					break;				
	
				case Keys.Down:				
					NavigateItem (ItemNavigation.Next);
					break;
				
				case Keys.PageUp:
					NavigateItem (ItemNavigation.PreviousPage);
					break;				
	
				case Keys.PageDown:				
					NavigateItem (ItemNavigation.NextPage);
					break;
				
				default:
					break;
				}
			}
			
			private void SetHighLightedItem (int index)
			{
				Rectangle invalidate;
				
				/* Previous item */
    				if (highlighted_item != -1) {
					invalidate = GetItemDisplayRectangle (highlighted_item, top_item);
	    				if (ClientRectangle.Contains (invalidate))
	    					Invalidate (invalidate);
	    			}
				
    				highlighted_item = index;

    				 /* Current item */
    				invalidate = GetItemDisplayRectangle (highlighted_item, top_item);
    				if (ClientRectangle.Contains (invalidate))
    					Invalidate (invalidate);
				
			}			
			
			private void OnMouseDownPUW (object sender, MouseEventArgs e)
	    		{
	    			/* Click outside the client area destroys the popup */
	    			if (ClientRectangle.Contains (e.X, e.Y) == false) {
	    				HideWindow ();
	    				return;
	    			}

	    			/* Click on an element */
	    			int index = IndexFromPointDisplayRectangle (e.X, e.Y);
    				if (index == -1) return;

				owner.SelectedIndex = index;
				HideWindow ();
			}			

			private void OnMouseUpPUW (object sender, MouseEventArgs e)
	    		{

			}

			private void OnMouseMovePUW (object sender, MouseEventArgs e)
			{				
				int index = IndexFromPointDisplayRectangle (e.X, e.Y);

    				if (index != -1)
					SetHighLightedItem (index);
			}

			private void OnPaintPUW (Object o, PaintEventArgs pevent)
			{
				if (Width <= 0 || Height <=  0 || Visible == false)
	    				return;

				Draw (pevent.ClipRectangle);
				pevent.Graphics.DrawImage (ImageBuffer, pevent.ClipRectangle, pevent.ClipRectangle, GraphicsUnit.Pixel);
			}

			public void ShowWindow ()
			{
				CalcListBoxArea ();
				Show ();
				Refresh ();
				
				if (owner.DropDown != null)
					owner.DropDown (owner, EventArgs.Empty);
			}

			// Value Changed
			private void VerticalScrollEvent (object sender, EventArgs e)
			{				
				top_item =  vscrollbar_ctrl.Value;
				last_item = LastVisibleItem ();
				Refresh ();
			}

			#endregion Private Methods
		}
	}
}

