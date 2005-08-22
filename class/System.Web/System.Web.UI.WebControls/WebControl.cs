//
// System.Web.UI.WebControls.WebControl.cs
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
using System.Collections;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Drawing;
using System.Collections.Specialized;

namespace System.Web.UI.WebControls
{
#if NET_2_0
	[ThemeableAttribute (true)]
#endif
	[PersistChildrenAttribute(false)]
	[ParseChildrenAttribute(true)]
	public class WebControl : Control, IAttributeAccessor
	{
		HtmlTextWriterTag tagKey;
		AttributeCollection attributes;
		StateBag attributeState;
		Style controlStyle;
		bool enabled = true;
		string tagName;

		protected WebControl () : this (HtmlTextWriterTag.Span)
		{
		}

		public WebControl (HtmlTextWriterTag tag)
		{
			tagKey = tag;
		}

		protected WebControl (string tag)
		{
			tagName = tag;
		}

#if NET_2_0
		[Localizable (true)]
#else
		[Bindable (true)]
#endif
		[DefaultValue (""), WebCategory ("Behavior")]
		[WebSysDescription ("A keyboard shortcut for the WebControl.")]
		public virtual string AccessKey
		{
			get
			{
				object o = ViewState["AccessKey"];
				if(o!=null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				if (value != null && value.Length > 1)
					throw new ArgumentOutOfRangeException ("value");
				ViewState["AccessKey"] = value;
			}
		}

		[Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("Attribute tags for the Webcontrol.")]
		public AttributeCollection Attributes
		{
			get
			{
				if(attributes==null)
				{
					//FIXME: From where to get StateBag and how? I think this method is OK!
					if(attributeState == null)
					{
						attributeState = new StateBag(true);
						if(IsTrackingViewState)
						{
							attributeState.TrackViewState();
						}
					}
					attributes = new AttributeCollection(attributeState);
				}
				return attributes;
			}
		}

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (typeof(Color), ""), WebCategory ("Appearance")]
		[TypeConverter (typeof (WebColorConverter))]
		[WebSysDescription ("The background color for the WebControl.")]
		public virtual Color BackColor
		{
			get {
				if (!ControlStyleCreated)
					return Color.Empty;
				return ControlStyle.BackColor;
			}

			set {
				ControlStyle.BackColor = value;
			}
		}

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (typeof(Color), ""), WebCategory ("Appearance")]
		[TypeConverter (typeof (WebColorConverter))]
		[WebSysDescription ("The border color for the WebControl.")]
		public virtual Color BorderColor
		{
			get {
				if (!ControlStyleCreated)
					return Color.Empty;
				return ControlStyle.BorderColor;
			}

			set {
				ControlStyle.BorderColor = value;
			}
		}

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (typeof(BorderStyle), "NotSet"), WebCategory ("Appearance")]
		[WebSysDescription ("The style/type of the border used for the WebControl.")]
		public virtual BorderStyle BorderStyle
		{
			get {
				if (!ControlStyleCreated)
					return BorderStyle.NotSet;
				return ControlStyle.BorderStyle;
			}

			set {
				ControlStyle.BorderStyle = value;
			}
		}

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (typeof (Unit), ""), WebCategory ("Appearance")]
		[WebSysDescription ("The width of the border used for the WebControl.")]
		public virtual Unit BorderWidth
		{
			get {
				if (!ControlStyleCreated)
					return Unit.Empty;
				return ControlStyle.BorderWidth;
			}

			set {
				if (value.Value < 0)
					throw new ArgumentOutOfRangeException ("value");
				ControlStyle.BorderWidth = value;
			}
		}

		[Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("The style used to display this Webcontrol.")]
		public Style ControlStyle
		{
			get
			{
				if(controlStyle == null)
				{
					controlStyle = CreateControlStyle();
					if(IsTrackingViewState)
					{
						controlStyle.TrackViewState();
					}
					controlStyle.LoadViewState(null);
				}
				return controlStyle;
			}
		}

#if NET_2_0
	    [EditorBrowsableAttribute (EditorBrowsableState.Advanced)]
