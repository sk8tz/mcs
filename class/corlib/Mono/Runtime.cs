//
// Mono Runtime gateway functions
//
//

using System;
using System.Runtime.CompilerServices;

namespace Mono {

	internal class Runtime
	{
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern void mono_runtime_install_handlers ();
		
		static internal void InstallSignalHandlers ()
		{
			mono_runtime_install_handlers ();
		}
	}
	
}
