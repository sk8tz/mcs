//
// System.Runtime.InteropServices.UCOMIEnumConnectionPoints.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;

namespace System.Runtime.InteropServices
{
	[Guid ("b196b285-bab4-101a-b69c-00aa00341d07")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface UCOMIEnumConnectionPoints
	{
		void Clone (ref UCOMIEnumConnectionPoints ppenum);
		int Next (int celt,out UCOMIConnectionPoint[] rgelt, ref int pceltFetched);
		int Reset ();
		int Skip (int celt);
	}
}
