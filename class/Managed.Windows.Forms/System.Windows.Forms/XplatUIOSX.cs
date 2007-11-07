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
// Copyright (c) 2004-2007 Novell, Inc.
//
// Authors:
//	Geoff Norton  <gnorton@novell.com>
//
//

// This really doesn't work at all; please dont file bugs on it yet.

// MAJOR TODO:
//  Wire up keyboard

#define EnableNCArea

using System;
using System.Threading;
using System.Drawing;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

/// OSX Version
namespace System.Windows.Forms {

	// The Carbon Event callback delegate
	delegate int CarbonEventDelegate (IntPtr inCallRef, IntPtr inEvent, IntPtr userData);

	internal class XplatUIOSX : XplatUIDriver {
		
		#region Local Variables
		
		// General driver variables
		private static XplatUIOSX Instance;
		private static OSXKeyboard Keyboard;
		private static int RefCount;
		private static bool themes_enabled;
		private static IntPtr FocusWindow;
		private static IntPtr ActiveWindow;
		private static IntPtr ReverseWindow;

		// Mouse 
		private static MouseButtons MouseState;
		Point mouse_position;
		private static Hwnd MouseWindow;
		
		// OSX Specific
		private static GrabStruct Grab;
		private static OSXCaret Caret;
		private static OSXHover Hover;
		private CarbonEventDelegate CarbonEventHandler;
		private static Hashtable WindowMapping;
		private static Hashtable WindowBackgrounds;
		private static Hwnd GrabWindowHwnd;
		private static IntPtr FosterParent;
		private static int MenuBarHeight;
		private static EventTypeSpec [] view_events = new EventTypeSpec [] {
									new EventTypeSpec (OSXConstants.kEventClassControl, OSXConstants.kEventControlSetFocusPart), 
									new EventTypeSpec (OSXConstants.kEventClassControl, OSXConstants.kEventControlClick), 
									new EventTypeSpec (OSXConstants.kEventClassControl, OSXConstants.kEventControlContextualMenuClick), 
									new EventTypeSpec (OSXConstants.kEventClassControl, OSXConstants.kEventControlTrack), 
									new EventTypeSpec (OSXConstants.kEventClassControl, OSXConstants.kEventControlSimulateHit), 
									new EventTypeSpec (OSXConstants.kEventClassControl, OSXConstants.kEventControlBoundsChanged), 
									new EventTypeSpec (OSXConstants.kEventClassControl, OSXConstants.kEventControlTrackingAreaEntered), 
									new EventTypeSpec (OSXConstants.kEventClassControl, OSXConstants.kEventControlTrackingAreaExited), 
									new EventTypeSpec (OSXConstants.kEventClassControl, OSXConstants.kEventControlDraw) 
									};
		private static EventTypeSpec [] window_events = new EventTypeSpec[] {
									//new EventTypeSpec (OSXConstants.kEventClassMouse, OSXConstants.kEventMouseEntered),
									//new EventTypeSpec (OSXConstants.kEventClassMouse, OSXConstants.kEventMouseExited),
									new EventTypeSpec (OSXConstants.kEventClassMouse, OSXConstants.kEventMouseMoved),
									//new EventTypeSpec (OSXConstants.kEventClassMouse, OSXConstants.kEventMouseDragged),
									//new EventTypeSpec (OSXConstants.kEventClassMouse, OSXConstants.kEventMouseWheelMoved),
									new EventTypeSpec (OSXConstants.kEventClassWindow, OSXConstants.kEventWindowBoundsChanged),
									new EventTypeSpec (OSXConstants.kEventClassWindow, OSXConstants.kEventWindowClose),
									new EventTypeSpec (OSXConstants.kEventClassKeyboard, OSXConstants.kEventRawKeyModifiersChanged),
									new EventTypeSpec (OSXConstants.kEventClassKeyboard, OSXConstants.kEventRawKeyDown),
									new EventTypeSpec (OSXConstants.kEventClassKeyboard, OSXConstants.kEventRawKeyRepeat),
									new EventTypeSpec (OSXConstants.kEventClassKeyboard, OSXConstants.kEventRawKeyUp)
									};
									
		
		// Message loop
		private static Queue MessageQueue;
		private static bool GetMessageResult;

		private static bool ReverseWindowMapped;

		// Timers
		private ArrayList TimerList;
		
		static readonly object lockobj = new object ();
		
		// Event Handlers
		internal override event EventHandler Idle;

		#endregion
		
		#region Constructors
		private XplatUIOSX() {

			RefCount = 0;
			TimerList = new ArrayList ();
			MessageQueue = new Queue ();
			
			Initialize ();
		}

		~XplatUIOSX() {
			// FIXME: Clean up the FosterParent here.
		}

		#endregion

		#region Singleton specific code
		
