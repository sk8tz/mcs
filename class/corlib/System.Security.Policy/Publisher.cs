//
// Publisher.cs: Publisher Policy using X509 Certificate
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Security.Cryptography.X509Certificates;
using System.Security.Permissions;

namespace System.Security.Policy {

[Serializable]
public sealed class Publisher : IIdentityPermissionFactory, IBuiltInEvidence {
	
	private X509Certificate m_cert;

	public Publisher (X509Certificate cert) 
	{
		if (cert == null)
			throw new ArgumentNullException ("cert");
		m_cert = cert;
	}

	public X509Certificate Certificate { 
		get { 
			// needed to match MS implementation
			if (m_cert.GetRawCertData () == null)
				throw new NullReferenceException ("m_cert");
			return m_cert; 
		}
	}

	public object Copy () 
	{
		return new Publisher (m_cert);
	}

	public IPermission CreateIdentityPermission (Evidence evidence) 
	{
		return new PublisherIdentityPermission (m_cert);
	}

	public override bool Equals (object o) 
	{
		if (!(o is Publisher))
			throw new ArgumentException ("not a Publisher");
		return m_cert.Equals ((o as Publisher).Certificate);
	}
	
	public override int GetHashCode () 
	{
		return m_cert.GetHashCode ();
	}

	public override string ToString ()
	{
		SecurityElement se = new SecurityElement ("System.Security.Policy.Publisher");
		se.AddAttribute ("version", "1");
		SecurityElement cert = new SecurityElement ("X509v3Certificate");
		string data = m_cert.GetRawCertDataString ();
		if (data != null)
			cert.Text = data;
		se.AddChild (cert);
		return se.ToString ();
	}

	// interface IBuiltInEvidence

	[MonoTODO]
	int IBuiltInEvidence.GetRequiredSize (bool verbose) 
	{
		return (verbose ? 3 : 1) + m_cert.GetRawCertData ().Length;
	}

	[MonoTODO]
	int IBuiltInEvidence.InitFromBuffer (char [] buffer, int position) 
	{
		return 0;
	}

	[MonoTODO]
	int IBuiltInEvidence.OutputToBuffer (char [] buffer, int position, bool verbose) 
	{
		return 0;
	}
}

}
