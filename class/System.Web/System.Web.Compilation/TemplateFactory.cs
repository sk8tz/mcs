//
// System.Web.Compilation.TemplateFactory
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.UI;

namespace System.Web.Compilation
{
	class TemplateFactory
	{
		internal class PageBuilder
		{
			private StringBuilder cscOptions;
			private string csFileName;
			private string className;
			public static char dirSeparator = Path.DirectorySeparatorChar;
			private static Hashtable cachedData = new Hashtable ();
			private static Random rnd_file = new Random ();

			private PageBuilder ()
			{
			}

			internal PageBuilder (string fileName)
			{
				csFileName = fileName;

				cscOptions = new StringBuilder ();
				cscOptions.Append ("--target library ");
				cscOptions.Append ("-L . ");
				AddReference ("corlib");
				AddReference ("System");
				AddReference ("System.Data");
				AddReference ("System.Web");
				AddReference ("System.Drawing");
			}

			internal Type Build ()
			{
				string dll;

				StreamReader st_file = new StreamReader (File.OpenRead (csFileName));
				
				StringReader file_content = new StringReader (st_file.ReadToEnd ());
				st_file.Close ();
				if (GetBuildOptions (file_content) == false)
					return null;

				dll = rnd_file.Next () + Path.GetFileName (csFileName).Replace (".cs", ".dll");
				if (Compile (csFileName, dll) == true){
					Assembly assembly = Assembly.LoadFrom (dll);
					Type type = assembly.GetType ("ASP." + className);
					return type;
				}

				return null;
			}

			private static bool RunProcess (string exe, string arguments, string output_file, string script_file)
			{
				Console.WriteLine ("{0} {1}", exe, arguments);
				Console.WriteLine ("Output goes to {0}", output_file);
				Console.WriteLine ("Script file is {0}", script_file);
				Process proc = new Process ();

				proc.StartInfo.FileName = exe;
				proc.StartInfo.Arguments = arguments;
				proc.StartInfo.UseShellExecute = false;
				proc.StartInfo.RedirectStandardOutput = true;
				proc.Start ();
				string poutput = proc.StandardOutput.ReadToEnd();
				proc.WaitForExit ();
				int result = proc.ExitCode;
				proc.Close ();

				StreamWriter cmd_output = new StreamWriter (File.Create (output_file));
				cmd_output.Write (poutput);
				cmd_output.Close ();
				StreamWriter bat_output = new StreamWriter (File.Create (script_file));
				bat_output.Write (exe + " " + arguments);
				bat_output.Close ();

				return (result == 0);
			}

			private bool GetBuildOptions (StringReader genCode)
			{
				string line;
				string dll;

				while ((line = genCode.ReadLine ()) != String.Empty) {
					if (line.StartsWith ("//<class ")){
						className = GetAttributeValue (line, "name");
					} else if (line.StartsWith ("//<compileandreference ")) {
						string src = GetAttributeValue (line, "src");
						dll = src.Replace (".cs", ".dll"); //FIXME
						//File.Delete (dll);
						if (Compile (src, dll) == false){
							Console.WriteLine ("Error compiling {0}. See the output file.", src);
							return false;
						}
						AddReference (dll.Replace (".dll", ""));
					} else if (line.StartsWith ("//<reference ")) {
						dll = GetAttributeValue (line, "dll");
						AddReference (dll);
					} else if (line.StartsWith ("//<compileroptions ")) {
						string options = GetAttributeValue (line, "options");
						cscOptions.Append (" " + options + " ");
					} else {
						Console.WriteLine ("This is the build option line i get:\n" + line);
						return false;
					}
				}

				return true;
			}

			private void AddReference (string reference)
			{
				string arg = String.Format ("/r:{0} ", reference);
				cscOptions.Append (arg);
			}
			
			private string GetAttributeValue (string line, string att)
			{
				string att_start = att + "=\"";
				int begin = line.IndexOf (att_start);
				int end = line.Substring (begin + att_start.Length).IndexOf ('"');
				if (begin == -1 || end == -1)
					throw new ApplicationException ("Error in reference option:\n" + line);

				return line.Substring (begin + att_start.Length, end);
			}
			
			private bool Compile (string csName, string dllName)
			{
				cscOptions.AppendFormat ("/out:{0} ", dllName);
				cscOptions.Append (csName);

				string cmdline = cscOptions.ToString ();
				string noext = csName.Replace (".cs", "");
				string output_file = noext + "_compilation_output.txt";
				string bat_file = noext + "_compile_command.bat";
				return RunProcess ("mcs", cmdline, output_file, bat_file);
			}
		}

		internal static string CompilationOutputFileName (string fileName)
		{
			string name = "xsp_" + Path.GetFileName (fileName).Replace (".aspx", ".txt");
			return "output" + PageBuilder.dirSeparator + "output_from_compilation_" + name;
		}

		internal static string GeneratedXspFileName (string fileName)
		{
			string name = Path.GetFileName (fileName).Replace (".aspx", ".cs");
			return "output" + PageBuilder.dirSeparator + "xsp_" + name;
		}

		private TemplateFactory ()
		{
		}

		internal static Type GetTypeFromSource (string fileName)
		{
			PageBuilder builder = new PageBuilder (fileName);
			return builder.Build ();
		}
	}
}

