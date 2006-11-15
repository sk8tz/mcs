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
//	Chris Toshok	toshok@ximian.com
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
#undef DriverDebugExtra
#undef DriverDebugParent
#undef DriverDebugCreate
#undef DriverDebugDestroy
#undef DriverDebugThreads
#undef DriverDebugXEmbed

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
using System.Windows.Forms.X11Internal;

/// X11 Version
namespace System.Windows.Forms {
	internal class XplatUIX11_new : XplatUIDriver {
		#region Local Variables
		// General
		static volatile XplatUIX11_new	Instance;
		static int		RefCount;
		static bool		themes_enabled;

		// Message Loop
		static Hashtable	MessageQueues;		// Holds our thread-specific XEventQueues

		// Cursors
		static IntPtr		LastCursorWindow;	// The last window we set the cursor on
		static IntPtr		LastCursorHandle;	// The handle that was last set on LastCursorWindow
		static IntPtr		OverrideCursorHandle;	// The cursor that is set to override any other cursors

		X11Display display;

		static readonly object lockobj = new object ();

		#endregion	// Local Variables
		#region Constructors
		private XplatUIX11_new() {
			// Handle singleton stuff first
			RefCount = 0;

			// Now regular initialization
			MessageQueues = Hashtable.Synchronized (new Hashtable(7));
			Xlib.XInitThreads();
		}

		private void InitializeDisplay ()
		{
			display = new X11Display (Xlib.XOpenDisplay(IntPtr.Zero));

			Graphics.FromHdcInternal (display.Handle);
		}

		~XplatUIX11_new() {
			// Remove our display handle from S.D
			Graphics.FromHdcInternal (IntPtr.Zero);
		}

		#endregion	// Constructors

