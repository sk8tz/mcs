//
// Mono.Data.TdsClient.Internal.Tds50.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using System;
using System.Data.Common;

namespace Mono.Data.TdsClient.Internal {
        internal class Tds50 : Tds, ITds
	{
		#region Fields

		public static readonly TdsVersion Version = TdsVersion.tds50;

		#endregion // Fields

		#region Constructors

		public Tds50 (string server, int port)
			: this (server, port, 512)
		{
		}

		public Tds50 (string server, int port, int packetSize)
			: base (server, port, packetSize, Version)
		{
		}

		#endregion // Constructors
	
		#region Methods

		public override bool Connect (TdsConnectionParameters connectionParameters)
		{
			if (IsConnected)
				throw new InvalidOperationException ("The connection is already open.");

			SetCharset (connectionParameters.Charset);
			SetLanguage (connectionParameters.Language);

			byte pad = (byte) 0;
			byte[] empty = new byte[0];
			bool isOkay = true;

			Comm.StartPacket (TdsPacketType.Logon);

			// hostname (offset 0)
			byte[] tmp = Comm.Append (connectionParameters.Hostname, 30, pad);
			Comm.Append ((byte) (tmp.Length < 30 ? tmp.Length : 30));

			// username (offset 31 0x1f)
			tmp = Comm.Append (connectionParameters.User, 30, pad);
			Comm.Append ((byte) (tmp.Length < 30 ? tmp.Length : 30));

			// password (offset 62 0x3e)
			tmp = Comm.Append (connectionParameters.Password, 30, pad);
			Comm.Append ((byte) (tmp.Length < 30 ? tmp.Length : 30));

			// hostproc (offset 93 0x5d)
			Comm.Append ("00000116", 8, pad);

			// unused (offset 109 0x6d)
			Comm.Append (empty, (30-14), pad);

			// apptype 
			Comm.Append ((byte) 0x0);
			Comm.Append ((byte) 0xa0);
			Comm.Append ((byte) 0x24);
			Comm.Append ((byte) 0xcc);
			Comm.Append ((byte) 0x50);
			Comm.Append ((byte) 0x12);

			// hostproc length 
			Comm.Append ((byte) 8);

			// Byte order of 2 byte ints
			// 2 = <MSB, LSB>, 3 = <LSB, MSB>
			Comm.Append ((byte) 3);

			// Byte order of 4 byte ints
			// 0 = <MSB, LSB>, 1 = <LSB, MSB>
			Comm.Append ((byte) 1);

			// Character representation
			// (6 = ASCII, 7 = EBCDIC)
			Comm.Append ((byte) 6);

			// Eight byte floating point representation
			// 4 = IEEE <MSB, ..., LSB>
			// 5 = VAX 'D'
			// 10 = IEEE <LSB, ..., MSB>
			// 11 = ND5000
			Comm.Append ((byte) 10);

			// Eight byte date format
			// 8 = <MSB, ..., LSB>
			Comm.Append ((byte) 9);
		
			// notify of use db
			Comm.Append ((byte) 1);

			// disallow dump/load and bulk insert
			Comm.Append ((byte) 1);

			// sql interface type
			Comm.Append ((byte) 0);

			// type of network connection
			Comm.Append ((byte) 0);


			// spare [7]
			Comm.Append (empty, 7, pad);
			// appname
			tmp = Comm.Append (connectionParameters.ApplicationName, 30, pad);
			Comm.Append ((byte) (tmp.Length < 30 ? tmp.Length : 30));

			// server name
			tmp = Comm.Append (DataSource, 30, pad);
			Comm.Append ((byte) (tmp.Length < 30 ? tmp.Length : 30));

			// remote passwords
			Comm.Append (empty, 2, pad);
			tmp = Comm.Append (connectionParameters.Password, 253, pad);
			Comm.Append ((byte) (tmp.Length < 253 ? tmp.Length + 2 : 253 + 2));

			// tds version
			Comm.Append ((byte) (((byte) Version) / 10));
			Comm.Append ((byte) (((byte) Version) % 10));
			Comm.Append ((byte) 0);
			Comm.Append ((byte) 0);

			// prog name
			tmp = Comm.Append (connectionParameters.ProgName, 10, pad);
			Comm.Append ((byte) (tmp.Length < 10 ? tmp.Length : 10));

			// prog version
			Comm.Append ((byte) 6);

			// Tell the server we can handle SQLServer version 6
			Comm.Append ((byte) 0);

			// Send zero to tell the server we can't handle any other version
			Comm.Append ((byte) 0);
			Comm.Append ((byte) 0);

			// auto convert short
			Comm.Append ((byte) 0);

			// type of flt4
			Comm.Append ((byte) 0x0d);

			// type of date4
			Comm.Append ((byte) 0x11);

			// language
			tmp = Comm.Append (Language, 30, pad);
			Comm.Append ((byte) (tmp.Length < 30 ? tmp.Length : 30));

			// notify on lang change
			Comm.Append ((byte) 1);

			// security label hierarchy
			Comm.Append ((short) 0);

			// security components
			Comm.Append (empty, 8, pad);

			// security spare
			Comm.Append ((short) 0);

			// security login role
			Comm.Append ((byte) 0);

			// charset
			tmp = Comm.Append (Charset, 30, pad);
			Comm.Append ((byte) (tmp.Length < 30 ? tmp.Length : 30));

			// notify on charset change
			Comm.Append ((byte) 1);

			// length of tds packets
			tmp = Comm.Append (PacketSize.ToString (), 6, pad);
			Comm.Append ((byte) 3);

			// pad out to a longword
			Comm.Append (empty, 8, pad);

			Comm.SendPacket ();

			TdsPacketResult result;

			while (!((result = ProcessSubPacket()) is TdsPacketEndTokenResult)) {
				if (result is TdsPacketErrorResult) {
					isOkay = false;
				}
				// XXX Should really process some more types of packets.
			}

			IsConnected = isOkay;
			return isOkay;
		}

