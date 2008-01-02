//
// System.Web.Compilation.PageCompiler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002,2003 Ximian, Inc (http://www.ximian.com)
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
using System;
using System.CodeDom;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.SessionState;
using System.Web.Util;
#if NET_2_0
using System.Web.Profile;
#endif

namespace System.Web.Compilation
{
	class PageCompiler : TemplateControlCompiler
	{
		PageParser pageParser;
		static CodeTypeReference intRef = new CodeTypeReference (typeof (int));

		public PageCompiler (PageParser pageParser)
			: base (pageParser)
		{
			this.pageParser = pageParser;
		}

#if NET_2_0
		protected override void CreateStaticFields ()
		{
			base.CreateStaticFields ();
			
			CodeMemberField fld = new CodeMemberField (typeof (object), "__fileDependencies");
			fld.Attributes = MemberAttributes.Private | MemberAttributes.Static;
			fld.InitExpression = new CodePrimitiveExpression (null);
			mainClass.Members.Add (fld);
		}
#endif
		
		protected override void CreateConstructor (CodeStatementCollection localVars,
							   CodeStatementCollection trueStmt)
		{
			if (pageParser.ClientTarget != null) {
				CodeExpression prop;
				prop = new CodePropertyReferenceExpression (thisRef, "ClientTarget");
				CodeExpression ct = new CodePrimitiveExpression (pageParser.ClientTarget);
				if (localVars == null)
					localVars = new CodeStatementCollection ();
				localVars.Add (new CodeAssignStatement (prop, ct));
			}

#if NET_2_0
			ArrayList deps = pageParser.Dependencies;
			int depsCount = deps != null ? deps.Count : 0;
			
			if (depsCount > 0) {
				if (localVars == null)
					localVars = new CodeStatementCollection ();
				if (trueStmt == null)
					trueStmt = new CodeStatementCollection ();

				localVars.Add (
					new CodeVariableDeclarationStatement (
						typeof (string[]),
						"dependencies")
				);

				CodeVariableReferenceExpression dependencies = new CodeVariableReferenceExpression ("dependencies");
				trueStmt.Add (
					new CodeAssignStatement (dependencies, new CodeArrayCreateExpression (typeof (string), depsCount))
				);
				
				CodeArrayIndexerExpression arrayIndex;
				CodeAssignStatement assign;
				object o;
				
				for (int i = 0; i < depsCount; i++) {
					o = deps [i];
					arrayIndex = new CodeArrayIndexerExpression (dependencies, new CodeExpression[] {new CodePrimitiveExpression (i)});
					assign = new CodeAssignStatement (arrayIndex, new CodePrimitiveExpression (o));
					trueStmt.Add (assign);
				}
				
				CodeMethodInvokeExpression getDepsCall = new CodeMethodInvokeExpression (
					thisRef,
					"GetWrappedFileDependencies",
					new CodeExpression[] {dependencies}
				);

				assign = new CodeAssignStatement (GetMainClassFieldReferenceExpression ("__fileDependencies"), getDepsCall);
				trueStmt.Add (assign);
			}
#endif
			base.CreateConstructor (localVars, trueStmt);
		}
		
		protected override void AddInterfaces () 
		{
			base.AddInterfaces ();
			CodeTypeReference cref;
			
			if (pageParser.EnableSessionState) {
				cref = new CodeTypeReference (typeof (IRequiresSessionState));
#if NET_2_0
				if (partialClass != null)
					partialClass.BaseTypes.Add (cref);
				else
#endif
					mainClass.BaseTypes.Add (cref);
			}
			
			if (pageParser.ReadOnlySessionState) {
				cref = new CodeTypeReference (typeof (IReadOnlySessionState));
#if NET_2_0
				if (partialClass != null)
					partialClass.BaseTypes.Add (cref);					
				else
#endif
					mainClass.BaseTypes.Add (cref);
			}

#if NET_2_0
			if (pageParser.Async)
				cref = new CodeTypeReference (typeof (System.Web.IHttpAsyncHandler));
			else
				cref = new CodeTypeReference (typeof (System.Web.IHttpHandler));
			mainClass.BaseTypes.Add (cref);
#endif
		}

		void CreateGetTypeHashCode () 
		{
			CodeMemberMethod method = new CodeMemberMethod ();
			method.ReturnType = intRef;
			method.Name = "GetTypeHashCode";
			method.Attributes = MemberAttributes.Public | MemberAttributes.Override;
			Random rnd = new Random (pageParser.InputFile.GetHashCode ());
			method.Statements.Add (new CodeMethodReturnStatement (new CodePrimitiveExpression (rnd.Next ())));
			mainClass.Members.Add (method);
		}

