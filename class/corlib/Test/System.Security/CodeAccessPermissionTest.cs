//
// CodeAccessPermissionTest.cs - NUnit Test Cases for CodeAccessPermission
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2004 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Security;

namespace MonoTests.System.Security {

	// Almost minimal CAS class for unit tests
	[Serializable]
	public class NonAbstractCodeAccessPermission : CodeAccessPermission {

		private string _tag;
		private string _text;

		public NonAbstractCodeAccessPermission (string tag, string text)
		{
			_tag = tag;
			_text = text;
		}

		public override IPermission Copy () 
		{
			return new NonAbstractCodeAccessPermission (_tag, _text);
		}

		public override void FromXml (SecurityElement elem) {}

		public override IPermission Intersect (IPermission target) 
		{
			return null;
		}

		public override bool IsSubsetOf (IPermission target) 
		{
			return false;
		}

		public override SecurityElement ToXml () 
		{
			if (_tag == null)
				return null;
			return new SecurityElement (_tag, _text);
		}
	}

	[TestFixture]
	public class CodeAccessPermissionTest : Assertion {

		[Test]
		public void Union () 
		{
			NonAbstractCodeAccessPermission cap = new NonAbstractCodeAccessPermission (null, null);
			IPermission p = cap.Union (null);
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void To_String_Exception () 
		{
			NonAbstractCodeAccessPermission cap = new NonAbstractCodeAccessPermission (null, null);
			string s = cap.ToString ();
		}

		[Test]
		public void To_String () 
		{
			NonAbstractCodeAccessPermission cap = new NonAbstractCodeAccessPermission ("CodeAccessPermission", "NonAbstract");
			string s = cap.ToString ();
			AssertEquals ("ToString", "<CodeAccessPermission>NonAbstract</CodeAccessPermission>\r\n", s);
		}
	}
}
