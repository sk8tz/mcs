//
// System.Diagnostics.SymbolStore/MonoSymbolWriter.cs
//
// Author:
//   Martin Baulig (martin@gnome.org)
//
// This is the default implementation of the System.Diagnostics.SymbolStore.ISymbolWriter
// interface.
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Diagnostics.SymbolStore;
using System.Collections;
using System.IO;
	
namespace Mono.CSharp.Debugger
{

	public class MonoSymbolWriter : IMonoSymbolWriter
	{
		protected Assembly assembly;
		protected ModuleBuilder module_builder;
		protected string output_filename = null;
		protected ArrayList locals = null;
		protected ArrayList orphant_methods = null;
		protected ArrayList methods = null;
		protected Hashtable sources = null;

		protected class SourceFile : ISourceFile
		{
			private ArrayList _methods = new ArrayList ();
			private string _file_name;

			public SourceFile (string filename)
			{
				this._file_name = filename;
			}

			public override string ToString ()
			{
				return _file_name;
			}

			// interface ISourceFile

			public string FileName {
				get {
					return _file_name;
				}
			}

			public ISourceMethod[] Methods {
				get {
					ISourceMethod[] retval = new ISourceMethod [_methods.Count];
					_methods.CopyTo (retval);
					return retval;
				}
			}

			public void AddMethod (ISourceMethod method)
			{
				_methods.Add (method);
			}
		}

		protected class SourceBlock : ISourceBlock
		{
			static private int next_index;
			private readonly int _index;

			public SourceBlock (ISourceMethod method, ISourceLine start, ISourceLine end)
			{
				this._method = method;
				this._start = start;
				this._end = end;
				this._index = ++next_index;
			}

			internal SourceBlock (ISourceMethod method, int startOffset)
			{
				this._method = method;
				this._start = new SourceLine (startOffset);
				this._index = ++next_index;
			}

			public override string ToString ()
			{
				return "SourceBlock #" + ID + " (" + Start + " - " + End + ")";
			}

			private readonly ISourceMethod _method;
			private ArrayList _blocks = new ArrayList ();
			internal ISourceLine _start;
			internal ISourceLine _end;

			private ArrayList _locals = new ArrayList ();

			public ISourceMethod SourceMethod {
				get {
					return _method;
				}
			}

			public ISourceBlock[] Blocks {
				get {
					ISourceBlock[] retval = new ISourceBlock [_blocks.Count];
					_blocks.CopyTo (retval);
					return retval;
				}
			}

			public void AddBlock (ISourceBlock block)
			{
				_blocks.Add (block);
			}

			public ISourceLine Start {
				get {
					return _start;
				}
			}

			public ISourceLine End {
				get {
					return _end;
				}
			}

			public int ID {
				get {
					return _index;
				}
			}

			public ILocalVariable[] Locals {
				get {
					ILocalVariable[] retval = new ILocalVariable [_locals.Count];
					_locals.CopyTo (retval);
					return retval;
				}
			}

			public void AddLocal (ILocalVariable local)
			{
				_locals.Add (local);
			}
		}

		protected class SourceLine : ISourceLine
		{
			public SourceLine (int row, int column)
				: this (0, row, column)
			{
				this._type = SourceOffsetType.OFFSET_NONE;
			}

			public SourceLine (int offset, int row, int column)
			{
				this._offset = offset;
				this._row = row;
				this._column = column;
				this._type = SourceOffsetType.OFFSET_IL;
			}

			internal SourceLine (int offset)
				: this (offset, 0, 0)
			{ }

			public override string ToString ()
			{
				return "SourceLine (" + _offset + "@" + _row + ":" + _column + ")";
			}

			internal SourceOffsetType _type;
			internal int _offset;
			internal int _row;
			internal int _column;

			// interface ISourceLine

			public SourceOffsetType OffsetType {
				get {
					return _type;
				}
			}

			public int Offset {
				get {
					return _offset;
				}
			}

			public int Row {
				get {
					return _row;
				}
			}

			public int Column {
				get {
					return _column;
				}
			}
		}

		protected class LocalVariable : ILocalVariable
		{
			public LocalVariable (string name, byte[] signature, ISourceMethod method,
					      int index)
				: this (name, signature, method, index, null)
			{ }

