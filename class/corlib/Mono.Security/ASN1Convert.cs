//
// ASN1Convert.cs: Abstract Syntax Notation 1 convertion routines
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Jesper Pedersen  <jep@itplus.dk>
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
// (C) 2004 IT+ A/S (http://www.itplus.dk)
//

using System;
using System.Collections;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Mono.Security {

	// References:
	// a.	ITU ASN.1 standards (free download)
	//	http://www.itu.int/ITU-T/studygroups/com17/languages/

#if INSIDE_CORLIB
	internal
#else
	public
#endif
	sealed class ASN1Convert {

		private ASN1Convert ()
		{
		}

		// RFC3280, section 4.2.1.5
		// CAs conforming to this profile MUST always encode certificate
		// validity dates through the year 2049 as UTCTime; certificate validity
		// dates in 2050 or later MUST be encoded as GeneralizedTime.
		static public ASN1 FromDateTime (DateTime dt) 
		{
			if (dt.Year < 2050) {
				// UTCTIME
				return new ASN1 (0x17, Encoding.ASCII.GetBytes (
					dt.ToString ("yyMMddHHmmss", CultureInfo.InvariantCulture) + "Z"));
			}
			else {
				// GENERALIZEDTIME
				return new ASN1 (0x18, Encoding.ASCII.GetBytes (
					dt.ToString ("yyyyMMddHHmmss", CultureInfo.InvariantCulture) + "Z"));
			}
		}

		static public ASN1 FromInt32 (Int32 value) 
		{
			byte[] integer = BitConverterLE.GetBytes (value);
			int x = 3;
			while (integer [x] == 0x00)
				x--;
			ASN1 asn1 = new ASN1 (0x02);

			byte[] smallerInt = new byte [x + 1];
			int index = smallerInt.Length - 1;
			for (int i = 0; i < smallerInt.Length; i++) {
				smallerInt [index] = integer [i];
				index--;
			}
			asn1.Value = smallerInt;

			return asn1;
		}

		static public ASN1 FromOid (string oid) 
		{
			return new ASN1 (CryptoConfig.EncodeOID (oid));
		}

		static public ASN1 FromUnsignedBigInteger (byte[] big) 
		{
			if (big [0] == 0x00) {
				// this first byte is added so we're sure it's an unsigned integer
				// however we can't feed it into RSAParameters or DSAParameters
				int length = big.Length + 1;
				byte[] uinteger = new byte [length];
				Buffer.BlockCopy (big, 0, uinteger, 1, length);
				big = uinteger;
			}
			return new ASN1 (0x02, big);
		}

		static public int ToInt32 (ASN1 asn1) 
		{
			if (asn1.Tag != 0x02)
				throw new NotSupportedException ("Only integer can be converted");
			int x = 0;
			for (int i=0; i < asn1.Value.Length; i++)
				x = (x << 8) + asn1.Value [i];
			return x;
		}

		// Convert a binary encoded OID to human readable string representation of 
		// an OID (IETF style). Based on DUMPASN1.C from Peter Gutmann.
		static public string ToOid (ASN1 asn1) 
		{
			byte[] aOID = asn1.Value;
			StringBuilder sb = new StringBuilder ();
			// Pick apart the OID
			byte x = (byte) (aOID[0] / 40);
			byte y = (byte) (aOID[0] % 40);
			if (x > 2) {
				// Handle special case for large y if x = 2
				y += (byte) ((x - 2) * 40);
				x = 2;
			}
			sb.Append (x.ToString (CultureInfo.InvariantCulture));
			sb.Append (".");
			sb.Append (y.ToString (CultureInfo.InvariantCulture));
			ulong val = 0;
			for (x = 1; x < aOID.Length; x++) {
				val = ((val << 7) | ((byte) (aOID [x] & 0x7F)));
				if ( !((aOID [x] & 0x80) == 0x80)) {
					sb.Append (".");
					sb.Append (val.ToString (CultureInfo.InvariantCulture));
					val = 0;
				}
			}
			return sb.ToString ();
		}

		static public DateTime ToDateTime (ASN1 time) 
		{
			string t = Encoding.ASCII.GetString (time.Value);
			// to support both UTCTime and GeneralizedTime (and not so common format)
			string mask = null;
			switch (t.Length) {
				case 11:
					mask = "yyMMddHHmmZ"; // illegal I think ... must check
					break;
				case 13: 
					// RFC3280: 4.1.2.5.1  UTCTime
					int year = Convert.ToInt16 (t.Substring (0, 2), CultureInfo.InvariantCulture);
					// Where YY is greater than or equal to 50, the 
					// year SHALL be interpreted as 19YY; and 
					// Where YY is less than 50, the year SHALL be 
					// interpreted as 20YY.
					if (year >= 50)
						t = "19" + t;
					else
						t = "20" + t;
					mask = "yyyyMMddHHmmssZ";
					break;
				case 15:
					mask = "yyyyMMddHHmmssZ"; // GeneralizedTime
					break;
			}
			return DateTime.ParseExact (t, mask, null);
		}
	}
}
