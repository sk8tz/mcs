//
// System.Console.cs
//
// Author:
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.IO;
using System.Text;

namespace System
{
	public sealed class Console
	{
		private static TextWriter stdout;
		private static TextWriter stderr;
		private static TextReader stdin;

		static Console ()
		{
			int code_page = 0;
			Encoding.InternalCodePage (ref code_page);
			Encoding encoding;

			if (((int) Environment.Platform) == 128){
				//
				// On Unix systems (128), do not output the
				// UTF-8 ZWNBSP (zero-width non-breaking space).
				//
				if (code_page == UTF8Encoding.UTF8_CODE_PAGE || ((code_page & 0x10000000) != 0))
					encoding = Encoding.UTF8Unmarked;
				else
					encoding = Encoding.Default;
			} else {
				//
				// On Windows, follow the Windows tradition
				//
				encoding = Encoding.Default;
			}

			stderr = new StreamWriter (OpenStandardError (0), encoding); 
			((StreamWriter)stderr).AutoFlush = true;
			stderr = TextWriter.Synchronized (stderr);

			stdout = new StreamWriter (OpenStandardOutput (0), encoding);
			((StreamWriter)stdout).AutoFlush = true;
			stdout = TextWriter.Synchronized (stdout);

			stdin  = new StreamReader (OpenStandardInput (0), encoding);
			stdin = TextReader.Synchronized (stdin);
		}

		private Console ()
		{
		}

		public static TextWriter Error {
			get {
				return stderr;
			}
		}

		public static TextWriter Out {
			get {
				return stdout;
			}
		}

		public static TextReader In {
			get {
				return stdin;
			}
		}

		public static Stream OpenStandardError ()
		{
			return OpenStandardError (0);
		}

		public static Stream OpenStandardError (int bufferSize)
		{
			try {
				return new FileStream (MonoIO.ConsoleError, FileAccess.Write, false, bufferSize, false, bufferSize == 0);
			} catch (IOException) {
				return new NullStream ();
			}
		}

		public static Stream OpenStandardInput ()
		{
			return OpenStandardInput (0);
		}

		public static Stream OpenStandardInput (int bufferSize)
		{
			try {
				return new FileStream (MonoIO.ConsoleInput, FileAccess.Read, false, bufferSize, false, bufferSize == 0);
			} catch (IOException) {
				return new NullStream ();
			}
		}

		public static Stream OpenStandardOutput ()
		{
			return OpenStandardOutput (0);
		}

		public static Stream OpenStandardOutput (int bufferSize)
		{
			try {
				return new FileStream (MonoIO.ConsoleOutput, FileAccess.Write, false, bufferSize, false, bufferSize == 0);
			} catch (IOException) {
				return new NullStream ();
			}
		}

		public static void SetError (TextWriter newError)
		{
			if (newError == null)
				throw new ArgumentNullException ("newError");

			stderr = newError;
		}

		public static void SetIn (TextReader newIn)
		{
			if (newIn == null)
				throw new ArgumentNullException ("newIn");

			stdin = newIn;
		}

		public static void SetOut (TextWriter newOut)
		{
			if (newOut == null)
				throw new ArgumentNullException ("newOut");

			stdout = newOut;
		}

		public static void Write (bool value)
		{
			stdout.Write (value);
		}

		public static void Write (char value)
		{
			stdout.Write (value);
		}

		public static void Write (char[] value)
		{
			stdout.Write (value);
		}

		public static void Write (decimal value)
		{
			stdout.Write (value);
		}
		
		public static void Write (double value)
		{
			stdout.Write (value);
		}

		public static void Write (int value)
		{
			stdout.Write (value);
		}

		public static void Write (long value)
		{
			stdout.Write (value);
		}

		public static void Write (object value)
		{
			stdout.Write (value);
		}

		public static void Write (float value)
		{
			stdout.Write (value);
		}

		public static void Write (string value)
		{
			stdout.Write (value);
		}

		[CLSCompliant (false)]
		public static void Write (uint value)
		{
			stdout.Write (value);
		}

		[CLSCompliant (false)]
		public static void Write (ulong value)
		{
			stdout.Write (value);
		}

		public static void Write (string format, object arg0)
		{
			stdout.Write (format, arg0);
		}

		public static void Write (string format, params object[] arg)
		{
			stdout.Write (format, arg);
		}

		public static void Write (char[] buffer, int index, int count)
		{
			stdout.Write (buffer, index, count);
		}

		public static void Write (string format, object arg0, object arg1)
		{
			stdout.Write (format, arg0, arg1);
		}

		public static void Write (string format, object arg0, object arg1, object arg2 )
		{
			stdout.Write (format, arg0, arg1, arg2);
		}

		[CLSCompliant (false)]
		[MonoTODO ("Arglist is missing")]
		public static void Write (string format, object arg0, object arg1, object arg2, object arg3)
		{
			stdout.Write (String.Format (format, arg0, arg1, arg2, arg3));
		}

		public static void WriteLine ()
		{
			stdout.WriteLine ();
		}

		public static void WriteLine (bool value)
		{
			stdout.WriteLine (value);
		}

		public static void WriteLine (char value)
		{
			stdout.WriteLine (value);
		}

		public static void WriteLine (char[] value)
		{
			stdout.WriteLine (value);
		}

		public static void WriteLine (decimal value)
		{
			stdout.WriteLine (value);
		}

		public static void WriteLine (double value)
		{
			stdout.WriteLine (value);
		}

		public static void WriteLine (int value)
		{
			stdout.WriteLine (value);
		}

		public static void WriteLine (long value)
		{
			stdout.WriteLine (value);
		}

		public static void WriteLine (object value)
		{
			stdout.WriteLine (value);
		}

		public static void WriteLine (float value)
		{
			stdout.WriteLine (value);
		}

		public static void WriteLine (string value)
		{
			stdout.WriteLine (value);
		}

		[CLSCompliant (false)]
		public static void WriteLine (uint value)
		{
			stdout.WriteLine (value);
		}

		[CLSCompliant (false)]
		public static void WriteLine (ulong value)
		{
			stdout.WriteLine (value);
		}

		public static void WriteLine (string format, object arg0)
		{
			stdout.WriteLine (format, arg0);
		}

		public static void WriteLine (string format, params object[] arg)
		{
			stdout.WriteLine (format, arg);
		}

		public static void WriteLine (char[] buffer, int index, int count)
		{
			stdout.WriteLine (buffer, index, count);
		}

		public static void WriteLine (string format, object arg0, object arg1)
		{
			stdout.WriteLine (format, arg0, arg1);
		}

		public static void WriteLine (string format, object arg0, object arg1, object arg2)
		{
			stdout.WriteLine (format, arg0, arg1, arg2);
		}

		[CLSCompliant (false)]
		[MonoTODO ("Arglist is missing")]
		public static void WriteLine (string format, object arg0, object arg1, object arg2, object arg3)
		{
			stdout.WriteLine (String.Format (format, arg0, arg1, arg2, arg3));
		}

		public static int Read ()
		{
			return stdin.Read ();
		}

		public static string ReadLine ()
		{
			return stdin.ReadLine ();
		}
	}
}
