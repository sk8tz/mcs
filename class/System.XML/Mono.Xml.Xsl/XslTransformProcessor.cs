//
// XslTransformProcessor.cs
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//	
// (C) 2003 Ben Maurer
// (C) 2003 Atsushi Enomoto
//

using System;
using System.IO;
using System.Collections;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using Mono.Xml.Xsl.Operations;

using QName = System.Xml.XmlQualifiedName;

namespace Mono.Xml.Xsl {
	public class XslTransformProcessor 
	{
		CompiledStylesheet compiledStyle;
		
		XslStylesheet style;
		
		Stack currentTemplateStack = new Stack ();
		
		XPathNavigator root;
		XsltContext ctx;
		XsltArgumentList args;
		XmlResolver resolver;

		// Store the values of global params
		internal Hashtable globalVariableTable = new Hashtable ();
		
		public XslTransformProcessor (CompiledStylesheet style)
		{
			this.compiledStyle = style;
			this.style = style.Style;
		}

		public void Process (XPathNavigator root, Outputter outputtter, XsltArgumentList args, XmlResolver resolver)
		{
			this.args = args;
			this.root = root;
			this.resolver = resolver != null ? resolver : new XmlUrlResolver ();
			
			PushNodeset (root.Select ("."));
			
			foreach (XslGlobalVariable v in CompiledStyle.Variables.Values)	{
				if (args != null && v is XslGlobalParam) {
					object p = args.GetParam(v.Name.Name, v.Name.Namespace);
					if (p != null)
						((XslGlobalParam)v).Override (this, p);
					else
						v.Evaluate (this);
				}
				v.Evaluate (this);
			}
			
			PopNodeset ();
			
			this.PushOutput (outputtter);
			this.ApplyTemplates (root.Select ("."), QName.Empty, null);
			this.PopOutput ();
		}
		
		public XsltContext Context { get { return ctx; }}
		public CompiledStylesheet CompiledStyle { get { return compiledStyle; }}
		public XsltArgumentList Arguments {get{return args;}}
		
		#region Document Resolution
		public XmlResolver Resolver {get{return resolver;}}
		
		Hashtable docCache;
		
		public XPathNavigator GetDocument (Uri uri)
		{
			XPathNavigator result;
			
			if (docCache != null) {
				result = docCache [uri] as XPathNavigator;
				if (result != null)
					return result.Clone();
			} else {
				docCache = new Hashtable();
			}

			XmlReader rdr = new XmlTextReader (uri.ToString(), (Stream) resolver.GetEntity (uri, null, null));
			result = new XPathDocument (rdr).CreateNavigator ();
			rdr.Close ();
			
			docCache [uri] = result.Clone ();
			return result;
		}
		
		#endregion
		
		#region Output
		Stack outputStack = new Stack ();
		
		public Outputter Out { get { return (Outputter)outputStack.Peek(); }}
		
		public void PushOutput (Outputter newOutput)
		{
			this.outputStack.Push (newOutput);
		}
		
		public Outputter PopOutput ()
		{
			Outputter ret = (Outputter)this.outputStack.Pop ();
			ret.Done ();
			return ret;
		}
		
		public Hashtable Outputs { get { return compiledStyle.Outputs; }}
		#endregion
		
		#region Templates -- Apply/Call
		public void ApplyTemplates (XPathNodeIterator nodes, QName mode, Hashtable withParams)
		{
			PushNodeset (nodes);
			while (NodesetMoveNext ()) {
				XslTemplate t = FindTemplate (CurrentNode, mode);
				currentTemplateStack.Push (t);
				t.Evaluate (this, withParams);
				currentTemplateStack.Pop ();
			}
			
			PopNodeset ();
		}
		
		public void CallTemplate (QName name, Hashtable withParams)
		{
			XslTemplate t = FindTemplate (name);
			currentTemplateStack.Push (null);
			t.Evaluate (this, withParams);
			currentTemplateStack.Pop ();
		}
		
		public void ApplyImports ()
		{	

			XslTemplate currentTemplate = (XslTemplate)currentTemplateStack.Peek();
			if (currentTemplate == null) throw new Exception ("Invalid context for apply-imports");
			XslTemplate t;
			
			for (int i = currentTemplate.Parent.Imports.Count - 1; i >= 0; i--) {
				XslStylesheet s = (XslStylesheet)currentTemplate.Parent.Imports [i];
				t = s.Templates.FindMatch (CurrentNode, currentTemplate.Mode);
				if (t != null) {					
					currentTemplateStack.Push (t);
					t.Evaluate (this);
					currentTemplateStack.Pop ();
					return;
				}
			}
			
			switch (CurrentNode.NodeType) {
			case XPathNodeType.Root:
			case XPathNodeType.Element:
				if (currentTemplate.Mode == QName.Empty)
					t = XslDefaultNodeTemplate.Instance;
				else
					t = new XslDefaultNodeTemplate(currentTemplate.Mode);
			
				break;
			case XPathNodeType.Attribute:
			case XPathNodeType.SignificantWhitespace:
			case XPathNodeType.Text:
			case XPathNodeType.Whitespace:
				t = XslDefaultTextTemplate.Instance;
				break;
			
			case XPathNodeType.Comment:
			case XPathNodeType.ProcessingInstruction:
				t = XslEmptyTemplate.Instance;
				break;
			
			default:
				t = XslEmptyTemplate.Instance;
				break;
			}
			currentTemplateStack.Push (t);
			t.Evaluate (this);
			currentTemplateStack.Pop ();
		}
		
