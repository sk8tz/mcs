//
// System.Web.UI.WebControls.RadioButton.cs
//
// Author:
//      Dick Porter  <dick@ximian.com>
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

using System.Collections.Specialized;
using System.ComponentModel;

namespace System.Web.UI.WebControls {
	[Designer ("System.Web.UI.Design.WebControls.CheckBoxDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class RadioButton : CheckBox , IPostBackDataHandler
	{
		public RadioButton () : base ("radio")
		{
		}

		[DefaultValue ("")]
#if NET_2_0
		[Themeable (false)]
#endif
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public virtual string GroupName
		{
			get {
				return (ViewState.GetString ("GroupName",
							     String.Empty));
			}
			set {
				ViewState["GroupName"] = value;
			}
		}

		internal override string NameAttribute 
		{
			get {
				return (GroupName);
			}
		}

		internal override void InternalAddAttributesToRender (HtmlTextWriter w)
		{
			base.InternalAddAttributesToRender (w);
			w.AddAttribute (HtmlTextWriterAttribute.Value, this.UniqueID);
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

#if NET_2_0
		[MonoTODO]
		protected override bool LoadPostData (string postDataKey, NameValueCollection postCollection) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void RaisePostDataChangedEvent ()
		{
			throw new NotImplementedException ();
		}
#endif		

		bool IPostBackDataHandler.LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			bool old_checked = Checked;
			
			if (postCollection[GroupName] == postDataKey) {
				Checked = true;
			} else {
				Checked = false;
			}

			if (old_checked != Checked) {
				return (true);
			} else {
				return (false);
			}
		}
	}
}
