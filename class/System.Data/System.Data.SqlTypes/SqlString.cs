//
// System.Data.SqlTypes.SqlString
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc. 2002
// (C) Copyright 2002 Tim Coleman
//

using System;
using System.Globalization;

namespace System.Data.SqlTypes
{
	/// <summary>
	/// A variable-length stream of characters 
	/// to be stored in or retrieved from the database
	/// </summary>
	public struct SqlString : INullable, IComparable 
	{

		#region Fields

		string value;

		public static readonly int BinarySort;
		public static readonly int IgnoreCase;
		public static readonly int IgnoreKanaType;
		public static readonly int IgnoreNonSpace;
		public static readonly int IgnoreWidth;
		public static readonly SqlString Null;

		#endregion // Fields

		#region Constructors

		// init with a string data
		public SqlString (string data) 
		{
			this.value = data;
		}

		// init with a string data and locale id values.
		[MonoTODO]
		public SqlString (string data, int lcid) 
		{
			throw new NotImplementedException ();
		}

		// init with locale id, compare options, 
		// and an array of bytes data
		[MonoTODO]
		public SqlString (int lcid, SqlCompareOptions compareOptions, byte[] data) 
		{
			throw new NotImplementedException ();
		}

		// init with string data, locale id, and compare options
		[MonoTODO]
		public SqlString (string data, int lcid, SqlCompareOptions compareOptions) 
		{
			throw new NotImplementedException ();
		}

		// init with locale id, compare options, array of bytes data,
		// and whether unicode is encoded or not
		[MonoTODO]
		public SqlString (int lcid, SqlCompareOptions compareOptions, byte[] data, bool fUnicode) 
		{
			throw new NotImplementedException ();
		}

		// init with locale id, compare options, array of bytes data,
		// starting index in the byte array, 
		// and number of bytes to copy
		[MonoTODO]
		public SqlString (int lcid, SqlCompareOptions compareOptions, byte[] data, int index, int count) 
		{
			throw new NotImplementedException ();
		}

		// init with locale id, compare options, array of bytes data,
		// starting index in the byte array, number of byte to copy,
		// and whether unicode is encoded or not
		[MonoTODO]
		public SqlString (int lcid, SqlCompareOptions compareOptions, byte[] data, int index, int count, bool fUnicode) 
		{
			throw new NotImplementedException ();
		}

		#endregion // Constructors


		#region Public Properties

		public CompareInfo CompareInfo {
			[MonoTODO]
			get { throw new NotImplementedException ();
			}
		}

		public CultureInfo CultureInfo {
			[MonoTODO]
			get { throw new NotImplementedException ();
			}
		}

		public bool IsNull {
			get { return (bool) (this == SqlString.Null); }
		}

		// geographics location and language (locale id)
		public int LCID {
			[MonoTODO]
			get { throw new NotImplementedException ();
			}
		}
	
		public SqlCompareOptions SqlCompareOptions {
			[MonoTODO]
			get { throw new NotImplementedException ();
			}
		}

                public string Value {
                        get {
                                if (this.IsNull)
                                        throw new SqlNullValueException ("The property contains Null.");
                                else
                                        return value;
                        }
                }

		#endregion // Public Properties

		#region Public Methods

		[MonoTODO]
		public SqlString Clone() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static CompareOptions CompareOptionsFromSqlCompareOptions (SqlCompareOptions compareOptions) 
		{
			throw new NotImplementedException ();
		}

		// **********************************
		// Comparison Methods
		// **********************************

		public int CompareTo(object value)
		{
			if (value == null)
				return 1;
			else if (!(value is SqlString))
				throw new ArgumentException (Locale.GetText ("Value is not a System.Data.SqlTypes.SqlString"));
			else if (value.IsNull)
				return 1;
			else
				return value.CompareTo (value.Value);
		}

		public static SqlString Concat(SqlString x, SqlString y) 
		{
			return (x + y);
		}

		public override bool Equals(object value) 
		{
			if (!(value is SqlString))
				return false;
			else
				return (bool) (this == value);
		}

		public static SqlBoolean Equals(SqlString x, SqlString y) 
		{
			return (x == y);
		}

		[MonoTODO]
		public override int GetHashCode() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public byte[] GetNonUnicodeBytes() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public byte[] GetUnicodeBytes() 
		{
			throw new NotImplementedException ();
		}

