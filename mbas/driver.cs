//
// driver.cs: The compiler command line driver.
//
// Author: Rafael Teixeira (rafaelteixeirabr@hotmail.com)
// Based on mcs by : Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2002, 2003, 2004 Rafael Teixeira
//

namespace Mono.Languages {

	using System;
	using System.Collections;
	using System.Diagnostics;
	using System.IO;
	using System.Globalization;
	using System.Reflection;
	using System.Reflection.Emit;

	using Mono.MonoBASIC;
	using Mono.GetOptions;

	enum Target {
		Library, Exe, Module, WinExe
	};
	
	enum OptionCompare {
		Binary, Text
	};
	
	/// <summary>
	///    The compiler driver.
	/// </summary>
	public class Driver : Options {
		// Temporary options
		//------------------------------------------------------------------
		[Option("[Mono] Only parses the source file (for debugging the tokenizer)", "parse", SecondLevelHelp = true)]
		public bool parse_only = false;

		[Option("[Mono] Only tokenizes source files", SecondLevelHelp = true)]
		public bool tokenize = false;

		[Option("[Mono] Shows stack trace at Error location", SecondLevelHelp = true)]
		public bool stacktrace { set { Report.Stacktrace = value; } }

		[Option("[Mono] Displays time stamps of various compiler events", SecondLevelHelp = true)]
		public bool timestamp {
			set
			{
				timestamps = true;
				last_time = DateTime.Now;
				debug_arglist.Add("timestamp");
			}
		}
		
		[Option("[Mono] Makes errors fatal", "fatal", SecondLevelHelp = true)]
		public bool Fatal { set { Report.Fatal = value; } }


		// Mono-specific options
		//------------------------------------------------------------------
		[Option("About the MonoBASIC compiler", "about")]
		public override WhatToDoNext DoAbout()
		{
			return base.DoAbout();
		}

		[Option("Show usage syntax and exit", "usage", SecondLevelHelp = true)]
		public override WhatToDoNext DoUsage()
		{
			return base.DoUsage();
		}

		[Option(-1, "[Mono] References packages listed. {packagelist}=package,...", "pkg")]
		public WhatToDoNext ReferenceSomePackage(string packageName)
		{
			return ReferencePackage(packageName)?WhatToDoNext.GoAhead:WhatToDoNext.AbandonProgram;
		}

		[Option("[Mono] Don\'t assume the standard library", "nostdlib", SecondLevelHelp = true)]
		public bool NoStandardLibraries { set { RootContext.StdLib = !value; } }

		[Option("[Mono] Disables implicit references to assemblies", "noconfig", SecondLevelHelp = true)]
		public bool NoConfig { set { load_default_config = !value; } }

		[Option("[Mono] Allows unsafe code", "unsafe", SecondLevelHelp = true)]
		public bool AllowUnsafeCode { set { RootContext.Unsafe = value; } }

		[Option("[Mono] Debugger {arguments}", "debug-args", SecondLevelHelp = true)]
		public WhatToDoNext SetDebugArgs(string args)
		{
			debug_arglist.AddRange(args.Split(','));
			return WhatToDoNext.GoAhead;
		}

		[Option("[Mono] Ignores warning number {XXXX}", "ignorewarn", SecondLevelHelp = true)]
		public WhatToDoNext SetIgnoreWarning(int warn)
		{
			Report.SetIgnoreWarning(warn);
			return WhatToDoNext.GoAhead;
		}	

		[Option("[Mono] Sets warning {level} (the highest is 4, the default)", "wlevel", SecondLevelHelp = true)]
		public int WarningLevel { set { RootContext.WarningLevel = value; } }

		// Output file options
		//------------------------------------------------------------------
		[Option("Specifies the output {file} name", 'o', "out")]
		public string OutputFileName = null;

		[Option("Specifies the target {type} for the output file (exe [default], winexe, library, module)", 't', "target")]
		public WhatToDoNext SetTarget(string type)
		{
			switch (type.ToLower()) {
				case "library":
					target = Target.Library;
					target_ext = ".dll";
					break;
							
				case "exe":
					target = Target.Exe;
					target_ext = ".exe";
					break;
							
				case "winexe":
					target = Target.WinExe;
					target_ext = ".exe";
					break;
							
				case "module":
					target = Target.Module;
					target_ext = ".netmodule";
					break;
			}
			return WhatToDoNext.GoAhead;
		}

