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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

// NOT COMPLETE

using System;
using System.Collections;
using System.Drawing;
using System.Runtime.InteropServices;

// NOTE: Possible optimization:
// Several properties calculate dimensions on the fly; instead; they can 
// be stored in a field and only be recalculated when a style is changed (DefaultClientRect, for example)

namespace System.Windows.Forms {
	internal class Hwnd : IDisposable {
		#region Local Variables
		private static Hashtable	windows	= new Hashtable(100, 0.5f);
		//private const int	menu_height = 14;			// FIXME - Read this value from somewhere
		
		private IntPtr		handle;
		internal IntPtr		client_window;
		internal IntPtr		whole_window;
		internal IntPtr		menu_handle;
		internal TitleStyle	title_style;
		internal FormBorderStyle	border_style;
		internal int		x;
		internal int		y;
		internal int		width;
		internal int		height;
		internal bool		allow_drop;
		internal Hwnd		parent;
		internal bool		visible;
		internal bool		zero_sized;
		internal Rectangle	invalid;
		internal bool		expose_pending;
		internal bool		nc_expose_pending;
		internal bool		configure_pending;
		internal Graphics	client_dc;
		internal object		user_data;
		internal Rectangle	client_rectangle;
		internal ArrayList	marshal_free_list;
		internal int		caption_height;
		internal int		tool_caption_height;
		#endregion	// Local Variables

		#region Constructors and destructors
		public Hwnd() {
			x = 0;
			y = 0;
			width = 0;
			height = 0;
			visible = false;
			menu_handle = IntPtr.Zero;
			border_style = FormBorderStyle.None;
			client_window = IntPtr.Zero;
			whole_window = IntPtr.Zero;
			handle = IntPtr.Zero;
			parent = null;
			invalid = Rectangle.Empty;
			expose_pending = false;
			nc_expose_pending = false;
			client_rectangle = Rectangle.Empty;
			marshal_free_list = new ArrayList(2);
		}

		public void Dispose() {
			windows[client_window] = null;
			windows[whole_window] = null;
			for (int i = 0; i < marshal_free_list.Count; i++) {
				Marshal.FreeHGlobal((IntPtr)marshal_free_list[i]);
			}
			marshal_free_list.Clear();
		}
		#endregion

		#region	Static Methods
		public void SetObjectWindow(Hwnd obj, IntPtr window) {
			windows[window] = obj;
		}

		public static Hwnd ObjectFromWindow(IntPtr window) {
			return (Hwnd)windows[window];
		}

		public static Hwnd ObjectFromHandle(IntPtr handle) {
			//return (Hwnd)(((GCHandle)handle).Target);
			return (Hwnd)windows[handle];
		}

		public static IntPtr HandleFromObject(Hwnd obj) {
			return obj.handle;
		}

		public static Hwnd GetObjectFromWindow(IntPtr window) {
			return (Hwnd)windows[window];
		}

		public static IntPtr GetHandleFromWindow(IntPtr window) {
			Hwnd	hwnd;

			hwnd = (Hwnd)windows[window];
			if (hwnd != null) {
				return hwnd.handle;
			} else {
				return IntPtr.Zero;
			}
		}

		public static Rectangle GetWindowRectangle(FormBorderStyle border_style,
				IntPtr menu_handle, TitleStyle title_style, int caption_height,
				int tool_caption_height, Rectangle client_rect)
		{
			Rectangle	rect;

			rect = new Rectangle(client_rect.Location, client_rect.Size);

			if (menu_handle != IntPtr.Zero) {
				MenuAPI.MENU menu = MenuAPI.GetMenuFromID (menu_handle);
				if (menu != null) {
					int menu_height = menu.Height;
					rect.Y -= menu_height;
					rect.Height += menu_height;
				} else
					Console.WriteLine("Hwnd.GetWindowRectangle: No MENU for menu_handle = {0}", menu_handle);
			}

			if (border_style == FormBorderStyle.Fixed3D) {
				rect.X -= 2;
				rect.Y -= 2;
				rect.Width += 4;
				rect.Height += 4;
			} else if (border_style == FormBorderStyle.FixedSingle) {
				rect.X -= 1;
				rect.Y -= 1;
				rect.Width += 2;
				rect.Height += 2;
			} else if ((int) border_style == 0xFFFF) {
				rect.X -= 3;
				rect.Y -= 3;
				rect.Width += 6;
				rect.Height += 6;
			}

			if (title_style == TitleStyle.Normal) {
				rect.Y -= caption_height;
				rect.Height += caption_height;
			} else if (title_style == TitleStyle.Tool) {
				rect.Y -= tool_caption_height;
				rect.Height += tool_caption_height;
			}

			return rect;
		}

