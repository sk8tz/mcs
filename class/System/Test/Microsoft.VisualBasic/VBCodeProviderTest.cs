//
// Microsoft.VisualBasic.VBCodeProvider.cs
//
// Author:
//   Jochen Wezel (jwezel@compumaster.de)
//
// (C) 2003 Jochen Wezel (CompuMaster GmbH)
//
// Last modifications:
// 2003-12-10 JW: publishing of this file
//

using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Specialized;
using System.Globalization;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualBasic;
using NUnit.Framework;

namespace MonoTests.Microsoft.VisualBasic
{
	enum OsType
	{
		Windows,
		Unix,
		Mac
	}

	[TestFixture]
	public class VBCodeProviderTest
	{
		private string _tempDir;
		private CodeDomProvider _codeProvider;
		private static OsType OS;
		private static char DSC = Path.DirectorySeparatorChar;

		private static readonly string _sourceTest1 = "Public Class Test1" +
			Environment.NewLine + "End Class";
		private static readonly string _sourceTest2 = "Public Class Test2" +
			Environment.NewLine + "End Class";

		[SetUp]
		public void GetReady ()
		{
			if ('/' == DSC) {
				OS = OsType.Unix;
			} else if ('\\' == DSC) {
				OS = OsType.Windows;
			} else {
				OS = OsType.Mac;
			}

			_codeProvider = new VBCodeProvider ();
			_tempDir = CreateTempDirectory ();
		}

		[TearDown]
		public void TearDown ()
		{
			RemoveDirectory (_tempDir);
		}

		[Test]
		public void FileExtension ()
		{
			Assert.AreEqual ("vb", _codeProvider.FileExtension, "#JW10");
		}

		[Test]
		public void LanguageOptionsTest ()
		{
			Assert.AreEqual (LanguageOptions.CaseInsensitive, _codeProvider.LanguageOptions, "#JW20");
		}

