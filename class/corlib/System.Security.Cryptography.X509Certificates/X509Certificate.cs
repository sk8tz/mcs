//
// X509Certificates.cs: Handles X.509 certificates.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
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

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

using Mono.Security;
using Mono.Security.Authenticode;
using Mono.Security.X509;

#if !(NET_1_0 || NET_1_1)
using System.Runtime.Serialization;
#endif

namespace System.Security.Cryptography.X509Certificates {

	// References:
	// a.	Internet X.509 Public Key Infrastructure Certificate and CRL Profile
	//	http://www.ietf.org/rfc/rfc3280.txt
	
	// LAMESPEC: the MSDN docs always talks about X509v3 certificates
	// and/or Authenticode certs. However this class works with older
	// X509v1 certificates and non-authenticode (code signing) certs.
#if (NET_1_0 || NET_1_1)
	[Serializable]
	public class X509Certificate {
#else
	public class X509Certificate : IDeserializationCallback, ISerializable {
#endif
		// typedef struct _CERT_CONTEXT {
                //	DWORD                   dwCertEncodingType;
                //	BYTE                    *pbCertEncoded;
		//	DWORD                   cbCertEncoded;
		//	PCERT_INFO              pCertInfo;
		//	HCERTSTORE              hCertStore;
		// } CERT_CONTEXT, *PCERT_CONTEXT;
		// typedef const CERT_CONTEXT *PCCERT_CONTEXT;
		[StructLayout (LayoutKind.Sequential)]
		internal struct CertificateContext {
			public UInt32 dwCertEncodingType;
			public IntPtr pbCertEncoded;
			public UInt32 cbCertEncoded;
			public IntPtr pCertInfo;
			public IntPtr hCertStore;
		}
		// NOTE: We only define the CryptoAPI structure (from WINCRYPT.H)
		// so we don't create any dependencies on Windows DLL in corlib

		private Mono.Security.X509.X509Certificate x509;
		private bool hideDates;
		private byte[] cachedCertificateHash;
	
		// almost every byte[] returning function has a string equivalent
		// sadly the BitConverter insert dash between bytes :-(
		private string tostr (byte[] data) 
		{
			if (data != null) {
				StringBuilder sb = new StringBuilder ();
				for (int i = 0; i < data.Length; i++)
					sb.Append (data[i].ToString ("X2"));
				return sb.ToString ();
			}
			else
				return null;
		}
	
		// static methods
	
		public static X509Certificate CreateFromCertFile (string filename) 
		{
			byte[] data = null;
			FileStream fs = new FileStream (filename, FileMode.Open);
			try {
				data = new byte [fs.Length];
				fs.Read (data, 0, data.Length);
			}
			finally {
				fs.Close ();
			}
	
			return new X509Certificate (data);
		}
	
		[MonoTODO ("Incomplete - minimal validation in this version")]
		public static X509Certificate CreateFromSignedFile (string filename)
		{
			AuthenticodeDeformatter a = new AuthenticodeDeformatter (filename);
			if (a.SigningCertificate != null) {
				return new X509Certificate (a.SigningCertificate.RawData);
			}
			else {
				// if no signature is present return an empty certificate
				byte[] cert = null; // must not confuse compiler about null ;)
				return new X509Certificate (cert);
			}
		}
	
		// constructors
	
		// special constructor for Publisher (and related classes).
		// Dates strings are null
		internal X509Certificate (byte[] data, bool dates) 
		{
			if (data != null) {
				x509 = new Mono.Security.X509.X509Certificate (data);
				hideDates = !dates;
			}
		}
	
		public X509Certificate (byte[] data) : this (data, true) {}
	
		public X509Certificate (IntPtr handle) 
		{
			CertificateContext cc = (CertificateContext) Marshal.PtrToStructure (handle, typeof (CertificateContext));
			byte[] data = new byte [cc.cbCertEncoded];
			Marshal.Copy (cc.pbCertEncoded, data, 0, (int)cc.cbCertEncoded);
			x509 = new Mono.Security.X509.X509Certificate (data);
		}
	
		public X509Certificate (System.Security.Cryptography.X509Certificates.X509Certificate cert) 
		{
			if (cert != null) {
				byte[] data = cert.GetRawCertData ();
				if (data != null)
					x509 = new Mono.Security.X509.X509Certificate (data);
				hideDates = false;
			}
		}

#if !(NET_1_0 || NET_1_1)
		public X509Certificate () {}

		public X509Certificate (byte[] rawData, string password) {}

		public X509Certificate (byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags) {}

		public X509Certificate (string fileName)
		{
		}

		public X509Certificate (string fileName, string password)
		{
		}

		public X509Certificate (string fileName, string password, X509KeyStorageFlags keyStorageFlags) {}

		public X509Certificate (SerializationInfo info, StreamingContext context) {}
#endif

		// public methods
	
		public virtual bool Equals (System.Security.Cryptography.X509Certificates.X509Certificate cert)
		{
			if (cert != null) {
				byte[] raw = cert.GetRawCertData ();
				if (raw != null) {
					if (x509 == null)
						return false;
					if (x509.RawData == null)
						return false;
					if (raw.Length == x509.RawData.Length) {
						for (int i = 0; i < raw.Length; i++) {
							if (raw[i] != x509.RawData [i])
								return false;
						}
						// well no choice must be equals!
						return true;
					}
					else
						return false;
				}
			}
			return (x509.RawData == null);
		}
	
		// LAMESPEC: This is the equivalent of the "thumbprint" that can be seen
		// in the certificate viewer of Windows. This is ALWAYS the SHA1 hash of
		// the certificate (i.e. it has nothing to do with the actual hash 
		// algorithm used to sign the certificate).
		public virtual byte[] GetCertHash () 
		{
			// we'll hash the cert only once and only if required
			if ((cachedCertificateHash == null) && (x509 != null)) {
				SHA1 sha = SHA1.Create ();
				cachedCertificateHash = sha.ComputeHash (x509.RawData);
			}
			return cachedCertificateHash;
		}
	
