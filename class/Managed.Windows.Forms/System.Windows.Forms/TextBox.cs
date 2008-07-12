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
// Copyright (c) 2004-2006 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//     Daniel Nauck    (dna(at)mono-project(dot)de)
//

// NOT COMPLETE

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
#if NET_2_0
using System.Collections.Generic;
using System.Runtime.InteropServices;
#endif

namespace System.Windows.Forms {

#if NET_2_0
	[ComVisible(true)]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[Designer ("System.Windows.Forms.Design.TextBoxDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
#endif
	public class TextBox : TextBoxBase {
		#region Variables
		private ContextMenu	menu;
		private MenuItem	undo;
		private MenuItem	cut;
		private MenuItem	copy;
		private MenuItem	paste;
		private MenuItem	delete;
		private MenuItem	select_all;

#if NET_2_0
		private bool use_system_password_char;
		private AutoCompleteStringCollection auto_complete_custom_source;
		private AutoCompleteMode auto_complete_mode = AutoCompleteMode.None;
		private AutoCompleteSource auto_complete_source = AutoCompleteSource.None;
		private AutoCompleteListBox auto_complete_listbox;
		private ComboBox auto_complete_cb_source;
#endif
		#endregion	// Variables

		#region Public Constructors
		public TextBox() {

			scrollbars = RichTextBoxScrollBars.None;
			alignment = HorizontalAlignment.Left;
			this.LostFocus +=new EventHandler(TextBox_LostFocus);
			this.RightToLeftChanged += new EventHandler (TextBox_RightToLeftChanged);
#if NET_2_0
			TextChanged += new EventHandler (TextBox_TextChanged);
#endif

			BackColor = SystemColors.Window;
			ForeColor = SystemColors.WindowText;
			backcolor_set = false;

			SetStyle (ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, false);
			SetStyle (ControlStyles.FixedHeight, true);

			undo = new MenuItem(Locale.GetText("&Undo"));
			cut = new MenuItem(Locale.GetText("Cu&t"));
			copy = new MenuItem(Locale.GetText("&Copy"));
			paste = new MenuItem(Locale.GetText("&Paste"));
			delete = new MenuItem(Locale.GetText("&Delete"));
			select_all = new MenuItem(Locale.GetText("Select &All"));

			menu = new ContextMenu(new MenuItem[] { undo, new MenuItem("-"), cut, copy, paste, delete, new MenuItem("-"), select_all});
			ContextMenu = menu;

			menu.Popup += new EventHandler(menu_Popup);
			undo.Click += new EventHandler(undo_Click);
			cut.Click += new EventHandler(cut_Click);
			copy.Click += new EventHandler(copy_Click);
			paste.Click += new EventHandler(paste_Click);
			delete.Click += new EventHandler(delete_Click);
			select_all.Click += new EventHandler(select_all_Click);

			document.multiline = false;
		}

		#endregion	// Public Constructors

		#region Private & Internal Methods

		void TextBox_RightToLeftChanged (object sender, EventArgs e)
		{
			UpdateAlignment ();
		}

		private void TextBox_LostFocus (object sender, EventArgs e) {
			if (hide_selection)
				document.InvalidateSelectionArea ();
#if NET_2_0
			if (auto_complete_listbox != null && auto_complete_listbox.Visible)
				auto_complete_listbox.HideListBox (false);
#endif
		}

#if NET_2_0
		void TextBox_TextChanged (object o, EventArgs args)
		{
			if (auto_complete_mode == AutoCompleteMode.None || auto_complete_source == AutoCompleteSource.None)
				return;

			// We only support CustomSource by now
			IList source;
			if (auto_complete_cb_source == null)
				source = auto_complete_custom_source;
			else
				source = auto_complete_cb_source.Items;

			if (auto_complete_source != AutoCompleteSource.CustomSource ||
				source == null || source.Count == 0)
				return;

			if (Text.Length == 0) {
				if (auto_complete_listbox != null)
					auto_complete_listbox.HideListBox (false);
				return;
			}

			if (auto_complete_listbox == null)
				auto_complete_listbox = new AutoCompleteListBox (this);

			// If the text was just set by the auto complete listbox, ignore it
			if (auto_complete_listbox.WasTextSet) {
				auto_complete_listbox.WasTextSet = false;
				return;
			}

			string text = Text;
			auto_complete_listbox.Items.Clear ();

			for (int i = 0; i < source.Count; i++) {
				string item_text = auto_complete_cb_source == null ? auto_complete_custom_source [i] :
					auto_complete_cb_source.GetItemText (auto_complete_cb_source.Items [i]);
				if (item_text.StartsWith (text, StringComparison.CurrentCultureIgnoreCase))
					auto_complete_listbox.Items.Add (item_text);
			}

			IList<string> matches = auto_complete_listbox.Items;
			if ((matches.Count == 0) ||
				(matches.Count == 1 && matches [0].Equals (text, StringComparison.CurrentCultureIgnoreCase))) { // Exact single match

				if (auto_complete_listbox.Visible)
					auto_complete_listbox.HideListBox (false);
				return;
			}

			// Show or update auto complete listbox contents
			auto_complete_listbox.Location = PointToScreen (new Point (0, Height));
			auto_complete_listbox.ShowListBox ();
		}

		internal ComboBox AutoCompleteInternalSource {
			get {
				return auto_complete_cb_source;
			}
			set {
				auto_complete_cb_source = value;
			}
		}
#endif

		private void UpdateAlignment ()
		{
			HorizontalAlignment new_alignment = alignment;
			RightToLeft rtol = GetInheritedRtoL ();

			if (rtol == RightToLeft.Yes) {
				if (new_alignment == HorizontalAlignment.Left)
					new_alignment = HorizontalAlignment.Right;
				else if (new_alignment == HorizontalAlignment.Right)
					new_alignment = HorizontalAlignment.Left;
			}

			document.alignment = new_alignment;

			// MS word-wraps if alignment isn't left
			if (Multiline) {
				if (alignment != HorizontalAlignment.Left) {
					document.Wrap = true;
				} else {
					document.Wrap = word_wrap;
				}
			}

			for (int i = 1; i <= document.Lines; i++) {
				document.GetLine (i).Alignment = new_alignment;
			}

			document.RecalculateDocument (CreateGraphicsInternal ());

			Invalidate ();	// Make sure we refresh
		}

		internal override Color ChangeBackColor (Color backColor)
		{
			if (backColor == Color.Empty) {
#if NET_2_0
				if (!ReadOnly)
					backColor = SystemColors.Window;
#else
				backColor = SystemColors.Window;
#endif
				backcolor_set = false;
			}
			return backColor;
		}

#if NET_2_0
		void OnAutoCompleteCustomSourceChanged(object sender, CollectionChangeEventArgs e) {
			if(auto_complete_source == AutoCompleteSource.CustomSource) {
				//FIXME: handle add, remove and refresh events in AutoComplete algorithm.
			}
		}
#endif
		#endregion	// Private & Internal Methods

		#region Public Instance Properties
#if NET_2_0
		[MonoTODO("AutoCompletion algorithm is currently not implemented.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		[Localizable (true)]
		[Editor ("System.Windows.Forms.Design.ListControlStringCollectionEditor, " + Consts.AssemblySystem_Design,
		 "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public AutoCompleteStringCollection AutoCompleteCustomSource { 
			get {
				if(auto_complete_custom_source == null) {
					auto_complete_custom_source = new AutoCompleteStringCollection ();
					auto_complete_custom_source.CollectionChanged += new CollectionChangeEventHandler (OnAutoCompleteCustomSourceChanged);
				}
				return auto_complete_custom_source;
			}
			set {
				if(auto_complete_custom_source == value)
					return;

				if(auto_complete_custom_source != null) //remove eventhandler from old collection
					auto_complete_custom_source.CollectionChanged -= new CollectionChangeEventHandler (OnAutoCompleteCustomSourceChanged);

				auto_complete_custom_source = value;

				if(auto_complete_custom_source != null)
					auto_complete_custom_source.CollectionChanged += new CollectionChangeEventHandler (OnAutoCompleteCustomSourceChanged);
			}
		}

		[MonoTODO("AutoCompletion algorithm is currently not implemented.")]
		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		[DefaultValue (AutoCompleteMode.None)]
		public AutoCompleteMode AutoCompleteMode {
			get { return auto_complete_mode; }
			set {
				if(auto_complete_mode == value)
					return;

				if((value < AutoCompleteMode.None) || (value > AutoCompleteMode.SuggestAppend))
					throw new InvalidEnumArgumentException (Locale.GetText ("Enum argument value '{0}' is not valid for AutoCompleteMode", value));

				auto_complete_mode = value;
			}
		}

		[MonoTODO("AutoCompletion algorithm is currently not implemented.")]
		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		[DefaultValue (AutoCompleteSource.None)]
		[TypeConverter (typeof (TextBoxAutoCompleteSourceConverter))]
		public AutoCompleteSource AutoCompleteSource {
			get { return auto_complete_source; }
			set {
				if(auto_complete_source == value)
					return;

				if(!Enum.IsDefined (typeof (AutoCompleteSource), value))
					throw new InvalidEnumArgumentException (Locale.GetText ("Enum argument value '{0}' is not valid for AutoCompleteSource", value));

				auto_complete_source = value;
			}
		}

		[DefaultValue(false)]
		[RefreshProperties (RefreshProperties.Repaint)]
		public bool UseSystemPasswordChar {
			get {
				return use_system_password_char;
			}

			set {
				if (use_system_password_char != value) {
					use_system_password_char = value;
					
					if (!Multiline)
						document.PasswordChar = PasswordChar.ToString ();
					else
						document.PasswordChar = string.Empty;
				}
			}
		}
#endif

		[DefaultValue(false)]
		[MWFCategory("Behavior")]
		public bool AcceptsReturn {
			get {
				return accepts_return;
			}

			set {
				if (value != accepts_return) {
					accepts_return = value;
				}
			}
		}

		[DefaultValue(CharacterCasing.Normal)]
		[MWFCategory("Behavior")]
		public CharacterCasing CharacterCasing {
			get {
				return character_casing;
			}

			set {
				if (value != character_casing) {
					character_casing = value;
				}
			}
		}

		[Localizable(true)]
		[DefaultValue('\0')]
		[MWFCategory("Behavior")]
#if NET_2_0
		[RefreshProperties (RefreshProperties.Repaint)]
#endif
		public char PasswordChar {
			get {
#if NET_2_0
				if (use_system_password_char) {
					return '*';
				}
#endif
				return password_char;
			}

			set {
				if (value != password_char) {
					password_char = value;
					if (!Multiline) {
						document.PasswordChar = PasswordChar.ToString ();
					} else {
						document.PasswordChar = string.Empty;
					}
					this.CalculateDocument();
				}
			}
		}

		[DefaultValue(ScrollBars.None)]
		[Localizable(true)]
		[MWFCategory("Appearance")]
		public ScrollBars ScrollBars {
			get {
				return (ScrollBars)scrollbars;
			}

			set {
				if (!Enum.IsDefined (typeof (ScrollBars), value))
					throw new InvalidEnumArgumentException ("value", (int) value,
						typeof (ScrollBars));

				if (value != (ScrollBars)scrollbars) {
					scrollbars = (RichTextBoxScrollBars)value;
					base.CalculateScrollBars();
				}
			}
		}

#if ONLY_1_1
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override int SelectionLength {
			get {
				return base.SelectionLength;
			}
			set {
				base.SelectionLength = value;
			}
		}
#endif

		public override string Text {
			get {
				return base.Text;
			}

			set {
				base.Text = value;
			}
		}

		[DefaultValue(HorizontalAlignment.Left)]
		[Localizable(true)]
		[MWFCategory("Appearance")]
		public HorizontalAlignment TextAlign {
			get {
				return alignment;
			}

			set {
				if (value != alignment) {
					alignment = value;

					UpdateAlignment ();

					OnTextAlignChanged(EventArgs.Empty);
				}
			}
		}
		#endregion	// Public Instance Properties

#if NET_2_0
		public void Paste (string text)
		{
			document.ReplaceSelection (CaseAdjust (text), false);

			ScrollToCaret();
			OnTextChanged(EventArgs.Empty);
		}
#endif
		#region Protected Instance Methods
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}

#if ONLY_1_1
		protected override ImeMode DefaultImeMode {
			get {
				return base.DefaultImeMode;
			}
		}
#endif
#if NET_2_0
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}
#endif

		protected override bool IsInputKey (Keys keyData)
		{
			return base.IsInputKey (keyData);
		}

		protected override void OnGotFocus (EventArgs e)
		{
			base.OnGotFocus (e);
			if (selection_length == -1 && !has_been_focused)
				SelectAllNoScroll ();
			has_been_focused = true;
		}

		protected override void OnHandleCreated (EventArgs e)
		{
			base.OnHandleCreated (e);
		}

#if ONLY_1_1
		protected override void OnMouseUp(MouseEventArgs mevent)
		{
			base.OnMouseUp (mevent);
		}
#endif

		protected virtual void OnTextAlignChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [TextAlignChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void WndProc (ref Message m)
		{
			switch ((Msg)m.Msg) {
				case Msg.WM_LBUTTONDOWN:
					// When the textbox gets focus by LBUTTON (but not by middle or right)
					// it does not do the select all / scroll thing.
					has_been_focused = true;
					FocusInternal (true);
					break;
			}

			base.WndProc(ref m);
		}
		#endregion	// Protected Instance Methods

		#region Events
		static object TextAlignChangedEvent = new object ();

		public event EventHandler TextAlignChanged {
			add { Events.AddHandler (TextAlignChangedEvent, value); }
			remove { Events.RemoveHandler (TextAlignChangedEvent, value); }
		}
		#endregion	// Events

		#region Private Methods

		internal override ContextMenu ContextMenuInternal {
			get {
				ContextMenu res = base.ContextMenuInternal;
				if (res == menu)
					return null;
				return res;
			}
			set {
				base.ContextMenuInternal = value;
			}
		}

		internal void RestoreContextMenu ()
		{
			ContextMenuInternal = menu;
		}

		private void menu_Popup(object sender, EventArgs e) {
			if (SelectionLength == 0) {
				cut.Enabled = false;
				copy.Enabled = false;
			} else {
				cut.Enabled = true;
				copy.Enabled = true;
			}

			if (SelectionLength == TextLength) {
				select_all.Enabled = false;
			} else {
				select_all.Enabled = true;
			}

			if (!CanUndo) {
				undo.Enabled = false;
			} else {
				undo.Enabled = true;
			}

			if (ReadOnly) {
				undo.Enabled = cut.Enabled = paste.Enabled = delete.Enabled = false;
			}
		}

		private void undo_Click(object sender, EventArgs e) {
			Undo();
		}

		private void cut_Click(object sender, EventArgs e) {
			Cut();
		}

		private void copy_Click(object sender, EventArgs e) {
			Copy();
		}

		private void paste_Click(object sender, EventArgs e) {
			Paste();
		}

		private void delete_Click(object sender, EventArgs e) {
			SelectedText = string.Empty;
		}

		private void select_all_Click(object sender, EventArgs e) {
			SelectAll();
		}
		#endregion	// Private Methods

#if NET_2_0
		public override bool Multiline {
			get {
				return base.Multiline;
			}

			set {
				base.Multiline = value;
			}
		}

		protected override void OnBackColorChanged (EventArgs e)
		{
			base.OnBackColorChanged (e);
		}
		
		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
		}

		protected override void OnHandleDestroyed (EventArgs e)
		{
			base.OnHandleDestroyed (e);
		}

		class AutoCompleteListBox : Control
		{
			TextBox owner;
			VScrollBar vscroll;
			List<string> items;
			int top_item;
			int last_item;
			int page_size;
			int item_height;
			int highlighted_index = -1;
			bool user_defined_size;
			bool resizing;
			bool was_text_set;
			Rectangle resizer_bounds;

			const int DefaultDropDownItems = 7;

			public AutoCompleteListBox (TextBox tb)
			{
				owner = tb;
				items = new List<string> ();
				item_height = FontHeight + 2;

				vscroll = new VScrollBar ();
				vscroll.ValueChanged += VScrollValueChanged;
				Controls.Add (vscroll);

				is_visible = false;
				InternalBorderStyle = BorderStyle.FixedSingle;
			}

			protected override CreateParams CreateParams {
				get {
					CreateParams cp = base.CreateParams;

					cp.Style ^= (int)WindowStyles.WS_CHILD;
					cp.Style ^= (int)WindowStyles.WS_VISIBLE;
					cp.Style |= (int)WindowStyles.WS_POPUP;
					cp.ExStyle |= (int)WindowExStyles.WS_EX_TOPMOST | (int)WindowExStyles.WS_EX_TOOLWINDOW;
					return cp;
				}
			}

			public IList<string> Items {
				get {
					return items;
				}
			}

			public int HighlightedIndex {
				get {
					return highlighted_index;
				}
				set {
					if (value == highlighted_index)
						return;

					if (highlighted_index != -1)
						Invalidate (GetItemBounds (highlighted_index));
					highlighted_index = value;
					if (highlighted_index != -1)
						Invalidate (GetItemBounds (highlighted_index));
				}
			}

			public bool WasTextSet {
				get {
					return was_text_set;
				}
				set {
					was_text_set = value;
				}
			}

			internal override bool ActivateOnShow {
				get {
					return false;
				}
			}

			void VScrollValueChanged (object o, EventArgs args)
			{
				if (top_item == vscroll.Value)
					return;

				top_item = vscroll.Value;
				last_item = GetLastVisibleItem ();
				Invalidate ();
			}

			int GetLastVisibleItem ()
			{
				int top_y = Height;

				for (int i = top_item; i < items.Count; i++) {
					int pos = i - top_item; // relative to visible area
					if ((pos * item_height) + item_height >= top_y)
						return i;
				}

				return items.Count - 1;
			}

			Rectangle GetItemBounds (int index)
			{
				int pos = index - top_item;
				Rectangle bounds = new Rectangle (0, pos * item_height, Width, item_height);
				if (vscroll.Visible)
					bounds.Width -= vscroll.Width;

				return bounds;
			}

			int GetItemAt (Point loc)
			{
				if (loc.Y > (last_item - top_item) * item_height + item_height)
					return -1;

				int retval = loc.Y / item_height;
				retval += top_item;

				return retval;
			}

			void LayoutListBox ()
			{
				int total_height = items.Count * item_height;
				page_size = Math.Max (Height / item_height, 1);
				last_item = GetLastVisibleItem ();

				if (Height < total_height) {
					vscroll.Visible = true;
					vscroll.Maximum = items.Count - 1;
					vscroll.LargeChange = page_size;
					vscroll.Location = new Point (Width - vscroll.Width, 0);
					vscroll.Height = Height - item_height;
				} else
					vscroll.Visible = false;

				resizer_bounds = new Rectangle (Width - item_height, Height - item_height,
						item_height, item_height);
			}

			public void HideListBox (bool set_text)
			{
				if (set_text) {
					was_text_set = true;
					owner.Text = items [HighlightedIndex];
					owner.SelectAll ();
				}

				Capture = false;
				Hide ();
			}

			public void ShowListBox ()
			{
				if (!user_defined_size) {
					// This should call the Layout routine for us
					int height = items.Count > DefaultDropDownItems ? DefaultDropDownItems * item_height : 
						(items.Count + 1) * item_height;
					Size = new Size (owner.Width, height);
				} else
					LayoutListBox ();

				vscroll.Value = 0;
				HighlightedIndex = -1;

				Show ();
				Invalidate ();
			}

			protected override void OnResize (EventArgs args)
			{
				base.OnResize (args);

				LayoutListBox ();
				Refresh ();
			}

			protected override void OnMouseDown (MouseEventArgs args)
			{
				base.OnMouseDown (args);

				if (!resizer_bounds.Contains (args.Location))
					return;

				user_defined_size = true;
				resizing = true;
				Capture = true;
			}

			protected override void OnMouseMove (MouseEventArgs args)
			{
				base.OnMouseMove (args);

				if (resizing) {
					Point mouse_loc = Control.MousePosition;
					Point ctrl_loc = PointToScreen (Point.Empty);

					Size new_size = new Size (mouse_loc.X - ctrl_loc.X, mouse_loc.Y - ctrl_loc.Y);
					if (new_size.Height < item_height)
						new_size.Height = item_height;
					if (new_size.Width < item_height)
						new_size.Width = item_height;

					Size = new_size;
					return;
				}

				Cursor = resizer_bounds.Contains (args.Location) ? Cursors.SizeNWSE : Cursors.Default;

				int item_idx = GetItemAt (args.Location);
				if (item_idx != -1)
					HighlightedIndex = item_idx;
			}

			protected override void OnMouseUp (MouseEventArgs args)
			{
				base.OnMouseUp (args);

				int item_idx = GetItemAt (args.Location);
				if (item_idx != -1 && !resizing)
					HideListBox (true);

				resizing = false;
				Capture = false;
			}

			internal override void OnPaintInternal (PaintEventArgs args)
			{
				Graphics g = args.Graphics;
				Brush brush = ThemeEngine.Current.ResPool.GetSolidBrush (ForeColor);

				int highlighted_idx = HighlightedIndex;

				int y = 0;
				for (int i = top_item; i <= last_item; i++) {
					Rectangle item_bounds = GetItemBounds (i);
					if (!item_bounds.IntersectsWith (args.ClipRectangle))
						continue;

					if (i == highlighted_idx) {
						g.FillRectangle (SystemBrushes.Highlight, item_bounds);
						g.DrawString (items [i], Font, SystemBrushes.HighlightText, item_bounds);
					} else 
						g.DrawString (items [i], Font, brush, item_bounds);

					y += item_height;
				}

				ThemeEngine.Current.CPDrawSizeGrip (g, SystemColors.Control, resizer_bounds);
			}
		}
#endif
	}
	
#if NET_2_0
	internal class TextBoxAutoCompleteSourceConverter : EnumConverter
	{
		public TextBoxAutoCompleteSourceConverter(Type type)
			: base(type)
		{ }

		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			StandardValuesCollection stdv = base.GetStandardValues(context);
			AutoCompleteSource[] arr = new AutoCompleteSource[stdv.Count];
			stdv.CopyTo(arr, 0);
			AutoCompleteSource[] arr2 = Array.FindAll(arr, delegate (AutoCompleteSource value) {
				// No "ListItems" in a TextBox.
				return value != AutoCompleteSource.ListItems;
			});
			return new StandardValuesCollection(arr2);
		}
	}
#endif
}
