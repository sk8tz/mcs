//
// Tests for System.Web.UI.Control
//
// Author:
//	Peter Dennis Bartok (pbartok@novell.com)
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
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

using NUnit.Framework;
using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MonoTests.SystemWeb.Framework;

#if NET_2_0
using System.Web.UI.Adapters;
#endif

namespace MonoTests.System.Web.UI
{
	[TestFixture]
	public class ControlTest
	{
		[Test]
		public void DataBindingInterfaceTest ()
		{
			Control c;
			DataBindingCollection db;

			c = new Control ();
			Assert.AreEqual (false, ((IDataBindingsAccessor) c).HasDataBindings, "DB1");
			db = ((IDataBindingsAccessor) c).DataBindings;
			Assert.IsNotNull (db, "DB2");
			Assert.AreEqual (false, ((IDataBindingsAccessor) c).HasDataBindings, "DB3");
			db.Add (new DataBinding ("property", typeof (bool), "expression"));
			Assert.AreEqual (true, ((IDataBindingsAccessor) c).HasDataBindings);
		}

		[Test]
		public void UniqueID1 ()
		{
			// Standalone NC
			Control nc = new MyNC ();
			Assert.IsNull (nc.UniqueID, "nulltest");
		}

		[Test]
		public void UniqueID2 ()
		{
			// NC in NC
			Control nc = new MyNC ();
			Control nc2 = new MyNC ();
			nc2.Controls.Add (nc);
			Assert.IsNotNull (nc.UniqueID, "notnull");
			Assert.IsTrue (nc.UniqueID.IndexOfAny (new char[] { ':', '$' }) == -1, "separator");
		}

		[Test]
		public void UniqueID3 ()
		{
			// NC in control
			Control control = new Control ();
			Control nc = new MyNC ();

			control.Controls.Add (nc);
			Assert.IsNull (nc.UniqueID, "null");
		}

		[Test]
		public void UniqueID4 ()
		{
			// NC in control
			Control control = new Control ();
			Control nc = new MyNC ();

			nc.Controls.Add (control);
			Assert.IsNotNull (control.UniqueID, "notnull");
		}

		[Test]
		public void UniqueID5 ()
		{
			// NC in control
			Control control = new Control ();
			Control nc = new MyNC ();
			Control nc2 = new MyNC ();

			nc2.Controls.Add (nc);
			nc.Controls.Add (control);
			Assert.IsNotNull (control.UniqueID, "notnull");
			Assert.IsNull (nc2.ID, "null-1");
			Assert.IsNull (nc.ID, "null-2");
			Assert.IsTrue (-1 != control.UniqueID.IndexOfAny (new char[] { ':', '$' }), "separator");
		}

		// From bug #76919: Control uses _controls instead of
		// Controls when RenderChildren is called.
		[Test]
		public void Controls1 ()
		{
			DerivedControl derived = new DerivedControl ();
			derived.Controls.Add (new LiteralControl ("hola"));
			StringWriter sw = new StringWriter ();
			HtmlTextWriter htw = new HtmlTextWriter (sw);
			derived.RenderControl (htw);
			string result = sw.ToString ();
			Assert.AreEqual ("", result, "#01");
		}

#if NET_2_0
		[Test]
		[Category ("NotWorking")]
		public void AppRelativeTemplateSourceDirectory ()
		{
			Control ctrl = new Control ();
			Assert.AreEqual ("", ctrl.AppRelativeTemplateSourceDirectory, "AppRelativeTemplateSourceDirectory#1");
			ctrl.AppRelativeTemplateSourceDirectory = "~/Fake";
			Assert.AreEqual ("~/Fake", ctrl.AppRelativeTemplateSourceDirectory, "AppRelativeTemplateSourceDirectory#2");
		}

		[Test]
		public void ApplyStyleSheetSkin ()
		{
			Page p = new Page ();
			p.StyleSheetTheme = "";
			Control c = new Control ();
			c.ApplyStyleSheetSkin (p);
		}

