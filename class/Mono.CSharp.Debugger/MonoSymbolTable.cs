//
// System.Diagnostics.SymbolStore/MonoSymbolTable.cs
//
// Author:
//   Martin Baulig (martin@gnome.org)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections;
using System.Text;
using System.IO;
	
namespace Mono.CSharp.Debugger
{
	public struct OffsetTable
	{
		public const int Version = 19;
		public const long Magic   = 0x45e82623fd7fa614;

		public int total_file_size;
		public int source_table_offset;
		public int source_table_size;
		public int method_count;
		public int method_table_offset;
		public int method_table_size;
		public int line_number_table_offset;
		public int line_number_table_size;
		public int address_table_size;

		public OffsetTable (BinaryReader reader)
		{
			total_file_size = reader.ReadInt32 ();
			source_table_offset = reader.ReadInt32 ();
			source_table_size = reader.ReadInt32 ();
			method_count = reader.ReadInt32 ();
			method_table_offset = reader.ReadInt32 ();
			method_table_size = reader.ReadInt32 ();
			line_number_table_offset = reader.ReadInt32 ();
			line_number_table_size = reader.ReadInt32 ();
			address_table_size = reader.ReadInt32 ();
		}

		public void Write (BinaryWriter bw)
		{
			bw.Write (total_file_size);
			bw.Write (source_table_offset);
			bw.Write (source_table_size);
			bw.Write (method_count);
			bw.Write (method_table_offset);
			bw.Write (method_table_size);
			bw.Write (line_number_table_offset);
			bw.Write (line_number_table_size);
			bw.Write (address_table_size);
		}
	}

	public struct LineNumberEntry
	{
		public readonly int Row;
		public readonly int Offset;

		public LineNumberEntry (int row, int offset)
		{
			this.Row = row;
			this.Offset = offset;
		}

		internal LineNumberEntry (ISourceLine line)
			: this (line.Row, line.Offset)
		{ }

		public LineNumberEntry (BinaryReader reader)
		{
			Row = reader.ReadInt32 ();
			Offset = reader.ReadInt32 ();
		}

		internal void Write (BinaryWriter bw)
		{
			bw.Write (Row);
			bw.Write (Offset);
		}

		public override string ToString ()
		{
			return String.Format ("[Line {0}:{1}]", Row, Offset);
		}
	}

	public class MethodAddress
	{
		public readonly long StartAddress;
		public readonly long EndAddress;
		public readonly int[] LineAddresses;

		public static int Size {
			get {
				return 3 * 8;
			}
		}

		public MethodAddress (MethodEntry entry, BinaryReader reader)
		{
			StartAddress = reader.ReadInt64 ();
			EndAddress = reader.ReadInt64 ();
			LineAddresses = new int [entry.NumLineNumbers];
			for (int i = 0; i < entry.NumLineNumbers; i++)
				LineAddresses [i] = reader.ReadInt32 ();
		}

		public override string ToString ()
		{
			return String.Format ("[Address {0:x}:{1:x}]",
					      StartAddress, EndAddress);
		}
	}

	public class MethodEntry
	{
		public readonly int Token;
		public readonly int StartRow;
		public readonly int EndRow;
		public readonly int NumLineNumbers;

		public readonly int SourceFileOffset;
		public readonly int LineNumberTableOffset;
		public readonly int AddressTableOffset;
		public readonly int AddressTableSize;

		public readonly string SourceFile = null;
		public readonly LineNumberEntry[] LineNumbers = null;
		public readonly MethodAddress Address = null;

		public static int Size
		{
			get {
				return 32;
			}
		}

		public MethodEntry (BinaryReader reader, BinaryReader address_reader)
		{
			Token = reader.ReadInt32 ();
			StartRow = reader.ReadInt32 ();
			EndRow = reader.ReadInt32 ();
			NumLineNumbers = reader.ReadInt32 ();

			SourceFileOffset = reader.ReadInt32 ();
			LineNumberTableOffset = reader.ReadInt32 ();
			AddressTableOffset = reader.ReadInt32 ();
			AddressTableSize = reader.ReadInt32 ();

			if (SourceFileOffset != 0) {
				long old_pos = reader.BaseStream.Position;
				reader.BaseStream.Position = SourceFileOffset;
				SourceFile = reader.ReadString ();
				reader.BaseStream.Position = old_pos;
			}

			// Console.WriteLine ("METHOD ENTRY: " + this);

			if (LineNumberTableOffset != 0) {
				long old_pos = reader.BaseStream.Position;
				reader.BaseStream.Position = LineNumberTableOffset;

				LineNumbers = new LineNumberEntry [NumLineNumbers];

				for (int i = 0; i < NumLineNumbers; i++) {
					LineNumbers [i] = new LineNumberEntry (reader);
					// Console.WriteLine ("LINE: " + LineNumbers [i]);
				}

				reader.BaseStream.Position = old_pos;
			}

			if (AddressTableSize != 0) {
				long old_pos = address_reader.BaseStream.Position;
				address_reader.BaseStream.Position = AddressTableOffset;
				int is_valid = address_reader.ReadInt32 ();
				if (is_valid != 0) {
					Address = new MethodAddress (this, address_reader);
					// Console.WriteLine ("ADDRESS: " + Address);
				}
				address_reader.BaseStream.Position = old_pos;
			}
		}

		internal MethodEntry (int token, int sf_offset, string source_file,
				      LineNumberEntry[] lines, int lnt_offset,
				      int addrtab_offset, int addrtab_size,
				      int start_row, int end_row)
		{
			this.Token = token;
			this.StartRow = start_row;
			this.EndRow = end_row;
			this.NumLineNumbers = lines.Length;
			this.SourceFileOffset = sf_offset;
			this.LineNumberTableOffset = lnt_offset;
			this.AddressTableOffset = addrtab_offset;
			this.AddressTableSize = addrtab_size;
			this.SourceFile = source_file;
			this.LineNumbers = lines;
		}

		internal void Write (BinaryWriter bw)
		{
			bw.Write (Token);
			bw.Write (StartRow);
			bw.Write (EndRow);
			bw.Write (NumLineNumbers);
			bw.Write (SourceFileOffset);
			bw.Write (LineNumberTableOffset);
			bw.Write (AddressTableOffset);
			bw.Write (AddressTableSize);
		}

		public override string ToString ()
		{
			return String.Format ("[Method {0}:{1}:{2}:{3}:{4} - {5}:{6}:{7}:{8}]",
					      Token, SourceFile, StartRow, EndRow, NumLineNumbers,
					      SourceFileOffset, LineNumberTableOffset, AddressTableOffset,
					      AddressTableSize);
		}
	}
}
