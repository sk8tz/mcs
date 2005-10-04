//
// System.Drawing.carbonFunctions.cs
//
// Authors:
//      Geoff Norton (gnorton@customerdna.com>
//
// Copyright (C) 2004 Novell, Inc. (http://www.novell.com)
//
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

using System.Runtime.InteropServices;
using System.Security;

namespace System.Drawing {

	[SuppressUnmanagedCodeSecurity]
	internal class Carbon {

		internal static CarbonContext GetCGContextForNSView (IntPtr hwnd) {
			IntPtr cgContext = IntPtr.Zero;

			cgContext = objc_msgSend (objc_msgSend (objc_getClass ("NSGraphicsContext"), sel_registerName ("currentContext")), sel_registerName ("graphicsPort"));
			HIRect rect = new HIRect ();
			objc_msgSend_stret (ref rect, hwnd, sel_registerName ("bounds"));
			return new CarbonContext (cgContext, (int)rect.size.width, (int)rect.size.height);
		}
		internal static CarbonContext GetCGContextForView (IntPtr hwnd) {
			IntPtr cgContext = IntPtr.Zero;
			// Grab the window we're in
			IntPtr window = Carbon.GetControlOwner (hwnd);
			// Get the port of the window
			IntPtr port = Carbon.GetWindowPort (window);
			// Create a CGContext ref
			Carbon.CreateCGContextForPort (port, ref cgContext);
			
			// Get the bounds of the window
			QRect wBounds = new QRect ();
			Carbon.GetWindowBounds (window, 32, ref wBounds);
			
			// Get the bounds of the view
			HIRect vBounds = new HIRect ();
			Carbon.HIViewGetBounds (hwnd, ref vBounds);
			
			// Convert the view local bounds to window coordinates
			Carbon.HIViewConvertRect (ref vBounds, hwnd, IntPtr.Zero);
			Carbon.CGContextTranslateCTM (cgContext, vBounds.origin.x, (wBounds.bottom-wBounds.top)-(vBounds.origin.y+vBounds.size.height));
			/* FIXME: Do we need this or is it inherintly clipped */
			HIRect rcClip = new HIRect ();
			rcClip.origin.x = 0;
			rcClip.origin.y = 0;
			rcClip.size.width = vBounds.size.width;
			rcClip.size.height = vBounds.size.height;
			Carbon.CGContextClipToRect (cgContext, rcClip);
			return new CarbonContext (cgContext, (int)vBounds.size.width, (int)vBounds.size.height);
		}
		#region Cocoa Methods
		[DllImport("libobjc.dylib")]
		public static extern IntPtr objc_getClass(string className); 
		[DllImport("libobjc.dylib")]
		public static extern IntPtr objc_msgSend(IntPtr basePtr, IntPtr selector, string argument);  
		[DllImport("libobjc.dylib")]
		public static extern IntPtr objc_msgSend(IntPtr basePtr, IntPtr selector);        
		[DllImport("libobjc.dylib")]
		public static extern void objc_msgSend_stret(ref HIRect arect, IntPtr basePtr, IntPtr selector);        
		[DllImport("libobjc.dylib")]
		public static extern IntPtr sel_registerName(string selectorName);         
		#endregion

		[DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
                internal static extern int HIViewGetBounds (IntPtr vHnd, ref HIRect r);
                [DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
                internal static extern int HIViewConvertRect (ref HIRect r, IntPtr a, IntPtr b);

                [DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
                internal static extern IntPtr GetControlOwner (IntPtr aView);

                [DllImport("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
                internal static extern int GetWindowBounds (IntPtr wHnd, uint reg, ref QRect rect);
                [DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
                internal static extern IntPtr GetWindowPort (IntPtr hWnd);
                [DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
                internal static extern int CGContextClipToRect (IntPtr cgContext, HIRect clip);
                [DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
                internal static extern void CreateCGContextForPort (IntPtr port, ref IntPtr cgc);
                [DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
                internal static extern void CGContextTranslateCTM (IntPtr cgc, double tx, double ty);
                [DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
                internal static extern void CGContextScaleCTM (IntPtr cgc, double x, double y);
                [DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
                internal static extern void CGContextFlush (IntPtr cgc);
                [DllImport ("/System/Library/Frameworks/Carbon.framework/Versions/Current/Carbon")]
                internal static extern void CGContextSynchronize (IntPtr cgc);
	}

	internal struct CGSize {
		public float width;
		public float height;
	}

	internal struct CGPoint {
		public float x;
		public float y;
	}

	internal struct HIRect {
		public CGPoint origin;
		public CGSize size;
	}

	internal struct QRect
	{
		public short top;
		public short left;
		public short bottom;
		public short right;
	}

	internal struct CarbonContext
	{
		public IntPtr ctx;
		public int width;
		public int height;

		public CarbonContext (IntPtr ctx, int width, int height)
		{
			this.ctx = ctx;
			this.width = width;
			this.height = height;
		}
	}
}
