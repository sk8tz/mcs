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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace System.Web.UI.WebControls {
#if NET_2_0
	[Designer ("System.Web.UI.Design.WebControls.PreviewControlDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
#endif
	public class ValidationSummary : WebControl {
		#region Public Constructors
		public ValidationSummary() : base(HtmlTextWriterTag.Div) {
			this.ForeColor = Color.Red;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
#if ONLY_1_1
		[Bindable(true)]
#endif		
		[DefaultValue(ValidationSummaryDisplayMode.BulletList)]
		public ValidationSummaryDisplayMode DisplayMode {
			get {
				object obj;

				obj = ViewState["DisplayMode"];
				if (obj != null) {
					return (ValidationSummaryDisplayMode)obj;
				}
				return ValidationSummaryDisplayMode.BulletList;
			}

			set {
				ViewState["DisplayMode"] = value;
			}
		}

		[DefaultValue(true)]
#if NET_2_0
		[Themeable (false)]
#endif		
		public bool EnableClientScript {
			get {
				return ViewState.GetBool("EnableClientScript", true);
			}

			set {
				ViewState["EnableClientScript"] = value;
			}
		}

		[DefaultValue("Color [Red]")]
		public override System.Drawing.Color ForeColor {
			get {
				return base.ForeColor;
			}

			set {
				base.ForeColor = value;
			}
		}

#if ONLY_1_1
		[Bindable(true)]
#endif		
		[DefaultValue("")]
#if NET_2_0
		[Localizable (true)]
#endif		
		public string HeaderText {
			get {
				return ViewState.GetString("HeaderText", string.Empty);
			}

			set {
				ViewState["HeaderText"] = value;
			}
		}

#if ONLY_1_1
		[Bindable(true)]
#endif		
		[DefaultValue(false)]
		public bool ShowMessageBox {
			get {
				return ViewState.GetBool("ShowMessageBox", false);
			}

			set {
				ViewState["ShowMessageBox"] = value;
			}
		}

#if ONLY_1_1
		[Bindable(true)]
#endif		
		[DefaultValue(true)]
		public bool ShowSummary {
			get {
				return ViewState.GetBool("ShowSummary", true);
			}

			set {
				ViewState["ShowSummary"] = value;
			}
		}

#if NET_2_0
		[DefaultValue ("")]
		[Themeable (false)]
		public string ValidationGroup
		{
			get {
				return ViewState.GetString("ValidationGroup", string.Empty);
			}
			set {
				ViewState["ValidationGroup"] = value;
			}
		}
#endif		
		
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		protected override void AddAttributesToRender(HtmlTextWriter writer) {
			base.AddAttributesToRender (writer);

			if (EnableClientScript && pre_render_called) {
				if (HeaderText != "")
					writer.AddAttribute ("headertext", HeaderText);

				if (ShowMessageBox)
					writer.AddAttribute ("showmessagebox", ShowMessageBox.ToString());

				if (ShowSummary == false)
					writer.AddAttribute ("showsummary", ShowSummary.ToString());

				if (DisplayMode != ValidationSummaryDisplayMode.BulletList)
					writer.AddAttribute ("displaymode", DisplayMode.ToString());

				writer.AddStyleAttribute ("display", "none");
			}
		}

		bool pre_render_called = false;

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void OnPreRender(EventArgs e) {
			base.OnPreRender (e);

			pre_render_called = true;
		}

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void Render(HtmlTextWriter writer) {
			ValidatorCollection	validators;
			ArrayList		errors;

			// First, figure out if there's even data to deal with
#if NET_2_0
			validators = Page.GetValidators (ValidationGroup);
#else
			validators = Page.Validators;
#endif
			if (validators.Count == 0) {
				return;
			}

			if (EnableClientScript && pre_render_called) {
				Page.RegisterArrayDeclaration ("Page_ValidationSummaries",
							       String.Format ("document.getElementById ('{0}')", ID));
			}

			// We have validators
			errors = new ArrayList(validators.Count);
			for (int i = 0; i < validators.Count; i++) {
				if (!validators[i].IsValid) {
					errors.Add(validators[i].ErrorMessage);
				}
			}

			if ((ShowSummary && errors.Count > 0) || (EnableClientScript && pre_render_called))
				base.RenderBeginTag(writer);

			if (ShowSummary && errors.Count > 0) {
				switch(DisplayMode) {
					case ValidationSummaryDisplayMode.BulletList: {
						if (HeaderText.Length > 0) {
							writer.Write(HeaderText);
						}

						writer.Write("<ul>");
						for (int i = 0; i < errors.Count; i++) {
							writer.Write("<li>");
							writer.Write(errors[i]);
							writer.Write("</li>");
						}
						writer.Write("</ul>");
						break;
					}

					case ValidationSummaryDisplayMode.List: {
						if (HeaderText.Length > 0) {
							writer.Write(HeaderText);
#if NET_2_0
							writer.Write("<br />");
#else
							writer.Write("<br>");
#endif
						}

						for (int i = 0; i < errors.Count; i++) {
							writer.Write(errors[i]);
#if NET_2_0
							writer.Write("<br />");
#else
							writer.Write("<br>");
#endif
						}
						break;
					}

					case ValidationSummaryDisplayMode.SingleParagraph: {
						if (HeaderText.Length > 0) {
							writer.Write(HeaderText);
							writer.Write(" ");
						}

						for (int i = 0; i < errors.Count; i++) {
							writer.Write(errors[i]);
							writer.Write(" ");
						}
#if NET_2_0
						writer.Write("<br />");
#else
						writer.Write("<br>");
#endif

						break;
					}
				}
			}

			if ((ShowSummary && errors.Count > 0) || (EnableClientScript && pre_render_called))
				base.RenderEndTag(writer);
		}
		#endregion	// Public Instance Methods
	}
}
