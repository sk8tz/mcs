//
// driver.cs: The compiler command line driver.
//
// Author: Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001, 2002, 2003 Ximian, Inc (http://www.ximian.com)
// (C) 2004, 2005 Novell, Inc
//

namespace Mono.CSharp
{
	using System;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Collections;
	using System.IO;
	using System.Text;
	using System.Globalization;
	using System.Diagnostics;

	public enum Target {
		Library, Exe, Module, WinExe
	};
	
	/// <summary>
	///    The compiler driver.
	/// </summary>
	public class Driver
	{
		
		//
		// Assemblies references to be linked.   Initialized with
		// mscorlib.dll here.
		static ArrayList references;

		//
		// If any of these fail, we ignore the problem.  This is so
		// that we can list all the assemblies in Windows and not fail
		// if they are missing on Linux.
		//
		static ArrayList soft_references;

		//
		// Modules to be linked
		//
		static ArrayList modules;

		// Lookup paths
		static ArrayList link_paths;

		// Whether we want to only run the tokenizer
		static bool tokenize = false;
		
		static string first_source;

		static bool want_debugging_support = false;

		static bool parse_only = false;
		static bool timestamps = false;
		static bool pause = false;
		static bool show_counters = false;
		
		//
		// Whether to load the initial config file (what CSC.RSP has by default)
		// 
		static bool load_default_config = true;

		//
		// A list of resource files
		//
		static ArrayList resources;
		static ArrayList embedded_resources;
		static string win32ResourceFile;
		static string win32IconFile;

		//
		// An array of the defines from the command line
		//
		static ArrayList defines;

		//
		// Output file
		//
		static string output_file = null;

		//
		// Last time we took the time
		//
		static DateTime last_time, first_time;

		//
		// Encoding.
		//
		static Encoding default_encoding;
		static Encoding encoding;


		static public void Reset ()
		{
			want_debugging_support = false;
			parse_only = false;
			timestamps = false;
			pause = false;
			show_counters = false;
			load_default_config = true;
			resources = embedded_resources = null;
			win32ResourceFile = win32IconFile = null;
			defines = null;
			output_file = null;
			encoding = default_encoding = null;
			first_source = null;
		}

		public static void ShowTime (string msg)
		{
			if (!timestamps)
				return;

			DateTime now = DateTime.Now;
			TimeSpan span = now - last_time;
			last_time = now;

			Console.WriteLine (
				"[{0:00}:{1:000}] {2}",
				(int) span.TotalSeconds, span.Milliseconds, msg);
		}

		public static void ShowTotalTime (string msg)
		{
			if (!timestamps)
				return;

			DateTime now = DateTime.Now;
			TimeSpan span = now - first_time;
			last_time = now;

			Console.WriteLine (
				"[{0:00}:{1:000}] {2}",
				(int) span.TotalSeconds, span.Milliseconds, msg);
		}	       
	       
		static void tokenize_file (SourceFile file)
		{
			Stream input;

			try {
				input = File.OpenRead (file.Name);
			} catch {
				Report.Error (2001, "Source file `" + file.Name + "' could not be found");
				return;
			}

			using (input){
				SeekableStreamReader reader = new SeekableStreamReader (input, encoding);
				Tokenizer lexer = new Tokenizer (reader, file, defines);
				int token, tokens = 0, errors = 0;

				while ((token = lexer.token ()) != Token.EOF){
					tokens++;
					if (token == Token.ERROR)
						errors++;
				}
				Console.WriteLine ("Tokenized: " + tokens + " found " + errors + " errors");
			}
			
			return;
		}

		// MonoTODO("Change error code for aborted compilation to something reasonable")]		
		static void parse (SourceFile file)
		{
			CSharpParser parser;
			Stream input;

			try {
				input = File.OpenRead (file.Name);
			} catch {
				Report.Error (2001, "Source file `" + file.Name + "' could not be found");
				return;
			}

			SeekableStreamReader reader = new SeekableStreamReader (input, encoding);

			// Check 'MZ' header
			if (reader.Read () == 77 && reader.Read () == 90) {
				Report.Error (2015, "Source file `{0}' is a binary file and not a text file", file.Name);
				input.Close ();
				return;
			}

			reader.Position = 0;
			parser = new CSharpParser (reader, file, defines);
			parser.ErrorOutput = Report.Stderr;
			try {
				parser.parse ();
			} catch (Exception ex) {
				Report.Error(666, "Compilation aborted: " + ex);
			} finally {
				input.Close ();
			}
		}

		static void OtherFlags ()
		{
			Console.WriteLine (
				"Other flags in the compiler\n" +
				"   --fatal            Makes errors fatal\n" +
				"   --parse            Only parses the source file\n" +
				"   --stacktrace       Shows stack trace at error location\n" +
				"   --timestamp        Displays time stamps of various compiler events\n" +
				"   --expect-error X   Expect that error X will be encountered\n" +
				"   -2                 Enables experimental C# features\n" +
				"   -v                 Verbose parsing (for debugging the parser)\n" + 
				"   --mcs-debug X      Sets MCS debugging level to X\n");
		}
		
