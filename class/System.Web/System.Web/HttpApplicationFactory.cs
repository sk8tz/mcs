//
// System.Web.HttpApplicationFactory
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2002,2003 Ximian, Inc. (http://www.ximian.com)
// (c) Copyright 2004 Novell, Inc. (http://www.novell.com)
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
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Web.UI;
using System.Web.SessionState;
using System.Web.Configuration;

using System.Web.Compilation;
#if TARGET_J2EE
using vmw.common;
#endif

#if NET_2_0 && !TARGET_J2EE
using System.CodeDom.Compiler;
#endif

namespace System.Web {
	class HttpApplicationFactory {
		object this_lock = new object ();
		
		// Initialized in InitType
#if TARGET_J2EE
		static HttpApplicationFactory theFactory {
			get
			{
				HttpApplicationFactory factory = (HttpApplicationFactory)AppDomain.CurrentDomain.GetData("HttpApplicationFactory");
				if (factory == null) {
					lock(typeof(HttpApplicationFactory)) {
						factory = (HttpApplicationFactory)AppDomain.CurrentDomain.GetData("HttpApplicationFactory");
						if (factory == null) {
							factory = new HttpApplicationFactory();
							System.Threading.Thread.Sleep(1);
							AppDomain.CurrentDomain.SetData("HttpApplicationFactory", factory);
						}
					}
				}
				return factory;
			}
		}
#else
		static HttpApplicationFactory theFactory = new HttpApplicationFactory();
#endif
		MethodInfo session_end;
		bool needs_init = true;
		bool app_start_needed = true;
		Type app_type;
		HttpApplicationState app_state;
		Hashtable app_event_handlers;
		static ArrayList watchers = new ArrayList();
		static object watchers_lock = new object();
		static bool app_shutdown = false;
#if NET_2_0
		static bool app_disabled = false;
		static string[] app_browsers_files = new string[0];
#endif
		Stack available = new Stack ();
		Stack available_for_end = new Stack ();
		
		bool IsEventHandler (MethodInfo m)
		{
			int pos = m.Name.IndexOf ('_');
			if (pos == -1 || (m.Name.Length - 1) <= pos)
				return false;

			if (m.ReturnType != typeof (void))
				return false;

			ParameterInfo [] pi = m.GetParameters ();
			int length = pi.Length;
			if (length == 0)
				return true;

			if (length != 2)
				return false;

			if (pi [0].ParameterType != typeof (object) ||
			    pi [1].ParameterType != typeof (EventArgs))
				return false;
			
			return true;
		}

		void AddEvent (MethodInfo method, Hashtable appTypeEventHandlers)
		{
			string name = method.Name.Replace ("_On", "_");
			if (appTypeEventHandlers [name] == null) {
				appTypeEventHandlers [name] = method;
				return;
			}

			MethodInfo old_method = appTypeEventHandlers [name] as MethodInfo;
			ArrayList list;
			if (old_method != null){
				list = new ArrayList (4);
				list.Add (old_method);
				appTypeEventHandlers [name] = list;
			} else 
				list = appTypeEventHandlers [name] as ArrayList;

			list.Add (method);
		}
		
		Hashtable GetApplicationTypeEvents (Type type)
		{
			lock (this_lock) {
				if (app_event_handlers != null)
					return app_event_handlers;

				app_event_handlers = new Hashtable ();
				BindingFlags flags = BindingFlags.Public    | BindingFlags.NonPublic | 
					BindingFlags.Instance  | BindingFlags.Static;

				MethodInfo [] methods = type.GetMethods (flags);
				foreach (MethodInfo m in methods) {
					if (m.DeclaringType != typeof (HttpApplication) && IsEventHandler (m))
						AddEvent (m, app_event_handlers);
				}
			}

			return app_event_handlers;
		}

		Hashtable GetApplicationTypeEvents (HttpApplication app)
		{
			lock (this_lock) {
				if (app_event_handlers != null)
					return app_event_handlers;

				return GetApplicationTypeEvents (app.GetType ());
			}
		}

		bool FireEvent (string method_name, object target, object [] args)
		{
			Hashtable possibleEvents = GetApplicationTypeEvents ((HttpApplication) target);
			MethodInfo method = possibleEvents [method_name] as MethodInfo;
			if (method == null)
				return false;

			if (method.GetParameters ().Length == 0)
				args = null;

			method.Invoke (target, args);

			return true;
		}

		HttpApplication FireOnAppStart (HttpContext context)
		{
			HttpApplication app = (HttpApplication) Activator.CreateInstance (app_type, true);
			context.ApplicationInstance = app;
			app.SetContext (context);
			object [] args = new object [] {app, EventArgs.Empty};
			FireEvent ("Application_Start", app, args);
			return app;
		}

		void FireOnAppEnd ()
		{
			if (app_type == null)
				return; // we didn't even get an application

			HttpApplication app = (HttpApplication) Activator.CreateInstance (app_type, true);
			FireEvent ("Application_End", app, new object [] {new object (), EventArgs.Empty});
			app.Dispose ();
			app_type = null;
		}

		//
		// This is invoked by HttpRuntime.Dispose, when we unload an AppDomain
		// To reproduce this in action, touch "global.asax" while XSP is running.
		//
		public static void Dispose ()
		{
			theFactory.FireOnAppEnd ();
		}

		static FileSystemWatcher CreateWatcher (string file, FileSystemEventHandler hnd, RenamedEventHandler reh)
		{
			FileSystemWatcher watcher = new FileSystemWatcher ();

			watcher.Path = Path.GetFullPath (Path.GetDirectoryName (file));
			watcher.Filter = Path.GetFileName (file);
			
			// This will enable the Modify flag for Linux/inotify
			watcher.NotifyFilter |= NotifyFilters.Size;
			
			watcher.Changed += hnd;
			watcher.Created += hnd;
			watcher.Deleted += hnd;
			watcher.Renamed += reh;

			watcher.EnableRaisingEvents = true;

			return watcher;
		}

		internal static void AttachEvents (HttpApplication app)
		{
			HttpApplicationFactory factory = theFactory;
			Hashtable possibleEvents = factory.GetApplicationTypeEvents (app);
			foreach (string key in possibleEvents.Keys) {
				int pos = key.IndexOf ('_');
				string moduleName = key.Substring (0, pos);
				object target;
				if (moduleName == "Application") {
					target = app;
				} else {
					target = app.Modules [moduleName];
					if (target == null)
						continue;
				}

				string eventName = key.Substring (pos + 1);
				EventInfo evt = target.GetType ().GetEvent (eventName);
				if (evt == null)
					continue;

				string usualName = moduleName + "_" + eventName;
				object methodData = possibleEvents [usualName];
				if (methodData != null && eventName == "End" && moduleName == "Session") {
					lock (factory) {
						if (factory.session_end == null)
							factory.session_end = (MethodInfo) methodData;
					}
					continue;
				}

				if (methodData == null)
					continue;

				if (methodData is MethodInfo) {
					factory.AddHandler (evt, target, app, (MethodInfo) methodData);
					continue;
				}

				ArrayList list = (ArrayList) methodData;
				foreach (MethodInfo method in list)
					factory.AddHandler (evt, target, app, method);
			}
		}

		void AddHandler (EventInfo evt, object target, HttpApplication app, MethodInfo method)
		{
			int length = method.GetParameters ().Length;

			if (length == 0) {
				NoParamsInvoker npi = new NoParamsInvoker (app, method);
				evt.AddEventHandler (target, npi.FakeDelegate);
			} else {
				evt.AddEventHandler (target, Delegate.CreateDelegate (
							     evt.EventHandlerType, app,
#if NET_2_0
							     method
#else
							     method.Name
#endif
						     ));
			}
			
		}

		internal static void InvokeSessionEnd (object state)
		{
			InvokeSessionEnd (state, null, EventArgs.Empty);
		}
		
		internal static void InvokeSessionEnd (object state, object source, EventArgs e)
		{
			HttpApplicationFactory factory = theFactory;
			MethodInfo method = null;
			HttpApplication app = null;
			lock (factory.available_for_end) {
				method = factory.session_end;
				if (method == null)
					return;

				app = GetApplicationForSessionEnd ();
			}

			app.SetSession ((HttpSessionState) state);
			try {
				method.Invoke (app, new object [] {(source == null ? app : source), e});
			} catch (Exception) {
				// Ignore
			}
			RecycleForSessionEnd (app);
		}

		static HttpStaticObjectsCollection MakeStaticCollection (ArrayList list)
		{
			if (list == null || list.Count == 0)
				return null;

			HttpStaticObjectsCollection coll = new HttpStaticObjectsCollection ();
			foreach (ObjectTagBuilder tag in list) {
				coll.Add (tag);
			}

			return coll;
		}
		
		internal static HttpApplicationState ApplicationState {
#if TARGET_J2EE
			get {
				HttpApplicationFactory factory = theFactory;
				if (factory.app_state == null)
					factory.app_state = new HttpApplicationState (null, null);
				return factory.app_state;
			}
#else
			get {
				if (theFactory.app_state == null) {
					HttpStaticObjectsCollection app = MakeStaticCollection (GlobalAsaxCompiler.ApplicationObjects);
					HttpStaticObjectsCollection ses = MakeStaticCollection (GlobalAsaxCompiler.SessionObjects);

					theFactory.app_state = new HttpApplicationState (app, ses);
				}
				return theFactory.app_state;
			}
#endif
		}

		internal static Type AppType {
			get {
				return theFactory.app_type;
			}
		}
		
		void InitType (HttpContext context)
		{
			lock (this_lock) {
				if (!needs_init)
					return;

#if NET_2_0
				try {
#endif
					string physical_app_path = HttpRuntime.AppDomainAppPath;
					string app_file = null;
					
					app_file = Path.Combine (physical_app_path, "Global.asax");
					if (!File.Exists (app_file)) {
						app_file = Path.Combine (physical_app_path, "global.asax");
						if (!File.Exists (app_file))
							app_file = null;
					}
			
#if !NET_2_0
					WebConfigurationSettings.Init (context);
#endif
		
#if NET_2_0 && !TARGET_J2EE
					AppResourcesCompiler ac = new AppResourcesCompiler (context);
					ac.Compile ();
				
					// Todo: Generate profile properties assembly from Web.config here
				
					AppCodeCompiler acc = new AppCodeCompiler ();
					acc.Compile ();

#if WEBSERVICES_DEP
					AppWebReferencesCompiler awrc = new AppWebReferencesCompiler ();
					awrc.Compile ();
#endif
					
					// Note whether there are any App_Browsers/*.browser files.  If there
					// are we will be using *.browser files for sniffing in addition to browscap.ini
					string app_browsers_path = Path.Combine (physical_app_path, "App_Browsers");
					app_browsers_files = new string[0];
					if (Directory.Exists (app_browsers_path)) {
						app_browsers_files = Directory.GetFiles (app_browsers_path, "*.browser");
					}
#endif

					if (app_file != null) {
#if TARGET_J2EE
						app_file = System.Web.Util.UrlUtils.ResolveVirtualPathFromAppAbsolute("~/" + Path.GetFileName(app_file));
						app_type = System.Web.J2EE.PageMapper.GetObjectType(context, app_file);
#else
#if NET_2_0
						app_type = BuildManager.GetCompiledType ("~/" + Path.GetFileName (app_file));
#else
						app_type = ApplicationFileParser.GetCompiledApplicationType (app_file, context);
#endif
#endif
						if (app_type == null) {
							string msg = String.Format ("Error compiling application file ({0}).", app_file);
							throw new ApplicationException (msg);
						}
					} else {
						app_type = typeof (System.Web.HttpApplication);
						app_state = new HttpApplicationState ();
					}

					WatchLocationForRestart("Global.asax");
					WatchLocationForRestart("global.asax");
					WatchLocationForRestart("Web.config");
					WatchLocationForRestart("web.config");
					WatchLocationForRestart("Web.Config");
					needs_init = false;
#if NET_2_0
				} catch (Exception) {
					if (BuildManager.CodeAssemblies != null)
						BuildManager.CodeAssemblies.Clear ();
					if (BuildManager.TopLevelAssemblies != null)
						BuildManager.TopLevelAssemblies.Clear ();
					if (WebConfigurationManager.ExtraAssemblies != null)
						WebConfigurationManager.ExtraAssemblies.Clear ();
					throw;
				}
#endif
				
				//
				// Now init the settings
				//

			}
		}
		
		//
		// Multiple-threads might hit this one on startup, and we have
		// to delay-initialize until we have the HttpContext
		//
		internal static HttpApplication GetApplication (HttpContext context)
		{
#if TARGET_J2EE
			if (context.ApplicationInstance!=null)
				return context.ApplicationInstance;
#endif
			HttpApplicationFactory factory = theFactory;
			HttpApplication app = null;
			if (factory.app_start_needed){
				if (context == null)
					return null;

				factory.InitType (context);
				lock (factory) {
					if (factory.app_start_needed) {
						foreach (string dir in HttpApplication.BinDirs)
							WatchLocationForRestart (dir, "*.dll");
#if NET_2_0
									// Restart if the App_* directories are created...
			                        WatchLocationForRestart (".", "App_Code");
			                        WatchLocationForRestart (".", "App_Browsers");
			                        WatchLocationForRestart (".", "App_GlobalResources");
			                        // ...or their contents is changed.
			                        WatchLocationForRestart ("App_Code", "*", true);
			                        WatchLocationForRestart ("App_Browsers", "*");
			                        WatchLocationForRestart ("App_GlobalResources", "*");
#endif
			                        app = factory.FireOnAppStart (context);
						factory.app_start_needed = false;
						return app;
					}
				}
			}

			lock (factory.available) {
				if (factory.available.Count > 0) {
					app = (HttpApplication) factory.available.Pop ();
					app.RequestCompleted = false;
					return app;
				}
			}
			
			return (HttpApplication) Activator.CreateInstance (factory.app_type, true);
		}

		// The lock is in InvokeSessionEnd
		static HttpApplication GetApplicationForSessionEnd ()
		{
			HttpApplicationFactory factory = theFactory;
			if (factory.available_for_end.Count > 0)
				return (HttpApplication) factory.available_for_end.Pop ();

			HttpApplication app = (HttpApplication) Activator.CreateInstance (factory.app_type, true);
			app.InitOnce (false);

			return app;
		}

		internal static void RecycleForSessionEnd (HttpApplication app)
		{
			HttpApplicationFactory factory = theFactory;
			lock (factory.available_for_end) {
				if (factory.available_for_end.Count < 32)
					factory.available_for_end.Push (app);
				else
					app.Dispose ();
			}
		}

		internal static void Recycle (HttpApplication app)
		{
			HttpApplicationFactory factory = theFactory;
			lock (factory.available) {
				if (factory.available.Count < 32)
					factory.available.Push (app);
				else
					app.Dispose ();
			}
		}

		internal static bool ContextAvailable {
			get { return theFactory != null && !theFactory.app_start_needed; }
		}


                internal static bool WatchLocationForRestart (string filter)
	        {
			return WatchLocationForRestart ("", filter, false);
	        }

		internal static bool WatchLocationForRestart (string virtualPath, string filter)
		{
			return WatchLocationForRestart (virtualPath, filter, false);
		}
		
                internal static bool WatchLocationForRestart(string virtualPath, string filter, bool watchSubdirs)
		{
			// map the path to the physical one
			string physicalPath = HttpRuntime.AppDomainAppPath;
			physicalPath = Path.Combine(physicalPath, virtualPath);
			bool isDir = Directory.Exists(physicalPath);
			bool isFile = isDir ? false : File.Exists(physicalPath);
			
			if (isDir || isFile) {
				// create the watcher
				FileSystemEventHandler fseh = new FileSystemEventHandler(OnFileChanged);
				RenamedEventHandler reh = new RenamedEventHandler(OnFileRenamed);
				FileSystemWatcher watcher = CreateWatcher(Path.Combine(physicalPath, filter), fseh, reh);
				if (isDir)
					watcher.IncludeSubdirectories = watchSubdirs;
				
				lock (watchers_lock) {
					watchers.Add(watcher);
				}
				return true;
			} else {
				return false;
			}
	        }

#if NET_2_0
		internal static bool ApplicationDisabled {
			get { return app_disabled; }
			set { app_disabled = value; }
		}

		internal static string[] AppBrowsersFiles {
			get { return app_browsers_files; }
		}
		
		static System.Web.Configuration.nBrowser.Build capabilities_processor = null;
		static object capabilities_processor_lock = new object();
		internal static System.Web.Configuration.ICapabilitiesProcess CapabilitiesProcessor {
			get {
				lock (capabilities_processor_lock) {
					if (capabilities_processor == null) {
						capabilities_processor = new System.Web.Configuration.nBrowser.Build();
						string machine_browsers_path = Path.Combine (HttpRuntime.MachineConfigurationDirectory, "Browsers");
						if (Directory.Exists (machine_browsers_path)) {
							string[] machine_browsers_files 
								= Directory.GetFiles (machine_browsers_path, "*.browser");
							foreach (string f in machine_browsers_files) {
								capabilities_processor.AddBrowserFile(f);
							}
						}
						foreach (string f in app_browsers_files) {
							capabilities_processor.AddBrowserFile(f);
						}
					}
				}
				return capabilities_processor;
			}
		}
#endif
		
		internal static void DisableWatchers ()
		{
			lock (watchers_lock) {
				foreach (FileSystemWatcher watcher in watchers)
					watcher.EnableRaisingEvents = false;
			}
		}

		internal static void EnableWatchers ()
		{
			lock (watchers_lock) {
				foreach (FileSystemWatcher watcher in watchers)
					watcher.EnableRaisingEvents = true;
			}
		}
		
	        static void OnFileRenamed(object sender, RenamedEventArgs args)
		{
			OnFileChanged(sender, args);
	        }

	        static void OnFileChanged(object sender, FileSystemEventArgs args)
	        {
	        	lock (watchers_lock) {
				if(app_shutdown)
					return;
				app_shutdown = true;

				// Disable event raising to avoid concurrent restarts
				DisableWatchers ();
				
				// Restart application
				HttpRuntime.UnloadAppDomain();
			}
	        }
	}
}

