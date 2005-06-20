//
// Mono.CSharp.Debugger/MonoSymbolFile.cs
//
// Author:
//   Martin Baulig (martin@ximian.com)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
//

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
using System.Reflection;
using System.Collections;
using System.Text;
using System.Threading;
using System.IO;
	
namespace Mono.CompilerServices.SymbolWriter
{
	public class MonoSymbolFileException : Exception
	{
		public MonoSymbolFileException ()
			: base ()
		{ }

		public MonoSymbolFileException (string message, params object[] args)
			: base (String.Format (message, args))
		{ }
	}

	internal class MyBinaryWriter : BinaryWriter
	{
		public MyBinaryWriter (Stream stream)
			: base (stream)
		{ }

		public void WriteLeb128 (int value)
		{
			base.Write7BitEncodedInt (value);
		}
	}

	internal class MyBinaryReader : BinaryReader
	{
		public MyBinaryReader (Stream stream)
			: base (stream)
		{ }

		public int ReadLeb128 ()
		{
			return base.Read7BitEncodedInt ();
		}
	}

	public class MonoDebuggerSupport
	{
		static GetTypeFunc get_type;
		static GetMethodTokenFunc get_method_token;
		static GetMethodFunc get_method;
		static GetLocalTypeFromSignatureFunc local_type_from_sig;
		static GetGuidFunc get_guid;
		static CheckRuntimeVersionFunc check_runtime_version;
		static GetMethodIndexFunc get_method_index;
		static MakeArrayTypeFunc make_array_type;
		static ResolveTypeFunc resolve_type;
		static GetTypeTokenFunc get_type_token;

		delegate Type GetTypeFunc (Assembly assembly, int token);
		delegate int GetMethodTokenFunc (Assembly assembly, MethodBase method);
		delegate MethodBase GetMethodFunc (Assembly assembly, int token);
		delegate Type GetLocalTypeFromSignatureFunc (Assembly assembly, byte[] sig);
		delegate Guid GetGuidFunc (Module module);
		delegate string CheckRuntimeVersionFunc (string filename);
		delegate int GetMethodIndexFunc (MethodBase method);
		delegate Type MakeArrayTypeFunc (Type type, int rank);
		delegate Type ResolveTypeFunc (Module module, int token);
		delegate int GetTypeTokenFunc (Type type);

		static Delegate create_delegate (Type type, Type delegate_type, string name)
		{
			MethodInfo mi = type.GetMethod (name, BindingFlags.Static |
							BindingFlags.NonPublic);
			if (mi == null)
				throw new Exception ("Can't find " + name);

			return Delegate.CreateDelegate (delegate_type, mi);
		}

		static MonoDebuggerSupport ()
		{
			get_type = (GetTypeFunc) create_delegate (
				typeof (Assembly), typeof (GetTypeFunc),
				"MonoDebugger_GetType");

			get_method_token = (GetMethodTokenFunc) create_delegate (
				typeof (Assembly), typeof (GetMethodTokenFunc),
				"MonoDebugger_GetMethodToken");

			get_method = (GetMethodFunc) create_delegate (
				typeof (Assembly), typeof (GetMethodFunc),
				"MonoDebugger_GetMethod");

			local_type_from_sig = (GetLocalTypeFromSignatureFunc) create_delegate (
				typeof (Assembly), typeof (GetLocalTypeFromSignatureFunc),
				"MonoDebugger_GetLocalTypeFromSignature");

			get_guid = (GetGuidFunc) create_delegate (
				typeof (Module), typeof (GetGuidFunc), "Mono_GetGuid");

			check_runtime_version = (CheckRuntimeVersionFunc) create_delegate (
				typeof (Assembly), typeof (CheckRuntimeVersionFunc),
				"MonoDebugger_CheckRuntimeVersion");

			get_method_index = (GetMethodIndexFunc) create_delegate (
				typeof (Assembly), typeof (GetMethodIndexFunc),
				"MonoDebugger_GetMethodIndex");

			make_array_type = (MakeArrayTypeFunc) create_delegate (
				typeof (Assembly), typeof (MakeArrayTypeFunc),
				"MonoDebugger_MakeArrayType");

			resolve_type = (ResolveTypeFunc) create_delegate (
				typeof (Module), typeof (ResolveTypeFunc),
				"MonoDebugger_ResolveType");

			get_type_token = (GetTypeTokenFunc) create_delegate (
				typeof (Assembly), typeof (GetTypeTokenFunc),
				"MonoDebugger_GetTypeToken");
		}

