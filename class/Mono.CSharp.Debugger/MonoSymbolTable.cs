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
		public const int  Version = 29;
		public const long Magic   = 0x45e82623fd7fa614;

		public int TotalFileSize;
		public int DataSectionOffset;
		public int DataSectionSize;
		public int SourceCount;
		public int SourceTableOffset;
		public int SourceTableSize;
		public int MethodCount;
		public int MethodTableOffset;
		public int MethodTableSize;
		public int TypeCount;

		internal OffsetTable (BinaryReader reader)
		{
			TotalFileSize = reader.ReadInt32 ();
			DataSectionOffset = reader.ReadInt32 ();
			DataSectionSize = reader.ReadInt32 ();
			SourceCount = reader.ReadInt32 ();
			SourceTableOffset = reader.ReadInt32 ();
			SourceTableSize = reader.ReadInt32 ();
			MethodCount = reader.ReadInt32 ();
			MethodTableOffset = reader.ReadInt32 ();
			MethodTableSize = reader.ReadInt32 ();
			TypeCount = reader.ReadInt32 ();
		}

		internal void Write (BinaryWriter bw)
		{
			bw.Write (TotalFileSize);
			bw.Write (DataSectionOffset);
			bw.Write (DataSectionSize);
			bw.Write (SourceCount);
			bw.Write (SourceTableOffset);
			bw.Write (SourceTableSize);
			bw.Write (MethodCount);
			bw.Write (MethodTableOffset);
			bw.Write (MethodTableSize);
			bw.Write (TypeCount);
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

		internal LineNumberEntry (SourceLine line)
			: this (line.Row, line.Offset)
		{ }

		internal LineNumberEntry (BinaryReader reader)
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

	public struct LocalVariableEntry
	{
		public readonly string Name;
		public readonly FieldAttributes Attributes;
		public readonly byte[] Signature;

		public LocalVariableEntry (string Name, FieldAttributes Attributes, byte[] Signature)
		{
			this.Name = Name;
			this.Attributes = Attributes;
			this.Signature = Signature;
		}

		internal LocalVariableEntry (BinaryReader reader)
		{
			int name_length = reader.ReadInt32 ();
			byte[] name = reader.ReadBytes (name_length);
			Name = Encoding.UTF8.GetString (name);
			Attributes = (FieldAttributes) reader.ReadInt32 ();
			int sig_length = reader.ReadInt32 ();
			Signature = reader.ReadBytes (sig_length);
		}

		internal void Write (MonoSymbolFile file, BinaryWriter bw)
		{
			file.WriteString (bw, Name);
			bw.Write ((int) Attributes);
			bw.Write ((int) Signature.Length);
			bw.Write (Signature);
		}

		public override string ToString ()
		{
			return String.Format ("[LocalVariable {0}:{1}]", Name, Attributes);
		}
	}

	public class SourceFileEntry
	{
		MonoSymbolFile file;
		string file_name;
		ArrayList methods;
		int index, count, name_offset, method_offset;
		bool creating;

		internal static int Size {
			get { return 16; }
		}

		internal SourceFileEntry (MonoSymbolFile file, string file_name, int index)
		{
			this.file = file;
			this.file_name = file_name;
			this.index = index;

			creating = true;
			methods = new ArrayList ();
		}

		public void DefineMethod (MethodBase method, int token, LocalVariableEntry[] locals,
					  LineNumberEntry[] lines, int start, int end)
		{
			if (!creating)
				throw new InvalidOperationException ();

			MethodEntry entry = new MethodEntry (
				file, this, method, token, locals, lines, start, end);

			methods.Add (entry);
			file.AddMethod (entry);
		}

		internal void WriteData (BinaryWriter bw)
		{
			name_offset = (int) bw.BaseStream.Position;
			file.WriteString (bw, file_name);

			ArrayList list = new ArrayList ();
			foreach (MethodEntry entry in methods)
				list.Add (entry.Write (file, bw));
			list.Sort ();
			count = list.Count;

			method_offset = (int) bw.BaseStream.Position;
			foreach (MethodSourceEntry method in list)
				method.Write (bw);
		}

		internal void Write (BinaryWriter bw)
		{
			bw.Write (index);
			bw.Write (count);
			bw.Write (name_offset);
			bw.Write (method_offset);
		}

		internal SourceFileEntry (MonoSymbolFile file, BinaryReader reader)
		{
			this.file = file;

			index = reader.ReadInt32 ();
			count = reader.ReadInt32 ();
			name_offset = reader.ReadInt32 ();
			method_offset = reader.ReadInt32 ();

			file_name = file.ReadString (name_offset);
		}

		public int Index {
			get { return index; }
		}

		public string FileName {
			get { return file_name; }
		}

		public MethodSourceEntry[] Methods {
			get {
				if (creating)
					throw new InvalidOperationException ();

				BinaryReader reader = file.BinaryReader;
				int old_pos = (int) reader.BaseStream.Position;

				reader.BaseStream.Position = method_offset;
				ArrayList list = new ArrayList ();
				for (int i = 0; i < count; i ++)
					list.Add (new MethodSourceEntry (reader));
				reader.BaseStream.Position = old_pos;

				MethodSourceEntry[] retval = new MethodSourceEntry [count];
				list.CopyTo (retval, 0);
				return retval;
			}
		}

		public override string ToString ()
		{
			return String.Format ("SourceFileEntry ({0}:{1}:{2})", index, file_name, count);
		}
	}

	public struct MethodSourceEntry : IComparable
	{
		public readonly int Index;
		public readonly int FileOffset;
		public readonly int StartRow;
		public readonly int EndRow;

		public MethodSourceEntry (int index, int file_offset, int start, int end)
		{
			this.Index = index;
			this.FileOffset = file_offset;
			this.StartRow = start;
			this.EndRow = end;
		}

		internal MethodSourceEntry (BinaryReader reader)
		{
			Index = reader.ReadInt32 ();
			FileOffset = reader.ReadInt32 ();
			StartRow = reader.ReadInt32 ();
			EndRow = reader.ReadInt32 ();
		}

		public static int Size
		{
			get {
				return 16;
			}
		}

		internal void Write (BinaryWriter bw)
		{
			bw.Write (Index);
			bw.Write (FileOffset);
			bw.Write (StartRow);
			bw.Write (EndRow);
		}

		public int CompareTo (object obj)
		{
			MethodSourceEntry method = (MethodSourceEntry) obj;

			if (method.StartRow < StartRow)
				return -1;
			else if (method.StartRow > StartRow)
				return 1;
			else
				return 0;
		}

		public override string ToString ()
		{
			return String.Format ("MethodSourceEntry ({0}:{1}:{2}:{3})",
					      Index, FileOffset, StartRow, EndRow);
		}
	}

	public class MethodEntry
	{
		#region This is actually written to the symbol file
		public readonly int SourceFileIndex;
		public readonly int Token;
		public readonly int StartRow;
		public readonly int EndRow;
		public readonly int ThisTypeIndex;
		public readonly int NumParameters;
		public readonly int NumLocals;
		public readonly int NumLineNumbers;

		int NameOffset;
		int FullNameOffset;
		int TypeIndexTableOffset;
		int LocalVariableTableOffset;
		int LineNumberTableOffset;
		#endregion

		int index;
		int file_offset;
		string name;
		string full_name;

		public readonly SourceFileEntry SourceFile;
		public readonly LineNumberEntry[] LineNumbers;
		public readonly int[] ParamTypeIndices;
		public readonly int[] LocalTypeIndices;
		public readonly LocalVariableEntry[] Locals;

		public static int Size
		{
			get {
				return 48;
			}
		}

		public string Name {
			get { return name; }
		}

		public string FullName {
			get { return full_name; }
		}

		internal MethodEntry (MonoSymbolFile file, BinaryReader reader)
		{
			SourceFileIndex = reader.ReadInt32 ();
			Token = reader.ReadInt32 ();
			StartRow = reader.ReadInt32 ();
			EndRow = reader.ReadInt32 ();
			ThisTypeIndex = reader.ReadInt32 ();
			NumParameters = reader.ReadInt32 ();
			NumLocals = reader.ReadInt32 ();
			NumLineNumbers = reader.ReadInt32 ();
			NameOffset = reader.ReadInt32 ();
			FullNameOffset = reader.ReadInt32 ();
			TypeIndexTableOffset = reader.ReadInt32 ();
			LocalVariableTableOffset = reader.ReadInt32 ();
			LineNumberTableOffset = reader.ReadInt32 ();

			name = file.ReadString (NameOffset);
			full_name = file.ReadString (FullNameOffset);

			SourceFile = file.GetSourceFile (SourceFileIndex);

			if (LineNumberTableOffset != 0) {
				long old_pos = reader.BaseStream.Position;
				reader.BaseStream.Position = LineNumberTableOffset;

				LineNumbers = new LineNumberEntry [NumLineNumbers];

				for (int i = 0; i < NumLineNumbers; i++)
					LineNumbers [i] = new LineNumberEntry (reader);

				reader.BaseStream.Position = old_pos;
			}

			if (LocalVariableTableOffset != 0) {
				long old_pos = reader.BaseStream.Position;
				reader.BaseStream.Position = LocalVariableTableOffset;

				Locals = new LocalVariableEntry [NumLocals];

				for (int i = 0; i < NumLocals; i++)
					Locals [i] = new LocalVariableEntry (reader);

				reader.BaseStream.Position = old_pos;
			}

			if (TypeIndexTableOffset != 0) {
				long old_pos = reader.BaseStream.Position;
				reader.BaseStream.Position = TypeIndexTableOffset;

				ParamTypeIndices = new int [NumParameters];
				LocalTypeIndices = new int [NumLocals];

				for (int i = 0; i < NumParameters; i++)
					ParamTypeIndices [i] = reader.ReadInt32 ();
				for (int i = 0; i < NumLocals; i++)
					LocalTypeIndices [i] = reader.ReadInt32 ();

				reader.BaseStream.Position = old_pos;
			}
		}

		internal MethodEntry (MonoSymbolFile file, SourceFileEntry source, MethodBase method,
				      int token, LocalVariableEntry[] locals, LineNumberEntry[] lines,
				      int start_row, int end_row)
		{
			index = file.GetNextMethodIndex ();

			Token = token;
			SourceFileIndex = source.Index;
			SourceFile = source;
			StartRow = start_row;
			EndRow = end_row;

			NumLineNumbers = lines.Length;
			LineNumbers = lines;

			ParameterInfo[] parameters = method.GetParameters ();
			if (parameters == null)
				parameters = new ParameterInfo [0];

			StringBuilder sb = new StringBuilder ();
			sb.Append (method.DeclaringType.FullName);
			sb.Append (".");
			sb.Append (method.Name);
			sb.Append ("(");
			for (int i = 0; i < parameters.Length; i++) {
				if (i > 0)
					sb.Append (",");
				sb.Append (parameters [i].ParameterType.FullName);
			}
			sb.Append (")");

			name = method.Name;
			full_name = sb.ToString ();

			NumParameters = parameters.Length;
			ParamTypeIndices = new int [NumParameters];
			for (int i = 0; i < NumParameters; i++)
				ParamTypeIndices [i] = file.DefineType (parameters [i].ParameterType);

			NumLocals = locals.Length;
			Locals = locals;

			LocalTypeIndices = new int [NumLocals];
			for (int i = 0; i < NumLocals; i++)
				LocalTypeIndices [i] = file.GetNextTypeIndex ();

			if (method.IsStatic)
				ThisTypeIndex = 0;
			else
				ThisTypeIndex = file.DefineType (method.ReflectedType);
		}

		internal MethodSourceEntry Write (MonoSymbolFile file, BinaryWriter bw)
		{
			NameOffset = (int) bw.BaseStream.Position;
			file.WriteString (bw, name);

			FullNameOffset = (int) bw.BaseStream.Position;
			file.WriteString (bw, full_name);

			TypeIndexTableOffset = (int) bw.BaseStream.Position;

			for (int i = 0; i < NumParameters; i++)
				bw.Write (ParamTypeIndices [i]);
			for (int i = 0; i < NumLocals; i++)
				bw.Write (LocalTypeIndices [i]);

			LocalVariableTableOffset = (int) bw.BaseStream.Position;

			for (int i = 0; i < NumLocals; i++)
				Locals [i].Write (file, bw);

			LineNumberTableOffset = (int) bw.BaseStream.Position;

			for (int i = 0; i < NumLineNumbers; i++)
				LineNumbers [i].Write (bw);

			file_offset = (int) bw.BaseStream.Position;

			bw.Write (SourceFileIndex);
			bw.Write (Token);
			bw.Write (StartRow);
			bw.Write (EndRow);
			bw.Write (ThisTypeIndex);
			bw.Write (NumParameters);
			bw.Write (NumLocals);
			bw.Write (NumLineNumbers);
			bw.Write (NameOffset);
			bw.Write (FullNameOffset);
			bw.Write (TypeIndexTableOffset);
			bw.Write (LocalVariableTableOffset);
			bw.Write (LineNumberTableOffset);

			return new MethodSourceEntry (index, file_offset, StartRow, EndRow);
		}

		internal void WriteIndex (BinaryWriter bw)
		{
			bw.Write (file_offset);
			bw.Write (FullNameOffset);
		}

		public override string ToString ()
		{
			return String.Format ("[Method {0}:{1}:{2}:{3}:{4} - {5} - {6}]",
					      SourceFileIndex, index, Token, StartRow, EndRow,
					      SourceFile, FullName);
		}
	}
}
