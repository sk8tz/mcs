//
// System.Reflection.Assembly Test Cases
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Philippe Lavoie (philippe.lavoie@cactus.ca)
//	Sebastien Pouliot (sebastien@ximian.com)
//
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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
using System.Configuration.Assemblies;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using System.Security;

namespace MonoTests.System.Reflection
{
	[TestFixture]
	public class AssemblyTest
	{
		[Test] 
		public void CreateInstance() 
		{
			Type type = typeof (AssemblyTest);
			Object obj = type.Assembly.CreateInstance ("MonoTests.System.Reflection.AssemblyTest");
			Assert.IsNotNull (obj, "#01");
			Assert.AreEqual (GetType (), obj.GetType (), "#02");
		}

		[Test] 
		public void CreateInvalidInstance() 
		{
			Type type = typeof (AssemblyTest);
			Object obj = type.Assembly.CreateInstance("NunitTests.ThisTypeDoesNotExist");
			Assert.IsNull (obj, "#03");
		}

		[Test]
#if NET_2_0
		[Category ("NotWorking")]
		[ExpectedException (typeof (ArgumentException))]
#else
		[ExpectedException (typeof (TypeLoadException))]
#endif
		public void TestGetType () 
		{
			// Bug #49114
			typeof (int).Assembly.GetType ("&blabla", true, true);
		}

		[Test]
		public void GetEntryAssembly ()
		{
			// note: only available in default appdomain
			// http://weblogs.asp.net/asanto/archive/2003/09/08/26710.aspx
			// Not sure we should emulate this behavior.
			string fname = AppDomain.CurrentDomain.FriendlyName;
			if (fname.EndsWith (".dll")) { // nunit-console
				Assert.IsNull (Assembly.GetEntryAssembly (), "GetEntryAssembly");
#if NET_2_0
				Assert.IsFalse (AppDomain.CurrentDomain.IsDefaultAppDomain (), "!default appdomain");
#endif
			} else { // gnunit
				Assert.IsNotNull (Assembly.GetEntryAssembly (), "GetEntryAssembly");
#if NET_2_0
				Assert.IsTrue (AppDomain.CurrentDomain.IsDefaultAppDomain (), "!default appdomain");
#endif
			}
		}
#if NET_2_0
		[Category ("NotWorking")]
#endif
		[Test]
		public void Corlib () 
		{
			Assembly corlib = typeof (int).Assembly;
			Assert.IsTrue (corlib.CodeBase.EndsWith ("mscorlib.dll"), "CodeBase");
			Assert.IsNull (corlib.EntryPoint, "EntryPoint");
			Assert.IsTrue (corlib.EscapedCodeBase.EndsWith ("mscorlib.dll"), "EscapedCodeBase");
			Assert.IsNotNull (corlib.Evidence, "Evidence");
			Assert.IsTrue (corlib.Location.EndsWith ("mscorlib.dll"), "Location");

			// corlib doesn't reference anything
			Assert.AreEqual (0, corlib.GetReferencedAssemblies ().Length, "GetReferencedAssemblies");
#if NET_2_0
			Assert.AreEqual ("mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", corlib.FullName, "FullName");
			// not really "true" but it's even more trusted so...
			Assert.IsTrue (corlib.GlobalAssemblyCache, "GlobalAssemblyCache");
			Assert.AreEqual (0, corlib.HostContext, "HostContext");
			Assert.AreEqual ("v2.0.50727", corlib.ImageRuntimeVersion, "ImageRuntimeVersion");
			Assert.IsFalse (corlib.ReflectionOnly, "ReflectionOnly");
			Assert.AreEqual (0x1, corlib.ManifestModule.MetadataToken);
#elif NET_1_1
			Assert.IsFalse (corlib.GlobalAssemblyCache, "GlobalAssemblyCache");
			Assert.AreEqual ("mscorlib, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", corlib.FullName, "FullName");
			Assert.AreEqual ("v1.1.4322", corlib.ImageRuntimeVersion, "ImageRuntimeVersion");
#endif
		}

