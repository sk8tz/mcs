using System;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Collections;

namespace TestRunner {

	interface ITester
	{
		string Output { get; }
		bool Invoke (string[] args);
	}

	class ReflectionTester: ITester {
		MethodInfo ep;
		object[] method_arg;
		StringWriter output;

		public ReflectionTester (Assembly a)
		{
			ep = a.GetType ("Mono.CSharp.CompilerCallableEntryPoint").GetMethod ("InvokeCompiler", 
				BindingFlags.Static | BindingFlags.Public);
			if (ep == null)
				throw new MissingMethodException ("static InvokeCompiler");
			method_arg = new object [2];
		}

		public string Output {
			get {
				return output.GetStringBuilder ().ToString ();
			}
		}

		public bool Invoke(string[] args)
		{
			output = new StringWriter ();
			method_arg [0] = args;
			method_arg [1] = output;
			return (bool)ep.Invoke (null, method_arg);
		}
	}

	class ProcessTester: ITester
	{
		ProcessStartInfo pi;
		string output;

		public ProcessTester (string p_path)
		{
			pi = new ProcessStartInfo ();
			pi.FileName = p_path;
			pi.CreateNoWindow = true;
			pi.WindowStyle = ProcessWindowStyle.Hidden;
			pi.RedirectStandardOutput = true;
			pi.RedirectStandardError = true;
			pi.UseShellExecute = false;
		}

		public string Output {
			get {
				return output;
			}
		}

		public bool Invoke(string[] args)
		{
			StringBuilder sb = new StringBuilder ("/nologo ");
			foreach (string s in args) {
				sb.Append (s);
				sb.Append (" ");
			}
			pi.Arguments = sb.ToString ();
			Process p = Process.Start (pi);
			output = p.StandardError.ReadToEnd ();
			if (output.Length == 0)
			    output = p.StandardOutput.ReadToEnd ();
			p.WaitForExit ();
			return p.ExitCode == 0;
		}
	}

	class Checker: IDisposable
	{
		protected ITester tester;
		protected int success;
		protected int total;
		protected int ignored;
		string issue_file;
		StreamWriter log_file;
		protected string[] compiler_options;

		protected ArrayList regression = new ArrayList ();
		protected ArrayList know_issues = new ArrayList ();
		protected ArrayList ignore_list = new ArrayList ();
		protected ArrayList no_error_list = new ArrayList ();

		protected Checker (ITester tester, string log_file, string issue_file)
		{
			this.tester = tester;
			this.issue_file = issue_file;
			ReadWrongErrors (issue_file);
			this.log_file = new StreamWriter (log_file, false);
		}

		protected virtual bool GetExtraOptions (string file)
		{
			int row = 0;
			using (StreamReader sr = new StreamReader (file)) {
				String line;
				while (row++ < 3 && (line = sr.ReadLine()) != null) {
					if (!AnalyzeTestFile (row, line))
						return false;
				}
			}
			return true;
		}

		protected virtual bool AnalyzeTestFile (int row, string line)
		{
			const string options = "// Compiler options:";

			if (row == 1)
				compiler_options = null;

			int index = line.IndexOf (options);
			if (index != -1) {
				compiler_options = line.Substring (index + options.Length).Trim().Split (' ');
				for (int i = 0; i < compiler_options.Length; i++)
					compiler_options[i] = compiler_options[i].TrimStart ();
			}
			return true;
		}

		public void Do (string filename)
		{
			Log (filename);
			Log ("...\t");

			if (ignore_list.Contains (filename)) {
				++ignored;
				LogLine ("NOT TESTED");
				return;
			}

			if (!GetExtraOptions (filename)) {
				return;
			}

			++total;
			Check (filename);
		}

		protected virtual bool Check (string filename)
		{
			string[] test_args;

			if (compiler_options != null) {
				test_args = new string [1 + compiler_options.Length];
				compiler_options.CopyTo (test_args, 0);
			} else {
				test_args = new string [1];
			}
			test_args [test_args.Length - 1] = filename;

			return tester.Invoke (test_args);
		}


		void ReadWrongErrors (string file)
		{
			const string ignored = "IGNORE";
			const string no_error = "NO ERROR";

			using (StreamReader sr = new StreamReader (file)) {
				string line;
				while ((line = sr.ReadLine()) != null) {
					if (line.StartsWith ("#"))
						continue;

					ArrayList active_cont = know_issues;

					if (line.IndexOf (ignored) > 0)
						active_cont = ignore_list;
					else if (line.IndexOf (no_error) > 0)
						active_cont = no_error_list;

					string file_name = line.Split (' ')[0];
					if (file_name.Length == 0)
						continue;

					active_cont.Add (file_name);
				}
			}
		}

