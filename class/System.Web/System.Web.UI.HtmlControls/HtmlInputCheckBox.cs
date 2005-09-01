//
// System.Web.UI.HtmlControls.HtmlInputCheckBox.cs
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
using System.Collections.Specialized;

namespace System.Web.UI.HtmlControls 
{
	[DefaultEvent ("ServerChange")]
	public class HtmlInputCheckBox : HtmlInputControl, IPostBackDataHandler
	{
		public HtmlInputCheckBox () : base ("checkbox")
		{
		}

		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription("")]
		[WebCategory("Misc")]
#if NET_2_0
		//[TypeConverter (typeof(System.Web.UI.MinimizableAttributeTypeConverter))]
#endif
		public bool Checked
		{
			get {
				string check = Attributes["checked"];

				if (check == null) {
					return (false);
				}

				return (true);
			}
			set {
				if (value == false) {
					Attributes.Remove ("checked");
				} else {
					Attributes["checked"] = "checked";
				}
			}
		}
		
		private static readonly object EventServerChange = new object ();

		[WebSysDescription("")]
		[WebCategory("Action")]
		public event EventHandler ServerChange
		{
			add {
				Events.AddHandler (EventServerChange, value);
			}
			remove {
				Events.RemoveHandler (EventServerChange, value);
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

			if (Page != null) {
				Page.RegisterRequiresPostBack (this);
			}
		}

		protected virtual void OnServerChange (EventArgs e)
		{
			EventHandler handler = (EventHandler)Events[EventServerChange];

			if (handler != null) {
				handler (this, e);
			}
		}

#if NET_2_0
		[MonoTODO]
		protected virtual bool LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void RaisePostDataChangedEvent ()
		{
			throw new NotImplementedException ();
		}
#endif
		
		bool IPostBackDataHandler.LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			string postedValue = postCollection[postDataKey];
			bool postedBool = ((postedValue != null) &&
					   (postedValue.Length > 0));

			if (Checked != postedBool) {
				Checked = postedBool;
				return (true);
			}
			
			return (false);
		}

		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			OnServerChange (EventArgs.Empty);
		}
	}
}

	
