//
// System.Data.IDBCommand.cs
//
// Author:
//   Christopher Podurgiel (cpodurgiel@msn.com)
//
// (C) Chris Podurgiel
//

namespace System.Data
{
	/// <summary>
	/// Represents a SQL statement that is executed while connected to a data source, and is implemented by .NET data providers that access relational databases.
	/// </summary>
	public interface IDBCommand
	{
		void Cancel()
		{
		}
		
		IDataParameter CreateParameter()
		{
		}
		
		int ExecuteNonQuery()
		{
		}

		IDataReader ExecuteReader()
		{
		}

		IDataReader ExecuteReader(CommandBehavior behavior)
		{
		}

		object ExecuteScalar()
		{
		}

		void Prepare()
		{
		}


		string CommandText
		{
			get
			{
			}
			set
			{
			}
		}

		int CommandTimeout
		{
			get
			{
			}
			set
			{
			}
		}

		CommandTpe CommandType
		{
			get
			{
			}
			set
			{
			}
		}

		IDbConnection Connection
		{
			get
			{
			}
			set
			{
			}
		}

		IDataParameterCollection Parameters
		{
			get
			{
			}
		}

		IDbTransaction Transaction
		{
			get
			{
			}
			set
			{
			}
		}

		UpdateRowSource UpdatedRowSource
		{
			get
			{
			}
			set
			{
			}
		}
	}
}