		[Test]
		public void GeneratorSupports ()
		{
			ICodeGenerator codeGenerator = _codeProvider.CreateGenerator ();
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.DeclareEnums), "#1");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.ArraysOfArrays), "#2");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.AssemblyAttributes), "#3");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.ChainedConstructorArguments), "#4");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.ComplexExpressions), "#5");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.DeclareDelegates), "#6");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.DeclareEnums), "#7");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.DeclareEvents), "#8");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.DeclareInterfaces), "#9");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.DeclareValueTypes), "#10");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.EntryPointMethod), "#11");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.GotoStatements), "#12");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.MultidimensionalArrays), "#13");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.MultipleInterfaceMembers), "#14");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.NestedTypes), "#15");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.ParameterAttributes), "#16");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.PublicStaticMembers), "#17");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.ReferenceParameters), "#18");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.ReturnTypeAttributes), "#19");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.StaticConstructors), "#20");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.TryCatchStatements), "#21");
			Assert.IsTrue (codeGenerator.Supports (GeneratorSupport.Win32Resources), "#22");
		}

		[Test]
		public void CreateCompiler ()
		{
			// Prepare the compilation
			ICodeCompiler codeCompiler = _codeProvider.CreateCompiler ();
			Assert.IsNotNull (codeCompiler, "#JW30 - CreateCompiler");

			CompilerParameters options = new CompilerParameters ();
			options.GenerateExecutable = true;
			options.IncludeDebugInformation = true;
			options.TreatWarningsAsErrors = true;

			// process compilation
			CompilerResults compilerResults = codeCompiler.CompileAssemblyFromSource (options,
				"public class TestModule" + Environment.NewLine + "public shared sub Main()"
				+ Environment.NewLine + "System.Console.Write(\"Hello world!\")"
				+ Environment.NewLine + "End Sub" + Environment.NewLine + "End Class");

			// Analyse the compilation success/messages
			StringCollection MyOutput = compilerResults.Output;
			string MyOutStr = "";
			foreach (string MyStr in MyOutput) {
				MyOutStr += MyStr + Environment.NewLine + Environment.NewLine;
			}

			if (compilerResults.Errors.Count != 0) {
				Assert.Fail ("#JW31 - Hello world compilation: " + MyOutStr);
			}

			try {
				Assembly MyAss = compilerResults.CompiledAssembly;
			} catch (Exception ex) {
				Assert.Fail ("#JW32 - compilerResults.CompiledAssembly hasn't been an expected object" +
						Environment.NewLine + ex.Message + Environment.NewLine + ex.StackTrace);
			}

			// Execute the test app
			ProcessStartInfo NewProcInfo = new ProcessStartInfo ();
			if (Windows) {
				NewProcInfo.FileName = compilerResults.CompiledAssembly.Location;
			} else {
				NewProcInfo.FileName = "mono";
				NewProcInfo.Arguments = compilerResults.CompiledAssembly.Location;
			}
			NewProcInfo.RedirectStandardOutput = true;
			NewProcInfo.UseShellExecute = false;
			NewProcInfo.CreateNoWindow = true;
			string TestAppOutput = "";
			try {
				Process MyProc = Process.Start (NewProcInfo);
				MyProc.WaitForExit ();
				TestAppOutput = MyProc.StandardOutput.ReadToEnd ();
				MyProc.Close ();
				MyProc.Dispose ();
			} catch (Exception ex) {
				Assert.Fail ("#JW34 - " + ex.Message + Environment.NewLine + ex.StackTrace);
			}
			Assert.AreEqual ("Hello world!", TestAppOutput, "#JW33 - Application output");

			// Clean up
			try {
				File.Delete (NewProcInfo.FileName);
			} catch { }
		}

		[Test]
		public void CreateGenerator ()
		{
			ICodeGenerator MyVBCodeGen;
			MyVBCodeGen = _codeProvider.CreateGenerator ();
			Assert.IsNotNull (MyVBCodeGen, "#JW40 - CreateGenerator");
			Assert.IsTrue (MyVBCodeGen.Supports (GeneratorSupport.DeclareEnums), "#JW41");
		}

		[Test]
		public void CompileFromFile_InMemory ()
		{
			// create vb source file
			string sourceFile = Path.Combine (_tempDir, "file." + _codeProvider.FileExtension);
			using (FileStream f = new FileStream (sourceFile, FileMode.Create)) {
				using (StreamWriter s = new StreamWriter (f)) {
					s.Write (_sourceTest1);
					s.Close ();
				}
				f.Close ();
			}

			CompilerParameters options = new CompilerParameters ();
			options.GenerateExecutable = false;
			options.GenerateInMemory = true;
			options.TempFiles = new TempFileCollection (_tempDir);

			ICodeCompiler compiler = _codeProvider.CreateCompiler ();
			CompilerResults results = compiler.CompileAssemblyFromFile (options,
				sourceFile);

			Assert.AreEqual (string.Empty, results.CompiledAssembly.Location, "#1");
			Assert.IsNull (results.PathToAssembly, "#2");
			Assert.IsNotNull (results.CompiledAssembly.GetType ("Test1"), "#3");

			// verify we don't cleanup files in temp directory too agressively
			string[] tempFiles = Directory.GetFiles (_tempDir);
			Assert.AreEqual (1, tempFiles.Length, "#4");
			Assert.AreEqual (sourceFile, tempFiles[0], "#5");

		}

		[Test]
		public void CompileFromFileBatch_InMemory ()
		{
			// create vb source file
			string sourceFile1 = Path.Combine (_tempDir, "file." + _codeProvider.FileExtension);
			using (FileStream f = new FileStream (sourceFile1, FileMode.Create)) {
				using (StreamWriter s = new StreamWriter (f)) {
					s.Write (_sourceTest1);
					s.Close ();
				}
				f.Close ();
			}

			string sourceFile2 = Path.Combine (_tempDir, "file2." + _codeProvider.FileExtension);
			using (FileStream f = new FileStream (sourceFile2, FileMode.Create)) {
				using (StreamWriter s = new StreamWriter (f)) {
					s.Write (_sourceTest2);
					s.Close ();
				}
				f.Close ();
			}

			CompilerParameters options = new CompilerParameters ();
			options.GenerateExecutable = false;
			options.GenerateInMemory = true;
			options.TempFiles = new TempFileCollection (_tempDir);

			ICodeCompiler compiler = _codeProvider.CreateCompiler ();
			CompilerResults results = compiler.CompileAssemblyFromFileBatch (options,
				new string[] { sourceFile1, sourceFile2 });

			Assert.AreEqual (string.Empty, results.CompiledAssembly.Location, "#1");
			Assert.IsNull (results.PathToAssembly, "#2");

			Assert.IsNotNull (results.CompiledAssembly.GetType ("Test1"), "#3");
			Assert.IsNotNull (results.CompiledAssembly.GetType ("Test2"), "#4");

			// verify we don't cleanup files in temp directory too agressively
			string[] tempFiles = Directory.GetFiles (_tempDir);
			Assert.AreEqual (2, tempFiles.Length, "#5");
			Assert.IsTrue (File.Exists (sourceFile1), "#6");
			Assert.IsTrue (File.Exists (sourceFile2), "#7");
		}

		[Test]
		public void CompileFromSource_InMemory ()
		{
			// create a file in temp directory to ensure that compiler is not removing
			// too much (temporary) files
			string tempFile = Path.Combine (_tempDir, "file." + _codeProvider.FileExtension);
			using (FileStream fs = File.Create (tempFile)) {
				fs.Close ();
			}

			CompilerParameters options = new CompilerParameters ();
			options.GenerateExecutable = false;
			options.GenerateInMemory = true;
			options.TempFiles = new TempFileCollection (_tempDir);

			ICodeCompiler compiler = _codeProvider.CreateCompiler ();
			CompilerResults results = compiler.CompileAssemblyFromSource (options,
				_sourceTest1);

			Assert.AreEqual (string.Empty, results.CompiledAssembly.Location, "#1");
			Assert.IsNull (results.PathToAssembly, "#2");
			Assert.IsNotNull (results.CompiledAssembly.GetType ("Test1"), "#3");

			// verify we don't cleanup files in temp directory too agressively
			string[] tempFiles = Directory.GetFiles (_tempDir);
			Assert.AreEqual (1, tempFiles.Length, "#4");
			Assert.AreEqual (tempFile, tempFiles[0], "#5");
		}

		[Test]
		public void CompileFromSourceBatch_InMemory ()
		{
			// create a file in temp directory to ensure that compiler is not removing
			// too much (temporary) files
			string tempFile = Path.Combine (_tempDir, "file." + _codeProvider.FileExtension);
			using (FileStream fs = File.Create (tempFile)) {
				fs.Close ();
			}

			CompilerParameters options = new CompilerParameters ();
			options.GenerateExecutable = false;
			options.GenerateInMemory = true;
			options.TempFiles = new TempFileCollection (_tempDir);

			ICodeCompiler compiler = _codeProvider.CreateCompiler ();
			CompilerResults results = compiler.CompileAssemblyFromSourceBatch (options,
				new string[] { _sourceTest1, _sourceTest2 });

			Assert.AreEqual (string.Empty, results.CompiledAssembly.Location, "#1");
			Assert.IsNull (results.PathToAssembly, "#2");

			Assert.IsNotNull (results.CompiledAssembly.GetType ("Test1"), "#3");
			Assert.IsNotNull (results.CompiledAssembly.GetType ("Test2"), "#4");

			// verify we don't cleanup files in temp directory too agressively
			string[] tempFiles = Directory.GetFiles (_tempDir);
			Assert.AreEqual (1, tempFiles.Length, "#5");
			Assert.AreEqual (tempFile, tempFiles[0], "#6");
		}

		[Test]
		public void CompileFromDom_NotInMemory ()
		{
			// create a file in temp directory to ensure that compiler is not removing
			// too much (temporary) files
			string tempFile = Path.Combine (_tempDir, "file." + _codeProvider.FileExtension);
			using (FileStream fs = File.Create (tempFile)) {
				fs.Close ();
			}

			// compile and verify result in separate appdomain to avoid file locks
			AppDomain testDomain = CreateTestDomain ();
			CrossDomainTester compileTester = CreateCrossDomainTester (testDomain);

			string outputAssembly = null;

			try {
				outputAssembly = compileTester.CompileAssemblyFromDom (_tempDir);
			} finally {
				AppDomain.Unload (testDomain);
			}

			// there should be two files in temp dir: temp file and output assembly
			string[] tempFiles = Directory.GetFiles (_tempDir);
			Assert.AreEqual (2, tempFiles.Length, "#1");
			Assert.IsTrue (File.Exists (outputAssembly), "#2");
			Assert.IsTrue (File.Exists (tempFile), "#3");
		}

		[Test]
		public void CompileFromDomBatch_NotInMemory ()
		{
			// create a file in temp directory to ensure that compiler is not removing
			// too much (temporary) files
			string tempFile = Path.Combine (_tempDir, "file." + _codeProvider.FileExtension);
			using (FileStream fs = File.Create (tempFile)) {
				fs.Close ();
			}

			// compile and verify result in separate appdomain to avoid file locks
			AppDomain testDomain = CreateTestDomain ();
			CrossDomainTester compileTester = CreateCrossDomainTester (testDomain);

			string outputAssembly = null;

			try {
				outputAssembly = compileTester.CompileAssemblyFromDomBatch (_tempDir);
			} finally {
				AppDomain.Unload (testDomain);
			}

			// there should be two files in temp dir: temp file and output assembly
			string[] tempFiles = Directory.GetFiles (_tempDir);
			Assert.AreEqual (2, tempFiles.Length, "#1");
			Assert.IsTrue (File.Exists (outputAssembly), "#2");
			Assert.IsTrue (File.Exists (tempFile), "#3");
		}

		[Test]
		public void CompileFromDom_InMemory ()
		{
			// create a file in temp directory to ensure that compiler is not removing
			// too much (temporary) files
			string tempFile = Path.Combine (_tempDir, "file." + _codeProvider.FileExtension);
			using (FileStream fs = File.Create (tempFile)) {
				fs.Close ();
			}

			CompilerParameters options = new CompilerParameters ();
			options.GenerateExecutable = false;
			options.GenerateInMemory = true;
			options.TempFiles = new TempFileCollection (_tempDir);

			ICodeCompiler compiler = _codeProvider.CreateCompiler ();
			CompilerResults results = compiler.CompileAssemblyFromDom (options, new CodeCompileUnit ());

			Assert.AreEqual (string.Empty, results.CompiledAssembly.Location, "#1");
			Assert.IsNull (results.PathToAssembly, "#2");

			// verify we don't cleanup files in temp directory too agressively
			string[] tempFiles = Directory.GetFiles (_tempDir);
			Assert.AreEqual (1, tempFiles.Length, "#3");
			Assert.AreEqual (tempFile, tempFiles[0], "#4");
		}

		[Test]
		public void CompileFromDomBatch_InMemory ()
		{
			// create a file in temp directory to ensure that compiler is not removing
			// too much (temporary) files
			string tempFile = Path.Combine (_tempDir, "file." + _codeProvider.FileExtension);
			using (FileStream fs = File.Create (tempFile)) {
				fs.Close ();
			}

			CompilerParameters options = new CompilerParameters ();
			options.GenerateExecutable = false;
			options.GenerateInMemory = true;
			options.TempFiles = new TempFileCollection (_tempDir);

			ICodeCompiler compiler = _codeProvider.CreateCompiler ();
			CompilerResults results = compiler.CompileAssemblyFromDomBatch (options,
				new CodeCompileUnit[] { new CodeCompileUnit (), new CodeCompileUnit () });

			Assert.AreEqual (string.Empty, results.CompiledAssembly.Location, "#1");
			Assert.IsNull (results.PathToAssembly, "#2");

			// verify we don't cleanup files in temp directory too agressively
			string[] tempFiles = Directory.GetFiles (_tempDir);
			Assert.AreEqual (1, tempFiles.Length, "#3");
			Assert.AreEqual (tempFile, tempFiles[0], "#4");
		}

		//TODO: [Test]
		public void CreateParser ()
		{
			//System.CodeDom.Compiler.ICodeParser CreateParser()
		}

		//TODO: [Test]
		public void CreateObjRef ()
		{
			//System.Runtime.Remoting.ObjRef CreateObjRef(System.Type requestedType)
		}

		bool Windows
		{
			get {
				return OS == OsType.Windows;
			}
		}

		bool Unix
		{
			get {
				return OS == OsType.Unix;
			}
		}

		bool Mac
		{
			get {
				return OS == OsType.Mac;
			}
		}

		private static string CreateTempDirectory ()
		{
			// create a uniquely named zero-byte file
			string tempFile = Path.GetTempFileName ();
			// remove the temporary file
			File.Delete (tempFile);
			// create a directory named after the unique temporary file
			Directory.CreateDirectory (tempFile);
			// return the path to the temporary directory
			return tempFile;
		}

		private static void RemoveDirectory (string path)
		{
			try {
				if (Directory.Exists (path)) {
					string[] directoryNames = Directory.GetDirectories (path);
					foreach (string directoryName in directoryNames) {
						RemoveDirectory (directoryName);
					}
					string[] fileNames = Directory.GetFiles (path);
					foreach (string fileName in fileNames) {
						File.Delete (fileName);
					}
					Directory.Delete (path, true);
				}
			} catch (Exception ex) {
				throw new AssertionException ("Unable to cleanup '" + path + "'.", ex);
			}
		}

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
			public string CompileAssemblyFromDom (string tempDir)
			{
				CompilerParameters options = new CompilerParameters ();
				options.GenerateExecutable = false;
				options.GenerateInMemory = false;
				options.TempFiles = new TempFileCollection (tempDir);

				VBCodeProvider codeProvider = new VBCodeProvider ();
				ICodeCompiler compiler = codeProvider.CreateCompiler ();
				CompilerResults results = compiler.CompileAssemblyFromDom (options, new CodeCompileUnit ());

				Assert.IsTrue (results.CompiledAssembly.Location.Length != 0,
					"Location should not be empty string");
				Assert.IsNotNull (results.PathToAssembly, "PathToAssembly should not be null");

				return results.PathToAssembly;
			}

			public string CompileAssemblyFromDomBatch (string tempDir)
			{
				CompilerParameters options = new CompilerParameters ();
				options.GenerateExecutable = false;
				options.GenerateInMemory = false;
				options.TempFiles = new TempFileCollection (tempDir);

				VBCodeProvider codeProvider = new VBCodeProvider ();
				ICodeCompiler compiler = codeProvider.CreateCompiler ();
				CompilerResults results = compiler.CompileAssemblyFromDomBatch (options, new CodeCompileUnit[] { new CodeCompileUnit (), new CodeCompileUnit () });

				Assert.IsTrue (results.CompiledAssembly.Location.Length != 0,
					"Location should not be empty string");
				Assert.IsNotNull (results.PathToAssembly, "PathToAssembly should not be null");

				return results.PathToAssembly;
			}
		}
	}
}
