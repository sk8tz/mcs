// ByteFX.Data data access components for .Net
// Copyright (C) 2002-2003  ByteFX, Inc.
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Data;
using System.Collections.Specialized;
using System.Text;
using System.ComponentModel;
using System.Globalization;
using ByteFX.Data.Common;

namespace ByteFX.Data.MySqlClient
{
	/// <summary>
	/// Summary description for MySqlConnection.
	/// </summary>
	[System.Drawing.ToolboxBitmap( typeof(MySqlConnection), "Designers.connection.bmp")]
	[System.ComponentModel.DesignerCategory("Code")]
	[ToolboxItem(true)]
	public sealed class MySqlConnection : IDbConnection, ICloneable
	{
		public ConnectionState			state;
		private MySqlInternalConnection	internalConnection;
		private MySqlDataReader			dataReader;
		private NumberFormatInfo		numberFormat;
		private MySqlConnectionString	settings;

		public event StateChangeEventHandler	StateChange;


		// Always have a default constructor.
		public MySqlConnection()
		{
			settings = new MySqlConnectionString();
		}

		public MySqlConnection(System.ComponentModel.IContainer container)
		{
			settings = new MySqlConnectionString();
		}
    

		// Have a constructor that takes a connection string.
		public MySqlConnection(string connectString)
		{
			settings = new MySqlConnectionString(connectString);
		}

		#region Properties
		[Browsable(true)]
		public string DataSource
		{
			get { return settings.Host; }
		}

		[Browsable(false)]
		public string User
		{
			get { return settings.Username; }
		}

		[Browsable(false)]
		public string Password
		{
			get { return settings.Password; }
		}

		[Browsable(true)]
		public int ConnectionTimeout
		{
			get { return settings.ConnectTimeout; }
		}
		
		[Browsable(true)]
		public string Database
		{
			get	{ return settings.Database; }
		}

		[Browsable(false)]
		public bool UseCompression
		{
			get { return settings.UseCompression; }
		}
		
		[Browsable(false)]
		public int Port
		{
			get	{ return settings.Port; }
		}

		[Browsable(false)]
		public ConnectionState State
		{
			get { return state; }
		}

		internal MySqlDataReader Reader
		{
			get { return dataReader; }
			set { dataReader = value; }
		}

		internal MySqlInternalConnection InternalConnection
		{
			get { return internalConnection; }
		}

		internal NumberFormatInfo NumberFormat
		{
			get 
			{
				if (numberFormat == null)
				{
					numberFormat = new NumberFormatInfo();
					numberFormat = (NumberFormatInfo)NumberFormatInfo.InvariantInfo.Clone();
					numberFormat.NumberDecimalSeparator = ".";
				}
				return numberFormat;
			}
		}

		/// <summary>
		/// Gets a string containing the version of the of the server to which the client is connected.
		/// </summary>
		[Browsable(false)]
		public string ServerVersion 
		{
			get { return ""; } //internalConnection.GetServerVersion(); }
		}

		internal Encoding Encoding 
		{
			get 
			{
//TODO				if (encoding == null)
					return System.Text.Encoding.Default;
//				else 
//					return encoding;
			}
		}


#if WINDOWS
		[Editor(typeof(Designers.ConnectionStringEditor), typeof(System.Drawing.Design.UITypeEditor))]
#endif
		[Browsable(true)]
		[Category("Data")]
		public string ConnectionString
		{
			get
			{
				// Always return exactly what the user set.
				// Security-sensitive information may be removed.
				return settings.ConnectString;
			}
			set
			{
				settings.ConnectString = value;
				if (internalConnection != null)
					internalConnection.Settings = settings;
			}
		}

		#endregion

		#region Transactions
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public MySqlTransaction BeginTransaction()
		{
			if (state != ConnectionState.Open)
				throw new MySqlException("Invalid operation: The connection is closed");

			MySqlTransaction t = new MySqlTransaction();
			t.Connection = this;
			InternalConnection.Driver.SendCommand( DBCmd.QUERY, "BEGIN");
			return t;
		}

		/// <summary>
		/// 
		/// </summary>
		IDbTransaction IDbConnection.BeginTransaction()
		{
			return BeginTransaction();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="level"></param>
		/// <returns></returns>
		public MySqlTransaction BeginTransaction(IsolationLevel level)
		{
			if (state != ConnectionState.Open)
				throw new MySqlException("Invalid operation: The connection is closed");

			MySqlTransaction t = new MySqlTransaction();
			t.Connection = this;
			t.IsolationLevel = level;
			string cmd = "SET SESSION TRANSACTION ISOLATION LEVEL ";
			switch (level) 
			{
				case IsolationLevel.ReadCommitted:
					cmd += "READ COMMITTED"; break;
				case IsolationLevel.ReadUncommitted:
					cmd += "READ UNCOMMITTED"; break;
				case IsolationLevel.RepeatableRead:
					cmd += "REPEATABLE READ"; break;
				case IsolationLevel.Serializable:
					cmd += "SERIALIZABLE"; break;
				case IsolationLevel.Chaos:
					throw new NotSupportedException("Chaos isolation level is not supported");
			}
			InternalConnection.Driver.SendCommand( DBCmd.QUERY, cmd );
			InternalConnection.Driver.SendCommand( DBCmd.QUERY, "BEGIN");
			return t;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="level"></param>
		/// <returns></returns>
		IDbTransaction IDbConnection.BeginTransaction(IsolationLevel level)
		{
			return BeginTransaction(level);
		}
		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dbName"></param>
		public void ChangeDatabase(string dbName)
		{
			if (state != ConnectionState.Open)
				throw new MySqlException("Invalid operation: The connection is closed");

			//TODOinternalConnection.ChangeDatabase( dbName );
			InternalConnection.Driver.SendCommand( DBCmd.INIT_DB, dbName );
		}

		internal void SetState( ConnectionState newState ) 
		{
			ConnectionState oldState = state;
			state = newState;
			if (this.StateChange != null)
				StateChange(this, new StateChangeEventArgs( oldState, newState ));
		}

		public void Open()
		{
			if (state == ConnectionState.Open)
				throw new MySqlException("error connecting: The connection is already Open (state=Open).");

			SetState( ConnectionState.Connecting );

			if (settings.Pooling) 
			{
				internalConnection = MySqlPoolManager.GetConnection( settings );
			}
			else
			{
				internalConnection = new MySqlInternalConnection( settings );
				internalConnection.Open();
			}

			SetState( ConnectionState.Open );
			internalConnection.SetServerVariables(this);
			ChangeDatabase( settings.Database );
		}


		public void Close()
		{
			if (state == ConnectionState.Closed) return;

			if (dataReader != null)
				dataReader.Close();

			if (settings.Pooling)
				MySqlPoolManager.ReleaseConnection( internalConnection );
			else
				internalConnection.Close();

			SetState( ConnectionState.Closed );
		}

		IDbCommand IDbConnection.CreateCommand()
		{
			return CreateCommand();
		}

		public MySqlCommand CreateCommand()
		{
			// Return a new instance of a command object.
			MySqlCommand c = new MySqlCommand();
			c.Connection = this;
			return c;
		}

		#region ICloneable
		public object Clone()
		{
			MySqlConnection clone = new MySqlConnection();
			clone.ConnectionString = this.ConnectionString;
			//TODO:  how deep should this go?
			return clone;
		}
		#endregion

		#region IDisposeable
		public void Dispose() 
		{
			if (State == ConnectionState.Open)
				Close();
		}
		#endregion
  }
}
