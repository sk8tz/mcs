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
	
	public class HtmlInputFile : HtmlInputControl, IPostBackDataHandler{
		
		public HtmlInputFile():base("file"){}
		
		public bool LoadPostData(string postDataKey, NameValueCollection postCollection){
			string postValue = postCollection[postDataKey];
			if (postValue != null){
				Value = postValue;
			}
			return false;
		}
		
		public void RaisePostDataChangedEvent(){}
		
		public string Accept{
			get{
				string attr = Attributes["accept"];
				if (attr != null){
					return attr;
				}
				return "";
			}
			set{
				Attributes["accept"] = MapStringAttributeToString(value);
			}
		}
		
		public int MaxLength{
			get{
				string attr = Attributes["maxlength"];
				if (attr != null){
					return Int32.Parse(attr, CultureInfo.InvariantCulture);
				}
				return -1;
			}
			set{
				Attributes["accept"] = MapIntegerAttributeToString(value);
			}
		}
		
		public int Size{
			get{
				string attr = Attributes["size"];
				if (attr != null){
					return Int32.Parse(attr);
				}
				return -1;
			}
			set{
				Attributes["size"] = MapIntegerAttributeToString(value);
			}
		}
		
		public HttpPostedFile PostedFile{
			get{
				return Context.Request.Files[RenderedNameAttribute];
			}
		}
		
	} // class HtmlInputFile
} // namespace System.Web.UI.HtmlControls

