/**
 * Namespace: System.Web.UI.WebControls
 * Class:     Button
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	[DefaultEvent("Click")]
	[DefaultProperty("Text")]
	//TODO: [Designer("??")]
	//TODO: [DataBindingHandler("??UI.Design.TextDataBindingHandler??")]
	[ToolboxData("<{0}:Button runat=\"server\" Text=\"Button\"></{0}:Button>")]
	public class Button : WebControl, IPostBackEventHandler
	{
		private static readonly object ClickEvent   = new object();
		private static readonly object CommandEvent = new object();

		//private EventHandlerList ehList;

		public Button(): base(HtmlTextWriterTag.Button)
		{
		}

		public bool CausesValidation
		{
			get
			{
				Object cv = ViewState["CausesValidation"];
				if(cv!=null)
					return (Boolean)cv;
				return true;
			}
			set
			{
				ViewState["CausesValidation"] = value;
			}
		}

		public string CommandArgument
		{
			get
			{
				string ca = (string) ViewState["CommandArgument"];
				if(ca!=null)
					return ca;
				return String.Empty;
			}
			set
			{
				ViewState["CommandArgument"] = value;
			}
		}

		public string CommandName
		{
			get
			{
				string cn = (string)ViewState["CommandName"];
				if(cn!=null)
					return cn;
				return String.Empty;
			}
			set
			{
				ViewState["CommandName"] = value;
			}
		}

		public string Text
		{
			get
			{
				string text = (string)ViewState["Text"];
				if(text!=null)
					return text;
				return String.Empty;
			}
			set
			{
				ViewState["Text"] = value;
			}
		}

		public event EventHandler Click
		{
			add
			{
				Events.AddHandler(ClickEvent, value);
			}
			remove
			{
				Events.RemoveHandler(ClickEvent, value);
			}
		}

		public event CommandEventHandler Command
		{
			add
			{
				Events.AddHandler(CommandEvent, value);
			}
			remove
			{
				Events.RemoveHandler(CommandEvent, value);
			}
		}

		protected virtual void OnClick(EventArgs e)
		{
			if(Events != null)
			{
				EventHandler eh = (EventHandler)(Events[ClickEvent]);
				if(eh!= null)
					eh(this,e);
			}
		}

		protected virtual void OnCommand(CommandEventArgs e)
		{
			if(Events != null)
			{
				EventHandler eh = (EventHandler)(Events[CommandEvent]);
				if(eh!= null)
					eh(this,e);
			}
		}

		void IPostBackEventHandler.RaisePostBackEvent(string eventArgument)
		{
			if(CausesValidation)
			{
				Page.Validate();
				OnClick(new EventArgs());
				OnCommand(new CommandEventArgs(CommandName, CommandArgument));
			}
		}

		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			writer.AddAttribute(HtmlTextWriterAttribute.Type,"submit");
			writer.AddAttribute(HtmlTextWriterAttribute.Name,base.UniqueID);
			writer.AddAttribute(HtmlTextWriterAttribute.Value,Text);
			if(Page!=null && CausesValidation && Page.Validators.Count > 0)
			{
				writer.AddAttribute(System.Web.UI.HtmlTextWriterAttribute.Onclick, Utils.GetClientValidatedEvent());
				writer.AddAttribute("language", "javascript");
			}
			AddAttributesToRender(writer);
		}

		protected override void RenderContents(HtmlTextWriter writer)
		{
			// Preventing subclasses to do anything
		}
	}
}
