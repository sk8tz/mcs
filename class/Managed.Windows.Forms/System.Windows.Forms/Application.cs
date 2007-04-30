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
// Copyright (c) 2004 - 2006 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//      Daniel Nauck    (dna(at)mono-project(dot)de)
//

// COMPLETE

#undef DebugRunLoop

using Microsoft.Win32;
using System;
using System.Drawing;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
#if NET_2_0
using System.Text;
using System.Windows.Forms.VisualStyles;
#endif

namespace System.Windows.Forms {
	public sealed class Application {
		internal class MWFThread {
			#region Fields
			private ApplicationContext	context;
			private bool			messageloop_started;
			private bool                    handling_exception;
			private int			thread_id;

			private static Hashtable	threads = new Hashtable();
			#endregion	// Fields

			#region Constructors
			private MWFThread() {
			}
			#endregion	// Constructors

			#region Properties
			public ApplicationContext Context {
				get { return context; }
				set { context = value; }
			}

			public bool MessageLoop {
				get { return messageloop_started; }
				set { messageloop_started = value; }
			}

			public bool HandlingException {
				get { return handling_exception; }
				set { handling_exception = value; }

			}

			public static int LoopCount {
				get {
					lock (threads) {
						int loops = 0;

						foreach (MWFThread thread in threads.Values) {
							if (thread.messageloop_started)
								loops++;
						}

						return loops;
					}
				}
			}

			public static MWFThread Current {
				get {
					MWFThread	thread;

					thread = null;
					lock (threads) {
						thread = (MWFThread)threads[Thread.CurrentThread.GetHashCode()];
						if (thread == null) {
							thread = new MWFThread();
							thread.thread_id = Thread.CurrentThread.GetHashCode();
							threads[thread.thread_id] = thread;
						}
					}

					return thread;
				}
			}
			#endregion	// Properties

			#region Methods
			public void Exit() {
				if (context != null) {
					context.ExitThread();
				}
				context = null;

				if (Application.ThreadExit != null) {
					Application.ThreadExit(null, EventArgs.Empty);
				}

				if (LoopCount == 0) {
					if (Application.ApplicationExit != null) {
						Application.ApplicationExit(null, EventArgs.Empty);
					}
				}

				((MWFThread)threads[thread_id]).MessageLoop = false;
			}
			#endregion	// Methods
		}

		private static bool			browser_embedded	= false;
		private static InputLanguage		input_language		= InputLanguage.CurrentInputLanguage;
		private static string			safe_caption_format	= "{1} - {0} - {2}";
		private static ArrayList		message_filters		= new ArrayList();
		private static FormCollection		forms			= new FormCollection ();
		private static bool			use_wait_cursor		= false;

#if NET_2_0
		private static ToolStrip keyboard_capture;
		private static VisualStyleState visual_style_state = VisualStyleState.ClientAndNonClientAreasEnabled;
#endif

		private Application () {
		}

		#region Private Methods
		internal static void CloseForms(Thread thread) {
			#if DebugRunLoop
				Console.WriteLine("   CloseForms({0}) called", thread);
			#endif

			ArrayList forms_to_close = new ArrayList ();

			lock (forms) {
				foreach (Form f in forms) {
					if (thread == null || thread == f.creator_thread)
						forms_to_close.Add (f);
				}

				foreach (Form f in forms_to_close) {
					#if DebugRunLoop
						Console.WriteLine("      Closing form {0}", f);
					#endif
					f.Dispose ();
				}
			}
		}
		#endregion	// Private methods

		#region Public Static Properties
		public static bool AllowQuit {
			get {
				return !browser_embedded;
			}
		}

		public static string CommonAppDataPath {
			get {
				return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			}
		}

		public static RegistryKey CommonAppDataRegistry {
			get {
				RegistryKey	key;

				key = Registry.LocalMachine.OpenSubKey("Software\\" + Application.CompanyName + "\\" + Application.ProductName + "\\" + Application.ProductVersion, true);

				return key;
			}
		}

		public static string CompanyName {
			get {
				AssemblyCompanyAttribute[] attrs = (AssemblyCompanyAttribute[]) Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), true);
				
