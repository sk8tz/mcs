//
// ConsoleLogger.cs: Outputs to the console
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
using System.Runtime.InteropServices;
using System.IO;
using System.Security;
using Microsoft.Build.Framework;

namespace Microsoft.Build.BuildEngine {
	public class ConsoleLogger : ILogger {
	
		string		parameters;
		int		indent;
		LoggerVerbosity	verbosity;
		WriteHandler	writeHandler;
		int		errorCount;
		int		warningCount;
		DateTime		buildStart;
		bool		performanceSummary;
		bool		summary;
		
		public ConsoleLogger ()
			: this (LoggerVerbosity.Normal)
		{
		}

		public ConsoleLogger (LoggerVerbosity verbosity)
		{
			this.verbosity = verbosity;
			this.indent = 0;
			this.errorCount = 0;
			this.warningCount = 0;
			this.writeHandler += new WriteHandler (WriteHandlerFunction);
			this.performanceSummary = false;
			this.summary = true;
		}
		
		public virtual void ApplyParameter (string parameterName,
						    string parameterValue)
		{
			// FIXME: what we should do here? in msbuild it isn't
			// changing "parameters" property
		}

		public virtual void Initialize (IEventSource eventSource)
		{
                        eventSource.BuildStarted +=  new BuildStartedEventHandler (BuildStartedHandler);
                        eventSource.BuildFinished += new BuildFinishedEventHandler (BuildFinishedHandler);
                        eventSource.ProjectStarted += new ProjectStartedEventHandler (ProjectStartedHandler);
                        eventSource.ProjectFinished += new ProjectFinishedEventHandler (ProjectFinishedHandler);
                        eventSource.TargetStarted += new TargetStartedEventHandler (TargetStartedHandler);
                        eventSource.TargetFinished += new TargetFinishedEventHandler (TargetFinishedHandler);
                        eventSource.TaskStarted += new TaskStartedEventHandler (TaskStartedHandler);
                        eventSource.TaskFinished += new TaskFinishedEventHandler (TaskFinishedHandler);
                        eventSource.MessageRaised += new BuildMessageEventHandler (MessageHandler);
                        eventSource.WarningRaised += new BuildWarningEventHandler (WarningHandler);
                        eventSource.ErrorRaised += new BuildErrorEventHandler (ErrorHandler);
		}
		
		public void BuildStartedHandler (object sender, BuildStartedEventArgs args)
		{
			WriteLine ("");
			WriteLine (String.Format ("Build started {0}.", args.Timestamp));
			WriteLine ("__________________________________________________");
			buildStart = args.Timestamp;
		}
		
		public void BuildFinishedHandler (object sender, BuildFinishedEventArgs args)
		{
			if (args.Succeeded == true) {
				WriteLine ("Build succeeded.");
			} else {
				WriteLine ("Build failed.");
			}
			if (performanceSummary == true)
				;
			if (summary == true){
				TimeSpan timeElapsed = args.Timestamp - buildStart;
				WriteLine (String.Format ("\t {0} Warning(s)", warningCount));
				WriteLine (String.Format ("\t {0} Error(s)", errorCount));
				WriteLine ("");
				WriteLine (String.Format ("Time Elapsed {0}", timeElapsed));
			} 
		}

		public void ProjectStartedHandler (object sender, ProjectStartedEventArgs args)
		{
			WriteLine (String.Format ("Project \"{0}\" ({1} target(s)):", args.ProjectFile, args.TargetNames));
			WriteLine ("");
		}
		
		public void ProjectFinishedHandler (object sender, ProjectFinishedEventArgs args)
		{
			if (IsVerbosityGreaterOrEqual (LoggerVerbosity.Diagnostic)) {
				WriteLine (String.Format ("Done building project \"{0}\".", args.ProjectFile));
				WriteLine ("");
			}
		}
		
		public void TargetStartedHandler (object sender, TargetStartedEventArgs args)
		{
			WriteLine (String.Format ("Target {0}:",args.TargetName));
			indent++;
		}
		
		public void TargetFinishedHandler (object sender, TargetFinishedEventArgs args)
		{
			indent--;
			if (IsVerbosityGreaterOrEqual (LoggerVerbosity.Diagnostic))
				WriteLine (String.Format ("Done building target \"{0}\" in project \"{1}\".",
					args.TargetName, args.ProjectFile));
			WriteLine ("");
		}
		
		public void TaskStartedHandler (object sender, TaskStartedEventArgs args)
		{
			if (this.verbosity == LoggerVerbosity.Diagnostic)
				WriteLine (String.Format ("Task \"{0}\"",args.TaskName));
			indent++;
		}
		
