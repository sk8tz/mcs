//
// System.Security.Cryptography.X509Certificates.X509Chain
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell Inc. (http://www.novell.com)//
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

namespace System.Security.Cryptography.X509Certificates {

	public class X509Chain {

		private bool _machineContext;
		private X509ChainElementCollection _elements;
		private X509ChainPolicy _policy;
		private X509ChainStatus[] _status;

		// constructors

		public X509Chain () : this (false)
		{
		}

		public X509Chain (bool useMachineContext) 
		{
			_machineContext = useMachineContext;
			_elements = new X509ChainElementCollection ();
			_policy = new X509ChainPolicy ();
		}

		// properties

		public X509ChainElementCollection ChainElements {
			get { return _elements; }
		}

		public X509ChainPolicy ChainPolicy {
			get { return _policy; }
		} 

		public X509ChainStatus[] ChainStatus {
			get { 
				if (_status == null)
					_status = new X509ChainStatus [0]; 
				return _status;
			}
		} 

		// methods

		[MonoTODO]
		public bool Build (X509CertificateEx certificate)
		{
			return false;
		}

		[MonoTODO]
		public void Reset () 
		{
		}

		// static methods

		public static X509Chain Create ()
		{
			return new X509Chain ();
		}
	}
}

#endif
