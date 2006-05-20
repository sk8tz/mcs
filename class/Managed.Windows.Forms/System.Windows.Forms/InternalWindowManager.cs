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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//
//


using System;
using System.Drawing;
using System.Runtime.InteropServices;


namespace System.Windows.Forms {

	internal class InternalWindowManager {

		private static Color titlebar_color;

		private Size MinTitleBarSize = new Size (115, 25);

		internal Form form;

		internal TitleButton close_button;
		internal TitleButton maximize_button;
		internal TitleButton minimize_button;
		protected Rectangle icon_rect;
		
		private TitleButton [] title_buttons = new TitleButton [3];
		
		// moving windows
		internal Point start;
		internal State state;
		private FormPos sizing_edge;
		internal Rectangle virtual_position;
		private Rectangle prev_virtual_position;

		public class TitleButton {
			public Rectangle Rectangle;
			public ButtonState State;
			public CaptionButton Caption;
			public EventHandler Clicked;

			public TitleButton (CaptionButton caption, EventHandler clicked)
			{
				Caption = caption;
				Clicked = clicked;
			}
		}

		public enum State {
			Idle,
			Moving,
			Sizing,
		}

		[Flags]
		private enum FormPos {
			None,

			TitleBar = 1,

			Top = 2,
			Left = 4,
			Right = 8,
			Bottom = 16,

			TopLeft = Top | Left,
			TopRight = Top | Right,

			BottomLeft = Bottom | Left,
			BottomRight = Bottom | Right,

			AnyEdge = Top | Left | Right | Bottom,
		}

		public InternalWindowManager (Form form)
		{
			titlebar_color = Color.FromArgb (255, 0, 0, 255);
			this.form = form;

			form.SizeChanged += new EventHandler (FormSizeChangedHandler);

			CreateButtons ();
		}

		public Form Form {
			get { return form; }
		}

		public Rectangle CloseButtonRect {
			get { return close_button.Rectangle; }
			set { close_button.Rectangle = value; }
		}

		public Rectangle MinimizeButtonRect {
			get { return minimize_button.Rectangle; }
			set { minimize_button.Rectangle = value; }
		}

		public Rectangle MaximizeButtonRect {
			get { return maximize_button.Rectangle; }
			set { maximize_button.Rectangle = value; }
		}

		public Rectangle IconRect {
			get { return icon_rect; }
			set { value = icon_rect; }
		}

		public int IconWidth {
			get { return TitleBarHeight - 5; }
		}

