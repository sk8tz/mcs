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

		internal override void ProcessMainAttributes (Hashtable atts)
		{
			autoEventWireup = GetBool (atts, "AutoEventWireup", true);
			enableViewState = GetBool (atts, "EnableViewState", true);

			atts.Remove ("TargetSchema"); // Ignored

			base.ProcessMainAttributes (atts);
		}

		internal object GetCompiledInstance (string virtualPath, string inputFile, HttpContext context)
		{
			Context = context;
			InputFile = Path.Combine (MapPath (virtualPath), inputFile);
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
					throw new ParseException (Location, "No TagPrefix attribute found.");

				string ns = GetString (atts, "Namespace", null);
				string assembly = GetString (atts, "Assembly", null);

				if (ns != null && assembly == null)
					throw new ParseException (Location, "Need an Assembly attribute with Namespace.");

				if (ns == null && assembly != null)
					throw new ParseException (Location, "Need a Namespace attribute with Assembly.");
				
				if (ns != null) {
					if (atts.Count != 0)
						throw new HttpException ("Unknown attribute: " + GetOneKey (atts));

					AddImport (ns);
					Assembly ass = AddAssemblyByName (assembly);
					AddDependency (ass.Location);
					RootBuilder.Foundry.RegisterFoundry (tagprefix, ass, ns);
					return;
				}

				string tagname = GetString (atts, "TagName", null);
				string src = GetString (atts, "Src", null);

				if (tagname == null && src != null)
					throw new ParseException (Location, "Need a TagName attribute with Src.");

				if (tagname != null && src == null)
					throw new ParseException (Location, "Need a Src attribute with TagName.");

				if (!src.EndsWith (".ascx"))
					throw new ParseException (Location, "Source file extension for controls " + 
									    "must be .ascx");

				
				AddDependency (Path.Combine (MapPath (CurrentVirtualPath), src));
				Type type = UserControlParser.GetCompiledType (CurrentVirtualPath, src, Context);
				AddAssembly (type.Assembly, true);
				RootBuilder.Foundry.RegisterFoundry (tagprefix, tagname, type);
				return;
			}

			cmp = String.Compare ("Reference", directive, true);
			if (cmp == 0) {
				string page = GetString (atts, "Page", null);
				string control = GetString (atts, "Control", null);

				//TODO: compile and store control/page
				Console.WriteLine ("WARNING: Reference is not supported yet!");
				
				if (atts.Count != 0)
					throw new HttpException ("Unknown attribute: " + GetOneKey (atts));

				return;
			}


			atts.Remove ("OutputCache"); // ignored
			base.AddDirective (directive, atts);
		}

		protected override void HandleOptions (object obj)
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

