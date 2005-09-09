//
// HtmlInputButtonTest.cs
//	- Unit tests for System.Web.UI.HtmlControls.HtmlInputButton
//
// Author:
//	Jackson Harper	(jackson@ximian.com)
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

using System;
using System.IO;
using System.Web.UI;
using System.Web.UI.HtmlControls;

using NUnit.Framework;

namespace MonoTests.System.Web.UI.HtmlControls {

	public class HtmlInputButtonPoker : HtmlInputButton {

		public HtmlInputButtonPoker ()
		{
			TrackViewState ();
		}

		public object SaveState ()
		{
			return SaveViewState ();
		}

		public void LoadState (object state)
		{
			LoadViewState (state);
		}

		public void DoRenderAttributes (HtmlTextWriter writer)
		{
			RenderAttributes (writer);
		}
	}

	[TestFixture]
	public class HtmlInputButtonTest {

		[Test]
		public void Defaults ()
		{
			HtmlInputButtonPoker p = new HtmlInputButtonPoker ();

			Assert.IsTrue (p.CausesValidation, "A1");
		}

		[Test]
		public void CleanProperties ()
		{
			HtmlInputButtonPoker p = new HtmlInputButtonPoker ();

			p.CausesValidation = false;
			Assert.IsFalse (p.CausesValidation, "A1");

			p.CausesValidation = true;
			Assert.IsTrue (p.CausesValidation, "A2");

			p.CausesValidation = false;
			Assert.IsFalse (p.CausesValidation, "A3");
		}

		[Test]
		public void ViewState ()
		{
			HtmlInputButtonPoker p = new HtmlInputButtonPoker ();
#if NET_2_0
			p.CausesValidation = false;
			p.ValidationGroup = "VG";
#endif
			object s = p.SaveState();
			HtmlInputButtonPoker copy = new HtmlInputButtonPoker ();
			copy.LoadState (s);

#if NET_2_0
			Assert.IsFalse (copy.CausesValidation, "A1");
			Assert.AreEqual ("VG", p.ValidationGroup, "A2");
#endif
		}

		[Test]
		public void RenderAttributes ()
		{
			StringWriter sw = new StringWriter ();
			HtmlTextWriter tw = new HtmlTextWriter (sw);

			HtmlInputButtonPoker p = new HtmlInputButtonPoker ();
			
			p.Page = new Page ();

			p.CausesValidation = false;
#if NET_2_0
			p.ValidationGroup = "VG";

			Assert.AreEqual (2, p.Attributes.Count, "A1");
#else
			Assert.AreEqual (2, p.Attributes.Count, "A1");
#endif

			p.DoRenderAttributes (tw);
#if NET_2_0
			Assert.AreEqual (" name type=\"button\" ValidationGroup=\"VG\" /", sw.ToString (), "A2");
#else
			Assert.AreEqual (" name type=\"button\" /", sw.ToString (), "A2");
#endif
		}

		private static void EmptyHandler (object sender, EventArgs e)
		{
		}
	}	
}

