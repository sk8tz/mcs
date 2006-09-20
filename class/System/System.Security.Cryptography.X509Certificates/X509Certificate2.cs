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
using Mono.Security.Cryptography;
using MX = Mono.Security.X509;

namespace System.Security.Cryptography.X509Certificates {

	public class X509Certificate2 : X509Certificate {

		private bool _archived;
		private X509ExtensionCollection _extensions;
		private string _name;
		private string _serial;
		private PublicKey _publicKey;
		X500DistinguishedName _issuer_name;

		private MX.X509Certificate _cert;

		// constructors

		public X509Certificate2 () : base () 
		{
			_cert = null;
		}

		public X509Certificate2 (byte[] rawData) : base (rawData) 
		{
			_cert = new MX.X509Certificate (base.GetRawCertData ());
		}

		public X509Certificate2 (byte[] rawData, string password) : base (rawData, password) 
		{
			Import (rawData, password, X509KeyStorageFlags.DefaultKeySet);
		}

		public X509Certificate2 (byte[] rawData, SecureString password) : base (rawData, password) 
		{
			Import (rawData, password, X509KeyStorageFlags.DefaultKeySet);
		}

		public X509Certificate2 (byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
			: base (rawData, password, keyStorageFlags) 
		{
			Import (rawData, password, keyStorageFlags);
		}

		public X509Certificate2 (byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags)
			: base (rawData, password, keyStorageFlags) 
		{
			Import (rawData, password, keyStorageFlags);
		}

		public X509Certificate2 (string fileName) : base (fileName) 
		{
			_cert = new MX.X509Certificate (base.GetRawCertData ());
		}

		public X509Certificate2 (string fileName, string password)
			: base (fileName, password)
		{
			Import (fileName, password, X509KeyStorageFlags.DefaultKeySet);
		}

		public X509Certificate2 (string fileName, SecureString password)
			: base (fileName, password)
		{
			Import (fileName, password, X509KeyStorageFlags.DefaultKeySet);
		}

		public X509Certificate2 (string fileName, string password, X509KeyStorageFlags keyStorageFlags)
			: base (fileName, password, keyStorageFlags) 
		{
			Import (fileName, password, keyStorageFlags);
		}

		public X509Certificate2 (string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags)
			: base (fileName, password, keyStorageFlags) 
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
			get { return _extensions; }
		}

		public string FriendlyName {
			get { return _name; }
			set { _name = value; }
		}

		[MonoTODO ("Probably it could be more efficient")]
		public bool HasPrivateKey {
			get { return PrivateKey != null; }
		}

		[MonoTODO]
		public X500DistinguishedName IssuerName {
			get {
				if (_issuer_name == null)
					_issuer_name = new X500DistinguishedName (Issuer);
				return _issuer_name;
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
				if (_cert.RSA != null) {
					RSACryptoServiceProvider rcsp = _cert.RSA as RSACryptoServiceProvider;
					if (rcsp != null)
						return rcsp.PublicOnly ? null : rcsp;
					RSAManaged rsam = _cert.RSA as RSAManaged;
					if (rsam != null)
						return rsam.PublicOnly ? null : rsam;
					try {
						_cert.RSA.ExportParameters (true);
						return _cert.RSA;
					} catch (CryptographicException) {
						return null;
					}
				}
				else if (_cert.DSA != null) {
					DSACryptoServiceProvider dcsp = _cert.DSA as DSACryptoServiceProvider;
					if (dcsp != null)
						return dcsp.PublicOnly ? null : dcsp;
					try {
						_cert.DSA.ExportParameters (true);
						return _cert.DSA;
					} catch (CryptographicException) {
						return null;
					}
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
					_publicKey = new PublicKey (_cert);
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
			get { return null; }
		} 

		[MonoTODO]
		public X500DistinguishedName SubjectName {
			get { return null; }
		} 

		public string Thumbprint {
			get { return base.GetCertHashString (); }
		} 

		public int Version {
			get { return _cert.Version; }
		}

		// methods

		[MonoTODO]
		public string GetNameInfo (X509NameType nameType, bool forIssuer) 
		{
			return null;
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
				_cert = new Mono.Security.X509.X509Certificate (rawData);
				// TODO - PKCS12 without password
			} else {
				// try PKCS#12
				try {
					MX.PKCS12 pfx = new MX.PKCS12 (rawData, password);
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

		private byte[] Load (string fileName)
		{
			byte[] data = null;
			using (FileStream fs = new FileStream (fileName, FileMode.Open)) {
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

		[MonoTODO]
		public override string ToString ()
		{
			return null;
		}

		[MonoTODO]
		public override string ToString (bool verbose)
		{
			return null;
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

		[MonoTODO]
		public static X509ContentType GetCertContentType (byte[] rawData)
		{
			return X509ContentType.Unknown;
		}

		[MonoTODO]
		public static X509ContentType GetCertContentType (string fileName)
		{
			return X509ContentType.Unknown;
		}
	}
}

#endif
