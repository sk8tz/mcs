﻿//
// StrongNameIdentityPermissionTest.cs -
//	NUnit Test Cases for StrongNameIdentityPermission
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using NUnit.Framework;
using System;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Security.Permissions {

	[TestFixture]
	public class StrongNameIdentityPermissionTest {

		static byte[] ecma = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

		[Test]
		public void PermissionStateNone ()
		{
			StrongNameIdentityPermission snip = new StrongNameIdentityPermission (PermissionState.None);
			Assert.AreEqual (String.Empty, snip.Name, "Name");
			Assert.IsNull (snip.PublicKey, "PublicKey");
			Assert.AreEqual ("0.0", snip.Version.ToString (), "Version");

			SecurityElement se = snip.ToXml ();
#if NET_2_0
			Assert.IsNull (se.Attribute ("Name"), "Xml-Name");
			Assert.IsNull (se.Attribute ("AssemblyVersion"), "Xml-AssemblyVersion");
#else
			Assert.AreEqual (String.Empty, se.Attribute ("Name"), "Xml-Name");
			Assert.AreEqual ("0.0", se.Attribute ("AssemblyVersion"), "Xml-AssemblyVersion");
#endif
			Assert.IsNull (se.Attribute ("PublicKeyBlob"), "Xml-PublicKeyBlob");

			// because Name == String.Empty, which is illegal using the other constructor
			StrongNameIdentityPermission copy = (StrongNameIdentityPermission) snip.Copy ();
			Assert.AreEqual (String.Empty, copy.Name, "Copy-Name");
#if NET_2_0
			// Strangely once copied the Name becomes equals to String.Empty in 2.0 [FDBK19351]
			Assert.IsNull (se.Attribute ("AssemblyVersion"), "Copy-Version");
#else
			Assert.AreEqual ("0.0", copy.Version.ToString (), "Copy-Version");
#endif
			Assert.IsNull (copy.PublicKey, "Copy-PublicKey");
		}
#if NET_2_0
		[Test]
		public void PermissionStateUnrestricted ()
		{
			// In 2.0 Unrestricted are permitted for identity permissions
			StrongNameIdentityPermission snip = new StrongNameIdentityPermission (PermissionState.Unrestricted);
			Assert.AreEqual (String.Empty, snip.Name, "Name");
			Assert.IsNull (snip.PublicKey, "PublicKey");
			Assert.AreEqual ("0.0", snip.Version.ToString (), "Version");
			SecurityElement se = snip.ToXml ();
			Assert.IsNull (se.Attribute ("Name"), "Xml-Name");
			Assert.IsNull (se.Attribute ("PublicKeyBlob"), "Xml-PublicKeyBlob");
			Assert.IsNull (se.Attribute ("AssemblyVersion"), "Xml-AssemblyVersion");
			StrongNameIdentityPermission copy = (StrongNameIdentityPermission)snip.Copy ();
			// Strangely once copied the Name becomes equals to String.Empty in 2.0 [FDBK19351]
			Assert.AreEqual (String.Empty, copy.Name, "Copy-Name");
			Assert.IsNull (copy.PublicKey, "Copy-PublicKey");
			Assert.IsNull (se.Attribute ("AssemblyVersion"), "Copy-Version");
			// and they aren't equals to None
			Assert.IsFalse (snip.Equals (new StrongNameIdentityPermission (PermissionState.None)));
		}
#else
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void PermissionStateUnrestricted ()
		{
			StrongNameIdentityPermission snip = new StrongNameIdentityPermission (PermissionState.Unrestricted);
		}
#endif
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void PermissionStateInvalid ()
		{
			StrongNameIdentityPermission snip = new StrongNameIdentityPermission ((PermissionState)2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void StrongNameIdentityPermission_BlobNull ()
		{
			StrongNameIdentityPermission snip = new StrongNameIdentityPermission (null, "mono", new Version (0,0));
		}

		[Test]
		public void StrongNameIdentityPermission_NameNull ()
		{
			StrongNamePublicKeyBlob blob = new StrongNamePublicKeyBlob (ecma);
			StrongNameIdentityPermission snip = new StrongNameIdentityPermission (blob, null, new Version (1, 2));
			Assert.IsNull (snip.Name, "Name");
			Assert.AreEqual ("00000000000000000400000000000000", snip.PublicKey.ToString (), "PublicKey");
			Assert.AreEqual ("1.2", snip.Version.ToString (), "Version");

			SecurityElement se = snip.ToXml ();
			Assert.IsNull (se.Attribute ("Name"), "Xml-Name");
			Assert.AreEqual ("00000000000000000400000000000000", se.Attribute ("PublicKeyBlob"), "Xml-PublicKeyBlob");
			Assert.AreEqual ("1.2", se.Attribute ("AssemblyVersion"), "Xml-AssemblyVersion");

			StrongNameIdentityPermission copy = (StrongNameIdentityPermission)snip.Copy ();
			Assert.IsNull (se.Attribute ("Name"), "Copy-Name");
			Assert.AreEqual ("00000000000000000400000000000000", se.Attribute ("PublicKeyBlob"), "Copy-PublicKeyBlob");
			Assert.AreEqual ("1.2", se.Attribute ("AssemblyVersion"), "Copy-AssemblyVersion");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void StrongNameIdentityPermission_NameEmpty ()
		{
			StrongNamePublicKeyBlob blob = new StrongNamePublicKeyBlob (ecma);
			StrongNameIdentityPermission snip = new StrongNameIdentityPermission (blob, String.Empty, new Version (1, 2));
#if !NET_2_0
			// TODO
#endif
		}

		[Test]
		public void StrongNameIdentityPermission_VersionNull ()
		{
			StrongNamePublicKeyBlob blob = new StrongNamePublicKeyBlob (ecma);
			StrongNameIdentityPermission snip = new StrongNameIdentityPermission (blob, "mono", null);
			Assert.AreEqual ("mono", snip.Name, "Name");
			Assert.AreEqual ("00000000000000000400000000000000", snip.PublicKey.ToString (), "PublicKey");
			Assert.IsNull (snip.Version, "Version");

			SecurityElement se = snip.ToXml ();
			Assert.AreEqual ("mono", se.Attribute ("Name"), "Xml-Name");
			Assert.AreEqual ("00000000000000000400000000000000", se.Attribute ("PublicKeyBlob"), "Xml-PublicKeyBlob");
			Assert.IsNull (se.Attribute ("AssemblyVersion"), "Xml-AssemblyVersion");

			StrongNameIdentityPermission copy = (StrongNameIdentityPermission)snip.Copy ();
			Assert.AreEqual ("mono", se.Attribute ("Name"), "Copy-Name");
			Assert.AreEqual ("00000000000000000400000000000000", se.Attribute ("PublicKeyBlob"), "Copy-PublicKeyBlob");
			Assert.IsNull (se.Attribute ("AssemblyVersion"), "Copy-AssemblyVersion");
		}

		[Test]
		public void StrongNameIdentityPermission_All ()
		{
			StrongNamePublicKeyBlob blob = new StrongNamePublicKeyBlob (ecma);
			StrongNameIdentityPermission snip = new StrongNameIdentityPermission (blob, "mono", new Version (1, 2, 3, 4));
			Assert.AreEqual ("mono", snip.Name, "Name");
			Assert.AreEqual ("00000000000000000400000000000000", snip.PublicKey.ToString (), "PublicKey");
			Assert.AreEqual ("1.2.3.4", snip.Version.ToString (), "Version");

			SecurityElement se = snip.ToXml ();
			Assert.AreEqual ("mono", se.Attribute ("Name"), "Xml-Name");
			Assert.AreEqual ("00000000000000000400000000000000", se.Attribute ("PublicKeyBlob"), "Xml-PublicKeyBlob");
			Assert.AreEqual ("1.2.3.4", se.Attribute ("AssemblyVersion"), "Xml-AssemblyVersion");

			StrongNameIdentityPermission copy = (StrongNameIdentityPermission)snip.Copy ();
			Assert.AreEqual ("mono", se.Attribute ("Name"), "Copy-Name");
			Assert.AreEqual ("00000000000000000400000000000000", se.Attribute ("PublicKeyBlob"), "Copy-PublicKeyBlob");
			Assert.AreEqual ("1.2.3.4", se.Attribute ("AssemblyVersion"), "Copy-AssemblyVersion");
		}

		[Test]
		public void Name ()
		{
			StrongNamePublicKeyBlob blob = new StrongNamePublicKeyBlob (ecma);
			StrongNameIdentityPermission snip = new StrongNameIdentityPermission (blob, "mono", new Version (1, 2, 3, 4));
			Assert.AreEqual ("mono", snip.Name, "Name-1");
			snip.Name = null;
			Assert.IsNull (snip.Name, "Name-2");
			snip.Name = "mono";
			Assert.AreEqual ("mono", snip.Name, "Name-3");
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void Name_Empty ()
		{
			StrongNamePublicKeyBlob blob = new StrongNamePublicKeyBlob (ecma);
			StrongNameIdentityPermission snip = new StrongNameIdentityPermission (blob, "mono", new Version (1, 2, 3, 4));
			snip.Name = String.Empty;
#if !NET_2_0
			Assert.AreEqual (String.Empty, snip.Name, "Name");
#endif
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void PublicKey_Null ()
		{
			StrongNamePublicKeyBlob blob = new StrongNamePublicKeyBlob (ecma);
			StrongNameIdentityPermission snip = new StrongNameIdentityPermission (blob, "mono", new Version (1, 2, 3, 4));
			snip.PublicKey = null;
		}

		[Test]
		public void Version ()
		{
			StrongNamePublicKeyBlob blob = new StrongNamePublicKeyBlob (ecma);
			StrongNameIdentityPermission snip = new StrongNameIdentityPermission (blob, "mono", new Version (1, 2, 3, 4));
			Assert.AreEqual ("1.2.3.4", snip.Version.ToString (), "Version-1");
			snip.Version = null;
			Assert.IsNull (snip.Version, "Version-2");
			snip.Version = new Version (1, 2, 3);
			Assert.AreEqual ("1.2.3", snip.Version.ToString (), "Version-3");
		}

		[Test]
		public void Copy_NameEmpty ()
		{
			StrongNameIdentityPermission snip = new StrongNameIdentityPermission (PermissionState.None);
			snip.PublicKey = new StrongNamePublicKeyBlob (ecma);
			snip.Version = new Version ("1.2.3.4");

			// because Name == String.Empty, which is illegal using the other constructor
			// but (somewhat) required to copy the teo other informations
			StrongNameIdentityPermission copy = (StrongNameIdentityPermission)snip.Copy ();
			Assert.IsTrue (copy.Equals (snip), "Equals");
		}

		private void Compare (StrongNameIdentityPermission p1, StrongNameIdentityPermission p2, string prefix)
		{
			Assert.AreEqual (p1.Name, p2.Name, prefix + ".Name");
			Assert.AreEqual (p1.PublicKey, p2.PublicKey, prefix + ".PublicKey");
			Assert.AreEqual (p1.Version, p2.Version, prefix + ".Version");
			Assert.IsFalse (Object.ReferenceEquals (p1, p2), "ReferenceEquals");
		}

		[Test]
		public void Intersect ()
		{
			StrongNamePublicKeyBlob blob = new StrongNamePublicKeyBlob (ecma);
			StrongNameIdentityPermission snip = new StrongNameIdentityPermission (blob, "mono", new Version (1, 2, 3, 4));

			StrongNameIdentityPermission intersect = (StrongNameIdentityPermission)snip.Intersect (null);
			Assert.IsNull (intersect, "snip N null");

			StrongNameIdentityPermission empty = new StrongNameIdentityPermission (PermissionState.None);
			intersect = (StrongNameIdentityPermission)snip.Intersect (empty);
#if NET_2_0
			Assert.IsNull (intersect, "snip N empty");
#else
			Compare (empty, intersect, "snip U empty");
#endif
			intersect = (StrongNameIdentityPermission)snip.Intersect (snip);
			Compare (snip, intersect, "snip U snip");

			StrongNameIdentityPermission samePk = new StrongNameIdentityPermission (blob, "novell", new Version (1, 2));
			intersect = (StrongNameIdentityPermission)snip.Intersect (samePk);
			Assert.IsNull (intersect, "(snip N samePk)");
			// strange, I would have expected a SNIP with the same public key...
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof (ArgumentException))]
#endif
		public void Intersect_DifferentPermissions ()
		{
			StrongNameIdentityPermission a = new StrongNameIdentityPermission (PermissionState.None);
			SecurityPermission b = new SecurityPermission (PermissionState.None);
			Assert.IsNull (a.Intersect (b));
		}

		[Test]
		public void IsSubsetOf ()
		{
			StrongNamePublicKeyBlob blob = new StrongNamePublicKeyBlob (ecma);
			StrongNameIdentityPermission snip = new StrongNameIdentityPermission (blob, "mono", new Version (1, 2, 3, 4));
			Assert.IsFalse (snip.IsSubsetOf (null), "snip.IsSubsetOf (null)");

			StrongNameIdentityPermission empty = new StrongNameIdentityPermission (PermissionState.None);
			Assert.IsTrue (empty.IsSubsetOf (null), "empty.IsSubsetOf (null)");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void IsSubsetOf_DifferentPermissions ()
		{
			StrongNameIdentityPermission a = new StrongNameIdentityPermission (PermissionState.None);
			SecurityPermission b = new SecurityPermission (PermissionState.None);
			a.IsSubsetOf (b);
		}

		[Test]
		public void Union ()
		{
			StrongNamePublicKeyBlob blob = new StrongNamePublicKeyBlob (ecma);
			StrongNameIdentityPermission snip = new StrongNameIdentityPermission (blob, "mono", new Version (1, 2, 3, 4));

			StrongNameIdentityPermission union = (StrongNameIdentityPermission)snip.Union (null);
			Compare (snip, union, "snip U null");

			StrongNameIdentityPermission empty = new StrongNameIdentityPermission (PermissionState.None);
			union = (StrongNameIdentityPermission)snip.Union (empty);
			Compare (snip, union, "snip U empty");

			union = (StrongNameIdentityPermission)snip.Union (snip);
			Compare (snip, union, "snip U snip");

			// note: can't be tested with PermissionState.Unrestricted

			StrongNameIdentityPermission samePk = new StrongNameIdentityPermission (blob, null, null);
			union = (StrongNameIdentityPermission)snip.Union (samePk);
#if !NET_2_0
			// can't compare the properties with multiple entries
			Compare (snip, union, "snip U samePk");
#endif
			Assert.IsTrue (snip.IsSubsetOf (union), "snip.IsSubsetOf (union)");

			union = (StrongNameIdentityPermission)samePk.Union (snip);
#if !NET_2_0
			// can't compare the properties with multiple entries
			Compare (snip, union, "samePk U snip");
#endif
			Assert.IsTrue (samePk.IsSubsetOf (union), "snip.IsSubsetOf (union)");
		}

		[Test]
		public void Union_DifferentPk ()
		{
			StrongNamePublicKeyBlob blob = new StrongNamePublicKeyBlob (ecma);
			StrongNameIdentityPermission snip = new StrongNameIdentityPermission (blob, "mono", new Version (1, 2, 3, 4));
			StrongNamePublicKeyBlob blob2 = new StrongNamePublicKeyBlob (new byte [16]);
			StrongNameIdentityPermission diffPk = new StrongNameIdentityPermission (blob2, "mono", new Version (1, 2, 3, 4));
			StrongNameIdentityPermission result = (StrongNameIdentityPermission) snip.Union (diffPk);
#if NET_2_0
			Assert.IsNotNull (result, "DifferentPk");
			// new XML format is used to contain more than one site
			SecurityElement se = result.ToXml ();
			Assert.AreEqual (2, se.Children.Count, "Childs");
			Assert.AreEqual ("00000000000000000400000000000000", (se.Children [0] as SecurityElement).Attribute ("PublicKeyBlob"), "Blob#1");
			Assert.AreEqual ("00000000000000000000000000000000", (se.Children [1] as SecurityElement).Attribute ("PublicKeyBlob"), "Blob#2");
			// strangely it is still versioned as 'version="1"'.
			Assert.AreEqual ("1", se.Attribute ("version"), "Version");
#endif
		}

		[Test]
		public void Union_SamePublicKey_DifferentName ()
		{
			StrongNamePublicKeyBlob blob = new StrongNamePublicKeyBlob (ecma);
			StrongNameIdentityPermission snip = new StrongNameIdentityPermission (blob, "mono", new Version (1, 2, 3, 4));
			StrongNameIdentityPermission diffName = new StrongNameIdentityPermission (blob, "novell", null);
			StrongNameIdentityPermission result = (StrongNameIdentityPermission) snip.Union (diffName);
#if NET_2_0
			Assert.IsNotNull (result, "DifferentName");
			// new XML format is used to contain more than one site
			SecurityElement se = result.ToXml ();
			Assert.AreEqual (2, se.Children.Count, "Childs");
			Assert.AreEqual ("mono", (se.Children [0] as SecurityElement).Attribute ("Name"), "Name#1");
			Assert.AreEqual ("novell", (se.Children [1] as SecurityElement).Attribute ("Name"), "Name#2");
			// strangely it is still versioned as 'version="1"'.
			Assert.AreEqual ("1", se.Attribute ("version"), "Version");
#endif
		}

		[Test]
		public void Union_SamePublicKey_DifferentVersion ()
		{
			StrongNamePublicKeyBlob blob = new StrongNamePublicKeyBlob (ecma);
			StrongNameIdentityPermission snip = new StrongNameIdentityPermission (blob, "mono", new Version (1, 2, 3, 4));
			StrongNameIdentityPermission diffVersion = new StrongNameIdentityPermission (blob, null, new Version (1, 2));
			StrongNameIdentityPermission result = (StrongNameIdentityPermission) snip.Union (diffVersion);
#if NET_2_0
			Assert.IsNotNull (result, "DifferentVersion");
			// new XML format is used to contain more than one site
			SecurityElement se = result.ToXml ();
			Assert.AreEqual (2, se.Children.Count, "Childs");
			Assert.AreEqual ("1.2.3.4", (se.Children [0] as SecurityElement).Attribute ("AssemblyVersion"), "AssemblyVersion#1");
			Assert.AreEqual ("1.2", (se.Children [1] as SecurityElement).Attribute ("AssemblyVersion"), "AssemblyVersion#2");
			// strangely it is still versioned as 'version="1"'.
			Assert.AreEqual ("1", se.Attribute ("version"), "Version");
#endif
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Union_DifferentPermissions ()
		{
			StrongNameIdentityPermission a = new StrongNameIdentityPermission (PermissionState.None);
			SecurityPermission b = new SecurityPermission (PermissionState.None);
			a.Union (b);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_Null ()
		{
			StrongNameIdentityPermission snip = new StrongNameIdentityPermission (PermissionState.None);
			snip.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTag ()
		{
			StrongNameIdentityPermission snip = new StrongNameIdentityPermission (PermissionState.None);
			SecurityElement se = snip.ToXml ();
			se.Tag = "IMono";
			snip.FromXml (se);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongTagCase ()
		{
			StrongNameIdentityPermission snip = new StrongNameIdentityPermission (PermissionState.None);
			SecurityElement se = snip.ToXml ();
			se.Tag = "IPERMISSION"; // instead of IPermission
			snip.FromXml (se);
		}

		[Test]
		public void FromXml_WrongClass ()
		{
			StrongNameIdentityPermission snip = new StrongNameIdentityPermission (PermissionState.None);
			SecurityElement se = snip.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", "Wrong" + se.Attribute ("class"));
			w.AddAttribute ("version", se.Attribute ("version"));
			snip.FromXml (w);
			// doesn't care of the class name at that stage
			// anyway the class has already be created so...
		}

		[Test]
		public void FromXml_NoClass ()
		{
			StrongNameIdentityPermission snip = new StrongNameIdentityPermission (PermissionState.None);
			SecurityElement se = snip.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("version", se.Attribute ("version"));
			snip.FromXml (w);
			// doesn't even care of the class attribute presence
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_WrongVersion ()
		{
			StrongNameIdentityPermission snip = new StrongNameIdentityPermission (PermissionState.None);
			SecurityElement se = snip.ToXml ();
			se.Attributes.Remove ("version");
			se.Attributes.Add ("version", "2");
			snip.FromXml (se);
		}

		[Test]
		public void FromXml_NoVersion ()
		{
			StrongNameIdentityPermission snip = new StrongNameIdentityPermission (PermissionState.None);
			SecurityElement se = snip.ToXml ();

			SecurityElement w = new SecurityElement (se.Tag);
			w.AddAttribute ("class", se.Attribute ("class"));
			snip.FromXml (w);
		}

		[Test]
		public void FromXml_NameEmpty ()
		{
			StrongNameIdentityPermission snip = new StrongNameIdentityPermission (PermissionState.None);
			SecurityElement se = snip.ToXml ();
			snip.FromXml (se);

			snip.PublicKey = new StrongNamePublicKeyBlob (ecma);
			snip.Version = new Version ("1.2.3.4");
			se = snip.ToXml ();
			snip.FromXml (se);
		}
	}
}
