//
// System.Web.UI.WebControls.HierarchicalDataBoundControl.cs
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
	[DesignerAttribute ("System.Web.UI.Design.WebControls.HierarchicalDataBoundControlDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public abstract class HierarchicalDataBoundControl : BaseDataBoundControl
	{
		[IDReferencePropertyAttribute (typeof(HierarchicalDataSourceControl))]
		public override string DataSourceID {
			get {
				object o = ViewState ["DataSourceID"];
				if (o != null)
					return (string)o;
				
				return String.Empty;
			}
			set {
				if (Initialized)
					RequiresDataBinding = true;
				
				ViewState ["DataSourceID"] = value;
			}
		}
		
		protected virtual HierarchicalDataSourceView GetData (string viewPath)
		{
			if (DataSource != null && DataSourceID != "")
				throw new HttpException ();
			
			IHierarchicalDataSource ds = GetDataSource ();
			if (ds != null)
				return ds.GetHierarchicalView (viewPath);
			else
				return null; 
		}
		
		protected virtual IHierarchicalDataSource GetDataSource ()
		{
			if (DataSourceID != "")
				return BaseDataBoundControl.FindDataSource (this, DataSourceID);
			
			return DataSource as IHierarchicalDataSource;
		}

		bool IsDataBound {
			get {
				return ViewState.GetBool ("DataBound", false);
			}
			set {
				ViewState ["DataBound"] = value;
			}
		}

		protected void MarkAsDataBound ()
		{
			IsDataBound = true;
		}
		
		protected override void OnDataPropertyChanged ()
		{
			RequiresDataBinding = true;
		}
		
		protected virtual void OnDataSourceChanged (object sender, EventArgs e)
		{
			RequiresDataBinding = true;
		}

		protected internal override void OnLoad (EventArgs e)
		{
			if (!Initialized) {
				Initialize ();
				ConfirmInitState ();
			}
			
			base.OnLoad(e);
		}

		private void Initialize ()
		{
			if (!Page.IsPostBack || (IsViewStateEnabled && !IsDataBound))
				RequiresDataBinding = true;

			IHierarchicalDataSource ds = GetDataSource ();
			if (ds != null && DataSourceID != "")
				ds.DataSourceChanged += new EventHandler (OnDataSourceChanged);
		}

		protected override void OnPagePreLoad (object sender, EventArgs e)
		{
			base.OnPagePreLoad (sender, e);
			
			Initialize ();
		}
		
		protected internal virtual void PerformDataBinding ()
		{
		}
		
		protected override void PerformSelect ()
		{
			OnDataBinding (EventArgs.Empty);
			PerformDataBinding ();
			// The PerformDataBinding method has completed.
			RequiresDataBinding = false;
			MarkAsDataBound ();
			OnDataBound (EventArgs.Empty);
		}
		
		protected override void ValidateDataSource (object dataSource)
		{
			if (dataSource == null || dataSource is IHierarchicalDataSource || dataSource is IHierarchicalEnumerable)
				return;
			throw new InvalidOperationException ("Invalid data source");
		}
	}
}
#endif


