//
// KeyInfoX509Data.cs - KeyInfoX509Data implementation for XML Signature
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;

namespace System.Security.Cryptography.Xml {

	// FIXME: framework class isn't documented so compatibility isn't assured!
	internal class IssuerSerial {
		public string Issuer;
		public string Serial;

		public IssuerSerial (string issuer, string serial) 
		{
			Issuer = issuer;
			Serial = serial;
		}
	}

	public class KeyInfoX509Data : KeyInfoClause {

		static private string xmldsig = "http://www.w3.org/2000/09/xmldsig#";

		private byte[] x509crl;
		private ArrayList IssuerSerialList;
		private ArrayList SubjectKeyIdList;
		private ArrayList SubjectNameList;
		private ArrayList X509CertificateList;

		public KeyInfoX509Data () 
		{
			IssuerSerialList = new ArrayList ();
			SubjectKeyIdList = new ArrayList ();
			SubjectNameList = new ArrayList ();
			X509CertificateList = new ArrayList ();
		}

		public KeyInfoX509Data (byte[] rgbCert) : this ()
		{
			AddCertificate (new X509Certificate (rgbCert));
		}

		public KeyInfoX509Data (X509Certificate cert) : this ()
		{
			AddCertificate (cert);
		}

		public ArrayList Certificates {
			get { return X509CertificateList; }
		}

		public byte[] CRL {
			get { return x509crl; }
			set { x509crl = value; }
		}

		public ArrayList IssuerSerials {
			get { return IssuerSerialList; }
		}

		public ArrayList SubjectKeyIds {
			get { return SubjectKeyIdList; }
		}

		public ArrayList SubjectNames {
			get { return SubjectNameList; }
		}

		public void AddCertificate (X509Certificate certificate) 
		{
			X509CertificateList.Add (certificate);
		}

		public void AddIssuerSerial (string issuerName, string serialNumber) 
		{
			IssuerSerial isser = new IssuerSerial (issuerName, serialNumber);
			IssuerSerialList.Add (isser);
		}

		public void AddSubjectKeyId (byte[] subjectKeyId) 
		{
			SubjectKeyIdList.Add (subjectKeyId);
		}

		public void AddSubjectName (string subjectName) 
		{
			SubjectNameList.Add (subjectName);
		}

		public override XmlElement GetXml () 
		{
			// sanity check
			int count = IssuerSerialList.Count + SubjectKeyIdList.Count + SubjectNameList.Count + X509CertificateList.Count;
			if ((x509crl == null) && (count == 0))
				throw new CryptographicException ("value");

			StringBuilder sb = new StringBuilder ();
			sb.Append ("<X509Data xmlns=\"");
			sb.Append (xmldsig);
			sb.Append ("\">");
			// <X509IssuerSerial>
			if (IssuerSerialList.Count > 0) {
				sb.Append ("<X509IssuerSerial>");
				foreach (IssuerSerial iser in IssuerSerialList) {
					sb.Append ("<X509IssuerName>");
					sb.Append (iser.Issuer);
					sb.Append ("</X509IssuerName>");
					sb.Append ("<X509SerialNumber>");
					sb.Append (iser.Serial);
					sb.Append ("</X509SerialNumber>");
				}
				sb.Append ("</X509IssuerSerial>");
			}
			// <X509SKI>
			if (SubjectKeyIdList.Count > 0) {
				foreach (byte[] skid in SubjectKeyIdList) {
					sb.Append ("<X509SKI>");
					sb.Append (Convert.ToBase64String (skid));
					sb.Append ("</X509SKI>");
				}
			}
			// <X509SubjectName>
			if (SubjectNameList.Count > 0) {
				foreach (string subject in SubjectNameList) {
					sb.Append ("<X509SubjectName>");
					sb.Append (subject);
					sb.Append ("</X509SubjectName>");
				}
			}
			// <X509Certificate>
			if (X509CertificateList.Count > 0) {
				foreach (X509Certificate x509 in X509CertificateList) {
					sb.Append ("<X509Certificate>");
					sb.Append (Convert.ToBase64String (x509.GetRawCertData ()));
					sb.Append ("</X509Certificate>");
				}
			}
			// only one <X509CRL> 
			if (x509crl != null) {
				sb.Append ("<X509CRL>");
				sb.Append (Convert.ToBase64String (x509crl));
				sb.Append ("</X509CRL>");
			}
			sb.Append ("</X509Data>");

			XmlDocument doc = new XmlDocument ();
			doc.LoadXml(sb.ToString ());
			return doc.DocumentElement;
		}

		public override void LoadXml (XmlElement element) 
		{
			if (element == null)
				throw new ArgumentNullException ("element");

			IssuerSerialList.Clear ();
			SubjectKeyIdList.Clear ();
			SubjectNameList.Clear ();
			X509CertificateList.Clear ();
			x509crl = null;

			if ((element.LocalName == "X509Data") && (element.NamespaceURI == xmldsig)) {
				XmlNodeList xnl = null;
				// <X509IssuerSerial>
				xnl = element.GetElementsByTagName ("X509IssuerSerial", xmldsig);
				if (xnl != null) {
					for (int i=0; i < xnl.Count; i++) {
						XmlElement xel = (XmlElement) xnl[i];
						XmlNodeList issuer = xel.GetElementsByTagName ("X509IssuerName", xmldsig);
						XmlNodeList serial = xel.GetElementsByTagName ("X509SerialNumber", xmldsig);
						AddIssuerSerial (issuer[0].InnerText, serial[0].InnerText);
					}
				}
				// <X509SKI>
				xnl = element.GetElementsByTagName ("X509SKI", xmldsig);
				if (xnl != null) {
					for (int i=0; i < xnl.Count; i++) {
						byte[] skid = Convert.FromBase64String (xnl[i].InnerXml);
						AddSubjectKeyId (skid);
					}
				}
				// <X509SubjectName>
				xnl = element.GetElementsByTagName ("X509SubjectName", xmldsig);
				if (xnl != null) {
					for (int i=0; i < xnl.Count; i++) {
						AddSubjectName (xnl[i].InnerXml);
					}
				}
				// <X509Certificate>
				xnl = element.GetElementsByTagName ("X509Certificate", xmldsig);
				if (xnl != null) {
					for (int i=0; i < xnl.Count; i++) {
						byte[] cert = Convert.FromBase64String (xnl[i].InnerXml);
						AddCertificate (new X509Certificate (cert));
					}
				}
				// only one <X509CRL> 
				xnl = element.GetElementsByTagName ("X509CRL", xmldsig);
				if ((xnl != null) && (xnl.Count > 0)) {
					x509crl = Convert.FromBase64String (xnl[0].InnerXml);
				}
			}
			else
				throw new CryptographicException ("element");
		}
	}
}