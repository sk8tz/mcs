//
// driver.cs: The compiler command line driver.
//
// Author: Miguel de Icaza (miguel@gnu.org)
//
// Licensed under the terms of the GNU GPL
//
// (C) 2001 Ximian, Inc (http://www.ximian.com)
//

namespace CSC
{
	using System;
	using System.Reflection;
	using System.Reflection.Emit;
	using System.Collections;
	using System.IO;
	using CIR;
	using Generator;
	using CSC;

	/// <summary>
	///    The compiler driver.
	/// </summary>
	public class Driver
	{
		//
		// Assemblies references to be linked.   Initialized with
		// mscorlib.dll here.
		ArrayList references;

		// Lookup paths
		ArrayList link_paths;

		RootContext context; 

		bool yacc_verbose = false;

		int error_count = 0;

		public int parse (Tree context, string input_file)
		{
			CSharpParser parser;
			System.IO.Stream input;
			int errors;
			
			try {
				input = System.IO.File.OpenRead (input_file);
			} catch {
				return 1;
			}

			parser = new CSharpParser (context, input_file, input);
			parser.yacc_verbose = yacc_verbose;
			try {
				errors = parser.parse ();
			} catch (Exception ex) {
				Console.WriteLine (ex);
				Console.WriteLine ("Compilation aborted");
				return 1;
			}
			
			return errors;
		}
		
		public void Usage ()
		{
			Console.WriteLine (
				"compiler [options] source-files\n\n" +
				"-v         Verbose parsing\n"+
				"-o         Specifies output file\n" +
				"-L         Specifies path for loading assemblies\n" +
				"--nostdlib Does not load core libraries\n" +
				"-r         References an assembly\n");
			
		}

		public ITreeDump lookup_dumper (string name)
		{
			if (name == "tree")
				return new Generator.TreeDump ();
			
			//			if (name == "il")
			// return new MSIL.Generator ();
			
			return null;
		}

		public static void error (string msg)
		{
			Console.WriteLine ("Error: " + msg);
		}

		public static void notice (string msg)
		{
			Console.WriteLine (msg);
		}
		
		public static int Main(string[] args)
		{
			Driver driver = new Driver (args);

			return driver.error_count;
		}

		public int LoadAssembly (string assembly)
		{
			Assembly a;

			foreach (string dir in link_paths){
				string full_path = dir + "/" + assembly;

				try {
					a = Assembly.Load (assembly);
				} catch (FileNotFoundException f) {
					error ("// File not found: " + full_path);
					error ("Log: " + f.FusionLog);
					return 1;
				} catch (BadImageFormatException) {
					error ("// Bad file format: " + full_path);
					return 1;
				} catch (FileLoadException f){
					error ("// File Load Exception: " + full_path);
					error ("Log: " + f.FusionLog);
					return 1;
				} catch (ArgumentNullException){
					error ("// Argument Null exception " + full_path);
					return 1;
				}

				context.TypeManager.AddAssembly (a);
			}
			return 0;
		}

		// <summary>
		//   Loads all assemblies referenced on the command line
		// </summary>
		public int LoadReferences ()
		{
			int errors = 0;
			
			foreach (string r in references){
				errors += LoadAssembly (r);
			}

			return errors;
		}

		// <summary>
		//    Parses the arguments, and drives the compilation
		//    process.
		//
		//    TODO: Mostly structured to debug the compiler
		//    now, needs to be turned into a real driver soon.
		// </summary>
		public Driver (string [] args)
		{
			ITreeDump generator = null;
			int errors = 0, i;
			string output_file = null;

			context = new RootContext ();
			references = new ArrayList ();
			link_paths = new ArrayList ();

			//
			// Setup defaults
			//
			link_paths.Add ("file:///C:/WINNT/Microsoft.NET/Framework/v1.0.2914");
			
			for (i = 0; i < args.Length; i++){
				string arg = args [i];
				
				if (arg.StartsWith ("-")){
					if (arg.StartsWith ("-v")){
						yacc_verbose = true;
						continue;
					}

					if (arg.StartsWith ("-t")){
						generator = lookup_dumper (args [++i]);
						continue;
					}

					if (arg.StartsWith ("-z")){
						generator.ParseOptions (args [++i]);
						continue;
					}
					
					if (arg.StartsWith ("-o")){
						try {
							output_file = args [++i];
						} catch (Exception){
							error ("Could not write to `"+args [i]);
							error_count++;
							return;
						}
						continue;
					}

					if (arg.StartsWith ("-r")){
						references.Add (args [++i]);
						continue;
					}

					if (arg.StartsWith ("-L")){
						link_paths.Add (args [++i]);
						continue;
					}

					if (arg == "--nostdlib"){
						context.StdLib = false;
					}

					Usage ();
					error_count++;
					return;
				}
				
				if (!arg.EndsWith (".cs")){
					error ("Do not know how to compile " + arg);
					errors++;
					continue;
				}

				errors += parse (context.Tree, arg);
			}

			//
			// Load Core Library for default compilation
			//
			if (context.StdLib)
				references.Add ("mscorlib");

			if (errors > 0){
				error ("Parsing failed");
				return;
			} else
				notice ("Parsing successful");

			//
			// Load assemblies required
			//
			errors += LoadReferences ();

			if (errors > 0){
				error ("Could not load one or more assemblies");
				return;
			}


			//
			// Dumping the parsed tree.
			//
			// This code generation interface is only here
			// for debugging the parser. 
			//
			if (generator != null){
				if (output_file == null){
					error ("Error: no output file specified");
					return;
				}

				Stream output_stream = File.Create (output_file);
				StreamWriter output = new StreamWriter (output_stream);
				
				errors += generator.Dump (context.Tree, output);

				if (errors > 0){
					error ("Compilation failed");
					return;
				} else
					notice ("Compilation successful");

				output.Flush ();
				output.Close ();
			} 

			
			error_count = errors;

			//
			// Quick hack
			//
			if (output_file == null)
				output_file = "a.exe";

			context.CodeGen = new CilCodeGen (output_file, output_file);

			//
			// The second pass of the compiler
			//
			context.ResolveTree ();

			if (context.Report.Errors > 0){
				error ("Compilation failed");
				return;
			}
			
			context.CloseTypes ();
			
			context.CodeGen.Save (output_file);

			notice ("Success");
		}

	}
}



