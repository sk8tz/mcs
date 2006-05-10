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
// Copyright (c) 2005-2006 Novell, Inc. (http://www.novell.com)
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
		internal Menu		menu;
		internal TitleStyle	title_style;
		internal FormBorderStyle	border_style;
		internal int		x;
		internal int		y;
		internal int		width;
		internal int		height;
		internal bool		allow_drop;
		internal Hwnd		parent;
		internal bool		visible;
		internal bool		mapped;
		internal uint		opacity;
		internal bool		enabled;
		internal bool		zero_sized;
		internal ArrayList	invalid_list;
		internal Rectangle	nc_invalid;
		internal bool		expose_pending;
		internal bool		nc_expose_pending;
		internal bool		configure_pending;
		internal bool		reparented;
		internal Graphics	client_dc;
		internal Graphics	non_client_dc;
		internal object		user_data;
		internal Rectangle	client_rectangle;
		internal ArrayList	marshal_free_list;
		internal int		caption_height;
		internal int		tool_caption_height;
		internal bool		whacky_wm;
		internal bool		fixed_size;
		internal static Bitmap	bmp = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
		internal XEventQueue	queue;
		#endregion	// Local Variables

		#region Constructors and destructors
		public Hwnd() {
			x = 0;
			y = 0;
			width = 0;
			height = 0;
			visible = false;
			menu = null;
			border_style = FormBorderStyle.None;
			client_window = IntPtr.Zero;
			whole_window = IntPtr.Zero;
			handle = IntPtr.Zero;
			parent = null;
			invalid_list = new ArrayList();
			expose_pending = false;
			nc_expose_pending = false;
			enabled = true;
			reparented = false;
			client_rectangle = Rectangle.Empty;
			marshal_free_list = new ArrayList(2);
			opacity = 0xffffffff;
			fixed_size = false;
		}

		public void Dispose() {
			windows.Remove(client_window);
			windows.Remove(whole_window);
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
				Menu menu, TitleStyle title_style, int caption_height,
				int tool_caption_height, Rectangle client_rect)
		{
			Rectangle	rect;

			rect = new Rectangle(client_rect.Location, client_rect.Size);

			if (menu != null) {
				int menu_height = menu.Rect.Height;
				if (menu_height == 0) {
					Graphics g;

					g = Graphics.FromImage(bmp);
					menu_height = ThemeEngine.Current.CalcMenuBarSize(g, menu, client_rect.Width);
					g.Dispose();
				}

				rect.Y -= menu_height;
				rect.Height += menu_height;
			}

			if (border_style == FormBorderStyle.Fixed3D) {
				Size border_3D_size = ThemeEngine.Current.Border3DSize;

				rect.X -= border_3D_size.Width;
				rect.Y -= border_3D_size.Height;
				rect.Width += border_3D_size.Width * 2;
				rect.Height += border_3D_size.Height * 2;
			} else if (border_style == FormBorderStyle.FixedSingle) {
				rect.X -= 1;
				rect.Y -= 1;
				rect.Width += 2;
				rect.Height += 2;
			}

			return rect;
		}

		public static Rectangle GetClientRectangle(FormBorderStyle border_style, Menu menu, TitleStyle title_style, int caption_height, int tool_caption_height, int width, int height) {
			Rectangle rect;

			rect = new Rectangle(0, 0, width, height);

			if (menu != null) {
				int menu_height = menu.Rect.Height;
				rect.Y += menu_height;
				rect.Height -= menu_height;
			}

			if (border_style == FormBorderStyle.Fixed3D) {
				Size border_3D_size = ThemeEngine.Current.Border3DSize;

				rect.X += border_3D_size.Width;
				rect.Y += border_3D_size.Height;
				rect.Width -= border_3D_size.Width * 2;
				rect.Height -= border_3D_size.Height * 2;
			} else if (border_style == FormBorderStyle.FixedSingle) {
				rect.X += 1;
				rect.Y += 1;
				rect.Width -= 2;
				rect.Height -= 2;
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

		public Graphics NonClientDC {
			get { return non_client_dc; }
			set { non_client_dc = value; }
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
				// We pass a Zero for the menu handle so the menu size is
				// not computed this is done via an WM_NCCALC
				return GetClientRectangle (border_style, null, title_style,
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

		public Menu Menu {
			get {
				return menu;
			}

			set {
				menu = value;
			}
		}

		public bool Reparented {
			get {
				return reparented;
			}

			set {
				reparented = value;
			}
		}

		public uint Opacity {
			get {
				return opacity;
			}

			set {
				opacity = value;
			}
		}

		public XEventQueue Queue {
			get {
				return queue;
			}

			set {
				queue = value;
			}
		}

		public bool Enabled {
			get {
				if (!enabled) {
					return false;
				}

				if (parent != null) {
					return parent.Enabled;
				}

				return true;
			}

			set {
				enabled = value;
			}
		}

		public IntPtr EnabledHwnd {
			get {
				if (Enabled || parent == null) {
					return Handle;
				}

				return parent.EnabledHwnd;
			}
		}

		public Point MenuOrigin {
			get {
				Point	pt;
				Size	border_3D_size = ThemeEngine.Current.Border3DSize;

				pt = new Point(0, 0);

				if (border_style == FormBorderStyle.Fixed3D) {
					pt.X += border_3D_size.Width;
					pt.Y += border_3D_size.Height;
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
				if (invalid_list.Count == 1) {
					return (Rectangle) invalid_list [0];
				}

				Rectangle result = Rectangle.Empty;
				foreach (Rectangle r in invalid_list) {
					result = Rectangle.Union (result, r);
				}
				return result;

			}
		}

		public Rectangle[] ClipRectangles {
			get {
				return (Rectangle[]) invalid_list.ToArray (typeof (Rectangle));
 			}
 		}

		public Rectangle NCInvalid {
			get { return nc_invalid; }
			set { nc_invalid = value; }

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

		public bool Mapped {
			get {
				if (!mapped) {
					return false;
				}

				if (parent != null) {
					return parent.mapped;
				}

				return true;
			}

			set {
				mapped = value;
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
			AddInvalidArea(new Rectangle(x, y, width, height));
		}

		public void AddInvalidArea(Rectangle rect) {
			ArrayList tmp = new ArrayList ();
			foreach (Rectangle r in invalid_list) {
				if (!rect.Contains (r)) {
					tmp.Add (r);
				}
			}
			tmp.Add (rect);
			invalid_list = tmp;
		}

		public void ClearInvalidArea() {
			invalid_list.Clear();
			expose_pending = false;
		}

		public void AddNcInvalidArea(int x, int y, int width, int height) {
			if (nc_invalid == Rectangle.Empty) {
				nc_invalid = new Rectangle (x, y, width, height);
				return;
			}

			int right, bottom;
			right = Math.Max (nc_invalid.Right, x + width);
			bottom = Math.Max (nc_invalid.Bottom, y + height);
			nc_invalid.X = Math.Min (nc_invalid.X, x);
			nc_invalid.Y = Math.Min (nc_invalid.Y, y);

			nc_invalid.Width = right - nc_invalid.X;
			nc_invalid.Height = bottom - nc_invalid.Y;
		}

		public void AddNcInvalidArea(Rectangle rect) {
			if (nc_invalid == Rectangle.Empty) {
				nc_invalid = rect;
				return;
			}
			nc_invalid = Rectangle.Union (nc_invalid, rect);
		}

		public void ClearNcInvalidArea() {
			nc_invalid = Rectangle.Empty;
			nc_expose_pending = false;
		}

		public override string ToString() {
			return String.Format("Hwnd, Mapped:{3} ClientWindow:0x{0:X}, WholeWindow:0x{1:X}, Parent:[{2:X}]", client_window.ToInt32(), whole_window.ToInt32(), parent != null ? parent.ToString() : "<null>", Mapped);
		}

		#endregion	// Methods
	}
}
