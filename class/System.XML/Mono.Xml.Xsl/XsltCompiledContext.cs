//
// XsltCompiledContext.cs
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	
// (C) 2003 Ben Maurer
//

using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Text;
using System.IO;
using Mono.Xml.Xsl.Functions;
using Mono.Xml.Xsl.Operations;
using System.Reflection;
using BF = System.Reflection.BindingFlags;

using QName = System.Xml.XmlQualifiedName;


namespace Mono.Xml.Xsl {

	internal class XsltCompiledContext : XsltContext {		
		XslTransformProcessor p;
			
		public XslTransformProcessor Processor { get { return p; }}
			
		public XsltCompiledContext (XslTransformProcessor p)
		{
			this.p = p;
		}

		public override string DefaultNamespace { get { return String.Empty; }}


		public override string LookupNamespace (string prefix)
		{
			throw new Exception ("we should never get here");
		}
		
		internal override IXsltContextFunction ResolveFunction (XmlQualifiedName name, XPathResultType [] argTypes)
		{
			IXsltContextFunction func = null;

			string ns = name.Namespace;

			if (ns == null) return null;

			object extension = null;
			
			if (p.Arguments != null)
				extension = p.Arguments.GetExtensionObject (ns);
			
			bool isScript = false;
			if (extension == null) {
				extension = p.ScriptManager.GetExtensionObject (ns);
				if (extension == null)
					return null;
				
				isScript = true;
			}
			
			
			MethodInfo method = FindBestMethod (extension.GetType (), name.Name, argTypes, isScript);
			
			if (method != null) 
				return new XsltExtensionFunction (extension, method);
			return null;
		}
		
		MethodInfo FindBestMethod (Type t, string name, XPathResultType [] argTypes, bool isScript)
		{
			int free, length;
			
			MethodInfo [] mi = t.GetMethods ((isScript ? BF.Public | BF.NonPublic : BF.Public) | BF.Instance | BF.Static);
			if (mi.Length == 0)
				return null;
			
			if (argTypes == null)
				return mi [0]; // if we dont have info on the arg types, nothing we can do


			free = 0;
			// filter on name + num args
			int numArgs = argTypes.Length;
			for (int i = 0; i < mi.Length; i ++) {
				if (mi [i].Name == name && mi [i].GetParameters ().Length == numArgs) 
					mi [free++] = mi [i];
			}
			length = free;
			
			// No method
			if (length == 0)
				return null;
			
			// Thats it!
			if (length == 1)
				return mi [0];
			
			free = 0;
			for (int i = 0; i < length; i ++) {
				bool match = true;
				ParameterInfo [] pi = mi [i].GetParameters ();
				
				for (int par = 0; par < pi.Length; par++) {
					XPathResultType required = argTypes [par];
					if (required == XPathResultType.Any)
						continue; // dunno what it is
					
					XPathResultType actual = XPFuncImpl.GetXPathType (pi [par].ParameterType);
					if (actual != required && actual != XPathResultType.Any) {
						match = false;
						break;
					}
					
					if (actual == XPathResultType.Any) {
						// try to get a stronger gind
						if (required != XPathResultType.NodeSet && !(pi [par].ParameterType == typeof (object)))
						{
							match = false;
							break;
						}
					}
				}
				if (match) return mi [i]; // TODO look for exact match
			}
			return null;
		}
			
		public override IXsltContextVariable ResolveVariable (string prefix, string name)
		{
			throw new Exception ("shouldn't get here");
		}
		
		public override IXsltContextFunction ResolveFunction (string prefix, string name, XPathResultType [] ArgTypes)
		{
			throw new Exception ("shouldn't get here");
		}
		
		internal override System.Xml.Xsl.IXsltContextVariable ResolveVariable(QName q)
		{
			return p.CompiledStyle.ResolveVariable (q);
		}

		public override int CompareDocument (string baseUri, string nextBaseUri) { throw new NotImplementedException (); }

		public override bool PreserveWhitespace (XPathNavigator nav) 
		{
			XPathNavigator tmp = nav.Clone ();
			switch (tmp.NodeType) {
			case XPathNodeType.Root:
				return false;
			case XPathNodeType.Element:
				break;
			default:
				tmp.MoveToParent ();
				break;
			}

			for (; tmp.NodeType == XPathNodeType.Element; tmp.MoveToParent ()) {
				object o = p.CompiledStyle.Style.SpaceControls [new XmlQualifiedName (tmp.LocalName, tmp.NamespaceURI)];
				if (o == null)
					continue;
				XmlSpace space = (XmlSpace) o;
				switch ((XmlSpace) o) {
				case XmlSpace.Preserve:
					return true;
				case XmlSpace.Default:
					return false;
				// None: continue.
				}
			}
			return true;
		}