		[Test]
		[Category ("NotWorking")]
		[Category ("NunitWeb")]
		public void ApplyStyleSheetSkin_1 ()
		{
			WebTest.CopyResource (GetType (), "Theme2.skin", "App_Themes/Theme2/Theme2.skin");
			WebTest t = new WebTest ();
			PageDelegates pd = new PageDelegates ();
			pd.PreInit = ApplyStyleSheetSkin_PreInit;
			pd.Load = ApplyStyleSheetSkin_Load;
			t.Invoker = new PageInvoker (pd);
			string str = t.Run ();
		}
		public static void ApplyStyleSheetSkin_PreInit (Page p)
		{
			p.Theme = "Theme2";
		}
		public static void ApplyStyleSheetSkin_Load (Page p)
		{
			Label lbl = new Label ();
			lbl.ID = "StyleLbl";
			lbl.SkinID = "red";
			lbl.Text = "StyleLabel";
			p.Controls.Add (lbl);
			lbl.ApplyStyleSheetSkin (p);
			Assert.AreEqual (Color.Red, lbl.ForeColor, "ApplyStyleSheetSkin_BackColor");
			Assert.AreEqual ("TextFromSkinFile", lbl.Text, "ApplyStyleSheetSkin");
		}

		[Test]
		[Category ("NunitWeb")]
		public void ClearChildControlState ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (ClearChildControlState_Load));
			t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls["__EVENTTARGET"].Value = "";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			t.Request = fr;
			t.Run ();
		}
		public static void ClearChildControlState_Load (Page p)
		{
			ControlWithState c1 = new ControlWithState ();
			p.Form.Controls.Add (c1);
			if (p.IsPostBack) {
				c1.ClearChildControlState ();
			}
			ControlWithState c2 = new ControlWithState ();
			c1.Controls.Add (c2);
			ControlWithState c3 = new ControlWithState ();
			c2.Controls.Add (c3);
			if (!p.IsPostBack) {
				c1.State = "State";
				c2.State = "Cool";
				c3.State = "SubCool";
			}
			else {
				Assert.AreEqual ("State", c1.State, "ControlState#1");
				Assert.AreEqual (null, c2.State, "ControlState#2");
				Assert.AreEqual (null, c3.State, "ControlState#2");
			}
		}

		[Test]
		[Category ("NunitWeb")]
		public void ClearChildState ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (ClearChildState_Load));
			t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls["__EVENTTARGET"].Value = "";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			t.Request = fr;
			t.Run ();
		}
		public static void ClearChildState_Load (Page p)
		{
			ControlWithState c1 = new ControlWithState ();
			p.Form.Controls.Add (c1);
			if (p.IsPostBack) {
				c1.ClearChildState ();
			}
			ControlWithState c2 = new ControlWithState ();
			c1.Controls.Add (c2);
			ControlWithState c3 = new ControlWithState ();
			c2.Controls.Add (c3);
			if (!p.IsPostBack) {
				c1.State = "State";
				c2.State = "Cool";
				c2.Viewstate = "Very Cool";
				c3.State = "SubCool";
				c3.Viewstate = "Super Cool";
			}
			else {
				Assert.AreEqual ("State", c1.State, "ClearChildState#1");
				Assert.AreEqual (null, c2.State, "ClearChildState#2");
				Assert.AreEqual (null, c3.State, "ClearChildState#3");
				Assert.AreEqual (null, c2.Viewstate, "ClearChildState#4");
				Assert.AreEqual (null, c3.Viewstate, "ClearChildState#5");
			}
		}

		[Test]
		public void DataBind ()
		{
			MyNC ctrl = new MyNC ();
			ctrl.DataBinding += new EventHandler (ctrl_DataBinding);
			Assert.AreEqual (false, _eventDataBinding, "Before DataBinding");
			ctrl.DataBind (false);
			Assert.AreEqual (false, _eventDataBinding, "Before DataBinding");
			ctrl.DataBind (true);
			Assert.AreEqual (true, _eventDataBinding, "After DataBinding");
		}
		bool _eventDataBinding;
		void ctrl_DataBinding (object sender, EventArgs e)
		{
			_eventDataBinding = true;
		}

		[Test]
		public void DataBindChildren ()
		{
			MyNC ctrl1 = new MyNC ();
			Control ctrl2 = new Control ();
			Control ctrl3 = new Control ();
			ctrl2.DataBinding += new EventHandler (ctrl2_DataBinding);
			ctrl3.DataBinding += new EventHandler (ctrl3_DataBinding);

			ctrl2.Controls.Add (ctrl3);
			ctrl1.Controls.Add (ctrl2);
			Assert.AreEqual (false, _eventChild1, "Before DataBinding#1");
			Assert.AreEqual (false, _eventChild2, "Before DataBinding#2");
			ctrl1.DataBindChildren ();
			Assert.AreEqual (true, _eventChild1, "After DataBinding#1");
			Assert.AreEqual (true, _eventChild2, "After DataBinding#2");
		}
		bool _eventChild1;
		bool _eventChild2;
		void ctrl3_DataBinding (object sender, EventArgs e)
		{
			_eventChild1 = true;
		}
		void ctrl2_DataBinding (object sender, EventArgs e)
		{
			_eventChild2 = true;
		}

		[Test]
		[Category ("NotWorking")] // Not implemented exception
		public void EnsureID ()
		{
			MyNC ctrl = new MyNC ();
			MyNC ctrl1 = new MyNC ();
			ctrl.Controls.Add (ctrl1);
			Page p = new Page ();
			p.Controls.Add (ctrl);
			ctrl.EnsureID ();
			if (ctrl.ID == string.Empty)
				Assert.Fail ("EnsureID#1");
			if (ctrl1.ID == string.Empty)
				Assert.Fail ("EnsureID#2");
		}

		[Test]
		public void Focus ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (Focus_Load));
			string html = t.Run ();
			Assert.AreEqual (3, contain (html, "TestBox"), "Focus script not created");

		}
		public static void Focus_Load (Page p)
		{
			TextBox tbx = new TextBox ();
			tbx.ID = "TestBox";
			p.Controls.Add (tbx);
			tbx.Focus ();
		}
		int contain (string orig, string compare)
		{
			if (orig.IndexOf (compare) == -1)
				return 0;
			return 1 + contain (orig.Substring (orig.IndexOf (compare) + compare.Length), compare);
		}

		[Test]
		[Category ("NotWorking")] // Not implemented exception
		public void HasEvent ()
		{
			MyNC ctrl1 = new MyNC ();
			Assert.AreEqual (false, ctrl1.HasEvents (), "HasEvent#1");
			EventHandler ctrl_hdlr = new EventHandler (ctrl1_Init);
			ctrl1.Init += new EventHandler (ctrl1_Init);
			ctrl1.Init += ctrl_hdlr;
			Assert.AreEqual (true, ctrl1.HasEvents (), "HasEvent#2");
			// Dosn't work than removed handler
			//ctrl1.Init -= ctrl_hdlr;
			//Assert.AreEqual (false, ctrl1.HasEvents (), "HasEvent#3");
		}
		void ctrl1_Init (object sender, EventArgs e)
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		[Test]
		[Category ("NotWorking")]
		public void IsViewStateEnabled ()
		{
			DerivedControl c = new DerivedControl ();
			Assert.IsTrue (c.DoIsViewStateEnabled);
			Page p = new Page ();
			c.Page = p;
			p.Controls.Add (c);
			Assert.IsTrue (c.DoIsViewStateEnabled);
			p.EnableViewState = false;
			Assert.IsFalse (c.DoIsViewStateEnabled);
		}

		[Test]
		[Category ("NunitWeb")]
		public void ControlState ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (ControlState_Load));
			t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls["__EVENTTARGET"].Value = "";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			t.Request = fr;
			t.Run ();
		}
		public static void ControlState_Load (Page p)
		{
			ControlWithState c1 = new ControlWithState ();
			ControlWithState c2 = new ControlWithState ();
			c1.Controls.Add (c2);
			p.Form.Controls.Add (c1);
			if (!p.IsPostBack) {
				c1.State = "State";
				c2.State = "Cool";
			}
			else {
				ControlWithState c3 = new ControlWithState ();
				p.Form.Controls.Add (c3);
				Assert.AreEqual ("State", c1.State, "ControlState");
				Assert.AreEqual ("Cool", c2.State, "ControlState");
			}
		}

		[Test]
		[Category ("NunitWeb")]
		public void ControlState2 () {
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (ControlState2_Load));
			t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls ["__EVENTTARGET"].Value = "";
			fr.Controls ["__EVENTARGUMENT"].Value = "";
			t.Request = fr;
			t.Run ();
			
			fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls ["__EVENTTARGET"].Value = "";
			fr.Controls ["__EVENTARGUMENT"].Value = "";
			t.Request = fr;
			t.Run ();
		}
		public static void ControlState2_Load (Page p) {
			ControlWithState parent = new ControlWithState ();
			p.Form.Controls.Add (parent);
			if (!p.IsPostBack) {
				// emulate DataBind
				parent.Controls.Clear ();
				parent.ClearChildControlState ();
				ControlWithState c1 = new ControlWithState ();
				ControlWithState c2 = new ControlWithState ();
				parent.Controls.Add (c1);
				parent.Controls.Add (c2);
				c1.State = "State1_1";
				c2.State = "State1_2";
				parent.State = "First";
			}
			else if (parent.State == "First") {
				// emulate DataBind
				parent.Controls.Clear ();
				parent.ClearChildControlState ();
				ControlWithState c1 = new ControlWithState ();
				ControlWithState c2 = new ControlWithState ();
				parent.Controls.Add (c1);
				parent.Controls.Add (c2);
				c1.State = "State2_1";
				c2.State = "State2_2";
				parent.State = "Second";
			}
			else {
				// emulate CrerateChildControl only
				parent.Controls.Clear ();
				ControlWithState c1 = new ControlWithState ();
				ControlWithState c2 = new ControlWithState ();
				parent.Controls.Add (c1);
				parent.Controls.Add (c2);

				Assert.AreEqual ("State2_1", c1.State, "ControlState#1");
				Assert.AreEqual ("State2_2", c2.State, "ControlState#2");
			}
		}

		[Test]
		public void ClientIDSeparator ()
		{
			DerivedControl ctrl = new DerivedControl ();
			Assert.AreEqual (95, (int) ctrl.ClientIDSeparator, "ClientIDSeparator");
		}

		[Test]
		public void IDSeparator ()
		{
			DerivedControl ctrl = new DerivedControl ();
			Assert.AreEqual (36, (int) ctrl.IdSeparator, "IDSeparator");
		}

		[Test]
		[Category ("NunitWeb")]
		public void IsChildControlStateCleared ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (IsChildControlStateCleared_Load));
			t.Run ();
			FormRequest fr = new FormRequest (t.Response, "form1");
			fr.Controls.Add ("__EVENTTARGET");
			fr.Controls.Add ("__EVENTARGUMENT");
			fr.Controls["__EVENTTARGET"].Value = "";
			fr.Controls["__EVENTARGUMENT"].Value = "";
			t.Request = fr;
			t.Run ();
		}
		public static void IsChildControlStateCleared_Load (Page p)
		{
			ControlWithState c1 = new ControlWithState ();
			p.Form.Controls.Add (c1);
			if (p.IsPostBack) {
				Assert.IsFalse (c1.IsChildControlStateCleared, "ControlState#1");
				c1.ClearChildControlState ();
				Assert.IsTrue (c1.IsChildControlStateCleared, "ControlState#1");
			}
			ControlWithState c2 = new ControlWithState ();
			c1.Controls.Add (c2);
			ControlWithState c3 = new ControlWithState ();
			c2.Controls.Add (c3);
			if (p.IsPostBack) {
				Assert.IsFalse (c2.IsChildControlStateCleared, "ControlState#1");
				Assert.IsFalse (c3.IsChildControlStateCleared, "ControlState#1");
			}
			if (!p.IsPostBack) {
				c1.State = "State";
				c2.State = "Cool";
				c3.State = "SubCool";
			}
		}

		[Test]
		[Category ("NunitWeb")]
		public void LoadViewStateByID ()
		{
			ControlWithState c1 = new ControlWithState ();
			ControlWithState c2 = new ControlWithState ();
			c1.Controls.Add (c2);
			Assert.AreEqual (false, c1.LoadViewStateByID, "LoadViewStateByID#1");
		}

		[Test]
		[Category ("NunitWeb")]
		public void OpenFile ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (OpenFile_Load));
			t.Run ();
		}
		public static void OpenFile_Load (Page p)
		{
			DerivedControl ctrl = new DerivedControl ();
			Stream strem = ctrl.OpenFile ("~/MyPage.aspx");
			Assert.IsNotNull (strem, "OpenFile failed");
		}

		[Test]
		[Category ("NunitWeb")]
		[ExpectedException (typeof (FileNotFoundException))]
		public void OpenFile_Exception ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (OpenFileException_Load));
			t.Run ();
		}
		public static void OpenFileException_Load (Page p)
		{
			DerivedControl ctrl = new DerivedControl ();
			Stream strem = ctrl.OpenFile ("~/Fake.tmp");
		}

		//// MonoTests.SystemWeb.Framework limitation for Add_browsers - directory include in project
		//[Test]
		//[Category ("NunitWeb")]
		//public void ResolveAdapter_1 ()
		//{
		//        WebTest.CopyResource (GetType (), "adapters.browser", "App_Browsers/adapters.browser");
		//        WebTest t = new WebTest (PageInvoker.CreateOnInit (ResolveAdapter_Init));
		//        string html = t.Run ();
		//}
		//public static void ResolveAdapter_Init (Page p)
		//{
		//        Customadaptercontrol ctrl = new Customadaptercontrol ();
		//        p.Controls.Add (ctrl);
		//        ctrl.Load += new EventHandler (ctrl_Load);
		//}
		//static void ctrl_Load (object sender, EventArgs e)
		//{
		//        Assert.IsNotNull (((Customadaptercontrol) sender).ResolveAdapter (), "ResolveAdapter Failed#1");
		//        Assert.AreEqual ("Customadapter", ((Customadaptercontrol) sender).ResolveAdapter ().ToString (), "ResolveAdapter Failed#2");
		//}

		[Test]
		[Category ("NunitWeb")]
		public void ResolveClientUrl ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (ResolveClientUrl_Load));
			string html = t.Run ();
		}
		public static void ResolveClientUrl_Load (Page p)
		{
			Control ctrl = new Control ();
			p.Controls.Add (ctrl);
			string url = ctrl.ResolveClientUrl ("~/MyPage.aspx");
			Assert.AreEqual ("MyPage.aspx", url, "ResolveClientUrl Failed");
		}

		[Test]
		[Category ("NotWorking")] // Not implemented exception
		public void ResolveAdapter_2 ()
		{
			DerivedControl ctrl = new DerivedControl ();
			Assert.AreEqual (null, ctrl.ResolveAdapter (), "ResolveAdapter");
		}

		[Test]
		public void EnableTheming ()
		{
			DerivedControl ctrl = new DerivedControl ();
			Assert.AreEqual (true, ctrl.EnableTheming, "EnableTheming#1");
			ctrl.EnableTheming = false;
			Assert.AreEqual (false, ctrl.EnableTheming, "EnableTheming#2");
		}

