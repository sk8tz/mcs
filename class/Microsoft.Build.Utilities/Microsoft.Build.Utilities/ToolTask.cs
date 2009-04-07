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
using System.Collections;
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
		int			exitCode;
		int			timeout;
		string			toolPath;
		Process			process;
		Encoding		responseFileEncoding;
		MessageImportance	standardErrorLoggingImportance;
		MessageImportance	standardOutputLoggingImportance;
		
		static Regex		regex;
		
		protected ToolTask ()
			: this (null, null)
		{
			this.standardErrorLoggingImportance = MessageImportance.High;
			this.standardOutputLoggingImportance = MessageImportance.Normal;
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
			this.responseFileEncoding = Encoding.UTF8;
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

		[MonoTODO]
		protected virtual bool CallHostObjectToExecute ()
		{
			return true;
		}

		[MonoTODO]
		// FIXME: it should write responseFileCommands to temporary response file
		protected virtual int ExecuteTool (string pathToTool,
						   string responseFileCommands,
						   string commandLineCommands)
		{
			return RealExecute (pathToTool, responseFileCommands, commandLineCommands) ? 0 : -1;
		}
		
		public override bool Execute ()
		{
			if (SkipTaskExecution ())
				return true;

			int result = ExecuteTool (GenerateFullPathToTool (), GenerateResponseFileCommands (),
				GenerateCommandLineCommands ());
			
			return result == 0;
		}
		
		[MonoTODO]
		protected virtual string GetWorkingDirectory ()
		{
			return null;
		}
		
		private bool RealExecute (string pathToTool,
					   string responseFileCommands,
					   string commandLineCommands)

		{
			if (pathToTool == null)
				throw new ArgumentNullException ("pathToTool");

			if (!File.Exists (pathToTool)) {
				Log.LogError ("Unable to find tool {0} at '{1}'", ToolName, pathToTool);
				return false;
			}

			string responseFileName = Path.GetTempFileName ();
			File.WriteAllText (responseFileName, responseFileCommands);

			string arguments = String.Concat (commandLineCommands, " ", GetResponseFileSwitch (responseFileName));

			Log.LogMessage (MessageImportance.Normal, String.Format ("Tool {0} execution started with arguments: {1} {2}",
				pathToTool, commandLineCommands, responseFileCommands));

			string output = Path.GetTempFileName ();
			string error = Path.GetTempFileName ();
			StreamWriter outwr = new StreamWriter (output);
			StreamWriter errwr = new StreamWriter (error);

			ProcessStartInfo pinfo = new ProcessStartInfo (pathToTool, arguments);
			pinfo.WorkingDirectory = GetWorkingDirectory () ?? Environment.CurrentDirectory;

			pinfo.UseShellExecute = false;
			pinfo.RedirectStandardOutput = true;
			pinfo.RedirectStandardError = true;

			try {
				ProcessWrapper pw = ProcessService.StartProcess (pinfo, outwr, errwr, null, environmentOverride);
				pw.WaitForOutput();
				exitCode = pw.ExitCode;
				outwr.Close();
				errwr.Close();
				pw.Dispose ();
			} catch (System.ComponentModel.Win32Exception e) {
				Log.LogError ("Error executing tool '{0}': {1}", pathToTool, e.Message);
				return false;
			}

			foreach (string s in new string[] { output, error }) {
				using (StreamReader sr = File.OpenText (s)) {
					string line;
					while ((line = sr.ReadLine ()) != null) {
						LogEventsFromTextOutput (line, MessageImportance.Low);
					}
				}
			}
			
			Log.LogMessage (MessageImportance.Low, String.Format ("Tool {0} execution finished.", pathToTool));
			
			return !Log.HasLoggedErrors;
		}
		
		
		[MonoTODO]
		protected virtual void LogEventsFromTextOutput (string singleLine, MessageImportance importance)
		{
			if (String.IsNullOrEmpty (singleLine))
				return;

			string filename, origin, category, code, subcategory, text;
			int lineNumber, columnNumber, endLineNumber, endColumnNumber;
		
			Match m = regex.Match (singleLine);
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
			} else {
				Log.LogError (singleLine);
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

		[MonoTODO]
		protected virtual string GenerateCommandLineCommands ()
		{
			return null;
		}

		protected abstract string GenerateFullPathToTool ();

		[MonoTODO]
		protected virtual string GenerateResponseFileCommands ()
		{
			return null;
		}

		[MonoTODO]
		protected virtual string GetResponseFileSwitch (string responseFilePath)
		{
			return String.Format ("@{0}", responseFilePath);
		}

		[MonoTODO]
		protected virtual bool HandleTaskExecutionErrors ()
		{
			return true;
		}

		protected virtual HostObjectInitializationStatus InitializeHostObject ()
		{
			return HostObjectInitializationStatus.NoActionReturnSuccess;
		}

		[MonoTODO]
		protected virtual void LogToolCommand (string message)
		{
		}
		
		[MonoTODO]
		protected virtual void LogPathToTool (string toolName,
						      string pathToTool)
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
		
		[MonoTODO]
		[Output]
		public int ExitCode {
			get { return exitCode; }
		}

		protected virtual Encoding ResponseFileEncoding
		{
			get { return responseFileEncoding; }
		}

		protected virtual Encoding StandardErrorEncoding
		{
			get { return Console.Error.Encoding; }
		}

		protected virtual MessageImportance StandardErrorLoggingImportance {
			get { return standardErrorLoggingImportance; }
		}

		protected virtual Encoding StandardOutputEncoding
		{
			get { return Console.Out.Encoding; }
		}

		protected virtual MessageImportance StandardOutputLoggingImportance {
			get { return standardOutputLoggingImportance; }
		}

		public virtual int Timeout
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
