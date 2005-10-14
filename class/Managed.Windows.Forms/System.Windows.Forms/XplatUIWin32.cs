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
using System.Drawing;
using System.Drawing.Imaging;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;


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
		internal static IntPtr		override_cursor;

		internal static bool		themes_enabled;
		private Hashtable		timer_list;
		private static Queue		message_queue;
		private static IntPtr 		clip_magic = new IntPtr(27051977);

		private static Win32DnD		DnD;
		#endregion	// Local Variables

		#region Private Structs
		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
		private struct WNDCLASS {
			internal int		style;
			internal WndProc	lpfnWndProc;
			internal int		cbClsExtra;
			internal int		cbWndExtra;
			internal IntPtr		hInstance;
			internal IntPtr		hIcon;
			internal IntPtr		hCursor;
			internal IntPtr		hbrBackground;
			[MarshalAs(UnmanagedType.LPWStr)]
			internal string		lpszMenuName;
			[MarshalAs(UnmanagedType.LPWStr)]
			internal string		lpszClassName;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct RECT {
			internal int		left;
			internal int		top;
			internal int		right;
			internal int		bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		internal struct POINT {
			internal int		x;
			internal int		y;
		}

		internal enum SPIAction {
			SPI_GETWORKAREA		= 0x0030
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

		[StructLayout(LayoutKind.Sequential)]
		internal struct NCCALCSIZE_PARAMS {
			internal RECT		rgrc1;
			internal RECT		rgrc2;
			internal RECT		rgrc3;
			internal IntPtr		lppos;
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

		internal enum SetWindowPosZOrder {
			HWND_TOP			= 0,
			HWND_BOTTOM			= 1,
			HWND_TOPMOST			= -1,
			HWND_NOTOPMOST			= -2
		}

		[Flags]
		internal enum SetWindowPosFlags {
			SWP_ASYNCWINDOWPOS		= 0x4000, 
			SWP_DEFERERASE			= 0x2000,
			SWP_DRAWFRAME			= 0x0020,
			SWP_FRAMECHANGED		= 0x0020,
			SWP_HIDEWINDOW			= 0x0080,
			SWP_NOACTIVATE			= 0x0010,
			SWP_NOCOPYBITS			= 0x0100,
			SWP_NOMOVE			= 0x0002,
			SWP_NOOWNERZORDER		= 0x0200,
			SWP_NOREDRAW			= 0x0008,
			SWP_NOREPOSITION		= 0x0200,
			SWP_NOENDSCHANGING		= 0x0400,
			SWP_NOSIZE			= 0x0001,
			SWP_NOZORDER			= 0x0004,
			SWP_SHOWWINDOW			= 0x0040
		}

		internal enum GetSysColorIndex {
			COLOR_SCROLLBAR			= 0,
			COLOR_BACKGROUND		= 1,
			COLOR_ACTIVECAPTION		= 2,
			COLOR_INACTIVECAPTION		= 3,
			COLOR_MENU			= 4,
			COLOR_WINDOW			= 5,
			COLOR_WINDOWFRAME		= 6,
			COLOR_MENUTEXT			= 7,
			COLOR_WINDOWTEXT		= 8,
			COLOR_CAPTIONTEXT		= 9,
			COLOR_ACTIVEBORDER		= 10,
			COLOR_INACTIVEBORDER		= 11,
			COLOR_APPWORKSPACE		= 12,
			COLOR_HIGHLIGHT			= 13,
			COLOR_HIGHLIGHTTEXT		= 14,
			COLOR_BTNFACE			= 15,
			COLOR_BTNSHADOW			= 16,
			COLOR_GRAYTEXT			= 17,
			COLOR_BTNTEXT			= 18,
			COLOR_INACTIVECAPTIONTEXT	= 19,
			COLOR_BTNHIGHLIGHT		= 20,
			COLOR_3DDKSHADOW		= 21,
			COLOR_3DLIGHT			= 22,
			COLOR_INFOTEXT			= 23,
			COLOR_INFOBK			= 24,
			
			COLOR_HOTLIGHT			= 26,
			COLOR_GRADIENTACTIVECAPTION	= 27,
			COLOR_GRADIENTINACTIVECAPTION	= 28,
			COLOR_MENUHIGHLIGHT		= 29,
			COLOR_MENUBAR			= 30,

			COLOR_DESKTOP			= 1,
			COLOR_3DFACE			= 16,
			COLOR_3DSHADOW			= 16,
			COLOR_3DHIGHLIGHT		= 20,
			COLOR_3DHILIGHT			= 20,
			COLOR_BTNHILIGHT		= 20,
			COLOR_MAXVALUE			= 24,/* Maximum value */
		}       

		private enum LoadCursorType {
			First				= 32512,
			IDC_ARROW			= 32512,
			IDC_IBEAM			= 32513,
			IDC_WAIT			= 32514,
			IDC_CROSS			= 32515,
			IDC_UPARROW			= 32516,
			IDC_SIZE			= 32640,
			IDC_ICON			= 32641,
			IDC_SIZENWSE			= 32642,
			IDC_SIZENESW			= 32643,
			IDC_SIZEWE			= 32644,
			IDC_SIZENS			= 32645,
			IDC_SIZEALL			= 32646,
			IDC_NO				= 32648,
			IDC_HAND			= 32649,
			IDC_APPSTARTING			= 32650,
			IDC_HELP			= 32651,
			Last				= 32651
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

		internal struct COLORREF {
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

		[StructLayout(LayoutKind.Sequential)]
		internal struct TEXTMETRIC { 
			internal int			tmHeight;
			internal int			tmAscent;
			internal int			tmDescent;
			internal int			tmInternalLeading;
			internal int			tmExternalLeading;
			internal int			tmAveCharWidth;
			internal int			tmMaxCharWidth;
			internal int			tmWeight;
			internal int			tmOverhang;
			internal int			tmDigitizedAspectX;
			internal int			tmDigitizedAspectY;
			internal byte			tmFirstChar; 
			internal byte			tmLastChar; 
			internal byte			tmDefaultChar; 
			internal byte			tmBreakChar; 
			internal byte			tmItalic; 
			internal byte			tmUnderlined; 
			internal byte			tmStruckOut; 
			internal byte			tmPitchAndFamily; 
			internal byte			tmCharSet; 
		} 

		[Flags]
		private enum ScrollWindowExFlags {
			SW_NONE				= 0x0000,
			SW_SCROLLCHILDREN		= 0x0001,
			SW_INVALIDATE			= 0x0002,
			SW_ERASE			= 0x0004,
			SW_SMOOTHSCROLL			= 0x0010
		}

		internal enum SystemMetrics {
			SM_CXSCREEN			= 0,
			SM_CYSCREEN             	= 1,
			SM_CXVSCROLL            	= 2,
			SM_CYHSCROLL            	= 3,
			SM_CYCAPTION            	= 4,
			SM_CXBORDER             	= 5,
			SM_CYBORDER             	= 6,
			SM_CXDLGFRAME           	= 7,
			SM_CYDLGFRAME           	= 8,
			SM_CYVTHUMB             	= 9,
			SM_CXHTHUMB             	= 10,
			SM_CXICON               	= 11,
			SM_CYICON               	= 12,
			SM_CXCURSOR             	= 13,
			SM_CYCURSOR             	= 14,
			SM_CYMENU               	= 15,
			SM_CXFULLSCREEN         	= 16,
			SM_CYFULLSCREEN         	= 17,
			SM_CYKANJIWINDOW        	= 18,
			SM_MOUSEPRESENT         	= 19,
			SM_CYVSCROLL            	= 20,
			SM_CXHSCROLL            	= 21,
			SM_DEBUG                	= 22,
			SM_SWAPBUTTON           	= 23,
			SM_RESERVED1            	= 24,
			SM_RESERVED2            	= 25,
			SM_RESERVED3            	= 26,
			SM_RESERVED4            	= 27,
			SM_CXMIN                	= 28,
			SM_CYMIN                	= 29,
			SM_CXSIZE               	= 30,
			SM_CYSIZE               	= 31,
			SM_CXFRAME              	= 32,
			SM_CYFRAME              	= 33,
			SM_CXMINTRACK			= 34,
			SM_CYMINTRACK           	= 35,
			SM_CXDOUBLECLK          	= 36,
			SM_CYDOUBLECLK          	= 37,
			SM_CXICONSPACING        	= 38,
			SM_CYICONSPACING        	= 39,
			SM_MENUDROPALIGNMENT    	= 40,
			SM_PENWINDOWS           	= 41,
			SM_DBCSENABLED          	= 42,
			SM_CMOUSEBUTTONS        	= 43,
			SM_CXFIXEDFRAME			= SM_CXDLGFRAME,
			SM_CYFIXEDFRAME			= SM_CYDLGFRAME,
			SM_CXSIZEFRAME			= SM_CXFRAME,
			SM_CYSIZEFRAME			= SM_CYFRAME,
			SM_SECURE               	= 44,
			SM_CXEDGE               	= 45,
			SM_CYEDGE               	= 46,
			SM_CXMINSPACING         	= 47,
			SM_CYMINSPACING         	= 48,
			SM_CXSMICON             	= 49,
			SM_CYSMICON             	= 50,
			SM_CYSMCAPTION          	= 51,
			SM_CXSMSIZE             	= 52,
			SM_CYSMSIZE             	= 53,
			SM_CXMENUSIZE           	= 54,
			SM_CYMENUSIZE           	= 55,
			SM_ARRANGE              	= 56,
			SM_CXMINIMIZED          	= 57,
			SM_CYMINIMIZED          	= 58,
			SM_CXMAXTRACK           	= 59,
			SM_CYMAXTRACK           	= 60,
			SM_CXMAXIMIZED          	= 61,
			SM_CYMAXIMIZED          	= 62,
			SM_NETWORK              	= 63,
			SM_CLEANBOOT            	= 67,
			SM_CXDRAG               	= 68,
			SM_CYDRAG               	= 69,
			SM_SHOWSOUNDS           	= 70,
			SM_CXMENUCHECK          	= 71,
			SM_CYMENUCHECK          	= 72,
			SM_SLOWMACHINE          	= 73,
			SM_MIDEASTENABLED       	= 74,
			SM_MOUSEWHEELPRESENT    	= 75,
			SM_XVIRTUALSCREEN       	= 76,
			SM_YVIRTUALSCREEN       	= 77,
			SM_CXVIRTUALSCREEN      	= 78,
			SM_CYVIRTUALSCREEN      	= 79,
			SM_CMONITORS            	= 80,
			SM_SAMEDISPLAYFORMAT    	= 81,
			SM_IMMENABLED           	= 82,
			SM_CXFOCUSBORDER        	= 83,
			SM_CYFOCUSBORDER        	= 84,
			SM_TABLETPC             	= 86,
			SM_MEDIACENTER          	= 87,
			SM_CMETRICS             	= 88
		}

		// We'll only support _WIN32_IE < 0x0500 for now
		internal enum NotifyIconMessage {
			NIM_ADD				= 0x00000000,
			NIM_MODIFY			= 0x00000001,
			NIM_DELETE			= 0x00000002,
		}

		[Flags]
		internal enum NotifyIconFlags {
			NIF_MESSAGE			= 0x00000001,
			NIF_ICON			= 0x00000002,
			NIF_TIP				= 0x00000004,
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
		internal struct NOTIFYICONDATA {
			internal uint			cbSize;
			internal IntPtr			hWnd;
			internal uint			uID;
			internal NotifyIconFlags	uFlags;
			internal uint			uCallbackMessage;
			internal IntPtr			hIcon;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=64)]
			internal string			szTip;
		}

		[Flags]
		internal enum DCExFlags {
			DCX_WINDOW			= 0x00000001,
			DCX_CACHE			= 0x00000002,
			DCX_NORESETATTRS     		= 0x00000004,
			DCX_CLIPCHILDREN     		= 0x00000008,
			DCX_CLIPSIBLINGS     		= 0x00000010,
			DCX_PARENTCLIP       		= 0x00000020,
			DCX_EXCLUDERGN       		= 0x00000040,
			DCX_INTERSECTRGN     		= 0x00000080,
			DCX_EXCLUDEUPDATE    		= 0x00000100,
			DCX_INTERSECTUPDATE  		= 0x00000200,
			DCX_LOCKWINDOWUPDATE 		= 0x00000400,
			DCX_USESTYLE			= 0x00010000,
			DCX_VALIDATE         		= 0x00200000
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
		internal struct CLIENTCREATESTRUCT {
			internal IntPtr			hWindowMenu;
			internal uint			idFirstChild;
		}

		private enum ClassLong : int {
			GCL_MENUNAME			= -8,
			GCL_HBRBACKGROUND		= -10,
			GCL_HCURSOR         		= -12,
			GCL_HICON            		= -14,
			GCL_HMODULE          		= -16,
			GCL_CBWNDEXTRA       		= -18,
			GCL_CBCLSEXTRA       		= -20,
			GCL_WNDPROC          		= -24,
			GCL_STYLE            		= -26,
			GCW_ATOM             		= -32,
			GCL_HICONSM			= -34
		}

		[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
		internal struct MINMAXINFO {
			internal POINT			ptReserved;
			internal POINT			ptMaxSize;
			internal POINT			ptMaxPosition;
			internal POINT			ptMinTrackSize;
			internal POINT			ptMaxTrackSize;
		}

		[Flags]
		internal enum GAllocFlags : uint {
			GMEM_FIXED			= 0x0000,
			GMEM_MOVEABLE			= 0x0002,
			GMEM_NOCOMPACT			= 0x0010,
			GMEM_NODISCARD			= 0x0020,
			GMEM_ZEROINIT			= 0x0040,
			GMEM_MODIFY			= 0x0080,
			GMEM_DISCARDABLE		= 0x0100,
			GMEM_NOT_BANKED			= 0x1000,
			GMEM_SHARE          		= 0x2000,
			GMEM_DDESHARE			= 0x2000,
			GMEM_NOTIFY			= 0x4000,
			GMEM_LOWER			= GMEM_NOT_BANKED,
			GMEM_VALID_FLAGS		= 0x7F72,
			GMEM_INVALID_HANDLE 		= 0x8000,
			GHND                		= (GMEM_MOVEABLE | GMEM_ZEROINIT),
			GPTR                		= (GMEM_FIXED | GMEM_ZEROINIT)
		}

		internal enum ROP2DrawMode : int {
			R2_BLACK			= 1,
			R2_NOTMERGEPEN      		= 2,
			R2_MASKNOTPEN       		= 3,
			R2_NOTCOPYPEN       		= 4,
			R2_MASKPENNOT       		= 5,
			R2_NOT              		= 6,
			R2_XORPEN           		= 7,
			R2_NOTMASKPEN       		= 8,
			R2_MASKPEN          		= 9,
			R2_NOTXORPEN        		= 10,
			R2_NOP              		= 11,
			R2_MERGENOTPEN      		= 12,
			R2_COPYPEN          		= 13,
			R2_MERGEPENNOT      		= 14,
			R2_MERGEPEN         		= 15,
			R2_WHITE            		= 16,
			R2_LAST             		= 16
		}

		internal enum PenStyle : int {
			PS_SOLID			= 0,
			PS_DASH             		= 1,
			PS_DOT              		= 2,
			PS_DASHDOT          		= 3,
			PS_DASHDOTDOT       		= 4,
			PS_NULL             		= 5,
			PS_INSIDEFRAME      		= 6,
			PS_USERSTYLE        		= 7,
			PS_ALTERNATE        		= 8
		}

		internal enum StockObject : int {
			WHITE_BRUSH			= 0,
			LTGRAY_BRUSH        		= 1,
			GRAY_BRUSH          		= 2,
			DKGRAY_BRUSH        		= 3,
			BLACK_BRUSH         		= 4,
			NULL_BRUSH          		= 5,
			HOLLOW_BRUSH        		= NULL_BRUSH,
			WHITE_PEN   			= 6,
			BLACK_PEN           		= 7,
			NULL_PEN            		= 8,
			OEM_FIXED_FONT      		= 10,
			ANSI_FIXED_FONT     		= 11,
			ANSI_VAR_FONT       		= 12,
			SYSTEM_FONT         		= 13,
			DEVICE_DEFAULT_FONT 		= 14,
			DEFAULT_PALETTE     		= 15,
			SYSTEM_FIXED_FONT  		= 16
		}

		internal enum HatchStyle : int {
			HS_HORIZONTAL			= 0,
			HS_VERTICAL         		= 1,
			HS_FDIAGONAL        		= 2,
			HS_BDIAGONAL        		= 3,
			HS_CROSS            		= 4,
			HS_DIAGCROSS        		= 5
		}
		#endregion

		#region Constructor & Destructor
		private XplatUIWin32() {
			WNDCLASS	wndClass;
			bool		result;

			// Handle singleton stuff first
			ref_count=0;

			DnD = new Win32DnD();

			mouse_state = MouseButtons.None;
			mouse_position = Point.Empty;

			message_queue = new Queue();

			themes_enabled = false;

			// Prepare 'our' window class
			wnd_proc = new WndProc(NativeWindow.WndProc);
			wndClass.style = (int)(ClassStyle.CS_OWNDC | ClassStyle.CS_DBLCLKS);
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

			FosterParent=Win32CreateWindow(0, "static", "Foster Parent Window", (int)WindowStyles.WS_OVERLAPPEDWINDOW, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

			if (FosterParent==IntPtr.Zero) {
				Win32MessageBox(IntPtr.Zero, "Could not create foster window, win32 error " + Win32GetLastError().ToString(), "Oops", 0);
			}

			timer_list = new Hashtable ();
		}
		#endregion	// Constructor & Destructor

		#region Private Support Methods
		private static IntPtr DefWndProc(IntPtr hWnd, Msg msg, IntPtr wParam, IntPtr lParam) {
			return Win32DefWindowProc(hWnd, msg, wParam, lParam);
		}

 		private static bool MessageWaiting {
			get {
				if (message_queue.Count == 0) {
					return false;
				}
				return true;
			}
		}

		private static bool RetrieveMessage(ref MSG msg) {
			MSG	message;

			if (message_queue.Count == 0) {
				return false;
			}

			message = (MSG)message_queue.Dequeue();
			msg = message;

			return true;
		}

		private static bool StoreMessage(ref MSG msg) {
			MSG message = new MSG();

			message = msg;
			message_queue.Enqueue(message);

			return true;
		}

		internal static String AnsiToString(IntPtr ansi_data) {
			return (string)Marshal.PtrToStringAnsi(ansi_data);
		}

		internal static String UnicodeToString(IntPtr unicode_data) {
			return (string)Marshal.PtrToStringUni(unicode_data);
		}

		internal static Image DIBtoImage(IntPtr dib_data) {
			MemoryStream		ms;
			byte[]			header;
			byte[]			buffer;
			Bitmap			bmp;
			BITMAPINFOHEADER	bmi;
			int			ncolors;
			int			palettesize;
			int			imagesize;
			int			size;
			int			offset;

			header = new byte[54];	// Size of a BMP file header, without palette
			// Grab the header
			header[0] = (byte)'B';
			header[1] = (byte)'M';
			// 2, 3, 4 and 5 = unsigned int size
			// 6, 7, 8 and 9 = reserved
			// 10, 11, 12 and 13 = offset to image data

			// Create a fake BMP header
			bmi = (BITMAPINFOHEADER)Marshal.PtrToStructure(dib_data, typeof(BITMAPINFOHEADER));

			ncolors = (int)bmi.biClrUsed;
			if (ncolors == 0) {
				if (bmi.biBitCount != 24) {
					ncolors = (int)(1 << bmi.biBitCount);
				}
			}
			palettesize = ncolors * 4;

			imagesize = (int)bmi.biSizeImage;
			if (imagesize == 0) {
				imagesize = (int)(((((bmi.biWidth * bmi.biBitCount) + 31) & ~31) >> 3) * bmi.biHeight);
			}

			size = 54 + palettesize + imagesize;
			offset = 54 + palettesize;
			buffer = new byte[size];

			// Copy the fake BMP file header
			header[2] = (byte)size;
			header[3] = (byte)(size >> 8);
			header[4] = (byte)(size >> 16);
			header[5] = (byte)(size >> 24);

			header[10] = (byte)offset;
			header[11] = (byte)(offset >> 8);
			header[12] = (byte)(offset >> 16);
			header[13] = (byte)(offset >> 24);

			Array.Copy(header, 0, buffer, 0, 14);

			for (int i = 14; i < size; i++) {
				buffer[i] = Marshal.ReadByte(dib_data, i - 14);
			}

			ms = new MemoryStream(buffer, 0, size, false);
			bmp = new Bitmap(ms);
			ms.Close();
			return bmp;
		}

		internal static byte[] ImageToDIB(Image image) {
			MemoryStream	ms;
			byte[]		buffer;
			byte[]		retbuf;

			ms = new MemoryStream();
			image.Save(ms, ImageFormat.Bmp);
			buffer = ms.GetBuffer();

			// Filter out the file header
			retbuf = new byte[buffer.Length];
			Array.Copy(buffer, 14, retbuf, 0, buffer.Length - 14);
			return retbuf;
		}

		internal static IntPtr DupGlobalMem(IntPtr mem) {
			IntPtr	dup;
			IntPtr	dup_ptr;
			IntPtr	mem_ptr;
			uint	len;

			len = Win32GlobalSize(mem);
			mem_ptr = Win32GlobalLock(mem);

			dup = Win32GlobalAlloc(GAllocFlags.GMEM_MOVEABLE, (int)len);
			dup_ptr = Win32GlobalLock(dup);

			Win32CopyMemory(dup_ptr, mem_ptr, (int)len);

			Win32GlobalUnlock(mem);
			Win32GlobalUnlock(dup);

			return dup;
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

				state = Win32GetKeyState(VirtualKeys.VK_MENU);
				if ((state & 0x8000) != 0) {
					key_state |= Keys.Alt;
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
					//throw new NotImplementedException("Need to figure out D'n'D for Win32");
				}
			}
		}

		internal override int Caption {
			get {
				return Win32GetSystemMetrics(SystemMetrics.SM_CYCAPTION);
			}
		}

		internal override Size CursorSize {
			get {
				return new Size(Win32GetSystemMetrics(SystemMetrics.SM_CXCURSOR), Win32GetSystemMetrics(SystemMetrics.SM_CYCURSOR));
			}
		}

		internal override bool DragFullWindows {
			get {
				return true;
			}
		}

		internal override Size DragSize {
			get {
				return new Size(Win32GetSystemMetrics(SystemMetrics.SM_CXDRAG), Win32GetSystemMetrics(SystemMetrics.SM_CYDRAG));
			}
		}

		internal override Size FrameBorderSize { 
			get {
				return new Size(Win32GetSystemMetrics(SystemMetrics.SM_CXFRAME), Win32GetSystemMetrics(SystemMetrics.SM_CYFRAME));
			}
		}

		internal override Size IconSize {
			get {
				return new Size(Win32GetSystemMetrics(SystemMetrics.SM_CXICON), Win32GetSystemMetrics(SystemMetrics.SM_CYICON));
			}
		}

		internal override Size MaxWindowTrackSize {
			get {
				return new Size(Win32GetSystemMetrics(SystemMetrics.SM_CXMAXTRACK), Win32GetSystemMetrics(SystemMetrics.SM_CYMAXTRACK));
			}
		}

		internal override Size MinimizedWindowSize {
			get {
				return new Size(Win32GetSystemMetrics(SystemMetrics.SM_CXMINIMIZED), Win32GetSystemMetrics(SystemMetrics.SM_CYMINIMIZED));
			}
		}

		internal override Size MinimizedWindowSpacingSize {
			get {
				return new Size(Win32GetSystemMetrics(SystemMetrics.SM_CXMINSPACING), Win32GetSystemMetrics(SystemMetrics.SM_CYMINSPACING));
			}
		}

		internal override Size MinimumWindowSize {
			get {
				return new Size(Win32GetSystemMetrics(SystemMetrics.SM_CXMIN), Win32GetSystemMetrics(SystemMetrics.SM_CYMIN));
			}
		}

		internal override Size MinWindowTrackSize {
			get {
				return new Size(Win32GetSystemMetrics(SystemMetrics.SM_CXMINTRACK), Win32GetSystemMetrics(SystemMetrics.SM_CYMINTRACK));
			}
		}

		internal override Size SmallIconSize {
			get {
				return new Size(Win32GetSystemMetrics(SystemMetrics.SM_CXSMICON), Win32GetSystemMetrics(SystemMetrics.SM_CYSMICON));
			}
		}

		internal override int MouseButtonCount {
			get {
				return Win32GetSystemMetrics(SystemMetrics.SM_CMOUSEBUTTONS);
			}
		}

		internal override bool MouseButtonsSwapped {
			get {
				return Win32GetSystemMetrics(SystemMetrics.SM_SWAPBUTTON) != 0;
			}
		}

		internal override bool MouseWheelPresent {
			get {
				return Win32GetSystemMetrics(SystemMetrics.SM_MOUSEWHEELPRESENT) != 0;
			}
		}

		internal override Rectangle VirtualScreen {
			get {
				return new Rectangle(	Win32GetSystemMetrics(SystemMetrics.SM_XVIRTUALSCREEN), Win32GetSystemMetrics(SystemMetrics.SM_YVIRTUALSCREEN),
							Win32GetSystemMetrics(SystemMetrics.SM_CXVIRTUALSCREEN), Win32GetSystemMetrics(SystemMetrics.SM_CYVIRTUALSCREEN));
			}
		}

		internal override Rectangle WorkingArea {
			get {
				RECT	rect;

				rect = new RECT();
				Win32SystemParametersInfo(SPIAction.SPI_GETWORKAREA, 0, ref rect, 0);
				return new Rectangle(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
				//return new Rectangle(0, 0, Win32GetSystemMetrics(SystemMetrics.SM.SM_CXSCREEN), Win32GetSystemMetrics(SystemMetrics.SM_CYSCREEN));
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
			Hwnd	hwnd;
			IntPtr	lParam;

			hwnd = new Hwnd();

			ParentHandle=cp.Parent;

			if ((ParentHandle==IntPtr.Zero) && (cp.Style & (int)(WindowStyles.WS_CHILD))!=0) {
				// We need to use our foster parent window until this poor child gets it's parent assigned
				ParentHandle=FosterParent;
			}

			lParam = IntPtr.Zero;
			if (cp.Param != null && cp.Param is CLIENTCREATESTRUCT) {
				lParam = Marshal.AllocHGlobal(Marshal.SizeOf(cp.Param));
				Marshal.StructureToPtr(cp.Param, lParam, false);
			}

			WindowHandle = Win32CreateWindow((uint)cp.ExStyle, cp.ClassName, cp.Caption, (uint)cp.Style, cp.X, cp.Y, cp.Width, cp.Height, ParentHandle, IntPtr.Zero, IntPtr.Zero, lParam);

			if (lParam != IntPtr.Zero) {
				Marshal.FreeHGlobal(lParam);
			}

			if (WindowHandle==IntPtr.Zero) {
				uint error = Win32GetLastError();

				Win32MessageBox(IntPtr.Zero, "Error : " + error.ToString(), "Failed to create window, class '"+cp.ClassName+"'", 0);
			}

			hwnd.ClientWindow = WindowHandle;

			Win32SetWindowLong(WindowHandle, WindowLong.GWL_USERDATA, (uint)ThemeEngine.Current.DefaultControlBackColor.ToArgb());

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
			Hwnd	hwnd;

			hwnd = Hwnd.ObjectFromHandle(handle);
			Win32DestroyWindow(handle);
			hwnd.Dispose();
			return;
		}

		internal override FormWindowState GetWindowState(IntPtr handle) {
			uint style;

			style = Win32GetWindowLong(handle, WindowLong.GWL_STYLE);
			if ((style & (uint)WindowStyles.WS_MAXIMIZE) != 0) {
				return FormWindowState.Maximized;
			} else if ((style & (uint)WindowStyles.WS_MINIMIZE) != 0) {
				return FormWindowState.Minimized;
			}
			return FormWindowState.Normal;
		}

		internal override void SetWindowState(IntPtr hwnd, FormWindowState state) {
			switch(state) {
				case FormWindowState.Normal: {
					Win32ShowWindow(hwnd, WindowPlacementFlags.SW_SHOWNORMAL);
					return;
				}

				case FormWindowState.Minimized: {
					Win32ShowWindow(hwnd, WindowPlacementFlags.SW_SHOWMINIMIZED);
					return;
				}

				case FormWindowState.Maximized: {
					Win32ShowWindow(hwnd, WindowPlacementFlags.SW_SHOWMAXIMIZED);
					return;
				}
			}
		}

		internal override void SetWindowStyle(IntPtr handle, CreateParams cp) {
			Win32SetWindowLong(handle, WindowLong.GWL_STYLE, (uint)cp.Style);
			Win32SetWindowLong(handle, WindowLong.GWL_EXSTYLE, (uint)cp.ExStyle);
		}

		internal override void UpdateWindow(IntPtr handle) {
			Win32UpdateWindow(handle);
		}

		[MonoTODO("FIXME - Add support for internal table of windows/DCs for cleanup; handle client=false to draw in NC area")]
		internal override PaintEventArgs PaintEventStart(IntPtr handle, bool client) {
			IntPtr		hdc;
			PAINTSTRUCT	ps;
			PaintEventArgs	paint_event;
			RECT		rect;
			Rectangle	clip_rect;
			Hwnd		hwnd;

			clip_rect = new Rectangle();
			rect = new RECT();
			ps = new PAINTSTRUCT();

			hwnd = Hwnd.ObjectFromHandle(handle);

			if (Win32GetUpdateRect(handle, ref rect, false)) {
				hdc = Win32BeginPaint(handle, ref ps);

				hwnd.user_data = (object)ps;

				clip_rect = new Rectangle(ps.rcPaint.left, ps.rcPaint.top, ps.rcPaint.right-ps.rcPaint.left, ps.rcPaint.bottom-ps.rcPaint.top);
			} else {
				hdc = Win32GetDC(handle);
				// FIXME: Add the DC to internal list
				clip_rect = new Rectangle(rect.top, rect.left, rect.right-rect.left, rect.bottom-rect.top);
			}

			hwnd.client_dc = Graphics.FromHdc(hdc);
			paint_event = new PaintEventArgs(hwnd.client_dc, clip_rect);

			return paint_event;
		}

		internal override void PaintEventEnd(IntPtr handle, bool client) {
			Hwnd		hwnd;
			PAINTSTRUCT	ps;

			hwnd = Hwnd.ObjectFromHandle(handle);
			hwnd.client_dc.Dispose();

			if (hwnd.user_data != null) {
				ps = (PAINTSTRUCT)hwnd.user_data;
				Win32EndPaint(handle, ref ps);
			}
		}


		internal override void SetWindowPos(IntPtr handle, int x, int y, int width, int height) {
			Win32MoveWindow(handle, x, y, width, height, true);
			return;
		}

		internal override void GetWindowPos(IntPtr handle, bool is_toplevel, out int x, out int y, out int width, out int height, out int client_width, out int client_height) {
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

			if (override_cursor != IntPtr.Zero) {
				Cursor.Current = null;
			}

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
			bool		result;

			if (RetrieveMessage(ref msg)) {
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

				case Msg.WM_ASYNC_MESSAGE: {
					XplatUIDriverSupport.ExecuteClientMessage((GCHandle)msg.lParam);
					break;
				}

				case Msg.WM_MOUSEMOVE: {
					if (msg.hwnd != prev_mouse_hwnd) {
						TRACKMOUSEEVENT	tme;

						// The current message will be sent out next time around
						StoreMessage(ref msg);

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

				case Msg.WM_DROPFILES: {
					return Win32DnD.HandleWMDropFiles(ref msg);
				}

				case Msg.WM_MOUSELEAVE: {
					prev_mouse_hwnd = IntPtr.Zero;
					msg.message=Msg.WM_MOUSE_LEAVE;
					break;
				}

				case Msg.WM_TIMER: {
					Timer timer=(Timer)timer_list[(int)msg.wParam];

					if (timer != null) {
						timer.FireTick();
					}
					break;
				}
			}

			return result;
		}

		internal override bool TranslateMessage(ref MSG msg) {
			return Win32TranslateMessage(ref msg);
		}

		internal override IntPtr DispatchMessage(ref MSG msg) {
			return Win32DispatchMessage(ref msg);
		}

		internal override bool SetZOrder(IntPtr hWnd, IntPtr AfterhWnd, bool Top, bool Bottom) {
			if (Top) {
				Win32SetWindowPos(hWnd, SetWindowPosZOrder.HWND_TOP, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE);
				return true;
			} else if (!Bottom) {
				Win32SetWindowPos(hWnd, AfterhWnd, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE);
			} else {
				Win32SetWindowPos(hWnd, (IntPtr)SetWindowPosZOrder.HWND_BOTTOM, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE);
				return true;
			}
			return false;
		}

		internal override bool SetTopmost(IntPtr hWnd, IntPtr hWndOwner, bool Enabled) {
			if (Enabled) {
				Win32SetWindowPos(hWnd, SetWindowPosZOrder.HWND_TOPMOST, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE);
				return true;
			} else {
				Win32SetWindowPos(hWnd, SetWindowPosZOrder.HWND_NOTOPMOST, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE);
				return true;
			}
		}

		internal override bool Text(IntPtr handle, string text) {
			Win32SetWindowText(handle, text);
			return true;
		}

		internal override bool GetText(IntPtr handle, out string text) {
			StringBuilder sb;

			sb = new StringBuilder(256);
			Win32GetWindowText(handle, sb, sb.Capacity);
			text = sb.ToString();
			return true;
		}

		internal override bool SetVisible(IntPtr handle, bool visible) {
			if (visible) {
				Win32ShowWindow(handle, WindowPlacementFlags.SW_SHOWNORMAL);
			} else {
				Win32ShowWindow(handle, WindowPlacementFlags.SW_HIDE);
			}
			return true;
		}

		internal override bool IsVisible(IntPtr handle) {
			return IsWindowVisible (handle);
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

		internal override void UngrabWindow(IntPtr hWnd) {
			Win32ReleaseCapture();
			grab_hwnd = IntPtr.Zero;
		}

		internal override bool CalculateWindowRect(IntPtr hWnd, ref Rectangle ClientRect, int Style, int ExStyle, IntPtr MenuHandle, out Rectangle WindowRect) {
			RECT	rect;

			rect.left=ClientRect.Left;
			rect.top=ClientRect.Top;
			rect.right=ClientRect.Right;
			rect.bottom=ClientRect.Bottom;

			if (!Win32AdjustWindowRectEx(ref rect, Style, MenuHandle != IntPtr.Zero, ExStyle)) {
				WindowRect = new Rectangle(ClientRect.Left, ClientRect.Top, ClientRect.Width, ClientRect.Height);
				return false;
			}

			WindowRect = new Rectangle(rect.left, rect.top, rect.right-rect.left, rect.bottom-rect.top);
			return true;
		}

		internal override void SetCursor(IntPtr window, IntPtr cursor) {
			if (override_cursor == IntPtr.Zero) {
				Win32SetCursor(cursor);
				return;
			}
			Win32SetCursor(override_cursor);
		}

		internal override void ShowCursor(bool show) {
			Win32ShowCursor(show);
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
			Color	pixel;
			int	width;
			int	height;

			// Win32 only allows creation cursors of a certain size
			if ((bitmap.Width != Win32GetSystemMetrics(SystemMetrics.SM_CXCURSOR)) || (bitmap.Width != Win32GetSystemMetrics(SystemMetrics.SM_CXCURSOR))) {
				cursor_bitmap = new Bitmap(bitmap, new Size(Win32GetSystemMetrics(SystemMetrics.SM_CXCURSOR), Win32GetSystemMetrics(SystemMetrics.SM_CXCURSOR)));
				cursor_mask = new Bitmap(mask, new Size(Win32GetSystemMetrics(SystemMetrics.SM_CXCURSOR), Win32GetSystemMetrics(SystemMetrics.SM_CXCURSOR)));
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
					pixel = cursor_bitmap.GetPixel(x, y);

					if (pixel == cursor_pixel) {
						cursor_bits[y * width / 8 + x / 8] |= (byte)(0x80 >> (x % 8));
					}

					pixel = cursor_mask.GetPixel(x, y);

					if (pixel == mask_pixel) {
						mask_bits[y * width / 8 + x / 8] |= (byte)(0x80 >> (x % 8));
					}
				}
			}

			cursor = Win32CreateCursor(IntPtr.Zero, xHotSpot, yHotSpot, width, height, mask_bits, cursor_bits);

			return cursor;
		}

		[MonoTODO("Define the missing cursors")]
		internal override IntPtr DefineStdCursor(StdCursor id) {
			switch(id) {
				case StdCursor.AppStarting:	return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_APPSTARTING);
				case StdCursor.Arrow:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);
				case StdCursor.Cross:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_CROSS);
				case StdCursor.Default:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);
				case StdCursor.Hand:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_HAND);
				case StdCursor.Help:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_HELP);
				case StdCursor.HSplit:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);		// FIXME
				case StdCursor.IBeam:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_IBEAM);
				case StdCursor.No:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_NO);
				case StdCursor.NoMove2D:	return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);		// FIXME
				case StdCursor.NoMoveHoriz:	return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);		// FIXME
				case StdCursor.NoMoveVert:	return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);		// FIXME
				case StdCursor.PanEast:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);		// FIXME
				case StdCursor.PanNE:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);		// FIXME
				case StdCursor.PanNorth:	return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);		// FIXME
				case StdCursor.PanNW:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);		// FIXME
				case StdCursor.PanSE:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);		// FIXME
				case StdCursor.PanSouth:	return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);		// FIXME
				case StdCursor.PanSW:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);		// FIXME
				case StdCursor.PanWest:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);		// FIXME
				case StdCursor.SizeAll:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_SIZEALL);
				case StdCursor.SizeNESW:	return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_SIZENESW);
				case StdCursor.SizeNS:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_SIZENS);
				case StdCursor.SizeNWSE:	return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_SIZENWSE);
				case StdCursor.SizeWE:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_SIZEWE);
				case StdCursor.UpArrow:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_UPARROW);
				case StdCursor.VSplit:		return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_ARROW);		// FIXME
				case StdCursor.WaitCursor:	return Win32LoadCursor(IntPtr.Zero, LoadCursorType.IDC_WAIT);
			}
			throw new NotImplementedException ();
		}

		internal override void DestroyCursor(IntPtr cursor) {
			if ((cursor.ToInt32() < (int)LoadCursorType.First) || (cursor.ToInt32() > (int)LoadCursorType.Last)) {
				Win32DestroyCursor(cursor);
			}
		}

		[MonoTODO]
		internal override void GetCursorInfo(IntPtr cursor, out int width, out int height, out int hotspot_x, out int hotspot_y) {
			throw new NotImplementedException ();
		}

		internal override void SetCursorPos(IntPtr handle, int x, int y) {
			Win32SetCursorPos(x, y);
		}

		internal override void EnableWindow(IntPtr handle, bool Enable) {
			Win32EnableWindow(handle, Enable);
		}

		internal override void SetModal(IntPtr handle, bool Modal) {
			// we do nothing on Win32; Application.cs simulates modal dialogs by disabling all toplevel windows
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
			POINT pnt = new POINT();			

			pnt.x = x;
			pnt.y = y;
			Win32ScreenToClient (handle, ref pnt);

			x = pnt.x;
			y = pnt.y;
		}

		internal override void ClientToScreen(IntPtr handle, ref int x, ref int y) {			
			POINT	pnt = new POINT();			

			pnt.x = x;
			pnt.y = y;

			Win32ClientToScreen(handle, ref pnt);

			x = pnt.x;
			y = pnt.y;
		}

		internal override void ScreenToMenu(IntPtr handle, ref int x, ref int y) {
			ScreenToClient(handle, ref x, ref y);
		}

		internal override void MenuToScreen(IntPtr handle, ref int x, ref int y) {			
			ClientToScreen(handle, ref x, ref y);
		}

		internal override void SendAsyncMethod (AsyncMethodData method)
		{
			Win32PostMessage(FosterParent, Msg.WM_ASYNC_MESSAGE, IntPtr.Zero, (IntPtr)GCHandle.Alloc (method));
		}

		internal override void SetTimer (Timer timer)
		{
			int	index;

			index = timer.GetHashCode();

			lock (timer_list) {
				timer_list[index]=timer;
			}

			Win32SetTimer(FosterParent, index, (uint)timer.Interval, IntPtr.Zero);
		}

		internal override void KillTimer (Timer timer)
		{
			int	index;

			index = timer.GetHashCode();

			Win32KillTimer(FosterParent, index);

			lock (timer_list) {
				timer_list.Remove(index);
			}
		}


		private void CaretCallback(object sender, EventArgs e) {
			Console.WriteLine("CaretCallback hit");
		}

		internal override void CreateCaret(IntPtr hwnd, int width, int height) {
			Win32CreateCaret(hwnd, IntPtr.Zero, width, height);
		}

		internal override void DestroyCaret(IntPtr hwnd) {
			Win32DestroyCaret();
		}

		internal override void SetCaretPos(IntPtr hwnd, int x, int y) {
			Win32SetCaretPos(x, y);
		}

		internal override void CaretVisible(IntPtr hwnd, bool visible) {
			if (visible) {
				Win32ShowCaret(hwnd);
			} else {
				Win32HideCaret(hwnd);
			}
		}

		internal override void SetFocus(IntPtr hwnd) {
			Win32SetFocus(hwnd);
		}

		internal override IntPtr GetActive() {
			return Win32GetActiveWindow();
		}

		internal override bool GetFontMetrics(Graphics g, Font font, out int ascent, out int descent) {
			IntPtr		dc;
			TEXTMETRIC	tm;

			tm = new TEXTMETRIC();

			dc = Win32GetDC(IntPtr.Zero);
			Win32SelectObject(dc, font.ToHfont());
			if (Win32GetTextMetrics(dc, ref tm) == false) {
				Win32ReleaseDC(IntPtr.Zero, dc);
				ascent = 0;
				descent = 0;
				return false;
			}
			Win32ReleaseDC(IntPtr.Zero, dc);

			ascent = tm.tmAscent;
			descent = tm.tmDescent;

			return true;
		}

		internal override void ScrollWindow(IntPtr hwnd, Rectangle rectangle, int XAmount, int YAmount, bool with_children) {
			RECT	rect;

			rect = new RECT();
			rect.left = rectangle.X;
			rect.top = rectangle.Y;
			rect.right = rectangle.Right;
			rect.bottom = rectangle.Bottom;

			Win32ScrollWindowEx(hwnd, XAmount, YAmount, ref rect, ref rect, IntPtr.Zero, IntPtr.Zero, ScrollWindowExFlags.SW_INVALIDATE | ScrollWindowExFlags.SW_ERASE | (with_children ? ScrollWindowExFlags.SW_SCROLLCHILDREN : ScrollWindowExFlags.SW_NONE));
			Win32UpdateWindow(hwnd);
		}

		internal override void ScrollWindow(IntPtr hwnd, int XAmount, int YAmount, bool with_children) {
			Win32ScrollWindowEx(hwnd, XAmount, YAmount, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, ScrollWindowExFlags.SW_INVALIDATE | ScrollWindowExFlags.SW_ERASE | (with_children ? ScrollWindowExFlags.SW_SCROLLCHILDREN : ScrollWindowExFlags.SW_NONE));
		}

		internal override bool SystrayAdd(IntPtr hwnd, string tip, Icon icon, out ToolTip tt) {
			NOTIFYICONDATA	nid;

			nid = new NOTIFYICONDATA();

			nid.cbSize = (uint)Marshal.SizeOf(nid);
			nid.hWnd = hwnd;
			nid.uID = 1;
			nid.uCallbackMessage = (uint)Msg.WM_USER;
			nid.uFlags = NotifyIconFlags.NIF_MESSAGE;

			if (tip != null) {	 
				nid.szTip = tip;
				nid.uFlags |= NotifyIconFlags.NIF_TIP;
			}

			if (icon != null) {
				nid.hIcon = icon.Handle;
				nid.uFlags |= NotifyIconFlags.NIF_ICON;
			}

			tt = null;

			return Win32Shell_NotifyIcon(NotifyIconMessage.NIM_ADD, ref nid);
		}

		internal override bool SystrayChange(IntPtr hwnd, string tip, Icon icon, ref ToolTip tt) {
			NOTIFYICONDATA	nid;

			nid = new NOTIFYICONDATA();

			nid.cbSize = (uint)Marshal.SizeOf(nid);
			nid.hIcon = icon.Handle;
			nid.hWnd = hwnd;
			nid.uID = 1;
			nid.uCallbackMessage = (uint)Msg.WM_USER;
			nid.uFlags = NotifyIconFlags.NIF_MESSAGE;

			if (tip != null) {
				nid.szTip = tip;
				nid.uFlags |= NotifyIconFlags.NIF_TIP;
			}

			if (icon != null) {
				nid.hIcon = icon.Handle;
				nid.uFlags |= NotifyIconFlags.NIF_ICON;
			}

			return Win32Shell_NotifyIcon(NotifyIconMessage.NIM_MODIFY, ref nid);
		}

		internal override void SystrayRemove(IntPtr hwnd, ref ToolTip tt) {
			NOTIFYICONDATA	nid;

			nid = new NOTIFYICONDATA();

			nid.cbSize = (uint)Marshal.SizeOf(nid);
			nid.hWnd = hwnd;
			nid.uID = 1;
			nid.uFlags = 0;

			Win32Shell_NotifyIcon(NotifyIconMessage.NIM_DELETE, ref nid);
		}


		internal override void SetBorderStyle(IntPtr handle, FormBorderStyle border_style) {
			uint	style;
			uint	exstyle;

			style = Win32GetWindowLong(handle, WindowLong.GWL_STYLE);
			exstyle = Win32GetWindowLong(handle, WindowLong.GWL_EXSTYLE);

			switch (border_style) {
				case FormBorderStyle.None: {
					style &= ~(uint)WindowStyles.WS_BORDER;
					exstyle &= ~(uint)WindowStyles.WS_EX_CLIENTEDGE;
					break;
				}

				case FormBorderStyle.FixedSingle: {
					style |= (uint)WindowStyles.WS_BORDER;
					exstyle &= ~(uint)WindowStyles.WS_EX_CLIENTEDGE;
					break;
				}

				case FormBorderStyle.Fixed3D: {
					style &= ~(uint)WindowStyles.WS_BORDER;
					exstyle |= (uint)WindowStyles.WS_EX_CLIENTEDGE;
					break;
				}
			}

			Win32SetWindowLong(handle, WindowLong.GWL_STYLE, style);
			Win32SetWindowLong(handle, WindowLong.GWL_EXSTYLE, exstyle);
			
			Win32SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, 
				SetWindowPosFlags.SWP_FRAMECHANGED | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE | SetWindowPosFlags.SWP_NOACTIVATE);
		}

		internal override void SetMenu(IntPtr handle, IntPtr menu_handle) {
			// Trigger WM_NCCALC
			Win32SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, SetWindowPosFlags.SWP_FRAMECHANGED | SetWindowPosFlags.SWP_NOMOVE | SetWindowPosFlags.SWP_NOSIZE);
		}


		internal override Graphics GetMenuDC(IntPtr hwnd, IntPtr ncpaint_region) {
			IntPtr		hdc;
			Graphics	g;

			// GDI+ Broken:
			// hdc = Win32GetDCEx(hwnd, ncpaint_region, DCExFlags.DCX_WINDOW | DCExFlags.DCX_INTERSECTRGN | DCExFlags.DCX_USESTYLE);
			hdc = Win32GetDCEx(hwnd, ncpaint_region, DCExFlags.DCX_WINDOW);

			g = Graphics.FromHdc(hdc);

			Win32ReleaseDC(hwnd, hdc);

			return g;
		}

		internal override Point GetMenuOrigin(IntPtr handle) {
			return new Point(SystemInformation.FrameBorderSize.Width, SystemInformation.FrameBorderSize.Height + ThemeEngine.Current.CaptionHeight);
		}


		internal override void ReleaseMenuDC(IntPtr hwnd, Graphics dc) {
			dc.Dispose();
		}

		internal override void SetIcon(IntPtr hwnd, Icon icon) {
			Win32SendMessage(hwnd, Msg.WM_SETICON, (IntPtr)1, icon.Handle);	// 1 = large icon (0 would be small)
		}

		internal override void ClipboardClose(IntPtr handle) {
			if (handle != clip_magic) {
				throw new ArgumentException("handle is not a valid clipboard handle");
			}
			Win32CloseClipboard();
		}

		internal override int ClipboardGetID(IntPtr handle, string format) {
			if (handle != clip_magic) {
				throw new ArgumentException("handle is not a valid clipboard handle");
			}
			if (format == "Text" ) return 1;
			else if (format == "Bitmap" ) return 2;
			else if (format == "MetaFilePict" ) return 3;
			else if (format == "SymbolicLink" ) return 4;
			else if (format == "DataInterchangeFormat" ) return 5;
			else if (format == "Tiff" ) return 6;
			else if (format == "OEMText" ) return 7;
			else if (format == "DeviceIndependentBitmap" ) return 8;
			else if (format == "Palette" ) return 9;
			else if (format == "PenData" ) return 10;
			else if (format == "RiffAudio" ) return 11;
			else if (format == "WaveAudio" ) return 12;
			else if (format == "UnicodeText" ) return 13;
			else if (format == "EnhancedMetafile" ) return 14;
			else if (format == "FileDrop" ) return 15;
			else if (format == "Locale" ) return 16;

			return (int)Win32RegisterClipboardFormat(format);
		}

		internal override IntPtr ClipboardOpen() {
			Win32OpenClipboard(FosterParent);
			return clip_magic;
		}

		internal override int[] ClipboardAvailableFormats(IntPtr handle) {
			uint	format;
			int[]	result;
			int	count;

			if (handle != clip_magic) {
				return null;
			}

			// Count first
			count = 0;
			format = 0;
			do {
				format = Win32EnumClipboardFormats(format);
				if (format != 0) {
					count++;
				}
			} while (format != 0);

			// Now assign
			result = new int[count];
			count = 0;
			format = 0;
			do {
				format = Win32EnumClipboardFormats(format);
				if (format != 0) {
					result[count++] = (int)format;
				}
			} while (format != 0);

			return result;
		}


		internal override object ClipboardRetrieve(IntPtr handle, int type, XplatUI.ClipboardToObject converter) {
			IntPtr	hmem;
			IntPtr	data;
			object	obj;

			if (handle != clip_magic) {
				throw new ArgumentException("handle is not a valid clipboard handle");
			}

			hmem = Win32GetClipboardData((uint)type);
			if (hmem == IntPtr.Zero) {
				return null;
			}

			data = Win32GlobalLock(hmem);
			if (data == IntPtr.Zero) {
				uint error = Win32GetLastError();
				Console.WriteLine("Error: {0}", error);
				return null;
			}

			obj = null;

			switch ((ClipboardFormats)type) {
				case ClipboardFormats.CF_TEXT: {
					obj = AnsiToString(data);
					break;
				}

				case ClipboardFormats.CF_DIB: {
					obj = DIBtoImage(data);
					break;
				}

				case ClipboardFormats.CF_UNICODETEXT: {
					obj = UnicodeToString(data);
					break;
				}

				default: {
					if (converter != null && !converter(type, data, out obj)) {
						obj = null;
					}
					break;
				}
			}
			Win32GlobalUnlock(hmem);

			return obj;

		}

		internal override void ClipboardStore(IntPtr handle, object obj, int type, XplatUI.ObjectToClipboard converter) {
			byte[]	data;
			IntPtr	hmem;
			IntPtr	hmem_ptr;

			if (handle != clip_magic) {
				throw new ArgumentException("handle is not a valid clipboard handle");
			}

			if (obj == null) {
				// Just clear it
				Win32EmptyClipboard();
				return;
			}

			if (type == -1) {
				if (obj is string) {
					type = (int)ClipboardFormats.CF_UNICODETEXT;
				} else if (obj is Image) {
					type = (int)ClipboardFormats.CF_DIB;
				}
			}

			switch((ClipboardFormats)type) {
				case ClipboardFormats.CF_UNICODETEXT: {
					hmem = Marshal.StringToHGlobalUni((string)obj);
					Win32EmptyClipboard();
					Win32SetClipboardData((uint)type, hmem);
					return;
				}

				case ClipboardFormats.CF_DIB: {
					data = ImageToDIB((Image)obj);

					hmem = Win32GlobalAlloc(GAllocFlags.GMEM_MOVEABLE | GAllocFlags.GMEM_DDESHARE, data.Length);
					hmem_ptr = Win32GlobalLock(hmem);
					Marshal.Copy(data, 0, hmem_ptr, data.Length);
					Win32GlobalUnlock(hmem);
					Win32EmptyClipboard();
					Win32SetClipboardData((uint)type, hmem);
					return;
				}

				default: {
					if (converter != null && converter(ref type, obj, out data)) {
						hmem = Win32GlobalAlloc(GAllocFlags.GMEM_MOVEABLE | GAllocFlags.GMEM_DDESHARE, data.Length);
						hmem_ptr = Win32GlobalLock(hmem);
						Marshal.Copy(data, 0, hmem_ptr, data.Length);
						Win32GlobalUnlock(hmem);
						Win32EmptyClipboard();
						Win32SetClipboardData((uint)type, hmem);
					}
					return;
				}
			}
		}

		internal override void SetAllowDrop(IntPtr hwnd, bool allowed) {
			if (allowed) {
				Win32DnD.RegisterDropTarget(hwnd);
			} else {
				Win32DnD.UnregisterDropTarget(hwnd);
			}
		}

		internal override DragDropEffects StartDrag(IntPtr hwnd, object data, DragDropEffects allowedEffects) {
			return Win32DnD.StartDrag(hwnd, data, allowedEffects);
		}

		internal override void DrawReversibleRectangle(IntPtr handle, Rectangle rect, int line_width) {
			IntPtr		hdc;
			IntPtr		pen;
			IntPtr		oldpen;
			POINT		pt;

			pt = new POINT();
			pt.x = 0;
			pt.y = 0;
			Win32ClientToScreen(handle, ref pt);

			// If we want the standard hatch pattern we would
			// need to create a brush

			// Grab a pen
			pen = Win32CreatePen(PenStyle.PS_SOLID, line_width, IntPtr.Zero);

			hdc = Win32GetDC(IntPtr.Zero);
			Win32SetROP2(hdc, ROP2DrawMode.R2_NOT);
			oldpen = Win32SelectObject(hdc, pen);

			// We might need to add clipping to the WindowRect of 'handle' - right now we're drawing on the desktop

			Win32MoveToEx(hdc, pt.x + rect.Left, pt.y + rect.Top, IntPtr.Zero);
			if ((rect.Width > 0) && (rect.Height > 0)) {
				Win32LineTo(hdc, pt.x + rect.Right, pt.y + rect.Top);
				Win32LineTo(hdc, pt.x + rect.Right, pt.y + rect.Bottom);
				Win32LineTo(hdc, pt.x + rect.Left, pt.y + rect.Bottom);
				Win32LineTo(hdc, pt.x + rect.Left, pt.y + rect.Top);
			} else {
				if (rect.Width > 0) {
					Win32LineTo(hdc, pt.x + rect.Right, pt.y + rect.Top);
				} else {
					Win32LineTo(hdc, pt.x + rect.Left, pt.y + rect.Bottom);
				}
			}

			Win32SelectObject(hdc, oldpen);
			Win32DeleteObject(pen);
			Win32ReleaseDC(IntPtr.Zero, hdc);
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

		internal override int KeyboardSpeed {
			get {
				Console.WriteLine ("KeyboardSpeed: need to query Windows");

				//
				// Return values range from 0 to 31 which map to 2.5 to 30 repetitions per second.
				//
				return 0;
			}
		}

		internal override int KeyboardDelay {
			get {
				Console.WriteLine ("KeyboardDelay: need to query Windows");

				//
				// Return values must range from 0 to 4, 0 meaning 250ms,
				// and 4 meaning 1000 ms.
				//
				return 1;
			}
		}
		
		internal override event EventHandler Idle;

		// Santa's little helper
		static void Where() {
			Console.WriteLine("Here: {0}", new StackTrace().ToString());
		}
		#endregion	// Public Static Methods

		#region Win32 Imports
		[DllImport ("kernel32.dll", EntryPoint="GetLastError", CallingConvention=CallingConvention.StdCall)]
		private extern static uint Win32GetLastError();

		[DllImport ("user32.dll", EntryPoint="CreateWindowExW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32CreateWindow(uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lParam);

		[DllImport ("user32.dll", EntryPoint="DestroyWindow", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32DestroyWindow(IntPtr hWnd);

		[DllImport ("user32.dll", EntryPoint="PeekMessageW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32PeekMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags);

		[DllImport ("user32.dll", EntryPoint="GetMessageW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32GetMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax);

		[DllImport ("user32.dll", EntryPoint="TranslateMessage", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32TranslateMessage(ref MSG msg);

		[DllImport ("user32.dll", EntryPoint="DispatchMessageW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32DispatchMessage(ref MSG msg);

		[DllImport ("user32.dll", EntryPoint="MoveWindow", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);

		[DllImport ("user32.dll", EntryPoint="SetWindowPos", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, SetWindowPosFlags Flags);

		[DllImport ("user32.dll", EntryPoint="SetWindowPos", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32SetWindowPos(IntPtr hWnd, SetWindowPosZOrder pos, int x, int y, int cx, int cy, SetWindowPosFlags Flags);

		[DllImport ("user32.dll", EntryPoint="SetWindowTextW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32SetWindowText(IntPtr hWnd, string lpString);

		[DllImport ("user32.dll", EntryPoint="GetWindowTextW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

		[DllImport ("user32.dll", EntryPoint="SetParent", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32SetParent(IntPtr hWnd, IntPtr hParent);

		[DllImport ("user32.dll", EntryPoint="RegisterClassW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32RegisterClass(ref WNDCLASS wndClass);

		[DllImport ("user32.dll", EntryPoint="LoadCursorW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32LoadCursor(IntPtr hInstance, LoadCursorType type);

		[DllImport ("user32.dll", EntryPoint="ShowCursor", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32ShowCursor(bool bShow);

		[DllImport ("user32.dll", EntryPoint="SetCursor", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32SetCursor(IntPtr hCursor);

		[DllImport ("user32.dll", EntryPoint="CreateCursor", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32CreateCursor(IntPtr hInstance, int xHotSpot, int yHotSpot, int nWidth, int nHeight, Byte[] pvANDPlane, Byte[] pvORPlane);

		[DllImport ("user32.dll", EntryPoint="DestroyCursor", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32DestroyCursor(IntPtr hCursor);

		[DllImport ("user32.dll", EntryPoint="DefWindowProcW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32DefWindowProc(IntPtr hWnd, Msg Msg, IntPtr wParam, IntPtr lParam);

		[DllImport ("user32.dll", EntryPoint="DefDlgProcW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32DefDlgProc(IntPtr hWnd, Msg Msg, IntPtr wParam, IntPtr lParam);

		[DllImport ("user32.dll", EntryPoint="PostQuitMessage", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32PostQuitMessage(int nExitCode);

		[DllImport ("user32.dll", EntryPoint="UpdateWindow", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32UpdateWindow(IntPtr hWnd);

		[DllImport ("user32.dll", EntryPoint="GetUpdateRect", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32GetUpdateRect(IntPtr hWnd, ref RECT rect, bool erase);

		[DllImport ("user32.dll", EntryPoint="BeginPaint", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32BeginPaint(IntPtr hWnd, ref PAINTSTRUCT ps);

		[DllImport ("user32.dll", EntryPoint="EndPaint", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32EndPaint(IntPtr hWnd, ref PAINTSTRUCT ps);

		[DllImport ("user32.dll", EntryPoint="GetDC", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32GetDC(IntPtr hWnd);

		[DllImport ("user32.dll", EntryPoint="GetDCEx", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32GetDCEx(IntPtr hWnd, IntPtr hRgn, DCExFlags flags);

		[DllImport ("user32.dll", EntryPoint="ReleaseDC", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32ReleaseDC(IntPtr hWnd, IntPtr hDC);

		[DllImport ("user32.dll", EntryPoint="MessageBoxW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32MessageBox(IntPtr hParent, string pText, string pCaption, uint uType);

		[DllImport ("user32.dll", EntryPoint="InvalidateRect", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32InvalidateRect(IntPtr hWnd, ref RECT lpRect, bool bErase);

		[DllImport ("user32.dll", EntryPoint="InvalidateRect", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

		[DllImport ("user32.dll", EntryPoint="SetCapture", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32SetCapture(IntPtr hWnd);

		[DllImport ("user32.dll", EntryPoint="ReleaseCapture", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32ReleaseCapture();

		[DllImport ("user32.dll", EntryPoint="GetWindowRect", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32GetWindowRect(IntPtr hWnd, out RECT rect);

		[DllImport ("user32.dll", EntryPoint="GetClientRect", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32GetClientRect(IntPtr hWnd, out RECT rect);

		[DllImport ("user32.dll", EntryPoint="ScreenToClient", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32ScreenToClient(IntPtr hWnd, ref POINT pt);

		[DllImport ("user32.dll", EntryPoint="ClientToScreen", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32ClientToScreen(IntPtr hWnd, ref POINT pt);

		[DllImport ("user32.dll", EntryPoint="GetParent", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32GetParent(IntPtr hWnd);

		[DllImport ("user32.dll", EntryPoint="SetActiveWindow", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32SetActiveWindow(IntPtr hWnd);

		[DllImport ("user32.dll", EntryPoint="AdjustWindowRectEx", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32AdjustWindowRectEx(ref RECT lpRect, int dwStyle, bool bMenu, int dwExStyle);

		[DllImport ("user32.dll", EntryPoint="GetCursorPos", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32GetCursorPos(out POINT lpPoint);

		[DllImport ("user32.dll", EntryPoint="SetCursorPos", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32SetCursorPos(int x, int y);

		[DllImport ("user32.dll", EntryPoint="GetWindowPlacement", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

		[DllImport ("user32.dll", EntryPoint="TrackMouseEvent", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32TrackMouseEvent(ref TRACKMOUSEEVENT tme);

		[DllImport ("gdi32.dll", EntryPoint="CreateBrushIndirect", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32CreateBrushIndirect(ref LOGBRUSH lb);

		[DllImport ("user32.dll", EntryPoint="FillRect", CallingConvention=CallingConvention.StdCall)]
		private extern static int Win32FillRect(IntPtr hdc, ref RECT rect, IntPtr hbr);

		[DllImport ("user32.dll", EntryPoint="SetWindowLong", CallingConvention=CallingConvention.StdCall)]
		private extern static uint Win32SetWindowLong(IntPtr hwnd, WindowLong index, uint value);

		[DllImport ("user32.dll", EntryPoint="GetWindowLong", CallingConvention=CallingConvention.StdCall)]
		private extern static uint Win32GetWindowLong(IntPtr hwnd, WindowLong index);

		[DllImport ("gdi32.dll", EntryPoint="DeleteObject", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32DeleteObject(IntPtr o);

		[DllImport ("user32.dll", EntryPoint="PostMessage", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32PostMessage(IntPtr hwnd, Msg msg, IntPtr wParam, IntPtr lParam);

		[DllImport ("user32.dll", EntryPoint="GetKeyState", CallingConvention=CallingConvention.StdCall)]
		private extern static short Win32GetKeyState(VirtualKeys nVirtKey);

		[DllImport ("user32.dll", EntryPoint="GetDesktopWindow", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32GetDesktopWindow();

		[DllImport ("user32.dll", EntryPoint="SetTimer", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32SetTimer(IntPtr hwnd, int nIDEvent, uint uElapse, IntPtr timerProc);

		[DllImport ("user32.dll", EntryPoint="KillTimer", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32KillTimer(IntPtr hwnd, int nIDEvent);

		[DllImport ("user32.dll", EntryPoint="ShowWindow", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32ShowWindow(IntPtr hwnd, WindowPlacementFlags nCmdShow);

		[DllImport ("user32.dll", EntryPoint="EnableWindow", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32EnableWindow(IntPtr hwnd, bool Enabled);

		[DllImport ("user32.dll", EntryPoint="SetFocus", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32SetFocus(IntPtr hwnd);

		[DllImport ("user32.dll", EntryPoint="CreateCaret", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32CreateCaret(IntPtr hwnd, IntPtr hBitmap, int nWidth, int nHeight);

		[DllImport ("user32.dll", EntryPoint="DestroyCaret", CallingConvention=CallingConvention.StdCall)]
		private  extern static bool Win32DestroyCaret();

		[DllImport ("user32.dll", EntryPoint="ShowCaret", CallingConvention=CallingConvention.StdCall)]
		private  extern static bool Win32ShowCaret(IntPtr hwnd);

		[DllImport ("user32.dll", EntryPoint="HideCaret", CallingConvention=CallingConvention.StdCall)]
		private  extern static bool Win32HideCaret(IntPtr hwnd);

		[DllImport ("user32.dll", EntryPoint="SetCaretPos", CallingConvention=CallingConvention.StdCall)]
		private  extern static bool Win32SetCaretPos(int X, int Y);

		[DllImport ("user32.dll", EntryPoint="GetCaretBlinkTime", CallingConvention=CallingConvention.StdCall)]
		private  extern static uint Win32GetCaretBlinkTime();

		[DllImport ("gdi32.dll", EntryPoint="GetTextMetricsW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32GetTextMetrics(IntPtr hdc, ref TEXTMETRIC tm);

		[DllImport ("gdi32.dll", EntryPoint="SelectObject", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32SelectObject(IntPtr hdc, IntPtr hgdiobject);

		[DllImport ("user32.dll", EntryPoint="ScrollWindowEx", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32ScrollWindowEx(IntPtr hwnd, int dx, int dy, ref RECT prcScroll, ref RECT prcClip, IntPtr hrgnUpdate, out RECT prcUpdate, ScrollWindowExFlags flags);

		[DllImport ("user32.dll", EntryPoint="ScrollWindowEx", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32ScrollWindowEx(IntPtr hwnd, int dx, int dy, ref RECT prcScroll, ref RECT prcClip, IntPtr hrgnUpdate, IntPtr prcUpdate, ScrollWindowExFlags flags);

		[DllImport ("user32.dll", EntryPoint="ScrollWindowEx", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32ScrollWindowEx(IntPtr hwnd, int dx, int dy, ref RECT prcScroll, IntPtr prcClip, IntPtr hrgnUpdate, IntPtr prcUpdate, ScrollWindowExFlags flags);

		[DllImport ("user32.dll", EntryPoint="ScrollWindowEx", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32ScrollWindowEx(IntPtr hwnd, int dx, int dy, IntPtr prcScroll, IntPtr prcClip, IntPtr hrgnUpdate, IntPtr prcUpdate, ScrollWindowExFlags flags);

		[DllImport ("user32.dll", EntryPoint="GetActiveWindow", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32GetActiveWindow();

		[DllImport ("user32.dll", EntryPoint="GetSystemMetrics", CallingConvention=CallingConvention.StdCall)]
		private extern static int Win32GetSystemMetrics(SystemMetrics nIndex);

		[DllImport ("shell32.dll", EntryPoint="Shell_NotifyIconW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32Shell_NotifyIcon(NotifyIconMessage dwMessage, ref NOTIFYICONDATA lpData);

		[DllImport ("gdi32.dll", EntryPoint="CreateRectRgn", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

		[DllImport ("user32.dll", EntryPoint="IsWindowVisible", CallingConvention=CallingConvention.StdCall)]
		private extern static bool IsWindowVisible(IntPtr hwnd);

		[DllImport ("user32.dll", EntryPoint="SetClassLong", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32SetClassLong(IntPtr hwnd, ClassLong nIndex, IntPtr dwNewLong);

		[DllImport ("user32.dll", EntryPoint="SendMessageW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32SendMessage(IntPtr hwnd, Msg msg, IntPtr wParam, IntPtr lParam);

		[DllImport ("user32.dll", EntryPoint="SystemParametersInfoW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32SystemParametersInfo(SPIAction uiAction, uint uiParam, ref RECT rect, uint fWinIni);

		[DllImport ("user32.dll", EntryPoint="OpenClipboard", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32OpenClipboard(IntPtr hwnd);

		[DllImport ("user32.dll", EntryPoint="EmptyClipboard", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32EmptyClipboard();

		[DllImport ("user32.dll", EntryPoint="RegisterClipboardFormatW", CharSet=CharSet.Unicode, CallingConvention=CallingConvention.StdCall)]
		private extern static uint Win32RegisterClipboardFormat(string format);

		[DllImport ("user32.dll", EntryPoint="CloseClipboard", CallingConvention=CallingConvention.StdCall)]
		private extern static bool Win32CloseClipboard();

		[DllImport ("user32.dll", EntryPoint="EnumClipboardFormats", CallingConvention=CallingConvention.StdCall)]
		private extern static uint Win32EnumClipboardFormats(uint format);

		[DllImport ("user32.dll", EntryPoint="GetClipboardData", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32GetClipboardData(uint format);

		[DllImport ("user32.dll", EntryPoint="SetClipboardData", CallingConvention=CallingConvention.StdCall)]
		private extern static IntPtr Win32SetClipboardData(uint format, IntPtr handle);

		[DllImport ("kernel32.dll", EntryPoint="GlobalAlloc", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32GlobalAlloc(GAllocFlags Flags, int dwBytes);

		[DllImport ("kernel32.dll", EntryPoint="CopyMemory", CallingConvention=CallingConvention.StdCall)]
		internal extern static void Win32CopyMemory(IntPtr Destination, IntPtr Source, int length);

		[DllImport ("kernel32.dll", EntryPoint="GlobalFree", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32GlobalFree(IntPtr hMem);

		[DllImport ("kernel32.dll", EntryPoint="GlobalSize", CallingConvention=CallingConvention.StdCall)]
		internal extern static uint Win32GlobalSize(IntPtr hMem);

		[DllImport ("kernel32.dll", EntryPoint="GlobalLock", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32GlobalLock(IntPtr hMem);

		[DllImport ("kernel32.dll", EntryPoint="GlobalUnlock", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32GlobalUnlock(IntPtr hMem);

		[DllImport ("gdi32.dll", EntryPoint="SetROP2", CallingConvention=CallingConvention.StdCall)]
		internal extern static int Win32SetROP2(IntPtr hdc, ROP2DrawMode fnDrawMode);

		[DllImport ("gdi32.dll", EntryPoint="MoveToEx", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32MoveToEx(IntPtr hdc, int x, int y, ref POINT lpPoint);

		[DllImport ("gdi32.dll", EntryPoint="MoveToEx", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32MoveToEx(IntPtr hdc, int x, int y, IntPtr lpPoint);

		[DllImport ("gdi32.dll", EntryPoint="LineTo", CallingConvention=CallingConvention.StdCall)]
		internal extern static bool Win32LineTo(IntPtr hdc, int x, int y);

		[DllImport ("gdi32.dll", EntryPoint="CreatePen", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32CreatePen(PenStyle fnPenStyle, int nWidth, ref COLORREF color);

		[DllImport ("gdi32.dll", EntryPoint="CreatePen", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32CreatePen(PenStyle fnPenStyle, int nWidth, IntPtr color);

		[DllImport ("gdi32.dll", EntryPoint="GetStockObject", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32GetStockObject(StockObject fnObject);

		[DllImport ("gdi32.dll", EntryPoint="CreateHatchBrush", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32CreateHatchBrush(HatchStyle fnStyle, IntPtr color);

		[DllImport ("gdi32.dll", EntryPoint="CreateHatchBrush", CallingConvention=CallingConvention.StdCall)]
		internal extern static IntPtr Win32CreateHatchBrush(HatchStyle fnStyle, ref COLORREF color);
		#endregion
	}
}
