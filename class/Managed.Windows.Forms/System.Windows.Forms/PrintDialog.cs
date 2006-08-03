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
// Copyright (c) 2005-2006 Novell, Inc.
//
// Authors:
//	Someone
//	Jonathan Chambers (jonathan.chambers@ansys.com)
//	Jordi Mas i Hernandez (jordimash@gmail.com)
//

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Reflection;

namespace System.Windows.Forms
{
	[DefaultProperty("Document")]
	public sealed class PrintDialog : CommonDialog {
		PrintDocument document;
		bool allow_current_page;
		bool allow_print_to_file;
		bool allow_selection;
		bool allow_some_pages;
		bool show_help;
		bool show_network;
		bool print_to_file;
		PrinterSettings current_settings;

		private Button cancel_button;
		private Button accept_button;
		private Button help_button;
		private ComboBox printer_combo;
		private RadioButton radio_all;
		private RadioButton radio_pages;
		private RadioButton radio_sel;
		private PrinterSettings.StringCollection installed_printers;
		private PrinterSettings default_printer_settings;
		private TextBox txtFrom;
		private TextBox txtTo;
		private Label labelTo;
		private Label labelFrom;
		private CheckBox chkbox_print;
		private NumericUpDown updown_copies;
		private CheckBox chkbox_collate;
		private Label label_status;
		private Label label_type;
		private Label label_where;
		private Label label_comment;

		public PrintDialog ()
		{
			help_button = null;
			installed_printers = System.Drawing.Printing.PrinterSettings.InstalledPrinters;

			form.Text = "Print";
			CreateFormControls ();
			Reset ();
		}

		public override void Reset ()
		{
			current_settings = new PrinterSettings ();
			AllowPrintToFile = true;
			AllowSelection = false;
			AllowSomePages = false;
			PrintToFile = false;
			ShowHelp = false;
			ShowNetwork = true;
		}

#if NET_2_0
		public bool AllowCurrentPage {
			get {
				return allow_current_page;
			}

			set {
				allow_current_page = value;
				radio_pages.Enabled = value;
			}
		}
#endif

		[DefaultValue(true)]
		public bool AllowPrintToFile {
			get {
				return allow_print_to_file;
			}

			set {
				allow_print_to_file = value;
				chkbox_print.Enabled = value;
			}
		}

		[DefaultValue(false)]
		public bool AllowSelection {
			get {
				return allow_selection;
			}

			set {
				allow_selection = value;
				radio_sel.Enabled = value;
			}
		}

		[DefaultValue(false)]
		public bool AllowSomePages {
			get {
				return allow_some_pages;
			}

			set {
				allow_some_pages = value;
				radio_pages.Enabled = value;
				txtFrom.Enabled = value;
				txtTo.Enabled = value;
				labelTo.Enabled = value;
				labelFrom.Enabled = value;

				if (current_settings != null) {
					txtFrom.Text = current_settings.FromPage.ToString ();
					txtTo.Text = current_settings.ToPage.ToString ();
				}
			}
		}

		[DefaultValue(null)]
		public PrintDocument Document {
			get {
				return document;
			}

			set {
				document = value;
				current_settings = value == null ? new PrinterSettings () : value.PrinterSettings;
			}
		}

		[Browsable(false)]
		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public PrinterSettings PrinterSettings {
			get {
				return current_settings;
			}

			set {
				if (value != null && value == current_settings)
					return;

				current_settings = value == null ? new PrinterSettings () : value;
				document = null;
			}
		}

		[DefaultValue(false)]
		public bool PrintToFile {
			get {
				return print_to_file;
			}

			set {
				print_to_file = value;
			}
		}

		[DefaultValue(true)]
		public bool ShowNetwork {
			get {
				return show_network;
			}

			set {
				show_network = value;
			}
		}

		[DefaultValue(false)]
		public bool ShowHelp {
			get {
				return show_help;
			}

			set {
				show_help = value;
				ShowHelpButton ();
			}
		}

