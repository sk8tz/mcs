//
// System.Data.SqlClient.SqlClientPermission.cs
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
	[Serializable]
	public sealed class SqlClientPermission : DBDataPermission 
	{
		#region Fields

		PermissionState state;

		#endregion // Fields

		#region Constructors

		 [Obsolete ("Use SqlClientPermission(PermissionState.None)", true)]
		public SqlClientPermission ()
#if NET_2_0
			: this (PermissionState.None)
#else
//			: this (PermissionState.None, false)
#endif
		{
		}

		public SqlClientPermission (PermissionState state) 
			: base (state)
		{
		}

		[Obsolete ("Use SqlClientPermission(PermissionState.None)", true)]
		public SqlClientPermission (PermissionState state, bool allowBlankPassword) 
#if NET_2_0
			: this (state)
#endif
		{
			AllowBlankPassword = allowBlankPassword;
		}

		#endregion // Constructors

		#region Methods

		public override IPermission Copy()
		{
			return new SqlClientPermission ( state);			
		}

		[MonoTODO]
		public override void Add (string connectionString, string restrictions, KeyRestrictionBehavior behavior)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}
