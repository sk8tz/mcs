using System;

public class Blah {

	public static void Foo (ref int i, ref int j)
	{
	}

	public static void Foo (int i, int j)
	{
	}

	public static int Main ()
	{
		int i = 1;
		int j = 2;

		Foo (i, j);
		Foo (ref i, ref j);

		return  0;
	}
}
