//
// System.Runtime.InteropServices.ComConversionLossAttribute.cs
//
// Name: Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;

namespace System.Runtime.InteropServices {

	[AttributeUsage (AttributeTargets.All, Inherited=false)]
	public sealed class ComConversionLossAttribute : Attribute
	{
		public ComConversionLossAttribute ()
		{
		}
	}
	
}
