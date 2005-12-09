//
// AppDomainSetupTest.cs - NUnit Test Cases for the System.AppDomainSetup class
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//      Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
// 

using NUnit.Framework;
using System;
using System.IO;

namespace MonoTests.System
{
	[TestFixture]
	public class AppDomainSetupTest {

		static readonly string tmpPath = Path.GetTempPath ();
		static readonly string curDir = Directory.GetCurrentDirectory ();

		private bool RunningOnWindows {
			get {
				int os = (int)Environment.OSVersion.Platform;
#if NET_2_0
				return (os != 4);
#else
				return (os != 128);
#endif
			}
		}

		[Test]
		public void ApplicationBase1 ()
		{
			string expected_path = tmpPath.Replace(@"\", @"/");
			AppDomainSetup setup = new AppDomainSetup ();
			string fileUri = "file://" + expected_path;
			setup.ApplicationBase = fileUri;
			// with MS 1.1 SP1 the expected_path starts with "//" but this make
			// sense only under Windows (i.e. reversed \\ for local files)
			if (RunningOnWindows)
				expected_path = "//" + expected_path;
#if NET_2_0
			try {
				// under 2.0 the NotSupportedException is throw when getting 
				// (and not setting) the ApplicationBase property
				Assert.AreEqual (expected_path, setup.ApplicationBase);
			}
			catch (NotSupportedException) {
				// however the path is invalid only on Windows
				if (!RunningOnWindows)
					throw;
			}
#else
			Assert.AreEqual (expected_path, setup.ApplicationBase);
#endif
		}

		[Test]
		public void ApplicationBase2 ()
		{
			AppDomainSetup setup = new AppDomainSetup ();
			setup.ApplicationBase = curDir;
			Assert.AreEqual (curDir, setup.ApplicationBase);
		}

		[Test]
		public void ApplicationBase3 ()
		{
			AppDomainSetup setup = new AppDomainSetup ();
			string expected = Path.Combine (Environment.CurrentDirectory, "lalala");
			setup.ApplicationBase = "lalala";
			Assert.AreEqual (expected, setup.ApplicationBase);
		}

		[Test]
		public void ApplicationBase4 ()
		{
			AppDomainSetup setup = new AppDomainSetup ();
			setup.ApplicationBase = "lala:la";
#if NET_2_0
			try {
				// under 2.0 the NotSupportedException is throw when getting 
				// (and not setting) the ApplicationBase property
				Assert.AreEqual (Path.GetFullPath ("lala:la"), setup.ApplicationBase);
			}
			catch (NotSupportedException) {
				// however the path is invalid only on Windows
				// (same exceptions as Path.GetFullPath)
				if (!RunningOnWindows)
					throw;
			}
#else
			// under 1.x a "bad" path containing ":" will be returned "as-is"
			// but the name is legal for linux so we return a full path
			if (RunningOnWindows)
				Assert.AreEqual ("lala:la", setup.ApplicationBase);
			else
				Assert.AreEqual (Path.GetFullPath ("lala:la"), setup.ApplicationBase);
#endif
		}

		[Test]
		public void ApplicationBase5 ()
		{
			// This is failing because of (probably) a windows-ism, so don't worry
			AppDomainSetup setup = new AppDomainSetup ();
			setup.ApplicationBase = "file:///lala:la";
#if NET_2_0
			try {
				// under 2.0 the NotSupportedException is throw when getting 
				// (and not setting) the ApplicationBase property
				Assert.AreEqual ("/lala:la", setup.ApplicationBase);
			}
			catch (NotSupportedException) {
				// however the path is invalid only on Windows
				// (same exceptions as Path.GetFullPath)
				if (!RunningOnWindows)
					throw;
			}
#else
			// under 1.x a "bad" path containing ":" will be returned "as-is"
			// but the name is legal for linux so we return a full path
			if (RunningOnWindows)
				Assert.AreEqual ("lala:la", setup.ApplicationBase);
			else
				Assert.AreEqual ("/lala:la", setup.ApplicationBase);
#endif
		}

		[Test]
		public void ApplicationBase6 ()
		{
			AppDomainSetup setup = new AppDomainSetup ();
			setup.ApplicationBase = "la?lala";
			// paths containing "?" are *always* bad on Windows
			// but are legal for linux so we return a full path
			if (RunningOnWindows) {
				try {
					// ArgumentException is throw when getting 
					// (and not setting) the ApplicationBase property
					Assert.Fail ("setup.ApplicationBase returned :" + setup.ApplicationBase);
				}
				catch (ArgumentException) {
				}
				catch (Exception e) {
					Assert.Fail ("Unexpected exception: " + e.ToString ());
				}
			} else {
				Assert.AreEqual (Path.GetFullPath ("la?lala"), setup.ApplicationBase);
			}
		}
	}
}
