// 
// System.Collections.SortedList
// 
// Author:
//   Sergey Chaban (serge@wildwestsoftware.com)
// 


using System;
using System.Collections;


namespace System.Collections {

	/// <summary>
	///  Represents a collection of associated keys and values
	///  that are sorted by the keys and are accessible by key
	///  and by index.
	/// </summary>
	[Serializable]
	public class SortedList : IDictionary, ICollection,
	                          IEnumerable, ICloneable {


		internal struct Slot {
			internal Object key;
			internal Object value;
		}

		private readonly static int INITIAL_SIZE = 16;

		public enum EnumeratorMode : int {KEY_MODE = 0, VALUE_MODE}

		private int inUse;
		private int modificationCount;
		private Slot[] table;
		private IComparer comparer;



		//
		// Constructors
		//
		public SortedList () : this (INITIAL_SIZE)
		{
		}

		public SortedList (int initialCapacity)
			: this (null, initialCapacity)
		{
		}

		public SortedList (IComparer comparer, int initialCapacity)
		{
			this.comparer = comparer;
			InitTable (initialCapacity);
		}

		public SortedList (IComparer comparer)
			: this (comparer, 0)
		{
		}


		public SortedList (IDictionary d) : this (d, null)
		{
		}

		public SortedList (IDictionary d, IComparer comparer)
		{
			if (d  ==  null)
				throw new ArgumentNullException ("dictionary");

			InitTable (d.Count);
			this.comparer = comparer;

			IDictionaryEnumerator it = d.GetEnumerator ();
			while (it.MoveNext ()) {
				if (it.Key is IComparable) {
					Add (it.Key, it.Value);
				} else {
					throw new InvalidCastException("!IComparable");
				}
			}
		}




		//
		// Properties
		//


		// ICollection

		public virtual int Count {
			get {
				return inUse;
			}
		}

		public virtual bool IsSynchronized {
			get {
				return false;
			}
		}

		public virtual Object SyncRoot {
			get {
				return this;
			}
		}


		// IDictionary

		public virtual bool IsFixedSize {
			get {
				return false;
			}
		}


		public virtual bool IsReadOnly {
			get {
				return false;
			}
		}

		public virtual ICollection Keys {
			get {
				return new ListKeys (this);
			}
		}

		public virtual ICollection Values {
			get {
				return new ListValues (this);
			}
		}



		public virtual Object this [Object key] {
			get {
				return GetImpl (key);
			}
			set {
				PutImpl (key, value, true);
			}
		}




		public virtual int Capacity {
			get {
				return table.Length;
			}
			set {
				Slot [] table = this.table;
				int current = table.Length;

				if (inUse > value)
					throw new ArgumentOutOfRangeException("capacity too small");

				if (value > current) {
					Slot [] newTable = new Slot [value];
					Array.Copy (table, newTable, current);
					this.table = newTable;
				}
			}
		}



		//
		// Public instance methods.
		//


		// IEnumerable

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new Enumerator (this, EnumeratorMode.KEY_MODE);
		}


		// IDictionary

		public virtual void Add (object key, object value)
		{
			PutImpl (key, value, false);
		}


		public virtual void Clear () 
		{
			this.table = new Slot [Capacity];
			inUse = 0;
			modificationCount++;
		}

		public virtual bool Contains (object key)
		{
			return (Find (key) >= 0);
		}


		public virtual IDictionaryEnumerator GetEnumerator ()
		{
			return new Enumerator (this, EnumeratorMode.KEY_MODE);
		}

		public virtual void Remove (object key)
		{
			int i = IndexOfKey (key);
			if (i >= 0) RemoveAt (i);
		}


		// ICollection

		public virtual void CopyTo (Array array, int arrayIndex)
		{
			IDictionaryEnumerator it = GetEnumerator ();
			int i = arrayIndex;

			while (it.MoveNext ()) {
				array.SetValue (it.Entry, i++);
			}
		}



		// ICloneable

		public virtual object Clone ()
		{
			SortedList sl = new SortedList (this, comparer);
			sl.modificationCount = this.modificationCount;
			return sl;
		}




		//
		// SortedList
		//

		public virtual IList GetKeyList ()
		{
			return new ListKeys (this);
		}


		public virtual IList GetValueList ()
		{
			return new ListValues (this);
		}


		public virtual void RemoveAt (int index)
		{
			Slot [] table = this.table;
			int cnt = Count;
			if (index >= 0 && index < cnt) {
				if (index != cnt - 1) {
					Array.Copy (table, index+1, table, index, cnt-1);
				} else {
					table [index].key = null;
					table [index].value = null;
				}
				--inUse;
				++modificationCount;
			} else {
				throw new ArgumentOutOfRangeException("index out of range");
			}
		}





		public virtual int IndexOfKey (object key)
		{
			int indx = Find (key);
			return (indx | (indx >> 31));
		}


		public virtual int IndexOfValue (object value)
		{
			Slot [] table = this.table;
			int len = table.Length;

			for (int i=0; i < len; i++) {
				if (table[i].value.Equals (value)) {
					return i;
				}
			}

			return -1;
		}


