public class C
{
	public static decimal operator -(dynamic p1, C p2)
	{
		return 9;
	}
	
	public static int Main ()
	{
		var c = new C ();
		var v = 1 - c;
		
		if (v != 9)
			return 1;
		
		return 0;
	}
}