		public static Type GetType (Assembly assembly, int token)
		{
			return get_type (assembly, token);
		}

		public static int GetMethodToken (MethodBase method)
		{
			return get_method_token (method.ReflectedType.Assembly, method);
		}

		public static MethodBase GetMethod (Assembly assembly, int token)
		{
			return get_method (assembly, token);
		}

		public static Type GetLocalTypeFromSignature (Assembly assembly, byte[] sig)
		{
			return local_type_from_sig (assembly, sig);
		}

		public static string CheckRuntimeVersion (string filename)
		{
			return check_runtime_version (filename);
		}

		public static int GetMethodIndex (MethodBase method)
		{
			return get_method_index (method);
		}

		public static Guid GetGuid (Module module)
		{
			return get_guid (module);
		}

		public static Type MakeArrayType (Type type, int rank)
		{
			return make_array_type (type, rank);
		}

		public static Type ResolveType (Module module, int token)
		{
			return resolve_type (module, token);
		}

		public static int GetTypeToken (Type type)
		{
			return get_type_token (type);
		}
	}

	public class MonoSymbolFile : IDisposable
	{
		ArrayList methods = new ArrayList ();
		ArrayList sources = new ArrayList ();
		Hashtable method_source_hash = new Hashtable ();
		Hashtable type_hash = new Hashtable ();

		OffsetTable ot;
		int last_type_index;
		int last_method_index;
		int last_source_index;
		int last_namespace_index;

		public int NumLineNumbers;

		public MonoSymbolFile ()
		{ }

		internal int AddSource (SourceFileEntry source)
		{
			sources.Add (source);
			return ++last_source_index;
		}

		internal int DefineType (Type type)
		{
			if (type_hash.Contains (type))
				return (int) type_hash [type];

			int index = ++last_type_index;
			type_hash.Add (type, index);
			return index;
		}

		internal void AddMethod (MethodEntry entry)
		{
			methods.Add (entry);
		}

		internal int GetNextTypeIndex ()
		{
			return ++last_type_index;
		}

		internal int GetNextMethodIndex ()
		{
			return ++last_method_index;
		}

		internal int GetNextNamespaceIndex ()
		{
			return ++last_namespace_index;
		}
		
		internal string ReadString (int offset)
		{
			int old_pos = (int) reader.BaseStream.Position;
			reader.BaseStream.Position = offset;

			string text = reader.ReadString ();

			reader.BaseStream.Position = old_pos;
			return text;
		}

