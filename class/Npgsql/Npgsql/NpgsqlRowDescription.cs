// created on 12/6/2002 at 20:29

// Npgsql.NpgsqlRowDescription.cs
// 
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
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
using System.Collections;
using System.IO;
using System.Text;
using System.Net;

namespace Npgsql
{
	
	
	/// <summary>
	/// This struct represents the internal data of the RowDescription message.
	/// </summary>
	/// 
	// [FIXME] Is this name OK? Does it represent well the struct intent?
	// Should it be a struct or a class?
	internal struct NpgsqlRowDescriptionFieldData
	{
		public String 	name;                      // Protocol 2/3
	  public Int32    table_oid;                 // Protocol 3
	  public Int16    column_attribute_number;   // Protocol 3 
		public Int32		type_oid;                  // Protocol 2/3
		public Int16		type_size;                 // Protocol 2/3
		public Int32		type_modifier;		         // Protocol 2/3
		public FormatCode    format_code;               // Protocol 3. 0 text, 1 binary
	}
	
	/// <summary>
	/// This class represents a RowDescription message sent from 
	/// the PostgreSQL.
	/// </summary>
	/// 
	internal sealed class NpgsqlRowDescription
	{
		// Logging related values
    private static readonly String CLASSNAME = "NpgsqlRowDescription";
		
				
		private ArrayList	fields_data = new ArrayList();
		
		private Hashtable fields_index = new Hashtable();
		
		private Int32 protocol_version;
	  
	  public NpgsqlRowDescription(Int32 protocolVersion)
	  {
	    protocol_version = protocolVersion;
	  }
		
		public void ReadFromStream(Stream input_stream, Encoding encoding)
		{
			NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ReadFromStream");
			
			
			if (protocol_version == ProtocolVersion.Version2)
			{
  			Byte[] input_buffer = new Byte[10]; // Max read will be 4 + 2 + 4
  			
  			// Read the number of fields.
  			input_stream.Read(input_buffer, 0, 2);
  			Int16 num_fields = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(input_buffer, 0));
  			
  			
  			// Temporary FieldData object to get data from stream and put in array.
  			NpgsqlRowDescriptionFieldData fd;
  			
  			// Now, iterate through each field getting its data.
  			for (Int16 i = 0; i < num_fields; i++)
  			{
  				fd = new NpgsqlRowDescriptionFieldData();
  				
  				// Set field name.
  				fd.name = PGUtil.ReadString(input_stream, encoding);
  				
  				// Read type_oid(Int32), type_size(Int16), type_modifier(Int32)
  				input_stream.Read(input_buffer, 0, 4 + 2 + 4);
  				
  				fd.type_oid = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(input_buffer, 0));
  				fd.type_size = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(input_buffer, 4));
  				fd.type_modifier = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(input_buffer, 6));
  				
  				// Add field data to array.
  				fields_data.Add(fd);
  				
  				fields_index.Add(fd.name, i);
  			}
			}
			else
			{
			  Byte[] input_buffer = new Byte[4]; // Max read will be 4 + 2 + 4 + 2 + 4 + 2
			  
			  // Read the length of message. 
			  // [TODO] Any use for now?
			  PGUtil.ReadInt32(input_stream, input_buffer);
			  Int16 num_fields = PGUtil.ReadInt16(input_stream, input_buffer);
			  
			  // Temporary FieldData object to get data from stream and put in array.
  			NpgsqlRowDescriptionFieldData fd;
			  
			  for (Int16 i = 0; i < num_fields; i++)
			  {
			    fd = new NpgsqlRowDescriptionFieldData();
			    
			    fd.name = PGUtil.ReadString(input_stream, encoding);
			    fd.table_oid = PGUtil.ReadInt32(input_stream, input_buffer);
			    fd.column_attribute_number = PGUtil.ReadInt16(input_stream, input_buffer);
			    fd.type_oid = PGUtil.ReadInt32(input_stream, input_buffer);
			    fd.type_size = PGUtil.ReadInt16(input_stream, input_buffer);
			    fd.type_modifier = PGUtil.ReadInt32(input_stream, input_buffer);
			    fd.format_code = (FormatCode)PGUtil.ReadInt16(input_stream, input_buffer);
			    
			    fields_data.Add(fd);
			    fields_index.Add(fd.name, i);
			  }
			  
			}
  	
						
		}
		
		public NpgsqlRowDescriptionFieldData this[Int32 index]
		{
			get
			{
				return (NpgsqlRowDescriptionFieldData)fields_data[index];
			}
			
		}
		
		public Int16 NumFields
		{
			get
			{
				return (Int16)fields_data.Count;
			}
		}
		
		public Int16 FieldIndex(String fieldName)
		{
			return (Int16) fields_index[fieldName];
		}
		
	}
	
}
