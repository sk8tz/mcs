//
// PolicyLevelTest.cs - NUnit Test Cases for PolicyLevel
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2004 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

namespace MonoTests.System.Security.Policy {

	[TestFixture]
	public class PolicyLevelTest : Assertion {

		static string minimal = null;
		static string minimal_policy = null;
		static byte[] snPublicKey = { 0x00, 0x24, 0x00, 0x00, 0x04, 0x80, 0x00, 0x00, 0x94, 0x00, 0x00, 0x00, 0x06, 0x02, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x52, 0x53, 0x41, 0x31, 0x00, 0x04, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x3D, 0xBD, 0x72, 0x08, 0xC6, 0x2B, 0x0E, 0xA8, 0xC1, 0xC0, 0x58, 0x07, 0x2B, 0x63, 0x5F, 0x7C, 0x9A, 0xBD, 0xCB, 0x22, 0xDB, 0x20, 0xB2, 0xA9, 0xDA, 0xDA, 0xEF, 0xE8, 0x00, 0x64, 0x2F, 0x5D, 0x8D, 0xEB, 0x78, 0x02, 0xF7, 0xA5, 0x36, 0x77, 0x28, 0xD7, 0x55, 0x8D, 0x14, 0x68, 0xDB, 0xEB, 0x24, 0x09, 0xD0, 0x2B, 0x13, 0x1B, 0x92, 0x6E, 0x2E, 0x59, 0x54, 0x4A, 0xAC, 0x18, 0xCF, 0xC9, 0x09, 0x02, 0x3F, 0x4F, 0xA8, 0x3E, 0x94, 0x00, 0x1F, 0xC2, 0xF1, 0x1A, 0x27, 0x47, 0x7D, 0x10, 0x84, 0xF5, 0x14, 0xB8, 0x61, 0x62, 0x1A, 0x0C, 0x66, 0xAB, 0xD2, 0x4C, 0x4B, 0x9F, 0xC9, 0x0F, 0x3C, 0xD8, 0x92, 0x0F, 0xF5, 0xFF, 0xCE, 0xD7, 0x6E, 0x5C, 0x6F, 0xB1, 0xF5, 0x7D, 0xD3, 0x56, 0xF9, 0x67, 0x27, 0xA4, 0xA5, 0x48, 0x5B, 0x07, 0x93, 0x44, 0x00, 0x4A, 0xF8, 0xFF, 0xA4, 0xCB };

