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


// NOT COMPLETE

using System;
using System.Drawing;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;


/// X11 Version
namespace System.Windows.Forms {
	internal class XplatUI {
		#region Local Variables
		static XplatUIDriver		driver;
		static String			default_class_name;
		#endregion	// Local Variables

		#region Subclasses

		public class State {
			static public Keys ModifierKeys {
				get {
					return driver.ModifierKeys;
				}
			}

			static public MouseButtons MouseButtons {
				get {
					return driver.MouseButtons;
				}
			}

			static public Point MousePosition {
				get {
					return driver.MousePosition;
				}
			}

			static public bool DropTarget {
				get {
					return driver.DropTarget;
				}

				set {
					driver.DropTarget=value;
				}
			}

		}
		#endregion	// Subclasses

		#region Constructor & Destructor
		static XplatUI() {
			Console.WriteLine("Mono System.Windows.Forms Assembly [Revision: 40575; built: 2005/1/13 1:47:46]");

			// Don't forget to throw the mac in here somewhere, too
			default_class_name="SWFClass";

			if (Environment.OSVersion.Platform == (PlatformID)128) {
				if (Environment.GetEnvironmentVariable ("MONO_MWF_USE_QUARTZ_BACKEND") != null)
					driver=XplatUIOSX.GetInstance();
				else
					driver=XplatUIX11.GetInstance();
			} else {
				driver=XplatUIWin32.GetInstance();
			}

		}

		~XplatUI() {
			Console.WriteLine("XplatUI Destructor called");
		}
		#endregion	// Constructor & Destructor

		#region Public Static Properties
		internal static string DefaultClassName {
			get {
				return default_class_name;
			}

			set {
				default_class_name=value;
			}
		}

		static public Size CursorSize {
			get {
				return driver.CursorSize;
			}
		}

		static public bool DragFullWindows {
			get {
				return driver.DragFullWindows;
			}
		}

		static public Size DragSize {
			get {
				return driver.DragSize;
			}
		}

		static public Size IconSize {
			get {
				return driver.IconSize;
			}
		}

		static public Size MaxWindowTrackSize {
			get {
				return driver.MaxWindowTrackSize;
			}
		}

		static public Size MinimizedWindowSize {
			get {
				return driver.MinimizedWindowSize;
			}
		}

		static public Size MinimizedWindowSpacingSize {
			get {
				return driver.MinimizedWindowSpacingSize;
			}
		}

		static public Size MinimumWindowSize {
			get {
				return driver.MinimumWindowSize;
			}
		}

		static public Size MinWindowTrackSize {
			get {
				return driver.MinWindowTrackSize;
			}
		}

		static public Size SmallIconSize {
			get {
				return driver.SmallIconSize;
			}
		}

		static public int MouseButtonCount {
			get {
				return driver.MouseButtonCount;
			}
		}

		static public bool MouseButtonsSwapped {
			get {
				return driver.MouseButtonsSwapped;
			}
		}

		static public bool MouseWheelPresent {
			get {
				return driver.MouseWheelPresent;
			}
		}

		static public Rectangle VirtualScreen {
			get {
				return driver.VirtualScreen;
			}
		}

		static public Rectangle WorkingArea {
			get {
				return driver.WorkingArea;
			}
		}
		#endregion	// Public Static Properties

                internal static event EventHandler Idle {
                        add {
                                driver.Idle += value;
                        }
                        remove {
                                driver.Idle -= value;
                        }
                }
                
		#region Public Static Methods
		internal static void Exit() {
			driver.Exit();
		}

		internal static void GetDisplaySize(out Size size) {
			driver.GetDisplaySize(out size);
		}

		internal static void EnableThemes() {
			driver.EnableThemes();
		}

		internal static bool Text(IntPtr hWnd, string text) {
			return driver.Text(hWnd, text);
		}

		internal static bool GetText(IntPtr hWnd, out string text) {
			return driver.GetText(hWnd, out text);
		}

		internal static bool SetVisible(IntPtr hWnd, bool visible) {
			return driver.SetVisible(hWnd, visible);
		}

		internal static bool IsVisible(IntPtr hWnd) {
			return driver.IsVisible(hWnd);
		}

		internal static IntPtr SetParent(IntPtr hWnd, IntPtr hParent) {
			return driver.SetParent(hWnd, hParent);
		}

		internal static IntPtr GetParent(IntPtr hWnd) {
			return driver.GetParent(hWnd);
		}

		internal static void Version() {
			Console.WriteLine("Xplat version $revision: $");
		}