		[Test]
		public void Corlib_test ()
		{
			Assembly corlib_test = Assembly.GetExecutingAssembly ();
			Assert.IsNull (corlib_test.EntryPoint, "EntryPoint");
			Assert.IsNotNull (corlib_test.Evidence, "Evidence");
			Assert.IsFalse (corlib_test.GlobalAssemblyCache, "GlobalAssemblyCache");

			Assert.IsTrue (corlib_test.GetReferencedAssemblies ().Length > 0, "GetReferencedAssemblies");
#if NET_2_0
			Assert.AreEqual (0, corlib_test.HostContext, "HostContext");
			Assert.AreEqual ("v2.0.50727", corlib_test.ImageRuntimeVersion, "ImageRuntimeVersion");
			Assert.IsNotNull (corlib_test.ManifestModule, "ManifestModule");
			Assert.IsFalse (corlib_test.ReflectionOnly, "ReflectionOnly");
#elif NET_1_1
			Assert.AreEqual ("v1.1.4322", corlib_test.ImageRuntimeVersion, "ImageRuntimeVersion");
#endif
		}

		[Test]
		public void GetAssembly ()
		{
			Assert.IsTrue (Assembly.GetAssembly (typeof (int)).FullName.StartsWith ("mscorlib"), "GetAssembly(int)");
			Assert.AreEqual (this.GetType ().Assembly.FullName, Assembly.GetAssembly (this.GetType ()).FullName, "GetAssembly(this)");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetFile_Null ()
		{
			Assembly.GetExecutingAssembly ().GetFile (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetFile_Empty ()
		{
			Assembly.GetExecutingAssembly ().GetFile (String.Empty);
		}

		[Test]
		public void GetFiles_False ()
		{
			Assembly corlib = typeof (int).Assembly;
			FileStream[] fss = corlib.GetFiles ();
			Assert.AreEqual (fss.Length, corlib.GetFiles (false).Length, "corlib.GetFiles (false)");

			Assembly corlib_test = Assembly.GetExecutingAssembly ();
			fss = corlib_test.GetFiles ();
			Assert.AreEqual (fss.Length, corlib_test.GetFiles (false).Length, "test.GetFiles (false)");
		}

		[Test]
		public void GetFiles_True ()
		{
			Assembly corlib = typeof (int).Assembly;
			FileStream[] fss = corlib.GetFiles ();
			Assert.IsTrue (fss.Length <= corlib.GetFiles (true).Length, "corlib.GetFiles (true)");

			Assembly corlib_test = Assembly.GetExecutingAssembly ();
			fss = corlib_test.GetFiles ();
			Assert.IsTrue (fss.Length <= corlib_test.GetFiles (true).Length, "test.GetFiles (true)");
		}

		[Test] // bug #78517
#if ONLY_1_1
		[Category ("NotDotNet")] // MS.NET 1.x throws FileLoadException
#endif
		public void LoadFrom_Empty_Assembly ()
		{
			string tempFile = Path.GetTempFileName ();

			try {
				Assembly.LoadFrom (tempFile);
				Assert.Fail ("#1");
			} catch (BadImageFormatException ex) {
				Assert.IsNull (ex.InnerException, "#2");
			} finally {
				File.Delete (tempFile);
			}
		}

		[Test] // bug #78517
		public void LoadFrom_Invalid_Assembly ()
		{
			string tempFile = Path.GetTempFileName ();
			using (StreamWriter sw = File.CreateText (tempFile)) {
				sw.WriteLine ("foo");
				sw.Close ();
			}

			try {
				Assembly.LoadFrom (tempFile);
				Assert.Fail ("#1");
			} catch (BadImageFormatException ex) {
				Assert.IsNull (ex.InnerException, "#2");
			} finally {
				File.Delete (tempFile);
			}
		}

		[Test]
		public void LoadFrom_NonExisting_Assembly ()
		{
			string tempFile = Path.GetTempFileName ();
			File.Delete (tempFile);

			try {
				Assembly.LoadFrom (tempFile);
				Assert.Fail ("#1");
			} catch (FileNotFoundException ex) {
				Assert.IsNull (ex.InnerException, "#2");
			} finally {
				File.Delete (tempFile);
			}
		}

		[Test]
		public void LoadWithPartialName ()
		{
			string [] names = { "corlib_test_default", "corlib_test_net_2_0", "corlib_plattest" };

			foreach (string s in names)
				if (Assembly.LoadWithPartialName (s) != null)
					return;
			Assertion.Fail ("Was not able to load any corlib test");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetObjectData_Null ()
		{
			Assembly corlib = typeof (int).Assembly;
			corlib.GetObjectData (null, new StreamingContext (StreamingContextStates.All));
		}

		[Test]
		public void GetReferencedAssemblies ()
		{
			Assembly corlib_test = Assembly.GetExecutingAssembly ();
			AssemblyName[] names = corlib_test.GetReferencedAssemblies ();
			foreach (AssemblyName an in names) {
				Assert.IsNull (an.CodeBase, "CodeBase");
				Assert.IsNotNull (an.CultureInfo, "CultureInfo");
				Assert.IsNull (an.EscapedCodeBase, "EscapedCodeBase");
				Assert.AreEqual (AssemblyNameFlags.None, an.Flags, "Flags");
				Assert.IsNotNull (an.FullName, "FullName");
				Assert.AreEqual (AssemblyHashAlgorithm.SHA1, an.HashAlgorithm, "HashAlgorithm");
				Assert.IsNull (an.KeyPair, "KeyPair");
				Assert.IsNotNull (an.Name, "Name");
				Assert.IsNotNull (an.Version, "Version");
				Assert.AreEqual (AssemblyVersionCompatibility.SameMachine, 
					an.VersionCompatibility, "VersionCompatibility");
			}
		}

		[Test]
		public void Location_Empty() {
			string assemblyFileName = Path.Combine (
				Path.GetTempPath (), "AssemblyLocation.dll");

			try {
				AssemblyName assemblyName = new AssemblyName ();
				assemblyName.Name = "AssemblyLocation";

				AssemblyBuilder ab = AppDomain.CurrentDomain
					.DefineDynamicAssembly (assemblyName,
					AssemblyBuilderAccess.Save,
					Path.GetTempPath (),
					AppDomain.CurrentDomain.Evidence);
				ab.Save (Path.GetFileName (assemblyFileName));

				using (FileStream fs = File.OpenRead (assemblyFileName)) {
					byte[] buffer = new byte[fs.Length];
					fs.Read (buffer, 0, buffer.Length);
					Assembly assembly = Assembly.Load (buffer);
					Assert.AreEqual (string.Empty, assembly.Location);
					fs.Close ();
				}
			} finally {
				File.Delete (assemblyFileName);
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void bug78464 ()
		{
			string assemblyFileName = Path.Combine (
				Path.GetTempPath (), "bug78464.dll");

			try {
				// execute test in separate appdomain to allow assembly to be unloaded
				AppDomain testDomain = CreateTestDomain ();
				CrossDomainTester crossDomainTester = CreateCrossDomainTester (testDomain);
				try {
					crossDomainTester.bug78464 (assemblyFileName);
				} finally {
					AppDomain.Unload (testDomain);
				}
			} finally {
				File.Delete (assemblyFileName);
			}
		}

		[Test]
		[Category ("NotWorking")]
		public void bug78465 ()
		{
			string assemblyFileName = Path.Combine (
				Path.GetTempPath (), "bug78465.dll");

			try {
				AssemblyName assemblyName = new AssemblyName ();
				assemblyName.Name = "bug78465";

				AssemblyBuilder ab = AppDomain.CurrentDomain
					.DefineDynamicAssembly (assemblyName,
					AssemblyBuilderAccess.Save,
					Path.GetDirectoryName (assemblyFileName),
					AppDomain.CurrentDomain.Evidence);
				ab.Save (Path.GetFileName (assemblyFileName));

				using (FileStream fs = File.OpenRead (assemblyFileName)) {
					byte[] buffer = new byte[fs.Length];
					fs.Read (buffer, 0, buffer.Length);
					Assembly assembly = Assembly.Load (buffer);
					Assert.AreEqual (string.Empty, assembly.Location, "#1");
					fs.Close ();
				}

				AppDomain testDomain = CreateTestDomain ();
				CrossDomainTester crossDomainTester = CreateCrossDomainTester (testDomain);
				try {
					crossDomainTester.bug78465 (assemblyFileName);
				} finally {
					AppDomain.Unload (testDomain);
				}
			} finally {
				File.Delete (assemblyFileName);
			}
		}

		[Test]
		public void bug78468 ()
		{
			string assemblyFileNameA = Path.Combine (Path.GetTempPath (),
				"bug78468a.dll");
			string resourceFileName = Path.Combine (Path.GetTempPath (),
				"readme.txt");

			using (StreamWriter sw = File.CreateText (resourceFileName)) {
				sw.WriteLine ("FOO");
				sw.Close ();
			}

			try {
				AssemblyName assemblyName = new AssemblyName ();
				assemblyName.Name = "bug78468a";

				AssemblyBuilder ab = AppDomain.CurrentDomain
					.DefineDynamicAssembly (assemblyName,
					AssemblyBuilderAccess.Save,
					Path.GetTempPath (),
					AppDomain.CurrentDomain.Evidence);
				ab.AddResourceFile ("read", "readme.txt");
				ab.Save (Path.GetFileName (assemblyFileNameA));

				Assembly assembly;

				using (FileStream fs = File.OpenRead (assemblyFileNameA)) {
					byte[] buffer = new byte[fs.Length];
					fs.Read (buffer, 0, buffer.Length);
					assembly = Assembly.Load (buffer);
					fs.Close ();
				}

				Assert.AreEqual (string.Empty, assembly.Location, "#A1");
				string[] resNames = assembly.GetManifestResourceNames ();
				Assert.IsNotNull (resNames, "#A2");
				Assert.AreEqual (1, resNames.Length, "#A3");
				Assert.AreEqual ("read", resNames[0], "#A4");
				ManifestResourceInfo resInfo = assembly.GetManifestResourceInfo ("read");
				Assert.IsNotNull (resInfo, "#A5");
				Assert.AreEqual ("readme.txt", resInfo.FileName, "#A6");
				Assert.IsNull (resInfo.ReferencedAssembly, "#A7");
				Assert.AreEqual ((ResourceLocation) 0, resInfo.ResourceLocation, "#A8");
#if NET_2_0
				try {
					assembly.GetManifestResourceStream ("read");
					Assert.Fail ("#A9");
				} catch (FileNotFoundException) {
				}
#else
				Assert.IsNull (assembly.GetManifestResourceStream ("read"), "#A9");
#endif
				try {
					assembly.GetFile ("readme.txt");
					Assert.Fail ("#A10");
				} catch (FileNotFoundException) {
				}

				string assemblyFileNameB = Path.Combine (Path.GetTempPath (),
					"bug78468b.dll");

				AppDomain testDomain = CreateTestDomain ();
				CrossDomainTester crossDomainTester = CreateCrossDomainTester (testDomain);
				try {
					crossDomainTester.bug78468 (assemblyFileNameB);
				} finally {
					AppDomain.Unload (testDomain);
					File.Delete (assemblyFileNameB);
				}
			} finally {
				File.Delete (assemblyFileNameA);
				File.Delete (resourceFileName);
			}
		}

#if NET_2_0
		[Test]
		[Category ("NotWorking")]
		public void ReflectionOnlyLoad ()
		{
			Assembly assembly = Assembly.ReflectionOnlyLoad (typeof (AssemblyTest).Assembly.FullName);
			
			Assert.IsNotNull (assembly);
			Assert.IsTrue (assembly.ReflectionOnly);
		}

		[Test]
		public void ReflectionOnlyLoadFrom ()
		{
			string loc = typeof (AssemblyTest).Assembly.Location;
			string filename = Path.GetFileName (loc);
			Assembly assembly = Assembly.ReflectionOnlyLoadFrom (filename);

			Assert.IsNotNull (assembly);
			Assert.IsTrue (assembly.ReflectionOnly);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CreateInstanceOnRefOnly ()
		{
			Assembly assembly = Assembly.ReflectionOnlyLoad (typeof (AssemblyTest).Assembly.FullName);
			assembly.CreateInstance ("MonoTests.System.Reflection.AssemblyTest");
		}
#endif

		private static AppDomain CreateTestDomain ()
		{
			return AppDomain.CreateDomain ("CompileFromDom", AppDomain.CurrentDomain.Evidence,
				AppDomain.CurrentDomain.SetupInformation);
		}

		private static CrossDomainTester CreateCrossDomainTester (AppDomain domain)
		{
			Type testerType = typeof (CrossDomainTester);

			return (CrossDomainTester) domain.CreateInstanceAndUnwrap (
				testerType.Assembly.FullName, testerType.FullName, false,
				BindingFlags.Public | BindingFlags.Instance, null, new object[0],
				CultureInfo.InvariantCulture, new object[0], domain.Evidence);
		}

		private class CrossDomainTester : MarshalByRefObject
		{
			public void bug78464 (string assemblyFileName)
			{
				AssemblyName assemblyName = new AssemblyName ();
				assemblyName.Name = "bug78464";

				AssemblyBuilder ab = AppDomain.CurrentDomain
					.DefineDynamicAssembly (assemblyName,
					AssemblyBuilderAccess.Save,
					Path.GetDirectoryName (assemblyFileName),
					AppDomain.CurrentDomain.Evidence);
				ab.Save (Path.GetFileName (assemblyFileName));

				Assembly assembly;

				using (FileStream fs = File.OpenRead (assemblyFileName)) {
					byte[] buffer = new byte[fs.Length];
					fs.Read (buffer, 0, buffer.Length);
					assembly = Assembly.Load (buffer);
					fs.Close ();
				}

				Assert.AreEqual (string.Empty, assembly.Location, "#1");

				assembly = Assembly.LoadFrom (assemblyFileName, AppDomain.CurrentDomain.Evidence);
				Assert.IsFalse (assembly.Location == string.Empty, "#2");
				Assert.AreEqual (Path.GetFileName (assemblyFileName), Path.GetFileName(assembly.Location), "#3");
				// note: we cannot check if directory names match, as MS.NET seems to 
				// convert directory part of assembly location to lowercase
				Assert.IsFalse (Path.GetDirectoryName(assembly.Location) == string.Empty, "#4");
			}

			public void bug78465 (string assemblyFileName)
			{
				Assembly assembly = Assembly.LoadFrom (assemblyFileName, AppDomain.CurrentDomain.Evidence);
				Assert.IsFalse (assembly.Location == string.Empty, "#2");
				Assert.AreEqual (Path.GetFileName (assemblyFileName), Path.GetFileName (assembly.Location), "#3");
				// note: we cannot check if directory names match, as MS.NET seems to 
				// convert directory part of assembly location to lowercase
				Assert.IsFalse (Path.GetDirectoryName (assembly.Location) == string.Empty, "#4");
			}

			public void bug78468 (string assemblyFileName)
			{
				AssemblyName assemblyName = new AssemblyName ();
				assemblyName.Name = "bug78468b";

				AssemblyBuilder ab = AppDomain.CurrentDomain
					.DefineDynamicAssembly (assemblyName,
					AssemblyBuilderAccess.Save,
					Path.GetDirectoryName (assemblyFileName),
					AppDomain.CurrentDomain.Evidence);
				ab.AddResourceFile ("read", "readme.txt");
				ab.Save (Path.GetFileName (assemblyFileName));

				Assembly assembly = Assembly.LoadFrom (assemblyFileName, AppDomain.CurrentDomain.Evidence);
				Assert.IsTrue (assembly.Location != string.Empty, "#B1");
				string[] resNames = assembly.GetManifestResourceNames ();
				Assert.IsNotNull (resNames, "#B2");
				Assert.AreEqual (1, resNames.Length, "#B3");
				Assert.AreEqual ("read", resNames[0], "#B4");
				ManifestResourceInfo resInfo = assembly.GetManifestResourceInfo ("read");
				Assert.IsNotNull (resInfo, "#B5");
				Assert.AreEqual ("readme.txt", resInfo.FileName, "#B6");
				Assert.IsNull (resInfo.ReferencedAssembly, "#B7");
				Assert.AreEqual ((ResourceLocation) 0, resInfo.ResourceLocation, "#B8");
				Stream s = assembly.GetManifestResourceStream ("read");
				Assert.IsNotNull (s, "#B9");
				s.Close ();
				s = assembly.GetFile ("readme.txt");
				Assert.IsNotNull (s, "#B10");
				s.Close ();
			}
		}
	}
}