		void Write (MyBinaryWriter bw, Guid guid)
		{
			// Magic number and file version.
			bw.Write (OffsetTable.Magic);
			bw.Write (OffsetTable.Version);

			bw.Write (guid.ToByteArray ());

			//
			// Offsets of file sections; we must write this after we're done
			// writing the whole file, so we just reserve the space for it here.
			//
			long offset_table_offset = bw.BaseStream.Position;
			ot.Write (bw);

			//
			// Sort the methods according to their tokens and update their index.
			//
			methods.Sort ();
			for (int i = 0; i < methods.Count; i++)
				((MethodEntry) methods [i]).Index = i + 1;

			//
			// Write data sections.
			//
			ot.DataSectionOffset = (int) bw.BaseStream.Position;
			foreach (SourceFileEntry source in sources)
				source.WriteData (bw);
			ot.DataSectionSize = (int) bw.BaseStream.Position - ot.DataSectionOffset;

			//
			// Write the method index table.
			//
			ot.MethodTableOffset = (int) bw.BaseStream.Position;
			for (int i = 0; i < methods.Count; i++) {
				MethodEntry entry = (MethodEntry) methods [i];
				entry.WriteIndex (bw);
			}
			ot.MethodTableSize = (int) bw.BaseStream.Position - ot.MethodTableOffset;

			//
			// Write source table.
			//
			ot.SourceTableOffset = (int) bw.BaseStream.Position;
			for (int i = 0; i < sources.Count; i++) {
				SourceFileEntry source = (SourceFileEntry) sources [i];
				source.Write (bw);
			}
			ot.SourceTableSize = (int) bw.BaseStream.Position - ot.SourceTableOffset;

			//
			// Fixup offset table.
			//
			ot.TypeCount = last_type_index;
			ot.MethodCount = methods.Count;
			ot.SourceCount = sources.Count;

			//
			// Write offset table.
			//
			ot.TotalFileSize = (int) bw.BaseStream.Position;
			bw.Seek ((int) offset_table_offset, SeekOrigin.Begin);
			ot.Write (bw);
			bw.Seek (0, SeekOrigin.End);
		}

		public void CreateSymbolFile (Guid guid, FileStream fs)
		{
			if (reader != null)
				throw new InvalidOperationException ();
			
			Write (new MyBinaryWriter (fs), guid);
		}

		Assembly assembly;
		MyBinaryReader reader;
		Hashtable method_hash;
		Hashtable source_file_hash;

		Hashtable method_token_hash;
		Hashtable source_name_hash;

		protected MonoSymbolFile (string filename, Assembly assembly)
		{
			this.assembly = assembly;

			FileStream stream = new FileStream (filename, FileMode.Open, FileAccess.Read);
			reader = new MyBinaryReader (stream);

			Guid guid;

			try {
				long magic = reader.ReadInt64 ();
				long version = reader.ReadInt32 ();
				if (magic != OffsetTable.Magic)
					throw new MonoSymbolFileException (
						"Symbol file `{0}' is not a valid " +
						"Mono symbol file", filename);
				if (version != OffsetTable.Version)
					throw new MonoSymbolFileException (
						"Symbol file `{0}' has version {1}, " +
						"but expected {2}", filename, version,
						OffsetTable.Version);

				guid = new Guid (reader.ReadBytes (16));

				ot = new OffsetTable (reader);
			} catch {
				throw new MonoSymbolFileException (
					"Cannot read symbol file `{0}'", filename);
			}

			Module[] modules = assembly.GetModules ();
			Guid assembly_guid = MonoDebuggerSupport.GetGuid (modules [0]);

			if (guid != assembly_guid)
				throw new MonoSymbolFileException (
					"Symbol file `{0}' does not match assembly `{1}'",
					filename, assembly.Location);

			method_hash = new Hashtable ();
			source_file_hash = new Hashtable ();
		}

		public static MonoSymbolFile ReadSymbolFile (Assembly assembly)
		{
			string filename = assembly.Location;
			string name = filename + ".mdb";

			return new MonoSymbolFile (name, assembly);
		}

		public Assembly Assembly {
			get { return assembly; }
		}

		public int SourceCount {
			get { return ot.SourceCount; }
		}

		public int MethodCount {
			get { return ot.MethodCount; }
		}

		public int TypeCount {
			get { return ot.TypeCount; }
		}

		public int NamespaceCount {
			get { return last_namespace_index; }
		}

		internal int LineNumberCount = 0;
		internal int LocalCount = 0;
		internal int StringSize = 0;

		public SourceFileEntry GetSourceFile (int index)
		{
			if ((index < 1) || (index > ot.SourceCount))
				throw new ArgumentException ();
			if (reader == null)
				throw new InvalidOperationException ();

			SourceFileEntry source = (SourceFileEntry) source_file_hash [index];
			if (source != null)
				return source;

			reader.BaseStream.Position = ot.SourceTableOffset +
				SourceFileEntry.Size * (index - 1);
			source = new SourceFileEntry (this, reader);
			source_file_hash.Add (index, source);
			return source;
		}

