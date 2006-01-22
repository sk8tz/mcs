//
// System.Web.UI.RootBuilder
//
// Authors:
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc. (http://www.ximian.com)
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

using System.Collections;
using System.Security.Permissions;
using System.Web.Compilation;
using System.Web.UI.HtmlControls;

namespace System.Web.UI {

	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#if NET_2_0
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class RootBuilder : TemplateBuilder {

		private Hashtable built_objects;

		public RootBuilder ()
		{
		}
#else
	public sealed class RootBuilder : TemplateBuilder {
#endif
		static Hashtable htmlControls;
		static Hashtable htmlInputControls;
		AspComponentFoundry foundry;

		static RootBuilder ()
		{
#if NET_2_0
			htmlControls = new Hashtable (StringComparer.InvariantCultureIgnoreCase);
#else
			htmlControls = new Hashtable (CaseInsensitiveHashCodeProvider.DefaultInvariant,
						      CaseInsensitiveComparer.DefaultInvariant); 
#endif

			htmlControls.Add ("A", typeof (HtmlAnchor));
			htmlControls.Add ("BUTTON", typeof (HtmlButton));
			htmlControls.Add ("FORM", typeof (HtmlForm));
#if NET_2_0
			htmlControls.Add ("HEAD", typeof (HtmlHead));
#endif
			htmlControls.Add ("IMG", typeof (HtmlImage));
			htmlControls.Add ("INPUT", "INPUT");
#if NET_2_0
			htmlControls.Add ("LINK", typeof (HtmlLink));
			htmlControls.Add ("META", typeof (HtmlLink));
#endif
			htmlControls.Add ("SELECT", typeof (HtmlSelect));
			htmlControls.Add ("TABLE", typeof (HtmlTable));
			htmlControls.Add ("TD", typeof (HtmlTableCell));
			htmlControls.Add ("TH", typeof (HtmlTableCell));
			htmlControls.Add ("TR", typeof (HtmlTableRow));
			htmlControls.Add ("TEXTAREA", typeof (HtmlTextArea));

			htmlInputControls = new Hashtable (CaseInsensitiveHashCodeProvider.DefaultInvariant,
						      CaseInsensitiveComparer.DefaultInvariant); 

			htmlInputControls.Add ("BUTTON", typeof (HtmlInputButton));
#if NET_2_0
			htmlInputControls.Add ("SUBMIT", typeof (HtmlInputSubmit));
			htmlInputControls.Add ("RESET", typeof (HtmlInputReset));
#else
			htmlInputControls.Add ("SUBMIT", typeof (HtmlInputButton));
			htmlInputControls.Add ("RESET", typeof (HtmlInputButton));
#endif
			htmlInputControls.Add ("CHECKBOX", typeof (HtmlInputCheckBox));
			htmlInputControls.Add ("FILE", typeof (HtmlInputFile));
			htmlInputControls.Add ("HIDDEN", typeof (HtmlInputHidden));
			htmlInputControls.Add ("IMAGE", typeof (HtmlInputImage));
			htmlInputControls.Add ("RADIO", typeof (HtmlInputRadioButton));
			htmlInputControls.Add ("TEXT", typeof (HtmlInputText));
#if NET_2_0
			htmlInputControls.Add ("PASSWORD", typeof (HtmlInputPassword));
#else
			htmlInputControls.Add ("PASSWORD", typeof (HtmlInputText));
#endif
		}

		public RootBuilder (TemplateParser parser)
		{
			foundry = new AspComponentFoundry ();
			line = 1;
			if (parser != null)
				fileName = parser.InputFile;
			Init (parser, null, null, null, null, null);
		}

		public override Type GetChildControlType (string tagName, IDictionary attribs) 
		{
			if (tagName == null)
				throw new ArgumentNullException ("tagName");

			string prefix;
			string cname;
			int colon = tagName.IndexOf (':');
			if (colon != -1) {
				if (colon == 0)
					throw new Exception ("Empty TagPrefix is not valid");

				if (colon + 1 == tagName.Length)
					return null;

				prefix = tagName.Substring (0, colon);
				cname = tagName.Substring (colon + 1);
			} else {
				prefix = "";
				cname = tagName;
			}

			Type t = foundry.GetComponentType (prefix, cname);
			if (t != null)
				return t;
			else if (prefix != "")
				throw new Exception ("TagPrefix " + prefix + " not registered");
			
			return LookupHtmlControls (tagName, attribs);
		}

		static Type LookupHtmlControls (string tagName, IDictionary attribs)
		{
			object o = htmlControls [tagName];
			if (o is string) {
				if (attribs == null)
					throw new HttpException ("Unable to map input type control to a Type.");

				string ctype = attribs ["TYPE"] as string;
				if (ctype == null)
					ctype = "TEXT"; // The default used by MS

				Type t = htmlInputControls [ctype] as Type;
				if (t == null)
					throw new HttpException ("Unable to map input type control to a Type.");

				return t;
			}

			if (o == null)
				o = typeof (HtmlGenericControl);

			return (Type) o;
		}

		internal AspComponentFoundry Foundry {
			get { return foundry; }
		}
#if NET_2_0
		// FIXME: it's empty (but not null) when using the new default ctor
		// but I'm not sure when something should gets in...
		public IDictionary BuiltObjects {
			get {
				if (built_objects == null)
					built_objects = new Hashtable ();
				return built_objects;
			}
		}
#endif
	}
}
