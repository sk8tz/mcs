// CS0579: The attribute `System.ObsoleteAttribute' cannot be applied multiple times
// Line: 12
// Compiler options: -langversion:linq

using System;

partial class C
{
	[Obsolete ("A")]
	partial void PartialMethod ();
	[Obsolete ("A")]
	partial void PartialMethod ()
	{
	}
}
