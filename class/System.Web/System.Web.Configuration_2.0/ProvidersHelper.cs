//
// System.Web.Configuration.ProvidersHelper
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
//

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

#if NET_2_0

using System;
using System.Collections;
using System.Configuration;
using System.Configuration.Provider;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Web.Compilation;

namespace System.Web.Configuration {

	public static class ProvidersHelper
	{
		private static string privateBinPath = null;
		
		public static ProviderBase InstantiateProvider (ProviderSettings providerSettings, Type providerType)
		{
			Type settingsType = Type.GetType (providerSettings.Type);
			if (settingsType == null && Directory.Exists (PrivateBinPath)) {
				string [] binDlls = Directory.GetFiles (PrivateBinPath, "*.dll");
				foreach (string s in binDlls) {
					Assembly binA = Assembly.LoadFrom (s);
					settingsType = binA.GetType (providerSettings.Type);
					if (settingsType != null)
						break;
				}
			}

			// check App_Code dlls
			if (settingsType == null) {
				IList appCode = BuildManager.CodeAssemblies;

				if (appCode != null && appCode.Count > 0) {
					Assembly asm;
					foreach (object o in appCode) {
						asm = o as Assembly;
						if (asm == null)
							continue;
						settingsType = asm.GetType (providerSettings.Type);
						if (settingsType != null)
							break;
					}
				}
			}

			if (settingsType == null)
				throw new ConfigurationErrorsException (String.Format ("Could not find type: {0}",
										       providerSettings.Type));
			if (!providerType.IsAssignableFrom (settingsType))
				throw new ConfigurationErrorsException (String.Format ("Provider '{0}' must subclass from '{1}'",
										       providerSettings.Name, providerType));

			ProviderBase provider = Activator.CreateInstance (settingsType) as ProviderBase;

			provider.Initialize (providerSettings.Name, providerSettings.Parameters);

			return provider;
		}

		private static string PrivateBinPath {
			get {
				if (privateBinPath != null)
					return privateBinPath;

				AppDomainSetup setup = AppDomain.CurrentDomain.SetupInformation;
				privateBinPath = Path.Combine (setup.ApplicationBase, setup.PrivateBinPath);

				return privateBinPath;
			}
		}

		public static void InstantiateProviders (ProviderSettingsCollection configProviders, ProviderCollection providers, Type providerType)
		{
			if (!typeof (ProviderBase).IsAssignableFrom (providerType))
				throw new ConfigurationErrorsException (String.Format ("type '{0}' must subclass from ProviderBase", providerType));

			foreach (ProviderSettings settings in configProviders)
				providers.Add (InstantiateProvider (settings, providerType));
		}

		internal static DbProviderFactory GetDbProviderFactory (string providerName)
		{
			DbProviderFactory f = null;

			if (providerName != null && providerName != "") {
				try {
					f = DbProviderFactories.GetFactory(providerName);
				}
				catch (Exception e) { Console.WriteLine (e); /* nada */ }
				if (f != null)
					return f;
			}

			return SqlClientFactory.Instance;
		}
	}

}

#endif

