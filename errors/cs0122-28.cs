// CS0122:
// Line: 18

using System;

class A
{
	class X
	{
		public static string V = "a";
	}
}

class C : A
{
	public static void Main ()
	{
		Console.WriteLine (X.V);
	}
}
