//
// OpenShare.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//
namespace Microsoft.VisualBasic {
	public enum OpenShare : int {
		LockReadWrite = 0,
		LockWrite = 1,
		LockRead = 2,
		Shared = 3,
		Default = -1
	};
}
