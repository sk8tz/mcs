//
// System.Windows.Forms.TextBoxBase
//
// Author:
//   stubbed out by Jackson Harper (jackson@latitudegeo.com)
//	Dennis Hayes (dennish@raytek.com)
//  Remco de Jong (rdj@rdj.cg.nu)
//	Joel Basson  (jstrike@mweb.co.za)
// (C) 2002 Ximian, Inc
//
using System.Drawing;

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

        public class TextBoxBase : Control {

		private int maxlength = 0;
		private Gtk.TextTagTable tagtable;
		private Gtk.TextBuffer textbuffer = null;
		//
		//  --- Public Properties
		//

		public TextBoxBase () {
		}

		protected Gtk.TextBuffer TextBuffer {
			get {
				if (textbuffer == null) {
					tagtable = new Gtk.TextTagTable ();
					textbuffer = new Gtk.TextBuffer (tagtable);
					// attach events

					textbuffer.ModifiedChanged += new EventHandler (modified_changed_cb);
				}
	
				return textbuffer;
			}
		}

		protected override void OnTextChanged(EventArgs e)
		{
			TextBuffer.SetText(Text);
		}

		public override string Text {
			get{
				return TextBuffer.Text;
			}
			set{
				TextBuffer.SetText(value);
			}
		}

		[MonoTODO]
		public bool AcceptsTab {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public virtual bool AutoSize {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
/*
		[MonoTODO]
		public override Image BackgroundImage {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public BorderStyle BorderStyle {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
*/		[MonoTODO]
		public bool CanUndo {
			get
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool HideSelection {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public string[] Lines {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				Clear ();
				foreach (String s in value) {
					AppendText (s + "\n");
				}
			}
		}
		[MonoTODO]
		public virtual int MaxLength {
			get
			{
				return maxlength;
			}
			set
			{
				maxlength = value;
			}
		}

		public bool Modified {
			get
			{
				return TextBuffer.Modified;
			}
			set
			{
				if (TextBuffer.Modified != value) { // only call if it has really been changed since this will trigger an event, is this correct behavior?
					TextBuffer.Modified = value;
				}
			}
		}
		[MonoTODO]
		public virtual bool Multiline {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int PreferredHeight {
			get
			{
				throw new NotImplementedException ();
			}
		}

		public virtual bool ReadOnly {
			// needs to be overwritten by child classes
			get { return false; }
			set {}
		}

		[MonoTODO]
		public virtual string SelectedText {
			get
			{
				String selection = "";
				Gtk.TextIter start = new Gtk.TextIter ();
				Gtk.TextIter end = new Gtk.TextIter ();
		
				if (TextBuffer.GetSelectionBounds(out start, out end))
					selection = TextBuffer.GetText(start, end, true);
					
				return selection;
			}
			set
			{
				// paste text over selection
			}
		}
		[MonoTODO]
		public virtual int SelectionLength {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int SelectionStart {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public virtual int TextLength  {

			get
			{
				return Text.Length;
			}
		}
		[MonoTODO]

		public virtual bool WordWrap {
			get
			{
				return false;
			}
			set
			{
			}
		}
		
		// --- Public Methods
		
		public void AppendText(string text) 
		{
			Text += text;
		}

		public void Clear()
		{
			TextBuffer.SetText("");
		}
		[MonoTODO]
		public void ClearUndo()
		{
			throw new NotImplementedException ();
		}

		public void Copy()
		{
			TextBuffer.CopyClipboard (Gtk.Clipboard.Get (Gdk.Atom.Intern("GDK_NONE", false)));
		}

		public void Cut()
		{
			TextBuffer.CutClipboard (Gtk.Clipboard.Get (Gdk.Atom.Intern("GDK_NONE", false)), true);
		}

		public void Paste()
		{
			// Wait for my gtk-sharp patch
			// TextBuffer.PasteClipboard(Gtk.Clipboard.Get (Gdk.Atom.Intern("GDK_NONE", false)), true);
		}

		[MonoTODO]
		public void ScrollToCaret()
		{
			throw new NotImplementedException ();
		}

		public void Select(int start, int length) 
		{
			Gtk.TextIter iter_start, iter_end;
			iter_start = TextBuffer.GetIterAtOffset (start);
			iter_end = TextBuffer.GetIterAtOffset (length);

			TextBuffer.MoveMark(TextBuffer.InsertMark , iter_end);
			TextBuffer.MoveMark (TextBuffer.SelectionBound, iter_start);
		}

		public void SelectAll()
		{
			Gtk.TextIter iter_start, iter_end;
			TextBuffer.GetBounds (out iter_start, out iter_end);

			TextBuffer.MoveMark(TextBuffer.InsertMark , iter_end);
			TextBuffer.MoveMark (TextBuffer.SelectionBound, iter_start);

		}

		public override string ToString()
		{
			// MS.NET returns "System.Windows.Forms.TextBox, Text: textBox1"
			return "System.Windows.Forms.TextBox, Text: " + this.Name;
			//throw new NotImplementedException ();
		}
		[MonoTODO]
		public void Undo()
		{
			throw new NotImplementedException ();
		}
		
		// --- Internal Events (Gtk)

		internal protected void modified_changed_cb (object o, EventArgs args)
		{
			OnModifiedChanged (EventArgs.Empty);
		}

		// --- Public Events
		
		[MonoTODO]
		public event EventHandler AcceptsTabChanged;
		[MonoTODO]
		public event EventHandler AutoSizeChanged;
		[MonoTODO]
		public event EventHandler BorderStyleChanged;
		[MonoTODO]
		//public override event EventHandler Click;
		[MonoTODO]
		public event EventHandler HideSelectionChanged;
		public event EventHandler ModifiedChanged;
		[MonoTODO]
		public event EventHandler MultilineChanged;
		public event EventHandler ReadOnlyChanged;

	        // --- Protected Properties
/*        
		[MonoTODO]
		protected override CreateParams CreateParams {
			get
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		protected override Size DefaultSize {
			get
			{
				throw new NotImplementedException ();
			}
		}
		
		// --- Protected Methods
		
		[MonoTODO]
		protected override void CreateHandle()
		{
			throw new NotImplementedException ();
		}
*/
		[MonoTODO]
/*		protected override bool IsInputKey(Keys keyData)
		{
			throw new NotImplementedException ();
		}
*/		[MonoTODO]
		protected virtual void OnAcceptsTabChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnAutoSizeChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnBorderStyleChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}

/*
		[MonoTODO]
		protected override void OnFontChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnHandleDestroyed(EventArgs e)
		{
			throw new NotImplementedException ();
		}
*/
		[MonoTODO]
		protected virtual void OnHideSelectionChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual void OnModifiedChanged(EventArgs e)
		{
			if (ModifiedChanged != null)
			 ModifiedChanged (this, e);
		}
		[MonoTODO]
		protected virtual void OnMultilineChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}

		protected virtual void OnReadOnlyChanged(EventArgs e)
		{
			if (ReadOnlyChanged != null)
			 ReadOnlyChanged (this, e);
		}
/*		[MonoTODO]
		protected override bool ProcessDialogKey(Keys keyData)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void WndProc(ref Message m)
		{
			throw new NotImplementedException ();
		}
*/	}
}
