//
// System.Web.UI.UserControlParser
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Web;
using System.Web.Compilation;

namespace System.Web.UI
{
	public sealed class UserControlParser : TemplateControlParser
	{
		internal UserControlParser (string inputFile)
		{
			InputFile = inputFile;
		}
		
		public static Type GetCompiledType (string virtualPath, string inputFile, HttpContext context)
		{
			UserControlParser ucp = new UserControlParser (inputFile);
			Type t = ucp.CompileIntoType ();
			return t;
		}

		protected override Type CompileIntoType ()
		{
			return UserControlCompiler.CompileUserControlType (this);
		}

		protected override Type DefaultBaseType
		{
			get {
				return typeof (Control);
			}
		}

		protected override string DefaultDirectiveName
		{
			get {
				return "control";
			}
		}
	}
}