		public static XplatUIOSX GetInstance() {
			lock (lockobj) {
				if (Instance == null) {
					Instance = new XplatUIOSX ();
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
		
		#region Internal methods
		
		internal void Initialize () {

			// Initialize the Event Handler delegate
			CarbonEventHandler = new CarbonEventDelegate (EventCallback);
			
			// Initilize the mouse controls
			Hover.Interval = 500;
			Hover.Timer = new Timer ();
			Hover.Timer.Enabled = false;
			Hover.Timer.Interval = Hover.Interval;
			Hover.Timer.Tick += new EventHandler (HoverCallback);
			Hover.X = -1;
			Hover.Y = -1;
			MouseState = MouseButtons.None;
			mouse_position = Point.Empty;
				
			// Initialize the Caret
			Caret.Timer = new Timer ();
			Caret.Timer.Interval = 500;
			Caret.Timer.Tick += new EventHandler (CaretCallback);
			
			// Initialize the OSX Specific stuff
			WindowMapping = new Hashtable ();
			WindowBackgrounds = new Hashtable ();
			
			// Initialize the FosterParent
			Rect rect = new Rect ();
			SetRect (ref rect, (short)0, (short)0, (short)0, (short)0);
			ProcessSerialNumber psn = new ProcessSerialNumber();

			CheckError (GetCurrentProcess( ref psn ), "GetCurrentProcess ()");
			CheckError (TransformProcessType (ref psn, 1), "TransformProcessType ()");
			CheckError (SetFrontProcess (ref psn), "SetFrontProcess ()");
			CheckError (CreateNewWindow (WindowClass.kDocumentWindowClass, WindowAttributes.kWindowStandardHandlerAttribute | WindowAttributes.kWindowCloseBoxAttribute | WindowAttributes.kWindowFullZoomAttribute | WindowAttributes.kWindowCollapseBoxAttribute | WindowAttributes.kWindowResizableAttribute | WindowAttributes.kWindowCompositingAttribute, ref rect, ref FosterParent), "CreateFosterParent ()");
			
			CreateNewWindow (WindowClass.kOverlayWindowClass, WindowAttributes.kWindowNoUpdatesAttribute | WindowAttributes.kWindowNoActivatesAttribute, ref rect, ref ReverseWindow);
			InstallEventHandler (GetWindowEventTarget (ReverseWindow), CarbonEventHandler, (uint)window_events.Length, window_events, ReverseWindow, IntPtr.Zero);
			
			// Get some values about bar heights
			Rect structRect = new Rect ();
			Rect contentRect = new Rect ();
			CheckError (GetWindowBounds (FosterParent, 32, ref structRect), "GetWindowBounds ()");
			CheckError (GetWindowBounds (FosterParent, 33, ref contentRect), "GetWindowBounds ()");
			
			MenuBarHeight = GetMBarHeight ();
			
			// Focus
			FocusWindow = IntPtr.Zero;
			
			Keyboard = new OSXKeyboard ();

			// Message loop
			GetMessageResult = true;
			
			ReverseWindowMapped = false;
		}
		
		#endregion
		
		#region Callbacks
		
		private void CaretCallback (object sender, EventArgs e) {
			if (Caret.Paused) {
				return;
			}

			if (!Caret.On) {
				ShowCaret ();
			} else {
				HideCaret ();
			}
		}
		
		private void HoverCallback (object sender, EventArgs e) {
			if ((Hover.X == mouse_position.X) && (Hover.Y == mouse_position.Y)) {
				MSG msg = new MSG ();
				msg.hwnd = Hover.Hwnd;
				msg.message = Msg.WM_MOUSEHOVER;
				msg.wParam = GetMousewParam (0);
				msg.lParam = (IntPtr)((ushort)Hover.X << 16 | (ushort)Hover.X);
				MessageQueue.Enqueue (msg);
			}
		}
		
		internal int EventCallback (IntPtr inCallRef, IntPtr inEvent, IntPtr handle) {
			uint eventClass = GetEventClass (inEvent);
			uint eventKind = GetEventKind (inEvent);
			int retVal = 0;
			lock (MessageQueue) {
				switch (eventClass) {
					// keyboard
					case OSXConstants.kEventClassKeyboard: {
						MSG msg = new MSG ();
						Keyboard.KeyEvent (inEvent, handle, eventKind, ref msg);
						MessageQueue.Enqueue (msg);
						return -9874;
					}
					//window
					case OSXConstants.kEventClassWindow: {
						retVal = ProcessWindowEvent (inEvent, eventKind, handle);
						break;
					}
					// mouse
					case OSXConstants.kEventClassMouse: {
						retVal = ProcessMouseEvent (inEvent, eventKind, handle);
						break;
					}
					// control
					case OSXConstants.kEventClassControl: {
						retVal = ProcessControlEvent (inEvent, eventKind, handle);
						break;
					}
					default: {
						break;
					}
				}
			}
			
			return retVal;
		}

		#endregion
		
		#region Private Methods

		internal Point ConvertScreenPointToClient (IntPtr handle, Point point) {
			Point converted_point = new Point ();
			Rect window_bounds = new Rect ();
			CGPoint native_point = new CGPoint ();

			GetWindowBounds (HIViewGetWindow (handle), 32, ref window_bounds);
			
			native_point.x = (point.X - window_bounds.left);
			native_point.y = (point.Y - window_bounds.top);

			HIViewConvertPoint (ref native_point, IntPtr.Zero, handle);

			converted_point.X = (int)native_point.x;
			converted_point.Y = (int)native_point.y;

			return converted_point;
		}
		
		internal Point ConvertClientPointToScreen (IntPtr handle, Point point) {
			Point converted_point = new Point ();
			Rect window_bounds = new Rect ();
			CGPoint native_point = new CGPoint ();

			GetWindowBounds (HIViewGetWindow (handle), 32, ref window_bounds);
			
			native_point.x = point.X;
			native_point.y = point.Y;

			HIViewConvertPoint (ref native_point, handle, IntPtr.Zero);

			converted_point.X = (int)(native_point.x + window_bounds.left);
			converted_point.Y = (int)(native_point.y + window_bounds.top);

			return converted_point;
		}
		
		private int ProcessWindowEvent (IntPtr inEvent, uint eventKind, IntPtr handle) {
			MSG msg = new MSG ();
			switch (eventKind) {
				// Someone closed a window
				case OSXConstants.kEventWindowClose: {
					// This is our real window; so we have to post to the corresponding view
					// FIXME: Should we doublehash the table to get the real window handle without this loop?
					IDictionaryEnumerator e = WindowMapping.GetEnumerator ();
					while (e.MoveNext ()) {
						if ((IntPtr)e.Value == handle) {
							NativeWindow.WndProc((IntPtr)e.Key, Msg.WM_DESTROY, IntPtr.Zero, IntPtr.Zero);
						}
					}
					return 0;
				}
				case OSXConstants.kEventWindowBoundsChanged: {
					// This is our real window; so we have to resize the corresponding view as well
					// FIXME: Should we doublehash the table to get the real window handle without this loop?
					
					IDictionaryEnumerator e = WindowMapping.GetEnumerator ();
					while (e.MoveNext ()) {
						if ((IntPtr)e.Value == handle) {
							Hwnd hwnd = Hwnd.ObjectFromHandle ((IntPtr) e.Key);
							// Get the bounds of the window
							Rect bounds = new Rect ();
							CheckError (GetWindowBounds (handle, 33, ref bounds), "GetWindowBounds ()");
							HIRect r = new HIRect ();
							
							// Get our frame for the Handle
							CheckError (HIViewGetFrame (hwnd.WholeWindow, ref r), "HIViewGetFrame ()");
							r.size.width = bounds.right-bounds.left;
							r.size.height = bounds.bottom-bounds.top;
							// Set the view to the new size
							CheckError (HIViewSetFrame (hwnd.WholeWindow, ref r), "HIViewSetFrame ()");
							 
							 // Update the hwnd internal size representation
							Size newsize = TranslateQuartzWindowSizeToWindowSize (Control.FromHandle (hwnd.Handle).GetCreateParams (), (int)r.size.width, (int)r.size.height);
							hwnd.x = (int)bounds.left;
							hwnd.y = (int)bounds.top;
							hwnd.width = (int)newsize.Width;
							hwnd.height = (int)newsize.Height;
							PerformNCCalc (hwnd);
							
							// Add the message to the queue
							msg.message = Msg.WM_WINDOWPOSCHANGED;
							msg.hwnd = hwnd.Handle;
							msg.wParam = IntPtr.Zero;
							msg.lParam = IntPtr.Zero;
							MessageQueue.Enqueue (msg);
							
							return 0;
						}
					}
					break;
				}
			}
			return -9874;
		}
				
		private int ProcessMouseEvent (IntPtr inEvent, uint eventKind, IntPtr handle) {
			MSG msg = new MSG ();		
			
			switch (eventKind) {
				case OSXConstants.kEventMouseMoved: {
					// Where is the mouse in global coordinates
					QDPoint pt = new QDPoint ();
					GetEventParameter (inEvent, OSXConstants.EventParamName.kEventParamMouseLocation, OSXConstants.EventParamType.typeQDPoint, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (QDPoint)), IntPtr.Zero, ref pt);
					
					// Where is the mouse in the window
					Rect window_bounds = new Rect ();
					GetWindowBounds (handle, 33, ref window_bounds);
					CGPoint window_pt = new CGPoint ((short) (pt.x - window_bounds.left), (short) (pt.y - window_bounds.top));
					
					IntPtr window_handle = IntPtr.Zero;
					HIViewFindByID (HIViewGetRoot (handle), new HIViewID (OSXConstants.kEventClassWindow, 1), ref window_handle);
					
					// Determine which control was hit
					IntPtr view_handle = IntPtr.Zero;
					HIViewGetSubviewHit (window_handle, ref window_pt, true, ref view_handle);
					
					// Convert the point to view local coordinates
					HIViewConvertPoint (ref window_pt, window_handle, view_handle);
					
					Hwnd hwnd = Hwnd.ObjectFromHandle (view_handle);
					
					if (hwnd == null)
						return -9874;
					
					bool client = (hwnd.ClientWindow == view_handle ? true : false);

					if (GrabWindowHwnd != null) {
						hwnd = GrabWindowHwnd;
						client = true;
					}
					
					// Generate the message
					msg.hwnd = hwnd.Handle;
					msg.message = (client ? Msg.WM_MOUSEMOVE : Msg.WM_NCMOUSEMOVE);
					msg.lParam = (IntPtr) ((ushort)window_pt.y << 16 | (ushort)window_pt.x);
					msg.wParam = GetMousewParam (0);
					mouse_position.X = (int)window_pt.x;
					mouse_position.Y = (int)window_pt.y;
					
					Hover.Hwnd = msg.hwnd;
					Hover.Timer.Enabled = true;
					MessageQueue.Enqueue (msg);
					return -9874;
				}
			}
			return -9874;
		}
					
		private int ProcessControlEvent (IntPtr inEvent, uint eventKind, IntPtr handle) {
			GetEventParameter (inEvent, OSXConstants.EventParamName.kEventParamDirectObject, OSXConstants.EventParamType.typeControlRef, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (IntPtr)), IntPtr.Zero, ref handle);
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			MSG msg = new MSG ();
					
			switch (eventKind) {
				case OSXConstants.kEventControlTrackingAreaEntered: {
					if (hwnd.Handle == handle)
						SetThemeCursor ((uint)hwnd.ClientCursor);
					else
						SetThemeCursor ((uint)hwnd.WholeCursor);
					break;
				}
				case OSXConstants.kEventControlTrackingAreaExited: {
					SetThemeCursor ((uint)ThemeCursor.kThemeArrowCursor);
					break;
				}
				case OSXConstants.kEventControlDraw: {
					
					if(!hwnd.visible || !HIViewIsVisible (handle))
						return 0;

					// Get the dirty area
					HIRect bounds = new HIRect ();
					HIViewGetBounds (handle, ref bounds); 
					
					bool client = (hwnd.ClientWindow == handle ? true : false);

					if (!client && bounds.origin.x >= hwnd.ClientRect.X && bounds.origin.y >= hwnd.ClientRect.Y) {
						// This is a paint on WholeWindow inside the clientRect; we can safely discard this
						return 0;
					}
					
					AddExpose (hwnd, client, (int)bounds.origin.x, (int)bounds.origin.y, (int)bounds.size.width, (int)bounds.size.height);
					if (WindowBackgrounds [hwnd] != null) {
						Color c = (Color)WindowBackgrounds [hwnd];
						IntPtr contextref = IntPtr.Zero;
						GetEventParameter (inEvent, OSXConstants.EventParamName.kEventParamCGContextRef, OSXConstants.EventParamType.typeCGContextRef, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (IntPtr)), IntPtr.Zero, ref contextref);
						CGContextSetRGBFillColor (contextref, (float)c.R/255, (float)c.G/255, (float)c.B/255, (float)c.A/255);
						CGContextFillRect (contextref, bounds);
					}
					
#if OptimizeDrawing
					if (!client && hwnd.nc_expose_pending) {
#else
					if (!client) {
#endif
						switch (hwnd.border_style) {
							case FormBorderStyle.Fixed3D: {
								Graphics g;

								g = Graphics.FromHwnd(hwnd.whole_window);
								if (hwnd.border_static)
									ControlPaint.DrawBorder3D(g, new Rectangle(0, 0, hwnd.Width, hwnd.Height), Border3DStyle.SunkenOuter);
								else
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
					}
			
					return 0;
				}
				case OSXConstants.kEventControlBoundsChanged: {
					// This can happen before our HWND is created so we need to check to make sure its not null
					if (hwnd != null) {
						// Get the bounds
						HIRect bounds = new HIRect ();
						HIViewGetFrame (handle, ref bounds); 
						// Update the hwnd size
						bool client = (hwnd.ClientWindow == handle ? true : false);
						if (!client) {
							hwnd.x = (int)bounds.origin.x;
							hwnd.y = (int)bounds.origin.y;
							hwnd.width = (int)bounds.size.width;
							hwnd.height = (int)bounds.size.height;
						}
						
						// TODO: Do we need to send a paint here or does BoundsChanged make a ControlDraw for the exposed area?
					}							
					return 0;
				}
				case OSXConstants.kEventControlTrack: {
					// get the point that was hit
					QDPoint point = new QDPoint ();
					CheckError (GetEventParameter (inEvent, OSXConstants.EventParamName.kEventParamMouseLocation, OSXConstants.EventParamType.typeQDPoint, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (QDPoint)), IntPtr.Zero, ref point), "GetEventParameter() MouseLocation");
					MouseTrackingResult mousestatus = MouseTrackingResult.kMouseTrackingMouseDown;
					IntPtr modifiers = IntPtr.Zero;
					if (GrabWindowHwnd != null)
						hwnd = GrabWindowHwnd;
					
					// FIXME: This isn't translating properly for DrawReversibleFrame and looses precision
					while (mousestatus != MouseTrackingResult.kMouseTrackingMouseUp) {
						CheckTimers (DateTime.UtcNow);
						if (mousestatus == MouseTrackingResult.kMouseTrackingMouseDragged) {
							QDPoint realpoint = point;
							int x = point.x;
							int y = point.y;
							ScreenToClient (hwnd.Handle, ref x, ref y);
							realpoint.x = (short)x;
							realpoint.y = (short)y;
							NativeWindow.WndProc (hwnd.Handle, Msg.WM_MOUSEMOVE, GetMousewParam (0), (IntPtr) ((ushort)realpoint.y << 16 | (ushort)realpoint.x));
						}
						// Process the rest of the event queue
						while (MessageQueue.Count > 0) {
							msg = (MSG)MessageQueue.Dequeue ();
							NativeWindow.WndProc (msg.hwnd, msg.message, msg.wParam, msg.lParam);
						}
						TrackMouseLocationWithOptions ((IntPtr)(-1), 0, 0.01, ref point, ref modifiers, ref mousestatus);
					}
					
					msg.hwnd = hwnd.Handle;
					
					bool client = (hwnd.ClientWindow == handle ? true : false);
					if (GrabWindowHwnd != null)
						client = true;
					
					
					int wparam = (int)GetMousewParam (0);
					switch (MouseState) {
						case MouseButtons.Left:
							MouseState &= ~MouseButtons.Left;
							msg.message = (client ? Msg.WM_LBUTTONUP : Msg.WM_NCLBUTTONUP);
							wparam &= (int)MsgButtons.MK_LBUTTON;
							break;
						case MouseButtons.Middle:
							MouseState &= ~MouseButtons.Middle;
							msg.message = (client ? Msg.WM_MBUTTONUP : Msg.WM_NCMBUTTONUP);
							wparam &= (int)MsgButtons.MK_MBUTTON;
							break;
						case MouseButtons.Right:
							MouseState &= ~MouseButtons.Right;
							msg.message = (client ? Msg.WM_RBUTTONUP : Msg.WM_NCRBUTTONUP);
							wparam &= (int)MsgButtons.MK_RBUTTON;
							break;
					}
					int x2 = point.x;
					int y2 = point.y;
					if (client)
						ScreenToClient (hwnd.Handle, ref x2, ref y2);
					point.x = (short)x2;
					point.y = (short)y2;

					msg.wParam = (IntPtr)wparam;
						
					msg.lParam = (IntPtr) ((ushort)point.y << 16 | (ushort)point.x);
					mouse_position.X = (int)point.x;
					mouse_position.Y = (int)point.y;
					//NativeWindow.WndProc (msg.hwnd, msg.message, msg.lParam, msg.wParam);
					MessageQueue.Enqueue (msg);
					
					IntPtr window = HIViewGetWindow (hwnd.Handle);
					SetKeyboardFocus (window, hwnd.Handle, 1);

					return 0;
				}
				case OSXConstants.kEventControlContextualMenuClick:
				case OSXConstants.kEventControlClick: {
					// get the point that was hit
					QDPoint point = new QDPoint ();
					CheckError (GetEventParameter (inEvent, OSXConstants.EventParamName.kEventParamMouseLocation, OSXConstants.EventParamType.typeQDPoint, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (QDPoint)), IntPtr.Zero, ref point), "GetEventParameter() MouseLocation");
					QDPoint trackpoint = point;
					int x = point.x;
					int y = point.y;

					bool client = (hwnd.ClientWindow == handle ? true : false);

					if (GrabWindowHwnd != null) {
						hwnd = GrabWindowHwnd;
						client = false;
					}

					if (client)
						ScreenToClient (hwnd.Handle, ref x, ref y);

					point.x = (short)x;
					point.y = (short)y;

					// which button was pressed?
					ushort button = 0;
					GetEventParameter (inEvent, OSXConstants.EventParamName.kEventParamMouseButton, OSXConstants.EventParamType.typeMouseButton, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (ushort)), IntPtr.Zero, ref button);
					if (button == 2) {
						point.x = (short)mouse_position.X;
						point.y = (short)mouse_position.Y;
					}
					
					msg.hwnd = hwnd.Handle;
					
					
					int wparam = (int)GetMousewParam (0);
					switch (button) {
						case 1:
							MouseState |= MouseButtons.Left;
							msg.message = (client ? Msg.WM_LBUTTONDOWN : Msg.WM_NCLBUTTONDOWN);
							wparam |= (int)MsgButtons.MK_LBUTTON;
							break;
						case 2:
							MouseState |= MouseButtons.Right;
							msg.message = (client ? Msg.WM_RBUTTONDOWN : Msg.WM_NCRBUTTONDOWN);
							wparam |= (int)MsgButtons.MK_RBUTTON;
							break;
						case 3:
							MouseState |= MouseButtons.Middle;
							msg.message = (client ? Msg.WM_MBUTTONDOWN : Msg.WM_NCMBUTTONDOWN);
							wparam |= (int)MsgButtons.MK_MBUTTON;
							break;
					}
					msg.wParam = (IntPtr)wparam;
						
					msg.lParam = (IntPtr) ((ushort)point.y << 16 | (ushort)point.x);
					mouse_position.X = (int)point.x;
					mouse_position.Y = (int)point.y;
					NativeWindow.WndProc (msg.hwnd, msg.message, msg.wParam, msg.lParam);
					
					TrackControl (handle, trackpoint, IntPtr.Zero);
					return 0;
				}
				case OSXConstants.kEventControlSetFocusPart: {
					// This handles setting focus
					short pcode = 1;
					GetEventParameter (inEvent, OSXConstants.EventParamName.kEventParamControlPart, OSXConstants.EventParamType.typeControlPartCode, IntPtr.Zero, (uint)Marshal.SizeOf (typeof (short)), IntPtr.Zero, ref pcode);
					switch (pcode) {
						case 0:
						case -1:
						case -2:
							pcode = 0;
							break;
					}
					SetEventParameter (inEvent, OSXConstants.EventParamName.kEventParamControlPart, OSXConstants.EventParamType.typeControlPartCode, (uint)Marshal.SizeOf (typeof (short)), ref pcode);
					return 0;
				}
			}
			return -9874;
		}
		private IntPtr GetMousewParam(int Delta) {
			int	 result = 0;

			if ((MouseState & MouseButtons.Left) != 0) {
				result |= (int)MsgButtons.MK_LBUTTON;
			}

			if ((MouseState & MouseButtons.Middle) != 0) {
				result |= (int)MsgButtons.MK_MBUTTON;
			}

			if ((MouseState & MouseButtons.Right) != 0) {
				result |= (int)MsgButtons.MK_RBUTTON;
			}

			return (IntPtr)result;
		}

