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

using System;
using System.Threading;
using System.Drawing;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Sockets;

// Only do the poll when building with mono for now
#if __MonoCS__
using Mono.Unix;
#endif

/// X11 Version
namespace System.Windows.Forms {
	internal class XplatUIX11 : XplatUIDriver {
		#region Structure Definitions
		internal struct Caret {
			internal Timer	timer;				// Blink interval
			internal IntPtr	hwnd;				// Window owning the caret
			internal int	x;				// X position of the caret
			internal int	y;				// Y position of the caret
			internal int	width;				// Width of the caret; if no image used
			internal int	height;				// Height of the caret, if no image used
			internal int	visible;			// Counter for visible/hidden
			internal bool	on;				// Caret blink display state: On/Off
			internal IntPtr	gc;				// Graphics context
			internal bool	paused;				// Don't update right now
		}

		internal struct Hover {
			internal Timer	timer;				// for hovering
			internal IntPtr	hwnd;				// Last window we entered; used to generate WM_MOUSEHOVER
			internal int	x;				// Last MouseMove X coordinate; used to generate WM_MOUSEHOVER
			internal int	y;				// Last MouseMove Y coordinate; used to generate WM_MOUSEHOVER
			internal int	interval;			// in milliseconds, how long to hold before hover is generated
			internal int	hevent;				// X Atom
		}
		#endregion	// Structure Definitions

		#region Local Variables
		private static XplatUIX11	instance;
		private static int		ref_count;
		private static bool		themes_enabled;

		private static IntPtr		DisplayHandle;		// X11 handle to display
		private static int		screen_num;		// Screen number used
		private static IntPtr		root_window;		// Handle of the root window for the screen/display
		private static IntPtr		active_window;		// Handle of the window with focus (if any)
		private static IntPtr		FosterParent;		// Container to hold child windows until their parent exists
		private static int		wm_protocols;		// X Atom
		private static int		wm_delete_window;	// X Atom
		private static int		wm_take_focus;		// X Atom
		private static int		mwm_hints;		// X Atom
		private static int		wm_no_taskbar;		// X Atom
		private static int		wm_state_above;		// X Atom
		private static int		atom;			// X Atom
		private static int		wm_state_modal;		// X Atom
		private static int		wm_state_maximized_horz;// X Atom
		private static int		wm_state_maximized_vert;// X Atom
		private static int		net_wm_state;		// X Atom
		private static int		net_active_window;	// X Atom
		private static int		net_wm_context_help;	// X Atom
		private static int		async_method;
		private static int		post_message;		// X Atom send to generate a PostMessage event
		private static IntPtr		default_colormap;	// X Colormap ID
		internal static MouseButtons	mouse_state;
		internal static Point		mouse_position;
		internal static bool		grab_confined;		// Is the current grab (if any) confined to grab_area?
		internal static IntPtr		grab_hwnd;		// The window that is grabbed
		internal static Rectangle	grab_area;		// The area the current grab is confined to
		internal static IntPtr		click_pending_hwnd;	// 
		internal static Msg		click_pending_message;	// 
		internal static IntPtr		click_pending_lparam;	// 
		internal static IntPtr		click_pending_wparam;	// 
		internal static long		click_pending_time;	// Last time we received a mouse click
		internal static bool		click_pending;		// True if we haven't sent the last mouse click
		internal static int		double_click_interval;	// in milliseconds, how fast one has to click for a double click
		internal static Stack		modal_window;		// Stack of modal window handles
		internal static Hover		hover;
		internal static bool		getmessage_ret;		// Return value for GetMessage function; 0 to terminate app
		internal static IntPtr		focus_hwnd;		// the window that currently has keyboard focus
		internal static IntPtr		override_cursor;	// The cursor overriding a standard cursor
		internal static IntPtr		last_cursor;		// To avoid server roundtrips we cache the cursor to avoid re-setting the same
		internal static IntPtr		last_window;		// To avoid server roundtrips we cache the cursor to avoid re-setting the same

		internal static Caret		caret;			// To display a blinking caret

		private static Hashtable	handle_data;
		private static XEventQueue	message_queue;

		private X11Keyboard keyboard;
		private ArrayList timer_list;
		private Thread timer_thread;
		private AutoResetEvent timer_wait;
		private Socket listen;
		private Socket wake;

#if __MonoCS__
		private Pollfd [] pollfds;
#endif

		private object xlib_lock = new object ();

		private static readonly EventMask  SelectInputMask = EventMask.ButtonPressMask | 
				EventMask.ButtonReleaseMask | 
				EventMask.KeyPressMask | 
				EventMask.KeyReleaseMask | 
				EventMask.EnterWindowMask | 
				EventMask.LeaveWindowMask |
				EventMask.ExposureMask |
				EventMask.FocusChangeMask |
				EventMask.PointerMotionMask | 
				EventMask.VisibilityChangeMask |
				EventMask.StructureNotifyMask;

		#endregion	// Local Variables

