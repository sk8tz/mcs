/*
 * This file was automatically generated by make-map from Mono.Posix.dll.
 *
 * DO NOT MODIFY.
 */

using System;
using System.IO;
using System.Runtime.InteropServices;
using Mono.Unix.Native;

namespace Mono.Unix.Native {

	[CLSCompliant (false)]
	public sealed /* static */ partial class NativeConvert
	{
		//
		// Non-generated exports
		//

		// convert from octal representation.
		public static FilePermissions FromOctalPermissionString (string value)
		{
			uint n = Convert.ToUInt32 (value, 8);
			return ToFilePermissions (n);
		}

		public static string ToOctalPermissionString (FilePermissions value)
		{
			string s = Convert.ToString ((int) (value & ~FilePermissions.S_IFMT), 8);
			return new string ('0', 4-s.Length) + s;
		}

		public static FilePermissions FromUnixPermissionString (string value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if (value.Length != 9 && value.Length != 10)
				throw new ArgumentException ("value", "must contain 9 or 10 characters");

			int i = 0;
			FilePermissions perms = new FilePermissions ();

			if (value.Length == 10) {
				perms |= GetUnixPermissionDevice (value [i]);
				++i;
			}

			perms |= GetUnixPermissionGroup (
				value [i++], FilePermissions.S_IRUSR,
				value [i++], FilePermissions.S_IWUSR,
				value [i++], FilePermissions.S_IXUSR,
				's', 'S', FilePermissions.S_ISUID);

			perms |= GetUnixPermissionGroup (
				value [i++], FilePermissions.S_IRGRP,
				value [i++], FilePermissions.S_IWGRP,
				value [i++], FilePermissions.S_IXGRP,
				's', 'S', FilePermissions.S_ISGID);

			perms |= GetUnixPermissionGroup (
				value [i++], FilePermissions.S_IROTH,
				value [i++], FilePermissions.S_IWOTH,
				value [i++], FilePermissions.S_IXOTH,
				't', 'T', FilePermissions.S_ISVTX);

			return perms;
		}

		private static FilePermissions GetUnixPermissionDevice (char value)
		{
			switch (value) {
			case 'd': return FilePermissions.S_IFDIR;
			case 'c': return FilePermissions.S_IFCHR;
			case 'b': return FilePermissions.S_IFBLK;
			case '-': return FilePermissions.S_IFREG;
			case 'p': return FilePermissions.S_IFIFO;
			case 'l': return FilePermissions.S_IFLNK;
			case 's': return FilePermissions.S_IFSOCK;
			}
			throw new ArgumentException ("value", "invalid device specification: " + 
				value);
		}

		private static FilePermissions GetUnixPermissionGroup (
			char read, FilePermissions readb, 
			char write, FilePermissions writeb, 
			char exec, FilePermissions execb,
			char xboth, char xbitonly, FilePermissions xbit)
		{
			FilePermissions perms = new FilePermissions ();
			if (read == 'r')
				perms |= readb;
			if (write == 'w')
				perms |= writeb;
			if (exec == 'x')
				perms |= execb;
			else if (exec == xbitonly)
				perms |= xbit;
			else if (exec == xboth)
				perms |= (execb | xbit);
			return perms;
		}

		// Create ls(1) drwxrwxrwx permissions display
		public static string ToUnixPermissionString (FilePermissions value)
		{
			char [] access = new char[] {
				'-',            // device
				'-', '-', '-',  // owner
				'-', '-', '-',  // group
				'-', '-', '-',  // other
			};
			bool have_device = true;
			switch (value & FilePermissions.S_IFMT) {
				case FilePermissions.S_IFDIR:   access [0] = 'd'; break;
				case FilePermissions.S_IFCHR:   access [0] = 'c'; break;
				case FilePermissions.S_IFBLK:   access [0] = 'b'; break;
				case FilePermissions.S_IFREG:   access [0] = '-'; break;
				case FilePermissions.S_IFIFO:   access [0] = 'p'; break;
				case FilePermissions.S_IFLNK:   access [0] = 'l'; break;
				case FilePermissions.S_IFSOCK:  access [0] = 's'; break;
				default:                        have_device = false; break;
			}
			SetUnixPermissionGroup (value, access, 1, 
				FilePermissions.S_IRUSR, FilePermissions.S_IWUSR, FilePermissions.S_IXUSR,
				's', 'S', FilePermissions.S_ISUID);
			SetUnixPermissionGroup (value, access, 4, 
				FilePermissions.S_IRGRP, FilePermissions.S_IWGRP, FilePermissions.S_IXGRP,
				's', 'S', FilePermissions.S_ISGID);
			SetUnixPermissionGroup (value, access, 7, 
				FilePermissions.S_IROTH, FilePermissions.S_IWOTH, FilePermissions.S_IXOTH,
				't', 'T', FilePermissions.S_ISVTX);
			return have_device 
				? new string (access)
				: new string (access, 1, 9);
		}

		private static void SetUnixPermissionGroup (FilePermissions value,
			char[] access, int index,
			FilePermissions read, FilePermissions write, FilePermissions exec,
			char both, char setonly, FilePermissions setxbit)
		{
			if (UnixFileSystemInfo.IsType (value, read))
				access [index] = 'r';
			if (UnixFileSystemInfo.IsType (value, write))
				access [index+1] = 'w';
			access [index+2] = GetSymbolicMode (value, exec, both, setonly, setxbit);
		}

		// Implement the GNU ls(1) permissions spec; see `info coreutils ls`,
		// section 10.1.2, the `-l' argument information.
		private static char GetSymbolicMode (FilePermissions value, 
			FilePermissions xbit, char both, char setonly, FilePermissions setxbit)
		{
			bool is_x  = UnixFileSystemInfo.IsType (value, xbit);
			bool is_sx = UnixFileSystemInfo.IsType (value, setxbit);
			
			if (is_x && is_sx)
				return both;
			if (is_sx)
				return setonly;
			if (is_x)
				return 'x';
			return '-';
		}

		public static readonly DateTime LocalUnixEpoch = 
			new DateTime (1970, 1, 1).ToLocalTime();

		public static DateTime ToDateTime (long time)
		{
			return FromTimeT (time);
		}

		public static long FromDateTime (DateTime time)
		{
			return ToTimeT (time);
		}

		public static DateTime FromTimeT (long time)
		{
			DateTime r = LocalUnixEpoch.AddSeconds (time);
			return r;
		}

		public static long ToTimeT (DateTime time)
		{
			return (long) time.Subtract (LocalUnixEpoch).TotalSeconds;
		}

		public static OpenFlags ToOpenFlags (FileMode mode, FileAccess access)
		{
			OpenFlags flags = 0;
			switch (mode) {
			case FileMode.CreateNew:
				flags = OpenFlags.O_CREAT | OpenFlags.O_EXCL;
				break;
			case FileMode.Create:
				flags = OpenFlags.O_CREAT | OpenFlags.O_TRUNC;
				break;
			case FileMode.Open:
				// do nothing
				break;
			case FileMode.OpenOrCreate:
				flags = OpenFlags.O_CREAT;
				break;
			case FileMode.Truncate:
				flags = OpenFlags.O_TRUNC;
				break;
			case FileMode.Append:
				flags = OpenFlags.O_APPEND;
				break;
			default:
				throw new ArgumentException (Locale.GetText ("Unsupported mode value"), "mode");
			}

			// Is O_LARGEFILE supported?
			int _v;
			if (TryFromOpenFlags (OpenFlags.O_LARGEFILE, out _v))
				flags |= OpenFlags.O_LARGEFILE;

			switch (access) {
			case FileAccess.Read:
				flags |= OpenFlags.O_RDONLY;
				break;
			case FileAccess.Write:
				flags |= OpenFlags.O_WRONLY;
				break;
			case FileAccess.ReadWrite:
				flags |= OpenFlags.O_RDWR;
				break;
			default:
				throw new ArgumentOutOfRangeException (Locale.GetText ("Unsupported access value"), "access");
			}

			return flags;
		}

		public static string ToFopenMode (FileAccess access)
		{
			switch (access) {
				case FileAccess.Read:       return "rb";
				case FileAccess.Write:      return "wb";
				case FileAccess.ReadWrite:  return "r+b";
				default:                    throw new ArgumentOutOfRangeException ("access");
			}
		}

		public static string ToFopenMode (FileMode mode)
		{
			switch (mode) {
				case FileMode.CreateNew: case FileMode.Create:        return "w+b";
				case FileMode.Open:      case FileMode.OpenOrCreate:  return "r+b";
				case FileMode.Truncate: return "w+b";
				case FileMode.Append:   return "a+b";
				default:                throw new ArgumentOutOfRangeException ("mode");
			}
		}

		private static readonly string[][] fopen_modes = new string[][]{
			//                                         Read                       Write ReadWrite
			/*    FileMode.CreateNew: */  new string[]{"Can't Read+Create",       "wb", "w+b"},
			/*       FileMode.Create: */  new string[]{"Can't Read+Create",       "wb", "w+b"},
			/*         FileMode.Open: */  new string[]{"rb",                      "wb", "r+b"},
			/* FileMode.OpenOrCreate: */  new string[]{"rb",                      "wb", "r+b"},
			/*     FileMode.Truncate: */  new string[]{"Cannot Truncate and Read","wb", "w+b"},
			/*       FileMode.Append: */  new string[]{"Cannot Append and Read",  "ab", "a+b"},
		};

		public static string ToFopenMode (FileMode mode, FileAccess access)
		{
			int fm = -1, fa = -1;
			switch (mode) {
				case FileMode.CreateNew:    fm = 0; break;
				case FileMode.Create:       fm = 1; break;
				case FileMode.Open:         fm = 2; break;
				case FileMode.OpenOrCreate: fm = 3; break;
				case FileMode.Truncate:     fm = 4; break;
				case FileMode.Append:       fm = 5; break;
			}
			switch (access) {
				case FileAccess.Read:       fa = 0; break;
				case FileAccess.Write:      fa = 1; break;
				case FileAccess.ReadWrite:  fa = 2; break;
			}

			if (fm == -1)
				throw new ArgumentOutOfRangeException ("mode");
			if (fa == -1)
				throw new ArgumentOutOfRangeException ("access");

			string fopen_mode = fopen_modes [fm][fa];
			if (fopen_mode [0] != 'r' && fopen_mode [0] != 'w' && fopen_mode [0] != 'a')
				throw new ArgumentException (fopen_mode);
			return fopen_mode;
		}
	}
}

// vim: noexpandtab
