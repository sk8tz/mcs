// cs0246-12.cs: The type or namespace name `RNGCryptoServiceProvider' could not be found. Are you missing a using directive or an assembly reference?
// Line: 13

using System;

namespace System.Web.Configuration
{
	class MachineKeyConfig
	{
		static MachineKeyConfig ()
		{
			autogenerated = new byte [64];
			RNGCryptoServiceProvider cp = new RNGCryptoServiceProvider ();
			cp.GetBytes (autogenerated);
		}
	}
}

