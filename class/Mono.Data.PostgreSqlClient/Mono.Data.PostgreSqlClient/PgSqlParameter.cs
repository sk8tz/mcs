//
// Mono.Data.PostgreSqlClient.PgSqlParameter.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc. 2002
//
using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;

namespace Mono.Data.PostgreSqlClient
{
	/// <summary>
	/// Represents a parameter to a Command object, and optionally, 
	/// its mapping to DataSet columns; and is implemented by .NET 
	/// data providers that access data sources.
	/// </summary>
	//public sealed class PgSqlParameter : MarshalByRefObject,
	//	IDbDataParameter, IDataParameter, ICloneable
	public sealed class PgSqlParameter : IDbDataParameter, IDataParameter
	{
		private string parmName;
		private SqlDbType dbtype;
		private DbType    theDbType;
		private object objValue;
		private int size;
		private string sourceColumn;
		private ParameterDirection direction;
		private bool isNullable;
		private byte precision;
		private byte scale;
		private DataRowVersion sourceVersion;
		private int offset;

		[MonoTODO]
		public PgSqlParameter () {
		
		}

		[MonoTODO]
		public PgSqlParameter (string parameterName, object value) {
			this.parmName = parameterName;
			this.objValue = value;
		}
		
		[MonoTODO]
		public PgSqlParameter(string parameterName, SqlDbType dbType) {
			this.parmName = parameterName;
			this.dbtype = dbType;
		}

		[MonoTODO]
		public PgSqlParameter(string parameterName, SqlDbType dbType,
			int size) {

			this.parmName = parameterName;
			this.dbtype = dbType;
			this.size = size;
		}
		
		[MonoTODO]
		public PgSqlParameter(string parameterName, SqlDbType dbType,
			int size, string sourceColumn) {

			this.parmName = parameterName;
			this.dbtype = dbType;
			this.size = size;
			this.sourceColumn = sourceColumn;
		}
			 
		[MonoTODO]
		public PgSqlParameter(string parameterName, SqlDbType dbType,
			int size, ParameterDirection direction, 
			bool isNullable, byte precision,
			byte scale, string sourceColumn,
			DataRowVersion sourceVersion, object value) {
			
			this.parmName = parameterName;
			this.dbtype = dbType;
			this.size = size;
			this.sourceColumn = sourceColumn;
			this.direction = direction;
			this.isNullable = isNullable;
			this.precision = precision;
			this.scale = scale;
			this.sourceVersion = sourceVersion;
			this.objValue = value;
		}

		[MonoTODO]
		public DbType DbType {
			get { 
				return theDbType;
			}
			set { 
				theDbType = value;
			}
		}

		[MonoTODO]
		public ParameterDirection Direction {
			get { 
				return direction;
			}
			set { 
				direction = value;
			}
		}

		[MonoTODO]
		public bool IsNullable	{
			get { 
				return isNullable;
			}
		}

		[MonoTODO]
		public int Offset {
			get {
				return offset;
			}
			
			set {
				offset = value;
			}
		}

		
		string IDataParameter.ParameterName {
			get { 
				return parmName;
			}

			set { 
				parmName = value;
			}
		}
		
		public string ParameterName {
			get { 
				return parmName;
			}

			set { 
				parmName = value;
			}
		}

		[MonoTODO]
		public string SourceColumn {
			get { 
				return sourceColumn;
			}

			set { 
				sourceColumn = value;
			}
		}

		[MonoTODO]
		public DataRowVersion SourceVersion {
			get { 
				return sourceVersion;
			}

			set { 
				sourceVersion = value;
			}
		}
		
		[MonoTODO]
		public SqlDbType SqlDbType {
			get {
				return dbtype;
			}
			
			set {
				dbtype = value;
			}
		}

		[MonoTODO]
		public object Value {
			get { 
				return objValue;
			}

			set { 
				objValue = value;
			}
		}

		[MonoTODO]
		public byte Precision {
			get { 
				return precision;
			}

			set { 
				precision = value;
			}
		}

		[MonoTODO]
                public byte Scale {
			get { 
				return scale;
			}

			set { 
				scale = value;
			}
		}

		[MonoTODO]
                public int Size
		{
			get { 
				return size;
			}

			set { 
				size = value;
			}
		}

		[MonoTODO]
		public override string ToString() {
			return parmName;
		}
	}
}
