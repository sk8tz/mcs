//
// ModuleBuilderTest - NUnit Test Cases for the ModuleBuilder class
//
// Zoltan Varga (vargaz@freemail.hu)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
//


using System;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using System.Collections;
using System.Diagnostics.SymbolStore;
using System.Runtime.InteropServices;

using NUnit.Framework;

namespace MonoTests.System.Reflection.Emit
{
	[TestFixture]
	public class ModuleBuilderTest
	{
		static string TempFolder = Path.Combine (Path.GetTempPath (), "MT.S.R.E.MBT");

		[SetUp]
		public void SetUp ()
		{
			Random AutoRand = new Random ();
			string TempPath = TempFolder;
			while (Directory.Exists (TempFolder))
				TempFolder = Path.Combine (TempPath, AutoRand.Next ().ToString ());
			Directory.CreateDirectory (TempFolder);
		}

		[TearDown]
		public void TearDown ()
		{
			try {
				// This throws an exception under MS.NET, since the directory contains loaded
				// assemblies.
				Directory.Delete (TempFolder, true);
			} catch (Exception) {
			}
		}

		[Test]
		public void TestIsTransient ()
		{
			AssemblyName assemblyName = new AssemblyName ();
			assemblyName.Name = "foo";

			AssemblyBuilder ab
				= Thread.GetDomain ().DefineDynamicAssembly (
					assemblyName, AssemblyBuilderAccess.RunAndSave, TempFolder);
			ModuleBuilder mb1 = ab.DefineDynamicModule ("foo.dll");
			Assert.IsTrue (mb1.IsTransient (), "#1");
			ModuleBuilder mb2 = ab.DefineDynamicModule ("foo2.dll", "foo2.dll");
			Assert.IsFalse (mb2.IsTransient (), "#2");
		}

		// Some of these tests overlap with the tests for Module

		[Test]
		public void TestGlobalData ()
		{

			AssemblyName assemblyName = new AssemblyName ();
			assemblyName.Name = "foo";

			AssemblyBuilder ab
				= Thread.GetDomain ().DefineDynamicAssembly (
					assemblyName, AssemblyBuilderAccess.RunAndSave, TempFolder);

			string resfile = Path.Combine (TempFolder, "res");
			using (StreamWriter sw = new StreamWriter (resfile)) {
				sw.WriteLine ("FOO");
			}

			ModuleBuilder mb = ab.DefineDynamicModule ("foo.dll", "foo.dll");

			mb.DefineInitializedData ("DATA", new byte [100], FieldAttributes.Public);
			mb.DefineInitializedData ("DATA2", new byte [100], FieldAttributes.Public);
			mb.DefineInitializedData ("DATA3", new byte [99], FieldAttributes.Public);
			mb.DefineUninitializedData ("DATA4", 101, FieldAttributes.Public);
			mb.DefineInitializedData ("DATA_PRIVATE", new byte [100], 0);
			mb.CreateGlobalFunctions ();

			ab.Save ("foo.dll");

			Assembly assembly = Assembly.LoadFrom (Path.Combine (TempFolder, "foo.dll"));

			Module module = assembly.GetLoadedModules () [0];

			string [] expectedFieldNames = new string [] {
				"DATA", "DATA2", "DATA3", "DATA4" };
			ArrayList fieldNames = new ArrayList ();
			foreach (FieldInfo fi in module.GetFields ()) {
				fieldNames.Add (fi.Name);
			}
			AssertArrayEqualsSorted (expectedFieldNames, fieldNames.ToArray (typeof (string)));

			Assert.IsNotNull (module.GetField ("DATA"), "#1");
			Assert.IsNotNull (module.GetField ("DATA2"), "#2");
			Assert.IsNotNull (module.GetField ("DATA3"), "#3");
			Assert.IsNotNull (module.GetField ("DATA4"), "#4");
			Assert.IsNull (module.GetField ("DATA_PRIVATE"), "#5");
			Assert.IsNotNull (module.GetField ("DATA_PRIVATE", BindingFlags.NonPublic | BindingFlags.Static), "#6");
		}

		[Test]
		public void TestGlobalMethods ()
		{
			AssemblyName an = new AssemblyName ();
			an.Name = "TestGlobalMethods";
			AssemblyBuilder builder =
				AppDomain.CurrentDomain.DefineDynamicAssembly (an, AssemblyBuilderAccess.Run);
			ModuleBuilder module = builder.DefineDynamicModule ("MessageModule");
			MethodBuilder method = module.DefinePInvokeMethod ("printf", "libc.so",
															  MethodAttributes.PinvokeImpl | MethodAttributes.Static | MethodAttributes.Public,
															  CallingConventions.Standard, typeof (void), new Type [] { typeof (string) }, CallingConvention.Winapi,
															  CharSet.Auto);
			method.SetImplementationFlags (MethodImplAttributes.PreserveSig |
										   method.GetMethodImplementationFlags ());
			module.CreateGlobalFunctions ();

			Assert.IsNotNull (module.GetMethod ("printf"));
		}

		[Test]
		public void DuplicateSymbolDocument ()
		{
			AssemblyName assemblyName = new AssemblyName ();
			assemblyName.Name = "ModuleBuilderTest.DuplicateSymbolDocument";

			AssemblyBuilder ab
				= Thread.GetDomain ().DefineDynamicAssembly (
					assemblyName, AssemblyBuilderAccess.RunAndSave, TempFolder);

			ModuleBuilder mb = ab.DefineDynamicModule ("foo.dll", "foo.dll", true);

			// Check that it is possible to redefine a symbol document
			ISymbolDocumentWriter doc1 =
				mb.DefineDocument ("foo.il", SymDocumentType.Text,
								  SymLanguageType.ILAssembly, SymLanguageVendor.Microsoft);
			ISymbolDocumentWriter doc2 =
				mb.DefineDocument ("foo.il", SymDocumentType.Text,
								  SymLanguageType.ILAssembly, SymLanguageVendor.Microsoft);
		}

		[Test] // Test case for #80435.
		public void GetArrayMethodToStringTest ()
		{
			AssemblyName name = new AssemblyName ();
			name.Name = "a";
			AssemblyBuilder assembly = AppDomain.CurrentDomain.DefineDynamicAssembly (name, AssemblyBuilderAccess.RunAndSave);

			ModuleBuilder module = assembly.DefineDynamicModule ("m", "test.dll");

			Type [] myArrayClass = new Type [1];
			Type [] parameterTypes = { typeof (Array) };
			MethodInfo myMethodInfo = module.GetArrayMethod (myArrayClass.GetType (), "Sort", CallingConventions.Standard, null, parameterTypes);

			string str = myMethodInfo.ToString ();
			Assert.IsNotNull (str);
			// Don't compare string, since MS returns System.Reflection.Emit.SymbolMethod here 
			// (they do not provide an implementation of ToString).
		}

		private static void AssertArrayEqualsSorted (Array o1, Array o2)
		{
			Array s1 = (Array) o1.Clone ();
			Array s2 = (Array) o2.Clone ();

			Array.Sort (s1);
			Array.Sort (s2);

			Assert.AreEqual (s1.Length, s2.Length, "#1");
			for (int i = 0; i < s1.Length; ++i)
				Assert.AreEqual (s1.GetValue (i), s2.GetValue (i), "#2: " + i);
		}
	}
}
