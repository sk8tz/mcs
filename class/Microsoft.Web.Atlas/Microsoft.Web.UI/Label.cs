//
// Microsoft.Web.UI.Label
//
// Author:
//   Chris Toshok (toshok@ximian.com)
//
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.ComponentModel;
using System.Web.UI;
using Microsoft.Web;

namespace Microsoft.Web.UI
{
	public class Label : ScriptControl
	{
		public Label ()
		{
		}

		protected override void AddParsedSubObject (object obj)
		{
			base.AddParsedSubObject (obj);
		}

		protected override void InitializeTypeDescriptor (ScriptTypeDescriptor typeDescriptor)
		{
			base.InitializeTypeDescriptor (typeDescriptor);

			typeDescriptor.AddProperty (new ScriptPropertyDescriptor ("text", ScriptType.String, false, "Text"));
		}

		protected override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);

			ScriptManager mgr = ScriptManager.GetCurrentScriptManager (Page);
			mgr.RegisterScriptReference ("ScriptLibrary/AtlasUI.js", true);
			mgr.RegisterScriptReference ("ScriptLibrary/AtlasControls.js", true);
		}

		protected override void Render (HtmlTextWriter writer)
		{
		}

		public override string TagName {
			get {
				return "label";
			}
		}

		public string Text {
			get {
				object o = ViewState["Text"];
				if (o == null)
					return "";
				return (string)o;
			}
			set {
				if (value == null)
					ViewState.Remove ("Text");
				else
					ViewState["Text"] = value;
			}
		}
	}

}

#endif