				if ((attrs != null) && attrs.Length>0) {
					return attrs[0].Company;
				}

				return Assembly.GetEntryAssembly().GetName().Name;
			}
		}

		public static CultureInfo CurrentCulture {
			get {
				return Thread.CurrentThread.CurrentUICulture;
			}

			set {
				
				Thread.CurrentThread.CurrentUICulture=value;
			}
		}

		public static InputLanguage CurrentInputLanguage {
			get {
				return input_language;
			}

			set {
				input_language=value;
			}
		}

		public static string ExecutablePath {
			get {
				return Assembly.GetEntryAssembly().Location;
			}
		}

		public static string LocalUserAppDataPath {
			get {
				return Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), CompanyName), ProductName), ProductVersion);
			}
		}

		public static bool MessageLoop {
			get {
				return MWFThread.Current.MessageLoop;
			}
		}

		public static string ProductName {
			get {
				AssemblyProductAttribute[] attrs = (AssemblyProductAttribute[]) Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), true);
				
				if ((attrs != null) && attrs.Length>0) {
					return attrs[0].Product;
				}

				return Assembly.GetEntryAssembly().GetName().Name;
			}
		}

		public static string ProductVersion {
			get {
				String version;

				version = Assembly.GetEntryAssembly().GetName().Version.ToString();

				return version;
			}
		}

		public static string SafeTopLevelCaptionFormat {
			get {
				return safe_caption_format;
			}

			set {
				safe_caption_format=value;
			}
		}

		public static string StartupPath {
			get {
				return Path.GetDirectoryName(Application.ExecutablePath);
			}
		}

		public static string UserAppDataPath {
			get {
				return Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), CompanyName), ProductName), ProductVersion);
			}
		}

		public static RegistryKey UserAppDataRegistry {
			get {
				RegistryKey	key;

				key = Registry.CurrentUser.OpenSubKey("Software\\" + Application.CompanyName + "\\" + Application.ProductName + "\\" + Application.ProductVersion, true);

				return key;
			}
		}

#if NET_2_0

		public static bool UseWaitCursor {
			get {
				return use_wait_cursor;
			}
			set {
				use_wait_cursor = value;
				if (use_wait_cursor) {
					foreach (Form form in OpenForms) {
						form.Cursor = Cursors.WaitCursor;
					}
				}
			}
		}
		
		public static bool RenderWithVisualStyles {
		      get {
				if (VisualStyleInformation.IsSupportedByOS)
				{
					if (!VisualStyleInformation.IsEnabledByUser)
						return false;
				  
					if (!XplatUI.ThemesEnabled)
						return false;
				  
					if (Application.VisualStyleState == VisualStyleState.ClientAndNonClientAreasEnabled)
						return true;
				  
					if (Application.VisualStyleState == VisualStyleState.ClientAreaEnabled)
				  		return true;
				}
				
				return false;
		      }
		}

		public static VisualStyleState VisualStyleState {
			get { return Application.visual_style_state; }
			set { Application.visual_style_state = value; }
		}
#endif

		#endregion

		#region Public Static Methods
		public static void AddMessageFilter(IMessageFilter value) {
			lock (message_filters) {
				message_filters.Add(value);
			}
		}

		public static void DoEvents() {
			XplatUI.DoEvents();
		}

		public static void EnableVisualStyles() {
			XplatUI.EnableThemes();
		}

