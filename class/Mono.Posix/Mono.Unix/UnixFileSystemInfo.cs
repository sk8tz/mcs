//
// Mono.Unix/UnixFileSystemInfo.cs
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
using System.IO;
using System.Text;
using Mono.Unix;

namespace Mono.Unix {

	public abstract class UnixFileSystemInfo
	{
		private Native.Stat stat;
		private string fullPath;
		private string originalPath;
		private bool valid = false;

		protected UnixFileSystemInfo (string path)
		{
			UnixPath.CheckPath (path);
			this.originalPath = path;
			this.fullPath = UnixPath.GetFullPath (path);
			Refresh (true);
		}

		internal UnixFileSystemInfo (String path, Native.Stat stat)
		{
			this.originalPath = path;
			this.fullPath = UnixPath.GetFullPath (path);
			this.stat = stat;
			this.valid = true;
		}

		protected string FullPath {
			get {return fullPath;}
			set {fullPath = value;}
		}

		protected string OriginalPath {
			get {return originalPath;}
			set {originalPath = value;}
		}

		private void AssertValid ()
		{
			Refresh (false);
			if (!valid)
				throw new InvalidOperationException ("Path doesn't exist!");
		}

		public virtual string FullName {
			get {return FullPath;}
		}

		public abstract string Name {get;}

		public bool Exists {
			get {
				Refresh (true);
				return valid;
			}
		}

		public long Device {
			get {AssertValid (); return Convert.ToInt64 (stat.st_dev);}
		}

		public long Inode {
			get {AssertValid (); return Convert.ToInt64 (stat.st_ino);}
		}

		[CLSCompliant (false)]
		[Obsolete ("Use Protection.")]
		public FilePermissions Mode {
			get {AssertValid (); return (FilePermissions) stat.st_mode;}
		}

		[CLSCompliant (false)]
		[Obsolete ("Use FileAccessPermissions.")]
		public FilePermissions Permissions {
			get {AssertValid (); return (FilePermissions) stat.st_mode & ~FilePermissions.S_IFMT;}
		}

		[CLSCompliant (false)]
		public Native.FilePermissions Protection {
			get {AssertValid (); return (Native.FilePermissions) stat.st_mode;}
			set {
				int r = Native.Syscall.chmod (FullPath, value);
				UnixMarshal.ThrowExceptionForLastErrorIf (r);
			}
		}

		public FileTypes FileType {
			get {
				AssertValid ();
				int type = (int) stat.st_mode;
				return (FileTypes) (type & (int) FileTypes.AllTypes);
			}
			// no set as chmod(2) won't accept changing the file type.
		}

		public FileAccessPermissions FileAccessPermissions {
			get {
				AssertValid (); 
				int perms = (int) stat.st_mode;
				return (FileAccessPermissions) (perms & (int) FileAccessPermissions.AllPermissions);
			}
			set {
				AssertValid ();
				int perms = (int) stat.st_mode;
				perms &= (int) ~FileAccessPermissions.AllPermissions;
				perms |= (int) value;
				Protection = (Native.FilePermissions) perms;
			}
		}

		public FileSpecialAttributes FileSpecialAttributes {
			get {
				AssertValid ();
				int attrs = (int) stat.st_mode;
				return (FileSpecialAttributes) (attrs & (int) FileSpecialAttributes.AllAttributes);
			}
			set {
				AssertValid ();
				int perms = (int) stat.st_mode;
				perms &= (int) ~FileSpecialAttributes.AllAttributes;
				perms |= (int) value;
				Protection = (Native.FilePermissions) perms;
			}
		}

		public long LinkCount {
			get {AssertValid (); return Convert.ToInt64 (stat.st_nlink);}
		}

		public UnixUserInfo OwnerUser {
			get {AssertValid (); return new UnixUserInfo (stat.st_uid);}
		}

		public long OwnerUserId {
			get {AssertValid (); return stat.st_uid;}
		}

