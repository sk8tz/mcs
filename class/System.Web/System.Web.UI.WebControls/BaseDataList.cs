//
// System.Web.UI.WebControls.BaseDataList.cs
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

using System.Collections;
using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[DefaultEvent ("SelectedIndexChanged")]
	[DefaultProperty ("DataSource")]
	[Designer ("System.Web.UI.Design.WebControls.BaseDataListDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public abstract class BaseDataList : WebControl {

		private static readonly object selectedIndexChangedEvent = new object ();

		private DataKeyCollection keycoll;
		private object source;
#if NET_2_0
		//private string dataSourceId;
		IDataSource boundDataSource = null;
		private bool initialized;
		private bool requiresDataBinding;
		private DataSourceSelectArguments selectArguments;
		private IEnumerable data;
#endif

		protected BaseDataList ()
		{
		}


		[DefaultValue ("")]
#if NET_2_0
		[Localizable (true)]
#endif
		[WebSysDescription ("")]
		[WebCategory ("Accessibility")]
		public virtual string Caption {
			get { return ViewState.GetString ("Caption", ""); }
			set {
				if (value == null)
					ViewState.Remove ("Caption");
				else
					ViewState ["Caption"] = value;
			}
		}

		[DefaultValue (TableCaptionAlign.NotSet)]
		public virtual TableCaptionAlign CaptionAlign {
			get { return (TableCaptionAlign) ViewState.GetInt ("CaptionAlign", (int)TableCaptionAlign.NotSet); }
			set {
				if ((value < TableCaptionAlign.NotSet) || (value > TableCaptionAlign.Right))
					throw new ArgumentOutOfRangeException (Locale.GetText ("Invalid TableCaptionAlign value."));

				ViewState ["CaptionAlign"] = value;
			}
		}

#if ONLY_1_1
		[Bindable (true)]
#endif
		[DefaultValue (-1)]
		[WebSysDescription("")]
		[WebCategory("Layout")]
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
		[DefaultValue (0)]
		[WebSysDescription("")]
		[WebCategory("Layout")]
		public virtual int CellSpacing {
			get {
				if (!ControlStyleCreated)
					return 0; // default value
				return TableStyle.CellSpacing;
			}
			set { TableStyle.CellSpacing = value; }
		}

		public override ControlCollection Controls {
			get {
				EnsureChildControls ();
				return base.Controls;
			}
		}

		[DefaultValue ("")]
#if NET_2_0
		[Themeable (false)]
#endif
		[MonoTODO ("incomplete")]
		[WebSysDescription("")]
		[WebCategory("Data")]
		public virtual string DataKeyField {
			get { return ViewState.GetString ("DataKeyField", ""); }
			set {
				if (value == null)
					ViewState.Remove ("DataKeyField");
				else
					ViewState ["DataKeyField"] = value;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription("")]
		[WebCategory("Data")]
		public DataKeyCollection DataKeys {
			get {
				if (keycoll == null)
					keycoll = new DataKeyCollection (DataKeysArray);
				return keycoll;
			}
		}

		protected ArrayList DataKeysArray {
			get {
				ArrayList keys = (ArrayList) ViewState ["DataKeys"];
				if (keys == null) {
					keys = new ArrayList ();
					ViewState ["DataKeys"] = keys;
				}
				return keys;
			}
		}

		[DefaultValue ("")]
#if NET_2_0
		[Themeable (false)]
#endif
		[WebSysDescription("")]
		[WebCategory("Data")]
		public string DataMember {
			get { return ViewState.GetString ("DataMember", ""); }
			set {
				if (value == null)
					ViewState.Remove ("DataMember");
				else
					ViewState ["DataMember"] = value;
#if NET_2_0
				if (!Initialized)
					OnDataPropertyChanged ();
#endif
			}
		}

		[Bindable (true)]
		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
#if NET_2_0
		[Themeable (false)]
#endif
		[WebSysDescription("")]
		[WebCategory("Data")]
		public virtual object DataSource {
			get { return source; }
			set {
				if ((value == null) || (value is IEnumerable) || (value is IListSource)) {
#if NET_2_0
// FIXME - can't duplicate in a test case ? LAMESPEC ?
// can't duplicate in a test case
//					if ((dataSourceId != null) && (dataSourceId.Length != 0))
//						throw new HttpException (Locale.GetText ("DataSourceID is already set."));

					source = value;

					if (!Initialized)
						OnDataPropertyChanged ();
#else
					source = value;
#endif
				} else {
					string msg = Locale.GetText ("Invalid data source. This requires an object implementing {0} or {1}.",
						"IEnumerable", "IListSource");
					throw new ArgumentException (msg);
				}
			}
		}

#if ONLY_1_1
		[Bindable (true)]
#endif
		[DefaultValue (GridLines.Both)]
		[WebSysDescription("")]
		[WebCategory("Appearance")]
		public virtual GridLines GridLines {
			get {
				if (!ControlStyleCreated)
					return GridLines.Both; // default value
				return TableStyle.GridLines;
			}
			set { TableStyle.GridLines = value; }
		}

#if ONLY_1_1
		[Bindable (true)]
#endif
		[Category ("Layout")]
		[DefaultValue (HorizontalAlign.NotSet)]
		[WebSysDescription("")]
		public virtual HorizontalAlign HorizontalAlign {
			get {
				if (!ControlStyleCreated)
					return HorizontalAlign.NotSet; // default value
				return TableStyle.HorizontalAlign;
			}
			set { TableStyle.HorizontalAlign = value; }
		}

		[DefaultValue (false)]
		public virtual bool UseAccessibleHeader {
			get { return ViewState.GetBool ("UseAccessibleHeader", false); }
			set { ViewState ["UseAccessibleHeader"] = value; }
		}
#if NET_2_0
		[DefaultValue ("")]
		[IDReferenceProperty (typeof (DataSourceControl))]
		[Themeable (false)]
		public virtual string DataSourceID {
			get { return ViewState.GetString ("DataSourceID", ""); }
			set {
				// LAMESPEC ? this is documented as an HttpException in beta2
				if (source != null)
					throw new InvalidOperationException (Locale.GetText ("DataSource is already set."));

				ViewState ["DataSourceID"] = value;

				if (!Initialized)
					OnDataPropertyChanged ();
			}
		}

		protected bool Initialized {
			get { return initialized; }
		}

		// as documented in BaseDataBoundControl
		protected bool IsBoundUsingDataSourceID {
			get { return (DataSourceID.Length != 0); }
		}

		// doc says ?automatically? called by ASP.NET
		protected bool RequiresDataBinding {
			get { return requiresDataBinding; }
			set { requiresDataBinding = value; }
		}

		protected DataSourceSelectArguments SelectArguments {
			get {
				if (selectArguments == null)
					selectArguments = CreateDataSourceSelectArguments ();
				return selectArguments;
			}
		}
#endif
		private TableStyle TableStyle {
			// this will throw an InvalidCasException just like we need
			get { return (TableStyle) ControlStyle; }
		}


		protected override void AddParsedSubObject (object obj)
		{
			// don't accept controls
		}

		// see Kothari, page 435
#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void CreateChildControls ()
		{
			// We are recreating the children from viewstate
			if (HasControls ())
				Controls.Clear();

			// If presents, build the children from the viewstate
			if (ViewState ["Items"] != null)
				CreateControlHierarchy (false);
		}

		protected abstract void CreateControlHierarchy (bool useDataSource);

		// see Kothari, page 434
		// see also: Control.DataBind on Fx 2.0 beta2 documentation
		public override void DataBind ()
		{
			// unlike most samples we don't call base.OnDataBinding
			// because we override it in this class
			OnDataBinding (EventArgs.Empty);

			// Clear, if required, then recreate the control hierarchy
			if (HasControls ())
				Controls.Clear ();
			if (HasChildViewState)
				ClearChildViewState ();
			if (!IsTrackingViewState)
				TrackViewState ();
			CreateControlHierarchy (true);

			// Indicate that child controls have been created, preventing
			// CreateChildControls from getting called.
			ChildControlsCreated = true;    
		}
#if NET_2_0
		protected virtual DataSourceSelectArguments CreateDataSourceSelectArguments ()
		{
			return DataSourceSelectArguments.Empty;
		}

		// best documentation is (again) in BaseDataBoundControl
		protected void EnsureDataBound ()
		{
			if (IsBoundUsingDataSourceID && RequiresDataBinding)
				DataBind ();
		}

		private void SelectCallback (IEnumerable data)
		{
			this.data = data;
		}

		protected virtual IEnumerable GetData ()
		{
			if (DataSourceID.Length == 0)
				return null;

			if (boundDataSource == null)
				return null;

			DataSourceView dsv = boundDataSource.GetView (String.Empty);
			dsv.Select (SelectArguments, new DataSourceViewSelectCallback (SelectCallback));
			return data;
		}
#endif

		protected override void OnDataBinding (EventArgs e)
		{
			base.OnDataBinding (e);
		}
#if NET_2_0
		protected virtual void OnDataPropertyChanged ()
		{
		}

		protected virtual void OnDataSourceViewChanged (object sender, EventArgs e)
		{
			RequiresDataBinding = true;
		}

		protected internal override void OnInit (EventArgs e)
		{
			base.OnInit (e);
		}

		protected internal override void OnLoad (EventArgs e)
		{
			if ((Page != null) && !Page.IsPostBack)
				RequiresDataBinding = true;

			if (IsBoundUsingDataSourceID)
				ConnectToDataSource ();

			initialized = true;
			base.OnLoad (e);
		}

		protected internal override void OnPreRender (EventArgs e)
		{
			EnsureDataBound ();
			base.OnPreRender (e);
		}
#endif
		protected virtual void OnSelectedIndexChanged (EventArgs e)
		{
			EventHandler selectedIndexChanged = (EventHandler) Events [selectedIndexChangedEvent];
			if (selectedIndexChanged != null)
				selectedIndexChanged (this, e);
		}

		protected abstract void PrepareControlHierarchy ();

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void Render (HtmlTextWriter writer)
		{
			PrepareControlHierarchy ();
			// don't call base class or RenderBegin|EndTag
			// or we'll get an extra <span></span>
			RenderContents (writer);
		}


		[WebSysDescription("")]
		[WebCategory("Action")]
		public event EventHandler SelectedIndexChanged {
			add { Events.AddHandler (selectedIndexChangedEvent, value); }
			remove { Events.RemoveHandler (selectedIndexChangedEvent, value); }
		}


		static public bool IsBindableType (Type type)
		{
			// I can't believe how many NRE are possible in System.Web
			if (type == null) // Type.GetTypeCode no longer throws when a null is passed.
				throw new NullReferenceException ();

			switch (Type.GetTypeCode (type)) {
			case TypeCode.Boolean:
			case TypeCode.Byte:
			case TypeCode.SByte:
			case TypeCode.Int16:
			case TypeCode.UInt16:
			case TypeCode.Int32:
			case TypeCode.UInt32:
			case TypeCode.Int64:
			case TypeCode.UInt64:
			case TypeCode.Char:
			case TypeCode.Double:
			case TypeCode.Single:
			case TypeCode.DateTime:
			case TypeCode.Decimal:
			case TypeCode.String:
				return true;
			default:
				return false;
			}
		}

		void ConnectToDataSource ()
		{
			if (NamingContainer != null)
				boundDataSource = (NamingContainer.FindControl (DataSourceID) as IDataSource);

			if (boundDataSource == null) {
				if (Parent != null)
					boundDataSource = (Parent.FindControl (DataSourceID) as IDataSource);

				if (boundDataSource == null)
					throw new HttpException (Locale.GetText ("Coulnd't find a DataSource named '{0}'.", DataSourceID));
			}
			DataSourceView dsv = boundDataSource.GetView (String.Empty);
			dsv.DataSourceViewChanged += new EventHandler (OnDataSourceViewChanged);
		}
	}
}
