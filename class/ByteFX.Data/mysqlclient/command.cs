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
using System.ComponentModel;
using System.Collections;

namespace ByteFX.Data.MySqlClient
{
#if WINDOWS
	[System.Drawing.ToolboxBitmap( typeof(MySqlCommand), "Designers.command.bmp")]
#endif

	[System.ComponentModel.DesignerCategory("Code")]
	public sealed class MySqlCommand : Component, IDbCommand, ICloneable
	{
		MySqlConnection				connection;
		MySqlTransaction			curTransaction;
		string						cmdText;
		int							updateCount;
		UpdateRowSource				updatedRowSource = UpdateRowSource.Both;
		MySqlParameterCollection	parameters = new MySqlParameterCollection();
		private ArrayList			arraySql = new ArrayList();

		// Implement the default constructor here.
		public MySqlCommand()
		{
		}

		// Implement other constructors here.
		public MySqlCommand(string cmdText)
		{
			this.cmdText = cmdText;
		}

		public MySqlCommand(System.ComponentModel.IContainer container)
		{
			/// <summary>
			/// Required for Windows.Forms Class Composition Designer support
			/// </summary>
			container.Add(this);
		}

		public MySqlCommand(string cmdText, MySqlConnection connection)
		{
			this.cmdText    = cmdText;
			this.connection  = connection;
		}

		public new void Dispose() 
		{
			base.Dispose();
		}

		public MySqlCommand(string cmdText, MySqlConnection connection, MySqlTransaction txn)
		{
			this.cmdText	= cmdText;
			this.connection	= connection;
			curTransaction	= txn;
		} 

		#region Properties
		[Category("Data")]
		[Description("Command text to execute")]
#if WINDOWS
		[Editor(typeof(ByteFX.Data.Common.SqlCommandTextEditor), typeof(System.Drawing.Design.UITypeEditor))]
#endif
		public string CommandText
		{
			get { return cmdText;  }
			set  { cmdText = value;  }
		}

		public int UpdateCount 
		{
			get { return updateCount; }
		}

		[Category("Misc")]
		[Description("Time to wait for command to execute")]
		public int CommandTimeout
		{
			/*
			* The sample does not support a command time-out. As a result,
			* for the get, zero is returned because zero indicates an indefinite
			* time-out period. For the set, throw an exception.
			*/
			get  { return 0; }
			set  { if (value != 0) throw new NotSupportedException(); }
		}

		[Category("Data")]
		public CommandType CommandType
		{
			/*
			* The sample only supports CommandType.Text.
			*/
			get { return CommandType.Text; }
			set 
			{ 
				if (value != CommandType.Text) 
					throw new NotSupportedException("This version of the MySql provider only supports Text command types"); 
			}
		}

		[Category("Behavior")]
		[Description("Connection used by the command")]
		public IDbConnection Connection
		{
			/*
			* The user should be able to set or change the connection at 
			* any time.
			*/
			get 
			{ 
				return connection;  
			}
			set
			{
				/*
				* The connection is associated with the transaction
				* so set the transaction object to return a null reference if the connection 
				* is reset.
				*/
				if (connection != value)
				this.Transaction = null;

				connection = (MySqlConnection)value;
			}
		}

