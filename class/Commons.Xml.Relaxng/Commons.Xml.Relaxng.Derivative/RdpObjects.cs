//
// Commons.Xml.Relaxng.Derivative.RdpObjects.cs
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// 2003 Atsushi Enomoto "No rights reserved."
//

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

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;

namespace Commons.Xml.Relaxng.Derivative
{
	///
	/// Datatype Related Classes
	///
	public class RdpParamList : ArrayList
	{
		public RdpParamList () : base ()
		{
		}
	}

	public class RdpParam
	{
		public RdpParam (string localName, string value)
		{
			this.localName = localName;
			this.value = value;
		}

		string value;
		public string Value {
			get { return this.value; }
		}

		string localName;
		public string LocalName {
			get { return localName; }
		}
	}

	public class RdpDatatype
	{
		RelaxngDatatypeProvider provider;
		string localName;
		string ns;
		RelaxngDatatype datatype;

		public RdpDatatype (string ns, string localName, RelaxngParamList parameters, RelaxngDatatypeProvider provider)
		{
			this.ns = ns;
			this.localName = localName;
			this.provider = provider;
			if (provider == null)
				provider = RelaxngMergedProvider.DefaultProvider;
			datatype = provider.GetDatatype (localName, ns, parameters);
			if (datatype == null) {
				throw new RelaxngException ("Invalid datatype was found.");
//				datatype = RelaxngString.Instance;
			}
		}

		public string NamespaceURI {
			get { return ns; }
		}

		public string LocalName {
			get { return localName; }
		}

		public virtual bool IsAllowed (string value, XmlReader reader)
		{
			return datatype.IsValid (value, reader);
		}

		static char [] wsChars = new char [] {' ', '\n', '\r', '\t'};

		public virtual bool IsTypeEqual (string s1, string s2, XmlReader reader)
		{
			return datatype.CompareString (s1, s2, reader);
		}
	}

	/*

	///
	/// ChildNode Classes
	///
	public abstract class RdpChildNode
	{
		// Strip
		public virtual bool IsNonWhitespaceText {
			get { return false; }
		}
	}

	public class RdpTextChild : RdpChildNode
	{
		public RdpTextChild (string text)
		{
			this.text = text;
		}

		string text;
		public string Text {
			get { return text; }
		}

		public override bool IsNonWhitespaceText {
			get { return RdpUtil.Whitespace (text); }
		}
	}

	public class RdpElementChild : RdpChildNode
	{
		public RdpElementChild (string name, string ns, RdpContext ctx, RdpAttributes attributes, RdpChildNodes childNodes)
		{
			this.name = name;
			this.ns = ns;
			this.ctx = ctx;
			this.attributes = attributes;
			this.childNodes = childNodes;
		}

		string name;
		public string LocalName {
			get { return name; }
		}

		string ns;
		public string NamespaceURI {
			get { return ns; }
		}

		RdpContext ctx;
		public RdpContext Context {
			get { return ctx; }
		}

		RdpAttributes attributes;
		public RdpAttributes Attributes {
			get { return attributes; }
		}

		RdpChildNodes childNodes;
		public RdpChildNodes ChildNodes {
			get { return childNodes; }
		}
	}
	*/

	/*
	public class RdpChildNodes : ArrayList
	{
		public RdpChildNodes () : base ()
		{
		}
	}

	public class RdpAttributes : ArrayList
	{
		public RdpAttributes () : base ()
		{
		}
	}

	public class RdpAttributeNode : RdpChildNode
	{
		public RdpAttributeNode (string name, string ns, string value) : base ()
		{
			this.name = name;
			this.ns = ns;
			this.value = value;
		}

		string value;
		public string Value {
			get { return value; }
		}

		string name;
		public string LocalName {
			get { return name; }
		}

		string ns;
		public string NamespaceURI {
			get { return ns; }
		}
	}
	*/
}

