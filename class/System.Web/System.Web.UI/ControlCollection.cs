//
// System.Web.UI.ControlCollection.cs
//
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;
using System.Collections;

namespace System.Web.UI {

	public class ControlCollection : ICollection, IEnumerable
	{
		ArrayList list = new ArrayList ();
		Control owner;
		
		public ControlCollection (Control owner)
		{
			if (owner == null)
				throw new ArgumentException ();

			this.owner = owner;
		}

		public int Count {
			get { return list.Count; }
		}

		public bool IsReadOnly {
			get { return list.IsReadOnly; }
		}

		public bool  IsSynchronized {
			get { return list.IsSynchronized; }
		}

		public virtual Control this [int index] {
			get { return list [index] as Control; }
		}

		protected Control Owner {
			get { return owner; }
		}

		public object SyncRoot {
			get { return list.SyncRoot; }
		}

		public virtual void Add (Control child)
		{
			if (child == null)
				throw new ArgumentNullException ();
			if (IsReadOnly)
				throw new HttpException ();

			list.Add (child);
		}

		public virtual void AddAt (int index, Control child)
		{
			if (child == null) // maybe we should check for ! (child is Control)?
				throw new ArgumentNullException ();
			
			if ((index < 0) || (index > Count))
				throw new ArgumentOutOfRangeException ();

			if (IsReadOnly)
				throw new HttpException ();

			list [index] = child;
		}

		public virtual void Clear ()
		{
			list.Clear ();
		}

		public virtual bool Contains (Control c)
		{
			return list.Contains (c as object);
		}

		public void CopyTo (Array array, int index)
		{
			list.CopyTo (array, index);
		}

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		public virtual int IndexOf (Control c)
		{
			return list.IndexOf (c as object);
		}

		public virtual void Remove (Control value)
		{
			list.Remove (value as object);
		}

		public virtual void RemoveAt (int index)
		{
			if (IsReadOnly)
				throw new HttpException ();

			list.RemoveAt (index);
		}
	}
}
