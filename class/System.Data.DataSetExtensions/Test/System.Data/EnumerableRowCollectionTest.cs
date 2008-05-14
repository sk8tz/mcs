//
// EnumerableRowCollectionTest.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell, Inc. http://www.novell.com
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
using System.Collections.Generic;
using System.Data;
using NUnit.Framework;

namespace MonoTests.System.Data
{
	[TestFixture]
	public class EnumerableRowCollectionTest
	{
		[Test]
		public void QueryWhere ()
		{
			var ds = new DataSet ();
			ds.ReadXml ("Test/System.Data/testdataset1.xml");
			var table = ds.Tables [0];
			/* schema generated as ...
			var table = ds.Tables.Add ("ScoreList");
			table.Columns.Add ("ID", typeof (int));
			table.Columns.Add ("RegisteredDate", typeof (DateTime));
			table.Columns.Add ("Name", typeof (string));
			table.Columns.Add ("Score", typeof (int));
			ds.WriteXml ("Test/System.Data/testdataset1.xsd", XmlWriteMode.WriteSchema);
			*/
			var q = from line in table.AsEnumerable ()
				where line.Field<int> ("Score") > 80
				select line;
			bool iterated = false;
			foreach (var line in q) {
				if (iterated)
					Assert.Fail ("should match only one raw");
				Assert.AreEqual (100, line ["Score"], "#1");
				iterated = true;
			}
		}

		/* FIXME: enable it when it gets fixed: https://bugzilla.novell.com/show_bug.cgi?id=389795
		[Test]
		public void QueryWhereSelect ()
		{
			var ds = new DataSet ();
			ds.ReadXml ("Test/System.Data/testdataset1.xml");
			var table = ds.Tables [0];
			var q = from line in table.AsEnumerable ()
				where line.Field<int> ("Score") > 80
				select new {
					StudentID = line.Field<int> ("ID"),
					StudentName = line.Field<string> ("Name"),
					StudentScore = line.Field<int> ("Score") };
			bool iterated = false;
			foreach (var ql in q) {
				if (iterated)
					Assert.Fail ("should match only one raw");
				Assert.AreEqual (100, ql.StudentScore, "#1");
				iterated = true;
			}
		}

		[Test]
		public void QueryWhereSelectOrderBy ()
		{
			var ds = new DataSet ();
			ds.ReadXml ("Test/System.Data/testdataset1.xml");
			var table = ds.Tables [0];
			var q = from line in table.AsEnumerable ()
				where line.Field<int> ("Score") >= 80
				orderby line.Field<int> ("ID")
				select new {
					StudentID = line.Field<int> ("ID"),
					StudentName = line.Field<string> ("Name"),
					StudentScore = line.Field<int> ("Score") };
			int prevID = -1;
			foreach (var ql in q) {
				switch (prevID) {
				case -1:
					Assert.AreEqual (4, ql.StudentID, "#1");
					break;
				case 4:
					Assert.AreEqual (1, ql.StudentID, "#2");
					break;
				default:
					Assert.Fail ("should match only one raw");
				}
				prevID = ql.StudentID;
			}
		}

		[Test]
		public void QueryWhereSelectOrderByDescending ()
		{
			var ds = new DataSet ();
			ds.ReadXml ("Test/System.Data/testdataset1.xml");
			var table = ds.Tables [0];
			var q = from line in table.AsEnumerable ()
				where line.Field<int> ("Score") >= 80
				orderby line.Field<int> ("ID") descending
				select new {
					StudentID = line.Field<int> ("ID"),
					StudentName = line.Field<string> ("Name"),
					StudentScore = line.Field<int> ("Score") };
			int prevID = -1;
			foreach (var ql in q) {
				switch (prevID) {
				case -1:
					Assert.AreEqual (1, ql.StudentID, "#1");
					break;
				case 4:
					Assert.AreEqual (4, ql.StudentID, "#2");
					break;
				default:
					Assert.Fail ("should match only one raw");
				}
				prevID = ql.StudentID;
			}
		}

		[Test]
		public void ThenBy ()
		{
			var ds = new DataSet ();
			ds.ReadXml ("Test/System.Data/testdataset1.xml");
			var table = ds.Tables [0];
			var q = from line in table.AsEnumerable ()
				where line.Field<int> ("Score") >= 80
				orderby line.Field<bool> ("Gender"), line.Field<int> ("ID")
				select new {
					StudentID = line.Field<int> ("ID"),
					StudentName = line.Field<string> ("Name"),
					StudentScore = line.Field<int> ("Score") };
			int prevID = -1;
			foreach (var ql in q) {
				switch (prevID) {
				case -1:
					Assert.AreEqual (4, ql.StudentID, "#1");
					break;
				case 4:
					Assert.AreEqual (1, ql.StudentID, "#2");
					break;
				default:
					Assert.Fail ("should match only one raw");
				}
				prevID = ql.StudentID;
			}
		}

		[Test]
		public void ThenByDescending ()
		{
			var ds = new DataSet ();
			ds.ReadXml ("Test/System.Data/testdataset1.xml");
			var table = ds.Tables [0];
			var q = from line in table.AsEnumerable ()
				where line.Field<int> ("Score") >= 80
				orderby line.Field<bool> ("Gender"), line.Field<int> ("ID") descending
				select new {
					StudentID = line.Field<int> ("ID"),
					StudentName = line.Field<string> ("Name"),
					StudentScore = line.Field<int> ("Score") };
			int prevID = -1;
			foreach (var ql in q) {
				switch (prevID) {
				case -1:
					Assert.AreEqual (1, ql.StudentID, "#1");
					break;
				case 4:
					Assert.AreEqual (4, ql.StudentID, "#2");
					break;
				default:
					Assert.Fail ("should match only one raw");
				}
				prevID = ql.StudentID;
			}
		}
		*/
	}
}
