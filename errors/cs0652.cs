// cs0652.cs: Comparison to integral constant is useless; the constant is outside the range of type `byte'
// Line: 9
// Compiler options: -warnaserror -warn:2

class X
{
	void b ()
	{
                byte b = 0;
                if (b == 500)
                    return;
	}

	static void Main () {}
}
