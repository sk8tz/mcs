// System.EnterpriseServices.Internal.IComManagedImportUtil.cs
//
// Author:
//   Alejandro S�nchez Acosta (raciel@es.gnu.org)
//
// (C) Alejandro S�nchez Acosta
//
using System;

using System.Runtime.InteropServices;

namespace System.EnterpriseServices.Internal
{
	//[Guid("")]
	public interface IComManagedImportUtil 
	{
		void GetComponentInfo (string assemblyPath, out string numComponents, out string componentInfo);

		void InstallAssembly (string filename, string parname, string appname);
	}
}
