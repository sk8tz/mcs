// System.Configuration.Install.IManagedInstaller.cs
//
// Author:
// 	Alejandro S�nchez Acosta
//
// (C) Alejandro S�nchez Acosta
// 

using System.Runtime.InteropServices;

namespace System.Configuration.Install
{
	[Guid("1E233FE7-C16D-4512-8C3B-2E9988F08D38")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IManagedInstaller
	{
		[return: MarshalAs(UnmanagedType.I4)] 
		int ManagedInstall (
			[MarshalAs(UnmanagedType.BStr)] 
			[In]
			string commandLine, 
			[MarshalAs(UnmanagedType.I4)] 
			[In]
			int hInstall);
	}
}