		public void TaskFinishedHandler (object sender, TaskFinishedEventArgs args)
		{
			indent--;
			if (this.verbosity == LoggerVerbosity.Diagnostic)
				WriteLine (String.Format ("Done executing task \"{0}\"",args.TaskName));
		}
		
		public void MessageHandler (object sender, BuildMessageEventArgs args)
		{
			if (IsMessageOk (args)) {
				WriteLine (args.Message);
			}
		}
		
		public void WarningHandler (object sender, BuildWarningEventArgs args)
		{
			if (IsVerbosityGreaterOrEqual (LoggerVerbosity.Normal)) 
				WriteLineWithoutIndent (FormatWarningEvent (args));
			warningCount++;
		}
		
		public void ErrorHandler (object sender, BuildErrorEventArgs args)
		{
			if (IsVerbosityGreaterOrEqual (LoggerVerbosity.Minimal)) 
				WriteLineWithoutIndent (FormatErrorEvent (args));
			errorCount++;
		}
		
		private void WriteLine (string message)
		{
			for (int i = 0; i < indent; i++)
				Console.Write ('\t');
			writeHandler (message);
		}
		
		private void WriteLineWithoutIndent (string message)
		{
			writeHandler (message);
		}
		
		private void WriteLineWithSender (object sender, string message)
		{
			if ((string) sender == "MSBuild")
				WriteLine (message);
			else
				WriteLine ((string) sender + ": " + message);
		}
		
		private void WriteHandlerFunction (string message)
		{
			Console.WriteLine (message);
		}
		
		private void ParseParameters ()
		{
			string[] splittedParameters = parameters.Split (';');
			foreach (string s in splittedParameters ) {
				switch (s) {
				case "PerformanceSummary":
					this.performanceSummary = true;
					break;
				case "NoSummary":
					this.summary = false;
					break;
				default:
					throw new ArgumentException ("Invalid parameter.");
				}
			}
		}
		
		public virtual void Shutdown ()
		{
		}
		
		private string FormatErrorEvent (BuildErrorEventArgs args)
		{
			// FIXME: show more complicated args
			if (args.LineNumber != 0 && args.ColumnNumber != 0) {
				return String.Format ("{0}({1},{2}): {3} error {4}: {5}", args.File, args.LineNumber, args.ColumnNumber,
					args.Subcategory, args.Code, args.Message);
			} else {
				return String.Format ("{0}: {1} error {2}: {3}", args.File, args.Subcategory, args.Code,
					args.Message);
			}
		}

		private string FormatWarningEvent (BuildWarningEventArgs args)
		{
			// FIXME: show more complicated args
			if (args.LineNumber != 0 && args.ColumnNumber != 0) {
				return String.Format ("{0}({1},{2}): {3} warning {4}: {5}", args.File, args.LineNumber, args.ColumnNumber,
					args.Subcategory, args.Code, args.Message);
			} else {
				return String.Format ("{0}: {1} warning {2}: {3}", args.File, args.Subcategory, args.Code,
					args.Message);
			}
		}
		
		private bool IsMessageOk (BuildMessageEventArgs bsea)
		{
			if (bsea.Importance == MessageImportance.High && IsVerbosityGreaterOrEqual (LoggerVerbosity.Minimal)) {
				return true;
			} else if (bsea.Importance == MessageImportance.Normal && IsVerbosityGreaterOrEqual (LoggerVerbosity.Normal)) {
				return true;
			} else if (bsea.Importance == MessageImportance.Low && IsVerbosityGreaterOrEqual (LoggerVerbosity.Detailed)) {
				return true;
			} else
				return false;
		}
		
                private bool IsVerbosityGreaterOrEqual (LoggerVerbosity v)
                {
                		if (v == LoggerVerbosity.Diagnostic) {
                			return LoggerVerbosity.Diagnostic <= verbosity;
                		} else if (v == LoggerVerbosity.Detailed) {
                			return LoggerVerbosity.Detailed <= verbosity;
                		} else if (v == LoggerVerbosity.Normal) {
                			return LoggerVerbosity.Normal <= verbosity;
                		} else if (v == LoggerVerbosity.Minimal) {
                			return LoggerVerbosity.Minimal <= verbosity;
                		} else if (v == LoggerVerbosity.Quiet) {
                			return true;
                		} else
                			return false;
                }

		public string Parameters {
			get {
				return parameters;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ();
				parameters = value;
				if (parameters != String.Empty)
					ParseParameters ();
			}
		}

		public LoggerVerbosity Verbosity {
			get {
				return verbosity;
			}
			set {
				verbosity = value;
			}
		}

		protected WriteHandler WriteHandler {
			get {
				return writeHandler;
			}
			set {
				writeHandler = value;
			}
		}
	}
}

#endif
