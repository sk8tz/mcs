// cs0208-9.cs: Cannot declare a pointer to a managed type ('Foo')
// Line: 7
// Compiler options: -t:library -unsafe

public unsafe struct Foo
{
        public Foo *foo;
	string x;
}


