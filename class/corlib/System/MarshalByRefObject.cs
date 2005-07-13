//
// System.MarshalByRefObject.cs
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Lluis Sanchez Gual (lsg@ctv.es)
//   Patrik Torstensson (totte_mono@yahoo.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Threading;
using System.Runtime.Remoting;
using System.Security.Permissions;

namespace System
{
	[Serializable]
	public abstract class MarshalByRefObject
	{
		[NonSerialized]
		private ServerIdentity _identity; // Holds marshalling iformation of the object

		protected MarshalByRefObject ()
		{
		}

		internal Identity GetObjectIdentity (MarshalByRefObject obj, out bool IsClient)
		{
			IsClient = false;
			Identity objId = null;

			if (RemotingServices.IsTransparentProxy (obj)) {
				objId = RemotingServices.GetRealProxy (obj).ObjectIdentity;
				IsClient = true;
			}
			else {
				objId = obj.ObjectIdentity;
			}

			return objId;
		}

		internal ServerIdentity ObjectIdentity {
			get { return _identity; }
			set { _identity = value; }
		}

		[SecurityPermission (SecurityAction.LinkDemand, Infrastructure = true)]
		public virtual ObjRef CreateObjRef (Type type)
		{
			// This method can only be called when this object has been marshalled
			if (_identity == null)
				throw new RemotingException (Locale.GetText ("No remoting information was found for the object."));
			return _identity.CreateObjRef (type);
		}

		[SecurityPermission (SecurityAction.LinkDemand, Infrastructure = true)]
		public virtual object GetLifetimeService ()
		{
			if (_identity == null)
				return null;
			else return _identity.Lease;
		}

		[SecurityPermission (SecurityAction.LinkDemand, Infrastructure = true)]
		public virtual object InitializeLifetimeService ()
		{
			return new System.Runtime.Remoting.Lifetime.Lease();
		}
	}
}