		[SetUp]
		public void SetUp () 
		{
			if (minimal == null) {
				minimal_policy = "<PolicyLevel version=\"1\">\r\n   <SecurityClasses>\r\n      <SecurityClass Name=\"PrintingPermission\"\r\n                     Description=\"System.Drawing.Printing.PrintingPermission, System.Drawing, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a\"/>\r\n      <SecurityClass Name=\"NamedPermissionSet\"\r\n                     Description=\"System.Security.NamedPermissionSet\"/>\r\n      <SecurityClass Name=\"ReflectionPermission\"\r\n                     Description=\"System.Security.Permissions.ReflectionPermission, mscorlib, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\"/>\r\n      <SecurityClass Name=\"DnsPermission\"\r\n                     Description=\"System.Net.DnsPermission, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\"/>\r\n      <SecurityClass Name=\"EventLogPermission\"\r\n                     Description=\"System.Diagnostics.EventLogPermission, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\"/>\r\n      <SecurityClass Name=\"IsolatedStorageFilePermission\"\r\n                     Description=\"System.Security.Permissions.IsolatedStorageFilePermission, mscorlib, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\"/>\r\n      <SecurityClass Name=\"AllMembershipCondition\"\r\n                     Description=\"System.Security.Policy.AllMembershipCondition, mscorlib, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\"/>\r\n      <SecurityClass Name=\"FirstMatchCodeGroup\"\r\n                     Description=\"System.Security.Policy.FirstMatchCodeGroup\"/>\r\n      <SecurityClass Name=\"EnvironmentPermission\"\r\n                     Description=\"System.Security.Permissions.EnvironmentPermission, mscorlib, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\"/>\r\n";
				minimal_policy += "      <SecurityClass Name=\"StrongNameMembershipCondition\"\r\n                     Description=\"System.Security.Policy.StrongNameMembershipCondition, mscorlib, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\"/>\r\n      <SecurityClass Name=\"SecurityPermission\"\r\n                     Description=\"System.Security.Permissions.SecurityPermission, mscorlib, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\"/>\r\n      <SecurityClass Name=\"UIPermission\"\r\n                     Description=\"System.Security.Permissions.UIPermission, mscorlib, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\"/>\r\n      <SecurityClass Name=\"FileDialogPermission\"\r\n                     Description=\"System.Security.Permissions.FileDialogPermission, mscorlib, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\"/>\r\n   </SecurityClasses>\r\n   <NamedPermissionSets>\r\n      <PermissionSet class=\"NamedPermissionSet\"\r\n                     version=\"1\"\r\n                     Unrestricted=\"true\"\r\n                     Name=\"FullTrust\"\r\n                     Description=\"Allows full access to all resources\"/>\r\n      <PermissionSet class=\"NamedPermissionSet\"\r\n                     version=\"1\"\r\n                     Name=\"SkipVerification\"\r\n                     Description=\"Grants right to bypass the verification\">\r\n         <IPermission class=\"SecurityPermission\"\r\n                      version=\"1\"\r\n                      Flags=\"SkipVerification\"/>\r\n      </PermissionSet>\r\n      <PermissionSet class=\"NamedPermissionSet\"\r\n                     version=\"1\"\r\n                     Name=\"Execution\"\r\n                     Description=\"Permits execution\">\r\n         <IPermission class=\"SecurityPermission\"\r\n                      version=\"1\"\r\n                      Flags=\"Execution\"/>\r\n      </PermissionSet>\r\n";
				minimal_policy += "      <PermissionSet class=\"NamedPermissionSet\"\r\n                     version=\"1\"\r\n                     Name=\"Nothing\"\r\n                     Description=\"Denies all resources, including the right to execute\"/>\r\n      <PermissionSet class=\"NamedPermissionSet\"\r\n                     version=\"1\"\r\n                     Name=\"LocalIntranet\"\r\n                     Description=\"Default rights given to applications on the local intranet\">\r\n         <IPermission class=\"EnvironmentPermission\"\r\n                      version=\"1\"\r\n                      Read=\"USERNAME\"/>\r\n         <IPermission class=\"FileDialogPermission\"\r\n                      version=\"1\"\r\n                      Unrestricted=\"true\"/>\r\n         <IPermission class=\"IsolatedStorageFilePermission\"\r\n                      version=\"1\"\r\n                      Allowed=\"AssemblyIsolationByUser\"\r\n                      UserQuota=\"9223372036854775807\"\r\n                      Expiry=\"9223372036854775807\"\r\n                      Permanent=\"True\"/>\r\n         <IPermission class=\"ReflectionPermission\"\r\n                      version=\"1\"\r\n                      Flags=\"ReflectionEmit\"/>\r\n         <IPermission class=\"SecurityPermission\"\r\n                      version=\"1\"\r\n                      Flags=\"Assertion, Execution, BindingRedirects\"/>\r\n         <IPermission class=\"UIPermission\"\r\n                      version=\"1\"\r\n                      Unrestricted=\"true\"/>\r\n         <IPermission class=\"DnsPermission\"\r\n                      version=\"1\"\r\n                      Unrestricted=\"true\"/>\r\n         <IPermission class=\"PrintingPermission\"\r\n                      version=\"1\"\r\n                      Level=\"DefaultPrinting\"/>\r\n         <IPermission class=\"EventLogPermission\"\r\n                      version=\"1\">\r\n            <Machine name=\".\"\r\n                     access=\"Instrument\"/>\r\n";
				minimal_policy += "         </IPermission>\r\n      </PermissionSet>\r\n      <PermissionSet class=\"NamedPermissionSet\"\r\n                     version=\"1\"\r\n                     Name=\"Internet\"\r\n                     Description=\"Default rights given to internet applications\">\r\n         <IPermission class=\"FileDialogPermission\"\r\n                      version=\"1\"\r\n                      Access=\"Open\"/>\r\n         <IPermission class=\"IsolatedStorageFilePermission\"\r\n                      version=\"1\"\r\n                      Allowed=\"DomainIsolationByUser\"\r\n                      UserQuota=\"10240\"/>\r\n         <IPermission class=\"SecurityPermission\"\r\n                      version=\"1\"\r\n                      Flags=\"Execution\"/>\r\n         <IPermission class=\"UIPermission\"\r\n                      version=\"1\"\r\n                      Window=\"SafeTopLevelWindows\"\r\n                      Clipboard=\"OwnClipboard\"/>\r\n         <IPermission class=\"PrintingPermission\"\r\n                      version=\"1\"\r\n                      Level=\"SafePrinting\"/>\r\n      </PermissionSet>\r\n   </NamedPermissionSets>\r\n   <CodeGroup class=\"FirstMatchCodeGroup\"\r\n              version=\"1\"\r\n              PermissionSetName=\"Nothing\">\r\n      <IMembershipCondition class=\"AllMembershipCondition\"\r\n                            version=\"1\"/>\r\n   </CodeGroup>\r\n   <FullTrustAssemblies>\r\n      <IMembershipCondition class=\"StrongNameMembershipCondition\"\r\n                            version=\"1\"\r\n                            PublicKeyBlob=\"00000000000000000400000000000000\"\r\n                            Name=\"System\"/>\r\n   </FullTrustAssemblies>\r\n</PolicyLevel>\r\n";
				minimal = Envelope (minimal_policy);
			}
		}

