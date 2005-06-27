/*	System.Web.UI.HtmlControls
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Globalization;
using System.Collections.Specialized;

namespace System.Web.UI.HtmlControls{
	[ValidationProperty("Value")]
	public class HtmlInputFile : HtmlInputControl, IPostBackDataHandler{
		
		public HtmlInputFile():base("file"){}
		
		bool IPostBackDataHandler.LoadPostData (string postDataKey,
						       NameValueCollection postCollection)
		{
			return false;
		}
		
		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
		}
		
		[DefaultValue("")]
		[WebCategory("Behavior")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string Accept{
			get{
				string attr = Attributes["accept"];
				if (attr != null)
					return attr;
				return String.Empty;
			}
			set{
				Attributes["accept"] = AttributeToString(value);
			}
		}
		
		[DefaultValue("")]
		[WebCategory("Behavior")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int MaxLength{
			get{
				string attr = Attributes["maxlength"];
				if (attr != null)
					return Int32.Parse(attr, CultureInfo.InvariantCulture);
				return -1;
			}
			set{
				Attributes["maxlength"] = AttributeToString(value);
			}
		}
		
		[DefaultValue("")]
		[WebCategory("Appearance")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int Size{
			get{
				string attr = Attributes["size"];
				if (attr != null)
					return Int32.Parse(attr, CultureInfo.InvariantCulture);
				return -1;
			}
			set{
				Attributes["size"] = AttributeToString(value);
			}
		}
		
		[DefaultValue("")]
		[WebCategory("Misc")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public HttpPostedFile PostedFile{
			get{
				return Context.Request.Files[RenderedName];
			}
		}

		protected override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
			for (Control ctrl = this.Parent; ctrl != null && !(ctrl is Page); ctrl = ctrl.Parent) {
				if (!(ctrl is HtmlForm))
					continue;

				HtmlForm form = (HtmlForm) ctrl;
				if (form.Enctype == "")
					form.Enctype = "multipart/form-data";
				break;
			}
		}
#if NET_1_1
		[Browsable (false)]
		public override string Value {
			get {
				HttpPostedFile file = PostedFile;
				if (file == null)
					return "";

				return file.FileName;
			}

			set {
				throw new NotSupportedException ();
			}
		}
#endif
		
	} // class HtmlInputFile
} // namespace System.Web.UI.HtmlControls

