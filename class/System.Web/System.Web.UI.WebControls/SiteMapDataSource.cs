//
// System.Web.UI.WebControls.SiteMapDataSource.cs
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

using System;
using System.Collections;
using System.Web.UI;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	[PersistChildrenAttribute (false)]
	[DesignerAttribute ("System.Web.UI.Design.WebControls.SiteMapDataSourceDesigner, System.Design, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.ComponentModel.Design.IDesigner")]
	[ParseChildrenAttribute (true)]
	public class SiteMapDataSource : HierarchicalDataSourceControl, IDataSource, IListSource
	{
		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[BrowsableAttribute (false)]
		public DataSourceView GetView (string viewName)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public ICollection GetViewNames ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public IList GetList ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public bool ContainsListCollection {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public event EventHandler DataSourceChanged {
			add { throw new NotImplementedException (); }
			remove { throw new NotImplementedException (); }
		}
	}
}

#endif

