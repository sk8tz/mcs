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
// System.Web.UI.HtmlControls.HtmlInputImage.cs
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//
// (C) 2005 Novell, Inc.


//
// TODO: getting the .x and .y in LoadData doesn't work with mozilla
//

using System;
using System.Globalization;
using System.Collections.Specialized;
using System.ComponentModel;

namespace System.Web.UI.HtmlControls {
	[DefaultEvent("ServerClick")]
	public class HtmlInputImage : HtmlInputControl, IPostBackDataHandler,
		      IPostBackEventHandler {

		private static readonly object ServerClickEvent = new object ();

		private int clicked_x;
		private int clicked_y;

		public HtmlInputImage () : base ("image")
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

		[DefaultValue("")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string Align {
			get { return GetAtt ("align"); }
			set { SetAtt ("align", value); }
		}

		[DefaultValue("")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#if NET_2_0
		[Localizable (true)]
#endif		
		public string Alt {
			get { return GetAtt ("alt"); }
			set { SetAtt ("alt", value); }
		}

		[DefaultValue("")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string Src {
			get { return GetAtt ("src"); }
			set { SetAtt ("src", value); }
		}

#if NET_2_0
		[DefaultValue("-1")]
#else		
		[DefaultValue("")]
#endif		
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int Border {
			get {
				string border = Attributes ["border"];
				if (border == null)
					return -1;
				return Int32.Parse (border, CultureInfo.InvariantCulture);
			}
			set {
				if (value == -1) {
					Attributes.Remove ("border");
					return;
				}
				Attributes ["border"] = value.ToString (CultureInfo.InvariantCulture);
			}
		}

#if NET_2_0
		[MonoTODO]
		[DefaultValue ("")]
		public string ValidationGroup
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		protected virtual bool LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void RaisePostBackEvent ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void RaisePostDataChangedEvent ()
		{
			throw new NotImplementedException ();
		}
#endif		

		bool IPostBackDataHandler.LoadPostData (string postDataKey,
				NameValueCollection postCollection)
		{
			string x = postCollection [UniqueID + ".x"];
			string y = postCollection [UniqueID + ".y"];

			if (x != null && x.Length != 0 &&
					y != null && y.Length != 0) {
				clicked_x = Int32.Parse (x, CultureInfo.InvariantCulture);
				clicked_y = Int32.Parse (y, CultureInfo.InvariantCulture);
				Page.RegisterRequiresRaiseEvent (this);
				return true;
			}

			return false;
		}

		
		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
		}
				
		void IPostBackEventHandler.RaisePostBackEvent (string eventArgument)
		{
			if (CausesValidation)
				Page.Validate ();
			OnServerClick (new ImageClickEventArgs (clicked_x, clicked_y));
		}

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
			if (Page != null)
				Page.RegisterRequiresPostBack (this);
		}

		protected virtual void OnServerClick (ImageClickEventArgs e)
		{
			EventHandler handler = Events [ServerClickEvent] as EventHandler;
			if (handler != null)
				handler (this, e);
		}

		protected override void RenderAttributes (HtmlTextWriter writer)
		{
			if (CausesValidation && Page != null && Page.AreValidatorsUplevel ()) {
				ClientScriptManager csm = new ClientScriptManager (Page);
				writer.WriteAttribute ("onclick", csm.GetClientValidationEvent ());
			}

			base.RenderAttributes (writer);
		}

		private void SetAtt (string name, string value)
		{
			if (value == null)
				Attributes.Remove (name);
			else
				Attributes [name] = value;
		}

		private string GetAtt (string name)
		{
			string res = Attributes [name];
			if (res == null)
				return String.Empty;
			return res;
		}

		public event ImageClickEventHandler ServerClick {
			add { Events.AddHandler (ServerClickEvent, value); }
			remove { Events.AddHandler (ServerClickEvent, value); }
		}
	}
}

