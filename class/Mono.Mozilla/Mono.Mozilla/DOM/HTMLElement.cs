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
// Copyright (c) 2007, 2008 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)
//

using System;
using System.Runtime.InteropServices;
using System.Text;
using Mono.WebBrowser;
using Mono.WebBrowser.DOM;

namespace Mono.Mozilla.DOM
{
	internal class HTMLElement : Element, IElement
	{
		private nsIDOMHTMLElement element {
			get { return base.element as nsIDOMHTMLElement; }
			set { base.element = value as nsIDOMElement; }
		}

		public HTMLElement (WebBrowser control, nsIDOMHTMLElement domHtmlElement) : base (control, domHtmlElement as nsIDOMElement)
		{
			this.element = domHtmlElement;
		}

		#region IDisposable Members
		protected override  void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					this.element = null;
				}
			}
			base.Dispose(disposing);
		}		
		#endregion

		#region IElement Members
		public override IElement AppendChild (IElement child) {
			nsIDOMNode newChild;
			HTMLElement elem = (HTMLElement) child;
			this.element.appendChild (elem.element, out newChild);
			return new HTMLElement (control, newChild as nsIDOMHTMLElement);
		}
		
		public override IElement InsertBefore (INode child, INode refChild) {
			nsIDOMNode newChild;
			Node elem = (Node) child;
			Node reference = (Node) refChild;
			this.element.insertBefore (elem.node, reference.node, out newChild);
			return new HTMLElement (control, newChild as nsIDOMHTMLElement);
		}		

		public new string InnerHTML
		{
			get {
				nsIDOMNSHTMLElement nsElem = this.element as nsIDOMNSHTMLElement;
				nsElem.getInnerHTML (storage);
				return Base.StringGet (storage);
			}
			set {
				nsIDOMNSHTMLElement nsElem = this.element as nsIDOMNSHTMLElement;
				Base.StringSet (storage, value);
				nsElem.setInnerHTML (storage);
			}
		}

		public override string OuterHTML
		{
			// bad emulation of outerHTML since gecko doesn't support it :P
			get {
				string tag = this.TagName;
				string str = "<" + tag;
				foreach (IAttribute att in this.Attributes) {
					str += " " + att.Name + "=\"" + att.Value + "\"";
				}
				nsIDOMNSHTMLElement nsElem = this.element as nsIDOMNSHTMLElement;
				nsElem.getInnerHTML (storage);
				str += ">" + Base.StringGet (storage) + "</" + tag + ">";
				return str;
			}
			set {
				nsIDOMDocumentRange docRange = ((Document) control.Document).ComObject as nsIDOMDocumentRange;
				nsIDOMRange range;
				docRange.createRange (out range);
				range.setStartBefore (this.element);
				nsIDOMNSRange nsRange = range as nsIDOMNSRange;
				Base.StringSet (storage, value);
				nsIDOMDocumentFragment fragment;
				nsRange.createContextualFragment (storage, out fragment);
				nsIDOMNode parent;
				this.element.getParentNode (out parent);
				parent = nsDOMNode.GetProxy (this.control, parent);
				nsIDOMNode newNode;
				parent.replaceChild (fragment as nsIDOMNode, this.element as nsIDOMNode, out newNode);
				this.element = newNode as Mono.Mozilla.nsIDOMHTMLElement;
			}
		}
		
		public override bool Disabled
		{			
			get {
				if (this.HasAttribute ("disabled")) {
					string dis = this.GetAttribute ("disabled");
					return bool.Parse (dis);
				}
				return false;
			}
			set {
				if (this.HasAttribute ("disabled")) {
					this.SetAttribute ("disabled", value.ToString ());
				}
			}
		}

		public override int TabIndex {
			get { 
				int tabIndex;
				((nsIDOMNSHTMLElement)this.element).getTabIndex (out tabIndex);
				return tabIndex;
			}
			set { 
				((nsIDOMNSHTMLElement)this.element).setTabIndex (value);
			}
		}

		public override int GetHashCode () {
			return this.hashcode;
		}
		#endregion
	}
}
