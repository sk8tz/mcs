//------------------------------------------------------------------------------
// 
// System.IO.FileSystemInfo.cs 
//
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
// 
// Author:         Jim Richardson, develop@wtfo-guru.com
//                 Dan Lewis (dihlewis@yahoo.co.uk)
// Created:        Monday, August 13, 2001 
//
//------------------------------------------------------------------------------

using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.IO {
	
	[Serializable]
	public abstract class FileSystemInfo : MarshalByRefObject, ISerializable {
		#region Implementation of ISerializable

		[ComVisible(false)]
		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			info.AddValue ("OriginalPath", OriginalPath, typeof(string));
			info.AddValue ("FullPath", FullPath, typeof(string));
		}

		#endregion Implementation of ISerializable

		// public properties

		public abstract bool Exists { get; }

		public abstract string Name { get; }

		public virtual string FullName {
			get {
				return FullPath;
			}
		}

		public string Extension {
			get {
				return Path.GetExtension (Name);
			}
		}

		public FileAttributes Attributes {
			get {
				Refresh (false);
				return stat.Attributes;
			}

			set {
				MonoIOError error;
				
				if (!MonoIO.SetFileAttributes (FullName,
							       value,
							       out error))
					throw MonoIO.GetException (FullName,
								   error);
				Refresh (true);
			}
		}

		public DateTime CreationTime {
			get {
				Refresh (false);
				return DateTime.FromFileTime (stat.CreationTime);
			}

			set {
				long filetime = value.ToFileTime ();
			
				MonoIOError error;
				
				if (!MonoIO.SetFileTime (FullName, filetime,
							 -1, -1, out error))
					throw MonoIO.GetException (FullName,
								   error);
				Refresh (true);
			}
		}

		[ComVisible(false)]
		public DateTime CreationTimeUtc {
			get {
				return CreationTime.ToUniversalTime ();
			}

			set {
				CreationTime = value.ToLocalTime ();
			}
		}

		public DateTime LastAccessTime {
			get {
				Refresh (false);
				return DateTime.FromFileTime (stat.LastAccessTime);
			}

			set {
				long filetime = value.ToFileTime ();

				MonoIOError error;
				
				if (!MonoIO.SetFileTime (FullName, -1,
							 filetime, -1,
							 out error))
					throw MonoIO.GetException (FullName,
								   error);
				Refresh (true);
			}
		}

		[ComVisible(false)]
		public DateTime LastAccessTimeUtc {
			get {
				Refresh (false);
				return LastAccessTime.ToUniversalTime ();
			}

			set {
				LastAccessTime = value.ToLocalTime ();
			}
		}

		public DateTime LastWriteTime {
			get {
				Refresh (false);
				return DateTime.FromFileTime (stat.LastWriteTime);
			}

			set {
				long filetime = value.ToFileTime ();

				MonoIOError error;
				
				if (!MonoIO.SetFileTime (FullName, -1, -1,
							 filetime, out error))
					throw MonoIO.GetException (FullName,
								   error);
				Refresh (true);
			}
		}

		[ComVisible(false)]
		public DateTime LastWriteTimeUtc {
			get {
				Refresh (false);
				return LastWriteTime.ToUniversalTime ();
			}

			set {
				LastWriteTime = value.ToLocalTime ();
			}
		}

		// public methods

		public abstract void Delete ();

		public void Refresh ()
		{
			Refresh (true);
		}

		// protected

		protected FileSystemInfo ()
		{
			this.valid = false;
			this.FullPath = null;
		}

		protected FileSystemInfo (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}

			FullPath = info.GetString("FullPath");
			OriginalPath = info.GetString("OriginalPath");
		}

		protected string FullPath;
		protected string OriginalPath;

		// internal

		internal void Refresh (bool force)
		{
			if (valid && !force)
				return;

			MonoIOError error;
			
			MonoIO.GetFileStat (FullName, out stat, out error);
			if (error != MonoIOError.ERROR_SUCCESS) {
				throw MonoIO.GetException (FullName,
							   error);
			}
			
			valid = true;
			
			InternalRefresh ();
		}
		
		internal virtual void InternalRefresh ()
		{
		}

		internal void CheckPath (string path)
		{
			if (path == null)
				throw new ArgumentNullException ();
			if (path.IndexOfAny (Path.InvalidPathChars) != -1)
				throw new ArgumentException ("Invalid characters in path.");
		}

		internal MonoIOStat stat;
		internal bool valid;
	}
}