		public virtual bool ContainsKey (object key)
		{
			return Contains (key);
		}


		public virtual bool ContainsValue (object value)
		{
			return IndexOfValue (value) >= 0;
		}


		public virtual object GetByIndex (int index)
		{
			if (index >= 0 && index < Count) {
				return table [index].value;
			} else {
				throw new ArgumentOutOfRangeException("index out of range");
			}
		}


		public virtual void SetByIndex (int index, object value)
		{
			if (index >= 0 && index < Count) {
				table [index].value = value;
			} else {
				throw new ArgumentOutOfRangeException("index out of range");
			}
		}


		public virtual object GetKey (int index)
		{
			if (index >= 0 && index < Count) {
				return table [index].key;
			} else {
				throw new ArgumentOutOfRangeException("index out of range");
			}
		}

		[MonoTODO]
		public static SortedList Synchronized (SortedList list)
		{
			return null;
		}

		public virtual void TrimToSize ()
		{
			// From Beta2:
			// Trimming an empty SortedList sets the capacity
			// of the SortedList to the default capacity,
			// not zero.
			if (Count == 0) Resize (INITIAL_SIZE, false);
			else Resize (Count, true);
		}


		//
		// Private methods
		//


		private void Resize (int n, bool copy)
		{
			Slot [] table = this.table;
			Slot [] newTable = new Slot [n];
			if (copy) Array.Copy (table, 0, newTable, 0, n);
			this.table = newTable;
		}


		private void EnsureCapacity (int n, int free)
		{
			Slot [] table = this.table;
			Slot [] newTable = null;
			int cap = Capacity;
			bool gap = (free >=0 && free < Count);

			if (n > cap) {
				newTable = new Slot [n << 1];
			}

			if (newTable != null) {
				if (gap) {
					int copyLen = free;
					if (copyLen > 0) {
						Array.Copy (table, 0, newTable, 0, copyLen);
					}
					copyLen = Count - free;
					if (copyLen > 0) {
						Array.Copy (table, free, newTable, free+1, copyLen);
					}
				} else {
					// Just a resizing, copy the entire table.
					Array.Copy (table, newTable, Count);
				}
				this.table = newTable;
			} else if (gap) {
				Array.Copy (table, free, table, free+1, Count - free);
			}
		}


		private void PutImpl (object key, object value, bool overwrite)
		{
			if (key == null)
				throw new ArgumentNullException ("null key");

			Slot [] table = this.table;
			int freeIndx = Find (key);


			if (freeIndx >= 0) {
				if (!overwrite)
					throw new ArgumentException("element already exists");

				table [freeIndx].value = value;
				return;
			}

			freeIndx = ~freeIndx;

			if (freeIndx > Capacity + 1)
				throw new Exception ("SortedList::internal error ("+key+", "+value+") at ["+freeIndx+"]");


			EnsureCapacity (Count+1, freeIndx);

			table = this.table;
			table [freeIndx].key = key;
			table [freeIndx].value = value;

			++inUse;
			++modificationCount;

		}


		private object GetImpl (object key)
		{
			int i = Find (key);

			if (i >= 0)
				return table [i].value;
			else
				return null;
		}


		private void InitTable (int capacity)
		{
			int size = (capacity + 1) & (~1);
			if (size < INITIAL_SIZE) size = INITIAL_SIZE;
			this.table = new Slot [size];
			this.inUse = 0;
			this.modificationCount = 0;
		}


		private void  CopyToArray (Array arr, int i, 
					   EnumeratorMode mode)
		{
			IEnumerator it = new Enumerator (this, mode);

			while (it.MoveNext ()) {
				arr.SetValue (it.Current, i++);
			}
		}


		private int Find (object key)
		{
			Slot [] table = this.table;
			int len = Count;

			if (len == 0) return ~0;

			IComparer comparer = (this.comparer == null)
			                      ? Comparer.Default
			                      : this.comparer;

			int left = 0;
			int right = len-1;

			while (left <= right) {
				int guess = (left + right) >> 1;

				int cmp = comparer.Compare (key, table[guess].key);

				if (cmp == 0) return guess;

				cmp &= ~Int32.MaxValue;

				if (cmp == 0) left = guess+1;
				else right = guess-1;
			}

			return ~left;
		}



		//
		// Inner classes
		//