		private double NextTimeout ()
		{
			DateTime now = DateTime.UtcNow;
			int timeout = 0x7FFFFFF;
			lock (TimerList) {
				foreach (Timer timer in TimerList) {
					int next = (int) (timer.Expires - now).TotalMilliseconds;
					if (next < 0)
						return 0;
					if (next < timeout)
						timeout = next;
				}
			}
			if (timeout < Timer.Minimum)
				timeout = Timer.Minimum;

			return (double)((double)timeout/1000);
		}
		
		private void CheckTimers (DateTime now)
		{
			lock (TimerList) {
				int count = TimerList.Count;
				if (count == 0)
					return;
				for (int i = 0; i < TimerList.Count; i++) {
					Timer timer = (Timer) TimerList [i];
					if (timer.Enabled && timer.Expires <= now) {
						timer.FireTick ();
						timer.Update (now);
					}
				}
			}
		}

		internal void InvertCaret () {
			IntPtr window = HIViewGetWindow (Caret.Hwnd);
			SetPortWindowPort (window);
			Rect r = new Rect ();
			GetWindowPortBounds (window, ref r);
			r.top += (short)Caret.Y;
			r.left += (short)Caret.X;
			r.bottom = (short)(r.top + Caret.Height);
			r.right = (short)(r.left + Caret.Width);
			InvertRect (ref r);
		}
		
		void SendParentNotify(IntPtr child, Msg cause, int x, int y)
		{	
			Hwnd hwnd;
			
			if (child == IntPtr.Zero) {
				return;
			}
			
			hwnd = Hwnd.GetObjectFromWindow (child);
			
			if (hwnd == null) {
				return;
			}
			
			if (hwnd.Handle == IntPtr.Zero) {
				return;
			}
			
			if (ExStyleSet ((int) hwnd.initial_ex_style, WindowExStyles.WS_EX_NOPARENTNOTIFY)) {
				return;
			}
			
			if (hwnd.Parent == null) {
				return;
			}
			
			if (hwnd.Parent.Handle == IntPtr.Zero) {
				return;
			}

			if (cause == Msg.WM_CREATE || cause == Msg.WM_DESTROY) {
				SendMessage(hwnd.Parent.Handle, Msg.WM_PARENTNOTIFY, Control.MakeParam((int)cause, 0), child);
			} else {
				SendMessage(hwnd.Parent.Handle, Msg.WM_PARENTNOTIFY, Control.MakeParam((int)cause, 0), Control.MakeParam(x, y));
			}
			
			SendParentNotify (hwnd.Parent.Handle, cause, x, y);
		}

		bool StyleSet (int s, WindowStyles ws)
		{
			return (s & (int)ws) == (int)ws;
		}

		bool ExStyleSet (int ex, WindowExStyles exws)
		{
			return (ex & (int)exws) == (int)exws;
		}

		internal static Rectangle TranslateClientRectangleToQuartzClientRectangle (Hwnd hwnd) {
			return TranslateClientRectangleToQuartzClientRectangle (hwnd, Control.FromHandle (hwnd.Handle));
		}

		internal static Rectangle TranslateClientRectangleToQuartzClientRectangle (Hwnd hwnd, Control ctrl) {
			/* From XplatUIX11
			 * If this is a form with no window manager, X is handling all the border and caption painting
			 * so remove that from the area (since the area we set of the window here is the part of the window 
			 * we're painting in only)
			 */
			Rectangle rect = hwnd.ClientRect;
			Form form = ctrl as Form;
			CreateParams cp = null;

			if (form != null)
				cp = form.GetCreateParams ();

			if (form != null && (form.window_manager == null || cp.IsSet (WindowExStyles.WS_EX_TOOLWINDOW))) {
				Hwnd.Borders borders = Hwnd.GetBorders (cp, null);
				Rectangle qrect = rect;
				
				qrect.Y -= borders.top;
				qrect.X -= borders.left;
				qrect.Width += borders.left + borders.right;
				qrect.Height += borders.top + borders.bottom;
				
				rect = qrect;
			}
			
			if (rect.Width < 1 || rect.Height < 1) {
				rect.Width = 1;
				rect.Height = 1;
				rect.X = -5;
				rect.Y = -5;
			}
			
			return rect;
		}

		internal static Size TranslateWindowSizeToQuartzWindowSize (CreateParams cp) {
			return TranslateWindowSizeToQuartzWindowSize (cp, new Size (cp.Width, cp.Height));
		}

		internal static Size TranslateWindowSizeToQuartzWindowSize (CreateParams cp, Size size) {
			/* From XplatUIX11
			 * If this is a form with no window manager, X is handling all the border and caption painting
			 * so remove that from the area (since the area we set of the window here is the part of the window 
			 * we're painting in only)
			 */
			Form form = cp.control as Form;
			if (form != null && (form.window_manager == null || cp.IsSet (WindowExStyles.WS_EX_TOOLWINDOW))) {
				Hwnd.Borders borders = Hwnd.GetBorders (cp, null);
				Size qsize = size;

				qsize.Width -= borders.left + borders.right;
				qsize.Height -= borders.top + borders.bottom - 15;
				
				size = qsize;
			}

			if (size.Height == 0)
				size.Height = 1;
			if (size.Width == 0)
				size.Width = 1;
			return size;
		}
			
		internal static Size TranslateQuartzWindowSizeToWindowSize (CreateParams cp, int width, int height) {
			/* From XplatUIX11
			 * If this is a form with no window manager, X is handling all the border and caption painting
			 * so remove that from the area (since the area we set of the window here is the part of the window 
			 * we're painting in only)
			 */
			Size size = new Size (width, height);
			Form form = cp.control as Form;
			if (form != null && (form.window_manager == null || cp.IsSet (WindowExStyles.WS_EX_TOOLWINDOW))) {
				Hwnd.Borders borders = Hwnd.GetBorders (cp, null);
				Size qsize = size;

				qsize.Width += borders.left + borders.right;
				qsize.Height += borders.top + borders.bottom - 15;
				
				size = qsize;
			}

			return size;
		}

