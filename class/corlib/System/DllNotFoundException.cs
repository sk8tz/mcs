//	
// System.DllNotFoundException.cs
//
// Author:
//	Duncan Mak (duncan@ximian.com)
//
// 2002 (C) Ximian, Inc. http://www.ximian.com
//

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace System
{
	[Serializable]
	public class DllNotFoundException : TypeLoadException
	{
		// Constructors
		public DllNotFoundException ()
			: base (Locale.GetText ("DLL not found."))
			{
			}
		
		public DllNotFoundException (string message)
			: base (message)
			{
			}
		
		public DllNotFoundException (SerializationInfo info,
					     StreamingContext context)
			: base (info, context)
			{
			}
		
		public DllNotFoundException (string message, Exception innerException)
			:base (message, innerException)
			{
			}
		
	}
}