		protected sealed class Enumerator : IDictionaryEnumerator,
		                                    IEnumerator {

			private SortedList host;
			private int stamp;
			private int pos;
			private int size;
			private EnumeratorMode mode;

			private object currentKey;
			private object currentValue;

			private readonly static string xstr = "SortedList.Enumerator: snapshot out of sync.";

			public Enumerator (SortedList host, EnumeratorMode mode)
			{
				this.host = host;
				stamp = host.modificationCount;
				size = host.Count;
				this.mode = mode;
				Reset ();
			}

			public Enumerator (SortedList host)
			           : this (host, EnumeratorMode.KEY_MODE)
			{
			}


			private void FailFast ()
			{
				if (host.modificationCount != stamp) {
					throw new InvalidOperationException (xstr);
				}
			}

			public void Reset ()
			{
				FailFast ();

				pos = -1;
				currentKey = null;
				currentValue = null;
			}

			public bool MoveNext ()
			{
				FailFast ();

				Slot [] table = host.table;

				if (++pos < size) {
					Slot entry = table [pos];

					currentKey = entry.key;
					currentValue = entry.value;
					return true;
				}

				currentKey = null;
				currentValue = null;
				return false;
			}

			public DictionaryEntry Entry
			{
				get {
					FailFast ();
					return new DictionaryEntry (currentKey,
					                            currentValue);
				}
			}

			public Object Key {
				get {
					FailFast ();
					return currentKey;
				}
			}

			public Object Value {
				get {
					FailFast ();
					return currentValue;
				}
			}

			public Object Current {
				get {
					FailFast ();
					return (mode == EnumeratorMode.KEY_MODE)
					        ? currentKey
					        : currentValue;
				}
			}
		}


		protected class ListKeys : IList, IEnumerable {

			private SortedList host;


			public ListKeys (SortedList host)
			{
				if (host == null)
					throw new ArgumentNullException ();

				this.host = host;
			}

			//
			// ICollection
			//

			public virtual int Count {
				get {
					return host.Count;
				}
			}

			public virtual bool IsSynchronized {
				get {
					return host.IsSynchronized;
				}
			}

			public virtual Object SyncRoot {
				get {
					return host.SyncRoot;
				}
			}

			public virtual void CopyTo (Array array, int arrayIndex)
			{
				host.CopyToArray (array, arrayIndex, EnumeratorMode.KEY_MODE);
			}


			//
			// IList
			//

			public virtual bool IsFixedSize {
				get {
					return true;
				}
			}

			public virtual bool IsReadOnly {
				get {
					return true;
				}
			}


			public virtual object this [int index] {
				get {
					return host.GetKey (index);
				}
				set {
					throw new NotSupportedException("attempt to modify a key");
				}
			}

			public virtual int Add (object value)
			{
				throw new NotSupportedException("IList::Add not supported");
			}

			public virtual void Clear ()
			{
				throw new NotSupportedException("IList::Clear not supported");
			}

			public virtual bool Contains (object key)
			{
				return host.Contains (key);
			}


			public virtual int IndexOf (object key)
			{
				return host.IndexOfKey (key);
			}


			public virtual void Insert (int index, object value)
			{
				throw new NotSupportedException("IList::Insert not supported");
			}


			public virtual void Remove (object value)
			{
				throw new NotSupportedException("IList::Remove not supported");
			}


			public virtual void RemoveAt (int index)
			{
				throw new NotSupportedException("IList::RemoveAt not supported");
			}


			//
			// IEnumerable
			//

			public virtual IEnumerator GetEnumerator ()
			{
				return new SortedList.Enumerator (host, EnumeratorMode.KEY_MODE);
			}


		}


		protected class ListValues : IList, IEnumerable {

			private SortedList host;


			public ListValues (SortedList host)
			{
				if (host == null)
					throw new ArgumentNullException ();

				this.host = host;
			}

			//
			// ICollection
			//

			public virtual int Count {
				get {
					return host.Count;
				}
			}

			public virtual bool IsSynchronized {
				get {
					return host.IsSynchronized;
				}
			}

			public virtual Object SyncRoot {
				get {
					return host.SyncRoot;
				}
			}

			public virtual void CopyTo (Array array, int arrayIndex)
			{
				host.CopyToArray (array, arrayIndex, EnumeratorMode.VALUE_MODE);
			}


			//
			// IList
			//

			public virtual bool IsFixedSize {
				get {
					return true;
				}
			}

			public virtual bool IsReadOnly {
				get {
					return true;
				}
			}


			[MonoTODO]
			public virtual object this [int index] {
				get {
					return host.GetByIndex (index);
				}
				set {
					// FIXME: It seems (according to tests)
					// that modifications are allowed
					// in Beta2.
					// ? host.SetByIndex (index, value);
					throw new NotSupportedException("attempt to modify a value");
				}
			}

			public virtual int Add (object value)
			{
				throw new NotSupportedException("IList::Add not supported");
			}

			public virtual void Clear ()
			{
				throw new NotSupportedException("IList::Clear not supported");
			}

			public virtual bool Contains (object value)
			{
				return host.ContainsValue (value);
			}


			public virtual int IndexOf (object value)
			{
				return host.IndexOfValue (value);
			}


			public virtual void Insert (int index, object value)
			{
				throw new NotSupportedException("IList::Insert not supported");
			}


			public virtual void Remove (object value)
			{
				throw new NotSupportedException("IList::Remove not supported");
			}


			public virtual void RemoveAt (int index)
			{
				throw new NotSupportedException("IList::RemoveAt not supported");
			}


			//
			// IEnumerable
			//

			public virtual IEnumerator GetEnumerator ()
			{
				return new SortedList.Enumerator (host, EnumeratorMode.VALUE_MODE);
			}


		}

	} // SortedList

} // System.Collections
