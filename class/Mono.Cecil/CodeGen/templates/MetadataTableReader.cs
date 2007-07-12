//
// MetadataTableReader.cs
//
// Author:
//   Jb Evain (jbevain@gmail.com)
//
// Generated by /CodeGen/cecil-gen.rb do not edit
// <%=Time.now%>
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

	using System;
	using System.Collections;
	using System.IO;

	class MetadataTableReader : BaseMetadataTableVisitor {

		MetadataRoot m_metadataRoot;
		TablesHeap m_heap;
		MetadataRowReader m_mrrv;
		BinaryReader m_binaryReader;

		int [] m_rows = new int [TablesHeap.MaxTableCount];

		public MetadataTableReader (MetadataReader mrv)
		{
			m_metadataRoot = mrv.GetMetadataRoot ();
			m_heap = m_metadataRoot.Streams.TablesHeap;
			m_binaryReader = new BinaryReader (new MemoryStream (m_heap.Data));
			m_binaryReader.BaseStream.Position = 24;
			m_mrrv = new MetadataRowReader (this);
		}

		public MetadataRoot GetMetadataRoot ()
		{
			return m_metadataRoot;
		}

		public BinaryReader GetReader ()
		{
			return m_binaryReader;
		}

		public override IMetadataRowVisitor GetRowVisitor ()
		{
			return m_mrrv;
		}

		public int GetNumberOfRows (int rid)
		{
			return m_rows [rid];
		}
<% $tables.each { |table|  %>
		public <%=table.table_name%> Get<%=table.table_name%> ()
		{
			return (<%=table.table_name%>) m_heap [<%=table.table_name%>.RId];
		}
<% } %>
		public override void VisitTableCollection (TableCollection coll)
		{
<% $stables.each { |table|  %>			if (m_heap.HasTable (<%=table.table_name%>.RId)) {
				coll.Add (new <%=table.table_name%> ());
				m_rows [<%=table.table_name%>.RId] = m_binaryReader.ReadInt32 ();
			}
<% } %>		}

<% $tables.each { |table| %>		public override void Visit<%=table.table_name%> (<%=table.table_name%> table)
		{
			int number = m_rows [<%=table.table_name%>.RId];
			table.Rows = new RowCollection (number);
			for (int i = 0; i < number; i++)
				table.Rows.Add (new <%=table.row_name%> ());
		}
<% } %>	}
}
