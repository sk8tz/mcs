//
// System.Data.SqlNullValueException.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.

using System;
using System.Runtime.Serialization;

namespace System.Data.SqlTypes {

	[Serializable]
	public class SqlNullValueException : SqlTypeException
	{
		public SqlNullValueException ()
			: base (Locale.GetText ("The value property is null"))
		{
		}

		public SqlNullValueException (string message)
			: base (message)
		{
		}
	}
}
