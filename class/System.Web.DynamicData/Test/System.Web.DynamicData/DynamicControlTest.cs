﻿//
// MetaModelTest.cs
//
// Authors:
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2009 Novell Inc. http://novell.com
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
using System.Web.DynamicData;
using System.Web.DynamicData.ModelProviders;
using System.Web.Routing;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.IO;

using NUnit.Framework;
using NUnit.Mocks;
using MonoTests.stand_alone.WebHarness;
using MonoTests.SystemWeb.Framework;
using MonoTests.Common;
using MonoTests.DataSource;
using MonoTests.DataObjects;

using MetaModel = System.Web.DynamicData.MetaModel;
using MetaTable = System.Web.DynamicData.MetaTable;

namespace MonoTests.System.Web.DynamicData
{
	[TestFixture]
	public class DynamicControlTest
	{
		sealed class FieldTemplateTestDescription
		{
			public string ColumnName { get; private set; }
			public string ControlVirtualPath { get; private set; }
			public bool IsNull { get; private set; }

			public FieldTemplateTestDescription (string columnName)
				: this (columnName, String.Empty, true)
			{ }

			public FieldTemplateTestDescription (string columnName, string virtualPath)
				: this (columnName, virtualPath, false)
			{ }

			public FieldTemplateTestDescription (string columnName, string virtualPath, bool isNull)
			{
				ColumnName = columnName;
				ControlVirtualPath = virtualPath;
				IsNull = isNull;
			}
		}

		[SetUp]
		public void PerTestSetUp ()
		{
			// This is ran before every test
			CleanUp_FullTypeNameTemplates ();
			CleanUp_ShortTypeNameTemplates ();
		}

