//
// System.Security.Cryptography CipherMode enumeration
//
// Authors:
//   Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//
// Copyright 2001 by Matthew S. Ford.
//


namespace System.Security.Cryptography {

	/// <summary>
	/// Block cipher modes of operation.
	/// </summary>
	public enum CipherMode {
		CBC, // Cipher Block Chaining
		CFB, // Cipher Feedback
		CTS, // Cipher Text Stealing
		ECB, // Electronic Codebook
		OFB  // Output Feedback
	}
}

