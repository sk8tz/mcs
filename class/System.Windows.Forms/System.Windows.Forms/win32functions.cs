/*
 * Copyright (C) 5/11/2002 Carlos Harvey Perez 
 * 
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject
 * to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.
 * IN NO EVENT SHALL CARLOS HARVEY PEREZ BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
 * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR
 * THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 * Except as contained in this notice, the name of Carlos Harvey Perez
 * shall not be used in advertising or otherwise to promote the sale,
 * use or other dealings in this Software without prior written
 * authorization from Carlos Harvey Perez.
 */

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;

//using UtilityLibrary.WinControls;

//namespace UtilityLibrary.Win32
namespace System.Windows.Forms{
	/// <summary>
	/// Windows API Functions
	/// </summary>
	public class Win32
	{
		#region Constructors
		// No need to construct this object
		#endregion
		
		#region Constans values
		internal const string TOOLBARCLASSNAME = "ToolbarWindow32";
		internal const string REBARCLASSNAME = "ReBarWindow32";
		internal const string PROGRESSBARCLASSNAME = "msctls_progress32";
		internal const string SCROLLBAR = "SCROLLBAR";
		#endregion

		#region CallBacks
		internal delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
		internal delegate int CompareFunc(IntPtr param1, IntPtr param2, IntPtr sortParam);
		internal delegate int WinProc(IntPtr hWnd, int message, int wParam, int lParam);
		#endregion

		#region Kernel32.dll functions
		[DllImport("kernel32.dll", ExactSpelling=true, CharSet=CharSet.Auto)]
		internal static extern int GetCurrentThreadId();
		[DllImport("kernel32.dll")]
		internal static extern int GetDriveType(string rootPathName);
		[DllImport("kernel32.dll")]
		internal static extern int GetVolumeInformation(string drivePath,
			StringBuilder volumeNameBuffer,
			int driveNameBufferSize,
			out int serialNumber,
			out int maxFileNameLength,
			out int fileSystemFlags,
			StringBuilder systemNameBuffer,
			int systemNameBufferSize);
		[DllImport("kernel32.dll")]
		internal static extern void OutputDebugString(string message);
		#endregion
	
		#region Gdi32.dll functions
		[DllImport("gdi32.dll")]
		static internal extern bool StretchBlt(IntPtr hDCDest, int XOriginDest, int YOriginDest, int WidthDest, int HeightDest,
			IntPtr hDCSrc,  int XOriginScr, int YOriginSrc, int WidthScr, int HeightScr, PatBltTypes Rop);
		[DllImport("gdi32.dll")]
		static internal extern IntPtr CreateCompatibleDC(IntPtr hDC);
		[DllImport("gdi32.dll")]
		static internal extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int Width, int Heigth);
		[DllImport("gdi32.dll")]
		static internal extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
		[DllImport("gdi32.dll")]
		static internal extern bool BitBlt(IntPtr hDCDest, int XOriginDest, int YOriginDest, int WidthDest, int HeightDest,
			IntPtr hDCSrc,  int XOriginScr, int YOriginSrc, PatBltTypes flags);
		[DllImport("gdi32.dll")]
		static internal extern IntPtr DeleteDC(IntPtr hDC);
		[DllImport("gdi32.dll")]
		static internal extern bool PatBlt(IntPtr hDC, int XLeft, int YLeft, int Width, int Height, int Rop);
		[DllImport("gdi32.dll")]
		static internal extern bool DeleteObject(IntPtr hObject);
		[DllImport("gdi32.dll")]
		static internal extern int GetPixel(IntPtr hDC, int XPos, int YPos);
		[DllImport("gdi32.dll")]
		static internal extern int SetMapMode(IntPtr hDC, int fnMapMode);
		[DllImport("gdi32.dll")]
		static internal extern int GetObjectType(IntPtr handle);
		[DllImport("gdi32")]
		internal static extern IntPtr CreateDIBSection(IntPtr hdc, ref BITMAPINFO_FLAT bmi, 
			int iUsage, ref int ppvBits, IntPtr hSection, int dwOffset);
		[DllImport("gdi32")]
		internal static extern int GetDIBits(IntPtr hDC, IntPtr hbm, int StartScan, int ScanLines, int lpBits, BITMAPINFOHEADER bmi, int usage);
		[DllImport("gdi32")]
		internal static extern int GetDIBits(IntPtr hdc, IntPtr hbm, int StartScan, int ScanLines, int lpBits, ref BITMAPINFO_FLAT bmi, int usage);
		[DllImport("gdi32")]
		internal static extern IntPtr GetPaletteEntries(IntPtr hpal, int iStartIndex, int nEntries, byte[] lppe);
		[DllImport("gdi32")]
		internal static extern IntPtr GetSystemPaletteEntries(IntPtr hdc, int iStartIndex, int nEntries, byte[] lppe);
		[DllImport("gdi32")]
		internal static extern int SetDCBrushColor(IntPtr hdc,  int crColor);
		[DllImport("gdi32")]
		internal static extern IntPtr CreateSolidBrush(int crColor);
		[DllImport("gdi32")]
		internal static extern int SetBkMode(IntPtr hDC, BackgroundMode mode);
		[DllImport("gdi32")]
		internal static extern int SetViewportOrgEx(IntPtr hdc,  int x, int y,  int param);
		[DllImport("gdi32")]
        internal static extern int SetTextColor(IntPtr hDC, int colorRef);
		[DllImport("gdi32")]
		internal static extern int SetStretchBltMode(IntPtr hDC, StrechModeFlags StrechMode);
		[DllImport("gdi32")]
		internal static extern int SetPixel(IntPtr hDC, int x, int y, int color);
		[DllImport("gdi32")]
		internal static extern IntPtr CreatePen(PenStyle penStyle, int width, int color);
		[DllImport("gdi32")]
		internal static extern int GetClipRgn(IntPtr hDC, ref IntPtr region);
		[DllImport("gdi32")]
		internal static extern IntPtr CreateRectRgn(int nLeftRect,  int TopRect, int nRightRect, int nBottomRect);
		[DllImport("gdi32")]
		internal static extern int GetRgnBox(IntPtr hRegion, ref RECT rc);
		#endregion

