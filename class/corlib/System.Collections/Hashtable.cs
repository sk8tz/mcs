//
// System.Collections.Hashtable
//
// Author:
//   Sergey Chaban (serge@wildwestsoftware.com)
//



using System;
using System.Collections;
using System.Runtime.Serialization;


// TODO: Interfaces to implement: ISerializable and IDeserializationCallback;


namespace System.Collections {

	[MonoTODO]
	public class Hashtable : IDictionary, ICollection, 
		IEnumerable, ICloneable, ISerializable
	{

		internal struct Slot {
			internal Object key;

			internal Object value;

			// Hashcode. Chains are also marked through this.
			internal int hashMix;
		}

		//
		// Private data
		//

		private readonly static int CHAIN_MARKER  = ~Int32.MaxValue;


		// Used as indicator for the removed parts of a chain.
		private readonly static Object REMOVED_MARKER = new Object ();


		private int inUse;
		private int modificationCount;
		private float loadFactor;
		private Slot [] table;
		private int threshold;

		private IHashCodeProvider hcpRef;
		private IComparer comparerRef;

		public static int [] primeTbl = {
			11,
			19,
			37,
			73,
			109,
			163,
			251,
			367,
			557,
			823,
			1237,
			1861,
			2777,
			4177,
			6247,
			9371,
			14057,
			21089,
			31627,
			47431,
			71143,
			106721,
			160073,
			240101,
			360163,
			540217,
			810343,
			1215497,
			1823231,
			2734867,
			4102283,
			6153409,
			9230113,
			13845163
		};

		// Class constructor

		static Hashtable () {
			// NOTE: previously this static constructor was used
			//       to calculate primeTbl, now primeTbl is
			//       hardcoded and constructor does nothing
			//       useful except for forcing compiler to
			//       eliminate beforefieldinit from signature.
		}


		//
		// Constructors
		//

		public Hashtable () : this (0, 1.0f) {}


		public Hashtable (int capacity, float loadFactor, IHashCodeProvider hcp, IComparer comparer) {
			if (capacity<0)
				throw new ArgumentOutOfRangeException ("negative capacity");

			if (loadFactor<0.1 || loadFactor>1)
				throw new ArgumentOutOfRangeException ("load factor");

			if (capacity == 0) ++capacity;
			this.loadFactor = 0.75f*loadFactor;
			int size = (int) (capacity/this.loadFactor);
			size = ToPrime (size);
			this.SetTable (new Slot [size]);

			this.hcp = hcp;
			this.comparer = comparer;

			this.inUse = 0;
			this.modificationCount = 0;

		}

		public Hashtable (int capacity, float loadFactor) :
			this (capacity, loadFactor, null, null)
		{
		}

		public Hashtable (int capacity) : this (capacity, 1.0f)
		{
		}

		public Hashtable (int capacity, 
		                  IHashCodeProvider hcp, 
		                  IComparer comparer)
			: this (capacity, 1.0f, hcp, comparer)
		{
		}


		public Hashtable (IDictionary d, float loadFactor, 
		                  IHashCodeProvider hcp, IComparer comparer)
			: this (d!=null ? d.Count : 0,
		                loadFactor, hcp, comparer)
		{

			if (d  ==  null)
				throw new ArgumentNullException ("dictionary");

			IDictionaryEnumerator it = d.GetEnumerator ();
			while (it.MoveNext ()) {
				Add (it.Key, it.Value);
			}
			
		}

		public Hashtable (IDictionary d, float loadFactor)
		       : this (d, loadFactor, null, null)
		{
		}


		public Hashtable (IDictionary d) : this (d, 1.0f)
		{
		}

		public Hashtable (IDictionary d, IHashCodeProvider hcp, IComparer comparer)
		                 : this (d, 1.0f, hcp, comparer)
		{
		}

		public Hashtable (IHashCodeProvider hcp, IComparer comparer)
		                 : this (1, 1.0f, hcp, comparer)
		{
		}

		[MonoTODO]
		protected Hashtable (SerializationInfo info, StreamingContext context)
		{
//			loadFactor = info.GetValue ("LoadFactor", Type.GetType ("System.Float"));
//			comparerRef = info.GetValue ("Comparer", typeof (object));
//			hcpRef = info.GetValue ("HashCodeProvider", typeof (object));
//			this.Count = info.GetValue ("HashSize");
// 			this.Keys = info.GetValue ("Keys");
// 			this.Values = info.GetValue ("Values");
 		}

		//
		// Properties
		//

		protected IComparer comparer {
			set {
				comparerRef = value;
			}
			get {
				return comparerRef;
			}
		}

		protected IHashCodeProvider hcp {
			set {
				hcpRef = value;
			}
			get {
				return hcpRef;
			}
		}

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
				return new HashKeys (this);
			}
		}

		public virtual ICollection Values {
			get {
				return new HashValues (this);
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




		//
		// Interface methods
		//


		// IEnumerable

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new Enumerator (this, EnumeratorMode.KEY_MODE);
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


		// IDictionary

		public virtual void Add (Object key, Object value)
		{
			PutImpl (key, value, false);
		}

		public virtual void Clear () 
		{
			for (int i = 0;i<table.Length;i++) {
				table [i].key = null;
				table [i].value = null;
				table [i].hashMix = 0;
			}

			inUse = 0;
			modificationCount++;
		}

		public virtual bool Contains (Object key)
		{
			return (Find (key) >= 0);
		}

		public virtual IDictionaryEnumerator GetEnumerator ()
		{
			return new Enumerator (this, EnumeratorMode.ENTRY_MODE);
		}

		public virtual void Remove (Object key)
		{
			int i = Find (key);
			Slot [] table = this.table;
			if (i >= 0) {
				int h = table [i].hashMix;
				h &= CHAIN_MARKER;
				table [i].hashMix = h;
				table [i].key = (h != 0)
				              ? REMOVED_MARKER
				              : null;
				table [i].value = null;
				--inUse;
				++modificationCount;
			}
		}




		public virtual bool ContainsKey (object key)
		{
			return Contains (key);
		}

		public virtual bool ContainsValue (object value)
		{
			int size = this.table.Length;
			Slot [] table = this.table;

			for (int i = 0; i < size; i++) {
				Slot entry = table [i];
				if (entry.key != null && entry.key!= REMOVED_MARKER
				    && value.Equals (entry.value)) {
					return true;
				}
			}
			return false;
		}


		// ICloneable

		public virtual object Clone ()
		{
			Hashtable ht = new Hashtable (Count, hcp, comparer);
			ht.modificationCount = this.modificationCount;
			ht.inUse = this.inUse;
			ht.AdjustThreshold ();

			// FIXME: maybe it's faster to simply
			//        copy the back-end array?

			IDictionaryEnumerator it = GetEnumerator ();
			while (it.MoveNext ()) {
				ht [it.Key] = it.Value;
			}

			return ht;
		}

		[MonoTODO]
		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			info.AddValue ("LoadFactor", loadFactor);
			info.AddValue ("Version", modificationCount);
			info.AddValue ("Comparer", comparerRef);
			info.AddValue ("HashCodeProvider", hcpRef);
			info.AddValue ("HashSize", this.Count );
			info.AddValue ("Keys", this.Keys);
			info.AddValue ("Values", this.Values);
		}

		//[MonoTODO]
		//public virtual void OnDeserialization (object sender);

		[MonoTODO]
		public override string ToString ()
		{
			// FIXME: What's it supposed to do?
			//        Maybe print out some internals here? Anyway.
			return "mono::System.Collections.Hashtable";
		}


		/// <summary>
		///  Returns a synchronized (thread-safe)
		///  wrapper for the Hashtable.
		/// </summary>
		public static Hashtable Synchronized (Hashtable table)
		{
			return new SynchedHashtable (table);
		}



		//
		// Protected instance methods
		//

		/// <summary>Returns the hash code for the specified key.</summary>
		protected virtual int GetHash (Object key)
		{
			IHashCodeProvider hcp = this.hcp;
			return (hcp!= null)
			        ? hcp.GetHashCode (key)
			        : key.GetHashCode ();
		}

		/// <summary>
		///  Compares a specific Object with a specific key
		///  in the Hashtable.
		/// </summary>
		protected virtual bool KeyEquals (Object item, Object key)
		{
			IComparer c = this.comparer;
			if (c!= null)
				return (c.Compare (item, key) == 0);
			else
				return item.Equals (key);
		}



		//
		// Private instance methods
		//

		private void AdjustThreshold ()
		{
			int size = table.Length;

			threshold = (int) (size*loadFactor);
			if (this.threshold >= size)
				threshold = size-1;
		}

		private void SetTable (Slot [] table)
		{
			if (table == null)
				throw new ArgumentNullException ("table");

			this.table = table;
			AdjustThreshold ();
		}

		private Object GetImpl (Object key)
		{
			int i = Find (key);

			if (i >= 0)
				return table [i].value;
			else
				return null;
		}


		private int Find (Object key)
		{
			if (key == null)
				throw new ArgumentNullException ("null key");

			uint size = (uint) this.table.Length;
			int h = this.GetHash (key) & Int32.MaxValue;
			uint spot = (uint)h;
			uint step = (uint) ((h >> 5)+1) % (size-1)+1;
			Slot[] table = this.table;

			for (int i = 0; i < size;i++) {
				int indx = (int) (spot % size);
				Slot entry = table [indx];
				Object k = entry.key;
				if (k == null)
					return -1;
				if ((entry.hashMix & Int32.MaxValue) == h
				    && this.KeyEquals (key, k)) {
					return indx;
				}

				if ((entry.hashMix & CHAIN_MARKER) == 0)
					return -1;

				spot+= step;
			}
			return -1;
		}


		private void Rehash ()
		{
			int oldSize = this.table.Length;

			// From the SDK docs:
			//   Hashtable is automatically increased
			//   to the smallest prime number that is larger
			//   than twice the current number of Hashtable buckets
			uint newSize = (uint)ToPrime ((oldSize<<1)|1);


			Slot [] newTable = new Slot [newSize];
			Slot [] table = this.table;

			for (int i = 0;i<oldSize;i++) {
				Slot s = table [i];
				if (s.key!= null) {
					int h = s.hashMix & Int32.MaxValue;
					uint spot = (uint)h;
					uint step = ((uint) (h>>5)+1)% (newSize-1)+1;
					for (uint j = spot%newSize;;spot+= step, j = spot%newSize) {
						// No check for REMOVED_MARKER here, 
						// because the table is just allocated.
						if (newTable [j].key == null) {
							newTable [j].key = s.key;
							newTable [j].value = s.value;
							newTable [j].hashMix |= h;
							break;
						} else {
							newTable [j].hashMix |= CHAIN_MARKER;
						}
					}
				}
			}

			++this.modificationCount;

			this.SetTable (newTable);
		}


		private void PutImpl (Object key, Object value, bool overwrite)
		{
			if (key == null)
				throw new ArgumentNullException ("null key");

			uint size = (uint)this.table.Length;
			if (this.inUse >= this.threshold) {
				this.Rehash ();
				size = (uint)this.table.Length;
			}

			int h = this.GetHash (key) & Int32.MaxValue;
			uint spot = (uint)h;
			uint step = (uint) ((spot>>5)+1)% (size-1)+1;
			Slot [] table = this.table;
			Slot entry;

			int freeIndx = -1;
			for (int i = 0; i < size; i++) {
				int indx = (int) (spot % size);
				entry = table [indx];

				if (freeIndx == -1
				    && entry.key == REMOVED_MARKER
				    && (entry.hashMix & CHAIN_MARKER)!= 0)
					freeIndx = indx;

				if (entry.key == null ||
				    (entry.key == REMOVED_MARKER
				     && (entry.hashMix & CHAIN_MARKER)!= 0)) {

					if (freeIndx == -1)
						freeIndx = indx;
					break;
				}

				if ((entry.hashMix & Int32.MaxValue) == h && KeyEquals (key, entry.key)) {
					if (overwrite) {
						table [indx].value = value;
						++this.modificationCount;
					} else {
						// Handle Add ():
						// An entry with the same key already exists in the Hashtable.
						throw new ArgumentException ("Key duplication");
					}
					return;
				}

				if (freeIndx == -1) {
					table [indx].hashMix |= CHAIN_MARKER;
				}

				spot+= step;

			}

			if (freeIndx!= -1) {
				table [freeIndx].key = key;
				table [freeIndx].value = value;
				table [freeIndx].hashMix |= h;

				++this.inUse;
				++this.modificationCount;
			}

		}

		private void  CopyToArray (Array arr, int i, 
					   EnumeratorMode mode)
		{
			IEnumerator it = new Enumerator (this, mode);

			while (it.MoveNext ()) {
				arr.SetValue (it.Current, i++);
			}
		}



		//
		// Private static methods
		//
		private static bool TestPrime (int x)
		{
			if ((x & 1) != 0) {
				for (int n = 3; n< (int)Math.Sqrt (x); n += 2) {
					if ((x % n) == 0)
						return false;
				}
				return true;
			}
			// There is only one even prime - 2.
			return (x == 2);
		}

		private static int CalcPrime (int x)
		{
			for (int i = (x & (~1))-1; i< Int32.MaxValue; i += 2) {
				if (TestPrime (i)) return i;
			}
			return x;
		}

		private static int ToPrime (int x)
		{
			for (int i = 0; i < primeTbl.Length; i++) {
				if (x <= primeTbl [i])
					return primeTbl [i];
			}
			return CalcPrime (x);
		}




		//
		// Inner classes
		//

		public enum EnumeratorMode : int {KEY_MODE = 0, VALUE_MODE, ENTRY_MODE};

		protected sealed class Enumerator : IDictionaryEnumerator, IEnumerator {

			private Hashtable host;
			private int stamp;
			private int pos;
			private int size;
			private EnumeratorMode mode;

			private Object currentKey;
			private Object currentValue;

			private readonly static string xstr = "Hashtable.Enumerator: snapshot out of sync.";

			public Enumerator (Hashtable host, EnumeratorMode mode) {
				this.host = host;
				stamp = host.modificationCount;
				size = host.table.Length;
				this.mode = mode;
				Reset ();
			}

			public Enumerator (Hashtable host)
			           : this (host, EnumeratorMode.KEY_MODE) {}


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

				if (pos < size) {
					while (++pos < size) {
						Slot entry = host.table [pos];

						if (entry.key != null && entry.key != REMOVED_MARKER) {
							currentKey = entry.key;
							currentValue = entry.value;
							return true;
						}
					}
				}

				currentKey = null;
				currentValue = null;
				return false;
			}

			public DictionaryEntry Entry
			{
				get {
					FailFast ();
					return new DictionaryEntry (currentKey, currentValue);
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
					switch (mode) {
					case EnumeratorMode.KEY_MODE:
						return currentKey;
					case EnumeratorMode.VALUE_MODE:
						return currentValue;
					case EnumeratorMode.ENTRY_MODE:
						return new DictionaryEntry (currentKey, currentValue);
					}
					throw new Exception ("should never happen");
				}
			}
		}



		protected class HashKeys : ICollection, IEnumerable {

			private Hashtable host;
			private int count;

			public HashKeys (Hashtable host) {
				if (host == null)
					throw new ArgumentNullException ();

				this.host = host;
				this.count = host.Count;
			}

			// ICollection

			public virtual int Count {
				get {
					return count;
				}
			}

			public virtual bool IsSynchronized {
				get {
					return host.IsSynchronized;
				}
			}

			public virtual Object SyncRoot {
				get {return host.SyncRoot;}
			}

			public virtual void CopyTo (Array array, int arrayIndex)
			{
				host.CopyToArray (array, arrayIndex, EnumeratorMode.KEY_MODE);
			}

			// IEnumerable

			public virtual IEnumerator GetEnumerator ()
			{
				return new Hashtable.Enumerator (host, EnumeratorMode.KEY_MODE);
			}
		}


		protected class HashValues : ICollection, IEnumerable {

			private Hashtable host;
			private int count;

			public HashValues (Hashtable host) {
				if (host == null)
					throw new ArgumentNullException ();

				this.host = host;
				this.count = host.Count;
			}

			// ICollection

			public virtual int Count {
				get {
					return count;
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

			// IEnumerable

			public virtual IEnumerator GetEnumerator ()
			{
				return new Hashtable.Enumerator (host, EnumeratorMode.VALUE_MODE);
			}
		}



		protected class SynchedHashtable : Hashtable, IEnumerable {

			private Hashtable host;

			public SynchedHashtable (Hashtable host) {
				if (host == null)
					throw new ArgumentNullException ();

				this.host = host;
			}

			// ICollection

			public override int Count {
				get {
					return host.Count;
				}
			}

			public override bool IsSynchronized {
				get {
					return true;
				}
			}

			public override Object SyncRoot {
				get {
					return host.SyncRoot;
				}
			}



			// IDictionary

			public override bool IsFixedSize {
				get {
					return host.IsFixedSize;
				}     
			}


			public override bool IsReadOnly {
				get {
					return host.IsReadOnly;
				}
			}

			public override ICollection Keys {
				get {
					ICollection keys = null;
					lock (host.SyncRoot) {
						keys = host.Keys;
					}
					return keys;
				}
			}

			public override ICollection Values {
				get {
					ICollection vals = null;
					lock (host.SyncRoot) {
						vals = host.Values;
					}
					return vals;
				}
			}



			public override Object this [Object key] {
				get {
					return host.GetImpl (key);
				}
				set {
					lock (host.SyncRoot) {
						host.PutImpl (key, value, true);
					}
				}
			}

			// IEnumerable

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return new Enumerator (host, EnumeratorMode.KEY_MODE);
			}




			// ICollection

			public override void CopyTo (Array array, int arrayIndex)
			{
				host.CopyTo (array, arrayIndex);
			}


			// IDictionary

			public override void Add (Object key, Object value)
			{
				lock (host.SyncRoot) {
					host.PutImpl (key, value, false);
				}
			}

			public override void Clear () 
			{
				lock (host.SyncRoot) {
					host.Clear ();
				}
			}

			public override bool Contains (Object key)
			{
				return (host.Find (key) >= 0);
			}

			public override IDictionaryEnumerator GetEnumerator ()
			{
				return new Enumerator (host, EnumeratorMode.ENTRY_MODE);
			}

			public override void Remove (Object key)
			{
				lock (host.SyncRoot) {
					host.Remove (key);
				}
			}



			public override bool ContainsKey (object key)
			{
				return host.Contains (key);
			}

			public override bool ContainsValue (object value)
			{
				return host.ContainsValue (value);
			}


			// ICloneable

			public override object Clone ()
			{
				return (host.Clone () as Hashtable);
			}

		} // SynchedHashtable


	} // Hashtable

}

