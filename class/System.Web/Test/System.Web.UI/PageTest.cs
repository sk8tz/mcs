//
// Tests for System.Web.UI.Page
//
// Authors:
//	Peter Dennis Bartok (pbartok@novell.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
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
using System.IO;
using System.Threading;
using System.Security.Principal;
using System.Web;
using System.Web.UI;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace MonoTests.System.Web.UI {

	class TestPage : Page {

		private HttpContext ctx;

		// don't call base class (so _context is never set to a non-null value)
		protected override HttpContext Context {
			get {
				if (ctx == null) {
					ctx = new HttpContext (null);
					ctx.User = new GenericPrincipal (new GenericIdentity ("me"), null);
				}
				return ctx;
			}
		}
	}

	class TestPage2 : Page {

		private HttpContext ctx;

		// don't call base class (so _context is never set to a non-null value)
		protected override HttpContext Context {
			get {
				if (ctx == null) {
					ctx = new HttpContext (
						new HttpRequest (String.Empty, "http://www.mono-project.com", String.Empty),
						new HttpResponse (new StringWriter ())
						);
				}
				return ctx;
			}
		}

		public HttpContext HttpContext {
			get { return Context; }
		}
	}

	[TestFixture]	
	public class PageTest {

		[Test]
		[ExpectedException (typeof(HttpException))]
		public void RequestExceptionTest ()
		{
			Page p;
			HttpRequest r;

			p = new Page ();
			r = p.Request;
		}

		[Test]
#if NET_2_0
		[Category ("NotDotNet")] // page.User throw NRE in 2.0 RC
		
#endif
		public void User_OverridenContext ()
		{
			TestPage page = new TestPage ();
			Assert.AreEqual ("me", page.User.Identity.Name, "User");
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void Request_OverridenContext ()
		{
			TestPage2 page = new TestPage2 ();
			Assert.IsNotNull (page.Request, "Request");
			// it doesn't seems to access the context via the virtual property
		}

		[Test]
		public void Request_OverridenContext_Indirect ()
		{
			TestPage2 page = new TestPage2 ();
			Assert.IsNotNull (page.HttpContext.Request, "Request");
		}

#if NET_2_0
		[Test]
		[Category ("NunitWeb")]
		public void PageHeaderOnPreInit ()
		{
			Thread.Sleep (200); 
			PageDelegate pd = new PageDelegate (Page_OnPreInit);
			WebTest t = new WebTest (PageInvoker.CreateOnPreInit (pd));
			string html = t.Run ();
			string newHtml = html.Substring (html.IndexOf ("<head"), (html.IndexOf ("<body") - html.IndexOf ("<head")));
			string origHtml = @" <head id=""Head1""><title>
	                                PreInit
                                    </title></head>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "HeaderRenderInit");
			Thread.Sleep (200); 
			WebTest.Unload ();
		}

		public static void Page_OnPreInit (Page p)
		{
			Assert.AreEqual (null, p.Header, "HeaderOnPreInit");
			p.Title = "PreInit";
		}

		[Test]
		[Category ("NunitWeb")]
		public void PageHeaderInit ()
		{
			Thread.Sleep (200); 
			PageDelegate pd = new PageDelegate (CheckHeader);
			WebTest t = new WebTest (PageInvoker.CreateOnInit (pd));
			string html = t.Run ();
			string newHtml = html.Substring (html.IndexOf ("<head"), (html.IndexOf ("<body") - html.IndexOf ("<head")));
			string origHtml = @" <head id=""Head1""><title>
	                            Test
                                </title></head>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "HeaderRenderInit");
			Thread.Sleep (200); 
			WebTest.Unload ();

		}

		[Test]
		[Category ("NunitWeb")]
		public void PageHeaderInitComplete ()
		{
			Thread.Sleep (200); 
			WebTest t = new WebTest ();
			PageDelegates pd = new PageDelegates ();
			pd.InitComplete = CheckHeader;
			t.Invoker = new PageInvoker (pd);
			string html = t.Run ();
			string newHtml = html.Substring (html.IndexOf ("<head"), (html.IndexOf ("<body") - html.IndexOf ("<head")));
			string origHtml = @" <head id=""Head1""><title>
	                            Test
                                </title></head>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "HeaderRenderInitComplete");
			Thread.Sleep (200); 
			WebTest.Unload ();
		}

		[Test]
		[Category ("NunitWeb")]
		public void PageHeaderPreLoad ()
		{
			Thread.Sleep (200); 
			WebTest t = new WebTest ();
			PageDelegates pd = new PageDelegates ();
			pd.PreLoad = CheckHeader;
			t.Invoker = new PageInvoker (pd);
			string html = t.Run ();
			string newHtml = html.Substring (html.IndexOf ("<head"), (html.IndexOf ("<body") - html.IndexOf ("<head")));
			string origHtml = @" <head id=""Head1""><title>
	                            Test
                                </title></head>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "HeaderRenderPreLoad");
			Thread.Sleep (200); 
			WebTest.Unload ();
		}

		[Test]
		[Category ("NunitWeb")]
		public void PageHeaderLoad ()
		{
			Thread.Sleep (200); 
			PageDelegate pd = new PageDelegate (CheckHeader);
			WebTest t = new WebTest (PageInvoker.CreateOnLoad (pd));
			string html = t.Run ();
			string newHtml = html.Substring (html.IndexOf ("<head"), (html.IndexOf ("<body") - html.IndexOf ("<head")));
			string origHtml = @" <head id=""Head1""><title>
	                            Test
                                </title></head>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "HeaderRenderLoad");
			Thread.Sleep (200); 
			WebTest.Unload ();
		}

		[Test]
		[Category ("NunitWeb")]
		public void PageHeaderLoadComplete ()
		{
			Thread.Sleep (200); 
			WebTest t = new WebTest ();
			PageDelegates pd = new PageDelegates ();
			pd.LoadComplete = CheckHeader;
			t.Invoker = new PageInvoker (pd);
			string html = t.Run ();
			string newHtml = html.Substring (html.IndexOf ("<head"), (html.IndexOf ("<body") - html.IndexOf ("<head")));
			string origHtml = @" <head id=""Head1""><title>
	                            Test
                                </title></head>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "HeaderRenderLoadComplete");
			Thread.Sleep (200); 
			WebTest.Unload ();
		}

		[Test]
		[Category ("NunitWeb")]
		public void PageHeaderPreRender ()
		{
			Thread.Sleep (200);
			WebTest t = new WebTest ();
			PageDelegates pd = new PageDelegates ();
			pd.PreRender = CheckHeader;
			t.Invoker = new PageInvoker (pd);
			string html = t.Run ();
			string newHtml = html.Substring (html.IndexOf ("<head"), (html.IndexOf ("<body") - html.IndexOf ("<head")));
			string origHtml = @" <head id=""Head1""><title>
	                            Test
                                </title></head>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "HeaderRenderPreRender");
			Thread.Sleep (200); 
			WebTest.Unload ();
		}

		[Test]
		[Category ("NunitWeb")]
		public void PageHeaderPreRenderComplete ()
		{
			Thread.Sleep (200); 
			WebTest t = new WebTest ();
			PageDelegates pd = new PageDelegates ();
			pd.PreRenderComplete = CheckHeader;
			t.Invoker = new PageInvoker (pd);
			string html = t.Run ();
			string newHtml = html.Substring (html.IndexOf ("<head"), (html.IndexOf ("<body") - html.IndexOf ("<head")));
			string origHtml = @" <head id=""Head1""><title>
	                            Test
                                </title></head>";
			HtmlDiff.AssertAreEqual (origHtml, newHtml, "HeaderRenderPreRenderComplete");
			Thread.Sleep (200); 
			WebTest.Unload ();
		}



		public static void CheckHeader (Page p)
		{
			Assert.AreEqual ("Untitled Page", p.Title, "CheckHeader#1");
			Assert.AreEqual ("Untitled Page", p.Header.Title, "CheckHeader#2");
			p.Title = "Test0";
			Assert.AreEqual ("Test0", p.Header.Title, "CheckHeader#3");
			p.Header.Title = "Test";
			Assert.AreEqual ("Test", p.Title, "CheckHeader#4");
		}     
