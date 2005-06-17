//
// System.Configuration.InternalConfigurationHost.cs
//
// Authors:
//  Lluis Sanchez Gual (lluis@novell.com)
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
using System.Security;
using System.Configuration.Internal;

namespace System.Configuration
{
	class InternalConfigurationHost: IInternalConfigHost
	{
		public virtual object CreateConfigurationContext (string configPath, string locationSubPath)
		{
			return null;
		}
		
		public virtual object CreateDeprecatedConfigContext (string configPath)
		{
			throw new NotImplementedException ();
		}
		
		public virtual string DecryptSection (string encryptedXml, ProtectedConfigurationProvider protectionProvider, ProtectedConfigurationSection protectedSection)
		{
			throw new NotImplementedException ();
		}
		
		public virtual void DeleteStream (string streamName)
		{
			File.Delete (streamName);
		}
		
		public virtual string EncryptSection (string encryptedXml, ProtectedConfigurationProvider protectionProvider, ProtectedConfigurationSection protectedSection)
		{
			throw new NotImplementedException ();
		}
		
		public virtual string GetConfigPathFromLocationSubPath (string configPath, string locatinSubPath)
		{
			throw new NotImplementedException ();
		}
		
		public virtual Type GetConfigType (string typeName, bool throwOnError)
		{
			throw new NotImplementedException ();
		}
		
		public virtual string GetConfigTypeName (Type t)
		{
			throw new NotImplementedException ();
		}
		
		public virtual void GetRestrictedPermissions (IInternalConfigRecord configRecord, out PermissionSet permissionSet, out bool isHostReady)
		{
			throw new NotImplementedException ();
		}
		
		public virtual string GetStreamName (string configPath)
		{
			throw new NotImplementedException ();
		}
		
		public virtual string GetStreamNameForConfigSource (string streamName, string configSource)
		{
			throw new NotImplementedException ();
		}
		
		public virtual object GetStreamVersion (string streamName)
		{
			throw new NotImplementedException ();
		}
		
		public virtual IDisposable Impersonate ()
		{
			throw new NotImplementedException ();
		}
		
		public virtual void Init (IInternalConfigRoot root, params object[] hostInitParams)
		{
		}
		
		public virtual void InitForConfiguration (ref string locationSubPath, out string configPath, out string locationConfigPath, IInternalConfigRoot root, params object[] hostInitConfigurationParams)
		{
			throw new NotImplementedException ();
		}
		
		public virtual bool IsAboveApplication (string configPath)
		{
			throw new NotImplementedException ();
		}
		
		public virtual bool IsConfigRecordRequired (string configPath)
		{
			throw new NotImplementedException ();
		}
		
		public virtual bool IsDefinitionAllowed (string configPath, ConfigurationAllowDefinition allowDefinition, ConfigurationAllowExeDefinition allowExeDefinition)
		{
			throw new NotImplementedException ();
		}
		
		public virtual bool IsFile (string streamName)
		{
			throw new NotImplementedException ();
		}
		
		public virtual bool IsLocationApplicable (string configPath)
		{
			throw new NotImplementedException ();
		}
		
		public virtual Stream OpenStreamForRead (string streamName)
		{
			if (!File.Exists (streamName))
				throw new ConfigurationException ("File '" + streamName + "' not found");
				
			return new FileStream (streamName, FileMode.Open, FileAccess.Read);
		}
		
		public virtual Stream OpenStreamForWrite (string streamName, string templateStreamName, ref object writeContext)
		{
			return new FileStream (streamName, FileMode.Create, FileAccess.Write);
		}
		
		public virtual bool PrefetchAll (string configPath, string streamName)
		{
			throw new NotImplementedException ();
		}
		
		public virtual bool PrefetchSection (string sectionGroupName, string sectionName)
		{
			throw new NotImplementedException ();
		}
		
		public virtual object StartMonitoringStreamForChanges (string streamName, StreamChangeCallback callback)
		{
			throw new NotImplementedException ();
		}
		
		public virtual void StopMonitoringStreamForChanges (string streamName, StreamChangeCallback callback)
		{
			throw new NotImplementedException ();
		}
		
		public virtual void VerifyDefinitionAllowed (string configPath, ConfigurationAllowDefinition allowDefinition, ConfigurationAllowExeDefinition allowExeDefinition, IConfigErrorInfo errorInfo)
		{
			throw new NotImplementedException ();
		}
		
		public virtual void WriteCompleted (string streamName, bool success, object writeContext)
		{
		}
		
		public virtual bool SupportsChangeNotifications {
			get { return false; }
		}
		
		public virtual bool SupportsLocation {
			get { return false; }
		}
		
		public virtual bool SupportsPath {
			get { return false; }
		}
		
		public virtual bool SupportsRefresh {
			get { return false; }
		}
	}
	
	class ExeConfigurationHost: InternalConfigurationHost
	{
		ExeConfigurationFileMap map;
		
		public override void Init (IInternalConfigRoot root, params object[] hostInitParams)
		{
			map = (ExeConfigurationFileMap) hostInitParams [0];
		}
		
		public override string GetStreamName (string configPath)
		{
			switch (configPath) {
				case "exe": return map.ExeConfigFilename; 
				case "local": return map.LocalUserConfigFilename;
				case "roaming": return map.LocalUserConfigFilename;
				case "machine": return map.MachineConfigFilename;
				default: return map.ExeConfigFilename;
			}
		}
		
		public override void InitForConfiguration (ref string locationSubPath, out string configPath, out string locationConfigPath, IInternalConfigRoot root, params object[] hostInitConfigurationParams)
		{
			map = (ExeConfigurationFileMap) hostInitConfigurationParams [0];
			configPath = null;
			string next = null;
			
			if ((locationSubPath == "exe" || locationSubPath == null) && map.ExeConfigFilename != null) {
				configPath = "exe";
				next = "local";
			}
			
			if ((locationSubPath == "local" || configPath == null) && map.LocalUserConfigFilename != null) {
				configPath = "local";
				next = "roaming";
			}
			
			if ((locationSubPath == "roaming" || configPath == null) && map.RoamingUserConfigFilename != null) {
				configPath = "roaming";
				next = "machine";
			}
			
			if ((locationSubPath == "machine" || configPath == null) && map.MachineConfigFilename != null) {
				configPath = "machine";
				next = null;
			}
			
			locationSubPath = next;
			locationConfigPath = null;
		}
	}
	
	class MachineConfigurationHost: InternalConfigurationHost
	{
		ConfigurationFileMap map;
		
		public override void Init (IInternalConfigRoot root, params object[] hostInitParams)
		{
			map = (ConfigurationFileMap) hostInitParams [0];
		}
		
		public override string GetStreamName (string configPath)
		{
			return map.MachineConfigFilename;
		}
		
		public override void InitForConfiguration (ref string locationSubPath, out string configPath, out string locationConfigPath, IInternalConfigRoot root, params object[] hostInitConfigurationParams)
		{
			map = (ConfigurationFileMap) hostInitConfigurationParams [0];
			locationSubPath = null;
			configPath = null;
			locationConfigPath = null;
		}
	}	
}

#endif