		private void DeriveStyles(int Style, int ExStyle, out FormBorderStyle border_style, out bool border_static, out TitleStyle title_style, out int caption_height, out int tool_caption_height) {

			caption_height = 0;
			tool_caption_height = 0;
			border_static = false;

			if (StyleSet (Style, WindowStyles.WS_CHILD)) {
				if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_CLIENTEDGE)) {
					border_style = FormBorderStyle.Fixed3D;
				} else if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_STATICEDGE)) {
					border_style = FormBorderStyle.Fixed3D;
					border_static = true;
				} else if (!StyleSet (Style, WindowStyles.WS_BORDER)) {
					border_style = FormBorderStyle.None;
				} else {
					border_style = FormBorderStyle.FixedSingle;
				}
				title_style = TitleStyle.None;
				
				if (StyleSet (Style, WindowStyles.WS_CAPTION)) {
					caption_height = 0;
					if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
						title_style = TitleStyle.Tool;
					} else {
						title_style = TitleStyle.Normal;
					}
				}

				if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_MDICHILD)) {
					caption_height = 0;

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
						} else if (ExStyleSet (ExStyle, WindowExStyles.WS_EX_STATICEDGE)) {
							border_style = FormBorderStyle.Fixed3D;
							border_static = true;
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
			DeriveStyles(cp.Style, cp.ExStyle, out hwnd.border_style, out hwnd.border_static, out hwnd.title_style, out hwnd.caption_height, out hwnd.tool_caption_height);
		}
		
		internal void ShowCaret () {
			if (Caret.On)
				return;
			Caret.On = true;
			InvertCaret ();
		}

		internal void HideCaret () {
			if (!Caret.On)
				return;
			Caret.On = false;
			InvertCaret ();
		}
		
		internal void CheckError (int result, string error) {
			if (result != 0)
				throw new Exception ("XplatUIOSX.cs::" + error + "() Carbon subsystem threw an error: " + result);
		}

		internal void CheckError (int result) {
			if (result != 0)
				throw new Exception ("XplatUIOSX.cs::Carbon subsystem threw an error: " + result);
		}

		private void AccumulateDestroyedHandles (Control c, ArrayList list)
		{
			if (c != null) {
				Control[] controls = c.Controls.GetAllControls ();

				if (c.IsHandleCreated && !c.IsDisposed) {
					Hwnd hwnd = Hwnd.ObjectFromHandle(c.Handle);

					list.Add (hwnd);
					CleanupCachedWindows (hwnd);
				}

				for (int  i = 0; i < controls.Length; i ++) {
					AccumulateDestroyedHandles (controls[i], list);
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

			if (Grab.Hwnd == hwnd.Handle) {
				Grab.Hwnd = IntPtr.Zero;
				Grab.Confined = false;
			}

			DestroyCaret (hwnd.Handle);
		}

		private void PerformNCCalc(Hwnd hwnd) {
#if EnableNCArea
			XplatUIWin32.NCCALCSIZE_PARAMS  ncp;
			IntPtr ptr;
			Rectangle rect;

			rect = new Rectangle (0, 0, hwnd.Width, hwnd.Height);

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


			rect = new Rectangle(ncp.rgrc1.left, ncp.rgrc1.top, ncp.rgrc1.right - ncp.rgrc1.left, ncp.rgrc1.bottom - ncp.rgrc1.top);
			hwnd.ClientRect = rect;

			rect = TranslateClientRectangleToQuartzClientRectangle (hwnd);

			if (hwnd.visible) {
				HIRect r = new HIRect (rect.X, rect.Y, rect.Width, rect.Height);
				HIViewSetFrame (hwnd.client_window, ref r);
			}
	
			AddExpose (hwnd, false, 0, 0, hwnd.Width, hwnd.Height);
#endif
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
#if OptimizeDrawing
				if (!hwnd.expose_pending) {
					if (!hwnd.nc_expose_pending) {
#endif
						MSG msg = new MSG ();
						msg.message = Msg.WM_PAINT;
						msg.hwnd = hwnd.Handle;
						msg.lParam = IntPtr.Zero;
						msg.wParam = IntPtr.Zero;
						MessageQueue.Enqueue (msg);
#if OptimizeDrawing
					}
					hwnd.expose_pending = true;
				}
#endif
			} else {
				hwnd.AddNcInvalidArea (x, y, width, height);
#if OptimizeDrawing
				if (!hwnd.nc_expose_pending) {
					if (!hwnd.expose_pending) {
#endif
						MSG msg = new MSG ();
						Rectangle rect = new Rectangle (x, y, width, height);
						Region region = new Region (rect);
						IntPtr hrgn = region.GetHrgn (null); 
						msg.message = Msg.WM_NCPAINT;
						msg.hwnd = hwnd.Handle;
						msg.wParam = hrgn == IntPtr.Zero ? (IntPtr)1 : hrgn;
						msg.refobject = region;
						MessageQueue.Enqueue (msg);
#if OptimizeDrawing
					}
					hwnd.nc_expose_pending = true;
				}
#endif
			}
		}
		#endregion 
		
		#region Public Methods
		internal override void RaiseIdle (EventArgs e)
		{
			if (Idle != null)
				Idle (this, e);
		}

		internal override IntPtr InitializeDriver() {
			return IntPtr.Zero;
		}

		internal override void ShutdownDriver(IntPtr token) {
		}

		internal override void EnableThemes() {
			themes_enabled = true;
		}

		internal override void Activate(IntPtr handle) {
			ActivateWindow (HIViewGetWindow (handle), true);
		}

		internal override void AudibleAlert() {
			throw new NotImplementedException();
		}

		internal override void CaretVisible (IntPtr hwnd, bool visible) {
			if (Caret.Hwnd == hwnd) {
				if (visible) {
					if (Caret.Visible < 1) {
						Caret.Visible++;
						Caret.On = false;
						if (Caret.Visible == 1) {
							ShowCaret ();
							Caret.Timer.Start ();
						}
					}
				} else {
					Caret.Visible--;
					if (Caret.Visible == 0) {
						Caret.Timer.Stop ();
						HideCaret ();
					}
				}
			}
		}
		
		internal override bool CalculateWindowRect(ref Rectangle ClientRect, CreateParams cp, Menu menu, out Rectangle WindowRect) {
			WindowRect = Hwnd.GetWindowRectangle (cp, menu, ClientRect);
			return true;
		}

		internal override void ClientToScreen(IntPtr handle, ref int x, ref int y) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);

			Point point = ConvertClientPointToScreen (hwnd.ClientWindow, new Point (x, y));

			x = point.X;
			y = point.Y;
		}
		
		internal override void MenuToScreen(IntPtr handle, ref int x, ref int y) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);

			Point point = ConvertClientPointToScreen (hwnd.ClientWindow, new Point (x, y));

			x = point.X;
			y = point.Y;
		}

		internal override int[] ClipboardAvailableFormats(IntPtr handle) {
			return null;
		}

		internal override void ClipboardClose(IntPtr handle) {
		}

		internal override int ClipboardGetID(IntPtr handle, string format) {
			return 0;
		}

		internal override IntPtr ClipboardOpen(bool primary_selection) {
			return IntPtr.Zero;
		}

		internal override object ClipboardRetrieve(IntPtr handle, int id, XplatUI.ClipboardToObject converter) {
			throw new NotImplementedException();
		}

		internal override void ClipboardStore(IntPtr handle, object obj, int type, XplatUI.ObjectToClipboard converter) {
			throw new NotImplementedException();
		}
		
		internal override void CreateCaret (IntPtr hwnd, int width, int height) {
			if (Caret.Hwnd != IntPtr.Zero)
				DestroyCaret (Caret.Hwnd);

			Caret.Hwnd = hwnd;
			Caret.Width = width;
			Caret.Height = height;
			Caret.Visible = 0;
			Caret.On = false;
		}
		
		internal override IntPtr CreateWindow(CreateParams cp) {
			Hwnd hwnd;
			Hwnd parent_hwnd = null;
			int X;
			int Y;
			int Width;
			int Height;
			IntPtr ParentHandle;
			IntPtr WindowHandle;
			IntPtr WholeWindow;
			IntPtr ClientWindow;
			IntPtr WholeWindowTracking;
			IntPtr ClientWindowTracking;

			hwnd = new Hwnd ();

			X = cp.X;
			Y = cp.Y;
			Width = cp.Width;
			Height = cp.Height;
			ParentHandle = IntPtr.Zero;
			WindowHandle = IntPtr.Zero;
			WholeWindow = IntPtr.Zero;
			ClientWindow = IntPtr.Zero;
			WholeWindowTracking = IntPtr.Zero;
			ClientWindowTracking = IntPtr.Zero;

			if (Width < 1) Width = 1;	
			if (Height < 1) Height = 1;	

			if (cp.Parent != IntPtr.Zero) {
				parent_hwnd = Hwnd.ObjectFromHandle (cp.Parent);
				ParentHandle = parent_hwnd.client_window;
			} else {
				if (StyleSet (cp.Style, WindowStyles.WS_CHILD)) {
					HIViewFindByID (HIViewGetRoot (FosterParent), new HIViewID (OSXConstants.kEventClassWindow, 1), ref ParentHandle);
				}
			}

			Point next;
			if (cp.control is Form) {
				next = Hwnd.GetNextStackedFormLocation (cp, parent_hwnd);
				X = next.X;
				Y = next.Y;
			}

			hwnd.x = X;
			hwnd.y = Y;
			hwnd.width = Width;
			hwnd.height = Height;
			hwnd.Parent = Hwnd.ObjectFromHandle (cp.Parent);
			hwnd.initial_style = cp.WindowStyle;
			hwnd.initial_ex_style = cp.WindowExStyle;

			if (StyleSet (cp.Style, WindowStyles.WS_DISABLED)) {
				hwnd.enabled = false;
			}

			ClientWindow = IntPtr.Zero;

			Size QWindowSize = TranslateWindowSizeToQuartzWindowSize (cp);
			Rectangle QClientRect = TranslateClientRectangleToQuartzClientRectangle (hwnd, cp.control);

/* FIXME */
			if (ParentHandle == IntPtr.Zero) {
				IntPtr window_view = IntPtr.Zero;
				WindowClass windowklass = WindowClass.kOverlayWindowClass;
				WindowAttributes attributes = WindowAttributes.kWindowCompositingAttribute | WindowAttributes.kWindowStandardHandlerAttribute;
				if (StyleSet (cp.Style, WindowStyles.WS_MINIMIZEBOX)) {
					attributes |= WindowAttributes.kWindowCollapseBoxAttribute;
				}
				if (StyleSet (cp.Style, WindowStyles.WS_MAXIMIZEBOX)) {
					attributes |= WindowAttributes.kWindowResizableAttribute | WindowAttributes.kWindowHorizontalZoomAttribute | WindowAttributes.kWindowVerticalZoomAttribute;
				}
				if (StyleSet (cp.Style, WindowStyles.WS_SYSMENU)) {
					attributes |= WindowAttributes.kWindowCloseBoxAttribute;
				}
				if (StyleSet (cp.Style, WindowStyles.WS_CAPTION)) {
					windowklass = WindowClass.kDocumentWindowClass;
				}
				if (ExStyleSet (cp.ExStyle, WindowExStyles.WS_EX_TOOLWINDOW)) {
					windowklass = WindowClass.kOverlayWindowClass;
					attributes = WindowAttributes.kWindowCompositingAttribute | WindowAttributes.kWindowStandardHandlerAttribute;
				}
					
				Rect rect = new Rect ();
				if (StyleSet (cp.Style, WindowStyles.WS_POPUP)) {
					SetRect (ref rect, (short)X, (short)(Y), (short)(X + QWindowSize.Width), (short)(Y + QWindowSize.Height));
				} else {
					SetRect (ref rect, (short)X, (short)(Y + MenuBarHeight), (short)(X + QWindowSize.Width), (short)(Y + MenuBarHeight + QWindowSize.Height));
				}

				CreateNewWindow (windowklass, attributes, ref rect, ref WindowHandle);
				InstallEventHandler (GetWindowEventTarget (WindowHandle), CarbonEventHandler, (uint)window_events.Length, window_events, WindowHandle, IntPtr.Zero);
				HIViewFindByID (HIViewGetRoot (WindowHandle), new HIViewID (OSXConstants.kEventClassWindow, 1), ref window_view);
				ParentHandle = window_view;
			}

			HIObjectCreate (__CFStringMakeConstantString ("com.apple.hiview"), 0, ref WholeWindow);
			HIObjectCreate (__CFStringMakeConstantString ("com.apple.hiview"), 0, ref ClientWindow);
			InstallEventHandler (GetControlEventTarget (WholeWindow), CarbonEventHandler, (uint)view_events.Length, view_events, WholeWindow, IntPtr.Zero);
			InstallEventHandler (GetControlEventTarget (ClientWindow), CarbonEventHandler, (uint)view_events.Length, view_events, ClientWindow, IntPtr.Zero);
			HIViewChangeFeatures (WholeWindow, 1<<1, 0);
			HIViewChangeFeatures (ClientWindow, 1<<1, 0);
			HIViewNewTrackingArea (WholeWindow, IntPtr.Zero, (UInt64)WholeWindow, ref WholeWindowTracking);
			HIViewNewTrackingArea (ClientWindow, IntPtr.Zero, (UInt64)ClientWindow, ref ClientWindowTracking);
			HIRect WholeRect;
			if (WindowHandle != IntPtr.Zero) {
				WholeRect = new HIRect (0, 0, QWindowSize.Width, QWindowSize.Height);
			} else {
				WholeRect = new HIRect (X, Y, QWindowSize.Width, QWindowSize.Height);
			}
			HIRect ClientRect = new HIRect (QClientRect.X, QClientRect.Y, QClientRect.Width, QClientRect.Height);
			HIViewSetFrame (WholeWindow, ref WholeRect);
			HIViewSetFrame (ClientWindow, ref ClientRect);

			HIViewAddSubview (ParentHandle, WholeWindow);
			HIViewAddSubview (WholeWindow, ClientWindow);

			hwnd.WholeWindow = WholeWindow;
			hwnd.ClientWindow = ClientWindow;

			if (StyleSet (cp.Style, WindowStyles.WS_VISIBLE) || StyleSet (cp.Style, WindowStyles.WS_POPUP)) {
				if (WindowHandle != IntPtr.Zero) {
					WindowMapping [hwnd.Handle] = WindowHandle;
					IntPtr active = GetActive ();
					CheckError (ShowWindow (WindowHandle));
					if (active != IntPtr.Zero)
						Activate (active);
				}
				HIViewSetVisible (WholeWindow, true);
				HIViewSetVisible (ClientWindow, true);
				hwnd.visible = true;
			} else {
				HIViewSetVisible (WholeWindow, false);
				HIViewSetVisible (ClientWindow, false);
				hwnd.visible = false;
			}

			Text (hwnd.Handle, cp.Caption);
			
			SendMessage (hwnd.Handle, Msg.WM_CREATE, (IntPtr)1, IntPtr.Zero /* XXX unused */);
			SendMessage (hwnd.Handle, Msg.WM_SHOWWINDOW, (IntPtr)1, IntPtr.Zero);

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

		[MonoTODO]
		internal override Bitmap DefineStdCursorBitmap (StdCursor id)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		internal override IntPtr DefineCursor(Bitmap bitmap, Bitmap mask, Color cursor_pixel, Color mask_pixel, int xHotSpot, int yHotSpot) {
			return IntPtr.Zero;
		}
		
		internal override IntPtr DefineStdCursor(StdCursor id) {
			switch (id) {
				case StdCursor.AppStarting:
					return (IntPtr)ThemeCursor.kThemeSpinningCursor;
				case StdCursor.Arrow:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
				case StdCursor.Cross:
					return (IntPtr)ThemeCursor.kThemeCrossCursor;
				case StdCursor.Default:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
				case StdCursor.Hand:
					return (IntPtr)ThemeCursor.kThemeOpenHandCursor;
				case StdCursor.Help:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
				case StdCursor.HSplit:
					return (IntPtr)ThemeCursor.kThemeResizeLeftRightCursor;
				case StdCursor.IBeam:
					return (IntPtr)ThemeCursor.kThemeIBeamCursor;
				case StdCursor.No:
					return (IntPtr)ThemeCursor.kThemeNotAllowedCursor;
				case StdCursor.NoMove2D:
					return (IntPtr)ThemeCursor.kThemeNotAllowedCursor;
				case StdCursor.NoMoveHoriz:
					return (IntPtr)ThemeCursor.kThemeNotAllowedCursor;
				case StdCursor.NoMoveVert:
					return (IntPtr)ThemeCursor.kThemeNotAllowedCursor;
				case StdCursor.PanEast:
					return (IntPtr)ThemeCursor.kThemeResizeRightCursor;
				case StdCursor.PanNE:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
				case StdCursor.PanNorth:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
				case StdCursor.PanNW:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
				case StdCursor.PanSE:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
				case StdCursor.PanSouth:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
				case StdCursor.PanSW:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
				case StdCursor.PanWest:
					return (IntPtr)ThemeCursor.kThemeResizeLeftCursor;
				case StdCursor.SizeAll:
					return (IntPtr)ThemeCursor.kThemeResizeLeftRightCursor;
				case StdCursor.SizeNESW:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
				case StdCursor.SizeNS:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
				case StdCursor.SizeNWSE:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
				case StdCursor.SizeWE:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
				case StdCursor.UpArrow:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
				case StdCursor.VSplit:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
				case StdCursor.WaitCursor:
					return (IntPtr)ThemeCursor.kThemeSpinningCursor;
				default:
					return (IntPtr)ThemeCursor.kThemeArrowCursor;
			}
		}
		
		internal override IntPtr DefWndProc(ref Message msg) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (msg.HWnd);
			switch ((Msg)msg.Msg) {
				case Msg.WM_DESTROY: {
					if (WindowMapping [hwnd.Handle] != null)

						Exit ();
					break;
				}
				case Msg.WM_PAINT: {
#if OptimizeDrawing
					hwnd.expose_pending = false;
#endif
					break;
				}
				case Msg.WM_NCPAINT: {
#if OptimizeDrawing
					hwnd.nc_expose_pending = false;
#endif
					break;
				}  
				case Msg.WM_NCCALCSIZE: {
					if (msg.WParam == (IntPtr)1) {
						XplatUIWin32.NCCALCSIZE_PARAMS ncp;
						ncp = (XplatUIWin32.NCCALCSIZE_PARAMS)Marshal.PtrToStructure (msg.LParam, typeof (XplatUIWin32.NCCALCSIZE_PARAMS));

						// Add all the stuff X is supposed to draw.
						Control ctrl = Control.FromHandle (hwnd.Handle);
						if (ctrl != null) {
							Hwnd.Borders rect = Hwnd.GetBorders (ctrl.GetCreateParams (), null);

							ncp.rgrc1.top += rect.top;
							ncp.rgrc1.bottom -= rect.bottom;
							ncp.rgrc1.left += rect.left;
							ncp.rgrc1.right -= rect.right;

							Marshal.StructureToPtr (ncp, msg.LParam, true);
						}
					}
					break;
				}
			}
			return IntPtr.Zero;
		}

		internal override void DestroyCaret (IntPtr hwnd) {
			if (Caret.Hwnd == hwnd) {
				if (Caret.Visible == 1) {
					Caret.Timer.Stop ();
					HideCaret ();
				}
				Caret.Hwnd = IntPtr.Zero;
				Caret.Visible = 0;
				Caret.On = false;
			}
		}
		
		[MonoTODO]
		internal override void DestroyCursor(IntPtr cursor) {
			throw new NotImplementedException ();
		}
	
		internal override void DestroyWindow(IntPtr handle) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (hwnd == null) {
				return;
			}

			SendParentNotify (hwnd.Handle, Msg.WM_DESTROY, int.MaxValue, int.MaxValue);
				
			CleanupCachedWindows (hwnd);

			ArrayList windows = new ArrayList ();

			AccumulateDestroyedHandles (Control.ControlNativeWindow.ControlFromHandle(hwnd.Handle), windows);


			foreach (Hwnd h in windows) {
				SendMessage (h.Handle, Msg.WM_DESTROY, IntPtr.Zero, IntPtr.Zero);
				h.zombie = true;
			}

