//
// XslLiteralElement.cs
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

namespace Mono.Xml.Xsl.Operations {	
	public class XslLiteralElement : XslCompiledElement {
		XslOperation children;
		string localname, prefix, nsUri;
		bool isEmptyElement;
		ArrayList attrs;
		XmlQualifiedName [] useAttributeSets;
		Hashtable nsDecls;
		string excludeResultPrefixes;
		string extensionElementPrefixes;
		ArrayList excludedPrefixes;
		bool requireNameFix;
		XPathNavigator stylesheetNode;
		XslStylesheet stylesheet;

		public XslLiteralElement (Compiler c) : base (c) {}
			
		class XslLiteralAttribute {
			string localname, prefix, nsUri;
			XslAvt val;
			
			public XslLiteralAttribute (Compiler c)
			{
				this.prefix = c.Input.Prefix;
				this.nsUri = c.Input.NamespaceURI;
				this.localname = c.Input.LocalName;
				this.val = new XslAvt (c.Input.Value, c);
			}
			
			public void Evaluate (XslTransformProcessor p)
			{
				p.Out.WriteAttributeString (prefix, localname, nsUri, val.Evaluate (p));
			}
		}
		
		protected override void Compile (Compiler c)
		{
			requireNameFix = true;
			stylesheetNode = c.Input.Clone ();
			stylesheet = c.CurrentStylesheet;

			this.localname = c.Input.LocalName;
			this.useAttributeSets = c.ParseQNameListAttribute ("use-attribute-sets", XsltNamespace);
			this.nsDecls = c.GetNamespacesToCopy ();
			if (nsDecls.Count == 0) nsDecls = null;
			this.isEmptyElement = c.Input.IsEmptyElement;
			this.excludeResultPrefixes = c.Input.GetAttribute ("exclude-result-prefixes", XsltNamespace);
			this.extensionElementPrefixes = c.Input.GetAttribute ("extension-element-prefixes", XsltNamespace);
			excludedPrefixes = new ArrayList (excludeResultPrefixes.Split (XmlChar.WhitespaceChars));
			excludedPrefixes.AddRange (extensionElementPrefixes.Split (XmlChar.WhitespaceChars));

			if (c.Input.MoveToFirstAttribute ())
			{
				attrs = new ArrayList ();
				do {
					if (c.Input.NamespaceURI == XsltNamespace)
						continue; //already handled
					attrs.Add (new XslLiteralAttribute (c));
				} while (c.Input.MoveToNextAttribute());
				c.Input.MoveToParent ();
			}
			
			if (!c.Input.MoveToFirstChild ()) return;
			children = c.CompileTemplateContent ();
			c.Input.MoveToParent ();
		}

		private void GetCorrectNames ()
		{
			requireNameFix = false;
			prefix = stylesheetNode.Prefix;
			string alias = stylesheet.PrefixInEffect (prefix, null);
			if (alias != null && alias != stylesheetNode.Prefix) {
				nsUri = stylesheetNode.GetNamespace (alias);
				if (alias != null)
					prefix = alias;
			}
			else
				nsUri = stylesheetNode.NamespaceURI;

		}
		
		public override void Evaluate (XslTransformProcessor p)
		{
			// Since namespace-alias might be determined after compilation
			// of import-ing stylesheets, this must be determined later.
			if (requireNameFix)
				GetCorrectNames ();

			bool isCData = p.InsideCDataElement;
			p.PushElementState (localname, nsUri, true);
			p.Out.WriteStartElement (prefix, localname, nsUri);

			if (useAttributeSets != null)
				foreach (XmlQualifiedName s in useAttributeSets)
					p.ResolveAttributeSet (s).Evaluate (p);
						
			if (attrs != null) {
				int len = attrs.Count;
				for (int i = 0; i < len; i++)
					((XslLiteralAttribute)attrs [i]).Evaluate (p);
			}

			p.TryStylesheetNamespaceOutput (excludedPrefixes);
			if (nsDecls != null) {
				foreach (DictionaryEntry de in nsDecls) {
					string actualPrefix = p.CompiledStyle.Style.PrefixInEffect (de.Key as String, excludedPrefixes);
					if (actualPrefix != null)
						p.Out.WriteNamespaceDecl (actualPrefix, (string)de.Value);
				}
			}
			
			if (children != null) children.Evaluate (p);

			if (isEmptyElement)
				p.Out.WriteEndElement ();
			else
				p.Out.WriteFullEndElement ();

			p.PopCDataState (isCData);
		}
	}
}
