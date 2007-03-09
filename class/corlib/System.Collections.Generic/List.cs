//
// System.Collections.Generic.List
//
// Authors:
//    Ben Maurer (bmaurer@ximian.com)
//    Martin Baulig (martin@ximian.com)
//    Carlos Alberto Cortez (calberto.cortez@gmail.com)
//    David Waite (mass@akuma.org)
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
// Copyright (C) 2005 David Waite
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

using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace System.Collections.Generic {
	[Serializable]
	public class List <T> : IList <T>, IList, ICollection {
		T [] _items;
		int _size;
		int _version;
		
		static readonly T [] EmptyArray = new T [0]; 
		const int DefaultCapacity = 4;
		
		public List ()
		{
			_items = EmptyArray;
		}
		
		public List (IEnumerable <T> collection)
		{
			CheckCollection (collection);

			// initialize to needed size (if determinable)
			ICollection <T> c = collection as ICollection <T>;
			if (c == null)
			{
				_items = EmptyArray;
				AddEnumerable (collection);
			}
			else
			{
				_items = new T [c.Count];
				AddCollection (c);
			}
		}
		
		public List (int capacity)
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException ("capacity");
			_items = new T [capacity];
		}
		
		internal List (T [] data, int size)
		{
			_items = data;
			_size = size;
		}
		public void Add (T item)
		{
			GrowIfNeeded (1);
			_items [_size ++] = item;
			_version++;
		}
		
		void GrowIfNeeded (int newCount)
		{
			int minimumSize = _size + newCount;
			if (minimumSize > _items.Length)
				Capacity = Math.Max (Math.Max (Capacity * 2, DefaultCapacity), minimumSize);
		}
		
		void CheckRange (int idx, int count)
		{
			if (idx < 0)
				throw new ArgumentOutOfRangeException ("index");
			
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count");

			if ((uint) idx + (uint) count > (uint) _size)
				throw new ArgumentException ("index and count exceed length of list");
		}
		
		void AddCollection (ICollection <T> collection)
		{
			int collectionCount = collection.Count;
			GrowIfNeeded (collectionCount);			 
			collection.CopyTo (_items, _size);
			_size += collectionCount;
		}
		void AddEnumerable (IEnumerable <T> enumerable)
		{
			foreach (T t in enumerable)
			{
				Add (t);
			}
		}

		public void AddRange (IEnumerable <T> collection)
		{
			CheckCollection (collection);
			
			ICollection <T> c = collection as ICollection <T>;
			if (c != null)
				AddCollection (c);
			else
				AddEnumerable (collection);
			_version++;
		}
		
		public ReadOnlyCollection <T> AsReadOnly ()
		{
			return new ReadOnlyCollection <T> (this);
		}
		
		public int BinarySearch (T item)
		{
			return Array.BinarySearch <T> (_items, 0, _size, item);
		}
		
		public int BinarySearch (T item, IComparer <T> comparer)
		{
			return Array.BinarySearch <T> (_items, 0, _size, item, comparer);
		}
		
		public int BinarySearch (int index, int count, T item, IComparer <T> comparer)
		{
			CheckRange (index, count);
			return Array.BinarySearch <T> (_items, index, count, item, comparer);
		}
		
		public void Clear ()
		{
			Array.Clear (_items, 0, _items.Length);
			_size = 0;
			_version++;
		}
		
		public bool Contains (T item)
		{
			EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
			for (int i = 0; i < _size; i++)
				if (equalityComparer.Equals (_items[i], item))
					return true;
			return false;
		}
		
		public List <TOutput> ConvertAll <TOutput> (Converter <T, TOutput> converter)
		{
			if (converter == null)
				throw new ArgumentNullException ("converter");
			List <TOutput> u = new List <TOutput> (_size);
			foreach (T t in this)
				u.Add (converter (t));
			return u;
		}
		
		public void CopyTo (T [] array)
		{
			Array.Copy (_items, 0, array, 0, _size);
		}
		
		public void CopyTo (T [] array, int arrayIndex)
		{
			Array.Copy (_items, 0, array, arrayIndex, _size);
		}
		
		public void CopyTo (int index, T [] array, int arrayIndex, int count)
		{
			CheckRange (index, count);
			Array.Copy (_items, index, array, arrayIndex, count);
		}

		public bool Exists (Predicate <T> match)
		{
			return FindIndex (match) != -1;
		}
		
		public T Find (Predicate <T> match)
		{
			int i = FindIndex (match);
			return (i != -1) ? _items [i] : default (T);
		}
		void CheckMatch (Predicate <T> match)
		{
			if (match == null)
				throw new ArgumentNullException ("match");
		}
		
		// Maybe we could make this faster. For example, you could
		// make a bit set with stackalloc for which elements to copy
		// then you could size the array correctly.
		public List <T> FindAll (Predicate <T> match)
		{
			CheckMatch (match);
			List <T> f = new List <T> ();
			
			foreach (T t in this)
				if (match (t))
					f.Add (t);
			
			return f;
		}
		
		public int FindIndex (Predicate <T> match)
		{
			CheckMatch (match);
			return GetIndex (0, _size, match);
		}
		
		public int FindIndex (int startIndex, Predicate <T> match)
		{
			CheckMatch (match);
			CheckIndex (startIndex);
			return GetIndex (startIndex, _size - startIndex, match);
		}
		public int FindIndex (int startIndex, int count, Predicate <T> match)
		{
			CheckMatch (match);
			CheckRange (startIndex, count);
			return GetIndex (startIndex, count, match);
		}
		int GetIndex (int startIndex, int count, Predicate <T> match)
		{
			for (int i = startIndex; i < startIndex + count; i ++)
				if (match (_items [i]))
					return i;
				
			return -1;
		}
		
		public T FindLast (Predicate <T> match)
		{
			CheckMatch (match);
			int i = GetLastIndex (0, _size, match);
			return i == -1 ? default (T) : this [i];
		}
		
		public int FindLastIndex (Predicate <T> match)
		{
			CheckMatch (match);
			return GetLastIndex (0, _size, match);
		}
		
		public int FindLastIndex (int startIndex, Predicate <T> match)
		{
			CheckMatch (match);
			CheckIndex (startIndex);
			return GetLastIndex (0, startIndex + 1, match);
		}
		
		public int FindLastIndex (int startIndex, int count, Predicate <T> match)
		{
			CheckMatch (match);
			int start = startIndex - count + 1;
			CheckRange (start, count);
			return GetLastIndex (start, count, match);
		}

		int GetLastIndex (int startIndex, int count, Predicate <T> match)
		{
			// unlike FindLastIndex, takes regular params for search range
			for (int i = startIndex + count; i != startIndex;)
				if (match (_items [--i]))
					return i;
			return -1;	
		}
		
		public void ForEach (Action <T> action)
		{
			if (action == null)
				throw new ArgumentNullException ("action");
			foreach (T t in this)
				action (t);
		}
		
		public Enumerator GetEnumerator ()
		{
			return new Enumerator (this);
		}
		
		public List <T> GetRange (int index, int count)
		{
			CheckRange (index, count);
			T [] tmpArray = new T [count];
			Array.Copy (_items, index, tmpArray, 0, count);
			return new List <T> (tmpArray, count);
		}
		
		public int IndexOf (T item)
		{
			return Array.IndexOf<T> (_items, item, 0, _size);
		}
		
		public int IndexOf (T item, int index)
		{
			CheckIndex (index);
			return Array.IndexOf<T> (_items, item, index, _size - index);
		}
		
		public int IndexOf (T item, int index, int count)
		{
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index");
			
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count");

			if ((uint) index + (uint) count > (uint) _size)
				throw new ArgumentOutOfRangeException ("index and count exceed length of list");

			return Array.IndexOf<T> (_items, item, index, count);
		}
		
		void Shift (int start, int delta)
		{
			if (delta < 0)
				start -= delta;
			
			Array.Copy (_items, start, _items, start + delta, _size - start);
			
			_size += delta;
		}

		void CheckIndex (int index)
		{
			if (index < 0 || (uint) index > (uint) _size)
				throw new ArgumentOutOfRangeException ("index");
		}
		
		public void Insert (int index, T item)
		{
			CheckIndex (index);			
			GrowIfNeeded (1);
			Shift (index, 1);
			this [index] = item;
			_version++;
		}

		void CheckCollection (IEnumerable <T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException ("collection");
		}
		
		public void InsertRange (int index, IEnumerable <T> collection)
		{
			CheckCollection (collection);
			CheckIndex (index);
			if (collection == this) {
				T[] buffer = new T[_size];
				CopyTo (buffer, 0);
				GrowIfNeeded (_size);
				Shift (index, buffer.Length);
				Array.Copy (buffer, 0, _items, index, buffer.Length);
			} else {
				ICollection <T> c = collection as ICollection <T>;
				if (c != null)
					InsertCollection (index, c);
				else
					InsertEnumeration (index, collection);
			}
			_version++;
		}

		void InsertCollection (int index, ICollection <T> collection)
		{
			int collectionCount = collection.Count;
			GrowIfNeeded (collectionCount);
			
			Shift (index, collectionCount);
			collection.CopyTo (_items, index);
		}
		void InsertEnumeration (int index, IEnumerable <T> enumerable)
		{
			foreach (T t in enumerable)
				Insert (index++, t);		
		}

		public int LastIndexOf (T item)
		{
			return Array.LastIndexOf<T> (_items, item, _size - 1, _size);
		}
		
		public int LastIndexOf (T item, int index)
		{
			CheckIndex (index);
			return Array.LastIndexOf<T> (_items, item, index, index + 1);
		}
		
		public int LastIndexOf (T item, int index, int count)
		{
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index", index, "index is negative");

			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", count, "count is negative");

			if (index - count + 1 < 0)
				throw new ArgumentOutOfRangeException ("cound", count, "count is too large");

			return Array.LastIndexOf<T> (_items, item, index, count);
		}
		
		public bool Remove (T item)
		{
			int loc = IndexOf (item);
			if (loc != -1)
				RemoveAt (loc);
			
			return loc != -1;
		}
		
		// FIXME: this could probably be made faster.
		public int RemoveAll (Predicate <T> match)
		{
			CheckMatch (match);

			int index = 0;
			int c = 0;
			while ((index = GetIndex (index, _size - index, match)) != -1) {
				RemoveAt (index);
				c ++;
			}
			
			Array.Clear (_items, _size, c);
			return c;
		}
		
		public void RemoveAt (int index)
		{
			CheckIndex (index);
			Shift (index, -1);
			Array.Clear (_items, _size, 0);
			_version++;
		}
		
		public void RemoveRange (int index, int count)
		{
			CheckRange (index, count);
			if (count > 0) {
				Shift (index, -count);
				Array.Clear (_items, _size, count);
				_version++;
			}
		}
		
		public void Reverse ()
		{
			Array.Reverse (_items, 0, _size);
			_version++;
		}
		public void Reverse (int index, int count)
		{
			CheckRange (index, count);
			Array.Reverse (_items, index, count);
			_version++;
		}
		
		public void Sort ()
		{
			Array.Sort<T> (_items, 0, _size, Comparer <T>.Default);
			_version++;
		}
		public void Sort (IComparer <T> comparer)
		{
			Array.Sort<T> (_items, 0, _size, comparer);
			_version++;
		}

		public void Sort (Comparison <T> comparison)
		{
			Array.Sort<T> (_items, _size, comparison);
			_version++;
		}
		
		public void Sort (int index, int count, IComparer <T> comparer)
		{
			CheckRange (index, count);
			Array.Sort<T> (_items, index, count, comparer);
			_version++;
		}

		public T [] ToArray ()
		{
			T [] t = new T [_size];
			Array.Copy (_items, t, _size);
			
			return t;
		}
		
		public void TrimExcess ()
		{
			Capacity = _size;
		}
		
		public bool TrueForAll (Predicate <T> match)
		{
			CheckMatch (match);

			foreach (T t in this)
				if (!match (t))
					return false;
				
			return true;
		}
		
		public int Capacity {
			get { 
				return _items.Length;
			}
			set {
				if ((uint) value < (uint) _size)
					throw new ArgumentOutOfRangeException ();
				
				Array.Resize (ref _items, value);
			}
		}
		
		public int Count {
			get { return _size; }
		}
		
		public T this [int index] {
			get {
				if ((uint) index >= (uint) _size)
					throw new ArgumentOutOfRangeException ("index");
				return _items [index];
			}
			set {
				CheckIndex (index);
				if ((uint) index == (uint) _size)
					throw new ArgumentOutOfRangeException ("index");
				_items [index] = value;
			}
		}
		
#region Interface implementations.
		IEnumerator <T> IEnumerable <T>.GetEnumerator ()
		{
			return GetEnumerator ();
		}
		
		void ICollection.CopyTo (Array array, int arrayIndex)
		{
			Array.Copy (_items, 0, array, arrayIndex, _size);
		}
		
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
		
		int IList.Add (object item)
		{
			try {
				Add ((T) item);
			} catch (InvalidCastException) {
				throw new ArgumentException("item");
			}
			return _size - 1;
		}
		
		bool IList.Contains (object item)
		{
			try {
				return Contains ((T) item);
			} catch (InvalidCastException) {
				return false;
			}
		}
		
		int IList.IndexOf (object item)
		{
			try {
				return IndexOf((T) item);
			} catch (InvalidCastException) {
				return -1;
			}
		}
		
		void IList.Insert (int index, object item)
		{
			// We need to check this first because, even if the
			// item is null or not the correct type, we need to
			// return an ArgumentOutOfRange exception if the
			// index is out of range
			CheckIndex (index);
			try {
				Insert (index, (T) item);
			} catch (InvalidCastException) {
				throw new ArgumentException("item");
			}
		}
		
		void IList.Remove (object item)
		{
			try {
				Remove ((T) item);
			} catch (InvalidCastException) {
				// Swallow the exception--if we
				// can't cast to the correct type
				// then we've already "succeeded"
				// in removing the item from the
				// List.
			}
		}
		
		bool ICollection <T>.IsReadOnly {
			get { return false; }
		}
		bool ICollection.IsSynchronized {
			get { return false; }
		}
		
		object ICollection.SyncRoot {
			get { return this; }
		}
		bool IList.IsFixedSize {
			get { return false; }
		}
		
		bool IList.IsReadOnly {
			get { return false; }
		}
		
		object IList.this [int index] {
			get { return this [index]; }
			set { this [index] = (T) value; }
		}
#endregion
				
		[Serializable]
		public struct Enumerator : IEnumerator <T>, IDisposable {
			const int NOT_STARTED = -2;
			
			// this MUST be -1, because we depend on it in move next.
			// we just decr the size, so, 0 - 1 == FINISHED
			const int FINISHED = -1;
			
			List <T> l;
			int idx;
			int ver;
			
			internal Enumerator (List <T> l)
			{
				this.l = l;
				idx = NOT_STARTED;
				ver = l._version;
			}
			
			// for some fucked up reason, MSFT added a useless dispose to this class
			// It means that in foreach, we must still do a try/finally. Broken, very
			// broken.
			public void Dispose ()
			{
				idx = NOT_STARTED;
			}
			
			public bool MoveNext ()
			{
				if (ver != l._version)
					throw new InvalidOperationException ("Collection was modified;"
						+ "enumeration operation may not execute.");
				
				if (idx == NOT_STARTED)
					idx = l._size;
				
				return idx != FINISHED && -- idx != FINISHED;
			}
			
			public T Current {
				get {
					if (idx < 0)
						throw new InvalidOperationException ();
					
					return l._items [l._size - 1 - idx];
				}
			}
			
			void IEnumerator.Reset ()
			{
				if (ver != l._version)
					throw new InvalidOperationException ("Collection was modified;"
						+ "enumeration operation may not execute.");
				
				idx = NOT_STARTED;
			}
			
			object IEnumerator.Current {
				get { return Current; }
			}
		}
	}
}
#endif
