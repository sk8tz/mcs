//
// System.Web.UI.HtmlControls.HtmlControl.cs
//
// Author
//   Bob Smith <bob@thestuff.net>
//
//
// (C) Bob Smith
//

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.HtmlControls{
	
	[ToolboxItem(false)]
	[Designer ("System.Web.UI.Design.HtmlIntrinsicControlDesigner, " + Consts.AssemblySystem_Design,
			"System.ComponentModel.Design.IDesigner")]
	public abstract class HtmlControl : Control, IAttributeAccessor
	{
		internal string _tagName;
		private AttributeCollection _attributes;

		
#if NET_2_0
		protected
#else
		public
#endif
		HtmlControl() : this ("span") {}
		
#if NET_2_0
		protected
#else
		public
#endif
		HtmlControl(string tag)
		{
			_tagName = tag;
		}
		
		protected override ControlCollection CreateControlCollection ()
		{
			return new EmptyControlCollection (this);
		}

		internal static string AttributeToString(int n){
			if (n != -1)return n.ToString(NumberFormatInfo.InvariantInfo);
			return null;
		}
		
		internal static string AttributeToString(string s){
			if (s != null && s.Length != 0) return s;
			return null;
		}
		
		internal void PreProcessRelativeReference(HtmlTextWriter writer, string attribName){
			string attr = Attributes[attribName];
			if (attr != null){
				if (attr.Length != 0){
					try{
						attr = ResolveUrl(attr);
					}
					catch (Exception) {
						throw new HttpException(attribName + " property had malformed url");
					}
					writer.WriteAttribute(attribName, attr);
					Attributes.Remove(attribName);
				}
			}
		}

#if NET_2_0
		/* keep these two methods in sync with the
		 * IAttributeAccessor iface methods below */
		protected virtual string GetAttribute (string name)
		{
			return Attributes[name];
		}

		protected virtual void SetAttribute (string name, string value)
		{
			Attributes[name] = value;
		}
#endif		
		
		string System.Web.UI.IAttributeAccessor.GetAttribute(string name){
			return Attributes[name];
		}
		
		void System.Web.UI.IAttributeAccessor.SetAttribute(string name, string value){
			Attributes[name] = value;
		}
		
		protected virtual void RenderBeginTag (HtmlTextWriter writer)
		{
			writer.WriteBeginTag (TagName);
			RenderAttributes (writer);
			writer.Write ('>');
		}

#if NET_2_0
		protected internal
#else
		protected
#endif
		override void Render (HtmlTextWriter writer)
		{
			RenderBeginTag (writer);
		}
		
		protected virtual void RenderAttributes(HtmlTextWriter writer){
			if (ID != null){
				writer.WriteAttribute("id",ClientID);
			}
			Attributes.Render(writer);
		}
		
		[Browsable(false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public AttributeCollection Attributes
		{
			get { 
				if (_attributes == null)
					_attributes = new AttributeCollection (ViewState);
				return _attributes;
			}
		}

		[DefaultValue(false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription("")]
		[WebCategory("Behavior")]
#if NET_2_0
		/* Can't find this type in the docs */
		//[TypeConverter (typeof(System.Web.UI.MinimizableAttributeTypeConverter))]
#endif
		public bool Disabled
		{
			get {
				string disableAttr = Attributes["disabled"] as string;
				return (disableAttr != null);
                        }
			set {
                                if (!value)
                                        Attributes.Remove ("disabled");
                                else
                                        Attributes["disabled"] = "disabled";
                        }
		}

		[Browsable(false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public CssStyleCollection Style
		{
			get { return Attributes.CssStyle; }
		}

		[DefaultValue("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription("")]
		[WebCategory("Appearance")]
		public virtual string TagName
		{
			get { return _tagName; }
		}

		protected override bool ViewStateIgnoresCase 
		{
			get {
				return true;
			}
		}
	}
}