		#region	Properties
		internal override Keys ModifierKeys {
			get {
				return keyboard.ModifierKeys;
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
		// Keyboard
		internal override int KeyboardSpeed {
			get {
				//
				// A lot harder: need to do:
				// XkbQueryExtension(0x08051008, 0xbfffdf4c, 0xbfffdf50, 0xbfffdf54, 0xbfffdf58)       = 1
				// XkbAllocKeyboard(0x08051008, 0xbfffdf4c, 0xbfffdf50, 0xbfffdf54, 0xbfffdf58)        = 0x080517a8
				// XkbGetControls(0x08051008, 1, 0x080517a8, 0xbfffdf54, 0xbfffdf58)                   = 0
				//
				// And from that we can tell the repetition rate
				//
				// Notice, the values must map to:
				//   [0, 31] which maps to 2.5 to 30 repetitions per second.
				//
				return 0;
			}
		}

		internal override int KeyboardDelay {
			get {
				//
				// Return values must range from 0 to 4, 0 meaning 250ms,
				// and 4 meaning 1000 ms.
				//
				return 1; // ie, 500 ms
			}
		}
		#endregion	// Properties

		#region Constructor & Destructor
                // This is always called from a locked context
		private XplatUIX11() {
			// Handle singleton stuff first
			ref_count=0;

			getmessage_ret = true;

			message_queue = new XEventQueue ();
			timer_list = new ArrayList ();

			// Now regular initialization
			SetDisplay(XOpenDisplay(IntPtr.Zero));

			keyboard = new X11Keyboard (DisplayHandle);
			
			listen = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
			IPEndPoint ep = new IPEndPoint (IPAddress.Loopback, 0);
			listen.Bind (ep);
			listen.Listen (1);

			wake = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
			wake.Connect (listen.LocalEndPoint);

			double_click_interval = 500;
			hover.interval = 500;

			hover.timer = new Timer();
			hover.timer.Enabled = false;
			hover.timer.Interval = hover.interval;		// FIXME - read this from somewhere
			hover.timer.Tick +=new EventHandler(MouseHover);
			hover.x = -1;
			hover.y = -1;

			focus_hwnd = IntPtr.Zero;

			modal_window = new Stack(3);

#if __MonoCS__
			pollfds = new Pollfd [2];
			pollfds [0] = new Pollfd ();
			pollfds [0].fd = XConnectionNumber (DisplayHandle);
			pollfds [0].events = PollEvents.POLLIN;

			pollfds [1] = new Pollfd ();
			pollfds [1].fd = wake.Handle.ToInt32 ();
			pollfds [1].events = PollEvents.POLLIN;
#endif

			caret.timer = new Timer();
			caret.timer.Interval = 500;		// FIXME - where should this number come from?
			caret.timer.Tick += new EventHandler(CaretCallback);
		}

		~XplatUIX11() {
			lock (this) {
				if (DisplayHandle!=IntPtr.Zero) {
					XCloseDisplay(DisplayHandle);
					DisplayHandle=IntPtr.Zero;
				}
			}
		}
		#endregion	// Constructor & Destructor

		#region Singleton Specific Code
		public static XplatUIX11 GetInstance() {
			lock (typeof (XplatUIX11)) {
				if (instance==null) {
					instance=new XplatUIX11();
				}
				ref_count++;
			}
			return instance;
		}

		public int Reference {
			get {
				return ref_count;
			}
		}
		#endregion


		#region Events
		internal override event EventHandler Idle;
		#endregion	// Events


		#region	Callbacks
		private void MouseHover(object sender, EventArgs e) {
			if ((hover.x == mouse_position.X) && (hover.y == mouse_position.Y)) {
				XEvent xevent;

				hover.timer.Enabled = false;

				if (hover.hwnd != IntPtr.Zero) {
					xevent = new XEvent ();
					xevent.type = XEventName.ClientMessage;
					xevent.ClientMessageEvent.display = DisplayHandle;
					xevent.ClientMessageEvent.window = (IntPtr)hover.hwnd;
					xevent.ClientMessageEvent.message_type = (IntPtr)hover.hevent;
					xevent.ClientMessageEvent.format = 32;
					xevent.ClientMessageEvent.ptr1 = (IntPtr) (hover.y << 16 | hover.x);

					message_queue.EnqueueLocked (xevent);

					WakeupMain ();
				}
			}
		}

		private void CaretCallback(object sender, EventArgs e) {
			if (caret.paused) {
				return;
			}
			caret.on = !caret.on;

			XDrawLine(DisplayHandle, caret.hwnd, caret.gc, caret.x, caret.y, caret.x, caret.y + caret.height);
		}
		#endregion	// Callbacks

		#region	Helpers
		private void SendNetWMMessage(IntPtr window, IntPtr message_type, IntPtr l0, IntPtr l1, IntPtr l2) {
			XEvent	xev;

			xev = new XEvent();
			xev.ClientMessageEvent.type = XEventName.ClientMessage;
			xev.ClientMessageEvent.send_event = true;
			xev.ClientMessageEvent.window = window;
			xev.ClientMessageEvent.message_type = message_type;
			xev.ClientMessageEvent.format = 32;
			xev.ClientMessageEvent.ptr1 = l0;
			xev.ClientMessageEvent.ptr2 = l1;
			xev.ClientMessageEvent.ptr3 = l2;
			XSendEvent(DisplayHandle, root_window, false, EventMask.SubstructureRedirectMask | EventMask.SubstructureNotifyMask, ref xev);
		}
		#endregion	// Helpers

		#region Public Static Methods
		internal override IntPtr InitializeDriver() {
			lock (this) {
				if (DisplayHandle==IntPtr.Zero) {
					DisplayHandle=XOpenDisplay(IntPtr.Zero);
					mouse_state=MouseButtons.None;
					mouse_position=Point.Empty;
				}
			}
			return IntPtr.Zero;
		}

		internal static void SetDisplay(IntPtr display_handle) {
			if (display_handle != IntPtr.Zero) {
				//IntPtr	Screen;

				if (FosterParent != IntPtr.Zero) {
					XDestroyWindow(DisplayHandle, FosterParent);
				}
				if (DisplayHandle != IntPtr.Zero) {
					XCloseDisplay(DisplayHandle);
				}

				DisplayHandle=display_handle;

				// We need to tell System.Drawing our DisplayHandle. FromHdcInternal has
				// been hacked to do this for us.
				Graphics.FromHdcInternal (DisplayHandle);

				// Create a few things
				active_window = IntPtr.Zero;
				mouse_state = MouseButtons.None;
				mouse_position = Point.Empty;
				//Screen = XDefaultScreenOfDisplay(DisplayHandle);
				//screen_num = XScreenNumberOfScreen(DisplayHandle, Screen);
				screen_num = 0;
				root_window = XRootWindow(display_handle, screen_num);
				default_colormap = XDefaultColormap(display_handle, screen_num);

				// Create the foster parent
				FosterParent=XCreateSimpleWindow(display_handle, root_window, 0, 0, 1, 1, 4, 0, 0);
				if (FosterParent==IntPtr.Zero) {
					Console.WriteLine("XplatUIX11 Constructor failed to create FosterParent");
				}

				// Prepare for shutdown
				wm_protocols=XInternAtom(display_handle, "WM_PROTOCOLS", false);
				wm_delete_window=XInternAtom(display_handle, "WM_DELETE_WINDOW", false);
				wm_take_focus=XInternAtom(display_handle, "WM_TAKE_FOCUS", false);

				// handling decorations and such
				mwm_hints=XInternAtom(display_handle, "_MOTIF_WM_HINTS", false);
				net_wm_state=XInternAtom(display_handle, "_NET_WM_STATE", false);
				wm_no_taskbar=XInternAtom(display_handle, "_NET_WM_STATE_NO_TASKBAR", false);
				wm_state_above=XInternAtom(display_handle, "_NET_WM_STATE_ABOVE", false);
				wm_state_modal = XInternAtom(display_handle, "_NET_WM_STATE_MODAL", false);
				net_active_window = XInternAtom(display_handle, "_NET_ACTIVE_WINDOW", false);
				wm_state_maximized_horz = XInternAtom(display_handle, "_NET_WM_STATE_MAXIMIZED_HORZ", false);
				wm_state_maximized_vert = XInternAtom(display_handle, "_NET_WM_STATE_MAXIMIZED_VERT", false);

				atom=XInternAtom(display_handle, "ATOM", false);
				async_method = XInternAtom(display_handle, "_SWF_AsyncAtom", false);
				post_message = XInternAtom (display_handle, "_SWF_PostMessageAtom", false);
				hover.hevent = XInternAtom(display_handle, "_SWF_HoverAtom", false);

				handle_data = new Hashtable ();

				XSelectInput(DisplayHandle, root_window, EventMask.PropertyChangeMask);
			} else {
				throw new ArgumentNullException("Display", "Could not open display (X-Server required. Check you DISPLAY environment variable)");
			}
		}

		internal override void ShutdownDriver(IntPtr token) {
			lock (this) {
				if (DisplayHandle!=IntPtr.Zero) {
					XCloseDisplay(DisplayHandle);
					DisplayHandle=IntPtr.Zero;
				}
			}
		}


		internal void Version() {
			Console.WriteLine("Xplat version $revision: $");
		}

		internal override void Exit() {
			getmessage_ret = false;
		}

		internal override void GetDisplaySize(out Size size) {
			XWindowAttributes	attributes=new XWindowAttributes();

			lock (xlib_lock) {
				XGetWindowAttributes(DisplayHandle, XRootWindow(DisplayHandle, 0), ref attributes);
			}

			size = new Size(attributes.width, attributes.height);
		}

		internal override void EnableThemes() {
			themes_enabled=true;
		}

		internal override IntPtr CreateWindow(CreateParams cp) {
			IntPtr			WindowHandle;
			IntPtr			ParentHandle;
			int			X;
			int			Y;
			int			Width;
			int			Height;
			MotifWmHints		mwmHints;
			IntPtr[]		atoms;
			int			atom_count;
			int			BorderWidth;
			XSetWindowAttributes	attr;

			ParentHandle=cp.Parent;

			X=cp.X;
			Y=cp.Y;
			Width=cp.Width;
			Height=cp.Height;
			BorderWidth=0;

			if (Width<1) Width=1;
			if (Height<1) Height=1;


			lock (xlib_lock) {
				if (ParentHandle==IntPtr.Zero) {
					if ((cp.Style & (int)(WindowStyles.WS_CHILD))!=0) {
						// We need to use our foster parent window until
						// this poor child gets it's parent assigned
						ParentHandle=FosterParent;
					} else if ((cp.Style & (int)(WindowStyles.WS_POPUP))!=0) {
						BorderWidth=0;
						ParentHandle=XRootWindow(DisplayHandle, 0);
					} else {
						if (X<1) X=50;
						if (Y<1) Y=50;
						BorderWidth=4;
						ParentHandle=XRootWindow(DisplayHandle, 0);
					}
				}

				attr = new XSetWindowAttributes();

				if ((cp.ExStyle & ((int)WindowStyles.WS_EX_TOOLWINDOW)) != 0) {
					attr.save_under = true;
				}

				attr.override_redirect = false;

				if ((cp.Style & ((int)WindowStyles.WS_POPUP)) != 0) {
					attr.override_redirect = true;
				}

				attr.bit_gravity = Gravity.NorthWestGravity;
				attr.win_gravity = Gravity.NorthWestGravity;

				if (attr.override_redirect) {
					if ((cp.Style & ((int)WindowStyles.WS_CAPTION)) != 0) {
						attr.override_redirect = false;
					}
				}

				WindowHandle=XCreateWindow(DisplayHandle, ParentHandle, X, Y, Width, Height, BorderWidth, (int)CreateWindowArgs.CopyFromParent, (int)CreateWindowArgs.InputOutput, IntPtr.Zero, SetWindowValuemask.BitGravity | SetWindowValuemask.WinGravity | SetWindowValuemask.SaveUnder | SetWindowValuemask.OverrideRedirect, ref attr);

				// Set the appropriate window manager hints
				if (((cp.Style & ((int)WindowStyles.WS_POPUP)) != 0)  && (ParentHandle != IntPtr.Zero)) {
					XSetTransientForHint(DisplayHandle, WindowHandle, ParentHandle);
				}

				MotifFunctions functions = 0;
				MotifDecorations decorations = 0;
				mwmHints = new MotifWmHints();
				mwmHints.flags = (IntPtr)(MotifFlags.Functions | MotifFlags.Decorations);
				mwmHints.functions = (IntPtr)0;
				mwmHints.decorations = (IntPtr)0;
				
				if ((cp.Style & ((int)WindowStyles.WS_CAPTION)) != 0) {
					functions |= MotifFunctions.Move;
					decorations |= MotifDecorations.Title | MotifDecorations.Menu;
				}

				if ((cp.Style & ((int)WindowStyles.WS_THICKFRAME)) != 0) {
					functions |= MotifFunctions.Move | MotifFunctions.Resize;
					decorations |= MotifDecorations.Border | MotifDecorations.ResizeH;
				}

				if ((cp.Style & ((int)WindowStyles.WS_MINIMIZEBOX)) != 0) {
					functions |= MotifFunctions.Minimize;
					decorations |= MotifDecorations.Minimize;
				}

				if ((cp.Style & ((int)WindowStyles.WS_MAXIMIZEBOX)) != 0) {
					functions |= MotifFunctions.Maximize;
					decorations |= MotifDecorations.Maximize;
				}

				if ((cp.Style & ((int)WindowStyles.WS_SYSMENU)) != 0) {
					functions |= MotifFunctions.Close;
				}

				if ((cp.ExStyle & ((int)WindowStyles.WS_EX_DLGMODALFRAME)) != 0) {
					decorations |= MotifDecorations.Border;
				}

				if ((cp.Style & ((int)WindowStyles.WS_DLGFRAME)) != 0) {
					decorations |= MotifDecorations.Border;
				}

				if ((cp.Style & ((int)WindowStyles.WS_BORDER)) != 0) {
					decorations |= MotifDecorations.Border;
				}

				if ((cp.ExStyle & ((int)WindowStyles.WS_EX_TOOLWINDOW)) != 0) {
					functions = 0;
					decorations = 0;
				}

				mwmHints.functions = (IntPtr)functions;
				mwmHints.decorations = (IntPtr)decorations;

				XChangeProperty(DisplayHandle, WindowHandle, mwm_hints, mwm_hints, 32, PropertyMode.Replace, ref mwmHints, 5);

				atoms = new IntPtr[8];
				atom_count = 0;

				if ((cp.ExStyle & ((int)WindowStyles.WS_EX_TOOLWINDOW)) != 0) {
					atoms[atom_count++] = (IntPtr)wm_state_above;
					atoms[atom_count++] = (IntPtr)wm_no_taskbar;
				}
				// Should we use SendNetWMMessage here?
				XChangeProperty(DisplayHandle, WindowHandle, net_wm_state, atom, 32, PropertyMode.Replace, ref atoms, atom_count);

				XSelectInput(DisplayHandle, WindowHandle, SelectInputMask);

				if ((cp.Style & ((int)WindowStyles.WS_VISIBLE)) != 0) {
					XMapWindow(DisplayHandle, WindowHandle);
				}

				atom_count = 0;
				atoms[atom_count++] = (IntPtr)wm_delete_window;

				#if notneeded
				// This is better handled via the root_window property notification
				// Only get the WM_TAKE_FOCUS message if we're a toplevel window
				if ((cp.Parent == IntPtr.Zero) && ((cp.Style & (int)(WindowStyles.WS_POPUP | WindowStyles.WS_OVERLAPPEDWINDOW)) != 0)) {
					atoms[atom_count++] = (IntPtr)wm_take_focus;
				}
				#endif

				if ((cp.ExStyle & (int)WindowStyles.WS_EX_CONTEXTHELP) != 0) {
					atoms[atom_count++] = (IntPtr)net_wm_context_help;
				}

				XSetWMProtocols(DisplayHandle, WindowHandle, atoms, atom_count);
			}
			return(WindowHandle);
		}

		internal override IntPtr CreateWindow(IntPtr Parent, int X, int Y, int Width, int Height) {
			CreateParams create_params = new CreateParams();

			create_params.Caption = "";
			create_params.X = X;
			create_params.Y = Y;
			create_params.Width = Width;
			create_params.Height = Height;

			create_params.ClassName=XplatUI.DefaultClassName;
			create_params.ClassStyle = 0;
			create_params.ExStyle=0;
			create_params.Parent=IntPtr.Zero;
			create_params.Param=0;

			return CreateWindow(create_params);
		}

		internal override void DestroyWindow(IntPtr handle) {
			lock (this) {
				HandleData data = (HandleData) handle_data [handle];
				if (data != null) {
					data.Dispose ();
					handle_data [handle] = null;
					XDestroyWindow(DisplayHandle, handle);
				}
			}
		}

		internal override FormWindowState GetWindowState(IntPtr handle) {
			Atom			actual_atom;
			int			actual_format;
			int			nitems;
			int			bytes_after;
			IntPtr			prop = IntPtr.Zero;
			IntPtr			atom;
			int			maximized;
			XWindowAttributes	attributes;

			maximized = 0;
			XGetWindowProperty(DisplayHandle, handle, net_wm_state, 0, 256, false, Atom.XA_ATOM, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);
			if ((nitems > 0) && (prop != IntPtr.Zero)) {

				for (int i = 0; i < nitems; i++) {
					atom = (IntPtr)Marshal.ReadInt32(prop, i * 4);
					if ((atom == (IntPtr)wm_state_maximized_horz) || (atom == (IntPtr)wm_state_maximized_vert)) {
						maximized++;
					}
				}
				XFree(prop);
			}
			if (maximized == 2) {
				return FormWindowState.Maximized;
			}


			attributes = new XWindowAttributes();
			XGetWindowAttributes(DisplayHandle, handle, ref attributes);
			if (attributes.map_state == MapState.IsUnmapped) {
				return FormWindowState.Minimized;
			}

			return FormWindowState.Normal;
		}

		internal override void SetWindowState(IntPtr handle, FormWindowState state) {
			FormWindowState	current_state;

			current_state = GetWindowState(handle);

			if (current_state == state) {
				return;
			}

			switch(state) {
				case FormWindowState.Normal: {
					if (current_state == FormWindowState.Minimized) {
						XMapWindow(DisplayHandle, handle);
					} else if (current_state == FormWindowState.Maximized) {
						SendNetWMMessage(handle, (IntPtr)net_wm_state, (IntPtr)2 /* toggle */, (IntPtr)wm_state_maximized_horz, (IntPtr)wm_state_maximized_vert);
					}
					Activate(handle);
					return;
				}

				case FormWindowState.Minimized: {
					if (current_state == FormWindowState.Maximized) {
					       SendNetWMMessage(handle, (IntPtr)net_wm_state, (IntPtr)2 /* toggle */, (IntPtr)wm_state_maximized_horz, (IntPtr)wm_state_maximized_vert);
					}
					XIconifyWindow(DisplayHandle, handle, 0);
					return;
				}

				case FormWindowState.Maximized: {
					if (current_state == FormWindowState.Minimized) {
						XMapWindow(DisplayHandle, handle);
					}

					SendNetWMMessage(handle, (IntPtr)net_wm_state, (IntPtr)1 /* Add */, (IntPtr)wm_state_maximized_horz, (IntPtr)wm_state_maximized_vert);
					Activate(handle);
					return;
				}
			}
		}

		internal override void SetWindowStyle(IntPtr handle, CreateParams cp) {
			MotifWmHints		mwmHints;
			IntPtr[]		atoms;
			int			atom_count;

			// Set the appropriate window manager hints
			if (((cp.Style & ((int)WindowStyles.WS_POPUP)) != 0)  && (GetParent(handle) != IntPtr.Zero)) {
				XSetTransientForHint(DisplayHandle, handle, GetParent(handle));
			}

			MotifFunctions functions = 0;
			MotifDecorations decorations = 0;
			mwmHints = new MotifWmHints();
			mwmHints.flags = (IntPtr)(MotifFlags.Functions | MotifFlags.Decorations);
			mwmHints.functions = (IntPtr)0;
			mwmHints.decorations = (IntPtr)0;
				
			if ((cp.Style & ((int)WindowStyles.WS_CAPTION)) != 0) {
				functions |= MotifFunctions.Move;
				decorations |= MotifDecorations.Title | MotifDecorations.Menu;
			}

			if ((cp.Style & ((int)WindowStyles.WS_THICKFRAME)) != 0) {
				functions |= MotifFunctions.Move | MotifFunctions.Resize;
				decorations |= MotifDecorations.Border | MotifDecorations.ResizeH;
			}

			if ((cp.Style & ((int)WindowStyles.WS_MINIMIZEBOX)) != 0) {
				functions |= MotifFunctions.Minimize;
				decorations |= MotifDecorations.Minimize;
			}

			if ((cp.Style & ((int)WindowStyles.WS_MAXIMIZEBOX)) != 0) {
				functions |= MotifFunctions.Maximize;
				decorations |= MotifDecorations.Maximize;
			}

			if ((cp.Style & ((int)WindowStyles.WS_SYSMENU)) != 0) {
				functions |= MotifFunctions.Close;
			}

			if ((cp.ExStyle & ((int)WindowStyles.WS_EX_DLGMODALFRAME)) != 0) {
				decorations |= MotifDecorations.Border;
			}

			if ((cp.Style & ((int)WindowStyles.WS_DLGFRAME)) != 0) {
				decorations |= MotifDecorations.Border;
			}

			if ((cp.Style & ((int)WindowStyles.WS_BORDER)) != 0) {
				decorations |= MotifDecorations.Border;
			}

			if ((cp.ExStyle & ((int)WindowStyles.WS_EX_TOOLWINDOW)) != 0) {
				functions = 0;
				decorations = 0;
			}

			mwmHints.functions = (IntPtr)functions;
			mwmHints.decorations = (IntPtr)decorations;

			XChangeProperty(DisplayHandle, handle, mwm_hints, mwm_hints, 32, PropertyMode.Replace, ref mwmHints, 5);

			atoms = new IntPtr[8];
			atom_count = 0;

			if ((cp.ExStyle & ((int)WindowStyles.WS_EX_TOOLWINDOW)) != 0) {
				atoms[atom_count++] = (IntPtr)wm_state_above;
				atoms[atom_count++] = (IntPtr)wm_no_taskbar;
			}
			// Should we use SendNetWMMessage here?
			XChangeProperty(DisplayHandle, handle, net_wm_state, atom, 32, PropertyMode.Replace, ref atoms, atom_count);

			XSelectInput(DisplayHandle, handle, SelectInputMask);
		}

		internal override void UpdateWindow(IntPtr handle) {
			// Nothing to do, happens automatically
			return;
		}

		internal override void SetWindowBackground(IntPtr handle, Color color) {
			XColor	xcolor;

			xcolor = new XColor();

			xcolor.red = (ushort)(color.R * 257);
			xcolor.green = (ushort)(color.G * 257);
			xcolor.blue = (ushort)(color.B * 257);
			XAllocColor(DisplayHandle, default_colormap, ref xcolor);

			lock (xlib_lock) {
				XSetWindowBackground(DisplayHandle, handle, xcolor.pixel);
				XClearWindow(DisplayHandle, handle);
			}
		}

		[MonoTODO("Add support for internal table of windows/DCs for looking up paint area and cleanup")]
		internal override PaintEventArgs PaintEventStart(IntPtr handle) {
			PaintEventArgs	paint_event;

			HandleData data = (HandleData) handle_data [handle];
			if (data == null) {
				throw new Exception ("null data on paint event start: " + handle);

			}

			if (caret.visible == 1) {
				caret.paused = true;
				HideCaret();
			}

			data.DeviceContext = Graphics.FromHwnd (handle);
			paint_event = new PaintEventArgs((Graphics)data.DeviceContext, data.InvalidArea);

			return paint_event;
		}

		internal override void PaintEventEnd(IntPtr handle) {
			HandleData data = (HandleData) handle_data [handle];
			if (data == null)
				throw new Exception ("null data on PaintEventEnd");
			data.ClearInvalidArea ();
			Graphics g = (Graphics) data.DeviceContext;
			g.Flush ();
			g.Dispose ();

			if (caret.visible == 1) {
				ShowCaret();
				caret.paused = false;
			}
		}

		internal override void SetWindowPos(IntPtr handle, int x, int y, int width, int height) {
			// X requires a sanity check for width & height; otherwise it dies
			if (width < 1) {
				width = 1;
			}

			if (height < 1) {
				height = 1;
			}

			lock (xlib_lock) {
				XMoveResizeWindow(DisplayHandle, handle, x, y, width, height);
			}
			return;
		}

		internal override void GetWindowPos(IntPtr handle, out int x, out int y, out int width, out int height, out int client_width, out int client_height) {
			IntPtr	root;
			int	border_width;
			int	depth;

			lock (xlib_lock) {
				
				XGetGeometry(DisplayHandle, handle, out root, out x,
						out y, out width, out height, out border_width, out depth);
			}

			client_width = width;
			client_height = height;
			return;
		}

		internal override void Activate(IntPtr handle) {

			lock (xlib_lock) {
				SendNetWMMessage(handle, (IntPtr)net_active_window, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
				//XRaiseWindow(DisplayHandle, handle);
			}
			return;
		}

		internal override void EnableWindow(IntPtr handle, bool Enable) {
			// We do nothing; On X11 SetModal is used to create modal dialogs, on Win32 this function is used (see comment there)
		}

		internal override void SetModal(IntPtr handle, bool Modal) {
			if (Modal) {
				modal_window.Push(handle);
			} else {
				if (modal_window.Contains(handle)) {
					modal_window.Pop();
				}
				if (modal_window.Count > 0) {
					Activate((IntPtr)modal_window.Peek());
				}
			}
		}

		internal override void Invalidate (IntPtr handle, Rectangle rc, bool clear) {
			if (clear) {
				XClearArea (DisplayHandle, handle, rc.Left, rc.Top, rc.Width, rc.Height, true);
			} else {
				XEvent xevent = new XEvent ();
				xevent.type = XEventName.Expose;
				xevent.ExposeEvent.display = DisplayHandle;
				xevent.ExposeEvent.window = handle;
				xevent.ExposeEvent.x = rc.X;
				xevent.ExposeEvent.y = rc.Y;
				xevent.ExposeEvent.width = rc.Width;
				xevent.ExposeEvent.height = rc.Height;

				AddExpose (xevent);
			}
		}

		internal override IntPtr DefWndProc(ref Message msg) {
			switch((Msg)msg.Msg) {
				case Msg.WM_ERASEBKGND: {
					HandleData data = (HandleData) handle_data [msg.HWnd];
					if (data == null) {
						throw new Exception ("null data on WM_ERASEBKGND: " + msg.HWnd);

					}
					
					XClearArea(DisplayHandle, msg.HWnd, data.InvalidArea.Left, data.InvalidArea.Top, data.InvalidArea.Width, data.InvalidArea.Height, false);

					return IntPtr.Zero;
				}
			}
			return IntPtr.Zero;
		}

		internal override void HandleException(Exception e) {
			StackTrace st = new StackTrace(e);
			Console.WriteLine("Exception '{0}'", e.Message+st.ToString());
			Console.WriteLine("{0}{1}", e.Message, st.ToString());
		}

		internal override void DoEvents () {
			MSG msg = new MSG ();
			while (PeekMessage(ref msg, IntPtr.Zero, 0, 0, (uint)PeekMessageFlags.PM_REMOVE)) {
				if (msg.message == Msg.WM_PAINT) {
					TranslateMessage (ref msg);
					DispatchMessage (ref msg);
				}
			}
		}

		[MonoTODO("Implement PM_NOREMOVE flag")]
		internal override bool PeekMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags) {
			bool	 pending;

			if ((flags & (uint)PeekMessageFlags.PM_REMOVE) == 0) {
				throw new NotImplementedException("PeekMessage PM_NOREMOVE is not implemented yet");
			}

			pending = false;
			if (message_queue.Count > 0) {
				pending = true;
			} else {
				// Only call UpdateMessageQueue if real events are pending 
				// otherwise we go to sleep on the socket
				if (XPending(DisplayHandle) != 0) {
					UpdateMessageQueue();
					pending = true;
				}
			}
			if (!pending) {
				return false;
			}
			return GetMessage(ref msg, hWnd, wFilterMin, wFilterMax);
		}

		private IntPtr GetMousewParam(int Delta) {
			int	result = 0;

			if ((mouse_state & MouseButtons.Left) != 0) {
				result |= (int)MsgButtons.MK_LBUTTON;
			}

			if ((mouse_state & MouseButtons.Middle) != 0) {
				result |= (int)MsgButtons.MK_MBUTTON;
			}

			if ((mouse_state & MouseButtons.Right) != 0) {
				result |= (int)MsgButtons.MK_RBUTTON;
			}

			Keys mods = ModifierKeys;
			if ((mods & Keys.Control) != 0) {
				result |= (int)MsgButtons.MK_CONTROL;
			}

			if ((mods & Keys.Shift) != 0) {
				result |= (int)MsgButtons.MK_SHIFT;
			}

			result |= Delta << 16;

			return (IntPtr)result;
		}

		private int NextTimeout (DateTime now)
		{
			int timeout = Int32.MaxValue; 
			lock (timer_list) {
				foreach (Timer timer in timer_list) {
					int next = (int) (timer.Expires - now).TotalMilliseconds;
					if (next < 0)
						return 0; // Have a timer that has already expired
					if (next < timeout)
						timeout = next;
				}
			}
			if (timeout < Timer.Minimum)
				timeout = Timer.Minimum;
			return timeout;
		}

		private void CheckTimers (DateTime now)
		{
			lock (timer_list) {
				int count = timer_list.Count;
				if (count == 0)
					return;
				for (int i = 0; i < timer_list.Count; i++) {
                                        Timer timer = (Timer) timer_list [i];
                                        if (timer.Enabled && timer.Expires <= now) {
                                                timer.FireTick ();
                                                timer.Update (now);
                                        }
                                }
			}
		}

		private void AddExpose (XEvent xevent)
		{
			HandleData data = (HandleData) handle_data [xevent.AnyEvent.window];
			if (data == null) {
				data = new HandleData ();
				handle_data [xevent.AnyEvent.window] = data;
			}

			if (!data.IsVisible) {
				return;
			}

			data.AddToInvalidArea (xevent.ExposeEvent.x, xevent.ExposeEvent.y,
					xevent.ExposeEvent.width, xevent.ExposeEvent.height);
				   
			if (!data.HasExpose) {
				message_queue.Enqueue (xevent);
				data.HasExpose = true;
			}
		}

		private void UpdateMessageQueue ()
		{
			DateTime now = DateTime.Now;

			int pending;
			lock (xlib_lock) {
				pending = XPending (DisplayHandle);
			}
			if (pending == 0) {
				if (Idle != null) {
					Idle (this, EventArgs.Empty);
				}
				lock (xlib_lock) {
					pending = XPending (DisplayHandle);
				}
			}

			if (pending == 0) {
				int timeout = NextTimeout (now);
				if (timeout > 0) {
#if __MonoCS__
					Syscall.poll (pollfds, (uint) pollfds.Length, timeout);
#endif
					pending = XPending (DisplayHandle);
				}
			}

			CheckTimers (now);

			if (pending == 0) {
				lock (xlib_lock) {
					pending = XPending (DisplayHandle);
				}
			}

			while (pending > 0) {
				XEvent xevent = new XEvent ();

				lock (xlib_lock) {
					XNextEvent (DisplayHandle, ref xevent);
				}
				
				switch (xevent.type) {
				case XEventName.Expose:
					AddExpose (xevent);
					break;
				case XEventName.KeyPress:
				case XEventName.KeyRelease:
				case XEventName.ButtonPress:
				case XEventName.ButtonRelease:
				case XEventName.MotionNotify:
				case XEventName.EnterNotify:
				case XEventName.LeaveNotify:
				case XEventName.ConfigureNotify:
				case XEventName.DestroyNotify:
				case XEventName.FocusIn:
				case XEventName.FocusOut:
				case XEventName.ClientMessage:
					message_queue.Enqueue (xevent);
					break;

				case XEventName.PropertyNotify:
					if (xevent.PropertyEvent.atom == (IntPtr)net_active_window) {
						Atom	actual_atom;
						int	actual_format;
						int	nitems;
						int	bytes_after;
						IntPtr	prop = IntPtr.Zero;
						IntPtr	prev_active;;

						prev_active = active_window;
						XGetWindowProperty(DisplayHandle, root_window, net_active_window, 0, 1, false, Atom.XA_WINDOW, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);
						if ((nitems > 0) && (prop != IntPtr.Zero)) {
							active_window = (IntPtr)Marshal.ReadInt32(prop);
							XFree(prop);

							if (prev_active != active_window) {
								if (prev_active != IntPtr.Zero) {
									PostMessage(prev_active, Msg.WM_ACTIVATE, (IntPtr)WindowActiveFlags.WA_INACTIVE, IntPtr.Zero);
								}
								if (active_window != IntPtr.Zero) {
									PostMessage(active_window, Msg.WM_ACTIVATE, (IntPtr)WindowActiveFlags.WA_ACTIVE, IntPtr.Zero);
								}
							}
							if (modal_window.Count == 0) {
								break;
							} else {
								// Modality handling, if we are modal and the new active window is one
								// of ours but not the modal one, switch back to the modal window

								if (NativeWindow.FindWindow(active_window) != null) {
									if (active_window != (IntPtr)modal_window.Peek()) {
										Activate((IntPtr)modal_window.Peek());
									}
								}
								break;
							}
						}
					}
					break;

				}

				lock (xlib_lock) {
					pending = XPending (DisplayHandle);
				}
			}
		}

		internal override bool GetMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax) {
			XEvent	xevent;

			if (message_queue.Count > 0) {
				xevent = (XEvent) message_queue.Dequeue ();
			} else {
				UpdateMessageQueue ();
				if (message_queue.Count > 0) {
					xevent = (XEvent) message_queue.Dequeue ();
				} else {
					msg.hwnd= IntPtr.Zero;
					msg.message = Msg.WM_ENTERIDLE;
					return true;
				}
			}

			msg.hwnd=xevent.AnyEvent.window;

			//
			// If you add a new event to this switch make sure to add it in
			// UpdateMessage also unless it is not coming through the X event system.
			//
			switch(xevent.type) {
				case XEventName.KeyPress: {
					//keyboard.KeyEvent (xevent.AnyEvent.window, xevent, ref msg);
					keyboard.KeyEvent (focus_hwnd, xevent, ref msg);
					break;
				}

				case XEventName.KeyRelease: {
					//keyboard.KeyEvent (xevent.AnyEvent.window, xevent, ref msg);
					keyboard.KeyEvent (focus_hwnd, xevent, ref msg);
					break;
				}

				case XEventName.ButtonPress: {
					switch(xevent.ButtonEvent.button) {
						case 1: {
							mouse_state |= MouseButtons.Left;
							msg.message=Msg.WM_LBUTTONDOWN;
							msg.wParam=GetMousewParam(0);
							break;
						}

						case 2: {
							mouse_state |= MouseButtons.Middle;
							msg.message=Msg.WM_MBUTTONDOWN;
							msg.wParam=GetMousewParam(0);
							break;
						}

						case 3: {
							mouse_state |= MouseButtons.Right;
							msg.message=Msg.WM_RBUTTONDOWN;
							msg.wParam=GetMousewParam(0);
							break;
						}

						case 4: {
							msg.message=Msg.WM_MOUSEWHEEL;
							msg.wParam=GetMousewParam(120);
							break;
						}

						case 5: {
							msg.message=Msg.WM_MOUSEWHEEL;
							msg.wParam=GetMousewParam(-120);
							break;
						}

					}

					msg.lParam=(IntPtr) (xevent.ButtonEvent.y << 16 | xevent.ButtonEvent.x);
					mouse_position.X=xevent.ButtonEvent.x;
					mouse_position.Y=xevent.ButtonEvent.y;

					if (!click_pending) {
						click_pending = true;
						click_pending_hwnd = msg.hwnd;
						click_pending_message = msg.message;
						click_pending_wparam = msg.wParam;
						click_pending_lparam = msg.lParam;
						click_pending_time = (long)xevent.ButtonEvent.time;
					} else {
						if ((((long)xevent.ButtonEvent.time - click_pending_time)<double_click_interval) && (msg.wParam == click_pending_wparam) && (msg.lParam == click_pending_lparam) && (msg.message == click_pending_message)) {
							// Looks like a genuine double click, clicked twice on the same spot with the same keys
							switch(xevent.ButtonEvent.button) {
								case 1: {
									msg.message=Msg.WM_LBUTTONDBLCLK;
									break;
								}

								case 2: {
									msg.message=Msg.WM_MBUTTONDBLCLK;
									break;
								}

								case 3: {
									msg.message=Msg.WM_RBUTTONDBLCLK;
									break;
								}
							}
						}
						click_pending = false;
					}

					break;
				}

				case XEventName.ButtonRelease: {
					switch(xevent.ButtonEvent.button) {
						case 1: {
							mouse_state &= ~MouseButtons.Left;
							msg.message=Msg.WM_LBUTTONUP;
							msg.wParam=GetMousewParam(0);
							break;
						}

						case 2: {
							mouse_state &= ~MouseButtons.Middle;
							msg.message=Msg.WM_MBUTTONUP;
							msg.wParam=GetMousewParam(0);
							break;
						}

						case 3: {
							mouse_state &= ~MouseButtons.Right;
							msg.message=Msg.WM_RBUTTONUP;
							msg.wParam=GetMousewParam(0);
							break;
						}

						case 4: {
							return true;
						}

						case 5: {
							return true;
						}
					}

					msg.lParam=(IntPtr) (xevent.ButtonEvent.y << 16 | xevent.ButtonEvent.x);
					mouse_position.X=xevent.ButtonEvent.x;
					mouse_position.Y=xevent.ButtonEvent.y;
					break;
				}

				case XEventName.MotionNotify: {
					NativeWindow.WndProc(msg.hwnd, Msg.WM_SETCURSOR, msg.hwnd, (IntPtr)HitTest.HTCLIENT);

					msg.message = Msg.WM_MOUSEMOVE;
					msg.wParam = GetMousewParam(0);
					msg.lParam = (IntPtr) (xevent.MotionEvent.y << 16 | xevent.MotionEvent.x);
					mouse_position.X = xevent.MotionEvent.x;
					mouse_position.Y = xevent.MotionEvent.y;
					hover.x = mouse_position.X;
					hover.y = mouse_position.Y;
					hover.timer.Interval = hover.interval;
					break;
				}

				case XEventName.EnterNotify: {
					if (xevent.CrossingEvent.mode != NotifyMode.NotifyNormal) {
						return true;
					}
					msg.message=Msg.WM_MOUSE_ENTER;
					hover.timer.Enabled = true;
					hover.hwnd = msg.hwnd;
					break;
				}

				case XEventName.LeaveNotify: {
					if (xevent.CrossingEvent.mode != NotifyMode.NotifyNormal) {
						return true;
					}
					msg.message=Msg.WM_MOUSE_LEAVE;
					hover.timer.Enabled = false;
					hover.hwnd = IntPtr.Zero;
					break;
				}

				case XEventName.ConfigureNotify: {
					msg.message=Msg.WM_WINDOWPOSCHANGED;
					msg.wParam=IntPtr.Zero;
					msg.lParam=IntPtr.Zero;

					break;
				}

				case XEventName.FocusIn: {
					//Console.WriteLine("Received focus: {0}", xevent.FocusChangeEvent.detail);
					msg.message=Msg.WM_SETFOCUS;
					msg.wParam=IntPtr.Zero;
					msg.lParam=IntPtr.Zero;
					break;
				}

				case XEventName.FocusOut: {
					//Console.WriteLine("Lost focus: {0}", xevent.FocusChangeEvent.detail);
					msg.message=Msg.WM_KILLFOCUS;
					msg.wParam=IntPtr.Zero;
					msg.lParam=IntPtr.Zero;
					break;
				}

				case XEventName.Expose: {
					if (caret.visible == 1) {
						caret.paused = true;
						HideCaret();
					}

					NativeWindow.WndProc(msg.hwnd, Msg.WM_ERASEBKGND, IntPtr.Zero, IntPtr.Zero);

					if (caret.visible == 1) {
						ShowCaret();
						caret.paused = false;
					}

					msg.message=Msg.WM_PAINT;
					msg.wParam=IntPtr.Zero;
					msg.lParam=IntPtr.Zero;
					break;
				}

				case XEventName.DestroyNotify: {
					msg.message=Msg.WM_DESTROY;
					msg.wParam=IntPtr.Zero;
					msg.lParam=IntPtr.Zero;
					break;
				}

				case XEventName.ClientMessage: {
					if (xevent.ClientMessageEvent.message_type == (IntPtr)async_method) {
						GCHandle handle = (GCHandle)xevent.ClientMessageEvent.ptr1;
						AsyncMethodData data = (AsyncMethodData) handle.Target;
						AsyncMethodResult result = data.Result.Target as AsyncMethodResult;
						object ret = data.Method.DynamicInvoke (data.Args);
						if (result != null)
							result.Complete (ret);
						handle.Free ();
						break;
					}

					if (xevent.ClientMessageEvent.message_type == (IntPtr)hover.hevent) {
						msg.message = Msg.WM_MOUSEHOVER;
						msg.wParam = GetMousewParam(0);
						msg.lParam = (IntPtr) (xevent.ClientMessageEvent.ptr1);
						break;
					}

					if (xevent.ClientMessageEvent.message_type == (IntPtr) post_message) {
						msg.message = (Msg) xevent.ClientMessageEvent.ptr1.ToInt32 ();
						msg.hwnd = xevent.ClientMessageEvent.window;
						msg.wParam = xevent.ClientMessageEvent.ptr2;
						msg.lParam = xevent.ClientMessageEvent.ptr3;
						break;
					}

					if  (xevent.ClientMessageEvent.message_type == (IntPtr)wm_protocols) {
						if (xevent.ClientMessageEvent.ptr1 == (IntPtr)wm_delete_window) {
							msg.message = Msg.WM_CLOSE;
							msg.wParam = IntPtr.Zero;
							msg.lParam = IntPtr.Zero;
							break;
						}

						// We should not get this, but I'll leave the code in case we need it in the future
						if (xevent.ClientMessageEvent.ptr1 == (IntPtr)wm_take_focus) {
							msg.message = Msg.WM_NULL;
							break;
						}
					}
					break;
				}

				case XEventName.TimerNotify: {
					xevent.TimerNotifyEvent.handler (this, EventArgs.Empty);
					break;
				}
		                        
				default: {
					msg.message = Msg.WM_NULL;
					break;
				}
			}

			return getmessage_ret;
		}

		internal override bool TranslateMessage(ref MSG msg) {
			return keyboard.TranslateMessage (ref msg);
		}

		internal override IntPtr DispatchMessage(ref MSG msg) {
			return NativeWindow.WndProc(msg.hwnd, msg.message, msg.wParam, msg.lParam);
		}

		internal override bool SetZOrder(IntPtr hWnd, IntPtr AfterhWnd, bool Top, bool Bottom) {
			if (Top) {
				XRaiseWindow(DisplayHandle, hWnd);
				return true;
			} else if (!Bottom) {
				XWindowChanges	values = new XWindowChanges();

				values.sibling = AfterhWnd;
				values.stack_mode = StackMode.Below;

				XConfigureWindow(DisplayHandle, hWnd, ChangeWindowFlags.CWStackMode | ChangeWindowFlags.CWSibling, ref values);
			} else {
				XLowerWindow(DisplayHandle, hWnd);
				return true;
			}
			return false;
		}

		internal override bool SetTopmost(IntPtr hWnd, IntPtr hWndOwner, bool Enabled) {
			if (Enabled) {
				if (hWndOwner == IntPtr.Zero) {
					hWndOwner = FosterParent;
				}
				XSetTransientForHint(DisplayHandle, hWnd, hWndOwner);
			} else {
				int	trans_prop;

				trans_prop = XInternAtom(DisplayHandle, "WM_TRANSIENT_FOR", false);
				XDeleteProperty(DisplayHandle, hWnd, trans_prop);
			}
			return true;
		}

		internal override bool Text(IntPtr handle, string text) {
#if notdef
			XTextProperty	property = new XTextProperty();

			property.encoding=
			XSetWMName(DisplayHandle, handle, ref property);
#else
			lock (xlib_lock) {
				XStoreName(DisplayHandle, handle, text);
			}
#endif
			return true;
		}

		internal override bool GetText(IntPtr handle, out string text) {
			IntPtr	textptr;

			textptr = IntPtr.Zero;

			lock (xlib_lock) {
				XFetchName(DisplayHandle, handle, ref textptr);
			}
			if (textptr != IntPtr.Zero) {
				text = Marshal.PtrToStringAnsi(textptr);
				XFree(textptr);
				return true;
			} else {
				text = "";
				return false;
			}
		}

		internal override bool SetVisible(IntPtr handle, bool visible) {
			HandleData data = (HandleData) handle_data [handle];

			if (data == null) {
				data = new HandleData ();
				handle_data [handle] = data;
			}

			data.IsVisible = visible;

			lock (xlib_lock) {
				if (visible) {
					XMapWindow(DisplayHandle, handle);
				} else {
					XUnmapWindow(DisplayHandle, handle);
				}
			}
			return true;
		}

		internal override bool IsVisible(IntPtr handle) {
			HandleData data = (HandleData) handle_data [handle];

			if (data == null || data.IsVisible == true) {
				return true;
			}
			return false;
		}

		internal override IntPtr SetParent(IntPtr handle, IntPtr parent) {
			XWindowAttributes	attributes=new XWindowAttributes();

			lock (xlib_lock) {
				XGetWindowAttributes(DisplayHandle, handle, ref attributes);
				XReparentWindow(DisplayHandle, handle, parent, attributes.x, attributes.y);
			}
			return IntPtr.Zero;
		}

		internal override IntPtr GetParent(IntPtr handle) {
			IntPtr	Root;
			IntPtr	Parent;
			IntPtr	Children;
			int	ChildCount;

			Root=IntPtr.Zero;
			Parent=IntPtr.Zero;
			Children=IntPtr.Zero;
			ChildCount=0;

			lock (xlib_lock) {
				XQueryTree(DisplayHandle, handle, ref Root, ref Parent, ref Children, ref ChildCount);
			}

			if (Children!=IntPtr.Zero) {
				lock (xlib_lock) {
					XFree(Children);
				}
			}
			return Parent;
		}

		internal override void GrabWindow(IntPtr hWnd, IntPtr confine_hwnd) {
			if (confine_hwnd != IntPtr.Zero) {
				XWindowAttributes	attributes = new XWindowAttributes();

				lock (xlib_lock) {
					XGetWindowAttributes(DisplayHandle, confine_hwnd, ref attributes);
				}
				grab_area.X = attributes.x;
				grab_area.Y = attributes.y;
				grab_area.Width = attributes.width;
				grab_area.Height = attributes.height;
				grab_confined = true;
			}
			grab_hwnd = hWnd;
			lock (xlib_lock) {
				XGrabPointer(DisplayHandle, hWnd, false,
					EventMask.ButtonPressMask | EventMask.ButtonMotionMask |
					EventMask.ButtonReleaseMask | EventMask.PointerMotionMask,
					GrabMode.GrabModeAsync, GrabMode.GrabModeAsync, confine_hwnd, 0, 0);
			}
		}

		internal override void GrabInfo(out IntPtr hWnd, out bool GrabConfined, out Rectangle GrabArea) {
			hWnd = grab_hwnd;
			GrabConfined = grab_confined;
			GrabArea = grab_area;
		}

		internal override void ReleaseWindow(IntPtr hWnd) {
			lock (xlib_lock) {
				XUngrabPointer(DisplayHandle, 0);
				grab_hwnd = IntPtr.Zero;
				grab_confined = false;
			}
		}

		internal override bool CalculateWindowRect(IntPtr hWnd, ref Rectangle ClientRect, int Style, bool HasMenu, out Rectangle WindowRect) {
			WindowRect = new Rectangle(ClientRect.Left, ClientRect.Top, ClientRect.Width, ClientRect.Height);
			return true;
		}

		internal override void SetCursor(IntPtr window, IntPtr cursor) {
			if (override_cursor == IntPtr.Zero) {
				if ((last_window == window) && (last_cursor == cursor)) {
					return;
				}

				last_cursor = cursor;
				last_window = window;

				XDefineCursor(DisplayHandle, window, cursor);
				return;
			}
			XDefineCursor(DisplayHandle, window, override_cursor);
		}

		internal override void ShowCursor(bool show) {
			;	// FIXME - X11 doesn't 'hide' the cursor. we could create an empty cursor
		}

		internal override void OverrideCursor(IntPtr cursor) {
			override_cursor = cursor;
		}

		internal override IntPtr DefineCursor(Bitmap bitmap, Bitmap mask, Color cursor_pixel, Color mask_pixel, int xHotSpot, int yHotSpot) {
			IntPtr	cursor;
			Bitmap	cursor_bitmap;
			Bitmap	cursor_mask;
			Byte[]	cursor_bits;
			Byte[]	mask_bits;
			Color	c_pixel;
			Color	m_pixel;
			int	width;
			int	height;
			IntPtr	cursor_pixmap;
			IntPtr	mask_pixmap;
			XColor	fg;
			XColor	bg;
			bool	and;
			bool	xor;

			if (XQueryBestCursor(DisplayHandle, root_window, bitmap.Width, bitmap.Height, out width, out height) == 0) {
				return IntPtr.Zero;
			}

			// Win32 only allows creation cursors of a certain size
			if ((bitmap.Width != width) || (bitmap.Width != height)) {
				cursor_bitmap = new Bitmap(bitmap, new Size(width, height));
				cursor_mask = new Bitmap(mask, new Size(width, height));
			} else {
				cursor_bitmap = bitmap;
				cursor_mask = mask;
			}

			width = cursor_bitmap.Width;
			height = cursor_bitmap.Height;

			cursor_bits = new Byte[(width / 8) * height];
			mask_bits = new Byte[(width / 8) * height];

			for (int y = 0; y < height; y++) {
				for (int x = 0; x < width; x++) {
					c_pixel = cursor_bitmap.GetPixel(x, y);
					m_pixel = cursor_mask.GetPixel(x, y);

					and = c_pixel == cursor_pixel;
					xor = m_pixel == mask_pixel;

					if (!and && !xor) {
						// Black
						// cursor_bits[y * width / 8 + x / 8] &= (byte)~((1 << (x % 8)));	// The bit already is 0
						mask_bits[y * width / 8 + x / 8] |= (byte)(1 << (x % 8));
					} else if (!and && xor) {
						// White
						cursor_bits[y * width / 8 + x / 8] |= (byte)(1 << (x % 8));
						mask_bits[y * width / 8 + x / 8] |= (byte)(1 << (x % 8));
					#if notneeded
					} else if (and && !xor) {
						// Screen
					} else if (and && xor) {
						// Inverse Screen

						// X11 doesn't know the 'reverse screen' concept, so we'll treat them the same
						// we want both to be 0 so nothing to be done
						//cursor_bits[y * width / 8 + x / 8] &= (byte)~((1 << (x % 8)));
						//mask_bits[y * width / 8 + x / 8] |= (byte)(01 << (x % 8));
					#endif
					}
				}
			}

			cursor_pixmap = XCreatePixmapFromBitmapData(DisplayHandle, root_window, cursor_bits, width, height, (IntPtr)1, (IntPtr)0, 1);
			mask_pixmap = XCreatePixmapFromBitmapData(DisplayHandle, root_window, mask_bits, width, height, (IntPtr)1, (IntPtr)0, 1);
			fg = new XColor();
			bg = new XColor();

			fg.pixel = XWhitePixel(DisplayHandle, screen_num);
			fg.red = (ushort)65535;
			fg.green = (ushort)65535;
			fg.blue = (ushort)65535;

			bg.pixel = XBlackPixel(DisplayHandle, screen_num);

			cursor = XCreatePixmapCursor(DisplayHandle, cursor_pixmap, mask_pixmap, ref fg, ref bg, xHotSpot, yHotSpot);

			XFreePixmap(DisplayHandle, cursor_pixmap);
			XFreePixmap(DisplayHandle, mask_pixmap);

			return cursor;
		}

		[MonoTODO("Define our own bitmaps for cursors to match Win32")]
		internal override IntPtr DefineStdCursor(StdCursor id) {
			switch(id) {
				case StdCursor.AppStarting:	return XCreateFontCursor(DisplayHandle, CursorFontShape.XC_watch);
				case StdCursor.Arrow:		return IntPtr.Zero;
				case StdCursor.Cross:		return XCreateFontCursor(DisplayHandle, CursorFontShape.XC_crosshair);
				case StdCursor.Default:		return IntPtr.Zero;
				case StdCursor.Hand:		return XCreateFontCursor(DisplayHandle, CursorFontShape.XC_hand2);
				case StdCursor.Help:		return XCreateFontCursor(DisplayHandle, CursorFontShape.XC_question_arrow);
				case StdCursor.HSplit:		return XCreateFontCursor(DisplayHandle, CursorFontShape.XC_sb_h_double_arrow);
				case StdCursor.IBeam:		return XCreateFontCursor(DisplayHandle, CursorFontShape.XC_xterm);
				case StdCursor.No:		return XCreateFontCursor(DisplayHandle, CursorFontShape.XC_circle);
				case StdCursor.NoMove2D:	return XCreateFontCursor(DisplayHandle, CursorFontShape.XC_fleur);
				case StdCursor.NoMoveHoriz:	return XCreateFontCursor(DisplayHandle, CursorFontShape.XC_fleur);
				case StdCursor.NoMoveVert:	return XCreateFontCursor(DisplayHandle, CursorFontShape.XC_fleur);
				case StdCursor.PanEast:		return XCreateFontCursor(DisplayHandle, CursorFontShape.XC_fleur);
				case StdCursor.PanNE:		return XCreateFontCursor(DisplayHandle, CursorFontShape.XC_fleur);
				case StdCursor.PanNorth:	return XCreateFontCursor(DisplayHandle, CursorFontShape.XC_fleur);
				case StdCursor.PanNW:		return XCreateFontCursor(DisplayHandle, CursorFontShape.XC_fleur);
				case StdCursor.PanSE:		return XCreateFontCursor(DisplayHandle, CursorFontShape.XC_fleur);
				case StdCursor.PanSouth:	return XCreateFontCursor(DisplayHandle, CursorFontShape.XC_fleur);
				case StdCursor.PanSW:		return XCreateFontCursor(DisplayHandle, CursorFontShape.XC_fleur);
				case StdCursor.PanWest:		return XCreateFontCursor(DisplayHandle, CursorFontShape.XC_sizing);
				case StdCursor.SizeAll:		return XCreateFontCursor(DisplayHandle, CursorFontShape.XC_fleur);
				case StdCursor.SizeNESW:	return XCreateFontCursor(DisplayHandle, CursorFontShape.XC_sizing);
				case StdCursor.SizeNS:		return XCreateFontCursor(DisplayHandle, CursorFontShape.XC_fleur);
				case StdCursor.SizeNWSE:	return XCreateFontCursor(DisplayHandle, CursorFontShape.XC_bottom_right_corner);
				case StdCursor.SizeWE:		return XCreateFontCursor(DisplayHandle, CursorFontShape.XC_fleur);
				case StdCursor.UpArrow:		return XCreateFontCursor(DisplayHandle, CursorFontShape.XC_center_ptr);
				case StdCursor.VSplit:		return XCreateFontCursor(DisplayHandle, CursorFontShape.XC_sb_v_double_arrow);
				case StdCursor.WaitCursor:	return XCreateFontCursor(DisplayHandle, CursorFontShape.XC_watch);
				default:			return IntPtr.Zero;
			}
		}

		internal override void DestroyCursor(IntPtr cursor) {
			XFreeCursor(DisplayHandle, cursor);
		}


		internal override void GetCursorInfo(IntPtr cursor, out int width, out int height, out int hotspot_x, out int hotspot_y) {
			throw new NotImplementedException ();
		}

		internal override void SetCursorPos(IntPtr handle, int x, int y) {
			lock (xlib_lock) {
				XWarpPointer(DisplayHandle, IntPtr.Zero, (handle!=IntPtr.Zero) ? handle : IntPtr.Zero, 0, 0, 0, 0, x, y);
			}
		}

		internal override void GetCursorPos(IntPtr handle, out int x, out int y) {
			IntPtr	root;
			IntPtr	child;
			int	root_x;
			int	root_y;
			int	win_x;
			int	win_y;
			int	keys_buttons;

			lock (xlib_lock) {
				XQueryPointer(DisplayHandle, (handle!=IntPtr.Zero) ? handle : root_window,
						out root, out child, out root_x, out root_y,
						out win_x, out win_y, out keys_buttons);
			}

			if (handle != IntPtr.Zero) {
				x = win_x;
				y = win_y;
			} else {
				x = root_x;
				y = root_y;
			}
		}

		internal override void ScreenToClient(IntPtr handle, ref int x, ref int y)
		{
			int	dest_x_return;
			int	dest_y_return;
			IntPtr	child;

			lock (xlib_lock) {
				XTranslateCoordinates (DisplayHandle, root_window,
						handle, x, y, out dest_x_return, out dest_y_return, out child);
			}

			x = dest_x_return;
			y = dest_y_return;
		}

		internal override void ClientToScreen(IntPtr handle, ref int x, ref int y) {
			int	dest_x_return;
			int	dest_y_return;
			IntPtr	child;

			lock (xlib_lock) {
				XTranslateCoordinates (DisplayHandle, handle, root_window,
					x, y, out dest_x_return, out dest_y_return, out child);
			}

			x = dest_x_return;
			y = dest_y_return;
		}

		internal override void SendAsyncMethod (AsyncMethodData method)
		{
			XEvent xevent = new XEvent ();

			xevent.type = XEventName.ClientMessage;
			xevent.ClientMessageEvent.display = DisplayHandle;
			xevent.ClientMessageEvent.window = IntPtr.Zero;
			xevent.ClientMessageEvent.message_type = (IntPtr)async_method;
			xevent.ClientMessageEvent.format = 32;
			xevent.ClientMessageEvent.ptr1 = (IntPtr) GCHandle.Alloc (method);

			message_queue.EnqueueLocked (xevent);

			WakeupMain ();
		}

		// must be called from main thread
		public static void PostMessage (IntPtr hwnd, Msg message, IntPtr wparam, IntPtr lparam)
		{
			XEvent xevent = new XEvent ();

			xevent.type = XEventName.ClientMessage;
			xevent.ClientMessageEvent.display = DisplayHandle;
			xevent.ClientMessageEvent.window = hwnd;
			xevent.ClientMessageEvent.message_type = (IntPtr) post_message;
			xevent.ClientMessageEvent.format = 32;
			xevent.ClientMessageEvent.ptr1 = (IntPtr) message;
			xevent.ClientMessageEvent.ptr2 = wparam;
			xevent.ClientMessageEvent.ptr3 = lparam;

			message_queue.Enqueue (xevent);
		}

		private void WakeupMain ()
		{
			wake.BeginSend (new byte [] { 0xFF }, 0, 1, SocketFlags.None, null, null);
		}

		internal override void SetTimer (Timer timer)
		{
			lock (timer_list) {
				timer_list.Add (timer);
			}
			WakeupMain ();
		}

		internal override void KillTimer (Timer timer)
		{
			lock (timer_list) {
				timer_list.Remove (timer);
			}
		}

		internal static void ShowCaret() {
			if ((caret.gc == IntPtr.Zero) || caret.on) {
				return;
			}
			caret.on = true;

			XDrawLine(DisplayHandle, caret.hwnd, caret.gc, caret.x, caret.y, caret.x, caret.y + caret.height);
		}

		internal static void HideCaret() {
			if ((caret.gc == IntPtr.Zero) || !caret.on) {
				return;
			}
			caret.on = false;

			XDrawLine(DisplayHandle, caret.hwnd, caret.gc, caret.x, caret.y, caret.x, caret.y + caret.height);
		}

		// Automatically destroys any previous caret
		internal override void CreateCaret(IntPtr hwnd, int width, int height) {
			XGCValues	gc_values;

			if (caret.hwnd != IntPtr.Zero) {
				DestroyCaret(caret.hwnd);
			}
			caret.hwnd = hwnd;
			caret.width = width;
			caret.height = height;
			caret.visible = 0;
			caret.on = false;

			gc_values = new XGCValues();

			gc_values.line_width = caret.width;
			caret.gc = XCreateGC(DisplayHandle, hwnd, GCFunction.GCLineWidth, ref gc_values);
			if (caret.gc == IntPtr.Zero) {
				caret.hwnd = IntPtr.Zero;
				return;
			}

			XSetFunction(DisplayHandle, caret.gc, GXFunction.GXinvert);
		}

		// Only destroy if the hwnd is the hwnd of the current caret
		internal override void DestroyCaret(IntPtr hwnd) {
			if (caret.hwnd == hwnd) {
				if (caret.visible == 1) {
					caret.timer.Stop();
					HideCaret();
				}
				if (caret.gc != IntPtr.Zero) {
					XFreeGC(DisplayHandle, caret.gc);
					caret.gc = IntPtr.Zero;
				}
				caret.hwnd = IntPtr.Zero;
				caret.visible = 0;
				caret.on = false;
			}
		}

		// When setting the position we restart the blink interval
		internal override void SetCaretPos(IntPtr hwnd, int x, int y) {
			if (caret.hwnd == hwnd) {
				caret.timer.Stop();
				HideCaret();

				caret.x = x;
				caret.y = y;

				if (caret.visible == 1) {
					ShowCaret();
					caret.timer.Start();
				}
			}
		}

		// Visible is cumulative; two hides require two shows before the caret is visible again
		internal override void CaretVisible(IntPtr hwnd, bool visible) {
			if (caret.hwnd == hwnd) {
				if (visible) {
					if (caret.visible < 1) {
						caret.visible++;
						caret.on = false;
						if (caret.visible == 1) {
							ShowCaret();
							caret.timer.Start();
						}
					}
				} else {
					caret.visible--;
					if (caret.visible == 0) {
						caret.timer.Stop();
						HideCaret();
					}
				}
			}
		}

		internal override void SetFocus(IntPtr hwnd) {
			if (focus_hwnd != IntPtr.Zero) {
				PostMessage(focus_hwnd, Msg.WM_KILLFOCUS, hwnd, IntPtr.Zero);
			}
			PostMessage(hwnd, Msg.WM_SETFOCUS, focus_hwnd, IntPtr.Zero);
			focus_hwnd = hwnd;

			//XSetInputFocus(DisplayHandle, hwnd, RevertTo.None, IntPtr.Zero);
		}

		internal override IntPtr GetActive() {
			Atom	actual_atom;
			int	actual_format;
			int	nitems;
			int	bytes_after;
			IntPtr	prop = IntPtr.Zero;
			IntPtr	active = IntPtr.Zero;

			XGetWindowProperty(DisplayHandle, root_window, net_active_window, 0, 1, false, Atom.XA_WINDOW, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);
			if ((nitems > 0) && (prop != IntPtr.Zero)) {
				active = (IntPtr)Marshal.ReadInt32(prop);
				XFree(prop);
			}

			return active;
		}

		internal override bool GetFontMetrics(Graphics g, Font font, out int ascent, out int descent) {
			return GetFontMetrics(g.GetHdc(), font.ToHfont(), out ascent, out descent);
		}

		internal override void ScrollWindow(IntPtr hwnd, Rectangle area, int XAmount, int YAmount, bool clear) {
			IntPtr		gc;
			XGCValues	gc_values;

			gc_values = new XGCValues();

			gc = XCreateGC(DisplayHandle, hwnd, 0, ref gc_values);

			XCopyArea(DisplayHandle, hwnd, hwnd, gc, area.X - XAmount, area.Y - YAmount, area.Width, area.Height, area.X, area.Y);

			// Generate an expose for the area exposed by the horizontal scroll
			if (XAmount > 0) {
				XClearArea(DisplayHandle, hwnd, area.X, area.Y, XAmount, area.Height, clear);
			} else if (XAmount < 0) {
				XClearArea(DisplayHandle, hwnd, XAmount + area.X + area.Width, area.Y, -XAmount, area.Height, clear);
			}

			// Generate an expose for the area exposed by the vertical scroll
			if (YAmount > 0) {
				XClearArea(DisplayHandle, hwnd, area.X, area.Y, area.Width, YAmount, clear);
			} else if (YAmount < 0) {
				XClearArea(DisplayHandle, hwnd, area.X, YAmount + area.Y + area.Height, area.Width, -YAmount, clear);
			}

			XFreeGC(DisplayHandle, gc);
		}

		internal override void ScrollWindow(IntPtr hwnd, int XAmount, int YAmount, bool clear) {
			IntPtr		gc;
			XGCValues	gc_values;
			IntPtr		root;
			int		x;
			int		y;
			int		width;
			int		height;
			int		border_width;
			int		depth;

			// We're abusing clear_width and height, here, don't want two extra vars, we don't use the results here
			XGetGeometry(DisplayHandle, hwnd, out root, out x, out y, out width, out height, out border_width, out depth);

			gc_values = new XGCValues();

			gc = XCreateGC(DisplayHandle, hwnd, 0, ref gc_values);

			XCopyArea(DisplayHandle, hwnd, hwnd, gc, -XAmount, -YAmount, width, height, 0, 0);

			// Generate an expose for the area exposed by the horizontal scroll
			if (XAmount > 0) {
				XClearArea(DisplayHandle, hwnd, 0, 0, XAmount, height, clear);
			} else if (XAmount < 0) {
                                XClearArea(DisplayHandle, hwnd, XAmount + width, 0, -XAmount, height, clear);
			}

			// Generate an expose for the area exposed by the vertical scroll
			if (YAmount > 0) {
				XClearArea(DisplayHandle, hwnd, 0, 0, width, YAmount, clear);
			} else if (YAmount < 0) {
				XClearArea(DisplayHandle, hwnd, 0, YAmount + height, width, -YAmount, clear);
			}

			XFreeGC(DisplayHandle, gc);
		}



		// Santa's little helper
		static void Where() 
		{
			Console.WriteLine("Here: {0}", new StackTrace().ToString());
		}
		#endregion	// Public Static Methods

		#region X11 Imports
		[DllImport ("libX11", EntryPoint="XOpenDisplay")]
		internal extern static IntPtr XOpenDisplay(IntPtr display);
		[DllImport ("libX11", EntryPoint="XCloseDisplay")]
		internal extern static int XCloseDisplay(IntPtr display);						    

		[DllImport ("libX11", EntryPoint="XCreateWindow")]
		internal extern static IntPtr XCreateWindow(IntPtr display, IntPtr parent, int x, int y, int width, int height, int border_width, int depth, int xclass, IntPtr visual, SetWindowValuemask valuemask, ref XSetWindowAttributes attributes);
		[DllImport ("libX11", EntryPoint="XCreateSimpleWindow")]
		internal extern static IntPtr XCreateSimpleWindow(IntPtr display, IntPtr parent, int x, int y, int width, int height, int border_width, int border, int background);
		[DllImport ("libX11", EntryPoint="XMapWindow")]
		internal extern static int XMapWindow(IntPtr display, IntPtr window);
		[DllImport ("libX11", EntryPoint="XUnmapWindow")]
		internal extern static int XUnmapWindow(IntPtr display, IntPtr window);
		[DllImport ("libX11", EntryPoint="XMapSubwindows")]
		internal extern static int XMapSubindows(IntPtr display, IntPtr window);
		[DllImport ("libX11", EntryPoint="XUnmapSubwindows")]
		internal extern static int XUnmapSubwindows(IntPtr display, IntPtr window);
		[DllImport ("libX11", EntryPoint="XRootWindow")]
		internal extern static IntPtr XRootWindow(IntPtr display, int screen_number);
		[DllImport ("libX11", EntryPoint="XNextEvent")]
		internal extern static IntPtr XNextEvent(IntPtr display, ref XEvent xevent);
		[DllImport ("libX11")]
		internal extern static int XConnectionNumber (IntPtr diplay);
		[DllImport ("libX11")]
		internal extern static int XPending (IntPtr diplay);
		[DllImport ("libX11")]
		internal extern static bool XCheckWindowEvent (IntPtr display, IntPtr window, EventMask mask, ref XEvent xevent);
		[DllImport ("libX11")]
		internal extern static bool XCheckMaskEvent (IntPtr display, EventMask mask, ref XEvent xevent);
		[DllImport ("libX11", EntryPoint="XSelectInput")]
		internal extern static IntPtr XSelectInput(IntPtr display, IntPtr window, EventMask mask);

		[DllImport ("libX11", EntryPoint="XDestroyWindow")]
		internal extern static int XDestroyWindow(IntPtr display, IntPtr window);

		[DllImport ("libX11", EntryPoint="XReparentWindow")]
		internal extern static int XReparentWindow(IntPtr display, IntPtr window, IntPtr parent, int x, int y);
		[DllImport ("libX11", EntryPoint="XMoveResizeWindow")]
		internal extern static int XMoveResizeWindow(IntPtr display, IntPtr window, int x, int y, int width, int height);

		[DllImport ("libX11", EntryPoint="XResizeWindow")]
		internal extern static int XResizeWindow(IntPtr display, IntPtr window, int width, int height);

		[DllImport ("libX11", EntryPoint="XGetWindowAttributes")]
		internal extern static int XGetWindowAttributes(IntPtr display, IntPtr window, ref XWindowAttributes attributes);

		[DllImport ("libX11", EntryPoint="XFlush")]
		internal extern static int XFlush(IntPtr display);

		[DllImport ("libX11", EntryPoint="XSetWMName")]
		internal extern static int XSetWMName(IntPtr display, IntPtr window, ref XTextProperty text_prop);

		[DllImport ("libX11", EntryPoint="XStoreName")]
		internal extern static int XStoreName(IntPtr display, IntPtr window, string window_name);

		[DllImport ("libX11", EntryPoint="XFetchName")]
		internal extern static int XFetchName(IntPtr display, IntPtr window, ref IntPtr window_name);

		[DllImport ("libX11", EntryPoint="XSendEvent")]
		internal extern static int XSendEvent(IntPtr display, IntPtr window, bool propagate, EventMask event_mask, ref XEvent send_event);

		[DllImport ("libX11", EntryPoint="XQueryTree")]
		internal extern static int XQueryTree(IntPtr display, IntPtr window, ref IntPtr root_return, ref IntPtr parent_return, ref IntPtr children_return, ref int nchildren_return);

		[DllImport ("libX11", EntryPoint="XFree")]
		internal extern static int XFree(IntPtr data);

		[DllImport ("libX11", EntryPoint="XRaiseWindow")]
		internal extern static int XRaiseWindow(IntPtr display, IntPtr window);

		[DllImport ("libX11", EntryPoint="XLowerWindow")]
		internal extern static uint XLowerWindow(IntPtr display, IntPtr window);

		[DllImport ("libX11", EntryPoint="XConfigureWindow")]
		internal extern static uint XConfigureWindow(IntPtr display, IntPtr window, ChangeWindowFlags value_mask, ref XWindowChanges values);

		[DllImport ("libX11", EntryPoint="XInternAtom")]
		internal extern static int XInternAtom(IntPtr display, string atom_name, bool only_if_exists);

		[DllImport ("libX11", EntryPoint="XSetWMProtocols")]
		internal extern static int XSetWMProtocols(IntPtr display, IntPtr window, IntPtr[] protocols, int count);

		[DllImport ("libX11", EntryPoint="XGrabPointer")]
		internal extern static int XGrabPointer(IntPtr display, IntPtr window, bool owner_events, EventMask event_mask, GrabMode pointer_mode, GrabMode keyboard_mode, IntPtr confine_to, uint cursor, uint timestamp);

		[DllImport ("libX11", EntryPoint="XUngrabPointer")]
		internal extern static int XUngrabPointer(IntPtr display, uint timestamp);

		[DllImport ("libX11", EntryPoint="XQueryPointer")]
		internal extern static bool XQueryPointer(IntPtr display, IntPtr window, out IntPtr root, out IntPtr child, out int root_x, out int root_y, out int win_x, out int win_y, out int keys_buttons);

		[DllImport ("libX11", EntryPoint="XTranslateCoordinates")]
		internal extern static bool XTranslateCoordinates (IntPtr display, IntPtr src_w, IntPtr dest_w, int src_x, int src_y, out int intdest_x_return,	 out int dest_y_return, out IntPtr child_return);

		[DllImport ("libX11", EntryPoint="XGetGeometry")]
		internal extern static bool XGetGeometry(IntPtr display, IntPtr window, out IntPtr root, out int x, out int y, out int width, out int height, out int border_width, out int depth);

		[DllImport ("libX11", EntryPoint="XGetGeometry")]
		internal extern static bool XGetGeometry(IntPtr display, IntPtr window, IntPtr root, out int x, out int y, out int width, out int height, IntPtr border_width, IntPtr depth);

		[DllImport ("libX11", EntryPoint="XGetGeometry")]
		internal extern static bool XGetGeometry(IntPtr display, IntPtr window, IntPtr root, out int x, out int y, IntPtr width, IntPtr height, IntPtr border_width, IntPtr depth);

		[DllImport ("libX11", EntryPoint="XGetGeometry")]
		internal extern static bool XGetGeometry(IntPtr display, IntPtr window, IntPtr root, IntPtr x, IntPtr y, out int width, out int height, IntPtr border_width, IntPtr depth);

		[DllImport ("libX11", EntryPoint="XWarpPointer")]
		internal extern static uint XWarpPointer(IntPtr display, IntPtr src_w, IntPtr dest_w, int src_x, int src_y, uint src_width, uint src_height, int dest_x, int dest_y);

		[DllImport ("libX11", EntryPoint="XClearWindow")]
		internal extern static int XClearWindow(IntPtr display, IntPtr window);

		[DllImport ("libX11", EntryPoint="XClearArea")]
		internal extern static int XClearArea(IntPtr display, IntPtr window, int x, int y, int width, int height, bool exposures);

		// Colormaps
		[DllImport ("libX11", EntryPoint="XDefaultScreenOfDisplay")]
		internal extern static IntPtr XDefaultScreenOfDisplay(IntPtr display);

		[DllImport ("libX11", EntryPoint="XScreenNumberOfScreen")]
		internal extern static int XScreenNumberOfScreen(IntPtr display, IntPtr Screen);

		[DllImport ("libX11", EntryPoint="XDefaultVisual")]
		internal extern static IntPtr XDefaultVisual(IntPtr display, int screen_number);

		[DllImport ("libX11", EntryPoint="XDefaultDepth")]
		internal extern static uint XDefaultDepth(IntPtr display, int screen_number);

		[DllImport ("libX11", EntryPoint="XDefaultColormap")]
		internal extern static IntPtr XDefaultColormap(IntPtr display, int screen_number);

		[DllImport ("libX11", EntryPoint="XLookupColor")]
		internal extern static int XLookupColor(IntPtr display, IntPtr Colormap, string Coloranem, ref XColor exact_def_color, ref XColor screen_def_color);

		[DllImport ("libX11", EntryPoint="XAllocColor")]
		internal extern static int XAllocColor(IntPtr display, IntPtr Colormap, ref XColor colorcell_def);

		[DllImport ("libX11", EntryPoint="XSetTransientForHint")]
		internal extern static int XSetTransientForHint(IntPtr display, IntPtr window, IntPtr prop_window);

		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty(IntPtr display, IntPtr window, int property, int type, int format, PropertyMode  mode, ref MotifWmHints data, int nelements);

		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty(IntPtr display, IntPtr window, int property, Atom format, int type, PropertyMode  mode, ref IntPtr[] atoms, int nelements);

		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty(IntPtr display, IntPtr window, int property, int format, int type, PropertyMode  mode, ref IntPtr[] atoms, int nelements);

		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty(IntPtr display, IntPtr window, int property, int format, int type, PropertyMode  mode, IntPtr data, int nelements);

		[DllImport ("libX11", EntryPoint="XDeleteProperty")]
		internal extern static int XDeleteProperty(IntPtr display, IntPtr window, int property);

		[DllImport ("gdiplus", EntryPoint="GetFontMetrics")]
		internal extern static bool GetFontMetrics(IntPtr graphicsObject, IntPtr nativeObject, out int ascent, out int descent);

		// Drawing
		[DllImport ("libX11", EntryPoint="XCreateGC")]
		internal extern static IntPtr XCreateGC(IntPtr display, IntPtr window, GCFunction valuemask, ref XGCValues values);

		[DllImport ("libX11", EntryPoint="XFreeGC")]
		internal extern static int XFreeGC(IntPtr display, IntPtr gc);

		[DllImport ("libX11", EntryPoint="XSetFunction")]
		internal extern static int XSetFunction(IntPtr display, IntPtr gc, GXFunction function);

		[DllImport ("libX11", EntryPoint="XDrawLine")]
		internal extern static int XDrawLine(IntPtr display, IntPtr drawable, IntPtr gc, int x1, int y1, int x2, int y2);

		[DllImport ("libX11", EntryPoint="XSetWindowBackground")]
		internal extern static int XSetWindowBackground(IntPtr display, IntPtr window, IntPtr background);

		[DllImport ("libX11", EntryPoint="XCopyArea")]
		internal extern static int XCopyArea(IntPtr display, IntPtr src, IntPtr dest, IntPtr gc, int src_x, int src_y, int width, int height, int dest_x, int dest_y);

		[DllImport ("libX11", EntryPoint="XGetAtomName")]
		internal extern static string XGetAtomName(IntPtr display, int atom);

		[DllImport ("libX11", EntryPoint="XGetWindowProperty")]
		internal extern static int XGetWindowProperty(IntPtr display, IntPtr window, int atom, int long_offset, int long_length, bool delete, Atom req_type, out Atom actual_type, out int actual_format, out int nitems, out int bytes_after, ref IntPtr prop);

		[DllImport ("libX11", EntryPoint="XSetInputFocus")]
		internal extern static int XSetInputFocus(IntPtr display, IntPtr window, RevertTo revert_to, IntPtr time);

		[DllImport ("libX11", EntryPoint="XIconifyWindow")]
		internal extern static int XIconifyWindow(IntPtr display, IntPtr window, int screen_number);

		[DllImport ("libX11", EntryPoint="XDefineCursor")]
		internal extern static int XDefineCursor(IntPtr display, IntPtr window, IntPtr cursor);

		[DllImport ("libX11", EntryPoint="XFreeCursor")]
		internal extern static int XFreeCursor(IntPtr display, IntPtr cursor);

		[DllImport ("libX11", EntryPoint="XCreateFontCursor")]
		internal extern static IntPtr XCreateFontCursor(IntPtr display, CursorFontShape shape);

		[DllImport ("libX11", EntryPoint="XCreatePixmapCursor")]
		internal extern static IntPtr XCreatePixmapCursor(IntPtr display, IntPtr source, IntPtr mask, ref XColor foreground_color, ref XColor background_color, int x_hot, int y_hot);

		[DllImport ("libX11", EntryPoint="XCreatePixmapFromBitmapData")]
		internal extern static IntPtr XCreatePixmapFromBitmapData(IntPtr display, IntPtr drawable, byte[] data, int width, int height, IntPtr fg, IntPtr bg, int depth);

		[DllImport ("libX11", EntryPoint="XFreePixmap")]
		internal extern static IntPtr XFreePixmap(IntPtr display, IntPtr pixmap);

		[DllImport ("libX11", EntryPoint="XQueryBestCursor")]
		internal extern static int XQueryBestCursor(IntPtr display, IntPtr drawable, int width, int height, out int best_width, out int best_height);

		[DllImport ("libX11", EntryPoint="XWhitePixel")]
		internal extern static IntPtr XWhitePixel(IntPtr display, int screen_no);

		[DllImport ("libX11", EntryPoint="XBlackPixel")]
		internal extern static IntPtr XBlackPixel(IntPtr display, int screen_no);
		#endregion
	}
}
