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
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Mono.Security.Protocol.Tls.Alerts;
using Mono.Security.Protocol.Tls.Handshake;

namespace Mono.Security.Protocol.Tls
{
	#region Delegates

	public delegate bool CertificateValidationCallback(
		X509Certificate certificate, 
		int[]			certificateErrors);
	
	public delegate X509Certificate CertificateSelectionCallback(
		X509CertificateCollection	clientCertificates, 
		X509Certificate				serverCertificate, 
		string						targetHost, 
		X509CertificateCollection	serverRequestedCertificates);
	
	public delegate AsymmetricAlgorithm PrivateKeySelectionCallback(
		X509Certificate	clientCertificate, 
		string			targetHost);

	#endregion

	public class SslClientStream : Stream, IDisposable
	{
		#region Internal Events
		
		internal event CertificateValidationCallback	ServerCertValidation;
		internal event CertificateSelectionCallback		ClientCertSelection;
		internal event PrivateKeySelectionCallback		PrivateKeySelection;
		
		#endregion

		#region Fields

		private CertificateValidationCallback	serverCertValidationDelegate;
		private CertificateSelectionCallback	clientCertSelectionDelegate;
		private PrivateKeySelectionCallback		privateKeySelectionDelegate;
		private Stream							innerStream;
		private BufferedStream					inputBuffer;
		private ClientContext					context;
		private ClientRecordProtocol			protocol;
		private bool							ownsStream;
		private bool							disposed;
		private bool							checkCertRevocationStatus;
		private object							read;
		private object							write;

		#endregion

		#region Properties

		public override bool CanRead
		{
			get { return this.innerStream.CanRead; }
		}

		public override bool CanSeek
		{
			get { return false; }
		}

		public override bool CanWrite
		{
			get { return this.innerStream.CanWrite; }
		}

		public override long Length
		{
			get { throw new NotSupportedException(); }
		}

