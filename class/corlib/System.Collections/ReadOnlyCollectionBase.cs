//
// System.Collections.ReadOnlyCollectionBase.cs
//
// Author:
//   Nick Drochak II (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak II
//

using System;

namespace System.Collections {

	[Serializable]
	public abstract class ReadOnlyCollectionBase : ICollection,	IEnumerable {

		// private instance properties
		private System.Collections.ArrayList list;
		
		// public instance properties
		public int Count { get { return InnerList.Count; } }
		
		// Public Instance Methods
		public System.Collections.IEnumerator GetEnumerator() { return InnerList.GetEnumerator(); }
		
		// Protected Instance Constructors
		protected ReadOnlyCollectionBase() {
			this.list = new System.Collections.ArrayList();
		}
		
		// Protected Instance Properties
		protected System.Collections.ArrayList InnerList {get { return this.list; } }
		
		// ICollection methods
		void ICollection.CopyTo(Array array, int index) {
			lock (InnerList) { InnerList.CopyTo(array, index); }
		}
		object ICollection.SyncRoot {
				get { return InnerList.SyncRoot; }
			}
		bool ICollection.IsSynchronized {
			get { return InnerList.IsSynchronized; }
		}
	}
}
