//
// System.Web.UI.WebControls.HyperLinkColumn.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
//

using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class HyperLinkColumn: DataGridColumn
	{
		PropertyDescriptor textFieldDescriptor;
		PropertyDescriptor urlFieldDescriptor;

		public HyperLinkColumn ()
		{
		}

		[DefaultValue (""), WebCategory ("Misc")]
		[WebSysDescription ("The field that gets data-bound to the NavigateUrl.")]
		public virtual string DataNavigateUrlField {
			get {
				object o = ViewState ["DataNavigateUrlField"];
				if (o != null)
					return (string) o;
				return String.Empty;
			}

			set {
				ViewState ["DataNavigateUrlField"] = value;
				OnColumnChanged ();
			}
		}

		// LAMESPEC should use WebSysDescription as all others do, but MS uses Description here

		[DefaultValue (""), WebCategory ("Misc")]
		[Description ("The formatting rule for the text content that gets data-bound to the NavigateUrl.")]
		public virtual string DataNavigateUrlFormatString {
			get {
				object o = ViewState ["DataNavigateUrlFormatString"];
				if (o != null)
					return (string) o;
				return String.Empty;
			}

			set {
				ViewState ["DataNavigateUrlFormatString"] = value;
				OnColumnChanged ();
			}
		}

		[DefaultValue (""), WebCategory ("Misc")]
		[WebSysDescription ("The field that gets data-bound to the Text property.")]
		public virtual string DataTextField {
			get {
				object o = ViewState ["DataTextField"];
				if (o != null)
					return (string) o;
				return String.Empty;
			}
			set {
				ViewState ["DataTextField"] = value;
				OnColumnChanged ();
			}
		}

		// LAMESPEC should use WebSysDescription as all others do, but MS uses Description here

		[DefaultValue (""), WebCategory ("Misc")]
		[Description ("The formatting rule for the text content that gets data-bound to the Text property.")]
		public virtual string DataTextFormatString {
			get {
				object o = ViewState ["DataTextFormatString"];
				if (o != null)
					return (string) o;
				return String.Empty;
			}

			set {
				ViewState ["DataTextFormatString"] = value;
				OnColumnChanged ();
			}
		}

		[DefaultValue (""), WebCategory ("Misc")]
		[WebSysDescription ("The URL that this hyperlink links to.")]
		public virtual string NavigateUrl {
			get {
				object o = ViewState ["NavigateUrl"];
				if (o != null)
					return (string) o;
				return String.Empty;
			}

			set {
				ViewState ["NavigateUrl"] = value;
				OnColumnChanged ();
			}
		}

		[DefaultValue (""), WebCategory ("Misc")]
		[WebSysDescription ("The target frame in which the NavigateUrl property should be opened.")]
		public virtual string Target {
			get {
				object o = ViewState ["Target"];
				if (o != null)
					return (string) o;
				return String.Empty;
			}

			set {
				ViewState ["Target"] = value;
				OnColumnChanged ();
			}
		}

		[DefaultValue (""), WebCategory ("Misc")]
		[WebSysDescription ("The Text for the hyperlink.")]
		public virtual string Text {
			get {
				object o = ViewState ["Text"];
				if (o != null)
					return (string) o;
				return String.Empty;
			}

			set {
				ViewState ["Text"] = value;
				OnColumnChanged ();
			}
		}

		public override void Initialize ()
		{
			textFieldDescriptor = null;
			urlFieldDescriptor  = null;
			base.Initialize ();
		}

		public override void InitializeCell (TableCell cell, int columnIndex, ListItemType itemType)
		{
			base.InitializeCell (cell, columnIndex, itemType);

			if (itemType != ListItemType.Header && itemType != ListItemType.Footer) {
				HyperLink toDisplay = new HyperLink ();
				toDisplay.Text = Text;
				toDisplay.NavigateUrl = NavigateUrl;
				toDisplay.Target = Target;

				if (DataTextField.Length > 0 || DataNavigateUrlField.Length > 0)
					toDisplay.DataBinding += new EventHandler (OnDataBindHyperLinkColumn);

				cell.Controls.Add (toDisplay);
			}
		}

		private void OnDataBindHyperLinkColumn (object sender, EventArgs e)
		{
			HyperLink link = (HyperLink) sender;
			object item = ((DataGridItem) link.NamingContainer).DataItem;

			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties (item);
			if (textFieldDescriptor == null)
				textFieldDescriptor = properties.Find (DataTextField, true);

			if (urlFieldDescriptor == null)
				urlFieldDescriptor = properties.Find (DataNavigateUrlField, true);

			if (DataTextField.Length > 0 && textFieldDescriptor == null && !DesignMode)
				throw new HttpException (HttpRuntime.FormatResourceString (
								"Field_Not_Found", DataTextField));

			if (DataNavigateUrlField.Length > 0 && urlFieldDescriptor == null && !DesignMode)
				throw new HttpException (HttpRuntime.FormatResourceString (
								"Field_Not_Found", DataNavigateUrlField));

			if (textFieldDescriptor != null) {
				link.Text = FormatDataTextValue (textFieldDescriptor.GetValue (item));
			} else {
				link.Text = Text;
			}

			if (urlFieldDescriptor != null) {
				link.NavigateUrl = FormatDataNavigateUrlValue (urlFieldDescriptor.GetValue (item));
			} else {
				link.NavigateUrl = "url";
			}
		}

		protected virtual string FormatDataNavigateUrlValue (object dataUrlValue)
		{
			if (dataUrlValue == null)
				return String.Empty;

			string retVal;
			if (DataNavigateUrlFormatString.Length > 0) {
				retVal = String.Format (DataNavigateUrlFormatString, dataUrlValue);
			} else {
				retVal = dataUrlValue.ToString ();
			}

			return retVal;
		}

		protected virtual string FormatDataTextValue (object dataTextValue)
		{
			if (dataTextValue == null)
				return String.Empty;

			string retVal;
			if (DataTextFormatString.Length > 0) {
				retVal = String.Format (DataTextFormatString, dataTextValue);
			} else {
				retVal = dataTextValue.ToString ();
			}

			return retVal;
		}
	}
}

