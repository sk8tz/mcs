//
// PrincipalPermissionAttributeTest.cs - NUnit Test Cases for PrincipalPermissionAttribute
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Security;
using System.Security.Permissions;

namespace MonoTests.System.Security.Permissions {

	[TestFixture]
	public class PrincipalPermissionAttributeTest : Assertion {

		private static string user = "user";
		private static string role = "role";

		[Test]
		public void Default () 
		{
			PrincipalPermissionAttribute a = new PrincipalPermissionAttribute (SecurityAction.Assert);
			AssertNull ("Name", a.Name);
			AssertNull ("Role", a.Role);
			Assert ("Authenticated", a.Authenticated);
			AssertEquals ("TypeId", a.ToString (), a.TypeId.ToString ());
			Assert ("Unrestricted", !a.Unrestricted);

			PrincipalPermission perm = (PrincipalPermission) a.CreatePermission ();
			AssertNotNull ("CreatePermission", perm);
		}

		[Test]
		public void Action () 
		{
			PublisherIdentityPermissionAttribute a = new PublisherIdentityPermissionAttribute (SecurityAction.Assert);
			AssertEquals ("Action=Assert", SecurityAction.Assert, a.Action);
			a.Action = SecurityAction.Demand;
			AssertEquals ("Action=Demand", SecurityAction.Demand, a.Action);
			a.Action = SecurityAction.Deny;
			AssertEquals ("Action=Deny", SecurityAction.Deny, a.Action);
			a.Action = SecurityAction.InheritanceDemand;
			AssertEquals ("Action=InheritanceDemand", SecurityAction.InheritanceDemand, a.Action);
			a.Action = SecurityAction.LinkDemand;
			AssertEquals ("Action=LinkDemand", SecurityAction.LinkDemand, a.Action);
			a.Action = SecurityAction.PermitOnly;
			AssertEquals ("Action=PermitOnly", SecurityAction.PermitOnly, a.Action);
			a.Action = SecurityAction.RequestMinimum;
			AssertEquals ("Action=RequestMinimum", SecurityAction.RequestMinimum, a.Action);
			a.Action = SecurityAction.RequestOptional;
			AssertEquals ("Action=RequestOptional", SecurityAction.RequestOptional, a.Action);
			a.Action = SecurityAction.RequestRefuse;
			AssertEquals ("Action=RequestRefuse", SecurityAction.RequestRefuse, a.Action);
		}

		[Test]
		public void NameNullRoleNullAuthenticated () 
		{
			PrincipalPermissionAttribute attr = new PrincipalPermissionAttribute (SecurityAction.Assert);
			attr.Name = null;
			attr.Role = null;
			attr.Authenticated = true;
			AssertNull ("NameNullRoleNullAuthenticated.Name", attr.Name);
			AssertNull ("NameNullRoleNullAuthenticated.Role", attr.Role);
			Assert ("NameNullRoleNullAuthenticated.Authenticated", attr.Authenticated);
			PrincipalPermission p = (PrincipalPermission) attr.CreatePermission ();
			Assert ("NameNullRoleNullAuthenticated.IsUnrestricted", p.IsUnrestricted ());
		}

		[Test]
		public void NameNullRoleNullNonAuthenticated () 
		{
			PrincipalPermissionAttribute attr = new PrincipalPermissionAttribute (SecurityAction.Assert);
			attr.Name = null;
			attr.Role = null;
			attr.Authenticated = false;
			AssertNull ("NameNullRoleNullNonAuthenticated.Name", attr.Name);
			AssertNull ("NameNullRoleNullNonAuthenticated.Role", attr.Role);
			Assert ("NameNullRoleNullNonAuthenticated.Authenticated", !attr.Authenticated);
			PrincipalPermission p = (PrincipalPermission) attr.CreatePermission ();
			Assert ("NameNullRoleNullNonAuthenticated.IsUnrestricted", !p.IsUnrestricted ());
		}

		[Test]
		public void NameRoleNullAuthenticated () 
		{
			PrincipalPermissionAttribute attr = new PrincipalPermissionAttribute (SecurityAction.Assert);
			attr.Name = user;
			attr.Role = null;
			attr.Authenticated = true;
			AssertEquals ("NameRoleNullAuthenticated.Name", user, attr.Name);
			AssertNull ("NameRoleNullAuthenticated.Role", attr.Role);
			Assert ("NameRoleNullAuthenticated.Authenticated", attr.Authenticated);
			PrincipalPermission p = (PrincipalPermission) attr.CreatePermission ();
			Assert ("NameRoleNullAuthenticated.IsUnrestricted", !p.IsUnrestricted ());
		}

