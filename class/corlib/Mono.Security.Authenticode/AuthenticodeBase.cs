//
// AuthenticodeBase.cs: Authenticode signature base class
//
// Author:
//	Sebastien Pouliot <sebastien@ximian.com>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;
using System.Security.Cryptography;

namespace Mono.Security.Authenticode {

	// References:
	// a.	http://www.cs.auckland.ac.nz/~pgut001/pubs/authenticode.txt

#if INSIDE_CORLIB
	internal
#else
	public
#endif
	enum Authority {
		Individual,
		Commercial,
		Maximum
	}

#if INSIDE_CORLIB
	internal
#else
	public
#endif
	class AuthenticodeBase {

		public const string spcIndirectDataContext = "1.3.6.1.4.1.311.2.1.4";

		internal byte[] rawData;

		public AuthenticodeBase ()
		{
		}

		protected byte[] HashFile (string fileName, string hashName) 
		{
			FileStream fs = new FileStream (fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			byte[] file = new byte [fs.Length];
			fs.Read (file, 0, file.Length);
			fs.Close ();

			// MZ - DOS header
			if (BitConverterLE.ToUInt16 (file, 0) != 0x5A4D)
				return null;

			// find offset of PE header
			int peOffset = BitConverterLE.ToInt32 (file, 60);
			if (peOffset > file.Length)
				return null;

			// PE - NT header
			if (BitConverterLE.ToUInt16 (file, peOffset) != 0x4550)
				return null;

			// IMAGE_DIRECTORY_ENTRY_SECURITY
			int dirSecurityOffset = BitConverterLE.ToInt32 (file, peOffset + 152);
			int dirSecuritySize = BitConverterLE.ToInt32 (file, peOffset + 156);

			if (dirSecuritySize > 8) {
				rawData = new byte [dirSecuritySize - 8];
				Buffer.BlockCopy (file, dirSecurityOffset + 8, rawData, 0, rawData.Length);
/* DEBUG 
			FileStream debug = new FileStream (fileName + ".sig", FileMode.Create, FileAccess.Write);
			debug.Write (rawData, 0, rawData.Length);
			debug.Close ();*/
			}
			else
				rawData = null;

			HashAlgorithm hash = HashAlgorithm.Create (hashName);
			// 0 to 215 (216) then skip 4 (checksum)
			int pe = peOffset + 88;
			hash.TransformBlock (file, 0, pe, file, 0);
			pe += 4;
			// 220 to 279 (60) then skip 8 (IMAGE_DIRECTORY_ENTRY_SECURITY)
			hash.TransformBlock (file, pe, 60, file, pe);
			pe += 68;
			// 288 to end of file
			int n = file.Length - pe;
			// minus any authenticode signature (with 8 bytes header)
			if (dirSecurityOffset != 0)
				n -= (dirSecuritySize);
			hash.TransformFinalBlock (file, pe, n);

			return hash.Hash;
		}
	}
}
