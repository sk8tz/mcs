//
// ToolTask.cs: Base class for command line tool tasks. 
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
using System.Diagnostics;
using System.Collections.Specialized;
using System.IO;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Mono.XBuild.Utilities;

namespace Microsoft.Build.Utilities
{
	public abstract class ToolTask : Task
	{
		StringDictionary	environmentOverride;
		int			timeout;
		string			toolPath;
		Process			process;
		
		static Regex		regex;
		
		protected ToolTask ()
			: this (null, null)
		{
		}

		protected ToolTask (ResourceManager taskResources)
			: this (taskResources, null)
		{
		}

		protected ToolTask (ResourceManager taskResources,
				   string helpKeywordPrefix)
		{
			this.TaskResources = taskResources;
			this.HelpKeywordPrefix = helpKeywordPrefix;
			this.toolPath = MonoLocationHelper.GetBinDir ();
		}

		static ToolTask ()
		{
			regex = new Regex (
				@"^\s*"
				+ @"(((?<ORIGIN>(((\d+>)?[a-zA-Z]?:[^:]*)|([^:]*))):)"
				+ "|())"
				+ "(?<SUBCATEGORY>(()|([^:]*? )))"
				+ "(?<CATEGORY>(error|warning)) "
				+ "(?<CODE>[^:]*):"
				+ "(?<TEXT>.*)$",
				RegexOptions.IgnoreCase);
		}

		protected virtual bool CallHostObjectToExecute ()
		{
			return true;
		}

		public override bool Execute ()
		{
			string arguments, toolFilename;
			
			arguments = String.Concat (GenerateCommandLineCommands (), " ", GenerateResponseFileCommands ());
			toolFilename = GenerateFullPathToTool (); 
			return RealExecute (toolFilename, arguments);
		}
		
		private bool RealExecute (string filename, string arguments)
		{
			string line;
		
			if (filename == null)
				throw new ArgumentNullException ("filename");
			if (arguments == null)
				throw new ArgumentNullException ("arguments");
			
			process = new Process ();
			process.StartInfo.Arguments = arguments;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.FileName = filename;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.UseShellExecute = false;
			
			Log.LogMessage (MessageImportance.Low, String.Format ("Tool {0} execution started with arguments: {1}",
				filename, arguments));
			
			process.Start ();
			process.WaitForExit ();
			
			while ((line = process.StandardError.ReadLine ()) != null) {
				HandleError (line);
			}
			
			Log.LogMessage (MessageImportance.Low, String.Format ("Tool {0} execution finished.", filename));
			
			return !Log.HasLoggedErrors;
		}
		
		private void HandleError (string line)
		{
			string filename, origin, category, code, subcategory, text;
			int lineNumber, columnNumber, endLineNumber, endColumnNumber;
		
			Match m = regex.Match (line);
			origin = m.Groups [regex.GroupNumberFromName ("ORIGIN")].Value;
			category = m.Groups [regex.GroupNumberFromName ("CATEGORY")].Value;
			code = m.Groups [regex.GroupNumberFromName ("CODE")].Value;
			subcategory = m.Groups [regex.GroupNumberFromName ("SUBCATEGORY")].Value;
			text = m.Groups [regex.GroupNumberFromName ("TEXT")].Value;
			
			ParseOrigin (origin, out filename, out lineNumber, out columnNumber, out endLineNumber, out endColumnNumber);
			
			if (category == "warning") {
				Log.LogWarning (subcategory, code, null, filename, lineNumber, columnNumber, endLineNumber,
					endColumnNumber, text, null);
			} else if (category == "error") {
				Log.LogError (subcategory, code, null, filename, lineNumber, columnNumber, endLineNumber,
					endColumnNumber, text, null);
			}
		}
		
		private void ParseOrigin (string origin, out string filename,
				     out int lineNumber, out int columnNumber,
				     out int endLineNumber, out int endColumnNumber)
		{
			int lParen;
			string[] temp;
			string[] left, right;
			
			if (origin.IndexOf ('(') != -1 ) {
				lParen = origin.IndexOf ('(');
				filename = origin.Substring (0, lParen);
				temp = origin.Substring (lParen + 1, origin.Length - lParen - 2).Split (',');
				if (temp.Length == 1) {
					left = temp [0].Split ('-');
					if (left.Length == 1) {
						lineNumber = Int32.Parse (left [0]);
						columnNumber = 0;
						endLineNumber = 0;
						endColumnNumber = 0;
					} else if (left.Length == 2) {
						lineNumber = Int32.Parse (left [0]);
						columnNumber = 0;
						endLineNumber = Int32.Parse (left [1]);
						endColumnNumber = 0;
					} else
						throw new Exception ("Invalid line/column format.");
				} else if (temp.Length == 2) {
					right = temp [1].Split ('-');
					lineNumber = Int32.Parse (temp [0]);
					endLineNumber = 0;
					if (right.Length == 1) {
						columnNumber = Int32.Parse (right [0]);
						endColumnNumber = 0;
					} else if (right.Length == 2) {
						columnNumber = Int32.Parse (right [0]);
						endColumnNumber = Int32.Parse (right [0]);
					} else
						throw new Exception ("Invalid line/column format.");
				} else if (temp.Length == 4) {
					lineNumber = Int32.Parse (temp [0]);
					endLineNumber = Int32.Parse (temp [2]);
					columnNumber = Int32.Parse (temp [1]);
					endColumnNumber = Int32.Parse (temp [3]);
				} else
					throw new Exception ("Invalid line/column format.");
			} else {
				filename = origin;
				lineNumber = 0;
				columnNumber = 0;
				endLineNumber = 0;
				endColumnNumber = 0;
			}
		}

		protected virtual string GenerateCommandLineCommands ()
		{
			return null;
		}

		protected abstract string GenerateFullPathToTool ();

		protected virtual string GenerateResponseFileCommands ()
		{
			return null;
		}

		protected virtual string GetResponseFileSwitch (string responseFilePath)
		{
			return String.Format ("@{0}", responseFilePath);
		}

		protected virtual bool HandleTaskExecutionErrors (int exitCode,
								 bool hasTaskLoggedErrors)
		{
			return true;
		}

		protected virtual bool InitializeHostObject (out bool appropriateHostObjectExists,
							    out bool continueBuild)
		{
			appropriateHostObjectExists = (HostObject != null) ? true : false;
			continueBuild = true;
			return true;
		}

		protected virtual void LogToolCommand (string message)
		{
		}

		protected virtual bool SkipTaskExecution()
		{
			return false;
		}

		protected virtual bool ValidateParameters()
		{
			return true;
		}

		protected virtual StringDictionary EnvironmentOverride
		{
			get { return environmentOverride; }
		}

		protected virtual Encoding ResponseFileEncoding
		{
			get { return Encoding.UTF8; }
		}

		protected virtual Encoding StandardErrorEncoding
		{
			get { return Console.Error.Encoding; }
		}

		protected virtual Encoding StandardOutputEncoding
		{
			get { return Console.Out.Encoding; }
		}

		public int Timeout
		{
			get { return timeout; }
			set { timeout = value; }
		}

		protected abstract string ToolName
		{
			get;
		}

		public string ToolPath
		{
			get { return toolPath; }
			set { toolPath  = value; }
		}
	}
}

#endif