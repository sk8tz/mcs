/* Transport Security Layer (TLS)
 * Copyright (c) 2003-2004 Carlos Guzman Alvarez
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

namespace Mono.Security.Protocol.Tls.Handshake.Client
{
	internal class TlsServerHello : TlsHandshakeMessage
	{
		#region Fields

		private SecurityProtocolType	protocol;
		private SecurityCompressionType	compressionMethod;
		private byte[]					random;
		private byte[]					sessionId;
		private CipherSuite	cipherSuite;
		
		#endregion

		#region Constructors

		public TlsServerHello(TlsContext context, byte[] buffer) 
			: base(context, TlsHandshakeType.ServerHello, buffer)
		{
		}

		#endregion

		#region Methods

		public override void Update()
		{
			base.Update();

			this.Context.SessionId			= this.sessionId;
			this.Context.ServerRandom		= this.random;
			this.Context.Cipher				= this.cipherSuite;
			this.Context.CompressionMethod	= this.compressionMethod;
			this.Context.Cipher.Context		= this.Context;

			// Compute ClientRandom + ServerRandom
			TlsStream random = new TlsStream();
			random.Write(this.Context.ClientRandom);
			random.Write(this.Context.ServerRandom);
			this.Context.RandomCS = random.ToArray();

			// Server Random + Client Random
			random.Reset();
			random.Write(this.Context.ServerRandom);
			random.Write(this.Context.ClientRandom);

			this.Context.RandomSC = random.ToArray();
			random.Reset();
		}

		#endregion

		#region Protected Methods

		protected override void ProcessAsSsl3()
		{
			// Read protocol version
			this.protocol	= (SecurityProtocolType)this.ReadInt16();
			
			// Read random  - Unix time + Random bytes
			this.random		= this.ReadBytes(32);
			
			// Read Session id
			int length = (int)ReadByte();
			if (length > 0)
			{
				this.sessionId = this.ReadBytes(length);
			}

			// Read cipher suite
			short cipherCode = this.ReadInt16();
			if (this.Context.SupportedCiphers.IndexOf(cipherCode) == -1)
			{
				// The server has sent an invalid ciphersuite
				throw new TlsException("Invalid cipher suite received from server");
			}
			this.cipherSuite = this.Context.SupportedCiphers[cipherCode];
			
			// Read compression methods ( always 0 )
			this.compressionMethod = (SecurityCompressionType)this.ReadByte();
		}

		protected override void ProcessAsTls1()
		{
			// Read protocol version
			this.protocol	= (SecurityProtocolType)this.ReadInt16();
			
			// Read random  - Unix time + Random bytes
			this.random		= this.ReadBytes(32);
			
			// Read Session id
			int length = (int)ReadByte();
			if (length > 0)
			{
				this.sessionId = this.ReadBytes(length);
			}

			// Read cipher suite
			short cipherCode = this.ReadInt16();
			if (this.Context.SupportedCiphers.IndexOf(cipherCode) == -1)
			{
				// The server has sent an invalid ciphersuite
				throw new TlsException("Invalid cipher suite received from server");
			}
			this.cipherSuite = this.Context.SupportedCiphers[cipherCode];
			
			// Read compression methods ( always 0 )
			this.compressionMethod = (SecurityCompressionType)this.ReadByte();
		}

		#endregion
	}
}