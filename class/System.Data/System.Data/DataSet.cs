// 
// System.Data/DataSet.cs
//
// Author:
//   Christopher Podurgiel <cpodurgiel@msn.com>
//   Daniel Morgan <danmorg@sc.rr.com>
//   Rodrigo Moya <rodrigo@ximian.com>
//   Stuart Caborn <stuart.caborn@virgin.net>
//   Tim Coleman (tim@timcoleman.com)
//   Ville Palo <vi64pa@koti.soon.fi>
//
// (C) Ximian, Inc. 2002
// Copyright (C) Tim Coleman, 2002, 2003
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Data.Common;

namespace System.Data {

	[ToolboxItem (false)]
	[DefaultProperty ("DataSetName")]
	[Serializable]
	public class DataSet : MarshalByValueComponent, IListSource, 
		ISupportInitialize, ISerializable, IXmlSerializable 
	{
		private string dataSetName;
		private string _namespace = "";
		private string prefix;
		private bool caseSensitive;
		private bool enforceConstraints = true;
		private DataTableCollection tableCollection;
		private DataRelationCollection relationCollection;
		private PropertyCollection properties;
		private DataViewManager defaultView;
		private CultureInfo locale = System.Threading.Thread.CurrentThread.CurrentCulture;
		
		#region Constructors

		public DataSet () : this ("NewDataSet") 
		{		
		}
		
		public DataSet (string name)
		{
			dataSetName = name;
			tableCollection = new DataTableCollection (this);
			relationCollection = new DataRelationCollection.DataSetRelationCollection (this);
			properties = new PropertyCollection ();
			this.prefix = String.Empty;
			
			this.Locale = CultureInfo.CurrentCulture;
		}

		[MonoTODO]
		protected DataSet (SerializationInfo info, StreamingContext context) : this ()
		{
			throw new NotImplementedException ();
		}

		#endregion // Constructors

		#region Public Properties

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates whether comparing strings within the DataSet is case sensitive.")]
		[DefaultValue (false)]
		public bool CaseSensitive {
			get {
				return caseSensitive;
			} 
			set {
				foreach (DataTable T in Tables) {
					if (T.VirginCaseSensitive)
						T.CaseSensitive = value;
				}

				caseSensitive = value; 
				if (!caseSensitive) {
					foreach (DataTable table in Tables) {
						foreach (Constraint c in table.Constraints)
							c.AssertConstraint ();
					}
				}
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("The name of this DataSet.")]
		[DefaultValue ("")]
		public string DataSetName {
			get { return dataSetName; } 
			set { dataSetName = value; }
		}

		[DataSysDescription ("Indicates a custom \"view\" of the data contained by the DataSet. This view allows filtering, searching, and navigating through the custom data view.")]
		[Browsable (false)]
		public DataViewManager DefaultViewManager {
			get {
				if (defaultView == null)
					defaultView = new DataViewManager (this);
				return defaultView;
			} 
		}

		[DataSysDescription ("Indicates whether constraint rules are to be followed.")]
		[DefaultValue (true)]
		public bool EnforceConstraints {
			get { return enforceConstraints; } 
			set { 
				if (value != enforceConstraints) {
					enforceConstraints = value; 
					if (value) {
						foreach (DataTable table in Tables) {
							foreach (Constraint c in table.Constraints)
								c.AssertConstraint ();
						}
					}
				}
			}
		}

		[Browsable (false)]
		[DataCategory ("Data")]
		[DataSysDescription ("The collection that holds custom user information.")]
		public PropertyCollection ExtendedProperties {
			get { return properties; }
		}

		[Browsable (false)]
		[DataSysDescription ("Indicates that the DataSet has errors.")]
		public bool HasErrors {
			[MonoTODO]
			get {
				for (int i = 0; i < Tables.Count; i++) {
					if (Tables[i].HasErrors)
						return true;
				}
				return false;
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates a locale under which to compare strings within the DataSet.")]
		public CultureInfo Locale {
			get {
				return locale;
			}
			set {
				if (locale == null || !locale.Equals (value)) {
					// TODO: check if the new locale is valid
					// TODO: update locale of all tables
					locale = value;
				}
			}
		}

		public void Merge (DataRow[] rows)
		{
			Merge (rows, false, MissingSchemaAction.Add);
		}
		
		public void Merge (DataSet dataSet)
		{
			Merge (dataSet, false, MissingSchemaAction.Add);
		}
		
		public void Merge (DataTable table)
		{
			Merge (table, false, MissingSchemaAction.Add);
		}
		
		public void Merge (DataSet dataSet, bool preserveChanges)
		{
			Merge (dataSet, preserveChanges, MissingSchemaAction.Add);
		}
		
		[MonoTODO]
		public void Merge (DataRow[] rows, bool preserveChanges, MissingSchemaAction missingSchemaAction)
		{
			if (rows == null)
				throw new ArgumentNullException ("rows");
			if (!IsLegalSchemaAction (missingSchemaAction))
				throw new ArgumentOutOfRangeException ("missingSchemaAction");
			
			MergeManager.Merge (this, rows, preserveChanges, missingSchemaAction);
		}
		
		[MonoTODO]
		public void Merge (DataSet dataSet, bool preserveChanges, MissingSchemaAction missingSchemaAction)
		{
			if (dataSet == null)
				throw new ArgumentNullException ("dataSet");
			if (!IsLegalSchemaAction (missingSchemaAction))
				throw new ArgumentOutOfRangeException ("missingSchemaAction");
			
			MergeManager.Merge (this, dataSet, preserveChanges, missingSchemaAction);
		}
		
		[MonoTODO]
		public void Merge (DataTable table, bool preserveChanges, MissingSchemaAction missingSchemaAction)
		{
			if (table == null)
				throw new ArgumentNullException ("table");
			if (!IsLegalSchemaAction (missingSchemaAction))
				throw new ArgumentOutOfRangeException ("missingSchemaAction");
			
			MergeManager.Merge (this, table, preserveChanges, missingSchemaAction);
		}

		private static bool IsLegalSchemaAction (MissingSchemaAction missingSchemaAction)
		{
			if (missingSchemaAction == MissingSchemaAction.Add || missingSchemaAction == MissingSchemaAction.AddWithKey
				|| missingSchemaAction == MissingSchemaAction.Error || missingSchemaAction == MissingSchemaAction.Ignore)
				return true;
			return false;
		}
		
		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the XML uri namespace for the root element pointed at by this DataSet.")]
		[DefaultValue ("")]
		public string Namespace {
			[MonoTODO]
			get { return _namespace; } 
			[MonoTODO]
			set {
				//TODO - trigger an event if this happens?
				_namespace = value;
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the prefix of the namespace used for this DataSet.")]
		[DefaultValue ("")]
		public string Prefix {
			[MonoTODO]
			get { return prefix; } 
			[MonoTODO]
			set {
				//TODO - trigger an event if this happens?

				if (value == null)
					value = string.Empty;
				
				if (value != this.prefix) 
					RaisePropertyChanging ("Prefix");
				prefix = value;
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("The collection that holds the relations for this DatSet.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public DataRelationCollection Relations {
			get {
				return relationCollection;		
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override ISite Site {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			} 
			
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("The collection that holds the tables for this DataSet.")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public DataTableCollection Tables {
			get { return tableCollection; }
		}

		#endregion // Public Properties

		#region Public Methods

		[MonoTODO]
		public void AcceptChanges ()
		{
			foreach (DataTable tempTable in tableCollection)
				tempTable.AcceptChanges ();
		}

		public void Clear ()
		{
			// TODO: if currently bound to a XmlDataDocument
			//       throw a NotSupportedException
			for (int t = 0; t < tableCollection.Count; t++) {
				tableCollection[t].Clear ();
			}
		}

		public virtual DataSet Clone ()
		{
			DataSet Copy = new DataSet ();
			CopyProperties (Copy);

			foreach (DataTable Table in Tables) {
				Copy.Tables.Add (Table.Clone ());
			}

			//Copy Relationships between tables after existance of tables
			//and setting properties correctly
			CopyRelations (Copy);
			
			return Copy;
		}

		// Copies both the structure and data for this DataSet.
		public DataSet Copy ()
		{
			DataSet Copy = new DataSet ();
			CopyProperties (Copy);

			// Copy DatSet's tables
			foreach (DataTable Table in Tables) 
				Copy.Tables.Add (Table.Copy ());

			//Copy Relationships between tables after existance of tables
			//and setting properties correctly
			CopyRelations (Copy);

			return Copy;
		}

		[MonoTODO]
		private void CopyProperties (DataSet Copy)
		{
			Copy.CaseSensitive = CaseSensitive;
			//Copy.Container = Container
			Copy.DataSetName = DataSetName;
			//Copy.DefaultViewManager
			//Copy.DesignMode
			Copy.EnforceConstraints = EnforceConstraints;
			//Copy.ExtendedProperties 
			//Copy.HasErrors
			//Copy.Locale = Locale;
			Copy.Namespace = Namespace;
			Copy.Prefix = Prefix;			
			//Copy.Site = Site;

		}
		
		
		private void CopyRelations (DataSet Copy)
		{

			//Creation of the relation contains some of the properties, and the constructor
			//demands these values. instead changing the DataRelation constructor and behaviour the
			//parameters are pre-configured and sent to the most general constructor

			foreach (DataRelation MyRelation in this.Relations) {
				string pTable = MyRelation.ParentTable.TableName;
				string cTable = MyRelation.ChildTable.TableName;
				DataColumn[] P_DC = new DataColumn[MyRelation.ParentColumns.Length]; 
				DataColumn[] C_DC = new DataColumn[MyRelation.ChildColumns.Length];
				int i = 0;
				
				foreach (DataColumn DC in MyRelation.ParentColumns) {
					P_DC[i]=Copy.Tables[pTable].Columns[DC.ColumnName];
					i++;
				}

				i = 0;

				foreach (DataColumn DC in MyRelation.ChildColumns) {
					C_DC[i]=Copy.Tables[cTable].Columns[DC.ColumnName];
					i++;
				}
				
				DataRelation cRel = new DataRelation (MyRelation.RelationName, P_DC, C_DC);
				//cRel.ChildColumns = MyRelation.ChildColumns;
				//cRel.ChildTable = MyRelation.ChildTable;
				//cRel.ExtendedProperties = cRel.ExtendedProperties; 
				//cRel.Nested = MyRelation.Nested;
				//cRel.ParentColumns = MyRelation.ParentColumns;
				//cRel.ParentTable = MyRelation.ParentTable;
								
				Copy.Relations.Add (cRel);
			}
		}

		


		public DataSet GetChanges ()
		{
			return GetChanges (DataRowState.Added | DataRowState.Deleted | DataRowState.Modified);
		}

		
		public DataSet GetChanges (DataRowState rowStates)
		{
			if (!HasChanges (rowStates))
				return null;
			
			DataSet copySet = Clone ();
			IEnumerator tableEnumerator = Tables.GetEnumerator ();
			DataTable origTable;
			DataTable copyTable;
			while (tableEnumerator.MoveNext ()) {
				origTable = (DataTable)tableEnumerator.Current;
				copyTable = copySet.Tables[origTable.TableName];

				IEnumerator rowEnumerator = origTable.Rows.GetEnumerator ();
				while (rowEnumerator.MoveNext ()) {
					DataRow row = (DataRow)rowEnumerator.Current;
					if (row.IsRowChanged (rowStates)) {
						DataRow newRow = copyTable.NewRow ();
						copyTable.Rows.Add (newRow);
						row.CopyValuesToRow (newRow);
					}
				}
			}
			return copySet;
		}

#if NET_1_2
		[MonoTODO]
		public DataTableReader GetDataReader (DataTable[] dataTables)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataTableReader GetDataReader ()
		{
			throw new NotImplementedException ();
		}
#endif
		
		public string GetXml ()
		{
			StringWriter Writer = new StringWriter ();

			// Sending false for not printing the Processing instruction
			WriteXml (Writer, XmlWriteMode.IgnoreSchema, false);
			return Writer.ToString ();
		}

		public string GetXmlSchema ()
		{
			StringWriter Writer = new StringWriter ();
			WriteXmlSchema (Writer);
			return Writer.ToString ();
		}

		[MonoTODO]
		public bool HasChanges ()
		{
			return HasChanges (DataRowState.Added | DataRowState.Deleted | DataRowState.Modified);
		}

		[MonoTODO]
		public bool HasChanges (DataRowState rowState)
		{
			if (((int)rowState & 0xffffffe0) != 0)
				throw new ArgumentOutOfRangeException ("rowState");

			DataTableCollection tableCollection = Tables;
			DataTable table;
			DataRowCollection rowCollection;
			DataRow row;

			for (int i = 0; i < tableCollection.Count; i++) {
				table = tableCollection[i];
				rowCollection = table.Rows;
				for (int j = 0; j < rowCollection.Count; j++) {
					row = rowCollection[j];
					if ((row.RowState & rowState) != 0)
						return true;
				}
			}

			return false;		
		}

		[MonoTODO]
		public void InferXmlSchema (XmlReader reader, string[] nsArray)
		{
		}

		public void InferXmlSchema (Stream stream, string[] nsArray)
		{
			InferXmlSchema (new XmlTextReader (stream), nsArray);
		}

		public void InferXmlSchema (TextReader reader, string[] nsArray)
		{
			InferXmlSchema (new XmlTextReader (reader), nsArray);
		}

		public void InferXmlSchema (string fileName, string[] nsArray)
		{
			XmlTextReader reader = new XmlTextReader (fileName);
			try {
				InferXmlSchema (reader, nsArray);
			} finally {
				reader.Close ();
			}
		}

#if NET_1_2
		[MonoTODO]
		public void Load (IDataReader reader, LoadOption loadOption, DataTable[] tables)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Load (IDataReader reader, LoadOption loadOption, string[] tables)
		{
			throw new NotImplementedException ();
		}
#endif

		public virtual void RejectChanges ()
		{
			int i;
			bool oldEnforceConstraints = this.EnforceConstraints;
			this.EnforceConstraints = false;
			
			for (i = 0; i < this.Tables.Count;i++) 
				this.Tables[i].RejectChanges ();

			this.EnforceConstraints = oldEnforceConstraints;
		}

		public virtual void Reset ()
		{
			IEnumerator constraintEnumerator;

			// first we remove all ForeignKeyConstraints (if we will not do that
			// we will get an exception when clearing the tables).
			for (int i = 0; i < Tables.Count; i++) {
				ConstraintCollection cc = Tables[i].Constraints;
				for (int j = 0; j < cc.Count; j++) {
					if (cc[j] is ForeignKeyConstraint)
						cc.Remove (cc[j]);
				}
			}

			Clear ();
			Relations.Clear ();
			Tables.Clear ();
		}

		public void WriteXml (Stream stream)
		{
			XmlWriter writer = new XmlTextWriter (stream, null);
			
			WriteXml (writer);
		}

		///<summary>
		/// Writes the current data for the DataSet to the specified file.
		/// </summary>
		/// <param name="filename">Fully qualified filename to write to</param>
		public void WriteXml (string fileName)
		{
			XmlWriter writer = new XmlTextWriter (fileName, null);
			
			WriteXml (writer);
			
			writer.Close ();
		}

		public void WriteXml (TextWriter writer)
		{
			XmlWriter xwriter = new XmlTextWriter (writer);
			
			WriteXml (xwriter);
		}

		public void WriteXml (XmlWriter writer)
		{
			WriteXml (writer, XmlWriteMode.IgnoreSchema, true);
		}

		public void WriteXml (string filename, XmlWriteMode mode)
		{
			XmlWriter writer = new XmlTextWriter (filename, null);
			WriteXml (writer, mode, true);
		}

		public void WriteXml (Stream stream, XmlWriteMode mode)
		{
			XmlWriter writer = new XmlTextWriter (stream, null);

			WriteXml (writer, mode, true);
		}

		public void WriteXml (TextWriter writer, XmlWriteMode mode)
		{
			XmlWriter xwriter = new XmlTextWriter (writer);
			WriteXml (xwriter, mode, true);
		}

		public void WriteXml (XmlWriter writer, XmlWriteMode mode)
		{
			WriteXml (writer, mode, true);
		}
		
		public void WriteXml (Stream stream, XmlWriteMode mode, bool writePI)
		{
			XmlWriter writer = new XmlTextWriter (stream, null);
			
			WriteXml (writer, mode, writePI);
		}

		public void WriteXml (string fileName, XmlWriteMode mode, bool writePI)
		{
			XmlWriter writer = new XmlTextWriter (fileName, null);
			
			WriteXml (writer, mode, writePI);
			
			writer.Close ();
		}

		public void WriteXml (TextWriter writer, XmlWriteMode mode, bool writePI)
		{
			XmlWriter xwriter = new XmlTextWriter (writer);
			
			WriteXml (xwriter, mode, writePI);
		}

		public void WriteXml (XmlWriter writer, XmlWriteMode mode, bool writePI)
		{
			if (writePI && (writer.WriteState == WriteState.Start))
				writer.WriteStartDocument (true);

			((XmlTextWriter)writer).Formatting = Formatting.Indented;

			if (mode == XmlWriteMode.DiffGram) {
				SetRowsID();
				WriteDiffGramElement(writer);
			}

			WriteStartElement (writer, mode, Namespace, Prefix, XmlConvert.EncodeName (DataSetName));
			
			if (mode == XmlWriteMode.WriteSchema) {
				DoWriteXmlSchema (writer);
			}
			
			WriteTables (writer, mode, Tables, DataRowVersion.Default);
			if (mode == XmlWriteMode.DiffGram) {
				writer.WriteEndElement (); //DataSet name
				if (HasChanges(DataRowState.Modified | DataRowState.Deleted)) {

					DataSet beforeDS = GetChanges (DataRowState.Modified | DataRowState.Deleted);	
					WriteStartElement (writer, XmlWriteMode.DiffGram, Namespace, Prefix, "diffgr:before");
					WriteTables (writer, mode, beforeDS.Tables, DataRowVersion.Original);
					writer.WriteEndElement ();
				}
			}
			writer.WriteEndElement (); // DataSet name or diffgr:diffgram
		}

		public void WriteXmlSchema (Stream stream)
		{
			XmlWriter writer = new XmlTextWriter (stream, null );
			
			WriteXmlSchema (writer);	
		}

		public void WriteXmlSchema (string fileName)
		{
			XmlWriter writer = new XmlTextWriter (fileName, null);
	    	
			WriteXmlSchema (writer);
		}

		public void WriteXmlSchema (TextWriter writer)
		{
			XmlWriter xwriter = new XmlTextWriter (writer);
			
			WriteXmlSchema (xwriter);
		}

		public void WriteXmlSchema (XmlWriter writer)
		{
			((XmlTextWriter)writer).Formatting = Formatting.Indented;
			//Create a skeleton doc and then write the schema 
			//proper which is common to the WriteXml method in schema mode
			writer.WriteStartDocument ();
			
			DoWriteXmlSchema (writer);
			
			writer.WriteEndDocument ();
		}

		public void ReadXmlSchema (Stream stream)
		{
			XmlReader reader = new XmlTextReader (stream, null);
			ReadXmlSchema (reader);
		}

		public void ReadXmlSchema (string str)
		{
			XmlReader reader = new XmlTextReader (str);
			ReadXmlSchema (reader);
		}

		public void ReadXmlSchema (TextReader treader)
		{
			XmlReader reader = new XmlTextReader (treader);
			ReadXmlSchema (reader);			
		}

		public void ReadXmlSchema (XmlReader reader)
		{
			XmlSchemaMapper SchemaMapper = new XmlSchemaMapper (this);
			SchemaMapper.Read (reader);
		}

		public XmlReadMode ReadXml (Stream stream)
		{
			return ReadXml (new XmlTextReader (stream));
		}

		public XmlReadMode ReadXml (string str)
		{
			return ReadXml (new XmlTextReader (str));
		}

		public XmlReadMode ReadXml (TextReader reader)
		{
			return ReadXml (new XmlTextReader (reader));
		}

		public XmlReadMode ReadXml (XmlReader r)
		{
			XmlDataLoader Loader = new XmlDataLoader (this);
			// FIXME: somekinda exception?
			if (!r.Read ())
				return XmlReadMode.Auto; // FIXME

			/*\
			 *  If document is diffgram we will use diffgram
			\*/
			if (r.LocalName == "diffgram")
				return ReadXml (r, XmlReadMode.DiffGram);

			/*\
			 *  If we already have a schema, or the document 
			 *  contains an in-line schema, sets XmlReadMode to ReadSchema.
			\*/

			// FIXME: is this always true: "if we have tables we have to have schema also"
			if (Tables.Count > 0)				
				return ReadXml (r, XmlReadMode.ReadSchema);

			/*\
			 *  If we dont have a schema yet and document 
			 *  contains no inline-schema  mode is XmlReadMode.InferSchema
			\*/

			return ReadXml (r, XmlReadMode.InferSchema);

		}

		public XmlReadMode ReadXml (Stream stream, XmlReadMode mode)
		{
			return ReadXml (new XmlTextReader (stream), mode);
		}

		public XmlReadMode ReadXml (string str, XmlReadMode mode)
		{
			return ReadXml (new XmlTextReader (str), mode);
		}

		public XmlReadMode ReadXml (TextReader reader, XmlReadMode mode)
		{
			return ReadXml (new XmlTextReader (reader), mode);
		}

		[MonoTODO]
		public XmlReadMode ReadXml (XmlReader reader, XmlReadMode mode)
		{
			XmlReadMode Result = XmlReadMode.Auto;

			if (mode == XmlReadMode.DiffGram) {
				if (reader.LocalName != "diffgram"){
					reader.MoveToContent ();
					reader.ReadStartElement ();	// <DataSet>

					reader.MoveToContent ();
					ReadXmlSchema (reader);

					reader.MoveToContent ();
				}
				XmlDiffLoader DiffLoader = new XmlDiffLoader (this);
				DiffLoader.Load (reader);
				Result =  XmlReadMode.DiffGram;
			}
			else {
				XmlDataLoader Loader = new XmlDataLoader (this);
				Result = Loader.LoadData (reader, mode);
			}

			return Result;
		}

		#endregion // Public Methods

		#region Public Events

		[DataCategory ("Action")]
		[DataSysDescription ("Occurs when it is not possible to merge schemas for two tables with the same name.")]
		public event MergeFailedEventHandler MergeFailed;

		#endregion // Public Events

		#region Destructors

		~DataSet ()
		{
		}

		#endregion Destructors

		#region IListSource methods
		IList IListSource.GetList ()
		{
			return DefaultViewManager;
		}
		
		bool IListSource.ContainsListCollection {
			get {
				return true;
			}
		}
		#endregion IListSource methods
		
		#region ISupportInitialize methods
		public void BeginInit ()
		{
		}
		
		public void EndInit ()
		{
		}
		#endregion

		#region ISerializable
		void ISerializable.GetObjectData (SerializationInfo si, StreamingContext sc)
		{
			throw new NotImplementedException ();
		}
		#endregion
		
		#region Protected Methods
		protected void GetSerializationData (SerializationInfo info, StreamingContext context)
		{
			string s = info.GetValue ("XmlDiffGram", typeof (String)) as String;
			if (s != null) ReadXmlSerializable (new XmlTextReader (new StringReader (s)));
		}
		
		
		protected virtual System.Xml.Schema.XmlSchema GetSchemaSerializable ()
		{
			return BuildSchema ();
		}
		
		protected virtual void ReadXmlSerializable (XmlReader reader)
		{
			ReadXml (reader, XmlReadMode.DiffGram); // FIXME
		}

		void IXmlSerializable.ReadXml (XmlReader reader)
		{

			ReadXml (reader, XmlReadMode.DiffGram);
			
			// the XmlSerializationReader does this lines!!!
			//reader.MoveToContent ();
			//reader.ReadEndElement ();	// </DataSet>
		}
		
		void IXmlSerializable.WriteXml (XmlWriter writer)
		{
			DoWriteXmlSchema (writer);
			WriteXml (writer, XmlWriteMode.DiffGram, true);
		}

		protected virtual bool ShouldSerializeRelations ()
		{
			return true;
		}
		
		protected virtual bool ShouldSerializeTables ()
		{
			return true;
		}

		[MonoTODO]
		protected internal virtual void OnPropertyChanging (PropertyChangedEventArgs pcevent)
		{
		}

		[MonoTODO]
		protected virtual void OnRemoveRelation (DataRelation relation)
		{
		}

		[MonoTODO]
		protected virtual void OnRemoveTable (DataTable table)
		{
		}

		protected internal virtual void OnMergeFailed (MergeFailedEventArgs e)
		{
			if (MergeFailed != null)
				MergeFailed (this, e);
		}

		[MonoTODO]
		protected internal void RaisePropertyChanging (string name)
		{
		}
		#endregion

		#region Private Xml Serialisation

		private string WriteObjectXml (object o)
		{
			switch (Type.GetTypeCode (o.GetType ())) {
				case TypeCode.Boolean:
					return XmlConvert.ToString ((Boolean) o);
				case TypeCode.Byte:
					return XmlConvert.ToString ((Byte) o);
				case TypeCode.Char:
					return XmlConvert.ToString ((Char) o);
				case TypeCode.DateTime:
					return XmlConvert.ToString ((DateTime) o);
				case TypeCode.Decimal:
					return XmlConvert.ToString ((Decimal) o);
				case TypeCode.Double:
					return XmlConvert.ToString ((Double) o);
				case TypeCode.Int16:
					return XmlConvert.ToString ((Int16) o);
				case TypeCode.Int32:
					return XmlConvert.ToString ((Int32) o);
				case TypeCode.Int64:
					return XmlConvert.ToString ((Int64) o);
				case TypeCode.SByte:
					return XmlConvert.ToString ((SByte) o);
				case TypeCode.Single:
					return XmlConvert.ToString ((Single) o);
				case TypeCode.UInt16:
					return XmlConvert.ToString ((UInt16) o);
				case TypeCode.UInt32:
					return XmlConvert.ToString ((UInt32) o);
				case TypeCode.UInt64:
					return XmlConvert.ToString ((UInt64) o);
			}
			if (o is TimeSpan) return XmlConvert.ToString ((TimeSpan) o);
			if (o is Guid) return XmlConvert.ToString ((Guid) o);
			return o.ToString ();
		}
		
		private void WriteTables (XmlWriter writer, XmlWriteMode mode, DataTableCollection tableCollection, DataRowVersion version)
		{
			//Write out each table in order, providing it is not
			//part of another table structure via a nested parent relationship
			foreach (DataTable table in tableCollection) {
				bool isTopLevel = true;
				foreach (DataRelation rel in table.ParentRelations) {
					if (rel.Nested) {
						isTopLevel = false;
						break;
					}
				}
				
				if (isTopLevel) {
					WriteTable ( writer, table, mode, version);
				}
			}
		}

		private void WriteTable (XmlWriter writer, DataTable table, XmlWriteMode mode, DataRowVersion version)
		{
			DataRow[] rows = new DataRow [table.Rows.Count];
			table.Rows.CopyTo (rows, 0);
			WriteTable (writer, rows, mode, version);
		}

		private void WriteTable (XmlWriter writer, DataRow[] rows, XmlWriteMode mode, DataRowVersion version)
		{
			//The columns can be attributes, hidden, elements, or simple content
			//There can be 0-1 simple content cols or 0-* elements
			System.Collections.ArrayList atts;
			System.Collections.ArrayList elements;
			DataColumn simple = null;

			if (rows.Length == 0) return;
			DataTable table = rows[0].Table;
			SplitColumns (table, out atts, out elements, out simple);
			//sort out the namespacing
			string nspc = table.Namespace.Length > 0 ? table.Namespace : Namespace;

			foreach (DataRow row in rows) {
				if (!row.HasVersion(version))
					continue;
				
				// First check are all the rows null. If they are we just write empty element
				bool AllNulls = true;
				foreach (DataColumn dc in table.Columns) {
				
					if (row [dc.ColumnName, version] != DBNull.Value) {
						AllNulls = false;
						break;
					} 
				}

				// If all of the columns were null, we have to write empty element
				if (AllNulls) {
					writer.WriteElementString (table.TableName, "");
					continue;
				}
				
				WriteTableElement (writer, mode, table, row, version);
				
				foreach (DataColumn col in atts) {					
					WriteColumnAsAttribute (writer, mode, col, row, version);
				}
				
				if (simple != null) {
					writer.WriteString (WriteObjectXml (row[simple, version]));
				}
				else {					
					foreach (DataColumn col in elements) {
						WriteColumnAsElement (writer, mode, nspc, col, row, version);
					}
				}
				
				foreach (DataRelation relation in table.ChildRelations) {
					if (relation.Nested) {
						WriteTable (writer, row.GetChildRows (relation), mode, version);
					}
				}
				
				writer.WriteEndElement ();
			}

		}

		private void WriteColumnAsElement (XmlWriter writer, XmlWriteMode mode, string nspc, DataColumn col, DataRow row, DataRowVersion version)
		{
			string colnspc = nspc;
			object rowObject = row [col, version];
									
			if (rowObject == null || rowObject == DBNull.Value)
				return;

			if (col.Namespace != null) {
				colnspc = col.Namespace;
			}
	
			//TODO check if I can get away with write element string
			WriteStartElement (writer, mode, colnspc, col.Prefix, col.ColumnName);
			writer.WriteString (WriteObjectXml (rowObject));
			writer.WriteEndElement ();
		}

		private void WriteColumnAsAttribute (XmlWriter writer, XmlWriteMode mode, DataColumn col, DataRow row, DataRowVersion version)
		{
			WriteAttributeString (writer, mode, col.Namespace, col.Prefix, col.ColumnName, row[col, version].ToString ());
		}

		private void WriteTableElement (XmlWriter writer, XmlWriteMode mode, DataTable table, DataRow row, DataRowVersion version)
		{
			//sort out the namespacing
			string nspc = table.Namespace.Length > 0 ? table.Namespace : Namespace;

			WriteStartElement (writer, mode, nspc, table.Prefix, table.TableName);

			if (mode == XmlWriteMode.DiffGram) {
				WriteAttributeString (writer, mode, "", "diffgr", "id", table.TableName + (row.XmlRowID + 1));
				WriteAttributeString (writer, mode, "", "msdata", "rowOrder", row.XmlRowID.ToString());
				if (row.RowState == DataRowState.Modified && version != DataRowVersion.Original){
					WriteAttributeString (writer, mode, "", "diffgr", "hasChanges", "modified");
				}
			}
		}
		    
		private void WriteStartElement (XmlWriter writer, XmlWriteMode mode, string nspc, string prefix, string name)
		{			
			switch ( mode) {
				case XmlWriteMode.WriteSchema:
					if (nspc == null || nspc == "") {
						writer.WriteStartElement (name);
					}
					else if (prefix != null) {							
						writer.WriteStartElement (prefix, name, nspc);
					}						
					else {					
						writer.WriteStartElement (writer.LookupPrefix (nspc), name, nspc);
					}
					break;
				case XmlWriteMode.DiffGram:
					writer.WriteStartElement (name);
					break;	
				default:					       
					writer.WriteStartElement (name);
					break;					
			};
		}
		
		private void WriteAttributeString (XmlWriter writer, XmlWriteMode mode, string nspc, string prefix, string name, string stringValue)
		{
			switch ( mode) {
				case XmlWriteMode.WriteSchema:
					writer.WriteAttributeString (prefix, name, nspc);
					break;
				case XmlWriteMode.DiffGram:
					writer.WriteAttributeString (prefix, name, nspc, stringValue);
					break;
				default:
					writer.WriteAttributeString (name, stringValue);
					break;					
			};
		}

		XmlSchema IXmlSerializable.GetSchema ()
		{
			return BuildSchema ();
		}
		
		XmlSchema BuildSchema ()
		{
			XmlSchema schema = new XmlSchema ();
			schema.AttributeFormDefault = XmlSchemaForm.Qualified;

			XmlSchemaElement elem = new XmlSchemaElement ();
			elem.Name = XmlConvert.EncodeName (DataSetName);

			XmlDocument doc = new XmlDocument ();

			XmlAttribute[] atts = new XmlAttribute [2];
			atts[0] = doc.CreateAttribute (XmlConstants.MsdataPrefix,  XmlConstants.IsDataSet, XmlConstants.MsdataNamespace);
			atts[0].Value = "true";

			atts[1] = doc.CreateAttribute (XmlConstants.MsdataPrefix, XmlConstants.Locale, XmlConstants.MsdataNamespace);
			atts[1].Value = locale.Name;
			elem.UnhandledAttributes = atts;

			schema.Items.Add (elem);

			XmlSchemaComplexType complex = new XmlSchemaComplexType ();
			elem.SchemaType = complex;

			XmlSchemaChoice choice = new XmlSchemaChoice ();
			complex.Particle = choice;
			choice.MaxOccursString = XmlConstants.Unbounded;
			
			//Write out schema for each table in order
			foreach (DataTable table in Tables) {		
				bool isTopLevel = true;
				foreach (DataRelation rel in table.ParentRelations) {
					if (rel.Nested) {
						isTopLevel = false;
						break;
					}
				}
				
				if (isTopLevel){
					choice.Items.Add (GetTableSchema (doc, table));
				}
			}
			
			bool nameModifier = true;
			foreach (DataRelation rel in Relations) {
				XmlSchemaUnique uniq = new XmlSchemaUnique();
				XmlSchemaKeyref keyRef = new XmlSchemaKeyref();
				ForeignKeyConstraint fkConst = rel.ChildKeyConstraint;
				UniqueConstraint uqConst = rel.ParentKeyConstraint;
								
				if (nameModifier) {
					uniq.Name = uqConst.ConstraintName;
					keyRef.Name = fkConst.ConstraintName;
					keyRef.Refer = new XmlQualifiedName(uniq.Name);
					XmlAttribute[] attrib = null;
					if (rel.Nested){
						attrib = new XmlAttribute [2];
						attrib [0] = doc.CreateAttribute (XmlConstants.MsdataPrefix,  XmlConstants.IsNested, XmlConstants.MsdataNamespace);
						attrib [0].Value = "true";
		
						attrib [1] = doc.CreateAttribute (XmlConstants.MsdataPrefix, XmlConstants.RelationName, XmlConstants.MsdataNamespace);
						attrib [1].Value = rel.RelationName;
					}
					else {
						attrib = new XmlAttribute [1];
						attrib[0] = doc.CreateAttribute (XmlConstants.MsdataPrefix, XmlConstants.RelationName, XmlConstants.MsdataNamespace);
						attrib[0].Value = rel.RelationName;

					}
					keyRef.UnhandledAttributes = attrib;
					nameModifier = false;
				}
				else {
					uniq.Name = rel.ParentTable.TableName+"_"+uqConst.ConstraintName;
					keyRef.Name = rel.ChildTable.TableName+"_"+fkConst.ConstraintName;
					keyRef.Refer = new XmlQualifiedName(uniq.Name);
					XmlAttribute[] attrib;
					if (rel.Nested)	{
						attrib = new XmlAttribute [3];
						attrib [0] = doc.CreateAttribute (XmlConstants.MsdataPrefix,  XmlConstants.ConstraintName, XmlConstants.MsdataNamespace);
						attrib [0].Value = fkConst.ConstraintName;
						attrib [1] = doc.CreateAttribute (XmlConstants.MsdataPrefix,  XmlConstants.IsNested, XmlConstants.MsdataNamespace);
						attrib [1].Value = "true";
		
						attrib [2] = doc.CreateAttribute (XmlConstants.MsdataPrefix, XmlConstants.RelationName, XmlConstants.MsdataNamespace);
						attrib [2].Value = rel.RelationName;
					}
					else {
						attrib = new XmlAttribute [2];
						attrib [0] = doc.CreateAttribute (XmlConstants.MsdataPrefix,  XmlConstants.ConstraintName, XmlConstants.MsdataNamespace);
						attrib [0].Value = fkConst.ConstraintName;
						attrib [1] = doc.CreateAttribute (XmlConstants.MsdataPrefix, XmlConstants.RelationName, XmlConstants.MsdataNamespace);
						attrib [1].Value = rel.RelationName;

					}
					keyRef.UnhandledAttributes = attrib;
					attrib = new XmlAttribute [1];
					attrib [0] = doc.CreateAttribute (XmlConstants.MsdataPrefix, XmlConstants.ConstraintName, XmlConstants.MsdataNamespace);
					attrib [0].Value = uqConst.ConstraintName; 
					uniq.UnhandledAttributes = attrib;
				}

				uniq.Selector = new XmlSchemaXPath();
				uniq.Selector.XPath = ".//"+rel.ParentTable.TableName;
				XmlSchemaXPath field;
				foreach (DataColumn column in rel.ParentColumns) {
				 	field = new XmlSchemaXPath();
					field.XPath = column.ColumnName;
					uniq.Fields.Add(field);
				}
				
				keyRef.Selector = new XmlSchemaXPath();
				keyRef.Selector.XPath = ".//"+rel.ChildTable.TableName;
				foreach (DataColumn column in rel.ChildColumns)	{
				 	field = new XmlSchemaXPath();
					field.XPath = column.ColumnName;
					keyRef.Fields.Add(field);
				}
				
				elem.Constraints.Add (uniq);
				elem.Constraints.Add (keyRef);
			}
			
			return schema;
		}

		private XmlSchemaElement GetTableSchema (XmlDocument doc, DataTable table)
		{
			ArrayList elements;
			ArrayList atts;
			DataColumn simple;
			
			SplitColumns (table, out atts, out elements, out simple);

			XmlSchemaElement elem = new XmlSchemaElement ();
			elem.Name = table.TableName;

			XmlSchemaComplexType complex = new XmlSchemaComplexType ();
			elem.SchemaType = complex;

			//TODO - what about the simple content?
			if (elements.Count == 0) {				
			}
			else {
				//A sequence of element types or a simple content node
				//<xs:sequence>
				XmlSchemaSequence seq = new XmlSchemaSequence ();
				complex.Particle = seq;

				foreach (DataColumn col in elements) {
					//<xs:element name=ColumnName type=MappedType Ordinal=index>
					XmlSchemaElement colElem = new XmlSchemaElement ();
					colElem.Name = col.ColumnName;
				
					if (col.ColumnName != col.Caption && col.Caption != string.Empty) {
						XmlAttribute[] xatts = new XmlAttribute[1];
						xatts[0] = doc.CreateAttribute (XmlConstants.MsdataPrefix, XmlConstants.Caption, XmlConstants.MsdataNamespace);
						xatts[0].Value = col.Caption;
						colElem.UnhandledAttributes = xatts;
					}

					if (col.DefaultValue.ToString () != string.Empty)
						colElem.DefaultValue = col.DefaultValue.ToString ();

					colElem.SchemaTypeName = MapType (col.DataType);

					if (col.AllowDBNull) {
						colElem.MinOccurs = 0;
					}

					//writer.WriteAttributeString (XmlConstants.MsdataPrefix, 
					//                            XmlConstants.Ordinal, 
					//                            XmlConstants.MsdataNamespace, 
					//                            col.Ordinal.ToString ());

					// Write SimpleType if column have MaxLength
					if (col.MaxLength > -1) {
						colElem.SchemaType = GetTableSimpleType (doc, col);
					}

					seq.Items.Add (colElem);
				}

				foreach (DataRelation rel in table.ChildRelations) {
					if (rel.Nested) {
						seq.Items.Add(GetTableSchema (doc, rel.ChildTable));
					}
				}
			}

			//Then a list of attributes
			foreach (DataColumn col in atts) {
				//<xs:attribute name=col.ColumnName form="unqualified" type=MappedType/>
				XmlSchemaAttribute att = new XmlSchemaAttribute ();
				att.Name = col.ColumnName;
				att.Form = XmlSchemaForm.Unqualified;
				att.SchemaTypeName = MapType (col.DataType);
				complex.Attributes.Add (att);
			}

			return elem;
		}

		private XmlSchemaSimpleType GetTableSimpleType (XmlDocument doc, DataColumn col)
		{
			// SimpleType
			XmlSchemaSimpleType simple = new XmlSchemaSimpleType ();

			// Restriction
			XmlSchemaSimpleTypeRestriction restriction = new XmlSchemaSimpleTypeRestriction ();
			restriction.BaseTypeName = MapType (col.DataType);
			
			// MaxValue
			XmlSchemaMaxLengthFacet max = new XmlSchemaMaxLengthFacet ();
			max.Value = XmlConvert.ToString (col.MaxLength);
			restriction.Facets.Add (max);

			return simple;
		}

		private void DoWriteXmlSchema (XmlWriter writer)
		{
			GetSchemaSerializable ().Write (writer);
		}
		
		///<summary>
		/// Helper function to split columns into attributes elements and simple
		/// content
		/// </summary>
		private void SplitColumns (DataTable table, 
			out ArrayList atts, 
			out ArrayList elements, 
			out DataColumn simple)
		{
			//The columns can be attributes, hidden, elements, or simple content
			//There can be 0-1 simple content cols or 0-* elements
			atts = new System.Collections.ArrayList ();
			elements = new System.Collections.ArrayList ();
			simple = null;
			
			//Sort out the columns
			foreach (DataColumn col in table.Columns) {
				switch (col.ColumnMapping) {
					case MappingType.Attribute:
						atts.Add (col);
						break;
					case MappingType.Element:
						elements.Add (col);
						break;
					case MappingType.SimpleContent:
						if (simple != null) {
							throw new System.InvalidOperationException ("There may only be one simple content element");
						}
						simple = col;
						break;
					default:
						//ignore Hidden elements
						break;
				}
			}
		}

		private void WriteDiffGramElement(XmlWriter writer)
		{
			WriteStartElement (writer, XmlWriteMode.DiffGram, Namespace, Prefix, "diffgr:diffgram");
			WriteAttributeString(writer, XmlWriteMode.DiffGram, null, "xmlns", XmlConstants.MsdataPrefix, XmlConstants.MsdataNamespace);
			WriteAttributeString(writer, XmlWriteMode.DiffGram, null, "xmlns", XmlConstants.DiffgrPrefix, XmlConstants.DiffgrNamespace);
		}

		private void SetRowsID()
		{
			foreach (DataTable Table in Tables) {
				int dataRowID = 0;
				foreach (DataRow Row in Table.Rows) {
					Row.XmlRowID = dataRowID;
					dataRowID++;
				}
			}
		}

		
		private XmlQualifiedName MapType (Type type)
		{
			switch (Type.GetTypeCode (type)) {
				case TypeCode.String: return XmlConstants.QnString;
				case TypeCode.Int16: return XmlConstants.QnShort;
				case TypeCode.Int32: return XmlConstants.QnInt;
				case TypeCode.Int64: return XmlConstants.QnLong;
				case TypeCode.Boolean: return XmlConstants.QnBoolean;
				case TypeCode.Byte: return XmlConstants.QnUnsignedByte;
				case TypeCode.Char: return XmlConstants.QnChar;
				case TypeCode.DateTime: return XmlConstants.QnDateTime;
				case TypeCode.Decimal: return XmlConstants.QnDecimal;
				case TypeCode.Double: return XmlConstants.QnDouble;
				case TypeCode.SByte: return XmlConstants.QnSbyte;
				case TypeCode.Single: return XmlConstants.QnFloat;
				case TypeCode.UInt16: return XmlConstants.QnUsignedShort;
				case TypeCode.UInt32: return XmlConstants.QnUnsignedInt;
				case TypeCode.UInt64: return XmlConstants.QnUnsignedLong;
			}
			
			if (typeof (TimeSpan) == type)
				return XmlConstants.QnDuration;
			else if (typeof (System.Uri) == type)
				return XmlConstants.QnUri;
			else if (typeof (byte[]) == type)
				return XmlConstants.QnBase64Binary;
			else if (typeof (XmlQualifiedName) == type)
				return XmlConstants.QnXmlQualifiedName;
			else
				return XmlConstants.QnString;
		}

		#endregion //Private Xml Serialisation
	}
}
