﻿//
// XQueryContext.cs - XQuery/XPath2 dynamic context
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
#if NET_2_0
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.XPath;
using System.Xml.Query;

namespace Mono.Xml.XPath2
{
	internal class XQueryContextManager
	{
		XQueryStaticContext staticContext;

		// Fixed dynamic context during evaluation
		XmlArgumentList args;
		XmlResolver extDocResolver;

		Stack<XQueryContext> contextStack = new Stack<XQueryContext> ();
		XQueryContext currentContext;
#if SEEMS_CONTEXT_FOR_CURRENT_REQURED
#else
		Stack<XPathItem> contextItemStack = new Stack<XPathItem> ();
#endif
		XmlWriter currentWriter;
		XPathItem input;
		XPathItem currentItem;
		Hashtable currentVariables = new Hashtable ();
		XmlNamespaceManager namespaceManager;

		internal XQueryContextManager (XQueryStaticContext ctx, XPathItem input, XmlWriter writer, XmlResolver resolver, XmlArgumentList args)
		{
			this.input = currentItem = input;
			this.staticContext = ctx;
			this.args = args;
			currentWriter = writer;
			this.extDocResolver = resolver;

			currentContext = new XQueryContext (this);

			namespaceManager = new XmlNamespaceManager (ctx.NameTable);
			foreach (DictionaryEntry de in ctx.NSResolver.GetNamespacesInScope (XmlNamespaceScope.ExcludeXml))
				namespaceManager.AddNamespace (de.Key.ToString (), de.Value.ToString ());
			namespaceManager.PushScope ();
		}

		public bool Initialized {
			get { return currentContext != null; }
		}

		public XmlResolver ExtDocResolver {
			get { return extDocResolver; }
		}

		public XmlArgumentList Arguments {
			get { return args; }
		}

		public Hashtable LocalVariables {
			get { return currentVariables; }
		}

		public XmlWriter Writer {
			get { return currentWriter; }
			// FIXME: might be better avoid setter as public
			set { currentWriter = value; }
		}

		public XPathItem CurrentItem {
			get { return currentContext.CurrentItem; }
		}

		public XPathNavigator CurrentNode {
			get { return (XPathNavigator) CurrentItem; }
		}

		public void PushCurrentItem (XPathItem item)
		{
#if SEEMS_CONTEXT_FOR_CURRENT_REQURED
			contextStack.Push (currentContext);
			currentItem = item;
			currentContext = new XQueryContext (this);
#else
			contextItemStack.Push (currentItem);
			currentItem = item;
#endif
		}

		public void PopCurrentItem ()
		{
#if SEEMS_CONTEXT_FOR_CURRENT_REQURED
			PopContext ();
#else
			currentItem = contextItemStack.Pop ();
#endif
		}

		// FIXME: According to the spec 3.8.1, variales bindings in
		// FLWOR is not necesarrily bound to the order of bindings. 
		// Thus, we might not have to create every XQueryContext for
		// each variable binding (not sure for other kind of bindings).
		public void PushVariable (XmlQualifiedName name, XPathSequence iter)
		{
			contextStack.Push (currentContext);
			currentVariables.Add (name, iter);
			currentContext = new XQueryContext (this);
		}

		public void PopVariable ()
		{
			PopContext ();
		}

		private void PopContext ()
		{
			currentContext = contextStack.Pop ();
		}

		internal XmlNamespaceManager NSManager {
			get { return namespaceManager; }
		}
	}

	public class XQueryContext : IXmlNamespaceResolver
	{
		XQueryContextManager contextManager;
		Hashtable currentVariables;
		XPathItem currentItem;

		internal XQueryContext (XQueryContextManager manager)
		{
			contextManager = manager;
			if (manager.Initialized) // this condition is not filled on initial creation.
				currentItem = manager.CurrentItem;
			currentVariables = (Hashtable) manager.LocalVariables.Clone ();
		}

		public XmlWriter Writer {
			get { return contextManager.Writer; }
			// FIXME: might be better avoid public setter.
			set { contextManager.Writer = value; }
		}

		internal XQueryContextManager ContextManager {
			get { return contextManager; }
		}

		public XPathItem CurrentItem {
			get { return currentItem; }
		}

		internal void PushVariable (XmlQualifiedName name, XPathSequence iter)
		{
			contextManager.PushVariable (name, iter);
		}

		internal void PopVariable ()
		{
			contextManager.PopVariable ();
		}

		internal XPathSequence ResolveVariable (XmlQualifiedName name, XPathSequence context)
		{
			object obj = currentVariables [name];
			if (obj == null)
				obj = contextManager.Arguments.GetParameter (name.Name, name.Namespace);
			if (obj == null)
				// FIXME: location
				throw new XmlQueryException (String.Format ("Cannot resolve variable '{0}'.", name));
			XPathSequence seq = obj as XPathSequence;
			if (seq != null)
				return seq;
			XPathItem item = obj as XPathItem;
			if (item == null)
				item = new XPathAtomicValue (obj, null);
			return new SingleItemIterator (item, context);
		}

		public IXmlNamespaceResolver NSResolver {
			get { return contextManager.NSManager; }
		}

		#region IXmlNamespaceResolver implementation
		public XmlNameTable NameTable {
			get { return contextManager.NSManager.NameTable; }
		}

		public string LookupPrefix (string ns)
		{
			return contextManager.NSManager.LookupPrefix (ns);
		}

		public string LookupPrefix (string ns, bool atomized)
		{
			return contextManager.NSManager.LookupPrefix (ns, atomized);
		}

		public string LookupNamespace (string prefix)
		{
			return contextManager.NSManager.LookupNamespace (prefix);
		}

		public string LookupNamespace (string prefix, bool atomized)
		{
			return contextManager.NSManager.LookupNamespace (prefix, atomized);
		}

		public IDictionary GetNamespacesInScope (XmlNamespaceScope scope)
		{
			return contextManager.NSManager.GetNamespacesInScope (scope);
		}
		#endregion
	}
}

#endif
