//
// Tests for System.Web.UI.WebControls.ThemeTest.cs
//
// Author:
//	Yoni Klein (yonik@mainsoft.com)
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

// Additional resources :
// PageWithStyleSheet.aspx; PageWithStyleSheet.aspx.cs;RunTimeSetTheme.aspx;
// RunTimeSetTheme.aspx.cs; PageWithTheme.aspx; PageWithTheme.aspx.cs; Theme1.skin

#if NET_2_0

using System;
using System.Drawing;
using System.IO;
using System.Configuration;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;
using MyWebControl = System.Web.UI.WebControls;
using System.Reflection;
using NUnit.Framework;
using NunitWeb;

namespace MonoTests.System.Web.UI.WebControls
{
	[Serializable]
	[TestFixture]
	public class ThemeTest
	{	
		[TestFixtureSetUp]
		public void Set_Up ()
		{
#if VISUAL_STUDIO
			Helper.Instance.CopyResource (Assembly.GetExecutingAssembly (), "Test1.Resources.Theme1.skin", "App_Themes/Theme1/Theme1.skin");
			Helper.Instance.CopyResource (Assembly.GetExecutingAssembly (), "Test1.Resources.PageWithStyleSheet.aspx", "PageWithStyleSheet.aspx");
			Helper.Instance.CopyResource (Assembly.GetExecutingAssembly (), "Test1.Resources.PageWithTheme.aspx", "PageWithTheme.aspx");
			Helper.Instance.CopyResource (Assembly.GetExecutingAssembly (), "Test1.Resources.RunTimeSetTheme.aspx", "RunTimeSetTheme.aspx");
#else
			Helper.Instance.CopyResource (Assembly.GetExecutingAssembly (), "Theme1.skin", "App_Themes/Theme1/Theme1.skin");
			Helper.Instance.CopyResource (Assembly.GetExecutingAssembly (), "PageWithStyleSheet.aspx", "PageWithStyleSheet.aspx");
			Helper.Instance.CopyResource (Assembly.GetExecutingAssembly (), "PageWithTheme.aspx", "PageWithTheme.aspx");
			Helper.Instance.CopyResource (Assembly.GetExecutingAssembly (), "RunTimeSetTheme.aspx", "RunTimeSetTheme.aspx");
#endif
		}


		[SetUp]
		public void SetupTestCase ()
		{
			Thread.Sleep (100);
		}
		
		//Run on page with theme

		[Test]
		[Category ("NunitWeb")]
		public void Theme_TestLabelTheme ()
		{
			Helper.Instance.RunUrl ("PageWithTheme.aspx", RenderLabelTest);
		}

		public static void RenderLabelTest (HttpContext c, Page p, object param)
		{
			Assert.AreEqual (Color.Black,((MyWebControl.Label) p.FindControl ("Label")).BackColor, "Default Theme#1");
			Assert.AreEqual (Color.Red, ((MyWebControl.Label) p.FindControl ("LabelRed")).BackColor, "Red Skin Theme#2");
			Assert.AreEqual (Color.Yellow, ((MyWebControl.Label) p.FindControl ("LabelYellow")).BackColor, "Yellow Skin Theme#3");
			Assert.AreEqual (Color.Black, ((MyWebControl.Label) p.FindControl ("LabelOverride")).BackColor, "Override Skin Theme#4");
		}

		[Test]
		[Category ("NunitWeb")]
		public void Theme_TestImageTheme ()
		{
			Helper.Instance.RunUrl ("PageWithTheme.aspx", RenderImageTest);
		}

		public static void RenderImageTest (HttpContext c, Page p, object param)
		{
			Assert.IsTrue (((MyWebControl.Image) p.FindControl ("Image")).ImageUrl.IndexOf ("myimageurl") >= 0, "Default Theme#1");
			Assert.IsTrue (((MyWebControl.Image) p.FindControl ("ImageRed")).ImageUrl.IndexOf ("myredimageurl") >= 0, "RedImage Theme#2");
			Assert.IsTrue (((MyWebControl.Image) p.FindControl ("ImageYellow")).ImageUrl.IndexOf ("myyellowimageurl") >= 0, "YellowImage Theme#3");
			Assert.IsTrue (((MyWebControl.Image) p.FindControl ("ImageOverride")).ImageUrl.IndexOf ("myimageurl") >= 0, "OverrideImage Theme#3");
		}

		// Run on page with StyleSheet

		[Test]
		[Category ("NunitWeb")]
		public void Theme_TestLabelStyleSheet ()
		{
			Helper.Instance.RunUrl ("PageWithStyleSheet.aspx", StyleSheetRenderLabelTest);
		}

		public static void StyleSheetRenderLabelTest (HttpContext c, Page p, object param)
		{
			Assert.AreEqual (Color.Black, ((MyWebControl.Label) p.FindControl ("Label")).BackColor, "Default Theme#1");
			Assert.AreEqual (Color.Red, ((MyWebControl.Label) p.FindControl ("LabelRed")).BackColor, "Red Skin Theme#2");
			Assert.AreEqual (Color.Yellow, ((MyWebControl.Label) p.FindControl ("LabelYellow")).BackColor, "Yellow Skin Theme#3");
			Assert.AreEqual (Color.White, ((MyWebControl.Label) p.FindControl ("LabelOverride")).BackColor, "Override Skin Theme#4");
		}

