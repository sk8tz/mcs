//
// System.Web.UI.TemplateControlParser
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Web.Compilation;
using System.Web.Util;

namespace System.Web.UI
{
	public abstract class TemplateControlParser : TemplateParser
	{
		bool autoEventWireup = true;
		bool enableViewState = true;

		protected TemplateControlParser ()
		{
		}

		internal override void ProcessMainAttributes (Hashtable atts)
		{
			autoEventWireup = GetBool (atts, "AutoEventWireup", PagesConfig.AutoEventWireup);
			enableViewState = GetBool (atts, "EnableViewState", PagesConfig.EnableViewState);

			atts.Remove ("TargetSchema"); // Ignored

			base.ProcessMainAttributes (atts);
		}

		internal object GetCompiledInstance ()
		{
			Type type = CompileIntoType ();
			if (type == null)
				return null;

			object ctrl = Activator.CreateInstance (type);
			if (ctrl == null)
				return null;

			HandleOptions (ctrl);
			return ctrl;
		}

		internal override void AddDirective (string directive, Hashtable atts)
		{
			int cmp = String.Compare ("Register", directive, true);
			if (cmp == 0) {
				string tagprefix = GetString (atts, "TagPrefix", null);
				if (tagprefix == null || tagprefix.Trim () == "")
					ThrowParseException ("No TagPrefix attribute found.");

				string ns = GetString (atts, "Namespace", null);
				string assembly = GetString (atts, "Assembly", null);

				if (ns != null && assembly == null)
					ThrowParseException ("Need an Assembly attribute with Namespace.");

				if (ns == null && assembly != null)
					ThrowParseException ("Need a Namespace attribute with Assembly.");
				
				if (ns != null) {
					if (atts.Count != 0)
						ThrowParseException ("Unknown attribute: " + GetOneKey (atts));

					AddImport (ns);
					Assembly ass = AddAssemblyByName (assembly);
					AddDependency (ass.Location);
					RootBuilder.Foundry.RegisterFoundry (tagprefix, ass, ns);
					return;
				}

				string tagname = GetString (atts, "TagName", null);
				string src = GetString (atts, "Src", null);

				if (tagname == null && src != null)
					ThrowParseException ("Need a TagName attribute with Src.");

				if (tagname != null && src == null)
					ThrowParseException ("Need a Src attribute with TagName.");

				if (!src.EndsWith (".ascx"))
					ThrowParseException ("Source file extension for controls must be .ascx");

				string realpath = MapPath (src);
				if (!File.Exists (realpath))
					throw new ParseException (Location, "Could not find file \"" 
						+ realpath + "\".");

				try {
					AddDependency (realpath);
				} catch (Exception e) {
					throw new ParseException (Location, e.Message);
				}

				string vpath = UrlUtils.Combine (BaseVirtualDir, src);
				Type type = UserControlParser.GetCompiledType (vpath, realpath, Context);
				AddAssembly (type.Assembly, true);
				RootBuilder.Foundry.RegisterFoundry (tagprefix, tagname, type);
				return;
			}

			cmp = String.Compare ("Reference", directive, true);
			if (cmp == 0) {
				string page = GetString (atts, "Page", null);
				string control = GetString (atts, "Control", null);

				bool is_page = (page != null);
				if (!is_page && control == null)
					ThrowParseException ("Must provide 'page' or 'control' attribute");

				if (is_page && control != null)
					ThrowParseException ("'page' and 'control' are mutually exclusive");

				string filepath = (!is_page) ? control : page;
				filepath = MapPath (filepath);
				AddDependency (filepath);
				Type ctype;
				if (is_page) {
					PageParser pp = new PageParser (page, filepath, Context);
					ctype = pp.CompileIntoType ();
				} else {
					ctype = UserControlParser.GetCompiledType (control, filepath, Context);
				}

				AddAssembly (ctype.Assembly, true);
				if (atts.Count != 0)
					ThrowParseException ("Unknown attribute: " + GetOneKey (atts));

				return;
			}

			base.AddDirective (directive, atts);
		}

		internal override void HandleOptions (object obj)
		{
			Control ctrl = obj as Control;
			ctrl.AutoEventWireup = autoEventWireup;
			ctrl.EnableViewState = enableViewState;
		}

		internal bool AutoEventWireup {
			get { return autoEventWireup; }
		}

		internal bool EnableViewState {
			get { return enableViewState; }
		}
	}
}

