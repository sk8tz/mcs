//
// System.Security.Permissions.StrongNameIdentityPermissionAttribute.cs
//
// Duncan Mak <duncan@ximian.com>
//
// (C) 2002 Ximian, Inc.			http://www.ximian.com
//

using System;
using System.Security.Permissions;

namespace System.Security.Permissions
{

	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class |
			 AttributeTargets.Struct | AttributeTargets.Constructor |
			 AttributeTargets.Method)]
	[Serializable]
	public sealed class StrongNameIdentityPermissionAttribute : CodeAccessSecurityAttribute
	{
		// Fields
		private string name;
		private string key;
		private string version;
		
		// Constructor
		public StrongNameIdentityPermissionAttribute (SecurityAction action) : base (action) {}
		
		// Properties
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		public string PublicKey
		{
			get { return key; }
			set { key = value; }
		}

		public string Version
		{
			get { return version; }
			set { version = value; }
		}
			 
		// Methods
		[MonoTODO]
		public override IPermission CreatePermission ()
		 {
			 return null;
		 }
	}
	
}
