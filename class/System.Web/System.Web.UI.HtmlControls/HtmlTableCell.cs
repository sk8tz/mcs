/*	System.Web.UI.HtmlControls
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.Web;
using System.Web.UI;
using System.Globalization;

namespace System.Web.UI.HtmlControls{
	public class HtmlTableCell : HtmlContainerControl {
		public HtmlTableCell(): base("td"){}
		
		public HtmlTableCell(string tagName): base(tagName){}
		
		protected override void RenderEndTag(HtmlTextWriter writer){
			base.RenderEndTag(writer);
			writer.WriteLine();
		}
		
		public string Align {
			get{
				string attr = Attributes["align"];
				if (attr != null) return attr;
				return String.Empty;
			}
			set{
				Attributes["align"] = AttributeToString(value);
			}
		}
		
		public string BgColor {
			get{
				string attr = Attributes["bgcolor"];
				if (attr != null) return attr;
				return String.Empty;
			}
			set{
				Attributes["bgcolor"] = AttributeToString(value);
			}
		}
		
		public string BorderColor {
			get{
				string attr = Attributes["bordercolor"];
				if (attr != null) return attr;
				return String.Empty;
			}
			set{
				Attributes["bordercolor"] = AttributeToString(value);
			}
		}
		
		public int ColSpan {
			get{
				string attr = Attributes["colspan"];
				if (attr != null) return Int32.Parse(attr, CultureInfo.InvariantCulture);
				return -1;
			}
			set{
				Attributes["colspan"] = AttributeToString(value);
			}
		}
		
		public string Height {
			get{
				string attr = Attributes["height"];
				if (attr != null) return attr;
				return String.Empty;
			}
			set{
				Attributes["height"] = AttributeToString(value);
			}
		}
		
		public bool NoWrap {
			get{
				string attr = Attributes["colspan"];
				if (attr != null) return attr.Equals("nowrap");
				return false;
			}
			set{
				if (value == true){
					Attributes["nowrap"] = "nowrap";
				}
				else{
					Attributes["nowrap"] = null;
				}
			}
		}
		
		public int RowSpan {
			get{
				string attr = Attributes["rowspan"];
				if (attr != null) return Int32.Parse(attr, CultureInfo.InvariantCulture);
				return -1;
			}
			set{
				Attributes["rowspan"] = AttributeToString(value);
			}
		}
		
		public string VAlign {
			get{
				string attr = Attributes["valign"];
				if (attr != null) return attr;
				return String.Empty;
			}
			set{
				Attributes["valign"] = AttributeToString(value);
			}
		}
		
		public string Width {
			get{
				string attr = Attributes["width"];
				if (attr != null) return attr;
				return String.Empty;
			}
			set{
				Attributes["width"] = AttributeToString(value);
			}
		}
		
	}
	// System.Web.UI.HtmlControls.HtmlTableCell
	
}