		public void PrintSummary ()
		{
			LogLine ("Done" + Environment.NewLine);
			LogLine ("{0} test cases passed ({1:.##%})", success, (float) (success) / (float)total);

			if (ignored > 0)
				LogLine ("{0} test cases ignored", ignored);

			know_issues.AddRange (no_error_list);
			if (know_issues.Count > 0) {
				LogLine ("");
				LogLine (issue_file + " contains {0} already fixed issues. Please remove", know_issues.Count);
				foreach (string s in know_issues)
					LogLine (s);
			}
			if (regression.Count > 0) {
				LogLine ("");
				LogLine ("The latest changes caused regression in {0} file(s)", regression.Count);
				foreach (string s in regression)
					LogLine (s);
			}
		}

		public int ResultCode
		{
			get {
				return regression.Count == 0 ? 0 : 1;
			}
		}

		protected void Log (string msg, params object [] rest)
		{
			Console.Write (msg, rest);
			log_file.Write (msg, rest);
		}

		protected void LogLine (string msg, params object [] rest)
		{
			Console.WriteLine (msg, rest);
			log_file.WriteLine (msg, rest);
		}

		#region IDisposable Members

		public void Dispose()
		{
			log_file.Close ();
		}

		#endregion
	}

	class PositiveChecker: Checker {
		readonly string files_folder;
		readonly static object[] default_args = new object[1] { new string[] {} };
		string doc_output;

		// When we cannot load assembly to domain we are automaticaly switching to process calling
		bool appdomain_limit_reached;

		ProcessStartInfo pi;
		readonly string mono;

		// This is really pain
		// This number is highly experimental on my box I am not able to load more than 1000 files to domain
		// Every files is there twice and we need some space for tests which reference assemblies
		const int MAX_TESTS_IN_DOMAIN = 420;
		int test_counter;

		protected enum TestResult {
			CompileError,
			ExecError,
			LoadError,
			XmlError,
			Success
		}

		public PositiveChecker (ITester tester, string log_file, string issue_file):
			base (tester, log_file, issue_file)
		{
			files_folder = Directory.GetCurrentDirectory ();

			pi = new ProcessStartInfo ();
			pi.CreateNoWindow = true;
			pi.WindowStyle = ProcessWindowStyle.Hidden;
			pi.RedirectStandardOutput = true;
			pi.RedirectStandardError = true;
			pi.UseShellExecute = false;

			mono = Environment.GetEnvironmentVariable ("MONO_RUNTIME");
			if (mono != null) {
				pi.FileName = mono;
			}
		}

		protected override bool GetExtraOptions(string file) {
			if (!base.GetExtraOptions (file))
				return false;

			doc_output = null;
			if (compiler_options == null)
				return true;

			foreach (string one_opt in compiler_options) {
				if (one_opt.StartsWith ("-doc:")) {
					doc_output = one_opt.Split (':')[1];
				}
			}
			return true;
		}

		protected override bool Check(string filename) {
			try {
				if (!base.Check (filename)) {
					HandleFailure (filename, TestResult.CompileError, tester.Output);
					return false;
				}
			}
			catch (Exception e) {
				HandleFailure (filename, TestResult.CompileError, e.ToString ());
				return false;
			}

			// Test setup
			if (filename.EndsWith ("-lib.cs") || filename.EndsWith ("-mod.cs")) {
				LogLine ("OK");
				--total;
				return true;
			}

			MethodInfo mi = null;
			string file = Path.Combine (files_folder, Path.GetFileNameWithoutExtension (filename) + ".exe");
			if (!appdomain_limit_reached) {
				try {
					mi = Assembly.LoadFile (file).EntryPoint;
					if (test_counter++ > MAX_TESTS_IN_DOMAIN)
						appdomain_limit_reached = true;
				}
				catch (FileNotFoundException) {
					if (File.Exists (file)) {
						Console.WriteLine ("APPDOMAIN LIMIT REACHED");
						appdomain_limit_reached = true;
					}
				}
				catch (Exception e) {
					HandleFailure (filename, TestResult.LoadError, e.ToString ());
					return false;
				}
			}

			if (appdomain_limit_reached) {
				if (!ExecuteFile (file, filename))
					return false;
			} else {
				if (!ExecuteFile (mi, filename))
					return false;
			}

			if (doc_output != null) {
				string ref_file = filename.Replace (".cs", "-ref.xml");
				try {
					XmlComparer.Compare (ref_file, doc_output);
				}
				catch (Exception e) {
					HandleFailure (filename, TestResult.XmlError, e.Message);
					return false;
				}
			}

			HandleFailure (filename, TestResult.Success, null);
			return true;
		}

