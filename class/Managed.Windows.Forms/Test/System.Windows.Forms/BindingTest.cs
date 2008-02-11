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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Jackson Harper	jackson@ximian.com


using System;
using System.Data;
using System.Collections;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms.DataBinding {

	[TestFixture]
	public class BindingTest {

		[SetUp]
		protected virtual void SetUp ()
		{
		}

		[TearDown]
		protected virtual void TearDown ()
		{
		}

		[Test]
		public void CtorTest ()
		{
			string prop = "PROPERTY NAME";
			object data_source = new object ();
			string data_member = "DATA MEMBER";
			Binding b = new Binding (prop, data_source, data_member);

			Assert.IsNull (b.BindingManagerBase, "ctor1");
			Console.WriteLine ("MEMBER INFO:  " + b.BindingMemberInfo);
			Assert.IsNotNull (b.BindingMemberInfo, "ctor2");
			Assert.IsNull (b.Control, "ctor3");
			Assert.IsFalse (b.IsBinding, "ctor4");

			Assert.AreSame (b.PropertyName, prop, "ctor5");
			Assert.AreSame (b.DataSource, data_source, "ctor6");
		}

		[Test]
		public void CtorNullTest ()
		{
			Binding b = new Binding (null, null, null);

			Assert.IsNull (b.PropertyName, "ctornull1");
			Assert.IsNull (b.DataSource, "ctornull2");
		}

		[Test]
		public void BindingManagerBaseTest ()
		{
			if (TestHelper.RunningOnUnix) {
				Assert.Ignore ("Fails at the moment");
			}

			Control c1 = new Control ();
			Control c2 = new Control ();
			Binding binding;

			c1.BindingContext = new BindingContext ();
			c2.BindingContext = c1.BindingContext;

			binding = c2.DataBindings.Add ("Text", c1, "Text");

			Assert.IsNull (binding.BindingManagerBase, "1");

			c1.CreateControl ();
			c2.CreateControl ();

			Assert.IsNull (binding.BindingManagerBase, "2");

			c2.DataBindings.Remove (binding);
			binding = c2.DataBindings.Add ("Text", c1, "Text");

			Assert.IsTrue (binding.BindingManagerBase != null, "3");
		}

		[Test]
		/* create control and set binding context */
		public void BindingContextChangedTest ()
		{
			Control c = new Control ();
			// Test BindingContextChanged Event
			c.BindingContextChanged += new EventHandler (Event_Handler1);
			BindingContext bcG1 = new BindingContext ();
			eventcount = 0;
			c.BindingContext = bcG1;
			Assert.AreEqual (1, eventcount, "A1");
		}

		[Test]
		/* create control and show control */
		public void BindingContextChangedTest2 ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			Control c = new Control ();
			f.Controls.Add (c);

			c.BindingContextChanged += new EventHandler (Event_Handler1);
			eventcount = 0;
			f.Show ();
#if NET_2_0
			Assert.AreEqual (1, eventcount, "A1");
#else
			Assert.AreEqual (2, eventcount, "A1");
#endif
			f.Dispose();
		}

		[Test]
		/* create control, set binding context, and show control */
		public void BindingContextChangedTest3 ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;

			Control c = new Control ();
			f.Controls.Add (c);

			c.BindingContextChanged += new EventHandler (Event_Handler1);
			eventcount = 0;
			c.BindingContext = new BindingContext ();;
			f.Show ();
			Assert.AreEqual (1, eventcount, "A1");
			f.Dispose ();
		}

		[Test]
		public void BindingContextChangedTest4 ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;

			ContainerControl cc = new ContainerControl ();

			Control c = new Control ();
			f.Controls.Add (cc);
			cc.Controls.Add (c);

			c.BindingContextChanged += new EventHandler (Event_Handler1);
			cc.BindingContextChanged += new EventHandler (Event_Handler1);
			f.BindingContextChanged += new EventHandler (Event_Handler1);

			eventcount = 0;
			f.Show ();
#if NET_2_0
			Assert.AreEqual (5, eventcount, "A1");
#else
			Assert.AreEqual (8, eventcount, "A1");
