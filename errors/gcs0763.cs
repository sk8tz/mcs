// CS0763: A partial method declaration and partial method implementation must be both `static' or neither
// Line: 11
// Compiler options: -langversion:linq

public partial class C
{
	static partial void Foo ()
	{
	}
	
	partial void Foo ();
}
