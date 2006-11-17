//
// FormTest.cs: Test cases for Form.
//
// Author:
//   Ritvik Mayank (mritvik@novell.com)
//
// (C) 2005 Novell, Inc. (http://www.novell.com)
//

using System;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing;
using System.Reflection;
using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class FormTest
	{
		[Test]
		public void FormPropertyTest ()
		{
			Form myform = new Form ();
			myform.Visible = true;
			myform.Text = "NewForm";
			myform.Name = "FormTest";
			Assert.IsNull (myform.AcceptButton, "#1");
			Assert.IsNull (myform.ActiveMdiChild, "#2"); 
			Assert.IsFalse (myform.AutoScale, "#3");
			Assert.IsNull (myform.CancelButton, "#6");
			Assert.IsTrue (myform.ControlBox, "#9");
			Assert.IsTrue (myform.DesktopBounds.X > 0, "#10a");
			Assert.IsTrue (myform.DesktopBounds.Y > 0, "#10b");
			Assert.AreEqual (300, myform.DesktopBounds.Height, "#10c");
			Assert.AreEqual (300, myform.DesktopBounds.Width, "#10d");
			Assert.IsTrue (myform.DesktopLocation.X > 0, "#11a");
			Assert.IsTrue (myform.DesktopLocation.Y > 0, "#11b");
			Assert.AreEqual (DialogResult.None, myform.DialogResult, "#12");
			Assert.AreEqual (FormBorderStyle.Sizable, myform.FormBorderStyle, "#13");
			Assert.IsFalse (myform.HelpButton, "#14");
			Assert.AreEqual ("System.Drawing.Icon", myform.Icon.GetType ().ToString (), "#15");
			Assert.IsFalse (myform.IsMdiChild, "#16");
			Assert.IsFalse (myform.IsMdiContainer, "#17");
			Assert.IsFalse (myform.KeyPreview, "#18");
			Assert.IsTrue (myform.MaximizeBox, "#19");
			Assert.AreEqual (0, myform.MaximumSize.Height, "#20a");
			Assert.AreEqual (0, myform.MaximumSize.Width, "#20b");
			Assert.AreEqual (0, myform.MdiChildren.Length, "#21a");
			Assert.AreEqual (1, myform.MdiChildren.Rank, "#21b");
			Assert.IsFalse (myform.MdiChildren.IsSynchronized, "#21c");
			Assert.IsNull (myform.MdiParent, "#22");
			Assert.IsNull (myform.Menu, "#23");
			Assert.IsNull (myform.MergedMenu, "#24");
			Assert.IsTrue (myform.MinimizeBox, "#25");
			Assert.AreEqual (0, myform.MinimumSize.Height, "#26a");
			Assert.AreEqual (0, myform.MinimumSize.Width, "#26b");
			Assert.IsTrue (myform.MinimumSize.IsEmpty, "#26c");
			Assert.IsFalse (myform.Modal, "#27");
			Assert.AreEqual (1, myform.Opacity, "#28");
			Assert.AreEqual (0, myform.OwnedForms.Length, "#29a");
			Assert.AreEqual (1, myform.OwnedForms.Rank, "#29b");
			Assert.IsNull (myform.Owner, "#30");
			Assert.IsTrue (myform.ShowInTaskbar, "#31");
			Assert.AreEqual (300, myform.Size.Height, "#32a");
			Assert.AreEqual (300, myform.Size.Width, "#32b");
			Assert.AreEqual (SizeGripStyle.Auto, myform.SizeGripStyle, "#33");
			Assert.AreEqual (FormStartPosition.WindowsDefaultLocation, myform.StartPosition, "#34");
			Assert.IsTrue (myform.TopLevel, "#35");
			Assert.IsFalse (myform.TopMost, "#36");
			Assert.AreEqual (Color.Empty, myform.TransparencyKey, "#37");
			Assert.AreEqual (FormWindowState.Normal, myform.WindowState, "#38");
			myform.Dispose ();
		}

		[Test]
		public void ActivateTest ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			myform.Visible = true;
			myform.Text = "NewForm";
			myform.Name = "FormTest";
			myform.Activate ();
			Assert.AreEqual (true, myform.Focus (), "#40");
			myform.Dispose ();
		}		

		[Test]
		public void AddOwnedFormTest ()
		{
			Form parent = new Form ();
			parent.ShowInTaskbar = false;
			parent.Text = "NewParent";
			Form ownedForm = new Form ();
			ownedForm.ShowInTaskbar = false;
			ownedForm.Text = "Owned Form";
			parent.AddOwnedForm (ownedForm);
			ownedForm.Show ();
			Assert.AreEqual ("NewParent", ownedForm.Owner.Text, "#41");
			ownedForm.Dispose ();
			parent.Dispose ();
		}

		[Test]
		public void RemoveOwnedFormTest ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			myform.Text = "NewForm";
			myform.Name = "FormTest";
			myform.RemoveOwnedForm (myform);
			myform.Show ();
			Assert.AreEqual (null, myform.Owner, "#44");
			myform.Dispose ();
		}

		[Test]
		public void SetDesktopBoundsTest ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			myform.Visible = true;
			myform.Text = "NewForm";
			myform.Name = "FormTest";
			myform.SetDesktopBounds (10, 10, 200 , 200);
			Assert.AreEqual (200, myform.DesktopBounds.Height, "#45");
			myform.Dispose ();
		}

		[Test]
		public void SetDesktopLocationTest ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			myform.Visible = true;
			myform.Text = "NewForm";
			myform.Name = "FormTest";
			myform.SetDesktopLocation (10, 10);
			Assert.AreEqual (10, myform.DesktopLocation.X, "#46");
			myform.Dispose ();
		}

		[Test]
		public void SetDialogResultOutOfRange ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			try {
				myform.DialogResult = (DialogResult) (-1);
				Assert.Fail ("#48");
			} catch (InvalidEnumArgumentException) {
			}

			try {
				myform.DialogResult = (DialogResult) ((int) DialogResult.No + 1);
				Assert.Fail ("#49");
			} catch (InvalidEnumArgumentException) {
			}
			myform.Dispose ();
		}

		void myform_set_dialogresult (object sender, EventArgs e)
		{
			Form f = (Form)sender;

			f.DialogResult = DialogResult.OK;
		}

		void myform_close (object sender, EventArgs e)
		{
			Form f = (Form)sender;

			f.Close();
		}

		[Test]
		public void SetDialogResult ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;
			myform.Visible = true;

			myform.DialogResult = DialogResult.Cancel;

			Assert.IsTrue (myform.Visible, "A1");
			Assert.IsFalse (myform.IsDisposed, "A2");

			myform.Close ();

			Assert.IsFalse (myform.Visible, "A3");
			Assert.IsTrue (myform.IsDisposed, "A4");

			DialogResult result;

			myform = new Form ();
			myform.ShowInTaskbar = false;
			myform.VisibleChanged += new EventHandler (myform_set_dialogresult);
			result = myform.ShowDialog ();

			Assert.AreEqual (result, DialogResult.OK, "A5");
			Assert.IsFalse (myform.Visible, "A6");
			Assert.IsFalse (myform.IsDisposed, "A7");

			myform = new Form ();
			myform.ShowInTaskbar = false;
			myform.VisibleChanged += new EventHandler (myform_close);
			result = myform.ShowDialog ();

			Assert.AreEqual (result, DialogResult.Cancel, "A8");
			Assert.IsFalse (myform.Visible, "A9");
			Assert.IsFalse (myform.IsDisposed, "A10");
		}

		[Test]
		public void DisposeOwnerTest ()
		{
			Form f1 = new Form ();
			Form f2 = new Form ();

			f2.Owner = f1;

			f1.Dispose ();

			Assert.IsNull (f2.Owner, "1");
			Assert.AreEqual (0, f1.OwnedForms.Length, "2");
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void AccessDisposedForm ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;

			myform.Show ();
			myform.Close (); // this should result in the form being disposed
			myform.Show (); // and this line should result in the ODE being thrown
		}

		class MyForm : Form
		{
			public void DoDestroyHandle ()
			{
				DestroyHandle();
			}
			public void DoRecreateHandle ()
			{
				RecreateHandle();
			}
		}

		int handle_destroyed_count;
		void handle_destroyed (object sender, EventArgs e)
		{
			handle_destroyed_count++;
		}

		[Test]
		public void DestroyHandleTest ()
		{
			handle_destroyed_count = 0;

			MyForm f1 = new MyForm ();
			f1.HandleDestroyed += new EventHandler (handle_destroyed);
			f1.Show ();
			f1.DoDestroyHandle ();
			Assert.AreEqual (1, handle_destroyed_count, "1");

			f1 = new MyForm ();
			f1.HandleDestroyed += new EventHandler (handle_destroyed);
			f1.Show ();
			f1.DoRecreateHandle ();
			Assert.AreEqual (2, handle_destroyed_count, "2");
		}

		[Test]
		public void FormClose ()
		{
			Form myform = new Form ();
			myform.ShowInTaskbar = false;

			Assert.IsFalse (myform.Visible, "A1");
			Assert.IsFalse (myform.IsDisposed, "A2");

			myform.Close ();

			Assert.IsFalse (myform.Visible, "A3");
			Assert.IsFalse (myform.IsDisposed, "A4");

			myform.Show ();

			Assert.IsTrue (myform.Visible, "A5");
			Assert.IsFalse (myform.IsDisposed, "A6");

			myform.Close ();

			Assert.IsFalse (myform.Visible, "A7");
			Assert.IsTrue (myform.IsDisposed, "A8");
		}

		[Test]
		public void FormClose2 ()
		{
			WMCloseWatcher f = new WMCloseWatcher ();
			f.ShowInTaskbar = false;

			f.close_count = 0;
			Assert.IsFalse (f.Visible, "A1");
			f.Close ();
			Assert.AreEqual (0, f.close_count, "A2");


			f.Show ();
			f.Close ();
			Assert.AreEqual (1, f.close_count, "A3");
		}

		class WMCloseWatcher : Form {
			public int close_count;

			protected override void WndProc (ref Message msg) {
				if (msg.Msg == 0x0010 /* WM_CLOSE */) {
					close_count ++;
				}

				base.WndProc (ref msg);
			}
		}
	}
}