		public override long Position
		{
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		#endregion

		#region Security Properties

		public bool CheckCertRevocationStatus 
		{
			get { return this.checkCertRevocationStatus ; }
			set { this.checkCertRevocationStatus = value; }
		}

		public CipherAlgorithmType CipherAlgorithm 
		{
			get 
			{ 
				if (this.context.HandshakeState == HandshakeState.Finished)
				{
					return this.context.Cipher.CipherAlgorithmType;
				}

				return CipherAlgorithmType.None;
			}
		}
		
		public int CipherStrength 
		{
			get 
			{ 
				if (this.context.HandshakeState == HandshakeState.Finished)
				{
					return this.context.Cipher.EffectiveKeyBits;
				}

				return 0;
			}
		}
		
		public X509CertificateCollection ClientCertificates 
		{
			get { return this.context.ClientSettings.Certificates;}
		}
		
		public HashAlgorithmType HashAlgorithm 
		{
			get 
			{ 
				if (this.context.HandshakeState == HandshakeState.Finished)
				{
					return this.context.Cipher.HashAlgorithmType; 
				}

				return HashAlgorithmType.None;
			}
		}
		
		public int HashStrength
		{
			get 
			{ 
				if (this.context.HandshakeState == HandshakeState.Finished)
				{
					return this.context.Cipher.HashSize * 8; 
				}

				return 0;
			}
		}
		
		public int KeyExchangeStrength 
		{
			get 
			{ 
				if (this.context.HandshakeState == HandshakeState.Finished)
				{
					return this.context.ServerSettings.Certificates[0].RSA.KeySize;
				}

				return 0;
			}
		}
		
		public ExchangeAlgorithmType KeyExchangeAlgorithm 
		{
			get 
			{ 
				if (this.context.HandshakeState == HandshakeState.Finished)
				{
					return this.context.Cipher.ExchangeAlgorithmType; 
				}

				return ExchangeAlgorithmType.None;
			}
		}
		
		public SecurityProtocolType SecurityProtocol 
		{
			get 
			{ 
				if (this.context.HandshakeState == HandshakeState.Finished)
				{
					return this.context.SecurityProtocol; 
				}

				return 0;
			}
		}
		
		public X509Certificate SelectedClientCertificate 
		{
			get { return this.context.ClientSettings.ClientCertificate; }
		}

		public X509Certificate ServerCertificate 
		{
			get 
			{ 
				if (this.context.HandshakeState == HandshakeState.Finished)
				{
					if (this.context.ServerSettings.Certificates != null &&
						this.context.ServerSettings.Certificates.Count > 0)
					{
						return new X509Certificate(this.context.ServerSettings.Certificates[0].RawData);
					}
				}

				return null;
			}
		} 

		#endregion

		#region Callback Properties

		public CertificateValidationCallback ServerCertValidationDelegate
		{
			get { return this.serverCertValidationDelegate; }
			set 
			{ 
				if (this.ServerCertValidation != null)
				{
					this.ServerCertValidation -= this.serverCertValidationDelegate;
				}
				this.serverCertValidationDelegate	= value;
				this.ServerCertValidation			+= this.serverCertValidationDelegate;
			}
		}

		public CertificateSelectionCallback ClientCertSelectionDelegate 
		{
			get { return this.clientCertSelectionDelegate; }
			set 
			{ 
				if (this.ClientCertSelection != null)
				{
					this.ClientCertSelection -= this.clientCertSelectionDelegate;
				}
				this.clientCertSelectionDelegate	= value;
				this.ClientCertSelection			+= this.clientCertSelectionDelegate;
			}
		}

		public PrivateKeySelectionCallback PrivateKeyCertSelectionDelegate 
		{
			get { return this.privateKeySelectionDelegate; }
			set 
			{ 
				if (this.PrivateKeySelection != null)
				{
					this.PrivateKeySelection -= this.privateKeySelectionDelegate;
				}
				this.privateKeySelectionDelegate	= value;
				this.PrivateKeySelection			+= this.privateKeySelectionDelegate;
			}
		}

		#endregion

		#region Constructors
		
		public SslClientStream(
			Stream	stream, 
			string	targetHost, 
			bool	ownsStream) 
			: this(
				stream, targetHost, ownsStream, 
				SecurityProtocolType.Default, null)
		{
		}
		
		public SslClientStream(
			Stream				stream, 
			string				targetHost, 
			X509Certificate		clientCertificate) 
			: this(
				stream, targetHost, false, SecurityProtocolType.Default, 
				new X509CertificateCollection(new X509Certificate[]{clientCertificate}))
		{
		}

		public SslClientStream(
			Stream						stream,
			string						targetHost, 
			X509CertificateCollection clientCertificates) : 
			this(
				stream, targetHost, false, SecurityProtocolType.Default, 
				clientCertificates)
		{
		}

		public SslClientStream(
			Stream					stream,
			string					targetHost,
			bool					ownsStream,
			SecurityProtocolType	securityProtocolType) 
			: this(
				stream, targetHost, ownsStream, securityProtocolType,
				new X509CertificateCollection())
		{
		}

		public SslClientStream(
			Stream						stream,
			string						targetHost,
			bool						ownsStream,
			SecurityProtocolType		securityProtocolType,
			X509CertificateCollection	clientCertificates)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream is null.");
			}
			if (!stream.CanRead || !stream.CanWrite)
			{
				throw new ArgumentNullException("stream is not both readable and writable.");
			}
			if (targetHost == null || targetHost.Length == 0)
			{
				throw new ArgumentNullException("targetHost is null or an empty string.");
			}

			this.context = new ClientContext(
				this,
				securityProtocolType, 
				targetHost, 
				clientCertificates);

			this.inputBuffer	= new BufferedStream(new MemoryStream());
			this.innerStream	= stream;
			this.ownsStream		= ownsStream;
			this.read			= String.Empty;
			this.write			= String.Empty;
			this.protocol		= new ClientRecordProtocol(innerStream, context);
		}

		#endregion

		#region Finalizer

		~SslClientStream()
		{
			this.Dispose(false);
		}

		#endregion

        #region IDisposable Methods

		void IDisposable.Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				if (disposing)
				{
					if (this.innerStream != null)
					{
						if (this.context.HandshakeState == HandshakeState.Finished &&
							!this.context.ConnectionEnd)
						{
							// Write close notify							
							this.protocol.SendAlert(TlsAlertDescription.CloseNotify);
						}

						if (this.ownsStream)
						{
							// Close inner stream
							this.innerStream.Close();
						}
					}
					this.ownsStream		= false;
					this.innerStream	= null;
					if (this.ClientCertSelection != null)
					{
						this.ClientCertSelection -= this.clientCertSelectionDelegate;
					}
					if (this.ServerCertValidation != null)
					{
						this.ServerCertValidation -= this.serverCertValidationDelegate;
					}
					this.serverCertValidationDelegate	= null;
					this.clientCertSelectionDelegate	= null;
				}

