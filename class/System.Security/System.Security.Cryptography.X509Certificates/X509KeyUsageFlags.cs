//
// X509KeyUsageFlags.cs - System.Security.Cryptography.X509Certificates.X509KeyUsageFlags
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_2_0

using System;

namespace System.Security.Cryptography.X509Certificates {

	// Note: Match the definition of framework version 1.2.3400.0 on http://longhorn.msdn.microsoft.com

	public enum X509KeyUsageFlags {
		EncipherOnly,
		CRLSign,
		KeyCertSign,
		KeyAgreement,
		DataEncipherment,
		KeyEncipherment,
		NonRepudiation,
		DigitalSignature,
		DecipherOnly
	}
}

#endif