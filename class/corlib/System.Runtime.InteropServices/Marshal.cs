// System.Runtime.InteropServices.Marshal
//
// Sean MacIsaac (macisaac@ximian.com)
//
// (C) 2001 Ximian, Inc.

using System.Runtime.CompilerServices;

namespace System.Runtime.InteropServices
{
	class Marshal
	{
		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		public static extern string PtrToStringAuto (IntPtr ptr);	

		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		public static extern IntPtr ReadIntPtr (IntPtr ptr);		
	}
}