#endif
		[Test]
		public void BindingContainer ()
		{
			ControlWithTemplate c = new ControlWithTemplate ();
			c.Template = new CompiledTemplateBuilder (new BuildTemplateMethod (BindingContainer_BuildTemplate));

			// cause CreateChildControls called
			c.FindControl ("stam");
		}
		static void BindingContainer_BuildTemplate (Control control)
		{
			Control child1 = new Control ();
			control.Controls.Add (child1);

			Assert.IsTrue (Object.ReferenceEquals (child1.NamingContainer, control), "NamingContainer #1");
			Assert.IsTrue (Object.ReferenceEquals (child1.BindingContainer, control), "BindingContainer #1");

			NamingContainer nc = new NamingContainer ();
			Control child2 = new Control ();
			nc.Controls.Add (child2);
			control.Controls.Add (nc);

			Assert.IsTrue (Object.ReferenceEquals (child2.NamingContainer, nc), "NamingContainer #2");
			Assert.IsTrue (Object.ReferenceEquals (child2.BindingContainer, nc), "BindingContainer #2");

#if NET_2_0
			// DetailsViewPagerRow marked to be not BindingContainer 
			DetailsViewPagerRow row = new DetailsViewPagerRow (0, DataControlRowType.Pager, DataControlRowState.Normal);
			TableCell cell = new TableCell ();
			Control child3 = new Control ();
			cell.Controls.Add (child3);
			row.Cells.Add (cell);
			control.Controls.Add (row);

			Assert.IsTrue (Object.ReferenceEquals (child3.NamingContainer, row), "NamingContainer #3");
			Assert.IsTrue (Object.ReferenceEquals (child3.BindingContainer, control), "BindingContainer #3");
#endif
		}