		#region Singleton Specific Code
		public static XplatUIX11_new GetInstance() {
			lock (lockobj) {
				if (Instance == null) {
					Instance = new XplatUIX11_new ();

					Instance.InitializeDisplay ();
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

		#region Internal Methods
		internal static void Where() {
			Console.WriteLine("Here: {0}\n", GetInstance().display.WhereString());
		}

		#endregion	// Internal Methods

		#region Private Methods

#if false
		private void WakeupMain ()
		{
			wake.Send (new byte [] { 0xFF });
		}
#endif

		internal XEventQueue ThreadQueue (Thread thread)
		{
			XEventQueue	queue;

			queue = (XEventQueue)MessageQueues[thread];
			if (queue == null) {
				queue = new XEventQueue(thread);
				MessageQueues[thread] = queue;
			}

			return queue;
		}

		private void FrameExtents (IntPtr window, out int left, out int top)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(window);

			if (hwnd != null)
				hwnd.FrameExtents (out left, out top);
			else {
				left = top = 0;
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

#if false
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
				if (hwnd == null)
					continue;

				switch (xevent.type) {
				case XEventName.Expose:
					hwnd.AddExpose (xevent.ExposeEvent.window == hwnd.ClientWindow, xevent.ExposeEvent.x, xevent.ExposeEvent.y, xevent.ExposeEvent.width, xevent.ExposeEvent.height);
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
					hwnd.AddConfigureNotify(xevent);
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
#endif

		private IntPtr GetMousewParam (int Delta)
		{
			return display.GetMousewParam (Delta);
		}

		#endregion	// Private Methods


		#region Public Properties
		internal override int Caption {
			get {
				return 19;
			}
		}

		internal override Size CursorSize {
			get {
				return display.CursorSize;
			}
		}

		internal override  bool DragFullWindows {
			get {
				return true;
			}
		} 

		internal override Size DragSize {
			get {
				return new Size(4, 4);
			}
		} 

		internal override Size FrameBorderSize { 
			get {
				throw new NotImplementedException(); 
			}
		}

		internal override Size IconSize {
			get {
				return display.IconSize;
			}
		}

		internal override int KeyboardSpeed {
			get {
				return display.KeyboardSpeed;
			}
		}

		internal override int KeyboardDelay {
			get {
				return display.KeyboardSpeed;
			}
		}

		internal override Size MaxWindowTrackSize {
			get {
				return new Size (WorkingArea.Width, WorkingArea.Height);
			}
		} 

		internal override Size MinimizedWindowSize {
			get {
				return new Size(1, 1);
			}
		} 

		internal override Size MinimizedWindowSpacingSize {
			get {
				return new Size(1, 1);
			}
		} 

		internal override Size MinimumWindowSize {
			get {
				return new Size(1, 1);
			}
		} 

		internal override Size MinWindowTrackSize {
			get {
				return new Size(1, 1);
			}
		}

		internal override Keys ModifierKeys {
			get {
				return display.ModifierKeys;
			}
		}

		internal override Size SmallIconSize {
			get {
				return display.SmallIconSize;
			}
		}

		internal override int MouseButtonCount {
			get {
				return 3;
			}
		} 

		internal override bool MouseButtonsSwapped {
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
				return display.MouseHoverTime;
			}
		}

		internal override  bool MouseWheelPresent {
			get {
				return true;	// FIXME - how to detect?
			}
		} 

		internal override Rectangle VirtualScreen {
			get {
				return WorkingArea;
			}
		} 

		internal override Rectangle WorkingArea {
			get {
				return display.WorkingArea;
			}
		}

		internal override bool ThemesEnabled {
			get {
				return XplatUIX11_new.themes_enabled;
			}
		}
 

		#endregion	// Public properties

		#region Public Static Methods
		internal override IntPtr InitializeDriver()
		{
			lock (this) {
				if (display == null)
					display = new X11Display (Xlib.XOpenDisplay(IntPtr.Zero));
			}
			return IntPtr.Zero;
		}

		internal override void ShutdownDriver(IntPtr token)
		{
			lock (this) {
				if (display != null) {
					display.Close ();
					display = null;
				}
			}
		}

		internal override void EnableThemes()
		{
			themes_enabled = true;
		}

		internal override void Activate (IntPtr handle)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				hwnd.Activate ();
		}

		internal override void AudibleAlert()
		{
			display.AudibleAlert ();
		}


		internal override void CaretVisible (IntPtr handle, bool visible)
		{
			display.CaretVisible (handle, visible);
		}

		// XXX this implementation should probably be shared between all non-win32 backends
		internal override bool CalculateWindowRect (ref Rectangle ClientRect, int Style, int ExStyle, Menu menu, out Rectangle WindowRect)
		{
			FormBorderStyle	border_style;
			TitleStyle	title_style;
			int caption_height;
			int tool_caption_height;

			// XXX this method should be static on Hwnd, not X11Hwnd
			X11Hwnd.DeriveStyles (Style, ExStyle, out border_style, out title_style,
					      out caption_height, out tool_caption_height);

			WindowRect = Hwnd.GetWindowRectangle(border_style, menu, title_style,
							     caption_height, tool_caption_height,
							     ClientRect);
			return true;
		}

		internal override void ClientToScreen (IntPtr handle, ref int x, ref int y)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				hwnd.ClientToScreen (ref x, ref y);
		}

		internal override int[] ClipboardAvailableFormats (IntPtr handle)
		{
			return display.ClipboardAvailableFormats (handle);
		}

		internal override void ClipboardClose (IntPtr handle)
		{
			display.ClipboardClose (handle);
		}

		internal override int ClipboardGetID (IntPtr handle, string format)
		{
			return display.ClipboardGetID (handle, format);
		}

		internal override IntPtr ClipboardOpen (bool primary_selection)
		{
			return display.ClipboardOpen (primary_selection);
		}

		internal override object ClipboardRetrieve (IntPtr handle, int type, XplatUI.ClipboardToObject converter)
		{
			return display.ClipboardRetrieve (handle, type, converter);
		}

		internal override void ClipboardStore (IntPtr handle, object obj, int type, XplatUI.ObjectToClipboard converter)
		{
			display.ClipboardStore (handle, obj, type, converter);
		}

		internal override void CreateCaret (IntPtr handle, int width, int height)
		{
			display.CreateCaret (handle, width, height);
		}

		internal override IntPtr CreateWindow (CreateParams cp)
		{
			X11Hwnd hwnd = new X11Hwnd (display);

			hwnd.CreateWindow (cp);

			return hwnd.Handle;
		}

		internal override IntPtr CreateWindow (IntPtr Parent, int X, int Y, int Width, int Height)
		{
			CreateParams create_params = new CreateParams();

			create_params.Caption = "";
			create_params.X = X;
			create_params.Y = Y;
			create_params.Width = Width;
			create_params.Height = Height;

			create_params.ClassName = XplatUI.DefaultClassName;
			create_params.ClassStyle = 0;
			create_params.ExStyle = 0;
			create_params.Parent = IntPtr.Zero;
			create_params.Param = 0;

			return CreateWindow (create_params);
		}

		internal override IntPtr DefineCursor (Bitmap bitmap, Bitmap mask, Color cursor_pixel, Color mask_pixel, int xHotSpot, int yHotSpot)
		{
			return display.DefineCursor (bitmap, mask, cursor_pixel, mask_pixel, xHotSpot, yHotSpot);
		}

		internal override IntPtr DefineStdCursor (StdCursor id)
		{
			return display.DefineStdCursor (id);
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

		internal override void DestroyCaret (IntPtr handle)
		{
			display.DestroyCaret (handle);
		}

		internal override void DestroyCursor(IntPtr cursor)
		{
			display.DestroyCursor (cursor);
		}

		internal override void DestroyWindow (IntPtr handle) {
			X11Hwnd hwnd;

			hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd == null) {
#if DriverDebug || DriverDebugDestroy
				Console.WriteLine("window {0:X} already destroyed", handle.ToInt32());
#endif
				return;
			}

#if DriverDebug || DriverDebugDestroy
			Console.WriteLine("Destroying window {0}", XplatUI.Window(hwnd.ClientWindow));
#endif

			display.DestroyWindow (hwnd);
		}

		internal override IntPtr DispatchMessage(ref MSG msg)
		{
			return NativeWindow.WndProc(msg.hwnd, msg.message, msg.wParam, msg.lParam);
		}

		internal override void DrawReversibleRectangle (IntPtr handle, Rectangle rect, int line_width)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				hwnd.DrawReversibleRectangle (rect, line_width);
		}

		// XXX this should be shared with other non-win32 backends
		// XXX perhaps after addressing the OverrideCursorHandle stuff.
		internal override void DoEvents ()
		{
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

		internal override void EnableWindow(IntPtr handle, bool Enable)
		{
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			if (hwnd != null) {
				hwnd.Enabled = Enable;
			}
		}

		internal override void EndLoop(Thread thread) {
			// This is where we one day will shut down the loop for the thread
		}


		internal override IntPtr GetActive()
		{
			X11Hwnd hwnd = display.GetActiveWindow ();

			return (hwnd == null) ? IntPtr.Zero : hwnd.Handle;
		}

		internal override Region GetClipRegion (IntPtr handle)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			return (hwnd == null) ? null : hwnd.GetClipRegion ();
		}

		internal override void GetCursorInfo(IntPtr cursor, out int width, out int height, out int hotspot_x, out int hotspot_y)
		{
			width = 20;
			height = 20;
			hotspot_x = 0;
			hotspot_y = 0;
		}

		internal override void GetDisplaySize(out Size size)
		{
			display.GetDisplaySize (out size);
		}

		internal override SizeF GetAutoScaleSize (Font font)
		{
			return display.GetAutoScaleSize (font);
		}

		// XXX this should be someplace shareable by all non-win32 backends..  like in Hwnd itself.
		// maybe a Hwnd.ParentHandle property
		internal override IntPtr GetParent (IntPtr handle) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			if (hwnd != null && hwnd.parent != null) {
				return hwnd.parent.Handle;
			}
			return IntPtr.Zero;
		}

