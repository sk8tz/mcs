//
// Mono.CSharp CSharpCodeCompiler Class implementation
//
// Authors:
//	Sean Kasun (seank@users.sf.net)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// Copyright (c) Novell, Inc. (http://www.novell.com)
//

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
//

namespace Mono.CSharp
{
	using System;
	using System.CodeDom;
	using System.CodeDom.Compiler;
	using System.IO;
	using System.Text;
	using System.Reflection;
	using System.Collections;
	using System.Collections.Specialized;
	using System.Diagnostics;
	using System.Text.RegularExpressions;

	internal class CSharpCodeCompiler : CSharpCodeGenerator, ICodeCompiler
	{
		static string windowsMcsPath;
		static string windowsMonoPath;

		static CSharpCodeCompiler ()
		{
			if (Path.DirectorySeparatorChar == '\\') {
				PropertyInfo gac = typeof (Environment).GetProperty ("GacPath", BindingFlags.Static|BindingFlags.NonPublic);
				MethodInfo get_gac = gac.GetGetMethod (true);
				string p = Path.GetDirectoryName (
					(string) get_gac.Invoke (null, null));
				windowsMonoPath = Path.Combine (
					Path.GetDirectoryName (
						Path.GetDirectoryName (p)),
					"bin\\mono.bat");
				if (!File.Exists (windowsMonoPath))
					windowsMonoPath = Path.Combine (
						Path.GetDirectoryName (
							Path.GetDirectoryName (p)),
						"bin\\mono.exe");
#if NET_2_0
				windowsMcsPath =
					Path.Combine (p, "2.0\\gmcs.exe");
#else
				windowsMcsPath =
					Path.Combine (p, "1.0\\mcs.exe");
#endif
			}
		}

		//
		// Constructors
		//
		public CSharpCodeCompiler()
		{
		}

		//
		// Methods
		//
		public CompilerResults CompileAssemblyFromDom (CompilerParameters options, CodeCompileUnit e)
		{
			return CompileAssemblyFromDomBatch (options, new CodeCompileUnit[] { e });
		}

		public CompilerResults CompileAssemblyFromDomBatch (CompilerParameters options, CodeCompileUnit[] ea)
		{
			if (options == null) {
				throw new ArgumentNullException ("options");
			}

			try {
				return CompileFromDomBatch (options, ea);
			} finally {
				options.TempFiles.Delete ();
			}
		}

		public CompilerResults CompileAssemblyFromFile (CompilerParameters options, string fileName)
		{
			return CompileAssemblyFromFileBatch (options, new string[] { fileName });
		}

		public CompilerResults CompileAssemblyFromFileBatch (CompilerParameters options, string[] fileNames)
		{
			if (options == null) {
				throw new ArgumentNullException ("options");
			}

			try {
				return CompileFromFileBatch (options, fileNames);
			} finally {
				options.TempFiles.Delete ();
			}
		}

		public CompilerResults CompileAssemblyFromSource (CompilerParameters options, string source)
		{
			return CompileAssemblyFromSourceBatch (options, new string[] { source });
		}

		public CompilerResults CompileAssemblyFromSourceBatch (CompilerParameters options, string[] sources)
		{
			if (options == null) {
				throw new ArgumentNullException ("options");
			}

			try {
				return CompileFromSourceBatch (options, sources);
			} finally {
				options.TempFiles.Delete ();
			}
		}

		private CompilerResults CompileFromFileBatch (CompilerParameters options, string[] fileNames)
		{
			if (null == options)
				throw new ArgumentNullException("options");
			if (null == fileNames)
				throw new ArgumentNullException("fileNames");

			CompilerResults results=new CompilerResults(options.TempFiles);
			Process mcs=new Process();

			string mcs_output;
			string[] mcs_output_lines;
			// FIXME: these lines had better be platform independent.
			if (Path.DirectorySeparatorChar == '\\') {
				mcs.StartInfo.FileName = windowsMonoPath;
                               mcs.StartInfo.Arguments = "\"" + windowsMcsPath + "\" " + BuildArgs (options, fileNames);
                       } else {
#if NET_2_0
				// FIXME: This is a temporary hack to make code genaration work in 2.0
				mcs.StartInfo.FileName="gmcs";
#else
				mcs.StartInfo.FileName="mcs";
#endif
				mcs.StartInfo.Arguments=BuildArgs(options,fileNames);
			}
			mcs.StartInfo.CreateNoWindow=true;
			mcs.StartInfo.UseShellExecute=false;
			mcs.StartInfo.RedirectStandardOutput=true;
			mcs.StartInfo.RedirectStandardError=true;
			try {
				mcs.Start();
				// If there are a few kB in stdout, we might lock
				mcs_output=mcs.StandardError.ReadToEnd();
				mcs.StandardOutput.ReadToEnd ();
				mcs.WaitForExit();
				results.NativeCompilerReturnValue = mcs.ExitCode;
			} finally {
				mcs.Close();
			}
			mcs_output_lines=mcs_output.Split(
				System.Environment.NewLine.ToCharArray());
			bool loadIt=true;
			foreach (string error_line in mcs_output_lines)
			{
				CompilerError error=CreateErrorFromString(error_line);
				if (null!=error)
				{
					results.Errors.Add(error);
					if (!error.IsWarning) loadIt=false;
				}
			}
			if (loadIt) {
				if (options.GenerateInMemory) {
					using (FileStream fs = File.OpenRead(options.OutputAssembly)) {
						byte[] buffer = new byte[fs.Length];
						fs.Read(buffer, 0, buffer.Length);
						results.CompiledAssembly = Assembly.Load(buffer, null, options.Evidence);
						fs.Close();
					}
				} else {
					results.CompiledAssembly = Assembly.LoadFrom(options.OutputAssembly);
					results.PathToAssembly = options.OutputAssembly;
				}
			} else {
				results.CompiledAssembly = null;
			}

			return results;
		}

