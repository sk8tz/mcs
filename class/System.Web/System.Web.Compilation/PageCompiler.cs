//
// System.Web.Compilation.PageCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.IO;
using System.Web.UI;

namespace System.Web.Compilation
{
	class PageCompiler
	{
		private PageParser pageParser;

		internal PageCompiler (PageParser pageParser)
		{
			this.pageParser = pageParser;
		}

		public static Type CompilePageType (PageParser pageParser)
		{
			string sourceFile = GenerateSourceFile (pageParser);
			Console.WriteLine ("Compiling {0}", sourceFile);
			return TemplateFactory.GetTypeFromSource (sourceFile);
		}

		private static string GenerateSourceFile (PageParser pageParser)
		{
			string inputFile = pageParser.InputFile;

			Stream input = File.OpenRead (inputFile);
			AspParser parser = new AspParser (inputFile, input);
			parser.Parse ();
			AspGenerator generator = new AspGenerator (inputFile, parser.Elements);
			generator.BaseType = "System.Web.UI.Page";
			generator.ProcessElements ();
			pageParser.Text = generator.GetCode ().ReadToEnd ();

			//FIXME: should get Tmp dir for this application
			string csName = Path.GetTempFileName () + ".cs";
			Console.WriteLine ("Writing {0}", csName);
			StreamWriter output = new StreamWriter (File.OpenWrite (csName));
			output.Write (pageParser.Text);
			output.Close ();
			return csName;
		}
	}
}