		[Option("Specifies the {name} of the Class or Module that contains Sub Main or inherits from System.Windows.Forms.Form.\tNeeded to select among many entry-points for a program (target=exe|winexe)",
			'm', "main")]
		public string main { set { RootContext.MainClass = value; } }

		// TODO: force option to accept number in hex format
//		[Option("[NOT IMPLEMENTED YET]The base {address} for a library or module (hex)", SecondLevelHelp = true)]
		public int baseaddress;

		// input file options
		//------------------------------------------------------------------
		[Option(-1, "Imports all type information from files in the {module-list}. {module-list}:module,...", "addmodule")]
		public string AddedModule { set { foreach(string module in value.Split(',')) addedNetModules.Add(module); } }

//		[Option("[NOT IMPLEMENTED YET]Include all files in the current directory and subdirectories according to the {wildcard}", "recurse")]
		public WhatToDoNext Recurse(string wildcard)
		{
			//AddFiles (DirName, true); // TODO wrong semantics
			return WhatToDoNext.GoAhead;
		}

		[Option(-1, "References metadata from the specified {assembly}", 'r', "reference")]
		public string AddedReference { set { references.Add(value); } }
		
		[Option("List of directories to search for metadata references. {path-list}:path,...", "libpath", "lib")]
		public string AddedLibPath { set { foreach(string path in value.Split(',')) libpath.Add(path); } }

		// support for the Compact Framework
		//------------------------------------------------------------------
//		[Option("[NOT IMPLEMENTED YET]Sets the compiler to target the Compact Framework", "netcf")]
		public bool CompileForCompactFramework = false;
		
//		[Option("[NOT IMPLEMENTED YET]Specifies the {path} to the location of mscorlib.dll and microsoft.visualbasic.dll", "sdkpath")]
		public string SDKPath = null;

		// resource options
		//------------------------------------------------------------------
		public ArrayList EmbeddedResources = new ArrayList();
		
		[Option(-1, "Adds the specified {file} as an embedded assembly resource", "resource", "res")]
		public string AddedResource { set { EmbeddedResources.Add(value); } }

		public ArrayList LinkedResources = new ArrayList();
		
//		[Option(-1, "[NOT IMPLEMENTED YET]Adds the specified {file} as a linked assembly resource", "linkresource", "linkres")]
		public string AddedLinkresource { set { LinkedResources.Add(value); } }

		public ArrayList Win32Resources = new ArrayList();
		
//		[Option(-1, "[NOT IMPLEMENTED YET]Specifies a Win32 resource {file} (.res)", "win32resource")]
		public string AddedWin32resource { set { Win32Resources.Add(value); } }

		public ArrayList Win32Icons = new ArrayList();
		
//		[Option(-1, "[NOT IMPLEMENTED YET]Specifies a Win32 icon {file} (.ico) for the default Win32 resources", "win32icon")]
		public string AddedWin32icon { set { Win32Icons.Add(value); } }

		// code generation options
		//------------------------------------------------------------------

//		[Option("[NOT IMPLEMENTED YET]Enable optimizations", "optimize", VBCStyleBoolean = true)]
		public bool optimize = false;

		[Option("Remove integer checks. Default off.", SecondLevelHelp = true, VBCStyleBoolean = true)]
		public bool removeintchecks { set { RootContext.Checked = !value; } }

		[Option("Emit debugging information", 'g', "debug", VBCStyleBoolean = true)]
		public bool want_debugging_support = false;

		[Option("Emit full debugging information (default)", "debug:full", SecondLevelHelp = true)]
		public bool fullDebugging = false;

		[Option("[IGNORED] Emit PDB file only", "debug:pdbonly", SecondLevelHelp = true)]
		public bool pdbOnly = false;

		// errors and warnings options
		//------------------------------------------------------------------

		[Option("Treat warnings as errors", "warnaserror", SecondLevelHelp = true)]
		public bool WarningsAreErrors { set { Report.WarningsAreErrors = value; } }

		[Option("Disable warnings", "nowarn", SecondLevelHelp = true)]
		public bool NoWarnings { set { if (value) RootContext.WarningLevel = 0; } }


		// defines
		//------------------------------------------------------------------
		public Hashtable Defines = new Hashtable();
		
