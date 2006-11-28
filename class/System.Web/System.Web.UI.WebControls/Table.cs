//
// System.Web.UI.WebControls.Table.cs
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
using System.Security.Permissions;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[DefaultProperty ("Rows")]
	[Designer ("System.Web.UI.Design.WebControls.TableDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ParseChildren (true, "Rows")]
#if NET_2_0
	[SupportsEventValidation]
	public class Table : WebControl, IPostBackEventHandler {
#else
	public class Table : WebControl {
#endif
		private TableRowCollection rows;


		public Table ()
			: base (HtmlTextWriterTag.Table)
		{
		}


#if NET_2_0
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[UrlProperty]
#else
		[Bindable (true)]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
#endif
		[DefaultValue ("")]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual string BackImageUrl {
			get {
				if (!ControlStyleCreated)
					return String.Empty; // default value
				return TableStyle.BackImageUrl;
			}
			set { TableStyle.BackImageUrl = value; }
		}

		// note: it seems that Caption and CaptionAlign appeared in 1.1 SP1

		[DefaultValue ("")]
#if NET_2_0
		[Localizable (true)]
#endif
		[WebSysDescription ("")]
		[WebCategory ("Accessibility")]
		public virtual string Caption {
			get {
				object o = ViewState ["Caption"];
				return (o == null) ? String.Empty : (string) o;
			}
			set {
				if (value == null)
					ViewState.Remove ("Caption");
				else
					ViewState ["Caption"] = value;
			}
		}

		[DefaultValue (TableCaptionAlign.NotSet)]
		[WebCategory ("Accessibility")]
		public virtual TableCaptionAlign CaptionAlign {
			get {
				object o = ViewState ["CaptionAlign"];
				return (o == null) ? TableCaptionAlign.NotSet : (TableCaptionAlign) o;
			}
			set {
				if ((value < TableCaptionAlign.NotSet) || (value > TableCaptionAlign.Right)) {
					throw new ArgumentOutOfRangeException (Locale.GetText ("Invalid TableCaptionAlign value."));
				}
				ViewState ["CaptionAlign"] = value;
			}
		}

#if ONLY_1_1
		[Bindable (true)]
#endif
		[DefaultValue (-1)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual int CellPadding {
			get {
				if (!ControlStyleCreated)
					return -1; // default value
				return TableStyle.CellPadding;
			}
			set { TableStyle.CellPadding = value; }
		}

#if ONLY_1_1
		[Bindable (true)]
#endif
		[DefaultValue (-1)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual int CellSpacing {
			get {
				if (!ControlStyleCreated)
					return -1; // default value
				return TableStyle.CellSpacing;
			}
			set { TableStyle.CellSpacing = value; }
		}

#if ONLY_1_1
		[Bindable (true)]
#endif
		[DefaultValue (GridLines.None)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual GridLines GridLines {
			get {
				if (!ControlStyleCreated)
					return GridLines.None; // default value
				return TableStyle.GridLines;
			}
			set { TableStyle.GridLines = value; }
		}

#if ONLY_1_1
		[Bindable (true)]
#endif
		[DefaultValue (HorizontalAlign.NotSet)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual HorizontalAlign HorizontalAlign {
			get {
				if (!ControlStyleCreated)
					return HorizontalAlign.NotSet; // default value
				return TableStyle.HorizontalAlign;
			}
			set { TableStyle.HorizontalAlign = value; }
		}

		[MergableProperty (false)]
		[PersistenceMode (PersistenceMode.InnerDefaultProperty)]
		[WebSysDescription ("")]
		public virtual TableRowCollection Rows {
			get {
				if (rows == null)
					rows = new TableRowCollection (this);
				return rows;
			}
		}

		private TableStyle TableStyle {
			get { return (ControlStyle as TableStyle); }
		}


		protected override void AddAttributesToRender (HtmlTextWriter writer)
		{
			base.AddAttributesToRender (writer);
			if (!ControlStyleCreated || TableStyle.IsEmpty) {
				// for some reason border=X seems to be always present
				// and isn't rendered as a style attribute
				writer.AddAttribute (HtmlTextWriterAttribute.Border, "0");
			}
		}

		protected override ControlCollection CreateControlCollection ()
		{
			return new RowControlCollection (this);
		}

		protected override Style CreateControlStyle ()
		{
			return new TableStyle (ViewState);
		}

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void RenderContents (HtmlTextWriter writer)
		{
			if (HasControls ()) {
				writer.Indent++;
				foreach (Control c in Controls) {
					c.RenderControl (writer);
				}

				writer.Indent--;
			}
		}


		// new in Fx 1.1 SP1 (to support Caption and CaptionAlign)

		public override void RenderBeginTag (HtmlTextWriter writer)
		{
			base.RenderBeginTag (writer);

			string s = Caption;
			if (s.Length > 0) {
				writer.WriteBeginTag ("caption");

				TableCaptionAlign tca = CaptionAlign;
				if (tca != TableCaptionAlign.NotSet) {
					writer.WriteAttribute ("align", tca.ToString ());
				}

				writer.Write (HtmlTextWriter.TagRightChar);
				writer.Indent++;
				writer.WriteLine ();
				writer.Write (s);
				writer.Indent--;
				writer.WriteLine ();
				writer.WriteEndTag ("caption");
			} else if (HasControls ()) {
				writer.Indent++;
			}
		}

#if NET_2_0
		void IPostBackEventHandler.RaisePostBackEvent (string argument)
		{
			RaisePostBackEvent (argument);
		}

		protected virtual void RaisePostBackEvent (string argument)
		{
		}
#endif

		// inner class
		protected class RowControlCollection : ControlCollection {

			internal RowControlCollection (Table owner)
				: base (owner)
			{
			}


			public override void Add (Control child)
			{
				if (child == null)
					throw new NullReferenceException ("null");
				if (!(child is TableRow))
					throw new ArgumentException ("child", Locale.GetText ("Must be an TableRow instance."));

				base.Add (child);
			}

			public override void AddAt (int index, Control child)
			{
				if (child == null)
					throw new NullReferenceException ("null");
				if (!(child is TableRow))
					throw new ArgumentException ("child", Locale.GetText ("Must be an TableRow instance."));

				base.AddAt (index, child);
			}
		}
	}
}
