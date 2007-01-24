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
//
//

// NOT COMPLETE
#undef Debug
#undef DebugClick

using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Text;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections;

namespace System.Windows.Forms {
	[DefaultEvent("TextChanged")]
	[Designer("System.Windows.Forms.Design.TextBoxBaseDesigner, " + Consts.AssemblySystem_Design)]
	public abstract class TextBoxBase : Control {
		#region Local Variables
		internal HorizontalAlignment	alignment;
		internal bool			accepts_tab;
		internal bool			accepts_return;
		internal bool			auto_size;
		internal bool			backcolor_set;
		internal CharacterCasing	character_casing;
		internal bool			hide_selection;
		internal int			max_length;
		internal bool			modified;
		internal bool			multiline;
		internal char			password_char;
		internal bool			read_only;
		internal bool			word_wrap;
		internal Document		document;
		internal LineTag		caret_tag;		// tag our cursor is in
		internal int			caret_pos;		// position on the line our cursor is in (can be 0 = beginning of line)
		internal ImplicitHScrollBar	hscroll;
		internal ImplicitVScrollBar	vscroll;
		internal RichTextBoxScrollBars	scrollbars;
		internal Timer			scroll_timer;
		internal bool			richtext;
		internal bool			show_selection;		// set to true to always show selection, even if no focus is set
		internal int			selection_length = -1;	// set to the user-specified selection length, or -1 if none
		internal bool show_caret_w_selection;  // TextBox shows the caret when the selection is visible
		internal int			requested_height;
		internal int			canvas_width;
		internal int			canvas_height;
		static internal int		track_width = 2;	//
		static internal int		track_border = 5;	//
		internal DateTime		click_last;
		internal int			click_point_x;
		internal int 			click_point_y;
		internal CaretSelection		click_mode;
		internal Bitmap			bmp;
		#if Debug
		internal static bool	draw_lines = false;
		#endif

		#endregion	// Local Variables

		#region Internal Constructor
		// Constructor will go when complete, only for testing - pdb
		internal TextBoxBase() {
			alignment = HorizontalAlignment.Left;
			accepts_return = false;
			accepts_tab = false;
			auto_size = true;
			border_style = BorderStyle.Fixed3D;
			character_casing = CharacterCasing.Normal;
			hide_selection = true;
			max_length = 32767;
			modified = true;
			multiline = false;
			password_char = '\0';
			read_only = false;
			word_wrap = true;
			richtext = false;
			show_selection = false;
			show_caret_w_selection = (this is TextBox);
			document = new Document(this);
			document.WidthChanged += new EventHandler(document_WidthChanged);
			document.HeightChanged += new EventHandler(document_HeightChanged);
			//document.CaretMoved += new EventHandler(CaretMoved);
			document.Wrap = false;
			requested_height = -1;
			click_last = DateTime.Now;
			click_mode = CaretSelection.Position;
			bmp = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			MouseDown += new MouseEventHandler(TextBoxBase_MouseDown);
			MouseUp += new MouseEventHandler(TextBoxBase_MouseUp);
			MouseMove += new MouseEventHandler(TextBoxBase_MouseMove);
			SizeChanged += new EventHandler(TextBoxBase_SizeChanged);
			FontChanged += new EventHandler(TextBoxBase_FontOrColorChanged);
			ForeColorChanged += new EventHandler(TextBoxBase_FontOrColorChanged);
			MouseWheel += new MouseEventHandler(TextBoxBase_MouseWheel);
			
			scrollbars = RichTextBoxScrollBars.None;

			hscroll = new ImplicitHScrollBar();
			hscroll.ValueChanged += new EventHandler(hscroll_ValueChanged);
			hscroll.SetStyle (ControlStyles.Selectable, false);
			hscroll.Enabled = false;
			hscroll.Visible = false;
			hscroll.Maximum = Int32.MaxValue;

			vscroll = new ImplicitVScrollBar();
			vscroll.ValueChanged += new EventHandler(vscroll_ValueChanged);
			vscroll.SetStyle (ControlStyles.Selectable, false);
			vscroll.Enabled = false;
			vscroll.Visible = false;
			vscroll.Maximum = Int32.MaxValue;

			SuspendLayout ();
			this.Controls.AddImplicit (hscroll);
			this.Controls.AddImplicit (vscroll);
			ResumeLayout ();
			
			SetStyle(ControlStyles.UserPaint | ControlStyles.StandardClick, false);
#if NET_2_0
			SetStyle(ControlStyles.UseTextForAccessibility, false);
#endif

			canvas_width = ClientSize.Width;
			canvas_height = ClientSize.Height;
			document.ViewPortWidth = canvas_width;
			document.ViewPortHeight = canvas_height;

			Cursor = Cursors.IBeam;

			CalculateScrollBars();
		}
		#endregion	// Internal Constructor

		#region Private and Internal Methods
		internal string CaseAdjust(string s) {
			if (character_casing == CharacterCasing.Normal) {
				return s;
			}
			if (character_casing == CharacterCasing.Lower) {
				return s.ToLower();
			} else {
				return s.ToUpper();
			}
		}

		internal override void HandleClick(int clicks, MouseEventArgs me) {
			// MS seems to fire the click event in spite of the styles they set
			bool click_set = GetStyle (ControlStyles.StandardClick);
			bool doubleclick_set = GetStyle (ControlStyles.StandardDoubleClick);

			// so explicitly set them to true first
			SetStyle (ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, true);

			base.HandleClick (clicks, me);

			// then revert to our previous state
			if (!click_set)
				SetStyle (ControlStyles.StandardClick, false);
			if (!doubleclick_set)
				SetStyle (ControlStyles.StandardDoubleClick, false);
		}

		#endregion	// Private and Internal Methods

