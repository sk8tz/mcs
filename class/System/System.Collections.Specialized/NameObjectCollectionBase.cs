/**
 * System.Collections.Specialized.NamaValueCollection class implementation
 * 
 * Author: Gleb Novodran
 */

using System;
using System.Collections;
using System.Runtime.Serialization;

namespace System.Collections.Specialized
{
	[Serializable]
	public abstract class NameObjectCollectionBase : ICollection, IEnumerable, ISerializable, IDeserializationCallback
	{
		private Hashtable m_ItemsContainer;
		/// <summary>
		/// Extends Hashtable based Items container to support storing null-key pairs
		/// </summary>
		private _Item m_NullKeyItem;
		private ArrayList m_ItemsArray;
		private IHashCodeProvider m_hashprovider;
		private IComparer m_comparer;
		private int m_defCapacity;
		private bool m_readonly;

		internal IComparer Comparer {
			get {return m_comparer;}
		}

		internal IHashCodeProvider HashCodeProvider {
			get {return m_hashprovider;}
		}

		internal protected /*?*/ class _Item
		{
			public string key;
			public object value;
			public _Item(string key, object value)
			{
				this.key = key;
				this.value = value;
			}
		}		
		/// <summary>
		/// Implements IEnumerable interface for KeysCollection
		/// </summary>
		[Serializable]
		internal protected /*?*/ class _KeysEnumerator : IEnumerator
		{
			private NameObjectCollectionBase m_collection;
			private int m_position;
			/*private*/internal _KeysEnumerator(NameObjectCollectionBase collection)
			{
				m_collection = collection;
				Reset();
			}
			public object Current 
			{
				
				get{
					if ((m_position<m_collection.Count)||(m_position<0))
						return m_collection.BaseGetKey(m_position);
					else 
						throw new InvalidOperationException();
				}
				
			}
			public bool MoveNext()
			{
				return ((++m_position)<m_collection.Count)?true:false;
			}
			public void Reset()
			{
				m_position = -1;
			}
		}
		
		/// <summary>
		/// SDK: Represents a collection of the String keys of a collection.
		/// </summary>
		public class KeysCollection : ICollection, IEnumerable
		{
			private NameObjectCollectionBase m_collection;

			internal/*protected?*/ KeysCollection(NameObjectCollectionBase collection)
			{
				this.m_collection = collection;
			}
			public virtual string Get( int index )
			{
				return m_collection.BaseGetKey(index);
				//throw new Exception("Not implemented yet");
			}
			
			// ICollection methods -----------------------------------
			/// <summary>
			/// 
			/// </summary>
			/// <param name="arr"></param>
			/// <param name="index"></param>
			public virtual void /*ICollection*/  CopyTo(Array arr, int index)
			{
				if (arr==null)
					throw new ArgumentNullException("array can't be null");
				IEnumerator en = this.GetEnumerator();
				int i = index;
				while (en.MoveNext())
				{
					arr.SetValue(en.Current,i);
					i++;
				}			
			}

			public virtual bool IsSynchronized
			{
				get{
					throw new Exception("Not implemented yet");
				}
			}
			public virtual object SyncRoot
			{
				get{
					throw new Exception("Not implemented yet");
				}
			}
			/// <summary>
			/// Gets the number of keys in the NameObjectCollectionBase.KeysCollection
			/// </summary>
			public int Count 
			{
				get{
					return m_collection.Count;
					//throw new Exception("Not implemented yet");
				}
			}
			// IEnumerable methods --------------------------------
			/// <summary>
			/// SDK: Returns an enumerator that can iterate through the NameObjectCollectionBase.KeysCollection.
			/// </summary>
			/// <returns></returns>
			public IEnumerator GetEnumerator()
			{
				return new _KeysEnumerator(m_collection);
//				throw new Exception("Not implemented yet");
			}
		}



		//--------------- Protected Instance Constructors --------------
		
		/// <summary>
		/// SDK: Initializes a new instance of the NameObjectCollectionBase class that is empty.
		/// </summary>
		[MonoTODO]
		protected NameObjectCollectionBase():base()
		{
			m_readonly = false;
			
			m_hashprovider = CaseInsensitiveHashCodeProvider.Default;
			m_comparer = CaseInsensitiveComparer.Default;
			m_defCapacity = 0;
			Init();
			/*m_ItemsContainer = new Hashtable(m_hashprovider,m_comparer);
			m_ItemsArray = new ArrayList();
			m_NullKeyItem = null;*/
			//TODO: consider common Reset() method
		}
		