#endif




#if NET_2_0
		[Test]
		[Category ("NunitWeb")]
		public void Page_ValidationGroup () {
			new WebTest (PageInvoker.CreateOnLoad (Page_ValidationGroup_Load)).Run ();
		}

		public static void Page_ValidationGroup_Load (Page page) {
			TextBox textbox;
			BaseValidator val;

			textbox = new TextBox ();
			textbox.ID = "T1";
			textbox.ValidationGroup = "VG1";
			page.Form.Controls.Add (textbox);
			val = new RequiredFieldValidator ();
			val.ControlToValidate = "T1";
			val.ValidationGroup = "VG1";
			page.Form.Controls.Add (val);

			textbox = new TextBox ();
			textbox.ID = "T2";
			textbox.ValidationGroup = "VG2";
			page.Form.Controls.Add (textbox);
			val = new RequiredFieldValidator ();
			val.ControlToValidate = "T2";
			val.ValidationGroup = "VG2";
			page.Form.Controls.Add (val);

			textbox = new TextBox ();
			textbox.ID = "T3";
			page.Form.Controls.Add (textbox);
			val = new RequiredFieldValidator ();
			val.ControlToValidate = "T3";
			page.Form.Controls.Add (val);

			Assert.AreEqual (3, page.Validators.Count, "Page_ValidationGroup#1");
			Assert.AreEqual (1, page.GetValidators ("").Count, "Page_ValidationGroup#2");
			Assert.AreEqual (1, page.GetValidators (null).Count, "Page_ValidationGroup#3");
			Assert.AreEqual (0, page.GetValidators ("Fake").Count, "Page_ValidationGroup#4");
			Assert.AreEqual (1, page.GetValidators ("VG1").Count, "Page_ValidationGroup#5");
			Assert.AreEqual (1, page.GetValidators ("VG2").Count, "Page_ValidationGroup#6");
		}
#endif

	}
	
}