		public UnixGroupInfo OwnerGroup {
			get {AssertValid (); return new UnixGroupInfo (stat.st_gid);}
		}

		public long OwnerGroupId {
			get {AssertValid (); return stat.st_gid;}
		}

		public long DeviceType {
			get {AssertValid (); return Convert.ToInt64 (stat.st_rdev);}
		}

		public long Length {
			get {AssertValid (); return (long) stat.st_size;}
		}

		public long BlockSize {
			get {AssertValid (); return (long) stat.st_blksize;}
		}

		public long BlocksAllocated {
			get {AssertValid (); return (long) stat.st_blocks;}
		}

		public DateTime LastAccessTime {
			get {AssertValid (); return Native.NativeConvert.ToDateTime (stat.st_atime);}
		}

		public DateTime LastAccessTimeUtc {
			get {return LastAccessTime.ToUniversalTime ();}
		}

		public DateTime LastWriteTime {
			get {AssertValid (); return Native.NativeConvert.ToDateTime (stat.st_mtime);}
		}

		public DateTime LastWriteTimeUtc {
			get {return LastWriteTime.ToUniversalTime ();}
		}

		public DateTime LastStatusChangeTime {
			get {AssertValid (); return Native.NativeConvert.ToDateTime (stat.st_ctime);}
		}

		public DateTime LastStatusChangeTimeUtc {
			get {return LastStatusChangeTime.ToUniversalTime ();}
		}

		public bool IsDirectory {
			get {AssertValid (); return IsType (stat.st_mode, Native.FilePermissions.S_IFDIR);}
		}

		public bool IsCharacterDevice {
			get {AssertValid (); return IsType (stat.st_mode, Native.FilePermissions.S_IFCHR);}
		}

		public bool IsBlockDevice {
			get {AssertValid (); return IsType (stat.st_mode, Native.FilePermissions.S_IFBLK);}
		}

		[Obsolete ("Use IsRegularFile")]
		public bool IsFile {
			get {AssertValid (); return IsType (stat.st_mode, Native.FilePermissions.S_IFREG);}
		}

		public bool IsRegularFile {
			get {AssertValid (); return IsType (stat.st_mode, Native.FilePermissions.S_IFREG);}
		}

		[Obsolete ("Use IsFifo")]
		[CLSCompliant (false)]
		public bool IsFIFO {
			get {AssertValid (); return IsType (stat.st_mode, Native.FilePermissions.S_IFIFO);}
		}

		public bool IsFifo {
			get {AssertValid (); return IsType (stat.st_mode, Native.FilePermissions.S_IFIFO);}
		}

		public bool IsSymbolicLink {
			get {AssertValid (); return IsType (stat.st_mode, Native.FilePermissions.S_IFLNK);}
		}

		public bool IsSocket {
			get {AssertValid (); return IsType (stat.st_mode, Native.FilePermissions.S_IFSOCK);}
		}

		public bool IsSetUser {
			get {AssertValid (); return IsType (stat.st_mode, Native.FilePermissions.S_ISUID);}
		}

		public bool IsSetGroup {
			get {AssertValid (); return IsType (stat.st_mode, Native.FilePermissions.S_ISGID);}
		}

		public bool IsSticky {
			get {AssertValid (); return IsType (stat.st_mode, Native.FilePermissions.S_ISVTX);}
		}

		[Obsolete ("Use IsType(Native.FilePermissions, Native.FilePermissions)", true)]
		internal static bool IsType (FilePermissions mode, FilePermissions type)
		{
			return (mode & type) == type;
		}

		internal static bool IsType (Native.FilePermissions mode, Native.FilePermissions type)
		{
			return (mode & type) == type;
		}

		[CLSCompliant (false)]
		[Obsolete ("Use CanAccess (Mono.Unix.Native.AccessModes)", true)]
		public bool CanAccess (AccessMode mode)
		{
			int r = Syscall.access (FullPath, mode);
			return r == 0;
		}

