//
// System.Data.Common.DbCommand
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

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

#if NET_2_0

using System.ComponentModel;
using System.Data;

namespace System.Data.Common {
	public abstract class DbCommand : Component, IDbCommand, IDisposable
	{
		protected DbCommand ()
		{
		}

		#region Properties

		public abstract string CommandText { get; set; }
		public abstract int CommandTimeout { get; set; }
		public abstract CommandType CommandType { get; set; }

		public DbConnection Connection {
			get { return DbConnection; }
			set { DbConnection = value; }
		}

		protected abstract DbConnection DbConnection { get; set; }
		protected abstract DbParameterCollection DbParameterCollection { get; }
		protected abstract DbTransaction DbTransaction { get; set; }
		public abstract bool DesignTimeVisible { get; set; }

		IDbConnection IDbCommand.Connection {
			get { return Connection; }
			set { Connection = (DbConnection) value; }
		}

		IDataParameterCollection IDbCommand.Parameters {
			get { return Parameters; }
		}

		IDbTransaction IDbCommand.Transaction {
			get { return Transaction; }
			set { Transaction = (DbTransaction) value; }
		}

		[MonoTODO]
		public virtual DbCommandOptionalFeatures OptionalFeatures { 
			get { throw new NotImplementedException (); }
		}

		public DbParameterCollection Parameters {
			get { return DbParameterCollection; }
		}

		public DbTransaction Transaction {
			get { return DbTransaction; }
			set { DbTransaction = value; }
		}

		public abstract UpdateRowSource UpdatedRowSource { get; set; }

		#endregion // Properties

		#region Methods

		public abstract void Cancel ();
		protected abstract DbParameter CreateDbParameter ();

		public DbParameter CreateParameter ()
		{
			return CreateDbParameter ();
		}

		protected abstract DbDataReader ExecuteDbDataReader (CommandBehavior behavior);

		[MonoTODO]
		protected virtual DbDataReader ExecuteDbPageReader (CommandBehavior behavior, int startRecord, int maxRecords)
		{
			throw new NotImplementedException ();
		}

		public abstract int ExecuteNonQuery ();
		
                public DbDataReader ExecutePageReader (CommandBehavior behavior, int startRecord, int maxRecords)
		{
			return ExecuteDbPageReader (behavior, startRecord, maxRecords);
		}

		public DbDataReader ExecuteReader ()
		{
			return ExecuteDbDataReader (CommandBehavior.Default);
		}

		public DbDataReader ExecuteReader (CommandBehavior behavior)
		{
                        return ExecuteDbDataReader (behavior);
		}

		public abstract object ExecuteScalar ();

		IDbDataParameter IDbCommand.CreateParameter ()
		{
			return CreateParameter ();
		}

		IDataReader IDbCommand.ExecuteReader ()
		{
			return ExecuteReader ();
		}

		IDataReader IDbCommand.ExecuteReader (CommandBehavior behavior)
		{
			return ExecuteReader (behavior);
		}

		public abstract void Prepare ();
		
		#endregion // Methods

	}
}

#endif
