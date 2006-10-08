//
// Target.cs: Represents a target.
//
// Author:
//   Marek Sieradzki (marek.sieradzki@gmail.com)
//
// (C) 2005 Marek Sieradzki
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

#if NET_2_0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Build.Framework;

namespace Microsoft.Build.BuildEngine {
	public class Target : IEnumerable {
	
		BatchingImpl	batchingImpl;
		BuildState	buildState;
		XmlAttribute	condition;
		XmlAttribute	dependsOnTargets;
		Engine		engine;
		bool		isImported;
		string		name;
		Project		project;
		XmlElement	targetElement;
		List <XmlElement>	onErrorElements;
		List <BuildTask>	buildTasks;
		
		internal Target (XmlElement targetElement, Project project)
		{
			if (project == null)
				throw new ArgumentNullException ("project");
			if (targetElement == null)
				throw new ArgumentNullException ("targetElement");

			this.targetElement = targetElement;
			this.name = targetElement.GetAttribute ("Name");
			this.condition = targetElement.GetAttributeNode ("Condition");
			this.dependsOnTargets = targetElement.GetAttributeNode ("DependsOnTargets");

			this.project = project;
			this.engine = project.ParentEngine;
			this.isImported = false;;

			this.onErrorElements  = new List <XmlElement> ();
			this.buildState = BuildState.NotStarted;
			this.buildTasks = new List <BuildTask> ();
			this.batchingImpl = new BatchingImpl (project, this.targetElement);

			foreach (XmlNode xn in targetElement.ChildNodes) {
				if (xn is XmlElement) {
					XmlElement xe = (XmlElement) xn;
					if (xe.Name == "OnError") {
						onErrorElements.Add (xe);
						continue;
					}
					buildTasks.Add (new BuildTask (xe, this));
				}
			}
		}
		
		internal bool Build ()
		{
			bool result;
		
			buildState = BuildState.Started;
			if (dependsOnTargets == null) {
				;
			} else if (dependsOnTargets.Value == String.Empty) {
				;
			} else {
				OldExpression dependencies = new OldExpression (Project);
				dependencies.ParseSource (dependsOnTargets.Value);
				
				string[] targetsToBuildFirst = (string[]) dependencies.ConvertTo (typeof (string[]));
				foreach (string target in targetsToBuildFirst) {
					string trimmed = target.Trim ();
					Target t = (Target) project.Targets [trimmed];
					if (t == null)
						throw new InvalidProjectFileException (String.Format ("Target {0} not found.", trimmed));
					if (t.BuildState == BuildState.NotStarted) {
						t.Build ();
					}
					if (t.BuildState == BuildState.Started)
						throw new InvalidProjectFileException ("Cycle in target dependencies detected.");
				}
			}
			
			result = RealBuild ();
			buildState = BuildState.Finished;
			
			return result;
		}
		
		private bool RealBuild ()
		{
			bool executeOnErrors = false;
			bool result = true;
		
			LogTargetStarted ();
			
			if (this.batchingImpl.BuildNeeded ()) {
				foreach (BuildTask bt in buildTasks) {
					result = batchingImpl.BatchBuildTask (bt);
				
					if (!result && !bt.ContinueOnError) {
						executeOnErrors = true;
						break;
					}
				}
			} else {
				LogTargetSkipped ();
			}

			LogTargetFinished (result);
			
			if (executeOnErrors == true)
				ExecuteOnErrors ();
				
			return result;
		}
		
		private void ExecuteOnErrors ()
		{
			foreach (XmlElement onError in onErrorElements) {
				// FIXME: add condition
				if (onError.GetAttribute ("ExecuteTargets") == String.Empty)
					throw new InvalidProjectFileException ("ExecuteTargets attribute is required in OnError element.");
				string[] targetsToExecute = onError.GetAttribute ("ExecuteTargets").Split (';');
				foreach (string t in targetsToExecute)
					this.project.Targets [t].Build ();
			}
		}
		
		private void LogTargetSkipped ()
		{
			BuildMessageEventArgs bmea;
			bmea = new BuildMessageEventArgs (String.Format ("Skipping target \"{0}\" because its outputs are up-to-date.",
				name), null, "MSBuild", MessageImportance.Normal);
			engine.EventSource.FireMessageRaised (this, bmea);
		}
		
		private void LogTargetStarted ()
		{
			TargetStartedEventArgs tsea;
			string projectFile = project.FullFileName;
			tsea = new TargetStartedEventArgs ("Target " + name + " started.", null, name, projectFile, null);
			engine.EventSource.FireTargetStarted (this, tsea);
		}
		
		private void LogTargetFinished (bool succeeded)
		{
			TargetFinishedEventArgs tfea;
			string projectFile = project.FullFileName;
			tfea = new TargetFinishedEventArgs ("Target " + name + " finished.", null, name, projectFile, null, succeeded);
			engine.EventSource.FireTargetFinished (this, tfea);
		}
	
		[MonoTODO]
		public BuildTask AddNewTask (string taskName)
		{
			throw new NotImplementedException ();
		}

		public IEnumerator GetEnumerator ()
		{
			foreach (BuildTask bt in buildTasks) {
				yield return bt;
			}
		}

		public void RemoveTask (BuildTask buildTask)
		{
			//taskElements.Remove (taskElement);
			buildTasks.Remove (buildTask);
		}

		public string Condition {
			get { return condition.Value; }
			set { condition.Value = value; }
		}

		public string DependsOnTargets {
			get {
				if (dependsOnTargets == null)
					return null;
				else
					return dependsOnTargets.Value;
			}
			set {
				if (dependsOnTargets != null)
					dependsOnTargets.Value = value;
			}
		}

		public bool IsImported {
			get { return isImported; }
			internal set { isImported = value; }
		}

		public string Name {
			get { return name; }
		}
		
		internal Project Project {
			get { return project; }
		}
		
		internal BuildState BuildState {
			get { return buildState; }
		}
	}
	
	internal enum BuildState {
		NotStarted,
		Started,
		Finished,
		Skipped
	}
}

#endif