		static CodeAssignStatement CreatePropertyAssign (CodeExpression expr, string name, object value)
		{
			CodePropertyReferenceExpression prop;
			prop = new CodePropertyReferenceExpression (expr, name);
			CodePrimitiveExpression prim;
			prim = new CodePrimitiveExpression (value);
			return new CodeAssignStatement (prop, prim);
		}

		static CodeAssignStatement CreatePropertyAssign (string name, object value)
		{
			return CreatePropertyAssign (thisRef, name, value);
		}

		protected override void AddStatementsToInitMethod (CodeMemberMethod method)
		{
#if NET_2_0
			ILocation directiveLocation = pageParser.DirectiveLocation;
			
			CodeArgumentReferenceExpression ctrlVar = new CodeArgumentReferenceExpression("__ctrl");
			if (pageParser.Title != null)
				method.Statements.Add (AddLinePragma (CreatePropertyAssign (ctrlVar, "Title", pageParser.Title), directiveLocation));

			if (pageParser.MasterPageFile != null)
				method.Statements.Add (AddLinePragma (CreatePropertyAssign (ctrlVar, "MasterPageFile", pageParser.MasterPageFile), directiveLocation));

			if (pageParser.Theme != null)
				method.Statements.Add (AddLinePragma (CreatePropertyAssign (ctrlVar, "Theme", pageParser.Theme), directiveLocation));

			if (pageParser.StyleSheetTheme != null)
				method.Statements.Add (AddLinePragma (CreatePropertyAssign (ctrlVar, "StyleSheetTheme", pageParser.StyleSheetTheme), directiveLocation));

			if (pageParser.Async != false)
				method.Statements.Add (AddLinePragma (CreatePropertyAssign (ctrlVar, "AsyncMode", pageParser.Async), directiveLocation));

			if (pageParser.AsyncTimeout != -1)
				method.Statements.Add (AddLinePragma (CreatePropertyAssign (ctrlVar, "AsyncTimeout",
											    TimeSpan.FromSeconds (pageParser.AsyncTimeout)), directiveLocation));

			CodeMethodInvokeExpression expr = new CodeMethodInvokeExpression (thisRef, "InitializeCulture");
			method.Statements.Add (AddLinePragma (new CodeExpressionStatement (expr), directiveLocation));
#endif
		}

		protected override void PrependStatementsToFrameworkInitialize (CodeMemberMethod method)
		{
			base.PrependStatementsToFrameworkInitialize (method);
#if NET_2_0
			if (pageParser.StyleSheetTheme != null)
				method.Statements.Add (CreatePropertyAssign ("StyleSheetTheme", pageParser.StyleSheetTheme));
#endif
		}

