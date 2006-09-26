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
// Copyright (c) 2004-2006 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//
//

// NOTE:
//	This driver understands the following environment variables: (Set the var to enable feature)
//
//	MONO_XEXCEPTIONS	= throw an exception when a X11 error is encountered;
//				  by default a message is displayed but execution continues
//
//	MONO_XSYNC		= perform all X11 commands synchronous; this is slower but
//				  helps in debugging errors
//

// NOT COMPLETE

// define to log Window handles and relationships to stdout
#undef DriverDebug

// Extra detailed debug
#undef	DriverDebugExtra
#undef DriverDebugParent
#undef DriverDebugCreate
#undef DriverDebugDestroy
#undef DriverDebugThreads

using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

// Only do the poll when building with mono for now
#if __MonoCS__
using Mono.Unix.Native;
#endif

/// X11 Version
namespace System.Windows.Forms {
	internal class XplatUIX11 : XplatUIDriver {
		#region Local Variables
		// General
		static volatile XplatUIX11	Instance;
		private static int		RefCount;
		private static object		XlibLock;		// Our locking object
		private static bool		themes_enabled;

		// General X11
		private static IntPtr		DisplayHandle;		// X11 handle to display
		private static int		ScreenNo;		// Screen number used
		private static IntPtr		DefaultColormap;	// Colormap for screen
		private static IntPtr		CustomVisual;		// Visual for window creation
		private static IntPtr		CustomColormap;		// Colormap for window creation
		private static IntPtr		RootWindow;		// Handle of the root window for the screen/display
		private static IntPtr		FosterParent;		// Container to hold child windows until their parent exists
		private static XErrorHandler	ErrorHandler;		// Error handler delegate
		private static bool		ErrorExceptions;	// Throw exceptions on X errors

		// Clipboard
		private static IntPtr 		ClipMagic;
		private static ClipboardStruct	Clipboard;		// Our clipboard

		// Communication
		private static IntPtr		PostAtom;		// PostMessage atom
		private static IntPtr		AsyncAtom;		// Support for async messages

		// Message Loop
		private static Hashtable	MessageQueues;		// Holds our thread-specific XEventQueues
		#if __MonoCS__						//
		private static Pollfd[]		pollfds;		// For watching the X11 socket
		#endif							//
		private static X11Keyboard	Keyboard;		//
		private static X11Dnd		Dnd;
		private static Socket		listen;			//
		private static Socket		wake;			//
		private static Socket		wake_receive;		//
		private static byte[]		network_buffer;		//
		private static bool		detectable_key_auto_repeat;

		// Focus tracking
		private static IntPtr		ActiveWindow;		// Handle of the active window
		private static IntPtr		FocusWindow;		// Handle of the window with keyboard focus (if any)

		// Modality support
		private static Stack		ModalWindows;		// Stack of our modal windows

		// Systray
		private static IntPtr		SystrayMgrWindow;	// Handle of the Systray Manager window

		// Cursors
		private static IntPtr		LastCursorWindow;	// The last window we set the cursor on
		private static IntPtr		LastCursorHandle;	// The handle that was last set on LastCursorWindow
		private static IntPtr		OverrideCursorHandle;	// The cursor that is set to override any other cursors

		// Caret
		private static CaretStruct	Caret;			//

		// Our atoms
		private static IntPtr WM_PROTOCOLS;
		private static IntPtr WM_DELETE_WINDOW;
		private static IntPtr WM_TAKE_FOCUS;
		private static IntPtr _NET_SUPPORTED;
		private static IntPtr _NET_CLIENT_LIST;
		private static IntPtr _NET_NUMBER_OF_DESKTOPS;
		private static IntPtr _NET_DESKTOP_GEOMETRY;
		private static IntPtr _NET_DESKTOP_VIEWPORT;
		private static IntPtr _NET_CURRENT_DESKTOP;
		private static IntPtr _NET_DESKTOP_NAMES;
		private static IntPtr _NET_ACTIVE_WINDOW;
		private static IntPtr _NET_WORKAREA;
		private static IntPtr _NET_SUPPORTING_WM_CHECK;
		private static IntPtr _NET_VIRTUAL_ROOTS;
		private static IntPtr _NET_DESKTOP_LAYOUT;
		private static IntPtr _NET_SHOWING_DESKTOP;
		private static IntPtr _NET_CLOSE_WINDOW;
		private static IntPtr _NET_MOVERESIZE_WINDOW;
		private static IntPtr _NET_WM_MOVERESIZE;
		private static IntPtr _NET_RESTACK_WINDOW;
		private static IntPtr _NET_REQUEST_FRAME_EXTENTS;
		private static IntPtr _NET_WM_NAME;
		private static IntPtr _NET_WM_VISIBLE_NAME;
		private static IntPtr _NET_WM_ICON_NAME;
		private static IntPtr _NET_WM_VISIBLE_ICON_NAME;
		private static IntPtr _NET_WM_DESKTOP;
		private static IntPtr _NET_WM_WINDOW_TYPE;
		private static IntPtr _NET_WM_STATE;
		private static IntPtr _NET_WM_ALLOWED_ACTIONS;
		private static IntPtr _NET_WM_STRUT;
		private static IntPtr _NET_WM_STRUT_PARTIAL;
		private static IntPtr _NET_WM_ICON_GEOMETRY;
		private static IntPtr _NET_WM_ICON;
		private static IntPtr _NET_WM_PID;
		private static IntPtr _NET_WM_HANDLED_ICONS;
		private static IntPtr _NET_WM_USER_TIME;
		private static IntPtr _NET_FRAME_EXTENTS;
		private static IntPtr _NET_WM_PING;
		private static IntPtr _NET_WM_SYNC_REQUEST;
		private static IntPtr _NET_SYSTEM_TRAY_S;
		private static IntPtr _NET_SYSTEM_TRAY_ORIENTATION;
		private static IntPtr _NET_SYSTEM_TRAY_OPCODE;
		private static IntPtr _NET_WM_STATE_MAXIMIZED_HORZ;
		private static IntPtr _NET_WM_STATE_MAXIMIZED_VERT;
		private static IntPtr _XEMBED;
		private static IntPtr _XEMBED_INFO;
		private static IntPtr _MOTIF_WM_HINTS;
		private static IntPtr _NET_WM_STATE_NO_TASKBAR;
		private static IntPtr _NET_WM_STATE_ABOVE;
		private static IntPtr _NET_WM_STATE_MODAL;
		private static IntPtr _NET_WM_STATE_HIDDEN;
		private static IntPtr _NET_WM_CONTEXT_HELP;
		private static IntPtr _NET_WM_WINDOW_OPACITY;
		private static IntPtr _NET_WM_WINDOW_TYPE_DESKTOP;
		private static IntPtr _NET_WM_WINDOW_TYPE_DOCK;
		private static IntPtr _NET_WM_WINDOW_TYPE_TOOLBAR;
		private static IntPtr _NET_WM_WINDOW_TYPE_MENU;
		private static IntPtr _NET_WM_WINDOW_TYPE_UTILITY;
		private static IntPtr _NET_WM_WINDOW_TYPE_SPLASH;
		private static IntPtr _NET_WM_WINDOW_TYPE_DIALOG;
		private static IntPtr _NET_WM_WINDOW_TYPE_NORMAL;
		private static IntPtr CLIPBOARD;
		private static IntPtr PRIMARY;
		private static IntPtr DIB;
		private static IntPtr OEMTEXT;
		private static IntPtr UNICODETEXT;
		private static IntPtr TARGETS;

		// mouse hover message generation
		private static HoverStruct	HoverState;		//

		// double click message generation
		private static ClickStruct	ClickPending;		//

		// Support for mouse grab
		private static GrabStruct	Grab;			//

		// State
		private static Point		MousePosition;		// Last position of mouse, in screen coords
		internal static MouseButtons	MouseState;		// Last state of mouse buttons

		// 'Constants'
		private static int		DoubleClickInterval;	// msec; max interval between clicks to count as double click

		const EventMask SelectInputMask = EventMask.ButtonPressMask | 
		                                  EventMask.ButtonReleaseMask | 
		                                  EventMask.KeyPressMask | 
		                                  EventMask.KeyReleaseMask | 
		                                  EventMask.EnterWindowMask | 
		                                  EventMask.LeaveWindowMask |
		                                  EventMask.ExposureMask |
		                                  EventMask.FocusChangeMask |
		                                  EventMask.PointerMotionMask | 
		                                  EventMask.VisibilityChangeMask |
		                                  EventMask.SubstructureNotifyMask |
		                                  EventMask.StructureNotifyMask;

		static readonly object lockobj = new object ();

		#endregion	// Local Variables
		#region Constructors
		private XplatUIX11() {
			// Handle singleton stuff first
			RefCount = 0;

			// Now regular initialization
			XlibLock = new object ();
			MessageQueues = Hashtable.Synchronized (new Hashtable(7));
			XInitThreads();

			ErrorExceptions = false;

			// X11 Initialization
			SetDisplay(XOpenDisplay(IntPtr.Zero));
			X11DesktopColors.Initialize();

			
			// Disable keyboard autorepeat
			try {
				XkbSetDetectableAutoRepeat (DisplayHandle, true,  IntPtr.Zero);
				detectable_key_auto_repeat = true;
			} catch {
				Console.Error.WriteLine ("Could not disable keyboard auto repeat, will attempt to disable manually.");
				detectable_key_auto_repeat = false;
			}

			// Handle any upcoming errors; we re-set it here, X11DesktopColor stuff might have stolen it (gtk does)
			ErrorHandler = new XErrorHandler(HandleError);
			XSetErrorHandler(ErrorHandler);
		}

		~XplatUIX11() {
			// Remove our display handle from S.D
			Graphics.FromHdcInternal (IntPtr.Zero);
		}

		#endregion	// Constructors

		#region Singleton Specific Code
		public static XplatUIX11 GetInstance() {
			lock (lockobj) {
				if (Instance == null) {
					Instance=new XplatUIX11();
				}
				RefCount++;
			}
			return Instance;
		}

		public int Reference {
			get {
				return RefCount;
			}
		}
		#endregion

		#region Internal Properties
		internal static IntPtr Display {
			get {
				return DisplayHandle;
			}

			set {
				XplatUIX11.GetInstance().SetDisplay(value);
			}
		}

		internal static int Screen {
			get {
				return ScreenNo;
			}

			set {
				ScreenNo = value;
			}
		}

		internal static IntPtr RootWindowHandle {
			get {
				return RootWindow;
			}

			set {
				RootWindow = value;
			}
		}

		internal static IntPtr Visual {
			get {
				return CustomVisual;
			}

			set {
				CustomVisual = value;
			}
		}

		internal static IntPtr ColorMap {
			get {
				return CustomColormap;
			}

			set {
				CustomColormap = value;
			}
		}
		#endregion

		#region XExceptionClass
		internal class XException : ApplicationException {
			IntPtr		Display;
			IntPtr		ResourceID;
			IntPtr		Serial;
			XRequest	RequestCode;
			byte		ErrorCode;
			byte		MinorCode;

			public XException(IntPtr Display, IntPtr ResourceID, IntPtr Serial, byte ErrorCode, XRequest RequestCode, byte MinorCode) {
				this.Display = Display;
				this.ResourceID = ResourceID;
				this.Serial = Serial;
				this.RequestCode = RequestCode;
				this.ErrorCode = ErrorCode;
				this.MinorCode = MinorCode;
			}

			public override string Message {
				get {
					return GetMessage(Display, ResourceID, Serial, ErrorCode, RequestCode, MinorCode);
				}
			}

			public static string GetMessage(IntPtr Display, IntPtr ResourceID, IntPtr Serial, byte ErrorCode, XRequest RequestCode, byte MinorCode) {
				StringBuilder	sb;
				string		x_error_text;
				string		error;
				string		hwnd_text;
				string		control_text;
				Hwnd		hwnd;
				Control		c;

				sb = new StringBuilder(160);
				XGetErrorText(Display, ErrorCode, sb, sb.Capacity);
				x_error_text = sb.ToString();
				hwnd = Hwnd.ObjectFromHandle(ResourceID);
				if (hwnd != null) {
					hwnd_text = hwnd.ToString();
					c = Control.FromHandle(hwnd.Handle);
					if (c != null) {
						control_text = c.ToString();
					} else {
						control_text = String.Format("<handle {0:X} non-existant>", hwnd.Handle);
					}
				} else {
					hwnd_text = "<null>";
					control_text = "<null>";
				}


				error = String.Format("\n  Error: {0}\n  Request:     {1:D} ({2})\n  Resource ID: 0x{3:X}\n  Serial:      {4}\n  Hwnd:        {5}\n  Control:     {6}", x_error_text, RequestCode, RequestCode, ResourceID.ToInt32(), Serial, hwnd_text, control_text);
				return error;
			}
		}
		#endregion	// XExceptionClass

		#region Internal Methods
		internal void SetDisplay(IntPtr display_handle) {
			if (display_handle != IntPtr.Zero) {
				Hwnd	hwnd;

				if ((DisplayHandle != IntPtr.Zero) && (FosterParent != IntPtr.Zero)) {
					hwnd = Hwnd.ObjectFromHandle(FosterParent);
					XDestroyWindow(DisplayHandle, FosterParent);
					hwnd.Dispose();
				}

				if (DisplayHandle != IntPtr.Zero) {
					XCloseDisplay(DisplayHandle);
				}

				DisplayHandle=display_handle;

				// We need to tell System.Drawing our DisplayHandle. FromHdcInternal has
				// been hacked to do this for us.
				Graphics.FromHdcInternal (DisplayHandle);

				// Debugging support
				if (Environment.GetEnvironmentVariable ("MONO_XSYNC") != null) {
					XSynchronize(DisplayHandle, true);
				}

				if (Environment.GetEnvironmentVariable ("MONO_XEXCEPTIONS") != null) {
					ErrorExceptions = true;
				}

				// Generic X11 setup
				ScreenNo = XDefaultScreen(DisplayHandle);
				RootWindow = XRootWindow(DisplayHandle, ScreenNo);
				DefaultColormap = XDefaultColormap(DisplayHandle, ScreenNo);

				// Create the foster parent
				FosterParent=XCreateSimpleWindow(DisplayHandle, RootWindow, 0, 0, 1, 1, 4, UIntPtr.Zero, UIntPtr.Zero);
				if (FosterParent==IntPtr.Zero) {
					Console.WriteLine("XplatUIX11 Constructor failed to create FosterParent");
				}

				hwnd = new Hwnd();
				hwnd.Queue = ThreadQueue(Thread.CurrentThread);
				hwnd.WholeWindow = FosterParent;
				hwnd.ClientWindow = FosterParent;

				// Create a HWND for RootWIndow as well, so our queue doesn't eat the events
				hwnd = new Hwnd();
				hwnd.Queue = ThreadQueue(Thread.CurrentThread);
				hwnd.whole_window = RootWindow;
				hwnd.ClientWindow = RootWindow;

				// For sleeping on the X11 socket
				listen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
				IPEndPoint ep = new IPEndPoint(IPAddress.Loopback, 0);
				listen.Bind(ep);
				listen.Listen(1);

				// To wake up when a timer is ready
				network_buffer = new byte[10];

				wake = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
				wake.Connect(listen.LocalEndPoint);
				wake_receive = listen.Accept();

				#if __MonoCS__
				pollfds = new Pollfd [2];
				pollfds [0] = new Pollfd ();
				pollfds [0].fd = XConnectionNumber (DisplayHandle);
				pollfds [0].events = PollEvents.POLLIN;

				pollfds [1] = new Pollfd ();
				pollfds [1].fd = wake_receive.Handle.ToInt32 ();
				pollfds [1].events = PollEvents.POLLIN;
				#endif

				Keyboard = new X11Keyboard(DisplayHandle, FosterParent);
				Dnd = new X11Dnd (DisplayHandle);

				DoubleClickInterval = 500;

				HoverState.Interval = 500;
				HoverState.Timer = new Timer();
				HoverState.Timer.Enabled = false;
				HoverState.Timer.Interval = HoverState.Interval;
				HoverState.Timer.Tick += new EventHandler(MouseHover);
				HoverState.Size = new Size(4, 4);
				HoverState.X = -1;
				HoverState.Y = -1;

				ActiveWindow = IntPtr.Zero;
				FocusWindow = IntPtr.Zero;
				ModalWindows = new Stack(3);

				MouseState = MouseButtons.None;
				MousePosition = new Point(0, 0);

				Caret.Timer = new Timer();
				Caret.Timer.Interval = 500;		// FIXME - where should this number come from?
				Caret.Timer.Tick += new EventHandler(CaretCallback);

				SetupAtoms();

				// Grab atom changes off the root window to catch certain WM events
				XSelectInput(DisplayHandle, RootWindow, new IntPtr ((int)EventMask.PropertyChangeMask));

				// Handle any upcoming errors
				ErrorHandler = new XErrorHandler(HandleError);
				XSetErrorHandler(ErrorHandler);
			} else {
				throw new ArgumentNullException("Display", "Could not open display (X-Server required. Check you DISPLAY environment variable)");
			}
		}

		internal static void Where() {
			Console.WriteLine("Here: {0}\n", WhereString());
		}

		internal static string WhereString() {
			StackTrace	stack;
			StackFrame	frame;
			string		newline;
			string		unknown;
			StringBuilder	sb;
			MethodBase	method;

			newline = String.Format("{0}\t {1} ", Environment.NewLine, Locale.GetText("at"));
			unknown = Locale.GetText("<unknown method>");
			sb = new StringBuilder();
			stack = new StackTrace(true);

			for (int i = 0; i < stack.FrameCount; i++) {
				frame = stack.GetFrame(i);
				sb.Append(newline);

				method = frame.GetMethod();
				if (method != null) {
					#if not
						sb.AppendFormat(frame.ToString());
					#endif
					if (frame.GetFileLineNumber() != 0) {
						sb.AppendFormat("{0}.{1} () [{2}:{3}]", method.DeclaringType.FullName, method.Name, Path.GetFileName(frame.GetFileName()), frame.GetFileLineNumber());
					} else {
						sb.AppendFormat("{0}.{1} ()", method.DeclaringType.FullName, method.Name);
					}
				} else { 
					sb.Append(unknown);
				}
			}
			return sb.ToString();
 		}
		#endregion	// Internal Methods

		#region Private Methods
		private int unixtime() {
			TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));

