//
// MACTripleDES.cs: Handles MAC with TripleDES
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;

namespace System.Security.Cryptography {

// References:
// a.	FIPS PUB 81: DES MODES OF OPERATION 
//	MAC: Appendix F (MACDES not MACTripleDES but close enough ;-)
//	http://www.itl.nist.gov/fipspubs/fip81.htm

// Generic MAC mechanims - most of the work is done in here
// It should work with any symmetric algorithm function e.g. DES for MACDES (fips81)
internal class MACAlgorithm {
	private SymmetricAlgorithm algo;
	private ICryptoTransform enc;
	private BlockProcessor block;

	public MACAlgorithm (SymmetricAlgorithm algorithm)
	{
		algo = (SymmetricAlgorithm) algorithm;
		algo.Mode = CipherMode.CBC;
		algo.Padding = PaddingMode.Zeros;
		algo.IV = new byte [(algo.BlockSize >> 3)];
	}

	public void Initialize (byte[] key) 
	{
		algo.Key = key;
		// note: the encryptor transform amy be reused - see Final
		if (enc == null) {
			enc = algo.CreateEncryptor ();
			block = new BlockProcessor (enc);
		}
		block.Initialize ();
	}
	
	public void Core (byte[] rgb, int ib, int cb) 
	{
		block.Core (rgb, ib, cb);
	}

	public byte[] Final () 
	{
		byte[] mac = block.Final ();
		if (!enc.CanReuseTransform) {
			enc.Dispose();
			enc = null;
		}
		return mac;
	}
}

// LAMESPEC: MACTripleDES == MAC-CBC using TripleDES (not MAC-CFB).
public class MACTripleDES: KeyedHashAlgorithm {

	private TripleDES tdes;
	private MACAlgorithm mac;
	private bool m_disposed;

	public MACTripleDES ()
	{
		Setup ("TripleDES", null);
	}

	public MACTripleDES (byte[] rgbKey)
	{
		if (rgbKey == null)
			throw new ArgumentNullException ("rgbKey");
		Setup ("TripleDES", rgbKey);
	}

	public MACTripleDES (string strTripleDES, byte[] rgbKey) 
	{
		if (rgbKey == null)
			throw new ArgumentNullException ("rgbKey");
		if (strTripleDES == null)
			Setup ("TripleDES", rgbKey);
		else
			Setup (strTripleDES, rgbKey);
	}

	private void Setup (string strTripleDES, byte[] rgbKey) 
	{
		tdes = TripleDES.Create (strTripleDES);
		// if rgbKey is null we keep the randomly generated key
		if (rgbKey != null) {
			// this way we get the TripleDES key validation (like weak
			// and semi-weak keys)
			tdes.Key = rgbKey;
		}
		HashSizeValue = tdes.BlockSize;
		// we use Key property to get the additional validations 
		// (from KeyedHashAlgorithm ancestor)
		Key = tdes.Key;
		mac = new MACAlgorithm (tdes);
		m_disposed = false;
	}

	~MACTripleDES () 
	{
		Dispose (false);
	}

	protected override void Dispose (bool disposing) 
	{
		if (!m_disposed) {
			// note: we ALWAYS zeroize keys (disposing or not)

			// clear our copy of the secret key
			if (KeyValue != null)
				Array.Clear (KeyValue, 0, KeyValue.Length);
			// clear the secret key (inside TripleDES)
			if (tdes != null)
				tdes.Clear ();

			if (disposing) {
				// disposed managed stuff
				KeyValue = null;
				tdes = null;
			}
			// ancestor
			base.Dispose (disposing);
			m_disposed = true;
		}
	}

	public override void Initialize () 
	{
		if (m_disposed)
			throw new ObjectDisposedException ("MACTripleDES");
		State = 0;
		mac.Initialize (KeyValue);
	}

	protected override void HashCore (byte[] rgb, int ib, int cb) 
	{
		if (m_disposed)
			throw new ObjectDisposedException ("MACTripleDES");
		if (State == 0) {
			Initialize ();
			State = 1;
		}
		mac.Core (rgb, ib, cb);
	}

	protected override byte[] HashFinal () 
	{
		if (m_disposed)
			throw new ObjectDisposedException ("MACTripleDES");
		State = 0;
		return mac.Final ();
	}
}

}