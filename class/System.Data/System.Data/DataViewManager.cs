//
// System.Data.DataViewManager
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//   Atsushi Enomoto (atsushi@ximian.com)
//
// (C) Ximian, Inc. 2002
// Copyright (C) Tim Coleman, 2002
// Copyright (C) 2005 Novell Inc,
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Xml;

namespace System.Data
{
	/// <summary>
	/// Contains a default DataViewSettingCollection for each DataTable in a DataSet.
	/// </summary>
	//[Designer]
	[DesignerAttribute ("Microsoft.VSDesigner.Data.VS.DataViewManagerDesigner, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.ComponentModel.Design.IDesigner")]
	public class DataViewManager : MarshalByValueComponent, IBindingList, ICollection, IList, ITypedList, IEnumerable
	{
		#region Fields

		DataSet dataSet;
		DataViewManagerListItemTypeDescriptor descriptor;
		DataViewSettingCollection settings;
		string xml;

		#endregion // Fields

		#region Constructors

		public DataViewManager ()
			: this (null)
		{
		}

		public DataViewManager (DataSet ds)
		{
			// Null argument is allowed here.
			SetDataSet (ds);
		}

		#endregion // Constructors

		#region Properties

		[DataSysDescription ("Indicates the source of data for this DataViewManager.")]
		[DefaultValue (null)]
		public DataSet DataSet {
			get { return dataSet; }
			set {
				if (value == null)
					throw new DataException ("Cannot set null DataSet.");
				SetDataSet (value);
			}
		}

		public string DataViewSettingCollectionString {
			get { return xml; }
			set {
				try {
					ParseSettingString (value);
					xml = BuildSettingString ();
				} catch (XmlException ex) {
					throw new DataException ("Cannot set DataViewSettingCollectionString.", ex);
				}
			}
		}

