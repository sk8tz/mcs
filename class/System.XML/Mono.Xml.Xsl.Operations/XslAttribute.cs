//
// XslAttribute.cs
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
using System.IO;

using QName = System.Xml.XmlQualifiedName;

namespace Mono.Xml.Xsl.Operations {
	public class XslAttribute : XslCompiledElement {
		XslAvt name, ns;
		string calcName, calcNs;
		XmlNamespaceManager nsm;
		
		XslOperation value;
		public XslAttribute (Compiler c) : base (c) {}
		
		protected override void Compile (Compiler c)
		{
			name = c.ParseAvtAttribute ("name");
			ns = c.ParseAvtAttribute ("namespace");
			
			
			calcName = XslAvt.AttemptPreCalc (ref name);
			
			if (calcName != null && ns == null) {
				QName q = XslNameUtil.FromString (calcName, c.Input);
				calcName = q.Name;
				calcNs = q.Namespace;	
			} else if (ns != null)
				calcNs = XslAvt.AttemptPreCalc (ref ns);
			
			if (ns == null && calcNs == null)
				nsm = c.GetNsm ();
				
			if (c.Input.MoveToFirstChild ()) {
				value = c.CompileTemplateContent ();
				c.Input.MoveToParent ();
			}
		}

		public override void Evaluate (XslTransformProcessor p)
		{
			string nm, nmsp;
			
			nm = calcName != null ? calcName : name.Evaluate (p);
			nmsp = calcNs != null ? calcNs : ns != null ? ns.Evaluate (p) : null;
			
			if (nmsp == null) {
				QName q = XslNameUtil.FromString (nm, nsm);
				nm = q.Name;
				nmsp = q.Namespace;	
			} else
				nm = XslNameUtil.LocalNameOf (nm);

			if (value == null)
				p.Out.WriteAttributeString("", nm, nmsp, "");
			else {
				StringWriter sw = new StringWriter ();
				Outputter outputter = new TextOutputter (sw, true);
				p.PushOutput (outputter);
				value.Evaluate (p);			    
				p.PopOutput ();
				outputter.Done ();			        
				p.Out.WriteAttributeString ("", nm, nmsp, sw.ToString ());			                    			        
			}						
		}
	}
}
