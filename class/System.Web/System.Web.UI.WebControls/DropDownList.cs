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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Security.Permissions;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[ValidationProperty("SelectedItem")]
#if NET_2_0
	[SupportsEventValidation]
#endif
	public class DropDownList : ListControl, IPostBackDataHandler {
		#region Public Constructors
		public DropDownList() {
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		[Browsable(false)]
		public override Color BorderColor {
			get {
				return base.BorderColor;
			}

			set {
				base.BorderColor = value;
			}
		}

		[Browsable(false)]
		public override BorderStyle BorderStyle {
			get {
				return base.BorderStyle;
			}

			set {
				base.BorderStyle = value;
			}
		}

		[Browsable(false)]
		public override Unit BorderWidth {
			get {
				return base.BorderWidth;
			}

			set {
				base.BorderWidth = value;
			}
		}

		[DefaultValue(0)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public override int SelectedIndex {
			get {
				int selected;

				selected = base.SelectedIndex;
				if ((selected != -1) || (Items.Count == 0)) {
					return selected;
				}

				Items[0].Selected = true;
				return 0;
			}

			set {
				base.SelectedIndex = value;
			}
		}

#if ONLY_1_1
		[Bindable(false)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override string ToolTip {
			get {
				return string.Empty;
			}

			set {
			}
		}
#endif		
		#endregion	// Public Instance Properties

		#region Protected Instance Methods
		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			if (Page != null)
				Page.VerifyRenderingInServerForm (this);
#if NET_2_0
			if (writer == null)
				return;
			if (ID != null)
				writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID, true);
#else
			writer.AddAttribute(HtmlTextWriterAttribute.Name, this.UniqueID, true);
#endif
			if (AutoPostBack) {
				writer.AddAttribute (HtmlTextWriterAttribute.Onchange, Page.ClientScript.GetPostBackClientHyperlink (this, ""));
			}

			base.AddAttributesToRender(writer);
		}

		protected override ControlCollection CreateControlCollection() {
			return base.CreateControlCollection();
		}

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void RenderContents(HtmlTextWriter writer) {
			int		count;
			ListItem	item;
			bool		selected;

			if (writer == null) {
				return;
			}

			count = Items.Count;
			selected = false;

			for (int i = 0; i < count; i++) {
				item = Items[i];

				writer.WriteBeginTag("option");
				if (item.Selected) {
					if (selected) {
						throw new HttpException("DropDownList only may have a single selected item");
					}
					writer.WriteAttribute("selected", "selected", false);
					selected = true;
				}
				writer.WriteAttribute("value", item.Value, true);

				writer.Write(">");
				string text = HttpUtility.HtmlEncode (item.Text);
				writer.Write (text);
				writer.WriteEndTag("option");
				writer.WriteLine();
			}
			
			base.RenderContents(writer);
		}

#if NET_2_0
		[MonoTODO]
		protected internal override void VerifyMultiSelect ()
		{
			throw new NotImplementedException ();
		}
#endif		
		
		#endregion	// Protected Instance Methods

		#region	Interface Methods
#if NET_2_0
		protected virtual
#endif
		bool LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			int	index;

			index = Items.IndexOf(postCollection[postDataKey]);
			if (index != this.SelectedIndex) {
				SelectedIndex = index;
				return true;
			}
			
			return false;
		}

#if NET_2_0
		protected virtual
#endif
		void RaisePostDataChangedEvent ()
		{
			OnSelectedIndexChanged(EventArgs.Empty);
		}
		
		bool IPostBackDataHandler.LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			return LoadPostData (postDataKey, postCollection);
		}

		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			RaisePostDataChangedEvent ();
		}
		#endregion	// Interface Methods
	}
}
