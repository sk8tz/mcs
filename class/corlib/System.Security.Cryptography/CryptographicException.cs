//
// System.Security.Cryptography.CryptographicException.cs
//
// Author:
//   Thomas Neidhart (tome@sbox.tugraz.at)
//
// (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace System.Security.Cryptography {

[Serializable]
public class CryptographicException : SystemException {

	public CryptographicException ()
		: base (Locale.GetText ("Error occured during a cryptographic operation."))
	{
		// default to CORSEC_E_CRYPTO
		// defined as EMAKEHR(0x1430) in CorError.h
		HResult = unchecked ((int)0x80131430);
	}

	public CryptographicException (int hr)
	{
		HResult = hr;
	}

	public CryptographicException (string message)
		: base (message)
	{
		HResult = unchecked ((int)0x80131430);
	}

	public CryptographicException (string message, Exception inner)
		: base (message, inner)
	{
		HResult = unchecked ((int)0x80131430);
	}

	public CryptographicException (string format, string insert)
		: base (String.Format (format, insert))
	{
		HResult = unchecked ((int)0x80131430);
	}

	protected CryptographicException (SerializationInfo info, StreamingContext context)
		: base (info, context) 
	{
	}
}

}
