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
using System.Security.Cryptography;

using Mono.Security.Cryptography;

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
	internal class TlsClientFinished : TlsHandshakeMessage
	{
		#region CONSTRUCTORS

		public TlsClientFinished(TlsSession session) 
			: base(session, TlsHandshakeType.Finished,  TlsContentType.Handshake)
		{
		}

		#endregion

		#region METHODS

		public override void UpdateSession()
		{
			base.UpdateSession();
			this.Reset();
		}

		#endregion

		#region PROTECTED_METHODS

		protected override void ProcessAsSsl3()
		{
			// Compute handshake messages hashes
			HashAlgorithm hash = new TlsSslHandshakeHash(this.Session.Context.MasterSecret);

			TlsStream data = new TlsStream();
			data.Write(this.Session.Context.HandshakeMessages.ToArray());
			data.Write((int)0x434C4E54);
			
			hash.TransformFinalBlock(data.ToArray(), 0, (int)data.Length);

			this.Write(hash.Hash);

			data.Reset();
		}

		protected override void ProcessAsTls1()
		{
			// Compute handshake messages hash
			HashAlgorithm hash = new MD5SHA1CryptoServiceProvider();
			hash.ComputeHash(
				Session.Context.HandshakeMessages.ToArray(),
				0,
				(int)Session.Context.HandshakeMessages.Length);

			// Write message
			Write(Session.Context.Cipher.PRF(Session.Context.MasterSecret, "client finished", hash.Hash, 12));
		}

		#endregion
	}
}