		public static Rectangle GetClientRectangle(FormBorderStyle border_style, IntPtr menu_handle, TitleStyle title_style, int caption_height, int tool_caption_height, int width, int height) {
			Rectangle rect;

			rect = new Rectangle(0, 0, width, height);

			if (menu_handle != IntPtr.Zero) {
				MenuAPI.MENU menu = MenuAPI.GetMenuFromID (menu_handle);
				if (menu != null) {
					int menu_height = menu.Height;
					rect.Y += menu_height;
					rect.Height -= menu_height;
				} else
					Console.WriteLine("Hwnd.GetClientRectangle: No MENU for menu_handle = {0}", menu_handle);
			}

			if (border_style == FormBorderStyle.Fixed3D) {
				rect.X += 2;
				rect.Y += 2;
				rect.Width -= 4;
				rect.Height -= 4;
			} else if (border_style == FormBorderStyle.FixedSingle) {
				rect.X += 1;
				rect.Y += 1;
				rect.Width -= 2;
				rect.Height -= 2;
			} else if ((int) border_style == 0xFFFF) {
				rect.X += 3;
				rect.Y += 3;
				rect.Width -= 6;
				rect.Height -= 6;
			}

			if (title_style == TitleStyle.Normal)  {
				rect.Y += caption_height;
				rect.Height -= caption_height;
			} else if (title_style == TitleStyle.Tool)  {
				rect.Y += tool_caption_height;
				rect.Height -= tool_caption_height;
			}

			return rect;
		}
		#endregion	// Static Methods

		#region Instance Properties
		public FormBorderStyle BorderStyle {
			get {
				return border_style;
			}

			set {
				border_style = value;
			}
		}

		public Graphics ClientDC {
			get {
				return client_dc;
			}

			set {
				client_dc = value;
			}
		}

		public Rectangle ClientRect {
			get {
				if (client_rectangle == Rectangle.Empty) {
					return DefaultClientRect;
				}
				return client_rectangle;
			}

			set {
				client_rectangle = value;
			}
		}

		public IntPtr ClientWindow {
			get {
				return client_window;
			}

			set {
				client_window = value;
				handle = value;

				if (windows[client_window] == null) {
					windows[client_window] = this;
				}
			}
		}

		public Rectangle DefaultClientRect {
			get {
				return GetClientRectangle (border_style, menu_handle, title_style,
						caption_height, tool_caption_height, width, height);
			}
		}

		public bool ExposePending {
			get {
				return expose_pending;
			}

			set {
				expose_pending = value;
			}
		}

		public IntPtr Handle {
			get {
				if (handle == IntPtr.Zero) {
					throw new ArgumentNullException("Handle", "Handle is not yet assigned, need a ClientWindow");
				}
				return handle;
			}
		}

		public int Height {
			get {
				return height;
			}

			set {
				height = value;
			}
		}

		public IntPtr MenuHandle {
			get {
				return menu_handle;
			}

			set {
				menu_handle = value;
			}
		}

		public Point MenuOrigin {
			get {
				Point	pt;

				pt = new Point(0, 0);

				if (border_style == FormBorderStyle.Fixed3D) {
					pt.X += 2;
					pt.Y += 2;
				} else if (border_style == FormBorderStyle.FixedSingle) {
					pt.X += 1;
					pt.Y += 1;
				}

				if (this.title_style == TitleStyle.Normal)  {
					pt.Y += caption_height;
				} else if (this.title_style == TitleStyle.Normal)  {
					pt.Y += tool_caption_height;
				}

				return pt;
			}
		}
		public Rectangle Invalid {
			get {
				return invalid;
			}

			set {
				invalid = value;
			}
		}

		public bool NCExposePending {
			get {
				return nc_expose_pending;
			}

			set {
				nc_expose_pending = value;
			}
		}

		public Hwnd Parent {
			get {
				return parent;
			}

			set {
				parent = value;
			}
		}

		public int CaptionHeight {
			get { return caption_height; }
			set { caption_height = value; }
		}

		public int ToolCaptionHeight {
			get { return tool_caption_height; }
			set { tool_caption_height = value; }
		}

		public TitleStyle TitleStyle {
			get {
				return title_style;
			}

			set {
				title_style = value;
			}
		}

		public object UserData {
			get {
				return user_data;
			}

			set {
				user_data = value;
			}
		}

		public IntPtr WholeWindow {
			get {
				return whole_window;
			}

			set {
				whole_window = value;

				if (windows[whole_window] == null) {
					windows[whole_window] = this;
				}
			}
		}

		public int Width {
			get {
				return width;
			}

			set {
				width = value;
			}
		}

		public bool Visible {
			get {
				return visible;
			}

			set {
				visible = value;
			}
		}

		public int X {
			get {
				return x;
			}

			set {
				x = value;
			}
		}

		public int Y {
			get {
				return y;
			}

			set {
				y = value;
			}
		}
		#endregion	// Instance properties

		#region Methods
		public void AddInvalidArea(int x, int y, int width, int height) {
			if (invalid == Rectangle.Empty) {
				invalid = new Rectangle (x, y, width, height);
				return;
			}

			int right, bottom;
			right = Math.Max (invalid.Right, x + width);
			bottom = Math.Max (invalid.Bottom, y + height);
			invalid.X = Math.Min (invalid.X, x);
			invalid.Y = Math.Min (invalid.Y, y);

			invalid.Width = right - invalid.X;
			invalid.Height = bottom - invalid.Y;
		}

		public void AddInvalidArea(Rectangle rect) {
			if (invalid == Rectangle.Empty) {
				invalid = rect;
				return;
			}
			invalid = Rectangle.Union (invalid, rect);
		}

		public void ClearInvalidArea() {
			invalid = Rectangle.Empty;
			expose_pending = false;
		}
		#endregion	// Methods
	}
}
