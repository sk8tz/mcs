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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)
//

using System;
using System.Text;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Mono.WebBrowser;

namespace Mono.Mozilla
{
	internal class Base
	{
		private static Hashtable boundControls;
		internal static bool xulbrowserInstalled;

		private class BindingInfo
		{
			public CallbackBinder callback;
			public IntPtr xulbrowser;
		}

		private static bool isInitialized ()
		{
			if (!xulbrowserInstalled)
				return false;
			return true;
		}

		private static BindingInfo getBinding (IWebBrowser control)
		{
			if (!boundControls.ContainsKey (control))
				return null;
			BindingInfo info = boundControls[control] as BindingInfo;
			return info;
		}

		static Base ()
		{
			boundControls = new Hashtable ();
		}

		public Base () { }

		public static void DebugStartup ()
		{
			xulbrowser_debug_startup ();
			Trace.Listeners.Add (new TextWriterTraceListener (@"log"));
			Trace.AutoFlush = true;
		}

		public static void Init (WebBrowser control)
		{
			BindingInfo info = new BindingInfo ();
			info.callback = new CallbackBinder (control);
			IntPtr ptrCallback = Marshal.AllocHGlobal (Marshal.SizeOf (info.callback));
			Marshal.StructureToPtr (info.callback, ptrCallback, true);

			try {
				info.xulbrowser = xulbrowser_init (ptrCallback, Environment.CurrentDirectory);
			}
			catch (DllNotFoundException) {
				Console.WriteLine ("libxulbrowser not found. To have webbrowser support, you need libxulbrowser installed");
				Marshal.FreeHGlobal (ptrCallback);
				xulbrowserInstalled = false;
				return;
			}
			xulbrowserInstalled = true;
			boundControls.Add (control as IWebBrowser, info);
			DebugStartup ();
		}

		public static void Shutdown (IWebBrowser control)
		{
			if (!isInitialized ())
				return;
			BindingInfo info = getBinding (control);

			xulbrowser_shutdown (info.xulbrowser);
		}

		public static void Bind (IWebBrowser control, IntPtr handle, int width, int height)
		{
			if (!isInitialized ())
				return;
			BindingInfo info = getBinding (control);

			xulbrowser_createBrowserWindow (info.xulbrowser, handle, width, height);
		}

		// layout
		public static void Focus (IWebBrowser control, FocusOption focus)
		{
			if (!isInitialized ())
				return;
			BindingInfo info = getBinding (control);

			xulbrowser_focus (info.xulbrowser, focus);
		}


		public static void Blur (IWebBrowser control)
		{
			if (!isInitialized ())
				return;
			BindingInfo info = getBinding (control);

			xulbrowser_deactivate (info.xulbrowser);
		}

		public static void Activate (IWebBrowser control)
		{
			if (!isInitialized ())
				return;
			BindingInfo info = getBinding (control);

			xulbrowser_activate (info.xulbrowser);
		}

		public static void Deactivate (IWebBrowser control)
		{
			if (!isInitialized ())
				return;
			BindingInfo info = getBinding (control);

			xulbrowser_deactivate (info.xulbrowser);
		}

		public static void Resize (IWebBrowser control, int width, int height)
		{
			if (!isInitialized ())
				return;
			BindingInfo info = getBinding (control);

			xulbrowser_resize (info.xulbrowser, width, height);
		}

		// navigation
		public static void Navigate (IWebBrowser control, string uri)
		{
			if (!isInitialized ())
				return;
			BindingInfo info = getBinding (control);

			xulbrowser_navigate (info.xulbrowser, uri);
		}


		public static bool Forward (IWebBrowser control)
		{
			if (!isInitialized ())
				return false;
			BindingInfo info = getBinding (control);

			return xulbrowser_forward (info.xulbrowser);
		}

		public static bool Back (IWebBrowser control)
		{
			if (!isInitialized ())
				return false;
			BindingInfo info = getBinding (control);

			return xulbrowser_back (info.xulbrowser);
		}

