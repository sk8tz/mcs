﻿//
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
//    Daniel Moragn <monodanmorg@yahoo.com>
//    Hubert FONGARNAND <informatique.internet@fiducial.fr>
//
// Copyright (C) Tim Coleman , 2003
// Copyright (C) Daniel Morgan, 2005, 2008
// Copyright (C) Hubert FONGARNAND, 2005
//
// Licensed under the MIT/X11 License.
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
#if NET_2_0
using System.Data.Common;
#endif
using System.Data.SqlTypes;
using System.Data.OracleClient.Oci;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Data.OracleClient
{
	[TypeConverter (typeof(OracleParameter.OracleParameterConverter))]
	public sealed class OracleParameter :
#if NET_2_0
		DbParameter, IDbDataParameter, ICloneable
#else
		MarshalByRefObject, IDbDataParameter, IDataParameter, ICloneable
#endif
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
#if NET_2_0
		bool sourceColumnNullMapping;
#endif
		DataRowVersion srcVersion;
		DbType dbType = DbType.AnsiString;
		int offset;
		bool sizeSet;
		bool oracleTypeSet;
		object value = DBNull.Value;
		OciLobLocator lobLocator;  // only if Blob or Clob
		IntPtr bindOutValue = IntPtr.Zero;
		OciDateTimeDescriptor dateTimeDesc;
		IntPtr cursor = IntPtr.Zero;

		OracleParameterCollection container;
		OciBindHandle bindHandle;
		OracleConnection connection;
		byte[] bytes;
		IntPtr bindValue = IntPtr.Zero;
		bool useRef;
		OciDataType bindType;
		OracleType bindOracleType;

		short indicator; 
		int bindSize;

		#endregion // Fields

		#region Constructors

		// constructor for cloning the object
		private OracleParameter (OracleParameter value)
		{
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
			this.oracleTypeSet = value.oracleTypeSet;
		}

		public OracleParameter ()
		{
			this.name = String.Empty;
			this.oracleType = OracleType.VarChar;
			this.size = 0;
			this.direction = ParameterDirection.Input;
			this.isNullable = false;
			this.precision = 0;
			this.scale = 0;
			this.srcColumn = String.Empty;
			this.srcVersion = DataRowVersion.Current;
			this.value = null;
			this.oracleTypeSet = false;
		}

		public OracleParameter (string name, object value)
		{
			this.name = name;
			this.value = value;
			srcColumn = string.Empty;
			SourceVersion = DataRowVersion.Current;
			InferOracleType (value);
		}

		public OracleParameter (string name, OracleType oracleType)
			: this (name, oracleType, 0, ParameterDirection.Input, false, 0, 0, String.Empty, DataRowVersion.Current, null)
		{
		}

		public OracleParameter (string name, OracleType oracleType, int size)
			: this (name, oracleType, size, ParameterDirection.Input, false, 0, 0, String.Empty, DataRowVersion.Current, null)
		{
		}

		public OracleParameter (string name, OracleType oracleType, int size, string srcColumn)
			: this (name, oracleType, size, ParameterDirection.Input, false, 0, 0, srcColumn, DataRowVersion.Current, null)
		{
		}

#if NET_2_0
		public OracleParameter (string name, OracleType oracleType, int size, ParameterDirection direction, string sourceColumn, DataRowVersion sourceVersion, bool sourceColumnNullMapping, object value)
		{
			this.name = name;
			if (size < 0)
				throw new ArgumentException("Size must be not be negative.");
			this.size = size;
			this.value = value;
			SourceColumnNullMapping = sourceColumnNullMapping;
			OracleType = oracleType;
			Direction = direction;
			SourceColumn = sourceColumn;
			SourceVersion = sourceVersion;
		}
#endif

		public OracleParameter (string name, OracleType oracleType, int size, ParameterDirection direction, bool isNullable, byte precision, byte scale, string srcColumn, DataRowVersion srcVersion, object value)
		{
			this.name = name;
			if (size < 0)
				throw new ArgumentException("Size must be not be negative.");
			this.size = size;
			this.value = value;
			this.isNullable = isNullable;
			this.precision = precision;
			this.scale = scale;

			OracleType = oracleType;
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

#if !NET_2_0
		[Browsable (false)]
		[RefreshProperties (RefreshProperties.All)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
#endif
		public
#if NET_2_0
		override
#endif
		DbType DbType {
			get { return dbType; }
			set { SetDbType (value); }
		}

#if !NET_2_0
		[DefaultValue (ParameterDirection.Input)]
#endif
		[RefreshProperties (RefreshProperties.All)]
		public
#if NET_2_0
		override
#endif
		ParameterDirection Direction {
			get { return direction; }
			set { direction = value; }
		}

#if !NET_2_0
		[Browsable (false)]
		[DesignOnly (true)]
		[DefaultValue (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
#endif
		public
#if NET_2_0
		override
#endif
		bool IsNullable {
			get { return isNullable; }
			set { isNullable = value; }
		}

#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Advanced)]
#else
		[DefaultValue (0)]
#endif
		[Browsable (false)]
		public int Offset {
			get { return offset; }
			set { offset = value; }
		}

		[DefaultValue (OracleType.VarChar)]
		[RefreshProperties (RefreshProperties.All)]
#if NET_2_0
		[DbProviderSpecificTypeProperty (true)]
#endif
		public OracleType OracleType {
			get { return oracleType; }
			set { 
				oracleTypeSet = true;
				SetOracleType (value, false); 
			}
		}

#if !NET_2_0
		[DefaultValue ("")]
#endif
		public
#if NET_2_0
		override
#endif
		string ParameterName {
			get {
				if (name == null)
					return string.Empty;
				return name;
			}
			set { name = value; }
		}

#if NET_2_0
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete("Set the precision of a decimal use the Math classes.")]
#else
		[DefaultValue (0)]
#endif
		public byte Precision {
			get { return precision; }
			set { /* NO EFFECT*/ }
		}

#if NET_2_0
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete("Set the precision of a decimal use the Math classes.")]
#else
		[DefaultValue (0)]
#endif
		public byte Scale {
			get { return scale; }
			set { /* NO EFFECT*/ }
		}

#if !NET_2_0
		[DefaultValue (0)]
#endif
		public
#if NET_2_0
		override
#endif
		int Size {
			get { return size; }
			set {
				sizeSet = true;
				size = value;
			}
		}

#if !NET_2_0
		[DefaultValue ("")]
#endif
		public
#if NET_2_0
		override
#endif
		string SourceColumn {
			get { return srcColumn; }
			set { srcColumn = value; }
		}

#if NET_2_0
		[MonoTODO]
		public override bool SourceColumnNullMapping {
			get { return sourceColumnNullMapping; }
			set { sourceColumnNullMapping = value; }
		}
#endif

#if !NET_2_0
		[DefaultValue ("Current")]
#endif
		public
#if NET_2_0
		override
#endif
		DataRowVersion SourceVersion {
			get { return srcVersion; }
			set { srcVersion = value; }
		}

#if !NET_2_0
		[DefaultValue (null)]
#endif
		[RefreshProperties (RefreshProperties.All)]
		[TypeConverter (typeof(StringConverter))]
		public
#if NET_2_0
		override
#endif
		object Value {
			get { return this.value; }
			set {
				this.value = value;
				if (!oracleTypeSet)
					InferOracleType (value);
			}
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

		internal void Bind (OciStatementHandle statement, OracleConnection con, uint pos)
		{
			connection = con;

			if (bindHandle == null)
				bindHandle = new OciBindHandle ((OciHandle) statement);

			IntPtr tmpHandle = bindHandle.Handle;

			if (Direction != ParameterDirection.Input)
				AssertSizeIsSet ();
			if (!sizeSet)
				size = InferSize ();

			bindSize = size;
			object v = value;
			int status = 0;
			bindType = ociType;
			int rsize = 0;

			string svalue;
			string sDate;
			DateTime dt;
			bool isnull = false;

			if (direction == ParameterDirection.Input || direction == ParameterDirection.InputOutput) {
				if (v == null)
					isnull = true;
				else if (v is DBNull)
					isnull = true;
				else {
					INullable mynullable = v as INullable;
					if (mynullable != null)
						isnull = mynullable.IsNull;
				}					
			} 

			if (isnull == true && direction == ParameterDirection.Input) {
				indicator = 0;
				bindType = OciDataType.VarChar2;
				bindSize = 0;
			} else {
				switch(ociType) {
				case OciDataType.VarChar2:
				case OciDataType.String:
				case OciDataType.VarChar:
				case OciDataType.Char:
				case OciDataType.CharZ:
				case OciDataType.OciString:
					bindType = OciDataType.String;
					indicator = 0;
					svalue = "\0";
					// convert value from managed type to type to marshal
					if (direction == ParameterDirection.Input || 
						direction == ParameterDirection.InputOutput) {

						svalue = v.ToString () + '\0';
					}

					// set bind length to size of data
					bindSize = (size + 1) * 4;

					// allocate memory based on bind length
					bytes = new byte[bindSize];				

					if (direction == ParameterDirection.Input ||
						direction == ParameterDirection.InputOutput) {
						
						// convert managed type to memory allocated earlier
						// in this case using OCIUnicodeToCharSet
						rsize = 0;
						// Get size of buffer
						status = OciCalls.OCIUnicodeToCharSet (statement.Parent, null, svalue, out rsize);
						// Fill buffer
						status = OciCalls.OCIUnicodeToCharSet (statement.Parent, bytes, svalue, out rsize);
					}
					break;
				case OciDataType.Date:
					bindType = OciDataType.Date;
					bindSize = 7;
					// convert value from managed type to type to marshal
					if (direction == ParameterDirection.Input || 
						direction == ParameterDirection.InputOutput) {

						if (isnull)
							bytes = new byte [7];
						else {
							sDate = "";
							dt = DateTime.MinValue;
							if (v is String) {
								sDate = (string) v;
								dt = DateTime.Parse (sDate);
							}
							else if (v is DateTime)
								dt = (DateTime) v;
							else if (v is OracleString) {
								sDate = v.ToString ();
								dt = DateTime.Parse (sDate);
							}
							else if (v is OracleDateTime) {
								OracleDateTime odt = (OracleDateTime) v;
								dt = (DateTime) odt.Value;
							}
							else
								throw new NotImplementedException ("For OracleType.DateTime, data type not implemented: " + v.GetType().ToString() + ".");

							// for Input and InputOuput, create byte array and pack DateTime into it
							bytes = PackDate (dt);
						}
					} else	{
						// allocate 7-byte array for Output and ReturnValue to put date
						bytes = new byte [7];
					}
					break;
				case OciDataType.Number:
					bindType = OciDataType.String;
					indicator = 0;
					svalue = "\0";
					// define size for binding
					bindSize = 30; // a NUMBER is 22 bytes but as a string we need more
					// allocate memory
					bytes = new byte [bindSize];
					// convert value from managed type to type to marshal
					if (direction == ParameterDirection.Input || 
						direction == ParameterDirection.InputOutput) {

						svalue = null;
						if(v is IFormattable)
							svalue = ((IFormattable)v).ToString (null, con.SessionFormatProvider);
						else if (v is OracleNumber)
							svalue = ((OracleNumber)v).ToString(con.SessionFormatProvider);
						else
							svalue = v.ToString();

						svalue = svalue + "\0";

						rsize = 0;
						// Get size of buffer
						OciCalls.OCIUnicodeToCharSet (statement.Parent, null, svalue, out rsize);

						// Fill buffer
						bytes = new byte [bindSize];
						OciCalls.OCIUnicodeToCharSet (statement.Parent, bytes, svalue, out rsize);
					} 
					break;
					case OciDataType.Long:
					case OciDataType.LongVarChar:
						bindType = OciDataType.LongVarChar;

						// FIXME: use piecewise fetching for Long, Clob, Blob, and Long Raw
						// See http://download.oracle.com/docs/cd/B19306_01/appdev.102/b14250/oci05bnd.htm#sthref724
						
						bindSize = Size + 5; // 4 bytes prepended for length, bytes, 1 byte NUL character

						indicator = 0;
						svalue = "\0";
						// convert value from managed type to type to marshal
						if (direction == ParameterDirection.Input || 
							direction == ParameterDirection.InputOutput) {

							svalue = v.ToString () + '\0';
						}

						bytes = new byte [bindSize];
						// LONG is only ANSI 
						ASCIIEncoding enc = new ASCIIEncoding ();
						
						if (direction == ParameterDirection.Input || 
							direction == ParameterDirection.InputOutput) {
							int byteCount = 0;
							if (svalue.Length > 0) {	
								byteCount = enc.GetBytes (svalue, 4, svalue.Length, bytes, 0);
								// LONG VARCHAR prepends a 4-byte length
								if (byteCount > 0) {
									byte[] byteArrayLen = BitConverter.GetBytes ((uint) byteCount);
									bytes[0] = byteArrayLen[0];
									bytes[1] = byteArrayLen[1];
									bytes[2] = byteArrayLen[2];
									bytes[3] = byteArrayLen[3];
								}
							}
						}
						break;
					case OciDataType.Clob:
						if (direction == ParameterDirection.Input) {
							svalue = v.ToString();
							rsize = 0;

							// Get size of buffer
							OciCalls.OCIUnicodeToCharSet (statement.Parent, null, svalue, out rsize);

							// Fill buffer
							bytes = new byte[rsize];
							OciCalls.OCIUnicodeToCharSet (statement.Parent, bytes, svalue, out rsize);

							bindType = OciDataType.Long;
							bindSize = bytes.Length;
						} 
						else if (direction == ParameterDirection.InputOutput)
							// not the exact error that .net 2.0 throws, but this is better
							throw new NotImplementedException ("Parameters of OracleType.Clob with direction of InputOutput are not supported.");
						else {
							// Output and Return parameters
							bindSize = -1;
							lobLocator = (OciLobLocator) connection.Environment.Allocate (OciHandleType.LobLocator);
							if (lobLocator == null) {
								OciErrorInfo info = connection.ErrorHandle.HandleError ();
								throw new OracleException (info.ErrorCode, info.ErrorMessage);
							}
							bindOutValue = lobLocator.Handle;
							bindValue = lobLocator.Handle;
							lobLocator.ErrorHandle = connection.ErrorHandle;
							lobLocator.Service = statement.Service;
							useRef = true;
						}
						break;
				default:
					// FIXME: move this up - see how Char, Number, and Date are done...
					if (direction == ParameterDirection.Output || 
						direction == ParameterDirection.InputOutput || 
						direction == ParameterDirection.ReturnValue) {

						switch(ociType) {
						case OciDataType.RowIdDescriptor:
							size = 10;
							bindType = OciDataType.Char;
							bindSize = size * 2;
							bindOutValue = OciCalls.AllocateClear (bindSize);
							bindValue = bindOutValue;
							break;
						case OciDataType.TimeStamp:
							dateTimeDesc = (OciDateTimeDescriptor) connection.Environment.Allocate (OciHandleType.TimeStamp);
							if (dateTimeDesc == null) {
								OciErrorInfo info = connection.ErrorHandle.HandleError ();
								throw new OracleException (info.ErrorCode, info.ErrorMessage);
							}
							dateTimeDesc.ErrorHandle = connection.ErrorHandle;
							bindSize = 11;
							bindType = OciDataType.TimeStamp;
							bindOutValue = dateTimeDesc.Handle;
							bindValue = dateTimeDesc.Handle;
							useRef = true;
							break;
						case OciDataType.Blob:
							bindSize = -1;
							lobLocator = (OciLobLocator) connection.Environment.Allocate (OciHandleType.LobLocator);
							if (lobLocator == null) {
								OciErrorInfo info = connection.ErrorHandle.HandleError ();
								throw new OracleException (info.ErrorCode, info.ErrorMessage);
							}
							bindOutValue = lobLocator.Handle;
							bindValue = lobLocator.Handle;
							lobLocator.ErrorHandle = connection.ErrorHandle;
							lobLocator.Service = statement.Service;
							useRef = true;
							break;
						case OciDataType.RSet: // REF CURSOR
							cursor = IntPtr.Zero;
							OciCalls.OCIHandleAlloc (connection.Environment,
								out cursor,
								OciHandleType.Statement,
								0,
								IntPtr.Zero);

							bindSize = 0;
							bindType = OciDataType.RSet;
							break;
						default:
							// define other types
							throw new NotImplementedException ("Data Type not implemented: " + ociType.ToString() + ".");
						} // switch of ociDataType for output
						bindValue = bindOutValue;
					}
					else if ((v == DBNull.Value || v == null || isnull == true) && direction == ParameterDirection.Input) {
						indicator = 0;
						bindType = OciDataType.VarChar2;
						bindSize = 0;
					}
					else {
						sDate = "";
						dt = DateTime.MinValue;
						if (bindOracleType == OracleType.Timestamp){
							bindType = OciDataType.TimeStamp;
							bindSize = 11;
							dt = DateTime.MinValue;
							sDate = "";
							if (v is String){
								sDate = (string) v;
								dt = DateTime.Parse (sDate);
							}
							else if (v is DateTime)
								dt = (DateTime) v;
							else if (v is OracleString){
								sDate = (string) v;
								dt = DateTime.Parse (sDate);
							}
							else if (v is OracleDateTime) {
								OracleDateTime odt = (OracleDateTime) v;
								dt = (DateTime) odt.Value;
							}
							else
								throw new NotImplementedException ("For OracleType.Timestamp, data type not implemented: " + v.GetType().ToString()); // ?

							short year = (short) dt.Year;
							byte month = (byte) dt.Month;
							byte day = (byte) dt.Day;
							byte hour = (byte) dt.Hour;
							byte min = (byte) dt.Minute;
							byte sec = (byte) dt.Second;
							uint fsec = (uint) dt.Millisecond;
							string timezone = "";
							dateTimeDesc = (OciDateTimeDescriptor) connection.Environment.Allocate (OciHandleType.TimeStamp);
							if (dateTimeDesc == null) {
								OciErrorInfo info = connection.ErrorHandle.HandleError ();
								throw new OracleException (info.ErrorCode, info.ErrorMessage);
							}
							dateTimeDesc.ErrorHandle = connection.ErrorHandle;
							dateTimeDesc.SetDateTime (connection.Session,
								connection.ErrorHandle,
								year, month, day, hour, min, sec, fsec,
								timezone);
							useRef = true;
						}
						else if (bindOracleType == OracleType.Blob) {
							bytes = (byte[]) v;
							bindType = OciDataType.LongRaw;
							bindSize = bytes.Length;
						}
						else if (bindOracleType == OracleType.Raw) {
							byte[] val = v as byte[];
							bindValue = OciCalls.AllocateClear (val.Length);
							Marshal.Copy (val, 0, bindValue, val.Length);
							bindSize = val.Length;
						} else {
							svalue = v.ToString () + '\0';
							rsize = 0;

							// Get size of buffer
							OciCalls.OCIUnicodeToCharSet (statement.Parent, null, svalue, out rsize);

							// Fill buffer
							bytes = new byte[rsize];
							OciCalls.OCIUnicodeToCharSet (statement.Parent, bytes, svalue, out rsize);

							bindType = OciDataType.String;
							bindSize = svalue.Length;
						} // else oracleType
					} // else - Input, Ouput...
					break;
				}			
			}
			
			// Now, call the appropriate OCI Bind function;

			if (useRef == true) {
				if (bindType == OciDataType.TimeStamp) {
					bindValue = dateTimeDesc.Handle;
					status = OciCalls.OCIBindByNameRef (statement,
						out tmpHandle,
						connection.ErrorHandle,
						ParameterName,
						ParameterName.Length,
						ref bindValue,
						bindSize,
						bindType,
						ref indicator,
						IntPtr.Zero,
						IntPtr.Zero,
						0,
						IntPtr.Zero,
						0);
				}
				else {
					status = OciCalls.OCIBindByNameRef (statement,
						out tmpHandle,
						connection.ErrorHandle,
						ParameterName,
						ParameterName.Length,
						ref bindValue,
						bindSize,
						bindType,
						ref indicator,
						IntPtr.Zero,
						IntPtr.Zero,
						0,
						IntPtr.Zero,
						0);
				}
			}
			else if (bindType == OciDataType.RSet) {
				status = OciCalls.OCIBindByNameRef (statement,
					out tmpHandle,
					connection.ErrorHandle,
					ParameterName,
					ParameterName.Length,
					ref cursor,
					bindSize,
					bindType,
					ref indicator,
					IntPtr.Zero,
					IntPtr.Zero,
					0,
					IntPtr.Zero,
					0);
			}
			else if (bytes != null) {
				status = OciCalls.OCIBindByNameBytes (statement,
					out tmpHandle,
					connection.ErrorHandle,
					ParameterName,
					ParameterName.Length,
					bytes,
					bindSize,
					bindType,
					ref indicator,
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
					ParameterName.Length, // FIXME: this should be in bytes!
					bindValue,
					bindSize,
					bindType,
					ref indicator,
					IntPtr.Zero,
					IntPtr.Zero,
					0,
					IntPtr.Zero,
					0);
			}
			OciErrorHandle.ThrowExceptionIfError (connection.ErrorHandle, status);

			bindHandle.SetHandle (tmpHandle);
		}

		object ICloneable.Clone ()
		{
			return new OracleParameter(this);
		}

		private void InferOracleType (object value)
		{
			Type type = value.GetType ();
			string exception = String.Format ("The parameter data type of {0} is invalid.", type.FullName);
			switch (type.FullName) {
			case "System.Int64":
				SetOracleType (OracleType.Number, true);
				break;
			case "System.Boolean":
			case "System.Byte":
				SetOracleType (OracleType.Byte, true);
				break;
			case "System.String":
			case "System.Data.OracleClient.OracleString":
				SetOracleType (OracleType.VarChar, true);
				break;
			case "System.Data.OracleClient.OracleDateTime":
			case "System.DateTime":
				SetOracleType (OracleType.DateTime, true);
				break;
			case "System.Decimal":
			case "System.Data.OracleClient.OracleNumber":
				SetOracleType (OracleType.Number, true);
				break;
			case "System.Double":
				SetOracleType (OracleType.Double, true);
				break;
			case "System.Byte[]":
			case "System.Guid":
				SetOracleType (OracleType.Raw, true);
				break;
			case "System.Int32":
				SetOracleType (OracleType.Int32, true);
				break;
			case "System.Single":
				SetOracleType (OracleType.Float, true);
				break;
			case "System.Int16":
				SetOracleType (OracleType.Int16, true);
				break;
			case "System.DBNull":
				break; //unable to guess type
			case "System.Data.OracleClient.OracleLob":
				SetOracleType (((OracleLob) value).LobType, true); 
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
			case OciDataType.TimeStamp:
				newSize = 11;
 				break;
			case OciDataType.Blob:
			case OciDataType.Clob:
			case OciDataType.RSet: // REF CURSOR
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

		private void SetOracleType (OracleType type, bool inferring)
		{
			FreeHandle ();
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
			case OracleType.Cursor: // REF CURSOR
				ociType = OciDataType.RSet;
				dbType = DbType.Object;
				break;
			case OracleType.IntervalDayToSecond:
				dbType = DbType.AnsiStringFixedLength;
				ociType = OciDataType.Char;
				break;
			case OracleType.Timestamp:
			case OracleType.TimestampLocal:
			case OracleType.TimestampWithTZ:
				dbType = DbType.DateTime;
				ociType = OciDataType.TimeStamp;
				break;
			case OracleType.DateTime:
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

			if (!inferring)
				oracleType = type;
			bindOracleType = type;
		}

#if NET_2_0
		public override void ResetDbType ()
		{
			ResetOracleType ();
		}

		public void ResetOracleType ()
		{
			oracleTypeSet = false;
			InferOracleType (value);
		}
#endif // NET_2_0

		public override string ToString ()
		{
			return ParameterName;
		}

		private void GetOutValue (OracleCommand cmd)
		{
			// used to update the parameter value
			// for Output, the output of InputOutput, and Return parameters
			value = DBNull.Value;
			if (indicator == -1)
				return;

			int rsize = 0;
			IntPtr env = IntPtr.Zero;
			StringBuilder ret = null;

			// FIXME: redo all types - see how Char, Number, and Date are done
			// here and in Bind()

			switch (ociType) {
			case OciDataType.VarChar2:
			case OciDataType.String:
			case OciDataType.VarChar:
			case OciDataType.Char:
			case OciDataType.CharZ:
			case OciDataType.OciString:
			case OciDataType.RowIdDescriptor:
				// Get length of returned string
				rsize = 0;
				env = cmd.Connection.Environment;
				OciCalls.OCICharSetToUnicode (env, null, bytes, out rsize);

				// Get string
				ret = new StringBuilder(rsize);
				OciCalls.OCICharSetToUnicode (env, ret, bytes, out rsize);

				value = ret.ToString ();
				break;
			case OciDataType.Long:
			case OciDataType.LongVarChar:
				int longSize = 0;
				if (BitConverter.IsLittleEndian)
					longSize = BitConverter.ToInt32 (new byte [] {bytes [0], bytes [1], bytes [2], bytes [3]}, 0);
				else
					longSize = BitConverter.ToInt32 (new byte [] {bytes [3], bytes [2], bytes [1], bytes [0]}, 0);

				ASCIIEncoding encoding = new ASCIIEncoding ();
				value = encoding.GetString (bytes, 4, longSize);
				encoding = null;
				break;
			case OciDataType.Integer:
			case OciDataType.Number:
			case OciDataType.Float:
				rsize = 0;
				env = cmd.Connection.Environment;
				OciCalls.OCICharSetToUnicode (env, null, bytes, out rsize);

				// Get string
				ret = new StringBuilder(rsize);
				OciCalls.OCICharSetToUnicode (env, ret, bytes, out rsize);

				// if not empty, parse string as a decimal using session format
				if (ret.Length > 0)
					value = Decimal.Parse (ret.ToString (), cmd.Connection.SessionFormatProvider);
				break;
			case OciDataType.TimeStamp:
				value = dateTimeDesc.GetDateTime (connection.Environment, dateTimeDesc.ErrorHandle);
				break;
			case OciDataType.Date:
				value = UnpackDate (bytes);
				break;
			case OciDataType.Blob:
			case OciDataType.Clob:
				OracleLob lob = new OracleLob (lobLocator, ociType);
				lob.connection = connection;
				value = lob;
				break;
			case OciDataType.RSet: // REF CURSOR				
				OciStatementHandle cursorStatement = GetOutRefCursor (cmd);
				value = new OracleDataReader (cursorStatement.Command, cursorStatement, true, CommandBehavior.Default);
				break;
			default:
				throw new NotImplementedException ("Data Type not implemented: " + ociType.ToString() + ".");
			}
		}

		internal OciStatementHandle GetOutRefCursor (OracleCommand cmd) 
		{
				OciStatementHandle cursorStatement = new OciStatementHandle (cmd.Connection.ServiceContext, cursor);

				cursorStatement.ErrorHandle = cmd.ErrorHandle;
				cursorStatement.Command = cmd;
				cursorStatement.SetupRefCursorResult (cmd.Connection);
				cursorStatement.Service = cmd.Connection.ServiceContext;
				cursor = IntPtr.Zero;
				return cursorStatement;			
		}

		internal void Update (OracleCommand cmd)
		{
			if (Direction != ParameterDirection.Input)
				GetOutValue (cmd);

			FreeHandle ();
		}

		internal void FreeHandle ()
		{
			switch (ociType) {
			case OciDataType.Clob:
			case OciDataType.Blob:
				lobLocator = null;
				break;
			case OciDataType.Raw:
				Marshal.FreeHGlobal (bindValue);
				break;
			case OciDataType.TimeStamp:
				break;
			default:
				Marshal.FreeHGlobal (bindOutValue);
				break;
			}

			bindOutValue = IntPtr.Zero;
			bindValue = IntPtr.Zero;

			bindHandle = null;
			connection = null;
		}

		// copied from OciDefineHandle
		[MonoTODO ("Be able to handle negative dates... i.e. BCE.")]
		private DateTime UnpackDate (byte[] bytes)
		{
			byte century = bytes [0];
			byte year    = bytes [1];
			byte month   = bytes [2];
			byte day     = bytes [3];
			byte hour    = bytes [4];
			byte minute  = bytes [5];
			byte second  = bytes [6];


			return new DateTime ((century - 100) * 100 + (year - 100),
						month,
						day,
						hour - 1,
						minute - 1,
						second - 1);

		}

		private byte[] PackDate (DateTime dateValue)
		{
			byte[] buffer = new byte[7];

			buffer[0] = (byte)((dateValue.Year / 100) + 100); //century
			buffer[1] = (byte)((dateValue.Year % 100) + 100); // Year
			buffer[2] = (byte)dateValue.Month;
			buffer[3] = (byte)dateValue.Day;
			buffer[4] = (byte)(dateValue.Hour+1);
			buffer[5] = (byte)(dateValue.Minute+1);
			buffer[6] = (byte)(dateValue.Second+1);

			return buffer;
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
