// CS0019: Operator `==' cannot be applied to operands of type `method group' and `null'
// Line: 8

public class C
{
	public static void Main ()
	{
		bool a = DelegateMethod == null;
	}

	static int DelegateMethod(bool b)
	{
		return 3;
	}	
}