#if NET_2_0
		//
		// If true, it uses GDI+, performance reasons were quoted
		//
		static internal bool use_compatible_text_rendering = true;
		
		public static void SetCompatibleTextRenderingDefault (bool defaultValue)
		{
			use_compatible_text_rendering = defaultValue;
		}

		public static FormCollection OpenForms {
			get {
				return forms;
			}
		}

		[MonoNotSupported ("Empty stub.")]
		public static void SetUnhandledExceptionMode (UnhandledExceptionMode mode)
		{
			//FIXME: a stub to fill
		}

		public static void Restart ()
		{
			//FIXME: ClickOnce stuff using the Update or UpdateAsync methods.
			//FIXME: SecurityPermission: Restart () requires IsUnrestricted permission.

			if (Assembly.GetEntryAssembly () == null)
				throw new NotSupportedException ("The method 'Restart' is not supported by this application type.");

			string mono_path = null;

			//Get mono path
			PropertyInfo gac = typeof (Environment).GetProperty ("GacPath", BindingFlags.Static | BindingFlags.NonPublic);
			MethodInfo get_gac = null;
			if (gac != null)
				get_gac = gac.GetGetMethod (true);

			if (get_gac != null) {
				string gac_path = Path.GetDirectoryName ((string)get_gac.Invoke (null, null));
				string mono_prefix = Path.GetDirectoryName (Path.GetDirectoryName (gac_path));

				if (Environment.OSVersion.Platform == PlatformID.Unix) {
					mono_path = Path.Combine (mono_prefix, "bin/mono");
					if (!File.Exists (mono_path))
						mono_path = "mono";
				}
				else {
					mono_path = Path.Combine (mono_prefix, "bin\\mono.bat");

					if (!File.Exists (mono_path))
						mono_path = Path.Combine (mono_prefix, "bin\\mono.exe");

					if (!File.Exists (mono_path))
						mono_path = Path.Combine (mono_prefix, "mono\\mono\\mini\\mono.exe");

					if (!File.Exists (mono_path))
						throw new FileNotFoundException (string.Format ("Windows mono path not found: '{0}'", mono_path));
				}
			}

			//Get command line arguments
			StringBuilder argsBuilder = new StringBuilder ();
			string[] args = Environment.GetCommandLineArgs ();
			for (int i = 0; i < args.Length; i++)
			{
				argsBuilder.Append (string.Format ("\"{0}\" ", args[i]));
			}
			string arguments = argsBuilder.ToString ();
			ProcessStartInfo procInfo = Process.GetCurrentProcess ().StartInfo;

			if (mono_path == null) { //it is .NET on Windows
				procInfo.FileName = args[0];
				procInfo.Arguments = arguments.Remove (0, args[0].Length + 3); //1 space and 2 quotes
			}
			else {
				procInfo.Arguments = arguments;
				procInfo.FileName = mono_path;
			}

			procInfo.WorkingDirectory = Environment.CurrentDirectory;

			Application.Exit ();
			Process.Start (procInfo);
		}
