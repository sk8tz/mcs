//
// XmlAnyElementAttributes.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//
// (C) 2002 John Donagher
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

using System.Collections;
using System.Collections.Generic;

namespace System.Xml.Serialization
{
	/// <summary>
	/// Summary description for XmlAnyElementAttributes.
	/// </summary>

	public class XmlAnyElementAttributes : CollectionBase {

		public XmlAnyElementAttribute this[int index] 
		{
			get 
			{
				return (XmlAnyElementAttribute)List[index];
			}
			set 
			{
				List[index] = value;
			}	
		}

		public int Add(XmlAnyElementAttribute attribute)
		{
			return (List as IList).Add (attribute);
		}

		public bool Contains(XmlAnyElementAttribute attribute)
		{
			return List.Contains(attribute);	
		}

		public int IndexOf(XmlAnyElementAttribute attribute)
		{
			return List.IndexOf(attribute);
		}

		public void Insert(int index, XmlAnyElementAttribute attribute)
		{
			List.Insert(index, attribute);
		}

		public void Remove(XmlAnyElementAttribute attribute)
		{
			List.Remove(attribute);
		}

		public void CopyTo(XmlAnyElementAttribute[] array,int index)
		{
			List.CopyTo(array, index);
		}
		
		internal void AddKeyHash (System.Text.StringBuilder sb)
		{
			if (Count == 0) return;
			
			sb.Append ("XAEAS ");
			for (int n=0; n<Count; n++)
				this[n].AddKeyHash (sb);
			sb.Append ('|');
		}

		internal int Order {
			get {
				foreach (XmlAnyElementAttribute e in this)
					if (e.Order >= 0)
						return e.Order;
				return -1;
			}
		}
	}
}
