/*	System.Web.UI.HtmlControls
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.Web;
using System.Web.UI;
using System.Globalization;
using System.Collections.Specialized;

namespace System.Web.UI.HtmlControls{
	
	public class HtmlInputImage : HtmlInputControl, IPostBackEventHandler, IPostBackDataHandler{
		
		private static readonly object EventServerClick;
		private int _x, _y;
		
		public HtmlInputImage(): base("image"){}
		
		protected override void OnPreRender(EventArgs e){
			if (Page != null && !Disabled){
				Page.RegisterRequiresPostBack(this);
			}
		}
		
		protected void OnServerClick(ImageClickEventArgs e){
			ImageClickEventHandler handler = (ImageClickEventHandler) Events[EventServerClick];
			// if (handler != null) handler.Invoke(this, e);
		}
		
		protected override void RenderAttributes(HtmlTextWriter writer){
			PreProcessRelativeReference(writer,"src");
			if (Page != null && !CausesValidation){
				WriteOnClickAttribute(
				                           writer,
				                           false,
				                           true,
				                           CausesValidation == false? Page.Validators.Count > 0: false);
			}
			RenderAttributes(writer);
		}
		
		public bool LoadPostData(string postDataKey, NameValueCollection postCollection){
			string postX = postCollection[String.Concat(RenderedName,".x")];
			string postY = postCollection[String.Concat(RenderedName,".y")];
			if (postX != null && postY != null && postX.Length >= 0 && postY.Length >= 0){
				_x = Int32.Parse(postX, CultureInfo.InvariantCulture);
				_y = Int32.Parse(postY, CultureInfo.InvariantCulture);
				Page.RegisterRequiresRaiseEvent(this);
			}
			return false;
		}
		
		public void RaisePostBackEvent(string eventArgument){
			if (CausesValidation){
				Page.Validate();
			}
			OnServerClick(new ImageClickEventArgs(_x,_y));
		}
		
		public void RaisePostDataChangedEvent(){}
		
		public event EventHandler ServerClick{
			add{
				Events.AddHandler(EventServerClick, value);
			}
			remove{
				Events.RemoveHandler(EventServerClick, value);
			}
		}
		
		public string Align{
			get{
				string attr = Attributes["align"];
				if (attr != null) return attr;
				return String.Empty;
			}
			set{
				Attributes["align"] = AttributeToString(value);
			}
		}
		
		public string Alt{
			get{
				string attr = Attributes["alt"];
				if (attr != null) return attr;
				return String.Empty;
			}
			set{
				Attributes["alt"] = AttributeToString(value);
			}
		}
		
		public int Border{
			get{
				string attr = Attributes["border"];
				if (attr != null) return Int32.Parse(attr,CultureInfo.InvariantCulture);
				return -1;
			}
			set{
				Attributes["border"] = AttributeToString(value);
			}
		}
		
		public bool CausesValidation{
			get{
				object causesVal = ViewState["CausesValidation"];
				if (causesVal != null) return (Boolean) causesVal;
				return true;
			}
			set{
				ViewState["CausesValidation"] = (Boolean) value;
			}
		}
		
		public string Src{
			get{
				string attr = Attributes["src"];
				if (attr != null) return attr;
				return String.Empty;
			}
			set{
				Attributes["src"] = AttributeToString(value);
			}
		}
	} // class HtmlInputImage
} // namespace System.Web.UI.HtmlControls