#endif
		[Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("Determines if a style exists for this Webcontrol.")]
		public bool ControlStyleCreated
		{
			get
			{
				return (controlStyle!=null);
			}
		}

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (""), WebCategory ("Appearance")]
		[WebSysDescription ("The cascading stylesheet class that is associated with this WebControl.")]
		public virtual string CssClass
		{
			get
			{
				return ControlStyle.CssClass;
			}
			set
			{
				ControlStyle.CssClass = value;
			}
		}

#if NET_2_0
	    [ThemeableAttribute (false)]
#endif
		[DefaultValue (true), Bindable (true), WebCategory ("Behavior")]
		[WebSysDescription ("The activation state of this WebControl.")]
		public virtual bool Enabled {
			get {
				return enabled;
			}
			set {
				if (enabled != value) {
					ViewState ["Enabled"] = value;
					if (IsTrackingViewState)
						EnableViewState = true;
				}

				enabled = value;
			}
		}

#if !NET_2_0
		[DefaultValue (null)]
#endif
		[NotifyParentProperty (true), WebCategory ("Appearance")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[WebSysDescription ("The font of this WebControl.")]
		public virtual FontInfo Font
		{
			get
			{
				return ControlStyle.Font;
			}
		}

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (typeof(Color), ""), WebCategory ("Appearance")]
		[TypeConverter (typeof (WebColorConverter))]
		[WebSysDescription ("The color that is used to paint the primary display of the WebControl.")]
		public virtual Color ForeColor
		{
			get {
				if (!ControlStyleCreated)
					return Color.Empty;
				return ControlStyle.ForeColor;
			}

			set {
				ControlStyle.ForeColor = value;
			}
		}

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (typeof(Unit), ""), WebCategory ("Layout")]
		[WebSysDescription ("The height of this WebControl.")]
		public virtual Unit Height
		{
			get
			{
				return ControlStyle.Height;
			}
			set
			{
				if (value.Value < 0)
					throw new ArgumentOutOfRangeException ("value");
				ControlStyle.Height = value;
			}
		}

		[Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("Direct access to the styles used for this Webcontrol.")]
		public CssStyleCollection Style
		{
			get
			{
				return Attributes.CssStyle;
			}
		}

		[DefaultValue (typeof (short), "0"), WebCategory ("Behavior")]
		[WebSysDescription ("The order in which this WebControl gets tabbed through.")]
		public virtual short TabIndex
		{
			get
			{
				object o = ViewState["TabIndex"];
				if(o!=null)
					return (short)o;
				return 0;
			}
			set
			{
				if(value < short.MinValue || value > short.MaxValue)
					throw new ArgumentOutOfRangeException ("value");
				ViewState["TabIndex"] = value;
			}
		}

#if NET_2_0
		[Localizable (true)]
#else
		[Bindable (true)]