		[TestFixtureSetUp]
		public void SetUp ()
		{
			Type type = GetType ();
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_01.aspx", "ListView_DynamicControl_01.aspx");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_01.aspx.cs", "ListView_DynamicControl_01.aspx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_02.aspx", "ListView_DynamicControl_02.aspx");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_02.aspx.cs", "ListView_DynamicControl_02.aspx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_03.aspx", "ListView_DynamicControl_03.aspx");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_03.aspx.cs", "ListView_DynamicControl_03.aspx.cs");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_04.aspx", "ListView_DynamicControl_04.aspx");
			WebTest.CopyResource (type, "MonoTests.WebPages.ListView_DynamicControl_04.aspx.cs", "ListView_DynamicControl_04.aspx.cs");
		}

		[Test]
		public void Defaults ()
		{
			var dc = new DynamicControl ();

			Assert.AreEqual (false, dc.ApplyFormatInEditMode, "#A1");
			Assert.AreEqual (null, dc.Column, "#A2");
			Assert.AreEqual (false, dc.ConvertEmptyStringToNull, "#A3");
			Assert.AreEqual (String.Empty, dc.CssClass, "#A4");
			Assert.AreEqual (String.Empty, dc.DataField, "#A5");
			Assert.AreEqual (String.Empty, dc.DataFormatString, "#A6");
			Assert.AreEqual (null, dc.FieldTemplate, "#A7");
			Assert.AreEqual (true, dc.HtmlEncode, "#A8");
			Assert.AreEqual (dc, ((IFieldTemplateHost) dc).FormattingOptions, "#A9");
			Assert.AreEqual (DataBoundControlMode.ReadOnly, dc.Mode, "#A10");
			Assert.AreEqual (String.Empty, dc.NullDisplayText, "#A11");
			// Throws NREX on .NET .... (why am I still surprised by this?)
			// Calls DynamicDataExtensions.FindMetaTable which is where the exception is thrown from
			// Assert.AreEqual (null, dc.Table, "#A12");
			Assert.AreEqual (String.Empty, dc.UIHint, "#A13");
			Assert.AreEqual (String.Empty, dc.ValidationGroup, "#A14");
		}

		[Test]
		public void ApplyFormatInEditMode ()
		{
			var test = new WebTest ("ListView_DynamicControl_01.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (ApplyFormatInEditMode_OnLoad);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void ApplyFormatInEditMode_OnLoad (Page p)
		{
			var lc = p.FindControl ("ListView1") as ListView;
			Assert.IsNotNull (lc, "#A1");

			var dc = lc.FindChild<DynamicControl> ("FirstName");
			Assert.IsNotNull (dc, "#B1-1");
			Assert.AreEqual ("FirstName", dc.ID, "#B1-2");
			Assert.AreEqual (false, dc.Column.ApplyFormatInEditMode, "#B1-3");
			Assert.AreEqual (false, dc.ApplyFormatInEditMode, "#B1-4");

			dc = lc.FindChild<DynamicControl> ("Active");
			Assert.IsNotNull (dc, "#C1");
			Assert.AreEqual ("Active", dc.ID, "#C1-1");
			Assert.AreEqual (true, dc.Column.ApplyFormatInEditMode, "#C1-2");
			Assert.AreEqual (true, dc.ApplyFormatInEditMode, "#C1-3");

			dc.ApplyFormatInEditMode = false;
			Assert.AreEqual (false, dc.ApplyFormatInEditMode, "#C1-4");
			Assert.AreEqual (true, dc.Column.ApplyFormatInEditMode, "#C1-5");
		}

		[Test]
		public void Column ()
		{
			var test = new WebTest ("ListView_DynamicControl_01.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (Column_OnLoad);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void Column_OnLoad (Page p)
		{
			var lc = p.FindControl ("ListView1") as ListView;
			Assert.IsNotNull (lc, "#A1");

			var dc = lc.FindChild<DynamicControl> ("FirstName");
			Assert.IsNotNull (dc, "#A1-1");
			Assert.IsNotNull (dc.Column, "#B1");

			// Safe not to check for GetModel's return value - it throws if model isn't found, same
			// goes for GetTable and GetColumn
			MetaTable table = MetaModel.GetModel (typeof (EmployeesDataContext)).GetTable ("EmployeeTable");
			MetaColumn column = table.GetColumn ("FirstName");
			Assert.AreEqual (column, dc.Column, "#B1-1");
			Assert.AreEqual (dc.Column.Table, dc.Table, "#B1-2");

			dc.Column = column;
			Assert.AreEqual (column, dc.Column, "#C1-3");

			column = table.GetColumn ("Active");
			dc.Column = column;
			Assert.AreEqual (column, dc.Column, "#C1-4");

			// Talk about consistency...
			table = MetaModel.GetModel (typeof (EmployeesDataContext)).GetTable ("SeasonalEmployeeTable");
			column = table.GetColumn ("FirstName");
			dc.Column = column;

			Assert.AreNotEqual (dc.Column.Table, dc.Table, "#C1-5");
		}

		[Test]
		public void ConvertEmptyStringToNull ()
		{
			var test = new WebTest ("ListView_DynamicControl_01.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (ConvertEmptyStringToNull_OnLoad);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void ConvertEmptyStringToNull_OnLoad (Page p)
		{
			var lc = p.FindControl ("ListView1") as ListView;
			Assert.IsNotNull (lc, "#A1");

			var dc = lc.FindChild<DynamicControl> ("FirstName");
			Assert.IsNotNull (dc, "#B1-1");
			Assert.AreEqual ("FirstName", dc.ID, "#B1-2");
			Assert.AreEqual (true, dc.Column.ConvertEmptyStringToNull, "#B1-3");
			Assert.AreEqual (true, dc.ConvertEmptyStringToNull, "#B1-4");

			dc = lc.FindChild<DynamicControl> ("LastName");
			Assert.IsNotNull (dc, "#C1");
			Assert.AreEqual (true, dc.ConvertEmptyStringToNull, "#C1-1");

			dc.ConvertEmptyStringToNull = false;
			Assert.AreEqual (false, dc.ConvertEmptyStringToNull, "#C1-2");
			Assert.AreEqual (true, dc.Column.ConvertEmptyStringToNull, "#C1-3");
		}

		[Test]
		public void CssClass ()
		{
			var dc = new DynamicControl ();
			dc.CssClass = "MyCssClass";
			Assert.AreEqual ("MyCssClass", dc.CssClass, "#A1");

			var test = new WebTest ("ListView_DynamicControl_01.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (CssClass_OnLoad);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");

			string html = @"<span class=""activeCssClass"">

<span class=""field"">Active</span>:";
			Assert.IsTrue (p.IndexOf (html) != -1, "#Y1");
		}

		static void CssClass_OnLoad (Page p)
		{
			var lc = p.FindControl ("ListView1") as ListView;
			Assert.IsNotNull (lc, "#B1");

			var dc = lc.FindChild<PokerDynamicControl> ("Active");
			Assert.IsNotNull (dc, "#C1");
			Assert.AreEqual ("Active", dc.DataField, "#C1-1");
			Assert.AreEqual ("activeCssClass", dc.CssClass, "#C1-2");
		}

		[Test]
		public void DataField ()
		{
			var dc = new DynamicControl ();

			Assert.AreEqual (String.Empty, dc.DataField, "#A1");
			dc.DataField = "MyField";
			Assert.AreEqual ("MyField", dc.DataField, "#A2");
		}

		[Test]
		public void DataField_1 ()
		{
			var test = new WebTest ("ListView_DynamicControl_01.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (DataField_OnLoad_1);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void DataField_OnLoad_1 (Page p)
		{
			var lc = p.FindControl ("ListView1") as ListView;
			Assert.IsNotNull (lc, "#A1");

			var dc = lc.FindChild<DynamicControl> ("FirstName");
			Assert.IsNotNull (dc, "#A1-1");
			Assert.IsNotNull (dc.DataField, "#A1-2");
			Assert.AreEqual("FirstName", dc.DataField, "#A1-3");

			// Column and Table aren't set on DataField assignment...
			dc.DataField = "Active";
			Assert.AreEqual ("Active", dc.DataField, "#B1");
			Assert.AreEqual ("FirstName", dc.Column.Name, "#B1-1");
		}

		[Test]
		public void DataFormatString ()
		{
			var test = new WebTest ("ListView_DynamicControl_01.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (DataFormatString_OnLoad);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void DataFormatString_OnLoad (Page p)
		{
			var lc = p.FindControl ("ListView1") as ListView;
			Assert.IsNotNull (lc, "#A1");

			var dc = lc.FindChild<DynamicControl> ("FirstName");
			Assert.IsNotNull (dc, "#B1-1");
			Assert.AreEqual ("FirstName", dc.ID, "#B1-2");
			Assert.AreEqual (String.Empty, dc.Column.DataFormatString, "#B1-3");
			Assert.AreEqual (String.Empty, dc.DataFormatString, "#B1-4");

			dc = lc.FindChild<DynamicControl> ("Active");
			Assert.IsNotNull (dc, "#C1");
			Assert.AreEqual ("Active", dc.ID, "#C1-1");
			Assert.AreEqual ("Boolean value: {0}", dc.Column.DataFormatString, "#C1-2");
			Assert.AreEqual ("Boolean value: {0}", dc.DataFormatString, "#C1-3");

			dc.DataFormatString = String.Empty;
			Assert.AreEqual (String.Empty, dc.DataFormatString, "#C1-4");
			Assert.AreEqual ("Boolean value: {0}", dc.Column.DataFormatString, "#C1-5");
		}

		[Test]
		public void FieldTemplate ()
		{
			var test = new WebTest ("ListView_DynamicControl_03.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (FieldTemplate_OnLoad);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static List <FieldTemplateTestDescription> fieldTemplateReadOnlyColumns = new List <FieldTemplateTestDescription> ()
		{
			new FieldTemplateTestDescription ("Char_Column", "~/DynamicData/FieldTemplates/Text.ascx"),
			new FieldTemplateTestDescription ("Byte_Column", "~/DynamicData/FieldTemplates/Text.ascx"),
			new FieldTemplateTestDescription ("Int_Column", "~/DynamicData/FieldTemplates/Text.ascx"),
			new FieldTemplateTestDescription ("Long_Column", "~/DynamicData/FieldTemplates/Text.ascx"),
			new FieldTemplateTestDescription ("Bool_Column", "~/DynamicData/FieldTemplates/Boolean.ascx"),
			new FieldTemplateTestDescription ("String_Column", "~/DynamicData/FieldTemplates/Text.ascx"),
			new FieldTemplateTestDescription ("Float_Column", "~/DynamicData/FieldTemplates/Text.ascx"),
			new FieldTemplateTestDescription ("Single_Column", "~/DynamicData/FieldTemplates/Text.ascx"),
			new FieldTemplateTestDescription ("Double_Column", "~/DynamicData/FieldTemplates/Text.ascx"),
			new FieldTemplateTestDescription ("Decimal_Column", "~/DynamicData/FieldTemplates/Text.ascx"),
			new FieldTemplateTestDescription ("SByte_Column"),
			new FieldTemplateTestDescription ("UInt_Column"),
			new FieldTemplateTestDescription ("ULong_Column"),
			new FieldTemplateTestDescription ("Short_Column", "~/DynamicData/FieldTemplates/Text.ascx"),
			new FieldTemplateTestDescription ("UShort_Column"),
			new FieldTemplateTestDescription ("DateTime_Column", "~/DynamicData/FieldTemplates/DateTime.ascx"),
			new FieldTemplateTestDescription ("FooEmpty_Column"),
			new FieldTemplateTestDescription ("Object_Column"),
			new FieldTemplateTestDescription ("ByteArray_Column"),
			new FieldTemplateTestDescription ("IntArray_Column"),
			new FieldTemplateTestDescription ("StringArray_Column"),
			new FieldTemplateTestDescription ("ObjectArray_Column"),
			new FieldTemplateTestDescription ("StringList_Column"),
			new FieldTemplateTestDescription ("Dictionary_Column"),
			new FieldTemplateTestDescription ("ICollection_Column"),
			new FieldTemplateTestDescription ("IEnumerable_Column"),
			new FieldTemplateTestDescription ("ICollectionByte_Column"),
			new FieldTemplateTestDescription ("IEnumerableByte_Column"),
			new FieldTemplateTestDescription ("ByteMultiArray_Column"),
			new FieldTemplateTestDescription ("BoolArray_Column"),
			new FieldTemplateTestDescription ("MaximumLength_Column4", "~/DynamicData/FieldTemplates/Text.ascx"),
		};

		static void FieldTemplate_OnLoad (Page p)
		{
			var lc = p.FindControl ("ListView3") as ListView;
			Assert.IsNotNull (lc, "#A1");

			int counter = 1;
			foreach (var entry in fieldTemplateReadOnlyColumns) { 
				string columnName = entry.ColumnName;
				var dc = lc.FindChild<PokerDynamicControl> (columnName);
				Assert.IsNotNull (dc, String.Format ("#B{0}-1 ({1}", counter, columnName));
				Assert.AreEqual (columnName, dc.ID, String.Format ("#B{0}-2 ({1})", counter, columnName));

				var templateControl = dc.FieldTemplate;
				var template = templateControl as FieldTemplateUserControl;
				if (entry.IsNull) {
					Assert.IsNull (templateControl, String.Format ("#B{0}-3 ({1})", counter, columnName));
					Assert.IsNull (template, String.Format ("#B{0}-4 ({1})", counter, columnName));
				} else {
					Assert.IsNotNull (templateControl, String.Format ("#B{0}-5 ({1})", counter, columnName));
					Assert.IsNotNull (template, String.Format ("#B{0}-6 ({1})", counter, columnName));
					Assert.AreEqual (entry.ControlVirtualPath, template.AppRelativeVirtualPath, String.Format ("#B{0}-7 ({1})", counter, columnName));
				}

				counter++;
			}
		}

		[Test]
		public void FieldTemplate_1 ()
		{
			var test = new WebTest ("ListView_DynamicControl_04.aspx");
			var p = test.Run ();

			// Fake post-back
			var delegates = new PageDelegates ();
			delegates.PreRenderComplete = FieldTemplate_OnPreRenderComplete_1;
			test.Invoker = new PageInvoker (delegates);
			var fr = new FormRequest (test.Response, "form1");
			fr.Controls.Add ("ListView4$ctrl0$editMe");
			fr.Controls["ListView4$ctrl0$editMe"].Value = "Edit";
			test.Request = fr;
			p = test.Run ();

			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static List<FieldTemplateTestDescription> fieldTemplateEditColumns = new List<FieldTemplateTestDescription> ()
		{
			new FieldTemplateTestDescription ("Char_Column", "~/DynamicData/FieldTemplates/Text_Edit.ascx"),
			new FieldTemplateTestDescription ("Byte_Column", "~/DynamicData/FieldTemplates/Integer_Edit.ascx"),
			new FieldTemplateTestDescription ("Int_Column", "~/DynamicData/FieldTemplates/Integer_Edit.ascx"),
			new FieldTemplateTestDescription ("Long_Column", "~/DynamicData/FieldTemplates/Integer_Edit.ascx"),
			new FieldTemplateTestDescription ("Bool_Column", "~/DynamicData/FieldTemplates/Boolean_Edit.ascx"),
			new FieldTemplateTestDescription ("String_Column", "~/DynamicData/FieldTemplates/Text_Edit.ascx"),
			new FieldTemplateTestDescription ("Float_Column", "~/DynamicData/FieldTemplates/Decimal_Edit.ascx"),
			new FieldTemplateTestDescription ("Single_Column", "~/DynamicData/FieldTemplates/Decimal_Edit.ascx"),
			new FieldTemplateTestDescription ("Double_Column", "~/DynamicData/FieldTemplates/Decimal_Edit.ascx"),
			new FieldTemplateTestDescription ("Decimal_Column", "~/DynamicData/FieldTemplates/Decimal_Edit.ascx"),
			new FieldTemplateTestDescription ("SByte_Column"),
			new FieldTemplateTestDescription ("UInt_Column"),
			new FieldTemplateTestDescription ("ULong_Column"),
			new FieldTemplateTestDescription ("Short_Column", "~/DynamicData/FieldTemplates/Integer_Edit.ascx"),
			new FieldTemplateTestDescription ("UShort_Column"),
			new FieldTemplateTestDescription ("DateTime_Column", "~/DynamicData/FieldTemplates/DateTime_Edit.ascx"),
			new FieldTemplateTestDescription ("FooEmpty_Column"),
			new FieldTemplateTestDescription ("Object_Column"),
			new FieldTemplateTestDescription ("ByteArray_Column"),
			new FieldTemplateTestDescription ("IntArray_Column"),
			new FieldTemplateTestDescription ("StringArray_Column"),
			new FieldTemplateTestDescription ("ObjectArray_Column"),
			new FieldTemplateTestDescription ("StringList_Column"),
			new FieldTemplateTestDescription ("Dictionary_Column"),
			new FieldTemplateTestDescription ("ICollection_Column"),
			new FieldTemplateTestDescription ("IEnumerable_Column"),
			new FieldTemplateTestDescription ("ICollectionByte_Column"),
			new FieldTemplateTestDescription ("IEnumerableByte_Column"),
			new FieldTemplateTestDescription ("ByteMultiArray_Column"),
			new FieldTemplateTestDescription ("BoolArray_Column"),
			new FieldTemplateTestDescription ("MaximumLength_Column4", "~/DynamicData/FieldTemplates/MultilineText_Edit.ascx"),
		};

		static void FieldTemplate_OnPreRenderComplete_1 (Page p)
		{
			var lc = p.FindControl ("ListView4") as ListView;
			Assert.IsNotNull (lc, "#A1");

			int counter = 1;
			foreach (var entry in fieldTemplateEditColumns) {
				string columnName = entry.ColumnName;
				var dc = lc.FindChild<PokerDynamicControl> (columnName);
				Assert.IsNotNull (dc, String.Format ("#B{0}-1 ({1})", counter, columnName));
				Assert.AreEqual (columnName, dc.ID, String.Format ("#B{0}-2 ({1})", counter, columnName));

				var templateControl = dc.FieldTemplate;
				var template = templateControl as FieldTemplateUserControl;
				if (entry.IsNull) {
					Assert.IsNull (templateControl, String.Format ("#B{0}-3 ({1})", counter, columnName));
					Assert.IsNull (template, String.Format ("#B{0}-4 ({1})", counter, columnName));
				} else {
					Assert.IsNotNull (templateControl, String.Format ("#B{0}-5 ({1})", counter, columnName));
					Assert.IsNotNull (template, String.Format ("#B{0}-6 ({1})", counter, columnName));
					Assert.AreEqual (entry.ControlVirtualPath, template.AppRelativeVirtualPath, String.Format ("#B{0}-7 ({1})", counter, columnName));
				}

				counter++;
			}
		}

		// This tests full type name templates
		[Test]
		public void FieldTemplate_2 ()
		{
			try {
				SetUp_FullTypeNameTemplates ();
				var test = new WebTest ("ListView_DynamicControl_03.aspx");
				test.Invoker = PageInvoker.CreateOnLoad (FieldTemplate_OnLoad_2);
				var p = test.Run ();
				Assert.IsNotNull (test.Response, "#X1");
				Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
				Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
				Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
			} finally {
				
			}
		}

		static List<string> nonDefaultFullTypeNameTemplates = new List<string> () {
			"System.Char.ascx",
			"System.Char.ascx.cs",
			"System.Byte.ascx",
			"System.Byte.ascx.cs",
			"System.Boolean.ascx",
			"System.Boolean.ascx.cs",
			"System.Int16.ascx",
			"System.Int16.ascx.cs",
			"System.Int32.ascx",
			"System.Int32.ascx.cs",
			"System.Int64.ascx",
			"System.Int64.ascx.cs",
			"System.String.ascx",
			"System.String.ascx.cs",
			"System.UInt16.ascx",
			"System.UInt16.ascx.cs",
			"System.UInt32.ascx",
			"System.UInt32.ascx.cs",
			"System.UInt64.ascx",
			"System.UInt64.ascx.cs",
			"System.SByte.ascx",
			"System.SByte.ascx.cs",
			"System.Object.ascx",
			"System.Object.ascx.cs",
			"System.Byte[].ascx",
			"System.Byte[].ascx.cs",
			"System.Collections.Generic.List`1[System.String].ascx",
			"System.Collections.Generic.List`1[System.String].ascx.cs",
			"MonoTests.Common.FooEmpty.ascx",
			"MonoTests.Common.FooEmpty.ascx.cs",
			"System.Collections.ICollection.ascx",
			"System.Collections.ICollection.ascx.cs",
		};

		void SetUp_FullTypeNameTemplates ()
		{
			Type type = GetType ();
			foreach (string tname in nonDefaultFullTypeNameTemplates)
				WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault." + tname, TestsSetup.BuildPath ("DynamicData/FieldTemplates/" + tname));
		}

		void CleanUp_FullTypeNameTemplates ()
		{
			string baseDir = WebTest.TestBaseDir;
			string filePath;

			foreach (string tname in nonDefaultFullTypeNameTemplates) {
				filePath = Path.Combine (baseDir, TestsSetup.BuildPath ("DynamicData/FieldTemplates/" + tname));
				try {
					if (File.Exists (filePath))
						File.Delete (filePath);
				} catch {
					// ignore
				}
			}
		}

		static List <FieldTemplateTestDescription> fieldTemplateNonDefaultColumns = new List <FieldTemplateTestDescription> ()
		{
			new FieldTemplateTestDescription ("Char_Column", "~/DynamicData/FieldTemplates/System.Char.ascx"),
			new FieldTemplateTestDescription ("Byte_Column", "~/DynamicData/FieldTemplates/System.Byte.ascx"),
			new FieldTemplateTestDescription ("Int_Column", "~/DynamicData/FieldTemplates/System.Int32.ascx"),
			new FieldTemplateTestDescription ("Long_Column", "~/DynamicData/FieldTemplates/System.Int64.ascx"),
			new FieldTemplateTestDescription ("Bool_Column", "~/DynamicData/FieldTemplates/System.Boolean.ascx"),
			new FieldTemplateTestDescription ("String_Column", "~/DynamicData/FieldTemplates/Text.ascx"),
			new FieldTemplateTestDescription ("Float_Column", "~/DynamicData/FieldTemplates/System.String.ascx"),
			new FieldTemplateTestDescription ("Single_Column", "~/DynamicData/FieldTemplates/System.String.ascx"),
			new FieldTemplateTestDescription ("Double_Column", "~/DynamicData/FieldTemplates/System.String.ascx"),
			new FieldTemplateTestDescription ("Decimal_Column", "~/DynamicData/FieldTemplates/System.String.ascx"),
			new FieldTemplateTestDescription ("SByte_Column", "~/DynamicData/FieldTemplates/System.SByte.ascx"),
			new FieldTemplateTestDescription ("UInt_Column", "~/DynamicData/FieldTemplates/System.UInt32.ascx"),
			new FieldTemplateTestDescription ("ULong_Column", "~/DynamicData/FieldTemplates/System.UInt64.ascx"),
			new FieldTemplateTestDescription ("Short_Column", "~/DynamicData/FieldTemplates/System.Int16.ascx"),
			new FieldTemplateTestDescription ("UShort_Column", "~/DynamicData/FieldTemplates/System.UInt16.ascx"),
			new FieldTemplateTestDescription ("DateTime_Column", "~/DynamicData/FieldTemplates/DateTime.ascx"),
			new FieldTemplateTestDescription ("FooEmpty_Column", "~/DynamicData/FieldTemplates/MonoTests.Common.FooEmpty.ascx"),
			new FieldTemplateTestDescription ("Object_Column", "~/DynamicData/FieldTemplates/System.Object.ascx"),
			new FieldTemplateTestDescription ("ByteArray_Column", "~/DynamicData/FieldTemplates/System.Byte[].ascx"),
			new FieldTemplateTestDescription ("IntArray_Column"),
			new FieldTemplateTestDescription ("StringArray_Column"),
			new FieldTemplateTestDescription ("ObjectArray_Column"),
			new FieldTemplateTestDescription ("StringList_Column"),

			// Doesn't work for some reason
			//new FieldTemplateTestDescription ("StringList_Column", "~/DynamicData/FieldTemplates/System.Collections.Generic.List`1[System.String].ascx"),
			new FieldTemplateTestDescription ("Dictionary_Column"),
			new FieldTemplateTestDescription ("ICollection_Column", "~/DynamicData/FieldTemplates/System.Collections.ICollection.ascx"),
			new FieldTemplateTestDescription ("IEnumerable_Column"),
			new FieldTemplateTestDescription ("ICollectionByte_Column"),
			new FieldTemplateTestDescription ("IEnumerableByte_Column"),
			new FieldTemplateTestDescription ("ByteMultiArray_Column"),
			new FieldTemplateTestDescription ("BoolArray_Column"),
			new FieldTemplateTestDescription ("MaximumLength_Column4", "~/DynamicData/FieldTemplates/System.String.ascx"),
		};

		static void FieldTemplate_OnLoad_2 (Page p)
		{
			var lc = p.FindControl ("ListView3") as ListView;
			Assert.IsNotNull (lc, "#A1");

			int counter = 1;
			foreach (var entry in fieldTemplateNonDefaultColumns) {
				string columnName = entry.ColumnName;
				var dc = lc.FindChild<PokerDynamicControl> (columnName);
				Assert.IsNotNull (dc, String.Format ("#B{0}-1 ({1})", counter, columnName));
				Assert.AreEqual (columnName, dc.ID, String.Format ("#B{0}-2 ({1})", counter, columnName));

				var templateControl = dc.FieldTemplate;
				var template = templateControl as FieldTemplateUserControl;
				if (entry.IsNull) {
					Assert.IsNull (templateControl, String.Format ("#B{0}-3 ({1})", counter, columnName));
					Assert.IsNull (template, String.Format ("#B{0}-4 ({1})", counter, columnName));
				} else {
					Assert.IsNotNull (templateControl, String.Format ("#B{0}-5 ({1})", counter, columnName));
					Assert.IsNotNull (template, String.Format ("#B{0}-6 ({1})", counter, columnName));
					Assert.AreEqual (entry.ControlVirtualPath, template.AppRelativeVirtualPath, String.Format ("#B{0}-7 ({1})", counter, columnName));
				}

				counter++;
			}
		}

		// This tests short type name templates
		[Test]
		public void FieldTemplate_3 ()
		{
			try {
				SetUp_ShortTypeNameTemplates ();
				var test = new WebTest ("ListView_DynamicControl_03.aspx");
				test.Invoker = PageInvoker.CreateOnLoad (FieldTemplate_OnLoad_3);
				var p = test.Run ();
				Assert.IsNotNull (test.Response, "#X1");
				Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
				Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
				Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
			} finally {

			}
		}

		static List<string> nonDefaultShortTypeNameTemplates = new List<string> () {
			"Char.ascx",
			"Char.ascx.cs",
			"Byte.ascx",
			"Byte.ascx.cs",
			"Boolean.ascx",
			"Boolean.ascx.cs",
			"Int16.ascx",
			"Int16.ascx.cs",
			"Int32.ascx",
			"Int32.ascx.cs",
			"Int64.ascx",
			"Int64.ascx.cs",
			"String.ascx",
			"String.ascx.cs",
			"UInt16.ascx",
			"UInt16.ascx.cs",
			"UInt32.ascx",
			"UInt32.ascx.cs",
			"UInt64.ascx",
			"UInt64.ascx.cs",
			"SByte.ascx",
			"SByte.ascx.cs",
			"Object.ascx",
			"Object.ascx.cs",
			"Byte[].ascx",
			"Byte[].ascx.cs",
			"FooEmpty.ascx",
			"FooEmpty.ascx.cs",
			"ICollection.ascx",
			"ICollection.ascx.cs",
		};

		void SetUp_ShortTypeNameTemplates ()
		{
			Type type = GetType ();
			foreach (string tname in nonDefaultShortTypeNameTemplates)
				WebTest.CopyResource (type, "MonoTests.WebPages.DynamicData.FieldTemplates_NonDefault." + tname, TestsSetup.BuildPath ("DynamicData/FieldTemplates/" + tname));
		}

		void CleanUp_ShortTypeNameTemplates ()
		{
			string baseDir = WebTest.TestBaseDir;
			string filePath;

			foreach (string tname in nonDefaultShortTypeNameTemplates) {
				filePath = Path.Combine (baseDir, TestsSetup.BuildPath ("DynamicData/FieldTemplates/" + tname));
				try {
					if (File.Exists (filePath))
						File.Delete (filePath);
				} catch {
					// ignore
				}
			}
		}

		static List<FieldTemplateTestDescription> fieldTemplateNonDefaultShortColumns = new List<FieldTemplateTestDescription> ()
		{
			new FieldTemplateTestDescription ("FooEmpty_Column", "~/DynamicData/FieldTemplates/FooEmpty.ascx"),
			new FieldTemplateTestDescription ("Char_Column", "~/DynamicData/FieldTemplates/Char.ascx"),
			new FieldTemplateTestDescription ("Byte_Column", "~/DynamicData/FieldTemplates/Byte.ascx"),
			new FieldTemplateTestDescription ("Int_Column", "~/DynamicData/FieldTemplates/Int32.ascx"),
			new FieldTemplateTestDescription ("Long_Column", "~/DynamicData/FieldTemplates/Int64.ascx"),
			new FieldTemplateTestDescription ("Bool_Column", "~/DynamicData/FieldTemplates/Boolean.ascx"),
			new FieldTemplateTestDescription ("String_Column", "~/DynamicData/FieldTemplates/Text.ascx"),
			new FieldTemplateTestDescription ("Float_Column", "~/DynamicData/FieldTemplates/String.ascx"),
			new FieldTemplateTestDescription ("Single_Column", "~/DynamicData/FieldTemplates/String.ascx"),
			new FieldTemplateTestDescription ("Double_Column", "~/DynamicData/FieldTemplates/String.ascx"),
			new FieldTemplateTestDescription ("Decimal_Column", "~/DynamicData/FieldTemplates/String.ascx"),
			new FieldTemplateTestDescription ("SByte_Column", "~/DynamicData/FieldTemplates/SByte.ascx"),
			new FieldTemplateTestDescription ("UInt_Column", "~/DynamicData/FieldTemplates/UInt32.ascx"),
			new FieldTemplateTestDescription ("ULong_Column", "~/DynamicData/FieldTemplates/UInt64.ascx"),
			new FieldTemplateTestDescription ("Short_Column", "~/DynamicData/FieldTemplates/Int16.ascx"),
			new FieldTemplateTestDescription ("UShort_Column", "~/DynamicData/FieldTemplates/UInt16.ascx"),
			new FieldTemplateTestDescription ("DateTime_Column", "~/DynamicData/FieldTemplates/DateTime.ascx"),
			new FieldTemplateTestDescription ("Object_Column", "~/DynamicData/FieldTemplates/Object.ascx"),
			new FieldTemplateTestDescription ("ByteArray_Column", "~/DynamicData/FieldTemplates/Byte[].ascx"),
			new FieldTemplateTestDescription ("IntArray_Column"),
			new FieldTemplateTestDescription ("StringArray_Column"),
			new FieldTemplateTestDescription ("ObjectArray_Column"),
			new FieldTemplateTestDescription ("StringList_Column"),

			// Doesn't work for some reason
			//new FieldTemplateTestDescription ("StringList_Column", "~/DynamicData/FieldTemplates/List`1[System.String].ascx"),
			new FieldTemplateTestDescription ("Dictionary_Column"),
			new FieldTemplateTestDescription ("ICollection_Column", "~/DynamicData/FieldTemplates/ICollection.ascx"),
			new FieldTemplateTestDescription ("IEnumerable_Column"),
			new FieldTemplateTestDescription ("ICollectionByte_Column"),
			new FieldTemplateTestDescription ("IEnumerableByte_Column"),
			new FieldTemplateTestDescription ("ByteMultiArray_Column"),
			new FieldTemplateTestDescription ("BoolArray_Column"),
			new FieldTemplateTestDescription ("MaximumLength_Column4", "~/DynamicData/FieldTemplates/String.ascx"),
		};

		static void FieldTemplate_OnLoad_3 (Page p)
		{
			var lc = p.FindControl ("ListView3") as ListView;
			Assert.IsNotNull (lc, "#A1");

			int counter = 1;
			foreach (var entry in fieldTemplateNonDefaultShortColumns) {
				string columnName = entry.ColumnName;
				var dc = lc.FindChild<PokerDynamicControl> (columnName);
				Assert.IsNotNull (dc, String.Format ("#B{0}-1 ({1})", counter, columnName));
				Assert.AreEqual (columnName, dc.ID, String.Format ("#B{0}-2 ({1})", counter, columnName));

				var templateControl = dc.FieldTemplate;
				var template = templateControl as FieldTemplateUserControl;
				if (entry.IsNull) {
					Assert.IsNull (templateControl, String.Format ("#B{0}-3 ({1})", counter, columnName));
					Assert.IsNull (template, String.Format ("#B{0}-4 ({1})", counter, columnName));
				} else {
					Assert.IsNotNull (templateControl, String.Format ("#B{0}-5 ({1})", counter, columnName));
					Assert.IsNotNull (template, String.Format ("#B{0}-6 ({1})", counter, columnName));
					Assert.AreEqual (entry.ControlVirtualPath, template.AppRelativeVirtualPath, String.Format ("#B{0}-7 ({1})", counter, columnName));
				}

				counter++;
			}
		}

		[Test]
		public void Table ()
		{
			var test = new WebTest ("ListView_DynamicControl_01.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (Table_OnLoad);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");
		}

		static void Table_OnLoad (Page p)
		{
			var lc = p.FindControl ("ListView1") as ListView;
			Assert.IsNotNull (lc, "#A1");

			var dc = lc.FindChild<DynamicControl> ("FirstName");
			Assert.IsNotNull (dc, "#A1-1");
			Assert.IsNotNull (dc.Table, "#B1");

			// Safe not to check for GetModel's return value - it throws if model isn't found, same
			// goes for GetTable
			MetaTable table = MetaModel.GetModel (typeof (EmployeesDataContext)).GetTable ("EmployeeTable");
			Assert.AreEqual (table, dc.Table, "#B1-1");
			Assert.AreEqual (dc.Table, dc.Column.Table, "#B1-2");
		}

		[Test]
		public void UIHint ()
		{
			var test = new WebTest ("ListView_DynamicControl_01.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (UIHint_OnLoad);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");

			Assert.IsTrue (p.IndexOf ("<span class=\"field\">LastName</span>: <span class=\"customFieldTemplate\">") != -1, "#Y1");
			Assert.IsTrue (p.IndexOf ("<span class=\"field\">FirstName</span>: <span class=\"defaultTemplate\">") != -1, "#Y1-1");
		}

		static void UIHint_OnLoad (Page p)
		{
			var lc = p.FindControl ("ListView1") as ListView;
			Assert.IsNotNull (lc, "#A1");
			
			var dc = lc.FindChild<DynamicControl> ("FirstName");
			Assert.IsNotNull (dc, "#B1");
			Assert.AreEqual ("FirstName", dc.DataField, "#B1-1");

			// Changes below won't affect rendering - we're being called too late in the process
			// This is just to test if the property is settable, what are the defaults and whether
			// they can be overriden

			// No UIHint attribute on the associated field, no explicit setting
			Assert.AreEqual (String.Empty, dc.UIHint, "#C1");
			dc.UIHint = "MyCustomUIHintTemplate_Text";
			Assert.AreEqual ("MyCustomUIHintTemplate_Text", dc.UIHint, "#C1-1");
			
			dc = lc.FindChild<DynamicControl> ("LastName");
			Assert.IsNotNull (dc, "#D1");
			Assert.AreEqual ("LastName", dc.DataField, "#D1-1");

			// UIHint attribute found on the associated field
			Assert.AreEqual ("CustomFieldTemplate", dc.UIHint, "#D1-2");
			dc.UIHint = "MyCustomUIHintTemplate_Text";
			Assert.AreEqual ("MyCustomUIHintTemplate_Text", dc.UIHint, "#D1-3");
		}

		[Test]
		public void UIHint_1 ()
		{
			var test = new WebTest ("ListView_DynamicControl_02.aspx");
			test.Invoker = PageInvoker.CreateOnLoad (UIHint_OnLoad_1);
			var p = test.Run ();
			Assert.IsNotNull (test.Response, "#X1");
			Assert.AreNotEqual (HttpStatusCode.NotFound, test.Response.StatusCode, "#X1-1{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.AreNotEqual (HttpStatusCode.InternalServerError, test.Response.StatusCode, "#X1-2{0}Returned HTML:{0}{1}", Environment.NewLine, p);
			Assert.IsFalse (String.IsNullOrEmpty (p), "#X1-3");

			Assert.IsTrue (p.IndexOf ("<span class=\"field\">LastName</span>: <span class=\"myCustomUIHintTemplate_Text\">") != -1, "#Y1");
			Assert.IsTrue (p.IndexOf ("<span class=\"field\">FirstName</span>: <span class=\"defaultTemplate\">") != -1, "#Y1-1");
		}

		static void UIHint_OnLoad_1 (Page p)
		{
			var lc = p.FindControl ("ListView2") as ListView;
			Assert.IsNotNull (lc, "#A1");

			var dc = lc.FindChild<DynamicControl> ("FirstName2");
			Assert.IsNotNull (dc, "#B1");
			Assert.AreEqual ("FirstName", dc.DataField, "#B1-1");

			// No UIHint attribute on the associated field, no explicit setting
			Assert.AreEqual (String.Empty, dc.UIHint, "#C1");

			dc = lc.FindChild<DynamicControl> ("LastName2");
			Assert.IsNotNull (dc, "#D1");
			Assert.AreEqual ("LastName", dc.DataField, "#D1-1");

			// UIHint attribute found on the associated field but overriden in the page
			Assert.AreEqual ("MyCustomUIHintTemplate_Text", dc.UIHint, "#D1-2");
		}
	}
}