/*
			if (hwnd.whole_window != IntPtr.Zero)
				CFRelease (hwnd.whole_window);
			if (hwnd.client_window != IntPtr.Zero)
				CFRelease (hwnd.client_window);
*/
			if (WindowMapping [hwnd.Handle] != null) 
				DisposeWindow ((IntPtr)(WindowMapping [hwnd.Handle]));
		}

		internal override IntPtr DispatchMessage(ref MSG msg) {
			return NativeWindow.WndProc(msg.hwnd, msg.message, msg.wParam, msg.lParam);
		}
		
		internal override void DoEvents() {
		}

		internal override void EnableWindow(IntPtr handle, bool Enable) {
			//Like X11 we need not do anything here
		}

		internal override void EndLoop(Thread thread) {
		}

		internal void Exit() {
			GetMessageResult = false;
			//ExitToShell ();
		}
		
		internal override IntPtr GetActive() {
			foreach (DictionaryEntry entry in WindowMapping)
				if (IsWindowActive ((IntPtr)(entry.Value)))
					return (IntPtr)(entry.Key);

			return IntPtr.Zero;
		}

		internal override Region GetClipRegion(IntPtr hwnd) {
			return null;
		}

		[MonoTODO]
		internal override void GetCursorInfo(IntPtr cursor, out int width, out int height, out int hotspot_x, out int hotspot_y) {
			width = 12;
			height = 12;
			hotspot_x = 0;
			hotspot_y = 0;
		}
		
		internal override void GetDisplaySize(out Size size) {
			HIRect bounds = CGDisplayBounds (CGMainDisplayID ());
			size = new Size ((int)bounds.size.width, (int)bounds.size.height);
		}

		internal override IntPtr GetParent(IntPtr handle) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			if (hwnd != null && hwnd.Parent != null) {
				return hwnd.Parent.Handle;
			}
			return IntPtr.Zero;
		}

		internal override IntPtr GetPreviousWindow(IntPtr handle) {
			return HIViewGetPreviousView(handle);
		}
		
		internal override void GetCursorPos(IntPtr handle, out int x, out int y) {
			QDPoint pt = new QDPoint ();
			GetGlobalMouse (ref pt);
			x = pt.x;
			y = pt.y;
		}

		internal override IntPtr GetFocus() {
			return FocusWindow;
		}

		
		internal override bool GetFontMetrics(Graphics g, Font font, out int ascent, out int descent) {
			FontFamily ff = font.FontFamily;
			ascent = ff.GetCellAscent (font.Style);
			descent = ff.GetCellDescent (font.Style);
			return true;
		}
		
		internal override Point GetMenuOrigin(IntPtr handle) {
			Hwnd hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (hwnd != null) {
				return hwnd.MenuOrigin;
			}
			return Point.Empty;
		}

		internal override bool GetMessage(object queue_id, ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax) {
			IntPtr evtRef = IntPtr.Zero;
			IntPtr target = GetEventDispatcherTarget();
			CheckTimers (DateTime.UtcNow);
			ReceiveNextEvent (0, IntPtr.Zero, 0, true, ref evtRef);
			if (evtRef != IntPtr.Zero && target != IntPtr.Zero) {
				SendEventToEventTarget (evtRef, target);
				ReleaseEvent (evtRef);
			}
			
			lock (MessageQueue) {
				if (MessageQueue.Count <= 0) {
					if (Idle != null) 
						Idle (this, EventArgs.Empty);
					else if (TimerList.Count == 0) {
						ReceiveNextEvent (0, IntPtr.Zero, Convert.ToDouble ("0." + Timer.Minimum), true, ref evtRef);
						if (evtRef != IntPtr.Zero && target != IntPtr.Zero) {
							SendEventToEventTarget (evtRef, target);
							ReleaseEvent (evtRef);
						}
					} else {
						ReceiveNextEvent (0, IntPtr.Zero, NextTimeout (), true, ref evtRef);
						if (evtRef != IntPtr.Zero && target != IntPtr.Zero) {
							SendEventToEventTarget (evtRef, target);
							ReleaseEvent (evtRef);
						}
					}
					msg.hwnd = IntPtr.Zero;
					msg.message = Msg.WM_ENTERIDLE;
					return GetMessageResult;
				}
				msg = (MSG) MessageQueue.Dequeue ();
			}
			return GetMessageResult;
		}
		
		[MonoTODO]
		internal override bool GetText(IntPtr handle, out string text) {
			throw new NotImplementedException ();
		}
		
		internal override void GetWindowPos(IntPtr handle, bool is_toplevel, out int x, out int y, out int width, out int height, out int client_width, out int client_height) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			Rectangle rect = hwnd.ClientRect;
			
			x = hwnd.x;
			y = hwnd.y;
			width = hwnd.width;
			height = hwnd.height;

			client_width = rect.Width;
			client_height = rect.Height;
		}
		
		internal override FormWindowState GetWindowState(IntPtr hwnd) {
			IntPtr window = HIViewGetWindow (hwnd);

			if (IsWindowCollapsed (window))
				return FormWindowState.Minimized;
			if (IsWindowInStandardState (window, IntPtr.Zero, IntPtr.Zero))
				return FormWindowState.Maximized;

			return FormWindowState.Normal;
		}
		
		internal override void GrabInfo(out IntPtr handle, out bool GrabConfined, out Rectangle GrabArea) {
			handle = Grab.Hwnd;
			GrabConfined = Grab.Confined;
			GrabArea = Grab.Area;
		}
		
		internal override void GrabWindow(IntPtr handle, IntPtr confine_to_handle) {
			GrabWindowHwnd = Hwnd.ObjectFromHandle (handle);
		}
		
		internal override void UngrabWindow(IntPtr hwnd) {
			GrabWindowHwnd = null;
			Grab.Hwnd = IntPtr.Zero;
			Grab.Confined = false;
		}
		
		internal override void HandleException(Exception e) {
			StackTrace st = new StackTrace(e);
			Console.WriteLine("Exception '{0}'", e.Message+st.ToString());
			Console.WriteLine("{0}{1}", e.Message, st.ToString());
		}
		
		internal override void Invalidate (IntPtr handle, Rectangle rc, bool clear) {
			Hwnd hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (clear) {
				AddExpose (hwnd, true, hwnd.X, hwnd.Y, hwnd.Width, hwnd.Height);
			} else {
				AddExpose (hwnd, true, rc.X, rc.Y, rc.Width, rc.Height);
			} 
		}

		internal override void InvalidateNC (IntPtr handle)
		{
			Hwnd hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			AddExpose (hwnd, false, 0, 0, hwnd.Width, hwnd.Height); 
		}
		
		internal override bool IsEnabled(IntPtr handle) {
			return Hwnd.ObjectFromHandle(handle).Enabled;
		}
		
		internal override bool IsVisible(IntPtr handle) {
			return Hwnd.ObjectFromHandle(handle).visible;
		}
		
		internal override void KillTimer(Timer timer) {
			lock (TimerList) {
				TimerList.Remove(timer);
			}
		}


		[MonoTODO]
		internal override void OverrideCursor(IntPtr cursor) {
			throw new NotImplementedException ();
		}

		internal override PaintEventArgs PaintEventStart(ref Message msg, IntPtr handle, bool client) {
			PaintEventArgs	paint_event;
			Hwnd		hwnd;
			Hwnd		paint_hwnd; 
			
			hwnd = Hwnd.ObjectFromHandle(msg.HWnd);
			if (msg.HWnd == handle) {
				paint_hwnd = hwnd;
			} else {
				paint_hwnd = Hwnd.ObjectFromHandle (handle);
			}
			
			if (Caret.Visible == 1) {
				Caret.Paused = true;
				HideCaret();
			}

			Graphics dc;

			if (client) {
				dc = Graphics.FromHwnd (paint_hwnd.client_window);

				Region clip_region = new Region ();
				clip_region.MakeEmpty();

				foreach (Rectangle r in hwnd.ClipRectangles) {
					clip_region.Union (r);
				}

				if (hwnd.UserClip != null) {
					clip_region.Intersect(hwnd.UserClip);
				}

//				dc.Clip = clip_region;
				paint_event = new PaintEventArgs(dc, hwnd.Invalid);
#if OptimizeDrawing
				hwnd.expose_pending = false;
#endif
				hwnd.ClearInvalidArea();

				hwnd.drawing_stack.Push (paint_event);
				hwnd.drawing_stack.Push (dc);
			} else {
				dc = Graphics.FromHwnd (paint_hwnd.whole_window);

				if (!hwnd.nc_invalid.IsEmpty) {
					dc.SetClip (hwnd.nc_invalid);
					paint_event = new PaintEventArgs(dc, hwnd.nc_invalid);
				} else {
					paint_event = new PaintEventArgs(dc, new Rectangle(0, 0, hwnd.width, hwnd.height));
				}
#if OptimizeDrawing
				hwnd.nc_expose_pending = false;
#endif
				hwnd.ClearNcInvalidArea ();

				hwnd.drawing_stack.Push (paint_event);
				hwnd.drawing_stack.Push (dc);
			}

			return paint_event;
		}
		
		internal override void PaintEventEnd(ref Message msg, IntPtr handle, bool client) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			Graphics dc = (Graphics)hwnd.drawing_stack.Pop();
			dc.Flush ();
			dc.Dispose ();
			
			PaintEventArgs pe = (PaintEventArgs)hwnd.drawing_stack.Pop();
			pe.SetGraphics (null);
			pe.Dispose ();  

			if (Caret.Visible == 1) {
				ShowCaret();
				Caret.Paused = false;
			}
		}
		
		internal override bool PeekMessage(Object queue_id, ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags) {
			return true;
		}

		internal override bool PostMessage (IntPtr hwnd, Msg message, IntPtr wParam, IntPtr lParam) {
			MSG msg = new MSG();
			msg.hwnd = hwnd;
			msg.message = message;
			msg.wParam = wParam;
			msg.lParam = lParam;
			MessageQueue.Enqueue (msg);
			return true;
		}

		internal override void PostQuitMessage(int exitCode) {
			PostMessage (FosterParent, Msg.WM_QUIT, IntPtr.Zero, IntPtr.Zero);
		}

		internal override void RequestAdditionalWM_NCMessages(IntPtr hwnd, bool hover, bool leave) {
		}

		internal override void RequestNCRecalc(IntPtr handle) {
			Hwnd hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (hwnd == null) {
				return;
			}

			PerformNCCalc(hwnd);
			SendMessage(handle, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);
			InvalidateNC(handle);
		}

		[MonoTODO]		
		internal override void ResetMouseHover(IntPtr handle) {
			throw new NotImplementedException();
		}

		internal override void ScreenToClient(IntPtr handle, ref int x, ref int y) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);

			Point point = ConvertScreenPointToClient (hwnd.ClientWindow, new Point (x, y));

			x = point.X;
			y = point.Y;
		}

		internal override void ScreenToMenu(IntPtr handle, ref int x, ref int y) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);

			Point point = ConvertScreenPointToClient (hwnd.WholeWindow, new Point (x, y));

			x = point.X;
			y = point.Y;
		}

		internal override void ScrollWindow(IntPtr handle, Rectangle area, int XAmount, int YAmount, bool clear) {
			HIRect scroll_rect = new HIRect ();
			scroll_rect.origin.x = area.X;
			scroll_rect.origin.y = area.Y;
			scroll_rect.size.width = area.Width;
			scroll_rect.size.height = area.Height;
			HIViewScrollRect (handle, ref scroll_rect, (float)XAmount, (float)-YAmount);
		}
		
		
		internal override void ScrollWindow(IntPtr hwnd, int XAmount, int YAmount, bool clear) {
			HIRect scroll_rect = new HIRect ();
			
			HIViewGetBounds (hwnd, ref scroll_rect);
			HIViewScrollRect (hwnd, ref scroll_rect, (float)XAmount, (float)-YAmount);
		}
		
		[MonoTODO]
		internal override void SendAsyncMethod (AsyncMethodData method) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		internal override IntPtr SendMessage (IntPtr hwnd, Msg message, IntPtr wParam, IntPtr lParam) {
			return NativeWindow.WndProc(hwnd, message, wParam, lParam);
		}
		
		internal override int SendInput(IntPtr hwnd, Queue keys) {
			return 0;
		}


		internal override void SetCaretPos (IntPtr hwnd, int x, int y) {
			if (Caret.Hwnd == hwnd) {
				CGPoint cpt = new CGPoint ();
				cpt.x = x;
				cpt.y = y;
				HIViewConvertPoint (ref cpt, hwnd, IntPtr.Zero);
				Caret.Timer.Stop ();
				HideCaret ();
				Caret.X = (int)cpt.x;
				Caret.Y = (int)cpt.y-23;
				if (Caret.Visible == 1) {
					ShowCaret ();
					Caret.Timer.Start ();
				}
			}
		}

		internal override void SetClipRegion(IntPtr hwnd, Region region) {
			throw new NotImplementedException();
		}
		
		internal override void SetCursor(IntPtr window, IntPtr cursor) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (window);

			if (hwnd.Handle == window)
				hwnd.ClientCursor = cursor;
			else
				hwnd.WholeCursor = cursor;
		}
		
		internal override void SetCursorPos(IntPtr handle, int x, int y) {
			CGDisplayMoveCursorToPoint (CGMainDisplayID (), new CGPoint (x, y));
		}
		
		internal override void SetFocus(IntPtr handle) {
			if (FocusWindow != IntPtr.Zero) {
				PostMessage(FocusWindow, Msg.WM_KILLFOCUS, handle, IntPtr.Zero);
			}
			PostMessage(handle, Msg.WM_SETFOCUS, FocusWindow, IntPtr.Zero);
			FocusWindow = handle;
		}

		internal override void SetIcon(IntPtr handle, Icon icon) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);

			// FIXME: we need to map the icon for active window switches
			if (WindowMapping [hwnd.Handle] != null) {
				if (icon == null) { 
					RestoreApplicationDockTileImage ();
				} else {
					Bitmap		bitmap;
					int		size;
					IntPtr[]	data;
					int		index;
	
					bitmap = new Bitmap (128, 128);
					using (Graphics g = Graphics.FromImage (bitmap)) {
						g.DrawImage (icon.ToBitmap (), 0, 0, 128, 128);
					}
					index = 0;
					size = bitmap.Width * bitmap.Height;
					data = new IntPtr[size];
	
					for (int y = 0; y < bitmap.Height; y++) {
						for (int x = 0; x < bitmap.Width; x++) {
							int pixel = bitmap.GetPixel (x, y).ToArgb ();
							byte a = (byte) ((pixel >> 24) & 0xFF);
							byte r = (byte) ((pixel >> 16) & 0xFF);
							byte g = (byte) ((pixel >> 8) & 0xFF);
							byte b = (byte) (pixel & 0xFF);
							data[index++] = (IntPtr)(a + (r << 8) + (g << 16) + (b << 24));
						}
					}

					IntPtr provider = CGDataProviderCreateWithData (IntPtr.Zero, data, size*4, IntPtr.Zero);
					IntPtr image = CGImageCreate (128, 128, 8, 32, 4*128, CGColorSpaceCreateDeviceRGB (), 4, provider, IntPtr.Zero, 0, 0);
					SetApplicationDockTileImage (image);
				}
			}
		}

		
		internal override void SetModal(IntPtr handle, bool Modal) {
			IntPtr hWnd = HIViewGetWindow (Hwnd.ObjectFromHandle (handle).WholeWindow);
			if (Modal)
				BeginAppModalStateForWindow (hWnd);
			else
				EndAppModalStateForWindow (hWnd);
			return;
		}

		internal override IntPtr SetParent(IntPtr handle, IntPtr parent) {
			IntPtr ParentHandle = IntPtr.Zero;
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			
			hwnd.Parent = Hwnd.ObjectFromHandle (parent);
			if (HIViewGetSuperview (hwnd.whole_window) != IntPtr.Zero) {
				CheckError (HIViewRemoveFromSuperview (hwnd.whole_window), "HIViewRemoveFromSuperview ()");
			}
			if (hwnd.parent == null)
				HIViewFindByID (HIViewGetRoot (FosterParent), new HIViewID (OSXConstants.kEventClassWindow, 1), ref ParentHandle);
			CheckError (HIViewAddSubview (hwnd.parent == null ? ParentHandle : hwnd.Parent.client_window, hwnd.whole_window));
			HIViewPlaceInSuperviewAt (hwnd.whole_window, hwnd.X, hwnd.Y);
			CheckError (HIViewAddSubview (hwnd.whole_window, hwnd.client_window));
			HIViewPlaceInSuperviewAt (hwnd.client_window, hwnd.ClientRect.X, hwnd.ClientRect.Y);
			
			return IntPtr.Zero;
		}
		
		internal override void SetTimer (Timer timer) {
			lock (TimerList) {
				TimerList.Add (timer);
			}
		}
		
		internal override bool SetTopmost(IntPtr hWnd, bool Enabled) {
			HIViewSetZOrder (hWnd, 1, IntPtr.Zero);
			return true;
		}
		
		internal override bool SetOwner(IntPtr hWnd, IntPtr hWndOwner) {
			// TODO: Set window owner. 
			return true;
		}
		
		internal override bool SetVisible(IntPtr handle, bool visible, bool activate) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			object window = WindowMapping [hwnd.Handle];
			if (window != null)
				if (visible)
					ShowWindow ((IntPtr)window);
				else
					HideWindow ((IntPtr)window);
					
			HIViewSetVisible (hwnd.whole_window, visible);
			HIViewSetVisible (hwnd.client_window, visible);
			hwnd.visible = visible;
			return true;
		}
		
		internal override void SetBorderStyle(IntPtr handle, FormBorderStyle border_style) {
			Form form = Control.FromHandle (handle) as Form;
			if (form != null && form.window_manager == null && (border_style == FormBorderStyle.FixedToolWindow ||
				border_style == FormBorderStyle.SizableToolWindow)) {
				form.window_manager = new ToolWindowManager (form);
			}

			RequestNCRecalc(handle);
		}

		internal override void SetMenu(IntPtr handle, Menu menu) {
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			hwnd.menu = menu;

			RequestNCRecalc(handle);
		}
		
		internal override void SetWindowMinMax(IntPtr handle, Rectangle maximized, Size min, Size max) {
		}

		internal override void SetWindowPos(IntPtr handle, int x, int y, int width, int height) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);

			if (hwnd == null) {
				return;
			}

			// Win32 automatically changes negative width/height to 0.
			if (width < 0)
				width = 0;
			if (height < 0)
				height = 0;
				
			// X requires a sanity check for width & height; otherwise it dies
			if (hwnd.zero_sized && width > 0 && height > 0) {
				if (hwnd.visible) {
					HIViewSetVisible(hwnd.WholeWindow, true);
				}
				hwnd.zero_sized = false;
			}

			if ((width < 1) || (height < 1)) {
				hwnd.zero_sized = true;
				HIViewSetVisible(hwnd.WholeWindow, false);
			}

			// Save a server roundtrip (and prevent a feedback loop)
			if ((hwnd.x == x) && (hwnd.y == y) && (hwnd.width == width) && (hwnd.height == height)) {
				return;
			}

			if (!hwnd.zero_sized) {
				hwnd.x = x;
				hwnd.y = y;
				hwnd.width = width;
				hwnd.height = height;
				SendMessage(hwnd.client_window, Msg.WM_WINDOWPOSCHANGED, IntPtr.Zero, IntPtr.Zero);

				Control ctrl = Control.FromHandle (handle);
				Size TranslatedSize = TranslateWindowSizeToQuartzWindowSize (ctrl.GetCreateParams (), new Size (width, height));
				Rect rect = new Rect ();

				if (WindowMapping [hwnd.Handle] != null) {
					SetRect (ref rect, (short)x, (short)(y+MenuBarHeight), (short)(x+TranslatedSize.Width), (short)(y+MenuBarHeight+TranslatedSize.Height));
					SetWindowBounds ((IntPtr) WindowMapping [hwnd.Handle], 33, ref rect);
					HIRect frame_rect = new HIRect (0, 0, TranslatedSize.Width, TranslatedSize.Height);
					HIViewSetFrame (hwnd.whole_window, ref frame_rect);
				} else {
					HIRect frame_rect = new HIRect (x, y, TranslatedSize.Width, TranslatedSize.Height);
					HIViewSetFrame (hwnd.whole_window, ref frame_rect);
				}
				PerformNCCalc(hwnd);
			}

			hwnd.x = x;
			hwnd.y = y;
			hwnd.width = width;
			hwnd.height = height;
		}
		
		internal override void SetWindowState(IntPtr hwnd, FormWindowState state) {
			IntPtr window = HIViewGetWindow (hwnd);

			switch (state) {
				case FormWindowState.Minimized: {
					CollapseWindow (window, true);
					break;
				}
				case FormWindowState.Normal: {
					ZoomWindow (window, 7, false);
					break;
				}
				case FormWindowState.Maximized: {
					ZoomWindow (window, 8, false);
					break;
				}
			}
		}
		
		internal override void SetWindowStyle(IntPtr handle, CreateParams cp) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			SetHwndStyles(hwnd, cp);
			
			if (WindowMapping [hwnd.Handle] != null) {
				WindowAttributes attributes = WindowAttributes.kWindowCompositingAttribute | WindowAttributes.kWindowStandardHandlerAttribute;
				if ((cp.Style & ((int)WindowStyles.WS_MINIMIZEBOX)) != 0) { 
					attributes |= WindowAttributes.kWindowCollapseBoxAttribute;
				}
				if ((cp.Style & ((int)WindowStyles.WS_MAXIMIZEBOX)) != 0) {
					attributes |= WindowAttributes.kWindowResizableAttribute | WindowAttributes.kWindowHorizontalZoomAttribute | WindowAttributes.kWindowVerticalZoomAttribute;
				}
				if ((cp.Style & ((int)WindowStyles.WS_SYSMENU)) != 0) {
					attributes |= WindowAttributes.kWindowCloseBoxAttribute;
				}
				if ((cp.ExStyle & ((int)WindowExStyles.WS_EX_TOOLWINDOW)) != 0) {
					attributes = WindowAttributes.kWindowStandardHandlerAttribute | WindowAttributes.kWindowCompositingAttribute;
				}

				WindowAttributes outAttributes = WindowAttributes.kWindowNoAttributes;
				GetWindowAttributes ((IntPtr)WindowMapping [hwnd.Handle], ref outAttributes);
				ChangeWindowAttributes ((IntPtr)WindowMapping [hwnd.Handle], attributes, outAttributes);
			}
		}

		internal override void SetWindowTransparency(IntPtr handle, double transparency, Color key) {
		}

		internal override double GetWindowTransparency(IntPtr handle)
		{
			return 1.0;
		}

		internal override TransparencySupport SupportsTransparency() {
			return TransparencySupport.None;
		}
		
		internal override bool SetZOrder(IntPtr handle, IntPtr after_handle, bool Top, bool Bottom) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			
			if (Top) {
				HIViewSetZOrder (hwnd.whole_window, 2, IntPtr.Zero);
				return true;
			} else if (!Bottom) {
				Hwnd after_hwnd = Hwnd.ObjectFromHandle (after_handle);
				HIViewSetZOrder (hwnd.whole_window, 2, after_hwnd.whole_window);
			} else {
				HIViewSetZOrder (hwnd.whole_window, 1, IntPtr.Zero);
				return true;
			}
			return false;
		}

		internal override void ShowCursor(bool show) {
			if (show)
				CGDisplayShowCursor (CGMainDisplayID ());
			else
				CGDisplayHideCursor (CGMainDisplayID ());
		}

		internal override object StartLoop(Thread thread) {
			return new object ();
		}
		
		[MonoTODO]
		internal override bool SystrayAdd(IntPtr hwnd, string tip, Icon icon, out ToolTip tt) {
			throw new NotImplementedException();
		}

		[MonoTODO]
		internal override bool SystrayChange(IntPtr hwnd, string tip, Icon icon, ref ToolTip tt) {
			throw new NotImplementedException();
		}

		[MonoTODO]
		internal override void SystrayRemove(IntPtr hwnd, ref ToolTip tt) {
			throw new NotImplementedException();
		}

