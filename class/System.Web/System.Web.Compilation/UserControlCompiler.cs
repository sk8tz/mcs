//
// System.Web.Compilation.UserControlCompiler
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
using System.Web.UI;

namespace System.Web.Compilation
{
	class UserControlCompiler : TemplateControlCompiler
	{
		UserControlParser parser;

		public UserControlCompiler (UserControlParser parser)
			: base (parser)
		{
			this.parser = parser;
		}

		public static Type CompileUserControlType (UserControlParser parser)
		{
			UserControlCompiler pc = new UserControlCompiler (parser);
			return pc.GetCompiledType ();
		}

		protected override void AddClassAttributes ()
		{
			if (parser.OutputCache)
				AddOutputCacheAttribute ();
		}

		private void AddOutputCacheAttribute ()
		{
			CodeAttributeDeclaration cad;
			cad = new CodeAttributeDeclaration ("System.Web.UI.PartialCachingAttribute");
			AddPrimitiveAttribute (cad, parser.OutputCacheDuration);
			AddPrimitiveAttribute (cad, parser.OutputCacheVaryByParam);
			AddPrimitiveAttribute (cad, parser.OutputCacheVaryByControls);
			AddPrimitiveAttribute (cad, parser.OutputCacheVaryByCustom);
			AddPrimitiveAttribute (cad, parser.OutputCacheShared);
			mainClass.CustomAttributes.Add (cad);
		}

		private void AddPrimitiveAttribute (CodeAttributeDeclaration cad, object obj)
		{
			cad.Arguments.Add (new CodeAttributeArgument (new CodePrimitiveExpression (obj)));
		}
	}
}

