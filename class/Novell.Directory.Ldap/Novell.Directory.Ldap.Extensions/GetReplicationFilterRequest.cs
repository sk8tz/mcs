/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
//
// Novell.Directory.Ldap.Extensions.GetReplicationFilterRequest.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap.Utilclass;

namespace Novell.Directory.Ldap.Extensions
{
	
	/// <summary> 
	/// Gets the Replication filter for all replicas on the server.
	/// 
	/// The filter is returned as an array of classnames-attribute names pairs. 
	/// 
	/// To get the filter for all replicas on a specific server, you must
	/// create an instance of this class and then call the
	/// extendedOperation method with this object as the required
	/// LdapExtendedOperation parameter.
	/// 
	/// The GetReplicationFilterRequest extension uses the following OID:
	/// 2.16.840.1.113719.1.27.100.37
	/// 
	/// The requestValue has the following format:
	/// 
	/// requestValue ::=
	///  serverName  LdapDN
	/// </summary>
	public class GetReplicationFilterRequest:LdapExtendedOperation
	{
		
		static GetReplicationFilterRequest() 
		{
			/*
				* Register the extendedresponse class which is returned by the
				* server in response to a ListReplicasRequest
				*/
			try
			{
				LdapExtendedResponse.register(ReplicationConstants.GET_REPLICATION_FILTER_RES, System.Type.GetType("Novell.Directory.Ldap.Extensions.GetReplicationFilterResponse"));
			}
			catch (System.Exception e)
			{
				System.Console.Error.WriteLine("Could not register Extended Response -" + " Class not found");
			}
		}

		/// <summary> 
		/// Constructs an extended operations object which contains the ber encoded
		/// replication filter.
		/// 
		/// </summary>
		/// <param name="serverDN">The server whose replication filter needs to be read
		/// 
		/// </param>
		/// <exception> LdapException A general exception which includes an error
		/// message and an Ldap error code.
		/// </exception>
		public GetReplicationFilterRequest(System.String serverDN):base(ReplicationConstants.GET_REPLICATION_FILTER_REQ, null)
		{
			
			try
			{
				
				if ((System.Object) serverDN == null)
					throw new System.ArgumentException(ExceptionMessages.PARAM_ERROR);
				
				System.IO.MemoryStream encodedData = new System.IO.MemoryStream();
				LBEREncoder encoder = new LBEREncoder();
				
				Asn1OctetString asn1_serverDN = new Asn1OctetString(serverDN);
				
				// Add the serverDN to encoded data
				asn1_serverDN.encode(encoder, encodedData);
				setValue(SupportClass.ToSByteArray(encodedData.ToArray()));
			}
			catch (System.IO.IOException ioe)
			{
				throw new LdapException(ExceptionMessages.ENCODING_ERROR, LdapException.ENCODING_ERROR, (System.String) null);
			}
		}
	}
}
