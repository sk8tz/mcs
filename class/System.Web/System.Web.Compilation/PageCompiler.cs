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

			base.CreateConstructor (localVars, trueStmt);
		}
		
		protected override void AddInterfaces () 
		{
			base.AddInterfaces ();
			if (pageParser.EnableSessionState)
				mainClass.BaseTypes.Add (new CodeTypeReference (typeof(IRequiresSessionState)));

			if (pageParser.ReadOnlySessionState)
				mainClass.BaseTypes.Add (new CodeTypeReference (typeof (IReadOnlySessionState)));
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

#if NET_2_0
		void InternalCreatePageProperty (string retType, string name, string contextProperty)
		{
			CodeMemberProperty property = new CodeMemberProperty ();
			property.Name = name;
			property.Type = new CodeTypeReference (retType);
			property.Attributes = MemberAttributes.Family | MemberAttributes.Final;

			CodeMethodReturnStatement ret = new CodeMethodReturnStatement ();
			CodeCastExpression cast = new CodeCastExpression ();
			ret.Expression = cast;
			
			CodePropertyReferenceExpression refexp = new CodePropertyReferenceExpression ();
			refexp.TargetObject = new CodePropertyReferenceExpression (new CodeThisReferenceExpression (), "Context");
			refexp.PropertyName = contextProperty;
			
			cast.TargetType = new CodeTypeReference (retType);
			cast.Expression = refexp;
			
			property.GetStatements.Add (ret);
			mainClass.Members.Add (property);
		}
		
		void CreateProfileProperty ()
		{
			string retType;
			ProfileSection ps = WebConfigurationManager.GetSection ("system.web/profile") as ProfileSection;
			if (ps != null && ps.PropertySettings.Count > 0)
				retType = "ProfileCommon";
			else
				retType = "System.Web.Profile.DefaultProfile";
			InternalCreatePageProperty (retType, "Profile", "Profile");
		}
#endif

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
			CodeMethodInvokeExpression expr = new CodeMethodInvokeExpression (thisRef, "InitializeCulture");
			method.Statements.Add (new CodeExpressionStatement (expr));

			CodeArgumentReferenceExpression ctrlVar = new CodeArgumentReferenceExpression("__ctrl");
			if (pageParser.Title != null)
				method.Statements.Add (CreatePropertyAssign (ctrlVar, "Title", pageParser.Title));

			if (pageParser.MasterPageFile != null)
				method.Statements.Add (CreatePropertyAssign (ctrlVar, "MasterPageFile", pageParser.MasterPageFile));

			if (pageParser.Theme != null)
				method.Statements.Add (CreatePropertyAssign (ctrlVar, "Theme", pageParser.Theme));

			if (pageParser.StyleSheetTheme != null)
				method.Statements.Add (CreatePropertyAssign (ctrlVar, "StyleSheetTheme", pageParser.StyleSheetTheme));
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
                        
			base.AppendStatementsToFrameworkInitialize (method);
		}

		private CodeExpression[] OutputCacheParams ()
		{
			return new CodeExpression [] {
				new CodePrimitiveExpression (pageParser.OutputCacheDuration),
				new CodePrimitiveExpression (pageParser.OutputCacheVaryByHeader),
				new CodePrimitiveExpression (pageParser.OutputCacheVaryByCustom),
				new CodeSnippetExpression (typeof (OutputCacheLocation).ToString () +
						"." + pageParser.OutputCacheLocation.ToString ()),
				new CodePrimitiveExpression (pageParser.OutputCacheVaryByParam)
				};
		}
                
		protected internal override void CreateMethods ()
		{
			base.CreateMethods ();

#if NET_2_0
			CreateProfileProperty ();
			if (pageParser.MasterType != null) {
				CodeMemberProperty mprop = new CodeMemberProperty ();
				mprop.Name = "Master";
				mprop.Type = new CodeTypeReference (pageParser.MasterType);
				mprop.Attributes = MemberAttributes.Public | MemberAttributes.New;
				CodeExpression prop = new CodePropertyReferenceExpression (new CodeBaseReferenceExpression (), "Master");
				prop = new CodeCastExpression (pageParser.MasterType, prop);
				mprop.GetStatements.Add (new CodeMethodReturnStatement (prop));
				mainClass.Members.Add (mprop);
			}
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


