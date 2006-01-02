//
// ActivatorTest.cs - NUnit Test Cases for System.Activator
//
// Authors:
//	Nick Drochak <ndrochak@gol.com>
//	Gert Driesen <drieseng@users.sourceforge.net>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Security;
using System.Security.Permissions;

using NUnit.Framework;

// The class in this namespace is used by the main test class
namespace MonoTests.System.ActivatorTestInternal {

	// We need a COM class to test the Activator class
	[ComVisible (true)]
	public class COMTest : MarshalByRefObject {

		private int id;
		public bool constructorFlag = false;

		public COMTest ()
		{
			id = 0;
		}

		public COMTest (int id)
		{
			this.id = id;
		}

		// This property is visible
		[ComVisible (true)]
		public int Id {
			get { return id; }
			set { id = value; }
		}
	}

	[ComVisible (false)]
	public class NonCOMTest : COMTest {
	}
}

namespace MonoTests.System {

	using MonoTests.System.ActivatorTestInternal;

	[TestFixture]
	public class ActivatorTest {

		private string corlibLocation = typeof (string).Assembly.Location;
		private string testLocation = typeof (ActivatorTest).Assembly.Location;

		[Test]
		public void CreateInstance_Type()
		{
			COMTest objCOMTest = (COMTest) Activator.CreateInstance (typeof (COMTest));
			Assert.AreEqual ("MonoTests.System.ActivatorTestInternal.COMTest", (objCOMTest.GetType ()).ToString (), "#A02");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CreateInstance_TypeNull ()
		{
			Activator.CreateInstance ((Type)null);
		}

		[Test]
		public void CreateInstance_StringString ()
		{
			ObjectHandle objHandle = Activator.CreateInstance (null, "MonoTests.System.ActivatorTestInternal.COMTest");
			COMTest objCOMTest = (COMTest)objHandle.Unwrap ();
			objCOMTest.Id = 2;
			Assert.AreEqual (2, objCOMTest.Id, "#A03");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CreateInstance_StringNull ()
		{
			Activator.CreateInstance ((string)null, null);
		}

		[Test]
		[ExpectedException (typeof (TypeLoadException))]
		public void CreateInstance_StringTypeNameDoesNotExists ()
		{
			Activator.CreateInstance ((string)null, "MonoTests.System.ActivatorTestInternal.DoesntExistsCOMTest");
		}

		[Test]
		public void CreateInstance_TypeBool ()
		{
			COMTest objCOMTest = (COMTest)Activator.CreateInstance (typeof (COMTest), false);
			Assert.AreEqual ("MonoTests.System.ActivatorTestInternal.COMTest", objCOMTest.GetType ().ToString (), "#A04");
		}

		[Test]
		public void CreateInstance_TypeObjectArray ()
		{
			object[] objArray = new object[1] { 7 };
			COMTest objCOMTest = (COMTest)Activator.CreateInstance (typeof (COMTest), objArray);
			Assert.AreEqual (7, objCOMTest.Id, "#A05");
		}

		[Test]
		[ExpectedException (typeof (MissingMethodException))]
		public void CreateInstance_TypeBuilder ()
		{
			Type tb = typeof (TypeBuilder); // no public ctor - but why is it documented as NotSupportedException ?
			ConstructorInfo[] ctors = tb.GetConstructors (BindingFlags.Instance | BindingFlags.NonPublic);
			Activator.CreateInstance (tb, new object [ctors [0].GetParameters ().Length]);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CreateInstance_TypedReference ()
		{
			Activator.CreateInstance (typeof (TypedReference), null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CreateInstance_ArgIterator ()
		{
			Activator.CreateInstance (typeof (ArgIterator), null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CreateInstance_Void ()
		{
			Activator.CreateInstance (typeof (void), null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CreateInstance_RuntimeArgumentHandle ()
		{
			Activator.CreateInstance (typeof (RuntimeArgumentHandle), null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void CreateInstance_NotMarshalByReferenceWithActivationAttributes ()
		{
			Activator.CreateInstance (typeof (object), null, new object[1] { null });
		}

		// TODO: Implemente the test methods for all the overriden functions using activationAttribute

		[Test]
#if NET_2_0
		[ExpectedException(typeof(MissingMethodException))]
#else
		[ExpectedException(typeof(MemberAccessException))]
#endif
		public void CreateInstanceAbstract1 () 
		{
			Activator.CreateInstance (typeof (Type));
		}

		[Test]
#if NET_2_0
		[ExpectedException(typeof(MissingMethodException))]
#else
		[ExpectedException(typeof(MemberAccessException))]
#endif
		public void CreateInstanceAbstract2 () 
		{
			Activator.CreateInstance (typeof (Type), true);
		}

		[Test]
		[ExpectedException(typeof(MissingMethodException))]
		public void CreateInstanceAbstract3 () 
		{
			Activator.CreateInstance (typeof (Type), null, null);
		}

		[Test]
		[ExpectedException(typeof(MissingMethodException))]
		public void CreateInstanceAbstract4() 
		{
			Activator.CreateInstance (typeof (Type), BindingFlags.CreateInstance | (BindingFlags.Public | BindingFlags.Instance), null, null, CultureInfo.InvariantCulture, null);
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (MissingMethodException))]
#else
		[ExpectedException (typeof (MemberAccessException))]
#endif
		public void CreateInstanceAbstract5 () 
		{
			Activator.CreateInstance (typeof (Type), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, null, CultureInfo.InvariantCulture, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetObject_TypeNull ()
		{
			Activator.GetObject (null, "tcp://localhost:1234/COMTestUri");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetObject_UrlNull ()
		{
			Activator.GetObject (typeof (COMTest), null);
		}

/* This test is now executed in System.Runtime.Remoting unit tests 
		[Test]
		public void GetObject ()
		{
			// This will provide a COMTest object on  tcp://localhost:1234/COMTestUri
			COMTest objCOMTest = new COMTest (8);
			TcpChannel chnServer = new TcpChannel (1234);
			ChannelServices.RegisterChannel (chnServer);
			RemotingServices.SetObjectUriForMarshal (objCOMTest, "COMTestUri");
			RemotingServices.Marshal (objCOMTest);

			// This will get the remoting object
			object objRem = Activator.GetObject (typeof (COMTest), "tcp://localhost:1234/COMTestUri");
			Assert.IsNotNull (objRem, "#A07");
			COMTest remCOMTest = (COMTest) objRem;
			Assert.AreEqual (8, remCOMTest.Id, "#A08");

			ChannelServices.UnregisterChannel(chnServer);
		}
*/
		// TODO: Implemente the test methods for all the overriden function using activationAttribute

		[Test]
		public void CreateInstanceFrom ()
		{
			ObjectHandle objHandle = Activator.CreateInstanceFrom (testLocation, "MonoTests.System.ActivatorTestInternal.COMTest");
			Assert.IsNotNull (objHandle, "#A09");
			objHandle.Unwrap ();
			// TODO: Implement the test methods for all the overriden function using activationAttribute
		}

		// note: this only ensure that the ECMA key support unification (more test required, outside corlib, for other keys, like MS final).
		private const string CorlibPermissionPattern = "System.Security.Permissions.FileDialogPermission, mscorlib, Version={0}, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		private const string SystemPermissionPattern = "System.Net.DnsPermission, System, Version={0}, Culture=neutral, PublicKeyToken=b77a5c561934e089";
		private const string fx10version = "1.0.3300.0";
		private const string fx11version = "1.0.5000.0";
		private const string fx20version = "2.0.0.0";

		private static object[] psNone = new object [1] { PermissionState.None };

		private void Unification (string fullname)
		{
			Type t = Type.GetType (fullname);
			IPermission p = (IPermission)Activator.CreateInstance (t, psNone);
			string currentVersion = typeof (string).Assembly.GetName ().Version.ToString ();
			Assert.IsTrue ((p.ToString ().IndexOf (currentVersion) > 0), currentVersion);
		}

		[Test]
		public void Unification_FromFx10 ()
		{
			Unification (String.Format (CorlibPermissionPattern, fx10version));
			Unification (String.Format (SystemPermissionPattern, fx10version));
		}

		[Test]
		public void Unification_FromFx11 ()
		{
			Unification (String.Format (CorlibPermissionPattern, fx11version));
			Unification (String.Format (SystemPermissionPattern, fx11version));
		}

		[Test]
		public void Unification_FromFx20 ()
		{
			Unification (String.Format (CorlibPermissionPattern, fx20version));
			Unification (String.Format (SystemPermissionPattern, fx20version));
		}

		[Test]
		public void Unification_FromFx99_Corlib ()
		{
			Unification (String.Format (CorlibPermissionPattern, "9.99.999.9999"));
#if ONLY_1_1
			Unification (String.Format (SystemPermissionPattern, "9.99.999.9999"));
#endif
		}

#if NET_2_0
		[Test]
		[Category ("NotWorking")]
		public void Unification_FromFx99_System ()
		{
			Assert.IsNull (Type.GetType (String.Format (SystemPermissionPattern, "9.99.999.9999")));
		}
#endif
	}
}