			public LocalVariable (string name, byte[] signature, ISourceMethod method,
					      int index, ISourceLine line)
			{
				this._name = name;
				this._signature = signature;
				this._method = method;
				this._index = index;
				this._line = line;
			}

			private readonly string _name;
			internal Type _type;
			private readonly byte[] _signature;
			private readonly ISourceMethod _method;
			private readonly int _index;
			private readonly ISourceLine _line;

			public override string ToString ()
			{
				return "LocalVariable (" + _index + "," + _name + ")";
			}

			// interface ILocalVariable

			public string Name {
				get {
					return _name;
				}
			}

			public ISourceMethod Method {
				get {
					return _method;
				}
			}

			public int Index {
				get {
					return _index;
				}
			}

			public Type Type {
				get {
					return _type;
				}
			}

			public byte[] Signature {
				get {
					return _signature;
				}
			}

			public ISourceLine Line {
				get {
					return _line;
				}
			}
		}

		protected class SourceMethod : ISourceMethod
		{
			private ArrayList _lines = new ArrayList ();
			private ArrayList _blocks = new ArrayList ();
			private Hashtable _block_hash = new Hashtable ();
			private Stack _block_stack = new Stack ();

			internal readonly MethodBase _method_base;
			internal ISourceFile _source_file;
			internal int _token;

			private SourceBlock _implicit_block;

			public SourceMethod (MethodBase method_base, ISourceFile source_file)
				: this (method_base)
			{
				this._source_file = source_file;
			}

			internal SourceMethod (MethodBase method_base)
			{
				this._method_base = method_base;

				this._implicit_block = new SourceBlock (this, 0);
			}

			public void SetSourceRange (ISourceFile sourceFile,
						    int startLine, int startColumn,
						    int endLine, int endColumn)
			{
				_source_file = sourceFile;
				_implicit_block._start = new SourceLine (startLine, startColumn);
				_implicit_block._end = new SourceLine (endLine, endColumn);
			}


			public void StartBlock (ISourceBlock block)
			{
				_block_stack.Push (block);
			}

			public void EndBlock (int endOffset) {
				SourceBlock block = (SourceBlock) _block_stack.Pop ();

				block._end = new SourceLine (endOffset);

				if (_block_stack.Count > 0) {
					ISourceBlock parent = (ISourceBlock) _block_stack.Peek ();

					parent.AddBlock (block);
				} else
					_blocks.Add (block);

				_block_hash.Add (block.ID, block);
			}

			public void SetBlockRange (int BlockID, int startOffset, int endOffset)
			{
				SourceBlock block = (SourceBlock) _block_hash [BlockID];
				((SourceLine) block.Start)._offset = startOffset;
				((SourceLine) block.End)._offset = endOffset;
			}

			public ISourceBlock CurrentBlock {
				get {
					if (_block_stack.Count > 0)
						return (ISourceBlock) _block_stack.Peek ();
					else
						return _implicit_block;
				}
			}

			// interface ISourceMethod

			public ISourceLine[] Lines {
				get {
					ISourceLine[] retval = new ISourceLine [_lines.Count];
					_lines.CopyTo (retval);
					return retval;
				}
			}

			public void AddLine (ISourceLine line)
			{
				_lines.Add (line);
			}

			public ISourceBlock[] Blocks {
				get {
					ISourceBlock[] retval = new ISourceBlock [_blocks.Count];
					_blocks.CopyTo (retval);
					return retval;
				}
			}

			public ILocalVariable[] Locals {
				get {
					return _implicit_block.Locals;
				}
			}

			public void AddLocal (ILocalVariable local)
			{
				_implicit_block.AddLocal (local);
			}

			public MethodBase MethodBase {
				get {
					return _method_base;
				}
			}

			public Type ReturnType {
				get {
					if (_method_base is MethodInfo)
						return ((MethodInfo)_method_base).ReturnType;
					else if (_method_base is ConstructorInfo)
						return _method_base.DeclaringType;
					else
						throw new NotSupportedException ();
				}
			}

			public ISourceFile SourceFile {
				get {
					return _source_file;
				}
			}