		static void Usage ()
		{
			Console.WriteLine (
				"Mono C# compiler, (C) 2001 - 2005 Novell, Inc.\n" +
				"mcs [options] source-files\n" +
				"   --about            About the Mono C# compiler\n" +
				"   -addmodule:MODULE  Adds the module to the generated assembly\n" + 
				"   -checked[+|-]      Set default context to checked\n" +
				"   -codepage:ID       Sets code page to the one in ID (number, utf8, reset)\n" +
				"   -clscheck[+|-]     Disables CLS Compliance verifications" + Environment.NewLine +
				"   -define:S1[;S2]    Defines one or more symbols (short: /d:)\n" +
				"   -debug[+|-], -g    Generate debugging information\n" + 
				"   -delaysign[+|-]    Only insert the public key into the assembly (no signing)\n" +
				"   -doc:FILE          XML Documentation file to generate\n" + 
				"   -keycontainer:NAME The key pair container used to strongname the assembly\n" +
				"   -keyfile:FILE      The strongname key file used to strongname the assembly\n" +
				"   -langversion:TEXT  Specifies language version modes: ISO-1 or Default\n" + 
				"   -lib:PATH1,PATH2   Adds the paths to the assembly link path\n" +
				"   -main:class        Specified the class that contains the entry point\n" +
				"   -noconfig[+|-]     Disables implicit references to assemblies\n" +
				"   -nostdlib[+|-]     Does not load core libraries\n" +
				"   -nowarn:W1[,W2]    Disables one or more warnings\n" + 
				"   -optimize[+|-]     Enables code optimalizations\n" + 
				"   -out:FNAME         Specifies output file\n" +
				"   -pkg:P1[,Pn]       References packages P1..Pn\n" + 
				"   -recurse:SPEC      Recursively compiles the files in SPEC ([dir]/file)\n" + 
				"   -reference:ASS     References the specified assembly (-r:ASS)\n" +
				"   -target:KIND       Specifies the target (KIND is one of: exe, winexe,\n" +
				"                      library, module), (short: /t:)\n" +
				"   -unsafe[+|-]       Allows unsafe code\n" +
				"   -warnaserror[+|-]  Treat warnings as errors\n" +
				"   -warn:LEVEL        Sets warning level (the highest is 4, the default is 2)\n" +
				"   -help2             Show other help flags\n" + 
				"\n" +
				"Resources:\n" +
				"   -linkresource:FILE[,ID] Links FILE as a resource\n" +
				"   -resource:FILE[,ID]     Embed FILE as a resource\n" +
				"   -win32res:FILE          Specifies Win32 resource file (.res)\n" +
				"   -win32icon:FILE         Use this icon for the output\n" +
                                "   @file                   Read response file for more options\n\n" +
				"Options can be of the form -option or /option");
		}

		static void TargetUsage ()
		{
			Report.Error (2019, "Invalid target type for -target. Valid options are `exe', `winexe', `library' or `module'");
		}
		
		static void About ()
		{
			Console.WriteLine (
				"The Mono C# compiler is (C) 2001-2005, Novell, Inc.\n\n" +
				"The compiler source code is released under the terms of the GNU GPL\n\n" +

				"For more information on Mono, visit the project Web site\n" +
				"   http://www.go-mono.com\n\n" +

				"The compiler was written by Miguel de Icaza, Ravi Pratap, Martin Baulig, Marek Safar, Raja R Harinath");
			Environment.Exit (0);
		}

		public static int counter1, counter2;
		
		public static int Main (string[] args)
		{
			Location.InEmacs = Environment.GetEnvironmentVariable ("EMACS") == "t";

			bool ok = MainDriver (args);
			
			if (ok && Report.Errors == 0) {
				if (Report.Warnings > 0) {
					Console.WriteLine ("Compilation succeeded - {0} warning(s)", Report.Warnings);
				}
				if (show_counters){
					Console.WriteLine ("Counter1: " + counter1);
					Console.WriteLine ("Counter2: " + counter2);
				}
				if (pause)
					Console.ReadLine ();
				return 0;
			} else {
				Console.WriteLine("Compilation failed: {0} error(s), {1} warnings",
					Report.Errors, Report.Warnings);
				return 1;
			}
		}

		static public void LoadAssembly (string assembly, bool soft)
		{
			Assembly a;
			string total_log = "";

			try {
				char[] path_chars = { '/', '\\' };

				if (assembly.IndexOfAny (path_chars) != -1) {
					a = Assembly.LoadFrom (assembly);
				} else {
					string ass = assembly;
					if (ass.EndsWith (".dll") || ass.EndsWith (".exe"))
						ass = assembly.Substring (0, assembly.Length - 4);
					a = Assembly.Load (ass);
				}
				TypeManager.AddAssembly (a);

			} catch (FileNotFoundException){
				foreach (string dir in link_paths){
					string full_path = Path.Combine (dir, assembly);
					if (!assembly.EndsWith (".dll") && !assembly.EndsWith (".exe"))
						full_path += ".dll";

					try {
						a = Assembly.LoadFrom (full_path);
						TypeManager.AddAssembly (a);
						return;
					} catch (FileNotFoundException ff) {
						total_log += ff.FusionLog;
						continue;
					}
				}
				if (!soft) {
					Report.Error (6, "Cannot find assembly `" + assembly + "'" );
					Console.WriteLine ("Log: \n" + total_log);
				}
			} catch (BadImageFormatException f) {
				Report.Error(6, "Cannot load assembly (bad file format)" + f.FusionLog);
			} catch (FileLoadException f){
				Report.Error(6, "Cannot load assembly " + f.FusionLog);
			} catch (ArgumentNullException){
				Report.Error(6, "Cannot load assembly (null argument)");
			}
		}

		static public void LoadModule (MethodInfo adder_method, string module)
		{
			Module m;
			string total_log = "";

			try {
				try {
					m = (Module)adder_method.Invoke (CodeGen.Assembly.Builder, new object [] { module });
				}
				catch (TargetInvocationException ex) {
					throw ex.InnerException;
				}
				TypeManager.AddModule (m);

			} 
			catch (FileNotFoundException){
				foreach (string dir in link_paths){
					string full_path = Path.Combine (dir, module);
					if (!module.EndsWith (".netmodule"))
						full_path += ".netmodule";

					try {
						try {
							m = (Module)adder_method.Invoke (CodeGen.Assembly.Builder, new object [] { full_path });
						}
						catch (TargetInvocationException ex) {
							throw ex.InnerException;
						}
						TypeManager.AddModule (m);
						return;
					} catch (FileNotFoundException ff) {
						total_log += ff.FusionLog;
						continue;
					}
				}
				Report.Error (6, "Cannot find module `" + module + "'" );
				Console.WriteLine ("Log: \n" + total_log);
			} catch (BadImageFormatException f) {
				Report.Error(6, "Cannot load module (bad file format)" + f.FusionLog);
			} catch (FileLoadException f){
				Report.Error(6, "Cannot load module " + f.FusionLog);
			} catch (ArgumentNullException){
				Report.Error(6, "Cannot load module (null argument)");
			}
		}