		protected override TdsPacketColumnInfoResult ProcessColumnInfo ()
		{
			byte precision;
			byte scale;
			int totalLength = Comm.GetTdsShort ();
			int bytesRead = 0;

			TdsPacketColumnInfoResult result = new TdsPacketColumnInfoResult ();

			while (bytesRead < totalLength) {
				scale = 0;
				precision = 0;

				int bufLength = -1;
				//int dispSize = -1;
				byte[] flagData = new byte[4];
				for (int i = 0; i < 4; i += 1) {
					flagData[i] = Comm.GetByte ();
					bytesRead += 1;
				}
				bool nullable = (flagData[2] & 0x01) > 0;
				bool caseSensitive = (flagData[2] & 0x02) > 0;
				bool writable = (flagData[2] & 0x0c) > 0;
				bool autoIncrement = (flagData[2] & 0x10) > 0;

				string tableName = String.Empty;
				TdsColumnType columnType = (TdsColumnType) Comm.GetByte ();

				bytesRead += 1;

				if (columnType == TdsColumnType.Text || columnType == TdsColumnType.Image) {
					Comm.Skip (4);
					bytesRead += 4;

					int tableNameLength = Comm.GetTdsShort ();
					bytesRead += 2;
					tableName = Comm.GetString (tableNameLength);
					bytesRead += tableNameLength;
					bufLength = 2 << 31 - 1;
				}
				else if (columnType == TdsColumnType.Decimal || columnType == TdsColumnType.Numeric) {
					bufLength = Comm.GetByte ();
					bytesRead += 1;
					precision = Comm.GetByte ();
					bytesRead += 1;
					scale = Comm.GetByte ();
					bytesRead += 1;
				}
				else if (IsFixedSizeColumn (columnType))
					bufLength = LookupBufferSize (columnType);
				else {
					bufLength = (int) Comm.GetByte () & 0xff;
					bytesRead += 1;
				}

				int index = result.Add (new TdsSchemaInfo ());
				result[index].NumericPrecision = precision;
				result[index].NumericScale = scale;
				result[index].ColumnSize = bufLength;
				result[index].ColumnName = ColumnNames[index];
				result[index].BaseTableName = tableName;
				result[index].AllowDBNull = nullable;
				result[index].IsReadOnly = !writable;
				result[index].ColumnType = columnType;
			}

			//int skipLength = totalLength - bytesRead;
			//if (skipLength != 0)
				//throw new TdsException ("skipping");

			return result;
		}

		#endregion // Methods
	}
}
