//
// System.Runtime.InteropServices.UCOMIStream.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;
using System.Runtime.InteropServices;

namespace System.Runtime.InteropServices
{
	[Guid ("0000000c-0000-0000-c000-000000000046")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface UCOMIStream
	{
		void Clone (out UCOMIStream ppstm);
		void Commit (int grfCommitFlags);
		void CopyTo (UCOMIStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten);
		void LockRegion (long libOffset, long cb, int dwLockType);
		void Read ([Out] byte[] pv, int cb, IntPtr pcbRead);
		void Revert ();
		void Seek (long dlibMove, int dwOrigin, IntPtr plibNewPosition);
		void SetSize (long libNewSize);
		void Stat (out STATSTG pstatstg, int grfStatFlag);
		void UnlockRegion (long libOffset, long cb, int dwLockType);
		void Write (byte[] pv, int cb, IntPtr pcbWritten);
	}
}
