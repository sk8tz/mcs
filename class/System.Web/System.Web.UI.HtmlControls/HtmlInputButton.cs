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
//
// System.Web.UI.HtmlControls.HtmlInputButton.cs
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//
// (C) 2005 Novell, Inc.


using System;
using System.ComponentModel;

namespace System.Web.UI.HtmlControls {

	[DefaultEventAttribute ("ServerClick")]
	public class HtmlInputButton : HtmlInputControl, IPostBackEventHandler {

		private static readonly object ServerClickEvent = new object();

		public HtmlInputButton () : this ("button")
		{
		}

		public HtmlInputButton (string type) :	base (type)
		{
		}

		[DefaultValue(true)]
#if NET_2_0
		public virtual
#else		
		public
#endif		
		bool CausesValidation {
			get {
				return ViewState.GetBool ("CausesValidation", true);
			}
			set {
				ViewState ["CausesValidation"] = value;
			}
		}

#if NET_2_0
		[DefaultValue ("")]
		[MonoTODO]
		public string ValidationGroup
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
#endif

#if NET_2_0
		[MonoTODO]
		protected virtual void RaisePostBackEvent (string eventArgument)
		{
			throw new NotImplementedException ();
		}
#endif
		
		void IPostBackEventHandler.RaisePostBackEvent (string eventArgument)
		{
			if (CausesValidation)
				Page.Validate ();
			OnServerClick (EventArgs.Empty);
		}

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
			if (Events [ServerClickEvent] != null)
				Page.RequiresPostBackScript ();
		}

		protected virtual void OnServerClick (EventArgs e)
		{
			EventHandler server_click = (EventHandler) Events [ServerClickEvent];
			if (server_click != null)
				server_click (this, e);
		}

		protected override void RenderAttributes (HtmlTextWriter writer)
		{
			if (CausesValidation && Page != null && Page.AreValidatorsUplevel ()) {
				ClientScriptManager csm = new ClientScriptManager (Page);
				writer.WriteAttribute ("onclick", csm.GetClientValidationEvent ());
				writer.WriteAttribute ("language", "javascript");
			}

			base.RenderAttributes (writer);
		}

		public event EventHandler ServerClick {
			add { Events.AddHandler (ServerClickEvent, value); }
			remove { Events.RemoveHandler (ServerClickEvent, value); }
		}
	}
	
}

