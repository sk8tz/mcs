//
// System.Security.Cryptography.ToBase64Transform
//
// Author:
//   Sergey Chaban (serge@wildwestsoftware.com)
//
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Globalization;

namespace System.Security.Cryptography {

	public class ToBase64Transform : ICryptoTransform {

		private bool m_disposed;

		public ToBase64Transform ()
		{
		}

		/* Right now we have nothing to dispose to finalizer isn't required
		~ToBase64Transform () 
		{
			Dispose (false);
		}*/

		public bool CanTransformMultipleBlocks {
			get { return false; }
		}

		public virtual bool CanReuseTransform {
			get { return true; }
		}

		public int InputBlockSize {
			get { return 3; }
		}

		public int OutputBlockSize {
			get { return 4; }
		}

		public void Clear() 
		{
			Dispose (true);
		}

		void IDisposable.Dispose () 
		{
			Dispose (true);
			GC.SuppressFinalize (this);  // Finalization is now unnecessary
		}

		protected virtual void Dispose (bool disposing) 
		{
			if (!m_disposed) {
				// dispose unmanaged objects
				if (disposing) {
					// dispose managed objects
				}
				m_disposed = true;
			}
		}

		// LAMESPEC: It's not clear from docs what should be happening 
		// here if inputCount > InputBlockSize. It just "Converts the 
		// specified region of the specified byte array" and that's all.
		public int TransformBlock (byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
		{
			if (m_disposed)
				throw new ObjectDisposedException ("TransformBlock");
			if (inputBuffer == null)
				throw new ArgumentNullException ("inputBuffer");
			if (outputBuffer == null)
				throw new ArgumentNullException ("outputBuffer");
// To match MS implementation
//			if (inputCount != this.InputBlockSize)
//				throw new CryptographicException (Locale.GetText ("Invalid input length"));

			byte[] lookup = Base64Constants.EncodeTable;

			int b1 = inputBuffer [inputOffset];
			int b2 = inputBuffer [inputOffset + 1];
			int b3 = inputBuffer [inputOffset + 2];

			outputBuffer [outputOffset] = lookup [b1 >> 2];
			outputBuffer [outputOffset+1] = lookup [((b1 << 4) & 0x30) | (b2 >> 4)];
			outputBuffer [outputOffset+2] = lookup [((b2 << 2) & 0x3c) | (b3 >> 6)];
			outputBuffer [outputOffset+3] = lookup [b3 & 0x3f];

			return this.OutputBlockSize;
		}

		public byte[] TransformFinalBlock (byte[] inputBuffer, int inputOffset, int inputCount)
		{
			if (m_disposed)
				throw new ObjectDisposedException ("TransformFinalBlock");
			if (inputBuffer == null)
				throw new ArgumentNullException ("inputBuffer");
			if (inputCount > this.InputBlockSize)
				throw new ArgumentOutOfRangeException (Locale.GetText ("Invalid input length"));
			
			return InternalTransformFinalBlock (inputBuffer, inputOffset, inputCount);
		}
		
		// Mono System.Convert depends on the ability to process multiple blocks		
		internal byte[] InternalTransformFinalBlock (byte[] inputBuffer, int inputOffset, int inputCount)
		{
			int blockLen = this.InputBlockSize;
			int outLen = this.OutputBlockSize;
			int fullBlocks = inputCount / blockLen;
			int tail = inputCount % blockLen;

			byte[] res = new byte [(inputCount != 0)
			                        ? ((inputCount + 2) / blockLen) * outLen
			                        : 0];

			int outputOffset = 0;

			for (int i = 0; i < fullBlocks; i++) {

				TransformBlock (inputBuffer, inputOffset,
				                blockLen, res, outputOffset);

				inputOffset += blockLen;
				outputOffset += outLen;
			}

			byte[] lookup = Base64Constants.EncodeTable;
			int b1,b2;

			// When fewer than 24 input bits are available
			// in an input group, zero bits are added
			// (on the right) to form an integral number of
			// 6-bit groups.
			switch (tail) {
			case 0:
				break;
			case 1:
				b1 = inputBuffer [inputOffset];
				res [outputOffset] = lookup [b1 >> 2];
				res [outputOffset+1] = lookup [(b1 << 4) & 0x30];

				// padding
				res [outputOffset+2] = (byte)'=';
				res [outputOffset+3] = (byte)'=';
				break;

			case 2:
				b1 = inputBuffer [inputOffset];
				b2 = inputBuffer [inputOffset + 1];
				res [outputOffset] = lookup [b1 >> 2];
				res [outputOffset+1] = lookup [((b1 << 4) & 0x30) | (b2 >> 4)];
				res [outputOffset+2] = lookup [(b2 << 2) & 0x3c];

				// one-byte padding
				res [outputOffset+3] = (byte)'=';
				break;
			}

			return res;
		}
	}
}
