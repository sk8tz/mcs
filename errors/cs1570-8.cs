// cs1570-8.cs: XML comment on `F:Testing.Test.Constant2' has non-well-formed XML (unmatched closing element: expected summary but found invalid  Line 3, position 10.)
// Line: 19
// Compiler options: -doc:dummy.xml -warn:1 -warnaserror

using System;

namespace Testing
{
	public class Test
	{
		/// <summary>
		/// comment for const declaration
		/// </summary>
		const string Constant = "CONSTANT STRING";

		/// <summary>
		/// invalid comment for const declaration
		/// </invalid>
		const string Constant2 = "CONSTANT STRING";

		/**
		<summary>
		Javaism comment for const declaration
		</summary>
		*/
		const string Constant3 = "CONSTANT STRING";

		public static void Main ()
		{
		}
	}
}

