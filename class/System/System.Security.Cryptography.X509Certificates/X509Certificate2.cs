//
// System.Security.Cryptography.X509Certificate2 class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2006 Novell Inc. (http://www.novell.com)
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

#if NET_2_0 && SECURITY_DEP

using System.IO;
using System.Text;
using Mono.Security;
using Mono.Security.Cryptography;
using MX = Mono.Security.X509;

namespace System.Security.Cryptography.X509Certificates {

	public class X509Certificate2 : X509Certificate {

		private bool _archived;
		private X509ExtensionCollection _extensions;
		private string _name = String.Empty;
		private string _serial;
		private PublicKey _publicKey;
		private X500DistinguishedName issuer_name;
		private X500DistinguishedName subject_name;
		private Oid signature_algorithm;

		private MX.X509Certificate _cert;

		// constructors

		public X509Certificate2 ()
		{
			_cert = null;
		}

		public X509Certificate2 (byte[] rawData)
		{
			Import (rawData, (string)null, X509KeyStorageFlags.DefaultKeySet);
		}

		public X509Certificate2 (byte[] rawData, string password)
		{
			Import (rawData, password, X509KeyStorageFlags.DefaultKeySet);
		}

		public X509Certificate2 (byte[] rawData, SecureString password)
		{
			Import (rawData, password, X509KeyStorageFlags.DefaultKeySet);
		}

