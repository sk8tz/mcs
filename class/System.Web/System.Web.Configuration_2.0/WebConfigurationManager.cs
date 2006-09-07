//
// System.Web.Configuration.WebConfigurationManager.cs
//
// Authors:
// 	Lluis Sanchez Gual (lluis@novell.com)
// 	Chris Toshok (toshok@ximian.com)
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
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

#if NET_2_0

using System;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Xml;
using System.Configuration;
using System.Configuration.Internal;
using _Configuration = System.Configuration.Configuration;

namespace System.Web.Configuration {

	public static class WebConfigurationManager
	{
#if !TARGET_J2EE
		static IInternalConfigConfigurationFactory configFactory;
		static Hashtable configurations = new Hashtable ();
#else
        static internal IInternalConfigConfigurationFactory configFactory
        {
            get{
                IInternalConfigConfigurationFactory factory = (IInternalConfigConfigurationFactory)AppDomain.CurrentDomain.GetData("WebConfigurationManager.configFactory");
                if (factory == null){
                    lock (AppDomain.CurrentDomain){
                        object initialized = AppDomain.CurrentDomain.GetData("WebConfigurationManager.configFactory.initialized");
                        if (initialized == null){
                            PropertyInfo prop = typeof(ConfigurationManager).GetProperty("ConfigurationFactory", BindingFlags.Static | BindingFlags.NonPublic);
                            if (prop != null){
                                factory = prop.GetValue(null, null) as IInternalConfigConfigurationFactory;
                                configFactory = factory;
                            }
                        }
                    }
                }
                return factory != null ? factory : configFactory;
            }
            set{
                AppDomain.CurrentDomain.SetData("WebConfigurationManager.configFactory", value);
                AppDomain.CurrentDomain.SetData("WebConfigurationManager.configFactory.initialized", true);
            }
        }

        static internal Hashtable configurations
        {
            get{
                Hashtable table = (Hashtable)AppDomain.CurrentDomain.GetData("WebConfigurationManager.configurations");
                if (table == null){
                    lock (AppDomain.CurrentDomain){
                        object initialized = AppDomain.CurrentDomain.GetData("WebConfigurationManager.configurations.initialized");
                        if (initialized == null){
                            table = new Hashtable();
                            configurations = table;
                        }
                    }
                }
                return table != null ? table : configurations;

            }
            set{
                AppDomain.CurrentDomain.SetData("WebConfigurationManager.configurations", value);
                AppDomain.CurrentDomain.SetData("WebConfigurationManager.configurations.initialized", true);
            }
        }
#endif
		
		static WebConfigurationManager ()
		{
			PropertyInfo prop = typeof(ConfigurationManager).GetProperty ("ConfigurationFactory", BindingFlags.Static | BindingFlags.NonPublic);
			if (prop != null)
				configFactory = prop.GetValue (null, null) as IInternalConfigConfigurationFactory;
		}

		public static _Configuration OpenMachineConfiguration ()
		{
			return ConfigurationManager.OpenMachineConfiguration ();
		}
		
		[MonoTODO ("need to handle locationSubPath")]
		public static _Configuration OpenMachineConfiguration (string locationSubPath)
		{
			return OpenMachineConfiguration ();
		}

		[MonoTODO]
		public static _Configuration OpenMachineConfiguration (string locationSubPath,
								       string server)
		{
			if (server == null)
				return OpenMachineConfiguration (locationSubPath);

			throw new NotSupportedException ("Mono doesn't support remote configuration");
		}

		[MonoTODO]
		public static _Configuration OpenMachineConfiguration (string locationSubPath,
								       string server,
								       IntPtr userToken)
		{
			if (server == null)
				return OpenMachineConfiguration (locationSubPath);
			throw new NotSupportedException ("Mono doesn't support remote configuration");
		}

		[MonoTODO]
		public static _Configuration OpenMachineConfiguration (string locationSubPath,
								       string server,
								       string userName,
								       string password)
		{
			if (server == null)
				return OpenMachineConfiguration (locationSubPath);
			throw new NotSupportedException ("Mono doesn't support remote configuration");
		}

		public static _Configuration OpenWebConfiguration (string path)
		{
			return OpenWebConfiguration (path, null, null, null, null, null);
		}
		
		public static _Configuration OpenWebConfiguration (string path, string site)
		{
			return OpenWebConfiguration (path, site, null, null, null, null);
		}
		
		public static _Configuration OpenWebConfiguration (string path, string site, string locationSubPath)
		{
			return OpenWebConfiguration (path, site, locationSubPath, null, null, null);
		}

		[MonoTODO]
		public static _Configuration OpenWebConfiguration (string path, string site, string locationSubPath, string server)
		{
			throw new NotImplementedException ();
		}