		public bool HandleMessage (ref Message m)
		{
			switch ((Msg)m.Msg) {


				// The mouse handling messages are actually
				// not WM_NC* messages except for the first button and NCMOVEs
				// down because we capture on the form

			case Msg.WM_MOUSEMOVE:
				return HandleMouseMove (form, ref m);

			case Msg.WM_LBUTTONUP:
				HandleLButtonUp (ref m);
				break;

			case Msg.WM_RBUTTONDOWN:
			case Msg.WM_LBUTTONDOWN:
				return HandleButtonDown (ref m);

			case Msg.WM_NCHITTEST:
				int x = Control.LowOrder ((int) m.LParam.ToInt32 ());
				int y = Control.HighOrder ((int) m.LParam.ToInt32 ());

				form.PointToClient (ref x, ref y);
				y += TitleBarHeight;

				FormPos pos = FormPosForCoords (x, y);
				
				if (pos == FormPos.TitleBar) {
					m.Result = new IntPtr ((int) HitTest.HTCAPTION);
					return true;
				}

				if (!IsSizable)
					return false;

				switch (pos) {
				case FormPos.Top:
					m.Result = new IntPtr ((int) HitTest.HTTOP);
					break;
				case FormPos.Left:
					m.Result = new IntPtr ((int) HitTest.HTLEFT);
					break;
				case FormPos.Right:
					m.Result = new IntPtr ((int) HitTest.HTRIGHT);
					break;
				case FormPos.Bottom:
					m.Result = new IntPtr ((int) HitTest.HTBOTTOM);
					break;
				case FormPos.TopLeft:
					m.Result = new IntPtr ((int) HitTest.HTTOPLEFT);
					break;
				case FormPos.TopRight:
					m.Result = new IntPtr ((int) HitTest.HTTOPRIGHT);
					break;
				case FormPos.BottomLeft:
					m.Result = new IntPtr ((int) HitTest.HTBOTTOMLEFT);
					break;
				case FormPos.BottomRight:
					m.Result = new IntPtr ((int) HitTest.HTBOTTOMRIGHT);
					break;
				default:
					// We return false so that DefWndProc handles things
					return false;
				}
				return true;

			case Msg.WM_NCMOUSEMOVE:
				return HandleNCMouseMove (form, ref m);

			case Msg.WM_NCLBUTTONUP:
				return HandleNCLButtonUp (ref m);

			case Msg.WM_NCLBUTTONDOWN:
				return HandleNCLButtonDown (ref m);

			case Msg.WM_MOUSE_LEAVE:
				FormMouseLeave (ref m);
				break;

			case Msg.WM_NCCALCSIZE:
				XplatUIWin32.NCCALCSIZE_PARAMS	ncp;

				if (m.WParam == (IntPtr) 1) {
					ncp = (XplatUIWin32.NCCALCSIZE_PARAMS) Marshal.PtrToStructure (m.LParam,
							typeof (XplatUIWin32.NCCALCSIZE_PARAMS));

					int bw = ThemeEngine.Current.ManagedWindowBorderWidth (this);

					if (HasBorders) {
						ncp.rgrc1.top += TitleBarHeight + bw;
						ncp.rgrc1.bottom -= bw;
						ncp.rgrc1.left += bw;
						ncp.rgrc1.right -= bw;
					}

					Marshal.StructureToPtr(ncp, m.LParam, true);
				}

				break;

			case Msg.WM_NCPAINT:
				PaintEventArgs pe;
				pe = XplatUI.PaintEventStart(m.HWnd, false);
				ThemeEngine.Current.DrawManagedWindowDecorations (pe.Graphics, pe.ClipRectangle, this);
//				PaintWindowDecorations (pe);
				XplatUI.PaintEventEnd(m.HWnd, false);

				
				return true;
			}
			return false;
		}

		public virtual void UpdateBorderStyle (FormBorderStyle border_style)
		{
			XplatUI.SetBorderStyle (form.Handle, border_style);

			if (ShouldRemoveWindowManager (border_style)) {
				form.RemoveWindowManager ();
				return;
			}
				
			CreateButtons ();
		}

		public void HandleMenuMouseDown (MainMenu menu, int x, int y)
		{
			Point pt = MenuTracker.ScreenToMenu (menu, new Point (x, y));

			foreach (TitleButton button in title_buttons) {
				if (button != null && button.Rectangle.Contains (pt)) {
					button.Clicked (this, EventArgs.Empty);
					button.State = ButtonState.Pushed;
					return;
				}
			}
		}

		public virtual void SetWindowState (FormWindowState old_state, FormWindowState window_state)
		{
		}

		public virtual FormWindowState GetWindowState ()
		{
			return form.window_state;
		}

		public virtual void PointToClient (ref int x, ref int y)
		{
			// toolwindows stay in screencoords
		}

		public virtual void PointToScreen (ref int x, ref int y)
		{
			XplatUI.ClientToScreen (form.Handle, ref x, ref y);
		}

		protected virtual bool ShouldRemoveWindowManager (FormBorderStyle style)
		{
			return style != FormBorderStyle.FixedToolWindow && style != FormBorderStyle.SizableToolWindow;
		}

		protected virtual void Activate ()
		{
			// Hack to get a paint
			NativeWindow.WndProc (form.Handle, Msg.WM_NCPAINT, IntPtr.Zero, IntPtr.Zero);
			form.Refresh ();
		}

