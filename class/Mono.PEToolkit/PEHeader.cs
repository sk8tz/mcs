/*
 * Copyright (c) 2002 Sergey Chaban <serge@wildwestsoftware.com>
 */

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Mono.PEToolkit {

	// IMAGE_OPTIONAL_HEADER

	[StructLayoutAttribute(LayoutKind.Sequential)]
	public struct PEHeader {

		/// <summary>
		/// Standard PE/COFF fields.
		/// </summary>
		[StructLayoutAttribute(LayoutKind.Sequential)]
		public struct StdFields {
			internal short magic; // always 0x10B?
			internal byte lMajor;
			internal byte lMinor;
			internal uint codeSize;
			internal uint initDataSize;
			internal uint uninitDataSize;
			internal RVA  entryRVA;
			internal RVA  codeBase;
			internal RVA  dataBase;


			/// <summary>
			/// </summary>
			public string LinkerVersion {
				get {
					return String.Format("{0}.{1}", lMajor, lMinor);
				}
			}
			

			/// <summary>
			/// </summary>
			/// <returns></returns>
			public override string ToString() {
				return String.Format(
					"Magic                           : 0x{0}" + Environment.NewLine +
					"Linker ver.                     : {1}" + Environment.NewLine +
					"Size of code                    : {2}" + Environment.NewLine +
					"Size of initialized data        : {3}" + Environment.NewLine +
					"Size of uinitialized data (BSS) : {4}" + Environment.NewLine,
					magic.ToString("X"), LinkerVersion,
					codeSize, initDataSize, uninitDataSize
				);
			}
		}
		

		/// <summary>
		/// Windows-specific fields.
		/// </summary>
		/// <remarks>
		/// See Partition II, 24.2.3.2
		/// </remarks>
		[StructLayoutAttribute(LayoutKind.Sequential)]
		public struct NTFields {
			internal uint      imgBase;
			internal uint      sectAlign;
			internal uint      fileAlign;
			internal short     osMaj;
			internal short     osMin;
			internal short     imgMaj;
			internal short     imgMin;
			internal short     subSysMaj;
			internal short     subSysMin;
			internal int       reserved_win32ver;
			internal uint      imgSize;
			internal uint      hdrSize;
			internal uint      chksum;
			internal Subsystem subSys;
			internal short     dllFlags;
			internal uint      stackRes;
			internal uint      stackCommit;
			internal uint      heapRes;
			internal uint      heapCommit;
			internal uint      ldrFlags;
			internal uint      numDirs;

			public string OSVersion {
				get {
					return String.Format("{0}.{1}", osMaj, osMin);
				}
			}

			public string ImageVersion {
				get {
					return String.Format("{0}.{1}", imgMaj, imgMin);
				}
			}

			public string SubsysVersion {
				get {
					return String.Format("{0}.{1}", subSysMaj, subSysMin);
				}
			}


			/// <summary>
			/// </summary>
			/// <returns></returns>
			public override string ToString() {
				return String.Format(
					"Image Base            : 0x{0}" + Environment.NewLine +
					"Section Alignment     : 0x{1}" + Environment.NewLine +
					"File Alignment        : 0x{2}" + Environment.NewLine +
					"OS Version            : {3}" + Environment.NewLine +
					"Image Version         : {4}" + Environment.NewLine +
					"Subsystem Version     : {5}" + Environment.NewLine +
					"Reserved/Win32Ver     : {6}" + Environment.NewLine +
					"Image Size            : {7}" + Environment.NewLine +
					"Header Size           : {8}" + Environment.NewLine +
					"Checksum              : 0x{9}" + Environment.NewLine +
					"Subsystem             : {10}" + Environment.NewLine +
					"DLL Flags             : {11}" + Environment.NewLine +
					"Stack Reserve Size    : 0x{12}" + Environment.NewLine +
					"Stack Commit Size     : 0x{13}" + Environment.NewLine +
					"Heap Reserve Size     : 0x{14}" + Environment.NewLine +
					"Heap Commit Size      : 0x{15}" + Environment.NewLine +
					"Loader Flags          : {16}" + Environment.NewLine +
					"Number of Directories : {17}" + Environment.NewLine,
					imgBase.ToString("X"), sectAlign.ToString("X"), fileAlign.ToString("X"),
					OSVersion, ImageVersion, SubsysVersion,
					reserved_win32ver,
					imgSize, hdrSize, chksum.ToString("X"), subSys, dllFlags,
					stackRes.ToString("X"), stackCommit.ToString("X"), heapRes.ToString("X"), heapCommit.ToString ("X"),
					ldrFlags, numDirs
					);
			}
		}


		internal StdFields stdFlds;
		internal NTFields ntFlds;

		internal DataDir exportDir;
		internal DataDir importDir;
		internal DataDir resourceDir;
		internal DataDir exceptionDir;
		internal DataDir securityDir;
		internal DataDir baseRelocDir;
		internal DataDir debugDir;
		internal DataDir copyrightDir;
		internal DataDir GPDir;
		internal DataDir TLSDir;
		internal DataDir loadCfgDir;
		internal DataDir boundImpDir;
		internal DataDir IATDir;
		internal DataDir delayImpDir;
		internal DataDir CLIHdrDir;
		internal DataDir reservedDir;


		public bool IsCLIImage {
			get {
				return (CLIHdrDir.virtAddr.Value != 0);
			}
		}


		//
		// Accessors for standard COFF fields.
		//
		
		public short Magic {
			get {
				return stdFlds.magic;
			}
			set {
				stdFlds.magic = value;
			}
		}

		public byte MajorLinkerVersion {
			get {
				return stdFlds.lMajor;
			}
			set {
				stdFlds.lMajor = value;
			}
		}

		public byte MinorLinkerVersion {
			get {
				return stdFlds.lMinor;
			}
			set {
				stdFlds.lMinor = value;
			}
		}

		public uint SizeOfCode {
			get {
				return stdFlds.codeSize;
			}
			set {
				stdFlds.codeSize = value;
			}
		}

		public uint SizeOfInitializedData {
			get {
				return stdFlds.initDataSize;
			}
			set {
				stdFlds.initDataSize = value;
			}
		}

		public uint SizeOfUninitializedData {
			get {
				return stdFlds.uninitDataSize;
			}
			set {
				stdFlds.uninitDataSize = value;
			}
		}

		public RVA AddressOfEntryPoint {
			get {
				return stdFlds.entryRVA;
			}
			set {
				stdFlds.entryRVA.value = value.value;
			}
		}

		public RVA BaseOfCode {
			get {
				return stdFlds.codeBase;
			}
			set {
				stdFlds.codeBase.value = value.value;
			}
		}

		public RVA BaseOfData {
			get {
				return stdFlds.dataBase;
			}
			set {
				stdFlds.dataBase.value = value.value;
			}
		}


		//
		// Accessors for Windows-specific fields.
		//


		/// <summary>
		/// Preferred address of image when loaded into memory.
		/// </summary>
		/// <remarks>
		///  <para>
		///  This is a linear address and not RVA,
		///  and must be a multiple of 64K.
		///  </para>
		///  <para>
		///  Table in the Partition II states that for CIL images
		///  it must be 0x400000.
		///  </para>
		/// </remarks>
		public uint ImageBase {
			get {
				return ntFlds.imgBase;
			}
			set {
				ntFlds.imgBase = value;
			}
		}

		/// <summary>
		///  Alignment of section when loaded into memory.
		/// </summary>
		/// <remarks>
		///  Must be greater or equal to FileAlignment.
		///  Default is the native page size.
		///  According to specs for CIL images it must be set to 8K.
		/// </remarks>
		public uint SectionAlignment {
			get {
				return ntFlds.sectAlign;
			}
			set {
				ntFlds.sectAlign = value;
			}
		}

		/// <summary>
		///  Byte alignment of pages in image file.
		/// </summary>
		/// <remarks>
		///  Valid values are powers of 2 between 512 and 64K.
		///  For CIL images it must be either 512 or 4K.
		/// </remarks>
		public uint FileAlignment {
			get {
				return ntFlds.fileAlign;
			}
			set {
				ntFlds.fileAlign = value;
			}
		}


		public short MajorOperatingSystemVersion {
			get {
				return ntFlds.osMaj;
			}
			set {
				ntFlds.osMaj = value;
			}
		}

		public short MinorOperatingSystemVersion {
			get {
				return ntFlds.osMin;
			}
			set {
				ntFlds.osMin = value;
			}
		}

		public short MajorImageVersion {
			get {
				return ntFlds.imgMaj;
			}
			set {
				ntFlds.imgMaj = value;
			}
		}

		public short MinorImageVersion {
			get {
				return ntFlds.imgMin;
			}
			set {
				ntFlds.imgMin = value;
			}
		}

		public short MajorSubsystemVersion {
			get {
				return ntFlds.subSysMaj;
			}
			set {
				ntFlds.subSysMaj = value;
			}
		}

		public short MinorSubsystemVersion {
			get {
				return ntFlds.subSysMin;
			}
			set {
				ntFlds.subSysMin = value;
			}
		}

		public int Win32VersionValue {
			get {
				return ntFlds.reserved_win32ver;
			}
			set {
				ntFlds.reserved_win32ver = value;
			}
		}

		public int Reserved {
			get {
				return ntFlds.reserved_win32ver;
			}
			set {
				ntFlds.reserved_win32ver = value;
			}
		}

		public uint SizeOfImage {
			get {
				return ntFlds.imgSize;
			}
			set {
				ntFlds.imgSize = value;
			}
		}

		public uint SizeOfHeaders {
			get {
				return ntFlds.hdrSize;
			}
			set {
				ntFlds.hdrSize = value;
			}
		}

		public uint CheckSum {
			get {
				return ntFlds.chksum;
			}
			set {
				ntFlds.chksum = value;
			}
		}

		public Subsystem Subsystem {
			get {
				return ntFlds.subSys;
			}
			set {
				ntFlds.subSys = value;
			}
		}

		public short DllCharacteristics {
			get {
				return ntFlds.dllFlags;
			}
			set {
				ntFlds.dllFlags = value;
			}
		}


		public uint SizeOfStackReserve {
			get {
				return ntFlds.stackRes;
			}
			set {
				ntFlds.stackRes = value;
			}
		}

		public uint SizeOfStackCommit {
			get {
				return ntFlds.stackCommit;
			}
			set {
				ntFlds.stackCommit = value;
			}
		}

		public uint SizeOfHeapReserve {
			get {
				return ntFlds.heapRes;
			}
			set {
				ntFlds.heapRes = value;
			}
		}

		public uint SizeOfHeapCommit {
			get {
				return ntFlds.heapCommit;
			}
			set {
				ntFlds.heapCommit = value;
			}
		}

		public uint LoaderFlags {
			get {
				return ntFlds.ldrFlags;
			}
			set {
				ntFlds.ldrFlags = value;
			}
		}

		public uint NumberOfRvaAndSizes {
			get {
				return ntFlds.numDirs;
			}
			set {
				ntFlds.numDirs = value;
			}
		}





		/// <summary>
		/// </summary>
		unsafe public void Read(BinaryReader reader)
		{
			fixed (void* pThis = &this) {
				int hdrSize = sizeof (StdFields) + sizeof (NTFields);
				PEUtils.ReadStruct(reader, pThis, hdrSize);
				PEUtils.ReadStruct(reader, (byte*)pThis + hdrSize, (int) NumberOfRvaAndSizes * sizeof (DataDir));

				if (!System.BitConverter.IsLittleEndian) {
					PEUtils.ChangeStructEndianess(pThis, typeof (PEHeader));
				}
			}
		}



		/// <summary>
		/// </summary>
		public void Dump(TextWriter writer)
		{
			string dirs = String.Format(
				"Export Table            : {0}"  + Environment.NewLine +
				"Import Table            : {1}"  + Environment.NewLine +
				"Win32 Resource Table    : {2}"  + Environment.NewLine +
				"Exception Table         : {3}"  + Environment.NewLine +
				"Certificate Table       : {4}"  + Environment.NewLine +
				"Base Relocation Table   : {5}"  + Environment.NewLine +
				"Debug Table             : {6}"  + Environment.NewLine +
				"Copyright               : {7}"  + Environment.NewLine +
				"MIPS Global Ptr         : {8}"  + Environment.NewLine +
				"TLS Table               : {9}"  + Environment.NewLine +
				"Load Config Table       : {10}"  + Environment.NewLine +
				"Bound Import            : {11}"  + Environment.NewLine +
				"IAT                     : {12}"  + Environment.NewLine +
				"Delay Import Descriptor : {13}"  + Environment.NewLine +
				"CLI Header              : {14}"  + Environment.NewLine +
				"Reserved                : {15}"  + Environment.NewLine,
				exportDir, importDir, resourceDir, exceptionDir,
				securityDir, baseRelocDir, debugDir, copyrightDir,
				GPDir, TLSDir, loadCfgDir, boundImpDir, IATDir, delayImpDir,
				CLIHdrDir, reservedDir
			);

			writer.WriteLine(
				stdFlds.ToString() + Environment.NewLine +
				ntFlds.ToString() + Environment.NewLine +
				"Directories: "+ Environment.NewLine +
				dirs
			);
		}


		/// <summary>
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringWriter sw = new StringWriter();
			Dump(sw);
			return sw.ToString();
		}

	}

}

