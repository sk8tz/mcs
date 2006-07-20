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
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//

using System;
using System.Data;
using System.Reflection;
using System.Collections;
using System.ComponentModel;

namespace System.Windows.Forms {
	public class CurrencyManager : BindingManagerBase {

		protected int listposition;
		protected Type finalType;

		private IList list;
		private bool binding_suspended;

		private object data_source;

		bool editing;

		internal CurrencyManager ()
		{
		}

		internal CurrencyManager (object data_source)
		{
			SetDataSource (data_source);
		}

		public IList List {
			get { return list; }
		}

		public override object Current {
			get {
				if (listposition == -1 || listposition >= list.Count)
					throw new IndexOutOfRangeException ("list position");
				return list [listposition];
			}
		}

		public override int Count {
			get { return list.Count; }
		}

		public override int Position {
			get {
				return listposition;
			} 
			set {
				if (value < 0)
					value = 0;
				if (value == list.Count)
					value = list.Count - 1;
				if (listposition == value)
					return;

				if (listposition != -1)
					EndCurrentEdit ();

				listposition = value;
				OnCurrentChanged (EventArgs.Empty);
				OnPositionChanged (EventArgs.Empty);
			}
		}

		[MonoTODO]
		internal void SetDataSource (object data_source)
		{
			if (this.data_source is IBindingList)
				((IBindingList)this.data_source).ListChanged -= new ListChangedEventHandler (ListChangedHandler);
			if (this.data_source is DataView) {
				DataView dataview = (DataView)this.data_source;
				dataview.Table.Columns.CollectionChanged  -= new CollectionChangeEventHandler (MetaDataChangedHandler);
				dataview.Table.ChildRelations.CollectionChanged  -= new CollectionChangeEventHandler (MetaDataChangedHandler);
				dataview.Table.ParentRelations.CollectionChanged  -= new CollectionChangeEventHandler (MetaDataChangedHandler);
				dataview.Table.Constraints.CollectionChanged -= new CollectionChangeEventHandler (MetaDataChangedHandler);
			}

			if (data_source is IListSource)
				data_source = ((IListSource)data_source).GetList();

			this.data_source = data_source;
			if (data_source != null)
				this.finalType = data_source.GetType();

			listposition = -1;
			if (this.data_source is IBindingList)
				((IBindingList)this.data_source).ListChanged += new ListChangedEventHandler (ListChangedHandler);
			if (this.data_source is DataView) {
				DataView dataview = (DataView)this.data_source;
				dataview.Table.Columns.CollectionChanged  += new CollectionChangeEventHandler (MetaDataChangedHandler);
				dataview.Table.ChildRelations.CollectionChanged  += new CollectionChangeEventHandler (MetaDataChangedHandler);
				dataview.Table.ParentRelations.CollectionChanged  += new CollectionChangeEventHandler (MetaDataChangedHandler);
				dataview.Table.Constraints.CollectionChanged += new CollectionChangeEventHandler (MetaDataChangedHandler);
			}

			list = (IList)data_source;

			// XXX this is wrong.  MS invokes OnItemChanged directly, which seems to call PushData.
			ListChangedHandler (null, new ListChangedEventArgs (ListChangedType.Reset, -1));
		}

		public override PropertyDescriptorCollection GetItemProperties ()
		{
			if (list is Array) {
				Type element = list.GetType ().GetElementType ();
				return TypeDescriptor.GetProperties (element);
			}

			if (list is ITypedList) {
				return ((ITypedList)list).GetItemProperties (null);
			}

			PropertyInfo [] props = data_source.GetType().GetProperties ();
			for (int i = 0; i < props.Length; i++) {
				if (props [i].Name == "Item") {
					Type t = props [i].PropertyType;
					if (t == typeof (object))
						continue;
					return GetBrowsableProperties (t);
				}
			}

			if (list.Count > 0) {
				return GetBrowsableProperties (list [0].GetType ());
			}
			
			return new PropertyDescriptorCollection (null);
		}

		public override void RemoveAt (int index)
		{
			list.RemoveAt (index);
		}

		public override void SuspendBinding ()
		{
			binding_suspended = true;
		}
		
		public override void ResumeBinding ()
		{
			binding_suspended = false;
		}

                internal override bool IsSuspended {
                        get { return binding_suspended; }
                }

                [MonoTODO ("this needs re-addressing once DataViewManager.AllowNew is implemented")]
                internal bool CanAddRows {
                	get {
				if (list is IBindingList) {
					return true;
					//return ((IBindingList)list).AllowNew;
				}

				return false;
			}
		}