#endif
			f.Dispose ();
		}

		int eventcount;
		public void Event_Handler1 (object sender, EventArgs e)
		{
			//Console.WriteLine (sender.GetType());
			//Console.WriteLine (Environment.StackTrace);
			eventcount++;
		}

		[Test]
		public void DataBindingCountTest1 ()
		{
			Control c = new Control ();
			Assert.AreEqual (0, c.DataBindings.Count, "1");
			c.DataBindings.Add (new Binding ("Text", c, "Name"));
			Assert.AreEqual (1, c.DataBindings.Count, "2");

			Binding b = c.DataBindings[0];
			Assert.AreEqual (c, b.Control, "3");
			Assert.AreEqual (c, b.DataSource, "4");
			Assert.AreEqual ("Text", b.PropertyName, "5");
			Assert.AreEqual ("Name", b.BindingMemberInfo.BindingField, "6");
		}

		[Test]
		public void DataBindingCountTest2 ()
		{
			Control c = new Control ();
			Control c2 = new Control ();
			Assert.AreEqual (0, c.DataBindings.Count, "1");
			c.DataBindings.Add (new Binding ("Text", c2, "Name"));
			Assert.AreEqual (1, c.DataBindings.Count, "2");
			Assert.AreEqual (0, c2.DataBindings.Count, "3");

			Binding b = c.DataBindings[0];
			Assert.AreEqual (c, b.Control, "4");
			Assert.AreEqual (c2, b.DataSource, "5");
			Assert.AreEqual ("Text", b.PropertyName, "6");
			Assert.AreEqual ("Name", b.BindingMemberInfo.BindingField, "7");
		}

		[Test]
		public void DataSourceNullTest ()
		{
			ChildMockItem item = new ChildMockItem ();
			Control c = new Control ();
			c.Tag = null;
			item.ObjectValue = null;

			c.DataBindings.Add ("Tag", item, "ObjectValue");

			Form f = new Form ();
			f.Controls.Add (c);

			f.Show (); // Need this to init data binding

			Assert.AreEqual (DBNull.Value, c.Tag, "1");
		}

		// For this case to work, the data source property needs
		// to have an associated 'PropertyChanged' event.
		[Test]
		public void DataSourcePropertyChanged ()
		{
			Control c = new Control ();
			c.BindingContext = new BindingContext ();
			c.CreateControl ();

			MockItem item = new MockItem ("A", 0);
			Binding binding = new Binding ("Text", item, "Text");

			c.DataBindings.Add (binding);
			Assert.AreEqual ("A", c.Text, "#A1");

			item.Text = "B";
			Assert.AreEqual ("B", c.Text, "#B1");
		}

		[Test]
		[Category ("NotWorking")]
		public void IsBindingTest ()
		{
			MockItem [] items = new MockItem [] { new MockItem ("A", 0) };
			Binding binding = new Binding ("Text", items, "Text");
			Binding binding2 = new Binding ("Text", items [0], "Text");
			Assert.IsFalse (binding.IsBinding, "#A1");
			Assert.IsFalse (binding2.IsBinding, "#A2");

			Control c = new Control ();
			Control c2 = new Control ();
			c.DataBindings.Add (binding);
			c2.DataBindings.Add (binding2);
			Assert.IsFalse (binding.IsBinding, "#B1");
			Assert.IsFalse (binding2.IsBinding, "#B2");

			c.CreateControl ();
			c2.CreateControl ();
			Assert.IsFalse (binding.IsBinding, "#C1");
			Assert.IsFalse (binding2.IsBinding, "#C2");

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (c);
			form.Controls.Add (c2);
			Assert.IsTrue (binding.IsBinding, "#D1");
			Assert.IsTrue (binding2.IsBinding, "#D2");

			form.Show ();

			// Important part -
			// IsBinding is true ALWAYS with PropertyManager, even when
			// ResumeBinding has been called
			//
			CurrencyManager curr_manager = (CurrencyManager)form.BindingContext [items];
			PropertyManager prop_manager = (PropertyManager)form.BindingContext [items [0]];
			curr_manager.SuspendBinding ();
			prop_manager.SuspendBinding ();
			//Assert.IsFalse (binding.IsBinding, "#E1"); // Comment by now
			Assert.IsTrue (binding2.IsBinding, "#E2");

			curr_manager.ResumeBinding ();
			prop_manager.ResumeBinding ();
			Assert.IsTrue (binding.IsBinding, "#F1");
			Assert.IsTrue (binding2.IsBinding, "#F2");

			form.Dispose ();
		}

