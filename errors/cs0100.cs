// cs0100.cs: same parameters name in a method declaration.
// Line: 6
//
// Author: 
// 	Alejandro S�nchez Acosta  <raciel@es.gnu.org>
//
// (C) Alejandro S�nchez Acosta
//

public class X 
{
	public void Add (int a, int a)
	{
		int c;
		c= a + a;
		Console.WriteLine (c);
	}

	static void Main ()
	{
		this.Add (3, 5);
	}
}

