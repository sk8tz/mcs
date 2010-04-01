//
// SortedSet.cs
//
// Authors:
//  Jb Evain  <jbevain@novell.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Diagnostics;

// SortedSet is basically implemented as a reduction of SortedDictionary<K, V>

#if NET_4_0

namespace System.Collections.Generic {

	[Serializable]
	[DebuggerDisplay ("Count={Count}")]
	[DebuggerTypeProxy (typeof (CollectionDebuggerView))]
	public class SortedSet<T> : ISet<T>, ICollection, ISerializable, IDeserializationCallback
	{
		class Node : RBTree.Node {

			public T item;

			public Node (T item)
			{
				this.item = item;
			}

			public override void SwapValue (RBTree.Node other)
			{
				var o = (Node) other;
				var i = this.item;
				this.item = o.item;
				o.item = i;
			}
		}

		class NodeHelper : RBTree.INodeHelper<T> {

			static NodeHelper Default = new NodeHelper (Comparer<T>.Default);

			public IComparer<T> comparer;

			public int Compare (T item, RBTree.Node node)
			{
				return comparer.Compare (item, ((Node) node).item);
			}

			public RBTree.Node CreateNode (T item)
			{
				return new Node (item);
			}

			NodeHelper (IComparer<T> comparer)
			{
				this.comparer = comparer;
			}

			public static NodeHelper GetHelper (IComparer<T> comparer)
			{
				if (comparer == null || comparer == Comparer<T>.Default)
					return Default;

				return new NodeHelper (comparer);
			}
		}

		RBTree tree;
		NodeHelper helper;
		SerializationInfo si;

		public SortedSet ()
			: this (null as IComparer<T>)
		{
		}

		public SortedSet (IComparer<T> comparer)
		{
			this.helper = NodeHelper.GetHelper (comparer);
			this.tree = new RBTree (this.helper);
		}

		protected SortedSet (SerializationInfo info, StreamingContext context)
		{
			this.si = info;
		}

		public IComparer<T> Comparer {
			get { return helper.comparer; }
		}

		public int Count {
			get { return (int) tree.Count; }
		}

		[MonoTODO]
		public T Max {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public T Min {
			get { throw new NotImplementedException (); }
		}

		public virtual bool Add (T item)
		{
			var node = new Node (item);
			return tree.Intern (item, node) == node;
		}

		public virtual void Clear ()
		{
			tree.Clear ();
		}

		public virtual bool Contains (T item)
		{
			return tree.Lookup (item) != null;
		}

		public void CopyTo (T [] array)
		{
			CopyTo (array, 0, Count);
		}

		public virtual void CopyTo (T [] array, int index)
		{
			CopyTo (array, index, Count);
		}

		public void CopyTo (T [] array, int index, int count)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index");
			if (index > array.Length)
				throw new ArgumentException ("index larger than largest valid index of array");
			if (array.Length - index < count)
				throw new ArgumentException ("destination array cannot hold the requested elements");

			foreach (Node node in tree) {
				if (count-- == 0)
					break;

				array [index++] = node.item;
			}
		}

		public virtual bool Remove (T item)
		{
			return tree.Remove (item) != null;
		}

		[MonoTODO]
		public int RemoveWhere (Predicate<T> match)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IEnumerable<T> Reverse ()
		{
			throw new NotImplementedException ();
		}

		public Enumerator GetEnumerator ()
		{
			return new Enumerator (this);
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator ()
		{
			return new Enumerator (this);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new Enumerator (this);
		}

		public static IEqualityComparer<SortedSet<T>> CreateSetComparer ()
		{
			return CreateSetComparer (EqualityComparer<T>.Default);
		}

		[MonoTODO]
		public static IEqualityComparer<SortedSet<T>> CreateSetComparer (IEqualityComparer<T> memberEqualityComparer)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}

		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			GetObjectData (info, context);
		}

		[MonoTODO]
		protected virtual void OnDeserialization (object sender)
		{
			if (si == null)
				return;

			throw new NotImplementedException ();
		}

		void IDeserializationCallback.OnDeserialization (object sender)
		{
			OnDeserialization (sender);
		}

		[MonoTODO]
		public virtual void ExceptWith (IEnumerable<T> other)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void GetViewBetween (T lowerValue, T upperValue)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void IntersectWith (IEnumerable<T> other)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool IsProperSubsetOf (IEnumerable<T> other)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool IsProperSupersetOf (IEnumerable<T> other)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool IsSubsetOf (IEnumerable<T> other)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool IsSupersetOf (IEnumerable<T> other)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool IsPropertSubsetOf (IEnumerable<T> other)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool Overlaps (IEnumerable<T> other)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual bool SetEquals (IEnumerable<T> other)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void SymmetricExceptWith (IEnumerable<T> other)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void UnionWith (IEnumerable<T> other)
		{
			throw new NotImplementedException ();
		}

		void ICollection<T>.Add (T item)
		{
			Add (item);
		}

		bool ICollection<T>.IsReadOnly {
			get { return false; }
		}

		void ICollection.CopyTo (Array array, int index)
		{
			if (Count == 0)
				return;
			if (array == null)
				throw new ArgumentNullException ("array");
			if (index < 0 || array.Length <= index)
				throw new ArgumentOutOfRangeException ("index");
			if (array.Length - index < Count)
				throw new ArgumentException ();

			foreach (Node node in tree)
				array.SetValue (node.item, index++);
		}

		bool ICollection.IsSynchronized {
			get { return false; }
		}

		// TODO:Is this correct? If this is wrong,please fix.
		object ICollection.SyncRoot {
			get { return this; }
		}

		[Serializable]
		public struct Enumerator : IEnumerator<T>, IDisposable {

			RBTree.NodeEnumerator host;

			T current;

			internal Enumerator (SortedSet<T> set)
			{
				host = set.tree.GetEnumerator ();
				current = default (T);
			}

			public T Current {
				get { return current; }
			}

			object IEnumerator.Current {
				get {
					host.check_current ();
					return ((Node) host.Current).item;
				}
			}

			public bool MoveNext ()
			{
				if (!host.MoveNext ())
					return false;

				current = ((Node) host.Current).item;
				return true;
			}

			public void Dispose ()
			{
				host.Dispose ();
			}

			void IEnumerator.Reset ()
			{
				host.Reset ();
			}
		}
	}
}

#endif
