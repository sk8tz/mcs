//
// System.Diagnostics.PerformanceCounterPermissionEntryCollection.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002
// (C) 2003 Andreas Nahr
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

using System;
using System.Diagnostics;
using System.Collections;
using System.Globalization;
using System.Security.Permissions;

namespace System.Diagnostics {

	[Serializable]
	public class PerformanceCounterPermissionEntryCollection : CollectionBase 
	{
		internal PerformanceCounterPermissionEntryCollection (ResourcePermissionBaseEntry[] entries)
		{
			foreach (ResourcePermissionBaseEntry entry in entries) {
				List.Add (new PerformanceCounterPermissionEntry ((PerformanceCounterPermissionAccess) entry.PermissionAccess, entry.PermissionAccessPath[0], entry.PermissionAccessPath[1]));
			}	
		}

		public PerformanceCounterPermissionEntry this [int index] {
			get {
				return (PerformanceCounterPermissionEntry)
					InnerList[index];
			}
			set {InnerList[index] = value;}
		}

		public int Add (PerformanceCounterPermissionEntry value)
		{
			return InnerList.Add (value);
		}

		public void AddRange (PerformanceCounterPermissionEntry[] value)
		{
			foreach (PerformanceCounterPermissionEntry e in value)
				Add (e);
		}

		public void AddRange (
			PerformanceCounterPermissionEntryCollection value)
		{
			foreach (PerformanceCounterPermissionEntry e in value)
				Add (e);
		}

		public bool Contains (PerformanceCounterPermissionEntry value)
		{
			return InnerList.Contains (value);
		}

		public void CopyTo (PerformanceCounterPermissionEntry[] array,
			int index)
		{
			InnerList.CopyTo (array, index);
		}

		public int IndexOf (PerformanceCounterPermissionEntry value)
		{
			return InnerList.IndexOf (value);
		}

		public void Insert (int index, 
			PerformanceCounterPermissionEntry value)
		{
			InnerList.Insert (index, value);
		}

		protected override void OnClear ()
		{
		}

		protected override void OnInsert (int index, object value)
		{
			if (!(value is PerformanceCounterPermissionEntry))
				throw new NotSupportedException (Locale.GetText(
					"You can only insert " +
					"PerformanceCounterPermissionEntry " +
					"objects into the collection."));
		}

		protected override void OnRemove (int index, object value)
		{
		}

		protected override void OnSet (int index, 
			object oldValue, 
			object newValue)
		{
			if (!(newValue is PerformanceCounterPermissionEntry))
				throw new NotSupportedException (Locale.GetText(
					"You can only insert " +
					"PerformanceCounterPermissionEntry " +
					"objects into the collection."));
		}

		public void Remove (PerformanceCounterPermissionEntry value)
		{
			InnerList.Remove (value);
		}
	}
}

