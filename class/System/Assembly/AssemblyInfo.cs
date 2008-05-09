//
// AssemblyInfo.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Diagnostics;
using System.Reflection;
using System.Resources;
using System.Security;
using System.Security.Permissions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about the system assembly

[assembly: AssemblyTitle ("System.dll")]
[assembly: AssemblyDescription ("System.dll")]
[assembly: AssemblyDefaultAlias ("System.dll")]

[assembly: AssemblyCompany (Consts.MonoCompany)]
[assembly: AssemblyProduct (Consts.MonoProduct)]
[assembly: AssemblyCopyright (Consts.MonoCopyright)]
[assembly: AssemblyVersion (Consts.FxVersion)]
[assembly: SatelliteContractVersion (Consts.FxVersion)]
[assembly: AssemblyInformationalVersion (Consts.FxFileVersion)]

[assembly: CLSCompliant (true)]
[assembly: NeutralResourcesLanguage ("en-US")]

[assembly: ComVisible (false)]
[assembly: AllowPartiallyTrustedCallers]

#if !BOOTSTRAP_WITH_OLDLIB
	[assembly: SecurityPermission (SecurityAction.RequestMinimum, SkipVerification = true)]
	[assembly: AssemblyDelaySign (true)]
#if NET_2_1
	[assembly: AssemblyKeyFile ("../silverlight.pub")]
#else
	[assembly: AssemblyKeyFile ("../ecma.pub")]
#endif
#endif

#if NET_2_0
	[assembly: AssemblyFileVersion (Consts.FxFileVersion)]
	[assembly: Debuggable (DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
	[assembly: CompilationRelaxations (CompilationRelaxations.NoStringInterning)]
	[assembly: StringFreezing]
	[assembly: ComCompatibleVersion (1, 0, 3300, 0)]
	[assembly: DefaultDependency (LoadHint.Always)]
#elif NET_1_1
	[assembly: AssemblyTrademark ("")]
	[assembly: AssemblyConfiguration ("")]
	[assembly: ComCompatibleVersion (1, 0, 3300, 0)]
	[assembly: TypeLibVersion (1, 10)]
#elif NET_1_0
	[assembly: AssemblyTrademark ("")]
	[assembly: AssemblyConfiguration ("")]
#endif

#if NET_2_1
	[assembly: InternalsVisibleTo ("Mono.Moonlight, PublicKey=002400000480000094000000060200000024000052534131000400000100010079159977d2d03a8e6bea7a2e74e8d1afcc93e8851974952bb480a12c9134474d04062447c37e0e68c080536fcf3c3fbe2ff9c979ce998475e506e8ce82dd5b0f350dc10e93bf2eeecf874b24770c5081dbea7447fddafa277b22de47d6ffea449674a4f9fccf84d15069089380284dbdd35f46cdff12a1bd78e4ef0065d016df")]
#endif