		protected override bool RunDialog (IntPtr hwnd)
		{
			if (Document == null && PrinterSettings == null)
				throw new ArgumentException ("PrintDialog needs a Docouement or PrinterSettings object to display");

			if (allow_some_pages && current_settings.FromPage > current_settings.ToPage)
				throw new ArgumentException ("FromPage out of range");

			if (allow_some_pages && current_settings != null) {
				txtFrom.Text = current_settings.FromPage.ToString ();
				txtTo.Text = current_settings.ToPage.ToString ();
			}

			updown_copies.Value = current_settings.Copies;
			chkbox_collate.Enabled = (updown_copies.Value > 1) ? true : false;

			if (show_help) {
				ShowHelpButton ();
			}

			SetPrinterDetails ();

			return true;
		}

		private void OnClickCancelButton (object sender, EventArgs e)
		{
			form.DialogResult = DialogResult.Cancel;
		}

		private void OnClickOkButton (object sender, EventArgs e)
		{
			int from = -1, to = -1;

			if (allow_some_pages) {
				if (radio_pages.Checked) {
					try {
						from = Int32.Parse (txtFrom.Text);
						to = Int32.Parse (txtTo.Text);
					}
					catch {
						MessageBox.Show ("From/To values should be numeric", "Print",
								 MessageBoxButtons.OK, MessageBoxIcon.Warning);
						return;
					}

					if (from > to) {
						MessageBox.Show ("From value cannot be greater than To value", "Print",
								 MessageBoxButtons.OK, MessageBoxIcon.Warning);
						return;
					}

					if (to < current_settings.MinimumPage || to > current_settings.MaximumPage) {
						MessageBox.Show ("To value is not within the page range\n" +
								 "Enter a number between " + current_settings.MinimumPage +
								 " and " + current_settings.MaximumPage, "Print",
								 MessageBoxButtons.OK, MessageBoxIcon.Warning);
						return;
					}

					if (from < current_settings.MinimumPage || from > current_settings.MaximumPage) {
						txtTo.Focus ();
						MessageBox.Show ("From value is not within the page range\n" +
								 "Enter a number between " + current_settings.MinimumPage +
								 " and " + current_settings.MaximumPage, "Print",
								 MessageBoxButtons.OK, MessageBoxIcon.Warning);
						return;
					}
				}
			}

			if (radio_all.Checked == true) {
				current_settings.PrintRange = PrintRange.AllPages;
			} else {
				if (radio_pages.Checked == true) {
					current_settings.PrintRange = PrintRange.SomePages;
				} else {
					if (radio_sel.Checked == true) {
						current_settings.PrintRange = PrintRange.Selection;
					}
				}
			}

			current_settings.Copies = (short) updown_copies.Value;
			if (current_settings.PrintRange == PrintRange.SomePages) {
				current_settings.FromPage = from;
				current_settings.ToPage = to;
			}
			current_settings.Collate = chkbox_collate.Checked;

			if (allow_print_to_file) {
				current_settings.PrintToFile = chkbox_print.Checked;
			}

			form.DialogResult = DialogResult.OK;

			if (printer_combo.SelectedItem != null)
				current_settings.PrinterName = (string) printer_combo.SelectedItem;

			if (document != null) {
				document.PrintController = new PrintControllerWithStatusDialog (document.PrintController);
				document.PrinterSettings = current_settings;
			}
		}

		private void ShowHelpButton ()
		{
			if (help_button == null) {
				help_button = new Button ();
				help_button.TabIndex = 60;

				help_button.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
				help_button.FlatStyle = FlatStyle.System;
				help_button.Location = new Point (20, 270);
				help_button.Text = "&Help";
				help_button.FlatStyle = FlatStyle.System;
				form.Controls.Add (help_button);
			}

			help_button.Visible = show_help;
		}

		private void OnUpDownValueChanged (object sender, System.EventArgs e)
		{
			chkbox_collate.Enabled = (updown_copies.Value > 1) ? true : false;
		}

