// cs0555.cs : User-defined conversion cannot take an object of the enclosing type
// and convert to an object of the enclosing type
// Line : 8

class Blah {

	public static void Main () {}

	public static implicit operator Blah (Blah i) {}

}
