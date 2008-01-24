//
// System.Web.Compilation.GenericBuildProvider
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2008 Novell, Inc
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
#if NET_2_0
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.UI;

namespace System.Web.Compilation
{
	internal abstract class GenericBuildProvider <TParser> : BuildProvider
	{
		TParser _parser;
		CompilerType _compilerType;
		BaseCompiler _compiler;
		bool _parsed;
		bool _codeGenerated;
		
		protected abstract TParser CreateParser (string virtualPath, string physicalPath, TextReader reader, HttpContext context);
		protected abstract TParser CreateParser (string virtualPath, string physicalPath, HttpContext context);
		protected abstract BaseCompiler CreateCompiler (TParser parser);
		protected abstract string GetParserLanguage (TParser parser);
		protected abstract ICollection GetParserDependencies (TParser parser);
		protected abstract string GetCodeBehindSource (TParser parser);
		protected abstract string GetClassType (BaseCompiler compiler, TParser parser);
		protected abstract AspGenerator CreateAspGenerator (TParser parser);
		
		protected virtual TParser Parse ()
		{
			TParser parser = Parser;
			
			if (_parsed)
				return parser;

			if (!IsDirectoryBuilder) {
				AspGenerator generator = CreateAspGenerator (parser);
				generator.Parse ();
			}
			
			_parsed = true;
			return parser;
		}

		protected virtual void OverrideAssemblyPrefix (TParser parser, AssemblyBuilder assemblyBuilder)
		{
		}

		internal override void GenerateCode ()
		{
			TParser parser = Parse ();
			_compiler = CreateCompiler (parser);
			if (NeedsConstructType)
				_compiler.ConstructType ();
			_codeGenerated = true;
		}
		
		protected virtual void GenerateCode (AssemblyBuilder assemblyBuilder, TParser parser, BaseCompiler compiler)
		{				
			CodeCompileUnit unit = _compiler.CompileUnit;
			if (unit == null)
				throw new HttpException ("Unable to generate source code.");
				
			assemblyBuilder.AddCodeCompileUnit (this, unit);
		}
		
		public override void GenerateCode (AssemblyBuilder assemblyBuilder)
		{
			if (!_codeGenerated)
				GenerateCode ();
			
			TParser parser = Parse ();
			OverrideAssemblyPrefix (parser, assemblyBuilder);
			
			string codeBehindSource = GetCodeBehindSource (parser);
			if (codeBehindSource != null)
				assemblyBuilder.AddCodeFile (codeBehindSource, this);

			GenerateCode (assemblyBuilder, parser, _compiler);
		}

		protected virtual Type LoadTypeFromBin (BaseCompiler compiler, TParser parser)
		{
			return null;
		}
		
		public override Type GetGeneratedType (CompilerResults results)
		{
			if (_compiler == null || results == null)
				return null;

			if (NeedsLoadFromBin)
				return LoadTypeFromBin (_compiler, Parser);
			
			// This is not called if compilation failed.
			// Returning null makes the caller throw an InvalidCastException
			Assembly assembly = results.CompiledAssembly;
			if (assembly == null)
				return null;
			
			return assembly.GetType (GetClassType (_compiler, Parser));
		}

		// This is intended to be used by builders which may need to do special processing
		// on the virtualPath before actually opening the reader.
		protected virtual TextReader SpecialOpenReader (string virtualPath, out string physicalPath)
		{
			physicalPath = null;
			return OpenReader (virtualPath);
		}
		
		// FIXME: figure this out.
		public override ICollection VirtualPathDependencies {
			get {
				TParser parser = Parser;
				return GetParserDependencies (parser);
			}
		}
		
		public override CompilerType CodeCompilerType {
			get {
				if (_compilerType == null) {
					TParser parser = Parse ();
					_compilerType = GetDefaultCompilerTypeForLanguage (GetParserLanguage (parser));
				}

				return _compilerType;
			}
		}

		public TParser Parser {
			get {
				if (_parser == null) {
					string vp = VirtualPath;					
					if (String.IsNullOrEmpty (vp))
						throw new HttpException ("VirtualPath not set, cannot instantiate parser.");
					
					if (!IsDirectoryBuilder) {
						string physicalPath;
						TextReader reader = SpecialOpenReader (vp, out physicalPath);
						_parser = CreateParser (vp, physicalPath, reader, HttpContext.Current);
					} else
						_parser = CreateParser (vp, null,  HttpContext.Current);
					
					if (_parser == null)
						throw new HttpException ("Unable to create type parser.");
				}
				
				return _parser;
			}
		}

		protected virtual bool IsDirectoryBuilder {
			get { return false; }
		}

		protected virtual bool NeedsConstructType {
			get { return true; }
		}

		protected virtual bool NeedsLoadFromBin {
			get { return false; }
		}
		
		internal override CodeCompileUnit CodeUnit {
			get {
				if (!_codeGenerated)
					GenerateCode ();
				return _compiler.CompileUnit;
			}
		}
	}
}
#endif