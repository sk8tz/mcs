// cs0120-7.cs: `MemRefMonoBug.String': An object reference is required for the nonstatic field, method or property
// Line: 11

using System;

public class MemRefMonoBug {
	private string String;	// this member has the same name as System.String class
	public static void Main ()
	{
		new MemRefMonoBug ().String = "";	// this line causes no problem
		String = "";	// mcs crashes in this line
	}
}

