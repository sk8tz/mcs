//
// Tests for System.Web.UI.WebControls.FormView.cs 
//
// Author:
//	Merav Sudri (meravs@mainsoft.com)
//
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using NUnit.Framework;
using System;
using System.Threading;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;


namespace MonoTests.System.Web.UI.WebControls
{
	public class  ObjectDataSourcePoker : ObjectDataSource
	{
		public ObjectDataSourcePoker () // constructor
		{
			
		TrackViewState ();
		}

		public object SaveState ()
		{	
		 return SaveViewState ();			
		}

		public void LoadState (object o)
		{
		  LoadViewState (o);
			
		}

		public StateBag StateBag 
		{
		 get { return base.ViewState; }
		}

		public string Render ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);
			Render (tw);
			return sw.ToString ();
		}

		public void DoOnBubbleEvent (Object source, EventArgs e)
		{
			base.OnBubbleEvent (source, e);
		}

		public object DoSaveControlState ()
		{
			return base.SaveControlState ();
		}

		public void DoLoadControlState (object savedState)
		{
			 base.LoadControlState (savedState);
		}

	}

	[TestFixture]
	public class ObjectDataSourceTest
	{

		[TestFixtureTearDown]
		public void TearDown ()
		{
			WebTest.Unload ();
		}

		public static void InitObjectDataSource (ObjectDataSourcePoker ds, string action)
		{
			Parameter p1, p2, p3;
			switch (action) {		
				
			case "insert":	p1 = new Parameter ("ID", TypeCode.String, "1004");
					p2 = new Parameter ("fname", TypeCode.String, "David");
					p3 = new Parameter ("LName", TypeCode.String, "Eli");
					break;
				
			case "update": 	p1 = new Parameter ("ID", TypeCode.String, "1001");
					p2 = new Parameter ("FName", TypeCode.String, "David");
					p3 = new Parameter ("LName", TypeCode.String, "Eli");
					break;
			case "DBNull":  p1 = new Parameter ("ID");
					p2 = new Parameter ("FName");
					p3 = new Parameter ("LName");
					break;
				
			default: 	p1 = new Parameter ("ID", TypeCode.String, "1001");
					p2 = new Parameter ("FName", TypeCode.String, "Mahesh");
					p3 = new Parameter ("LName", TypeCode.String, "chand");
					break;
				
			}
			ds.SelectMethod = "GetMyData";
			ds.DeleteMethod = "Delete";
			ds.InsertMethod = "Insert";
			ds.UpdateMethod = "Update";
			ds.SelectCountMethod = "SelectCount";
			ds.DeleteParameters.Add (p1);
			ds.DeleteParameters.Add (p2);
			ds.DeleteParameters.Add (p3);
			ds.InsertParameters.Add (p1);
			ds.InsertParameters.Add (p2);
			ds.InsertParameters.Add (p3);
			ds.UpdateParameters.Add (p1);
			ds.UpdateParameters.Add (p2);
			ds.UpdateParameters.Add (p3);
			ds.ID = "MyObject";
			ds.TypeName = "MonoTests.System.Web.UI.WebControls.MyTableObject";
		}

		//Default properties

		[Test]		
		public void ObjectDataSource_DefaultProperties ()
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			
			Assert.AreEqual (ConflictOptions.OverwriteChanges, ods.ConflictDetection, "ConflictDetection");			
			Assert.AreEqual ("",ods.DataObjectTypeName ,"DataObjectTypeName ");
			Assert.AreEqual ("", ods.DeleteMethod, "DeleteMethod");
			Assert.AreEqual (typeof(ParameterCollection),ods.DeleteParameters.GetType (),"DeleteParameters");			
			Assert.AreEqual (false, ods.EnablePaging, "EnablePaging ");
			Assert.AreEqual ("", ods.FilterExpression, "FilterExpression ");
			Assert.AreEqual (typeof (ParameterCollection), ods.FilterParameters.GetType (), "FilterParameters");
			Assert.AreEqual ("", ods.InsertMethod, "InsertMethod ");
			Assert.AreEqual (typeof (ParameterCollection), ods.InsertParameters.GetType (), "InsertParameters ");
			Assert.AreEqual ("maximumRows", ods.MaximumRowsParameterName, "MaximumRowsParameterName");
			Assert.AreEqual ("{0}", ods.OldValuesParameterFormatString, "OldValuesParameterFormatString");
			Assert.AreEqual ("", ods.SelectCountMethod, "SelectCountMethod");
			Assert.AreEqual ("", ods.SelectMethod, "SelectMethod ");
			Assert.AreEqual (typeof (ParameterCollection), ods.SelectParameters.GetType (), "SelectParameters");
			Assert.AreEqual ("", ods.SortParameterName, "SortParameterName");			
			Assert.AreEqual ("startRowIndex", ods.StartRowIndexParameterName, "StartRowIndexParameterName");
			Assert.AreEqual ("", ods.TypeName, "TypeName");
			Assert.AreEqual ("", ods.UpdateMethod, "UpdateMethod ");
			Assert.AreEqual (typeof (ParameterCollection), ods.UpdateParameters.GetType (), "UpdateParameters");
			
		}

		[Test]
		public void ObjectDataSource_NotWorkingDefaultProperties ()
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			Assert.AreEqual (0, ods.CacheDuration, "CacheDuration");
			Assert.AreEqual (DataSourceCacheExpiry.Absolute, ods.CacheExpirationPolicy, "CacheExpirationPolicy");
			Assert.AreEqual ("", ods.CacheKeyDependency, "CacheKeyDependency");
			Assert.AreEqual (false, ods.ConvertNullToDBNull, "ConvertNullToDBNull ");
			Assert.AreEqual (false, ods.EnableCaching, "EnableCaching ");
			Assert.AreEqual ("", ods.SqlCacheDependency, "SqlCacheDependency");
		}

		//Non default properties values

		[Test]		
		public void ObjectDataSource_AssignToDefaultProperties ()
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods,"");				
			ods.ConflictDetection = ConflictOptions.CompareAllValues;
			Assert.AreEqual (ConflictOptions.CompareAllValues, ods.ConflictDetection, "ConflictDetection");			
			ods.DataObjectTypeName = "MyData";
			Assert.AreEqual ("MyData", ods.DataObjectTypeName, "DataObjectTypeName ");
			Assert.AreEqual ("Delete", ods.DeleteMethod, "DeleteMethod");
			Assert.AreEqual (3, ods.DeleteParameters.Count, "DeleteParameters");			
			ods.EnablePaging = true;
			Assert.AreEqual (true, ods.EnablePaging, "EnablePaging ");
			ods.FilterExpression = "ID='{0}'";
			Assert.AreEqual ("ID='{0}'", ods.FilterExpression, "FilterExpression ");
			TextBox TextBox1=new TextBox ();
			TextBox1.Text ="1001"; 
			FormParameter p=new FormParameter ("ID","TextBox1");
			p.DefaultValue = "1002";
			ods.FilterParameters.Add (p);  
			Assert.AreEqual ("ID", ods.FilterParameters[0].Name, "FilterParameters1");
			Assert.AreEqual ("1002", ods.FilterParameters[0].DefaultValue , "FilterParameters2");
			Assert.AreEqual ("TextBox1", ((FormParameter )ods.FilterParameters[0]).FormField, "FilterParameters3");
			Assert.AreEqual ("Insert", ods.InsertMethod, "InsertMethod ");
			Assert.AreEqual ("ID", ods.InsertParameters[0].Name , "InsertParameters ");
			ods.MaximumRowsParameterName = "SelectCount";
			Assert.AreEqual ("SelectCount", ods.MaximumRowsParameterName, "MaximumRowsParameterName");
			ods.OldValuesParameterFormatString = "ID";
			Assert.AreEqual ("ID", ods.OldValuesParameterFormatString, "OldValuesParameterFormatString");
			Assert.AreEqual ("SelectCount", ods.SelectCountMethod, "SelectCountMethod");
			Assert.AreEqual ("GetMyData", ods.SelectMethod, "SelectMethod ");
			Parameter dummy = new Parameter ();
			dummy.Name = "Test";
			ods.SelectParameters.Add (dummy);
			Assert.AreEqual ("Test", ods.SelectParameters[0].Name , "SelectParameters");
			ods.SortParameterName = "sortExpression";
			Assert.AreEqual ("sortExpression", ods.SortParameterName, "SortParameterName");			
			ods.StartRowIndexParameterName = "ID";
			Assert.AreEqual ("ID", ods.StartRowIndexParameterName, "StartRowIndexParameterName");
			Assert.AreEqual ("MonoTests.System.Web.UI.WebControls.MyTableObject", ods.TypeName, "TypeName");
			Assert.AreEqual ("Update", ods.UpdateMethod, "UpdateMethod ");
			Assert.AreEqual ("FName", ods.UpdateParameters[1].Name, "UpdateParameters");		
			
		}

		[Test]
		public void ObjectDataSource_NotWorkingAssignToDefaultProperties ()
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			ods.CacheDuration = 1000;
			Assert.AreEqual (1000, ods.CacheDuration, "CacheDuration");
			ods.CacheExpirationPolicy = DataSourceCacheExpiry.Sliding;
			Assert.AreEqual (DataSourceCacheExpiry.Sliding, ods.CacheExpirationPolicy, "CacheExpirationPolicy");
			ods.CacheKeyDependency = "ID";
			Assert.AreEqual ("ID", ods.CacheKeyDependency, "CacheKeyDependency");
			ods.ConvertNullToDBNull = true;
			Assert.AreEqual (true, ods.ConvertNullToDBNull, "ConvertNullToDBNull ");
			ods.EnableCaching = true;
			Assert.AreEqual (true, ods.EnableCaching, "EnableCaching ");
			ods.SqlCacheDependency = "Northwind:Employees";
			Assert.AreEqual ("Northwind:Employees", ods.SqlCacheDependency, "SqlCacheDependency");
		}

		//ViewState

		[Test]
		public void ObjectDataSource_ViewState ()
		{
			ObjectDataSourcePoker  ods = new ObjectDataSourcePoker ();
			//InitObjectDataSource (ods,"");	
			ObjectDataSourcePoker copy = new ObjectDataSourcePoker ();
			FormParameter p = new FormParameter ("ID", "TextBox1");
			p.DefaultValue = "1002";
			ods.FilterParameters.Add (p);
			Parameter p1 = new Parameter ("ID", TypeCode.String, "1001");
			Parameter p2 = new Parameter ("FName", TypeCode.String, "Mahesh");		
			ods.SelectParameters.Add (p1);
			ods.SelectParameters.Add (p2); 
			object state = ods.SaveState ();
			copy.LoadState (state);
			Assert.AreEqual ("ID", copy.FilterParameters [0].Name, "ViewStateFilterParameters1");
			Assert.AreEqual ("1002", copy.FilterParameters [0].DefaultValue, "ViewStateFilterParameters2");
			Assert.AreEqual ("1001", copy.SelectParameters[0].DefaultValue, "ViewStateSelectParameters1");
			Assert.AreEqual (2, copy.SelectParameters.Count , "ViewStateSelectParameters2");
		}

		//Properties functionality

		public void ObjectDataSource_ConflictDetection ()
		{ 
			//Not implemented			 
		}

		[Test]
		[Category("NotWorking")]
		[Category ("NunitWeb")]
		public void ObjectDataSource_ConvertNullToDBNull ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (new PageDelegate (ConvertNullToDBNull))).Run ();
		}

		
		public static void ConvertNullToDBNull (Page p)
		{
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods,"DBNull");
			bool dbnull = false;
			ods.ConvertNullToDBNull = true;
			try {
				ods.Delete ();
			}
			catch (Exception ex) {
				Assert.AreEqual (true,
					ex.Message.Contains ("type 'System.DBNull' cannot be converted to type 'System.String'") || // dotnet
					ex.Message.Contains ("Value cannot be null."), "ConvertNullToDBNull"); // mono
				dbnull = true;
			}
			Assert.AreEqual (true, dbnull, "ConvertNullToDBNull2");
		}

		[Test]
		[Category ("NunitWeb")]
		public void ObjectDataSource_FilterExpression ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (FilterExpression))).Run ();
			string newHtml= HtmlDiff.GetControlFromPageHtml (html);
			string origHtml = @"<table cellspacing=""0"" rules=""all"" border=""1"" style=""border-collapse:collapse;"">
						<tr>
						<td>ID</td><td>FName</td><td>LName</td>
						</tr><tr>
						<td>1002</td><td>Melanie</td><td>Talmadge</td>
						</tr>
						</table>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "FilterExpression");
		}


		public static void FilterExpression (Page p)
		{
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			DataGrid dg = new DataGrid ();			
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			ods.FilterExpression = "ID='1002'";
			p.Controls.Add (lcb); 
			p.Controls.Add (dg);
			p.Controls.Add (ods);
			p.Controls.Add (lce); 
			dg.DataSource = ods;
			dg.DataBind ();
			
		}

		[Test]
		[Category ("NunitWeb")]
		public void ObjectDataSource_FilterParameter ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (FilterParameter))).Run ();
			string newHtml = HtmlDiff.GetControlFromPageHtml (html);
			string origHtml = @"<table cellspacing=""0"" rules=""all"" border=""1"" style=""border-collapse:collapse;"">
						<tr>
						<td>ID</td><td>FName</td><td>LName</td>
						</tr><tr>
						<td>1003</td><td>Vinay</td><td>Bansal</td>
						</tr>
						</table>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "FilterExpression");
		}


		public static void FilterParameter (Page p)
		{
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			DataGrid dg = new DataGrid ();
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			ods.FilterExpression = "{0}";
			Parameter p1 = new Parameter ("ID", TypeCode.String, "ID=1003");
			ods.FilterParameters.Add (p1); 
			p.Controls.Add (lcb);
			p.Controls.Add (dg);
			p.Controls.Add (ods);
			p.Controls.Add (lce);
			dg.DataSource = ods;
			dg.DataBind ();

		}


		[Test]
		[Category ("NunitWeb")]
		[Category ("NotWorking")]
		public void ObjectDataSource_EnablePaging ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (EnablePaging))).Run ();
			string newHtml = HtmlDiff.GetControlFromPageHtml (html);
			string origHtml = @"<div>
						<table cellspacing=""0"" rules=""all"" border=""1"" style=""border-collapse:collapse;"">
						<tr>
						<th scope=""col"">Name</th><th scope=""col"">Number</th>
						</tr><tr>
						<td>Number0</td><td>0</td>
						</tr><tr>
						<td>Number1</td><td>1</td>
						</tr><tr>
						<td>Number2</td><td>2</td>
						</tr><tr>
						<td>Number3</td><td>3</td>
						</tr><tr>
						<td>Number4</td><td>4</td>
						</tr><tr>
						<td colspan=""2""><table border=""0"">
						<tr>
						<td><span>1</span></td><td><a href=""javascript:__doPostBack('ctl01','Page$2')"">2</a></td><td><a href=""javascript:__doPostBack('ctl01','Page$3')"">3</a></td><td><a href=""javascript:__doPostBack('ctl01','Page$4')"">4</a></td>
						</tr>
						</table></td>
						</tr>
						</table>
						</div>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "EnablePaging");
		}


		public static void EnablePaging (Page p)
		{
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			GridView  gv = new GridView ();
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			ods.ID = "ObjectDataSource1";
			ods.TypeName = "MonoTests.System.Web.UI.WebControls.MyTableObject";
			ods.SelectMethod = "SelectForPaging";
			ods.EnablePaging = true;
			ods.SelectCountMethod = "SelectCount";
			ods.MaximumRowsParameterName = "maxRows";
			ods.StartRowIndexParameterName = "startIndex";
			gv.AllowPaging = true;
			gv.PageSize = 5;
			p.Controls.Add (lcb);
			p.Controls.Add (gv);
			p.Controls.Add (ods);
			p.Controls.Add (lce);
			gv.DataSourceID = "ObjectDataSource1";
			gv.DataBind ();	

		}

		//public methods

		[Test]
		[Category ("NunitWeb")]
		public void ObjectDataSource_Delete ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (DeleteMethod))).Run ();
			string newHtml = HtmlDiff.GetControlFromPageHtml (html);
			string origHtml = @"<div>
					<table cellspacing=""0"" rules=""all"" border=""1"" style=""border-collapse:collapse;"">
					<tr>
					<td>ID</td><td>1002</td>
					</tr><tr>
					<td>FName</td><td>Melanie</td>
					</tr><tr>
					<td>LName</td><td>Talmadge</td>
					</tr>
					</table>
					</div>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "DeleteRender");

		}

		public static void DeleteMethod (Page p)
		{
			LiteralControl lcb = new LiteralControl (HtmlDiff.BEGIN_TAG);
			LiteralControl lce = new LiteralControl (HtmlDiff.END_TAG);
			MyTableObject.ds = MyTableObject.CreateDataTable ();  
			DetailsView dv = new DetailsView ();
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			ods.Deleted += new ObjectDataSourceStatusEventHandler (odc_Deleted);
			ods.Deleting += new ObjectDataSourceMethodEventHandler (odc_Deleting);	
			InitObjectDataSource (ods,"");
			dv.Page = p;
			ods.Page = p;
			dv.DataKeyNames = new string[] { "ID" };
			dv.DataSource = ods;
			p.Controls.Add (lcb); 
			p.Controls.Add (ods);
			p.Controls.Add (dv);
			p.Controls.Add (lce); 
			dv.DataBind ();
			Assert.AreEqual (3, dv.DataItemCount, "BeforeDelete1");
			Assert.AreEqual (1001, dv.SelectedValue, "BeforeDelete2");
			Assert.AreEqual (false, deleting, "BeforeDeletingEvent");
			Assert.AreEqual (false, deleted, "BeforeDeletedEvent");
			ods.Delete ();			
			dv.DataBind ();
			Assert.AreEqual (true, deleting, "AfterDeletingEvent");
			Assert.AreEqual (true, deleted, "AfterDeletedEvent");
			Assert.AreEqual (2, dv.DataItemCount, "BeforeDelete1");
			Assert.AreEqual (1002, dv.SelectedValue, "BeforeDelete2");
		}

		[Test]
		[Category ("NunitWeb")]
		public void ObjectDataSource_Select ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (SelectMethod))).Run ();
		}

		
		public static void SelectMethod (Page p)
		{
			MyTableObject.ds = MyTableObject.CreateDataTable ();  
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods,"");			
			ods.Selected += new ObjectDataSourceStatusEventHandler (odc_Selected);
			ods.Selecting += new ObjectDataSourceSelectingEventHandler (odc_Selecting);
			p.Controls.Add (ods);
			Assert.AreEqual (false, selecting, "BeforeSelectingEvent");
			Assert.AreEqual (false, selected, "BeforeSelectedEvent");			
			IEnumerable table = (IEnumerable) ods.Select ();
			Assert.AreEqual (3,((DataView) table).Count, "ItemsCount");
			Assert.AreEqual ("Mahesh", ((DataView) table)[0].Row.ItemArray[1], "FirstItemData");
			Assert.AreEqual (1002, ((DataView) table)[1].Row.ItemArray[0], "SecondItemData");
			Assert.AreEqual ("Bansal", ((DataView) table)[2].Row.ItemArray[2], "ThirdItemData");			
			Assert.AreEqual (true, selecting, "AfterSelectingEvent");
			Assert.AreEqual (true, selected, "AfterSelectedEvent");
		}

		[Test]
		[Category ("NunitWeb")]
		public void ObjectDataSource_Select_Cached ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (SelectMethodCached))).Run ();
		}


		public static void SelectMethodCached (Page p)
		{
			MyTableObject.ds = MyTableObject.CreateDataTable ();
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			ods.EnableCaching = true;
			InitObjectDataSource (ods, "");
			p.Controls.Add (ods);
			ods.Selecting += new ObjectDataSourceSelectingEventHandler (odc_Selecting);

			selecting = false;
			IEnumerable table = (IEnumerable) ods.Select ();
			Assert.AreEqual (true, selecting, "AfterSelectingEvent");

			selecting = false;
			IEnumerable table2 = (IEnumerable) ods.Select ();
			Assert.AreEqual (false, selecting, "AfterSelectingEvent");
		}
		[Test]
		[Category ("NunitWeb")]
		public void ObjectDataSource_Insert ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (InsertMethod))).Run ();
		}

		public static void InsertMethod (Page p)
		{
			MyTableObject.ds = MyTableObject.CreateDataTable ();  
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods,"insert");
			ods.Inserted += new ObjectDataSourceStatusEventHandler (odc_Inserted);
			ods.Inserting += new ObjectDataSourceMethodEventHandler (odc_Inserting);
			p.Controls.Add (ods);			
			Assert.AreEqual (3, ((DataView) ods.Select ()).Count, "BeforeInsert");
			Assert.AreEqual (false, inserted , "BeforeInsertedEvent");
			Assert.AreEqual (false, inserting , "BeforeInsertingEvent");
			ods.Insert ();		
			Assert.AreEqual (4, ((DataView) ods.Select ()).Count , "AfterInsert1");
			Assert.AreEqual (1004,((DataView) ods.Select ())[3].Row.ItemArray[0], "AfterInsert2");
			Assert.AreEqual ("David", ((DataView) ods.Select ())[3].Row.ItemArray[1], "AfterInsert3");
			Assert.AreEqual (true, inserted, "AfterInsertedEvent");
			Assert.AreEqual (true, inserting, "AfterInsertingEvent");
			
		}

		[Test]
		[Category ("NunitWeb")]
		public void ObjectDataSource_Update ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (UpdateMethod))).Run ();
		}

		public static void UpdateMethod (Page p)
		{
			MyTableObject.ds = MyTableObject.CreateDataTable ();  
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "update");
			ods.Updated += new ObjectDataSourceStatusEventHandler (odc_Updated);
			ods.Updating += new ObjectDataSourceMethodEventHandler (odc_Updating);
			p.Controls.Add (ods);
			Assert.AreEqual (3, ((DataView) ods.Select ()).Count, "BeforeUpdate1");
			Assert.AreEqual (1001, ((DataView) ods.Select ())[0].Row.ItemArray[0], "BeforeUpdate2");
			Assert.AreEqual ("Mahesh", ((DataView) ods.Select ())[0].Row.ItemArray[1], "BeforeUpdate3");
			Assert.AreEqual ("Chand", ((DataView) ods.Select ())[0].Row.ItemArray[2], "BeforeUpdate4");
			Assert.AreEqual (false, updated, "BeforeUpdateEvent");
			Assert.AreEqual (false, updating, "BeforeUpdatingEvent");
			ods.Update ();
			Assert.AreEqual (3, ((DataView) ods.Select ()).Count, "AfterUpdate1");
			Assert.AreEqual (1001, ((DataView) ods.Select ())[0].Row.ItemArray[0], "AfterUpdate2");
			Assert.AreEqual ("David", ((DataView) ods.Select ())[0].Row.ItemArray[1], "AfterUpdate3");
			Assert.AreEqual ("Eli", ((DataView) ods.Select ())[0].Row.ItemArray[2], "AfterUpdate4");
			Assert.AreEqual (true, updated, "AfterUpdateEvent");
			Assert.AreEqual (true, updating, "AfterUpdatingEvent");
		}

		

		//Events

		private static bool deleted = false;
		private static bool deleting = false;
		private static bool filtering = false;
		private static bool inserted = false;
		private static bool inserting = false;
		private static bool objectCreated = false;
		private static bool objectCreating = false;
		private static bool objectDisposing = false;
		private static bool selected = false;
		private static bool selecting = false;
		private static bool updated = false;
		private static bool updating = false;

		// Tests for events Select,Update,Delete and Insert include in Select,Update,Delete and Insert methods tests.

		[Test]
		[Category ("NunitWeb")]
		public void ObjectDataSource_Events ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (EventsTest))).Run ();
		}

				
		public static void EventsTest (Page p)
		{				
			
			MyTableObject.ds = MyTableObject.CreateDataTable ();  
			DetailsView dv = new DetailsView  ();
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			ods.ObjectCreated += new ObjectDataSourceObjectEventHandler (odc_ObjectCreated);
			ods.ObjectCreating += new ObjectDataSourceObjectEventHandler (odc_ObjectCreating);
			InitObjectDataSource (ods,"");
			ods.FilterExpression = "ID='1001'";			
			dv.Page = p;
			ods.Page = p;
			dv.DataKeyNames = new string[] { "ID" };
			dv.DataSource = ods;
			p.Controls.Add (ods);
			p.Controls.Add (dv);
			dv.DataBind ();							
			ods.Filtering += new ObjectDataSourceFilteringEventHandler (odc_Filtering);
			Assert.AreEqual (false, filtering, "BeforeFilteringEvent");
			ods.Select ();
			Assert.AreEqual (true, filtering, "AfterFilteringEvent");
			ods.ObjectDisposing += new ObjectDataSourceDisposingEventHandler (odc_ObjectDisposing);
			//ToDo: Dispose, ObjectCreated and ObjectCreating should be tested.
			
		}
		
		static void odc_Updating (object sender, ObjectDataSourceMethodEventArgs e)
		{
			updating = true;
		}

		static void odc_Updated (object sender, ObjectDataSourceStatusEventArgs e)
		{
			updated = true;
		}

		static void odc_Selecting (object sender, ObjectDataSourceSelectingEventArgs e)
		{
			selecting = true;
		}

		static void odc_Selected (object sender, ObjectDataSourceStatusEventArgs e)
		{
			selected = true;
		}

		static void odc_ObjectDisposing (object sender, ObjectDataSourceDisposingEventArgs e)
		{
			objectDisposing = true;
		}

		static void odc_ObjectCreating (object sender, ObjectDataSourceEventArgs e)
		{
			objectCreating = true;
		}

		static void odc_ObjectCreated (object sender, ObjectDataSourceEventArgs e)
		{
			objectCreated = true;
		}

		static void odc_Inserting (object sender, ObjectDataSourceMethodEventArgs e)
		{
			inserting = true;
		}

		static void odc_Inserted (object sender, ObjectDataSourceStatusEventArgs e)
		{
			inserted = true;
		}

		static void odc_Filtering (object sender, ObjectDataSourceFilteringEventArgs e)
		{
			filtering = true;
		}

		static void odc_Deleting (object sender, ObjectDataSourceMethodEventArgs e)
		{
			deleting = true;
		}

		static void odc_Deleted (object sender, ObjectDataSourceStatusEventArgs e)
		{
			deleted = true;			
		}

		//Excpetions

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		[Category ("NunitWeb")]
		public void ObjectDataSource_EnableCachingException ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (EnableCachingException))).Run ();
		}

		
		public static void EnableCachingException (Page p)
		{
			MyTableObject.ds = MyTableObject.CreateDataTable ();
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			ods.SelectMethod = "SelectException";
			ods.EnableCaching = true;			
			p.Controls.Add (ods);			
			IEnumerable table = (IEnumerable) ods.Select ();
			
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		[Category ("NunitWeb")]
		public void ObjectDataSource_FilterExpressionException ()
		{
			string html = new WebTest (PageInvoker.CreateOnLoad (
				new PageDelegate (FilterExpressionException))).Run ();
		}


		public static void FilterExpressionException (Page p)
		{
			MyTableObject.ds = MyTableObject.CreateDataTable ();
			ObjectDataSourcePoker ods = new ObjectDataSourcePoker ();
			InitObjectDataSource (ods, "");
			ods.SelectMethod = "SelectException";
			ods.FilterExpression  = "ID='1001'";
			p.Controls.Add (ods);
			IEnumerable table = (IEnumerable) ods.Select ();

		}
	}

	public class MyTableObject 
	{
		public static DataTable ds = CreateDataTable ();
		public static DataTable GetMyData ()
		{
			return ds;
		}

		public static DbDataReader SelectException ()
		{
			return new DataTableReader (new DataTable ());
		}

		public static int SelectCount ()
		{
			return 20;
		}

		
		public static DataTable Delete (string ID, string FName, string LName)
		{
			DataRow dr = ds.Rows.Find (ID);
			ds.Rows.Remove (dr);
			return ds;

		}

		public static DataTable Update (string ID, string FName, string LName)
		{
			DataRow dr = ds.Rows.Find (ID);
			if (dr == null) {
				Label lbl = new Label ();
				lbl.Text = "ID doesn't exist. update only FName and LName";
				return ds;
			}
			dr["FName"] = FName;
			dr["LName"] = LName;
			return ds;

		}

		public static DataTable Insert (string ID, string FName, string LName)
		{
			DataRow dr = ds.NewRow ();
			dr["ID"] = ID;
			dr["FName"] = FName;
			dr["LName"] = LName;
			ds.Rows.Add (dr);
			return ds;
		}


		public static DataTable CreateDataTable ()
		{

			DataTable aTable = new DataTable ("A");
			DataColumn dtCol;
			DataRow dtRow;

			// Create ID column and add to the DataTable.

			dtCol = new DataColumn ();
			dtCol.DataType = Type.GetType ("System.Int32");
			dtCol.ColumnName = "ID";
			dtCol.AutoIncrement = true;
			dtCol.Caption = "ID";
			dtCol.ReadOnly = true;
			dtCol.Unique = true;

			// Add the column to the DataColumnCollection.

			aTable.Columns.Add (dtCol);

			// Create Name column and add to the table

			dtCol = new DataColumn ();
			dtCol.DataType = Type.GetType ("System.String");
			dtCol.ColumnName = "FName";
			dtCol.AutoIncrement = false;
			dtCol.Caption = "First Name";
			dtCol.ReadOnly = false;
			dtCol.Unique = false;
			aTable.Columns.Add (dtCol);


			// Create Last Name column and add to the table.

			dtCol = new DataColumn ();
			dtCol.DataType = Type.GetType ("System.String");
			dtCol.ColumnName = "LName";
			dtCol.AutoIncrement = false;
			dtCol.Caption = "Last Name";
			dtCol.ReadOnly = false;
			dtCol.Unique = false;
			aTable.Columns.Add (dtCol);


			// Create three rows to the table
			dtRow = aTable.NewRow ();
			dtRow["ID"] = 1001;
			dtRow["FName"] = "Mahesh";
			dtRow["LName"] = "Chand";
			aTable.Rows.Add (dtRow);


			dtRow = aTable.NewRow ();
			dtRow["ID"] = 1002;
			dtRow["FName"] = "Melanie";
			dtRow["LName"] = "Talmadge";
			aTable.Rows.Add (dtRow);

			dtRow = aTable.NewRow ();
			dtRow["ID"] = 1003;
			dtRow["FName"] = "Vinay";
			dtRow["LName"] = "Bansal";
			aTable.Rows.Add (dtRow);

			aTable.PrimaryKey = new DataColumn[] { aTable.Columns["ID"] };
			return aTable;

		}

		public static DataTable SelectForPaging (int startIndex, int maxRows)
		{
			DataTable table = new DataTable ();
			table.Columns.Add ("Name", typeof (string));
			table.Columns.Add ("Number", typeof (int));
			int current;
			for (int i = 0; i < maxRows; i++) {
				current = i + startIndex;
				table.Rows.Add (new object[] { "Number" + current.ToString (), current });
			}
			return table;
		}
		

		
	}
	
}
#endif
