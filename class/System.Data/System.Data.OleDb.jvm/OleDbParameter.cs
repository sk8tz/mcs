//
// System.Data.OleDb.OleDbParameter
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//	Boris Kirzner <borisk@mainsoft.com>
//	
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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


using System;
using System.Text;
using System.Data;
using System.Data.Common;
using System.Data.ProviderBase;

using java.sql;
using java.lang;

namespace System.Data.OleDb
{
    public sealed class OleDbParameter : AbstractDbParameter, IDbDataParameter, ICloneable
    {

		#region Fields

        internal OleDbType _oleDbType = OleDbType.VarWChar;
		private bool _isOracleRefCursor = false;

		#endregion // Fields
    
		#region Constructors

        public OleDbParameter()
        {
        }
    
        public OleDbParameter(String parameterName, Object value)
			: this (parameterName, OleDbType.VarWChar, 0, ParameterDirection.Input,
					false, 0, 0, String.Empty, DataRowVersion.Current, value)
        {
			_isDbTypeSet = false;
        }
    
        public OleDbParameter(String parameterName, OleDbType dbType)
			: this (parameterName, dbType, 0, ParameterDirection.Input,
					false, 0, 0, String.Empty, DataRowVersion.Current, null)
        {
        }
    
        public OleDbParameter(String parameterName, OleDbType dbType, int size)
			: this (parameterName, dbType, size, ParameterDirection.Input,
					false, 0, 0, String.Empty, DataRowVersion.Current, null)
        {
        }
    
        public OleDbParameter(String parameterName, OleDbType dbType, int size,
            String sourceColumn)
			: this (parameterName, dbType, size, ParameterDirection.Input,
					false, 0, 0, sourceColumn, DataRowVersion.Current, null)
        {
        }
    
        
        public OleDbParameter(String parameterName, 
							OleDbType dbType, 
							int size,
							ParameterDirection direction, 
							bool isNullable,
							byte precision, 
							byte scale, 
							String sourceColumn,
							DataRowVersion sourceVersion, 
							Object value)
        {
            ParameterName = parameterName;
            OleDbType = dbType;
            Size = size;
            Direction = direction;
            IsNullable = isNullable;
            Precision = precision;
            Scale = scale;
            SourceColumn = sourceColumn;
            SourceVersion = sourceVersion;
            Value = value;
        }

		#endregion // Constructors

		#region Properties

		public override DbType DbType
        {
            get { return OleDbConvert.OleDbTypeToDbType(_oleDbType); }           
			set { _oleDbType = OleDbConvert.DbTypeToOleDbType(value); }
        }                
        
        public OleDbType OleDbType
        {
            get { return _oleDbType; }            
			set {
                _oleDbType = value;
				_isDbTypeSet = true;
            }
        }    
    
        public new Object Value
        {
            get { return base.Value; }
            set {
                if (!_isDbTypeSet && (value != null)) {
                    _oleDbType = OleDbConvert.ValueTypeToOleDbType(value.GetType());
				}
                base.Value = value;
            }
        }

		internal override bool IsSpecial {
			get {
				return (Direction == ParameterDirection.Output) && IsOracleRefCursor;
			}
		}


		internal bool IsOracleRefCursor
		{
			get { return _isOracleRefCursor; }
			set { _isOracleRefCursor = value; }
		}

		#endregion // Properties

		#region Methods

		public override String ToString()
        {
            return ParameterName;
        }
    
        public override object Clone()
        {
            OleDbParameter clone = new OleDbParameter();
			CopyTo(clone);

            clone._oleDbType = _oleDbType;
			clone._isDbTypeSet = _isDbTypeSet;
			clone._isOracleRefCursor = _isOracleRefCursor;
            return clone;
        }

		internal override object ConvertValue(object value)
		{
			// can not convert null or DbNull to other types
			if (value == null || value == DBNull.Value) {
				return value;
			}

			// FIXME : some other way to do this?
			if (OleDbType == OleDbType.Binary) {
				return value;
			}
			// .NET throws an exception to the user.
			object convertedValue  = value;

			// note : if we set user parameter jdbc type inside prepare interbal, the db type is not set
			if (value is IConvertible && (_isDbTypeSet || IsJdbcTypeSet)) {
				OleDbType oleDbType = (_isDbTypeSet) ? OleDbType : OleDbConvert.JdbcTypeToOleDbType((int)JdbcType);
				Type to = OleDbConvert.OleDbTypeToValueType(oleDbType);
				if (!(value is DateTime && to == DbTypes.TypeOfTimespan)) //anyway will go by jdbc type
					convertedValue = Convert.ChangeType(value,to);
			}
			return convertedValue;
		}

		internal override void SetParameterName(ResultSet res)
		{
			ParameterName = res.getString("COLUMN_NAME");

			if (ParameterName.StartsWith("@")) {
				ParameterName = ParameterName.Remove(0,1);
			}
		}

		internal override void SetParameterDbType(ResultSet res)
		{
			int jdbcType = res.getInt("DATA_TYPE");			
			// FIXME : is that correct?
			if (jdbcType == Types.OTHER) {
				string typeName = res.getString("TYPE_NAME");
				if (String.Compare("BLOB",typeName,true) == 0) {
					jdbcType = Types.BLOB;
				}
				else if (String.Compare("CLOB",typeName,true) == 0) {
					jdbcType = Types.CLOB;
				}
				else if(String.Compare("FLOAT",typeName,true) == 0) {
					jdbcType = Types.FLOAT;
				}
				else if(String.Compare("NVARCHAR2",typeName,true) == 0) {
					jdbcType = Types.VARCHAR;
				}
				else if(String.Compare("NCHAR",typeName,true) == 0) {
					jdbcType = Types.CHAR;
				}
			}
			OleDbType = OleDbConvert.JdbcTypeToOleDbType(jdbcType);
			JdbcType = (DbTypes.JavaSqlTypes)jdbcType;
		}

		internal override void SetSpecialFeatures(ResultSet res)
		{
			IsOracleRefCursor = (res.getString("TYPE_NAME") == "REF CURSOR");
		}

		internal override DbTypes.JavaSqlTypes JdbcTypeFromProviderType()
		{
			return (DbTypes.JavaSqlTypes)OleDbConvert.OleDbTypeToJdbcType(OleDbType);
		}

		#endregion // Methods
    
    }
}