		internal override void GetCursorPos (IntPtr handle, out int x, out int y)
		{
			display.GetCursorPos ((X11Hwnd)Hwnd.ObjectFromHandle(handle),
					      out x, out y);
		}

		internal override IntPtr GetFocus()
		{
			return display.GetFocus ();
		}

		// XXX this should be shared amongst non-win32 backends
		internal override bool GetFontMetrics (Graphics g, Font font, out int ascent, out int descent)
		{
			return Xlib.GetFontMetrics(g.GetHdc(), font.ToHfont(), out ascent, out descent);
		}


		// XXX this should be shared amongst non-win32 backends
		internal override Point GetMenuOrigin (IntPtr handle)
		{
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (hwnd != null) {
				return hwnd.MenuOrigin;
			}
			return Point.Empty;
		}

		internal override bool GetMessage (object queue_id, ref MSG msg, IntPtr handle, int wFilterMin, int wFilterMax)
		{
			return display.GetMessage (queue_id, ref msg, handle, wFilterMin, wFilterMax);
		}

		internal override bool GetText(IntPtr handle, out string text)
		{
#if true
			text = "";
			return false;
#else
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
#endif
		}

		internal override void GetWindowPos (IntPtr handle, bool is_toplevel, out int x, out int y, out int width, out int height, out int client_width, out int client_height)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null) {
				hwnd.GetPosition (is_toplevel, out x, out y, out width, out height, out client_width, out client_height);
			}
			else {
				// Should we throw an exception or fail silently?
				// throw new ArgumentException("Called with an invalid window handle", "handle");

				x = 0;
				y = 0;
				width = 0;
				height = 0;
				client_width = 0;
				client_height = 0;
			}
		}

		internal override FormWindowState GetWindowState (IntPtr handle)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd == null)
				return FormWindowState.Normal; // XXX should we throw an exception here?  probably

			return hwnd.GetWindowState ();
		}

		internal override void GrabInfo (out IntPtr handle, out bool GrabConfined, out Rectangle GrabArea)
		{
			display.GrabInfo (out handle, out GrabConfined, out GrabArea);
		}

		internal override void GrabWindow (IntPtr handle, IntPtr confine_to_handle)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);
			X11Hwnd confine_to_hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(confine_to_handle);

			display.GrabWindow (hwnd, confine_to_hwnd);
		}

		internal override void UngrabWindow (IntPtr handle)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			display.UngrabWindow (hwnd);
		}

		internal override void HandleException(Exception e) {
			StackTrace st = new StackTrace(e, true);
			Console.WriteLine("Exception '{0}'", e.Message+st.ToString());
			Console.WriteLine("{0}{1}", e.Message, st.ToString());
		}

		internal override void Invalidate (IntPtr handle, Rectangle rc, bool clear)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			hwnd.Invalidate (rc, clear);
		}

		internal override bool IsEnabled(IntPtr handle)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle (handle);

			return hwnd != null && hwnd.Enabled;
		}
		
		internal override bool IsVisible(IntPtr handle)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle (handle);

			return hwnd != null && hwnd.Visible;
		}

		internal override void KillTimer (Timer timer)
		{
			XEventQueue queue = (XEventQueue) MessageQueues [timer.thread];

			if (queue == null) {
				// This isn't really an error, MS doesn't start the timer if
				// it has no assosciated queue
				return;
			}
			queue.timer_list.Remove (timer);
		}

		internal override void MenuToScreen (IntPtr handle, ref int x, ref int y)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			hwnd.MenuToScreen (ref x, ref y);
		}

		internal override void OverrideCursor (IntPtr cursor)
		{
			OverrideCursorHandle = cursor;
		}

		internal override PaintEventArgs PaintEventStart (IntPtr handle, bool client)
		{
			return display.PaintEventStart (handle, client);
		}

		internal override void PaintEventEnd (IntPtr handle, bool client)
		{
			display.PaintEventEnd (handle, client);
		}


		// XXX this can probably be shared between all non-win32 backends
		[MonoTODO("Implement filtering and PM_NOREMOVE")]
		internal override bool PeekMessage(Object queue_id, ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags)
		{
			XEventQueue queue = (XEventQueue) queue_id;
			bool	pending;

			if ((flags & (uint)PeekMessageFlags.PM_REMOVE) == 0) {
				throw new NotImplementedException("PeekMessage PM_NOREMOVE is not implemented yet");	// FIXME - Implement PM_NOREMOVE flag
			}

			pending = false;
			if (queue.Count > 0) {
				pending = true;
			} else if (((XEventQueue)queue_id).Paint.Count > 0) {
				pending = true;
			}

			CheckTimers(queue.timer_list, DateTime.UtcNow);

			if (!pending) {
				return false;
			}
			return GetMessage(queue_id, ref msg, hWnd, wFilterMin, wFilterMax);
		}

		// FIXME - I think this should just enqueue directly
		internal override bool PostMessage (IntPtr handle, Msg message, IntPtr wparam, IntPtr lparam)
		{
			return display.PostMessage (handle, message, wparam, lparam);
		}

		internal override void PostQuitMessage(int exitCode)
		{
			display.Flush ();
			ThreadQueue(Thread.CurrentThread).PostQuitState = true;
		}

		internal override void RequestNCRecalc (IntPtr handle)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				hwnd.RequestNCRecalc ();
		}

		internal override void ResetMouseHover (IntPtr handle)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			display.ResetMouseHover (hwnd);
		}

		internal override void ScreenToClient(IntPtr handle, ref int x, ref int y)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				hwnd.ScreenToClient (ref x, ref y);
		}

		internal override void ScreenToMenu (IntPtr handle, ref int x, ref int y)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				hwnd.ScreenToMenu (ref x, ref y);
		}

		internal override void ScrollWindow (IntPtr handle, Rectangle area, int XAmount, int YAmount, bool with_children)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				hwnd.ScrollWindow (area, XAmount, YAmount, with_children);
		}

		internal override void ScrollWindow(IntPtr handle, int XAmount, int YAmount, bool with_children)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.GetObjectFromWindow(handle);

			if (hwnd != null) {
				Rectangle	rect;

				rect = hwnd.ClientRect;
				rect.X = 0;
				rect.Y = 0;
				hwnd.ScrollWindow (rect, XAmount, YAmount, with_children);
			}
		}

		internal override void SendAsyncMethod (AsyncMethodData method)
		{
			display.SendAsyncMethod (method);
		}

		// XXX this is likely shareable amongst other backends
		internal override IntPtr SendMessage (IntPtr handle, Msg message, IntPtr wParam, IntPtr lParam)
		{
			return display.SendMessage (handle, message, wParam, lParam);
		}

		internal override void SetAllowDrop (IntPtr handle, bool value)
		{
			// We allow drop on all windows
		}

		internal override DragDropEffects StartDrag (IntPtr handle, object data,
							     DragDropEffects allowed_effects)
		{
			return display.StartDrag (handle, data, allowed_effects);
		}

		internal override void SetBorderStyle (IntPtr handle, FormBorderStyle border_style)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				hwnd.SetBorderStyle (border_style);
		}

		internal override void SetCaretPos (IntPtr handle, int x, int y)
		{
			display.SetCaretPos (handle, x, y);
		}

		internal override void SetClipRegion (IntPtr handle, Region region)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				hwnd.SetClipRegion (region);
		}

		internal override void SetCursor (IntPtr handle, IntPtr cursor)
		{
			// XXX this needs moving
#if false
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
#endif
		}

		internal override void SetCursorPos (IntPtr handle, int x, int y)
		{
			if (handle == IntPtr.Zero) {
				display.SetCursorPos (x, y);
			}
			else {
				X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

				hwnd.SetCursorPos (x, y);
			}
		}

		internal override void SetFocus (IntPtr handle)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			display.SetFocus (hwnd);
		}

		internal override void SetIcon(IntPtr handle, Icon icon)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);;

			if (hwnd != null)
				hwnd.SetIcon (icon);
		}

		internal override void SetMenu(IntPtr handle, Menu menu)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			hwnd.SetMenu (menu);
		}

		internal override void SetModal(IntPtr handle, bool Modal)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				display.SetModal (hwnd, Modal);
		}

		internal override IntPtr SetParent(IntPtr handle, IntPtr parent)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);
			X11Hwnd parent_hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(parent);

			if (hwnd != null)
				hwnd.SetParent (parent_hwnd);

			return IntPtr.Zero;
		}

		internal override void SetTimer (Timer timer)
		{
			XEventQueue queue = (XEventQueue) MessageQueues [timer.thread];

			if (queue == null) {
				// This isn't really an error, MS doesn't start the timer if
				// it has no assosciated queue
				return;
			}
			queue.timer_list.Add (timer);
			//WakeupMain ();
		}

		internal override bool SetTopmost(IntPtr handle, IntPtr handle_owner, bool enabled)
		{
			X11Hwnd hwnd;

			hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd == null)
				return false;

			X11Hwnd hwnd_owner = (X11Hwnd)Hwnd.ObjectFromHandle(handle_owner);

			return hwnd.SetTopmost (hwnd_owner, enabled);
		}

		internal override bool SetVisible (IntPtr handle, bool visible, bool activate)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			return hwnd != null && hwnd.SetVisible (visible, activate);
		}

		internal override void SetWindowMinMax (IntPtr handle, Rectangle maximized, Size min, Size max)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd == null)
				return;

			hwnd.SetMinMax (maximized, min, max);
		}

		internal override void SetWindowPos (IntPtr handle, int x, int y, int width, int height)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				hwnd.SetPosition (x, y, width, height);
		}

		internal override void SetWindowState (IntPtr handle, FormWindowState state)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				hwnd.SetWindowState (state);
		}

		internal override void SetWindowStyle (IntPtr handle, CreateParams cp)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null) {
				hwnd.SetHwndStyles (cp);
				hwnd.SetWMStyles (cp);
			}
		}

		internal override void SetWindowTransparency(IntPtr handle, double transparency, Color key)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				hwnd.SetWindowTransparency (transparency, key);
		}

		internal override bool SetZOrder (IntPtr handle, IntPtr after_handle, bool top, bool bottom)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd == null || !hwnd.mapped)
				return false;

			X11Hwnd after_hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(after_handle);

			return hwnd.SetZOrder (after_hwnd, top, bottom);
		}

		internal override void ShowCursor (bool show)
		{
			display.ShowCursor (show);
		}

		internal override object StartLoop(Thread thread)
		{
			return (object) ThreadQueue(thread);
		}

		internal override bool SupportsTransparency()
		{
			return display.SupportsTransparency ();
		}

		internal override bool SystrayAdd (IntPtr handle, string tip, Icon icon, out ToolTip tt)
		{
			return display.SystrayAdd (handle, tip, icon, out tt);
		}

		internal override bool SystrayChange (IntPtr handle, string tip, Icon icon, ref ToolTip tt)
		{
			return display.SystrayChange (handle, tip, icon, ref tt);
		}

		internal override void SystrayRemove (IntPtr handle, ref ToolTip tt)
		{
			display.SystrayRemove (handle, ref tt);
		}

		internal override bool Text (IntPtr handle, string text)
		{
			X11Hwnd	hwnd = (X11Hwnd) Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				hwnd.Text = text;

			return true;
		}

		internal override bool TranslateMessage (ref MSG msg)
		{
			return display.TranslateMessage (ref msg);
		}

		internal override void UpdateWindow (IntPtr handle)
		{
			X11Hwnd hwnd = (X11Hwnd)Hwnd.ObjectFromHandle(handle);

			if (hwnd != null)
				hwnd.Update ();
		}

		#endregion	// Public Static Methods

		#region Events
		internal override event EventHandler Idle;
		#endregion	// Events
	}
}
