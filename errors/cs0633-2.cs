// cs0633-2.cs: The argument to the `IndexerName' attribute must be a valid identifier
// Line: 5

public class MonthDays {
   [System.Runtime.CompilerServices.IndexerName ("")]
   public int this [int a] {
      get {
         return 0;
      }
   }

   public static void Main ()
   {
	int i = new MonthDays () [1];
   }
}