		public static _Configuration OpenWebConfiguration (string path, string site, string locationSubPath, string server, IntPtr userToken)
		{
			return OpenWebConfiguration (path, site, locationSubPath, server, null, null);
		}
		
		[MonoTODO]
		public static _Configuration OpenWebConfiguration (string path, string site, string locationSubPath, string server, string userName, string password)
		{
			if (path == null)
				path = "";

			string basePath = GetBasePath (path);
			_Configuration conf;

			
			lock (configurations) {
				conf = (_Configuration) configurations [basePath];
				if (conf == null) {
					conf = ConfigurationFactory.Create (typeof(WebConfigurationHost), null, basePath, site, locationSubPath, server, userName, password);
					configurations [basePath] = conf;
				}
			}
			if (basePath.Length < path.Length) {
			
				// If the path has a file name, look for a location specific configuration
				
				int dif = path.Length - basePath.Length;
				string file = path.Substring (path.Length - dif);
				int i=0;
				while (i < file.Length && file [i] == '/')
					i++;
				if (i != 0)
					file = file.Substring (i);

				if (file.Length != 0) {
					foreach (ConfigurationLocation loc in conf.Locations) {
						if (loc.Path == file)
							return loc.OpenConfiguration ();
					}
				}
			}
			return conf;
		}

		public static _Configuration OpenMappedWebConfiguration (WebConfigurationFileMap fileMap, string path)
		{
			return ConfigurationFactory.Create (typeof(WebConfigurationHost), fileMap, path);
		}
		
		[MonoTODO ("Do something with the extra parameters")]
		public static _Configuration OpenMappedWebConfiguration (WebConfigurationFileMap fileMap, string path, string site)
		{
			return ConfigurationFactory.Create (typeof(WebConfigurationHost), fileMap, path, site);
		}
		
		[MonoTODO ("Do something with the extra parameters")]
		public static _Configuration OpenMappedWebConfiguration (WebConfigurationFileMap fileMap, string path, string site, string locationSubPath)
		{
			return ConfigurationFactory.Create (typeof(WebConfigurationHost), fileMap, path, site, locationSubPath);
		}
		
		public static _Configuration OpenMappedMachineConfiguration (ConfigurationFileMap fileMap)
		{
			return ConfigurationFactory.Create (typeof(WebConfigurationHost), fileMap);
		}

		[MonoTODO ("need to handle locationSubPath")]
		public static _Configuration OpenMappedMachineConfiguration (ConfigurationFileMap fileMap,
									     string locationSubPath)
		{
			return OpenMappedMachineConfiguration (fileMap);
		}

		public static object GetSection (string sectionName)
		{
			_Configuration c;
			if (HttpContext.Current != null
			    && HttpContext.Current.Request != null)
				c = OpenWebConfiguration (HttpContext.Current.Request.Path);
			else
				c = OpenWebConfiguration (HttpRuntime.AppDomainAppVirtualPath);

			if (c == null)
				return null;
			else
				return c.GetSection (sectionName);
		}

		[MonoTODO]
		public static object GetSection (string sectionName, string path)
		{
			try {
				_Configuration c = OpenWebConfiguration (path);
				return c.GetSection (sectionName);
			}
			catch {
				return null;
			}
		}

		static _Configuration GetWebApplicationConfiguration ()
		{
			_Configuration config;

			if (HttpContext.Current == null
			    || HttpContext.Current.Request == null
			    || HttpContext.Current.Request.ApplicationPath == null
			    || HttpContext.Current.Request.ApplicationPath == "") {
				config = OpenWebConfiguration ("");
			}
			else {
				config = OpenWebConfiguration (HttpContext.Current.Request.ApplicationPath);
			}

			return config;
		}

		static MethodInfo get_runtime_object = typeof (ConfigurationSection).GetMethod ("GetRuntimeObject", BindingFlags.NonPublic | BindingFlags.Instance);

		[MonoTODO]
		public static object GetWebApplicationSection (string sectionName)
		{
			_Configuration config = GetWebApplicationConfiguration ();
			if (config == null)
				return null;

			ConfigurationSection section = config.GetSection (sectionName);
			if (section == null)
				return null;
			
			return get_runtime_object.Invoke (section, new object [0]);
		}

		public static NameValueCollection AppSettings {
			get { return ConfigurationManager.AppSettings; }
		}

		public static ConnectionStringSettingsCollection ConnectionStrings {
			get { return ConfigurationManager.ConnectionStrings; }
		}

		internal static IInternalConfigConfigurationFactory ConfigurationFactory {
			get { return configFactory; }
		}
		