		public override void AddNew ()
		{
			if (list as IBindingList == null)
				throw new NotSupportedException ();

			(list as IBindingList).AddNew ();

			EndCurrentEdit ();

			listposition = list.Count - 1;

			OnCurrentChanged (EventArgs.Empty);
			OnPositionChanged (EventArgs.Empty);
		}


		void BeginEdit ()
		{
			IEditableObject editable = Current as IEditableObject;

			if (editable != null) {
				try {
					editable.BeginEdit ();
					editing = true;
				}
				catch {
					/* swallow exceptions in IEditableObject.BeginEdit () */
				}
			}
		}

		public override void CancelCurrentEdit ()
		{
			if (listposition == -1)
				return;

			IEditableObject editable = Current as IEditableObject;

			if (editable != null) {
				editing = false;
				editable.CancelEdit ();
				OnItemChanged (new ItemChangedEventArgs (Position));
			}
		}
		
		public override void EndCurrentEdit ()
		{
			if (listposition == -1)
				return;

			IEditableObject editable = Current as IEditableObject;

			if (editable != null) {
				editing = false;
				editable.EndEdit ();
			}
		}

		public void Refresh ()
		{
			ListChangedHandler (null, new ListChangedEventArgs (ListChangedType.Reset, -1));
		}

		protected void CheckEmpty ()
		{
			if (list == null || list.Count < 1)
				throw new Exception ("List is empty.");
				
		}

		protected internal override void OnCurrentChanged (EventArgs e)
		{
			BeginEdit ();

			if (onCurrentChangedHandler != null) {
				onCurrentChangedHandler (this, e);
			}
		}

		protected virtual void OnItemChanged (ItemChangedEventArgs e)
		{
			if (ItemChanged != null)
				ItemChanged (this, e);
		}

		protected virtual void OnPositionChanged (EventArgs e)
		{
			if (onPositionChangedHandler != null)
				onPositionChangedHandler (this, e);
		}

		protected internal override string GetListName (ArrayList accessors)
		{
			if (list is ITypedList) {
				PropertyDescriptor [] pds = null;
				if (accessors != null) {
					pds = new PropertyDescriptor [accessors.Count];
					accessors.CopyTo (pds, 0);
				}
				return ((ITypedList) list).GetListName (pds);
			}
			else if (finalType != null) {
				return finalType.Name;
			}
			return String.Empty;
		}

		protected override void UpdateIsBinding ()
		{
			UpdateItem ();

			foreach (Binding binding in Bindings)
				binding.UpdateIsBinding ();
		}

		private void UpdateItem ()
		{
			// Probably should be validating or something here
			if (listposition != -1) {
				EndCurrentEdit ();
			}
			else if (list.Count > 0) {

				listposition = 0;
				
				BeginEdit ();
			}
		}
		
		internal object this [int index] {
			get { return list [index]; }
		}		
		
		private PropertyDescriptorCollection GetBrowsableProperties (Type t)
		{
			Attribute [] att = new System.Attribute [1];
			att [0] = new BrowsableAttribute (true);
			return TypeDescriptor.GetProperties (t, att);
		}

		private void MetaDataChangedHandler (object sender, CollectionChangeEventArgs e)
		{
			if (MetaDataChanged != null)
				MetaDataChanged (this, EventArgs.Empty);
		}

		private void ListChangedHandler (object sender, ListChangedEventArgs e)
		{
			switch (e.ListChangedType) {
			case ListChangedType.ItemDeleted:
				if (listposition == e.NewIndex) {
					listposition = e.NewIndex - 1;
					OnCurrentChanged (EventArgs.Empty);
					OnPositionChanged (EventArgs.Empty);
				}
					
				OnItemChanged (new ItemChangedEventArgs (-1));
				break;
			case ListChangedType.ItemAdded:
				if (listposition == -1) {
					listposition = e.NewIndex - 1;
					OnCurrentChanged (EventArgs.Empty);
					OnPositionChanged (EventArgs.Empty);
				}

				OnItemChanged (new ItemChangedEventArgs (-1));
				break;
			case ListChangedType.ItemChanged:
				if (editing)
					OnItemChanged (new ItemChangedEventArgs (e.NewIndex));
				break;
			default:
				PushData ();
				OnItemChanged (new ItemChangedEventArgs (-1));
				//				UpdateIsBinding ();
				break;
			}
		}

		public event ItemChangedEventHandler ItemChanged;
		public event EventHandler MetaDataChanged;
	}
}

