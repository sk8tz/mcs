//
// System.Boolean.cs
//
// Author:
//   Derek Holden (dholden@draper.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

//
// I guess this is the Boolean class. This was written word for word
// off the Library specification for System.Boolean in the ECMA
// TC39 TG2 and TG3 working documents.
//
// The XML style documentation isn't that elegant, but it seems to 
// be the standard way according to the poorly documented C# 
// Programmer's Reference section on XML Documentation.
//

using System.Globalization;

namespace System
{
	/// <summary>
	/// Represents the boolean values of logical true and false.
	/// </summary>
	[Serializable]
	public struct Boolean : IComparable, IConvertible
	{
		/// <value>
		/// The String representation of Boolean False
		/// </value>	
		public static readonly string FalseString = "False";

		/// <value>
		/// The String representation of Boolean True
		/// </value>	
		public static readonly string TrueString = "True";

		internal bool m_value;

		/// <summary>
		/// Compares the current Boolean instance against another object.
		/// </summary>
		/// <remarks>
		/// Throws an ArgumentException if <c>obj</c> isn't null or 
		/// a Boolean.
		/// </remarks>
		/// <param name="obj">
		/// The object to compare against
		/// </param>
		/// <returns>
		/// An int reflecting the sort order of this instance as 
		/// compared to <c>obj</c>
		/// -1 if this instance is false and <c>obj</c> is true
		///  0 if this instance is equal to <c>obj</c>
		///  1 if this instance is true and <c>obj</c> is false, 
		///    or <c>obj</c> is null
		/// </returns>
		public int CompareTo (object obj)
		{
			if (obj == null)
				return 1;

			if (!(obj is System.Boolean))
				throw new ArgumentException (Locale.GetText (
					"Object is not a Boolean."));

			// for case #3
			if (obj == null || (m_value == true && (bool) obj == false))
				return 1;

			// for case #2, else it's #1
			return (m_value == (bool) obj) ? 0 : -1;
		}

		/// <summary>
		/// Determines whether this instance and another object represent the
		/// same type and value.
		/// </summary>
		/// <param name="obj">
		/// The object to check against
		/// </param>
		/// <returns>
		/// true if this instnace and <c>obj</c> are same value, 
		/// otherwise false if it is not or null
		/// </returns>
		public override bool Equals (Object obj)
		{
			if (obj == null || !(obj is System.Boolean))
				return false;

			return ((bool) obj) == m_value;
		}

		/// <summary>
		/// Generates a hashcode for this object.
		/// </summary>
		/// <returns>
		/// An Int32 value holding the hash code
		/// </returns>
		public override int GetHashCode ()
		{
			// Guess there's not too many ways to hash a Boolean
			return m_value ? 1 : 0;
		}

		/// <summary>
		/// Returns a given string as a boolean value. The string must be 
		/// equivalent to either TrueString or FalseString, with leading and/or
		/// trailing spaces, and is parsed case-insensitively.
		/// </summary>
		/// <remarks>
		/// Throws an ArgumentNullException if <c>val</c> is null, or a 
		/// FormatException if <c>val</c> doesn't match <c>TrueString</c> 
		/// or <c>FalseString</c>
		/// </remarks>
		/// <param name="val">
		/// The string value to parse
		/// </param>
		/// <returns>
		/// true if <c>val</c> is equivalent to TrueString, 
		/// otherwise false
		/// </returns>
		public static bool Parse (string value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			value = value.Trim ();

			if (String.Compare (value, TrueString, true, CultureInfo.InvariantCulture) == 0)
				return true;

			if (String.Compare (value, FalseString, true, CultureInfo.InvariantCulture) == 0)
				return false;

			throw new FormatException (Locale.GetText (
				"Value is not equivalent to either TrueString or FalseString."));
		}

		/// <summary>
		/// Returns a string representation of this Boolean object.
		/// </summary>
		/// <returns>
		/// <c>FalseString</c> if the instance value is false, otherwise 
		/// <c>TrueString</c>
		/// </returns>
		public override string ToString ()
		{
			return m_value ? TrueString : FalseString;
		}

		// =========== IConvertible Methods =========== //
		public TypeCode GetTypeCode ()
		{
			return TypeCode.Boolean;
		}

		object IConvertible.ToType (Type conversionType, IFormatProvider provider)
		{
			return System.Convert.ToType (m_value, conversionType, provider);
		}

		bool IConvertible.ToBoolean (IFormatProvider provider)
		{
			return m_value;
		}

		byte IConvertible.ToByte (IFormatProvider provider)
		{
			return System.Convert.ToByte (m_value);
		}

		char IConvertible.ToChar (IFormatProvider provider)
		{
			throw new InvalidCastException ();
		}

		DateTime IConvertible.ToDateTime (IFormatProvider provider)
		{
			throw new InvalidCastException ();
		}

		decimal IConvertible.ToDecimal (IFormatProvider provider)
		{
			return System.Convert.ToDecimal (m_value);
		}

		double IConvertible.ToDouble (IFormatProvider provider)
		{
			return System.Convert.ToDouble (m_value);
		}

		short IConvertible.ToInt16 (IFormatProvider provider)
		{
			return System.Convert.ToInt16 (m_value);
		}

		int IConvertible.ToInt32 (IFormatProvider provider)
		{
			return System.Convert.ToInt32 (m_value);
		}

		long IConvertible.ToInt64 (IFormatProvider provider)
		{
			return System.Convert.ToInt64 (m_value);
		}

		sbyte IConvertible.ToSByte (IFormatProvider provider)
		{
			return System.Convert.ToSByte (m_value);
		}

		float IConvertible.ToSingle (IFormatProvider provider)
		{
			return System.Convert.ToSingle (m_value);
		}

		public string ToString (IFormatProvider provider)
		{
			return ToString ();
		}

		ushort IConvertible.ToUInt16 (IFormatProvider provider)
		{
			return System.Convert.ToUInt16 (m_value);
		}

		uint IConvertible.ToUInt32 (IFormatProvider provider)
		{
			return System.Convert.ToUInt32 (m_value);
		}

		ulong IConvertible.ToUInt64 (IFormatProvider provider)
		{
			return System.Convert.ToUInt64 (m_value);
		}
	}
}
