//
// Mono.Security.Protocol.Ntlm.Type2MessageTest
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// Copyright (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Text;

using Mono.Security.Protocol.Ntlm;
using NUnit.Framework;

namespace MonoTests.Mono.Security.Protocol.Ntlm {

	[TestFixture]
	public class Type2MessageTest : Assertion {

		static byte[] nonce = { 0x53, 0x72, 0x76, 0x4e, 0x6f, 0x6e, 0x63, 0x65 };

		[Test]
		// Example from http://www.innovation.ch/java/ntlm.html
		public void Encode1 () 
		{
			Type2Message msg = new Type2Message ();
			AssertEquals ("Type", 2, msg.Type);
			msg.Nonce = nonce;
			AssertEquals ("GetBytes", "4E-54-4C-4D-53-53-50-00-02-00-00-00-00-00-00-00-28-00-00-00-01-82-00-00-53-72-76-4E-6F-6E-63-65-00-00-00-00-00-00-00-00", BitConverter.ToString (msg.GetBytes ()));
		}

		[Test]
		// Example from http://www.innovation.ch/java/ntlm.html
		public void Decode1 () 
		{
			byte[] data = { 0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x01, 0x82, 0x00, 0x00, 0x53, 0x72, 0x76, 0x4e, 0x6f, 0x6e, 0x63, 0x65, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
			Type2Message msg = new Type2Message (data);
			AssertEquals ("Flags", (NtlmFlags)0x8201, msg.Flags);
			AssertEquals ("Nonce", BitConverter.ToString (nonce), BitConverter.ToString (msg.Nonce));
			AssertEquals ("Type", 2, msg.Type);
		}

		[Test]
		// Example from http://davenport.sourceforge.net/ntlm.html#type2MessageExample
		public void Decode2 () 
		{
			byte[] data = { 0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50, 0x00, 0x02, 0x00, 0x00, 0x00, 0x0c, 0x00, 0x0c, 0x00, 0x30, 0x00, 0x00, 0x00, 0x01, 0x02, 0x81, 0x00, 0x01, 0x23, 0x45, 0x67, 0x89, 0xab, 0xcd, 0xef, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x62, 0x00, 0x62, 0x00, 0x3c, 0x00, 0x00, 0x00, 0x44, 0x00, 0x4f, 0x00, 0x4d, 0x00, 0x41, 0x00, 0x49, 0x00, 0x4e, 0x00, 0x02, 0x00, 0x0c, 0x00, 0x44, 0x00, 0x4f, 0x00, 0x4d, 0x00, 0x41, 0x00, 0x49, 0x00, 0x4e, 0x00, 0x01, 0x00, 0x0c, 0x00, 0x53, 0x00, 0x45, 0x00, 0x52, 0x00, 0x56, 0x00, 0x45, 0x00, 0x52, 0x00, 0x04, 0x00, 0x14, 0x00, 0x64, 0x00, 0x6f, 0x00, 0x6d, 0x00, 0x61, 0x00, 0x69, 0x00, 0x6e, 0x00, 0x2e, 0x00, 0x63, 0x00, 0x6f, 0x00, 0x6d, 0x00, 0x03, 0x00, 0x22, 0x00, 0x73, 0x00, 0x65, 0x00, 0x72, 0x00, 0x76, 0x00, 0x65, 0x00, 0x72, 0x00, 0x2e, 0x00, 0x64, 0x00, 0x6f, 0x00, 0x6d, 0x00, 0x61, 0x00, 0x69, 0x00, 0x6e, 0x00, 0x2e, 0x00, 0x63, 0x00, 0x6f, 0x00, 0x6d, 0x00, 0x00, 0x00, 0x00, 0x00 };
			Type2Message msg = new Type2Message (data);
			AssertEquals ("Flags", (NtlmFlags)0x00810201, msg.Flags);
			AssertEquals ("Nonce", "01-23-45-67-89-AB-CD-EF", BitConverter.ToString (msg.Nonce));
			AssertEquals ("Type", 2, msg.Type);
		}
	}
}
