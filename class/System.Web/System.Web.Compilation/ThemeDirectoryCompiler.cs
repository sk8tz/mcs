//
// System.Web.UI.ThemeDirectoryCompiler
//
// Authors:
//   Chris Toshok (toshok@ximian.com)
//
// (C) 2006 Novell, Inc. (http://www.novell.com)
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
using System.Collections;
using System.IO;
using System.Web;
using System.Web.Compilation;
using System.Web.Util;

namespace System.Web.UI
{
	internal sealed class ThemeDirectoryCompiler
	{
		public static Type GetCompiledType (string virtualPath, HttpContext context)
		{
			string physicalPath = context.Request.MapPath (virtualPath);
			string[] skin_files = Directory.GetFiles (physicalPath, "*.skin");

			PageThemeParser ptp = new PageThemeParser (physicalPath, context);
			
			string[] css_files = Directory.GetFiles (physicalPath, "*.css");
			string[] css_urls = new string[css_files.Length];
			for (int i = 0; i < css_files.Length; i++)
				css_urls [i] = virtualPath + Path.GetFileName (css_files [i]);
			ptp.LinkedStyleSheets = css_urls;
			
			ptp.RootBuilder = new RootBuilder ();

			for (int i = 0; i < skin_files.Length; i ++) {
				string skin_file_url = UrlUtils.Combine (physicalPath, Path.GetFileName (skin_files [i]));
				PageThemeFileParser ptfp = new PageThemeFileParser (skin_file_url,
										   skin_files[i],
										   context);

				AspGenerator gen = new AspGenerator (ptfp);
				gen.Parse ();

				foreach (object o in ptfp.RootBuilder.Children) {
					if (!(o is ControlBuilder))
						continue;
					ptp.RootBuilder.AppendSubBuilder ((ControlBuilder)o);
				}

				foreach (string ass in ptfp.Assemblies)
					if (!ptp.Assemblies.Contains (ass))
						ptp.AddAssemblyByFileName (ass);
			}

			PageThemeCompiler compiler = new PageThemeCompiler (ptp);
			return compiler.GetCompiledType ();
		}

		public static PageTheme GetCompiledInstance (string virtualPath, HttpContext context)
		{
			Type t = ThemeDirectoryCompiler.GetCompiledType (virtualPath, context);
			if (t == null)
				return null;

			PageTheme pt = (PageTheme)Activator.CreateInstance (t);
			return pt;
		}
	}
}

#endif
