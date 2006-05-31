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
using System.Reflection;
using System.Resources;
using System.Security;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about the System.Runtime.Serialization.Formatters.Soap assembly

[assembly: AssemblyVersion (Consts.FxVersion)]
[assembly: SatelliteContractVersion (Consts.FxVersion)]

#if (ONLY_1_1)
[assembly: TypeLibVersion (1, 10)]
#endif

#if NET_1_1
[assembly: ComCompatibleVersion (1, 0, 3300, 0)]
#endif

#if NET_2_0
[assembly: AssemblyDefaultAlias ("System.Runtime.Serialization.Formatters.Soap.dll")]
[assembly: AssemblyDescription ("System.Runtime.Serialization.Formatters.Soap.dll")]
[assembly: AssemblyTitle ("System.Runtime.Serialization.Formatters.Soap.dll")]
[assembly: AssemblyFileVersion (Consts.FxFileVersion)]
[assembly: AssemblyInformationalVersion (Consts.FxFileVersion)]
[assembly: ComVisible (true)]
#endif

//[assembly: AssemblyConfiguration("Development version")]
//[assembly: AssemblyCompany("MONO development team")]
//[assembly: AssemblyProduct("MONO CLI")]
//[assembly: AssemblyCopyright("(c) 2003 Various Authors")]
//[assembly: AssemblyTrademark("")]

#if !TARGET_JVM
[assembly: CLSCompliant(true)]
#endif
[assembly: NeutralResourcesLanguage("en-US")]

[assembly: AssemblyDelaySign(true)]
#if !TARGET_JVM
[assembly: AssemblyKeyFile("../msfinal.pub")]
#endif
