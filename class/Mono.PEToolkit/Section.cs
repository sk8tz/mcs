/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace Mono.PEToolkit {

	public class Section {

		// IMAGE_SECTION_HEADER
		[StructLayoutAttribute(LayoutKind.Sequential)]
		public struct Header {
			internal uint  phAddr_virtSize;
			internal RVA   virtAddr;
			internal uint  rawSize;
			internal RVA   rawDataPtr;
			internal RVA   relocPtr;
			internal RVA   lineNumPtr;
			internal short relocNum;
			internal short linenumNum;
			internal SectionCharacteristics flags;
		}

		private string name;
		internal Header hdr;

		public readonly static Section Invalid;

		static Section()
		{
			Invalid = new Section();
		}

		public Section()
		{
		}


		public string Name {
			get {
				return name;
			}
			set {
				name = value;
			}
		}


		public uint PhysicalAddress {
			get {
				return hdr.phAddr_virtSize;
			}
			set {
				hdr.phAddr_virtSize = value;
			}
		}

		public uint VirtualSize {
			get {
				return hdr.phAddr_virtSize;
			}
			set {
				hdr.phAddr_virtSize = value;
			}
		}

		public RVA VirtualAddress {
			get {
				return hdr.virtAddr;
			}
			set {
				hdr.virtAddr = value;
			}
		}

		public uint SizeOfRawData {
			get {
				return hdr.rawSize;
			}
			set {
				hdr.rawSize = value;
			}
		}

		public RVA PointerToRawData {
			get {
				return hdr.rawDataPtr;
			}
			set {
				hdr.rawDataPtr = value;
			}
		}
		
		public RVA PointerToRelocations {
			get {
				return hdr.relocPtr;
			}
			set {
				hdr.relocPtr = value;
			}
		}
		
		public RVA PointerToLinenumbers {
			get {
				return hdr.lineNumPtr;
			}
			set {
				hdr.lineNumPtr = value;
			}
		}
		
		public short NumberOfRelocations {
			get {
				return hdr.relocNum;
			}
			set {
				hdr.relocNum = value;
			}
		}
		
		public short NumberOfLinenumbers {
			get {
				return hdr.linenumNum;
			}
			set {
				hdr.linenumNum = value;
			}
		}
		
		public SectionCharacteristics Characteristics {
			get {
				return hdr.flags;
			}
			set {
				hdr.flags = value;
			}
		}



		/// <summary>
		/// </summary>
		unsafe public void Read(BinaryReader reader)
		{
			sbyte* pName = stackalloc sbyte [8];
			sbyte* p = pName;

			for (int i = 8; --i >=0;) {
				sbyte c = reader.ReadSByte();
				if (c == 0) break;
				*p++ = c;
			}

			int len = (int) (p - pName);
			if (len != 0) {
				name = new string(pName, 0, len, Encoding.ASCII);
			} else {
				name = String.Empty;
			}

			reader.BaseStream.Position += 8 - len - 1;

			fixed (void* pHdr = &this.hdr) {
				PEUtils.ReadStruct(reader, pHdr, sizeof (Header), typeof (Header));
			}
		}

	}

}

