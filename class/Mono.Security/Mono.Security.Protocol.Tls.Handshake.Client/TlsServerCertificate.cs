/* Transport Security Layer (TLS)
 * Copyright (c) 2003-2004 Carlos Guzman Alvarez
 * Sebastien Pouliot, Copyright (c) 2004 Novell (http://www.novell.com)
 * 
 * Permission is hereby granted, free of charge, to any person 
 * obtaining a copy of this software and associated documentation 
 * files (the "Software"), to deal in the Software without restriction, 
 * including without limitation the rights to use, copy, modify, merge, 
 * publish, distribute, sublicense, and/or sell copies of the Software, 
 * and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included 
 * in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, 
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND 
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Net;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using X509Cert = System.Security.Cryptography.X509Certificates;

using Mono.Security.X509;
using Mono.Security.X509.Extensions;

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
	internal class TlsServerCertificate : HandshakeMessage
	{
		#region Fields

		private X509CertificateCollection certificates;
		
		#endregion

		#region Constructors

		public TlsServerCertificate(Context context, byte[] buffer) 
			: base(context, HandshakeType.Certificate, buffer)
		{
		}

		#endregion

		#region Methods

		public override void Update()
		{
			base.Update();
			this.Context.ServerSettings.Certificates = this.certificates;
			this.Context.ServerSettings.UpdateCertificateRSA();
		}

		#endregion

		#region Protected Methods

		protected override void ProcessAsSsl3()
		{
			this.ProcessAsTls1();
		}

		protected override void ProcessAsTls1()
		{
			this.certificates = new X509CertificateCollection();
			
			int readed	= 0;
			int length	= this.ReadInt24();

			while (readed < length)
			{
				// Read certificate length
				int certLength = ReadInt24();

				// Increment readed
				readed += 3;

				if (certLength > 0)
				{
					// Read certificate data
					byte[] buffer = this.ReadBytes(certLength);

					// Create a new X509 Certificate
					X509Certificate certificate = new X509Certificate(buffer);
					certificates.Add(certificate);

					readed += certLength;
				}
			}

			this.validateCertificates(certificates);
		}

		#endregion

		#region Private Methods

		// Note: this method only works for RSA certificates
		// DH certificates requires some changes - does anyone use one ?
		private bool checkCertificateUsage (X509Certificate cert) 
		{
			ClientContext context = (ClientContext)this.Context;

			// certificate extensions are required for this
			// we "must" accept older certificates without proofs
			if (cert.Version < 3)
				return true;

			KeyUsage ku = KeyUsage.none;
			switch (context.Cipher.ExchangeAlgorithmType) 
			{
				case ExchangeAlgorithmType.RsaSign:
					ku = KeyUsage.digitalSignature;
					break;
				case ExchangeAlgorithmType.RsaKeyX:
					ku = KeyUsage.keyEncipherment;
					break;
				case ExchangeAlgorithmType.DiffieHellman:
					ku = KeyUsage.keyAgreement;
					break;
				case ExchangeAlgorithmType.Fortezza:
					return false; // unsupported certificate type
			}

			KeyUsageExtension kux = null;
			ExtendedKeyUsageExtension eku = null;

			X509Extension xtn = cert.Extensions ["2.5.29.15"];
			if (xtn != null)
				kux = new KeyUsageExtension (xtn);

			xtn = cert.Extensions ["2.5.29.37"];
			if (xtn != null)
				eku = new ExtendedKeyUsageExtension (xtn);

			if ((kux != null) && (eku != null)) 
			{
				// RFC3280 states that when both KeyUsageExtension and 
				// ExtendedKeyUsageExtension are present then BOTH should
				// be valid
				return (kux.Support (ku) &&
					eku.KeyPurpose.Contains ("1.3.6.1.5.5.7.3.1"));
			}
			else if (kux != null) 
			{
				return kux.Support (ku);
			}
			else if (eku != null) 
			{
				// Server Authentication (1.3.6.1.5.5.7.3.1)
				return eku.KeyPurpose.Contains ("1.3.6.1.5.5.7.3.1");
			}

			// last chance - try with older (deprecated) Netscape extensions
			xtn = cert.Extensions ["2.16.840.1.113730.1.1"];
			if (xtn != null) 
			{
				NetscapeCertTypeExtension ct = new NetscapeCertTypeExtension (xtn);
				return ct.Support (NetscapeCertTypeExtension.CertType.SslServer);
			}

			// certificate isn't valid for SSL server usage
			return false;
		}

		private void validateCertificates(X509CertificateCollection certificates)
		{
			ClientContext context = (ClientContext)this.Context;

			// the leaf is the web server certificate
			X509Certificate leaf = certificates [0];
			X509Cert.X509Certificate cert = new X509Cert.X509Certificate (leaf.RawData);

			ArrayList errors = new ArrayList();

			// SSL specific check - not all certificates can be 
			// used to server-side SSL some rules applies after 
			// all ;-)
			if (!checkCertificateUsage (leaf)) 
			{
				// WinError.h CERT_E_PURPOSE 0x800B0106
				errors.Add ((int)-2146762490);
			}

			// SSL specific check - does the certificate match 
			// the host ?
			if (!checkServerIdentity (leaf))
			{
				// WinError.h CERT_E_CN_NO_MATCH 0x800B010F
				errors.Add ((int)-2146762481);
			}

			// Note: building and verifying a chain can take much time
			// so we do it last (letting simple things fails first)

			// Note: In TLS the certificates MUST be in order (and
			// optionally include the root certificate) so we're not
			// building the chain using LoadCertificate (it's faster)

			// Note: IIS doesn't seem to send the whole certificate chain
			// but only the server certificate :-( it's assuming that you
			// already have this chain installed on your computer. duh!
			// http://groups.google.ca/groups?q=IIS+server+certificate+chain&hl=en&lr=&ie=UTF-8&oe=UTF-8&selm=85058s%24avd%241%40nnrp1.deja.com&rnum=3

			// we must remove the leaf certificate from the chain
			X509CertificateCollection chain = new X509CertificateCollection (certificates);
			chain.Remove (leaf);
			X509Chain verify = new X509Chain (chain);
			if (!verify.Build (leaf)) 
			{
				switch (verify.Status) 
				{
					case X509ChainStatusFlags.InvalidBasicConstraints:
						// WinError.h TRUST_E_BASIC_CONSTRAINTS 0x80096019
						errors.Add ((int)-2146869223);
						break;
					case X509ChainStatusFlags.NotSignatureValid:
						// WinError.h TRUST_E_BAD_DIGEST 0x80096010
						errors.Add ((int)-2146869232);
						break;
					case X509ChainStatusFlags.NotTimeNested:
						// WinError.h CERT_E_VALIDITYPERIODNESTING 0x800B0102
						errors.Add ((int)-2146762494);
						break;
					case X509ChainStatusFlags.NotTimeValid:
						// WinError.h CERT_E_EXPIRED 0x800B0101
						errors.Add ((int)-2146762495);
						break;
					case X509ChainStatusFlags.PartialChain:
						// WinError.h CERT_E_CHAINING 0x800B010A
						errors.Add ((int)-2146762486);
						break;
					case X509ChainStatusFlags.UntrustedRoot:
						// WinError.h CERT_E_UNTRUSTEDROOT 0x800B0109
						errors.Add ((int)-2146762487);
						break;
					default:
						// unknown error
						errors.Add ((int)verify.Status);
						break;
				}
			}

			int[] certificateErrors = (int[])errors.ToArray(typeof(int));

			if (!context.SslStream.RaiseServerCertificateValidation(
				cert, 
				certificateErrors))
			{
				throw context.CreateException("Invalid certificate received form server.");
			}
		}

		// RFC2818 - HTTP Over TLS, Section 3.1
		// http://www.ietf.org/rfc/rfc2818.txt
		// 
		// 1.	if present MUST use subjectAltName dNSName as identity
		// 1.1.		if multiples entries a match of any one is acceptable
		// 1.2.		wildcard * is acceptable
		// 2.	URI may be an IP address -> subjectAltName.iPAddress
		// 2.1.		exact match is required
		// 3.	Use of the most specific Common Name (CN=) in the Subject
		// 3.1		Existing practice but DEPRECATED
		private bool checkServerIdentity (X509Certificate cert) 
		{
			ClientContext context = (ClientContext)this.Context;

			string targetHost = context.ClientSettings.TargetHost;

			X509Extension ext = cert.Extensions ["2.5.29.17"];
			// 1. subjectAltName
			if (ext != null) 
			{
				SubjectAltNameExtension subjectAltName = new SubjectAltNameExtension (ext);
				// 1.1 - multiple dNSName
				foreach (string dns in subjectAltName.DNSNames) 
				{
					// 1.2 TODO - wildcard support
					if (dns == targetHost)
						return true;
				}
				// 2. ipAddress
				foreach (string ip in subjectAltName.IPAddresses) 
				{
					// 2.1. Exact match required
					if (ip == targetHost)
						return true;
				}
			}
			// 3. Common Name (CN=)
			return checkDomainName (cert.SubjectName);
		}

		private bool checkDomainName(string subjectName)
		{
			ClientContext context = (ClientContext)this.Context;

			string	domainName = String.Empty;
			Regex search = new Regex(@"([\w\s\d]*)\s*=\s*([^,]*)");

			MatchCollection	elements = search.Matches(subjectName);

			foreach (Match element in elements)
			{
				switch (element.Groups[1].Value.Trim().ToUpper())
				{
					case "CN":
						domainName = element.Groups[2].Value;
						break;
				}
			}

			// TODO: add wildcard * support
			return (String.Compare (context.ClientSettings.TargetHost, domainName, true, CultureInfo.InvariantCulture) == 0);

			/*
			 * the only document found describing this is:
			 * http://www.geocities.com/SiliconValley/Byte/4170/articulos/tls/autentic.htm#Autenticaci%F3n%20del%20Server
			 * however I don't see how this could deal with wildcards ?
			 * other issues
			 * a. there could also be many address returned
			 * b. Address property is obsoleted in .NET 1.1
			 * 
						if (domainName == String.Empty)
						{
							return false;
						}
						else
						{
							string targetHost = context.ClientSettings.TargetHost;

							// Check that the IP is correct
							try
							{
								IPAddress	ipHost		= Dns.Resolve(targetHost).AddressList[0];
								IPAddress	ipDomain	= Dns.Resolve(domainName).AddressList[0];

								// Note: Address is obsolete in 1.1
								return (ipHost.Address == ipDomain.Address);
							}
							catch (Exception)
							{
								return false;
							}
						}*/
		}

		#endregion
	}
}
