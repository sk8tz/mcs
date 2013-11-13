//
// ProjectCollection.cs
//
// Author:
//   Leszek Ciesielski (skolima@gmail.com)
//   Rolf Bjarne Kvinge (rolf@xamarin.com)
//   Atsushi Enomoto (atsushi@xamarin.com)
//
// (C) 2011 Leszek Ciesielski
// Copyright (C) 2011,2013 Xamarin Inc.
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

using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml;
using System.Reflection;

namespace Microsoft.Build.Evaluation
{
	public class ProjectCollection : IDisposable
	{
		public delegate void ProjectAddedEventHandler (object target, ProjectAddedToProjectCollectionEventArgs args);
		
		public class ProjectAddedToProjectCollectionEventArgs : EventArgs
		{
			public ProjectAddedToProjectCollectionEventArgs (ProjectRootElement project)
			{
				if (project == null)
					throw new ArgumentNullException ("project");
				ProjectRootElement = project;
			}
			
			public ProjectRootElement ProjectRootElement { get; private set; }
		}

		// static members

		static readonly ProjectCollection global_project_collection;

		static ProjectCollection ()
		{
			#if NET_4_5
			global_project_collection = new ProjectCollection (new ReadOnlyDictionary<string, string> (new Dictionary<string, string> ()));
			#else
			global_project_collection = new ProjectCollection (new Dictionary<string, string> ());
			#endif
		}

		public static string Escape (string unescapedString)
		{
			return Mono.XBuild.Utilities.MSBuildUtils.Escape (unescapedString);
		}

		public static string Unescape (string escapedString)
		{
			return Mono.XBuild.Utilities.MSBuildUtils.Unescape (escapedString);
		}

		public static ProjectCollection GlobalProjectCollection {
			get { return global_project_collection; }
		}

		// semantic model part

		public ProjectCollection ()
			: this (null)
		{
		}

		public ProjectCollection (IDictionary<string, string> globalProperties)
        	: this (globalProperties, null, ToolsetDefinitionLocations.Registry | ToolsetDefinitionLocations.ConfigurationFile)
		{
		}

		public ProjectCollection (ToolsetDefinitionLocations toolsetDefinitionLocations)
        	: this (null, null, toolsetDefinitionLocations)
		{
		}

		public ProjectCollection (IDictionary<string, string> globalProperties, IEnumerable<ILogger> loggers,
				ToolsetDefinitionLocations toolsetDefinitionLocations)
			: this (globalProperties, loggers, null, toolsetDefinitionLocations, int.MaxValue, false)
		{
		}

		public ProjectCollection (IDictionary<string, string> globalProperties,
				IEnumerable<ILogger> loggers, IEnumerable<ForwardingLoggerRecord> remoteLoggers,
				ToolsetDefinitionLocations toolsetDefinitionLocations,
				int maxNodeCount, bool onlyLogCriticalEvents)
		{
			global_properties = globalProperties ?? new Dictionary<string, string> ();
			this.loggers = loggers != null ? loggers.ToList () : new List<ILogger> ();
			toolset_locations = toolsetDefinitionLocations;
			max_node_count = maxNodeCount;
			OnlyLogCriticalEvents = onlyLogCriticalEvents;

			LoadDefaultToolsets ();
		}
		
		[MonoTODO ("not fired yet")]
		public event ProjectAddedEventHandler ProjectAdded;
		[MonoTODO ("not fired yet")]
		public event EventHandler<ProjectChangedEventArgs> ProjectChanged;
		[MonoTODO ("not fired yet")]
		public event EventHandler<ProjectCollectionChangedEventArgs> ProjectCollectionChanged;
		[MonoTODO ("not fired yet")]
		public event EventHandler<ProjectXmlChangedEventArgs> ProjectXmlChanged;

		readonly int max_node_count;

		public void AddProject (Project project)
		{
			this.loaded_projects.Add (project);
			if (ProjectAdded != null)
				ProjectAdded (this, new ProjectAddedToProjectCollectionEventArgs (project.Xml));
		}

		public int Count {
			get { return loaded_projects.Count; }
		}

		public string DefaultToolsVersion {
			get { return Toolsets.Any () ? Toolsets.First ().ToolsVersion : null; }
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing) {
			}
		}

		public ICollection<Project> GetLoadedProjects (string fullPath)
		{
			return LoadedProjects.Where (p => p.FullPath != null && Path.GetFullPath (p.FullPath) == Path.GetFullPath (fullPath)).ToList ();
		}

		readonly IDictionary<string, string> global_properties;

		public IDictionary<string, string> GlobalProperties {
			get { return global_properties; }
		}

		readonly List<Project> loaded_projects = new List<Project> ();
		
		public Project LoadProject (string fileName)
		{
			return LoadProject (fileName, DefaultToolsVersion);
		}
		
		public Project LoadProject (string fileName, string toolsVersion)
		{
			return LoadProject (fileName, toolsVersion);
		}
		
