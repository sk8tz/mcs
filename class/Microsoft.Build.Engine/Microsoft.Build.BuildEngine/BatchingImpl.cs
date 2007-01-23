//
// BatchingImpl.cs: Class that implements BatchingAlgorithm from the wiki.
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
using System.IO;
using System.Xml;

namespace Microsoft.Build.BuildEngine {
	internal class BatchingImpl {
	
		string		inputs;
		string		outputs;
		Project		project;
	
		public BatchingImpl (Project project, XmlElement targetElement)
		{
			if (targetElement == null)
				throw new ArgumentNullException ("targetElement");
			if (project == null)
				throw new ArgumentNullException ("project");
		
			this.project = project;
			
			inputs = targetElement.GetAttribute ("Inputs");
			outputs = targetElement.GetAttribute ("Outputs");
		}
		
		public bool BuildNeeded ()
		{
			// FIXME: change this to ITaskItem instead of string
		
			Expression inputExpr, outputExpr;
			string[] inputFiles, outputFiles;
			DateTime oldestInput, youngestOutput;
		
			if (inputs == String.Empty)
				return true;
			
			if (outputs == String.Empty)
				return true;
			
			
			inputExpr = new Expression ();
			inputExpr.Parse (inputs, true);
			outputExpr = new Expression ();
			outputExpr.Parse (outputs, true);
			
			inputFiles = (string[]) inputExpr.ConvertTo (project, typeof (string[]));
			outputFiles = (string[]) outputExpr.ConvertTo (project, typeof (string[]));
			
			if (inputFiles == null)
				return true;
			
			if (outputFiles == null)
				return true;
			
			if (inputFiles.Length == 0)
				return true;
			
			if (outputFiles.Length == 0)
				return true;
			
			
			if (File.Exists (inputFiles [0])) 
				oldestInput = File.GetLastWriteTime (inputFiles [0]);
			else 
				return true;
			
			if (File.Exists (outputFiles [0]))
				youngestOutput = File.GetLastWriteTime (outputFiles [0]);
			else
				return true;
			
				
			foreach (string file in inputFiles) {
				if (file.Trim () == String.Empty)
					continue;
			
				if (File.Exists (file.Trim ())) {
					if (File.GetLastWriteTime (file.Trim ()) > oldestInput)
						oldestInput = File.GetLastWriteTime (file.Trim ());
				} else {
					return true;
				}
			}
			foreach (string file in outputFiles) {
				if (file.Trim () == String.Empty)
					continue;
			
				if (File.Exists (file.Trim ())) {
					if (File.GetLastWriteTime (file.Trim ()) < youngestOutput)
						youngestOutput = File.GetLastWriteTime (file.Trim ());
				} else
					return true;
			}
			
			if (oldestInput > youngestOutput)
				return true;
			else
				return false;
		}
		
		// FIXME: should do everything from task batching specification, not just run task once
		public bool BatchBuildTask (BuildTask buildTask)
		{
			if (buildTask.Condition == String.Empty)
				return buildTask.Execute ();
			
			ConditionExpression ce = ConditionParser.ParseCondition (buildTask.Condition);
			
			if (ce.BoolEvaluate (project))
				return buildTask.Execute ();
			else
			// FIXME: skipped, it should be logged
				return true;
			
		}
	}
}

#endif
