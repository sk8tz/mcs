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
//	Peter Bartok	pbartok@novell.com
//
//


// NOT COMPLETE

namespace System.Windows.Forms {
	public class ContainerControl : ScrollableControl, IContainerControl {
		private Control active_control;
		private Control focused_control;

		#region Public Constructors
		public ContainerControl() {
			active_control = null;
			focused_control = null;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public Control ActiveControl {
			get {
				return active_control;
			}

			set {
				if ((active_control==value) || (value==null)) {
					return;
				}

				active_control = value;

				if (!Contains(value)) {
					throw new ArgumentException("Not a child control");
				}

				XplatUI.SetFocus(active_control.window.Handle);
			}
		}

		public override BindingContext BindingContext {
			get {
				if (base.BindingContext == null) {
					base.BindingContext = new BindingContext();
				}
				return base.BindingContext;
			}

			set {
				base.BindingContext = value;
			}
		}

		public Form ParentForm {
			get {
				Control parent;

				parent = this.parent;

				while (parent != null) {
					if (parent is Form) {
						return (Form)parent;
					}
					parent = parent.parent;
				}

				return null;
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Methods
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}
		#endregion	// Public Instance Methods

		#region Public Instance Methods
		public bool Validate() {
			throw new NotImplementedException();
		}

		bool IContainerControl.ActivateControl(Control control) {
			throw new NotImplementedException();
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override void AdjustFormScrollbars(bool displayScrollbars) {
		}

		protected override void Dispose(bool disposing) {
		}

		protected override void OnControlRemoved(ControlEventArgs e) {
		}

		protected override void OnCreateControl() {
		}

		protected override bool ProcessDialogChar(char charCode) {
			throw new NotImplementedException();
		}

		protected override bool ProcessDialogKey(Keys keyData) {
			Keys	key;
			bool	forward;

			key = keyData & Keys.KeyCode;
			forward = true;

			switch (key) {
				case Keys.Tab: {
					if (ProcessTabKey((Control.ModifierKeys & Keys.Shift) == 0)) {
						return true;
					}
					break;
				}

				case Keys.Left: {
					forward = false;
					goto case Keys.Down;
				}

				case Keys.Up: {
					forward = false;
					goto case Keys.Down;
				}

				case Keys.Right: {
					goto case Keys.Down;
				}
				case Keys.Down: {
					if (SelectNextControl(active_control, forward, false, false, true)) {
						return true;
					}
					break;
				} 


			}
			return base.ProcessDialogKey(keyData);

		}

		protected override bool ProcessMnemonic(char charCode) {
			throw new NotImplementedException();
		}

		protected virtual bool ProcessTabKey(bool forward) {
			return SelectNextControl(active_control, forward, true, true, true);
		}

		protected override void Select(bool directed, bool forward) {
			base.Select(directed, forward);
		}

		protected virtual void UpdateDefaultButton() {
		}

		protected override void WndProc(ref Message m) {
			base.WndProc(ref m);
		}
		#endregion	// Protected Instance Methods
	}
}
