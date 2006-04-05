//
// System.Net.Security.SslStream.cs
//
// Authors:
//	Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004
// (c) 2004 Novell, Inc. (http://www.novell.com)
//

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

#if NET_2_0 

using System;
using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;

namespace System.Net.Security 
{
	public class SslStream : AuthenticatedStream
	{
		#region Fields

		int readTimeout;
		int writeTimeout;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public SslStream (Stream innerStream)
			: base (innerStream, false)
		{
		}

		[MonoTODO]
		public SslStream (Stream innerStream, bool leaveStreamOpen)
			: base (innerStream, leaveStreamOpen)
		{
		}
#if SECURITY_DEP
		[MonoTODO]
		public SslStream (Stream innerStream, bool leaveStreamOpen, RemoteCertificateValidationCallback certValidationCallback)
			: base (innerStream, leaveStreamOpen)
		{
		}

		[MonoTODO]
		public SslStream (Stream innerStream, bool leaveStreamOpen, RemoteCertificateValidationCallback certValidationCallback, LocalCertificateSelectionCallback certSelectionCallback)
			: base (innerStream, leaveStreamOpen)
		{
		}
#endif
		#endregion // Constructors

		#region Properties

		public override bool CanRead {
			get { return InnerStream.CanRead; }
		}

		public override bool CanSeek {
			get { return InnerStream.CanSeek; }
		}

		[MonoTODO]
		public override bool CanTimeout {
			get { throw new NotImplementedException (); }
		}

		public override bool CanWrite {
			get { return InnerStream.CanWrite; }
		}

		[MonoTODO]
		public virtual bool CheckCertRevocationStatus {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual CipherAlgorithmType CipherAlgorithm  {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual int CipherStrength  {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual HashAlgorithmType HashAlgorithm  {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual int HashStrength  {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override bool IsAuthenticated { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override bool IsEncrypted { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override bool IsMutuallyAuthenticated { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override bool IsServer { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override bool IsSigned { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual ExchangeAlgorithmType KeyExchangeAlgorithm { 
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual int KeyExchangeStrength { 
			get { throw new NotImplementedException (); }
		}

		public override long Length {
			get { return InnerStream.Length; }
		}

		[MonoTODO]
		public virtual X509Certificate LocalCertificate {
			get { throw new NotImplementedException (); }
		}

		public override long Position {
			get { return InnerStream.Position; }
			set { InnerStream.Position = value; }
		}

		public override int ReadTimeout {
			get { return readTimeout; }
			set { readTimeout = value; }
		}

		[MonoTODO]
		public virtual X509Certificate RemoteCertificate {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual SslProtocols SslProtocol {
			get { throw new NotImplementedException (); }
		}

		public override int WriteTimeout {
			get { return writeTimeout; }
			set { writeTimeout = value; }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public virtual IAsyncResult BeginAuthenticateAsClient (string targetHost, AsyncCallback asyncCallback, object asyncState)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual IAsyncResult BeginAuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates, SslProtocols sslProtocolType, bool checkCertificateRevocation, AsyncCallback asyncCallback, object asyncState)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IAsyncResult BeginRead (byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, AsyncCallback callback, object asyncState)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual IAsyncResult BeginAuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols sslProtocolType, bool checkCertificateRevocation, AsyncCallback callback, object asyncState)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override IAsyncResult BeginWrite (byte[] buffer, int offset, int count, AsyncCallback asyncCallback, object asyncState)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void AuthenticateAsClient (string targetHost)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void AuthenticateAsClient (string targetHost, X509CertificateCollection clientCertificates, SslProtocols sslProtocolType, bool checkCertificateRevocation)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void AuthenticateAsServer (X509Certificate serverCertificate)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void AuthenticateAsServer (X509Certificate serverCertificate, bool clientCertificateRequired, SslProtocols sslProtocolType, bool checkCertificateRevocation)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		[MonoTODO]
		public virtual void EndAuthenticateAsClient (IAsyncResult asyncResult)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int EndRead (IAsyncResult asyncResult)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void EndAuthenticateAsServer (IAsyncResult asyncResult)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void EndWrite (IAsyncResult asyncResult)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Flush ()
		{
			InnerStream.Flush ();
		}

		[MonoTODO]
		public override int Read (byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void SetLength (long value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void Write (byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException ();
		}

		public void Write (byte[] buffer)
		{
			Write (buffer, 0, buffer.Length);
		}

		#endregion // Methods
	}
}

#endif
