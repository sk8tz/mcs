using System;
using ByteFX.Data.Common;

namespace ByteFX.Data.MySqlClient
{
	/// <summary>
	/// 
	/// </summary>
	internal sealed class MySqlInternalConnection
	{
		MySqlConnectionString	settings;
		Driver					driver;
		DateTime				createTime;
		bool					serverVariablesSet;

		public MySqlInternalConnection( MySqlConnectionString connectString )
		{
			settings = connectString;
			serverVariablesSet = false;
		}

		#region Properties
		public MySqlConnectionString Settings 
		{
			get { return settings; }
			set { settings = value; }
		}

		internal Driver Driver 
		{
			get { return driver; }
		}

		#endregion

		#region Methods

		public bool IsAlive() 
		{
			Packet packet;
			try 
			{
				byte[] bytes = driver.Encoding.GetBytes("select connection_id();");
				packet = driver.SendSql( bytes );
				// we have to read for two last packets since MySql sends
				// us a last packet after schema and again after rows
				// I will likely change this later to have the driver just
				// return schema in one very large packet.
				while (! packet.IsLastPacket())
					packet = driver.ReadPacket();

				// now read off the resultset
				packet = driver.ReadPacket();
				while (! packet.IsLastPacket())
					packet = driver.ReadPacket();
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		public bool IsTooOld() 
		{
			TimeSpan ts = DateTime.Now.Subtract( createTime );
			if (ts.Seconds > settings.ConnectionLifetime)
				return true;
			return false;
		}

		/// <summary>
		/// I don't like this setup but can't think of a better way of doing
		/// right now.
		/// </summary>
		/// <param name="connection"></param>
		public void SetServerVariables(MySqlConnection connection)
		{
			if (serverVariablesSet) return;

			// retrieve the encoding that should be used for character data
			MySqlCommand cmd = new MySqlCommand("show variables like 'max_allowed_packet'", connection);
			try 
			{
				MySqlDataReader reader = cmd.ExecuteReader();
				reader.Read();
				driver.MaxPacketSize = reader.GetInt64( 1 );
				reader.Close();
			}
			catch 
			{
				driver.MaxPacketSize = 1047552;
			}

			cmd.CommandText = "show variables like 'character_set'";
			driver.Encoding = System.Text.Encoding.Default;
			
			try 
			{
				MySqlDataReader reader = cmd.ExecuteReader();
				if (reader.Read())
					driver.Encoding = CharSetMap.GetEncoding( reader.GetString(1) );
				reader.Close();
			}
			catch 
			{ 
				throw new MySqlException("Failure to initialize connection");
			}

			serverVariablesSet = true;
		}

		public void Open() 
		{
			driver = new Driver();
			driver.Open( settings );

			createTime = DateTime.Now;
		}

		public void Close() 
		{
			driver.Close();
		}

		#endregion

	}
}
