delegate void S ();
using System;

class X {
	static int Main ()
	{
		int a = 1;
		Console.WriteLine ("A is = " + a);
		int c = a;
		Console.WriteLine (c);
		if (a != 1){
			return 1;
		}
		
		S b = delegate {
			if (a != 1)
				Environment.Exit (1);
			Console.WriteLine ("in Delegate");
			a = 2;
			if (a != 2)
				Environment.Exit (2);
			Console.WriteLine ("Inside = " + a);
			a = 3;
			Console.WriteLine ("After = " + a);
		};
		if (a != 1)
			return 3;
		b ();
		if (a != 3)
			return 4;
		Console.WriteLine ("Back, got " + a);

		return 0;
	}
}
