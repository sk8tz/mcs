//
// System.Windows.Forms.RichTextBox.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002/3 Ximian, Inc
//

//
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
using System.Drawing;
using System.IO;

namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>

    public class RichTextBox : TextBoxBase 
	{
		private IntPtr	handleCommCtrlLib;

		//
		//  --- Constructor
		//
		[MonoTODO]
		public RichTextBox()
		{
			handleCommCtrlLib = Win32.LoadLibraryA("riched20.dll");
		}

		#region Properties
		//
		//  --- Public Properties
		//
		[MonoTODO]
		public override bool AllowDrop {
			get {
				//FIXME:
				return base.AllowDrop;
			}
			set {
				//FIXME:
				base.AllowDrop = value;
			}
		}
		[MonoTODO]
		public override bool AutoSize {
			get {
				//FIXME:
				return base.AutoSize;;
			}
			set {
				//FIXME:
				base.AutoSize = value;
			}
		}
		[MonoTODO]
		public bool AutoWordSelection {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override Image BackgroundImage {
			get {
				//FIXME:
				return base.BackgroundImage;
			}
			set {
				//FIXME:
				base.BackgroundImage = value;
			}
		}
		[MonoTODO]
		public int BulletIndent {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool CanRedo {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool DetectUrls {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override Font Font {
			get {
				//FIXME:
				return base.Font;
			}
			set {
				//FIXME:
				base.Font = value;
			}
		}
		[MonoTODO]
		public override Color ForeColor {
			get {
				//FIXME:
				return base.ForeColor;
			}
			set {
				//FIXME:
				base.ForeColor = value;
			}
		}
		[MonoTODO]
		public override int MaxLength {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override bool Multiline {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public string RedoActionName {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int RightMargin {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public string Rtf {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public RichTextBoxScrollBars ScrollBars {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public string SelectedRtf {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override string SelectedText {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public HorizontalAlignment SelectionAlignment {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool SelectionBullet {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int SelectionCharOffset {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public Color SelectionColor {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public Font SelectionFont {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int SelectionHangingIndent {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int SelectionIndent {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override int SelectionLength {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool SelectionProtected {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int SelectionRightIndent {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int[] SelectionTabs {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public RichTextBoxSelectionTypes SelectionType {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public bool ShowSelectionMargin {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override string Text {
			get {
				return base.Text;
			}
			set {
				base.Text = value;
			}
		}
		[MonoTODO]
		public override int TextLength {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public string UndoActionName {
			get {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public float ZoomFactor {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		#endregion

		#region Public Methods
		//
		//  --- Public Methods
		//

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			Win32.FreeLibrary(handleCommCtrlLib);
		}


		[MonoTODO]
		public bool CanPaste(DataFormats.Format clipFormat)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Find(char[] characterSet)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int Find(string str)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int Find(char[] characterSet, int start)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int Find(string str, RichTextBoxFinds options)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int Find(char[] characterSet, int start, int end)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int Find(string str, int start, RichTextBoxFinds options)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int Find(string str, int val1, int val2, RichTextBoxFinds finds)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public char GetCharFromPosition(Point pt)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public int GetLineFromCharIndex(int index)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public Point GetPositionFromCharIndex(int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void LoadFile(string path)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void LoadFile(Stream path, RichTextBoxStreamType fileType)
		{
			throw new NotImplementedException ();
		}
		

		[MonoTODO]
		public void Paste(DataFormats.Format clipFormat)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Redo()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SaveFile(string path)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void SaveFile(Stream path, RichTextBoxStreamType fileType)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public void SaveFile(string path, RichTextBoxStreamType fileType)
		{
			throw new NotImplementedException ();
		}
		#endregion

		#region Public Events
		//
		//  --- Public Events
		//
		public event ContentsResizedEventHandler ContentsResized;
		public event EventHandler HScroll;
		public event EventHandler ImeChange;
		public event LinkClickedEventHandler LinkClicked;
		public event EventHandler Protected;
		public event EventHandler SelectionChanged;
		public event EventHandler VScroll;
		#endregion

		#region Protected Properties
		//
		//  --- Protected Properties
		//
		[MonoTODO]
		protected override CreateParams CreateParams 
		{
			get {
				CreateParams createParams = base.CreateParams;

				createParams.ClassName = "RichEdit20A";
				createParams.Style = (int) ( WindowStyles.WS_CHILD | WindowStyles.WS_VISIBLE);
				return createParams;
			}		
		}

		[MonoTODO]
		protected override Size DefaultSize {
			get {
				return new System.Drawing.Size(300,300);
			}
		}
		#endregion

		#region Protected Methods
		//
		//  --- Protected Methods
		//

		[MonoTODO]
		protected virtual object CreateRichEditOleCallback()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnBackColorChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnContentsResized(ContentsResizedEventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void  OnContextMenuChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void  OnHandleCreated(EventArgs e)
		{
		}

		[MonoTODO]
		protected override void  OnHandleDestroyed(EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnHScroll(EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnImeChange(EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnLinkClicked(LinkClickedEventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnProtected(EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnRightToLeftChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnSelectionChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnSystemColorsChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void OnTextChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnVScroll(EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void WndProc(ref Message m)
		{
			throw new NotImplementedException ();
		}
		#endregion
	 }
}