		[CLSCompliant (false)]
		public bool CanAccess (Native.AccessModes mode)
		{
			int r = Native.Syscall.access (FullPath, mode);
			return r == 0;
		}

		public UnixFileSystemInfo CreateLink (string path)
		{
			int r = Native.Syscall.link (FullName, path);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
			return Create (path);
		}

		public UnixSymbolicLinkInfo CreateSymbolicLink (string path)
		{
			int r = Native.Syscall.symlink (FullName, path);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
			return new UnixSymbolicLinkInfo (path);
		}

		public abstract void Delete ();

		[CLSCompliant (false)]
		[Obsolete ("Use GetConfigurationValue (Mono.Unix.Native.PathconfName)", true)]
		public long GetConfigurationValue (PathConf name)
		{
			long r = Syscall.pathconf (FullPath, name);
			if (r == -1 && Syscall.GetLastError() != (Error) 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return r;
		}

		[CLSCompliant (false)]
		public long GetConfigurationValue (Native.PathconfName name)
		{
			long r = Native.Syscall.pathconf (FullPath, name);
			if (r == -1 && Native.Stdlib.GetLastError() != (Native.Errno) 0)
				UnixMarshal.ThrowExceptionForLastError ();
			return r;
		}

		public void Refresh ()
		{
			Refresh (true);
		}

		internal void Refresh (bool force)
		{
			if (valid && !force)
				return;
			int r = GetFileStatus (FullPath, out this.stat);
			valid = r == 0;
		}

		protected virtual int GetFileStatus (string path, out Native.Stat stat)
		{
			return Native.Syscall.stat (path, out stat);
		}

		public void SetLength (long length)
		{
			int r;
			do {
				r = Native.Syscall.truncate (FullPath, length);
			}	while (UnixMarshal.ShouldRetrySyscall (r));
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		[CLSCompliant (false)]
		[Obsolete ("Use Protection setter", true)]
		public void SetPermissions (FilePermissions perms)
		{
			int r = Syscall.chmod (FullPath, perms);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		[CLSCompliant (false)]
		[Obsolete ("Use SetOwner (long, long)", true)]
		public virtual void SetOwner (uint owner, uint group)
		{
			int r = Syscall.chown (FullPath, owner, group);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public virtual void SetOwner (long owner, long group)
		{
			uint _owner = Convert.ToUInt32 (owner);
			uint _group = Convert.ToUInt32 (group);
			int r = Native.Syscall.chown (FullPath, _owner, _group);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);
		}

		public void SetOwner (string owner)
		{
			Native.Passwd pw = Native.Syscall.getpwnam (owner);
			if (pw == null)
				throw new ArgumentException (Locale.GetText ("invalid username"), "owner");
			uint uid = pw.pw_uid;
			uint gid = pw.pw_gid;
			SetOwner ((long) uid, (long) gid);
		}

		public void SetOwner (string owner, string group)
		{
			long uid = new UnixUserInfo (owner).UserId;
			long gid = new UnixGroupInfo (group).GroupId;

			SetOwner (uid, gid);
		}

		public override string ToString ()
		{
			return FullPath;
		}

		public Native.Stat ToStat ()
		{
			return stat;
		}

		internal static UnixFileSystemInfo Create (string path)
		{
			Native.Stat stat;
			int r = Native.Syscall.lstat (path, out stat);
			UnixMarshal.ThrowExceptionForLastErrorIf (r);

			if (IsType (stat.st_mode, Native.FilePermissions.S_IFDIR))
				return new UnixDirectoryInfo (path, stat);
			else if (IsType (stat.st_mode, Native.FilePermissions.S_IFLNK))
				return new UnixSymbolicLinkInfo (path, stat);
			return new UnixFileInfo (path, stat);
		}
	}
}

// vim: noexpandtab
