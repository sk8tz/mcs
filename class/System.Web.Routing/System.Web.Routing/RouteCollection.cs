//
// RouteCollection.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell Inc. http://novell.com
//

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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Permissions;
using System.Web;
using System.Web.Hosting;

namespace System.Web.Routing
{
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class RouteCollection : Collection<RouteBase>
	{
		class Lock : IDisposable
		{
			RouteCollection owner;
			bool read;

			public Lock (RouteCollection owner, bool read)
			{
				this.owner = owner;
				this.read = read;
			}

			public void Dispose ()
			{
				//if (read)
				//	owner.read_lock = null;
				//else
				//	owner_write_lock = null;
			}
		}

		public RouteCollection ()
			: this (null)
		{
		}

		public RouteCollection (VirtualPathProvider virtualPathProvider)
		{
			// null argument is allowed
			provider = virtualPathProvider;

			read_lock = new Lock (this, true);
			write_lock = new Lock (this, false);
		}

		VirtualPathProvider provider;
		Dictionary<string,RouteBase> d = new Dictionary<string,RouteBase> ();

		Lock read_lock, write_lock;

		public RouteBase this [string name] {
			get {
				foreach (var p in d)
					if (p.Key == name)
						return p.Value;
				return null;
			}
		}

		[MonoTODO]
		public bool RouteExistingFiles { get; set; }

		public void Add (string name, RouteBase item)
		{
			lock (GetWriteLock ()) {
				base.Add (item);
				if (!String.IsNullOrEmpty (name))
					d.Add (name, item);
			}
		}

		protected override void ClearItems ()
		{
			lock (GetWriteLock ())
				base.ClearItems ();
		}

		public IDisposable GetReadLock ()
		{
			return read_lock;
		}

		[MonoTODO]
		public RouteData GetRouteData (HttpContextBase httpContext)
		{
			if (httpContext == null)
				throw new ArgumentNullException ("httpContext");

			var path = httpContext.Request.AppRelativeCurrentExecutionFilePath;
			// FIXME: do some check wrt the property above.

			if (Count == 0)
				return null;

			foreach (RouteBase rb in this) {
				var rd = rb.GetRouteData (httpContext);
				if (rd != null)
					return rd;
			}

			return null;
		}

		[MonoTODO]
		public VirtualPathData GetVirtualPath (RequestContext requestContext, RouteValueDictionary values)
		{
			return GetVirtualPath (requestContext, null, values);
		}

		[MonoTODO]
		public VirtualPathData GetVirtualPath (RequestContext requestContext, string name, RouteValueDictionary values)
		{
			if (requestContext == null)
				throw new ArgumentNullException ("httpContext");

			if (Count == 0)
				return null;

			foreach (RouteBase rb in this) {
				var vp = rb.GetVirtualPath (requestContext, values);
				if (vp != null)
					return vp;
			}

			return null;
		}

		public IDisposable GetWriteLock ()
		{
			return write_lock;
		}

		protected override void InsertItem (int index, RouteBase item)
		{
			// FIXME: what happens wrt its name?
			lock (GetWriteLock ())
				base.InsertItem (index, item);
		}

		protected override void RemoveItem (int index)
		{
			// FIXME: what happens wrt its name?
			lock (GetWriteLock ()) {
				string k = GetKey (index);
				base.RemoveItem (index);
				if (k != null)
					d.Remove (k);
			}
		}

		protected override void SetItem (int index, RouteBase item)
		{
			// FIXME: what happens wrt its name?
			lock (GetWriteLock ()) {
				string k = GetKey (index);
				base.SetItem (index, item);
				if (k != null)
					d.Remove (k);
			}
		}

		string GetKey (int index)
		{
			var item = this [index];
			foreach (var p in d)
				if (p.Value == item)
					return p.Key;
			return null;
		}
	}
}