		[Option(-1, "Declares global conditional compilation symbol(s). {symbol-list}:name=value,...", 'd', "define")]
		public string define { 
			set {
				foreach(string item in value.Split(','))  {
					string[] dados = item.Split('=');
					try {
						if (dados.Length > 1)
							Defines.Add(dados[0], dados[1]); 
						else
							Defines.Add(dados[0], string.Empty);
					}
					catch  {
						Error ("Could not define symbol" + dados[0]);
					}
				}
			} 
		}
		
		// language options
		//------------------------------------------------------------------

//		[Option("[NOT IMPLEMENTED YET]Require explicit declaration of variables", VBCStyleBoolean = true)]
		public bool optionexplicit { set { Mono.MonoBASIC.Parser.InitialOptionExplicit = value; } }

//		[Option("[NOT IMPLEMENTED YET]Enforce strict language semantics", VBCStyleBoolean = true)]
		public bool optionstrict { set { Mono.MonoBASIC.Parser.InitialOptionStrict = value; } }
		
//		[Option("[NOT IMPLEMENTED YET]Specifies binary-style string comparisons. This is the default", "optioncompare:binary")]
		public bool optioncomparebinary { set { Mono.MonoBASIC.Parser.InitialOptionCompareBinary = true; } }

//		[Option("[NOT IMPLEMENTED YET]Specifies text-style string comparisons.", "optioncompare:text")]
		public bool optioncomparetext { set { Mono.MonoBASIC.Parser.InitialOptionCompareBinary = false; } }

		[Option(-1, "Declare global Imports for listed namespaces. {import-list}:namespace,...", "imports")]
		public string imports
		{
			set {
				foreach(string importedNamespace in value.Split(','))
					Mono.MonoBASIC.Parser.ImportsList.Add(importedNamespace);
			}
		}

		[Option("Specifies the root {namespace} for all type declarations", SecondLevelHelp = true)]
		public string rootnamespace { set { RootContext.RootNamespace = value; } }
		
		// Signing options	
		//------------------------------------------------------------------
//		[Option("[NOT IMPLEMENTED YET]Delay-sign the assembly using only the public portion of the strong name key", VBCStyleBoolean = true)]
		public bool delaysign;
		
//		[Option("[NOT IMPLEMENTED YET]Specifies a strong name key {container}")]
		public string keycontainer;
		
//		[Option("[NOT IMPLEMENTED YET]Specifies a strong name key {file}")]
		public string keyfile;

		// Compiler output options	
		//------------------------------------------------------------------
		
		[Option("Do not display compiler copyright banner")]
		public bool nologo = false;
		
		//TODO: Correct semantics
		[Option("Commands the compiler to show only error messages for syntax-related errors and warnings", 'q', "quiet", SecondLevelHelp = true)]
		public bool SuccintErrorDisplay = false;
		
		// TODO: semantics are different and should be adjusted
		[Option("Display verbose messages", 'v', SecondLevelHelp = true)] 
		public bool verbose	{ set { GenericParser.yacc_verbose_flag = value ? 1 : 0; } }

		[Option("[IGNORED] Emit compiler output in UTF8 character encoding", SecondLevelHelp = true, VBCStyleBoolean = true)]
		public bool utf8output;

//		[Option("[NOT IMPLEMENTED YET]Create bug report {file}")]
		public string bugreport;


		ArrayList defines = new ArrayList();
		ArrayList references = new ArrayList();
		ArrayList soft_references = new ArrayList();
		ArrayList addedNetModules = new ArrayList();
		ArrayList libpath = new ArrayList();
		
		string firstSourceFile = null;
		Target target = Target.Exe;
		string target_ext = ".exe";
		ArrayList debug_arglist = new ArrayList ();
		bool timestamps = false;
		Hashtable source_files = new Hashtable ();
		bool load_default_config = true;

		//
		// Last time we took the time
		//
		DateTime last_time;
		void ShowTime (string msg)
		{
			DateTime now = DateTime.Now;
			TimeSpan span = now - last_time;
			last_time = now;

			Console.WriteLine (
				"[{0:00}:{1:000}] {2}",
				(int) span.TotalSeconds, span.Milliseconds, msg);
		}
	       		
