//
// System.Data.ObjectSpaces.ObjectList.cs
//
// Author:
//   Mark Easton (mark.easton@blinksoftware.co.uk)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) BLiNK Software Ltd.  http://www.blinksoftware.co.uk
// Copyright (C) Tim Coleman, 2003-2004
//

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

using System.Collections;
using System.Reflection;

namespace System.Data.ObjectSpaces
{
        [MonoTODO]
        public class ObjectList : ICollection, IEnumerable, IList
        {
		#region Fields

               	IList list;

		#endregion // Fields

		#region Constructors

                public ObjectList () 
			: this (typeof (ArrayList), null) 
		{
		}

                public ObjectList (Type type, object[] parameters)
                {
			if (type == null)
				throw new ObjectException ();

			bool isIList = false;
			foreach (Type t in type.GetInterfaces ())
				if (t.Equals (typeof (IList))) {
					isIList = true;
					break;
				}

			if (!isIList)
                                throw new ObjectException ();

			Type[] types = Type.EmptyTypes;
			if (parameters != null)
				types = Type.GetTypeArray (parameters);

			ConstructorInfo ci = type.GetConstructor (types);
			list = (IList) ci.Invoke (parameters);
                }

		#endregion // Constructors

		#region Properties

                public int Count {
                        get { return InnerList.Count; }
                }     

		bool ICollection.IsSynchronized {	
			get { return InnerList.IsSynchronized; }
		}

		object ICollection.SyncRoot {
			get { return InnerList.SyncRoot; }
		}
                
                public IList InnerList {
			get { return list; }
                }        
                
                public bool IsFixedSize {
			get { return InnerList.IsFixedSize; }
                }  

                public bool IsReadOnly {
			get { return InnerList.IsReadOnly; }
                }
                
                public object this [int index] {
			get { return InnerList [index]; }
			set { InnerList [index] = value; }
                }

		#endregion // Properties

		#region Methods
                
                public int Add (object value)
                {
			return InnerList.Add (value);
                }
                
                public void Clear () 
		{
			InnerList.Clear ();
		}
                
                public bool Contains (object value)
                {
			return InnerList.Contains (value);
                }
                
                public void CopyTo (Array array, int index) 
		{
			InnerList.CopyTo (array, index);
		}
                
                public IEnumerator GetEnumerator ()
                {
			return InnerList.GetEnumerator (); 
                }
                
                public int IndexOf (object value)
                {
			return InnerList.IndexOf (value);
                }
                
                public void Insert (int index, object value) 
		{
			InnerList.Insert (index, value);
		}
                
                public void Remove (object value) 
		{
			InnerList.Remove (value);
		}
                
                public void RemoveAt (int index) 
		{
			InnerList.RemoveAt (index);
		}

		#endregion // Methods
        }
}

#endif