		[Category("Data")]
		[Description("The parameters collection")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public MySqlParameterCollection Parameters
		{
			get  { return parameters; }
		}

		IDataParameterCollection IDbCommand.Parameters
		{
			get  { return parameters; }
		}

		[Browsable(false)]
		public IDbTransaction Transaction
		{
			/*
			* Set the transaction. Consider additional steps to ensure that the transaction
			* is compatible with the connection, because the two are usually linked.
			*/
			get 
			{ 
				return curTransaction; 
			}
			set 
			{ 
				curTransaction = (MySqlTransaction)value; 
			}
		}

		[Category("Behavior")]
		public UpdateRowSource UpdatedRowSource
		{
			get 
			{ 
				return updatedRowSource;  
			}
			set 
			{ 
				updatedRowSource = value; 
			}
		}
		#endregion

		#region Methods
		public void Cancel()
		{
			throw new NotSupportedException();
		}

		public MySqlParameter CreateParameter()
		{
			return new MySqlParameter();
		}

		IDbDataParameter IDbCommand.CreateParameter()
		{
			return CreateParameter();
		}

		private ArrayList SplitSql(string sql)
		{
			ArrayList commands = new ArrayList();
			System.IO.MemoryStream ms = new System.IO.MemoryStream();

			// first we tack on a semi-colon, if not already there, to make our
			// sql processing code easier.  Then we ask our encoder to give us
			// the bytes for this sql string
			byte[] bytes = connection.Encoding.GetBytes(sql + ";");

			byte left_byte = 0;
			bool escaped = false;
			int  parm_start=-1;
			for (int x=0; x < bytes.Length; x++)
			{
				byte b = bytes[x];

				// if we see a quote marker, then check to see if we are opening
				// or closing a quote
				if ((b == '\'' || b == '\"') && ! escaped )
				{
					if (b == left_byte) left_byte = 0;
					else if (left_byte == 0) left_byte = b;
				}

				else if (b == '\\') 
				{
					escaped = !escaped;
				}

					// if we see the marker for a parameter, then save its position and
					// look for the end
				else if (b == '@' && left_byte == 0 && ! escaped && parm_start==-1) 
					parm_start = x;

					// if we see a space and we are tracking a parameter, then end the parameter and have
					// that parameter serialize itself to the memory stream
				else if (parm_start > -1 && (b != '@') && (b != '$') && (b != '_') && (b != '.') && ! Char.IsLetterOrDigit((char)b))
				{
					string parm_name = sql.Substring(parm_start, x-parm_start); 

					if(parm_name.Length<2 || parm_name[1]!='@') // if doesn't begin with @@, do our processing.
					{
						MySqlParameter p = (parameters[parm_name] as MySqlParameter);
						p.SerializeToBytes(ms, connection );
					}
					else
					{
						// otherwise assume system param. just write it out
						byte[] buf = connection.Encoding.GetBytes(parm_name);
						ms.Write(buf, 0, buf.Length); 
					}
					parm_start=-1;
				}

				// if we are not in a string and we are not escaped and we are on a semi-colon,
				// then write out what we have as a command
				if (left_byte == 0 && ! escaped && b == ';' && ms.Length > 0)
				{
					commands.Add( ms.ToArray() );
					ms.SetLength(0);
				}
				else if (parm_start == -1)
					ms.WriteByte(b);


				// we want to write out the bytes in all cases except when we are parsing out a parameter
				if (escaped && b != '\\') escaped = false;
			}

			return commands;
		}

/*		internal void ExecuteRemainingCommands()
		{
			// let's execute any remaining commands
			Packet packet = ExecuteNextSql();
			while (packet != null)
			{
				while (packet.Type != PacketType.Last)
					packet = connection.InternalConnection.Driver.ReadPacket();
				packet = ExecuteNextSql();
			}
		}
*/
		/// <summary>
		/// Internal function to execute the next command in an array of commands
		/// </summary>
		internal Packet ExecuteBatch( bool stopAtResultSet )
		{
			Driver driver = connection.InternalConnection.Driver;

			while (arraySql.Count > 0)
			{
				byte[] sql = (byte[])arraySql[0];
				arraySql.RemoveAt(0);

				string s = connection.Encoding.GetString(sql);
				Packet packet =  driver.SendSql( s );
				if (packet.Type == PacketType.UpdateOrOk)
					updateCount += (int)packet.ReadLenInteger();
				else if (packet.Type == PacketType.ResultSchema && stopAtResultSet)
					return packet;
				else do 
					 {
						packet = driver.ReadPacket();
					 } while (packet.Type != PacketType.Last);
			}
			return null;
		}

		/// <summary>
		/// Executes a single non-select SQL statement.  Examples of this are update,
		/// insert, etc.
		/// </summary>
		/// <returns>Number of rows affected</returns>
		public int ExecuteNonQuery()
		{
			// There must be a valid and open connection.
			if (connection == null || connection.State != ConnectionState.Open)
				throw new InvalidOperationException("Connection must valid and open");

			// Data readers have to be closed first
			if (connection.Reader != null)
				throw new MySqlException("There is already an open DataReader associated with this Connection which must be closed first.");

			// execute any commands left in the queue from before.
			ExecuteBatch(false);
			
			arraySql = SplitSql( cmdText );
			updateCount = 0;

			ExecuteBatch(false);

			return (int)updateCount;
		}

		IDataReader IDbCommand.ExecuteReader ()
		{
			return ExecuteReader ();
		}

		IDataReader IDbCommand.ExecuteReader (CommandBehavior behavior)
		{
			return ExecuteReader (behavior);
		}

		public MySqlDataReader ExecuteReader()
		{
			return ExecuteReader(CommandBehavior.Default);
		}

		public MySqlDataReader ExecuteReader(CommandBehavior behavior)
		{
			/*
			* ExecuteReader should retrieve results from the data source
			* and return a DataReader that allows the user to process 
			* the results.
			*/

			// There must be a valid and open connection.
			if (connection == null || connection.State != ConnectionState.Open)
				throw new InvalidOperationException("Connection must valid and open");

			// make sure all readers on this connection are closed
			if (connection.Reader != null)
				throw new InvalidOperationException("There is already an open DataReader associated with this Connection which must be closed first.");

			string sql = cmdText;

			if (0 != (behavior & CommandBehavior.KeyInfo))
			{
			}

			if (0 != (behavior & CommandBehavior.SchemaOnly))
			{
			}

			if (0 != (behavior & CommandBehavior.SequentialAccess))
			{
			}

			if (0 != (behavior & CommandBehavior.SingleResult))
			{
			}

			if (0 != (behavior & CommandBehavior.SingleRow))
			{
				sql = String.Format("SET SQL_SELECT_LIMIT=1;{0};SET sql_select_limit=-1;", cmdText);
			}

			// execute any commands left in the queue from before.
			ExecuteBatch(false);

			arraySql = SplitSql( sql );

			MySqlDataReader reader = new MySqlDataReader(this, behavior);

			try 
			{
				if (reader.NextResult()) 
				{
					connection.Reader = reader;
					return reader;
				}
				return null;
			}
			catch (Exception e) 
			{
				System.Diagnostics.Trace.WriteLine("Exception in ExecuteReader: " + e.Message);
				throw e;
			}
		}

		/// <summary>
		/// ExecuteScalar executes a single SQL command that will return
		/// a single row with a single column, or if more rows/columns are
		/// returned it will return the first column of the first row.
		/// </summary>
		/// <returns></returns>
		public object ExecuteScalar()
		{
			// There must be a valid and open connection.
			if (connection == null || connection.State != ConnectionState.Open)
				throw new InvalidOperationException("Connection must valid and open");

			// Data readers have to be closed first
			if (connection.Reader != null)
				throw new MySqlException("There is already an open DataReader associated with this Connection which must be closed first.");

			// execute any commands left in the queue from before.
			ExecuteBatch(false);

			arraySql = SplitSql( cmdText );

			MySqlDataReader reader = new MySqlDataReader(this, 0);
			reader.NextResult();
			object val = null;
			if (reader.Read())
				val = reader.GetValue(0);
			reader.Close();
			return val;
		}

		public void Prepare()
		{
		}
		#endregion

		#region ICloneable
		public object Clone() 
		{
			MySqlCommand clone = new MySqlCommand(cmdText, connection, curTransaction);
			foreach (MySqlParameter p in parameters) 
			{
				clone.Parameters.Add(p.Clone());
			}
			return clone;
		}
		#endregion
  }
}
