//
// Anonymous method group conversions
//

class X {
	delegate void T ();
	static event T Click;

	static void Method ()
	{
	}

	static void Main ()
	{
		T t;

		// Method group assignment
		t = Method;

		Click += Method;
	}
}
