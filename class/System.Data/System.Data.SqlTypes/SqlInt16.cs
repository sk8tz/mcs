//
// System.Data.SqlTypes.SqlInt16
//
// Author:
//   Tim Coleman <tim@timcoleman.com>
//
// (C) Copyright 2002 Tim Coleman
//

using System;

namespace System.Data.SqlTypes
{
	public struct SqlInt16 : INullable, IComparable
	{
		#region Fields
		private short value;

		public static readonly SqlInt16 MaxValue = new SqlInt16 (32767);
		public static readonly SqlInt16 MinValue = new SqlInt16 (-32768);
		[MonoTODO]
		public static readonly SqlInt16 Null;
		public static readonly SqlInt16 Zero = new SqlInt16 (0);

		#endregion

		#region Constructors

		public SqlInt16 (short value) 
		{
			this.value = value;
		}

		#endregion

		#region Properties

		public bool IsNull { 
			get { return (bool) (this == SqlInt16.Null); }
		}

		public short Value { 
			get { return value; }
		}

		#endregion

		#region Methods

		public static SqlInt16 Add (SqlInt16 x, SqlInt16 y)
		{
			return (x + y);
		}

		public static SqlInt16 BitwiseAnd (SqlInt16 x, SqlInt16 y)
		{
			return (x & y);
		}

		public static SqlInt16 BitwiseOr (SqlInt16 x, SqlInt16 y)
		{
			return (x | y);
		}

		[MonoTODO]
		public int CompareTo (object value)
		{
			throw new NotImplementedException ();
		}

		public static SqlInt16 Divide (SqlInt16 x, SqlInt16 y)
		{
			return (x / y);
		}

		[MonoTODO]
		public override bool Equals (object value)
		{
			throw new NotImplementedException ();
		}

		public static SqlBoolean Equals (SqlInt16 x, SqlInt16 y)
		{
			return (x == y);
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			return (int)value;
		}

		public static SqlBoolean GreaterThan (SqlInt16 x, SqlInt16 y)
		{
			return (x > y);
		}

		public static SqlBoolean GreaterThanOrEqual (SqlInt16 x, SqlInt16 y)
		{
			return (x >= y);
		}

		public static SqlBoolean LessThan (SqlInt16 x, SqlInt16 y)
		{
			return (x < y);
		}

		public static SqlBoolean LessThanOrEqual (SqlInt16 x, SqlInt16 y)
		{
			return (x <= y);
		}

		public static SqlInt16 Mod (SqlInt16 x, SqlInt16 y)
		{
			return (x % y);
		}

		public static SqlInt16 Multiply (SqlInt16 x, SqlInt16 y)
		{
			return (x * y);
		}

		public static SqlBoolean NotEquals (SqlInt16 x, SqlInt16 y)
		{
			return (x != y);
		}

		public static SqlInt16 OnesComplement (SqlInt16 x)
		{
			return ~x;
		}

		[MonoTODO]
		public static SqlInt16 Parse (string s)
		{
			throw new NotImplementedException ();
		}

		public static SqlInt16 Subtract (SqlInt16 x, SqlInt16 y)
		{
			return (x - y);
		}

		public SqlBoolean ToSqlBoolean ()
		{
			return ((SqlBoolean)this);
		}
		
		public SqlByte ToSqlByte ()
		{
			return ((SqlByte)this);
		}

		public SqlDecimal ToSqlDecimal ()
		{
			return ((SqlDecimal)this);
		}

		public SqlDouble ToSqlDouble ()
		{
			return ((SqlDouble)this);
		}

		public SqlInt32 ToSqlInt32 ()
		{
			return ((SqlInt32)this);
		}

		public SqlInt64 ToSqlInt64 ()
		{
			return ((SqlInt64)this);
		}

		public SqlMoney ToSqlMoney ()
		{
			return ((SqlMoney)this);
		}

		public SqlSingle ToSqlSingle ()
		{
			return ((SqlSingle)this);
		}

