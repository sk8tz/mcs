//
// FileIOPermissionAttributeTest.cs - NUnit Test Cases for FileIOPermissionAttribute
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Runtime.InteropServices;


namespace MonoTests.System.Security.Permissions {

	public class FilePathUtil 
	{
		[DllImport("kernel32.dll")]
		private static extern uint GetLongPathName (string shortPath, 
			StringBuilder buffer, uint bufLength);

		static public string GetLongPathName (string somePath) 
		{
			StringBuilder buffer = new StringBuilder(260);
			StringBuilder temp = new StringBuilder();
			if (0 != GetLongPathName (somePath, buffer, (uint) buffer.Capacity))
				return buffer.ToString ();
			else
				return null;
		}
	}

	[TestFixture]
	public class FileIOPermissionAttributeTest : Assertion {

		private static string filename;

		[SetUp]
		public void SetUp () {
			Environment.CurrentDirectory = Path.GetTempPath();
			filename = Path.GetTempFileName ();
		}

		[TearDown]
		public void TearDown () {
			 if (File.Exists (filename))
				File.Delete (filename);
		}

		[Test]
		public void All () 
		{
			FileIOPermissionAttribute attr = new FileIOPermissionAttribute (SecurityAction.Assert);
			attr.All = Path.GetFullPath(filename);
			AssertEquals ("All=Append", filename, attr.Append);
			AssertEquals ("All=PathDiscovery", filename, attr.PathDiscovery);
			AssertEquals ("All=Read", filename, attr.Read);
			AssertEquals ("All=Write", filename, attr.Write);
			FileIOPermission p = (FileIOPermission) attr.CreatePermission ();
			AssertEquals ("All=FileIOPermissionAttribute-Append", FilePathUtil.GetLongPathName (filename), Path.GetFullPath(p.GetPathList (FileIOPermissionAccess.Append)[0]));
			AssertEquals ("All=FileIOPermissionAttribute-PathDiscovery", FilePathUtil.GetLongPathName (filename), p.GetPathList (FileIOPermissionAccess.PathDiscovery)[0]);
			AssertEquals ("All=FileIOPermissionAttribute-Read", FilePathUtil.GetLongPathName (filename), p.GetPathList (FileIOPermissionAccess.Read)[0]);
			AssertEquals ("All=FileIOPermissionAttribute-Write", FilePathUtil.GetLongPathName (filename), p.GetPathList (FileIOPermissionAccess.Write)[0]);
		}

		[Test]
		public void Append ()
		{
			FileIOPermissionAttribute attr = new FileIOPermissionAttribute (SecurityAction.Assert);
			attr.Append = Path.GetFullPath(filename);
			AssertEquals ("Append=Append", filename, attr.Append);
			AssertNull ("PathDiscovery=null", attr.PathDiscovery);
			AssertNull ("Read=null", attr.Read);
			AssertNull ("Write=null", attr.Write);
			FileIOPermission p = (FileIOPermission) attr.CreatePermission ();
			AssertEquals ("Append=FileIOPermissionAttribute-Append", FilePathUtil.GetLongPathName (filename), p.GetPathList (FileIOPermissionAccess.Append)[0]);
			AssertNull ("Append=FileIOPermissionAttribute-PathDiscovery", p.GetPathList (FileIOPermissionAccess.PathDiscovery));
			AssertNull ("Append=FileIOPermissionAttribute-Read", p.GetPathList (FileIOPermissionAccess.Read));
			AssertNull ("Append=FileIOPermissionAttribute-Write", p.GetPathList (FileIOPermissionAccess.Write));
		}

		[Test]
		public void PathDiscovery () 
		{
			FileIOPermissionAttribute attr = new FileIOPermissionAttribute (SecurityAction.Assert);
			attr.PathDiscovery = Path.GetFullPath(filename);
			AssertNull ("Append=null", attr.Append);
			AssertEquals ("PathDiscovery=PathDiscovery", filename, attr.PathDiscovery);
			AssertNull ("Read=null", attr.Read);
			AssertNull ("Write=null", attr.Write);
			FileIOPermission p = (FileIOPermission) attr.CreatePermission ();
			AssertNull ("PathDiscovery=FileIOPermissionAttribute-Append", p.GetPathList (FileIOPermissionAccess.Append));
			AssertEquals ("PathDiscovery=FileIOPermissionAttribute-PathDiscovery", FilePathUtil.GetLongPathName (filename), p.GetPathList (FileIOPermissionAccess.PathDiscovery)[0]);
			AssertNull ("PathDiscovery=FileIOPermissionAttribute-Read", p.GetPathList (FileIOPermissionAccess.Read));
			AssertNull ("PathDiscovery=FileIOPermissionAttribute-Write", p.GetPathList (FileIOPermissionAccess.Write));
		}

		[Test]
		public void Read () 
		{
			FileIOPermissionAttribute attr = new FileIOPermissionAttribute (SecurityAction.Assert);
			attr.Read = Path.GetFullPath(filename);
			AssertNull ("Append=null", attr.Append);
			AssertNull ("PathDiscovery=null", attr.PathDiscovery);
			AssertEquals ("Read=Read", filename, attr.Read);
			AssertNull ("Write=null", attr.Write);
			FileIOPermission p = (FileIOPermission) attr.CreatePermission ();
			AssertNull ("PathDiscovery=FileIOPermissionAttribute-Append", p.GetPathList (FileIOPermissionAccess.Append));
			AssertNull ("PathDiscovery=FileIOPermissionAttribute-PathDiscovery", p.GetPathList (FileIOPermissionAccess.PathDiscovery));
			AssertEquals ("PathDiscovery=FileIOPermissionAttribute-Read", FilePathUtil.GetLongPathName (filename), p.GetPathList (FileIOPermissionAccess.Read)[0]);
			AssertNull ("PathDiscovery=FileIOPermissionAttribute-Write", p.GetPathList (FileIOPermissionAccess.Write));
		}

		[Test]
		public void Write () 
		{
			FileIOPermissionAttribute attr = new FileIOPermissionAttribute (SecurityAction.Assert);
			attr.Write = Path.GetFullPath(filename);
			AssertNull ("Append=null", attr.Append);
			AssertNull ("PathDiscovery=null", attr.PathDiscovery);
			AssertNull ("Read=null", attr.Read);
			AssertEquals ("Write=Write", filename, attr.Write);
			FileIOPermission p = (FileIOPermission) attr.CreatePermission ();
			AssertNull ("PathDiscovery=FileIOPermissionAttribute-Append", p.GetPathList (FileIOPermissionAccess.Append));
			AssertNull ("PathDiscovery=FileIOPermissionAttribute-PathDiscovery", p.GetPathList (FileIOPermissionAccess.PathDiscovery));
			AssertNull ("PathDiscovery=FileIOPermissionAttribute-Read", p.GetPathList (FileIOPermissionAccess.Read));
			AssertEquals ("PathDiscovery=FileIOPermissionAttribute-Write", FilePathUtil.GetLongPathName (filename), p.GetPathList (FileIOPermissionAccess.Write)[0]);
		}
	}
}
