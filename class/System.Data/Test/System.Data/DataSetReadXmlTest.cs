//
// DataSetReadXmlTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (C)2004 Novell Inc.
//

using System;
using System.IO;
using System.Data;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Data
{
	[TestFixture]
	public class DataSetReadXmlTest : Assertion
	{

		const string xml1 = "";
		const string xml2 = "<root/>";
		const string xml3 = "<root></root>";
		const string xml4 = "<root>   </root>";
		const string xml5 = "<root>test</root>";
		const string xml6 = "<root><test>1</test></root>";
		const string xml7 = "<root><test>1</test><test2>a</test2></root>";
		const string xml8 = "<dataset><table><col1>foo</col1><col2>bar</col2></table></dataset>";

		const string diff1 = @"<diffgr:diffgram xmlns:msdata='urn:schemas-microsoft-com:xml-msdata' xmlns:diffgr='urn:schemas-microsoft-com:xml-diffgram-v1'>
  <NewDataSet>
    <Table1 diffgr:id='Table11' msdata:rowOrder='0' diffgr:hasChanges='inserted'>
      <Column1_1>ppp</Column1_1>
      <Column1_2>www</Column1_2>
      <Column1_3>xxx</Column1_3>
    </Table1>
  </NewDataSet>
</diffgr:diffgram>";
		const string diff2 = diff1 + xml8;

		const string schema1 = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
	<xs:element name='Root'>
		<xs:complexType>
			<xs:sequence>
				<xs:element name='Child' type='xs:string' />
			</xs:sequence>
		</xs:complexType>
	</xs:element>
</xs:schema>";
		const string schema2 = schema1 + xml8;

		private void AssertDataSet (string label, DataSet ds, string name, int tableCount)
		{
			AssertEquals (label + ".DataSetName", name, ds.DataSetName);
			AssertEquals (label + ".TableCount", tableCount, ds.Tables.Count);
		}

		private void AssertDataTable (DataTable dt, string name, int columnCount, int rowCount)
		{
			AssertEquals ("TableName", name, dt.TableName);
			AssertEquals ("ColumnCount", columnCount, dt.Columns.Count);
			AssertEquals ("RowCount", rowCount, dt.Rows.Count);
		}

		private void AssertReadXml (DataSet ds, string label, string xml, XmlReadMode readMode, XmlReadMode resultMode, string datasetName, int tableCount)
		{
			AssertReadXml (ds, label, xml, readMode, resultMode, datasetName, tableCount, ReadState.EndOfFile);
		}

		// a bit detailed version
		private void AssertReadXml (DataSet ds, string label, string xml, XmlReadMode readMode, XmlReadMode resultMode, string datasetName, int tableCount, ReadState state)
		{
			XmlReader xtr = new XmlTextReader (xml, XmlNodeType.Element, null);
			AssertEquals (label + ".return", resultMode, ds.ReadXml (xtr, readMode));
			AssertDataSet (label + ".dataset", ds, datasetName, tableCount);
			AssertEquals (label + ".readstate", state, xtr.ReadState);
		}

		[Test]
		public void ReadSimpleAuto ()
		{
			DataSet ds = new DataSet ();

			// empty XML
			AssertReadXml (ds, "EmptyString", xml1,
				XmlReadMode.Auto, XmlReadMode.Auto,
				"NewDataSet", 0);

			// simple element
			AssertReadXml (ds, "EmptyElement", xml2,
				XmlReadMode.Auto, XmlReadMode.InferSchema,
				"NewDataSet", 0);

			// simple element2
			AssertReadXml (ds, "StartEndTag", xml3,
				XmlReadMode.Auto, XmlReadMode.InferSchema,
				"NewDataSet", 0);

			// whitespace in simple element
			AssertReadXml (ds, "Whitespace", xml4,
				XmlReadMode.Auto, XmlReadMode.InferSchema,
				"NewDataSet", 0);

			// text in simple element
			AssertReadXml (ds, "SingleText", xml5,
				XmlReadMode.Auto, XmlReadMode.InferSchema,
				"NewDataSet", 0);

			// simple table pattern:
			// root becomes a table and test becomes a column.
			AssertReadXml (ds, "SimpleTable", xml6,
				XmlReadMode.Auto, XmlReadMode.InferSchema,
				"NewDataSet", 1);
			AssertDataTable (ds.Tables [0], "root", 1, 1);

			// simple table with 2 columns:
			AssertReadXml (ds, "SimpleTable2", xml7,
				XmlReadMode.Auto, XmlReadMode.InferSchema,
				"NewDataSet", 1);
			AssertDataTable (ds.Tables [0], "root", 2, 1);

			// simple dataset with 1 table:
			AssertReadXml (ds, "SimpleDataSet", xml8,
				XmlReadMode.Auto, XmlReadMode.InferSchema,
				"dataset", 1);
			AssertDataTable (ds.Tables [0], "table", 2, 1);
		}

		[Test]
		public void ReadSimpleDiffgram ()
		{
			DataSet ds = new DataSet ();

			// empty XML
			AssertReadXml (ds, "EmptyString", xml1,
				XmlReadMode.DiffGram, XmlReadMode.DiffGram,
				"NewDataSet", 0);

			// simple element
			AssertReadXml (ds, "EmptyElement", xml2,
				XmlReadMode.DiffGram, XmlReadMode.DiffGram,
				"NewDataSet", 0);

			// simple element2
			AssertReadXml (ds, "StartEndTag", xml3,
				XmlReadMode.DiffGram, XmlReadMode.DiffGram,
				"NewDataSet", 0);

			// whitespace in simple element
			AssertReadXml (ds, "Whitespace", xml4,
				XmlReadMode.DiffGram, XmlReadMode.DiffGram,
				"NewDataSet", 0);

			// text in simple element
			AssertReadXml (ds, "SingleText", xml5,
				XmlReadMode.DiffGram, XmlReadMode.DiffGram,
				"NewDataSet", 0);

			// simple table pattern:
			AssertReadXml (ds, "SimpleTable", xml6,
				XmlReadMode.DiffGram, XmlReadMode.DiffGram,
				"NewDataSet", 0);

			// simple table with 2 columns:
			AssertReadXml (ds, "SimpleTable2", xml7,
				XmlReadMode.DiffGram, XmlReadMode.DiffGram,
				"NewDataSet", 0);

			// simple dataset with 1 table:
			AssertReadXml (ds, "SimpleDataSet", xml8,
				XmlReadMode.DiffGram, XmlReadMode.DiffGram,
				"NewDataSet", 0);
		}

		[Test]
		public void ReadSimpleFragment ()
		{
			DataSet ds = new DataSet ();

			// empty XML
			AssertReadXml (ds, "EmptyString", xml1,
				XmlReadMode.Fragment, XmlReadMode.Fragment,
				"NewDataSet", 0);

			// simple element
			AssertReadXml (ds, "EmptyElement", xml2,
				XmlReadMode.Fragment, XmlReadMode.Fragment,
				"NewDataSet", 0);

			// simple element2
			AssertReadXml (ds, "StartEndTag", xml3,
				XmlReadMode.Fragment, XmlReadMode.Fragment,
				"NewDataSet", 0);

			// whitespace in simple element
			AssertReadXml (ds, "Whitespace", xml4,
				XmlReadMode.Fragment, XmlReadMode.Fragment,
				"NewDataSet", 0);

			// text in simple element
			AssertReadXml (ds, "SingleText", xml5,
				XmlReadMode.Fragment, XmlReadMode.Fragment,
				"NewDataSet", 0);

			// simple table pattern:
			AssertReadXml (ds, "SimpleTable", xml6,
				XmlReadMode.Fragment, XmlReadMode.Fragment,
				"NewDataSet", 0);

			// simple table with 2 columns:
			AssertReadXml (ds, "SimpleTable2", xml7,
				XmlReadMode.Fragment, XmlReadMode.Fragment,
				"NewDataSet", 0);

			// simple dataset with 1 table:
			AssertReadXml (ds, "SimpleDataSet", xml8,
				XmlReadMode.Fragment, XmlReadMode.Fragment,
				"NewDataSet", 0);
		}

		[Test]
		public void ReadSimpleIgnoreSchema ()
		{
			DataSet ds = new DataSet ();

			// empty XML
			AssertReadXml (ds, "EmptyString", xml1,
				XmlReadMode.IgnoreSchema, XmlReadMode.IgnoreSchema,
				"NewDataSet", 0);

			// simple element
			AssertReadXml (ds, "EmptyElement", xml2,
				XmlReadMode.IgnoreSchema, XmlReadMode.IgnoreSchema,
				"NewDataSet", 0);

			// simple element2
			AssertReadXml (ds, "StartEndTag", xml3,
				XmlReadMode.IgnoreSchema, XmlReadMode.IgnoreSchema,
				"NewDataSet", 0);

			// whitespace in simple element
			AssertReadXml (ds, "Whitespace", xml4,
				XmlReadMode.IgnoreSchema, XmlReadMode.IgnoreSchema,
				"NewDataSet", 0);

			// text in simple element
			AssertReadXml (ds, "SingleText", xml5,
				XmlReadMode.IgnoreSchema, XmlReadMode.IgnoreSchema,
				"NewDataSet", 0);

			// simple table pattern:
			AssertReadXml (ds, "SimpleTable", xml6,
				XmlReadMode.IgnoreSchema, XmlReadMode.IgnoreSchema,
				"NewDataSet", 0);

			// simple table with 2 columns:
			AssertReadXml (ds, "SimpleTable2", xml7,
				XmlReadMode.IgnoreSchema, XmlReadMode.IgnoreSchema,
				"NewDataSet", 0);

			// simple dataset with 1 table:
			AssertReadXml (ds, "SimpleDataSet", xml8,
				XmlReadMode.IgnoreSchema, XmlReadMode.IgnoreSchema,
				"NewDataSet", 0);
		}

		[Test]
		public void ReadSimpleInferSchema ()
		{
			DataSet ds = new DataSet ();

			// empty XML
			AssertReadXml (ds, "EmptyString", xml1,
				XmlReadMode.InferSchema, XmlReadMode.InferSchema,
				"NewDataSet", 0);

			// simple element
			AssertReadXml (ds, "EmptyElement", xml2,
				XmlReadMode.InferSchema, XmlReadMode.InferSchema,
				"NewDataSet", 0);

			// simple element2
			AssertReadXml (ds, "StartEndTag", xml3,
				XmlReadMode.InferSchema, XmlReadMode.InferSchema,
				"NewDataSet", 0);

			// whitespace in simple element
			AssertReadXml (ds, "Whitespace", xml4,
				XmlReadMode.InferSchema, XmlReadMode.InferSchema,
				"NewDataSet", 0);

			// text in simple element
			AssertReadXml (ds, "SingleText", xml5,
				XmlReadMode.InferSchema, XmlReadMode.InferSchema,
				"NewDataSet", 0);

			// simple table pattern:
			// root becomes a table and test becomes a column.
			AssertReadXml (ds, "SimpleTable", xml6,
				XmlReadMode.InferSchema, XmlReadMode.InferSchema,
				"NewDataSet", 1);
			AssertDataTable (ds.Tables [0], "root", 1, 1);

			// simple table with 2 columns:
			AssertReadXml (ds, "SimpleTable2", xml7,
				XmlReadMode.InferSchema, XmlReadMode.InferSchema,
				"NewDataSet", 1);
			AssertDataTable (ds.Tables [0], "root", 2, 1);

			// simple dataset with 1 table:
			AssertReadXml (ds, "SimpleDataSet", xml8,
				XmlReadMode.InferSchema, XmlReadMode.InferSchema,
				"dataset", 1);
			AssertDataTable (ds.Tables [0], "table", 2, 1);
		}

		[Test]
		public void ReadSimpleReadSchema ()
		{
			DataSet ds = new DataSet ();

			// empty XML
			AssertReadXml (ds, "EmptyString", xml1,
				XmlReadMode.ReadSchema, XmlReadMode.ReadSchema,
				"NewDataSet", 0);

			// simple element
			AssertReadXml (ds, "EmptyElement", xml2,
				XmlReadMode.ReadSchema, XmlReadMode.ReadSchema,
				"NewDataSet", 0);

			// simple element2
			AssertReadXml (ds, "StartEndTag", xml3,
				XmlReadMode.ReadSchema, XmlReadMode.ReadSchema,
				"NewDataSet", 0);

			// whitespace in simple element
			AssertReadXml (ds, "Whitespace", xml4,
				XmlReadMode.ReadSchema, XmlReadMode.ReadSchema,
				"NewDataSet", 0);

			// text in simple element
			AssertReadXml (ds, "SingleText", xml5,
				XmlReadMode.ReadSchema, XmlReadMode.ReadSchema,
				"NewDataSet", 0);

			// simple table pattern:
			AssertReadXml (ds, "SimpleTable", xml6,
				XmlReadMode.ReadSchema, XmlReadMode.ReadSchema,
				"NewDataSet", 0);

			// simple table with 2 columns:
			AssertReadXml (ds, "SimpleTable2", xml7,
				XmlReadMode.ReadSchema, XmlReadMode.ReadSchema,
				"NewDataSet", 0);

			// simple dataset with 1 table:
			AssertReadXml (ds, "SimpleDataSet", xml8,
				XmlReadMode.ReadSchema, XmlReadMode.ReadSchema,
				"NewDataSet", 0);
		}

		[Test]
		public void TestSimpleDiffXmlAll ()
		{
			DataSet ds = new DataSet ();

			// ignored
			AssertReadXml (ds, "Fragment", diff1,
				XmlReadMode.Fragment, XmlReadMode.Fragment,
				"NewDataSet", 0);

			AssertReadXml (ds, "IgnoreSchema", diff1,
				XmlReadMode.IgnoreSchema, XmlReadMode.IgnoreSchema,
				"NewDataSet", 0);

			AssertReadXml (ds, "InferSchema", diff1,
				XmlReadMode.InferSchema, XmlReadMode.InferSchema,
				"NewDataSet", 0);

			AssertReadXml (ds, "ReadSchema", diff1,
				XmlReadMode.ReadSchema, XmlReadMode.ReadSchema,
				"NewDataSet", 0);

			// Auto, DiffGram ... treated as DiffGram
			AssertReadXml (ds, "Auto", diff1,
				XmlReadMode.Auto, XmlReadMode.DiffGram,
				"NewDataSet", 0);
			AssertReadXml (ds, "DiffGram", diff1,
				XmlReadMode.DiffGram, XmlReadMode.DiffGram,
				"NewDataSet", 0);
		}

		[Test]
		public void TestSimpleDiffPlusContentAll ()
		{
			DataSet ds = new DataSet ();

			// Fragment ... skipped
			AssertReadXml (ds, "Fragment", diff2,
				XmlReadMode.Fragment, XmlReadMode.Fragment,
				"NewDataSet", 0);

			// others ... kept 
			AssertReadXml (ds, "IgnoreSchema", diff2,
				XmlReadMode.IgnoreSchema, XmlReadMode.IgnoreSchema,
				"NewDataSet", 0, ReadState.Interactive);

			AssertReadXml (ds, "InferSchema", diff2,
				XmlReadMode.InferSchema, XmlReadMode.InferSchema,
				"NewDataSet", 0, ReadState.Interactive);

			AssertReadXml (ds, "ReadSchema", diff2,
				XmlReadMode.ReadSchema, XmlReadMode.ReadSchema,
				"NewDataSet", 0, ReadState.Interactive);

			// Auto, DiffGram ... treated as DiffGram
			AssertReadXml (ds, "Auto", diff2,
				XmlReadMode.Auto, XmlReadMode.DiffGram,
				"NewDataSet", 0, ReadState.Interactive);
			AssertReadXml (ds, "DiffGram", diff2,
				XmlReadMode.DiffGram, XmlReadMode.DiffGram,
				"NewDataSet", 0, ReadState.Interactive);
		}

		[Test]
		public void TestSimpleSchemaXmlAll ()
		{
			DataSet ds = new DataSet ();

			// ignored
			AssertReadXml (ds, "IgnoreSchema", schema1,
				XmlReadMode.IgnoreSchema, XmlReadMode.IgnoreSchema,
				"NewDataSet", 0);

			AssertReadXml (ds, "InferSchema", schema1,
				XmlReadMode.InferSchema, XmlReadMode.InferSchema,
				"NewDataSet", 0);

			// misc ... consume schema
			AssertReadXml (ds, "Fragment", schema1,
				XmlReadMode.Fragment, XmlReadMode.Fragment,
				"NewDataSet", 1);
			AssertDataTable (ds.Tables [0], "Root", 1, 0);

			AssertReadXml (ds, "ReadSchema", schema1,
				XmlReadMode.ReadSchema, XmlReadMode.ReadSchema,
				"NewDataSet", 1);
			AssertDataTable (ds.Tables [0], "Root", 1, 0);

			AssertReadXml (ds, "Auto", schema1,
				XmlReadMode.Auto, XmlReadMode.ReadSchema,
				"NewDataSet", 1);
			AssertDataTable (ds.Tables [0], "Root", 1, 0);

			AssertReadXml (ds, "DiffGram", schema1,
				XmlReadMode.DiffGram, XmlReadMode.DiffGram,
				"NewDataSet", 1);
		}

		[Test]
		public void TestSimpleSchemaPlusContentAll ()
		{
			DataSet ds = new DataSet ();

			// ignored
			AssertReadXml (ds, "IgnoreSchema", schema2,
				XmlReadMode.IgnoreSchema, XmlReadMode.IgnoreSchema,
				"NewDataSet", 0, ReadState.Interactive);

			AssertReadXml (ds, "InferSchema", schema2,
				XmlReadMode.InferSchema, XmlReadMode.InferSchema,
				"NewDataSet", 0, ReadState.Interactive);

			// Fragment ... consumed both
			AssertReadXml (ds, "Fragment", schema2,
				XmlReadMode.Fragment, XmlReadMode.Fragment,
				"NewDataSet", 1);
			AssertDataTable (ds.Tables [0], "Root", 1, 0);

			// rest ... treated as schema
			AssertReadXml (ds, "Auto", schema2,
				XmlReadMode.Auto, XmlReadMode.ReadSchema,
				"NewDataSet", 1, ReadState.Interactive);
			AssertDataTable (ds.Tables [0], "Root", 1, 0);

			AssertReadXml (ds, "DiffGram", schema2,
				XmlReadMode.DiffGram, XmlReadMode.DiffGram,
				"NewDataSet", 1, ReadState.Interactive);
			AssertDataTable (ds.Tables [0], "Root", 1, 0);

			AssertReadXml (ds, "ReadSchema", schema2,
				XmlReadMode.ReadSchema, XmlReadMode.ReadSchema,
				"NewDataSet", 1, ReadState.Interactive);
		}

		/* To be added
		[Test]
		public void SaveDiffLoadAutoSaveSchema ()
		{
			DataSet ds = new DataSet ();
			ds.Tables.Add ("Table1");
			ds.Tables.Add ("Table2");
			ds.Tables [0].Columns.Add ("Column1_1");
			ds.Tables [0].Columns.Add ("Column1_2");
			ds.Tables [0].Columns.Add ("Column1_3");
			ds.Tables [1].Columns.Add ("Column2_1");
			ds.Tables [1].Columns.Add ("Column2_2");
			ds.Tables [1].Columns.Add ("Column2_3");
			ds.Tables [0].Rows.Add (new object [] {"ppp", "www", "xxx"});

			// save as diffgram
			StringWriter sw = new StringWriter ();
			ds.WriteXml (sw, XmlWriteMode.DiffGram);
			string xml = sw.ToString ();
			string result = new StreamReader ("Test/System.Data/DataSetReadXmlTest1.xml", Encoding.ASCII).ReadToEnd ();
			AssertEquals ("#01", result, xml);

			// load diffgram above
			ds.ReadXml (new StringReader (sw.ToString ()));
			sw = new StringWriter ();
			ds.WriteXml (sw, XmlWriteMode.WriteSchema);
			xml = sw.ToString ();
			result = new StreamReader ("Test/System.Data/DataSetReadXmlTest2.xml", Encoding.ASCII).ReadToEnd ();
			AssertEquals ("#02", result, xml);
		}
		*/
	}
}
