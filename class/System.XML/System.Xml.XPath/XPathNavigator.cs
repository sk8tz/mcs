//
// System.Xml.XPath.XPathNavigator
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//
// (C) 2002 Jason Diamond  http://injektilo.org/
//

using System;
using Mono.Xml.XPath;

namespace System.Xml.XPath
{
	public abstract class XPathNavigator : ICloneable
	{
		#region Constructor

		protected XPathNavigator ()
		{
		}

		#endregion

		#region Properties

		public abstract string BaseURI { get; }

		public abstract bool HasAttributes { get; }

		public abstract bool HasChildren { get; }

		public abstract bool IsEmptyElement { get; }

		public abstract string LocalName { get; }

		public abstract string Name { get; }

		public abstract string NamespaceURI { get; }

		public abstract XmlNameTable NameTable { get; }

		public abstract XPathNodeType NodeType { get; }

		public abstract string Prefix { get; }

		public abstract string Value { get; }

		public abstract string XmlLang { get; }

		int Depth
		{
			get
			{
				int cLevels = 0;
				XPathNavigator nav = Clone ();
				while (nav.MoveToParent ())
					cLevels ++;
				return cLevels;
			}
		}

		#endregion

		#region Methods

		public abstract XPathNavigator Clone ();

		public virtual XmlNodeOrder ComparePosition (XPathNavigator nav)
		{
			if (IsSamePosition (nav))
				return XmlNodeOrder.Same;

			XPathNavigator nav1 = Clone ();
			XPathNavigator nav2 = nav.Clone ();

			int nDepth1 = nav1.Depth;
			int nDepth2 = nav2.Depth;

			if (nDepth1 > nDepth2)
			{
				while (nDepth1 > nDepth2)
				{
					nav1.MoveToParent ();
					nDepth1 --;
				}
				if (nav1.IsSamePosition (nav2))
					return XmlNodeOrder.After;
			}
			else if (nDepth1 < nDepth2)
			{
				while (nDepth1 < nDepth2)
				{
					nav2.MoveToParent ();
					nDepth2 --;
				}
				if (nav1.IsSamePosition (nav2))
					return XmlNodeOrder.Before;
			}

			XPathNavigator parent1 = nav1.Clone ();
			XPathNavigator parent2 = nav2.Clone ();
			while (parent1.MoveToParent () && parent2.MoveToParent ())
			{
				if (parent1.IsSamePosition (parent2))
				{
					// the ordering is namespace, attribute, children
					// assume that nav1 is before nav2, find counter-example
					if (nav1.NodeType == XPathNodeType.Namespace)
					{
						if (nav2.NodeType == XPathNodeType.Namespace)
						{
							// match namespaces
							while (nav2.MoveToNextNamespace ())
								if (nav2.IsSamePosition (nav1))
									return XmlNodeOrder.After;
						}
					}
					else if (nav1.NodeType == XPathNodeType.Attribute)
					{
						if (nav2.NodeType == XPathNodeType.Namespace)
							return XmlNodeOrder.After;
						else if (nav2.NodeType == XPathNodeType.Attribute)
						{
							// match attributes
							while (nav2.MoveToNextAttribute ())
								if (nav2.IsSamePosition (nav1))
									return XmlNodeOrder.After;
						}
					}
					else
					{
						// match children
						while (nav2.MoveToNext ())
							if (nav2.IsSamePosition (nav1))
								return XmlNodeOrder.After;
					}
					return XmlNodeOrder.Before;
				}
				nav1.MoveToParent ();
				nav2.MoveToParent ();
			}
			return XmlNodeOrder.Unknown;
		}

		public virtual XPathExpression Compile (string xpath)
		{
			XPathParser parser = new XPathParser ();
			return new CompiledExpression (parser.Compile (xpath));
		}
		
		internal virtual XPathExpression Compile (string xpath, System.Xml.Xsl.IStaticXsltContext ctx)
		{
			XPathParser parser = new XPathParser (ctx);
			return new CompiledExpression (parser.Compile (xpath));
		}

		public virtual object Evaluate (string xpath)
		{
			return Evaluate (Compile (xpath));
		}

		public virtual object Evaluate (XPathExpression expr)
		{
			return Evaluate (expr, null);
		}