		private void CreateFormControls ()
		{
			form.SuspendLayout ();

			GroupBox group_box_prn = new GroupBox ();
			group_box_prn.Location = new Point (10, 8);
			group_box_prn.Text = "Printer";
			group_box_prn.Size = new Size (420, 145);

			GroupBox group_box_range = new GroupBox ();
			group_box_range.Location = new Point (10, 155);
			group_box_range.Text = "Print range";
			group_box_range.Size = new Size (240, 100);

			GroupBox group_box_copies = new GroupBox ();
			group_box_copies.Location = new Point (265, 155);
			group_box_copies.Text = "Copies";
			group_box_copies.Size = new Size (165, 100);

			// Accept button
			accept_button = new Button ();
			form.AcceptButton = accept_button;
			accept_button.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
			accept_button.FlatStyle = FlatStyle.System;
			accept_button.Location = new Point (265, 270);
			accept_button.Text = "OK";
			accept_button.FlatStyle = FlatStyle.System;
			accept_button.Click += new EventHandler (OnClickOkButton);

			// Cancel button
			cancel_button = new Button ();
			form.CancelButton = cancel_button;
			cancel_button.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
			cancel_button.FlatStyle = FlatStyle.System;
			cancel_button.Location = new Point (350, 270);
			cancel_button.Text = "Cancel";
			cancel_button.FlatStyle = FlatStyle.System;
			cancel_button.Click += new EventHandler (OnClickCancelButton);

			// Static controls
			Label label = new Label ();
			label.AutoSize = true;
			label.Text = "&Name:";
			label.Location = new Point (20, 33);
			group_box_prn.Controls.Add (label);

			label = new Label ();
			label.Text = "Status:";
			label.AutoSize = true;
			label.Location = new Point (20, 60);
			group_box_prn.Controls.Add (label);

			label_status = new Label ();
			label_status.AutoSize = true;
			label_status.Location = new Point (80, 60);
			group_box_prn.Controls.Add (label_status);

			label = new Label ();
			label.Text = "Type:";
			label.AutoSize = true;
			label.Location = new Point (20, 80);
			group_box_prn.Controls.Add (label);

			label_type = new Label ();
			label_type.AutoSize = true;
			label_type.Location = new Point (80, 80);
			group_box_prn.Controls.Add (label_type);

			label = new Label ();
			label.Text = "Where:";
			label.AutoSize = true;
			label.Location = new Point (20, 100);
			group_box_prn.Controls.Add (label);

			label = new Label ();
			label.Text = "Comment:";
			label.AutoSize = true;
			label.Location = new Point (20, 120);
			group_box_prn.Controls.Add (label);

			label_comment = new Label ();
			label_comment.AutoSize = true;
			label_comment.Location = new Point (80, 120);

			radio_all = new RadioButton ();
			radio_all.TabIndex = 21;
			radio_all.Location = new Point (20, 20);
			radio_all.Text = "&All";
			radio_all.Checked = true;
			group_box_range.Controls.Add (radio_all);

			radio_pages = new RadioButton ();
			radio_pages.TabIndex = 22;
			radio_pages.Location = new Point (20, 46);
			radio_pages.Text = "Pa&ges";
			radio_pages.Width = 60;
			group_box_range.Controls.Add (radio_pages);

			radio_sel = new RadioButton ();
			radio_sel.TabIndex = 23;
			radio_sel.Location = new Point (20, 72);
			radio_sel.Text = "&Selection";
			group_box_range.Controls.Add (radio_sel);

			labelFrom = new Label ();
			labelFrom.Text = "&from:";
			labelFrom.TabIndex = 24;
			labelFrom.AutoSize = true;
			labelFrom.Location = new Point (80, 50);
			group_box_range.Controls.Add (labelFrom);

			txtFrom = new TextBox ();
			txtFrom.TabIndex = 25;
			txtFrom.Location = new Point (120, 50);
			txtFrom.Width = 40;
			group_box_range.Controls.Add (txtFrom);

			labelTo = new Label ();
			labelTo.Text = "&to:";
			labelTo.TabIndex = 26;
			labelTo.AutoSize = true;
			labelTo.Location = new Point (170, 50);
			group_box_range.Controls.Add (labelTo);

			txtTo = new TextBox ();
			txtTo.TabIndex = 27;
			txtTo.Location = new Point (190, 50);
			txtTo.Width = 40;
			group_box_range.Controls.Add (txtTo);

			chkbox_print = new CheckBox ();
			chkbox_print.Location = new Point (305, 115);
			chkbox_print.Text = "Print to fil&e";

			label = new Label ();
			label.Text = "Number of &copies:";
			label.AutoSize = true;
			label.Location = new Point (20, 20);
			group_box_copies.Controls.Add (label);

			updown_copies = new NumericUpDown ();
			updown_copies.TabIndex = 31;
			updown_copies.Location = new Point (120, 20);
			updown_copies.Minimum = 1;
			group_box_copies.Controls.Add (updown_copies);
			updown_copies.ValueChanged += new System.EventHandler (OnUpDownValueChanged);
			updown_copies.Size = new System.Drawing.Size (40, 20);

			chkbox_collate = new CheckBox ();
			chkbox_collate.TabIndex = 32;
			chkbox_collate.Location = new Point (20, 40);
			chkbox_collate.Text = "C&ollate";
			chkbox_collate.Width = 80;
			group_box_copies.Controls.Add (chkbox_collate);



			// Printer combo
			printer_combo = new ComboBox ();
			printer_combo.DropDownStyle = ComboBoxStyle.DropDownList;
			printer_combo.Location = new Point (80, 32);
			printer_combo.Width = 220;
			printer_combo.SelectedIndexChanged += new EventHandler (OnPrinterSelectedIndexChanged);

			default_printer_settings = new PrinterSettings ();
			for (int i = 0; i < installed_printers.Count; i++) {
				printer_combo.Items.Add (installed_printers[i]);
				if (installed_printers[i] == default_printer_settings.PrinterName)
					printer_combo.SelectedItem = installed_printers[i];
			}
			printer_combo.TabIndex = 11;
			chkbox_print.TabIndex = 12;
			group_box_prn.Controls.Add (printer_combo);
			group_box_prn.Controls.Add (chkbox_print);

			form.Size =  new Size (450, 327); // 384
			form.FormBorderStyle = FormBorderStyle.FixedDialog;
			form.MaximizeBox = false;
			group_box_prn.TabIndex = 10;
			group_box_range.TabIndex = 20;
			group_box_copies.TabIndex = 30;
			accept_button.TabIndex = 40;
			cancel_button.TabIndex = 50;
			form.Controls.Add (group_box_prn);
			form.Controls.Add (group_box_range);
			form.Controls.Add (group_box_copies);
			form.Controls.Add (accept_button);
			form.Controls.Add (cancel_button);
			form.ResumeLayout (false);
		}

		private void OnPrinterSelectedIndexChanged (object sender,  System.EventArgs e)
    		{    			
    			SetPrinterDetails ();
    		}

		private void SetPrinterDetails ()
		{
			try
			{
				string printer, port = string.Empty, type = string.Empty;
				string status = string.Empty, comment = string.Empty;
				Type sysprn = Type.GetType ("System.Drawing.Printing.SysPrn, System.Drawing");
				MethodInfo dlg_info = sysprn.GetMethod ("GetPrintDialogInfo", BindingFlags.Static | BindingFlags.NonPublic);

				printer = (string) printer_combo.SelectedItem;

				if (printer != null) {
					object[] args  = new object [5];
					args[0] = printer;
					args[1] = port;
					args[2] = type;
					args[3] = status;
					args[4] = comment;
					dlg_info.Invoke (null, args);
					port = (string) args[1];
					type = (string) args[2];
					status = (string) args[3];
					comment = (string) args[4];
				}

				label_status.Text = status;
				label_type.Text = type;
				label_where.Text = port;
				label_comment.Text = comment;

			}
			catch  {
			}
		}
	}
}