		public override bool Whitespace { get { throw new NotImplementedException (); }}
	}


}
namespace Mono.Xml.Xsl.Functions {

	internal abstract class XPFuncImpl : IXsltContextFunction {
		int minargs, maxargs;
		XPathResultType returnType;
		XPathResultType [] argTypes;

		public XPFuncImpl () {}
		public XPFuncImpl (int minArgs, int maxArgs, XPathResultType returnType, XPathResultType[] argTypes)
		{
			this.Init(minArgs, maxArgs, returnType, argTypes);
		}
		
		protected void Init (int minArgs, int maxArgs, XPathResultType returnType, XPathResultType[] argTypes)
		{
			this.minargs	= minArgs;
			this.maxargs	= maxArgs;
			this.returnType = returnType;
			this.argTypes	= argTypes;
		}

		public int Minargs { get { return this.minargs; }}
		public int Maxargs { get { return this.maxargs; }}
		public XPathResultType ReturnType { get { return this.returnType; }}
		public XPathResultType [] ArgTypes { get { return this.argTypes; }}
		public object Invoke (XsltContext xsltContext, object [] args, XPathNavigator docContext)
		{
			return Invoke ((XsltCompiledContext)xsltContext, args, docContext);
		}
		
		public abstract object Invoke (XsltCompiledContext xsltContext, object [] args, XPathNavigator docContext);
		
		public static XPathResultType GetXPathType (Type type) {
			switch (Type.GetTypeCode(type)) {
			case TypeCode.String:
				return XPathResultType.String;
			case TypeCode.Boolean:
				return XPathResultType.Boolean;
			case TypeCode.Object:
				if (typeof (XPathNavigator).IsAssignableFrom (type) || typeof (IXPathNavigable).IsAssignableFrom (type))
					return XPathResultType.Navigator;
				
				if (typeof (XPathNodeIterator).IsAssignableFrom (type))
					return XPathResultType.NodeSet;
				
				return XPathResultType.Any;
			case TypeCode.DateTime :
				throw new Exception ();
			default: // Numeric
				return XPathResultType.Number;
			} 
		}
	}
	
	class XsltExtensionFunction : XPFuncImpl {
		private object extension;
		private MethodInfo method;
		private TypeCode [] typeCodes;

		public XsltExtensionFunction (object extension, MethodInfo method)
		{
			this.extension = extension;
			this.method = method;

			ParameterInfo [] parameters = method.GetParameters ();
			int minArgs = parameters.Length;
			int maxArgs = parameters.Length;
			
			this.typeCodes = new TypeCode [parameters.Length];
			XPathResultType[] argTypes = new XPathResultType [parameters.Length];
			
			bool canBeOpt = true;
			for (int i = parameters.Length - 1; 0 <= i; i--) { // optionals at the end
				typeCodes [i] = Type.GetTypeCode (parameters [i].ParameterType);
				argTypes [i] = GetXPathType (parameters [i].ParameterType);
				if (canBeOpt) {
					if (parameters[i].IsOptional)
						minArgs --;
					else
						canBeOpt = false;
				}
			}
			base.Init (minArgs, maxArgs, GetXPathType (method.ReturnType), argTypes);
		}

		public override object Invoke (XsltCompiledContext xsltContext, object [] args, XPathNavigator docContext)
		{
			try {
				object result = method.Invoke(extension, args);
				IXPathNavigable navigable = result as IXPathNavigable;
				if (navigable != null)
					return navigable.CreateNavigator ();

				return result;
			} catch {
				Debug.WriteLine ("****** INCORRECT RESOLUTION **********");
				return "";
			}
		}
	}
	
	class XsltCurrent : XPathFunction {
		public XsltCurrent (FunctionArguments args) : base (args)
		{
			if (args != null)
				throw new XPathException ("current takes 0 args");
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.NodeSet; }}