		public SourceFileEntry[] Sources {
			get {
				if (reader == null)
					throw new InvalidOperationException ();

				SourceFileEntry[] retval = new SourceFileEntry [SourceCount];
				for (int i = 0; i < SourceCount; i++)
					retval [i] = GetSourceFile (i + 1);
				return retval;
			}
		}

		public MethodIndexEntry GetMethodIndexEntry (int index)
		{
			int old_pos = (int) reader.BaseStream.Position;
			reader.BaseStream.Position = ot.MethodTableOffset +
				MethodIndexEntry.Size * (index - 1);
			MethodIndexEntry ie = new MethodIndexEntry (reader);
			reader.BaseStream.Position = old_pos;
			return ie;
		}

		public MethodEntry GetMethodByToken (int token)
		{
			if (reader == null)
				throw new InvalidOperationException ();

			if (method_token_hash == null) {
				method_token_hash = new Hashtable ();

				for (int i = 0; i < MethodCount; i++) {
					MethodIndexEntry ie = GetMethodIndexEntry (i + 1);

					method_token_hash.Add (ie.Token, i + 1);
				}
			}

			object value = method_token_hash [token];
			if (value == null)
				return null;

			return GetMethod ((int) value);
		}

		public MethodEntry GetMethod (MethodBase method)
		{
			if (reader == null)
				throw new InvalidOperationException ();
			int token = MonoDebuggerSupport.GetMethodToken (method);
			return GetMethodByToken (token);
		}

		public MethodEntry GetMethod (int index)
		{
			if ((index < 1) || (index > ot.MethodCount))
				throw new ArgumentException ();
			if (reader == null)
				throw new InvalidOperationException ();

			MethodEntry entry = (MethodEntry) method_hash [index];
			if (entry != null)
				return entry;

			MethodIndexEntry ie = GetMethodIndexEntry (index);
			reader.BaseStream.Position = ie.FileOffset;

			entry = new MethodEntry (this, reader, index);
			method_hash.Add (index, entry);
			return entry;
		}

		public MethodEntry[] Methods {
			get {
				if (reader == null)
					throw new InvalidOperationException ();

				MethodEntry[] retval = new MethodEntry [MethodCount];
				for (int i = 0; i < MethodCount; i++)
					retval [i] = GetMethod (i + 1);
				return retval;
			}
		}

		public MethodSourceEntry GetMethodSource (int index)
		{
			if ((index < 1) || (index > ot.MethodCount))
				throw new ArgumentException ();
			if (reader == null)
				throw new InvalidOperationException ();

			object entry = method_source_hash [index];
			if (entry != null)
				return (MethodSourceEntry) entry;

			MethodEntry method = GetMethod (index);
			foreach (MethodSourceEntry source in method.SourceFile.Methods) {
				if (source.Index == index) {
					method_source_hash.Add (index, source);
					return source;
				}
			}

			throw new MonoSymbolFileException ("Internal error.");
		}

		public int FindSource (string file_name)
		{
			if (reader == null)
				throw new InvalidOperationException ();

			if (source_name_hash == null) {
				source_name_hash = new Hashtable ();

				for (int i = 0; i < ot.SourceCount; i++) {
					SourceFileEntry source = GetSourceFile (i + 1);

					source_name_hash.Add (source.FileName, i);
				}
			}

			object value = source_name_hash [file_name];
			if (value == null)
				return -1;
			return (int) value;
		}

		internal MyBinaryReader BinaryReader {
			get {
				if (reader == null)
					throw new InvalidOperationException ();

				return reader;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposing) {
				if (reader != null) {
					reader.Close ();
					reader = null;
				}
			}
		}
	}
}