		[Test]
		[Category ("NunitWeb")]
		public void Theme_TestImageStyleSheet ()
		{
			Helper.Instance.RunUrl ("PageWithStyleSheet.aspx", StyleSheetRenderImageTest);
		}

		public static void StyleSheetRenderImageTest (HttpContext c, Page p, object param)
		{
			Assert.IsTrue (((MyWebControl.Image) p.FindControl ("Image")).ImageUrl.IndexOf ("myimageurl") >= 0, "Default Theme#1");
			Assert.IsTrue (((MyWebControl.Image) p.FindControl ("ImageRed")).ImageUrl.IndexOf ("myredimageurl") >= 0, "RedImage Theme#2");
			Assert.IsTrue (((MyWebControl.Image) p.FindControl ("ImageYellow")).ImageUrl.IndexOf ("myyellowimageurl") >= 0, "YellowImage Theme#3");
			Assert.IsTrue (((MyWebControl.Image) p.FindControl ("ImageOverride")).ImageUrl.IndexOf ("overridedurl") >= 0, "OverrideImage Theme#3");
		}

		[Test]
		[Category ("NunitWeb")]
		public void Theme_TestRuntimeSetTheme ()
		{
			PageDelegates p = new PageDelegates ();
			p.PreInit = RuntimeSetThemePreInit;
			p.Load = RuntimeSetThemeLoad;
			Helper.Instance.RunUrlDelegates ("RunTimeSetTheme.aspx", p);
		}

		public static void RuntimeSetThemePreInit (HttpContext c, Page p, object param)
		{
			p.Theme = "Theme1";
		}

		public static void RuntimeSetThemeLoad (HttpContext c, Page p, object param)
		{
			Assert.AreEqual (Color.Black, ((MyWebControl.Label) p.FindControl ("Label")).BackColor, "Default Theme#1");
			Assert.AreEqual (Color.Red, ((MyWebControl.Label) p.FindControl ("LabelRed")).BackColor, "Red Skin Theme#2");
			Assert.AreEqual (Color.Yellow, ((MyWebControl.Label) p.FindControl ("LabelYellow")).BackColor, "Yellow Skin Theme#3");
			Assert.AreEqual (Color.Black, ((MyWebControl.Label) p.FindControl ("LabelOverride")).BackColor, "Override Skin Theme#4");
			Assert.IsTrue (((MyWebControl.Image) p.FindControl ("Image")).ImageUrl.IndexOf ("myimageurl") >= 0, "Default Theme#1");
			Assert.IsTrue (((MyWebControl.Image) p.FindControl ("ImageRed")).ImageUrl.IndexOf ("myredimageurl") >= 0, "RedImage Theme#2");
			Assert.IsTrue (((MyWebControl.Image) p.FindControl ("ImageYellow")).ImageUrl.IndexOf ("myyellowimageurl") >= 0, "YellowImage Theme#3");
			Assert.IsTrue (((MyWebControl.Image) p.FindControl ("ImageOverride")).ImageUrl.IndexOf ("myimageurl") >= 0, "OverrideImage Theme#3");
		}

		[Test]
		[Category ("NunitWeb")]
		public void Theme_TestThemeNotExistExeption()
		{
			string page =	Helper.Instance.RunInPagePreInit (_ThemeNotExistException);
			Assert.IsTrue (page.IndexOf("System.Web.HttpException") >= 0, "System.Web.HttpException was expected, actual result: "+page);
		}

		public static  void _ThemeNotExistException (HttpContext c, Page p, object param)
		{
			p.Theme = "NotExistTheme";
		}

		[Test]
		[Category ("NunitWeb")]
		public void Theme_SetThemeException ()
		{
			string page=Helper.Instance.RunInPagePreInit (SetThemeExeption);
			Assert.IsTrue (page.IndexOf("System.Web.HttpException") >= 0, "System.Web.HttpException was expected, actual result: "+page);
		}

		//// Delegate running on Page Load , only before PreInit possible set Theme on running time !
		//[Test]
		//[Category ("NunitWeb")]
		////Use Assert.Fail to print the actual result
		////[ExpectedException (typeof (InvalidOperationException))]
		//[Category ("NotWorking")]
		//public void Theme_SetThemeException ()
		//{
		//        try {
		//                string res=Helper.Instance.RunInPagePreInit (SetThemeExeption);
		//                Assert.Fail ("InvalidOperationException was expected. Result: "+res); 
		//        }
		//        catch (InvalidOperationException e) {
		//                //swallow the expected exception
		//        }
		//}

		public static void SetThemeExeption (HttpContext c, Page p, object param)
		{
			p.Theme = "InvalidTheme1";
		}

		[TestFixtureTearDown]
		public void TearDown ()
		{
			Thread.Sleep (100);
			Helper.Unload ();
			Thread.Sleep (100);
		}
	}
}
#endif