		static string GetBasePath (string path)
		{
 			if (path == "/" || path == "")
				return path;

			/* first if we can, map it to a physical path
			 * to see if it corresponds to a file */
			if (HttpContext.Current != null
			    && HttpContext.Current.Request != null) {
				string pd = HttpContext.Current.Request.MapPath (path);

				if (!Directory.Exists (pd)) {
					/* if it does, remove the file from the url */
					int i = path.LastIndexOf ('/');
					path = path.Substring (0, i);
				} 
			}
			
			if (path.Length == 0)
				return path;

			/* remove excess /'s from the end of the virtual path */
			while (path [path.Length - 1] == '/')
				path = path.Substring (0, path.Length - 1);

			return path;
		}


#region stuff copied from WebConfigurationSettings
#if TARGET_J2EE
		static internal IConfigurationSystem oldConfig {
			get {
				return (IConfigurationSystem)AppDomain.CurrentDomain.GetData("WebConfigurationManager.oldConfig");
			}
			set {
				AppDomain.CurrentDomain.SetData("WebConfigurationManager.oldConfig", value);
			}
		}

		static private Web20DefaultConfig config {
			get {
				return (Web20DefaultConfig) AppDomain.CurrentDomain.GetData ("Web20DefaultConfig.config");
			}
			set {
				AppDomain.CurrentDomain.SetData ("Web20DefaultConfig.config", value);
			}
		}

		static private IInternalConfigSystem configSystem {
			get {
				return (IInternalConfigSystem) AppDomain.CurrentDomain.GetData ("IInternalConfigSystem.configSystem");
			}
			set {
				AppDomain.CurrentDomain.SetData ("IInternalConfigSystem.configSystem", value);
			}
		}
#else
		static internal IConfigurationSystem oldConfig;
		static Web20DefaultConfig config;
		static IInternalConfigSystem configSystem;
#endif
		const BindingFlags privStatic = BindingFlags.NonPublic | BindingFlags.Static;
		static readonly object lockobj = new object ();

		internal static void Init ()
		{
			lock (lockobj) {
				if (config != null)
					return;

				/* deal with the ConfigurationSettings stuff */
				{
					Web20DefaultConfig settings = Web20DefaultConfig.GetInstance ();
					Type t = typeof (ConfigurationSettings);
					MethodInfo changeConfig = t.GetMethod ("ChangeConfigurationSystem",
									       privStatic);

					if (changeConfig == null)
						throw new ConfigurationException ("Cannot find method CCS");

					object [] args = new object [] {settings};
					oldConfig = (IConfigurationSystem)changeConfig.Invoke (null, args);
					config = settings;

					config.Init ();
				}

				/* deal with the ConfigurationManager stuff */
				{
					HttpConfigurationSystem system = new HttpConfigurationSystem ();
					Type t = typeof (ConfigurationManager);
					MethodInfo changeConfig = t.GetMethod ("ChangeConfigurationSystem",
									       privStatic);

					if (changeConfig == null)
						throw new ConfigurationException ("Cannot find method CCS");

					object [] args = new object [] {system};
					changeConfig.Invoke (null, args);
					configSystem = system;
				}
			}
		}
	}

	class Web20DefaultConfig : IConfigurationSystem
	{
#if TARGET_J2EE
		static private Web20DefaultConfig instance {
			get {
				Web20DefaultConfig val = (Web20DefaultConfig)AppDomain.CurrentDomain.GetData("Web20DefaultConfig.instance");
				if (val == null) {
					val = new Web20DefaultConfig();
					AppDomain.CurrentDomain.SetData("Web20DefaultConfig.instance", val);
				}
				return val;
			}
			set {
				AppDomain.CurrentDomain.SetData("Web20DefaultConfig.instance", value);
			}
		}
#else
		static Web20DefaultConfig instance;
#endif

		static Web20DefaultConfig ()
		{
			instance = new Web20DefaultConfig ();
		}

		public static Web20DefaultConfig GetInstance ()
		{
			return instance;
		}

		public object GetConfig (string sectionName)
		{
			object o = WebConfigurationManager.GetWebApplicationSection (sectionName);

			if (o == null || o is IgnoreSection) {
				/* this can happen when the section
				 * handler doesn't subclass from
				 * ConfigurationSection.  let's be
				 * nice and try to load it using the
				 * 1.x style routines in case there's
				 * a 1.x section handler registered
				 * for it.
				 */
				object o1 = WebConfigurationManager.oldConfig.GetConfig (sectionName);
				if (o1 != null)
					return o1;
			}

			return o;
		}

		public void Init ()
		{
			// nothing. We need a context.
		}
	}

#endregion
}

#endif
