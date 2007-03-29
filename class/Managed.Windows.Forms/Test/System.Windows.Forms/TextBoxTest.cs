//
// Copyright (c) 2005 Novell, Inc.
//
// Authors:
//      Ritvik Mayank (mritvik@novell.com)
//

using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class TextBoxTest
	{
		TextBox textBox;
		int _invalidated;
		int _paint;

		[SetUp]
		public void SetUp()
		{
			textBox = new TextBox();
			textBox.Invalidated += new InvalidateEventHandler (TextBox_Invalidated);
			textBox.Paint += new PaintEventHandler (TextBox_Paint);
			Reset ();
		}

		[Test]
		[Category ("NotWorking")]
		public void TextBoxBasePropertyTest ()
		{
			Assert.AreEqual (false, textBox.AcceptsTab, "#1a");
			textBox.Multiline = true;
			textBox.AcceptsTab = true;
			//	SendKeys.SendWait ("^%");
			Assert.AreEqual (true, textBox.AcceptsTab, "#1b");
			Assert.AreEqual (true, textBox.AutoSize, "#2");
			Assert.AreEqual (null, textBox.BackgroundImage, "#4a");
			string gif = "M.gif";
			textBox.BackgroundImage = Image.FromFile (gif);
			// comparing image objects fails on MS .Net so using Size property
			Assert.AreEqual (Image.FromFile(gif, true).Size, textBox.BackgroundImage.Size, "#4b");
			
			Assert.AreEqual (BorderStyle.Fixed3D, textBox.BorderStyle, "#5");
			Assert.AreEqual (false, textBox.CanUndo, "#6a");

			Clipboard.SetDataObject ("TEST");
			textBox.Paste ();
			Assert.AreEqual (true, textBox.CanUndo, "#6b");
			textBox.Undo ();
			textBox.ClearUndo ();
			Assert.AreEqual (false, textBox.CanUndo, "#6c");

			Assert.AreEqual (true, textBox.HideSelection, "#8");
			Assert.AreEqual (1, textBox.Lines.Length, "#9");
			Assert.AreEqual (32767, textBox.MaxLength, "#10");
			Assert.AreEqual (true, textBox.Modified, "#11");
			Assert.AreEqual (true, textBox.Multiline, "#12a");
			textBox.WordWrap = false;
			Assert.AreEqual (true, textBox.Multiline, "#12b");
			textBox.AcceptsReturn = true;
			Assert.AreEqual (true, textBox.Multiline, "#12c");
			Assert.AreEqual (20, textBox.PreferredHeight, "#13");
			Assert.AreEqual (false, textBox.ReadOnly, "#14");
			Assert.AreEqual ("", textBox.SelectedText, "#15");
			textBox.Text = "sample TextBox";
			Assert.AreEqual (0, textBox.SelectionLength, "#16b");
			Assert.AreEqual (0, textBox.SelectionStart, "#17");
			textBox.WordWrap = false;
			textBox.AcceptsReturn = true;
			Assert.AreEqual ("sample TextBox", textBox.Text, "#18");
			Assert.AreEqual (14, textBox.TextLength, "#19");
			Assert.AreEqual (false, textBox.WordWrap, "#20");
		}

		[Test]
		public void TextBoxPropertyTest ()
		{
			Assert.AreEqual (false, textBox.AcceptsReturn, "#21");
			Assert.AreEqual (CharacterCasing.Normal, textBox.CharacterCasing, "#22");
			Assert.AreEqual ('\0', textBox.PasswordChar, "#23");
			textBox.PasswordChar = '*';
			Assert.AreEqual ('*', textBox.PasswordChar, "#23b");
			Assert.AreEqual (ScrollBars.None, textBox.ScrollBars, "#24");
			Assert.AreEqual (-1, textBox.SelectionLength, "#25");
			Assert.AreEqual (HorizontalAlignment.Left , textBox.TextAlign, "#26");
#if NET_2_0
			Assert.AreEqual (true, textBox.AutoCompleteCustomSource != null, "#27");
			Assert.AreEqual (AutoCompleteMode.None, textBox.AutoCompleteMode, "#28");
			Assert.AreEqual (AutoCompleteSource.None, textBox.AutoCompleteSource, "#29");

			textBox.AutoCompleteCustomSource = null;
			Assert.AreEqual (true, textBox.AutoCompleteCustomSource != null, "#30");
#endif
		}

#if NET_2_0
		[Test]
		public void UseSystemPasswordCharDefault()
		{
			Assert.IsFalse(textBox.UseSystemPasswordChar);
		}

		[Test]
		public void UseSystemPasswordCharOverridesPasswordChar()
		{
			textBox.PasswordChar = '!';
			textBox.UseSystemPasswordChar = true;
			Assert.AreEqual('*', textBox.PasswordChar);
		}
#endif

		[Test]
		public void AppendTextTest ()
		{
			Form f = new Form (); 
			f.ShowInTaskbar = false;
			f.Visible = true;
			textBox.Visible = true;
			textBox.Text = "TextBox1";
			TextBox textBox2 = new TextBox ();
			textBox2.Visible = true;
			f.Controls.Add (textBox);
			f.Controls.Add (textBox2);
			textBox2.AppendText (textBox.Text);
			Assert.AreEqual ("TextBox1", textBox2.Text, "#27");
			f.Dispose ();
		}

		[Test]
		public void AppendTextTest2 ()
		{
			TextBox textBox2 = new TextBox ();
			textBox2.AppendText ("hi");
			textBox2.AppendText ("ho");
			Assert.AreEqual ("hiho", textBox2.Text, "#1");
			Assert.IsNotNull (textBox2.Lines, "#2");
			Assert.AreEqual (1, textBox2.Lines.Length, "#3");
			Assert.AreEqual ("hiho", textBox2.Lines [0], "#4");
		}

		[Test]
		public void AppendText_Multiline_CRLF ()
		{
			TextBox textBox = new TextBox ();
			textBox.Text = "ha";
			textBox.AppendText ("hi\r\n\r\n");
			textBox.AppendText ("ho\r\n");
			Assert.AreEqual ("hahi\r\n\r\nho\r\n", textBox.Text, "#A1");
			Assert.IsNotNull (textBox.Lines, "#A2");
			Assert.AreEqual (4, textBox.Lines.Length, "#A3");
			Assert.AreEqual ("hahi", textBox.Lines [0], "#A4");
			Assert.AreEqual (string.Empty, textBox.Lines [1], "#A5");
			Assert.AreEqual ("ho", textBox.Lines [2], "#A6");
			Assert.AreEqual (string.Empty, textBox.Lines [3], "#A7");

			textBox.Multiline = true;

			textBox.Text = "ha";
			textBox.AppendText ("hi\r\n\r\n");
			textBox.AppendText ("ho\r\n");
			Assert.AreEqual ("hahi\r\n\r\nho\r\n", textBox.Text, "#B1");
			Assert.IsNotNull (textBox.Lines, "#B2");
			Assert.AreEqual (4, textBox.Lines.Length, "#B3");
			Assert.AreEqual ("hahi", textBox.Lines [0], "#B4");
			Assert.AreEqual (string.Empty, textBox.Lines [1], "#B5");
			Assert.AreEqual ("ho", textBox.Lines [2], "#B6");
			Assert.AreEqual (string.Empty, textBox.Lines [3], "#B7");
		}

		[Test]
		public void AppendText_Multiline_LF ()
		{
			TextBox textBox = new TextBox ();

			textBox.Text = "ha";
			textBox.AppendText ("hi\n\n");
			textBox.AppendText ("ho\n");
			Assert.AreEqual ("hahi\n\nho\n", textBox.Text, "#A1");
			Assert.IsNotNull (textBox.Lines, "#A2");
			Assert.AreEqual (4, textBox.Lines.Length, "#A3");
			Assert.AreEqual ("hahi", textBox.Lines [0], "#A4");
			Assert.AreEqual (string.Empty, textBox.Lines [1], "#A5");
			Assert.AreEqual ("ho", textBox.Lines [2], "#A6");
			Assert.AreEqual (string.Empty, textBox.Lines [3], "#A7");

			textBox.Multiline = true;

			textBox.Text = "ha";
			textBox.AppendText ("hi\n\n");
			textBox.AppendText ("ho\n");
			Assert.AreEqual ("hahi\n\nho\n", textBox.Text, "#B1");
			Assert.IsNotNull (textBox.Lines, "#B2");
			Assert.AreEqual (4, textBox.Lines.Length, "#B3");
			Assert.AreEqual ("hahi", textBox.Lines [0], "#B4");
			Assert.AreEqual (string.Empty, textBox.Lines [1], "#B5");
			Assert.AreEqual ("ho", textBox.Lines [2], "#B6");
			Assert.AreEqual (string.Empty, textBox.Lines [3], "#B7");
		}

		[Test]
		public void BackColorTest ()
		{
			Assert.AreEqual (SystemColors.Window, textBox.BackColor, "#A1");
			textBox.BackColor = Color.Red;
			Assert.AreEqual (Color.Red, textBox.BackColor, "#A2");
			textBox.BackColor = Color.White;
			Assert.AreEqual (Color.White, textBox.BackColor, "#A3");
			Assert.AreEqual (0, _invalidated, "#A4");
			Assert.AreEqual (0, _paint, "#A5");

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (textBox);
			form.Show ();

			Assert.AreEqual (Color.White, textBox.BackColor, "#B1");
			Assert.AreEqual (0, _invalidated, "#B2");
			Assert.AreEqual (0, _paint, "#B3");
			textBox.BackColor = Color.Red;
			Assert.AreEqual (Color.Red, textBox.BackColor, "#B4");
			Assert.AreEqual (1, _invalidated, "#B5");
			Assert.AreEqual (0, _paint, "#B6");
			textBox.BackColor = Color.Red;
			Assert.AreEqual (Color.Red, textBox.BackColor, "#B7");
			Assert.AreEqual (1, _invalidated, "#B8");
			Assert.AreEqual (0, _paint, "#B9");
			textBox.BackColor = Color.Blue;
			Assert.AreEqual (Color.Blue, textBox.BackColor, "#B10");
			Assert.AreEqual (2, _invalidated, "#B11");
			Assert.AreEqual (0, _paint, "#B12");
		}

		[Test]
		public void ClearTest ()
		{
			textBox.Text = "TextBox1";
			Assert.AreEqual ("TextBox1", textBox.Text, "#28a" );
			textBox.Clear ();
			Assert.AreEqual ("", textBox.Text, "#28b");
		}

		[Test]
		public void ClearUndoTest ()
		{
			textBox.Text = "TextBox1";
			textBox.SelectionLength = 4;
			textBox.Copy ();
			Assert.AreEqual ("Text", textBox.SelectedText, "#29a");
			textBox.Paste ();
			Assert.AreEqual (true, textBox.CanUndo, "#29b");
			textBox.ClearUndo ();
			Assert.AreEqual (false, textBox.CanUndo, "#29c");
		}

		[Test] // bug #80163
		public void ContextMenu ()
		{
			TextBox textBox = new TextBox ();
			Assert.IsNull (textBox.ContextMenu);
		}

		[Test]
		public void CopyTest ()
		{
			textBox.Text = "ABCDE";
			textBox.SelectionLength = 4;
			textBox.Copy ();
			Assert.AreEqual ("ABCD", textBox.SelectedText, "#30");
		}

		[Test]
		public void CutTest ()
		{
			textBox.Text = "ABCDE";
			textBox.SelectionLength = 4;
			textBox.Cut ();
			Assert.AreEqual ("E", textBox.Text, "#31");
		}

		[Test]
		public void PasteTest ()
		{
			textBox.Text = "ABCDE";
			textBox.SelectionLength = 4;
			textBox.Copy ();
			textBox.SelectionStart = textBox.SelectionStart + textBox.SelectionLength;
			textBox.Paste ();
			Assert.AreEqual ("ABCDABCD", textBox.Text, "#32");
		}

		[Test] // bug #80301
		[Ignore ("Depends on specific DPI")]
		public void PreferredHeight ()
		{
			textBox.Font = new Font ("Arial", 14);
			Assert.AreEqual (29, textBox.PreferredHeight, "#A1");
			textBox.Font = new Font ("Arial", 16);
			Assert.AreEqual (32, textBox.PreferredHeight, "#A2");
			textBox.Font = new Font ("Arial", 17);
			Assert.AreEqual (34, textBox.PreferredHeight, "#A3");

			textBox.BorderStyle = BorderStyle.None;

			Assert.AreEqual (27, textBox.PreferredHeight, "#B1");
			textBox.Font = new Font ("Arial", 14);
			Assert.AreEqual (22, textBox.PreferredHeight, "#B2");
			textBox.Font = new Font ("Arial", 16);
			Assert.AreEqual (25, textBox.PreferredHeight, "#B3");
		}

		[Test]
		public void SelectTest ()
		{
			textBox.Text = "This is a sample test.";
			textBox.Select (0, 4);
			Assert.AreEqual ("This", textBox.SelectedText, "#33");
		}

		[Test]
		public void SelectAllTest ()
		{
			textBox.Text = "This is a sample test.";
			textBox.SelectAll ();
			Assert.AreEqual ("This is a sample test.", textBox.SelectedText, "#34");
		}

		[Test]
		[Category ("NotWorking")]
		public void FocusSelectsAllTest ()
		{
			Form form = new Form ();
			form.ShowInTaskbar = false;

			TextBox textBoxA = new TextBox ();
			textBoxA.Text = "This is a sample testA.";
			textBoxA.TabIndex = 0;
			form.Controls.Add (textBoxA);

			TextBox textBoxB = new TextBox ();
			textBoxB.Text = "This is a sample testB.";
			textBoxB.TabIndex = 1;
			form.Controls.Add (textBoxB);

#if NET_2_0
			Assert.AreEqual (String.Empty, textBoxA.SelectedText, "#A1 (2.0)");
			Assert.AreEqual (String.Empty, textBoxB.SelectedText, "#A2 (2.0)");
#else
			Assert.IsNull (textBoxA.SelectedText, "#A1");
			Assert.IsNull (textBoxB.SelectedText, "#A2");
#endif

			form.Show ();

			textBoxA.Focus ();

			Assert.AreEqual ("This is a sample testA.", textBoxA.SelectedText, "#B1");
			Assert.AreEqual (string.Empty, textBoxB.SelectedText, "#B2");

			textBoxB.Focus ();

			Assert.AreEqual ("This is a sample testA.", textBoxA.SelectedText, "#C1");
			Assert.AreEqual ("This is a sample testB.", textBoxB.SelectedText, "#C2");

			textBoxA.Text = "another testA.";
			textBoxB.Text = "another testB.";

			Assert.AreEqual (string.Empty, textBoxA.SelectedText, "#D1");
			Assert.AreEqual (string.Empty, textBoxB.SelectedText, "#D2");

			textBoxA.Focus ();

			Assert.AreEqual ("another testA.", textBoxA.SelectedText, "#E1");
			Assert.AreEqual (string.Empty, textBoxB.SelectedText, "#E2");

			textBoxB.Focus ();

			Assert.AreEqual ("another testA.", textBoxA.SelectedText, "#F1");
			Assert.AreEqual ("another testB.", textBoxB.SelectedText, "#F2");

			form.Dispose ();
		}

		[Test]
		[Category ("NotWorking")]
		public void ForeColorTest ()
		{
			Assert.AreEqual (SystemColors.WindowText, textBox.ForeColor, "#A1");
			textBox.ForeColor = Color.Red;
			Assert.AreEqual (Color.Red, textBox.ForeColor, "#A2");
			textBox.ForeColor = Color.White;
			Assert.AreEqual (Color.White, textBox.ForeColor, "#A3");
			Assert.AreEqual (0, _invalidated, "#A4");
			Assert.AreEqual (0, _paint, "#A5");

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (textBox);
			form.Show ();

			Assert.AreEqual (Color.White, textBox.ForeColor, "#B1");
			Assert.AreEqual (0, _invalidated, "#B2");
			Assert.AreEqual (0, _paint, "#B3");
			textBox.ForeColor = Color.Red;
			Assert.AreEqual (Color.Red, textBox.ForeColor, "#B4");
			Assert.AreEqual (1, _invalidated, "#B5");
			Assert.AreEqual (0, _paint, "#B6");
			textBox.ForeColor = Color.Red;
			Assert.AreEqual (Color.Red, textBox.ForeColor, "#B7");
			Assert.AreEqual (1, _invalidated, "#B8");
			Assert.AreEqual (0, _paint, "#B9");
			textBox.ForeColor = Color.Blue;
			Assert.AreEqual (Color.Blue, textBox.ForeColor, "#B10");
			Assert.AreEqual (2, _invalidated, "#B11");
			Assert.AreEqual (0, _paint, "#B12");
		}

		[Test]
		public void ReadOnly_BackColor_NotSet ()
		{
			textBox.ReadOnly = true;
			Assert.IsTrue (textBox.ReadOnly, "#A1");
#if NET_2_0
			Assert.AreEqual (SystemColors.Control, textBox.BackColor, "#A2");
#else
			Assert.AreEqual (SystemColors.Window, textBox.BackColor, "#A2");
#endif

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (textBox);
			form.Show ();

			Assert.IsTrue (textBox.ReadOnly, "#B1");
#if NET_2_0
			Assert.AreEqual (SystemColors.Control, textBox.BackColor, "#B2");
#else
			Assert.AreEqual (SystemColors.Window, textBox.BackColor, "#B2");
#endif

			textBox.ReadOnly = false;
			Assert.IsFalse (textBox.ReadOnly, "#C1");
			Assert.AreEqual (SystemColors.Window, textBox.BackColor, "#C2");

			textBox.ReadOnly = true;
			Assert.IsTrue (textBox.ReadOnly, "#D1");
#if NET_2_0
			Assert.AreEqual (SystemColors.Control, textBox.BackColor, "#D2");
#else
			Assert.AreEqual (SystemColors.Window, textBox.BackColor, "#D2");
#endif

			textBox.BackColor = Color.Red;
			Assert.IsTrue (textBox.ReadOnly, "#E1");
			Assert.AreEqual (Color.Red, textBox.BackColor, "#E2");

			textBox.ReadOnly = false;
			Assert.IsFalse (textBox.ReadOnly, "#F1");
			Assert.AreEqual (Color.Red, textBox.BackColor, "#F2");

			textBox.ReadOnly = true;
			Assert.IsTrue (textBox.ReadOnly, "#G1");
			Assert.AreEqual (Color.Red, textBox.BackColor, "#G2");
		}

		[Test]
		public void ReadOnly_BackColor_Set ()
		{
			textBox.BackColor = Color.Blue;
			textBox.ReadOnly = true;
			Assert.IsTrue (textBox.ReadOnly, "#A1");
			Assert.AreEqual (Color.Blue, textBox.BackColor, "#A2");

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (textBox);
			form.Show ();

			Assert.IsTrue (textBox.ReadOnly, "#B1");
			Assert.AreEqual (Color.Blue, textBox.BackColor, "#B2");

			textBox.ReadOnly = false;
			Assert.IsFalse (textBox.ReadOnly, "#C1");
			Assert.AreEqual (Color.Blue, textBox.BackColor, "#C2");

			textBox.ReadOnly = true;
			Assert.IsTrue (textBox.ReadOnly, "#D1");
			Assert.AreEqual (Color.Blue, textBox.BackColor, "#D2");

			textBox.BackColor = Color.Red;
			Assert.IsTrue (textBox.ReadOnly, "#E1");
			Assert.AreEqual (Color.Red, textBox.BackColor, "#E2");

			textBox.ReadOnly = false;
			Assert.IsFalse (textBox.ReadOnly, "#F1");
			Assert.AreEqual (Color.Red, textBox.BackColor, "#F2");

			form.Dispose ();

			textBox = new TextBox ();
			textBox.ReadOnly = true;
			textBox.BackColor = Color.Blue;
			Assert.IsTrue (textBox.ReadOnly, "#G1");
			Assert.AreEqual (Color.Blue, textBox.BackColor, "#G2");

			form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (textBox);
			form.Show ();

			Assert.IsTrue (textBox.ReadOnly, "#H1");
			Assert.AreEqual (Color.Blue, textBox.BackColor, "#H2");

			textBox.ReadOnly = false;
			Assert.IsFalse (textBox.ReadOnly, "#I1");
			Assert.AreEqual (Color.Blue, textBox.BackColor, "#I2");
		}

		[Test]
		public void ToStringTest ()
		{
			Assert.AreEqual ("System.Windows.Forms.TextBox, Text: ", textBox.ToString(), "#35");
		}

		[Test]
		public void UndoTest1 ()
		{
			textBox.Text = "ABCDE";
			textBox.SelectionLength = 4;
			textBox.Copy ();
			textBox.SelectionStart = textBox.SelectionStart + textBox.SelectionLength;
			textBox.Paste ();
			Console.WriteLine ("pre paste text:  {0}", textBox.Text);
			textBox.Undo ();
			Assert.AreEqual ("ABCDE", textBox.Text, "#36");
		}

		[Test] // bug #79851
		public void WrappedText ()
		{
			string text = "blabla blablabalbalbalbalbalbal blabla blablabl bal " +
				"bal bla bal balajkdhfk dskfk ersd dsfjksdhf sdkfjshd f";

			textBox.Multiline = true;
			textBox.Size = new Size (30, 168);
			textBox.Text = text;

			Form form = new Form ();
			form.Controls.Add (textBox);
			form.ShowInTaskbar = false;
			form.Show ();

			Assert.AreEqual (text, textBox.Text);
		}

		[Test] // bug #79909
		public void MultilineText ()
		{
			string text = "line1\n\nline2\nline3\r\nline4";

			textBox.Size = new Size (300, 168);
			textBox.Text = text;

			Form form = new Form ();
			form.Controls.Add (textBox);
			form.ShowInTaskbar = false;
			form.Show ();

			Assert.AreEqual (text, textBox.Text, "#1");

			text = "line1\n\nline2\nline3\r\nline4\rline5\r\n\nline6\n\n\nline7";

			textBox.Text = text;

			form.Visible = false;
			form.Show ();

			Assert.AreEqual (text, textBox.Text, "#2");
		}

		
		[Test]
		public void ModifiedTest ()
		{
			Assert.AreEqual (true, textBox.Modified, "modified-1");

			textBox.Modified = false;
			Assert.AreEqual (false, textBox.Modified, "modified-2");

			textBox.Modified = true;
			Assert.AreEqual (true, textBox.Modified, "modified-2");

			textBox.Text = "TEXT";
			Assert.AreEqual (false, textBox.Modified, "modified-3");
		}

		void TextBox_Invalidated (object sender, InvalidateEventArgs e)
		{
			_invalidated++;
		}

		void TextBox_Paint (object sender, PaintEventArgs e)
		{
			_paint++;
		}

		void Reset ()
		{
			_invalidated = 0;
			_paint = 0;
		}
	}
}
