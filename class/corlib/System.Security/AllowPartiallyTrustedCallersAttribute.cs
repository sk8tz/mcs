//
// System.Security.AllowPartiallyTrustedCallersAttribute implementation
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003, 2004 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace System.Security {

	// LAMESPEC: Undocument in original help file, but present, in framework 1.0
	[AttributeUsage (AttributeTargets.Assembly, AllowMultiple=false, Inherited=false)]
	public sealed class AllowPartiallyTrustedCallersAttribute : Attribute {

		public AllowPartiallyTrustedCallersAttribute () : base () {}
	}
}
