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
// $Revision: 1.2 $
// $Modtime: $
// $Log: XplatUIX11.cs,v $
// Revision 1.2  2004/08/04 20:11:24  pbartok
// - Added Invalidate handling
//
// Revision 1.1  2004/07/09 05:21:25  pbartok
// - Initial check-in
//
//

// NOT COMPLETE

using System;
using System.Drawing;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

/// X11 Version
namespace System.Windows.Forms {
	public class XplatUIX11 : XplatUIDriver {
		#region Local Variables
		private static XplatUIX11	instance;
		private static int		ref_count;

		private static IntPtr		DisplayHandle;		// X11 handle to display
		internal static Keys		key_state;
		internal static MouseButtons	mouse_state;
		internal static Point		mouse_position;
		internal static Rectangle	paint_area;
		#endregion	// Local Variables

		internal override Color BackColor {
			get {
				return Color.DarkCyan;
			}
		}

		internal override Color ForeColor {
			get {
				return Color.White;
			}
		}

		internal override Font Font {
			get {
				return new Font("Arial", 12);
			}
		}

		internal override Keys ModifierKeys {
			get {
				return key_state;
			}
		}

		internal override MouseButtons MouseButtons {
			get {
				return mouse_state;
			}
		}

		internal override Point MousePosition {
			get {
				return mouse_position;
			}
		}

		internal override bool DropTarget {
			get {
				return false;
			}

			set {
				if (value) {
					throw new NotImplementedException("Need to figure out D'n'D for X11");
				}
			}
		}

		#region Constructor & Destructor
		private XplatUIX11() {
			// Handle singleton stuff first
			ref_count=0;

			// Now regular initialization
			DisplayHandle=XOpenDisplay(IntPtr.Zero);
			key_state=Keys.None;
			mouse_state=MouseButtons.None;
			mouse_position=Point.Empty;
			paint_area=new Rectangle(0, 0, 0, 0);

			Console.WriteLine("XplatUIX11 Constructor called, DisplayHandle {0:X}", DisplayHandle);
		}

		~XplatUIX11() {
			if (DisplayHandle!=IntPtr.Zero) {
				XCloseDisplay(DisplayHandle);
				DisplayHandle=IntPtr.Zero;
			}
			Console.WriteLine("XplatUI Destructor called");
		}
		#endregion	// Constructor & Destructor

		#region Singleton Specific Code
		public static XplatUIX11 GetInstance() {
			if (instance==null) {
				instance=new XplatUIX11();
			}
			ref_count++;
			return instance;
		}

		public int Reference {
			get {
				return ref_count;
			}
		}
		#endregion

		#region Public Static Methods
		internal override IntPtr InitializeDriver() {
			if (DisplayHandle==IntPtr.Zero) {
				DisplayHandle=XOpenDisplay(IntPtr.Zero);
				key_state=Keys.None;
				mouse_state=MouseButtons.None;
				mouse_position=Point.Empty;
			}

			Console.WriteLine("XplatUIX11.InitializeDriver() called");

			return IntPtr.Zero;
		}

		internal override void ShutdownDriver(IntPtr token) {
			if (DisplayHandle!=IntPtr.Zero) {
				XCloseDisplay(DisplayHandle);
				DisplayHandle=IntPtr.Zero;
			}
			Console.WriteLine("XplatUIX11.ShutdownDriver called");
		}


		internal void Version() {
			Console.WriteLine("Xplat version $revision: $");
		}

		internal override void Exit() {
			Console.WriteLine("XplatUIX11.Exit");
		}

		internal override IntPtr CreateWindow(CreateParams cp) {
			return CreateWindow(cp.Parent, cp.X, cp.Y, cp.Width, cp.Height);
		}