		/// <summary>
		///   Loads all assemblies referenced on the command line
		/// </summary>
		static public void LoadReferences ()
		{
			foreach (string r in references)
				LoadAssembly (r, false);

			foreach (string r in soft_references)
				LoadAssembly (r, true);
			
			return;
		}

		static void SetupDefaultDefines ()
		{
			defines = new ArrayList ();
			defines.Add ("__MonoCS__");
		}

		static string [] LoadArgs (string file)
		{
			StreamReader f;
			ArrayList args = new ArrayList ();
			string line;
			try {
				f = new StreamReader (file);
			} catch {
				return null;
			}

			StringBuilder sb = new StringBuilder ();
			
			while ((line = f.ReadLine ()) != null){
				int t = line.Length;

				for (int i = 0; i < t; i++){
					char c = line [i];
					
					if (c == '"' || c == '\''){
						char end = c;
						
						for (i++; i < t; i++){
							c = line [i];

							if (c == end)
								break;
							sb.Append (c);
						}
					} else if (c == ' '){
						if (sb.Length > 0){
							args.Add (sb.ToString ());
							sb.Length = 0;
						}
					} else
						sb.Append (c);
				}
				if (sb.Length > 0){
					args.Add (sb.ToString ());
					sb.Length = 0;
				}
			}

			string [] ret_value = new string [args.Count];
			args.CopyTo (ret_value, 0);

			return ret_value;
		}

		//
		// Returns the directory where the system assemblies are installed
		//
		static string GetSystemDir ()
		{
			return Path.GetDirectoryName (typeof (object).Assembly.Location);
		}

		//
		// Given a path specification, splits the path from the file/pattern
		//
		static void SplitPathAndPattern (string spec, out string path, out string pattern)
		{
			int p = spec.LastIndexOf ('/');
			if (p != -1){
				//
				// Windows does not like /file.cs, switch that to:
				// "\", "file.cs"
				//
				if (p == 0){
					path = "\\";
					pattern = spec.Substring (1);
				} else {
					path = spec.Substring (0, p);
					pattern = spec.Substring (p + 1);
				}
				return;
			}

			p = spec.LastIndexOf ('\\');
			if (p != -1){
				path = spec.Substring (0, p);
				pattern = spec.Substring (p + 1);
				return;
			}

			path = ".";
			pattern = spec;
		}

		static void ProcessFile (string f)
		{
			if (first_source == null)
				first_source = f;

			Location.AddFile (f);
		}

		static void ProcessFiles ()
		{
			Location.Initialize ();

			foreach (SourceFile file in Location.SourceFiles) {
				if (tokenize) {
					tokenize_file (file);
				} else {
					parse (file);
				}
			}
		}

		static void CompileFiles (string spec, bool recurse)
		{
			string path, pattern;

			SplitPathAndPattern (spec, out path, out pattern);
			if (pattern.IndexOf ('*') == -1){
				ProcessFile (spec);
				return;
			}

			string [] files = null;
			try {
				files = Directory.GetFiles (path, pattern);
			} catch (System.IO.DirectoryNotFoundException) {
				Report.Error (2001, "Source file `" + spec + "' could not be found");
				return;
			} catch (System.IO.IOException){
				Report.Error (2001, "Source file `" + spec + "' could not be found");
				return;
			}
			foreach (string f in files) {
				ProcessFile (f);
			}

			if (!recurse)
				return;
			
			string [] dirs = null;

			try {
				dirs = Directory.GetDirectories (path);
			} catch {
			}
			
			foreach (string d in dirs) {
					
				// Don't include path in this string, as each
				// directory entry already does
				CompileFiles (d + "/" + pattern, true);
			}
		}

		static void DefineDefaultConfig ()
		{
			//
			// For now the "default config" is harcoded into the compiler
			// we can move this outside later
			//
			string [] default_config = {
				"System",
				"System.Xml",
#if false
				//
				// Is it worth pre-loading all this stuff?
				//
				"Accessibility",
				"System.Configuration.Install",
				"System.Data",
				"System.Design",
				"System.DirectoryServices",
				"System.Drawing.Design",
				"System.Drawing",
				"System.EnterpriseServices",
				"System.Management",
				"System.Messaging",
				"System.Runtime.Remoting",
				"System.Runtime.Serialization.Formatters.Soap",
				"System.Security",
				"System.ServiceProcess",
				"System.Web",
				"System.Web.RegularExpressions",
				"System.Web.Services",
				"System.Windows.Forms"
#endif
			};
			
			int p = 0;
			foreach (string def in default_config)
				soft_references.Insert (p++, def);
		}

		public static string OutputFile
		{
			set {
				output_file = value;
			}
			get {
				return Path.GetFileName (output_file);
			}
		}

		static void SetWarningLevel (string s)
		{
			int level = -1;

			try {
				level = Int32.Parse (s);
			} catch {
			}
			if (level < 0 || level > 4){
				Report.Error (1900, "Warning level must be in the range 0-4");
				return;
			}
			RootContext.WarningLevel = level;
		}

		static void SetupV2 ()
		{
			RootContext.Version = LanguageVersion.Default;
			defines.Add ("__V2__");
		}
		
		static void Version ()
		{
			string version = Assembly.GetExecutingAssembly ().GetName ().Version.ToString ();
			Console.WriteLine ("Mono C# compiler version {0}", version);
			Environment.Exit (0);
		}
		
