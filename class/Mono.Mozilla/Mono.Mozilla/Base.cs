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
// Copyright (c) 2007, 2008 Novell, Inc.
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
		internal static bool gluezillaInstalled;

		private class BindingInfo
		{
			public CallbackBinder callback;
			public IntPtr gluezilla;
		}

		private static bool isInitialized ()
		{
			if (!gluezillaInstalled)
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
			gluezilla_debug_startup ();
		}
		
		public static bool Init (WebBrowser control, Platform platform)
		{
			BindingInfo info = new BindingInfo ();
			info.callback = new CallbackBinder (control.callbacks);
			IntPtr ptrCallback = Marshal.AllocHGlobal (Marshal.SizeOf (info.callback));
			Marshal.StructureToPtr (info.callback, ptrCallback, true);

			string monoMozDir = System.IO.Path.Combine (
				System.IO.Path.Combine (
				Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData),
				".mono"), "mozilla");

			if (!System.IO.Directory.Exists (monoMozDir))
				System.IO.Directory.CreateDirectory (monoMozDir);

			Platform mozPlatform;
			try {
				info.gluezilla = gluezilla_init (platform, ptrCallback, Environment.CurrentDirectory, monoMozDir, out mozPlatform);
			}
			catch (DllNotFoundException) {
				Console.WriteLine ("libgluezilla not found. To have webbrowser support, you need libgluezilla installed");
				Marshal.FreeHGlobal (ptrCallback);
				gluezillaInstalled = false;
				return false;
			}
			control.enginePlatform = mozPlatform;
			gluezillaInstalled = true;
			boundControls.Add (control as IWebBrowser, info);
			DebugStartup ();
			return true;
		}

		public static void Shutdown (IWebBrowser control)
		{
			if (!isInitialized ())
				return;
			BindingInfo info = getBinding (control);

			gluezilla_shutdown (info.gluezilla);
		}

		public static void Bind (IWebBrowser control, IntPtr handle, int width, int height)
		{
			if (!isInitialized ())
				return;
			BindingInfo info = getBinding (control);

			gluezilla_createBrowserWindow (info.gluezilla, handle, width, height);
		}

		// layout
		public static void Focus (IWebBrowser control, FocusOption focus)
		{
			if (!isInitialized ())
				return;
			BindingInfo info = getBinding (control);

			gluezilla_focus (info.gluezilla, focus);
		}


		public static void Blur (IWebBrowser control)
		{
			if (!isInitialized ())
				return;
			BindingInfo info = getBinding (control);

			gluezilla_blur (info.gluezilla);
		}

		public static void Activate (IWebBrowser control)
		{
			if (!isInitialized ())
				return;
			BindingInfo info = getBinding (control);

			gluezilla_activate (info.gluezilla);
		}

		public static void Deactivate (IWebBrowser control)
		{
			if (!isInitialized ())
				return;
			BindingInfo info = getBinding (control);

			gluezilla_deactivate (info.gluezilla);
		}

		public static void Resize (IWebBrowser control, int width, int height)
		{
			if (!isInitialized ())
				return;
			BindingInfo info = getBinding (control);

			gluezilla_resize (info.gluezilla, width, height);
		}

		// navigation
		public static void Home (IWebBrowser control)
		{
			if (!isInitialized ())
				return;
			BindingInfo info = getBinding (control);

			gluezilla_home (info.gluezilla);
		}

		public static nsIWebNavigation GetWebNavigation (IWebBrowser control)
		{
			if (!isInitialized ())
				return null;
			BindingInfo info = getBinding (control);

			return gluezilla_getWebNavigation (info.gluezilla);
		}

		public static IntPtr StringInit ()
		{
			return gluezilla_stringInit ();
		}

		public static void StringFinish (HandleRef str)
		{
			gluezilla_stringFinish (str);
		}

		public static string StringGet (HandleRef str)
		{
			IntPtr p = gluezilla_stringGet (str);
			return Marshal.PtrToStringUni (p);
		}

		public static void StringSet (HandleRef str, string text)
		{
			gluezilla_stringSet (str, text);
		}


		public static object GetProxyForObject (IWebBrowser control, Guid iid, object obj)
		{
			if (!isInitialized ())
				return null;
			BindingInfo info = getBinding (control);
			
			IntPtr ret;
			gluezilla_getProxyForObject (info.gluezilla, iid, obj, out ret);
			
			object o = Marshal.GetObjectForIUnknown (ret);
			return o;
		}

		#region pinvokes
		[DllImport("gluezilla")]
		private static extern void gluezilla_debug_startup();

		[DllImport("gluezilla")]
		private static extern IntPtr gluezilla_init (Platform platform, IntPtr events, string startDir, string dataDir, out Platform mozPlatform);

		[DllImport ("gluezilla")]
		private static extern IntPtr gluezilla_shutdown (IntPtr instance);

		[DllImport ("gluezilla")]
		private static extern int gluezilla_createBrowserWindow (IntPtr instance, IntPtr hwnd, Int32 width, Int32 height);

		// layout
		[DllImport ("gluezilla")]
		private static extern int gluezilla_focus (IntPtr instance, FocusOption focus);
		[DllImport ("gluezilla")]
		private static extern int gluezilla_blur (IntPtr instance);
		[DllImport ("gluezilla")]
		private static extern int gluezilla_activate (IntPtr instance);
		[DllImport ("gluezilla")]
		private static extern int gluezilla_deactivate (IntPtr instance);
		[DllImport ("gluezilla")]
		private static extern int gluezilla_resize (IntPtr instance, Int32 width, Int32 height);

		// navigation
		[DllImport ("gluezilla")]
		private static extern int gluezilla_home (IntPtr instance);

		// dom
		[DllImport ("gluezilla")]
		[return:MarshalAs(UnmanagedType.Interface)]
		private static extern nsIWebNavigation gluezilla_getWebNavigation (IntPtr instance);

		[DllImport ("gluezilla")]
		private static extern IntPtr gluezilla_stringInit ();
		[DllImport ("gluezilla")]
		private static extern int gluezilla_stringFinish (HandleRef str);
		[DllImport ("gluezilla")]
		private static extern IntPtr gluezilla_stringGet (HandleRef str);
		[DllImport ("gluezilla")]
		private static extern void gluezilla_stringSet (HandleRef str, [MarshalAs (UnmanagedType.LPWStr)] string text);

		[DllImport ("gluezilla")]
		private static extern void gluezilla_getProxyForObject (
			IntPtr instance, 
			[MarshalAs (UnmanagedType.LPStruct)] Guid iid, 
			[MarshalAs (UnmanagedType.Interface)] object obj,
			out IntPtr ret);


		[DllImport ("gluezilla")]
		public static extern uint gluezilla_StringContainerInit (HandleRef /*nsStringContainer & */aStr);

		[DllImport ("gluezilla")]
		public static extern void gluezilla_StringContainerFinish (HandleRef /*nsStringContainer & */aStr);

		[DllImport ("gluezilla")]
		public static extern uint gluezilla_StringGetData (HandleRef /*const nsAString &*/ aStr, 
			out IntPtr /*const PRUnichar ** */aBuf, 
			out bool /*PRBool * */aTerm);

		[DllImport ("gluezilla")]
		public static extern uint gluezilla_StringSetData (HandleRef /*nsAString &*/ aStr, 
			[MarshalAs (UnmanagedType.LPWStr)] string /*const PRUnichar * */ aBuf, uint aCount);

		[DllImport ("gluezilla")]
		public static extern uint gluezilla_CStringContainerInit (HandleRef /*nsCStringContainer &*/ aStr);

		[DllImport ("gluezilla")]
		public static extern void gluezilla_CStringContainerFinish (HandleRef /*nsCStringContainer &*/ aStr);

		[DllImport ("gluezilla")]
		public static extern uint gluezilla_CStringGetData (HandleRef /*const nsACString &*/ aStr, 
			out IntPtr /*const PRUnichar ** */aBuf, 
			out bool /*PRBool **/ aTerm);

		[DllImport ("gluezilla")]
		public static extern uint gluezilla_CStringSetData (HandleRef /*nsACString &*/ aStr, 
			string aBuf, 
			uint aCount);


		#endregion
	}
}