		#region Uxtheme.dll functions
		[DllImport("uxtheme.dll")]
		static public extern int SetWindowTheme(IntPtr hWnd, StringBuilder AppID, StringBuilder ClassID);
		static public void DisableWindowsXPTheme(IntPtr hWnd)
		{
			// Disable using the Window XP Theme for the Window handle
			// passed as a parameter
			StringBuilder applicationName = new StringBuilder(" ", 1); 
			StringBuilder classIDs = new StringBuilder(" " , 1); 
			Win32.SetWindowTheme(hWnd, applicationName, classIDs);
		}
		#endregion
	
		#region User32.dll functions
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern IntPtr GetDesktopWindow();
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern bool ShowWindow(IntPtr hWnd, ShowWindowStyles State);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern bool SetForegroundWindow(IntPtr hWnd);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern bool OpenClipboard(IntPtr hWndNewOwner);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern bool CloseClipboard();
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern bool EmptyClipboard();
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern IntPtr SetClipboardData( int Format, IntPtr hData);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern bool GetMenuItemRect(IntPtr hWnd, IntPtr hMenu, int Item, ref RECT rc);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern int SendMessage(IntPtr hWnd, Msg msg, int wParam, int lParam);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, IntPtr lParam);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern void SendMessage(IntPtr hWnd, int msg, int wParam, ref RECT lParam);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern int SendMessage(IntPtr hWnd, int msg, int wParam, ref POINT lParam);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern void SendMessage(IntPtr hWnd, ToolBarMessages msg, int wParam, ref TBBUTTON lParam);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern void SendMessage(IntPtr hWnd, ToolBarMessages msg, int wParam, ref TBBUTTONINFO lParam);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern int SendMessage(IntPtr hWnd, RebarMessages msg, int wParam, ref REBARBANDINFO lParam);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern void SendMessage(IntPtr hWnd, TreeViewMessages msg, int wParam, ref TVITEM lParam);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern void SendMessage(IntPtr hWnd, TreeViewMessages msg, int wParam, ref TVINSERTSTRUCT lParam);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern void SendMessage(IntPtr hWnd, TreeViewMessages msg, int wParam, ref TVSORTCB lParam);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern int SendMessage(IntPtr hWnd, TreeViewMessages msg, int wParam, ref TVHITTESTINFO hti);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern void SendMessage(IntPtr hWnd, ListViewMessages msg, int wParam, ref LVITEM lParam);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern void SendMessage(IntPtr hWnd, HeaderControlMessages msg, int wParam, ref HDITEM lParam);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern void SendMessage(IntPtr hWnd, HeaderControlMessages msg, int wParam, ref HD_HITTESTINFO hti);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern int SendMessage(IntPtr hWnd, HeaderControlMessages msg, int wParam, int lParam);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr PostMessage(IntPtr hWnd, int msg, int wParam, int lParam);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr PostMessage(IntPtr hWnd, Msg msg, int wParam, int lParam);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr SetWindowsHookEx(WindowsHookCodes hookid, HookProc pfnhook, IntPtr hinst, int threadid);
		[DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
		internal static extern bool UnhookWindowsHookEx(IntPtr hhook);
		[DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
		internal static extern IntPtr CallNextHookEx(IntPtr hhook, int code, IntPtr wparam, IntPtr lparam);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal extern static int DrawText(IntPtr hdc, string lpString, int nCount, ref RECT lpRect, DrawTextFormatFlags flags);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal extern static IntPtr GetDlgItem(IntPtr hDlg, int nControlID);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
        internal extern static int InvalidateRect(IntPtr hWnd,  ref RECT rc, int bErase);
		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		internal extern static int InvalidateRect(IntPtr hWnd,  IntPtr rc, int bErase);
		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool WaitMessage();

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool PeekMessage(ref MESSAGE msg, int hWnd, int wFilterMin, int wFilterMax, PeekMessageFlags flags);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool GetMessage(ref MESSAGE msg, int hWnd, int wFilterMin, int wFilterMax);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool TranslateMessage(ref MESSAGE msg);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool DispatchMessage(ref MESSAGE msg);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr LoadCursor(IntPtr hInstance, CursorType cursor);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr SetCursor(IntPtr hCursor);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr BeginPaint(IntPtr hWnd, ref PAINTSTRUCT ps);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT ps);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref POINT pptDst, ref SIZE psize, 
			IntPtr hdcSrc, ref POINT pprSrc, Int32 crKey, ref BLENDFUNCTION pblend, UpdateLayeredWindowFlags dwFlags);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool ClientToScreen(IntPtr hWnd, ref POINT pt);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool TrackMouseEvent(ref TRACKMOUSEEVENTS tme);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool SetWindowRgn(IntPtr hWnd, IntPtr hRgn, bool redraw);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern short GetKeyState(int virtKey);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern bool MoveWindow(IntPtr hWnd, int x, int y, int width, int height, bool repaint);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern int GetClassName(IntPtr hWnd,  StringBuilder ClassName, int nMaxCount);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern int SetWindowLong(IntPtr hWnd, GetWindowLongFlag flag, int dwNewLong);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern int SetWindowLong(IntPtr hWnd, GetWindowLongFlag flag, WinProc winProc);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr GetDCEx(IntPtr hWnd, IntPtr hRegion, int flags);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr GetWindowDC(IntPtr hWnd);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern int FillRect(IntPtr hDC, ref RECT rect, IntPtr hBrush);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern int SetWindowText(IntPtr hWnd, string text);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		internal static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxCount);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", CharSet=CharSet.Auto)] 
		static internal extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern int ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern int GetSystemMetrics(SystemMetricsCodes code);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern int SetScrollInfo(IntPtr hwnd,  int bar, ref SCROLLINFO si, int fRedraw);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern int ShowScrollBar(IntPtr hWnd, int bar,  int show);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern int EnableScrollBar(IntPtr hWnd, int flags, int arrows);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern int BringWindowToTop(IntPtr hWnd);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern int GetScrollInfo(IntPtr hwnd, int bar, ref SCROLLINFO si);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern int ScrollWindowEx(IntPtr hWnd, int dx, int dy, 
			ref RECT rcScroll, ref RECT rcClip, IntPtr UpdateRegion, ref RECT rcInvalidated, int flags);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern bool IsWindow(IntPtr hWnd);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern int LockWindowUpdate(IntPtr hWnd);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern bool ValidateRect(IntPtr hWnd, ref RECT rcInvalidated);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern bool ValidateRect(IntPtr hWnd, IntPtr rc);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern int GetScrollBarInfo(IntPtr hWnd, SystemObject id, ref SCROLLBARINFO sbi);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern IntPtr GetWindowLong(IntPtr hWnd, GetWindowLongFlag flag);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern int SetProp(IntPtr hWnd, IntPtr atom, IntPtr hData);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern int CallWindowProc(IntPtr hOldProc, IntPtr hWnd, int message, int wParam, int lParam);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		static internal extern int EndMenu();

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
        static internal extern int DefWindowProc(IntPtr hWnd, int message, int wParam, int lParam);

		#endregion

		#region Shell32.dll functions

		[DllImport("Shell32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr SHGetFileInfo(string drivePath, int fileAttributes, 
			out SHFILEINFO fileInfo, int countBytesFileInfo, ShellFileInfoFlags flags);

		[DllImport("Shell32.dll", CharSet=CharSet.Auto)]
		internal static extern IntPtr SHGetFileInfo(IntPtr idl, int fileAttributes, 
			out SHFILEINFO fileInfo, int countBytesFileInfo, ShellFileInfoFlags flags);

		[DllImport("Shell32.dll", CharSet=CharSet.Auto)]
		internal static extern int SHGetSpecialFolderLocation(IntPtr hwndOwner, ShellSpecialFolder folder, out IntPtr idl);

		[DllImport("Shell32.dll", CharSet=CharSet.Auto)]
		internal static extern int SHGetMalloc(out IMalloc alloc);

		[DllImport("Shell32.dll", CharSet=CharSet.Auto)]
		internal static extern int SHGetDesktopFolder(out IShellFolder folder);
		
		[DllImport("Shell32.dll", CharSet=CharSet.Auto)]
		internal static extern int SHGetPathFromIDList(IntPtr idl, StringBuilder path);

		internal static void SHFreeMalloc(IntPtr handle)
		{
			IMalloc alloc = null;
			try 
			{
				Win32.SHGetMalloc(out alloc);
				Debug.Assert(alloc != null);
				alloc.Free(handle);
				// Free allocator itself
				IUnknown iUnknown = (IUnknown)alloc;
				iUnknown.Release();
			}
			catch (Exception e)
			{
				// In case the Garbage collector is trying to free
				// this memory from its own thread
				Debug.WriteLine(e.Message);
			}
		}
		
		#endregion

		#region Common Controls functions

		[DllImport("comctl32.dll")]
		internal static extern bool InitCommonControlsEx(INITCOMMONCONTROLSEX icc);

		[DllImport("comctl32.dll")]
		internal static extern bool InitCommonControls();

		[DllImport("comctl32.dll", EntryPoint="DllGetVersion")]
		internal extern static int GetCommonControlDLLVersion(ref DLLVERSIONINFO dvi);

		[DllImport("comctl32.dll")]
		internal static extern IntPtr ImageList_Create(int width, int height, int flags, int count, int grow);

		[DllImport("comctl32.dll")]
		internal static extern bool ImageList_Destroy(IntPtr handle);

		[DllImport("comctl32.dll")]
		internal static extern int ImageList_Add(IntPtr imageHandle, IntPtr hBitmap, IntPtr hMask);

		[DllImport("comctl32.dll")]
		internal static extern bool ImageList_Remove(IntPtr imageHandle, int index);

		[DllImport("comctl32.dll")]
		internal static extern bool ImageList_BeginDrag(IntPtr imageHandle, int imageIndex, int xHotSpot, int yHotSpot);

		[DllImport("comctl32.dll")]
		internal static extern bool ImageList_DragEnter(IntPtr hWndLock, int x, int y);

		[DllImport("comctl32.dll")]
		internal static extern bool ImageList_DragMove(int x, int y);

		[DllImport("comctl32.dll")]
		internal static extern bool ImageList_DragLeave(IntPtr hWndLock);

		[DllImport("comctl32.dll")]
		internal static extern void ImageList_EndDrag();

		[DllImport("comctl32.dll")]
		internal static extern bool ImageList_Draw(IntPtr hImageList, int imageIndex, 
			IntPtr hDCDest, int x, int y, ImageListDrawFlags flags);

		[DllImport("comctl32.dll")]
		internal static extern int ImageList_DrawEx(
			IntPtr hImageList, int imageIndex, IntPtr hDCDest, int x, int y, int dx, int dy, 
			uint backColor, uint foregColor, ImageListDrawFlags flags);

		[DllImport("comctl32.dll")]
		internal static extern int ImageList_DragShowNolock(int show);
		
		[DllImport("comctl32.dll")]
		internal static extern int ImageList_AddMasked(IntPtr hImageList, IntPtr hBitmap, int crMask);

		[DllImport("comctl32.dll")]
		internal static extern int ImageList_SetDragCursorImage(IntPtr himlDrag, int iDrag, int dxHotspot, int dyHotspot);
								
		internal static int ImageList_DrawEx(IntPtr hImageList, int imageIndex, IntPtr hDCDest, int x, int y, int dx, int dy,   
			ImageListDrawColor backColor, ImageListDrawColor foreColor, ImageListDrawFlags flags)
		{
            uint bColor = (uint)ImageListDrawColors.CLR_NONE;
			if ( backColor == ImageListDrawColor.Default )
				bColor =  (uint)ImageListDrawColors.CLR_DEFAULT;

			uint fColor = (uint)ImageListDrawColors.CLR_NONE;
			if ( foreColor == ImageListDrawColor.Default )
				fColor =  (uint)ImageListDrawColors.CLR_DEFAULT;
			 
			// Call actual function
			return ImageList_DrawEx(hImageList, imageIndex, hDCDest, x, y, dx, dy, bColor, fColor, flags);
		}

		
		static internal bool IsCommonCtrl6()
		{
			DLLVERSIONINFO dllVersion = new DLLVERSIONINFO();
			// We are assummng here that anything greater or equal than 6
			// will have the new XP theme drawing enable
			dllVersion.cbSize = Marshal.SizeOf(typeof(DLLVERSIONINFO));
			Win32.GetCommonControlDLLVersion(ref dllVersion);
			return (dllVersion.dwMajorVersion >= 6);
		}

		#endregion

		#region Win32 Macro-Like helpers
		internal static int X_LPARAM(int lParam)
		{
			return (lParam & 0xffff);
		}
	 
		internal static int Y_LPARAM(int lParam)
		{
			return (lParam >> 16);
		}

		internal static Point GetPointFromLPARAM(int lParam)
		{
			return new Point(X_LPARAM(lParam), Y_LPARAM(lParam));
		}

		internal static int LOW_ORDER(int param)
		{
			return (param & 0xffff);
		}

		internal static int HIGH_ORDER(int param)
		{
			return (param >> 16);
		}

		internal static int INDEXTOOVERLAYMASK(int index)
		{
			return (int)((uint)index << 8); 
		}

		internal static int OVERLAYMASKTOINDEX(int index)
		{
			return (int)((uint)index >> 8); 
		}

		internal static int INDEXTOSTATEIMAGEMASK(int i)
		{
			return (int)((uint)i << 12);
		}

		internal static int STATEIMAGEMASKTOINDEX(int i)
		{
			 return (int)((uint)i >> 12);
		}

		internal static short HRESULT_CODE(int hr)
		{
             return (short)(hr & 0xFFFF);
		}

		internal static bool SUCCEEDED(int status)
		{
			return (status >= 0);
		}

		internal static bool FAILED(int status)
		{
			return (status < 0);
		}

		internal static int  MAKEINTRESOURCE(int res)
		{
			return 0x0000FFFF & res;
		}
		#endregion


		#region Mono win32 Fuinctions

		internal delegate IntPtr WndProc (IntPtr hwnd, Msg msg, IntPtr wParam, IntPtr lParam);
		
		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall,
			 CharSet = CharSet.Ansi, EntryPoint = "RegisterClassA")]
		internal static extern uint RegisterClass(ref WNDCLASS wndClass);
		
		#region Added by Dennis hayes 10-20-2002
		//correct?
		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall,
			 CharSet = CharSet.Auto)]
		internal static extern uint SendMessage(
			IntPtr hWnd, uint Msg,
			IntPtr wParam, IntPtr lParam);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall,
			 CharSet = CharSet.Auto)]
		internal static extern bool GetWindowPlacement(
			IntPtr hWnd,
			ref  WINDOWPLACEMENT  lpwndpl  // position data
			);
		#endregion

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall,
			 CharSet = CharSet.Ansi, EntryPoint = "CreateWindowExA")]
		internal static extern IntPtr CreateWindowEx (
			uint dwExStyle, string lpClassName, 
			string lpWindowName, uint dwStyle, 
			int x, int y, int nWidth, int nHeight,
			IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance,
			ref object lpParam);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern IntPtr CreateMenu ();

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Ansi)]
		internal static extern bool AppendMenuA(IntPtr hMenu, uint uflags, IntPtr NewItem, string item);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall)]
		internal static extern bool DestroyMenu (IntPtr hMenu);
		
		[DllImport ("user32.dll", CallingConvention = 
			 CallingConvention.StdCall, CharSet = CharSet.Auto)]
		internal extern static IntPtr DefWindowProcA (
			IntPtr hWnd, Msg Msg, IntPtr wParam, IntPtr lParam);

		[DllImport ("user32.dll", CallingConvention = 
			 CallingConvention.StdCall, CharSet = CharSet.Auto)]
		internal extern static int DestroyWindow (IntPtr hWnd);

		[DllImport ("user32.dll", CallingConvention = 
			 CallingConvention.StdCall, CharSet = CharSet.Auto)]
		internal static extern int ShowWindow (IntPtr hWnd, 
			uint nCmdShow);

		[DllImport ("user32.dll", CallingConvention = 
			 CallingConvention.StdCall,CharSet = CharSet.Auto)]
		internal static extern int GetMessageA (ref MSG msg, int hwnd, 
			int msgFrom,  int msgTo);

		[DllImport ("user32.dll", CallingConvention = 
			 CallingConvention.StdCall, CharSet = CharSet.Auto)]
		internal static extern int  TranslateMessage (ref MSG msg);

		[DllImport ("user32.dll", CallingConvention = 
			 CallingConvention.StdCall, CharSet = CharSet.Auto)]
		internal static extern int DispatchMessageA (ref MSG msg);

		[DllImport ("user32.dll", CallingConvention = 
			 CallingConvention.StdCall, CharSet = CharSet.Auto)]
		internal static extern int PeekMessageA (
			ref MSG msg, IntPtr hWnd, uint wMsgFilterMin, 
			uint wMsgFilterMax, uint wRemoveMsg);

		[DllImport ("user32.dll", CallingConvention = 
			 CallingConvention.StdCall,
			 CharSet = CharSet.Auto)]
		internal extern static void PostQuitMessage (int nExitCode);

		[DllImport ("user32.dll", CallingConvention = 
			 CallingConvention.StdCall,
			 CharSet = CharSet.Auto)]
		internal extern static IntPtr SetActiveWindow (IntPtr hWnd);

		[DllImport ("user32.dll", CallingConvention = 
			 CallingConvention.StdCall,
			 CharSet = CharSet.Auto)]
		internal extern static int CloseWindow (IntPtr hWnd);

		[DllImport ("user32.dll", CallingConvention = 
			 CallingConvention.StdCall,
			 CharSet = CharSet.Auto)]
		internal extern static int SetWindowPos (
			IntPtr hWnd, SetWindowPosZOrder pos,
			int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern int MessageBoxA (
			IntPtr hWnd, string pText, string pCaption, uint uType);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern IntPtr SetParent (
			IntPtr hWndChild, IntPtr hWndNewParent);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern IntPtr GetParent (IntPtr hWnd);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern bool SetWindowTextA (
			IntPtr hWnd, string lpString);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern bool UpdateWindow (IntPtr hWnd);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern uint GetBkColor (IntPtr hdc);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern uint SetBkColor (IntPtr hdc, uint crColor);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern IntPtr GetDC (IntPtr hWnd);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern int ReleaseDC (IntPtr hWnd, IntPtr hDC);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern IntPtr GetFocus();

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern IntPtr SetFocus (IntPtr hWnd);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern bool IsWindowEnabled (IntPtr hWnd);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern bool IsMenu (IntPtr hWnd);

		
		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern bool EnableWindow (
			IntPtr hWnd, bool bEnable);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern bool GetWindowRect (
			IntPtr hWnd, ref RECT lpRect);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern bool GetClientRect (
			IntPtr hWnd, ref RECT lpRect);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern bool InvalidateRect (
			IntPtr hWnd, ref RECT lpRect, bool bErase); 

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern IntPtr GetCapture ();

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern IntPtr SetCapture (IntPtr hWnd);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern bool ReleaseCapture ();

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern int GetWindowTextA (
			IntPtr hWnd, ref String lpString, int nMaxCount);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern int GetWindowTextLengthA (IntPtr hWnd);

		[DllImport ("user32.dll", 
			 CallingConvention = CallingConvention.StdCall, 
			 CharSet = CharSet.Auto)]
		internal static extern bool GetCursorPos (ref POINT lpPoint);

		#endregion


	}

}