		internal override IntPtr CreateWindow(IntPtr Parent, int X, int Y, int Width, int Height) {
			IntPtr	WindowHandle;

			if (X<1) X=50;
			if (Y<1) Y=50;
			if (Width<1) Width=100;
			if (Height<1) Height=100;
			WindowHandle=XCreateSimpleWindow(DisplayHandle, Parent!=IntPtr.Zero ? Parent : XRootWindow(DisplayHandle, 0), X, Y, Width, Height, 4, 0, 0);
			Console.WriteLine("Received window handle {0,8:X} at pos {1},{2} {3}x{4}", WindowHandle, X, Y, Width, Height);
			XMapWindow(DisplayHandle, WindowHandle);
			XSelectInput(DisplayHandle, WindowHandle, 0xffffff);
			XSetWindowBackground(DisplayHandle, WindowHandle, (uint)this.BackColor.ToArgb());
			return(WindowHandle);
		}

		internal override void DestroyWindow(IntPtr handle) {
			XDestroyWindow(DisplayHandle, handle);
			return;
		}

		internal override void RefreshWindow(IntPtr handle) {
			Console.WriteLine("XplatUIX11.RefreshWindow");
		}

		[MonoTODO("Add support for internal table of windows/DCs for looking up paint area and cleanup")]
		internal override PaintEventArgs PaintEventStart(IntPtr handle) {
			PaintEventArgs	paint_event;
			Rectangle	update_area;

			// FIXME: Assign proper values
			update_area = new Rectangle();
			paint_event = new PaintEventArgs(Graphics.FromHwnd(handle), update_area);

			return paint_event;
		}

		internal override void PaintEventEnd(IntPtr handle) {
			// FIXME: Lookup in the internal list how to clean
			;
		}

		internal override void SetWindowPos(IntPtr handle, Rectangle rc) {
			SetWindowPos(handle, rc.X, rc.Y, rc.Width, rc.Height);
			return;
		}

		internal override bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height) {
			XMoveResizeWindow(DisplayHandle, hWnd, x, y, width, height);
			return true;
		}

		internal override void SetWindowPos(IntPtr handle, int x, int y, int width, int height) {
			XMoveResizeWindow(DisplayHandle, handle, x, y, width, height);
Console.WriteLine("Moving window to {0}:{1} {2}x{3}", x, y, width, height);
			return;
		}

		internal override void Activate(IntPtr handle) {
			Console.WriteLine("XplatUIX11.Activate");
			return;
		}

		internal override void Invalidate(IntPtr handle, Rectangle rc, bool clear) {
			Console.WriteLine("XplatUIX11.Invalidate");
			return;
		}

		internal override IntPtr DefWndProc(ref Message msg) {
#if not
			switch (msg.Msg) {
				case (int)Msg.WM_PAINT: {
					IntPtr	gc;

					if (msg.Hwnd!=IntPtr.Zero) {
						gc=XCreateGC(DisplayHandle, msg.Hwnd, 0, IntPtr.Zero);
						XSetBackground(DisplayHandle, gc, this.BackColor.ToArgb());
						XFreeGC(DisplayHandle, gc);
					}
					break;
				}
			}
#endif

#if debug
			Console.WriteLine("XplatUIX11.DefWndProc");
#endif
			return IntPtr.Zero;
		}

		internal override void HandleException(Exception e) {
			StackTrace st = new StackTrace(e);
			Console.WriteLine("Exception '{0}'", e.Message+st.ToString());
			Console.WriteLine("{0}{1}", e.Message, st.ToString());
		}

		internal override void DoEvents() {
			Console.WriteLine("XplatUIX11.DoEvents");
		}

