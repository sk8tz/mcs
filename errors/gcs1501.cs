// CS1501: No overload for method `Foo' takes `1' arguments
// Line: 17
// Compiler options: -langversion:linq

static class Extensions
{
	public static string Foo (this string s)
	{
		return s;
	}
}

public class M
{
	public static void Main ()
	{
		1.Foo ("foo");
	}
}