		public X509Certificate2 (byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
		{
			Import (rawData, password, keyStorageFlags);
		}

		public X509Certificate2 (byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags)
		{
			Import (rawData, password, keyStorageFlags);
		}

		public X509Certificate2 (string fileName) : base (fileName) 
		{
			Import (fileName, (string)null, X509KeyStorageFlags.DefaultKeySet);
		}

		public X509Certificate2 (string fileName, string password)
		{
			Import (fileName, password, X509KeyStorageFlags.DefaultKeySet);
		}

		public X509Certificate2 (string fileName, SecureString password)
		{
			Import (fileName, password, X509KeyStorageFlags.DefaultKeySet);
		}

		public X509Certificate2 (string fileName, string password, X509KeyStorageFlags keyStorageFlags)
		{
			Import (fileName, password, keyStorageFlags);
		}

		public X509Certificate2 (string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags)
		{
			Import (fileName, password, keyStorageFlags);
		}

		public X509Certificate2 (IntPtr handle) : base (handle) 
		{
			_cert = new MX.X509Certificate (base.GetRawCertData ());
		}

		public X509Certificate2 (X509Certificate certificate) 
			: base (certificate)
		{
			_cert = new MX.X509Certificate (base.GetRawCertData ());
		}

		// properties

		public bool Archived {
			get { return _archived; }
			set { _archived = value; }
		}

		public X509ExtensionCollection Extensions {
			get {
				if (_extensions == null)
					_extensions = new X509ExtensionCollection (_cert);
				return _extensions;
			}
		}

		public string FriendlyName {
			get { return _name; }
			set { _name = value; }
		}

		// FIXME - Could be more efficient
		public bool HasPrivateKey {
			get { return PrivateKey != null; }
		}

		public X500DistinguishedName IssuerName {
			get {
				if (issuer_name == null)
					issuer_name = new X500DistinguishedName (_cert.GetIssuerName ().GetBytes ());
				return issuer_name;
			}
		} 

		public DateTime NotAfter {
			get { return _cert.ValidUntil; }
		}

		public DateTime NotBefore {
			get { return _cert.ValidFrom; }
		}

		public AsymmetricAlgorithm PrivateKey {
			get {
				try {
					if (_cert.RSA != null) {
						RSACryptoServiceProvider rcsp = _cert.RSA as RSACryptoServiceProvider;
						if (rcsp != null)
							return rcsp.PublicOnly ? null : rcsp;
						RSAManaged rsam = _cert.RSA as RSAManaged;
						if (rsam != null)
							return rsam.PublicOnly ? null : rsam;

						_cert.RSA.ExportParameters (true);
						return _cert.RSA;
					} else if (_cert.DSA != null) {
						DSACryptoServiceProvider dcsp = _cert.DSA as DSACryptoServiceProvider;
						if (dcsp != null)
							return dcsp.PublicOnly ? null : dcsp;
	
						_cert.DSA.ExportParameters (true);
						return _cert.DSA;
					}
				}
				catch {
				}
				return null;
			}
			set {
				if (value is RSA)
					_cert.RSA = (RSA) value;
				else if (value is DSA)
					_cert.DSA = (DSA) value;
				else
					throw new NotSupportedException ();
			}
		} 

		public PublicKey PublicKey {
			get { 
				if (_publicKey == null) {
					try {
						_publicKey = new PublicKey (_cert);
					}
					catch (Exception e) {
						string msg = Locale.GetText ("Unable to decode public key.");
						throw new CryptographicException (msg, e);
					}
				}
				return _publicKey;
			}
		} 

		public byte[] RawData {
			get {
				if (_cert == null) {
					throw new CryptographicException (Locale.GetText ("No certificate data."));
				}
				return base.GetRawCertData ();
			}
		} 

		public string SerialNumber {
			get { 
				if (_serial == null) {
					StringBuilder sb = new StringBuilder ();
					byte[] serial = _cert.SerialNumber;
					for (int i=serial.Length - 1; i >= 0; i--)
						sb.Append (serial [i].ToString ("X2"));
					_serial = sb.ToString ();
				}
				return _serial; 
			}
		} 

		public Oid SignatureAlgorithm {
			get {
				if (signature_algorithm == null)
					signature_algorithm = new Oid (_cert.SignatureAlgorithm);
				return signature_algorithm;
			}
		} 

		public X500DistinguishedName SubjectName {
			get {
				if (subject_name == null)
					subject_name = new X500DistinguishedName (_cert.GetSubjectName ().GetBytes ());
				return subject_name;
			}
		} 

		public string Thumbprint {
			get { return base.GetCertHashString (); }
		} 

		public int Version {
			get { return _cert.Version; }
		}

		// methods

		[MonoTODO ("always returns String.Empty")]
		public string GetNameInfo (X509NameType nameType, bool forIssuer) 
		{
			switch (nameType) {
			case X509NameType.SimpleName:
			case X509NameType.EmailName:
			case X509NameType.UpnName:
			case X509NameType.DnsName:
			case X509NameType.DnsFromAlternativeName:
			case X509NameType.UrlName:
				return String.Empty;
			default:
				throw new ArgumentException ("nameType");
			}
		}

		private void ImportPkcs12 (byte[] rawData, string password)
		{
			MX.PKCS12 pfx = (password == null) ? new MX.PKCS12 (rawData) : new MX.PKCS12 (rawData, password);
			if (pfx.Certificates.Count > 0) {
				_cert = pfx.Certificates [0];
			} else {
				_cert = null;
			}
			if (pfx.Keys.Count > 0) {
				_cert.RSA = (pfx.Keys [0] as RSA);
				_cert.DSA = (pfx.Keys [0] as DSA);
			}
		}

		public override void Import (byte[] rawData) 
		{
			Import (rawData, (string)null, X509KeyStorageFlags.DefaultKeySet);
		}

		[MonoTODO ("missing KeyStorageFlags support")]
		public override void Import (byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
		{
			base.Import (rawData, password, keyStorageFlags);
			if (password == null) {
				try {
					_cert = new Mono.Security.X509.X509Certificate (rawData);
				}
				catch (Exception e) {
					try {
						ImportPkcs12 (rawData, null);
					}
					catch {
						string msg = Locale.GetText ("Unable to decode certificate.");
						// inner exception is the original (not second) exception
						throw new CryptographicException (msg, e);
					}
				}
			} else {
				// try PKCS#12
				try {
					ImportPkcs12 (rawData, password);
				}
				catch {
					// it's possible to supply a (unrequired/unusued) password
					// fix bug #79028
					_cert = new Mono.Security.X509.X509Certificate (rawData);
				}
			}
		}

		[MonoTODO ("SecureString is incomplete")]
		public override void Import (byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags)
		{
			Import (rawData, (string) null, keyStorageFlags);
		}

		public override void Import (string fileName) 
		{
			byte[] rawData = Load (fileName);
			Import (rawData, (string)null, X509KeyStorageFlags.DefaultKeySet);
		}

		[MonoTODO ("missing KeyStorageFlags support")]
		public override void Import (string fileName, string password, X509KeyStorageFlags keyStorageFlags) 
		{
			byte[] rawData = Load (fileName);
			Import (rawData, password, keyStorageFlags);
		}

		[MonoTODO ("SecureString is incomplete")]
		public override void Import (string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags) 
		{
			byte[] rawData = Load (fileName);
			Import (rawData, (string)null, keyStorageFlags);
		}

		private static byte[] Load (string fileName)
		{
			byte[] data = null;
			using (FileStream fs = File.OpenRead (fileName)) {
				data = new byte [fs.Length];
				fs.Read (data, 0, data.Length);
				fs.Close ();
			}
			return data;
		}

		public override void Reset () 
		{
			_serial = null;
			_publicKey = null;
			base.Reset ();
		}

		public override string ToString ()
		{
			return base.ToString (true);
		}

		public override string ToString (bool verbose)
		{
			// the non-verbose X509Certificate2 == verbose X509Certificate
			if (!verbose)
				return base.ToString (true);

			string nl = Environment.NewLine;
			StringBuilder sb = new StringBuilder ();
			sb.AppendFormat ("[Version]{0}  V{1}{0}{0}", nl, Version);
			sb.AppendFormat ("[Subject]{0}  {1}{0}{0}", nl, Subject);
			sb.AppendFormat ("[Issuer]{0}  {1}{0}{0}", nl, Issuer);
			sb.AppendFormat ("[Serial Number]{0}  {1}{0}{0}", nl, SerialNumber);
			sb.AppendFormat ("[Not Before]{0}  {1}{0}{0}", nl, NotBefore);
			sb.AppendFormat ("[Not After]{0}  {1}{0}{0}", nl, NotAfter);
			sb.AppendFormat ("[Thumbprint]{0}  {1}{0}{0}", nl, Thumbprint);
			sb.AppendFormat ("[Signature Algorithm]{0}  {1}({2}){0}{0}", nl, SignatureAlgorithm.FriendlyName, 
				SignatureAlgorithm.Value);

			AsymmetricAlgorithm key = PublicKey.Key;
			sb.AppendFormat ("[Public Key]{0}  Algorithm: ", nl);
			if (key is RSA)
				sb.Append ("RSA");
			else if (key is DSA)
				sb.Append ("DSA");
			else
				sb.Append (key.ToString ());
			sb.AppendFormat ("{0}  Length: {1}{0}  Key Blob: ", nl, key.KeySize);
			AppendBuffer (sb, PublicKey.EncodedKeyValue.RawData);
			sb.AppendFormat ("{0}  Parameters: ", nl);
			AppendBuffer (sb, PublicKey.EncodedParameters.RawData);
			sb.Append (nl);

			return sb.ToString ();
		}

		private static void AppendBuffer (StringBuilder sb, byte[] buffer)
		{
			if (buffer == null)
				return;
			for (int i=0; i < buffer.Length; i++) {
				sb.Append (buffer [i].ToString ("x2"));
				if (i < buffer.Length - 1)
					sb.Append (" ");
			}
		}

		[MonoTODO]
		public bool Verify ()
		{
			X509Chain chain = new X509Chain ();
			if (!chain.Build (this))
				return false;
			// TODO - check chain and other stuff ???
			return true;
		}

		// static methods

		[MonoTODO ("Detection limited to Cert, Pfx, Pkcs12 and Unknown")]
		public static X509ContentType GetCertContentType (byte[] rawData)
		{
			if ((rawData == null) || (rawData.Length == 0))
				throw new ArgumentException ("rawData");

			X509ContentType type = X509ContentType.Unknown;
			try {
				ASN1 data = new ASN1 (rawData);
				if (data.Tag != 0x30) {
					string msg = Locale.GetText ("Unable to decode certificate.");
					throw new CryptographicException (msg);
				}

				if (data.Count == 3) {
					switch (data [0].Tag) {
					case 0x30:
						// SEQUENCE / SEQUENCE / BITSTRING
						if ((data [1].Tag == 0x30) && (data [2].Tag == 0x03))
							type = X509ContentType.Cert;
						break;
					case 0x02:
						// INTEGER / SEQUENCE / SEQUENCE
						if ((data [1].Tag == 0x30) && (data [2].Tag == 0x30))
							type = X509ContentType.Pkcs12;
						// note: Pfx == Pkcs12
						break;
					}
				}
			}
			catch (Exception e) {
				string msg = Locale.GetText ("Unable to decode certificate.");
				throw new CryptographicException (msg, e);
			}

			return type;
		}

		[MonoTODO ("Detection limited to Cert, Pfx, Pkcs12 and Unknown")]
		public static X509ContentType GetCertContentType (string fileName)
		{
			if (fileName == null)
				throw new ArgumentNullException ("fileName");
			if (fileName.Length == 0)
				throw new ArgumentException ("fileName");

			byte[] data = Load (fileName);
			return GetCertContentType (data);
		}
	}
}

#endif
