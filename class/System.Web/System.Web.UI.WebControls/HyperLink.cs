//
// System.Web.UI.WebControls.HyperLink.cs
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
using System.Web;
using System.Web.UI;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.Web.UI.WebControls
{
	[DefaultProperty("Text")]
	[ControlBuilder(typeof(HyperLinkControlBuilder))]
	[Designer("System.Web.UI.Design.WebControls.HyperLinkDesigner, " + Consts.AssemblySystem_Design, typeof (IDesigner))]
	[DataBindingHandler("System.Web.UI.Design.HyperLinkDataBindingHandler, " + Consts.AssemblySystem_Design)]
	[ParseChildren(false)]
	[ToolboxData("<{0}:HyperLink runat=\"server\">HyperLink</{0}:HyperLink>")]
	public class HyperLink: WebControl
	{
		bool textSet;

		public HyperLink(): base(HtmlTextWriterTag.A)
		{
		}

#if NET_2_0
		[UrlPropertyAttribute]
#endif
		[DefaultValue (""), Bindable (true), WebCategory ("Appearance")]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		[WebSysDescription ("The URL to the image file.")]
		public virtual string ImageUrl
		{
			get
			{
				object o = ViewState["ImageUrl"];
				if(o!=null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["ImageUrl"] = value;
			}
		}

#if NET_2_0
		[UrlPropertyAttribute]
#endif
		[DefaultValue (""), Bindable (true), WebCategory ("Navigation")]
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		[WebSysDescription ("The URL to navigate to.")]
		public string NavigateUrl
		{
			get
			{
				object o = ViewState["NavigateUrl"];
				if(o!=null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["NavigateUrl"] = value;
			}
		}

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (""), WebCategory ("Navigation")]
		[TypeConverter (typeof (TargetConverter))]
		[WebSysDescription ("The target frame in which the navigation target should be opened.")]
		public string Target
		{
			get
			{
				object o = ViewState["Target"];
				if(o!=null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["Target"] = value;
			}
		}

#if NET_2_0
		[Localizable (true)]
#endif
		[DefaultValue (""), Bindable (true), WebCategory ("Appearance")]
		[PersistenceMode (PersistenceMode.InnerDefaultProperty)]
		[WebSysDescription ("The text that should be shown on this HyperLink.")]
		public virtual string Text
		{
			get {
				object o = ViewState ["Text"];
				if (o != null)
					return (string) o;

				return String.Empty;
			}
			set {
				if (HasControls())
					Controls.Clear();
				ViewState["Text"] = value;
				textSet = true;
			}
		}

#if NET_2_0
		[BindableAttribute (true)]
		[LocalizableAttribute (true)]
		[DefaultValueAttribute ("")]
		public string SoftkeyLabel {
			get {
				string text = (string)ViewState["SoftkeyLabel"];
				if (text!=null) return text;
				return String.Empty;
			}
			set {
				ViewState["SoftkeyLabel"] = value;
			}
		}
#endif
		
		string InternalText
		{
			get { return Text; }
			set { ViewState["Text"] = value; }
		}

		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			base.AddAttributesToRender(writer);
			if(NavigateUrl.Length > 0)
			{
				string url = ResolveUrl (NavigateUrl);
				writer.AddAttribute(HtmlTextWriterAttribute.Href, url);
			}
			if(Target.Length > 0)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Target, Target);
			}
		}

		protected override void AddParsedSubObject(object obj)
		{
			if(HasControls())
			{
				base.AddParsedSubObject(obj);
				return;
			}
			if(obj is LiteralControl)
			{
				// This is a hack to workaround the behaviour of the code generator, which
				// may split a text in several LiteralControls if there's a special character
				// such as '<' in it.
				if (textSet) {
					Text = ((LiteralControl)obj).Text;
					textSet = false;
				} else {
					InternalText += ((LiteralControl)obj).Text;
				}
				//

				return;
			}
			if(Text.Length > 0)
			{
				base.AddParsedSubObject(new LiteralControl (Text));
				Text = String.Empty;
			}
			base.AddParsedSubObject (obj);
		}

		protected override void LoadViewState(object savedState)
		{
			if(savedState != null)
			{
				base.LoadViewState(savedState);
				object o = ViewState["Text"];
				if(o!=null)
					Text = (string)o;
			}
		}

		protected override void RenderContents(HtmlTextWriter writer)
		{
			if(ImageUrl.Length > 0)
			{
				Image img = new Image();
				img.ImageUrl = ResolveUrl(ImageUrl);
				if(ToolTip.Length > 0)
					img.ToolTip = ToolTip;
				if(Text.Length > 0)
					img.AlternateText = Text;
				img.RenderControl(writer);
				return;
			}
			if(HasControls())
			{
				base.RenderContents(writer);
				return;
			}
			writer.Write(Text);
		}
	}
}
