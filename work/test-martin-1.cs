using System;

public class X
{
	public delegate void TestDelegate ();

	static long sum_i, sum_k;

	public static int Test ()
	{
		TestDelegate d = null;
		for (int i = 1; i <= 5; i++) {
			int k = i;
			TestDelegate temp = delegate {
				Console.WriteLine (i);
				Console.WriteLine (k);
				sum_i += 1 << i;
				sum_k += 1 << k;
			};
			temp ();
			d += temp;
		}
		Console.WriteLine ();
		if (sum_i != 62)
			return 1;
		if (sum_k != 62)
			return 2;
		sum_i = sum_k = 0;
		d();
		if (sum_i != 320)
			return 3;
		if (sum_k != 62)
			return 4;
		return 0;
	}

	public static int Main ()
	{
		int result = Test ();
		if (result != 0)
			Console.WriteLine ("ERROR: {0}", result);
		return result;
	}
}
