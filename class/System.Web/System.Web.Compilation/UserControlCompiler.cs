//
// System.Web.Compilation.UserControlCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.IO;
using System.Reflection;
using System.Web.UI;
using System.Web.Util;

namespace System.Web.Compilation
{
	class UserControlCompiler : BaseCompiler
	{
		UserControlParser userControlParser;
		string sourceFile;
		string targetFile;

		internal UserControlCompiler (UserControlParser userControlParser, string targetFile)
		{
			this.userControlParser = userControlParser;
			this.targetFile = targetFile;
		}

		public override Type GetCompiledType ()
		{
			string inputFile = userControlParser.InputFile;
			sourceFile = GenerateSourceFile (userControlParser);

			CachingCompiler compiler = new CachingCompiler (this);
			CompilationResult result = new CompilationResult ();
			if (compiler.Compile (result) == false)
				throw new CompilationException (result);
				
			Assembly assembly = Assembly.LoadFrom (result.OutputFile);
			Type [] types = assembly.GetTypes ();
			if (types.Length != 1)
				throw new CompilationException ("More than 1 Type in a user control?", result);

			result.Data = types [0];
			return types [0];
		}

		public override string Key {
			get {
				return userControlParser.InputFile;
			}
		}

		public override string SourceFile {
			get {
				return sourceFile;
			}
		}

		public override string TargetFile {
			get {
				if (targetFile == null)
					targetFile = Path.ChangeExtension (sourceFile, ".dll");

				return targetFile;
			}
		}

		public static Type CompileUserControlType (UserControlParser userControlParser)
		{
			CompilationCacheItem item = CachingCompiler.GetCached (userControlParser.InputFile);
			if (item != null && item.Result != null) {
				if (item.Result != null)
					return item.Result.Data as Type;

				throw new CompilationException (item.Result);
			}

			UserControlCompiler pc = new UserControlCompiler (userControlParser, null);
			return pc.GetCompiledType ();
		}

		static string GenerateSourceFile (UserControlParser userControlParser)
		{
			string inputFile = userControlParser.InputFile;

			Stream input = File.OpenRead (inputFile);
			AspParser parser = new AspParser (inputFile, input);
			parser.Parse ();
			AspGenerator generator = new AspGenerator (inputFile, parser.Elements);
			generator.BaseType = userControlParser.BaseType.ToString ();
			generator.ProcessElements ();
			userControlParser.Text = generator.GetCode ().ReadToEnd ();

			//FIXME: should get Tmp dir for this application
			string csName = Path.GetTempFileName () + ".cs";
			WebTrace.WriteLine ("Writing {0}", csName);
			StreamWriter output = new StreamWriter (File.OpenWrite (csName));
			output.Write (userControlParser.Text);
			output.Close ();
			return csName;
		}
	}
}