#endif

		public static void Exit() {
			XplatUI.PostQuitMessage(0);
			CloseForms(null);
		}

		public static void ExitThread() {
			CloseForms(Thread.CurrentThread);
			// this might not be right - need to investigate (somehow) if a WM_QUIT message is generated here
			XplatUI.PostQuitMessage(0);
		}

		public static ApartmentState OleRequired() {
			//throw new NotImplementedException("OLE Not supported by this System.Windows.Forms implementation");
			return ApartmentState.Unknown;
		}

		public static void OnThreadException(Exception t) {
			if (MWFThread.Current.HandlingException) {
				/* we're already handling an exception and we got
				   another one?  print it out and exit, this means
				   we've got a runtime/SWF bug. */
				Console.WriteLine (t);
				Application.Exit();
			}

			MWFThread.Current.HandlingException = true;

			if (Application.ThreadException != null) {
				Application.ThreadException(null, new ThreadExceptionEventArgs(t));
				return;
			}

			if (SystemInformation.UserInteractive) {
				Form form = new ThreadExceptionDialog (t);
				form.ShowDialog ();
			} else {
				Console.WriteLine (t.ToString ());
				Application.Exit ();
			}

			MWFThread.Current.HandlingException = false;
		}

		public static void RemoveMessageFilter(IMessageFilter filter) {
			lock (message_filters) {
				message_filters.Remove(filter);
			}
		}

		public static void Run() {
			RunLoop(false, new ApplicationContext());
		}

		public static void Run(Form mainForm) {
			RunLoop(false, new ApplicationContext(mainForm));
		}

		public static void Run(ApplicationContext context) {
			RunLoop(false, context);
		}

		private static void DisableFormsForModalLoop (Queue toplevels, ApplicationContext context)
		{
			Form f;

			lock (forms) {
				IEnumerator control = forms.GetEnumerator ();

				while (control.MoveNext ()) {
					f = (Form)control.Current;

					// Don't disable the main form.
					if (f == context.MainForm) {
						continue;
					}

					// Don't disable any children of the main form.
					// These do not have to be MDI children.
					Control current = f;
					bool is_child_of_main = false; ;

					do {
						if (current.Parent == context.MainForm) {
							is_child_of_main = true;
							break;
						}
						current = current.Parent;
					} while (current != null);

					if (is_child_of_main)
						continue;

					// Disable the rest
					if (f.IsHandleCreated && XplatUI.IsEnabled (f.Handle)) {
#if DebugRunLoop
								Console.WriteLine("      Disabling form {0}", f);
#endif
						XplatUI.EnableWindow (f.Handle, false);
						toplevels.Enqueue (f);
					}
				}
			}
				
		}
		
		
		private static void EnableFormsForModalLoop (Queue toplevels, ApplicationContext context)
		{
			while (toplevels.Count > 0) {
#if DebugRunLoop
						Console.WriteLine("      Re-Enabling form form {0}", toplevels.Peek());
#endif
				Form c = (Form)toplevels.Dequeue ();
				if (c.IsHandleCreated) {
					XplatUI.EnableWindow (c.window.Handle, true);
					context.MainForm = c;
				}
			}
#if DebugRunLoop
					Console.WriteLine("   Done with the re-enable");
#endif
		}

		internal static void RunLoop(bool Modal, ApplicationContext context) {
			Queue		toplevels;
			MSG		msg;
			Object		queue_id;
			MWFThread	thread;
			ApplicationContext previous_thread_context;
			
			thread = MWFThread.Current;

			/*
			 * There is a NotWorking test for this, but since we are using this method both for Form.ShowDialog as for ApplicationContexts we'll
			 * fail on nested ShowDialogs, so disable the check for the moment.
			 */
			//if (thread.MessageLoop) {
			//        throw new InvalidOperationException ("Starting a second message loop on a single thread is not a valid operation. Use Form.ShowDialog instead.");
			//}

			msg = new MSG();

			if (context == null) {
				context = new ApplicationContext();
			}
		
			previous_thread_context = thread.Context;
			thread.Context = context;

			if (context.MainForm != null) {
				context.MainForm.is_modal = Modal;
				context.MainForm.context = context;
				context.MainForm.closing = false;
				context.MainForm.Visible = true;	// Cannot use Show() or scaling gets confused by menus
				// XXX the above line can be used to close the form. another problem with our handling of Show/Activate.
				if (context.MainForm != null)
					context.MainForm.Activate();
			}

			#if DebugRunLoop
				Console.WriteLine("Entering RunLoop(Modal={0}, Form={1})", Modal, context.MainForm != null ? context.MainForm.ToString() : "NULL");
			#endif

			if (Modal) {
				toplevels = new Queue ();
				DisableFormsForModalLoop (toplevels, context);
				
				// FIXME - need activate?
				/* make sure the MainForm is enabled */
				if (context.MainForm != null) {
					XplatUI.EnableWindow (context.MainForm.Handle, true);
					XplatUI.SetModal(context.MainForm.Handle, true);
				}
			} else {
				toplevels = null;
			}

			queue_id = XplatUI.StartLoop(Thread.CurrentThread);
			thread.MessageLoop = true;

			bool quit = false;

			while (!quit && XplatUI.GetMessage(queue_id, ref msg, IntPtr.Zero, 0, 0)) {
				lock (message_filters) {
					if (message_filters.Count > 0) {
						Message	m;
						bool	drop;

						drop = false;
						m = Message.Create(msg.hwnd, (int)msg.message, msg.wParam, msg.lParam);
						for (int i = 0; i < message_filters.Count; i++) {
							if (((IMessageFilter)message_filters[i]).PreFilterMessage(ref m)) {
								// we're dropping the message
								drop = true;
								break;
							}
						}
						if (drop) {
							continue;
						}
					}
				}

				switch((Msg)msg.message) {
				case Msg.WM_KEYDOWN:
				case Msg.WM_SYSKEYDOWN:
				case Msg.WM_CHAR:
				case Msg.WM_SYSCHAR:
				case Msg.WM_KEYUP:
				case Msg.WM_SYSKEYUP:
					Message m;
					m = Message.Create(msg.hwnd, (int)msg.message, msg.wParam, msg.lParam);

					Control c;
					c = Control.FromHandle(msg.hwnd);

#if NET_2_0
					// If we have a control with keyboard capture (usually a *Strip)
					// give it the message, and then drop the message
					if (keyboard_capture != null) {
						// WM_SYSKEYUP does not make it into ProcessCmdKey, so do it here
						if ((Msg)m.Msg == Msg.WM_SYSKEYUP)
							if (((Keys)m.WParam.ToInt32 () & Keys.Menu) == Keys.Menu) {
								keyboard_capture.GetTopLevelToolStrip ().Dismiss (ToolStripDropDownCloseReason.Keyboard);
								continue;
						}

						m.HWnd = keyboard_capture.Handle;
						
						if (!keyboard_capture.PreProcessMessage (ref m)) {
							if (c == null || c.Parent == null || !(c.Parent is ToolStrip))
								continue;
							else
								m.HWnd = msg.hwnd;
						} else
							continue;
					}
#endif

					if ((c != null) && !c.PreProcessMessage(ref m)) {
						goto default;
					}
					break;
				case Msg.WM_QUIT:
					quit = true; // make sure we exit
					break;
				default:
					XplatUI.TranslateMessage(ref msg);
					XplatUI.DispatchMessage(ref msg);
					break;
				}

				// Handle exit, Form might have received WM_CLOSE and set 'closing' in response
				if ((context.MainForm != null) && (context.MainForm.closing || (Modal && !context.MainForm.Visible))) {
					if (!Modal) {
						XplatUI.PostQuitMessage(0);
					} else {
						break;
					}
				}
			}
			#if DebugRunLoop
				Console.WriteLine("   RunLoop loop left");
			#endif

			thread.MessageLoop = false;
			XplatUI.EndLoop(Thread.CurrentThread);

			if (Modal) {
				Form old = context.MainForm;

				context.MainForm = null;

				EnableFormsForModalLoop (toplevels, context);
				
				if (context.MainForm != null && context.MainForm.IsHandleCreated) {
					XplatUI.SetModal(context.MainForm.Handle, false);
				}
				#if DebugRunLoop
					Console.WriteLine("   Done with the SetModal");
				#endif
				old.Close();
				old.is_modal = false;
			}

			#if DebugRunLoop
				Console.WriteLine("Leaving RunLoop(Modal={0}, Form={1})", Modal, context.MainForm != null ? context.MainForm.ToString() : "NULL");
			#endif
			if (context.MainForm != null) {
				context.MainForm.context = null;
				context.MainForm = null;
			}

			thread.Context = previous_thread_context;

			if (!Modal) {
				thread.Exit();
			}
		}

		#endregion	// Public Static Methods

		#region Events
		public static event EventHandler	ApplicationExit;

		public static event EventHandler	Idle {
			add {
				XplatUI.Idle += value;
			}
			remove {
				XplatUI.Idle -= value;
			}
		}

		public static event EventHandler	ThreadExit;
		public static event ThreadExceptionEventHandler	ThreadException;

#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public static event EventHandler EnterThreadModal;

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public static event EventHandler LeaveThreadModal;
#endif
		#endregion	// Events

		#region Internal Properties
#if NET_2_0
		internal static ToolStrip KeyboardCapture {
			get { return keyboard_capture; }
			set { keyboard_capture = value; }
		}
#endif
		#endregion
		
		#region Internal Methods
		internal static void AddForm (Form f)
		{
			lock (forms)
				forms.Add (f);
		}
		
		internal static void RemoveForm (Form f)
		{
			lock (forms)
				forms.Remove (f);
		}
		#endregion
	}
}
