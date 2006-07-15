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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//
//

// This is an internal class that allows us to use a textbox for label editing
// in the tree and in listview.  The textbox will make itself invisible when
// the user pressed the enter key

namespace System.Windows.Forms {

	internal class LabelEditTextBox : FixedSizeTextBox {

		public LabelEditTextBox () : base (true, true)
		{
		}

		protected override bool IsInputKey (Keys key_data)
		{
			if ((key_data & Keys.Alt) == 0) {
				switch (key_data & Keys.KeyCode) {
				case Keys.Enter:
					return true;
				}
			}
			return base.IsInputKey (key_data);
		}

		protected override void OnKeyDown (KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return && Visible) {
				this.Visible = false;
				OnEditingFinished (e);
			}
		}

		protected override void OnLostFocus (EventArgs e)
		{
			if (Visible) {
				OnEditingFinished (e);
			}
		}

		protected void OnEditingFinished (EventArgs e)
		{
			if (EditingFinished != null)
				EditingFinished (this, EventArgs.Empty);
		}

		public event EventHandler EditingFinished;
	}
}