		public int LoadAssembly (string assembly, bool soft)
		{
			Assembly a;
			string total_log = "";

			try  {
				char[] path_chars = { '/', '\\' };

				if (assembly.IndexOfAny (path_chars) != -1)
					a = Assembly.LoadFrom(assembly);
				else {
					string ass = assembly;
					if (ass.EndsWith (".dll"))
						ass = assembly.Substring (0, assembly.Length - 4);
					a = Assembly.Load (ass);
				}
				TypeManager.AddAssembly (a);
				return 0;
			}
			catch (FileNotFoundException) {
				if (libpath != null) {
					foreach (string dir in libpath) {
						string full_path = dir + "/" + assembly + ".dll";

						try  {
							a = Assembly.LoadFrom (full_path);
							TypeManager.AddAssembly (a);
							return 0;
						} 
						catch (FileNotFoundException ff)  {
							total_log += ff.FusionLog;
							continue;
						}
					}
				}
				if (soft)
					return 0;
			}
			catch (BadImageFormatException f)  {
				Error ("// Bad file format while loading assembly");
				Error ("Log: " + f.FusionLog);
				return 1;
			} catch (FileLoadException f){
				Error ("File Load Exception: " + assembly);
				Error ("Log: " + f.FusionLog);
				return 1;
			} catch (ArgumentNullException){
				Error ("// Argument Null exception ");
				return 1;
			}
			
			Report.Error (6, "Can not find assembly `" + assembly + "'" );
			Console.WriteLine ("Log: \n" + total_log);

			return 0;
		}

		public bool ReferencePackage(string packageName)
		{
			if (packageName == ""){
				DoAbout ();
				return false;
			}
				
			ProcessStartInfo pi = new ProcessStartInfo ();
			pi.FileName = "pkg-config";
			pi.RedirectStandardOutput = true;
			pi.UseShellExecute = false;
			pi.Arguments = "--libs " + packageName;
			Process p = null;
			try {
				p = Process.Start (pi);
			} catch (Exception e) {
				Report.Error (-27, "Couldn't run pkg-config: " + e.Message);
				return false;
			}

			if (p.StandardOutput == null){
				Report.Warning (-27, "Specified package did not return any information");
			}
			string pkgout = p.StandardOutput.ReadToEnd ();
			p.WaitForExit ();
			if (p.ExitCode != 0) {
				Report.Error (-27, "Error running pkg-config. Check the above output.");
				return false;
			}
			p.Close ();
			
			if (pkgout != null) {
				string [] xargs = pkgout.Trim (new Char [] {' ', '\n', '\r', '\t'}).
					Split (new Char [] { ' ', '\t'});
				foreach(string arg in xargs) {
					string[] zargs = arg.Split(':', '=');
					try {
						if (zargs.Length > 1)
							AddedReference = zargs[1];
						else
							AddedReference = arg;
					} catch (Exception e) {
						Report.Error (-27, "Something wrong with argument (" + arg + ") in 'pkg-config --libs' output: " + e.Message);
						return false;
					}
				}
			}

			return true;
		}

		public void LoadModule (MethodInfo adder_method, string module)
		{
			System.Reflection.Module m;
			string total_log = "";

			try {
				try {
					m = (System.Reflection.Module)adder_method.Invoke (CodeGen.AssemblyBuilder, new object [] { module });
				}
				catch (TargetInvocationException ex) {
					throw ex.InnerException;
				}
				TypeManager.AddModule (m);

			} 
			catch (FileNotFoundException) {
				foreach (string dir in libpath)	{
					string full_path = Path.Combine (dir, module);
					if (!module.EndsWith (".netmodule"))
						full_path += ".netmodule";

					try {
						try {
							m = (System.Reflection.Module) adder_method.Invoke (CodeGen.AssemblyBuilder, new object [] { full_path });
						}
						catch (TargetInvocationException ex) {
							throw ex.InnerException;
						}
						TypeManager.AddModule (m);
						return;
					}
					catch (FileNotFoundException ff) {
						total_log += ff.FusionLog;
						continue;
					}
				}
				Report.Error (6, "Cannot find module `" + module + "'" );
				Console.WriteLine ("Log: \n" + total_log);
			}
			catch (BadImageFormatException f) {
				Report.Error(6, "Cannot load module (bad file format)" + f.FusionLog);
			}
			catch (FileLoadException f)	{
				Report.Error(6, "Cannot load module " + f.FusionLog);
			}
			catch (ArgumentNullException) {
				Report.Error(6, "Cannot load module (null argument)");
			}
		}

