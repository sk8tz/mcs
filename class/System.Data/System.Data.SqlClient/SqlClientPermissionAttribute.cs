//
// System.Data.SqlClient.SqlClientPermissionAttribute.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//   Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc 2002
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Data;
using System.Data.Common;
using System.Security;
using System.Security.Permissions;

namespace System.Data.SqlClient {
	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class | 
			 AttributeTargets.Struct | AttributeTargets.Constructor | 
			 AttributeTargets.Method, AllowMultiple=true,
			 Inherited=false)]
	[Serializable]
	public sealed class SqlClientPermissionAttribute : DBDataPermissionAttribute 
	{
		#region Constructors

		public SqlClientPermissionAttribute (SecurityAction action) 
			: base (action)
		{
		}

		#endregion // Constructors

		#region Methods

		public override IPermission CreatePermission() 
		{
			if (base.Unrestricted) {
				return new SqlClientPermission ( PermissionState.Unrestricted); 
			}
			return new SqlClientPermission ( PermissionState.None); 

		}

		#endregion // Methods
	}
}
