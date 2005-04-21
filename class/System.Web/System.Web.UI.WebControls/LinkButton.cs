//
// System.Web.UI.WebControls.LinkButton.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
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
	[DefaultEvent("Click")]
	[DefaultProperty("Text")]
	[ControlBuilder(typeof(LinkButtonControlBuilder))]
	[Designer("System.Web.UI.Design.WebControls.LinkButtonDesigner, " + Consts.AssemblySystem_Design, typeof (IDesigner))]
	[DataBindingHandler("System.Web.UI.Design.TextDataBindingHandler, " + Consts.AssemblySystem_Design)]
	[ParseChildren(false)]
	[ToolboxData("<{0}:LinkButton runat=\"server\">LinkButton</{0}:LinkButton>")]
	public class LinkButton : WebControl, IPostBackEventHandler
#if NET_2_0
		, IButtonControl
#endif
	{
		private static readonly object ClickEvent   = new object();
		private static readonly object CommandEvent = new object();

		public LinkButton () : base (HtmlTextWriterTag.A)
		{
		}

#if NET_2_0
		[ThemeableAttribute (false)]
#else
		[Bindable (false)]
#endif
		[DefaultValue (true), WebCategory ("Behavior")]
		[WebSysDescription ("Determines if validation is performed when clicked.")]
		public bool CausesValidation
		{
			get {
				object o = ViewState ["CausesValidation"];
				return (o == null) ? true : (bool) o;
			}

			set { ViewState ["CausesValidation"] = value; }
		}

#if NET_2_0
		[ThemeableAttribute (false)]
#endif
		[DefaultValue (""), Bindable (true), WebCategory ("Behavior")]
		[WebSysDescription ("An argument for the Command of this control.")]
		public string CommandArgument
		{
			get {
				object o = ViewState ["CommandArgument"];
				return (o == null) ? String.Empty : (string) o;
			}

			set { ViewState ["CommandArgument"] = value; }
		}

#if NET_2_0
		[ThemeableAttribute (false)]
#endif
		[DefaultValue (""), WebCategory ("Behavior")]
		[WebSysDescription ("The name of the Command of this control.")]
		public string CommandName
		{
			get {
				object o = ViewState ["CommandName"];
				return (o == null) ? String.Empty : (string) o;
			}

			set { ViewState ["CommandName"] = value; }
		}

#if NET_2_0
		[Localizable (true)]
#endif
		[DefaultValue (""), Bindable (true), WebCategory ("Appearance")]
		[PersistenceMode (PersistenceMode.InnerDefaultProperty)]
		[WebSysDescription ("The text that should be shown on this LinkButton.")]
		public virtual string Text
		{
			get {
				object o = ViewState ["Text"];
				return (o == null) ? String.Empty : (string) o;
			}

			set { ViewState ["Text"] = value; }
		}
		
#if NET_2_0
		[ThemeableAttribute (false)]
		[EditorAttribute ("System.Web.UI.Design.UrlEditor, System.Design, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		[DefaultValueAttribute ("")]
		[UrlProperty]
		public string PostBackUrl {
			get {
				string text = (string)ViewState["PostBackUrl"];
				if (text!=null) return text;
				return String.Empty;
			}
			set {
				ViewState["PostBackUrl"] = value;
			}
		}
		
		[DefaultValueAttribute ("")]
		[ThemeableAttribute (false)]
		[WebCategoryAttribute ("Behavior")]
		public string ValidationGroup {
			get {
				string text = (string)ViewState["ValidationGroup"];
				if (text!=null) return text;
				return String.Empty;
			}
			set {
				ViewState["ValidationGroup"] = value;
			}
		}
		
		[DefaultValueAttribute ("")]
		[ThemeableAttribute (false)]
		[WebCategoryAttribute ("Behavior")]
		public string OnClientClick {
			get {
				string text = (string)ViewState["OnClientClick"];
				if (text!=null) return text;
				return String.Empty;
			}
			set {
				ViewState["OnClientClick"] = value;
			}
		}
		
		protected virtual PostBackOptions GetPostBackOptions ()
		{
			PostBackOptions ops = new PostBackOptions (this);
			if (PostBackUrl != "")
				ops.ActionUrl = PostBackUrl;
			ops.PerformValidation = Page.Validators.Count > 0 && CausesValidation;
			if (ops.PerformValidation && ValidationGroup != "")
				ops.ValidationGroup = ValidationGroup;
			ops.RequiresJavaScriptProtocol = false;
			return ops;
		}
#endif


		[WebCategory ("Action")]
		[WebSysDescription ("Raised when the LinkButton is clicked.")]
		public event EventHandler Click
		{
			add { Events.AddHandler(ClickEvent, value); }
			remove { Events.RemoveHandler(ClickEvent, value); }
		}

		[WebCategory ("Action")]
		[WebSysDescription ("Raised when a LinkButton Command is executed.")]
		public event CommandEventHandler Command
		{
			add { Events.AddHandler(CommandEvent, value); }
			remove { Events.RemoveHandler(CommandEvent, value); }
		}

		protected virtual void OnClick (EventArgs e)
		{
			if(Events != null){
				EventHandler eh = (EventHandler) (Events [ClickEvent]);
				if (eh != null)
					eh (this, e);
			}
		}

		protected virtual void OnCommand (CommandEventArgs e)
		{
			if(Events != null){
				CommandEventHandler ceh = (CommandEventHandler) (Events [CommandEvent]);
				if (ceh != null)
					ceh (this, e);
			}
			RaiseBubbleEvent (this, e);
		}

		protected override void OnPreRender (EventArgs e)
		{
			base.OnPreRender(e);
		}

#if NET_2_0
		void IPostBackEventHandler.RaisePostBackEvent (string eventArgument)
		{
			RaisePostBackEvent (eventArgument);
		}
		
		protected virtual void RaisePostBackEvent (string eventArgument)
		{
			if (CausesValidation)
				Page.Validate (ValidationGroup);
			OnClick (EventArgs.Empty);
			OnCommand (new CommandEventArgs (CommandName, CommandArgument));
		}
#else
		void IPostBackEventHandler.RaisePostBackEvent (string eventArgument)
		{
			if (CausesValidation)
				Page.Validate ();
			OnClick (EventArgs.Empty);
			OnCommand (new CommandEventArgs (CommandName, CommandArgument));
		}
#endif

		protected override void AddAttributesToRender (HtmlTextWriter writer)
		{
			base.AddAttributesToRender (writer);
#if NET_2_0
			if (Page != null && Enabled) {
				
				if (OnClientClick != "")
					writer.AddAttribute (HtmlTextWriterAttribute.Onclick, OnClientClick);

				string script = "";

				PostBackOptions ops = GetPostBackOptions ();
				if (ops.RequiresSpecialPostBack || ops.ClientSubmit) {
					script = Page.ClientScript.GetPostBackEventReference (ops);
				}
				else if (CausesValidation && Page.Validators.Count > 0) {
					script = Utils.GetClientValidatedPostBack (this);
				}
				else {
					script = Page.ClientScript.GetPostBackClientEvent (this,"");
				}			
				if (script != "")
					writer.AddAttribute (HtmlTextWriterAttribute.Href, "javascript:" + script);
			}

#else
			if (Enabled && Page != null){
				if (CausesValidation && Page.Validators.Count > 0){
					writer.AddAttribute (HtmlTextWriterAttribute.Href,
							     "javascript:" + 
							     Utils.GetClientValidatedPostBack (this));
					return;
				}
				writer.AddAttribute (HtmlTextWriterAttribute.Href,
						     Page.ClientScript.GetPostBackClientHyperlink (this, ""));
			}
#endif
		}

		protected override void AddParsedSubObject (object obj)
		{
			if (HasControls ()){
				base.AddParsedSubObject (obj);
				return;
			}

			if (obj is LiteralControl){
				Text = ((LiteralControl) obj).Text;
				return;
			}

			if (Text.Length > 0){
				base.AddParsedSubObject (new LiteralControl (Text));
				Text = String.Empty;
			}

			base.AddParsedSubObject (obj);
		}

		protected override void LoadViewState (object savedState)
		{
			if (savedState != null){
				base.LoadViewState (savedState);
				string savedText = (string) ViewState ["Text"];
				if (savedText != null)
					Text = savedText;
			}
		}

		protected override void RenderContents (HtmlTextWriter writer)
		{
			if (HasControls ()){
				base.RenderContents (writer);
				return;
			}
			writer.Write (Text);
		}
	}
}
