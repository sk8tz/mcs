/*	System.Web.UI.HtmlControls
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.Web;
using System.Web.UI;
using System.Globalization;

namespace System.Web.UI.HtmlControls{
	
	public class HtmlImage : HtmlControl{
		
		public HtmlImage(): base("img"){}
		
		protected override void RenderAttributes(HtmlTextWriter writer){
			PreProcessRelativeReferenceAttribute(writer,"src");
			RenderAttributes(writer);
			writer.Write(" /");
		}
		
		public string Align{
			get{
				string attr = Attributes["align"];
				if (attr != null){
					return attr;
				}
				return "";
			}
			set{
				Attributes["align"] = MapStringAttributeToString(value);
			}
		}
		
		public string Alt{
			get{
				string attr = Attributes["alt"];
				if (attr != null){
					return attr;
				}
				return "";
			}
			set{
				Attributes["alt"] = MapStringAttributeToString(value);
			}
		}
		
		public int Border{
			get{
				string attr = Attributes["border"];
				if (attr != null){
					return Int32.Parse(attr,CultureInfo.InvariantCulture);
				}
				return -1;
			}
			set{
				Attributes["border"] = MapIntegerAttributeToString(value);
			}
		}
		
		public string Src{
			get{
				string attr = Attributes["src"];
				if (attr != null){
					return attr;
				}
				return "";
			}
			set{
				Attributes["src"] = MapStringAttributeToString(value);
			}
		}
		
		public int Width{
			get{
				string attr = Attributes["width"];
				if (attr != null){
					return Int32.Parse(attr,CultureInfo.InvariantCulture);
				}
				return -1;
			}
			set{
				Attributes["width"] = MapIntegerAttributeToString(value);
			}
		}
		
	} // class HtmlImage
} // namespace System.Web.UI.HtmlControls


