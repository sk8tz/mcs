// FileShare.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Fri, 7 Sep 2001 16:32:26 UTC
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com


namespace System.IO {


	/// <summary>
	/// </summary>
	[Flags]
	public enum FileShare {

		/// <summary>
		/// </summary>
		None = 0x00000000,

		/// <summary>
		/// </summary>
		Read = 0x00000001,

		/// <summary>
		/// </summary>
		Write = 0x00000002,

		/// <summary>
		/// </summary>
		ReadWrite = Read | Write,
	} // FileShare

} // System.IO
