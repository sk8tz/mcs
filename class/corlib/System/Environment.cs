//------------------------------------------------------------------------------
// 
// System.Environment.cs 
//
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
// 
// Author:         Jim Richardson, develop@wtfo-guru.com
//                 Dan Lewis (dihlewis@yahoo.co.uk)
// Created:        Saturday, August 11, 2001 
//
//------------------------------------------------------------------------------

using System;
using System.IO;
//using System.Diagnostics;
using System.Collections;
using System.Security;
using System.Security.Permissions;
using System.Runtime.CompilerServices;

namespace System
{
	public sealed class Environment
	{
		/*
		 * This is the version number of the corlib-runtime interface. When
		 * making changes to this interface (by changing the layout
		 * of classes the runtime knows about, changing icall semantics etc),
		 * increment this variable. Also increment the
		 * pair of this variable in the runtime in metadata/appdomain.c.
		 */
		private const int mono_corlib_version = 14;

		private Environment ()
		{
		}

		[MonoTODO]
		public enum SpecialFolder
		{	// TODO: Determine if these windoze style folder identifiers 
			//       have unix/linux counterparts

#if NET_1_1
			Desktop = 0x00,
			MyComputer = 0x11,
#endif
			Programs = 0x02,
			Personal = 0x05,
			Favorites = 0x06,
			Startup = 0x07,
			Recent = 0x08,
			SendTo = 0x09,
			StartMenu = 0x0b,
			MyMusic = 0x0d,
			DesktopDirectory = 0x10,
			Templates = 0x15,
			ApplicationData	= 0x1a,
			LocalApplicationData = 0x1c,
			InternetCache = 0x20,
			Cookies = 0x21,
			History = 0x22,
			CommonApplicationData	= 0x23,
			System = 0x25,
			ProgramFiles = 0x26,
			MyPictures = 0x27,
			CommonProgramFiles = 0x2b,
		}

		// TODO: Make sure the security attributes do what I expect
			
		/// <summary>
		/// Gets the command line for this process
		/// </summary>
		public static string CommandLine
		{	// TODO: Coordinate with implementor of EnvironmentPermissionAttribute
			// [EnvironmentPermissionAttribute(SecurityAction.Demand, Read = "COMMANDLINE")]
			get
			{
				// FIXME: we may need to quote, but any sane person
				// should use GetCommandLineArgs () instead.
				return String.Join (" ", GetCommandLineArgs ());
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
			
			// [EnvironmentPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
			get {
				return Directory.GetCurrentDirectory ();
			}
			[MonoTODO("disabled because of compile error. Need mcs magic.")]
			//[SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			set {
				Directory.SetCurrentDirectory (value);
			}
		}

		/// <summary>
		/// Gets or sets the exit code of this process
		/// </summary>
		public extern static int ExitCode
		{	
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			set;
		}

#if NET_1_1
		static
#endif
		public extern bool HasShutdownStarted
		{
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}
		

		/// <summary>
		/// Gets the name of the local computer
		/// </summary>
		public extern static string MachineName {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		/// <summary>
		/// Gets the standard new line value
		/// </summary>
		public extern static string NewLine {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		//
		// Support methods and fields for OSVersion property
		//
		static OperatingSystem os;

		internal static extern PlatformID Platform {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		/// <summary>
		/// Gets the current OS version information
		/// </summary>
		[MonoTODO("Correct version")]
		public static OperatingSystem OSVersion {
			get {
				if (os == null)
					os = new OperatingSystem (Platform, new Version (5,1,2600,0));

				return os;
			}
		}

		/// <summary>
		/// Get StackTrace
		/// </summary>
		public static string StackTrace {
			get {
				System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace (1);
				return trace.ToString ();
			}
		}

		/// <summary>
		/// Get a fully qualified path to the system directory
		/// </summary>
		public static string SystemDirectory {
			get {
				return GetFolderPath (SpecialFolder.System);
			}
		}

		/// <summary>
		/// Get the number of milliseconds that have elapsed since the system was booted
		/// </summary>
		public extern static int TickCount {
			[MethodImplAttribute (MethodImplOptions.InternalCall)]
			get;
		}

		/// <summary>
		/// Get UserDomainName
		/// </summary>
		public static string UserDomainName {
			get {
				return MachineName;
			}
		}

		/// <summary>
		/// Gets a flag indicating whether the process is in interactive mode
		/// </summary>
		[MonoTODO]
		public static bool UserInteractive {
			get {
				return false;
			}
		}

		/// <summary>
		/// Get the user name of current process is running under
		/// </summary>
		public static string UserName
		{
			get {
				string result = GetEnvironmentVariable ("USERNAME");

				if (result == null || result == String.Empty)
					result = GetEnvironmentVariable ("USER");

				return result;
			}
		}

		/// <summary>
		/// Get the version of the common language runtime 
		/// </summary>
		public static Version Version {
			get {
#if NET_1_1				    
				return new Version (1, 1, 4322, 573);
#else
				return new Version (1, 0, 3705, 288);
#endif
			}
		}

		/// <summary>
		/// Get the amount of physical memory mapped to process
		/// </summary>
		[MonoTODO]
		public static long WorkingSet
		{
			get {
				return 0;
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static void Exit (int exitCode);

		/// <summary>
		/// Substitute environment variables in the argument "name"
		/// </summary>
		[MonoTODO]
		public static string ExpandEnvironmentVariables(string name)
		{
			return name;
		}

		/// <summary>
		/// Return an array of the command line arguments of the current process
		/// </summary>
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static string[] GetCommandLineArgs();

		/// <summary>
		/// Return a string containing the value of the environment
		/// variable identifed by parameter "variable"
		/// </summary>
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern static string GetEnvironmentVariable (string name);

		/// <summary>
		/// Return a set of all environment variables and their values
		/// </summary>
	   
		public static IDictionary GetEnvironmentVariables()
		{
			Hashtable vars = new Hashtable ();
			foreach (string name in GetEnvironmentVariableNames ()) {
				vars [name] = GetEnvironmentVariable (name);
			}

			return vars;
		}

		/// <summary>
		/// Returns the fully qualified path of the
		/// folder specified by the "folder" parameter
		/// </summary>
		public static string GetFolderPath (SpecialFolder folder)
		{
                        string path;

                        switch (folder) {

#if NET_1_1                                
                        case SpecialFolder.MyComputer: // MyComputer is a virtual directory
                                path = "";
                                break;
                                
                        case SpecialFolder.Desktop:
#endif                                
                        case SpecialFolder.DesktopDirectory:
                                path = Path.Combine (GetEnvironmentVariable ("HOME"), "Desktop");
                                break;
                                
                         case SpecialFolder.ApplicationData:
                         case SpecialFolder.Personal:
                                 path = GetEnvironmentVariable ("HOME");
                                 break;

                        default:
                                path = "";
                                break;
                        }

                        return path;
                }

		/// <summary>
		/// Returns an array of the logical drives
		/// </summary>
		[MonoTODO]
		public static string[] GetLogicalDrives ()
		{
			return null;
		}

		static internal string GetResourceString (string s) { return ""; }

		// private methods

		private static string GacPath
		{
			get { return Path.Combine (Path.Combine (internalGetGacPath (), "mono"), "gac"); }
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static string [] GetEnvironmentVariableNames ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static string GetMachineConfigPath ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static string internalGetGacPath ();
	}
}

