// System.EnterpriseServices.SxsOption.cs
//
// Author:  Mike Kestner (mkestner@ximian.com)
//
// Copyright (C) 2004 Novell, Inc.
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {

#if NET_1_1
	[Serializable]
	[ComVisible(false)]
	public enum SxsOption {

		Ignore,
		Inherit,
		New
	}
#endif
}