#if NET_2_0
		[Test]
		public void ReadValueTest ()
		{
			Control c = new Control ();
			c.BindingContext = new BindingContext ();
			c.CreateControl ();

			ChildMockItem item = new ChildMockItem ();
			item.ObjectValue = "A";
			Binding binding = new Binding ("Tag", item, "ObjectValue");
			binding.ControlUpdateMode = ControlUpdateMode.Never;

			c.DataBindings.Add (binding);
			Assert.AreEqual (null, c.Tag, "#A1");

			item.ObjectValue = "B";
			Assert.AreEqual (null, c.Tag, "#B1");

			binding.ReadValue ();
			Assert.AreEqual ("B", c.Tag, "#C1");

			item.ObjectValue = "C";
			binding.ReadValue ();
			Assert.AreEqual ("C", c.Tag, "#D1");
		}

		[Test]
		[Category ("NotWorking")]
		public void WriteValueTest ()
		{
			Control c = new Control ();
			c.BindingContext = new BindingContext ();
			c.CreateControl ();

			MockItem item = new MockItem ();
			item.Text = "A";
			Binding binding = new Binding ("Text", item, "Text");
			binding.DataSourceUpdateMode = DataSourceUpdateMode.Never;

			c.DataBindings.Add (binding);
			Assert.AreEqual ("A", c.Text, "#A1");

			c.Text = "B";
			Assert.AreEqual ("A", item.Text, "#B1");

			binding.WriteValue ();
			Assert.AreEqual ("B", item.Text, "#C1");
		}

		[Test]
		public void ControlUpdateModeTest ()
		{
			Control c = new Control ();
			c.BindingContext = new BindingContext ();
			c.CreateControl ();

			MockItem item = new MockItem ("A", 0);
			Binding binding = new Binding ("Text", item, "Text");
			binding.ControlUpdateMode = ControlUpdateMode.Never;

			c.DataBindings.Add (binding);
			Assert.AreEqual (String.Empty, c.Text, "#A1");

			item.Text = "B";
			Assert.AreEqual (String.Empty, c.Text, "#B1");
		}

		[Test]
		[Category ("NotWorking")]
		public void DataSourceUpdateModeTest ()
		{
			Control c = new Control ();
			c.BindingContext = new BindingContext ();
			c.CreateControl ();

			MockItem item = new MockItem ("A", 0);
			Binding binding = new Binding ("Text", item, "Text");
			binding.DataSourceUpdateMode = DataSourceUpdateMode.Never;

			c.DataBindings.Add (binding);
			Assert.AreEqual ("A", c.Text, "#A1");

			c.Text = "B";
			Assert.AreEqual ("A", item.Text, "#B1");

			binding.DataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;
			Assert.AreEqual ("A", item.Text, "#C1");

			c.Text = "C";
			Assert.AreEqual ("C", item.Text, "#D1");

			// This requires a Validation even, which we can't test
			// by directly modifying the property
			binding.DataSourceUpdateMode = DataSourceUpdateMode.OnValidation;

			c.Text = "D";
			Assert.AreEqual ("C", item.Text, "#E1");
		}

		[Test]
		[Category ("NotWorking")]
		public void DataSourceNullValueTest ()
		{
			Control c = new Control ();
			c.BindingContext = new BindingContext ();
			c.CreateControl ();

			ChildMockItem item = new ChildMockItem ();
			item.ObjectValue = "A";
			Binding binding = new Binding ("Tag", item, "ObjectValue");
			binding.DataSourceNullValue = "NonNull";

			c.DataBindings.Add (binding);
			Assert.AreEqual (c.Tag, "A", "#A1");

			// Since Tag property doesn't have a 
			// TagChanged event, we need to force an update
			c.Tag = null;
			binding.WriteValue ();
			Assert.AreEqual (item.ObjectValue, "NonNull", "#B1");
		}
#endif
	}

	class ChildMockItem : MockItem
	{
		object value;

		public ChildMockItem ()
			: base (null, 0)
		{
		}

		public object ObjectValue
		{
			get
			{
				return value;
			}
			set
			{
				this.value = value;
			}
		}
	}

}

