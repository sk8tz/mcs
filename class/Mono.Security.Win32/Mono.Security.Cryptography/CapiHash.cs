//
// Mono.Security.Cryptography.CapiHash
//
// Authors:
//	Sebastien Pouliot (spouliot@motus.com)
//
// Copyright (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography;

namespace Mono.Security.Cryptography {

public class CapiHash {

	private CapiContext context;
	private IntPtr handle;
	private uint hashSize;

	public CapiHash (uint hashAlgorithm) 
	{
		context = new CapiContext ();
		Initialize (hashAlgorithm);
	}

	public CapiHash (CapiContext ctx, uint hashAlgorithm) 
	{
		context = ctx;
		Initialize (hashAlgorithm);
	}

	public CapiHash (CspParameters cspParams, uint hashAlgorithm) 
	{
		context = new CapiContext (cspParams);
		Initialize (hashAlgorithm);
	}

	public IntPtr Handle {
		get { return handle; }
	}

	public int HashSize {
		get { return (int) hashSize; }
	}

	public void Initialize (uint algo) 
	{
		if (context != null) {
			context.InternalResult = CryptoAPI.CryptCreateHash (context.Handle, algo, IntPtr.Zero, 0, ref handle);
			hashSize = 0;
			if (context.Result)
				context.InternalResult = CryptoAPI.CryptGetHashParam (handle, CryptoAPI.HP_HASHVAL, null, ref hashSize, 0);
		}
	}

	public void Dispose () 
	{
		if (handle != IntPtr.Zero) {
			CryptoAPI.CryptDestroyHash (handle);
			handle = IntPtr.Zero;
		}
	}

	// FIXME: calling this function 1,000,000 times (with a single character)
	// is a good way to lose time (and hung NUnit)
	// TODO: find the bug that hang NUnit
	// TODO: optimize the function to call CryptHashData less often (bufferize)
	public void HashCore (byte[] data, int start, int length) 
	{
		byte[] toBeHashed = data;
		if (start != 0) {
			toBeHashed = new byte [length];
			Array.Copy (data, start, toBeHashed, 0, length);
		}
		context.InternalResult = CryptoAPI.CryptHashData (handle, toBeHashed, (uint)length, 0);
	}

	public byte[] HashFinal () 
	{
		byte[] hash = new byte [hashSize];
		context.InternalResult = CryptoAPI.CryptGetHashParam (handle, CryptoAPI.HP_HASHVAL, hash, ref hashSize, 0);
		return hash;
	}
}

}