		[DataSysDescription ("Indicates the sorting/filtering/state settings for any table in the corresponding DataSet.")]
                [DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public DataViewSettingCollection DataViewSettings {
			get { return settings; }
		}

		int ICollection.Count {
			get { return settings.Count; }
		}

		bool ICollection.IsSynchronized {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		object ICollection.SyncRoot {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		bool IList.IsFixedSize {
			get { return true; }
		}

		bool IList.IsReadOnly {
			get { return true; }
		}

		object IList.this [int index] {
			get { 
				if (descriptor == null)
					descriptor = new DataViewManagerListItemTypeDescriptor (this);

				return descriptor;
			}

			set { throw new ArgumentException ("Not modifiable"); }
		}

		bool IBindingList.AllowEdit {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		bool IBindingList.AllowNew {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		bool IBindingList.AllowRemove {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		bool IBindingList.IsSorted {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		ListSortDirection IBindingList.SortDirection {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		PropertyDescriptor IBindingList.SortProperty {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		bool IBindingList.SupportsChangeNotification {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		bool IBindingList.SupportsSearching {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		bool IBindingList.SupportsSorting {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods
		private void SetDataSet (DataSet ds)
		{
			dataSet = ds;
			settings = new DataViewSettingCollection (this);
			xml = BuildSettingString ();
		}

		private void ParseSettingString (string source)
		{
			XmlTextReader xtr = new XmlTextReader (source,
				XmlNodeType.Element, null);

			xtr.ReadStartElement ("DataViewSettingCollectionString");
			if (xtr.IsEmptyElement)
				return; // MS does not change the value.

			ArrayList result = new ArrayList ();
			xtr.Read ();
			do {
				xtr.MoveToContent ();
				if (xtr.NodeType == XmlNodeType.EndElement)
					break;
				if (xtr.NodeType == XmlNodeType.Element)
					ReadTableSetting (xtr);
				else
					xtr.Skip ();
			} while (!xtr.EOF);
			if (xtr.NodeType == XmlNodeType.EndElement)
				xtr.ReadEndElement ();
		}

		private void ReadTableSetting (XmlReader reader)
		{
			// Namespace is ignored BTW.
			DataTable dt = DataSet.Tables [XmlConvert.DecodeName (
				reader.LocalName)];
			// The code below might result in NullReference error.
			DataViewSetting s = settings [dt];
			string sort = reader.GetAttribute ("Sort");
			if (sort != null)
				s.Sort = sort.Trim ();
			string ads = reader.GetAttribute ("ApplyDefaultSort");
			if (ads != null && ads.Trim () == "true")
				s.ApplyDefaultSort = true;
			string rowFilter = reader.GetAttribute ("RowFilter");
			if (rowFilter != null)
				s.RowFilter = rowFilter.Trim ();
			string rsf = reader.GetAttribute ("RowStateFilter");
			if (rsf != null)
				s.RowStateFilter = (DataViewRowState)
					Enum.Parse (typeof (DataViewRowState), 
					rsf.Trim ());

			reader.Skip ();
		}

		private string BuildSettingString ()
		{
			if (dataSet == null)
				return String.Empty;

			StringWriter sw = new StringWriter ();
			XmlTextWriter xw = new XmlTextWriter (sw);
			xw.WriteStartElement (
				"DataViewSettingCollectionString");
			foreach (DataViewSetting s in DataViewSettings) {
				xw.WriteStartElement (XmlConvert.EncodeName (
						s.Table.TableName));
				xw.WriteAttributeString ("Sort", s.Sort);
				// LAMESPEC: MS.NET does not seem to handle this property as expected.
				if (s.ApplyDefaultSort)
					xw.WriteAttributeString ("ApplyDefaultSort", "true");
				xw.WriteAttributeString ("RowFilter", s.RowFilter);
				xw.WriteAttributeString ("RowStateFilter", s.RowStateFilter.ToString ());
				xw.WriteEndElement ();
			}
			xw.WriteFullEndElement ();
			xw.Flush ();
			return sw.ToString ();
		}

		public DataView CreateDataView (DataTable table) 
		{
			DataViewSetting s = settings [table];
			return new DataView (table, s.Sort, s.RowFilter, s.RowStateFilter);
		}

		[MonoTODO]
		void IBindingList.AddIndex (PropertyDescriptor property)
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		object IBindingList.AddNew ()
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		void IBindingList.ApplySort (PropertyDescriptor property, ListSortDirection direction)
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		int IBindingList.Find (PropertyDescriptor property, object key)
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		void IBindingList.RemoveIndex (PropertyDescriptor property)
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		void IBindingList.RemoveSort ()
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		void ICollection.CopyTo (Array array, int index)
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		IEnumerator IEnumerable.GetEnumerator ()
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		int IList.Add (object value)
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		void IList.Clear ()
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		bool IList.Contains (object value)
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		int IList.IndexOf (object value)
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		void IList.Insert (int index, object value)
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		void IList.Remove (object value)
		{
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		void IList.RemoveAt (int index)
		{
			throw new NotImplementedException ();
		}
	
		PropertyDescriptorCollection ITypedList.GetItemProperties (PropertyDescriptor[] listAccessors)
		{
			if (dataSet == null)
				throw new DataException ("dataset is null");

			if (listAccessors == null || listAccessors.Length == 0) {
				ICustomTypeDescriptor desc = new DataViewManagerListItemTypeDescriptor (this);
				return desc.GetProperties ();
			}
				
			throw new NotImplementedException ();
		}
	
		[MonoTODO]
		string ITypedList.GetListName (PropertyDescriptor[] listAccessors)
		{
			throw new NotImplementedException ();
		}
	
		protected virtual void OnListChanged (ListChangedEventArgs e) 
		{
			if (ListChanged != null)
				ListChanged (this, e);
		}

		protected virtual void RelationCollectionChanged (object sender, CollectionChangeEventArgs e) 
		{
		}

		protected virtual void TableCollectionChanged (object sender, CollectionChangeEventArgs e) 
		{
		}

		#endregion // Methods

		#region Events

		public event ListChangedEventHandler ListChanged;

		#endregion // Events
	}
}