#if NET_2_0
		[Test]
		public void Contorl_Adapter ()
		{
			MyNC ctr = new MyNC ();
			Assert.AreEqual (null, ctr.Adapter (), "Adapter");
		}
#endif
#if NET_2_0
		[TestFixtureTearDown]
		public void Tear_down ()
		{
			WebTest.Unload ();
		}
#endif

		#region helpcalsses
#if NET_2_0
		class ControlWithState : Control
		{
			string _state;

			public string State
			{
				get { return _state; }
				set { _state = value; }
			}

			public string Viewstate {
				get { return (string) ViewState ["Viewstate"]; }
				set { ViewState ["Viewstate"] = value; }
			}

			protected override void OnInit (EventArgs e)
			{
				base.OnInit (e);
				Page.RegisterRequiresControlState (this);
			}

			protected override object SaveControlState ()
			{
				return State;
			}

			protected override void LoadControlState (object savedState)
			{
				State = (string) savedState;
			}

			public new void ClearChildState ()
			{
				base.ClearChildState ();
			}

			public new void ClearChildControlState ()
			{
				base.ClearChildControlState ();
			}

			public new bool IsChildControlStateCleared
			{
				get { return base.IsChildControlStateCleared; }
			}

			public new bool LoadViewStateByID
			{
				get { return base.LoadViewStateByID; }
			}
		}

