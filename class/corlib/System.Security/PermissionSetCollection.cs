//
// System.Security.PermissionSetCollection class
//
// Authors
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.Collections;
using System.Security.Permissions;

namespace System.Security {

	[Serializable]
	public sealed class PermissionSetCollection : ICollection, IEnumerable {

		private IList _list;

		public PermissionSetCollection ()
		{
			_list = (IList) new ArrayList ();
		}

		// properties

		public int Count {
			get { return _list.Count; }
		}

		public bool IsSynchronized {
			get { return false; }
		}

		public IList PermissionSets {
			get { return _list; }
			set { _list = value; }
		}

		public object SyncRoot {
			// this is the "real thing"
			get { throw new NotSupportedException (); }
		}

		// methods

		public void Add (PermissionSet permSet)
		{
			if (permSet == null)
				throw new ArgumentNullException ("permSet");
			_list.Add (permSet);
		}

		[MonoTODO ("check if permission are copied (or just referenced)")]
		public PermissionSetCollection Copy ()
		{
			PermissionSetCollection psc = new PermissionSetCollection ();
			foreach (PermissionSet ps in _list) {
				psc._list.Add (ps);
			}
			return psc;
		}

		public void CopyTo (PermissionSet[] array, int index)
		{
			// this is the "real thing"
			throw new NotSupportedException ();
		}

		void ICollection.CopyTo (Array array, int index)
		{
			// this is the "real thing"
			throw new NotSupportedException ();
		}

		[MonoTODO]
		public void Demand ()
		{
			// check all collection in a single stack walk
		}

		[MonoTODO]
		public void FromXml (SecurityElement el) 
		{
			if (el == null)
				throw new ArgumentNullException ("el");
			// TODO
		}

		public IEnumerator GetEnumerator ()
		{
			return _list.GetEnumerator ();
		}

		public PermissionSet GetSet (int index) 
		{
			return (PermissionSet) _list [index];
		}

		public void RemoveSet (int index) 
		{
			_list.Remove (index);
		}

		public override string ToString ()
		{
			return ToXml ().ToString ();
		}

		[MonoTODO ("verify syntax")]
		public SecurityElement ToXml ()
		{
			SecurityElement se = new SecurityElement ("PermissionSetCollection");
			foreach (PermissionSet ps in _list) {
				se.AddChild (ps.ToXml ());
			}
			return se;
		}
	}
}

#endif
