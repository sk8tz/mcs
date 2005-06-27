//
// System.Web.UI.WebControls.FontInfo.cs
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
using System.Text;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Drawing;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public sealed class FontInfo
	{
		private Style infoOwner;				
		
		internal FontInfo(Style owner)
		{
			infoOwner = owner;
		}
		
		/// <summary>
		/// Default constructor
		/// <remarks>
		/// The default constructor is made private to prevent any instances being made.
		/// </remarks>
		/// </summary>
		private FontInfo()
		{
		}
		
#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (false), WebCategory ("Font")]
		[NotifyParentProperty (true)]
		[WebSysDescription ("The 'bold' style of the font.")]
		public bool Bold
		{
			get
			{
				if(infoOwner.IsSet(Style.FONT_BOLD))
					return (bool)(infoOwner.ViewState["FontInfoBold"]);
				return false;
			}
			set
			{
				infoOwner.ViewState["FontInfoBold"] = value;
				infoOwner.Set(Style.FONT_BOLD);
			}
		}
		
#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (false), WebCategory ("Font")]
		[NotifyParentProperty (true)]
		[WebSysDescription ("The 'italic' style of the font.")]
		public bool Italic
		{
			get
			{
				if(infoOwner.IsSet(Style.FONT_ITALIC))
					return (bool)(infoOwner.ViewState["FontInfoItalic"]);
				return false;
			}
			set
			{
				infoOwner.ViewState["FontInfoItalic"] = value;
				infoOwner.Set(Style.FONT_ITALIC);
			}
		}
		
#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (false), WebCategory ("Font")]
		[NotifyParentProperty (true)]
		[WebSysDescription ("The 'overline' style of the font.")]
		public bool Overline
		{
			get
			{
				if(infoOwner.IsSet(Style.FONT_OLINE))
					return (bool)(infoOwner.ViewState["FontInfoOverline"]);
				return false;
			}
			set
			{
				infoOwner.ViewState["FontInfoOverline"] = value;
				infoOwner.Set(Style.FONT_OLINE);
			}
		}
		
#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (false), WebCategory ("Font")]
		[NotifyParentProperty (true)]
		[WebSysDescription ("The 'strikeout' style of the font.")]
		public bool Strikeout
		{
			get
			{
				if(infoOwner.IsSet(Style.FONT_STRIKE))
					return (bool)(infoOwner.ViewState["FontInfoStrikeout"]);
				return false;
			}
			set
			{
				infoOwner.ViewState["FontInfoStrikeout"] = value;
				infoOwner.Set(Style.FONT_STRIKE);
			}
		}
		
#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (false), WebCategory ("Font")]
		[NotifyParentProperty (true)]
		[WebSysDescription ("The 'underline' style of the font.")]
		public bool Underline
		{
			get
			{
				if(infoOwner.IsSet(Style.FONT_ULINE))
					return (bool)(infoOwner.ViewState["FontInfoUnderline"]);
				return false;
			}
			set
			{
				infoOwner.ViewState["FontInfoUnderline"] = value;
				infoOwner.Set(Style.FONT_ULINE);
			}
		}

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (null), WebCategory ("Font")]
		[NotifyParentProperty (true)]
		[WebSysDescription ("The size of the font.")]
		public FontUnit Size
		{
			get
			{
				if(infoOwner.IsSet(Style.FONT_SIZE))
					return (FontUnit)(infoOwner.ViewState["FontInfoSize"]);
				return FontUnit.Empty;
			}
			set
			{
				if ((value.Type == FontSize.AsUnit) && (value.Unit.Value < 0))
					throw new ArgumentOutOfRangeException("value");
				infoOwner.ViewState["FontInfoSize"] = value;
				infoOwner.Set(Style.FONT_SIZE);
			}
		}

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (""), WebCategory ("Font")]
		[NotifyParentProperty (true), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Editor ("System.Drawing.Design.FontNameEditor, " + Consts.AssemblySystem_Drawing_Design, typeof (System.Drawing.Design.UITypeEditor))]
		[TypeConverter (typeof (FontConverter.FontNameConverter))]
		[WebSysDescription ("The name of the font that this control should be rendered with.")]
		public string Name
		{
			get
			{
				if(Names!=null && Names.Length > 0)
					return Names[0];
				return String.Empty;
			}
			set
			{
				if(value == null)
					throw new ArgumentException();
				string[] strArray = null;
				if(value.Length > 0)
				{
					strArray = new string[1];
					strArray[0] = value;
				}
				Names = strArray;
			}
		}

		[WebCategory ("Font")]
		[NotifyParentProperty (true)]
		[Editor ("System.Windows.Forms.Design.StringArrayEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		[TypeConverter (typeof (FontNamesConverter))]
		[WebSysDescription ("Multiple fonts that can be used to render the control.")]
		public string[] Names
		{
			get
			{
				if(infoOwner.IsSet(Style.FONT_NAMES))
					return (string[])(infoOwner.ViewState["FontInfoNames"]);
				return (new string[0]);
			}
			set
			{
				if(value!=null)
				{
					infoOwner.ViewState["FontInfoNames"] = value;
					infoOwner.Set(Style.FONT_NAMES);
				}
			}
		}
		
		internal void Reset()
		{
			if(infoOwner.IsSet(Style.FONT_NAMES))
				infoOwner.ViewState.Remove("FontInfoNames");
			if(infoOwner.IsSet(Style.FONT_BOLD))
				infoOwner.ViewState.Remove("FontInfoBold");
			if(infoOwner.IsSet(Style.FONT_ITALIC))
				infoOwner.ViewState.Remove("FontInfoItalic");
			if(infoOwner.IsSet(Style.FONT_STRIKE))
				infoOwner.ViewState.Remove("FontInfoStrikeout");
			if(infoOwner.IsSet(Style.FONT_OLINE))
				infoOwner.ViewState.Remove("FontInfoOverline");
			if(infoOwner.IsSet(Style.FONT_ULINE))
				infoOwner.ViewState.Remove("FontInfoUnderline");
			if(infoOwner.IsSet(Style.FONT_SIZE) && infoOwner.Font.Size != FontUnit.Empty)
				infoOwner.ViewState.Remove("FontInfoSize");
		}
		
		internal Style Owner
		{
			get
			{
				return infoOwner;
			}
		}
		
		public void CopyFrom(FontInfo source)
		{
			if(source!=null)
			{
				if(source.Owner.IsSet(Style.FONT_NAMES))
					Names = source.Names;
				if(source.Owner.IsSet(Style.FONT_BOLD)&& source.Bold)
					Bold = source.Bold;
				if(source.Owner.IsSet(Style.FONT_ITALIC)&& source.Italic)
					Italic = source.Italic;
				if(source.Owner.IsSet(Style.FONT_STRIKE)&& source.Strikeout)
					Strikeout = source.Strikeout;
				if(source.Owner.IsSet(Style.FONT_OLINE)&& source.Overline)
					Overline = source.Overline;
				if(source.Owner.IsSet(Style.FONT_ULINE)&& source.Underline)
					Underline = source.Underline;
				if(source.Owner.IsSet(Style.FONT_SIZE) && source.Size != FontUnit.Empty)
					Size = source.Size;
			}
		}
		
		public void MergeWith(FontInfo with)
		{
			if(with!=null)
			{
				if(with.Owner.IsSet(Style.FONT_NAMES) && !infoOwner.IsSet(Style.FONT_NAMES))
					Names = with.Names;
				if(with.Owner.IsSet(Style.FONT_BOLD) && !infoOwner.IsSet(Style.FONT_BOLD))
					Bold = with.Bold;
				if(with.Owner.IsSet(Style.FONT_ITALIC) && !infoOwner.IsSet(Style.FONT_ITALIC))
					Italic = with.Italic;
				if(with.Owner.IsSet(Style.FONT_STRIKE) && !infoOwner.IsSet(Style.FONT_STRIKE))
					Strikeout = with.Strikeout;
				if(with.Owner.IsSet(Style.FONT_OLINE) && !infoOwner.IsSet(Style.FONT_OLINE))
					Overline = with.Overline;
				if(with.Owner.IsSet(Style.FONT_ULINE) && !infoOwner.IsSet(Style.FONT_ULINE))
					Underline = with.Underline;
				if(with.Owner.IsSet(Style.FONT_SIZE) && with.Size != FontUnit.Empty && !infoOwner.IsSet(Style.FONT_SIZE))
					Size = with.Size;
			}
		}
		
		public bool ShouldSerializeNames()
		{
			return (Names.Length > 0);
		}
		
		public override string ToString()
		{
			return ( (Name.Length > 0) ? (Name.ToString() + ", " + Size.ToString()) : Size.ToString() );
		}
	}
}