				this.disposed = true;
			}
		}

		#endregion

		#region Methods

		public override IAsyncResult BeginRead(
			byte[]			buffer,
			int				offset,
			int				count,
			AsyncCallback	callback,
			object			state)
		{
			this.checkDisposed();
			
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer is a null reference.");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset is less than 0.");
			}
			if (offset > buffer.Length)
			{
				throw new ArgumentOutOfRangeException("offset is greater than the length of buffer.");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count is less than 0.");
			}
			if (count > (buffer.Length - offset))
			{
				throw new ArgumentOutOfRangeException("count is less than the length of buffer minus the value of the offset parameter.");
			}

			lock (this)
			{
				if (this.context.HandshakeState == HandshakeState.None)
				{
					this.NegotiateHandshake();
				}
			}

			IAsyncResult asyncResult;

			lock (this.read)
			{
				try
				{
					// If actual buffer is full readed reset it
					if (this.inputBuffer.Position == this.inputBuffer.Length &&
						this.inputBuffer.Length > 0)
					{
						this.resetBuffer();
					}

					if (!this.context.ConnectionEnd)
					{
						// Check if we have space in the middle buffer
						// if not Read next TLS record and update the inputBuffer
						while ((this.inputBuffer.Length - this.inputBuffer.Position) < count)
						{
							// Read next record and write it into the inputBuffer
							long	position	= this.inputBuffer.Position;					
							byte[]	record		= this.protocol.ReceiveRecord();
					
							if (record != null && record.Length > 0)
							{
								// Write new data to the inputBuffer
								this.inputBuffer.Seek(0, SeekOrigin.End);
								this.inputBuffer.Write(record, 0, record.Length);

								// Restore buffer position
								this.inputBuffer.Seek(position, SeekOrigin.Begin);
							}
							else
							{
								if (record == null)
								{
									break;
								}
							}

							// TODO: Review if we need to check the Length
							// property of the innerStream for other types
							// of streams, to check that there are data available
							// for read
							if (this.innerStream is NetworkStream &&
								!((NetworkStream)this.innerStream).DataAvailable)
							{
								break;
							}
						}
					}

					asyncResult = this.inputBuffer.BeginRead(
						buffer, offset, count, callback, state);
				}
				catch (TlsException)
				{
					throw new IOException("The authentication or decryption has failed.");
				}
				catch (Exception)
				{
					throw new IOException("IO exception during read.");
				}
			}

			return asyncResult;
		}

		public override IAsyncResult BeginWrite(
			byte[]			buffer,
			int				offset,
			int				count,
			AsyncCallback	callback,
			object			state)
		{
			this.checkDisposed();

			if (buffer == null)
			{
				throw new ArgumentNullException("buffer is a null reference.");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset is less than 0.");
			}
			if (offset > buffer.Length)
			{
				throw new ArgumentOutOfRangeException("offset is greater than the length of buffer.");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count is less than 0.");
			}
			if (count > (buffer.Length - offset))
			{
				throw new ArgumentOutOfRangeException("count is less than the length of buffer minus the value of the offset parameter.");
			}

			lock (this)
			{
				if (this.context.HandshakeState == HandshakeState.None)
				{
					this.NegotiateHandshake();
				}
			}

			IAsyncResult asyncResult;

			lock (this.write)
			{
				try
				{
					// Send the buffer as a TLS record
					
					byte[] record = this.protocol.EncodeRecord(
						TlsContentType.ApplicationData, buffer, offset, count);
				
					asyncResult = this.innerStream.BeginWrite(
						record, 0, record.Length, callback, state);
				}
				catch (TlsException)
				{
					throw new IOException("The authentication or decryption has failed.");
				}
				catch (Exception)
				{
					throw new IOException("IO exception during Write.");
				}
			}

			return asyncResult;
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			this.checkDisposed();

			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult is null or was not obtained by calling BeginRead.");
			}

			return this.inputBuffer.EndRead(asyncResult);
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			this.checkDisposed();

			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult is null or was not obtained by calling BeginRead.");
			}

			this.innerStream.EndWrite (asyncResult);
		}

		public override void Close()
		{
			((IDisposable)this).Dispose();
		}

		public override void Flush()
		{
			this.checkDisposed();

			this.innerStream.Flush();
		}

		public int Read(byte[] buffer)
		{
			return this.Read(buffer, 0, buffer.Length);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			IAsyncResult res = this.BeginRead(buffer, offset, count, null, null);

			return this.EndRead(res);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}
		
		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public void Write(byte[] buffer)
		{
			this.Write(buffer, 0, buffer.Length);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			IAsyncResult res = this.BeginWrite (buffer, offset, count, null, null);

			this.EndWrite(res);
		}

		#endregion

		#region Misc Methods

		private void resetBuffer()
		{
			this.inputBuffer.SetLength(0);
			this.inputBuffer.Position = 0;
		}

		private void checkDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException("The SslClientStream is closed.");
			}
		}

		#endregion

		#region Handsake Methods

		/*
			Client											Server

			ClientHello                 -------->
															ServerHello
															Certificate*
															ServerKeyExchange*
															CertificateRequest*
										<--------			ServerHelloDone
			Certificate*
			ClientKeyExchange
			CertificateVerify*
			[ChangeCipherSpec]
			Finished                    -------->
															[ChangeCipherSpec]
										<--------           Finished
			Application Data            <------->			Application Data

					Fig. 1 - Message flow for a full handshake		
		*/

		internal void NegotiateHandshake()
		{
			lock (this)
			{
				try
				{
					if (this.context.HandshakeState != HandshakeState.None)
					{
						this.context.Clear();
					}

					// Obtain supported cipher suites
					this.context.SupportedCiphers = TlsCipherSuiteFactory.GetSupportedCiphers(this.context.SecurityProtocol);

					// Send client hello
					this.protocol.SendRecord(TlsHandshakeType.ClientHello);

					// Read server response
					while (!this.context.HelloDone)
					{
						// Read next record
						this.protocol.ReceiveRecord();
					}

					// Send client certificate if requested
					if (this.context.ServerSettings.CertificateRequest)
					{
						this.protocol.SendRecord(TlsHandshakeType.Certificate);
					}

					// Send Client Key Exchange
					this.protocol.SendRecord(TlsHandshakeType.ClientKeyExchange);

					// Now initialize session cipher with the generated keys
					this.context.Cipher.InitializeCipher();

					// Send certificate verify if requested
					if (this.context.ServerSettings.CertificateRequest)
					{
						this.protocol.SendRecord(TlsHandshakeType.CertificateVerify);
					}

					// Send Cipher Spec protocol
					this.protocol.SendChangeCipherSpec();			
			
					// Read record until server finished is received
					while (this.context.HandshakeState != HandshakeState.Finished)
					{
						// If all goes well this will process messages:
						// 		Change Cipher Spec
						//		Server finished
						this.protocol.ReceiveRecord();
					}

					// Clear Key Info
					this.context.ClearKeyInfo();
				}
				catch
				{
					throw new IOException("The authentication or decryption has failed.");
				}
			}
		}

		#endregion

		#region Event Methods

		internal virtual bool RaiseServerCertificateValidation(
			X509Certificate certificate, 
			int[]			certificateErrors)
		{
			if (this.ServerCertValidation != null)
			{
				return this.ServerCertValidation(certificate, certificateErrors);
			}

			return (certificateErrors != null && certificateErrors.Length == 0);
		}

		internal X509Certificate RaiseClientCertificateSelection(
			X509CertificateCollection	clientCertificates, 
			X509Certificate				serverCertificate, 
			string						targetHost, 
			X509CertificateCollection	serverRequestedCertificates)
		{
			if (this.ClientCertSelection != null)
			{
				return this.ClientCertSelection(
					clientCertificates,
					serverCertificate,
					targetHost,
					serverRequestedCertificates);
			}

			return null;
		}

		internal AsymmetricAlgorithm RaisePrivateKeySelection(
			X509Certificate clientCertificate, 
			string			targetHost)
		{
			if (this.PrivateKeySelection != null)
			{
				return this.PrivateKeySelection(
					clientCertificate,
					targetHost);
			}

			return null;
		}

		#endregion
	}
}