		public virtual object Evaluate (XPathExpression expr, XPathNodeIterator context)
		{
			return Evaluate (expr, context, null);
		}
		
		internal virtual object Evaluate (XPathExpression expr, XPathNodeIterator context, XmlNamespaceManager ctx)
		{
			CompiledExpression cexpr = (CompiledExpression) expr;
			if (ctx == null)
				ctx = cexpr.NamespaceManager;
			
			if (context == null)
				context = new NullIterator (this, ctx);
			BaseIterator iterContext = (BaseIterator) context;
			iterContext.NamespaceManager = ctx;
			return cexpr.Evaluate (iterContext);
		}

		internal XPathNodeIterator EvaluateNodeSet (XPathExpression expr, XPathNodeIterator context, XmlNamespaceManager ctx)
		{
			CompiledExpression cexpr = (CompiledExpression) expr;
			if (ctx == null)
				ctx = cexpr.NamespaceManager;
			
			if (context == null)
				context = new NullIterator (this, cexpr.NamespaceManager);
			BaseIterator iterContext = (BaseIterator) context;
			iterContext.NamespaceManager = ctx;
			return cexpr.EvaluateNodeSet (iterContext);
		}

		internal string EvaluateString (XPathExpression expr, XPathNodeIterator context, XmlNamespaceManager ctx)
		{
			CompiledExpression cexpr = (CompiledExpression) expr;
			if (ctx == null)
				ctx = cexpr.NamespaceManager;
			
			if (context == null)
				context = new NullIterator (this, cexpr.NamespaceManager);
			BaseIterator iterContext = (BaseIterator) context;
			iterContext.NamespaceManager = ctx;
			return cexpr.EvaluateString (iterContext);
		}

		internal double EvaluateNumber (XPathExpression expr, XPathNodeIterator context, XmlNamespaceManager ctx)
		{
			CompiledExpression cexpr = (CompiledExpression) expr;
			if (ctx == null)
				ctx = cexpr.NamespaceManager;
			
			if (context == null)
				context = new NullIterator (this, cexpr.NamespaceManager);
			BaseIterator iterContext = (BaseIterator) context;
			iterContext.NamespaceManager = ctx;
			return cexpr.EvaluateNumber (iterContext);
		}

		internal bool EvaluateBoolean (XPathExpression expr, XPathNodeIterator context, XmlNamespaceManager ctx)
		{
			CompiledExpression cexpr = (CompiledExpression) expr;
			if (ctx == null)
				ctx = cexpr.NamespaceManager;
			
			if (context == null)
				context = new NullIterator (this, cexpr.NamespaceManager);
			BaseIterator iterContext = (BaseIterator) context;
			iterContext.NamespaceManager = ctx;
			return cexpr.EvaluateBoolean (iterContext);
		}

		public abstract string GetAttribute (string localName, string namespaceURI);

		public abstract string GetNamespace (string name);
		
		object ICloneable.Clone ()
		{
			return Clone ();
		}

		public virtual bool IsDescendant (XPathNavigator nav)
		{
			if (nav != null)
			{
				nav = nav.Clone ();
				while (nav.MoveToParent ())
				{
					if (IsSamePosition (nav))
						return true;
				}
			}
			return false;
		}

		public abstract bool IsSamePosition (XPathNavigator other);

		public virtual bool Matches (string xpath)
		{
			return Matches (Compile (xpath));
		}

		[MonoTODO]	// optimize...
		public virtual bool Matches (XPathExpression expr)
		{
			Expression e = ((CompiledExpression)expr).ExpressionNode;
			
			if (e is NodeTest)
				return ((NodeTest)e).Match (((CompiledExpression)expr).NamespaceManager, this);
			if (e is ExprFilter) {
				do {
					e = ((ExprFilter)e).LeftHandSide;
				} while (e is ExprFilter);
				
				if (e is NodeTest && !((NodeTest)e).Match (((CompiledExpression)expr).NamespaceManager, this))
					return false;
			}
			
			//e = ((CompiledExpression)expr).ExpressionNode;
			//Console.WriteLine ("Didnt filter : " + e.GetType ().ToString () + " " + e.ToString ());
			
			XPathNodeIterator nodes = Select (expr);

			while (nodes.MoveNext ()) {
				if (IsSamePosition (nodes.Current))
					return true;
			}

			XPathNavigator navigator = Clone ();

			while (navigator.MoveToParent ()) {
				nodes = navigator.Select (expr);

				while (nodes.MoveNext ()) {
					if (IsSamePosition (nodes.Current))
						return true;
				}
			}

			return false;
		}

