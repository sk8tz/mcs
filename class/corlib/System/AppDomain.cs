//
// System/AppDomain.cs
//
// Authors:
//   Paolo Molaro (lupus@ximian.com)
//   Dietmar Maurer (dietmar@ximian.com)
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001, 2002 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Security.Principal;
using System.Security.Policy;
using System.Security;

namespace System {

	public sealed class AppDomain : MarshalByRefObject , _AppDomain , IEvidenceFactory {

		IntPtr _mono_app_domain;

		// Evidence evidence;
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern AppDomainSetup getSetup ();

		public AppDomainSetup SetupInformation {

			get {
				return getSetup ();
			}
		}

		public string BaseDirectory {

			get {
				return SetupInformation.ApplicationBase;
			}
		}

		public string RelativeSearchPath {

			get {
				return SetupInformation.PrivateBinPath;
			}
		}

		public string DynamicDirectory {

			get {
				// fixme: dont know if this is right?
				return SetupInformation.DynamicBase;
			}
		}

		public bool ShadowCopyFiles {

			get {
				if (SetupInformation.ShadowCopyFiles == "true")
					return true;
				return false;
			}
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern string getFriendlyName ();

		public string FriendlyName {

			get {
				return getFriendlyName ();
			}
		}

		public Evidence Evidence {

			get {
				return null;
				//return evidence;
			}
		}
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern AppDomain getCurDomain ();
		
		public static AppDomain CurrentDomain
		{
			get {
				return getCurDomain ();
			}
		}

		[MonoTODO]
		public void AppendPrivatePath (string path)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void ClearPrivatePath ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void ClearShadowCopyPath ()
		{
			throw new NotImplementedException ();
		}

		public ObjectHandle CreateInstance (string assemblyName, string typeName)
		{
			return CreateInstance (assemblyName, typeName, false, 0,
					       null, null, null, null, null);
		}

		public ObjectHandle CreateInstance (string assemblyName, string typeName,
						    object[] activationAttributes)
		{
			return CreateInstance (assemblyName, typeName, false, 0,
					       null, null, null, activationAttributes, null);
		}
		
		[MonoTODO]
		public ObjectHandle CreateInstance (string assemblyName,
						    string typeName,
						    bool ignoreCase,
						    BindingFlags bindingAttr,
						    Binder binder,
						    object[] args,
						    CultureInfo culture,
						    object[] activationAttributes,
						    Evidence securityAttribtutes)
		{
			throw new NotImplementedException ();
		}

		public ObjectHandle CreateInstanceFrom (string assemblyName, string typeName)
		{
			return CreateInstanceFrom (assemblyName, typeName, false, 0,
						   null, null, null, null, null);
		}
		
		public ObjectHandle CreateInstanceFrom (string assemblyName, string typeName,
							object[] activationAttributes)
		{
			return CreateInstanceFrom (assemblyName, typeName, false, 0,
						   null, null, null, activationAttributes, null);
		}
		
		[MonoTODO]
		public ObjectHandle CreateInstanceFrom (string assemblyName,
							string typeName,
							bool ignoreCase,
							BindingFlags bindingAttr,
							Binder binder,
							object[] args,
							CultureInfo culture,
							object[] activationAttributes,
							Evidence securityAttribtutes)
		{
			throw new NotImplementedException ();			
		}

		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name,
							      AssemblyBuilderAccess access)
		{
			return DefineDynamicAssembly (name, access, null, null,
						      null, null, null, false);
		}

		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name,
							      AssemblyBuilderAccess access,
							      Evidence evidence)
		{
			return DefineDynamicAssembly (name, access, null, evidence,
						      null, null, null, false);
		}
		
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name,
							      AssemblyBuilderAccess access,
							      string dir)
		{
			return DefineDynamicAssembly (name, access, dir, null,
						      null, null, null, false);
		}
		
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name,
							      AssemblyBuilderAccess access,
							      string dir,
							      Evidence evidence)
		{
			return DefineDynamicAssembly (name, access, dir, evidence,
						      null, null, null, false);
		}
		
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name,
							      AssemblyBuilderAccess access,
							      PermissionSet requiredPermissions,
							      PermissionSet optionalPermissions,
							      PermissionSet refusedPersmissions)
		{
			return DefineDynamicAssembly (name, access, null, null,
						      requiredPermissions, optionalPermissions,
						      refusedPersmissions, false);
		}
		
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name,
							      AssemblyBuilderAccess access,
							      Evidence evidence,
							      PermissionSet requiredPermissions,
							      PermissionSet optionalPermissions,
							      PermissionSet refusedPersmissions)
		{
			return DefineDynamicAssembly (name, access, null, evidence,
						      requiredPermissions, optionalPermissions,
						      refusedPersmissions, false);
		}
		
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name,
							      AssemblyBuilderAccess access,
							      string dir,
							      PermissionSet requiredPermissions,
							      PermissionSet optionalPermissions,
							      PermissionSet refusedPersmissions)
		{
			return DefineDynamicAssembly (name, access, dir, null,
						      requiredPermissions, optionalPermissions,
						      refusedPersmissions, false);
		}
		
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name,
							      AssemblyBuilderAccess access,
							      string dir,
							      Evidence evidence,
							      PermissionSet requiredPermissions,
							      PermissionSet optionalPermissions,
							      PermissionSet refusedPersmissions)
		{
			return DefineDynamicAssembly (name, access, dir, evidence,
						      requiredPermissions, optionalPermissions,
						      refusedPersmissions, false);

		}
		
		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name,
							      AssemblyBuilderAccess access,
							      string dir,
							      Evidence evidence,
							      PermissionSet requiredPermissions,
							      PermissionSet optionalPermissions,
							      PermissionSet refusedPersmissions,
							      bool isSynchronized)
		{
			// FIXME: examine all other parameters
			
			AssemblyBuilder ab = new AssemblyBuilder (name, access);
			return ab;
		}


		[MonoTODO]
		public void DoCallBack (CrossAppDomainDelegate theDelegate)
		{
			throw new NotImplementedException ();
		}
		
		public override bool Equals (object other)
		{
			if (!(other is AppDomain))
				return false;

			return this._mono_app_domain == ((AppDomain)other)._mono_app_domain;
		}

		public int ExecuteAssembly (string assemblyFile)
		{
			return ExecuteAssembly (assemblyFile, new Evidence (), null);
		}
		
		public int ExecuteAssembly (string assemblyFile, Evidence assemblySecurity)
		{
			return ExecuteAssembly (assemblyFile, new Evidence (), null);
		}
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern int ExecuteAssembly (string assemblyFile, Evidence assemblySecurity, string[] args);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern Assembly [] GetAssemblies ();

		[MonoTODO]
		public object GetData (string name)
		{
			throw new NotImplementedException ();			
		}
		
		public override int GetHashCode ()
		{
			return (int)_mono_app_domain;
		}

		[MonoTODO]
		public object GetLifetimeService ()
		{
			throw new NotImplementedException ();			
		}

		[MonoTODO]
		public object InitializeLifetimeService ()
		{
			throw new NotImplementedException ();			
		}
	
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public extern Assembly LoadAssembly (AssemblyName assemblyRef, Evidence securityEvidence);

		public Assembly Load (AssemblyName assemblyRef)
		{
			return Load (assemblyRef, new Evidence ());
		}

		public Assembly Load (AssemblyName assemblyRef, Evidence assemblySecurity)
		{
			return LoadAssembly (assemblyRef, assemblySecurity);
		}

		public Assembly Load (string assemblyString)
		{
			AssemblyName an = new AssemblyName ();
			an.Name = assemblyString;
			
			return Load (an, new Evidence ());			
		}

		public Assembly Load (string assemblyString, Evidence assemblySecurity)
		{
			AssemblyName an = new AssemblyName ();
			an.Name = assemblyString;
			
			return Load (an, assemblySecurity);			
		}

		public Assembly Load (byte[] rawAssembly)
		{
			return Load (rawAssembly, null, new Evidence ());
		}

		public Assembly Load (byte[] rawAssembly, byte[] rawSymbolStore)
		{
			return Load (rawAssembly, rawSymbolStore, new Evidence ());
		}

		[MonoTODO]
		public Assembly Load (byte[] rawAssembly, byte[] rawSymbolStore, Evidence securityEvidence)
		{
			throw new NotImplementedException ();
		}
			
		[MonoTODO]
		public void SetAppDomainPolicy (PolicyLevel domainPolicy)
		{
			throw new NotImplementedException ();
		}
		
		public void SetCachePath (string s)
		{
			SetupInformation.CachePath = s;
		}
		
		[MonoTODO]
		public void SetPrincipalPolicy (PrincipalPolicy policy)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void SetShadowCopyPath (string s)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void SetThreadPrincipal (IPrincipal principal)
		{
			throw new NotImplementedException ();
		}
		
		public static AppDomain CreateDomain (string friendlyName)
		{
			return CreateDomain (friendlyName, new Evidence (), new AppDomainSetup ());
		}
		
		public static AppDomain CreateDomain (string friendlyName, Evidence securityInfo)
		{
			return CreateDomain (friendlyName, securityInfo, new AppDomainSetup ());
		}
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern AppDomain createDomain (string friendlyName, AppDomainSetup info);

		public static AppDomain CreateDomain (string friendlyName,
						      Evidence securityInfo,
						      AppDomainSetup info)
		{
			if (friendlyName == null || securityInfo == null || info == null)
				throw new System.ArgumentNullException();

			AppDomain ad = createDomain (friendlyName, info);

			// ad.evidence = securityInfo;

			return ad;
		}

		public static AppDomain CreateDomain (string friendlyName, Evidence securityInfo,
						      string appBasePath, string appRelativeSearchPath,
						      bool shadowCopyFiles)
		{
			AppDomainSetup info = new AppDomainSetup ();

			info.ApplicationBase = appBasePath;
			info.PrivateBinPath = appRelativeSearchPath;

			if (shadowCopyFiles)
				info.ShadowCopyFiles = "true";
			else
				info.ShadowCopyFiles = "false";

			return CreateDomain (friendlyName, securityInfo, info);
		}


		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern void Unload (AppDomain domain);
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern object GetData ();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern void SetData (string name, object data);

		[MonoTODO]
		public static int GetCurrentThreadId ()
		{
			throw new NotImplementedException ();
		}


		public event AssemblyLoadEventHandler AssemblyLoad;
		
		public event ResolveEventHandler AssemblyResolve;
		
		public event EventHandler DomainUnload;

		public event EventHandler ProcessExit;

		public event ResolveEventHandler ResourceResolve;

		public event ResolveEventHandler TypeResolve;

		public event UnhandledExceptionEventHandler UnhandledException;
    
	}
}
