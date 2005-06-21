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
// Copyright (c) 2004-2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//
//

// NOT COMPLETE

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;

namespace System.Windows.Forms {
	public class TextBox : TextBoxBase {
		#region Local Variables
		internal char			password_char;
		#endregion	// Local Variables

		#region Public Constructors
		public TextBox() {
			accepts_return = false;
			password_char = '\u25cf';
			scrollbars = RichTextBoxScrollBars.None;
			alignment = HorizontalAlignment.Left;
			this.LostFocus +=new EventHandler(TextBox_LostFocus);
			this.BackColor = ThemeEngine.Current.ColorWindow;
			this.ForeColor = ThemeEngine.Current.ColorWindowText;
		}
		#endregion	// Public Constructors


		#region Private & Internal Methods
		private void TextBox_LostFocus(object sender, EventArgs e) {
			has_focus = false;
			Invalidate();
		}
		#endregion	// Private & Internal Methods

		#region Public Instance Properties
		[DefaultValue(false)]
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
		[DefaultValue("")]
		public char PasswordChar {
			get {
				return password_char;
			}

			set {
				if (value != password_char) {
					password_char = value;
				}
			}
		}

		[DefaultValue(ScrollBars.None)]
		[Localizable(true)]
		public ScrollBars ScrollBars {
			get {
				return (ScrollBars)scrollbars;
			}

			set {
				if (value != (ScrollBars)scrollbars) {
					scrollbars = (RichTextBoxScrollBars)value;
					base.CalculateScrollBars();
				}
			}
		}

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
		public HorizontalAlignment TextAlign {
			get {
				return alignment;
			}

			set {
				if (value != alignment) {
					alignment = value;

					// MS word-wraps if alignment isn't left
					if (alignment != HorizontalAlignment.Left) {
						document.Wrap = true;
					} else {
						document.Wrap = word_wrap;
					}

					for (int i = 1; i <= document.Lines; i++) {
						document.GetLine(i).Alignment = value;
					}
					document.RecalculateDocument(CreateGraphics());
					OnTextAlignChanged(EventArgs.Empty);
				}
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Methods
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}

		protected override ImeMode DefaultImeMode {
			get {
				return base.DefaultImeMode;
			}
		}
		#endregion	// Protected Instance Methods

		#region Protected Instance Methods
		protected override bool IsInputKey(Keys keyData) {
			return base.IsInputKey (keyData);
		}

		protected override void OnGotFocus(EventArgs e) {
			has_focus=true;
			Invalidate();
			base.OnGotFocus (e);
		}

		protected override void OnHandleCreated(EventArgs e) {
			base.OnHandleCreated (e);
		}

		protected override void OnMouseUp(MouseEventArgs e) {
			base.OnMouseUp (e);
		}

		protected virtual void OnTextAlignChanged(EventArgs e) {
			if (TextAlignChanged != null) {
				TextAlignChanged(this, e);
			}
		}

		protected override void WndProc(ref Message m) {
			base.WndProc(ref m);
		}
		#endregion	// Protected Instance Methods

		#region Events
		public event EventHandler TextAlignChanged;
		#endregion	// Events
	}
}
