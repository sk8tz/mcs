/* Transport Security Layer (TLS)
 * Copyright (c) 2003 Carlos Guzm�n �lvarez
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

namespace Mono.Security.Protocol.Tls
{
	internal class TlsCipherSuiteFactory
	{
		public static TlsCipherSuiteCollection GetSupportedCiphers(TlsProtocol protocol)
		{
			switch (protocol)
			{
				case TlsProtocol.Tls1:
					return TlsCipherSuiteFactory.GetTls1SupportedCiphers();

				case TlsProtocol.Ssl3:
					return TlsCipherSuiteFactory.GetSsl3SupportedCiphers();

				default:
					throw new NotSupportedException();
			}
		}

		#region PRIVATE_STATIC_METHODS

		private static TlsCipherSuiteCollection GetTls1SupportedCiphers()
		{
			TlsCipherSuiteCollection scs = new TlsCipherSuiteCollection(TlsProtocol.Tls1);

			// Supported ciphers
			// scs.Add((0x00 << 0x08) | 0x06, "TLS_RSA_EXPORT_WITH_RC2_CBC_40_MD5", "RC2", "MD5", true, true, 5, 16, 40, 8, 8);
			scs.Add((0x00 << 0x08) | 0x35, "TLS_RSA_WITH_AES_256_CBC_SHA", "Rijndael", "SHA1", false, true, 32, 32, 256, 16, 16);
			scs.Add((0x00 << 0x08) | 0x2F, "TLS_RSA_WITH_AES_128_CBC_SHA", "Rijndael", "SHA1", false, true, 16, 16, 128, 16, 16);
			scs.Add((0x00 << 0x08) | 0x0A, "TLS_RSA_WITH_3DES_EDE_CBC_SHA", "3DES", "SHA1", false, true, 24, 24, 168, 8, 8);
			scs.Add((0x00 << 0x08) | 0x09, "TLS_RSA_WITH_DES_CBC_SHA", "DES", "SHA1", false, true, 8, 8, 56, 8, 8);
			scs.Add((0x00 << 0x08) | 0x05, "TLS_RSA_WITH_RC4_128_SHA", "RC4", "SHA1", false, false, 16, 16, 128, 0, 0);
			scs.Add((0x00 << 0x08) | 0x04, "TLS_RSA_WITH_RC4_128_MD5", "RC4", "MD5", false, false, 16, 16, 128, 0, 0);			
			
			// Default CipherSuite
			// scs.Add(0, "TLS_NULL_WITH_NULL_NULL", "", "", true, false, 0, 0, 0, 0, 0);
			
			// RSA Cipher Suites
			// scs.Add((0x00 << 0x08) | 0x01, "TLS_RSA_WITH_NULL_MD5", "", "MD5", true, false, 0, 0, 0, 0, 0);
			// scs.Add((0x00 << 0x08) | 0x02, "TLS_RSA_WITH_NULL_SHA", "", "SHA1", true, false, 0, 0, 0, 0, 0);
			// scs.Add((0x00 << 0x08) | 0x03, "TLS_RSA_EXPORT_WITH_RC4_40_MD5", "RC4", "MD5", true, false, 5, 16, 40, 0, 0);
			// scs.Add((0x00 << 0x08) | 0x05, "TLS_RSA_WITH_RC4_128_SHA", "RC4", "SHA1", false, false, 16, 16, 128, 0, 0);
			// scs.Add((0x00 << 0x08) | 0x04, "TLS_RSA_WITH_RC4_128_MD5", "RC4", "MD5", false, false, 16, 16, 128, 0, 0);			
			// scs.Add((0x00 << 0x08) | 0x06, "TLS_RSA_EXPORT_WITH_RC2_CBC_40_MD5", "RC2", "MD5", true, true, 5, 16, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x07, "TLS_RSA_WITH_IDEA_CBC_SHA", "IDEA", "SHA1", false, true, 16, 16, 128, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x08, "TLS_RSA_EXPORT_WITH_DES40_CBC_SHA", "DES", "SHA1", true, true, 5, 8, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x09, "TLS_RSA_WITH_DES_CBC_SHA", "DES", "SHA1", false, true, 8, 8, 56, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x0A, "TLS_RSA_WITH_3DES_EDE_CBC_SHA", "3DES", "SHA1", false, true, 24, 24, 168, 8, 8);
			
			// Diffie-Hellman Cipher Suites
			// scs.Add((0x00 << 0x08) | 0x0B, "TLS_DH_DSS_EXPORT_WITH_DES40_CBC_SHA", "DES", "SHA1", true, true, 5, 8, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x0C, "TLS_DH_DSS_WITH_DES_CBC_SHA", "DES", "SHA1", false, true, 8, 8, 56, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x0D, "TLS_DH_DSS_WITH_3DES_EDE_CBC_SHA", "3DES", "SHA1", false, true, 24, 24, 168, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x0E, "TLS_DH_RSA_EXPORT_WITH_DES40_CBC_SHA", "DES", "SHA1", true, true, 5, 8, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x0F, "TLS_DH_RSA_WITH_DES_CBC_SHA", "DES", "SHA1", false, true, 8, 8, 56, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x10, "TLS_DH_RSA_WITH_3DES_EDE_CBC_SHA", "3DES", "SHA1", false, true, 24, 24, 168, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x11, "TLS_DHE_DSS_EXPORT_WITH_DES40_CBC_SHA", "DES", "SHA1", true, true, 5, 8, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x12, "TLS_DHE_DSS_WITH_DES_CBC_SHA", "DES", "SHA1", false, true, 8, 8, 56, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x13, "TLS_DHE_DSS_WITH_3DES_EDE_CBC_SHA", "3DES", "SHA1", false, true, 24, 24, 168, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x14, "TLS_DHE_RSA_EXPORT_WITH_DES40_CBC_SHA", "DES", "SHA1", true, true, 5, 8, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x15, "TLS_DHE_RSA_WITH_DES_CBC_SHA", "SHA1", "DES", false, true, 8, 8, 56, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x16, "TLS_DHE_RSA_WITH_3DES_EDE_CBC_SHA", "3DES", "SHA1", false, true, 24, 24, 168, 8, 8);

			// Anonymous Diffie-Hellman Cipher Suites
			// scs.Add((0x00 << 0x08) | 0x17, "TLS_DH_anon_EXPORT_WITH_RC4_40_MD5", "RC4", "MD5", true, false, 5, 16, 40, 0, 0);
			// scs.Add((0x00 << 0x08) | 0x18, "TLS_DH_anon_WITH_RC4_128_MD5", "RC4", "MD5", false, false, 16, 16, 128, 0, 0);
			// scs.Add((0x00 << 0x08) | 0x19, "TLS_DH_anon_EXPORT_WITH_DES40_CBC_SHA", "DES", "SHA1", false, true, 5, 8, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x1A, "TLS_DH_anon_WITH_DES_CBC_SHA", "DES4", "SHA1", false, true, 8, 8, 56, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x1B, "TLS_DH_anon_WITH_3DES_EDE_CBC_SHA", "3DES", "SHA1", false, true, 24, 24, 168, 8, 8);

			// AES CipherSuites
			//
			// Ref: RFC3268 - (http://www.ietf.org/rfc/rfc3268.txt)
		
			// scs.Add((0x00 << 0x08) | 0x2F, "TLS_RSA_WITH_AES_128_CBC_SHA", "Rijndael", "SHA1", false, true, 16, 16, 128, 16, 16);
			// scs.Add((0x00 << 0x08) | 0x30, "TLS_DH_DSS_WITH_AES_128_CBC_SHA", "Rijndael", "SHA1", false, true, 16, 16, 128, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x31, "TLS_DH_RSA_WITH_AES_128_CBC_SHA", "Rijndael", "SHA1", false, true, 16, 16, 128, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x32, "TLS_DHE_DSS_WITH_AES_128_CBC_SHA", "Rijndael", "SHA1", false, true, 16, 16, 128, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x33, "TLS_DHE_RSA_WITH_AES_128_CBC_SHA", "Rijndael", "SHA1", false, true, 16, 16, 128, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x34, "TLS_DH_anon_WITH_AES_128_CBC_SHA", "Rijndael", "SHA1", false, true, 16, 16, 128, 8, 8);

			// scs.Add((0x00 << 0x08) | 0x35, "TLS_RSA_WITH_AES_256_CBC_SHA", "Rijndael", "SHA1", false, true, 32, 32, 256, 16, 16);
			// scs.Add((0x00 << 0x08) | 0x36, "TLS_DH_DSS_WITH_AES_256_CBC_SHA", "Rijndael", "SHA1", false, true, 32, 32, 256, 16, 16);
			// scs.Add((0x00 << 0x08) | 0x37, "TLS_DH_RSA_WITH_AES_256_CBC_SHA", "Rijndael", "SHA1", false, true, 32, 32, 256, 16, 16);
			// scs.Add((0x00 << 0x08) | 0x38, "TLS_DHE_DSS_WITH_AES_256_CBC_SHA", "Rijndael", "SHA1", false, true, 32, 32, 256, 16, 16);
			// scs.Add((0x00 << 0x08) | 0x39, "TLS_DHE_RSA_WITH_AES_256_CBC_SHA", "Rijndael", "SHA1", false, true, 32, 32, 256, 16, 16);
			// scs.Add((0x00 << 0x08) | 0x3A, "TLS_DH_anon_WITH_AES_256_CBC_SHA", "Rijndael", "SHA1", false, true, 32, 32, 256, 16, 16);

			return scs;
		}

		private static TlsCipherSuiteCollection GetSsl3SupportedCiphers()
		{
			TlsCipherSuiteCollection scs = new TlsCipherSuiteCollection(TlsProtocol.Ssl3);

			// Supported ciphers
			scs.Add((0x00 << 0x08) | 0x0A, "SSL_RSA_WITH_3DES_EDE_CBC_SHA", "3DES", "SHA1", false, true, 24, 24, 168, 8, 8);
			scs.Add((0x00 << 0x08) | 0x09, "SSL_RSA_WITH_DES_CBC_SHA", "DES", "SHA1", false, true, 8, 8, 56, 8, 8);
			scs.Add((0x00 << 0x08) | 0x05, "SSL_RSA_WITH_RC4_128_SHA", "RC4", "SHA1", false, false, 16, 16, 128, 0, 0);
			scs.Add((0x00 << 0x08) | 0x04, "SSL_RSA_WITH_RC4_128_MD5", "RC4", "MD5", false, false, 16, 16, 128, 0, 0);			
			
			// Default CipherSuite
			// scs.Add(0, "SSL_NULL_WITH_NULL_NULL", "", "", true, false, 0, 0, 0, 0, 0);
			
			// RSA Cipher Suites
			// scs.Add((0x00 << 0x08) | 0x01, "SSL_RSA_WITH_NULL_MD5", "", "MD5", true, false, 0, 0, 0, 0, 0);
			// scs.Add((0x00 << 0x08) | 0x02, "SSL_RSA_WITH_NULL_SHA", "", "SHA1", true, false, 0, 0, 0, 0, 0);
			// scs.Add((0x00 << 0x08) | 0x03, "SSL_RSA_EXPORT_WITH_RC4_40_MD5", "RC4", "MD5", true, false, 5, 16, 40, 0, 0);
			// scs.Add((0x00 << 0x08) | 0x05, "SSL_RSA_WITH_RC4_128_SHA", "RC4", "SHA1", false, false, 16, 16, 128, 0, 0);
			// scs.Add((0x00 << 0x08) | 0x04, "SSL_RSA_WITH_RC4_128_MD5", "RC4", "MD5", false, false, 16, 16, 128, 0, 0);			
			// scs.Add((0x00 << 0x08) | 0x06, "SSL_RSA_EXPORT_WITH_RC2_CBC_40_MD5", "RC2", "MD5", true, true, 5, 16, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x07, "SSL_RSA_WITH_IDEA_CBC_SHA", "IDEA", "SHA1", false, true, 16, 16, 128, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x08, "SSL_RSA_EXPORT_WITH_DES40_CBC_SHA", "DES", "SHA1", true, true, 5, 8, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x09, "SSL_RSA_WITH_DES_CBC_SHA", "DES", "SHA1", false, true, 8, 8, 56, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x0A, "SSL_RSA_WITH_3DES_EDE_CBC_SHA", "3DES", "SHA1", false, true, 24, 24, 168, 8, 8);
			
			// Diffie-Hellman Cipher Suites
			// scs.Add((0x00 << 0x08) | 0x0B, "SSL_DH_DSS_EXPORT_WITH_DES40_CBC_SHA", "DES", "SHA1", true, true, 5, 8, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x0C, "SSL_DH_DSS_WITH_DES_CBC_SHA", "DES", "SHA1", false, true, 8, 8, 56, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x0D, "SSL_DH_DSS_WITH_3DES_EDE_CBC_SHA", "3DES", "SHA1", false, true, 24, 24, 168, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x0E, "SSL_DH_RSA_EXPORT_WITH_DES40_CBC_SHA", "DES", "SHA1", true, true, 5, 8, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x0F, "SSL_DH_RSA_WITH_DES_CBC_SHA", "DES", "SHA1", false, true, 8, 8, 56, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x10, "SSL_DH_RSA_WITH_3DES_EDE_CBC_SHA", "3DES", "SHA1", false, true, 24, 24, 168, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x11, "SSL_DHE_DSS_EXPORT_WITH_DES40_CBC_SHA", "DES", "SHA1", true, true, 5, 8, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x12, "SSL_DHE_DSS_WITH_DES_CBC_SHA", "DES", "SHA1", false, true, 8, 8, 56, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x13, "SSL_DHE_DSS_WITH_3DES_EDE_CBC_SHA", "3DES", "SHA1", false, true, 24, 24, 168, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x14, "SSL_DHE_RSA_EXPORT_WITH_DES40_CBC_SHA", "DES", "SHA1", true, true, 5, 8, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x15, "SSL_DHE_RSA_WITH_DES_CBC_SHA", "SHA1", "DES", false, true, 8, 8, 56, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x16, "SSL_DHE_RSA_WITH_3DES_EDE_CBC_SHA", "3DES", "SHA1", false, true, 24, 24, 168, 8, 8);

			// Anonymous Diffie-Hellman Cipher Suites
			// scs.Add((0x00 << 0x08) | 0x17, "SSL_DH_anon_EXPORT_WITH_RC4_40_MD5", "RC4", "MD5", true, false, 5, 16, 40, 0, 0);
			// scs.Add((0x00 << 0x08) | 0x18, "SSL_DH_anon_WITH_RC4_128_MD5", "RC4", "MD5", false, false, 16, 16, 128, 0, 0);
			// scs.Add((0x00 << 0x08) | 0x19, "SSL_DH_anon_EXPORT_WITH_DES40_CBC_SHA", "DES", "SHA1", false, true, 5, 8, 40, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x1A, "SSL_DH_anon_WITH_DES_CBC_SHA", "DES4", "SHA1", false, true, 8, 8, 56, 8, 8);
			// scs.Add((0x00 << 0x08) | 0x1B, "SSL_DH_anon_WITH_3DES_EDE_CBC_SHA", "3DES", "SHA1", false, true, 24, 24, 168, 8, 8);

			return scs;
		}

		#endregion
	}
}
