//
// System.Windows.Forms.Application
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Miguel de Icaza (miguel@ximian.com)
//	Dennis hayes (dennish@raytek.com)
//   WINELib implementation started by John Sohn (jsohn@columbus.rr.com)
//
// (C) Ximian, Inc 2002
//

using System;
using System.Drawing;
using Microsoft.Win32;
using System.ComponentModel;
using System.Threading;
using System.Globalization;
using System.Reflection;
using System.Collections;
using System.Runtime.CompilerServices;

namespace System.Windows.Forms {

	/// <summary>
	/// Provides static methods and properties to manage an application, 
	/// such as methods to start and stop an application, to process 
	/// Windows messages, and properties to get information about an 
	/// application. This class cannot be inherited.
	/// </summary>

	[MonoTODO]
	public sealed class Application {
		static private ApplicationContext applicationContext = null;
		static private bool messageLoopStarted = false;
		static private bool messageLoopStopRequest = false;
		private static ArrayList messageFilters = new ArrayList ();

		// --- (public) Properties ---
		public static bool AllowQuit {
			// according to docs return false if embbedded in a 
			// browser, not (yet?) embedded in a browser
			get { return true; } 
		}
	
		[MonoTODO]
		public static string CommonAppDataPath {
			get { throw new NotImplementedException (); }
		}
	
		//[MonoTODO]
		// Registry key not yet defined (this should be interesting)
		//public static RegistryKey CommonAppDataRegistry {
		//	get { throw new NotImplementedException (); }
		//}
	
		[MonoTODO]
		public static string CompanyName {
			get { throw new NotImplementedException (); }
		}
	
		[MonoTODO]
		public static CultureInfo CurrentCulture {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	
		[MonoTODO]
		public static InputLanguage CurrentInputLanguage {
			get { throw new NotImplementedException (); }
		}
	
		[MonoTODO]
		public static string ExecutablePath {
			get { throw new NotImplementedException (); }
		}
	
		[MonoTODO]
		public static string LocalUserAppDataPath {
			get { throw new NotImplementedException (); }
		}
	
		public static bool MessageLoop {
			get { return messageLoopStarted; }
		}
	
		[MonoTODO]
		public static string ProductName {
			get { throw new NotImplementedException (); }
		}
	
		[MonoTODO]
		public static string ProductVersion {
			get { throw new NotImplementedException (); }
		}
	
		[MonoTODO]
		public static string SafeTopLevelCaptionFormat {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	
		[MonoTODO]
		public static string StartupPath {
			get { throw new NotImplementedException (); }
		}
	
		[MonoTODO]
		public static string UserAppDataPath {
			get { throw new NotImplementedException (); }
		}
	
		//[MonoTODO]
		// Registry key not yet defined
		//public static RegistryKey UserAppDataRegistry {
		//	get { throw new NotImplementedException (); }
		//}
	
		// --- Methods ---
		public static void AddMessageFilter (IMessageFilter value) 
		{
			messageFilters.Add (value);
		}

		//Compact Framework	
		public static void DoEvents () 
		{
			Win32.MSG msg = new Win32.MSG();

			while (Win32.PeekMessageA (ref msg, (IntPtr) 0,  0, 0,
						   Win32.PM_REMOVE) != 0);
		}

		//Compact Framework	
		public static void Exit () 
		{
			Win32.PostQuitMessage (0);
		}
	
		public static void ExitThread () 
		{
			messageLoopStopRequest = true;
		}
	
		[MonoTODO]
		public static ApartmentState OleRequired () 
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		public static void OnThreadException (Exception t) 
		{
			throw new NotImplementedException ();
		}
	
		public static void RemoveMessageFilter (IMessageFilter value)
		{
			messageFilters.Remove (value);
		}

		static private void ApplicationFormClosed (object o, EventArgs args)
		{
			Win32.PostQuitMessage (0);
		}

		//Compact Framework
		static public void Run ()
		{
			Win32.MSG msg = new Win32.MSG();

			messageLoopStarted = true;

			while (!messageLoopStopRequest && 
			       Win32.GetMessageA (ref msg, 0, 0, 0) != 0) {

				bool dispatchMessage = true;

				Message message = new Message ();
				message.HWnd = msg.hwnd;
				message.Msg = (int) msg.message;
				message.WParam = msg.wParam;
				message.LParam = msg.lParam;

				IEnumerator e = messageFilters.GetEnumerator();

				while (e.MoveNext()) {
					IMessageFilter filter = 
					    (IMessageFilter) e.Current;

					// if PreFilterMessage returns true
					// the message should not be dispatched
					if (filter.PreFilterMessage (ref message))
						dispatchMessage = false;
				}

				if (dispatchMessage) {
					Win32.TranslateMessage (ref msg);
					Win32.DispatchMessageA (ref msg);
				}

				//if (Idle != null)
					//Idle (null, new EventArgs());
			}

			//if (ApplicationExit != null)
				//ApplicationExit (null, new EventArgs());
		}

		public static void Run (ApplicationContext context) 
		{
			applicationContext = context;
			applicationContext.MainForm.Show ();
			applicationContext.MainForm.Closed += 
			    new EventHandler (ApplicationFormClosed);
			Run();
		}

		//[TypeAttributes.BeforeFieldInit]
		public static void Run (Form form)
		// Documents say this parameter name should be mainform, 
		// but the verifier says context.
		{
			form.CreateControl ();
			ApplicationContext context = new ApplicationContext (
				form);
			Run (context);
		}
		
		// --- Events ---
		public static event EventHandler ApplicationExit;
		public static event EventHandler Idle;
		public static event ThreadExceptionEventHandler ThreadException;
		public static event EventHandler ThreadExit;
	}
}