		protected override void AppendStatementsToFrameworkInitialize (CodeMemberMethod method)
		{
			base.AppendStatementsToFrameworkInitialize (method);
			
#if NET_2_0
			ArrayList deps = pageParser.Dependencies;
			int depsCount = deps != null ? deps.Count : 0;
			
			if (depsCount > 0) {
				CodeFieldReferenceExpression fileDependencies = GetMainClassFieldReferenceExpression ("__fileDependencies");

				method.Statements.Add (
					new CodeMethodInvokeExpression (
						thisRef,
						"AddWrappedFileDependencies",
						new CodeExpression[] {fileDependencies})
				);
			}
#endif
			
			string responseEncoding = pageParser.ResponseEncoding;
			if (responseEncoding != null)
				method.Statements.Add (CreatePropertyAssign ("ResponseEncoding", responseEncoding));
			
			int codepage = pageParser.CodePage;
			if (codepage != -1)
				method.Statements.Add (CreatePropertyAssign ("CodePage", codepage));

			string contentType = pageParser.ContentType;
			if (contentType != null)
				method.Statements.Add (CreatePropertyAssign ("ContentType", contentType));

			if (pageParser.OutputCache) {
				CodeMethodReferenceExpression init = new CodeMethodReferenceExpression (null,
						"InitOutputCache");
				CodeMethodInvokeExpression invoke = new CodeMethodInvokeExpression (init,
						OutputCacheParams ());
				method.Statements.Add (invoke);
			}
			
			int lcid = pageParser.LCID;
			if (lcid != -1)
				method.Statements.Add (CreatePropertyAssign ("LCID", lcid));

			string culture = pageParser.Culture;
			if (culture != null)
				method.Statements.Add (CreatePropertyAssign ("Culture", culture));

			culture = pageParser.UICulture;
			if (culture != null)
				method.Statements.Add (CreatePropertyAssign ("UICulture", culture));

			string errorPage = pageParser.ErrorPage;
			if (errorPage != null)
				method.Statements.Add (CreatePropertyAssign ("ErrorPage", errorPage));

                        if (pageParser.HaveTrace) {
                                CodeAssignStatement stmt = new CodeAssignStatement ();
                                stmt.Left = new CodePropertyReferenceExpression (thisRef, "TraceEnabled");
                                stmt.Right = new CodePrimitiveExpression (pageParser.Trace);
                                method.Statements.Add (stmt);
                        }

                        if (pageParser.TraceMode != TraceMode.Default) {
                                CodeAssignStatement stmt = new CodeAssignStatement ();
                                CodeTypeReferenceExpression tm = new CodeTypeReferenceExpression ("System.Web.TraceMode");
                                stmt.Left = new CodePropertyReferenceExpression (thisRef, "TraceModeValue");
                                stmt.Right = new CodeFieldReferenceExpression (tm, pageParser.TraceMode.ToString ());
                                method.Statements.Add (stmt);
                        }

                        if (pageParser.NotBuffer) {
                                CodeAssignStatement stmt = new CodeAssignStatement ();
                                stmt.Left = new CodePropertyReferenceExpression (thisRef, "Buffer");
                                stmt.Right = new CodePrimitiveExpression (false);
                                method.Statements.Add (stmt);
                        }

#if NET_1_1
			if (pageParser.ValidateRequest) {
				CodeMethodInvokeExpression expr = new CodeMethodInvokeExpression ();
                                CodePropertyReferenceExpression prop;
                                prop = new CodePropertyReferenceExpression (thisRef, "Request");
				expr.Method = new CodeMethodReferenceExpression (prop, "ValidateInput");
				method.Statements.Add (expr);
			}
#endif
#if NET_2_0
			if (!pageParser.EnableEventValidation) {
                                CodeAssignStatement stmt = new CodeAssignStatement ();
                                CodePropertyReferenceExpression prop;
                                prop = new CodePropertyReferenceExpression (thisRef, "EnableEventValidation");
				stmt.Left = prop;
				stmt.Right = new CodePrimitiveExpression (pageParser.EnableEventValidation);
				method.Statements.Add (stmt);
			}

			if (pageParser.MaintainScrollPositionOnPostBack) {
				CodeAssignStatement stmt = new CodeAssignStatement ();
				CodePropertyReferenceExpression prop;
                                prop = new CodePropertyReferenceExpression (thisRef, "MaintainScrollPositionOnPostBack");
				stmt.Left = prop;
				stmt.Right = new CodePrimitiveExpression (pageParser.MaintainScrollPositionOnPostBack);
				method.Statements.Add (stmt);
			}
#endif
		}

		private CodeExpression[] OutputCacheParams ()
		{
			return new CodeExpression [] {
				new CodePrimitiveExpression (pageParser.OutputCacheDuration),
#if NET_2_0
				new CodePrimitiveExpression (pageParser.OutputCacheVaryByContentEncodings),
#endif
				new CodePrimitiveExpression (pageParser.OutputCacheVaryByHeader),
				new CodePrimitiveExpression (pageParser.OutputCacheVaryByCustom),
				new CodeSnippetExpression (typeof (OutputCacheLocation).ToString () +
						"." + pageParser.OutputCacheLocation.ToString ()),
				new CodePrimitiveExpression (pageParser.OutputCacheVaryByParam)
				};
		}

#if NET_2_0
		void CreateStronglyTypedProperty (Type type, string name)
		{
			if (type == null)
				return;
			
			CodeMemberProperty mprop = new CodeMemberProperty ();
			mprop.Name = name;
			mprop.Type = new CodeTypeReference (type);
			mprop.Attributes = MemberAttributes.Public | MemberAttributes.New;
			CodeExpression prop = new CodePropertyReferenceExpression (new CodeBaseReferenceExpression (), name);
			prop = new CodeCastExpression (type, prop);
			mprop.GetStatements.Add (new CodeMethodReturnStatement (prop));
			if (partialClass != null)
				partialClass.Members.Add (mprop);
			else
				mainClass.Members.Add (mprop);
		}
#endif
		
		protected internal override void CreateMethods ()
		{
			base.CreateMethods ();

#if NET_2_0
			CreateProfileProperty ();
			CreateStronglyTypedProperty (pageParser.MasterType, "Master");
			CreateStronglyTypedProperty (pageParser.PreviousPageType, "PreviousPage");
#endif
			
			CreateGetTypeHashCode ();
		}

		public static Type CompilePageType (PageParser pageParser)
		{
			PageCompiler compiler = new PageCompiler (pageParser);
			return compiler.GetCompiledType ();
		}
	}
}


