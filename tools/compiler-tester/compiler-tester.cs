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
		bool IsWarning (int warningNumber);
	}

	class ReflectionTester: ITester {
		MethodInfo ep;
		object[] method_arg;
		StringWriter output;
		int[] all_warnings;

		public ReflectionTester (Assembly a)
		{
			Type t = a.GetType ("Mono.CSharp.CompilerCallableEntryPoint");

			if (t == null)
				Console.Error.WriteLine ("null, huh?");

			ep = t.GetMethod ("InvokeCompiler", 
				BindingFlags.Static | BindingFlags.Public);
			if (ep == null)
				throw new MissingMethodException ("static InvokeCompiler");
			method_arg = new object [2];

			PropertyInfo pi = t.GetProperty ("AllWarningNumbers");
			all_warnings = (int[])pi.GetValue (null, null);
			Array.Sort (all_warnings);
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

		public bool IsWarning (int warningNumber)
		{
			return Array.BinarySearch (all_warnings, warningNumber) >= 0;
		}
	}

#if !NET_2_1
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

		public bool IsWarning (int warningNumber)
		{
			throw new NotImplementedException ();
		}
	}
#endif

	class TestCase
	{
		public readonly string FileName;
		public readonly string[] CompilerOptions;
		public readonly string[] Dependencies;

		public TestCase (string filename, string[] options, string[] deps)
		{
			this.FileName = filename;
			this.CompilerOptions = options;
			this.Dependencies = deps;
		}
	}

	class Checker: IDisposable
	{
		protected ITester tester;
		protected int success;
		protected int total;
		protected int ignored;
		protected int syntax_errors;
		string issue_file;
		StreamWriter log_file;
		// protected string[] compiler_options;
		// protected string[] dependencies;

		protected ArrayList tests = new ArrayList ();
		protected Hashtable test_hash = new Hashtable ();
		protected ArrayList regression = new ArrayList ();
		protected ArrayList know_issues = new ArrayList ();
		protected ArrayList ignore_list = new ArrayList ();
		protected ArrayList no_error_list = new ArrayList ();
		
		protected bool verbose; // = true;
			
		int total_known_issues;

		protected Checker (ITester tester, string log_file, string issue_file)
		{
			this.tester = tester;
			this.issue_file = issue_file;
			ReadWrongErrors (issue_file);
			this.log_file = new StreamWriter (log_file, false);
		}

		protected virtual bool GetExtraOptions (string file, out string[] compiler_options,
							out string[] dependencies)
		{
			int row = 0;
			compiler_options = null;
			dependencies = null;
			try {
				using (StreamReader sr = new StreamReader (file)) {
					String line;
					while (row++ < 3 && (line = sr.ReadLine()) != null) {
						if (!AnalyzeTestFile (file, ref row, line, ref compiler_options,
								      ref dependencies))
							return false;
					}
				}
			} catch {
				return false;
			}
			return true;
		}

		protected virtual bool AnalyzeTestFile (string file, ref int row, string line,
							ref string[] compiler_options,
							ref string[] dependencies)
		{
			const string options = "// Compiler options:";
			const string depends = "// Dependencies:";

			if (row == 1) {
				compiler_options = null;
				dependencies = null;
			}

			int index = line.IndexOf (options);
			if (index != -1) {
				compiler_options = line.Substring (index + options.Length).Trim().Split (' ');
				for (int i = 0; i < compiler_options.Length; i++)
					compiler_options[i] = compiler_options[i].TrimStart ();
			}
			index = line.IndexOf (depends);
			if (index != -1) {
				dependencies = line.Substring (index + depends.Length).Trim().Split (' ');
				for (int i = 0; i < dependencies.Length; i++)
					dependencies[i] = dependencies[i].TrimStart ();
			}

			return true;
		}

		public bool Do (string filename)
		{
			if (test_hash.Contains (filename))
				return true;

			if (ignore_list.Contains (filename)) {
				++ignored;
				LogFileLine (filename, "NOT TESTED");
				return false;
			}

			string[] compiler_options, dependencies;
			if (!GetExtraOptions (filename, out compiler_options, out dependencies)) {
				LogFileLine (filename, "ERROR");
				return false;
			}

			TestCase test = new TestCase (filename, compiler_options, dependencies);
			test_hash.Add (filename, test);

			++total;
			if (dependencies != null) {
				foreach (string dependency in dependencies) {
					if (!Do (dependency)) {
						LogFileLine (filename, "DEPENDENCY FAILED");
						return false;
					}
				}
			}

			tests.Add (test);

			return Check (test);
		}

		protected virtual bool Check (TestCase test)
		{
			string[] test_args;

			if (test.CompilerOptions != null) {
				test_args = new string [1 + test.CompilerOptions.Length];
				test.CompilerOptions.CopyTo (test_args, 0);
			} else {
				test_args = new string [1];
			}
			test_args [test_args.Length - 1] = test.FileName;

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
			total_known_issues = know_issues.Count;
		}

		public virtual void PrintSummary ()
		{
			LogLine ("Done" + Environment.NewLine);
			LogLine ("{0} test cases passed ({1:0.##%})", success, (float) (success) / (float)total);

			if (syntax_errors > 0)
				LogLine ("{0} test(s) ignored because of wrong syntax !", syntax_errors);
				
			if (ignored > 0)
				LogLine ("{0} test(s) ignored", ignored);
			
			if (total_known_issues - know_issues.Count > 0)
				LogLine ("{0} known issue(s)", total_known_issues - know_issues.Count);

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
		
		protected void LogFileLine (string file, string msg, params object [] args)
		{
			string s = file + "...\t" + string.Format (msg, args); 
			Console.WriteLine (s);
			log_file.WriteLine (s);
		}

		#region IDisposable Members

		public void Dispose()
		{
			log_file.Close ();
		}

		#endregion
	}

	class PositiveChecker: Checker
	{
		readonly string files_folder;
		readonly static object[] default_args = new object[1] { new string[] {} };
		string doc_output;

#if !NET_2_1
		ProcessStartInfo pi;
#endif
		readonly string mono;

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

#if !NET_2_1
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
#endif
		}

		protected override bool GetExtraOptions(string file, out string[] compiler_options,
							out string[] dependencies) {
			if (!base.GetExtraOptions (file, out compiler_options, out dependencies))
				return false;

			doc_output = null;
			if (compiler_options == null)
				return true;

			foreach (string one_opt in compiler_options) {
				if (one_opt.StartsWith ("-doc:")) {
					doc_output = one_opt.Split (':', '/')[1];
				}
			}
			return true;
		}

		protected override bool Check(TestCase test)
		{
			string filename = test.FileName;
			try {
				if (!base.Check (test)) {
					HandleFailure (filename, TestResult.CompileError, tester.Output);
					return false;
				}
			}
			catch (Exception e) {
				if (e.InnerException != null)
					e = e.InnerException;
				
				HandleFailure (filename, TestResult.CompileError, e.ToString ());
				return false;
			}

			// Test setup
			if (filename.EndsWith ("-lib.cs") || filename.EndsWith ("-mod.cs")) {
				if (verbose)
					LogFileLine (filename, "OK");
				--total;
				return true;
			}

			MethodInfo mi = null;
			string file = Path.Combine (files_folder, Path.GetFileNameWithoutExtension (filename) + ".exe");

			// Enable .dll only tests (no execution required)
			if (!File.Exists(file)) {
				HandleFailure (filename, TestResult.Success, null);
				return true;
			}

			try {
				mi = Assembly.LoadFile (file).EntryPoint;
			}
			catch (FileNotFoundException) {
				if (File.Exists (file)) {
					Console.WriteLine ("APPDOMAIN LIMIT REACHED");
				}
			}
			catch (Exception e) {
				HandleFailure (filename, TestResult.LoadError, e.ToString ());
				return false;
			}

			if (!ExecuteFile (mi, file, filename))
				return false;

			if (doc_output != null) {
				string ref_file = filename.Replace (".cs", "-ref.xml");
				try {
#if !NET_2_1
					XmlComparer.Compare (ref_file, doc_output);
#endif
				}
				catch (Exception e) {
					HandleFailure (filename, TestResult.XmlError, e.Message);
					return false;
				}
			}

			HandleFailure (filename, TestResult.Success, null);
			return true;
		}

#if !NET_2_1
		int ExecFile (string exe_name, string filename)
		{
			if (mono == null)
				pi.FileName = exe_name;
			else
				pi.Arguments = exe_name;

			Process p = Process.Start (pi);
			p.WaitForExit ();
			return p.ExitCode;
		}
#endif

		bool ExecuteFile (MethodInfo entry_point, string exe_name, string filename)
		{
			TextWriter stdout = Console.Out;
			TextWriter stderr = Console.Error;
			Console.SetOut (TextWriter.Null);
			Console.SetError (TextWriter.Null);
			ParameterInfo[] pi = entry_point.GetParameters ();
			object[] args = pi.Length == 0 ? null : default_args;

			object result = null;
			try {
				try {
					result = entry_point.Invoke (null, args);
				} finally {
					Console.SetOut (stdout);
					Console.SetError (stderr);
				}
			}
			catch (Exception e) {
#if !NET_2_1
				int exit_code = ExecFile (exe_name, filename);
				if (exit_code == 0) {
					LogLine ("(appdomain method failed, external executable succeeded)");
					LogLine (e.ToString ());
					return true;
				}
#endif
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
						LogFileLine (file, "FIXED ISSUE");
						return;
					}
					if (verbose)
						LogFileLine (file, "OK");
					return;

				case TestResult.CompileError:
					if (know_issues.Contains (file)) {
						LogFileLine (file, "KNOWN ISSUE (Compilation error)");
						know_issues.Remove (file);
						return;
					}
					LogFileLine (file, "REGRESSION (SUCCESS -> COMPILATION ERROR)");
					break;

				case TestResult.ExecError:
					if (know_issues.Contains (file)) {
						LogFileLine (file, "KNOWN ISSUE (Execution error)");
						know_issues.Remove (file);
						return;
					}
					LogFileLine (file, "REGRESSION (SUCCESS -> EXECUTION ERROR)");
					break;

				case TestResult.XmlError:
					if (know_issues.Contains (file)) {
						LogFileLine (file, "KNOWN ISSUE (Xml comparision error)");
						know_issues.Remove (file);
						return;
					}
					LogFileLine (file, "REGRESSION (SUCCESS -> DOCUMENTATION ERROR)");
					break;

				case TestResult.LoadError:
					LogFileLine (file, "REGRESSION (SUCCESS -> LOAD ERROR)");
					break;
			}

			if (extra != null)
				LogLine ("{0}", extra);

			regression.Add (file);
		}
	}

	class NegativeChecker: Checker
	{
		string expected_message;
		string error_message;
		bool check_msg;
		bool check_error_line;
		bool is_warning;
		IDictionary wrong_warning;

		protected enum CompilerError {
			Expected,
			Wrong,
			Missing,
			WrongMessage,
			MissingLocation,
			Duplicate
		}

		public NegativeChecker (ITester tester, string log_file, string issue_file, bool check_msg):
			base (tester, log_file, issue_file)
		{
			this.check_msg = check_msg;
			wrong_warning = new Hashtable ();
		}

		protected override bool AnalyzeTestFile (string file, ref int row, string line,
							ref string[] compiler_options,
							ref string[] dependencies)
		{
			if (row == 1) {
				expected_message = null;

				int index = line.IndexOf (':');
				if (index == -1 || index > 15) {
					LogFileLine (file, "IGNORING: Wrong test file syntax (missing error mesage text)");
					++syntax_errors;
					base.AnalyzeTestFile (file, ref row, line, ref compiler_options,
							      ref dependencies);
					return false;
				}

				expected_message = line.Substring (index + 1).Trim ();
			}

			if (row == 2) {
				string filtered = line.Replace(" ", "");

				// Some error tests require to have different error text for different runtimes.
				if (filtered.StartsWith ("//GMCS")) {
					row = 1;
#if !NET_2_0
					return true;
#else
					return AnalyzeTestFile(file, ref row, line, ref compiler_options, ref dependencies);
#endif
				}

				check_error_line = !filtered.StartsWith ("//Line:0");

				if (!filtered.StartsWith ("//Line:")) {
					LogFileLine (file, "IGNORING: Wrong test syntax (following line after an error messsage must have `// Line: xx' syntax");
					++syntax_errors;
					return false;
				}
			}

			if (!base.AnalyzeTestFile (file, ref row, line, ref compiler_options, ref dependencies))
				return false;

			is_warning = false;
			if (compiler_options != null) {
				foreach (string s in compiler_options) {
					if (s.EndsWith ("warnaserror"))
						is_warning = true;
				}
			}
			return true;
		}


		protected override bool Check (TestCase test)
		{
			string filename = test.FileName;

			int start_char = 0;
			while (Char.IsLetter (filename, start_char))
				++start_char;

			int end_char = filename.IndexOfAny (new char [] { '-', '.' } );
			string expected = filename.Substring (start_char, end_char - start_char);

			try {
				if (base.Check (test)) {
					HandleFailure (filename, CompilerError.Missing);
					return false;
				}
			}
			catch (Exception e) {
				HandleFailure (filename, CompilerError.Missing);
				if (e.InnerException != null)
					e = e.InnerException;
				
				Log (e.ToString ());
				return false;
			}

			int err_id = int.Parse (expected, System.Globalization.CultureInfo.InvariantCulture);
			if (tester.IsWarning (err_id)) {
				if (!is_warning)
					wrong_warning [err_id] = true;
			} else {
				if (is_warning)
					wrong_warning [err_id] = false;
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
			ArrayList ld = new ArrayList ();
			CompilerError result = CompilerError.Missing;
			while (line != null) {
				if (ld.Contains (line)) {
					if (line.IndexOf ("Location of the symbol related to previous") == -1)
						return CompilerError.Duplicate;
				}
				ld.Add (line);

				if (result != CompilerError.Expected) {
					if (line.IndexOf (tested_text) != -1) {
						if (check_msg) {
							int first = line.IndexOf (':');
							int second = line.IndexOf (':', first + 1);
							if (second == -1 || !check_error_line)
								second = first;

							string msg = line.Substring (second + 1).TrimEnd ('.').Trim ();
							if (msg != expected_message && msg != expected_message.Replace ('`', '\'')) {
								error_message = msg;
								return CompilerError.WrongMessage;
							}

							if (check_error_line && line.IndexOf (".cs(") == -1)
								return CompilerError.MissingLocation;
						}
						result = CompilerError.Expected;
					} else if (line.IndexOf (error_prefix) != -1 &&
						line.IndexOf (ignored_error) == -1)
						result = CompilerError.Wrong;
				}

				line = sr.ReadLine ();
			}
			
			return result;
		}

		bool HandleFailure (string file, CompilerError status)
		{
			switch (status) {
				case CompilerError.Expected:
					if (know_issues.Contains (file) || no_error_list.Contains (file)) {
						LogFileLine (file, "FIXED ISSUE");
						return true;
					}
				
					if (verbose)
						LogFileLine (file, "OK");
					return true;

				case CompilerError.Wrong:
					if (know_issues.Contains (file)) {
						LogFileLine (file, "KNOWN ISSUE (Wrong error reported)");
						know_issues.Remove (file);
						return false;
					}
					if (no_error_list.Contains (file)) {
						LogFileLine (file, "REGRESSION (NO ERROR -> WRONG ERROR CODE)");
						no_error_list.Remove (file);
					}
					else {
						LogFileLine (file, "REGRESSION (CORRECT ERROR -> WRONG ERROR CODE)");
					}
					break;

				case CompilerError.WrongMessage:
					if (know_issues.Contains (file)) {
						LogFileLine (file, "KNOWN ISSUE (Wrong error message reported)");
						know_issues.Remove (file);
						return false;
					}
					if (no_error_list.Contains (file)) {
						LogFileLine (file, "REGRESSION (NO ERROR -> WRONG ERROR MESSAGE)");
						no_error_list.Remove (file);
					}
					else {
						LogFileLine (file, "REGRESSION (CORRECT ERROR -> WRONG ERROR MESSAGE)");
						LogLine ("Exp: {0}", expected_message);
						LogLine ("Was: {0}", error_message);
					}
					break;

				case CompilerError.Missing:
					if (no_error_list.Contains (file)) {
						LogFileLine (file, "KNOWN ISSUE (No error reported)");
						no_error_list.Remove (file);
						return false;
					}

					if (know_issues.Contains (file)) {
						LogFileLine (file, "REGRESSION (WRONG ERROR -> NO ERROR)");
						know_issues.Remove (file);
					}
					else {
						LogFileLine (file, "REGRESSION (CORRECT ERROR -> NO ERROR)");
					}

					break;

				case CompilerError.MissingLocation:
					if (know_issues.Contains (file)) {
						LogFileLine (file, "KNOWN ISSUE (Missing error location)");
						know_issues.Remove (file);
						return false;
					}
					if (no_error_list.Contains (file)) {
						LogFileLine (file, "REGRESSION (NO ERROR -> MISSING ERROR LOCATION)");
						no_error_list.Remove (file);
					}
					else {
						LogFileLine (file, "REGRESSION (CORRECT ERROR -> MISSING ERROR LOCATION)");
					}
					break;

				case CompilerError.Duplicate:
					// Will become an error soon
					LogFileLine (file, "WARNING: EXACTLY SAME ERROR HAS BEEN ISSUED MULTIPLE TIMES");
					return true;
			}

			regression.Add (file);
			return false;
		}

		public override void PrintSummary()
		{
			base.PrintSummary ();

			if (wrong_warning.Count > 0) {
				LogLine ("");
				LogLine ("List of incorectly defined warnings (they should be either defined in the compiler as a warning or a test-case has redundant `warnaserror' option)");
				LogLine ("");
				foreach (DictionaryEntry de in wrong_warning)
					LogLine ("CS{0:0000} : {1}", de.Key, (bool)de.Value ? "incorrect warning definition" : "missing warning definition");
			}
		}

	}

	class Tester {

		static int Main(string[] args) {
			if (args.Length != 5) {
				Console.Error.WriteLine ("Usage: TestRunner [negative|positive] test-pattern compiler know-issues log-file");
				return 1;
			}

			string mode = args[0].ToLower ();
#if NET_2_0
			string test_pattern = args [1] == "0" ? "*cs*.cs" : "*test-*.cs"; //args [1];
#else
			string test_pattern = args [1] == "0" ? "cs*.cs" : "test-*.cs"; //args [1];
#endif
			string mcs = args [2];
			string issue_file = args [3];
			string log_fname = args [4];

			string[] files = Directory.GetFiles (".", test_pattern);

			ITester tester;
			try {
				Console.WriteLine ("Testing: " + mcs);
				tester = new ReflectionTester (Assembly.LoadFile (mcs));
			}
			catch (Exception) {
#if NET_2_1
				throw;
#else
				Console.Error.WriteLine ("Switching to command line mode (compiler entry point was not found)");
				if (!File.Exists (mcs)) {
					Console.Error.WriteLine ("ERROR: Tested compiler was not found");
					return 1;
				}
				tester = new ProcessTester (mcs);
#endif
			}

			Checker checker;
			switch (mode) {
				case "negative":
					checker = new NegativeChecker (tester, log_fname, issue_file, true);
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
