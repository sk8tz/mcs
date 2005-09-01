//
// System.Web.UI.HtmlControls.HtmlInputRadioButton.cs
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
using System.Collections.Specialized;

namespace System.Web.UI.HtmlControls {

	[DefaultEvent ("ServerChange")]
	public class HtmlInputRadioButton : HtmlInputControl, IPostBackDataHandler {

		private static readonly object serverChangeEvent = new object ();


		public HtmlInputRadioButton ()
			: base ("radio")
		{
		}


		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription("")]
		[WebCategory("Misc")]
		public bool Checked {
			get { return (Attributes ["checked"] == "checked"); }
			set {
				if (value)
					Attributes ["checked"] = "checked";
				else
					Attributes.Remove ("checked");
			}
		}

		public override string Name {
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

		public override string Value {
			get {
				string s = Attributes ["value"];
				if (s == null) {
					s = ID;
					if ((s != null) && (s.Length == 0))
						s = null;
				}
				return s;
			}
			set {
				if (value == null)
					Attributes.Remove ("value");
				else
					Attributes ["value"] = value;
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

		protected virtual void OnServerChange (EventArgs e)
		{
			EventHandler serverChange = (EventHandler) Events [serverChangeEvent];
			if (serverChange != null)
				serverChange (this, e);
		}

		protected override void RenderAttributes (HtmlTextWriter writer)
		{
			writer.WriteAttribute ("value", ID);
			base.RenderAttributes (writer);
		}
#if NET_2_0
		protected virtual bool LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			return DefaultLoadPostData (postDataKey, postCollection);
		}

		protected virtual void RaisePostDataChangedEvent ()
		{
			OnServerChange (EventArgs.Empty);
		}
#endif

		internal bool DefaultLoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			string s = postCollection [postDataKey];
			if ((s != null) && (Attributes ["value"] != postDataKey)) {
				Attributes ["value"] = postDataKey;
				// this doesn't seems to trigger a change
			}
			return false;
		}

		bool IPostBackDataHandler.LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
#if NET_2_0
			return LoadPostData (postDataKey, postCollection);
#else
			return DefaultLoadPostData (postDataKey, postCollection);
#endif
		}

		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
#if NET_2_0
			RaisePostDataChangedEvent ();
#else
			OnServerChange (EventArgs.Empty);
#endif
		}


		[WebSysDescription("")]
		[WebCategory("Action")]
		public event EventHandler ServerChange {
			add { Events.AddHandler (serverChangeEvent, value); }
			remove { Events.RemoveHandler (serverChangeEvent, value); }
		}
	}
}
