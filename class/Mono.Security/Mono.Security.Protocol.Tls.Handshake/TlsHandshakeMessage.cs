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
// using Mono.Security.Protocol.Tls;

namespace Mono.Security.Protocol.Tls.Handshake
{
	internal abstract class TlsHandshakeMessage : TlsStream
	{
		#region Fields

		private TlsContext			context;
		private TlsHandshakeType	handshakeType;
		private TlsContentType		contentType;

		#endregion

		#region Properties

		public TlsContext Context
		{
			get { return this.context; }
		}

		public TlsHandshakeType HandshakeType
		{
			get { return this.handshakeType; }
		}

		public TlsContentType ContentType
		{
			get { return this.contentType; }
		}

		#endregion

		#region Constructors

		public TlsHandshakeMessage(
			TlsContext			context,
			TlsHandshakeType	handshakeType) 
			: this(context, handshakeType, TlsContentType.Handshake)
		{
		}

		public TlsHandshakeMessage(
			TlsContext			context,
			TlsHandshakeType	handshakeType,
			TlsContentType		contentType) : base()
		{
			this.context		= context;
			this.handshakeType	= handshakeType;
			this.contentType	= contentType;

			// Process message
			this.process();
		}

		public TlsHandshakeMessage(
			TlsContext			context, 
			TlsHandshakeType	handshakeType, 
			byte[]				data) : base(data)
		{
			this.context		= context;
			this.handshakeType	= handshakeType;
						
			// Process message
			this.process();
		}

		#endregion

		#region Abstract Methods

		protected abstract void ProcessAsTls1();

		protected abstract void ProcessAsSsl3();

		#endregion

		#region Methods

		private void process()
		{
			switch (this.Context.Protocol)
			{
				case SecurityProtocolType.Ssl3:
					this.ProcessAsSsl3();
					break;

				case SecurityProtocolType.Tls:
					this.ProcessAsTls1();
					break;
			}
		}

		public virtual void Update()
		{			
			if (CanWrite)
			{
				this.context.HandshakeMessages.Write(this.EncodeMessage());
				this.Reset();
			}
		}

		public virtual byte[] EncodeMessage()
		{
			byte[] result = null;

			if (CanWrite)
			{
				TlsStream c = new TlsStream();

				c.Write((byte)HandshakeType);
				c.WriteInt24((int)this.Length);
				c.Write(this.ToArray());

				result = c.ToArray();
			}

			return result;
		}

		#endregion
	}
}
