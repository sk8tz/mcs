//
// System.Diagnostics.DiagnosticsConfigurationHandler.cs
//
// Comments from John R. Hicks <angryjohn69@nc.rr.com> original implementation 
// can be found at: /mcs/docs/apidocs/xml/en/System.Diagnostics
//
// Authors: 
//	John R. Hicks <angryjohn69@nc.rr.com>
//	Jonathan Pryor <jonpryor@vt.edu>
//
// (C) 2002
//
using System;
using System.Collections;
using System.Configuration;#if (XML_DEP)
using System.Xml;
#endif
namespace System.Diagnostics
{
	internal sealed class DiagnosticsConfiguration
	{
		private static IDictionary settings = null;

		public static IDictionary Settings {
			get {
				// TODO: Does anybody know if this is actually thread-safe under .NET?
				// I've heard that this construct isn't safe under Java, but it's used
				// reasonably often under C++, so I'm not sure about .NET.
				if (settings == null) {
					lock (typeof(DiagnosticsConfiguration)) {
						if (settings == null)
							settings = (IDictionary) ConfigurationSettings.GetConfig ("system.diagnostics");
					}
				}
				return settings;
			}
		}
	}
#if (XML_DEP)
	public class DiagnosticsConfigurationHandler : IConfigurationSectionHandler
	{
		delegate void ElementHandler (IDictionary d, XmlNode node);

		IDictionary elementHandlers = new Hashtable ();

		public DiagnosticsConfigurationHandler ()
		{
			elementHandlers ["assert"] = new ElementHandler (AddAssertNode);
			elementHandlers ["switches"] = new ElementHandler (AddSwitchesNode);
			elementHandlers ["trace"] = new ElementHandler (AddTraceNode);
		}

		public virtual object Create (object parent, object configContext, XmlNode section)
		{
			IDictionary d;
			if (parent == null)
				d = new Hashtable (CaseInsensitiveHashCodeProvider.Default, CaseInsensitiveComparer.Default);
			else
				d = (IDictionary) ((ICloneable)parent).Clone();

			foreach (XmlNode child in section.ChildNodes) {
				XmlNodeType type = child.NodeType;

				switch (type) {
				/* ignore */
				case XmlNodeType.Whitespace:
				case XmlNodeType.Comment:
					continue;
				case XmlNodeType.Element:
					ElementHandler eh = (ElementHandler) elementHandlers [child.Name];
					if (eh != null)
						eh (d, child);
					else
						ThrowUnrecognizedElement (child);
					break;
				default:
					ThrowUnrecognizedElement (child);
					break;
				}
			}

			return d;
		}

		// Remarks: Both attribute are optional
		private void AddAssertNode (IDictionary d, XmlNode node)
		{
			XmlAttributeCollection c = node.Attributes;
			string assertuienabled = GetAttribute (c, "assertuienabled", false, node);
			string logfilename = GetAttribute (c, "logfilename", false, node);
			ValidateInvalidAttributes (c, node);
			if (assertuienabled != null) {
				try {
					d ["assertuienabled"] = bool.Parse (assertuienabled);
				}
				catch (Exception e) {
					throw new ConfigurationException ("The `assertuienabled' attribute must be `true' or `false'",
							e, node);
				}
			}

			if (logfilename != null)
				d ["logfilename"] = logfilename;

			DefaultTraceListener dtl = (DefaultTraceListener) TraceImpl.Listeners["Default"];
			if (dtl != null) {
				if (assertuienabled != null)
					dtl.AssertUiEnabled = (bool) d ["assertuienabled"];
				if (logfilename != null)
					dtl.LogFileName = logfilename;
			}

			if (node.ChildNodes.Count > 0)
				ThrowUnrecognizedElement (node.ChildNodes[0]);
		}

		// name attribute is required, value is optional
		// Docs do not define "remove" or "clear" elements, but .NET recognizes
		// them
		private void AddSwitchesNode (IDictionary d, XmlNode node)
		{
			// There are no attributes on <switch/>
			ValidateInvalidAttributes (node.Attributes, node);

			IDictionary newNodes = new Hashtable ();

			foreach (XmlNode child in node.ChildNodes) {
				XmlNodeType t = child.NodeType;
				if (t == XmlNodeType.Whitespace || t == XmlNodeType.Comment)
					continue;
				if (t == XmlNodeType.Element) {
					XmlAttributeCollection attributes = child.Attributes;
					string name = null;
					string value = null;
					switch (child.Name) {
						case "add":
							name = GetAttribute (attributes, "name", true, child);
							value = GetAttribute (attributes, "value", false, child);
							newNodes[name] = AsString (value);
							break;
						case "remove":
							name = GetAttribute (attributes, "name", true, child);
							newNodes.Remove (name);
							break;
						case "clear":
							newNodes.Clear ();
							break;
						default:
							ThrowUnrecognizedElement (child);
							break;
					}
					ValidateInvalidAttributes (attributes, child);
				}
				else
					ThrowUnrecognizedNode (child);
			}

			d [node.Name] = newNodes;
		}

		private void AddTraceNode (IDictionary d, XmlNode node)
		{
			AddTraceAttributes (d, node);

			foreach (XmlNode child in node.ChildNodes) {
				XmlNodeType t = child.NodeType;
				if (t == XmlNodeType.Whitespace || t == XmlNodeType.Comment)
					continue;
				if (t == XmlNodeType.Element) {
					if (child.Name == "listeners")
						AddTraceListeners (child);
					else
						ThrowUnrecognizedElement (child);
					ValidateInvalidAttributes (child.Attributes, child);
				}
				else
					ThrowUnrecognizedNode (child);
			}
		}

