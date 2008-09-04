//
// HttpSessionStateBase.cs
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
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.SessionState;

namespace System.Web
{
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public abstract class HttpSessionStateBase : ICollection, IEnumerable
	{
		[MonoTODO]
		public virtual int CodePage { get; set; }
		[MonoTODO]
		public virtual HttpSessionStateBase Contents { get; private set; }
		[MonoTODO]
		public virtual HttpCookieMode CookieMode { get; private set; }
		[MonoTODO]
		public virtual int Count { get; private set; }
		[MonoTODO]
		public virtual bool IsCookieless { get; private set; }
		[MonoTODO]
		public virtual bool IsNewSession { get; private set; }
		[MonoTODO]
		public virtual bool IsReadOnly { get; private set; }
		[MonoTODO]
		public virtual bool IsSynchronized { get; private set; }
		[MonoTODO]
		public virtual object this [int index] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public virtual object this [string name] {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public virtual NameObjectCollectionBase.KeysCollection Keys { get; private set; }
		[MonoTODO]
		public virtual int LCID { get; set; }
		[MonoTODO]
		public virtual SessionStateMode Mode { get; private set; }
		[MonoTODO]
		public virtual string SessionID { get; private set; }
		[MonoTODO]
		public virtual HttpStaticObjectsCollectionBase StaticObjects { get; private set; }
		[MonoTODO]
		public virtual object SyncRoot { get; private set; }
		[MonoTODO]
		public virtual int Timeout { get; set; }

		[MonoTODO]
		public virtual void Abandon ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Add (string name, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Clear ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void CopyTo (Array array, int index)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual IEnumerator GetEnumerator ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void Remove (string name)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void RemoveAll ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void RemoveAt (int index)
		{
			throw new NotImplementedException ();
		}
	}
}
