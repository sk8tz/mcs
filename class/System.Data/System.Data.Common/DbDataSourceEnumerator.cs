//
// System.Data.Common.DbDataSourceEnumerator.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_2_0

namespace System.Data.Common {
	public abstract class DbDataSourceEnumerator
	{
		#region Constructors

		[MonoTODO]
		protected DbDataSourceEnumerator ()
		{
		}

		#endregion // Constructors

		#region Methods

		public abstract DataTable GetDataSources ();

		#endregion // Methods
	}
}

#endif // NET_2_0