		internal override bool PeekMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags) {
			Console.WriteLine("XplatUIX11.PeekMessage");
			return true;
		}

		internal override bool GetMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax) {
			XEvent	xevent = new XEvent();

			XNextEvent(DisplayHandle, ref xevent);
			msg.hwnd=xevent.AnyEvent.window;

			switch(xevent.type) {
				case XEventName.KeyPress: {
					IntPtr	buffer = Marshal.AllocHGlobal(24);
					XKeySym	keysym;
					string	keys;
					int	len;

					len=XLookupString(ref xevent, buffer, 24, out keysym, IntPtr.Zero);
					if (len>0) {
						char[] keychars;

						keys=Marshal.PtrToStringAuto(buffer);
						keychars=keys.ToCharArray(0, 1);
						msg.wParam=(IntPtr)keychars[0];
						Console.WriteLine("Got char {0}", keys);
					} else {
						Console.WriteLine("Got special key {0}", keysym);
					}
					Marshal.FreeHGlobal(buffer);
					break;

					msg.message=Msg.WM_KEYDOWN;
					break;
				}

				case XEventName.KeyRelease: {
					msg.message=Msg.WM_KEYUP;
					break;
				}

				case XEventName.ButtonPress: {
					msg.message=Msg.WM_LBUTTONDOWN;
					msg.lParam=(IntPtr) (xevent.ButtonEvent.y << 16 | xevent.ButtonEvent.x);
					break;
				}

				case XEventName.ButtonRelease: {
					break;
				}

				case XEventName.MotionNotify: {
					msg.message=Msg.WM_MOUSEMOVE;
					break;
				}

				case XEventName.Expose: {
					msg.message=Msg.WM_PAINT;
					msg.hwnd=hWnd;
					paint_area.X=xevent.ExposeEvent.x;
					paint_area.Y=xevent.ExposeEvent.y;
					paint_area.Width=xevent.ExposeEvent.width;
					paint_area.Height=xevent.ExposeEvent.height;
					break;
				}

				case XEventName.ResizeRequest: {
					msg.message=Msg.WM_SIZE;
					msg.wParam=IntPtr.Zero;
					msg.lParam=(IntPtr) (xevent.ResizeRequestEvent.width<<16 | xevent.ResizeRequestEvent.width);
					break;
				}
			}

			NativeWindow.WndProc(msg.hwnd, msg.message, msg.wParam, msg.lParam);

			return true;
		}

		internal override bool TranslateMessage(ref MSG msg) {
//			Console.WriteLine("XplatUIX11 TranslateMessage");
			return true;
		}

		internal override bool DispatchMessage(ref MSG msg) {
//			Console.WriteLine("XplatUIX11 DispatchMessage");
			return true;
		}

		internal override bool Text(IntPtr handle, string text) {
			Console.WriteLine("Setting window text {0}", text);
			return true;
		}

		internal override bool SetVisible(IntPtr handle, bool visible) {
			Console.WriteLine("Setting window visibility: {0}", visible);
			return true;
		}

		internal override bool IsVisible(IntPtr handle) {
			Console.WriteLine("Getting window visibility");
			return true;
		}

		internal override IntPtr SetParent(IntPtr handle, IntPtr parent) {
			int result=XReparentWindow(DisplayHandle, handle, parent, 0, 0);
			Console.WriteLine("Setting parent for window {0} to {1}, result {2}", handle, parent, result);
			return IntPtr.Zero;
		}

		internal override IntPtr GetParent(IntPtr handle) {
			Console.WriteLine("Getting parent {0}", handle);
			return IntPtr.Zero;
		}

		// Santa's little helper
		static void Where() {
			Console.WriteLine("Here: {0}", new StackTrace().ToString());
		}

		internal override void Run() {
			XEvent	xevent = new XEvent();

			//Console.WriteLine("Size of XEvent: {0}", Marshal.SizeOf(typeof(XEvent)));
			//Where();
			//XNextEvent(DisplayHandle, ref xevent);
			//Where();

			while (true==true) {
				XNextEvent(DisplayHandle, ref xevent);

				switch(xevent.type) {
					case XEventName.KeyPress: {
						IntPtr	buffer = Marshal.AllocHGlobal(24);
						XKeySym	keysym;
						string	keys;
						int	len;

						len=XLookupString(ref xevent, buffer, 24, out keysym, IntPtr.Zero);
						if (len>0) {
							keys=Marshal.PtrToStringAuto(buffer);
							Console.WriteLine("Got char {0}", keys);
						} else {
							Console.WriteLine("Got special key {0}", keysym);
						}
						Marshal.FreeHGlobal(buffer);
						break;
					}

					case XEventName.Expose: {
						Rectangle	r = new Rectangle(xevent.ExposeEvent.x, xevent.ExposeEvent.y, xevent.ExposeEvent.width, xevent.ExposeEvent.height);
						Graphics	g = Graphics.FromHwnd (xevent.ExposeEvent.window );
						Font		f = new Font("Bitstream Vera Sans Mono", 20);
						SolidBrush	b = new SolidBrush(Color.Red);
						Rectangle	r2 = new Rectangle(0, 0, 300, 300);

						g.FillRectangle(SystemBrushes.Window, r2);
						g.DrawString("TestString", f, b, 0, 0);

						Console.WriteLine("XplatUI.Run(): Exposed {0}", r);
 						break;
					}

					case XEventName.ButtonPress: {
						Console.WriteLine("XplatUI.Run(): leaving loop");
						return;
					}

					default: {
						Console.WriteLine("Received event {0}", xevent.type);
						break;
					}
				}
			}
		}

		#endregion	// Public Static Methods

		#region X11 Imports
		[DllImport ("libX11.so", EntryPoint="XOpenDisplay")]
		internal extern static IntPtr XOpenDisplay(IntPtr display);
		[DllImport ("libX11.so", EntryPoint="XCloseDisplay")]
		internal extern static void XCloseDisplay(IntPtr display);

		[DllImport ("libX11.so", EntryPoint="XCreateWindow")]
		internal extern static IntPtr XCreateWindow(IntPtr display, IntPtr parent, int x, int y, int width, int height, int border_width, int depth, int xclass, IntPtr visual, IntPtr attributes);
		[DllImport ("libX11.so", EntryPoint="XCreateSimpleWindow")]
		internal extern static IntPtr XCreateSimpleWindow(IntPtr display, IntPtr parent, int x, int y, int width, int height, int border_width, int border, int background);
		[DllImport ("libX11.so", EntryPoint="XMapWindow")]
		internal extern static IntPtr XMapWindow(IntPtr display, IntPtr window);
		[DllImport ("libX11.so", EntryPoint="XRootWindow")]
		internal extern static IntPtr XRootWindow(IntPtr display, int screen_number);
		[DllImport ("libX11.so", EntryPoint="XNextEvent")]
		internal extern static IntPtr XNextEvent(IntPtr display, ref XEvent xevent);
		[DllImport ("libX11.so", EntryPoint="XSelectInput")]
		internal extern static IntPtr XSelectInput(IntPtr display, IntPtr window, int mask);
		[DllImport ("libX11.so", EntryPoint="XLookupString")]
		internal extern static int XLookupString(ref XEvent xevent, IntPtr buffer, int num_bytes, out XKeySym keysym, IntPtr status);

		[DllImport ("libX11.so", EntryPoint="XDestroyWindow")]
		internal extern static int XDestroyWindow(IntPtr display, IntPtr window);

		[DllImport ("libX11.so", EntryPoint="XReparentWindow")]
		internal extern static int XReparentWindow(IntPtr display, IntPtr window, IntPtr parent, int x, int y);
		[DllImport ("libX11.so", EntryPoint="XMoveResizeWindow")]
		internal extern static int XMoveResizeWindow(IntPtr display, IntPtr window, int x, int y, int width, int height);

		// Drawing
		[DllImport ("libX11.so", EntryPoint="XCreateGC")]
		internal extern static IntPtr XCreateGC(IntPtr display, IntPtr window, int valuemask, IntPtr values);
		[DllImport ("libX11.so", EntryPoint="XFreeGC")]
		internal extern static int XFreeGC(IntPtr display, IntPtr gc);
		[DllImport ("libX11.so", EntryPoint="XSetWindowBackground")]
		internal extern static int XSetWindowBackground(IntPtr display, IntPtr window, uint background);
		#endregion

	}
}
