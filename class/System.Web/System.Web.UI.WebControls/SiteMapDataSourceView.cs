//
// System.Web.UI.WebControls.SiteMapDataSourceView
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//	Sanjay Gupta (gsanjay@novell.com)
//
// (C) 2003 Ben Maurer
// (C) 2004 Novell, Inc. (http://www.novell.com)
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
using System.Collections.Specialized;
using System.Text;
using System.Web.UI;

namespace System.Web.UI.WebControls {
	public class SiteMapDataSourceView : DataSourceView {
		public SiteMapDataSourceView (SiteMapNode node) : this (new SiteMapNodeCollection (node)) {}
		public SiteMapDataSourceView (SiteMapNodeCollection collection)
		{
			this.collection = collection;
		}
		
		[MonoTODO ("Need to bring in sync with latest DataSourceView class")]
		public override IEnumerable Select ()
		{
			throw new NotImplementedException ();
		}

		SiteMapNodeCollection collection;
	}
}
#endif