		public override object Evaluate (BaseIterator iter)
		{
			return new SelfIterator ((iter.NamespaceManager as XsltCompiledContext).Processor.CurrentNode, null);
		}
	}
	
	class XsltDocument : XPathFunction {
		Expression arg0, arg1;
		XPathNavigator doc;
		
		public XsltDocument (FunctionArguments args, Compiler c) : base (args)
		{
			if (args == null || (args.Tail != null && args.Tail.Tail != null))
				throw new XPathException ("document takes one or two args");
			
			arg0 = args.Arg;
			if (args.Tail != null)
				arg1 = args.Tail.Arg;
			doc = c.Input.Clone ();
		}
		public override XPathResultType ReturnType { get { return XPathResultType.NodeSet; }}
		
		public override object Evaluate (BaseIterator iter)
		{
			string baseUri = null;
			if (arg1 != null) {
				XPathNodeIterator it = arg1.EvaluateNodeSet (iter);
				if (it.MoveNext())
					baseUri = it.Current.BaseURI;
				else
					baseUri = VoidBaseUriFlag;
			}

			object o = arg0.Evaluate (iter);
			if (o is XPathNodeIterator)
				return GetDocument ((iter.NamespaceManager as XsltCompiledContext), (XPathNodeIterator)o, baseUri);
			else
				return GetDocument ((iter.NamespaceManager as XsltCompiledContext), o.ToString (), baseUri);
		}
		
		static string VoidBaseUriFlag = "&^)(*&%*^$&$VOID!BASE!URI!";
		
		Uri Resolve (string thisUri, string baseUri, XslTransformProcessor p)
		{
			Debug.WriteLine ("THIS: " + thisUri);
			Debug.WriteLine ("BASE: " + baseUri);
			XmlResolver r = p.Resolver;
			
			Uri uriBase = null;
			if (! object.ReferenceEquals (baseUri, VoidBaseUriFlag))
				uriBase = r.ResolveUri (null, baseUri);
				
			return r.ResolveUri (uriBase, thisUri);
		}
		
		XPathNodeIterator GetDocument (XsltCompiledContext xsltContext, XPathNodeIterator itr, string baseUri)
		{
			ArrayList list = new ArrayList ();
			Hashtable got = new Hashtable ();
			
			while (itr.MoveNext()) {
				Uri uri = Resolve (itr.Current.Value, baseUri != null ? baseUri : /*itr.Current.BaseURI*/doc.BaseURI, xsltContext.Processor);
				if (!got.ContainsKey (uri)) {
					got.Add (uri, null);
					if (uri.ToString () == "") {
						XPathNavigator n = doc.Clone ();
						n.MoveToRoot ();
						list.Add (n);
					} else
						list.Add (xsltContext.Processor.GetDocument (uri));
				}
			}
			
			return new EnumeratorIterator (list.GetEnumerator (), xsltContext);
		}
	
		XPathNodeIterator GetDocument (XsltCompiledContext xsltContext, string arg0, string baseUri)
		{
			Uri uri = Resolve (arg0, baseUri != null ? baseUri : doc.BaseURI, xsltContext.Processor);
			XPathNavigator n;
			if (uri.ToString () == "") {
				n = doc.Clone ();
				n.MoveToRoot ();
			} else
				n = xsltContext.Processor.GetDocument (uri);
			
			return new SelfIterator (n, xsltContext);
		}
	}
	
	class XsltElementAvailable : XPathFunction {
		Expression arg0;
		XmlNamespaceManager nsm;
		
		public XsltElementAvailable (FunctionArguments args, IStaticXsltContext ctx) : base (args)
		{
			if (args == null || args.Tail != null)
				throw new XPathException ("element-available takes 1 arg");
			
			arg0 = args.Arg;
			nsm = ctx.GetNsm ();
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.Boolean; }}

		public override object Evaluate (BaseIterator iter)
		{
			QName name = XslNameUtil.FromString (arg0.EvaluateString (iter), nsm);

			return (
				(name.Namespace == Compiler.XsltNamespace) &&
				(
					//
					// A list of all the instructions (does not include top-level-elements)
					//
					name.Name == "apply-imports" ||
					name.Name == "apply-templates" ||
					name.Name == "call-template" ||
					name.Name == "choose" ||
					name.Name == "comment" ||
					name.Name == "copy" ||
					name.Name == "copy-of" ||
					name.Name == "element" ||
					name.Name == "fallback" ||
					name.Name == "for-each" ||
					name.Name == "message" ||
					name.Name == "number" ||
					name.Name == "processing-instruction" ||
					name.Name == "text" ||
					name.Name == "value-of" ||
					name.Name == "variable"
				)
			);
		}
	}

	class XsltFormatNumber : XPathFunction {
		Expression arg0, arg1, arg2;
		XmlNamespaceManager nsm;
		
		public XsltFormatNumber (FunctionArguments args, IStaticXsltContext ctx) : base (args)
		{
			if (args == null || args.Tail == null || (args.Tail.Tail != null && args.Tail.Tail.Tail != null))
				throw new XPathException ("format-number takes 2 or 3 args");
			
			arg0 = args.Arg;
			arg1 = args.Tail.Arg;
			if (args.Tail.Tail != null) {
				arg2= args.Tail.Tail.Arg;
				nsm = ctx.GetNsm ();
			}
		}
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}
		
		public override object Evaluate (BaseIterator iter)
		{
			double d = arg0.EvaluateNumber (iter);
			string s = arg1.EvaluateString (iter);
			QName nm = QName.Empty;
			
			if (arg2 != null)
				nm = XslNameUtil.FromString (arg2.EvaluateString (iter), nsm);
			
			return (iter.NamespaceManager as XsltCompiledContext).Processor.CompiledStyle
				.LookupDecimalFormat (nm).FormatNumber (d, s);
		}
	}
	
	class XsltFunctionAvailable : XPathFunction {
		Expression arg0;
		XmlNamespaceManager nsm;
		
		public XsltFunctionAvailable (FunctionArguments args, IStaticXsltContext ctx) : base (args)
		{
			if (args == null || args.Tail != null)
				throw new XPathException ("element-available takes 1 arg");
			
			arg0 = args.Arg;
			nsm = ctx.GetNsm ();
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.Boolean; }}
		
		public override object Evaluate (BaseIterator iter)
		{
			
			string name = arg0.EvaluateString (iter);
			int colon = name.IndexOf (':');
			// extension function
			if (colon > 0)
				return (iter.NamespaceManager as XsltCompiledContext).ResolveFunction (
					XslNameUtil.FromString (name, nsm),
					null) != null;
			
			return (
				//
				// XPath
				//
                                name == "boolean" ||
                                name == "ceiling" ||
                                name == "concat" ||
                                name == "contains" ||
                                name == "count" ||
                                name == "false" ||
                                name == "floor" ||
                                name == "id"||
                                name == "lang" ||
                                name == "last" ||
                                name == "local-name" ||
                                name == "name" ||
                                name == "namespace-uri" ||
                                name == "normalize-space" ||
                                name == "not" ||
                                name == "number" ||
                                name == "position" ||
                                name == "round" ||
                                name == "starts-with" ||
                                name == "string" ||
                                name == "string-length" ||
                                name == "substring" ||
                                name == "substring-after" ||
                                name == "substring-before" ||
                                name == "sum" ||
                                name == "translate" ||
                                name == "true" ||
				// XSLT
				name == "document" ||
				name == "format-number" ||
				name == "function-available" ||
				name == "generate-id" ||
				name == "key" ||
				name == "current" ||
				name == "unparsed-entity-uri" ||
				name == "element-available" ||
				name == "system-property"
			);
		}
	} 

	class XsltGenerateId : XPathFunction {
		Expression arg0;
		public XsltGenerateId (FunctionArguments args) : base (args)
		{
			if (args != null) {
				if (args.Tail != null)
					throw new XPathException ("generate-id takes 1 or no args");
				arg0 = args.Arg;
			}
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}
		public override object Evaluate (BaseIterator iter)
		{
			XPathNavigator n;
			if (arg0 != null) {
				XPathNodeIterator itr = arg0.EvaluateNodeSet (iter);
				if (itr.MoveNext ())
					n = itr.Current.Clone ();
				else
					return string.Empty; // empty nodeset == empty string
			} else
				n = iter.Current.Clone ();
			
			StringBuilder sb = new StringBuilder ("Mono"); // Ensure begins with alpha
			sb.Append (XmlConvert.EncodeLocalName (n.BaseURI));
			sb.Replace ('_', 'm'); // remove underscores from EncodeLocalName
			sb.Append (n.NodeType);
			sb.Append ('m');

			do {
				sb.Append (IndexInParent (n));
				sb.Append ('m');
			} while (n.MoveToParent ());
			
			return sb.ToString ();
		}
		
		int IndexInParent (XPathNavigator nav)
		{
			int n = 0;
			while (nav.MoveToPrevious ())
				n++;
			
			return n;
		}
		
	} 
	
	class XsltKey : XPathFunction {
		Expression arg0, arg1;
		XmlNamespaceManager nsm;
		
		public XsltKey (FunctionArguments args, IStaticXsltContext ctx) : base (args)
		{
			if (args == null || args.Tail == null)
				throw new XPathException ("key takes 2 args");
			arg0 = args.Arg;
			arg1 = args.Tail.Arg;
			nsm = ctx.GetNsm ();
		}
		public Expression KeyName { get { return arg0; } }
		public Expression Field { get { return arg1; } }
		public XmlNamespaceManager NamespaceManager { get { return nsm; } }
		public override XPathResultType ReturnType { get { return XPathResultType.NodeSet; }}
		
		public override object Evaluate (BaseIterator iter)
		{
			ArrayList result = new ArrayList ();
			QName name = XslNameUtil.FromString (arg0.EvaluateString (iter), nsm);
			object o = arg1.Evaluate (iter);
			XPathNodeIterator it = o as XPathNodeIterator;
			
			if (it != null) {
				while (it.MoveNext())
					FindKeyMatch ((iter.NamespaceManager as XsltCompiledContext), name, it.Current.Value, result, iter.Current);
			} else {
				FindKeyMatch ((iter.NamespaceManager as XsltCompiledContext), name, XPathFunctions.ToString (o), result, iter.Current);
			}
			
			return new EnumeratorIterator (result.GetEnumerator (), (iter.NamespaceManager as XsltCompiledContext));
		}
		
		void FindKeyMatch (XsltCompiledContext xsltContext, QName name, string value, ArrayList result, XPathNavigator context)
		{
			XPathNavigator searchDoc = context.Clone ();
			searchDoc.MoveToRoot ();
			XslKey key = xsltContext.Processor.CompiledStyle.Style.FindKey (name);
			if (key != null) {
				XPathNodeIterator desc = searchDoc.SelectDescendants (XPathNodeType.All, true);

				while (desc.MoveNext ()) {
					if (key.Matches (desc.Current, value))
						AddResult (result, desc.Current);
					
					if (desc.Current.MoveToFirstAttribute ()) {
						do {
							if (key.Matches (desc.Current, value))
								AddResult (result, desc.Current);	
						} while (desc.Current.MoveToNextAttribute ());
						
						desc.Current.MoveToParent ();
					}
				}
			}
		}

		void AddResult (ArrayList result, XPathNavigator nav)
		{
			for (int i = 0; i < result.Count; i++) {
				XmlNodeOrder docOrder = nav.ComparePosition (((XPathNavigator)result [i]));
				if (docOrder == XmlNodeOrder.Same)
					return;
				
				if (docOrder == XmlNodeOrder.Before) {
					result.Insert(i, nav.Clone ());
					return;
				}
			}
			result.Add (nav.Clone ());
		}
	}
	
	class XsltSystemProperty : XPathFunction {
		Expression arg0;
		XmlNamespaceManager nsm;
		
		public XsltSystemProperty (FunctionArguments args, IStaticXsltContext ctx) : base (args)
		{
			if (args == null || args.Tail != null)
				throw new XPathException ("system-property takes 1 arg");
			
			arg0 = args.Arg;
			nsm = ctx.GetNsm ();
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}
		public override object Evaluate (BaseIterator iter)
		{
			QName name = XslNameUtil.FromString (arg0.EvaluateString (iter), nsm);
			
			if (name.Namespace == Compiler.XsltNamespace) {
				switch (name.Name) {
					case "version": return "1.0";
					case "vendor": return "Mono";
					case "vendor-url": return "http://www.go-mono.com/";
				}
			}
			
			return "";
		}
	} 

	class XsltUnparsedEntityUri : XPathFunction {
		Expression arg0;
		
		public XsltUnparsedEntityUri (FunctionArguments args) : base (args)
		{
			if (args == null || args.Tail != null)
				throw new XPathException ("unparsed-entity-uri takes 1 arg");
			
			arg0 = args.Arg;
		}
		
		public override XPathResultType ReturnType { get { return XPathResultType.String; }}
		public override object Evaluate (BaseIterator iter)
		{
			IHasXmlNode xn = iter.Current as IHasXmlNode;
			if (xn == null)
				return String.Empty;
			XmlNode n = xn.GetNode ();
			XmlDocumentType doctype = n.OwnerDocument.DocumentType;
			if (doctype == null)
				return String.Empty;
			XmlEntity ent = doctype.Entities.GetNamedItem (arg0.EvaluateString (iter)) as XmlEntity;
			if (ent == null)
				return String.Empty;
			else
				return ent.BaseURI;
		}
	}
}