		//
		// Currently handles the Unix-like command line options, but will be
		// deprecated in favor of the CSCParseOption, which will also handle the
		// options that start with a dash in the future.
		//
		static bool UnixParseOption (string arg, ref string [] args, ref int i)
		{
			switch (arg){
			case "-v":
				CSharpParser.yacc_verbose_flag++;
				return true;

			case "--version":
				Version ();
				return true;
				
			case "--parse":
				parse_only = true;
				return true;
				
			case "--main": case "-m":
				Report.Warning (-29, "Compatibility: Use -main:CLASS instead of --main CLASS or -m CLASS");
				if ((i + 1) >= args.Length){
					Usage ();
					Environment.Exit (1);
				}
				RootContext.MainClass = args [++i];
				return true;
				
			case "--unsafe":
				Report.Warning (-29, "Compatibility: Use -unsafe instead of --unsafe");
				RootContext.Unsafe = true;
				return true;
				
			case "/?": case "/h": case "/help":
			case "--help":
				Usage ();
				Environment.Exit (0);
				return true;

			case "--define":
				Report.Warning (-29, "Compatibility: Use -d:SYMBOL instead of --define SYMBOL");
				if ((i + 1) >= args.Length){
					Usage ();
					Environment.Exit (1);
				}
				defines.Add (args [++i]);
				return true;

			case "--show-counters":
				show_counters = true;
				return true;
				
			case "--expect-error": {
				int code = 0;
				
				try {
					code = Int32.Parse (
						args [++i], NumberStyles.AllowLeadingSign);
					Report.ExpectedError = code;
				} catch {
					Report.Error (-14, "Invalid number specified");
				} 
				return true;
			}
				
			case "--tokenize": 
				tokenize = true;
				return true;
				
			case "-o": 
			case "--output":
				Report.Warning (-29, "Compatibility: Use -out:FILE instead of --output FILE or -o FILE");
				if ((i + 1) >= args.Length){
					Usage ();
					Environment.Exit (1);
				}
				OutputFile = args [++i];
				return true;

			case "--checked":
				Report.Warning (-29, "Compatibility: Use -checked instead of --checked");
				RootContext.Checked = true;
				return true;
				
			case "--stacktrace":
				Report.Stacktrace = true;
				return true;
				
			case "--linkresource":
			case "--linkres":
				Report.Warning (-29, "Compatibility: Use -linkres:VALUE instead of --linkres VALUE");
				if ((i + 1) >= args.Length){
					Usage ();
					Report.Error (5, "Missing argument to --linkres"); 
					Environment.Exit (1);
				}
				if (resources == null)
					resources = new ArrayList ();
				
				resources.Add (args [++i]);
				return true;
				
			case "--resource":
			case "--res":
				Report.Warning (-29, "Compatibility: Use -res:VALUE instead of --res VALUE");
				if ((i + 1) >= args.Length){
					Usage ();
					Report.Error (5, "Missing argument to --resource"); 
					Environment.Exit (1);
				}
				if (embedded_resources == null)
					embedded_resources = new ArrayList ();
				
				embedded_resources.Add (args [++i]);
				return true;
				
			case "--target":
				Report.Warning (-29, "Compatibility: Use -target:KIND instead of --target KIND");
				if ((i + 1) >= args.Length){
					Environment.Exit (1);
					return true;
				}
				
				string type = args [++i];
				switch (type){
				case "library":
					RootContext.Target = Target.Library;
					RootContext.TargetExt = ".dll";
					break;
					
				case "exe":
					RootContext.Target = Target.Exe;
					break;
					
				case "winexe":
					RootContext.Target = Target.WinExe;
					break;
					
				case "module":
					RootContext.Target = Target.Module;
					RootContext.TargetExt = ".dll";
					break;
				default:
					TargetUsage ();
					break;
				}
				return true;
				
			case "-r":
				Report.Warning (-29, "Compatibility: Use -r:LIBRARY instead of -r library");
				if ((i + 1) >= args.Length){
					Usage ();
					Environment.Exit (1);
				}
				
				references.Add (args [++i]);
				return true;
				
			case "-L":
				Report.Warning (-29, "Compatibility: Use -lib:ARG instead of --L arg");
				if ((i + 1) >= args.Length){
					Usage ();	
					Environment.Exit (1);
				}
				link_paths.Add (args [++i]);
				return true;
				
			case "--nostdlib":
				Report.Warning (-29, "Compatibility: Use -nostdlib instead of --nostdlib");
				RootContext.StdLib = false;
				return true;
				
			case "--fatal":
				Report.Fatal = true;
				return true;
				
			case "--werror":
				Report.Warning (-29, "Compatibility: Use -warnaserror: option instead of --werror");
				Report.WarningsAreErrors = true;
				return true;
				
			case "--nowarn":
				Report.Warning (-29, "Compatibility: Use -nowarn instead of --nowarn");
				if ((i + 1) >= args.Length){
					Usage ();
					Environment.Exit (1);
				}
				int warn = 0;
				
				try {
					warn = Int32.Parse (args [++i]);
				} catch {
					Usage ();
					Environment.Exit (1);
				}
				Report.SetIgnoreWarning (warn);
				return true;
				
			case "--wlevel":
				Report.Warning (-29, "Compatibility: Use -warn:LEVEL instead of --wlevel LEVEL");
				if ((i + 1) >= args.Length){
					Report.Error (
						1900,
						"--wlevel requires a value from 0 to 4");
					Environment.Exit (1);
				}

				SetWarningLevel (args [++i]);
				return true;

			case "--mcs-debug":
				if ((i + 1) >= args.Length){
					Report.Error (5, "--mcs-debug requires an argument");
					Environment.Exit (1);
				}

				try {
					Report.DebugFlags = Int32.Parse (args [++i]);
				} catch {
					Report.Error (5, "Invalid argument to --mcs-debug");
					Environment.Exit (1);
				}
				return true;
				
			case "--about":
				About ();
				return true;
				
			case "--recurse":
				Report.Warning (-29, "Compatibility: Use -recurse:PATTERN option instead --recurse PATTERN");
				if ((i + 1) >= args.Length){
					Report.Error (5, "--recurse requires an argument");
					Environment.Exit (1);
				}
				CompileFiles (args [++i], true); 
				return true;
				
			case "--timestamp":
				timestamps = true;
				last_time = first_time = DateTime.Now;
				return true;

			case "--pause":
				pause = true;
				return true;
				
			case "--debug": case "-g":
				Report.Warning (-29, "Compatibility: Use -debug option instead of -g or --debug");
				want_debugging_support = true;
				return true;
				
			case "--noconfig":
				Report.Warning (-29, "Compatibility: Use -noconfig option instead of --noconfig");
				load_default_config = false;
				return true;
			}

			return false;
		}

