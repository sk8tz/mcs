//
// Collection.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//
// (C) 2002 Chris J Breisch
//
using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.ComponentModel;


namespace Microsoft.VisualBasic {
	sealed public class Collection : ICollection, IList {
	//
	// <summary>
	//		Collection : The BASIC Collection Object
	//	</summary>
	//
	//	<remarks>
	//	</remarks>
		// Declarations
		private Hashtable m_Hashtable = new Hashtable();
		private ArrayList m_HashIndexers = new ArrayList();
		internal bool Modified = false;

		private class ColEnumerator: IEnumerator 
		//
		// <summary>
		//		ColEnumerator : This internal class is used
		//		for enumerating through our Collection
		//	</summary>
		//
		//	<remarks>
		//	</remarks>
		{
			private int Index;
			private Collection Col;
			private object Item;

			public ColEnumerator(Collection coll) 
			{
				Col = coll;
				Index = 0;
				Col.Modified = false;
			}

			public void Reset() 
			{
				if (Col.Modified) 
				{
					// FIXME : spec says throw exception, MS doesn't
					// throw new InvalidOperationException();
				}
				Index = 0;
			}

			public bool MoveNext() 
			{
				if (Col.Modified) 
				{
					// FIXME : spec says throw exception, MS doesn't
					// throw new InvalidOperationException();
				}
				Index++;
				try {
					Item = Col[Index];
				}
				catch {
					// do nothing
				}

				return Index <= Col.Count;
			}

			public object Current {
				get {
					if (Index == 0) {
						// FIXME : spec says throw InvalidOperation, 
						// but MS throws IndexOutOfRange
						throw new IndexOutOfRangeException();
						// throw new InvalidOperationException();
					}
					else {
						return Item;
					}
				}
			}

			// The current property on the IEnumerator interface:
			object IEnumerator.Current {
				get {
					return(Current);
				}
			}
		}
		// Constructors
		// Properties
		System.Boolean IList.IsReadOnly {
			get {return false;}
		}

		System.Boolean ICollection.IsSynchronized {
			get {return m_Hashtable.IsSynchronized;}
		}

		System.Object ICollection.SyncRoot {
			get {return m_Hashtable.SyncRoot;}
		}

		System.Boolean IList.IsFixedSize { 
			get {return false;} 
		}

		public System.Int32 Count {
			get {return m_HashIndexers.Count;} 
		}

		[ReadOnly(true)]
		[System.Runtime.CompilerServices.IndexerName("Item")] 
		public System.Object this [System.Int32 Index] {
			get {
				try {
					// Collections are 1-based
					return m_Hashtable[m_HashIndexers[Index-1]];
				}
				catch {
					throw new IndexOutOfRangeException();
				}
			}
			set {throw new NotImplementedException();} 
		}

		[System.Runtime.CompilerServices.IndexerName("Item")] 
		public System.Object this [System.Object Index] 
		{
			get	{
				if (Index is string) {
					if (m_HashIndexers.IndexOf(Index) < 0) {
						//	throw new IndexOutOfRangeException();
						// FIXME : Spec Says IndexOutOfRange...MS throws Argument
						throw new ArgumentException();
					}
					return m_Hashtable[Index];
				} 
				else {
					throw new ArgumentException();
				}
			}
		}

		// Methods
		System.Int32 IList.IndexOf (System.Object value) 
		{ 
			int LastPos = 0;
			bool Found = false;

			while (!Found) {
				LastPos = m_HashIndexers.IndexOf(value.GetHashCode(), LastPos);
				if (LastPos == -1) {
					Found = true;
				} else if (m_Hashtable[m_HashIndexers[LastPos]] == value) {
					Found = true;
				}
			}
			return LastPos;
		}

		System.Boolean IList.Contains (System.Object value) 
		{ 
			return m_Hashtable.ContainsValue(value);
		}

		void IList.Clear () 
		{
			m_Hashtable.Clear();
			m_HashIndexers.Clear();
		}

		public void Remove (System.String Key) 
		{ 
			int Index;
			
			try {
				Index = m_HashIndexers.IndexOf(Key) + 1;
				Remove(Index);
			}
			catch {
				throw new ArgumentException();
			}
		}

		public void Remove (System.Int32 Index)
		{ 
			try {
				// Collections are 1-based
				m_Hashtable.Remove(m_HashIndexers[Index-1]);
				m_HashIndexers.RemoveAt(Index-1);
				Modified = true;
			} 
			catch {
				throw new IndexOutOfRangeException();
			}
		}

		void IList.Remove (System.Object value)	
		{
			if (!(value is string)) {
				throw new ArgumentException();
			}
			Remove((string)value);
		}

		void IList.RemoveAt (System.Int32 index) 
		{
			Remove(index);
		}

		void IList.Insert (System.Int32 index, System.Object value) 
		{
			Insert(index, value, value.GetHashCode().ToString());
		}

		void Insert(System.Int32 index, System.Object value, string Key)
		{
			m_HashIndexers.Insert(index -1, Key);
			m_Hashtable.Add(Key, value);
			Modified = true;
		}

		System.Int32 IList.Add (System.Object Item) 
		{
			return Add(Item, Item.GetHashCode().ToString());
		}

		System.Int32 Add(System.Object Item, string Key)
		{
			m_Hashtable.Add(Key, Item);
			Modified = true;

			return m_HashIndexers.Add(Key);
		}

		private int GetIndexPosition(System.Object Item) 
		{
			int Position = int.MinValue;

			if (Item is string) {
				Position = m_HashIndexers.IndexOf(Item) + 1;
			} 
			else if (Item is int) {
				Position = Convert.ToInt32(Item);
			}
			else {
				throw new InvalidCastException();
			}
			if (Position < 0) {
				throw new ArgumentException();
			}
			return Position;
		}

		public void Add (System.Object Item, 
			[Optional] [DefaultValue(null)] String Key, 
			[Optional] [DefaultValue(null)] System.Object Before, 
			[Optional] [DefaultValue(null)] System.Object After)
		{
			int Position = int.MinValue;
			
			// check for valid args
			if (Before != null && After != null) {
				throw new ArgumentException();
			}
			if (Key != null && m_HashIndexers.IndexOf(Key) != -1) {
				throw new ArgumentException();
			}
			if (Before != null) {
				Position = GetIndexPosition(Before);
			}
			if (After != null) {
				Position = GetIndexPosition(After) + 1;
			}
			if (Key == null) {
				Key = Item.GetHashCode().ToString();
			}

			if (Position > (m_HashIndexers.Count+1) || Position == int.MinValue) {
				Add(Item, Key);
			} 
			else {
				Insert(Position, Item, Key);
			}
		}

		void ICollection.CopyTo (System.Array array, System.Int32 index) 
		{
			System.Array NewArray = 
				Array.CreateInstance(typeof(System.Object), 
					m_HashIndexers.Count - index);
			
			// Collections are 1-based
			for (int i = index -1; i < m_HashIndexers.Count; i++) {
				NewArray.SetValue(m_Hashtable[m_HashIndexers[i]], i - index);
			}
		}
		
		public IEnumerator GetEnumerator () 
		{
			return new ColEnumerator(this);
		}
	};
}
