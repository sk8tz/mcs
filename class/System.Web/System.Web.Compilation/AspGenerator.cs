//
// System.Web.Compilation.AspGenerator
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace System.Web.Compilation
{

class ControlStack
{
	private Stack controls;
	private ControlStackData top;
	private bool space_between_tags;
	private bool sbt_valid;

	class ControlStackData 
	{
		public Type controlType;
		public string controlID;
		public string tagID;
		public ChildrenKind childKind;
		public string defaultPropertyName;
		public int childrenNumber;
		public Type container;
		public StringBuilder dataBindFunction;
		public StringBuilder codeRenderFunction;
		public bool useCodeRender;

		public ControlStackData (Type controlType,
					 string controlID,
					 string tagID,
					 ChildrenKind childKind,
					 string defaultPropertyName,
					 Type container)
		{
			this.controlType = controlType;
			this.controlID = controlID;
			this.tagID = tagID;
			this.childKind = childKind;
			this.defaultPropertyName = defaultPropertyName;
			this.container = container;
			childrenNumber = 0;
		}

		public override string ToString ()
		{
			return controlType + " " + controlID + " " + tagID + " " + childKind + " " + childrenNumber;
		}
	}
	
	public ControlStack ()
	{
		controls = new Stack ();
	}

	private Type GetContainerType (Type type)
	{
		if (type != typeof (System.Web.UI.Control) &&
		    !type.IsSubclassOf (typeof (System.Web.UI.Control)))
			return null;
		
		Type container_type;
		if (type == typeof (System.Web.UI.WebControls.DataList))
			container_type = typeof (System.Web.UI.WebControls.DataListItem);
		else if (type == typeof (System.Web.UI.WebControls.DataGrid))
			container_type = typeof (System.Web.UI.WebControls.DataGridItem);
		else if (type == typeof (System.Web.UI.WebControls.Repeater))
			container_type = typeof (System.Web.UI.WebControls.RepeaterItem);
		else 
			container_type = type;

		return container_type;
	}

	public void Push (Type controlType,
			  string controlID,
			  string tagID,
			  ChildrenKind childKind,
			  string defaultPropertyName)
	{
		Type container_type = null;
		if (controlType != null){
			AddChild ();
			container_type = GetContainerType (controlType);
			if (container_type == null)
				container_type = this.Container;
		}

		top = new ControlStackData (controlType,
					    controlID,
					    tagID,
					    childKind,
					    defaultPropertyName,
					    container_type);
		sbt_valid = false;
		controls.Push (top);
	}

	public void Pop ()
	{
		controls.Pop ();
		if (controls.Count != 0)
			top = (ControlStackData) controls.Peek ();
		sbt_valid = false;
	}

	public Type PeekType ()
	{
		return top.controlType;
	}

	public string PeekControlID ()
	{
		return top.controlID;
	}

	public string PeekTagID ()
	{
		return top.tagID;
	}

	public ChildrenKind PeekChildKind ()
	{
		return top.childKind;
	}

	public string PeekDefaultPropertyName ()
	{
		return top.defaultPropertyName;
	}

	public void AddChild ()
	{
		if (top != null)
			top.childrenNumber++;
	}

	public bool HasDataBindFunction ()
	{
		if (top.dataBindFunction == null || top.dataBindFunction.Length == 0)
			return false;
		return true;
	}
	
	public bool UseCodeRender
	{
		get {
			if (top.codeRenderFunction == null || top.codeRenderFunction.Length == 0)
				return false;
			return top.useCodeRender;
		}

		set { top.useCodeRender= value; }
	}
	
	public bool SpaceBetweenTags
	{
		get {
			if (!sbt_valid){
				sbt_valid = true;
				Type type = top.controlType;
				if (type.Namespace == "System.Web.UI.WebControls")
					space_between_tags = true;
				else if (type.IsSubclassOf (typeof (System.Web.UI.WebControls.WebControl)))
					space_between_tags = true;
				else if (type == typeof (System.Web.UI.HtmlControls.HtmlSelect))
					space_between_tags = true;
				else
					space_between_tags = false;
			}
			return space_between_tags;
		}
	}
	
	public Type Container
	{
		get { return top.container; }
	}
	
	public StringBuilder DataBindFunction
	{
		get {
			if (top.dataBindFunction == null)
				top.dataBindFunction = new StringBuilder ();
			return top.dataBindFunction;
		}
	}

	public StringBuilder CodeRenderFunction
	{
		get {
			if (top.codeRenderFunction == null)
				top.codeRenderFunction = new StringBuilder ();
			return top.codeRenderFunction;
		}
	}

	public int ChildIndex
	{
		get { return top.childrenNumber - 1; }
	}
	
	public int Count
	{
		get { return controls.Count; }
	}

	public override string ToString ()
	{
		return top.ToString () + " " + top.useCodeRender;
	}
		
}

class ArrayListWrapper
{
	private ArrayList list;
	private int index;

	public ArrayListWrapper (ArrayList list)
	{
		this.list = list;
		index = -1;
	}

	private void CheckIndex ()
	{
		if (index == -1 || index == list.Count)
			throw new InvalidOperationException ();
	}
			
	public object Current
	{
		get {
			CheckIndex ();
			return list [index];
		}

		set {
			CheckIndex ();
			list [index] = value;
		}
	}

	public bool MoveNext ()
	{
		if (index < list.Count)
			index++;

		return index < list.Count;
	}
}

class AspGenerator
{
	private object [] parts;
	private ArrayListWrapper elements;
	private StringBuilder buildOptions;
	private StringBuilder prolog;
	private StringBuilder declarations;
	private StringBuilder script;
	private StringBuilder constructor;
	private StringBuilder init_funcs;
	private StringBuilder epilog;
	private StringBuilder current_function;
	private Stack functions;
	private ControlStack controls;
	private bool parse_ok;
	private bool has_form_tag;
	private AspComponentFoundry aspFoundry;

	private string classDecl;
	private string className;
	private string interfaces;
	private string parent;
	private string fullPath;
	private static string enableSessionStateLiteral =  ", System.Web.SessionState.IRequiresSessionState";

	enum UserControlResult
	{
		OK = 0,
		FileNotFound = 1,
		XspFailed = 2,
		CompilationFailed = 3
	}

	public AspGenerator (string pathToFile, ArrayList elements)
	{
		if (elements == null)
			throw new ArgumentNullException ();

		this.elements = new ArrayListWrapper (elements);
		string filename = Path.GetFileName (pathToFile);
		this.className = filename.Replace ('.', '_'); // Overridden by @ Page classname
		this.className = className.Replace ('-', '_'); 
		this.className = className.Replace (' ', '_');
		this.fullPath = Path.GetFullPath (pathToFile);
		/*
		if (IsUserControl) {
			this.parent = "System.Web.UI.UserControl"; // Overriden by @ Control Inherits
			this.interfaces = "";
		} else {
			this.parent = "System.Web.UI.Page"; // Overriden by @ Page Inherits
			this.interfaces = enableSessionStateLiteral;
		}
		//
		//*/
		this.has_form_tag = false;
		Init ();
	}

	public string BaseType
	{
		get {
			return parent;
		}

		set {
			parent = value;
		}
	}

	public bool IsUserControl
	{
		get {
			return (BaseType == typeof (UserControl).ToString ());
		}
	}
	
	public string Interfaces 
	{
		get {
			return interfaces;
		}
	}

	public void AddInterface (string iface)
	{
		if (interfaces == "") {
			interfaces = iface;
		} else {
			string s = ", " + iface;
			if (interfaces.IndexOf (s) == -1)
				interfaces += s;
		}
	}

	private AspComponentFoundry Foundry
	{
		get {
			if (aspFoundry == null)
				aspFoundry = new AspComponentFoundry ();

			return aspFoundry;
		}
	}

	private void Init ()
	{
		controls = new ControlStack ();
		controls.Push (typeof (System.Web.UI.Control), "Root", null, ChildrenKind.CONTROLS, null);
		prolog = new StringBuilder ();
		declarations = new StringBuilder ();
		script = new StringBuilder ();
		constructor = new StringBuilder ();
		init_funcs = new StringBuilder ();
		epilog = new StringBuilder ();
		buildOptions = new StringBuilder ();

		current_function = new StringBuilder ();
		functions = new Stack ();
		functions.Push (current_function);

		parts = new Object [7];
		parts [0] = buildOptions;
		parts [1] = prolog;
		parts [2] = declarations;
		parts [3] = script;
		parts [4] = constructor;
		parts [5] = init_funcs;
		parts [6] = epilog;

		prolog.Append ("namespace ASP {\n" +
			      "\tusing System;\n" + 
			      "\tusing System.Collections;\n" + 
			      "\tusing System.Collections.Specialized;\n" + 
			      "\tusing System.Configuration;\n" + 
			      "\tusing System.IO;\n" + 
			      "\tusing System.Text;\n" + 
			      "\tusing System.Text.RegularExpressions;\n" + 
			      "\tusing System.Web;\n" + 
			      "\tusing System.Web.Caching;\n" + 
			      "\tusing System.Web.Security;\n" + 
			      "\tusing System.Web.SessionState;\n" + 
			      "\tusing System.Web.UI;\n" + 
			      "\tusing System.Web.UI.WebControls;\n" + 
			      "\tusing System.Web.UI.HtmlControls;\n");

		declarations.Append ("\t\tprivate static int __autoHandlers;\n");

		current_function.Append ("\t\tprivate void __BuildControlTree (System.Web.UI.Control __ctrl)\n\t\t{\n");
		if (!IsUserControl)
			current_function.Append ("\t\t\tSystem.Web.UI.IParserAccessor __parser = " + 
						 "(System.Web.UI.IParserAccessor) __ctrl;\n\n");
		else
			controls.UseCodeRender = true;
	}

	public StringReader GetCode ()
	{
		if (!parse_ok)
			throw new ApplicationException ("You gotta call ProcessElements () first!");

		StringBuilder code = new StringBuilder ();
		for (int i = 0; i < parts.Length; i++)
			code.Append ((StringBuilder) parts [i]);

		return new StringReader (code.ToString ());
	}

	public void Print ()
	{
		if (!parse_ok){
			Console.WriteLine ("//Warning!!!: Elements not correctly parsed.");
		}

		Console.Write (GetCode ().ReadToEnd ());
	}

	// Regex.Escape () make some illegal escape sequences for a C# source.
	private string Escape (string input)
	{
		string output = input.Replace ("\\", "\\\\");
		output = output.Replace ("\"", "\\\"");
		output = output.Replace ("\t", "\\t");
		output = output.Replace ("\r", "\\r");
		output = output.Replace ("\n", "\\n");
		output = output.Replace ("\n", "\\n");
		return output;
	}
	
	private void PageDirective (TagAttributes att)
	{
		if (att ["ClassName"] != null){
			this.className = (string) att ["ClassName"];
		}

		if (att ["EnableSessionState"] != null){
			string est = (string) att ["EnableSessionState"];
			if (0 == String.Compare (est, "false", true))
				interfaces = interfaces.Replace (enableSessionStateLiteral, "");
			else if (0 != String.Compare (est, "true", true))
				throw new ApplicationException ("EnableSessionState in Page directive not set to " +
								"a correct value: " + est);
		}

		/*
		if (att ["Inherits"] != null){
			parent = (string) att ["Inherits"];
			string source_file = att ["Src"] as string;
			if (source_file != null)
				buildOptions.AppendFormat ("//<compileandreference src=\"{0}\"/>\n", source_file);
			else
				buildOptions.AppendFormat ("//<reference dll=\"{0}\"/>\n", parent);

		}
		*/

		if (att ["CompilerOptions"] != null){
			string compilerOptions = (string) att ["CompilerOptions"];
			buildOptions.AppendFormat ("//<compileroptions options=\"{0}\"/>\n", compilerOptions);
		}

		//FIXME: add support for more attributes.
	}

	private void RegisterDirective (TagAttributes att)
	{
		string tag_prefix = (string) (att ["tagprefix"] == null ?  "" : att ["tagprefix"]);
		string name_space = (string) (att ["namespace"] == null ?  "" : att ["namespace"]);
		string assembly_name = (string) (att ["assembly"] == null ?  "" : att ["assembly"]);
		string tag_name =  (string) (att ["tagname"] == null ?  "" : att ["tagname"]);
		string src = (string) (att ["src"] == null ?  "" : att ["src"]);

		if (tag_prefix != "" && name_space != "" && assembly_name != ""){
			if (tag_name != "" || src != "")
				throw new ApplicationException ("Invalid attributes for @ Register: " +
								att.ToString ());
			prolog.AppendFormat ("\tusing {0};\n", name_space);
			string dll = "output" + Path.DirectorySeparatorChar + assembly_name + ".dll";
			Foundry.RegisterFoundry (tag_prefix, dll, name_space);
			buildOptions.AppendFormat ("//<reference dll=\"{0}\"/>\n", dll);
			return;
		}

		if (tag_prefix != "" && tag_name != "" && src != ""){
			if (name_space != "" && assembly_name != "")
				throw new ApplicationException ("Invalid attributes for @ Register: " +
								att.ToString ());
			
			if (!src.EndsWith (".ascx"))
				throw new ApplicationException ("Source file extension for controls " + 
								"must be .ascx");

			string pathToFile = Path.GetDirectoryName (src);
			if (pathToFile == "") {
				pathToFile = Path.GetDirectoryName (fullPath);
			} else if (!Path.IsPathRooted (pathToFile)) {
				pathToFile = Path.Combine  (Path.GetDirectoryName (fullPath), pathToFile);
			}

			string srcLocation = pathToFile + Path.DirectorySeparatorChar + Path.GetFileName (src);
			UserControlData data = GenerateUserControl (srcLocation);
			switch (data.result) {
			case UserControlResult.OK:
				prolog.AppendFormat ("\tusing {0};\n", "ASP");
				string dll = "output" + Path.DirectorySeparatorChar + data.assemblyName + ".dll";
				Foundry.RegisterFoundry (tag_prefix, data.assemblyName, "ASP", data.className);
				buildOptions.AppendFormat ("//<reference dll=\"{0}\"/>\n", data.assemblyName);
				break;
			case UserControlResult.FileNotFound:
				throw new ApplicationException ("File '" + src + "' not found.");
			case UserControlResult.XspFailed:
				//TODO
				throw new NotImplementedException ();
			case UserControlResult.CompilationFailed:
				//TODO: should say where the generated .cs file is for the server to
				//show the source and the compiler error
				throw new NotImplementedException ();
			}
			return;
		}

		throw new ApplicationException ("Invalid combination of attributes in " +
						"@ Register: " + att.ToString ());
	}

	private void ProcessDirective ()
	{
		Directive directive = (Directive) elements.Current;
		TagAttributes att = directive.Attributes;
		if (att == null)
			return;

		string id = directive.TagID.ToUpper ();
		switch (id){
		case "PAGE":
		case "CONTROL":
			if (IsUserControl && id != "CONTROL")
				throw new ApplicationException ("@Page not allowed if --control specified.");
			else if (!IsUserControl && id != "PAGE")
				throw new ApplicationException ("@Control not allowed here.");
			PageDirective (att);
			break;
		case "IMPORT":
			foreach (string key in att.Keys){
				if (0 == String.Compare (key, "NAMESPACE", true)){
					string _using = "using " + (string) att [key] + ";";
					if (prolog.ToString ().IndexOf (_using) == -1)
						prolog.AppendFormat ("\tusing {0};\n", (string) att [key]);
					break;
				}
			}
			break;
		case "IMPLEMENTS":
			string iface = (string) att ["interface"];
			interfaces += ", " + iface;
			break;
		case "REGISTER":
			RegisterDirective (att);
			break;
		}
	}

	private void ProcessPlainText ()
	{
		PlainText asis = (PlainText) elements.Current;
		string trimmed = asis.Text.Trim ();
		if (trimmed == "" && controls.SpaceBetweenTags == true)
			return;

		if (trimmed != "" && controls.PeekChildKind () != ChildrenKind.CONTROLS){
			string tag_id = controls.PeekTagID ();
			throw new ApplicationException ("Literal content not allowed for " + tag_id);
		}
		
		string escaped_text = Escape (asis.Text);
		current_function.AppendFormat ("\t\t\t__parser.AddParsedSubObject (" + 
					       "new System.Web.UI.LiteralControl (\"{0}\"));\n",
					       escaped_text);
		StringBuilder codeRenderFunction = controls.CodeRenderFunction;
		codeRenderFunction.AppendFormat ("\t\t\t__output.Write (\"{0}\");\n", escaped_text);
	}

	private string EnumValueNameToString (Type enum_type, string value_name)
	{
		if (value_name.EndsWith ("*"))
			throw new ApplicationException ("Invalid property value: '" + value_name + 
							". It must be a valid " + enum_type.ToString () + " value.");

		MemberInfo [] nested_types = enum_type.FindMembers (MemberTypes.Field, 
								    BindingFlags.Public | BindingFlags.Static,
								    Type.FilterNameIgnoreCase,
								    value_name);

		if (nested_types.Length == 0)
			throw new ApplicationException ("Value " + value_name + " not found in enumeration " +
							enum_type.ToString ());
		if (nested_types.Length > 1)
			throw new ApplicationException ("Value " + value_name + " found " + 
							nested_types.Length + " in enumeration " +
							enum_type.ToString ());

		return enum_type.ToString () + "." + nested_types [0].Name;
	}
	
	private void NewControlFunction (string tag_id,
					 string control_id,
					 Type control_type,
					 ChildrenKind children_kind,
					 string defaultPropertyName)
	{
		ChildrenKind prev_children_kind = controls.PeekChildKind ();
		if (prev_children_kind == ChildrenKind.NONE || 
		    prev_children_kind == ChildrenKind.PROPERTIES){
			string prev_tag_id = controls.PeekTagID ();
			throw new ApplicationException ("Child controls not allowed for " + prev_tag_id);
		}

		if (prev_children_kind == ChildrenKind.DBCOLUMNS &&
		    control_type != typeof (System.Web.UI.WebControls.DataGridColumn) &&
		    !control_type.IsSubclassOf (typeof (System.Web.UI.WebControls.DataGridColumn)))
			throw new ApplicationException ("Inside " + controls.PeekTagID () + " only " + 
							"System.Web.UI.WebControls.DataGridColum " + 
							"objects are allowed");
		else if (prev_children_kind == ChildrenKind.LISTITEM &&
			 control_type != typeof (System.Web.UI.WebControls.ListItem))
			throw new ApplicationException ("Inside " + controls.PeekTagID () + " only " + 
							"System.Web.UI.WebControls.ListItem " + 
							"objects are allowed");
	
					
		StringBuilder func_code = new StringBuilder ();
		current_function = func_code;
		if (0 == String.Compare (tag_id, "form", true)){
			if (has_form_tag)
				throw new ApplicationException ("Only one form server tag allowed.");
			has_form_tag = true;
		}

		controls.Push (control_type, control_id, tag_id, children_kind, defaultPropertyName);
		bool is_generic = control_type ==  typeof (System.Web.UI.HtmlControls.HtmlGenericControl);
		functions.Push (current_function);
		if (control_type != typeof (System.Web.UI.WebControls.ListItem))
			current_function.AppendFormat ("\t\tprivate System.Web.UI.Control __BuildControl_" +
							"{0} ()\n\t\t{{\n\t\t\t{1} __ctrl;\n\n\t\t\t__ctrl" +
							" = new {1} ({2});\n\t\t\tthis.{0} = __ctrl;\n",
							control_id, control_type,
							(is_generic? "\"" + tag_id + "\"" : ""));
		else
			current_function.AppendFormat ("\t\tprivate void __BuildControl_{0} ()\n\t\t{{" +
							"\n\t\t\t{1} __ctrl;\n\t\t\t__ctrl = new {1} ();" +
							"\n\t\t\tthis.{0} = __ctrl;\n",
							control_id, control_type);

		if (children_kind == ChildrenKind.CONTROLS || children_kind == ChildrenKind.OPTION)
			current_function.Append ("\t\t\tSystem.Web.UI.IParserAccessor __parser = " + 
						 "(System.Web.UI.IParserAccessor) __ctrl;\n");
	}
	
	private void DataBoundProperty (string varName, string value)
	{
		if (value == "")
			throw new ApplicationException ("Empty data binding tag.");

		string control_id = controls.PeekControlID ();
		string control_type_string = controls.PeekType ().ToString ();
		StringBuilder db_function = controls.DataBindFunction;
		string container = "System.Web.UI.Control";
		if (db_function.Length == 0)
			db_function.AppendFormat ("\t\tpublic void __DataBind_{0} (object sender, " + 
						  "System.EventArgs e) {{\n" +
						  "\t\t\t{1} Container;\n" +
						  "\t\t\t{2} target;\n" +
						  "\t\t\ttarget = ({2}) sender;\n" +
						  "\t\t\tContainer = ({1}) target.BindingContainer;\n",
						  control_id, container, control_type_string);

		/* Removes '<%#' and '%>' */
		string real_value = value.Remove (0,3);
		real_value = real_value.Remove (real_value.Length - 2, 2);
		real_value = real_value.Trim ();

		db_function.AppendFormat ("\t\t\ttarget.{0} = System.Convert.ToString ({1});\n",
					  varName, real_value);
	}

	/*
	 * Returns true if it generates some code for the specified property
	 */
	private void AddPropertyCode (Type prop_type, string var_name, string att, bool isDataBound)
	{
		/* FIXME: should i check for this or let the compiler fail?
		 * if (!prop.CanWrite)
		 *    ....
		 */
		if (prop_type == typeof (string)){
			if (att == null)
				throw new ApplicationException ("null value for attribute " + var_name );

			if (isDataBound)
				DataBoundProperty (var_name, att);
			else
				current_function.AppendFormat ("\t\t\t__ctrl.{0} = \"{1}\";\n", var_name,
								Escape (att)); // FIXME: really Escape this?
				
		} 
		else if (prop_type.IsEnum){
			if (att == null)
				throw new ApplicationException ("null value for attribute " + var_name );

			string enum_value = EnumValueNameToString (prop_type, att);

			current_function.AppendFormat ("\t\t\t__ctrl.{0} = {1};\n", var_name, enum_value);
		} 
		else if (prop_type == typeof (bool)){
			string value;
			if (att == null)
				value = "true"; //FIXME: is this ok for non Style properties?
			else if (0 == String.Compare (att, "true", true))
				value = "true";
			else if (0 == String.Compare (att, "false", true))
				value = "false";
			else
				throw new ApplicationException ("Value '" + att  + "' is not a valid boolean.");

			current_function.AppendFormat ("\t\t\t__ctrl.{0} = {1};\n", var_name, value);
		}
		else if (prop_type == typeof (System.Web.UI.WebControls.Unit)){
			 //FIXME: should use the culture specified in Page
			try {
				Unit value = Unit.Parse (att, System.Globalization.CultureInfo.InvariantCulture);
			} catch (Exception) {
				throw new ApplicationException ("'" + att + "' cannot be parsed as a unit.");
			}
			current_function.AppendFormat ("\t\t\t__ctrl.{0} = " + 
							"System.Web.UI.WebControls.Unit.Parse (\"{1}\", " + 
							"System.Globalization.CultureInfo.InvariantCulture);\n", 
							var_name, att);
		}
		else if (prop_type == typeof (System.Web.UI.WebControls.FontUnit)){
			 //FIXME: should use the culture specified in Page
			try {
				FontUnit value = FontUnit.Parse (att, System.Globalization.CultureInfo.InvariantCulture);
			} catch (Exception) {
				throw new ApplicationException ("'" + att + "' cannot be parsed as a unit.");
			}
			current_function.AppendFormat ("\t\t\t__ctrl.{0} = " + 
							"System.Web.UI.WebControls.FontUnit.Parse (\"{1}\", " + 
							"System.Globalization.CultureInfo.InvariantCulture);\n", 
							var_name, att);
		}
		else if (prop_type == typeof (Int16) ||
			 prop_type == typeof (Int32) ||
			 prop_type == typeof (Int64)){
			long value;
			try {
				value = Int64.Parse (att); //FIXME: should use the culture specified in Page
			} catch (Exception){
				throw new ApplicationException (att + " is not a valid signed number " + 
								"or is out of range.");
			}

			current_function.AppendFormat ("\t\t\t__ctrl.{0} = {1};\n", var_name, value);
		}
		else if (prop_type == typeof (UInt16) ||
			 prop_type == typeof (UInt32) ||
			 prop_type == typeof (UInt64)){
			ulong value;
			try {
				value = UInt64.Parse (att); //FIXME: should use the culture specified in Page
			} catch (Exception){
				throw new ApplicationException (att + " is not a valid unsigned number " + 
								"or is out of range.");
			}

			current_function.AppendFormat ("\t\t\t__ctrl.{0} = {1};\n", var_name, value);
		}
		else if (prop_type == typeof (float)){
			float value;
			try {
				value = Single.Parse (att);
			} catch (Exception){
				throw new ApplicationException (att + " is not  avalid float number or " +
								"is out of range.");
			}

			current_function.AppendFormat ("\t\t\t__ctrl.{0} = {1};\n", var_name, value);
		}
		else if (prop_type == typeof (double)){
			double value;
			try {
				value = Double.Parse (att);
			} catch (Exception){
				throw new ApplicationException (att + " is not  avalid double number or " +
								"is out of range.");
			}

			current_function.AppendFormat ("\t\t\t__ctrl.{0} = {1};\n", var_name, value);
		}
		else if (prop_type == typeof (System.Drawing.Color)){
			Color c;
			try {
				c = (Color) TypeDescriptor.GetConverter (typeof (Color)).ConvertFromString (att);
			} catch (Exception e){
				throw new ApplicationException ("Color " + att + " is not a valid color.", e);
			}

			// Should i also test for IsSystemColor?
			// Are KnownColor members in System.Drawing.Color?
			if (c.IsKnownColor){
				current_function.AppendFormat ("\t\t\t__ctrl.{0} = System.Drawing.Color." +
							       "{1};\n", var_name, c.Name);
			}
			else {
				current_function.AppendFormat ("\t\t\t__ctrl.{0} = System.Drawing.Color." +
							       "FromArgb ({1}, {2}, {3}, {4});\n",
							       var_name, c.A, c.R, c.G, c.B);
			}
		}	
		else {
			throw new ApplicationException ("Unsupported type in property: " + 
							prop_type.ToString ());
		}
	}

	private bool ProcessProperties (PropertyInfo prop, string id, TagAttributes att)
	{
		int hyphen = id.IndexOf ('-');

		if (hyphen == -1 && prop.CanWrite == false)
			return false;

		bool is_processed = false;
		bool isDataBound = att.IsDataBound ((string) att [id]);
		Type type = prop.PropertyType;
		Type style = typeof (System.Web.UI.WebControls.Style);
		Type fontinfo = typeof (System.Web.UI.WebControls.FontInfo);

		if (0 == String.Compare (prop.Name, id, true)){
			AddPropertyCode (type, prop.Name, (string) att [id], isDataBound);
			is_processed = true;
		} else if ((type == fontinfo || type == style || type.IsSubclassOf (style)) && hyphen != -1){
			string prop_field = id.Replace ("-", ".");
			string [] parts = prop_field.Split (new char [] {'.'});
			if (parts.Length != 2 || 0 != String.Compare (prop.Name, parts [0], true))
				return false;

			PropertyInfo [] subprops = type.GetProperties ();
			foreach (PropertyInfo subprop in subprops){
				if (0 != String.Compare (subprop.Name, parts [1], true))
					continue;

				if (subprop.CanWrite == false)
					return false;

				bool is_bool = subprop.PropertyType == typeof (bool);
				if (!is_bool && att == null){
					att [id] = ""; // Font-Size -> Font-Size="" as html
					return false;
				}

				string value;
				if (att == null && is_bool)
					value = "true"; // Font-Bold <=> Font-Bold="true"
				else
					value = (string) att [id];

				AddPropertyCode (subprop.PropertyType,
						 prop.Name + "." + subprop.Name,
						 value, isDataBound);
				is_processed = true;
			}
		}

		return is_processed;
	}
	
	private void AddCodeForAttributes (Type type, TagAttributes att)
	{
		EventInfo [] ev_info = type.GetEvents ();
		PropertyInfo [] prop_info = type.GetProperties ();
		bool is_processed = false;
		ArrayList processed = new ArrayList ();

		foreach (string id in att.Keys){
			if (0 == String.Compare (id, "runat", true) || 0 == String.Compare (id, "id", true))
				continue;

			if (id.Length > 2 && id.Substring (0, 2).ToUpper () == "ON"){
				string id_as_event = id.Substring (2);
				foreach (EventInfo ev in ev_info){
					if (0 == String.Compare (ev.Name, id_as_event, true)){
						current_function.AppendFormat (
								"\t\t\t__ctrl.{0} += " + 
								"new {1} (this.{2});\n", 
								ev.Name, ev.EventHandlerType, att [id]);
						is_processed = true;
						break;
					}
				}
				if (is_processed){
					is_processed = false;
					continue;
				}
			} 

			foreach (PropertyInfo prop in prop_info){
				is_processed = ProcessProperties (prop, id, att);
				if (is_processed)
					break;
			}

			if (is_processed){
				is_processed = false;
				continue;
			}

			current_function.AppendFormat ("\t\t\t((System.Web.UI.IAttributeAccessor) __ctrl)." +
						"SetAttribute (\"{0}\", \"{1}\");\n",
						id, Escape ((string) att [id]));
		}
	}
	
	private void AddCodeRenderControl (StringBuilder function, int index)
	{
		function.AppendFormat ("\t\t\tparameterContainer.Controls [{0}]." + 
				       "RenderControl (__output);\n", index);
	}

	private void AddRenderMethodDelegate (StringBuilder function, string control_id)
	{
		function.AppendFormat ("\t\t\t__ctrl.SetRenderMethodDelegate (new System.Web." + 
				       "UI.RenderMethod (this.__Render_{0}));\n", control_id);
	}

	private void AddCodeRenderFunction (string codeRender, string control_id)
	{
		StringBuilder codeRenderFunction = new StringBuilder ();
		codeRenderFunction.AppendFormat ("\t\tprivate void __Render_{0} " + 
						 "(System.Web.UI.HtmlTextWriter __output, " + 
						 "System.Web.UI.Control parameterContainer)\n" +
						 "\t\t{{\n", control_id);
		codeRenderFunction.Append (codeRender);
		codeRenderFunction.Append ("\t\t}\n\n");
		init_funcs.Append (codeRenderFunction);
	}

	private void RemoveLiterals (StringBuilder function)
	{
		string no_literals = Regex.Replace (function.ToString (),
						    @"\t\t\t__parser.AddParsedSubObject \(" + 
						    @"new System.Web.UI.LiteralControl \(.+\);\n", "");
		function.Length = 0;
		function.Append (no_literals);
	}

	private bool FinishControlFunction (string tag_id)
	{
		if (functions.Count == 0)
			throw new ApplicationException ("Unbalanced open/close tags");

		if (controls.Count == 0)
			return false;

		string saved_id = controls.PeekTagID ();
		if (0 != String.Compare (saved_id, tag_id, true))
			return false;

		StringBuilder old_function = (StringBuilder) functions.Pop ();
		current_function = (StringBuilder) functions.Peek ();

		string control_id = controls.PeekControlID ();
		Type control_type = controls.PeekType ();

		bool hasDataBindFunction = controls.HasDataBindFunction ();
		if (hasDataBindFunction)
			old_function.AppendFormat ("\t\t\t__ctrl.DataBinding += new System.EventHandler " +
						   "(this.__DataBind_{0});\n", control_id);

		bool useCodeRender = controls.UseCodeRender;
		if (useCodeRender)
			AddRenderMethodDelegate (old_function, control_id);
		
		if (control_type == typeof (System.Web.UI.ITemplate)){
			old_function.Append ("\n\t\t}\n\n");
			current_function.AppendFormat ("\t\t\t__ctrl.{0} = new System.Web.UI." + 
						       "CompiledTemplateBuilder (new System.Web.UI." +
						       "BuildTemplateMethod (this.__BuildControl_{1}));\n",
						       saved_id, control_id);
		}
		else if (control_type == typeof (System.Web.UI.WebControls.DataGridColumnCollection)){
			old_function.Append ("\n\t\t}\n\n");
			current_function.AppendFormat ("\t\t\tthis.__BuildControl_{0} (__ctrl.{1});\n",
							control_id, saved_id);
		}
		else if (control_type == typeof (System.Web.UI.WebControls.DataGridColumn) ||
			 control_type.IsSubclassOf (typeof (System.Web.UI.WebControls.DataGridColumn)) ||
			 control_type == typeof (System.Web.UI.WebControls.ListItem)){
			old_function.Append ("\n\t\t}\n\n");
			string parsed = "";
			string ctrl_name = "ctrl";
			if (controls.Container == typeof (System.Web.UI.HtmlControls.HtmlSelect)){
				parsed = "ParsedSubObject";
				ctrl_name = "parser";
			}

			current_function.AppendFormat ("\t\t\tthis.__BuildControl_{0} ();\n" +
						       "\t\t\t__{1}.Add{2} (this.{0});\n\n",
						       control_id, ctrl_name, parsed);
		}
		else if (controls.PeekChildKind () == ChildrenKind.LISTITEM){
			old_function.Append ("\n\t\t}\n\n");
			init_funcs.Append (old_function); // Closes the BuildList function
			old_function = (StringBuilder) functions.Pop ();
			current_function = (StringBuilder) functions.Peek ();
			old_function.AppendFormat ("\n\t\t\tthis.__BuildControl_{0} (__ctrl.{1});\n\t\t\t" +
						   "return __ctrl;\n\t\t}}\n\n",
						   control_id, controls.PeekDefaultPropertyName ());

			controls.Pop ();
			control_id = controls.PeekControlID ();
			current_function.AppendFormat ("\t\t\tthis.__BuildControl_{0} ();\n\t\t\t__parser." +
						       "AddParsedSubObject (this.{0});\n\n", control_id);
		}
		else {
			old_function.Append ("\n\t\t\treturn __ctrl;\n\t\t}\n\n");
			current_function.AppendFormat ("\t\t\tthis.__BuildControl_{0} ();\n\t\t\t__parser." +
						       "AddParsedSubObject (this.{0});\n\n", control_id);
		}

		if (useCodeRender)
			RemoveLiterals (old_function);

		init_funcs.Append (old_function);
		if (useCodeRender)
			AddCodeRenderFunction (controls.CodeRenderFunction.ToString (), control_id);
		
		if (hasDataBindFunction){
			StringBuilder db_function = controls.DataBindFunction;
			db_function.Append ("\t\t}\n\n");
			init_funcs.Append (db_function);
		}

		// Avoid getting empty stacks for unbalanced open/close tags
		if (controls.Count > 1){
			controls.Pop ();
			AddCodeRenderControl (controls.CodeRenderFunction, controls.ChildIndex);
		}

		return true;
	}

	private void ProcessHtmlControlTag ()
	{
		HtmlControlTag html_ctrl = (HtmlControlTag) elements.Current;
		if (html_ctrl.TagID.ToUpper () == "SCRIPT"){
			//FIXME: if the is script is to be read from disk, do it!
			if (html_ctrl.SelfClosing)
				throw new ApplicationException ("Read script from file not supported yet.");

			if (elements.MoveNext () == false)
				throw new ApplicationException ("Error after " + html_ctrl.ToString ());

			if (elements.Current is PlainText){
				script.Append (((PlainText) elements.Current).Text);
				if (!elements.MoveNext ())
					throw new ApplicationException ("Error after " +
									elements.Current.ToString ());
			}

			if (elements.Current is CloseTag)
				elements.MoveNext ();
			return;
		}
		
		Type controlType = html_ctrl.ControlType;
		declarations.AppendFormat ("\t\tprotected {0} {1};\n", controlType, html_ctrl.ControlID);

		ChildrenKind children_kind;
		if (0 != String.Compare (html_ctrl.TagID, "select", true))
			children_kind = html_ctrl.IsContainer ? ChildrenKind.CONTROLS :
								ChildrenKind.NONE;
		else
			children_kind = ChildrenKind.OPTION;

		NewControlFunction (html_ctrl.TagID, html_ctrl.ControlID, controlType, children_kind, null); 

		current_function.AppendFormat ("\t\t\t__ctrl.ID = \"{0}\";\n", html_ctrl.ControlID);

		AddCodeForAttributes (html_ctrl.ControlType, html_ctrl.Attributes);

		if (!html_ctrl.SelfClosing)
			JustDoIt ();
		else
			FinishControlFunction (html_ctrl.TagID);
	}

	// Closing is performed in FinishControlFunction ()
	private void NewBuildListFunction (AspComponent component)
	{
		string control_id = Tag.GetDefaultID ();

		controls.Push (component.ComponentType,
			       control_id, 
			       component.TagID, 
			       ChildrenKind.LISTITEM, 
			       component.DefaultPropertyName);

		current_function = new StringBuilder ();
		functions.Push (current_function);
		current_function.AppendFormat ("\t\tprivate void __BuildControl_{0} " +
						"(System.Web.UI.WebControls.ListItemCollection __ctrl)\n" +
						"\t\t{{\n", control_id);
	}

	private void ProcessComponent ()
	{
		AspComponent component = (AspComponent) elements.Current;
		Type component_type = component.ComponentType;
		declarations.AppendFormat ("\t\tprotected {0} {1};\n", component_type, component.ControlID);

		NewControlFunction (component.TagID, component.ControlID, component_type,
				    component.ChildrenKind, component.DefaultPropertyName); 

		if (component_type.IsSubclassOf (typeof (System.Web.UI.UserControl)))
			current_function.Append ("\t\t\t__ctrl.InitializeAsUserControl (Page);\n");

		if (component_type.IsSubclassOf (typeof (System.Web.UI.Control)))
			current_function.AppendFormat ("\t\t\t__ctrl.ID = \"{0}\";\n", component.ControlID);

		AddCodeForAttributes (component.ComponentType, component.Attributes);
		if (component.ChildrenKind == ChildrenKind.LISTITEM)
			NewBuildListFunction (component);

		if (!component.SelfClosing)
			JustDoIt ();
		else
			FinishControlFunction (component.TagID);
	}

	private void ProcessServerObjectTag ()
	{
		ServerObjectTag obj = (ServerObjectTag) elements.Current;
		declarations.AppendFormat ("\t\tprivate {0} cached{1};\n", obj.ObjectClass, obj.ObjectID);
		constructor.AppendFormat ("\n\t\tprivate {0} {1}\n\t\t{{\n\t\t\tget {{\n\t\t\t\t" + 
					  "if (this.cached{1} == null)\n\t\t\t\t\tthis.cached{1} = " + 
					  "new {0} ();\n\t\t\t\treturn cached{1};\n\t\t\t}}\n\t\t}}\n\n",
					  obj.ObjectClass, obj.ObjectID);
	}

	// Creates a new function that sets the values of subproperties.
	private void NewStyleFunction (PropertyTag tag)
	{
		current_function = new StringBuilder ();

		string prop_id = tag.PropertyID;
		Type prop_type = tag.PropertyType;
		// begin function
		current_function.AppendFormat ("\t\tprivate void __BuildControl_{0} ({1} __ctrl)\n" +
						"\t\t{{\n", prop_id, prop_type);
		
		// Add property initialization code
		PropertyInfo [] subprop_info = prop_type.GetProperties ();
		TagAttributes att = tag.Attributes;

		string subprop_name = null;
		foreach (string id in att.Keys){
			if (0 == String.Compare (id, "runat", true) || 0 == String.Compare (id, "id", true))
				continue;

			bool is_processed = false;
			foreach (PropertyInfo subprop in subprop_info){
				is_processed = ProcessProperties (subprop, id, att);
				if (is_processed){
					subprop_name = subprop.Name;
					break;
				}
			}

			if (subprop_name == null)
				throw new ApplicationException ("Property " + tag.TagID + " does not have " + 
								"a " + id + " subproperty.");
		}

		// Finish function
		current_function.Append ("\n\t\t}\n\n");
		init_funcs.Append (current_function);
		current_function = (StringBuilder) functions.Peek ();
		current_function.AppendFormat ("\t\t\tthis.__BuildControl_{0} (__ctrl.{1});\n",
						prop_id, tag.PropertyName);

		if (!tag.SelfClosing){
			// Next tag should be the closing tag
			controls.Push (null, null, null, ChildrenKind.NONE, null);
			bool closing_tag_found = false;
			Element elem;
			while (!closing_tag_found && elements.MoveNext ()){
				elem = (Element) elements.Current;
				if (elem is PlainText)
					ProcessPlainText ();
				else if (!(elem is CloseTag))
					throw new ApplicationException ("Tag " + tag.TagID + 
									" not properly closed.");
				else
					closing_tag_found = true;
			}

			if (!closing_tag_found)
				throw new ApplicationException ("Tag " + tag.TagID + " not properly closed.");

			controls.Pop ();
		}
	}

	// This one just opens the function. Closing is performed in FinishControlFunction ()
	private void NewTemplateFunction (PropertyTag tag)
	{
		/*
		 * FIXME
		 * This function does almost the same as NewControlFunction.
		 * Consider merging.
		 */
		string prop_id = tag.PropertyID;
		Type prop_type = tag.PropertyType;
		string tag_id = tag.PropertyName; // Real property name used in FinishControlFunction

		controls.Push (prop_type, prop_id, tag_id, ChildrenKind.CONTROLS, null);
		current_function = new StringBuilder ();
		functions.Push (current_function);
		current_function.AppendFormat ("\t\tprivate void __BuildControl_{0} " +
						"(System.Web.UI.Control __ctrl)\n" +
						"\t\t{{\n" +
						"\t\t\tSystem.Web.UI.IParserAccessor __parser " + 
						"= (System.Web.UI.IParserAccessor) __ctrl;\n" , prop_id);
	}

	// Closing is performed in FinishControlFunction ()
	private void NewDBColumnFunction (PropertyTag tag)
	{
		/*
		 * FIXME
		 * This function also does almost the same as NewControlFunction.
		 * Consider merging.
		 */
		string prop_id = tag.PropertyID;
		Type prop_type = tag.PropertyType;
		string tag_id = tag.PropertyName; // Real property name used in FinishControlFunction

		controls.Push (prop_type, prop_id, tag_id, ChildrenKind.DBCOLUMNS, null);
		current_function = new StringBuilder ();
		functions.Push (current_function);
		current_function.AppendFormat ("\t\tprivate void __BuildControl_{0} " +
						"(System.Web.UI.WebControl.DataGridColumnCollection __ctrl)\n" +
						"\t\t{{\n", prop_id);
	}

	private void NewPropertyFunction (PropertyTag tag)
	{
		if (tag.PropertyType == typeof (System.Web.UI.WebControls.Style) ||
		    tag.PropertyType.IsSubclassOf (typeof (System.Web.UI.WebControls.Style)))
			NewStyleFunction (tag);
		else if (tag.PropertyType == typeof (System.Web.UI.ITemplate))
			NewTemplateFunction (tag);
		else if (tag.PropertyType == typeof (System.Web.UI.WebControls.DataGridColumnCollection))
			NewDBColumnFunction (tag);
		else
			throw new ApplicationException ("Other than Style and ITemplate not supported yet. " + 
							tag.PropertyType);
	}
	
	private void ProcessHtmlTag ()
	{
		Tag tag = (Tag) elements.Current;
		ChildrenKind child_kind = controls.PeekChildKind ();
		if (child_kind == ChildrenKind.NONE){
			string tag_id = controls.PeekTagID ();
			throw new ApplicationException (tag + " not allowed inside " + tag_id);
		}
					
		if (child_kind == ChildrenKind.OPTION){
			if (0 != String.Compare (tag.TagID, "option", true))
				throw new ApplicationException ("Only <option> tags allowed inside <select>.");

			string default_id = Tag.GetDefaultID ();
			Type type = typeof (System.Web.UI.WebControls.ListItem);
			declarations.AppendFormat ("\t\tprotected {0} {1};\n", type, default_id);
			NewControlFunction (tag.TagID, default_id, type, ChildrenKind.CONTROLS, null); 
			return;
		}

		if (child_kind == ChildrenKind.CONTROLS){
			elements.Current = new PlainText (((Tag) elements.Current).PlainHtml);
			ProcessPlainText ();
			return;
		}

		// Now child_kind should be PROPERTIES, so only allow tag_id == property
		Type control_type = controls.PeekType ();
		PropertyInfo [] prop_info = control_type.GetProperties ();
		bool is_processed = false;
		foreach (PropertyInfo prop in prop_info){
			if (0 == String.Compare (prop.Name, tag.TagID, true)){
				PropertyTag prop_tag = new PropertyTag (tag, prop.PropertyType, prop.Name);
				NewPropertyFunction (prop_tag);
				is_processed = true;
				break;
			}
		}
		
		if (!is_processed){
			string tag_id = controls.PeekTagID ();
			throw new ApplicationException (tag.TagID + " is not a property of " + control_type);
		}
	}

	private Tag Map (Tag tag)
	{
		int pos = tag.TagID.IndexOf (":");
		if (tag is CloseTag || 
		    ((tag.Attributes == null || 
		    !tag.Attributes.IsRunAtServer ()) && pos == -1))
			return tag;

		if (pos == -1){
			if (0 == String.Compare (tag.TagID, "object", true))
				return new ServerObjectTag (tag);
			return new HtmlControlTag (tag);
		}

		string foundry_name = tag.TagID.Substring (0, pos);
		string component_name = tag.TagID.Substring (pos + 1);

		if (Foundry.LookupFoundry (foundry_name) == false)
			throw new ApplicationException ("Cannot find foundry for alias'" + foundry_name + "'");

		AspComponent component = Foundry.MakeAspComponent (foundry_name, component_name, tag);
		if (component == null)
			throw new ApplicationException ("Cannot find component '" + component_name + 
							"' for alias '" + foundry_name + "'");

		return component;
	}
	
	private void ProcessCloseTag ()
	{
		CloseTag close_tag = (CloseTag) elements.Current;
		if (FinishControlFunction (close_tag.TagID))
				return;

		elements.Current = new PlainText (close_tag.PlainHtml);
		ProcessPlainText ();
	}

	private void ProcessDataBindingLiteral ()
	{
		DataBindingTag dataBinding = (DataBindingTag) elements.Current;
		string actual_value = dataBinding.Data;
		if (actual_value == "")
			throw new ApplicationException ("Empty data binding tag.");

		if (controls.PeekChildKind () != ChildrenKind.CONTROLS)
			throw new ApplicationException ("Data bound content not allowed for " + 
							controls.PeekTagID ());

		StringBuilder db_function = new StringBuilder ();
		string control_id = Tag.GetDefaultID ();
		string control_type_string = "System.Web.UI.DataBoundLiteralControl";
		declarations.AppendFormat ("\t\tprotected {0} {1};\n", control_type_string, control_id);
		// Build the control
		db_function.AppendFormat ("\t\tprivate System.Web.UI.Control __BuildControl_{0} ()\n" +
					  "\t\t{{\n\t\t\t{1} __ctrl;\n\n" +
					  "\t\t\t__ctrl = new {1} (0, 1);\n" + 
					  "\t\t\tthis.{0} = __ctrl;\n" +
					  "\t\t\t__ctrl.DataBinding += new System.EventHandler " + 
					  "(this.__DataBind_{0});\n" +
					  "\t\t\treturn __ctrl;\n"+
					  "\t\t}}\n\n",
					  control_id, control_type_string);
		// DataBinding handler
		db_function.AppendFormat ("\t\tpublic void __DataBind_{0} (object sender, " + 
					  "System.EventArgs e) {{\n" +
					  "\t\t\t{1} Container;\n" +
					  "\t\t\t{2} target;\n" +
					  "\t\t\ttarget = ({2}) sender;\n" +
					  "\t\t\tContainer = ({1}) target.BindingContainer;\n" +
					  "\t\t\ttarget.SetDataBoundString (0, System.Convert." +
					  "ToString ({3}));\n" +
					  "\t\t}}\n\n",
					  control_id, controls.Container, control_type_string,
					  actual_value);

		init_funcs.Append (db_function);
		current_function.AppendFormat ("\t\t\tthis.__BuildControl_{0} ();\n\t\t\t__parser." +
					       "AddParsedSubObject (this.{0});\n\n", control_id);
	}

	private void ProcessCodeRenderTag ()
	{
		CodeRenderTag code_tag = (CodeRenderTag) elements.Current;

		controls.UseCodeRender = true;
		if (code_tag.IsVarName)
			controls.CodeRenderFunction.AppendFormat ("\t\t\t__output.Write ({0});\n",
								  code_tag.Code);
		else
			controls.CodeRenderFunction.AppendFormat ("\t\t\t{0}\n", code_tag.Code);
	}
	
	public void ProcessElements ()
	{
		JustDoIt ();
		End ();
		parse_ok = true;
	}
	
	private void JustDoIt ()
	{
		Element element;

		while (elements.MoveNext ()){
			element = (Element) elements.Current;
			if (element is Directive){
				ProcessDirective ();
			} else if (element is PlainText){
				ProcessPlainText ();
			} else if (element is DataBindingTag){
				ProcessDataBindingLiteral ();
			} else if (element is CodeRenderTag){
				ProcessCodeRenderTag ();
			} else {
				elements.Current = Map ((Tag) element);
				if (elements.Current is HtmlControlTag)
					ProcessHtmlControlTag ();
				else if (elements.Current is AspComponent)
					ProcessComponent ();
				else if (elements.Current is CloseTag)
					ProcessCloseTag ();
				else if (elements.Current is ServerObjectTag)
					ProcessServerObjectTag ();
				else if (elements.Current is Tag)
					ProcessHtmlTag ();
				else
					throw new ApplicationException ("This place should not be reached.");
			}
		}
	}

	private void End ()
	{
		buildOptions.AppendFormat ("//<class name=\"{0}\"/>\n", className);
		buildOptions.Append ("\n");
		classDecl = "\tpublic class " + className + " : " + parent + interfaces + " {\n"; 
		prolog.Append ("\n" + classDecl);
		declarations.Append ("\t\tprivate static bool __intialized = false;\n\n");
		if (!IsUserControl)
			declarations.Append ("\t\tprivate static ArrayList __fileDependencies;\n\n");

		// adds the constructor
		constructor.AppendFormat ("\t\tpublic {0} ()\n\t\t{{\n" + 
					"\t\t\tSystem.Collections.ArrayList dependencies;\n\n" +
					"\t\t\tif (ASP.{0}.__intialized == false){{\n", className); 
		if (!IsUserControl) {
			constructor.AppendFormat ("\t\t\t\tdependencies = new System.Collections.ArrayList ();\n" +
						"\t\t\t\tdependencies.Add (@\"{1}\");\n" +
						"\t\t\t\tASP.{0}.__fileDependencies = dependencies;\n",
						className, fullPath);
		}

		constructor.AppendFormat ("\t\t\t\tASP.{0}.__intialized = true;\n\t\t\t}}\n\t\t}}\n\n",
					  className);
         
		//FIXME: add AutoHandlers: don't know what for...yet!
		constructor.AppendFormat (
			"\t\tprotected override int AutoHandlers\n\t\t{{\n" +
			"\t\t\tget {{ return ASP.{0}.__autoHandlers; }}\n" +
			"\t\t\tset {{ ASP.{0}.__autoHandlers = value; }}\n" +
			"\t\t}}\n\n", className);

		//FIXME: add ApplicationInstance: don't know what for...yet!
		constructor.Append (
			"\t\tprotected System.Web.HttpApplication ApplicationInstance\n\t\t{\n" +
			"\t\t\tget { return (System.Web.HttpApplication) this.Context.ApplicationInstance; }\n" +
			"\t\t}\n\n");
		//FIXME: add TemplateSourceDirectory: don't know what for...yet!
		//FIXME: it should be the path from the root where the file resides
		constructor.Append (
			"\t\tpublic override string TemplateSourceDirectory\n\t\t{\n" +
			"\t\t\tget { return \"/dummypath\"; }\n" +
			"\t\t}\n\n");

		epilog.Append ("\n\t\tprotected override void FrameworkInitialize ()\n\t\t{\n" +
				"\t\t\tthis.__BuildControlTree (this);\n");

		if (!IsUserControl) {
			epilog.AppendFormat ("\t\t\tResponse.AddFileDependencies (ASP.{0}.__fileDependencies);\n" +
						"\t\t\tthis.EnableViewStateMac = true;\n", className);
		}
		epilog.Append ("\t\t}\n\n");

		if (!IsUserControl) {
			Random rnd = new Random ();
			epilog.AppendFormat ("\t\tpublic override int GetTypeHashCode ()\n\t\t{{\n" +
					     "\t\t\treturn {0};\n" +
					     "\t\t}}\n", rnd.Next ());
		}

		epilog.Append ("\t}\n}\n");

		// Closes the currently opened tags
		StringBuilder old_function = current_function;
		string control_id;
		while (functions.Count > 1){
			old_function.Append ("\n\t\t\treturn __ctrl;\n\t\t}\n\n");
			init_funcs.Append (old_function);
			control_id = controls.PeekControlID ();
			FinishControlFunction (control_id);
			controls.AddChild ();
			old_function = (StringBuilder) functions.Pop ();
			current_function = (StringBuilder) functions.Peek ();
			controls.Pop ();
		}

		bool useCodeRender = controls.UseCodeRender;
		if (useCodeRender){
			RemoveLiterals (current_function);
			AddRenderMethodDelegate (current_function, controls.PeekControlID ());
		}
		
		current_function.Append ("\t\t}\n\n");
		init_funcs.Append (current_function);
		if (useCodeRender)
			AddCodeRenderFunction (controls.CodeRenderFunction.ToString (), controls.PeekControlID ());

		functions.Pop ();
	}

	//
	// Functions related to compilation of user controls
	//
	
	private static char dirSeparator = Path.DirectorySeparatorChar;
	struct UserControlData
	{
		public UserControlResult result;
		public string className;
		public string assemblyName;
	}

	private static UserControlData GenerateUserControl (string src)
	{
		UserControlData data = new UserControlData ();
		data.result = UserControlResult.OK;

		if (!File.Exists (src)) {
			data.result = UserControlResult.FileNotFound;
			return data;
		}

		string noExt = Path.GetFileNameWithoutExtension (src);
		string csName = "output" + dirSeparator + "xsp_ctrl_" + noExt + ".cs";
		if (!Directory.Exists ("output"))
			Directory.CreateDirectory ("output");

		if (Xsp (src, csName) == false) {
			data.result = UserControlResult.XspFailed;
			return data;
		}
		
		StreamReader fileReader = new StreamReader (File.Open (csName, FileMode.Open));
		data.className = src.Replace ('.', '_');
		
		StringBuilder compilerOptions = new StringBuilder ("/r:System.Web.dll /r:System.Drawing.dll ");
		compilerOptions.Append ("/target:library ");

		string line;
		while ((line = fileReader.ReadLine ()) != null && line != "") {
			if (line.StartsWith ("//<class ")) {
				data.className = GetAttributeValue (line, "name");
			} else if (line.StartsWith ("//<reference ")) {
				string dllName = GetAttributeValue (line, "dll");
				compilerOptions.AppendFormat ("/r:{0} ", dllName);
			} else if (line.StartsWith ("//<compileroptions ")) {
				string options = GetAttributeValue (line, "options");
				compilerOptions.Append (" " + options + " ");
			} else {
				Console.Error.WriteLine ("Ignoring build option: {0}", line);
			}
		}
		fileReader.Close ();

		string dll = Path.ChangeExtension (csName, ".dll");
		data.assemblyName = dll;
		if (Compile (csName, dll, compilerOptions) == false) {
			data.result = UserControlResult.CompilationFailed;
		}
		
		return data;
	}

	private static string GetAttributeValue (string line, string att)
	{
		string att_start = att + "=\"";
		int begin = line.IndexOf (att_start);
		int end = line.Substring (begin + att_start.Length).IndexOf ('"');
		if (begin == -1 || end == -1)
			throw new ApplicationException ("Error in compilation option:\n" + line);

		return line.Substring (begin + att_start.Length, end);
	}
		
	private static bool Xsp (string fileName, string csFileName)
	{
#if MONO
		return RunProcess ("mono", 
				   "xsp.exe --control " + fileName, 
				   csFileName,
				   "output" + dirSeparator + "xsp_ctrl_" + Path.GetFileName (fileName) + 
				   ".sh");
#else
		return RunProcess ("xsp", 
				   "--control " + fileName, 
				   csFileName,
				   "output" + dirSeparator + "xsp_ctrl_" + fileName + ".bat");
#endif
	}

	private static bool Compile (string csName, string dllName, StringBuilder compilerOptions)
	{
		compilerOptions.AppendFormat ("/out:{0} ", dllName);
		compilerOptions.Append (csName + " ");

		string cmdline = compilerOptions.ToString ();
		string noext = Path.GetFileNameWithoutExtension (csName);
		string output_file = "output" + dirSeparator + "output_from_compilation_" + noext + ".txt";
		string bat_file = "output" + dirSeparator + "last_compilation_" + noext + ".bat";
		return RunProcess ("mcs", cmdline, output_file, bat_file);
	}

	private static bool RunProcess (string exe, string arguments, string output_file, string script_file)
	{
		Process proc = new Process ();

		proc.StartInfo.FileName = exe;
		proc.StartInfo.Arguments = arguments;
		proc.StartInfo.UseShellExecute = false;
		proc.StartInfo.RedirectStandardOutput = true;
		proc.Start ();
		string poutput = proc.StandardOutput.ReadToEnd();
		proc.WaitForExit ();
		int result = proc.ExitCode;
		proc.Close ();

		StreamWriter cmd_output = new StreamWriter (File.Create (output_file));
		cmd_output.Write (poutput);
		cmd_output.Close ();
		StreamWriter bat_output = new StreamWriter (File.Create (script_file));
		bat_output.Write (exe + " " + arguments);
		bat_output.Close ();

		return (result == 0);
	}

}

}

