//------------------------------------------------------------------------------
// 
// System.Environment.cs 
//
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
// 
// Author:         Jim Richardson, develop@wtfo-guru.com
// Created:        Saturday, August 11, 2001 
//
//------------------------------------------------------------------------------

using System;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Security;
using System.Security.Permissions;

namespace System
{
	public sealed class Environment
	{
		public enum SpecialFolder
		{	// TODO: Determine if these windoze style folder identifiers 
			//       have unix/linux counterparts
			ApplicationData,
			CommonApplicationData,
			CommonProgramFiles,
			Cookies,
			DesktopDirectory,
			Favorites,
			History,
			InternetCache,
			LocalApplicationData,
			Personal,
			ProgramFiles,
			Programs,
			Recent,
			SendTo,
			StartMenu,
			Startup,
			System,
			Templates
		}

		// TODO: Make sure the security attributes do what I expect
			
		/// <summary>
		/// Gets the command line for this process
		/// </summary>
		public static string CommandLine
		{	// TODO: Coordinate with implementor of EnvironmentPermissionAttribute
			[EnvironmentPermissionAttribute(SecurityAction.Demand, Read = "COMMANDLINE")]
			get
			{
				return PlatformSpecific.getCommandLine();
			}
		}

		/// <summary>
		/// Gets or sets the current directory. Actually this is supposed to get
		/// and/or set the process start directory acording to the documentation
		/// but actually test revealed at beta2 it is just Getting/Setting the CurrentDirectory
		/// </summary>
		public static string CurrentDirectory
		{
			// originally it was my thought that the external call would be made in
			// the directory class however that class has additional security requirements
			// so the Directory class will call this class for its get/set current directory
			
			[EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
			get
			{
				return PlatformSpecific.getCurrentDirectory();
			}
			[SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			set
			{
				PlatformSpecific.setCurrentDirectory(value);
			}
		}

		/// <summary>
		/// Gets or sets the exit code of this process
		/// </summary>
		public static int ExitCode
		{	// TODO: find a way to implement this property
			get
			{
				return 0;
			}
			set
			{
			}
		}

		/// <summary>
		/// Gets the name of the local computer
		/// </summary>
		public static string MachineName
		{
			get
			{
				return PlatformSpecific.getMachineName();
			}
		}

		/// <summary>
		/// Gets the standard new line value
		/// </summary>
		public static string NewLine
		{
			get
			{
				return PlatformSpecific.NewLine;
			}
		}

		/// <summary>
		/// Gets the current OS version information
		/// </summary>
		public static OperatingSystem OSVersion
		{
			get
			{
				return PlatformSpecific.getOSVersion();
			}
		}

		/// <summary>
		/// Get StackTrace
		/// </summary>
		public static string StackTrace
		{
			get
			{
				return null;
			}
		}

		/// <summary>
		/// Get a fully qualified path to the system directory
		/// </summary>
		public static string SystemDirectory
		{
			get
			{
				return GetFolderPath(SpecialFolder.System);
			}
		}

		/// <summary>
		/// Get the number of milliseconds that have elapsed since the system was booted
		/// </summary>
		public static int TickCount
		{
			get
			{
				return 0;
				//return getTickCount();
			}
		}

		/// <summary>
		/// Get UserDomainName
		/// </summary>
		public static string UserDomainName
		{
			get
			{
				return null;
			}
		}

		/// <summary>
		/// Gets a flag indicating whether the process is in interactive mode
		/// </summary>
		public static bool UserInteractive
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Get the user name of current process is running under
		/// </summary>
		public static string UserName
		{
			get
			{
				// TODO: needs more research/work/thought
				string result = GetEnvironmentVariable("USERNAME");
				if(result == null || result.Equals(string.Empty))
				{
					result = GetEnvironmentVariable("USER");
				}
				return result;
			}
		}

		/// <summary>
		/// Get the version of an assembly
		/// </summary>
		public static Version Version
		{
			get
			{
				return null;
			}
		}

		/// <summary>
		/// Get the amount of physical memory mapped to process
		/// </summary>
		public static long WorkingSet
		{
			get
			{
				return 0;
			}
		}

		public static void Exit(int exitCode)
		{ 
		}

		/// <summary>
		/// Substitute environment variables in the argument "name"
		/// </summary>
		public static string ExpandEnvironmentVariables(string name)
		{
			return name;
		}

		/// <summary>
		/// Return an array of the command line arguments of the current process
		/// </summary>
		public static string[] GetCommandLineArgs()
		{
			char[] delimiter = new char[1];
			delimiter[0] = ' ';
			return PlatformSpecific.getCommandLine().Split(delimiter);
		}

		/// <summary>
		/// Return a string containing the value of the environment
		/// variable identifed by parameter "variable"
		/// </summary>
		public static string GetEnvironmentVariable(string variable)
		{
			return (string)(getEnvironmentStrings()[variable]);
		}

		/// <summary>
		/// Return a set of all environment variables and their values
		/// </summary>
	   
		public static IDictionary getEnvironmentStrings()
		{
			// could cache these in a member variable, but that
			// wouldn't be very safe because the environment is
			// dyanamic ya know
			string strEnv = PlatformSpecific.getEnvironment();
			char[] delimiter = new char[1];
			delimiter[0] = '\t';
			string[] arEnv = strEnv.Split(delimiter);
			string[] arStr;
			Hashtable ht = new Hashtable();
			foreach(string str in arEnv)
			{
				delimiter[0] = '=';
				arStr = str.Split(delimiter, 2);
				switch(arStr.Length)
				{
				case 1:
					ht.Add(arStr[0], "");
					break;
				case 2:
					ht.Add(arStr[0], arStr[1]);
					break;
				default:
					Debug.Assert(false);	// this shouldn't happen
					break;
				}
			}
			return ht;
		}

		/// <summary>
		/// Returns the fully qualified path of the
		/// folder specified by the "folder" parameter
		/// </summary>
		public static string GetFolderPath(SpecialFolder folder)
		{
			return null;
		}

		/// <summary>
		/// Returns an array of the logical drives
		/// </summary>
		public static string[] GetLogicalDrives()
		{
			return null;
		}

	}
}
