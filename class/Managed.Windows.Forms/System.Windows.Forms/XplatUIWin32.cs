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
// $Revision: 1.1 $
// $Modtime: $
// $Log: XplatUIWin32.cs,v $
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
	public class XplatUIWin32 : XplatUIDriver {
		#region Local Variables
		private static XplatUIWin32	instance;
		private static int		ref_count;
		private static IntPtr		FosterParent;

		internal static Keys		key_state;
		internal static MouseButtons	mouse_state;
		internal static Point		mouse_position;
		internal static WndProc		wnd_proc;
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

		private enum GetSysColorIndex {
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
			COLOR_BTNHILIGHT		=20
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
		#endregion

		#region Constructor & Destructor
		private XplatUIWin32() {
			WNDCLASS	wndClass;
			bool		result;

			// Handle singleton stuff first
			ref_count=0;

			// Now regular initialization
			key_state=Keys.None;
			mouse_state=MouseButtons.None;
			mouse_position=Point.Empty;

			// Prepare 'our' window class
			wnd_proc = new WndProc(NativeWindow.WndProc);
			wndClass.style = (int)ClassStyle.CS_OWNDC;
			wndClass.lpfnWndProc = wnd_proc;
			wndClass.cbClsExtra = 0;
			wndClass.cbWndExtra = 0;
			wndClass.hbrBackground = (IntPtr)(GetSysColorIndex.COLOR_BTNFACE+1);
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

			Console.WriteLine("#region #line XplatUI WIN32 Constructor called, result {0}", result);
		}

		~XplatUIWin32() {
			Console.WriteLine("XplatUI Destructor called");
		}
		#endregion	// Constructor & Destructor

		#region Private Support Methods
		private static IntPtr DefWndProc(IntPtr hWnd, Msg msg, IntPtr wParam, IntPtr lParam) {
			return Win32DefWindowProc(hWnd, msg, wParam, lParam);
		}
		#endregion	// Private Support Methods

		#region Static Properties
		internal override Color BackColor {
			get {
				return Color.Red;
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
			key_state=Keys.None;
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
			Win32UpdateWindow(handle);
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
				hdc = Win32BeginPaint(handle, ref ps);
				// FIXME: Add the DC to internal list

				// FIXME: Figure out why the rectangle is always 0 size
				clip_rect = new Rectangle(ps.rcPaint.left, ps.rcPaint.top, ps.rcPaint.right-ps.rcPaint.left, ps.rcPaint.bottom-ps.rcPaint.top);
//				clip_rect = new Rectangle(rect.top, rect.left, rect.right-rect.left, rect.bottom-rect.top);
			} else {
				hdc = Win32GetDC(handle);
				// FIXME: Add the DC to internal list

				clip_rect = new Rectangle(rect.top, rect.left, rect.right-rect.left, rect.bottom-rect.top);
			}

			paint_event = new PaintEventArgs(Graphics.FromHdc(hdc), clip_rect);

			return paint_event;
		}

		internal override void PaintEventEnd(IntPtr handle) {
			// FIXME: Lookup in the internal list how to clean
			;
			// paintEventArgs.Dispose();
		}


		internal override void SetWindowPos(IntPtr handle, Rectangle rc) {
			
			Console.WriteLine("#region #line");
			return;
		}

		internal override void SetWindowPos(IntPtr handle, int x, int y, int width, int height) {
			Console.WriteLine("#region #line");
			return;
		}

		internal override void Activate(IntPtr handle) {
			Console.WriteLine("#region #line");
			return;
		}

		internal override void Invalidate(IntPtr handle, Rectangle rc) {
			Console.WriteLine("#region #line");
			return;
		}

		internal override IntPtr DefWndProc(ref Message msg) {
			msg.Result=Win32DefWindowProc(msg.Hwnd, (Msg)msg.Msg, msg.WParam, msg.LParam);
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
			return Win32GetMessage(ref msg, hWnd, wFilterMin, wFilterMax);
		}

		internal override bool TranslateMessage(ref MSG msg) {
			return Win32TranslateMessage(ref msg);
		}

		internal override bool DispatchMessage(ref MSG msg) {
			return Win32DispatchMessage(ref msg);
		}

		internal override bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height) {
			return Win32MoveWindow(hWnd, x, y, width, height);
		}

		internal override bool Text(IntPtr handle, string text) {
			Console.WriteLine("Setting window text {0}", text);
			Win32SetWindowText(handle, text);
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
			Console.WriteLine("Setting parent {0}", parent);
			Win32SetParent(handle, parent);
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
		internal extern static bool Win32MoveWindow(IntPtr hWnd, int x, int y, int width, int height);

		[DllImport ("user32.dll", EntryPoint="SetWindowTextA", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32SetWindowText(IntPtr hWnd, string lpString);

		[DllImport ("user32.dll", EntryPoint="SetParent", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32SetParent(IntPtr hWnd, IntPtr hParent);

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
		#endregion

	}
}
