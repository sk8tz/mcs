//
// Mono.Data.TdsClient.TdsTransaction.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using Mono.Data.TdsClient.Internal;
using System;
using System.ComponentModel;
using System.Data;

namespace Mono.Data.TdsClient {
        public class TdsTransaction : Component, ICloneable, IDbTransaction
	{
		#region Fields

		TdsConnection connection;
		IsolationLevel isolationLevel;
		bool open;

		#endregion // Fields

		#region Constructors

		internal TdsTransaction (TdsConnection connection, IsolationLevel isolevel)
		{
			this.connection = connection;
			this.isolationLevel = isolevel;

			connection.Tds.BeginTransaction ();
			open = true;
		}

		#endregion // Constructors

		#region Properties

		TdsConnection Connection {
			get { return connection; }
		}

		IDbConnection IDbTransaction.Connection {
			get { return Connection; }
		}

		IsolationLevel IDbTransaction.IsolationLevel {
			get { return isolationLevel; }
		}

		public bool Open {	
			get { return open; }
		}

		#endregion // Properties

                #region Methods

		public void Commit ()
		{
			if (!open)
				throw new InvalidOperationException ("This TdsTransaction has completed; it is no longer usable.");
			connection.Tds.CommitTransaction ();
			open = false;
		}

                object ICloneable.Clone()
                {
                        throw new NotImplementedException ();
                }

		public void Rollback ()
		{
			if (!open)
				throw new InvalidOperationException ("This TdsTransaction has completed; it is no longer usable.");
			connection.Tds.RollbackTransaction ();
			open = false;
		}

		public void Save (string savePointName)
		{
			if (!open)
				throw new InvalidOperationException ("This TdsTransaction has completed; it is no longer usable.");
			connection.Tds.SaveTransaction (savePointName);
		}

                #endregion // Methods
	}
}
