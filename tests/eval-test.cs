using System;
using Mono.CSharp;

class X {
	static void Run (string id, string stmt)
	{
		try {
			Evaluator.Run (stmt);
		} catch {
			Console.WriteLine ("Failed on test {0}", id);
			throw;
		}
	}

	static void Evaluate (string id, string expr, object expected)
	{
		try {
			object res = Evaluator.Evaluate (expr);
			if (res == null && expected == null)
				return;

			if (!expected.Equals (res)){
				Console.WriteLine ("Failed on test {2} Expecting {0}, got {1}", expected, res, id);
				throw new Exception ();
			}
		} catch {
			Console.WriteLine ("Failed on test {0}", id);
			throw;
		}
	}
	
	static void Main ()
	{
		Run ("1",      "System.Console.WriteLine (100);");
		Run ("Length", "var a = new int [] {1,2,3}; var b = a.Length");
		
		Evaluate ("CompareString", "\"foo\" == \"bar\";", false);
		Evaluate ("CompareInt", "var a = 1; a+2;", 3);

		Evaluator.Run ("using System; using System.Linq;");
		Run ("LINQ-1", "var a = new int[]{1,2,3};\nfrom x in a select x;");
		Run ("LINQ-2", "var a = from f in System.IO.Directory.GetFiles (\"/tmp\") where f == \"passwd\" select f;");
	}
	
}