		public virtual bool IsActive ()
		{
			return true;
		}

		private void FormSizeChangedHandler (object sender, EventArgs e)
		{
			ThemeEngine.Current.ManagedWindowSetButtonLocations (this);
		}

		private void CreateButtons ()
		{
			switch (form.FormBorderStyle) {
			case FormBorderStyle.None:
				close_button = null;
				minimize_button = null;
				maximize_button = null;
				break;
			case FormBorderStyle.FixedSingle:
			case FormBorderStyle.Fixed3D:
			case FormBorderStyle.FixedDialog:
			case FormBorderStyle.Sizable:
				close_button = new TitleButton (CaptionButton.Close, new EventHandler (CloseClicked));
				minimize_button = new TitleButton (CaptionButton.Minimize, new EventHandler (MinimizeClicked));
				maximize_button = new TitleButton (CaptionButton.Maximize, new EventHandler (MaximizeClicked));
				break;
			case FormBorderStyle.FixedToolWindow:
			case FormBorderStyle.SizableToolWindow:
				close_button = new TitleButton (CaptionButton.Close, new EventHandler (CloseClicked));
				break;
			}

			title_buttons [0] = close_button;
			title_buttons [1] = minimize_button;
			title_buttons [2] = maximize_button;
		}

		protected virtual bool HandleButtonDown (ref Message m)
		{
			Activate ();
			return false;
		}

		protected virtual bool HandleNCLButtonDown (ref Message m)
		{
			Activate ();

			int x = Control.LowOrder ((int) m.LParam.ToInt32 ());
			int y = Control.HighOrder ((int) m.LParam.ToInt32 ());

			start = Cursor.Position;
			virtual_position = form.Bounds;

			form.PointToClient (ref x, ref y);
			// Need to adjust because we are in NC land
			y += TitleBarHeight;
			FormPos pos = FormPosForCoords (x, y);

			if (pos == FormPos.TitleBar) {
				HandleTitleBarDown (x, y);
				return true;
			}

			if (IsSizable) {
				SetCursorForPos (pos);
			
				if ((pos & FormPos.AnyEdge) == 0)
					return false;

				state = State.Sizing;
				sizing_edge = pos;
				form.Capture = true;
				return true;
			}

			return false;
		}

		protected virtual void HandleTitleBarDown (int x, int y)
		{
			foreach (TitleButton button in title_buttons) {
				if (button != null && button.Rectangle.Contains (x, y)) {
					button.State = ButtonState.Pushed;
					return;
				}
			}

			if (IsMaximized)
				return;

			state = State.Moving;
			form.Capture = true;
		}

		private bool HandleMouseMove (Form form, ref Message m)
		{
			switch (state) {
			case State.Moving:
				HandleWindowMove (m);
				return true;
			case State.Sizing:
				HandleSizing (m);
				return true;
			}

			/*
			if (IsSizable) {
				int x = Control.LowOrder ((int) m.LParam.ToInt32 ());
				int y = Control.HighOrder ((int) m.LParam.ToInt32 ());
				FormPos pos = FormPosForCoords (x, y);
				Console.WriteLine ("position:   " + pos);
				SetCursorForPos (pos);

				ClearVirtualPosition ();
				state = State.Idle;
			}
			*/
			
			return false;
		}

		private bool HandleNCMouseMove (Form form, ref Message m)
		{
			int x = Control.LowOrder ((int) m.LParam.ToInt32 ());
			int y = Control.HighOrder ((int) m.LParam.ToInt32 ());
			
			if (IsSizable) {
				form.PointToClient (ref x, ref y);
				y += TitleBarHeight;
			}

			return false;
		}

		private void FormMouseLeave (ref Message m)
		{
			form.ResetCursor ();
		}