		private string Envelope (string policy) 
		{
			return "<configuration><mscorlib><security><policy>" + policy + "</policy></security></mscorlib></configuration>";
		}

		private PolicyLevel Load (string xml, PolicyLevelType type) 
		{
			return SecurityManager.LoadPolicyLevelFromString (xml, type);
//			return SecurityManager.LoadPolicyLevelFromFile (@"C:\WINDOWS\Microsoft.NET\Framework\v1.0.3705\CONFIG\minimal.config", type);
		}

		[Test]
		public void AddFullTrustAssembly () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			int n = pl.FullTrustAssemblies.Count;

			StrongName sn = new StrongName (new StrongNamePublicKeyBlob (snPublicKey), "First", new Version (1, 2, 3, 4)); 
			pl.AddFullTrustAssembly (sn);
			AssertEquals ("FullTrustAssemblies.Count+1", n+1, pl.FullTrustAssemblies.Count);

			StrongNameMembershipCondition snmc = new StrongNameMembershipCondition (new StrongNamePublicKeyBlob (snPublicKey), "Second", new Version ("0.1.2.3"));
			pl.AddFullTrustAssembly (snmc);
			AssertEquals ("FullTrustAssemblies.Count+2", n+2, pl.FullTrustAssemblies.Count);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddFullTrustAssembly_NullStrongName () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			StrongName sn = null; 
			pl.AddFullTrustAssembly (sn);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddFullTrustAssembly_NullStrongNameMembershipCondition () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			StrongNameMembershipCondition snmc = null;
			pl.AddFullTrustAssembly (snmc);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddFullTrustAssembly_DuplicateStrongName () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			StrongName sn = new StrongName (new StrongNamePublicKeyBlob (snPublicKey), "First", new Version (1, 2, 3, 4)); 
			pl.AddFullTrustAssembly (sn);
			pl.AddFullTrustAssembly (sn);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddFullTrustAssembly_DuplicateStrongNameMembershipCondition () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			StrongNameMembershipCondition snmc = new StrongNameMembershipCondition (new StrongNamePublicKeyBlob (snPublicKey), "Second", new Version ("0.1.2.3"));
			pl.AddFullTrustAssembly (snmc);
			pl.AddFullTrustAssembly (snmc);
		}

		[Test]
		[Ignore ("System.ExecutionEngineException on MS runtime (1.1)")]
		public void AddNamedPermissionSet () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			int n = pl.NamedPermissionSets.Count;

			NamedPermissionSet nps = new NamedPermissionSet ("Mono", PermissionState.Unrestricted);
			pl.AddNamedPermissionSet (nps);
			// ExecutionEngineException here!
			AssertEquals ("NamedPermissionSets.Count+1", n+1, pl.NamedPermissionSets.Count);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddNamedPermissionSet_Null () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			pl.AddNamedPermissionSet (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		[Ignore ("System.ExecutionEngineException on MS runtime (1.1)")]
		public void AddNamedPermissionSet_Duplicate () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			NamedPermissionSet nps1 = new NamedPermissionSet ("Mono", PermissionState.Unrestricted);
			pl.AddNamedPermissionSet (nps1);
			NamedPermissionSet nps2 = new NamedPermissionSet ("Mono", PermissionState.None);
			// ExecutionEngineException here!
			pl.AddNamedPermissionSet (nps2);
		}