		public static SqlBoolean GreaterThan(SqlString x, SqlString y) 
		{
			return (x > y);
		}

		public static SqlBoolean GreaterThanOrEqual(SqlString x, SqlString y) 
		{
			return (x >= y);
		}

		public static SqlBoolean LessThan(SqlString x, SqlString y) 
		{
			return (x < y);
		}

		public static SqlBoolean LessThanOrEqual(SqlString x, SqlString y) 
		{
			return (x <= y);
		}

		public static SqlBoolean NotEquals(SqlString x,	SqlString y) 
		{
			return (x != y);
		}

		// ****************************************
		// Type Conversions From SqlString To ...
		// ****************************************

		public SqlBoolean ToSqlBoolean() 
		{
			return ((SqlBoolean)this);
		}

		public SqlByte ToSqlByte() 
		{
			return ((SqlByte)this);
		}

		public SqlDateTime ToSqlDateTime() 
		{
			return ((SqlDateTime)this);
		}

		public SqlDecimal ToSqlDecimal() 
		{
			return ((SqlDecimal)this);
		}

		public SqlDouble ToSqlDouble() 
		{
			return ((SqlDouble)this);
		}

		public SqlGuid ToSqlGuid() 
		{
			return ((SqlGuid)this);
		}

		public SqlInt16 ToSqlInt16() 
		{
			return ((SqlInt16)this);
		}

		public SqlInt32 ToSqlInt32() 
		{
			return ((SqlInt32)this);
		}

		public SqlInt64 ToSqlInt64() 
		{
			return ((SqlInt64)this);
		}

		public SqlMoney ToSqlMoney() 
		{
			return ((SqlMoney)this);
		}

		public SqlSingle ToSqlSingle() 
		{
			return ((SqlSingle)this);
		}

		public override string ToString() 
		{
			return ((string)this);
		}

		// ***********************************
		// Operators
		// ***********************************

		// Concatenates
		public static SqlString operator + (SqlString x, SqlString y) 
		{
			return new SqlString (x.Value + y.Value);
		}

		// Equality
		public static SqlBoolean operator == (SqlString x, SqlString y) 
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value == y.Value);
		}

		// Greater Than
		public static SqlBoolean operator > (SqlString x, SqlString y) 
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			else
				throw new NotImplementedException ();
		}

		// Greater Than Or Equal
		public static SqlBoolean operator >= (SqlString x, SqlString y) 
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			else
				throw new NotImplementedException ();
		}

		public static SqlBoolean operator != (SqlString x, SqlString y) 
		{ 
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value != y.Value);
		}

		// Less Than
		public static SqlBoolean operator < (SqlString x, SqlString y) 
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			else
				throw new NotImplementedException ();
		}

		// Less Than Or Equal
		public static SqlBoolean operator <= (SqlString x, SqlString y) 
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;
			else
				throw new NotImplementedException ();
		}

		// **************************************
		// Type Conversions
		// **************************************

		public static explicit operator SqlString (SqlBoolean x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlString (x.ByteValue.ToString ());
		}

		public static explicit operator SqlString (SqlByte x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlString (x.Value.ToString ());
		}

		public static explicit operator SqlString (SqlDateTime x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlString (x.Value.ToString ());
		}

		public static explicit operator SqlString (SqlDecimal x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlString (x.Value.ToString ());
		}

		public static explicit operator SqlString (SqlDouble x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlString (x.Value.ToString ());
		}

		public static explicit operator SqlString (SqlGuid x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlString (x.Value.ToString ());
		}

		public static explicit operator SqlString (SqlInt16 x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlString (x.Value.ToString ());
		}

		public static explicit operator SqlString (SqlInt32 x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlString (x.Value.ToString ());
		}

		public static explicit operator SqlString (SqlInt64 x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlString (x.Value.ToString ());
		}

		public static explicit operator SqlString (SqlMoney x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlString (x.Value.ToString ());
		}

		public static explicit operator SqlString (SqlSingle x) 
		{
			if (x.IsNull)
				return Null;
			else
				return new SqlString (x.Value.ToString ());
		}

		public static explicit operator string (SqlString x) 
		{
			return x.Value;
		}

		public static implicit operator SqlString (string x) 
		{
			return new SqlString (x);
		}

		#endregion // Public Methods
	}
}
