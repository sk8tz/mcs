//
// Signature.cs - Signature implementation for XML Signature
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System.Collections;
using System.Security.Cryptography;
using System.Xml;

namespace System.Security.Cryptography.Xml {

	public class Signature {

		private ArrayList list;
		private SignedInfo info;
		private KeyInfo key;
		private string id;
		private byte[] signature;

		public Signature() 
		{
			list = new ArrayList ();
		}

		public string Id {
			get { return id; }
			set { id = value; }
		}

		public KeyInfo KeyInfo {
			get { return key; }
			set { key = value; }
		}

		public IList ObjectList {
			get { return list; }
			set { list = ArrayList.Adapter (value); }
		}

		public byte[] SignatureValue {
			get { return signature; }
			set { signature = value; }
		}

		public SignedInfo SignedInfo {
			get { return info; }
			set { info = value; }
		}

		public void AddObject (DataObject dataObject) 
		{
			list.Add (dataObject);
		}

		public XmlElement GetXml () 
		{
			if (info == null)
				throw new CryptographicException ("SignedInfo");
			if (signature == null)
				throw new CryptographicException ("SignatureValue");

			XmlDocument document = new XmlDocument ();
			XmlElement xel = document.CreateElement (XmlSignature.ElementNames.Signature, XmlSignature.NamespaceURI);
			if (id != null)
				xel.SetAttribute (XmlSignature.AttributeNames.Id, id);

			XmlNode xn = info.GetXml ();
			XmlNode newNode = document.ImportNode (xn, true);
			xel.AppendChild (newNode);

			if (signature != null) {
				XmlElement sv = document.CreateElement (XmlSignature.ElementNames.SignatureValue, XmlSignature.NamespaceURI);
				sv.InnerText = Convert.ToBase64String (signature);
				xel.AppendChild (sv);
			}

			if (key != null) {
				xn = key.GetXml ();
				newNode = document.ImportNode (xn, true);
				xel.AppendChild (newNode);
			}

			if (list.Count > 0) {
				foreach (DataObject obj in list) {
					xn = obj.GetXml ();
					newNode = document.ImportNode (xn, true);
					xel.AppendChild (newNode);
				}
			}

			return xel;
		}

		private string GetAttribute (XmlElement xel, string attribute) 
		{
			XmlAttribute xa = xel.Attributes [attribute];
			return ((xa != null) ? xa.InnerText : null);
		}

		public void LoadXml (XmlElement value) 
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			if ((value.LocalName == XmlSignature.ElementNames.Signature) && (value.NamespaceURI == XmlSignature.NamespaceURI)) {
				id = GetAttribute (value, XmlSignature.AttributeNames.Id);

				XmlNodeList xnl = value.GetElementsByTagName (XmlSignature.ElementNames.SignedInfo);
				if ((xnl != null) && (xnl.Count == 1)) {
					info = new SignedInfo ();
					info.LoadXml ((XmlElement) xnl[0]);
				}

				xnl = value.GetElementsByTagName (XmlSignature.ElementNames.SignatureValue);
				if ((xnl != null) && (xnl.Count == 1)) {
					signature = Convert.FromBase64String (xnl[0].InnerText);
				}

				xnl = value.GetElementsByTagName (XmlSignature.ElementNames.KeyInfo);
				if ((xnl != null) && (xnl.Count == 1)) {
					key = new KeyInfo ();
					key.LoadXml ((XmlElement) xnl[0]);
				}

				xnl = value.GetElementsByTagName (XmlSignature.ElementNames.Object);
				if ((xnl != null) && (xnl.Count > 0)) {
					foreach (XmlNode xn in xnl) {
						DataObject obj = new DataObject ();
						obj.LoadXml ((XmlElement) xn);
						AddObject (obj);
					}
				}
			}

			// if invalid
			if (info == null)
				throw new CryptographicException ("SignedInfo");
			if (signature == null)
				throw new CryptographicException ("SignatureValue");
		}
	}
}