//
// ModuleTable.cs
//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Generated by /CodeGen/cecil-gen.rb do not edit
// Tue Oct 10 23:08:26 CEST 2006
//
// (C) 2005 Jb Evain
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

namespace Mono.Cecil.Metadata {

	public sealed class ModuleTable : IMetadataTable {

		public const int RId = 0x00;

		RowCollection m_rows;

		public ModuleRow this [int index] {
			get { return m_rows [index] as ModuleRow; }
			set { m_rows [index] = value; }
		}

		public RowCollection Rows {
			get { return m_rows; }
			set { m_rows = value; }
		}

		public int Id {
			get { return RId; }
		}

		internal ModuleTable ()
		{
		}

		public void Accept (IMetadataTableVisitor visitor)
		{
			visitor.VisitModuleTable (this);
			this.Rows.Accept (visitor.GetRowVisitor ());
		}
	}

	public sealed class ModuleRow : IMetadataRow {

		public ushort Generation;
		public uint Name;
		public uint Mvid;
		public uint EncId;
		public uint EncBaseId;

		internal ModuleRow ()
		{
		}

		public void Accept (IMetadataRowVisitor visitor)
		{
			visitor.VisitModuleRow (this);
		}
	}
}
