using System;
using System.Runtime.InteropServices;

[StructLayout (LayoutKind.Sequential)]
public class MySystemTime {
	public ushort Year; 
	public ushort Month;
	public ushort DayOfWeek; 
	public ushort Day; 
	public ushort Hour; 
	public ushort Minute; 
	public ushort Second; 
	public ushort Milliseconds; 
}

[StructLayout (LayoutKind.Sequential)]
public struct Point {
	public int x;
	public int y;
}

[StructLayout (LayoutKind.Explicit)]
public struct Rect {	
	[FieldOffset (0)] public int left;
	[FieldOffset (4)] public int top;
	[FieldOffset (8)] public int right;
	[FieldOffset (12)] public int bottom;
}

public class Blah {

	[DllImport ("Kernel32.dll")]
	public static extern void GetSystemTime (MySystemTime st);

	[DllImport ("User32.dll")]
	public static extern bool PtInRect (ref Rect r, Point p);	

	public static int Main () {

		MySystemTime st = new MySystemTime ();

		GetSystemTime (st);

		Console.WriteLine ("Today's date is : {0:0000}-{1:00}-{2:00}", st.Year, st.Month, st.Day);
		Console.WriteLine ("The time now is : {0:00}:{1:00}:{2:00}", st.Hour, st.Minute, st.Second);

		Rect r = new Rect ();

		r.left = 10;
		r.top  = 12;
		r.right = 30;
		r.bottom = 30;

		Point p = new Point ();

		p.x = 15;
		p.y = 20;

		Console.WriteLine (PtInRect (ref r, p));
			

		Console.WriteLine ("Test passes");
		return 0;
	}
}
