//
// Tests for System.Web.UI.WebControls.HierarchicalDataBoundControl.cs
//
// Author:
//	Igor Zelmanovich (igorz@mainsoft.com)
//
//
// Copyright (C) 2006 Mainsoft, Inc (http://www.mainsoft.com)
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


#if NET_2_0


using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Drawing;
using System.Threading;
using MyWebControl = System.Web.UI.WebControls;
using System.Collections;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;
using System.Text.RegularExpressions;
using System.Reflection;

namespace MonoTests.System.Web.UI.WebControls
{
	[Serializable]
	[TestFixture]
	public class HierarchicalDataBoundControlTest
	{
		class MyHierarchicalDataBoundControl : HierarchicalDataBoundControl
		{
			private StringBuilder dataBindTrace;
			public string DataBindTrace {
				get { return dataBindTrace.ToString (); }
			}

			public override void DataBind () {
				dataBindTrace = new StringBuilder ();
				dataBindTrace.Append("[Start DataBind]");
				base.DataBind ();
				dataBindTrace.Append ("[End DataBind]");
			}

			protected override void PerformSelect () {
				dataBindTrace.Append ("[Start PerformSelect]");
				base.PerformSelect ();
				dataBindTrace.Append ("[End PerformSelect]");
			}

			protected override void PerformDataBinding () {
				dataBindTrace.Append ("[Start PerformDataBinding]");
				base.PerformDataBinding ();
				dataBindTrace.Append ("[End PerformDataBinding]");
			}

			protected override void OnDataBinding (EventArgs e) {
				dataBindTrace.Append ("[Start OnDataBinding]");
				base.OnDataBinding (e);
				dataBindTrace.Append ("[End OnDataBinding]");
			}

			protected override void OnDataBound (EventArgs e) {
				dataBindTrace.Append ("[Start OnDataBound]");
				base.OnDataBound (e);
				dataBindTrace.Append ("[End OnDataBound]");
			}
		}

		[Test]
		public void HierarchicalDataBoundControl_DataBindFlow () {
			Page p = new Page ();
			MyHierarchicalDataBoundControl hc = new MyHierarchicalDataBoundControl ();
			p.Controls.Add (hc);
			hc.DataBind ();
			string expected = "[Start DataBind][Start PerformSelect][Start OnDataBinding][End OnDataBinding][Start PerformDataBinding][End PerformDataBinding][Start OnDataBound][End OnDataBound][End PerformSelect][End DataBind]";
			Assert.AreEqual (expected, hc.DataBindTrace, "DataBindFlow");
		}

	}
}

#endif
