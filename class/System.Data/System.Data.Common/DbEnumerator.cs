//
// System.Data.SqlClient.DbEnumerator.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Collections;
using System.Data;

namespace System.Data.Common {
	public class DbEnumerator : IEnumerator
	{
		#region Fields

		IDataReader reader;
		bool closeReader;
		SchemaInfo[] schema;
		FieldNameLookup lookup;
		int fieldCount;
	
		#endregion // Fields

		#region Constructors

		public DbEnumerator (IDataReader reader) 
			: this (reader, false)
		{
		}

		public DbEnumerator (IDataReader reader, bool closeReader)
		{
			this.reader = reader;
			this.closeReader = closeReader;
			this.lookup = new FieldNameLookup ();
			this.fieldCount = reader.FieldCount;
			LoadSchema (reader.GetSchemaTable ());
		}

		#endregion // Constructors

		#region Properties

		public virtual object Current {
			get { 
				object[] values = new object[fieldCount];
				reader.GetValues (values);
				return new DbDataRecord (schema, values, lookup); 
			}
		}

		#endregion // Properties

		#region Methods

		public void LoadSchema (DataTable schemaTable)
		{
			schema = new SchemaInfo [fieldCount];
			int index = 0;
			foreach (DataRow row in schemaTable.Rows) {
				SchemaInfo columnSchema = new SchemaInfo ();

				lookup.Add ((string) row["ColumnName"]);

				columnSchema.AllowDBNull = (bool) row ["AllowDBNull"];
				columnSchema.ColumnName = row ["ColumnName"].ToString ();
				columnSchema.ColumnOrdinal = (int) row ["ColumnOrdinal"];
				columnSchema.ColumnSize = (int) row ["ColumnSize"];
				columnSchema.DataTypeName = reader.GetDataTypeName (index);
				columnSchema.FieldType = reader.GetFieldType (index);
				columnSchema.IsReadOnly = (bool) row ["IsReadOnly"];
				columnSchema.TableName = row ["BaseTableName"].ToString ();

				if (row["NumericPrecision"] != DBNull.Value)
					columnSchema.NumericPrecision = (byte) row["NumericPrecision"];
				else
					columnSchema.NumericPrecision = (byte) 0;

				if (row["NumericScale"] != DBNull.Value)
					columnSchema.NumericScale = (byte) row["NumericScale"];
				else
					columnSchema.NumericScale = (byte) 0;

				schema[index] = columnSchema;
				index += 1;
			}
		}

		public virtual bool MoveNext ()
		{
			if (reader.Read ()) 
				return true;
			if (closeReader)
				reader.Close ();
			return false;
		}

		public virtual void Reset ()
		{
			throw new InvalidOperationException ("This enumerator can only go forward.");	
		}
		
		#endregion // Methods
	}
}
