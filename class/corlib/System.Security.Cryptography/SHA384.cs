//
// System.Security.Cryptography SHA384 Class implementation
//
// Authors:
//   Matthew S. Ford (Matthew.S.Ford@Rose-Hulman.Edu)
//   Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright 2001 by Matthew S. Ford.
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

namespace System.Security.Cryptography {

	public abstract class SHA384 : HashAlgorithm {

		public SHA384 () 
		{
			HashSizeValue = 384;
		}

		public static new SHA384 Create () 
		{
			return Create ("System.Security.Cryptography.SHA384");
		}
	
		public static new SHA384 Create (string hashName) 
		{
			return (SHA384) CryptoConfig.CreateFromName (hashName);
		}
	}
}
