//
// EncryptedKey.cs - EncryptedKey implementation for XML Encryption
// http://www.w3.org/2001/04/xmlenc#sec-EncryptedKey
//
// Author:
//      Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004

#if NET_2_0

using System.Xml;

namespace System.Security.Cryptography.Xml {
	public sealed class EncryptedKey : EncryptedType {

		#region Fields

		string carriedKeyName;
		string recipient;
		ReferenceList referenceList;

		#endregion // Fields

		#region Constructors

		public EncryptedKey ()
			: base ()
		{
			CarriedKeyName = null;
			Recipient = null;
			ReferenceList = new ReferenceList ();
		}

		#endregion // Constructors

		#region Properties

		public string CarriedKeyName {
			get { return carriedKeyName; }
			set { carriedKeyName = value; }
		}

		public string Recipient {
			get { return recipient; }
			set { recipient = value; }
		}

		public ReferenceList ReferenceList {
			get { return referenceList; }
			set { referenceList = value; }
		}

		#endregion // Properties

		#region Methods

		public void AddReference (DataReference dataReference)
		{
			ReferenceList.Add (dataReference);
		}

		public void AddReference (KeyReference keyReference)
		{
			ReferenceList.Add (keyReference);
		}

		public override XmlElement GetXml ()
		{
			return GetXml (new XmlDocument ());
		}

		internal XmlElement GetXml (XmlDocument document)
		{
			if (CipherData == null)
				throw new CryptographicException ("Cipher data is not specified.");

			XmlElement xel = document.CreateElement (XmlEncryption.ElementNames.EncryptedData, EncryptedXml.XmlEncNamespaceUrl);

			if (EncryptionMethod != null)
				xel.AppendChild (EncryptionMethod.GetXml (document));
			if (KeyInfo != null) 
				xel.AppendChild (document.ImportNode (KeyInfo.GetXml (), true));
			if (CipherData != null)
				xel.AppendChild (CipherData.GetXml (document));

			if (EncryptionProperties.Count > 0) {
				XmlElement xep = document.CreateElement (XmlEncryption.ElementNames.EncryptionProperties, EncryptedXml.XmlEncNamespaceUrl);
				foreach (EncryptionProperty p in EncryptionProperties)
					xep.AppendChild (p.GetXml (document));
				xel.AppendChild (xep);
			}

			if (ReferenceList.Count > 0) {
				XmlElement xrl = document.CreateElement (XmlEncryption.ElementNames.ReferenceList, EncryptedXml.XmlEncNamespaceUrl);
				foreach (EncryptedReference er in ReferenceList) 
					xrl.AppendChild (er.GetXml (document));
				xel.AppendChild (xrl);
			}

			if (CarriedKeyName != null) {
				XmlElement xck = document.CreateElement (XmlEncryption.ElementNames.CarriedKeyName, EncryptedXml.XmlEncNamespaceUrl);
				xck.InnerText = CarriedKeyName;
				xel.AppendChild (xck);
			}

			if (Id != null)
				xel.SetAttribute (XmlEncryption.AttributeNames.Id, Id);
			if (Type != null)
				xel.SetAttribute (XmlEncryption.AttributeNames.Type, Type);
			if (MimeType != null)
				xel.SetAttribute (XmlEncryption.AttributeNames.MimeType, MimeType);
			if (Encoding != null)
				xel.SetAttribute (XmlEncryption.AttributeNames.Encoding, Encoding);
			if (Recipient != null)
				xel.SetAttribute (XmlEncryption.AttributeNames.Recipient, Recipient);
			return xel;
		}

		public override void LoadXml (XmlElement value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			if ((value.LocalName != XmlEncryption.ElementNames.EncryptedKey) || (value.NamespaceURI != EncryptedXml.XmlEncNamespaceUrl))
				throw new CryptographicException ("Malformed EncryptedKey element.");
			else {
				EncryptionMethod = null;
				KeyInfo keyInfo = null;
				CipherData cipherData = null;
				EncryptionMethod = null;
				EncryptionProperties = new EncryptionProperties ();
				ReferenceList = new ReferenceList ();
				CarriedKeyName = null;
				Id = null;
				Type = null;
				MimeType = null;
				Encoding = null;
				Recipient = null;

				foreach (XmlNode n in value.ChildNodes) {
					if (n is XmlWhitespace)
						continue;

					switch (n.LocalName) {
					case XmlEncryption.ElementNames.EncryptionMethod:
						EncryptionMethod = new EncryptionMethod ();
						EncryptionMethod.LoadXml ((XmlElement) n);
						break;
					case XmlSignature.ElementNames.KeyInfo:
						KeyInfo = new KeyInfo ();
						KeyInfo.LoadXml ((XmlElement) n);
						break;
					case XmlEncryption.ElementNames.CipherData:
						CipherData = new CipherData ();
						CipherData.LoadXml ((XmlElement) n);
						break;
					case XmlEncryption.ElementNames.EncryptionProperties:
						foreach (XmlElement element in ((XmlElement) n).GetElementsByTagName (XmlEncryption.ElementNames.EncryptionProperty, EncryptedXml.XmlEncNamespaceUrl))
							EncryptionProperties.Add (new EncryptionProperty (element));
						break;
					case XmlEncryption.ElementNames.ReferenceList:
						foreach (XmlNode r in ((XmlElement) n).ChildNodes) {
							if (r is XmlWhitespace) 
								continue;

							switch (r.LocalName) {
							case XmlEncryption.ElementNames.DataReference:
								DataReference dr = new DataReference ();
								dr.LoadXml ((XmlElement) r);
								AddReference (dr);
								break;
							case XmlEncryption.ElementNames.KeyReference:
								KeyReference kr = new KeyReference ();
								kr.LoadXml ((XmlElement) r);
								AddReference (kr);
								break;
							}
						}
						break;
					case XmlEncryption.ElementNames.CarriedKeyName:
						CarriedKeyName = ((XmlElement) n).InnerText;
						break;
					}
				}

				if (value.HasAttribute (XmlEncryption.AttributeNames.Id))
					Id = value.Attributes [XmlEncryption.AttributeNames.Id].Value;
				if (value.HasAttribute (XmlEncryption.AttributeNames.Type))
					Type = value.Attributes [XmlEncryption.AttributeNames.Type].Value;
				if (value.HasAttribute (XmlEncryption.AttributeNames.MimeType))
					MimeType = value.Attributes [XmlEncryption.AttributeNames.MimeType].Value;
				if (value.HasAttribute (XmlEncryption.AttributeNames.Encoding))
					Encoding = value.Attributes [XmlEncryption.AttributeNames.Encoding].Value;
				if (value.HasAttribute (XmlEncryption.AttributeNames.Recipient))
					Encoding = value.Attributes [XmlEncryption.AttributeNames.Recipient].Value;
			}
		}

		#endregion // Methods
	}
}

#endif