		//
		// This parses the -arg and /arg options to the compiler, even if the strings
		// in the following text use "/arg" on the strings.
		//
		static bool CSCParseOption (string option, ref string [] args, ref int i)
		{
			int idx = option.IndexOf (':');
			string arg, value;

			if (idx == -1){
				arg = option;
				value = "";
			} else {
				arg = option.Substring (0, idx);

				value = option.Substring (idx + 1);
			}

			switch (arg){
			case "/nologo":
				return true;

			case "/t":
			case "/target":
				switch (value){
				case "exe":
					RootContext.Target = Target.Exe;
					break;

				case "winexe":
					RootContext.Target = Target.WinExe;
					break;

				case "library":
					RootContext.Target = Target.Library;
					RootContext.TargetExt = ".dll";
					break;

				case "module":
					RootContext.Target = Target.Module;
					RootContext.TargetExt = ".netmodule";
					break;

				default:
					TargetUsage ();
					break;
				}
				return true;

			case "/out":
				if (value == ""){
					Usage ();
					Environment.Exit (1);
				}
				OutputFile = value;
				return true;

			case "/optimize":
			case "/optimize+":
				RootContext.Optimize = true;
				return true;

			case "/optimize-":
				RootContext.Optimize = false;
				return true;

			case "/incremental":
			case "/incremental+":
			case "/incremental-":
				// nothing.
				return true;

			case "/d":
			case "/define": {
				string [] defs;

				if (value == ""){
					Usage ();
					Environment.Exit (1);
				}

				defs = value.Split (new Char [] {';', ','});
				foreach (string d in defs){
					defines.Add (d);
				}
				return true;
			}

			case "/bugreport":
				//
				// We should collect data, runtime, etc and store in the file specified
				//
				Console.WriteLine ("To file bug reports, please visit: http://www.mono-project.com/Bugs");
				return true;

			case "/linkres":
			case "/linkresource":
				if (value == ""){
					Report.Error (5, arg + " requires an argument");
					Environment.Exit (1);
				}
				if (resources == null)
					resources = new ArrayList ();
				
				resources.Add (value);
				return true;

			case "/pkg": {
				string packages;

				if (value == ""){
					Usage ();
					Environment.Exit (1);
				}
				packages = String.Join (" ", value.Split (new Char [] { ';', ',', '\n', '\r'}));
				
				ProcessStartInfo pi = new ProcessStartInfo ();
				pi.FileName = "pkg-config";
				pi.RedirectStandardOutput = true;
				pi.UseShellExecute = false;
				pi.Arguments = "--libs " + packages;
				Process p = null;
				try {
					p = Process.Start (pi);
				} catch (Exception e) {
					Report.Error (-27, "Couldn't run pkg-config: " + e.Message);
					Environment.Exit (1);
				}

				if (p.StandardOutput == null){
					Report.Warning (-27, "Specified package did not return any information");
					return true;
				}
				string pkgout = p.StandardOutput.ReadToEnd ();
				p.WaitForExit ();
				if (p.ExitCode != 0) {
					Report.Error (-27, "Error running pkg-config. Check the above output.");
					Environment.Exit (1);
				}

				if (pkgout != null){
					string [] xargs = pkgout.Trim (new Char [] {' ', '\n', '\r', '\t'}).
						Split (new Char [] { ' ', '\t'});
					args = AddArgs (args, xargs);
				}
				
				p.Close ();
				return true;
			}
				
			case "/res":
			case "/resource":
				if (value == ""){
					Report.Error (5, "-resource requires an argument");
					Environment.Exit (1);
				}
				if (embedded_resources == null)
					embedded_resources = new ArrayList ();
				
				if (embedded_resources.Contains (value)) {
					Report.Error (1508, String.Format ("The resource identifier `{0}' has already been used in this assembly.", value));
				}
				else if (value.IndexOf (',') != -1 && embedded_resources.Contains (value.Split (',')[1])) {
					Report.Error (1508, String.Format ("The resource identifier `{0}' has already been used in this assembly.", value));
				}
				else {
					embedded_resources.Add (value);
				}
				return true;
				
			case "/recurse":
				if (value == ""){
					Report.Error (5, "-recurse requires an argument");
					Environment.Exit (1);
				}
				CompileFiles (value, true); 
				return true;

			case "/r":
			case "/reference": {
				if (value == ""){
					Report.Error (5, "-reference requires an argument");
					Environment.Exit (1);
				}

				string [] refs = value.Split (new char [] { ';', ',' });
				foreach (string r in refs){
					references.Add (r);
				}
				return true;
			}
			case "/addmodule": {
				if (value == ""){
					Report.Error (5, arg + " requires an argument");
					Environment.Exit (1);
				}

				string [] refs = value.Split (new char [] { ';', ',' });
				foreach (string r in refs){
					modules.Add (r);
				}
				return true;
			}
			case "/win32res": {
				if (value == "") {
					Report.Error (5, arg + " requires an argument");
					Environment.Exit (1);
				}

				win32ResourceFile = value;
				return true;
			}
			case "/win32icon": {
				if (value == "") {
					Report.Error (5, arg + " requires an argument");
					Environment.Exit (1);
				}

				win32IconFile = value;
				return true;
			}
			case "/doc": {
				if (value == ""){
					Report.Error (2006, arg + " requires an argument");
					Environment.Exit (1);
				}
				RootContext.Documentation = new Documentation (value);
				return true;
			}
			case "/lib": {
				string [] libdirs;
				
				if (value == ""){
					Report.Error (5, "/lib requires an argument");
					Environment.Exit (1);
				}

				libdirs = value.Split (new Char [] { ',' });
				foreach (string dir in libdirs)
					link_paths.Add (dir);
				return true;
			}

			case "/debug-":
				want_debugging_support = false;
				return true;
				
			case "/debug":
			case "/debug+":
				want_debugging_support = true;
				return true;

			case "/checked":
			case "/checked+":
				RootContext.Checked = true;
				return true;

			case "/checked-":
				RootContext.Checked = false;
				return true;

			case "/clscheck":
			case "/clscheck+":
				return true;

			case "/clscheck-":
				RootContext.VerifyClsCompliance = false;
				return true;

			case "/unsafe":
			case "/unsafe+":
				RootContext.Unsafe = true;
				return true;

			case "/unsafe-":
				RootContext.Unsafe = false;
				return true;

			case "/warnaserror":
			case "/warnaserror+":
				Report.WarningsAreErrors = true;
				return true;

			case "/warnaserror-":
				Report.WarningsAreErrors = false;
				return true;

			case "/warn":
				SetWarningLevel (value);
				return true;

			case "/nowarn": {
				string [] warns;

				if (value == ""){
					Report.Error (5, "/nowarn requires an argument");
					Environment.Exit (1);
				}
				
				warns = value.Split (new Char [] {','});
				foreach (string wc in warns){
					try {
						int warn = Int32.Parse (wc);
						if (warn < 1) {
							throw new ArgumentOutOfRangeException("warn");
						}
						Report.SetIgnoreWarning (warn);
					} catch {
						Report.Error (1904, String.Format("`{0}' is not a valid warning number", wc));
					}
				}
				return true;
			}

			case "/noconfig-":
				load_default_config = true;
				return true;
				
			case "/noconfig":
			case "/noconfig+":
				load_default_config = false;
				return true;

			case "/help2":
				OtherFlags ();
				Environment.Exit(0);
				return true;
				
			case "/help":
			case "/?":
				Usage ();
				Environment.Exit (0);
				return true;

			case "/main":
			case "/m":
				if (value == ""){
					Report.Error (5, arg + " requires an argument");					
					Environment.Exit (1);
				}
				RootContext.MainClass = value;
				return true;

			case "/nostdlib":
			case "/nostdlib+":
				RootContext.StdLib = false;
				return true;

			case "/nostdlib-":
				RootContext.StdLib = true;
				return true;

			case "/fullpaths":
				return true;

			case "/keyfile":
				if (value == String.Empty) {
					Report.Error (5, arg + " requires an argument");
					Environment.Exit (1);
				}
				RootContext.StrongNameKeyFile = value;
				return true;
			case "/keycontainer":
				if (value == String.Empty) {
					Report.Error (5, arg + " requires an argument");
					Environment.Exit (1);
				}
				RootContext.StrongNameKeyContainer = value;
				return true;
			case "/delaysign+":
				RootContext.StrongNameDelaySign = true;
				return true;
			case "/delaysign-":
				RootContext.StrongNameDelaySign = false;
				return true;

			case "/v2":
			case "/2":
				Console.WriteLine ("The compiler option -2 is obsolete. Please use /langversion instead");
				SetupV2 ();
				return true;
				
			case "/langversion":
				switch (value.ToLower (CultureInfo.InvariantCulture)) {
					case "iso-1":
						RootContext.Version = LanguageVersion.ISO_1;
						return true;

					case "default":
						SetupV2 ();
						return true;
				}
				Report.Error (1617, "Invalid option `{0}' for /langversion. It must be either `ISO-1' or `Default'", value);
				return true;

			case "/codepage":
				switch (value) {
				case "utf8":
					encoding = new UTF8Encoding();
					break;
				case "reset":
					encoding = default_encoding;
					break;
				default:
					try {
						encoding = Encoding.GetEncoding (
						Int32.Parse (value));
					} catch {
						Report.Error (2016, "Code page `{0}' is invalid or not installed", value);
					}
					break;
				}
				return true;
			}

			Report.Error (2007, "Unrecognized command-line option: `{0}'", option);
			return false;
		}