		protected NameObjectCollectionBase( int capacity )
		{
			m_readonly = false;
			
			m_hashprovider = CaseInsensitiveHashCodeProvider.Default;
			m_comparer = CaseInsensitiveComparer.Default;
			m_defCapacity = capacity;
			Init();
			/*m_ItemsContainer = new Hashtable(m_defCapacity, m_hashprovider,m_comparer);
			m_ItemsArray = new ArrayList();
			m_NullKeyItem = null;			*/
			//throw new Exception("Not implemented yet");
		}
		protected NameObjectCollectionBase( IHashCodeProvider hashProvider, IComparer comparer )
		{
			m_readonly = false;
			
			m_hashprovider = hashProvider;
			m_comparer = comparer;
			m_defCapacity = 0;
			Init();
			/*m_ItemsContainer = new Hashtable(m_hashprovider,m_comparer);
			m_ItemsArray = new ArrayList();
			m_NullKeyItem = null;			*/
			//throw new Exception("Not implemented yet");
		}
		protected NameObjectCollectionBase( SerializationInfo info, StreamingContext context )
		{
			throw new Exception("Not implemented yet");
		}
		protected NameObjectCollectionBase( int capacity, IHashCodeProvider hashProvider, IComparer comparer )
		{
			m_readonly = false;
			
			m_hashprovider = hashProvider;
			m_comparer = comparer;
			m_defCapacity = capacity;
			Init();
			/*m_ItemsContainer = new Hashtable(m_defCapacity,m_hashprovider,m_comparer);
			m_ItemsArray = new ArrayList();
			m_NullKeyItem = null;	*/

			//throw new Exception("Not implemented yet");
		}
		
		private void Init(){
			m_ItemsContainer = new Hashtable(m_defCapacity,m_hashprovider,m_comparer);
			m_ItemsArray = new ArrayList();
			m_NullKeyItem = null;	
		}
		//--------------- Public Instance Properties -------------------
		public virtual NameObjectCollectionBase.KeysCollection Keys 
		{
			get
			{
				return new KeysCollection(this);
				//throw new Exception("Not implemented yet");
			}
		}
				
		//--------------- Public Instance Methods ----------------------
		// 
		/// <summary>
		/// SDK: Returns an enumerator that can iterate through the NameObjectCollectionBase.
		/// 
		/// <remark>This enumerator returns the keys of the collection as strings.</remark>
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
			return new _KeysEnumerator(this);
		}
		// GetHashCode
		
		// ISerializable
		public virtual void /*ISerializable*/ GetObjectData( SerializationInfo info, StreamingContext context )
		{
			throw new Exception("Not implemented yet");
		}

		// ICollection
		public virtual int Count 
		{
			get{
				return m_ItemsArray.Count;
				//throw new Exception("Not implemented yet");
			}
		}
		bool ICollection.IsSynchronized
		{
			get { return false; }
		}
		object ICollection.SyncRoot
		{
			get { return this; }
		}

		void ICollection.CopyTo (Array array, int index)
		{
			throw new NotImplementedException ();
		}
		

		// IDeserializationCallback
		public virtual void OnDeserialization( object sender)
		{
			throw new Exception("Not implemented yet");
		}
		
		//--------------- Protected Instance Properties ----------------
		/// <summary>
		/// SDK: Gets or sets a value indicating whether the NameObjectCollectionBase instance is read-only.
		/// </summary>
		protected bool IsReadOnly 
		{
			get{
				return m_readonly;
			}
			set{
				m_readonly=value;
			}
		}
		
		//--------------- Protected Instance Methods -------------------
		/// <summary>
		/// Adds an Item with the specified key and value into the <see cref="NameObjectCollectionBase"/>NameObjectCollectionBase instance.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="value"></param>
		protected void BaseAdd( string name, object value )
		{
			if (this.IsReadOnly)
				throw new NotSupportedException("Collection is read-only");
			
			_Item newitem=new _Item(name, value);

			if (name==null){
				//todo: consider nullkey entry
				if (m_NullKeyItem==null)
					m_NullKeyItem = newitem;
			}
			else
				if (m_ItemsContainer[name]==null){
					m_ItemsContainer.Add(name,newitem);
				}
			m_ItemsArray.Add(newitem);
			
//			throw new Exception("Not implemented yet");
		}
		protected void BaseClear()
		{
			if (this.IsReadOnly)
				throw new NotSupportedException("Collection is read-only");
			Init();
			//throw new Exception("Not implemented yet");
		}
		/// <summary>
		/// SDK: Gets the value of the entry at the specified index of the NameObjectCollectionBase instance.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		protected object BaseGet( int index )
		{
			return ((_Item)m_ItemsArray[index]).value;
			//throw new Exception("Not implemented yet");
		}
		/// <summary>
		/// SDK: Gets the value of the first entry with the specified key from the NameObjectCollectionBase instance.
		/// </summary>
		/// <remark>CAUTION: The BaseGet method does not distinguish between a null reference which is returned because the specified key is not found and a null reference which is returned because the value associated with the key is a null reference.</remark>
		/// <param name="name"></param>
		/// <returns></returns>
		protected object BaseGet( string name )
		{
			_Item item = FindFirstMatchedItem(name);
			/// CAUTION: The BaseGet method does not distinguish between a null reference which is returned because the specified key is not found and a null reference which is returned because the value associated with the key is a null reference.
			if (item==null)
				return null;
			else
				return item.value;
		}

