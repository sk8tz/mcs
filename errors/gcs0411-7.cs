// CS0411: The type arguments for method `Test' cannot be infered from the usage. Try specifying the type arguments explicitly.
// Line: 15
using System;

public delegate void Foo<T> (T t);

class X
{
	public void Test<T> (Foo<T> foo)
	{ }

	static void Main ()
	{
		X x = new X ();
		x.Test (delegate (string str) { });
	}
}