		public Project LoadProject (string fileName, IDictionary<string,string> globalProperties, string toolsVersion)
		{
			return new Project (fileName, globalProperties, toolsVersion);
		}
		
		// These methods somehow don't add the project to ProjectCollection...
		public Project LoadProject (XmlReader xmlReader)
		{
			return LoadProject (xmlReader, DefaultToolsVersion);
		}
		
		public Project LoadProject (XmlReader xmlReader, string toolsVersion)
		{
			return LoadProject (xmlReader, null, toolsVersion);
		}
		
		public Project LoadProject (XmlReader xmlReader, IDictionary<string,string> globalProperties, string toolsVersion)
		{
			return new Project (xmlReader, globalProperties, toolsVersion);
		}
		
		public ICollection<Project> LoadedProjects {
			get { return loaded_projects; }
		}

		readonly List<ILogger> loggers = new List<ILogger> ();
		[MonoTODO]
		public ICollection<ILogger> Loggers {
			get { return loggers; }
		}

		[MonoTODO]
		public bool OnlyLogCriticalEvents { get; set; }

		[MonoTODO]
		public bool SkipEvaluation { get; set; }

		readonly ToolsetDefinitionLocations toolset_locations;
		public ToolsetDefinitionLocations ToolsetLocations {
			get { return toolset_locations; }
		}

		readonly List<Toolset> toolsets = new List<Toolset> ();
		// so what should we do without ToolLocationHelper in Microsoft.Build.Utilities.dll? There is no reference to it in this dll.
		public ICollection<Toolset> Toolsets {
			// For ConfigurationFile and None, they cannot be added externally.
			get { return (ToolsetLocations & ToolsetDefinitionLocations.Registry) != 0 ? toolsets : toolsets.ToList (); }
		}
		
		public Toolset GetToolset (string toolsVersion)
		{
			return Toolsets.FirstOrDefault (t => t.ToolsVersion == toolsVersion);
		}

		//FIXME: should also support config file, depending on ToolsetLocations
		void LoadDefaultToolsets ()
		{
			AddToolset (new Toolset ("2.0",
				ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version20), this, null));
			AddToolset (new Toolset ("3.0",
				ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version30), this, null));
			AddToolset (new Toolset ("3.5",
				ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version35), this, null));
#if NET_4_0
			AddToolset (new Toolset ("4.0",
				ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version40), this, null));
#endif
#if NET_4_5
			AddToolset (new Toolset ("12.0",
				ToolLocationHelper.GetMSBuildInstallPath ("12.0"), this, ToolLocationHelper.GetPathToDotNetFramework (TargetDotNetFrameworkVersion.Version40)));
