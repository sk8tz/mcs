//
// System.Data.SqlNullValueException.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Data.SqlTypes {

	[Serializable]
	public sealed class SqlNullValueException : SqlTypeException
	{
		public SqlNullValueException ()
			: base (Locale.GetText ("Data is Null. This method or property cannot be called on Null values."))
		{
		}

		public SqlNullValueException (string message)
			: base (message)
		{
		}

		public SqlNullValueException (string message, Exception inner)
			: base (message, inner)
		{
		}
	}
}
