//
// System.Web.Security.IMembershipProvider
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_2_0
using System.Configuration.Provider;

namespace System.Web.Security {
	public interface IMembershipProvider : IProvider {
		bool ChangePassword (string name, string oldPwd, string newPwd);
		bool ChangePasswordQuestionAndAnswer (string name, string password, string newPwdQuestion, string newPwdAnswer);
		MembershipUser CreateUser (string username, string password, string email, out MembershipCreateStatus status);
		bool DeleteUser (string name);
		MembershipUserCollection GetAllUsers ();
		int GetNumberOfUsersOnline ();
		string GetPassword (string name, string answer);
		MembershipUser GetUser (string name, bool userIsOnline);
		string GetUserNameByEmail (string email);
		string ResetPassword (string name, string answer);
		void UpdateUser (MembershipUser user);
		bool ValidateUser (string name, string password);
		string ApplicationName { get; set; }
		bool EnablePasswordReset { get; }
		bool EnablePasswordRetrieval { get; }
		bool RequiresQuestionAndAnswer { get; }
	}
}
#endif