		bool ExecuteFile (string exe_name, string filename)
		{
			if (mono == null)
				pi.FileName = exe_name;
			else
				pi.Arguments = exe_name;

			Process p = Process.Start (pi);
			p.WaitForExit ();

			// TODO: How can I recognize return type void ?
			if (p.ExitCode == 0)
				return true;

			HandleFailure (filename, TestResult.ExecError, "Wrong return code: " + p.ExitCode.ToString ());
			return false;
		}

		bool ExecuteFile (MethodInfo entry_point, string filename)
		{
			TextWriter standart_ouput = Console.Out;
			TextWriter standart_error = Console.Error;
			Console.SetOut (TextWriter.Null);
			Console.SetError (TextWriter.Null);
			ParameterInfo[] pi = entry_point.GetParameters ();
			object[] args = pi.Length == 0 ? null : default_args;

			object result = null;
			try {
				result = entry_point.Invoke (null, args);
				Console.SetOut (standart_ouput);
				Console.SetError (standart_error);
			}
			catch (Exception e) {
				Console.SetOut (standart_ouput);
				Console.SetError (standart_error);
				HandleFailure (filename, TestResult.ExecError, e.ToString ());
				return false;
			}

			if (result is int && (int)result != 0) {
				HandleFailure (filename, TestResult.ExecError, "Wrong return code: " + result.ToString ());
				return false;
			}
			return true;
		}

		void HandleFailure (string file, TestResult status, string extra)
		{
			switch (status) {
				case TestResult.Success:
					success++;
					if (know_issues.Contains (file)) {
						LogLine ("FIXED ISSUE");
						return;
					}
					LogLine ("OK");
					return;

				case TestResult.CompileError:
					if (know_issues.Contains (file)) {
						LogLine ("KNOWN ISSUE (Compilation error)");
						know_issues.Remove (file);
						return;
					}
					LogLine ("REGRESSION (SUCCESS -> COMPILATION ERROR)");
					break;

				case TestResult.ExecError:
					if (know_issues.Contains (file)) {
						LogLine ("KNOWN ISSUE (Execution error)");
						know_issues.Remove (file);
						return;
					}
					LogLine ("REGRESSION (SUCCESS -> EXECUTION ERROR)");
					break;

				case TestResult.XmlError:
					if (know_issues.Contains (file)) {
						LogLine ("KNOWN ISSUE (Xml comparision error)");
						know_issues.Remove (file);
						return;
					}
					LogLine ("REGRESSION (SUCCESS -> DOCUMENTATION ERROR)");
					break;

				case TestResult.LoadError:
					LogLine ("REGRESSION (SUCCESS -> LOAD ERROR)");
					break;
			}

			if (extra != null)
				LogLine (extra);

			regression.Add (file);
		}
	}

	class NegativeChecker: Checker
	{
		string expected_message;
		string error_message;

		protected enum CompilerError {
			Expected,
			Wrong,
			Missing,
			WrongMessage
		}

		public NegativeChecker (ITester tester, string log_file, string issue_file):
			base (tester, log_file, issue_file)
		{
		}

		protected override bool AnalyzeTestFile(int row, string line)
		{
			if (row == 1) {
				expected_message = null;

				int index = line.IndexOf (':');
				if (index == -1 || index > 15) {
					LogLine ("IGNORING: Wrong test file syntax (missing error mesage text)");
					++ignored;
					base.AnalyzeTestFile (row, line);
					return false;
				}

				expected_message = line.Substring (index + 1).Trim ();
			}

			return base.AnalyzeTestFile (row, line);
		}


		protected override bool Check (string filename)
		{
			int start_char = 0;
			while (Char.IsLetter (filename, start_char))
				++start_char;

			int end_char = filename.IndexOfAny (new char [] { '-', '.' } );
			string expected = filename.Substring (start_char, end_char - start_char);

			try {
				if (base.Check (filename)) {
					HandleFailure (filename, CompilerError.Missing);
					return false;
				}
			}
			catch (Exception e) {
				HandleFailure (filename, CompilerError.Missing);
				Log (e.ToString ());
				return false;
			}

			CompilerError result_code = GetCompilerError (expected, tester.Output);
			if (HandleFailure (filename, result_code)) {
				success++;
				return true;
			}

			if (result_code == CompilerError.Wrong)
				LogLine (tester.Output);

			return false;
		}