		#region Public Instance Properties
		[DefaultValue(false)]
		[MWFCategory("Behavior")]
		public bool AcceptsTab {
			get {
				return accepts_tab;
			}

			set {
				if (value != accepts_tab) {
					accepts_tab = value;
					OnAcceptsTabChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(true)]
		[Localizable(true)]
		[RefreshProperties(RefreshProperties.Repaint)]
		[MWFCategory("Behavior")]
		public
#if NET_2_0
		override
#else
		virtual
#endif
		bool AutoSize {
			get {
				return auto_size;
			}

			set {
				if (value != auto_size) {
					auto_size = value;
					if (auto_size) {
						if (PreferredHeight != ClientSize.Height) {
							ClientSize = new Size(ClientSize.Width, PreferredHeight);
						}
					}
					OnAutoSizeChanged(EventArgs.Empty);
				}
			}
		}

		[DispId(-501)]
		public override System.Drawing.Color BackColor {
			get {
				return base.BackColor;
			}
			set {
				if (value != ThemeEngine.Current.ColorWindow) {
					backcolor_set = true;
				} else {
					backcolor_set = false;
				}
				base.BackColor = value;
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override System.Drawing.Image BackgroundImage {
			get {
				return base.BackgroundImage;
			}
			set {
				base.BackgroundImage = value;
			}
		}

		[DefaultValue(BorderStyle.Fixed3D)]
		[DispId(-504)]
		[MWFCategory("Appearance")]
		public BorderStyle BorderStyle {
			get { return InternalBorderStyle; }
			set { 
				InternalBorderStyle = value; 
				OnBorderStyleChanged(EventArgs.Empty);
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool CanUndo {
			get {
				return document.undo.CanUndo;
			}
		}

		[DispId(-513)]
		public override System.Drawing.Color ForeColor {
			get {
				return base.ForeColor;
			}
			set {
				base.ForeColor = value;
			}
		}

		[DefaultValue(true)]
		[MWFCategory("Behavior")]
		public bool HideSelection {
			get {
				return hide_selection;
			}

			set {
				if (value != hide_selection) {
					hide_selection = value;
					OnHideSelectionChanged(EventArgs.Empty);
				}
				if (hide_selection) {
					document.selection_visible = false;
				} else {
					document.selection_visible = true;
				}
				document.InvalidateSelectionArea();

			}
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Editor("System.Windows.Forms.Design.StringArrayEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		[Localizable(true)]
		[MWFCategory("Appearance")]
		public string[] Lines {
			get {
				int count;
				ArrayList lines;

				count = document.Lines;

				// Handle empty document
				if ((count == 1) && (document.GetLine (1).text.Length == 0)) {
					return new string [0];
				}

				lines = new ArrayList ();

				int i = 1;
				while (i <= count) {
					Line line;
					StringBuilder lt = new StringBuilder ();

					do {
						line = document.GetLine (i++);
						lt.Append (line.text.ToString ());
					} while (line.soft_break && i <= count);

					lines.Add (lt.ToString ());	
				}

				return (string []) lines.ToArray (typeof (string));
			}

			set {
				int	i;
				int	l;
				SolidBrush brush;

				document.Empty();

				l = value.Length;
				brush = ThemeEngine.Current.ResPool.GetSolidBrush(this.ForeColor);

				document.SuspendRecalc ();
				for (i = 0; i < l; i++) {

					// Don't add the last line if it is just an empty line feed
					// the line feed is reflected in the previous line's soft_break = false
					if (i == l - 1 && value [i].Length == 0)
						break;

					bool carriage_return = false;
					if (value [i].EndsWith ("\r")) {
						value [i] = value [i].Substring (0, value [i].Length - 1);
						carriage_return = true;
					}

					document.Add(i+1, CaseAdjust(value[i]), alignment, Font, brush);
					if (carriage_return) {
						Line line = document.GetLine (i + 1);
						line.carriage_return = true;
					}
				}

				document.ResumeRecalc (true);

				// CalculateDocument();
				OnTextChanged(EventArgs.Empty);
			}
		}

		[DefaultValue(32767)]
		[Localizable(true)]
		[MWFCategory("Behavior")]
		public virtual int MaxLength {
			get {
				if (max_length == 2147483646) {	// We don't distinguish between single and multi-line limits
					return 0;
				}
				return max_length;
			}

			set {
				if (value != max_length) {
					max_length = value;
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool Modified {
			get {
				return modified;
			}

			set {
				if (value != modified) {
					modified = value;
					OnModifiedChanged(EventArgs.Empty);
				}
			}
		}

		[DefaultValue(false)]
		[Localizable(true)]
		[RefreshProperties(RefreshProperties.All)]
		[MWFCategory("Behavior")]
		public virtual bool Multiline {
			get {
				return multiline;
			}

			set {
				if (value != multiline) {
					multiline = value;
					// Make sure we update our size; the user may have already set the size before going to multiline
					if (multiline && requested_height != -1) {
						Height = requested_height;
						requested_height = -1;
					}

					if (Parent != null)
						Parent.PerformLayout ();

					OnMultilineChanged(EventArgs.Empty);
				}

				document.multiline = multiline;

				if (multiline) {
					document.Wrap = word_wrap;
					document.PasswordChar = "";

				} else {
					document.Wrap = false;
					if (this.password_char != '\0') {
						document.PasswordChar = password_char.ToString();
					} else {
						document.PasswordChar = "";
					}
				}

				CalculateDocument ();
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public int PreferredHeight {
			get {
				return Font.Height + (BorderStyle == BorderStyle.None ? 0 : 7);
			}
		}

		[DefaultValue(false)]
		[MWFCategory("Behavior")]
		public bool ReadOnly {
			get {
				return read_only;
			}

			set {
				if (value != read_only) {
					read_only = value;
					OnReadOnlyChanged(EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual string SelectedText {
			get {
				return document.GetSelection();
			}

			set {
				document.ReplaceSelection(CaseAdjust(value), false);

				ScrollToCaret();
				OnTextChanged(EventArgs.Empty);
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual int SelectionLength {
			get {
				int res = document.SelectionLength ();
				if (res == 0)
					res = -1;
				return res;
			}

			set {
				if (value < 0) {
					throw new ArgumentException(String.Format("{0} is not a valid value", value), "value");
				}

				if (value != 0) {
					int	start;
					Line	line;
					LineTag	tag;
					int	pos;

					selection_length = value;

					start = document.LineTagToCharIndex(document.selection_start.line, document.selection_start.pos);

					document.CharIndexToLineTag(start + value, out line, out tag, out pos);
					document.SetSelectionEnd(line, pos);
					document.PositionCaret(line, pos);
				} else {
					selection_length = -1;

					document.SetSelectionEnd(document.selection_start.line, document.selection_start.pos);
					document.PositionCaret(document.selection_start.line, document.selection_start.pos);
				}
			}
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int SelectionStart {
			get {
				int index;

				index = document.LineTagToCharIndex(document.selection_start.line, document.selection_start.pos);

				return index;
			}

			set {
				document.SetSelectionStart(value);
				if (selection_length > -1 ) {
					document.SetSelectionEnd(value + selection_length);
				} else {
					document.SetSelectionEnd(value);
				}
				document.PositionCaret(document.selection_start.line, document.selection_start.pos);
				ScrollToCaret();
			}
		}

		[Localizable(true)]
		public override string Text {
			get {
				if (document == null || document.Root == null || document.Root.text == null) {
					return string.Empty;
				}

				StringBuilder sb = new StringBuilder();

				Line line = null;
				for (int i = 1; i <= document.Lines; i++) {
					if (line != null && line.carriage_return)
						sb.Append ("\r");
					if (line != null && !line.soft_break)
						sb.Append (Environment.NewLine);
					line = document.GetLine (i);
					sb.Append(line.text.ToString());
				}

				return sb.ToString();
			}

			set {
				if (value == Text)
					return;

				if ((value != null) && (value != "")) {

					document.Empty ();

					document.Insert (document.GetLine (1), 0, false, value);
							
					document.PositionCaret (document.GetLine (1), 0);
					document.SetSelectionToCaret (true);

					ScrollToCaret ();
				} else {
					document.Empty();
					CalculateDocument();
				}

				// set the var so OnModifiedChanged is not raised
				modified = false;
				OnTextChanged(EventArgs.Empty);
			}
		}

		[Browsable(false)]
		public virtual int TextLength {
			get {
				if (document == null || document.Root == null || document.Root.text == null) {
					return 0;
				}

				if (!multiline) {
					return document.Root.text.Length;
				} else {
					int	total;
					int	i;

					total = 0;
					for (i = 1; i < document.Lines; i++) {
						total += document.GetLine(i).text.Length + Environment.NewLine.Length;
					}
					total += document.GetLine(i).text.Length;

					return total;
				}
			}
		}

		[DefaultValue(true)]
		[Localizable(true)]
		[MWFCategory("Behavior")]
		public bool WordWrap {
			get {
				return word_wrap;
			}

			set {
				if (value != word_wrap) {
					if (multiline) {
						word_wrap = value;
						document.Wrap = value;
					}
				}
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Properties
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}

		protected override System.Drawing.Size DefaultSize {
			get {
				return new Size(100, 20);
			}
		}
		#endregion	// Protected Instance Properties

		#region Public Instance Methods
		public void AppendText(string text) {
			//if (multiline) {

				document.MoveCaret(CaretDirection.CtrlEnd);				
				document.Insert (document.caret.line, /* document.caret.tag, */ document.caret.pos, false, text);
				//	CalculateDocument();
				//} else {
				//	document.MoveCaret(CaretDirection.CtrlEnd);
				//	document.InsertStringAtCaret(text, true);

				//	Invalidate();
				// }

			document.MoveCaret(CaretDirection.CtrlEnd);
			document.SetSelectionToCaret (true);

			OnTextChanged(EventArgs.Empty);
		}

		public void Clear() {
			Text = null;
		}

		public void ClearUndo() {
			document.undo.Clear();
		}

		public void Copy() {
			DataObject	o;

			o = new DataObject(DataFormats.Text, SelectedText);
			if (this is RichTextBox) {
				o.SetData(DataFormats.Rtf, ((RichTextBox)this).SelectedRtf);
			}
			Clipboard.SetDataObject(o);
		}

		public void Cut() {
			DataObject	o;

			o = new DataObject(DataFormats.Text, SelectedText);
			if (this is RichTextBox) {
				o.SetData(DataFormats.Rtf, ((RichTextBox)this).SelectedRtf);
			}
			Clipboard.SetDataObject (o);
			document.ReplaceSelection ("", false);
		}

		public void Paste() {
			Paste(Clipboard.GetDataObject(), null, false);
		}

		public void ScrollToCaret() {
			if (IsHandleCreated) {
				CaretMoved(this, EventArgs.Empty);
			}
		}

		public void Select(int start, int length) {
			SelectionStart = start;
			SelectionLength = length;
		}


		public void SelectAll() {
			Line	last;

			last = document.GetLine(document.Lines);
			document.SetSelectionStart(document.GetLine(1), 0);
			document.SetSelectionEnd(last, last.text.Length);
			document.PositionCaret (document.selection_end.line, document.selection_end.pos);
			selection_length = -1;
		}

		public override string ToString() {
			return String.Concat (base.ToString (), ", Text: ", Text);
		}

		public void Undo() {
			document.undo.Undo();
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override void CreateHandle() {
			base.CreateHandle ();
			document.AlignCaret();
			ScrollToCaret();
		}

		protected override bool IsInputKey(Keys keyData) {
			if ((keyData & Keys.Alt) != 0) {
				return base.IsInputKey(keyData);
			}

			switch (keyData & Keys.KeyCode) {
				case Keys.Enter: {
					if (multiline && accepts_return) {
						return true;
					}
					return false;
				}

				case Keys.Tab: {
					if (accepts_tab && multiline) {
						if ((keyData & Keys.Control) == 0) {
							return true;
						}
					}
					return false;
				}

				case Keys.Left:
				case Keys.Right:
				case Keys.Up:
				case Keys.Down:
				case Keys.PageUp:
				case Keys.PageDown:
				case Keys.Home:
				case Keys.End: {
					return true;
				}
			}
			return false;
		}


		protected virtual void OnAcceptsTabChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [AcceptsTabChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnAutoSizeChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [AutoSizeChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnBorderStyleChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [BorderStyleChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnFontChanged(EventArgs e) {
			base.OnFontChanged (e);

			if (auto_size && !multiline) {
				if (PreferredHeight != ClientSize.Height) {
					Height = PreferredHeight;
				}
			}
		}

		protected override void OnHandleCreated(EventArgs e) {
			base.OnHandleCreated (e);
		}

		protected override void OnHandleDestroyed(EventArgs e) {
			base.OnHandleDestroyed (e);
		}

		protected virtual void OnHideSelectionChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [HideSelectionChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnModifiedChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [ModifiedChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnMultilineChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [MultilineChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnReadOnlyChanged(EventArgs e) {
			EventHandler eh = (EventHandler)(Events [ReadOnlyChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override bool ProcessDialogKey(Keys keyData) {
			return base.ProcessDialogKey(keyData);
		}

		private bool ProcessKey(Keys keyData) {
			bool control;
			bool shift;

			control = (Control.ModifierKeys & Keys.Control) != 0;
			shift = (Control.ModifierKeys & Keys.Shift) != 0;

			switch (keyData & Keys.KeyCode) {
				case Keys.X: {	// Cut (Ctrl-X)
					if (control) {
						Cut();
						return true;
					}
					return false;
				}

				case Keys.C: {	// Copy (Ctrl-C)
					if (control) {
						Copy();
						return true;
					}
					return false;
				}

				case Keys.V: {	// Paste (Ctrl-V)
					if (control) {
						return Paste(Clipboard.GetDataObject(), null, true);
					}
					return false;
				}

				case Keys.Z: {	// Undo (Ctrl-Z)
					if (control) {
						Undo();
						return true;
					}
					return false;
				}

				case Keys.A: {	// Select All (Ctrl-A)
					if (control) {
						SelectAll();
						return true;
					}
					return false;
				}

				case Keys.Left: {
					if (control) {
						document.MoveCaret(CaretDirection.WordBack);
					} else {
						if (!document.selection_visible || shift) {
							document.MoveCaret(CaretDirection.CharBack);
						} else {
							document.MoveCaret(CaretDirection.SelectionStart);
						}
					}

					if (!shift) {
						document.SetSelectionToCaret(true);
					} else {
						document.SetSelectionToCaret(false);
					}

					CaretMoved(this, null);
					return true;
				}

				case Keys.Right: {
					if (control) {
						document.MoveCaret(CaretDirection.WordForward);
					} else {
						if (!document.selection_visible || shift) {
							document.MoveCaret(CaretDirection.CharForward);
						} else {
							document.MoveCaret(CaretDirection.SelectionEnd);
						}
					}
					if (!shift) {
						document.SetSelectionToCaret(true);
					} else {
						document.SetSelectionToCaret(false);
					}

					CaretMoved(this, null);
					return true;
				}

				case Keys.Up: {
					if (control) {
						if (document.CaretPosition == 0) {
							document.MoveCaret(CaretDirection.LineUp);
						} else {
							document.MoveCaret(CaretDirection.Home);
						}
					} else {
						document.MoveCaret(CaretDirection.LineUp);
					}

					if ((Control.ModifierKeys & Keys.Shift) == 0) {
						document.SetSelectionToCaret(true);
					} else {
						document.SetSelectionToCaret(false);
					}

					CaretMoved(this, null);
					return true;
				}

				case Keys.Down: {
					if (control) {
						if (document.CaretPosition == document.CaretLine.Text.Length) {
							document.MoveCaret(CaretDirection.LineDown);
						} else {
							document.MoveCaret(CaretDirection.End);
						}
					} else {
						document.MoveCaret(CaretDirection.LineDown);
					}

					if ((Control.ModifierKeys & Keys.Shift) == 0) {
						document.SetSelectionToCaret(true);
					} else {
						document.SetSelectionToCaret(false);
					}

					CaretMoved(this, null);
					return true;
				}

				case Keys.Home: {
					if ((Control.ModifierKeys & Keys.Control) != 0) {
						document.MoveCaret(CaretDirection.CtrlHome);
					} else {
						document.MoveCaret(CaretDirection.Home);
					}

					if ((Control.ModifierKeys & Keys.Shift) == 0) {
						document.SetSelectionToCaret(true);
					} else {
						document.SetSelectionToCaret(false);
					}

					CaretMoved(this, null);
					return true;
				}

				case Keys.End: {
					if ((Control.ModifierKeys & Keys.Control) != 0) {
						document.MoveCaret(CaretDirection.CtrlEnd);
					} else {
						document.MoveCaret(CaretDirection.End);
					}

					if ((Control.ModifierKeys & Keys.Shift) == 0) {
						document.SetSelectionToCaret(true);
					} else {
						document.SetSelectionToCaret(false);
					}

					CaretMoved(this, null);
					return true;
				}

				case Keys.Enter: {
					// ignoring accepts_return, fixes bug #76355
					if (!read_only && multiline && (accepts_return || (FindForm() != null && FindForm().AcceptButton == null) || ((Control.ModifierKeys & Keys.Control) != 0))) {
						Line	line;

						if (document.selection_visible) {
							document.ReplaceSelection("", false);
						}

						line = document.CaretLine;

						document.Split (document.CaretLine, document.CaretTag, document.CaretPosition, false);
						OnTextChanged(EventArgs.Empty);
						document.UpdateView(line, 2, 0);

						CaretMoved(this, null);
						return true;
					}
					break;
				}

				case Keys.Tab: {
					if (!read_only && accepts_tab && multiline) {
						document.InsertChar(document.CaretLine, document.CaretPosition, '\t');
						if (document.selection_visible) {
							document.ReplaceSelection("", false);
						}
						document.SetSelectionToCaret(true);

						OnTextChanged(EventArgs.Empty);
						CaretMoved(this, null);
						return true;
					}
					break;
				}

				case Keys.Insert: {
					if (shift) {
						Paste(Clipboard.GetDataObject(), null, true);
						return true;
					}

					if (control) {
						Copy();
						return true;
					}

					// FIXME - need overwrite/insert toggle?
					return false;
				}

				case Keys.PageUp: {
					if ((Control.ModifierKeys & Keys.Control) != 0) {
						document.MoveCaret(CaretDirection.CtrlPgUp);
					} else {
						document.MoveCaret(CaretDirection.PgUp);
					}
					return true;
				}

				case Keys.PageDown: {
					if ((Control.ModifierKeys & Keys.Control) != 0) {
						document.MoveCaret(CaretDirection.CtrlPgDn);
					} else {
						document.MoveCaret(CaretDirection.PgDn);
					}
					return true;
				}

				case Keys.Delete: {
					if (shift) {
						Cut();
						return true;
					}

					if (read_only) {
						break;
					}

					if (document.selection_visible) {
						document.ReplaceSelection("", false);
					} else {
						// DeleteChar only deletes on the line, doesn't do the combine
						if (document.CaretPosition == document.CaretLine.Text.Length) {
							if (document.CaretLine.LineNo < document.Lines) {
								Line	line;

								line = document.GetLine(document.CaretLine.LineNo + 1);
								document.Combine(document.CaretLine, line);
								document.UpdateView(document.CaretLine, 2, 0);

								#if not_Debug
								Line	check_first;
								Line	check_second;

								check_first = document.GetLine(document.CaretLine.LineNo);
								check_second = document.GetLine(check_first.line_no + 1);

								Console.WriteLine("Post-UpdateView: Y of first line: {0}, second line: {1}", check_first.Y, check_second.Y);
								#endif

								// Caret doesn't move
							}
						} else {
							if (!control) {
								document.DeleteChar(document.CaretTag, document.CaretPosition, true);
							} else {
								int end_pos;

								end_pos = document.CaretPosition;

								while ((end_pos < document.CaretLine.Text.Length) && !Document.IsWordSeparator(document.CaretLine.Text[end_pos])) {
									end_pos++;
								}

								if (end_pos < document.CaretLine.Text.Length) {
									end_pos++;
								}
								document.DeleteChars(document.CaretTag, document.CaretPosition, end_pos - document.CaretPosition);
							}
						}
					}

					OnTextChanged(EventArgs.Empty);
					document.AlignCaret();
					document.UpdateCaret();
					CaretMoved(this, null);
					return true;
				}
			}
			return false;
		}

		private void HandleBackspace(bool control) {
			bool	fire_changed;

			fire_changed = false;

			// delete only deletes on the line, doesn't do the combine
			if (document.selection_visible) {
				document.ReplaceSelection("", false);
				fire_changed = true;
			}
			document.SetSelectionToCaret(true);

			if (document.CaretPosition == 0) {
				if (document.CaretLine.LineNo > 1) {
					Line	line;
					int	new_caret_pos;

					line = document.GetLine(document.CaretLine.LineNo - 1);
					new_caret_pos = line.text.Length;

					document.Combine(line, document.CaretLine);
					document.UpdateView(line, 1, 0);
					document.PositionCaret(line, new_caret_pos);
					document.SetSelectionToCaret (true);
					document.UpdateCaret();
					fire_changed = true;
				}
			} else {
				if (!control || document.CaretPosition == 0) {

					// Move before we delete because the delete will change positions around
					// if we cross a wrap border
					LineTag tag = document.CaretTag;
					int pos = document.CaretPosition;
					document.MoveCaret (CaretDirection.CharBack);
					document.DeleteChar (tag, pos, false);
					document.SetSelectionToCaret (true);
				} else {
					int start_pos;

					start_pos = document.CaretPosition - 1;

					while ((start_pos > 0) && !Document.IsWordSeparator(document.CaretLine.Text[start_pos - 1])) {
						start_pos--;
					}
					document.DeleteChars(document.CaretTag, start_pos, document.CaretPosition - start_pos);
					document.PositionCaret(document.CaretLine, start_pos);
					document.SetSelectionToCaret (true);
				}
				document.UpdateCaret();
				fire_changed = true;
			}
			if (fire_changed) {
				OnTextChanged(EventArgs.Empty);
			}
			CaretMoved(this, null);
		}

		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) {
			// Make sure we don't get sized bigger than we want to be
			if (!richtext) {
				if (!multiline) {
					if (height != PreferredHeight) {
						requested_height = height;
						height = PreferredHeight;
						specified |= BoundsSpecified.Height;
					}
				}
			}

			base.SetBoundsCore (x, y, width, height, specified);
		}

		protected override void WndProc(ref Message m) {
			switch ((Msg)m.Msg) {
			case Msg.WM_KEYDOWN: {
				if (ProcessKeyMessage(ref m) || ProcessKey((Keys)m.WParam.ToInt32() | XplatUI.State.ModifierKeys)) {
					m.Result = IntPtr.Zero;
					return;
				}
				DefWndProc (ref m);
				return;
			}

			case Msg.WM_CHAR: {
				int	ch;

				if (ProcessKeyMessage(ref m)) {
					m.Result = IntPtr.Zero;
					return;
				}

				if (read_only) {
					return;
				}

				m.Result = IntPtr.Zero;

				ch = m.WParam.ToInt32();

				if (ch == 127) {
					HandleBackspace(true);
				} else if (ch >= 32) {
					if (document.selection_visible) {
						document.ReplaceSelection("", false);
					}

					char c = (char)m.WParam;
					switch (character_casing) {
					case CharacterCasing.Upper:
						c = Char.ToUpper((char) m.WParam);
						break;
					case CharacterCasing.Lower:
						c = Char.ToLower((char) m.WParam);
						break;
					}

					if (document.Length < max_length) {
						document.InsertCharAtCaret(c, true);
						OnTextChanged(EventArgs.Empty);
						CaretMoved(this, null);
					} else {
						XplatUI.AudibleAlert();
					}
					return;
				} else if (ch == 8) {
					HandleBackspace(false);
				}

				return;
			}

			case Msg.WM_SETFOCUS:
				document.CaretHasFocus ();
				base.WndProc(ref m);
				break;

			case Msg.WM_KILLFOCUS:
				document.CaretLostFocus ();
				base.WndProc(ref m);
				break;

			default:
				base.WndProc(ref m);
				return;
			}
		}

		#endregion	// Protected Instance Methods

		#region Events
		static object AcceptsTabChangedEvent = new object ();
		static object AutoSizeChangedEvent = new object ();
		static object BorderStyleChangedEvent = new object ();
		static object HideSelectionChangedEvent = new object ();
		static object ModifiedChangedEvent = new object ();
		static object MultilineChangedEvent = new object ();
		static object ReadOnlyChangedEvent = new object ();
		static object HScrolledEvent = new object ();
		static object VScrolledEvent = new object ();

		public event EventHandler AcceptsTabChanged {
			add { Events.AddHandler (AcceptsTabChangedEvent, value); }
			remove { Events.RemoveHandler (AcceptsTabChangedEvent, value); }
		}

		public new event EventHandler AutoSizeChanged {
			add { Events.AddHandler (AutoSizeChangedEvent, value); }
			remove { Events.RemoveHandler (AutoSizeChangedEvent, value); }
		}

		public event EventHandler BorderStyleChanged {
			add { Events.AddHandler (BorderStyleChangedEvent, value); }
			remove { Events.RemoveHandler (BorderStyleChangedEvent, value); }
		}

		public event EventHandler HideSelectionChanged {
			add { Events.AddHandler (HideSelectionChangedEvent, value); }
			remove { Events.RemoveHandler (HideSelectionChangedEvent, value); }
		}

		public event EventHandler ModifiedChanged {
			add { Events.AddHandler (ModifiedChangedEvent, value); }
			remove { Events.RemoveHandler (ModifiedChangedEvent, value); }
		}

		public event EventHandler MultilineChanged {
			add { Events.AddHandler (MultilineChangedEvent, value); }
			remove { Events.RemoveHandler (MultilineChangedEvent, value); }
		}

		public event EventHandler ReadOnlyChanged {
			add { Events.AddHandler (ReadOnlyChangedEvent, value); }
			remove { Events.RemoveHandler (ReadOnlyChangedEvent, value); }
		}

		internal event EventHandler HScrolled {
			add { Events.AddHandler (HScrolledEvent, value); }
			remove { Events.RemoveHandler (HScrolledEvent, value); }
		}

		internal event EventHandler VScrolled {
			add { Events.AddHandler (VScrolledEvent, value); }
			remove { Events.RemoveHandler (VScrolledEvent, value); }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public new event EventHandler Click {
			add { base.Click += value; }
			remove { base.Click -= value; }
		}

		// XXX should this not manipulate base.Paint?
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event PaintEventHandler Paint;
		#endregion	// Events

		#region Private Methods
		internal Document Document {
			get {
				return document;
			}

			set {
				document = value;
			}
		}

		internal bool ShowSelection {
			get {
				if (show_selection || !hide_selection) {
					return true;
				}

				return has_focus;
			}

			set {
				if (show_selection == value)
					return;

				show_selection = value;
				// Currently InvalidateSelectionArea is commented out so do a full invalidate
				document.InvalidateSelectionArea();
			}
		}

		internal Graphics CreateGraphicsInternal() {
			if (IsHandleCreated) {
				return base.CreateGraphics();
			}

			return Graphics.FromImage(bmp);
		}

		#if Debug
		static int current;
		#endif

		internal override void OnPaintInternal (PaintEventArgs pevent) {
			// Fill background
			if (backcolor_set || (Enabled && !read_only)) {
				pevent.Graphics.FillRectangle(ThemeEngine.Current.ResPool.GetSolidBrush(BackColor), pevent.ClipRectangle);
			} else {
				pevent.Graphics.FillRectangle(ThemeEngine.Current.ResPool.GetSolidBrush(ThemeEngine.Current.ColorControl), pevent.ClipRectangle);
			}
			pevent.Graphics.TextRenderingHint=TextRenderingHint.AntiAlias;

			// Draw the viewable document
			document.Draw(pevent.Graphics, pevent.ClipRectangle);

			Rectangle	rect = ClientRectangle;
			rect.Width--;
			rect.Height--;
			//pevent.Graphics.DrawRectangle(ThemeEngine.Current.ResPool.GetPen(ThemeEngine.Current.ColorControlDark), rect);

			#if Debug
				int		start;
				int		end;
				Line		line;
				int		line_no;
				Pen		p;

				p = new Pen(Color.Red, 1);

				// First, figure out from what line to what line we need to draw
				start = document.GetLineByPixel(pevent.ClipRectangle.Top - document.ViewPortY, false).line_no;
				end = document.GetLineByPixel(pevent.ClipRectangle.Bottom - document.ViewPortY, false).line_no;

				//Console.WriteLine("Starting drawing on line '{0}'", document.GetLine(start));
				//Console.WriteLine("Ending drawing on line '{0}'", document.GetLine(end));

				line_no = start;
				while (line_no <= end) {
					line = document.GetLine(line_no);

					if (draw_lines) {
						for (int i = 0; i < line.text.Length; i++) {
							pevent.Graphics.DrawLine(p, (int)line.widths[i] - document.ViewPortX, line.Y - document.ViewPortY, (int)line.widths[i] - document.ViewPortX, line.Y + line.height  - document.ViewPortY);
						}
					}

					line_no++;
				}
			#endif
		}

		private bool IsDoubleClick (MouseEventArgs e)
		{
			TimeSpan interval = DateTime.Now - click_last;
			if (interval.TotalMilliseconds > SystemInformation.DoubleClickTime)
				return false;
			Size dcs = SystemInformation.DoubleClickSize;
			if (e.X < click_point_x - dcs.Width / 2 || e.X > click_point_x + dcs.Width / 2)
				return false;
			if (e.Y < click_point_y - dcs.Height / 2 || e.Y > click_point_y + dcs.Height / 2)
				return false;
			return true;
		}

		private void TextBoxBase_MouseDown (object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left) {

				document.PositionCaret(e.X + document.ViewPortX, e.Y + document.ViewPortY);

				if (IsDoubleClick (e)) {
					switch (click_mode) {
					case CaretSelection.Position:
						SelectWord ();
						click_mode = CaretSelection.Word;
						break;
					case CaretSelection.Word:

						if (this is TextBox) {
							document.SetSelectionToCaret (true);
							click_mode = CaretSelection.Position;
							break;
						}

						document.ExpandSelection (CaretSelection.Line, false);
						click_mode = CaretSelection.Line;
						break;
					case CaretSelection.Line:

						// Gotta do this first because Exanding to a word
						// from a line doesn't really work
						document.SetSelectionToCaret (true);

						SelectWord ();
						click_mode = CaretSelection.Word;
						break;
					}
				} else {
					document.SetSelectionToCaret (true);
					click_mode = CaretSelection.Position;
				}

				click_point_x = e.X;
				click_point_y = e.Y;
				click_last = DateTime.Now;
			}

			if ((e.Button == MouseButtons.Middle) && (((int)Environment.OSVersion.Platform == 4) || ((int)Environment.OSVersion.Platform == 128))) {
				Document.Marker	marker;

				marker.tag = document.FindCursor(e.X + document.ViewPortX, e.Y + document.ViewPortY, out marker.pos);
				marker.line = marker.tag.line;
				marker.height = marker.tag.height;

				document.SetSelection(marker.line, marker.pos, marker.line, marker.pos);
				Paste (Clipboard.GetDataObject (true), null, true);

			}
		}

		private void TextBoxBase_MouseUp(object sender, MouseEventArgs e) {
			if (e.Button == MouseButtons.Left) {
				if (click_mode == CaretSelection.Position) {
					document.SetSelectionToCaret(false);
					document.DisplayCaret();
				}

				if (scroll_timer != null) {
					scroll_timer.Enabled = false;
				}
				return;
			}
		}

		private void PositionControls ()
		{
			if (hscroll.Visible) {
				//vscroll.Maximum += hscroll.Height;
				canvas_height = ClientSize.Height - hscroll.Height;
			} else {
				canvas_height = ClientSize.Height;
			}

			if (vscroll.Visible) {
				//hscroll.Maximum += vscroll.Width;
				canvas_width = ClientSize.Width - vscroll.Width;
			} else {
				canvas_width = ClientSize.Width;
			}


			document.ViewPortWidth = canvas_width;
			document.ViewPortHeight = canvas_height;

			if (canvas_height < 1 || canvas_width < 1)
				return;

			// We always move them, they just might not be displayed
			hscroll.Bounds = new Rectangle (ClientRectangle.Left,
					Math.Max (0, ClientRectangle.Height - hscroll.Height),
					Math.Max (0, ClientSize.Width - (vscroll.Visible ? vscroll.Width : 0)),
					hscroll.Height);

			vscroll.Bounds = new Rectangle (Math.Max (0, ClientRectangle.Right - vscroll.Width),
					ClientRectangle.Top, vscroll.Width,
					Math.Max (0, ClientSize.Height - (hscroll.Visible ? hscroll.Height : 0)));
			
		}

		private void TextBoxBase_SizeChanged(object sender, EventArgs e) {
			CalculateDocument();
		}

		private void TextBoxBase_MouseWheel(object sender, MouseEventArgs e) {

			if (!vscroll.Enabled) {
				return;
			}

			if (e.Delta < 0)
				vscroll.Value = Math.Min (vscroll.Value + SystemInformation.MouseWheelScrollLines,
						Math.Max (0, vscroll.Maximum - document.ViewPortHeight + 1));
			else
				vscroll.Value = Math.Max (0, vscroll.Value - SystemInformation.MouseWheelScrollLines);
		}

		internal virtual void SelectWord ()
		{
			StringBuilder s = document.caret.line.text;
			int start = document.caret.pos;
			int end = document.caret.pos;

			if (s.Length < 1) {
				if (document.caret.line.line_no >= document.Lines)
					return;
				Line line = document.GetLine (document.caret.line.line_no + 1);
				document.PositionCaret (line, 0);
				return;
			}

			if (start > 0) {
				start--;
				end--;
			}

			// skip whitespace until we hit a word
			while (start > 0 && s [start] == ' ')
				start--;
			if (start > 0) {
				while (start > 0 && (s [start] != ' '))
					start--;
				if (s [start] == ' ')
					start++;
			}

			if (s [end] == ' ') {
				while (end < s.Length && s [end] == ' ')
					end++;
			} else {
				while (end < s.Length && s [end] != ' ')
					end++;
				while (end < s.Length && s [end] == ' ')
					end++;
			}

			document.SetSelection (document.caret.line, start, document.caret.line, end);
			document.PositionCaret (document.selection_end.line, document.selection_end.pos);
		}

		internal void CalculateDocument() {
			if (!IsHandleCreated) {
				return;
			}
			document.RecalculateDocument(CreateGraphicsInternal());
			CalculateScrollBars();

			 if (document.caret.line != null && document.caret.line.Y < document.ViewPortHeight) {
				// The window has probably been resized, making the entire thing visible, so
				// we need to set the scroll position back to zero.
				vscroll.Value = 0;
			 }

			 Invalidate();
		}

		internal void CalculateScrollBars () {
			// FIXME - need separate calculations for center and right alignment

			if (!multiline) {
				PositionControls ();
				return;
			}

			if (document.Width >= document.ViewPortWidth) {
				hscroll.SetValues (0, Math.Max (1, document.Width), -1,
						document.ViewPortWidth < 0 ? 0 : document.ViewPortWidth);
				hscroll.Enabled = true;
			} else {
				hscroll.Enabled = false;
				hscroll.Maximum = document.ViewPortWidth;
			}

			if (document.Height >= document.ViewPortHeight) {
				vscroll.SetValues (0, Math.Max (1, document.Height), -1,
						document.ViewPortHeight < 0 ? 0 : document.ViewPortHeight);
				vscroll.Enabled = true;
			} else {
				vscroll.Enabled = false;
				vscroll.Maximum = document.ViewPortHeight;
			}


			if (!WordWrap) {
				if ((scrollbars & RichTextBoxScrollBars.Horizontal) != 0) {
					if (((scrollbars & RichTextBoxScrollBars.ForcedHorizontal) != 0) || hscroll.Enabled) {
						hscroll.Visible = true;
					} else {
						hscroll.Visible = false;
					}
				} else {
					hscroll.Visible = false;
				}
			}

			if ((scrollbars & RichTextBoxScrollBars.Vertical) != 0) {
				if (((scrollbars & RichTextBoxScrollBars.ForcedVertical) != 0) || vscroll.Enabled) {
					vscroll.Visible = true;
				} else {
					vscroll.Visible = false;
				}
			} else {
				vscroll.Visible = false;
			}

			PositionControls ();
		}

		private void document_WidthChanged(object sender, EventArgs e) {
			CalculateScrollBars();
		}

		private void document_HeightChanged(object sender, EventArgs e) {
			CalculateScrollBars();
		}

		private void hscroll_ValueChanged(object sender, EventArgs e) {
			int old_viewport_x;

			old_viewport_x = document.ViewPortX;
			document.ViewPortX = this.hscroll.Value;

			//
			// Before scrolling we want to destroy the caret, then draw a new one after the scroll
			// the reason for this is that scrolling changes the coordinates of the caret, and we
			// will get tracers if we don't 
			//
			if (Focused)
				document.CaretLostFocus ();

			if (vscroll.Visible) {
				XplatUI.ScrollWindow(this.Handle, new Rectangle(0, 0, ClientSize.Width - vscroll.Width, ClientSize.Height), old_viewport_x - this.hscroll.Value, 0, false);
			} else {
				XplatUI.ScrollWindow(this.Handle, ClientRectangle, old_viewport_x - this.hscroll.Value, 0, false);
			}

			if (Focused)
				document.CaretHasFocus ();

			EventHandler eh = (EventHandler)(Events [HScrolledEvent]);
			if (eh != null)
				eh (this, EventArgs.Empty);
		}

		private void vscroll_ValueChanged(object sender, EventArgs e) {
			int old_viewport_y;

			old_viewport_y = document.ViewPortY;
			document.ViewPortY = this.vscroll.Value;

			//
			// Before scrolling we want to destroy the caret, then draw a new one after the scroll
			// the reason for this is that scrolling changes the coordinates of the caret, and we
			// will get tracers if we don't 
			//
			if (Focused)
				document.CaretLostFocus ();

			if (hscroll.Visible) {
				XplatUI.ScrollWindow(this.Handle, new Rectangle(0, 0, ClientSize.Width, ClientSize.Height - hscroll.Height), 0, old_viewport_y - this.vscroll.Value, false);
			} else {
				XplatUI.ScrollWindow(this.Handle, ClientRectangle, 0, old_viewport_y - this.vscroll.Value, false);
			}

			if (Focused)
				document.CaretHasFocus ();

			EventHandler eh = (EventHandler)(Events [VScrolledEvent]);
			if (eh != null)
				eh (this, EventArgs.Empty);
		}

		private void TextBoxBase_MouseMove(object sender, MouseEventArgs e) {
			// FIXME - handle auto-scrolling if mouse is to the right/left of the window
			if (e.Button == MouseButtons.Left && Capture) {
				if (!ClientRectangle.Contains (e.X, e.Y)) {
					if (scroll_timer == null) {
						scroll_timer = new Timer ();
						scroll_timer.Interval = 100;
						scroll_timer.Tick += new EventHandler (ScrollTimerTickHandler);
					}

					if (!scroll_timer.Enabled) {
						scroll_timer.Start ();

						// Force the first tick
						ScrollTimerTickHandler (null, EventArgs.Empty);
					}

					return;
				}

				document.PositionCaret(e.X + document.ViewPortX, e.Y + document.ViewPortY);
				if (click_mode == CaretSelection.Position) {
					document.SetSelectionToCaret(false);
					document.DisplayCaret();
				}
			}
		}
									      
		private void TextBoxBase_FontOrColorChanged(object sender, EventArgs e) {
			if (!richtext) {
				Line	line;

				document.SuspendRecalc ();
				// Font changes apply to the whole document
				for (int i = 1; i <= document.Lines; i++) {
					line = document.GetLine(i);
					LineTag.FormatText(line, 1, line.text.Length, Font,
							ThemeEngine.Current.ResPool.GetSolidBrush(ForeColor),
							null, FormatSpecified.Font | FormatSpecified.Color);
				}
				document.UpdateView (document.GetLine (1), 0);
				document.ResumeRecalc (true);

				// Make sure the caret height is matching the new font height
				document.AlignCaret();
			}
		}

		private void ScrollTimerTickHandler (object sender, EventArgs e)
		{
			Point pt = Cursor.Position;

			pt = PointToClient (pt);

			if (pt.X < ClientRectangle.Left) {
				document.MoveCaret(CaretDirection.CharBackNoWrap);
				document.SetSelectionToCaret(false);
				
				CaretMoved(this, null);
			} else if (pt.X > ClientRectangle.Right) {
				document.MoveCaret(CaretDirection.CharForwardNoWrap);
				document.SetSelectionToCaret(false);
				
				CaretMoved(this, null);
			} else if (pt.Y > ClientRectangle.Bottom) {
				document.MoveCaret(CaretDirection.LineDown);
				document.SetSelectionToCaret(false);
				
				CaretMoved(this, null);
			} else if (pt.Y < ClientRectangle.Top) {
				document.MoveCaret(CaretDirection.LineUp);
				document.SetSelectionToCaret(false);
				
				CaretMoved(this, null);
			}
		}

		/// <summary>Ensure the caret is always visible</summary>
		internal void CaretMoved(object sender, EventArgs e) {
			Point	pos;
			int	height;

			if (canvas_width < 1 || canvas_height < 1)
				return;
			
			pos = document.Caret;

			//Console.WriteLine("Caret now at {0} (Thumb: {1}x{2}, Canvas: {3}x{4}, Document {5}x{6})", pos, hscroll.Value, vscroll.Value, canvas_width, canvas_height, document.Width, document.Height);


			// Horizontal scrolling:
			// If the caret moves to the left outside the visible area, we jump the document into view, not just one
			// character, but 1/3 of the width of the document
			// If the caret moves to the right outside the visible area, we scroll just enough to keep the caret visible

			// Handle horizontal scrolling
			if (document.CaretLine.alignment == HorizontalAlignment.Left) {
				// Check if we moved out of view to the left
				if (pos.X < (document.ViewPortX)) {
					do {
						if ((hscroll.Value - document.ViewPortWidth / 3) >= hscroll.Minimum) {
							hscroll.Value -= document.ViewPortWidth / 3;
						} else {
							hscroll.Value = hscroll.Minimum;
						}
					} while (hscroll.Value > pos.X);
				}

				// Check if we moved out of view to the right
				if ((pos.X >= (document.ViewPortWidth + document.ViewPortX)) && (hscroll.Value != hscroll.Maximum)) {
					if ((pos.X - document.ViewPortWidth + 1) <= hscroll.Maximum) {
						if (pos.X - document.ViewPortWidth >= 0) {
							hscroll.Value = pos.X - document.ViewPortWidth + 1;
						} else {
							hscroll.Value = 0;
						}
					} else {
						hscroll.Value = hscroll.Maximum;
					}
				}
			} else if (document.CaretLine.alignment == HorizontalAlignment.Right) {
//				hscroll.Value = pos.X;

//				if ((pos.X > (this.canvas_width + document.ViewPortX)) && (hscroll.Enabled && (hscroll.Value != hscroll.Maximum))) {
//					hscroll.Value = hscroll.Maximum;
//				}
			} else {
				// FIXME - implement center cursor alignment
			}

			if (!multiline) {
				return;
			}

			// Handle vertical scrolling
			height = document.CaretLine.Height + 1;

			if (pos.Y < document.ViewPortY) {
				vscroll.Value = pos.Y;
			}

			if ((pos.Y + height) > (document.ViewPortY + canvas_height)) {
				vscroll.Value = Math.Max (0, pos.Y - canvas_height + height);
			}
		}

		internal bool Paste(IDataObject clip, DataFormats.Format format, bool obey_length) {
			string		s;

			if (clip == null)
				return false;

			if (format == null) {
				if ((this is RichTextBox) && clip.GetDataPresent(DataFormats.Rtf)) {
					format = DataFormats.GetFormat(DataFormats.Rtf);
				} else if ((this is RichTextBox) && clip.GetDataPresent (DataFormats.Bitmap)) {
					format = DataFormats.GetFormat (DataFormats.Bitmap);
				} else if (clip.GetDataPresent(DataFormats.UnicodeText)) {
					format = DataFormats.GetFormat(DataFormats.UnicodeText);
				} else if (clip.GetDataPresent(DataFormats.Text)) {
					format = DataFormats.GetFormat(DataFormats.Text);
				} else {
					return false;
				}
			} else {
				if ((format.Name == DataFormats.Rtf) && !(this is RichTextBox)) {
					return false;
				}

				if (!clip.GetDataPresent(format.Name)) {
					return false;
				}
			}

			if (format.Name == DataFormats.Rtf) {
				document.undo.BeginUserAction (Locale.GetText ("Paste"));
				((RichTextBox)this).SelectedRtf = (string)clip.GetData(DataFormats.Rtf);
				document.undo.EndUserAction ();
				return true;
			} else if (format.Name == DataFormats.Bitmap) {
				document.undo.BeginUserAction (Locale.GetText ("Paste"));
				document.InsertImage (document.caret.tag, document.caret.pos, (Image) clip.GetData (DataFormats.Bitmap));
				document.MoveCaret (CaretDirection.CharForward);
				document.undo.EndUserAction ();
				return true;
			} else if (format.Name == DataFormats.UnicodeText) {
				s = (string)clip.GetData(DataFormats.UnicodeText);
			} else if (format.Name == DataFormats.Text) {
				s = (string)clip.GetData(DataFormats.Text);
			} else {
				return false;
			}

			if (!obey_length) {
				document.undo.BeginUserAction (Locale.GetText ("Paste"));
				this.SelectedText = s;
				document.undo.EndUserAction ();
			} else {
				if ((s.Length + document.Length) < max_length) {
					document.undo.BeginUserAction (Locale.GetText ("Paste"));
					this.SelectedText = s;
					document.undo.EndUserAction ();
				} else if (document.Length < max_length) {
					document.undo.BeginUserAction (Locale.GetText ("Paste"));
					this.SelectedText = s.Substring (0, max_length - document.Length);
					document.undo.EndUserAction ();
				}
			}

			return true;
		}
		#endregion	// Private Methods

#if NET_2_0
		protected override void OnTextChanged (EventArgs e)
		{
			base.OnTextChanged (e);
		}

		public virtual int GetLineFromCharIndex (int index)
		{
			Line line_out;
			LineTag tag_out;
			int pos;
			
			document.CharIndexToLineTag (index, out line_out, out tag_out, out pos);

			return line_out.LineNo;
		}

		protected override void OnMouseUp (MouseEventArgs e)
		{
			base.OnMouseUp (e);
		}

#endif
	}
}
