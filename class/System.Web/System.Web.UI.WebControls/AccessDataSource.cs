﻿//
// System.Web.UI.WebControls.AccessDataSource.cs
//
// Authors:
//   Sanjay Gupta (gsanjay@novell.com)
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.IO;
using System.ComponentModel;
using System.Data.Common;
using System.Drawing;
using System.Security.Permissions;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[DesignerAttribute ("System.Web.UI.Design.WebControls.AccessDataSourceDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ToolboxBitmap ("")]
	public class AccessDataSource : SqlDataSource {

		string dataFile;
		
		public AccessDataSource () : base ()
		{
			this.ProviderName = "System.Data.OleDb";
		}

		public AccessDataSource (string dataFile, string selectCommand) : 
			base (String.Empty, selectCommand)
		{
			this.dataFile = dataFile;
			//After setting dataFile, connectionString gets recreated
			//On accessing ConnectionString, MS.Net throws NullReferenceException
			//Need to dig more on this.
			this.ProviderName = "System.Data.OleDb";							
		}

		protected override SqlDataSourceView CreateDataSourceView (string viewName)
		{
			AccessDataSourceView view = new AccessDataSourceView (this, viewName, this.Context);
			view.DataSourceViewChanged += new EventHandler (ViewChanged);
			if (IsTrackingViewState)
				((IStateManager) view).TrackViewState ();				
			return view;
		}

		void ViewChanged (object source, EventArgs e)
		{
			OnDataSourceChanged (e);
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[MonoTODO]
		public override string SqlCacheDependency {
			get { throw new NotSupportedException ("AccessDataSource does not supports SQL Cache Dependencies."); }
			set { throw new NotSupportedException ("AccessDataSource does not supports SQL Cache Dependencies."); }
		}

		[MonoTODO]
		protected override DbProviderFactory GetDbProviderFactory ()
		{
			throw new NotImplementedException ();
		}

		[BrowsableAttribute (false), 
		DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public override string ConnectionString {
			get { return this.ConnectionString; }
			set { throw new InvalidOperationException 
				("The ConnectionString is automatically generated for AccessDataSource and hence cannot be set."); 
			}
		}

		[UrlPropertyAttribute]
		[DefaultValueAttribute ("")]
		[WebCategoryAttribute ("Data")]
		[WebSysDescriptionAttribute ("MS Office Access database file name")]
		[EditorAttribute ("System.Web.UI.Design.MdbDataFileEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public string DataFile {
			get { return dataFile; }
			set { dataFile = value; }
			//After setting dataFile, connectionString gets recreated
			//On accessing ConnectionString, MS.Net throws NullReferenceException
			//Need to dig more on this.
		}

		[BrowsableAttribute (false), 
		DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]			
		public override string ProviderName {
			get { return this.ProviderName; }
			set { throw new InvalidOperationException
				("Setting ProviderName on an AccessDataSource is not allowed");
			}
		}		
	}
}
#endif