		public static void Home (IWebBrowser control)
		{
			if (!isInitialized ())
				return;
			BindingInfo info = getBinding (control);

			xulbrowser_home (info.xulbrowser);
		}

		public static void Stop (IWebBrowser control)
		{
			if (!isInitialized ())
				return;
			BindingInfo info = getBinding (control);

			xulbrowser_stop (info.xulbrowser);
		}

		public static void Reload (IWebBrowser control, ReloadOption option)
		{
			if (!isInitialized ())
				return;
			BindingInfo info = getBinding (control);

			xulbrowser_reload (info.xulbrowser, option);
		}

		public static nsIDOMHTMLDocument GetDOMDocument (IWebBrowser control)
		{
			if (!isInitialized ())
				return null;
			BindingInfo info = getBinding (control);

			return xulbrowser_getDomDocument (info.xulbrowser);
		}

		public static IntPtr StringInit ()
		{
			return xulbrowser_stringInit ();
		}

		public static void StringFinish (HandleRef str)
		{
			xulbrowser_stringFinish (str);
		}

		public static string StringGet (HandleRef str)
		{
			IntPtr p = xulbrowser_stringGet (str);
			return Marshal.PtrToStringUni (p);
		}

		public static void StringSet (HandleRef str, string text)
		{
			xulbrowser_stringSet (str, text);
		}

		#region pinvokes
		[DllImport("xulbrowser")]
		private static extern void xulbrowser_debug_startup();

		[DllImport("xulbrowser")]
		private static extern IntPtr xulbrowser_init (IntPtr events, string startDir);

		[DllImport ("xulbrowser")]
		private static extern IntPtr xulbrowser_shutdown (IntPtr instance);

		[DllImport ("xulbrowser")]
		private static extern int xulbrowser_createBrowserWindow (IntPtr instance, IntPtr hwnd, Int32 width, Int32 height);

		// layout
		[DllImport ("xulbrowser")]
		private static extern int xulbrowser_focus (IntPtr instance, FocusOption focus);
		[DllImport ("xulbrowser")]
		private static extern int xulbrowser_blur (IntPtr instance);
		[DllImport ("xulbrowser")]
		private static extern int xulbrowser_activate (IntPtr instance);
		[DllImport ("xulbrowser")]
		private static extern int xulbrowser_deactivate (IntPtr instance);
		[DllImport ("xulbrowser")]
		private static extern int xulbrowser_resize (IntPtr instance, Int32 width, Int32 height);

		// navigation
		[DllImport("xulbrowser")]
		private static extern int xulbrowser_navigate (IntPtr instance, string uri);
		[DllImport ("xulbrowser")]
		private static extern bool xulbrowser_forward (IntPtr instance);
		[DllImport ("xulbrowser")]
		private static extern bool xulbrowser_back (IntPtr instance);
		[DllImport ("xulbrowser")]
		private static extern int xulbrowser_home (IntPtr instance);
		[DllImport ("xulbrowser")]
		private static extern int xulbrowser_stop (IntPtr instance);
		[DllImport ("xulbrowser")]
		private static extern int xulbrowser_reload (IntPtr instance, ReloadOption option);

		// dom
		[DllImport ("xulbrowser")]
		private static extern nsIDOMHTMLDocument xulbrowser_getDomDocument (IntPtr instance);

		[DllImport ("xulbrowser")]
		private static extern IntPtr xulbrowser_stringInit ();
		[DllImport ("xulbrowser")]
		private static extern int xulbrowser_stringFinish (HandleRef str);
		[DllImport ("xulbrowser")]
		private static extern IntPtr xulbrowser_stringGet (HandleRef str);
		[DllImport ("xulbrowser")]
		private static extern void xulbrowser_stringSet (HandleRef str, [MarshalAs (UnmanagedType.LPWStr)] string text);
		#endregion
	}
}
