//
// KeyInfoRetrievalMethod.cs - KeyInfoRetrievalMethod implementation for XML Signature
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//      Tim Coleman (tim@timcoleman.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) Tim Coleman, 2004
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

using System.Xml;

namespace System.Security.Cryptography.Xml {

	public class KeyInfoRetrievalMethod : KeyInfoClause {

		private string URI;
		private XmlElement element;

#if NET_2_0
		string type;
#endif

		public KeyInfoRetrievalMethod () {}

		public KeyInfoRetrievalMethod (string strUri) 
		{
			URI = strUri;
		}

#if NET_2_0
		public KeyInfoRetrievalMethod (string strUri, string strType)
			: this (strUri)
		{
			Type = strType;
		}

		public string Type {
			get { return type; }
			set {
				element = null;
				type = value;
			}
		}
#endif

		public string Uri {
			get { return URI; }
			set {
				element = null;
				URI = value;
			}
		}


		public override XmlElement GetXml () 
		{
			if (element != null)
				return element;

			XmlDocument document = new XmlDocument ();
			XmlElement xel = document.CreateElement (XmlSignature.ElementNames.RetrievalMethod, XmlSignature.NamespaceURI);
			if (URI != null)
				xel.SetAttribute (XmlSignature.AttributeNames.URI, URI);
#if NET_2_0
			if (Type != null)
				xel.SetAttribute (XmlSignature.AttributeNames.Type, Type);
#endif
			return xel;
		}

		public override void LoadXml (XmlElement value) 
		{
			if (value == null)
				throw new ArgumentNullException ();

			if ((value.LocalName != XmlSignature.ElementNames.RetrievalMethod) || (value.NamespaceURI != XmlSignature.NamespaceURI)) {
				URI = ""; // not null - so we return URI="" as attribute !!!
			} else {
				URI = value.Attributes [XmlSignature.AttributeNames.URI].Value;
#if NET_2_0
				if (value.HasAttribute (XmlSignature.AttributeNames.Type))
					Type = value.Attributes [XmlSignature.AttributeNames.Type].Value;
#endif
				element = value;
			}
		}
	}
}
