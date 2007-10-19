//
// System.Data.Odbc.OdbcTransaction
//
// Authors:
//  Brian Ritchie (brianlritchie@hotmail.com) 
//
// Copyright (C) Brian Ritchie, 2002
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System;
using System.Data;

#if NET_2_0
using System.Data.Common;
#endif

namespace System.Data.Odbc
{
#if NET_2_0
	public sealed class OdbcTransaction : DbTransaction, IDisposable
#else
	public sealed class OdbcTransaction : MarshalByRefObject, IDbTransaction
#endif
	{
		private bool disposed;
		private OdbcConnection connection;
		private IsolationLevel isolationlevel;

		internal OdbcTransaction (OdbcConnection conn, IsolationLevel isolationlevel)
		{
			// Set Auto-commit (102) to false
			SetAutoCommit (conn, false);
			// Handle isolation level
			int lev = 0;
			switch (isolationlevel) {
			case IsolationLevel.ReadUncommitted:
				lev = 1;
				break;
			case IsolationLevel.ReadCommitted:
				lev = 2;
				break;
			case IsolationLevel.RepeatableRead:
				lev = 3;
				break;
			case IsolationLevel.Serializable:
				lev = 4;
				break;
			case IsolationLevel.Unspecified:
				lev = 0;
				break;
			default:
				throw new NotSupportedException();
			}
			// mbd: Getting the return code of the second call to SQLSetConnectAttr is missing from original code!
			OdbcReturn ret = libodbc.SQLSetConnectAttr (conn.hDbc,
				OdbcConnectionAttribute.TransactionIsolation,
				(IntPtr) lev, 0);
			if (ret != OdbcReturn.Success && ret != OdbcReturn.SuccessWithInfo)
				throw new OdbcException (new OdbcError ("SQLSetConnectAttr", OdbcHandleType.Dbc, conn.hDbc));
			this.isolationlevel = isolationlevel;
			connection=conn;
		}

		// Set Auto-commit (102) connection attribute
		// [MonoTODO]: nice to have before svn: define libodbc.SQL_IS_UINTEGER = -5
		private static void SetAutoCommit (OdbcConnection conn, bool isAuto)
		{
			OdbcReturn ret = libodbc.SQLSetConnectAttr (conn.hDbc,
				OdbcConnectionAttribute.AutoCommit,
				(IntPtr) (isAuto ? 1 : 0), -5);
			if (ret != OdbcReturn.Success && ret != OdbcReturn.SuccessWithInfo)
				throw new OdbcException (new OdbcError ("SQLSetConnectAttr", OdbcHandleType.Dbc, conn.hDbc));
		}

		#region Implementation of IDisposable

#if NET_2_0
		protected override
#endif
		void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing)
					Rollback ();
				disposed = true;
			}
		}

		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		#endregion Implementation of IDisposable

		#region Implementation of IDbTransaction

		public
#if NET_2_0
		override
#endif //NET_2_0
		void Commit ()
		{
			if (connection.transaction == this) {
				OdbcReturn ret = libodbc.SQLEndTran ((short) OdbcHandleType.Dbc, connection.hDbc, 0);
				if (ret != OdbcReturn.Success && ret != OdbcReturn.SuccessWithInfo)
					throw new OdbcException (new OdbcError ("SQLEndTran", OdbcHandleType.Dbc, connection.hDbc));
				SetAutoCommit (connection, true); // restore default auto-commit
				connection.transaction = null;
			} else
				throw new InvalidOperationException ();
		}

		public
#if NET_2_0
		override
#endif //NET_2_0
		void Rollback()
		{
			if (connection.transaction == this) {
				OdbcReturn ret = libodbc.SQLEndTran ((short) OdbcHandleType.Dbc, connection.hDbc, 1);
				if (ret!=OdbcReturn.Success && ret != OdbcReturn.SuccessWithInfo)
					throw new OdbcException (new OdbcError ("SQLEndTran", OdbcHandleType.Dbc, connection.hDbc));
				SetAutoCommit (connection, true);    // restore default auto-commit
				connection.transaction = null;
			} else
				throw new InvalidOperationException ();
		}

#if NET_2_0
		protected override DbConnection DbConnection {
			get {
				return Connection;
			}
		}
#else
		IDbConnection IDbTransaction.Connection {
			get {
				return Connection;
			}
		}

#endif

		public
#if NET_2_0
		override
#endif
		IsolationLevel IsolationLevel {
			get {
				return isolationlevel;
			}
		}

		#endregion Implementation of IDbTransaction

		#region Public Instance Properties

		public new OdbcConnection Connection {
			get {
				return connection;
			}
		}

		#endregion Public Instance Properties
	}
}
