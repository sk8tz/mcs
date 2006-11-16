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

namespace MonoTests.System.Web.UI
{
	[TestFixture]	
	public class ControlTest {
		[Test]
		public void DataBindingInterfaceTest ()
		{
			Control c;
			DataBindingCollection db;

			c = new Control ();
			Assert.AreEqual (false, ((IDataBindingsAccessor)c).HasDataBindings, "DB1");
			db = ((IDataBindingsAccessor)c).DataBindings;
			Assert.IsNotNull (db, "DB2");
			Assert.AreEqual (false, ((IDataBindingsAccessor)c).HasDataBindings, "DB3");
			db.Add(new DataBinding ("property", typeof(bool), "expression"));
			Assert.AreEqual (true, ((IDataBindingsAccessor)c).HasDataBindings);

		}

		class MyNC : Control, INamingContainer {
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
			Assert.IsTrue (nc.UniqueID.IndexOfAny (new char [] {':', '$' }) == -1, "separator");
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
			Assert.IsTrue (-1 != control.UniqueID.IndexOfAny (new char [] {':', '$' }), "separator");
		}

		class DerivedControl : Control {
			ControlCollection coll;

			public DerivedControl ()
			{
				coll = new ControlCollection (this);
			}

			public override ControlCollection Controls {
				get { return coll; }
			}
			
#if NET_2_0
			public bool DoIsViewStateEnabled {
				get { return IsViewStateEnabled; }
			}
#endif
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
		public void ApplyStyleSheetSkin ()
		{
			Page p = new Page ();
			p.StyleSheetTheme = "";
			Control c = new Control ();
			c.ApplyStyleSheetSkin (p);
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

		class NamingContainer : Control, INamingContainer
		{
		}

		class ControlWithTemplate : Control
		{
			ITemplate _template;

			[TemplateContainer (typeof(TemplateContainer))]
			public ITemplate Template {
				get { return _template; }
				set { _template = value; }
			}

			protected override void CreateChildControls () {
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
	}
}

