//
// System.Data.SqlClient.SqlRowUpdatedEventArgs.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002
//

using System;
using System.Data;
using System.Data.Common;

namespace System.Data.SqlClient {
	public sealed class SqlRowUpdatedEventArgs : RowUpdatedEventArgs 
	{
		[MonoTODO]
		public SqlRowUpdatedEventArgs (DataRow row, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
			: base (row, command, statementType, tableMapping)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public new SqlCommand Command {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		~SqlRowUpdatedEventArgs () 
		{
			throw new NotImplementedException ();
		}

	}
}
