//
// Mono.Data.TdsClient.Internal.TdsConnectionInternal.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Net;
using System.Net.Sockets;

namespace Mono.Data.TdsClient.Internal {
        internal class TdsConnectionInternal : Component, ICloneable, IDbConnection
	{
		#region Fields

		bool autoCommit;
		//TdsCommandCollection commands;
		ConnectionState connectionState;
		string connectionString;
		int connectionTimeout;
		string database;
		//TdsDbMetadata databaseMetadata;		
		string host;
		bool isClosed;
		int packetSize;
		string password;
		int port;
		bool readOnly;
		TdsServerType serverType;
		ArrayList tdsPool;
		TdsVersion tdsVersion = TdsVersion.tds42; // default to TDS version 4.2 which is used by both servers
		IsolationLevel isolationLevel;
		TdsCommInternal comm;
		string user;

		Socket socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

		#endregion // Fields

		#region Constructors

		public TdsConnectionInternal (TdsServerType serverType)
			: this (serverType, 15, true, false, IsolationLevel.ReadCommitted)
		{
		}

		public TdsConnectionInternal (TdsServerType serverType, int connectionTimeout)
			: this (serverType, connectionTimeout, true, false, IsolationLevel.ReadCommitted)
		{
		}

		public TdsConnectionInternal (TdsServerType serverType, int connectionTimeout, bool autoCommit)
			: this (serverType, connectionTimeout, autoCommit, false, IsolationLevel.ReadCommitted)
		{
		}

		public TdsConnectionInternal (TdsServerType serverType, int connectionTimeout, bool autoCommit, bool readOnly, IsolationLevel isolationLevel)
		{
			this.connectionState = ConnectionState.Closed;
			this.serverType = serverType;
			this.autoCommit = autoCommit;
			this.isolationLevel = isolationLevel;
			this.readOnly = readOnly;
			this.connectionTimeout = connectionTimeout;
			this.packetSize = 512; // Minimum TDS packet size
		}
			
		#endregion // Constructors

		#region Properties

		public ConnectionState State {
			get { return connectionState; }
		}

		public string ConnectionString {
			get { return connectionString; }
			set { connectionString = value; }
		}
		
		public int ConnectionTimeout {
			get { return connectionTimeout; }
			set { connectionTimeout = value; }
		}
		
		public string Host {
			get { return host; }
			set { host = value; }
		}
		
		public int Port {
			get { return port; }
			set { port = value; }
		}
		
		public string Database {
			get { return database; }
			set { database = value; }
		}
		
		public string User {
			get { return user; }
			set { user = value; }
		}
		
		public string Password {
			get { return password; }
			set { password = value; }
		}
		
		public int PacketSize {
			get { return packetSize; }
			set { packetSize = value; }
		}

		public TdsVersion TdsVersion {
			get { return tdsVersion; }
			set { tdsVersion = value; }
		}

		#endregion // Properties

		#region Methods

		public TdsTransactionInternal BeginTransaction ()
		{
			return BeginTransaction (IsolationLevel.ReadCommitted);
		}

		public TdsTransactionInternal BeginTransaction (IsolationLevel il)
		{
			return new TdsTransactionInternal (this, il);
		}

		[System.MonoTODO]
		public void ChangeDatabase (string databaseName)
		{
			throw new NotImplementedException ();
		}

		[System.MonoTODO("Logout?")]
		public void Close ()
		{
			socket.Shutdown (SocketShutdown.Both);
			socket.Close ();
		}

		public TdsCommandInternal CreateCommand ()
		{
			TdsCommandInternal command = new TdsCommandInternal ();
			command.Connection = this;
			return command;
		}

                object ICloneable.Clone()
                {
                        throw new NotImplementedException ();
                }

		IDbTransaction IDbConnection.BeginTransaction ()
		{
			return BeginTransaction ();
		}

		IDbTransaction IDbConnection.BeginTransaction (IsolationLevel il)
		{
			return BeginTransaction (il);
		}

		IDbCommand IDbConnection.CreateCommand ()
		{
			return CreateCommand ();
		}

		[System.MonoTODO("Login?")]
		public void Open ()
		{
			IPHostEntry hostEntry = Dns.GetHostByName (host);
			IPAddress[] addresses = hostEntry.AddressList;
		
			IPEndPoint endPoint;

			foreach (IPAddress address in addresses) {
				endPoint = new IPEndPoint (address, port);
				socket.Connect (endPoint);

				if (socket.Connected)
					break;
			}
		}

		#endregion // Methods
	}
}
