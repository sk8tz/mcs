//
// MSXslScriptManager.cs
//
// Author:
//	Atsushi Enomoto (atsushi@ximian.com)
//
// (C)2003 Novell inc.
//
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Policy;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;
using Microsoft.CSharp;
using Microsoft.VisualBasic;

namespace Mono.Xml.Xsl
{
	internal abstract class ScriptCompilerInfo
	{
		string compilerCommand;
		string defaultCompilerOptions;

		public virtual string CompilerCommand {
			get { return compilerCommand; }
			set { compilerCommand = value; }
		}

		public virtual string DefaultCompilerOptions {
			get { return defaultCompilerOptions; }
			set { defaultCompilerOptions = value; }
		}

		public abstract CodeDomProvider CodeDomProvider { get; }

		public abstract string Extension { get; }

		public abstract string SourceTemplate { get; }

		public virtual string GetCompilerArguments (string targetFileName)
		{
			return String.Concat (DefaultCompilerOptions, " ", targetFileName);
		}


#if true // Use CodeDom
		public virtual Type GetScriptClass (string code, string classSuffix, XPathNavigator scriptNode, Evidence evidence)
		{
			PermissionSet ps = SecurityManager.ResolvePolicy (evidence);
			if (ps != null)
				ps.Demand ();

			ICodeCompiler compiler = CodeDomProvider.CreateCompiler ();
			CompilerParameters parameters = new CompilerParameters ();
			parameters.CompilerOptions = DefaultCompilerOptions;

			// get source filename
			string filename = String.Empty;
			try {
				if (scriptNode.BaseURI != String.Empty)
					filename = new Uri (scriptNode.BaseURI).LocalPath;
			} catch (FormatException) {
			}

			// get source location
			IXmlLineInfo li = scriptNode as IXmlLineInfo;
			string lineInfoLine = li != null ? "\n#line " + li.LineNumber + " \"" + filename + "\"" : String.Empty;

			string source = SourceTemplate.Replace ("{0}", DateTime.Now.ToString ()).Replace ("{1}", classSuffix).Replace ("{2}", lineInfoLine + code);

			CompilerResults res = compiler.CompileAssemblyFromSource (parameters, source);
			if (res.Errors.Count != 0)
				throw new XsltCompileException ("Stylesheet script compile error: \n" + FormatErrorMessage (res) /*+ "Code :\n" + source*/, null, scriptNode);
			if (res.CompiledAssembly == null)
				throw new XsltCompileException ("Cannot compile stylesheet script", null, scriptNode);
			return res.CompiledAssembly.GetType ("GeneratedAssembly.Script" + classSuffix);
		}
#else // obsolete code - uses external process
		[MonoTODO ("Should use Assembly.LoadFile() instead of LoadFrom() after its implementation has finished.")]
		public virtual Type GetScriptClass (string code, string classSuffix, XPathNavigator scriptNode, Evidence evidence)
		{
			string tmpPath = Path.GetTempPath ();
			if (!tmpPath.EndsWith (Path.DirectorySeparatorChar.ToString ()))
				tmpPath += Path.DirectorySeparatorChar;
			string tmpbase = tmpPath + Guid.NewGuid ();
			ProcessStartInfo psi = new ProcessStartInfo ();
			psi.UseShellExecute = false;
			psi.RedirectStandardError = true;
			psi.RedirectStandardOutput = true;
			Process proc = new Process ();
			proc.StartInfo = psi;
			StreamWriter sw = null;
			try {
				PermissionSet ps = SecurityManager.ResolvePolicy (evidence);
				if (ps != null)
					ps.Demand ();
				sw = File.CreateText (tmpbase + Extension);
				IXmlLineInfo li = scriptNode as IXmlLineInfo;
				string lineInfoLine = li != null ? "\n#line " + li.LineNumber + " \"" + scriptNode.BaseURI + "\"\n" : String.Empty;
				sw.WriteLine (SourceTemplate.Replace ("{0}", DateTime.Now.ToString ()).Replace ("{1}", classSuffix).Replace ("{2}", lineInfoLine + code));

				sw.Close ();
				psi.FileName = CompilerCommand;
				psi.Arguments = String.Concat (GetCompilerArguments (tmpbase + Extension));
				psi.WorkingDirectory = tmpPath;
				proc.Start ();
//				Console.WriteLine (proc.StandardOutput.ReadToEnd ());
//				Console.WriteLine (proc.StandardError.ReadToEnd ());
				proc.WaitForExit (); // should we configure timeout?
				Assembly generated = Assembly.LoadFrom (tmpbase + ".dll");

				if (generated == null)
					throw new XsltCompileException ("Could not load script assembly", null, scriptNode);
				return generated.GetType ("GeneratedAssembly.Script" + classSuffix);
			} catch (Exception ex) {
				throw new XsltCompileException ("Script compilation error: " + ex.Message, ex, scriptNode);
			} finally {
				try {
					File.Delete (tmpbase + Extension);
					File.Delete (tmpbase + ".dll");
					if (sw != null)
						sw.Close ();
				} catch (Exception) {
				}
			}
		}
#endif