#endif
		class MyNC : Control, INamingContainer
		{
			#if NET_2_0
			public ControlAdapter Adapter ()
			{
				return base.Adapter;
			}

			public new void DataBind (bool opt)
			{
				base.DataBind (opt);
			}

			public new void DataBindChildren ()
			{
				base.DataBindChildren ();
			}

			public new void EnsureID ()
			{
				base.EnsureID ();
			}

			public new bool HasEvents ()
			{
				return base.HasEvents ();
			}
			#endif
		}

		class DerivedControl : Control
		{
			ControlCollection coll;

			public DerivedControl ()
			{
				coll = new ControlCollection (this);
			}

			public override ControlCollection Controls
			{
				get { return coll; }
			}

#if NET_2_0
			public bool DoIsViewStateEnabled
			{
				get { return IsViewStateEnabled; }
			}

			public new char ClientIDSeparator
			{
				get { return base.ClientIDSeparator; }
			}

			public new char IdSeparator
			{
				get { return base.IdSeparator; }
			}

			public new Stream OpenFile (string path)
			{
				return base.OpenFile (path);
			}

			public new ControlAdapter ResolveAdapter ()
			{
				return base.ResolveAdapter ();
			}
#endif
		}

		class NamingContainer : Control, INamingContainer
		{
		}

		class ControlWithTemplate : Control
		{
			ITemplate _template;

			[TemplateContainer (typeof (TemplateContainer))]
			public ITemplate Template
			{
				get { return _template; }
				set { _template = value; }
			}

			protected override void CreateChildControls ()
			{
				Controls.Clear ();

				TemplateContainer container = new TemplateContainer ();
				Controls.Add (container);

				if (Template != null)
					Template.InstantiateIn (container);
			}
		}

		class TemplateContainer : Control, INamingContainer
		{
		}
		#endregion
	}

#if NET_2_0
	public class Customadaptercontrol : Control
	{
		public new ControlAdapter ResolveAdapter ()
		{
			return base.ResolveAdapter ();
		}
	}

	public class Customadapter : ControlAdapter
	{
	}
#endif
}



