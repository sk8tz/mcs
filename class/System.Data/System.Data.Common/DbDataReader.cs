//
// System.Data.Common.DbDataReader.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
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

#if NET_2_0

using System.Collections;
using System.Data;

namespace System.Data.Common {
	public abstract class DbDataReader : MarshalByRefObject, IDataReader, IDataReader2, IDataRecord, IDataRecord2, IDisposable, IEnumerable
	{
		#region Constructors

		protected DbDataReader ()
		{
		}

		#endregion // Constructors

		#region Properties

		public abstract int Depth { get; }
		public abstract int FieldCount { get; }
		public abstract bool HasRows { get; }
		public abstract bool IsClosed { get; }
		public abstract object this [int index] { get; }
		public abstract object this [string name] { get; }
		public abstract int RecordsAffected { get; }
		public abstract int VisibleFieldCount { get; }

		#endregion // Properties

		#region Methods

		public abstract void Close ();
		public abstract void Dispose ();
		public abstract bool GetBoolean (int i);
		public abstract byte GetByte (int i);
		public abstract long GetBytes (int i, long fieldOffset, byte[] buffer, int bufferOffset, int length);
		public abstract char GetChar (int i);
		public abstract long GetChars (int i, long dataIndex, char[] buffer, int bufferIndex, int length);

		[MonoTODO]
		public DbDataReader GetData (int i)
		{
			throw new NotImplementedException ();
		}

		public abstract string GetDataTypeName (int i);
		public abstract DateTime GetDateTime (int i);
		public abstract decimal GetDecimal (int i);
		public abstract double GetDouble (int i);
		public abstract IEnumerator GetEnumerator ();
		public abstract Type GetFieldProviderSpecificType (int i);
		public abstract Type GetFieldType (int i);
		public abstract float GetFloat (int i);
		public abstract Guid GetGuid (int i);
		public abstract short GetInt16 (int i);
		public abstract int GetInt32 (int i);
		public abstract long GetInt64 (int i);
		public abstract string GetName (int i);
		public abstract int GetOrdinal (string name);
		public abstract object GetProviderSpecificValue (int i);
		public abstract int GetProviderSpecificValues (object[] values);
		public abstract DataTable GetSchemaTable ();
		public abstract string GetString (int i);
		public abstract object GetValue (int i);
		public abstract int GetValues (object[] values);

		IDataReader IDataRecord.GetData (int i)
		{
			return ((IDataReader) this).GetData (i);
		}

		public abstract bool IsDBNull (int i);
		public abstract bool NextResult ();
		public abstract bool Read ();

                internal static DataTable GetSchemaTableTemplate ()
		{
			Type booleanType        = Type.GetType ("System.Boolean");
			Type stringType         = Type.GetType ("System.String");
			Type intType            = Type.GetType ("System.Int32");
			Type typeType           = Type.GetType ("System.Type");
			Type shortType          = Type.GetType ("System.Int16");

			DataTable schemaTable = new DataTable ("SchemaTable");
			schemaTable.Columns.Add ("ColumnName",          stringType);
			schemaTable.Columns.Add ("ColumnOrdinal",       intType);
			schemaTable.Columns.Add ("ColumnSize",          intType);
			schemaTable.Columns.Add ("NumericPrecision",    shortType);
			schemaTable.Columns.Add ("NumericScale",        shortType);
			schemaTable.Columns.Add ("IsUnique",            booleanType);
			schemaTable.Columns.Add ("IsKey",               booleanType);
			schemaTable.Columns.Add ("BaseServerName",      stringType);
			schemaTable.Columns.Add ("BaseCatalogName",     stringType);
			schemaTable.Columns.Add ("BaseColumnName",      stringType);
			schemaTable.Columns.Add ("BaseSchemaName",      stringType);
			schemaTable.Columns.Add ("BaseTableName",       stringType);
			schemaTable.Columns.Add ("DataType",            typeType);
			schemaTable.Columns.Add ("AllowDBNull",         booleanType);
			schemaTable.Columns.Add ("ProviderType",        intType);
			schemaTable.Columns.Add ("IsAliased",           booleanType);
			schemaTable.Columns.Add ("IsExpression",        booleanType);
			schemaTable.Columns.Add ("IsIdentity",          booleanType);
			schemaTable.Columns.Add ("IsAutoIncrement",     booleanType);
			schemaTable.Columns.Add ("IsRowVersion",        booleanType);
			schemaTable.Columns.Add ("IsHidden",            booleanType);
			schemaTable.Columns.Add ("IsLong",              booleanType);
			schemaTable.Columns.Add ("IsReadOnly",          booleanType);

			return schemaTable;
		}

		#endregion // Methods
	}
}

#endif // NET_2_0
