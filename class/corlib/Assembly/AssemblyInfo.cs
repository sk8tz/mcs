//
// AssemblyInfo.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Resources;
using System.Security;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about the system assembly

#if (NET_1_0)
	[assembly: AssemblyVersion("1.0.3300.0")]
	[assembly: SatelliteContractVersion("1.0.3300.0")]
#endif
#if (NET_1_1)
	[assembly: AssemblyVersion("1.0.5000.0")]
	[assembly: SatelliteContractVersion("1.0.5000.0")]
	[assembly: ComCompatibleVersion(1, 0, 3300, 0)]
	[assembly: TypeLibVersion(1, 10)]
#endif

//[assembly: AssemblyTitle("mscorlib.dll")]
[assembly: AssemblyDescription("mscorlib.dll")]
//[assembly: AssemblyConfiguration("Development version")]
//[assembly: AssemblyCompany("MONO development team")]
//[assembly: AssemblyProduct("MONO CLI")]
//[assembly: AssemblyCopyright("(c) 2003 Various Authors")]

[assembly: CLSCompliant(true)]
//[assembly: AssemblyDefaultAlias("mscorlib.dll")]
//[assembly: AssemblyInformationalVersion("0.0.0.1")]
[assembly: NeutralResourcesLanguage("en-US")]

[assembly: AllowPartiallyTrustedCallers]
[assembly: Guid("BED7F4EA-1A96-11D2-8F08-00A0C9A6186D")]

//[assembly: AssemblyDelaySign(true)]
//[assembly: AssemblyKeyFile("../../EcmaKey.snk")]