		private static string BuildArgs(CompilerParameters options,string[] fileNames)
		{
			StringBuilder args=new StringBuilder();
			if (options.GenerateExecutable)
				args.Append("/target:exe ");
			else
				args.Append("/target:library ");

			if (options.Win32Resource != null)
				args.AppendFormat("/win32res:\"{0}\" ",
					options.Win32Resource);

			if (options.IncludeDebugInformation)
				args.Append("/debug+ /optimize- ");
			else
				args.Append("/debug- /optimize+ ");

			if (options.TreatWarningsAsErrors)
				args.Append("/warnaserror ");

			if (options.WarningLevel >= 0)
				args.AppendFormat ("/warn:{0} ", options.WarningLevel);

			if (options.OutputAssembly==null)
				options.OutputAssembly = GetTempFileNameWithExtension (options.TempFiles, "dll", !options.GenerateInMemory);
			args.AppendFormat("/out:\"{0}\" ",options.OutputAssembly);

			if (null != options.ReferencedAssemblies)
			{
				foreach (string import in options.ReferencedAssemblies)
					args.AppendFormat("/r:\"{0}\" ",import);
			}

			if (options.CompilerOptions != null) {
				args.Append (options.CompilerOptions);
				args.Append (" ");
			}
			
			args.Append (" -- ");
			foreach (string source in fileNames)
				args.AppendFormat("\"{0}\" ",source);
			return args.ToString();
		}
		private static CompilerError CreateErrorFromString(string error_string)
		{
#if NET_2_0
			if (error_string.StartsWith ("BETA"))
				return null;
#endif
			if (error_string == null || error_string == "")
				return null;

			CompilerError error=new CompilerError();
			Regex reg = new Regex (@"^(\s*(?<file>.*)\((?<line>\d*)(,(?<column>\d*))?\)(:)?\s+)*(?<level>\w+)\s*(?<number>.*):\s(?<message>.*)",
				RegexOptions.Compiled | RegexOptions.ExplicitCapture);
			Match match=reg.Match(error_string);
			if (!match.Success) return null;
			if (String.Empty != match.Result("${file}"))
				error.FileName=match.Result("${file}");
			if (String.Empty != match.Result("${line}"))
				error.Line=Int32.Parse(match.Result("${line}"));
			if (String.Empty != match.Result("${column}"))
				error.Column=Int32.Parse(match.Result("${column}"));

			string level = match.Result ("${level}");
			if (level == "warning")
				error.IsWarning = true;
			else if (level != "error")
				return null; // error CS8028 will confuse the regex.

			error.ErrorNumber=match.Result("${number}");
			error.ErrorText=match.Result("${message}");
			return error;
		}

		private static string GetTempFileNameWithExtension (TempFileCollection temp_files, string extension, bool keepFile)
		{
			return temp_files.AddExtension (extension, keepFile);
		}

		private static string GetTempFileNameWithExtension (TempFileCollection temp_files, string extension)
		{
			return temp_files.AddExtension (extension);
		}

		private CompilerResults CompileFromDomBatch (CompilerParameters options, CodeCompileUnit[] ea)
		{
			if (options == null) {
				throw new ArgumentNullException ("options");
			}

			if (ea == null) {
				throw new ArgumentNullException ("ea");
			}

			string[] fileNames = new string[ea.Length];
			StringCollection assemblies = options.ReferencedAssemblies;

			for (int i = 0; i < ea.Length; i++) {
				CodeCompileUnit compileUnit = ea[i];
				fileNames[i] = GetTempFileNameWithExtension (options.TempFiles, i + ".cs");
				FileStream f = new FileStream (fileNames[i], FileMode.OpenOrCreate);
				StreamWriter s = new StreamWriter (f, Encoding.UTF8);
				if (compileUnit.ReferencedAssemblies != null) {
					foreach (string str in compileUnit.ReferencedAssemblies) {
						if (!assemblies.Contains (str))
							assemblies.Add (str);
					}
				}

				((ICodeGenerator) this).GenerateCodeFromCompileUnit (compileUnit, s, new CodeGeneratorOptions ());
				s.Close ();
				f.Close ();
			}
			return CompileAssemblyFromFileBatch (options, fileNames);
		}

		private CompilerResults CompileFromSourceBatch (CompilerParameters options, string[] sources)
		{
			if (options == null) {
				throw new ArgumentNullException ("options");
			}

			if (sources == null) {
				throw new ArgumentNullException ("sources");
			}

			string[] fileNames = new string[sources.Length];

			for (int i = 0; i < sources.Length; i++) {
				fileNames[i] = GetTempFileNameWithExtension (options.TempFiles, i + ".cs");
				FileStream f = new FileStream (fileNames[i], FileMode.OpenOrCreate);
				using (StreamWriter s = new StreamWriter (f, Encoding.UTF8)) {
					s.Write (sources[i]);
					s.Close ();
				}
				f.Close ();
			}
			return CompileFromFileBatch (options, fileNames);
		}
	}
}
