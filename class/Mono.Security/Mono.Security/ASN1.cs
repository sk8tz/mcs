//
// ASN1.cs: Abstract Syntax Notation 1 - micro-parser and generator
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Jesper Pedersen  <jep@itplus.dk>
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// (C) 2004 Novell (http://www.novell.com)
// (C) 2004 IT+ A/S (http://www.itplus.dk)
//

using System;
using System.Collections;
using System.IO;
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
	class ASN1 {

		private byte m_nTag;
		private byte[] m_aValue;
		private ArrayList elist;

		public ASN1 () : this (0x00, null) {}

		public ASN1 (byte tag) : this (tag, null) {}

		public ASN1 (byte tag, byte[] data) 
		{
			m_nTag = tag;
			m_aValue = data;
		}

		public ASN1 (byte[] data) 
		{
			m_nTag = data [0];

			int nLenLength = 0;
			int nLength = data [1];

			if (nLength > 0x80) {
				// composed length
				nLenLength = nLength - 0x80;
				nLength = 0;
				for (int i = 0; i < nLenLength; i++) {
					nLength *= 256;
					nLength += data [i + 2];
				}
			}

			m_aValue = new byte [nLength];
			Buffer.BlockCopy (data, (2 + nLenLength), m_aValue, 0, nLength);

			if ((m_nTag & 0x20) == 0x20) {
				int nStart = (2 + nLenLength);
				Decode (data, ref nStart, data.Length);
			}
		}

		public int Count {
			get { 
				if (elist == null)
					return 0;
				return elist.Count; 
			}
		}

		public byte Tag {
			get { return m_nTag; }
		}

		public int Length {
			get { 
				if (m_aValue != null)
					return m_aValue.Length; 
				else
					return 0;
			}
		}

		public byte[] Value {
			get { 
				if (m_aValue == null)
					GetBytes ();
				return (byte[]) m_aValue.Clone (); 
			}
			set { 
				if (value != null)
					m_aValue = (byte[]) value.Clone (); 
			}
		}

		private bool CompareArray (byte[] array1, byte[] array2)
		{
			bool bResult = (array1.Length == array2.Length);
			if (bResult) {
				for (int i = 0; i < array1.Length; i++) {
					if (array1[i] != array2[i])
						return false;
				}
			}
			return bResult;
		}

		public bool Equals (byte[] asn1) 
		{
			return CompareArray (this.GetBytes (), asn1);
		}

		public bool CompareValue (byte[] value) 
		{
			return CompareArray (m_aValue, value);
		}

		public ASN1 Add (ASN1 asn1) 
		{
			if (asn1 != null) {
				if (elist == null)
					elist = new ArrayList ();
				elist.Add (asn1);
			}
			return asn1;
		}

		public virtual byte[] GetBytes () 
		{
			byte[] val = null;
			if (m_aValue != null) {
				val = m_aValue;
			}
			else if (Count > 0) {
				int esize = 0;
				ArrayList al = new ArrayList ();
				foreach (ASN1 a in elist) {
					byte[] item = a.GetBytes ();
					al.Add (item);
					esize += item.Length;
				}
				val = new byte [esize];
				int pos = 0;
				for (int i=0; i < elist.Count; i++) {
					byte[] item = (byte[]) al[i];
					Buffer.BlockCopy (item, 0, val, pos, item.Length);
					pos += item.Length;
				}
			}

			byte[] der;
			int nLengthLen = 0;

			if (val != null) {
				int nLength = val.Length;
				// special for length > 127
				if (nLength > 127) {
					if (nLength < 256) {
						der = new byte [3 + nLength];
						Buffer.BlockCopy (val, 0, der, 3, nLength);
						nLengthLen += 0x81;
						der[2] = (byte)(nLength);
					}
					else {
						der = new byte [4 + nLength];
						Buffer.BlockCopy (val, 0, der, 4, nLength);
						nLengthLen += 0x82;
						der[2] = (byte)(nLength / 256);
						der[3] = (byte)(nLength % 256);
					}
				}
				else {
					der = new byte [2 + nLength];
					Buffer.BlockCopy (val, 0, der, 2, nLength);
					nLengthLen = nLength;
				}
				if (m_aValue == null)
					m_aValue = val;
			}
			else
				der = new byte[2];

			der[0] = m_nTag;
			der[1] = (byte)nLengthLen;

			return der;
		}

		// Note: Recursive
		protected void Decode (byte[] asn1, ref int anPos, int anLength) 
		{
			byte nTag;
			int nLength;
			byte[] aValue;

			// minimum is 2 bytes (tag + length of 0)
			while (anPos < anLength - 1) {
				int nPosOri = anPos;
				DecodeTLV (asn1, ref anPos, out nTag, out nLength, out aValue);

				ASN1 elm = Add (new ASN1 (nTag, aValue));

				if ((nTag & 0x20) == 0x20) {
					int nConstructedPos = anPos;
					elm.Decode (asn1, ref nConstructedPos, nConstructedPos + nLength);
				}
				anPos += nLength; // value length
			}
		}

		// TLV : Tag - Length - Value
		protected void DecodeTLV (byte[] asn1, ref int pos, out byte tag, out int length, out byte[] content) 
		{
			tag = asn1 [pos++];
			length = asn1 [pos++];

			// special case where L contains the Length of the Length + 0x80
			if ((length & 0x80) == 0x80) {
				int nLengthLen = length & 0x7F;
				length = 0;
				for (int i = 0; i < nLengthLen; i++)
					length = length * 256 + asn1 [pos++];
			}

			content = new byte [length];
			Buffer.BlockCopy (asn1, pos, content, 0, length);
		}

		public ASN1 this [int index] {
			get { 		
				try {
					if ((elist == null) || (index >= elist.Count))
						return null;
					return (ASN1) elist [index];
				}
				catch (ArgumentOutOfRangeException) {
					return null;
				}
			}
		}

		public ASN1 Element (int index, byte anTag) 
		{
			try {
				if ((elist == null) || (index >= elist.Count))
					return null;

				ASN1 elm = (ASN1) elist [index];
				if (elm.Tag == anTag)
					return elm;
				else
					return null;
			}
			catch (ArgumentOutOfRangeException) {
				return null;
			}
		}

		public override string ToString()
		{
			string lineSeperator = Environment.NewLine;

			StringBuilder hexLine = new StringBuilder ();
            
			// Add tag
			hexLine.Append ("Tag: ");
			hexLine.Append (System.Convert.ToString (Tag, 16));
			hexLine.Append (lineSeperator);

			// Add value
			hexLine.Append ("Value: ");
			hexLine.Append (lineSeperator);
			for (int i = 0; i < Value.Length; i++) {
				if (Value[i] < 16) {
					hexLine.Append ("0");
				}
				hexLine.Append (System.Convert.ToString (Value [i], 16));
				hexLine.Append (" ");
				if ((i+1) % 16 == 0) {
					hexLine.Append (lineSeperator);
				}
			}
			return hexLine.ToString ();
		}

		public void SaveToFile (string filename)
		{
			if (filename == null)
				throw new ArgumentNullException ("filename");

			using (FileStream fs = File.OpenWrite (filename)) {
				byte[] data = GetBytes ();
				fs.Write (data, 0, data.Length);
				fs.Flush ();
				fs.Close ();
			}
		}
	}
}
