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
using System.Security.Permissions;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[Designer ("System.Web.UI.Design.WebControls.CheckBoxDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
#if NET_2_0
	[SupportsEventValidation]
#endif
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
				string unique = UniqueID;
				string gn = GroupName;
				int colon = -1;
				if (unique != null)
					colon = unique.IndexOf (':');
				if (colon == -1)
					return gn;

				return unique.Substring (0, colon + 1) + gn;
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
		protected override
#endif
		bool LoadPostData (string postDataKey, NameValueCollection postCollection) 
		{
			bool old_checked = Checked;
			
			if (postCollection[NameAttribute] == postDataKey) {
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

#if NET_2_0
		protected override
#endif
		void RaisePostDataChangedEvent ()
		{
			OnCheckedChanged (EventArgs.Empty);
		}

		bool IPostBackDataHandler.LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			return LoadPostData (postDataKey, postCollection);
		}
	}
}
