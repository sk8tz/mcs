//
// WindowsIdentityTest.cs - NUnit Test Cases for WindowsIdentity
//
// Author:
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Runtime.Serialization;
using System.Security.Principal;

namespace MonoTests.System.Security.Principal {

	[TestFixture]
	public class WindowsIdentityTest : Assertion {

		private bool IsPosix {
			get { return ((int) Environment.OSVersion.Platform == 128); }
		}

		// some features works only in Windows 2003 and later
		private bool IsWin2k3orLater {
			get {
				OperatingSystem os = Environment.OSVersion;
				if (os.Platform != PlatformID.Win32NT)
					return false;

				if (os.Version.Major > 5) {
					return false;
				}
				else if (os.Version.Major == 5) {
					return (os.Version.Minor > 1);
				}
				return false;
			}
		}

		[Test]
		public void ConstructorIntPtrZero () 
		{
			// should fail on Windows (invalid token)
			// should not fail on Posix (root uid)
			try {
				WindowsIdentity id = new WindowsIdentity (IntPtr.Zero);
				if (!IsPosix)
					Fail ("Expected ArgumentException on Windows platforms");
			}
			catch (ArgumentException) {
				if (IsPosix)
					throw;
			}
		}
#if !NET_1_0
		[Test]
		//[ExpectedException (typeof (ArgumentNullException))]
		[ExpectedException (typeof (NullReferenceException))]
		public void ConstructorW2KS1_Null () 
		{
			WindowsIdentity id = new WindowsIdentity (null);
		}

		[Test]
		public void ConstructorW2KS1 () 
		{
			WindowsIdentity wi = WindowsIdentity.GetCurrent ();
			// should fail with ArgumentException unless
			// - running Windows 2003 or later
			// - running Posix
			try {
				WindowsIdentity id = new WindowsIdentity (wi.Name);
				if (!IsWin2k3orLater && !IsPosix)
					Fail ("Expected ArgumentException but got none");
			}
			catch (ArgumentException) {
				if (IsWin2k3orLater || IsPosix)
					throw;
			}
		}

		[Test]
		//[ExpectedException (typeof (ArgumentNullException))]
		[ExpectedException (typeof (NullReferenceException))]
		public void ConstructorW2KS2_UserNull () 
		{
			WindowsIdentity id = new WindowsIdentity (null, "NTLM");
		}

		[Test]
		public void ConstructorW2KS2_TypeNull() 
		{
			WindowsIdentity wi = WindowsIdentity.GetCurrent ();
			// should fail with ArgumentException unless
			// - running Windows 2003 or later
			// - running Posix
			try {
				WindowsIdentity id = new WindowsIdentity (wi.Name, null);
				if (!IsWin2k3orLater && !IsPosix)
					Fail ("Expected ArgumentException but got none");
			}
			catch (ArgumentException) {
				if (IsWin2k3orLater || IsPosix)
					throw;
			}
		}

		[Test]
		public void ConstructorW2KS2 () 
		{
			WindowsIdentity wi = WindowsIdentity.GetCurrent ();
			// should fail with ArgumentException unless
			// - running Windows 2003 or later
			// - running Posix
			try {
				WindowsIdentity id = new WindowsIdentity (wi.Name, wi.AuthenticationType);
				if (!IsWin2k3orLater && !IsPosix)
					Fail ("Expected ArgumentException but got none");
			}
			catch (ArgumentException) {
				if (IsWin2k3orLater || IsPosix)
					throw;
			}
		}
#endif
		[Test]
		public void Anonymous () 
		{
			WindowsIdentity id = WindowsIdentity.GetAnonymous ();
			AssertEquals ("AuthenticationType", String.Empty, id.AuthenticationType);
			Assert ("IsAnonymous", id.IsAnonymous);
			Assert ("IsAuthenticated", !id.IsAuthenticated);
			Assert ("IsGuest", !id.IsGuest);
			Assert ("IsSystem", !id.IsSystem);
			if (IsPosix) {
				Assert ("Token", (IntPtr.Zero != id.Token));
				AssertNotNull ("Name", id.Name);
			}
			else {
				AssertEquals ("Token", IntPtr.Zero, id.Token);
				AssertEquals ("Name", String.Empty, id.Name);
			}
		}

		[Test]
		public void Current () 
		{
			WindowsIdentity id = WindowsIdentity.GetCurrent ();
			AssertNotNull ("AuthenticationType", id.AuthenticationType);
			Assert ("IsAnonymous", !id.IsAnonymous);
			Assert ("IsAuthenticated", id.IsAuthenticated);
			Assert ("IsGuest", !id.IsGuest);
			// root is 0 - so IntPtr.Zero is valid on Linux (but not on Windows)
			Assert ("IsSystem", (!id.IsSystem || (id.Token == IntPtr.Zero)));
			if (!IsPosix) {
				Assert ("Token", (id.Token != IntPtr.Zero));
			}
			AssertNotNull ("Name", id.Name);
		}

		[Test]
		public void Interfaces () 
		{
			WindowsIdentity id = WindowsIdentity.GetAnonymous ();

			IIdentity i = (id as IIdentity);
			AssertNotNull ("IIdentity", i);

			IDeserializationCallback dc = (id as IDeserializationCallback);
			AssertNotNull ("IDeserializationCallback", dc);
#if NET_1_1
			ISerializable s = (id as ISerializable);
			AssertNotNull ("ISerializable", s);
#endif
		}
	}
}
