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

#if (WSE1)
	[assembly: AssemblyVersion("1.0.0.0")]
	[assembly: SatelliteContractVersion("1.0.0.0")]
#endif
#if (WSE2)
	[assembly: AssemblyVersion("2.0.0.0")]
	[assembly: SatelliteContractVersion("2.0.0.0")]
//	[assembly: ComCompatibleVersion(1, 0, 3300, 0)]
//	[assembly: TypeLibVersion(1, 10)]
#endif

[assembly: AssemblyTitle("Microsoft.Web.Services.dll")]
[assembly: AssemblyDescription("Web Service Enhancement")]
[assembly: AssemblyConfiguration("Development version")]
[assembly: AssemblyCompany("MONO development team")]
[assembly: AssemblyProduct("MONO CLI")]
[assembly: AssemblyCopyright("(c) 2003 Various Authors")]

[assembly: CLSCompliant(true)]
[assembly: AssemblyDefaultAlias("Microsoft.Web.Services.dll")]
[assembly: AssemblyInformationalVersion("0.0.0.1")]
[assembly: NeutralResourcesLanguage("en-US")]

//[assembly: AssemblyDelaySign(false)]
//[assembly: AssemblyKeyFile("")]