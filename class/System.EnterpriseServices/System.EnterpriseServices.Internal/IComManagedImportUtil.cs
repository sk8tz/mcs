// System.EnterpriseServices.Internal.IComManagedImportUtil.cs
//
// Author:
//   Alejandro S�nchez Acosta (raciel@es.gnu.org)
//
// Copyright (C) 2002 Alejandro S�nchez Acosta
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices.Internal
{
#if NET_1_1
	[Guid("c3f8f66b-91be-4c99-a94f-ce3b0a951039")]
	public interface IComManagedImportUtil 
	{
		[DispId(4)] 
		void GetComponentInfo (string assemblyPath, out string numComponents, out string componentInfo);
		[DispId(5)] 
		void InstallAssembly (string filename, string parname, string appname);
	}
#endif
}
