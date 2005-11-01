//
// Mono.Unix/UnixGroupInfo.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2004-2005 Jonathan Pryor
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

	public sealed class UnixGroupInfo
	{
		private Group group;

		public UnixGroupInfo (string group)
		{
			this.group = new Group ();
			Group gr;
			int r = Syscall.getgrnam_r (group, this.group, out gr);
			if (r != 0 || gr == null)
				throw new ArgumentException (Locale.GetText ("invalid group name"), "group");
		}

		[CLSCompliant (false)]
		[Obsolete ("Use UnixGroupInfo(long)")]
		public UnixGroupInfo (uint group)
		{
			this.group = new Group ();
			Group gr;
			int r = Syscall.getgrgid_r (group, this.group, out gr);
			if (r != 0 || gr == null)
				throw new ArgumentException (Locale.GetText ("invalid group id"), "group");
		}

		public UnixGroupInfo (long group)
		{
			this.group = new Group ();
			Group gr;
			int r = Syscall.getgrgid_r (Convert.ToUInt32 (group), this.group, out gr);
			if (r != 0 || gr == null)
				throw new ArgumentException (Locale.GetText ("invalid group id"), "group");
		}

		[Obsolete ("Use UnixGroupInfo(Mono.Unix.Native.Group)")]
		public UnixGroupInfo (Group group)
		{
			this.group = group;
		}

		public UnixGroupInfo (Native.Group group)
		{
			this.group = new Group ();
			this.group.gr_name    = group.gr_name;
			this.group.gr_passwd  = group.gr_passwd;
			this.group.gr_gid     = group.gr_gid;
			this.group.gr_mem     = group.gr_mem;
		}

		public string GroupName {
			get {return group.gr_name;}
		}

		public string Password {
			get {return group.gr_passwd;}
		}

		[CLSCompliant (false)]
		[Obsolete ("The type of this property will change in the next release")]
		public uint GroupId {
			get {return group.gr_gid;}
		}

		[Obsolete ("Use GetMemberNames()")]
		public string[] Members {
			get {return group.gr_mem;}
		}

		[Obsolete ("The return type will change to UnixUserInfo[] in the next release")]
		public string[] GetMembers ()
		{
			return group.gr_mem;
		}

		public string[] GetMemberNames ()
		{
			return group.gr_mem;
		}

		public override int GetHashCode ()
		{
			return group.GetHashCode ();
		}

		public override bool Equals (object obj)
		{
			if (obj == null || GetType () != obj.GetType())
				return false;
			return group.Equals (((UnixGroupInfo) obj).group);
		}

		public override string ToString ()
		{
			return group.ToString();
		}

		[Obsolete ("The return type of this method will change in the next release")]
		public Group ToGroup ()
		{
			return group;
		}

		public static UnixGroupInfo[] GetLocalGroups ()
		{
			ArrayList entries = new ArrayList ();
			lock (Syscall.grp_lock) {
				if (Native.Syscall.setgrent () != 0)
					UnixMarshal.ThrowExceptionForLastError ();
				try {
					Group g;
					while ((g = Syscall.getgrent()) != null)
						entries.Add (new UnixGroupInfo (g));
					if (Syscall.GetLastError() != (Error) 0)
						UnixMarshal.ThrowExceptionForLastError ();
				}
				finally {
					Native.Syscall.endgrent ();
				}
			}
			return (UnixGroupInfo[]) entries.ToArray (typeof(UnixGroupInfo));
		}
	}
}

// vim: noexpandtab
