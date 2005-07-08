// 
// OracleParameter.cs
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient
//
// Authors: 
//    Tim Coleman <tim@timcoleman.com>
//    Daniel Moragn <danielmorgan@verizon.net>
//
// Copyright (C) Tim Coleman , 2003
// Copyright (C) Daniel Morgan, 2005
//
// Licensed under the MIT/X11 License.
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.OracleClient.Oci;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Data.OracleClient {
	[TypeConverter (typeof(OracleParameter.OracleParameterConverter))]
	public sealed class OracleParameter : MarshalByRefObject, IDbDataParameter, IDataParameter, ICloneable
	{
		#region Fields

		string name;
		OracleType oracleType = OracleType.VarChar;
		OciDataType ociType;
		int size;
		ParameterDirection direction = ParameterDirection.Input;
		bool isNullable;
		byte precision;
		byte scale;
		string srcColumn;
		DataRowVersion srcVersion;
		DbType dbType = DbType.AnsiString;
		int offset = 0;
		bool sizeSet = false;
		object value = DBNull.Value;
		OciLobLocator lobLocator = null;  // only if Blob or Clob
		IntPtr bindOutValue = IntPtr.Zero;

		OracleParameterCollection container = null;
		OciBindHandle bindHandle;
		OciErrorHandle errorHandle;
		OracleConnection connection;

		int indicator = 0; // TODO: handle indicator to indicate NULL value for OUT parameters
		int bindSize = 0;

		#endregion // Fields

		#region Constructors

		// constructor for cloning the object
		internal OracleParameter (OracleParameter value) {
			this.name = value.name;
			this.oracleType = value.oracleType;
			this.ociType = value.ociType;
			this.size = value.size;
			this.direction = value.direction;
			this.isNullable = value.isNullable;
			this.precision = value.precision;
			this.scale = value.scale;
			this.srcColumn = value.srcColumn;
			this.srcVersion = value.srcVersion;
			this.dbType = value.dbType;
			this.offset = value.offset;
			this.sizeSet = value.sizeSet;
			this.value = value.value;
			this.lobLocator = value.lobLocator;
		}

		public OracleParameter ()
			: this (String.Empty, OracleType.VarChar, 0, ParameterDirection.Input, false, 0, 0, String.Empty, DataRowVersion.Current, null)
		{
		}

		public OracleParameter (string name, object value)
		{
			this.name = name;
			this.value = value;
			SourceVersion = DataRowVersion.Current;
			InferOracleType (value);
		}

		public OracleParameter (string name, OracleType dataType)
			: this (name, dataType, 0, ParameterDirection.Input, false, 0, 0, String.Empty, DataRowVersion.Current, null)
		{
		}

		public OracleParameter (string name, OracleType dataType, int size)
			: this (name, dataType, size, ParameterDirection.Input, false, 0, 0, String.Empty, DataRowVersion.Current, null)
		{
		}

		public OracleParameter (string name, OracleType dataType, int size, string srcColumn)
			: this (name, dataType, size, ParameterDirection.Input, false, 0, 0, srcColumn, DataRowVersion.Current, null)
		{
		}

		public OracleParameter (string name, OracleType dataType, int size, ParameterDirection direction, bool isNullable, byte precision, byte scale, string srcColumn, DataRowVersion srcVersion, object value)
		{
			this.name = name;
			this.size = size;
			this.value = value;

			OracleType = dataType;
			Direction = direction;
			SourceColumn = srcColumn;
			SourceVersion = srcVersion;
		}

		#endregion // Constructors

		#region Properties

		internal OracleParameterCollection Container {
			get { return container; }
			set { container = value; }
		}

		[Browsable (false)]
		[RefreshProperties (RefreshProperties.All)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public DbType DbType {
			get { return dbType; }
			set { SetDbType (value); }
		}

		[DefaultValue (ParameterDirection.Input)]
		[RefreshProperties (RefreshProperties.All)]
		public ParameterDirection Direction {
			get { return direction; }
			set { direction = value; }
		}

		[Browsable (false)]
		[DesignOnly (true)]
		[DefaultValue (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public bool IsNullable {
			get { return isNullable; }
			set { isNullable = value; }
		}

		[DefaultValue (0)]
		[Browsable (false)]
		public int Offset {
			get { return offset; }
			set { offset = value; }
		}

		[DefaultValue (OracleType.VarChar)]
		[RefreshProperties (RefreshProperties.All)]
		public OracleType OracleType {
			get { return oracleType; }
			set { SetOracleType (value); }
		}
		
		[DefaultValue ("")]
		public string ParameterName {
			get { return name; }
			set { name = value; }
		}

		[DefaultValue (0)]
		public byte Precision {
			get { return precision; }
			set { /* NO EFFECT*/ }
		}

		[DefaultValue (0)]
		public byte Scale {
			get { return scale; }
			set { /* NO EFFECT*/ }
		}

		[DefaultValue (0)]
		public int Size {
			get { return size; }
			set { 
				sizeSet = true;
				size = value; 
			}
		}

		[DefaultValue ("")]
		public string SourceColumn {
			get { return srcColumn; }
			set { srcColumn = value; }
		}

		[DefaultValue (DataRowVersion.Current)]
		public DataRowVersion SourceVersion {
			get { return srcVersion; }
			set { srcVersion = value; }
		}

		[DefaultValue (null)]
		[RefreshProperties (RefreshProperties.All)]
		[TypeConverter (typeof(StringConverter))]
		public object Value {
			get { return this.value; }
			set { this.value = value; }
		}

		#endregion // Properties

		#region Methods

		private void AssertSizeIsSet ()
		{
			switch (ociType) {
			case OciDataType.VarChar2:
			case OciDataType.String:
			case OciDataType.VarChar:
			case OciDataType.Char:
			case OciDataType.CharZ:
			case OciDataType.OciString:
				if (!sizeSet)
					throw new Exception ("Size must be set.");
				break;
			default:
				break;
			}
		}

		internal void Bind (OciStatementHandle statement, OracleConnection connection) 
		{
			errorHandle = connection.ErrorHandle;

			if (bindHandle == null)
				bindHandle = new OciBindHandle ((OciHandle) statement);

			IntPtr tmpHandle = bindHandle.Handle;

			if (Direction != ParameterDirection.Input)
				AssertSizeIsSet ();
			if (!sizeSet)
				size = InferSize ();

			bindSize = size;
			byte[] bytes = null;
			object v = value;
			int status = 0;
			OciDataType bindType = ociType;
			IntPtr bindValue = IntPtr.Zero;
			int rsize = 0;

			// TODO: handle InputOutput and Return parameters
			if (direction == ParameterDirection.Output) {
				// TODO: need to figure out how OracleParameter
				//       which uses OciBindHandle to share code
				//       with OciDefineHandle
				switch(ociType) {
					case OciDataType.VarChar2:
					case OciDataType.String:
					case OciDataType.VarChar:
					case OciDataType.Char:
					case OciDataType.CharZ:
					case OciDataType.OciString:
						bindType = OciDataType.Char;
						bindSize = size * 2;
						bindOutValue = Marshal.AllocHGlobal (bindSize);
						break;
					case OciDataType.RowIdDescriptor:
						size = 10;
						bindType = OciDataType.Char;
						bindSize = size * 2;
						bindOutValue = Marshal.AllocHGlobal (bindSize);
						break;
					case OciDataType.Date:
						bindSize = 7;
						bindType = OciDataType.Date;
						bindOutValue = Marshal.AllocHGlobal (bindSize);
						break;
					case OciDataType.Number:
						bindSize = 22;
						bindType = OciDataType.Char;
						bindOutValue = Marshal.AllocHGlobal (bindSize);
						break;
					case OciDataType.Long:
					case OciDataType.LongVarChar:
						// LAMESPEC: you don't know size until you get it;
						// therefore, you must allocate an insane size
						// see OciDefineHandle
						bindSize = OciDefineHandle.LongVarCharMaxValue;
						bindOutValue = Marshal.AllocHGlobal (bindSize);
						bindType = OciDataType.LongVarChar;
						break;
					case OciDataType.Blob:
					case OciDataType.Clob:
						bindSize = -1;
						lobLocator = (OciLobLocator) connection.Environment.Allocate (OciHandleType.LobLocator);
						if (lobLocator == null) {
							OciErrorInfo info = connection.ErrorHandle.HandleError ();
							throw new OracleException (info.ErrorCode, info.ErrorMessage);
						}
						value = lobLocator.Handle;
						lobLocator.ErrorHandle = connection.ErrorHandle;
						lobLocator.Service = statement.Service;
						break;
					default:
						// define other types
						throw new NotImplementedException ();
				}
				bindValue = bindOutValue;
			}
			else if (v == DBNull.Value || v == null) {
				indicator = 0;
				bindType = OciDataType.VarChar2;
				bindSize = 0;
			}
			else {
				// TODO: do other data types and oracle data types
				// should I be using IConvertible to convert?
				if (oracleType == OracleType.DateTime) {
					string oraDateFormat = connection.GetSessionDateFormat ();
					string sysDateFormat = OracleDateTime.ConvertOracleDateFormatToSystemDateTime (oraDateFormat);

					string sDate = "";
					DateTime dt = DateTime.MinValue;
					if (v is String) {
						sDate = (string) v;
						dt = DateTime.Parse (sDate);
					}
					else if (v is DateTime)
						dt = (DateTime) v;
					else if (v is OracleString) {
						sDate = (string) v;
						dt = DateTime.Parse (sDate);
					}
					else if (v is OracleDateTime) {
						OracleDateTime odt = (OracleDateTime) v;
						dt = (DateTime) odt.Value;
					}
					else
						throw new NotImplementedException (); // ?

					sDate = dt.ToString (sysDateFormat);
					rsize = 0;
			
					// Get size of buffer
					OciCalls.OCIUnicodeToCharSet (statement.Parent, null, sDate, out rsize);
			
					// Fill buffer
					bytes = new byte[rsize];
					OciCalls.OCIUnicodeToCharSet (statement.Parent, bytes, sDate, out rsize);
					
					bindType = OciDataType.VarChar2; 
					//bindValue = Marshal.StringToHGlobalAnsi (sDate);
					bindSize = sDate.Length;
				}
				else if (oracleType == OracleType.Blob) {
					bytes = (byte[]) v;
					bindType = OciDataType.LongRaw;
					bindSize = bytes.Length;
				}
				else if (oracleType == OracleType.Clob) {
					string sv = (string) v;
					rsize = 0;
			
					// Get size of buffer
					OciCalls.OCIUnicodeToCharSet (statement.Parent, null, sv, out rsize);
			
					// Fill buffer
					bytes = new byte[rsize];
					OciCalls.OCIUnicodeToCharSet (statement.Parent, bytes, sv, out rsize);

					bindType = OciDataType.Long;
					bindSize = bytes.Length;
				}
				else if (oracleType == OracleType.Raw) {
					byte[] val = v as byte[];
					bindValue = Marshal.AllocHGlobal (val.Length);
					Marshal.Copy (val, 0, bindValue, val.Length);
					bindSize = val.Length;
				}
				else {
					string svalue = v.ToString ();
					rsize = 0;
			
					// Get size of buffer
					OciCalls.OCIUnicodeToCharSet (statement.Parent, null, svalue, out rsize);
			
					// Fill buffer
					bytes = new byte[rsize];
					OciCalls.OCIUnicodeToCharSet (statement.Parent, bytes, svalue, out rsize);

					//bindValue = Marshal.StringToHGlobalAnsi (value.ToString ());
					bindType = OciDataType.VarChar2;
					bindSize = v.ToString ().Length;
				}
			}

			if (bytes != null) {
				status = OciCalls.OCIBindByNameBytes (statement,
					out tmpHandle,
					connection.ErrorHandle,
					ParameterName,
					ParameterName.Length,
					bytes,
					bindSize,
					bindType,
					indicator,
					IntPtr.Zero,
					IntPtr.Zero,
					0,
					IntPtr.Zero,
					0);
			}
			else {
				status = OciCalls.OCIBindByName (statement,
					out tmpHandle,
					connection.ErrorHandle,
					ParameterName,
					ParameterName.Length,
					bindValue,
					bindSize,
					bindType,
					indicator,
					IntPtr.Zero,
					IntPtr.Zero,
					0,
					IntPtr.Zero,
					0);
			}


			if (status != 0) {
				OciErrorInfo info = connection.ErrorHandle.HandleError ();
				throw new OracleException (info.ErrorCode, info.ErrorMessage);
			}

			bindHandle.SetHandle (tmpHandle);
		}

		object ICloneable.Clone ()
		{
			return new OracleParameter(this);
		}

		private void InferOracleType (object value)
		{
			Type type = value.GetType ();
			string exception = String.Format ("The parameter data type of {0} is invalid.", type.Name);
			switch (type.FullName) {
			case "System.Int64":
				SetOracleType (OracleType.Number);
				break;
			case "System.Boolean":
			case "System.Byte":
				SetOracleType (OracleType.Byte);
				break;
			case "System.String":
				SetOracleType (OracleType.VarChar);
				break;
			case "System.DateTime":
				SetOracleType (OracleType.DateTime);
				break;
			case "System.Decimal":
				SetOracleType (OracleType.Number);
				//scale = ((decimal) value).Scale;
				break;
			case "System.Double":
				SetOracleType (OracleType.Double);
				break;
			case "System.Byte[]":
			case "System.Guid":
				SetOracleType (OracleType.Raw);
				break;
			case "System.Int32":
				SetOracleType (OracleType.Int32);
				break;
			case "System.Single":
				SetOracleType (OracleType.Float);
				break;
			case "System.Int16":
				SetOracleType (OracleType.Int16);
				break;
			default:
				throw new ArgumentException (exception);
			}
		}

		private int InferSize ()
		{
			int newSize = 0;
			
			switch (ociType) {
			case OciDataType.VarChar2:
			case OciDataType.String:
			case OciDataType.VarChar:
			case OciDataType.Char:
			case OciDataType.CharZ:
			case OciDataType.OciString:
			case OciDataType.Long:
			case OciDataType.LongVarChar:
				if (value == null || value == DBNull.Value)
					newSize = 0;
				else
					newSize = value.ToString ().Length;
				break;
			case OciDataType.RowIdDescriptor:
				newSize = 10;
				break;
			case OciDataType.Integer:
			case OciDataType.Number:
			case OciDataType.Float:
				newSize = 22;
				break;
			case OciDataType.Date:
				newSize = 7;
				break;	
			case OciDataType.Blob:
			case OciDataType.Clob:
				newSize = -1;
				break;					
			default:
				if (value == null || value == DBNull.Value)
					newSize = 0;
				else
					newSize = value.ToString ().Length;
				break;
			}

			sizeSet = true;

			return newSize;
		}

		private void SetDbType (DbType type)
		{
			string exception = String.Format ("No mapping exists from DbType {0} to a known OracleType.", type);
			switch (type) {
			case DbType.AnsiString:
				oracleType = OracleType.VarChar;
				ociType = OciDataType.VarChar;
				break;
			case DbType.AnsiStringFixedLength:
				oracleType = OracleType.Char;
				ociType = OciDataType.Char;
				break;
			case DbType.Binary:
			case DbType.Guid:
				oracleType = OracleType.Raw;
				ociType = OciDataType.Raw;
				break;
			case DbType.Boolean:
			case DbType.Byte:
				oracleType = OracleType.Byte;
				ociType = OciDataType.Integer;
				break;
			case DbType.Currency:
			case DbType.Decimal:
			case DbType.Int64:
				oracleType = OracleType.Number;
				ociType = OciDataType.Number;
				break;
			case DbType.Date:
			case DbType.DateTime:
			case DbType.Time:
				oracleType = OracleType.DateTime;
				ociType = OciDataType.Char;
				break;
			case DbType.Double:
				oracleType = OracleType.Double;
				ociType = OciDataType.Float;
				break;
			case DbType.Int16:
				oracleType = OracleType.Int16;
				ociType = OciDataType.Integer;
				break;
			case DbType.Int32:
				oracleType = OracleType.Int32;
				ociType = OciDataType.Integer;
				break;
			case DbType.Object:
				oracleType = OracleType.Blob;
				ociType = OciDataType.Blob;
				break;
			case DbType.Single:
				oracleType = OracleType.Float;
				ociType = OciDataType.Float;
				break;
			case DbType.String:
				oracleType = OracleType.NVarChar;
				ociType = OciDataType.VarChar;
				break;
			case DbType.StringFixedLength:
				oracleType = OracleType.NChar;
				ociType = OciDataType.Char;
				break;
			default:
				throw new ArgumentException (exception);
			}
			dbType = type;

		}

		private void SetOracleType (OracleType type)
		{
			string exception = String.Format ("No mapping exists from OracleType {0} to a known DbType.", type);
			switch (type) {
			case OracleType.BFile:
			case OracleType.Blob:
				dbType = DbType.Binary;
				ociType = OciDataType.Blob;
				break;
			case OracleType.LongRaw:
			case OracleType.Raw:
				dbType = DbType.Binary;
				ociType = OciDataType.Raw;
				break;
			case OracleType.Byte:
				dbType = DbType.Byte;
				ociType = OciDataType.Number;
				break;
			case OracleType.Char:
				dbType = DbType.AnsiString;
				ociType = OciDataType.Char;
				break;
			case OracleType.Clob:
				dbType = DbType.AnsiString;
				ociType = OciDataType.Clob;
				break;
			case OracleType.LongVarChar:
			case OracleType.RowId:
			case OracleType.VarChar:
				dbType = DbType.AnsiString;
				ociType = OciDataType.VarChar;
				break;
			case OracleType.Cursor:
			case OracleType.IntervalDayToSecond:
				dbType = DbType.AnsiStringFixedLength;
				ociType = OciDataType.Char;
				break;
			case OracleType.DateTime:
			case OracleType.Timestamp:
			case OracleType.TimestampLocal:
			case OracleType.TimestampWithTZ:
				dbType = DbType.DateTime;
				ociType = OciDataType.Date;
				break;
			case OracleType.Double:
				dbType = DbType.Double;
				ociType = OciDataType.Number;
				break;
			case OracleType.Float:
				dbType = DbType.Single;
				ociType = OciDataType.Number;
				break;
			case OracleType.Int16:
				dbType = DbType.Int16;
				ociType = OciDataType.Number;
				break;
			case OracleType.Int32:
			case OracleType.IntervalYearToMonth:
				dbType = DbType.Int32;
				ociType = OciDataType.Number;
				break;
			case OracleType.NChar:
				dbType = DbType.StringFixedLength;
				ociType = OciDataType.Char;
				break;
			case OracleType.NClob:
			case OracleType.NVarChar:
				dbType = DbType.String;
				ociType = OciDataType.Char;
				break;
			case OracleType.Number:
				dbType = DbType.VarNumeric;
				ociType = OciDataType.Number;
				break;
			case OracleType.SByte:
				dbType = DbType.SByte;
				ociType = OciDataType.Number;
				break;
			case OracleType.UInt16:
				dbType = DbType.UInt16;
				ociType = OciDataType.Number;
				break;
			case OracleType.UInt32:
				dbType = DbType.UInt32;
				ociType = OciDataType.Number;
				break;
			default:
				throw new ArgumentException (exception);
			}

			oracleType = type;
		}

		public override string ToString ()
		{
			return ParameterName;
		}

		internal void Update (OracleCommand cmd) 
		{
			// used to update the parameter value
			// for Output, the output of InputOutput, and Return parameters

			// TODO: handle NULL values by using the indicator variable
			//       referenced in call to OCIBindByName
			value = DBNull.Value;

			byte[] buffer = null;
			object tmp = null;

			switch (ociType) {
			case OciDataType.VarChar2:
			case OciDataType.String:
			case OciDataType.VarChar:
			case OciDataType.Char:
			case OciDataType.CharZ:
			case OciDataType.OciString:
			case OciDataType.RowIdDescriptor:
				buffer = new byte [Size];
				Marshal.Copy (bindOutValue, buffer, 0, Size);
				
				// Get length of returned string
				int 	rsize = 0;
				IntPtr	env = cmd.Connection.Environment;
				OciCalls.OCICharSetToUnicode (env, null, buffer, out rsize);
			
				// Get string
				StringBuilder ret = new StringBuilder(rsize);
				OciCalls.OCICharSetToUnicode (env, ret, buffer, out rsize);
	
				value = ret.ToString ();
				break;
			case OciDataType.Integer:
			case OciDataType.Number:
			case OciDataType.Float:
				tmp = Marshal.PtrToStringAnsi (bindOutValue, bindSize);
				if (tmp != null)
					value = Decimal.Parse (String.Copy ((string) tmp));
				break;
			case OciDataType.Date:
				value = UnpackDate (bindOutValue);
				break;	
			case OciDataType.Blob:
			case OciDataType.Clob:
				OracleLob lob = new OracleLob (lobLocator, ociType);
				lob.connection = connection;
				value = lob;
				break;
			default:
				throw new NotImplementedException ();
			}
		}

		// copied from OciDefineHandle
		[MonoTODO ("Be able to handle negative dates... i.e. BCE.")]
		internal DateTime UnpackDate (IntPtr dateValue)
		{
			byte century = Marshal.ReadByte (dateValue, 0);
			byte year = Marshal.ReadByte (dateValue, 1);
			byte month = Marshal.ReadByte (dateValue, 2);
			byte day = Marshal.ReadByte (dateValue, 3);
			byte hour = Marshal.ReadByte (dateValue, 4);
			byte minute = Marshal.ReadByte (dateValue, 5);
			byte second = Marshal.ReadByte (dateValue, 6);

			return new DateTime ((century - 100) * 100 + (year - 100),
						month,
						day,
						hour - 1,
						minute - 1,
						second - 1);

		}

		#endregion // Methods

		internal sealed class OracleParameterConverter : ExpandableObjectConverter
		{
			public OracleParameterConverter ()
			{
			}

			[MonoTODO]
			public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
			{
				throw new NotImplementedException ();
			}

			[MonoTODO]
			public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
			{
				throw new NotImplementedException ();
			}
		}
	}
}
