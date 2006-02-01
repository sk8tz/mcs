//
// System.Configuration.ConfigurationManager.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
// 	Lluis Sanchez Gual (lluis@novell.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
#if NET_2_0
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;
using System.Reflection;
using System.Xml;
using System.IO;
using System.Text;
using System.Configuration.Internal;

namespace System.Configuration {

	/*roaming user config path: C:\Documents and Settings\toshok\Application Data\domain-System.Configurati_Url_py3nlovv3wxe21qgacxc3n2b1mph2log\1.0.0.0\user.config */

	public static class ConfigurationManager
	{
		static InternalConfigurationFactory configFactory = new InternalConfigurationFactory ();
		static IInternalConfigSystem configSystem = new ClientConfigurationSystem ();
		static object lockobj = new object ();

		[MonoTODO ("Evidence and version still needs work")]
		static string GetAssemblyInfo (Assembly a)
		{
			object[] attrs;
			StringBuilder sb;

			string app_name;
			string evidence_str;
			string version;

			attrs = a.GetCustomAttributes (typeof (AssemblyProductAttribute), false);
			if (attrs != null && attrs.Length > 0)
				app_name = ((AssemblyProductAttribute)attrs[0]).Product;
			else
				app_name = AppDomain.CurrentDomain.FriendlyName;

			sb = new StringBuilder();

			sb.Append ("evidencehere");

			evidence_str = sb.ToString();

			attrs = a.GetCustomAttributes (typeof (AssemblyVersionAttribute), false);
			if (attrs != null && attrs.Length > 0)
				version = ((AssemblyVersionAttribute)attrs[0]).Version;
			else
				version = "1.0.0.0" /* XXX */;


			return Path.Combine (String.Format ("{0}_{1}", app_name, evidence_str), version);
		}

		internal static Configuration OpenExeConfigurationInternal (ConfigurationUserLevel userLevel, Assembly calling_assembly, string exePath)
		{
			if (calling_assembly == null && exePath == null)
				throw new ArgumentException ("exePath must be specified when not running inside a stand alone exe.");

			ExeConfigurationFileMap map = new ExeConfigurationFileMap ();

			/* Roaming and RoamingAndLocal should be different

			On windows,
			  PerUserRoaming = \Documents and Settings\<username>\Application Data\...
			  PerUserRoamingAndLocal = \Documents and Settings\<username>\Local Settings\Application Data\...
			*/

			switch (userLevel) {
			case ConfigurationUserLevel.None:
				if (exePath == null)
					exePath = calling_assembly.Location;
				else if (!File.Exists (exePath))
					exePath = "";

				if (exePath != "") {
					if (!exePath.EndsWith (".config"))
						map.ExeConfigFilename = exePath + ".config";
					else
						map.ExeConfigFilename = exePath;
				}
				break;
			case ConfigurationUserLevel.PerUserRoaming:
				map.RoamingUserConfigFilename = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), GetAssemblyInfo(calling_assembly));
				map.RoamingUserConfigFilename = Path.Combine (map.RoamingUserConfigFilename, "user.config");
				goto case ConfigurationUserLevel.PerUserRoamingAndLocal;

			case ConfigurationUserLevel.PerUserRoamingAndLocal:
				map.LocalUserConfigFilename = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData), GetAssemblyInfo(calling_assembly));
				map.LocalUserConfigFilename = Path.Combine (map.LocalUserConfigFilename, "user.config");
				break;
			}

			return ConfigurationFactory.Create (typeof(ExeConfigurationHost), map);
		}

		public static Configuration OpenExeConfiguration (ConfigurationUserLevel userLevel)
		{
			return OpenExeConfigurationInternal (userLevel, Assembly.GetEntryAssembly (), null);
		}
		
		public static Configuration OpenExeConfiguration (string exePath)
		{
			return OpenExeConfigurationInternal (ConfigurationUserLevel.None, null, exePath);
		}

		[MonoTODO ("userLevel")]
		public static Configuration OpenMappedExeConfiguration (ExeConfigurationFileMap fileMap, ConfigurationUserLevel userLevel)
		{
			return ConfigurationFactory.Create (typeof(ExeConfigurationHost), fileMap);
		}

		public static Configuration OpenMachineConfiguration ()
		{
			ConfigurationFileMap map = new ConfigurationFileMap ();
			return ConfigurationFactory.Create (typeof(MachineConfigurationHost), map);
		}
		
		public static Configuration OpenMappedMachineConfiguration (ConfigurationFileMap fileMap)
		{
			return ConfigurationFactory.Create (typeof(MachineConfigurationHost), fileMap);
		}
		
		internal static IInternalConfigConfigurationFactory ConfigurationFactory {
			get { return configFactory; }
		}

		internal static IInternalConfigSystem ConfigurationSystem {
			get { return configSystem; }
		}

		public static object GetSection (string sectionName)
		{
			return ConfigurationSystem.GetSection (sectionName);
		}

		public static void RefreshSection (string sectionName)
		{
			ConfigurationSystem.RefreshConfig (sectionName);
		}

		public static NameValueCollection AppSettings {
			get {
				AppSettingsSection appsettings = (AppSettingsSection) GetSection ("appSettings");
				return (NameValueCollection)appsettings.GetRuntimeObject ();
			}
		}

		[MonoTODO]
		public static ConnectionStringSettingsCollection ConnectionStrings {
			get {
				ConnectionStringsSection connectionStrings = (ConnectionStringsSection) GetSection ("connectionStrings");
				return connectionStrings.ConnectionStrings;
			}
		}

		/* invoked from System.Web */
		static IInternalConfigSystem ChangeConfigurationSystem (IInternalConfigSystem newSystem)
		{
			if (newSystem == null)
				throw new ArgumentNullException ("newSystem");

			lock (lockobj) {
				IInternalConfigSystem old = configSystem;
				configSystem = newSystem;
				return old;
			}
		}
	}
}

#endif
