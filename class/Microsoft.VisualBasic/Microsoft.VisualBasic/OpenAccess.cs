//
// OpenAccess.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//
namespace Microsoft.VisualBasic {
	public enum OpenAccess : int {
		Read = 1,
		Write = 2,
		ReadWrite = 3,
		Default = -1
	};
}
