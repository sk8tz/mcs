// System.EnterpriseServices.Internal.ISoapClientImport.cs
//
// Author:  Mike Kestner (mkestner@ximian.com)
//
// Copyright (C) 2004 Novell, Inc.
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices.Internal {

#if NET_1_1
	[Guid("E7F0F021-9201-47e4-94DA-1D1416DEC27A")]
	public interface ISoapClientImport {
		[DispId(1)]
		void ProcessClientTlbEx (
			[MarshalAs(UnmanagedType.BStr)] string progId,
			[MarshalAs(UnmanagedType.BStr)] string virtualRoot,
			[MarshalAs(UnmanagedType.BStr)] string baseUrl,
			[MarshalAs(UnmanagedType.BStr)] string authentication,
			[MarshalAs(UnmanagedType.BStr)] string assemblyName,
			[MarshalAs(UnmanagedType.BStr)] string typeName);
	}
#endif
}
