//
// Mono.Security.Cryptography.MD4CryptoServiceProvider
//
// Authors:
//	Sebastien Pouliot (spouliot@motus.com)
//
// Copyright (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography;

namespace Mono.Security.Cryptography {

public class MD4CryptoServiceProvider : MD4 {

	private CapiHash hash;

	public MD4CryptoServiceProvider () 
	{
		hash = null;
	}

	~MD4CryptoServiceProvider () 
	{
		Dispose (true);
	}

	// 2 cases:
	// a. we were calculing a hash and want to abort
	// b. we haven't started yet
	public override void Initialize () 
	{
		State = 0;
		if (hash == null) {
			hash = new CapiHash (CryptoAPI.CALG_MD4);
		}
	}

	protected override void Dispose (bool disposing) 
	{
		if (hash != null) {
			hash.Dispose ();
			hash = null;
			// there's no unmanaged resources (so disposing isn't used)
		}
	}

	protected override void HashCore (byte[] rgb, int ibStart, int cbSize) 
	{
		if (State == 0)
			Initialize ();
		if (hash == null)
			throw new ObjectDisposedException ("MD4CryptoServiceProvider");
		State = 1;
		hash.HashCore (rgb, ibStart, cbSize);
	}

	protected override byte[] HashFinal () 
	{
		if (hash == null)
			throw new ObjectDisposedException ("MD4CryptoServiceProvider");
		State = 0;
		byte[] result = hash.HashFinal ();
		Dispose (false);
		return result;
	}
}

}
