//
// X509VerificationFlags.cs - System.Security.Cryptography.X509Certificates.X509VerificationFlags
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;

namespace System.Security.Cryptography.X509Certificates {

	// Note: Match the definition of framework version 1.2.3400.0 on http://longhorn.msdn.microsoft.com

	public enum X509VerificationFlags {
		NoFlag,
		IgnoreNotTimeValid,
		IgnoreCtlNotTimeValid,
		IgnoreNotTimeNested,
		IgnoreInvalidBasicConstraints,
		AllowUnknownCertificateAuthority,
		IgnoreWrongUsage,
		IgnoreInvalidName,
		IgnoreInvalidPolicy,
		IgnoreEndRevocationUnknown,
		IgnoreCtlSignerRevocationUnknown,
		IgnoreCertificateAuthorityRevocationUnknown,
		IgnoreRootRevocationUnknown,
		AllFlags
	}
}

#endif