//
// BuildManagerTest.cs
//
// Author:
//   Atsushi Enomoto (atsushi@xamarin.com)
//
// Copyright (C) 2013 Xamarin Inc. (http://www.xamarin.com)
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
using System;
using System.IO;
using System.Xml;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

namespace MonoTests.Microsoft.Build.Execution
{
	[TestFixture]
	public class BuildManagerTest
	{
		Project GetDummyProject ()
		{
			string empty_project_xml = "<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";
			var path = "file://localhost/foo.xml";
			var xml = XmlReader.Create (new StringReader (empty_project_xml), null, path);
			var root = ProjectRootElement.Create (xml);
			return new Project (root);
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetProjectInstanceForBuildNullFullPath ()
		{
			var manager = new BuildManager ();
			manager.GetProjectInstanceForBuild (GetDummyProject ());
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetProjectInstanceForBuildEmptyFullPath ()
		{
			var proj = GetDummyProject ();
			proj.FullPath = "";
			var manager = new BuildManager ();
			manager.GetProjectInstanceForBuild (proj);
		}
		
		[Test]
		public void GetProjectInstanceForBuild ()
		{
            string empty_project_xml = "<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";
            var path = "file://localhost/foo.xml";
            var xml = XmlReader.Create (new StringReader(empty_project_xml), null, path);
            var root = ProjectRootElement.Create (xml);
            root.FullPath = path;
            var proj = new Project (root);
            var manager = new BuildManager ();
            var inst = manager.GetProjectInstanceForBuild (proj);
            Assert.AreEqual (inst, manager.GetProjectInstanceForBuild (proj), "#1");
		}
		
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void PendBuildRequestBeforeBeginBuild ()
		{
			string empty_project_xml = "<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' />";
			var path = "file://localhost/foo.xml";
			var xml = XmlReader.Create (new StringReader (empty_project_xml), null, path);
			var root = ProjectRootElement.Create (xml);
			var proj = new ProjectInstance (root);
            new BuildManager ().PendBuildRequest (new BuildRequestData (proj, new string [0]));
		}
		
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ResetCachesDuringBuildIsInvalid ()
		{
			// Windows does not have useful sleep or alternative, so skip it
			bool is_windows = true;
			switch (Environment.OSVersion.Platform) {
			case PlatformID.Unix:
			case PlatformID.MacOSX:
				is_windows = false;
				break;
			}
			string project_xml = string.Format (@"<Project DefaultTargets='Wait1Sec' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Target Name='Wait1Sec'>
    <Exec Command='{0}' />
  </Target>
</Project>", is_windows ? "powershell -command \"Start-Sleep -s 1\"" : "/bin/sleep 1");
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			var proj = new ProjectInstance (root);
			var bm = new BuildManager ();
			bm.BeginBuild (new BuildParameters ());
			var sub = bm.PendBuildRequest (new BuildRequestData (proj, new string [] { "Wait1Sec" }));
			sub.ExecuteAsync (delegate {}, null);
			try {
				bm.ResetCaches ();
			} finally {
				bm.EndBuild (); // yes, it should work even after invalid ResetCaches call... at least on .NET it does.
			}
		}
		
		[Test]
		public void BasicManualParallelBuilds ()
		{
			string project_xml = @"<Project DefaultTargets='Wait1Sec' xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Target Name='Wait1Sec'>
    <Exec Command='ping 10.1.1.1 -n 1 -w 1' />
  </Target>
</Project>";
			var xml = XmlReader.Create (new StringReader (project_xml));
			var root = ProjectRootElement.Create (xml);
			var proj = new ProjectInstance (root);
			var bm = new BuildManager ();
			bm.BeginBuild (new BuildParameters ());
			DateTime waitDone = DateTime.MinValue;
			DateTime beforeExec = DateTime.Now;
			var l = new List<BuildSubmission> ();
			for (int i = 0; i < 10; i++) {
				var sub = bm.PendBuildRequest (new BuildRequestData (proj, new string [] { "Wait1Sec" }));
				l.Add (sub);
				sub.ExecuteAsync (delegate { waitDone = DateTime.Now; }, null);
			}
			bm.EndBuild ();
			Assert.IsTrue (l.All (s => s.BuildResult.OverallResult == BuildResultCode.Success), "#1");
			DateTime endBuildDone = DateTime.Now;
			Assert.IsTrue (endBuildDone - beforeExec >= TimeSpan.FromSeconds (1), "#2");
			Assert.IsTrue (endBuildDone > waitDone, "#3");
		}
		
		[Test]
		public void BuildCommonResolveAssemblyReferences ()
		{
            string project_xml = @"<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
  <Import Project='$(MSBuildToolsPath)\Microsoft.Common.targets' />
  <ItemGroup>
    <Reference Include='System.Core' />
    <Reference Include='System.Xml' />
  </ItemGroup>
</Project>";
            var xml = XmlReader.Create (new StringReader (project_xml));
            var root = ProjectRootElement.Create (xml);
			root.FullPath = "BuildManagerTest.BuildCommonResolveAssemblyReferences.proj";
            var proj = new ProjectInstance (root);
			var manager = new BuildManager ();
			var parameters = new BuildParameters () { Loggers = new ILogger [] {new ConsoleLogger (LoggerVerbosity.Diagnostic)} };
			var request = new BuildRequestData (proj, new string [] {"ResolveAssemblyReferences"});
			var result = manager.Build (parameters, request);
			var items = result.ResultsByTarget ["ResolveAssemblyReferences"].Items;
			Assert.AreEqual (2, items.Count (), "#0");
			Assert.IsTrue (items.Any (i => Path.GetFileName (i.ItemSpec) == "System.Core.dll"), "#1");
			Assert.IsTrue (items.Any (i => Path.GetFileName (i.ItemSpec) == "System.Xml.dll"), "#2");
			Assert.IsTrue (File.Exists (items.First (i => Path.GetFileName (i.ItemSpec) == "System.Core.dll").ItemSpec), "#3");
			Assert.IsTrue (File.Exists (items.First (i => Path.GetFileName (i.ItemSpec) == "System.Xml.dll").ItemSpec), "#4");
			Assert.AreEqual (BuildResultCode.Success, result.OverallResult, "#5");
		}
	}
}

