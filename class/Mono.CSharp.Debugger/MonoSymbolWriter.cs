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
	internal class SourceFile
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

		public string FileName {
			get {
				return _file_name;
			}
		}

		public SourceMethod[] Methods {
			get {
				SourceMethod[] retval = new SourceMethod [_methods.Count];
				_methods.CopyTo (retval);
				return retval;
			}
		}

		public void AddMethod (SourceMethod method)
		{
			_methods.Add (method);
		}
	}

	internal class SourceBlock
	{
		static private int next_index;
		private readonly int _index;

		public SourceBlock (SourceMethod method, SourceLine start, SourceLine end)
		{
			this._method = method;
			this._start = start;
			this._end = end;
			this._index = ++next_index;
		}

		internal SourceBlock (SourceMethod method, int startOffset)
		{
			this._method = method;
			this._start = new SourceLine (startOffset);
			this._index = ++next_index;
		}

		public override string ToString ()
		{
			return "SourceBlock #" + ID + " (" + Start + " - " + End + ")";
		}

		private readonly SourceMethod _method;
		private ArrayList _blocks = new ArrayList ();
		internal SourceLine _start;
		internal SourceLine _end;

		private ArrayList _locals = new ArrayList ();

		public SourceMethod SourceMethod {
			get {
				return _method;
			}
		}

		public SourceBlock[] Blocks {
			get {
				SourceBlock[] retval = new SourceBlock [_blocks.Count];
				_blocks.CopyTo (retval);
				return retval;
			}
		}

		public void AddBlock (SourceBlock block)
		{
			_blocks.Add (block);
		}

		public SourceLine Start {
			get {
				return _start;
			}
		}

		public SourceLine End {
			get {
				return _end;
			}
		}

		public int ID {
			get {
				return _index;
			}
		}

		public LocalVariableEntry[] Locals {
			get {
				LocalVariableEntry[] retval = new LocalVariableEntry [_locals.Count];
				_locals.CopyTo (retval);
				return retval;
			}
		}

		public void AddLocal (LocalVariableEntry local)
		{
			_locals.Add (local);
		}
	}

	internal class SourceLine
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

		// interface SourceLine

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

	internal class SourceMethod
	{
		private ArrayList _lines = new ArrayList ();
		private ArrayList _blocks = new ArrayList ();
		private Hashtable _block_hash = new Hashtable ();
		private Stack _block_stack = new Stack ();

		internal readonly MethodBase _method_base;
		internal SourceFile _source_file;
		internal int _token;

		private SourceBlock _implicit_block;

		public SourceMethod (MethodBase method_base, SourceFile source_file)
			: this (method_base)
		{
			this._source_file = source_file;
		}

		internal SourceMethod (MethodBase method_base)
		{
			this._method_base = method_base;

			this._implicit_block = new SourceBlock (this, 0);
		}

		public void SetSourceRange (SourceFile sourceFile,
					    int startLine, int startColumn,
					    int endLine, int endColumn)
		{
			_source_file = sourceFile;
			_implicit_block._start = new SourceLine (startLine, startColumn);
			_implicit_block._end = new SourceLine (endLine, endColumn);
		}


		public void StartBlock (SourceBlock block)
		{
			_block_stack.Push (block);
		}

		public void EndBlock (int endOffset) {
			SourceBlock block = (SourceBlock) _block_stack.Pop ();

			block._end = new SourceLine (endOffset);

			if (_block_stack.Count > 0) {
				SourceBlock parent = (SourceBlock) _block_stack.Peek ();

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

		public SourceBlock CurrentBlock {
			get {
				if (_block_stack.Count > 0)
					return (SourceBlock) _block_stack.Peek ();
				else
					return _implicit_block;
			}
		}

		public SourceLine[] Lines {
			get {
				SourceLine[] retval = new SourceLine [_lines.Count];
				_lines.CopyTo (retval);
				return retval;
			}
		}

		public void AddLine (SourceLine line)
		{
			_lines.Add (line);
		}

		public SourceBlock[] Blocks {
			get {
				SourceBlock[] retval = new SourceBlock [_blocks.Count];
				_blocks.CopyTo (retval);
				return retval;
			}
		}

		public LocalVariableEntry[] Locals {
			get {
				return _implicit_block.Locals;
			}
		}

		public void AddLocal (LocalVariableEntry local)
		{
			_implicit_block.AddLocal (local);
		}

		public MethodBase MethodBase {
			get {
				return _method_base;
			}
		}

		public string FullName {
			get {
				return _method_base.DeclaringType.FullName + "." + _method_base.Name;
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

		public ParameterInfo[] Parameters {
			get {
				if (_method_base == null)
					return new ParameterInfo [0];

				ParameterInfo [] retval = _method_base.GetParameters ();
				if (retval == null)
					return new ParameterInfo [0];
				else
					return retval;
			}
		}

		public SourceFile SourceFile {
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

		public SourceLine Start {
			get {
				return _implicit_block.Start;
			}
		}

		public SourceLine End {
			get {
				return _implicit_block.End;
			}
		}
	}

	public class MonoSymbolWriter : IMonoSymbolWriter
	{
		protected Assembly assembly;
		protected ModuleBuilder module_builder;
		protected ArrayList locals = null;
		protected ArrayList orphant_methods = null;
		protected ArrayList methods = null;
		protected Hashtable sources = null;
		private ArrayList mbuilder_array = null;

		internal SourceMethod[] Methods {
			get {
				SourceMethod[] retval = new SourceMethod [methods.Count];
				methods.CopyTo (retval);
				return retval;
			}
		}

		internal SourceFile[] Sources {
			get {
				SourceFile[] retval = new SourceFile [sources.Count];
				sources.Values.CopyTo (retval, 0);
				return retval;
			}
		}

		private SourceMethod current_method = null;
		private string assembly_filename = null;
		private string output_filename = null;

		//
		// Interface IMonoSymbolWriter
		//

		public MonoSymbolWriter (ModuleBuilder mb, string filename, ArrayList mbuilder_array)
		{
			this.assembly_filename = filename;
			this.module_builder = mb;
			this.methods = new ArrayList ();
			this.sources = new Hashtable ();
			this.orphant_methods = new ArrayList ();
			this.locals = new ArrayList ();
			this.mbuilder_array = mbuilder_array;
		}

		public void Close () {
			if (assembly == null)
				assembly = Assembly.LoadFrom (assembly_filename);

			DoFixups (assembly);

			CreateOutput (assembly);
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
			throw new NotSupportedException ();
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
			throw new NotSupportedException ();
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

			current_method.AddLocal (new LocalVariableEntry (name, attributes, signature));
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
			if (current_method == null)
				return;

			SourceLine source_line = new SourceLine (offsets [0], lines [0], columns [0]);

			if (current_method != null)
				current_method.AddLine (source_line);
		}

		public void Initialize (IntPtr emitter, string filename, bool fFullBuild)
		{
			throw new NotSupportedException ();
		}

		public void Initialize (string assembly_filename, string filename, string[] args)
		{
			this.output_filename = filename;
			this.assembly_filename = assembly_filename;
		}

		public void OpenMethod (SymbolToken symbol_token)
		{
			int token = symbol_token.GetToken ();

			if ((token & 0xff000000) != 0x06000000)
				throw new ArgumentException ();

			int index = (token & 0xffffff) - 1;

			MethodBase mb = (MethodBase) mbuilder_array [index];

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
			throw new NotSupportedException ();
		}

		public int OpenScope (int startOffset)
		{
			if (current_method == null)
				return 0;

			SourceBlock block = new SourceBlock (current_method, startOffset);
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
			throw new NotSupportedException ();
		}

		public void SetUnderlyingWriter (IntPtr underlyingWriter)
		{
			throw new NotSupportedException ();
		}

		public void SetUserEntryPoint (SymbolToken entryMethod)
		{
			throw new NotSupportedException ();
		}

		public void UsingNamespace (string fullName)
		{
			throw new NotSupportedException ();
		}

		//
		// MonoSymbolWriter implementation
		//
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
		}

		protected void CreateOutput (Assembly assembly)
		{
			using (MonoSymbolTableWriter writer = new MonoSymbolTableWriter (output_filename))
				writer.WriteSymbolTable (this);
		}
	}
}

