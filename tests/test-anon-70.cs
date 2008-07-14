// Cloning tests

using System;

public class C
{
	delegate void D ();
	
	static void Test (D d)
	{
	}
	
	public static void Main ()
	{
		Exception diffException;
		
		Test (delegate () {
			diffException = null;
					try {
					} catch (Exception ex) {
						diffException = ex;
					} finally {
					}
					
					try {
					} catch {
					}
				});
				
		int[] i_a = new int [] { 1,2,3 };
		
		Test (delegate () {
				foreach (int t in i_a) {
				}
				Test (delegate () {
					Console.WriteLine (typeof (void));
				});
			});
			
		Test (delegate () {
			Console.WriteLine (typeof (void));
		});

	}
}
