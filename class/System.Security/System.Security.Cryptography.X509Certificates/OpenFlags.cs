//
// OpenFlags.cs - System.Security.Cryptography.X509Certificates.OpenFlags
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

	public enum OpenFlags {
		ReadOnly,
		ReadWrite, 
		MaxAllowed,
		OpenExistingOnly,
		IncludeArchived
	}
}

#endif