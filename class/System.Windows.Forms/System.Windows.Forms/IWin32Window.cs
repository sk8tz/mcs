//
// System.IWin32Window.cs
//
// Author:
// William Lamb (wdlamb@notwires.com)
// Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
//

namespace System.Windows.Forms
{
	[ComVisible(true)]
	[Guid("")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IWin32Window
	{
		IntPtr Handle {get;}
	}
}