		internal static IntPtr CreateWindow(CreateParams cp) {
			return driver.CreateWindow(cp);
		}

		internal static IntPtr CreateWindow(IntPtr Parent, int X, int Y, int Width, int Height) {
			return driver.CreateWindow(Parent, X, Y, Width, Height);
		}

		internal static void DestroyWindow(IntPtr handle) {
			driver.DestroyWindow(handle);
		}

		internal static FormWindowState GetWindowState(IntPtr handle) {
			return driver.GetWindowState(handle);
		}

		internal static void SetWindowState(IntPtr handle, FormWindowState state) {
			driver.SetWindowState(handle, state);
		}

		internal static void SetWindowStyle(IntPtr handle, CreateParams cp) {
			driver.SetWindowStyle(handle, cp);
		}

		internal static void UpdateWindow(IntPtr handle) {
			driver.UpdateWindow(handle);
		}

		internal static void SetWindowBackground(IntPtr handle, Color color) {
			driver.SetWindowBackground(handle, color);
		}
			
		internal static PaintEventArgs PaintEventStart(IntPtr handle) {
			return driver.PaintEventStart(handle);
		}

		internal static void PaintEventEnd(IntPtr handle) {
			driver.PaintEventEnd(handle);
		}

		internal static void SetWindowPos(IntPtr handle, int x, int y, int width, int height) {
			driver.SetWindowPos(handle, x, y, width, height);
		}

		internal static void GetWindowPos(IntPtr handle, bool is_toplevel, out int x, out int y, out int width, out int height, out int client_width, out int client_height) {
			driver.GetWindowPos(handle, is_toplevel, out x, out y, out width, out height, out client_width, out client_height);
		}

		internal static void Invalidate(IntPtr handle, Rectangle rc, bool clear) {
			driver.Invalidate(handle, rc, clear);
		}

		internal static void Activate(IntPtr handle) {
			driver.Activate(handle);
		}

		internal static void EnableWindow(IntPtr handle, bool Enable) {
			driver.EnableWindow(handle, Enable);
		}

		internal static void SetModal(IntPtr handle, bool Modal) {
			driver.SetModal(handle, Modal);
		}

		internal static IntPtr DefWndProc(ref Message msg) {
			return driver.DefWndProc(ref msg);
		}

		internal static void HandleException(Exception e) {
			driver.HandleException(e);
		}

		internal static void DoEvents() {
			driver.DoEvents();
		}

		internal static bool PeekMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax, uint flags) {
			return driver.PeekMessage(ref msg, hWnd, wFilterMin, wFilterMax, flags);
		}

		internal static bool GetMessage(ref MSG msg, IntPtr hWnd, int wFilterMin, int wFilterMax) {
			return driver.GetMessage(ref msg, hWnd, wFilterMin, wFilterMax);
		}

		internal static bool TranslateMessage(ref MSG msg) {
			return driver.TranslateMessage(ref msg);
		}

		internal static IntPtr DispatchMessage(ref MSG msg) {
			return driver.DispatchMessage(ref msg);
		}

		internal static void GrabWindow(IntPtr hWnd, IntPtr ConfineToHwnd) {
			driver.GrabWindow(hWnd, ConfineToHwnd);
		}

		internal static void GrabInfo(out IntPtr hWnd, out bool GrabConfined, out Rectangle GrabArea) {
			driver.GrabInfo(out hWnd, out GrabConfined, out GrabArea);
		}

		internal static void UngrabWindow(IntPtr hWnd) {
			driver.UngrabWindow(hWnd);
		}

		internal static bool SetZOrder(IntPtr hWnd, IntPtr AfterhWnd, bool Top, bool Bottom) {
			return driver.SetZOrder(hWnd, AfterhWnd, Top, Bottom);
		}

		internal static bool SetTopmost(IntPtr hWnd, IntPtr hWndOwner, bool Enabled) {
			return driver.SetTopmost(hWnd, hWndOwner, Enabled);
		}

		internal static bool CalculateWindowRect(IntPtr hWnd, ref Rectangle ClientRect, int Style, int ExStyle, IntPtr MenuHandle, out Rectangle WindowRect) {
			return driver.CalculateWindowRect(hWnd, ref ClientRect, Style, ExStyle, MenuHandle, out WindowRect);
		}

		internal static void SetCursor(IntPtr hwnd, IntPtr cursor) {
			driver.SetCursor(hwnd, cursor);
		}

		internal static void ShowCursor(bool show) {
			driver.ShowCursor(show);
		}

		internal static void OverrideCursor(IntPtr cursor) {
			driver.OverrideCursor(cursor);
		}

