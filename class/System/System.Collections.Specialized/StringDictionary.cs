//
// System.Collections.Specialized.StringDictionary.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.ComponentModel.Design.Serialization;

namespace System.Collections.Specialized {

#if NET_2_0
	[Serializable]
#endif
	[DesignerSerializer ("System.Diagnostics.Design.StringDictionaryCodeDomSerializer, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.Serialization.CodeDomSerializer, " + Consts.AssemblySystem_Design)]
	public class StringDictionary : IEnumerable
	{
		private Hashtable table;
			
		public StringDictionary()
		{
			table = new Hashtable();
		}
		
		// Public Instance Properties
		
		public virtual int Count
		{
			get {
				return table.Count;
			}
		}
		
		public virtual bool IsSynchronized
		{
			get {
				return false;
			}
		}
		
		public virtual string this[string key]
		{
			get {
#if NET_2_0
			if (key == null)
				throw new ArgumentNullException ("key");
#endif
				return (string) table[key.ToLower()];
			}
			
			set {
#if NET_2_0
			if (key == null)
				throw new ArgumentNullException ("key");
#endif
				table[key.ToLower()] = value;
			}
		}
		
		public virtual ICollection Keys
		{
			get {
				return table.Keys;
			}
		}
		
		public virtual ICollection Values
		{
			get {
				return table.Values;
			}
		}
		
		public virtual object SyncRoot
		{
			get {
				return table.SyncRoot;
			}
		}
		
		// Public Instance Methods
		
		public virtual void Add(string key, string value)
		{
#if NET_2_0
			if (key == null)
				throw new ArgumentNullException ("key");
#endif
			table.Add(key.ToLower(), value);
		}
		
		public virtual void Clear()
		{
			table.Clear();
		}
		
		public virtual bool ContainsKey(string key)
		{
#if NET_2_0
			if (key == null)
				throw new ArgumentNullException ("key");
#endif
			return table.ContainsKey(key.ToLower());
		}
		
		public virtual bool ContainsValue(string value)
		{
			return table.ContainsValue(value);
		}
		
		public virtual void CopyTo(Array array, int index)
		{
			table.CopyTo(array, index);
		}
		
		public virtual IEnumerator GetEnumerator()
		{
			return table.GetEnumerator();
		}
		
		public virtual void Remove(string key)
		{
#if NET_2_0
			if (key == null)
				throw new ArgumentNullException ("key");
#endif
			table.Remove(key.ToLower());
		}
	}
}
