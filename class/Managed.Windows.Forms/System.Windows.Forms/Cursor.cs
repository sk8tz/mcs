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


// NOT COMPLETE

using System;
using System.Drawing;
using System.Runtime.Serialization;

namespace System.Windows.Forms {
	public sealed class Cursor : IDisposable, ISerializable {
		#region Local Variables
		internal IntPtr		handle;
		internal Size		size;
		#endregion	// Local Variables

		#region Public Constructors
		private Cursor() {
		}

		// This is supposed to take a Win32 handle
		public Cursor(IntPtr handle) {
			this.handle = handle;
		}

		public Cursor(System.IO.Stream stream) {
		}

		public Cursor(string fileName) {
			Bitmap c = (Bitmap)Bitmap.FromFile("cursor.bmp");
			Bitmap m = (Bitmap)Bitmap.FromFile("mask.bmp");

			handle = XplatUI.DefineCursor(c, m, Color.FromArgb(0, 0, 0), Color.FromArgb(0, 0, 0), 16, 16);
			size = new Size(c.Width, c.Height);
		}

		public Cursor(Type type, string resource) {
		}
		#endregion	// Public Constructors

		#region Public Static Properties
		public static Rectangle Clip {
			get {
				IntPtr		handle;
				bool		confined;
				Rectangle	rect;
				Size		size;

				XplatUI.GrabInfo(out handle, out confined, out rect);
				if (handle != IntPtr.Zero) {
					return rect;
				}

				XplatUI.GetDisplaySize(out size);
				rect.X = 0;
				rect.Y = 0;
				rect.Width = size.Width;
				rect.Height = size.Height;
				return rect;
			}

			[MonoTODO("First need to add ability to set cursor clip rectangle to XplatUI drivers to implement this property")]
			set {
				;
			}
		}
#if not
		public static Cursor Current {
			get {
				return IntPtr.Zero;
			}

			set {
			}
		}
#endif
		public static Point Position {
			get {
				int x;
				int y;

				XplatUI.GetCursorPos (IntPtr.Zero, out x, out y);
				return new Point (x, y);
			}

			set {
				XplatUI.SetCursorPos(IntPtr.Zero, value.X, value.Y);
			}
		}
		#endregion	// Public Static Properties

		#region Public Instance Properties
		public IntPtr Handle {
			get {
				return handle;
			}
		}

		public Size Size {
			get {
				return size;
			}
		}
		#endregion	// Public Instance Properties

		#region Public Static Methods
		public static void Hide() {
			XplatUI.ShowCursor(false);
		}

		public static void Show() {
			XplatUI.ShowCursor(false);
		}
		#endregion	// Public Static Methods

		#region Public Instance Methods
		public void Dispose() {
		}

		void ISerializable.GetObjectData(SerializationInfo si, StreamingContext context) {
			throw new NotImplementedException();
		}
		#endregion	// Public Instance Methods
	}
}