		internal static IntPtr DefineCursor(Bitmap bitmap, Bitmap mask, Color cursor_pixel, Color mask_pixel, int xHotSpot, int yHotSpot) {
			return driver.DefineCursor(bitmap, mask, cursor_pixel, mask_pixel, xHotSpot, yHotSpot);
		}

		internal static IntPtr DefineStdCursor(StdCursor id) {
			return driver.DefineStdCursor(id);
		}

		internal static void DestroyCursor(IntPtr cursor) {
			driver.DestroyCursor(cursor);
		}

		internal static void GetCursorInfo(IntPtr cursor, out int width, out int height, out int hotspot_x, out int hotspot_y) {
			driver.GetCursorInfo(cursor, out width, out height, out hotspot_x, out hotspot_y);
		}

		internal static void SetCursorPos(IntPtr handle, int x, int y) {
			driver.SetCursorPos(handle, x, y);
		}

		internal static void GetCursorPos(IntPtr handle, out int x, out int y) {
			driver.GetCursorPos(handle, out x, out y);
		}

		internal static void ScreenToClient(IntPtr handle, ref int x, ref int y) {
			driver.ScreenToClient (handle, ref x, ref y);
		}

		internal static void ClientToScreen(IntPtr handle, ref int x, ref int y) {
			driver.ClientToScreen(handle, ref x, ref y);
		}

		internal static void SendAsyncMethod (AsyncMethodData data) {
			driver.SendAsyncMethod (data);
		}

		internal static void CreateCaret(IntPtr hwnd, int width, int height) {
			driver.CreateCaret(hwnd, width, height);
		}

		internal static void DestroyCaret(IntPtr hwnd) {
			driver.DestroyCaret(hwnd);
		}

		internal static void SetCaretPos(IntPtr hwnd, int x, int y) {
			driver.SetCaretPos(hwnd, x, y);
		}

		internal static void CaretVisible(IntPtr hwnd, bool visible) {
			driver.CaretVisible(hwnd, visible);
		}

		internal static void SetFocus(IntPtr hwnd) {
			driver.SetFocus(hwnd);
		}

		internal static IntPtr GetActive() {
			return driver.GetActive();
		}

		internal static bool GetFontMetrics(Graphics g, Font font, out int ascent, out int descent) {
			return driver.GetFontMetrics(g, font, out ascent, out descent);
		}
			
		internal static void SetTimer (Timer timer)
		{
			driver.SetTimer (timer);
		}

		internal static void KillTimer (Timer timer)
		{
			driver.KillTimer (timer);
		}

		internal static int KeyboardSpeed {
			get {
				return driver.KeyboardSpeed;
			}
		}

		internal static int KeyboardDelay {
			get {
				return driver.KeyboardSpeed;
			}
		}

		internal static void ScrollWindow(IntPtr hwnd, Rectangle rectangle, int XAmount, int YAmount, bool with_children) {
			driver.ScrollWindow(hwnd, rectangle, XAmount, YAmount, with_children);
		}

		internal static void ScrollWindow(IntPtr hwnd, int XAmount, int YAmount, bool with_children) {
			driver.ScrollWindow(hwnd, XAmount, YAmount, with_children);
		}

		internal static bool SystrayAdd(IntPtr hwnd, string tip, Icon icon, out ToolTip tt) {
			return driver.SystrayAdd(hwnd, tip, icon, out tt);
		}

		internal static void SystrayChange(IntPtr hwnd, string tip, Icon icon, ref ToolTip tt) {
			driver.SystrayChange(hwnd, tip, icon, ref tt);
		}

		internal static void SystrayRemove(IntPtr hwnd, ref ToolTip tt) {
			driver.SystrayRemove(hwnd, ref tt);
		}

		internal static void SetBorderStyle(IntPtr handle, BorderStyle border_style) {
			driver.SetBorderStyle(handle, border_style);
		}

		internal static void SetMenu(IntPtr handle, IntPtr menu_handle) {
			driver.SetMenu(handle, menu_handle);
		}


		internal static Graphics GetMenuDC(IntPtr hwnd, IntPtr ncpaint_region) {
			return driver.GetMenuDC(hwnd, ncpaint_region);
		}

		internal static void ReleaseMenuDC(IntPtr hwnd, Graphics dc) {
			driver.ReleaseMenuDC(hwnd, dc);
		}

		// Santa's little helper
		internal static void Where() {
			Console.WriteLine("Here: {0}", new StackTrace().ToString());
		}
		#endregion	// Public Static Methods

	}
}
