//
// RSAOAEPKeyExchangeFormatter.cs - Handles OAEP keyex encryption.
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace System.Security.Cryptography { 

public class RSAOAEPKeyExchangeFormatter : AsymmetricKeyExchangeFormatter {

	protected RSA rsa;
	protected RandomNumberGenerator random;

	public RSAOAEPKeyExchangeFormatter () 
	{
		rsa = null;
	}

	public RSAOAEPKeyExchangeFormatter (AsymmetricAlgorithm key) 
	{
		SetKey (key);
	}

	public override string Parameters {
		get { return null; }
	}

	public RandomNumberGenerator Rng {
		get { return random; }
		set { random = value; }
	}

	public override byte[] CreateKeyExchange (byte[] rgbData) 
	{
		throw new CryptographicException ();
	}

	public override byte[] CreateKeyExchange (byte[] rgbData, Type symAlgType) 
	{
		// documentation says that symAlgType is not used !?!
		// FIXME: must be the same as previous method ?
		return CreateKeyExchange (rgbData);
	}

	public override void SetKey (AsymmetricAlgorithm key) 
	{
		if (key is RSA) {
			rsa = (RSA) key;
		}
		else
			throw new CryptographicException ();
	}
}

}