		// all attributes are optional
		private void AddTraceAttributes (IDictionary d, XmlNode node)
		{
			XmlAttributeCollection c = node.Attributes;
			string autoflush = GetAttribute (c, "autoflush", false, node);
			string indentsize = GetAttribute (c, "indentsize", false, node);
			ValidateInvalidAttributes (c, node);
			if (autoflush != null) {
				try {
					bool b = bool.Parse (autoflush);
					d ["autoflush"] = b;
					TraceImpl.AutoFlush = b;
				}
				catch (Exception e) {
					throw new ConfigurationException ("The `autoflush' attribute must be `true' or `false'",
							e, node);
				}
			}
			if (indentsize != null) {
				try {
					int n = int.Parse (indentsize);
					d ["indentsize"] = n;
					TraceImpl.IndentSize = n;
				}
				catch (Exception e) {
					throw new ConfigurationException ("The `indentsize' attribute must be an integral value.",
							e, node);
				}
			}
		}

		// only defines "add" and "remove", but "clear" also works
		// for add, "name" and "type" are required; initializeData is optional
		private void AddTraceListeners (XmlNode listenersNode)
		{
			// There are no attributes on <listeners/>
			ValidateInvalidAttributes (listenersNode.Attributes, listenersNode);

			foreach (XmlNode child in listenersNode.ChildNodes) {
				XmlNodeType t = child.NodeType;
				if (t == XmlNodeType.Whitespace || t == XmlNodeType.Comment)
					continue;
				if (t == XmlNodeType.Element) {
					XmlAttributeCollection attributes = child.Attributes;
					string name = null;
					string type = null;
					string id = null;
					switch (child.Name) {
						case "add":
							name = GetAttribute (attributes, "name", true, child);
							type = GetAttribute (attributes, "type", true, child);
							id = GetAttribute (attributes, "initializeData", false, child);
							AddTraceListener (name, type, id);
							break;
						case "remove":
							name = GetAttribute (attributes, "name", true, child);
							RemoveTraceListener (name);
							break;
						case "clear":
							TraceImpl.Listeners.Clear ();
							break;
						default:
							ThrowUnrecognizedElement (child);
							break;
					}
					ValidateInvalidAttributes (attributes, child);
				}
				else
					ThrowUnrecognizedNode (child);
			}
		}

		private void AddTraceListener (string name, string type, string initializeData)
		{
			Type t = Type.GetType (type);
			if (t == null)
				throw new ConfigurationException (string.Format ("Invalid Type Specified: {0}", type));

			object[] args;
			Type[] types;
			
			if (initializeData != null) {
				args = new object[] { initializeData };
				types = new Type[] { typeof(string) };
			}
			else {
				args = null;
				types = new Type[0];
			}
				
			System.Reflection.ConstructorInfo ctor = t.GetConstructor (types);
			if (ctor == null) 
				throw new ConfigurationException ("Couldn't find constructor for class " + type);
			
			TraceListener l = (TraceListener) ctor.Invoke (args);
			l.Name = name;
			TraceImpl.Listeners.Add (l);
		}

		private void RemoveTraceListener (string name)
		{
			try {
				TraceImpl.Listeners.Remove (name);
			}
			catch (ArgumentException) {
				// The specified listener wasn't in the collection
				// Ignore this; .NET does.
			}
			catch (Exception e) {
				throw new ConfigurationException (
						string.Format ("Unknown error removing listener: {0}", name),
						e);
			}
		}

		private string GetAttribute (XmlAttributeCollection attrs, string attr, bool required, XmlNode node)
		{
			XmlAttribute a = attrs[attr];

			string r = null;

			if (a != null) {
				r = a.Value;
				if (required)
					ValidateAttribute (attr, r, node);
				attrs.Remove (a);
			}
			else if (required)
				ThrowMissingAttribute (attr, node);

			return r;
		}
		
		private string AsString (string s)
		{
			return s == null ? string.Empty : s;
		}

		private void ValidateAttribute (string attribute, string value, XmlNode node)
		{
			if (value == null || value.Length == 0)
				throw new ConfigurationException (string.Format ("Required attribute `{0}' cannot be empty.", attribute), node);
		}

		private void ValidateInvalidAttributes (XmlAttributeCollection c, XmlNode node)
		{
			if (c.Count != 0)
				ThrowUnrecognizedAttribute (c[0].Name, node);
		}

		private void ThrowMissingAttribute (string attribute, XmlNode node)
		{
			throw new ConfigurationException (string.Format ("Missing required attribute `{0}'.", attribute), node);
		}

		private void ThrowUnrecognizedNode (XmlNode node)
		{
			throw new ConfigurationException (
					string.Format ("Unrecognized node `{0}'; nodeType={1}", node.Name, node.NodeType),
					node);
		}

		private void ThrowUnrecognizedElement (XmlNode node)
		{
			throw new ConfigurationException (
					string.Format ("Unrecognized element <{0}/>", node.Name),
					node);
		}

		private void ThrowUnrecognizedAttribute (string attribute, XmlNode node)
		{
			throw new ConfigurationException (
					string.Format ("Unrecognized attribute `{0}' on element <{1}/>.", attribute, node.Name),
					node);
		}
	}
#endif
}

