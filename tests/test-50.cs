using System;
using System.Runtime.InteropServices;

public class Blah {

	[DllImport ("user32")]
	public static extern int MessageBox (int hWnd, string pText, string pCaption, int uType);

	public static int Main ()
	{
		MessageBox (0, "Hello World !", "PInvoke Test", 0);

		return 0;
	}
}