		XslTemplate FindTemplate (XPathNavigator node, QName mode)
		{
			XslTemplate ret = style.Templates.FindMatch (CurrentNode, mode);
			
			if (ret != null) return ret;

			switch (node.NodeType) {
			case XPathNodeType.Root:
			case XPathNodeType.Element:
				if (mode == QName.Empty)
					return XslDefaultNodeTemplate.Instance;
				else
					return new XslDefaultNodeTemplate(mode);
			
			case XPathNodeType.Attribute:
			case XPathNodeType.SignificantWhitespace:
			case XPathNodeType.Text:
			case XPathNodeType.Whitespace:
				return XslDefaultTextTemplate.Instance;
			
			case XPathNodeType.Comment:
			case XPathNodeType.ProcessingInstruction:
				return XslEmptyTemplate.Instance;
			
			default:
				return XslEmptyTemplate.Instance;
			}
		}
		
		XslTemplate FindTemplate (QName name)
		{
			XslTemplate ret = style.Templates.FindTemplate (name);
			if (ret != null) return ret;
				
			throw new Exception ("Could not resolve named template " + name);
		}
		
		#endregion
		

		public void PushForEachContext ()
		{
			currentTemplateStack.Push (null);
		}
		
		public void PopForEachContext ()
		{
			currentTemplateStack.Pop ();
		}
		

		#region Nodeset Context
		Stack nodesetStack = new Stack ();
		
		public XPathNodeIterator CurrentNodeset {
			get { return (XPathNodeIterator)nodesetStack.Peek (); }
		}
		
		public XPathNavigator CurrentNode {
			get { return CurrentNodeset.Current; }
		}
		
		public bool NodesetMoveNext ()
		{
			return CurrentNodeset.MoveNext ();
		}
		
		public void PushNodeset (XPathNodeIterator itr)
		{
			nodesetStack.Push (itr.Clone ());
		}
		
		public void PopNodeset ()
		{
			nodesetStack.Pop ();
		}
		#endregion
		
		#region Evaluate
		public object Evaluate (XPathExpression expr)
		{
			expr = CompiledStyle.ExpressionStore.PrepForExecution (expr, this);
			
			XPathNodeIterator itr = CurrentNodeset;
			return itr.Current.Evaluate (expr, itr);
		}
		
		public string EvaluateString (XPathExpression expr)
		{
			expr = CompiledStyle.ExpressionStore.PrepForExecution (expr, this);
			
			XPathNodeIterator itr = CurrentNodeset;
			return itr.Current.EvaluateString (expr, itr);
		}
				
		public bool EvaluateBoolean (XPathExpression expr)
		{
			expr = CompiledStyle.ExpressionStore.PrepForExecution (expr, this);
			
			XPathNodeIterator itr = CurrentNodeset;
			return itr.Current.EvaluateBoolean (expr, itr);
		}
		
		public double EvaluateNumber (XPathExpression expr)
		{
			expr = CompiledStyle.ExpressionStore.PrepForExecution (expr, this);
			
			XPathNodeIterator itr = CurrentNodeset;
			return itr.Current.EvaluateNumber (expr, itr);
		}
		
		public XPathNodeIterator Select (XPathExpression expr)
		{
			expr = CompiledStyle.ExpressionStore.PrepForExecution (expr, this);
			return CurrentNodeset.Current.Select (expr);
		}
		
		#endregion
		
		public XslAttributeSet ResolveAttributeSet (QName name)
		{
			return CompiledStyle.ResolveAttributeSet (name);
		}
		
		#region Variable Stack
		Stack variableStack = new Stack ();
		object [] currentStack;
		
		public object GetStackItem (int slot)
		{
			return currentStack [slot];
		}
		
		public void SetStackItem (int slot, object o)
		{
			currentStack [slot] = o;
		}
		
		public void PushStack (int stackSize)
		{
			variableStack.Push (currentStack);
			currentStack = new object [stackSize];
		}
		
		public void PopStack ()
		{
			currentStack = (object[])variableStack.Pop();
		}
		
		#endregion
		
		#region Free/Busy
		Hashtable busyTable = new Hashtable ();
		static object busyObject = new object ();
		
		public void SetBusy (object o)
		{
			busyTable [o] = busyObject;
		}
		
		public void SetFree (object o)
		{
			busyTable.Remove (o);
		}
		
		public bool IsBusy (object o)
		{
			return busyTable [o] == busyObject;
		}
		#endregion
		
	}
}
