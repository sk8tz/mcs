//
// System.Web.Compilation.ConnectionStringsExpressionBuilder
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.Configuration;
using System.Web.Configuration;
using System.Web.UI;

namespace System.Web.Compilation {

	[ExpressionEditor("System.Web.UI.Design.ConnectionStringsExpressionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[ExpressionPrefix("ConnectionStrings")]
	public class ConnectionStringsExpressionBuilder : ExpressionBuilder {

		public override object EvaluateExpression (object target, BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
		{
			return GetConnectionString (entry.Expression.Trim());
		}

		public override CodeExpression GetCodeExpression (BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
		{
			throw new NotImplementedException ();
		}

		public static string GetConnectionString (string connectionStringName)
		{
			ConnectionStringSettings conn = WebConfigurationManager.ConnectionStrings [connectionStringName];
			if (conn == null)
				return "";
			else
				return conn.ConnectionString;
		}

		public static string GetConnectionStringProviderName (string connectionStringName)
		{
			ConnectionStringSettings conn = WebConfigurationManager.ConnectionStrings [connectionStringName];
			if (conn == null)
				return "";
			else
				return conn.ProviderName;
		}

		[MonoTODO]
		public override	object ParseExpression (string expression, Type propertyType, ExpressionBuilderContext context)
		{
			throw new NotImplementedException ();
		}

		public override bool SupportsEvaluate {
			get { return true; }
		}
	}

}

#endif


