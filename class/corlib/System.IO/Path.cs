//------------------------------------------------------------------------------
// 
// System.IO.Path.cs 
//
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
// 
// Author:         Jim Richardson, develop@wtfo-guru.com
// Created:        Saturday, August 11, 2001 
//
//------------------------------------------------------------------------------

using System;
using System.PAL;

namespace System.IO
{
	public class Path
	{
		private static OpSys _os = Platform.OS;

		public static readonly char AltDirectorySeparatorChar = _os.AltDirectorySeparator;
		public static readonly char DirectorySeparatorChar = _os.DirectorySeparator;
		public static readonly char[] InvalidPathChars = _os.InvalidPathChars;
		public static readonly char PathSeparator = _os.PathSeparator;
		public static readonly char VolumeSeparatorChar = _os.VolumeSeparator;

		private static readonly char[] PathSeparatorChars = {	DirectorySeparatorChar, 
									AltDirectorySeparatorChar,
									VolumeSeparatorChar };

		// class methods
		public static string ChangeExtension(string path, string extension)
		{
			if(path == null)
			{
				return null;
			}

			int iExt = findExtension(path);
			
			if(iExt < 0)
			{
				return extension == null ? path : path + extension;
			}
			else if(iExt > 0)
			{
				string temp = path.Substring(0, iExt);
				if(extension != null)
				{
					return temp + extension;
				}
				return  temp;
			}

			return extension;
		}

		[MonoTODO]
		public static string Combine(string path1, string path2)
		{
			if(path1 == null || path2 == null)
			{
				return null;
			}

			CheckArgument.Empty(path2);

			// TODO: Check for invalid DirectoryInfo characters
			//       although I don't think it is necesary for linux

			// TODO: Verify functionality further after NUnit tests written
			//		 since the documentation was rather sketchy

			if(IsPathRooted(path2))
			{
				if(path1.Equals(string.Empty))
				{
					return path2;
				}
				throw new ArgumentException("Rooted path");
			}
			
			string dirSep = new string(DirectorySeparatorChar, 1);
			string altSep = new string(AltDirectorySeparatorChar, 1);
			
			bool b1 = path1.EndsWith(dirSep) || path1.EndsWith(dirSep);
			bool b2 = path2.StartsWith(dirSep) || path2.StartsWith(altSep);
			if(b1 && b2)
			{
				throw new ArgumentException("Invalid combination");
			}
			
			if(!b1 && !b2)
			{
				return path1 + dirSep + path2;
			}

			return path1 + path2;
		}

		public static string GetDirectoryName(string path)
		{
			if(path != null)
			{
				CheckArgument.Empty(path);
				CheckArgument.WhitespaceOnly(path);
				CheckArgument.PathChars(path);

				if(path.Length > 2)
				{
					int nLast = path.LastIndexOfAny(PathSeparatorChars, path.Length - 2);

					if(nLast > 0)
					{
						return path.Substring(0, nLast);
					}
				}
			}
			return path;
		}

		public static string GetExtension(string path)
		{
			if(path == null)
			{
				return string.Empty;
			}

			CheckArgument.Empty(path);
			CheckArgument.WhitespaceOnly(path);
			
			int iExt = findExtension(path);
			int iLastSep = path.LastIndexOfAny( PathSeparatorChars );

			if(iExt > -1)
			{	// okay it has an extension
				return path.Substring(iExt);
			}
			return string.Empty;
		}

		public static string GetFileName(string path)
		{
			if(path == null)
			{
				return string.Empty;
			}

			CheckArgument.Empty(path);
			CheckArgument.WhitespaceOnly(path);

			int nLast = path.LastIndexOfAny(PathSeparatorChars);

			if(nLast > 0)
			{
				return path.Substring(nLast + 1);
			}

			return nLast == 0 ? null : path;
		}

		public static string GetFileNameWithoutExtension(string path)
		{
			return ChangeExtension(GetFileName(path), null);
		}

		public static string GetFullPath(string path)
		{
			if (path == null)
				throw(new ArgumentNullException("path", "You must specify a path when calling System.IO.Path.GetFullPath"));

			if (path.StartsWith(new string(DirectorySeparatorChar, 1)) ||
						path.StartsWith(new string(AltDirectorySeparatorChar, 1)))
				return path;

			return _os.GetCurrentDirectory() + new string(DirectorySeparatorChar, 1) + path;
		}

		public static string GetPathRoot(string path)
		{
			if(path != null || 
				(path.StartsWith(new string(DirectorySeparatorChar, 1)) ||
					path.StartsWith(new string(AltDirectorySeparatorChar, 1))))
			{
				return path.Substring(0, 1);
			}
			return null;
		}

		[MonoTODO]
		public static string GetTempFileName()
		{
			//TODO: Implement method
			return string.Empty;
		}

		/// <summary>
		/// Returns the path of the current systems temp directory
		/// </summary>
		[MonoTODO]
		public static string GetTempPath()
		{	// TODO: This might vary with distribution and there
			//       might be an api to provide it. Research is needed
			return "/tmp";
		}

		public static bool HasExtension(string path)
		{  
			CheckArgument.Null(path);
			CheckArgument.Empty(path);
			CheckArgument.WhitespaceOnly(path);
			
			return findExtension(path) > -1;
		}

		public static bool IsPathRooted(string path)
		{
			return path.StartsWith(new string(VolumeSeparatorChar,1));
		}

		// private class methods

		private static int findExtension(string path)
		{	// method should return the index of the path extension
			// start or -1 if no valid extension
			if(path != null)
			{		
				int iLastDot = path.LastIndexOf(".");
				int iLastSep = path.LastIndexOfAny( PathSeparatorChars );

				if(iLastDot > iLastSep)
				{
					return iLastDot;
				}
			}
			return -1;
		}
	}
}
