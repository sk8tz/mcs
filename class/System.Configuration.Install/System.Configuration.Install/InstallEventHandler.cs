// System.Configuration.Install.InstallEventHandler.cs
//
// Author:
// 	Alejandro S�nchez Acosta  <raciel@es.gnu.org>
//
// (C) Alejandro S�nchez Acosta
// 

using System.Runtime.Serialization;

namespace System.Configuration.Install
{
	[Serializable]
	public delegate void InstallEventHandler (object sender, InstallEventArgs e);
}
