//
// System.Web.UI.WebControls.BaseDataBoundControl.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
//

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

#if NET_2_0
using System.Collections;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	[DefaultProperty ("DataSourceID")]
	[DesignerAttribute ("System.Web.UI.Design.WebControls.BaseDataBoundControlDesigner, System.Design, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.ComponentModel.Design.IDesigner")]
	public abstract class BaseDataBoundControl: WebControl
	{
		public event EventHandler DataBound;
		
		object dataSource;
		string dataSourceId;
		bool initialized;
		bool requiresDataBinding;
		
		[BindableAttribute (true)]
		[ThemeableAttribute (false)]
		[DefaultValueAttribute (null)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public virtual object DataSource {
			get {
				return dataSource;
			}
			set {
				ValidateDataSource (value);
				dataSource = value;
				if (initialized)
					OnDataPropertyChanged ();
			}
		}
		
		[DefaultValueAttribute ("")]
		[ThemeableAttribute (false)]
		public virtual string DataSourceID {
			get {
				return dataSourceId != null ? dataSourceId : string.Empty;
			}
			set {
				dataSourceId = value;
				if (initialized)
					OnDataPropertyChanged ();
			}
		}
		
		protected bool Initialized {
			get { return initialized; }
		}
		
		protected bool IsBoundUsingDataSourceID {
			get { return DataSourceID.Length > 0; }
		}
		
		protected bool RequiresDataBinding {
			get { return requiresDataBinding; }
			set { requiresDataBinding = value; }
		}
		
		protected void ConfirmInitState ()
		{
			initialized = true;
		}
		
		public override void DataBind ()
		{
			RequiresDataBinding = false;
			PerformSelect ();
			base.DataBind ();
			OnDataBound (EventArgs.Empty);
		}
		
		protected virtual void EnsureDataBound ()
		{
			if (RequiresDataBinding && IsBoundUsingDataSourceID)
				DataBind ();
		}
		
		protected virtual void OnDataBound (EventArgs e)
		{
			if (DataBound != null)
				DataBound (this, e);
		}

		protected virtual void OnDataPropertyChanged ()
		{
			RequiresDataBinding = true;
		}
		
		protected override void OnInit (EventArgs e)
		{
			base.OnInit (e);
			Page.PreLoad += new EventHandler (OnPagePreLoad);
		}
		
		protected virtual void OnPagePreLoad (object sender, EventArgs e)
		{
			ConfirmInitState ();
		}
		
		protected override void OnPreRender (EventArgs e)
		{
			EnsureDataBound ();
			base.OnPreRender (e);
		}
		
		protected abstract void PerformSelect ();
		
		protected abstract void ValidateDataSource (object dataSource);
		
	}
}

#endif