		[Test]
		[Ignore ("System.ExecutionEngineException on MS runtime (1.1)")]
		public void ChangeNamedPermissionSet () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			NamedPermissionSet nps1 = new NamedPermissionSet ("Mono", PermissionState.Unrestricted);
			pl.AddNamedPermissionSet (nps1);

			NamedPermissionSet nps2 = new NamedPermissionSet ("Mono", PermissionState.None);
			// ExecutionEngineException here!
			pl.ChangeNamedPermissionSet ("Mono", nps2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ChangeNamedPermissionSet_NullName () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			NamedPermissionSet nps2 = new NamedPermissionSet ("Mono", PermissionState.None);
			pl.ChangeNamedPermissionSet (null, nps2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ChangeNamedPermissionSet_NullPermissionSet () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			pl.ChangeNamedPermissionSet ("Mono", null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ChangeNamedPermissionSet_NotFound () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			NamedPermissionSet nps2 = new NamedPermissionSet ("Mono", PermissionState.None);
			pl.ChangeNamedPermissionSet ("Mono", nps2);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ChangeNamedPermissionSet_Reserved_FullTrust () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			PermissionSet ps = new PermissionSet (PermissionState.None);
			pl.ChangeNamedPermissionSet ("FullTrust", ps);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ChangeNamedPermissionSet_Reserved_LocalIntranet () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			PermissionSet ps = new PermissionSet (PermissionState.None);
			pl.ChangeNamedPermissionSet ("LocalIntranet", ps);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ChangeNamedPermissionSet_Reserved_Internet () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			PermissionSet ps = new PermissionSet (PermissionState.None);
			pl.ChangeNamedPermissionSet ("Internet", ps);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ChangeNamedPermissionSet_Reserved_SkipVerification () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			PermissionSet ps = new PermissionSet (PermissionState.None);
			pl.ChangeNamedPermissionSet ("SkipVerification", ps);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ChangeNamedPermissionSet_Reserved_ () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			PermissionSet ps = new PermissionSet (PermissionState.None);
			pl.ChangeNamedPermissionSet ("Execution", ps);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ChangeNamedPermissionSet_Reserved_Nothing () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			PermissionSet ps = new PermissionSet (PermissionState.None);
			pl.ChangeNamedPermissionSet ("SkipVerification", ps);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ChangeNamedPermissionSet_Reserved_Everything () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			PermissionSet ps = new PermissionSet (PermissionState.None);
			pl.ChangeNamedPermissionSet ("Everything", ps);
		}

		[Test]
		public void CreateAppDomainLevel () 
		{
			PolicyLevel pl = PolicyLevel.CreateAppDomainLevel ();
			AssertEquals ("Label", "AppDomain", pl.Label);
			AssertEquals ("RootCodeGroup==FullTrust", "FullTrust", pl.RootCodeGroup.PermissionSetName);
			AssertEquals ("RootCodeGroup/NoChildren", 0, pl.RootCodeGroup.Children.Count);
			Assert ("RootCodeGroup.PolicyStatement.PermissionSet.IsUnrestricted", pl.RootCodeGroup.PolicyStatement.PermissionSet.IsUnrestricted ());
		}

		[Test]
		public void FromXml () 
		{
			PolicyLevel pl = PolicyLevel.CreateAppDomainLevel ();
			SecurityElement se = pl.ToXml ();
			pl.FromXml (se);
			AssertEquals ("Label", "AppDomain", pl.Label);
			AssertEquals ("RootCodeGroup", "All_Code", pl.RootCodeGroup.Name);
			AssertEquals ("RootCodeGroup", "FullTrust", pl.RootCodeGroup.PermissionSetName);
			AssertEquals ("RootCodeGroup", 0, pl.RootCodeGroup.Children.Count);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void FromXml_Null () 
		{
			PolicyLevel pl = PolicyLevel.CreateAppDomainLevel ();
			pl.FromXml (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FromXml_Invalid () 
		{
			PolicyLevel pl = PolicyLevel.CreateAppDomainLevel ();
			SecurityElement se = pl.ToXml ();
			se.Tag = "Mono";
			// strangely this works :(
			pl.FromXml (se);
			// let's get weirder :)
			foreach (SecurityElement child in se.Children) {
				child.Tag = "Mono";
			}
			pl.FromXml (se);
			// it's enough >:)
		}

		[Test]
		[Ignore ("System.ExecutionEngineException on MS runtime (1.1)")]
		public void GetNamedPermissionSet () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			NamedPermissionSet nps = pl.GetNamedPermissionSet ("Mono");
			AssertNull ("GetNamedPermissionSet(notfound)", nps);
			nps = new NamedPermissionSet ("Mono", PermissionState.None);
			pl.AddNamedPermissionSet (nps);
			// ExecutionEngineException here!
			nps = pl.GetNamedPermissionSet ("Mono");
			AssertNotNull ("GetNamedPermissionSet(found)", nps);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetNamedPermissionSet_Null () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			NamedPermissionSet nps = pl.GetNamedPermissionSet (null);
		}

		[Test]
		public void Label () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.AppDomain);
			AssertEquals ("Label.AppDomain", "AppDomain", pl.Label);
			pl = Load (minimal, PolicyLevelType.Enterprise);
			AssertEquals ("Label.Enterprise", "Enterprise", pl.Label);
			pl = Load (minimal, PolicyLevelType.Machine);
			AssertEquals ("Label.Machine", "Machine", pl.Label);
			pl = Load (minimal, PolicyLevelType.User);
			AssertEquals ("Label.User", "User", pl.Label);
			// static method
			pl = PolicyLevel.CreateAppDomainLevel ();
			AssertEquals ("Label.AppDomain", "AppDomain", pl.Label);
		}

		[Test]
		public void Recover () 
		{
			// note: may be dangerous to test
		}

		[Test]
		public void RemoveFullTrustAssembly () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			int n = pl.FullTrustAssemblies.Count;

			StrongName sn = new StrongName (new StrongNamePublicKeyBlob (snPublicKey), "First", new Version (1, 2, 3, 4)); 
			pl.AddFullTrustAssembly (sn);
			AssertEquals ("FullTrustAssemblies.Count+1", n+1, pl.FullTrustAssemblies.Count);

			StrongNameMembershipCondition snmc = new StrongNameMembershipCondition (new StrongNamePublicKeyBlob (snPublicKey), "Second", new Version ("0.1.2.3"));
			pl.AddFullTrustAssembly (snmc);
			AssertEquals ("FullTrustAssemblies.Count+2", n+2, pl.FullTrustAssemblies.Count);

			pl.RemoveFullTrustAssembly (sn);
			AssertEquals ("FullTrustAssemblies.Count-1", n+1, pl.FullTrustAssemblies.Count);

			pl.RemoveFullTrustAssembly (snmc);
			AssertEquals ("FullTrustAssemblies.Count-2", n, pl.FullTrustAssemblies.Count);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void RemoveFullTrustAssembly_NullStrongName () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			StrongName sn = null; 
			pl.RemoveFullTrustAssembly (sn);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void RemoveFullTrustAssembly_NullStrongNameMembershipCondition () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			StrongNameMembershipCondition snmc = null;
			pl.RemoveFullTrustAssembly (snmc);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void RemoveFullTrustAssembly_UnknownStrongName () {
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			StrongName sn = new StrongName (new StrongNamePublicKeyBlob (snPublicKey), "First", new Version (1, 2, 3, 4)); 
			pl.RemoveFullTrustAssembly (sn);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void RemoveFullTrustAssembly_UnknownStrongNameMembershipCondition () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			StrongNameMembershipCondition snmc = new StrongNameMembershipCondition (new StrongNamePublicKeyBlob (snPublicKey), "Second", new Version ("0.1.2.3"));
			pl.RemoveFullTrustAssembly (snmc);
		}

		[Test]
		[Ignore ("System.ExecutionEngineException on MS runtime (1.1)")]
		public void RemoveNamedPermissionSet () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			int n = pl.NamedPermissionSets.Count;
			NamedPermissionSet nps = new NamedPermissionSet ("Mono", PermissionState.Unrestricted);
			pl.AddNamedPermissionSet (nps);
			// ExecutionEngineException here!
			pl.RemoveNamedPermissionSet (nps);
			AssertEquals ("NamedPermissionSets.Count", n, pl.NamedPermissionSets.Count);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void RemoveNamedPermissionSet_Null () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			pl.RemoveNamedPermissionSet ((NamedPermissionSet)null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void RemoveNamedPermissionSet_NotFound () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			NamedPermissionSet nps = new NamedPermissionSet ("Mono", PermissionState.Unrestricted);
			pl.RemoveNamedPermissionSet (nps);
		}

		[Test]
		[Ignore ("System.ExecutionEngineException on MS runtime (1.1)")]
		public void RemoveNamedPermissionSet_String () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			int n = pl.NamedPermissionSets.Count;
			NamedPermissionSet nps = new NamedPermissionSet ("Mono", PermissionState.Unrestricted);
			pl.AddNamedPermissionSet (nps);
			// ExecutionEngineException here!
			pl.RemoveNamedPermissionSet ("Mono");
			AssertEquals ("NamedPermissionSets.Count", n, pl.NamedPermissionSets.Count);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void RemoveNamedPermissionSet_StringNull () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			pl.RemoveNamedPermissionSet ((string)null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void RemoveNamedPermissionSet_StringNotFound () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			pl.RemoveNamedPermissionSet ("Mono");
		}

		[Test]
		[Ignore ("not yet implement in Mono - CAS related")]
		public void Reset () 
		{
			PolicyLevel pl = PolicyLevel.CreateAppDomainLevel ();

			int n = pl.FullTrustAssemblies.Count;
			StrongName sn = new StrongName (new StrongNamePublicKeyBlob (snPublicKey), "First", new Version (1, 2, 3, 4)); 
			pl.AddFullTrustAssembly (sn);
			AssertEquals ("FullTrustAssemblies.Count+1", n+1, pl.FullTrustAssemblies.Count);

			int m = pl.NamedPermissionSets.Count;

			NamedPermissionSet nps = new NamedPermissionSet ("Mono");
			// ExecutionEngineException here!
			//AssertEquals ("NamedPermissionSets.Count+1", m+1, pl.NamedPermissionSets.Count);

			pl.Reset ();
			AssertEquals ("FullTrustAssemblies.Count", n, pl.FullTrustAssemblies.Count);
			// ExecutionEngineException here!
			AssertEquals ("NamedPermissionSets.Count", m, pl.NamedPermissionSets.Count);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Resolve_Null () 
		{
			PolicyLevel pl = PolicyLevel.CreateAppDomainLevel ();
			pl.Resolve (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void RootCodeGroup_Null () 
		{
			PolicyLevel pl = PolicyLevel.CreateAppDomainLevel ();
			pl.RootCodeGroup = null;
		}

		[Test]
		public void StoreLocation () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			// loaded from a string - no store
			AssertNull ("StoreLocation(string)", pl.StoreLocation);

			string filename = Path.GetFullPath ("unittest.config");
			using (StreamWriter sw = new StreamWriter (filename, false)) {
				sw.Write (minimal);
			}
			pl = SecurityManager.LoadPolicyLevelFromFile (filename, PolicyLevelType.Machine);
			AssertEquals ("StoreLocation(file)", filename, pl.StoreLocation);
		}

		[Test]
		public void ToXml () 
		{
			PolicyLevel pl = Load (minimal, PolicyLevelType.Machine);
			PolicyLevel pl2 = PolicyLevel.CreateAppDomainLevel ();
			SecurityElement se = pl.ToXml ();
			pl2.FromXml (se);

			AssertEquals ("ToXml-FullTrustAssemblies", pl.FullTrustAssemblies.Count, pl2.FullTrustAssemblies.Count);
			AssertEquals ("ToXml-NamedPermissionSets", pl.NamedPermissionSets.Count, pl2.NamedPermissionSets.Count);
			Assert ("ToXml-RootCodeGroup", pl.RootCodeGroup.Equals (pl2.RootCodeGroup, true));
			AssertEquals ("ToXml-StoreLocation", pl.StoreLocation, pl2.StoreLocation);
		}
	}
}
