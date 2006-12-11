//
// ImportTest.cs
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2006 Marek Sieradzki
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

using System;
using System.Collections;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.BuildEngine {
	[TestFixture]
	public class ImportTest {
		
		Engine			engine;
		Project			project;
		
		[Test]
		public void TestAdd1 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Import Project='Test/resources/Import.csproj'/>
                                </Project>
                        ";

                        engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			Import[] t = new Import [1];
			project.Imports.CopyTo (t, 0);

			Assert.IsNull (t [0].Condition, "A1");
			Assert.AreEqual (false, t [0].IsImported, "A2");
			Assert.AreEqual ("Test/resources/Import.csproj", t [0].ProjectPath, "A3");
			Assert.IsNotNull (t [0].EvaluatedProjectPath, "A4");
			// FIXME: why there is no IsNotEmpty () in Mono NUnit?
			Assert.IsTrue (String.Empty != t [0].EvaluatedProjectPath, "A5");
		}

		[Test]
		[ExpectedException (typeof (InvalidProjectFileException))]
		public void TestAdd2 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Import Project=''/>
                                </Project>
                        ";

                        engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);
		}

		[Test]
		[Category ("NotWorking")]
		public void TestAdd3 ()
		{
                        string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Import Project='Test/resources/SelfImport.csproj'/>
                                </Project>
                        ";

                        engine = new Engine (Consts.BinPath);

                        project = engine.CreateNewProject ();
                        project.LoadXml (documentString);

			Assert.AreEqual (1, project.Imports.Count, "A1");
		}

		[Test]
		public void TestItems1 ()
		{
			string documentString = @"
                                <Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
					<Import Project='Test/resources/Items.csproj'/>
                                </Project>
                        ";

			engine = new Engine (Consts.BinPath);

			project = engine.CreateNewProject ();
			project.LoadXml (documentString);

			BuildItemGroup [] groups = new BuildItemGroup [1];
			project.ItemGroups.CopyTo (groups, 0);

			Assert.IsTrue (groups [0].IsImported, "A1");
			Assert.AreEqual (1, groups [0].Count, "A2");
		}
	}
}
