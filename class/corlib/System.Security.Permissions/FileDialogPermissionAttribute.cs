//
// System.Security.Permissions.FileDialogPermissionAttribute.cs
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
	public sealed class FileDialogPermissionAttribute : CodeAccessSecurityAttribute
	{
		// Fields
		private bool canOpen;
		private bool canSave;
		
		// Constructor
		public FileDialogPermissionAttribute (SecurityAction action) : base (action) {}

		// Properties
		public bool Open
		{
				get { return canOpen; }
				set { canOpen = value; }
		} 

		public bool Save
		{
				get { return canSave; }
				set { canSave = value; }
		}

		// Methods
		[MonoTODO]
		public override IPermission CreatePermission ()
		{
				return null;
		}
	}
}