#endif
		}
		
		[MonoTODO ("not verified at all")]
		public void AddToolset (Toolset toolset)
		{
			toolsets.Add (toolset);
		}
		
		[MonoTODO ("not verified at all")]
		public void RemoveAllToolsets ()
		{
			toolsets.Clear ();
		}
		
		[MonoTODO ("not verified at all")]
		public void RegisterLogger (ILogger logger)
		{
			loggers.Add (logger);
		}
		
		[MonoTODO ("not verified at all")]
		public void RegisterLoggers (IEnumerable<ILogger> loggers)
		{
			foreach (var logger in loggers)
				this.loggers.Add (logger);
		}

		public void UnloadAllProjects ()
		{
			throw new NotImplementedException ();
		}

		public void UnloadProject (Project project)
		{
			throw new NotImplementedException ();
		}

		public void UnloadProject (ProjectRootElement projectRootElement)
		{
			throw new NotImplementedException ();
		}

		public static Version Version {
			get { throw new NotImplementedException (); }
		}

		// Execution part

		[MonoTODO]
		public bool DisableMarkDirty { get; set; }

		[MonoTODO]
		public HostServices HostServices { get; set; }

		[MonoTODO]
		public bool IsBuildEnabled { get; set; }
		
		internal string BuildStartupDirectory { get; set; }
		
		Stack<string> ongoing_imports = new Stack<string> ();
		
		internal Stack<string> OngoingImports {
			get { return ongoing_imports; }
		}
		
		// common part
		internal static IEnumerable<EnvironmentProjectProperty> GetWellKnownProperties (Project project)
		{
			Func<string,string,EnvironmentProjectProperty> create = (name, value) => new EnvironmentProjectProperty (project, name, value, true);
			return GetWellKnownProperties (create);
		}
		
		internal static IEnumerable<ProjectPropertyInstance> GetWellKnownProperties (ProjectInstance project)
		{
			Func<string,string,ProjectPropertyInstance> create = (name, value) => new ProjectPropertyInstance (name, true, value);
			return GetWellKnownProperties (create);
		}
		
		static IEnumerable<T> GetWellKnownProperties<T> (Func<string,string,T> create)
		{
			var ext = Environment.GetEnvironmentVariable ("MSBuildExtensionsPath") ?? DefaultExtensionsPath;
			yield return create ("MSBuildExtensionsPath", ext);
			var ext32 = Environment.GetEnvironmentVariable ("MSBuildExtensionsPath32") ?? DefaultExtensionsPath;
			yield return create ("MSBuildExtensionsPath32", ext32);
			var ext64 = Environment.GetEnvironmentVariable ("MSBuildExtensionsPath64") ?? DefaultExtensionsPath;
			yield return create ("MSBuildExtensionsPath64", ext64);
		}

		static string extensions_path;
		internal static string DefaultExtensionsPath {
			get {
				if (extensions_path == null) {
					// NOTE: code from mcs/tools/gacutil/driver.cs
					PropertyInfo gac = typeof (System.Environment).GetProperty (
							"GacPath", BindingFlags.Static | BindingFlags.NonPublic);

					if (gac != null) {
						MethodInfo get_gac = gac.GetGetMethod (true);
						string gac_path = (string) get_gac.Invoke (null, null);
						extensions_path = Path.GetFullPath (Path.Combine (
									gac_path, Path.Combine ("..", "xbuild")));
					}
				}
				return extensions_path;
			}
		}
		
		internal IEnumerable<ReservedProjectProperty> GetReservedProperties (Toolset toolset, Project project)
		{
			Func<string,Func<string>,ReservedProjectProperty> create = (name, value) => new ReservedProjectProperty (project, name, value);
			return GetReservedProperties<ReservedProjectProperty> (toolset, project.Xml, create, () => project.FullPath);
		}
		
		internal IEnumerable<ProjectPropertyInstance> GetReservedProperties (Toolset toolset, ProjectInstance project, ProjectRootElement xml)
		{
			Func<string,Func<string>,ProjectPropertyInstance> create = (name, value) => new ProjectPropertyInstance (name, true, null, value);
			return GetReservedProperties<ProjectPropertyInstance> (toolset, xml, create, () => project.FullPath);
		}
		
		// seealso http://msdn.microsoft.com/en-us/library/ms164309.aspx
		IEnumerable<T> GetReservedProperties<T> (Toolset toolset, ProjectRootElement project, Func<string,Func<string>,T> create, Func<string> projectFullPath)
		{
			yield return create ("MSBuildBinPath", () => toolset.ToolsPath);
			// FIXME: add MSBuildLastTaskResult
			// FIXME: add MSBuildNodeCount
			// FIXME: add MSBuildProgramFiles32
			yield return create ("MSBuildProjectDefaultTargets", () => project.DefaultTargets);
			yield return create ("MSBuildProjectDirectory", () => project.DirectoryPath + Path.DirectorySeparatorChar);
			yield return create ("MSBuildProjectDirectoryNoRoot", () => project.DirectoryPath.Substring (Path.GetPathRoot (project.DirectoryPath).Length));
			yield return create ("MSBuildProjectExtension", () => Path.GetExtension (project.FullPath));
			yield return create ("MSBuildProjectFile", () => Path.GetFileName (project.FullPath));
			yield return create ("MSBuildProjectFullPath", () => project.FullPath);
			yield return create ("MSBuildProjectName", () => Path.GetFileNameWithoutExtension (project.FullPath));
			yield return create ("MSBuildStartupDirectory", () => BuildStartupDirectory);
			yield return create ("MSBuildThisFile", () => Path.GetFileName (GetEvaluationTimeThisFile (projectFullPath)));
			yield return create ("MSBuildThisFileFullPath", () => GetEvaluationTimeThisFile (projectFullPath));
			yield return create ("MSBuildThisFileName", () => Path.GetFileNameWithoutExtension (GetEvaluationTimeThisFile (projectFullPath)));
			yield return create ("MSBuildThisFileExtension", () => Path.GetExtension (GetEvaluationTimeThisFile (projectFullPath)));

			yield return create ("MSBuildThisFileDirectory", () => Path.GetDirectoryName (GetEvaluationTimeThisFileDirectory (projectFullPath)));
			yield return create ("MSBuildThisFileDirectoryNoRoot", () => {
				string dir = GetEvaluationTimeThisFileDirectory (projectFullPath) + Path.DirectorySeparatorChar;
				return dir.Substring (Path.GetPathRoot (dir).Length);
				});
			yield return create ("MSBuildToolsPath", () => toolset.ToolsPath);
			yield return create ("MSBuildToolsVersion", () => toolset.ToolsVersion);
		}
		
		// These are required for reserved property, represents dynamically changing property values.
		// This should resolve to either the project file path or that of the imported file.
		internal string GetEvaluationTimeThisFileDirectory (Func<string> nonImportingTimeFullPath)
		{
			var file = GetEvaluationTimeThisFile (nonImportingTimeFullPath);
			var dir = Path.IsPathRooted (file) ? Path.GetDirectoryName (file) : Directory.GetCurrentDirectory ();
			return dir + Path.DirectorySeparatorChar;
		}

		internal string GetEvaluationTimeThisFile (Func<string> nonImportingTimeFullPath)
		{
			return OngoingImports.Count > 0 ? OngoingImports.Peek () : (nonImportingTimeFullPath () ?? string.Empty);
		}
	}
}