		public abstract bool MoveTo (XPathNavigator other);

		public abstract bool MoveToAttribute (string localName, string namespaceURI);

		public abstract bool MoveToFirst ();

		public abstract bool MoveToFirstAttribute ();

		public abstract bool MoveToFirstChild ();

		public bool MoveToFirstNamespace ()
		{
			return MoveToFirstNamespace (XPathNamespaceScope.All);
		}

		public abstract bool MoveToFirstNamespace (XPathNamespaceScope namespaceScope);

		public abstract bool MoveToId (string id);

		public abstract bool MoveToNamespace (string name);

		public abstract bool MoveToNext ();

		public abstract bool MoveToNextAttribute ();

		public bool MoveToNextNamespace ()
		{
			return MoveToNextNamespace (XPathNamespaceScope.All);
		}

		public abstract bool MoveToNextNamespace (XPathNamespaceScope namespaceScope);

		public abstract bool MoveToParent ();

		public abstract bool MoveToPrevious ();

		public abstract void MoveToRoot ();

		public virtual XPathNodeIterator Select (string xpath)
		{
			return Select (Compile (xpath));
		}

		public virtual XPathNodeIterator Select (XPathExpression expr)
		{
			return Select (expr, null);
		}
		
		internal virtual XPathNodeIterator Select (XPathExpression expr, XmlNamespaceManager ctx)
		{
			CompiledExpression cexpr = (CompiledExpression) expr;
			if (ctx == null)
				ctx = cexpr.NamespaceManager;
			
			BaseIterator iter = new NullIterator (this, ctx);
			return cexpr.EvaluateNodeSet (iter);	
		}

		public virtual XPathNodeIterator SelectAncestors (XPathNodeType type, bool matchSelf)
		{
			Axes axis = (matchSelf) ? Axes.AncestorOrSelf : Axes.Ancestor;
			return SelectTest (new NodeTypeTest (axis, type));
		}

		public virtual XPathNodeIterator SelectAncestors (string name, string namespaceURI, bool matchSelf)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (namespaceURI == null)
				throw new ArgumentNullException ("namespaceURI");

			Axes axis = (matchSelf) ? Axes.AncestorOrSelf : Axes.Ancestor;
			XmlQualifiedName qname = new XmlQualifiedName (name, namespaceURI);
			return SelectTest (new NodeNameTest (axis, qname, true));
		}

		public virtual XPathNodeIterator SelectChildren (XPathNodeType type)
		{
			return SelectTest (new NodeTypeTest (Axes.Child, type));
		}

		public virtual XPathNodeIterator SelectChildren (string name, string namespaceURI)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (namespaceURI == null)
				throw new ArgumentNullException ("namespaceURI");

			Axes axis = Axes.Child;
			XmlQualifiedName qname = new XmlQualifiedName (name, namespaceURI);
			return SelectTest (new NodeNameTest (axis, qname, true));
		}

		public virtual XPathNodeIterator SelectDescendants (XPathNodeType type, bool matchSelf)
		{
			Axes axis = (matchSelf) ? Axes.DescendantOrSelf : Axes.Descendant;
			return SelectTest (new NodeTypeTest (axis, type));
		}

		public virtual XPathNodeIterator SelectDescendants (string name, string namespaceURI, bool matchSelf)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (namespaceURI == null)
				throw new ArgumentNullException ("namespaceURI");


			Axes axis = (matchSelf) ? Axes.DescendantOrSelf : Axes.Descendant;
			XmlQualifiedName qname = new XmlQualifiedName (name, namespaceURI);
			return SelectTest (new NodeNameTest (axis, qname, true));
		}

		internal XPathNodeIterator SelectTest (NodeTest test)
		{
			return test.EvaluateNodeSet (new NullIterator (this));
		}

		public override string ToString ()
		{
			return Value;
		}

		#endregion
	}
}
