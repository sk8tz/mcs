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
// $Revision: 1.27 $
// $Modtime: $
// $Log: XplatUIWin32.cs,v $
// Revision 1.27  2004/08/21 20:51:27  pbartok
// - Added method to get default display size
//
// Revision 1.26  2004/08/21 20:23:56  pbartok
// - Added method to query current grab state
// - Added argument to allow confining a grab to a window
//
// Revision 1.25  2004/08/21 18:35:38  pbartok
// - Fixed bug with Async message handling
// - Implemented getting the ModifierKeys
//
// Revision 1.24  2004/08/21 17:31:21  pbartok
// - Drivers now return proper mouse state
//
// Revision 1.23  2004/08/20 20:39:07  pbartok
// - Added jackson's Async code from X11 to Win32
//
// Revision 1.22  2004/08/20 20:02:45  pbartok
// - Added method for setting the background color
// - Added handling for erasing the window background
//
// Revision 1.21  2004/08/20 19:14:35  jackson
// Expose functionality to send async messages through the driver
//
// Revision 1.20  2004/08/20 01:37:47  pbartok
// - Added generation of MouseEnter, MouseLeave and MouseHover events
// - Added cleanup on EndPaint
//
// Revision 1.19  2004/08/18 19:16:53  jordi
// Move colors to a table
//
// Revision 1.18  2004/08/17 21:24:03  pbartok
// - Finished IsVisible
// - Added Win32GetWindowPlacement
//
// Revision 1.17  2004/08/13 21:42:15  pbartok
// - Changed signature for GetCursorPos
//
// Revision 1.16  2004/08/13 19:00:15  jordi
// implements PointToClient (ScreenToClient)
//
// Revision 1.15  2004/08/13 18:53:57  pbartok
// - Changed GetWindowPos to also provide client area size
// - Fixed broken prototypes for several win32 functions
//
// Revision 1.14  2004/08/12 22:59:03  pbartok
// - Implemented method to get current mouse position
//
// Revision 1.13  2004/08/11 22:20:59  pbartok
// - Signature fixes
//
// Revision 1.12  2004/08/11 19:41:38  jordi
// Fixes ClientRect
//
// Revision 1.11  2004/08/11 19:19:44  pbartok
// - We had SetWindowPos and MoveWindow to set window positions and size,
//   removed MoveWindow. We have GetWindowPos, so it made sense to keep
//   SetWindowPos as matching counterpart
// - Added some X11 sanity checking
//
// Revision 1.10  2004/08/11 18:55:46  pbartok
// - Added method to calculate difference between decorated window and raw
//   client area
//
// Revision 1.9  2004/08/10 18:47:16  jordi
// Calls InvalidateRect before UpdateWindow
//
// Revision 1.8  2004/08/10 17:36:17  pbartok
// - Implemented several methods
//
// Revision 1.7  2004/08/09 20:55:59  pbartok
// - Removed Run method, was only required for initial development
//
// Revision 1.6  2004/08/09 20:51:25  pbartok
// - Implemented GrabWindow/ReleaseWindow methods to allow pointer capture
//
// Revision 1.5  2004/08/09 16:05:16  jackson
// These properties are handled by the theme now.
//
// Revision 1.4  2004/08/06 15:53:39  jordi
// X11 keyboard navigation
//
// Revision 1.3  2004/08/04 20:11:24  pbartok
// - Added Invalidate handling
//
// Revision 1.2  2004/07/21 16:19:17  jordi
// LinkLabel control implementation
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

/// Win32 Version
namespace System.Windows.Forms {
	internal class XplatUIWin32 : XplatUIDriver {
		#region Local Variables
		private static XplatUIWin32	instance;
		private static int		ref_count;
		private static IntPtr		FosterParent;

		internal static MouseButtons	mouse_state;
		internal static Point		mouse_position;
		internal static bool		grab_confined;
		internal static IntPtr		grab_hwnd;
		internal static Rectangle	grab_area;
		internal static WndProc		wnd_proc;
		internal static IntPtr		prev_mouse_hwnd;

		internal static bool		themes_enabled;
		private static Hashtable	handle_data;
		#endregion	// Local Variables