			return (int) t.TotalSeconds;
		}

		private static void SetupAtoms() {
			WM_PROTOCOLS = XInternAtom(DisplayHandle, "WM_PROTOCOLS", false);
			WM_DELETE_WINDOW = XInternAtom(DisplayHandle, "WM_DELETE_WINDOW", false);
			WM_TAKE_FOCUS = XInternAtom(DisplayHandle, "WM_TAKE_FOCUS", false);

			_NET_SUPPORTED = XInternAtom(DisplayHandle, "_NET_SUPPORTED", false);
			_NET_CLIENT_LIST = XInternAtom(DisplayHandle, "_NET_CLIENT_LIST", false);
			_NET_NUMBER_OF_DESKTOPS = XInternAtom(DisplayHandle, "_NET_NUMBER_OF_DESKTOPS", false);
			_NET_DESKTOP_GEOMETRY = XInternAtom(DisplayHandle, "_NET_DESKTOP_GEOMETRY", false);
			_NET_DESKTOP_VIEWPORT = XInternAtom(DisplayHandle, "_NET_DESKTOP_VIEWPORT", false);
			_NET_CURRENT_DESKTOP = XInternAtom(DisplayHandle, "_NET_CURRENT_DESKTOP", false);
			_NET_DESKTOP_NAMES = XInternAtom(DisplayHandle, "_NET_DESKTOP_NAMES", false);
			_NET_ACTIVE_WINDOW = XInternAtom(DisplayHandle, "_NET_ACTIVE_WINDOW", false);
			_NET_WORKAREA = XInternAtom(DisplayHandle, "_NET_WORKAREA", false);
			_NET_SUPPORTING_WM_CHECK = XInternAtom(DisplayHandle, "_NET_SUPPORTING_WM_CHECK", false);
			_NET_VIRTUAL_ROOTS = XInternAtom(DisplayHandle, "_NET_VIRTUAL_ROOTS", false);
			_NET_DESKTOP_LAYOUT = XInternAtom(DisplayHandle, "_NET_DESKTOP_LAYOUT", false);
			_NET_SHOWING_DESKTOP = XInternAtom(DisplayHandle, "_NET_SHOWING_DESKTOP", false);

			_NET_CLOSE_WINDOW = XInternAtom(DisplayHandle, "_NET_CLOSE_WINDOW", false);
			_NET_MOVERESIZE_WINDOW = XInternAtom(DisplayHandle, "_NET_MOVERESIZE_WINDOW", false);
			_NET_WM_MOVERESIZE = XInternAtom(DisplayHandle, "_NET_WM_MOVERESIZE", false);
			_NET_RESTACK_WINDOW = XInternAtom(DisplayHandle, "_NET_RESTACK_WINDOW", false);
			_NET_REQUEST_FRAME_EXTENTS = XInternAtom(DisplayHandle, "_NET_REQUEST_FRAME_EXTENTS", false);

			_NET_WM_NAME = XInternAtom(DisplayHandle, "_NET_WM_NAME", false);
			_NET_WM_VISIBLE_NAME = XInternAtom(DisplayHandle, "_NET_WM_VISIBLE_NAME", false);
			_NET_WM_ICON_NAME = XInternAtom(DisplayHandle, "_NET_WM_ICON_NAME", false);
			_NET_WM_VISIBLE_ICON_NAME = XInternAtom(DisplayHandle, "_NET_WM_VISIBLE_ICON_NAME", false);
			_NET_WM_DESKTOP = XInternAtom(DisplayHandle, "_NET_WM_DESKTOP", false);
			_NET_WM_WINDOW_TYPE = XInternAtom(DisplayHandle, "_NET_WM_WINDOW_TYPE", false);
			_NET_WM_STATE = XInternAtom(DisplayHandle, "_NET_WM_STATE", false);
			_NET_WM_ALLOWED_ACTIONS = XInternAtom(DisplayHandle, "_NET_WM_ALLOWED_ACTIONS", false);
			_NET_WM_STRUT = XInternAtom(DisplayHandle, "_NET_WM_STRUT", false);
			_NET_WM_STRUT_PARTIAL = XInternAtom(DisplayHandle, "_NET_WM_STRUT_PARTIAL", false);
			_NET_WM_ICON_GEOMETRY = XInternAtom(DisplayHandle, "_NET_WM_ICON_GEOMETRY", false);
			_NET_WM_ICON = XInternAtom(DisplayHandle, "_NET_WM_ICON", false);
			_NET_WM_PID = XInternAtom(DisplayHandle, "_NET_WM_PID", false);
			_NET_WM_HANDLED_ICONS = XInternAtom(DisplayHandle, "_NET_WM_HANDLED_ICONS", false);
			_NET_WM_USER_TIME = XInternAtom(DisplayHandle, "_NET_WM_USER_TIME", false);
			_NET_FRAME_EXTENTS = XInternAtom(DisplayHandle, "_NET_FRAME_EXTENTS", false);

			_NET_WM_PING = XInternAtom(DisplayHandle, "_NET_WM_PING", false);
			_NET_WM_SYNC_REQUEST = XInternAtom(DisplayHandle, "_NET_WM_SYNC_REQUEST", false);

			_NET_SYSTEM_TRAY_S = XInternAtom(DisplayHandle, "_NET_SYSTEM_TRAY_S" + ScreenNo.ToString(), false);
			_NET_SYSTEM_TRAY_OPCODE = XInternAtom(DisplayHandle, "_NET_SYSTEM_TRAY_OPCODE", false);
			_NET_SYSTEM_TRAY_ORIENTATION = XInternAtom(DisplayHandle, "_NET_SYSTEM_TRAY_ORIENTATION", false);

			_NET_WM_STATE_MAXIMIZED_HORZ = XInternAtom(DisplayHandle, "_NET_WM_STATE_MAXIMIZED_HORZ", false);
			_NET_WM_STATE_MAXIMIZED_VERT = XInternAtom(DisplayHandle, "_NET_WM_STATE_MAXIMIZED_VERT", false);
			_NET_WM_STATE_HIDDEN = XInternAtom(DisplayHandle, "_NET_WM_STATE_HIDDEN", false);

			_XEMBED = XInternAtom(DisplayHandle, "_XEMBED", false);
			_XEMBED_INFO = XInternAtom(DisplayHandle, "_XEMBED_INFO", false);

			_MOTIF_WM_HINTS = XInternAtom(DisplayHandle, "_MOTIF_WM_HINTS", false);

			_NET_WM_STATE_NO_TASKBAR = XInternAtom(DisplayHandle, "_NET_WM_STATE_NO_TASKBAR", false);
			_NET_WM_STATE_ABOVE = XInternAtom(DisplayHandle, "_NET_WM_STATE_ABOVE", false);
			_NET_WM_STATE_MODAL = XInternAtom(DisplayHandle, "_NET_WM_STATE_MODAL", false);
			_NET_WM_CONTEXT_HELP = XInternAtom(DisplayHandle, "_NET_WM_CONTEXT_HELP", false);
			_NET_WM_WINDOW_OPACITY = XInternAtom(DisplayHandle, "_NET_WM_WINDOW_OPACITY", false);

			_NET_WM_WINDOW_TYPE_DESKTOP = XInternAtom(DisplayHandle, "_NET_WM_WINDOW_TYPE_DESKTOP", false);
			_NET_WM_WINDOW_TYPE_DOCK = XInternAtom(DisplayHandle, "_NET_WM_WINDOW_TYPE_DOCK", false);
			_NET_WM_WINDOW_TYPE_TOOLBAR = XInternAtom(DisplayHandle, "_NET_WM_WINDOW_TYPE_TOOLBAR", false);
			_NET_WM_WINDOW_TYPE_MENU = XInternAtom(DisplayHandle, "_NET_WM_WINDOW_TYPE_MENU", false);
			_NET_WM_WINDOW_TYPE_UTILITY = XInternAtom(DisplayHandle, "_NET_WM_WINDOW_TYPE_UTILITY", false);
			_NET_WM_WINDOW_TYPE_DIALOG = XInternAtom(DisplayHandle, "_NET_WM_WINDOW_TYPE_DIALOG", false);
			_NET_WM_WINDOW_TYPE_SPLASH = XInternAtom(DisplayHandle, "_NET_WM_WINDOW_TYPE_SPLASH", false);
			_NET_WM_WINDOW_TYPE_NORMAL = XInternAtom(DisplayHandle, "_NET_WM_WINDOW_TYPE_NORMAL", false);

			// Clipboard support
			CLIPBOARD = XInternAtom (DisplayHandle, "CLIPBOARD", false);
			PRIMARY = XInternAtom (DisplayHandle, "PRIMARY", false);
			DIB = (IntPtr)Atom.XA_PIXMAP;
			OEMTEXT = XInternAtom(DisplayHandle, "COMPOUND_TEXT", false);
			UNICODETEXT = XInternAtom(DisplayHandle, "UTF8_STRING", false);
			TARGETS = XInternAtom(DisplayHandle, "TARGETS", false);

			// Special Atoms
			AsyncAtom = XInternAtom(DisplayHandle, "_SWF_AsyncAtom", false);
			PostAtom = XInternAtom (DisplayHandle, "_SWF_PostMessageAtom", false);
			HoverState.Atom = XInternAtom(DisplayHandle, "_SWF_HoverAtom", false);
		}

		private void GetSystrayManagerWindow() {
			XGrabServer(DisplayHandle);
			SystrayMgrWindow = XGetSelectionOwner(DisplayHandle, _NET_SYSTEM_TRAY_S);
			XUngrabServer(DisplayHandle);
			XFlush(DisplayHandle);
		}

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
			XSendEvent(DisplayHandle, RootWindow, false, new IntPtr ((int) (EventMask.SubstructureRedirectMask | EventMask.SubstructureNotifyMask)), ref xev);
		}

		private void SendNetClientMessage(IntPtr window, IntPtr message_type, IntPtr l0, IntPtr l1, IntPtr l2) {
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
			XSendEvent(DisplayHandle, window, false, new IntPtr ((int)EventMask.NoEventMask), ref xev);
		}

		bool StyleSet (int s, WindowStyles ws)
		{
			return (s & (int)ws) != 0;
		}

		bool ExStyleSet (int ex, WindowExStyles exws)
		{
			return (ex & (int)exws) != 0;
		}

		private void DeriveStyles(int Style, int ExStyle, out FormBorderStyle border_style, out TitleStyle title_style, out int caption_height, out int tool_caption_height) {

			// Only MDI windows get caption_heights
			caption_height = 0;
			tool_caption_height = 19;

			if (StyleSet (Style, WindowStyles.WS_CHILD)) {
				if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_CLIENTEDGE)) {
					border_style = FormBorderStyle.Fixed3D;
				} else if (!StyleSet (Style, WindowStyles.WS_BORDER)) {
					border_style = FormBorderStyle.None;
				} else {
					border_style = FormBorderStyle.FixedSingle;
				}
				title_style = TitleStyle.None;

				if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_MDICHILD)) {
					caption_height = 26;

					if (StyleSet (Style, WindowStyles.WS_CAPTION)) {
						if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
							title_style = TitleStyle.Tool;
						} else {
							title_style = TitleStyle.Normal;
						}
					}

					if (StyleSet (Style, WindowStyles.WS_OVERLAPPEDWINDOW) ||
					    ExStyleSet (ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
						border_style = (FormBorderStyle) 0xFFFF;
					} else {
						border_style = FormBorderStyle.None;
					}
				}

			} else {
				title_style = TitleStyle.None;
				if (StyleSet (Style, WindowStyles.WS_CAPTION)) {
					if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
						title_style = TitleStyle.Tool;
					} else {
						title_style = TitleStyle.Normal;
					}
				}

				border_style = FormBorderStyle.None;

				if (StyleSet (Style, WindowStyles.WS_THICKFRAME)) {
					if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
						border_style = FormBorderStyle.SizableToolWindow;
					} else {
						border_style = FormBorderStyle.Sizable;
					}
				} else {
					if (StyleSet (Style, WindowStyles.WS_CAPTION)) {
						if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_CLIENTEDGE)) {
							border_style = FormBorderStyle.Fixed3D;
						} else if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_DLGMODALFRAME)) {
							border_style = FormBorderStyle.FixedDialog;
						} else if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
							border_style = FormBorderStyle.FixedToolWindow;
						} else if (StyleSet (Style, WindowStyles.WS_BORDER)) {
							border_style = FormBorderStyle.FixedSingle;
						}
					} else {
						if (StyleSet (Style, WindowStyles.WS_BORDER)) {
							border_style = FormBorderStyle.FixedSingle;
						}
					}
				}
			}
		}

		private void SetHwndStyles(Hwnd hwnd, CreateParams cp) {
			DeriveStyles(cp.Style, cp.ExStyle, out hwnd.border_style, out hwnd.title_style, out hwnd.caption_height, out hwnd.tool_caption_height);
		}

		private void SetWMStyles(Hwnd hwnd, CreateParams cp) {
			MotifWmHints		mwmHints;
			MotifFunctions		functions;
			MotifDecorations	decorations;
			int[]			atoms;
			int			atom_count;
			Rectangle		client_rect;

			// Child windows don't need WM window styles
			if (StyleSet (cp.Style, WindowStyles.WS_CHILDWINDOW)) {
				return;
			}

			atoms = new int[8];
			mwmHints = new MotifWmHints();
			functions = 0;
			decorations = 0;

			mwmHints.flags = (IntPtr)(MotifFlags.Functions | MotifFlags.Decorations);
			mwmHints.functions = (IntPtr)0;
			mwmHints.decorations = (IntPtr)0;

			if (ExStyleSet (cp.ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)
			    || !StyleSet (cp.Style, WindowStyles.WS_CAPTION | WindowStyles.WS_BORDER | WindowStyles.WS_DLGFRAME)) {
				/* tool windows get no window manager
				   decorations, and neither do windows
				   which lack CAPTION/BORDER/DLGFRAME
				   styles.
				*/
			}
			else {
				if (StyleSet (cp.Style, WindowStyles.WS_CAPTION)) {
					functions |= MotifFunctions.Move;
					decorations |= MotifDecorations.Title | MotifDecorations.Menu;
				}

				if (StyleSet (cp.Style, WindowStyles.WS_THICKFRAME)) {
					functions |= MotifFunctions.Move | MotifFunctions.Resize;
					decorations |= MotifDecorations.Border | MotifDecorations.ResizeH;
				}

				if (StyleSet (cp.Style, WindowStyles.WS_MINIMIZEBOX)) {
					functions |= MotifFunctions.Minimize;
					decorations |= MotifDecorations.Minimize;
				}

				if (StyleSet (cp.Style, WindowStyles.WS_MAXIMIZEBOX)) {
					functions |= MotifFunctions.Maximize;
					decorations |= MotifDecorations.Maximize;
				}

				if (StyleSet (cp.Style, WindowStyles.WS_SIZEBOX)) {
					functions |= MotifFunctions.Resize;
					decorations |= MotifDecorations.ResizeH;
				}

				if (ExStyleSet (cp.ExStyle, WindowExStyles.WS_EX_DLGMODALFRAME)) {
					decorations |= MotifDecorations.Border;
				}

				if (StyleSet (cp.Style, WindowStyles.WS_BORDER)) {
					decorations |= MotifDecorations.Border;
				}
			
				if (StyleSet (cp.Style, WindowStyles.WS_DLGFRAME)) {
					decorations |= MotifDecorations.Border;
				}

				if (StyleSet (cp.Style, WindowStyles.WS_SYSMENU)) {
					functions |= MotifFunctions.Close;
				}
				else {
					functions &= ~(MotifFunctions.Maximize | MotifFunctions.Minimize | MotifFunctions.Close);
					decorations &= ~(MotifDecorations.Menu | MotifDecorations.Maximize | MotifDecorations.Minimize);
					if (cp.Caption == "") {
						functions &= ~MotifFunctions.Move;
						decorations &= ~(MotifDecorations.Title | MotifDecorations.ResizeH);
					}
				}
			}

			if ((functions & MotifFunctions.Resize) == 0) {
				hwnd.fixed_size = true;
				XplatUI.SetWindowMinMax(hwnd.Handle, new Rectangle(cp.X, cp.Y, cp.Width, cp.Height), new Size(cp.Width, cp.Height), new Size(cp.Width, cp.Height));
			} else {
				hwnd.fixed_size = false;
			}

			mwmHints.functions = (IntPtr)functions;
			mwmHints.decorations = (IntPtr)decorations;

			client_rect = hwnd.ClientRect;
			lock (XlibLock) {
				// needed! map toolwindows to _NET_WM_WINDOW_TYPE_UTILITY to make newer metacity versions happy
				// and get those windows in front of their parents
				if (ExStyleSet (cp.ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
					atoms [0] = _NET_WM_WINDOW_TYPE_UTILITY.ToInt32 ();
					XChangeProperty (DisplayHandle, hwnd.whole_window,  _NET_WM_WINDOW_TYPE,
							 (IntPtr)Atom.XA_ATOM, 32, PropertyMode.Replace, atoms, 1);
				}
				
				XChangeProperty(DisplayHandle, hwnd.whole_window, _MOTIF_WM_HINTS, _MOTIF_WM_HINTS, 32, PropertyMode.Replace, ref mwmHints, 5);
				if (StyleSet (cp.Style, WindowStyles.WS_POPUP) && (hwnd.parent != null) && (hwnd.parent.whole_window != IntPtr.Zero)) {
					atoms[0] = _NET_WM_WINDOW_TYPE_NORMAL.ToInt32();
					XChangeProperty(DisplayHandle, hwnd.whole_window, _NET_WM_WINDOW_TYPE, (IntPtr)Atom.XA_ATOM, 32, PropertyMode.Replace, atoms, 1);

					XSetTransientForHint(DisplayHandle, hwnd.whole_window, hwnd.parent.whole_window);
				} else if (!ExStyleSet (cp.ExStyle, WindowExStyles.WS_EX_APPWINDOW)) {
//					XSetTransientForHint(DisplayHandle, hwnd.whole_window, FosterParent);
				}
				if ((client_rect.Width < 1) || (client_rect.Height < 1)) {
					XMoveResizeWindow(DisplayHandle, hwnd.client_window, -5, -5, 1, 1);
				} else {
					XMoveResizeWindow(DisplayHandle, hwnd.client_window, client_rect.X, client_rect.Y, client_rect.Width, client_rect.Height);
				}

				atom_count = 0;

				if (ExStyleSet (cp.ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
					atoms[atom_count++] = _NET_WM_STATE_NO_TASKBAR.ToInt32();
				}
				XChangeProperty(DisplayHandle, hwnd.whole_window, _NET_WM_STATE, (IntPtr)Atom.XA_ATOM, 32, PropertyMode.Replace, atoms, atom_count);

				atom_count = 0;
				IntPtr[] atom_ptrs = new IntPtr[2];
				atom_ptrs[atom_count++] = WM_DELETE_WINDOW;
				if (ExStyleSet (cp.ExStyle, WindowExStyles.WS_EX_CONTEXTHELP)) {
					atom_ptrs[atom_count++] = _NET_WM_CONTEXT_HELP;
				}

				XSetWMProtocols(DisplayHandle, hwnd.whole_window, atom_ptrs, atom_count);
			}
		}

		private void SetIcon(Hwnd hwnd, Icon icon) {
			Bitmap		bitmap;
			int		size;
			IntPtr[]	data;
			int		index;

			bitmap = icon.ToBitmap();
			index = 0;
			size = bitmap.Width * bitmap.Height + 2;
			data = new IntPtr[size];

			data[index++] = (IntPtr)bitmap.Width;
			data[index++] = (IntPtr)bitmap.Height;

			for (int y = 0; y < bitmap.Height; y++) {
				for (int x = 0; x < bitmap.Width; x++) {
					data[index++] = (IntPtr)bitmap.GetPixel(x, y).ToArgb();
				}
			}

			XChangeProperty(DisplayHandle, hwnd.whole_window, _NET_WM_ICON, (IntPtr)Atom.XA_CARDINAL, 32, PropertyMode.Replace, data, size);
		}

		private IntPtr ImageToPixmap(Image image) {
			return IntPtr.Zero;
		}

		private void WakeupMain () {
			wake.Send (new byte [] { 0xFF });
		}

		private XEventQueue ThreadQueue(Thread thread) {
			XEventQueue	queue;

			queue = (XEventQueue)MessageQueues[thread];
			if (queue == null) {
				queue = new XEventQueue(thread);
				MessageQueues[thread] = queue;
			}

			return queue;
		}

		private void TranslatePropertyToClipboard(IntPtr property) {
			IntPtr			actual_atom;
			int			actual_format;
			IntPtr			nitems;
			IntPtr			bytes_after;
			IntPtr			prop = IntPtr.Zero;

			Clipboard.Item = null;

			XGetWindowProperty(DisplayHandle, FosterParent, property, IntPtr.Zero, new IntPtr (0x7fffffff), true, (IntPtr)Atom.AnyPropertyType, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);

			if ((long)nitems > 0) {
				if (property == (IntPtr)Atom.XA_STRING) {
					Clipboard.Item = Marshal.PtrToStringAnsi(prop);
				} else if (property == (IntPtr)Atom.XA_BITMAP) {
					// FIXME - convert bitmap to image
				} else if (property == (IntPtr)Atom.XA_PIXMAP) {
					// FIXME - convert pixmap to image
				} else if (property == OEMTEXT) {
					Clipboard.Item = Marshal.PtrToStringAnsi(prop);
				} else if (property == UNICODETEXT) {
					Clipboard.Item = Marshal.PtrToStringAnsi(prop);
				}

				XFree(prop);
			}
		}

		private void AddExpose (Hwnd hwnd, bool client, int x, int y, int width, int height) {
			// Don't waste time
			if ((hwnd == null) || (x > hwnd.Width) || (y > hwnd.Height) || ((x + width) < 0) || ((y + height) < 0)) {
				return;
			}

			// Keep the invalid area as small as needed
			if ((x + width) > hwnd.width) {
				width = hwnd.width - x;
			}

			if ((y + height) > hwnd.height) {
				height = hwnd.height - y;
			}

			if (client) {
				hwnd.AddInvalidArea(x, y, width, height);
				if (!hwnd.expose_pending) {
					if (!hwnd.nc_expose_pending) {
						hwnd.Queue.Paint.Enqueue(hwnd);
					}
					hwnd.expose_pending = true;
				}
			} else {
				hwnd.AddNcInvalidArea (x, y, width, height);
				
				if (!hwnd.nc_expose_pending) {
					if (!hwnd.expose_pending) {
						hwnd.Queue.Paint.Enqueue(hwnd);
					}
					hwnd.nc_expose_pending = true;
				}
			}
		}

		private void InvalidateWholeWindow(IntPtr handle) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			InvalidateWholeWindow(handle, new Rectangle(0, 0, hwnd.Width, hwnd.Height));
		}

		private void InvalidateWholeWindow(IntPtr handle, Rectangle rectangle) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			AddExpose (hwnd, false, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
		}

		private void WholeToScreen(IntPtr handle, ref int x, ref int y) {
			int	dest_x_return;
			int	dest_y_return;
			IntPtr	child;
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			lock (XlibLock) {
				XTranslateCoordinates(DisplayHandle, hwnd.whole_window, RootWindow, x, y, out dest_x_return, out dest_y_return, out child);
			}

			x = dest_x_return;
			y = dest_y_return;
		}

		private void AbsoluteGeometry(IntPtr window, out int ret_x, out int ret_y, out int width, out int height) {
			IntPtr	root;
			IntPtr	win;
			IntPtr	parent;
			IntPtr	children;
			int	x;
			int	y;
			int	w;
			int	h;
			int	absX;
			int	absY;
			int	b;
			int	d;
			int	nchildren;

			absX = 0;
			absY = 0;
			win = window;
			width = 0;
			height = 0;
			do {
				XGetGeometry(DisplayHandle, win, out root, out x, out y, out w, out h, out b, out d);
				if (win == window) {
					width = w;
					height = h;
				}
				absX += x;
				absY += y;
				if (XQueryTree(DisplayHandle, win, out root, out parent,  out children, out nchildren) == 0) {
					break;
				}

				if (children != IntPtr.Zero) {
					XFree(children);
				}
				win = parent;
			} while (win != root);

			ret_x = absX;
			ret_y = absY;

//Console.WriteLine("Absolute pos for window {0} = {1},{2} {3}x{4}", XplatUI.Window(window), ret_x, ret_y, width, height);
		}

		private void FrameExtents(IntPtr window, out int left, out int top) {
			IntPtr			actual_atom;
			int			actual_format;
			IntPtr			nitems;
			IntPtr			bytes_after;
			IntPtr			prop = IntPtr.Zero;

			XGetWindowProperty(DisplayHandle, window, _NET_FRAME_EXTENTS, IntPtr.Zero, new IntPtr (16), false, (IntPtr)Atom.XA_CARDINAL, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);
			if (((long)nitems == 4) && (prop != IntPtr.Zero)) {
				left = Marshal.ReadIntPtr(prop, 0).ToInt32();
				//right = Marshal.ReadIntPtr(prop, IntPtr.Size).ToInt32();
				top = Marshal.ReadIntPtr(prop, IntPtr.Size * 2).ToInt32();
				//bottom = Marshal.ReadIntPtr(prop, IntPtr.Size * 3).ToInt32();
			} else {
				left = 0;
				top = 0;
			}

			if (prop != IntPtr.Zero) {
				XFree(prop);
			}
			return;
		}

		private void AddConfigureNotify (XEvent xevent) {
			Hwnd	hwnd;

			hwnd = Hwnd.GetObjectFromWindow(xevent.ConfigureEvent.window);

			// Don't waste time
			if (hwnd == null) {
				return;
			}

			if ((xevent.ConfigureEvent.window == hwnd.whole_window) && (xevent.ConfigureEvent.window == xevent.ConfigureEvent.xevent)) {
				if (!hwnd.reparented) {
					hwnd.x = xevent.ConfigureEvent.x;
					hwnd.y = xevent.ConfigureEvent.y;
				} else {
					// This sucks ass, part 1
					// Every WM does the ConfigureEvents of toplevel windows different, so there's
					// no standard way of getting our adjustment. 
					// The code below is needed for KDE and FVWM, the 'whacky_wm' part is for metacity
					// Several other WMs do their decorations different yet again and we fail to deal 
					// with that, since I couldn't find any frigging commonality between them.
					// The only sane WM seems to be KDE

					if (!xevent.ConfigureEvent.send_event) {
						IntPtr	dummy_ptr;

						XTranslateCoordinates(DisplayHandle, hwnd.whole_window, RootWindow, -xevent.ConfigureEvent.x, -xevent.ConfigureEvent.y, out hwnd.x, out hwnd.y, out dummy_ptr);
					} else {
						// This is a synthetic event, coordinates are in root space
						hwnd.x = xevent.ConfigureEvent.x;
						hwnd.y = xevent.ConfigureEvent.y;
						if (hwnd.whacky_wm) {
							int frame_left;
							int frame_top;

							FrameExtents(hwnd.whole_window, out frame_left, out frame_top);
							hwnd.x -= frame_left;
							hwnd.y -= frame_top;
						}
					}
				}
				hwnd.width = xevent.ConfigureEvent.width;
				hwnd.height = xevent.ConfigureEvent.height;
				hwnd.ClientRect = Rectangle.Empty;

				if (!hwnd.configure_pending) {
					hwnd.Queue.Enqueue(xevent);
					hwnd.configure_pending = true;
				}
			}
			// We drop configure events for Client windows
		}

		private void ShowCaret() {
			if ((Caret.gc == IntPtr.Zero) || Caret.On) {
				return;
			}
			Caret.On = true;

			lock (XlibLock) {
				XDrawLine(DisplayHandle, Caret.Window, Caret.gc, Caret.X, Caret.Y, Caret.X, Caret.Y + Caret.Height);
			}
		}

		private void HideCaret() {
			if ((Caret.gc == IntPtr.Zero) || !Caret.On) {
				return;
			}
			Caret.On = false;

			lock (XlibLock) {
				XDrawLine(DisplayHandle, Caret.Window, Caret.gc, Caret.X, Caret.Y, Caret.X, Caret.Y + Caret.Height);
			}
		}

		private int NextTimeout (ArrayList timers, DateTime now) {
			int timeout = Int32.MaxValue; 

			foreach (Timer timer in timers) {
				int next = (int) (timer.Expires - now).TotalMilliseconds;
				if (next < 0) {
					return 0; // Have a timer that has already expired
				}

				if (next < timeout) {
					timeout = next;
				}
			}
			if (timeout < Timer.Minimum) {
				timeout = Timer.Minimum;
			}

			if (timeout > 1000)
				timeout = 1000;
			return timeout;
		}

		private void CheckTimers (ArrayList timers, DateTime now) {
			int count;

			count = timers.Count;

			if (count == 0)
				return;

			for (int i = 0; i < timers.Count; i++) {
				Timer timer;

				timer = (Timer) timers [i];

				if (timer.Enabled && timer.Expires <= now) {
					timer.Update (now);
					timer.FireTick ();
				}
			}
		}

		private void MapWindow(Hwnd hwnd, WindowType windows) {
			hwnd.mapped = true;
			if ((windows & WindowType.Whole) != 0) {
				XMapWindow(DisplayHandle, hwnd.whole_window);
			}
			if ((windows & WindowType.Client) != 0) {
				XMapWindow(DisplayHandle, hwnd.client_window);
			}
		}

		private void UnmapWindow(Hwnd hwnd, WindowType windows) {
			hwnd.mapped = false;
			if ((windows & WindowType.Whole) != 0) {
				XUnmapWindow(DisplayHandle, hwnd.whole_window);
			}
			if ((windows & WindowType.Client) != 0) {
				XUnmapWindow(DisplayHandle, hwnd.client_window);
			}
		}

		private void UpdateMessageQueue (XEventQueue queue) {
			DateTime	now;
			int		pending;
			Hwnd		hwnd;

			now = DateTime.UtcNow;

			lock (XlibLock) {
				pending = XPending (DisplayHandle);
			}

			if (pending == 0) {
				if ((queue == null || queue.DispatchIdle) && Idle != null) {
					Idle (this, EventArgs.Empty);
				}

				lock (XlibLock) {
					pending = XPending (DisplayHandle);
				}
			}

			if (pending == 0) {
				int	timeout = 0;

				if (queue != null) {
					if (queue.Paint.Count > 0)
						return;

					timeout = NextTimeout (queue.timer_list, now);
				}

				if (timeout > 0) {
					#if __MonoCS__
					Syscall.poll (pollfds, (uint) pollfds.Length, timeout);
					// Clean out buffer, so we're not busy-looping on the same data
					if (pollfds[1].revents != 0) {
						wake_receive.Receive(network_buffer, 0, 1, SocketFlags.None);
					}
					#endif
					lock (XlibLock) {
						pending = XPending (DisplayHandle);
					}
				}
			}

			if (queue != null)
				CheckTimers (queue.timer_list, now);

			while (true) {
				XEvent xevent = new XEvent ();

				lock (XlibLock) {
					if (XPending (DisplayHandle) == 0)
						break;

					XNextEvent (DisplayHandle, ref xevent);

					if (xevent.AnyEvent.type == XEventName.KeyPress) {
						if (XFilterEvent(ref xevent, FosterParent)) {
							continue;
						}
					}
				}

				hwnd = Hwnd.GetObjectFromWindow(xevent.AnyEvent.window);
				if (hwnd == null) {
					if (xevent.type == XEventName.Expose) {
					}
					continue;
				}

				switch (xevent.type) {
					case XEventName.Expose:
						AddExpose (hwnd, xevent.ExposeEvent.window == hwnd.ClientWindow, xevent.ExposeEvent.x, xevent.ExposeEvent.y, xevent.ExposeEvent.width, xevent.ExposeEvent.height);
						break;

					case XEventName.SelectionClear: {
						// Should we do something?
						break;
					}

					case XEventName.SelectionRequest: {
						if (Dnd.HandleSelectionRequestEvent (ref xevent))
							break;
						XEvent sel_event;

						sel_event = new XEvent();
						sel_event.SelectionEvent.type = XEventName.SelectionNotify;
						sel_event.SelectionEvent.send_event = true;
						sel_event.SelectionEvent.display = DisplayHandle;
						sel_event.SelectionEvent.selection = xevent.SelectionRequestEvent.selection;
						sel_event.SelectionEvent.target = xevent.SelectionRequestEvent.target;
						sel_event.SelectionEvent.requestor = xevent.SelectionRequestEvent.requestor;
						sel_event.SelectionEvent.time = xevent.SelectionRequestEvent.time;
						sel_event.SelectionEvent.property = IntPtr.Zero;

						// Seems that some apps support asking for supported types
						if (xevent.SelectionEvent.target == TARGETS) {
							int[]	atoms;
							int	atom_count;

							atoms = new int[5];
							atom_count = 0;

							if (Clipboard.Item is String) {
								atoms[atom_count++] = (int)Atom.XA_STRING;
								atoms[atom_count++] = (int)OEMTEXT;
								atoms[atom_count++] = (int)UNICODETEXT;
							} else if (Clipboard.Item is Image) {
								atoms[atom_count++] = (int)Atom.XA_PIXMAP;
								atoms[atom_count++] = (int)Atom.XA_BITMAP;
							} else {
								// FIXME - handle other types
							}

							XChangeProperty(DisplayHandle, xevent.SelectionEvent.requestor, (IntPtr)xevent.SelectionRequestEvent.property, (IntPtr)xevent.SelectionRequestEvent.target, 32, PropertyMode.Replace, atoms, atom_count);
						} else if (Clipboard.Item is string) {
							IntPtr	buffer;
							int	buflen;

							buflen = 0;

							if (xevent.SelectionRequestEvent.target == (IntPtr)Atom.XA_STRING) {
								Byte[] bytes;

								bytes = new ASCIIEncoding().GetBytes((string)Clipboard.Item);
								buffer = Marshal.AllocHGlobal(bytes.Length);
								buflen = bytes.Length;

								for (int i = 0; i < buflen; i++) {
									Marshal.WriteByte(buffer, i, bytes[i]);
								}
							} else if (xevent.SelectionRequestEvent.target == OEMTEXT) {
								// FIXME - this should encode into ISO2022
								buffer = Marshal.StringToHGlobalAnsi((string)Clipboard.Item);
								while (Marshal.ReadByte(buffer, buflen) != 0) {
									buflen++;
								}
							} else if (xevent.SelectionRequestEvent.target == UNICODETEXT) {
								buffer = Marshal.StringToHGlobalAnsi((string)Clipboard.Item);
								while (Marshal.ReadByte(buffer, buflen) != 0) {
									buflen++;
								}
							} else {
								buffer = IntPtr.Zero;
							}

							if (buffer != IntPtr.Zero) {
								XChangeProperty(DisplayHandle, xevent.SelectionRequestEvent.requestor, (IntPtr)xevent.SelectionRequestEvent.property, (IntPtr)xevent.SelectionRequestEvent.target, 8, PropertyMode.Replace, buffer, buflen);
								sel_event.SelectionEvent.property = xevent.SelectionRequestEvent.property;
								Marshal.FreeHGlobal(buffer);
							}
						} else if (Clipboard.Item is Image) {
							if (xevent.SelectionEvent.target == (IntPtr)Atom.XA_PIXMAP) {
								// FIXME - convert image and store as property
							} else if (xevent.SelectionEvent.target == (IntPtr)Atom.XA_PIXMAP) {
								// FIXME - convert image and store as property
							}
						}

						XSendEvent(DisplayHandle, xevent.SelectionRequestEvent.requestor, false, new IntPtr ((int)EventMask.NoEventMask), ref sel_event);
						break;
					}

					case XEventName.SelectionNotify: {
						if (Clipboard.Enumerating) {
							Clipboard.Enumerating = false;
							if (xevent.SelectionEvent.property != IntPtr.Zero) {
								XDeleteProperty(DisplayHandle, FosterParent, (IntPtr)xevent.SelectionEvent.property);
								if (!Clipboard.Formats.Contains(xevent.SelectionEvent.property)) {
									Clipboard.Formats.Add(xevent.SelectionEvent.property);
									#if DriverDebugExtra
										Console.WriteLine("Got supported clipboard atom format: {0}", xevent.SelectionEvent.property);
									#endif
								}
							}
						} else if (Clipboard.Retrieving) {
							Clipboard.Retrieving = false;
							if (xevent.SelectionEvent.property != IntPtr.Zero) {
								TranslatePropertyToClipboard(xevent.SelectionEvent.property);
							} else {
								Clipboard.Item = null;
							}
						} else {
							Dnd.HandleSelectionNotifyEvent (ref xevent);
						}
						break;
					}

					case XEventName.MapNotify: {
						if (hwnd.client_window == xevent.MapEvent.window) {
							hwnd.mapped = true;
						}
						break;
					}

					case XEventName.UnmapNotify: {
						if (hwnd.client_window == xevent.MapEvent.window) {
							hwnd.mapped = false;
						}
						break;
					}

					case XEventName.KeyRelease:
						if (!detectable_key_auto_repeat && XPending (DisplayHandle) != 0) {
							XEvent nextevent = new XEvent ();

							XPeekEvent (DisplayHandle, ref nextevent);

							if (nextevent.type == XEventName.KeyPress &&
							nextevent.KeyEvent.keycode == xevent.KeyEvent.keycode &&
							nextevent.KeyEvent.time == xevent.KeyEvent.time) {
								continue;
							}
						}
						goto case XEventName.KeyPress;
					
					case XEventName.MotionNotify: {
						XEvent peek;

						if (hwnd.Queue.Count > 0) {
							peek = hwnd.Queue.Peek();
							if (peek.AnyEvent.type == XEventName.MotionNotify) {
								continue;
							}
						}
						goto case XEventName.KeyPress;
					}

					case XEventName.KeyPress:
					case XEventName.ButtonPress:
					case XEventName.ButtonRelease:
					case XEventName.EnterNotify:
					case XEventName.LeaveNotify:
					case XEventName.CreateNotify:
					case XEventName.DestroyNotify:
					case XEventName.FocusIn:
					case XEventName.FocusOut:
					case XEventName.ClientMessage:
					case XEventName.ReparentNotify:
						hwnd.Queue.Enqueue (xevent);
						break;

					case XEventName.ConfigureNotify:
						AddConfigureNotify(xevent);
						break;

					case XEventName.PropertyNotify:
						if (xevent.PropertyEvent.atom == _NET_ACTIVE_WINDOW) {
							IntPtr	actual_atom;
							int	actual_format;
							IntPtr	nitems;
							IntPtr	bytes_after;
							IntPtr	prop = IntPtr.Zero;
							IntPtr	prev_active;;

							prev_active = ActiveWindow;
							XGetWindowProperty(DisplayHandle, RootWindow, _NET_ACTIVE_WINDOW, IntPtr.Zero, new IntPtr (1), false, (IntPtr)Atom.XA_WINDOW, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);
							if (((long)nitems > 0) && (prop != IntPtr.Zero)) {
								ActiveWindow = Hwnd.GetHandleFromWindow((IntPtr)Marshal.ReadInt32(prop));
								XFree(prop);

								if (prev_active != ActiveWindow) {
									if (prev_active != IntPtr.Zero) {
										PostMessage(prev_active, Msg.WM_ACTIVATE, (IntPtr)WindowActiveFlags.WA_INACTIVE, IntPtr.Zero);
									}
									if (ActiveWindow != IntPtr.Zero) {
										PostMessage(ActiveWindow, Msg.WM_ACTIVATE, (IntPtr)WindowActiveFlags.WA_ACTIVE, IntPtr.Zero);
									}
								}
								if (ModalWindows.Count == 0) {
									break;
								} else {
									// Modality handling, if we are modal and the new active window is one
									// of ours but not the modal one, switch back to the modal window

									if (NativeWindow.FindWindow(ActiveWindow) != null) {
										if (ActiveWindow != (IntPtr)ModalWindows.Peek()) {
											Activate((IntPtr)ModalWindows.Peek());
										}
									}
									break;
								}
							}
						}
						break;

				}
			}
		}

		private IntPtr GetMousewParam(int Delta) {
			int	result = 0;

			if ((MouseState & MouseButtons.Left) != 0) {
				result |= (int)MsgButtons.MK_LBUTTON;
			}

			if ((MouseState & MouseButtons.Middle) != 0) {
				result |= (int)MsgButtons.MK_MBUTTON;
			}

			if ((MouseState & MouseButtons.Right) != 0) {
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
		private IntPtr XGetParent(IntPtr handle) {
			IntPtr	Root;
			IntPtr	Parent;
			IntPtr	Children;
			int	ChildCount;

			lock (XlibLock) {
				XQueryTree(DisplayHandle, handle, out Root, out Parent, out Children, out ChildCount);
			}

			if (Children!=IntPtr.Zero) {
				lock (XlibLock) {
					XFree(Children);
				}
			}
			return Parent;
		}

		private int HandleError(IntPtr display, ref XErrorEvent error_event) {
			if (ErrorExceptions) {
				throw new XException(error_event.display, error_event.resourceid, error_event.serial, error_event.error_code, error_event.request_code, error_event.minor_code);
			} else {
				Console.WriteLine("X11 Error encountered: {0}{1}\n", XException.GetMessage(error_event.display, error_event.resourceid, error_event.serial, error_event.error_code, error_event.request_code, error_event.minor_code), WhereString());
			}
			return 0;
		}

		private void SendWMDestroyMessages(Control c) {
			Hwnd		hwnd;
			int		i;
			Control[]	controls;

			if (c != null) {
				controls = c.child_controls.GetAllControls ();

				if (c.IsHandleCreated && !c.IsDisposed) {
					#if DriverDebugDestroy
						Console.WriteLine("Destroying {0}, child of {1}", XplatUI.Window(c.Handle), (c.Parent != null) ? XplatUI.Window(c.Parent.Handle) : "<none>");
					#endif

					hwnd = Hwnd.ObjectFromHandle(c.Handle);
					CleanupCachedWindows (hwnd);
					SendMessage(c.Handle, Msg.WM_DESTROY, IntPtr.Zero, IntPtr.Zero);
				}

				for (i = 0; i < controls.Length; i++) {
					if (controls[i].IsHandleCreated) {
						/* set all the children hwnd's to zombies so all events will
						   be ignored (except DestroyNotify) until their X windows are
						   reset) */
						hwnd = Hwnd.ObjectFromHandle(controls[i].Handle);
						hwnd.zombie = true;
					}
					SendWMDestroyMessages(controls[i]);
				}
			}
		}

		void CleanupCachedWindows (Hwnd hwnd)
		{
			if (ActiveWindow == hwnd.Handle) {
				SendMessage(hwnd.client_window, Msg.WM_ACTIVATE, (IntPtr)WindowActiveFlags.WA_INACTIVE, IntPtr.Zero);
				ActiveWindow = IntPtr.Zero;
			}

			if (FocusWindow == hwnd.Handle) {
				SendMessage(hwnd.client_window, Msg.WM_KILLFOCUS, IntPtr.Zero, IntPtr.Zero);
				FocusWindow = IntPtr.Zero;
			}

			DestroyCaret (hwnd.Handle);
		}

		private void PerformNCCalc(Hwnd hwnd) {
			XplatUIWin32.NCCALCSIZE_PARAMS	ncp;
			IntPtr				ptr;
			Rectangle			rect;

			rect = hwnd.DefaultClientRect;

			ncp = new XplatUIWin32.NCCALCSIZE_PARAMS();
			ptr = Marshal.AllocHGlobal(Marshal.SizeOf(ncp));

			ncp.rgrc1.left = rect.Left;
			ncp.rgrc1.top = rect.Top;
			ncp.rgrc1.right = rect.Right;
			ncp.rgrc1.bottom = rect.Bottom;

			Marshal.StructureToPtr(ncp, ptr, true);
			NativeWindow.WndProc(hwnd.client_window, Msg.WM_NCCALCSIZE, (IntPtr)1, ptr);
			ncp = (XplatUIWin32.NCCALCSIZE_PARAMS)Marshal.PtrToStructure(ptr, typeof(XplatUIWin32.NCCALCSIZE_PARAMS));
			Marshal.FreeHGlobal(ptr);

			// FIXME - debug this with Menus

			rect = new Rectangle(ncp.rgrc1.left, ncp.rgrc1.top, ncp.rgrc1.right - ncp.rgrc1.left, ncp.rgrc1.bottom - ncp.rgrc1.top);
			hwnd.ClientRect = rect;

			if (hwnd.visible) {
				if ((rect.Width < 1) || (rect.Height < 1)) {
					XMoveResizeWindow(DisplayHandle, hwnd.client_window, -5, -5, 1, 1);
				} else {
					XMoveResizeWindow(DisplayHandle, hwnd.client_window, rect.X, rect.Y, rect.Width, rect.Height);
				}
			}
		}
		#endregion	// Private Methods

		#region	Callbacks
		private void MouseHover(object sender, EventArgs e) {
			XEvent	xevent;
			Hwnd	hwnd;

			HoverState.Timer.Enabled = false;

			if (HoverState.Window != IntPtr.Zero) {
				hwnd = Hwnd.GetObjectFromWindow(HoverState.Window);
				if (hwnd != null) {
					xevent = new XEvent ();

					xevent.type = XEventName.ClientMessage;
					xevent.ClientMessageEvent.display = DisplayHandle;
					xevent.ClientMessageEvent.window = HoverState.Window;
					xevent.ClientMessageEvent.message_type = HoverState.Atom;
					xevent.ClientMessageEvent.format = 32;
					xevent.ClientMessageEvent.ptr1 = (IntPtr) (HoverState.Y << 16 | HoverState.X);

					hwnd.Queue.EnqueueLocked (xevent);

					WakeupMain ();
				}
			}
		}

		private void CaretCallback(object sender, EventArgs e) {
			if (Caret.Paused) {
				return;
			}
			Caret.On = !Caret.On;

			XDrawLine(DisplayHandle, Caret.Hwnd, Caret.gc, Caret.X, Caret.Y, Caret.X, Caret.Y + Caret.Height);
		}
		#endregion	// Callbacks

		#region Public Properties

		internal override int Caption {
			get {
				return 19;
			}
		}

		internal override  Size CursorSize {
			get {
				int	x;
				int	y;

				if (XQueryBestCursor(DisplayHandle, RootWindow, 32, 32, out x, out y) != 0) {
					return new Size(x, y);
				} else {
					return new Size(16, 16);
				}
			}
		} 

		internal override  bool DragFullWindows {
			get {
				return true;
			}
		} 

		internal override  Size DragSize {
			get {
				return new Size(4, 4);
			}
		} 

		internal override  Size FrameBorderSize { 
			get {
				throw new NotImplementedException(); 
			}
		}

		internal override  Size IconSize {
			get {
				IntPtr		list;
				XIconSize	size;
				int		count;

				if (XGetIconSizes(DisplayHandle, RootWindow, out list, out count) != 0) {
					long		current;
					int		largest;

					current = (long)list;
					largest = 0;

					size = new XIconSize();

					for (int i = 0; i < count; i++) {
						size = (XIconSize)Marshal.PtrToStructure((IntPtr)current, size.GetType());
						current += Marshal.SizeOf(size);

						// Look for our preferred size
						if (size.min_width == 32) {
							XFree(list);
							return new Size(32, 32);
						}

						if (size.max_width == 32) {
							XFree(list);
							return new Size(32, 32);
						}

						if (size.min_width < 32 && size.max_width > 32) {
							int	x;

							// check if we can fit one
							x = size.min_width;
							while (x < size.max_width) {
								x += size.width_inc;
								if (x == 32) {
									XFree(list);
									return new Size(32, 32);
								}
							}
						}

						if (largest < size.max_width) {
							largest = size.max_width;
						}
					}

					// We didn't find a match or we wouldn't be here
					return new Size(largest, largest);

				} else {
					return new Size(32, 32);
				}
			}
		} 

		internal override int KeyboardSpeed {
			get{
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

		internal override  Size MaxWindowTrackSize {
			get {
				return new Size (WorkingArea.Width, WorkingArea.Height);
			}
		} 

		internal override  Size MinimizedWindowSize {
			get {
				return new Size(1, 1);
			}
		} 

		internal override  Size MinimizedWindowSpacingSize {
			get {
				return new Size(1, 1);
			}
		} 

		internal override  Size MinimumWindowSize {
			get {
				return new Size(1, 1);
			}
		} 

		internal override  Size MinWindowTrackSize {
			get {
				return new Size(1, 1);
			}
		}

		internal override Keys ModifierKeys {
			get {
				return Keyboard.ModifierKeys;
			}
		}

		internal override  Size SmallIconSize {
			get {
				IntPtr		list;
				XIconSize	size;
				int		count;

				if (XGetIconSizes(DisplayHandle, RootWindow, out list, out count) != 0) {
					long		current;
					int		smallest;

					current = (long)list;
					smallest = 0;

					size = new XIconSize();

					for (int i = 0; i < count; i++) {
						size = (XIconSize)Marshal.PtrToStructure((IntPtr)current, size.GetType());
						current += Marshal.SizeOf(size);

						// Look for our preferred size
						if (size.min_width == 16) {
							XFree(list);
							return new Size(16, 16);
						}

						if (size.max_width == 16) {
							XFree(list);
							return new Size(16, 16);
						}

						if (size.min_width < 16 && size.max_width > 16) {
							int	x;

							// check if we can fit one
							x = size.min_width;
							while (x < size.max_width) {
								x += size.width_inc;
								if (x == 16) {
									XFree(list);
									return new Size(16, 16);
								}
							}
						}

						if (smallest == 0 || smallest > size.min_width) {
							smallest = size.min_width;
						}
					}

					// We didn't find a match or we wouldn't be here
					return new Size(smallest, smallest);

				} else {
					return new Size(16, 16);
				}
			}
		} 

		internal override  int MouseButtonCount {
			get {
				return 3;
			}
		} 

		internal override  bool MouseButtonsSwapped {
			get {
				return false;	// FIXME - how to detect?
			}
		} 

		internal override Size MouseHoverSize {
			get {
				return new Size (1, 1);
			}
		}

		internal override int MouseHoverTime {
			get {
				return HoverState.Interval;
			}
		}



		internal override  bool MouseWheelPresent {
			get {
				return true;	// FIXME - how to detect?
			}
		} 

		internal override  Rectangle VirtualScreen {
			get {
				return WorkingArea;
			}
		} 

		internal override  Rectangle WorkingArea {
			get {
				IntPtr			actual_atom;
				int			actual_format;
				IntPtr			nitems;
				IntPtr			bytes_after;
				IntPtr			prop = IntPtr.Zero;
				int			width;
				int			height;
				int			current_desktop;
				int			x;
				int			y;

				XGetWindowProperty(DisplayHandle, RootWindow, _NET_CURRENT_DESKTOP, IntPtr.Zero, new IntPtr(1), false, (IntPtr)Atom.XA_CARDINAL, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);
				if ((long)nitems < 1) {
					goto failsafe;
				}

				current_desktop = Marshal.ReadIntPtr(prop, 0).ToInt32();
				XFree(prop);

				XGetWindowProperty(DisplayHandle, RootWindow, _NET_WORKAREA, IntPtr.Zero, new IntPtr (256), false, (IntPtr)Atom.XA_CARDINAL, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);
				if ((long)nitems < 4 * current_desktop) {
					goto failsafe;
				}

				x = Marshal.ReadIntPtr(prop, IntPtr.Size * 4 * current_desktop).ToInt32();
				y = Marshal.ReadIntPtr(prop, IntPtr.Size * 4 * current_desktop + IntPtr.Size).ToInt32();
				width = Marshal.ReadIntPtr(prop, IntPtr.Size * 4 * current_desktop + IntPtr.Size * 2).ToInt32();
				height = Marshal.ReadIntPtr(prop, IntPtr.Size * 4 * current_desktop + IntPtr.Size * 3).ToInt32();
				XFree(prop);

				return new Rectangle(x, y, width, height);

			failsafe:
				XWindowAttributes	attributes=new XWindowAttributes();

				lock (XlibLock) {
					XGetWindowAttributes(DisplayHandle, XRootWindow(DisplayHandle, 0), ref attributes);
				}

				return new Rectangle(0, 0, attributes.width, attributes.height);
			}
		}

		internal override bool ThemesEnabled {
			get {
				return XplatUIX11.themes_enabled;
			}
		}
 

		#endregion	// Public properties

		#region Public Static Methods
		internal override IntPtr InitializeDriver() {
			lock (this) {
				if (DisplayHandle==IntPtr.Zero) {
					SetDisplay(XOpenDisplay(IntPtr.Zero));
				}
			}
			return IntPtr.Zero;
		}

		internal override void ShutdownDriver(IntPtr token) {
			lock (this) {
				if (DisplayHandle!=IntPtr.Zero) {
					XCloseDisplay(DisplayHandle);
					DisplayHandle=IntPtr.Zero;
				}
			}
		}

		internal override void EnableThemes() {
			themes_enabled = true;
		}


		internal override void Activate(IntPtr handle) {
			Hwnd hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (hwnd != null) lock (XlibLock) {
				SendNetWMMessage(hwnd.whole_window, _NET_ACTIVE_WINDOW, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
				//XRaiseWindow(DisplayHandle, handle);
			}
			return;
		}

		internal override void AudibleAlert() {
			XBell(DisplayHandle, 0);
			return;
		}


		internal override void CaretVisible(IntPtr handle, bool visible) {
			if (Caret.Hwnd == handle) {
				if (visible) {
					if (!Caret.Visible) {
						Caret.Visible = true;
						ShowCaret();
						Caret.Timer.Start();
					}
				} else {
					Caret.Visible = false;
					Caret.Timer.Stop();
					HideCaret();
				}
			}
		}

		internal override bool CalculateWindowRect(ref Rectangle ClientRect, int Style, int ExStyle, Menu menu, out Rectangle WindowRect) {
			FormBorderStyle	border_style;
			TitleStyle	title_style;
			int caption_height;
			int tool_caption_height;

			DeriveStyles(Style, ExStyle, out border_style, out title_style,
				out caption_height, out tool_caption_height);

			WindowRect = Hwnd.GetWindowRectangle(border_style, menu, title_style,
					caption_height, tool_caption_height,
					ClientRect);
			return true;
		}

		internal override void ClientToScreen(IntPtr handle, ref int x, ref int y) {
			int	dest_x_return;
			int	dest_y_return;
			IntPtr	child;
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			lock (XlibLock) {
				XTranslateCoordinates(DisplayHandle, hwnd.client_window, RootWindow, x, y, out dest_x_return, out dest_y_return, out child);
			}

			x = dest_x_return;
			y = dest_y_return;
		}

		internal override int[] ClipboardAvailableFormats(IntPtr handle) {
			DataFormats.Format	f;
			int[]			result;

			f = DataFormats.Format.List;

			if (XGetSelectionOwner(DisplayHandle, CLIPBOARD) == IntPtr.Zero) {
				return null;
			}

			Clipboard.Formats = new ArrayList();

			while (f != null) {
				XConvertSelection(DisplayHandle, CLIPBOARD, (IntPtr)f.Id, (IntPtr)f.Id, FosterParent, IntPtr.Zero);

				Clipboard.Enumerating = true;
				while (Clipboard.Enumerating) {
					UpdateMessageQueue(null);
				}
				f = f.Next;
			}

			result = new int[Clipboard.Formats.Count];

			for (int i = 0; i < Clipboard.Formats.Count; i++) {
				result[i] = ((IntPtr)Clipboard.Formats[i]).ToInt32 ();
			}

			Clipboard.Formats = null;
			return result;
		}

		internal override void ClipboardClose(IntPtr handle) {
			if (handle != ClipMagic) {
				throw new ArgumentException("handle is not a valid clipboard handle");
			}
			return;
		}

		internal override int ClipboardGetID(IntPtr handle, string format) {
			if (handle != ClipMagic) {
				throw new ArgumentException("handle is not a valid clipboard handle");
			}

			if (format == "Text" ) return (int)Atom.XA_STRING;
			else if (format == "Bitmap" ) return (int)Atom.XA_BITMAP;
			//else if (format == "MetaFilePict" ) return 3;
			//else if (format == "SymbolicLink" ) return 4;
			//else if (format == "DataInterchangeFormat" ) return 5;
			//else if (format == "Tiff" ) return 6;
			else if (format == "OEMText" ) return XInternAtom(DisplayHandle, "COMPOUND_TEXT", false).ToInt32();
			else if (format == "DeviceIndependentBitmap" ) return (int)Atom.XA_PIXMAP;
			else if (format == "Palette" ) return (int)Atom.XA_COLORMAP;	// Useless
			//else if (format == "PenData" ) return 10;
			//else if (format == "RiffAudio" ) return 11;
			//else if (format == "WaveAudio" ) return 12;
			else if (format == "UnicodeText" ) return XInternAtom(DisplayHandle, "UTF8_STRING", false).ToInt32();
			//else if (format == "EnhancedMetafile" ) return 14;
			//else if (format == "FileDrop" ) return 15;
			//else if (format == "Locale" ) return 16;

			return XInternAtom(DisplayHandle, format, false).ToInt32();
		}

		internal override IntPtr ClipboardOpen(bool primary_selection) {
			if (!primary_selection)
				ClipMagic = CLIPBOARD;
			else
				ClipMagic = PRIMARY;
			return ClipMagic;
		}

		internal override object ClipboardRetrieve(IntPtr handle, int type, XplatUI.ClipboardToObject converter) {
			XConvertSelection(DisplayHandle, handle, (IntPtr)type, (IntPtr)type, FosterParent, IntPtr.Zero);

			Clipboard.Retrieving = true;
			while (Clipboard.Retrieving) {
				UpdateMessageQueue(null);
			}

			return Clipboard.Item;
		}

		internal override void ClipboardStore(IntPtr handle, object obj, int type, XplatUI.ObjectToClipboard converter) {
			Clipboard.Item = obj;
			Clipboard.Type = type;
			Clipboard.Converter = converter;

			if (obj != null) {
				XSetSelectionOwner(DisplayHandle, CLIPBOARD, FosterParent, IntPtr.Zero);
			} else {
				// Clearing the selection
				XSetSelectionOwner(DisplayHandle, CLIPBOARD, IntPtr.Zero, IntPtr.Zero);
			}
		}

		internal override void CreateCaret(IntPtr handle, int width, int height) {
			XGCValues	gc_values;
			Hwnd		hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (Caret.Hwnd != IntPtr.Zero) {
				DestroyCaret(Caret.Hwnd);
			}

			Caret.Hwnd = handle;
			Caret.Window = hwnd.client_window;
			Caret.Width = width;
			Caret.Height = height;
			Caret.Visible = false;
			Caret.On = false;

			gc_values = new XGCValues();
			gc_values.line_width = width;

			Caret.gc = XCreateGC(DisplayHandle, Caret.Window, new IntPtr ((int)GCFunction.GCLineWidth), ref gc_values);
			if (Caret.gc == IntPtr.Zero) {
				Caret.Hwnd = IntPtr.Zero;
				return;
			}

			XSetFunction(DisplayHandle, Caret.gc, GXFunction.GXinvert);
		}

		internal override IntPtr CreateWindow(CreateParams cp) {
			XSetWindowAttributes	Attributes;
			Hwnd			hwnd;
			int			X;
			int			Y;
			int			Width;
			int			Height;
			IntPtr			ParentHandle;
			IntPtr			WholeWindow;
			IntPtr			ClientWindow;
			Rectangle		ClientRect;
			SetWindowValuemask	ValueMask;
			int[]			atoms;

			hwnd = new Hwnd();

			Attributes = new XSetWindowAttributes();
			X = cp.X;
			Y = cp.Y;
			Width = cp.Width;
			Height = cp.Height;

			if (Width<1) Width=1;
			if (Height<1) Height=1;

			if (cp.Parent != IntPtr.Zero) {
				ParentHandle = Hwnd.ObjectFromHandle(cp.Parent).client_window;
			} else {
				if (StyleSet (cp.Style, WindowStyles.WS_CHILD)) {
					// We need to use our foster parent window until this poor child gets it's parent assigned
					ParentHandle=FosterParent;
				} else if (StyleSet (cp.Style, WindowStyles.WS_POPUP)) {
					ParentHandle=RootWindow;
				} else {
					// Default position on screen, if window manager doesn't place us somewhere else
					if (X<0) X = 50;
					if (Y<0) Y = 50;
					ParentHandle=RootWindow;
				}
			}

			ValueMask = SetWindowValuemask.BitGravity | SetWindowValuemask.WinGravity;

			Attributes.bit_gravity = Gravity.NorthWestGravity;
			Attributes.win_gravity = Gravity.NorthWestGravity;

			// Save what's under the toolwindow
			if (ExStyleSet (cp.ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
				Attributes.save_under = true;
				ValueMask |= SetWindowValuemask.SaveUnder;
			}


			// If we're a popup without caption we override the WM
			if (StyleSet (cp.Style, WindowStyles.WS_POPUP) && !StyleSet (cp.Style, WindowStyles.WS_CAPTION)) {
				Attributes.override_redirect = true;
				ValueMask |= SetWindowValuemask.OverrideRedirect;
			}

			hwnd.x = X;
			hwnd.y = Y;
			hwnd.width = Width;
			hwnd.height = Height;
			hwnd.parent = Hwnd.ObjectFromHandle(cp.Parent);

			if (StyleSet (cp.Style, WindowStyles.WS_DISABLED)) {
				hwnd.enabled = false;
			}

			ClientRect = hwnd.ClientRect;
			ClientWindow = IntPtr.Zero;

			lock (XlibLock) {
				WholeWindow = XCreateWindow(DisplayHandle, ParentHandle, X, Y, Width, Height, 0, (int)CreateWindowArgs.CopyFromParent, (int)CreateWindowArgs.InputOutput, IntPtr.Zero, new UIntPtr ((uint)ValueMask), ref Attributes);
				if (WholeWindow != IntPtr.Zero) {
					ValueMask &= ~(SetWindowValuemask.OverrideRedirect | SetWindowValuemask.SaveUnder);

					if (CustomVisual != IntPtr.Zero && CustomColormap != IntPtr.Zero) {
						ValueMask = SetWindowValuemask.ColorMap;
						Attributes.colormap = CustomColormap;
					}
					ClientWindow = XCreateWindow(DisplayHandle, WholeWindow, ClientRect.X, ClientRect.Y, ClientRect.Width, ClientRect.Height, 0, (int)CreateWindowArgs.CopyFromParent, (int)CreateWindowArgs.InputOutput, CustomVisual, new UIntPtr ((uint)ValueMask), ref Attributes);
				}
			}

			if ((WholeWindow == IntPtr.Zero) || (ClientWindow == IntPtr.Zero)) {
				throw new Exception("Could not create X11 windows");
			}

			hwnd.Queue = ThreadQueue(Thread.CurrentThread);
			hwnd.WholeWindow = WholeWindow;
			hwnd.ClientWindow = ClientWindow;

			#if DriverDebug || DriverDebugCreate
				Console.WriteLine("Created window {0:X} / {1:X} parent {2:X}, Style {3}, ExStyle {4}", ClientWindow.ToInt32(), WholeWindow.ToInt32(), hwnd.parent != null ? hwnd.parent.Handle.ToInt32() : 0, (WindowStyles)cp.Style, (WindowExStyles)cp.ExStyle);
			#endif

			if (!StyleSet (cp.Style, WindowStyles.WS_CHILD)) {
				if ((X != unchecked((int)0x80000000)) && (Y != unchecked((int)0x80000000))) {
					XSizeHints	hints;

					hints = new XSizeHints();
					hints.x = X;
					hints.y = Y;
					hints.flags = (IntPtr)(XSizeHintsFlags.USPosition | XSizeHintsFlags.PPosition);
					XSetWMNormalHints(DisplayHandle, WholeWindow, ref hints);
				}
			}

			lock (XlibLock) {
				XSelectInput(DisplayHandle, hwnd.whole_window, new IntPtr ((int)SelectInputMask));
				XSelectInput(DisplayHandle, hwnd.client_window, new IntPtr ((int)SelectInputMask));

				if (StyleSet (cp.Style, WindowStyles.WS_VISIBLE)) {
					MapWindow(hwnd, WindowType.Both);
					hwnd.visible = true;
				}
			}

			if (ExStyleSet (cp.ExStyle, WindowExStyles.WS_EX_TOPMOST)) {
				atoms = new int[2];
				atoms[0] = _NET_WM_WINDOW_TYPE_NORMAL.ToInt32();
				XChangeProperty(DisplayHandle, hwnd.whole_window, _NET_WM_WINDOW_TYPE, (IntPtr)Atom.XA_ATOM, 32, PropertyMode.Replace, atoms, 1);

				XSetTransientForHint (DisplayHandle, hwnd.whole_window, RootWindow);
			} else if (!ExStyleSet (cp.ExStyle, WindowExStyles.WS_EX_APPWINDOW)) {
//				XSetTransientForHint (DisplayHandle, hwnd.whole_window, FosterParent);
			}

			SetWMStyles(hwnd, cp);
			
			// set the group leader
			XWMHints wm_hints = new XWMHints ();
			
			wm_hints.flags = (IntPtr)(XWMHintsFlags.InputHint | XWMHintsFlags.StateHint | XWMHintsFlags.WindowGroupHint);
			wm_hints.input = !StyleSet (cp.Style, WindowStyles.WS_DISABLED);
			wm_hints.initial_state = StyleSet (cp.Style, WindowStyles.WS_MINIMIZE) ? XInitialState.IconicState : XInitialState.NormalState;
			
			if (ParentHandle != RootWindow) {
				wm_hints.window_group = hwnd.whole_window;
			} else {
				wm_hints.window_group = ParentHandle;
			}
			
			lock (XlibLock) {
				XSetWMHints(DisplayHandle, hwnd.whole_window, ref wm_hints );
			}

			if (StyleSet (cp.Style, WindowStyles.WS_MINIMIZE)) {
				SetWindowState(hwnd.Handle, FormWindowState.Minimized);
			} else if (StyleSet (cp.Style, WindowStyles.WS_MAXIMIZE)) {
				SetWindowState(hwnd.Handle, FormWindowState.Maximized);
			}

			// for now make all windows dnd enabled
			Dnd.SetAllowDrop (hwnd, true);

			// Set caption/window title
			Text(hwnd.Handle, cp.Caption);

			return hwnd.Handle;
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

			if (XQueryBestCursor(DisplayHandle, RootWindow, bitmap.Width, bitmap.Height, out width, out height) == 0) {
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
					} else if (and && !xor) {
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

			cursor_pixmap = XCreatePixmapFromBitmapData(DisplayHandle, RootWindow, cursor_bits, width, height, (IntPtr)1, (IntPtr)0, 1);
			mask_pixmap = XCreatePixmapFromBitmapData(DisplayHandle, RootWindow, mask_bits, width, height, (IntPtr)1, (IntPtr)0, 1);
			fg = new XColor();
			bg = new XColor();

			fg.pixel = XWhitePixel(DisplayHandle, ScreenNo);
			fg.red = (ushort)65535;
			fg.green = (ushort)65535;
			fg.blue = (ushort)65535;

			bg.pixel = XBlackPixel(DisplayHandle, ScreenNo);

			cursor = XCreatePixmapCursor(DisplayHandle, cursor_pixmap, mask_pixmap, ref fg, ref bg, xHotSpot, yHotSpot);

			XFreePixmap(DisplayHandle, cursor_pixmap);
			XFreePixmap(DisplayHandle, mask_pixmap);

			return cursor;
		}

		internal override IntPtr DefineStdCursor(StdCursor id) {
			CursorFontShape	shape;
			IntPtr		cursor;

			// FIXME - define missing shapes

			switch (id) {
				case StdCursor.AppStarting: {
					shape = CursorFontShape.XC_watch;
					break;
				}

				case StdCursor.Arrow: {
					shape = CursorFontShape.XC_top_left_arrow;
					break;
				}

				case StdCursor.Cross: {
					shape = CursorFontShape.XC_crosshair;
					break;
				}

				case StdCursor.Default: {
					shape = CursorFontShape.XC_top_left_arrow;
					break;
				}

				case StdCursor.Hand: {
					shape = CursorFontShape.XC_hand1;
					break;
				}

				case StdCursor.Help: {
					shape = CursorFontShape.XC_question_arrow;
					break;
				}

				case StdCursor.HSplit: {
                                        shape = CursorFontShape.XC_sb_v_double_arrow; 
					break;
				}

				case StdCursor.IBeam: {
					shape = CursorFontShape.XC_xterm; 
					break;
				}

				case StdCursor.No: {
					shape = CursorFontShape.XC_circle; 
					break;
				}

				case StdCursor.NoMove2D: {
					shape = CursorFontShape.XC_fleur; 
					break;
				}

				case StdCursor.NoMoveHoriz: {
					shape = CursorFontShape.XC_fleur; 
					break;
				}

				case StdCursor.NoMoveVert: {
					shape = CursorFontShape.XC_fleur; 
					break;
				}

				case StdCursor.PanEast: {
					shape = CursorFontShape.XC_fleur; 
					break;
				}

				case StdCursor.PanNE: {
					shape = CursorFontShape.XC_fleur; 
					break;
				}

				case StdCursor.PanNorth: {
					shape = CursorFontShape.XC_fleur; 
					break;
				}

				case StdCursor.PanNW: {
					shape = CursorFontShape.XC_fleur; 
					break;
				}

				case StdCursor.PanSE: {
					shape = CursorFontShape.XC_fleur; 
					break;
				}

				case StdCursor.PanSouth: {
					shape = CursorFontShape.XC_fleur; 
					break;
				}

				case StdCursor.PanSW: {
					shape = CursorFontShape.XC_fleur; 
					break;
				}

				case StdCursor.PanWest: {
					shape = CursorFontShape.XC_sizing; 
					break;
				}

				case StdCursor.SizeAll: {
					shape = CursorFontShape.XC_fleur; 
					break;
				}

				case StdCursor.SizeNESW: {
					shape = CursorFontShape.XC_top_right_corner; 
					break;
				}

				case StdCursor.SizeNS: {
					shape = CursorFontShape.XC_sb_v_double_arrow;
					break;
				}

				case StdCursor.SizeNWSE: {
					shape = CursorFontShape.XC_top_left_corner; 
					break;
				}

				case StdCursor.SizeWE: {
					shape = CursorFontShape.XC_sb_h_double_arrow; 
					break;
				}

				case StdCursor.UpArrow: {
					shape = CursorFontShape.XC_center_ptr; 
					break;
				}

				case StdCursor.VSplit: {
                                        shape = CursorFontShape.XC_sb_h_double_arrow;
					break;
				}

				case StdCursor.WaitCursor: {
					shape = CursorFontShape.XC_watch; 
					break;
				}

				default: {
					return IntPtr.Zero;
				}
			}

			lock (XlibLock) {
				cursor = XCreateFontCursor(DisplayHandle, shape);
			}
			return cursor;
		}

		internal override IntPtr DefWndProc(ref Message msg) {
			switch ((Msg)msg.Msg) {
				case Msg.WM_PAINT: {
					Hwnd hwnd;

					hwnd = Hwnd.GetObjectFromWindow(msg.HWnd);
					if (hwnd != null) {
						hwnd.expose_pending = false;
					}

					return IntPtr.Zero;
				}

				case Msg.WM_NCPAINT: {
					Hwnd hwnd;

					hwnd = Hwnd.GetObjectFromWindow(msg.HWnd);
					if (hwnd != null) {
						hwnd.nc_expose_pending = false;
					}

					return IntPtr.Zero;
				}

				case Msg.WM_CONTEXTMENU: {
					Hwnd hwnd;

					hwnd = Hwnd.GetObjectFromWindow(msg.HWnd);

					if ((hwnd != null) && (hwnd.parent != null)) {
						SendMessage(hwnd.parent.client_window, Msg.WM_CONTEXTMENU, msg.WParam, msg.LParam);
					}
					return IntPtr.Zero;
				}

				case Msg.WM_MOUSEWHEEL: {
					Hwnd hwnd;

					hwnd = Hwnd.GetObjectFromWindow(msg.HWnd);

					if ((hwnd != null) && (hwnd.parent != null)) {
						SendMessage(hwnd.parent.client_window, Msg.WM_MOUSEWHEEL, msg.WParam, msg.LParam);
						if (msg.Result == IntPtr.Zero) {
							return IntPtr.Zero;
						}
					}
					return IntPtr.Zero;
				}

				case Msg.WM_SETCURSOR: {
					Hwnd	hwnd;

					hwnd = Hwnd.GetObjectFromWindow(msg.HWnd);
					if (hwnd == null)
						break; // not sure how this happens, but it does

					// Pass to parent window first
					while ((hwnd.parent != null) && (msg.Result == IntPtr.Zero)) {
						hwnd = hwnd.parent;
						msg.Result = NativeWindow.WndProc(hwnd.Handle, Msg.WM_SETCURSOR, msg.HWnd, msg.LParam);
					}

					if (msg.Result == IntPtr.Zero) {
						IntPtr handle;

						switch((HitTest)(msg.LParam.ToInt32() & 0xffff)) {
							case HitTest.HTBOTTOM:		handle = Cursors.SizeNS.handle; break;
							case HitTest.HTBORDER:		handle = Cursors.SizeNS.handle; break;
							case HitTest.HTBOTTOMLEFT:	handle = Cursors.SizeNESW.handle; break;
							case HitTest.HTBOTTOMRIGHT:	handle = Cursors.SizeNWSE.handle; break;
							case HitTest.HTERROR:		if ((msg.LParam.ToInt32() >> 16) == (int)Msg.WM_LBUTTONDOWN) {
												AudibleAlert();
											}
											handle = Cursors.Default.handle;
											break;

							case HitTest.HTHELP:		handle = Cursors.Help.handle; break;
							case HitTest.HTLEFT:		handle = Cursors.SizeWE.handle; break;
							case HitTest.HTRIGHT:		handle = Cursors.SizeWE.handle; break;
							case HitTest.HTTOP:		handle = Cursors.SizeNS.handle; break;
							case HitTest.HTTOPLEFT:		handle = Cursors.SizeNWSE.handle; break;
							case HitTest.HTTOPRIGHT:	handle = Cursors.SizeNESW.handle; break;

							#if SameAsDefault
							case HitTest.HTGROWBOX:
							case HitTest.HTSIZE:
							case HitTest.HTZOOM:
							case HitTest.HTVSCROLL:
							case HitTest.HTSYSMENU:
							case HitTest.HTREDUCE:
							case HitTest.HTNOWHERE:
							case HitTest.HTMAXBUTTON:
							case HitTest.HTMINBUTTON:
							case HitTest.HTMENU:
							case HitTest.HSCROLL:
							case HitTest.HTBOTTOM:
							case HitTest.HTCAPTION:
							case HitTest.HTCLIENT:
							case HitTest.HTCLOSE:
							#endif
							default: handle = Cursors.Default.handle; break;
						}
						SetCursor(msg.HWnd, handle);
					}
					return (IntPtr)1;
				}
			}
			return IntPtr.Zero;
		}

		internal override void DestroyCaret(IntPtr handle) {
			if (Caret.Hwnd == handle) {
				if (Caret.Visible == true) {
					Caret.Timer.Stop();
					HideCaret();
				}
				if (Caret.gc != IntPtr.Zero) {
					XFreeGC(DisplayHandle, Caret.gc);
					Caret.gc = IntPtr.Zero;
				}
				Caret.Hwnd = IntPtr.Zero;
				Caret.Visible = false;
				Caret.On = false;
			}
		}

		internal override void DestroyCursor(IntPtr cursor) {
			lock (XlibLock) {
				XFreeCursor(DisplayHandle, cursor);
			}
		}

		internal override void DestroyWindow(IntPtr handle) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (hwnd == null) {
				#if DriverDebug || DriverDebugDestroy
					Console.WriteLine("window {0:X} already destroyed", handle.ToInt32());
				#endif
				return;
			}

			#if DriverDebug || DriverDebugDestroy
				Console.WriteLine("Destroying window {0:X}", handle.ToInt32());
			#endif

			CleanupCachedWindows (hwnd);

			SendWMDestroyMessages(Control.ControlNativeWindow.ControlFromHandle(hwnd.Handle));

			lock (XlibLock) {
				if (hwnd.whole_window != IntPtr.Zero) {
					XDestroyWindow(DisplayHandle, hwnd.whole_window);
				}
				else if (hwnd.client_window != IntPtr.Zero) {
					XDestroyWindow(DisplayHandle, hwnd.client_window);
				}

			}
			hwnd.Dispose();
		}

		internal override IntPtr DispatchMessage(ref MSG msg) {
			return NativeWindow.WndProc(msg.hwnd, msg.message, msg.wParam, msg.lParam);
		}

		internal override void DrawReversibleRectangle(IntPtr handle, Rectangle rect, int line_width) {
			Hwnd		hwnd;
			XGCValues	gc_values;
			IntPtr		gc;

			hwnd = Hwnd.ObjectFromHandle(handle);

			gc_values = new XGCValues();

			gc_values.subwindow_mode = GCSubwindowMode.IncludeInferiors;
			gc_values.line_width = line_width;
			gc_values.foreground = XBlackPixel(DisplayHandle, ScreenNo);

			// This logic will give us true rubber bands: (libsx, SANE_XOR)
			//mask = foreground ^ background; 
			//XSetForeground(DisplayHandle, gc, 0xffffffff);
			//XSetBackground(DisplayHandle, gc, background);
			//XSetFunction(DisplayHandle,   gc, GXxor);
			//XSetPlaneMask(DisplayHandle,  gc, mask);


			gc = XCreateGC(DisplayHandle, hwnd.client_window, new IntPtr ((int) (GCFunction.GCSubwindowMode | GCFunction.GCLineWidth | GCFunction.GCForeground)), ref gc_values);
			uint foreground;
			uint background;

			Control control;
			control = Control.FromHandle(handle);

			XColor xcolor = new XColor();

			xcolor.red = (ushort)(control.ForeColor.R * 257);
			xcolor.green = (ushort)(control.ForeColor.G * 257);
			xcolor.blue = (ushort)(control.ForeColor.B * 257);
			XAllocColor(DisplayHandle, DefaultColormap, ref xcolor);
			foreground = (uint)xcolor.pixel.ToInt32();

			xcolor.red = (ushort)(control.BackColor.R * 257);
			xcolor.green = (ushort)(control.BackColor.G * 257);
			xcolor.blue = (ushort)(control.BackColor.B * 257);
			XAllocColor(DisplayHandle, DefaultColormap, ref xcolor);
			background = (uint)xcolor.pixel.ToInt32();

			uint mask = foreground ^ background; 

			XSetForeground(DisplayHandle, gc, (UIntPtr)0xffffffff);
			XSetBackground(DisplayHandle, gc, (UIntPtr)background);
			XSetFunction(DisplayHandle,   gc, GXFunction.GXxor);
			XSetPlaneMask(DisplayHandle,  gc, (IntPtr)mask);

			if ((rect.Width > 0) && (rect.Height > 0)) {
				XDrawRectangle(DisplayHandle, hwnd.client_window, gc, rect.Left, rect.Top, rect.Width, rect.Height);
			} else {
				if (rect.Width > 0) {
					XDrawLine(DisplayHandle, hwnd.client_window, gc, rect.X, rect.Y, rect.Right, rect.Y);
				} else {
					XDrawLine(DisplayHandle, hwnd.client_window, gc, rect.X, rect.Y, rect.X, rect.Bottom);
				}
			}
			XFreeGC(DisplayHandle, gc);
		}

		internal override void DoEvents() {
			MSG	msg = new MSG ();
			XEventQueue queue;

			if (OverrideCursorHandle != IntPtr.Zero) {
				OverrideCursorHandle = IntPtr.Zero;
			}

			queue = ThreadQueue(Thread.CurrentThread);

			queue.DispatchIdle = false;

			while (PeekMessage(queue, ref msg, IntPtr.Zero, 0, 0, (uint)PeekMessageFlags.PM_REMOVE)) {
				TranslateMessage (ref msg);
				DispatchMessage (ref msg);
			}

			queue.DispatchIdle = true;
		}

		internal override void EnableWindow(IntPtr handle, bool Enable) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			if (hwnd != null) {
				hwnd.Enabled = Enable;
			}
		}

		internal override void EndLoop(Thread thread) {
			// This is where we one day will shut down the loop for the thread
		}


		internal override IntPtr GetActive() {
			IntPtr	actual_atom;
			int	actual_format;
			IntPtr	nitems;
			IntPtr	bytes_after;
			IntPtr	prop = IntPtr.Zero;
			IntPtr	active = IntPtr.Zero;

			XGetWindowProperty(DisplayHandle, RootWindow, _NET_ACTIVE_WINDOW, IntPtr.Zero, new IntPtr (1), false, (IntPtr)Atom.XA_WINDOW, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);
			if (((long)nitems > 0) && (prop != IntPtr.Zero)) {
				active = (IntPtr)Marshal.ReadInt32(prop);
				XFree(prop);
			}

			if (active != IntPtr.Zero) {
				Hwnd	hwnd;

				hwnd = Hwnd.GetObjectFromWindow(active);
				if (hwnd != null) {
					active = hwnd.Handle;
				} else {
					active = IntPtr.Zero;
				}
			}
			return active;
		}

		internal override Region GetClipRegion(IntPtr handle) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			if (hwnd != null) {
				return hwnd.UserClip;
			}

			return null;
		}

		internal override void GetCursorInfo(IntPtr cursor, out int width, out int height, out int hotspot_x, out int hotspot_y) {
			width = 20;
			height = 20;
			hotspot_x = 0;
			hotspot_y = 0;
		}

		internal override void GetDisplaySize(out Size size) {
			XWindowAttributes	attributes=new XWindowAttributes();

			lock (XlibLock) {
				// FIXME - use _NET_WM messages instead?
				XGetWindowAttributes(DisplayHandle, XRootWindow(DisplayHandle, 0), ref attributes);
			}

			size = new Size(attributes.width, attributes.height);
		}

		internal override SizeF GetAutoScaleSize(Font font) {
			Graphics	g;
			float		width;
			string		magic_string = "The quick brown fox jumped over the lazy dog.";
			double		magic_number = 44.549996948242189;

			g = Graphics.FromHwnd(FosterParent);

			width = (float) (g.MeasureString (magic_string, font).Width / magic_number);
			return new SizeF(width, font.Height);
		}

		internal override IntPtr GetParent(IntPtr handle) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			if (hwnd != null && hwnd.parent != null) {
				return hwnd.parent.Handle;
			}
			return IntPtr.Zero;
		}

		internal override void GetCursorPos(IntPtr handle, out int x, out int y) {
			IntPtr	use_handle;
			IntPtr	root;
			IntPtr	child;
			int	root_x;
			int	root_y;
			int	win_x;
			int	win_y;
			int	keys_buttons;

			if (handle != IntPtr.Zero) {
				use_handle = Hwnd.ObjectFromHandle(handle).client_window;
			} else {
				use_handle = RootWindow;
			}

			lock (XlibLock) {
				XQueryPointer(DisplayHandle, use_handle, out root, out child, out root_x, out root_y, out win_x, out win_y, out keys_buttons);
			}

			if (handle != IntPtr.Zero) {
				x = win_x;
				y = win_y;
			} else {
				x = root_x;
				y = root_y;
			}
		}

		internal override IntPtr GetFocus() {
			return FocusWindow;
		}


		internal override bool GetFontMetrics(Graphics g, Font font, out int ascent, out int descent) {
			return GetFontMetrics(g.GetHdc(), font.ToHfont(), out ascent, out descent);
		}

		internal override Point GetMenuOrigin(IntPtr handle) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (hwnd != null) {
				return hwnd.MenuOrigin;
			}
			return Point.Empty;
		}

		[MonoTODO("Implement filtering")]
		internal override bool GetMessage(Object queue_id, ref MSG msg, IntPtr handle, int wFilterMin, int wFilterMax) {
			XEvent	xevent;
			bool	client;
			Hwnd	hwnd;

			ProcessNextMessage:

			if (((XEventQueue)queue_id).Count > 0) {
				xevent = (XEvent) ((XEventQueue)queue_id).Dequeue ();
			} else {
				UpdateMessageQueue ((XEventQueue)queue_id);

				if (((XEventQueue)queue_id).Count > 0) {
					xevent = (XEvent) ((XEventQueue)queue_id).Dequeue ();
				} else if (((XEventQueue)queue_id).Paint.Count > 0) {
					xevent = ((XEventQueue)queue_id).Paint.Dequeue();
				} else {
					if (!ThreadQueue(Thread.CurrentThread).PostQuitState) {
						msg.hwnd= IntPtr.Zero;
						msg.message = Msg.WM_ENTERIDLE;
						return true;
					}

					// We reset ourselves so GetMessage can be called again
					ThreadQueue(Thread.CurrentThread).PostQuitState = false;

					return false;
				}
			}

			hwnd = Hwnd.GetObjectFromWindow(xevent.AnyEvent.window);

			// Handle messages for windows that are already or are about to be destroyed.

			// We need to make sure we only allow DestroyNotify events through for zombie
			// hwnds, since much of the event handling code makes requests using the hwnd's
			// client_window, and that'll result in BadWindow errors if there's some lag
			// between the XDestroyWindow call and the DestroyNotify event.
			if (hwnd == null || (hwnd.zombie && xevent.type != XEventName.DestroyNotify)) {
				#if DriverDebug
					Console.WriteLine("GetMessage(): Got message {0} for non-existent or already destroyed window {1:X}", xevent.type, xevent.AnyEvent.window.ToInt32());
				#endif
				goto ProcessNextMessage;
			}

			if (hwnd.client_window == xevent.AnyEvent.window) {
				client = true;
				//Console.WriteLine("Client message, sending to window {0:X}", msg.hwnd.ToInt32());
			} else {
				client = false;
				//Console.WriteLine("Non-Client message, sending to window {0:X}", msg.hwnd.ToInt32());
			}

			msg.hwnd = hwnd.Handle;

			//
			// If you add a new event to this switch make sure to add it in
			// UpdateMessage also unless it is not coming through the X event system.
			//
			switch(xevent.type) {
				case XEventName.KeyPress: {
					Keyboard.KeyEvent (FocusWindow, xevent, ref msg);
					break;
				}

				case XEventName.KeyRelease: {
					Keyboard.KeyEvent (FocusWindow, xevent, ref msg);
					break;
				}

				case XEventName.ButtonPress: {
					switch(xevent.ButtonEvent.button) {
						case 1: {
							MouseState |= MouseButtons.Left;
							if (client) {
								msg.message = Msg.WM_LBUTTONDOWN;
							} else {
								msg.message = Msg.WM_NCLBUTTONDOWN;
								ClientToScreen (msg.hwnd, ref xevent.ButtonEvent.x, ref xevent.ButtonEvent.y);
							}
							// TODO: For WM_NCLBUTTONDOWN wParam specifies a hit-test value not the virtual keys down
							msg.wParam=GetMousewParam(0);
							break;
						}

						case 2: {
							MouseState |= MouseButtons.Middle;
							if (client) {
								msg.message = Msg.WM_MBUTTONDOWN;
							} else {
								msg.message = Msg.WM_NCMBUTTONDOWN;
								ClientToScreen (msg.hwnd, ref xevent.ButtonEvent.x, ref xevent.ButtonEvent.y);
							}
							msg.wParam=GetMousewParam(0);
							break;
						}

						case 3: {
							MouseState |= MouseButtons.Right;
							if (client) {
								msg.message = Msg.WM_RBUTTONDOWN;
							} else {
								msg.message = Msg.WM_NCRBUTTONDOWN;
								ClientToScreen (msg.hwnd, ref xevent.ButtonEvent.x, ref xevent.ButtonEvent.y);
							}
							msg.wParam=GetMousewParam(0);
							break;
						}

						case 4: {
							msg.hwnd = FocusWindow;
							msg.message=Msg.WM_MOUSEWHEEL;
							msg.wParam=GetMousewParam(120);
							break;
						}

						case 5: {
							msg.hwnd = FocusWindow;
							msg.message=Msg.WM_MOUSEWHEEL;
							msg.wParam=GetMousewParam(-120);
							break;
						}

					}

					msg.lParam=(IntPtr) (xevent.ButtonEvent.y << 16 | xevent.ButtonEvent.x);
					MousePosition.X = xevent.ButtonEvent.x;
					MousePosition.Y = xevent.ButtonEvent.y;

					if (!hwnd.Enabled) {
						IntPtr dummy;

						msg.hwnd = hwnd.EnabledHwnd;
						XTranslateCoordinates(DisplayHandle, xevent.AnyEvent.window, Hwnd.ObjectFromHandle(msg.hwnd).ClientWindow, xevent.ButtonEvent.x, xevent.ButtonEvent.y, out xevent.ButtonEvent.x, out xevent.ButtonEvent.y, out dummy);
						msg.lParam = (IntPtr)(MousePosition.Y << 16 | MousePosition.X);
					}

					if (Grab.Hwnd != IntPtr.Zero) {
						msg.hwnd = Grab.Hwnd;
					}

					if (ClickPending.Pending && ((((long)xevent.ButtonEvent.time - ClickPending.Time) < DoubleClickInterval) && (msg.wParam == ClickPending.wParam) && (msg.lParam == ClickPending.lParam) && (msg.message == ClickPending.Message))) {
						// Looks like a genuine double click, clicked twice on the same spot with the same keys
						switch(xevent.ButtonEvent.button) {
							case 1: {
								msg.message = client ? Msg.WM_LBUTTONDBLCLK : Msg.WM_NCLBUTTONDBLCLK;
								break;
							}

							case 2: {
								msg.message = client ? Msg.WM_MBUTTONDBLCLK : Msg.WM_NCMBUTTONDBLCLK;
								break;
							}

							case 3: {
								msg.message = client ? Msg.WM_RBUTTONDBLCLK : Msg.WM_NCRBUTTONDBLCLK;
								break;
							}
						}
						ClickPending.Pending = false;
					} else {
						ClickPending.Pending = true;
						ClickPending.Hwnd = msg.hwnd;
						ClickPending.Message = msg.message;
						ClickPending.wParam = msg.wParam;
						ClickPending.lParam = msg.lParam;
						ClickPending.Time = (long)xevent.ButtonEvent.time;
					}

					break;
				}

				case XEventName.ButtonRelease: {
					if (Dnd.InDrag()) {
						Dnd.HandleButtonRelease (ref xevent);
						break;
					}

					switch(xevent.ButtonEvent.button) {
						case 1: {
							MouseState &= ~MouseButtons.Left;
							if (client) {
								msg.message = Msg.WM_LBUTTONUP;
							} else {
								msg.message = Msg.WM_NCLBUTTONUP;
								ClientToScreen (msg.hwnd, ref xevent.ButtonEvent.x, ref xevent.ButtonEvent.y);
							}
							msg.wParam=GetMousewParam(0);
							break;
						}

						case 2: {
							MouseState &= ~MouseButtons.Middle;
							if (client) {
								msg.message = Msg.WM_MBUTTONUP;
							} else {
								msg.message = Msg.WM_NCMBUTTONUP;
								ClientToScreen (msg.hwnd, ref xevent.ButtonEvent.x, ref xevent.ButtonEvent.y);
							}
							msg.wParam=GetMousewParam(0);
							break;
						}

						case 3: {
							MouseState &= ~MouseButtons.Right;
							if (client) {
								msg.message = Msg.WM_RBUTTONUP;
							} else {
								msg.message = Msg.WM_NCRBUTTONUP;
								ClientToScreen (msg.hwnd, ref xevent.ButtonEvent.x, ref xevent.ButtonEvent.y);
							}
							msg.wParam=GetMousewParam(0);
							break;
						}

						case 4: {
							goto ProcessNextMessage;
						}

						case 5: {
							goto ProcessNextMessage;
						}
					}

					if (!hwnd.Enabled) {
						IntPtr dummy;

						msg.hwnd = hwnd.EnabledHwnd;
						XTranslateCoordinates(DisplayHandle, xevent.AnyEvent.window, Hwnd.ObjectFromHandle(msg.hwnd).ClientWindow, xevent.ButtonEvent.x, xevent.ButtonEvent.y, out xevent.ButtonEvent.x, out xevent.ButtonEvent.y, out dummy);
						msg.lParam = (IntPtr)(MousePosition.Y << 16 | MousePosition.X);
					}

					if (Grab.Hwnd != IntPtr.Zero) {
						msg.hwnd = Grab.Hwnd;
					}

					msg.lParam=(IntPtr) (xevent.ButtonEvent.y << 16 | xevent.ButtonEvent.x);
					MousePosition.X = xevent.ButtonEvent.x;
					MousePosition.Y = xevent.ButtonEvent.y;
					break;
				}

				case XEventName.MotionNotify: {
					if (client) {
						#if DriverDebugExtra
							Console.WriteLine("GetMessage(): Window {0:X} MotionNotify x={1} y={2}", client ? hwnd.client_window.ToInt32() : hwnd.whole_window.ToInt32(), xevent.MotionEvent.x, xevent.MotionEvent.y);
						#endif

						if (Dnd.HandleMotionNotify (ref xevent))
							goto ProcessNextMessage;
						if (Grab.Hwnd != IntPtr.Zero) {
							msg.hwnd = Grab.Hwnd;
						} else {
							NativeWindow.WndProc(msg.hwnd, Msg.WM_SETCURSOR, msg.hwnd, (IntPtr)HitTest.HTCLIENT);
						}

						msg.message = Msg.WM_MOUSEMOVE;
						msg.wParam = GetMousewParam(0);
						msg.lParam = (IntPtr) (xevent.MotionEvent.y << 16 | xevent.MotionEvent.x & 0xFFFF);

						if (!hwnd.Enabled) {
							IntPtr dummy;

							msg.hwnd = hwnd.EnabledHwnd;
							XTranslateCoordinates(DisplayHandle, xevent.AnyEvent.window, Hwnd.ObjectFromHandle(msg.hwnd).ClientWindow, xevent.MotionEvent.x, xevent.MotionEvent.y, out xevent.MotionEvent.x, out xevent.MotionEvent.y, out dummy);
							msg.lParam = (IntPtr)(MousePosition.Y << 16 | MousePosition.X);
						}

						MousePosition.X = xevent.MotionEvent.x;
						MousePosition.Y = xevent.MotionEvent.y;

						if ((HoverState.Timer.Enabled) &&
						    (((MousePosition.X + HoverState.Size.Width) < HoverState.X) ||
						    ((MousePosition.X - HoverState.Size.Width) > HoverState.X) ||
						    ((MousePosition.Y + HoverState.Size.Height) < HoverState.Y) ||
						    ((MousePosition.Y - HoverState.Size.Height) > HoverState.Y))) {
							HoverState.Timer.Stop();
							HoverState.Timer.Start();
							HoverState.X = MousePosition.X;
							HoverState.Y = MousePosition.Y;
						}

						break;
					} else {
						HitTest	ht;
						IntPtr dummy;
						int screen_x;
						int screen_y;

						#if DriverDebugExtra
							Console.WriteLine("GetMessage(): non-client area {0:X} MotionNotify x={1} y={2}", client ? hwnd.client_window.ToInt32() : hwnd.whole_window.ToInt32(), xevent.MotionEvent.x, xevent.MotionEvent.y);
						#endif
						msg.message = Msg.WM_NCMOUSEMOVE;

						if (!hwnd.Enabled) {
							msg.hwnd = hwnd.EnabledHwnd;
							XTranslateCoordinates(DisplayHandle, xevent.AnyEvent.window, Hwnd.ObjectFromHandle(msg.hwnd).ClientWindow, xevent.MotionEvent.x, xevent.MotionEvent.y, out xevent.MotionEvent.x, out xevent.MotionEvent.y, out dummy);
							msg.lParam = (IntPtr)(MousePosition.Y << 16 | MousePosition.X);
						}

						// The hit test is sent in screen coordinates
						XTranslateCoordinates (DisplayHandle, hwnd.client_window, RootWindow,
								xevent.MotionEvent.x, xevent.MotionEvent.y,
								out screen_x, out screen_y, out dummy);

						msg.lParam = (IntPtr) (screen_y << 16 | screen_x & 0xFFFF);
						ht = (HitTest)NativeWindow.WndProc (hwnd.client_window, Msg.WM_NCHITTEST,
								IntPtr.Zero, msg.lParam).ToInt32 ();
						NativeWindow.WndProc(hwnd.client_window, Msg.WM_SETCURSOR, msg.hwnd, (IntPtr)ht);

						MousePosition.X = xevent.MotionEvent.x;
						MousePosition.Y = xevent.MotionEvent.y;
					}

					break;
				}

				case XEventName.EnterNotify: {
					if (!hwnd.Enabled) {
						goto ProcessNextMessage;
					}
					if (xevent.CrossingEvent.mode != NotifyMode.NotifyNormal) {
						goto ProcessNextMessage;
					}
					msg.message = Msg.WM_MOUSE_ENTER;
					HoverState.X = xevent.CrossingEvent.x;
					HoverState.Y = xevent.CrossingEvent.y;
					HoverState.Timer.Enabled = true;
					HoverState.Window = xevent.CrossingEvent.window;
					break;
				}

				case XEventName.LeaveNotify: {
					if (!hwnd.Enabled) {
						goto ProcessNextMessage;
					}
					if ((xevent.CrossingEvent.mode != NotifyMode.NotifyNormal) || (xevent.CrossingEvent.window != hwnd.client_window)) {
						goto ProcessNextMessage;
					}
					msg.message=Msg.WM_MOUSE_LEAVE;
					HoverState.Timer.Enabled = false;
					HoverState.Window = IntPtr.Zero;
					break;
				}

				#if later
				case XEventName.CreateNotify: {
					if (client && (xevent.ConfigureEvent.xevent == xevent.ConfigureEvent.window)) {
						msg.message = WM_CREATE;
						// Set up CreateStruct
					} else {
						goto ProcessNextMessage;
					}
					break;
				}
				#endif


				case XEventName.ReparentNotify: {
					if (hwnd.parent == null) {	// Toplevel
						if ((xevent.ReparentEvent.parent != IntPtr.Zero) && (xevent.ReparentEvent.window == hwnd.whole_window)) {
							// We need to adjust x/y
							// This sucks ass, part 2
							// Every WM does the reparenting of toplevel windows different, so there's
							// no standard way of getting our adjustment considering frames/decorations
							// The code below is needed for metacity. KDE doesn't works just fine without this
							int	dummy_int;
							IntPtr	dummy_ptr;
							int	new_x;
							int	new_y;
							int	frame_left;
							int	frame_top;

							hwnd.Reparented = true;

							XGetGeometry(DisplayHandle, XGetParent(hwnd.whole_window), out dummy_ptr, out new_x, out new_y, out dummy_int, out dummy_int, out dummy_int, out dummy_int);
							FrameExtents(hwnd.whole_window, out frame_left, out frame_top);
							if ((frame_left != 0) && (frame_top != 0) && (new_x != frame_left) && (new_y != frame_top)) {
								hwnd.x = new_x;
								hwnd.y = new_y;
								hwnd.whacky_wm = true;
							}

							if (hwnd.opacity != 0xffffffff) {
								IntPtr opacity;

								opacity = (IntPtr)(Int32)hwnd.opacity;
								XChangeProperty(DisplayHandle, XGetParent(hwnd.whole_window), _NET_WM_WINDOW_OPACITY, (IntPtr)Atom.XA_CARDINAL, 32, PropertyMode.Replace, ref opacity, 1);
							}
							SendMessage(msg.hwnd, Msg.WM_WINDOWPOSCHANGED, msg.wParam, msg.lParam);
							goto ProcessNextMessage;
						} else {
							hwnd.Reparented = false;
							goto ProcessNextMessage;
						}
					}
					goto ProcessNextMessage;
				}

				case XEventName.ConfigureNotify: {
					if (ThreadQueue(Thread.CurrentThread).PostQuitState|| !client && (xevent.ConfigureEvent.xevent == xevent.ConfigureEvent.window)) {	// Ignore events for children (SubstructureNotify) and client areas
						#if DriverDebugExtra
							Console.WriteLine("GetMessage(): Window {0:X} ConfigureNotify x={1} y={2} width={3} height={4}", hwnd.client_window.ToInt32(), xevent.ConfigureEvent.x, xevent.ConfigureEvent.y, xevent.ConfigureEvent.width, xevent.ConfigureEvent.height);
						#endif
//						if ((hwnd.x != xevent.ConfigureEvent.x) || (hwnd.y != xevent.ConfigureEvent.y) || (hwnd.width != xevent.ConfigureEvent.width) || (hwnd.height != xevent.ConfigureEvent.height)) {
							SendMessage(msg.hwnd, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);
							hwnd.configure_pending = false;

							// We need to adjust our client window to track the resize of whole_window
							PerformNCCalc(hwnd);
//						}
					}
					goto ProcessNextMessage;
				}

				case XEventName.FocusIn: {
					// We received focus. We use X11 focus only to know if the app window does or does not have focus
					// We do not track the actual focussed window via it. Instead, this is done via FocusWindow internally
					// Receiving focus means we've gotten activated and therefore we need to let the actual FocusWindow know 
					// about it having focus again
					if (xevent.FocusChangeEvent.detail != NotifyDetail.NotifyNonlinear) {
						goto ProcessNextMessage;
					}

					if (FocusWindow == IntPtr.Zero) {
						Control c = Control.FromHandle (hwnd.client_window);
						if (c == null)
							goto ProcessNextMessage;
						Form form = c.FindForm ();
						if (form == null)
							goto ProcessNextMessage;
						ActiveWindow = form.Handle;
						SendMessage (ActiveWindow, Msg.WM_ACTIVATE, (IntPtr) WindowActiveFlags.WA_ACTIVE, IntPtr.Zero);
						goto ProcessNextMessage;
					}
					Keyboard.FocusIn(FocusWindow);
					SendMessage(FocusWindow, Msg.WM_SETFOCUS, IntPtr.Zero, IntPtr.Zero);
					goto ProcessNextMessage;
				}

				case XEventName.FocusOut: {
					// Se the comment for our FocusIn handler
					if (xevent.FocusChangeEvent.detail != NotifyDetail.NotifyNonlinear) {
						goto ProcessNextMessage;
					}
					Keyboard.FocusOut(FocusWindow);

					while (Keyboard.ResetKeyState(FocusWindow, ref msg)) {
						SendMessage(FocusWindow, msg.message, msg.wParam, msg.lParam);
					}

					SendMessage(FocusWindow, Msg.WM_KILLFOCUS, IntPtr.Zero, IntPtr.Zero);
					goto ProcessNextMessage;
				}

				case XEventName.Expose: {
					if (ThreadQueue(Thread.CurrentThread).PostQuitState || !hwnd.Mapped) {
						if (client) {
							hwnd.expose_pending = false;
						} else {
							hwnd.nc_expose_pending = false;
						}
						goto ProcessNextMessage;
					}

					if (client) {
						if (!hwnd.expose_pending) {
							goto ProcessNextMessage;
						}
					} else {
						if (!hwnd.nc_expose_pending) {
							goto ProcessNextMessage;
						}

						switch (hwnd.border_style) {
							case FormBorderStyle.Fixed3D: {
								Graphics g;

								g = Graphics.FromHwnd(hwnd.whole_window);
								ControlPaint.DrawBorder3D(g, new Rectangle(0, 0, hwnd.Width, hwnd.Height), Border3DStyle.Sunken);
								g.Dispose();
								break;
							}

							case FormBorderStyle.FixedSingle: {
								Graphics g;

								g = Graphics.FromHwnd(hwnd.whole_window);
								ControlPaint.DrawBorder(g, new Rectangle(0, 0, hwnd.Width, hwnd.Height), Color.Black, ButtonBorderStyle.Solid);
								g.Dispose();
								break;
							}
						}
						#if DriverDebugExtra
							Console.WriteLine("GetMessage(): Window {0:X} Exposed non-client area {1},{2} {3}x{4}", hwnd.client_window.ToInt32(), xevent.ExposeEvent.x, xevent.ExposeEvent.y, xevent.ExposeEvent.width, xevent.ExposeEvent.height);
						#endif

						Rectangle rect = new Rectangle (xevent.ExposeEvent.x, xevent.ExposeEvent.y, xevent.ExposeEvent.width, xevent.ExposeEvent.height);
						Region region = new Region (rect);
						IntPtr hrgn = region.GetHrgn (null); // Graphics object isn't needed
						msg.message = Msg.WM_NCPAINT;
						msg.wParam = hrgn == IntPtr.Zero ? (IntPtr)1 : hrgn;
						break;
					}
					#if DriverDebugExtra
						Console.WriteLine("GetMessage(): Window {0:X} Exposed area {1},{2} {3}x{4}", hwnd.client_window.ToInt32(), xevent.ExposeEvent.x, xevent.ExposeEvent.y, xevent.ExposeEvent.width, xevent.ExposeEvent.height);
					#endif
					if (Caret.Visible == true) {
						Caret.Paused = true;
						HideCaret();
					}

					if (Caret.Visible == true) {
						ShowCaret();
						Caret.Paused = false;
					}
					msg.message = Msg.WM_PAINT;
					break;
				}

				case XEventName.DestroyNotify: {

					// This is a bit tricky, we don't receive our own DestroyNotify, we only get those for our children
					hwnd = Hwnd.ObjectFromHandle(xevent.DestroyWindowEvent.window);

					// We may get multiple for the same window, act only one the first (when Hwnd still knows about it)
					if ((hwnd != null) && (hwnd.client_window == xevent.DestroyWindowEvent.window)) {
						CleanupCachedWindows (hwnd);

						#if DriverDebugDestroy
							Console.WriteLine("Received X11 Destroy Notification for {0}", XplatUI.Window(hwnd.client_window));
						#endif

						msg.hwnd = hwnd.client_window;
						msg.message=Msg.WM_DESTROY;
						hwnd.Dispose();

						#if DriverDebug
							Console.WriteLine("Got DestroyNotify on Window {0:X}", msg.hwnd.ToInt32());
						#endif
					} else {
						goto ProcessNextMessage;
					}

					break;
				}

				case XEventName.ClientMessage: {
					if (Dnd.HandleClientMessage (ref xevent)) {
						goto ProcessNextMessage;
					}

					if (xevent.ClientMessageEvent.message_type == AsyncAtom) {
						XplatUIDriverSupport.ExecuteClientMessage((GCHandle)xevent.ClientMessageEvent.ptr1);
						goto ProcessNextMessage;
					}

					if (xevent.ClientMessageEvent.message_type == HoverState.Atom) {
						msg.message = Msg.WM_MOUSEHOVER;
						msg.wParam = GetMousewParam(0);
						msg.lParam = (IntPtr) (xevent.ClientMessageEvent.ptr1);
						return true;
					}

					if (xevent.ClientMessageEvent.message_type == (IntPtr)PostAtom) {
						msg.hwnd = xevent.ClientMessageEvent.ptr1;
						msg.message = (Msg) xevent.ClientMessageEvent.ptr2.ToInt32 ();
						msg.wParam = xevent.ClientMessageEvent.ptr3;
						msg.lParam = xevent.ClientMessageEvent.ptr4;
						return true;
					}

					#if dontcare
					if  (xevent.ClientMessageEvent.message_type == _XEMBED) {
						Console.WriteLine("GOT EMBED MESSAGE {0:X}", xevent.ClientMessageEvent.ptr2.ToInt32());
						break;
					}
					#endif

					if  (xevent.ClientMessageEvent.message_type == WM_PROTOCOLS) {
						if (xevent.ClientMessageEvent.ptr1 == WM_DELETE_WINDOW) {
							msg.message = Msg.WM_CLOSE;
							return true;
						}

						// We should not get this, but I'll leave the code in case we need it in the future
						if (xevent.ClientMessageEvent.ptr1 == WM_TAKE_FOCUS) {
							goto ProcessNextMessage;
						}
					}
					goto ProcessNextMessage;
				}

				case XEventName.TimerNotify: {
					xevent.TimerNotifyEvent.handler (this, EventArgs.Empty);
					goto ProcessNextMessage;
				}
		                        
				default: {
					goto ProcessNextMessage;
				}
			}

			return true;
		}

		internal override bool GetText(IntPtr handle, out string text) {
			IntPtr	textptr;

			textptr = IntPtr.Zero;

			lock (XlibLock) {
				// FIXME - use _NET properties
				XFetchName(DisplayHandle, Hwnd.ObjectFromHandle(handle).whole_window, ref textptr);
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

		internal override void GetWindowPos(IntPtr handle, bool is_toplevel, out int x, out int y, out int width, out int height, out int client_width, out int client_height) {
			Hwnd		hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (hwnd != null) {
				x = hwnd.x;
				y = hwnd.y;
				width = hwnd.width;
				height = hwnd.height;

				PerformNCCalc(hwnd);

				client_width = hwnd.ClientRect.Width;
				client_height = hwnd.ClientRect.Height;

				return;
			}

			// Should we throw an exception or fail silently?
			// throw new ArgumentException("Called with an invalid window handle", "handle");

			x = 0;
			y = 0;
			width = 0;
			height = 0;
			client_width = 0;
			client_height = 0;
		}

		internal override FormWindowState GetWindowState(IntPtr handle) {
			IntPtr			actual_atom;
			int			actual_format;
			IntPtr			nitems;
			IntPtr			bytes_after;
			IntPtr			prop = IntPtr.Zero;
			IntPtr			atom;
			int			maximized;
			bool			minimized;
			XWindowAttributes	attributes;
			Hwnd			hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			maximized = 0;
			minimized = false;
			XGetWindowProperty(DisplayHandle, hwnd.whole_window, _NET_WM_STATE, IntPtr.Zero, new IntPtr (256), false, (IntPtr)Atom.XA_ATOM, out actual_atom, out actual_format, out nitems, out bytes_after, ref prop);
			if (((long)nitems > 0) && (prop != IntPtr.Zero)) {
				for (int i = 0; i < (long)nitems; i++) {
					atom = (IntPtr)Marshal.ReadInt32(prop, i * 4);
					if ((atom == _NET_WM_STATE_MAXIMIZED_HORZ) || (atom == _NET_WM_STATE_MAXIMIZED_VERT)) {
						maximized++;
					} else if (atom == _NET_WM_STATE_HIDDEN) {
						minimized = true;
					}
				}
				XFree(prop);
			}

			if (minimized) {
				return FormWindowState.Minimized;
			} else if (maximized == 2) {
				return FormWindowState.Maximized;
			}

			attributes = new XWindowAttributes();
			XGetWindowAttributes(DisplayHandle, handle, ref attributes);
			if (attributes.map_state == MapState.IsUnmapped) {
				throw new NotSupportedException("Cannot retrieve the state of an unmapped window");
			}


			return FormWindowState.Normal;
		}

		internal override void GrabInfo(out IntPtr handle, out bool GrabConfined, out Rectangle GrabArea) {
			handle = Grab.Hwnd;
			GrabConfined = Grab.Confined;
			GrabArea = Grab.Area;
		}

		internal override void GrabWindow(IntPtr handle, IntPtr confine_to_handle) {
			Hwnd	hwnd;
			IntPtr	confine_to_window;

			confine_to_window = IntPtr.Zero;

			if (confine_to_handle != IntPtr.Zero) {
				XWindowAttributes	attributes = new XWindowAttributes();

				hwnd = Hwnd.ObjectFromHandle(confine_to_handle);

				lock (XlibLock) {
					XGetWindowAttributes(DisplayHandle, hwnd.client_window, ref attributes);
				}
				Grab.Area.X = attributes.x;
				Grab.Area.Y = attributes.y;
				Grab.Area.Width = attributes.width;
				Grab.Area.Height = attributes.height;
				Grab.Confined = true;
				confine_to_window = hwnd.client_window;
			}

			Grab.Hwnd = handle;

			hwnd = Hwnd.ObjectFromHandle(handle);

			lock (XlibLock) {
				XGrabPointer(DisplayHandle, hwnd.client_window, false, 
					EventMask.ButtonPressMask | EventMask.ButtonMotionMask |
					EventMask.ButtonReleaseMask | EventMask.PointerMotionMask,
					GrabMode.GrabModeAsync, GrabMode.GrabModeAsync, confine_to_window, IntPtr.Zero, IntPtr.Zero);
			}
		}

		internal override void UngrabWindow(IntPtr hwnd) {
			lock (XlibLock) {
				XUngrabPointer(DisplayHandle, IntPtr.Zero);
				XFlush(DisplayHandle);
			}
			Grab.Hwnd = IntPtr.Zero;
			Grab.Confined = false;
		}

		internal override void HandleException(Exception e) {
			StackTrace st = new StackTrace(e, true);
			Console.WriteLine("Exception '{0}'", e.Message+st.ToString());
			Console.WriteLine("{0}{1}", e.Message, st.ToString());
		}

		internal override void Invalidate(IntPtr handle, Rectangle rc, bool clear) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (clear) {
				AddExpose (hwnd, true, hwnd.X, hwnd.Y, hwnd.Width, hwnd.Height);
			} else {
				AddExpose (hwnd, true, rc.X, rc.Y, rc.Width, rc.Height);
			}
		}

		internal override bool IsEnabled(IntPtr handle) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			return (hwnd != null && hwnd.Enabled);
		}
		
		internal override bool IsVisible(IntPtr handle) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			return (hwnd != null && hwnd.visible);
		}

		internal override void KillTimer(Timer timer) {
			XEventQueue queue = (XEventQueue) MessageQueues [timer.thread];

			if (queue == null) {
				// This isn't really an error, MS doesn't start the timer if
				// it has no assosciated queue
				return;
			}
			queue.timer_list.Remove (timer);
		}

		internal override void MenuToScreen(IntPtr handle, ref int x, ref int y) {
			int	dest_x_return;
			int	dest_y_return;
			IntPtr	child;
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			lock (XlibLock) {
				XTranslateCoordinates(DisplayHandle, hwnd.whole_window, RootWindow, x, y, out dest_x_return, out dest_y_return, out child);
			}

			x = dest_x_return;
			y = dest_y_return;
		}

		internal override void OverrideCursor(IntPtr cursor) {
			OverrideCursorHandle = cursor;
		}

		internal override PaintEventArgs PaintEventStart(IntPtr handle, bool client) {
			PaintEventArgs	paint_event;
			Hwnd		hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (Caret.Visible == true) {
				Caret.Paused = true;
				HideCaret();
			}
			
			if (client) {
#if true
				hwnd.client_dc = Graphics.FromHwnd (hwnd.client_window);
#else
				// Protected against illegal cross-thread painting
				lock (XlibLock) {
					if (hwnd.client_dc != null) {
						return null;
					}

					hwnd.client_dc = Graphics.FromHwnd (hwnd.client_window);
				}
#endif

				Region clip_region = new Region ();
				clip_region.MakeEmpty();

				foreach (Rectangle r in hwnd.ClipRectangles) {
					clip_region.Union (r);
				}

				if (hwnd.UserClip != null) {
					clip_region.Intersect(hwnd.UserClip);
				}

				hwnd.client_dc.Clip = clip_region;
				paint_event = new PaintEventArgs(hwnd.client_dc, hwnd.Invalid);
				hwnd.expose_pending = false;

				hwnd.ClearInvalidArea();

				return paint_event;
			} else {
				hwnd.non_client_dc = Graphics.FromHwnd (hwnd.whole_window);

				if (!hwnd.nc_invalid.IsEmpty) {
					hwnd.non_client_dc.SetClip (hwnd.nc_invalid);
					paint_event = new PaintEventArgs(hwnd.non_client_dc, hwnd.nc_invalid);
				} else {
					paint_event = new PaintEventArgs(hwnd.non_client_dc, new Rectangle(0, 0, hwnd.width, hwnd.height));
				}
				hwnd.nc_expose_pending = false;

				hwnd.ClearNcInvalidArea ();

				return paint_event;
			}
		}

		internal override void PaintEventEnd(IntPtr handle, bool client) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (client) {
#if true
				hwnd.client_dc.Flush();
				hwnd.client_dc.Dispose();
				hwnd.client_dc = null;
#else
				lock (XlibLock) {
					hwnd.client_dc.Flush();
					hwnd.client_dc.Dispose();
					hwnd.client_dc = null;
				}
#endif
			} else {
				hwnd.non_client_dc.Flush ();
				hwnd.non_client_dc.Dispose ();
				hwnd.non_client_dc = null;
			}


			if (Caret.Visible == true) {
				ShowCaret();
				Caret.Paused = false;
			}
		}

		[MonoTODO("Implement filtering and PM_NOREMOVE")]
		internal override bool PeekMessage(Object queue_id, ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags) {
			XEventQueue queue = (XEventQueue) queue_id;
			bool	pending;

			if ((flags & (uint)PeekMessageFlags.PM_REMOVE) == 0) {
				throw new NotImplementedException("PeekMessage PM_NOREMOVE is not implemented yet");	// FIXME - Implement PM_NOREMOVE flag
			}

			pending = false;
			if (queue.Count > 0) {
				pending = true;
			} else {
				// Only call UpdateMessageQueue if real events are pending 
				// otherwise we go to sleep on the socket
				if (XPending(DisplayHandle) != 0) {
					UpdateMessageQueue((XEventQueue)queue_id);
					pending = true;
				} else if (((XEventQueue)queue_id).Paint.Count > 0) {
					pending = true;
				}
			}

			CheckTimers(queue.timer_list, DateTime.UtcNow);

			if (!pending) {
				return false;
			}
			return GetMessage(queue_id, ref msg, hWnd, wFilterMin, wFilterMax);
		}

		// FIXME - I think this should just enqueue directly
		internal override bool PostMessage (IntPtr handle, Msg message, IntPtr wparam, IntPtr lparam) {
			XEvent xevent = new XEvent ();
			Hwnd hwnd = Hwnd.ObjectFromHandle(handle);

			xevent.type = XEventName.ClientMessage;
			xevent.ClientMessageEvent.display = DisplayHandle;

			if (hwnd != null) {
				xevent.ClientMessageEvent.window = hwnd.whole_window;
			} else {
				xevent.ClientMessageEvent.window = IntPtr.Zero;
			}

			xevent.ClientMessageEvent.message_type = (IntPtr) PostAtom;
			xevent.ClientMessageEvent.format = 32;
			xevent.ClientMessageEvent.ptr1 = handle;
			xevent.ClientMessageEvent.ptr2 = (IntPtr) message;
			xevent.ClientMessageEvent.ptr3 = wparam;
			xevent.ClientMessageEvent.ptr4 = lparam;

			hwnd.Queue.Enqueue (xevent);

			return true;
		}

		internal override void PostQuitMessage(int exitCode) {
			
			XFlush(DisplayHandle);
			ThreadQueue(Thread.CurrentThread).PostQuitState = true;
		}

		internal override void RequestNCRecalc(IntPtr handle) {
			Hwnd				hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (hwnd == null) {
				return;
			}

			PerformNCCalc(hwnd);
			SendMessage(handle, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);
			InvalidateWholeWindow(handle);
		}

		internal override void ResetMouseHover(IntPtr handle) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			if (hwnd == null) {
				return;
			}

			HoverState.Timer.Enabled = true;
			HoverState.X = MousePosition.X;
			HoverState.Y = MousePosition.Y;
			HoverState.Window = handle;
		}


		internal override void ScreenToClient(IntPtr handle, ref int x, ref int y) {
			int	dest_x_return;
			int	dest_y_return;
			IntPtr	child;
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			lock (XlibLock) {
				XTranslateCoordinates (DisplayHandle, RootWindow, hwnd.client_window, x, y, out dest_x_return, out dest_y_return, out child);
			}

			x = dest_x_return;
			y = dest_y_return;
		}

		internal override void ScreenToMenu(IntPtr handle, ref int x, ref int y) {
			int	dest_x_return;
			int	dest_y_return;
			IntPtr	child;
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			lock (XlibLock) {
				XTranslateCoordinates (DisplayHandle, RootWindow, hwnd.whole_window, x, y, out dest_x_return, out dest_y_return, out child);
			}

			x = dest_x_return;
			y = dest_y_return;
		}

		internal override void ScrollWindow(IntPtr handle, Rectangle area, int XAmount, int YAmount, bool with_children) {
			Hwnd		hwnd;
			IntPtr		gc;
			XGCValues	gc_values;
			Rectangle	r;

			hwnd = Hwnd.ObjectFromHandle(handle);

			r = hwnd.Invalid;
			if (r != Rectangle.Empty) {
				/* We have an invalid area in the window we're scrolling. 
				   Adjust our stored invalid rectangle to to match the scrolled amount */

				r.X += XAmount;
				r.Y += YAmount;

				if (r.X < 0) {
					r.Width += r.X;
					r.X =0;
				}

				if (r.Y < 0) {
					r.Height += r.Y;
					r.Y =0;
				}

				hwnd.ClearInvalidArea();
				hwnd.AddInvalidArea(r);
			}

			gc_values = new XGCValues();

			if (with_children) {
				gc_values.subwindow_mode = GCSubwindowMode.IncludeInferiors;
			}

			gc = XCreateGC(DisplayHandle, hwnd.client_window, IntPtr.Zero, ref gc_values);

			int src_x, src_y;
			int dest_x, dest_y;
			int width, height;

			if (YAmount > 0) {
				src_y = area.Y;
				height = area.Height - YAmount;
				dest_y = area.Y + YAmount;
			}
			else {
				src_y = area.Y - YAmount;
				height = area.Height + YAmount;
				dest_y = area.Y;
			}

			if (XAmount > 0) {
				src_x = area.X;
				width = area.Width - XAmount;
				dest_x = area.X + XAmount;
			}
			else {
				src_x = area.X - XAmount;
				width = area.Width + XAmount;
				dest_x = area.X;
			}

			XCopyArea(DisplayHandle, hwnd.client_window, hwnd.client_window, gc, src_x, src_y, width, height, dest_x, dest_y);

			// Generate an expose for the area exposed by the horizontal scroll
			// We don't use AddExpose since we're 
			if (XAmount > 0) {
				AddExpose(hwnd, true, area.X, area.Y, XAmount, area.Height);
			} else if (XAmount < 0) {
				AddExpose(hwnd, true, XAmount + area.X + area.Width, area.Y, -XAmount, area.Height);
			}

			// Generate an expose for the area exposed by the vertical scroll
			if (YAmount > 0) {
				AddExpose(hwnd, true, area.X, area.Y, area.Width, YAmount);
			} else if (YAmount < 0) {
				AddExpose(hwnd, true, area.X, YAmount + area.Y + area.Height, area.Width, -YAmount);
			}
			XFreeGC(DisplayHandle, gc);
		}

		internal override void ScrollWindow(IntPtr handle, int XAmount, int YAmount, bool with_children) {
			Hwnd		hwnd;
			Rectangle	rect;

			hwnd = Hwnd.GetObjectFromWindow(handle);

			rect = hwnd.ClientRect;
			rect.X = 0;
			rect.Y = 0;
			ScrollWindow(handle, rect, XAmount, YAmount, with_children);
		}

		internal override void SendAsyncMethod (AsyncMethodData method) {
			Hwnd	hwnd;
			XEvent	xevent = new XEvent ();

			hwnd = Hwnd.ObjectFromHandle(method.Handle);

			xevent.type = XEventName.ClientMessage;
			xevent.ClientMessageEvent.display = DisplayHandle;
			xevent.ClientMessageEvent.window = method.Handle;
			xevent.ClientMessageEvent.message_type = (IntPtr)AsyncAtom;
			xevent.ClientMessageEvent.format = 32;
			xevent.ClientMessageEvent.ptr1 = (IntPtr) GCHandle.Alloc (method);

			hwnd.Queue.EnqueueLocked (xevent);

			WakeupMain ();
		}

		delegate IntPtr WndProcDelegate (IntPtr hwnd, Msg message, IntPtr wParam, IntPtr lParam);

		internal override IntPtr SendMessage (IntPtr hwnd, Msg message, IntPtr wParam, IntPtr lParam)
		{
			Hwnd	h;
			h = Hwnd.ObjectFromHandle(hwnd);

			if (h.queue != ThreadQueue (Thread.CurrentThread)) {
				AsyncMethodResult	result;
				AsyncMethodData		data;

				result = new AsyncMethodResult ();
				data = new AsyncMethodData ();

				data.Handle = hwnd;
				data.Method = new WndProcDelegate (NativeWindow.WndProc);
				data.Args = new object[] { hwnd, message, wParam, lParam };
				data.Result = result;
				
				SendAsyncMethod (data);
				#if DriverDebug || DriverDebugParent
				Console.WriteLine ("Sending {0} message across.", message);
				#endif

				return IntPtr.Zero;
			}
			return NativeWindow.WndProc(hwnd, message, wParam, lParam);
		}

		internal override void SetAllowDrop (IntPtr handle, bool value)
		{
			// We allow drop on all windows
		}

		internal override DragDropEffects StartDrag (IntPtr handle, object data,
				DragDropEffects allowed_effects)
		{
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);

			if (hwnd == null)
				throw new ArgumentException ("Attempt to begin drag from invalid window handle (" + handle.ToInt32 () + ").");

			return Dnd.StartDrag (hwnd.client_window, data, allowed_effects);
		}

		internal override void SetBorderStyle(IntPtr handle, FormBorderStyle border_style) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			Form form = Control.FromHandle (handle) as Form;
			if (form != null && border_style == FormBorderStyle.FixedToolWindow ||
					border_style == FormBorderStyle.SizableToolWindow) {
				form.window_manager = new InternalWindowManager (form);
			}
			
			hwnd.border_style = border_style;
			RequestNCRecalc(handle);
		}

		internal override void SetCaretPos(IntPtr handle, int x, int y) {
			if (Caret.Hwnd == handle) {
				Caret.Timer.Stop();
				HideCaret();

				Caret.X = x;
				Caret.Y = y;

				if (Caret.Visible == true) {
					ShowCaret();
					Caret.Timer.Start();
				}
			}
		}

		internal override void SetClipRegion(IntPtr handle, Region region) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			if (hwnd == null) {
				return;
			}

			hwnd.UserClip = region;
			Invalidate(handle, new Rectangle(0, 0, hwnd.Width, hwnd.Height), false);
		}

		internal override void SetCursor(IntPtr handle, IntPtr cursor) {
			Hwnd	hwnd;

			if (OverrideCursorHandle == IntPtr.Zero) {
				if ((LastCursorWindow == handle) && (LastCursorHandle == cursor)) {
					return;
				}

				LastCursorHandle = cursor;
				LastCursorWindow = handle;

				hwnd = Hwnd.ObjectFromHandle(handle);
				lock (XlibLock) {
					if (cursor != IntPtr.Zero) {
						XDefineCursor(DisplayHandle, hwnd.whole_window, cursor);
					} else {
						XUndefineCursor(DisplayHandle, hwnd.whole_window);
					}
					XFlush(DisplayHandle);
				}
				return;
			}

			hwnd = Hwnd.ObjectFromHandle(handle);
			lock (XlibLock) {
				XDefineCursor(DisplayHandle, hwnd.whole_window, OverrideCursorHandle);
			}
		}

		internal override void SetCursorPos(IntPtr handle, int x, int y) {
			if (handle == IntPtr.Zero) {
				int cx, cy;
				GetCursorPos (handle, out cx, out cy);
				lock (XlibLock) {
					XWarpPointer(DisplayHandle, IntPtr.Zero, IntPtr.Zero, 0, 0, 0, 0, x - cx, y - cy);
				}
				return;
			} else {
				Hwnd	hwnd;

				hwnd = Hwnd.ObjectFromHandle(handle);
				lock (XlibLock) {
					XWarpPointer(DisplayHandle, IntPtr.Zero, hwnd.client_window, 0, 0, 0, 0, x, y);
				}
				return;
			}
		}

		internal override void SetFocus(IntPtr handle) {
			Hwnd	hwnd;
			IntPtr	prev_focus_window;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (hwnd.client_window == FocusWindow) {
				return;
			}

			prev_focus_window = FocusWindow;
			FocusWindow = hwnd.client_window;

			if (prev_focus_window != IntPtr.Zero) {
				SendMessage(prev_focus_window, Msg.WM_KILLFOCUS, FocusWindow, IntPtr.Zero);
			}
			SendMessage(FocusWindow, Msg.WM_SETFOCUS, prev_focus_window, IntPtr.Zero);

			//XSetInputFocus(DisplayHandle, Hwnd.ObjectFromHandle(handle).client_window, RevertTo.None, IntPtr.Zero);
		}

		internal override void SetIcon(IntPtr handle, Icon icon) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			if (hwnd != null) {
				SetIcon(hwnd, icon);
			}
		}

		internal override void SetMenu(IntPtr handle, Menu menu) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			hwnd.menu = menu;

			RequestNCRecalc(handle);
		}

		internal override void SetModal(IntPtr handle, bool Modal) {
			if (Modal) {
				ModalWindows.Push(handle);
			} else {
				if (ModalWindows.Contains(handle)) {
					ModalWindows.Pop();
				}
				if (ModalWindows.Count > 0) {
					Activate((IntPtr)ModalWindows.Peek());
				}
			}
		}

		internal override IntPtr SetParent(IntPtr handle, IntPtr parent) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			hwnd.parent = Hwnd.ObjectFromHandle(parent);

			lock (XlibLock) {
				#if DriverDebug || DriverDebugParent
					Console.WriteLine("Parent for window {0} = {1}", XplatUI.Window(hwnd.Handle), XplatUI.Window(hwnd.parent != null ? hwnd.parent.Handle : IntPtr.Zero));
				#endif
				XReparentWindow(DisplayHandle, hwnd.whole_window, hwnd.parent == null ? FosterParent : hwnd.parent.client_window, hwnd.x, hwnd.y);
			}

			return IntPtr.Zero;
		}

		internal override void SetTimer (Timer timer) {
			XEventQueue queue = (XEventQueue) MessageQueues [timer.thread];

			if (queue == null) {
				// This isn't really an error, MS doesn't start the timer if
				// it has no assosciated queue
				return;
			}
			queue.timer_list.Add (timer);
			WakeupMain ();
		}

		internal override bool SetTopmost(IntPtr handle, IntPtr handle_owner, bool enabled) {
			Hwnd	hwnd;
			Hwnd	hwnd_owner;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (handle_owner != IntPtr.Zero) {
				hwnd_owner = Hwnd.ObjectFromHandle(handle_owner);
			} else {
				hwnd_owner = null;
			}

			if (enabled) {
				lock (XlibLock) {
					int[]	atoms;

					atoms = new int[8];

					atoms[0] = _NET_WM_WINDOW_TYPE_NORMAL.ToInt32();
					XChangeProperty(DisplayHandle, hwnd.whole_window, _NET_WM_WINDOW_TYPE, (IntPtr)Atom.XA_ATOM, 32, PropertyMode.Replace, atoms, 1);

					if (hwnd_owner != null) {
						XSetTransientForHint(DisplayHandle, hwnd.whole_window, hwnd_owner.whole_window);
					} else {
						XSetTransientForHint(DisplayHandle, hwnd.whole_window, RootWindow);
					}
				}
			} else {
				lock (XlibLock) {
					XDeleteProperty(DisplayHandle, hwnd.whole_window, (IntPtr)Atom.XA_WM_TRANSIENT_FOR);
				}
			}
			return true;
		}

		internal override bool SetVisible(IntPtr handle, bool visible) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			hwnd.visible = visible;

			lock (XlibLock) {
				if (visible) {
					if (Control.FromHandle(handle) is Form) {
						FormWindowState	s;

						s = ((Form)Control.FromHandle(handle)).WindowState;

						MapWindow(hwnd, WindowType.Both);

						switch(s) {
							case FormWindowState.Minimized:	SetWindowState(handle, FormWindowState.Minimized); break;
							case FormWindowState.Maximized:	SetWindowState(handle, FormWindowState.Maximized); break;
						}

					} else {
						MapWindow(hwnd, WindowType.Both);
					}
					SendMessage(handle, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);
				} else {
					UnmapWindow(hwnd, WindowType.Whole);
				}
			}
			return true;
		}

		internal override void SetWindowMinMax(IntPtr handle, Rectangle maximized, Size min, Size max) {
			Hwnd		hwnd;
			XSizeHints	hints;
			IntPtr		dummy;

			hwnd = Hwnd.ObjectFromHandle(handle);
			if (hwnd == null) {
				return;
			}

			hints = new XSizeHints();

			XGetWMNormalHints(DisplayHandle, hwnd.whole_window, ref hints, out dummy);
			if ((min != Size.Empty) && (min.Width > 0) && (min.Height > 0)) {
				hints.flags = (IntPtr)((int)hints.flags | (int)XSizeHintsFlags.PMinSize);
				hints.min_width = min.Width;
				hints.min_height = min.Height;
			}

			if ((max != Size.Empty) && (max.Width > 0) && (max.Height > 0)) {
				hints.flags = (IntPtr)((int)hints.flags | (int)XSizeHintsFlags.PMaxSize);
				hints.max_width = max.Width;
				hints.max_height = max.Height;
			}

			if (hints.flags != IntPtr.Zero) {
				XSetWMNormalHints(DisplayHandle, hwnd.whole_window, ref hints);
			}

			if ((maximized != Rectangle.Empty) && (maximized.Width > 0) && (maximized.Height > 0)) {
				hints.flags = (IntPtr)XSizeHintsFlags.PPosition;
				hints.x = maximized.X;
				hints.y = maximized.Y;
				hints.width = maximized.Width;
				hints.height = maximized.Height;

				// Metacity does not seem to follow this constraint for maximized (zoomed) windows
				XSetZoomHints(DisplayHandle, hwnd.whole_window, ref hints);
			}
		}


		internal override void SetWindowPos(IntPtr handle, int x, int y, int width, int height) {
			Hwnd		hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (hwnd == null) {
				return;
			}

			// X requires a sanity check for width & height; otherwise it dies
			if (hwnd.zero_sized && width > 0 && height > 0) {
				if (hwnd.visible) {
					MapWindow(hwnd, WindowType.Whole);
				}
				hwnd.zero_sized = false;
			}

			if ((width < 1) || (height < 1)) {
				hwnd.zero_sized = true;
				UnmapWindow(hwnd, WindowType.Whole);
			}

			// Save a server roundtrip (and prevent a feedback loop)
			if ((hwnd.x == x) && (hwnd.y == y) && 
				(hwnd.width == width) && (hwnd.height == height)) {
				return;
			}

			if (!hwnd.zero_sized) {
				//Hack?
				hwnd.x = x;
				hwnd.y = y;
				hwnd.width = width;
				hwnd.height = height;
				SendMessage(hwnd.client_window, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);

				if (hwnd.fixed_size) {
					SetWindowMinMax(handle, Rectangle.Empty, new Size(width, height), new Size(width, height));
				}

				lock (XlibLock) {
					XMoveResizeWindow(DisplayHandle, hwnd.whole_window, x, y, width, height);
					PerformNCCalc(hwnd);
				}
			}

			// Prevent an old queued ConfigureNotify from setting our width with outdated data, set it now
			hwnd.width = width;
			hwnd.height = height;
		}

		internal override void SetWindowState(IntPtr handle, FormWindowState state) {
			FormWindowState	current_state;
			Hwnd		hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			try {
				current_state = GetWindowState(handle);
			}
			catch (NotSupportedException) {
				current_state = (FormWindowState)(-1);
			}

			if (current_state == state) {
				return;
			}

			switch(state) {
				case FormWindowState.Normal: {
					lock (XlibLock) {
						if (current_state == FormWindowState.Minimized) {
							MapWindow(hwnd, WindowType.Both);
						} else if (current_state == FormWindowState.Maximized) {
							SendNetWMMessage(hwnd.whole_window, _NET_WM_STATE, (IntPtr)2 /* toggle */, _NET_WM_STATE_MAXIMIZED_HORZ, _NET_WM_STATE_MAXIMIZED_VERT);
						}
					}
					Activate(handle);
					return;
				}

				case FormWindowState.Minimized: {
					lock (XlibLock) {
						if (current_state == FormWindowState.Maximized) {
							SendNetWMMessage(hwnd.whole_window, _NET_WM_STATE, (IntPtr)2 /* toggle */, _NET_WM_STATE_MAXIMIZED_HORZ, _NET_WM_STATE_MAXIMIZED_VERT);
						}
						XIconifyWindow(DisplayHandle, hwnd.whole_window, ScreenNo);
					}
					return;
				}

				case FormWindowState.Maximized: {
					lock (XlibLock) {
						if (current_state == FormWindowState.Minimized) {
							MapWindow(hwnd, WindowType.Both);
						}

						SendNetWMMessage(hwnd.whole_window, _NET_WM_STATE, (IntPtr)1 /* Add */, _NET_WM_STATE_MAXIMIZED_HORZ, _NET_WM_STATE_MAXIMIZED_VERT);
					}
					Activate(handle);
					return;
				}
			}
		}

		internal override void SetWindowStyle(IntPtr handle, CreateParams cp) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			SetHwndStyles(hwnd, cp);
			SetWMStyles(hwnd, cp);
		}

		internal override void SetWindowTransparency(IntPtr handle, double transparency, Color key) {
			Hwnd	hwnd;
			IntPtr	opacity;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (hwnd == null) {
				return;
			}

			hwnd.opacity = (uint)(0xffffffff * transparency);
			opacity = (IntPtr)((int)hwnd.opacity);

			if (hwnd.reparented) {
				XChangeProperty(DisplayHandle, XGetParent(hwnd.whole_window), _NET_WM_WINDOW_OPACITY, (IntPtr)Atom.XA_CARDINAL, 32, PropertyMode.Replace, ref opacity, 1);
			}
		}

		internal override bool SetZOrder(IntPtr handle, IntPtr after_handle, bool top, bool bottom) {
			Hwnd	hwnd = Hwnd.ObjectFromHandle(handle);

			if (!hwnd.mapped) {
				return false;
			}

			if (top) {
				lock (XlibLock) {
					XRaiseWindow(DisplayHandle, hwnd.whole_window);
				}
				return true;
			} else if (!bottom) {
				Hwnd	after_hwnd = null;

				if (after_handle != IntPtr.Zero) {
					after_hwnd = Hwnd.ObjectFromHandle(after_handle);
				}

				XWindowChanges	values = new XWindowChanges();

				if (after_hwnd == null) {
					// Work around metacity 'issues'
					int[]	atoms;

					atoms = new int[2];
					atoms[0] = unixtime();
					XChangeProperty(DisplayHandle, hwnd.whole_window, _NET_WM_USER_TIME, (IntPtr)Atom.XA_CARDINAL, 32, PropertyMode.Replace, atoms, 1);

					XRaiseWindow(DisplayHandle, hwnd.whole_window);
					SendNetWMMessage(hwnd.whole_window, _NET_ACTIVE_WINDOW, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
					return true;
					//throw new ArgumentNullException("after_handle", "Need sibling to adjust z-order");
				}

				values.sibling = after_hwnd.whole_window;
				values.stack_mode = StackMode.Below;

				lock (XlibLock) {
					XConfigureWindow(DisplayHandle, hwnd.whole_window, ChangeWindowFlags.CWStackMode | ChangeWindowFlags.CWSibling, ref values);
				}
			} else {
				// Bottom
				lock (XlibLock) {
					XLowerWindow(DisplayHandle, hwnd.whole_window);
				}
				return true;
			}
			return false;
		}

		internal override void ShowCursor(bool show) {
			;	// FIXME - X11 doesn't 'hide' the cursor. we could create an empty cursor
		}

		internal override object StartLoop(Thread thread) {
			return (Object) ThreadQueue(thread);
		}

		internal override bool SupportsTransparency() {
			// We need to check if the x compositing manager is running
			return true;
		}

		internal override bool SystrayAdd(IntPtr handle, string tip, Icon icon, out ToolTip tt) {
			GetSystrayManagerWindow();

			if (SystrayMgrWindow != IntPtr.Zero) {
				XSizeHints	size_hints;
				IntPtr		dummy;
				Hwnd		hwnd;

				hwnd = Hwnd.ObjectFromHandle(handle);
				#if DriverDebug
					Console.WriteLine("Adding Systray Whole:{0:X}, Client:{1:X}", hwnd.whole_window.ToInt32(), hwnd.client_window.ToInt32());
				#endif

				UnmapWindow(hwnd, WindowType.Whole);

				// Oh boy.
				XDestroyWindow(DisplayHandle, hwnd.client_window);
				hwnd.client_window = hwnd.whole_window;

				size_hints = new XSizeHints();

				XGetWMNormalHints(DisplayHandle, hwnd.whole_window, ref size_hints, out dummy);
				size_hints.flags = (IntPtr)(XSizeHintsFlags.PMinSize | XSizeHintsFlags.PMaxSize | XSizeHintsFlags.PBaseSize);
				size_hints.min_width = icon.Width;
				size_hints.min_height = icon.Height;

				size_hints.max_width = icon.Width;
				size_hints.max_height = icon.Height;

				size_hints.base_width = icon.Width;
				size_hints.base_height = icon.Height;
				XSetWMNormalHints(DisplayHandle, hwnd.whole_window, ref size_hints);

				int[] atoms = new int[2];
				atoms [0] = 1;			// Version 1
				atoms [1] = 0;			// We're not mapped

				// This line cost me 3 days...
				XChangeProperty(DisplayHandle, hwnd.whole_window, _XEMBED_INFO, _XEMBED_INFO, 32, PropertyMode.Replace, atoms, 2);

				// Need to pick some reasonable defaults
				tt = new ToolTip();
				tt.AutomaticDelay = 100;
				tt.InitialDelay = 250;
				tt.ReshowDelay = 250;
				tt.ShowAlways = true;

				if ((tip != null) && (tip != string.Empty)) {
					tt.SetToolTip(Control.FromHandle(handle), tip);
					tt.Active = true;
				} else {
					tt.Active = false;
				}

				// Make sure the window exists
				XSync(DisplayHandle, hwnd.whole_window);

				SendNetClientMessage(SystrayMgrWindow, _NET_SYSTEM_TRAY_OPCODE, IntPtr.Zero, (IntPtr)SystrayRequest.SYSTEM_TRAY_REQUEST_DOCK, hwnd.whole_window);
				return true;
			}
			tt = null;
			return false;
		}

		internal override bool SystrayChange(IntPtr handle, string tip, Icon icon, ref ToolTip tt) {
			Control	control;

			control = Control.FromHandle(handle);
			if (control != null && tt != null) {
				tt.SetToolTip(control, tip);
				tt.Active = true;
				return true;
			} else {
				return false;
			}
		}

		internal override void SystrayRemove(IntPtr handle, ref ToolTip tt) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			UnmapWindow(hwnd, WindowType.Whole);
			SetParent(hwnd.whole_window, FosterParent);

			// The caller can now re-dock it later...
			if (tt != null) {
				tt.Dispose();
				tt = null;
			}
		}

		internal override bool Text(IntPtr handle, string text) {
			lock (XlibLock) {
				// FIXME - use _NET properties
				XStoreName(DisplayHandle, Hwnd.ObjectFromHandle(handle).whole_window, text);
			}
			return true;
		}

		internal override bool TranslateMessage(ref MSG msg) {
			return Keyboard.TranslateMessage (ref msg);
		}

		internal override void UpdateWindow(IntPtr handle) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (!hwnd.visible || !hwnd.expose_pending || !hwnd.Mapped) {
				return;
			}

			SendMessage(handle, Msg.WM_PAINT, IntPtr.Zero, IntPtr.Zero);
			hwnd.Queue.Paint.Remove(hwnd);
		}

		private bool WindowIsMapped(IntPtr handle) {
			XWindowAttributes attributes;

			attributes = new XWindowAttributes();
			XGetWindowAttributes(DisplayHandle, handle, ref attributes);
			if (attributes.map_state == MapState.IsUnmapped) {
				return false;
			}
			return true;
		}

		#endregion	// Public Static Methods

		#region Events
		internal override event EventHandler Idle;
		#endregion	// Events

		#region X11 Imports
		[DllImport ("libX11", EntryPoint="XOpenDisplay")]
		internal extern static IntPtr XOpenDisplay(IntPtr display);
		[DllImport ("libX11", EntryPoint="XCloseDisplay")]
		internal extern static int XCloseDisplay(IntPtr display);						    
		[DllImport ("libX11", EntryPoint="XSynchronize")]
		internal extern static IntPtr XSynchronize(IntPtr display, bool onoff);

		[DllImport ("libX11", EntryPoint="XCreateWindow")]
		internal extern static IntPtr XCreateWindow(IntPtr display, IntPtr parent, int x, int y, int width, int height, int border_width, int depth, int xclass, IntPtr visual, UIntPtr valuemask, ref XSetWindowAttributes attributes);
		[DllImport ("libX11", EntryPoint="XCreateSimpleWindow")]
		internal extern static IntPtr XCreateSimpleWindow(IntPtr display, IntPtr parent, int x, int y, int width, int height, int border_width, UIntPtr border, UIntPtr background);
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
		[DllImport ("libX11", EntryPoint="XSelectInput")]
		internal extern static IntPtr XSelectInput(IntPtr display, IntPtr window, IntPtr mask);

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
		internal extern static int XSendEvent(IntPtr display, IntPtr window, bool propagate, IntPtr event_mask, ref XEvent send_event);

		[DllImport ("libX11", EntryPoint="XQueryTree")]
		internal extern static int XQueryTree(IntPtr display, IntPtr window, out IntPtr root_return, out IntPtr parent_return, out IntPtr children_return, out int nchildren_return);

		[DllImport ("libX11", EntryPoint="XFree")]
		internal extern static int XFree(IntPtr data);

		[DllImport ("libX11", EntryPoint="XRaiseWindow")]
		internal extern static int XRaiseWindow(IntPtr display, IntPtr window);

		[DllImport ("libX11", EntryPoint="XLowerWindow")]
		internal extern static uint XLowerWindow(IntPtr display, IntPtr window);

		[DllImport ("libX11", EntryPoint="XConfigureWindow")]
		internal extern static uint XConfigureWindow(IntPtr display, IntPtr window, ChangeWindowFlags value_mask, ref XWindowChanges values);

		[DllImport ("libX11", EntryPoint="XInternAtom")]
		internal extern static IntPtr XInternAtom(IntPtr display, string atom_name, bool only_if_exists);

		[DllImport ("libX11", EntryPoint="XSetWMProtocols")]
		internal extern static int XSetWMProtocols(IntPtr display, IntPtr window, IntPtr[] protocols, int count);

		[DllImport ("libX11", EntryPoint="XGrabPointer")]
		internal extern static int XGrabPointer(IntPtr display, IntPtr window, bool owner_events, EventMask event_mask, GrabMode pointer_mode, GrabMode keyboard_mode, IntPtr confine_to, IntPtr cursor, IntPtr timestamp);

		[DllImport ("libX11", EntryPoint="XUngrabPointer")]
		internal extern static int XUngrabPointer(IntPtr display, IntPtr timestamp);

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

		[DllImport ("libX11", EntryPoint="XDefaultScreen")]
		internal extern static int XDefaultScreen(IntPtr display);

		[DllImport ("libX11", EntryPoint="XDefaultColormap")]
		internal extern static IntPtr XDefaultColormap(IntPtr display, int screen_number);

		[DllImport ("libX11", EntryPoint="XLookupColor")]
		internal extern static int XLookupColor(IntPtr display, IntPtr Colormap, string Coloranem, ref XColor exact_def_color, ref XColor screen_def_color);

		[DllImport ("libX11", EntryPoint="XAllocColor")]
		internal extern static int XAllocColor(IntPtr display, IntPtr Colormap, ref XColor colorcell_def);

		[DllImport ("libX11", EntryPoint="XSetTransientForHint")]
		internal extern static int XSetTransientForHint(IntPtr display, IntPtr window, IntPtr prop_window);

		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, ref MotifWmHints data, int nelements);

		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, ref uint value, int nelements);

		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, ref IntPtr value, int nelements);

		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, uint[] data, int nelements);

		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, int[] data, int nelements);

		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, IntPtr[] data, int nelements);

		[DllImport ("libX11", EntryPoint="XChangeProperty")]
		internal extern static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, IntPtr atoms, int nelements);

		[DllImport ("libX11", EntryPoint="XChangeProperty", CharSet=CharSet.Ansi)]
		internal extern static int XChangeProperty(IntPtr display, IntPtr window, IntPtr property, IntPtr type, int format, PropertyMode mode, string text, int text_length);

		[DllImport ("libX11", EntryPoint="XDeleteProperty")]
		internal extern static int XDeleteProperty(IntPtr display, IntPtr window, IntPtr property);

		[DllImport ("gdiplus", EntryPoint="GetFontMetrics")]
		internal extern static bool GetFontMetrics(IntPtr graphicsObject, IntPtr nativeObject, out int ascent, out int descent);

		// Drawing
		[DllImport ("libX11", EntryPoint="XCreateGC")]
		internal extern static IntPtr XCreateGC(IntPtr display, IntPtr window, IntPtr valuemask, ref XGCValues values);

		[DllImport ("libX11", EntryPoint="XFreeGC")]
		internal extern static int XFreeGC(IntPtr display, IntPtr gc);

		[DllImport ("libX11", EntryPoint="XSetFunction")]
		internal extern static int XSetFunction(IntPtr display, IntPtr gc, GXFunction function);

		[DllImport ("libX11", EntryPoint="XDrawLine")]
		internal extern static int XDrawLine(IntPtr display, IntPtr drawable, IntPtr gc, int x1, int y1, int x2, int y2);

		[DllImport ("libX11", EntryPoint="XDrawRectangle")]
		internal extern static int XDrawRectangle(IntPtr display, IntPtr drawable, IntPtr gc, int x1, int y1, int width, int height);

		[DllImport ("libX11", EntryPoint="XSetWindowBackground")]
		internal extern static int XSetWindowBackground(IntPtr display, IntPtr window, IntPtr background);

		[DllImport ("libX11", EntryPoint="XCopyArea")]
		internal extern static int XCopyArea(IntPtr display, IntPtr src, IntPtr dest, IntPtr gc, int src_x, int src_y, int width, int height, int dest_x, int dest_y);

		[DllImport ("libX11", EntryPoint="XGetWindowProperty")]
		internal extern static int XGetWindowProperty(IntPtr display, IntPtr window, IntPtr atom, IntPtr long_offset, IntPtr long_length, bool delete, IntPtr req_type, out IntPtr actual_type, out int actual_format, out IntPtr nitems, out IntPtr bytes_after, ref IntPtr prop);

		[DllImport ("libX11", EntryPoint="XSetInputFocus")]
		internal extern static int XSetInputFocus(IntPtr display, IntPtr window, RevertTo revert_to, IntPtr time);

		[DllImport ("libX11", EntryPoint="XIconifyWindow")]
		internal extern static int XIconifyWindow(IntPtr display, IntPtr window, int screen_number);

		[DllImport ("libX11", EntryPoint="XDefineCursor")]
		internal extern static int XDefineCursor(IntPtr display, IntPtr window, IntPtr cursor);

		[DllImport ("libX11", EntryPoint="XUndefineCursor")]
		internal extern static int XUndefineCursor(IntPtr display, IntPtr window);

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

		[DllImport ("libX11", EntryPoint="XGrabServer")]
		internal extern static void XGrabServer(IntPtr display);

		[DllImport ("libX11", EntryPoint="XUngrabServer")]
		internal extern static void XUngrabServer(IntPtr display);

		[DllImport ("libX11", EntryPoint="XGetWMNormalHints")]
		internal extern static void XGetWMNormalHints(IntPtr display, IntPtr window, ref XSizeHints hints, out IntPtr supplied_return);

		[DllImport ("libX11", EntryPoint="XSetWMNormalHints")]
		internal extern static void XSetWMNormalHints(IntPtr display, IntPtr window, ref XSizeHints hints);

		[DllImport ("libX11", EntryPoint="XSetZoomHints")]
		internal extern static void XSetZoomHints(IntPtr display, IntPtr window, ref XSizeHints hints);

		[DllImport ("libX11", EntryPoint="XSetWMHints")]
		internal extern static void XSetWMHints(IntPtr display, IntPtr window, ref XWMHints wmhints);

		[DllImport ("libX11", EntryPoint="XSync")]
		internal extern static void XSync(IntPtr display, IntPtr window);

		[DllImport ("libX11", EntryPoint="XGetIconSizes")]
		internal extern static int XGetIconSizes(IntPtr display, IntPtr window, out IntPtr size_list, out int count);

		[DllImport ("libX11", EntryPoint="XSetErrorHandler")]
		internal extern static IntPtr XSetErrorHandler(XErrorHandler error_handler);

		[DllImport ("libX11", EntryPoint="XGetErrorText")]
		internal extern static IntPtr XGetErrorText(IntPtr display, byte code, StringBuilder buffer, int length);

		[DllImport ("libX11", EntryPoint="XInitThreads")]
		internal extern static int XInitThreads();

		[DllImport ("libX11", EntryPoint="XConvertSelection")]
		internal extern static int XConvertSelection(IntPtr display, IntPtr selection, IntPtr target, IntPtr property, IntPtr requestor, IntPtr time);

		[DllImport ("libX11", EntryPoint="XGetSelectionOwner")]
		internal extern static IntPtr XGetSelectionOwner(IntPtr display, IntPtr selection);

		[DllImport ("libX11", EntryPoint="XSetSelectionOwner")]
		internal extern static int XSetSelectionOwner(IntPtr display, IntPtr selection, IntPtr owner, IntPtr time);

		[DllImport ("libX11", EntryPoint="XSetPlaneMask")]
		internal extern static int XSetPlaneMask(IntPtr display, IntPtr gc, IntPtr mask);

		[DllImport ("libX11", EntryPoint="XSetForeground")]
		internal extern static int XSetForeground(IntPtr display, IntPtr gc, UIntPtr foreground);

		[DllImport ("libX11", EntryPoint="XSetBackground")]
		internal extern static int XSetBackground(IntPtr display, IntPtr gc, UIntPtr background);

		[DllImport ("libX11", EntryPoint="XBell")]
		internal extern static int XBell(IntPtr display, int percent);

		[DllImport ("libX11", EntryPoint="XChangeActivePointerGrab")]
		internal extern static int XChangeActivePointerGrab (IntPtr display, EventMask event_mask, IntPtr cursor, IntPtr time);

		[DllImport ("libX11", EntryPoint="XFilterEvent")]
		internal extern static bool XFilterEvent(ref XEvent xevent, IntPtr window);

		[DllImport ("libX11")]
		internal extern static void XkbSetDetectableAutoRepeat (IntPtr display, bool detectable, IntPtr supported);

		[DllImport ("libX11")]
		internal extern static void XPeekEvent (IntPtr display, ref XEvent xevent);
		#endregion
	}
}
