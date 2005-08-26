//
// System.Web.UI.WebControls.CompositeControl
//
// Authors: Ben Maurer <bmaurer@novell.com>
//          Chris Toshok <toshok@novell.com>
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

namespace System.Web.UI.WebControls {

#if NET_2_0
	[Designer ("System.Web.UI.Design.WebControls.CompositeControlDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public abstract class CompositeControl : WebControl, INamingContainer, ICompositeControlDesignerAccessor {

		protected CompositeControl ()
		{
		}

		public override void DataBind ()
		{
			/* make sure all the child controls have been created */
			EnsureChildControls ();
			/* and then... */
			base.DataBind();
		}

		protected internal override void Render (HtmlTextWriter w)
		{
			/* make sure all the child controls have been created */
			EnsureChildControls ();
			/* and then... */
			base.Render (w);
		}

		[MonoTODO("not sure exactly what this one does..")]
		void ICompositeControlDesignerAccessor.RecreateChildControls ()
		{
			/* for now just call CreateChildControls to force
			 * the recreation of our children. */
			CreateChildControls ();
		}
	
		public override ControlCollection Controls {
			get {
				/* make sure all the child controls have been created */
				EnsureChildControls ();
				/* and then... */
				return base.Controls;
			}
		}
	}
#endif

}
