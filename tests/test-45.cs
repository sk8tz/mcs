using System;

public class Blah {

	private static int[] array = {0, 1, 2, 3};
	
	public static int Main ()
	{
		int [] i = new int [] { 0, 1, 2, 3 };

		int [,] j = new int [,] { {0,1}, {2,3}, {4,5}, {6,7} };
		
		int [] a = { 4, 5, 6, 7 };

		// int foo = 1;
		// int [] k = new int [] { foo };

		if (i [2] != 2)
			return 1;
		
		if (j [1,1] != 3)
			return 1;
		
		for (int t = 0; t < 4; ++t) {
			if (array [t] != t)
				return 1;
			
			if (a [t] != (t + 4))
				return 1;
		}
		
		Console.WriteLine ("Array initialization test okay.");
				   
		return 0;
	}
}
