// 
// System.Web.Caching.CacheDependency
//
// Authors:
// 	Patrik Torstensson (Patrik.Torstensson@labs2.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) Copyright Patrik Torstensson, 2001
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
//
using System;
using System.Web;

namespace System.Web.Caching
{
	internal class CacheDependencyChangedArgs : EventArgs
	{
		string key;

		public CacheDependencyChangedArgs (string key)
		{
			this.key = key;
		}

		public string Key {
			get { return key; }
		}
	}

	internal delegate void CacheDependencyChangedHandler (object sender, CacheDependencyChangedArgs args);

	public sealed class CacheDependency : IDisposable
	{
		DateTime start;
		string [] filenames;

		public CacheDependency (string filename)
			: this (filename, DateTime.MaxValue)
		{
		}

		public CacheDependency (string filename, DateTime start)
			: this (new string [] {filename}, null, null, start)
		{
		}

		public CacheDependency (string [] filenames)
			: this (filenames, null, null, DateTime.MaxValue)
		{
		}

		public CacheDependency (string [] filenames, DateTime start)
			: this (filenames, null, null, start)
		{
		}

		public CacheDependency (string [] filenames, string [] cachekeys)
			: this (filenames, cachekeys, null, DateTime.MaxValue)
		{
		}

		public CacheDependency (string [] filenames, string [] cachekeys, DateTime start)
			: this (filenames, cachekeys, null, start)
		{
		}

		public CacheDependency (string [] filenames, string [] cachekeys, CacheDependency dependency)
			: this (filenames, cachekeys, dependency, DateTime.MaxValue)
		{
		}

		[MonoTODO]
		public CacheDependency (string [] filenames,
					string [] cachekeys,
					CacheDependency dependency,
					DateTime start)
		{
			this.start = start;
		}

		[MonoTODO]
		public void Dispose ()
		{
			throw new NotImplementedException ();
		}


		public bool HasChanged
		{
			get {
				return false;
			}
		}

		[MonoTODO]
		internal CacheEntry [] GetCacheEntries ()
		{
			return null;
		}

		internal event CacheDependencyChangedHandler Changed;
	}
}

