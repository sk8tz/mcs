using System;

public class Blah {

	public enum MyEnum : byte {
		Foo = 254,
		Bar
	}

	enum A {
		a, b, c
	}
	
	enum B {
		x, y, z
	}
	
	enum AA : byte { a, b }
	enum BB : ulong { x, y }
	
	public static int Main ()
	{
		byte b = (byte) MyEnum.Foo;
		
		Console.WriteLine ("Foo has a value of " + b);

		if (b != 254)
			return 1;
		
		int i = (int) A.a;
		int j = (int) B.x;
		int k = (int) A.c;
		int l = (int) AA.b + 1;

		if (i != j)
			return 1;

		if (k != l)
			return 1;

		A var = A.b;

		Console.WriteLine ("Enum emission test okay");
		return 0;
	}
}
