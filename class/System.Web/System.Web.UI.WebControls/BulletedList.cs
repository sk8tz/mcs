//
// System.Web.UI.WebControls.BulletedList.cs
//
// Authors:
//   Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
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
#if NET_2_0
using System.IO;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.Web.UI.WebControls {

	[DesignerAttribute ("System.Web.UI.Design.WebControls.BulletedListDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[DefaultEventAttribute ("Click")]
	[DefaultPropertyAttribute ("BulletStyle")]
	public class BulletedList : ListControl, IPostBackEventHandler {
		
		[MonoTODO ("we are missing a new style enum, we should be using it")]
		protected override void AddAttributesToRender (HtmlTextWriter writer)
		{
			const string ListStyleType = "list-style-type";
			const string ListStyleImage = "list-style-image";
			
			bool isNumeric = false;
			switch (BulletStyle)
			{
				case BulletStyle.NotSet:
					break;
				
				case BulletStyle.Numbered:
					writer.AddStyleAttribute (ListStyleType, "decimal");
					isNumeric = true;
					break;
				
				case BulletStyle.LowerAlpha:
					writer.AddStyleAttribute (ListStyleType, "lower-alpha");
					isNumeric = true;
					break;
				
				case BulletStyle.UpperAlpha:
					writer.AddStyleAttribute (ListStyleType, "upper-alpha");
					isNumeric = true;
					break;
				
				case BulletStyle.LowerRoman:
					writer.AddStyleAttribute (ListStyleType, "lower-roman");
					isNumeric = true;
					break;
				
				case BulletStyle.UpperRoman:
					writer.AddStyleAttribute (ListStyleType, "lower-roman");
					isNumeric = true;
					break;

				case BulletStyle.Disc:
					writer.AddStyleAttribute (ListStyleType, "disc");
					break;
				
				case BulletStyle.Circle:
					writer.AddStyleAttribute (ListStyleType, "circle");
					break;
				
				case BulletStyle.Square:
					writer.AddStyleAttribute (ListStyleType, "square");
					break;
								
				case BulletStyle.CustomImage:
					writer.AddStyleAttribute (ListStyleImage, "url(" + BulletImageUrl+ ")");
					break;
			}

			if (isNumeric && FirstBulletNumber != 1)
				writer.AddAttribute ("start", FirstBulletNumber.ToString ());
			
			base.AddAttributesToRender (writer);
		}
		
		bool cacheIsEnabled;
		[MonoTODO ("new bool prop on ListItem: Enabled")]
		protected virtual void RenderBulletText (ListItem item, int index, HtmlTextWriter writer)
		{
			switch (DisplayMode) {
				case BulletedListDisplayMode.Text:
					//if (!item.Enabled) {
					//	writer.AddAttribute (HtmlTextWriterAttribute.Disabled, "disabled");
					//	writer.RenderBeginTag (HtmlTextWriterTag.Span);
					//}
					writer.Write (item.Text);
					//if (!item.Enabled)
					//	writer.RenderEndTag ();
					break;

				case BulletedListDisplayMode.HyperLink:
					//if (cacheIsEnabled && item.Enabled) {
					//	writer.AddAttribute (HtmlTextWriterAttribute.Href, item.Value);
					//	if (Target != "")
					//		writer.AddAttribute(HtmlTextWriterAttribute.Target, this.Target);
					//	
					//}
					//else
						writer.AddAttribute (HtmlTextWriterAttribute.Disabled, "disabled");
					
					writer.RenderBeginTag (HtmlTextWriterTag.A);
					writer.Write (item.Text);
					writer.RenderEndTag ();
					break;

				case BulletedListDisplayMode.LinkButton:
					//if (cacheIsEnabled && item.Enabled)
						writer.AddAttribute (HtmlTextWriterAttribute.Href, Page.ClientScript.GetPostBackClientHyperlink (this, (index.ToString (CultureInfo.InvariantCulture))));
					//else
					//	writer.AddAttribute (HtmlTextWriterAttribute.Disabled, "disabled");
					writer.RenderBeginTag (HtmlTextWriterTag.A);
					writer.Write (item.Text);
					writer.RenderEndTag ();
					break;
			}
		}
		
		protected override void RenderContents (HtmlTextWriter writer)
		{
			cacheIsEnabled = this.Enabled;
			int idx = 0;
			foreach (ListItem i in Items) {
				writer.RenderBeginTag (HtmlTextWriterTag.Li);
				this.RenderBulletText (i, idx ++, writer);
				writer.RenderEndTag ();
			}
		}
		
		void IPostBackEventHandler.RaisePostBackEvent (string eventArgument)
		{
			RaisePostBackEvent (eventArgument);
		}
			
		[MonoTODO ("ListControl has a CausesValidation prop in v1.2, we need to use it")]
		protected virtual void RaisePostBackEvent (string eventArgument)
		{
			//if (CausesValidation)
			//	Page.Validate ();
			
			this.OnClick (new BulletedListEventArgs (int.Parse (eventArgument, CultureInfo.InvariantCulture)));
		}
			
	    [BrowsableAttribute (false)]
    	[EditorBrowsableAttribute (EditorBrowsableState.Never)]
		public override bool AutoPostBack { 
			get { return base.AutoPostBack; }
			set { throw new NotSupportedException (String.Format ("This property is not supported in {0}", GetType ())); }
		}
		
	    [EditorBrowsableAttribute (EditorBrowsableState.Never)]
		public override int SelectedIndex {
			get { return base.SelectedIndex; }
			set { throw new NotSupportedException (String.Format ("This property is not supported in {0}", GetType ())); }
		}
		
	    [EditorBrowsableAttribute (EditorBrowsableState.Never)]
		public override ListItem SelectedItem {
			get { return base.SelectedItem; }
		}
		
		[DefaultValueAttribute ("")]
		[EditorAttribute ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[UrlPropertyAttribute]
		public virtual string BulletImageUrl {
			get {
				object ret = ViewState ["BulletImageUrl"];
				if (ret != null)
					return (string) ret; 
			
				return "";
			}
			set {
				ViewState ["BulletImageUrl"] = value;
			}
		}
		
	    [DefaultValueAttribute (BulletStyle.NotSet)]
		public virtual BulletStyle BulletStyle {
			get {
				object ret = ViewState ["BulletStyle"];
				if (ret != null)
					return (BulletStyle) ret; 
			
				return BulletStyle.NotSet;
			}
			set {
				if ((int) value < 0 || (int) value > 9)
					throw new ArgumentOutOfRangeException ("value");

				ViewState ["BulletStyle"] = value;
			}
		}
		
		public override ControlCollection Controls { get { return new EmptyControlCollection (this); } }
		
	    [DefaultValueAttribute (BulletedListDisplayMode.Text)]
		public virtual BulletedListDisplayMode DisplayMode {
			get {
				object ret = ViewState ["DisplayMode"];
				if (ret != null)
					return (BulletedListDisplayMode) ret; 
			
				return BulletedListDisplayMode.Text;
			}
			set {
				if ((int) value < 0 || (int) value > 2)
					throw new ArgumentOutOfRangeException ("value");

				ViewState ["DisplayMode"] = value;
			}
		}
		
	    [DefaultValueAttribute (1)]
		public virtual int FirstBulletNumber {
			get {
				object ret = ViewState ["FirstBulletNumber"];
				if (ret != null)
					return (int) ret; 
			
				return 1;
			}
			set {
				ViewState ["FirstBulletNumber"] = value;
			}
		}
		

		protected override HtmlTextWriterTag TagKey {
			get {
				switch (BulletStyle) {			
					case BulletStyle.Numbered:
					case BulletStyle.LowerAlpha:
					case BulletStyle.UpperAlpha:
					case BulletStyle.LowerRoman:
					case BulletStyle.UpperRoman:
						return HtmlTextWriterTag.Ol;
					
					case BulletStyle.NotSet:
					case BulletStyle.Disc:
					case BulletStyle.Circle:
					case BulletStyle.Square:
					case BulletStyle.CustomImage:
					default:
						return HtmlTextWriterTag.Ul;
				}
			}
		}
		
	    [DefaultValueAttribute ("")]
		public virtual string Target {
			get {
				object ret = ViewState ["Target"];
				if (ret != null)
					return (string) ret; 
			
				return "";
			}
			set {
				ViewState ["Target"] = value;
			}
		}
		
		static readonly object ClickEvent = new object ();
		public event BulletedListEventHandler Click
		{
			add {
				Events.AddHandler (ClickEvent, value);
			}
			remove {
				Events.RemoveHandler (ClickEvent, value);
			}
		}
		
		protected virtual void OnClick (BulletedListEventArgs e)
		{
			if (Events != null) {
				BulletedListEventHandler eh = (BulletedListEventHandler) (Events [ClickEvent]);
				if (eh != null)
					eh (this, e);
			}
		}
	}
}
#endif