		[MonoTODO]
		public static SqlString ToSqlString ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override string ToString ()
		{
			throw new NotImplementedException ();
		}

		public static SqlInt16 Xor (SqlInt16 x, SqlInt16 y)
		{
			return (x ^ y);
		}

		public static SqlInt16 operator + (SqlInt16 x, SqlInt16 y)
		{
			return new SqlInt16 ((short) (x.Value + y.Value));
		}

		public static SqlInt16 operator & (SqlInt16 x, SqlInt16 y)
		{
			return new SqlInt16 ((short) (x.value & y.Value));
		}

		public static SqlInt16 operator | (SqlInt16 x, SqlInt16 y)
		{
			return new SqlInt16 ((short) (x | y));
		}

		public static SqlInt16 operator / (SqlInt16 x, SqlInt16 y)
		{
			return new SqlInt16 ((short) (x.Value / y.Value));
		}

		public static SqlBoolean operator == (SqlInt16 x, SqlInt16 y)
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value == y.Value);
		}

		public static SqlInt16 operator ^ (SqlInt16 x, SqlInt16 y)
		{
			return new SqlInt16 ((short) (x.Value ^ y.Value));
		}

		public static SqlBoolean operator > (SqlInt16 x, SqlInt16 y)
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value > y.Value);
		}

		public static SqlBoolean operator >= (SqlInt16 x, SqlInt16 y)
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value >= y.Value);
		}

		public static SqlBoolean operator != (SqlInt16 x, SqlInt16 y)
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (!(x.Value == y.Value));
		}

		public static SqlBoolean operator < (SqlInt16 x, SqlInt16 y)
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value < y.Value);
		}

		public static SqlBoolean operator <= (SqlInt16 x, SqlInt16 y)
		{
			if (x.IsNull || y.IsNull) return SqlBoolean.Null;
			return new SqlBoolean (x.Value <= y.Value);
		}

		public static SqlInt16 operator % (SqlInt16 x, SqlInt16 y)
		{
			return new SqlInt16 ((short) (x.Value % y.Value));
		}

		public static SqlInt16 operator * (SqlInt16 x, SqlInt16 y)
		{
			return new SqlInt16 ((short) (x.Value * y.Value));
		}

		public static SqlInt16 operator ~ (SqlInt16 x)
		{
			return new SqlInt16 ((short) (~x.Value));
		}

		public static SqlInt16 operator - (SqlInt16 x, SqlInt16 y)
		{
			return new SqlInt16 ((short) (x.Value - y.Value));
		}

		public static SqlInt16 operator - (SqlInt16 n)
		{
			return new SqlInt16 ((short) (-n.Value));
		}

		public static explicit operator SqlInt16 (SqlBoolean x)
		{
			return new SqlInt16 ((short)x.ByteValue);
		}

		public static explicit operator SqlInt16 (SqlDecimal x)
		{
			return new SqlInt16 ((short)x.Value);
		}

		public static explicit operator SqlInt16 (SqlDouble x)
		{
			return new SqlInt16 ((short)x.Value);
		}

		public static explicit operator short (SqlInt16 x)
		{
			return x.Value; 
		}

		public static explicit operator SqlInt16 (SqlInt32 x)
		{
			return new SqlInt16 ((short)x.Value);
		}

		public static explicit operator SqlInt16 (SqlInt64 x)
		{
			return new SqlInt16 ((short)x.Value);
		}

		public static explicit operator SqlInt16 (SqlMoney x)
		{
			return new SqlInt16 ((short)x.Value);
		}

		public static explicit operator SqlInt16 (SqlSingle x)
		{
			return new SqlInt16 ((short)x.Value);
		}

		[MonoTODO]
		public static explicit operator SqlInt16 (SqlString x)
		{
			throw new NotImplementedException ();
		}

		public static explicit operator SqlInt16 (short x)
		{
			return new SqlInt16 (x);
		}

		public static explicit operator SqlInt16 (SqlByte x)
		{
			return new SqlInt16 ((short)x.Value);
		}

		#endregion
	}
}
			
