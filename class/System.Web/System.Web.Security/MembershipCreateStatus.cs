//
// System.Web.Security.MembershipCreateStatus
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_2_0
namespace System.Web.Security {
	public enum MembershipCreateStatus {
		Success,
		UserNotFound,
		InvalidPassword,
		InvalidQuestion,
		InvalidAnswer,
		InvalidEmail,
		DuplicateUsername,
		DuplicateEmail,
		UserRejected,
		ProviderError
	}
}
#endif

