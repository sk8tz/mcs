//
// System.Data.Common.DataTableMappingCollection.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc
// Copyright (C) 2002 Tim Coleman
//

using System;
using System.Collections;

namespace System.Data.Common
{
	/// <summary>
	/// A collection of DataTableMapping objects. This class cannot be inherited.
	/// </summary>
	public sealed class DataTableMappingCollection :
	MarshalByRefObject, // ITableMappingCollection, IList,
	        IEnumerable //ICollection, 
	{

		#region Fields

		ArrayList mappings;
		Hashtable sourceTables;
		Hashtable dataSetTables;

		#endregion

		#region Constructors 

		public DataTableMappingCollection() 
		{
			mappings = new ArrayList ();
			sourceTables = new Hashtable ();
			dataSetTables = new Hashtable ();
		}

		#endregion

		#region Properties

		public int Count 
		{
			get { return mappings.Count; }
		}

		public DataTableMapping this[int index] {
			get { return (DataTableMapping)(mappings[index]); }
			set { 
				DataTableMapping mapping = (DataTableMapping)(mappings[index]);
				sourceTables[mapping.SourceTable] = value;
				dataSetTables[mapping.DataSetTable] = value;
				mappings[index] = value; 
			}
		}

		[MonoTODO]
		public DataTableMapping this[string sourceTable] {
			get { return (DataTableMapping)(sourceTables[sourceTable]); }
			set { this[mappings.IndexOf(sourceTables[sourceTable])] = value; }
		}

		#endregion

		#region Methods

		public int Add (object value) 
		{
			if (!(value is System.Data.Common.DataTableMapping))
				throw new SystemException ("The object passed in was not a DataTableMapping object.");

			sourceTables[((DataTableMapping)value).SourceTable] = value;	
			dataSetTables[((DataTableMapping)value).DataSetTable] = value;	
			return mappings.Add (value);
		}

		public DataTableMapping Add (string sourceTable, string dataSetTable) 
		{
			DataTableMapping mapping = new DataTableMapping (sourceTable, dataSetTable);
			Add (mapping);
			return mapping;
		}

		public void AddRange(DataTableMapping[] values) 
		{
			foreach (DataTableMapping dataTableMapping in values)
				this.Add (dataTableMapping);
		}

		public void Clear() 
		{
			sourceTables.Clear ();
			dataSetTables.Clear ();
			mappings.Clear ();
		}

		public bool Contains (object value) 
		{
			return mappings.Contains (value);
		}

		public bool Contains (string value) 
		{
			return sourceTables.Contains (value);
		}

		[MonoTODO]
		public void CopyTo(Array array, int index) 
		{
			throw new NotImplementedException ();
		}

		public DataTableMapping GetByDataSetTable (string dataSetTable) 
		{
			return (DataTableMapping)(dataSetTables[dataSetTable]);
		}

		public static DataTableMapping GetTableMappingBySchemaAction (DataTableMappingCollection tableMappings, string sourceTable, string dataSetTable, MissingMappingAction mappingAction) 
		{
			if (tableMappings.Contains (sourceTable))
				return tableMappings[sourceTable];
			if (mappingAction == MissingMappingAction.Error)
				throw new InvalidOperationException ();
			if (mappingAction == MissingMappingAction.Ignore)
				return null;
			return new DataTableMapping (sourceTable, dataSetTable);
		}

		public IEnumerator GetEnumerator ()
		{
			return mappings.GetEnumerator ();
		}

		public int IndexOf (object value) 
		{
			return mappings.IndexOf (value);
		}

		public int IndexOf (string sourceTable) 
		{
			return IndexOf (sourceTables[sourceTable]);
		}

		public int IndexOfDataSetTable (string dataSetTable) 
		{
			return IndexOf ((DataTableMapping)(dataSetTables[dataSetTable]));
		}

		[MonoTODO]
		public void Insert (int index, object value) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Remove (object value) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveAt (int index) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveAt (string index) 
		{
			throw new NotImplementedException ();
		}
		


		#endregion
	}
}
