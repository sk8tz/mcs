//
// System.Collections.Generic.Dictionary
//
// Authors:
//	Sureshkumar T (tsureshkumar@novell.com)
//	Marek Safar (marek.safar@seznam.cz) (stubs)
//	Ankit Jain (radical@corewars.org)
//	David Waite (mass@akuma.org)
//
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Collections.Generic {

	[Serializable]
	public class Dictionary<TKey, TValue> : IDictionary<TKey, TValue>,
		IDictionary,
		ICollection,
		ICollection<KeyValuePair<TKey, TValue>>,
		IEnumerable<KeyValuePair<TKey, TValue>>,
		ISerializable,
		IDeserializationCallback
	{
		const int INITIAL_SIZE = 10;
		const float DEFAULT_LOAD_FACTOR = (90f / 100);

		private class Slot {
			public KeyValuePair<TKey, TValue> Data;
			public Slot Next;

			public Slot (TKey Key, TValue Value, Slot Next)
			{
				this.Data = new KeyValuePair<TKey,TValue> (Key, Value);
				this.Next = Next;
			}
		}

		Slot [] table;
		int used_slots;
		private int threshold;

		IEqualityComparer<TKey> hcp;
		SerializationInfo serialization_info;

		private uint generation;

		public int Count {
			get { return used_slots; }
			/* FIXME: this should be 'private' not 'internal'.  */
			internal set {
				used_slots = value;
				++generation;
			}
		}

		public TValue this [TKey key] {
			get {
				int index;
				Slot slot = GetSlot (key, out index);
				if (slot == null)
					throw new KeyNotFoundException ();
				return slot.Data.Value;
			}

			set {
				int index;
				Slot slot = GetSlot (key, out index);
				if (slot == null) {
					DoAdd (index, key, value);
				} else {
					++generation;
					slot.Data = new KeyValuePair<TKey, TValue> (key, value);
				}
			}
		}

		public Dictionary ()
		{
			Init (INITIAL_SIZE, null);
		}

		public Dictionary (IEqualityComparer<TKey> comparer)
		{
			Init (INITIAL_SIZE, comparer);
		}

		public Dictionary (IDictionary<TKey, TValue> dictionary)
			: this (dictionary, null)
		{
		}

		public Dictionary (int capacity)
		{
			Init (capacity, null);
		}

		public Dictionary (IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
		{
			if (dictionary == null)
				throw new ArgumentNullException ("dictionary");
			int capacity = dictionary.Count;
			Init (capacity, comparer);
			foreach (KeyValuePair<TKey, TValue> entry in dictionary)
				this.Add (entry.Key, entry.Value);
		}

		public Dictionary (int capacity, IEqualityComparer<TKey> comparer)
		{
			Init (capacity, comparer);
		}

		protected Dictionary (SerializationInfo info, StreamingContext context)
		{
			serialization_info = info;
		}

		void SetThreshold ()
		{
			threshold = (int)(table.Length * DEFAULT_LOAD_FACTOR);
			if (threshold == 0 && table.Length > 0)
				threshold = 1;
		}

		private void Init (int capacity, IEqualityComparer<TKey> hcp)
		{
			if (capacity < 0)
				throw new ArgumentOutOfRangeException ("capacity");
			this.hcp = (hcp != null) ? hcp : EqualityComparer<TKey>.Default;
			if (capacity == 0)
				capacity = INITIAL_SIZE;
			
			table = new Slot [capacity];
			SetThreshold ();
			generation = 0;
		}

		void CopyTo (KeyValuePair<TKey, TValue> [] array, int index)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (index < 0)
				throw new ArgumentOutOfRangeException ("index");
			if (index >= array.Length)
				throw new ArgumentException ("index larger than largest valid index of array");
			if (array.Length - index < Count)
				throw new ArgumentException ("Destination array cannot hold the requested elements!");

			for (int i = 0; i < table.Length; ++i) {
				for (Slot slot = table [i]; slot != null; slot = slot.Next)
					array [index++] = slot.Data;
			}
		}

		private void Resize ()
		{
			// From the SDK docs:
			//	 Hashtable is automatically increased
			//	 to the smallest prime number that is larger
			//	 than twice the current number of Hashtable buckets
			uint newSize = (uint) Hashtable.ToPrime ((table.Length << 1) | 1);

			Slot nextslot = null;
			Slot [] oldTable = table;

			table = new Slot [newSize];
			SetThreshold ();

			int index;
			for (int i = 0; i < oldTable.Length; i++) {
				for (Slot slot = oldTable [i]; slot != null; slot = nextslot) {
					nextslot = slot.Next;

					index = DoHash (slot.Data.Key);
					slot.Next = table [index];
					table [index] = slot;
				}
			}
		}

		public void Add (TKey key, TValue value)
		{
			int index;
			Slot slot = GetSlot (key, out index);
			if (slot != null)
				throw new ArgumentException ("An element with the same key already exists in the dictionary.");
			DoAdd (index, key, value);
		}

		void DoAdd (int index, TKey key, TValue value)
		{
			if (Count++ >= threshold) {
				Resize ();
				index = DoHash (key);
			}
			table [index] = new Slot (key, value, table [index]);
		}

		private int DoHash (TKey key)
		{
			int size = this.table.Length;
			int h = hcp.GetHashCode (key) & Int32.MaxValue;
			int spot = (int) ((uint) h % size);
			return spot;
		}

		public IEqualityComparer<TKey> Comparer {
			get { return hcp; }
		}

		public void Clear ()
		{
			Count = 0;
			for (int i = 0; i < table.Length; i++)
				table [i] = null;
		}

		public bool ContainsKey (TKey key)
		{
			int index;
			return GetSlot (key, out index) != null;
		}

		public bool ContainsValue (TValue value)
		{
			IEqualityComparer<TValue> cmp = EqualityComparer<TValue>.Default;

			for (int i = 0; i < table.Length; ++i) {
				for (Slot slot = table [i]; slot != null; slot = slot.Next) {
					if (cmp.Equals (value, slot.Data.Value))
						return true;
				}
			}
			return false;
		}

		[SecurityPermission (SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			info.AddValue ("hcp", hcp);
			KeyValuePair<TKey, TValue> [] data = null;
			if (Count > 0) {
				data = new KeyValuePair<TKey,TValue> [Count];
				CopyTo (data, 0);
			}
			info.AddValue ("data", data);
			info.AddValue ("buckets_hint", table.Length);
		}

		public virtual void OnDeserialization (object sender)
		{
			if (serialization_info == null)
				return;

			hcp = (IEqualityComparer<TKey>) serialization_info.GetValue ("hcp", typeof (IEqualityComparer<TKey>));
			KeyValuePair<TKey, TValue> [] data =
				(KeyValuePair<TKey, TValue> [])
				serialization_info.GetValue ("data", typeof (KeyValuePair<TKey, TValue> []));

			int buckets = serialization_info.GetInt32 ("buckets_hint");
			if (buckets < INITIAL_SIZE)
				buckets = INITIAL_SIZE;

			table = new Slot [buckets];
			SetThreshold ();
			Count = 0;

			if (data != null) {
				for (int i = 0; i < data.Length; ++i)
					Add (data [i].Key, data [i].Value);
			}
			serialization_info = null;
		}

		public bool Remove (TKey key)
		{
			int index;
			Slot slot = GetSlot (key, out index);
			if (slot == null)
				return false;

			--Count;
			if (slot == table [index]) {
				table [index] = table [index].Next;
			} else {
				Slot prev = table [index];
				while (prev.Next != slot)
					prev = prev.Next;
				prev.Next = slot.Next;
			}
			return true;
		}

		//
		// Return the slot containing key, and set 'index' to the chain the key was found in.
		// If the key is not found, return null and set 'index' to the chain that would've contained the key.
		//
		private Slot GetSlot (TKey key, out int index)
		{
			if (key == null)
				throw new ArgumentNullException ("key");
			index = DoHash (key);
			Slot slot = table [index];
			while (slot != null && !hcp.Equals (key, slot.Data.Key))
				slot = slot.Next;
			return slot;
		}

		public bool TryGetValue (TKey key, out TValue value)
		{
			int index;
			Slot slot = GetSlot (key, out index);
			bool found = slot != null;
			value = found ? slot.Data.Value : default (TValue);
			return found;
		}

		ICollection<TKey> IDictionary<TKey, TValue>.Keys {
			get { return Keys; }
		}

		ICollection<TValue> IDictionary<TKey, TValue>.Values {
			get { return Values; }
		}

		public KeyCollection Keys {
			get { return new KeyCollection (this); }
		}

		public ValueCollection Values {
			get { return new ValueCollection (this); }
		}

		ICollection IDictionary.Keys {
			get { return Keys; }
		}

		ICollection IDictionary.Values {
			get { return Values; }
		}

		bool IDictionary.IsFixedSize {
			get { return false; }
		}

		bool IDictionary.IsReadOnly {
			get { return false; }
		}

		object IDictionary.this [object key] {
			get {
				if (!(key is TKey))
					throw new ArgumentException ("key is of not '" + typeof (TKey).ToString () + "'!");
				return this [(TKey) key];
			}
			set { this [(TKey) key] = (TValue) value; }
		}

		void IDictionary.Add (object key, object value)
		{
			if (!(key is TKey))
				throw new ArgumentException ("key is of not '" + typeof (TKey).ToString () + "'!");
			if (!(value is TValue))
				throw new ArgumentException ("value is of not '" + typeof (TValue).ToString () + "'!");
			this.Add ((TKey) key, (TValue) value);
		}

		bool IDictionary.Contains (object key)
		{
			return ContainsKey ((TKey) key);
		}

		void IDictionary.Remove (object key)
		{
			Remove ((TKey) key);
		}

		bool ICollection.IsSynchronized {
			get { return false; }
		}

		object ICollection.SyncRoot {
			get { return this; }
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly {
			get { return false; }
		}

		void ICollection<KeyValuePair<TKey, TValue>>.Add (KeyValuePair<TKey, TValue> keyValuePair)
		{
			Add (keyValuePair.Key, keyValuePair.Value);
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Contains (KeyValuePair<TKey, TValue> keyValuePair)
		{
			return this.ContainsKey (keyValuePair.Key);
		}

		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo (KeyValuePair<TKey, TValue> [] array, int index)
		{
			this.CopyTo (array, index);
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Remove (KeyValuePair<TKey, TValue> keyValuePair)
		{
			return Remove (keyValuePair.Key);
		}

		void ICollection.CopyTo (Array array, int index)
		{
			// TODO: Verify this can be a KeyValuePair, and doesn't need to be
			// a DictionaryEntry type
			CopyTo ((KeyValuePair<TKey, TValue> []) array, index);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new Enumerator (this);
		}

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator ()
		{
			return new Enumerator (this);
		}

		IDictionaryEnumerator IDictionary.GetEnumerator ()
		{
			return new Enumerator (this);
		}

		public Enumerator GetEnumerator ()
		{
			return new Enumerator (this);
		}

		[Serializable]
		public struct Enumerator : IEnumerator<KeyValuePair<TKey,TValue>>,
			IDisposable, IDictionaryEnumerator, IEnumerator
		{
			Dictionary<TKey, TValue> dictionary;
			uint stamp;

			Slot current;
			int next_index;

			internal Enumerator (Dictionary<TKey, TValue> dictionary)
			{
				this.dictionary = dictionary;
				stamp = dictionary.generation;

				// The following stanza is identical to IEnumerator.Reset (),
				// but because of the definite assignment rule, we cannot call it here.
				next_index = 0;
				current = null;
			}

			public bool MoveNext ()
			{
				if (dictionary == null)
					throw new ObjectDisposedException (null);
				if (dictionary.generation != stamp)
					throw new InvalidOperationException ("out of sync");

				// Pre-condition: current == null => this is the first call
				// to MoveNext ()
				if (current != null)
					current = current.Next;

				while (current == null && next_index < dictionary.table.Length)
					current = dictionary.table [next_index++];

				// Post-condition: current == null => this is the last call
				// to MoveNext()
				return current != null;
			}

			public KeyValuePair<TKey, TValue> Current {
				get {
					VerifyState ();
					return current.Data;
				}
			}

			object IEnumerator.Current {
				get { return ((IDictionaryEnumerator) this).Entry; }
			}

			void IEnumerator.Reset ()
			{
				next_index = 0;
				current = null;
			}

			DictionaryEntry IDictionaryEnumerator.Entry {
				get {
					VerifyState ();
					return new DictionaryEntry (current.Data.Key, current.Data.Value);
				}
			}

			object IDictionaryEnumerator.Key {
				get { return Current.Key; }
			}

			object IDictionaryEnumerator.Value {
				get { return Current.Value; }
			}

			void VerifyState ()
			{
				if (dictionary == null)
					throw new ObjectDisposedException (null);
				if (dictionary.generation != stamp)
					throw new InvalidOperationException ("out of sync");
				if (current == null)
					throw new InvalidOperationException ();
			}
			public void Dispose ()
			{
				current = null;
				dictionary = null;
			}
		}

		// This collection is a read only collection
		[Serializable]
		public sealed class KeyCollection : ICollection<TKey>, IEnumerable<TKey>, ICollection, IEnumerable {
			Dictionary<TKey, TValue> dictionary;

			public KeyCollection (Dictionary<TKey, TValue> dictionary)
			{
				if (dictionary == null)
					throw new ArgumentNullException ("dictionary");
				this.dictionary = dictionary;
			}

			public void CopyTo (TKey [] array, int index)
			{
				if (array == null)
					throw new ArgumentNullException ("array");
				if (index < 0)
					throw new ArgumentOutOfRangeException ("index");
				if (index >= array.Length)
					throw new ArgumentException ("index larger than largest valid index of array");
				if (array.Length - index < dictionary.Count)
					throw new ArgumentException ("Destination array cannot hold the requested elements!");

				foreach (TKey k in this)
					array [index++] = k;
			}

			public Enumerator GetEnumerator ()
			{
				return new Enumerator (dictionary);
			}

			void ICollection<TKey>.Add (TKey item)
			{
				throw new NotSupportedException ("this is a read-only collection");
			}

			void ICollection<TKey>.Clear ()
			{
				throw new NotSupportedException ("this is a read-only collection");
			}

			bool ICollection<TKey>.Contains (TKey item)
			{
				return dictionary.ContainsKey (item);
			}

			bool ICollection<TKey>.Remove (TKey item)
			{
				throw new NotSupportedException ("this is a read-only collection");
			}

			IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator ()
			{
				return this.GetEnumerator ();
			}

			void ICollection.CopyTo (Array array, int index)
			{
				CopyTo ((TKey []) array, index);
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return this.GetEnumerator ();
			}

			public int Count {
				get { return dictionary.Count; }
			}

			bool ICollection<TKey>.IsReadOnly {
				get { return true; }
			}

			bool ICollection.IsSynchronized {
				get { return false; }
			}

			object ICollection.SyncRoot {
				get { return ((ICollection) dictionary).SyncRoot; }
			}

			public struct Enumerator : IEnumerator<TKey>, IDisposable, IEnumerator {
				Dictionary<TKey, TValue>.Enumerator host_enumerator;

				internal Enumerator (Dictionary<TKey, TValue> host)
				{
					host_enumerator = host.GetEnumerator ();
				}

				public void Dispose ()
				{
					host_enumerator.Dispose ();
				}

				public bool MoveNext ()
				{
					return host_enumerator.MoveNext ();
				}

				public TKey Current {
					get { return host_enumerator.Current.Key; }
				}

				object IEnumerator.Current {
					get { return host_enumerator.Current.Key; }
				}

				void IEnumerator.Reset ()
				{
					((IEnumerator)host_enumerator).Reset ();
				}
			}
		}

		// This collection is a read only collection
		[Serializable]
		public sealed class ValueCollection : ICollection<TValue>, IEnumerable<TValue>, ICollection, IEnumerable {
			Dictionary<TKey, TValue> dictionary;

			public ValueCollection (Dictionary<TKey, TValue> dictionary)
			{
				if (dictionary == null)
					throw new ArgumentNullException ("dictionary");
				this.dictionary = dictionary;
			}

			public void CopyTo (TValue [] array, int index)
			{
				if (array == null)
					throw new ArgumentNullException ("array");
				if (index < 0)
					throw new ArgumentOutOfRangeException ("index");
				if (index >= array.Length)
					throw new ArgumentException ("index larger than largest valid index of array");
				if (array.Length - index < dictionary.Count)
					throw new ArgumentException ("Destination array cannot hold the requested elements!");

				foreach (TValue k in this)
					array [index++] = k;
			}

			public Enumerator GetEnumerator ()
			{
				return new Enumerator (dictionary);
			}

			void ICollection<TValue>.Add (TValue item)
			{
				throw new NotSupportedException ("this is a read-only collection");
			}

			void ICollection<TValue>.Clear ()
			{
				throw new NotSupportedException ("this is a read-only collection");
			}

			bool ICollection<TValue>.Contains (TValue item)
			{
				return dictionary.ContainsValue (item);
			}

			bool ICollection<TValue>.Remove (TValue item)
			{
				throw new NotSupportedException ("this is a read-only collection");
			}

			IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator ()
			{
				return this.GetEnumerator ();
			}

			void ICollection.CopyTo (Array array, int index)
			{
				CopyTo ((TValue []) array, index);
			}

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return this.GetEnumerator ();
			}

			public int Count {
				get { return dictionary.Count; }
			}

			bool ICollection<TValue>.IsReadOnly {
				get { return true; }
			}

			bool ICollection.IsSynchronized {
				get { return false; }
			}

			object ICollection.SyncRoot {
				get { return ((ICollection) dictionary).SyncRoot; }
			}

			public struct Enumerator : IEnumerator<TValue>, IDisposable, IEnumerator {
				Dictionary<TKey, TValue>.Enumerator host_enumerator;

				internal Enumerator (Dictionary<TKey,TValue> host)
				{
					host_enumerator = host.GetEnumerator ();
				}

				public void Dispose ()
				{
					host_enumerator.Dispose();
				}

				public bool MoveNext ()
				{
					return host_enumerator.MoveNext ();
				}

				public TValue Current {
					get { return host_enumerator.Current.Value; }
				}

				object IEnumerator.Current {
					get { return host_enumerator.Current.Value; }
				}

				void IEnumerator.Reset ()
				{
					((IEnumerator)host_enumerator).Reset ();
				}
			}
		}
	}
}
#endif