		#region Private Structs
		[StructLayout(LayoutKind.Sequential)]
		private struct WNDCLASS {
			internal int		style;
			internal WndProc	lpfnWndProc;
			internal int		cbClsExtra;
			internal int		cbWndExtra;
			internal IntPtr		hInstance;
			internal IntPtr		hIcon;
			internal IntPtr		hCursor;
			internal IntPtr		hbrBackground;
			internal string		lpszMenuName;
			internal string		lpszClassName;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct RECT {
			internal int		left;
			internal int		top;
			internal int		right;
			internal int		bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct POINT {
			internal int		x;
			internal int		y;
		}

		internal enum WindowPlacementFlags {
			SW_HIDE			= 0,
			SW_SHOWNORMAL       	= 1,
			SW_NORMAL           	= 1,
			SW_SHOWMINIMIZED    	= 2,
			SW_SHOWMAXIMIZED    	= 3,
			SW_MAXIMIZE         	= 3,
			SW_SHOWNOACTIVATE   	= 4,
			SW_SHOW             	= 5,
			SW_MINIMIZE         	= 6,
			SW_SHOWMINNOACTIVE  	= 7,
			SW_SHOWNA           	= 8,
			SW_RESTORE          	= 9,
			SW_SHOWDEFAULT      	= 10,
			SW_FORCEMINIMIZE    	= 11,
			SW_MAX              	= 11
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct WINDOWPLACEMENT {
			internal uint			length;
			internal uint			flags;
			internal WindowPlacementFlags	showCmd;
			internal POINT			ptMinPosition;
			internal POINT			ptMaxPosition;
			internal RECT			rcNormalPosition;
		}

		[Flags]
		private enum TMEFlags {
			TME_HOVER		= 0x00000001,
			TME_LEAVE		= 0x00000002,
			TME_QUERY		= unchecked((int)0x40000000),
			TME_CANCEL		= unchecked((int)0x80000000)
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct TRACKMOUSEEVENT {
			internal int		size;
			internal TMEFlags	dwFlags;
			internal IntPtr		hWnd;
			internal int		dwHoverTime;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct PAINTSTRUCT {
			internal IntPtr		hdc;
			internal int		fErase;
			internal RECT		rcPaint;
			internal int		fRestore;
			internal int		fIncUpdate;
			internal int		Reserved1;
			internal int		Reserved2;
			internal int		Reserved3;
			internal int		Reserved4;
			internal int		Reserved5;
			internal int		Reserved6;
			internal int		Reserved7;
			internal int		Reserved8;
		}

		internal enum ClassStyle {
			CS_VREDRAW			= 0x00000001,
			CS_HREDRAW			= 0x00000002,
			CS_KEYCVTWINDOW			= 0x00000004,
			CS_DBLCLKS			= 0x00000008,
			CS_OWNDC			= 0x00000020,
			CS_CLASSDC			= 0x00000040,
			CS_PARENTDC			= 0x00000080,
			CS_NOKEYCVT			= 0x00000100,
			CS_NOCLOSE			= 0x00000200,
			CS_SAVEBITS			= 0x00000800,
			CS_BYTEALIGNCLIENT		= 0x00001000,
			CS_BYTEALIGNWINDOW		= 0x00002000,
			CS_GLOBALCLASS			= 0x00004000,
			CS_IME				= 0x00010000
		}

		internal enum PeekMessageFlags {
			PM_NOREMOVE			= 0x00000000,
			PM_REMOVE			= 0x00000001,
			PM_NOYIELD			= 0x00000002
		}


		internal enum VirtualKeys {
			VK_LBUTTON		= 0x01,
			VK_RBUTTON              = 0x02,
			VK_CANCEL		= 0x03,
			VK_MBUTTON              = 0x04,
			VK_XBUTTON1             = 0x05,
			VK_XBUTTON2             = 0x06,
			VK_BACK			= 0x08,
			VK_TAB			= 0x09,
			VK_CLEAR		= 0x0C,
			VK_RETURN		= 0x0D,
			VK_SHIFT		= 0x10,
			VK_CONTROL		= 0x11,
			VK_MENU			= 0x12,
			VK_CAPITAL		= 0x14,
			VK_ESCAPE		= 0x1B,
			VK_SPACE		= 0x20,
			VK_PRIOR		= 0x21,
			VK_NEXT			= 0x22,
			VK_END			= 0x23,
			VK_HOME			= 0x24,
			VK_LEFT			= 0x25,
			VK_UP			= 0x26,
			VK_RIGHT		= 0x27,
			VK_DOWN			= 0x28,
			VK_SELECT		= 0x29,
			VK_EXECUTE		= 0x2B,
			VK_SNAPSHOT		= 0x2C,
			VK_HELP			= 0x2F,
			VK_0			= 0x30,
			VK_1			= 0x31,
			VK_2			= 0x32,
			VK_3			= 0x33,
			VK_4			= 0x34,
			VK_5			= 0x35,
			VK_6			= 0x36,
			VK_7			= 0x37,
			VK_8			= 0x38,
			VK_9			= 0x39,
			VK_A			= 0x41,
			VK_B			= 0x42,
			VK_C			= 0x43,
			VK_D			= 0x44,
			VK_E			= 0x45,
			VK_F			= 0x46,
			VK_G			= 0x47,
			VK_H			= 0x48,
			VK_I			= 0x49,
			VK_J			= 0x4A,
			VK_K			= 0x4B,
			VK_L			= 0x4C,
			VK_M			= 0x4D,
			VK_N			= 0x4E,
			VK_O			= 0x4F,
			VK_P			= 0x50,
			VK_Q			= 0x51,
			VK_R			= 0x52,
			VK_S			= 0x53,
			VK_T			= 0x54,
			VK_U			= 0x55,
			VK_V			= 0x56,
			VK_W			= 0x57,
			VK_X			= 0x58,
			VK_Y			= 0x59,
			VK_Z			= 0x5A,
			VK_NUMPAD0		= 0x60,
			VK_NUMPAD1		= 0x61,
			VK_NUMPAD2		= 0x62,
			VK_NUMPAD3		= 0x63,
			VK_NUMPAD4		= 0x64,
			VK_NUMPAD5		= 0x65,
			VK_NUMPAD6		= 0x66,
			VK_NUMPAD7		= 0x67,
			VK_NUMPAD8		= 0x68,
			VK_NUMPAD9		= 0x69,
			VK_MULTIPLY		= 0x6A,
			VK_ADD			= 0x6B,
			VK_SEPARATOR		= 0x6C,
			VK_SUBTRACT		= 0x6D,
			VK_DECIMAL		= 0x6E,
			VK_DIVIDE		= 0x6F,
			VK_ATTN			= 0xF6,
			VK_CRSEL		= 0xF7,
			VK_EXSEL		= 0xF8,
			VK_EREOF		= 0xF9,
			VK_PLAY			= 0xFA,  
			VK_ZOOM			= 0xFB,
			VK_NONAME		= 0xFC,
			VK_PA1			= 0xFD,
			VK_OEM_CLEAR		= 0xFE,
			VK_LWIN			= 0x5B,
			VK_RWIN			= 0x5C,
			VK_APPS			= 0x5D,   
			VK_LSHIFT		= 0xA0,   
			VK_RSHIFT		= 0xA1,   
			VK_LCONTROL		= 0xA2,   
			VK_RCONTROL		= 0xA3,   
			VK_LMENU		= 0xA4,   
			VK_RMENU		= 0xA5
		}

		internal enum GetSysColorIndex {
			COLOR_SCROLLBAR			=0,
			COLOR_BACKGROUND		=1,
			COLOR_ACTIVECAPTION		=2,
			COLOR_INACTIVECAPTION		=3,
			COLOR_MENU			=4,
			COLOR_WINDOW			=5,
			COLOR_WINDOWFRAME		=6,
			COLOR_MENUTEXT			=7,
			COLOR_WINDOWTEXT		=8,
			COLOR_CAPTIONTEXT		=9,
			COLOR_ACTIVEBORDER		=10,
			COLOR_INACTIVEBORDER		=11,
			COLOR_APPWORKSPACE		=12,
			COLOR_HIGHLIGHT			=13,
			COLOR_HIGHLIGHTTEXT		=14,
			COLOR_BTNFACE			=15,
			COLOR_BTNSHADOW			=16,
			COLOR_GRAYTEXT			=17,
			COLOR_BTNTEXT			=18,
			COLOR_INACTIVECAPTIONTEXT	=19,
			COLOR_BTNHIGHLIGHT		=20,
			COLOR_3DDKSHADOW		=21,
			COLOR_3DLIGHT			=22,
			COLOR_INFOTEXT			=23,
			COLOR_INFOBK			=24,
			COLOR_DESKTOP			=1,
			COLOR_3DFACE			=16,
			COLOR_3DSHADOW			=16,
			COLOR_3DHIGHLIGHT		=20,
			COLOR_3DHILIGHT			=20,
			COLOR_BTNHILIGHT		=20,
			COLOR_MAXVALUE			=24,/* Maximum value */
		}       

		private enum LoadCursorType {
			IDC_ARROW			=32512,
			IDC_IBEAM			=32513,
			IDC_WAIT			=32514,
			IDC_CROSS			=32515,
			IDC_UPARROW			=32516,
			IDC_SIZE			=32640,
			IDC_ICON			=32641,
			IDC_SIZENWSE			=32642,
			IDC_SIZENESW			=32643,
			IDC_SIZEWE			=32644,
			IDC_SIZENS			=32645,
			IDC_SIZEALL			=32646,
			IDC_NO				=32648,
			IDC_HAND			=32649,
			IDC_APPSTARTING			=32650,
			IDC_HELP			=32651
		}

		[Flags]
		private enum WindowLong {
			GWL_WNDPROC     		= -4,
			GWL_HINSTANCE			= -6,
			GWL_HWNDPARENT      		= -8,
			GWL_STYLE           		= -16,
			GWL_EXSTYLE         		= -20,
			GWL_USERDATA			= -21,
			GWL_ID				= -12
		}

		[Flags]
		private enum LogBrushStyle {
			BS_SOLID			= 0,
			BS_NULL             		= 1,
			BS_HATCHED          		= 2,
			BS_PATTERN          		= 3,
			BS_INDEXED          		= 4,
			BS_DIBPATTERN       		= 5,
			BS_DIBPATTERNPT     		= 6,
			BS_PATTERN8X8       		= 7,
			BS_DIBPATTERN8X8    		= 8,
			BS_MONOPATTERN      		= 9
		}

		[Flags]
		private enum LogBrushHatch {
			HS_HORIZONTAL			= 0,       /* ----- */
			HS_VERTICAL         		= 1,       /* ||||| */
			HS_FDIAGONAL        		= 2,       /* \\\\\ */
			HS_BDIAGONAL        		= 3,       /* ///// */
			HS_CROSS            		= 4,       /* +++++ */
			HS_DIAGCROSS        		= 5,       /* xxxxx */
		}

		private struct COLORREF {
			internal byte			B;
			internal byte			G;
			internal byte			R;
			internal byte			A;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct LOGBRUSH {
			internal LogBrushStyle		lbStyle;
			internal COLORREF		lbColor;
			internal LogBrushHatch		lbHatch;
		}
		#endregion

		#region Constructor & Destructor
		private XplatUIWin32() {
			WNDCLASS	wndClass;
			bool		result;

			// Handle singleton stuff first
			ref_count=0;

			// Now regular initialization
			mouse_state = MouseButtons.None;
			mouse_position = Point.Empty;

			themes_enabled = false;

			// Prepare 'our' window class
			wnd_proc = new WndProc(NativeWindow.WndProc);
			wndClass.style = (int)ClassStyle.CS_OWNDC;
			wndClass.lpfnWndProc = wnd_proc;
			wndClass.cbClsExtra = 0;
			wndClass.cbWndExtra = 0;
			wndClass.hbrBackground = IntPtr.Zero;
			wndClass.hCursor = Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);
			wndClass.hIcon = IntPtr.Zero;
			wndClass.hInstance = IntPtr.Zero;
			wndClass.lpszClassName = XplatUI.DefaultClassName;
			wndClass.lpszMenuName = "";

			result=Win32RegisterClass(ref wndClass);
			if (result==false) {
				Win32MessageBox(IntPtr.Zero, "Could not register the "+XplatUI.DefaultClassName+" window class, win32 error " + Win32GetLastError().ToString(), "Oops", 0);
			}

			FosterParent=Win32CreateWindow(0, "Static", "Foster Parent Window", (int)WindowStyles.WS_OVERLAPPEDWINDOW, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

			if (FosterParent==IntPtr.Zero) {
				Win32MessageBox(IntPtr.Zero, "Could not create foster window, win32 error " + Win32GetLastError().ToString(), "Oops", 0);
			}

			handle_data = new Hashtable ();
		}

		~XplatUIWin32() {
			Console.WriteLine("XplatUI Destructor called");
		}
		#endregion	// Constructor & Destructor

		#region Private Support Methods
		private static IntPtr DefWndProc(IntPtr hWnd, Msg msg, IntPtr wParam, IntPtr lParam) {
			return Win32DefWindowProc(hWnd, msg, wParam, lParam);
		}

		private void EraseWindowBackground(IntPtr hWnd, IntPtr hDc) {
			IntPtr		hbr;
			LOGBRUSH	lb;
			uint		argb;
			RECT		rect;
						
			//msg.wParam
			argb = (uint)Win32GetWindowLong(hWnd, WindowLong.GWL_USERDATA);
			lb = new LOGBRUSH();
						
			lb.lbColor.B = (byte)((argb & 0xff0000)>>16);
			lb.lbColor.G = (byte)((argb & 0xff00)>>8);
			lb.lbColor.R = (byte)(argb & 0xff);

			lb.lbStyle = LogBrushStyle.BS_SOLID;
			hbr = Win32CreateBrushIndirect(ref lb);
			Win32GetClientRect(hWnd, out rect);
			Win32FillRect(hDc, ref rect, hbr);
			Win32DeleteObject(hbr);
		}

		#endregion	// Private Support Methods

		#region Static Properties
		internal override Keys ModifierKeys {
			get {
				short	state;
				Keys	key_state;

				key_state = Keys.None;

				state = Win32GetKeyState(VirtualKeys.VK_SHIFT);
				if ((state & 0x8000) != 0) {
					key_state |= Keys.Shift;
				}
				state = Win32GetKeyState(VirtualKeys.VK_CONTROL);
				if ((state & 0x8000) != 0) {
					key_state |= Keys.Control;
				}
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
					throw new NotImplementedException("Need to figure out D'n'D for Win32");
				}
			}
		}
		#endregion	// Static Properties

		#region Singleton Specific Code
		public static XplatUIWin32 GetInstance() {
			if (instance==null) {
				instance=new XplatUIWin32();
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
			mouse_state=MouseButtons.None;
			mouse_position=Point.Empty;

			Console.WriteLine("#region #line XplatUI Win32 Constructor called");

			return IntPtr.Zero;
		}

		internal override void ShutdownDriver(IntPtr token) {
			Console.WriteLine("XplatUIWin32 ShutdownDriver called");
		}


		internal void Version() {
			Console.WriteLine("Xplat version $revision: $");
		}

		internal override void Exit() {
			Win32PostQuitMessage(0);
		}

		internal override void GetDisplaySize(out Size size) {
			RECT	rect;

			Win32GetWindowRect(Win32GetDesktopWindow(), out rect);

			size = new Size(rect.right - rect.left, rect.bottom - rect.top);
		}

		internal override void EnableThemes() {
			themes_enabled=true;
		}

		internal override IntPtr CreateWindow(CreateParams cp) {
			IntPtr	WindowHandle;
			IntPtr	ParentHandle;

			ParentHandle=cp.Parent;

			if ((ParentHandle==IntPtr.Zero) && (cp.Style & (int)WindowStyles.WS_CHILD)!=0) {
				// We need to use our foster parent window until this poor child gets it's parent assigned
				ParentHandle=FosterParent;
			}
			WindowHandle = Win32CreateWindow((uint)cp.ExStyle, cp.ClassName, cp.Caption, (uint)cp.Style, cp.X, cp.Y, cp.Width, cp.Height, ParentHandle, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

			if (WindowHandle==IntPtr.Zero) {
				uint error = Win32GetLastError();

				Win32MessageBox(IntPtr.Zero, "Error : " + error.ToString(), "Failed to create window", 0);
			}

			Win32SetWindowLong(WindowHandle, WindowLong.GWL_USERDATA, (IntPtr)ThemeEngine.Current.DefaultControlBackColor.ToArgb());

			return WindowHandle;
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
			Console.WriteLine("#region #line");
			return;
		}

		internal override void RefreshWindow(IntPtr handle) {			
			Win32InvalidateRect(handle, IntPtr.Zero, true);
			Win32UpdateWindow(handle);
		}

		internal override void SetWindowBackground(IntPtr handle, Color color) {
			Win32SetWindowLong(handle, WindowLong.GWL_USERDATA, (IntPtr)color.ToArgb());
		}

		[MonoTODO("Add support for internal table of windows/DCs for cleanup")]
		internal override PaintEventArgs PaintEventStart(IntPtr handle) {
			IntPtr		hdc;
			PAINTSTRUCT	ps;
			PaintEventArgs	paint_event;
			RECT		rect;
			Rectangle	clip_rect;

			clip_rect = new Rectangle();
			rect = new RECT();
			ps = new PAINTSTRUCT();

			if (Win32GetUpdateRect(handle, ref rect, false)) {
				HandleData	data;

				hdc = Win32BeginPaint(handle, ref ps);

				data = (HandleData) handle_data [0];
				if (data == null) {
					data = new HandleData();
					handle_data[0] = data;
				}

				data.DeviceContext=(Object)ps;

				// FIXME: Figure out why the rectangle is always 0 size
				clip_rect = new Rectangle(ps.rcPaint.left, ps.rcPaint.top, ps.rcPaint.right-ps.rcPaint.left, ps.rcPaint.bottom-ps.rcPaint.top);
//				clip_rect = new Rectangle(rect.top, rect.left, rect.right-rect.left, rect.bottom-rect.top);

				if (ps.fErase!=0) {
					EraseWindowBackground(handle, hdc);
				}
			} else {
				hdc = Win32GetDC(handle);
				// FIXME: Add the DC to internal list
				clip_rect = new Rectangle(rect.top, rect.left, rect.right-rect.left, rect.bottom-rect.top);
			}

			paint_event = new PaintEventArgs(Graphics.FromHdc(hdc), clip_rect);

			return paint_event;
		}

		internal override void PaintEventEnd(IntPtr handle) {
			HandleData	data;
			PAINTSTRUCT	ps;
			PaintEventArgs	paint_event;

			data = (HandleData) handle_data [0];
			if (data == null) {
				data = new HandleData();
				handle_data[0] = data;
			}

			//paint_event.Graphics.Dispose();
			ps = (PAINTSTRUCT)data.DeviceContext;
			Win32EndPaint(handle, ref ps);
		}


		internal override void SetWindowPos(IntPtr handle, int x, int y, int width, int height) {
			Win32MoveWindow(handle, x, y, width, height, true);
			return;
		}

		internal override void GetWindowPos(IntPtr handle, out int x, out int y, out int width, out int height, out int client_width, out int client_height) {
			RECT	rect;
			POINT	pt;

			Win32GetWindowRect(handle, out rect);
			width = rect.right - rect.left;
			height = rect.bottom - rect.top;

			pt.x=rect.left;
			pt.y=rect.top;
			Win32ScreenToClient(Win32GetParent(handle), ref pt);
			x = pt.x;
			y = pt.y;

			Win32GetClientRect(handle, out rect);
			client_width = rect.right - rect.left;
			client_height = rect.bottom - rect.top;
			return;
		}

		internal override void Activate(IntPtr handle) {
			Win32SetActiveWindow(handle);
		}

		internal override void Invalidate(IntPtr handle, Rectangle rc, bool clear) {
			RECT rect;

			rect.left=rc.Left;
			rect.top=rc.Top;
			rect.right=rc.Right;
			rect.bottom=rc.Bottom;
			Win32InvalidateRect(handle, ref rect, clear);
		}

		internal override IntPtr DefWndProc(ref Message msg) {
			msg.Result=Win32DefWindowProc(msg.HWnd, (Msg)msg.Msg, msg.WParam, msg.LParam);
			return msg.Result;
		}

		internal override void HandleException(Exception e) {
			StackTrace st = new StackTrace(e);
			Win32MessageBox(IntPtr.Zero, e.Message+st.ToString(), "Exception", 0);
			Console.WriteLine("{0}{1}", e.Message, st.ToString());
		}

		internal override void DoEvents() {
			MSG msg = new MSG();

			while (Win32PeekMessage(ref msg, IntPtr.Zero, 0, 0, (uint)PeekMessageFlags.PM_REMOVE)!=true) {
				if (msg.message==Msg.WM_PAINT) {
					XplatUI.TranslateMessage(ref msg);
					XplatUI.DispatchMessage(ref msg);
				}
			}
		}

		internal override bool PeekMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags) {
			return Win32PeekMessage(ref msg, hWnd, wFilterMin, wFilterMax, flags);
		}

		internal override bool GetMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax) {
			HandleData	data;
			bool		result;
			data = (HandleData) handle_data [0];
			if ((data!=null) && data.GetMessage(ref msg)) {
				return true;
			}

			result = Win32GetMessage(ref msg, hWnd, wFilterMin, wFilterMax);

			// We need to fake WM_MOUSE_ENTER/WM_MOUSE_LEAVE
			switch (msg.message) {
				case Msg.WM_LBUTTONDOWN: {
					mouse_state |= MouseButtons.Left;
					break;
				}

				case Msg.WM_MBUTTONDOWN: {
					mouse_state |= MouseButtons.Middle;
					break;
				}

				case Msg.WM_RBUTTONDOWN: {
					mouse_state |= MouseButtons.Right;
					break;
				}

				case Msg.WM_LBUTTONUP: {
					mouse_state &= ~MouseButtons.Left;
					break;
				}

				case Msg.WM_MBUTTONUP: {
					mouse_state &= ~MouseButtons.Middle;
					break;
				}

				case Msg.WM_RBUTTONUP: {
					mouse_state &= ~MouseButtons.Right;
					break;
				}

				case Msg.WM_ERASEBKGND: {
					EraseWindowBackground(msg.hwnd, msg.wParam);
					break;
				}

				case Msg.WM_ASYNC_MESSAGE: {
					GCHandle handle = (GCHandle)msg.lParam;
					AsyncMethodData asyncdata = (AsyncMethodData) handle.Target;
					AsyncMethodResult asyncresult = asyncdata.Result.Target as AsyncMethodResult;
					object ret = asyncdata.Method.DynamicInvoke (asyncdata.Args);
					if (asyncresult != null) {
						asyncresult.Complete (ret);
					}
					handle.Free ();
					break;
				}

				case Msg.WM_MOUSEMOVE: {
					if (msg.hwnd != prev_mouse_hwnd) {
						TRACKMOUSEEVENT	tme;

						if (data == null) {
							data = new HandleData();
							handle_data[0] = data;
						}

						// The current message will be sent out next time around
						data.StoreMessage(ref msg);

						// This is the message we want to send at this point
						msg.message = Msg.WM_MOUSE_ENTER;

						prev_mouse_hwnd = msg.hwnd;

						tme = new TRACKMOUSEEVENT();
						tme.size = Marshal.SizeOf(tme);
						tme.hWnd = msg.hwnd;
						tme.dwFlags = TMEFlags.TME_LEAVE | TMEFlags.TME_HOVER;
						Win32TrackMouseEvent(ref tme);
						return result;
					}
					break;
				}

				case Msg.WM_MOUSELEAVE: {
					prev_mouse_hwnd = IntPtr.Zero;
					msg.message=Msg.WM_MOUSE_LEAVE;
					break;
				}
			}

			return result;
		}

		internal override bool TranslateMessage(ref MSG msg) {
			return Win32TranslateMessage(ref msg);
		}

		internal override bool DispatchMessage(ref MSG msg) {
			return Win32DispatchMessage(ref msg);
		}

		internal override bool Text(IntPtr handle, string text) {
			Win32SetWindowText(handle, text);
			return true;
		}

		internal override bool SetVisible(IntPtr handle, bool visible) {
			Console.WriteLine("Setting window visibility: {0}", visible);
			return true;
		}

		internal override bool IsVisible(IntPtr handle) {
			WINDOWPLACEMENT	wndpl;

			wndpl = new WINDOWPLACEMENT();
			wndpl.length=(uint)Marshal.SizeOf(wndpl);
			Win32GetWindowPlacement(handle, ref wndpl);
			if ((wndpl.showCmd == WindowPlacementFlags.SW_SHOWMINIMIZED)) {
				return false;
			}
			return true;
		}

		internal override IntPtr SetParent(IntPtr handle, IntPtr parent) {
			return Win32SetParent(handle, parent);
		}

		internal override IntPtr GetParent(IntPtr handle) {
			return Win32GetParent(handle);
		}

		internal override void GrabWindow(IntPtr hWnd, IntPtr ConfineToHwnd) {
			grab_hwnd = hWnd;
			Win32SetCapture(hWnd);
		}

		internal override void GrabInfo(out IntPtr hWnd, out bool GrabConfined, out Rectangle GrabArea) {
			hWnd = grab_hwnd;
			GrabConfined = grab_confined;
			GrabArea = grab_area;
		}

		internal override void ReleaseWindow(IntPtr hWnd) {
			Win32ReleaseCapture();
			grab_hwnd = IntPtr.Zero;
		}

		internal override bool CalculateWindowRect(IntPtr hWnd, ref Rectangle ClientRect, int Style, bool HasMenu, out Rectangle WindowRect) {
			RECT	rect;

			rect.left=ClientRect.Left;
			rect.top=ClientRect.Top;
			rect.right=ClientRect.Right;
			rect.bottom=ClientRect.Bottom;

			if (!Win32AdjustWindowRectEx(ref rect, Style, HasMenu, 0)) {
				WindowRect = new Rectangle(ClientRect.Left, ClientRect.Top, ClientRect.Width, ClientRect.Height);
				return false;
			}

			WindowRect = new Rectangle(rect.left, rect.top, rect.right-rect.left, rect.bottom-rect.top);
			return true;
		}

		internal override void GetCursorPos(IntPtr handle, out int x, out int y) {
			POINT	pt;

			Win32GetCursorPos(out pt);

			if (handle!=IntPtr.Zero) {
				Win32ScreenToClient(handle, ref pt);
			}

			x=pt.x;
			y=pt.y;
		}

		internal override void ScreenToClient(IntPtr handle, ref int x, ref int y)
		{
			POINT pnt;			

			pnt.x = x;
			pnt.y = y;
			Win32ScreenToClient (handle, ref pnt);

			x = pnt.x;
			y = pnt.y;
			
		}

		internal override void SendAsyncMethod (AsyncMethodData method)
		{
			Win32PostMessage(FosterParent, Msg.WM_ASYNC_MESSAGE, IntPtr.Zero, (IntPtr)GCHandle.Alloc (method));
		}

		// Santa's little helper
		static void Where() {
			Console.WriteLine("Here: {0}", new StackTrace().ToString());
		}
		#endregion	// Public Static Methods

		#region Win32 Imports
		[DllImport ("kernel32.dll", EntryPoint="GetLastError", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static uint Win32GetLastError();

		[DllImport ("user32.dll", EntryPoint="CreateWindowExA", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32CreateWindow(uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lParam);

		[DllImport ("user32.dll", EntryPoint="PeekMessageA", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32PeekMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags);

		[DllImport ("user32.dll", EntryPoint="GetMessageA", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32GetMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax);

		[DllImport ("user32.dll", EntryPoint="TranslateMessage", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32TranslateMessage(ref MSG msg);

		[DllImport ("user32.dll", EntryPoint="DispatchMessageA", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32DispatchMessage(ref MSG msg);

		[DllImport ("user32.dll", EntryPoint="MoveWindow", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);

		[DllImport ("user32.dll", EntryPoint="SetWindowTextA", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32SetWindowText(IntPtr hWnd, string lpString);

		[DllImport ("user32.dll", EntryPoint="SetParent", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32SetParent(IntPtr hWnd, IntPtr hParent);

		[DllImport ("user32.dll", EntryPoint="RegisterClassA", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32RegisterClass(ref WNDCLASS wndClass);

		[DllImport ("user32.dll", EntryPoint="LoadCursorA", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32LoadCursor(IntPtr hInstance, LoadCursorType type);

		[DllImport ("user32.dll", EntryPoint="DefWindowProcA", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32DefWindowProc(IntPtr hWnd, Msg Msg, IntPtr wParam, IntPtr lParam);

		[DllImport ("user32.dll", EntryPoint="PostQuitMessage", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32PostQuitMessage(int nExitCode);

		[DllImport ("user32.dll", EntryPoint="UpdateWindow", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32UpdateWindow(IntPtr hWnd);

		[DllImport ("user32.dll", EntryPoint="GetUpdateRect", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32GetUpdateRect(IntPtr hWnd, ref RECT rect, bool erase);

		[DllImport ("user32.dll", EntryPoint="BeginPaint", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32BeginPaint(IntPtr hWnd, ref PAINTSTRUCT ps);

		[DllImport ("user32.dll", EntryPoint="EndPaint", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32EndPaint(IntPtr hWnd, ref PAINTSTRUCT ps);

		[DllImport ("user32.dll", EntryPoint="GetDC", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32GetDC(IntPtr hWnd);

		[DllImport ("user32.dll", EntryPoint="ReleaseDC", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32ReleaseDC(IntPtr hWnd, IntPtr hDC);

		[DllImport ("user32.dll", EntryPoint="MessageBoxA", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32MessageBox(IntPtr hParent, string pText, string pCaption, uint uType);

		[DllImport ("user32.dll", EntryPoint="InvalidateRect", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32InvalidateRect(IntPtr hWnd, ref RECT lpRect, bool bErase);

		[DllImport ("user32.dll", EntryPoint="InvalidateRect", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

		[DllImport ("user32.dll", EntryPoint="SetCapture", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32SetCapture(IntPtr hWnd);

		[DllImport ("user32.dll", EntryPoint="ReleaseCapture", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32ReleaseCapture();

		[DllImport ("user32.dll", EntryPoint="GetWindowRect", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32GetWindowRect(IntPtr hWnd, out RECT rect);

		[DllImport ("user32.dll", EntryPoint="GetClientRect", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32GetClientRect(IntPtr hWnd, out RECT rect);

		[DllImport ("user32.dll", EntryPoint="ScreenToClient", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32ScreenToClient(IntPtr hWnd, ref POINT pt);

		[DllImport ("user32.dll", EntryPoint="GetParent", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32GetParent(IntPtr hWnd);

		[DllImport ("user32.dll", EntryPoint="SetActiveWindow", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32SetActiveWindow(IntPtr hWnd);

		[DllImport ("user32.dll", EntryPoint="PostQuitMessage", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32PostQuitMessage(IntPtr hWnd);

		[DllImport ("user32.dll", EntryPoint="AdjustWindowRectEx", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32AdjustWindowRectEx(ref RECT lpRect, int dwStyle, bool bMenu, int dwExStyle);

		[DllImport ("user32.dll", EntryPoint="GetCursorPos", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32GetCursorPos(out POINT lpPoint);

		[DllImport ("user32.dll", EntryPoint="GetWindowPlacement", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

		[DllImport ("user32.dll", EntryPoint="TrackMouseEvent", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32TrackMouseEvent(ref TRACKMOUSEEVENT tme);

		[DllImport ("gdi32.dll", EntryPoint="CreateBrushIndirect", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32CreateBrushIndirect(ref LOGBRUSH lb);

		[DllImport ("user32.dll", EntryPoint="FillRect", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static int Win32FillRect(IntPtr hdc, ref RECT rect, IntPtr hbr);

		[DllImport ("user32.dll", EntryPoint="SetWindowLong", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32SetWindowLong(IntPtr hwnd, WindowLong index, IntPtr value);

		[DllImport ("user32.dll", EntryPoint="GetWindowLong", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32GetWindowLong(IntPtr hwnd, WindowLong index);

		[DllImport ("gdi32.dll", EntryPoint="DeleteObject", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32DeleteObject(IntPtr o);

		[DllImport ("user32.dll", EntryPoint="PostMessage", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32PostMessage(IntPtr hwnd, Msg msg, IntPtr wParam, IntPtr lParam);

		[DllImport ("user32.dll", EntryPoint="GetKeyState", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static short Win32GetKeyState(VirtualKeys nVirtKey);

		[DllImport ("user32.dll", EntryPoint="GetDesktopWindow", CharSet=CharSet.Ansi, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32GetDesktopWindow();
		#endregion

	}
}