		static string [] AddArgs (string [] args, string [] extra_args)
		{
			string [] new_args;
			new_args = new string [extra_args.Length + args.Length];

			// if args contains '--' we have to take that into account
			// split args into first half and second half based on '--'
			// and add the extra_args before --
			int split_position = Array.IndexOf (args, "--");
			if (split_position != -1)
			{
				Array.Copy (args, new_args, split_position);
				extra_args.CopyTo (new_args, split_position);
				Array.Copy (args, split_position, new_args, split_position + extra_args.Length, args.Length - split_position);
			}
			else
			{
				args.CopyTo (new_args, 0);
				extra_args.CopyTo (new_args, args.Length);
			}

			return new_args;
		}
		
		/// <summary>
		///    Parses the arguments, and drives the compilation
		///    process.
		/// </summary>
		///
		/// <remarks>
		///    TODO: Mostly structured to debug the compiler
		///    now, needs to be turned into a real driver soon.
		/// </remarks>
		// [MonoTODO("Change error code for unknown argument to something reasonable")]
		internal static bool MainDriver (string [] args)
		{
			int i;
			bool parsing_options = true;

			try {
				// Latin1
				default_encoding = Encoding.GetEncoding (28591);
			} catch (Exception) {
				// iso-8859-1
				default_encoding = Encoding.GetEncoding (1252);
			}
			encoding = default_encoding;

			references = new ArrayList ();
			soft_references = new ArrayList ();
			modules = new ArrayList ();
			link_paths = new ArrayList ();

			SetupDefaultDefines ();
			
			//
			// Setup defaults
			//
			// This is not required because Assembly.Load knows about this
			// path.
			//

			Hashtable response_file_list = null;

			for (i = 0; i < args.Length; i++){
				string arg = args [i];
				if (arg == "")
					continue;
				
				if (arg.StartsWith ("@")){
					string [] extra_args;
					string response_file = arg.Substring (1);

					if (response_file_list == null)
						response_file_list = new Hashtable ();
					
					if (response_file_list.Contains (response_file)){
						Report.Error (
							1515, "Response file `" + response_file +
							"' specified multiple times");
						Environment.Exit (1);
					}
					
					response_file_list.Add (response_file, response_file);
						    
					extra_args = LoadArgs (response_file);
					if (extra_args == null){
						Report.Error (2011, "Unable to open response file: " +
							      response_file);
						return false;
					}

					args = AddArgs (args, extra_args);
					continue;
				}

				if (parsing_options){
					if (arg == "--"){
						parsing_options = false;
						continue;
					}
					
					if (arg.StartsWith ("-")){
						if (UnixParseOption (arg, ref args, ref i))
							continue;

						// Try a -CSCOPTION
						string csc_opt = "/" + arg.Substring (1);
						if (CSCParseOption (csc_opt, ref args, ref i))
							continue;
						return false;
					} else {
						// Need to skip `/home/test.cs' however /test.cs is considered as error
						if (arg [0] == '/' && (arg.Length < 2 ||  arg.IndexOf ('/', 2) == -1)){
							if (CSCParseOption (arg, ref args, ref i))
								continue;
							return false;
						}
					}
				}

				CompileFiles (arg, false); 
			}

			ProcessFiles ();

			if (tokenize)
				return true;

			//
			// This will point to the NamespaceEntry of the last file that was parsed, and may
			// not be meaningful when resolving classes from other files.  So, reset it to prevent
			// silent bugs.
			//
			RootContext.Tree.Types.NamespaceEntry = null;

			//
			// If we are an exe, require a source file for the entry point
			//
			if (RootContext.Target == Target.Exe || RootContext.Target == Target.WinExe){
				if (first_source == null){
					Report.Error (2008, "No files to compile were specified");
					return false;
				}

			}

			//
			// If there is nothing to put in the assembly, and we are not a library
			//
			if (first_source == null && embedded_resources == null && resources == null){
				Report.Error (2008, "No files to compile were specified");
				return false;
			}

			if (Report.Errors > 0)
				return false;
			
			if (parse_only)
				return true;

			//
			// Load Core Library for default compilation
			//
			if (RootContext.StdLib)
				references.Insert (0, "mscorlib");

			if (load_default_config)
				DefineDefaultConfig ();

			if (Report.Errors > 0){
				return false;
			}

			//
			// Load assemblies required
			//
			if (timestamps)
				ShowTime ("Loading references");
			link_paths.Add (GetSystemDir ());
			link_paths.Add (Directory.GetCurrentDirectory ());
			LoadReferences ();
			
			if (timestamps)
				ShowTime ("   References loaded");
			
			if (Report.Errors > 0){
				return false;
			}

			//
			// Quick hack
			//
			if (output_file == null){
				int pos = first_source.LastIndexOf ('.');

				if (pos > 0)
					output_file = first_source.Substring (0, pos) + RootContext.TargetExt;
				else
					output_file = first_source + RootContext.TargetExt;
			}

			if (!CodeGen.Init (output_file, output_file, want_debugging_support))
				return false;

			if (RootContext.Target == Target.Module) {
				PropertyInfo module_only = typeof (AssemblyBuilder).GetProperty ("IsModuleOnly", BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
				if (module_only == null) {
					Report.RuntimeMissingSupport (Location.Null, "/target:module");
					Environment.Exit (1);
				}

				MethodInfo set_method = module_only.GetSetMethod (true);
				set_method.Invoke (CodeGen.Assembly.Builder, BindingFlags.Default, null, new object[]{true}, null);
			}

			TypeManager.AddModule (CodeGen.Module.Builder);

			if (modules.Count > 0) {
				MethodInfo adder_method = typeof (AssemblyBuilder).GetMethod ("AddModule", BindingFlags.Instance|BindingFlags.NonPublic);
				if (adder_method == null) {
					Report.RuntimeMissingSupport (Location.Null, "/addmodule");
					Environment.Exit (1);
				}

				foreach (string module in modules)
					LoadModule (adder_method, module);
			}

			TypeManager.ComputeNamespaces ();
			
			//
			// Before emitting, we need to get the core
			// types emitted from the user defined types
			// or from the system ones.
			//
			if (timestamps)
				ShowTime ("Initializing Core Types");
			if (!RootContext.StdLib){
				RootContext.ResolveCore ();
				if (Report.Errors > 0)
					return false;
			}
			
			TypeManager.InitCoreTypes ();
			if (timestamps)
				ShowTime ("   Core Types done");

			CodeGen.Module.ResolveAttributes ();

			//
			// The second pass of the compiler
			//
			if (timestamps)
				ShowTime ("Resolving tree");
			RootContext.ResolveTree ();

			if (Report.Errors > 0)
				return false;
			if (timestamps)
				ShowTime ("Populate tree");
			if (!RootContext.StdLib)
				RootContext.BootCorlib_PopulateCoreTypes ();

			RootContext.PopulateTypes ();

			TypeManager.InitCodeHelpers ();

			RootContext.DefineTypes ();
			
			if (RootContext.Documentation != null &&
				!RootContext.Documentation.OutputDocComment (
					output_file))
				return false;

			//
			// Verify using aliases now
			//
			Namespace.VerifyUsing ();
			
			if (Report.Errors > 0){
				return false;
			}
			
			if (RootContext.VerifyClsCompliance) {
				CodeGen.Assembly.ResolveClsCompliance ();
				if (CodeGen.Assembly.IsClsCompliant) {
					AttributeTester.VerifyModulesClsCompliance ();
					TypeManager.LoadAllImportedTypes ();
				}
			}
			if (Report.Errors > 0)
				return false;
			
			//
			// The code generator
			//
			if (timestamps)
				ShowTime ("Emitting code");
			ShowTotalTime ("Total so far");
			RootContext.EmitCode ();
			if (timestamps)
				ShowTime ("   done");

			if (Report.Errors > 0){
				return false;
			}

			if (timestamps)
				ShowTime ("Closing types");

			RootContext.CloseTypes ();

			PEFileKinds k = PEFileKinds.ConsoleApplication;
			
			switch (RootContext.Target) {
			case Target.Library:
			case Target.Module:
				k = PEFileKinds.Dll; break;
			case Target.Exe:
				k = PEFileKinds.ConsoleApplication; break;
			case Target.WinExe:
				k = PEFileKinds.WindowApplication; break;
			}

			if (RootContext.NeedsEntryPoint) {
				MethodInfo ep = RootContext.EntryPoint;

				if (ep == null) {
					if (RootContext.MainClass != null) {
						DeclSpace main_cont = RootContext.Tree.GetDecl (MemberName.FromDotted (RootContext.MainClass));
						if (main_cont == null) {
							Report.Error (1555, "Could not find `{0}' specified for Main method", RootContext.MainClass); 
							return false;
						}

						if (!(main_cont is ClassOrStruct)) {
							Report.Error (1556, "`{0}' specified for Main method must be a valid class or struct", RootContext.MainClass);
							return false;
						}

						Report.Error (1558, main_cont.Location, "`{0}' does not have a suitable static Main method", main_cont.GetSignatureForError ());
						return false;
					}

					if (Report.Errors == 0)
						Report.Error (5001, "Program `{0}' does not contain a static `Main' method suitable for an entry point",
							output_file);
					return false;
				}

				CodeGen.Assembly.Builder.SetEntryPoint (ep, k);
			} else if (RootContext.MainClass != null) {
				Report.Error (2017, "Cannot specify -main if building a module or library");
			}

			//
			// Add the resources
			//
			if (resources != null){
				foreach (string spec in resources){
					string file, res;
					int cp;
					
					cp = spec.IndexOf (',');
					if (cp != -1){
						file = spec.Substring (0, cp);
						res = spec.Substring (cp + 1);
					} else
						file = res = spec;

					CodeGen.Assembly.Builder.AddResourceFile (res, file);
				}
			}
			
			if (embedded_resources != null){
				object[] margs = new object [2];
				Type[] argst = new Type [2];
				argst [0] = argst [1] = typeof (string);

				MethodInfo embed_res = typeof (AssemblyBuilder).GetMethod (
					"EmbedResourceFile", BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic,
					null, CallingConventions.Any, argst, null);
				
				if (embed_res == null) {
					Report.RuntimeMissingSupport (Location.Null, "Resource embedding");
				} else {
					foreach (string spec in embedded_resources) {
						int cp;

						cp = spec.IndexOf (',');
						if (cp != -1){
							margs [0] = spec.Substring (cp + 1);
							margs [1] = spec.Substring (0, cp);
						} else {
							margs [1] = spec;
							margs [0] = Path.GetFileName (spec);
						}

						if (File.Exists ((string) margs [1]))
							embed_res.Invoke (CodeGen.Assembly.Builder, margs);
						else {
							Report.Error (1566, "Can not find the resource " + margs [1]);
						}
					}
				}
			}

			//
			// Add Win32 resources
			//

			CodeGen.Assembly.Builder.DefineVersionInfoResource ();

			if (win32ResourceFile != null) {
				try {
					CodeGen.Assembly.Builder.DefineUnmanagedResource (win32ResourceFile);
				}
				catch (ArgumentException) {
					Report.Warning (0, new Location (-1), "Cannot embed win32 resources on this runtime: try the Mono runtime instead.");
				}
			}

			if (win32IconFile != null) {
				MethodInfo define_icon = typeof (AssemblyBuilder).GetMethod ("DefineIconResource", BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
				if (define_icon == null) {
					Report.Warning (0, new Location (-1), "Cannot embed icon resource on this runtime: try the Mono runtime instead.");
				}
				define_icon.Invoke (CodeGen.Assembly.Builder, new object [] { win32IconFile });
			}

			if (Report.Errors > 0)
				return false;
			
			CodeGen.Save (output_file);
			if (timestamps) {
				ShowTime ("Saved output");
				ShowTotalTime ("Total");
			}

			Timer.ShowTimers ();
			
			if (Report.ExpectedError != 0) {
				if (Report.Errors == 0) {
					Console.WriteLine ("Failed to report expected error " + Report.ExpectedError + ".\n" +
						"No other errors reported.");
					
					Environment.Exit (2);
				} else {
					Console.WriteLine ("Failed to report expected error " + Report.ExpectedError + ".\n" +
						"However, other errors were reported.");
					
					Environment.Exit (1);
				}
				
				
				return false;
			}

#if DEBUGME
			Console.WriteLine ("Size of strings held: " + DeclSpace.length);
			Console.WriteLine ("Size of strings short: " + DeclSpace.small);
#endif
			return (Report.Errors == 0);
		}
	}

	//
	// This is the only public entry point
	//
	public class CompilerCallableEntryPoint : MarshalByRefObject {
		public static bool InvokeCompiler (string [] args, TextWriter error)
		{
			Report.Stderr = error;
			try {
				return Driver.MainDriver (args) && Report.Errors == 0;
			}
			finally {
				Report.Stderr = Console.Error;
				Reset ();
			}
		}
		
		static void Reset ()
		{
			Driver.Reset ();
			Location.Reset ();
			RootContext.Reset ();
			Report.Reset ();
			TypeManager.Reset ();
			TypeHandle.Reset ();
			Namespace.Reset ();
			CodeGen.Reset ();
		}
	}
}
