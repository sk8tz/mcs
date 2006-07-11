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

using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;

namespace System.Windows.Forms {
	public class ContainerControl : ScrollableControl, IContainerControl {
		private Control		active_control;
		private Control		focused_control;
		private Control		unvalidated_control;
#if NET_2_0
		private SizeF		auto_scale_dimensions;
		private AutoScaleMode	auto_scale_mode;
#endif

		#region Public Constructors
		public ContainerControl() {
			active_control = null;
			focused_control = null;
			unvalidated_control = null;
			ControlRemoved += new ControlEventHandler(OnControlRemoved);
#if NET_2_0
			auto_scale_dimensions = SizeF.Empty;
			auto_scale_mode = AutoScaleMode.None;
#endif
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		[Browsable (false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Control ActiveControl {
			get {
				return active_control;
			}

			set {
				if ((active_control==value) || (value==null)) {
					return;
				}

				
				if (!Contains(value) && this != value) {
					throw new ArgumentException("Not a child control");
				}

				if (value == this) {
					
				}

				// Fire the enter and leave events if possible
				Form form = FindForm ();
				if (form != null) {
					Control common_container = GetCommonContainer (form.ActiveControl, value);
					ArrayList chain = new ArrayList ();
					Control walk = active_control;

					// Generate the leave messages	
					walk = active_control;
					while (walk != common_container) {
						walk.FireLeave ();
						walk = walk.Parent;
					}

					walk = value;
					while (walk != common_container) {
						chain.Add (walk);
						walk = walk.Parent;
					}

					for (int i = chain.Count - 1; i >= 0; i--) {
						walk = (Control) chain [i];
						walk.FireEnter ();
					}
				}

				active_control = value;


				if (this is Form)
					CheckAcceptButton();

				// Scroll control into view
				ScrollControlIntoView(active_control);

				// Let the control know it's selected
//				value.is_selected = true;
//				if (value.IsHandleCreated) {
//					XplatUI.SetFocus (value.window.Handle);
//				}

				SelectChild (value);
			}
		}

		// Just in a separate method to make debugging a little easier,
		// should eventually be rolled into ActiveControl setter
		private Control GetCommonContainer (Control active_control, Control value)
		{
			Control new_container = null;
			Control prev_container = active_control;

			while (prev_container != null) {
				new_container = value.Parent;
				while (new_container != null) {
					if (new_container == prev_container)
						return new_container;
					new_container = new_container.Parent;
				}

				prev_container = prev_container.Parent;
			}

			return null;
		}

#if NET_2_0
		public SizeF AutoScaleDimensions {
			get {
				return auto_scale_dimensions;
			}

			set {
				auto_scale_dimensions = value;
			}
		}

		public SizeF AutoScaleFactor {
			get {
				if (auto_scale_dimensions.IsEmpty) {
					return new SizeF(1f, 1f);
				}
				return new SizeF(CurrentAutoScaleDimensions.Width / auto_scale_dimensions.Width, 
					CurrentAutoScaleDimensions.Height / auto_scale_dimensions.Height);
			}
		}


		[MonoTODO("Call scaling method")]
		public virtual AutoScaleMode AutoScaleMode {
			get {
				return auto_scale_mode;
			}
			set {
				if (auto_scale_mode != value) {
					auto_scale_mode = value;

					// Trigger scaling
				}
			}
		}
#endif // NET_2_0

		[Browsable (false)]
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

#if NET_2_0
		[MonoTODO("Revisit when System.Drawing.GDI.WindowsGraphics.GetTextMetrics is done or come up with other cross-plat avg. font width calc method")]
		public SizeF CurrentAutoScaleDimensions {
			get {
				switch(auto_scale_mode) {
					case AutoScaleMode.Dpi: {
						Bitmap		bmp;
						Graphics	g;
						SizeF		size;

						bmp = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
						g = Graphics.FromImage(bmp);
						size = new SizeF(g.DpiX, g.DpiY);
						g.Dispose();
						bmp.Dispose();
						return size;
					}

					case AutoScaleMode.Font: {
						// http://msdn2.microsoft.com/en-us/library/system.windows.forms.containercontrol.currentautoscaledimensions(VS.80).aspx
						// Implement System.Drawing.GDI.WindowsGraphics.GetTextMetrics first...
						break;
					}
				}

				return auto_scale_dimensions;
			}
		}
#endif

		[Browsable (false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
		[MonoTODO]
		static bool ValidateWarned;
		public bool Validate() {
			//throw new NotImplementedException();
			if (!ValidateWarned) {
				Console.WriteLine("ContainerControl.Validate is not yet implemented");
				ValidateWarned = true;
			}
			return true;
		}

		bool IContainerControl.ActivateControl(Control control) {
			return Select(control);
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void AdjustFormScrollbars(bool displayScrollbars) {
			base.AdjustFormScrollbars(displayScrollbars);
		}

		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
		}

		// LAMESPEC This used to be documented, but it's not in code 
		// and no longer listed in MSDN2
		// [EditorBrowsable (EditorBrowsableState.Advanced)]
		// protected override void OnControlRemoved(ControlEventArgs e) {
		private void OnControlRemoved(object sender, ControlEventArgs e) {
			if (e.Control == this.unvalidated_control) {
				this.unvalidated_control = null;
			}

			if (e.Control == this.active_control) {
				this.unvalidated_control = null;
			}

			// base.OnControlRemoved(e);
		}

		protected override void OnCreateControl() {
			base.OnCreateControl();
			// MS seems to call this here, it gets the whole databinding process started
			OnBindingContextChanged (EventArgs.Empty);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override bool ProcessDialogChar(char charCode) {
			if (GetTopLevel()) {
				if (ProcessMnemonic(charCode)) {
					return true;
				}
			}
			return base.ProcessDialogChar(charCode);
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
			bool	wrapped;
			Control	c;

			wrapped = false;
			c = active_control;

			do {
				c = GetNextControl(c, true);
				if (c != null) {
					// This is stupid. I want to be able to call c.ProcessMnemonic directly
					if (c.ProcessControlMnemonic(charCode)) {
						return(true);
					}
					continue;
				} else {
					if (wrapped) {
						break;
					}
					wrapped = true;
				}
			} while (c != active_control);
			
			return false;
		}

		protected virtual bool ProcessTabKey(bool forward) {
			return SelectNextControl(active_control, forward, true, true, false);
		}

		protected override void Select(bool directed, bool forward) {

			int	index;
			bool	result;

			if (!directed) {
				// Select this control
				Select(this);
				return;
			}

			if (parent == null) {
				return;
			}

			// FIXME - this thing is doing the wrong stuff, needs to be similar to SelectNextControl

			index = parent.child_controls.IndexOf(this);
			result = false;

			do {
				if (forward) {
					if ((index+1) < parent.child_controls.Count) {
						index++;
					} else {
						index = 0;
					}
				} else {
					if (index>0) {
						index++;
					} else {
						index = parent.child_controls.Count-1;
					}
				}
				result = Select(parent.child_controls[index]);
			} while (!result && parent.child_controls[index] != this);
		}

		protected virtual void UpdateDefaultButton() {
			// MS Internal
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void WndProc(ref Message m) {
			switch ((Msg) m.Msg) {
/*
			case Msg.WM_LBUTTONDOWN:
				OnMouseDown (new MouseEventArgs (FromParamToMouseButtons ((int) m.WParam.ToInt32()), 
				        mouse_clicks, LowOrder ((int) m.LParam.ToInt32 ()),
					HighOrder ((int) m.LParam.ToInt32 ()), 0));
					return;
*/
			case Msg.WM_SETFOCUS:
				if (active_control == null)
					SelectNextControl (null, true, true, true, false);
				break;
			case Msg.WM_KILLFOCUS:
				break;
			}
			base.WndProc(ref m);
		}
		#endregion	// Protected Instance Methods

		#region Internal Methods
		internal virtual void CheckAcceptButton()
		{
			// do nothing here, only called if it is a Form
		}
		#endregion	// Internal Methods
	}
}
