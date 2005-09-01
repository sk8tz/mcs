//
// System.Web.UI.HtmlControls.HtmlAnchor.cs
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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

namespace System.Web.UI.HtmlControls {

	[DefaultEvent ("ServerClick")]
	public class HtmlAnchor : HtmlContainerControl, IPostBackEventHandler {
		private static readonly object serverClickEvent = new object ();

		public HtmlAnchor ()
			: base ("a")
		{
		}


		[DefaultValue ("")]
		[WebSysDescription("")]
		[WebCategory("Action")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string HRef {
			get {
				string s = Attributes ["href"];
				return (s == null) ? String.Empty : s;
			}
			set {
				if (value == null) {
					Attributes.Remove ("href");
				} else {
					Attributes ["href"] = value;
				}
			}
		}

		[DefaultValue ("")]
		[WebSysDescription("")]
		[WebCategory("Navigation")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string Name {
			get {
				string s = Attributes ["name"];
				return (s == null) ? String.Empty : s;
			}
			set {
				if (value == null)
					Attributes.Remove ("name");
				else
					Attributes ["name"] = value;
			}
		}

		[DefaultValue ("")]
		[WebSysDescription("")]
		[WebCategory("Navigation")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string Target {
			get {
				string s = Attributes ["target"];
				return (s == null) ? String.Empty : s;
			}
			set {
				if (value == null)
					Attributes.Remove ("target");
				else
					Attributes ["target"] = value;
			}
		}

		[DefaultValue ("")]
		[WebSysDescription("")]
		[WebCategory("Appearance")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
#if NET_2_0
		[Localizable (true)]
#endif
		public string Title {
			get {
				string s = Attributes ["title"];
				return (s == null) ? String.Empty : s;
			}
			set {
				if (value == null)
					Attributes.Remove ("title");
				else
					Attributes ["title"] = value;
			}
		}

#if NET_2_0
		protected internal
#else
		protected
#endif		
		override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
		}

		protected virtual void OnServerClick (EventArgs e)
		{
			EventHandler serverClick = (EventHandler) Events [serverClickEvent];
			if (serverClick != null)
				serverClick (this, e);
		}

		protected override void RenderAttributes (HtmlTextWriter writer)
		{
			// we don't want to render the "user" URL, so we either render:

			EventHandler serverClick = (EventHandler) Events [serverClickEvent];
			if (serverClick != null) {
				// a script
				ClientScriptManager csm = new ClientScriptManager (Page);
				Attributes ["href"] = csm.GetPostBackClientHyperlink (this, String.Empty);
			} else {
				string hr = HRef;
				if (hr != "")
					HRef = ResolveUrl (hr);
			}
			base.RenderAttributes (writer);

			// but we never set back the href attribute after the rendering
			// nor is the property available after rendering
			Attributes.Remove ("href");
		}
#if NET_2_0
		protected virtual void RaisePostBackEvent (string eventArgument)
		{
			OnServerClick (EventArgs.Empty);
		}
#endif

		void IPostBackEventHandler.RaisePostBackEvent (string eventArgument)
		{
#if NET_2_0
			RaisePostBackEvent (eventArgument);
#else
			OnServerClick (EventArgs.Empty);
#endif
		}


		[WebSysDescription("")]
		[WebCategory("Action")]
		public event EventHandler ServerClick {
			add { Events.AddHandler (serverClickEvent, value); }
			remove { Events.RemoveHandler (serverClickEvent, value); }
		}
	}
}