		void Error(string message)
		{
			Console.WriteLine(message);
		}

		/// <summary>
		///   Loads all assemblies referenced on the command line
		/// </summary>
		public int LoadReferences ()
		{
			int errors = 0;

			foreach (string r in references)
				errors += LoadAssembly (r, false);

			foreach (string r in soft_references)
				errors += LoadAssembly (r, true);
			
			return errors;
		}

		void SetupDefaultDefines ()
		{
			defines = new ArrayList ();
			defines.Add ("__MonoBASIC__");
		}
		
		void SetupDefaultImports()
		{
			Mono.MonoBASIC.Parser.ImportsList = new ArrayList();
			Mono.MonoBASIC.Parser.ImportsList.Add("Microsoft.VisualBasic");
		}

		//
		// Given a path specification, splits the path from the file/pattern
		//
		void SplitPathAndPattern (string spec, out string path, out string pattern)
		{
			int p = spec.LastIndexOf ("/");
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

			p = spec.LastIndexOf ("\\");
			if (p != -1){
				path = spec.Substring (0, p);
				pattern = spec.Substring (p + 1);
				return;
			}

			path = ".";
			pattern = spec;
		}

		bool AddFiles (string spec, bool recurse)
		{
			string path, pattern;

			SplitPathAndPattern(spec, out path, out pattern);
			if (pattern.IndexOf("*") == -1) {
				AddFile(spec);
				return true;
			}

			string [] files = null;
			try {
				files = Directory.GetFiles(path, pattern);
			} catch (System.IO.DirectoryNotFoundException) {
				Report.Error (2001, "Source file `" + spec + "' could not be found");
				return false;
			} catch (System.IO.IOException){
				Report.Error (2001, "Source file `" + spec + "' could not be found");
				return false;
			}
			foreach (string f in files)
				AddFile (f);

			if (!recurse)
				return true;
			
			string [] dirs = null;

			try {
				dirs = Directory.GetDirectories(path);
			} catch {
			}
			
			foreach (string d in dirs) {
					
				// Don't include path in this string, as each
				// directory entry already does
				AddFiles (d + "/" + pattern, true);
			}

			return true;
		}

		void DefineDefaultConfig ()
		{
			//
			// For now the "default config" is harcoded into the compiler
			// we can move this outside later
			//
			string [] default_config = 
			{
				"System",
				"System.Data",
				"System.Xml",
				"Microsoft.VisualBasic" , 
#if EXTRA_DEFAULT_REFS
				//
				// Is it worth pre-loading all this stuff?
				//
				"Accessibility",
				"System.Configuration.Install",
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
				"System.Web.Services" ,
				"System.Windows.Forms"
#endif
			};
			
			foreach (string def in default_config)
				if (!(references.Contains(def) || references.Contains (def + ".dll")))
					soft_references.Add(def);
		}

		[ArgumentProcessor]
		public void AddFile(string fileName)
		{
			string f = fileName;
			if (firstSourceFile == null)
				firstSourceFile = f;

			if (source_files.Contains(f))
				Report.Error(1516, "Source file '" + f + "' specified multiple times");
			else
				source_files.Add(f, f);
		}

		void ProcessSourceFile(string filename)
		{
			if (tokenize)
				GenericParser.Tokenize(filename);
			else
				GenericParser.Parse(filename);
		}

		string outputFile_Name = null;

		string outputFileName {
			get 
			{
				if (outputFile_Name == null) {
					if (OutputFileName == null) {
						int pos = firstSourceFile.LastIndexOf(".");

						if (pos > 0)
							OutputFileName = firstSourceFile.Substring(0, pos);
						else
							OutputFileName = firstSourceFile;
					}
					string bname = CodeGen.Basename(OutputFileName);
					if (bname.IndexOf(".") == -1)
						OutputFileName +=  target_ext;
					outputFile_Name = OutputFileName;
				}
				return outputFile_Name;
			}
		}

		bool ParseAll() // Phase 1
		{
			if (firstSourceFile == null) {
				Report.Error(2008, "No files to compile were specified");
				return false;
			}

			foreach(string filename in source_files.Values)
				ProcessSourceFile(filename);

			if (tokenize || parse_only || (Report.Errors > 0))
				return false;		

			return true; // everything went well go ahead
		}