#endif
		[DefaultValue (""), WebCategory ("Behavior")]
		[WebSysDescription ("A tooltip that is shown when hovering the mouse above the WebControl.")]
		public virtual string ToolTip
		{
			get
			{
				object o = ViewState["ToolTip"];
				if(o!=null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["ToolTip"] = value;
			}
		}

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue ( typeof (Unit), ""), WebCategory ("Layout")]
		[WebSysDescription ("The width of this WebControl.")]
		public virtual Unit Width
		{
			get
			{
				return ControlStyle.Width;
			}
			set
			{
				if (value.Value < 0)
					throw new ArgumentOutOfRangeException ("value");
				ControlStyle.Width = value;
			}
		}

		public void ApplyStyle(Style s)
		{
			if (s != null && !s.IsEmpty)
				ControlStyle.CopyFrom (s);
		}

		public void CopyBaseAttributes(WebControl controlSrc)
		{
			/*
			 * AccessKey, Enabled, ToolTip, TabIndex, Attributes
			*/
			AccessKey  = controlSrc.AccessKey;
			Enabled    = controlSrc.Enabled;
			ToolTip    = controlSrc.ToolTip;
			TabIndex   = controlSrc.TabIndex;
			AttributeCollection otherAtt = controlSrc.Attributes;
			foreach (string key in otherAtt.Keys)
				Attributes [key] = otherAtt [key];
		}

		public void MergeStyle(Style s)
		{
			ControlStyle.MergeWith(s);
		}

		public virtual void RenderBeginTag (HtmlTextWriter writer)
		{
			AddAttributesToRender (writer);
			HtmlTextWriterTag tkey = TagKey;
			// The tagkey goes takes precedence if TagKey != 0 and TagName != null
			if (tkey != 0)
				writer.RenderBeginTag (tkey);
			else
				writer.RenderBeginTag (TagName);
		}

		public virtual void RenderEndTag(HtmlTextWriter writer)
		{
			writer.RenderEndTag();
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		protected virtual HtmlTextWriterTag TagKey
		{
			get
			{
				return tagKey;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		protected virtual string TagName
		{
			get
			{
				if(tagName == null && TagKey != 0)
				{
					tagName = Enum.Format(typeof(HtmlTextWriterTag), TagKey, "G").ToString();
				}
				// What if tagName is null and tagKey 0?
				// Got the answer: nothing is rendered, empty, null
				return tagName;
			}
		}

		protected virtual void AddAttributesToRender(HtmlTextWriter writer)
		{
			if(ID!=null)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
			}
			if(AccessKey.Length>0)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Accesskey, AccessKey);
			}
			if(!Enabled)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Disabled, "disabled");
			}
			if(ToolTip.Length>0)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Title, ToolTip);
			}
			if(TabIndex != 0)
			{
				writer.AddAttribute(HtmlTextWriterAttribute.Tabindex, TabIndex.ToString());
			}
			if(ControlStyleCreated)
			{
				if(!ControlStyle.IsEmpty)
				{
					ControlStyle.AddAttributesToRender(writer, this);
				}
			}
			if(attributeState != null){
				IEnumerator ie = Attributes.Keys.GetEnumerator ();
				while (ie.MoveNext ()){
					string key = (string) ie.Current;
					writer.AddAttribute (key, Attributes [key]);
				}
			}
		}

		protected virtual Style CreateControlStyle ()
		{
			return new Style (ViewState);
		}

		protected override void LoadViewState (object savedState)
		{
			if (savedState == null)
				return;

			Pair saved = (Pair) savedState;
			base.LoadViewState (saved.First);
			
			if (ControlStyleCreated || ViewState [System.Web.UI.WebControls.Style.selectionBitString] != null)
				ControlStyle.LoadViewState (null);

			if (saved.Second != null)
			{
				if (attributeState == null)
				{
					attributeState = new StateBag(true);
					attributeState.TrackViewState();
				}
				attributeState.LoadViewState (saved.Second);
			}
			
			object enable = ViewState["Enabled"];
			if (enable!=null)
			{
				Enabled = (bool)enable;
				EnableViewState = true; 
			}
		}

		protected override void Render(HtmlTextWriter writer)
		{
			RenderBeginTag (writer);
			RenderContents (writer);
			RenderEndTag (writer);
		}

		protected virtual void RenderContents(HtmlTextWriter writer)
		{
			base.Render (writer);
		}

		protected override object SaveViewState()
		{
			if (EnableViewState)
				ViewState["Enabled"] = enabled;
			if (ControlStyleCreated)
				ControlStyle.SaveViewState ();
			
			object baseView = base.SaveViewState ();
			object attrView = null;
			if (attributeState != null)
				attrView = attributeState.SaveViewState ();
			
			if (baseView == null && attrView == null)
				return null;

			return new Pair (baseView, attrView);
		}

		protected override void TrackViewState()
		{
			base.TrackViewState();
			if (ControlStyleCreated)
				ControlStyle.TrackViewState ();
			if (attributeState != null)
				attributeState.TrackViewState ();
		}

		string IAttributeAccessor.GetAttribute(string key)
		{
			if (attributes != null)
				return Attributes [key] as string;

			return null;
		}

		void IAttributeAccessor.SetAttribute(string key, string value)
		{
			Attributes [key] = value;
		}
	}
}

