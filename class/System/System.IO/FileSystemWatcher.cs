// 
// System.IO.FileSystemWatcher.cs
//
// Authors:
// 	Tim Coleman (tim@timcoleman.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// Copyright (C) Tim Coleman, 2002 
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
// (c) 2004 Novell, Inc. (http://www.novell.com)
//

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.IO {
	[DefaultEvent("Changed")]
	public class FileSystemWatcher : Component, ISupportInitialize {

		#region Fields

		bool enableRaisingEvents;
		string filter;
		bool includeSubdirectories;
		int internalBufferSize;
		NotifyFilters notifyFilter;
		string path;
		string fullpath;
		ISynchronizeInvoke synchronizingObject;
		bool disposed;
		WaitForChangedResult lastData;
		bool waiting;
		SearchPattern2 pattern;
		static IFileWatcher watcher;

		#endregion // Fields

		#region Constructors

		public FileSystemWatcher ()
		{
			this.notifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
			this.enableRaisingEvents = false;
			this.filter = "*.*";
			this.includeSubdirectories = false;
			this.internalBufferSize = 8192;
			this.path = "";
			InitWatcher ();
		}

		public FileSystemWatcher (string path)
			: this (path, "*.*")
		{
		}

		public FileSystemWatcher (string path, string filter)
		{
			if (path == null)
				throw new ArgumentNullException ("path");

			if (filter == null)
				throw new ArgumentNullException ("filter");

			if (path == String.Empty)
				throw new ArgumentException ("Empty path", "path");

			if (!Directory.Exists (path))
				throw new ArgumentException ("Directory does not exists", "path");

			this.enableRaisingEvents = false;
			this.filter = filter;
			this.includeSubdirectories = false;
			this.internalBufferSize = 8192;
			this.notifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
			this.path = path;
			this.synchronizingObject = null;
			InitWatcher ();
		}

		void InitWatcher ()
		{
			lock (typeof (FileSystemWatcher)) {
				if (watcher != null)
					return;

				string managed = Environment.GetEnvironmentVariable ("MONO_MANAGED_WATCHER");
				int mode = 0;
				if (managed == null)
					mode = InternalSupportsFSW ();

				bool ok = false;
				if (mode == 2)
					ok = FAMWatcher.GetInstance (out watcher);
				else if (mode == 1)
					ok = DefaultWatcher.GetInstance (out watcher);
					//ok = WindowsWatcher.GetInstance (out watcher);

				if (mode == 0 || !ok)
					DefaultWatcher.GetInstance (out watcher);
			}
		}

		#endregion // Constructors

		#region Properties

		/* If this is enabled, we Pulse this instance */
		internal bool Waiting {
			get { return waiting; }
			set { waiting = value; }
		}

		internal SearchPattern2 Pattern {
			get {
				if (pattern == null) {
					string f = Filter;
					if (f == "*.*" && !(watcher.GetType () == typeof (WindowsWatcher)))
						f = "*";

					pattern = new SearchPattern2 (f);
				}
				return pattern;
			}
		}

		internal string FullPath {
			get {
				if (fullpath == null) {
					if (path == null || path == "")
						fullpath = Environment.CurrentDirectory;
					else
						fullpath = System.IO.Path.GetFullPath (path);
				}

				return fullpath;
			}
		}

		[DefaultValue(false)]
		[IODescription("Flag to indicate if this instance is active")]
		public bool EnableRaisingEvents {
			get { return enableRaisingEvents; }
			set {
				if (value == enableRaisingEvents)
					return; // Do nothing

				enableRaisingEvents = value;
				if (value) {
					Start ();
				} else {
					Stop ();
				}
			}
		}

		[DefaultValue("*.*")]
		[IODescription("File name filter pattern")]
		[RecommendedAsConfigurable(true)]
		[TypeConverter ("System.Diagnostics.Design.StringValueConverter, " + Consts.AssemblySystem_Design)]
		public string Filter {
			get { return filter; }
			set {
				if (value == null || value == "")
					value = "*.*";

				if (filter != value) {
					filter = value;
					pattern = null;
				}
			}
		}

		[DefaultValue(false)]
		[IODescription("Flag to indicate we want to watch subdirectories")]
		public bool IncludeSubdirectories {
			get { return includeSubdirectories; }
			set {
				if (includeSubdirectories == value)
					return;

				includeSubdirectories = value;
				if (value && enableRaisingEvents) {
					Stop ();
					Start ();
				}
			}
		}

		[Browsable(false)]
		[DefaultValue(8192)]
		public int InternalBufferSize {
			get { return internalBufferSize; }
			set {
				if (internalBufferSize == value)
					return;

				if (value < 4196)
					value = 4196;

				internalBufferSize = value;
				if (enableRaisingEvents) {
					Stop ();
					Start ();
				}
			}
		}

		[DefaultValue(NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite)]
		[IODescription("Flag to indicate which change event we want to monitor")]
		public NotifyFilters NotifyFilter {
			get { return notifyFilter; }
			set {
				if (notifyFilter == value)
					return;
					
				notifyFilter = value;
				if (enableRaisingEvents) {
					Stop ();
					Start ();
				}
			}
		}

		[DefaultValue("")]
		[IODescription("The directory to monitor")]
		[RecommendedAsConfigurable(true)]
		[TypeConverter ("System.Diagnostics.Design.StringValueConverter, " + Consts.AssemblySystem_Design)]
		[Editor ("System.Diagnostics.Design.FSWPathEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public string Path {
			get { return path; }
			set {
				if (path == value)
					return;

				bool exists = false;
				Exception exc = null;

				try {
					exists = Directory.Exists (value);
				} catch (Exception e) {
					exc = e;
				}

				if (exc != null)
					throw new ArgumentException ("Invalid directory name", "value", exc);

				if (!exists)
					throw new ArgumentException ("Directory does not exists", "value");

				path = value;
				fullpath = null;
				if (enableRaisingEvents) {
					Stop ();
					Start ();
				}
			}
		}

		[Browsable(false)]
		public override ISite Site {
			get { return base.Site; }
			set { base.Site = value; }
		}

		[DefaultValue(null)]
		[IODescription("The object used to marshal the event handler calls resulting from a directory change")]
		public ISynchronizeInvoke SynchronizingObject {
			get { return synchronizingObject; }
			set { synchronizingObject = value; }
		}

		#endregion // Properties

		#region Methods
	
		[MonoTODO]
		public void BeginInit ()
		{
			throw new NotImplementedException (); 
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				Stop ();
			}
			disposed = true;
			base.Dispose (disposing);
		}

		[MonoTODO]
		public void EndInit ()
		{
			throw new NotImplementedException (); 
		}

		private void RaiseEvent (Delegate ev, EventArgs arg)
		{
			if (ev == null)
				return;

			object [] args = new object [] {this, arg};

			if (synchronizingObject == null) {
				ev.DynamicInvoke (args);
				return;
			}
			
			synchronizingObject.BeginInvoke (ev, args);
		}

		protected void OnChanged (FileSystemEventArgs e)
		{
			RaiseEvent (Changed, e);
		}

		protected void OnCreated (FileSystemEventArgs e)
		{
			RaiseEvent (Created, e);
		}

		protected void OnDeleted (FileSystemEventArgs e)
		{
			RaiseEvent (Deleted, e);
		}

		protected void OnError (ErrorEventArgs e)
		{
			RaiseEvent (Error, e);
		}

		protected void OnRenamed (RenamedEventArgs e)
		{
			RaiseEvent (Renamed, e);
		}

		public WaitForChangedResult WaitForChanged (WatcherChangeTypes changeType)
		{
			return WaitForChanged (changeType, Timeout.Infinite);
		}

		public WaitForChangedResult WaitForChanged (WatcherChangeTypes changeType, int timeout)
		{
			WaitForChangedResult result = new WaitForChangedResult ();
			bool prevEnabled = EnableRaisingEvents;
			if (!prevEnabled)
				EnableRaisingEvents = true;

			bool gotData;
			lock (this) {
				waiting = true;
				gotData = Monitor.Wait (this, timeout);
				if (gotData)
					result = this.lastData;
			}

			EnableRaisingEvents = prevEnabled;
			if (!gotData)
				result.TimedOut = true;

			return result;
		}

		internal void DispatchEvents (FileAction act, string filename, ref RenamedEventArgs renamed)
		{
			if (waiting) {
				lastData = new WaitForChangedResult ();
			}

			switch (act) {
			case FileAction.Added:
				lastData.Name = filename;
				lastData.ChangeType = WatcherChangeTypes.Created;
				OnCreated (new FileSystemEventArgs (WatcherChangeTypes.Created, path, filename));
				break;
			case FileAction.Removed:
				lastData.Name = filename;
				lastData.ChangeType = WatcherChangeTypes.Deleted;
				OnDeleted (new FileSystemEventArgs (WatcherChangeTypes.Deleted, path, filename));
				break;
			case FileAction.Modified:
				lastData.Name = filename;
				lastData.ChangeType = WatcherChangeTypes.Changed;
				OnChanged (new FileSystemEventArgs (WatcherChangeTypes.Changed, path, filename));
				break;
			case FileAction.RenamedOldName:
				if (renamed != null) {
					OnRenamed (renamed);
				}
				lastData.OldName = filename;
				lastData.ChangeType = WatcherChangeTypes.Renamed;
				renamed = new RenamedEventArgs (WatcherChangeTypes.Renamed, path, null, filename);
				break;
			case FileAction.RenamedNewName:
				lastData.Name = filename;
				lastData.ChangeType = WatcherChangeTypes.Renamed;
				if (renamed != null) {
					renamed.SetName (filename);
				} else {
					renamed = new RenamedEventArgs (WatcherChangeTypes.Renamed, path, filename, null);
				}
				OnRenamed (renamed);
				renamed = null;
				break;
			default:
				break;
			}
		}

		void Start ()
		{
			watcher.StartDispatching (this);
		}

		void Stop ()
		{
			watcher.StopDispatching (this);
		}
		#endregion // Methods

		#region Events and Delegates

		[IODescription("Occurs when a file/directory change matches the filter")]
		public event FileSystemEventHandler Changed;

		[IODescription("Occurs when a file/directory creation matches the filter")]
		public event FileSystemEventHandler Created;

		[IODescription("Occurs when a file/directory deletion matches the filter")]
		public event FileSystemEventHandler Deleted;

		[Browsable(false)]
		public event ErrorEventHandler Error;

		[IODescription("Occurs when a file/directory rename matches the filter")]
		public event RenamedEventHandler Renamed;

		#endregion // Events and Delegates

		/* 0 -> not supported	*/
		/* 1 -> windows		*/
		/* 2 -> FAM		*/
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern int InternalSupportsFSW ();
		
		/*[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern IntPtr InternalOpenDirectory (string path, IntPtr reserved);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern IntPtr InternalCloseDirectory (IntPtr handle);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static extern bool InternalReadDirectoryChanges (IntPtr handle,
								 byte [] buffer,
								 bool includeSubdirectories,
								 NotifyFilters notifyFilter,
								 out NativeOverlapped overlap,
								 OverlappedHandler overlappedCallback);

		*/
	}
}
