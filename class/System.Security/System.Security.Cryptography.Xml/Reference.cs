//
// Reference.cs - Reference implementation for XML Signature
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

using System.IO;
using System.Xml;

namespace System.Security.Cryptography.Xml { 

	// http://www.w3.org/TR/2002/REC-xmldsig-core-20020212/Overview.html#sec-Reference
	public class Reference {

		private TransformChain chain;
		private string digestMethod;
		private byte[] digestValue;
		private string id;
		private string uri;
		private string type;
		private Stream stream;
		private XmlElement element;

		public Reference () 
		{
			chain = new TransformChain ();
			digestMethod = XmlSignature.NamespaceURI + "sha1";
		}

		[MonoTODO ("There is no description about how it is used.")]
		public Reference (Stream stream) : this () 
		{
			this.stream = stream;
		}

		public Reference (string uri) : this ()
		{
			this.uri = uri;
		}

		// default to SHA1
		public string DigestMethod {
			get { return digestMethod; }
			set {
				element = null;
				digestMethod = value;
			}
		}

		public byte[] DigestValue {
			get { return digestValue; }
			set {
				element = null;
				digestValue = value;
			}
		}

		public string Id {
			get { return id; }
			set {
				element = null;
				id = value;
			}
		}

		public TransformChain TransformChain {
			get { return chain; }
		}

		public string Type {
			get { return type; }
			set {
				element = null;
				type = value;
			}
		}

		public string Uri {
			get { return uri; }
			set {
				element = null;
				uri = value;
			}
		}

		public void AddTransform (Transform transform) 
		{
			chain.Add (transform);
		}

		public XmlElement GetXml () 
		{
			if (element != null)
				return element;

			if (digestMethod == null)
				throw new CryptographicException ("DigestMethod");
			if (digestValue == null)
				throw new NullReferenceException ("DigestValue");

			XmlDocument document = new XmlDocument ();
			XmlElement xel = document.CreateElement (XmlSignature.ElementNames.Reference, XmlSignature.NamespaceURI);
			if (id != null)
				xel.SetAttribute (XmlSignature.AttributeNames.Id, id);
			if (uri != null)
				xel.SetAttribute (XmlSignature.AttributeNames.URI, uri);
			if (type != null)
				xel.SetAttribute (XmlSignature.AttributeNames.Type, type);

			if (chain.Count > 0) {
				XmlElement ts = document.CreateElement (XmlSignature.ElementNames.Transforms, XmlSignature.NamespaceURI);
				foreach (Transform t in chain) {
					XmlNode xn = t.GetXml ();
					XmlNode newNode = document.ImportNode (xn, true);
					ts.AppendChild (newNode);
				}
				xel.AppendChild (ts);
			}

			XmlElement dm = document.CreateElement (XmlSignature.ElementNames.DigestMethod, XmlSignature.NamespaceURI);
			dm.SetAttribute (XmlSignature.AttributeNames.Algorithm, digestMethod);
			xel.AppendChild (dm);

			XmlElement dv = document.CreateElement (XmlSignature.ElementNames.DigestValue, XmlSignature.NamespaceURI);
			dv.InnerText = Convert.ToBase64String (digestValue);
			xel.AppendChild (dv);

			return xel;
		}

		// note: we do NOT return null -on purpose- if attribute isn't found
		private string GetAttribute (XmlElement xel, string attribute) 
		{
			XmlAttribute xa = xel.Attributes [attribute];
			return ((xa != null) ? xa.InnerText : null);
		}

		public void LoadXml (XmlElement value) 
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			if ((value.LocalName != XmlSignature.ElementNames.Reference) || (value.NamespaceURI != XmlSignature.NamespaceURI))
				throw new CryptographicException ();

			id = GetAttribute (value, XmlSignature.AttributeNames.Id);
			uri = GetAttribute (value, XmlSignature.AttributeNames.URI);
			type = GetAttribute (value, XmlSignature.AttributeNames.Type);
			// Note: order is important for validations
			XmlNodeList xnl = value.GetElementsByTagName (XmlSignature.ElementNames.Transform, XmlSignature.NamespaceURI);
			if ((xnl != null) && (xnl.Count > 0)) {
				Transform t = null;
				foreach (XmlNode xn in xnl) {
					string a = GetAttribute ((XmlElement)xn, XmlSignature.AttributeNames.Algorithm);
					switch (a) {
						case "http://www.w3.org/2000/09/xmldsig#base64":
							t = new XmlDsigBase64Transform ();
							break;
						case "http://www.w3.org/TR/2001/REC-xml-c14n-20010315":
							t = new XmlDsigC14NTransform ();
							break;
						case "http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments":
							t = new XmlDsigC14NWithCommentsTransform ();
							break;
						case "http://www.w3.org/2000/09/xmldsig#enveloped-signature":
							t = new XmlDsigEnvelopedSignatureTransform ();
							break;
						case "http://www.w3.org/TR/1999/REC-xpath-19991116":
							t = new XmlDsigXPathTransform ();
							break;
						case "http://www.w3.org/TR/1999/REC-xslt-19991116":
							t = new XmlDsigXsltTransform ();
							break;
						default:
							throw new NotSupportedException ();
					}
					if (xn.ChildNodes.Count > 0) {
						t.LoadInnerXml (xn.ChildNodes);
					}
					AddTransform (t);
				}
			}
			// get DigestMethod
			DigestMethod = XmlSignature.GetAttributeFromElement (value, XmlSignature.AttributeNames.Algorithm, XmlSignature.ElementNames.DigestMethod);
			// get DigestValue
			XmlElement dig = XmlSignature.GetChildElement (value, XmlSignature.ElementNames.DigestValue, XmlSignature.NamespaceURI);
			if (dig != null)
				DigestValue = Convert.FromBase64String (dig.InnerText);
			element = value;
		}
	}
}
