//
// Mono.Data.SybaseClient.SybaseDataReader.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc 2002
// (C) Daniel Morgan 2002
// Copyright (C) Tim Coleman, 2002
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

using Mono.Data.SybaseTypes;
using Mono.Data.Tds.Protocol;
using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace Mono.Data.SybaseClient {
	public sealed class SybaseDataReader : MarshalByRefObject, IEnumerable, IDataReader, IDisposable, IDataRecord
	{
		#region Fields

		SybaseCommand command;
		ArrayList dataTypeNames;
		bool disposed = false;
		int fieldCount;
		bool isClosed;
		bool isSelect;
		bool moreResults;
		int resultsRead;
		int rowsRead;
		DataTable schemaTable;

		#endregion // Fields

		#region Constructors

		internal SybaseDataReader (SybaseCommand command)
		{
			this.command = command;
			schemaTable = ConstructSchemaTable ();
			resultsRead = 0;
			fieldCount = 0;
			isClosed = false;
			isSelect = (command.CommandText.Trim ().ToUpper ().StartsWith ("SELECT"));
			command.Tds.RecordsAffected = 0;
			NextResult ();
		}

		#endregion // Constructors

		#region Properties

		public int Depth {
			get { return 0; }
		}

		public int FieldCount {
			get { return fieldCount; }
		}

		public bool IsClosed {
			get { return isClosed; }
		}

		public object this [int i] {
			get { return GetValue (i); }
		}

		public object this [string name] {
			get { return GetValue (GetOrdinal (name)); }
		}
	
		public int RecordsAffected {
			get { 
				if (isSelect) 
					return -1;
				else
					return command.Tds.RecordsAffected; 
			}
		}

		#endregion // Properties

		#region Methods

		public void Close ()
		{
			isClosed = true;
			command.CloseDataReader (moreResults);
		}

		private static DataTable ConstructSchemaTable ()
		{
			Type booleanType = typeof (bool);
			Type stringType = typeof (string);
			Type intType = typeof (int);
			Type typeType = typeof (Type);
			Type shortType = typeof (short);

			DataTable schemaTable = new DataTable ("SchemaTable");
			schemaTable.Columns.Add ("ColumnName", stringType);
			schemaTable.Columns.Add ("ColumnOrdinal", intType);
			schemaTable.Columns.Add ("ColumnSize", intType);
			schemaTable.Columns.Add ("NumericPrecision", shortType);
			schemaTable.Columns.Add ("NumericScale", shortType);
			schemaTable.Columns.Add ("IsUnique", booleanType);
			schemaTable.Columns.Add ("IsKey", booleanType);
			schemaTable.Columns.Add ("BaseServerName", stringType);
			schemaTable.Columns.Add ("BaseCatalogName", stringType);
			schemaTable.Columns.Add ("BaseColumnName", stringType);
			schemaTable.Columns.Add ("BaseSchemaName", stringType);
			schemaTable.Columns.Add ("BaseTableName", stringType);
			schemaTable.Columns.Add ("DataType", typeType);
			schemaTable.Columns.Add ("AllowDBNull", booleanType);
			schemaTable.Columns.Add ("ProviderType", intType);
			schemaTable.Columns.Add ("IsAliased", booleanType);
			schemaTable.Columns.Add ("IsExpression", booleanType);
			schemaTable.Columns.Add ("IsIdentity", booleanType);
			schemaTable.Columns.Add ("IsAutoIncrement", booleanType);
			schemaTable.Columns.Add ("IsRowVersion", booleanType);
			schemaTable.Columns.Add ("IsHidden", booleanType);
			schemaTable.Columns.Add ("IsLong", booleanType);
			schemaTable.Columns.Add ("IsReadOnly", booleanType);

			return schemaTable;
		}

		private void Dispose (bool disposing) 
		{
			if (!disposed) {
				if (disposing) {
					schemaTable.Dispose ();
					Close ();
				}
				disposed = true;
			}
		}

		public bool GetBoolean (int i)
		{
			object value = GetValue (i);
			if (!(value is bool)) {
				if (value is DBNull) throw new SybaseNullValueException ();
				throw new InvalidCastException ();
			}
			return (bool) value;
		}

		public byte GetByte (int i)
		{
			object value = GetValue (i);
			if (!(value is byte)) {
				if (value is DBNull) throw new SybaseNullValueException ();
				throw new InvalidCastException ();
			}
			return (byte) value;
		}

		public long GetBytes (int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
		{
			object value = GetValue (i);
			if (!(value is byte [])) {
				if (value is DBNull) throw new SybaseNullValueException ();
				throw new InvalidCastException ();
			}
			Array.Copy ((byte []) value, (int) dataIndex, buffer, bufferIndex, length);
			return ((byte []) value).Length - dataIndex;
		}

		public char GetChar (int i)
		{
			object value = GetValue (i);
			if (!(value is char)) {
				if (value is DBNull) throw new SybaseNullValueException ();
				throw new InvalidCastException ();
			}
			return (char) value;
		}

		public long GetChars (int i, long dataIndex, char[] buffer, int bufferIndex, int length)
		{
			object value = GetValue (i);
			if (!(value is char[])) {
				if (value is DBNull) throw new SybaseNullValueException ();
				throw new InvalidCastException ();
			}
			Array.Copy ((char []) value, (int) dataIndex, buffer, bufferIndex, length);
			return ((char []) value).Length - dataIndex;
		}

		[MonoTODO ("Implement GetData")]
		public IDataReader GetData (int i)
		{
			throw new NotImplementedException ();
		}

		public string GetDataTypeName (int i)
		{
			return (string) dataTypeNames [i];
		}

		public DateTime GetDateTime (int i)
		{
			object value = GetValue (i);
			if (!(value is DateTime)) {
				if (value is DBNull) throw new SybaseNullValueException ();
				throw new InvalidCastException ();
			}
			return (DateTime) value;
		}

		public decimal GetDecimal (int i)
		{
			object value = GetValue (i);
			if (!(value is decimal)) {
				if (value is DBNull) throw new SybaseNullValueException ();
				throw new InvalidCastException ();
			}
			return (decimal) value;
		}

		public double GetDouble (int i)
		{
			object value = GetValue (i);
			if (!(value is double)) {
				if (value is DBNull) throw new SybaseNullValueException ();
				throw new InvalidCastException ();
			}
			return (double) value;
		}

		public Type GetFieldType (int i)
		{
			return (Type) schemaTable.Rows[i]["DataType"];
		}

		public float GetFloat (int i)
		{
			object value = GetValue (i);
			if (!(value is float)) {
				if (value is DBNull) throw new SybaseNullValueException ();
				throw new InvalidCastException ();
			}
			return (float) value;
		}

		public Guid GetGuid (int i)
		{
			object value = GetValue (i);
			if (!(value is Guid)) {
				if (value is DBNull) throw new SybaseNullValueException ();
				throw new InvalidCastException ();
			}
			return (Guid) value;
		}

		public short GetInt16 (int i)
		{
			object value = GetValue (i);
			if (!(value is short)) {
				if (value is DBNull) throw new SybaseNullValueException ();
				throw new InvalidCastException ();
			}
			return (short) value;
		}

		public int GetInt32 (int i)
		{
			object value = GetValue (i);
			if (!(value is int)) {
				if (value is DBNull) throw new SybaseNullValueException ();
				throw new InvalidCastException ();
			}
			return (int) value;
		}

		public long GetInt64 (int i)
		{
			object value = GetValue (i);
			if (!(value is long)) {
				if (value is DBNull) throw new SybaseNullValueException ();
				throw new InvalidCastException ();
			}
			return (long) value;
		}

		public string GetName (int i)
		{
			return (string) schemaTable.Rows[i]["ColumnName"];
		}

		public int GetOrdinal (string name)
		{
			foreach (DataRow schemaRow in schemaTable.Rows)
				if (((string) schemaRow ["ColumnName"]).Equals (name))
					return (int) schemaRow ["ColumnOrdinal"];
			foreach (DataRow schemaRow in schemaTable.Rows)
				if (String.Compare (((string) schemaRow ["ColumnName"]), name, true) == 0)
					return (int) schemaRow ["ColumnOrdinal"];
			throw new IndexOutOfRangeException ();
		}

		public DataTable GetSchemaTable ()
		{
			if (schemaTable.Rows != null && schemaTable.Rows.Count > 0)
				return schemaTable;

			if (!moreResults)
				return null;

			fieldCount = 0;

			dataTypeNames = new ArrayList ();

			foreach (TdsDataColumn schema in command.Tds.Columns) {
				DataRow row = schemaTable.NewRow ();

				row ["ColumnName"]		= GetSchemaValue (schema, "ColumnName");
				row ["ColumnSize"]		= GetSchemaValue (schema, "ColumnSize"); 
				row ["ColumnOrdinal"]		= GetSchemaValue (schema, "ColumnOrdinal");
				row ["NumericPrecision"]	= GetSchemaValue (schema, "NumericPrecision");
				row ["NumericScale"]		= GetSchemaValue (schema, "NumericScale");
				row ["IsUnique"]		= GetSchemaValue (schema, "IsUnique");
				row ["IsKey"]			= GetSchemaValue (schema, "IsKey");
				row ["BaseServerName"]		= GetSchemaValue (schema, "BaseServerName");
				row ["BaseCatalogName"]		= GetSchemaValue (schema, "BaseCatalogName");
				row ["BaseColumnName"]		= GetSchemaValue (schema, "BaseColumnName");
				row ["BaseSchemaName"]		= GetSchemaValue (schema, "BaseSchemaName");
				row ["BaseTableName"]		= GetSchemaValue (schema, "BaseTableName");
				row ["AllowDBNull"]		= GetSchemaValue (schema, "AllowDBNull");
				row ["IsAliased"]		= GetSchemaValue (schema, "IsAliased");
				row ["IsExpression"]		= GetSchemaValue (schema, "IsExpression");
				row ["IsIdentity"]		= GetSchemaValue (schema, "IsIdentity");
				row ["IsAutoIncrement"]		= GetSchemaValue (schema, "IsAutoIncrement");
				row ["IsRowVersion"]		= GetSchemaValue (schema, "IsRowVersion");
				row ["IsHidden"]		= GetSchemaValue (schema, "IsHidden");
				row ["IsReadOnly"]		= GetSchemaValue (schema, "IsReadOnly");

				// We don't always get the base column name.
				if (row ["BaseColumnName"] == DBNull.Value)
					row ["BaseColumnName"] = row ["ColumnName"];

				switch ((TdsColumnType) schema ["ColumnType"]) {
					case TdsColumnType.Image :
						dataTypeNames.Add ("image");
						row ["ProviderType"] = (int) SybaseType.Image;
						row ["DataType"] = typeof (byte[]);
						row ["IsLong"] = true;
						break;
					case TdsColumnType.Text :
						dataTypeNames.Add ("text");
						row ["ProviderType"] = (int) SybaseType.Text;
						row ["DataType"] = typeof (string);
						row ["IsLong"] = true;
						break;
					case TdsColumnType.UniqueIdentifier :
						dataTypeNames.Add ("uniqueidentifier");
						row ["ProviderType"] = (int) SybaseType.UniqueIdentifier;
						row ["DataType"] = typeof (Guid);
						row ["IsLong"] = false;
						break;
					case TdsColumnType.VarBinary :
					case TdsColumnType.BigVarBinary :
						dataTypeNames.Add ("varbinary");
						row ["ProviderType"] = (int) SybaseType.VarBinary;
						row ["DataType"] = typeof (byte[]);
						row ["IsLong"] = true;
						break;
					case TdsColumnType.IntN :
					case TdsColumnType.Int4 :
						dataTypeNames.Add ("int");
						row ["ProviderType"] = (int) SybaseType.Int;
						row ["DataType"] = typeof (int);
						row ["IsLong"] = false;
						break;
					case TdsColumnType.VarChar :
					case TdsColumnType.BigVarChar :
						dataTypeNames.Add ("varchar");
						row ["ProviderType"] = (int) SybaseType.VarChar;
						row ["DataType"] = typeof (string);
						row ["IsLong"] = false;
						break;
					case TdsColumnType.Binary :
					case TdsColumnType.BigBinary :
						dataTypeNames.Add ("binary");
						row ["ProviderType"] = (int) SybaseType.Binary;
						row ["DataType"] = typeof (byte[]);
						row ["IsLong"] = true;
						break;
					case TdsColumnType.Char :
					case TdsColumnType.BigChar :
						dataTypeNames.Add ("char");
						row ["ProviderType"] = (int) SybaseType.Char;
						row ["DataType"] = typeof (string);
						row ["IsLong"] = false;
						break;
					case TdsColumnType.Int1 :
						dataTypeNames.Add ("tinyint");
						row ["ProviderType"] = (int) SybaseType.TinyInt;
						row ["DataType"] = typeof (byte);
						row ["IsLong"] = false;
						break;
					case TdsColumnType.Bit :
					case TdsColumnType.BitN :
						dataTypeNames.Add ("bit");
						row ["ProviderType"] = (int) SybaseType.Bit;
						row ["DataType"] = typeof (bool);
						row ["IsLong"] = false;
						break;
					case TdsColumnType.Int2 :
						dataTypeNames.Add ("smallint");
						row ["ProviderType"] = (int) SybaseType.SmallInt;
						row ["DataType"] = typeof (short);
						row ["IsLong"] = false;
						break;
					case TdsColumnType.DateTime4 :
					case TdsColumnType.DateTime :
					case TdsColumnType.DateTimeN :
						dataTypeNames.Add ("datetime");
						row ["ProviderType"] = (int) SybaseType.DateTime;
						row ["DataType"] = typeof (DateTime);
						row ["IsLong"] = false;
						break;
					case TdsColumnType.Real :
						dataTypeNames.Add ("real");
						row ["ProviderType"] = (int) SybaseType.Real;
						row ["DataType"] = typeof (float);
						break;
					case TdsColumnType.Money :
					case TdsColumnType.MoneyN :
					case TdsColumnType.Money4 :
						dataTypeNames.Add ("money");
						row ["ProviderType"] = (int) SybaseType.Money;
						row ["DataType"] = typeof (decimal);
						row ["IsLong"] = false;
						break;
					case TdsColumnType.Float8 :
					case TdsColumnType.FloatN :
						dataTypeNames.Add ("float");
						row ["ProviderType"] = (int) SybaseType.Float;
						row ["DataType"] = typeof (double);
						row ["IsLong"] = false;
						break;
					case TdsColumnType.NText :
						dataTypeNames.Add ("ntext");
						row ["ProviderType"] = (int) SybaseType.NText;
						row ["DataType"] = typeof (string);
						row ["IsLong"] = true;
						break;
					case TdsColumnType.NVarChar :
						dataTypeNames.Add ("nvarchar");
						row ["ProviderType"] = (int) SybaseType.NVarChar;
						row ["DataType"] = typeof (string);
						row ["IsLong"] = false;
						break;
					case TdsColumnType.Decimal :
					case TdsColumnType.Numeric :
						dataTypeNames.Add ("decimal");
						row ["ProviderType"] = (int) SybaseType.Decimal;
						row ["DataType"] = typeof (decimal);
						row ["IsLong"] = false;
						break;
					case TdsColumnType.NChar :
						dataTypeNames.Add ("nchar");
						row ["ProviderType"] = (int) SybaseType.NChar;
						row ["DataType"] = typeof (string);
						row ["IsLong"] = false;
						break;
					case TdsColumnType.SmallMoney :
						dataTypeNames.Add ("smallmoney");
						row ["ProviderType"] = (int) SybaseType.SmallMoney;
						row ["DataType"] = typeof (decimal);
						row ["IsLong"] = false;
						break;
					default :
						dataTypeNames.Add ("variant");
						row ["ProviderType"] = (int) SybaseType.Variant;
						row ["DataType"] = typeof (object);
						row ["IsLong"] = false;
						break;
				}

				schemaTable.Rows.Add (row);

				fieldCount += 1;
			}
			return schemaTable;
		}		

		private static object GetSchemaValue (TdsDataColumn schema, string key)
		{
			object val = schema [key];
			if (val != null)
				return val;
			else
				return DBNull.Value;
		}

		public SybaseBinary GetSybaseBinary (int i)
		{
			throw new NotImplementedException ();
		}

		public SybaseBoolean GetSybaseBoolean (int i) 
		{
			object value = GetSybaseValue (i);
			if (!(value is SybaseBoolean))
				throw new InvalidCastException ();
			return (SybaseBoolean) value;
		}

		public SybaseByte GetSybaseByte (int i)
		{
			object value = GetSybaseValue (i);
			if (!(value is SybaseByte))
				throw new InvalidCastException ();
			return (SybaseByte) value;
		}

		public SybaseDateTime GetSybaseDateTime (int i)
		{
			object value = GetSybaseValue (i);
			if (!(value is SybaseDateTime))
				throw new InvalidCastException ();
			return (SybaseDateTime) value;
		}

		public SybaseDecimal GetSybaseDecimal (int i)
		{
			object value = GetSybaseValue (i);
			if (!(value is SybaseDecimal))
				throw new InvalidCastException ();
			return (SybaseDecimal) value;
		}

		public SybaseDouble GetSybaseDouble (int i)
		{
			object value = GetSybaseValue (i);
			if (!(value is SybaseDouble))
				throw new InvalidCastException ();
			return (SybaseDouble) value;
		}

		public SybaseGuid GetSybaseGuid (int i)
		{
			object value = GetSybaseValue (i);
			if (!(value is SybaseGuid))
				throw new InvalidCastException ();
			return (SybaseGuid) value;
		}

		public SybaseInt16 GetSybaseInt16 (int i)
		{
			object value = GetSybaseValue (i);
			if (!(value is SybaseInt16))
				throw new InvalidCastException ();
			return (SybaseInt16) value;
		}

		public SybaseInt32 GetSybaseInt32 (int i)
		{
			object value = GetSybaseValue (i);
			if (!(value is SybaseInt32))
				throw new InvalidCastException ();
			return (SybaseInt32) value;
		}

		public SybaseInt64 GetSybaseInt64 (int i)
		{
			object value = GetSybaseValue (i);
			if (!(value is SybaseInt64))
				throw new InvalidCastException ();
			return (SybaseInt64) value;
		}

		public SybaseMoney GetSybaseMoney (int i)
		{
			object value = GetSybaseValue (i);
			if (!(value is SybaseMoney))
				throw new InvalidCastException ();
			return (SybaseMoney) value;
		}

		public SybaseSingle GetSybaseSingle (int i)
		{
			object value = GetSybaseValue (i);
			if (!(value is SybaseSingle))
				throw new InvalidCastException ();
			return (SybaseSingle) value;
		}

		public SybaseString GetSybaseString (int i)
		{
			object value = GetSybaseValue (i);
			if (!(value is SybaseString))
				throw new InvalidCastException ();
			return (SybaseString) value;
		}

		[MonoTODO ("Implement TdsBigDecimal conversion.  SybaseType.Real fails tests?")]
		public object GetSybaseValue (int i)
		{
			SybaseType type = (SybaseType) (schemaTable.Rows [i]["ProviderType"]);
			object value = GetValue (i);

			switch (type) {
			case SybaseType.BigInt:
				if (value == DBNull.Value)
					return SybaseInt64.Null;
				return (SybaseInt64) ((long) value);
			case SybaseType.Binary:
			case SybaseType.Image:
			case SybaseType.VarBinary:
			case SybaseType.Timestamp:
				if (value == DBNull.Value)
					return SybaseBinary.Null;
				return (SybaseBinary) ((byte[]) value);
			case SybaseType.Bit:
				if (value == DBNull.Value)
					return SybaseBoolean.Null;
				return (SybaseBoolean) ((bool) value);
			case SybaseType.Char:
			case SybaseType.NChar:
			case SybaseType.NText:
			case SybaseType.NVarChar:
			case SybaseType.Text:
			case SybaseType.VarChar:
				if (value == DBNull.Value)
					return SybaseString.Null;
				return (SybaseString) ((string) value);
			case SybaseType.DateTime:
			case SybaseType.SmallDateTime:
				if (value == DBNull.Value)
					return SybaseDateTime.Null;
				return (SybaseDateTime) ((DateTime) value);
			case SybaseType.Decimal:
				if (value == DBNull.Value)
					return SybaseDecimal.Null;
				if (value is TdsBigDecimal)
					return SybaseDecimal.FromTdsBigDecimal ((TdsBigDecimal) value);
				return (SybaseDecimal) ((decimal) value);
			case SybaseType.Float:
				if (value == DBNull.Value)
					return SybaseDouble.Null;
				return (SybaseDouble) ((double) value);
			case SybaseType.Int:
				if (value == DBNull.Value)
					return SybaseInt32.Null;
				return (SybaseInt32) ((int) value);
			case SybaseType.Money:
			case SybaseType.SmallMoney:
				if (value == DBNull.Value)
					return SybaseMoney.Null;
				return (SybaseMoney) ((decimal) value);
			case SybaseType.Real:
				if (value == DBNull.Value)
					return SybaseSingle.Null;
				return (SybaseSingle) ((float) value);
			case SybaseType.UniqueIdentifier:
				if (value == DBNull.Value)
					return SybaseGuid.Null;
				return (SybaseGuid) ((Guid) value);
			case SybaseType.SmallInt:
				if (value == DBNull.Value)
					return SybaseInt16.Null;
				return (SybaseInt16) ((short) value);
			case SybaseType.TinyInt:
				if (value == DBNull.Value)
					return SybaseByte.Null;
				return (SybaseByte) ((byte) value);
			}

			throw new InvalidOperationException ("The type of this column is unknown.");
		}

		public int GetSybaseValues (object[] values)
		{
			int count = 0;
			int columnCount = schemaTable.Rows.Count;
			int arrayCount = values.Length;

			if (arrayCount > columnCount)
				count = columnCount;
			else
				count = arrayCount;

			for (int i = 0; i < count; i += 1) 
				values [i] = GetSybaseValue (i);

			return count;
		}

		public string GetString (int i)
		{
			object value = GetValue (i);
			if (!(value is string)) {
				if (value is DBNull) throw new SybaseNullValueException ();
				throw new InvalidCastException ();
			}
			return (string) value;
		}

		public object GetValue (int i)
		{
			return command.Tds.ColumnValues [i];
		}

		public int GetValues (object[] values)
		{
			int len = values.Length;
			int bigDecimalIndex = command.Tds.ColumnValues.BigDecimalIndex;

			// If a four-byte decimal is stored, then we can't convert to
			// a native type.  Throw an OverflowException.
			if (bigDecimalIndex >= 0 && bigDecimalIndex < len)
				throw new OverflowException ();

			command.Tds.ColumnValues.CopyTo (0, values, 0, len);
			return (len > FieldCount ? len : FieldCount);
		}

		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new DbEnumerator (this);
		}

		public bool IsDBNull (int i)
		{
			return GetValue (i) == DBNull.Value;
		}

		public bool NextResult ()
		{
			if ((command.CommandBehavior & CommandBehavior.SingleResult) != 0 && resultsRead > 0)
				return false;
			if (command.Tds.DoneProc)
				return false;

			schemaTable.Rows.Clear ();

			moreResults = command.Tds.NextResult ();
			GetSchemaTable ();

			rowsRead = 0;
			resultsRead += 1;
			return moreResults;
		}

		public bool Read ()
		{
			if ((command.CommandBehavior & CommandBehavior.SingleRow) != 0 && rowsRead > 0)
				return false;
			if ((command.CommandBehavior & CommandBehavior.SchemaOnly) != 0)
				return false;
			if (!moreResults)
				return false;

			bool result = command.Tds.NextRow ();

			rowsRead += 1;

			return result;
		}

		#endregion // Methods
	}
}
