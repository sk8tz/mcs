//
// fixed
//
using System;

class X {

	void A ()
	{
	}
				
	static void Main ()
	{
		int loop = 0;
		
		goto a;
	b:
		loop++;
	a:
		Console.WriteLine ("Hello");
		for (;;){
			goto b;
		}
	}
}
