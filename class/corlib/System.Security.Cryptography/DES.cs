//
// System.Security.Cryptography.DES
//
// Author:
//	Sergey Chaban (serge@wildwestsoftware.com)
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Globalization;
using System.Security.Cryptography;

// References:
// a.	FIPS PUB 46-3: Data Encryption Standard
//	http://csrc.nist.gov/publications/fips/fips46-3/fips46-3.pdf

namespace System.Security.Cryptography {

public abstract class DES : SymmetricAlgorithm {

	private static int blockSizeByte = 8;

	public DES ()
	{
		KeySizeValue = 64; 
		BlockSizeValue = 64; 
		FeedbackSizeValue = 64;

		LegalKeySizesValue = new KeySizes[1];
		LegalKeySizesValue[0] = new KeySizes(64, 64, 0);

		LegalBlockSizesValue = new KeySizes[1];
		LegalBlockSizesValue[0] = new KeySizes(64, 64, 0);
	}

	public static new DES Create () 
	{
		return Create ("System.Security.Cryptography.DES");
	}

	public static new DES Create (string algo) 
        {
		return (DES) CryptoConfig.CreateFromName (algo);
	}

	internal static ulong PackKey (byte[] key) 
	{
		byte[] paritySetKey = new byte [blockSizeByte];
		// adapted from bouncycastle - see bouncycastle.txt
		for (int i=0; i < key.Length; i++) {
			byte b = key [i];
			paritySetKey [i] = (byte)((b & 0xfe) |
				((((b >> 1) ^ (b >> 2) ^ (b >> 3) ^ (b >> 4) ^
				(b >> 5) ^ (b >> 6) ^ (b >> 7)) ^ 0x01) & 0x01));
		}

		ulong res = 0;
		for (int i = 0, sh = 8*blockSizeByte; (sh = sh - 8) >= 0; i++) {
			res |= (ulong) paritySetKey [i] << sh;
		}

		Array.Clear (paritySetKey, 0, paritySetKey.Length);
		return res;
	}

	// Ek(Ek(m)) = m
	internal static ulong[] weakKeys = {
		0x0101010101010101, /* 0000000 0000000 */
		0xFEFEFEFEFEFEFEFE, /* FFFFFFF FFFFFFF */
		0x1F1F1F1F0E0E0E0E, /* 0000000 FFFFFFF */
		0xE0E0E0E0F1F1F1F1  /* FFFFFFF 0000000 */
	};

	// Ek1(Ek2(m)) = m
	internal static ulong[] semiweakKeys = {
		0x01FE01FE01FE01FE, 0xFE01FE01FE01FE01,
		0x1FE01FE00EF10EF1, 0xE01FE01FF10EF10E,
		0x01E001E001F101F1, 0xE001E001F101F101,
		0x1FFE1FFE0EFE0EFE, 0xFE1FFE1FFE0EFE0E,
		0x011F011F010E010E, 0x1F011F010E010E01,
		0xE0FEE0FEF1FEF1FE, 0xFEE0FEE0FEF1FEF1
	};

	public static bool IsWeakKey (byte[] rgbKey) 
	{
		if (rgbKey.Length != blockSizeByte)
			throw new CryptographicException (Locale.GetText ("Wrong Key Length"));

		ulong lk = PackKey (rgbKey);
		foreach (ulong wk in weakKeys) {
			if (lk == wk)
				return true;
		}
		return false;
	}

	public static bool IsSemiWeakKey (byte[] rgbKey)
	{
		if (rgbKey.Length != blockSizeByte)
			throw new CryptographicException (Locale.GetText ("Wrong Key Length"));

		ulong lk = PackKey (rgbKey);
		foreach (ulong swk in semiweakKeys) {
			if (lk == swk)
				return true;
		}
		return false;
	}

	public override byte[] Key {
		get {
			if (KeyValue == null) {
				// generate keys as long as we get weak or semi-weak keys
				GenerateKey ();
				while (IsWeakKey (KeyValue) || IsSemiWeakKey (KeyValue))
					GenerateKey ();
			}
			return (byte[]) KeyValue.Clone ();
		}
		set {
			if (value == null)
				throw new ArgumentNullException ("Key");
			if (value.Length != blockSizeByte)
				throw new ArgumentException (Locale.GetText ("Wrong Key Length"));
			if (IsWeakKey (value))
				throw new CryptographicException (Locale.GetText ("Weak Key"));
			if (IsSemiWeakKey (value))
				throw new CryptographicException (Locale.GetText ("Semi Weak Key"));

			KeyValue = (byte[]) value.Clone ();
		}
	}
}

}