#if NET_2_0
		[MonoTODO]
		internal override void SystrayBalloon(IntPtr hwnd, int timeout, string title, string text, ToolTipIcon icon)
		{
			throw new NotImplementedException ();
		}
#endif
		
		internal override bool Text(IntPtr handle, string text) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			if (WindowMapping [hwnd.Handle] != null) {
				CheckError (SetWindowTitleWithCFString ((IntPtr)(WindowMapping [hwnd.Handle]), __CFStringMakeConstantString (text)));
			}
			CheckError (SetControlTitleWithCFString (hwnd.whole_window, __CFStringMakeConstantString (text)));
			CheckError (SetControlTitleWithCFString (hwnd.client_window, __CFStringMakeConstantString (text)));
			return true;
		}
		
		internal override void UpdateWindow(IntPtr handle) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (handle);
			
#if OptimizeDrawing
			if (hwnd.visible && HIViewIsVisible (handle) && !hwnd.expose_pending) { 
#else
			if (hwnd.visible && HIViewIsVisible (handle)) {
#endif
				MSG msg = new MSG ();
				msg.message = Msg.WM_PAINT;
				msg.hwnd = hwnd.Handle;
				msg.lParam = IntPtr.Zero;
				msg.wParam = IntPtr.Zero;
				MessageQueue.Enqueue (msg);
			}
		}
		
		internal override bool TranslateMessage(ref MSG msg) {
			Hwnd hwnd = Hwnd.ObjectFromHandle (msg.hwnd);
					
			switch (msg.message) {
				case Msg.WM_MOUSEMOVE: {
					// We're grabbed
					if (GrabWindowHwnd != null) {
						if (GrabWindowHwnd.Handle != hwnd.Handle) {
							return false;
						}
					} else {
						if (MouseWindow != null) {
							if (MouseWindow.Handle != hwnd.Handle) {
								PostMessage (MouseWindow.Handle, Msg.WM_MOUSELEAVE, IntPtr.Zero, IntPtr.Zero);
								PostMessage (hwnd.Handle, Msg.WM_MOUSE_ENTER, IntPtr.Zero, IntPtr.Zero);
								MouseWindow = hwnd;
							}
						} else {
							MouseWindow = hwnd;
						}
					}
					break;
				}
				case Msg.WM_SETFOCUS: {
					break;	 
				}					
				
			}
			
			return Keyboard.TranslateMessage (ref msg);
		}
		
		#region Reversible regions
		/* 
		 * Quartz has no concept of XOR drawing due to its compositing nature
		 * We fake this by mapping a overlay window on the first draw and mapping it on the second.
		 * This has some issues with it because its POSSIBLE for ControlPaint.DrawReversible* to actually
		 * reverse two regions at once.  We dont do this in MWF, but this behaviour woudn't work.
		 * We could in theory cache the Rectangle/Color combination to handle this behaviour.
		 *
		 * PROBLEMS: This has some flicker / banding
		 */
		internal void SizeReversibleWindow (Rectangle rect) {
			Rect qrect = new Rect ();

			SetRect (ref qrect, (short)rect.X, (short)rect.Y, (short)(rect.X+rect.Width), (short)(rect.Y+rect.Height));

			SetWindowBounds (ReverseWindow, 33, ref qrect);
		}

		internal override void DrawReversibleLine(Point start, Point end, Color backColor) {
			throw new NotImplementedException();
		}

		internal override void FillReversibleRectangle (Rectangle rectangle, Color backColor) {
			throw new NotImplementedException();
		}

		internal override void DrawReversibleFrame (Rectangle rectangle, Color backColor, FrameStyle style) {
			throw new NotImplementedException();
		}

		internal override void DrawReversibleRectangle(IntPtr handle, Rectangle rect, int line_width) {
			Rectangle size_rect = rect;
			int new_x = 0;
			int new_y = 0;

			if (ReverseWindowMapped) {
				HideWindow (ReverseWindow);
				ReverseWindowMapped = false;
			} else {
				ClientToScreen(handle, ref new_x, ref new_y);

				size_rect.X += new_x;
				size_rect.Y += new_y;

				SizeReversibleWindow (size_rect);
				ShowWindow (ReverseWindow);

				rect.X = 0;
				rect.Y = 0;
				rect.Width -= 1;
				rect.Height -= 1;

				Graphics g = Graphics.FromHwnd (HIViewGetRoot (ReverseWindow));

				for (int i = 0; i < line_width; i++) {
					g.DrawRectangle (ThemeEngine.Current.ResPool.GetPen (Color.Black), rect);
					rect.X += 1;
					rect.Y += 1;
					rect.Width -= 1;
					rect.Height -= 1;
				}
	
				g.Flush ();
				g.Dispose ();
				
				ReverseWindowMapped = true;
			}
		}
		#endregion

		[MonoTODO]
		internal override SizeF GetAutoScaleSize(Font font) {
			throw new NotImplementedException();
		}

		internal override Point MousePosition {
			get {
				return mouse_position;
			}
		}
		#endregion
		
		#region System information
		internal override int KeyboardSpeed { get{ throw new NotImplementedException(); } } 
		internal override int KeyboardDelay { get{ throw new NotImplementedException(); } } 

		internal override int CaptionHeight {
			get {
				return 19;
			}
		}

		internal override  Size CursorSize { get{ throw new NotImplementedException(); } }
		internal override  bool DragFullWindows { get{ throw new NotImplementedException(); } }
		internal override  Size DragSize {
			get {
				return new Size(4, 4);
			}
		}

		internal override  Size FrameBorderSize {
			get {
				return new Size (2, 2);
			}
		}

		internal override  Size IconSize { get{ throw new NotImplementedException(); } }
		internal override  Size MaxWindowTrackSize { get{ throw new NotImplementedException(); } }
		internal override bool MenuAccessKeysUnderlined {
			get {
				return false;
			}
		}
		internal override Size MinimizedWindowSize { get{ throw new NotImplementedException(); } }
		internal override Size MinimizedWindowSpacingSize { get{ throw new NotImplementedException(); } }
		internal override Size MinimumWindowSize { get{ throw new NotImplementedException(); } }
		internal override Size MinWindowTrackSize { get{ throw new NotImplementedException(); } }
		internal override Size SmallIconSize { get{ throw new NotImplementedException(); } }
		internal override int MouseButtonCount { get{ throw new NotImplementedException(); } }
		internal override bool MouseButtonsSwapped { get{ throw new NotImplementedException(); } }
		internal override bool MouseWheelPresent { get{ throw new NotImplementedException(); } }

		internal override Rectangle VirtualScreen {
			get {
				return WorkingArea;
			}
		}

		internal override Rectangle WorkingArea { 
			get { 
				HIRect bounds = CGDisplayBounds (CGMainDisplayID ());
				return new Rectangle ((int)bounds.origin.x, (int)bounds.origin.y, (int)bounds.size.width, (int)bounds.size.height);
			}
		}
		internal override bool ThemesEnabled {
			get {
				return XplatUIOSX.themes_enabled;
			}
		}
 

		#endregion
		
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewSetNeedsDisplayInRegion (IntPtr view, IntPtr rgn, bool needsDisplay);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewGetSubviewHit (IntPtr contentView, ref CGPoint point, bool tval, ref IntPtr outPtr);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewGetViewForMouseEvent (IntPtr inView, IntPtr inEvent, ref IntPtr outView);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewConvertPoint (ref CGPoint point, IntPtr pView, IntPtr cView);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewChangeFeatures (IntPtr aView, ulong bitsin, ulong bitsout);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewFindByID (IntPtr rootWnd, HIViewID id, ref IntPtr outPtr);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr HIViewGetRoot (IntPtr hWnd);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIObjectCreate (IntPtr cfStr, uint what, ref IntPtr hwnd);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewSetNeedsDisplay (IntPtr viewHnd, bool update);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewGetFrame (IntPtr viewHnd, ref HIRect rect);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewSetFrame (IntPtr viewHnd, ref HIRect rect);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewPlaceInSuperviewAt (IntPtr view, float x, float y);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewAddSubview (IntPtr parentHnd, IntPtr childHnd);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr HIViewGetNextView (IntPtr aView);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr HIViewGetPreviousView (IntPtr aView);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr HIViewGetFirstSubview (IntPtr aView);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr HIViewGetSuperview (IntPtr aView);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewRemoveFromSuperview (IntPtr aView);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewSetVisible (IntPtr vHnd, bool visible);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern bool HIViewIsVisible (IntPtr vHnd);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewGetBounds (IntPtr vHnd, ref HIRect r);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewScrollRect (IntPtr vHnd, ref HIRect rect, float x, float y);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewScrollRect (IntPtr vHnd, Rect rect, float x, float y);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewSetZOrder (IntPtr hWnd, int cmd, IntPtr oHnd);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewSetBoundsOrigin (IntPtr vHnd, float x, float y);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewConvertRect (ref HIRect r, IntPtr a, IntPtr b);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HIViewNewTrackingArea (IntPtr inView, IntPtr inShape, UInt64 inID, ref IntPtr outRef);
		
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void ScrollRect (ref Rect r, short dh, short dv, IntPtr rgnHandle);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void SetRect (ref Rect r, short left, short top, short right, short bottom);

		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int InstallEventHandler (IntPtr window, CarbonEventDelegate handlerProc, uint numtypes, EventTypeSpec [] typeList, IntPtr userData, IntPtr handlerRef);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr HIViewGetWindow (IntPtr aView);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int ActivateWindow (IntPtr windowHnd, bool inActivate);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern bool IsWindowActive (IntPtr windowHnd);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int SetKeyboardFocus (IntPtr windowHdn, IntPtr cntrlHnd, short partcode);

		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr GetWindowEventTarget (IntPtr window);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr GetControlEventTarget (IntPtr aControl);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr GetEventDispatcherTarget ();
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int SendEventToEventTarget (IntPtr evt, IntPtr target);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int ReleaseEvent (IntPtr evt);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int ReceiveNextEvent (uint evtCount, IntPtr evtTypes, double timeout, bool processEvt, ref IntPtr evt);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern uint GetEventClass (IntPtr eventRef);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern uint GetEventKind (IntPtr eventRef);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GetEventParameter (IntPtr evt, OSXConstants.EventParamName inName, OSXConstants.EventParamType inType, IntPtr outActualType, uint bufSize, IntPtr outActualSize, ref IntPtr outData);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GetEventParameter (IntPtr evt, OSXConstants.EventParamName inName, OSXConstants.EventParamType inType, IntPtr outActualType, uint bufSize, IntPtr outActualSize, ref ushort outData);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GetEventParameter (IntPtr evt, OSXConstants.EventParamName inName, OSXConstants.EventParamType inType, IntPtr outActualType, uint bufSize, IntPtr outActualSize, ref short outData);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GetEventParameter (IntPtr evt, OSXConstants.EventParamName inName, OSXConstants.EventParamType inType, IntPtr outActualType, uint bufSize, IntPtr outActualSize, ref QDPoint outData);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int SetEventParameter (IntPtr evt, OSXConstants.EventParamName inName, OSXConstants.EventParamType inType, uint bufSize, ref short outData);
		//[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		//static extern int SetEventParameter (IntPtr evt, OSXConstants.EventParamName inName, OSXConstants.EventParamType inType, uint bufSize, ref IntPtr outData);

		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGContextFlush (IntPtr cgc);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int CGContextFillRect (IntPtr cgc, HIRect r);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern CGAffineTransform CGContextGetTextMatrix (IntPtr cgContext);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int CGContextSetTextMatrix (IntPtr cgContext, CGAffineTransform ctm);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int CGContextSetRGBFillColor (IntPtr cgContext, float r, float g, float b, float alpha);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int CGContextSetRGBStrokeColor (IntPtr cgContext, float r, float g, float b, float alpha);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int CGContextSetTextDrawingMode (IntPtr cgContext, int drawingMode);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int CGContextSelectFont (IntPtr cgContext, string fontName, float size, int textEncoding);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int CGContextShowTextAtPoint (IntPtr cgContext, float x, float y, string text, int length);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int CGContextClipToRect (IntPtr cgContext, HIRect clip);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CreateCGContextForPort (IntPtr port, ref IntPtr cgc);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern bool IsWindowCollapsed (IntPtr hWnd);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern bool IsWindowInStandardState (IntPtr hWnd, IntPtr a, IntPtr b);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CollapseWindow (IntPtr hWnd, bool collapse);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void ZoomWindow (IntPtr hWnd, short partCode, bool front);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int GetWindowAttributes (IntPtr hWnd, ref WindowAttributes outAttributes);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int ChangeWindowAttributes (IntPtr hWnd, WindowAttributes inAttributes, WindowAttributes outAttributes);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr GetWindowPort (IntPtr hWnd);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int SetPortWindowPort (IntPtr hWnd);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int GetGlobalMouse (ref QDPoint outData);
		//[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		//static extern int GlobalToLocal (ref QDPoint outData);
		//[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		//static extern int LocalToGlobal (ref QDPoint outData);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int TrackControl (IntPtr handle, QDPoint point, IntPtr data);
		
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int BeginAppModalStateForWindow (IntPtr window);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int EndAppModalStateForWindow (IntPtr window);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int CreateNewWindow (WindowClass klass, WindowAttributes attributes, ref Rect r, ref IntPtr window);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int DisposeWindow (IntPtr wHnd);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void BringToFront (IntPtr wHnd);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int ShowWindow (IntPtr wHnd);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int HideWindow (IntPtr wHnd);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int SetWindowBounds (IntPtr wHnd, uint reg, ref Rect rect);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int GetWindowPortBounds (IntPtr wHnd, ref Rect rect);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int GetWindowBounds (IntPtr wHnd, uint reg, ref Rect rect);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int InvertRect (ref Rect r);

		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int SetControlTitleWithCFString (IntPtr hWnd, IntPtr titleCFStr);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int SetWindowTitleWithCFString (IntPtr hWnd, IntPtr titleCFStr);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern IntPtr __CFStringMakeConstantString (string cString);
		
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGContextRestoreGState (IntPtr ctx);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGContextSaveGState (IntPtr ctx);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGContextTranslateCTM (IntPtr ctx, double tx, double ty);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern void CGContextScaleCTM (IntPtr ctx, double tx, double ty);

		//[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		//static extern int SetWindowContentColor (IntPtr hWnd, ref RGBColor backColor);
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		static extern int TrackMouseLocationWithOptions (IntPtr port, int options, double eventtimeout, ref QDPoint point, ref IntPtr modifier, ref MouseTrackingResult status);
		//[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		//static extern int CreateMouseTrackingRegion (IntPtr windowref, IntPtr rgn, IntPtr clip, int options, MouseTrackingRegionID rid, IntPtr refcon, IntPtr evttargetref, ref IntPtr mousetrackref);
		//[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		//static extern int ReleaseMouseTrackingRegion (IntPtr region_handle);
		
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal static extern int CFRelease (IntPtr wHnd);
		
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static IntPtr NewRgn ();
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static void CopyRgn (IntPtr srcrgn, IntPtr destrgn);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static void SetRectRgn (IntPtr rgn, short left, short top, short right, short bottom);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static void DisposeRgn (IntPtr rgn);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static void ExitToShell ();
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static short GetMBarHeight ();
		
		#region Cursor imports
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static HIRect CGDisplayBounds (IntPtr displayID);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static IntPtr CGMainDisplayID ();
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static void CGDisplayShowCursor (IntPtr display);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static void CGDisplayHideCursor (IntPtr display);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static void CGDisplayMoveCursorToPoint (IntPtr display, CGPoint point);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static void SetThemeCursor (uint inCursor);
		#endregion

		#region Windowing imports
		[DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		private extern static int GetCurrentProcess (ref ProcessSerialNumber psn);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static int TransformProcessType (ref ProcessSerialNumber psn, uint type);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static int SetFrontProcess (ref ProcessSerialNumber psn);
		#endregion

		#region Dock tile imports
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static IntPtr CGColorSpaceCreateDeviceRGB();
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static IntPtr CGDataProviderCreateWithData (IntPtr info, IntPtr [] data, int size, IntPtr releasefunc);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static IntPtr CGImageCreate (int width, int height, int bitsPerComponent, int bitsPerPixel, int bytesPerRow, IntPtr colorspace, uint bitmapInfo, IntPtr provider, IntPtr decode, int shouldInterpolate, int intent);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static void SetApplicationDockTileImage(IntPtr imageRef);
		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
		internal extern static void RestoreApplicationDockTileImage();
		#endregion
	}
}