		private string FormatErrorMessage (CompilerResults res)
		{
			string s = String.Empty;
			foreach (CompilerError e in res.Errors) {
				object [] parameters = new object [] {"\n",
					e.FileName,
					" line ",
					e.Line,
					e.IsWarning ? " WARNING: " : " ERROR: ",
					e.ErrorNumber,
					": ",
					e.ErrorText};
				s += String.Concat (parameters);
			}
			return s;
		}
	}

	internal class CSharpCompilerInfo : ScriptCompilerInfo
	{
		public CSharpCompilerInfo ()
		{
			this.CompilerCommand = "mcs";
#if MS_NET
			this.CompilerCommand = "csc.exe";
#endif
			this.DefaultCompilerOptions = "/t:library /r:System.dll /r:System.Xml.dll /r:Microsoft.VisualBasic.dll";
		}

		public override CodeDomProvider CodeDomProvider {
			get { return new CSharpCodeProvider (); }
		}

		public override string Extension {
			get { return ".cs"; }
		}

		public override string SourceTemplate {
			get {
				return @"// This file is automatically created by Mono managed XSLT engine.
// Created time: {0}
using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using Microsoft.VisualBasic;

namespace GeneratedAssembly
{
public class Script{1}
{
	{2}
}
}";
			}
		}
	}

	internal class VBCompilerInfo : ScriptCompilerInfo
	{
		public VBCompilerInfo ()
		{
			this.CompilerCommand = "mbas";
#if MS_NET
			this.CompilerCommand = "vbc.exe";
#endif
			this.DefaultCompilerOptions = "/t:library  /r:System.dll /r:System.XML.dll /r:Microsoft.VisualBasic.dll";
		}

		public override CodeDomProvider CodeDomProvider {
			get { return new VBCodeProvider (); }
		}

		public override string Extension {
			get { return ".vb"; }
		}

		public override string SourceTemplate {
			get {
				return @"' This file is automatically created by Mono managed XSLT engine.
' Created time: {0}
imports System
imports System.Collections
imports System.Text
imports System.Text.RegularExpressions
imports System.Xml
imports System.Xml.XPath
imports System.Xml.Xsl
imports Microsoft.VisualBasic

namespace GeneratedAssembly
public Class Script{1}
	{2}
end Class
end namespace
";
			}
		}
	}

	internal class JScriptCompilerInfo : ScriptCompilerInfo
	{
		static Type providerType;

		static JScriptCompilerInfo ()
		{
			Assembly jsasm = Assembly.LoadWithPartialName ("Microsoft.JScript.dll", null);
			providerType = jsasm.GetType ("Microsoft.JScript.JScriptCodeProvider");
		}

		public JScriptCompilerInfo ()
		{
			this.CompilerCommand = "mjs";
#if MS_NET
			this.CompilerCommand = "jsc.exe";
#endif
			this.DefaultCompilerOptions = "/t:library /r:Microsoft.VisualBasic.dll";
		}

		public override CodeDomProvider CodeDomProvider {
			get { return (CodeDomProvider) Activator.CreateInstance (providerType); }
		}

		public override string Extension {
			get { return ".js"; }
		}

		public override string SourceTemplate {
			get {
				return @"// This file is automatically created by Mono managed XSLT engine.
// Created time: {0}
import System;
import System.Collections;
import System.Text;
import System.Text.RegularExpressions;
import System.Xml;
import System.Xml.XPath;
import System.Xml.Xsl;
import Microsoft.VisualBasic;

package GeneratedAssembly
{
class Script{1} {
	{2}
}
}
";
			}
		}
	}
}

