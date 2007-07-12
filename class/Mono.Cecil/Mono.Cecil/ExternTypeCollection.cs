//
// ExternTypeCollection.cs
//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Generated by /CodeGen/cecil-gen.rb do not edit
// Fri Mar 30 18:43:57 +0200 2007
//
// (C) 2005 Jb Evain
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

namespace Mono.Cecil {

	using System;
	using System.Collections;
	using System.Collections.Specialized;

	using Mono.Cecil.Cil;

	using Hcp = Mono.Cecil.HashCodeProvider;
	using Cmp = System.Collections.Comparer;

	public sealed class ExternTypeCollection : NameObjectCollectionBase, IList, IReflectionVisitable  {

		ModuleDefinition m_container;

		public TypeReference this [int index] {
			get { return this.BaseGet (index) as TypeReference; }
			set { this.BaseSet (index, value); }
		}

		public TypeReference this [string fullName] {
			get { return this.BaseGet (fullName) as TypeReference; }
			set { this.BaseSet (fullName, value); }
		}

		public ModuleDefinition Container {
			get { return m_container; }
		}

		public bool IsSynchronized {
			get { return false; }
		}

		public object SyncRoot {
			get { return this; }
		}

		bool IList.IsReadOnly {
			get { return false; }
		}

		bool IList.IsFixedSize {
			get { return false; }
		}

		object IList.this [int index] {
			get { return BaseGet (index); }
			set {
				Check (value);
				BaseSet (index, value);
			}
		}

		public ExternTypeCollection (ModuleDefinition container) :
			base (Hcp.Instance, Cmp.Default)
		{
			m_container = container;
		}

		public void Add (TypeReference value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			Attach (value);

			this.BaseAdd (value.FullName, value);
		}

		public void Clear ()
		{
			foreach (TypeReference item in this)
				Detach (item);

			this.BaseClear ();
		}

		public bool Contains (TypeReference value)
		{
			return Contains (value.FullName);
		}

		public bool Contains (string fullName)
		{
			return this.BaseGet (fullName) != null;
		}

		public int IndexOf (TypeReference value)
		{
			string [] keys = this.BaseGetAllKeys ();
			return Array.IndexOf (keys, value.FullName, 0, keys.Length);
		}

		public void Remove (TypeReference value)
		{
			this.BaseRemove (value.FullName);

			Detach (value);
		}

		public void RemoveAt (int index)
		{
			TypeReference item = this [index];
			Remove (item);

			Detach (item);
		}

		public void CopyTo (Array ary, int index)
		{
			this.BaseGetAllValues ().CopyTo (ary, index);
		}

		public new IEnumerator GetEnumerator ()
		{
			return this.BaseGetAllValues ().GetEnumerator ();
		}

		public void Accept (IReflectionVisitor visitor)
		{
			visitor.VisitExternTypeCollection (this);
		}

#if CF_1_0 || CF_2_0
		internal object [] BaseGetAllValues ()
		{
			object [] values = new object [this.Count];
			for (int i=0; i < values.Length; ++i) {
				values [i] = this.BaseGet (i);
			}
			return values;
		}
#endif

		void Check (object value)
		{
			if (!(value is TypeReference))
				throw new ArgumentException ();
		}

		int IList.Add (object value)
		{
			Check (value);
			Add (value as TypeReference);
			return 0;
		}

		bool IList.Contains (object value)
		{
			Check (value);
			return Contains (value as TypeReference);
		}

		int IList.IndexOf (object value)
		{
			throw new NotSupportedException ();
		}

		void IList.Insert (int index, object value)
		{
			throw new NotSupportedException ();
		}

		void IList.Remove (object value)
		{
			Check (value);
			Remove (value as TypeReference);
		}

		void Detach (TypeReference type)
		{
			type.Module = null;
		}

		void Attach (TypeReference type)
		{
			if (type.Module != null)
				throw new ReflectionException ("Type is already attached, clone it instead");

			type.Module = m_container;
		}
	}
}