		private void SetCursorForPos (FormPos pos)
		{
			switch (pos) {
			case FormPos.TopLeft:
			case FormPos.BottomRight:
				form.Cursor = Cursors.SizeNWSE;
				break;
			case FormPos.TopRight:
			case FormPos.BottomLeft:
				form.Cursor = Cursors.SizeNESW;
				break;
			case FormPos.Top:
			case FormPos.Bottom:
				form.Cursor = Cursors.SizeNS;
				break;
			case FormPos.Left:
			case FormPos.Right:
				form.Cursor = Cursors.SizeWE;
				break;
			default:
				form.ResetCursor ();
				break;
			}
		}
	
		protected virtual void HandleWindowMove (Message m)
		{
			Point move = MouseMove (m);

			UpdateVP (virtual_position.X + move.X, virtual_position.Y + move.Y,
					virtual_position.Width, virtual_position.Height);
		}

		private void HandleSizing (Message m)
		{
			Point move = MouseMove (m);
			Rectangle pos = virtual_position;
			int bw = ThemeEngine.Current.ManagedWindowBorderWidth (this);
			int mw = MinTitleBarSize.Width + (bw * 2);
			int mh = MinTitleBarSize.Height + (bw * 2);
			
			if ((sizing_edge & FormPos.Top) != 0) {
				int height = form.Height - move.Y;
				if (height <= mh) {
					move.Y += height - mh;
					height = mh;
				}
				pos.Y = form.Top + move.Y;
				pos.Height = height;
			} else if ((sizing_edge & FormPos.Bottom) != 0) {
				int height = form.Height + move.Y;
				if (height <= mh)
					move.Y -= height - mh;
				pos.Height = form.Height + move.Y;
			}

			if ((sizing_edge & FormPos.Left) != 0) {
				int width = form.Width - move.X;
				if (width <= mw) {
					move.X += width - mw;
					width = mw;
				}
				pos.X = form.Left + move.X;
				pos.Width = width;
			} else if ((sizing_edge & FormPos.Right) != 0) {
				int width = form.Width + move.X;
				if (width <= mw)
					move.X -= width - mw;
				pos.Width = form.Width + move.X;
			}

			UpdateVP (pos);
		}

		public bool IsMaximized {
			get { return GetWindowState () == FormWindowState.Maximized; }
		}

		public bool IsSizable {
			get {
				switch (form.FormBorderStyle) {
				case FormBorderStyle.Sizable:
				case FormBorderStyle.SizableToolWindow:
					return true;
				default:
					return false;
				}
			}
		}

		public bool HasBorders {
			get {
				return form.FormBorderStyle != FormBorderStyle.None;
			}
		}

		public bool IsToolWindow {
			get {
				if (form.FormBorderStyle == FormBorderStyle.SizableToolWindow ||
						form.FormBorderStyle == FormBorderStyle.FixedToolWindow)
					return true;
				return false;
			}
		}

		public int TitleBarHeight {
			get {
				if (IsToolWindow)
					return SystemInformation.ToolWindowCaptionHeight;
				if (form.FormBorderStyle == FormBorderStyle.None)
					return 0;
				return SystemInformation.CaptionHeight;
			}
		}

		public Size ButtonSize {
			get {
				int height = TitleBarHeight;
				if (IsToolWindow)
					return new Size (SystemInformation.ToolWindowCaptionButtonSize.Width - 2,
							height - 5);
				if (form.FormBorderStyle == FormBorderStyle.None)
					return Size.Empty;
				return new Size (SystemInformation.CaptionButtonSize.Width - 2,
						height - 5);
			}
		}

		protected void UpdateVP (Rectangle r)
		{
			UpdateVP (r.X, r.Y, r.Width, r.Height);
		}

		protected void UpdateVP (Point loc, int w, int h)
		{
			UpdateVP (loc.X, loc.Y, w, h);
		}

		protected void UpdateVP (int x, int y, int w, int h)
		{
			virtual_position.X = x;
			virtual_position.Y = y;
			virtual_position.Width = w;
			virtual_position.Height = h;

			DrawVirtualPosition (virtual_position);
		}

