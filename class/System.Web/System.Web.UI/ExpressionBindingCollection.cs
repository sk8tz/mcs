//
// System.Web.UI.ExpressionBindingCollection.cs
//
// Authors:
// 	Sanjay Gupta gsanjay@novell.com)
//
// (C) 2004 Novell, Inc. (http://www.novell.com)
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
using System;
using System.Collections;

namespace System.Web.UI {
    	
	public sealed class ExpressionBindingCollection : ICollection, IEnumerable
    	{
		Hashtable list;
		ArrayList removed;
		
		public ExpressionBindingCollection ()
		{
			list = new Hashtable ();
			removed = new ArrayList ();
		}

		public int Count {
			get { return list.Count; }
		}

		public bool IsReadOnly {
			get { return list.IsReadOnly; }
		}

		public bool IsSynchronized {
			get { return list.IsSynchronized; }
		}

		public ExpressionBinding this [string propertyName] {
            		get { return list [propertyName] as ExpressionBinding; }
        	}

		public string [] RemovedBindings {
			get { return (string []) removed.ToArray (typeof (string)); }
		}

		public object SyncRoot {
			get { return list.SyncRoot; }
		}

		public void Add (ExpressionBinding binding)
		{
			list.Add (binding.PropertyName, binding);
            		OnChanged (new EventArgs ());
        	}

		public void Clear ()
		{
			list.Clear ();
            		removed.Clear ();
            		OnChanged (new EventArgs ());
        	}

        	public bool Contains (string propertyName)
        	{
            		return list.Contains (propertyName);
        	}

		public void CopyTo (Array array, int index)
		{
			list.CopyTo (array, index);
		}

        	public void CopyTo (ExpressionBinding [] bindings, int index)
        	{
            		if (index < 0)
                		throw new ArgumentNullException ("Index cannot be negative");
            		if (index >= bindings.Length)
                		throw new ArgumentException ("Index cannot be greater than or equal to length of array passed");            
            		if (list.Count > (bindings.Length - index + 1))
                		throw new ArgumentException ("Number of elements in source is greater than available space from index to end of destination");
            
            		foreach (string key in list.Keys)
                		bindings [index++] = (ExpressionBinding) list [key];
        	}

		public IEnumerator GetEnumerator ()
		{
			return list.GetEnumerator ();
		}

		public void Remove (ExpressionBinding binding)
		{
			Remove(binding.PropertyName, true);
        	}

		public void Remove (string propertyName)
		{
			Remove (propertyName, true);            
        	}

		public void Remove (string propertyName, bool addToRemovedList)
		{
			if (addToRemovedList)
				removed.Add (String.Empty); 
			else
				removed.Add (propertyName);

			list.Remove (propertyName);
            		OnChanged (new EventArgs ());
        	}

        	public event EventHandler Changed;

        	protected void OnChanged (EventArgs e)   
        	{
            		if (Changed != null)
                		Changed (this, e);
        	}        

    	}
}
#endif
