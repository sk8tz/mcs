//------------------------------------------------------------------------------
// 
// System.IO.CheckArgument.cs 
//
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
// 
// Author:         Jim Richardson, develop@wtfo-guru.com
// Created:        Saturday, August 25, 2001 
//
// NOTE: All contributors can freely add to this class or make modifications
//       that do not break existing usage of methods 
//------------------------------------------------------------------------------


using System;
using System.IO;

namespace System.IO.Private
{
	/// <summary>
	/// A utility class to assist with various argument validations in System.IO
	/// </summary>
	public sealed class CheckArgument
	{
		/// <summary>
		/// Generates and exception if arg contains whitepace only
		/// </summary>
		public static void WhitespaceOnly(string arg, string desc)
		{
			if(arg != null && arg.Length > 0)
			{
				string temp = arg;
				temp.Trim();
				if(temp.Length == 0)
				{
					throw new ArgumentException(desc);
				}
			}
		}
		
		/// <summary>
		/// Generates and exception if arg contains whitepace only
		/// </summary>
		public static void WhitespaceOnly(string arg)
		{
			WhitespaceOnly(arg, "Argument string consists of whitespace characters only.");
		}
		
		/// <summary>
		/// Generates and exception if arg is empty
		/// </summary>
		public static void Empty(string arg, string desc)
		{
			if(arg != null && arg.Length == 0)
			{
				throw new ArgumentException(desc);
			}
		}
		
		/// <summary>
		/// Generates and exception if arg is empty
		/// </summary>
		public static void Empty(string arg)
		{
			Empty(arg, "Argument string is empty.");
		}
		
		/// <summary>
		/// Generates and exception if arg is null
		/// </summary>
		public static void Null(Object arg, string desc)
		{
			if(arg == null)
			{
				throw new ArgumentNullException(desc);
			}
		}
		
		/// <summary>
		/// Generates and exception if arg is null
		/// </summary>
		public static void Null(Object arg)
		{
			if(arg == null)
			{
				throw new ArgumentNullException();
			}
		}
		
		/// <summary>
		/// Generates and exception if path contains invalid path characters
		/// </summary>
		public static void PathChars(string path, string desc)
		{
			if(path != null)
			{
				if(path.IndexOfAny(System.IO.Path.InvalidPathChars) > -1)
				{
					throw new ArgumentException(desc);
				}
			}
		}
		
		/// <summary>
		/// Generates and exception if path contains invalid path characters
		/// </summary>
		public static void PathChars(string path)
		{
			PathChars(path, "Argument contains invalid path characters");
		}
		
		/// <summary>
		/// Generates and exception if path too long
		/// </summary>
		public static void PathLength(string path, string desc)
		{
		 	//TODO: find out how long is too long
		}
		
		/// <summary>
		/// Generates and exception if path too long
		/// </summary>
		public static void PathLength(string path)
		{
			PathLength(path);
		}
		
		/// <summary>
		/// Generates and exception if path is illegal
		/// </summary>
		public static void Path(string path, bool bLength, string desc)
		{
			if(path != null) //allow null
			{
				Empty(path, desc);	// path can't be empty
				WhitespaceOnly(path, desc);	// path can't be all whitespace
				PathChars(desc);	// path can't contain invalid characters
				if(bLength)
				{
					PathLength(desc);
				}
			}
		}
		
		/// <summary>
		/// Generates and exception if path is illegal
		/// </summary>
		public static void Path(string path, bool bLength)
		{
			Path(path, bLength, "Path agrument is not legal");
		}
		
		/// <summary>
		/// Generates and exception if path is illegal
		/// </summary>
		public static void Path(string path)
		{
			Path(path, false);
		}

	}
}	// namespace System.IO.Private