		private void HandleLButtonUp (ref Message m)
		{
			if (state == State.Idle)
				return;

			ClearVirtualPosition ();

			form.Capture = false;
			form.Bounds = virtual_position;
			state = State.Idle;

			OnWindowFinishedMoving ();
		}

		private bool HandleNCLButtonUp (ref Message m)
		{
			int x = Control.LowOrder ((int) m.LParam.ToInt32 ());
			int y = Control.HighOrder ((int) m.LParam.ToInt32 ());

			form.PointToClient (ref x, ref y);

			// Need to adjust because we are in NC land
			y += TitleBarHeight;

			foreach (TitleButton button in title_buttons) {
				if (button != null && button.Rectangle.Contains (x, y)) {
					button.Clicked (this, EventArgs.Empty);
					return true;
				}
			}

			return true;
		}
		
		protected void DrawTitleButton (Graphics dc, TitleButton button, Rectangle clip)
		{
			if (!button.Rectangle.IntersectsWith (clip))
				return;

			dc.FillRectangle (SystemBrushes.Control, button.Rectangle);

			ControlPaint.DrawCaptionButton (dc, button.Rectangle,
					button.Caption, ButtonState.Normal);
		}

		public virtual void DrawMaximizedButtons (PaintEventArgs pe, MainMenu menu)
		{

		}

		protected virtual void CloseClicked (object sender, EventArgs e)
		{
			form.Close ();
		}

		private void MinimizeClicked (object sender, EventArgs e)
		{
			if (GetWindowState () != FormWindowState.Minimized) {
				form.WindowState = FormWindowState.Minimized;
			} else {
				form.WindowState = FormWindowState.Normal;
			}
		}

		private void MaximizeClicked (object sender, EventArgs e)
		{
			if (GetWindowState () != FormWindowState.Maximized) {
				form.WindowState = FormWindowState.Maximized;
			} else {
				form.WindowState = FormWindowState.Normal;
			}
		}

		protected Point MouseMove (Message m)
		{
			Point cp = Cursor.Position;
			return new Point (cp.X - start.X, cp.Y - start.Y);
		}

		protected virtual void DrawVirtualPosition (Rectangle virtual_position)
		{
			form.Location = virtual_position.Location;
			start = Cursor.Position;
		}

		protected virtual void ClearVirtualPosition ()
		{
			
		}

		protected virtual void OnWindowFinishedMoving ()
		{
		}

		private FormPos FormPosForCoords (int x, int y)
		{
			int bw = ThemeEngine.Current.ManagedWindowBorderWidth (this);
			if (y < TitleBarHeight + bw) {
				//	Console.WriteLine ("A");
				if (y > bw && x > bw &&
						x < form.Width - bw)
					return FormPos.TitleBar;

				if (x < bw || (x < 20 && y < bw))
					return FormPos.TopLeft;

				if (x > form.Width - bw ||
					(x > form.Width - 20 && y < bw))
					return FormPos.TopRight;

				if (y < bw)
					return FormPos.Top;

			} else if (y > form.Height - 20) {
				//	Console.WriteLine ("B");
				if (x < bw ||
						(x < 20 && y > form.Height - bw))
					return FormPos.BottomLeft;

				if (x > form.Width - (bw * 2) ||
						(x > form.Width - 20 &&
						 y > form.Height - bw))
					return FormPos.BottomRight;

				if (y > form.Height - (bw * 2))
					return FormPos.Bottom;


			} else if (x < bw) {
				//	Console.WriteLine ("C");
				return FormPos.Left;
			} else if (x > form.Width - (bw * 2)) {
//				Console.WriteLine ("D");
				return FormPos.Right;
			} else {
				//			Console.WriteLine ("E   {0}", form.Width - bw);
			}
			
			return FormPos.None;
		}
	}
}


