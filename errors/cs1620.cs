// cs1620.cs: Argument `1' must be passed with the `out' keyword
// Line: 13

class C
{
	public static void test (out int i)
	{
		i = 5;
	}

	public static void Main() {
		int i = 1;
		test (ref i);
	}
}