		[Test]
		public void NameRoleNullNonAuthenticated () 
		{
			PrincipalPermissionAttribute attr = new PrincipalPermissionAttribute (SecurityAction.Assert);
			attr.Name = user;
			attr.Role = null;
			attr.Authenticated = false;
			AssertEquals ("NameRoleNullNonAuthenticated.Name", user, attr.Name);
			AssertNull ("NameRoleNullNonAuthenticated.Role", attr.Role);
			Assert ("NameRoleNullNonAuthenticated.Authenticated", !attr.Authenticated);
			PrincipalPermission p = (PrincipalPermission) attr.CreatePermission ();
			Assert ("NameRoleNullNonAuthenticated.IsUnrestricted", !p.IsUnrestricted ());
		}

		[Test]
		public void NameNullRoleAuthenticated () 
		{
			PrincipalPermissionAttribute attr = new PrincipalPermissionAttribute (SecurityAction.Assert);
			attr.Name = null;
			attr.Role = role;
			attr.Authenticated = true;
			AssertNull ("NameNullRoleAuthenticated.Name", attr.Name);
			AssertEquals ("NameNullRoleAuthenticated.Role", role, attr.Role);
			Assert ("NameNullRoleAuthenticated.Authenticated", attr.Authenticated);
			PrincipalPermission p = (PrincipalPermission) attr.CreatePermission ();
			Assert ("NameNullRoleAuthenticated.IsUnrestricted", !p.IsUnrestricted ());
		}

		[Test]
		public void NameNullRoleNonAuthenticated () 
		{
			PrincipalPermissionAttribute attr = new PrincipalPermissionAttribute (SecurityAction.Assert);
			attr.Name = null;
			attr.Role = role;
			attr.Authenticated = false;
			AssertNull ("NameNullRoleNonAuthenticated.Name", attr.Name);
			AssertEquals ("NameNullRoleNonAuthenticated.Role", role, attr.Role);
			Assert ("NameNullRoleNonAuthenticated.Authenticated", !attr.Authenticated);
			PrincipalPermission p = (PrincipalPermission) attr.CreatePermission ();
			Assert ("NameNullRoleNonAuthenticated.IsUnrestricted", !p.IsUnrestricted ());
		}

		[Test]
		public void NameRoleAuthenticated () 
		{
			PrincipalPermissionAttribute attr = new PrincipalPermissionAttribute (SecurityAction.Assert);
			attr.Name = user;
			attr.Role = role;
			attr.Authenticated = true;
			AssertEquals ("NameRoleAuthenticated.Name", user, attr.Name);
			AssertEquals ("NameRoleAuthenticated.Role", role, attr.Role);
			Assert ("NameRoleAuthenticated.Authenticated", attr.Authenticated);
			PrincipalPermission p = (PrincipalPermission) attr.CreatePermission ();
			Assert ("NameRoleAuthenticated.IsUnrestricted", !p.IsUnrestricted ());
		}

		[Test]
		public void NameRoleNonAuthenticated () 
		{
			PrincipalPermissionAttribute attr = new PrincipalPermissionAttribute (SecurityAction.Assert);
			attr.Name = user;
			attr.Role = role;
			attr.Authenticated = false;
			AssertEquals ("NameRoleNonAuthenticated.Name", user, attr.Name);
			AssertEquals ("NameRoleNonAuthenticated.Role", role, attr.Role);
			Assert ("NameRoleNonAuthenticated.Authenticated", !attr.Authenticated);
			PrincipalPermission p = (PrincipalPermission) attr.CreatePermission ();
			Assert ("NameRoleNonAuthenticated.IsUnrestricted", !p.IsUnrestricted ());
		}

		[Test]
		public void Unrestricted () 
		{
			PrincipalPermissionAttribute a = new PrincipalPermissionAttribute (SecurityAction.Assert);
			a.Unrestricted = true;

			PrincipalPermission perm = (PrincipalPermission) a.CreatePermission ();
			Assert ("CreatePermission.IsUnrestricted", perm.IsUnrestricted ());
		}
	}
}
