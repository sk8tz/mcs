//
// Mono.Unix/UnixUser.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2004 Jonathan Pryor
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

using System;
using System.Collections;
using System.Text;
using Mono.Unix;

namespace Mono.Unix {

	public sealed class UnixUser
	{
		private UnixUser () {}

		[CLSCompliant (false)]
		[Obsolete ("The return type of this method will change after the next release")]
		public static uint GetUserId (string user)
		{
			return new UnixUserInfo (user).UserId;
		}

		[CLSCompliant (false)]
		[Obsolete ("The return type of this method will change after the next release")]
		public static uint GetCurrentUserId ()
		{
			return Syscall.getuid ();
		}

		public static string GetCurrentUserName ()
		{
			return GetName (GetCurrentUserId ());
		}

		public static UnixUserInfo GetCurrentUser ()
		{
			return new UnixUserInfo (GetCurrentUserId ());
		}

		// I would hope that this is the same as GetCurrentUserName, but it is a
		// different syscall, so who knows.
		public static string GetLogin ()
		{
			StringBuilder buf = new StringBuilder (4);
			int r;
			do {
				buf.Capacity *= 2;
				r = Syscall.getlogin_r (buf, (ulong) buf.Capacity);
			} while (r == (-1) && Syscall.GetLastError() == Error.ERANGE);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
			return buf.ToString ();
		}

		[CLSCompliant (false)]
		[Obsolete ("The return type of this method will change after the next release")]
		public static uint GetGroupId (string user)
		{
			return new UnixUserInfo (user).GroupId;
		}

		[CLSCompliant (false)]
		[Obsolete ("Use GetGroupId (long)")]
		public static uint GetGroupId (uint user)
		{
			return new UnixUserInfo (user).GroupId;
		}

		public static long GetGroupId (long user)
		{
			return new UnixUserInfo (user).GroupId;
		}

		public static string GetRealName (string user)
		{
			return new UnixUserInfo (user).RealName;
		}

		[CLSCompliant (false)]
		[Obsolete ("Use GetRealName (long)")]
		public static string GetRealName (uint user)
		{
			return new UnixUserInfo (user).RealName;
		}

		public static string GetRealName (long user)
		{
			return new UnixUserInfo (user).RealName;
		}

		public static string GetHomeDirectory (string user)
		{
			return new UnixUserInfo (user).HomeDirectory;
		}

		[CLSCompliant (false)]
		public static string GetHomeDirectory (uint user)
		{
			return new UnixUserInfo (user).HomeDirectory;
		}

		[CLSCompliant (false)]
		[Obsolete ("Use GetUserName(long)")]
		public static string GetName (uint user)
		{
			return new UnixUserInfo (user).UserName;
		}

		[CLSCompliant (false)]
		[Obsolete ("Use GetUserName(long)")]
		public static string GetUserName (uint user)
		{
			return new UnixUserInfo (user).UserName;
		}

		public static string GetUserName (long user)
		{
			return new UnixUserInfo (user).UserName;
		}

		public static string GetPassword (string user)
		{
			return new UnixUserInfo (user).Password;
		}

		[CLSCompliant (false)]
		[Obsolete ("Use GetPassword (long)")]
		public static string GetPassword (uint user)
		{
			return new UnixUserInfo (user).Password;
		}

		public static string GetPassword (long user)
		{
			return new UnixUserInfo (user).Password;
		}

		public static string GetShellProgram (string user)
		{
			return new UnixUserInfo (user).ShellProgram;
		}

		[CLSCompliant (false)]
		[Obsolete ("Use GetShellProgram(long)")]
		public static string GetShellProgram (uint user)
		{
			return new UnixUserInfo (user).ShellProgram;
		}

		public static string GetShellProgram (long user)
		{
			return new UnixUserInfo (user).ShellProgram;
		}

		public static UnixUserInfo[] GetLocalUsers ()
		{
			ArrayList entries = new ArrayList ();
			lock (Syscall.pwd_lock) {
				if (Native.Syscall.setpwent () != 0) {
					UnixMarshal.ThrowExceptionForLastError ();
				}
				try {
					Passwd p;
					while ((p = Syscall.getpwent()) != null)
						entries.Add (new UnixUserInfo (p));
					if (Syscall.GetLastError () != (Error) 0)
						UnixMarshal.ThrowExceptionForLastError ();
				}
				finally {
					Syscall.endpwent ();
				}
			}
			return (UnixUserInfo[]) entries.ToArray (typeof(UnixUserInfo));
		}
	}
}

// vim: noexpandtab
