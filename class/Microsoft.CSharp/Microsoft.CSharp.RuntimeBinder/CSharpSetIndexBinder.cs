//
// CSharpSetIndexBinder.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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
using System.Dynamic;
using System.Collections.Generic;
using System.Linq;
using Compiler = Mono.CSharp;

namespace Microsoft.CSharp.RuntimeBinder
{
	class CSharpSetIndexBinder : SetIndexBinder
	{
		IList<CSharpArgumentInfo> argumentInfo;
		Type callingContext;

		public CSharpSetIndexBinder (Type callingContext, IEnumerable<CSharpArgumentInfo> argumentInfo)
			: base (CSharpArgumentInfo.CreateCallInfo (argumentInfo, 2))
		{
			this.callingContext = callingContext;
			this.argumentInfo = argumentInfo.ToReadOnly ();
		}
		
		public override DynamicMetaObject FallbackSetIndex (DynamicMetaObject target, DynamicMetaObject[] indexes, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
		{
			if (argumentInfo.Count != indexes.Length + 2) {
				if (errorSuggestion == null)
					throw new NotImplementedException ();

				return errorSuggestion;
			}

			var expr = CSharpBinder.CreateCompilerExpression (argumentInfo [0], target);
			var args = CSharpBinder.CreateCompilerArguments (argumentInfo.Skip (2), indexes);
			expr = new Compiler.ElementAccess (expr, args);

			var source = CSharpBinder.CreateCompilerExpression (argumentInfo[1], value);
			expr = new Compiler.SimpleAssign (expr, source);
			expr = new Compiler.Cast (new Compiler.TypeExpression (ReturnType, Compiler.Location.Null), expr);

			var restrictions = CSharpBinder.CreateRestrictionsOnTarget (target).Merge (
				CSharpBinder.CreateRestrictionsOnTarget (value)).Merge (
				CSharpBinder.CreateRestrictionsOnTarget (indexes));

			return CSharpBinder.Bind (this, expr, callingContext, restrictions, errorSuggestion);
		}
	}
}