			public int Token {
				get {
					if (_token != 0)
						return _token;
					else
						throw new NotSupportedException ();
				}
			}

			public ISourceLine Start {
				get {
					return _implicit_block.Start;
				}
			}

			public ISourceLine End {
				get {
					return _implicit_block.End;
				}
			}
		}

		protected SourceMethod current_method = null;
		private readonly string assembly_filename = null;

		//
		// Interface IMonoSymbolWriter
		//

		public MonoSymbolWriter (ModuleBuilder mb, string filename)
		{
			this.assembly_filename = filename;
			this.module_builder = mb;

			this.methods = new ArrayList ();
			this.sources = new Hashtable ();
			this.orphant_methods = new ArrayList ();
			this.locals = new ArrayList ();
		}

		public void Close () {
			if (assembly == null)
				assembly = Assembly.LoadFrom (assembly_filename);

			DoFixups (assembly);

			CreateDwarfFile (output_filename);
		}

		public void CloseNamespace () {
		}

		// Create and return a new IMonoSymbolDocumentWriter.
		public ISymbolDocumentWriter DefineDocument (string url,
							     Guid language,
							     Guid languageVendor,
							     Guid documentType)
		{
			return new MonoSymbolDocumentWriter (url);
		}

		public void DefineField (
			SymbolToken parent,
			string name,
			FieldAttributes attributes,
			byte[] signature,
			SymAddressKind addrKind,
			int addr1,
			int addr2,
			int addr3)
		{
		}

		public void DefineGlobalVariable (
			string name,
			FieldAttributes attributes,
			byte[] signature,
			SymAddressKind addrKind,
			int addr1,
			int addr2,
			int addr3)
		{
		}

		public void DefineLocalVariable (string name,
						 FieldAttributes attributes,
						 byte[] signature,
						 SymAddressKind addrKind,
						 int addr1,
						 int addr2,
						 int addr3,
						 int startOffset,
						 int endOffset)
		{
			if (current_method == null)
				return;

			LocalVariable local_info = new LocalVariable (name, signature,
								      current_method, addr1);

			current_method.CurrentBlock.AddLocal (local_info);
			locals.Add (local_info);
		}

		public void DefineParameter (string name,
					     ParameterAttributes attributes,
					     int sequence,
					     SymAddressKind addrKind,
					     int addr1,
					     int addr2,
					     int addr3)
		{
			throw new NotSupportedException ();
		}

		public void DefineSequencePoints (ISymbolDocumentWriter document,
						  int[] offsets,
						  int[] lines,
						  int[] columns,
						  int[] endLines,
						  int[] endColumns)
		{
			SourceLine source_line = new SourceLine (offsets [0], lines [0], columns [0]);

			if (current_method != null)
				current_method.AddLine (source_line);
		}

		public void Initialize (IntPtr emitter, string filename, bool fFullBuild)
		{
			this.output_filename = filename;
		}

		public void OpenMethod (SymbolToken symbol_token)
		{
			int token = symbol_token.GetToken ();

			MethodBuilder mb = get_method (module_builder, token);

			current_method = new SourceMethod (mb);

			methods.Add (current_method);
		}

		public void SetMethodSourceRange (ISymbolDocumentWriter startDoc,
						  int startLine, int startColumn,
						  ISymbolDocumentWriter endDoc,
						  int endLine, int endColumn)
		{
			if (current_method == null)
				return;

			if ((startDoc == null) || (endDoc == null))
				throw new NullReferenceException ();

			if (!(startDoc is MonoSymbolDocumentWriter) || !(endDoc is MonoSymbolDocumentWriter))
				throw new NotSupportedException ("both startDoc and endDoc must be of type "
								 + "MonoSymbolDocumentWriter");

			if (!startDoc.Equals (endDoc))
				throw new NotSupportedException ("startDoc and endDoc must be the same");

			string source_file = ((MonoSymbolDocumentWriter) startDoc).FileName;
			SourceFile source_info;

			if (sources.ContainsKey (source_file))
				source_info = (SourceFile) sources [source_file];
			else {
				source_info = new SourceFile (source_file);
				sources.Add (source_file, source_info);
			}

			current_method.SetSourceRange (source_info, startLine, startColumn,
						       endLine, endColumn);

			source_info.AddMethod (current_method);
		}

