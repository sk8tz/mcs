//
// XslTemplateContent.cs
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
	internal class XslTemplateContent : XslCompiledElement {
		ArrayList content = new ArrayList ();
		
		bool hasStack;
		int stackSize;
		
		public XslTemplateContent (Compiler c, XPathNodeType parentType)
			: base (c, parentType) 
		{
		}

		private void ThrowIfNotElement (Compiler c)
		{
			switch (ParentType) {
			case XPathNodeType.All:
			case XPathNodeType.Element:
					break;
			default:
					throw new XsltCompileException ("Cannot contain attribute from this parent node " + ParentType, null, c.Input);
			}
		}

		protected override void Compile (Compiler c)
		{
			hasStack = (c.CurrentVariableScope == null);
			c.PushScope ();
			do {	
				Debug.EnterNavigator (c);
				XPathNavigator n = c.Input;			
				switch (n.NodeType) {
				case XPathNodeType.Element:
					switch (n.NamespaceURI) {
					case XsltNamespace:
						
						switch (n.LocalName) {
						case "apply-imports":
							content.Add (new XslApplyImports (c));
							break;
						case "apply-templates":
							content.Add (new XslApplyTemplates (c));
							break;
						case "attribute":
							ThrowIfNotElement (c);
							content.Add (new XslAttribute (c));
							break;
						case "call-template":
							content.Add (new XslCallTemplate (c));
							break;
						case "choose":
							content.Add (new XslChoose (c));
							break;
						case "comment":
							ThrowIfNotElement (c);
							content.Add (new XslComment (c));
							break;
						case "copy":
							content.Add (new XslCopy (c));
							break;
						case "copy-of":
							content.Add (new XslCopyOf (c));
							break;
						case "element":
							ThrowIfNotElement (c);
							content.Add (new XslElement (c));
							break;
						case "fallback":
							content.Add (new XslFallback (c));
							break;
						case "for-each":
							content.Add (new XslForEach (c));
							break;
						case "if":
							content.Add (new XslIf (c));
							break;
						case "message":
							content.Add (new XslMessage(c));
							break;
						case "number":
							content.Add (new XslNumber(c));
							break;
						case "processing-instruction":
							ThrowIfNotElement (c);
							content.Add (new XslProcessingInstruction(c));
							break;
						case "text":
							content.Add (new XslText(c, false));
							break;
						case "value-of":
							content.Add (new XslValueOf(c));
							break;
						case "variable":
							content.Add (new XslLocalVariable (c));
							break;
						default:
							// TODO: handle fallback, like we should
							throw new XsltCompileException ("Did not recognize element " + n.Name, null, n);
						}
						break;
					default:
						if (!c.IsExtensionNamespace (n.NamespaceURI))
							content.Add (new XslLiteralElement(c));
						else {
							if (n.MoveToFirstChild ()) {
								do {
									if (n.NamespaceURI == XsltNamespace && n.LocalName == "fallback")
										content.Add (new XslFallback (c));
								} while (n.MoveToNext ());
								n.MoveToParent ();
							}
						}
						break;
					}
					break;

				case XPathNodeType.SignificantWhitespace:
					content.Add (new XslText(c, true));
					break;
				case XPathNodeType.Text:
					content.Add (new XslText(c, false));
					break;
				default:
					break;
				}

				Debug.ExitNavigator (c);
				
			} while (c.Input.MoveToNext ());
			
			
			if (hasStack) {
				stackSize = c.PopScope ().VariableHighTide;
				hasStack = stackSize > 0;
			} else 
				c.PopScope ();
		}
		
		public override void Evaluate (XslTransformProcessor p)
		{
			if (hasStack)
				p.PushStack (stackSize);
			
			int len = content.Count;
			for (int i = 0; i < len; i++)
				((XslOperation) content [i]).Evaluate (p);
			
			if (hasStack)
				p.PopStack ();
		}
	}
}