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
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Mono.Security;
using Mono.Security.Cryptography;

namespace Mono.Security.Protocol.Tls
{
	internal class TlsCipherSuite : CipherSuite
	{
		#region CONSTRUCTORS
		
		public TlsCipherSuite(
			short code, string name, CipherAlgorithmType cipherAlgorithmType, 
			HashAlgorithmType hashAlgorithmType, ExchangeAlgorithmType exchangeAlgorithmType,
			bool exportable, bool blockMode, byte keyMaterialSize, 
			byte expandedKeyMaterialSize, short effectiveKeyBytes, 
			byte ivSize, byte blockSize) :
			base(code, name, cipherAlgorithmType, hashAlgorithmType, 
			exchangeAlgorithmType, exportable, blockMode, keyMaterialSize, 
			expandedKeyMaterialSize, effectiveKeyBytes, ivSize, blockSize)
		{
		}

		#endregion

		#region MAC_GENERATION_METHOD

		public override byte[] ComputeServerRecordMAC(TlsContentType contentType, byte[] fragment)
		{
			TlsStream	data	= new TlsStream();
			byte[]		result	= null;

			data.Write(this.Context.ReadSequenceNumber);
			data.Write((byte)contentType);
			data.Write((short)this.Context.Protocol);
			data.Write((short)fragment.Length);
			data.Write(fragment);

			result = this.ServerHMAC.ComputeHash(data.ToArray());

			data.Reset();

			return result;
		}

		public override byte[] ComputeClientRecordMAC(TlsContentType contentType, byte[] fragment)
		{
			TlsStream	data	= new TlsStream();
			byte[]		result	= null;

			data.Write(this.Context.WriteSequenceNumber);
			data.Write((byte)contentType);
			data.Write((short)this.Context.Protocol);
			data.Write((short)fragment.Length);
			data.Write(fragment);

			result = this.ClientHMAC.ComputeHash(data.ToArray());

			data.Reset();

			return result;
		}

		#endregion

		#region KEY_GENERATION_METODS

		public override void ComputeMasterSecret(byte[] preMasterSecret)
		{
			// Create master secret
			this.Context.MasterSecret = new byte[preMasterSecret.Length];
			this.Context.MasterSecret = this.PRF(
				preMasterSecret, "master secret", this.Context.RandomCS, 48);
		}

		public override void ComputeKeys()
		{
			// Create keyblock
			TlsStream keyBlock = new TlsStream(
				this.PRF(
				this.Context.MasterSecret, 
				"key expansion",
				this.Context.RandomSC,
				this.KeyBlockSize));

			this.Context.ClientWriteMAC = keyBlock.ReadBytes(this.HashSize);
			this.Context.ServerWriteMAC = keyBlock.ReadBytes(this.HashSize);
			this.Context.ClientWriteKey = keyBlock.ReadBytes(this.KeyMaterialSize);
			this.Context.ServerWriteKey = keyBlock.ReadBytes(this.KeyMaterialSize);

			if (!this.IsExportable)
			{
				if (this.IvSize != 0)
				{
					this.Context.ClientWriteIV = keyBlock.ReadBytes(this.IvSize);
					this.Context.ServerWriteIV = keyBlock.ReadBytes(this.IvSize);
				}
				else
				{
					this.Context.ClientWriteIV = new byte[0];
					this.Context.ServerWriteIV = new byte[0];
				}
			}
			else
			{
				// Generate final write keys
				byte[] finalClientWriteKey	= PRF(this.Context.ClientWriteKey, "client write key", this.Context.RandomCS, this.KeyMaterialSize);
				byte[] finalServerWriteKey	= PRF(this.Context.ServerWriteKey, "server write key", this.Context.RandomCS, this.KeyMaterialSize);
				
				this.Context.ClientWriteKey	= finalClientWriteKey;
				this.Context.ServerWriteKey	= finalServerWriteKey;

				// Generate IV block
				byte[] ivBlock = PRF(new byte[]{}, "IV block", this.Context.RandomCS, this.IvSize*2);

				// Generate IV keys
				this.Context.ClientWriteIV = new byte[this.IvSize];				
				System.Array.Copy(ivBlock, 0, this.Context.ClientWriteIV, 0, this.Context.ClientWriteIV.Length);

				this.Context.ServerWriteIV = new byte[this.IvSize];
				System.Array.Copy(ivBlock, this.IvSize, this.Context.ServerWriteIV, 0, this.Context.ServerWriteIV.Length);
			}

			// Clear no more needed data
			keyBlock.Reset();
		}

		#endregion
	}
}