		public void CloseMethod () {
			current_method = null;
		}

		public void OpenNamespace (string name)
		{
		}

		public int OpenScope (int startOffset)
		{
			if (current_method == null)
				return 0;

			ISourceBlock block = new SourceBlock (current_method, startOffset);
			current_method.StartBlock (block);

			return block.ID;
		}

		public void CloseScope (int endOffset) {
			if (current_method == null)
				return;

			current_method.EndBlock (endOffset);
		}

		public void SetScopeRange (int scopeID, int startOffset, int endOffset)
		{
			if (current_method == null)
				return;

			current_method.SetBlockRange (scopeID, startOffset, endOffset);
		}

		public void SetSymAttribute (SymbolToken parent, string name, byte[] data)
		{
		}

		public void SetUnderlyingWriter (IntPtr underlyingWriter)
		{
			throw new NotSupportedException ();
		}

		public void SetUserEntryPoint (SymbolToken entryMethod)
		{
		}

		public void UsingNamespace (string fullName)
		{
		}

		//
		// MonoSymbolWriter implementation
		//
		protected void WriteLocal (DwarfFileWriter.Die parent_die, ILocalVariable local)
		{
			DwarfFileWriter.DieMethodVariable die;

			die = new DwarfFileWriter.DieMethodVariable (parent_die, local);
		}

		protected void WriteBlock (DwarfFileWriter.Die parent_die, ISourceBlock block)
		{
			DwarfFileWriter.DieLexicalBlock die;

			die = new DwarfFileWriter.DieLexicalBlock (parent_die, block);

			foreach (ILocalVariable local in block.Locals)
				WriteLocal (die, local);

			foreach (ISourceBlock subblock in block.Blocks)
				WriteBlock (die, subblock);
		}

		protected void WriteMethod (DwarfFileWriter.DieCompileUnit parent_die, ISourceMethod method)
		{
			DwarfFileWriter.DieSubProgram die;

			die = new DwarfFileWriter.DieSubProgram (parent_die, method);

			foreach (ILocalVariable local in method.Locals)
				WriteLocal (die, local);

			foreach (ISourceBlock block in method.Blocks)
				WriteBlock (die, block);
		}

		protected void WriteSource (DwarfFileWriter writer, ISourceFile source)
		{
			DwarfFileWriter.CompileUnit compile_unit = new DwarfFileWriter.CompileUnit (
				writer, source.FileName);

			DwarfFileWriter.DieCompileUnit die = new DwarfFileWriter.DieCompileUnit (compile_unit);

			foreach (ISourceMethod method in source.Methods)
				WriteMethod (die, method);
		}


		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static Type get_local_type_from_sig (Assembly assembly, byte[] sig);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static MethodBuilder get_method (ModuleBuilder mb, int token);

		protected void DoFixups (Assembly assembly)
		{
			foreach (SourceMethod method in methods) {
				if (method._method_base is MethodBuilder) {
					MethodBuilder mb = (MethodBuilder) method._method_base;
					method._token = mb.GetToken ().Token;
				} else if (method._method_base is ConstructorBuilder) {
					ConstructorBuilder cb = (ConstructorBuilder) method._method_base;
					method._token = cb.GetToken ().Token;
				} else
					throw new NotSupportedException ();

				if (method.SourceFile == null)
					orphant_methods.Add (method);
			}

			foreach (LocalVariable local in locals) {
				byte[] signature = local.Signature;

				Type type = get_local_type_from_sig (assembly, signature);

				((LocalVariable) local)._type = type;
			}
		}

		protected void CreateDwarfFile (string filename)
		{
			DwarfFileWriter writer = new DwarfFileWriter (filename);

			foreach (ISourceFile source in sources.Values)
				WriteSource (writer, source);

			if (orphant_methods.Count > 0) {
				SourceFile source = new SourceFile ("<unknown>");

				foreach (SourceMethod orphant in orphant_methods) {
					orphant._source_file = source;
					source.AddMethod (orphant);
				}

				WriteSource (writer, source);
			}

			writer.Close ();
		}
	}
}

