//------------------------------------------------------------------------------
// 
// System.IO.Path.cs 
//
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
// Copyright (C) 2002 Ximian, Inc. (http://www.ximian.com)
// 
// Author:         Jim Richardson, develop@wtfo-guru.com
//                 Dan Lewis (dihlewis@yahoo.co.uk)
//                 Gonzalo Paniagua Javier (gonzalo@ximian.com)
// Created:        Saturday, August 11, 2001 
//
//------------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;

namespace System.IO
{
	public sealed class Path
	{
		public static readonly char AltDirectorySeparatorChar;
		public static readonly char DirectorySeparatorChar;
		public static readonly char[] InvalidPathChars;
		public static readonly char PathSeparator;
		internal static readonly string DirectorySeparatorStr;
		public static readonly char VolumeSeparatorChar;

		private static readonly char[] PathSeparatorChars;

		private Path () {}

		// class methods
		public static string ChangeExtension (string path, string extension)
		{
			if (path == null)
			{
				return null;
			}

			if (path.IndexOfAny (InvalidPathChars) != -1)
				throw new ArgumentException ("Illegal characters in path", "path");

			int iExt = findExtension (path);

			if (extension != null) {
				if (extension [0] != '.')
					extension = "." + extension;
			} else
				extension = "";
			
			if (iExt < 0) {
				return path + extension;
			} else if (iExt > 0) {
				string temp = path.Substring (0, iExt);
				return temp + extension;
			}

			return extension;
		}

		public static string Combine (string path1, string path2)
		{
			if (path1 == null)
				throw new ArgumentNullException ("path1");

			if (path2 == null)
				throw new ArgumentNullException ("path2");

			if (path1 == String.Empty)
				return path2;

			if (path2 == String.Empty)
				return path1;

			if (path1.IndexOfAny (InvalidPathChars) != -1)
				throw new ArgumentException ("Illegal characters in path", "path1");

			if (path2.IndexOfAny (InvalidPathChars) != -1)
				throw new ArgumentException ("Illegal characters in path", "path2");

			//TODO???: UNC names
			// LAMESPEC: MS says that if path1 is not empty and path2 is a full path
			// it should throw ArgumentException
			if (IsPathRooted (path2))
				return path2;
			
			string dirSep = new string (DirectorySeparatorChar, 1);
			string altSep = new string (AltDirectorySeparatorChar, 1);
			
			bool b1 = path1.EndsWith (dirSep) ||
				  path1.EndsWith (dirSep);

			bool b2 = path2 [0] == DirectorySeparatorChar ||
				  path2 [0] == AltDirectorySeparatorChar;

			if (b1 && b2)
			{
				throw new ArgumentException ("Invalid combination");
			}
			
			if (!b1 && !b2)
			{
				return path1 + dirSep + path2;
			}

			return path1 + path2;
		}

		public static string GetDirectoryName (string path)
		{
			if (path != null)
			{
				CheckArgument.Empty (path);
				CheckArgument.WhitespaceOnly (path);
				CheckArgument.PathChars (path);

				if (path.Length > 0)
				{
					int nLast = path.LastIndexOfAny (PathSeparatorChars);

					if (nLast > 0)
						return path.Substring (0, nLast);
					else
						return String.Empty;
				} 
			}
			return path;
		}

		public static string GetExtension (string path)
		{
			if (path == null)
				return null;

			if (path.IndexOfAny (InvalidPathChars) != -1)
				throw new ArgumentException ("Illegal characters in path", "path");

			int iExt = findExtension (path);

			if (iExt > -1)
			{	// okay it has an extension
				return path.Substring (iExt);
			}
			return string.Empty;
		}

		public static string GetFileName (string path)
		{
			if (path == null || path == String.Empty)
				return path;

			if (path.IndexOfAny (InvalidPathChars) != -1)
				throw new ArgumentException ("Illegal characters in path", "path");

			int nLast = path.LastIndexOfAny (PathSeparatorChars);
			if (nLast > 0)
				return path.Substring (nLast + 1);

			return nLast == 0 ? null : path;
		}

		public static string GetFileNameWithoutExtension (string path)
		{
			return ChangeExtension (GetFileName (path), null);
		}

		public static string GetFullPath (string path)
		{
			if (path == null)
				throw (new ArgumentNullException (
					"path",
					"You must specify a path when calling System.IO.Path.GetFullPath"));
			
			if (path == String.Empty)
				throw new ArgumentException ("The path is not of a legal form", "path");

			if (path.StartsWith (new string (DirectorySeparatorChar, 1)) ||
						path.StartsWith (new string (AltDirectorySeparatorChar, 1)))
				return path;

			return Directory.GetCurrentDirectory () + new string (DirectorySeparatorChar, 1) + path;
		}

		public static string GetPathRoot (string path)
		{
			if (path == null)
				return null;

			if (!IsPathRooted (path))
				return String.Empty;

			int i = path.IndexOfAny (new char [] {DirectorySeparatorChar, AltDirectorySeparatorChar});
			if (i == -1)
				return null; // This should never happen, cause IsPathRooted returned true

			return path.Substring (0, i + 1);
		}

		public static string GetTempFileName ()
		{
			FileStream f = null;
			string path;
			Random rnd;
			int num = 0;

			rnd = new Random ();
			do {
				num = rnd.Next ();
				num++;
				path = GetTempPath() + DirectorySeparatorChar + "tmp" + num.ToString("x");

				try {
					f = new FileStream (path, FileMode.CreateNew);
				} catch {
				}
			} while (f == null);
			
			f.Close();
			return path;
		}

		/// <summary>
		/// Returns the path of the current systems temp directory
		/// </summary>
		public static string GetTempPath ()
		{
			return get_temp_path ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern string get_temp_path ();

		public static bool HasExtension (string path)
		{  
			CheckArgument.Null (path);
			CheckArgument.Empty (path);
			CheckArgument.WhitespaceOnly (path);
			
			return findExtension (path) > -1;
		}

		public static bool IsPathRooted (string path)
		{
			if (path == null || path.Length == 0)
				return false;

			if (path.IndexOfAny (InvalidPathChars) != -1)
				throw new ArgumentException ("Illegal characters in path", "path");

			char c = path [0];
			return (c == DirectorySeparatorChar 	||
				c == AltDirectorySeparatorChar 	||
				(path.Length > 1 && path [1] == VolumeSeparatorChar));
		}

		// private class methods

		private static int findExtension (string path)
		{
			// method should return the index of the path extension
			// start or -1 if no valid extension
			if (path != null){
				int iLastDot = path.LastIndexOf (".");
				int iLastSep = path.LastIndexOfAny ( PathSeparatorChars );

				if (iLastDot > iLastSep)
					return iLastDot;
			}
			return -1;
		}

		static Path () {
			VolumeSeparatorChar = MonoIO.VolumeSeparatorChar;
			DirectorySeparatorChar = MonoIO.DirectorySeparatorChar;
			AltDirectorySeparatorChar = MonoIO.AltDirectorySeparatorChar;

			PathSeparator = MonoIO.PathSeparator;
			InvalidPathChars = MonoIO.InvalidPathChars;

			// internal fields

			DirectorySeparatorStr = DirectorySeparatorChar.ToString ();
			PathSeparatorChars = new char [] {
				DirectorySeparatorChar,
				AltDirectorySeparatorChar,
				VolumeSeparatorChar
			};
		}
	}
}
