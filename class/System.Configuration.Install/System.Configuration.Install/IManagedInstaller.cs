// System.Configuration.Installer.IManagedInstaller.cs
//
// Author:
// 	Alejandro S�nchez Acosta
//
// (C) Alejandro S�nchez Acosta
// 

using System.Runtime.InteropServices;

namespace System.Configuration.Installer
{
	//[Guid("")]
	//[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IManagedInstaller
	{
		int ManagedInstall (string commandLine, int hInstall);
	}
}
