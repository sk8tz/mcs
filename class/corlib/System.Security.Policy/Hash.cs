//
// System.Security.Policy.Hash
//
// Authors:
//	Jackson Harper (Jackson@LatitudeGeo.com)
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Jackson Harper, All rights reserved.
// Portions (C) 2002 Motus Technologies Inc. (http://www.motus.com)
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
using System.Text;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography;

namespace System.Security.Policy {

[Serializable]
public sealed class Hash : ISerializable, IBuiltInEvidence {

	private Assembly assembly;
	private byte[] data = null;

	public Hash (Assembly assembly) 
	{
		if (assembly == null)
			throw new ArgumentNullException ("assembly");
		this.assembly = assembly;
	}

	//
	// Public Properties
	//

	public byte[] MD5 {
		get {
			// fully named to avoid conflit between MD5 property and class name
			HashAlgorithm hash = System.Security.Cryptography.MD5.Create ();
			return GenerateHash (hash);
		}
	}

	public byte[] SHA1 {
		get {
			// fully named to avoid conflit between SHA1 property and class name
			HashAlgorithm hash = System.Security.Cryptography.SHA1.Create ();
			return GenerateHash (hash);
		}
	}

	//
	// Public Methods
	//

	public byte[] GenerateHash (HashAlgorithm hashAlg) 
	{
		if (hashAlg == null)
			throw new ArgumentNullException ("hashAlg");
		return hashAlg.ComputeHash (GetData ());
	}

	[MonoTODO]
	public void GetObjectData (SerializationInfo info, StreamingContext context) 
	{
		if (info == null)
			throw new ArgumentNullException ("info");
		throw new NotImplementedException ();
	}

	[MonoTODO("The Raw data seems to be different than the raw data I have")]
	public override string ToString () 
	{
		SecurityElement se = new SecurityElement (GetType ().FullName);
		se.AddAttribute ("version", "1");
		
		StringBuilder sb = new StringBuilder ();
		byte[] raw = GetData ();
		for (int i=0; i < raw.Length; i++)
			sb.Append (raw [i].ToString ("X2"));

		se.AddChild (new SecurityElement ("RawData", sb.ToString ()));
		return se.ToString ();
	}

	//
	// Private Methods
	//

	[MonoTODO("This doesn't match the MS version perfectly.")]
	private byte[] GetData () 
	{
		if (null == data) {
			// TODO we mustn't hash the complete assembly!
			// ---- Look at ToString (MS version) for what to hash (and what not to)
			// TODO we must drop the authenticode signature (if present)
			FileStream stream = new 
				FileStream (assembly.Location, FileMode.Open, FileAccess.Read);
			data = new byte [stream.Length];
			stream.Read (data, 0, (int)stream.Length);
		}

		return data;
	}

	// interface IBuiltInEvidence

	[MonoTODO]
	int IBuiltInEvidence.GetRequiredSize (bool verbose) 
	{
		return 0;
	}

	[MonoTODO]
	int IBuiltInEvidence.InitFromBuffer (char [] buffer, int position) 
	{
		return 0;
	}

	[MonoTODO]
	int IBuiltInEvidence.OutputToBuffer (char [] buffer, int position, bool verbose) 
	{
		return 0;
	}
}

}
