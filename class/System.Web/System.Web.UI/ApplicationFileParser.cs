//
// System.Web.UI.ApplicationFileParser.cs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Web;
using System.Web.Compilation;

namespace System.Web.UI
{
	sealed class ApplicationFileParser : TemplateParser
	{
		public ApplicationFileParser (string fname, HttpContext context)
		{
			InputFile = fname;
			Context = context;
		}
		
		protected override Type CompileIntoType ()
		{
			GlobalAsaxCompiler compiler = new GlobalAsaxCompiler (this);
			return compiler.GetCompiledType ();
		}

		internal static Type GetCompiledApplicationType (string inputFile, HttpContext context)
		{
			ApplicationFileParser parser = new ApplicationFileParser (inputFile, context);
			AspGenerator generator = new AspGenerator (parser);
			return generator.GetCompiledType ();
		}

		internal override Type DefaultBaseType {
			get { return typeof (HttpApplication); }
		}

		internal override string DefaultDirectiveName {
			get { return "application"; }
		}

		internal override string BaseVirtualDir {
			get { return Context.Request.ApplicationPath; }
		}
	}

}

