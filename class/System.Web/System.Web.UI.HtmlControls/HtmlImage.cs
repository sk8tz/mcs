//
// System.Web.UI.HtmlControls.HtmlImage.cs
//
// Author:
//	Dick Porter  <dick@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.ComponentModel;
using System.Globalization;

namespace System.Web.UI.HtmlControls 
{
#if NET_2_0
	[ControlBuilder (typeof (HtmlEmptyTagControlBuilder))]
#else
	[ControlBuilder (typeof (HtmlControlBuilder))]
#endif
	public class HtmlImage : HtmlControl 
	{
		public HtmlImage () : base ("img")
		{
		}

		[MonoTODO ("need to validate input strings?")]
		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription("")]
		[WebCategory("Layout")]
		public string Align 
		{
			get {
				string align = Attributes["align"];

				if (align == null) {
					return (String.Empty);
				}
				
				return (align);
			}
			set {
				/* Validate: left, center, right, top,
				 * middle, bottom?
				 */
				if (value == null) {
					Attributes.Remove ("align");
				} else {
					Attributes["align"] = value;
				}
			}
		}

		[DefaultValue ("")]
		[WebSysDescription("")]
		[WebCategory("Appearance")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
#if NET_2_0
		[Localizable (true)]
#endif
		public string Alt 
		{
			get {
				string alt = Attributes["alt"];

				if (alt == null) {
					return (String.Empty);
				}
				
				return (alt);
			}
			set {
				if (value == null) {
					Attributes.Remove ("alt");
				} else {
					Attributes["alt"] = value;
				}
			}
		}
	
		[DefaultValue (0)]
		[WebSysDescription("")]
		[WebCategory("Appearance")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int Border 
		{
			get {
				string border = Attributes["border"];
				
				if (border == null) {
					return (-1);
				} else {
					return (Int32.Parse (border, CultureInfo.InvariantCulture));
				}
			}
			set {
				if (value == -1) {
					Attributes.Remove ("border");
				} else {
					Attributes["border"] = value.ToString ();
				}
			}
		}

		[DefaultValue (100)]
		[WebSysDescription("")]
		[WebCategory("Layout")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int Height
		{
			get {
				string height = Attributes["height"];
				
				if (height == null) {
					return (-1);
				} else {
					return (Int32.Parse (height, CultureInfo.InvariantCulture));
				}
			}
			set {
				if (value == -1) {
					Attributes.Remove ("height");
				} else {
					Attributes["height"] = value.ToString ();
				}
			}
		}
		
		[DefaultValue ("")]
		[WebSysDescription("")]
		[WebCategory("Behavior")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string Src 
		{
			get {
				string src = Attributes["src"];

				if (src == null) {
					return (String.Empty);
				}
				
				return (src);
			}
			set {
				if (value == null) {
					Attributes.Remove ("src");
				} else {
					Attributes["src"] = value;
				}
			}
		}

		[DefaultValue (100)]
		[WebSysDescription("")]
		[WebCategory("Layout")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int Width 
		{
			get {
				string width = Attributes["width"];

				if (width == null) {
					return (-1);
				}
				else {
					return (Int32.Parse (width, CultureInfo.InvariantCulture));
				}
			}
			set {
				if (value == -1) {
					Attributes.Remove ("width");
				} else {
					Attributes["width"] = value.ToString ();
				}
			}
		}

		protected override void RenderAttributes (HtmlTextWriter w)
		{
			PreProcessRelativeReference (w, "src");
			base.RenderAttributes (w);

			/* MS closes the HTML element at the end of
			 * the attributes too, according to the nunit
			 * tests
			 */
			w.Write (" /");
		}
	}
}

	
