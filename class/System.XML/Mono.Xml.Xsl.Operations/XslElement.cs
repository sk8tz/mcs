//
// XslElement.cs
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//	
// (C) 2003 Ben Maurer
// (C) 2003 Atsushi Enomoto
//

using System;
using System.Collections;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

using QName = System.Xml.XmlQualifiedName;

namespace Mono.Xml.Xsl.Operations {	
	public class XslElement : XslCompiledElement {
		XslAvt name, ns;
		string calcName, calcNs, calcPrefix;
		XmlNamespaceManager nsm;
		bool isEmptyElement;

		XslOperation value;
		XmlQualifiedName [] useAttributeSets;
		
		public XslElement (Compiler c) : base (c) {}
		protected override void Compile (Compiler c)
		{
			name = c.ParseAvtAttribute ("name");
			ns = c.ParseAvtAttribute ("namespace");
			
			calcName = XslAvt.AttemptPreCalc (ref name);
			
			if (calcName != null && ns == null) {
				int colonAt = calcName.IndexOf (':');
				calcPrefix = colonAt < 0 ? String.Empty : calcName.Substring (0, colonAt);
				calcName = colonAt < 0 ? calcName : calcName.Substring (colonAt + 1, calcName.Length - colonAt - 1);
				calcNs = c.Input.GetNamespace (calcPrefix);
			} else if (ns != null)
				calcNs = XslAvt.AttemptPreCalc (ref ns);
			
			if (ns == null && calcNs == null)
				nsm = c.GetNsm ();
			
			useAttributeSets = c.ParseQNameListAttribute ("use-attribute-sets");
			
			isEmptyElement = c.Input.IsEmptyElement;

			if (c.Input.MoveToFirstChild ()) {
				value = c.CompileTemplateContent ();
				c.Input.MoveToParent ();
			}
		}
		
		public override void Evaluate (XslTransformProcessor p)
		{
			string nm, nmsp, localName, prefix;
			
			nm = calcName != null ? calcName : name.Evaluate (p);
			nmsp = calcNs != null ? calcNs : ns != null ? ns.Evaluate (p) : null;
			prefix = calcPrefix != null ? calcPrefix : String.Empty;

			if (nmsp == null) {
				QName q = XslNameUtil.FromString (nm, nsm);
				localName = q.Name;
				nmsp = q.Namespace;
			}
			if (calcPrefix == String.Empty) {
				XPathNavigator nav = this.InputNode.Clone ();
				if (nav.MoveToFirstNamespace (XPathNamespaceScope.ExcludeXml)) {
					do {
						if (nav.Value == nmsp) {
							prefix = nav.Name;
							break;
						}
					} while (nav.MoveToNextNamespace (XPathNamespaceScope.ExcludeXml));
				}
			}

			p.Out.WriteStartElement (prefix, nm, nmsp);
			
			p.TryStylesheetNamespaceOutput ();
			if (useAttributeSets != null)
				foreach (XmlQualifiedName s in useAttributeSets)
					p.ResolveAttributeSet (s).Evaluate (p);

			if (value != null) value.Evaluate (p);

			p.Out.WriteFullEndElement ();
		}
	}
}