		/// <summary>
		/// SDK:Returns a String array that contains all the keys in the NameObjectCollectionBase instance.
		/// </summary>
		/// <returns>A String array that contains all the keys in the NameObjectCollectionBase instance.</returns>
		protected string[] BaseGetAllKeys()
		{
			int cnt = m_ItemsArray.Count;
			string[] allKeys = new string[cnt];
			for(int i=0; i<cnt; i++)
				allKeys[i] = BaseGetKey(i);//((_Item)m_ItemsArray[i]).key;
			
			return allKeys;
		}

		/// <summary>
		/// SDK: Returns an Object array that contains all the values in the NameObjectCollectionBase instance.
		/// </summary>
		/// <returns>An Object array that contains all the values in the NameObjectCollectionBase instance.</returns>
		protected object[] BaseGetAllValues()
		{
			int cnt = m_ItemsArray.Count;
			object[] allValues = new object[cnt];
			for(int i=0; i<cnt; i++)
				allValues[i] = BaseGet(i);
			
			return allValues;
//			throw new Exception("Not implemented yet");
		}
		[MonoTODO]
		protected object[] BaseGetAllValues( Type type )
		{
			if (type == null)
				throw new ArgumentNullException("'type' argument can't be null");
			// TODO: implements this

			throw new Exception("Not implemented yet");
		}
		
		protected string BaseGetKey( int index )
		{
			return ((_Item)m_ItemsArray[index]).key;
			//throw new Exception("Not implemented yet");
		}
		/// <summary>
		/// Gets a value indicating whether the NameObjectCollectionBase instance contains entries whose keys are not a null reference 
		/// </summary>
		/// <returns>true if the NameObjectCollectionBase instance contains entries whose keys are not a null reference otherwise, false.</returns>
		protected bool BaseHasKeys()
		{
			return (m_ItemsContainer.Count>0);
//			throw new Exception("Not implemented yet");
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		[MonoTODO]
		protected void BaseRemove( string name )
		{
			int cnt = 0;
			String key;
			if (this.IsReadOnly)
				throw new NotSupportedException("Collection is read-only");
			if (name!=null)
			{
				m_ItemsContainer.Remove(name);
			}
			else {
				m_NullKeyItem = null;
			}
			
			cnt = m_ItemsArray.Count;
			for (int i=0 ; i< cnt; ){
				key=BaseGetKey(i);
				// TODO: consider case-sensivity 
				if (String.Compare(key,name)==0){
					m_ItemsArray.RemoveAt(i);
					cnt--;
				}
				else 
					i++;
				
			}
//			throw new Exception("Not implemented yet");
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="index"></param>
		/// <LAME>This function implemented the way Microsoft implemented it - 
		/// item is removed from hashtable and array without considering the case when there are two items with the same key but different values in array.
		/// E.g. if
		/// hashtable is [("Key1","value1")] and array contains [("Key1","value1")("Key1","value2")] then
		/// after RemoveAt(1) the collection will be in following state:
		/// hashtable:[] 
		/// array: [("Key1","value1")] 
		/// It's ok only then the key is uniquely assosiated with the value
		/// To fix it a comparsion of objects stored under the same key in the hashtable and in the arraylist should be added 
		/// </LAME>>
		[MonoTODO]
		protected void BaseRemoveAt( int index )
		{
			if (this.IsReadOnly)
				throw new NotSupportedException("Collection is read-only");
			string key = BaseGetKey(index);
			if (key!=null){
				// TODO: see LAME description above
				m_ItemsContainer.Remove(key);
			}
			else
				m_NullKeyItem = null;
			m_ItemsArray.RemoveAt(index);
//			throw new Exception("Not implemented yet");
		}
		/// <summary>
		/// SDK: Sets the value of the entry at the specified index of the NameObjectCollectionBase instance.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="value"></param>
		protected void BaseSet( int index, object value )
		{
			if (this.IsReadOnly)
				throw new NotSupportedException("Collection is read-only");
			_Item item = (_Item)m_ItemsArray[index];
			item.value = value;
			//throw new Exception("Not implemented yet");
		}
		/// <summary>
		/// Sets the value of the first entry with the specified key in the NameObjectCollectionBase instance, if found; otherwise, adds an entry with the specified key and value into the NameObjectCollectionBase instance.
		/// </summary>
		/// <param name="name">The String key of the entry to set. The key can be a null reference </param>
		/// <param name="value">The Object that represents the new value of the entry to set. The value can be a null reference</param>
		protected void BaseSet( string name, object value )
		{
			if (this.IsReadOnly)
				throw new NotSupportedException("Collection is read-only");
			_Item item = FindFirstMatchedItem(name);
			if (item!=null)
				item.value=value;
			else 
				BaseAdd(name, value);

			//throw new Exception("Not implemented yet");
		}
		[MonoTODO]
		private _Item FindFirstMatchedItem(string name)
		{
			if (name!=null)
 				return (_Item)m_ItemsContainer[name];
			else {
				//TODO: consider null key case
				return m_NullKeyItem;
				//throw new Exception("Not implemented yet");
			}

		}
		//~Object();
		
	}
}