		public virtual string GetCertHashString () 
		{
			// must call GetCertHash (not variable) or optimization wont work
			return tostr (GetCertHash ());
		}
	
		// strangly there are no DateTime returning function
		// LAMESPEC: Microsoft returns the local time from Pacific Time (GMT-8)
		// BUG: This will not be corrected in Framework 1.1 and also affect WSE 1.0
		public virtual string GetEffectiveDateString ()
		{
			if (hideDates)
				return null;
			DateTime dt = x509.ValidFrom.ToUniversalTime().AddHours (-8);
			return dt.ToString (); //"yyyy-MM-dd HH:mm:ss");
		}
	
		// strangly there are no DateTime returning function
		// LAMESPEC: Microsoft returns the local time from Pacific Time (GMT-8)
		// BUG: This will not be corrected in Framework 1.1 and also affect WSE 1.0
		public virtual string GetExpirationDateString () 
		{
			if (hideDates)
				return null;
			DateTime dt = x509.ValidUntil.ToUniversalTime().AddHours (-8);
			return dt.ToString (); //"yyyy-MM-dd HH:mm:ss");
		}
	
		// well maybe someday there'll be support for PGP or SPKI ?
		public virtual string GetFormat () 
		{
			return "X509";	// DO NOT TRANSLATE
		}
	
		public override int GetHashCode ()
		{
			// the cert hash may not be (yet) calculated
			if (cachedCertificateHash == null)
				GetCertHash();
		
			// return the integer of the first 4 bytes of the cert hash
			if ((cachedCertificateHash != null) && (cachedCertificateHash.Length >= 4))
				return ((cachedCertificateHash[0] << 24) |(cachedCertificateHash[1] << 16) |
					(cachedCertificateHash[2] << 8) | cachedCertificateHash[3]);
			else
				return 0;
		}
	
		public virtual string GetIssuerName () 
		{
			return x509.IssuerName;
		}
	
		public virtual string GetKeyAlgorithm () 
		{
			return x509.KeyAlgorithm;
		}
	
		public virtual byte[] GetKeyAlgorithmParameters () 
		{
			return x509.KeyAlgorithmParameters;
		}
	
		public virtual string GetKeyAlgorithmParametersString () 
		{
			return tostr (x509.KeyAlgorithmParameters);
		}
	
		public virtual string GetName ()
		{
			return x509.SubjectName;
		}
	
		public virtual byte[] GetPublicKey () 
		{
			return x509.PublicKey;
		}
	
		public virtual string GetPublicKeyString () 
		{
			return tostr (x509.PublicKey);
		}
	
		public virtual byte[] GetRawCertData () 
		{
			return ((x509 != null) ? x509.RawData : null);
		}
	
		public virtual string GetRawCertDataString () 
		{
			return ((x509 != null) ? tostr (x509.RawData) : null);
		}
	
		public virtual byte[] GetSerialNumber () 
		{
			return x509.SerialNumber;
		}
	
		public virtual string GetSerialNumberString () 
		{
			return tostr (x509.SerialNumber);
		}
	
		// to please corcompare ;-)
		public override string ToString () 
		{
			return base.ToString ();
		}
	
		public virtual string ToString (bool details) 
		{
			if (details) {
				string nl = Environment.NewLine;
				StringBuilder sb = new StringBuilder ();
				sb.Append ("CERTIFICATE:");
				sb.Append (nl);
				sb.Append ("\tFormat:  ");
				sb.Append (GetFormat ());
				if (x509.SubjectName != null) {
					sb.Append (nl);
					sb.Append ("\tName:  ");
					sb.Append (GetName ());
				}
				if (x509.IssuerName != null) {
					sb.Append (nl);
					sb.Append ("\tIssuing CA:  ");
					sb.Append (GetIssuerName ());
				}
				if (x509.SignatureAlgorithm != null) {
					sb.Append (nl);
					sb.Append ("\tKey Algorithm:  ");
					sb.Append (GetKeyAlgorithm ());
				}
				if (x509.SerialNumber != null) {
					sb.Append (nl);
					sb.Append ("\tSerial Number:  ");
					sb.Append (GetSerialNumberString ());
				}
				// Note: Algorithm is not spelled right as the actual 
				// MS implementation (we do exactly the same for the
				// comparison in the unit tests)
				if (x509.KeyAlgorithmParameters != null) {
					sb.Append (nl);
					sb.Append ("\tKey Alogrithm Parameters:  ");
					sb.Append (GetKeyAlgorithmParametersString ());
				}
				if (x509.PublicKey != null) {
					sb.Append (nl);
					sb.Append ("\tPublic Key:  ");
					sb.Append (GetPublicKeyString ());
				}
				sb.Append (nl);
				sb.Append (nl);
				return sb.ToString ();
			}
			else
				return base.ToString ();
		}

#if !(NET_1_0 || NET_1_1)
		public virtual byte[] Export (X509ContentType contentType)
		{
			return null;
		}

		public virtual byte[] Export (X509ContentType contentType, string password)
		{
			return null;
		}

		void IDeserializationCallback.OnDeserialization (object sender) {}

		public virtual void Import (byte[] rawData) {}

		public virtual void Import (byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags) {}

		public virtual void Import (string fileName) {}

		public virtual void Import (string fileName, string password, X509KeyStorageFlags keyStorageFlags) {}

		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context) {}

		public virtual void Reset() {}

		// properties

		public IntPtr Handle {
			get { return (IntPtr) 0; }
		}
#endif
	}
}