		CompilerError GetCompilerError (string expected, string buffer)
		{
			const string error_prefix = "CS";
			const string ignored_error = "error CS5001";
			string tested_text = "error " + error_prefix + expected;
			StringReader sr = new StringReader (buffer);
			string line = sr.ReadLine ();
			bool any_error = false;
			while (line != null) {

				if (line.IndexOf (tested_text) != -1) {
//					string msg = line.Substring (line.IndexOf (':', 22) + 1).TrimEnd ('.').Trim ();
//					if (msg != expected_message && msg != expected_message.Replace ('`', '\'')) {
//						error_message = msg;
//						return CompilerError.WrongMessage;
//					}
					return CompilerError.Expected;
				}

				if (line.IndexOf (error_prefix) != -1 &&
					line.IndexOf (ignored_error) == -1)
					any_error = true;

				line = sr.ReadLine ();
			}
			
			return any_error ? CompilerError.Wrong : CompilerError.Missing;
		}

		bool HandleFailure (string file, CompilerError status)
		{
			switch (status) {
				case CompilerError.Expected:
					if (know_issues.Contains (file) || no_error_list.Contains (file)) {
						LogLine ("FIXED ISSUE");
						return true;
					}
					LogLine ("OK");
					return true;

				case CompilerError.Wrong:
					if (know_issues.Contains (file)) {
						LogLine ("KNOWN ISSUE (Wrong error reported)");
						know_issues.Remove (file);
						return false;
					}
					if (no_error_list.Contains (file)) {
						LogLine ("REGRESSION (NO ERROR -> WRONG ERROR CODE)");
						no_error_list.Remove (file);
					}
					else {
						LogLine ("REGRESSION (CORRECT ERROR -> WRONG ERROR CODE)");
					}
					break;

				case CompilerError.WrongMessage:
					if (know_issues.Contains (file)) {
						LogLine ("KNOWN ISSUE (Wrong error message reported)");
						know_issues.Remove (file);
						return false;
					}
					if (no_error_list.Contains (file)) {
						LogLine ("REGRESSION (NO ERROR -> WRONG ERROR MESSAGE)");
						no_error_list.Remove (file);
					}
					else {
						LogLine ("REGRESSION (CORRECT ERROR -> WRONG ERROR MESSAGE)");
						Console.WriteLine ("E: {0}", expected_message);
						Console.WriteLine ("W: {0}", error_message);
					}
					break;

				case CompilerError.Missing:
					if (no_error_list.Contains (file)) {
						LogLine ("KNOWN ISSUE (No error reported)");
						no_error_list.Remove (file);
						return false;
					}

					if (know_issues.Contains (file)) {
						LogLine ("REGRESSION (WRONG ERROR -> NO ERROR)");
						know_issues.Remove (file);
					}
					else {
						LogLine ("REGRESSION (CORRECT ERROR -> NO ERROR)");
					}

					break;
			}

			regression.Add (file);
			return false;
		}
	}

	class Tester {

		static int Main(string[] args) {
			if (args.Length != 5) {
				Console.Error.WriteLine ("Usage: TestRunner [negative|positive] test-pattern compiler know-issues log-file");
				return 1;
			}

			string mode = args[0].ToLower ();
			string test_pattern = args [1];
			string mcs = args [2];
			string issue_file = args [3];
			string log_fname = args [4];

			string[] files = Directory.GetFiles (".", test_pattern);

			ITester tester;
			try {
				Console.WriteLine ("Loading: " + mcs);
				tester = new ReflectionTester (Assembly.LoadFile (mcs));
			}
			catch (Exception) {
				Console.Error.WriteLine ("Switching to command line mode (compiler entry point was not found)");
				if (!File.Exists (mcs)) {
					Console.Error.WriteLine ("ERROR: Tested compiler was not found");
					return 1;
				}
				tester = new ProcessTester (mcs);
			}

			Checker checker;
			switch (mode) {
				case "negative":
					checker = new NegativeChecker (tester, log_fname, issue_file);
					break;
				case "positive":
					checker = new PositiveChecker (tester, log_fname, issue_file);
					break;
				default:
					Console.Error.WriteLine ("You must specify testing mode (positive or negative)");
					return 1;
			}

			foreach (string s in files) {
				string filename = Path.GetFileName (s);
				if (Char.IsUpper (filename, 0)) { // Windows hack
					continue;
				}

				if (filename.EndsWith ("-p2.cs"))
					continue;
			    
				checker.Do (filename);
			}

			checker.PrintSummary ();

			checker.Dispose ();

			return checker.ResultCode;
		}
	}
}