		void InitializeDebuggingSupport()
		{
			string[] debug_args = new string [debug_arglist.Count];
			debug_arglist.CopyTo(debug_args);
			CodeGen.Init(outputFileName, outputFileName, want_debugging_support, debug_args);
			TypeManager.AddModule(CodeGen.ModuleBuilder);
		}

		public bool ResolveAllTypes() // Phase 2
		{
			// Load Core Library for default compilation
			if (RootContext.StdLib)
				references.Insert(0, "mscorlib");

			if (load_default_config)
				DefineDefaultConfig();

			if (timestamps)
				ShowTime("Loading references");

			// Load assemblies required
			if (LoadReferences() > 0) {
				Error ("Could not load one or more assemblies");
				return false;
			}

			if (timestamps)
				ShowTime("References loaded");

			InitializeDebuggingSupport();

			// target is Module 
			if (target == Target.Module) {
				PropertyInfo module_only = typeof (AssemblyBuilder).GetProperty ("IsModuleOnly", BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
				if (module_only == null) {
					Report.Error (0, new Location (-1, -1), "Cannot use /target:module on this runtime: try the Mono runtime instead.");
					Environment.Exit (1);
				}

				MethodInfo set_method = module_only.GetSetMethod (true);
				set_method.Invoke (CodeGen.AssemblyBuilder, BindingFlags.Default, null, new object[]{true}, null);

				TypeManager.AddModule (CodeGen.ModuleBuilder);
			}

			if (addedNetModules.Count > 0) {
				MethodInfo adder_method = typeof (AssemblyBuilder).GetMethod ("AddModule", BindingFlags.Instance|BindingFlags.NonPublic);
				if (adder_method == null) {
					Report.Error (0, new Location (-1, -1), "Cannot use /addmodule on this runtime: Try the Mono runtime instead.");
					Environment.Exit (1);
				}

				foreach (string module in addedNetModules)
					LoadModule (adder_method, module);
			}


			//
			// Before emitting, we need to get the core
			// types emitted from the user defined types
			// or from the system ones.
			//
			if (timestamps)
				ShowTime ("Initializing Core Types");

			if (!RootContext.StdLib)
				RootContext.ResolveCore ();
			if (Report.Errors > 0)
				return false;
			
			TypeManager.InitCoreTypes();
			if (Report.Errors > 0)
				return false;

			if (timestamps)
				ShowTime ("   Core Types done");

			if (timestamps)
				ShowTime ("Resolving tree");

			// The second pass of the compiler
			RootContext.ResolveTree ();
			if (Report.Errors > 0)
				return false;
			
			if (timestamps)
				ShowTime ("Populate tree");

			if (!RootContext.StdLib)
				RootContext.BootCorlib_PopulateCoreTypes();
			if (Report.Errors > 0)
				return false;

			RootContext.PopulateTypes();
			if (Report.Errors > 0)
				return false;
			
			TypeManager.InitCodeHelpers();
			if (Report.Errors > 0)
				return false;

			return true;
		}
		
		bool IsSWFApp()
		{
			string mainclass = GetFQMainClass();
			
			if (mainclass != null) {
				foreach (string r in references) {
					if (r.IndexOf ("System.Windows.Forms") >= 0) {
						Type t = TypeManager.LookupType(mainclass);
						if (t != null) 
							return t.IsSubclassOf (TypeManager.LookupType("System.Windows.Forms.Form"));
						break;	
					}	
				}
			}
			return false;
		}
		
		string GetFQMainClass()
		{	
			if (RootContext.RootNamespace != "")
				return RootContext.RootNamespace + "." + RootContext.MainClass;
			else
				return RootContext.MainClass;			
		}
		
		void FixEntryPoint()
		{
			if (target == Target.Exe || target == Target.WinExe) {
				MethodInfo ep = RootContext.EntryPoint;
			
				if (ep == null) {
					// If we don't have a valid entry point yet
					// AND if System.Windows.Forms is included
					// among the dependencies, we have to build
					// a new entry point on-the-fly. Otherwise we
					// won't be able to compile SWF code out of the box.

					if (IsSWFApp())  {
						Type t = TypeManager.LookupType(GetFQMainClass());
						if (t != null) 
						{							
							TypeBuilder tb = t as TypeBuilder;
							MethodBuilder mb = tb.DefineMethod ("Main", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, 
								typeof(void), new Type[0]);

							Type SWFA = TypeManager.LookupType("System.Windows.Forms.Application");
							Type SWFF = TypeManager.LookupType("System.Windows.Forms.Form");
							Type[] args = new Type[1];
							args[0] = SWFF;
							MethodInfo mi = SWFA.GetMethod("Run", args);
							ILGenerator ig = mb.GetILGenerator();
							ConstructorInfo ci = TypeManager.GetConstructor (TypeManager.LookupType(t.FullName), new Type[0]);
							
							ig.Emit (OpCodes.Newobj, ci);
							ig.Emit (OpCodes.Call, mi);
							ig.Emit (OpCodes.Ret);

							RootContext.EntryPoint = mb as MethodInfo;
						}
					}
				}
			}
		}

		bool GenerateAssembly()
		{
			//
			// The code generator
			//
			if (timestamps)
				ShowTime ("Emitting code");
			
			

			RootContext.EmitCode();
			FixEntryPoint();
			if (Report.Errors > 0)
				return false;

			if (timestamps)
				ShowTime ("   done");


			if (timestamps)
				ShowTime ("Closing types");

			RootContext.CloseTypes ();
			if (Report.Errors > 0)
				return false;

			if (timestamps)
				ShowTime ("   done");

			PEFileKinds k = PEFileKinds.ConsoleApplication;
							
			if (target == Target.Library || target == Target.Module)
				k = PEFileKinds.Dll;
			else if (target == Target.Exe)
				k = PEFileKinds.ConsoleApplication;
			else if (target == Target.WinExe)
				k = PEFileKinds.WindowApplication;
			
			if (target == Target.Exe || target == Target.WinExe) {
				MethodInfo ep = RootContext.EntryPoint;
			
				if (ep == null) {
					Report.Error (30737, "Program " + outputFileName +
						" does not have an entry point defined");
					return false;
				}
							
				CodeGen.AssemblyBuilder.SetEntryPoint (ep, k);
			}

			// Add the resources
			if (EmbeddedResources != null)
				foreach (string file in EmbeddedResources)
					CodeGen.AssemblyBuilder.AddResourceFile (file, file);
			
			CodeGen.Save(outputFileName);

			if (timestamps)
				ShowTime ("Saved output");

			
			if (want_debugging_support)  {
				CodeGen.SaveSymbols ();
				if (timestamps)
					ShowTime ("Saved symbols");
			}

			return true;
		}

		public void CompileAll()
		{
			try {
/* 
		    VB.NET expects the default namespace to be "" (empty string)		
		    
		    if (RootContext.RootNamespace == "") {
		      RootContext.RootNamespace = System.IO.Path.GetFileNameWithoutExtension(outputFileName);
		    }
*/
				if (!ParseAll()) // Phase 1
					return;

				if (!ResolveAllTypes()) // Phase 2
					return;

				GenerateAssembly(); // Phase 3 
				
			} catch (Exception ex) {
				Error("Exception: " + ex.ToString());
			}
		}
		
		private bool quiet { get { return nologo || SuccintErrorDisplay; } } 
		
		private void Banner()
		{
			if (!quiet) {
				ShowBanner();
				// TODO: remove next lines when the compiler has matured enough
				Console.WriteLine ("--------");
				Console.WriteLine ("THIS IS AN ALPHA SOFTWARE.");
				Console.WriteLine ("--------");
			}		
		}
		
		protected void SetupDefaults()
		{
			SetupDefaultDefines();	
			SetupDefaultImports();
			Report.Stacktrace = false;
			RootContext.Checked = true;
		}		

		/// <summary>
		///    Parses the arguments, and calls the compilation process.
		/// </summary>
		int MainDriver(string [] args)
		{
			SetupDefaults();
			ProcessArgs(args);
			
			if (firstSourceFile == null) {
				if (!quiet) 
					DoHelp();
				return 2;
			}

			Banner();			
			CompileAll();
			return Report.ProcessResults(quiet);
		}

		public static int Main (string[] args)
		{
			Driver Exec = new Driver();
			
			return Exec.MainDriver(args);
		}

	}
}
