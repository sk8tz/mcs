//
// System.Runtime.InteropServices.GCHandle Test Cases
//
// Authors:
// 	Paolo Molaro (lupus@ximian.com)
//
// (c) 2005 Novell, Inc. (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Runtime.InteropServices;

namespace MonoTests.System.Runtime.InteropServices
{
	[TestFixture]
	public class GCHandleTest : Assertion
	{
		static GCHandle handle;

		[Test]
		public void DefaultZeroValue ()
		{
			AssertEquals (false, handle.IsAllocated);
		}

		[Test]
		public void AllocNull ()
		{
			IntPtr ptr = (IntPtr)GCHandle.Alloc(null);
			GCHandle gch = (GCHandle)ptr;
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AddrOfPinnedObjectNormal ()
		{
			GCHandle handle = GCHandle.Alloc (new Object (), GCHandleType.Normal);
			try {
				IntPtr ptr = handle.AddrOfPinnedObject();
			}
			finally {
				handle.Free();
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AddrOfPinnedObjectWeak ()
		{
			GCHandle handle = GCHandle.Alloc (new Object (), GCHandleType.Weak);
			try {
				IntPtr ptr = handle.AddrOfPinnedObject();
			}
			finally {
				handle.Free();
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AddrOfPinnedObjectWeakTrackResurrection ()
		{
			GCHandle handle = GCHandle.Alloc (new Object (), GCHandleType.WeakTrackResurrection);
			try {
				IntPtr ptr = handle.AddrOfPinnedObject();
			}
			finally {
				handle.Free();
			}
		}

		[Test]
		public void AddrOfPinnedObjectNull ()
		{
			GCHandle handle = GCHandle.Alloc (null, GCHandleType.Pinned);
			try {
				IntPtr ptr = handle.AddrOfPinnedObject();
				AssertEquals (new IntPtr (0), ptr);
			}
			finally {
				handle.Free();
			}
		}
	}
}

